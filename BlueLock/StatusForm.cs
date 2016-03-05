using System;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;
using BlueLock.Extensions;
using InTheHand.Net.Bluetooth;

namespace BlueLock
{
    public partial class StatusForm : Form
    {
        /// <summary>
        /// The Bluetooth Component used for Bluetooth information gathering.
        /// </summary>
        public static readonly BluetoothComponent Component = new BluetoothComponent();

        ///// <summary>
        ///// The Bluetooth Client used for connecting to Bluetooth devices.
        ///// </summary>
        //private static readonly BluetoothClient Client = new BluetoothClient();

        ///// <summary>
        ///// Event registration for Bluetooth events.
        ///// </summary>
        //private static readonly BluetoothWin32Events EventRegistration = BluetoothWin32Events.GetInstance();

        /// <summary>
        /// The currently selected Bluetooth device.
        /// </summary>
        public static BluetoothDevice Device;

        /// <summary>
        /// The timer interval used for device checking.
        /// </summary>
        public static int TimerInterval = 15000;

        /// <summary>
        /// The timer used for periodic device checking.
        /// </summary>
        public static readonly System.Timers.Timer Timer = new System.Timers.Timer(TimerInterval);

        /// <summary>
        /// The Windows command used to lock the workstation.
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        //[DllImport("user32.dll", SetLastError = true)]
        public static extern bool LockWorkStation();

        /// <summary>
        /// The initialisation of the main form.
        /// </summary>
        public StatusForm()
        {
            InitializeComponent();
        }

        public void Start()
        {
            AttachEvents();
            var lastDevice = DeviceStorage.LoadLastDevice();
            if (lastDevice != null)
            {
                UseLastDevice(lastDevice);
            }
        }

        public event EventHandler<BluetoothDevice.DeviceStateEventArgs> DeviceStateChanged;

        public event EventHandler<BluetoothDevice.DeviceStateEventArgs> InitialDeviceState;

        public void TriggerDeviceStateChanged(bool inRange)
        {
            DeviceStateChanged?.Invoke(this, new BluetoothDevice.DeviceStateEventArgs { InRange = inRange });
        }

        public void TriggerInitialDeviceState(bool inRange)
        {
            InitialDeviceState?.Invoke(this, new BluetoothDevice.DeviceStateEventArgs { InRange = inRange });
        }

        /// <summary>
        /// Set the last device as current device.
        /// </summary>
        /// <param name="discoveredDevice">The discovered device.</param>
        private void UseLastDevice(BluetoothDevice discoveredDevice)
        {
            LogMessage($"Last device loaded > {discoveredDevice.DeviceName}");
            SetDevice(discoveredDevice);
        }

        public void EnsureTimerRunning()
        {
            if (!Timer.EnsureTimerRunning(TimerInterval))
            {
                return;
            }

            SetInitialState();
            numericUpDown1.Enabled = false;
        }

        public void EnsureTimerStopped()
        {
            if (!Timer.EnsureTimerStopped())
            {
                return;
            }

            numericUpDown1.Enabled = true;
        }

        /// <summary>
        /// Attach the necessary events.
        /// </summary>
        private void AttachEvents()
        {
            if (/*EventRegistration == null || */Component == null || Timer == null)
            {
                LogMessage("Having trouble registering events.");
                return;
            }

            //EventRegistration.InRange += EventRegistrationOnInRange;
            //EventRegistration.OutOfRange += EventRegistrationOnOutOfRange;
            Component.DiscoverDevicesComplete += ComponentOnDiscoverDevicesComplete;
            Timer.Elapsed += TimerOnElapsed;
            LogMessage("Status events registered.");
        }

        /// <summary>
        /// Log a messge.
        /// </summary>
        /// <param name="message">
        /// The message to log.
        /// </param>
        public void LogMessage(string message)
        {
            listBox1.PerformSafely(() => listBox1.Items.Add(message));
            listBox1.PerformSafely(() =>
            {
                listBox1.SetSelected(listBox1.Items.Count - 1, true);
                listBox1.SetSelected(listBox1.Items.Count - 1, false);
            });
        }

        /// <summary>
        /// Clear all log messages.
        /// </summary>
        private void ClearLogging()
        {
            listBox1.PerformSafely(() => listBox1.Items.Clear());
        }

        ///// <summary>
        ///// The event callback for when a device goes out of range according to Bluetooth events.
        ///// </summary>
        ///// <param name="sender">The sender.</param>
        ///// <param name="bluetoothWin32RadioInRangeEventArgs">The event arguments.</param>
        //private void EventRegistrationOnOutOfRange(object sender, BluetoothWin32RadioOutOfRangeEventArgs bluetoothWin32RadioOutOfRangeEventArgs)
        //{
        //    LogMessage($"Out of range > {bluetoothWin32RadioOutOfRangeEventArgs.Device.DeviceName}");
        //}

        ///// <summary>
        ///// The event callback for when a device comes in range according to Bluetooth events.
        ///// </summary>
        ///// <param name="sender">
        ///// The sender.
        ///// </param>
        ///// <param name="bluetoothWin32RadioInRangeEventArgs">
        ///// The event arguments.
        ///// </param>
        //private void EventRegistrationOnInRange(object sender, BluetoothWin32RadioInRangeEventArgs bluetoothWin32RadioInRangeEventArgs)
        //{
        //    LogMessage($"In range > {bluetoothWin32RadioInRangeEventArgs.Device.DeviceName}: {(bluetoothWin32RadioInRangeEventArgs.Device.Connected ? "Connected" : "Not connected")}");
        //}

        /// <summary>
        /// The callback for when asynchronous device discovery completes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="discoverDevicesEventArgs">The event arguments.</param>
        private void ComponentOnDiscoverDevicesComplete(object sender, DiscoverDevicesEventArgs discoverDevicesEventArgs)
        {
            foreach (var discoveredDevice in discoverDevicesEventArgs.Devices)
            {
                LogMessage($"Discovered > {discoveredDevice.DeviceName} [{discoveredDevice.DeviceAddress}]: {(discoveredDevice.Authenticated ? "Authenticated, " : "")}{(discoveredDevice.Connected ? "Connected, " : "")}{(discoveredDevice.Remembered ? "Remembered, " : "")}{discoveredDevice.ClassOfDevice.Device} ({discoveredDevice.ClassOfDevice.MajorDevice}), {discoveredDevice.Rssi}");
            }
        }

        /// <summary>
        /// Set the initial state of the device.
        /// </summary>
        private void SetInitialState()
        {
            CheckDeviceInRange(true);
            TriggerInitialDeviceState(Device.DeviceInRange);
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
            var inRange = Device.IsInRange();
            LogMessage($"[{DateTime.Now.ToLongTimeString()}] Fake service > {(inRange ? "In range" : "Not in range")}");
            Device.SetLockedState(inRange, force);
        }

        private void DeviceInRangeCallback(bool inRange)
        {
            if (inRange)
            {
                // Device came back in range
                LogMessage("Device in range");
            }
            else
            {
                // Device went out of range
                LogMessage("Device out of range > locking Windows!");
                LockWorkStation();
            }

            TriggerDeviceStateChanged(inRange);
        }

        private void SetDevice(BluetoothDevice device, bool noNewEvents = false)
        {
            Device = device;
            if (!noNewEvents)
            {
                Device.DeviceStateChanged += (sender, args) => { DeviceInRangeCallback(args.InRange); };
            }

            EnsureTimerRunning();
        }

        public void SelectDevice()
        {
            var device = DeviceDiscovery.SelectDeviceInDialog(this);
            if (device != null)
            {
                LogMessage(
                    $"Device selected > {device.DeviceName} [{device.DeviceAddress}]: {(device.Authenticated ? "Authenticated, " : "")}{(device.Connected ? "Connected, " : "")}{(device.Remembered ? "Remembered, " : "")}{device.ClassOfDevice.Device} ({device.ClassOfDevice.MajorDevice}), {device.Rssi}");
            }

            SetDevice(device);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ClearLogging();

            Component.DiscoverDevicesAsync(byte.MaxValue, true, true, false, false, null);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SelectDevice();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Device?.InstalledServices == null)
            {
                return;
            }

            var services = Device.GetInstalledServices();
            foreach (var service in services)
            {
                LogMessage($"Service found > {service.Key}: {service.Value}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Device == null)
            {
                return;
            }

            Device.Refresh();
            LogMessage($"Status updated > {Device.DeviceName} [{Device.DeviceAddress}]: {(Device.Authenticated ? "Authenticated, " : "")}{(Device.Connected ? "Connected, " : "")}{(Device.Remembered ? "Remembered, " : "")}{Device.ClassOfDevice.Device} ({Device.ClassOfDevice.MajorDevice}), {Device.Rssi}");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RadioVersions versions;
            if (!Device.TryGetVersions(out versions)) return;

            LogMessage($"{versions.Manufacturer} {versions.LmpVersion}.{versions.LmpSubversion}");
        }


        private void button6_Click(object sender, EventArgs e)
        {
            Device?.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (Device == null)
            {
                return;
            }

            LogMessage($"Fake service > {(Device.IsInRange() ? "In range" : "Not in range")}");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            EnsureTimerRunning();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            EnsureTimerStopped();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Device.SetLockedState();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            TimerInterval = (int)numericUpDown1.Value * 1000;
            LogMessage($"Timer interval changed to {numericUpDown1.Value} seconds");
        }

        private void DebugForm_Load(object sender, EventArgs e)
        {
            this.Closing += DebugForm_Closing;
        }

        private void DebugForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
