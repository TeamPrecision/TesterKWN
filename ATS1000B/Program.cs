using System;
using System.Windows.Forms;

namespace ATS1000B
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += (s, e) =>
                MessageBox.Show("Unhandled error:\n" + e.Exception, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                MessageBox.Show("Fatal:\n" + e.ExceptionObject, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                string msg = "Startup error:\n" + ex;
                System.IO.File.WriteAllText("crash.log", msg);
                MessageBox.Show(msg, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
