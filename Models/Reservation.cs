using System;
using System.Collections.Generic;

namespace restaurent_hamhamma.Models
{
    public class Reservation
    {
        public int reservation_Id { get; set; }
        public string Nom { get; set; }
        public int TableId { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public int nbr_Personnes { get; set; }
        public DateTime Date { get; set; }
        public string Commentaire { get; set; }
        public string Choix { get; set; }
       
        public DateTime TimeCreated { get; set; }
        public int ClientID { get; set; }
        public RestaurantTable TableID { get; set; }
        public ICollection<MenuItemModel> MenuItems { get; set; }
    }
}