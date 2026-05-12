using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WebView2Desktop
{
    public class GlobalHotkey : NativeWindow
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HotkeyId = 9000;
        // Ctrl键修饰符
        private const uint ModCtrl = 0x0002;
        // , 键虚拟码
        private const uint KeyComma = 0xBC;

        public event Action? OnHotkeyPressed;

        public GlobalHotkey()
        {
            CreateHandle(new CreateParams());
        }

        public void Register()
        {
            RegisterHotKey(Handle, HotkeyId, ModCtrl, KeyComma);
        }

        public void UnRegister()
        {
            UnregisterHotKey(Handle, HotkeyId);
            DestroyHandle();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // 热键消息 0x0312
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == HotkeyId)
            {
                OnHotkeyPressed?.Invoke();
            }
        }
    }
}