namespace BlueLock
{
    partial class LogoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogoForm));
            this.pctStatus = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pctStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // pctStatus
            // 
            this.pctStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pctStatus.BackColor = System.Drawing.Color.Orange;
            this.pctStatus.Location = new System.Drawing.Point(5, 5);
            this.pctStatus.Name = "pctStatus";
            this.pctStatus.Size = new System.Drawing.Size(128, 118);
            this.pctStatus.TabIndex = 0;
            this.pctStatus.TabStop = false;
            this.pctStatus.DoubleClick += new System.EventHandler(this.pctStatus_DoubleClick);
            // 
            // LogoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Orange;
            this.ClientSize = new System.Drawing.Size(138, 127);
            this.ControlBox = false;
            this.Controls.Add(this.pctStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogoForm";
            this.ShowInTaskbar = false;
            this.Text = "BlueLock_";
            ((System.ComponentModel.ISupportInitialize)(this.pctStatus)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pctStatus;
    }
}