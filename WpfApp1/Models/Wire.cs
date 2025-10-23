using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1.Models
{
    public class Wire //classe para o fio de conexão entre as portas
    {
        public GateModel Source { get; set; }
        public GateModel Target { get; set; }
        public int TargetInputIndex { get; set; } //se quiser indexar entradas específicas em portas com múltiplas entradas

        public Line LineShape { get; private set; } //linha desenhada no canvas
        public Brush ActiveColor { get; set; } = Brushes.LimeGreen; //cor quando o fio está ativo (lógico 1)
        public Brush InactiveColor { get; set; } = Brushes.Gray; //cor quando o fio está inativo (lógico 0)

        public Wire(GateModel source, GateModel target, int targetInputIndex = 0)
        {
            Source = source;
            Target = target;
            TargetInputIndex = targetInputIndex;

            LineShape = new Line //cria visual inicial
            {
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };
        }

        public void UpdatePosition(double x1, double y1, double x2, double y2)
        {
            LineShape.X1 = x1;
            LineShape.Y1 = y1;
            LineShape.X2 = x2;
            LineShape.Y2 = y2;
        }

        public void UpdateColor(bool Active)
        {
            LineShape.Stroke = Active ? Brushes.LimeGreen : Brushes.Gray;
        }

    }
}
