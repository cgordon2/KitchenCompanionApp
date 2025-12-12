using RecipePOC.Services.Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.Services
{
    public class PageHelpers
    {

        public static bool HasInternet()
        {

            bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
            return hasInternet;
        }
    }
}
