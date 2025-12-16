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
        var prepTime = PrepEntry.Text;
        var cookTime = CookEntry.Text;
        var serves = ServesEntry.Text;
        var unitName = UnitNameEntry.Text;
        var storeName = StoreNameEntry.Text;
        var storeUrl = StoreUrlEntry.Text;

        var dto = new IngredientDto();

        dto.IngredientName = title;
        dto.UnitName = unitName;  ;
        dto.StoreName = storeName; 
        dto.StoreUrl = storeUrl; 
        dto.CreatedBy = _username; 
        dto.Photo = "food.png";
        dto.Stars = "5";
        dto.PrepTime = prepTime;
        dto.CookTime = cookTime;
        dto.Serves = serves; 

        await _recipeService.AddIngredient(dto);

        await APIClient.CreateIngredient(_httpClientFactory, dto);

        var ingredientsFresh = await _recipeService.GetIngredientsFresh();

        await _recipeService.ResetIngredients(ingredientsFresh); 
    }
}