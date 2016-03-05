using System.Windows.Forms;
using InTheHand.Windows.Forms;

namespace BlueLock
{
    /// <summary>
    /// Methods to use for device discovery.
    /// </summary>
    public class DeviceDiscovery
    {
        /// <summary>
        /// Allow the user to select a Bluetooth device in a Windows dialog.
        /// </summary>
        /// <returns></returns>
        public static BluetoothDevice SelectDeviceInDialog(IWin32Window window)
        {
            var dlg = new SelectBluetoothDeviceDialog();
            var result = dlg.ShowDialog(window);
            if (result != DialogResult.OK || dlg.SelectedDevice == null)
            {
                return null;
            }

            var bluetoothDevice = new BluetoothDevice(dlg.SelectedDevice.DeviceAddress);
            DeviceStorage.SaveLastDevice(bluetoothDevice);
            return bluetoothDevice;
        }
    }
}