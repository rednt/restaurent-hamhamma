using System.Collections.Generic;
using System.ComponentModel;

namespace restaurent_hamhamma.Models
{
    public class MenuItemModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int itemId;
        public int ItemId
        {
            get => itemId;
            set
            {
                itemId = value;
                OnPropertyChanged(nameof(ItemId));
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private bool disponible;
        public bool Disponible
        {
            get => disponible;
            set
            {
                disponible = value;
                OnPropertyChanged(nameof(Disponible));
            }
        }

        private string description;
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private float prix;
        public float Prix
        {
            get => prix;
            set
            {
                prix = value;
                OnPropertyChanged(nameof(Prix));
            }
        }

        private string imagePath;
        public string ImagePath
        {
            get => imagePath;
            set
            {
                imagePath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        // Navigation properties (not required for UI)
        public ICollection<ClientFeedback> Feedbacks { get; set; }
        public ICollection<Reservation> Reservations { get; set; }

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
