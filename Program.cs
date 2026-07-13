using System.Text;

namespace TSProxyCapture;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Register Windows-874 encoding support for Server.ini
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}