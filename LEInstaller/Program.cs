using System;
using System.Windows.Forms;

namespace LEInstaller
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form1=null;
            if (args.Length == 1)
            {
                if (args[0] == "--InstallAll")
                    form1 = new Form1(1);
                else if (args[0] == "--UninstallAll")
                    form1 = new Form1(2);
                else
                    form1 = new Form1();
            }
            else
                form1 = new Form1();
            Application.Run(form1);

            

            
        }
    }
}