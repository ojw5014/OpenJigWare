using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace OpenJigWare.Docking
{
    public partial class frmDesigner : Form
    {
        public static CDhParam m_COjwDhParam = new CDhParam(); // for DH Checking
        public static CDhParamAll m_COjwDhParamAll = new CDhParamAll(); // for DH Checking
        public static Ojw.C3d m_C3d = new Ojw.C3d();
        public frmDesigner()
        {
            InitializeComponent();
        }

        #region Form variable
        public static Form m_frm3D = new Form();
        public static frmDrawText m_frmDrawText = new frmDrawText();
        public static frmKinematics m_frmKinematics = new frmKinematics();
        public static frmGridEditor m_frmGridEditor = new frmGridEditor();
        #endregion Form variable

        private float m_fDefault_W = 0;
        private float m_fDefault_H = 0;
        private float m_fTab_W = 0;
        private float m_fTab_H = 0;
        private int m_nInit = 0;
        private int m_nMonitor = 0;
        public void SetMonitor(int nNum)
        {
            m_nMonitor = nNum;
        }
        private void frmDesigner_Load(object sender, EventArgs e)
        {
            //m_bProgEnd = false;
            m_fDefault_W = (float)this.Width;
            m_fDefault_H = (float)this.Height;
            m_fTab_W = (float)this.tcJson.Width;
            m_fTab_H = (float)this.tcJson.Height;

            //m_frmDrawText.Show();

            Init3D();

            m_frmGridEditor = new frmGridEditor();
            m_frmGridEditor.Show();
            //int nMonitor = (Screen.AllScreens.Length >= m_nMonitor) ? Screen.AllScreens.Length - 1 : m_nMonitor;
            //if (nMonitor < 0) nMonitor = 0;
            //this.Left = Screen.AllScreens[nMonitor].Bounds.Width - this.Width;
            //this.Top = Screen.AllScreens[nMonitor].Bounds.Bottom - this.Height;

            tmrDisp.Enabled = true;
            m_nInit = 1;
        }

        #region 3D Buttons
        private Button m_btn3d_Open = new Button();
        private Panel m_pn3d = new Panel();
        private RadioButton m_rd3d_Display = new RadioButton();
        private RadioButton m_rd3d_Control = new RadioButton();
        private void m_btn3d_Open_Init(Form frm)
        {
            m_btn3d_Open.Left = 0;
            m_btn3d_Open.Top = 0;
            m_btn3d_Open.Width = 50;
            m_btn3d_Open.Height = 26;
            m_btn3d_Open.Text = "Open";
            m_btn3d_Open.Click += new EventHandler(m_btn3d_Open_Click);
            frm.Controls.Add(m_btn3d_Open);

            m_pn3d.Left = m_btn3d_Open.Right + 30;
            m_pn3d.Top = 0;
            m_pn3d.Width = 200;
            m_pn3d.Height = m_btn3d_Open.Height;
            frm.Controls.Add(m_pn3d);

            m_rd3d_Display.Checked = true;
            //m_rd3d_Display.BackColor = Color.Transparent;
            m_rd3d_Display.Left = 10;
            m_rd3d_Display.Top = m_btn3d_Open.Top;
            m_rd3d_Display.Width = 80;
            m_rd3d_Display.Height = 26;
            m_rd3d_Display.Text = "Display";
            m_rd3d_Display.Click += new EventHandler(m_rd3d_Display_Click);
            m_pn3d.Controls.Add(m_rd3d_Display);

            //m_rd3d_Display.BackColor = Color.Transparent;
            m_rd3d_Control.Left = m_rd3d_Display.Right + 10;
            m_rd3d_Control.Top = m_btn3d_Open.Top;
            m_rd3d_Control.Width = m_rd3d_Display.Width;
            m_rd3d_Control.Height = m_rd3d_Display.Height;
            m_rd3d_Control.Text = "Control";
            m_rd3d_Control.Click += new EventHandler(m_rd3d_Control_Click);
            m_pn3d.Controls.Add(m_rd3d_Control);
        }
        void m_btn3d_Open_Click(object sender, EventArgs e)
        {
            m_C3d.FileOpen();
            //throw new NotImplementedException();
        }
        void m_rd3d_Display_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(0);
            m_C3d.Prop_Update_VirtualObject();
        }
        void m_rd3d_Control_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(1);
            m_C3d.Prop_Update_VirtualObject();
        }
        #endregion 3D Buttons

        private void Init3D()
        {
            m_C3d = new Ojw.C3d();
            // 
            m_frm3D = new Form();
            //m_frm3D.Left = 0;
            //m_frm3D.Top = 0;

            
            m_frm3D.Width = 600;
            m_frm3D.Height = 600;
            //m_frm3D.FormClosing += new FormClosingEventHandler(m_frm3D_FormClosing);
            m_frm3D.FormClosing += new FormClosingEventHandler(m_frm3D_FormClosing);

            m_btn3d_Open_Init(m_frm3D);
            //m_frm3D.Controls.Add(m_btn3d_Open);
            m_frm3D.Show();
            //m_frm3D.TopMost = true;

#if false
            int nMonitor = (Screen.AllScreens.Length >= m_nMonitor) ? Screen.AllScreens.Length - 1 : m_nMonitor;
            if (nMonitor < 0) nMonitor = 0;
#else
            int nMonitor = 0;
#endif
            m_frm3D.Left = Screen.AllScreens[nMonitor].Bounds.Left;
            m_frm3D.Top = Screen.AllScreens[nMonitor].Bounds.Top;

            //m_



            this.Left = m_frm3D.Left + m_frm3D.Width;
            this.Top = m_frm3D.Top;// + m_frm3D.Height;


            this.Left = Screen.AllScreens[nMonitor].Bounds.Left + Screen.AllScreens[nMonitor].Bounds.Width - this.Width;
            this.Top = Screen.AllScreens[nMonitor].Bounds.Top + Screen.AllScreens[nMonitor].Bounds.Bottom - this.Height;

            Ojw.CMessage.Init(txtMessage);
            MakeBox(pnMotors, 256);

            m_C3d.Init(m_frm3D);//pnDisp);
            m_C3d.CreateProb_VirtualObject(pnProp);
            m_C3d.CreateProp_Selected(pnProp_Selected, null);

            m_C3d.SetAseFile_Path("ase");

            // 기준축 보이기
            m_C3d.SetStandardAxis(true);
            // 빛 사용
            m_C3d.Enable_Light(true);

            // 클릭한 부분 색 / 투명도 지정
            //m_C3d.SetAlpha_Display_Enalbe(true);
            m_C3d.SetPick_ColorMode(true);
            m_C3d.SetPick_ColorValue(Color.Green); // 클릭된 부분을 녹색으로 설정
            m_C3d.SetPick_AlphaMode(true); // 투명 모드 활성화
            m_C3d.SetPick_AlphaValue(0.5f); // 클릭된 부분을 반투명으로 한다.


            m_C3d.SetVirtualClass_Enable(true);

            // 모터클릭시 컨트롤 모드(true), 화면이동모드(false)로...
            SetMouse(false);

            #region PropertyGrid

            //m_C3d.InitTools_Motor(pnMotors);
            m_C3d.SetTextboxes_ForAngle(m_atxtAngle);

            // Add User Function
            m_C3d.AddMouseEvent_Down(OjwMouseDown);
            m_C3d.AddMouseEvent_Move(OjwMouseMove);
            m_C3d.AddMouseEvent_Up(OjwMouseUp);


            m_C3d.Event_FileOpen.UserEvent += new EventHandler(FileOpen);

            m_C3d.SelectMotor_Sync_With_Mouse(true);

#if false
            m_C3d.InitTools_Kinematics(m_pnKinematics);
            //m_C3d.InitTools_Background(pnBackground);
            // 
            m_C3d.Prop_Set_Main_ShowStandardAxis(true);
            m_C3d.Prop_Set_Main_ShowVirtualAxis(true);
            m_C3d.Prop_Update_VirtualObject();

#endif
            #endregion PropertyGrid
        }
        private void FileOpen(object sender, EventArgs e)
        {
            // FileOpen 후에 생기는 이벤트 처리
            tstxtAlpha.Text = "1.0";
            float fData = Ojw.CConvert.StrToFloat(tstxtAlpha.Text);
            m_C3d.Prop_Set_Main_Alpha(fData);
            m_C3d.Prop_Update_VirtualObject();
        }
        bool m_bMouseDown = false;
        void OjwMouseDown(object sender, MouseEventArgs e)
        {
            m_bMouseDown = true;
            //throw new NotImplementedException();
        }
        void OjwMouseMove(object sender, MouseEventArgs e)
        {
            if ((m_bMouseDown == true) && (m_nIndex >= 0))
            {
                
            }
            //throw new NotImplementedException();
        }
        void OjwMouseUp(object sender, MouseEventArgs e)
        {
            if (m_bMouseDown == true)
            {
                m_bMouseDown = false;

                for (int i = 0; i < 255; i++)
                {
                    m_atxtAngle[i].Text = Ojw.CConvert.FloatToStr(m_C3d.GetData(i), 3);
                }
            }
            float fX, fY, fZ;
            float fPan, fTilt, fSwing;
            m_C3d.GetPos_Display(out fX, out fY, out fZ);
            m_C3d.GetAngle_Display(out fPan, out fTilt, out fSwing);
            txtBack_X.Text = Ojw.CConvert.FloatToStr(fX, 1);
            txtBack_Y.Text = Ojw.CConvert.FloatToStr(fY, 1);
            txtBack_Z.Text = Ojw.CConvert.FloatToStr(fZ, 1);

            txtBack_Pan.Text = Ojw.CConvert.FloatToStr(fPan, 1);
            txtBack_Tilt.Text = Ojw.CConvert.FloatToStr(fTilt, 1);
            txtBack_Swing.Text = Ojw.CConvert.FloatToStr(fSwing, 1);
            //throw new NotImplementedException();
        }

        

        private void DInit()
        {
            if (m_bProgEnd == false)
            {
                m_bProgEnd = true;
                m_frmDrawText.Close();
                m_frmKinematics.Close();
                m_frmGridEditor.Close();
                m_frm3D.Close();
                DInit3D();
                this.Close();
            }
        }
        private void m_frm3D_FormClosing(object sender, FormClosingEventArgs e)
        {
            DInit();
        }
        private void DInit3D()
        {
            m_C3d.Event_FileOpen.UserEvent -= new EventHandler(FileOpen);
            m_C3d.DInit();
        }

        private RichTextBox m_rtxtDraw;
        private Label[] m_albAngle;
        public static TextBox[] m_atxtAngle;
        private void MakeBox(Panel pnMotors, int nCnt)
        {
            if (nCnt > 0)
            {

                TabControl tabAngle = new TabControl();
                tabAngle = new TabControl();
                tabAngle.Left = 0;
                tabAngle.Top = 0;
                tabAngle.Size = new Size(pnMotors.Width, pnMotors.Height);
                pnMotors.Controls.Add(tabAngle);

                //tabAngle.Size = new Size(m_pnDrawModel.Width, 173);// new Size(640, 173);
                int nColCount = 6;
                int nRowCount = 5;
                int nMax = nColCount * nRowCount;

                m_albAngle = new Label[nCnt];
                m_atxtAngle = new TextBox[nCnt];

                //int nGapRight = 0;// -42;// 5;
                int nWidth = 60;// 35;
                int nHeight = 18;

                int nLableWidth = 26;
                int nLableOffset = nLableWidth;// 29;
                int nGapLeft = nLableWidth;// 40;// 80;// 12;

                int nWidth_Offset = nLableWidth + 2;// 10;// (this.Width - (nGapLeft + nGapRight) - (nWidth * nCnt)) / nCnt;
                int nHeight_Offset = 2;// 5;

                TabPage tp = null;//new TabPage();


                Color cBackColor = Color.White;
#if false
                #region For RichTextBox
                m_rtxtDraw = new RichTextBox();
                tp = new TabPage();
                tp.Name = "tabPgText";
                tp.BackColor = cBackColor;
                tp.Text = "Draw";
                tabAngle.Controls.Add(tp);
                m_rtxtDraw.Left = 10;
                m_rtxtDraw.Top = 10;
                m_rtxtDraw.Width = tabAngle.Width - m_rtxtDraw.Left * 2 - 10;
                m_rtxtDraw.Height = tabAngle.Height - m_rtxtDraw.Top * 2 - 30;
                //m_rtxtDraw.Text = "test";
                tp.Controls.Add(m_rtxtDraw);
                #endregion For RichTextBox
#else
                // Draw Text 위치 정하기
                //m_rtxtDraw = m_frmDrawText.rtxtDraw;
                //m_C3d.SetDrawText_ForDisplay(m_rtxtDraw);
                ShowDrawText();
                ShowKinematics();
#endif

                //int nPage_Back = -1;
                int nPage = 0;
                for (int i = 0; i < nCnt; i++)
                {
                    nPage = i / nMax;
                    try
                    {
                        String strName = "tabPg" + Ojw.CConvert.IntToStr(nPage);
                        if (tabAngle.Controls.Find("tabPg" + Ojw.CConvert.IntToStr(nPage), true).Length > 0)
                        {
                            if ((tp == null) || (tp.Name != strName))
                            {
                                tp = (TabPage)(tabAngle.Controls.Find("tabPg" + Ojw.CConvert.IntToStr(nPage), true)[0]);
                                tp.BackColor = cBackColor;
                                tp.Text = Ojw.CConvert.IntToStr(nMax * nPage) +
                                    " ~ " +
                                    Ojw.CConvert.IntToStr(((nMax * (nPage + 1) - 1) < nCnt) ? (nMax * (nPage + 1) - 1) : nCnt - 1);
                            }
                        }
                        else
                        {
                            if ((tp == null) || (tp.Name != strName))
                            {
                                tp = new TabPage();
                                tp.Name = strName;
                                tp.BackColor = cBackColor;
                                tp.Text = Ojw.CConvert.IntToStr(nMax * nPage) +
                                " ~ " +
                                Ojw.CConvert.IntToStr(((nMax * (nPage + 1) - 1) < nCnt) ? (nMax * (nPage + 1) - 1) : nCnt - 1);
                                tabAngle.Controls.Add(tp);
                            }
                        }
                    }
                    catch
                    {
                        //   if (tp == null)
                        //   {
                        tp = new TabPage("tabPg" + Ojw.CConvert.IntToStr(nPage));
                        tp.Text = Ojw.CConvert.IntToStr(nMax * nPage) +
                                " ~ " +
                                Ojw.CConvert.IntToStr(((nMax * (nPage + 1) - 1) < nCnt) ? (nMax * (nPage + 1) - 1) : nCnt - 1);
                        tabAngle.Controls.Add(tp);
                        //   }
                    }

                    if (tp == null) break;

                    int nPos = i % nColCount;
                    int nLine = (i % (nColCount * nRowCount)) / nColCount;
                    m_atxtAngle[i] = new TextBox();
                    m_atxtAngle[i].Top = (nHeight_Offset) * (nLine + 1) + (nHeight + nHeight_Offset) * nLine;
                    m_atxtAngle[i].Left = nGapLeft + nWidth * nPos + nWidth_Offset * nPos;
                    m_atxtAngle[i].Width = nWidth;
                    m_atxtAngle[i].Height = nHeight;
                    m_atxtAngle[i].Name = "txtAngle" + Ojw.CConvert.IntToStr(nPos);
                    m_atxtAngle[i].Text = "0.0";
                    m_atxtAngle[i].Visible = true;
                    m_atxtAngle[i].TextAlign = HorizontalAlignment.Center;
                    m_atxtAngle[i].TextChanged += new System.EventHandler(m_atxtAngle_TextChanged);
                    m_atxtAngle[i].DoubleClick += new EventHandler(frmDesigner_DoubleClick);
                    tp.Controls.Add(m_atxtAngle[i]);

                    m_albAngle[i] = new Label();
                    m_albAngle[i].Top = 5 + m_atxtAngle[i].Top;
                    m_albAngle[i].Height = nHeight;
                    m_albAngle[i].Width = nLableWidth;
                    m_albAngle[i].Name = "lbAngle" + Ojw.CConvert.IntToStr(nPos);
                    m_albAngle[i].Text = Ojw.CConvert.IntToStr(i, 3);
                    m_albAngle[i].Left = m_atxtAngle[i].Left - nLableOffset;// m_albPos[i].Width - 1;
                    tp.Controls.Add(m_albAngle[i]);
                    //tp = null;
                }
            }
        }

        void frmDesigner_DoubleClick(object sender, EventArgs e)
        {
            if (chkEnMotor.Checked == true)
            {
                if (m_bAngle_NoUpdate == true) return;
                int nPort = Ojw.CConvert.StrToInt(txtComPort.Text);
                int nBaudRate = Ojw.CConvert.StrToInt(txtBaud.Text);

                int nCnt = m_atxtAngle.Length;
                for (int i = 0; i < nCnt; i++)
                {
                    if (m_atxtAngle[i].Focused == true)
                    {
                        m_C3d.SetData(i, Ojw.CConvert.StrToFloat(m_atxtAngle[i].Text));
                        //MoveToDhPosition(m_fBallSize, 1.0f, Color.Red, m_afAngle);
                        if (m_C3d.m_CMonster.Open(nPort, nBaudRate) == true)
                        {
                            m_C3d.m_CMonster.SetTorq(i, false);     
                            //m_C3d.m_CMonster.SetOperation(Ojw.CMonster.EOperation_t._Position);
                            m_C3d.m_CMonster.SetTorq(i, true);
                            m_C3d.m_CMonster.Set(i, m_C3d.GetData(i));
                            m_C3d.m_CMonster.Send_Motor(1000);
                            m_C3d.m_CMonster.Close();
                        }
                        break;
                    }
                }
            }
            //throw new NotImplementedException();
        }
        private bool m_bAngle_NoUpdate = false;
        private void BlockUpdate(bool bBlock) { m_bAngle_NoUpdate = bBlock; }
        private void m_atxtAngle_TextChanged(object sender, EventArgs e)
        {
            if (m_bAngle_NoUpdate == true) return;
            int nCnt = m_atxtAngle.Length;
            for (int i = 0; i < nCnt; i++)
            {
                if (m_atxtAngle[i].Focused == true)
                {
                    m_C3d.SetData(i, Ojw.CConvert.StrToFloat(m_atxtAngle[i].Text));
                    //MoveToDhPosition(m_fBallSize, 1.0f, Color.Red, m_afAngle);
                    break;
                }
            }
        }
        private int m_nIndex = -1;
        private void tmrDisp_Tick(object sender, EventArgs e)
        {
            if (m_C3d != null)
            {
                m_C3d.OjwDraw();
                
                int nGroupA, nGroupB, nGroupC, nInverseKinematicsNumber;
                if (m_C3d.GetPickingData(out nGroupA, out nGroupB, out nGroupC, out nInverseKinematicsNumber) == true)
                {
                    if (nGroupA > 0)
                    {
                        //Ojw.printf(nGroupB.ToString());
                        m_nIndex = nGroupB;
                    }
                }
            }
        }

        private bool m_bProgEnd = false;
        private void frmDesigner_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void frmDesigner_Resize(object sender, EventArgs e)
        {
            if (m_nInit != 0)
            {
                float fW = (float)this.Width;
                float fH = (float)this.Height;
                float fRatio_W = 1.0f + (fW - m_fDefault_W) / m_fDefault_W;
                float fRatio_H = 1.0f + (fH - m_fDefault_H) / m_fDefault_H;
                //m_fPrev_W = fW;
                //m_fPrev_H = fH;
                if (fRatio_W < 1.0f) fRatio_W = 1.0f;

                float fRatio = (fRatio_W > fRatio_H) ? fRatio_W : fRatio_H;
                tcJson.Width = (int)Math.Round(m_fTab_W * fRatio_W);
                tcJson.Height = (int)Math.Round(m_fTab_H * fRatio_H);
                //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
                //this.Scale(new SizeF(fRatio_W, fRatio_H));

                pnProp.Width = tpProp.Width / 2 - 1;
                //pnProp_Selected.Width = tpProp.Width / 2 - 1;
            }
        }

        private void tpProp_Click(object sender, EventArgs e)
        {
            
        }

        private void mnOpen_Click(object sender, EventArgs e)
        {
            m_C3d.FileOpen();
            m_frmKinematics.DhRefresh();
        }

        private void mnClose_Click(object sender, EventArgs e)
        {
            m_C3d.FileClose();
        }

        private void mnSave_Click(object sender, EventArgs e)
        {
            DialogResult dlgRet = MessageBox.Show("Overwrite?", "File Save", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (dlgRet == System.Windows.Forms.DialogResult.OK)
            {
                m_C3d.FileSave(m_C3d.GetFileName(), m_C3d.GetHeader());
            }
            else 
            {
                #region File Save
                SaveFileDialog saveDlg = new SaveFileDialog();
                saveDlg.FileName = "*." + Ojw.C3d._STR_EXT;
                saveDlg.Filter = "Design file(*." + Ojw.C3d._STR_EXT + ")|*." + Ojw.C3d._STR_EXT;

                saveDlg.DefaultExt = Ojw.C3d._STR_EXT;
                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    String fileName = saveDlg.FileName;
                    if (fileName != null)
                    {
                        if (fileName.Length > 0)
                        {
                            //m_strWorkDirectory = Directory.GetCurrentDirectory();
                            //txtFileName.Text = fileName;

                            m_C3d.FileSave(fileName, m_C3d.GetHeader());
                            Ojw.CMessage.Write(fileName + "(" + m_C3d.GetHeader().strVersion + ") file saved");
                        }
                    }
                }
                #endregion File Save
            }
        }

        private void mnView_DrawText_Click(object sender, EventArgs e)
        {
            ShowDrawText();
        }
        private void ShowDrawText()
        {
            //if (m_frmDrawText == null) m_frmDrawText = new frmDrawText();
            if (m_frmDrawText.IsAccessible == false) m_frmDrawText = new frmDrawText();
            m_frmDrawText.Show();
            m_frmDrawText.Left = m_frm3D.Left;
            m_frmDrawText.Top = m_frm3D.Top + m_frm3D.Height;

            m_rtxtDraw = m_frmDrawText.rtxtDraw;
            m_C3d.SetDrawText_ForDisplay(m_rtxtDraw);
            m_rtxtDraw.Text = m_C3d.m_CHeader.strDrawModel;
        }
        private void ShowKinematics()
        {
            if (m_frmKinematics.IsAccessible == false)
            {
                m_frmKinematics = new frmKinematics();
            }
            m_frmKinematics.Show();
            m_frmKinematics.DhRefresh();

            //m_C3d.InitTools_Kinematics(m_frmKinematics.pnKinematics);
        }

        private void mnKinematics_Click(object sender, EventArgs e)
        {
            ShowKinematics();
        }

        private void frmDesigner_FormClosing(object sender, FormClosingEventArgs e)
        {
            DInit();
        }

        private void tsbtnModel0_Click(object sender, EventArgs e)
        {
            //if (
            m_C3d.Prop_Set_DispObject("#7");
            m_C3d.Prop_Update_VirtualObject();
        }

        private void mnOpenAndCopyStl_Click(object sender, EventArgs e)
        {
            m_C3d.FileExport();
            //if (m_C3d.FileOpen() == true)
            //{
            //    if (m_C3d.GetModelingFiles_Count() > 0)
            //    {
            //        if (MessageBox.Show("Do you want me to copy all 3d-files for you?", "Files Copy(modeling files)", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            //        {
            //            FolderBrowserDialog dlg = new FolderBrowserDialog();
            //            //dlg.SelectedPath = 

            //            if (dlg.ShowDialog() != DialogResult.OK) return;

            //            string strPath = dlg.SelectedPath;
            //            DirectoryInfo dirInfo = new DirectoryInfo(strPath);
            //            List<string> lststrMissing = new List<string>();
            //            lststrMissing.Clear();
            //            if (dirInfo.Exists == true)
            //            {
            //                string[] pstrMissing = Ojw.CFile.FileCopy(String.Format("{0}{1}", Application.StartupPath, m_C3d.GetAseFile_Path()), dirInfo.FullName, m_C3d.GetModelingFiles().ToArray());
            //                if (pstrMissing.Length > 0)
            //                {
            //                    string strError = String.Empty;
            //                    foreach (string strItem in pstrMissing) strError += strItem + ", ";
            //                    Ojw.CMessage.Write_Error("FileCopy Error => {0}", strError);
            //                }
            //                else Ojw.CMessage.Write("Done : (File Copy)");
            //                //foreach (string strItem in m_C3d.GetModelingFiles())
            //                //{
            //                //    string strFile = String.Format("{0}\\{1}{2}", m_C3d.GetAseFile_Path(), strItem, (strItem.IndexOf('.') < 0) ? ".ase" : "");
            //                //    //Ojw.CMessage.Write2("{0}\r\n", strItem);
                                
            //                //}
            //            }
            //        }
            //    }
            //}

        }

        private void btnInit0_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 255; i++)
            {
                m_atxtAngle[i].Text = "0";
                m_C3d.SetData(i, 0.0f);
            }
        }

        private void btnInit1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 255; i++)
            {
                float fData = m_C3d.GetHeader().pSMotorInfo[i].fInitAngle;
                m_atxtAngle[i].Text = Ojw.CConvert.FloatToStr(fData);
                m_C3d.SetData(i, fData);
            }
        }

        private void btnInit2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 255; i++)
            {
                float fData = m_C3d.GetHeader().pSMotorInfo[i].fInitAngle2;
                m_atxtAngle[i].Text = Ojw.CConvert.FloatToStr(fData);
                m_C3d.SetData(i, fData);
            }
        }

        private void tsbtnModel1_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_DispObject("#8");
            m_C3d.Prop_Update_VirtualObject();
        }

        private void tsbtnModel2_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_DispObject("#9");
            m_C3d.Prop_Update_VirtualObject();
        }

        private void tsbtnModel3_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_DispObject("#3");
            m_C3d.Prop_Update_VirtualObject();
        }

        private void tsbtnModel4_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_DispObject("#4");
            m_C3d.Prop_Update_VirtualObject();
        }

        private void tsbtnModel5_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_DispObject("#5");
            m_C3d.Prop_Update_VirtualObject();
        }

        private void tsbtnModel6_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_DispObject("#6");
            m_C3d.Prop_Update_VirtualObject();
        }

        private void tsbtnModel10_Click(object sender, EventArgs e)
        {

            m_C3d.Prop_Set_DispObject("#10");
            m_C3d.Prop_Update_VirtualObject();
        }

        private void tsbtnModel11_Click(object sender, EventArgs e)
        {

            m_C3d.Prop_Set_DispObject("#11");
            m_C3d.Prop_Update_VirtualObject();
        }

        private void tsbtnModel12_Click(object sender, EventArgs e)
        {

            m_C3d.Prop_Set_DispObject("#12");
            m_C3d.Prop_Update_VirtualObject();
        }

        private void tsbtnModel13_Click(object sender, EventArgs e)
        {

            m_C3d.Prop_Set_DispObject("#13");
            m_C3d.Prop_Update_VirtualObject();
        }

        private void tsbtnModel14_Click(object sender, EventArgs e)
        {

            m_C3d.Prop_Set_DispObject("#14");
            m_C3d.Prop_Update_VirtualObject();
        }
        private void SetMouse(bool bControl)
        {            
            tsbtnMouseMode.Checked = bControl;
            m_C3d.Prop_Set_Main_MouseControlMode((bControl == true) ? 1 : 0);
            m_C3d.Prop_Update_VirtualObject();
        }
        private void tsbtnMouseMode_Click(object sender, EventArgs e)
        {
            if (tsbtnMouseMode.Checked == true)
                SetMouse(false);
            else 
                SetMouse(true);
        }

        private void tstxtAlpha_TextChanged(object sender, EventArgs e)
        {
            float fData = Ojw.CConvert.StrToFloat(tstxtAlpha.Text);
            m_C3d.Prop_Set_Main_Alpha(fData);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void tsTop_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void txtBack_X_TextChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }

        private void txtBack_Y_TextChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }

        private void txtBack_Z_TextChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }

        private void txtBack_Pan_TextChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }

        private void txtBack_Tilt_TextChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }

        private void txtBack_Swing_TextChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            float fX, fY, fZ;
            float fPan, fTilt, fSwing;
            m_C3d.GetPos_Display(out fX, out fY, out fZ);
            m_C3d.GetAngle_Display(out fPan, out fTilt, out fSwing);

            fX = Ojw.CConvert.StrToFloat(txtBack_X.Text);
            fY = Ojw.CConvert.StrToFloat(txtBack_Y.Text);
            fZ = Ojw.CConvert.StrToFloat(txtBack_Z.Text);
            fPan = Ojw.CConvert.StrToFloat(txtBack_Pan.Text);
            fTilt = Ojw.CConvert.StrToFloat(txtBack_Tilt.Text);
            fSwing = Ojw.CConvert.StrToFloat(txtBack_Swing.Text);

            m_C3d.SetPos_Display(fX, fY, fZ);
            m_C3d.SetAngle_Display(fPan, fTilt, fSwing);
        }
        private void btnPos_Front_Click(object sender, EventArgs e)
        {
            txtBack_X.Text = "0";
            txtBack_Y.Text = "0";
            txtBack_Z.Text = "0";
            txtBack_Pan.Text = "0";
            txtBack_Tilt.Text = "0";
            txtBack_Swing.Text = "0";

            RefreshDisplay();
        }

        private void btnPos_Right_Click(object sender, EventArgs e)
        {
            txtBack_X.Text = "0";
            txtBack_Y.Text = "0";
            txtBack_Z.Text = "0";
            txtBack_Pan.Text = "90";
            txtBack_Tilt.Text = "0";
            txtBack_Swing.Text = "0";
            RefreshDisplay();
        }

        private void btnPos_Bottom_Click(object sender, EventArgs e)
        {
            txtBack_X.Text = "0";
            txtBack_Y.Text = "0";
            txtBack_Z.Text = "0";
            txtBack_Pan.Text = "0";
            txtBack_Tilt.Text = "-90";
            txtBack_Swing.Text = "0";
            RefreshDisplay();
        }

        private void btnPos_Left_Click(object sender, EventArgs e)
        {
            txtBack_X.Text = "0";
            txtBack_Y.Text = "0";
            txtBack_Z.Text = "0";
            txtBack_Pan.Text = "-90";
            txtBack_Tilt.Text = "0";
            txtBack_Swing.Text = "0";
            RefreshDisplay();
        }

        private void btnPos_Top_Click(object sender, EventArgs e)
        {
            txtBack_X.Text = "0";
            txtBack_Y.Text = "0";
            txtBack_Z.Text = "0";
            txtBack_Pan.Text = "0";
            txtBack_Tilt.Text = "90";
            txtBack_Swing.Text = "0";
            RefreshDisplay();
        }

        private void btnCheckComport_Click(object sender, EventArgs e)
        {
            Ojw.CSerial m_CSerial = new Ojw.CSerial();

            string [] pstrComport;
            Ojw.CRegistry.GetSerialPort(out pstrComport, true, true);
            if (pstrComport.Length > 0)
            {
                for (int i = 0; i < pstrComport.Length; i++)
                {
                    Ojw.printf("Comport[{0}/{1}] = {2}\r\n", i + 1, pstrComport.Length, pstrComport[i]);
                }
            }
            else
            {
                Ojw.printf("[warning] <No Found> Comport\r\n");
            }
        }

        private void mnMotionTool_Click(object sender, EventArgs e)
        {
            ShowMotionTool();
        }
        private void ShowMotionTool()
        {
            if (m_frmGridEditor.IsAccessible == false)
            {
                m_frmGridEditor = new frmGridEditor();
            }
            m_frmGridEditor.Show();
            //m_C3d.InitTools_Kinematics(m_frmKinematics.pnKinematics);
        }

        private void mn3D_Click(object sender, EventArgs e)
        {

        }
        private void Show3D()
        {
            if (m_frm3D.IsAccessible == false)
            {
                m_frm3D = new Form();
                Init3D();
            }
            m_frm3D.Show();
            //m_C3d.InitTools_Kinematics(m_frmKinematics.pnKinematics);
        }

        //private void frmDesigner_FormClosing(object sender, FormClosingEventArgs e)
        //{
            
        //}
#if false // test
        private float m_fDefault_W = 0;
        private float m_fDefault_H = 0;
        private float m_fPrev_W = 0;
        private float m_fPrev_H = 0;
        private bool m_bInit = false;
        private void frmDesigner_Load(object sender, EventArgs e)
        {
            m_fDefault_W = (float)this.Width;
            m_fDefault_H = (float)this.Height;
            m_fPrev_W = (float)this.Width;
            m_fPrev_H = (float)this.Height;
            m_bInit = true;
        }

        private void frmDesigner_Resize(object sender, EventArgs e)
        {
            //if (m_bInit == true)
            //{
            //    float fW = (float)this.Width;
            //    float fH = (float)this.Height;
            //    float fRatio_W = 1.0f + (fW - m_fPrev_W) / m_fPrev_W;
            //    float fRatio_H = 1.0f + (fH - m_fPrev_H) / m_fPrev_H;
            //    m_fPrev_W = fW;
            //    m_fPrev_H = fH;

            //    float fRatio = (fRatio_W > fRatio_H) ? fRatio_W : fRatio_H;
            //    this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            //    this.Scale(new SizeF(fRatio_W, fRatio_H));
            //}
        }


        private void btnScale_Click(object sender, EventArgs e)
        {
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            float fScale_W = Ojw.CConvert.StrToFloat(txtScale_W.Text);
            float fScale_H = Ojw.CConvert.StrToFloat(txtScale_H.Text);
            this.Scale(new SizeF(fScale_W, fScale_H));
        }
#endif

    }
}
