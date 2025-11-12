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
        private object pendingSource = null; // Usado ao conectar manualmente fios entre bolinhas
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

            // adiciona na coluna atual
            if (!colunas.ContainsKey(currentColumn))
                colunas[currentColumn] = new List<GateModel>();
            colunas[currentColumn].Add(model);

            // posiciona automaticamente
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
                    entrada.Gate.Inputs.Add(saida.Gate);

                    wire.UpdatePosition();
                    wire.UpdateColor();

                    EvaluateAll();

                    pendingSource = null;
                    RefreshAllControls();
                }
            }
        }

        private void OnOutputPortClicked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is PortInfo info)
            {
                if (pendingSource == null)
                {
                    pendingSource = info;
                    info.VisualEllipse.Stroke = Brushes.LimeGreen; 
                }
                else
                {
                    pendingSource = null;
                    RefreshAllControls();
                }
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
            if (wire.PathVisual == null) return;

            wire.UpdatePosition();
        }

        private void EvaluateAll()
        {
            // Primeiro: sincroniza os valores das bolinhas manuais com o modelo
            foreach (var kv in modelToControl)
            {
                var model = kv.Key;
                var control = kv.Value;

                // Para cada entrada manual (sem fio), aplica o valor da PortInfo diretamente no modelo
                for (int i = 0; i < control.Inputs.Length; i++)
                {
                    var port = control.Inputs[i];
                    if (port.ConnectedWire == null)
                    {
                        // cria uma "porta de entrada" virtual para propagar o valor manual
                        var dummy = new InputGateAdapter(port.Value);
                        if (model.Inputs.Count > i)
                            model.Inputs[i] = dummy;
                        else
                            model.Inputs.Add(dummy);
                    }
                }
            }

            // Agora: avalia as portas em ordem de profundidade (entrada → saída)
            var ordered = gates.OrderBy(g => GetDepth(g)).ToList();
            foreach (var g in ordered)
            {
                try { g.Evaluate(); } catch { }
            }

            RefreshAllControls();
        }

        private Point GetAbsolutePosition(PortInfo port)
        {
            if (port?.VisualEllipse == null)
                return new Point(0, 0);

            // Transforma a posição da bolinha (ellipse) em coordenadas absolutas do Canvas
            var transform = port.VisualEllipse.TransformToAncestor(cnvSimulador);
            Point relativePoint = transform.Transform(new Point(port.VisualEllipse.Width / 2, port.VisualEllipse.Height / 2));
            return relativePoint;
        }

        private void RefreshAllControls()
        {
            // atualiza visuais de cada GateControl
            foreach (var kv in modelToControl)
            {
                var control = kv.Value;
                control.UpdatePortVisuals();
            }

            // reposiciona e recolore todos os fios (se você implementou Wire.Line)
            foreach (var wire in wires)
            {
                UpdateWirePosition(wire);
                wire.UpdateColor();
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
            // Organiza as portas automaticamente em colunas com base em suas conexões
            double startX = 100;
            double startY = 80;
            double offsetX = 200;
            double offsetY = 100;

            // Agrupar por "nível" (profundidade de dependência)
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
