using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1.Models
{
    public class PortInfo
    {
        public GateModel Gate { get; set; }            
        public bool IsOutput { get; set; }             
        public int Index { get; set; }                 
        public bool Value { get; set; } = false;       
        public Ellipse VisualEllipse { get; set; }     
        public Wire ConnectedWire { get; set; }        

        public void ToggleState()
        {
            if (IsOutput) return;
            if (ConnectedWire != null) return;         

            Value = !Value;
            UpdateVisual();
        }

        public void SetState(bool state)
        {
            Value = state;
            VisualEllipse.Fill = state ? Brushes.LimeGreen : Brushes.Gray;
        }


        public void UpdateVisual()
        {
            if (VisualEllipse == null) return;
            VisualEllipse.Fill = Value ? Brushes.LimeGreen : Brushes.Gray;
        }
    }
}
