using RecipePOC.DB;
using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using SQLite;

namespace RecipePOC;

public partial class RegisterUser : ContentPage
{
    private IAuthService _authService;
    private IHttpClientFactory _theFactory;
    private IRecipeService _recipeService; 

	public RegisterUser()
	{
		InitializeComponent();

        _theFactory = MauiProgram.Services.GetRequiredService<IHttpClientFactory>();
        _authService = MauiProgram.Services.GetService<IAuthService>();
        _recipeService = MauiProgram.Services.GetService<IRecipeService>();
    }
    protected override bool OnBackButtonPressed()
    { 
        return true;
    }


    private async void OnRegisterClicked(object sender, EventArgs e)
    { 
        var username = UserName.Text; 
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPassword.Text;

        SQLiteAsyncConnection _connection = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);

        if (password != null)
        {
            if (password != string.Empty)
            {
                if (password == confirmPassword)
                {
                    var user = new LoginRequestDto();
                    user.Password = password; 
                    user.Username = username;

                    // string for alert
                    var result = await _authService.RegisterUser(_connection, user);

                    var foundUser = await APIClient.GetUser(_theFactory, username);

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
                }
                else
                {
                    await DisplayAlert("Password Mismatch", "Check confirm password", "Cancel");

                }
            }
            else
            {
                await DisplayAlert("Password Error", "Password cant be blank", "Cancel"); 
            }
        }
        else
        {
            await DisplayAlert("No password entered", "You didn't enter in a password!", "Cancel"); 
        }
    }
}