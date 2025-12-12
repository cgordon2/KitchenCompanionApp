using RecipePOC.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.Services
{
    public static class AppConstants
    {
        public const string HttpClientName = "login-flow-http-client";
        public const string AuthStorageKeyName = "login-flow-auth-key";

        public static RecipeDB Database { get; } = new RecipeDB();


    }
}
