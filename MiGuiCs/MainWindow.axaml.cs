using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.TextFormatting;
using LogicaFs; // <- espacio de nombres de la librería F#

namespace MiGuiCs
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();


            var ahorcadoBtn = this.FindControl<Button>("ahorcadoBtn");
            var sopaBtn = this.FindControl<Button>("sopaBtn");
            //var nameBox = this.FindControl<TextBox>("NameBox");
            //var greetingText = this.FindControl<TextBlock>("GreetingText");
            //var otro = this.FindControl<TextBlock>("Otro");
             // Ejemplo de uso de la librería F#


            ahorcadoBtn.Click += (_, __) =>
            {
                var ventanaAhorcado = new AhorcadoWindow();
                ventanaAhorcado.Show();
                //this.Close();
            };
            sopaBtn.Click += (_, __) =>
            {
                var ventanaSopa = new SopaLetrasWindow();
                ventanaSopa.Show();
                //this.Close();
            };
        }
    }
}