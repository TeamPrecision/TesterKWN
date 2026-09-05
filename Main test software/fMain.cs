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
    public partial class fMain : Form
    {
        #region============================================================= Variable =============================================================
        public fMain()
        {
            setupPay.SelectTab = SetupPay.tabPage.TAB1;
            setupPay.set_nameTab(configTester.nameFile);
            setupPay.SelectTab = SetupPay.tabPage.TAB2;
            setupPay.set_nameTab(configPrism.nameFile);
            setupPay.SelectTab = SetupPay.tabPage.TAB3;
            setupPay.set_nameTab(configUpData.nameFile);
            setupPay.SelectTab = SetupPay.tabPage.TAB4;
            setupPay.set_nameTab(dataLog.nameFile);
            setupPay.SelectTab = SetupPay.tabPage.TAB5;
            setupPay.set_nameTab(FormSetDataDrid.Config.FileName);
            setupPay.SelectTab = SetupPay.tabPage.TAB6;
            setupPay.set_nameTab(configUI.nameFile);
            setupPay.SelectTab = SetupPay.tabPage.TAB7;
            setupPay.set_nameTab(configScript.nameFile);
            setupPay.setup();
            InitializeComponent();
            InitHeadDataGridViewMap();

            functionExcel = Type.GetType(define.formClass);
        }

        public Define define = new Define();
        public DataLog dataLog = new DataLog();
        public Excel excel = new Excel();
        public ConfigTester configTester = new ConfigTester();
        public ConfigPrism configPrism = new ConfigPrism();
        public ConfigUpData configUpData = new ConfigUpData();
        public ConfigUI configUI = new ConfigUI();
        public ConfigScript configScript = new ConfigScript();
        public SetupPay.FormPay setupPay = new SetupPay.FormPay();
        public Automation autoMation = new Automation();

        Type functionExcel;
        public bool[] GlobalTestingFlag = { false, false, false, false, false, false, false, false, false, false,
                                            false, false, false, false, false, false, false, false, false, false,
                                            false, false, false, false, false, false, false, false, false, false,
                                            false, false, false, false, false, false };

        private int[] row = new int[36];
        private string[] time_start = new string[36];
        public bool flag_this_close;
        public bool[] flag_sn_pass = new bool[36];
        public bool[] flagNotReTest = new bool[36];
        public DataGridView[] dataGridViews;
        private Dictionary<string, DataGridView> _headDataGridViewMap;
        private DataGridView[]        _dataGridViews;
        private TextBox[]             _textBoxSNs;
        private Label[]               _labelStatuses;
        private Label[]               _labelTestTimes;
        private Label[]               _labelInOutTimes;
        private Label[]               _labelProgressInfos;
        private Button[]              _buttonTests;
        private Button[]              _buttonExits;
        private ProgressBar[]         _progressBars;
        private ToolStripMenuItem[]   _debugMenuItems;
        private GroupBox[]            _groupBoxHeads;

        // lock
        public object lockRS485 = new object();
        private object lockRoobotStart = new object();
        private object lockLoadTestSpec = new object();

        private volatile bool _bypassInputActive = false;



        //timer loop retest auto support type one
        public System.Windows.Forms.Timer tmTypeone = new System.Windows.Forms.Timer();

        public string function_timeout(Func<string> function, int timeout)
        {
            Task<string> task = Task.Run(function);
            if (task.Wait(timeout)) return task.Result;
            else return "over timeout";
        }
        #endregion

    }
}
