using System.Windows;
using restaurent_hamhamma.Pages;

namespace restaurent_hamhamma
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Afficher la page d'accueil par défaut
            MainFrame.NavigationService.Navigate(new AccueilPage());
        }
        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new Menu());
        }
        private void BtnAccueil_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new AccueilPage());
        }

        private void BtnReservation_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new ReservationPage());
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService.Navigate(new LoginPage());
        }
    }
}
