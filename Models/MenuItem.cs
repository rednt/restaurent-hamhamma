using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace restaurent_hamhamma.Models
{
    public class MenuItem
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public bool Disponible { get; set; }
        public float Prix { get; set; }

        // Navigation properties
        public ICollection<ClientFeedback> Feedbacks { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
