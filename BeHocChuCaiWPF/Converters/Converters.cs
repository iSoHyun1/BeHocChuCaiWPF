using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BeHocChuCaiWPF.Converters
{
    public class EggStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                            object parameter, CultureInfo culture)
        {
            bool isCracked = (bool)value;
            string imagePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                isCracked ? "egg_cracked.png" : "egg_whole.png"
            );
            return new BitmapImage(new Uri(imagePath));
        }

        public object ConvertBack(object value, Type targetType,
                                object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LockOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                            object parameter, CultureInfo culture)
        {
            return (bool)value ? 0.5 : 1.0;
        }

        public object ConvertBack(object value, Type targetType,
                                object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}