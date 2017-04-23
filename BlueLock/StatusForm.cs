using System;
using System.Timers;
using System.Windows.Forms;
using BlueLock.Extensions;
using InTheHand.Net.Bluetooth;
using System.Diagnostics;
using System.Threading;
using Win32_API;

namespace BlueLock
{
    public partial class StatusForm : Form
    {
        /// <summary>
        /// The Bluetooth Component used for Bluetooth information gathering.
        /// </summary>
        public static readonly BluetoothComponent Component = new BluetoothComponent();

        /// <summary>
        /// The currently selected Bluetooth device.
        /// </summary>
        public static BluetoothDevice Device;

        /// <summary>
        /// The timer interval used for device checking.
        /// </summary>
        public static int TimerInterval = 15;

        /// <summary>
        /// The timer used for periodic device checking.
        /// </summary>
        public static System.Timers.Timer Timer;

        /// <summary>
        /// The initialisation of the main form.
        /// </summary>
        public StatusForm()
        {
            InitializeComponent();

            TimerInterval = Properties.Settings.Default.TimerInterval;
            Timer = new System.Timers.Timer(TimerInterval * 1000);
            numericUpDown1.Value = TimerInterval;
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
            Debug.WriteLine(discoveredDevice.DeviceName);
            SetDevice(discoveredDevice);
        }

        public void EnsureTimerRunning()
        {
            if (!Timer.EnsureTimerRunning(TimerInterval*1000))
            {
                return;
            }

            SetInitialState();
            numericUpDown1.Enabled = false;
            LogMessage($"Timer Start({TimerInterval} seconds)");
        }

        public void EnsureTimerStopped()
        {
            if (!Timer.EnsureTimerStopped())
            {
                return;
            }

            numericUpDown1.Enabled = true;
            LogMessage("Timer Stop");
        }

        /// <summary>
        /// Attach the necessary events.
        /// </summary>
        private void AttachEvents()
        {
            if (Component == null || Timer == null)
            {
                LogMessage("Having trouble registering events.");
                return;
            }

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
            Debug.WriteLine(message);
            listBox1.PerformSafely(() =>
            {
                listBox1.Items.Add(message);
                if (listBox1.SelectedItem != null)
                {
                    listBox1.SetSelected(listBox1.Items.Count - 1, true);
                    listBox1.SetSelected(listBox1.Items.Count - 1, false);
                }
            });
        }

        /// <summary>
        /// Clear all log messages.
        /// </summary>
        private void ClearLogging()
        {
            listBox1.PerformSafely(() => listBox1.Items.Clear());
        }

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
            Timer.Enabled = false;
            CheckDeviceInRange();
            Timer.Enabled = true;
        }

        /// <summary>
        /// Check whether the device is in range.
        /// </summary>
        /// <param name="force">Whether to force setting the device state even if it has not changed.</param>
        private void CheckDeviceInRange(bool force = false)
        {
            var inRange = Device.IsInRange();
            string str = String.Format("InRC:{0} OutRC:{1}", Device.InRangeCount, Device.OutRangeCount);
            Device.SetLockedState(inRange, force);
            LogMessage($"[{DateTime.Now.ToLongTimeString()}] Fake service > {(inRange ? "In range" : "Not in range")} (" + str +")");
        }

        private void DeviceInRangeCallback(bool inRange)
        {
            var idt = Win32.GetIdleTime();
            //string str = "Total time : " + Win32.GetTickCount().ToString() + "; " + "Last input time : " + Win32.GetLastInputTime().ToString();
            string str = "Idle time : " + idt.ToString();

            if (inRange)
            {
                // Device came back in range
                LogMessage("Device in range(" + str + ")");
            }
            else
            {
                bool b = false;
                // Device went out of range
                if (Device.OutRangeCount > 2)
                {
                    if (!Device.FirstInRange)
                    {
                        if (idt > TimerInterval * 3000)
                        {
                            b = true;
                            Device.FirstInRange = true;
                            LogMessage("Device out of range" + str + " > locking Windows!(" + Device.OutRangeCount.ToString() + ")");
                            Win32.LockWorkStation();
                        }
                    }
                }
                if (!b)
                {
                    LogMessage("Device out of range(" + str + ")");
                }
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
                SetDevice(device);
            }
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
            int tv = (int)numericUpDown1.Value;
            if(tv < 5)
            {
                numericUpDown1.Value = 5;
            }
            else
            {
                TimerInterval = tv;
                Properties.Settings.Default.TimerInterval = TimerInterval;
                Properties.Settings.Default.Save();

                LogMessage($"Timer interval changed to {TimerInterval} seconds");
            }
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
