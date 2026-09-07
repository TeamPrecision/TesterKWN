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
        #region ========================================================= Common Function ==========================================================
        public void ClearDataGridView(int head)
        {
            DataGridView g = getDataGridView(head);

            int count = 0;

            do //Clear data gridview
            {
                g.Rows[count].Cells[define.dataGrid.columnMeasure].Value = "";
                g.Rows[count].Cells[define.dataGrid.columnResult].Value = "";
                count++;

            } while (count < g.Rows.Count);

            g.Rows.Clear();
            g.Refresh();
            row[head - 1] = 0;
            if (_labelProgressInfos != null)
            {
                _labelProgressInfos[head - 1].Text = "";
                _labelProgressInfos[head - 1].ForeColor = SystemColors.ControlText;
            }
        }
        private string CheckFailure(int head)
        {
            DataGridView g = getDataGridView(head);

            string result = "";
            string failure = "";

            for (int i = 0; i < g.Rows.Count; i++)
            {
                try
                {
                    result = g.Rows[i].Cells[define.dataGrid.columnResult].Value.ToString();

                    if (result == define.fail)
                    {
                        failure = failure + "<>" + g.Rows[i].Cells[define.dataGrid.columnStep].Value.ToString();
                    }

                }
                catch (Exception ex) { Trace.TraceWarning("CheckFailure: failed reading cell at row {0}: {1}", i, ex.Message); }
            }

            return failure;
        }
        private void CreateFolder_datalog()
        {
            Folder.list.Clear();

            Folder.list.Add(Folder.driveD + this.Text + Folder.dataBase + tb_detail.Text);
            Folder.list.Add(Folder.list[0] + Folder.operationComplete);
            Folder.list.Add(Folder.list[0] + Folder.operationInComplete);
            Folder.list.Add(Folder.list[0] + Folder.debugComplete);
            Folder.list.Add(Folder.list[0] + Folder.debugInComplete);
            Folder.list.Add(Folder.dataMIS);
            Folder.list.Add(Folder.driveD + this.Text + Folder.dataBase + Folder.timeLine);

            for (int i = 1; i <= 4; i++)
            {

                if (excel.sameStep)
                {

                    if (!Directory.Exists(Folder.list[i] + "Head All"))
                    {
                        Directory.CreateDirectory(Folder.list[i] + "Head All");
                    }

                }
                else
                {

                    for (int j = 1; j <= configTester.numHead; j++)
                    {

                        if (!Directory.Exists(Folder.list[i] + "Head " + j))
                        {
                            Directory.CreateDirectory(Folder.list[i] + "Head " + j);
                        }

                    }
                }
            }

            if (!Directory.Exists(Folder.list[5]))
            {
                Directory.CreateDirectory(Folder.list[5]);
            }

            if (!Directory.Exists(Folder.list[6]))
            {
                Directory.CreateDirectory(Folder.list[6]);
            }

            if (!Directory.Exists("D:\\Datalog to SQL"))
            {
                Directory.CreateDirectory("D:\\Datalog to SQL");
            }
        }
        private void InitHeadDataGridViewMap()
        {
            _headDataGridViewMap = new Dictionary<string, DataGridView>
            {
                ["Head 1\\"]   = dataGridView_1,
                ["Head 2\\"]   = dataGridView_2,
                ["Head 3\\"]   = dataGridView_3,
                ["Head 4\\"]   = dataGridView_4,
                ["Head 5\\"]   = dataGridView_5,
                ["Head 6\\"]   = dataGridView_6,
                ["Head 7\\"]   = dataGridView_7,
                ["Head 8\\"]   = dataGridView_8,
                ["Head 9\\"]   = dataGridView_9,
                ["Head 10\\"]  = dataGridView_10,
                ["Head 11\\"]  = dataGridView_11,
                ["Head 12\\"]  = dataGridView_12,
                ["Head 13\\"]  = dataGridView_13,
                ["Head 14\\"]  = dataGridView_14,
                ["Head 15\\"]  = dataGridView_15,
                ["Head 16\\"]  = dataGridView_16,
                ["Head 17\\"]  = dataGridView_17,
                ["Head 18\\"]  = dataGridView_18,
                ["Head 19\\"]  = dataGridView_19,
                ["Head 20\\"]  = dataGridView_20,
                ["Head 21\\"]  = dataGridView_21,
                ["Head 22\\"]  = dataGridView_22,
                ["Head 23\\"]  = dataGridView_23,
                ["Head 24\\"]  = dataGridView_24,
                ["Head 25\\"]  = dataGridView_25,
                ["Head 26\\"]  = dataGridView_26,
                ["Head 27\\"]  = dataGridView_27,
                ["Head 28\\"]  = dataGridView_28,
                ["Head 29\\"]  = dataGridView_29,
                ["Head 30\\"]  = dataGridView_30,
                ["Head 31\\"]  = dataGridView_31,
                ["Head 32\\"]  = dataGridView_32,
                ["Head 33\\"]  = dataGridView_33,
                ["Head 34\\"]  = dataGridView_34,
                ["Head 35\\"]  = dataGridView_35,
                ["Head 36\\"]  = dataGridView_36,
                ["Head All\\"] = dataGridView_1,
            };

            _dataGridViews   = new[] { dataGridView_1, dataGridView_2, dataGridView_3, dataGridView_4, dataGridView_5, dataGridView_6, dataGridView_7, dataGridView_8, dataGridView_9, dataGridView_10, dataGridView_11, dataGridView_12, dataGridView_13, dataGridView_14, dataGridView_15, dataGridView_16, dataGridView_17, dataGridView_18, dataGridView_19, dataGridView_20, dataGridView_21, dataGridView_22, dataGridView_23, dataGridView_24, dataGridView_25, dataGridView_26, dataGridView_27, dataGridView_28, dataGridView_29, dataGridView_30, dataGridView_31, dataGridView_32, dataGridView_33, dataGridView_34, dataGridView_35, dataGridView_36 };
            _textBoxSNs      = new[] { txtSNBoard_1, txtSNBoard_2, txtSNBoard_3, txtSNBoard_4, txtSNBoard_5, txtSNBoard_6, txtSNBoard_7, txtSNBoard_8, txtSNBoard_9, txtSNBoard_10, txtSNBoard_11, txtSNBoard_12, txtSNBoard_13, txtSNBoard_14, txtSNBoard_15, txtSNBoard_16, txtSNBoard_17, txtSNBoard_18, txtSNBoard_19, txtSNBoard_20, txtSNBoard_21, txtSNBoard_22, txtSNBoard_23, txtSNBoard_24, txtSNBoard_25, txtSNBoard_26, txtSNBoard_27, txtSNBoard_28, txtSNBoard_29, txtSNBoard_30, txtSNBoard_31, txtSNBoard_32, txtSNBoard_33, txtSNBoard_34, txtSNBoard_35, txtSNBoard_36 };
            _labelStatuses   = new[] { status_1, status_2, status_3, status_4, status_5, status_6, status_7, status_8, status_9, status_10, status_11, status_12, status_13, status_14, status_15, status_16, status_17, status_18, status_19, status_20, status_21, status_22, status_23, status_24, status_25, status_26, status_27, status_28, status_29, status_30, status_31, status_32, status_33, status_34, status_35, status_36 };
            _labelTestTimes  = new[] { lblTestTime_1, lblTestTime_2, lblTestTime_3, lblTestTime_4, lblTestTime_5, lblTestTime_6, lblTestTime_7, lblTestTime_8, lblTestTime_9, lblTestTime_10, lblTestTime_11, lblTestTime_12, lblTestTime_13, lblTestTime_14, lblTestTime_15, lblTestTime_16, lblTestTime_17, lblTestTime_18, lblTestTime_19, lblTestTime_20, lblTestTime_21, lblTestTime_22, lblTestTime_23, lblTestTime_24, lblTestTime_25, lblTestTime_26, lblTestTime_27, lblTestTime_28, lblTestTime_29, lblTestTime_30, lblTestTime_31, lblTestTime_32, lblTestTime_33, lblTestTime_34, lblTestTime_35, lblTestTime_36 };
            _labelInOutTimes = new[] { lblLoadInOutTime_1, lblLoadInOutTime_2, lblLoadInOutTime_3, lblLoadInOutTime_4, lblLoadInOutTime_5, lblLoadInOutTime_6, lblLoadInOutTime_7, lblLoadInOutTime_8, lblLoadInOutTime_9, lblLoadInOutTime_10, lblLoadInOutTime_11, lblLoadInOutTime_12, lblLoadInOutTime_13, lblLoadInOutTime_14, lblLoadInOutTime_15, lblLoadInOutTime_16, lblLoadInOutTime_17, lblLoadInOutTime_18, lblLoadInOutTime_19, lblLoadInOutTime_20, lblLoadInOutTime_21, lblLoadInOutTime_22, lblLoadInOutTime_23, lblLoadInOutTime_24, lblLoadInOutTime_25, lblLoadInOutTime_26, lblLoadInOutTime_27, lblLoadInOutTime_28, lblLoadInOutTime_29, lblLoadInOutTime_30, lblLoadInOutTime_31, lblLoadInOutTime_32, lblLoadInOutTime_33, lblLoadInOutTime_34, lblLoadInOutTime_35, lblLoadInOutTime_36 };
            _buttonTests     = new[] { btnTEST_1, btnTEST_2, btnTEST_3, btnTEST_4, btnTEST_5, btnTEST_6, btnTEST_7, btnTEST_8, btnTEST_9, btnTEST_10, btnTEST_11, btnTEST_12, btnTEST_13, btnTEST_14, btnTEST_15, btnTEST_16, btnTEST_17, btnTEST_18, btnTEST_19, btnTEST_20, btnTEST_21, btnTEST_22, btnTEST_23, btnTEST_24, btnTEST_25, btnTEST_26, btnTEST_27, btnTEST_28, btnTEST_29, btnTEST_30, btnTEST_31, btnTEST_32, btnTEST_33, btnTEST_34, btnTEST_35, btnTEST_36 };
            _buttonExits     = new[] { btnEXIT_1, btnEXIT_2, btnEXIT_3, btnEXIT_4, btnEXIT_5, btnEXIT_6, btnEXIT_7, btnEXIT_8, btnEXIT_9, btnEXIT_10, btnEXIT_11, btnEXIT_12, btnEXIT_13, btnEXIT_14, btnEXIT_15, btnEXIT_16, btnEXIT_17, btnEXIT_18, btnEXIT_19, btnEXIT_20, btnEXIT_21, btnEXIT_22, btnEXIT_23, btnEXIT_24, btnEXIT_25, btnEXIT_26, btnEXIT_27, btnEXIT_28, btnEXIT_29, btnEXIT_30, btnEXIT_31, btnEXIT_32, btnEXIT_33, btnEXIT_34, btnEXIT_35, btnEXIT_36 };
            _progressBars    = new[] { progressBar_1, progressBar_2, progressBar_3, progressBar_4, progressBar_5, progressBar_6, progressBar_7, progressBar_8, progressBar_9, progressBar_10, progressBar_11, progressBar_12, progressBar_13, progressBar_14, progressBar_15, progressBar_16, progressBar_17, progressBar_18, progressBar_19, progressBar_20, progressBar_21, progressBar_22, progressBar_23, progressBar_24, progressBar_25, progressBar_26, progressBar_27, progressBar_28, progressBar_29, progressBar_30, progressBar_31, progressBar_32, progressBar_33, progressBar_34, progressBar_35, progressBar_36 };
            _debugMenuItems  = new[] { set_debug_1, set_debug_2, set_debug_3, set_debug_4, set_debug_5, set_debug_6, set_debug_7, set_debug_8, set_debug_9, set_debug_10, set_debug_11, set_debug_12, set_debug_13, set_debug_14, set_debug_15, set_debug_16, set_debug_17, set_debug_18, set_debug_19, set_debug_20, set_debug_21, set_debug_22, set_debug_23, set_debug_24, set_debug_25, set_debug_26, set_debug_27, set_debug_28, set_debug_29, set_debug_30, set_debug_31, set_debug_32, set_debug_33, set_debug_34, set_debug_35, set_debug_36 };
            _groupBoxHeads   = new[] { groupBox_head1, groupBox_head2, groupBox_head3, groupBox_head4, groupBox_head5, groupBox_head6, groupBox_head7, groupBox_head8, groupBox_head9, groupBox_head10, groupBox_head11, groupBox_head12, groupBox_head13, groupBox_head14, groupBox_head15, groupBox_head16, groupBox_head17, groupBox_head18, groupBox_head19, groupBox_head20, groupBox_head21, groupBox_head22, groupBox_head23, groupBox_head24, groupBox_head25, groupBox_head26, groupBox_head27, groupBox_head28, groupBox_head29, groupBox_head30, groupBox_head31, groupBox_head32, groupBox_head33, groupBox_head34, groupBox_head35, groupBox_head36 };

            _labelProgressInfos = new Label[36];
            for (int i = 0; i < 36; i++)
            {
                DataGridView dgv = _dataGridViews[i];
                Label lbl = new Label
                {
                    Location  = dgv.Location,
                    Size      = dgv.Size,
                    Anchor    = dgv.Anchor,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font      = new Font("Microsoft Sans Serif", 9f, FontStyle.Bold),
                    BackColor = SystemColors.ButtonFace,
                    Visible   = false,
                    Text      = ""
                };
                _groupBoxHeads[i].Controls.Add(lbl);
                _labelProgressInfos[i] = lbl;
            }
        }
        private DataGridView getDataGridViewByHeader(string headFile)
        {
            return _headDataGridViewMap.TryGetValue(headFile, out DataGridView g) ? g : new DataGridView();
        }
        private int getRowArray_CreateHeader(string headFile)
        {
            if (headFile == "Head All\\") return 1;
            string numPart = headFile.Replace("Head ", "").TrimEnd('\\').Trim();
            return int.TryParse(numPart, out int n) ? n : 1;
        }
        private void CreateHeader_datalog(string fileName, string headFile)
        {
            string headTimeLine = "";
            string Header = null;
            string csvPath = null;
            int count = 0;
            DataGridView g = getDataGridViewByHeader(headFile);

            if (configTester.saveData == configTester.saveNormal)
            {

                do
                {
                    if (!string.IsNullOrEmpty(g.Rows[count].Cells[2].Value as string))
                    {

                        Header = Header + "STEP" + g.Rows[count].Cells[0].Value + "(" +
                            g.Rows[count].Cells[2].Value.ToString().Replace(",", " ").Replace("\n", " ") + ")" +
                            g.Rows[count].Cells[1].Value.ToString().Replace(",", " ") + ",";
                    }

                    count++;
                } while (count < g.Rows.Count);

                row[getRowArray_CreateHeader(headFile) - 1] = 0;

                headTimeLine = dataLog.headLog + dataLog.headLog_header + dataLog.headLog_fgAndWo + Header;
                Header = dataLog.headLog_header + Header;
                Header = dataLog.headLog + Header;

            }
            else
            {

                //ตรงนี้เป็นกรณีที่ save data แบบพิเศษ ใช้แค่โปรเจก denali ในอนาคตตรงนี้อาจจะเอาออก
                string[] headSpecial = File.ReadAllLines(excel.pathFile + cbb_fg.Text + dataLog.lastNameTXT);

                foreach (string headSplit in headSpecial)
                {
                    Header += headSplit + ",";
                }

                headTimeLine = Header;
            }


            //get header มันจะต้องทำทั้งหมด 4 file 
            for (int indexFolder = 1; indexFolder <= 4; indexFolder++)
            {

                bool checkHead = CheckHeader(Header, fileName, headFile, indexFolder);
                csvPath = Folder.list[indexFolder] + headFile + fileName + dataLog.lastNameCSV;

                if (!File.Exists(csvPath) || !checkHead)
                {

                    StreamWriter streamWriter = new StreamWriter(csvPath, true);
                    streamWriter.WriteLine(Header);
                    streamWriter.Close();
                }
            }

            //อันนี้เป็นการ get header ของ log time line
            bool checkHeadTimeLine = CheckHeader(headTimeLine, dataLog.timeLine.nameFile + dataLog.timeLine.numFile, "", 6);
            string pathDataTimeLine = Folder.list[6] + dataLog.timeLine.nameFile + dataLog.timeLine.numFile + dataLog.lastNameCSV;

            if (!File.Exists(pathDataTimeLine) || !checkHeadTimeLine)
            {

                StreamWriter streamWriter = new StreamWriter(pathDataTimeLine, true);
                streamWriter.WriteLine(headTimeLine);
                streamWriter.Close();
            }
        }
        public DataGridView getDataGridView(int head) => _dataGridViews[head - 1];
        public TextBox getTextBoxSN(int head) => _textBoxSNs[head - 1];
        public Label getLabelStatus(int head) => _labelStatuses[head - 1];
        public Label getLabelTestTime(int head) => _labelTestTimes[head - 1];
        public Label getLabelInOutTime(int head) => _labelInOutTimes[head - 1];
        public string getHeadFile(int head) => $"Head {head}\\";
        public Button getButtonTest(int head) => _buttonTests[head - 1];
        public Button getButtonExit(int head) => _buttonExits[head - 1];
        public ProgressBar getProgressBar(int head) => _progressBars[head - 1];
        public ToolStripMenuItem getToolStripMenuItemDebug(int head) => _debugMenuItems[head - 1];
        public GroupBox getGroupBoxHead(int head) => _groupBoxHeads[head - 1];
        private bool getResultTest(DataGridView gridView)
        {
            bool results = true;

            if (flagNotReTest[select_test - 1])
            {
                return true;
            }

            for (int i = 0; i < gridView.Rows.Count - 1; i++)
            {
                try
                {

                    if (gridView.Rows[i].Cells[define.dataGrid.columnResult].Value.ToString() != define.pass &&
                        gridView.Rows[i].Cells[define.dataGrid.columnResult].Value.ToString() != "")
                    {
                        results = false;
                        break;
                    }

                }
                catch
                {
                    results = false;
                    break;
                }
            }

            return results;
        }
        private string getDataSummarySpecial(DataGridView gridView, string Detial, string checkFailure, bool results, DateTime dateTime,
            TextBox textboxSN, string csvPath, string headFile, string fileName, Label labelTestTime, int head)
        {
            string dataSummary = "";

            //อันนี้เป็นแบบ พิเศษ ใช้แค่ denali ในอนาคตอาจจะเอาออก
            string[] headLog = File.ReadAllLines(excel.pathFile + cbb_fg.Text + "_step.txt");
            List<int> rowDataGridViewList = new List<int>();

            for (int i = 0; i < gridView.Rows.Count; i++)
            {
                rowDataGridViewList.Add(i);
            }

            foreach (string headLogSplit in headLog)
            {
                if (headLogSplit == "non")
                {
                    Detial += "-,";
                    continue;
                }

                if (headLogSplit.Contains("data"))
                {
                    switch (headLogSplit)
                    {
                        case "data_fail":
                            Detial += checkFailure.Replace(",", "") + ",";
                            break;
                        case "data_Final_Result":
                            if (results)
                            {
                                Detial += "PASS,";
                            }
                            else
                            {
                                Detial += "FAIL,";
                            }
                            break;
                        case "data_DATE_TIME":
                            Detial += dateTime.ToString(DateTimePay.format, CultureInfo.CreateSpecificCulture(DateTimePay.us));
                            break;
                        case "data_FG":
                            Detial += cbb_fg.Text + ",";
                            break;
                        case "data_WO":
                            try
                            {
                                Detial += tb_wo.Text + ",";
                            }
                            catch
                            {
                                Detial += ",";
                            }
                            break;
                        case "data_TESTER_ID":
                            Detial += setupPay.read_text(ConfigTester.Header.komsonTester, configTester.nameFile) + " (head " + head + "),";
                            break;
                        case "data_prism_number":
                            try
                            {
                                Detial += textboxSN.Text + ",";
                            }
                            catch
                            {
                                Detial += ",";
                            }
                            break;
                        case "data_Operator":
                            try
                            {
                                Detial += tb_userID.Text + ",";
                            }
                            catch
                            {
                                Detial += ",";
                            }
                            break;
                        case "data_mode":
                            if (cb_OperationMode.Checked)
                            {
                                Detial += configPrism.OperationMode + ",";
                                if (results) csvPath = Folder.list[1] + headFile + fileName + dataLog.lastNameCSV;
                                else csvPath = Folder.list[2] + headFile + fileName + dataLog.lastNameCSV;
                            }
                            else
                            {
                                Detial += configPrism.DebugMode + ",";
                                if (results) csvPath = Folder.list[3] + headFile + fileName + dataLog.lastNameCSV;
                                else csvPath = Folder.list[4] + headFile + fileName + dataLog.lastNameCSV;
                            }
                            break;
                        case "data_Test_Finish_Time":
                            Detial += dateTime.Year.ToString() + "." + dateTime.Month.ToString("00") + "." +
                                dateTime.Day.ToString("00") + " " + dateTime.ToString("T") + ",";
                            break;
                        case "data_Test_Total_Time":
                            Detial += TimeSpan.FromSeconds(Convert.ToInt32(labelTestTime.Text)).ToString(@"hh\:mm\:ss") + ",";
                            break;
                        case "data_Test_Start_Time":
                            Detial += time_start[select_test - 1] + ",";
                            break;
                    }
                    continue;
                }

                string[] headLogArray = headLogSplit.Split(',');

                if (headLogArray.Length == 1)
                {
                    foreach (int rowSplit in rowDataGridViewList)
                    {
                        string stepGridView = "";

                        try
                        {
                            stepGridView = gridView.Rows[rowSplit].Cells[define.dataGrid.columnStep].Value.ToString();
                        }
                        catch { }

                        if (stepGridView == headLogArray[0])
                        {
                            try
                            {
                                if (gridView.Rows[rowSplit].Cells[define.dataGrid.columnMeasure].Value.ToString().Length > 12)
                                {
                                    Detial += "'" + gridView.Rows[rowSplit].Cells[define.dataGrid.columnMeasure].Value.ToString() + ",";

                                }
                                else
                                {
                                    Detial += gridView.Rows[rowSplit].Cells[define.dataGrid.columnMeasure].Value.ToString() + ",";
                                }

                            }
                            catch
                            {
                                Detial += ",";
                            }

                            rowDataGridViewList.Remove(rowSplit);
                            break;
                        }
                    }

                }
                else
                {
                    foreach (string s_split in headLogArray)
                    {
                        foreach (int nt in rowDataGridViewList)
                        {
                            if (gridView.Rows[nt].Cells[0].Value.ToString() == s_split)
                            {
                                try
                                {
                                    if (gridView.Rows[nt].Cells[3].Value.ToString().Length > 12) Detial += "'" + gridView.Rows[nt].Cells[3].Value.ToString() + " ";
                                    else Detial += gridView.Rows[nt].Cells[3].Value.ToString() + " ";
                                }
                                catch { Detial += ","; }
                                rowDataGridViewList.Remove(nt);
                                break;
                            }
                        }
                    }
                    Detial += ",";
                }
            }
            dataSummary = Detial;

            return dataSummary;
        }
        private void save_data(int head)
        {
            DataGridView gridView = getDataGridView(head);
            TextBox textboxSN = getTextBoxSN(head);
            Label labelStatus = getLabelStatus(head);
            Label labelTestTime = getLabelTestTime(head);
            Label labelInOutTime = getLabelInOutTime(head);
            string headFile = getHeadFile(head);
            bool results = getResultTest(gridView);

            if (excel.sameStep)
            {
                headFile = "Head All\\";
            }

            if (results)
            {
                labelStatus.Text = define.pass;
                if (configUI.lbStatus == ConfigUI.LbStatus.ForeColor)
                {
                    labelStatus.ForeColor = Color.Green;
                }
                else
                {
                    labelStatus.BackColor = Color.Green;
                    labelStatus.ForeColor = Color.White;
                }
            }
            else
            {
                labelStatus.Text = define.fail;
                if (configUI.lbStatus == ConfigUI.LbStatus.ForeColor)
                {
                    labelStatus.ForeColor = Color.Red;
                }
                else
                {
                    labelStatus.BackColor = Color.Red;
                    labelStatus.ForeColor = Color.White;
                }
            }

            string csvPath = "";
            string dataSummary = "";
            string Detial = "";
            string fileName = "";
            DateTime dateTime = DateTime.Now;

            //แปลงเป็นแบบของไทย ทำเก็บไว้เผื่อมีโอกาสได้ใช้
            //ThaiBuddhistCalendar calTime = new ThaiBuddhistCalendar();
            //DateTime dateTimeThai = new DateTime(calTime.GetYear(dateTime), calTime.GetMonth(dateTime), dateTime.Day);

            if (configPrism.mode == configPrism.Operation)
            {
                fileName = tb_wo.Text.Replace("/", "_");
            }
            else
            {
                fileName = configPrism.Debug;
            }

            CreateHeader_datalog(fileName, headFile);
            string checkFailure = CheckFailure(head);

            if (configTester.saveData == configTester.saveNormal)
            {
                Detial = dateTime.ToString(DateTimePay.format, CultureInfo.CreateSpecificCulture(DateTimePay.us));
                Detial += tb_userID.Text + ",";
                Detial += tb_swVersion.Text + ",";
                Detial += tb_fwVersion.Text + ",";
                Detial += tb_spec.Text + ",";
                Detial += labelTestTime.Text + ",";
                Detial += labelInOutTime.Text + ",";

                if (cb_OperationMode.Checked)
                {
                    Detial = Detial + configPrism.OperationMode + ",";

                    if (results)
                    {
                        csvPath = Folder.list[1] + headFile + fileName + dataLog.lastNameCSV;

                    }
                    else
                    {
                        csvPath = Folder.list[2] + headFile + fileName + dataLog.lastNameCSV;
                    }

                }
                else
                {
                    Detial = Detial + configPrism.DebugMode + ",";

                    if (results)
                    {
                        csvPath = Folder.list[3] + headFile + fileName + dataLog.lastNameCSV;

                    }
                    else
                    {
                        csvPath = Folder.list[4] + headFile + fileName + dataLog.lastNameCSV;
                    }
                }

                Detial = Detial + labelStatus.Text + "," + textboxSN.Text + "," + checkFailure;
                Detial += "," + "Head " + head;

                for (int i = 0; i < gridView.Rows.Count; i++)
                {
                    try
                    {
                        if ((gridView.Rows[i].Cells[2].Value.ToString() != ""))
                        {

                            if (gridView.Rows[i].Cells[3].Value == null)
                            {
                                dataSummary = dataSummary + "," + "";

                            }
                            else
                            {
                                dataSummary = dataSummary + "," + gridView.Rows[i].Cells[3].Value.ToString().Replace(",", " ");
                            }

                        }
                    }
                    catch { }
                }

                dataLog.timeLine.data = Detial + "," + cbb_fg.Text + "," + tb_wo.Text + dataSummary;
                dataSummary = Detial + dataSummary;

            }
            else
            {

                string[] sup = File.ReadAllLines("../../TestDescription/" + cbb_fg.Text + "_step.txt");
                List<Int32> row_data = new List<int>();
                for (int i = 0; i < gridView.Rows.Count; i++)
                {
                    row_data.Add(i);
                }
                foreach (string s in sup)
                {
                    if (s == "non") { Detial += "-,"; continue; }
                    if (s.Contains("data"))
                    {
                        switch (s)
                        {
                            case "data_fail": Detial += checkFailure.Replace(",", "") + ","; break;
                            case "data_Final_Result":
                                if (results) Detial += "PASS,";
                                else Detial += "FAIL,";
                                break;
                            case "data_DATE_TIME": Detial += DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss,", System.Globalization.CultureInfo.CreateSpecificCulture("en-US")); break;
                            case "data_FG": Detial += cbb_fg.Text + ","; break;
                            case "data_WO":
                                try { Detial += tb_wo.Text + ","; } catch { Detial += ","; }
                                break;
                            case "data_TESTER_ID": Detial += File.ReadAllText("tester_no.txt") + " (head " + head + "),"; break;
                            case "data_prism_number":
                                try { Detial += textboxSN.Text + ","; } catch { Detial += ","; }
                                break;
                            case "data_Operator":
                                try { Detial += tb_userID.Text + ","; } catch { Detial += ","; }
                                break;
                            case "data_mode":
                                if (cb_OperationMode.Checked)
                                {
                                    Detial += "Operation Mode" + ",";
                                    if (results) csvPath = Folder.list[1] + headFile + fileName + dataLog.lastNameCSV;
                                    else csvPath = Folder.list[2] + headFile + fileName + dataLog.lastNameCSV;
                                }
                                else
                                {
                                    Detial += "Debug Mode" + ",";
                                    if (results) csvPath = Folder.list[3] + headFile + fileName + dataLog.lastNameCSV;
                                    else csvPath = Folder.list[4] + headFile + fileName + dataLog.lastNameCSV;
                                }
                                break;
                            case "data_Test_Finish_Time":
                                Detial += dateTime.Year.ToString() + "." +
                                    dateTime.Month.ToString("00") + "." + dateTime.Day.ToString("00") + " " +
                                    dateTime.ToString("T") + ","; break;
                            case "data_Test_Total_Time": Detial += TimeSpan.FromSeconds(Convert.ToInt32(labelTestTime.Text)).ToString(@"hh\:mm\:ss") + ","; break;
                            case "data_Test_Start_Time": Detial += time_start[select_test - 1] + ","; break;
                        }
                        continue;
                    }
                    string[] split_s = s.Split(',');
                    if (split_s.Length == 1)
                    {
                        foreach (int nt in row_data)
                        {
                            string jj = "";
                            try { jj = gridView.Rows[nt].Cells[0].Value.ToString(); } catch { }
                            if (jj == split_s[0])
                            {
                                try
                                {
                                    if (gridView.Rows[nt].Cells[3].Value.ToString().Length > 12)
                                        Detial += "'" + gridView.Rows[nt].Cells[3].Value.ToString() + ",";
                                    else
                                        Detial += gridView.Rows[nt].Cells[3].Value.ToString() + ",";
                                }
                                catch
                                {
                                    Detial += ",";
                                }
                                row_data.Remove(nt);
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (string s_split in split_s)
                        {
                            foreach (int nt in row_data)
                            {
                                if (gridView.Rows[nt].Cells[0].Value.ToString() == s_split)
                                {
                                    try
                                    {
                                        if (gridView.Rows[nt].Cells[3].Value.ToString().Length > 12)
                                            Detial += "'" + gridView.Rows[nt].Cells[3].Value.ToString() + " ";
                                        else
                                            Detial += gridView.Rows[nt].Cells[3].Value.ToString() + " ";
                                    }
                                    catch { }
                                    row_data.Remove(nt);
                                    break;
                                }
                            }
                        }
                        Detial += ",";
                    }
                }

                dataLog.timeLine.data = Detial;
                dataSummary = Detial;
            }

            StreamWriter swOut = new StreamWriter(csvPath, true);
            while (true)
            {
                try
                {
                    swOut.WriteLine(dataSummary);
                }
                catch
                {
                    MessageBox.Show("_กรุณาปิด log file csv ก่อน");
                    continue;
                }
                break;
            }
            swOut.Close();

            try
            {
                dataLog.timeLine.rowCSV = Convert.ToDouble(File.ReadAllText(Folder.list[6] + dataLog.timeLine.nameFileRow));
                dataLog.timeLine.rowCSV++;
                File.WriteAllText(Folder.list[6] + dataLog.timeLine.nameFileRow, dataLog.timeLine.rowCSV.ToString());
            }
            catch
            {
                dataLog.timeLine.rowCSV = 1;
                File.WriteAllText(Folder.list[6] + dataLog.timeLine.nameFileRow, 1.ToString());
            }
            if (dataLog.timeLine.rowCSV > dataLog.timeLine.maxRow)
            {
                File.WriteAllText(Folder.list[6] + dataLog.timeLine.nameFileRow, 1.ToString());
                dataLog.timeLine.numFile = (Convert.ToInt32(dataLog.timeLine.numFile) + 1).ToString();
                setupPay.write_text(dataLog.headConfig.fileTimeLine, dataLog.nameFile, configTester.nameFile);
            }

            csvPath = Folder.list[6] + dataLog.timeLine.nameFile + dataLog.timeLine.numFile + ".csv";
            StreamWriter StreamWriterTimeLine = new StreamWriter(csvPath, true);
            while (true)
            {
                try
                {
                    StreamWriterTimeLine.WriteLine(dataLog.timeLine.data);
                }
                catch
                {
                    MessageBox.Show("_กรุณาปิด log file TimeLine.csv ก่อน");
                    continue;
                }
                break;
            }
            StreamWriterTimeLine.Close();

            if (results || configTester.upFail)
            {//Update data log to PRISM
                bool digit_snn = true;
                if (textboxSN.Text.Length != Convert.ToInt32(configPrism.digitSN))
                {
                    digit_snn = false;
                    Log(LogMsgType.Error_Red, "\n" + "sn prism not " + configPrism.digitSN + " digit");
                }
                if (cb_OperationMode.Checked && configPrism.mode != configPrism.Debug && digit_snn && flag_sn_pass[select_test - 1])
                {
                    JsonConvertMain jsonConvert = new JsonConvertMain();
                    jsonConvert.Date = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.CreateSpecificCulture("en-US"));
                    jsonConvert.Time = DateTime.Now.ToString("HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US"));
                    jsonConvert.LoginID = tb_userID.Text;
                    jsonConvert.SWVersion = tb_swVersion.Text;
                    jsonConvert.FWVersion = tb_fwVersion.Text;
                    jsonConvert.SpecVersion = tb_spec.Text;
                    jsonConvert.TestTime = labelTestTime.Text;
                    jsonConvert.LoadInOut = labelInOutTime.Text;
                    if (cb_OperationMode.Checked)
                    {
                        jsonConvert.Mode = configPrism.Operation;
                    }
                    else
                    {
                        jsonConvert.Mode = configPrism.Debug;
                    }
                    jsonConvert.FinalResult = labelStatus.Text;
                    jsonConvert.SN = textboxSN.Text;
                    jsonConvert.Header = select_test;
                    jsonConvert.Failure = checkFailure.Replace(";", string.Empty).Trim();
                    for (int i = 0; i < gridView.Rows.Count; i++)
                    {
                        JsonConvertMain.ResultString_ resultString = new JsonConvertMain.ResultString_();
                        try
                        {
                            string trySup = gridView.Rows[i].Cells[2].Value.ToString();
                        }
                        catch { continue; }
                        if (gridView.Rows[i].Cells[2].Value.ToString() == string.Empty) continue;
                        try
                        {
                            resultString.Step = gridView.Rows[i].Cells[0].Value.ToString();
                        }
                        catch { }
                        try
                        {
                            resultString.Description = gridView.Rows[i].Cells[1].Value.ToString();
                        }
                        catch { }
                        try
                        {
                            resultString.Tolerance = gridView.Rows[i].Cells[2].Value.ToString();
                        }
                        catch { }
                        try
                        {
                            resultString.Measured = gridView.Rows[i].Cells[3].Value.ToString().Replace(",", " ");
                        }
                        catch { }
                        try
                        {
                            resultString.Result = gridView.Rows[i].Cells[4].Value.ToString();
                        }
                        catch { }
                        jsonConvert.ResultString.Add(resultString);
                    }
                    string jsonString = new JavaScriptSerializer().Serialize(jsonConvert);
                    jsonString = Regex.Unescape(jsonString);
                    string pathMis = Folder.list[5] + "\\" + tb_userID.Text + "_" + textboxSN.Text + "_";
                    pathMis += tb_wo.Text.Replace("/", "-") + "_" + cbb_fg.Text + "_";
                    pathMis += labelStatus.Text + "_" + DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss",
                        CultureInfo.CreateSpecificCulture("en-US")) + ".txt";
                    File.WriteAllText(pathMis, jsonString);

                    WaitUpData(pathMis);
                }

                //bool digit_snn = true;
                //if (t.Text.Length != Convert.ToInt32(prismTest.digitSN)) { digit_snn = false; Log(LogMsgType.Error_Red, "\n" + "sn prism not " + prismTest.digitSN + " digit"); }
                //if (radioOperationMode.Checked == true && TeamPrecision.PRISM.cSettingValues.TestingMode != "Debug" && digit_snn && flag_sn_pass[select_test - 1])
                //{
                //    DataSummary = DataSummary.Replace("'", "");
                //    string fctx = TeamPrecision.PRISM.cSettingValues.ProcessName.Trim().ToString();
                //    string MsgPRISM = "";
                //    for (int hgf = 1; hgf <= 5; hgf++)
                //    {
                //        //up_prism_timeout_sn = t.Text;
                //        //up_prism_timeout_result = "FAIL";
                //        //up_prism_timeout_dataSummary = DataSummary;
                //        //if (results) up_prism_timeout_result = "PASS";
                //        //else up_prism_timeout_result = "FAIL";
                //        //MsgPRISM = function_timeout(up_prism_timeout, 2500);
                //        if (results) MsgPRISM = TeamPrecision.PRISM.cResults.SaveTestResult(t.Text, "PASS", DataSummary);
                //        else MsgPRISM = TeamPrecision.PRISM.cResults.SaveTestResult(t.Text, "FAIL", DataSummary);
                //        Log(LogMsgType.Incoming_Blue, "\n" + "up data to prism: " + MsgPRISM);
                //        if (MsgPRISM == "SUCCESS") break;
                //        if (hgf != 5)
                //        {
                //            DelaymS(300);
                //            continue;
                //        }
                //        l.Text = "FAIL";
                //        l.ForeColor = Color.Red;
                //        csvPath = folder[2] + headfer + filename + ".csv";
                //        Log(LogMsgType.Error_Red, "\nPRISM fail while update data");
                //        GlobalTestingFlag[head - 1] = false;
                //    }
                //}
            }
            if (configPrism.mode == configPrism.Operation && configPrism.upDataToKomson)
            {
                DataGridView d = getDataGridView(select_test);
                string q = cbb_fg.Text + ",";
                q += textboxSN.Text + ",";
                string ddd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                q += ddd + ",";
                if (results) q += "PASS,";
                else q += "FAIL,";
                q += tb_userID.Text + ",";
                q += tb_wo.Text + ",";
                q += setupPay.read_text(ConfigTester.Header.komsonTester, configTester.nameFile) + ",";
                if (checkFailure != "")
                {
                    for (int i = 0; i < d.Rows.Count; i++)
                    {
                        if (d.Rows[i].Cells[0].Value.ToString() != checkFailure.Replace("<>", "")) continue;
                        q += checkFailure + " " + d.Rows[i].Cells[1].Value.ToString() + ",";
                        q += d.Rows[i].Cells[3].Value.ToString() + ",";
                        break;
                    }
                }
                else q += ",,";
                q += head.ToString() + ",";
                File.WriteAllText("D:/Datalog to SQL/" + textboxSN.Text + ddd.Replace("-", "_").Replace(" ", "_").Replace(":", "_") + ".txt", q);
            }

            labelInOutTime.Text = "0000";

            if (configPrism.mode == configPrism.Operation)
            {
                try
                {
                    string[] strArrGetWO = TeamPrecision.PRISM.cSNs.getWO(tb_wo.Text, TeamPrecision.PRISM.cSettingValues.ProcessName);
                    tb_outputQty.Text = strArrGetWO[4];
                }
                catch { }
            }
        }
        private void WaitUpData(string path)
        {
            if (!configUpData.waitUpData)
            {
                return;
            }

            while (true)
            {
                List<string> fileData = new List<string>();

                try
                {
                    string[] getFile = Directory.GetFiles(Folder.list[5]);
                    fileData = getFile.ToList<string>();
                }
                catch { }

                if (fileData.Contains(path))
                {
                    DelaymS(25);
                    continue;
                }

                break;
            }
        }
        private string up_prism_timeout_sn = "";
        private string up_prism_timeout_result = "";
        private string up_prism_timeout_dataSummary = "";
        private string up_prism_timeout()
        {
            return TeamPrecision.PRISM.cResults.SaveTestResult(up_prism_timeout_sn, up_prism_timeout_result, up_prism_timeout_dataSummary);
        }

        public bool[] flag_test = { false, false, false, false, false, false, false, false, false, false,
                                    false, false, false, false, false, false, false, false, false, false,
                                    false, false, false, false, false, false, false, false, false, false,
                                    false, false, false, false, false, false};//คือ flag แสดงสถานะของ header ที่กำลังเทสอยู่
        private bool[] flag_head = { true, true, true, true, true, true, true, true, true, true,
                                     true, true, true, true, true, true, true, true, true, true,
                                     true, true, true, true, true, true, true, true, true, true,
                                     true, true, true, true, true, true};//คือ flag ที่แสดงสถานะของการบรรจุบอร์ดลงบน header
        private bool[] flag_loop = { false, false, false, false, false, false, false, false, false, false,
                                     false, false, false, false, false, false, false, false, false, false,
                                     false, false, false, false, false, false, false, false, false, false,
                                     false, false, false, false, false, false};//คือ flag ที่ให้รออ่านค่าจาก txt file ใช้คู่กับ โปรแกรมย่อย exe
        private int[] flag_txt = {  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};//คือ flag ที่จำค่าลำดับ function ใน 1 ช่อง ของ excel
        public int select_test = 1;//คือ flag ที่แสดงสถานะ header ที่ถูกเทสอยู่ปัจจุบัน, flag นี้จะถูกนับวนลูปตามจำนวน header, เริ่มต้นจาก 1
        private bool flag_lock_select_test = false;//คือ flag ที่ล็อกให้เทสหัวใดหัวหนึ่ง ใช้คู่กับ excel ถ้าหากในตารางมีการทาสี ฟ้า ทับไว้ จะทำให้เทสหัวนั้นจนหมดสีฟ้าก่อน
        public int[] row_test = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };//เก็บลำดับของแถวของ excel step test
        private bool[] flagTxtLast = { false, false, false, false, false, false, false, false, false, false,
                                     false, false, false, false, false, false, false, false, false, false,
                                     false, false, false, false, false, false, false, false, false, false,
                                     false, false, false, false, false, false};//เป็น true ถ้า command อยู่ที่บันทัดสุดท้ายของ cells จะใช้งานร่วมกับ Fail To Continue Cells เท่านั้น
        private int[] loopFailToRetest = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                                           1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };// สำหรับนับ loop เวลา fail แล้วต้องการให้ retest
        private int[] flag_solo_step = { 0, 0, };
        private bool flag_goto_after_test = false;
        private void timer_run_Tick(object sender, EventArgs e)
        {
            bool b = false;
            for (int i = 0; i < configTester.numHead; i++)
            {
                b |= flag_test[i];
            }
            if (b)
            {
                timer_run.Stop();
                if (select_test > configTester.numHead) select_test = 1;
                if (flag_goto_after_test)
                {
                    flag_goto_after_test = false;
                    if (!flag_lock_select_test)
                    {
                        if (select_test != 1) select_test--;
                        else select_test = configTester.numHead;
                    }
                }
                run(select_test);
                if (!flag_lock_select_test) select_test++;
                timer_run.Start();
            }
        }
        private void run(int head)
        {
            DataGridView g = getDataGridView(select_test);
            TextBox t = getTextBoxSN(select_test);
            Label l = getLabelStatus(select_test);
            Button b_test = getButtonTest(select_test);
            ProgressBar p = getProgressBar(select_test);

            if (flag_test[select_test - 1] != true) return;
            if (GlobalTestingFlag[select_test - 1] != true)
            {
                string str = "after_test";
                try { Activator.CreateInstance(functionExcel, this, str); }
                catch
                {
                    Log(LogMsgType.Error_Red, "\ncall function " + str + " error");
                }
                flag_lock_select_test = false;
                b_test.Enabled = true;
                save_data(select_test);
                flag_test[select_test - 1] = false;
                return;
            }
            excel.workSheet = excel.workBook.Worksheets["info"];
            excel.sheetTest = excel.workSheet.GetText(44 + select_test, 2);
            excel.workSheet = excel.workBook.Worksheets[excel.sheetTest];
            if (row_test[select_test - 1] == 0)
            {
                time_start[select_test - 1] = DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString("00") + "." + DateTime.Now.Day.ToString("00") + " " + DateTime.Now.ToString("T");
                ClearDataGridView(select_test);
                excel.row[select_test - 1] = GetRowSequenceExcel();
                p.Maximum = GetRowSteptestExcel();
                GetDescription(select_test);
                p.Value = 0;
                rtfTerminal.Clear();
                flag_sn_pass[select_test - 1] = false;
                flagNotReTest[select_test - 1] = false;
                flagUpdateDataGrid[select_test - 1] = true;
                loopFailToRetest[select_test - 1] = 1;
                try
                {
                    Activator.CreateInstance(functionExcel, this, "intro_test()");
                }
                catch (Exception)
                {
                    Log(LogMsgType.Error_Red, "\ncall function intro_test() error");
                    GlobalTestingFlag[select_test - 1] = false;
                    return;
                }
                delete_txt_error();
                flag_txt[select_test - 1] = 0;
                row_test[select_test - 1] += 2;
            }
            if (row_test[select_test - 1] > excel.row[select_test - 1])
            {
                string str = "after_test";
                try { Activator.CreateInstance(functionExcel, this, str); }
                catch (Exception)
                {
                    Log(LogMsgType.Error_Red, "\ncall function " + str + " error");
                }
                flag_lock_select_test = false;
                b_test.Enabled = true;
                save_data(select_test);
                flag_test[select_test - 1] = false;
                return;
            }
            string string_in_excel;
            string string_in_excel_Formula;
            string[] string_array_in_excel;
            if (excel.workSheet.Range[row_test[select_test - 1], 1].Style.KnownColor.ToString() == excel.color.skyBlue)
                flag_lock_select_test = true;
            else flag_lock_select_test = false;
            if (excel.workSheet.Range[row_test[select_test - 1], 1].Style.KnownColor.ToString() == excel.color.gold)
            {
                if (flag_solo_step[0] == 0)
                {
                    flag_solo_step[0] = 1;
                    flag_solo_step[1] = select_test;
                }
                else
                {
                    if (select_test != flag_solo_step[1]) return;
                }
            }
            if (excel.workSheet.Range[row_test[select_test - 1], 1].Style.KnownColor.ToString() != excel.color.none &&
                excel.workSheet.Range[row_test[select_test - 1], 1].Style.KnownColor.ToString() != excel.color.skyBlue &&
                excel.workSheet.Range[row_test[select_test - 1], 1].Style.KnownColor.ToString() != excel.color.gold)
            {
                row_test[select_test - 1]++;
                return;
            }
            string_in_excel = excel.workSheet.GetText(row_test[select_test - 1], 5);
            string_in_excel_Formula = excel.workSheet.GetFormulaStringValue(row_test[select_test - 1], 5);
            if (string_in_excel_Formula != null) string_in_excel = string_in_excel_Formula.Replace("\r\n", "\n");
            if (string_in_excel == "" || string_in_excel == " " || string_in_excel == "  " || string_in_excel == "   " || string_in_excel == null)
            {
                p.Value += 1;
                row_test[select_test - 1]++;
                return;
            }
            excel.stepTest = excel.workSheet.GetText(row_test[select_test - 1], 1);
            if (excel.stepTest == null)
            {
                excel.stepTest = excel.workSheet.GetNumber(row_test[select_test - 1], 1).ToString();
            }//if และ for ด้านล่างนี้ ไม่เกี่ยวอะไรกับ จำนวน header มันเป็นการดึงข้อมูลจาก excel ให้ตรงตำแหน่งเท่านั้น แนวแกน y หรือ คอลัม
            excel.alice[0] = excel.workSheet.GetNumber(row_test[select_test - 1], 1).ToString();
            if (excel.alice[0] == "NaN") excel.alice[0] = excel.workSheet.GetText(row_test[select_test - 1], 1);
            if (excel.alice[0] == null) excel.alice[0] = excel.workSheet.GetFormulaStringValue(row_test[select_test - 1], 1);
            excel.alice[1] = excel.workSheet.GetNumber(row_test[select_test - 1], 2).ToString();
            if (excel.alice[1] == "NaN") excel.alice[1] = excel.workSheet.GetText(row_test[select_test - 1], 2);
            if (excel.alice[1] == null) excel.alice[1] = excel.workSheet.GetFormulaStringValue(row_test[select_test - 1], 2);
            excel.alice[2] = excel.workSheet.GetNumber(row_test[select_test - 1], 3).ToString();
            if (excel.alice[2] == "NaN") excel.alice[2] = excel.workSheet.GetFormulaNumberValue(row_test[select_test - 1], 3).ToString();
            if (excel.alice[2] == "NaN") excel.alice[2] = excel.workSheet.GetText(row_test[select_test - 1], 3);
            if (excel.alice[2] == null) excel.alice[2] = excel.workSheet.GetFormulaStringValue(row_test[select_test - 1], 3);
            excel.alice[3] = excel.workSheet.GetNumber(row_test[select_test - 1], 4).ToString();
            if (excel.alice[3] == "NaN") excel.alice[3] = excel.workSheet.GetFormulaNumberValue(row_test[select_test - 1], 4).ToString();
            if (excel.alice[3] == "NaN") excel.alice[3] = excel.workSheet.GetText(row_test[select_test - 1], 4);
            if (excel.alice[3] == null) excel.alice[3] = excel.workSheet.GetFormulaStringValue(row_test[select_test - 1], 4);
            excel.alice[4] = excel.workSheet.GetNumber(row_test[select_test - 1], 5).ToString();
            if (excel.alice[4] == "NaN") excel.alice[4] = excel.workSheet.GetText(row_test[select_test - 1], 5);
            if (excel.alice[4] == null) excel.alice[4] = excel.workSheet.GetFormulaStringValue(row_test[select_test - 1], 5);
            for (int ii = 0; ii <= 4; ii++)
            {
                if (excel.alice[ii] == null)
                {
                    excel.alice[ii] = "";
                }
            }
            Log(LogMsgType.Incoming_Blue, "\n");
            flagTxtLast[select_test - 1] = false;
            if (string_in_excel.Contains("\n"))
            {
                string_array_in_excel = string_in_excel.Split('\n');
                for (int dsa = flag_txt[select_test - 1]; dsa < string_array_in_excel.Length; dsa++)
                {
                    if (GlobalTestingFlag[select_test - 1] == false) { flag_goto_after_test = true; break; }
                    string str = string_array_in_excel[dsa];
                    if (configTester.failTo == ConfigTester.FailTo.ContinueCel)
                    {
                        if (dsa == string_array_in_excel.Length - 1)
                        {
                            flagTxtLast[select_test - 1] = true;
                        }
                    }
                    if (str.Substring(0, 1) == "#") continue;
                    Log(LogMsgType.Incoming_Blue, "\n" + select_test + ". " + str);
                    if (str.Substring(0, 2) == ">>")
                    {
                        if (configTester.showCMD)
                        {
                            if (!run_call_exe(str, ">>")) continue;
                            return;
                        }
                        else
                        {
                            if (!run_call_exe_black(str, ">>")) continue;
                            return;
                        }
                    }
                    if (str.StartsWith("<[<"))
                    {
                        string resultFilePath = Path.Combine("D:\\PathZero\\Result", str.Replace("<[<", string.Empty).Replace(".txt", $"{select_test}.txt"));
                        try
                        {
                            if (File.Exists(resultFilePath))
                            {
                                string[] resultLines = File.ReadAllLines(resultFilePath);
                                timer_result[select_test - 1].Reset();

                                File.Delete(resultFilePath);
                                flag_solo_step[0] = 0;
                                string resultTest = string.Empty;
                                for (int loop = 1; loop < resultLines.Length; loop++)
                                {
                                    resultTest += resultLines[loop] + Environment.NewLine;
                                }
                                UpdateResultToDataGrid(excel.alice[0], resultTest.Trim(), resultLines[0]);
                                string pathAck = Path.Combine("D:\\PathZero\\Command", str.Replace("<[<", string.Empty).Replace(".txt", $"Ack{select_test}.txt"));
                                File.WriteAllText(pathAck, string.Empty);
                                flag_txt[select_test - 1] = 0;
                                continue;
                            }
                        }
                        catch (IOException)
                        {
                            // Handle file access error
                            // Log or display an appropriate error message
                        }

                        if (timer_result[select_test - 1].ElapsedMilliseconds > timeout_result[select_test - 1])
                        {
                            UpdateResultToDataGrid(excel.alice[0], "*FAIL", "FAIL");
                            return;
                        }

                        flag_loop[select_test - 1] = true;
                        flag_txt[select_test - 1] = dsa;
                        break;
                    }
                    if (str.Substring(0, 2) == "<<")
                    {
                        string[] sup;
                        try
                        {
                            sup = File.ReadAllLines("test_head_" + select_test + "_" + str.Replace("<<", ""));
                            timer_result[select_test - 1].Reset();
                        }
                        catch
                        {
                            if (timer_result[select_test - 1].ElapsedMilliseconds > timeout_result[select_test - 1])
                            {
                                UpdateResultToDataGrid(excel.alice[0], "*FAIL", "FAIL");
                                return;
                            }
                            flag_loop[select_test - 1] = true;
                            flag_txt[select_test - 1] = dsa;
                            break;
                        }
                        File.Delete("test_head_" + select_test + "_" + str.Replace("<<", ""));
                        flag_solo_step[0] = 0;
                        try
                        {
                            if (sup[1] != "NODISPLAY") UpdateResultToDataGrid(excel.alice[0], sup[0], sup[1]);
                        }
                        catch
                        {
                            Log(LogMsgType.Error_Red, "\nformat error " + "test_head_" + select_test + "_" + str.Replace("<<", ""));
                            UpdateResultToDataGrid(excel.alice[0], "Fail", "FAIL");
                            GlobalTestingFlag[select_test - 1] = false;
                            return;
                        }
                        flag_txt[select_test - 1] = 0;
                        continue;
                    }
                    if (str.Substring(0, 3) == "<|<")
                    {
                        string[] sup = { };
                        while (flag_test[select_test - 1])
                        {
                            try
                            {
                                sup = File.ReadAllLines("test_head_" + select_test + "_" + str.Replace("<|<", ""));
                                timer_result[select_test - 1].Reset();
                            }
                            catch
                            {
                                if (timer_result[select_test - 1].ElapsedMilliseconds > timeout_result[select_test - 1])
                                {
                                    UpdateResultToDataGrid(excel.alice[0], "*FAIL", "FAIL");
                                    return;
                                }
                                Log(LogMsgType.Incoming_Blue, "\n" + select_test + ". " + str);
                                DelaymS(50);
                                continue;
                            }
                            break;
                        }
                        File.Delete("test_head_" + select_test + "_" + str.Replace("<|<", ""));
                        try
                        {
                            if (sup[1] != "NODISPLAY") UpdateResultToDataGrid(excel.alice[0], sup[0], sup[1]);
                        }
                        catch (Exception)
                        {
                            Log(LogMsgType.Error_Red, "\nformat error " + "test_head_" + select_test + "_" + str.Replace("<|<", ""));
                            UpdateResultToDataGrid(excel.alice[0], "Fail", "FAIL");
                            GlobalTestingFlag[select_test - 1] = false;
                            return;
                        }
                        flag_txt[select_test - 1] = 0;
                        continue;
                    }
                    if (str.Substring(0, 1) == "[")
                    {
                        if (flagUpdateDataGrid[select_test - 1])
                        {
                            continue;
                        }

                        Match match = Regex.Match(str, @"\[(\d+)\]");// รูปแบบสำหรับหาค่าที่อยู่ในวงเล็บ []
                        string strFunction = Regex.Replace(str, @"\[\d+\]", "");
                        if (match.Success)
                        {
                            string value = match.Groups[1].Value; // ดึงค่าเลขที่อยู่ในวงเล็บ
                            int loop = Convert.ToInt32(value);
                            if (loopFailToRetest[select_test - 1] < loop)
                            {
                                if (!string.IsNullOrEmpty(strFunction))
                                {
                                    try
                                    {
                                        Activator.CreateInstance(functionExcel, this, strFunction);
                                    }
                                    catch (Exception)
                                    {
                                        Log(LogMsgType.Error_Red, "\ncall function " + str + " error");
                                        UpdateResultToDataGrid(excel.alice[0], "Fail", "FAIL");
                                        GlobalTestingFlag[select_test - 1] = false;
                                        return;
                                    }
                                }

                                loopFailToRetest[select_test - 1]++;
                                dsa = -1;
                                continue;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        Log(LogMsgType.Error_Red, "\ncall function " + str + " error");
                        UpdateResultToDataGrid(excel.alice[0], "Fail", "FAIL");
                        GlobalTestingFlag[select_test - 1] = false;
                        return;
                    }
                    try
                    {
                        Console.WriteLine($"{select_test}.{str}");
                        Activator.CreateInstance(functionExcel, this, str);
                    }
                    catch (Exception)
                    {
                        Log(LogMsgType.Error_Red, "\ncall function " + str + " error");
                        UpdateResultToDataGrid(excel.alice[0], "Fail", "FAIL");
                        GlobalTestingFlag[select_test - 1] = false;
                        return;
                    }
                }
            }
            else
            {
                for (int oo = 0; oo < 1; oo++)
                {
                    flagTxtLast[select_test - 1] = true;
                    if (string_in_excel.Substring(0, 1) == "#") continue;
                    Log(LogMsgType.Incoming_Blue, "\n" + select_test + ". " + string_in_excel);
                    if (string_in_excel.Substring(0, 2) == ">>")
                    {
                        if (configTester.showCMD)
                        {
                            if (!run_call_exe(string_in_excel, ">>")) continue;
                            return;
                        }
                        else
                        {
                            if (!run_call_exe_black(string_in_excel, ">>")) continue;
                            return;
                        }
                    }
                    if (string_in_excel.Substring(0, 3) == ">|>")
                    {
                        if (configTester.showCMD)
                        {
                            if (!run_call_exe_black(string_in_excel, ">|>")) continue;
                            return;
                        }
                        else
                        {
                            if (!run_call_exe(string_in_excel, ">|>")) continue;
                            return;
                        }
                    }
                    if (string_in_excel.Substring(0, 2) == "<<")
                    {
                        string[] sup;
                        try
                        {
                            sup = File.ReadAllLines("test_head_" + select_test + "_" + string_in_excel.Replace("<<", ""));
                        }
                        catch (Exception)
                        {
                            flag_loop[select_test - 1] = true;
                            continue;
                        }
                        File.Delete("test_head_" + select_test + "_" + string_in_excel.Replace("<<", ""));
                        flag_solo_step[0] = 0;
                        try
                        {
                            if (sup[1] != "NODISPLAY") UpdateResultToDataGrid(excel.alice[0], sup[0], sup[1]);
                        }
                        catch (Exception)
                        {
                            Log(LogMsgType.Error_Red, "\nformat error " + "test_head_" + select_test + "_" + string_in_excel.Replace("<<", ""));
                            UpdateResultToDataGrid(excel.alice[0], "Fail", "FAIL");
                            GlobalTestingFlag[select_test - 1] = false;
                            return;
                        }
                        continue;
                    }
                    if (string_in_excel.Substring(0, 3) == "<|<")
                    {
                        string[] sup = { };
                        while (flag_test[select_test - 1])
                        {
                            try { sup = File.ReadAllLines("test_head_" + select_test + "_" + string_in_excel.Replace("<|<", "")); }
                            catch (Exception)
                            {
                                Log(LogMsgType.Incoming_Blue, "\n" + select_test + ". " + string_in_excel);
                                DelaymS(50);
                                continue;
                            }
                            break;
                        }
                        File.Delete("test_head_" + select_test + "_" + string_in_excel.Replace("<|<", ""));
                        try
                        {
                            if (sup[1] != "NODISPLAY") UpdateResultToDataGrid(excel.alice[0], sup[0], sup[1]);
                        }
                        catch (Exception)
                        {
                            Log(LogMsgType.Error_Red, "\nformat error " + "test_head_" + select_test + "_" + string_in_excel.Replace("<|<", ""));
                            UpdateResultToDataGrid(excel.alice[0], "Fail", "FAIL");
                            GlobalTestingFlag[select_test - 1] = false;
                            return;
                        }
                        continue;
                    }
                    try
                    {
                        Console.WriteLine($"{select_test}.{string_in_excel}");
                        Activator.CreateInstance(functionExcel, this, string_in_excel);
                    }
                    catch (Exception)
                    {
                        Log(LogMsgType.Error_Red, "\ncall function " + string_in_excel + " error");
                        UpdateResultToDataGrid(excel.alice[0], "Fail", "FAIL");
                        GlobalTestingFlag[select_test - 1] = false;
                        return;
                    }
                }
            }
            if (flagTxtLast[select_test - 1])
            {
                if (!flagUpdateDataGrid[select_test - 1])
                {
                    GlobalTestingFlag[select_test - 1] = false;
                }
                loopFailToRetest[select_test - 1] = 1;
            }
            if (flag_loop[select_test - 1] != true)
            {
                p.Value += 1;
                row_test[select_test - 1]++;
            }
            else
            {
                for (int i = 0; i < configTester.numHead; i++)
                {
                    flag_loop[i] = false;
                }
            }
        }
        private bool run_call_exe(string s, string k)
        {
            if (call_exe(s.Replace(k, ""))) return false;
            Log(LogMsgType.Error_Red, "\ncall function " + s + " error");
            UpdateResultToDataGrid(excel.alice[0], "Fail", "FAIL");
            GlobalTestingFlag[select_test - 1] = false;
            return true;
        }
        private bool run_call_exe_black(string s, string k)
        {
            if (call_exe_black(s.Replace(k, ""))) return false;
            Log(LogMsgType.Error_Red, "\ncall function " + s + " error");
            UpdateResultToDataGrid(excel.alice[0], "Fail", "FAIL");
            GlobalTestingFlag[select_test - 1] = false;
            return true;
        }
        private Stopwatch[] timer_result = { new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(),
                                             new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(),
                                             new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(),
                                             new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch(), new Stopwatch() };
        private int[] timeout_result = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private bool call_exe(string str)
        {
            timeout_result[select_test - 1] = 99000;
            try
            {
                string[] zxc = str.Replace(".exe", "#").Split('#');
                timeout_result[select_test - 1] = Convert.ToInt32(zxc[1]) * 1000;
                str = zxc[0] + ".exe";
            }
            catch { }
            switch (select_test)  //ไอศครีมเชอเบร็ตรสมะนาว
            {
                case 1: if (set_debug_1.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 2: if (set_debug_2.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 3: if (set_debug_3.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 4: if (set_debug_4.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 5: if (set_debug_5.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 6: if (set_debug_6.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 7: if (set_debug_7.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 8: if (set_debug_8.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 9: if (set_debug_9.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 10: if (set_debug_10.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 11: if (set_debug_11.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 12: if (set_debug_12.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 13: if (set_debug_13.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 14: if (set_debug_14.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 15: if (set_debug_15.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 16: if (set_debug_16.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 17: if (set_debug_17.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 18: if (set_debug_18.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 19: if (set_debug_19.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 20: if (set_debug_20.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 21: if (set_debug_21.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 22: if (set_debug_22.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 23: if (set_debug_23.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 24: if (set_debug_24.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 25: if (set_debug_25.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 26: if (set_debug_26.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 27: if (set_debug_27.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 28: if (set_debug_28.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 29: if (set_debug_29.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 30: if (set_debug_30.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 31: if (set_debug_31.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 32: if (set_debug_32.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 33: if (set_debug_33.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 34: if (set_debug_34.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 35: if (set_debug_35.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 36: if (set_debug_36.Checked) timeout_result[select_test - 1] = 99999999; break;
            }
            File.WriteAllText(Program.ConfigPath + "head.txt", select_test.ToString());
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "../../mini_projeck/" + str;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            File.Delete("call_exe_tric.txt");
            try
            {
                Process.Start(startInfo);
            }
            catch
            {
                return false;
            }
            while (true)
            {
                try { File.ReadAllText("call_exe_tric.txt"); break; } catch { }
                DelaymS(50);
            }
            timer_result[select_test - 1].Restart();
            return true;
        }
        private bool call_exe_black(string str)
        {
            timeout_result[select_test - 1] = 99000;
            try
            {
                string[] zxc = str.Replace(".exe", "#").Split('#');
                timeout_result[select_test - 1] = Convert.ToInt32(zxc[1]) * 1000;
                str = zxc[0] + ".exe";
            }
            catch { }
            switch (select_test) //ไอศครีมเชอเบร็ตรสมะนาว
            {
                case 1: if (set_debug_1.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 2: if (set_debug_2.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 3: if (set_debug_3.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 4: if (set_debug_4.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 5: if (set_debug_5.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 6: if (set_debug_6.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 7: if (set_debug_7.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 8: if (set_debug_8.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 9: if (set_debug_9.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 10: if (set_debug_10.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 11: if (set_debug_11.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 12: if (set_debug_12.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 13: if (set_debug_13.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 14: if (set_debug_14.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 15: if (set_debug_15.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 16: if (set_debug_16.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 17: if (set_debug_17.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 18: if (set_debug_18.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 19: if (set_debug_19.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 20: if (set_debug_20.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 21: if (set_debug_21.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 22: if (set_debug_22.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 23: if (set_debug_23.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 24: if (set_debug_24.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 25: if (set_debug_25.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 26: if (set_debug_26.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 27: if (set_debug_27.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 28: if (set_debug_28.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 29: if (set_debug_29.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 30: if (set_debug_30.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 31: if (set_debug_31.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 32: if (set_debug_32.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 33: if (set_debug_33.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 34: if (set_debug_34.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 35: if (set_debug_35.Checked) timeout_result[select_test - 1] = 99999999; break;
                case 36: if (set_debug_36.Checked) timeout_result[select_test - 1] = 99999999; break;
            }
            File.WriteAllText(Program.ConfigPath + "head.txt", select_test.ToString());
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "../../mini_projeck/" + str;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            File.Delete("call_exe_tric.txt");
            try
            {
                Process.Start(startInfo);
            }
            catch
            {
                return false;
            }
            while (true)
            {
                try { File.ReadAllText("call_exe_tric.txt"); break; } catch { }
                DelaymS(50);
            }
            timer_result[select_test - 1].Restart();
            return true;
        }
        private void delete_txt_error()
        {
            for (int hh = 1; hh <= excel.row[select_test - 1]; hh++)
            {
                string string_in_excel;
                string[] string_array_in_excel;
                if (excel.workSheet.Range[hh, 1].Style.KnownColor.ToString() != excel.color.none &&
                    excel.workSheet.Range[hh, 1].Style.KnownColor.ToString() != excel.color.skyBlue &&
                    excel.workSheet.Range[hh, 1].Style.KnownColor.ToString() != excel.color.gold)
                {
                    continue;
                }
                string_in_excel = excel.workSheet.GetText(hh, 5);
                if (string_in_excel == "" || string_in_excel == " " || string_in_excel == "  " || string_in_excel == "   " ||
                    string_in_excel == null)
                {
                    continue;
                }
                excel.stepTest = excel.workSheet.GetText(hh, 1);
                if (excel.stepTest == null)
                {
                    excel.stepTest = excel.workSheet.GetNumber(hh, 1).ToString();
                }
                if (excel.workSheet.GetNumber(hh, 1).ToString() == "NaN")
                {
                    excel.alice[0] = excel.workSheet.GetText(hh, 1);
                }
                else
                {
                    excel.alice[0] = excel.workSheet.GetNumber(hh, 1).ToString();
                }
                if (excel.workSheet.GetNumber(hh, 2).ToString() == "NaN")
                {
                    excel.alice[1] = excel.workSheet.GetText(hh, 2);
                }
                else
                {
                    excel.alice[1] = excel.workSheet.GetNumber(hh, 2).ToString();
                }
                if (excel.workSheet.GetNumber(hh, 3).ToString() == "NaN")
                {
                    excel.alice[2] = excel.workSheet.GetText(hh, 3);
                }
                else
                {
                    excel.alice[2] = excel.workSheet.GetNumber(hh, 3).ToString();
                }
                if (excel.workSheet.GetNumber(hh, 4).ToString() == "NaN")
                {
                    excel.alice[3] = excel.workSheet.GetText(hh, 4);
                }
                else
                {
                    excel.alice[3] = excel.workSheet.GetNumber(hh, 4).ToString();
                }
                if (excel.workSheet.GetNumber(hh, 6).ToString() == "NaN")
                {
                    excel.alice[4] = excel.workSheet.GetText(hh, 6);
                }
                else
                {
                    excel.alice[4] = excel.workSheet.GetNumber(hh, 6).ToString();
                }
                for (int ii = 0; ii <= 4; ii++)
                {
                    if (excel.alice[ii] == null)
                    {
                        excel.alice[ii] = "";
                    }
                }
                if (string_in_excel.Contains("\n"))
                {
                    string_in_excel = string_in_excel.Replace("\n", "&");
                    string_array_in_excel = string_in_excel.Split('&');
                    for (int dsa = flag_txt[select_test - 1]; dsa < string_array_in_excel.Length; dsa++)
                    {
                        string str = string_array_in_excel[dsa];
                        if (str.Substring(0, 1) == "#") continue;
                        if (str.Substring(0, 2) == "<<")
                        {
                            File.Delete("test_head_" + select_test + "_" + str.Replace("<<", ""));
                        }
                        if (str.Substring(0, 3) == "<|<")
                        {
                            File.Delete("test_head_" + select_test + "_" + str.Replace("<|<", ""));
                        }
                    }
                }
                else
                {
                    for (int oo = 0; oo < 1; oo++)
                    {
                        if (string_in_excel.Substring(0, 1) == "#") continue;
                        if (string_in_excel.Substring(0, 2) == "<<")
                        {
                            File.Delete("test_head_" + select_test + "_" + string_in_excel.Replace("<<", ""));
                        }
                        if (string_in_excel.Substring(0, 3) == "<|<")
                        {
                            File.Delete("test_head_" + select_test + "_" + string_in_excel.Replace("<|<", ""));
                        }
                    }
                }
            }
        }

        bool CheckHeader(string CurrentHeader, string f, string header, int index_folder)
        {
            string TodayFile = null;
            string PreviousHeader = null;
            string[] CurrentHeader_split = CurrentHeader.Split(',');

            string filename = Folder.list[index_folder] + header + f + ".csv";
            TodayFile = filename;
            const Int32 BufferSize = 500;
            try
            {
                using (var fileStream = File.OpenRead(TodayFile))//=====================                                               
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        List<string> names = new List<string>(line.Split(','));
                        if (names[0] == "Date" || names[0] == CurrentHeader_split[0]) PreviousHeader = line;
                    }
                    fileStream.Close();
                    if (string.Compare(CurrentHeader, PreviousHeader) != 0)
                        return false;
                }
            }
            catch (Exception) { }
            return true;
        }
        public static void DelaymS(int mS)
        {
            Stopwatch stopwatchDelaymS = new Stopwatch();
            stopwatchDelaymS.Restart();
            while (mS > stopwatchDelaymS.ElapsedMilliseconds)
            {
                if (!stopwatchDelaymS.IsRunning) stopwatchDelaymS.Start();
                Application.DoEvents();
            }
            stopwatchDelaymS.Stop();
        }
        private bool[] flagUpdateDataGrid = { true, true, true, true, true, true, true, true, true, true,
                                     true, true, true, true, true, true, true, true, true, true,
                                     true, true, true, true, true, true, true, true, true, true,
                                     true, true, true, true, true, true};//คล้ายกับ GlobalTestingFlag แต่จะไม่ผูกกับเงื่อนไขอื่น แค่เปลี่ยนไปตามการอัพ pass fail
        public void UpdateResultToDataGrid(string strStep, string strValue, string strResult = "FAIL")
        {
            string strBuf = "";
            DataGridView g = getDataGridView(select_test);

            for (int i = 0; i < g.RowCount; i++)
            {
                try { strBuf = g.Rows[i].Cells[0].Value.ToString(); } catch (Exception) { g.Rows[i].Cells[4].Value = ""; continue; }
                if (strBuf != strStep)
                {
                    if (g.Rows[i].Cells[4].Value == null) g.Rows[i].Cells[4].Value = "";
                    continue;
                }
                g.Rows[i].Cells[3].Value = strValue;
                if (strResult == "PASS" || strResult == "Pass" || strResult == "pass")
                {
                    g.Rows[i].Cells[4].Style.ForeColor = Color.Green;
                    GlobalTestingFlag[select_test - 1] = true;
                    flagUpdateDataGrid[select_test - 1] = true;
                }
                else if (strResult == "FAIL" || strResult == "Fail" || strResult == "fail")
                {
                    g.Rows[i].Cells[4].Style.ForeColor = Color.Red;
                    flagUpdateDataGrid[select_test - 1] = false;
                    if (configTester.failTo == ConfigTester.FailTo.Stop)
                    {
                        GlobalTestingFlag[select_test - 1] = false;
                    }

                }
                g.Rows[i].Cells[4].Value = strResult;
                if (i > Convert.ToInt32(configTester.ScrollDatagrid) && g.Visible) g.FirstDisplayedScrollingRowIndex = i - Convert.ToInt32(configTester.ScrollDatagrid);
                if (!configUI.showDataGrid && _labelProgressInfos != null)
                {
                    string lblStep   = g.Rows[i].Cells[0].Value?.ToString() ?? "";
                    string lblDetail = g.Rows[i].Cells[1].Value?.ToString() ?? "";
                    string lblSpec   = g.Rows[i].Cells[2].Value?.ToString() ?? "";
                    Label lbl = _labelProgressInfos[select_test - 1];
                    lbl.Text = $"Step: {lblStep}  {lblDetail}\nSpec: {lblSpec}  |  Measured: {strValue}\nResult: {strResult}";
                    lbl.ForeColor = strResult.ToUpper() == "PASS" ? Color.Green : Color.Red;
                }
                i = g.RowCount;
                //DelaymS(5);
                break;
            }
        }
        private void CheckPRISMStatus()
        {
            rtfTerminal.Clear();
            //lblUserID.Text = TeamPrecision.PRISM.cSettingValues.EmployeeID;
            setupPay.setup();
            tb_userID.Text = setupPay.read_text(ConfigPrism.Header.employeeID, configPrism.nameFile);
            Log(LogMsgType.Warning_Orange, "\n========= PRISM Info ========");
            //Log(LogMsgType.Outgoing_Green, "\n- TestingMode = " + TeamPrecision.PRISM.cSettingValues.TestingMode);
            //Log(LogMsgType.Outgoing_Green, "\n- Computer name = " + TeamPrecision.PRISM.cSettingValues.ComputerName);
            //Log(LogMsgType.Outgoing_Green, "\n- Station name = " + TeamPrecision.PRISM.cSettingValues.StationName);
            //Log(LogMsgType.Outgoing_Green, "\n- Process name = " + TeamPrecision.PRISM.cSettingValues.ProcessName);
            //Log(LogMsgType.Outgoing_Green, "\n- Employee ID = " + TeamPrecision.PRISM.cSettingValues.EmployeeID);
            //Log(LogMsgType.Outgoing_Green, "\n- Employee ID = " + TeamPrecision.PRISM.cSettingValues.DatabaseServer);
            Log(LogMsgType.Outgoing_Green, "\n- TestingMode = " + configPrism.mode);
            Log(LogMsgType.Outgoing_Green, "\n- Computer name = " + setupPay.read_text(ConfigPrism.Header.computerName, configPrism.nameFile));
            Log(LogMsgType.Outgoing_Green, "\n- Station name = " + setupPay.read_text(ConfigPrism.Header.stationName, configPrism.nameFile));
            Log(LogMsgType.Outgoing_Green, "\n- Process name = " + setupPay.read_text(ConfigPrism.Header.processName, configPrism.nameFile));
            Log(LogMsgType.Outgoing_Green, "\n- Employee ID = " + setupPay.read_text(ConfigPrism.Header.employeeID, configPrism.nameFile));
            Log(LogMsgType.Outgoing_Green, "\n- Database Server = " + setupPay.read_text(ConfigPrism.Header.databaseServerTPP, configPrism.nameFile));
            Log(LogMsgType.Warning_Orange, "\n============================");

            if (configPrism.mode == configPrism.Debug)
            {
                cb_DebugMode.Checked = true; cb_OperationMode.Checked = false;
                tb_wo.Enabled = false;
                this.BackColor = Color.Gold;
            }
            else if (configPrism.mode == configPrism.Operation)
            {
                cb_OperationMode.Checked = true; cb_DebugMode.Checked = false;
                cbb_fg.Enabled = false;
                this.BackColor = default(Color);
                this.ForeColor = default(Color);
            }
            else
            {
                Log(LogMsgType.Error_Red, "\n- Selecting PRISM mode Err");
                GlobalTestingFlag[0] = false;
            }
        }
        private void enable_button_test_and_exit()
        {
            for (int i = 1; i <= configTester.numHead; i++)
            {
                Button b_test = getButtonTest(i);
                Button b_exit = getButtonExit(i);
                DataGridView g = getDataGridView(i);

                if (g.Rows[0].Cells[0].Value == null) continue;
                b_test.Enabled = true;
                b_exit.Enabled = true;
            }
            txtSNBoard_1.Focus();
        }
        private void getWorkOrder(string WO)
        {
            string[] strArrGetWO = { "", "", "", "", "" };
            try
            {
                strArrGetWO = TeamPrecision.PRISM.cSNs.getWO(tb_wo.Text, setupPay.read_text(ConfigPrism.Header.processName, configPrism.nameFile));
            }
            catch (Exception)
            {
                rtfTerminal.Clear();
                Log(LogMsgType.Error_Red, "\nPRISM Err at getWorkOrder functiom");
            }
            if (strArrGetWO[0] == "SUCCESS")
            {
                tb_fwVersion.ForeColor = Color.Green;
                tb_spec.ForeColor = Color.Green;
                tb_detail.ForeColor = Color.Green;
                cbb_fg.Items.Clear();
                cbb_fg.Items.Add(strArrGetWO[1]);
                cbb_fg.SelectedIndex = 0;
                tb_orderQty.Text = strArrGetWO[3]; tb_orderQty.ForeColor = Color.Green;
                tb_outputQty.Text = strArrGetWO[4]; tb_outputQty.ForeColor = Color.Green;
                txtSNBoard_1.Focus();
                string nameFG = strArrGetWO[1];
                try
                {
                    excel.workBook.LoadFromFile("../../TestDescription/" + nameFG + excel.lastName);
                }
                catch
                {
                    Log(LogMsgType.Error_Red, "_ปิด excel ก่อน" + nameFG + excel.lastName + "\n");
                    return;
                }
                // LoadDiscription();
                // Activator.CreateInstance(functionExcel, this, "LoadTestSpec()");
            }
            else
            {
                rtfTerminal.Clear();
                tb_fwVersion.Text = "XXXX"; tb_fwVersion.ForeColor = Color.Red;
                tb_spec.Text = "XXXX"; tb_spec.ForeColor = Color.Red;
                tb_detail.Text = "XXXX"; tb_detail.ForeColor = Color.Red;
                tb_orderQty.Text = "0000"; tb_orderQty.ForeColor = Color.Red;
                tb_outputQty.Text = "0000"; tb_outputQty.ForeColor = Color.Red;
                Log(LogMsgType.Error_Red, "-ไม่พบข้อมูล WO " + tb_wo.Text + " ในระบบ.!");
                for (int i = 0; i < strArrGetWO.Length; i++) Log(LogMsgType.Error_Red, "\n" + strArrGetWO[i]);
                tb_wo.SelectAll();
                tb_wo.Focus();
            }
            enable_button_test_and_exit();
        }
        private void CallUpDataExe()
        {
            if (!configUpData.callExe)
            {
                return;
            }

            Process[] process = Process.GetProcessesByName("up_data");
            if (process.Length == 0)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "up_data.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                try
                {
                    Process.Start(startInfo);
                }
                catch { }
            }
        }
        private void CloseUpDataExe()
        {
            if (!configUpData.callExe)
            {
                return;
            }

            Process[] process = Process.GetProcessesByName("up_data");
            for (int num = 0; num < process.Count(); num++)
            {
                try
                {
                    process[num].Kill();
                }
                catch { }
            }
        }
        private void CallPipeServer()
        {
            if (!configTester.automation)
            {
                return;
            }

            Process[] process = Process.GetProcessesByName("ServerPipe");
            if (process.Length == 0)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "ServerPipe.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                try
                {
                    Process.Start(startInfo);
                }
                catch { }
            }
        }
        private void ClosePipeServer()
        {
            if (!configTester.automation)
            {
                return;
            }

            Process[] process = Process.GetProcessesByName("ServerPipe");
            for (int num = 0; num < process.Count(); num++)
            {
                try
                {
                    process[num].Kill();
                }
                catch { }
            }
        }
        private void CallScan2D()
        {
            if (configUI.readBarcode != ConfigUI.ReadBarcode.Scanner)
            {
                return;
            }

            string nameExeScaner = "Scan2D";
            Process[] pname = Process.GetProcessesByName(nameExeScaner);
            if (pname.Length == 0)
            {
                Process.Start(nameExeScaner, String.Empty);
            }
        }
        private void CloseScan2D()
        {
            if (configUI.readBarcode != ConfigUI.ReadBarcode.Scanner)
            {
                return;
            }

            Process[] process = Process.GetProcessesByName("Scan2D");
            for (int num = 0; num < process.Count(); num++)
            {
                try
                {
                    process[num].Kill();
                }
                catch { }
            }
        }
        private void DataGridSetting(DataGridView dataGridView)
        {
            Dictionary<string, string> columnLengths = new Dictionary<string, string>()
            {
        { "STEP", FormSetDataDrid.Config.LengthStep },
        { "SPEC", FormSetDataDrid.Config.LengthSpec },
        { "MEASURE", FormSetDataDrid.Config.LengthMeasure },
        { "RESULT", FormSetDataDrid.Config.LengthResult }
    };

            foreach (var kvp in columnLengths)
            {
                string columnName = kvp.Key;
                string configKey = kvp.Value;

                string lengthText = setupPay.read_text(configKey, FormSetDataDrid.Config.FileName);
                if (int.TryParse(lengthText, out int columnLength))
                {
                    DataGridViewColumn column = dataGridView.Columns
                        .Cast<DataGridViewColumn>()
                        .FirstOrDefault(c => c.HeaderText.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                    if (column != null)
                    {
                        column.Width = columnLength;
                    }
                }
            }
        }
        public void CreatePath(string path)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                Console.WriteLine("Path นี้มีอยู่แล้ว");
                return;
            }

            string directoryPath = Path.GetDirectoryName(path);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(path))
            {
                // ตรวจสอบว่าเป็นไฟล์หรือโฟลเดอร์
                if (Path.GetExtension(path) == "")
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    using (File.Create(path))
                    { }
                }
            }
        }
        private void set_default()
        {
            configTester.useRelayCard = Convert.ToBoolean(setupPay.read_text(ConfigTester.Header.useRelayCard, configTester.nameFile));
            configTester.numHead = Convert.ToInt32(setupPay.read_text(ConfigTester.Header.numHead, configTester.nameFile));

            dataGridViews = new DataGridView[configTester.numHead];
            for (int i = 0; i < configTester.numHead; i++)
            {
                dataGridViews[i] = Controls.Find("dataGridView_" + (i + 1), true).FirstOrDefault() as DataGridView;
                if (dataGridViews[i] == null)
                {
                    break;
                }
            }

            for (int i = 1; i <= configTester.numHead; i++)
            {
                ToolStripMenuItem c = getToolStripMenuItemDebug(i);
                DataGridSetting(dataGridViews[i - 1]);

                if (!configTester.useRelayCard) flag_head[i - 1] = true;
                else flag_head[i - 1] = false;
                c.Visible = true;
                File.WriteAllText(Program.ConfigPath + "test_head_" + i + "_debug.txt", c.Checked.ToString());
            }
            configTester.ScrollDatagrid = setupPay.read_text(ConfigTester.Header.ScrollDatagrid, configTester.nameFile);

            bool flagAutomation = Convert.ToBoolean(setupPay.read_text(ConfigTester.Header.automation, configTester.nameFile));
            if (flagAutomation)
                configTester.automation = true;
            else
            {
                configTester.automation = false;
            }
            autoTestToolStripMenuItem.Checked = Convert.ToBoolean(setupPay.read_text(ConfigTester.Header.testAuto, configTester.nameFile));
            prism_retest.Checked = Convert.ToBoolean(setupPay.read_text(ConfigTester.Header.allowRetest, configTester.nameFile));
            configPrism.processBefore = Convert.ToBoolean(setupPay.read_text(ConfigPrism.Header.checkProcessBefore, configPrism.nameFile));
            configPrism.processName = setupPay.read_text(ConfigPrism.Header.processName, configPrism.nameFile);
            configTester.upFail = Convert.ToBoolean(setupPay.read_text(ConfigTester.Header.upFail, configTester.nameFile));
            configTester.saveData = setupPay.read_text(ConfigTester.Header.saveData, configTester.nameFile);
            configPrism.digitSN = setupPay.read_text(ConfigPrism.Header.digitSN, configPrism.nameFile);
            configPrism.upDataToKomson = Convert.ToBoolean(setupPay.read_text(ConfigPrism.Header.upDataToKomson, configPrism.nameFile));
            try { prism_retest_text_pass.Text = File.ReadAllText(Program.ConfigPath + "prism_retest_text_pass.txt"); } catch { }
            try { prism_retest_text_fail.Text = File.ReadAllText(Program.ConfigPath + "prism_retest_text_fail.txt"); } catch { }
            configPrism.processBeforeText = setupPay.read_text(ConfigPrism.Header.ProcessBefore, configPrism.nameFile);
            configTester.click2ClearSN = Convert.ToBoolean(setupPay.read_text(ConfigTester.Header.click2ClearSN, configTester.nameFile));
            configTester.showCMD = Convert.ToBoolean(setupPay.read_text(ConfigTester.Header.showCMD, configTester.nameFile));
            ctms_showCmd.Checked = configTester.showCMD;
            configTester.numCardRelay = setupPay.read_text(ConfigTester.Header.numCardRelay, configTester.nameFile);
            configTester.testPanel = Convert.ToBoolean(setupPay.read_text(ConfigTester.Header.testPanel, configTester.nameFile));
            configTester.nameDMM = setupPay.read_text(ConfigTester.Header.nameDMM, configTester.nameFile);
            configTester.failTo = setupPay.read_text(ConfigTester.Header.failTo, configTester.nameFile);
            dataLog.timeLine.numFile = setupPay.read_text(dataLog.headConfig.fileTimeLine, dataLog.nameFile);
            if (setupPay.read_text(ConfigTester.Header.fileTestDescription, configTester.nameFile) == configTester.excel)
            {
                configTester.selectExcel = true;
                configTester.selectLibre = false;
            }
            else
            {
                configTester.selectExcel = false;
                configTester.selectLibre = true;
            }
            configPrism.mode = setupPay.read_text(ConfigPrism.Header.mode, configPrism.nameFile);
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
            configUpData.callExe = Convert.ToBoolean(setupPay.read_text(ConfigUpData.Header.callExe, configUpData.nameFile));
            configUpData.waitUpData = Convert.ToBoolean(setupPay.read_text(ConfigUpData.Header.waitUpData, configUpData.nameFile));
            configUpData.callExeFingerPrint = Convert.ToBoolean(setupPay.read_text(ConfigUpData.Header.callExeFingerPrint, configUpData.nameFile));
            configUI.lbStatus = setupPay.read_text(ConfigUI.Header.StatusLabel, configUI.nameFile);
            configUI.readBarcode = setupPay.read_text(ConfigUI.Header.ReadBarcode, configUI.nameFile);
            configUI.showDataGrid = Convert.ToBoolean(setupPay.read_text(ConfigUI.Header.ShowDataGrid, configUI.nameFile));
            configScript.pyVersion = setupPay.read_text(ConfigScript.Header.pyVersion, configScript.nameFile);
            configScript.pyFolder = setupPay.read_text(ConfigScript.Header.pyFolder, configScript.nameFile);
            if (configScript.pyFolder == "Normal")
            {
                configScript.pyFolder = String.Empty;
            }
            ctms_excel_saveFile_sup();
            try { File.Delete("auto_test_trick.txt"); } catch { }

            //set timer of type one
            tmTypeone.Interval = 5000;
            tmTypeone.Tick += new EventHandler(TmTypeone_Tick);
        }
        //call exe finger print and get employee id and mode
        private bool GetUserIdByFingerPrint(ref string employeeID)
        {
            //call exe finger print
            Process[] process = Process.GetProcessesByName("FingerPrintScanner");
            if (process.Length == 0)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "D:\\svn\\0.Tools\\Software\\FingerPrintScanner\\FingerPrintScanner\\bin\\Release\\FingerPrintScanner.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.WorkingDirectory = "D:\\svn\\0.Tools\\Software\\FingerPrintScanner\\FingerPrintScanner\\bin\\Release";
                try
                {
                    Process.Start(startInfo);
                }
                catch { }
            }

            //set path to finger print for wait read text file
            string pathCurrent = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory("D:\\svn\\0.Tools\\Software\\FingerPrintScanner\\FingerPrintScanner\\bin\\Release");
            File.Delete("LoginFingerPrint.txt");

            //wait read text file
            while (true)
            {
                try
                {
                    string employee = File.ReadAllText("LoginFingerPrint.txt");
                    if (employee == "close")
                    {//if close program finger print with login not success
                        Directory.SetCurrentDirectory(pathCurrent);
                        return false;
                    }
                    else if (employee == "Operation" || employee == "Debug")
                    {//if login by design
                        employeeID = employee;
                        configPrism.mode = employee;
                        Directory.SetCurrentDirectory(pathCurrent);
                        return false;
                    }
                    else
                    {//if login by user
                        string[] employeeLines = employee.Replace("\r\n", "#").Split('#');
                        string[] employeeSup = employeeLines[0].Split('=');
                        employeeID = employeeSup[1];
                        string[] modeSup = employeeLines[1].Split('=');
                        configPrism.mode = modeSup[1];
                    }

                    break;
                }
                catch
                {
                    Thread.Sleep(250);
                }

            }

            //set path to main program
            Directory.SetCurrentDirectory(pathCurrent);
            return true;
        }
        private void ShowMessage(Form formScan, string topic, string message)
        {
            formScan.FormBorderStyle = FormBorderStyle.FixedSingle;
            formScan.MaximizeBox = false;
            formScan.Text = topic;
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
        #endregion
    }
}
