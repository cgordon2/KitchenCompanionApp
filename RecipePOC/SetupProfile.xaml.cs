using RecipePOC.DB;
using RecipePOC.DB.Models;
using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using SQLite;
using System.Net.Http.Json;
using System.Numerics;
using System.Security.Cryptography;
using System.Net.Http.Headers;
using System.ComponentModel;
namespace RecipePOC;

public partial class SetupProfile : ContentPage, INotifyPropertyChanged
{
    private IRecipeService _recipeService;
    private INotificationService _notificationService; 
	private IAuthService _authService;
    private UserDTO _requestUser;
    private IHttpClientFactory theFactory;


    private string _avatarUrl;

    public string AvatarUrl
    {
        get => _avatarUrl;
        set
        {
            if (_avatarUrl != value)
            {
                _avatarUrl = value;
                OnPropertyChanged(nameof(AvatarUrl));
                OnPropertyChanged(nameof(UserAvatar));  
            }
        }
    }

    public SetupProfile(UserDTO requestUser, IHttpClientFactory factory)
	{
        _authService = MauiProgram.Services.GetService<IAuthService>();

        _requestUser = requestUser;
        _recipeService = MauiProgram.Services.GetService<IRecipeService>();

        _notificationService = MauiProgram.Services.GetService<INotificationService>(); 

        BindingContext = this;

        theFactory = factory;

        InitializeComponent();

        languagePicker.SelectedItem = "English";

    }

    public ImageSource UserAvatar
    {
        get
        {
            const string ApiBaseUrl = "http://192.168.7.203:5285";

            var file = string.IsNullOrWhiteSpace(AvatarUrl)
                       || AvatarUrl == "user_ico.png"
                       ? "user_ico.png"
                       : AvatarUrl;

            return ImageSource.FromUri(
                new Uri($"{ApiBaseUrl}/uploads/{file}")
            );
        }
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        if (_requestUser.IsSetup)
        {
            var userName = await SecureStorage.GetAsync("user_name");
            var foundUser = await APIClient.GetUser(theFactory, userName);

            var zipCodeString = foundUser.Location;
            var shortBio = foundUser.ShortBio;
            var realName = foundUser.RealName;
            var emailAddress = foundUser.Email;
            AvatarUrl = foundUser.AvatarUrl;

            EmailAddressTxt.IsEnabled = false;
            EmailAddressTxt.Text = emailAddress;


            real_name.Text = realName;  
        }
    }

    public class ZipResponse
    {
        public string PostCode { get; set; }
        public string Country { get; set; }
        public List<Place> Places { get; set; }
    }

    public class Place
    {
        public string PlaceName { get; set; }
        public string State { get; set; }
        public string StateAbbreviation { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    public static async Task<ZipResponse> GetLocationFromZip(string zip)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "C# App");

        var url = $"https://api.zippopotam.us/us/{zip}";
        var result = await http.GetFromJsonAsync<ZipResponse>(url);

        return result; 
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

                await SecureStorage.Default.SetAsync("AvatarPhotoName", result.FileName);

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

    private async void UpdateProfile(object sender, EventArgs e)
    {
        if (_authService != null)
        {
            var userName = await SecureStorage.GetAsync("user_name");
            var avatarUrl = await SecureStorage.GetAsync("AvatarPhotoName");

            if (avatarUrl == null)
                avatarUrl = "user_ico.png"; 

            var realName = real_name.Text;  
            var userLanguage =  languagePicker.SelectedItem;
            var temp = string.Empty;
            var shortBio = bio_1.Text; 
            var zipcode = zipCode.Text;
            var email = EmailAddressTxt.Text;

            string country = string.Empty; 

            if (userLanguage == null)
            {
                temp = "English"; 
            }
            else
            {
                temp = (string)userLanguage; 
            }

            if (zipcode != null)
            {
                zipcode = zipcode.Trim();

                var cityAndCountry = await GetLocationFromZip(zipcode);

                country = cityAndCountry.Country;
                var state = "";

                if (cityAndCountry.Places.Count > 0)
                {
                    state = cityAndCountry.Places[0].State;
                }
            }

            var userDto = new UserDTO();

            userDto.UserName = userName;
            userDto.IsSetup = true;
            userDto.ShortBio = shortBio; 
            userDto.RealName = realName; 
            userDto.ShortBio = "";
            userDto.Email = email;
            userDto.AvatarUrl = avatarUrl; 
            if (zipcode != string.Empty && zipcode != null)
            {
                userDto.Location = country;
            }

            userDto.Language = temp;

            var foundUser = await APIClient.GetUser(theFactory, userName); 

            if (!foundUser.IsSetup)
            {
                await SecureStorage.Default.SetAsync("email", email); 
                userDto.Email = email;
            }
            else
            {
                // cant update
            }

                // first set it in here for securestorage :)
                // can only set email once

            var _connection = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);
            await _authService.UpdateUserProfile(_connection, email, userName, shortBio, temp, country, false, realName, avatarUrl);

            await APIClient.CompleteProfile(theFactory, userDto);

            if (_requestUser.NavigateToProfile == true)
            {
                await Navigation.PushAsync(new Profile()); 
            }
            else
            {
                await Navigation.PushAsync(new MainPage(_authService, _recipeService, theFactory, _connection));
            }
        }
    }
}