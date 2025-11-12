using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1.Models
{
    public class Wire
    {
        public PortInfo Source { get; }
        public PortInfo Target { get; }
        public Path PathVisual { get; private set; }  

        public Wire(PortInfo source, PortInfo target)
        {
            Source = source;
            Target = target;

            PathVisual = new Path
            {
                Stroke = Brushes.LightGreen,
                StrokeThickness = 2
            };
        }

        public void UpdatePosition()
        {
            if (Source?.VisualEllipse == null || Target?.VisualEllipse == null)
                return;

            // pega as posições absolutas das elipses no canvas
            Point p1 = Source.VisualEllipse.TranslatePoint(
                new Point(Source.VisualEllipse.Width / 2, Source.VisualEllipse.Height / 2),
                Application.Current.MainWindow);

            Point p2 = Target.VisualEllipse.TranslatePoint(
                new Point(Target.VisualEllipse.Width / 2, Target.VisualEllipse.Height / 2),
                Application.Current.MainWindow);

            // cria o formato dobrado (em L ou Z)
            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = p1 };

            // ponto intermediário para o "L"
            double midX = (p1.X + p2.X) / 2;
            figure.Segments.Add(new LineSegment(new Point(midX, p1.Y), true));
            figure.Segments.Add(new LineSegment(new Point(midX, p2.Y), true));
            figure.Segments.Add(new LineSegment(p2, true));

            geometry.Figures.Add(figure);
            PathVisual.Data = geometry;

            // cor conforme sinal lógico
            PathVisual.Stroke = (Source.Value) ? Brushes.LimeGreen : Brushes.Gray;
        }

        public void UpdateColor()
        {
            if (PathVisual == null) return;
            PathVisual.Stroke = Source?.Value == true ? Brushes.LimeGreen : Brushes.Gray;
        }
    }
}
