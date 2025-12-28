using RecipePOC.DTOs;
using RecipePOC.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RecipePOC.Services.Recipes.RecipeService;

namespace RecipePOC.Services.Recipes
{
    public interface IRecipeService
    { 
        Task AddRecipe(List<IngredientItem> ingredientDtos, RecipeDto recipeDto);
        Task AddIngredient(IngredientDto dto); 
        Task AddShoppingList(ShoppingListDTO dto);
        Task<List<RecipeIngredientFull>> GetRecipeIngredients(int recipeId);
        Task<List<DB.Models.Ingredient>> SearchIngredients(string query, int page, int size); 
        Task<List<DB.Models.Recipe>> GetRecipes(bool all, bool yours, bool recent, bool favs, string username, int page, int size);
        Task<List<DB.Models.Recipe>> GetTaggedRecipes(string username); 
        Task<List<DB.Models.Recipe>> SearchRecipes(string query, bool all, bool yours, bool recent, string username);
        Task<List<DB.Models.Ingredient>> GetIngredients(int page, int size);

        Task<List<IngredientDto>> GetIngredientsFresh();
        Task<List<ShoppingListDTO>> GetShoppingListFresh(string username);
        Task<List<DB.Models.ShoppingList>> GetShoppingListFromDB(string username); 

        Task ResetRecipes(List<RecipeDto> recipeDtos);
        Task ResetIngredients(List<IngredientDto> ingredients);
        Task ResetShoppingList(List<ShoppingListDTO> shoppingListDtos); 

        Task MarkShoppingListComplete(int shoppingListItemId);
        Task DeleteShoppingListItem(int shoppingListItemId); 


        Task InsertUser(UserDTO user); 

        // get favorites 
        // get recipes 
        // search 
        // shopping list 
        // ingredients
    }
}
