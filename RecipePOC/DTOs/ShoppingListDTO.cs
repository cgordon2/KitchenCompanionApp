using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DTOs
{
    public class ShoppingListDTO
    {
        public int ChefId { get; set; }

        public string Item { get; set; } 

        public string Store { get; set; } 

        public string RequestedBy { get; set; } 

        public DateTime DateCreated { get; set; } 
    }
}
