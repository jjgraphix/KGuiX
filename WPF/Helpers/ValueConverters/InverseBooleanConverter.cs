using System;
using System.Globalization;
using System.Windows.Data;

namespace KGuiX.Helpers.ValueConverters
{
    internal class InverseBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Get the inverse of an existing boolean value or existence of an empty string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (Boolean.TryParse(System.Convert.ToString(value, culture), out var isBool)) ? !isBool :
                                                !(String.IsNullOrEmpty(System.Convert.ToString(value, culture)));

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}