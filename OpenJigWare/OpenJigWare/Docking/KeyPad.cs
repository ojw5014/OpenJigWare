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
    public partial class CKeyPad_t : Form
    {
        private int m_nLeft = 0;
        private int m_nTop = 0;
        private bool m_bNumPad = true;
        
        private const int _BUTTON_LEFT = 1;
        private const int _BUTTON_TOP = 1;//2;
        private const int _BUTTON_WIDTH = 49;//67;
        private const int _BUTTON_HEIGHT = 43;//50;

        public CKeyPad_t()
        {
            InitializeComponent();
        }
        private void SetButtonPos(Label lb, int nX, int nY)
        {
            int nLeft = _BUTTON_LEFT;
            int nTop = _BUTTON_TOP;
            int nW = _BUTTON_WIDTH;
            int nH = _BUTTON_HEIGHT;

            lb.Left = nLeft + nW * nX;
            lb.Top = nTop + nH * nY;
        }
        private void SetKeyPadImage(bool bShift)
        {
            if (bShift == false)
            {
                lbAlpha_q.Image = global::OpenJigWare.Properties.Resources.es_nomal_q;
                lbAlpha_w.Image = global::OpenJigWare.Properties.Resources.es_nomal_w;
                lbAlpha_e.Image = global::OpenJigWare.Properties.Resources.es_nomal_e;
                lbAlpha_r.Image = global::OpenJigWare.Properties.Resources.es_nomal_r;
                lbAlpha_t.Image = global::OpenJigWare.Properties.Resources.es_nomal_t;
                lbAlpha_y.Image = global::OpenJigWare.Properties.Resources.es_nomal_y;
                lbAlpha_u.Image = global::OpenJigWare.Properties.Resources.es_nomal_u;
                lbAlpha_i.Image = global::OpenJigWare.Properties.Resources.es_nomal_i;
                lbAlpha_o.Image = global::OpenJigWare.Properties.Resources.es_nomal_o;
                lbAlpha_p.Image = global::OpenJigWare.Properties.Resources.es_nomal_p;
                lbAlpha_a.Image = global::OpenJigWare.Properties.Resources.es_nomal_a;
                lbAlpha_s.Image = global::OpenJigWare.Properties.Resources.es_nomal_s;
                lbAlpha_d.Image = global::OpenJigWare.Properties.Resources.es_nomal_d;
                lbAlpha_f.Image = global::OpenJigWare.Properties.Resources.es_nomal_f;
                lbAlpha_g.Image = global::OpenJigWare.Properties.Resources.es_nomal_g;
                lbAlpha_h.Image = global::OpenJigWare.Properties.Resources.es_nomal_h;
                lbAlpha_j.Image = global::OpenJigWare.Properties.Resources.es_nomal_j;
                lbAlpha_k.Image = global::OpenJigWare.Properties.Resources.es_nomal_k;
                lbAlpha_l.Image = global::OpenJigWare.Properties.Resources.es_nomal_l;
                lbAlpha_z.Image = global::OpenJigWare.Properties.Resources.es_nomal_z;
                lbAlpha_x.Image = global::OpenJigWare.Properties.Resources.es_nomal_x;
                lbAlpha_c.Image = global::OpenJigWare.Properties.Resources.es_nomal_c;
                lbAlpha_v.Image = global::OpenJigWare.Properties.Resources.es_nomal_v;
                lbAlpha_b.Image = global::OpenJigWare.Properties.Resources.es_nomal_b;
                lbAlpha_n.Image = global::OpenJigWare.Properties.Resources.es_nomal_n;
                lbAlpha_m.Image = global::OpenJigWare.Properties.Resources.es_nomal_m;
            }
            else
            {
                lbAlpha_q.Image = global::OpenJigWare.Properties.Resources.eL_normal_Q;
                lbAlpha_w.Image = global::OpenJigWare.Properties.Resources.eL_normal_W;
                lbAlpha_e.Image = global::OpenJigWare.Properties.Resources.eL_normal_E;
                lbAlpha_r.Image = global::OpenJigWare.Properties.Resources.eL_normal_R;
                lbAlpha_t.Image = global::OpenJigWare.Properties.Resources.eL_normal_T;
                lbAlpha_y.Image = global::OpenJigWare.Properties.Resources.eL_normal_Y;
                lbAlpha_u.Image = global::OpenJigWare.Properties.Resources.eL_normal_U;
                lbAlpha_i.Image = global::OpenJigWare.Properties.Resources.eL_normal_I;
                lbAlpha_o.Image = global::OpenJigWare.Properties.Resources.eL_normal_O;
                lbAlpha_p.Image = global::OpenJigWare.Properties.Resources.eL_normal_P;
                lbAlpha_a.Image = global::OpenJigWare.Properties.Resources.eL_normal_A;
                lbAlpha_s.Image = global::OpenJigWare.Properties.Resources.eL_normal_S;
                lbAlpha_d.Image = global::OpenJigWare.Properties.Resources.eL_normal_D;
                lbAlpha_f.Image = global::OpenJigWare.Properties.Resources.eL_normal_F;
                lbAlpha_g.Image = global::OpenJigWare.Properties.Resources.eL_normal_G;
                lbAlpha_h.Image = global::OpenJigWare.Properties.Resources.eL_normal_H;
                lbAlpha_j.Image = global::OpenJigWare.Properties.Resources.eL_normal_J;
                lbAlpha_k.Image = global::OpenJigWare.Properties.Resources.eL_normal_K;
                lbAlpha_l.Image = global::OpenJigWare.Properties.Resources.eL_normal_L;
                lbAlpha_z.Image = global::OpenJigWare.Properties.Resources.eL_normal_Z;
                lbAlpha_x.Image = global::OpenJigWare.Properties.Resources.eL_normal_X;
                lbAlpha_c.Image = global::OpenJigWare.Properties.Resources.eL_normal_C;
                lbAlpha_v.Image = global::OpenJigWare.Properties.Resources.eL_normal_V;
                lbAlpha_b.Image = global::OpenJigWare.Properties.Resources.eL_normal_B;
                lbAlpha_n.Image = global::OpenJigWare.Properties.Resources.eL_normal_N;
                lbAlpha_m.Image = global::OpenJigWare.Properties.Resources.eL_normal_M;
            }
        }
        private void KeyPad_Init(bool bNumPad)
        {
            int nX;
            int nY;

            int nLeft = _BUTTON_LEFT;
            int nTop = _BUTTON_TOP;
            int nW = _BUTTON_WIDTH;
            int nH = _BUTTON_HEIGHT;
            if (bNumPad == true)
            {
                nX = 0; nY = 0;
                SetButtonPos(lb_0_0, nX++, nY); // 1
                SetButtonPos(lb_0_1, nX++, nY); // 2
                SetButtonPos(lb_0_2, nX++, nY); // 3
                nX = 0; nY++;
                SetButtonPos(lb_0_3, nX++, nY); // 4
                SetButtonPos(lb_0_4, nX++, nY); // 5
                SetButtonPos(lb_0_5, nX++, nY); // 6
                nX = 0; nY++;
                SetButtonPos(lb_0_6, nX++, nY); // 7
                SetButtonPos(lb_0_7, nX++, nY); // 8
                SetButtonPos(lb_0_8, nX++, nY); // 9
                nX = 0; nY++;
                SetButtonPos(lb_Point, nX++, nY); // Point
                SetButtonPos(lb_0_9, nX++, nY); // 0
                SetButtonPos(lb_1_10, nX++, nY); // Back
                nX = 0; nY++;
                SetButtonPos(lbDummy, nX++, nY); // Dummy
                nX++;
                SetButtonPos(lb_2_10, nX++, nY); // Enter
                nY++;
                lbDummy.Visible = true;
                pnNum.Width = nLeft * 2 + nW * nX;
                pnNum.Height = nTop * 2 + nH * nY;
                this.Width = pnNum.Width + 2;
                this.Height = pnNum.Height + 2;
            }
            else
            {
                int nX2 = 0;
                nX = 0; nY = 0;
                SetButtonPos(lb_0_0, nX++, nY); // 1
                SetButtonPos(lb_0_1, nX++, nY); // 2
                SetButtonPos(lb_0_2, nX++, nY); // 3
                SetButtonPos(lb_0_3, nX++, nY); // 4
                SetButtonPos(lb_0_4, nX++, nY); // 5
                SetButtonPos(lb_0_5, nX++, nY); // 6
                SetButtonPos(lb_0_6, nX++, nY); // 7
                SetButtonPos(lb_0_7, nX++, nY); // 8
                SetButtonPos(lb_0_8, nX++, nY); // 9
                SetButtonPos(lb_0_9, nX++, nY); // 0
                nX2 = nX;
                nX = 0; nY++;
                SetButtonPos(lbAlpha_q, nX++, nY);
                SetButtonPos(lbAlpha_w, nX++, nY);
                SetButtonPos(lbAlpha_e, nX++, nY);
                SetButtonPos(lbAlpha_r, nX++, nY);
                SetButtonPos(lbAlpha_t, nX++, nY);
                SetButtonPos(lbAlpha_y, nX++, nY);
                SetButtonPos(lbAlpha_u, nX++, nY);
                SetButtonPos(lbAlpha_i, nX++, nY);
                SetButtonPos(lbAlpha_o, nX++, nY);
                SetButtonPos(lbAlpha_p, nX++, nY);
                nX = 0; nY++;
                SetButtonPos(lbAlpha_a, nX++, nY);
                SetButtonPos(lbAlpha_s, nX++, nY);
                SetButtonPos(lbAlpha_d, nX++, nY);
                SetButtonPos(lbAlpha_f, nX++, nY);
                SetButtonPos(lbAlpha_g, nX++, nY);
                SetButtonPos(lbAlpha_h, nX++, nY);
                SetButtonPos(lbAlpha_j, nX++, nY);
                SetButtonPos(lbAlpha_k, nX++, nY);
                SetButtonPos(lbAlpha_l, nX++, nY);
                SetButtonPos(lb_1_10, nX++, nY); // Back
                nX = 0; nY++;
                SetButtonPos(lbAlpha_Shift, nX++, nY); // Shift
                SetButtonPos(lbAlpha_z, nX++, nY);
                SetButtonPos(lbAlpha_x, nX++, nY);
                SetButtonPos(lbAlpha_c, nX++, nY);
                SetButtonPos(lbAlpha_v, nX++, nY);
                SetButtonPos(lbAlpha_Space, nX++, nY); nX++;
                SetButtonPos(lbAlpha_b, nX++, nY);
                SetButtonPos(lbAlpha_n, nX++, nY);
                SetButtonPos(lbAlpha_m, nX++, nY);

                SetButtonPos(lb_Point, nX++, nY); // Point
                nX = 0; nY++;
                //SetButtonPos(lbDummy, nX++, nY); // Dummy
                //nX++;
                lbDummy.Visible = false;
                SetButtonPos(lb_2_10, nX++, nY); // Enter
                nY++;

                pnNum.Width = nLeft * 2 + nW * nX2;
                pnNum.Height = nTop * 2 + nH * nY;
                this.Width = pnNum.Width + 2;
                this.Height = pnNum.Height + 2;
            }
        }
        private void KeyPad_Load(object sender, EventArgs e)
        {
            this.Left = m_nLeft;
            this.Top = m_nTop;

            KeyPad_Init(m_bNumPad);
        }

        //private int m_nWidth = 206;
        //private int m_nHeight = 255;
        public void SetMode(bool bNumPad)
        {
            m_bNumPad = bNumPad;
            KeyPad_Init(bNumPad);
            //if (bNumPad == true)
            //{
            //    m_nWidth = 206;
            //    m_nHeight = 255;
            //}
            //else
            //{
            //    m_nWidth = 206;
            //    m_nHeight = 255;
            //}
        }

        public void SetLocation(int nX, int nY)
        {
            m_nLeft = nX;
            m_nTop = nY;
            this.Left = nX;
            this.Top = nY;
        }

        private TextBox m_txtData = null;
        public void SetTextBox(TextBox txt)
        {
            m_txtData = txt;
        }
        private bool m_bShift = false;
        private void AppendText(string str)
        {
            if (m_bShift == true)
            {
                if (str == "-") str = "_";
                else str = str.ToUpper();
            }
            else
            {
                if (str == "_") str = "-";
                str = str.ToLower();
            }

            //m_txtData.AppendText(str);       // 채팅 목록 창에 문자열 추가
            m_txtData.Text = m_txtData.Text + str;
            m_txtData.ScrollToCaret();
        }
        private void DeleteLastChar()
        {
            if (m_txtData.Text.Length > 0) m_txtData.Text = m_txtData.Text.Remove(m_txtData.Text.Length - 1);
            m_txtData.ScrollToCaret();
        }

        private void lb_MouseDown(object sender, MouseEventArgs e)
        {
            #region MouseDown
            ((Label)sender).BorderStyle = BorderStyle.Fixed3D;
            #endregion MouseDown
        }

        private void lb_MouseUp(object sender, MouseEventArgs e)
        {
            #region MouseUp
            ((Label)sender).BorderStyle = BorderStyle.Fixed3D;
            switch(((Label)sender).Name)
            {
                case "lb_0_0": AppendText("1"); break;
                case "lb_0_1": AppendText("2"); break;
                case "lb_0_2": AppendText("3"); break;
                case "lb_0_3": AppendText("4"); break;
                case "lb_0_4": AppendText("5"); break;
                case "lb_0_5": AppendText("6"); break;
                case "lb_0_6": AppendText("7"); break;
                case "lb_0_7": AppendText("8"); break;
                case "lb_0_8": AppendText("9"); break;
                case "lb_0_9": AppendText("0"); break;
                case "lb_1_10": DeleteLastChar(); break; // Back
                case "lb_2_10": Close(); break; // Enter
                case "lb_Point":
                {
                    if (m_bNumPad)
                    {
                        if (m_txtData.Text.IndexOf(".") < 0)
                        {
                            if (m_txtData.Text.IndexOf(".") == 0) AppendText("0");
                            AppendText(".");
                        }
                    }
                    else
                    {
                        AppendText(".");
                    }
                }
                break;
                //case "lbAlpha_MouseUp":
                //{
                //    ((Label)sender).BorderStyle = BorderStyle.None;
                //}
                //break;
            }
            #endregion MouseUp
        }
        
        private void lb_MouseClick(object sender, EventArgs e)
        {
            #region MouseClick
            switch(((Label)sender).Name)
            {
                case "lbAlpha_l": AppendText("l"); break;
                case "lbAlpha_h": AppendText("h"); break;
                case "lbAlpha_n": AppendText("n"); break;
                case "lbAlpha_b": AppendText("b"); break;
                case "lbAlpha_d": AppendText("d"); break;
                case "lbAlpha_m": AppendText("m"); break;
                case "lbAlpha_Space": AppendText(" "); break;
                case "lbAlpha_c": AppendText("c"); break;
                case "lbAlpha_Shift": { m_bShift = !m_bShift; SetKeyPadImage(m_bShift); } break;
                case "lbAlpha_Hyphen": AppendText("-"); break;
                case "lbAlpha_z": AppendText("z"); break;
                case "lbAlpha_y": AppendText("y"); break;
                case "lbAlpha_x": AppendText("x"); break;
                case "lbAlpha_w": AppendText("w"); break;
                case "lbAlpha_v": AppendText("v"); break;
                case "lbAlpha_q": AppendText("q"); break;
                case "lbAlpha_u": AppendText("u"); break;
                case "lbAlpha_f": AppendText("f"); break;
                case "lbAlpha_t": AppendText("t"); break;
                case "lbAlpha_g": AppendText("g"); break;
                case "lbAlpha_s": AppendText("s"); break;
                case "lbAlpha_e": AppendText("e"); break;
                case "lbAlpha_r": AppendText("r"); break;
                case "lbAlpha_i": AppendText("i"); break;
                case "lbAlpha_k": AppendText("k"); break;
                case "lbAlpha_p": AppendText("p"); break;
                case "lbAlpha_j": AppendText("j"); break;
                case "lbAlpha_o": AppendText("o"); break;
                case "lbAlpha_a": AppendText("a"); break;
            }
            #endregion MouseClick
        }
        //public static Form SetItem() {  }
        public static void ShowCalculator(TextBox txt)
        {
            //Int32 currentMonitorCount = Screen.AllScreens.Length;
            Rectangle rc = txt.RectangleToScreen(new Rectangle(new Point(0, 0), txt.Size));
            CKeyPad_t m_frmKeyPad = new CKeyPad_t();//rc.Right, rc.Bottom);
            //m_frmKeyPad.Location = new Point(rc.Right, rc.Bottom);//txt.Left + txt.Width + 1, txt.Top + txt.Height);
            m_frmKeyPad.SetMode(true);
            int nLeft = rc.Right;
            int nTop = rc.Top;
            int nMonitor = 0;
            if (Screen.AllScreens.Length > 1)
            {
                if (
                    (nTop < 0) ||
                    (nTop >= Screen.AllScreens[nMonitor].Bounds.Size.Height) ||
                    (nLeft < 0) ||
                    (nLeft >= Screen.AllScreens[nMonitor].Bounds.Size.Width)
                    )
                {
                    nMonitor = 1;
                }
            }
            if ((rc.Top + m_frmKeyPad.Height) > 1024) // 일단 모니터 사이즈를 1024로 가정하고 넣어둔다. - 나중에 수정
            {
                nTop = rc.Top - ((rc.Top + m_frmKeyPad.Height) - Screen.AllScreens[nMonitor].Bounds.Size.Height);
            }
            if ((rc.Right + m_frmKeyPad.Width) > Screen.AllScreens[nMonitor].Bounds.Size.Width) // 일단 모니터 사이즈를 1280으로 가정하고 넣어둔다. - 나중에 수정
            {
                nLeft = rc.Left - m_frmKeyPad.Width;
            }
            m_frmKeyPad.SetLocation(nLeft, nTop);
            m_frmKeyPad.SetTextBox(txt);
            m_frmKeyPad.ShowDialog();
        }
        public static void ShowKeyboard(TextBox txt)
        {
            Rectangle rc = txt.RectangleToScreen(new Rectangle(new Point(0, 0), txt.Size));
            CKeyPad_t m_frmKeyPad = new CKeyPad_t();//rc.Right, rc.Bottom);
            //m_frmKeyPad.Location = new Point(rc.Right, rc.Bottom);//txt.Left + txt.Width + 1, txt.Top + txt.Height);
            m_frmKeyPad.SetMode(false);
            int nLeft = rc.Right;
            int nTop = rc.Top;
            int nMonitor = 0;
            if (Screen.AllScreens.Length > 1)
            {
                if (
                    (nTop < 0) ||
                    (nTop >= Screen.AllScreens[nMonitor].Bounds.Size.Height) ||
                    (nLeft < 0) ||
                    (nLeft >= Screen.AllScreens[nMonitor].Bounds.Size.Width)
                    )
                {
                    nMonitor = 1;
                }
            }
            if ((rc.Top + m_frmKeyPad.Height) > 1024)
            {
                nTop = rc.Top - ((rc.Top + m_frmKeyPad.Height) - Screen.AllScreens[nMonitor].Bounds.Size.Height);
            }
            if ((rc.Right + m_frmKeyPad.Width) > Screen.AllScreens[nMonitor].Bounds.Size.Width)
            {
                nLeft = rc.Left - m_frmKeyPad.Width;
            }
            m_frmKeyPad.SetLocation(nLeft, nTop);
            m_frmKeyPad.SetTextBox(txt);
            m_frmKeyPad.ShowDialog();
        }

        private void pnNum_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
