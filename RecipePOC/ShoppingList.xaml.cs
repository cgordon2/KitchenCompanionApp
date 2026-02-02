using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using RecipePOC.Services.UIModels;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace RecipePOC;

public partial class ShoppingList : ContentPage
{
    private IHttpClientFactory _theFactory { get; set; } 
    private IRecipeService _recipeService { get; set; }
    public ObservableCollection<ShoppingListItem> Items { get; set; } 

    public class ShoppingListItem
    {
        public int Id { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty; 

        public string GUID { get; set; } = string.Empty;

        public Color StripColor { get; set; } 

        public bool IsSelectedDone { get; set; } 
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        // split by checkbox state
        var doneItems = Items
            .Where(i => i.IsSelectedDone)
            .ToList();

        var undoneItems = Items
            .Where(i => !i.IsSelectedDone)
            .ToList();

        // extract GUIDs (or IDs)
        var doneGuids = doneItems.Select(i => i.GUID).ToList();
        var undoneGuids = undoneItems.Select(i => i.GUID).ToList();

        Console.WriteLine($"Done: {doneGuids.Count}, Undone: {undoneGuids.Count}");

        await UpdateShoppingItemsBatch(doneGuids, undoneGuids);
    }

    private async Task UpdateShoppingItemsBatch(
    List<string> doneGuids,
    List<string> undoneGuids)
    {
        var payload = new
        {
            markDone = doneGuids,
            markUndone = undoneGuids
        };

        await APIClient.PostRoute(_theFactory, "api/recipes/UpdateShoppingList", payload); 
    }

    private async void OnAddItemTapped(object sender, EventArgs e)
    {
        var text = FoodEntry.Text?.Trim();

        if (string.IsNullOrEmpty(text))
            return;
         
        Items.Add(new ShoppingListItem
        {
            Description = text,
            IsSelectedDone = false
        });

        // Clear input
        FoodEntry.Text = string.Empty;

        // Optional: hide keyboard
        FoodEntry.Unfocus();

        var username = await SecureStorage.GetAsync("user_name"); 
        await APIClient.CreateShoppingListItem(_theFactory, text, username); 
    }

    public ShoppingList(IHttpClientFactory theFactory, IRecipeService recipeService)
	{
		InitializeComponent();

        _theFactory = theFactory; 
        _recipeService = recipeService;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _ = OnAppearingAsync();
    }

    private async Task OnAppearingAsync()
    {
        Items = new ObservableCollection<ShoppingListItem>();

        if (PageHelpers.HasInternet())
        {
            var username = await SecureStorage.GetAsync("user_name"); 
            var shoppingListDto = await APIClient.GetShoppingList(_theFactory, username);

            await _recipeService.ResetShoppingList(shoppingListDto);

            var shoppingList = await _recipeService.GetShoppingListFromDB(username);

            foreach (var test in shoppingList)
            {
                var item = new ShoppingListItem();

                item.Description = test.Text;
                item.IsSelectedDone = test.IsDone;
                item.GUID = test.ShoppingListGUID; 

                Items.Add(item);
            }

            ShoppingListview.ItemsSource = Items; 
        }
    }

    private void Arrow_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.BindingContext is ExpandItem item)
        {
            item.IsExpanded = !item.IsExpanded;
        }
    }
}