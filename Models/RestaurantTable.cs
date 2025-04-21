using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace restaurent_hamhamma.Models
{
    public class RestaurantTable
    {
        public int TableId { get; set; }
        public int NumTable { get; set; }
        public int Seats { get; set; }

        public bool IsAvailable(List<Reservation> reservations)
        {
            // Check if the table is available based on reservations
            foreach (var reservation in reservations)
            {
                if (reservation.TableId == TableId)
                {
                    return false;
                }
            }
            return true;
        }

        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; }
    }
}
