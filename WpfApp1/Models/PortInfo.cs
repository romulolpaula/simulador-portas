using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1.Models
{
    public class PortInfo
    {
        public GateModel Gate { get; set; }            // referência ao modelo lógico (opcional)
        public bool IsOutput { get; set; }             // true se for saída
        public int Index { get; set; }                 // índice do pino (0/1)
        public bool Value { get; set; } = false;       // valor lógico público
        public Ellipse VisualEllipse { get; set; }     // elipse associada
        public Wire ConnectedWire { get; set; }        // referência ao fio (null se livre)

        public void ToggleState()
        {
            if (IsOutput) return;                      // não toggla saída
            if (ConnectedWire != null) return;         // se conectado, não pode alterar manualmente

            Value = !Value;
            UpdateVisual();
        }

        public void SetState(bool val)
        {
            Value = val;
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            if (VisualEllipse == null) return;
            VisualEllipse.Fill = Value ? Brushes.LimeGreen : Brushes.Gray;
        }
    }
}
