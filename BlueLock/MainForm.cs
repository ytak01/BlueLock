using System;
using System.Drawing;
using System.Windows.Forms;
using BlueLock.Properties;
using System.Security.Permissions;
using Win32_API;

namespace BlueLock
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// The singleton StatusForm.
        /// </summary>
        private static StatusForm _statusForm;

        /// <summary>
        /// The singleton SettingsForm.
        /// </summary>
        private static SettingsForm _settingsForm;

        /// <summary>
        /// The singleton LogoForm.
        /// </summary>
        private static LogoForm _logoForm;

        public enum ConsoleNotificationFlagsEnum
        {
            NOTIFY_FOR_ALL_SESSIONS = 1,
            NOTIFY_FOR_THIS_SESSION = 0
        }
        private const int WM_WTSSESSION_CHANGE = 0x2b1;

        public enum WM_WTSESSION_CHANGE_WparamEnum
        {
            WTS_CONSOLE_CONNECT = 0x1,
            WTS_CONSOLE_DISCONNECT = 0x2,
            WTS_REMOTE_CONNECT = 0x3,
            WTS_REMOTE_DISCONNECT = 0x4,
            WTS_SESSION_LOGON = 0x5,
            WTS_SESSION_LOGOFF = 0x6,
            WTS_SESSION_LOCK = 0x7,
            WTS_SESSION_UNLOCK = 0x8,
            WTS_SESSION_REMOTE_CONTROL = 0x9
        }

        public event Action<WM_WTSESSION_CHANGE_WparamEnum> WTSessionChange;

        public MainForm()
        {
            InitializeComponent();

            Win32.WTSRegisterSessionNotification(this.Handle, (int)ConsoleNotificationFlagsEnum.NOTIFY_FOR_THIS_SESSION);
            this.Closed += (sender, args) => Win32.WTSUnRegisterSessionNotification(this.Handle);
            this.WTSessionChange += (param) =>
            {
                switch (param)
                {
                    case WM_WTSESSION_CHANGE_WparamEnum.WTS_CONSOLE_CONNECT:
                        _statusForm.LogMessage("Console Connect");
                        break;
                    case WM_WTSESSION_CHANGE_WparamEnum.WTS_CONSOLE_DISCONNECT:
                        _statusForm.LogMessage("Console Disconnect");
                        break;
                    case WM_WTSESSION_CHANGE_WparamEnum.WTS_REMOTE_CONNECT:
                        _statusForm.LogMessage("Remote Connect");
                        break;
                    case WM_WTSESSION_CHANGE_WparamEnum.WTS_REMOTE_DISCONNECT:
                        _statusForm.LogMessage("Remote Disconnect");
                        break;
                    case WM_WTSESSION_CHANGE_WparamEnum.WTS_SESSION_LOGON:
                        break;
                    case WM_WTSESSION_CHANGE_WparamEnum.WTS_SESSION_LOGOFF:
                        break;
                    case WM_WTSESSION_CHANGE_WparamEnum.WTS_SESSION_LOCK:
                        break;
                    case WM_WTSESSION_CHANGE_WparamEnum.WTS_SESSION_UNLOCK:
                        _statusForm.LogMessage("Session Unlock");
                        break;
                    case WM_WTSESSION_CHANGE_WparamEnum.WTS_SESSION_REMOTE_CONTROL:
                        _statusForm.LogMessage("Remote Control");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("param");
                }
            };

            //前のバージョンの設定を読み込み、新しいバージョンの設定とする
            Properties.Settings.Default.Upgrade();

            _statusForm = new StatusForm();
            _logoForm = new LogoForm(this);

            _settingsForm = new SettingsForm(_statusForm);
        }

        /// <summary>
        /// Attach the necessary events.
        /// </summary>
        private void AttachEvents()
        {
            if (StatusForm.Timer == null)
            {
                _statusForm.LogMessage("Having trouble registering main events.");
                return;
            }

            _statusForm.DeviceStateChanged += (sender, args) => { DeviceStateChangeCallback(args.InRange); };
            _statusForm.InitialDeviceState += (sender, args) => { DeviceStateChangeCallback(args.InRange); };
            _statusForm.LogMessage("Main events registered.");
        }

        private void DeviceStateChange(bool inRange)
        {
            if (inRange)
            {
                txtStatus.BackColor = Color.Green;
                _logoForm.SetStatusBackground(Color.Green);
                txtStatus.ForeColor = Color.White;
                txtStatus.Text = @"Status: In range";
            }
            else
            {
                txtStatus.BackColor = Color.Red;
                _logoForm.SetStatusBackground(Color.Red);
                txtStatus.ForeColor = Color.White;
                txtStatus.Text = @"Status: Out of range";
            }
        }

        delegate void delegateDeviceStateChangeCallback(bool inRange);
        private void DeviceStateChangeCallback(bool inRange)
        {
            Invoke(new delegateDeviceStateChangeCallback(DeviceStateChange), inRange);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            AttachEvents();
            _statusForm.Start();
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            _statusForm.Show();
            _statusForm.Location = new Point(this.Location.X, this.Location.Y + this.Size.Height);
            //_statusForm.Size = new Size(this.Size.Width + _settingsForm.Size.Width, _statusForm.Size.Height);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            _settingsForm.Show();
            _settingsForm.Location = new Point(this.Location.X + this.Size.Width, this.Location.Y);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            _logoForm.Hide();
            base.Show();
            this.Visible = true;
            this.ShowInTaskbar = true;
            base.Activate();
        }

        private void terminateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void toolStripMenuItem_Start_Click(object sender, EventArgs e)
        {
            _statusForm.EnsureTimerRunning();
        }

        private void toolStripMenuItem_Stop_Click(object sender, EventArgs e)
        {
            _statusForm.EnsureTimerStopped();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                //this.Hide();
                _logoForm.SetStatusImage(Resources.bluetooth_shield_64);
                _logoForm.Show();
                _logoForm.Size = new Size(74, 74);
                _logoForm.Location = new Point(Screen.PrimaryScreen.WorkingArea.X + Screen.PrimaryScreen.WorkingArea.Width - _logoForm.Width, Screen.PrimaryScreen.WorkingArea.Y);
                this.ShowInTaskbar = false;
            }
            else
            {
                if (_logoForm != null)
                {
                    _logoForm.Hide();
                    this.ShowInTaskbar = true;
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand,
            Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDBLCLK = 0xA3;

            if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                //非クライアント領域がダブルクリックされた時
                m.Result = IntPtr.Zero;
                return;
            }
            if (m.Msg == WM_WTSSESSION_CHANGE)
            {
                this.onWtSessionChange((WM_WTSESSION_CHANGE_WparamEnum)m.WParam);
            }

            base.WndProc(ref m);
        }

        private void onWtSessionChange(WM_WTSESSION_CHANGE_WparamEnum wparamEnum)
        {
            this.WTSessionChange?.Invoke(wparamEnum);
        }
    }
}