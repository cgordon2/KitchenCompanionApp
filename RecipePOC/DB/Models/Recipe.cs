using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DB.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Recipe")]
    public class Recipe
    {
        [PrimaryKey, AutoIncrement]
        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string CreatedBy { get; set; } 
        public string Description { get; set; }
        public string ChefName { get; set; }
        public string ChefEmail { get; set; }
        public string Category { get; set; }
        public string Favorite { get; set; } 
        public string  CreatedAt { get; set; } 
        public string RecipeGuid { get; set; } 
    }
}
