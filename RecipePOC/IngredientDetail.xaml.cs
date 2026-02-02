using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace RecipePOC;
// https://chatgpt.com/share/694f0ef2-deb4-8009-9ad2-8a5fd6fc2bab
public partial class IngredientDetail : ContentPage, INotifyPropertyChanged
{ 
    private Recipe _recipe;
    private IRecipeService _recipeService;
    private INotificationService _notifService; 
    private IHttpClientFactory _httpClientFactory; 
    public event PropertyChangedEventHandler PropertyChanged;
    public ICommand FavoriteCommand { get; }

    public List<RecipeAndRiDTO> RIEditList = new List<RecipeAndRiDTO>();

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ===== BACKING FIELDS =====
    private bool _isOwner;
    private bool _isFavorite;

    // ===== PUBLIC PROPERTIES =====
    public bool IsOwner
    {
        get => _isOwner;
        set
        {
            _isOwner = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FavoriteText));
            OnPropertyChanged(nameof(ShowFavorite));
        }
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            _isFavorite = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FavoriteText));
        }
    }

    public string FavoriteText
    {
        get
        {
            if (!IsOwner)
                return "\uD83D\uDCC4";

            return IsFavorite ? "\u2764\uFE0F" : "\uD83E\uDD0D";
        } 
    }

    public bool ShowFavorite => true;

    public string RecipeName { get; set; } 
    public string Favorite { get; set; } 
    public string UserName { get; set; } 
    public string Description { get; set; } 

    public string PrepTime { get; set; } 
    public string Serves { get; set; } 

    public string CookTime { get; set; } 

    public int Stars { get; set; } 

    public string Photo { get; set; } 

    public int RecipeId { get; set; }

    private string _ingredientsCount;
    public string IngredientsCount
    {
        get => _ingredientsCount;
        set
        {
            if (_ingredientsCount != value)
            {
                _ingredientsCount = value;
                OnPropertyChanged();
            }
        }
    }

    public class IngredientDetailObj
    {
        public string Name { get; set; } 
        public string store { get; set; }
        public string storeUrl { get; set; } 
        public int Stars { get; set; } 
        public string PrepTime { get; set; } 
        public string CookTime { get; set; } 
        public string Serves { get; set; } 
        public string Photo { get; set; }
        public ImageSource RecipePhoto
        {
            get
            {
                // fallback image from Resources/Images
                if (string.IsNullOrWhiteSpace(Photo) || Photo == "food.jpg")
                    return ImageSource.FromFile("food.jpg");

                const string ApiBaseUrl = "http://192.168.7.203:5285";
                var imageUrl = $"{ApiBaseUrl}/uploads/{Photo}";

                return ImageSource.FromUri(new Uri(imageUrl));
            }
        }
    }

	public IngredientDetail(RecipePOC.Recipe recipe, IRecipeService service, IHttpClientFactory theFactory)
	{
		InitializeComponent();
        _recipe = recipe;
        _recipeService = service;
        _httpClientFactory = theFactory;

        Favorite = recipe.Favorite; 
        RecipeName = recipe.Title; 
        Description = recipe.Description;

        if (string.IsNullOrWhiteSpace(recipe.Photo) || recipe.Photo == "food.jpg")
        {
            Photo = "food.jpg"; 
        } 
        else
        {

            const string ApiBaseUrl = "http://192.168.7.203:5285"; // dev PC

            var imageUrl = $"{ApiBaseUrl}/uploads/{recipe.Photo}";
            Photo = imageUrl; 
        }
        _notifService = MauiProgram.Services.GetService<INotificationService>();

        CookTime = "Cook time " + recipe.CookTime + " mins"; 
        Serves = "Serves " + recipe.Serves;
        PrepTime = "Prep time "+recipe.Prep + " mins";
        UserName = recipe.UserName;
        Stars = recipe.Stars; 
        
        RecipeId = Convert.ToInt16(recipe.RecipeGUID); 

        Ingredients = new ObservableCollection<IngredientDetailObj>();

        //FavoriteCommand = new Command(OnFavoriteClicked);
        FavoriteCommand = new Command(async () => await OnFavoriteClicked());

        BindingContext = this;
    }

    private async Task OnFavoriteClicked()
    {
        switch (FavoriteText)
        {
            case "\uD83E\uDD0D":
                var favRequest = new FavoriteRequest();
                favRequest.RecipeId = RecipeId;
                IsFavorite = true; 

                await APIClient.FavRecipe(_httpClientFactory, favRequest); 
                break;

            case "\u2764\uFE0F":
                var unfavRequest = new FavoriteRequest();
                unfavRequest.RecipeId = RecipeId;
                IsFavorite = false; 

                await APIClient.UnfavRecipe(_httpClientFactory, unfavRequest);  
                break;

            case "\uD83D\uDCC4":
                // also seriaized RI 
                _recipe.RecipeGUID = Convert.ToString(RecipeId);
                _recipe.RecipeIngredients = RIEditList;

                var json = JsonSerializer.Serialize(_recipe);
                await SecureStorage.SetAsync("selected_recipe", json);
                await SecureStorage.SetAsync("should_clone_or_edit", "clone");

                await Navigation.PushAsync(new CreateRecipe(_recipeService, _httpClientFactory)); 
                break;
        }
    }

    public ObservableCollection<IngredientDetailObj> Ingredients { get; set; }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = OnAppearingAsync();
    }

    private async Task OnAppearingAsync()
    {
        var username = await SecureStorage.Default.GetAsync("user_name");
        IsFavorite = string.Equals(Favorite, "Yes", StringComparison.OrdinalIgnoreCase);

        Ingredients.Clear();
        RIEditList.Clear(); 

        if (string.IsNullOrEmpty(username))
            return;

        // Owner comes from your recipe data
        IsOwner = username == UserName;

        // Favorite comes from your recipe data
        IsFavorite = Favorite == "Yes";

        var result = await _recipeService.GetRecipeIngredients(RecipeId); 


        foreach (var item in result)
        {
            var name = item.IngredientName;
            var store = item.StoreName;
            var storeUrl = item.StoreUrl;
            var stars = item.Stars;
            var prepTime = item.PrepTime;
            var cookTime = item.CookTime;
            var serves = item.Serves; 

            var ingredient = new IngredientDetailObj();
            var recipeRI = new RecipeAndRiDTO();

            recipeRI.RecipeId = RecipeId; 
            recipeRI.ingredientName = item.IngredientName;
            recipeRI.Quantity = item.Quantity; 
            recipeRI.IngredientId = item.IngredientId;
            recipeRI.storeName = item.StoreName; 
            recipeRI.storeUrl = storeUrl;
            recipeRI.unitName = item.UnitName;

            RIEditList.Add(recipeRI);

            
            ingredient.Name = name;
            ingredient.store = store;
            ingredient.storeUrl = storeUrl;
            ingredient.Stars = stars;
            ingredient.Photo = item.Photo; 

            ingredient.PrepTime = "Prep Time: " + prepTime;
            ingredient.CookTime = "Cook Time: " + cookTime;
            ingredient.Serves = "Serves: " + serves; 

            Ingredients.Add(ingredient); 
        }

        IngredientList.ItemsSource = Ingredients;
        IngredientsCount = "Ingredient(s): " + Ingredients.Count; 
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        // prompt the user if this is not one of theirs
        var username= _recipe.UserName;
        var loggedInUser = await SecureStorage.GetAsync("user_name"); 

        if (loggedInUser != username)
        {
            // done in tagging
        }
        else
        {
            // also seriaized RI 
            _recipe.RecipeGUID = Convert.ToString(RecipeId);
            _recipe.RecipeIngredients = RIEditList;

            var json = JsonSerializer.Serialize(_recipe);
            await SecureStorage.SetAsync("selected_recipe", json);
            await SecureStorage.SetAsync("should_clone_or_edit", "edit"); 

            await Navigation.PushAsync(new CreateRecipe(_recipeService, _httpClientFactory)); 
        }
    } 
    private async void DeleteButton_Clicked(object sender, EventArgs e)
    {
        var username = _recipe.UserName;
        var loggedInUser = await SecureStorage.GetAsync("user_name");

        var recipeDto = new RecipeDto();

        recipeDto.RecipeID = RecipeId;
        recipeDto.CookTime = 1;
        recipeDto.Stars = 1;
        recipeDto.Serves = 1;
        recipeDto.Prep = 1;
        recipeDto.IsCloned = false;
        recipeDto.Description = "awefawef"; 

        await APIClient.DeleteRecipe(_httpClientFactory, recipeDto);

        var allRecipes = await APIClient.GetAllRecipes(_httpClientFactory);
        var clonedRecipes = await APIClient.GetClonedRecipes(_httpClientFactory);

        allRecipes.AddRange(clonedRecipes);

        await _recipeService.ResetRecipes(allRecipes);

        await Navigation.PushAsync(new Search(_recipeService, null, _httpClientFactory)); 
    }
}