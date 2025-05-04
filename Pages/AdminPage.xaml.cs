using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using restaurent_hamhamma.Services;
using restaurent_hamhamma.Models;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;

namespace restaurent_hamhamma.Pages
{
    public partial class AdminPage : Page
    {
        private CollectionViewSource reservationsViewSource;

        OracleConnection conn = new OracleConnection(@"User Id=Hamhama;Password=1234;Data Source=localhost:1521/XE;Connection Timeout=30");

        public AdminPage()
        {
            InitializeComponent();
            LoadMenuItems();
            LoadReservations(); // Load reservations before binding
        }

        private void LoadReservations()
        {
            ReservationService.Instance.LoadReservationsFromDatabase();

            reservationsViewSource = new CollectionViewSource();
            reservationsViewSource.Source = ReservationService.Instance.Reservations;
            dgReservations.ItemsSource = reservationsViewSource.View;
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
        private void LoadMenuItems()
        {
            try
            {
                using (conn)
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

                    lbMenuItems.ItemsSource = MenuItemRepository.Items;

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
        private void BtnAjouterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtMenuName.Text))
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
                Description = txtMenuDescription.Text,
                Prix = price,
                Disponible = chkDisponible.IsChecked == true,
                ImagePath = txtMenuImagePath.Text
            };

            try
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
                MenuItemRepository.Items.Add(item); 
                MessageBox.Show("Menu item added successfully!");

                // Clear form fields
                txtMenuName.Text = string.Empty;
                txtMenuPrice.Text = string.Empty;
                txtMenuDescription.Text = string.Empty;
                txtMenuImagePath.Text = string.Empty;
                chkDisponible.IsChecked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
        
        private void ApplyFilters()
        {
            reservationsViewSource.View.Filter = item =>
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

        private void DgReservations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isItemSelected = dgReservations.SelectedItem != null;
            btnModifier.IsEnabled = isItemSelected;
            btnSupprimer.IsEnabled = isItemSelected;
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
                // Dans une application réelle, on ouvrirait une fenêtre d'édition
                MessageBox.Show($"Modification de la réservation de {selectedReservation.Nom} en cours de développement.",
                    "Fonctionnalité à venir", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
                    ReservationService.Instance.SupprimerReservation(selectedReservation.reservation_Id);
                    // Rafraîchir la vue
                    reservationsViewSource.View.Refresh();
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
                        using (conn)
                        {
                            conn.Open();

                            string query = "DELETE FROM Reservation WHERE reservation_id = :id";
                            OracleCommand cmd = new OracleCommand(query, conn);
                            cmd.Parameters.Add(new OracleParameter("id", item.ItemId));

                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Erreur lors de la suppression de réservation: " + ex.Message);
                        throw new Exception("Impossible de supprimer la réservation: " + ex.Message);
                    }
                    MenuItemRepository.Items.Remove(item);

                }
            }
        }
        private void BtnModifierItem_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the selected menu item
            if (sender is Button button && button.DataContext is MenuItemModel selectedItem)
            {
                // Open a window to modify the selected item
                
                EditMenuItemWindow editWindow = new EditMenuItemWindow(selectedItem);
                editWindow.ShowDialog();

                // If the user saved the changes, the repository and database will be updated.
                // Update the display (in case any changes were made)
                lbMenuItems.ItemsSource = MenuItemRepository.Items; // Refresh the list
            }
            else
            {
                MessageBox.Show("Aucun élément sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



    }
}