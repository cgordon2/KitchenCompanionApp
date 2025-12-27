using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DTOs
{
    public class ShoppingListDTO
    {
        public string ShoppingListIdGuid { get; set; } // maps to int on post for MC and delete 
        public string Text { get; set; } 

        public string Category { get; set; } 

        public bool IsDone { get; set; } 

        public string UserName { get; set; } 
    }
}
