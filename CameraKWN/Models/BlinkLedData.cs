using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace camera_show {
    /// <summary>Measurement state for the blink-LED frequency / duty-cycle detection mode.</summary>
    public class BlinkLedData {
        public double    TimeHigh      { get; set; }
        public double    TimeLow       { get; set; }
        public double    Duty          { get; set; }
        public double    DutySup       { get; set; }
        public double    Frequency     { get; set; }
        public double    FrequencySup  { get; set; }
        public double    Result        { get; set; }
        public bool      FlagResult    { get; set; }
        public Stopwatch StopWatchHigh { get; } = new Stopwatch();
        public Stopwatch StopWatchLow  { get; } = new Stopwatch();
        public bool      TricCal       { get; set; }
        public int       Counter       { get; set; }
        public int       CounterMax    { get; set; }
        /// <summary>Minimum measurement window in ms before frequency is accepted (default 2000).</summary>
        public double    TimeAckMin    { get; set; } = 2000;

        private readonly List<double> _results = new List<double> { 0.0, 0.0, 0.0 };
        /// <summary>Max allowed % deviation between the three latest samples for a stable reading.</summary>
        private const double MaxDiffPercent = 5.0;

        /// <summary>
        /// Adds the current Frequency reading to the rolling window and decides whether
        /// the three latest samples are stable enough to publish a result.
        /// BUG FIX: original code always compared listResult[0] instead of listResult[i].
        /// </summary>
        public void FindResult() {
            if (Frequency > 999999) return;

            _results.Add(Frequency);
            _results.RemoveAt(0);

            double avg = (_results[0] + _results[1] + _results[2]) / 3.0;

            for (int i = 0; i < 3; i++) {
                double diff    = Math.Abs(avg - _results[i]); // FIX: use index i, not always [0]
                double percent = (avg > 0) ? diff / avg * 100.0 : 0;
                if (percent > MaxDiffPercent) {
                    FlagResult = false;
                    return;
                }
            }

            Result     = avg;
            FlagResult = true;
        }
    }
}
