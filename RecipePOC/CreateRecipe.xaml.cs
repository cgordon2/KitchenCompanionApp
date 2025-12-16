using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Models;
using RecipePOC.Services.Recipes;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecipePOC;

public partial class CreateRecipe : ContentPage
{
    public ObservableCollection<TodoItem> Items { get; set; }
    private IRecipeService _recipeService;
    private IHttpClientFactory _httpClientFactory; 

    public CreateRecipe(IRecipeService recipeService, IHttpClientFactory httpClientFactory)
    {
        InitializeComponent();

        Items = new ObservableCollection<TodoItem>
        {
            new TodoItem { Name="Eggs", IsChecked=false }, 
        };

        _recipeService = recipeService;
        _httpClientFactory = httpClientFactory;

        BindingContext = this; // important!
    }

    private async void OnSelectExistingIngredientTapped(object sender, TappedEventArgs e)
    {
        var username = await SecureStorage.GetAsync("user_name"); 

        await Navigation.PushAsync(new IngredientsListView(_recipeService, username, true, _httpClientFactory)); 
    }

    private async void CreateRecipeDB(object sender, EventArgs e)
    {
        var recipeTitle = IngredientNameEntry.Text;
        var recipeDescription = QuantityEntry.Text;

        var realName = await SecureStorage.GetAsync("real_name");
        var email = await SecureStorage.GetAsync("email"); //chef email or user email 
        var category = CategoryPicker2.SelectedItem; 

        if (category != null)
        {
            category = (string)category; 
        }

        var serializedIngredients = await SecureStorage.GetAsync("selected_ingredients");
        if (!string.IsNullOrEmpty(serializedIngredients))
        {
            var selectedItems = JsonSerializer.Deserialize<List<IngredientItem>>(serializedIngredients); 
        }

        var recipeDto = new RecipeDto();

        recipeDto.RecipeName = recipeTitle; 
        recipeDto.Description = recipeDescription;
        recipeDto.ChefName = realName;
        recipeDto.ChefEmail = email; 
        recipeDto.Category = "Seafood";
        recipeDto.Favorite = "Yes";

        await APIClient.CreateRecipe(_httpClientFactory, recipeDto); 

        var allRecipes = await APIClient.GetAllRecipes(_httpClientFactory);
        await _recipeService.ResetRecipes(allRecipes); 

    }
}
public class TodoItem
{
    public string Name { get; set; }
    public bool IsChecked { get; set; }
}