using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelJournalApp.Models
{
    public class ImagePreview : INotifyPropertyChanged
    {
        private bool _isHeroImage;
        public bool IsHeroImage
        {
            get => _isHeroImage;
            set
            {
                if (_isHeroImage != value)
                {
                    _isHeroImage = value;
                    OnPropertyChanged(nameof(IsHeroImage));
                }
            }
        }

        public string ImageSource { get; set; } // This property holds the image source path

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
