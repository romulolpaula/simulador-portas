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
using WpfApp1.Controls;
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
            circuit.Gates.Add(model);

            var control = new GateControl();
            string imageName = GetImageNameForGate(selectedGateType);
            control.Initialize(model, imageName);

            Canvas.SetLeft(control, pos.X - control.Width / 2);
            Canvas.SetTop(control, pos.Y - control.Height / 2);
            cnvSimulador.Children.Add(control);

            modelToControl[model] = control;
            selectedGateType = null; //reseta depois de colocar
        }

        //associa o tipo da porta à imagem correspondente 
        private string GetImageNameForGate(Type gateType)
        {
            if (gateType == typeof(AndGate)) return "and.png";
            if (gateType == typeof(OrGate)) return "or.png";
            if (gateType == typeof(NotGate)) return "not.png";
            if (gateType == typeof(XorGate)) return "xor.png";
            if (gateType == typeof(NandGate)) return "nand.png";
            if (gateType == typeof(NorGate)) return "nor.png";
            if (gateType == typeof(XnorGate)) return "xnor.png";
            return "default.png";
        }

        private void cmbPorta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPorta.SelectedItem is System.Windows.Controls.ComboBoxItem item)
            {
                switch (item.Content.ToString())
                {
                    case "Porta AND": selectedGateType = typeof(AndGate); break;
                    case "Porta NAND": selectedGateType = typeof(NandGate); break;
                    case "Porta OR": selectedGateType = typeof(OrGate); break;
                    case "Porta NOR": selectedGateType = typeof(NorGate); break;
                    case "Porta XOR": selectedGateType = typeof(XorGate); break;
                    case "Porta XNOR": selectedGateType = typeof(XnorGate); break;
                    case "Porta NOT": selectedGateType = typeof(NotGate); break;
                }
            }
        }
    }
}
