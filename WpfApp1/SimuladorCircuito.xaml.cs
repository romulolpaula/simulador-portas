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
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace WpfApp1
{
    /// <summary>
    /// Lógica interna para SimuladorCircuito.xaml
    /// </summary>
    public partial class SimuladorCircuito : Window
    {
        private CircuitManager circuit = new CircuitManager(); //guarda o modelo do circuito
        private Type selectedGateType = null; //guarda o tipo de porta escolhida no ComboBox 
        private Dictionary<GateModel, GateControl> modelToControl = new Dictionary<GateModel, GateControl>(); //liga cada modelo lógico (GateModel) ao seu controle visual (GateControl)
        
        public SimuladorCircuito()
        {
            InitializeComponent();
        }

        private void cnvSimulador_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) //adiciona porta ao clicar no canvas
        {
            if (selectedGateType == null) return; //verifica se alguma porta foi escolhida

            var pos = e.GetPosition(cnvSimulador);
            GateModel model = (GateModel)Activator.CreateInstance(selectedGateType); //cria o modelo lógico da porta
            circuit.Gates.Add(model);

            var control = new GateControl(); //cria o controle visual e carrega a imagem correspondente
            string imageName = GetImageNameForGate(selectedGateType);
            control.Initialize(model, imageName);

            //registra eventos de cliques nas portas
            control.AddHandler(GateControl.OutputPortClickedEvent, new RoutedEventHandler(OnOutputPortClicked));
            control.AddHandler(GateControl.InputPortClickedEvent, new RoutedEventHandler(OnInputPortClicked));

            Canvas.SetLeft(control, pos.X - control.Width / 2); //posiciona a porta no canvas e adiciona ao dicionário
            Canvas.SetTop(control, pos.Y - control.Height / 2);
            cnvSimulador.Children.Add(control);

            modelToControl[model] = control;
            selectedGateType = null; //reseta depois de colocar pra não colocar várias portas sem querer
        }

        //associa o tipo da porta à imagem correspondente 
        private string GetImageNameForGate(Type gateType) //função que mapeia o tipo da porta para o arquivo da imagem 
        {
            if (gateType == typeof(AndGate)) return "and.svg";
            if (gateType == typeof(OrGate)) return "or.svg";
            if (gateType == typeof(NotGate)) return "not.svg";
            if (gateType == typeof(XorGate)) return "xor.svg";
            if (gateType == typeof(NandGate)) return "nand.svg";
            if (gateType == typeof(NorGate)) return "nor.svg";
            if (gateType == typeof(XnorGate)) return "xnor.svg";
            return "default.png";
        }

        private void cmbPorta_SelectionChanged(object sender, SelectionChangedEventArgs e) //detecta qual porta foi selecionada e guarda o tipo
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

        private GateModel pendingSource = null; //guarda a porta de onde vai sair o fio 
        private List<Line> uiWires = new List<Line>(); //lista de fios desenhados no canvas

        private void OnOutputPortClicked(object sender, RoutedEventArgs e) //marca a porta clicada como fonte do fio
        {
            var control = (GateControl)sender;
            pendingSource = control.Model;
        }

        private void OnInputPortClicked(object sender, RoutedEventArgs e)
        {
            if (pendingSource == null) return; //verifica se já tem uma fonte

            var args = (GateControl.InputPortClickedEventArgs)e;
            var control = (GateControl)sender;
            var target = control.Model;

            if (pendingSource == target) { pendingSource = null; return; } //evita loop

            target.Inputs.Add(pendingSource); //conecta a saída na entrada clicada 

            var line = new Line //desenha fio visual
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
            };
            UpdateLinePositions(line, pendingSource, target, args.InputIndex);

            Canvas.SetZIndex(line, 0);
            cnvSimulador.Children.Add(line);
            uiWires.Add(line);

            circuit.EvaluateAll(); //recalcula todo o circuito
            RefreshAllControls(); //atualiza as cores das portas 

            pendingSource = null; //reseta
        }

        //calcula coordenadas de saída e entrada 
        private void UpdateLinePositions(Line line, GateModel source, GateModel target, int inputIndex)
        {
            var sCtrl = modelToControl[source];
            var tCtrl = modelToControl[target];

            //ponto de saída
            var sPoint = sCtrl.TranslatePoint(
                new Point(sCtrl.Output.Width / 2 + Canvas.GetLeft(sCtrl.Output),
                           sCtrl.Output.Height / 2 + Canvas.GetTop(sCtrl.Output)), 
                cnvSimulador
            );

            //ponto de entrada
            Ellipse inputEllipse = inputIndex == 0 ? tCtrl.Input0 : tCtrl.Input1;
            var tPoint = tCtrl.TranslatePoint(
                new Point(inputEllipse.Width / 2 + Canvas.GetLeft(inputEllipse),
                          inputEllipse.Height / 2 + Canvas.GetTop(inputEllipse)),
                cnvSimulador
               );

            line.X1 = sPoint.X;
            line.Y1 = sPoint.Y;
            line.X2 = tPoint.X;
            line.Y2 = tPoint.Y;
        }

        private void RefreshAllControls() //atualiza as portas 
        {
            foreach (var kv in modelToControl) kv.Value.UpdatePortVisuals();
        }

        public void EvaluateAndRefresh()
        {
            circuit.EvaluateAll(); //recalcula todo o circuito
            RefreshAllControls(); //atualiza visual (cores das portas e linhas)
        }
    }
}
