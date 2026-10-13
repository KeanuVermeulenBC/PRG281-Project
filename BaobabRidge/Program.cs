using System;
using System.Windows.Forms;

namespace BaobabRidge
{
    internal static class Program
    {
        // Starts the Windows Forms application with the real animals.txt file.
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
