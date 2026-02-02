using RecipePOC.DTOs;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Text; 

namespace RecipePOC;

public partial class UserProfileDirectory : ContentPage, INotifyPropertyChanged
{
    private ObservableCollection<Recipe> Recipes = new(); 
    private IHttpClientFactory _theFactory;
    private UserDTO _user;
    private int page = 1;
    private IRecipeService _recipeService;
    public ICommand CloneCommand { get; }

    private string _realName;
    private string _username;
    private string _shortBio;
    private string _avatarUrl;
    private string _location;

    private string _totalRecipes;
    private string _totalFollowing;
    private string _totalFollowers;
    private string _creationDate;

    public string CreationDate
    {
        get => _creationDate;
        set
        {
            if (_creationDate != value)
            {
                _creationDate = value;
                OnPropertyChanged(nameof(CreationDate));
            }
        }
    }

    public string TotalFollowers
    {
        get => _totalFollowers;
        set
        {
            if (_totalFollowers != value)
            {
                _totalFollowers = value;
                OnPropertyChanged(nameof(TotalFollowers));
            }
        }
    }

    public ImageSource UserAvatar
    {
        get
        {
            const string ApiBaseUrl = "http://192.168.7.203:5285";

            var file = string.IsNullOrWhiteSpace(AvatarURL)
                       || AvatarURL == "user_ico.png"
                       ? "user_ico.png"
                       : AvatarURL;

            return ImageSource.FromUri(
                new Uri($"{ApiBaseUrl}/uploads/{file}")
            );
        }
    }

    public string TotalFollowing
    {
        get => _totalFollowing;
        set
        {
            if (_totalFollowing != value)
            {
                _totalFollowing = value;
                OnPropertyChanged(nameof(TotalFollowing));
            }
        }
    }

    public string TotalRecipes
    {
        get => _totalRecipes;
        set
        {
            if (_totalRecipes != value)
            {
                _totalRecipes = value;
                OnPropertyChanged(nameof(TotalRecipes));
            }
        }
    }

    public string AvatarURL
    {
        get => _avatarUrl;
        set
        {
            if (_avatarUrl != value)
            {
                _avatarUrl = value;
                OnPropertyChanged(nameof(AvatarURL));
                OnPropertyChanged(nameof(UserAvatar)); 
            }
        }
    }

    public string ShortBio
    {
        get => _shortBio;
        set
        {
            if (_shortBio != value)
            {
                _shortBio = value;
                OnPropertyChanged(nameof(ShortBio));
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
                OnPropertyChanged(nameof(RealName));
            }
        }
    }

    public string UserName
    {
        get => _username;
        set
        {
            if (_username != value)
            {
                _username = value;
                OnPropertyChanged(nameof(UserName));
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
                OnPropertyChanged(nameof(Location));
            }
        }
    }
    public ICommand ToggleExpandCommand { get; }

    public UserProfileDirectory(UserDTO user, IHttpClientFactory theFactory)
    {
        InitializeComponent();

        _recipeService = MauiProgram.Services.GetService<IRecipeService>(); 
        _theFactory = theFactory;

        ToggleExpandCommand = new Command<Recipe>(item =>
        {
            item.IsExpanded = !item.IsExpanded;
        });
        BindingContext = this;

        _user = user;
        Recipes.Clear();
        page = 1;

        CloneCommand = new Command<Recipe>(OnClone);

        AvatarURL = user.AvatarUrl;
        Location = user.Location;
        ShortBio = user.ShortBio;
        UserName = "@" + user.UserName; 
        RealName = user.RealName; 

        CreationDate = "October 2025"; 
        TotalFollowers = "1 Follower";
        TotalFollowing = "1 Following";

    }

    private async void OnClone(Recipe recipe)
    {
        if (recipe == null)
            return;

        var username = await SecureStorage.GetAsync("user_name");
        var email = await SecureStorage.GetAsync("email"); 

        var CategoryMap = new Dictionary<string, int>
                        {
                            { "Appetizer", 1 },
                            { "Beverage", 2 },
                            { "Breakfast", 1002 },
                            { "Brunch", 1003 },
                            { "Dessert", 1004 },
                            { "Main Dish", 1005 },
                            { "Side Dish", 2002 },
                            { "Snack", 2003 }
                        };

        var recipeDto = new RecipeDto();

        recipeDto.RecipeName = recipe.Title;
        recipeDto.Description = recipe.Description;
        recipeDto.ChefName = username;
        recipeDto.IsCloned = true;
        recipeDto.Favorite = "No";
        recipeDto.Photo = "food.jpg";
        recipeDto.CookTime = Convert.ToInt32(recipe.CookTime);
        recipeDto.Serves = Convert.ToInt32(recipe.Serves); 
        recipeDto.Stars = Convert.ToInt32(recipe.Stars);
        recipeDto.Category = Convert.ToString(CategoryMap[recipe.Category]);
        recipeDto.Prep = recipe.Prep; 
        recipeDto.RecipeID = Convert.ToInt32(recipe.RecipeGUID);
        recipeDto.ChefEmail = email; 

        recipeDto.RecipeIngredients = recipe.RecipeIngredients;

        await APIClient.CreateRecipe(_theFactory, recipeDto);

        await DisplayAlert("Clone Success", "You cloned this recipe.", "Ok"); 
    }



    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var test = await APIClient.GetRecipesByUserId(_theFactory, page,_user.UserId);

        foreach (var t in test)
        {
            var recipe = new Recipe();

            recipe.Title = t.RecipeName;
            recipe.Description = t.Description;
            recipe.UserName = UserName;
            recipe.Photo = t.Photo;
            recipe.CookTime = t.CookTime;
            recipe.Serves = t.Serves;
            recipe.Prep = t.Prep;
            recipe.RecipeGUID = Convert.ToString(t.RecipeID); 
            recipe.RecipeIngredients = t.RecipeIngredients;
            recipe.Stars = t.Stars;
            recipe.Category = t.Category;
            recipe.ChefEmail = t.ChefEmail;
            var sb = new StringBuilder();

            // base description
            sb.AppendLine(t.Description);
            sb.AppendLine(); // blank line after description (optional)

            // append ingredients
            foreach (var ri in t.RecipeIngredients)
            {
                sb.AppendLine(ri.ingredientName);
            }

            // assign once
            recipe.ExtraInfo = sb.ToString();

            Recipes.Add(recipe);
        }

        RecipesList.ItemsSource = Recipes;

        var recipesCount = await _recipeService.GetRecipesCount(_user.Email);
        int recipesCount2 = recipesCount.Count;

        if (recipesCount2 == 1)
        {
            TotalRecipes = recipesCount2 + " Recipe"; 
        }
        else
        {
            TotalRecipes = recipesCount2 + " Recipes"; 
        }

        var following = await APIClient.GetFollowers(_theFactory, _user.UserId); 
        var followers = await APIClient.GetFollowing(_theFactory , _user.UserId);

        var followingCount = following.Count + " Following";
        var followerCount = followers.Count + " Followers";

        TotalFollowers = followerCount; 
        TotalFollowing = followingCount;
    }
}