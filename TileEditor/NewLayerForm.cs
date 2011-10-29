using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TileEditor
{
    public partial class NewLayerForm : Form
    {

        public bool OKPressed = false;
        public NewLayerForm()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            OKPressed = true;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            OKPressed = false;
            Close();
        }
    }
}
