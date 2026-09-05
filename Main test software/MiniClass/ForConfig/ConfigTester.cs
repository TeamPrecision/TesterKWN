namespace ATS.Config
{
    public class ConfigTester
    {
        public string nameFile = "tester_config";
        public string saveNormal = "Normal";
        public string excel = "Excel";

        public bool upFail { get; set; }
        public string ScrollDatagrid { get; set; }
        public bool showCMD { get; set; }
        public bool click2ClearSN { get; set; }
        public string numCardRelay { get; set; }
        public bool useRelayCard { get; set; }
        public bool automation { get; set; }
        public bool testPanel { get; set; }
        public int numHead { get; set; }
        public string saveData { get; set; }
        public string nameDMM { get; set; }
        public bool selectExcel { get; set; }
        public bool selectLibre { get; set; }

        public string failTo { get; set; }

        public class FailTo
        {
            public static string Stop = "Stop";
            public static string ContinueCel = "Continue Cells";
            public static string ContinueAll = "Continue All";
        }

        public static class Header
        {
            public static string arduinoComport = "Arduino Comport Main";
            public static string komsonTester = "Komson Tester";
            public static string useRelayCard = "Use Relay Card";
            public static string numHead = "Number of Head";
            public static string ScrollDatagrid = "Scroll Datagrid";
            public static string automation = "Automation";
            public static string testAuto = "Test Auto";
            public static string allowRetest = "Allow Retest";
            public static string upFail = "Updata Fail to Prism";
            public static string saveData = "Save Data Type";
            public static string click2ClearSN = "Click to Clear SN";
            public static string showCMD = "Show CMD";
            public static string numCardRelay = "Num Card Relay";
            public static string testPanel = "Test Panel";
            public static string nameDMM = "Name DMM";
            public static string fileTestDescription = "File TestDescription";
            public static string failTo = "Fail To";
        }
    }
}
