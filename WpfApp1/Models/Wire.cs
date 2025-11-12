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
            if (PathVisual == null || Source?.VisualEllipse == null || Target?.VisualEllipse == null)
                return;

            // Pega a posição absoluta das bolinhas
            Point p1 = Source.VisualEllipse.TranslatePoint(
                new Point(Source.VisualEllipse.Width / 2, Source.VisualEllipse.Height / 2),
                Application.Current.MainWindow);
            Point p2 = Target.VisualEllipse.TranslatePoint(
                new Point(Target.VisualEllipse.Width / 2, Target.VisualEllipse.Height / 2),
                Application.Current.MainWindow);

            // desenha fio dobrado (em L)
            double midX = (p1.X + p2.X) / 2;

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure { StartPoint = p1 };

            // Caminho: saída → dobra no meio → entrada
            figure.Segments.Add(new LineSegment(new Point(midX, p1.Y), true));
            figure.Segments.Add(new LineSegment(new Point(midX, p2.Y), true));
            figure.Segments.Add(new LineSegment(p2, true));

            geometry.Figures.Add(figure);
            PathVisual.Data = geometry;
        }


        public void UpdateColor()
        {
            if (PathVisual == null || Source == null) return;

            bool ativo = Source.IsOutput ? Source.Gate.Output : Source.Value;
            PathVisual.Stroke = ativo ? Brushes.LimeGreen : Brushes.Gray;
            PathVisual.StrokeThickness = 2;
        }

    }
}
