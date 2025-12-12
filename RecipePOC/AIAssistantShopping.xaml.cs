using RecipePOC.Services;
using System.ComponentModel;

namespace RecipePOC;

public partial class AIAssistantShopping : ContentPage, INotifyPropertyChanged
{
    private int _notifCount;
    private INotificationService _notificationsService; 

    public int NotifCount
    {
        get => _notifCount;
        set
        {
            if (_notifCount != value)
            {
                _notifCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotifCount)));
            }
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public AIAssistantShopping(INotificationService notificationService)
	{
		InitializeComponent();

        BindingContext = this;

        _notificationsService = notificationService; 

        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsEnabled = false,
            IsVisible = false
        });
 
    }

    protected override void OnAppearing()
    {
        base.OnAppearing(); 
        _ = OnAppearingAsync();
    }

    private async Task OnAppearingAsync()
    {
        var chefguid = await SecureStorage.GetAsync("chef_guid"); 
        var notifs = await _notificationsService.GetNotifications(false, chefguid);
        NotifCount = notifs.Count;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        //await Shell.Current.GoToAsync(nameof(HomePage));
        // await Shell.Current.GoToAsync("///Search");
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync("//HomePage");
        });
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        // await Shell.Current.GoToAsync(nameof(Search));
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync("//Search");
        });
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        //await Shell.Current.GoToAsync(nameof(CreateRecipe));
        await Shell.Current.GoToAsync("//CreateRecipe");

    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {  
    }

    private async void Button_Clicked_4(object sender, EventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync(nameof(Profile));
        });
    }
}