using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Azimuth
{
    public partial class frmProjectionTypeDialog : Form
    {
        private ProjectionType projectionType;
        private string fileName;

        public ProjectionType ProjectionType
        {
            get { return projectionType; }
            set
            {
                projectionType = value;
                cmbProjectionType.SelectedIndex = (int) value;
            }
        }

        public string FileName
        {
            get { return fileName; }
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
            fileName = openFileDlg.FileName;
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
