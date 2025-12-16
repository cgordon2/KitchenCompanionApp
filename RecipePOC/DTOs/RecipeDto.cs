using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RecipePOC.DTOs
{
    public class RecipeDto
    {
        [JsonPropertyName("recipeName")]
        public string RecipeName { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; } 
        public string ChefName { get; set; }
        [JsonPropertyName("chefEmail")]
        public string ChefEmail { get; set; } 
        public string Category { get; set; } 
        public string Favorite { get; set; } 
    }
}
