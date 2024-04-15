using System;
using System.Globalization;
using System.Windows.Data;

namespace KGuiX.Helpers.ValueConverters
{
    internal class IsGreaterThanConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return decimal.TryParse(System.Convert.ToString(value, culture), out var num1)
                && decimal.TryParse(System.Convert.ToString(parameter, culture), out var num2)
                && num1 > num2;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
