using System;
using System.Collections.Generic;
using System.IO;
using Emgu.CV.CvEnum;

namespace camera_show {
    /// <summary>
    /// NEW (Issue #3): Reads a plain-text file listing camera properties that
    /// should be silently skipped when applying settings.
    ///
    /// Config file location: {headTxtPath}_skip_settings.txt
    /// Format: one CapProp name per line, case-insensitive.
    ///
    /// Example:
    ///   Focus
    ///   Exposure
    ///   Zoom
    ///
    /// If the file does not exist, nothing is skipped.
    /// </summary>
    public class CameraSkipSettings {
        private readonly HashSet<string> _skip =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Load (or reload) skip settings from the file at
        /// <c>headTxtPath + AppFilePath.SkipSettings</c>.
        /// Clears any previously loaded settings first.
        /// </summary>
        public void Load(string headTxtPath) {
            _skip.Clear();
            string file = headTxtPath + AppFilePath.SkipSettings;
            if (!File.Exists(file)) return;

            try {
                foreach (string raw in File.ReadAllLines(file)) {
                    string name = raw.Trim();
                    if (!string.IsNullOrEmpty(name))
                        _skip.Add(name);
                }
            } catch { /* if file is unreadable, behave as if empty */ }
        }

        /// <summary>Returns true if <paramref name="prop"/> is in the skip list.</summary>
        public bool ShouldSkip(CapProp prop) => _skip.Contains(prop.ToString());

        /// <summary>
        /// Returns a newline-separated string of all currently skipped property names,
        /// suitable for displaying in the settings form.
        /// </summary>
        public string ToDisplayText() => string.Join(Environment.NewLine, _skip);

        /// <summary>
        /// Saves a new skip list from a multi-line text string (one name per line)
        /// and reloads it.
        /// </summary>
        public void SaveFromText(string headTxtPath, string text) {
            string file = headTxtPath + AppFilePath.SkipSettings;
            try {
                File.WriteAllText(file, text ?? string.Empty);
                Load(headTxtPath);
            } catch { }
        }
    }
}
