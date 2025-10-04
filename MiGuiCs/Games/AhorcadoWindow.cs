using Avalonia.Controls;
using LogicaFs;
using System;
using System.IO;

namespace MiGuiCs
{
    public partial class AhorcadoWindow : Window
    {
        string palabraBuscar;
        int intentos = 6;
        public AhorcadoWindow()
        {
            InitializeComponent();
            string path = "MiGuiCs/Games/Lectura_games/palabrasAhorcado.txt";
            var rnd = new System.Random();
            palabraBuscar = AhorcadoGame.palabraAleatoria(AhorcadoGame.separarPalabras(File.ReadAllText(path)), rnd);
            var palabraElegidaTextBlock = this.FindControl<TextBlock>("PalabraElegida");
            var intentosTextBlock = this.FindControl<TextBlock>("IntentosTextBlock");
            intentosTextBlock.Text = $"Intentos restantes: {intentos}";

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
                    Width = 30,
                    Height = 30,
                    Margin = new Avalonia.Thickness(2)
                };
                letraBtn.Click += (_, __) =>
                {
                    letraBtn.IsEnabled = false;
                    if (AhorcadoGame.comprobarLetra(palabraBuscar, char.ToLower(char.Parse(letraBtn.Content.ToString()))))
                    {
                        palabraElegidaTextBlock.Text = AhorcadoGame.revelarLetras(palabraBuscar, palabraElegidaTextBlock.Text.Replace(" ", ""), char.ToLower(char.Parse(letraBtn.Content.ToString())));
                        if (!palabraElegidaTextBlock.Text.Contains("_"))
                        {
                            var resultadoTextBlock = this.FindControl<TextBlock>("InstruccionesTextBlock");
                            resultadoTextBlock.Text = "¡Felicidades! Has ganado.";
                            resultadoTextBlock.Foreground = Avalonia.Media.Brushes.Green;
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
                        intentos--;
                        intentosTextBlock.Text = $"Intentos restantes: {intentos}";
                        /*var linea = new Avalonia.Controls.Shapes.Line
                        {
                            Stroke = Avalonia.Media.Brushes.Red,
                            StrokeThickness = 2
                        };*/
                        if (intentos == 0)
                        {
                            var resultadoTextBlock = this.FindControl<TextBlock>("InstruccionesTextBlock");
                            resultadoTextBlock.Text = $"Has perdido. La palabra era: {palabraBuscar}";
                            resultadoTextBlock.Foreground = Avalonia.Media.Brushes.Red;
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
        }
    }
}
