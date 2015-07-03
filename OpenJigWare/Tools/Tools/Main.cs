using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenJigWare;

namespace Tools
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }
        private Ojw.CTools m_CTool = new Ojw.CTools();
        //private Ojw.C3d m_C3d = new Ojw.C3d();
        private void btn3D_Click(object sender, EventArgs e)
        {
            m_CTool.ShowTools_Modeling();
        }

        private void btn3D_Full_Click(object sender, EventArgs e)
        {
            m_CTool.ShowTools_Modeling(Ojw.CConvert.StrToFloat(txtRatio.Text));
            //m_CTool.ShowTools_Modeling(0);
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void txtRatio_MouseClick(object sender, MouseEventArgs e)
        {
            Ojw.ShowKeyPad_Number(sender);
        }

        private void txtTest_MouseClick(object sender, MouseEventArgs e)
        {
            Ojw.ShowKeyPad_Alpha(sender);
        }
    }
}
