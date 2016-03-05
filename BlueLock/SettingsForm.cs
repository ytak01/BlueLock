using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
