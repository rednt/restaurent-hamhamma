using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using restaurent_hamhamma.Models;
using restaurent_hamhamma.Services;
using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;
using System.Windows.Data;
using dotenv.net;
using System.Diagnostics;

namespace restaurent_hamhamma.Pages
{
    public partial class ReservationPage : Page
    {
        private readonly string _connectionString;
        private MenuItemModel _selectedMenuItem;

        public ReservationPage()
        {
            InitializeComponent();
            LoadData();
            SetupEventHandlers();
            try
            {
                // Load environment variables from .env file
                DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

                // Retrieve environment variables
                string user = Environment.GetEnvironmentVariable("DB_USER");
                string password = Environment.GetEnvironmentVariable("DB_PASSWORD");
                string host = Environment.GetEnvironmentVariable("DB_HOST");
                string port = Environment.GetEnvironmentVariable("DB_PORT");
                string service = Environment.GetEnvironmentVariable("DB_SERVICE");

                // Validate the environment variables
                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password) ||
                    string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(service))
                {
                    throw new InvalidOperationException("One or more required environment variables are missing.");
                }

                // Build the connection string using the environment variables
                _connectionString = $"User Id={user};Password={password};Data Source={host}:{port}/{service};Connection Timeout=30";

                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing the Admin page: {ex.Message}",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Exception in AdminPage constructor: {ex}");
            }
        }

        

        private void LoadData()
        {
            // Heures disponibles (format 24h)
            List<string> heures = new List<string>
            {
                "11:00", "11:30", "12:00", "12:30", "13:00", "13:30",
                "18:00", "18:30", "19:00", "19:30", "20:00", "20:30", "21:00", "21:30"
            };
            cmbHeure.ItemsSource = heures;
            cmbHeure.SelectedIndex = 0;

            // Nombre de personnes
            List<int> nbPersonnes = Enumerable.Range(1, 12).ToList();
            cmbNbPersonnes.ItemsSource = nbPersonnes;
            cmbNbPersonnes.SelectedIndex = 1; // Par défaut 2 personnes

            // Date par défaut = aujourd'hui
            dpDate.SelectedDate = DateTime.Now;

            // Chargement des éléments de menu depuis la base de données
            LoadMenuItems();
        }

        private void LoadMenuItems()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"SELECT 
                                    item_id, 
                                    name_item, 
                                    disponible, 
                                    description, 
                                    prix, 
                                    ImagePath 
                                FROM MenuItem 
                                WHERE disponible = 1 
                                ORDER BY name_item";

                    OracleCommand cmd = new OracleCommand(query, conn);
                    OracleDataReader reader = cmd.ExecuteReader();

                    List<MenuItemModel> menuItems = new List<MenuItemModel>();

                    while (reader.Read())
                    {
                        menuItems.Add(new MenuItemModel
                        {
                            ItemId = Convert.ToInt32(reader["item_id"]),
                            Name = reader["name_item"].ToString(),
                            Disponible = Convert.ToBoolean(reader["disponible"]),
                            Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : "",
                            Prix = Convert.ToSingle(reader["prix"]),
                            ImagePath = reader["ImagePath"] != DBNull.Value ? reader["ImagePath"].ToString() : ""
                        });
                    }

                    lbMenuItems.ItemsSource = menuItems;

                    // Mettre à jour repository (pour l'admin)
                    if (MenuItemRepository.Items.Count == 0)
                    {
                        foreach (var item in menuItems)
                        {
                            MenuItemRepository.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des menus: " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupEventHandlers()
        {
            // Mise à jour du résumé lors de la sélection d'un menu
            lbMenuItems.SelectionChanged += (s, e) =>
            {
                _selectedMenuItem = lbMenuItems.SelectedItem as MenuItemModel;
                UpdateSummary();
            };

            // Mise à jour du résumé lors du changement de date/heure/personnes
            dpDate.SelectedDateChanged += (s, e) => UpdateSummary();
            cmbHeure.SelectionChanged += (s, e) => UpdateSummary();
            cmbNbPersonnes.SelectionChanged += (s, e) => UpdateSummary();
        }

        private void UpdateSummary()
        {
            // Mise à jour du menu sélectionné
            txtSelectedMenu.Text = _selectedMenuItem != null
                ? $"{_selectedMenuItem.Name} ({_selectedMenuItem.Prix:C})"
                : "-";

            // Mise à jour de la date et heure
            if (dpDate.SelectedDate.HasValue && cmbHeure.SelectedItem != null)
            {
                txtSelectedDateTime.Text = $"{dpDate.SelectedDate.Value.ToShortDateString()} à {cmbHeure.SelectedItem}";
            }
            else
            {
                txtSelectedDateTime.Text = "-";
            }

            // Mise à jour du nombre de personnes
            txtSelectedPersonnes.Text = cmbNbPersonnes.SelectedItem != null
                ? cmbNbPersonnes.SelectedItem.ToString()
                : "-";
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            // Retour à la page précédente
            NavigationService.GoBack();
        }

        private void BtnConfirmer_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    // 1. Vérifier si le client existe déjà
                    int clientid = CheckClientExists(conn);

                    // 2. Attribuer une table disponible
                    int tableId = AssignTable(conn);
                    if (tableId == -1)
                    {
                        MessageBox.Show("Désolé, aucune table n'est disponible pour ce créneau horaire et ce nombre de personnes.",
                            "Table indisponible", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // 3. Créer la réservation
                    CreateReservation(conn, clientid, tableId);

                    // 4. Mise à jour de l'UI
                    MessageBox.Show("Réservation enregistrée avec succès !",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Retour à la page précédente
                    NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la création de la réservation: " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            // Validation du nom
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MessageBox.Show("Veuillez entrer votre nom complet.",
                    "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNom.Focus();
                return false;
            }

            // Validation de l'email
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Veuillez entrer une adresse email valide.",
                    "Format invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            // Validation du téléphone
            if (string.IsNullOrWhiteSpace(txtTelephone.Text) || !IsValidPhone(txtTelephone.Text))
            {
                MessageBox.Show("Veuillez entrer un numéro de téléphone valide.",
                    "Format invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTelephone.Focus();
                return false;
            }

            // Validation de la date
            if (!dpDate.SelectedDate.HasValue || dpDate.SelectedDate.Value < DateTime.Today)
            {
                MessageBox.Show("Veuillez sélectionner une date valide (aujourd'hui ou dans le futur).",
                    "Date invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDate.Focus();
                return false;
            }

            // Validation de l'heure
            if (cmbHeure.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner une heure.",
                    "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbHeure.Focus();
                return false;
            }

            // Validation du nombre de personnes
            if (cmbNbPersonnes.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner le nombre de personnes.",
                    "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbNbPersonnes.Focus();
                return false;
            }

            // Validation du menu
            if (_selectedMenuItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un menu.",
                    "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                lbMenuItems.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            // Validation simple de l'email
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        private bool IsValidPhone(string phone)
        {
            // Validation simple du téléphone (min 8 chiffres)
            string pattern = @"^\d{8,}$";
            return Regex.IsMatch(phone, pattern);
        }

        private int CheckClientExists(OracleConnection conn)
        {
            try
            {
                // Vérifier si le client existe déjà par son email
                string checkSql = "SELECT client_id FROM Client WHERE email = :email";
                OracleCommand checkCmd = new OracleCommand(checkSql, conn);
                checkCmd.Parameters.Add(new OracleParameter("email", txtEmail.Text));

                object result = checkCmd.ExecuteScalar();

                if (result != null)
                {
                    // Client trouvé, retourner son ID
                    return Convert.ToInt32(result);
                }
                else
                {
                    // Client n'existe pas, l'ajouter

                    // 1. D'abord générer un nouvel ID
                    string seqSql = "SELECT seq_user.NEXTVAL FROM DUAL";
                    OracleCommand seqCmd = new OracleCommand(seqSql, conn);
                    int newId = Convert.ToInt32(seqCmd.ExecuteScalar());

                    // 2. Insérer dans la table usr
                    string userSql = "INSERT INTO usr (user_id, full_name, email) VALUES (:user_id, :name, :email)";
                    OracleCommand userCmd = new OracleCommand(userSql, conn);
                    userCmd.Parameters.Add(new OracleParameter("user_id", newId));
                    userCmd.Parameters.Add(new OracleParameter("name", txtNom.Text));
                    userCmd.Parameters.Add(new OracleParameter("email", txtEmail.Text));
                    userCmd.ExecuteNonQuery();

                    // 3. Insérer dans la table Client
                    string clientSql = "INSERT INTO Client (client_id, full_name, num_phone, email) VALUES (:client_id, :name, :phone, :email)";
                    OracleCommand clientCmd = new OracleCommand(clientSql, conn);
                    clientCmd.Parameters.Add(new OracleParameter("client_id", newId));
                    clientCmd.Parameters.Add(new OracleParameter("name", txtNom.Text));
                    clientCmd.Parameters.Add(new OracleParameter("phone", txtTelephone.Text));
                    clientCmd.Parameters.Add(new OracleParameter("email", txtEmail.Text));
                    clientCmd.ExecuteNonQuery();

                    return newId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur lors de la vérification/création du client: " + ex.Message);
            }
        }

        private int AssignTable(OracleConnection conn)
        {
            try
            {
                // Trouver une table disponible avec assez de places
                string sql = @"SELECT table_id 
                             FROM RestaurantTable 
                             WHERE seats >= :nb_personnes 
                             AND ROWNUM = 1";

                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add(new OracleParameter("nb_personnes", Convert.ToInt32(cmbNbPersonnes.SelectedItem)));

                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    return -1; // Aucune table disponible
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur lors de l'attribution de table: " + ex.Message);
            }
        }

        private void CreateReservation(OracleConnection conn, int clientId, int tableId)
        {
            try
            {
                // Préparation de la date et heure
                DateTime dateReservation = dpDate.SelectedDate.Value;
                TimeSpan heureReservation = TimeSpan.Parse(cmbHeure.SelectedItem.ToString());
                DateTime dateTimeReservation = dateReservation.Date + heureReservation;

                // Insertion de la réservation
                string sql = @"INSERT INTO Reservation (
                               reservation_id, 
                               reservation_datetime, 
                               nbr_personnes, 
                               choix_item, 
                               client_id, 
                               table_id
                               )
                             VALUES (
                               seq_reservation.NEXTVAL, 
                               :datetime,
                               :nb_personnes, 
                               :choix, 
                               :client_id, 
                               :table_id
                               )";

                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add(new OracleParameter("datetime", dateTimeReservation));
                cmd.Parameters.Add(new OracleParameter("nb_personnes", Convert.ToInt32(cmbNbPersonnes.SelectedItem)));
                cmd.Parameters.Add(new OracleParameter("choix", _selectedMenuItem.ItemId));
                cmd.Parameters.Add(new OracleParameter("client_id", clientId));
                cmd.Parameters.Add(new OracleParameter("table_id", tableId));
                

                cmd.ExecuteNonQuery();

                // Ajouter la réservation localement pour l'admin
                var reservation = new Reservation
                {
                    reservation_Id = GetLastReservationId(conn),
                    Nom = txtNom.Text,
                    TableId = tableId,
                    Telephone = txtTelephone.Text,
                    nbr_Personnes = Convert.ToInt32(cmbNbPersonnes.SelectedItem),
                    Date = dateTimeReservation,
                    Choix = _selectedMenuItem.Name, // Storing the name for display
                    TimeCreated = DateTime.Now
                };

                ReservationService.Instance.AjouterReservation(reservation);
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur lors de la création de la réservation: " + ex.Message);
            }
        }

        private int GetLastReservationId(OracleConnection conn)
        {
            try
            {
                string sql = "SELECT MAX(reservation_id) FROM Reservation";
                OracleCommand cmd = new OracleCommand(sql, conn);
                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
                return 1; // Default if no reservations yet
            }
            catch
            {
                return 1; // Default on error
            }
        }
    }
}