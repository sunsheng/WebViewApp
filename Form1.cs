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
            // 读取网址配置
            _currentUrl = IniHelper.ReadValue("WebConfig", "DefaultUrl", "https://www.baidu.com");
            // 读取程序标题配置
            _currentAppTitle = IniHelper.ReadValue("WebConfig", "AppTitle", "WebView2 桌面应用");

            // 赋值窗口标题
            this.Text = _currentAppTitle;

            // 初始化WebView2跳转
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

        private void ShowConfigWindow()
        {
            Invoke(() =>
            {
                // 传入当前网址+标题
                using var configForm = new ConfigForm(_currentUrl, _currentAppTitle);
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    // 更新内存变量
                    _currentUrl = configForm.UrlText;
                    _currentAppTitle = configForm.AppTitleText;

                    // 写入INI配置
                    IniHelper.WriteValue("WebConfig", "DefaultUrl", _currentUrl);
                    IniHelper.WriteValue("WebConfig", "AppTitle", _currentAppTitle);

                    // 实时刷新窗口标题 + 网页
                    this.Text = _currentAppTitle;
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