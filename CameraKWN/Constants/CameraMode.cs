namespace camera_show {
    /// <summary>
    /// Step-test mode identifiers — the strings that appear inside the
    /// steptest config file and drive which Application.Idle handler is registered.
    /// </summary>
    public static class CameraMode {
        public static readonly string Normal   = "normal";
        public static readonly string Read2d   = "read2d";
        public static readonly string ComPar   = "compar_image";   // legacy spelling
        public static readonly string ComPear  = "compare_image";  // current spelling
        public static readonly string CheckLed = "check_led";
        public static readonly string BlinkLed = "BlinkLed";
    }
}
