using Avalonia.Controls;
using Avalonia.Interactivity;
using LogicaFs; // <- espacio de nombres de la librería F#


namespace MiGuiCs
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            var greetBtn = this.FindControl<Button>("GreetBtn");
            var nameBox = this.FindControl<TextBox>("NameBox");
            var greetingText = this.FindControl<TextBlock>("GreetingText");


            greetBtn.Click += (_, __) =>
            {
                var name = string.IsNullOrWhiteSpace(nameBox.Text) ? "Mundo" : nameBox.Text;
                // Llamada a la función F#
                var text = Greeting.GetGreeting(name);
                greetingText.Text = text;
            };
        }
    }   
}