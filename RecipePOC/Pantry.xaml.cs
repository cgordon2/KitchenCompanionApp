using RecipePOC.DTOs;
using RecipePOC.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RecipePOC;

public partial class Pantry : ContentPage
{
	public ObservableCollection<PantryLocal> Pantrys { get; } = new ObservableCollection<PantryLocal>();
    private IHttpClientFactory _theFactory;
    private string _selectedIngredientGuid { get; set; } 

	public Pantry()
	{
		InitializeComponent();

        _theFactory = MauiProgram.Services.GetService<IHttpClientFactory>();
    }

   

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Pantrys.Clear(); 

        var pantryItems = await APIClient.GetPantryItems(_theFactory); 

        foreach (var item in pantryItems)
        {
            var pantryId = item.PantryId;
            var ingredientName = item.IngredientName;
            var quantity = item.Quantity;

            var dto = new PantryLocal();

            dto.Quantity = "Quantity Left: " + quantity;
            dto.PantryId = pantryId;
            dto.IngredientName = ingredientName;

            Pantrys.Add(dto); 
        }

        PantryList.ItemsSource = Pantrys; 
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        // This is the Pantry DTO for that row
        var pantryItem = (PantryLocal)button.BindingContext;

        int pantryId = pantryItem.PantryId;
        int updatedQty = pantryItem.EditedQuantity;

        pantryItem.Quantity = "Quantity Left: " + updatedQty;

        var pantryDto = new PantryDto();

        
        pantryDto.Quantity = updatedQty;
        pantryDto.PantryId = pantryId;
        pantryDto.UserName = "devon";
        pantryDto.IngredientGUID = "awefawefwaef"; 

        await APIClient.UpdatePantryByUser(_theFactory, pantryDto);  
    }

    private async void OnPickerTapped(object sender, EventArgs e)
    {
        var picker = new IngredientPicker((selectedIngredient) =>
        {
            string id = selectedIngredient.IngredientGuid;
            string name = selectedIngredient.Name;

            SelectedIngredientEntry.Text = name;

            _selectedIngredientGuid = id; 
        });

        await Navigation.PushAsync(picker);
    }


    public class PantryLocal : INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;
        private string _quantity;

        public int PantryId { get; set; } 

		public string IngredientName { get; set; }

        public string Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public string Unit { get; set; }
        
        public int EditedQuantity { get; set; } 
	}
}