using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DB.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("UserFavoriteGuid")]
    public class UserFavoriteGuid
    {
        public int UserFavoriteId { get; set; } 
        public string UserName { get; set; } 
        public string RecipeGUID { get; set; } 
    }
}
