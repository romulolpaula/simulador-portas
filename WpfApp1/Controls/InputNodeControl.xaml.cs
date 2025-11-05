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
            model.OnValueChanged += UpdateVisual;
            UpdateVisual();
        }

        private void ChkValue_Checked(object sender, RoutedEventArgs e)
        {
            if (Model != null)
            {
                Model.Value = true;
            }
            CircuitUpdated();
        }

        private void ChkValue_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Model != null)
            {
                Model.Value = false;
            }
            CircuitUpdated();
        }

        private void CircuitUpdated()
        {
            (Application.Current.MainWindow as SimuladorCircuito)?.EvaluateAll();
        }

        public void UpdateVisual()
        {
            if (Model == null) return;

            // Cor da bolinha (verde se ativo, cinza se inativo)
            Output.Fill = Model.Value ? Brushes.LimeGreen : Brushes.Gray;

            // Sincroniza o CheckBox com o estado lógico
            chkValue.IsChecked = Model.Value;
        }

        // Quando o usuário clica na bolinha (para criar fio)
        private void OutputPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OutputPortClickedEvent, this));
        }

        // Evento personalizado (dispara para o SimuladorCircuito)
        public static readonly RoutedEvent OutputPortClickedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(OutputPortClicked),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(InputNodeControl)
            );

        // Permite que o simulador se inscreva nesse evento
        public event RoutedEventHandler OutputPortClicked
        {
            add { AddHandler(OutputPortClickedEvent, value); }
            remove { RemoveHandler(OutputPortClickedEvent, value); }
        }
    }
}
