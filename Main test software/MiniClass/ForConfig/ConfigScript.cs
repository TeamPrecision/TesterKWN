namespace ATS.Config
{
    public class ConfigScript
    {
        public string nameFile = "Script_config";
        public string pyVersion { get; set; }
        public string pyFolder { get; set; }

        public static class Header
        {
            public static string pyVersion = "Python Version";
            public static string pyFolder = "Python Folder";
        }
    }
}
