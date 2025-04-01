using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HeatBot
{
    public partial class LogConsoleForm : Form
    {
        private static LogConsoleForm _instance;
        private static readonly object _lock = new object();

        private System.Windows.Forms.Timer runtimeTimer;
        private DateTime startTime;

        public static LogConsoleForm Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null || _instance.IsDisposed)
                    {
                        _instance = new LogConsoleForm();
                    }
                    return _instance;
                }
            }
        }

        private RichTextBox logTextBox;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_LAYERED = 0x80000;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        private LogConsoleForm()
        {
            InitializeComponent();
            SetupRichTextBox();
            ConfigureFormProperties();
            StartRuntimeTimer();
            MakeFormAlwaysOnTop();
            MakeFormClickThrough();
        }

        private void SetupRichTextBox()
        {
            logTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                BorderStyle = BorderStyle.None
            };

            this.Controls.Add(logTextBox);
        }

        private void ConfigureFormProperties()
        {

            this.Size = new Size(350, 300);

            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, screenHeight - this.Height);

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;

            this.BackColor = Color.Black;
            this.Opacity = 0.99;
        }

        private void StartRuntimeTimer()
        {
            startTime = DateTime.Now;

            runtimeTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            runtimeTimer.Tick += UpdateRuntimeTitle;
            runtimeTimer.Start();
        }

        private void UpdateRuntimeTitle(object sender, EventArgs e)
        {
            TimeSpan runtime = DateTime.Now - startTime;
            this.Text = $"HeatBot - {runtime:hh\\:mm\\:ss}";
        }

        public void AppendLog(string message, Color color)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string, Color>(AppendLog), message, color);
                return;
            }

            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.SelectionLength = 0;

            logTextBox.SelectionColor = color;
            logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            logTextBox.SelectionColor = logTextBox.ForeColor;

            logTextBox.ScrollToCaret();
        }

        public void LogInfo(string message) => AppendLog(message, Color.Green);
        public void LogError(string message) => AppendLog(message, Color.Red);
        public void LogDebug(string message) => AppendLog(message, Color.Cyan);
        public void LogWarning(string message) => AppendLog(message, Color.Pink);
        public void LogNormal(string message) => AppendLog(message, Color.White);

        private void MakeFormAlwaysOnTop()
        {
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }

        private void MakeFormClickThrough()
        {
            int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
    }
}