using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1.Models
{
    public class Wire
    {
        public PortInfo Source { get; }
        public PortInfo Target { get; }
        public Path PathVisual { get; }

        private PathGeometry geometry;

        public Wire(PortInfo source, PortInfo target)
        {
            Source = source;
            Target = target;

            geometry = new PathGeometry();

            PathVisual = new Path
            {
                Stroke = Brushes.Gray,
                StrokeThickness = 2,
                Data = geometry
            };
        }

        public void UpdateGeometry(Point p1, Point p2, double midX)
        {
            if (PathVisual == null)
                return;

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure { StartPoint = p1 };

            // Caminho dobrado usando midX deslocado
            figure.Segments.Add(new LineSegment(new Point(midX, p1.Y), true));
            figure.Segments.Add(new LineSegment(new Point(midX, p2.Y), true));
            figure.Segments.Add(new LineSegment(p2, true));

            geometry.Figures.Add(figure);
            PathVisual.Data = geometry;
        }



        public void UpdateColor()
        {
            if (Source == null) return;

            bool ativo = Source.IsOutput ? Source.Gate.Output : Source.Value;

            PathVisual.Stroke = ativo ? Brushes.LimeGreen : Brushes.Gray;
        }
    }
}
