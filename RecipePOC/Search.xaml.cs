using RecipePOC.DB;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RecipePOC;

public partial class Search : ContentPage
{
    public List<Recipe> AllRecipes { get; set; }
    private IHttpClientFactory _httpClientFactory; 
    private IRecipeService _recipeService;
    private INotificationService _notificationsService;
    private View? _loadMoreFooter;

    /*public Search()
    {
        InitializeComponent(); 
        BindingContext = this; 
    }**/

    public Search(IRecipeService recipeService, INotificationService service, IHttpClientFactory httpClientFactory)
	{
		InitializeComponent();

        _recipeService = recipeService;
        _notificationsService = service;
        _httpClientFactory = httpClientFactory; 

        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsEnabled = false,
            IsVisible = false
        });

        BindingContext = this;

        RecipesList.ItemAppearing += RecipesList_ItemAppearing;
    }

    private async void RecipesList_ItemAppearing(object sender, ItemVisibilityEventArgs e)
    {
        if (sender is not ListView list || list.ItemsSource != RecipeBuffer)
            return;

        if (isLoading || !hasMoreData)
            return;

        if (RecipeBuffer.Count == 0)
            return;

        // LAST item check
        if (e.Item == RecipeBuffer[^1])
        {
            if (currentFilter == RecipeFilter.All)
                await LoadNextPage_All();
            else if (currentFilter == RecipeFilter.Yours)
                await LoadNextPage_Yours();
            else if (currentFilter == RecipeFilter.Recent)
                await LoadTaggedRecipes(); 
        }
    }

    public enum RecipeFilter
    {
        All,
        Yours,
        Recent
    }

    bool _loading = false;
    private async void OnLoadMore(object sender, EventArgs e)
    {
        if (isLoading || !hasMoreData)
            return;

        if (currentFilter == RecipeFilter.All)
            await LoadNextPage_All();
        else if (currentFilter == RecipeFilter.Yours)
            await LoadNextPage_Yours();
        else if (currentFilter == RecipeFilter.Recent)
            await LoadTaggedRecipes();
    }

    private async Task EnsureScrollableAsync()
    {
        // Keep loading until:
        // - we can scroll
        // - or there's no more data
        if (isLoading || !hasMoreData)
            return;

        // Load ONE extra page only
        if (currentFilter == RecipeFilter.All)
            await LoadNextPage_All();
    }
    bool _appearingInProgress;
    CancellationTokenSource _appearingCts;
    volatile bool _pageAlive;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _pageAlive = true;
        SecureStorage.Default.Remove("should_clone_or_edit");
        SecureStorage.Default.Remove("selected_recipe");
        SecureStorage.Default.Remove("selected_ingredients");

        SecureStorage.Default.Remove("recipe_title");
        SecureStorage.Default.Remove("recipe_description");
        SecureStorage.Default.Remove("selected_category");

        SecureStorage.Default.Remove("selected_recipe");
        SecureStorage.Default.Remove("selected_ingredients");
        SecureStorage.Default.Remove("should_clone_or_edit");
        SecureStorage.Default.Remove("RecipePhotoName");


        MainThread.BeginInvokeOnMainThread(() =>
        {
            LoadingSearchSpinner.IsVisible = false;
            LoadingSearchSpinner.IsRunning = false;
        });

        if (_appearingInProgress)
            return;

        _appearingInProgress = true;

        currentFilter = Enum.Parse<RecipeFilter>(
                Preferences.Get("LastFilter", "All")); 

        UpdateUIStyles_FromStartup();
        /*if (currentFilter == RecipeFilter.All)
             await LoadNextPage_All();
         else if (currentFilter == RecipeFilter.Yours)
             await LoadNextPage_Yours();
         else if (currentFilter == RecipeFilter.Recent)
             await LoadNextPage_Recent();
        **/
            _appearingCts?.Cancel();
    _appearingCts = new CancellationTokenSource();
        _ = RunOnAppearingAsync(_appearingCts.Token);
    }

    private async Task RunOnAppearingAsync(CancellationToken token)
    {
        try
        {
            await OnAppearingAsync(token);
        }
        catch (OperationCanceledException)
        {
            // Expected when navigating away
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            _appearingInProgress = false;
        }
    }

    private async Task OnAppearingAsync(CancellationToken token)
    {
        if (_loading)
            return;

        _loading = true;

       // var chefGuid = await SecureStorage.GetAsync("chef_guid"); 

       // ResetPaging();


        try
        {
            // Respect cancellation early
            token.ThrowIfCancellationRequested();

            var chefGuid = await SecureStorage.GetAsync("chef_guid");
            token.ThrowIfCancellationRequested();

            await ResetPaging();

            if (currentFilter == RecipeFilter.All)
            {
                await LoadNextPage_All(); 

                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    await EnsureScrollableAsync(); 
                }
            }
            else if (currentFilter == RecipeFilter.Yours)
            {
                await LoadNextPage_Yours();

            }
            else if (currentFilter == RecipeFilter.Recent)
            {
                await LoadTaggedRecipes();
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    await EnsureScrollableAsync();
                }
            }

            token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            // Expected when navigating away — DO NOT log
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            _loading = false;
        }
    }

    readonly SemaphoreSlim _dataGate = new(1, 1);
    int _generation = 0;

    private async Task ResetPaging()
    {
        await _dataGate.WaitAsync();
        try
        {
            _generation++;              // invalidate ALL loads
            currentPage = 0;
            hasMoreData = true;
            isLoading = false;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                RecipeBuffer.Clear();   // now SAFE
            });
        }
        finally
        {
            _dataGate.Release();
        }
    }

    private async Task LoadNextPage_Yours()
    {
        if (!_pageAlive) return;
        if (currentFilter != RecipeFilter.Yours) return;

        int gen;
        int localPage;
        int localPageSize;

        // 1) Enter critical section: decide if load is allowed + snapshot state
        await _dataGate.WaitAsync();
        try
        {
            gen = _generation;

            if (isLoading || !hasMoreData)
                return;

            isLoading = true;

            localPage = currentPage;
            localPageSize = pageSize;
        }
        finally
        {
            _dataGate.Release();
        }

        List<Recipe> batch;

        try
        {
            await Task.Yield();

            var email = await SecureStorage.GetAsync("email");

            var dbResult = await _recipeService.GetRecipes(
                false, true, false, false, email, localPage, localPageSize);

            batch = ParseResponse(dbResult);
        }
        finally
        {
            // do NOT clear isLoading here
        }

        // 2) Re-enter critical section: commit results if still valid
        await _dataGate.WaitAsync();
        try
        {
            // If reset or tab switch happened, drop results
            if (gen != _generation || !_pageAlive || currentFilter != RecipeFilter.Yours)
                return;

            if (batch.Count < localPageSize)
                hasMoreData = false;

            currentPage++;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // final safety check on UI thread
                if (gen != _generation || !_pageAlive || currentFilter != RecipeFilter.Yours)
                    return;

                foreach (var item in batch)
                    RecipeBuffer.Add(item);
            });
        }
        finally
        {
            isLoading = false;
            _dataGate.Release();
        }
        /*if (!_pageAlive)
            return;

        if (currentFilter != RecipeFilter.Yours)
            return;

        if (isLoading || !hasMoreData)
            return;


        isLoading = true;

        try
        {
            await Task.Yield();

            var email = await SecureStorage.GetAsync("email");

            var dbResult = await _recipeService.GetRecipes(
                false, true, false, false, email, currentPage, pageSize);

            var batch = ParseResponse(dbResult);

            if (batch.Count < pageSize)
                hasMoreData = false;

            currentPage++;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (!_pageAlive)
                    return;
                foreach (var item in batch)
                    RecipeBuffer.Add(item);
            });
        }
        finally
        {
            isLoading = false;
        }**/

        /*var email = await SecureStorage.GetAsync("email"); 

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
         
        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach (var item in batch)
                RecipeBuffer.Add(item);
        }); 

        isLoading = false;**/
    }

    private int currentPage = 0;
    private int pageSize = 20; 
    private bool isLoading = false;
    private bool hasMoreData = true;

    public ObservableCollection<Recipe> RecipeBuffer { get; }
        = new ObservableCollection<Recipe>(); 

    private async Task LoadNextPage_All()
    {
        if (!_pageAlive) return;
        if (currentFilter != RecipeFilter.All) return;

        int gen;
        int localPage;
        int localPageSize;

        // 1) Enter critical section: decide whether we can load + snapshot state
        await _dataGate.WaitAsync();
        try
        {
            gen = _generation;

            if (isLoading || !hasMoreData)
                return;

            isLoading = true;

            // Snapshot paging inputs so they can’t change mid-request
            localPage = currentPage;
            localPageSize = pageSize;

            if (DeviceInfo.Platform == DevicePlatform.WinUI)
                localPageSize = 10; // don't mutate shared pageSize here
        }
        finally
        {
            _dataGate.Release();
        }

        List<Recipe> batch;
        try
        {
            await Task.Yield();

            var dbResult = await _recipeService.GetRecipes(
                true, false, false, false, "", localPage, localPageSize);

            batch = ParseResponse(dbResult);
        }
        finally
        {
            // do not set isLoading=false here; must happen after we commit or abort under gate
        }

        // 2) Re-enter critical section: commit results if still valid
        await _dataGate.WaitAsync();
        try
        {
            // If page/filter reset happened while we were awaiting, drop results
            if (gen != _generation || !_pageAlive || currentFilter != RecipeFilter.All)
                return;

            if (batch.Count < localPageSize)
                hasMoreData = false;

            currentPage++;

            // Important: mutate collection on UI thread
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // last-chance validity check
                if (gen != _generation || !_pageAlive || currentFilter != RecipeFilter.All)
                    return;

                foreach (var item in batch)
                    RecipeBuffer.Add(item);
            });
        }
        finally
        {
            isLoading = false;
            _dataGate.Release();
        }

       /* await _dataGate.WaitAsync();
        try
        {
            gen = _generation;

            if (isLoading || !hasMoreData)
                return;

            isLoading = true;
        }
        finally
        {
            _dataGate.Release();
        }
        if (!_pageAlive)
            return;

        if (!_pageAlive)
            return;

        if (currentFilter != RecipeFilter.All)
            return;

        if (isLoading || !hasMoreData)
            return;


        isLoading = true;

        try
        {
            await Task.Yield();

            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                pageSize = 10; 
            }
            var dbResult = await _recipeService.GetRecipes(
                true, false, false, false, "", currentPage, pageSize);

            var batch = ParseResponse(dbResult);

            await _dataGate.WaitAsync();

            try
            {
                if (gen != _generation)
                    return; // reset happened
                // If fewer than requested — no more data
                if (batch.Count < pageSize)
                    hasMoreData = false;

                currentPage++;

                // UI mutation must be awaited
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (gen != _generation)
                        return;
                    foreach (var item in batch)
                        RecipeBuffer.Add(item);
                });
            }
            finally
            {
                _dataGate.Release();
            }
        }
        finally
        {
            // Only clear AFTER UI is done
            isLoading = false;
        }**/

        /*if (currentFilter != RecipeFilter.All)
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
         
        MainThread.BeginInvokeOnMainThread(() =>
        { 
            foreach (var item in batch)
                RecipeBuffer.Add(item);
        }); 

        isLoading = false;**/
    }

    private async Task LoadTaggedRecipes()
    {
        if (!_pageAlive) return;
        if (currentFilter != RecipeFilter.Recent) return;

        int gen;
        int localPage;
        int localPageSize;

        // 1) Enter critical section: decide if load is allowed + snapshot state
        await _dataGate.WaitAsync();
        try
        {
            gen = _generation;

            if (isLoading || !hasMoreData)
                return;

            isLoading = true;

            localPage = currentPage;
            localPageSize = pageSize;
        }
        finally
        {
            _dataGate.Release();
        }

        List<Recipe> batch;

        try
        {
            await Task.Yield();

            var email = await SecureStorage.GetAsync("email");

            var dbResult = await _recipeService.GetRecipes(
                false, false, true, false, email, localPage, localPageSize);

            batch = ParseResponse(dbResult);
        }
        finally
        {
            // do NOT set isLoading=false here
        }

        // 2) Re-enter critical section: commit results if still valid
        await _dataGate.WaitAsync();
        try
        {
            // Drop results if a reset/tab change happened
            if (gen != _generation || !_pageAlive || currentFilter != RecipeFilter.Recent)
                return;

            if (batch.Count < localPageSize)
                hasMoreData = false;

            currentPage++;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // final safety check on UI thread
                if (gen != _generation || !_pageAlive || currentFilter != RecipeFilter.Recent)
                    return;

                foreach (var item in batch)
                    RecipeBuffer.Add(item);
            });
        }
        finally
        {
            isLoading = false;
            _dataGate.Release();
        }
        /*if (currentFilter != RecipeFilter.Recent)
            return;

        if (isLoading || !hasMoreData)
            return;


        isLoading = true;

        try
        {
            await Task.Yield();

            var email = await SecureStorage.GetAsync("email");

            var dbResult = await _recipeService.GetRecipes(
                false, false, true, false, email, currentPage, pageSize);

            var batch = ParseResponse(dbResult);

            // If fewer than requested — no more data
            if (batch.Count < pageSize)
                hasMoreData = false;

            currentPage++;

            // IMPORTANT: await UI mutation
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var item in batch)
                    RecipeBuffer.Add(item);
            });
        }
        finally
        {
            // Clear loading ONLY after UI work completes
            isLoading = false;
        }**/
        /* var email = await SecureStorage.GetAsync("email");

         if (currentFilter != RecipeFilter.Recent)
             return;

         if (isLoading || !hasMoreData)
             return;

         isLoading = true;

         var dbResult = await _recipeService.GetRecipes(false, false, true, false, email, currentPage, pageSize);

         var batch = ParseResponse(dbResult);

         // If fewer than requested — no more data
         if (batch.Count < pageSize)
             hasMoreData = false;

         currentPage++;
         MainThread.BeginInvokeOnMainThread(() =>
         {
             foreach (var item in batch)
                 RecipeBuffer.Add(item);
         }); 
         isLoading = false; **/
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

        await ResetPaging(); 

        UpdateUIStyles(sender, e);
        await LoadFilteredRecipes();

    }

    private async Task LoadFilteredRecipes()
    { 
        try
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
        catch (Exception ex)
        {
            Console.WriteLine(ex); 
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
        public string Photo { get; set; } = string.Empty; 
        public int Stars { get; set; } 
        public int CookTime { get; set; }
        public string UserName { get; set; } 
        public int Serves { get; set; } 
        public int Prep { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;

        public string CreatedOn { get; set; }

        public string RecipeGUID { get; set; } 

        public string Favorite { get; set; } 
        public string? Description { get; set; } 

        public int Index { get; set; }
        public ImageSource RecipePhoto
        {
            get
            {
                // fallback image from Resources/Images
                if (string.IsNullOrWhiteSpace(Photo) || Photo == "food.jpg")
                    return ImageSource.FromFile("food.jpg");

                const string ApiBaseUrl = "http://192.168.7.203:5285";
                var imageUrl = $"{ApiBaseUrl}/uploads/{Photo}";

                return ImageSource.FromUri(new Uri(imageUrl));
            }
        }
    }

    public async void OnSearchButtonPressed(object sender, EventArgs e)
    {
       /* var keyword = RecipeSearchBar.Text?.ToLower() ?? "";

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
        }**/
    }

    private List<Recipe> ParseResponse(List<DB.Models.Recipe> recipes)
    {
        var filteredRecipes = new List<Recipe>(); 

        foreach (var recipe in recipes)
        {
            var recipe2 = new Recipe();

            recipe2.Title = recipe.RecipeName;
            recipe2.CreatedBy = recipe.ChefName;
            recipe2.RecipeGUID = recipe.RecipeGuid;
            recipe2.Photo = recipe.Photo; 
            recipe2.Stars = recipe.Stars;
            recipe2.CookTime = recipe.CookTime;
            recipe2.Serves = recipe.Serves;
            recipe2.Prep = recipe.Prep;
            recipe2.UserName = recipe.ChefName;
            recipe2.Favorite = recipe.Favorite;
            recipe2.Description = recipe.Description; 
          
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

    private async void SearchList_OnItemChanged(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            return;

        var recipe = e.SelectedItem as RecipePOC.Search.Recipe;

        var username = await SecureStorage.Default.GetAsync("user_name"); 

        if (username == recipe.UserName)
        {
            if (recipe.Favorite == "Yes")
            {
                recipe.Favorite = "Yes"; 
            }
            else
            {
                recipe.Favorite = "No"; 
            }
        }
        else
        {
            recipe.Favorite = "No"; 
        }

            var test = new RecipePOC.Recipe
            {
                Title = recipe.Title,
                UserName = recipe.CreatedBy,
                RecipeGUID = recipe.RecipeGUID,
                Photo = recipe.Photo,
                Stars = recipe.Stars,
                Prep = recipe.Prep,
                CookTime = recipe.CookTime,
                Serves = recipe.Serves,
                Favorite = recipe.Favorite,
                Description = recipe.Description
            }; 

        await Navigation.PushAsync(
            new IngredientDetail(test, _recipeService, _httpClientFactory)
        );

        // Clear selection so it can be tapped again
        ((ListView)sender).SelectedItem = null;
    }

    private async void OnSearchTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new AdvancedSearch()); 
    }

    protected override void OnDisappearing()
    {
        _pageAlive = false;
        _appearingCts?.Cancel();   // you already had this
        base.OnDisappearing();
    }

    private async Task ResetPagingAsync()
    {
        await _dataGate.WaitAsync();
        try
        {
            _generation++;          
            currentPage = 0;
            hasMoreData = true;
            isLoading = false;
        }
        finally
        {
            _dataGate.Release();
        }

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            RecipeBuffer.Clear();
        });
    }

    private async void OnRefreshTapped(object sender, TappedEventArgs e)
    {
        if (PageHelpers.HasInternet())
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingSearchSpinner.IsVisible = true;
                LoadingSearchSpinner.IsRunning = true;
            });

            var allRecipes = await APIClient.GetAllRecipes(_httpClientFactory);

            var clonedRecipes = await APIClient.GetClonedRecipes(_httpClientFactory);

            allRecipes.AddRange(clonedRecipes);

            await _recipeService.ResetRecipes(allRecipes);

            await ResetPagingAsync(); 

            if (currentFilter == RecipeFilter.All)
            {
                await LoadNextPage_All();

                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    await EnsureScrollableAsync();
                }
            }
            else if (currentFilter == RecipeFilter.Yours)
            {
                await LoadNextPage_Yours();

            }
            else if (currentFilter == RecipeFilter.Recent)
            {
                await LoadTaggedRecipes();
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    await EnsureScrollableAsync();
                }
            }
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingSearchSpinner.IsVisible = false;
                LoadingSearchSpinner.IsRunning = false;
            });
        }
        else
        {
            await DisplayAlert("Not online", "To refresh recipes, check Internet connection.", "Ok"); 
        }
    } 
}