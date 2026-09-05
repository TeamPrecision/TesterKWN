using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using USBClassLibrary;
using Spire.Xls;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.IO.Ports;
using System.Management;
using System.Text;
using System.Linq;
using ATS.Config;

namespace ATS
{
    public class InstrumentConfig
    {
        public string nameDMM { get; set; }
        public string nameOSC { get; set; }
        public DeviceDefine define { get; set; }
        public MessageErr messageErr { get; set; }
        public DmmCommands dmm { get; set; }
        public OscCommands osc { get; set; }

        public InstrumentConfig()
        {
            define = new DeviceDefine();
            messageErr = new MessageErr();
            dmm = new DmmCommands();
            osc = new OscCommands();
        }

        public class DmmCommands
        {
            public string connected { get; set; }
            public string clearErr { get; set; }
            public string readResister { get; set; }
            public string readVoltage { get; set; }
            public string readCurrent { get; set; }
            public string setLocal { get; set; }
            public string selectHold { get; set; }
            public string onHold { get; set; }
            public string offHold { get; set; }
            public string onBeeper { get; set; }
            public string offBeeper { get; set; }
            public string sourceOn { get; set; }
            public string sourceOff { get; set; }
            public string volt { get; set; }

            public DmmCommands()
            {
                connected = "Connected";
                clearErr = "*CLS\n";
                readResister = ":MEAS:RES? ";
                readVoltage = ":MEAS:VOLT:DC? ";
                readCurrent = "MEAS:CURR:DC? ";
                setLocal = ":SYST:LOC\n";
                selectHold = "CALC:FUNC HOLD";
                onHold = "CALC:STAT ON";
                offHold = "CALC:STAT OFF";
                onBeeper = "SYSTem:BEEPer:STATe 1";
                offBeeper = "SYSTem:BEEPer:STATe 0";
                sourceOn = "OUTP ON";
                sourceOff = "OUTP OFF";
                volt = "VOLT ";
            }
        }

        public class OscCommands
        {
            public string readFetc { get; set; }
            public string clearErr { get; set; }
            public string read { get; set; }

            public OscCommands()
            {
                readFetc = "INIT;FETC?";
                clearErr = "CLS\n";
                read = ":READ?";
            }
        }

        public class DeviceDefine
        {
            public string nameOSC { get; set; }
            public string connect { get; set; }
            public string OSC { get; set; }
            public string DMM { get; set; }
            public string deviceGet { get; set; }
            public string deviceDescription { get; set; }
            public string deviceName { get; set; }
            public string TF960 { get; set; }
            public string pidOSC { get; set; }
            public string vidOSC { get; set; }

            public DeviceDefine()
            {
                nameOSC = "0x0957::0x1807";
                connect = "Connected";
                OSC = "Oscilloscope";
                DMM = "DMM";
                deviceGet = "SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'";
                TF960 = "TTi TF960";
                deviceDescription = "Description";
                deviceName = "NAME";
                pidOSC = "0492";
                vidOSC = "103E";
            }
        }

        public class MessageErr
        {
            public string connectDMM { get; set; }
            public string fineNameDMM { get; set; }
            public string fineNameOSC { get; set; }

            public MessageErr()
            {
                connectDMM = "\nDMM no connect";
                fineNameDMM = "\nCan't find DMM name";
                fineNameOSC = "\nCan't find OSC name";
            }
        }
    }

    public class SetFile
    {
        public void cameraList()
        {
            File.WriteAllText("camera_show_list.txt", "");
        }
    }

    class TestScriptRunner
    {
        public TestScriptRunner(fMain f1, string strCMD)
        {
            fMain = f1;
            alice = fMain.excel.alice;
            eval(strCMD);
        }

        #region ===============================================Define====================================================
        private fMain fMain;
        private static InstrumentConfig psu;
        private static Connect DcPSU;
        private static string _atsPortName;
        private static string _scannerPortName;
        private static string _panelSN;
        private static bool   _panelSNScanned;
        private static bool   _panelSNFailed;
        private static bool   _highGearReady;
        private static bool   _mediumGearReady;
        private static bool   _lowGearReady;
        private static bool   _acnGndReady;
        private static bool   _relayReady;
        private static bool   _sw301Ready;
        private static bool   _daqRunning;
        private static bool   _daqResultsReady;
        private static double[] _daqResults;
        private static bool   _tp37ResultsReady;
        private static double[] _tp37Results;
        private static bool   _tp33ResultsReady;
        private static double[] _tp33Results;
        private static NIDAQTester12.DaqManager12 _daq;
        private static readonly object _panelSNLock = new object();

        public static void ResetPanelScan()
        {
            lock (_panelSNLock)
            {
                _panelSNScanned = false; _panelSN = null; _panelSNFailed = false;
                _highGearReady = false; _mediumGearReady = false; _lowGearReady = false;
                _acnGndReady = false; _relayReady = false; _sw301Ready = false;
                _daqRunning = false; _daqResultsReady = false; _daqResults = null;
                _tp37ResultsReady = false; _tp37Results = null;
                _tp33ResultsReady = false; _tp33Results = null;
                _ledDetectDone = false; _ledResults = null;
                _ledOnDetectDone = false; _ledOnResults = null;
                _daq = null;
            }
        }
        private SetFile setFile = new SetFile();

        static string[] alice = new string[5];
        private List<USBClassLibrary.USBClass.DeviceProperties> pidList;
        #endregion

        #region ===============================================Function Support====================================================
        private void define_class()
        {
            psu = new InstrumentConfig();
            DcPSU = new Connect();
        }
        private void getNameDMM_visa()
        {
            Ivi.Visa.Interop.ResourceManager visa = new Ivi.Visa.Interop.ResourceManager();

            try
            {
                string[] visaList = visa.FindRsrc("?*");

                foreach (string list in visaList)
                {
                    if (list.Contains(fMain.configTester.nameDMM))
                    {
                        psu.nameDMM = list;
                    }
                }
            }
            catch
            {
                fMain.Log(fMain.LogMsgType.Incoming_Blue, psu.messageErr.fineNameDMM);
                return;
            }

            if (psu.nameDMM == "" || psu.nameDMM == null)
            {
                psu.nameDMM = string.Empty;
                fMain.Log(fMain.LogMsgType.Incoming_Blue, psu.messageErr.fineNameDMM);
            }
        }
        private void getNameOSC_visa()
        {
            Ivi.Visa.Interop.ResourceManager visa = new Ivi.Visa.Interop.ResourceManager();

            try
            {
                string[] visaList = visa.FindRsrc("?*");

                foreach (string list in visaList)
                {
                    if (list.Contains(psu.define.nameOSC))
                    {
                        psu.nameOSC = list;
                    }
                }
            }
            catch
            {
                fMain.Log(fMain.LogMsgType.Incoming_Blue, psu.messageErr.fineNameOSC);
                return;
            }

            if (psu.nameOSC == "" || psu.nameOSC == null)
            {
                psu.nameOSC = string.Empty;
                fMain.Log(fMain.LogMsgType.Incoming_Blue, psu.messageErr.fineNameOSC);
            }
        }
        private void checkDMMconnect_visa()
        {
            fMain.bt_reserve1.Text = psu.define.DMM;

            if (DcPSU.ConnectInstr(psu.nameDMM) == psu.define.connect)
            {
                fMain.bt_reserve1.BackColor = Color.LimeGreen;
                DcPSU.DisConnectInstr();
            }
            else
            {
                fMain.bt_reserve1.BackColor = Color.Red;
            }
        }
        private void checkOSCconnect_visa()
        {
            fMain.bt_reserve2.Text = psu.define.OSC;

            if (DcPSU.ConnectInstr(psu.nameOSC) == psu.define.connect)
            {
                fMain.bt_reserve2.BackColor = Color.LimeGreen;
                DcPSU.DisConnectInstr();
            }
            else
            {
                fMain.bt_reserve2.BackColor = Color.Red;
            }
        }
        private void checkOSCconnect_deviceComport()
        {
            fMain.bt_reserve2.Text = psu.define.OSC;
            fMain.bt_reserve2.BackColor = Color.Red;

            ManagementObjectSearcher getComport = new ManagementObjectSearcher(psu.define.deviceGet);
            ManagementObjectCollection getComportAll = getComport.Get();

            foreach (ManagementObject nameList in getComportAll)
            {
                if (!nameList[psu.define.deviceDescription].ToString().Contains(psu.define.TF960))
                    continue;

                string[] nameComport = nameList.GetPropertyValue(psu.define.deviceName).ToString().Split('(', ')');
                psu.nameOSC = nameComport[1];

                fMain.bt_reserve2.BackColor = Color.LimeGreen;
            }
        }
        private void checkOSCconnect_pid()
        {
            fMain.bt_reserve2.Text = psu.define.OSC;

            pidList = new List<USBClass.DeviceProperties>();

            if (USBClass.GetUSBDevice(uint.Parse(psu.define.vidOSC, System.Globalization.NumberStyles.AllowHexSpecifier),
                uint.Parse(psu.define.pidOSC, System.Globalization.NumberStyles.AllowHexSpecifier), ref pidList, true, null))
            {
                psu.nameOSC = pidList[0].COMPort;
                fMain.bt_reserve2.BackColor = Color.LimeGreen;
            }
            else
            {
                fMain.bt_reserve2.BackColor = Color.Red;
            }
        }

        public void PrismCheckProcess()
        {
            fMain.UpdateResultToDataGrid(alice[0], alice[2], fMain.define.pass);

            if (fMain.configPrism.mode != fMain.configPrism.Debug)
            {
                TextBox textBox = fMain.getTextBoxSN(fMain.select_test);

                string[] status = TeamPrecision.PRISM.cSNs.CheckStatusSNv2(textBox.Text, fMain.tb_wo.Text);

                if (status[0] == fMain.configPrism.success)
                {
                    if (status[1] == textBox.Text)
                    {
                        fMain.flag_sn_pass[fMain.select_test - 1] = true;
                        return;
                    }
                }

                if (status[1].Contains(fMain.configPrism.processBeforeText))
                {
                    if (!status[1].Contains($"{fMain.configPrism.processBeforeText}."))
                    {
                        fMain.Log(fMain.LogMsgType.Error_Red, "\n" + status[1]);
                        fMain.UpdateResultToDataGrid(alice[0], status[1], fMain.define.fail);
                        return;
                    }
                }

                fMain.Log(fMain.LogMsgType.Incoming_Blue, "\n" + status[1]);

                if (status[1].Contains(fMain.prism_retest_text_fail.Text))
                {
                    fMain.flag_sn_pass[fMain.select_test - 1] = true;
                    return;
                }

                if (status[1].Contains(fMain.prism_retest_text_pass.Text))
                {
                    if (fMain.prism_retest.Checked)
                    {
                        fMain.flag_sn_pass[fMain.select_test - 1] = true;
                        return;
                    }
                    else
                    {
                        if (CallFormReTest())
                        {
                            fMain.flag_sn_pass[fMain.select_test - 1] = true;
                            return;
                        }

                        fMain.flagNotReTest[fMain.select_test - 1] = true;
                        fMain.row_test[fMain.select_test - 1] += 10000;
                        return;
                    }
                }

                fMain.UpdateResultToDataGrid(alice[0], status[1], fMain.define.fail);
                fMain.Log(fMain.LogMsgType.Error_Red, $"\n{status[1]} ไม่เข้าเงื่อนไข");
            }
        }
        public void PrismCheckProcessV2()
        {
            fMain.UpdateResultToDataGrid(alice[0], alice[2], fMain.define.pass);

            if (fMain.configPrism.mode != fMain.configPrism.Debug)
            {
                TextBox textBox = fMain.getTextBoxSN(fMain.select_test);

                string[] status = TeamPrecision.PRISM.cSNs.sn_check_valid(textBox.Text, fMain.tb_wo.Text, fMain.configPrism.processName, true);

                if (status[0] == "PASS")
                {
                    fMain.flag_sn_pass[fMain.select_test - 1] = true;
                    return;
                }

                if (status[0].Contains(fMain.configPrism.processBeforeText))
                {
                    if (!status[0].Contains($"{fMain.configPrism.processBeforeText}."))
                    {
                        fMain.Log(fMain.LogMsgType.Error_Red, "\n" + status[0]);
                        fMain.UpdateResultToDataGrid(alice[0], status[0], fMain.define.fail);
                        return;
                    }
                }

                fMain.Log(fMain.LogMsgType.Incoming_Blue, "\n" + status[0]);

                if (status[0].Contains(fMain.prism_retest_text_fail.Text))
                {
                    fMain.flag_sn_pass[fMain.select_test - 1] = true;
                    return;
                }

                if (status[0].Contains(fMain.prism_retest_text_pass.Text))
                {
                    if (fMain.prism_retest.Checked)
                    {
                        fMain.flag_sn_pass[fMain.select_test - 1] = true;
                        return;
                    }
                    else
                    {
                        if (CallFormReTest())
                        {
                            fMain.flag_sn_pass[fMain.select_test - 1] = true;
                            return;
                        }

                        fMain.flagNotReTest[fMain.select_test - 1] = true;
                        fMain.row_test[fMain.select_test - 1] += 10000;
                        return;
                    }
                }

                fMain.UpdateResultToDataGrid(alice[0], status[0], fMain.define.fail);
            }
        }

        private void GenFunctionToCSV()
        {
            List<string> function = new List<string>();

            foreach (MemberInfo memberInfo in this.GetType().GetMembers())
            {
                if (memberInfo.Name == ".ctor" || memberInfo.Name == "Equals" || memberInfo.Name == "GetHashCode" ||
                    memberInfo.Name == "GetType" || memberInfo.Name == "ToString" || memberInfo.Name == "LoadTestSpec" ||
                    memberInfo.Name == "OnAllFunction" || memberInfo.Name == "intro_test" || memberInfo.Name == "after_test" ||
                    memberInfo.Name == "OffAllFunction")
                {
                    continue;
                }

                string nameFunction = memberInfo.Name;
                string parameter = "";

                foreach (ParameterInfo parameterInfo in ((MethodInfo)memberInfo).GetParameters())
                {
                    parameter += "," + parameterInfo.Name;
                }

                function.Add(nameFunction + parameter);
            }

            File.WriteAllLines("AllFunctionInClass.csv", function);
        }

        public void camera_set_step(string cmd = "read2d")
        {
            File.WriteAllText("../../config/test_head_1_steptest.txt", cmd);
            File.Delete("test_head_1_result.txt");
        }
        public void set_timeout(string cmd = "1000")
        {
            File.WriteAllText("../../config/test_head_1_timeout.txt", cmd);
        }
        public void camera_set_list()
        {
            while (true)
            {
                try
                {
                    File.AppendAllText("camera_show_list.txt", fMain.select_test.ToString());
                    break;
                }
                catch { }
                fMain.DelaymS(250);
            }
        }

        private void SetPortCameraCsv(string fileTarget, string head, string port)
        {
            string[] dataConfig = File.ReadAllLines(fileTarget);

            for (int loop = 0; loop < dataConfig.Length; loop++)
            {
                if (dataConfig[loop].StartsWith($"Head {head} Port"))
                {
                    string[] parts = dataConfig[loop].Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int value))
                    {
                        dataConfig[loop] = $"Head {head} Port,{port}";
                        break;
                    }
                }
            }

            File.WriteAllLines(fileTarget, dataConfig);
        }
        private void ShowMessage(Form formScan, string message)
        {
            formScan.FormBorderStyle = FormBorderStyle.FixedSingle;
            formScan.MaximizeBox = false;
            formScan.Text = "Put Unit Error";
            formScan.StartPosition = FormStartPosition.CenterScreen;
            formScan.Size = new Size(1000, 400);
            formScan.BackColor = Color.Red;
            Label label = new Label();
            label.Text = $"{message}";
            label.ForeColor = Color.White;
            label.Size = new Size(900, 350);
            label.Location = new Point(50, 70);
            FontFamily fontFamily = new FontFamily("Yu Gothic UI");
            label.Font = new Font(fontFamily, 130, FontStyle.Bold, GraphicsUnit.Pixel);
            formScan.Controls.Add(label);
            formScan.Show();
            fMain.DelaymS(150);
        }

        // Script execution — consolidated: RunScriptWait uses visible window based on config,
        // RunScriptWait2 always hides the window.
        private string RunScript(string nameFile)
        {
            string username = Environment.UserName;
            string pythonPath = $@"C:\Users\{username}\AppData\Local\Programs\Python\{fMain.configScript.pyVersion}\{fMain.configScript.pyFolder}\python.exe";
            string scriptPath = $"../../Script/{nameFile}.py";

            Process process = new Process();
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.FileName = pythonPath;
            process.StartInfo.Arguments = $"\"{scriptPath}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = false;

            try { process.Start(); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            return string.Empty;
        }

        private string RunScriptWait(string nameFile, string folder, params string[] args)
            => ExecuteScript(nameFile, folder, forceHidden: false, args);

        private string RunScriptWait2(string nameFile, string folder, params string[] args)
            => ExecuteScript(nameFile, folder, forceHidden: true, args);

        private string ExecuteScript(string nameFile, string folder, bool forceHidden, string[] args)
        {
            string username = Environment.UserName;
            string pythonPath = $@"C:\Users\{username}\AppData\Local\Programs\Python\{fMain.configScript.pyVersion}\{fMain.configScript.pyFolder}\python.exe";
            string scriptPath = $"../../Script/{folder}/{nameFile}.py";
            string quotedArgs = args.Length > 0
                ? " " + string.Join(" ", Array.ConvertAll(args, a => $"\"{a}\""))
                : string.Empty;

            Process process = new Process();
            process.StartInfo.CreateNoWindow = forceHidden || !fMain.ctms_showCmd.Checked;
            process.StartInfo.FileName = pythonPath;
            process.StartInfo.Arguments = $"\"{scriptPath}\"{quotedArgs}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            try
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                return lines.Length > 0 ? lines[lines.Length - 1] : string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return "Error";
            }
        }

        //Find Port
        private static string PortDmm;
        private void FindPortDmm()
        {
            string result = RunScriptWait("FindPort", "U3606");
            fMain.bt_reserve1.Text = "DMM";

            if (result.Contains("0x0957::0x4D18"))
            {
                PortDmm = result;
                fMain.bt_reserve1.BackColor = Color.LimeGreen;
            }
            else
            {
                fMain.bt_reserve1.BackColor = Color.Red;
            }
        }

        // Measure
        public void MeasureResistor(string rang = "5000", string timeout = "1000")
        {
            double min_ = Convert.ToDouble(alice[2]);
            double max_ = Convert.ToDouble(alice[3]);
            double value = 0;
            double timeout_ = Convert.ToDouble(timeout);
            bool passed = false;

            Stopwatch time = Stopwatch.StartNew();

            while (time.ElapsedMilliseconds < timeout_)
            {
                string valueString = RunScriptWait("MeasureResistance", "U3606", PortDmm, rang);

                if (valueString.Contains("VISA error"))
                {
                    RunScriptWait("ClearError", "U3606", PortDmm);
                    continue;
                }

                value = Convert.ToDouble(valueString);
                if (value > max_ || value < min_)
                {
                    fMain.UpdateResultToDataGrid(alice[0], value.ToString(), "FAIL");
                    fMain.DelaymS(50);
                    continue;
                }

                fMain.UpdateResultToDataGrid(alice[0], value.ToString(), "PASS");
                passed = true;
                break;
            }

            if (!passed)
            {
                fMain.UpdateResultToDataGrid(alice[0], value.ToString(), "FAIL");
            }
        }

        // Skip
        public void SkipPass()
        {
            fMain.UpdateResultToDataGrid(alice[0], alice[2], "PASS");
        }
        #endregion

        #region ===============================================Function Main====================================================
        public void delay_ms(string time)
        {
            fMain.DelaymS(Convert.ToInt32(time));
        }

        public void test_pass()
        {
            fMain.UpdateResultToDataGrid(alice[0], "PASS", "PASS");
        }
        public void test_fail()
        {
            fMain.UpdateResultToDataGrid(alice[0], "FAIL", "FAIL");
        }
        public void test_write(string data)
        {
            fMain.UpdateResultToDataGrid(alice[0], data, "PASS");
        }

        private TextBox getTextBox()
        {
            return fMain.getTextBoxSN(fMain.select_test);
        }
        private DataGridView getDataGridView()
        {
            return fMain.getDataGridView(fMain.select_test);
        }
        public void Camera_set_sn_to_textbox()
        {
            fMain.flag_sn_pass[fMain.select_test - 1] = false;
            fMain.getTextBoxSN(fMain.select_test).Text = serialTeamSup[fMain.select_test - 1];
        }
        public void Camera_clear_sn_in_textbox()
        {
            fMain.getTextBoxSN(fMain.select_test).Text = "";
        }
        public void ScannerGetFile()
        {
            string sn = string.Empty;
            bool flagReadSN = false;
            try
            {
                sn = File.ReadAllText("dryice_scan2d_sn_header_" + fMain.select_test + ".txt");
                flagReadSN = true;
            }
            catch { }
            if (flagReadSN)
            {
                fMain.UpdateResultToDataGrid(alice[0], sn, "PASS");
            }
            else
            {
                fMain.UpdateResultToDataGrid(alice[0], "Not Found SN", "FAIL");
                ScanSnPutUnitError();
            }
            File.Delete("dryice_scan2d_sn_header_" + fMain.select_test + ".txt");
        }
        private static bool[] waitTextBarCode = new bool[36];
        private static bool   _ledDetectDone;
        private static string[] _ledResults;
        private static bool   _ledOnDetectDone;
        private static string[] _ledOnResults;
        private static string[] serialTeamSup = new string[36];
        public void Read2DBarCode()
        {
            if (!waitTextBarCode[fMain.select_test - 1])
            {
                Camera_clear_sn_in_textbox();
            }

            if (fMain.configUI.readBarcode == ConfigUI.ReadBarcode.Camera)
            {
                string pathResult = $"test_head_{fMain.select_test}_result.txt";

                if (waitTextBarCode[fMain.select_test - 1])
                {
                    try
                    {
                        if (File.Exists(pathResult))
                        {
                            string[] dataCamera = File.ReadAllLines(pathResult);
                            File.Delete(pathResult);
                            fMain.UpdateResultToDataGrid(alice[0], dataCamera[0], dataCamera[1]);
                            serialTeamSup[fMain.select_test - 1] = dataCamera[0];
                            ProgressBar progressBar = fMain.getProgressBar(fMain.select_test);
                            progressBar.Value--;
                            return;
                        }
                    }
                    catch { }

                    ProgressBar progressBar_ = fMain.getProgressBar(fMain.select_test);
                    progressBar_.Value--;
                    fMain.row_test[fMain.select_test - 1]--;
                    return;
                }

                File.WriteAllText("../../config/head.txt", fMain.select_test.ToString());
                camera_set_step("read2d");
                camera_set_list();
                set_timeout("5000");

                Process process = new Process();
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = $"../../mini_projeck/camera_show.exe";
                process.StartInfo.UseShellExecute = false;

                File.Delete(pathResult);
                File.Delete("call_exe_tric.txt");

                try
                {
                    process.Start();
                    waitTextBarCode[fMain.select_test - 1] = true;
                }
                catch (Exception ex) { }

                while (true)
                {
                    try
                    {
                        File.ReadAllText("call_exe_tric.txt");
                        break;
                    }
                    catch { }
                    fMain.DelaymS(50);
                }

                fMain.row_test[fMain.select_test - 1]--;
            }

            if (fMain.configUI.readBarcode == ConfigUI.ReadBarcode.Scanner)
            {
                ScannerGetFile();
            }
        }

        private void ScanSnPutUnitError()
        {
            Form formScan = new Form();
            formScan.FormBorderStyle = FormBorderStyle.FixedSingle;
            formScan.MaximizeBox = false;
            formScan.Text = "Put Unit Error";
            formScan.StartPosition = FormStartPosition.CenterScreen;
            formScan.Size = new Size(1000, 400);
            formScan.BackColor = Color.Red;
            Label label = new Label();
            label.Text = "ใส่งานผิดหัว";
            label.ForeColor = Color.White;
            label.Size = new Size(900, 350);
            label.Location = new Point(50, 70);
            FontFamily fontFamily = new FontFamily("Yu Gothic UI");
            label.Font = new Font(fontFamily, 150, FontStyle.Bold, GraphicsUnit.Pixel);
            formScan.Controls.Add(label);
            formScan.ShowDialog();
        }

        private Form formTest = new Form();
        private bool flagReTestPopUp;
        private bool CallFormReTest()
        {
            flagReTestPopUp = false;
            formTest.Size = new Size(710, 300);
            formTest.FormBorderStyle = FormBorderStyle.FixedSingle;
            formTest.StartPosition = FormStartPosition.CenterScreen;
            formTest.MaximizeBox = false;
            formTest.Text = fMain.select_test + ".ReTest";
            Label lb_head = new Label();
            lb_head.Text = "Head";
            lb_head.ForeColor = Color.Brown;
            lb_head.Location = new Point(0, 0);
            FontFamily fontFamily = new FontFamily("Arial");
            lb_head.Font = new Font(fontFamily, 70, FontStyle.Bold, GraphicsUnit.Pixel);
            lb_head.AutoSize = true;
            Label lb_number = new Label();
            lb_number.Text = fMain.select_test.ToString();
            lb_number.ForeColor = Color.Blue;
            lb_number.Location = new Point(10, 60);
            lb_number.Font = new Font(fontFamily, 200, FontStyle.Bold, GraphicsUnit.Pixel);
            lb_number.AutoSize = true;
            Label lb_detail = new Label();
            lb_detail.Text = "Want to repeat the test?";
            lb_detail.Font = new Font(fontFamily, 20, FontStyle.Bold, GraphicsUnit.Pixel);
            lb_detail.AutoSize = true;
            lb_detail.Location = new Point(350, 50);
            Button bt_reTest = new Button();
            bt_reTest.Click += ButtonReTestClick;
            bt_reTest.Text = "ReTest";
            bt_reTest.Font = new Font(fontFamily, 30, FontStyle.Bold, GraphicsUnit.Pixel);
            bt_reTest.Location = new Point(250, 150);
            bt_reTest.Size = new Size(200, 60);
            Button bt_cancel = new Button();
            bt_cancel.Click += ButtonCancelClick;
            bt_cancel.Text = "Cancel";
            bt_cancel.Font = new Font(fontFamily, 30, FontStyle.Bold, GraphicsUnit.Pixel);
            bt_cancel.Location = new Point(470, 150);
            bt_cancel.Size = new Size(200, 60);

            formTest.Controls.Add(lb_head);
            formTest.Controls.Add(lb_number);
            formTest.Controls.Add(lb_detail);
            formTest.Controls.Add(bt_reTest);
            formTest.Controls.Add(bt_cancel);
            formTest.ShowDialog();

            return flagReTestPopUp;
        }
        private void ButtonReTestClick(object sender, EventArgs e)
        {
            flagReTestPopUp = true;
            formTest.Close();
        }
        private void ButtonCancelClick(object sender, EventArgs e)
        {
            formTest.Close();
        }
        #endregion

        #region ================================================EXCEL========================================================
        private void eval(string string_of_function)
        {
            string name_function = "";
            List<string> parameter = new List<string>();
            string support_parameter = "";
            int num_parameter = 0;
            try
            {
                for (int i = 0; i < string_of_function.Length; i++)
                {
                    if (string_of_function.Substring(i, 1) == "(")
                    {
                        break;
                    }
                    name_function += string_of_function.Substring(i, 1);
                }
                for (int i = name_function.Length + 1; i < string_of_function.Length; i++)
                {
                    if (string_of_function.Substring(i, 1) != "=")
                    {
                        continue;
                    }
                    for (int j = i + 1; j < string_of_function.Length; j++)
                    {
                        if (string_of_function.Substring(j, 1) == "," || string_of_function.Substring(j, 1) == ")")
                        {
                            num_parameter++;
                            i = j;
                            parameter.Add(support_parameter);
                            support_parameter = "";
                            goto label_num_parameter;
                        }
                        support_parameter += string_of_function.Substring(j, 1);
                    }
                label_num_parameter:
                    ;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("header " + fMain.select_test + " function error " + string_of_function);
                fMain.UpdateResultToDataGrid(alice[0], "", "FAIL");
                return;
            }
            for (int reHead = 0; reHead <= 1; reHead++)
            {
                object[] objects = parameter.ConvertAll<object>(item => (object)item).ToArray();
                MethodInfo mi = this.GetType().GetMethod(name_function);
                try
                {
                    mi.Invoke(this, objects);
                    break;
                }
                catch
                {
                    if (reHead == 0)
                    {
                        string headSup = name_function.Substring(0, 1);
                        string tailSup = name_function.Substring(1, name_function.Length - 1);
                        if (headSup.Any(char.IsUpper))
                        {
                            headSup = headSup.ToLower();
                        }
                        else
                        {
                            headSup = headSup.ToUpper();
                        }
                        name_function = headSup + tailSup;
                        continue;
                    }
                    MessageBox.Show("header " + fMain.select_test + " function error " + string_of_function);
                    fMain.UpdateResultToDataGrid(alice[0], "", "FAIL");
                    return;
                }
            }
        }
        #endregion

        public void LoadTestSpec()
        {
        }
        public void setup()
        {
            define_class();
            setFile.cameraList();
        }
        public void OnAllFunction()
        {
        }
        public void intro_test()
        {
            waitTextBarCode[fMain.select_test - 1] = false;
        }
        public void after_test()
        {
            GetStatusResult();
        }
        public void OffAllFunction()
        {
            if (_daq != null)
            {
                try { _daq.Dispose(); } catch { }
            }
            if (fMain.configTester.useRelayCard)
            {
                if (_atsPortName != null)
                {
                    try
                    {
                        using (ATS1000B.AtsController ats = new ATS1000B.AtsController(_atsPortName, Convert.ToInt32(_atsbaudRate)))
                            ats.SetOutput(false);
                        fMain.DelaymS(1000);
                    }
                    catch { }
                }
                fMain.OffAllRelay();
            }
            ResetPanelScan();
        }

        #region ============================================== Function User =====================================================

        public void GetStatusResult()
        {
            DataGridView dataGrid = fMain.getDataGridView(fMain.select_test);
            if (dataGrid.Rows.Count == 0)
            {
                MessageBox.Show("Get DataGridView Error");
            }
            bool results = true;
            for (int loop = 0; loop < dataGrid.Rows.Count - 1; loop++)
            {
                try
                {
                    if (dataGrid.Rows[loop].Cells[4].Value.ToString() != "PASS" && dataGrid.Rows[loop].Cells[4].Value.ToString() != "")
                    {
                        results = false;
                        break;
                    }
                }
                catch { results = false; break; }
            }
            if (results)
                File.WriteAllText("dryice_scan2d_tested_" + fMain.select_test + ".txt", "PASS");
            else
                File.WriteAllText("dryice_scan2d_tested_" + fMain.select_test + ".txt", "FAIL");
        }

        public void SettimeDelayExe(string cmd = "")
        {
            File.WriteAllText("../../config/delay_" + fMain.select_test + "_time.txt", cmd);
        }
        public void SetDisplayDelayExe(string cmd = "")
        {
            File.WriteAllText("../../config/delay_" + fMain.select_test + "_display.txt", cmd);
        }
        public void GenCommandUpFw()
        {
            string nameProgramPath = "DryiceProgram_";
            string pathCommand = Path.Combine("D:\\PathZero\\Command", nameProgramPath);
            string pathResult = Path.Combine("D:\\PathZero\\Result", nameProgramPath);

            try
            {
                File.Delete(pathResult + $"Result{fMain.select_test}.txt");
                File.WriteAllText(pathCommand + "Head.txt", fMain.select_test.ToString());
                File.WriteAllText(pathCommand + "CheckSum.txt", "0x0168EC74");
                File.WriteAllText(pathCommand + "Debug.txt", "False");
                File.WriteAllText(pathCommand + "Hex.txt", "TTULTRA_MU_16K_64K_v716.hex");
                if (fMain.select_test == 1)
                {
                    File.WriteAllText(pathCommand + "StLink.txt", "35001800120000334351534E");
                }
                else
                {
                    File.WriteAllText(pathCommand + "StLink.txt", "280019000B0000334351534E");
                }
                File.WriteAllText(pathCommand + "TimeOut.txt", "30000");
                File.WriteAllText(pathCommand + "Start.txt", string.Empty);
            }
            catch (Exception ex) { }

            string pathAck = pathResult + "Ack.txt";
            int timeout = 30000;
            DateTime startTime = DateTime.Now;

            while (true)
            {
                fMain.DelaymS(50);
                if (File.Exists(pathAck))
                {
                    try
                    {
                        File.ReadAllText(pathAck);
                        File.Delete(pathAck);
                        break;
                    }
                    catch (Exception ex) { continue; }
                }

                TimeSpan elapsedTime = DateTime.Now - startTime;
                if (elapsedTime.TotalMilliseconds >= timeout)
                {
                    fMain.UpdateResultToDataGrid(alice[0], "Ack file not found", "FAIL");
                    break;
                }
            }
        }

        public void Check_plc_connect(string ip, string port)
        {
            fMain.bt_reserve1.Text = "PLC";
            using (HaiwellPlc.HaiwellController plc = new HaiwellPlc.HaiwellController())
            {
                if (plc.Connect(ip, Convert.ToInt32(port)))
                {
                    fMain.bt_reserve1.BackColor = Color.LimeGreen;
                    plc.Disconnect();
                }
                else
                {
                    fMain.bt_reserve1.BackColor = Color.Red;
                }
            }
        }

        private static string _atsbaudRate;
        public void Check_ats_connect(string baudRate)
        {
            _atsbaudRate = baudRate;
            fMain.bt_reserve2.Text = "AC Source";
            _atsPortName = ATS1000B.AtsController.Scan(Convert.ToInt32(_atsbaudRate));
            fMain.bt_reserve2.BackColor = _atsPortName != null ? Color.LimeGreen : Color.Red;
        }

        public void Setup_ac_source()
        {
            string portName = _atsPortName ?? ATS1000B.AtsController.Scan(Convert.ToInt32(_atsbaudRate));
            if (portName == null) return;

            using (ATS1000B.AtsController ats = new ATS1000B.AtsController(portName, Convert.ToInt32(_atsbaudRate)))
            {
                ats.SetVoltage(120.0);
                ats.SetFrequency(60.0);
                ats.SetVoltageRange(0x02);  // High range
                ats.SetCurrentLimit(1.5);
                ats.SetRampUpTime(0.3);
                ats.SetOutput(false);
            }
        }

        public void Check_ni_connect()
        {
            fMain.bt_reserve3.Text = "NI DAQ";
            try
            {
                using (NIDAQTester12.DaqManager12 daq = new NIDAQTester12.DaqManager12())
                {
                    daq.Initialize(5000, 500);
                    daq.Start();
                    daq.Stop();
                }
                fMain.bt_reserve3.BackColor = Color.LimeGreen;
            }
            catch
            {
                fMain.bt_reserve3.BackColor = Color.Red;
            }
        }

        public void Check_camera_connect()
        {
            fMain.bt_reserve4.Text = "Camera";
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_PnPEntity WHERE PNPClass = 'Camera'"))
                {
                    fMain.bt_reserve4.BackColor = searcher.Get().Count >= 1
                        ? Color.LimeGreen
                        : Color.Red;
                }
            }
            catch
            {
                fMain.bt_reserve4.BackColor = Color.Red;
            }
        }

        public void Check_scanner_connect()
        {
            fMain.bt_reserve5.Text = "Scanner";
            _scannerPortName = null;
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%Newland%' AND Caption LIKE '%(COM%'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string caption = obj["Caption"]?.ToString() ?? "";
                        int start = caption.LastIndexOf("(COM");
                        if (start >= 0)
                        {
                            int end = caption.IndexOf(')', start);
                            if (end > start)
                            {
                                _scannerPortName = caption.Substring(start + 1, end - start - 1);
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
            fMain.bt_reserve5.BackColor = _scannerPortName != null ? Color.LimeGreen : Color.Red;
        }

        public void Scan_barcode()
        {
            lock (_panelSNLock)
            {
                if (_panelSNFailed)
                {
                    fMain.UpdateResultToDataGrid(alice[0], "Scan failed", "FAIL");
                    return;
                }
                if (!_panelSNScanned)
                {
                    if (_scannerPortName == null)
                    {
                        _panelSNFailed = true;
                        fMain.UpdateResultToDataGrid(alice[0], "No scanner", "FAIL");
                        return;
                    }
                    try
                    {
                        using (SerialPort port = new SerialPort(_scannerPortName, 9600, Parity.None, 8, StopBits.One)
                        {
                            WriteTimeout = 3000
                        })
                        {
                            port.Open();
                            port.DiscardInBuffer();
                            port.Write("SCAN\n");

                            DateTime deadline = DateTime.Now.AddMilliseconds(3000);
                            while (port.BytesToRead == 0)
                            {
                                if (DateTime.Now >= deadline)
                                    throw new TimeoutException();
                                System.Threading.Thread.Sleep(20);
                            }

                            System.Threading.Thread.Sleep(100);
                            _panelSN = port.ReadExisting().Trim();
                        }

                        fMain.Invoke((MethodInvoker)delegate
                        {
                            for (int head = 1; head <= fMain.configTester.numHead; head++)
                                fMain.getTextBoxSN(head).Text = _panelSN + "_" + head;
                        });

                        _panelSNScanned = true;
                    }
                    catch (TimeoutException)
                    {
                        _panelSNFailed = true;
                        fMain.UpdateResultToDataGrid(alice[0], "Timeout", "FAIL");
                        return;
                    }
                    catch (Exception ex)
                    {
                        _panelSNFailed = true;
                        fMain.UpdateResultToDataGrid(alice[0], ex.Message, "FAIL");
                        return;
                    }
                }
            }

            fMain.UpdateResultToDataGrid(alice[0], _panelSN, "PASS");
        }

        public void Enable_high_gear()
        {
            lock (_panelSNLock)
            {
                if (!_highGearReady)
                {
                    fMain.Relay_On(1, 3);
                    fMain.DelaymS(1500);
                    fMain.Relay_Off(1, 3);
                    _highGearReady = true;
                }
            }
            fMain.UpdateResultToDataGrid(alice[0], "High Gear", "PASS");
        }

        public void Enable_medium_gear()
        {
            lock (_panelSNLock)
            {
                if (!_mediumGearReady)
                {
                    fMain.Relay_On(1, 4);
                    fMain.DelaymS(1500);
                    fMain.Relay_Off(1, 4);
                    _mediumGearReady = true;
                }
            }
            fMain.UpdateResultToDataGrid(alice[0], "Medium Gear", "PASS");
        }

        public void Enable_low_gear()
        {
            lock (_panelSNLock)
            {
                if (!_lowGearReady)
                {
                    fMain.Relay_On(1, 5);
                    fMain.DelaymS(1500);
                    fMain.Relay_Off(1, 5);
                    _lowGearReady = true;
                }
            }
            fMain.UpdateResultToDataGrid(alice[0], "LOW Gear", "PASS");
        }

        public void Connect_acn_gnd()
        {
            lock (_panelSNLock)
            {
                if (!_acnGndReady)
                {
                    fMain.Relay_On(1, 7);
                    _acnGndReady = true;
                }
            }
            fMain.UpdateResultToDataGrid(alice[0], "ACN/GND", "PASS");
        }

        public void Disconnect_acn_gnd()
        {
            lock (_panelSNLock)
            {
                if (_acnGndReady)
                {
                    fMain.Relay_Off(1, 7);
                    _acnGndReady = false;
                }
            }
            fMain.UpdateResultToDataGrid(alice[0], "ACN/GND", "PASS");
        }

        public void Press_SW301()
        {
            lock (_panelSNLock)
            {
                if (!_sw301Ready)
                {
                    fMain.Relay_On(1, 6);
                    fMain.DelaymS(1500);
                    fMain.Relay_Off(1, 6);
                    _sw301Ready = true;
                }
            }
            fMain.UpdateResultToDataGrid(alice[0], "SW301", "PASS");
        }

        public void Power_on_ac()
        {
            lock (_panelSNLock)
            {
                if (!_relayReady)
                {
                    fMain.Relay_On(1, 1);
                    fMain.Relay_On(1, 2);
                    fMain.DelaymS(1000);
                    if (_atsPortName != null)
                    {
                        try
                        {
                            using (ATS1000B.AtsController ats = new ATS1000B.AtsController(_atsPortName, Convert.ToInt32(_atsbaudRate)))
                                ats.SetOutput(true);
                        }
                        catch { }
                    }
                    _relayReady = true;
                }
            }
            fMain.UpdateResultToDataGrid(alice[0], "AC ON", "PASS");
        }

        public void Measure_ac_current()
        {
            double minMa = double.TryParse(alice[2], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double lo) ? lo : 0;
            double maxMa = double.TryParse(alice[3], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double hi) ? hi : double.MaxValue;

            lock (_panelSNLock)
            {
                if (!_daqRunning)
                {
                    _daq = new NIDAQTester12.DaqManager12();
                    _daq.Initialize(5000, 500);
                    _daq.Start();
                    _daqRunning = true;
                }

                if (!_daqResultsReady)
                {
                    int count = fMain.configTester.numHead;
                    double[] fresh = new double[count];
                    try
                    {
                        DateTime deadline = DateTime.Now.AddSeconds(5);
                        do
                        {
                            var m = _daq.ReadBlock();
                            bool allPass = true;
                            for (int i = 0; i < count; i++)
                            {
                                fresh[i] = m.LineCurrentRms[i] * 1000.0;
                                if (fresh[i] < minMa || fresh[i] > maxMa)
                                    allPass = false;
                            }
                            _daqResults = fresh;
                            if (allPass) break;
                        } while (DateTime.Now < deadline);
                    }
                    catch { }
                    if (_daqResults == null) _daqResults = fresh;
                    _daqResultsReady = true;
                }
            }

            int boardIndex = fMain.select_test - 1;
            if (_daqResults == null || boardIndex >= _daqResults.Length)
            {
                fMain.UpdateResultToDataGrid(alice[0], "DAQ error", "FAIL");
                return;
            }
            double currentMa = _daqResults[boardIndex];
            bool pass = currentMa >= minMa && currentMa <= maxMa;
            fMain.UpdateResultToDataGrid(alice[0], currentMa.ToString("F2") + " mA", pass ? "PASS" : "FAIL");
        }

        public void Measure_tp37_voltage()
        {
            double minV = double.TryParse(alice[2], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double lo) ? lo : 0;
            double maxV = double.TryParse(alice[3], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double hi) ? hi : double.MaxValue;

            lock (_panelSNLock)
            {
                if (!_daqRunning)
                {
                    _daq = new NIDAQTester12.DaqManager12();
                    _daq.Initialize(5000, 500);
                    _daq.Start();
                    _daqRunning = true;
                }
                if (!_tp37ResultsReady)
                {
                    int count = fMain.configTester.numHead;
                    double[] fresh = new double[count];
                    try
                    {
                        var m = _daq.ReadBlock();
                        for (int i = 0; i < count; i++)
                            fresh[i] = m.Tp37Voltage[i];
                    }
                    catch { }
                    _tp37Results = fresh;
                    _tp37ResultsReady = true;
                }
            }

            int boardIndex = fMain.select_test - 1;
            if (_tp37Results == null || boardIndex >= _tp37Results.Length)
            {
                fMain.UpdateResultToDataGrid(alice[0], "DAQ error", "FAIL");
                return;
            }
            double voltage37 = _tp37Results[boardIndex];
            bool pass37 = voltage37 >= minV && voltage37 <= maxV;
            fMain.UpdateResultToDataGrid(alice[0], voltage37.ToString("F3") + " V", pass37 ? "PASS" : "FAIL");
        }

        public void Measure_tp33_voltage()
        {
            double minV = double.TryParse(alice[2], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double lo) ? lo : 0;
            double maxV = double.TryParse(alice[3], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double hi) ? hi : double.MaxValue;

            lock (_panelSNLock)
            {
                if (!_daqRunning)
                {
                    _daq = new NIDAQTester12.DaqManager12();
                    _daq.Initialize(5000, 500);
                    _daq.Start();
                    _daqRunning = true;
                }
                if (!_tp33ResultsReady)
                {
                    int count = fMain.configTester.numHead;
                    double[] fresh = new double[count];
                    try
                    {
                        var m = _daq.ReadBlock();
                        for (int i = 0; i < count; i++)
                            fresh[i] = m.Tp33Voltage[i];
                    }
                    catch { }
                    _tp33Results = fresh;
                    _tp33ResultsReady = true;
                }
            }

            int boardIndex = fMain.select_test - 1;
            if (_tp33Results == null || boardIndex >= _tp33Results.Length)
            {
                fMain.UpdateResultToDataGrid(alice[0], "DAQ error", "FAIL");
                return;
            }
            double voltage33 = _tp33Results[boardIndex];
            bool pass33 = voltage33 >= minV && voltage33 <= maxV;
            fMain.UpdateResultToDataGrid(alice[0], voltage33.ToString("F3") + " V", pass33 ? "PASS" : "FAIL");
        }

        public void Detect_LED_off(string timeout = "16")
        {
            lock (_panelSNLock)
            {
                if (!_ledDetectDone)
                {
                    const string pathResult = "led_detect_result.txt";
                    File.Delete(pathResult);

                    double timeoutSec = Convert.ToDouble(timeout);
                    string debugPath = "../../config/test_head_1_debug.txt";
                    bool isDebug = File.Exists(debugPath) &&
                                   File.ReadAllText(debugPath).Trim().Equals("True", StringComparison.OrdinalIgnoreCase);

                    camera_set_step("check_led_off");
                    set_timeout(isDebug ? "999999000" : ((int)((timeoutSec) * 1000)).ToString());

                    const string pathTrick = "call_exe_tric.txt";
                    File.Delete(pathTrick);

                    Process process = new Process();
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.FileName = "../../mini_projeck/camera_show.exe";
                    process.StartInfo.UseShellExecute = false;

                    try { process.Start(); } catch { }

                    DateTime trickDeadline = DateTime.Now.AddSeconds(10);
                    while (!File.Exists(pathTrick) && DateTime.Now < trickDeadline && fMain.GlobalTestingFlag[fMain.select_test - 1])
                        fMain.DelaymS(100);

                    if (isDebug)
                    {
                        while (!File.Exists(pathResult) && fMain.GlobalTestingFlag[fMain.select_test - 1])
                            fMain.DelaymS(100);
                    }
                    else
                    {
                        DateTime deadline = DateTime.Now.AddSeconds(timeoutSec + 10);
                        while (!File.Exists(pathResult) && DateTime.Now < deadline)
                            fMain.DelaymS(100);
                    }

                    int count = fMain.configTester.numHead;
                    _ledResults = new string[count];
                    try
                    {
                        string[] lines = File.ReadAllLines(pathResult);
                        for (int i = 0; i < count && i < lines.Length; i++)
                            _ledResults[i] = lines[i];
                    }
                    catch { }

                    _ledDetectDone = true;
                }
            }

            int boardIndex = fMain.select_test - 1;
            if (_ledResults == null || boardIndex >= _ledResults.Length || _ledResults[boardIndex] == null)
            {
                fMain.UpdateResultToDataGrid(alice[0], "Camera error", "FAIL");
                return;
            }

            // camera returns FAIL when it cannot detect the color (LED is off) → that is what we want → PASS
            string[] parts = _ledResults[boardIndex].Split(new[] { ' ' }, 2);
            string status = parts.Length >= 2 ? parts[1].Trim() : "PASS";
            bool ledIsOff = status == "FAIL";
            fMain.UpdateResultToDataGrid(alice[0], ledIsOff ? "LED OFF" : "LED ON", ledIsOff ? "PASS" : "FAIL");
        }

        public void Detect_LED_on(string timeout = "16")
        {
            lock (_panelSNLock)
            {
                if (!_ledOnDetectDone)
                {
                    const string pathResult = "led_detect_result.txt";
                    File.Delete(pathResult);

                    double timeoutSec = Convert.ToDouble(timeout);
                    string debugPath = "../../config/test_head_1_debug.txt";
                    bool isDebug = File.Exists(debugPath) &&
                                   File.ReadAllText(debugPath).Trim().Equals("True", StringComparison.OrdinalIgnoreCase);

                    camera_set_step("check_led_on");
                    set_timeout(isDebug ? "999999000" : ((int)((timeoutSec) * 1000)).ToString());

                    const string pathTrick = "call_exe_tric.txt";
                    File.Delete(pathTrick);

                    Process process = new Process();
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.FileName = "../../mini_projeck/camera_show.exe";
                    process.StartInfo.UseShellExecute = false;

                    try { process.Start(); } catch { }

                    DateTime trickDeadline = DateTime.Now.AddSeconds(10);
                    while (!File.Exists(pathTrick) && DateTime.Now < trickDeadline && fMain.GlobalTestingFlag[fMain.select_test - 1])
                        fMain.DelaymS(100);

                    if (isDebug)
                    {
                        while (!File.Exists(pathResult) && fMain.GlobalTestingFlag[fMain.select_test - 1])
                            fMain.DelaymS(100);
                    }
                    else
                    {
                        DateTime deadline = DateTime.Now.AddSeconds(timeoutSec + 10);
                        while (!File.Exists(pathResult) && DateTime.Now < deadline)
                            fMain.DelaymS(100);
                    }

                    int count = fMain.configTester.numHead;
                    _ledOnResults = new string[count];
                    try
                    {
                        string[] lines = File.ReadAllLines(pathResult);
                        for (int i = 0; i < count && i < lines.Length; i++)
                            _ledOnResults[i] = lines[i];
                    }
                    catch { }

                    _ledOnDetectDone = true;
                }
            }

            int boardIndex = fMain.select_test - 1;
            if (_ledOnResults == null || boardIndex >= _ledOnResults.Length || _ledOnResults[boardIndex] == null)
            {
                fMain.UpdateResultToDataGrid(alice[0], "Camera error", "FAIL");
                return;
            }

            string[] onParts = _ledOnResults[boardIndex].Split(new[] { ' ' }, 2);
            string onStatus = onParts.Length >= 2 ? onParts[1].Trim() : "FAIL";
            bool ledIsOn = onStatus == "PASS";
            fMain.UpdateResultToDataGrid(alice[0], ledIsOn ? "LED ON" : "LED OFF", ledIsOn ? "PASS" : "FAIL");
        }

        public void AAA(string fff)
        {
            MessageBox.Show(fff);
        }
        public void BBB(string fff)
        {
            MessageBox.Show(fff);
        }

        #endregion
    }
}
