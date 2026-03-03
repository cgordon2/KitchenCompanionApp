using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DTOs
{
    public class PantryDto
    {
        public int PantryId { get; set; }

        public string? IngredientName { get; set; }

        public int? Quantity { get; set; }

        public string? Unit { get; set; }

        public int? EditedQuantity { get; set; }
        public string? IngredientGUID { get; set; } 
        public string? UserName { get; set; } 

    }
}
