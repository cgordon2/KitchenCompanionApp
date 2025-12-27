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
    public ObservableCollection<IngredientItem> Items { get; set; }
    private IRecipeService _recipeService;
    private IHttpClientFactory _httpClientFactory;
    private INotificationService _notifService;

    public CreateRecipe(IRecipeService recipeService, IHttpClientFactory httpClientFactory)
    {
        InitializeComponent(); 

        _recipeService = recipeService;
        _httpClientFactory = httpClientFactory;
        _notifService = MauiProgram.Services.GetService<INotificationService>();

        Items = new ObservableCollection<IngredientItem>();

        BindingContext = this; // important!
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var serializedRecipe = await SecureStorage.GetAsync("selected_recipe"); 
        var serializedIngredients = await SecureStorage.GetAsync("selected_ingredients");
        var selectedItems = new List<IngredientItem>();
        var recipeIngredients = new List<RecipeAndRiDTO>();
        Recipe unserializedRecipe = null; 

        if (serializedRecipe != null)
        {
            SecureStorage.Default.Remove("selected_recipe");
            unserializedRecipe = JsonSerializer.Deserialize<Recipe>(serializedRecipe);
        }

        Items.Clear(); 

        if (!string.IsNullOrEmpty(serializedIngredients))
        {
            selectedItems = JsonSerializer.Deserialize<List<IngredientItem>>(serializedIngredients);
        }


        var selectedList = new List<IngredientItem>();

        foreach (var item in selectedItems)
        {
            var test = new IngredientItem();

            test.Stars = item.Stars;
            test.Name = item.Name;
            test.StoreURL = item.StoreURL;
            test.StoreName = item.StoreName;
            test.CreatedBy = item.CreatedBy;

            Items.Add(test);
        }

        SelectedListview.ItemsSource = Items; 
    }

    private async void OnSelectExistingIngredientTapped(object sender, EventArgs e)
    {
        var username = await SecureStorage.GetAsync("user_name"); 

        await Navigation.PushAsync(new IngredientsListView(_recipeService, username, true, _httpClientFactory)); 
    }

    private async void CreateRecipeDB(object sender, EventArgs e)
    {
        var recipeTitle = IngredientNameEntry.Text;
        var recipeDescription = QuantityEntry.Text;

        var prepTime = PrepEntry.Text;
        var cookTime = CookEntry.Text;
        var servesTime = ServesEntry.Text; 

        var realName = await SecureStorage.GetAsync("user_name");
        var email = await SecureStorage.GetAsync("email"); //chef email or user email 
        var category = CategoryPicker2.SelectedItem; 

        if (category != null)
        {
            category = (string)category; 
        }

        var serializedIngredients = await SecureStorage.GetAsync("selected_ingredients");
        var selectedItems = new List<IngredientItem>();
        var recipeIngredients = new List<RecipeAndRiDTO>(); 

        if (!string.IsNullOrEmpty(serializedIngredients))
        {

            selectedItems = JsonSerializer.Deserialize<List<IngredientItem>>(serializedIngredients); 
        }

        foreach (var item in selectedItems)
        {
            var test = new RecipeAndRiDTO();

            test.IngredientId = Convert.ToInt32(item.IngredientGUID);
            test.Quantity = 1;
            test.UnitId = 1;
            test.RecipeId = 0;
            test.ingredientName = item.Name;
            test.storeName = "walmart";
            test.storeUrl = "https://walmart.com";
            test.unitName = "cups"; 

            recipeIngredients.Add(test);
        }

        var recipeDto = new RecipeDto();

        recipeDto.RecipeName = recipeTitle; 
        recipeDto.Description = recipeDescription;
        recipeDto.ChefName = realName;
        recipeDto.ChefEmail = email; 
        recipeDto.Category = "Seafood";
        recipeDto.Favorite = "Yes";
        recipeDto.RecipeIngredients = recipeIngredients;
        recipeDto.Photo = "food.jpg";
        recipeDto.CookTime = Convert.ToInt32(cookTime);
        recipeDto.Serves = Convert.ToInt32(servesTime); /** We dont need to add to DB, will be reset **/
        recipeDto.Stars = 5; 

        await APIClient.CreateRecipe(_httpClientFactory, recipeDto); 

        var allRecipes = await APIClient.GetAllRecipes(_httpClientFactory);
        await _recipeService.ResetRecipes(allRecipes);

        await Navigation.PushAsync(new Search(_recipeService, _notifService, _httpClientFactory)); 

    } 
}
public class TodoItem
{
    public string Name { get; set; }
    public bool IsChecked { get; set; }
}