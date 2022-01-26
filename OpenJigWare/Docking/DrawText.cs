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
            btnGet_Click(sender, e);

            txtP0_0.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtP0_1.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtP0_2.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtP0_3.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtP1_0.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtP1_1.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtP1_2.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtP1_3.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            
            txtD0_0.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtD0_1.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtD0_2.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtD1_0.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtD1_1.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);
            txtD1_2.MouseWheel +=new MouseEventHandler(txtP0_0_MouseWheel);

            txtAmb0_0.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtAmb0_1.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtAmb0_2.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtAmb0_3.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtAmb1_0.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtAmb1_1.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtAmb1_2.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtAmb1_3.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtAmb2_0.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtAmb2_1.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtAmb2_2.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtAmb2_3.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);

            txtDif0_0.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtDif0_1.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtDif0_2.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtDif0_3.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtDif1_0.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtDif1_1.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtDif1_2.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtDif1_3.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtDif2_0.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtDif2_1.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtDif2_2.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtDif2_3.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);

            txtSpe0_0.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpe0_1.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpe0_2.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpe0_3.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpe1_0.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpe1_1.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpe1_2.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpe1_3.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpe2_0.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpe2_1.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpe2_2.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpe2_3.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);

            txtSpot_0.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtSpot_1.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtExp_0.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtExp_1.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
            txtShin.MouseWheel += new MouseEventHandler(txtP0_0_MouseWheel);
        }
        private void txtP0_0_MouseWheel(object sender, MouseEventArgs e)
        {
            float fDelta = 0.1f;
            float fData = (e.Delta > 0) ? -fDelta : fDelta;

            if (m_nMouse > 0)
            {
                //((TextBox)sender).ReadOnly = true;
                m_fVal += fData;
                ((TextBox)sender).Text = Ojw.CConvert.FloatToStr(m_fVal, 1);
            }

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

        private int m_nStatus = 0;
        private void btnGet_Click(object sender, EventArgs e)
        {
            m_nStatus = 0;
            txtP0_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light0_position[0]);
            txtP0_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light0_position[1]);
            txtP0_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light0_position[2]);
            txtP0_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light0_position[3]);

            txtP1_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light1_position[0]);
            txtP1_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light1_position[1]);
            txtP1_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light1_position[2]);
            txtP1_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light1_position[3]);

            txtD0_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light0_direction[0]);
            txtD0_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light0_direction[1]);
            txtD0_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light0_direction[2]);

            txtD1_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light1_direction[0]);
            txtD1_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light1_direction[1]);
            txtD1_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_light1_direction[2]);

            
            txtAmb0_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_ambient[0]);
            txtAmb0_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_ambient[1]);
            txtAmb0_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_ambient[2]);
            txtAmb0_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_ambient[3]);
            
            txtAmb1_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_ambient2[0]);
            txtAmb1_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_ambient2[1]);
            txtAmb1_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_ambient2[2]);
            txtAmb1_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_ambient2[3]);
                        
            txtAmb2_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_ambient[0]);
            txtAmb2_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_ambient[1]);
            txtAmb2_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_ambient[2]);
            txtAmb2_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_ambient[3]);

            
            
            txtDif0_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_diffuseLight[0]);
            txtDif0_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_diffuseLight[1]);
            txtDif0_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_diffuseLight[2]);
            txtDif0_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_diffuseLight[3]);
            
            txtDif1_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_diffuseLight2[0]);
            txtDif1_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_diffuseLight2[1]);
            txtDif1_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_diffuseLight2[2]);
            txtDif1_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_diffuseLight2[3]);
                        
            txtDif2_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_diffuse[0]);
            txtDif2_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_diffuse[1]);
            txtDif2_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_diffuse[2]);
            txtDif2_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_diffuse[3]);

            
            
            txtSpe0_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_specular[0]);
            txtSpe0_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_specular[1]);
            txtSpe0_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_specular[2]);
            txtSpe0_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_specular[3]);
            
            txtSpe1_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_specular2[0]);
            txtSpe1_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_specular2[1]);
            txtSpe1_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_specular2[2]);
            txtSpe1_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_specular2[3]);
                        
            txtSpe2_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_specular[0]);
            txtSpe2_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_specular[1]);
            txtSpe2_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_specular[2]);
            txtSpe2_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_specular[3]);

            txtSpot_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_fSpot);
            txtSpot_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_fSpot2);

            txtExp_0.Text = Ojw.CConvert.FloatToStr(m_C3d.m_fExponent);
            txtExp_1.Text = Ojw.CConvert.FloatToStr(m_C3d.m_fExponent2);

            txtShin.Text = Ojw.CConvert.FloatToStr(m_C3d.m_mat_shiness[0]);
                
                    
                    
            
            m_nStatus = 1;
        }

        private void txtP0_0_TextChanged(object sender, EventArgs e)
        {
            SetLight_Sync();
        }
        private void SetLight_Sync()
        {
            if (m_nStatus > 0)
            {
                //m_C3d.m_light0_position[0] = Ojw.CConvert.StrToFloat(txtP0_0.Text);
                //m_C3d.m_light0_position[1] = Ojw.CConvert.StrToFloat(txtP0_1.Text);
                //m_C3d.m_light0_position[2] = Ojw.CConvert.StrToFloat(txtP0_2.Text);
                //m_C3d.m_light0_position[3] = Ojw.CConvert.StrToFloat(txtP0_3.Text);

                //m_C3d.m_light1_position[0] = Ojw.CConvert.StrToFloat(txtP1_0.Text);
                //m_C3d.m_light1_position[1] = Ojw.CConvert.StrToFloat(txtP1_1.Text);
                //m_C3d.m_light1_position[2] = Ojw.CConvert.StrToFloat(txtP1_2.Text);
                //m_C3d.m_light1_position[3] = Ojw.CConvert.StrToFloat(txtP1_3.Text);






                m_C3d.m_light0_position[0] = Ojw.CConvert.StrToFloat(txtP0_0.Text);
                m_C3d.m_light0_position[1] = Ojw.CConvert.StrToFloat(txtP0_1.Text);
                m_C3d.m_light0_position[2] = Ojw.CConvert.StrToFloat(txtP0_2.Text);
                m_C3d.m_light0_position[3] = Ojw.CConvert.StrToFloat(txtP0_3.Text);

                m_C3d.m_light1_position[0] = Ojw.CConvert.StrToFloat(txtP1_0.Text);
                m_C3d.m_light1_position[1] = Ojw.CConvert.StrToFloat(txtP1_1.Text);
                m_C3d.m_light1_position[2] = Ojw.CConvert.StrToFloat(txtP1_2.Text);
                m_C3d.m_light1_position[3] = Ojw.CConvert.StrToFloat(txtP1_3.Text);

                m_C3d.m_light0_direction[0] = Ojw.CConvert.StrToFloat(txtD0_0.Text);
                m_C3d.m_light0_direction[1] = Ojw.CConvert.StrToFloat(txtD0_1.Text);
                m_C3d.m_light0_direction[2] = Ojw.CConvert.StrToFloat(txtD0_2.Text);

                m_C3d.m_light1_direction[0] = Ojw.CConvert.StrToFloat(txtD1_0.Text);
                m_C3d.m_light1_direction[1] = Ojw.CConvert.StrToFloat(txtD1_1.Text);
                m_C3d.m_light1_direction[2] = Ojw.CConvert.StrToFloat(txtD1_2.Text);






                m_C3d.m_specular[0] = Ojw.CConvert.StrToFloat(txtSpe0_0.Text);
                m_C3d.m_specular[1] = Ojw.CConvert.StrToFloat(txtSpe0_1.Text);
                m_C3d.m_specular[2] = Ojw.CConvert.StrToFloat(txtSpe0_2.Text);
                m_C3d.m_specular[3] = Ojw.CConvert.StrToFloat(txtSpe0_3.Text);

                m_C3d.m_specular2[0] = Ojw.CConvert.StrToFloat(txtSpe1_0.Text);
                m_C3d.m_specular2[1] = Ojw.CConvert.StrToFloat(txtSpe1_1.Text);
                m_C3d.m_specular2[2] = Ojw.CConvert.StrToFloat(txtSpe1_2.Text);
                m_C3d.m_specular2[3] = Ojw.CConvert.StrToFloat(txtSpe1_3.Text);

                m_C3d.m_mat_specular[0] = Ojw.CConvert.StrToFloat(txtSpe2_0.Text);
                m_C3d.m_mat_specular[1] = Ojw.CConvert.StrToFloat(txtSpe2_1.Text);
                m_C3d.m_mat_specular[2] = Ojw.CConvert.StrToFloat(txtSpe2_2.Text);
                m_C3d.m_mat_specular[3] = Ojw.CConvert.StrToFloat(txtSpe2_3.Text);

                m_C3d.m_fSpot = Ojw.CConvert.StrToFloat(txtSpot_0.Text);
                m_C3d.m_fSpot2 = Ojw.CConvert.StrToFloat(txtSpot_1.Text);
                m_C3d.m_fExponent = Ojw.CConvert.StrToFloat(txtExp_0.Text);
                m_C3d.m_fExponent2 = Ojw.CConvert.StrToFloat(txtExp_1.Text);

                m_C3d.m_mat_shiness[0] = Ojw.CConvert.StrToFloat(txtShin.Text);




                m_C3d.m_ambient[0] = Ojw.CConvert.StrToFloat(txtAmb0_0.Text);
                m_C3d.m_ambient[1] = Ojw.CConvert.StrToFloat(txtAmb0_1.Text);
                m_C3d.m_ambient[2] = Ojw.CConvert.StrToFloat(txtAmb0_2.Text);
                m_C3d.m_ambient[3] = Ojw.CConvert.StrToFloat(txtAmb0_3.Text);

                m_C3d.m_ambient2[0] = Ojw.CConvert.StrToFloat(txtAmb1_0.Text);
                m_C3d.m_ambient2[1] = Ojw.CConvert.StrToFloat(txtAmb1_1.Text);
                m_C3d.m_ambient2[2] = Ojw.CConvert.StrToFloat(txtAmb1_2.Text);
                m_C3d.m_ambient2[3] = Ojw.CConvert.StrToFloat(txtAmb1_3.Text);

                m_C3d.m_mat_ambient[0] = Ojw.CConvert.StrToFloat(txtAmb2_0.Text);
                m_C3d.m_mat_ambient[1] = Ojw.CConvert.StrToFloat(txtAmb2_1.Text);
                m_C3d.m_mat_ambient[2] = Ojw.CConvert.StrToFloat(txtAmb2_2.Text);
                m_C3d.m_mat_ambient[3] = Ojw.CConvert.StrToFloat(txtAmb2_3.Text);

                m_C3d.m_diffuseLight[0] = Ojw.CConvert.StrToFloat(txtDif0_0.Text);
                m_C3d.m_diffuseLight[1] = Ojw.CConvert.StrToFloat(txtDif0_1.Text);
                m_C3d.m_diffuseLight[2] = Ojw.CConvert.StrToFloat(txtDif0_2.Text);
                m_C3d.m_diffuseLight[3] = Ojw.CConvert.StrToFloat(txtDif0_3.Text);

                m_C3d.m_diffuseLight2[0] = Ojw.CConvert.StrToFloat(txtDif1_0.Text);
                m_C3d.m_diffuseLight2[1] = Ojw.CConvert.StrToFloat(txtDif1_1.Text);
                m_C3d.m_diffuseLight2[2] = Ojw.CConvert.StrToFloat(txtDif1_2.Text);
                m_C3d.m_diffuseLight2[3] = Ojw.CConvert.StrToFloat(txtDif1_3.Text);

                m_C3d.m_mat_diffuse[0] = Ojw.CConvert.StrToFloat(txtDif2_0.Text);
                m_C3d.m_mat_diffuse[1] = Ojw.CConvert.StrToFloat(txtDif2_1.Text);
                m_C3d.m_mat_diffuse[2] = Ojw.CConvert.StrToFloat(txtDif2_2.Text);
                m_C3d.m_mat_diffuse[3] = Ojw.CConvert.StrToFloat(txtDif2_3.Text);


                m_C3d.m_specular[0] = Ojw.CConvert.StrToFloat(txtSpe0_0.Text);
                m_C3d.m_specular[1] = Ojw.CConvert.StrToFloat(txtSpe0_1.Text);
                m_C3d.m_specular[2] = Ojw.CConvert.StrToFloat(txtSpe0_2.Text);
                m_C3d.m_specular[3] = Ojw.CConvert.StrToFloat(txtSpe0_3.Text);

                m_C3d.m_specular2[0] = Ojw.CConvert.StrToFloat(txtSpe1_0.Text);
                m_C3d.m_specular2[1] = Ojw.CConvert.StrToFloat(txtSpe1_1.Text);
                m_C3d.m_specular2[2] = Ojw.CConvert.StrToFloat(txtSpe1_2.Text);
                m_C3d.m_specular2[3] = Ojw.CConvert.StrToFloat(txtSpe1_3.Text);

                m_C3d.m_mat_specular[0] = Ojw.CConvert.StrToFloat(txtSpe2_0.Text);
                m_C3d.m_mat_specular[1] = Ojw.CConvert.StrToFloat(txtSpe2_1.Text);
                m_C3d.m_mat_specular[2] = Ojw.CConvert.StrToFloat(txtSpe2_2.Text);
                m_C3d.m_mat_specular[3] = Ojw.CConvert.StrToFloat(txtSpe2_3.Text);
            }
        }
        /*
        private void MouseDown(nMode)
        {            
            m_C3d.m_light0_position[0] = Ojw.CConvert.StrToFloat(txtP0_0.Text);
            m_C3d.m_light0_position[1] = Ojw.CConvert.StrToFloat(txtP0_1.Text);
            m_C3d.m_light0_position[2] = Ojw.CConvert.StrToFloat(txtP0_2.Text);
            m_C3d.m_light0_position[3] = Ojw.CConvert.StrToFloat(txtP0_3.Text);
        }*/
        private int m_nMouse = 0;
        private int m_nMouse_X = 0;
        private int m_nMouse_Y = 0;
        private float m_fVal = 0.0f;
        private void txtP0_0_MouseDown(object sender, MouseEventArgs e)
        {
            m_fVal = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            m_nMouse_X = e.X;
            m_nMouse_Y = e.Y;
            m_nMouse = 1;
        }

        private void txtP0_0_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_nMouse > 0)
            {
                //((TextBox)sender).ReadOnly = true;
                //float fVal = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
                //int nX = m_nMouse_X - e.X;
                //int nY = m_nMouse_Y - e.Y;
                //if (nX > 2) m_fVal -= 0.1f;
                //else if (nX > -2) m_fVal += 0.1f;
                //((TextBox)sender).Text = Ojw.CConvert.FloatToStr(m_fVal,1);
            }
        }

        private void txtP0_0_MouseUp(object sender, MouseEventArgs e)
        {
            m_nMouse = 0;
            //((TextBox)sender).ReadOnly = false;
        }
    }
}
