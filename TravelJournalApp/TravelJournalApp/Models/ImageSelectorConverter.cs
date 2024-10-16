//using System.Diagnostics;
//using System.Globalization;
//using TravelJournalApp.Data;
//using TravelJournalApp.Models;

//namespace TravelJournalApp.Models
//{
//    public class ImageSelectorConverter : IMultiValueConverter
//    {
//        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (values[0] is not TravelViewModel selectedTravel || values[1] is not int index)
//            {
//                Debug.WriteLine("Error in ImageSelectorConverter: Invalid input values.");
//                return "path/to/default/image.jpg"; // Tagasta vaikimisi pildi tee
//            }

//            var images = selectedTravel.TravelImages;

//            Debug.WriteLine($"SelectedIndex: {index}, Total Images: {images.Count}");

//            if (index >= 0 && index < images.Count)
//            {
//                return images[index].FilePath;
//            }
//            else
//            {
//                Debug.WriteLine($"Error in ImageSelectorConverter: Invalid image index: {index}");
//                return "path/to/default/image.jpg"; // Tagasta vaikimisi pildi tee
//            }
//        }

//        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
//        {
//            throw new NotImplementedException();
//        }}
//}
