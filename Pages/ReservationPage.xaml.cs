using System;
using System.Windows;
using System.Windows.Controls;
using restaurent_hamhamma.Models;
using restaurent_hamhamma.Services;

namespace restaurent_hamhamma.Pages
{
    public partial class ReservationPage : Page
    {
        public ReservationPage()
        {
            InitializeComponent();
            dpDate.SelectedDate = DateTime.Today;
        }

        private void BtnReserver_Click(object sender, RoutedEventArgs e)
        {
            // Valider les entrées
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MessageBox.Show("Veuillez entrer votre nom.", "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNom.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTelephone.Text))
            {
                MessageBox.Show("Veuillez entrer votre numéro de téléphone.", "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTelephone.Focus();
                return;
            }

            if (dpDate.SelectedDate == null)
            {
                MessageBox.Show("Veuillez sélectionner une date.", "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDate.Focus();
                return;
            }

            if (cmbHeure.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner une heure.", "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbHeure.Focus();
                return;
            }

            // Créer un objet réservation
            var reservation = new Reservation
            {
                Nom = txtNom.Text,
                Telephone = txtTelephone.Text,
                nbr_Personnes = cmbPersonnes.SelectedIndex + 1,
                Date = dpDate.SelectedDate.Value,
                Heure = TimeSpan.Parse(((ComboBoxItem)cmbHeure.SelectedItem).Content.ToString()),
                Commentaire = txtCommentaire.Text
            };

            // Ajouter la réservation
            ReservationService.Instance.AjouterReservation(reservation);

            // Afficher le message de confirmation
            borderConfirmation.Visibility = Visibility.Visible;

            // Réinitialiser le formulaire
            txtNom.Text = string.Empty;
            txtTelephone.Text = string.Empty;
            cmbPersonnes.SelectedIndex = 1; // 2 personnes par défaut
            dpDate.SelectedDate = DateTime.Today;
            cmbHeure.SelectedIndex = 6; // 19:30 par défaut
            txtCommentaire.Text = string.Empty;
        }
        

        private void txtCommentaire_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
