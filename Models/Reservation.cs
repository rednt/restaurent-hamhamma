using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace restaurent_hamhamma.Models
{
    public class Reservation
    {
        public int reservation_Id { get; set; }
        public string Nom { get; set; }
        public int TableId { get; set; }
        public string Telephone { get; set; }
        public int nbr_Personnes { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Heure { get; set; }
        public string Commentaire { get; set; }
        public string Choix { get; set; }
        public DateTime TimeCreated { get; set; }
        public Client Client { get; set; }
        public RestaurantTable Table { get; set; }
        public ICollection<MenuItem> MenuItems { get; set; }
    }
}
