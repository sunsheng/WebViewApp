using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WebView2Desktop
{
    public static class IniHelper
    {
        private static readonly string IniPath = Path.Combine(Application.StartupPath, "config.ini");

        public static string ReadValue(string section, string key, string defaultValue = "")
        {
            if (!File.Exists(IniPath))
                return defaultValue;

            bool inSection = false;
            foreach (var line in File.ReadLines(IniPath, Encoding.UTF8))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    inSection = trimmed.Equals($"[{section}]", StringComparison.OrdinalIgnoreCase);
                    continue;
                }
                if (!inSection) continue;
                var eq = trimmed.IndexOf('=');
                if (eq <= 0) continue;
                if (trimmed[..eq].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                    return trimmed[(eq + 1)..].Trim();
            }
            return defaultValue;
        }

        public static void WriteValue(string section, string key, string value)
        {
            var lines = File.Exists(IniPath)
                ? new List<string>(File.ReadAllLines(IniPath, Encoding.UTF8))
                : new List<string>();

            int sectionLine = -1;
            int keyLine = -1;
            int nextSectionLine = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                var trimmed = lines[i].Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    if (trimmed.Equals($"[{section}]", StringComparison.OrdinalIgnoreCase))
                        sectionLine = i;
                    else if (sectionLine >= 0 && nextSectionLine < 0)
                        nextSectionLine = i;
                }
                else if (sectionLine >= 0 && nextSectionLine < 0)
                {
                    var eq = trimmed.IndexOf('=');
                    if (eq > 0 && trimmed[..eq].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                        keyLine = i;
                }
            }

            if (keyLine >= 0)
                lines[keyLine] = $"{key}={value}";
            else if (sectionLine >= 0)
                lines.Insert(nextSectionLine >= 0 ? nextSectionLine : lines.Count, $"{key}={value}");
            else
            {
                lines.Add($"[{section}]");
                lines.Add($"{key}={value}");
            }

            File.WriteAllLines(IniPath, lines, new UTF8Encoding(false));
        }
    }
}
