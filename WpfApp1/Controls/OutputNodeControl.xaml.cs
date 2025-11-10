using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp1.Models;

namespace WpfApp1.Controls
{
    public partial class OutputNodeControl : UserControl
    {
        private OutputNode _node;

        public OutputNode Model { get; private set; }

        public OutputNodeControl()
        {
            InitializeComponent();
        }

        public void Initialize(OutputNode node)
        {
            Model = node;
            _node = node;
            UpdateDisplay();
        }

        // Atualiza a cor conforme o valor lógico da saída
        public void UpdateDisplay()
        {
            if (_node == null) return;

            DisplayLight.Fill = _node.Output
                ? Brushes.LimeGreen
                : Brushes.Gray;
        }
        public static readonly RoutedEvent InputPortClickedEvent =
        EventManager.RegisterRoutedEvent("InputPortClicked",
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(OutputNodeControl));

        public event RoutedEventHandler InputPortClicked
        {
            add { AddHandler(InputPortClickedEvent, value); }
            remove { RemoveHandler(InputPortClickedEvent, value); }
        }

        private void InputPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_node == null) return;

            RaiseEvent(new RoutedEventArgs(InputPortClickedEvent, this));
        }

    }
}
