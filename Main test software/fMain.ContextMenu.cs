using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Spire.Xls;
using System.IO.Ports;
using System.Management;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.Web.Script.Serialization;
using System.Net;
using ATS.MiniForm;
using ATS.Config;
using ATS.Hardware;
using ATS.Models;
using System.Text.RegularExpressions;
using System.IO.Pipes;
using Newtonsoft.Json;

namespace ATS
{
    public partial class fMain
    {
        #region ====================================================== ContextMenuStrip Event ======================================================
        private bool flagLoadTestSpec;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < configTester.numHead; i++)
            {
                if (flag_test[i])
                {
                    MessageBox.Show("_ไม่สามารถโหลดไฟล์ได้ ขณะโปรแกรมกำลังเทส");
                    return;
                }
            }
            string nameFG = cbb_fg.Text;
            try
            {
                excel.workBook.LoadFromFile("../../TestDescription/" + nameFG + excel.lastName);
            }
            catch
            {
                Log(LogMsgType.Error_Red, "_ปิด excel ก่อน" + nameFG + excel.lastName + "\n");
                return;
            }
            lock (lockLoadTestSpec)
            {
                flagLoadTestSpec = true;
            }

            //display pupup please wait
            this.WindowState = FormWindowState.Minimized;
            Form form = new Form();
            ShowMessage(form, "Initial Process", "Please wait ...");

            Activator.CreateInstance(functionExcel, this, "LoadTestSpec()");
            CallScan2D();
            CallPipeServer();
            lock (lockLoadTestSpec)
            {
                flagLoadTestSpec = false;
            }
            LoadDiscription();

            //close popup please wait
            form.Close();
            this.WindowState = FormWindowState.Maximized;

            CreateFolder_datalog();
            enable_button_test_and_exit();
        }
        private void fMain_SizeChanged(object sender, EventArgs e)
        {
            fMain_sizechanged();
        }
        /// <summary>
        /// Flag for disable enable size changed
        /// </summary>
        private bool flagSizeChanged { get; set; }
        private void fMain_sizechanged()
        {
            if (!flagSizeChanged)
            {
                return;
            }
            GroupBox[] g = _groupBoxHeads;
            if (show_data_grid.Checked)
            {
                for (int i = 0; i < 36; i++) { g[i].Visible = false; }
                DataGridView gg = getDataGridView(show_datagrid_int);
                GroupBox gb = getGroupBoxHead(show_datagrid_int);

                gg.Visible = true;
                gb.Visible = true;
                gb.Location = new Point(groupBox_head1.Location.X, groupBox_head1.Location.Y);
                gb.Size = new Size(((groupBox1.Width) - 2), ((this.Height - 82) / 1));
                return;
            }
            else { for (int i = 0; i < 36; i++) { g[i].Visible = true; } }
            configTester.numHead = Convert.ToInt32(setupPay.read_text(ConfigTester.Header.numHead, configTester.nameFile));
            if (configTester.numHead > 20)
            {
                int Factor_groupbox = 0;
                int Factor_groupbox_2 = 0;
                int factor = 3;
                switch (configTester.numHead)
                {//ไอศครีมเชอเบร็ตรสมะนาว
                    case 21: Factor_groupbox = 112; Factor_groupbox_2 = 7; break;
                    case 22: Factor_groupbox = 116; Factor_groupbox_2 = 8; groupBox_head23.Visible = false; groupBox_head24.Visible = false; break;
                    case 23: Factor_groupbox = 116; Factor_groupbox_2 = 8; groupBox_head24.Visible = false; break;
                    case 24: Factor_groupbox = 116; Factor_groupbox_2 = 8; break;
                    case 25: Factor_groupbox = 120; Factor_groupbox_2 = 9; groupBox_head26.Visible = false; groupBox_head27.Visible = false; break;
                    case 26: Factor_groupbox = 120; Factor_groupbox_2 = 9; groupBox_head27.Visible = false; break;
                    case 27: Factor_groupbox = 120; Factor_groupbox_2 = 9; break;
                    case 28: Factor_groupbox = 124; Factor_groupbox_2 = 10; groupBox_head29.Visible = false; groupBox_head30.Visible = false; break;
                    case 29: Factor_groupbox = 124; Factor_groupbox_2 = 10; groupBox_head30.Visible = false; break;
                    case 30: Factor_groupbox = 124; Factor_groupbox_2 = 10; break;
                    case 31: Factor_groupbox = 118; Factor_groupbox_2 = 11; groupBox_head32.Visible = false; groupBox_head33.Visible = false; break;
                    case 32: Factor_groupbox = 118; Factor_groupbox_2 = 11; groupBox_head33.Visible = false; break;
                    case 33: Factor_groupbox = 118; Factor_groupbox_2 = 11; break;
                    case 34: Factor_groupbox = 122; Factor_groupbox_2 = 12; groupBox_head35.Visible = false; groupBox_head36.Visible = false; break;
                    case 35: Factor_groupbox = 122; Factor_groupbox_2 = 12; groupBox_head36.Visible = false; break;
                    case 36: Factor_groupbox = 122; Factor_groupbox_2 = 12; break;
                }
                groupBox_head1.Size = new Size(((groupBox1.Width / 3) - 2), ((this.Height - Factor_groupbox) / Factor_groupbox_2));
                groupBox_head2.Location = new Point(groupBox_head1.Location.X + groupBox_head1.Width + 5, groupBox_head2.Location.Y);
                groupBox_head2.Size = new Size(((groupBox1.Width / 3) - 2), ((this.Height - Factor_groupbox) / Factor_groupbox_2));
                groupBox_head3.Location = new Point(groupBox_head2.Location.X + groupBox_head2.Width + 5, groupBox_head2.Location.Y);
                groupBox_head3.Size = new Size(((groupBox1.Width / 3) - 2), ((this.Height - Factor_groupbox) / Factor_groupbox_2));
                groupBox_head4.Location = new Point(groupBox_head1.Location.X, groupBox_head1.Location.Y + groupBox_head1.Size.Height + factor);
                groupBox_head4.Size = new Size(groupBox_head1.Size.Width, groupBox_head1.Size.Height);
                groupBox_head5.Location = new Point(groupBox_head2.Location.X, groupBox_head2.Location.Y + groupBox_head2.Size.Height + factor);
                groupBox_head5.Size = new Size(groupBox_head2.Size.Width, groupBox_head2.Size.Height);
                groupBox_head6.Location = new Point(groupBox_head3.Location.X, groupBox_head3.Location.Y + groupBox_head3.Size.Height + factor);
                groupBox_head6.Size = new Size(groupBox_head3.Size.Width, groupBox_head3.Size.Height);
                groupBox_head7.Location = new Point(groupBox_head4.Location.X, groupBox_head4.Location.Y + groupBox_head4.Size.Height + factor);
                groupBox_head7.Size = new Size(groupBox_head4.Size.Width, groupBox_head4.Size.Height);
                groupBox_head8.Location = new Point(groupBox_head5.Location.X, groupBox_head5.Location.Y + groupBox_head5.Size.Height + factor);
                groupBox_head8.Size = new Size(groupBox_head5.Size.Width, groupBox_head5.Size.Height);
                groupBox_head9.Location = new Point(groupBox_head6.Location.X, groupBox_head6.Location.Y + groupBox_head6.Size.Height + factor);
                groupBox_head9.Size = new Size(groupBox_head6.Size.Width, groupBox_head6.Size.Height);
                groupBox_head10.Location = new Point(groupBox_head7.Location.X, groupBox_head7.Location.Y + groupBox_head7.Size.Height + factor);
                groupBox_head10.Size = new Size(groupBox_head7.Size.Width, groupBox_head7.Size.Height);
                groupBox_head11.Location = new Point(groupBox_head8.Location.X, groupBox_head8.Location.Y + groupBox_head8.Size.Height + factor);
                groupBox_head11.Size = new Size(groupBox_head8.Size.Width, groupBox_head8.Size.Height);
                groupBox_head12.Location = new Point(groupBox_head9.Location.X, groupBox_head9.Location.Y + groupBox_head9.Size.Height + factor);
                groupBox_head12.Size = new Size(groupBox_head9.Size.Width, groupBox_head9.Size.Height);
                groupBox_head13.Location = new Point(groupBox_head10.Location.X, groupBox_head10.Location.Y + groupBox_head10.Size.Height + factor);
                groupBox_head13.Size = new Size(groupBox_head10.Size.Width, groupBox_head10.Size.Height);
                groupBox_head14.Location = new Point(groupBox_head11.Location.X, groupBox_head11.Location.Y + groupBox_head11.Size.Height + factor);
                groupBox_head14.Size = new Size(groupBox_head11.Size.Width, groupBox_head11.Size.Height);
                groupBox_head15.Location = new Point(groupBox_head12.Location.X, groupBox_head12.Location.Y + groupBox_head12.Size.Height + factor);
                groupBox_head15.Size = new Size(groupBox_head12.Size.Width, groupBox_head12.Size.Height);
                groupBox_head16.Location = new Point(groupBox_head13.Location.X, groupBox_head13.Location.Y + groupBox_head13.Size.Height + factor);
                groupBox_head16.Size = new Size(groupBox_head13.Size.Width, groupBox_head13.Size.Height);
                groupBox_head17.Location = new Point(groupBox_head14.Location.X, groupBox_head14.Location.Y + groupBox_head14.Size.Height + factor);
                groupBox_head17.Size = new Size(groupBox_head14.Size.Width, groupBox_head14.Size.Height);
                groupBox_head18.Location = new Point(groupBox_head15.Location.X, groupBox_head15.Location.Y + groupBox_head15.Size.Height + factor);
                groupBox_head18.Size = new Size(groupBox_head15.Size.Width, groupBox_head15.Size.Height);
                groupBox_head19.Location = new Point(groupBox_head16.Location.X, groupBox_head16.Location.Y + groupBox_head16.Size.Height + factor);
                groupBox_head19.Size = new Size(groupBox_head16.Size.Width, groupBox_head16.Size.Height);
                groupBox_head20.Location = new Point(groupBox_head17.Location.X, groupBox_head17.Location.Y + groupBox_head17.Size.Height + factor);
                groupBox_head20.Size = new Size(groupBox_head17.Size.Width, groupBox_head17.Size.Height);
                groupBox_head21.Location = new Point(groupBox_head18.Location.X, groupBox_head18.Location.Y + groupBox_head18.Size.Height + factor);
                groupBox_head21.Size = new Size(groupBox_head18.Size.Width, groupBox_head18.Size.Height);
                groupBox_head22.Location = new Point(groupBox_head19.Location.X, groupBox_head19.Location.Y + groupBox_head19.Size.Height + factor);
                groupBox_head22.Size = new Size(groupBox_head19.Size.Width, groupBox_head19.Size.Height);
                groupBox_head23.Location = new Point(groupBox_head20.Location.X, groupBox_head20.Location.Y + groupBox_head20.Size.Height + factor);
                groupBox_head23.Size = new Size(groupBox_head20.Size.Width, groupBox_head20.Size.Height);
                groupBox_head24.Location = new Point(groupBox_head21.Location.X, groupBox_head21.Location.Y + groupBox_head21.Size.Height + factor);
                groupBox_head24.Size = new Size(groupBox_head21.Size.Width, groupBox_head21.Size.Height);
                groupBox_head25.Location = new Point(groupBox_head22.Location.X, groupBox_head22.Location.Y + groupBox_head22.Size.Height + factor);
                groupBox_head25.Size = new Size(groupBox_head22.Size.Width, groupBox_head22.Size.Height);
                groupBox_head26.Location = new Point(groupBox_head23.Location.X, groupBox_head23.Location.Y + groupBox_head23.Size.Height + factor);
                groupBox_head26.Size = new Size(groupBox_head23.Size.Width, groupBox_head23.Size.Height);
                groupBox_head27.Location = new Point(groupBox_head24.Location.X, groupBox_head24.Location.Y + groupBox_head24.Size.Height + factor);
                groupBox_head27.Size = new Size(groupBox_head24.Size.Width, groupBox_head24.Size.Height);
                groupBox_head28.Location = new Point(groupBox_head25.Location.X, groupBox_head25.Location.Y + groupBox_head25.Size.Height + factor);
                groupBox_head28.Size = new Size(groupBox_head25.Size.Width, groupBox_head25.Size.Height);
                groupBox_head29.Location = new Point(groupBox_head26.Location.X, groupBox_head26.Location.Y + groupBox_head26.Size.Height + factor);
                groupBox_head29.Size = new Size(groupBox_head26.Size.Width, groupBox_head26.Size.Height);
                groupBox_head30.Location = new Point(groupBox_head27.Location.X, groupBox_head27.Location.Y + groupBox_head27.Size.Height + factor);
                groupBox_head30.Size = new Size(groupBox_head27.Size.Width, groupBox_head27.Size.Height);
                groupBox_head31.Location = new Point(groupBox_head28.Location.X, groupBox_head28.Location.Y + groupBox_head28.Size.Height + factor);
                groupBox_head31.Size = new Size(groupBox_head28.Size.Width, groupBox_head28.Size.Height);
                groupBox_head32.Location = new Point(groupBox_head29.Location.X, groupBox_head29.Location.Y + groupBox_head29.Size.Height + factor);
                groupBox_head32.Size = new Size(groupBox_head29.Size.Width, groupBox_head29.Size.Height);
                groupBox_head33.Location = new Point(groupBox_head30.Location.X, groupBox_head30.Location.Y + groupBox_head30.Size.Height + factor);
                groupBox_head33.Size = new Size(groupBox_head30.Size.Width, groupBox_head30.Size.Height);
                groupBox_head34.Location = new Point(groupBox_head31.Location.X, groupBox_head31.Location.Y + groupBox_head31.Size.Height + factor);
                groupBox_head34.Size = new Size(groupBox_head31.Size.Width, groupBox_head31.Size.Height);
                groupBox_head35.Location = new Point(groupBox_head32.Location.X, groupBox_head32.Location.Y + groupBox_head32.Size.Height + factor);
                groupBox_head35.Size = new Size(groupBox_head32.Size.Width, groupBox_head32.Size.Height);
                groupBox_head36.Location = new Point(groupBox_head33.Location.X, groupBox_head33.Location.Y + groupBox_head33.Size.Height + factor);
                groupBox_head36.Size = new Size(groupBox_head33.Size.Width, groupBox_head33.Size.Height);
                for (int i = configTester.numHead; i < 36; i++) { g[i].Visible = false; }
            }
            else
            {
                int factor_groupbox = 0;
                int factor_groupbox_2 = 0;
                switch (configTester.numHead)
                {//ไอศครีมเชอเบร็ตรสมะนาว
                    case 1: factor_groupbox = 82; factor_groupbox_2 = 1; groupBox_head2.Visible = false; break;
                    case 2: factor_groupbox = 82; factor_groupbox_2 = 1; break;
                    case 3: factor_groupbox = 88; factor_groupbox_2 = 2; groupBox_head4.Visible = false; break;
                    case 4: factor_groupbox = 88; factor_groupbox_2 = 2; break;
                    case 5: factor_groupbox = 92; factor_groupbox_2 = 3; groupBox_head6.Visible = false; break;
                    case 6: factor_groupbox = 92; factor_groupbox_2 = 3; break;
                    case 7: factor_groupbox = 96; factor_groupbox_2 = 4; groupBox_head8.Visible = false; break;
                    case 8: factor_groupbox = 96; factor_groupbox_2 = 4; break;
                    case 9: factor_groupbox = 104; factor_groupbox_2 = 5; groupBox_head10.Visible = false; break;
                    case 10: factor_groupbox = 104; factor_groupbox_2 = 5; break;
                    case 11: factor_groupbox = 108; factor_groupbox_2 = 6; groupBox_head12.Visible = false; break;
                    case 12: factor_groupbox = 108; factor_groupbox_2 = 6; break;
                    case 13: factor_groupbox = 112; factor_groupbox_2 = 7; groupBox_head14.Visible = false; break;
                    case 14: factor_groupbox = 112; factor_groupbox_2 = 7; break;
                    case 15: factor_groupbox = 116; factor_groupbox_2 = 8; groupBox_head16.Visible = false; break;
                    case 16: factor_groupbox = 116; factor_groupbox_2 = 8; break;
                    case 17: factor_groupbox = 120; factor_groupbox_2 = 9; groupBox_head18.Visible = false; break;
                    case 18: factor_groupbox = 120; factor_groupbox_2 = 9; break;
                    case 19: factor_groupbox = 124; factor_groupbox_2 = 10; groupBox_head20.Visible = false; break;
                    case 20: factor_groupbox = 124; factor_groupbox_2 = 10; break;
                }
                for (int i = 20; i < 36; i++) { g[i].Visible = false; }
                if (configTester.numHead == 1) groupBox_head1.Size = new Size(((groupBox1.Width) - 2), ((this.Height - factor_groupbox) / factor_groupbox_2));
                else groupBox_head1.Size = new Size(((groupBox1.Width / 2) - 2), ((this.Height - factor_groupbox) / factor_groupbox_2));
                groupBox_head2.Size = new Size(((groupBox1.Width / 2) - 2), ((this.Height - factor_groupbox) / factor_groupbox_2));
                groupBox_head2.Location = new Point(groupBox_head1.Location.X + groupBox_head1.Width + 5, groupBox_head2.Location.Y);
                groupBox_head3.Location = new Point(groupBox_head1.Location.X, groupBox_head1.Location.Y + groupBox_head1.Size.Height + 5);
                groupBox_head3.Size = new Size(groupBox_head1.Size.Width, groupBox_head1.Size.Height);
                groupBox_head4.Location = new Point(groupBox_head2.Location.X, groupBox_head2.Location.Y + groupBox_head2.Size.Height + 5);
                groupBox_head4.Size = new Size(groupBox_head2.Size.Width, groupBox_head2.Size.Height);
                groupBox_head5.Location = new Point(groupBox_head3.Location.X, groupBox_head3.Location.Y + groupBox_head3.Size.Height + 5);
                groupBox_head5.Size = new Size(groupBox_head3.Size.Width, groupBox_head3.Size.Height);
                groupBox_head6.Location = new Point(groupBox_head4.Location.X, groupBox_head4.Location.Y + groupBox_head4.Size.Height + 5);
                groupBox_head6.Size = new Size(groupBox_head4.Size.Width, groupBox_head4.Size.Height);
                groupBox_head7.Location = new Point(groupBox_head5.Location.X, groupBox_head5.Location.Y + groupBox_head5.Size.Height + 5);
                groupBox_head7.Size = new Size(groupBox_head5.Size.Width, groupBox_head5.Size.Height);
                groupBox_head8.Location = new Point(groupBox_head6.Location.X, groupBox_head6.Location.Y + groupBox_head6.Size.Height + 5);
                groupBox_head8.Size = new Size(groupBox_head6.Size.Width, groupBox_head6.Size.Height);
                groupBox_head9.Location = new Point(groupBox_head7.Location.X, groupBox_head7.Location.Y + groupBox_head7.Size.Height + 5);
                groupBox_head9.Size = new Size(groupBox_head7.Size.Width, groupBox_head7.Size.Height);
                groupBox_head10.Location = new Point(groupBox_head8.Location.X, groupBox_head8.Location.Y + groupBox_head8.Size.Height + 5);
                groupBox_head10.Size = new Size(groupBox_head8.Size.Width, groupBox_head8.Size.Height);
                groupBox_head11.Location = new Point(groupBox_head9.Location.X, groupBox_head9.Location.Y + groupBox_head9.Size.Height + 5);
                groupBox_head11.Size = new Size(groupBox_head9.Size.Width, groupBox_head9.Size.Height);
                groupBox_head12.Location = new Point(groupBox_head10.Location.X, groupBox_head10.Location.Y + groupBox_head10.Size.Height + 5);
                groupBox_head12.Size = new Size(groupBox_head10.Size.Width, groupBox_head10.Size.Height);
                groupBox_head13.Location = new Point(groupBox_head11.Location.X, groupBox_head11.Location.Y + groupBox_head11.Size.Height + 5);
                groupBox_head13.Size = new Size(groupBox_head11.Size.Width, groupBox_head11.Size.Height);
                groupBox_head14.Location = new Point(groupBox_head12.Location.X, groupBox_head12.Location.Y + groupBox_head12.Size.Height + 5);
                groupBox_head14.Size = new Size(groupBox_head12.Size.Width, groupBox_head12.Size.Height);
                groupBox_head15.Location = new Point(groupBox_head13.Location.X, groupBox_head13.Location.Y + groupBox_head13.Size.Height + 5);
                groupBox_head15.Size = new Size(groupBox_head13.Size.Width, groupBox_head13.Size.Height);
                groupBox_head16.Location = new Point(groupBox_head14.Location.X, groupBox_head14.Location.Y + groupBox_head14.Size.Height + 5);
                groupBox_head16.Size = new Size(groupBox_head14.Size.Width, groupBox_head14.Size.Height);
                groupBox_head17.Location = new Point(groupBox_head15.Location.X, groupBox_head15.Location.Y + groupBox_head15.Size.Height + 5);
                groupBox_head17.Size = new Size(groupBox_head15.Size.Width, groupBox_head15.Size.Height);
                groupBox_head18.Location = new Point(groupBox_head16.Location.X, groupBox_head16.Location.Y + groupBox_head16.Size.Height + 5);
                groupBox_head18.Size = new Size(groupBox_head16.Size.Width, groupBox_head16.Size.Height);
                groupBox_head19.Location = new Point(groupBox_head17.Location.X, groupBox_head17.Location.Y + groupBox_head17.Size.Height + 5);
                groupBox_head19.Size = new Size(groupBox_head17.Size.Width, groupBox_head17.Size.Height);
                groupBox_head20.Location = new Point(groupBox_head18.Location.X, groupBox_head18.Location.Y + groupBox_head18.Size.Height + 5);
                groupBox_head20.Size = new Size(groupBox_head18.Size.Width, groupBox_head18.Size.Height);
            }
            if (!configUI.showDataGrid)
            {
                dataGridView_1.Visible = false; dataGridView_2.Visible = false;
                dataGridView_3.Visible = false; dataGridView_4.Visible = false;
                dataGridView_5.Visible = false; dataGridView_6.Visible = false;
                dataGridView_7.Visible = false; dataGridView_8.Visible = false;
                dataGridView_9.Visible = false; dataGridView_10.Visible = false;
                dataGridView_11.Visible = false; dataGridView_12.Visible = false;
                dataGridView_13.Visible = false; dataGridView_14.Visible = false;
                dataGridView_15.Visible = false; dataGridView_16.Visible = false;
                dataGridView_17.Visible = false; dataGridView_18.Visible = false;
                dataGridView_19.Visible = false; dataGridView_20.Visible = false;
                dataGridView_21.Visible = false; dataGridView_22.Visible = false;
                dataGridView_23.Visible = false; dataGridView_24.Visible = false;
                dataGridView_25.Visible = false; dataGridView_26.Visible = false;
                dataGridView_27.Visible = false; dataGridView_28.Visible = false;
                dataGridView_29.Visible = false; dataGridView_30.Visible = false;
                dataGridView_31.Visible = false; dataGridView_32.Visible = false;
                dataGridView_33.Visible = false; dataGridView_34.Visible = false;
                dataGridView_35.Visible = false; dataGridView_36.Visible = false;
                if (_labelProgressInfos != null)
                {
                    for (int i = 0; i < configTester.numHead; i++) _labelProgressInfos[i].Visible = true;
                    for (int i = configTester.numHead; i < 36; i++) _labelProgressInfos[i].Visible = false;
                }
            }
            else
            {
                if (_labelProgressInfos != null)
                    for (int i = 0; i < 36; i++) _labelProgressInfos[i].Visible = false;
            }
        }
        private int show_datagrid_int = 1;
        private void status_1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 1;
        }
        private void status_2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 2;
        }
        private void status_3_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 3;
        }
        private void status_4_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 4;
        }
        private void status_5_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 5;
        }
        private void status_6_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 6;
        }
        private void status_7_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 7;
        }
        private void status_8_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 8;
        }
        private void status_9_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 9;
        }
        private void status_10_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 10;
        }
        private void status_11_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 11;
        }
        private void status_12_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 12;
        }
        private void status_13_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 13;
        }
        private void status_14_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 14;
        }
        private void status_15_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 15;
        }
        private void status_16_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 16;
        }
        private void status_17_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 17;
        }
        private void status_18_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 18;
        }
        private void status_19_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 19;
        }
        private void status_20_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 20;
        }
        private void status_21_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 21;
        }
        private void status_22_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 22;
        }
        private void status_23_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 23;
        }
        private void status_24_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 24;
        }
        private void status_25_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 25;
        }
        private void status_26_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 26;
        }
        private void status_27_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 27;
        }
        private void status_28_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 28;
        }
        private void status_29_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 29;
        }
        private void status_30_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 30;
        }
        private void status_31_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 31;
        }
        private void status_32_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 32;
        }
        private void status_33_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 33;
        }
        private void status_34_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 34;
        }
        private void status_35_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 35;
        }
        private void status_36_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            show_datagrid_int = 36;
        }
        int save_datagrit_excel;
        private void dataGridView_1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 1;
        }
        private void dataGridView_2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 2;
        }
        private void dataGridView_3_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 3;
        }
        private void dataGridView_4_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 4;
        }
        private void dataGridView_5_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 5;
        }
        private void dataGridView_6_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 6;
        }
        private void dataGridView_7_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 7;
        }
        private void dataGridView_8_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 8;
        }
        private void dataGridView_9_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 9;
        }
        private void dataGridView_10_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 10;
        }
        private void dataGridView_11_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 11;
        }
        private void dataGridView_12_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 12;
        }
        private void dataGridView_13_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 13;
        }
        private void dataGridView_14_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 14;
        }
        private void dataGridView_15_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 15;
        }
        private void dataGridView_16_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 16;
        }
        private void dataGridView_17_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 17;
        }
        private void dataGridView_18_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 18;
        }
        private void dataGridView_19_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 19;
        }
        private void dataGridView_20_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 20;
        }
        private void dataGridView_21_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 21;
        }
        private void dataGridView_22_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 22;
        }
        private void dataGridView_23_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 23;
        }
        private void dataGridView_24_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 24;
        }
        private void dataGridView_25_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 25;
        }
        private void dataGridView_26_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 26;
        }
        private void dataGridView_27_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 27;
        }
        private void dataGridView_28_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 28;
        }
        private void dataGridView_29_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 29;
        }
        private void dataGridView_30_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 30;
        }
        private void dataGridView_31_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 31;
        }
        private void dataGridView_32_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 32;
        }
        private void dataGridView_33_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 33;
        }
        private void dataGridView_34_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 34;
        }
        private void dataGridView_35_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 35;
        }
        private void dataGridView_36_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            save_datagrit_excel = 36;
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.csv)|*.csv";
            sfd.FileName = "export.csv";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            StreamWriter swOut = new StreamWriter(sfd.FileName, true);
            DataGridView data = getDataGridView(save_datagrit_excel);

            for (int i = 0; i < data.RowCount; i++)
            {
                string str = "";
                for (int j = 0; j < data.ColumnCount; j++)
                {
                    str += data.Rows[i].Cells[j].Value + ",";  // "\t" สำหรับ excel และ "," สำหรับ csv 
                }
                swOut.WriteLine(str);
            }
            swOut.Close();
        }
        private void ctms_showCmd_Click(object sender, EventArgs e)
        {
            configTester.showCMD = ctms_showCmd.Checked;
            setupPay.write_text(ConfigTester.Header.showCMD, configTester.showCMD.ToString().ToUpper(), configTester.nameFile);
        }
        private void prism_retest_text_pass_Click(object sender, EventArgs e)
        {
            string asd = "";
            while (true)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("_ใส่ข้อความที่ prism pass", "prism pass", prism_retest_text_pass.Text, 500, 300);
                if (input == "") return;
                asd = input;
                break;
            }
            prism_retest_text_pass.Text = asd;
            File.WriteAllText("../../config/prism_retest_text_pass.txt", asd);
        }
        private void prism_retest_text_fail_Click(object sender, EventArgs e)
        {
            string asd = "";

            while (true)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("_ใส่ข้อความที่ prism fail", "prism fail",
                    prism_retest_text_fail.Text, 500, 300);
                if (input == "") return;
                asd = input;
                break;
            }

            prism_retest_text_fail.Text = asd;
            File.WriteAllText("../../config/prism_retest_text_fail.txt", asd);
        }
        private void ctms_excel_saveFile_sup()
        {
            if (configTester.selectExcel)
            {
                excel.lastName = LastNameExcel.excel;
            }

            if (configTester.selectLibre)
            {
                excel.lastName = LastNameExcel.libre;
            }
        }
        private void settingDataGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MiniForm.FormSetDataDrid formDird = MiniForm.FormSetDataDrid.GetInstance(this);
            formDird.Show();
        }

        //support type one
        private void TmTypeone_Tick(object sender, EventArgs e)
        {
            tmTypeone.Stop();
            start_all_head();
        }
        #endregion
    }
}
