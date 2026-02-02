using Microsoft.Maui.ApplicationModel.Communication;
using RecipePOC.DTOs;
using RecipePOC.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using System.Text; 

namespace RecipePOC;

public partial class AdvancedSearchListview : ContentPage
{
	private IHttpClientFactory _theFactory;
	private RecipeSearchDto _searchDto; 
	public ObservableCollection<RecipeListItem> Items { get; set; } = new();
    public ICommand ToggleExpandCommand { get; }

    public AdvancedSearchListview(RecipeSearchDto dto)
	{
		InitializeComponent();

		_searchDto = dto;
		_theFactory = MauiProgram.Services.GetService<IHttpClientFactory>();

        ToggleExpandCommand = new Command<RecipeListItem>(item =>
        {
            item.IsExpanded = !item.IsExpanded;
        });

        BindingContext = this; 
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		_ = OnAppearingAsync(); 
    }

    public class RecipeListItem : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public string Photo { get; set; } // base64 or url
        public int Prep { get; set; }
        public int CookTime { get; set; }
        public int Serves { get; set; }
        public int Stars { get; set; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
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

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private async Task OnAppearingAsync()
	{
		var results = await APIClient.SearchAllRecipes(_theFactory, _searchDto);

        foreach (var t in results)
        {
            var recipe = new RecipeListItem();

            recipe.Title = t.RecipeName;
            recipe.UserName = t.ChefName;
            recipe.Photo = t.Photo;
            recipe.CookTime = t.CookTime;
            recipe.Serves = t.Serves;
            recipe.Prep = t.Prep;

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
            recipe.Description = sb.ToString();

            recipe.Stars = t.Stars;


            Items.Add(recipe);
        }

        SearchListview.ItemsSource = Items; 
    }
}