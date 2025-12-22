using Microsoft.Maui.ApplicationModel.Communication;
using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using SQLite;
using System.Threading.Tasks;
using Windows.Networking.XboxLive;
using System.Security.Cryptography;
using System.Numerics;
namespace RecipePOC
{
    public partial class MainPage : ContentPage
    {
        private readonly IAuthService _authService;
        private readonly IRecipeService _recipeService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SQLiteAsyncConnection _connection; 

        public MainPage(IAuthService authService, IRecipeService recipeService, IHttpClientFactory httpClientFactory, SQLiteAsyncConnection connection)
        {
            InitializeComponent();

            _authService = authService;
            _recipeService = recipeService;
            _httpClientFactory = httpClientFactory;
            _connection = connection;
        }


        static BigInteger RandomBigInt(int bytes)
        {
            var buffer = new byte[bytes];
            RandomNumberGenerator.Fill(buffer);
            return new BigInteger(buffer, isUnsigned: true, isBigEndian: true);
        }


        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var username = UsernameTxt.Text;
            var password = PasswordEntry.Text;
            // cameron Hieverybody17$

            /** cameron!!! for user**/
            if (username != null && password != null)
            {
                var error = await _authService.LoginAsync(new DTOs.LoginRequestDto(username, password));

                if (!string.IsNullOrEmpty(error))
                {
                    if (PageHelpers.HasInternet())
                    {
                        var foundUser = await APIClient.GetUser(_httpClientFactory, username);
                        var chefID = foundUser.ChefId ?? 0;
                        var realName = string.Empty;



                        await SecureStorage.Default.SetAsync("auth_token", "aewfawfwaefawef");
                        await SecureStorage.Default.SetAsync("user_name", foundUser.UserName);

                        if (foundUser.Email != null && foundUser.Email != string.Empty)
                        {
                            await SecureStorage.Default.SetAsync("email", foundUser.Email);
                        }

                        await SecureStorage.Default.SetAsync("chef_guid", Convert.ToString(chefID));

                        if (realName != null && realName != string.Empty)
                        {
                            await SecureStorage.Default.SetAsync("real_name", realName);
                        }


                        var dbUserLocal = await _authService.GetUser(_connection, foundUser.UserName);

                        if (dbUserLocal == null)
                        {
                            // insert 
                            var userDto = new UserDTO();

                            userDto.UserId = Random.Shared.Next();
                            userDto.UserName = foundUser.UserName;

                            await _recipeService.InsertUser(userDto);

                            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
                        }
                        else
                        {
                            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
                        }

                        /*var username = "cameron";
                        var email = "camerongordon93@gmail.com";
                        var chef_guid = "1";
                        var realName = "Cameron Gordon"; 

                        await SecureStorage.Default.SetAsync("auth_token", "awefawefawef");
                        await SecureStorage.Default.SetAsync("user_name", username);
                        await SecureStorage.Default.SetAsync("email", email); // chef email and email should be the same
                        await SecureStorage.Default.SetAsync("chef_guid", chef_guid);
                        await SecureStorage.Default.SetAsync("real_name", realName);
         **/

                        /*await SecureStorage.Default.SetAsync("auth_token", "awefawefawef");
                        await SecureStorage.Default.SetAsync("user_name", "gordoncm2");
                        await SecureStorage.Default.SetAsync("email", "chef32@example.com"); // chef email and email should be the same
                        await SecureStorage.Default.SetAsync("chef_guid", "2");
                        await SecureStorage.Default.SetAsync("real_name", "Cameron Gordon");

                       ;**/
                    }
                    else
                    {
                        // no internet
                    }

                }
            }
            else
            {
                // display popup
            }
        }

        protected override bool OnBackButtonPressed()
        {
            // return true → disables back button
            return true;
        }

        private async void OnSignUpTapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new RegisterUser()); 
        }
    }

}
