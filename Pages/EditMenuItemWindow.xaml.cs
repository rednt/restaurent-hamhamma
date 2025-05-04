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
using System.Windows.Shapes;
using Oracle.ManagedDataAccess.Client;
using restaurent_hamhamma.Models;

namespace restaurent_hamhamma.Pages
{
    public partial class EditMenuItemWindow : Window
    {
        private MenuItemModel _menuItem;

        public EditMenuItemWindow(MenuItemModel item)
        {
            InitializeComponent();
            _menuItem = item;
            DataContext = _menuItem;  // Bind data to the window controls
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Save the changes back to the database
            try
            {
                using (OracleConnection conn = new OracleConnection(@"User Id=Hamhama;Password=1234;Data Source=localhost:1521/XE;"))
                {
                    conn.Open();

                    string query = @"UPDATE MenuItem 
                                 SET name_item = :name, 
                                     description = :description, 
                                     prix = :prix, 
                                     disponible = :disponible 
                                 WHERE item_id = :itemId";

                    OracleCommand cmd = new OracleCommand(query, conn);
                    cmd.Parameters.Add(new OracleParameter("name", _menuItem.Name));
                    cmd.Parameters.Add(new OracleParameter("description", _menuItem.Description ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new OracleParameter("prix", _menuItem.Prix));
                    cmd.Parameters.Add(new OracleParameter("disponible", _menuItem.Disponible ? 1 : 0));
                    cmd.Parameters.Add(new OracleParameter("itemId", _menuItem.ItemId));

                    cmd.ExecuteNonQuery();
                }

                // Update the repository (in-memory data)
                var existingItem = MenuItemRepository.Items.FirstOrDefault(i => i.ItemId == _menuItem.ItemId);
                if (existingItem != null)
                {
                    existingItem.Name = _menuItem.Name;
                    existingItem.Description = _menuItem.Description;
                    existingItem.Prix = _menuItem.Prix;
                    existingItem.Disponible = _menuItem.Disponible;
                }

                MessageBox.Show("Élément de menu modifié avec succès !");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la modification : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
