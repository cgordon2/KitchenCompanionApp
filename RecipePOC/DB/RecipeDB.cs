
using NuGet.Common;
using RecipePOC.DB.Models;
using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public async Task AddShoppingListItemAsync(string item, string store, string requestedBy)
        {
            var entry = new DB.Models.ShoppingList
            {
                Item = item,
                Store = store, 
                RequestedBy = requestedBy, 
                DateCreated = DateTime.Now
            };

            await _connection.InsertAsync(entry);
        }

        public async Task<bool> DeleteShoppingListItem(int listId)
        {
            var item = await _connection.Table<DB.Models.ShoppingList>()
                        .Where(s => s.ShoppingListId == listId)
                        .FirstOrDefaultAsync();

            if (item != null)
            {
                await _connection.DeleteAsync(item);

                return true; 
            }

            return false; 
        } 

        public async Task<List<DB.Models.RecipeIngredient>> GetRecipeIngredients(int recipeId)
        {
            var recipeIngredients = await _connection.Table<Models.RecipeIngredient>().Where(r => r.RecipeId == recipeId).ToListAsync();

            return recipeIngredients; 
        }

        public async Task<List<DB.Models.ShoppingList>> GetShoppingList(int chefId)
        {
            var shoppingList = await _connection.Table<Models.ShoppingList>().Where(r => r.ChefId == chefId).ToListAsync();

            return shoppingList; 
        }

        public static async Task<RecipeDB> CreateUserFavorite()
        {
            var instance = new RecipeDB();

            await instance._connection.CreateTableAsync<DB.Models.Social.Followers>();
            await instance._connection.CreateTableAsync<DB.Models.Social.Following>();


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
