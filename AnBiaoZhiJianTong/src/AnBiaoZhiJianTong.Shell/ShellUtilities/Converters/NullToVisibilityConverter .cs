using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnBiaoZhiJianTong.Shell.ShellUtilities.Converters
{
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type _, object __, CultureInfo ___)
            => value == null ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object _, Type __, object ___, CultureInfo ____)
            => throw new NotSupportedException();
    }
}
