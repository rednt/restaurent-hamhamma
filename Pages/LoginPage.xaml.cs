using Oracle.ManagedDataAccess.Client;
using System;
using System.Windows;
using System.Windows.Controls;


namespace restaurent_hamhamma.Pages
{
    public partial class LoginPage : Page
    {
        OracleConnection conn = new OracleConnection(@"User Id=Hamhama;Password=1234;Data Source=localhost:1521/XE;Connection Timeout=30");

        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            try
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM admin_tab WHERE username = :username AND pwd = :password";

                OracleCommand cmd = new OracleCommand(query, conn);
                cmd.Parameters.Add(new OracleParameter("username", username));
                cmd.Parameters.Add(new OracleParameter("password", password));

                int userCount = Convert.ToInt32(cmd.ExecuteScalar());

                if (userCount > 0)
                {
                    NavigationService.Navigate(new AdminPage());
                    MessageBox.Show("Login successful!");
                }
                else
                {
                    MessageBox.Show("Invalid username or password.");
                }
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
    }
}
