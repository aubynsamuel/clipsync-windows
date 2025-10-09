using System;
using System.Globalization;
using System.Windows.Data;

namespace ClipSyncWindows.Converters
{
    public class FirstLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string deviceName && !string.IsNullOrEmpty(deviceName))
            {
                return deviceName.Substring(0, 1).ToUpper();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
