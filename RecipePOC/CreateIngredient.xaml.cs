using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RecipePOC;

public partial class CreateIngredient : ContentPage
{
	private IRecipeService _recipeService;
    private IHttpClientFactory _httpClientFactory;
	private string _username = string.Empty; 

    /**
     * Combine the ingredients for prep
     * Make sure prep time / cook time can be n/a
     * **/
	public CreateIngredient(IRecipeService recipeService, string username, IHttpClientFactory httpClientFactory)
	{
		InitializeComponent();

		_recipeService = recipeService;
		_username = username;
        _httpClientFactory = httpClientFactory;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing(); 
    }

    private async void UploadImage_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select a file"
            });

            if (result != null)
            {
                await DisplayAlert("File Selected", result.FileName, "OK");
                // You can also access result.FullPath
                // or process the stream:
                // using var stream = await result.OpenReadAsync();
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "File picking failed.", "OK");
        }
    } 

    // recipe ingredients = linked ingredient and recipe

    private async void AddIngredient(object sender, EventArgs e)
    {
        var title = TitleEntry.Text; 
        var unitName = UnitNameEntry.Text;
        var storeName = StoreNameEntry.Text;
        var storeUrl = StoreUrlEntry.Text;

        var dto = new IngredientDto();

        dto.IngredientName = title;
        dto.Store_ID = 1;
        dto.Unit_ID = 1; 
        dto.UnitName = unitName;  ;
        dto.StoreName = storeName; 
        dto.StoreUrl = storeUrl; 
        dto.CreatedBy = _username; 
        dto.Photo = "food.png";
        dto.Stars = "5";
        dto.Preptime = "0";
        dto.CookTime = "0";
        dto.Serves = "0";
        dto.IngredientGUID = "waefwaefwaef"; 

        await _recipeService.AddIngredient(dto);

        await APIClient.CreateIngredient(_httpClientFactory, dto);

        var ingredientsFresh = await _recipeService.GetIngredientsFresh();

        await _recipeService.ResetIngredients(ingredientsFresh); 
    }

    private readonly Color Grey = Color.FromArgb("#C0C0C0");
    private readonly Color Yellow = Color.FromArgb("#FFD700"); // your yellow

    private void OnStarTapped(object sender, EventArgs e)
    {
        if (sender is not Label tappedStar)
            return;

        int rating = int.Parse(tappedStar.GestureRecognizers
            .OfType<TapGestureRecognizer>()
            .First()
            .CommandParameter
            .ToString());

        var parent = (HorizontalStackLayout)tappedStar.Parent;

        for (int i = 0; i < parent.Children.Count; i++)
        {
            if (parent.Children[i] is Label star)
            {
                star.TextColor = (i < rating) ? Yellow : Grey;
            }
        }

        // Optional: save rating
        // SelectedRating = rating;
    }
}