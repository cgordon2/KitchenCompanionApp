using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DB.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("ShoppingList")]
    public class ShoppingList
    {
        public int ShoppingListId { get; set; }
        public int ChefId { get; set; }

        public string Item { get; set; }

        public string Store { get; set; }
        public string RequestedBy { get; set; }

        public DateTime DateCreated { get; set; }

    }
}
