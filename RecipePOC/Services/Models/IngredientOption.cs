using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.Services.Models
{
    public class IngredientOption
    {
       public string Name { get; set; }
        public string IngredientGuid { get; set; }
        public int Quantity { get; set; } 
        public string Unit { get; set; } 
    }
}
