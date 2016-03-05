using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Timers;
using System.Windows.Forms;
using BlueLock.Extensions;
using BlueLock.Properties;

namespace BlueLock
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// The currently selected Bluetooth device.
        /// </summary>
        //private static BluetoothDevice _device;

        /// <summary>
        /// The timer interval used for device checking.
        /// </summary>
        //private static int _timerInterval = 15000;

        /// <summary>
        /// The timer used for periodic device checking.
        /// </summary>
        //private static readonly System.Timers.Timer Timer = new System.Timers.Timer(_timerInterval);

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

        public MainForm()
        {
            InitializeComponent();

            _statusForm = new StatusForm();
            _logoForm = new LogoForm(this);

            AttachEvents();

            _statusForm.Start();
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

            StatusForm.Timer.Elapsed += TimerOnElapsed;
            _statusForm.DeviceStateChanged += (sender, args) => { DeviceStateChangeCallback(args.InRange); };
            _statusForm.InitialDeviceState += (sender, args) => { DeviceStateChangeCallback(args.InRange); };
            _statusForm.LogMessage("Main events registered.");
        }

        /// <summary>
        /// The callback of the timer elapsing.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="elapsedEventArgs">The event arguments.</param>
        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            CheckDeviceInRange();
        }

        /// <summary>
        /// Check whether the device is in range.
        /// </summary>
        /// <param name="force">Whether to force setting the device state even if it has not changed.</param>
        private void CheckDeviceInRange(bool force = false)
        {
            var inRange = StatusForm.Device.IsInRange();
            _statusForm.LogMessage($"[{DateTime.Now.ToLongTimeString()}] Fake service > {(inRange ? "In range" : "Not in range")}");
            StatusForm.Device.SetLockedState(inRange, force);
        }

        private void DeviceStateChangeCallback(bool inRange)
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

        private void MainForm_Load(object sender, EventArgs e) { }

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
                _logoForm.Hide();
                this.ShowInTaskbar = true;
            }
        }
    }
}