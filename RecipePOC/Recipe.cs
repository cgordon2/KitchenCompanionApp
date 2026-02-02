using RecipePOC.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RecipePOC
{
    public class Recipe : INotifyPropertyChanged
    {
        public Recipe()
        {
            IsExpanded = false; 
        }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ExtraInfo { get; set; }
        public string UserName { get; set; } 
        public string? ChefEmail { get; set; } 
        public string Photo { get; set; } 
        public int Stars { get; set; } 
        public string Favorite { get; set; } 
        public string? Category { get; set; } 
        public int CookTime { get; set; } 
        public int Serves { get; set; }   
        public int Prep { get; set; } 
        public int Index { get; set; }   // for alternating styles
        public string RecipeGUID { get; set; }
        private bool? _isExpanded;
        public bool? IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public List<RecipeAndRiDTO> RecipeIngredients { get; set; }

        public ImageSource? RecipePhoto
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

    }
}
