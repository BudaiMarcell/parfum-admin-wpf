using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ParfumAdmin_WPF.Helpers
{
    // Ha az érték (aktuális státusz) egyezik a ConverterParameterrel,
    // akkor élénk lila, különben a szokásos sötét háttér.
    public class StatusMatchToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush ActiveBrush  = Freeze(Color.FromRgb(0x7C, 0x3A, 0xED)); // #7C3AED
        private static readonly SolidColorBrush DefaultBrush = Freeze(Color.FromRgb(0x3D, 0x2B, 0x6B)); // #3D2B6B

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var current = value as string;
            var expected = parameter as string;
            if (!string.IsNullOrEmpty(current) && string.Equals(current, expected, StringComparison.OrdinalIgnoreCase))
                return ActiveBrush;
            return DefaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private static SolidColorBrush Freeze(Color c)
        {
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }
    }
}
