using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DB.Models.Social
{
    [Table("Followers")]
    public class Followers
    {
        public int FollowerId { get; set; }

        public string PersonToFollowGuid { get; set; }

        public string UserGuid { get; set; }
    }
}
