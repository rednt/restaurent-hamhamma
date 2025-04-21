using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace restaurent_hamhamma.Models
{
    public class Admin : User
    {
        public string Username { get; set; }
        public string Password { get; set; }

        
        public bool Login(string username, string password)
        {
            
            return Username == username && Password == password;
        }

        public void Logout()
        {
            
        }

        public void AddMenuItem(string name, float prix, bool disponible)
        {
            
        }

        public void UpdateMenuItem(int itemId, float prix, bool disponible, string description)
        {
           
        }

        public void RemoveMenuItem(int itemId)
        {
            
        }

        // Navigation properties
        public ICollection<MenuItem> MenuItems { get; set; }
    }
}
