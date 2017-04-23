using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Bluetooth.AttributeIds;
using InTheHand.Net.Sockets;
using System.Diagnostics;

namespace BlueLock
{
    public class BluetoothDevice : BluetoothDeviceInfo
    {
        /// <summary>
        /// The fake service id used for checking device availability.
        /// </summary>
        private static readonly Guid FakeServiceId = new Guid("{F13F471D-47CB-41D6-9609-BAD0690BF891}"); //Guid.Empty

        /// <summary>
        /// Check whether the supplied device is in range.
        /// </summary>
        /// <returns>
        /// Returns whether the device is in range.
        /// </returns>
        public bool IsInRange()
        {
            try
            {
                this.GetServiceRecords(FakeServiceId);
                Debug.WriteLine("Rssi:{0}",this.Rssi);
                return true;
            }
            catch (SocketException e)
            {
                Debug.WriteLine(e.ErrorCode + ":" + e.Message);
                Debug.WriteLine("LastSeen:{0} LastUsed:{1}", this.LastSeen, this.LastUsed);
                return false;
            }
        }

        private static bool firstinrange = true;
        public bool FirstInRange
        {
            get {
                return firstinrange;
            }
            set {
                firstinrange = value;
            }
        }

        public DateTime OutRangeDate
        {
            get;
            private set;
        }

        public DateTime InRangeDate
        {
            get;
            private set;
        }

        public int InRangeCount
        {
            get;
            private set;
        }

        public int OutRangeCount
        {
            get;
            private set;
        }

        private bool dinrange;

        /// <summary>
        /// The current in range state.
        /// </summary>
        public bool DeviceInRange
        {
            get {
                return dinrange;
            }
            set {
                dinrange = value;
                if(dinrange)
                {
                    firstinrange = false;
                    InRangeCount += 1;
                    OutRangeCount = 0;
                    InRangeDate = DateTime.Now;
                    OutRangeDate = DateTime.MinValue;
                }
                else
                {
                    InRangeCount = 0;
                    OutRangeCount += 1;
                    InRangeDate = DateTime.MinValue;
                    OutRangeDate = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Try to get the version information from the current device.
        /// </summary>
        /// <param name="versions">The returned version information.</param>
        /// <returns>
        /// Whether this operation was successful.
        /// </returns>
        public bool TryGetVersions(out RadioVersions versions)
        {
            versions = null;

            this.Refresh();
            if (!this.Connected)
            {
                return false;
            }

            versions = this.GetVersions();
            return versions != null;
        }

        public class DeviceStateEventArgs : EventArgs
        {
            public bool InRange
            {
                get;
                set;
            }
        }

        public event EventHandler<DeviceStateEventArgs> DeviceStateChanged;

        public void TriggerDeviceStateChanged(bool inRange)
        {
            DeviceStateChanged?.Invoke(this, new DeviceStateEventArgs { InRange = inRange });
        }

        /// <summary>
        /// Set the lock state of the workstation.
        /// </summary>
        /// <param name="inRange">Whether the device is currently in range.</param>
        /// <param name="force">Whether to force setting the device state even if it has not changed.</param>
        public void SetLockedState(bool inRange = false, bool force = false)
        {
            if (inRange)
            {
                // Device came back in range
                DeviceInRange = true;
                TriggerDeviceStateChanged(true);
            }
            else if (!inRange)
            {
                // Device went out of range
                DeviceInRange = false;
                TriggerDeviceStateChanged(false);
            }
            else if (force)
            {
                DeviceInRange = inRange;
            }
        }

        /// <summary>
        /// Get a list of installed services.
        /// </summary>
        /// <returns>
        /// The installed services.
        /// </returns>
        public Dictionary<Guid, string> GetInstalledServices()
        {
            var services = new Dictionary<Guid, string>();

            foreach (var service in this.InstalledServices)
            {
                var serviceNames = string.Empty;
                var records = this.GetServiceRecords(service);
                foreach (var record in records.Where(record => record.Contains(UniversalAttributeId.ServiceName) && record.AttributeIds.Contains(UniversalAttributeId.ServiceName)))
                {
                    try
                    {
                        var serviceName = record.GetMultiLanguageStringAttributeById(UniversalAttributeId.ServiceName, LanguageBaseItem.CreateEnglishUtf8PrimaryLanguageItem());
                        serviceNames += (serviceNames.Length > 0 ? ", " : "") + serviceName;
                    }
                    catch (KeyNotFoundException)
                    {
                        // Apparently the check is not enough
                    }
                }

                services.Add(service, serviceNames);
            }

            return services;
        }

        public BluetoothDevice(BluetoothAddress address) : base(address)
        {
            DeviceInRange = true;
        }
    }
}
