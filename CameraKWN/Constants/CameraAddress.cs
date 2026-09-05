namespace camera_show {
    /// <summary>Camera identification mode: how to locate the right camera on startup.</summary>
    public static class CameraAddress {
        /// <summary>Match camera by comparing its Gamma property to the value stored in CSV.</summary>
        public static readonly string Gamma = "Gamma";
        /// <summary>Use the port number stored in CSV directly — no address scanning.</summary>
        public static readonly string Port  = "Port";
    }
}
