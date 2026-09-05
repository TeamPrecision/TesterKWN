namespace camera_show {
    /// <summary>Computed file/folder paths resolved at startup from the head and step config.</summary>
    public class AppSetPath {
        /// <summary>Path prefix for per-head plain-text config files, e.g. "../../config/test_head_1"</summary>
        public string HeadTxt       { get; set; }
        /// <summary>Path prefix for per-head CSV config rows, e.g. "Head 1"</summary>
        public string HeadCsv       { get; set; }
        /// <summary>CSV file name for min/max calibration, e.g. "cameraMinMax_normal"</summary>
        public string MinmaxCsv     { get; set; }
        /// <summary>Relative path to the PASS/FAIL result file read by fMain.</summary>
        public string ResultTxt     { get; set; }
        /// <summary>CSV file name for the current step configuration, e.g. "cameraStep_normal"</summary>
        public string StepCsv       { get; set; }
        /// <summary>Folder containing reference images for compare-image mode.</summary>
        public string FolderCompare { get; set; }
        /// <summary>Folder containing reference data for check-LED mode.</summary>
        public string FolderCheckLed{ get; set; }
        /// <summary>Folder containing reference data for blink-LED mode.</summary>
        public string FolderBlinkLed{ get; set; }
    }
}
