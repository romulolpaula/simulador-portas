using System;
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

        public PortInfo[] Inputs { get; private set; }
        public PortInfo OutputPort { get; private set; }

        public GateControl()
        {
            InitializeComponent();
        }

        public void Initialize(GateModel model, string svgFile)
        {
            Model = model;

            Inputs = new[]
            {
                new PortInfo { Gate = model, IsOutput = false, Index = 0, VisualEllipse = Input0 },
                new PortInfo { Gate = model, IsOutput = false, Index = 1, VisualEllipse = Input1 }
            };
            OutputPort = new PortInfo { Gate = model, IsOutput = true, Index = 0, VisualEllipse = Output };

            Input0.Tag = Inputs[0];
            Input1.Tag = Inputs[1];
            Output.Tag = OutputPort;

            // liga o evento no XAML para Port_MouseDown ou aqui:
            Input0.MouseDown += Port_MouseDown;
            Input1.MouseDown += Port_MouseDown;
            Output.MouseDown += Port_MouseDown;

            GateImage.Source = new Uri($"pack://application:,,,/Images/{svgFile}", UriKind.Absolute);

            UpdatePortVisuals();
        }

        public void UpdatePortVisuals()
        {
            if (Inputs != null)
            {
                for (int i = 0; i < Inputs.Length; i++)
                {
                    var p = Inputs[i];
                    if (p?.VisualEllipse != null)
                        p.VisualEllipse.Fill = p.Value ? Brushes.LimeGreen : Brushes.Gray;
                }
            }

            if (OutputPort?.VisualEllipse != null && Model != null)
            {
                // sincroniza valor da saída com o modelo lógico
                OutputPort.Value = Model.Output;
                OutputPort.VisualEllipse.Fill = OutputPort.Value ? Brushes.LimeGreen : Brushes.Gray;
            }
        }


        private void Port_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse ellipse && ellipse.Tag is PortInfo port)
            {
                e.Handled = true; // impede o clique de se perder

                Console.WriteLine($"Clique detectado em {(port.IsOutput ? "Saída" : "Entrada")} de {port.Gate.GetType().Name}");

                // Propaga corretamente o evento com o PortInfo no OriginalSource
                RoutedEventArgs args = new RoutedEventArgs(
                    port.IsOutput ? OutputPortClickedEvent : InputPortClickedEvent,
                    port
                );
                RaiseEvent(args);
            }
        }


        private void OutputPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Avança o evento para o SimuladorCircuito
            RaiseEvent(new RoutedEventArgs(OutputPortClickedEvent, sender));
        }

        private void InputPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Avança o evento para o SimuladorCircuito
            RaiseEvent(new RoutedEventArgs(InputPortClickedEvent, sender));
        }

        // kept events for SimuladorCircuito to attach
        public static readonly RoutedEvent OutputPortClickedEvent =
        EventManager.RegisterRoutedEvent("OutputPortClicked",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GateControl));

        public static readonly RoutedEvent InputPortClickedEvent =
            EventManager.RegisterRoutedEvent("InputPortClicked",
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GateControl));

    }
}
