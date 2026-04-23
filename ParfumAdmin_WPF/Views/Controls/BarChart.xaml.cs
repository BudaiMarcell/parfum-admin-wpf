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
    /// Simple vertical bar chart on a Canvas.
    /// </summary>
    public partial class BarChart : UserControl
    {
        private IList<string> _labels = new List<string>();
        private IList<double> _values = new List<double>();
        private Color         _accent = Color.FromRgb(0x7C, 0x3A, 0xED);

        private static readonly Brush GridBrush  = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x44));
        private static readonly Brush AxisBrush  = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x66));
        private static readonly Brush LabelBrush = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x99));

        public BarChart()
        {
            InitializeComponent();
        }

        public void SetData(IList<string> labels, IList<double> values, Color? accentColor = null)
        {
            _labels = labels ?? new List<string>();
            _values = values ?? new List<double>();
            if (accentColor.HasValue) _accent = accentColor.Value;
            Render();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) => Render();

        private void Render()
        {
            ChartCanvas.Children.Clear();

            double width  = ChartCanvas.ActualWidth;
            double height = ChartCanvas.ActualHeight;
            if (width < 20 || height < 20 || _values.Count == 0) return;

            const double padLeft   = 40;
            const double padRight  = 10;
            const double padTop    = 12;
            const double padBottom = 28;

            double plotW = Math.Max(10, width  - padLeft - padRight);
            double plotH = Math.Max(10, height - padTop  - padBottom);

            double max = _values.Count > 0 ? Math.Max(1, _values.Max()) : 1;

            // Y-axis grid lines
            for (int i = 0; i <= 4; i++)
            {
                double y = padTop + plotH * (1 - i / 4.0);
                ChartCanvas.Children.Add(new Line
                {
                    X1 = padLeft, X2 = padLeft + plotW,
                    Y1 = y, Y2 = y,
                    Stroke = GridBrush, StrokeThickness = 1
                });

                double val = max * (i / 4.0);
                var lbl = new TextBlock
                {
                    Text = FormatShort(val),
                    Foreground = LabelBrush,
                    FontSize = 10
                };
                Canvas.SetLeft(lbl, 2);
                Canvas.SetTop(lbl, y - 8);
                ChartCanvas.Children.Add(lbl);
            }

            // Axis
            ChartCanvas.Children.Add(new Line
            {
                X1 = padLeft, X2 = padLeft + plotW,
                Y1 = padTop + plotH, Y2 = padTop + plotH,
                Stroke = AxisBrush, StrokeThickness = 1
            });

            // Bars
            int n = _values.Count;
            double slot    = plotW / n;
            double barW    = Math.Max(4, slot * 0.6);

            for (int i = 0; i < n; i++)
            {
                double h = plotH * (_values[i] / max);
                double x = padLeft + slot * i + (slot - barW) / 2.0;
                double y = padTop + plotH - h;

                var rect = new Rectangle
                {
                    Width  = barW,
                    Height = h,
                    Fill   = new LinearGradientBrush(
                        Color.FromArgb(0xFF, _accent.R, _accent.G, _accent.B),
                        Color.FromArgb(0x88, _accent.R, _accent.G, _accent.B),
                        90),
                    RadiusX = 3, RadiusY = 3
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                ChartCanvas.Children.Add(rect);

                // Label under bar
                if (i < _labels.Count)
                {
                    var lbl = new TextBlock
                    {
                        Text = _labels[i],
                        Foreground = LabelBrush,
                        FontSize = 10
                    };
                    lbl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    // Stride labels on crowded charts
                    int stride = Math.Max(1, (int)Math.Ceiling(n / Math.Max(1, plotW / 40.0)));
                    if (i % stride == 0)
                    {
                        Canvas.SetLeft(lbl, x + (barW - lbl.DesiredSize.Width) / 2);
                        Canvas.SetTop(lbl, padTop + plotH + 6);
                        ChartCanvas.Children.Add(lbl);
                    }
                }
            }
        }

        private string FormatShort(double v)
        {
            if (v >= 1000) return (v / 1000.0).ToString("0.#") + "k";
            return v.ToString("0", CultureInfo.InvariantCulture);
        }
    }
}
