using System.Globalization;
using System.Windows.Data;

namespace ClipSyncWindows
{
    public class BoolToThemeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDark)
            {
                // Use Segoe MDL2 Assets icons
                return isDark ? "\uE706" : "\uE708"; // Moon for dark mode, Sun for light mode
            }
            return "\uE708"; // Default to sun icon
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}