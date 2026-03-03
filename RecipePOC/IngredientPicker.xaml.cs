using RecipePOC.Services;
using RecipePOC.Services.Models;
using RecipePOC.Services.Recipes;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks; 

namespace RecipePOC;

public partial class IngredientPicker : ContentPage
{
    private readonly Action<IngredientOption> _onSelected;
    private IHttpClientFactory _theFactory;
    public ObservableCollection<IngredientOption> IGs { get; set;  } = new ObservableCollection<IngredientOption>();
    private IRecipeService recipeService { get; set; } 

    public IngredientPicker(Action<IngredientOption> onSelected)
    {
        InitializeComponent();
        _onSelected = onSelected;
        _theFactory = MauiProgram.Services.GetService<IHttpClientFactory>();
        recipeService = MauiProgram.Services.GetService<IRecipeService>();

        PopupList.ItemsSource = IGs;
        PopupList.ItemAppearing += PopupList_ItemAppearing;
    }

    private int currentPage = 0;
    private int pageSize = 2;

    protected override async void OnAppearing()
    {
        base.OnAppearing();


        currentPage = 0;
        IGs.Clear();


        await GetIngredients();
    }

    private bool isLoading = false;
    private bool hasMoreData = true;
    private async void PopupList_ItemAppearing(object sender, ItemVisibilityEventArgs e)
    {
        if (isLoading || !hasMoreData)
            return;

        if (e.Item == IGs.Last())
        {
            await GetIngredients();
        }
    }
    private async Task GetIngredients()
    {
        if (isLoading || !hasMoreData)
            return;

        isLoading = true;

        var ingredients = await recipeService.GetIngredients(currentPage, pageSize);

        if (ingredients == null || !ingredients.Any())
        {
            hasMoreData = false;  
            isLoading = false;
            return;
        }

        foreach (var ingredient in ingredients)
        {
            IGs.Add(new IngredientOption
            {
                IngredientGuid = ingredient.IngredientGUID,
                Name = ingredient.IngredientName
            });
        }

        currentPage++;

        isLoading = false;
    }

    private async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            return;

        IngredientOption selected = (IngredientOption) e.SelectedItem; 

        _onSelected?.Invoke(selected);

        await Navigation.PopAsync();
    }
}

public class RowColorConverter : IValueConverter
{
    private static int _index = -1;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        _index++;
        return _index % 2 == 0 ? Colors.White : Color.FromArgb("#FFE6F0");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => null;
}