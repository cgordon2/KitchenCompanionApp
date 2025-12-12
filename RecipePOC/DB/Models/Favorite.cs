using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DB.Models
{
    [Table("Favorite")]
    public class Favorite
    {
        public int Id { get; set; }

        public string IsFavorite {  get; set; }

        public int RecipeId { get; set; }
    }
}
