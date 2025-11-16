using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfApp1.Banco_de_Dados;
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
        private CircuitoDAO circuitoDAO = new CircuitoDAO();



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

            double midXBase = (p1.X + p2.X) / 2;

            var sameColumn = wires.Where(w =>
            {
                if (w.Source == null || w.Target == null) return false;
                var a = GetPortCenter(w.Source);
                var b = GetPortCenter(w.Target);
                double x = (a.X + b.X) / 2;
                return Math.Abs(x - midXBase) < 6; 
            }).ToList();

            int index = sameColumn.IndexOf(wire);
            int count = sameColumn.Count;

            double spacing = 12; 
                                 
            double midX = midXBase + (index - (count - 1) / 2.0) * spacing;

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

        private void btnTabelaVerdade_Click(object sender, RoutedEventArgs e)
        {
            var entradas = obterEntradasSoltas();
            var saidas = obterSaidas();

            if (entradas.Count == 0 || saidas.Count == 0)
            {
                MessageBox.Show("É necessário pelo menos uma entrada e uma saída.");
                return;
            }

            int n = entradas.Count;
            int linhas = 1 << n; // 2^n combinações

            var tabela = new System.Data.DataTable();

            for (int i = 0; i < n; i++)
                tabela.Columns.Add($"IN{i + 1}");

            for (int i = 0; i < saidas.Count; i++)
                tabela.Columns.Add($"OUT{i + 1}");

            for (int i = 0; i < linhas; i++)
            {
                bool[] bits = new bool[n];
                for (int b = 0; b < n; b++)
                    bits[b] = ((i >> b) & 1) == 1;

                for (int b = 0; b < n; b++)
                    entradas[b].SetState(bits[b]);

                EvaluateAll();

                var row = tabela.NewRow();

                for (int b = 0; b < n; b++)
                    row[b] = bits[b] ? 1 : 0;

                // saídas (usa PortInfo.Value da saída)
                for (int s = 0; s < saidas.Count; s++)
                    row[n + s] = saidas[s].Value ? 1 : 0;

                tabela.Rows.Add(row);
            }

            var janela = new TabelaVerdade(tabela);
            janela.Owner = this;
            janela.Show();
        }


        private List<PortInfo> obterEntradasSoltas()
        {
            return modelToControl.Values
                .SelectMany(c => c.Inputs ?? Array.Empty<PortInfo>())
                .Where(p => p != null && p.ConnectedWire == null)
                .ToList();
        }

        private List<PortInfo> obterSaidas()
        {
            return modelToControl.Values
                .Select(c => c.OutputPort)
                .Where(p => p != null)
                .ToList();
        }

        private void SalvarCircuitoUI(string nomeDoCircuito, string username)
        {
            try
            {
                var data = new CircuitoData
                {
                    Nome = nomeDoCircuito,
                    Username = username
                };

                // map GateModel -> tempIndex
                var gateIndex = new Dictionary<GateModel, int>();
                for (int i = 0; i < gates.Count; i++)
                    gateIndex[gates[i]] = i;

                // preparar portas
                for (int i = 0; i < gates.Count; i++)
                {
                    var gm = gates[i];
                    var ctrl = modelToControl[gm];
                    double posX = Canvas.GetLeft(ctrl);
                    double posY = Canvas.GetTop(ctrl);

                    // obter coluna e index na coluna (se existir)
                    int coluna = -1;
                    int idx = -1;
                    foreach (var kv in colunas)
                    {
                        var list = kv.Value;
                        int pos = list.IndexOf(gm);
                        if (pos >= 0)
                        {
                            coluna = kv.Key;
                            idx = pos;
                            break;
                        }
                    }

                    data.Portas.Add(new PortaRecord
                    {
                        TempIndex = i,
                        Tipo = gm.GetType().Name,
                        PosX = posX,
                        PosY = posY,
                        Coluna = coluna >= 0 ? coluna : 0,
                        IndexNaColuna = idx >= 0 ? idx : 0
                    });
                }

                // preparar conexões a partir de wires
                foreach (var w in wires)
                {
                    if (w.Source == null || w.Target == null) continue;

                    var srcGate = w.Source.Gate;
                    var tgtGate = w.Target.Gate;

                    if (!gateIndex.ContainsKey(srcGate) || !gateIndex.ContainsKey(tgtGate)) continue;
                    int srcIdx = gateIndex[srcGate];
                    int tgtIdx = gateIndex[tgtGate];

                    int srcPortIndex = GetPortIndex(w.Source);
                    int tgtPortIndex = GetPortIndex(w.Target);

                    data.Conexoes.Add(new ConexaoRecord
                    {
                        SourceTempIndex = srcIdx,
                        SourcePortIndex = srcPortIndex,
                        TargetTempIndex = tgtIdx,
                        TargetPortIndex = tgtPortIndex
                    });
                }

                int id = circuitoDAO.SalvarCircuito(data);
                MessageBox.Show($"Circuito '{nomeDoCircuito}' salvo com id {id}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar circuito: " + ex.Message);
            }
        }

        private void CarregarCircuitoUI(int circuitoId)
        {
            try
            {
                var data = circuitoDAO.CarregarCircuito(circuitoId);
                if (data == null) { MessageBox.Show("Circuito não encontrado"); return; }

                // limpar canvas atual (wires visuais)
                foreach (var w in wires.ToList())
                {
                    if (w.PathVisual != null && cnvSimulador.Children.Contains(w.PathVisual))
                        cnvSimulador.Children.Remove(w.PathVisual);
                }
                wires.Clear();

                // limpar controles das portas
                foreach (var ctrl in modelToControl.Values.ToList())
                    cnvSimulador.Children.Remove(ctrl);
                modelToControl.Clear();
                gates.Clear();
                colunas.Clear();

                // recriar portas (mantendo a ordem data.Portas -> tempIndex)
                foreach (var p in data.Portas)
                {
                    // localizar Type a partir do nome salvo
                    Type gateType = Type.GetType($"WpfApp1.Models.{p.Tipo}") ?? Type.GetType($"WpfApp1.{p.Tipo}") ?? null;
                    if (gateType == null)
                    {
                        // tenta procurar pelo assembly atual
                        gateType = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .FirstOrDefault(t => t.Name == p.Tipo);
                    }
                    if (gateType == null) continue;

                    var model = (GateModel)Activator.CreateInstance(gateType);
                    gates.Add(model);

                    var control = new GateControl();
                    string imageName = GetImageNameForGate(gateType);
                    control.Initialize(model, imageName);

                    control.AddHandler(GateControl.OutputPortClickedEvent, new RoutedEventHandler(OnOutputPortClicked));
                    control.AddHandler(GateControl.InputPortClickedEvent, new RoutedEventHandler(OnInputPortClicked));

                    // posicionamento no canvas
                    Canvas.SetLeft(control, p.PosX);
                    Canvas.SetTop(control, p.PosY);

                    cnvSimulador.Children.Add(control);
                    modelToControl[model] = control;

                    // reconstruir colunas
                    if (!colunas.ContainsKey(p.Coluna))
                        colunas[p.Coluna] = new List<GateModel>();
                    colunas[p.Coluna].Add(model);
                }

                // Agora recriar conexões (wires) usando os indices temporários
                foreach (var c in data.Conexoes)
                {
                    if (c.SourceTempIndex < 0 || c.SourceTempIndex >= gates.Count) continue;
                    if (c.TargetTempIndex < 0 || c.TargetTempIndex >= gates.Count) continue;

                    var srcGate = gates[c.SourceTempIndex];
                    var tgtGate = gates[c.TargetTempIndex];

                    var srcControl = modelToControl[srcGate];
                    var tgtControl = modelToControl[tgtGate];

                    var srcPort = GetOutputPortByIndex(srcControl, c.SourcePortIndex);
                    var tgtPort = GetInputPortByIndex(tgtControl, c.TargetPortIndex);

                    if (srcPort == null || tgtPort == null) continue;

                    var wire = new Wire(srcPort, tgtPort);
                    cnvSimulador.Children.Insert(0, wire.PathVisual);
                    wires.Add(wire);
                    tgtPort.ConnectedWire = wire;

                    if (!tgtPort.Gate.Inputs.Contains(srcPort.Gate))
                        tgtPort.Gate.Inputs.Add(srcPort.Gate);

                    tgtPort.SetState(srcPort.Gate.Output);
                    UpdateWirePosition(wire);
                    wire.UpdateColor();
                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        // garante que os controles já foram medidos e posicionados
                        this.UpdateLayout();

                        // recalcula posição/geomtria de todos os wires explicitamente
                        foreach (var wire in wires)
                        {
                            // proteção caso algum wire ainda não tenha PathVisual
                            if (wire == null || wire.PathVisual == null) continue;
                            UpdateWirePosition(wire);
                            wire.UpdateColor();
                        }

                        // atualiza visuais dos controles (círculos, cores, etc)
                        RefreshAllControls();

                        // reavalia lógica de saída das portas (opcional, mas seguro)
                        EvaluateAll();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao finalizar o carregamento visual: " + ex.Message);
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar circuito: " + ex.Message);
            }
        }

        // Helper: obtém índice do pino dentro do control (input index ou 0 para saída)
        private int GetPortIndex(PortInfo p)
        {
            if (p == null) return 0;
            // se PortInfo expõe propriedade Index -> usar aqui (ex: p.Index)
            // caso contrário, procura dentro do control inputs
            if (p.IsOutput)
                return 0; // saída padrão
            var ctrl = modelToControl.GetValueOrDefault(p.Gate);
            if (ctrl != null && ctrl.Inputs != null)
            {
                for (int i = 0; i < ctrl.Inputs.Length; i++)
                {
                    if (ctrl.Inputs[i] == p) return i;
                }
            }
            return 0;
        }

        private PortInfo GetInputPortByIndex(GateControl control, int index)
        {
            if (control == null) return null;
            if (control.Inputs == null || index < 0 || index >= control.Inputs.Length) return null;
            return control.Inputs[index];
        }

        private PortInfo GetOutputPortByIndex(GateControl control, int index)
        {
            if (control == null) return null;
            // normalmente um gate tem apenas uma saída
            return control.OutputPort;
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nome = Microsoft.VisualBasic.Interaction
                    .InputBox("Nome do circuito:", "Salvar circuito", "meu-circuito");

                if (string.IsNullOrWhiteSpace(nome))
                    return;

                // CORREÇÃO IMPORTANTE AQUI
                if (string.IsNullOrWhiteSpace(App.CurrentUsername))
                {
                    MessageBox.Show("Usuário não identificado. Faça login novamente.");
                    return;
                }

                SalvarCircuitoUI(nome, App.CurrentUsername);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao iniciar salvar: " + ex.Message);
            }
        }
        private void btnCarregar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(App.CurrentUsername))
                {
                    MessageBox.Show("Usuário não identificado. Faça login novamente.");
                    return;
                }

                var lista = circuitoDAO.ListarCircuitosDoUsuario(App.CurrentUsername);

                if (lista.Count == 0)
                {
                    MessageBox.Show("Nenhum circuito salvo.");
                    return;
                }

                var janela = new CarregarCircuitoWindow(lista);

                if (janela.ShowDialog() == true)
                {
                    var escolhido = janela.CircuitoSelecionado;
                    CarregarCircuitoUI(escolhido.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar lista: " + ex.Message);
            }
        }

    }

    internal static class DictExt
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey k)
        {
            if (d.TryGetValue(k, out var v)) return v;
            return default;
        }
    }
}

