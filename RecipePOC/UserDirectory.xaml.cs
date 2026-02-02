using RecipePOC.DB;
using RecipePOC.Services;
using RecipePOC.Services.Recipes;
using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RecipePOC;

public partial class UserDirectory : ContentPage, INotifyPropertyChanged
{ 
    private IHttpClientFactory _theFactory;
    private IAuthService _authService;

    public ObservableCollection<UserItem> Items { get; set; } 

    public class UserItem
    {
        public string UserName { get; set; } 
        public string RealName { get; set; } 
        public string TotalRecipes { get; set; } 
        public string Photo { get; set; } 

        public string ShortBio { get; set; } 

        public string AvatarUrl { get; set; } 

        public string Location { get; set; } 
        public string Email { get; set; } 

        public int UserId { get; set; }
        public ImageSource UserAvatar
        {
            get
            {
                const string ApiBaseUrl = "http://192.168.7.203:5285";

                var file = string.IsNullOrWhiteSpace(AvatarUrl)
                           || AvatarUrl == "user_ico.png"
                           ? "user_ico.png"
                           : AvatarUrl;

                return ImageSource.FromUri(
                    new Uri($"{ApiBaseUrl}/uploads/{file}")
                );
            }
        }
    }

    private int page = 1;

    public UserDirectory()
    {
        InitializeComponent();

        _authService = MauiProgram.Services.GetService<IAuthService>();
        _theFactory = MauiProgram.Services.GetRequiredService<IHttpClientFactory>();

        Items = new ObservableCollection<UserItem>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        page = 1;
        Items.Clear(); 

        _ = OnAppearingAsync(); 
    }

    private async Task OnAppearingAsync()
    {
        var users = await APIClient.GetUsers(_theFactory, page);
        page++; 

        foreach (var user in users)
        {
            var userItem = new UserItem();

            if (user.AvatarUrl == null)
                user.AvatarUrl = "user_ico.png"; 

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

        UserListview.ItemsSource = Items;
    }

    private async void UserListview_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item == null)
            return;

        var selectedUser = e.Item as UserItem;
        var userDto = new DTOs.UserDTO();

        userDto.UserName = selectedUser.UserName;
        userDto.UserId = selectedUser.UserId;
        userDto.RealName = selectedUser.RealName;
        userDto.ShortBio = selectedUser.ShortBio; 
        userDto.Location = selectedUser.Location;
        userDto.AvatarUrl = selectedUser.Photo;
        userDto.Email = selectedUser.Email; 

        await Navigation.PushAsync(new UserProfileDirectory(userDto, _theFactory)); 
    }
}