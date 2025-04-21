using restaurent_hamhamma.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace restaurent_hamhamma.Services
{
    public class ReservationService
    {
        private static ReservationService _instance;
        private static readonly object _lock = new object();

        public ObservableCollection<Reservation> Reservations { get; private set; }

        private ReservationService()
        {
            Reservations = new ObservableCollection<Reservation>();
            // Ajouter quelques données de test
            Reservations.Add(new Reservation
            {
                reservation_Id = 1,
                Nom = "Dupont",
                Telephone = "06 12 34 56 78",
                nbr_Personnes = 4,
                Date = DateTime.Today.AddDays(1),
                Heure = new TimeSpan(19, 30, 0),
                Commentaire = "Table près de la fenêtre"
            });
            Reservations.Add(new Reservation
            {
                reservation_Id = 2,
                Nom = "Martin",
                Telephone = "07 98 76 54 32",
                nbr_Personnes = 2,
                Date = DateTime.Today.AddDays(2),
                Heure = new TimeSpan(20, 0, 0),
                Commentaire = ""
            });
        }

        public static ReservationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ReservationService();
                        }
                    }
                }
                return _instance;
            }
        }

        public void AjouterReservation(Reservation reservation)
        {
            reservation.reservation_Id = Reservations.Count > 0 ? Reservations[Reservations.Count - 1].reservation_Id + 1 : 1;
            Reservations.Add(reservation);
        }

        public void SupprimerReservation(int id)
        {
            var reservation = Reservations.FirstOrDefault(r => r.reservation_Id == id);
            if (reservation != null)
            {
                Reservations.Remove(reservation);
            }
        }
    }
}