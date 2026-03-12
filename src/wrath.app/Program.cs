using Microsoft.UI.Xaml;

namespace Wrath.App;

public static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.Start(_ => new App());
    }
}
