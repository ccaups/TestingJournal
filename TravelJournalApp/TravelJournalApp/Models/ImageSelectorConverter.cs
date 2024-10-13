using System.Globalization;
using TravelJournalApp.Models;
using TravelJournalApp.Data;
using System.Diagnostics;

namespace TravelJournalApp.Models
{
    public class ImageSelectorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is not IList<ImageDatabase> images || values[1] is not int index)
                return null;

            // Logi, milline pilt on valitud
            Debug.WriteLine($"SelectedIndex: {index}, Total Images: {images.Count}");

            if (index >= 0 && index < images.Count)
                return images[index].FilePath; // Assuming ImageDatabase has a FilePath property

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
