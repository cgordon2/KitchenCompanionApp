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
        public int Index { get; set; }   // for alternating styles
        public string RecipeGUID; 
    }
}
