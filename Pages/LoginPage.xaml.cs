using Oracle.ManagedDataAccess.Client;
using System;
using System.Windows;
using System.Windows.Controls;
using dotenv.net;

namespace restaurent_hamhamma.Pages
{
    public partial class LoginPage : Page
    {
        private readonly OracleConnection conn;

        public LoginPage()
        {
            InitializeComponent();

            // Load environment variables from .env
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

            string user = Environment.GetEnvironmentVariable("DB_USER");
            string password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string host = Environment.GetEnvironmentVariable("DB_HOST");
            string port = Environment.GetEnvironmentVariable("DB_PORT");
            string service = Environment.GetEnvironmentVariable("DB_SERVICE");

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(service))
            {
                throw new InvalidOperationException("One or more required environment variables are missing.");
            }

            string connectionString = $"User Id={user};Password={password};Data Source={host}:{port}/{service};Connection Timeout=30";
            conn = new OracleConnection(connectionString);
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
