using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Models;
using RecipePOC.Services.Recipes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
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

    int _rating = 0;
    Label[] _stars;

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

    private void StarTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter == null)
            return;

        _rating = Convert.ToInt32(e.Parameter);

        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i].TextColor = i < _rating
                ? Colors.Gold
                : Colors.Silver;
        }
    }

    protected override async void OnAppearing()
    {
        /**
         * 
         * We need recipe ID for this **/ 
        base.OnAppearing();

        _stars = new[] { Star1, Star2, Star3, Star4, Star5 };


        var recipeTitle = await SecureStorage.Default.GetAsync("recipe_title");
        var recipeDescription = await SecureStorage.Default.GetAsync("recipe_description");
        var selectedCategory = await SecureStorage.Default.GetAsync("selected_category");
        var fromIngredients = await SecureStorage.Default.GetAsync("from_ingredients");

        SecureStorage.Default.Remove("from_ingredients"); 

        var serializedRecipe = await SecureStorage.GetAsync("selected_recipe");
        shouldCloneOrEdit = await SecureStorage.GetAsync("should_clone_or_edit"); 
        var serializedIngredients = await SecureStorage.GetAsync("selected_ingredients");
        var selectedItems = new List<IngredientItem>();
        var recipeIngredients = new List<RecipeAndRiDTO>();
        Recipe unserializedRecipe = null;

        SecureStorage.Default.Remove("recipe_title");
        SecureStorage.Default.Remove("recipe_description");
        SecureStorage.Default.Remove("selected_category"); 

        var CategoryMap = new Dictionary<string, int>
                        {
                            { "Appetizer", 2 },
                            { "Beverage", 3 },
                            { "Breakfast", 4 },
                            { "Brunch", 5 },
                            { "Dessert", 6 },
                            { "Main Dish", 7 },
                            { "Side Dish", 8 },
                        };


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


            /*RecipeGUID = unserializedRecipe.RecipeGUID;
            CookTime = Convert.ToString(unserializedRecipe.CookTime);
            Serves = Convert.ToString(unserializedRecipe.Serves);
            Prep = Convert.ToString(unserializedRecipe.Prep);**/ 

            if (fromIngredients == null)
            { 
            }
            else
            {
                if (selectedCategory != null)
                { 
                }

                if (recipeTitle != null)
                    RecipeName = recipeTitle;
                if (recipeDescription != null)
                    RecipeDirections = recipeDescription;
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

            if (unserializedRecipe.Photo != "food.jpg" &&  unserializedRecipe.Photo != "food.png")
            {
                const string ApiBaseUrl = "http://192.168.7.203:5285"; // dev PC

                var imageUrl = $"{ApiBaseUrl}/uploads/{unserializedRecipe.Photo}";

                RecipePhotoPickerBtn.Source =
                    ImageSource.FromUri(new Uri(imageUrl));
            }

            if (!string.IsNullOrEmpty(serializedIngredients))
            {
                selectedItems = JsonSerializer.Deserialize<List<IngredientItem>>(serializedIngredients);
            }

            RecipeGUID = unserializedRecipe.RecipeGUID;
            CookTime = Convert.ToString(unserializedRecipe.CookTime);
            Serves = Convert.ToString(unserializedRecipe.Serves);
            Prep = Convert.ToString(unserializedRecipe.Prep);
            if (fromIngredients == null)
            {

                RecipeName = unserializedRecipe.Title;
                RecipeDirections = unserializedRecipe.Description;
            }
            else
            {

                if (recipeTitle != null)
                    RecipeName = recipeTitle;
                if (recipeDescription != null)
                    RecipeDirections = recipeDescription;
            }

                var test = unserializedRecipe.RecipeIngredients;

            if (selectedItems.Count == 0)
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

            if (unserializedRecipe.Photo != "food.jpg" && unserializedRecipe.Photo != "food.png")
            {
                const string ApiBaseUrl = "http://192.168.7.203:5285"; // dev PC

                var imageUrl = $"{ApiBaseUrl}/uploads/{unserializedRecipe.Photo}";

                RecipePhotoPickerBtn.Source =
                    ImageSource.FromUri(new Uri(imageUrl));
            }

            RecipeGUID = unserializedRecipe.RecipeGUID;
            CookTime = Convert.ToString(unserializedRecipe.CookTime);
            Serves = Convert.ToString(unserializedRecipe.Serves);
            Prep = Convert.ToString(unserializedRecipe.Prep);
            if (fromIngredients == null)
            {

                RecipeName = unserializedRecipe.Title;
                RecipeDirections = unserializedRecipe.Description;
            }
            else
            {

                if (recipeTitle  != null)
                    RecipeName = recipeTitle;
                if (recipeDescription != null)
                    RecipeDirections = recipeDescription;
            }

            var test = unserializedRecipe.RecipeIngredients;

            if (selectedItems.Count == 0)
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

        var recipeTitle = IngredientNameEntry.Text;
        var recipeDescription = QuantityEntry.Text;

        string categoryName = CategoryPicker2.SelectedItem as string ?? "";

        if (recipeTitle != null)
        {
            await SecureStorage.Default.SetAsync("recipe_title", recipeTitle);
        }

        if (recipeDescription != null)
            await SecureStorage.Default.SetAsync("recipe_description", recipeDescription);

        if (categoryName != null)
            await SecureStorage.Default.SetAsync("selected_category", categoryName);

        await SecureStorage.Default.SetAsync("from_ingredients", "true"); 

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
                            { "Appetizer", 2 },
                            { "Beverage", 3 },
                            { "Breakfast", 4 },
                            { "Brunch", 5 },
                            { "Dessert", 6 },
                            { "Main Dish", 7 },
                            { "Side Dish", 8 }, 
                        };

        if (category != null)
        {
            category = (string)category; 
        }

        var serializedIngredients = await SecureStorage.GetAsync("selected_ingredients");
        var selectedItems = new List<IngredientItem>();
        var recipeIngredients = new List<RecipeAndRiDTO>();

        var recipePhotoName = await SecureStorage.GetAsync("RecipePhotoName"); 


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
                test.RecipeId = 0; // this is set in the api :D
                test.ingredientName = item.Name;
                test.storeName = item.StoreName;
                test.storeUrl = item.StoreURL;
                test.unitName = item.UnitName;

                recipeIngredients.Add(test);
            }

        var recipeDto = new RecipeDto();

        recipeDto.RecipeName = recipeTitle; 
        
        recipeDto.Description = recipeDescription;
        recipeDto.ChefName = realName;
        recipeDto.ChefEmail = email;
        recipeDto.Category = Convert.ToString(CategoryMap[selectedCategory]); 
        recipeDto.Favorite = "No"; // TODO: GET BUG
        recipeDto.RecipeIngredients = recipeIngredients;
        recipeDto.Photo = recipePhotoName; // food.jpg
        //?
        recipeDto.CookTime = Convert.ToInt32(cookTime);
        recipeDto.Serves = Convert.ToInt32(servesTime);
        recipeDto.Stars = _rating;
        recipeDto.Prep = Convert.ToInt32(prepTime); 

        SecureStorage.Default.Remove("recipe_title");
        SecureStorage.Default.Remove("recipe_description");
        SecureStorage.Default.Remove("selected_category"); 

        SecureStorage.Default.Remove("selected_recipe");
        SecureStorage.Default.Remove("selected_ingredients");
        SecureStorage.Default.Remove("should_clone_or_edit");
        SecureStorage.Default.Remove("RecipePhotoName"); 

        if (shouldCloneOrEdit == "clone" || shouldCloneOrEdit == "edit")
        {
            recipeDto.RecipeID = Convert.ToInt32(RecipeGUID); 
        }

        if (shouldCloneOrEdit == null)
        {
            await APIClient.CreateRecipe(_httpClientFactory, recipeDto);

            var allRecipes = await APIClient.GetAllRecipes(_httpClientFactory);
            var clonedRecipes = await APIClient.GetClonedRecipes(_httpClientFactory);

            allRecipes.AddRange(clonedRecipes);
            await _recipeService.ResetRecipes(allRecipes);

            await Navigation.PushAsync(new Search(_recipeService, _notifService, _httpClientFactory));
        }
        else if (shouldCloneOrEdit == "edit")
        {
            await APIClient.EditRecipe(_httpClientFactory, recipeDto);

            var allRecipes = await APIClient.GetAllRecipes(_httpClientFactory);
            var clonedRecipes = await APIClient.GetClonedRecipes(_httpClientFactory);

            allRecipes.AddRange(clonedRecipes); 

            await _recipeService.ResetRecipes(allRecipes);

            await Navigation.PushAsync(new Search(_recipeService, _notifService, _httpClientFactory));
        }
        else if (shouldCloneOrEdit == "clone")
        {
            recipeDto.IsCloned = true;

            await APIClient.CreateRecipe(_httpClientFactory, recipeDto);

            var allRecipes = await APIClient.GetAllRecipes(_httpClientFactory);
            var clonedRecipes = await APIClient.GetClonedRecipes(_httpClientFactory);

            allRecipes.AddRange(clonedRecipes);

            await _recipeService.ResetRecipes(allRecipes);

            await Navigation.PushAsync(new Search(_recipeService, _notifService, _httpClientFactory));
        } 
    }

    private async void OnPickPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select an image",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null)
                return;

            using var stream = await result.OpenReadAsync();
            using var content = new MultipartFormDataContent();

            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            content.Add(streamContent, "file", result.FileName);

            using var httpClient = new HttpClient();
             
            var response = await httpClient.PostAsync(
                "http://192.168.7.203:5285/api/recipes/uploadimage",
                content);

            if (response.IsSuccessStatusCode)
            {
                const string ApiBaseUrl = "http://192.168.7.203:5285"; // dev PC

                var imageUrl = $"{ApiBaseUrl}/uploads/{result.FileName}";

                RecipePhotoPickerBtn.Source =
                    ImageSource.FromUri(new Uri(imageUrl));

                await SecureStorage.Default.SetAsync("RecipePhotoName", result.FileName);

                await DisplayAlert("Success", "Image uploaded", "OK");
            }
            else
            {
                await DisplayAlert("Error", response.StatusCode.ToString(), "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        } 
    }
}
public class TodoItem
{
    public string Name { get; set; }
    public bool IsChecked { get; set; }
}