using RecipePOC.Services.UIModels;
using System.Collections.ObjectModel;

namespace RecipePOC;

public partial class ShoppingList : ContentPage
{
    public ObservableCollection<ExpandItem> Items { get; set; }
    public ObservableCollection<EmojiItem> EmojiItems { get; set; }

    public ShoppingList()
	{
		InitializeComponent();

        Items = new ObservableCollection<ExpandItem>
                    {
                        new ExpandItem { Title = "Pancake Mix" },
                        new ExpandItem { Title = "Eggs" },
                        new ExpandItem { Title = "Milk" }
                    };

        EmojiItems = new ObservableCollection<EmojiItem>
    {
        new EmojiItem { Emoji = "Pancake Mix" },
        new EmojiItem { Emoji = "Milk" },
        new EmojiItem { Emoji = "Syrup" },
        new EmojiItem { Emoji = "Sugar" }, 
    };

        BindingContext = this;
    }

    private void Arrow_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.BindingContext is ExpandItem item)
        {
            item.IsExpanded = !item.IsExpanded;
        }
    }
}

public class EmojiItem
{
    public string Emoji { get; set; }
}