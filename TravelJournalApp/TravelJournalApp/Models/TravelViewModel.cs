using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TravelJournalApp.Data;
using TravelJournalApp.Views;

namespace TravelJournalApp.Models
{
    public class TravelViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public DateTime TravelStartDate { get; set; }
        public DateTime TravelEndDate { get; set; }
        public ObservableCollection<ImageTable> TravelImages { get; set; } = new ObservableCollection<ImageTable>();

        private int _selectedImageIndex;
        private TravelViewModel _selectedTravel;
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));

                }
            }
        }
        public ICommand NavigateToBigHeroCommand => new Command(() =>
        {
            // Leia TravelPage objekt
            var travelPage = (TravelMainPage)Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault(p => p is TravelMainPage);

            // Leia ListViewModel objekt
            var listViewModel = (ListViewModel)travelPage.BindingContext;

            // Uuenda SelectedTravel omadust
            listViewModel.SelectedTravel = this;
            listViewModel.OnPropertyChanged(nameof(listViewModel.SelectedTravel));
        });


        public TravelViewModel SelectedTravel
        {
            get => _selectedTravel;
            set
            {
                if (_selectedTravel != value)
                {
                    _selectedTravel = value;
                    OnPropertyChanged(nameof(SelectedTravel));
                }
            }
        }
        public int SelectedImageIndex
        {
            get => _selectedImageIndex;
            set
            {
                if (_selectedImageIndex != value)
                {
                    _selectedImageIndex = value;
                    OnPropertyChanged(nameof(SelectedTravel));
                    OnPropertyChanged(nameof(SelectedImageIndex));
                    OnPropertyChanged(nameof(HeroImageSource));
                }
            }
        }

        // This property provides the source for the HeroImage
        public string HeroImageSource
        {
            get
            {
                if (TravelImages != null && TravelImages.Count > 0 && SelectedImageIndex >= 0 && SelectedImageIndex < TravelImages.Count)
                {
                    OnPropertyChanged(nameof(SelectedTravel));
                    return TravelImages[SelectedImageIndex].FilePath;
                }
                else
                {
                    // Return a default image or placeholder if no images are available
                    return TravelImages[0].FilePath; ;
                }
            }
        }




        // This property provides the source for the HeroImage backup
        //public string HeroImageSource
        //{
        //    get
        //    {
        //        if (TravelImages != null && TravelImages.Count > 0)
        //        {
        //            // Leia esimene valitud pilt
        //            var selectedImage = TravelImages.FirstOrDefault(img => img.IsSelected);

        //            if (selectedImage != null)
        //            {
        //                return selectedImage.FilePath;
        //            }
        //            else
        //            {
        //                // Kui ühtegi pilti pole valitud, tagasta esimene pilt
        //                return TravelImages[0].FilePath;
        //            }
        //        }
        //        else
        //        {
        //            // Kui pilte pole, tagasta vaikepilt
        //            return "default_image.png";
        //        }
        //    }
        //}

        public string TravelDates => $"{TravelStartDate:dd.MM.yy} - {TravelEndDate:dd.MM.yy}";


        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


	}
}