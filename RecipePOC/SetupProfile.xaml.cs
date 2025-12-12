using RecipePOC.DB;
using RecipePOC.Services;
using SQLite;
using System.Net.Http.Json;

namespace RecipePOC;

public partial class SetupProfile : ContentPage
{
	private IAuthService _authService; 

	public SetupProfile()
	{
		InitializeComponent();

        _authService = MauiProgram.Services.GetService<IAuthService>();

        BindingContext = this;

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
            var realName = real_name.Text;
            var password_cs = password.Text;
            var bio1 = bio_1.Text;
            var bio2 = bio_2.Text; 
            var bio3 = bio_3.Text;
            var userLanguage =  languagePicker.SelectedItem;
            var temp = string.Empty;
            var zipcode = zipCode.Text;
            var emailUser = email.Text; 

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

            if (state != "")
            {
                country = country + " " + state; 
            }

            var _connection = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);
            _authService.UpdateUserProfile(_connection, emailUser, "gordoncm", bio1 + "," + bio2 + "," + bio3, temp, country, false); 
        }
    }
}