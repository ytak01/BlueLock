using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Windows.Forms;

namespace BlueLock
{
    public partial class SettingsForm : Form
    {
        private static StatusForm _statusForm;

        public SettingsForm(StatusForm statusForm)
        {
            InitializeComponent();
            _statusForm = statusForm;
            txtCurrent.Text = StatusForm.Device?.DeviceName;
            _statusForm.InitialDeviceState += (sender, args) =>
            {
                txtCurrent.Text = StatusForm.Device?.DeviceName;
            };
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            this.Closing += SettingsForm_Closing;
        }

        private void SettingsForm_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            _statusForm.SelectDevice();
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

            base.WndProc(ref m);
        }
    }
}
