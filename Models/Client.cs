using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace restaurent_hamhamma.Models
{
    public class Client : User
    {
        public string NumPhone { get; set; }

        // Navigation properties
        public ICollection<ClientFeedback> Feedbacks { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
