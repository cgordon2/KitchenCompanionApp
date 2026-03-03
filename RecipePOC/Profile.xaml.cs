using RecipePOC.DB;
using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using SQLite;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace RecipePOC;

public partial class Profile : ContentPage, INotifyPropertyChanged
{
    private IAuthService _authService;
    private IRecipeService _recipeService; 
    private SQLiteAsyncConnection _connection;
    public event PropertyChangedEventHandler PropertyChanged;
    private IHttpClientFactory _theFactory; 

    private string _profileName;
    private string _combinedFacts;
    private string _realName; 

    private string _totalRecipes; 
    private string _followerCount;
    private string _followingCount;
    private string _joinedDate;
    private string _location;
    private string _avatarUrl;

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

    public string AvatarUrl
    {
        get => _avatarUrl; 
        set
        {
            if (_avatarUrl != value)
            {
                _avatarUrl = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvatarUrl)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserAvatar)));
            }
        }
    }

    public string RealName
    {
        get => _realName;
        set
        {
            if (_realName != value)
            {
                _realName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RealName)));
            }
        }
    }

    public string Location
    {
        get => _location;
        set
        {
            if (_location != value)
            {
                _location = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Location)));
            }
        }
    }

    public string joinedDate
    {
        get => _joinedDate;
        set
        {
            if (_joinedDate != value)
            {
                _joinedDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(joinedDate)));
            }
        }
    }

    public string totalRecipes
    {
        get => _totalRecipes;
        set
        {
            if (_totalRecipes != value)
            {
                _totalRecipes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(totalRecipes)));
            }
        }
    }

    public static string AddOrdinal(int number)
    {
        if (number <= 0) return number.ToString();

        switch (number % 100)
        {
            case 11:
            case 12:
            case 13:
                return number + "th";
        }

        switch (number % 10)
        {
            case 1: return number + "st";
            case 2: return number + "nd";
            case 3: return number + "rd";
            default: return number + "th";
        }
    }

    public string followerCount
    {
        get => _followerCount;
        set
        {
            if (_followerCount != value)
            {
                _followerCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(followerCount)));
            }
        }
    }

    public string followingCount
    {
        get => _followingCount;
        set
        {
            if (_followingCount != value)
            {
                _followingCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(followingCount)));
            }
        }
    }

    public string ProfileName
    {
        get => _profileName;
        set
        {
            if (_profileName != value)
            {
                _profileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProfileName)));
            }
        }
    }
    public string combinedFacts
    {
        get => _combinedFacts;
        set
        {
            if (_combinedFacts != value)
            {
                _combinedFacts = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(combinedFacts)));
            }
        }
    }



    public class SettingItem
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
    }
    
    public Profile()
	{
		InitializeComponent();

        _authService = MauiProgram.Services.GetService<IAuthService>();
        _recipeService = MauiProgram.Services.GetService<IRecipeService>();
        _connection = new SQLiteAsyncConnection(DBConstants.DatabasePath, DBConstants.Flags);
        _theFactory = MauiProgram.Services.GetRequiredService<IHttpClientFactory>();

        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsEnabled = false,
            IsVisible = false
        }); 

        BindingContext = this; 
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = OnAppearingAsync();
    }

    private async Task OnAppearingAsync()
    {
        var username = await SecureStorage.GetAsync("user_name"); 
        var user = await _authService.GetUser(_connection, username);

        if (user.Location != null && user.Location != string.Empty)
        {
            Location = user.Location; 
        }
        else
        {
            Location = "N/A Location"; 
        }

            RealName = user.real_name;

        ProfileName = "@"+user.UserName;
        
        if (user.AvatarUrl == null)
        {
            AvatarUrl = "user_ico.png"; 
        }
        else
        {
            AvatarUrl = user.AvatarUrl;
        }

        combinedFacts = user.ShortBio; 

        var email = await SecureStorage.GetAsync("email");
        var chefId = await SecureStorage.GetAsync("chef_guid");

        var followers = await APIClient.GetFollowers(_theFactory, Convert.ToInt16(chefId));
        var following = await APIClient.GetFollowing(_theFactory, Convert.ToInt32(chefId));

        followerCount = followers.Count + " Followers";
        followingCount = following.Count + " Following"; 

        var recipesCount = await _recipeService.GetRecipesCount(email);
        int recipesCount2 = recipesCount.Count;  

        totalRecipes = Convert.ToString(recipesCount2) + " Recipes";

        if (user.Created != null)
        {
            DateTime dt = DateTime.Parse(user.Created);
            string formatted = $"Joined {dt:MMMM yyyy}";

            joinedDate = formatted;
        }
    }

    public class SettingOption
    {
        public string Icon { get; set; }
        public string Title { get; set; }

        public bool IsNotificationsEnabled { get; set; }

        public bool ShowSwitch => Title == "Notifications";
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync("//HomePage");
        });

    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Search");

    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateRecipe));

    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AIAssistantShopping));

    }

    private void Button_Clicked_4(object sender, EventArgs e)
    {
        // we are already here
    }

    /**
     *     public class UserDTO
    {
        public int UserId { get; set; } 
        public string UserName { get; set; } = string.Empty; 
        public string? Password { get; set; } 
        public string? ConfirmPassword { get; set; } 
        public string? Email { get; set; } 

        public bool IsSetup { get; set; } 

        public int? ChefId { get; set; } 

        public string? RealName { get; set; }
        public string? ShortBio { get; set; } 
        public string? Location { get; set; } 

        public string? Language { get; set; }
        
        public string? AvatarUrl { get; set; } 
    }
     * **/

    private async void EditButton_Clicked(object sender, EventArgs e)
    {
        var chefGuid = await SecureStorage.GetAsync("chef_guid");
        var userName = await SecureStorage.GetAsync("user_name");
        var email = await SecureStorage.GetAsync("email");

        var userDto = new UserDTO()
        {
            UserName = userName,
            Email = email,
            IsSetup = true,
            NavigateToProfile = true
        }; 

        await Navigation.PushAsync(new SetupProfile(userDto, _theFactory)); 
    } 

    private async void OnLogoutTapped(object sender, TappedEventArgs e)
    {
        SecureStorage.Default.Remove("auth_token");
        SecureStorage.Default.Remove("user_name");
        SecureStorage.Default.Remove("email");
        SecureStorage.Default.Remove("chef_guid");
        SecureStorage.Default.Remove("real_name");
        SecureStorage.Default.Remove("selected_ingredients"); // 
        SecureStorage.Default.Remove("selected_recipe");
        SecureStorage.Default.Remove("should_clone_or_edit"); 

        await Navigation.PushAsync(new MainPage(_authService, _recipeService, _theFactory, _connection));  
    }

    private async void TapGestureRecognizer_Pantry(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new Pantry());
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new UserDirectory()); 
    }

    private async void RefreshButtonTapped(object sender, EventArgs e)
    {
        var username = await SecureStorage.GetAsync("user_name"); 
        var user = await APIClient.GetUser(_theFactory, username);

        await _authService.UpdateAvatarUrl(username, user.AvatarUrl, _connection);

        AvatarUrl = user.AvatarUrl; 

        Console.WriteLine(user); 
    }
}