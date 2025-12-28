using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Models;
using RecipePOC.Services.Recipes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecipePOC;

public partial class CreateRecipe : ContentPage, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void RemoveItem_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button &&
            button.CommandParameter is IngredientItem item)
        {
            Items.Remove(item);
        }
    }

    public ObservableCollection<IngredientItem> Items { get; set; }
    public string RecipeGUID { get; set; } 

    private IRecipeService _recipeService;
    private IHttpClientFactory _httpClientFactory;
    private INotificationService _notifService;
    private string shouldCloneOrEdit = null;

    private string _addBtnText; 
    private string _cookTime;
    private string _prepTime;
    private string _serves;

    private string _recipeName;
    private string _recipeDirections;

    public string AddBtn
    {
        get => _addBtnText;
        set
        {
            _addBtnText = value;
            OnPropertyChanged();
        }
    }

    public string RecipeName
    {
        get => _recipeName;
        set
        {
            _recipeName = value;
            OnPropertyChanged();
        }
    }

    public string RecipeDirections
    {
        get => _recipeDirections;
        set
        {
            _recipeDirections = value;
            OnPropertyChanged();
        }
    }

    public string Prep
    {
        get => _prepTime;
        set
        {
            _prepTime = value;
            OnPropertyChanged();
        }
    }

    public string Serves
    {
        get => _serves;
        set
        {
            _serves = value;
            OnPropertyChanged();
        }
    }

    public string CookTime
    {
        get => _cookTime;
        set
        {
            _cookTime = value;
            OnPropertyChanged();
        }
    }

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
        /**
         * 
         * We need recipe ID for this **/ 
        base.OnAppearing();

        var serializedRecipe = await SecureStorage.GetAsync("selected_recipe");
        shouldCloneOrEdit = await SecureStorage.GetAsync("should_clone_or_edit"); 
        var serializedIngredients = await SecureStorage.GetAsync("selected_ingredients");
        var selectedItems = new List<IngredientItem>();
        var recipeIngredients = new List<RecipeAndRiDTO>();
        Recipe unserializedRecipe = null;
        Items.Clear();

        if (shouldCloneOrEdit == null)
        {
            AddBtn = "Add Recipe"; 

            if (serializedRecipe != null)
            {
                unserializedRecipe = JsonSerializer.Deserialize<Recipe>(serializedRecipe);
            }


            if (!string.IsNullOrEmpty(serializedIngredients))
            {
                selectedItems = JsonSerializer.Deserialize<List<IngredientItem>>(serializedIngredients);
            }

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
        else if (shouldCloneOrEdit == "clone")
        {
            AddBtn = "Clone Recipe";
            //... same as add? basically? clone the id to new recipe dont need tagged table

            if (serializedRecipe != null)
            {
                unserializedRecipe = JsonSerializer.Deserialize<Recipe>(serializedRecipe);
            }

            if (!string.IsNullOrEmpty(serializedIngredients))
            {
                selectedItems = JsonSerializer.Deserialize<List<IngredientItem>>(serializedIngredients);
            }

            RecipeGUID = unserializedRecipe.RecipeGUID;

            CookTime = Convert.ToString(unserializedRecipe.CookTime);
            Serves = Convert.ToString(unserializedRecipe.Serves);
            Prep = Convert.ToString(unserializedRecipe.Prep);
            RecipeName = unserializedRecipe.Title;
            RecipeDirections = unserializedRecipe.Description;

            var test = unserializedRecipe.RecipeIngredients;

            if (test.Count > 0)
            {
                foreach (var item in test)
                {
                    var ingredientGuid = item.IngredientId;
                    var storeName = item.storeName;
                    var storeUrl = item.storeUrl;

                    var ii = new IngredientItem();

                    ii.Name = item.ingredientName;
                    ii.StoreName = storeName;
                    ii.IngredientGUID = Convert.ToString(item.IngredientId);
                    ii.StoreURL = storeUrl;

                    Items.Add(ii);
                }
            }
            else
            {
                foreach (var item in selectedItems)
                {
                    var ii = new IngredientItem();

                    ii.Name = item.Name;
                    ii.StoreName = item.StoreName;
                    ii.IngredientGUID = Convert.ToString(item.IngredientGUID);
                    ii.StoreURL = item.StoreURL;

                    Items.Add(ii);
                }
            }

            SelectedListview.ItemsSource = Items;
        }
        else if (shouldCloneOrEdit == "edit")
        {
            AddBtn = "Edit Recipe"; 


            if (serializedRecipe != null)
            {
                unserializedRecipe = JsonSerializer.Deserialize<Recipe>(serializedRecipe);
            }

            if (!string.IsNullOrEmpty(serializedIngredients))
            {
                selectedItems = JsonSerializer.Deserialize<List<IngredientItem>>(serializedIngredients);
            }

            RecipeGUID = unserializedRecipe.RecipeGUID; 

            CookTime = Convert.ToString(unserializedRecipe.CookTime);
            Serves = Convert.ToString(unserializedRecipe.Serves);
            Prep = Convert.ToString(unserializedRecipe.Prep);
            RecipeName = unserializedRecipe.Title;
            RecipeDirections = unserializedRecipe.Description; 
            
            var test = unserializedRecipe.RecipeIngredients;

            if (test.Count > 0)
            {
                foreach (var item in test)
                {
                    var ingredientGuid = item.IngredientId;
                    var storeName = item.storeName;
                    var storeUrl = item.storeUrl;

                    var ii = new IngredientItem();

                    ii.Name = item.ingredientName;
                    ii.StoreName = storeName;
                    ii.IngredientGUID = Convert.ToString(item.IngredientId);
                    ii.StoreURL = storeUrl;

                    Items.Add(ii);
                }
            }
            else
            {
                foreach (var item in selectedItems)
                {
                    var ii = new IngredientItem();

                    ii.Name = item.Name;
                    ii.StoreName = item.StoreName;
                    ii.IngredientGUID = Convert.ToString(item.IngredientGUID);
                    ii.StoreURL = item.StoreURL;

                    Items.Add(ii);
                }
            }

                SelectedListview.ItemsSource = Items;
        }
    }

    private async void OnSelectExistingIngredientTapped(object sender, EventArgs e)
    {
        var username = await SecureStorage.GetAsync("user_name"); 

        await Navigation.PushAsync(new IngredientsListView(_recipeService, username, true, _httpClientFactory)); 
    }

    private void CategoryPicker2_Loaded(object sender, EventArgs e)
    {
        CategoryPicker2.SelectedIndex = 0;
    }

    private async void CreateRecipeDB(object sender, EventArgs e)
    { 
        var recipeTitle = IngredientNameEntry.Text;
        var recipeDescription = QuantityEntry.Text;
        var selectedCategory = CategoryPicker2.SelectedItem.ToString(); 

        var prepTime = PrepEntry.Text;
        var cookTime = CookEntry.Text;
        var servesTime = ServesEntry.Text; 

        var realName = await SecureStorage.GetAsync("user_name");
        var email = await SecureStorage.GetAsync("email"); //chef email or user email 
        var category = CategoryPicker2.SelectedItem;

        var CategoryMap = new Dictionary<string, int>
                        {
                            { "Appetizer", 1 },
                            { "Beverage", 2 },
                            { "Breakfast", 1002 },
                            { "Brunch", 1003 },
                            { "Dessert", 1004 },
                            { "Main Dish", 1005 },
                            { "Side Dish", 2002 },
                            { "Snack", 2003 }
                        };

        if (category != null)
        {
            category = (string)category; 
        }

        var serializedIngredients = await SecureStorage.GetAsync("selected_ingredients");
        var selectedItems = new List<IngredientItem>();
        var recipeIngredients = new List<RecipeAndRiDTO>(); 

        if (shouldCloneOrEdit == null)
        {
            if (!string.IsNullOrEmpty(serializedIngredients))
            {

                selectedItems = JsonSerializer.Deserialize<List<IngredientItem>>(serializedIngredients);
            }
        }
        else if (shouldCloneOrEdit == "edit")
        {
            selectedItems = SelectedListview.ItemsSource
                            ?.Cast<IngredientItem>()
                            .ToList();
        }
        else if (shouldCloneOrEdit == "clone")
        {
            selectedItems = SelectedListview.ItemsSource
                ?.Cast<IngredientItem>()
                .ToList();
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
        recipeDto.Category = Convert.ToString(CategoryMap[selectedCategory]); 
        recipeDto.Favorite = "Yes";
        recipeDto.RecipeIngredients = recipeIngredients;
        recipeDto.Photo = "food.jpg";
        //?
        recipeDto.CookTime = Convert.ToInt32(cookTime);
        recipeDto.Serves = Convert.ToInt32(servesTime);
        recipeDto.Stars = 5;

        SecureStorage.Default.Remove("selected_recipe");
        SecureStorage.Default.Remove("selected_ingredients");
        SecureStorage.Default.Remove("should_clone_or_edit");

        if (shouldCloneOrEdit == "clone" || shouldCloneOrEdit == "edit")
        {
            recipeDto.RecipeID = Convert.ToInt32(RecipeGUID); 
        }

        if (shouldCloneOrEdit == null)
        {
            await APIClient.CreateRecipe(_httpClientFactory, recipeDto);

            var allRecipes = await APIClient.GetAllRecipes(_httpClientFactory);
            await _recipeService.ResetRecipes(allRecipes);

            await Navigation.PushAsync(new Search(_recipeService, _notifService, _httpClientFactory));
        }
        else if (shouldCloneOrEdit == "edit")
        {
            await APIClient.EditRecipe(_httpClientFactory, recipeDto);

            var allRecipes = await APIClient.GetAllRecipes(_httpClientFactory);
            await _recipeService.ResetRecipes(allRecipes);

            await Navigation.PushAsync(new Search(_recipeService, _notifService, _httpClientFactory));
        }
        else if (shouldCloneOrEdit == "clone")
        {
            recipeDto.IsCloned = true;

            await APIClient.CreateRecipe(_httpClientFactory, recipeDto);

            var allRecipes = await APIClient.GetAllRecipes(_httpClientFactory);
            await _recipeService.ResetRecipes(allRecipes);

            await Navigation.PushAsync(new Search(_recipeService, _notifService, _httpClientFactory));
        } 
    } 
}
public class TodoItem
{
    public string Name { get; set; }
    public bool IsChecked { get; set; }
}