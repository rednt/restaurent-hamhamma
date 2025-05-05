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

        // Navigation properties
        public ICollection<MenuItemModel> MenuItems { get; set; }
    }
}
