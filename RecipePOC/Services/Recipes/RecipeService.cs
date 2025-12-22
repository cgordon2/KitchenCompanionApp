using RecipePOC.DB;
using RecipePOC.DB.Models;
using RecipePOC.DTOs;
using RecipePOC.Services.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.Services.Recipes
{
    public class RecipeService : IRecipeService
    {
        private readonly SQLiteAsyncConnection _connection;
        private readonly IHttpClientFactory _httpClientFactory;

        public RecipeService(IHttpClientFactory httpClientFactory)
        {
            _connection = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);
            _httpClientFactory = httpClientFactory;
        }

        public async Task InsertUser(UserDTO user)
        {
            var foundUser = await _connection.Table<User>().Where(r => r.UserName == user.UserName).FirstOrDefaultAsync(); 

            if (foundUser == null)
            {
                var modelUser = new User();

                modelUser.UserId = user.UserId; 
                modelUser.UserName = user.UserName; 

                if (user.Email != null && user.Email != string.Empty)
                {
                    modelUser.Email = user.Email;
                }

                await _connection.InsertAsync(modelUser);
            }
        }

        public async Task ResetRecipes(List<RecipeDto> recipeDtos)
        {
            recipeDtos ??= new List<RecipeDto>();

            var entities = recipeDtos.Select(dto => new DB.Models.Recipe
            { 
                RecipeName = dto.RecipeName, 
                Description = dto.Description,
                ChefName = dto.ChefName,
                ChefEmail = dto.ChefEmail,
                Category = dto.Category,
                Favorite = dto.Favorite,
            }).ToList();

            await _connection.RunInTransactionAsync(tran =>
            { 
                tran.Execute("DELETE FROM Recipe"); 
                tran.InsertAll(entities);
            });
        }

        public async Task<List<IngredientDto>> GetIngredientsFresh()
        {
            var ingredients = await APIClient.GetIngredients(_httpClientFactory); 

            return ingredients; 
        }

        public async Task ResetIngredients(List<IngredientDto> ingredientDtos)
        {
            ingredientDtos ??= new List<IngredientDto>();

            var entities = ingredientDtos.Select(dto => new DB.Models.Ingredient
            { 
                IngredientName = dto.IngredientName, 
                IngredientGUID = dto.IngredientGUID, 
                CreatedBy = dto.CreatedBy, 
            }).ToList();

            await _connection.RunInTransactionAsync(tran =>
            {
                tran.Execute("DELETE FROM Ingredient");
                tran.InsertAll(entities);
            });
        }

        public async Task<List<DB.Models.Ingredient>> SearchIngredients(string query, int page, int size)
        { 
            return await _connection.Table<DB.Models.Ingredient>()
                .Where(r => r.IngredientName.ToLower().Contains(query.ToLower())).Skip(page * size).Take(size)
                .ToListAsync(); 
        }

        public async Task<List<DB.Models.Recipe>> GetTaggedRecipes(string username)
        {
            var favorites = await _connection.Table<DB.Models.UserFavoriteGuid>().ToListAsync();
            var recipes = await _connection.Table<DB.Models.Recipe>().ToListAsync();

            var result = (from f in favorites
                          join r in recipes
                          on f.RecipeGUID equals r.RecipeGuid
                          where f.UserName == username
                          select r).ToList();   // return ONLY recipes

            return result;
        }

        public async Task<List<DB.Models.Recipe>> GetRecipes(bool all, bool yours, bool recent, bool favs, string username, int page, int size)
        { 
            var query = _connection.Table<DB.Models.Recipe>();

            // If NOT "all", then apply filters
            if (!all)
            {
                if (yours)
                {
                    query = query.Where(r => r.ChefEmail == username).Skip(page * size).Take(size); 
                    return await query.ToListAsync(); 
                }

                if (recent)
                {
                } 

                if (favs)
                {
                    return await query.Where(r => r.ChefEmail == username).Where(r => r.Favorite == "Yes").Skip(page * size).Take(size).ToListAsync(); 
                }
            }

            return await query.Skip(page * size).Take(size).ToListAsync();
        }

        public async Task<List<DB.Models.Recipe>> SearchRecipes(string query, bool all, bool yours, bool recent, string username)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<DB.Models.Recipe>();

            if (all)
            {
                // Case-insensitive search
                return await _connection.Table<DB.Models.Recipe>()
                    .Where(r => r.RecipeName.ToLower().Contains(query.ToLower()))
                    .ToListAsync();
            } 

            if (yours)
            {
                return await _connection.Table<DB.Models.Recipe>().Where(r => r.ChefEmail == username).Where(r => r.RecipeName.ToLower().Contains(query.ToLower())).ToListAsync(); 
            }

            if (recent)
            {
                var favorites = await _connection.Table<DB.Models.UserFavoriteGuid>().ToListAsync();
                var recipes = await _connection.Table<DB.Models.Recipe>().ToListAsync();

                var result = (from f in favorites
                              join r in recipes
                              on f.RecipeGUID equals r.RecipeGuid
                              where f.UserName == username
                              where r.RecipeName.ToLower().Contains(query.ToLower())
                              select r).ToList();   // return ONLY recipes

                return result;
            }

            return new List<DB.Models.Recipe>(); 
        }


        public async Task<List<DB.Models.Ingredient>> GetIngredients(int page, int size)
        {
            //await _connection.ExecuteAsync("ALTER TABLE Notification ADD COLUMN NotifGUID TEXT");
           // await _connection.ExecuteAsync("ALTER TABLE Ingredient ADD COLUMN CookTime TEXT");
          //  await _connection.ExecuteAsync("ALTER TABLE Ingredient ADD COLUMN Serves TEXT");
            var ingredients = await _connection.Table<DB.Models.Ingredient>()
                .Skip(page * size).Take(size).ToListAsync(); 

            return ingredients;
        }

        public class RecipeIngredientFull
        {
            public int RecipeId { get; set; }
            public int IngredientId { get; set; }
            public int Quantity { get; set; }
            public int UnitId { get; set; }

            public string IngredientName { get; set; }
            public string StoreName { get; set; }
            public string StoreUrl { get; set; }
            public string UnitName { get; set; }

            public int Stars { get; set; } 

            public int PrepTime { get; set; } 

            public int CookTime { get; set; } 

            public int Serves { get; set; } 
        }

        /**
         * For detail page
         * **/
        public async Task<List<RecipeIngredientFull>> GetRecipeIngredients(int recipeId)
        {
            string sql = "    ";
            var ingredients = await _connection.QueryAsync<RecipeIngredientFull>(@"SELECT 
                                                                                    ri.*,
                                                                                    i.IngredientName,
                                                                                    r.RecipeName, 
	                                                                                r.Description, 
	                                                                                i.Stars, 
	                                                                                i.PrepTime, 
	                                                                                i.CookTime, 
	                                                                                i.Serves
                                                                                FROM RecipeIngredient ri    
                                                                                INNER JOIN Ingredient i ON ri.IngredientId = i.IngredientId  
                                                                                INNER JOIN Recipe r ON ri.RecipeId = r.RecipeId
                                                                                WHERE ri.RecipeId = ?", recipeId);

            return ingredients;
        }

        public async Task AddShoppingList(ShoppingListDTO dto)
        {
        }

        public async Task AddIngredient(IngredientDto dto)
        {
            var ingredient = new DB.Models.Ingredient(); 

            ingredient.IngredientName = dto.IngredientName;
            ingredient.UnitName = dto.UnitName;
            ingredient.StoreName = dto.StoreName;
            ingredient.StoreUrl = dto.StoreUrl;
            ingredient.CreatedBy = dto.CreatedBy;
            ingredient.Photo = dto.Photo;
            ingredient.Stars = dto.Stars;

            ingredient.PrepTime = dto.PrepTime; 
            ingredient.CookTime = dto.CookTime;
            ingredient.Serves = dto.Serves;

            await _connection.InsertAsync(ingredient); 
        }

        public async Task AddRecipe(List<IngredientItem> ingredientDtos, RecipeDto recipeDto)
        {  
            // Insert Ingredient
            /*var ingredientDB = new DB.Models.Ingredient
            {
                IngredientName = "Salmon",
                UnitName = "kg",
                StoreName = "Walmart",
                StoreUrl = "https://www.walmart.com"
            };

            await _connection.InsertAsync(ingredientDB);
            int ingredientId = ingredientDB.IngredientId;**/

            // Insert Recipe
            /*var recipeDb = new DB.Models.Recipe
            {
                RecipeId = 1200,
                RecipeName = "Grilled Salmon",
                Description = "Delicious grilled salmon with lemon butter sauce.",
                ChefName = "Cameron Gordon",
                ChefEmail = "chef@example.com",
                Category = "Seafood",
                Favorite = "YES"
            };

            await _connection.InsertAsync(recipeDb);
            int recipeId = recipeDb.RecipeId;

            // Link Ingredient ↔ Recipe
            var recipeIngredient = new DB.Models.RecipeIngredient
            {
                IngredientId = ingredientId,
                RecipeId = recipeId,
                Quantity = 1,
                UnitId = 1,
                IngredientName = "Salmon",
                StoreName = "Walmart",
                StoreUrl = "https://www.walmart.com",
                UnitName = "kg"
            };

            await _connection.InsertAsync(recipeIngredient);

            int recipeIngredientId = recipeIngredient.IngredientId;

            System.Diagnostics.Debug.WriteLine(
                $"Inserted Recipe #{recipeId}, Ingredient #{ingredientId}, RecipeIngredient #{recipeIngredientId}");**/
        }
    }
}
