using RecipePOC.DB;
using RecipePOC.DTOs;
using RecipePOC.Services;
using SQLite;

namespace RecipePOC;

public partial class RegisterUser : ContentPage
{
    private IAuthService _authService; 

	public RegisterUser()
	{
		InitializeComponent();

        _authService = MauiProgram.Services.GetService<IAuthService>();
    }
    protected override bool OnBackButtonPressed()
    { 
        return true;
    }


    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var email = emailAddress.Text; 
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
                    var result = _authService.RegisterUser(_connection, user); 
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