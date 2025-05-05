using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using restaurent_hamhamma.Services;
using restaurent_hamhamma.Models;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Diagnostics;
using dotenv.net;

namespace restaurent_hamhamma.Pages
{
    public partial class AdminPage : Page
    {
        private readonly string _connectionString;
        private bool isEditMode = false;
        private Reservation editingReservation = null;

        public AdminPage()
        {
            InitializeComponent();

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

                // Set up direct binding for DataGrid
                LoadData();

                // Add placeholder text to form fields
                SetupFormFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing the Admin page: {ex.Message}",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Exception in AdminPage constructor: {ex}");
            }
        
        }

        private void SetupFormFields()
        {
            // Add placeholder text
            txtMenuName.Text = "Nom";
            txtMenuDescription.Text = "Description";
            txtMenuPrice.Text = "Prix";
            txtMenuImagePath.Text = "Chemin de l'image";
            chkDisponible.IsChecked = true;
        }

        private void LoadData()
        {
            try
            {
                // Ensure ReservationService is initialized and loads data
                Debug.WriteLine("Loading reservations from database...");
                ReservationService.Instance.LoadReservationsFromDatabase();

                // Directly bind the DataGrid to the Reservations collection
                dgReservations.ItemsSource = ReservationService.Instance.Reservations;

                // Also load menu items
                LoadMenuItems();

                Debug.WriteLine($"Loaded {ReservationService.Instance.Reservations.Count} reservations");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}",
                    "Data Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Exception in LoadData: {ex}");
            }
        }

        // Add Page_Loaded event handler
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Page_Loaded event fired");

            // Force UI update
            if (dgReservations.ItemsSource == null)
            {
                LoadData();
            }
            else
            {
                // Refresh just in case
                dgReservations.Items.Refresh();
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Clear placeholder text when textbox gets focus
            if (sender is TextBox textBox)
            {
                if (textBox.Text == textBox.Tag?.ToString() ||
                    (textBox == txtMenuName && textBox.Text == "Nom") ||
                    (textBox == txtMenuDescription && textBox.Text == "Description") ||
                    (textBox == txtMenuPrice && textBox.Text == "Prix") ||
                    (textBox == txtMenuImagePath && textBox.Text == "Chemin de l'image"))
                {
                    textBox.Text = string.Empty;
                }
            }
        }

        private void LoadMenuItems()
        {
            try
            {
                Debug.WriteLine("Loading menu items...");
                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                    conn.Open();
                    Debug.WriteLine("Database connection opened");

                    string query = @"SELECT 
                                    item_id, 
                                    name_item, 
                                    disponible, 
                                    description, 
                                    prix, 
                                    ImagePath 
                                FROM MenuItem 
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

                    // Clear and repopulate the MenuItemRepository
                    MenuItemRepository.Items.Clear();
                    foreach (var item in menuItems)
                    {
                        MenuItemRepository.Items.Add(item);
                    }

                    // Update the UI
                    lbMenuItems.ItemsSource = null;
                    lbMenuItems.ItemsSource = MenuItemRepository.Items;

                    Debug.WriteLine($"Loaded {menuItems.Count} menu items");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des menus: " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Exception in LoadMenuItems: {ex}");
            }
        }

        private void DpFilterDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtFilterNom_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            dpFilterDate.SelectedDate = null;
            txtFilterNom.Text = string.Empty;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            // Create a CollectionView since we're not using CollectionViewSource
            if (dgReservations.ItemsSource != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(dgReservations.ItemsSource);

                view.Filter = item =>
                {
                    if (item is Reservation reservation)
                    {
                        // Filtre par date
                        if (dpFilterDate.SelectedDate != null && reservation.Date.Date != dpFilterDate.SelectedDate.Value.Date)
                        {
                            return false;
                        }

                        // Filtre par nom
                        if (!string.IsNullOrWhiteSpace(txtFilterNom.Text) &&
                            !reservation.Nom.ToLower().Contains(txtFilterNom.Text.ToLower()))
                        {
                            return false;
                        }

                        return true;
                    }
                    return false;
                };
            }
        }

        private void BtnAjouterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtMenuName.Text) || txtMenuName.Text == "Nom")
            {
                MessageBox.Show("Please enter a name for the menu item.");
                return;
            }

            if (!float.TryParse(txtMenuPrice.Text, out var price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price.");
                return;
            }

            var item = new MenuItemModel
            {
                Name = txtMenuName.Text,
                Description = txtMenuDescription.Text == "Description" ? "" : txtMenuDescription.Text,
                Prix = price,
                Disponible = chkDisponible.IsChecked == true,
                ImagePath = txtMenuImagePath.Text == "Chemin de l'image" ? "" : txtMenuImagePath.Text
            };

            try
            {
                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"INSERT INTO MenuItem 
                            (item_id, name_item, disponible, description, ImagePath, prix) 
                            VALUES 
                            (seq_menuitem.nextval, :name, :disponible, :description, :imagepath, :prix)";

                    OracleCommand cmd = new OracleCommand(query, conn);
                    cmd.Parameters.Add(new OracleParameter("name", item.Name));
                    cmd.Parameters.Add(new OracleParameter("disponible", item.Disponible ? 1 : 0)); // Convert boolean to 1/0
                    cmd.Parameters.Add(new OracleParameter("description", item.Description ?? (object)DBNull.Value)); // Handle null values
                    cmd.Parameters.Add(new OracleParameter("imagepath", item.ImagePath ?? (object)DBNull.Value)); // Handle null values
                    cmd.Parameters.Add(new OracleParameter("prix", item.Prix));

                    cmd.ExecuteNonQuery();

                    // Get the new ID to complete the item
                    query = "SELECT seq_menuitem.currval FROM dual";
                    cmd = new OracleCommand(query, conn);
                    item.ItemId = Convert.ToInt32(cmd.ExecuteScalar());

                    MenuItemRepository.Items.Add(item);
                    MessageBox.Show("Menu item added successfully!");

                    // Clear form fields
                    txtMenuName.Text = "Nom";
                    txtMenuPrice.Text = "Prix";
                    txtMenuDescription.Text = "Description";
                    txtMenuImagePath.Text = "Chemin de l'image";
                    chkDisponible.IsChecked = true;

                    // Refresh the menu items list
                    lbMenuItems.ItemsSource = null;
                    lbMenuItems.ItemsSource = MenuItemRepository.Items;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                Debug.WriteLine($"Exception in BtnAjouterMenuItem_Click: {ex}");
            }
        }

        private void DgReservations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isItemSelected = dgReservations.SelectedItem != null;
            btnModifier.IsEnabled = isItemSelected;
            btnSupprimer.IsEnabled = isItemSelected;

            // If we're in edit mode and selection changes, disable edit mode
            if (isEditMode && e.AddedItems.Count > 0)
            {
                ExitEditMode();
            }
        }

        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            // Naviguer vers la page de réservation pour ajouter une nouvelle réservation
            NavigationService.Navigate(new ReservationPage());
        }

        private void BtnModifier_Click(object sender, RoutedEventArgs e)
        {
            if (dgReservations.SelectedItem is Reservation selectedReservation)
            {
                try
                {
                    // Enable edit mode for the DataGrid
                    EnterEditMode(selectedReservation);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error enabling edit mode: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Debug.WriteLine($"Exception in BtnModifier_Click: {ex}");
                }
            }
        }

        private void EnterEditMode(Reservation reservation)
        {
            // Save the current reservation for possible rollback
            editingReservation = reservation;

            // Enable editing in the DataGrid
            dgReservations.IsReadOnly = false;

            // Make the save button visible and enable it
            btnSauvegarder.IsEnabled = true;

            // Disable other action buttons during edit
            btnAjouter.IsEnabled = false;
            btnModifier.IsEnabled = false;
            btnSupprimer.IsEnabled = false;

            // Set the flag that we're in edit mode
            isEditMode = true;

            // Begin edit on the selected item
            dgReservations.BeginEdit();

            MessageBox.Show("Vous pouvez maintenant modifier les champs directement dans la grille. " +
                          "Cliquez sur 'Sauvegarder' quand vous avez terminé.",
                          "Mode Édition", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitEditMode()
        {
            // Disable editing in the DataGrid
            dgReservations.IsReadOnly = true;

            // Reset UI buttons
            btnSauvegarder.IsEnabled = false;
            btnAjouter.IsEnabled = true;
            btnModifier.IsEnabled = dgReservations.SelectedItem != null;
            btnSupprimer.IsEnabled = dgReservations.SelectedItem != null;

            // Clear the edit flag
            isEditMode = false;
            editingReservation = null;
        }

        private void BtnSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // End the current edit on the DataGrid
                dgReservations.CommitEdit();

                // Get the currently edited reservation
                if (dgReservations.SelectedItem is Reservation updatedReservation)
                {
                    // Update the reservation in the database
                    ReservationService.Instance.ModifierReservation(updatedReservation);

                    MessageBox.Show("Réservation mise à jour avec succès!",
                                  "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Exit edit mode
                ExitEditMode();

                // Refresh the view
                dgReservations.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde: {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Exception in BtnSauvegarder_Click: {ex}");
            }
        }

        private void DgReservations_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Debug.WriteLine($"Cell edited: {e.Column.Header}");
        }

        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (dgReservations.SelectedItem is Reservation selectedReservation)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer la réservation de {selectedReservation.Nom} ?",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        ReservationService.Instance.SupprimerReservation(selectedReservation.reservation_Id);
                        dgReservations.Items.Refresh();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting reservation: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Debug.WriteLine($"Exception in BtnSupprimer_Click: {ex}");
                    }
                }
            }
        }

        private void BtnSupprimerItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is MenuItemModel item)
            {
                var result = MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer \"{item.Name}\" ?",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (OracleConnection conn = new OracleConnection(_connectionString))
                        {
                            conn.Open();

                            string query = "DELETE FROM MenuItem WHERE item_id = :id";
                            OracleCommand cmd = new OracleCommand(query, conn);
                            cmd.Parameters.Add(new OracleParameter("id", item.ItemId));

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                // Remove from local collection after successful DB deletion
                                MenuItemRepository.Items.Remove(item);

                                // Refresh UI
                                lbMenuItems.ItemsSource = null;
                                lbMenuItems.ItemsSource = MenuItemRepository.Items;

                                MessageBox.Show($"L'article \"{item.Name}\" a été supprimé.",
                                    "Suppression réussie", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show($"L'article \"{item.Name}\" n'a pas pu être supprimé.",
                                    "Erreur de suppression", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lors de la suppression: " + ex.Message,
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        Debug.WriteLine($"Exception in BtnSupprimerItem_Click: {ex}");
                    }
                }
            }
        }

        private void BtnModifierItem_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the selected menu item
            if (sender is Button button && button.DataContext is MenuItemModel selectedItem)
            {
                try
                {
                    // Open a window to modify the selected item
                    EditMenuItemWindow editWindow = new EditMenuItemWindow(selectedItem);
                    bool? result = editWindow.ShowDialog();

                    if (result == true)
                    {
                        // Refresh the list if changes were made
                        lbMenuItems.ItemsSource = null;
                        lbMenuItems.ItemsSource = MenuItemRepository.Items;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening edit window: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Debug.WriteLine($"Exception in BtnModifierItem_Click: {ex}");
                }
            }
            else
            {
                MessageBox.Show("Aucun élément sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}