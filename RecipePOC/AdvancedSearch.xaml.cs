using RecipePOC.DTOs;
using RecipePOC.Services.Recipes;

namespace RecipePOC;

public partial class AdvancedSearch : ContentPage
{
	public AdvancedSearch()
	{
		InitializeComponent();

        BindingContext = this; 
	}
    int _rating = 0;
    Label[] _stars;
    private bool _searchOnlyUser;
    public bool SearchOnlyUser
    {
        get => _searchOnlyUser;
        set
        {
            if (_searchOnlyUser != value)
            {
                _searchOnlyUser = value;
                OnPropertyChanged();
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _stars = new[] { Star1, Star2, Star3, Star4, Star5 };
    }

    private void StarTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter == null)
            return;

        _rating = Convert.ToInt32(e.Parameter);

        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i].TextColor = i < _rating
                ? Colors.Gold
                : Colors.Silver;
        }
    }

    private async void AdvancedSearchRecipes(object sender, EventArgs e)
    {
        var allOfWords = AllOfWords.Text;
        var thisExactPhrase = ThisExactPhrase.Text; 

        if (allOfWords == null)
            allOfWords = string.Empty;

        if (thisExactPhrase == null)
            thisExactPhrase = string.Empty; 
        var loggedInUserGuid = await SecureStorage.GetAsync("chef_guid"); 

        var recipeSearchDto = new RecipeSearchDto();

        recipeSearchDto.AllWords = allOfWords; 
        recipeSearchDto.ExactPhrase = thisExactPhrase;
        //recipeSearchDto.NoneWords = noneOfWords;
        recipeSearchDto.NoneWords = string.Empty; 
        recipeSearchDto.loggedInUserGuid = Convert.ToInt32(loggedInUserGuid); 
        recipeSearchDto.SearchOnlyUser = SearchOnlyUser;

        if (_rating > 0)
            recipeSearchDto.Stars = _rating; 


        await Navigation.PushAsync(new AdvancedSearchListview(recipeSearchDto)); 
    }
}