using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Media;
using LogicaFs;

namespace MiGuiCs
{
    public partial class SopaLetrasWindow : Window
    {
    // Estado para selección visual
        private bool isSelecting = false;
        private readonly List<Border> currentSelection = new List<Border>();
        private readonly SolidColorBrush selectionBrush = new SolidColorBrush(Color.Parse("#80C6FF6B"));
        private readonly SolidColorBrush foundBrush = new SolidColorBrush(Color.Parse("#FF8BC34A"));
        private readonly HashSet<Border> foundBorders = new HashSet<Border>();
        private readonly HashSet<string> foundWords = new HashSet<string>();
        private char[,]? currentMatrix;
         private string[] placedWordsState = new string[0];
        private readonly Dictionary<string, SolidColorBrush> wordBrushes = new Dictionary<string, SolidColorBrush>();
        private readonly string[] colorPalette = new[] {
            "#39FF14",
            "#CCFF00",
            "#FF007F",
            "#00FFFF",
            "#FF9900",
            "#BF00FF",
            "#00CCFF",
            "#7FFF00",
            "#FF00FF",
            "#FFD700",
            "#FF0000",
            "#00FFCC",
            "#FF69B4",
            "#FFA500",
            "#00FF7F"
        };

        private int? selectionStartRow = null;
        private int? selectionStartCol = null;

        private double currentCellSize = 0;
        private int currentGridSize = 0;

        public SopaLetrasWindow()
        {
            InitializeComponent();

            // Generar primera vez (matriz fija 11x11)
            GenerarSopa(11);
            // Suscribir botón Rendirse (auto-solve)
            try
            {
                var btn = this.Find<Button>("RendirseButton");
                if (btn != null) btn.Click += RendirseButton_Click;
            }
            catch { }
        }


    private void GenerarSopa(int size)
        {
            var relativePath = System.IO.Path.Combine("Games", "Lectura_games", "palabrasSopaLetras.txt");
            var path = System.IO.Path.Combine(AppContext.BaseDirectory ?? string.Empty, relativePath);
            string[] lineas;
            try
            {
                bool exists = System.IO.File.Exists(path);
                Console.WriteLine($"[SopaLetras] Archivo '{path}' existe: {exists}");
                lineas = exists ? System.IO.File.ReadAllLines(path) : new string[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SopaLetras] Error comprobando/leyendo '{path}': {ex.Message}");
                lineas = new string[0];
            }
            // Generar con seed explícito (puro en la librería)
            long seed = System.DateTime.UtcNow.Ticks;
            // La librería espera un System.Random; crear uno a partir del seed
            var rnd = new System.Random((int)(seed & 0xFFFFFFFF));
            var tuple = LogicaFs.SopaLetras.generarSopa(lineas, size, size, rnd);
            var matriz = tuple.Item1;
            currentMatrix = matriz;
            var palabrasColocadas = tuple.Item2;
            // Guardar localmente las palabras colocadas en mayúsculas
            placedWordsState = palabrasColocadas.Select(p => (p ?? string.Empty).ToUpper()).ToArray();
            wordBrushes.Clear();
            for (int i = 0; i < palabrasColocadas.Length; i++)
            {
                var key = (palabrasColocadas[i] ?? string.Empty).ToUpper();
                var color = colorPalette[i % colorPalette.Length];
                var wb = new SolidColorBrush(Color.Parse(color));
                try { wb.Opacity = 0.85; } catch { }
                wordBrushes[key] = wb;
            }

            PalabrasWrap.Children.Clear();
            foreach (var palabra in palabrasColocadas)
            {
                var tbPal = new TextBlock
                {
                    Text = palabra,
                    Tag = (palabra ?? string.Empty).ToUpper(),
                    Margin = new Avalonia.Thickness(6, 4),
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
                };
                PalabrasWrap.Children.Add(tbPal);
            }

            SopaGrid.Children.Clear();
            SopaGrid.ColumnDefinitions.Clear();
            SopaGrid.RowDefinitions.Clear();

            // Definir filas y columnas
            for (int c = 0; c < size; c++)
                SopaGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
            for (int r = 0; r < size; r++)
                SopaGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));

            var tamaños = CalcularTamanos(size);
            double cellSize = tamaños.cellSize;
            double fontSize = tamaños.fontSize;

        
            try
            {
                SopaGrid.Width = cellSize * size;
                SopaGrid.Height = cellSize * size;
                currentCellSize = cellSize;
                currentGridSize = size;
            }
            catch { }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var border = new Border
                    {
                        BorderThickness = new Avalonia.Thickness(0),
                        Background = Brushes.White
                    };

                    var tb = new TextBlock
                    {
                        Text = matriz[i, j].ToString(),
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        FontFamily = "Consolas",
                        FontSize = fontSize,
                        FontWeight = FontWeight.ExtraBold,
                        Foreground = new SolidColorBrush(Color.Parse("#1F354F"))
                    };

                    border.Width = cellSize;
                    border.Height = cellSize;
                    var cellGrid = new Grid();
                    var ellipse = new Avalonia.Controls.Shapes.Ellipse
                    {
                        Fill = Brushes.White,
                        Stretch = Avalonia.Media.Stretch.UniformToFill,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Width = border.Width * 0.9,
                        Height = border.Height * 0.9
                    };

                    cellGrid.Children.Add(ellipse);
                    cellGrid.Children.Add(tb);
                    border.Tag = ellipse;
                    border.Child = cellGrid;
                    border.PointerPressed += Cell_PointerPressed;
                    border.PointerReleased += Cell_PointerReleased;
                    SopaGrid.Children.Add(border);
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                }
            }


            SopaGrid.PointerReleased -= SopaGrid_PointerReleased;
            SopaGrid.PointerCaptureLost -= SopaGrid_PointerCaptureLost;
            SopaGrid.PointerMoved -= SopaGrid_PointerMoved;


            SopaGrid.PointerReleased += SopaGrid_PointerReleased;
            SopaGrid.PointerCaptureLost += SopaGrid_PointerCaptureLost;
            SopaGrid.PointerMoved += SopaGrid_PointerMoved;

            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    double borderHeight = SopaBorder.Bounds.Height;
                    if (borderHeight <= 0)
                    {
                        borderHeight = SopaGrid.Bounds.Height;
                    }
                    var current = PalabrasWrap.Margin;
                    var gap = Math.Max(0, borderHeight * 0.05);
                    PalabrasWrap.Margin = new Avalonia.Thickness(current.Left, gap, current.Right, current.Bottom);
                }
                catch {}
            });
        }

        // Manejadores de selección visual
        private void Cell_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (sender is Border b)
            {
                isSelecting = true;
                
                try { e.Pointer.Capture(SopaGrid); } catch { }
                // Guardar coordenadas de inicio
                selectionStartRow = Grid.GetRow(b);
                selectionStartCol = Grid.GetColumn(b);
                UpdateSelection(selectionStartRow.Value, selectionStartCol.Value);
                e.Handled = true;
            }
        }


        private void Cell_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (sender is Border b)
            {
                isSelecting = false;
                try { e.Pointer.Capture(null); } catch { }
                // Construir palabra a partir de celdas seleccionadas
                var palabra = BuildWordFromSelection();
                if (!string.IsNullOrEmpty(palabra) && currentMatrix != null)
                {
                    // Construir coordenadas desde currentSelection
                    var coords = currentSelection.Select(b => System.ValueTuple.Create(Grid.GetRow(b), Grid.GetColumn(b))).ToArray();
                    var matched = TryMatchCoords(currentMatrix, coords, placedWordsState);
                    if (!string.IsNullOrEmpty(matched))
                    {
                        var key = matched.ToUpper();
                        foundWords.Add(key);
                        MarkSelectionAsFound(key);
                        MarkPalabraAsFound(matched);
                    }
                }
                ClearSelectionVisual();
                selectionStartRow = null;
                selectionStartCol = null;
                e.Handled = true;
            }
        }

        private void SopaGrid_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (!isSelecting) return;
            // Obtener la posición relativa dentro de SopaGrid
            var point = e.GetPosition(SopaGrid);
            var idx = PuntoAIndice(point.X, point.Y, currentCellSize, currentGridSize);
            if (idx == null) return;
            int row = idx.Value.row;
            int col = idx.Value.col;
            UpdateSelection(row, col);
            e.Handled = true;
        }

        private void SopaGrid_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            isSelecting = false;
            // si hay selección intentar evaluar también
            var palabra = BuildWordFromSelection();
            if (!string.IsNullOrEmpty(palabra) && currentMatrix != null)
            {
                var coords = currentSelection.Select(b => System.ValueTuple.Create(Grid.GetRow(b), Grid.GetColumn(b))).ToArray();
                var matched = TryMatchCoords(currentMatrix, coords, placedWordsState);
                if (!string.IsNullOrEmpty(matched))
                {
                    var key = matched.ToUpper();
                    foundWords.Add(key);
                    MarkSelectionAsFound(key);
                    MarkPalabraAsFound(matched);
                }
            }
            ClearSelectionVisual();
        }

        private void SopaGrid_PointerCaptureLost(object? sender, Avalonia.Input.PointerCaptureLostEventArgs e)
        {
            isSelecting = false;
            ClearSelectionVisual();
        }

        private void MarkCell(Border b)
        {
            // Evitar duplicados en la selección
            if (currentSelection.Contains(b)) return;

            // Añadir a la selección para construir la palabra
            currentSelection.Add(b);
            if (foundBorders.Contains(b))
            {
                if (b.Tag is Avalonia.Controls.Shapes.Ellipse elFound)
                {
                    if (elFound.Tag is IBrush tagBrush)
                    {
                        elFound.Fill = tagBrush;
                    }
                    else if (elFound.Fill == null)
                    {
                        elFound.Fill = foundBrush;
                    }
                }
                else
                {
                    if (b.Tag is IBrush prevBrush)
                    {
                        b.Background = prevBrush;
                    }
                    else if (b.Background == null)
                    {
                        b.Background = foundBrush;
                    }
                }
                return;
            }
            if (b.Tag is Avalonia.Controls.Shapes.Ellipse el)
            {
                if (el.Tag == null) el.Tag = el.Fill;
                el.Fill = selectionBrush;
            }
            else
            {
                if (b.Tag == null) b.Tag = b.Background;
                b.Background = selectionBrush;
            }
        }

        private void ClearSelectionVisual()
        {
            foreach (var b in currentSelection)
            {
                if (foundBorders.Contains(b)) continue;
                if (b.Tag is Avalonia.Controls.Shapes.Ellipse el)
                {
                    if (el.Tag is IBrush prevBrush) el.Fill = prevBrush;
                    else el.Fill = Brushes.White;
                }
                else if (b.Tag is IBrush prev)
                {
                    b.Background = prev;
                }
                else
                {
                    b.Background = Brushes.White;
                }
            }
            currentSelection.Clear();
        }

        private void UpdateSelection(int targetRow, int targetCol)
        {
            if (!selectionStartRow.HasValue || !selectionStartCol.HasValue) return;
            int sr = selectionStartRow.Value;
            int sc = selectionStartCol.Value;
            // Calcular la línea alineada localmente
            var linea = ObtenerLineaAlineada(sr, sc, targetRow, targetCol);
            if (linea == null || linea.Length == 0)
            {
                ClearSelectionVisual();
                return;
            }
            ClearSelectionVisual();
            foreach (var tup in linea)
            {
                var r = tup.Item1;
                var c = tup.Item2;
                var b = FindCellBorder(r, c);
                if (b != null) MarkCell(b);
            }
        }

        private Border? FindCellBorder(int row, int col)
        {
            foreach (var child in SopaGrid.Children)
            {
                if (child is Border br)
                {
                    int rr = Grid.GetRow(br);
                    int cc = Grid.GetColumn(br);
                    if (rr == row && cc == col) return br;
                }
            }
            return null;
        }

        private (double cellSize, double fontSize) CalcularTamanos(int size)
        {
            double baseFont = 20.0;
            double fontSize = System.Math.Max(8.0, baseFont - (size - 10));

            double baseCell = 30.0;
            double cellSize = System.Math.Max(12.0, baseCell - (size - 10) * 1.0);
            cellSize *= 1.10; // aumentar 10%
            return (cellSize, fontSize);
        }

        private (int row, int col)? PuntoAIndice(double x, double y, double cellSize, int gridSize)
        {
            if (cellSize <= 0 || gridSize <= 0) return null;
            int col = (int)(x / cellSize);
            int row = (int)(y / cellSize);
            if (row < 0 || col < 0 || row >= gridSize || col >= gridSize) return null;
            return (row, col);
        }

        private (int,int)[] ObtenerLineaAlineada(int sr, int sc, int tr, int tc)
        {
            try
            {
                var tuples = LogicaFs.SopaLetras.obtenerLineaAlineada(sr, sc, tr, tc);
                if (tuples == null) return new (int,int)[0];
                var res = new (int,int)[tuples.Length];
                for (int i = 0; i < tuples.Length; i++)
                {
                    var t = tuples[i];
                    // t may be System.Tuple<int,int>
                    res[i] = (t.Item1, t.Item2);
                }
                return res;
            }
            catch
            {
                return new (int,int)[0];
            }
        }

        private string BuildWordFromSelection()
        {
            if (currentSelection.Count == 0) return string.Empty;
            var chars = new List<char>();
            foreach (var b in currentSelection)
            {
                if (b.Child is Grid g)
                {
                    foreach (var child in g.Children)
                    {
                        if (child is TextBlock tb)
                        {
                            if (!string.IsNullOrEmpty(tb.Text)) chars.Add(tb.Text[0]);
                            break;
                        }
                    }
                }
            }
            return new string(chars.ToArray());
        }

        // Comprueba localmente la secuencia de coordenadas contra las palabras colocadas
        private string? TryMatchCoords(char[,] mat, System.ValueTuple<int,int>[] coords, string[] placed)
        {
            if (mat == null || coords == null || coords.Length == 0) return null;
            try
            {
                var chars = new char[coords.Length];
                for (int i = 0; i < coords.Length; i++)
                {
                    var (r, c) = coords[i];
                    if (r < 0 || c < 0 || r >= mat.GetLength(0) || c >= mat.GetLength(1)) return null;
                    chars[i] = mat[r, c];
                }
                var s = new string(chars).ToUpper();
                if (placed != null)
                {
                    if (placed.Any(p => string.Equals(p, s, System.StringComparison.OrdinalIgnoreCase))) return s;
                    var rev = new string(s.Reverse().ToArray());
                    if (placed.Any(p => string.Equals(p, rev, System.StringComparison.OrdinalIgnoreCase))) return rev;
                }
                return null;
            }
            catch { return null; }
        }

        private void MarkSelectionAsFound()
        {
            foreach (var b in currentSelection)
            {
                if (!foundBorders.Contains(b))
                {
                    // pintar de color encontrado
                    if (b.Tag is Avalonia.Controls.Shapes.Ellipse el)
                    {
                        // Guardar el brush en el.Tag para evitar que otras rutinas lo reemplacen
                        el.Tag = foundBrush;
                        el.Fill = foundBrush;
                    }
                    else
                    {
                        b.Tag = foundBrush;
                        b.Background = foundBrush;
                    }
                    foundBorders.Add(b);
                }
            }
        }

        // Marca sólo el TextBlock que corresponde a la palabra encontrada
        private void MarkPalabraAsFound(string palabra)
        {
            if (string.IsNullOrEmpty(palabra)) return;
            var key = palabra.ToUpper();
            foreach (var child in PalabrasWrap.Children)
            {
                if (child is TextBlock tb)
                {
                    var tag = (tb.Tag as string) ?? (tb.Text ?? string.Empty).ToUpper();
                    if (tag.ToUpper() == key)
                    {
                        tb.Foreground = new SolidColorBrush(Color.Parse("#808080"));
                        return;
                    }
                }
            }
        }

        // Marcar las celdas de la selección usando el color asignado a la palabra
        private void MarkSelectionAsFound(string palabraKey)
        {
            var key = (palabraKey ?? string.Empty).ToUpper();
            SolidColorBrush brush = foundBrush;
            if (wordBrushes.ContainsKey(key)) brush = wordBrushes[key];

            foreach (var b in currentSelection)
            {
                if (!foundBorders.Contains(b))
                {
                    if (b.Tag is Avalonia.Controls.Shapes.Ellipse el)
                    {
                        el.Tag = brush;
                        el.Fill = brush;
                    }
                    else
                    {
                        b.Tag = brush;
                        b.Background = brush;
                    }
                    foundBorders.Add(b);
                }
                else
                {
                    // si ya estaba marcada asegurar el color
                    if (b.Tag is Avalonia.Controls.Shapes.Ellipse el2)
                    {
                        el2.Fill = brush;
                        el2.Tag = brush;
                    }
                    else
                    {
                        b.Background = brush;
                        b.Tag = brush;
                    }
                }
            }
        }

        // Funcionalidad de "Rendirse" eliminada — método intencionalmente retirado.
        private void RendirseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (currentMatrix == null || placedWordsState == null) return;

            try
            {
                // Llamar a la función F# que resuelve todas las palabras
                var results = LogicaFs.SopaLetras.SolveAll(currentMatrix, placedWordsState);
                if (results == null) return;

                // Para cada palabra con ruta, marcar las celdas y la palabra
                foreach (var pair in results)
                {
                    var palabra = pair.Item1 ?? string.Empty;
                    var ruta = pair.Item2;
                    if (ruta == null || ruta.Length == 0) continue;

                    // Marcar las celdas de la ruta: buscar el Border correspondiente y agregarlo temporalmente a currentSelection
                    ClearSelectionVisual();
                    foreach (var coord in ruta)
                    {
                        int r = coord.Item1;
                        int c = coord.Item2;
                        var b = FindCellBorder(r, c);
                        if (b != null)
                        {
                            // Asegurar que currentSelection contenga estas celdas para que MarkSelectionAsFound opere correctamente
                            if (!currentSelection.Contains(b)) currentSelection.Add(b);
                        }
                    }

                    // Marcar visualmente
                    MarkSelectionAsFound(palabra.ToUpper());
                    MarkPalabraAsFound(palabra);

                    // dejar currentSelection vacío para la siguiente palabra
                    currentSelection.Clear();
                }
            }
            catch { }
        }
    }
}

