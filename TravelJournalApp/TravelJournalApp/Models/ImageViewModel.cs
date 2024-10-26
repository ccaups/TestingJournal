using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TravelJournalApp.Data;
using System.IO;
using System.Threading.Tasks;


namespace TravelJournalApp.Models
{
    public class ImageViewModel : INotifyPropertyChanged
    {
        public string FilePath { get; set; }
        public ImageSource ImageSource { get; set; }
        //public string ImageSource { get; set; } // This property holds the image source path
		private ObservableCollection<ImageViewModel> _imageViewModels;
		private DatabaseContext _databaseContext;

        public ICommand DeleteImageByOneCommand { get; }

		public ImageViewModel(ObservableCollection<ImageViewModel> imageViewModels, DatabaseContext databaseContext)
		{
			_imageViewModels = imageViewModels;
			_databaseContext = databaseContext;
			DeleteImageByOneCommand = new Command(async () => await TempDeleteImages());
		}

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
                    //OnPropertyChanged(nameof(ButtonLabelHero));
                    OnPropertyChanged(nameof(ButtonBackgroundColorHero));
                }
            }
        }

        private bool _isMarkedForDeletion;
        public bool IsMarkedForDeletion
        {
            get => _isMarkedForDeletion;
            set
            {
                _isMarkedForDeletion = value;
                OnPropertyChanged(nameof(IsMarkedForDeletion));
                //OnPropertyChanged(nameof(ButtonLabelDelete));
                OnPropertyChanged(nameof(ButtonBackgroundColorDelete));
                // Sa võid siia lisada logi, et teavitada, et olek on muutunud
            }
        }

        //public string ButtonLabelHero => _isHeroImage ? "Hero Image" : "Add as Hero Image";
        //public Color ButtonBackgroundColorHero => _isHeroImage
        //    ? Color.FromArgb("#012f36")
        //    : Color.FromArgb("#00525e");

        //public string ButtonLabelDelete => _isMarkedForDeletion ? "Removed" : "Remove?";
        //public Color ButtonBackgroundColorDelete => _isMarkedForDeletion
        //    ? Color.FromArgb("#ed331f")
        //    : Color.FromArgb("#a12315");

        public Color ButtonBackgroundColorHero => _isHeroImage
            ? Color.FromArgb("#30d1b4")
            : Color.FromRgba(0, 0, 0, 0);

        public Color ButtonBackgroundColorDelete => _isMarkedForDeletion
            ? Color.FromArgb("#d13035")
            : Color.FromRgba(0, 0, 0, 0);


        // Funktsioon, mis ajutiselt eemaldab pildi ja liigutab selle eraldi kausta
        private async Task TempDeleteImages()
        {
            string tempDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TempDeletedImages");

            // Kui kaust ei eksisteeri, loo see.
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            // Määra ajutine failitee, kuhu pilt ajutiselt salvestatakse.
            string tempFilePath = Path.Combine(tempDirectory, Path.GetFileName(FilePath));

            // Liiguta fail ajutisse kausta.
            if (File.Exists(FilePath))
            {
                File.Move(FilePath, tempFilePath);
            }

            // Eemalda pilt kollektsioonist
            _imageViewModels.Remove(this);

            // Teavita kollektsiooni muutumisest
            OnPropertyChanged(nameof(_imageViewModels));

            // Salvestada vajalikud andmed (võib-olla ajutine failitee) andmebaasi, kui vaja.
        }

        // Kinnita kustutamine ja eemalda ajutised failid jäädavalt
        public void ConfirmDeleteImages()
        {
            string tempDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TempDeletedImages");

            if (Directory.Exists(tempDirectory))
            {
                var tempFiles = Directory.GetFiles(tempDirectory);

                // Kustuta kõik failid ajutisest kaustast
                foreach (var tempFile in tempFiles)
                {
                    File.Delete(tempFile);
                }
            }
        }

        // Taasta ajutiselt kustutatud failid algsesse asukohta
        public void RestoreDeletedImages()
        {
            string tempDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TempDeletedImages");

            if (Directory.Exists(tempDirectory))
            {
                var tempFiles = Directory.GetFiles(tempDirectory);

                // Taasta failid algsesse asukohta
                foreach (var tempFile in tempFiles)
                {
                    string originalFilePath = Path.Combine("original/path", Path.GetFileName(tempFile)); // Lisa õige algne tee
                    File.Move(tempFile, originalFilePath);
                }
            }
            IsMarkedForDeletion = false; // Taasta olek
            OnPropertyChanged(nameof(IsMarkedForDeletion)); // Teata, et olek on muutunud
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}