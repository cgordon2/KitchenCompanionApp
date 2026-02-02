using RecipePOC.Services;
using System.Collections.ObjectModel;
using static RecipePOC.UserDirectory;

namespace RecipePOC;

public partial class Following : ContentPage
{
	private IHttpClientFactory _theFactory;
    private IAuthService _authService;

    public ObservableCollection<UserItem> Items { get; set; } = new(); 

	public Following()
	{
		InitializeComponent();

        _theFactory = MauiProgram.Services.GetRequiredService<IHttpClientFactory>();
        _authService = MauiProgram.Services.GetService<IAuthService>(); 

        BindingContext = this; 
	}


    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = OnAppearingAsync(); 
    }

    private async Task OnAppearingAsync()
    {
        var chefGuid = await SecureStorage.GetAsync("chef_guid");
        var following = await APIClient.GetFollowing(_theFactory, Convert.ToInt32(chefGuid)); 

        foreach (var user in following)
        {
            var userItem = new UserItem();

            userItem.UserName = user.UserName;
            userItem.UserId = Convert.ToInt32(user.ChefId);
            userItem.RealName = user.RealName;
            userItem.Photo = user.AvatarUrl;
            userItem.ShortBio = user.ShortBio;
            userItem.Location = user.Location;
            userItem.AvatarUrl = user.AvatarUrl;
            userItem.Email = user.Email;

            Items.Add(userItem); 
        }

        FollowingListview.ItemsSource = Items; 
    }
}