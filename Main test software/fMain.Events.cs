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
        #region ========================================================== Control Event ===========================================================
        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            Process[] pname = Process.GetProcessesByName("ATS");
            if (pname.Length == 2)
            {
                MessageBox.Show("_โปรแกรมนี้ เปิดใช้งานอยู่");
                Application.Exit();
                return;
            }

            Console.WriteLine("Set all propertise");
            set_default();

            Console.WriteLine("Process fingerprint scanner");
            if (configUpData.callExeFingerPrint)
            {
                string employeeID = string.Empty;
                if (!GetUserIdByFingerPrint(ref employeeID))
                {
                    if (String.IsNullOrEmpty(employeeID))
                    {
                        //if close program finger print with scan finger not success
                        setupPay.write_text(ConfigPrism.Header.mode, configPrism.mode, configPrism.nameFile);
                        File.WriteAllText("FingerPrint_EmployeeID.txt", employeeID);
                        this.Close();
                        return;
                    }
                    else
                    {
                        // for display popup key employee id in exe updata
                        //if login by design
                        File.Delete("FingerPrint_EmployeeID.txt");
                    }
                }
                else
                {
                    //if scan finger success
                    setupPay.write_text(ConfigPrism.Header.mode, configPrism.mode, configPrism.nameFile);
                    setupPay.write_text(ConfigPrism.Header.employeeID, employeeID, configPrism.nameFile);
                    File.WriteAllText("FingerPrint_EmployeeID.txt", employeeID);
                }

                //set properties after get mode from exe finger print
                if (configPrism.mode == configPrism.Debug)
                {
                    cb_OperationMode.Checked = false;
                    cb_DebugMode.Checked = true;
                }
                else
                {
                    cb_OperationMode.Checked = true;
                    cb_DebugMode.Checked = false;
                }
            }
            else
            {//clear file if not use finger print
                File.Delete("FingerPrint_EmployeeID.txt");
            }

            Console.WriteLine("Call program up data prism");
            CallUpDataExe();

            Console.WriteLine("Specify program up data prism read all config again");
            File.WriteAllText(configUpData.reReadConfig, string.Empty);

            //display popup key employee id of exe updata
            //if mode is operation
            //if mode is debug and use finger print
            if (configPrism.mode == configPrism.Operation ||
               (configPrism.mode == configPrism.Debug && configUpData.callExeFingerPrint))
            {
                this.WindowState = FormWindowState.Minimized;
                File.Delete("up_data_login_ok.txt");
                File.Delete("up_data_login_fail.txt");
                File.WriteAllText("up_data_login.txt", "");
                while (true)
                {
                    try
                    {
                        File.ReadAllText("up_data_login_ok.txt");
                        File.Delete("up_data_login_ok.txt");
                        break;
                    }
                    catch { }
                    try
                    {
                        File.ReadAllText("up_data_login_fail.txt");
                        File.Delete("up_data_login_fail.txt");
                        Application.Exit();
                        return;
                    }
                    catch { }
                    Thread.Sleep(100);
                }
                this.WindowState = FormWindowState.Maximized;
            }
            Cursor.Hide();
            CheckPRISMStatus();
            connect_relay(true);
            background_time.RunWorkerAsync();
            DirectoryInfo lish_excel = new DirectoryInfo("../../TestDescription");
            if (configTester.selectExcel)
            {
                FileInfo[] Files = lish_excel.GetFiles("*.xlsx");
                foreach (FileInfo file in Files)
                {
                    if (file.Name.Contains("~")) continue;
                    cbb_fg.Items.Add(file.Name.Replace(file.Extension, ""));
                }
            }
            if (configTester.selectLibre)
            {
                FileInfo[] Files = lish_excel.GetFiles("*.ods");
                foreach (FileInfo file in Files)
                {
                    if (file.Name.Contains("~")) continue;
                    cbb_fg.Items.Add(file.Name.Replace(file.Extension, ""));
                }
            }
            this.Activate();
            tb_wo.Focus();
            Activator.CreateInstance(functionExcel, this, "setup()");
            Cursor.Show();

            this.WindowState = FormWindowState.Maximized;
            flagSizeChanged = true;
            fMain_sizechanged();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (configTester.useRelayCard) OffAllRelay();
            flag_this_close = true;
            if (cbb_fg.Text != "") Activator.CreateInstance(functionExcel, this, "OffAllFunction()");
            background_time.Dispose();
            background_relay.Dispose();
            tcpip.Dispose();
            for (int i = 1; i <= configTester.numHead; i++)
            {
                File.Delete("../../config/test_head_" + i + "_debug.txt");
            }
            CloseUpDataExe();
            ClosePipeServer();
            CloseScan2D();
            Application.Exit();
            Application.ExitThread();
            Environment.Exit(0);
        }
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            return;
            ///If from size less than 1155, 625. Program will auto adjust to 1155, 625
            ///and set it to center position of screen
            if ((this.Size.Width < 1155) | (this.Size.Height < 625))
            {
                this.Width = 1155;
                this.Height = 625;

                Screen screen = Screen.FromControl(this);
                Rectangle workingArea = screen.WorkingArea;
                this.Location = new Point()
                {
                    X = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - this.Width) / 2),
                    Y = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - this.Height) / 2)
                };
            }
        }
        private void btnTEST_Click(object sender, EventArgs e)
        {
            Button btnTest = (Button)sender;
            int index = int.Parse(btnTest.Name.Substring("btnTEST_".Length));

            if (flag_head[index - 1] != true)
                return;

            btnTest.Enabled = false;

            row_test[index - 1] = 0;
            flag_test[index - 1] = true;
            GlobalTestingFlag[index - 1] = true;

            Label statusLabel = (Label)Controls.Find("status_" + (index), true)[0];
            statusLabel.Text = "TESTING";
            if (configUI.lbStatus == ConfigUI.LbStatus.ForeColor)
            {
                statusLabel.ForeColor = Color.Blue;
            }
            else
            {
                statusLabel.BackColor = Color.Blue;
                statusLabel.ForeColor = Color.White;
            }
        }
        private void btnTEST_1_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_2_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_3_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_4_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_5_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_6_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_7_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_8_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_9_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_10_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_11_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_12_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_13_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_14_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_15_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_16_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_17_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_18_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_19_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_20_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_21_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_22_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_23_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_24_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_25_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_26_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_27_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_28_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_29_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_30_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_31_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_32_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_33_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_34_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_35_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnTEST_36_Click(object sender, EventArgs e)
        {
            btnTEST_Click(sender, e);
        }
        private void btnEXIT_Click(object sender, EventArgs e)
        {
            Button btnTest = (Button)sender;
            int index = int.Parse(btnTest.Name.Substring("btnEXIT_".Length));

            GlobalTestingFlag[index - 1] = false;
            btnTest.Enabled = true;

            Label statusLabel = (Label)Controls.Find("status_" + (index), true)[0];
            statusLabel.Text = "FAIL";
            if (configUI.lbStatus == ConfigUI.LbStatus.ForeColor)
            {
                statusLabel.ForeColor = Color.Red;
            }
            else
            {
                statusLabel.BackColor = Color.Red;
                statusLabel.ForeColor = Color.White;
            }
        }
        private void btnEXIT_1_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_2_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_3_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_4_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_5_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_6_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_7_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_8_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_9_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_10_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_11_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_12_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_13_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_14_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_15_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_16_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_17_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_18_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_19_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_20_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_21_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_22_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_23_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_24_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_25_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_26_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_27_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_28_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_29_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_30_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_31_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_32_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_33_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_34_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_35_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void btnEXIT_36_Click(object sender, EventArgs e)
        {
            btnEXIT_Click(sender, e);
        }
        private void autoTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setupPay.write_text(ConfigTester.Header.testAuto, autoTestToolStripMenuItem.Checked.ToString().ToUpper(), configTester.nameFile);
            setupPay.setup();
            if (autoTestToolStripMenuItem.Checked != true) return;
            start_all_head();
        }
        private void simulateTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _bypassInputActive = true;
            for (int i = 0; i < configTester.numHead; i++)
                flag_head[i] = true;
            start_all_head();
        }
        public void start_all_head()
        {
            bool[] sup1 = flag_test;
            bool[] sup2 = flag_head;
            if (!configTester.useRelayCard)
            {
                for (int i = 0; i < configTester.numHead; i++)
                {
                    sup2[i] = true;
                }
            }//ไอศครีมเชอเบร็ตรสมะนาว
            Label[] l = { status_1, status_2, status_3, status_4, status_5, status_6, status_7, status_8, status_9, status_10,
                          status_11, status_12, status_13, status_14, status_15, status_16, status_17, status_18, status_19, status_20,
                          status_21, status_22, status_23, status_24, status_25, status_26, status_27, status_28, status_29, status_30,
                          status_31, status_32, status_33, status_34, status_35, status_36};
            Button[] b = { btnTEST_1, btnTEST_2, btnTEST_3, btnTEST_4, btnTEST_5, btnTEST_6, btnTEST_7, btnTEST_8, btnTEST_9, btnTEST_10,
                           btnTEST_11, btnTEST_12, btnTEST_13, btnTEST_14, btnTEST_15, btnTEST_16, btnTEST_17, btnTEST_18, btnTEST_19, btnTEST_20,
                           btnTEST_21, btnTEST_22, btnTEST_23, btnTEST_24, btnTEST_25, btnTEST_26, btnTEST_27, btnTEST_28, btnTEST_29, btnTEST_30,
                           btnTEST_31, btnTEST_32, btnTEST_33, btnTEST_34, btnTEST_35, btnTEST_36};
            for (int i = 0; i < configTester.numHead; i++)
            {
                if (!sup1[i] && sup2[i])
                {
                    b[i].Enabled = false;
                    row_test[i] = 0;
                    flag_test[i] = true;
                    GlobalTestingFlag[i] = true;
                    l[i].Text = "TESTING";
                    if (configUI.lbStatus == ConfigUI.LbStatus.ForeColor)
                    {
                        l[i].ForeColor = Color.Blue;
                    }
                    else
                    {
                        l[i].BackColor = Color.Blue;
                        l[i].ForeColor = Color.White;
                    }
                }
            }
        }
        private Stopwatch[] timer_head = { new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(),
                                           new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(),
                                           new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(),
                                           new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(),
                                           new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(),
                                           new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(),
                                           new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(),
                                           new Stopwatch()};
        private void background_time_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {//ไอศครีมเชอเบร็ตรสมะนาว
            bool[] Status = { flag_test[0], flag_test[1], flag_test[2], flag_test[3], flag_test[4],
                              flag_test[5], flag_test[6], flag_test[7], flag_test[8], flag_test[9],
                              flag_test[10], flag_test[11], flag_test[12], flag_test[13], flag_test[14],
                              flag_test[15], flag_test[16], flag_test[17], flag_test[18], flag_test[19],
                              flag_test[20], flag_test[21], flag_test[22], flag_test[23], flag_test[24],
                              flag_test[25], flag_test[26], flag_test[27], flag_test[28], flag_test[29],
                              flag_test[30], flag_test[31], flag_test[32], flag_test[33], flag_test[34],
                              flag_test[35] };
            for (int i = 0; i < configTester.numHead; i++)
            {
                timer_head[i].Restart();
            }
            bool flag_brink = false;
            bool status_all = false;
            bool status_all_current = false;
            while (true)
            {
                if (flag_this_close)
                    break;
                for (int i = 0; i < configTester.numHead; i++)
                {
                    if (Status[i] != flag_test[i])
                    {
                        Status[i] = flag_test[i];
                        timer_head[i].Restart();
                    }
                }
                background_time.ReportProgress(0);
                Thread.Sleep(250);
                if (flag_brink)
                {
                    background_time.ReportProgress(1);
                    flag_brink = false;
                }
                else
                    flag_brink = true;

                status_all_current = false;
                for (int i = 0; i < configTester.numHead; i++)
                {
                    if (flag_test[i])
                        status_all_current = true;
                }
                if (status_all != status_all_current)
                {
                    status_all = status_all_current;
                    if (status_all)
                        background_time.ReportProgress(100);
                    else
                    { background_time.ReportProgress(101); }
                }

                if (autoTestToolStripMenuItem.Checked)
                {
                    try
                    {
                        File.ReadAllText("auto_test_trick.txt");
                        File.Delete("auto_test_trick.txt");
                        background_time.ReportProgress(9);
                    }
                    catch { }
                }
            }
        }
        private void background_time_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {

                for (int i = 1; i <= configTester.numHead; i++)
                {
                    Label l_testtime = getLabelTestTime(i);
                    Label l_inout = getLabelInOutTime(i);

                    if (flag_test[i - 1])
                    {
                        l_testtime.Text = (timer_head[i - 1].ElapsedMilliseconds / 1000).ToString("0000");
                    }
                    else
                    {
                        l_inout.Text = (timer_head[i - 1].ElapsedMilliseconds / 1000).ToString("0000");
                    }
                    DateTime now = DateTime.Now;
                    if (lb_time.Text != now.ToString("T"))
                    {
                        lb_time.Text = now.ToString("T");
                        lb_date.Text = now.ToString("d");
                    }
                }
            }
            else if (e.ProgressPercentage == 1)
            {

                for (int i = 1; i <= configTester.numHead; i++)
                {
                    Label l = getLabelStatus(i);

                    if (flag_test[i - 1])
                        l.Visible = !l.Visible;
                    else
                        l.Visible = true;
                }
            }
            if (e.ProgressPercentage == 100)
                Activator.CreateInstance(functionExcel, this, "OnAllFunction()");
            if (e.ProgressPercentage == 101)
                Activator.CreateInstance(functionExcel, this, "OffAllFunction()");
            if (e.ProgressPercentage == 9)
                start_all_head();
        }

        private void LoopCheckInputIO()
        {
            bool clearScaner = false;
            bool statusInput = false;
            while (true)
            {
                lock (lockRS485)
                {
                    statusInput = rs485.Read.ReadChannel(1, 1); // bit 1 triggers all heads
                }

                // Auto-deactivate bypass once all heads finish testing
                if (_bypassInputActive)
                {
                    bool anyTesting = false;
                    for (int i = 0; i < configTester.numHead; i++)
                        if (flag_test[i]) { anyTesting = true; break; }
                    if (!anyTesting)
                        _bypassInputActive = false;
                }

                bool effectiveInput = statusInput || _bypassInputActive;

                if (effectiveInput)
                {
                    for (int head = 1; head <= configTester.numHead; head++)
                        background_relay.ReportProgress(Convert.ToInt32("10" + head));
                    clearScaner = true;
                }
                else
                {
                    for (int head = 1; head <= configTester.numHead; head++)
                        background_relay.ReportProgress(Convert.ToInt32("11" + head));
                    if (clearScaner)
                    {
                        clearScaner = false;
                        for (int head = 1; head <= configTester.numHead; head++)
                            File.WriteAllText("dryice_scan2d_clear_sn_" + head + ".txt", "");
                    }
                }

                Thread.Sleep(5000);
            }
        }

        private string namePipe = "PipeGeoBlue";
        private bool[] SensorUnit = new bool[] { false, false, false, false };
        private bool[] SensorPlace = new bool[] { false, false, false, false };
        private void GetSensor()
        {
            try
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", namePipe, PipeDirection.InOut))
                {
                    pipeClient.Connect(2000); // ตั้ง timeout 2 วินาทีสำหรับการเชื่อมต่อ
                    using (StreamReader reader = new StreamReader(pipeClient))
                    using (StreamWriter writer = new StreamWriter(pipeClient) { AutoFlush = true })
                    {
                        JsonFormat jsonFormat = new JsonFormat();
                        DateTime currentDate = DateTime.Now;
                        jsonFormat.Date = currentDate.ToString("dd/MM/yyyy");
                        jsonFormat.Time = currentDate.ToString("HH:mm:ss");
                        jsonFormat.Command = "GetSensor";
                        jsonFormat.CRC = Crc32.Compute(jsonFormat);
                        string jsonString = JsonConvert.SerializeObject(jsonFormat);

                        writer.WriteLine(jsonString);
                        string response = reader.ReadLine();

                        JsonFormat inputJson = JsonConvert.DeserializeObject<JsonFormat>(response);
                        string checkCRC = Crc32.Compute(inputJson);

                        if (checkCRC != inputJson.CRC)
                        {
                            MessageBox.Show("CRC Error [Read].");
                        }

                        else if (inputJson.Payload.Data1 == "CRC Error.")
                        {
                            MessageBox.Show("CRC Error [Send].");
                        }

                        else
                        {
                            SensorUnit[0] = inputJson.Payload.Data1.Split(',')[0] != "Off";
                            SensorPlace[0] = inputJson.Payload.Data1.Split(',')[1] != "Off";
                            SensorUnit[1] = inputJson.Payload.Data2.Split(',')[0] != "Off";
                            SensorPlace[1] = inputJson.Payload.Data2.Split(',')[1] != "Off";
                            SensorUnit[2] = inputJson.Payload.Data3.Split(',')[0] != "Off";
                            SensorPlace[2] = inputJson.Payload.Data3.Split(',')[1] != "Off";
                            SensorUnit[3] = inputJson.Payload.Data4.Split(',')[0] != "Off";
                            SensorPlace[3] = inputJson.Payload.Data4.Split(',')[1] != "Off";
                        }
                    }
                }
            }
            catch (TimeoutException)
            {
                MessageBox.Show("Unable to connect to server. Please ensure the server is running.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException)
            {
                MessageBox.Show("An error occurred while communicating with the server.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UpdateHead(string head, string status)
        {
            try
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", namePipe, PipeDirection.InOut))
                {
                    pipeClient.Connect(2000); // ตั้ง timeout 2 วินาทีสำหรับการเชื่อมต่อ
                    using (StreamReader reader = new StreamReader(pipeClient))
                    using (StreamWriter writer = new StreamWriter(pipeClient) { AutoFlush = true })
                    {
                        JsonFormat jsonFormat = new JsonFormat();
                        DateTime currentDate = DateTime.Now;
                        jsonFormat.Date = currentDate.ToString("dd/MM/yyyy");
                        jsonFormat.Time = currentDate.ToString("HH:mm:ss");
                        jsonFormat.Command = "UpdateHead";
                        jsonFormat.Payload.Data1 = head;
                        jsonFormat.Payload.Data2 = status;
                        jsonFormat.CRC = Crc32.Compute(jsonFormat);
                        string jsonString = JsonConvert.SerializeObject(jsonFormat);

                        writer.WriteLine(jsonString);
                        string response = reader.ReadLine();

                        JsonFormat inputJson = JsonConvert.DeserializeObject<JsonFormat>(response);
                        string checkCRC = Crc32.Compute(inputJson);

                        if (checkCRC != inputJson.CRC)
                        {
                            MessageBox.Show("CRC Error [Read].");
                        }

                        else if (inputJson.Payload.Data1 == "CRC Error.")
                        {
                            MessageBox.Show("CRC Error [Send].");
                        }

                        else if (inputJson.Payload.Data1 != "Success")
                        {
                            MessageBox.Show("Unsuccessful.");
                        }

                        else
                        {

                        }
                    }
                }
            }
            catch (TimeoutException)
            {
                MessageBox.Show("Unable to connect to server. Please ensure the server is running.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException)
            {
                MessageBox.Show("An error occurred while communicating with the server.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool[] SetRoobotStartTest = new bool[4];
        private bool[] SetRoobotStartTestSup = new bool[4];
        private bool[] SetRoobotStopTest = new bool[4];
        private void background_relay_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Thread.Sleep(500);
            //LoopCheckInputArduino();
            //LoopCheckSupMain();
            LoopCheckInputIO();
        }
        private void background_relay_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 99999)
            {
                //bool[] sup1 = flag_test;
                //bool[] sup2 = flag_head;
                //if (!tester.useRelayCard) {
                //    for (int i = 0; i < num_head; i++) {
                //        sup2[i] = true;
                //    }
                //}//ไอศครีมเชอเบร็ตรสมะนาว
                //Label[] l = { status_1, status_2, status_3, status_4, status_5,
                //              status_6, status_7, status_8, status_9, status_10,
                //              status_11, status_12, status_13, status_14, status_15,
                //              status_16, status_17, status_18, status_19, status_20,
                //              status_21, status_22, status_23, status_24, status_25,
                //              status_26, status_27, status_28, status_29, status_30,
                //              status_31, status_32, status_33, status_34, status_35,
                //              status_36};
                //Button[] b = { btnTEST_1, btnTEST_2, btnTEST_3, btnTEST_4, btnTEST_5,
                //               btnTEST_6, btnTEST_7, btnTEST_8, btnTEST_9, btnTEST_10,
                //               btnTEST_11, btnTEST_12, btnTEST_13, btnTEST_14, btnTEST_15,
                //               btnTEST_16, btnTEST_17, btnTEST_18, btnTEST_19, btnTEST_20,
                //               btnTEST_21, btnTEST_22, btnTEST_23, btnTEST_24, btnTEST_25,
                //               btnTEST_26, btnTEST_27, btnTEST_28, btnTEST_29, btnTEST_30,
                //               btnTEST_31, btnTEST_32, btnTEST_33, btnTEST_34, btnTEST_35,
                //               btnTEST_36};
                //for (int i = 0; i < num_head; i++) {
                //    if (sup1[i] != true && sup2[i] == true) {
                //        b[i].Enabled = false;
                //        row_test[i] = 0;
                //        flag_test[i] = true;
                //        GlobalTestingFlag[i] = true;
                //        l[i].Text = "TESTING";
                //        l[i].ForeColor = Color.Blue;
                //    }
                //}
                start_all_head();
                return;
            }
            string s = Convert.ToString(e.ProgressPercentage);
            string s1 = "1";
            if (s.Length == 3)
                s1 = s.Substring(2, 1);
            else
                s1 = s.Substring(2, 2);
            string s2 = s.Substring(1, 1);
            if (s2 == "0")
                sup_backgroud_checkhead(Convert.ToInt32(s1), 0);
            if (s2 == "1")
                sup_backgroud_checkhead(Convert.ToInt32(s1), 1);
        }
        private void sup_backgroud_checkhead(int head, int get)
        {
            Button b = getButtonTest(head);
            Label l = getLabelStatus(head);
            GroupBox g = getGroupBoxHead(head);

            switch (get)
            {
                case 0:
                    bt_relayCard.BackColor = Color.LimeGreen;
                    if (flag_head[head - 1])
                        break;
                    flag_head[head - 1] = true;
                    g.ForeColor = Color.Blue;
                    if (!autoTestToolStripMenuItem.Checked) break;
                    if (flag_test[head - 1]) break;
                    b.BeginInvoke((MethodInvoker)delegate
                    {
                        b.Enabled = false;
                    });
                    // b.Enabled = false;
                    row_test[head - 1] = 0;
                    flag_test[head - 1] = true;
                    GlobalTestingFlag[head - 1] = true;
                    l.BeginInvoke((MethodInvoker)delegate
                    {
                        l.Text = "TESTING";
                    });
                    // l.Text = "TESTING";
                    if (configUI.lbStatus == ConfigUI.LbStatus.ForeColor)
                    {
                        l.ForeColor = Color.Blue;
                    }
                    else
                    {
                        l.BackColor = Color.Blue;
                        l.ForeColor = Color.White;
                    }
                    break;
                case 1:
                    bt_relayCard.BackColor = Color.LimeGreen;
                    flag_head[head - 1] = false;
                    g.ForeColor = Color.Black;
                    flag_test[head - 1] = false;
                    break;
                case -4:
                    bt_relayCard.BackColor = Color.Red;
                    break;
            }
        }

        private void txtSNBoard_1_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_1.Clear();
        }
        private void txtSNBoard_2_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_2.Clear();
        }
        private void txtSNBoard_3_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_3.Clear();
        }
        private void txtSNBoard_4_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_4.Clear();
        }
        private void txtSNBoard_5_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_5.Clear();
        }
        private void txtSNBoard_6_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_6.Clear();
        }
        private void txtSNBoard_7_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_7.Clear();
        }
        private void txtSNBoard_8_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_8.Clear();
        }
        private void txtSNBoard_9_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_9.Clear();
        }
        private void txtSNBoard_10_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_10.Clear();
        }
        private void txtSNBoard_11_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_11.Clear();
        }
        private void txtSNBoard_12_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_12.Clear();
        }
        private void txtSNBoard_13_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_13.Clear();
        }
        private void txtSNBoard_14_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_14.Clear();
        }
        private void txtSNBoard_15_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_15.Clear();
        }
        private void txtSNBoard_16_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_16.Clear();
        }
        private void txtSNBoard_17_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_17.Clear();
        }
        private void txtSNBoard_18_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_18.Clear();
        }
        private void txtSNBoard_19_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_19.Clear();
        }
        private void txtSNBoard_20_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_20.Clear();
        }
        private void txtSNBoard_21_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_21.Clear();
        }
        private void txtSNBoard_22_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_22.Clear();
        }
        private void txtSNBoard_23_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_23.Clear();
        }
        private void txtSNBoard_24_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_24.Clear();
        }
        private void txtSNBoard_25_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_25.Clear();
        }
        private void txtSNBoard_26_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_26.Clear();
        }
        private void txtSNBoard_27_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_27.Clear();
        }
        private void txtSNBoard_28_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_28.Clear();
        }
        private void txtSNBoard_29_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_29.Clear();
        }
        private void txtSNBoard_30_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_30.Clear();
        }
        private void txtSNBoard_31_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_31.Clear();
        }
        private void txtSNBoard_32_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_32.Clear();
        }
        private void txtSNBoard_33_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_33.Clear();
        }
        private void txtSNBoard_34_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_34.Clear();
        }
        private void txtSNBoard_35_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_35.Clear();
        }
        private void txtSNBoard_36_Click(object sender, EventArgs e)
        {
            if (configTester.click2ClearSN) txtSNBoard_36.Clear();
        }
        private void txtWO_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || tb_wo.Text == "") return;
            bool b = false;
            for (int i = 0; i < configTester.numHead; i++)
            {
                b |= flag_test[i];
            }
            if (b)
            {
                MessageBox.Show("key WO ไม่ได้ ขณะโปรแกรมกำลังเทสอยู่");
                return;
            }
            tb_wo.Text = tb_wo.Text.ToUpper();
            getWorkOrder(tb_wo.Text);
            //lblUserID.Text = TeamPrecision.PRISM.cSettingValues.EmployeeID;
            tb_userID.Text = setupPay.read_text(ConfigPrism.Header.employeeID, configPrism.nameFile);
        }
        private void lblModelName_TextChanged(object sender, EventArgs e)
        {
            Size size = TextRenderer.MeasureText(tb_detail.Text, tb_detail.Font);
            tb_detail.Width = size.Width;
            lb_fwVersion.Location = new Point(tb_detail.Location.X + tb_detail.Size.Width + 5, lb_fwVersion.Location.Y);
            tb_fwVersion.Location = new Point(lb_fwVersion.Location.X + lb_fwVersion.Size.Width, tb_fwVersion.Location.Y);
            lb_spec.Location = new Point(tb_fwVersion.Location.X + tb_fwVersion.Size.Width + 5, lb_spec.Location.Y);
            tb_spec.Location = new Point(lb_spec.Location.X + lb_spec.Size.Width, tb_spec.Location.Y);
            tb_spec.Size = new Size(groupBox1.Size.Width - (lb_detail.Size.Width + tb_detail.Size.Width + lb_fwVersion.Size.Width + tb_fwVersion.Size.Width + lb_spec.Size.Width + 25), tb_spec.Size.Height);
        }
        private void lblFirmwareVersion_TextChanged(object sender, EventArgs e)
        {
            Size size = TextRenderer.MeasureText(tb_fwVersion.Text, tb_fwVersion.Font);
            tb_fwVersion.Width = size.Width;
            lb_spec.Location = new Point(tb_fwVersion.Location.X + tb_fwVersion.Size.Width + 5, lb_spec.Location.Y);
            tb_spec.Location = new Point(lb_spec.Location.X + lb_spec.Size.Width, tb_spec.Location.Y);
            tb_spec.Size = new Size(groupBox1.Size.Width - (lb_detail.Size.Width + tb_detail.Size.Width + lb_fwVersion.Size.Width + tb_fwVersion.Size.Width + lb_spec.Size.Width + 25), tb_spec.Size.Height);
        }
        private void set_debug_all_Click(object sender, EventArgs e)
        {
            bool val = set_debug_all.Checked;
            ToolStripMenuItem[] allDebug = {
                set_debug_1,  set_debug_2,  set_debug_3,  set_debug_4,  set_debug_5,
                set_debug_6,  set_debug_7,  set_debug_8,  set_debug_9,  set_debug_10,
                set_debug_11, set_debug_12, set_debug_13, set_debug_14, set_debug_15,
                set_debug_16, set_debug_17, set_debug_18, set_debug_19, set_debug_20,
                set_debug_21, set_debug_22, set_debug_23, set_debug_24, set_debug_25,
                set_debug_26, set_debug_27, set_debug_28, set_debug_29, set_debug_30,
                set_debug_31, set_debug_32, set_debug_33, set_debug_34, set_debug_35,
                set_debug_36
            };
            for (int i = 0; i < configTester.numHead; i++)
            {
                allDebug[i].Checked = val;
                File.WriteAllText("../../config/test_head_" + (i + 1) + "_debug.txt", val.ToString());
            }
        }
        private void head1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_1_debug.txt", set_debug_1.Checked.ToString());
        }
        private void head2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_2_debug.txt", set_debug_2.Checked.ToString());
        }
        private void head3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_3_debug.txt", set_debug_3.Checked.ToString());
        }
        private void head4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_4_debug.txt", set_debug_4.Checked.ToString());
        }
        private void head5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_5_debug.txt", set_debug_5.Checked.ToString());
        }
        private void head6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_6_debug.txt", set_debug_6.Checked.ToString());
        }
        private void head7ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_7_debug.txt", set_debug_7.Checked.ToString());
        }
        private void head8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_8_debug.txt", set_debug_8.Checked.ToString());
        }
        private void head9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_9_debug.txt", set_debug_9.Checked.ToString());
        }
        private void head10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_10_debug.txt", set_debug_10.Checked.ToString());
        }
        private void set_debug_11_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_11_debug.txt", set_debug_11.Checked.ToString());
        }
        private void set_debug_12_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_12_debug.txt", set_debug_12.Checked.ToString());
        }
        private void set_debug_13_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_13_debug.txt", set_debug_13.Checked.ToString());
        }
        private void set_debug_14_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_14_debug.txt", set_debug_14.Checked.ToString());
        }
        private void set_debug_15_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_15_debug.txt", set_debug_15.Checked.ToString());
        }
        private void set_debug_16_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_16_debug.txt", set_debug_16.Checked.ToString());
        }
        private void set_debug_17_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_17_debug.txt", set_debug_17.Checked.ToString());
        }
        private void set_debug_18_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_18_debug.txt", set_debug_18.Checked.ToString());
        }
        private void set_debug_19_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_19_debug.txt", set_debug_19.Checked.ToString());
        }
        private void set_debug_20_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_20_debug.txt", set_debug_20.Checked.ToString());
        }
        private void set_debug_21_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_21_debug.txt", set_debug_21.Checked.ToString());
        }
        private void set_debug_22_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_22_debug.txt", set_debug_22.Checked.ToString());
        }
        private void set_debug_23_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_23_debug.txt", set_debug_23.Checked.ToString());
        }
        private void set_debug_24_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_24_debug.txt", set_debug_24.Checked.ToString());
        }
        private void set_debug_25_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_25_debug.txt", set_debug_25.Checked.ToString());
        }
        private void set_debug_26_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_26_debug.txt", set_debug_26.Checked.ToString());
        }
        private void set_debug_27_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_27_debug.txt", set_debug_27.Checked.ToString());
        }
        private void set_debug_28_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_28_debug.txt", set_debug_28.Checked.ToString());
        }
        private void set_debug_29_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_29_debug.txt", set_debug_29.Checked.ToString());
        }
        private void set_debug_30_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_30_debug.txt", set_debug_30.Checked.ToString());
        }
        private void set_debug_31_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_31_debug.txt", set_debug_31.Checked.ToString());
        }
        private void set_debug_32_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_32_debug.txt", set_debug_32.Checked.ToString());
        }
        private void set_debug_33_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_33_debug.txt", set_debug_33.Checked.ToString());
        }
        private void set_debug_34_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_34_debug.txt", set_debug_34.Checked.ToString());
        }
        private void set_debug_35_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_35_debug.txt", set_debug_35.Checked.ToString());
        }
        private void set_debug_36_Click(object sender, EventArgs e)
        {
            File.WriteAllText("../../config/test_head_36_debug.txt", set_debug_36.Checked.ToString());
        }
        private void show_data_grid_Click(object sender, EventArgs e)
        {
            fMain_sizechanged();
        }
        private void bt_config_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the path to the executable file
                string exePath = @"fMainConfig.exe";

                // Create a new process start info
                ProcessStartInfo startInfo = new ProcessStartInfo(exePath);

                // Start the process
                using (Process process = Process.Start(startInfo))
                {
                    // Wait for the process to exit
                    process.WaitForExit();

                    MessageBox.Show("Please restart the program if config values ​​are changed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void bt_AutoPortCamera_Click(object sender, EventArgs e)
        {
            if (!flag_test.All(value => !value))
            {
                MessageBox.Show("***โปรแกรมกำลังทำงานอยู่***");
                return;
            }

            // กำหนดขนาดของ Form
            Form popup = new Form();
            popup.Size = new Size(1500, 720);
            popup.Text = "Set Fixture for Auto Camera";
            popup.BackColor = Color.Black;
            popup.Icon = Properties.Resources.SetPortCamera_Icon;
            popup.FormBorderStyle = FormBorderStyle.FixedSingle;
            popup.MaximizeBox = false;

            // สร้าง PictureBox
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = Image.FromFile("../../Image/AutoPortCamera_1.png"); // แทน path_to_your_image.jpg ด้วยที่อยู่ของรูปภาพของคุณ
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // กำหนดโหมดการแสดงรูปภาพ
            pictureBox.Size = new Size(700, 600); // กำหนดขนาดของ PictureBox
            pictureBox.Location = new Point(10, 10); // กำหนดตำแหน่งของ PictureBox
            popup.Controls.Add(pictureBox);

            // สร้าง PictureBox
            PictureBox pictureBox2 = new PictureBox();
            pictureBox2.Image = Image.FromFile("../../Image/AutoPortCamera_2.png"); // แทน path_to_your_image.jpg ด้วยที่อยู่ของรูปภาพของคุณ
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage; // กำหนดโหมดการแสดงรูปภาพ
            pictureBox2.Size = new Size(700, 600); // กำหนดขนาดของ PictureBox
            pictureBox2.Location = new Point(770, 10); // กำหนดตำแหน่งของ PictureBox
            popup.Controls.Add(pictureBox2);

            // ตรวจสอบว่ากด OK หรือไม่
            bool flagOK = false;

            // สร้าง Button "OK"
            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.Location = new Point(690, 620);
            okButton.BackColor = Color.White;
            okButton.Size = new Size(100, 50);
            okButton.Font = new Font("Comic Sans MS", 20, FontStyle.Bold);
            okButton.Click += (s, ev) => { flagOK = true; popup.Close(); };
            popup.Controls.Add(okButton);

            popup.StartPosition = FormStartPosition.CenterParent;
            popup.ShowDialog();

            if (!flagOK)
                return;

            try
            {
                Activator.CreateInstance(functionExcel, this, "SetPortCameraMain()");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //open form document
        private void bt_document_Click(object sender, EventArgs e)
        {
            Document document = new Document();
            document.Show();
        }
        #endregion
    }
}
