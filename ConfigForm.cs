using System;
using System.Windows.Forms;

namespace WebView2Desktop
{
    public partial class ConfigForm : Form
    {
        public string UrlText => txtUrl.Text.Trim();

        private TextBox txtUrl;
        private Button btnSave;

        public ConfigForm(string initUrl)
        {
            InitializeComponent();
            txtUrl.Text = initUrl;
        }

        private void InitializeComponent()
        {
            Text = "配置网址";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Size = new Size(450, 180);
            KeyPreview = true;

            // 输入框
            txtUrl = new TextBox
            {
                Location = new Point(20, 30),
                Size = new Size(380, 28)
            };

            // 保存按钮
            btnSave = new Button
            {
                Text = "保存配置",
                Location = new Point(160, 80),
                Size = new Size(120, 35)
            };
            btnSave.Click += BtnSave_Click;

            Controls.Add(txtUrl);
            Controls.Add(btnSave);

            // ESC关闭
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    Close();
            };
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UrlText))
            {
                MessageBox.Show("请输入有效网址", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}