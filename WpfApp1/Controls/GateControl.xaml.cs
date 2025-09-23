using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfApp1.Models;

namespace WpfApp1.Controls
{
    public partial class GateControl : UserControl
    {
        public GateModel Model { get; private set; }

        public GateControl()
        {
            InitializeComponent();
        }

        public void Initialize(GateModel model, string svgFile)
        {
            Model = model;
            GateImage.Source = new System.Uri($"pack://application:,,,/Images/{svgFile}", UriKind.Absolute);
            UpdatePortVisuals();
        }

        public void UpdatePortVisuals()
        {
            // Entrada
            if (Model.Inputs.Count > 0)
                Input0.Fill = Model.Inputs[0].Output ? Brushes.Green : Brushes.Gray;
            if (Model.Inputs.Count > 1)
                Input1.Fill = Model.Inputs[1].Output ? Brushes.Green : Brushes.Gray;

            // Saída
            Output.Fill = Model.Output ? Brushes.Green : Brushes.Gray;
        }

        private void Input_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ellipse = sender as Ellipse;
            int index = ellipse == Input0 ? 0 : 1;
            RaiseEvent(new InputPortClickedEventArgs(InputPortClickedEvent, this, index));
        }

        private void Output_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OutputPortClickedEvent, this));
        }

        // Evento da entrada
        public static readonly RoutedEvent InputPortClickedEvent =
            EventManager.RegisterRoutedEvent("InputPortClicked", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(GateControl));

        public class InputPortClickedEventArgs : RoutedEventArgs
        {
            public int InputIndex { get; }

            public InputPortClickedEventArgs(RoutedEvent routedEvent, object source, int index)
                : base(routedEvent, source)
            {
                InputIndex = index;
            }
        }

        // Evento da saída
        public static readonly RoutedEvent OutputPortClickedEvent =
            EventManager.RegisterRoutedEvent("OutputPortClicked", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(GateControl));
    }
}
