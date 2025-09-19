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
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1
{
    /// <summary>
    /// Lógica interna para SimuladorCircuito.xaml
    /// </summary>
    public partial class SimuladorCircuito : Window
    {
        private CircuitManager circuit = new CircuitManager();
        private Type selectedGateType = null;
        private Dictionary<GateModel, GateControl> modelToControl = new Dictionary<GateModel, GateControl>();
        
        public SimuladorCircuito()
        {
            InitializeComponent();
        }

        private void cnvSimulador_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedGateType == null) return;

            var pos = e.GetPosition(cnvSimulador);
            GateModel model = (GateModel)Activator.CreateInstance(selectedGateType);
        }

    }
}
