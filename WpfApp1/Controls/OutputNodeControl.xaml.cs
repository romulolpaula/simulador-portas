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
            Model = Model;
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

        // Quando o usuário tenta conectar um fio na entrada
        private void InputPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_node == null) return;
            ((SimuladorCircuito)Application.Current.MainWindow)?.IniciarLigacao(_node, "input");
        }
    }
}
