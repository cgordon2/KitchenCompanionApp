using SQLite;
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
        [PrimaryKey]
        public int UserId { get; set; }
        public string BearerToken { get; set; } = string.Empty; 

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int FollowersCount { get; set; }

        public int FollowingCount { get; set; }

        public string ShortBio { get; set; } = string.Empty;

        public int TotalRecipes {  get; set; }

        public string Language { get; set; } = string.Empty;

        public string Created { get; set; } = string.Empty; 

        public string Location { get; set; } = string.Empty;

        public bool DisplayNotifications { get; set; }

        public string AvatarUrl { get; set; } = string.Empty;

        public string real_name { get; set; } = string.Empty;

        [Column("is_setup")]
        public bool IsSetup { get; set; }  // is_setup
    }
}
