using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private object pendingSource = null; 
        private int currentColumn = 0;
        private Dictionary<int, List<GateModel>> colunas = new();
        private PortInfo selectedOutput = null;
        private List<Wire> wires = new();


        public SimuladorCircuito()
        {
            InitializeComponent();
        }

        private void cmbPorta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPorta.SelectedItem is ComboBoxItem selectedItem)
            {
                string nomePorta = selectedItem.Content.ToString();

                Type tipoPorta = nomePorta switch
                {
                    "Porta AND" => typeof(AndGate),
                    "Porta NAND" => typeof(NandGate),
                    "Porta OR" => typeof(OrGate),
                    "Porta NOR" => typeof(NorGate),
                    "Porta XOR" => typeof(XorGate),
                    "Porta XNOR" => typeof(XnorGate),
                    _ => null
                };

                if (tipoPorta != null)
                    AddGateToCanvas(tipoPorta);
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

            if (!colunas.ContainsKey(currentColumn))
                colunas[currentColumn] = new List<GateModel>();
            colunas[currentColumn].Add(model);

            double startX = 100;
            double startY = 80;
            double offsetX = 220;
            double offsetY = 100;

            double posX = startX + currentColumn * offsetX;
            double posY = startY + (colunas[currentColumn].Count - 1) * offsetY;

            Canvas.SetLeft(control, posX);
            Canvas.SetTop(control, posY);

            cnvSimulador.Children.Add(control);
            modelToControl[model] = control;

            RefreshAllControls();
            EvaluateAll();
        }

        private void OnInputPortClicked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is PortInfo entrada)
            {
                if (pendingSource == null)
                {
                    if (entrada.ConnectedWire == null)
                    {
                        entrada.ToggleState();
                        EvaluateAll();
                    }
                    return;
                }

                if (pendingSource is PortInfo saida)
                {
                    if (saida.Gate == entrada.Gate)
                    {
                        pendingSource = null;
                        RefreshAllControls();
                        return;
                    }

                    var wire = new Wire(saida, entrada);
                    cnvSimulador.Children.Insert(0, wire.PathVisual);
                    wires.Add(wire);

                    entrada.ConnectedWire = wire;

                    if (!entrada.Gate.Inputs.Contains(saida.Gate))
                        entrada.Gate.Inputs.Add(saida.Gate);

                    entrada.SetState(saida.Gate.Output);

                    EvaluateAll();
                    var p1 = GetPortCenter(wire.Source);
                    var p2 = GetPortCenter(wire.Target);
                    UpdateWirePosition(wire);
                    

                    wire.UpdateColor();

                    pendingSource = null;
                    RefreshAllControls();
                }
            }
        }



        private void OnOutputPortClicked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is PortInfo saida)
            {
                pendingSource = saida;
                saida.VisualEllipse.Stroke = Brushes.LimeGreen;
            }
        }



        private void CreateWireBetween(PortInfo source, PortInfo target)
        {
            if (CreatesCycle(source.Gate, target.Gate))
            {
                MessageBox.Show("Conexão inválida: criaria um ciclo no circuito.");
                return;
            }

            target.Gate.Inputs.Add(source.Gate);
            wires.Add(new Wire(source, target));
            EvaluateAll();
        }

        private void UpdateWirePosition(Wire wire)
        {
            if (wire == null || wire.PathVisual == null) return;

            var p1 = GetPortCenter(wire.Source);
            var p2 = GetPortCenter(wire.Target);

            // midX base
            double midXBase = (p1.X + p2.X) / 2;

            // agrupa fios que teriam midX próximo (tolerância)
            var sameColumn = wires.Where(w =>
            {
                if (w.Source == null || w.Target == null) return false;
                var a = GetPortCenter(w.Source);
                var b = GetPortCenter(w.Target);
                double x = (a.X + b.X) / 2;
                return Math.Abs(x - midXBase) < 6; // ajusta tolerância se necessário
            }).ToList();

            int index = sameColumn.IndexOf(wire);
            int count = sameColumn.Count;

            double spacing = 12; // distância horizontal entre fios — ajuste aqui
                                 // centraliza os offsets: por exemplo para 3 fios -> -12, 0, +12
            double midX = midXBase + (index - (count - 1) / 2.0) * spacing;

            // chama a rotina do Wire que desenha usando midX
            wire.UpdateGeometry(p1, p2, midX);
        }




        private Point GetPortCenter(PortInfo port)
        {
            if (port?.VisualEllipse == null)
                return new Point(0, 0);

            var center = new Point(
                port.VisualEllipse.Width / 2,
                port.VisualEllipse.Height / 2
            );

            return port.VisualEllipse.TranslatePoint(center, cnvSimulador);
        }


        private void EvaluateAll()
        {
            foreach (var kv in modelToControl)
            {
                var model = kv.Key;
                var control = kv.Value;

                model.Inputs.Clear(); 

                foreach (var port in control.Inputs)
                {
                    if (port.ConnectedWire != null)
                    {
                        var origem = port.ConnectedWire.Source.Gate;
                        model.Inputs.Add(origem);
                    }
                }

                for (int i = 0; i < control.Inputs.Length; i++)
                {
                    var port = control.Inputs[i];
                    if (port.ConnectedWire == null)
                    {
                        var dummy = new InputGateAdapter(port.Value);
                        if (model.Inputs.Count > i)
                            model.Inputs[i] = dummy;
                        else
                            model.Inputs.Add(dummy);
                    }
                }
            }

            bool mudou;
            int safety = 0;
            do
            {
                mudou = false;
                var ordered = gates.OrderBy(g => GetDepth(g)).ToList();
                foreach (var g in ordered)
                {
                    bool oldValue = g.Output;
                    try { g.Evaluate(); } catch { }

                    if (g.Output != oldValue)
                        mudou = true;
                }
                safety++;
            } while (mudou && safety < 10); // evita loop infinito

            RefreshAllControls();
        }




        private Point GetAbsolutePosition(PortInfo port)
        {
            if (port?.VisualEllipse == null)
                return new Point(0, 0);

            Point relative = port.VisualEllipse.TranslatePoint(
                new Point(port.VisualEllipse.Width / 2, port.VisualEllipse.Height / 2),
                cnvSimulador
            );

            return relative;
        }

        private void RefreshAllControls()
        {
            foreach (var wire in wires)
            {
                var p1 = GetPortCenter(wire.Source);
                var p2 = GetPortCenter(wire.Target);
                UpdateWirePosition(wire);
                


                wire.UpdateColor();

                if (wire.Source != null && wire.Target != null)
                {
                    bool val;
                    try
                    {
                        val = wire.Source.IsOutput ? wire.Source.Gate.Output : wire.Source.Value;
                    }
                    catch
                    {
                        val = wire.Source?.Value ?? false;
                    }

                    wire.Target.SetState(val);
                }
            }

            foreach (var kv in modelToControl)
            {
                var control = kv.Value;
                control.UpdatePortVisuals();
            }
        }


        private bool CreatesCycle(GateModel from, GateModel to)
        {
            HashSet<GateModel> visited = new();
            return HasPath(to, from, visited);
        }

        private bool HasPath(GateModel from, GateModel target, HashSet<GateModel> visited)
        {
            if (from == target) return true;
            visited.Add(from);
            foreach (var input in from.Inputs)
            {
                if (!visited.Contains(input) && HasPath(input, target, visited))
                    return true;
            }
            return false;
        }

        private string GetImageNameForGate(Type gateType)
        {
            string nome = gateType.Name.Replace("Gate", "").ToLower();
            return $"{nome}.svg";
        }

        private GateModel GetModelFromControl(GateControl control)
        {
            return modelToControl.FirstOrDefault(x => x.Value == control).Key;
        }

        private void btnOrganizar_Click(object sender, RoutedEventArgs e)
        {
            double startX = 100;
            double startY = 80;
            double offsetX = 200;
            double offsetY = 100;

            var levels = new Dictionary<GateModel, int>();
            foreach (var g in gates)
                levels[g] = GetDepth(g);

            var grouped = gates.GroupBy(g => levels[g])
                               .OrderBy(g => g.Key);

            int col = 0;
            foreach (var group in grouped)
            {
                int row = 0;
                foreach (var g in group)
                {
                    if (modelToControl.TryGetValue(g, out var control))
                    {
                        double posX = startX + col * offsetX;
                        double posY = startY + row * offsetY;
                        Canvas.SetLeft(control, posX);
                        Canvas.SetTop(control, posY);
                        row++;
                    }
                }
                col++;
            }

            RefreshAllControls();
        }

        private int GetDepth(GateModel gate)
        {
            if (gate.Inputs.Count == 0)
                return 0;

            int depth = 0;
            foreach (var input in gate.Inputs)
                depth = Math.Max(depth, GetDepth(input) + 1);
            return depth;
        }

        private void btnProximaColuna_Click(object sender, RoutedEventArgs e)
        {
            currentColumn++;
        }

        private void btnColunaAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (currentColumn > 0)
                currentColumn--;
        }
    }
}
