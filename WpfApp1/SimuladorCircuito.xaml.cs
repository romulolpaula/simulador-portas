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
                Type gateType = item.Content.ToString() switch
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
            modelToControl[model] = control;

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

        private void PositionNextGate(UserControl control)
        {
            double maxX = cnvSimulador.ActualWidth - control.Width - 10;
            double maxY = cnvSimulador.ActualHeight - control.Height - 10;

            if (nextX > maxX)
            {
                nextX = 20;
                nextY += offsetY;
            }
            if (nextY > maxY)
                nextY = 20;

            Canvas.SetLeft(control, nextX);
            Canvas.SetTop(control, nextY);

            nextX += offsetX;
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

            if (pendingSource == target) { pendingSource = null; return; }

            // Verifica ciclo antes de criar a conexão
            var sourceGate = pendingSource as GateModel;
            if(sourceGate != null && CreatesCycle(sourceGate, target))
            {
                MessageBox.Show("Conexão inválida! Essa ação criaria um ciclo no circuito.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                pendingSource = null;
                return;
            }

            // Cria linha visual
            var wire = new Wire(pendingSource as GateModel, target, args.InputIndex);
            wires.Add(wire);
            cnvSimulador.Children.Add(wire.LineShape);

            // Conecta modelo lógico
            target.Inputs.Add(pendingSource as GateModel ?? throw new InvalidOperationException("InputNode não conectado a porta"));

            UpdateWirePosition(wire);
            EvaluateAll();

            pendingSource = null;
        }

        private void UpdateWirePosition(Wire wire)
        {
            var sourceCtrl = GetControlForOutput(wire.Source);
            var targetCtrl = GetControlForInput(wire.Target);

            if (sourceCtrl == null || targetCtrl == null) return;

            var sourceEllipse = GetOutputEllipse(sourceCtrl);
            var targetEllipse = GetInputEllipse(targetCtrl, wire.TargetInputIndex);

            if (sourceEllipse == null || targetEllipse == null) return;

            var sourcePoint = sourceCtrl.TranslatePoint(
                new Point(sourceEllipse.Width / 2, sourceEllipse.Height / 2), cnvSimulador);
            var targetPoint = targetCtrl.TranslatePoint(
                new Point(targetEllipse.Width / 2, targetEllipse.Height / 2), cnvSimulador);

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

            foreach (var wire in wires)
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
    }
}
