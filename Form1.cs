using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace WebView2Desktop
{
    public partial class Form1 : Form
    {
        private WebView2? _webView;
        private GlobalHotkey? _globalHotkey;
        private string _currentUrl = "";
        private string _currentAppTitle = "";

        public Form1()
        {
            InitializeComponent();
            _globalHotkey = new GlobalHotkey();
            _globalHotkey.OnHotkeyPressed += ShowConfigWindow;
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private void InitializeComponent()
        {
            WindowState = FormWindowState.Maximized;

            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(_webView);
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            _currentUrl = IniHelper.ReadValue("WebConfig", "DefaultUrl", "https://www.baidu.com");
            _currentAppTitle = IniHelper.ReadValue("WebConfig", "AppTitle", "WebView2 桌面应用");
            Text = _currentAppTitle;

            if (_webView != null)
            {
                // 配置WebView2环境，允许局域网HTTP访问
                var envOptions = new CoreWebView2EnvironmentOptions
                {
                    AdditionalBrowserArguments = "--disable-features=EnhancedSecurityMode --allow-insecure-localhost"
                };
                var env = await CoreWebView2Environment.CreateAsync(null, null, envOptions);
                
                await _webView.EnsureCoreWebView2Async(env);
                
                // 启用所有必要功能
                _webView.CoreWebView2.Settings.IsScriptEnabled = true;
                _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                _webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                _webView.CoreWebView2.Settings.IsStatusBarEnabled = true;
                
                // 导航到目标地址
                _webView.CoreWebView2.Navigate(_currentUrl);
            }

            _globalHotkey?.Register();
        }

        private async void ShowConfigWindow()
        {
            Invoke(async () =>
            {
                using var configForm = new ConfigForm(_currentUrl, _currentAppTitle);
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    _currentUrl = configForm.UrlText;
                    _currentAppTitle = configForm.AppTitleText;

                    // 写入配置文件
                    IniHelper.WriteValue("WebConfig", "DefaultUrl", _currentUrl);
                    IniHelper.WriteValue("WebConfig", "AppTitle", _currentAppTitle);

                    // 实时更新窗口标题
                    Text = _currentAppTitle;

                    // 强制重新加载（彻底解决不刷新问题）
                    if (_webView != null && _webView.CoreWebView2 != null)
                    {
                        _webView.CoreWebView2.Stop();
                        _webView.CoreWebView2.Navigate("about:blank");
                        await Task.Delay(150);
                        _webView.CoreWebView2.Navigate(_currentUrl);
                    }
                }
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _globalHotkey?.UnRegister();
        }
    }
}
