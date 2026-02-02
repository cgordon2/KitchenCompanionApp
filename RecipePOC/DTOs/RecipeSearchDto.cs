using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DTOs
{
    public class RecipeSearchDto
    {
        public string AllWords { get; set; }
        public string ExactPhrase { get; set; }
        public string NoneWords { get; set; }
        public int loggedInUserGuid { get; set; }
        public bool SearchOnlyUser { get; set; }
        public int? Stars { get; set; }
    }
}
