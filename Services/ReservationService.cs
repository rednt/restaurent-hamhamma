using restaurent_hamhamma.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Diagnostics;
using dotenv.net;
using DotNetEnv;

namespace restaurent_hamhamma.Services
    
{

    public class ReservationService
    {
        

        private static ReservationService _instance;
        private static readonly object _lock = new object();
        private readonly string _connectionString;

        public ObservableCollection<Reservation> Reservations { get; private set; }

        private ReservationService()
        {
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            // Load variables from .env

            string user = Environment.GetEnvironmentVariable("DB_USER");
            string password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string host = Environment.GetEnvironmentVariable("DB_HOST");
            string port = Environment.GetEnvironmentVariable("DB_PORT");
            string service = Environment.GetEnvironmentVariable("DB_SERVICE");

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(service))
            {
                throw new InvalidOperationException("One or more required environment variables are missing.");
            }

            _connectionString = $"User Id={user};Password={password};Data Source={host}:{port}/{service};Connection Timeout=30";

            Reservations = new ObservableCollection<Reservation>();
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
                Debug.WriteLine("ReservationService: Starting to load reservations from database");

                
                Reservations.Clear();

                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                   
                    try
                    {
                        conn.Open();
                        Debug.WriteLine("ReservationService: Database connection opened successfully");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ReservationService: Failed to open database connection: {ex.Message}");
                        throw new Exception("Failed to connect to the database. Please check your connection settings.", ex);
                    }

                    string query = @"SELECT 
                                     cl.client_id AS client_id,
                                     cl.full_name AS full_name, 
                                     cl.num_phone AS num_phone, 
                                     cl.email AS email,
                                     r.choix_item AS choix_item, 
                                     r.reservation_datetime,
                                     r.reservation_id,
                                     r.nbr_personnes,
                                     r.table_id
                                    FROM Reservation r
                                    JOIN Client cl ON r.client_id = cl.client_id
                                    LEFT JOIN MenuItem mi ON r.choix_item = mi.name_item
                                    ORDER BY r.reservation_datetime DESC";

                    OracleCommand cmd = new OracleCommand(query, conn);
                    OracleDataReader reader = cmd.ExecuteReader();

                    int count = 0;
                    while (reader.Read())
                    {
                        try
                        {
                            // Create a new reservation object with data from database
                            Reservation reservation = new Reservation
                            {
                                ClientID = reader["client_id"] != DBNull.Value ? Convert.ToInt32(reader["client_id"]) : 0,
                                reservation_Id = reader["reservation_id"] != DBNull.Value ? Convert.ToInt32(reader["reservation_id"]) : 0,
                                Date = reader["reservation_datetime"] != DBNull.Value ? Convert.ToDateTime(reader["reservation_datetime"]) : DateTime.Now,
                                nbr_Personnes = reader["nbr_personnes"] != DBNull.Value ? Convert.ToInt32(reader["nbr_personnes"]) : 0,
                                Choix = reader["choix_item"] != DBNull.Value ? reader["choix_item"].ToString() : "N/A",
                                Nom = reader["full_name"] != DBNull.Value ? reader["full_name"].ToString() : "",
                                Telephone = reader["num_phone"] != DBNull.Value ? reader["num_phone"].ToString() : "",
                                Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : "",
                                TableId = reader["table_id"] != DBNull.Value ? Convert.ToInt32(reader["table_id"]) : 0,
                                TimeCreated = DateTime.Now // Using current time as this might not be stored in the database
                            };

                            // Add to the collection
                            Reservations.Add(reservation);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            // Log the error but continue processing other records
                            Debug.WriteLine($"ReservationService: Error processing reservation record: {ex.Message}");
                        }
                    }

                    Debug.WriteLine($"ReservationService: Successfully loaded {count} reservations");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReservationService: Error loading reservations: {ex.Message}");
                Debug.WriteLine($"ReservationService: Stack trace: {ex.StackTrace}");

                // Re-throw for UI handling
                throw new Exception($"Failed to load reservations: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ensures a client exists in the database and returns the client ID.
        /// If the client doesn't exist, it creates a new one.
        /// </summary>
        private int EnsureClientExists(OracleConnection conn, Reservation reservation)
        {
            try
            {
                // First check if client already exists with the same email
                if (!string.IsNullOrEmpty(reservation.Email))
                {
                    string queryCheck = "SELECT client_id FROM Client WHERE email = :email";
                    OracleCommand cmdCheck = new OracleCommand(queryCheck, conn);
                    cmdCheck.Parameters.Add(new OracleParameter("email", reservation.Email));

                    object result = cmdCheck.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }

                // If not found by email, check by phone number
                if (!string.IsNullOrEmpty(reservation.Telephone))
                {
                    string queryCheck = "SELECT client_id FROM Client WHERE num_phone = :phone";
                    OracleCommand cmdCheck = new OracleCommand(queryCheck, conn);
                    cmdCheck.Parameters.Add(new OracleParameter("phone", reservation.Telephone));

                    object result = cmdCheck.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }

                // Client doesn't exist, so create a new one
                string query = @"INSERT INTO Client 
                              (client_id, full_name, num_phone, email) 
                              VALUES 
                              (seq_client.nextval, :name, :phone, :email) 
                              RETURNING client_id INTO :client_id";

                OracleCommand cmd = new OracleCommand(query, conn);
                cmd.Parameters.Add(new OracleParameter("name", reservation.Nom));
                cmd.Parameters.Add(new OracleParameter("phone", reservation.Telephone));

                // Handle null email
                if (string.IsNullOrEmpty(reservation.Email))
                {
                    cmd.Parameters.Add(new OracleParameter("email", DBNull.Value));
                }
                else
                {
                    cmd.Parameters.Add(new OracleParameter("email", reservation.Email));
                }

                // Parameter for the returned client_id
                OracleParameter clientIdParam = new OracleParameter("client_id", OracleDbType.Int32);
                clientIdParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(clientIdParam);

                cmd.ExecuteNonQuery();

                return Convert.ToInt32(clientIdParam.Value.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReservationService: Error ensuring client exists: {ex.Message}");
                throw new Exception($"Failed to create or find client: {ex.Message}", ex);
            }
        }

        public void AjouterReservation(Reservation reservation)
        {
            try
            {
                Debug.WriteLine($"ReservationService: Adding reservation for {reservation.Nom}");

                // First add to database
                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    // First ensure the client exists
                    int clientId = EnsureClientExists(conn, reservation);

                    // Now insert the reservation
                    string query = @"INSERT INTO Reservation 
                                   (reservation_id, reservation_datetime, nbr_personnes, choix_item, client_id, table_id)
                                   VALUES 
                                   (seq_reservation.nextval, :datetime, :nbr_personnes, :choix_item, :client_id, :table_id)
                                   RETURNING reservation_id INTO :reservation_id";

                    OracleCommand cmd = new OracleCommand(query, conn);
                    cmd.Parameters.Add(new OracleParameter("datetime", OracleDbType.TimeStamp, reservation.Date, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("nbr_personnes", OracleDbType.Int32, reservation.nbr_Personnes, ParameterDirection.Input));

                    // Handle null choix_item
                    if (string.IsNullOrEmpty(reservation.Choix))
                    {
                        cmd.Parameters.Add(new OracleParameter("choix_item", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input));
                    }
                    else
                    {
                        cmd.Parameters.Add(new OracleParameter("choix_item", OracleDbType.Varchar2, reservation.Choix, ParameterDirection.Input));
                    }

                    cmd.Parameters.Add(new OracleParameter("client_id", OracleDbType.Int32, clientId, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("table_id", OracleDbType.Int32, reservation.TableId, ParameterDirection.Input));

                    // Parameter for the returned reservation_id
                    OracleParameter reservationIdParam = new OracleParameter("reservation_id", OracleDbType.Int32);
                    reservationIdParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(reservationIdParam);

                    cmd.ExecuteNonQuery();

                    // Get the newly created reservation_id
                    reservation.reservation_Id = Convert.ToInt32(reservationIdParam.Value.ToString());
                    reservation.ClientID = clientId;

                    Debug.WriteLine($"ReservationService: Added reservation with ID {reservation.reservation_Id}");
                }

                // Now add to the local collection
                Reservations.Add(reservation);

                Debug.WriteLine("ReservationService: Reservation added successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReservationService: Error adding reservation: {ex.Message}");
                Debug.WriteLine($"ReservationService: Stack trace: {ex.StackTrace}");

                // Re-throw for UI handling
                throw new Exception($"Failed to add reservation: {ex.Message}", ex);
            }
        }

        public void SupprimerReservation(int reservationId)
        {
            try
            {
                Debug.WriteLine($"ReservationService: Deleting reservation with ID {reservationId}");

                // First delete from database
                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM Reservation WHERE reservation_id = :id";
                    OracleCommand cmd = new OracleCommand(query, conn);
                    cmd.Parameters.Add(new OracleParameter("id", OracleDbType.Int32, reservationId, ParameterDirection.Input));

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception($"No reservation found with ID {reservationId}");
                    }

                    Debug.WriteLine($"ReservationService: Successfully deleted reservation from database");
                }

                // Then remove from local collection
                Reservation reservationToRemove = Reservations.FirstOrDefault(r => r.reservation_Id == reservationId);
                if (reservationToRemove != null)
                {
                    Reservations.Remove(reservationToRemove);
                    Debug.WriteLine($"ReservationService: Successfully removed reservation from local collection");
                }
                else
                {
                    Debug.WriteLine($"ReservationService: Warning - Reservation with ID {reservationId} not found in local collection");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReservationService: Error deleting reservation: {ex.Message}");
                Debug.WriteLine($"ReservationService: Stack trace: {ex.StackTrace}");

                // Re-throw for UI handling
                throw new Exception($"Failed to delete reservation: {ex.Message}", ex);
            }
        }

        // Method to modify an existing reservation
        public void ModifierReservation(Reservation reservation)
        {
            try
            {
                Debug.WriteLine($"ReservationService: Modifying reservation with ID {reservation.reservation_Id}");

                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    // First, update client information
                    string clientQuery = @"UPDATE Client 
                                         SET full_name = :name, 
                                             num_phone = :phone, 
                                             email = :email 
                                         WHERE client_id = :client_id";

                    OracleCommand clientCmd = new OracleCommand(clientQuery, conn);
                    clientCmd.Parameters.Add(new OracleParameter("name", reservation.Nom));
                    clientCmd.Parameters.Add(new OracleParameter("phone", reservation.Telephone));

                    if (string.IsNullOrEmpty(reservation.Email))
                    {
                        clientCmd.Parameters.Add(new OracleParameter("email", DBNull.Value));
                    }
                    else
                    {
                        clientCmd.Parameters.Add(new OracleParameter("email", reservation.Email));
                    }

                    clientCmd.Parameters.Add(new OracleParameter("client_id", reservation.ClientID));

                    clientCmd.ExecuteNonQuery();

                    // Then update reservation details
                    string reservationQuery = @"UPDATE Reservation 
                                              SET reservation_datetime = :datetime, 
                                                  nbr_personnes = :nbr_personnes, 
                                                  choix_item = :choix_item,
                                                  table_id = :table_id
                                              WHERE reservation_id = :reservation_id";

                    OracleCommand reservationCmd = new OracleCommand(reservationQuery, conn);
                    reservationCmd.Parameters.Add(new OracleParameter("datetime", OracleDbType.TimeStamp, reservation.Date, ParameterDirection.Input));
                    reservationCmd.Parameters.Add(new OracleParameter("nbr_personnes", OracleDbType.Int32, reservation.nbr_Personnes, ParameterDirection.Input));

                    if (string.IsNullOrEmpty(reservation.Choix))
                    {
                        reservationCmd.Parameters.Add(new OracleParameter("choix_item", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input));
                    }
                    else
                    {
                        reservationCmd.Parameters.Add(new OracleParameter("choix_item", OracleDbType.Varchar2, reservation.Choix, ParameterDirection.Input));
                    }

                    reservationCmd.Parameters.Add(new OracleParameter("table_id", OracleDbType.Int32, reservation.TableId, ParameterDirection.Input));
                    reservationCmd.Parameters.Add(new OracleParameter("reservation_id", OracleDbType.Int32, reservation.reservation_Id, ParameterDirection.Input));

                    int rowsAffected = reservationCmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception($"No reservation found with ID {reservation.reservation_Id}");
                    }

                    Debug.WriteLine($"ReservationService: Successfully updated reservation in database");
                }

                // Update the local collection by finding and replacing the reservation
                int index = -1;
                for (int i = 0; i < Reservations.Count; i++)
                {
                    if (Reservations[i].reservation_Id == reservation.reservation_Id)
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0)
                {
                    Reservations[index] = reservation;
                    Debug.WriteLine($"ReservationService: Successfully updated reservation in local collection");
                }
                else
                {
                    Debug.WriteLine($"ReservationService: Warning - Reservation with ID {reservation.reservation_Id} not found in local collection");
                    // Add it to the collection if it doesn't exist
                    Reservations.Add(reservation);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReservationService: Error modifying reservation: {ex.Message}");
                Debug.WriteLine($"ReservationService: Stack trace: {ex.StackTrace}");

                // Re-throw for UI handling
                throw new Exception($"Failed to modify reservation: {ex.Message}", ex);
            }
        }
    }
}