using RecipePOC.Services;
using System.Threading.Tasks;

namespace RecipePOC
{
    public partial class MainPage : ContentPage
    {
        private readonly IAuthService _authService; 

        public MainPage(IAuthService authService)
        {
            InitializeComponent();

            _authService = authService;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            /** cameron!!! for user**/ 
            var error = await _authService.LoginAsync(new DTOs.LoginRequestDto("cameron", "Hieverybody17$"));

            if (!string.IsNullOrEmpty(error))
            {
                await SecureStorage.Default.SetAsync("auth_token", "awefawefawef");
                await SecureStorage.Default.SetAsync("user_name", "gordoncm");
                await SecureStorage.Default.SetAsync("email", "chef@example.com"); // chef email and email should be the same
                await SecureStorage.Default.SetAsync("chef_guid", "1");
                await SecureStorage.Default.SetAsync("real_name", "Cameron Gordon"); 

                await Shell.Current.GoToAsync($"//{nameof(HomePage)}"); 
                 
                /*await SecureStorage.Default.SetAsync("auth_token", "awefawefawef");
                await SecureStorage.Default.SetAsync("user_name", "gordoncm2");
                await SecureStorage.Default.SetAsync("email", "chef32@example.com"); // chef email and email should be the same
                await SecureStorage.Default.SetAsync("chef_guid", "2");
                await SecureStorage.Default.SetAsync("real_name", "Cameron Gordon");

                await Shell.Current.GoToAsync($"//{nameof(HomePage)}");**/ 

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
