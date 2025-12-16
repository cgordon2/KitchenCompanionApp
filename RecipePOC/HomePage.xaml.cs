using Microsoft.Maui.Networking;
using RecipePOC.DB;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static SQLite.SQLite3;

namespace RecipePOC;

public partial class HomePage : ContentPage, INotifyPropertyChanged
{
    private readonly IAuthService _authService; 
    private readonly IRecipeService _recipeService;
    private IHttpClientFactory _httpClientFactory; 
    private readonly INotificationService _notificationsService; 

    private int _notifCount; 
    private string _realName;

    public int NotifCount
    {
        get => _notifCount;
        set
        {
            if (_notifCount != value)
            {
                _notifCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotifCount)));
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    int page = 0;
    int pageSize = 10; 
    ObservableCollection<Recipe> Recipes = new();

    public HomePage(IAuthService authService, IRecipeService recipeService, INotificationService notifService, IHttpClientFactory httpClientFactory)
	{
		InitializeComponent();
        LoadingSpinner.IsVisible = false;

        BindingContext = this; 

        _authService = authService; 
        _recipeService = recipeService;
        _notificationsService = notifService;
        _httpClientFactory = httpClientFactory;

        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsEnabled = false,
            IsVisible = false
        });
    } 


    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = OnAppearingAsync();
    }

    private async Task OnAppearingAsync()
    {
        page = 0;
        Recipes.Clear();
        RecipesList.ItemsSource = null;

        var userName = await SecureStorage.GetAsync("user_name");
        var chefGuid = await SecureStorage.GetAsync("chef_guid");

        RealName = "Hello, @"+userName+"!";  

        if (await SecureStorage.GetAsync("auth_token") == null)
        {
            await Navigation.PushAsync(new MainPage(_authService)); 
        }

        var notifs = await _notificationsService.GetNotifications(false, chefGuid);

        NotifCount = notifs.Count; 


        // Show spinner
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;
        });
        try
        {
            await Task.Delay(1500); // simulate real load

            if (PageHelpers.HasInternet())
            {
                var allRecipes = await APIClient.GetAllRecipes(_httpClientFactory);
                await _recipeService.ResetRecipes(allRecipes); 

                await LoadNextPage(); 
            }
            else
            {
                await LoadNextPage();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR in OnAppearingAsync: " + ex);
        }
        finally
        {
            // Hide spinner
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingSpinner.IsRunning = false;
                LoadingSpinner.IsVisible = false;
            });
        }
    }

    private async Task LoadNextPage()
    {
        var email = await SecureStorage.GetAsync("email"); 
        // Ask DB for only items for this page
        var newRecipes = await _recipeService.GetRecipes(false, false, false, true, email, page, pageSize);

        foreach (var recipe in newRecipes)
        {
            Recipes.Add(new Recipe
            {
                Title = recipe.RecipeName,
                Description = recipe.Description, 
                UserName = recipe.ChefName,
                RecipeGUID = recipe.RecipeGuid
            });
        }

        for (int i = 0; i < Recipes.Count; i++)
            Recipes[i].Index = i % 2;

        RecipesList.ItemsSource = Recipes;

        // Show footer only if there are possibly more items
        RecipesList.Footer = (newRecipes.Count == pageSize)
            ? CreateLoadMoreFooter()
            : null;

        page++;
    }

    private View CreateLoadMoreFooter()
    {
        return new StackLayout
        {
            Padding = 10,
            VerticalOptions = LayoutOptions.Start,
            Children =
        {
            new Button
            {
                Text = "Load More",
                BackgroundColor = Colors.Black,
                CornerRadius = 10,
                TextColor = Colors.White,
                WidthRequest = 150, 
                HorizontalOptions = LayoutOptions.Center,
                Command = new Command(async () => await LoadNextPage())
            }
        }
        };
    } 

    private async void OnRecipesTapped(object sender, TappedEventArgs e)
    { 
        var email = await SecureStorage.GetAsync("email"); 
        await Navigation.PushAsync(new IngredientsListView(_recipeService, email, false, _httpClientFactory), false); 
    }

    private async void OnShoppingTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new ShoppingList(), false); 
    }

    private async void HomePage_Navigate(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(HomePage));
    }

    private async void Search_Navigate(object sender, EventArgs e)
    {
        // await Shell.Current.GoToAsync("///Search");
        await Shell.Current.GoToAsync("//Search");

    }

    private  async void Profile_Navigate(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(Profile)); 
    }
            
    private async void AI_Plugin(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AIAssistantShopping));
    }



    private async void CreateRecipe_Navigate(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateRecipe));
    }

    private async void NewNotifications_Handler(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(Notifications));
    }

    private async void RecipesList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            return;

        var recipie = (Recipe)e.SelectedItem; 

        await Navigation.PushAsync(new IngredientDetail(recipie, _recipeService));

        ((ListView)sender).SelectedItem = null; // ← important
    }
    private async void OnBellTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(Notifications));
    }
}