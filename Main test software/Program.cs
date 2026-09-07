using System;
using System.IO;
using System.Windows.Forms;

namespace ATS
{
    static class Program
    {
        // Full path to D:\Github\TesterKWN\config_{N}\ based on tester_no.txt
        internal static string ConfigPath { get; private set; }

        [STAThread]
        static void Main()
        {
            string testerNo = File.Exists("tester_no.txt")
                ? File.ReadAllText("tester_no.txt").Trim()
                : "1";
            // BaseDirectory = bin\Debug\  →  ..\..\..\  = D:\Github\TesterKWN\
            string repoDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string configFolder = Path.Combine(repoDir, "config_" + testerNo);

            if (!Directory.Exists(configFolder))
            {
                MessageBox.Show("Config folder not found:\n" + configFolder, "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ConfigPath = configFolder + Path.DirectorySeparatorChar;

            // TODO: pass ConfigPath to SetupPay once the DLL is updated
            // e.g. SetupPay.FormPay.SetConfigPath(ConfigPath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new fMain());
        }
    }
}
