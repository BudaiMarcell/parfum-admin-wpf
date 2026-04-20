using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ParfumAdmin_WPF.Helpers
{
    /// <summary>
    /// Készlet-szinthez szín:
    ///   0      -> piros  (elfogyott)
    ///   1..10  -> narancs (alacsony)
    ///   >10    -> zöld   (rendben)
    /// </summary>
    public class StockToColorConverter : IValueConverter
    {
        public const int LowStockThreshold = 10;

        private static readonly SolidColorBrush OutBrush  = Freeze(Color.FromRgb(220, 38, 38));   // #DC2626
        private static readonly SolidColorBrush LowBrush  = Freeze(Color.FromRgb(234, 88, 12));   // #EA580C
        private static readonly SolidColorBrush OkBrush   = Freeze(Color.FromRgb(22, 163, 74));   // #16A34A
        private static readonly SolidColorBrush NoneBrush = Freeze(Color.FromRgb(42, 42, 62));    // #2A2A3E

        private static SolidColorBrush Freeze(Color c)
        {
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
            {
                if (stock <= 0) return OutBrush;
                if (stock <= LowStockThreshold) return LowBrush;
                return OkBrush;
            }
            return NoneBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
