using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TravelJournalApp.Data;

public class TravelDetailViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	private ObservableCollection<ImageTable> _travelImages;
	public ObservableCollection<ImageTable> TravelImages
	{
		get => _travelImages;
		set
		{
			_travelImages = value;
			OnPropertyChanged();
		}
	}

	private int _currentImageIndex;
	public int CurrentImageIndex
	{
		get => _currentImageIndex;
		set
		{
			if (_currentImageIndex != value)
			{
				_currentImageIndex = value;
				OnPropertyChanged();
			}
		}
	}

	public int TotalImages => TravelImages?.Count ?? 0;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
