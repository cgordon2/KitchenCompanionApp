using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DTOs
{
    public class UserDTO
    {
        public int UserId { get; set; } 
        public string UserName { get; set; } = string.Empty; 
        public string? Password { get; set; } 
        public string? ConfirmPassword { get; set; } 
        public string? Email { get; set; } 

        public bool IsSetup { get; set; } 

        public int? ChefId { get; set; } 

        public string? RealName { get; set; }
        public string? ShortBio { get; set; } 
        public string? Location { get; set; } 

        public string? Language { get; set; } 
    }
}
