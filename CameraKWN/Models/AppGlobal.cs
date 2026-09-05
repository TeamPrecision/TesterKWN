using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;

namespace camera_show {
    /// <summary>Shared runtime state for the current inspection session.</summary>
    public class AppGlobal {
        public string Head          { get; set; } = "1";
        public string StepTest      { get; set; } = CameraMode.Normal;
        public int    Port          { get; set; }
        public int    TimeOut       { get; set; } = 10000;
        public int    DigitSn       { get; set; } = 13;
        public int    AdjustDegree  { get; set; }
        public int    NumberCompare { get; set; } = 1;
        public int    NumberCheckLed{ get; set; } = 1;
        public int    NumStep       { get; set; } = 1;
        public string ResultBackup  { get; set; } = string.Empty;

        public Stopwatch StopWatch     { get; } = new Stopwatch();
        public Stopwatch StopWatchShow { get; } = new Stopwatch();
        public Stopwatch StopWatchHsv  { get; } = new Stopwatch();

        public BlinkLedData BlinkLed { get; } = new BlinkLedData();

        // ── Image frame storage ──────────────────────────────────────────────
        // NOTE: Callers are responsible for proper disposal.
        // Use the UpdateImage() / UpdateImageHsv() helpers below so the
        // PictureBox is updated and the old frame is disposed in the right order.

        /// <summary>Current BGR display frame. Do not set directly; use UpdateImage().</summary>
        public Image<Bgr, byte> Image    { get; set; }
        /// <summary>Current HSV frame. Do not set directly; use UpdateImageHsv().</summary>
        public Image<Hsv, byte> ImageHsv { get; set; }

        /// <summary>
        /// Replaces the current BGR frame and returns the old one so the caller
        /// can dispose it AFTER updating the PictureBox:
        /// <code>
        ///   var old = global.UpdateImage(newFrame);
        ///   pictureBox1.Image = global.Image.Bitmap;
        ///   old?.Dispose();
        /// </code>
        /// </summary>
        public Image<Bgr, byte> UpdateImage(Image<Bgr, byte> newImage) {
            var old = Image;
            Image = newImage;
            return old;
        }

        /// <summary>Same as UpdateImage but for the HSV frame.</summary>
        public Image<Hsv, byte> UpdateImageHsv(Image<Hsv, byte> newImage) {
            var old = ImageHsv;
            ImageHsv = newImage;
            return old;
        }
    }
}
