using System;
using System.Net.Http.Headers;
using System.Net.Http; 
using SQLite;
using RecipePOC.DB;
using RecipePOC.Services;

namespace RecipePOC
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell(); 
        } 
    }


}
