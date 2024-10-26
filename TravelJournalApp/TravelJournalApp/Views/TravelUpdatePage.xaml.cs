using DocumentFormat.OpenXml.Presentation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using TravelJournalApp.Data;
using TravelJournalApp.Models;


namespace TravelJournalApp.Views
{

	public partial class TravelUpdatePage : ContentPage
	{
		private readonly DatabaseContext _databaseContext;
		private TravelViewModel _travelViewModel;
		public ObservableCollection<ImageViewModel> ImageViewModels { get; set; } = new ObservableCollection<ImageViewModel>();
        private List<string> selectedTempImagePaths = new List<string>();
       
        private string _heroImageFile; // Property to store the hero image file path


        public TravelUpdatePage(TravelViewModel travel)
		{
			InitializeComponent();
			BindingContext = travel;
			_databaseContext = new DatabaseContext();
			_travelViewModel = travel;

			LoadImagePreviews();
		}

		private void LoadImagePreviews()
		{
            //ImageViewModels.Clear(); // Tühjendage kollektsioon enne uute piltide lisamist
			ImageViewModels = new ObservableCollection<ImageViewModel>();

            foreach (var image in _travelViewModel.TravelImages)
			{
				if (File.Exists(image.FilePath)) // Check if the file exists
				{
					var heroo = _travelViewModel.HeroImageFile;
                    var imageViewModel = new ImageViewModel(ImageViewModels, _databaseContext)
					{
						FilePath = image.FilePath,
						ImageSource = ImageSource.FromFile(image.FilePath),
						IsSelected = image.IsSelected,
                        IsHeroImage = image.FilePath == _travelViewModel.HeroImageFile // Kontrollige, kas pilt on kangelaspilt
					};
					ImageViewModels.Add(imageViewModel);

                }
				else
				{
					Debug.WriteLine($"Image file not found: {image.FilePath}");
				}
			}
			ImagesCollectionView.ItemsSource = ImageViewModels; // Määrake kollektsioon ItemsSource'iks
        }

		private async void OnPickPhotosClicked(object sender, EventArgs e)
		{
			try
			{
				var pickResult = await FilePicker.PickMultipleAsync(new PickOptions
				{
					FileTypes = FilePickerFileType.Images,
					PickerTitle = "Select Images"
				});

				if (pickResult != null)
				{
					selectedTempImagePaths.Clear(); // Kustuta eelnev valik

					foreach (var image in pickResult)
					{
						// Säilita ajutine pildi path
						selectedTempImagePaths.Add(image.FullPath);

						// Näita eelnevaid selekteerituid pilte
						var imageViewModel = new ImageViewModel(ImageViewModels, _databaseContext)
						{
							FilePath = image.FullPath,
							ImageSource = ImageSource.FromFile(image.FullPath),
							IsSelected = true
						};
						ImageViewModels.Add(imageViewModel);
					}

					// Värskenda UI-d
					ImagesCollectionView.ItemsSource = ImageViewModels;
				}
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Error picking images: {ex.Message}";
			}
		}

        private void OnButtonClickedUpdateHero(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.BindingContext is ImageViewModel selectedImage)
            {
                selectedImage.IsHeroImage = !selectedImage.IsHeroImage;

                foreach (var image in ImageViewModels)
                {
                    if (image != selectedImage)
                    {
                        image.IsHeroImage = false;
                    }
                    image.OnPropertyChanged(nameof(image.ButtonBackgroundColorHero));
                }

                OnPropertyChanged(nameof(ImageViewModels));
            }
        }

        private void OnButtonClickedUpdateDelete(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.BindingContext is ImageViewModel selectedImage)
            {
                // Märgi pilt kustutamiseks
                selectedImage.IsMarkedForDeletion = !selectedImage.IsMarkedForDeletion;

                // Teavitame, et omadused on muutunud
                //OnPropertyChanged(nameof(ButtonLabelDelete));
                //OnPropertyChanged(nameof(ButtonBackgroundColorDelete));
                OnPropertyChanged(nameof(ImageViewModels));
            }
        }


        private async Task<bool> ConfirmNoSaveAsync()
        {
            return await Application.Current.MainPage.DisplayAlert(
                "Unsaved Changes",
                "Are you sure you want to discard this travel entry without saving?",
                "Yes",
                "No"
            );
        }
        private async Task<bool> ConfirmSaveAsync()
        {
            return await Application.Current.MainPage.DisplayAlert(
                "Save Travel Entry",
                "Are you sure you want to save this travel entry?",
                "Yes",
                "No"
            );
        }



        private async void OnUpdateButtonClicked(object sender, EventArgs e)
		{
            if (!await ConfirmSaveAsync()) return;
			ConfirmDeleteImages();

            // Leia eksisteeriv travel journal
            var travelJournal = await _databaseContext.GetItemAsync(_travelViewModel.Id);
			if (travelJournal == null)
			{
				StatusLabel.Text = "Travel entry not found.";
				return;
			}

			// Leia kangelaspilt
			var heroImage = ImageViewModels.FirstOrDefault(image => image.IsHeroImage);

			// Uuenda travel journal detailid
			travelJournal.Title = TitleEntry.Text;
			travelJournal.Description = DescriptionEditor.Text;
			travelJournal.Location = LocationEntry.Text;
			travelJournal.TravelStartDate = DateStartEntry.Date;
			travelJournal.TravelEndDate = DateEndEntry.Date;
			travelJournal.LastUpdatedAt = DateTime.Now;
			travelJournal.HeroImageFile = heroImage?.FilePath; // Salvesta kangelaspildi faili tee

			// Uuenda travel journal sisend
			bool result = await _databaseContext.UpdateItemAsync(travelJournal);

			// Leia eksisteeriva pildid selle reisi jaoks
			var existingImages = await _databaseContext.GetImagesForTravelJournalAsync(travelJournal.Id);
			var existingImageFilePaths = existingImages.Select(img => img.FilePath).ToList();

			// Uute piltide valimine
			foreach (var tempImagePath in selectedTempImagePaths)
			{
				var newFilePath = Path.Combine(FileSystem.AppDataDirectory, Path.GetFileName(tempImagePath));

				try
				{
					// Ainult lisa pilt kui ei ole juba andmebaasis
					if (!existingImageFilePaths.Contains(newFilePath))
					{
						File.Copy(tempImagePath, newFilePath, true);

						var imageTable = new ImageTable
						{
							TravelJournalId = travelJournal.Id,
							FilePath = newFilePath,
							ImageIndex = existingImages.Count // Uuenda ImageIndex uute piltide jaoks
						};

						await _databaseContext.AddItemAsync(imageTable);
						existingImageFilePaths.Add(newFilePath); // Väldib duplikaate ehk jälgib pilti
					}
				}
				catch (Exception ex)
				{
					StatusLabel.Text = $"Error copying images: {ex.Message}";
					return;
				}
			}

            // Uuenda eksisteerivaid pilte andmebaasis
            // Kasuta List<ImageViewModel> selle asemel, et iteratsiooni ajal muudatusi teha
            var imagesToUpdate = ImageViewModels.ToList(); // Looge koopia

            foreach (var imageViewModel in imagesToUpdate)
            {
                var existingImage = existingImages.FirstOrDefault(img => img.FilePath == imageViewModel.FilePath);
                if (existingImage != null)
                {
                    existingImage.IsSelected = imageViewModel.IsSelected;
                    existingImage.ImageIndex = imagesToUpdate.IndexOf(imageViewModel); // Kasutage imagesToUpdate, mitte ImageViewModels

                    // Salvesta uuendused
                    await _databaseContext.SaveImageAsync(existingImage);
                }
            }



            if (result)
			{
				await Navigation.PopToRootAsync();
			}
			else
			{
				StatusLabel.Text = "Failed to update travel.";
				StatusLabel.TextColor = Color.FromArgb("#FF6347");
			}
		}

        private async void BackTravelButton_Clicked(object sender, EventArgs e)
		{
			if (!await ConfirmNoSaveAsync()) return;
			RestoreDeletedImages();
            await Navigation.PopAsync();
		}

        private async void ConfirmDeleteImages()
        {
            // Kui on olemas ajutiselt kustutatud pildid, kustuta need lõplikult
            foreach (var imageViewModel in ImageViewModels.ToList()) // Kasutame ToList(), et vältida modifitseerimist samal ajal kui kollektsioonis itereerime
            {
                if (imageViewModel.IsMarkedForDeletion) // Eeldades, et teil on mingi lipp, mis märgib pildid kustutamiseks
                {
                    try
                    {
                        // Kustuta pilt andmebaasist
                        var imageRecord = await _databaseContext.GetImageByFilePathAsync(imageViewModel.FilePath);
                        if (imageRecord != null)
                        {
                            await _databaseContext.DeleteItemAsync(imageRecord);
                        }

                        // Kustuta pilt failisüsteemist
                        if (File.Exists(imageViewModel.FilePath))
                        {
                            File.Delete(imageViewModel.FilePath);
                        }

                        // Eemalda pilt UI-st
                        ImageViewModels.Remove(imageViewModel);
                    }
                    catch (Exception ex)
                    {
                        StatusLabel.Text = $"Error deleting image {imageViewModel.FilePath}: {ex.Message}";
                        StatusLabel.TextColor = Color.FromArgb("#b01d0c");
                        StatusLabel.IsVisible = true;
                    }
                }
            }

            // Värskenda UI-d peale kustutamist
            ImagesCollectionView.ItemsSource = ImageViewModels;
        }


        private void RestoreDeletedImages()
        {
            // Kontrolli, kas on olemas ajutiselt eemaldatud pildid
            if (selectedTempImagePaths != null && selectedTempImagePaths.Count > 0)
            {
                foreach (var imagePath in selectedTempImagePaths)
                {
                    // Taasta ajutiselt kustutatud pildid uuesti ImageViewModel kollektsiooni
                    var imageViewModel = new ImageViewModel(ImageViewModels, _databaseContext)
                    {
                        FilePath = imagePath,
                        ImageSource = ImageSource.FromFile(imagePath),
                        IsSelected = true // Taasta valitud olek
                    };
                    ImageViewModels.Add(imageViewModel);
                }

                // Puhasta ajutine nimekiri, kuna pildid on taastatud
                selectedTempImagePaths.Clear();

                // Uuenda UI
                ImagesCollectionView.ItemsSource = ImageViewModels;
            }
            else
            {
                // Kui ei ole ajutiselt eemaldatud pilte, anna kasutajale tagasisidet (valikuline)
                StatusLabel.Text = "No images to restore.";
				StatusLabel.TextColor = Color.FromArgb("#b0aa0c");
                StatusLabel.IsVisible = true;
            }
        }

        //          private async void OnBackButtonClicked(object sender, EventArgs e)
        //{
        //	await Navigation.PopAsync();
        //}
    }

}