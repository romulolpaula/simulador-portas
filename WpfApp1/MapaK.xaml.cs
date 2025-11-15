using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    public partial class MapaK : Window
    {
        public MapaK()
        {
            InitializeComponent();
        }

        // ---------------- UI events ----------------
        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int vars = int.Parse(((ComboBoxItem)cmbVars.SelectedItem).Content.ToString());
                string input = txtInput.Text.Trim();
                bool[] truthBits = ParseInput(input, vars);
                DrawKMap(truthBits, vars);
                txtExpressaoSimplificada.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
            }
        }

        private void BtnAutoGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int vars = int.Parse(((ComboBoxItem)cmbVars.SelectedItem).Content.ToString());
                string input = txtInput.Text.Trim();
                bool[] truthBits = ParseInput(input, vars);

                var grupos = FindGroups(truthBits, vars);
                DrawKMap(truthBits, vars); // redesenha células limpas
                DrawGroupsFilled(grupos, vars); // preenchimento leve + contorno

                string expressao = SimplificarExpressaoFromGroups(grupos, vars);
                txtExpressaoSimplificada.Text = expressao;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog
                {
                    Filter = "Imagem PNG|*.png",
                    FileName = "MapaK.png"
                };

                if (dlg.ShowDialog() == true)
                {
                    RenderTargetBitmap rtb = new RenderTargetBitmap(
                        (int)Math.Max(karnaughCanvas.Width, 100), (int)Math.Max(karnaughCanvas.Height, 100), 96, 96, PixelFormats.Pbgra32);
                    rtb.Render(karnaughCanvas);

                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(rtb));

                    using (var stream = System.IO.File.Create(dlg.FileName))
                        encoder.Save(stream);

                    MessageBox.Show("Mapa exportado com sucesso!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao exportar: {ex.Message}");
            }
        }

        // ---------------- Input parsing ----------------
        private bool[] ParseInput(string input, int vars)
        {
            int length = (int)Math.Pow(2, vars);
            bool[] bits = new bool[length];

            if (string.IsNullOrWhiteSpace(input))
                return bits;

            if (input.Contains(","))
            {
                foreach (string part in input.Split(','))
                {
                    if (int.TryParse(part.Trim(), out int idx) && idx >= 0 && idx < length)
                        bits[idx] = true;
                }
            }
            else
            {
                if (input.Length != length)
                    throw new Exception($"Esperado {length} bits para {vars} variáveis.");

                for (int i = 0; i < length; i++)
                    bits[i] = input[i] == '1';
            }

            return bits;
        }

        // ---------------- Gray orders & helpers ----------------
        private int[,] GetKmapOrder(int vars)
        {
            return vars switch
            {
                // 2 variables -> 2x2: rows = A(0,1), cols = B(0,1)
                2 => new int[,] { { 0, 1 }, { 2, 3 } },

                // 3 variables -> 2x4: rows = A(0,1), cols = BC in Gray 00,01,11,10
                3 => new int[,] { { 0, 1, 3, 2 }, { 4, 5, 7, 6 } },

                // 4 variables -> 4x4: rows = AB (Gray), cols = CD (Gray)
                4 => new int[,] {
                    { 0, 1, 3, 2 },
                    { 4, 5, 7, 6 },
                    { 12,13,15,14 },
                    { 8, 9,11,10 }
                },
                _ => new int[,] { }
            };
        }

        // ---------------- Draw kmap with labels ----------------
        private void DrawKMap(bool[] truthBits, int vars)
        {
            karnaughCanvas.Children.Clear();

            int rows = (vars == 2) ? 2 : (vars == 3 ? 2 : 4);
            int cols = (vars == 2) ? 2 : 4;
            double cellSize = 80;

            // cabeçalhos Gray
            string[] grayCols2 = { "0", "1" };                // for 2 vars columns (B)
            string[] grayCols4 = { "00", "01", "11", "10" };  // for 3/4 vars columns (BC or CD)
            string[] grayRowsA = { "0", "1" };                // for 3 vars rows (A)
            string[] grayRowsAB = { "00", "01", "11", "10" }; // for 4 vars rows (AB)

            // titles
            string colTitle = vars switch { 2 => "B", 3 => "BC", 4 => "CD", _ => "" };
            string rowTitle = vars switch { 2 => "A", 3 => "A", 4 => "AB", _ => "" };

            // draw col title
            TextBlock colTitleText = new TextBlock
            {
                Text = colTitle,
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(colTitleText, cellSize * (cols / 2.0) - 10);
            Canvas.SetTop(colTitleText, -50);
            karnaughCanvas.Children.Add(colTitleText);

            // draw column labels
            for (int x = 0; x < cols; x++)
            {
                string label = vars == 2 ? grayCols2[x] : grayCols4[x];
                TextBlock lbl = new TextBlock
                {
                    Text = label,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(lbl, x * cellSize + cellSize / 3);
                Canvas.SetTop(lbl, -25);
                karnaughCanvas.Children.Add(lbl);
            }

            // draw row title
            TextBlock rowTitleText = new TextBlock
            {
                Text = rowTitle,
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(rowTitleText, -60);
            Canvas.SetTop(rowTitleText, cellSize * (rows / 2.0) - 20);
            karnaughCanvas.Children.Add(rowTitleText);

            // draw row labels
            for (int y = 0; y < rows; y++)
            {
                string label = vars == 4 ? grayRowsAB[y] : grayRowsA[y];
                TextBlock lbl = new TextBlock
                {
                    Text = label,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(lbl, -35);
                Canvas.SetTop(lbl, y * cellSize + cellSize / 3);
                karnaughCanvas.Children.Add(lbl);
            }

            var order = GetKmapOrder(vars);

            // desenha células e valores
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    int index = order[y, x];

                    Rectangle rect = new Rectangle
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Fill = Brushes.White
                    };
                    Canvas.SetLeft(rect, x * cellSize);
                    Canvas.SetTop(rect, y * cellSize);
                    karnaughCanvas.Children.Add(rect);

                    TextBlock txtIndex = new TextBlock
                    {
                        Text = index.ToString(),
                        FontSize = 10,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(3, 2, 0, 0)
                    };
                    Canvas.SetLeft(txtIndex, x * cellSize);
                    Canvas.SetTop(txtIndex, y * cellSize);
                    karnaughCanvas.Children.Add(txtIndex);

                    TextBlock txtValue = new TextBlock
                    {
                        Text = (index < truthBits.Length && truthBits[index]) ? "1" : "0",
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        Width = cellSize,
                        Height = cellSize,
                        TextAlignment = TextAlignment.Center
                    };
                    Canvas.SetLeft(txtValue, x * cellSize);
                    Canvas.SetTop(txtValue, y * cellSize + (cellSize / 3.5));
                    karnaughCanvas.Children.Add(txtValue);
                }
            }

            // ajuste do canvas
            karnaughCanvas.Width = cols * cellSize + 120;
            karnaughCanvas.Height = rows * cellSize + 120;
        }

        // ---------------- Draw groups (filled + outline) ----------------
        private void DrawGroupsFilled(List<List<(int y, int x)>> grupos, int vars)
        {
            double cellSize = 80;
            Random rand = new Random(42);

            foreach (var grupo in grupos)
            {
                Color cor = Color.FromArgb(110,
                    (byte)rand.Next(60, 220),
                    (byte)rand.Next(60, 220),
                    (byte)rand.Next(60, 220));
                SolidColorBrush fill = new SolidColorBrush(cor);

                foreach (var (y, x) in grupo)
                {
                    Rectangle overlay = new Rectangle
                    {
                        Width = cellSize,
                        Height = cellSize,
                        RadiusX = 8,
                        RadiusY = 8,
                        Fill = fill,
                        Stroke = Brushes.Transparent,
                        StrokeThickness = 0
                    };
                    Canvas.SetLeft(overlay, x * cellSize);
                    Canvas.SetTop(overlay, y * cellSize);
                    karnaughCanvas.Children.Add(overlay);
                }

                // draw outlines grouping consecutive columns per row for better visuals
                var byRow = grupo.GroupBy(g => g.y).OrderBy(g => g.Key);
                foreach (var rowGroup in byRow)
                {
                    var xs = rowGroup.Select(r => r.x).Distinct().OrderBy(v => v).ToArray();

                    int start = xs[0];
                    int end = xs[0];
                    for (int i = 1; i < xs.Length; i++)
                    {
                        if (xs[i] == end + 1)
                            end = xs[i];
                        else
                        {
                            DrawOutlineRect(start, end, rowGroup.Key, cellSize, fill);
                            start = xs[i];
                            end = xs[i];
                        }
                    }
                    DrawOutlineRect(start, end, rowGroup.Key, cellSize, fill);
                }
            }
        }

        private void DrawOutlineRect(int xStart, int xEnd, int y, double cellSize, SolidColorBrush fill)
        {
            Rectangle outline = new Rectangle
            {
                Width = (xEnd - xStart + 1) * cellSize,
                Height = cellSize,
                Stroke = new SolidColorBrush(Color.FromArgb(200, fill.Color.R, fill.Color.G, fill.Color.B)),
                StrokeThickness = 3,
                RadiusX = 10,
                RadiusY = 10,
                Fill = Brushes.Transparent
            };
            Canvas.SetLeft(outline, xStart * cellSize + 2);
            Canvas.SetTop(outline, y * cellSize + 2);
            karnaughCanvas.Children.Add(outline);
        }

        // ---------------- Find groups (2,4,8) with wrap-around ----------------
        private List<List<(int y, int x)>> FindGroups(bool[] truthBits, int vars)
        {
            int rows = (vars == 2) ? 2 : (vars == 3 ? 2 : 4);
            int cols = (vars == 2) ? 2 : 4;
            var order = GetKmapOrder(vars);

            // coord->index matrix
            int[,] coordToIndex = new int[rows, cols];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    coordToIndex[y, x] = order[y, x];

            bool IsOne(int y, int x)
            {
                int idx = coordToIndex[(y + rows) % rows, (x + cols) % cols];
                return idx < truthBits.Length && truthBits[idx];
            }

            // sizes allowed (prioritize bigger)
            int[] sizes = vars == 4 ? new[] { 8, 4, 2 } : new[] { 4, 2 };

            var candidatos = new List<List<(int y, int x)>>();

            foreach (int size in sizes)
            {
                for (int h = 1; h <= rows; h *= 2)
                {
                    for (int w = 1; w <= cols; w *= 2)
                    {
                        if (h * w != size) continue;

                        for (int y0 = 0; y0 < rows; y0++)
                        {
                            for (int x0 = 0; x0 < cols; x0++)
                            {
                                bool allOne = true;
                                var grupo = new List<(int y, int x)>();
                                for (int dy = 0; dy < h; dy++)
                                {
                                    for (int dx = 0; dx < w; dx++)
                                    {
                                        int yy = (y0 + dy) % rows;
                                        int xx = (x0 + dx) % cols;
                                        if (!IsOne(yy, xx)) { allOne = false; break; }
                                        grupo.Add((yy, xx));
                                    }
                                    if (!allOne) break;
                                }
                                if (allOne)
                                {
                                    var normal = grupo.Distinct().OrderBy(t => t.y).ThenBy(t => t.x).ToList();
                                    if (!candidatos.Any(c => c.Count == normal.Count && c.All(n => normal.Contains(n))))
                                        candidatos.Add(normal);
                                }
                            }
                        }
                    }
                }
            }

            // greedy selection by coverage
            var selecionados = new List<List<(int y, int x)>>();
            var uncovered = new HashSet<(int y, int x)>();
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                {
                    int idx = coordToIndex[y, x];
                    if (idx < truthBits.Length && truthBits[idx]) uncovered.Add((y, x));
                }

            foreach (var grupo in candidatos.OrderByDescending(g => g.Count))
            {
                if (grupo.Any(c => uncovered.Contains(c)))
                {
                    selecionados.Add(grupo);
                    foreach (var c in grupo) uncovered.Remove(c);
                }
            }

            // last resort: singletons if still uncovered
            foreach (var cell in uncovered.ToList())
            {
                selecionados.Add(new List<(int y, int x)> { cell });
                uncovered.Remove(cell);
            }

            return selecionados;
        }

        // ---------------- Simplify expression from groups (correct mapping) ----------------
        private string SimplificarExpressaoFromGroups(List<List<(int y, int x)>> grupos, int vars)
        {
            string[] letras = vars switch
            {
                2 => new[] { "A", "B" },
                3 => new[] { "A", "B", "C" },
                4 => new[] { "A", "B", "C", "D" },
                _ => Array.Empty<string>()
            };

            string[] grayCols2 = { "0", "1" };
            string[] grayCols4 = { "00", "01", "11", "10" };
            string[] grayRowsA = { "0", "1" };
            string[] grayRowsAB = { "00", "01", "11", "10" };

            var order = GetKmapOrder(vars);
            List<string> termos = new();

            foreach (var grupo in grupos)
            {
                var bitsDoGrupo = new List<string>();

                foreach (var (y, x) in grupo)
                {
                    string bits = vars switch
                    {
                        2 => $"{grayRowsA[y]}{grayCols2[x]}",   // A B
                        3 => $"{grayRowsA[y]}{grayCols4[x]}",   // A BC
                        4 => $"{grayRowsAB[y]}{grayCols4[x]}",  // AB CD
                        _ => ""
                    };
                    bitsDoGrupo.Add(bits);
                }

                string termo = "";
                for (int bit = 0; bit < vars; bit++)
                {
                    bool all1 = bitsDoGrupo.All(b => b[bit] == '1');
                    bool all0 = bitsDoGrupo.All(b => b[bit] == '0');

                    if (all1) termo += letras[bit];
                    else if (all0) termo += letras[bit] + "'";
                }

                if (string.IsNullOrEmpty(termo))
                    termo = "1";

                termos.Add(termo);
            }

            termos = termos.Distinct().ToList();
            if (termos.Count == 0) return "F = 0";

            bool hasSingleton = grupos.Any(g => g.Count == 1);
            string expr = "F(" + string.Join(",", letras) + ") = " + string.Join(" + ", termos);
            if (hasSingleton) expr += "   // atenção: alguns 1s não puderam ser agrupados (singleton)";

            return expr;
        }
    }
}
