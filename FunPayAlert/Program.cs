using System;
using System.Windows.Forms;

namespace FunPayAlert
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(Global.GetMainForm());
        }
    }
}
