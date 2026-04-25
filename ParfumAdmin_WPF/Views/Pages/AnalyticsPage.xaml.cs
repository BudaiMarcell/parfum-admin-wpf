using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ParfumAdmin_WPF.ViewModels;
using ParfumAdmin_WPF.Views.Controls;

namespace ParfumAdmin_WPF.Views.Pages
{
    public partial class AnalyticsPage : Page
    {
        private readonly AnalyticsViewModel _vm;

        private static readonly Color AccentPurple = Color.FromRgb(0x7C, 0x3A, 0xED);
        private static readonly Color AccentBlue   = Color.FromRgb(0x60, 0xA5, 0xFA);
        private static readonly Color AccentGreen  = Color.FromRgb(0x34, 0xD3, 0x99);
        private static readonly Color AccentAmber  = Color.FromRgb(0xFB, 0xBF, 0x24);

        public AnalyticsPage(AnalyticsViewModel viewModel)
        {
            InitializeComponent();
            _vm = viewModel;
            DataContext = _vm;

            _vm.DataLoaded    += (_, __) => RedrawAll();
            _vm.MetricChanged += (_, __) => RedrawLine();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _vm.LoadAllAsync();
        }

        private void RedrawAll()
        {
            RedrawLine();
            HourlyChart.SetData(_vm.HourlyLabels, _vm.HourlyValues, AccentBlue);
            DrawDeviceBars();
            DrawFunnel();
        }

        private void RedrawLine()
        {
            if (_vm == null) return;
            var color = _vm.SelectedMetric switch
            {
                "Sessionök"    => AccentBlue,
                "Rendelések"   => AccentGreen,
                "Bevétel (Ft)" => AccentAmber,
                _              => AccentPurple,
            };
            var format = _vm.SelectedMetric == "Bevétel (Ft)" ? "N0" : "N0";
            DailyChart.SetData(_vm.DailyLabels, _vm.GetSelectedSeries(), color, format);
        }

        private void DrawDeviceBars()
        {
            DevicePanel.Children.Clear();
            var d = _vm.Devices;
            if (d == null) return;

            var rows = new[]
            {
                (Icon: "🖥", Label: "Desktop", Count: (double)d.Desktop, Color: AccentPurple),
                (Icon: "📱", Label: "Mobile",  Count: (double)d.Mobile,  Color: AccentBlue),
                (Icon: "📟", Label: "Tablet",  Count: (double)d.Tablet,  Color: AccentAmber),
            };

            double total = 0;
            foreach (var r in rows) total += r.Count;
            DeviceTotalText.Text = total > 0 ? $"Összesen: {total:N0} session" : "Nincs adat";

            var maxCount = 0.0;
            foreach (var r in rows) if (r.Count > maxCount) maxCount = r.Count;

            // A DevicePanel (XAML-ben Grid "*"/"*"/"*" sorokkal) 3 egyforma magas
            // sávra osztja a rendelkezésre álló függőleges helyet. Itt minden bárt
            // középre igazítunk a saját során belül, fix magassággal — így a sávok
            // közötti "rés" egyenletes, és alul nincs kihasználatlan hely.
            const double BarHeight = 40;

            int rowIndex = 0;
            foreach (var r in rows)
            {
                double ratio = maxCount > 0 ? r.Count / maxCount : 0;
                double pct   = total > 0    ? r.Count / total    : 0;

                var row = new Grid { VerticalAlignment = VerticalAlignment.Center };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

                // Ikon + név
                var labelText = new TextBlock
                {
                    Text = $"{r.Icon}  {r.Label}",
                    Foreground = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xEE)),
                    FontSize = 15,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(labelText, 0);
                row.Children.Add(labelText);

                // Sáv
                var track = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x33)),
                    CornerRadius = new CornerRadius(8),
                    Height = BarHeight
                };
                var barHost = new Grid();
                barHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(System.Math.Max(0.0001, ratio), GridUnitType.Star) });
                barHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(System.Math.Max(0.0001, 1 - ratio), GridUnitType.Star) });

                var fill = new Border
                {
                    Background = new SolidColorBrush(r.Color),
                    CornerRadius = new CornerRadius(8),
                    Height = BarHeight,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                Grid.SetColumn(fill, 0);
                barHost.Children.Add(fill);

                var trackGrid = new Grid();
                trackGrid.Children.Add(track);
                trackGrid.Children.Add(barHost);
                Grid.SetColumn(trackGrid, 1);
                row.Children.Add(trackGrid);

                // Szám + százalék
                var countText = new TextBlock
                {
                    Text = total > 0 ? $"{r.Count:N0}  ·  {pct * 100:N0}%" : "0",
                    Foreground = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xFF)),
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(12, 0, 0, 0)
                };
                Grid.SetColumn(countText, 2);
                row.Children.Add(countText);

                Grid.SetRow(row, rowIndex++);
                DevicePanel.Children.Add(row);
            }
        }

        private void DrawFunnel()
        {
            FunnelPanel.Children.Clear();
            var f = _vm.Funnel;
            if (f == null) return;

            var stages = new[]
            {
                (Label: "Oldalmegtekintések", Count: f.Pageviews,    Color: AccentPurple),
                (Label: "Kosárba tétel",      Count: f.AddToCarts,   Color: AccentBlue),
                (Label: "Checkout",           Count: f.Checkouts,    Color: AccentAmber),
                (Label: "Kifizetett rendelés", Count: f.Orders,      Color: AccentGreen),
            };

            double top = stages[0].Count;
            double prev = top;

            foreach (var s in stages)
            {
                double ratio       = top > 0 ? s.Count / top : 0;
                double stepRatio   = prev > 0 ? s.Count / prev : 0;

                var row = new Grid { Margin = new Thickness(0, 0, 0, 10) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

                // Label
                var lbl = new TextBlock
                {
                    Text = s.Label,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xDD)),
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(lbl, 0);
                row.Children.Add(lbl);

                // Bar track + fill
                var track = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x33)),
                    CornerRadius = new CornerRadius(6),
                    Height = 22
                };
                var trackGrid = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch };
                var fill = new Border
                {
                    Background = new SolidColorBrush(s.Color),
                    CornerRadius = new CornerRadius(6),
                    Height = 22,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                // Width binding — use a child Rectangle via Grid with star-weighted columns
                var barHost = new Grid();
                barHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(System.Math.Max(0.0001, ratio), GridUnitType.Star) });
                barHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(System.Math.Max(0.0001, 1 - ratio), GridUnitType.Star) });
                Grid.SetColumn(fill, 0);
                barHost.Children.Add(fill);
                trackGrid.Children.Add(track);
                trackGrid.Children.Add(barHost);
                Grid.SetColumn(trackGrid, 1);
                row.Children.Add(trackGrid);

                // Count + step conversion
                var countText = new TextBlock
                {
                    Text = $"{s.Count:N0}"
                         + (prev > 0 && prev != s.Count ? $"  ·  {stepRatio * 100:N1}%" : ""),
                    Foreground = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xFF)),
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(12, 0, 0, 0)
                };
                Grid.SetColumn(countText, 2);
                row.Children.Add(countText);

                FunnelPanel.Children.Add(row);
                prev = s.Count;
            }
        }
    }
}
