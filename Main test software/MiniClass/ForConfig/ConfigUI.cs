namespace ATS.Config
{
    public class ConfigUI
    {
        public string nameFile = "UI_config";
        public string lbStatus { get; set; }
        public string readBarcode { get; set; }
        public bool showDataGrid { get; set; }

        public static class LbStatus
        {
            public static string ForeColor = "ForeColor";
            public static string BackColor = "BackColor";
        }

        public static class ReadBarcode
        {
            public static readonly string Camera = "Camera";
            public static readonly string Scanner = "Scanner";
        }

        public static class Header
        {
            public static string StatusLabel = "Status Label";
            public static string ReadBarcode = "Read 2D Barcode";
            public static string ShowDataGrid = "Show DataGrid";
        }
    }
}
