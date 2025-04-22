using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace restaurent_hamhamma.Models
{
    public class MenuItemModel
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public bool Disponible { get; set; }
        public string Description { get; set; }
        public float Prix { get; set; }
        public string ImagePath { get; set; }

        // Navigation properties
        public ICollection<ClientFeedback> Feedbacks { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
