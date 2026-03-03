using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
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
        _stars = new[] { Star1, Star2, Star3, Star4, Star5 };

        SecureStorage.Default.Remove("IngredientPhotoName"); 
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        SecureStorage.Default.Remove("IngredientPhotoName");
    }
    int _rating = 0;
    Label[] _stars;
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

    private async void UploadImage_Clicked(object sender, EventArgs e)
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
                "https://api.recipetracker.xyz/api/recipes/uploadimage",
                content);

            if (response.IsSuccessStatusCode)
            {
                const string ApiBaseUrl = "http://192.168.7.203:5285"; // dev PC

                var imageUrl = $"{ApiBaseUrl}/uploads/{result.FileName}";

                IngredientImageBtn.Source =
                    ImageSource.FromUri(new Uri(imageUrl));

                await SecureStorage.Default.SetAsync("IngredientPhotoName", result.FileName);

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

    // recipe ingredients = linked ingredient and recipe

    private async void AddIngredient(object sender, EventArgs e)
    {
        var UnitsMap = new Dictionary<string, int>
        {
            { "g", 1},
            {"tsp", 2 },
            {"tbsp", 3 },
            {"kg", 7 },
            {"lbs", 8 },
            {"oz", 9 }, 
            {"gal", 10 },
            {"loaf", 11 },
            {"cups", 13 },
            {"pt", 15 }, 
        }; 

        var CategoryMap = new Dictionary<string, int>
                        {
                            { "Walmart", 1 },
                            { "Target", 2 },
                            { "Costco", 3 },
                            { "Meijer", 4 },
                            { "Kroger", 5 }, 
                        };
        var title = TitleEntry.Text; 
        //var unitName = UnitNameEntry.Text;
        //var storeName = StoreNameEntry.Text;
        //var storeUrl = StoreUrlEntry.Text;
        var selectedCategory = StorePicker.SelectedItem.ToString();
        var photoName = await SecureStorage.Default.GetAsync("IngredientPhotoName");
        SecureStorage.Default.Remove("IngredientPhotoName");

        var selectedUnit = UnitsPicker.SelectedItem.ToString(); 

        var dto = new IngredientDto();

        dto.IngredientName = title;
        dto.Quantity = Convert.ToInt32(QuantityEntry.Text);
        dto.Store_ID = CategoryMap[selectedCategory]; 
        dto.Unit_ID = UnitsMap[selectedUnit];
        dto.UnitName = Convert.ToString(UnitsMap[selectedUnit]);  ;
        dto.StoreName = Convert.ToString(CategoryMap[selectedCategory]);
        dto.StoreUrl = "testtelly"; 
        dto.CreatedBy = _username;
        dto.Photo = photoName; 
        dto.Stars = Convert.ToString(_rating);
        dto.Preptime = "0";
        dto.CookTime = "0";
        dto.Serves = "0";
        dto.IngredientGUID = "waefwaefwaef"; 

        await _recipeService.AddIngredient(dto);

        await APIClient.CreateIngredient(_httpClientFactory, dto);

        var ingredientsFresh = await _recipeService.GetIngredientsFresh();

        await _recipeService.ResetIngredients(ingredientsFresh);

        await Shell.Current.GoToAsync("..");

    }

    private void CategoryPicker2_Loaded(object sender, EventArgs e)
    {
        StorePicker.SelectedIndex = 0;
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