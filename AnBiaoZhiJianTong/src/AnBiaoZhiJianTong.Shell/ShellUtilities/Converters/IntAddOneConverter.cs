using System;
using System.Globalization;
using System.Windows.Data;

namespace AnBiaoZhiJianTong.Shell.ShellUtilities.Converters
{
    public sealed class IntAddOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int i ? (i + 1).ToString() : "1";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
