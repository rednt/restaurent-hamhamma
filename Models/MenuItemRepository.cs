using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace restaurent_hamhamma.Models { 
public static class MenuItemRepository
{
    public static ObservableCollection<MenuItemModel> Items { get; set; } = new ObservableCollection<MenuItemModel>();
}

}