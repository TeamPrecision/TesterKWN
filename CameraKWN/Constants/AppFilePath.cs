using System;
using System.IO;
using System.Windows.Forms;

namespace camera_show {
    /// <summary>
    /// File-path constants for every text file used to communicate with fMain
    /// or store per-head configuration.
    /// The values here must NOT be changed — fMain depends on the exact names.
    /// </summary>
    public static class AppFilePath {

        // ── Config root (resolved at startup from tester_no.txt) ─────────────
        /// <summary>
        /// Absolute path to the config folder for this tester, e.g. C:\Testers\config_1\
        /// Set by calling Initialize() before accessing any path that uses this.
        /// </summary>
        public static string ConfigRoot { get; private set; }

        /// <summary>
        /// Reads tester_no.txt from the exe folder, resolves ../../../config_N/ and
        /// stores it in ConfigRoot.  Returns false and sets <paramref name="error"/>
        /// if the file is missing or unreadable.
        /// </summary>
        public static bool Initialize(out string error) {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            // tester_no.txt is one level up from the exe folder, inside bin\Debug\
            string testerNoFile = Path.GetFullPath(Path.Combine(exeDir, @"..\bin\Debug\tester_no.txt"));
            try {
                string no = File.ReadAllText(testerNoFile).Trim();
                string threeUp = Path.GetFullPath(Path.Combine(exeDir, @"..\..\.."));
                ConfigRoot = Path.GetFullPath(Path.Combine(threeUp, "config_" + no))
                             + Path.DirectorySeparatorChar;
                error = null;
                return true;
            } catch (Exception ex) {
                error = "Cannot read tester_no.txt from:\n" + testerNoFile + "\n\n" + ex.Message;
                return false;
            }
        }

        // ── Files written/read by fMain (in exe folder) ──────────────────────
        public static readonly string List    = "camera_show_list.txt";
        public static readonly string TricExe = "call_exe_tric.txt";
        public static readonly string SetPort = "CameraSetPort.txt";

        // ── Files in config folder ────────────────────────────────────────────
        public static string Head   => ConfigRoot + "head.txt";
        public static string Folder => ConfigRoot + "test_head_";

        // ── Per-head config suffixes (appended to HeadTxt path) ──────────────
        public static readonly string StepTest     = "_steptest.txt";
        public static readonly string Debug        = "_debug.txt";
        public static readonly string TimeOut      = "_timeout.txt";
        public static readonly string AutoFocus    = "_autoFocus.txt";
        /// <summary>
        /// One CapProp name per line; any property listed here is silently
        /// skipped when applying camera settings (Issue #3).
        /// </summary>
        public static readonly string SkipSettings = "_skip_settings.txt";

        // ── Result file (in exe folder) ──────────────────────────────────────
        public static readonly string LedResultTxt = "led_detect_result.txt";

        // ── Rectangle data filenames (no directory prefix) ───────────────────
        public static readonly string RectX      = "RectangleX.txt";
        public static readonly string RectY      = "RectangleY.txt";
        public static readonly string RectWidth  = "RectangleWidth.txt";
        public static readonly string RectHeight = "RectangleHeight.txt";
    }
}
