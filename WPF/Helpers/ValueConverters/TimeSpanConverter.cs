using System;
using System.Globalization;
using System.Windows.Data;

namespace KGuiX.Helpers.ValueConverters
{
    internal class TimeSpanConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string formattedTime = string.Empty;

            if (value is TimeSpan timeSpan)
            {
                /* Supported format strings:
                    'FullDay' = 2 Days 01:30:00
                    'ShortDay' = 2d 01:30:00
                */
                string format = (parameter != null) ? parameter.ToString() : "";

                int totalHours = (int)timeSpan.TotalHours;
                int days = totalHours / 24;
                int hours = totalHours % 24;
                int min = timeSpan.Minutes;
                int sec = timeSpan.Seconds;

                if (format.EndsWith("Day"))
                {
                    // Console.WriteLine($"Format: {format}");    // DEBUG
                    if (days > 0)
                    {
                        if (format == "FullDay")
                            formattedTime += (days == 1) ? $"1 Day " : $"{days} Days ";
                        else
                            formattedTime += $"{days}d ";       // Uses 'ShortDay' format
                    }

                    formattedTime += (hours > 0) ? $"{hours}:" : "";           // Hours without placeholder
                }
                else
                {
                    formattedTime += (days > 0) ? $"{days}:{hours:D2}:" : $"{hours:D2}:";
                }

                formattedTime += $"{min:D2}:{sec:D2}";
            }

            return formattedTime;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
