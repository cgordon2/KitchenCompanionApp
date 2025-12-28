using RecipePOC.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC
{
    public class Recipe
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ExtraInfo { get; set; }
        public string UserName { get; set; } 
        public string Photo { get; set; } 
        public int Stars { get; set; } 
        public string Favorite { get; set; } 
        public int CookTime { get; set; } 
        public int Serves { get; set; } 
        public int Prep { get; set; } 
        public int Index { get; set; }   // for alternating styles
        public string RecipeGUID { get; set; } 
        public List<RecipeAndRiDTO> RecipeIngredients { get; set; } 
    }
}
