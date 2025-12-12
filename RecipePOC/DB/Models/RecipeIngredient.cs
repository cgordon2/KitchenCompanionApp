using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DB.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("RecipeIngredient")]
    public class RecipeIngredient
    {
        public int RecipeId { get; set; } 
        public int IngredientId { get; set; }
        public int Quantity { get; set; }
        public int? UnitId { get; set; }
        public string IngredientName { get; set; }
        public string StoreName { get; set; }
        public string StoreUrl { get; set; }
        public string? UnitName { get; set; }
    }
}
