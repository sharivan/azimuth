namespace Azimuth
{
    partial class frmAzimuth
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAzimuth));
            this.pnl3D = new System.Windows.Forms.Panel();
            this.wpf3DHost = new System.Windows.Forms.Integration.ElementHost();
            this.pnl2D = new System.Windows.Forms.Panel();
            this.tmrTick = new System.Windows.Forms.Timer(this.components);
            this.ssStatusBar = new System.Windows.Forms.StatusStrip();
            this.tsslText = new System.Windows.Forms.ToolStripStatusLabel();
            this.tbToolBar = new System.Windows.Forms.ToolStrip();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSelection = new System.Windows.Forms.ToolStripButton();
            this.btnPencil = new System.Windows.Forms.ToolStripButton();
            this.btnLine = new System.Windows.Forms.ToolStripButton();
            this.btnDefineFramework = new System.Windows.Forms.ToolStripButton();
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.btnClear = new System.Windows.Forms.ToolStripButton();
            this.btnGeodesic = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.colorDlg = new System.Windows.Forms.ColorDialog();
            this.btnColor = new System.Windows.Forms.ToolStripButton();
            this.pnl3D.SuspendLayout();
            this.ssStatusBar.SuspendLayout();
            this.tbToolBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnl3D
            // 
            this.pnl3D.Controls.Add(this.wpf3DHost);
            this.pnl3D.Location = new System.Drawing.Point(1, 2);
            this.pnl3D.Name = "pnl3D";
            this.pnl3D.Size = new System.Drawing.Size(500, 500);
            this.pnl3D.TabIndex = 0;
            // 
            // wpf3DHost
            // 
            this.wpf3DHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpf3DHost.Location = new System.Drawing.Point(0, 0);
            this.wpf3DHost.Name = "wpf3DHost";
            this.wpf3DHost.Size = new System.Drawing.Size(500, 500);
            this.wpf3DHost.TabIndex = 0;
            this.wpf3DHost.Child = null;
            // 
            // pnl2D
            // 
            this.pnl2D.Location = new System.Drawing.Point(511, 2);
            this.pnl2D.Name = "pnl2D";
            this.pnl2D.Size = new System.Drawing.Size(500, 500);
            this.pnl2D.TabIndex = 1;
            this.pnl2D.Paint += new System.Windows.Forms.PaintEventHandler(this.pnl2D_Paint);
            this.pnl2D.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnl2D_MouseDown);
            this.pnl2D.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnl2D_MouseMove);
            this.pnl2D.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnl2D_MouseUp);
            // 
            // tmrTick
            // 
            this.tmrTick.Interval = 16;
            // 
            // ssStatusBar
            // 
            this.ssStatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslText});
            this.ssStatusBar.Location = new System.Drawing.Point(0, 478);
            this.ssStatusBar.Name = "ssStatusBar";
            this.ssStatusBar.Size = new System.Drawing.Size(1016, 22);
            this.ssStatusBar.TabIndex = 2;
            // 
            // tsslText
            // 
            this.tsslText.Name = "tsslText";
            this.tsslText.Size = new System.Drawing.Size(0, 17);
            // 
            // tbToolBar
            // 
            this.tbToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpen,
            this.toolStripSeparator1,
            this.btnSelection,
            this.btnPencil,
            this.btnLine,
            this.btnGeodesic,
            this.btnDefineFramework,
            this.btnClear,
            this.toolStripSeparator2,
            this.btnColor});
            this.tbToolBar.Location = new System.Drawing.Point(0, 0);
            this.tbToolBar.Name = "tbToolBar";
            this.tbToolBar.Size = new System.Drawing.Size(1016, 25);
            this.tbToolBar.TabIndex = 3;
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(23, 22);
            this.btnOpen.ToolTipText = "Abrir Arquivo";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnSelection
            // 
            this.btnSelection.Checked = true;
            this.btnSelection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnSelection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelection.Image = ((System.Drawing.Image)(resources.GetObject("btnSelection.Image")));
            this.btnSelection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelection.Name = "btnSelection";
            this.btnSelection.Size = new System.Drawing.Size(23, 22);
            this.btnSelection.ToolTipText = "Seleção";
            this.btnSelection.Click += new System.EventHandler(this.btnSelection_Click);
            // 
            // btnPencil
            // 
            this.btnPencil.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPencil.Image = ((System.Drawing.Image)(resources.GetObject("btnPencil.Image")));
            this.btnPencil.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPencil.Name = "btnPencil";
            this.btnPencil.Size = new System.Drawing.Size(23, 22);
            this.btnPencil.Text = "toolStripButton3";
            this.btnPencil.ToolTipText = "Lápis";
            this.btnPencil.Click += new System.EventHandler(this.btnPencil_Click);
            // 
            // btnLine
            // 
            this.btnLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLine.Image = ((System.Drawing.Image)(resources.GetObject("btnLine.Image")));
            this.btnLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLine.Name = "btnLine";
            this.btnLine.Size = new System.Drawing.Size(23, 22);
            this.btnLine.Text = "toolStripButton1";
            this.btnLine.ToolTipText = "Linha Reta";
            this.btnLine.Click += new System.EventHandler(this.btnLine_Click);
            // 
            // btnDefineFramework
            // 
            this.btnDefineFramework.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDefineFramework.Image = ((System.Drawing.Image)(resources.GetObject("btnDefineFramework.Image")));
            this.btnDefineFramework.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDefineFramework.Name = "btnDefineFramework";
            this.btnDefineFramework.Size = new System.Drawing.Size(23, 22);
            this.btnDefineFramework.Text = "Definir Enquadramento";
            this.btnDefineFramework.Click += new System.EventHandler(this.btnDefineFramework_Click);
            // 
            // btnClear
            // 
            this.btnClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnClear.Image = ((System.Drawing.Image)(resources.GetObject("btnClear.Image")));
            this.btnClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(23, 22);
            this.btnClear.Text = "Limpar";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnGeodesic
            // 
            this.btnGeodesic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnGeodesic.Image = ((System.Drawing.Image)(resources.GetObject("btnGeodesic.Image")));
            this.btnGeodesic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnGeodesic.Name = "btnGeodesic";
            this.btnGeodesic.Size = new System.Drawing.Size(23, 22);
            this.btnGeodesic.Text = "Geodésica";
            this.btnGeodesic.Click += new System.EventHandler(this.btnGeodesic_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // colorDlg
            // 
            this.colorDlg.Color = System.Drawing.Color.Red;
            // 
            // btnColor
            // 
            this.btnColor.BackColor = System.Drawing.Color.Red;
            this.btnColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnColor.ForeColor = System.Drawing.Color.Black;
            this.btnColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnColor.Name = "btnColor";
            this.btnColor.Size = new System.Drawing.Size(23, 22);
            this.btnColor.ToolTipText = "Cor do traçado";
            this.btnColor.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // frmAzimuth
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 500);
            this.Controls.Add(this.tbToolBar);
            this.Controls.Add(this.ssStatusBar);
            this.Controls.Add(this.pnl2D);
            this.Controls.Add(this.pnl3D);
            this.DoubleBuffered = true;
            this.Name = "frmAzimuth";
            this.Text = "Azimuth";
            this.Load += new System.EventHandler(this.frmAzimuth_Load);
            this.Resize += new System.EventHandler(this.frmAzimuth_Resize);
            this.pnl3D.ResumeLayout(false);
            this.ssStatusBar.ResumeLayout(false);
            this.ssStatusBar.PerformLayout();
            this.tbToolBar.ResumeLayout(false);
            this.tbToolBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnl3D;
        private System.Windows.Forms.Panel pnl2D;
        private System.Windows.Forms.Timer tmrTick;
        private System.Windows.Forms.Integration.ElementHost wpf3DHost;
        private System.Windows.Forms.StatusStrip ssStatusBar;
        private System.Windows.Forms.ToolStripStatusLabel tsslText;
        private System.Windows.Forms.ToolStrip tbToolBar;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnSelection;
        private System.Windows.Forms.ToolStripButton btnPencil;
        private System.Windows.Forms.OpenFileDialog openFileDlg;
        private System.Windows.Forms.ToolStripButton btnLine;
        private System.Windows.Forms.ToolStripButton btnDefineFramework;
        private System.Windows.Forms.ToolStripButton btnClear;
        private System.Windows.Forms.ToolStripButton btnGeodesic;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ColorDialog colorDlg;
        private System.Windows.Forms.ToolStripButton btnColor;
    }
}

