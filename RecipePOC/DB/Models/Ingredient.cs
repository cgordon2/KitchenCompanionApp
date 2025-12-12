
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DB.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Ingredient")]
    public class Ingredient
    {
        [AutoIncrement, PrimaryKey]
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string UnitName { get; set; }
        public string StoreName { get; set; }
        public string StoreUrl { get; set; }
        public string CreatedBy { get; set; } 
        public string Photo { get; set; } 
        public string Stars { get; set; } 
        public string PrepTime { get; set; } 
        public string CookTime { get; set; } 
        public string Serves { get; set; } 
        public string IngredientGUID { get; set; } 
    }
}
