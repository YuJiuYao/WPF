using System;
using System.Globalization;
using System.Windows.Data;

namespace AnBiaoZhiJianTong.Shell.ShellUtilities.Converters
{
    public class IsSelectedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string selectedButton = values[0] as string;
            string buttonName = values[1] as string;
            return buttonName == selectedButton;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
