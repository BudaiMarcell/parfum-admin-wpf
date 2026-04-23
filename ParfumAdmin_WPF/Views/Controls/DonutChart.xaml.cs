using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ParfumAdmin_WPF.Views.Controls
{
    public class DonutSegment
    {
        public string Label { get; set; }
        public double Value { get; set; }
        public Color  Color { get; set; }
    }

    /// <summary>
    /// Simple donut chart with a legend.
    /// </summary>
    public partial class DonutChart : UserControl
    {
        private IList<DonutSegment> _segments = new List<DonutSegment>();

        private static readonly Brush LabelBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xDD));
        private static readonly Brush SubBrush   = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x99));
        private static readonly Brush CenterBg   = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E));

        public DonutChart()
        {
            InitializeComponent();
        }

        public void SetData(IList<DonutSegment> segments)
        {
            _segments = segments ?? new List<DonutSegment>();
            RenderLegend();
            RenderChart();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) => RenderChart();

        private void RenderLegend()
        {
            LegendPanel.Children.Clear();
            double total = 0;
            foreach (var s in _segments) total += Math.Max(0, s.Value);

            foreach (var s in _segments)
            {
                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 4, 0, 4)
                };

                var swatch = new Rectangle
                {
                    Width = 12, Height = 12,
                    Fill = new SolidColorBrush(s.Color),
                    RadiusX = 2, RadiusY = 2,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                row.Children.Add(swatch);

                var text = new TextBlock
                {
                    Text = $"{s.Label}  ·  {s.Value:N0}"
                         + (total > 0 ? $"  ·  {(s.Value / total) * 100:N0}%" : ""),
                    Foreground = LabelBrush,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                };
                row.Children.Add(text);
                LegendPanel.Children.Add(row);
            }
        }

        private void RenderChart()
        {
            ChartCanvas.Children.Clear();
            double w = ChartCanvas.ActualWidth;
            double h = ChartCanvas.ActualHeight;
            if (w < 20 || h < 20 || _segments.Count == 0) return;

            double diameter = Math.Min(w, h) - 20;
            double radius   = diameter / 2.0;
            double inner    = radius * 0.6;
            double cx       = w / 2.0;
            double cy       = h / 2.0;

            double total = 0;
            foreach (var s in _segments) total += Math.Max(0, s.Value);
            if (total <= 0)
            {
                // Empty state: just a ring outline
                var ring = new Ellipse
                {
                    Width = diameter, Height = diameter,
                    Stroke = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x55)),
                    StrokeThickness = 2,
                    Fill = Brushes.Transparent,
                };
                Canvas.SetLeft(ring, cx - radius);
                Canvas.SetTop(ring, cy - radius);
                ChartCanvas.Children.Add(ring);
                return;
            }

            double startAngle = -Math.PI / 2; // 12 o'clock

            foreach (var seg in _segments)
            {
                if (seg.Value <= 0) continue;
                double sweep = (seg.Value / total) * Math.PI * 2;
                double endAngle = startAngle + sweep;

                var figure = BuildRingSlice(cx, cy, radius, inner, startAngle, endAngle);
                var path = new Path
                {
                    Fill = new SolidColorBrush(seg.Color),
                    Data = new PathGeometry { Figures = { figure } }
                };
                ChartCanvas.Children.Add(path);

                startAngle = endAngle;
            }

            // Total in the middle
            var totalText = new TextBlock
            {
                Text = total.ToString("N0"),
                Foreground = LabelBrush,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
            };
            totalText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(totalText, cx - totalText.DesiredSize.Width / 2);
            Canvas.SetTop(totalText, cy - totalText.DesiredSize.Height / 2 - 6);
            ChartCanvas.Children.Add(totalText);

            var subText = new TextBlock
            {
                Text = "összesen",
                Foreground = SubBrush,
                FontSize = 10
            };
            subText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(subText, cx - subText.DesiredSize.Width / 2);
            Canvas.SetTop(subText, cy + 10);
            ChartCanvas.Children.Add(subText);
        }

        private static PathFigure BuildRingSlice(double cx, double cy, double outerR, double innerR, double a0, double a1)
        {
            Point p(double r, double a) => new Point(cx + r * Math.Cos(a), cy + r * Math.Sin(a));

            var fig = new PathFigure { StartPoint = p(outerR, a0), IsClosed = true };
            bool isLarge = (a1 - a0) > Math.PI;

            fig.Segments.Add(new ArcSegment(p(outerR, a1), new Size(outerR, outerR),
                0, isLarge, SweepDirection.Clockwise, true));
            fig.Segments.Add(new LineSegment(p(innerR, a1), true));
            fig.Segments.Add(new ArcSegment(p(innerR, a0), new Size(innerR, innerR),
                0, isLarge, SweepDirection.Counterclockwise, true));
            fig.Segments.Add(new LineSegment(p(outerR, a0), true));
            return fig;
        }
    }
}
