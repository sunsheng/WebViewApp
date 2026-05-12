using System.Diagnostics;
using Microsoft.Win32;
using System.Windows.Forms;

const string ProductName = "WebView2桌面应用";

string? FindProductCode()
{
    using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
    using var uninstallKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
    if (uninstallKey == null) return null;

    foreach (var name in uninstallKey.GetSubKeyNames())
    {
        using var sub = uninstallKey.OpenSubKey(name);
        if (sub?.GetValue("DisplayName")?.ToString() == ProductName)
            return name;
    }
    return null;
}

var code = FindProductCode();
if (code == null)
{
    MessageBox.Show("未找到已安装的程序。", "卸载", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    return;
}

if (MessageBox.Show("确定要卸载 WebView2桌面应用 吗？", "卸载确认",
        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
    return;

Process.Start(new ProcessStartInfo
{
    FileName = "msiexec.exe",
    Arguments = $"/x {code}",
    UseShellExecute = true
});
