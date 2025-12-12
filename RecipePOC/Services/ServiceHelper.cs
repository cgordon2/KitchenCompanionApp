using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.Services
{
    /*
     * var auth = ServiceHelper.GetService<IAuthService>();
await auth.Logout();
     * **/
    public static class ServiceHelper
    {
        public static T GetService<T>() => Ioc.Default.GetService<T>();
    }
}
