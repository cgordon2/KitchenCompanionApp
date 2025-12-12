using RecipePOC.Services;
using System.Collections.ObjectModel;

namespace RecipePOC;

public partial class Notifications : ContentPage
{
    public class Recipe
    {
        public int NotifId { get; set; } 
        public string Message { get; set; } 
        public string NotifGUID { get; set; }

        public Color StripColor {  get; set; }
        public bool IsSelectedForRead { get; set; }   // << NEW
    }

    public INotificationService notificationService; 
    public System.Collections.ObjectModel.ObservableCollection<Recipe> Recipes { get; set; }

    public Notifications(INotificationService _notificationService)
	{
		InitializeComponent();

        notificationService = _notificationService; 
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var chefGuid = await SecureStorage.GetAsync("chef_guid");
        var unreadNotifs = await notificationService.GetNotifications(false, chefGuid);

        Recipes = new ObservableCollection<Recipe>();

        foreach (var unreadNotif in unreadNotifs)
        {
            string message = unreadNotif.Message;

            var notif = new Recipe();

            notif.NotifId = unreadNotif.Id;
            notif.Message = message;
            notif.NotifGUID = unreadNotif.NotifGUID; 

            Recipes.Add(notif); 
        }

        RecipesList2.ItemsSource = Recipes; 
    }

    private async void MarkRead_Clicked(object sender, EventArgs e)
    {
        var selected = Recipes.Where(x => x.IsSelectedForRead).ToList();
        var dbList = new List<DB.Models.Notification>(); 

        foreach (var item in selected)
        {
            var notif = new DB.Models.Notification();

            notif.NotifGUID = item.NotifGUID;
            notif.Id = item.NotifId; 
            notif.IsRead = true; 

            dbList.Add(notif);
            Recipes.Remove(item);
        }

        await notificationService.MarkRead(dbList); 
    }
}