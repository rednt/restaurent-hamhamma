using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace restaurent_hamhamma.Models
{
    public class ClientFeedback
    {
        public int ClientId { get; set; }
        public int ItemId { get; set; }
        public int Rating { get; set; }
        public string Commentaire { get; set; }
        public DateTime DateInteracted { get; set; }

        // Navigation properties
        public Client Client { get; set; }
        public MenuItem MenuItem { get; set; }
    }
}
