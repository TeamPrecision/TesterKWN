using System.Configuration;
using System.Diagnostics;
using System.Windows.Forms;

namespace ATS
{
    public partial class Document : Form
    {
        public Document()
        {
            InitializeComponent();
        }

        private void OpenFolder(string configKey)
        {
            string path = ConfigurationManager.AppSettings[configKey];
            if (!string.IsNullOrEmpty(path))
                Process.Start("explorer.exe", path);
        }

        private void pb_schematicCus_Click(object sender, System.EventArgs e)   => OpenFolder("Path.SchematicCus");
        private void pb_gerberCus_Click(object sender, System.EventArgs e)       => OpenFolder("Path.GerberCus");
        private void pb_testspecCus_Click(object sender, System.EventArgs e)     => OpenFolder("Path.TestSpecCus");
        private void pb_firmwareCus_Click(object sender, System.EventArgs e)     => OpenFolder("Path.FirmwareCus");
        private void pb_bomCus_Click(object sender, System.EventArgs e)          => OpenFolder("Path.BomCus");
        private void pb_modelTeam_Click(object sender, System.EventArgs e)       => OpenFolder("Path.ModelTeam");
        private void pb_fixtureTeam_Click(object sender, System.EventArgs e)     => OpenFolder("Path.FixtureTeam");
        private void pb_datalogTeam_Click(object sender, System.EventArgs e)     => OpenFolder("Path.DatalogTeam");
        private void pb_userManualTeam_Click(object sender, System.EventArgs e)  => OpenFolder("Path.UserManualTeam");
        private void pb_flowchartTeam_Click(object sender, System.EventArgs e)   => OpenFolder("Path.FlowchartTeam");
        private void pb_fingerprintTeam_Click(object sender, System.EventArgs e) => OpenFolder("Path.FingerprintTeam");
    }
}
