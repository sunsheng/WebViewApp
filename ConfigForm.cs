using System;
using System.Windows.Forms;
using System.Drawing;

namespace WebView2Desktop
{
    public partial class ConfigForm : Form
    {
        public string UrlText => txtUrl.Text.Trim();
        public string AppTitleText => txtTitle.Text.Trim();

        private TextBox? txtUrl;
        private TextBox? txtTitle;
        private Button? btnSave;

        public ConfigForm(string initUrl, string initTitle)
        {
            InitializeComponent();
            txtUrl.Text = initUrl;
            txtTitle.Text = initTitle;
        }

        private void InitializeComponent()
        {
            Text = "程序配置";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Size = new Size(450, 220);
            KeyPreview = true;

            var lblTitle = new Label
            {
                Text = "程序标题：",
                Location = new Point(20, 15),
                AutoSize = true
            };
            txtTitle = new TextBox
            {
                Location = new Point(90, 12),
                Size = new Size(300, 28)
            };

            var lblUrl = new Label
            {
                Text = "默认网址：",
                Location = new Point(20, 60),
                AutoSize = true
            };
            txtUrl = new TextBox
            {
                Location = new Point(90, 57),
                Size = new Size(300, 28)
            };

            btnSave = new Button
            {
                Text = "保存配置",
                Location = new Point(160, 110),
                Size = new Size(120, 35)
            };
            btnSave.Click += BtnSave_Click;

            Controls.Add(lblTitle);
            Controls.Add(txtTitle);
            Controls.Add(lblUrl);
            Controls.Add(txtUrl);
            Controls.Add(btnSave);

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
            if (string.IsNullOrWhiteSpace(AppTitleText))
            {
                MessageBox.Show("程序标题不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
