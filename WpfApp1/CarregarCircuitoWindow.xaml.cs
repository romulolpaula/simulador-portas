using System.Collections.Generic;
using System.Windows;
using WpfApp1.Banco_de_Dados; // IMPORTANTE pois CircuitoData está aqui

namespace WpfApp1
{
    public partial class CarregarCircuitoWindow : Window
    {
        public CircuitoData CircuitoSelecionado { get; private set; }

        public CarregarCircuitoWindow(List<CircuitoData> circuitos)
        {
            InitializeComponent();
            dgCircuitos.ItemsSource = circuitos;
        }

        private void BtnCarregar_Click(object sender, RoutedEventArgs e)
        {
            CircuitoSelecionado = (CircuitoData)dgCircuitos.SelectedItem;

            if (CircuitoSelecionado == null)
            {
                MessageBox.Show("Selecione um circuito!");
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
