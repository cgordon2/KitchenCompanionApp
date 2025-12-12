using RecipePOC.DB;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using SQLite;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RecipePOC;

public partial class Search : ContentPage, INotifyPropertyChanged
{
    public List<Recipe> AllRecipes { get; set; }
    private IRecipeService _recipeService;
    private INotificationService _notificationsService;
    private int _notifCount; 

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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Search(IRecipeService recipeService, INotificationService service)
	{
		InitializeComponent();

        _recipeService = recipeService;
        _notificationsService = service;

        BindingContext = this; 

        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsEnabled = false,
            IsVisible = false
        });

        currentFilter = Enum.Parse<RecipeFilter>(
                        Preferences.Get("LastFilter", "All"));

        UpdateUIStyles_FromStartup();

 
    }

    public enum RecipeFilter
    {
        All,
        Yours,
        Recent
    }

    bool _loading = false;


    protected override void OnAppearing()
    {
        base.OnAppearing();



        /*if (currentFilter == RecipeFilter.All)
            await LoadNextPage_All();
        else if (currentFilter == RecipeFilter.Yours)
            await LoadNextPage_Yours();
        else if (currentFilter == RecipeFilter.Recent)
            await LoadNextPage_Recent();**/

        _ = OnAppearingAsync();
    }

    private async Task OnAppearingAsync()
    {
        if (_loading)
            return;

        _loading = true;

        var chefGuid = await SecureStorage.GetAsync("chef_guid"); 
        var notifs = await _notificationsService.GetNotifications(false, chefGuid);

        NotifCount = notifs.Count; 

        ResetPaging();

        try
        {
            if (currentFilter == RecipeFilter.All)
                await LoadNextPage_All();
            else if (currentFilter == RecipeFilter.Yours)
                await LoadNextPage_Yours();
            else if (currentFilter == RecipeFilter.Recent)
                await LoadTaggedRecipes(); 
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task LoadNextPage_Recent()
    {
        RecipesList.ItemsSource = null; 
        RecipesList.Footer = null;
    }

    private void ResetPaging()
    {
        currentPage = 0;
        hasMoreData = true; 
            isLoading = false;
        RecipeBuffer.Clear();

        RecipesList.ItemsSource = null;   // <-- KEY
        RecipesList.Footer = null;        // <-- KEY
    }

    private async Task LoadNextPage_Yours()
    {
        var email = await SecureStorage.GetAsync("email"); 

        if (currentFilter != RecipeFilter.Yours)
            return;
        if (isLoading || !hasMoreData)
            return;

        isLoading = true;

        var dbResult = await _recipeService.GetRecipes(
            false, true, false, false, email, currentPage, pageSize);

        var batch = ParseResponse(dbResult);

        if (batch.Count < pageSize)
            hasMoreData = false;

        currentPage++;

        RecipeBuffer.AddRange(batch);

        RecipesList.ItemsSource = null;
        RecipesList.ItemsSource = RecipeBuffer;

        /// RecipesList.Footer = hasMoreData ? CreateLoadMoreFooter() : null;
        /// 
        RecipesList.Footer =
     (RecipeBuffer.Count > 0 && hasMoreData)
         ? CreateLoadMoreFooter()
         : null;

        isLoading = false;
    }

    private int currentPage = 0;
    private int pageSize = 2;
    private bool isLoading = false;
    private bool hasMoreData = true;

    private List<Recipe> RecipeBuffer = new(); // contains all recipes accumulated

    private View CreateLoadMoreFooter()
    {
        return new StackLayout
        {
            Padding = new Thickness(12),
            HorizontalOptions = LayoutOptions.Center,
            Children =
        {
            new Button
            {
                Text = "Load more",
                FontSize = 14,
                CornerRadius = 18,
                WidthRequest = 140,
                HeightRequest = 36,
                BackgroundColor = Color.FromArgb("#222222"),
                TextColor = Colors.White,
                BorderColor = Color.FromArgb("#444444"),
                BorderWidth = 1,
                Shadow = new Shadow
                {
                    Opacity = 0.3f,
                    Radius = 6,
                    Offset = new Point(0,2),
                },
                Command = new Command(async () =>
                {
                    if (currentFilter == RecipeFilter.All)
                        await LoadNextPage_All();
                    else if (currentFilter == RecipeFilter.Yours)
                        await LoadNextPage_Yours();
                    else if (currentFilter == RecipeFilter.Recent)
                        await LoadTaggedRecipes();
                })
            }
        }
        };
    }

    /*private View CreateLoadMoreFooter()
    {
        return new StackLayout
        {
            Padding = new Thickness(10),
            Children =
        {
            new Button
            {
                Text = "Load More",
                BackgroundColor = Colors.Black,
                TextColor = Colors.White,
                Command = new Command(async () =>
                {
                    if (currentFilter == RecipeFilter.All)
                        await LoadNextPage_All();
                    if (currentFilter == RecipeFilter.Yours)
                        await LoadNextPage_Yours();
                    if (currentFilter == RecipeFilter.Recent)
                        await LoadTaggedRecipes(); 
                })
            }
        }
        };
    }**/

    private async Task LoadNextPage_All()
    {
        if (currentFilter != RecipeFilter.All)
            return;

        if (isLoading || !hasMoreData)
            return;

        isLoading = true;

        var dbResult = await _recipeService.GetRecipes(
            true, false, false, false, "", currentPage, pageSize);

        var batch = ParseResponse(dbResult);

        // If fewer than requested — no more data
        if (batch.Count < pageSize)
            hasMoreData = false;

        currentPage++;

        RecipeBuffer.AddRange(batch);

        RecipesList.ItemsSource = null;
        RecipesList.ItemsSource = RecipeBuffer;

        //RecipesList.Footer = hasMoreData ? CreateLoadMoreFooter() : null;
        RecipesList.Footer =
    (RecipeBuffer.Count > 0 && hasMoreData)
        ? CreateLoadMoreFooter()
        : null;
        isLoading = false;
    }

    private async Task LoadTaggedRecipes()
    {
        var email = await SecureStorage.GetAsync("email");

        if (currentFilter != RecipeFilter.Recent)
            return;

        var yourTaggedRecipes = await _recipeService.GetTaggedRecipes(email);

        foreach (var recipe in yourTaggedRecipes)
        {
            var recipeDto = new Recipe();

            recipeDto.Title = recipe.RecipeName;
            recipeDto.CreatedBy = recipe.ChefName;
            DateTime dt = DateTime.Parse(recipe.CreatedAt);
            string dateOnly = dt.ToString("yyyy-MM-dd");

            recipeDto.CreatedOn = dateOnly;

            RecipeBuffer.Add(recipeDto); 
        }

        if (RecipeBuffer.Count < pageSize)
            hasMoreData = false;

        currentPage++;

        RecipesList.ItemsSource = null;
        RecipesList.ItemsSource = RecipeBuffer;

        //RecipesList.Footer = hasMoreData ? CreateLoadMoreFooter() : null;
        RecipesList.Footer =
    (RecipeBuffer.Count > 0 && hasMoreData)
        ? CreateLoadMoreFooter()
        : null;
        isLoading = false;
    }


    private async void LoadYourRecipes()
    { 
        // TODO: FIX ME! RECIPEGUID
        var allRecipes = await _recipeService.GetRecipes(false, true, false, false, "chef@example.com", 0, 0);
        var allRecipesDto = new List<Recipe>();

        foreach (var recipe in allRecipes)
        {
            var recipeDto = new Recipe();

            recipeDto.Title = recipe.RecipeName;
            recipeDto.CreatedBy = recipe.ChefName;

            if (recipe.CreatedAt != null)
            {
                DateTime dt = DateTime.Parse(recipe.CreatedAt);
                string dateOnly = dt.ToString("yyyy-MM-dd");

                recipeDto.CreatedOn = dateOnly;
            }

            allRecipesDto.Add(recipeDto);
        }

        RecipesList.ItemsSource = allRecipesDto;
    }

    private async void LoadAllRecipes()
    {
        var allRecipes = await _recipeService.GetRecipes(true, false, false, false, "", 0, 0);
        var allRecipesDto = new List<Recipe>();

        foreach (var recipe in allRecipes)
        {
            var recipeDto = new Recipe();

            recipeDto.Title = recipe.RecipeName;
            recipeDto.CreatedBy = recipe.ChefName;

            if (recipe.CreatedAt != null)
            {
                DateTime dt = DateTime.Parse(recipe.CreatedAt);
                string dateOnly = dt.ToString("yyyy-MM-dd");

                recipeDto.CreatedOn = dateOnly;
            }

            allRecipesDto.Add(recipeDto);
        }

        RecipesList.ItemsSource = allRecipesDto;
    }

    private void UpdateUIStyles_FromStartup()
    {
        // Reset all filters first
        AllFilter.BackgroundColor = Color.FromArgb("#F9EDEC");
        YourRecipesFilter.BackgroundColor = Color.FromArgb("#F9EDEC");
        RecentlyFilter.BackgroundColor = Color.FromArgb("#F9EDEC");

        // Apply selected one
        switch (currentFilter)
        {
            case RecipeFilter.All:
                AllFilter.BackgroundColor = Color.FromArgb("#DB2C78");
                break;

            case RecipeFilter.Yours:
                YourRecipesFilter.BackgroundColor = Color.FromArgb("#DB2C78");
                break;

            case RecipeFilter.Recent:
                RecentlyFilter.BackgroundColor = Color.FromArgb("#DB2C78");
                break;
        }
    }

    private RecipeFilter currentFilter = RecipeFilter.All;


    public async void OnFilterTapped(object sender, TappedEventArgs e)
    {
        string selected = (string)e.Parameter;

        switch (selected)
        {
            case "All":
                currentFilter = RecipeFilter.All;
                break;

            case "Your Recipes":
                currentFilter = RecipeFilter.Yours;
                break;

            case "Recently Used":
                currentFilter = RecipeFilter.Recent;
                break;
        }
         
        Preferences.Set("LastFilter", currentFilter.ToString());

        ResetPaging(); 

        UpdateUIStyles(sender, e);
        await LoadFilteredRecipes();

    }

    private async Task LoadFilteredRecipes()
    { 
        if (currentFilter == RecipeFilter.All)
        {
            await LoadNextPage_All(); 
        }
        else if (currentFilter == RecipeFilter.Yours)
        {
            await LoadNextPage_Yours(); 
        }
        else if (currentFilter == RecipeFilter.Recent)
        {
            await LoadTaggedRecipes(); 
        }
    }

    private void UpdateUIStyles(object sender, TappedEventArgs e)
    {
        var selected = (string)e.Parameter;

        // Reset all background colors
        AllFilter.BackgroundColor = Color.FromArgb("#F9EDEC");
        YourRecipesFilter.BackgroundColor = Color.FromArgb("#F9EDEC");
        RecentlyFilter.BackgroundColor = Color.FromArgb("#F9EDEC");

        // Apply dark pink to selected
        if (sender == AllFilter)
            AllFilter.BackgroundColor = Color.FromArgb("#DB2C78");

        else if (sender == YourRecipesFilter)
            YourRecipesFilter.BackgroundColor = Color.FromArgb("#DB2C78");

        else if (sender == RecentlyFilter)
            RecentlyFilter.BackgroundColor = Color.FromArgb("#DB2C78");
    }

    public class Recipe
    {
        public string Title { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;

        public string CreatedOn { get; set; }
    }

    public async void OnSearchButtonPressed(object sender, EventArgs e)
    {
        var keyword = RecipeSearchBar.Text?.ToLower() ?? "";

        if (currentFilter == RecipeFilter.All)
        {
            var result = await  _recipeService.SearchRecipes(keyword, true, false, false, "");

            RecipesList.ItemsSource = ParseResponse(result); 
        }
        else if (currentFilter == RecipeFilter.Yours)
        {
            var result = await _recipeService.SearchRecipes(keyword, false, true, false, "chef@example.com");

            RecipesList.ItemsSource = ParseResponse(result); 

        }
        else if (currentFilter == RecipeFilter.Recent)
        {
            RecipesList.ItemsSource = new List<Recipe>(); 
        }
    }

    private List<Recipe> ParseResponse(List<DB.Models.Recipe> recipes)
    {
        var filteredRecipes = new List<Recipe>(); 

        foreach (var recipe in recipes)
        {
            var recipe2 = new Recipe();

            recipe2.Title = recipe.RecipeName;
            recipe2.CreatedBy = recipe.ChefName; 

            filteredRecipes.Add(recipe2);
        }


        return filteredRecipes; 
    }

    bool _isNavigating = false;

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;

        var btn = (Button)sender;
        btn.IsEnabled = false;

        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.GoToAsync("//HomePage");
            });
        }
        finally
        {
            btn.IsEnabled = true;
        }

        _isNavigating = false;
        //await Shell.Current.GoToAsync("//HomePage"); 
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        // nothing here for search
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        //await Shell.Current.GoToAsync(nameof(CreateRecipe));
        await Shell.Current.GoToAsync("//CreateRecipe");

    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        //await Shell.Current.GoToAsync(nameof(AIAssistantShopping));
        await Shell.Current.GoToAsync(nameof(AIAssistantShopping));

    }

    private async void Button_Clicked_4(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(Profile));
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateRecipe));
    }
}