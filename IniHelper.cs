using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WebView2Desktop
{
    public static class IniHelper
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def,
            StringBuilder retVal, int size, string filePath);

        private static readonly string IniPath = Path.Combine(Application.StartupPath, "config.ini");

        public static void WriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, IniPath);
        }

        public static string ReadValue(string section, string key, string defaultValue = "")
        {
            var sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, defaultValue, sb, sb.Capacity, IniPath);
            return sb.ToString().Trim();
        }
    }
}
