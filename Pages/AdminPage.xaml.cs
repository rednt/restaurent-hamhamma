using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using restaurent_hamhamma.Services;
using restaurent_hamhamma.Models;

namespace restaurent_hamhamma.Pages
{
    public partial class AdminPage : Page
    {
        private CollectionViewSource reservationsViewSource;

        public AdminPage()
        {
            InitializeComponent();

            // Initialiser la source de vue
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

        private void BtnAjouterMenuItem_Click(object sender, RoutedEventArgs e) 
        {
            
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

        
    }
}