using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DB.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("User")]
    public class User
    {
        public int UserId { get; set; }
        public string BearerToken { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public int FollowersCount { get; set; }

        public int FollowingCount { get; set; }

        public string ShortBio { get; set; }

        public int TotalRecipes {  get; set; }

        public string Language { get; set; } 

        public string Created { get; set; } 

        public string Location { get; set; }

        public bool DisplayNotifications { get; set; } 

        public string AvatarUrl { get; set; } 

        public string real_name { get; set; } 
    }
}
