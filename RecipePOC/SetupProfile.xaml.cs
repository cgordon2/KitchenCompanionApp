using RecipePOC.DB;
using RecipePOC.DB.Models;
using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using SQLite;
using System.Net.Http.Json;
using System.Numerics;
using System.Security.Cryptography;

namespace RecipePOC;

public partial class SetupProfile : ContentPage
{
    private IRecipeService _recipeService;
    private INotificationService _notificationService; 
	private IAuthService _authService;
    private UserDTO _requestUser;
    private IHttpClientFactory theFactory; 

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

private async void UpdateProfile(object sender, EventArgs e)
    {
        if (_authService != null)
        {
            var userName = await SecureStorage.GetAsync("user_name");

            var realName = real_name.Text; 
            var bio1 = bio_1.Text;
            var bio2 = bio_2.Text; 
            var bio3 = bio_3.Text;
            var userLanguage =  languagePicker.SelectedItem;
            var temp = string.Empty;
            var zipcode = zipCode.Text;
            var email = EmailAddressTxt.Text; 

            if (userLanguage == null)
            {
                temp = "English"; 
            }
            else
            {
                temp = (string)userLanguage; 
            }
            zipcode = zipcode.Trim(); 

            var cityAndCountry = await GetLocationFromZip(zipcode);

            var country = cityAndCountry.Country;
            var state = ""; 

            if (cityAndCountry.Places.Count > 0)
            {
                state = cityAndCountry.Places[0].State;
            }

            var shortBio = bio1; 

            if (bio2 != null)
            {
                shortBio += "," + bio2;
            }

            if (bio3 != null)
            {
                shortBio += "," + bio3; 
            }

            var userDto = new UserDTO();

            userDto.UserName = userName;
            userDto.IsSetup = true;
            userDto.RealName = realName; 
            userDto.ShortBio = shortBio;
            userDto.Location = country;
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
            await _authService.UpdateUserProfile(_connection, email, userName, shortBio, temp, country, false, realName);

            await APIClient.CompleteProfile(theFactory, userDto);

            await Navigation.PushAsync(new MainPage(_authService, _recipeService, theFactory, _connection)); 

        }
    }
}