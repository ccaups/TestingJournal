using System.Collections.ObjectModel;
using System.Diagnostics;
using TravelJournalApp.Data;
using TravelJournalApp.Models;


namespace TravelJournalApp.Views;

public partial class TravelUpdatePage : ContentPage
{
	private readonly DatabaseContext _databaseContext;
	private TravelViewModel _travelViewModel;
	public ObservableCollection<ImageViewModel> ImageViewModels { get; set; }
	private List<string> selectedTempImagePaths = new List<string>(); 


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
		ImageViewModels = new ObservableCollection<ImageViewModel>();
		foreach (var image in _travelViewModel.TravelImages)
		{
			if (File.Exists(image.FilePath)) // Check if the file exists
			{
				var imageViewModel = new ImageViewModel
				{
					FilePath = image.FilePath,
					ImageSource = ImageSource.FromFile(image.FilePath),
					IsSelected = image.IsSelected,
				};
				ImageViewModels.Add(imageViewModel);
			}
			else
			{
				Debug.WriteLine($"Image file not found: {image.FilePath}");
			}
		}
		ImagesCollectionView.ItemsSource = ImageViewModels;
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
					var imageViewModel = new ImageViewModel
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


	private async void OnUpdateButtonClicked(object sender, EventArgs e)
	{

		// Leia eksisteeriv travel journal
		var travelJournal = await _databaseContext.GetItemAsync(_travelViewModel.Id);
		if (travelJournal == null)
		{
			StatusLabel.Text = "Travel entry not found.";
			return;
		}

		// Uuenda travel journal detailid
		travelJournal.Title = TitleEntry.Text;
		travelJournal.Description = DescriptionEditor.Text;
		travelJournal.Location = LocationEntry.Text;
		travelJournal.TravelStartDate = DateStartEntry.Date;
		travelJournal.TravelEndDate = DateEndEntry.Date;
		travelJournal.LastUpdatedAt = DateTime.Now;

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
		foreach (var imageViewModel in ImageViewModels)
		{
			var existingImage = existingImages.FirstOrDefault(img => img.FilePath == imageViewModel.FilePath);
			if (existingImage != null)
			{
				existingImage.IsSelected = imageViewModel.IsSelected;
				existingImage.ImageIndex = ImageViewModels.IndexOf(imageViewModel);

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


	private async void OnBackButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}
}