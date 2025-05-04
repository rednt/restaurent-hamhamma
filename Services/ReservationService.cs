using restaurent_hamhamma.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace restaurent_hamhamma.Services
{
    public class ReservationService
    {
        private static ReservationService _instance;
        private static readonly object _lock = new object();
        private readonly string _connectionString = @"User Id=Hamhama;Password=1234;Data Source=localhost:1521/XE;Connection Timeout=30";

        public ObservableCollection<Reservation> Reservations { get; private set; }

        private ReservationService()
        {
            Reservations = new ObservableCollection<Reservation>();
            LoadReservationsFromDatabase();
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

        // Charger les réservations depuis la base de données
        public void LoadReservationsFromDatabase()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"SELECT 
                                   cl.client_id,
                                   cl.full_name, 
                                   cl.num_phone, 
                                   cl.email,
                                   r.choix_item, 
                                   TO_CHAR(r.reservation_datetime, 'YYYY-MM-DD') AS date_reservation,
                                   r.reservation_id
                                   FROM Reservation r
                                   JOIN Client cl ON r.client_id = cl.client_id
                                   LEFT JOIN MenuItem mi ON r.choix_item = mi.name_item
                                   ORDER BY 
                                   r.reservation_datetime DESC;";

                    OracleCommand cmd = new OracleCommand(query, conn);
                    OracleDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Reservations.Add(new Reservation
                        {
                            ClientID = Convert.ToInt32(reader["client_id"]),
                            reservation_Id = Convert.ToInt32(reader["reservation_id"]),
                            Date = Convert.ToDateTime(reader["reservation_datetime"]),
                            nbr_Personnes = reader["nbr_personnes"] != DBNull.Value ? Convert.ToInt32(reader["nbr_personnes"]) : 0,
                            Choix = reader["choix_item"].ToString(),
                            ChoixItem = reader["choix_item"] != DBNull.Value ? reader["choix_item"].ToString() : "N/A",
                            Nom = reader["full_name"].ToString(),
                            Telephone = reader["num_phone"].ToString(),
                            Email = reader["email"].ToString(),
                            TableId = reader["table_id"] != DBNull.Value ? Convert.ToInt32(reader["table_id"]) : 0,
                            TimeCreated = DateTime.Now
                        });
                    }


                }
            }
            catch (Exception ex)
            {
                // En cas d'erreur, nous allons simplement laisser la collection vide
                // et enregistrer l'erreur pour le débogage
                System.Diagnostics.Debug.WriteLine("Erreur lors du chargement des réservations: " + ex.Message);
            }
        }

        public void AjouterReservation(Reservation reservation)
        {
            Reservations.Add(reservation);
        }

        public void SupprimerReservation(int id)
        {
            // Supprimer de la liste locale
            var reservation = Reservations.FirstOrDefault(r => r.reservation_Id == id);
            if (reservation != null)
            {
                Reservations.Remove(reservation);
            }

            // Supprimer de la base de données
            try
            {
                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM Reservation WHERE reservation_id = :id";
                    OracleCommand cmd = new OracleCommand(query, conn);
                    cmd.Parameters.Add(new OracleParameter("id", id));

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erreur lors de la suppression de réservation: " + ex.Message);
                throw new Exception("Impossible de supprimer la réservation: " + ex.Message);
            }
        }
    }
}