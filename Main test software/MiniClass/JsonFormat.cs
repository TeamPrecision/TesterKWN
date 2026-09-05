namespace ATS.Models
{
    public class JsonFormat
    {
        public string Header = "GeoBlue";
        public string Date = string.Empty;
        public string Time = string.Empty;
        public string Tester = string.Empty;
        public string Command = string.Empty;
        public Payload_ Payload = new Payload_();
        public string CRC = string.Empty;

        public class Payload_
        {
            public string Data1 = string.Empty;
            public string Data2 = string.Empty;
            public string Data3 = string.Empty;
            public string Data4 = string.Empty;
            public string Data5 = string.Empty;
        }
    }
}
