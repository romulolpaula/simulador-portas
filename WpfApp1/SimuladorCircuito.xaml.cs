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
        private List<Line> uiWires = new List<Line>();

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

            // Conecta modelo lógico
            target.Inputs.Add(pendingSource as GateModel ?? throw new InvalidOperationException("InputNode não conectado a porta"));

            // Cria linha visual
            Line line = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Tag = Tuple.Create(pendingSource, target, args.InputIndex)
            };
            cnvSimulador.Children.Add(line);
            uiWires.Add(line);

            UpdateLinePositions(line, pendingSource, target, args.InputIndex);

            EvaluateAll();

            pendingSource = null;
        }

        private void UpdateLinePositions(Line line, object source, object target, int inputIndex)
        {
            UserControl sCtrl = GetControlForOutput(source);
            UserControl tCtrl = GetControlForInput(target);

            if (sCtrl == null || tCtrl == null) return;

            var sPoint = sCtrl.TranslatePoint(new Point(GetOutputEllipse(sCtrl).Width / 2, GetOutputEllipse(sCtrl).Height / 2), cnvSimulador);
            var tEllipse = GetInputEllipse(tCtrl, inputIndex);
            var tPoint = tCtrl.TranslatePoint(new Point(tEllipse.Width / 2, tEllipse.Height / 2), cnvSimulador);

            line.X1 = sPoint.X;
            line.Y1 = sPoint.Y;
            line.X2 = tPoint.X;
            line.Y2 = tPoint.Y;
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

            foreach (var line in uiWires)
            {
                if (line.Tag is Tuple<object, object, int> tag)
                    UpdateLinePositions(line, tag.Item1, tag.Item2, tag.Item3);
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
    }
}
