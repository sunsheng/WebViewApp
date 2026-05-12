using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WebView2Desktop
{
    public partial class Form1 : Form
    {
        private readonly WebView2 _webView;
        private readonly GlobalHotkey _globalHotkey;
        private string _currentUrl = "";

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
            Text = "WebView2 桌面应用";
            WindowState = FormWindowState.Maximized;

            // 原生WebView2控件
            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(_webView);
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // 读取配置
            _currentUrl = IniHelper.ReadValue("WebConfig", "DefaultUrl", "https://www.baidu.com");

            // 初始化WebView2并跳转
            _webView.EnsureCoreWebView2Async().ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    Invoke(() => _webView.CoreWebView2.Navigate(_currentUrl));
                }
            });

            // 注册全局快捷键 Ctrl+,
            _globalHotkey.Register();
        }

        // 唤起配置弹窗
        private void ShowConfigWindow()
        {
            Invoke(() =>
            {
                using var configForm = new ConfigForm(_currentUrl);
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    _currentUrl = configForm.UrlText;
                    // 保存到ini
                    IniHelper.WriteValue("WebConfig", "DefaultUrl", _currentUrl);
                    // 刷新页面
                    _webView.CoreWebView2?.Navigate(_currentUrl);
                }
            });
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _globalHotkey.UnRegister();
        }
    }
}