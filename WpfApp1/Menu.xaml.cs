using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Lógica interna para Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu(String nomeUsuario)
        {
            InitializeComponent();
            lblBemVindo.Content = $"Olá, {nomeUsuario}!";
        }

        public Menu()
        {
            InitializeComponent();
        }

        private void btnSimularCircuito_Click(object sender, RoutedEventArgs e)
        {
            SimuladorCircuito simulador = new SimuladorCircuito();
            simulador.Show();
        }

        private void btnSimularKarnaugh_Click(object sender, RoutedEventArgs e)
        {
            MapaK mapaK = new MapaK();
            mapaK.Show();
        }
    }
}
