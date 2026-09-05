namespace ATS.Config
{
    public class ConfigUpData
    {
        public string nameFile = "up_data_config";
        public string reReadConfig = "up_data_config.txt";
        public bool callExe { get; set; }
        public bool waitUpData { get; set; }
        public bool callExeFingerPrint { get; set; }

        public static class Header
        {
            public static string callExe = "Call Exe";
            public static string waitUpData = "Wait Up Data";
            public static string callExeFingerPrint = "Call Exe Finger Print Scanner";
        }

        public static class Define
        {
            public static string getOutPut = "up_data_getOutPut.txt";
            public static string getOutPutOk = "up_data_getOutPutOk.txt";
        }
    }
}
