using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.Services.Models
{
    public class IngredientItem : INotifyPropertyChanged
    {
        public string IngredientGUID { get; set; } = string.Empty; 
        public string Photo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string StoreURL { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public int Stars { get; set; } = 0;

        private string _Serves;
        private string _CookTime;
        private string _PrepTime; 
        private bool _isCheckboxVisible;

        public string PrepTime
        {
            get => _PrepTime; 
            set
            {
                _PrepTime = value;
                OnPropertyChanged(nameof(PrepTime)); 
            }
        }

        public string CookTime
        {
            get => _CookTime; 
            set
            {
                _CookTime = value;
                OnPropertyChanged(nameof(CookTime)); 
            }
        }

        public string Serves
        {
            get => _Serves;
            set
            {
                _Serves = value;
                OnPropertyChanged(nameof(Serves));
            }
        }

        public bool IsCheckboxVisible
        {
            get => _isCheckboxVisible;
            set
            {
                _isCheckboxVisible = value;
                OnPropertyChanged(nameof(IsCheckboxVisible));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }
}
