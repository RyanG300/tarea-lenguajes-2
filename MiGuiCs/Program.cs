/*using Avalonia.Controls;

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
}*/

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Reactive;

namespace MiGuiCs;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or
    // any SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}