using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DTOs
{
    public class RecipeAndRiDTO
    {
        public int RecipeId { get; set; } 

        public int IngredientId { get; set; } 

        public int Quantity { get; set; } 

        public int UnitId {  get; set; }

        public string ingredientName { get; set; } = string.Empty;
        public string storeName { get; set; } = string.Empty;
        public string storeUrl { get; set; } = string.Empty;

        public string unitName { get; set; } = string.Empty; 
    }
}
