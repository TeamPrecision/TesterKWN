//----------------------------------------------------------------------------
// camera_show — partial Form1 (core fields + lifecycle)
// This file replaces the original monolithic Form1.cs.
// Nested class definitions have been extracted to separate files under:
//   Constants/, Models/, Camera/, Services/, UI/
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using camera_show.MiniClass;

namespace camera_show {
    public partial class Form1 : Form {

        // ── Service objects ───────────────────────────────────────────────────
        public SetupPay.FormPay  setupPay     = new SetupPay.FormPay();
        public SetCamera         setCamera;                           // created in SetUpConfig
        public CreateMinMax      createMinMax = new CreateMinMax();
        public AppFlag           flag         = new AppFlag();
        public AppGlobal         global       = new AppGlobal();
        public FormCancelDialog  formCancel   = new FormCancelDialog();
        public AppSetPath        setPath      = new AppSetPath();
        public AutoScale         autoScale    = new AutoScale();
        public ReAdjust          reAdjust     = new ReAdjust();
        public EditValueCamDiv3  editValueCamDiv3;

        // ── Camera capture parameters ─────────────────────────────────────────
        private VideoCapture.API _captureApi = Emgu.CV.VideoCapture.API.DShow;
        private int              _consecutiveBlackFrames;
        private const int        BlackFrameThreshold        = 5;
        private const double     BlackFrameMeanThreshold    = 8.0;

        // ── Camera stability guard (Issue #1) ─────────────────────────────────
        // Minimum interval between reopen attempts so rapid cycling cannot damage
        // the camera hardware.
        private DateTime         _lastReopenAttempt = DateTime.MinValue;
        private const int        ReopenCooldownMs   = 3000;

        // ── Camera address mode ───────────────────────────────────────────────
        private string _address = string.Empty;

        // ── Layout / ROI reference rectangle ─────────────────────────────────
        /// <summary>The user's "base" ROI before any AutoScale spiral shift.</summary>
        public Rectangle RectSup { get; set; }

        // ── CheckLed mode: simultaneous multi-LED detection ──────────────────
        private bool            _ledModeStarted;
        private bool            _sequentialDebugMode; // captured from debug file at startup; never changed by GUI
        private int             _ledTotal;
        private List<Rectangle> _ledRects;
        private Stopwatch[]     _ledSustainSw;   // per-LED sustained-detection timer
        private string[]        _ledStatuses;    // null=pending, "PASS", "FAIL"
        private bool[]          _ledPrevOn;      // was LED detected in the previous frame

        // ── Snapshot state ────────────────────────────────────────────────────
        /// <summary>Frozen frame displayed while snapshot is active; null when snapshot is off.</summary>
        private Image<Bgr, byte> _snapshotImage;

        // ── Constructor ───────────────────────────────────────────────────────
        public Form1() {
            InitializeComponent();
            editValueCamDiv3 = new EditValueCamDiv3(this);
        }

        // ── Startup ───────────────────────────────────────────────────────────

        /// <summary>
        /// Initialises SetupPay tab names and creates the SetCamera instance.
        /// Must be called AFTER setPath is populated.
        /// </summary>
        public void SetUpConfig() {
            setupPay.SelectTab = SetupPay.tabPage.TAB1;
            setupPay.set_nameTab(setPath.MinmaxCsv);
            setupPay.SelectTab = SetupPay.tabPage.TAB2;
            setupPay.set_nameTab(setPath.StepCsv);
            setupPay.setup();
            setCamera = new SetCamera(this);
        }

        private void Form1_Load(object sender, EventArgs e) {
            if (!AppFilePath.Initialize(out string initError)) {
                MessageBox.Show(initError, "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            GetHead();
            GetStepTest();
            SetAllPath();
            GenFileList();
            GetDebug();
            GetTimeOut();
            GetAutoFocus();

            SetUpConfig();
            ReadFileAddress();
            GetPort();
            WindowCheck();

            GetSetPort();

            if (flag.SetPort) {
                SetPortCamera();
            } else {
                OpenCameraList();
            }

            if (flag.CloseForm) {
                this.Close();
                return;
            }

            CheckMode();
            ReadConfigCamera();
            ReadConfigFile();
            AddSnapshotMenuItem();          // Issue #2 — adds F5 snapshot item to context menu
        }

        // ── Shutdown ──────────────────────────────────────────────────────────

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            try { setCamera?.Capture?.Dispose(); } catch { }
            try { setCamera.Capture = null;      } catch { }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            DelaymS(2000);

            // BUG FIX: original had an infinite while loop — added retry counter.
            int maxRetries = 20;
            while (maxRetries-- > 0) {
                try {
                    string fileList = File.ReadAllText(AppFilePath.List);
                    File.WriteAllText(AppFilePath.List,
                        fileList.Trim().Replace(global.Head, string.Empty));
                    break;
                } catch {
                    Thread.Sleep(50);
                }
            }
        }

        // ── Utilities ─────────────────────────────────────────────────────────

        /// <summary>
        /// Pumps the message loop while waiting, keeping the UI responsive.
        /// Used for short in-startup waits; prefer a timer for repeating work.
        /// </summary>
        public void DelaymS(int ms) {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < ms)
                Application.DoEvents();
            sw.Stop();
        }

        /// <summary>Linear re-mapping of a value from one range to another.</summary>
        private static float Map(float value, float from1, float to1, float from2, float to2) =>
            (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
