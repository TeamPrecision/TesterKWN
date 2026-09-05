namespace camera_show {
    /// <summary>Boolean state flags for the current session.</summary>
    public class AppFlag {
        public bool SetPort       { get; set; }
        public bool Read2d        { get; set; }
        public bool ComPear       { get; set; }
        public bool CheckLed      { get; set; }
        public bool BlinkLed      { get; set; }
        public bool ResultPass    { get; set; }
        /// <summary>When true, result files are not written and the app does not auto-close.</summary>
        public bool Debug         { get; set; }
        public bool CloseForm     { get; set; }
        public bool AutoFocus     { get; set; }
        public bool ClearStopWatch{ get; set; }
        /// <summary>When true the display is frozen on the snapshot frame; processing continues.</summary>
        public bool Snapshot      { get; set; }

        public AppFlag() {
            Debug = true; // default: start in debug mode until fMain hands off a job
        }
    }
}
