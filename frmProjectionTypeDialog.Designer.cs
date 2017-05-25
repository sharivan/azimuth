namespace Azimuth
{
    partial class frmProjectionTypeDialog
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
            this.cmbProjectionType = new System.Windows.Forms.ComboBox();
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblProjectionType = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmbProjectionType
            // 
            this.cmbProjectionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProjectionType.FormattingEnabled = true;
            this.cmbProjectionType.Items.AddRange(new object[] {
            "Azimutal",
            "Mercator"});
            this.cmbProjectionType.Location = new System.Drawing.Point(12, 52);
            this.cmbProjectionType.Name = "cmbProjectionType";
            this.cmbProjectionType.Size = new System.Drawing.Size(185, 21);
            this.cmbProjectionType.TabIndex = 0;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(12, 83);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(91, 19);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Abrir";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(109, 83);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(91, 19);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblProjectionType
            // 
            this.lblProjectionType.Location = new System.Drawing.Point(13, 9);
            this.lblProjectionType.Name = "lblProjectionType";
            this.lblProjectionType.Size = new System.Drawing.Size(183, 34);
            this.lblProjectionType.TabIndex = 3;
            this.lblProjectionType.Text = "Selectione o tipo de projeção:";
            this.lblProjectionType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmProjectionTypeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(209, 105);
            this.ControlBox = false;
            this.Controls.Add(this.lblProjectionType);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.cmbProjectionType);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmProjectionTypeDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Abrir arquivo";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbProjectionType;
        private System.Windows.Forms.OpenFileDialog openFileDlg;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblProjectionType;
    }
}