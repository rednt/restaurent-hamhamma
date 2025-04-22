using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace restaurent_hamhamma.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            if (username == "admin" && password == "password")
            {
                // Verifification avec la base de données
                NavigationService.Navigate(new AdminPage());
            }
            else
            {
                MessageBox.Show("Identifiants incorrects. Veuillez réessayer. ( Verifification avec la base de données )", "Warning");
                NavigationService.Navigate(new AdminPage());
            }
            
        }
    }
}
