using SQLite;
using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DB.Models
{
    [Table("ShoppingList_V2")]
    public class ShoppingList
    { 
        public int ShoppingListId { get; set; } 

        public string ShoppingListGUID { get; set; } 

        public string Text { get; set; } 

        public bool IsDone { get; set; } 

        public string UserName { get; set; } 
    }
}
