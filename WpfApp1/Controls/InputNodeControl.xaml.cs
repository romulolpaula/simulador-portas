using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp1.Models;

namespace WpfApp1.Controls
{
    public partial class InputNodeControl : UserControl
    {
        public InputNode Model { get; private set; }

        public InputNodeControl()
        {
            InitializeComponent();
        }

        public void Initialize(InputNode model)
        {
            Model = model;
            UpdateVisual();
        }

        private void ChkValue_Checked(object sender, RoutedEventArgs e)
        {
            Model.Value = true;
            CircuitUpdated();
        }

        private void ChkValue_Unchecked(object sender, RoutedEventArgs e)
        {
            Model.Value = false;
            CircuitUpdated();
        }

        private void CircuitUpdated()
        {
            (Application.Current.MainWindow as SimuladorCircuito)?.EvaluateAll();
        }

        public void UpdateVisual()
        {
            Output.Fill = Model.Output ? Brushes.Green : Brushes.Gray;
        }

        private void OutputPort_MouseDown(object sender, MouseButtonEventArgs e)
            => RaiseEvent(new RoutedEventArgs(OutputPortClickedEvent, this));

        public static readonly RoutedEvent OutputPortClickedEvent =
            EventManager.RegisterRoutedEvent("OutputPortClicked", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(InputNodeControl));
    }
}
