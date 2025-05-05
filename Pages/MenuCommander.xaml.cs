using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using restaurent_hamhamma.Models;

namespace restaurent_hamhamma.Pages
{
    public partial class MenuCommander : Window
    {
        private MenuItemModel _selectedMenuItem;
        private int _quantity = 1;

        public MenuCommander()
        {
            InitializeComponent();
        }

        public MenuCommander(MenuItemModel selectedMenuItem) : this()
        {
            _selectedMenuItem = selectedMenuItem;
            LoadMenuItemDetails();
        }

        private void LoadMenuItemDetails()
        {
            if (_selectedMenuItem != null)
            {
                // Afficher les détails du plat sélectionné
                ItemName.Text = _selectedMenuItem.Name;
                ItemDescription.Text = _selectedMenuItem.Description;
                ItemPrice.Text = _selectedMenuItem.Prix.ToString("F2");
                ItemAvailability.Text = _selectedMenuItem.Disponible ? "Oui" : "Non";

                // Charger l'image si disponible
                try
                {
                    if (!string.IsNullOrEmpty(_selectedMenuItem.ImagePath))
                    {
                        ItemImage.Source = new BitmapImage(new Uri(_selectedMenuItem.ImagePath, UriKind.RelativeOrAbsolute));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement de l'image: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                UpdateTotalPrice();
            }
        }

        private void UpdateTotalPrice()
        {
            if (_selectedMenuItem != null && int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                float total = _selectedMenuItem.Prix * quantity;
                TotalPriceTextBlock.Text = total.ToString("F2");
            }
        }

        private void IncrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                QuantityTextBox.Text = (quantity + 1).ToString();
                UpdateTotalPrice();
            }
        }

        private void DecrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int quantity) && quantity > 1)
            {
                QuantityTextBox.Text = (quantity - 1).ToString();
                UpdateTotalPrice();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Valider les entrées
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Veuillez entrer votre nom.", "Champ obligatoire", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                MessageBox.Show("Veuillez entrer votre numéro de téléphone.", "Champ obligatoire", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Veuillez entrer votre adresse de livraison.", "Champ obligatoire", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Créer l'objet commande (à personnaliser selon votre modèle de données)
            var order = new OrderModel
            {
                MenuItem = _selectedMenuItem,
                MenuItemId = _selectedMenuItem.ItemId,
                Quantity = int.Parse(QuantityTextBox.Text),
                SpecialInstructions = SpecialInstructionsTextBox.Text,
                CustomerName = NameTextBox.Text,
                CustomerPhone = PhoneTextBox.Text,
                DeliveryAddress = AddressTextBox.Text,
                OrderDate = DateTime.Now,
                TotalPrice = float.Parse(TotalPriceTextBlock.Text)
            };

            // Ici, vous pouvez ajouter le code pour enregistrer la commande dans votre base de données
            // OrderRepository.AddOrder(order);

            MessageBox.Show($"Votre commande a été enregistrée avec succès!\n\nRécapitulatif:\n- {order.Quantity} x {_selectedMenuItem.Name}\n- Total: {order.TotalPrice} €\n\nVous serez livré à l'adresse indiquée dans les plus brefs délais.",
                "Commande confirmée", MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }
    }
}

// Classe modèle pour les commandes (à ajouter à votre projet dans le dossier Models)
namespace restaurent_hamhamma.Models
{
    public class OrderModel
    {
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public MenuItemModel MenuItem { get; set; }
        public int Quantity { get; set; }
        public string SpecialInstructions { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime OrderDate { get; set; }
        public float TotalPrice { get; set; }
    }
}