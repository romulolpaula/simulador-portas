using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private Dictionary<OutputNode, OutputNodeControl> outputToControl = new Dictionary<OutputNode, OutputNodeControl>();
        private object pendingSource = null; // Pode ser GateModel ou InputNode
        private List<Wire> wires = new List<Wire>();
        private List<List<GateModel>> colunas = new List<List<GateModel>>();
        private int colunaSelecionada = 0;

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

                if (selected == "Entrada" )
                {   if (colunaSelecionada != 0)
                    {
                        MessageBox.Show("Entradas só podem ser adicionadas na Coluna 0!");
                        return;
                    }
                    var inputNode = new InputNode();
                    AddInputNode(inputNode);
                }
                else if (selected == "Saída")
                {   
                    if (colunaSelecionada != colunas.Count -1)
                    {
                        MessageBox.Show("Saídas só podem ser adicionadas na última coluna!");
                        return;
                    }
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

            // --- POSICIONAMENTO PELA COLUNA ---
            double posX = 150 + (colunaSelecionada * 180);
            double posY = 80 + (colunas[colunaSelecionada].Count * 120);
            Canvas.SetLeft(control, posX);
            Canvas.SetTop(control, posY);

            cnvSimulador.Children.Add(control);
            modelToControl[model] = control;
            colunas[colunaSelecionada].Add(model);

            // --- CONECTA COM COLUNA ANTERIOR ---
            if (colunaSelecionada > 0 && colunas[colunaSelecionada - 1].Count > 0)
            {
                var anterior = colunas[colunaSelecionada - 1];
                int qtdEntradas = GetRequiredInputCount(model);

                // Conecta a porta atual com as últimas N saídas da coluna anterior
                for (int i = 0; i < qtdEntradas && i < anterior.Count; i++)
                {
                    var src = anterior[i];
                    var wire = new Wire(src, model, i);
                    wires.Add(wire);
                    cnvSimulador.Children.Add(wire.LineShape);
                    UpdateWirePosition(wire);

                    model.Inputs.Add(src);
                }
            }

            RefreshAllControls();
            EvaluateAll();
        }


        // Adiciona InputNode manualmente
        // Adiciona InputNode manualmente
        private void AddInputNode(InputNode inputNode)
        {
            var control = new InputNodeControl();
            control.Initialize(inputNode);

            // registra o evento de click na saída para iniciar ligação
            control.AddHandler(InputNodeControl.OutputPortClickedEvent, new RoutedEventHandler(OnOutputPortClicked));

            PositionNextGate(control);

            cnvSimulador.Children.Add(control);
            inputToControl[inputNode] = control;
        }

        // Adiciona OutputNode manualmente
        private void AddOutputNode(OutputNode outputNode)
        {
            var control = new OutputNodeControl();
            control.Initialize(outputNode);

            // registra handler caso queira permitir ligar entradas diretamente a saída por clique
            control.AddHandler(OutputNodeControl.InputPortClickedEvent, new RoutedEventHandler(OnInputPortClicked));

            PositionNextGate(control);

            cnvSimulador.Children.Add(control);
            outputToControl[outputNode] = control;
        }


        private double currentX = 20;
        private double currentY = 20;
        private const double espacoHorizontal = 140;
        private const double espacoVertical = 100;
        private const double larguraMaxima = 650;
        private void PositionNextGate(UserControl control)
        {
            double baseY = currentY;

            if (control is InputNodeControl)
            {
                // Sempre fixa à esquerda
                Canvas.SetLeft(control, 50);
                Canvas.SetTop(control, baseY);
            }
            else if (control is OutputNodeControl)
            {
                // Sempre fixa à direita
                Canvas.SetLeft(control, cnvSimulador.ActualWidth - control.Width - 80);
                Canvas.SetTop(control, baseY);
            }
            else
            {
                // Organiza portas no meio, em linhas horizontais
                if (currentX + control.Width > larguraMaxima)
                {
                    currentX = 200; // volta mais pra esquerda
                    currentY += espacoVertical; // desce pra próxima linha
                    baseY = currentY;
                }

                Canvas.SetLeft(control, currentX);
                Canvas.SetTop(control, baseY);
                currentX += espacoHorizontal;
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
            {
                target.Inputs.Add(gm);
            }
            else if (pendingSource is InputNode inp)
            {
                target.Inputs.Add(new InputGateAdapter(inp)); // cria adaptador lógico
            }
            else
            {
                MessageBox.Show("Conexão inválida!", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


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
            // Conjunto de nós já visitados (evita recursões infinitas)
            var visited = new HashSet<Guid>();

            // Avalia todas as entradas (InputNode)
            foreach (var input in inputToControl.Keys)
            {
                try
                {
                    input.Evaluate(visited);
                }
                catch
                {
                    // Caso o método Evaluate não aceite parâmetros, usa a sobrecarga sem HashSet
                    try { input.Evaluate(new HashSet<Guid>()); } catch { }
                }
            }

            // Avalia todas as portas lógicas (AND, OR, NOT, etc.)
            foreach (var gate in gates)
            {
                try
                {
                    gate.Evaluate(visited);
                }
                catch
                {
                    try { gate.Evaluate(); } catch { }
                }
            }

            // Avalia todas as saídas (OutputNode)
            foreach (var output in outputToControl.Keys)
            {
                try
                {
                    output.Evaluate(visited);
                }
                catch
                {
                    try { output.Evaluate(new HashSet<Guid>()); } catch { }
                }
            }

            // Atualiza os controles visuais (cores, estados, etc.)
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
            currentX = 20;
            currentY = 20;

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

        private int GetRequiredInputCount(GateModel gate) //retorna quantas entradas a porta precisa 
        {
            return gate switch
            {
                NotGate _ => 1,
                _ => 2
            };
        }

        private void ClearAutoWires() //limpa apenas fios gerados automaticamente
        {
            for (int i = wires.Count - 1; i >= 0; i--)
            {
                var w = wires[i]; //supondo que wire tenha propriedade bool Auto 
                var autoProp = w.GetType().GetProperty("Auto");
                bool isAuto = false;
                if (autoProp != null) isAuto = (bool)autoProp.GetValue(w);

                if (isAuto)
                {
                    if (w.LineShape != null && cnvSimulador.Children.Contains(w.LineShape))
                        cnvSimulador.Children.Remove(w.LineShape);
                    wires.RemoveAt(i);
                }
            }
        }

        private GateModel GetModelFromControl(FrameworkElement control)
        {
            if (control is GateControl gate)
                return gate.Model;
            if (control is InputNodeControl input)
                return input.Model;
            if (control is OutputNodeControl output)
                return output.Model;

            return null!;
        }

        // cria um fio e marca como automático
        private Wire CreateWireBetween(GateModel source, GateModel target, int targetIndex = 0)
        {
            // evita conectar uma porta a ela mesma ou criar ciclos
            if (target == source || target.DependsOn(source))
            {
                MessageBox.Show("Conexão inválida (loop detectado)");
                return null;
            }

            var wire = new Wire(source, target, targetIndex)
            {
                Auto = true
            };

            // Conecta o modelo lógico (isso garante que o sinal passe de uma porta pra outra)
            if (source != null && target != null && !target.Inputs.Contains(source))
                target.Inputs.Add(source);

            wires.Add(wire);
            cnvSimulador.Children.Add(wire.LineShape);

            UpdateWirePosition(wire);
            EvaluateAll(); // Recalcula as saídas do circuito

            return wire;
        }

        //organiza em camadas e conecta automaticamente
        private void AutoArrangeAndConnect()
        {
            double startX = 80;
            double startY = 50;
            double offsetX = 150;
            double offsetY = 100;

            var entradas = inputToControl.Values.ToList();
            var saidas = outputToControl.Values.ToList();
            var allGates = modelToControl.Keys
                .Where(m => !(m is InputNode) && !(m is OutputNode))
                .ToList();

            // Limpa conexões antigas
            foreach (var gate in allGates)
                gate.Inputs.Clear();

            wires.Clear();
            cnvSimulador.Children.OfType<Line>().ToList().ForEach(l => cnvSimulador.Children.Remove(l));

            // === Cria as camadas ===
            var depthLayers = new List<List<GateModel>>();
            depthLayers.Add(inputToControl.Keys.Cast<GateModel>().ToList()); // camada 0: entradas
            var allGatesToProcess = new Queue<GateModel>(allGates);
            var previousLayer = depthLayers[0];

            while (previousLayer.Count > 0 && allGatesToProcess.Count > 0)
            {
                int nextLayerSize = Math.Min(previousLayer.Count, allGatesToProcess.Count);
                var nextLayer = new List<GateModel>();

                for (int i = 0; i < nextLayerSize; i++)
                    nextLayer.Add(allGatesToProcess.Dequeue());

                if (nextLayer.Count > 0)
                {
                    depthLayers.Add(nextLayer);
                    previousLayer = nextLayer;
                }
                else
                {
                    break;
                }
            }

            // Adiciona camada final (saídas)
            if (outputToControl.Count > 0)
                depthLayers.Add(outputToControl.Keys.Cast<GateModel>().ToList());

            // === Posiciona graficamente ===
            double currentLayerX = startX;
            for (int d = 0; d < depthLayers.Count; d++)
            {
                var layer = depthLayers[d];
                if (layer == null || layer.Count == 0)
                    continue;

                double totalLayerHeight = layer.Count * offsetY;
                double canvasCenterY = cnvSimulador.ActualHeight > 0 ? cnvSimulador.ActualHeight / 2 : 200;
                double startYAdjusted = canvasCenterY - (totalLayerHeight / 2);
                double currentLayerY = Math.Max(startY, startYAdjusted);

                foreach (var model in layer)
                {
                    UserControl ctrl = null;

                    if (model is OutputNode outNode && outputToControl.TryGetValue(outNode, out var outCtrl))
                        ctrl = outCtrl;
                    else if (model is InputNode inpNode && inputToControl.TryGetValue(inpNode, out var inpCtrl))
                        ctrl = inpCtrl;
                    else if (modelToControl.TryGetValue(model, out var gateCtrl))
                        ctrl = gateCtrl;

                    if (ctrl == null) continue;

                    Canvas.SetLeft(ctrl, currentLayerX);
                    Canvas.SetTop(ctrl, currentLayerY);
                    currentLayerY += offsetY;
                }

                currentLayerX += offsetX;
            }

            // === Conectar automaticamente ===
            for (int d = 0; d < depthLayers.Count - 1; d++)
            {
                var current = depthLayers[d];
                var next = depthLayers[d + 1];

                if (current == null || next == null || current.Count == 0 || next.Count == 0)
                    continue;

                for (int j = 0; j < next.Count; j++)
                {
                    var dst = next[j];
                    if (dst == null) continue;

                    // Caso seja uma saída, conecta a última da camada anterior
                    if (dst is OutputNode outNode)
                    {
                        var src = current.LastOrDefault();
                        if (src != null)
                        {
                            outNode.Source = src;
                            CreateWireBetween(src, dst, 0);
                        }
                    }
                    else
                    {
                        // Primeira camada de portas: conecta todas as entradas
                        if (current.All(m => m is InputNode))
                        {
                            int inputIndex = 0;
                            foreach (var src in current)
                            {
                                if (src != null)
                                    CreateWireBetween(src, dst, inputIndex++);
                            }
                        }
                        else
                        {
                            // Camadas intermediárias: conecta 2 anteriores
                            var src1 = current[Math.Min(j, current.Count - 1)];
                            var src2 = current[Math.Min(j + 1, current.Count - 1)];

                            if (src1 != null)
                                CreateWireBetween(src1, dst, 0);
                            if (src2 != null)
                                CreateWireBetween(src2, dst, 1);
                        }
                    }
                }
            }

            EvaluateAll();
            RefreshAllControls();

            MessageBox.Show("Circuito organizado e conectado automaticamente!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void btnOrganizar_Click(object sender, RoutedEventArgs e)
        {
            // Liga todas as entradas (coluna 0) a todas as portas da primeira coluna (coluna 1) 
            if (colunas.Count > 1)
            {
                var colEntradas = colunas[0];
                var colPrimeirasPortas = colunas[1];

                foreach (var entrada in colEntradas)
                {
                    foreach (var porta in colPrimeirasPortas)
                    {
                        // Evita duplicatas
                        if (!porta.Inputs.Contains(entrada))
                        {
                            porta.Inputs.Add(entrada);
                        }
                    }
                }
            }

        }

        private void cmbColuna_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            colunaSelecionada = cmbColuna.SelectedIndex;

            // Cria colunas automaticamente até o índice selecionado
            while (colunas.Count <= colunaSelecionada)
                colunas.Add(new List<GateModel>());

            MessageBox.Show($"Coluna selecionada: {colunaSelecionada}");
        }
    }
}
