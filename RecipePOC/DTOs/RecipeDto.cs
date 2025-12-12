using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DTOs
{
    public class RecipeDto
    {
        public string RecipeName { get; set; } 
        public string RecipeDescription { get; set; } 
        public string ChefName { get; set; } 
        public string ChefEmail { get; set; } 
        public string Category { get; set; } 
        public string Favorite { get; set; } 
    }
}
