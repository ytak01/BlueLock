using System;
using System.Drawing;
using System.Windows.Forms;
using BlueLock.Properties;
using System.Security.Permissions;
using Win32_API;
using Microsoft.Win32;
using MMFrame.Windows.WindowMessaging;

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


        public static WindowMessage wm = new WindowMessage();

        public static bool FirstOpen = true;

        private const int WM_WTSSESSION_CHANGE = 0x2b1;
        private const int WM_QUERYENDSESSION = 0x11;
        private const int WM_NCLBUTTONDBLCLK = 0xa3;

        public enum ConsoleNotificationFlagsEnum
        {
            NOTIFY_FOR_ALL_SESSIONS = 1,
            NOTIFY_FOR_THIS_SESSION = 0
        }

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

        public MainForm()
        {
            InitializeComponent();

            SystemEvents.SessionEnding +=
                new SessionEndingEventHandler(SystemEvents_SessionEnding);

            wm.CreateReceiver();

            Win32.WTSRegisterSessionNotification(wm.ReceiverHandle, (int)ConsoleNotificationFlagsEnum.NOTIFY_FOR_ALL_SESSIONS);
            this.Closed += (sender, args) => Win32.WTSUnRegisterSessionNotification(wm.ReceiverHandle);

            wm.RegisterMessage(WM_WTSSESSION_CHANGE);
            //wm.RegisterMessage(WM_QUERYENDSESSION);
            wm.RegisterMessage(Win32.WM_MyMessage1);
            wm.RegisterMessage(Win32.WM_MyMessage2);
            wm.RegisterEvent((m) => { MessageReceiveChangeCallback(m); });

            //前のバージョンの設定を読み込み、新しいバージョンの設定とする
            Properties.Settings.Default.Upgrade();

            _statusForm = new StatusForm(wm.ReceiverHandle);
            _logoForm = new LogoForm(this);

            _settingsForm = new SettingsForm(_statusForm);
        }

        delegate void delegateMessageReceiveChangeCallback(Message m);
        private void MessageReceiveChangeCallback(Message m)
        {
            Invoke(new delegateMessageReceiveChangeCallback(MessageReceive),m);
        }

        private void MessageReceive(Message m)
        {
            switch (m.Msg)
            {
                case WM_WTSSESSION_CHANGE:
                    _statusForm.LogMessage($"WM_WTSSESSION_CHANGE SessionParam:{ (WM_WTSESSION_CHANGE_WparamEnum)m.WParam }");
                    switch ((WM_WTSESSION_CHANGE_WparamEnum)m.WParam)
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
                            _statusForm.LogMessage("Session Logon");
                            break;
                        case WM_WTSESSION_CHANGE_WparamEnum.WTS_SESSION_LOGOFF:
                            _statusForm.LogMessage("Session Logoff");
                            break;
                        case WM_WTSESSION_CHANGE_WparamEnum.WTS_SESSION_LOCK:
                            _statusForm.LogMessage("Session Lock");
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
                    break;
                //case WM_QUERYENDSESSION:
                //    _statusForm.LogMessage("Shutdown detect");
                //    terminateToolStripMenuItem.PerformClick();
                //    break;
                case Win32.WM_MyMessage1:
                    toolStripMenuItem_Start.Enabled = false;
                    toolStripMenuItem_Stop.Enabled = true;
                    break;
                case Win32.WM_MyMessage2:
                    toolStripMenuItem_Start.Enabled = true;
                    toolStripMenuItem_Stop.Enabled = false;
                    break;
            }
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
                if (FirstOpen)
                {
                    this.WindowState = FormWindowState.Minimized;
                    FirstOpen = false;
                }
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
            FirstOpen = false;
            _settingsForm.Show();
            _settingsForm.Location = new Point(this.Location.X + this.Size.Width, this.Location.Y);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

        private void terminateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wm.ClearMessages();
            wm.DestroyReceiver();
            _statusForm.LogMessage("Terminate");
            _statusForm.ForceTimerStop();
            notifyIcon1.Visible = false;
            this.Close();
            //Application.Exit();
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
                _logoForm.SetStatusImage(Resources.bluetooth_shield_64);
                _logoForm.Show();
                _logoForm.Size = new Size(74, 74);
                _logoForm.Location = new Point(Screen.PrimaryScreen.WorkingArea.X + Screen.PrimaryScreen.WorkingArea.Width - _logoForm.Width, Screen.PrimaryScreen.WorkingArea.Y);
                this.Opacity = 0;
                this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                this.ShowInTaskbar = false;
            }
            else
            {
                if (_logoForm != null)
                {
                    _logoForm.Hide();
                }
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.Opacity = 100;
                this.ShowInTaskbar = true;
            }
        }

        [SecurityPermission(SecurityAction.Demand,
            Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                //非クライアント領域がダブルクリックされた時
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);
        }
        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (e.Reason == SessionEndReasons.Logoff)
            {
                terminateToolStripMenuItem.PerformClick();
            }
            else if (e.Reason == SessionEndReasons.SystemShutdown)
            {
                terminateToolStripMenuItem.PerformClick();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SystemEvents.SessionEnding -=
                new SessionEndingEventHandler(SystemEvents_SessionEnding);
        }
    }
}