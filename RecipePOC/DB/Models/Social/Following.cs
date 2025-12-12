using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DB.Models.Social
{
    [Table("Following")]
    public class Following
    {
        public int FollowingId { get; set; } 

        public string PersonFollowingGuid { get; set; } 

        public string UserGuid { get; set;  }
    }
}
