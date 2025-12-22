using RecipePOC.DB;
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

        if (user.ShortBio != null)
        {
            string[] facts = user.ShortBio.Split(",");

            if (facts.Length == 3)
            {
                var fact1 = facts[0];
                var fact2 = facts[1];
                var fact3 = facts[2];

                combinedFacts = fact1 + " | " + fact2 + " | " + fact3;
            }
            else if (facts.Length == 2)
            {
                var fact1 = facts[0];
                var fact2 = facts[1];

                combinedFacts = fact1 + " | " + fact2;
            }
            else if (facts.Length == 1)
            {
                var fact1 = facts[0];

                combinedFacts = fact1;
            }
            else if (facts.Length == 4)
            {
                var fact1 = facts[0];
                var fact2 = facts[1];
                var fact3 = facts[2];
                var fact4 = facts[3];

                combinedFacts = fact1 + " | " + fact2 + " | " + fact3 + " | " + fact4; 
            }
        }

        followerCount = Convert.ToString(user.FollowersCount) + " Followers";
        followingCount = Convert.ToString(user.FollowingCount) + " Following";
        totalRecipes = Convert.ToString(user.TotalRecipes) + " Recipes";

        if (user.Created != null)
        {
            DateTime dt = DateTime.Parse(user.Created);
            string formatted = $"Joined {dt:MMMM} {AddOrdinal(dt.Day)} {dt:yyyy}";

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

    private async void EditButton_Clicked(object sender, EventArgs e)
    { 

    }

    private async void OnFollowingTapped(object sender, TappedEventArgs e)
    {
        var followingPage = new Following();

        await Navigation.PushAsync(followingPage);

    }

    private async void OnFollowersTapped(object sender, TappedEventArgs e)
    {
        var followersPage = new Followers(); 

        await Navigation.PushAsync(followersPage);
    }

    private async void OnLogoutTapped(object sender, TappedEventArgs e)
    {
        SecureStorage.Default.Remove("auth_token");
        SecureStorage.Default.Remove("user_name");
        SecureStorage.Default.Remove("email");
        SecureStorage.Default.Remove("chef_guid");
        SecureStorage.Default.Remove("real_name");
        SecureStorage.Default.Remove("selected_ingredients");

        await Navigation.PushAsync(new MainPage(_authService, _recipeService, _theFactory, _connection));  
    }
}