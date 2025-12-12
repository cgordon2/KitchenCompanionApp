using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DTOs
{
    public class NotificationDTO
    {
        public int NotifId { get; set; } 

        public int UserId { get; set; } 

        public string Message { get; set; } 

        public bool IsRead { get; set; } 

        public DateTime Created { get; set; } 
    }
}
