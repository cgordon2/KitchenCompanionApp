using RecipePOC.Services.Recipes;
using System.Collections.ObjectModel;

namespace RecipePOC;

public partial class IngredientDetail : ContentPage
{ 
    private Recipe _recipe;
    private IRecipeService _recipeService;  
    
    public string RecipeName { get; set; } 
    public string Description { get; set; } 

    public class IngredientDetailObj
    {
        public string Name { get; set; } 
        public string store { get; set; }
        public string storeUrl { get; set; } 
        public int Stars { get; set; } 
        public string PrepTime { get; set; } 
        public string CookTime { get; set; } 
        public string Serves { get; set; } 
    }

	public IngredientDetail( Recipe recipe, IRecipeService service)
	{
		InitializeComponent();
        _recipe = recipe;
        _recipeService = service;

        Ingredients = new ObservableCollection<IngredientDetailObj>();

        RecipeName = _recipe.Title;
        Description = _recipe.Description;

        BindingContext = this;
    }

    public ObservableCollection<IngredientDetailObj> Ingredients { get; set; }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = OnAppearingAsync();
    }

    private async Task OnAppearingAsync()
    {
        var result = await _recipeService.GetRecipeIngredients(1);



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

            ingredient.Name = name;
            ingredient.store = store;
            ingredient.storeUrl = storeUrl;
            ingredient.Stars = stars;

            ingredient.PrepTime = "Prep Time: " + prepTime;
            ingredient.CookTime = "Cook Time: " + cookTime;
            ingredient.Serves = "Serves: " + serves; 

            Ingredients.Add(ingredient); 
        }
    }
}