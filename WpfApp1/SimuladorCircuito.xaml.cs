using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfApp1.Controls;
using WpfApp1.Models;

namespace WpfApp1
{
    public partial class SimuladorCircuito : Window
    {
        private List<GateModel> gates = new List<GateModel>();
        private Dictionary<GateModel, GateControl> modelToControl = new Dictionary<GateModel, GateControl>();
        private Dictionary<InputNode, InputNodeControl> inputToControl = new Dictionary<InputNode, InputNodeControl>();
        private object pendingSource = null; // Pode ser GateModel ou InputNode
        private List<Wire> wires = new List<Wire>();

        private double nextX = 20;
        private double nextY = 20;
        private const double offsetX = 150;
        private const double offsetY = 150;

        public SimuladorCircuito()
        {
            InitializeComponent();
            cnvSimulador.SizeChanged += (s, e) => LayoutAllGates();
        }

        // Adiciona porta automaticamente quando selecionada no ComboBox
        private void cmbPorta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPorta.SelectedItem is ComboBoxItem item)
            {
                string selected = item.Content.ToString();

                if (selected == "Entrada")
                {
                    var inputNode = new InputNode();
                    AddInputNode(inputNode);
                }
                else if (selected == "Saída")
                {
                    var outputNode = new OutputNode();
                    AddOutputNode(outputNode);
                }
                else
                {
                    Type gateType = selected switch
                    {
                        "Porta AND" => typeof(AndGate),
                        "Porta NAND" => typeof(NandGate),
                        "Porta OR" => typeof(OrGate),
                        "Porta NOR" => typeof(NorGate),
                        "Porta XOR" => typeof(XorGate),
                        "Porta XNOR" => typeof(XnorGate),
                        "Porta NOT" => typeof(NotGate),
                        _ => null
                    };

                    if (gateType != null)
                        AddGateToCanvas(gateType);
                }

                cmbPorta.SelectedIndex = -1;
            }
        }


        private void AddGateToCanvas(Type gateType)
        {
            GateModel model = (GateModel)Activator.CreateInstance(gateType);
            gates.Add(model);

            var control = new GateControl();
            string imageName = GetImageNameForGate(gateType);
            control.Initialize(model, imageName);

            control.AddHandler(GateControl.OutputPortClickedEvent, new RoutedEventHandler(OnOutputPortClicked));
            control.AddHandler(GateControl.InputPortClickedEvent, new RoutedEventHandler(OnInputPortClicked));

            PositionNextGate(control);

            cnvSimulador.Children.Add(control);
            control.LayoutUpdated += (s, ev) => {
                // small optimization: você pode checar se realmente mudou posição antes de refrescar tudo
                RefreshAllControls();
            };

            modelToControl[model] = control;

            if (gates.Count > 1)
            {
                var source = gates[gates.Count - 2];
                var target = gates[gates.Count - 1];

                var wire = new Wire(source, target, 0);//conecta a saída do gate anterior à entrada do novo gate
                wires.Add(wire);
                cnvSimulador.Children.Add(wire.LineShape);

                UpdateWirePosition(wire);
                EvaluateAll();

            }
            RefreshAllControls();
        }

        // Adiciona InputNode manualmente
        private void AddInputNode(InputNode inputNode)
        {
            var control = new InputNodeControl();
            control.Initialize(inputNode);

            PositionNextGate(control);

            cnvSimulador.Children.Add(control);
            inputToControl[inputNode] = control;
        }

        private void AddOutputNode(OutputNode outputNode)
        {
            var control = new OutputNodeControl();
            control.Initialize(outputNode);

            PositionNextGate(control);

            cnvSimulador.Children.Add(control);
        }

        private double currentX = 20;
        private double currentY = 20;
        private const double espacoHorizontal = 140;
        private const double espacoVertical = 100;
        private const double larguraMaxima = 650;
        private void PositionNextGate(UserControl control)
        {
            double baseY = cnvSimulador.Children.Count * 80 + 50;

            if (control is InputNodeControl)
            {
                Canvas.SetLeft(control, 50);
                Canvas.SetTop(control, baseY);
            }
            else if (control is OutputNodeControl)
            {
                Canvas.SetLeft(control, cnvSimulador.ActualWidth - control.Width - 80);
                Canvas.SetTop(control, baseY);
            }
            else // Portas lógicas
            {
                Canvas.SetLeft(control, cnvSimulador.ActualWidth / 2 - control.Width / 2);
                Canvas.SetTop(control, baseY);
            }
        }

        private void OnOutputPortClicked(object sender, RoutedEventArgs e)
        {
            var control = sender as UserControl;
            if (control is GateControl gc) pendingSource = gc.Model;
            else if (control is InputNodeControl ic) pendingSource = ic.Model;
        }

        private void OnInputPortClicked(object sender, RoutedEventArgs e)
        {
            if (pendingSource == null) return;

            var args = e as GateControl.InputPortClickedEventArgs;
            var control = sender as GateControl;
            var target = control.Model;

            // Evita conectar uma porta a ela mesma
            if (pendingSource == target)
            {
                pendingSource = null;
                return;
            }

            // Verifica ciclo antes de criar a conexão
            var sourceGate = pendingSource as GateModel;
            if (sourceGate != null && CreatesCycle(sourceGate, target))
            {
                MessageBox.Show("Conexão inválida! Essa ação criaria um ciclo no circuito.",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                pendingSource = null;
                return;
            }

            //Cria o fio (Wire) e adiciona ao Canvas antes de posicionar
            var wire = new Wire(sourceGate, target, args.InputIndex);
            wires.Add(wire);
            cnvSimulador.Children.Add(wire.LineShape);

            //Conecta o modelo lógico
            if (pendingSource is GateModel gm)
                target.Inputs.Add(gm);
            else
                throw new InvalidOperationException("InputNode não conectado a porta");

            //Atualiza posição do fio após o layout do WPF estar pronto
            // (evita o bug da linha aparecendo no topo)
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateWirePosition(wire);
                EvaluateAll(); // Atualiza saídas e cores das linhas
            }), System.Windows.Threading.DispatcherPriority.Loaded);

            //Limpa seleção de origem
            pendingSource = null;
        }

        private void UpdateWirePosition(Wire wire)
        {
            var sourceCtrl = GetControlForOutput(wire.Source);
            var targetCtrl = GetControlForInput(wire.Target);

            if (sourceCtrl == null || targetCtrl == null) return;

            var sourceEllipse = GetOutputEllipse(sourceCtrl);
            var targetEllipse = GetInputEllipse(targetCtrl, wire.TargetInputIndex);

            Point sourcePoint;
            if (sourceEllipse != null)
            {
                // Traduz o ponto a partir da própria bolinha (correto)
                sourcePoint = sourceEllipse.TranslatePoint(
                    new Point(sourceEllipse.Width / 2, sourceEllipse.Height / 2),
                    cnvSimulador);
            }
            else
            {
                sourcePoint = sourceCtrl.TranslatePoint(
                    new Point(sourceCtrl.ActualWidth / 2, sourceCtrl.ActualHeight / 2),
                    cnvSimulador);
            }

            Point targetPoint;
            if (targetEllipse != null)
            {
                targetPoint = targetEllipse.TranslatePoint(
                    new Point(targetEllipse.Width / 2, targetEllipse.Height / 2),
                    cnvSimulador);
            }
            else
            {
                targetPoint = targetCtrl.TranslatePoint(
                    new Point(targetCtrl.ActualWidth / 2, targetCtrl.ActualHeight / 2),
                    cnvSimulador);
            }

            wire.UpdatePosition(sourcePoint.X, sourcePoint.Y, targetPoint.X, targetPoint.Y);

        }

        private UserControl GetControlForOutput(object obj) =>
            obj switch
            {
                GateModel gm when modelToControl.ContainsKey(gm) => modelToControl[gm],
                InputNode inp when inputToControl.ContainsKey(inp) => inputToControl[inp],
                _ => null
            };

        private UserControl GetControlForInput(object obj) =>
            obj switch
            {
                GateModel gm when modelToControl.ContainsKey(gm) => modelToControl[gm],
                _ => null
            };

        private Ellipse GetOutputEllipse(UserControl ctrl) =>
            ctrl is GateControl gc ? gc.Output : (ctrl as InputNodeControl)?.Output;

        private Ellipse GetInputEllipse(UserControl ctrl, int index) =>
            ctrl is GateControl gc ? (index == 0 ? gc.Input0 : gc.Input1) : null;

        public void EvaluateAll()
        {
            foreach (var gate in gates)
                gate.Evaluate();

            RefreshAllControls();
        }

        private void RefreshAllControls()
        {
            foreach (var kv in modelToControl)
                kv.Value.UpdatePortVisuals();

            foreach (var wire in wires) //reposiciona e recolore todos os fios
            {
                UpdateWirePosition(wire);
                wire.UpdateColor(wire.Source?.Output ?? false);
            }
        }

        private void LayoutAllGates()
        {
            nextX = 20;
            nextY = 20;

            foreach (var kv in modelToControl)
                PositionNextGate(kv.Value);

            foreach (var kv in inputToControl)
                PositionNextGate(kv.Value);

            RefreshAllControls();
        }

        private string GetImageNameForGate(Type gateType)
        {
            return gateType switch
            {
                Type t when t == typeof(AndGate) => "and.svg",
                Type t when t == typeof(OrGate) => "or.svg",
                Type t when t == typeof(NotGate) => "not.svg",
                Type t when t == typeof(XorGate) => "xor.svg",
                Type t when t == typeof(NandGate) => "nand.svg",
                Type t when t == typeof(NorGate) => "nor.svg",
                Type t when t == typeof(XnorGate) => "xnor.svg",
                _ => "default.png"
            };
        }

        private bool CreatesCycle(GateModel source, GateModel target)
        {
            if (source == null || target == null) return false; //evita nulos

            if (source == target) return true; //se a saída for ligada de volta a propria entrada, autoloop

            HashSet<GateModel> visited = new HashSet<GateModel>(); //faz uma busca recursiva nas entradas do target
            return HasPath(target, source, visited);
        }

        private bool HasPath(GateModel current, GateModel target, HashSet<GateModel> visited)
        {
            if (current == null || visited.Contains(current)) return false;

            visited.Add(current);

            foreach (var input in current.Inputs)
            {
                if (input == target) return true;
                if (HasPath(input, target, visited)) return true;
            }
            return false;
        }

        public void IniciarLigacao(GateModel node, string tipo)
        {
            if (tipo == "output")
            {
                pendingSource = node;
            }
            else if (tipo == "input" && pendingSource != null)
            {
                var wire = new Wire(pendingSource as GateModel, node, 0);
                wires.Add(wire);
                cnvSimulador.Children.Add(wire.LineShape);
                UpdateWirePosition(wire);
                EvaluateAll();

                pendingSource = null;
            }
        }
    }
}
