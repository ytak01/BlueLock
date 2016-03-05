using InTheHand.Net;

namespace BlueLock
{
    /// <summary>
    /// Methods for loading and storing the last used device.
    /// </summary>
    public static class DeviceStorage
    {
        /// <summary>
        /// Load the last used device.
        /// </summary>
        public static BluetoothDevice LoadLastDevice()
        {
            var lastDevice = Properties.Settings.Default.LastDevice;
            if (string.IsNullOrEmpty(lastDevice))
            {
                return null;
            }

            BluetoothAddress lastDeviceAddress;
            return !BluetoothAddress.TryParse(lastDevice, out lastDeviceAddress) ? null : new BluetoothDevice(lastDeviceAddress);
        }

        /// <summary>
        /// Save the last device to the Application Settings.
        /// </summary>
        public static void SaveLastDevice(BluetoothDevice device)
        {
            var lastDevice = device.DeviceAddress.ToString();
            Properties.Settings.Default.LastDevice = lastDevice;
            Properties.Settings.Default.Save();
        }
    }
}