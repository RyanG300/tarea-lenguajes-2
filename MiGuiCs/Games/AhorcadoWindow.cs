using Avalonia.Controls;
using LogicaFs;
using System;
using System.IO;
using System.Timers;
using Avalonia.Threading;

namespace MiGuiCs
{
    public partial class AhorcadoWindow : Window
    {
        string palabraBuscar;
        int intentos = 5;
        static int segundos = 0;
        DispatcherTimer dispatcherTimer;
        public AhorcadoWindow()
        {
            InitializeComponent();
            ocultarMonigote();
            string path = "MiGuiCs/Games/Lectura_games/palabrasAhorcado.txt";
            var rnd = new System.Random();
            palabraBuscar = AhorcadoGame.palabraAleatoria(AhorcadoGame.separarPalabras(File.ReadAllText(path)), rnd);
            var palabraElegidaTextBlock = this.FindControl<TextBlock>("PalabraElegida");
            var intentosTextBlock = this.FindControl<TextBlock>("IntentosTextBlock");
            intentosTextBlock.Text = $"❤️ Intentos restantes: {intentos}";

            // Timer setup
            var tiempoTextBlock = this.FindControl<TextBlock>("TiempoTextBlock");
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += (sender, e) =>
            {
                segundos++;
                tiempoTextBlock.Text = $"⏱️ {AhorcadoGame.tiempo(segundos)}";
            };
            dispatcherTimer.Start();

            // Botones
            for (int i = 0; i < palabraBuscar.Length; i++)
            {
                palabraElegidaTextBlock.Text += "_ ";
            }
            var panel = this.FindControl<WrapPanel>("BotonesPanel");
            var abedecedario = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ";
            for (int i = 0; i < abedecedario.Length; i++)
            {
                var letraBtn = new Button
                {
                    Content = abedecedario[i].ToString(),
                    Width = 45,
                    Height = 45,
                    Margin = new Avalonia.Thickness(4),
                    FontSize = 18,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Background = Avalonia.Media.Brush.Parse("#74b9ff"),
                    Foreground = Avalonia.Media.Brushes.White,
                    BorderBrush = Avalonia.Media.Brush.Parse("#0984e3"),
                    BorderThickness = new Avalonia.Thickness(2),
                    CornerRadius = new Avalonia.CornerRadius(8),
                    Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
                };
                letraBtn.Click += (_, __) =>
                {
                    letraBtn.IsEnabled = false;
                    if (AhorcadoGame.comprobarLetra(palabraBuscar, char.ToLower(char.Parse(letraBtn.Content.ToString()))))
                    {
                        // Letra correcta - botón verde
                        letraBtn.Background = Avalonia.Media.Brush.Parse("#00b894");
                        letraBtn.BorderBrush = Avalonia.Media.Brush.Parse("#00a085");
                        palabraElegidaTextBlock.Text = AhorcadoGame.revelarLetras(palabraBuscar, palabraElegidaTextBlock.Text.Replace(" ", ""), char.ToLower(char.Parse(letraBtn.Content.ToString())));
                        if (AhorcadoGame.esGanador(AhorcadoGame.stringToCharArray(palabraElegidaTextBlock.Text)))
                        {
                            var resultadoTextBlock = this.FindControl<TextBlock>("InstruccionesTextBlock");
                            resultadoTextBlock.Text = "🎉 ¡Felicidades! Has ganado";
                            resultadoTextBlock.Foreground = Avalonia.Media.Brushes.Green;
                            dispatcherTimer.Stop();
                            foreach (var child in panel.Children)
                            {
                                if (child is Button btn)
                                {
                                    btn.IsEnabled = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Letra incorrecta - botón rojo
                        letraBtn.Background = Avalonia.Media.Brush.Parse("#e17055");
                        letraBtn.BorderBrush = Avalonia.Media.Brush.Parse("#d63031");
                        intentos--;
                        intentosTextBlock.Text = $"❤️ Intentos restantes: {intentos}";
                        mostrarMonigote(intentos);
                        if (intentos == 0)
                        {
                            var resultadoTextBlock = this.FindControl<TextBlock>("InstruccionesTextBlock");
                            resultadoTextBlock.Text = $"💀 Has perdido. La palabra era: {palabraBuscar}";
                            resultadoTextBlock.Foreground = Avalonia.Media.Brushes.Red;
                            dispatcherTimer.Stop();
                            foreach (var child in panel.Children)
                            {
                                if (child is Button btn)
                                {
                                    btn.IsEnabled = false;
                                }
                            }
                        }
                    }
                };
                panel.Children.Add(letraBtn);

            }

            var reiniciarBtn = this.FindControl<Button>("ReiniciarBtn");
            reiniciarBtn.Click += (_, __) =>
            {
                var nuevaVentana = new AhorcadoWindow();
                segundos = 0;
                dispatcherTimer.Stop();
                nuevaVentana.Show();
                this.Close();
            };
        }

        public void ocultarMonigote()
        {
            var HangmanCanvas = this.FindControl<Canvas>("HangmanCanvas");
            foreach (var child in HangmanCanvas.Children)
            {
                child.IsVisible = false;
            }
        }

        public void mostrarMonigote(int intentosRestantes)
        {
            var HangmanCanvas = this.FindControl<Canvas>("HangmanCanvas");
            int partesTotales = 10;
            int partesAMostrar = partesTotales - intentosRestantes;

            for (int i = 0; i < partesAMostrar; i++)
            {
                if (i < HangmanCanvas.Children.Count)
                {
                    HangmanCanvas.Children[i].IsVisible = true;
                }
            }
        }
    }
}
