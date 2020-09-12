using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenJigWare.Docking
{
    public partial class frmDrawText : Form
    {
        public frmDrawText()
        {
            InitializeComponent();
        }

        private void frmDrawText_Load(object sender, EventArgs e)
        {

        }
        private Ojw.C3d m_C3d { get { return frmDesigner.m_C3d; } set { frmDesigner.m_C3d = value; } }
        private void rtxtDraw_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                int nLine = rtxtDraw.GetLineFromCharIndex(rtxtDraw.GetFirstCharIndexOfCurrentLine());
                if (e.KeyValue == 38) // Up
                {
                    nLine--;
                    if (nLine < 0) nLine = 0;
                }
                else if (e.KeyValue == 40) // Down
                {
                    nLine++;
                    if (nLine >= rtxtDraw.Lines.Length)
                    {
                        nLine = rtxtDraw.Lines.Length - 1;
                    }
                }
                m_C3d.SelectLine(nLine);
            }
            catch
            {
            }
        }

        private void rtxtDraw_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // http://blog.naver.com/PostView.nhn?blogId=gurae83&logNo=20155939347 => 참고
                int nLine = rtxtDraw.GetLineFromCharIndex(rtxtDraw.GetFirstCharIndexOfCurrentLine());

                m_C3d.SelectLine(nLine);
            }
            catch
            {
            }
        }
    }
}
