using System.Net.Http.Json;
using System.Text.Json;

namespace 근무시간계산기;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}
