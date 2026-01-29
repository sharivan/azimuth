using System;
using System.Windows.Forms;

namespace Azimuth
{
    public partial class frmProjectionTypeDialog : Form
    {
        private ProjectionType projectionType;

        public ProjectionType ProjectionType
        {
            get => projectionType;
            set
            {
                projectionType = value;
                cmbProjectionType.SelectedIndex = (int) value;
            }
        }

        public string FileName
        {
            get;
            private set;
        }

        public frmProjectionTypeDialog()
        {
            InitializeComponent();
            projectionType = ProjectionType.AZIMUTHAL;
            cmbProjectionType.SelectedIndex = (int) projectionType;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            DialogResult = openFileDlg.ShowDialog();
            FileName = openFileDlg.FileName;
            projectionType = (ProjectionType) cmbProjectionType.SelectedIndex;
            Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Hide();
        }
    }
}
