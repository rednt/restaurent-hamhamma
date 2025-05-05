using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using dotenv.net;
using restaurent_hamhamma.Models;
using Oracle.ManagedDataAccess.Client;

namespace restaurent_hamhamma.Pages
{
    public partial class Menu : Page
    
    {
        private readonly string _connectionString;
        public Menu()
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing the Admin page: {ex.Message}",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Exception in AdminPage constructor: {ex}");
            }
            LoadMenuItems();
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
                                WHERE disponible != 0
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
        private void Commander_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer l'élément du menu sélectionné
            if (sender is Button button && button.DataContext is MenuItemModel selectedMenuItem)
            {
                // Vérifier si l'élément est disponible
                if (!selectedMenuItem.Disponible)
                {
                    MessageBox.Show("Nous sommes désolés, ce plat n'est pas disponible actuellement.",
                        "Plat indisponible", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Ouvrir la fenêtre de commande avec les détails du plat sélectionné
                MenuCommander menuCommander = new MenuCommander(selectedMenuItem);
                menuCommander.ShowDialog();
            }
        }
    }
}