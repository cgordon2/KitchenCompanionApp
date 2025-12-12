using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace RecipePOC.DB.Models
{
    [Table("Notification")]
    public class Notification
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Message { get; set; }

        public bool IsRead { get; set; }

        public int ChefId { get; set; }
        public string ChefGUID { get; set; }
        public string NotifGUID { get; set; } 
    }
}
