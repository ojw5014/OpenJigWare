using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenJigWare.Docking
{
    public partial class frmFolder : Form
    {
        public frmFolder()
        {
            InitializeComponent();
        }

        // 폼을 부모폼의 중간에 위치하도록 한다.
        public void CenterParentFrm(Form frmParent)
        {
            int nX, nY;

            nX = (int)((frmParent.Width - this.Width) / 2) + frmParent.Location.X;
            nY = (int)((frmParent.Height - this.Height) / 2) + frmParent.Location.Y;

            this.Location = new Point(nX, nY);
        }

        private void frmFolder_Load(object sender, EventArgs e)
        {

        }
    }
}
