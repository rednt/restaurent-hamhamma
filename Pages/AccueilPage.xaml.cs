using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace restaurent_hamhamma.Pages;

public partial class AccueilPage : Page
{
    public AccueilPage()
    {
        InitializeComponent();
        
    }

    private void BtnReserverTable_Click(object sender, RoutedEventArgs e)
    {
        // Naviguer vers la page de réservation
        NavigationService.Navigate(new ReservationPage());
    }
}