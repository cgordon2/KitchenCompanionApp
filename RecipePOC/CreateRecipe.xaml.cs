using RecipePOC.Services.Models;
using RecipePOC.Services.Recipes;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecipePOC;

public partial class CreateRecipe : ContentPage
{
    public ObservableCollection<TodoItem> Items { get; set; }
    private IRecipeService _recipeService; 

    public CreateRecipe(IRecipeService recipeService)
    {
        InitializeComponent();

        Items = new ObservableCollection<TodoItem>
        {
            new TodoItem { Name="Eggs", IsChecked=false }, 
        };

        _recipeService = recipeService;

        BindingContext = this; // important!
    }

    private async void OnSelectExistingIngredientTapped(object sender, TappedEventArgs e)
    {
        var username = await SecureStorage.GetAsync("user_name"); 

        await Navigation.PushAsync(new IngredientsListView(_recipeService, username, true)); 
    }

    private async void CreateRecipeDB(object sender, EventArgs e)
    {
        var recipeTitle = IngredientNameEntry.Text;
        var recipeDescription = QuantityEntry.Text;

        var realName = await SecureStorage.GetAsync("real_name");
        var email = await SecureStorage.GetAsync("email"); //chef email or user email 
        var category = CategoryPicker2.SelectedItem; 

        if (category != null)
        {
            category = (string)category; 
        }

        var serializedIngredients = await SecureStorage.GetAsync("selected_ingredients");
        if (!string.IsNullOrEmpty(serializedIngredients))
        {
            var selectedItems = JsonSerializer.Deserialize<List<IngredientItem>>(serializedIngredients); 
        }
    }
}
public class TodoItem
{
    public string Name { get; set; }
    public bool IsChecked { get; set; }
}