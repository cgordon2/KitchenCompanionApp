
using NuGet.Common;
using RecipePOC.DB.Models;
using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks; 

namespace RecipePOC.DB
{
    public class RecipeDB
    {
        private readonly SQLiteAsyncConnection _connection; 

        public RecipeDB()
        {
            _connection = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);
        }   

        public async Task<List<DB.Models.RecipeIngredient>> GetRecipeIngredients(int recipeId)
        {
            var recipeIngredients = await _connection.Table<Models.RecipeIngredient>().Where(r => r.RecipeId == recipeId).ToListAsync();

            return recipeIngredients; 
        }
         

        public static async Task<RecipeDB> CreateUserFavorite()
        {
            var instance = new RecipeDB();

            await instance._connection.CreateTableAsync<DB.Models.Social.Followers>();
            await instance._connection.CreateTableAsync<DB.Models.Social.Following>();


            return instance; 
        } 

        public static async Task<RecipeDB> AddIsClone()
        {
            var _db = new RecipeDB();

            await _db._connection.ExecuteAsync("ALTER TABLE Recipe ADD COLUMN IsCloned INTEGER DEFAULT 0;");

            return _db;
        }

        public static async Task<RecipeDB> Prep()
        {
            var _db = new RecipeDB();

            await _db._connection.ExecuteAsync("ALTER TABLE Recipe ADD COLUMN Prep INTEGER"); 

            return _db;
        }

        public static async Task<RecipeDB> AddMore()
        {
            var _db = new RecipeDB();

            await _db._connection.ExecuteAsync("ALTER TABLE Recipe ADD COLUMN Photo TEXT");
            await _db._connection.ExecuteAsync("ALTER TABLE Recipe ADD COLUMN Stars INTEGER");
            await _db._connection.ExecuteAsync("ALTER TABLE Recipe ADD COLUMN CookTime INTEGER");
            await _db._connection.ExecuteAsync("ALTER TABLE Recipe ADD COLUMN Serves INTEGER");

            return _db; 
        }

        public static async Task<RecipeDB> CreateShoppingListNew()
        {
            var instance = new RecipeDB();

            await instance._connection.CreateTableAsync<DB.Models.ShoppingList>();

            return instance; 
        }

        public static async Task<RecipeDB> CreateAsync()
        {
            var instance = new RecipeDB();

            // Ensure all tables exist before use
            await instance._connection.CreateTableAsync<DB.Models.Recipe>();
            await instance._connection.CreateTableAsync<DB.Models.Notification>();
            await instance._connection.CreateTableAsync<DB.Models.ShoppingList>();
            await instance._connection.DropTableAsync<DB.Models.Favorite>();
            await instance._connection.CreateTableAsync<DB.Models.Favorite>();
            await instance._connection.CreateTableAsync<DB.Models.Ingredient>();
            await instance._connection.CreateTableAsync<DB.Models.RecipeIngredient>();
            await instance._connection.CreateTableAsync<DB.Models.User>(); 


            return instance;
        }
    }
} 
