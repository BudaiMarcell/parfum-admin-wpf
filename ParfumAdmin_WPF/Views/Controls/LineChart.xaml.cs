using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ParfumAdmin_WPF.Views.Controls
{
    /// <summary>
    /// Lightweight line chart rendered on a Canvas — no external deps.
    /// Call SetData(labels, values, accentColor, valueFormat) and it draws.
    /// </summary>
    public partial class LineChart : UserControl
    {
        private IList<string> _labels = new List<string>();
        private IList<double> _values = new List<double>();
        private Color         _accent = Color.FromRgb(0x7C, 0x3A, 0xED);
        private string        _format = "N0";

        private static readonly Brush GridBrush   = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x44));
        private static readonly Brush AxisBrush   = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x66));
        private static readonly Brush LabelBrush  = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x99));

        public LineChart()
        {
            InitializeComponent();
        }

        public void SetData(IList<string> labels, IList<double> values, Color? accentColor = null, string valueFormat = "N0")
        {
            _labels = labels ?? new List<string>();
            _values = values ?? new List<double>();
            if (accentColor.HasValue) _accent = accentColor.Value;
            _format = valueFormat;
            Render();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) => Render();

        private void Render()
        {
            ChartCanvas.Children.Clear();

            double width  = ChartCanvas.ActualWidth;
            double height = ChartCanvas.ActualHeight;
            if (width < 20 || height < 20 || _values.Count == 0) return;

            // Layout: leave room for axes labels
            const double padLeft   = 48;
            const double padRight  = 12;
            const double padTop    = 12;
            const double padBottom = 28;

            double plotW = Math.Max(10, width  - padLeft - padRight);
            double plotH = Math.Max(10, height - padTop  - padBottom);

            double max = _values.Count > 0 ? _values.Max() : 0;
            double min = 0;
            if (max == 0) max = 1; // avoid div-by-zero

            // Y-axis grid lines (4 horizontal)
            for (int i = 0; i <= 4; i++)
            {
                double y = padTop + plotH * (1 - i / 4.0);
                var line = new Line
                {
                    X1 = padLeft, X2 = padLeft + plotW,
                    Y1 = y, Y2 = y,
                    Stroke = GridBrush, StrokeThickness = 1
                };
                ChartCanvas.Children.Add(line);

                double val = min + (max - min) * (i / 4.0);
                var lbl = new TextBlock
                {
                    Text = FormatValue(val),
                    Foreground = LabelBrush,
                    FontSize = 10
                };
                Canvas.SetLeft(lbl, 2);
                Canvas.SetTop(lbl, y - 8);
                ChartCanvas.Children.Add(lbl);
            }

            // X-axis line
            ChartCanvas.Children.Add(new Line
            {
                X1 = padLeft, X2 = padLeft + plotW,
                Y1 = padTop + plotH, Y2 = padTop + plotH,
                Stroke = AxisBrush, StrokeThickness = 1
            });

            // Build points
            int n = _values.Count;
            double stepX = n > 1 ? plotW / (n - 1) : 0;
            var points = new PointCollection();
            for (int i = 0; i < n; i++)
            {
                double x = padLeft + stepX * i;
                double y = padTop + plotH * (1 - (_values[i] - min) / (max - min));
                points.Add(new Point(x, y));
            }

            // Area fill under the line
            if (points.Count > 1)
            {
                var area = new Polygon
                {
                    Points = new PointCollection(points) { new Point(points.Last().X, padTop + plotH), new Point(points.First().X, padTop + plotH) },
                    Fill = new LinearGradientBrush(
                        Color.FromArgb(0x55, _accent.R, _accent.G, _accent.B),
                        Color.FromArgb(0x00, _accent.R, _accent.G, _accent.B),
                        90),
                };
                ChartCanvas.Children.Add(area);
            }

            // Line polyline
            if (points.Count > 1)
            {
                var line = new Polyline
                {
                    Points = points,
                    Stroke = new SolidColorBrush(_accent),
                    StrokeThickness = 2,
                    StrokeLineJoin = PenLineJoin.Round
                };
                ChartCanvas.Children.Add(line);
            }

            // Data dots
            foreach (var p in points)
            {
                var dot = new Ellipse
                {
                    Width = 6, Height = 6,
                    Fill = new SolidColorBrush(_accent),
                    Stroke = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E)),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(dot, p.X - 3);
                Canvas.SetTop(dot, p.Y - 3);
                ChartCanvas.Children.Add(dot);
            }

            // X-axis labels (stride so they don't overlap)
            int stride = Math.Max(1, (int)Math.Ceiling(n / Math.Max(1, plotW / 60.0)));
            for (int i = 0; i < _labels.Count; i += stride)
            {
                double x = padLeft + stepX * i;
                var lbl = new TextBlock
                {
                    Text = _labels[i],
                    Foreground = LabelBrush,
                    FontSize = 10
                };
                lbl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(lbl, x - lbl.DesiredSize.Width / 2);
                Canvas.SetTop(lbl, padTop + plotH + 6);
                ChartCanvas.Children.Add(lbl);
            }
        }

        private string FormatValue(double v)
        {
            if (_format.StartsWith("C", StringComparison.OrdinalIgnoreCase))
                return v.ToString(_format, CultureInfo.InvariantCulture);
            if (v >= 1000) return (v / 1000.0).ToString("0.#") + "k";
            return v.ToString(_format, CultureInfo.InvariantCulture);
        }
    }
}
