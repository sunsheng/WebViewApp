using System;
using System.Windows.Forms;
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

        private void Form1_Load(object? sender, EventArgs e)
        {
            _currentUrl = IniHelper.ReadValue("WebConfig", "DefaultUrl", "https://www.baidu.com");
            _currentAppTitle = IniHelper.ReadValue("WebConfig", "AppTitle", "WebView2 桌面应用");
            Text = _currentAppTitle;

            if (_webView != null)
            {
                _webView.EnsureCoreWebView2Async().ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        Invoke(() => _webView.CoreWebView2.Navigate(_currentUrl));
                    }
                });
            }

            _globalHotkey?.Register();
        }

        private void ShowConfigWindow()
        {
            Invoke(() =>
            {
                using var configForm = new ConfigForm(_currentUrl, _currentAppTitle);
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    _currentUrl = configForm.UrlText;
                    _currentAppTitle = configForm.AppTitleText;

                    IniHelper.WriteValue("WebConfig", "DefaultUrl", _currentUrl);
                    IniHelper.WriteValue("WebConfig", "AppTitle", _currentAppTitle);

                    Text = _currentAppTitle;
                    _webView?.CoreWebView2?.Navigate(_currentUrl);
                }
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _globalHotkey?.UnRegister();
        }
    }
}
