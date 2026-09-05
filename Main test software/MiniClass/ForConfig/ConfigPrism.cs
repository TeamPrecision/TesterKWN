namespace ATS.Config
{
    public class ConfigPrism
    {
        public string nameFile = "prism_config";
        public string Operation = "Operation";
        public string OperationMode = "Operation Mode";
        public string Debug = "Debug";
        public string DebugMode = "Debug Mode";
        public string success = "SUCCESS";

        public string processBeforeText { get; set; }
        public bool processBefore { get; set; }
        public string digitSN { get; set; }
        public bool upDataToKomson { get; set; }
        public string mode { get; set; }
        public string processName { get; set; }

        public static class Header
        {
            public static string mode = "Mode";
            public static string databaseName = "Database Name";
            public static string databaseServerTPP = "Database Server TPP";
            public static string databaseServerTPR = "Database Server TPR";
            public static string computerName = "Computer Name";
            public static string stationName = "Station Name";
            public static string processName = "Process Name";
            public static string employeeID = "Employee ID";
            public static string checkProcessBefore = "Check Process Before";
            public static string ProcessBefore = "Process Before";
            public static string digitSN = "Digit SN";
            public static string upDataToKomson = "Up Data To Komson";
        }
    }
}
