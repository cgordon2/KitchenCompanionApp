using RecipePOC.Services;
using RecipePOC.Services.Models;
using RecipePOC.Services.Recipes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecipePOC;

public partial class IngredientsListView : ContentPage
{
    private IRecipeService _recipeService;
    private IHttpClientFactory _httpClientFactory;
    private string _username = string.Empty;
    private bool _displayCheckbox = false; 
    private bool _IsVisible { get; set; } 
    public ObservableCollection<IngredientItem> Items { get; set; } 

    public IngredientsListView(IRecipeService recipeService, string username, bool displayCheckbox, IHttpClientFactory httpClientFactory)
	{
		InitializeComponent();

        _recipeService = recipeService;
        _username = username; 
        _displayCheckbox = displayCheckbox;
        _httpClientFactory = httpClientFactory;

        BindingContext = this; 
    } 

    private View CreateLoadMoreFooter()
    {
        return new StackLayout
        {
            Padding = new Thickness(12),
            HorizontalOptions = LayoutOptions.Center, 
            Children =
    {
        new Button
        {
            Text = "Load more",
            FontSize = 14,
            CornerRadius = 18,
            WidthRequest = 140,
            HeightRequest = 36,
            BackgroundColor = Color.FromArgb("#222222"), // stays black
            TextColor = Colors.White,
            BorderColor = Color.FromArgb("#444444"),
            BorderWidth = 1,
            Shadow = new Shadow
            {
                Opacity = 0.3f,
                Radius = 6,
                Offset = new Point(0,2),
            },
                Command = new Command(async () => await LoadMore())
        }
    }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (PageHelpers.HasInternet())
        {
            var ingredientsFresh = await _recipeService.GetIngredientsFresh();

            await _recipeService.ResetIngredients(ingredientsFresh);
        }

        ResetPaging(); 

        await GetIngredients(); 
    }

    private async Task GetIngredients()
    {
        var ingredients = await _recipeService.GetIngredients(currentPage, pageSize);
        var items = new List<IngredientItem>(); 

        foreach (var ingredient in ingredients)
        {
            var ingredientItem = new IngredientItem();

            ingredientItem.Photo = ingredient.Photo;
            ingredientItem.Name = ingredient.IngredientName;
            ingredientItem.UnitName = ingredient.UnitName;
            ingredientItem.CreatedBy = ingredient.CreatedBy;
            ingredientItem.Stars = Convert.ToInt16(ingredient.Stars);
            ingredientItem.StoreName = ingredient.StoreName;
            ingredientItem.IngredientGUID = ingredient.IngredientGUID;
            ingredientItem.PrepTime = "Prep: "+ingredient.PrepTime + "m"; 
            ingredientItem.CookTime = "Cook: " + ingredient.CookTime + "m";
            ingredientItem.Serves = "Serves: " + ingredient.Serves + "m"; 

            if (_displayCheckbox)
            {
                ingredientItem.IsCheckboxVisible = true; 
            }
            else
            {
                ingredientItem.IsCheckboxVisible = false;
            }

            items.Add(ingredientItem);
        }

        currentPage++;

        IngredientBuffer.AddRange(items);

        RecipeListView.ItemsSource = null; 
        RecipeListView.ItemsSource = IngredientBuffer;

        RecipeListView.Footer =
                    (IngredientBuffer.Count > 0 && hasMoreData)
                     ? CreateLoadMoreFooter()
                     : null;
    }

    private int currentPage = 0;
    private int pageSize = 2;
    private bool isLoading = false;
    private bool hasMoreData = true;
    private List<IngredientItem> IngredientBuffer = new(); 

    private void ResetPaging()
    {
        currentPage = 0;
        hasMoreData = true;
        isLoading = false;
        IngredientBuffer.Clear();

        RecipeListView.ItemsSource = null;   // <-- KEY
        RecipeListView.Footer = null;        // <-- KEY
    }



    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (_displayCheckbox)
        {
            var selectedItems = IngredientBuffer.Where(i => i.IsSelected).ToList();

            var json = JsonSerializer.Serialize(selectedItems);
            await SecureStorage.SetAsync("selected_ingredients", json);

            await Navigation.PopAsync(); 
        }
        else
        { 
            var email = await SecureStorage.GetAsync("email"); 

            await Navigation.PushAsync(new CreateIngredient(_recipeService, email, _httpClientFactory)); 
        }
    }

    private async Task LoadMore()
    {
        if (currentMode == IngredientMode.Normal)
            await GetIngredients();
        else
            await SearchIngredients(currentSearchKeyword);
    }

    private async Task SearchIngredients(string keyword)
    {
        var ingredients = await _recipeService.SearchIngredients(keyword, currentPage, pageSize);
        var tempList = new List<IngredientItem>(); 

        // IMPORTANT!!!
        foreach (var ingredient in ingredients)
        {
            tempList.Add(new IngredientItem
            {
                Name = ingredient.IngredientName,
                StoreName = ingredient.StoreName,
                StoreURL = ingredient.StoreUrl,
                Stars = 5,
                Photo = ingredient.Photo,
                CreatedBy = ingredient.CreatedBy,
                IsCheckboxVisible = _displayCheckbox,
                IngredientGUID = ingredient.IngredientGUID, 
            });  
        }

        IngredientBuffer.AddRange(tempList); 

        currentPage++;
        hasMoreData = ingredients.Any();

        RecipeListView.ItemsSource = null; 
        RecipeListView.ItemsSource = IngredientBuffer;

        RecipeListView.Footer = hasMoreData ? CreateLoadMoreFooter() : null;
    }

    enum IngredientMode
    {
        Normal,
        Search
    }

    private IngredientMode currentMode = IngredientMode.Normal;
    private string currentSearchKeyword = "";

    private async void OnSearchButtonPressed(object sender, EventArgs e)
    {
        ResetPaging();
        var keyword = IngredientsSearchBar.Text?.ToLower() ?? "";
        currentMode = IngredientMode.Search;
        currentSearchKeyword = keyword;

        if (keyword != "")
        {
            await SearchIngredients(keyword); 
        }
        else
        {
            currentMode = IngredientMode.Normal;
            await GetIngredients(); 
        }

    }
}