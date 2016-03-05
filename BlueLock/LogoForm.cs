using System.Drawing;
using System.Windows.Forms;

namespace BlueLock
{
    public partial class LogoForm : Form
    {
        private static MainForm _mainForm;

        public LogoForm()
        {
            InitializeComponent();
        }

        public LogoForm(MainForm mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
        }

        public void SetStatusImage(Image image)
        {
            pctStatus.Image = image;
        }

        public void SetStatusBackground(Color color)
        {
            this.BackColor = color;
            pctStatus.BackColor = color;
        }

        private void pctStatus_DoubleClick(object sender, System.EventArgs e)
        {
            this.Hide();
            _mainForm.WindowState = FormWindowState.Normal;
        }
    }
}
