using NotificadorVendasSoftware.Models;

namespace NotificadorVendasSoftware;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        bool iniciarMinimizado = args.Contains("--minimized");

        Application.Run(new Form1(iniciarMinimizado));
    }
}