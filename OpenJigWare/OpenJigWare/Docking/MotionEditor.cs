//#define _ENABLE_MEDIAPLAYER
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenJigWare;
using System.Threading;
using System.IO;

//#if _ENABLE_MEDIAPLAYER
//
#if false
도구 -> 도구상자 항목선택 -> COM 구성요소 -> Windows Media Player   : wmp.dll
Tools-> Choose Item -> Com -> Windows media player    : wmp.dll
#endif

//#endif

namespace OpenJigWare.Docking
{
    public partial class frmMotionEditor : Form
    {
        public frmMotionEditor()
        {
            InitializeComponent();
//#if _ENABLE_MEDIAPLAYER

//#endif
        }
        
        //private Ojw.COjwMotor m_CMotor = new Ojw.COjwMotor();
        
        private Ojw.C3d m_C3d = new Ojw.C3d();
        private bool m_bLoadedForm = false;

        private Ojw.CFile m_CFile = new Ojw.CFile();
        private string m_strTitle = "Motion Editor";
        
        private bool m_bOneShotMp3Command = false;

        // 파일의 경로
        private String m_strWorkDirectory_Dmt = String.Empty;
        private String m_strWorkDirectory_Mp3 = String.Empty;

        private void frmMotionEditor_Load(object sender, EventArgs e)
        {
            m_strTitle = String.Format("{0} - Open Jig Ware Ver [{1}]", m_strTitle, SVersion_T.strVersion);
            lbTitle.Text = m_strTitle;

            Ojw.CTimer.Init(100);
            // 이것만 선언하면 기본 선언은 끝.
            m_C3d.Init(picDisp);
            // 캐드파일의 경로를 임의의 경로로 바꾼다. -> 안 정하면 실행파일과 같은 경로
            m_C3d.SetAseFile_Path("ase");

            Ojw.CMessage.Init(txtMessage);

            // property window
            m_C3d.CreateProb_VirtualObject(pnProperty);

            // Add User Function
            m_C3d.AddMouseEvent_Down(OjwMouseDown);
            m_C3d.AddMouseEvent_Move(OjwMouseMove);
            m_C3d.AddMouseEvent_Up(OjwMouseUp);
            m_C3d.Prop_Set_Main_MouseControlMode(0);
            m_C3d.GridMotionEditor_Init(dgAngle, 40, 999);
            m_C3d.GridMotionEditor_Init_Panel(pnButton);

            m_C3d.SelectMotor_Sync_With_Mouse(true);

            if (m_CFile.Load(100, Application.StartupPath + _STR_FILENAME) > 0)
            {
                int i = 0;
                bool bFile = false;
                if (m_CFile.GetData_String(i).IndexOf(".ojw") > 0)
                {
                    if (m_C3d.FileOpen(m_CFile.GetData_String(i)) == true) // 모델링 파일이 잘 로드 되었다면 
                    {
                        Ojw.CMessage.Write("File Opened");
                        bFile = true;

                        //cmbVersion.SelectedIndex = m_C3d.m_strVersion - 11;
                        float[] afData = new float[3];
                        m_C3d.GetPos_Display(out afData[0], out afData[1], out afData[2]);
                        //int i = 0;
                        //txtDisplay_X.Text = Ojw.CConvert.FloatToStr(afData[i++]);
                        //txtDisplay_Y.Text = Ojw.CConvert.FloatToStr(afData[i++]);
                        //txtDisplay_Z.Text = Ojw.CConvert.FloatToStr(afData[i++]);
                        m_C3d.GetAngle_Display(out afData[0], out afData[1], out afData[2]);
                        //i = 0;                                    

                        m_C3d.m_strDesignerFilePath = Ojw.CFile.GetPath(m_CFile.GetData_String(i));


                        // File Restore
                        m_C3d.FileRestore();
                    }
                }
                i++;

                txtPort.Text = m_CFile.GetData_String(i++);
                txtBaudrate.Text = m_CFile.GetData_String(i++);
                txtIp.Text = m_CFile.GetData_String(i++);
                txtSocket_Port.Text = m_CFile.GetData_String(i++);
                chkFreeze_X.Checked = m_CFile.GetData_Bool(i++);
                chkFreeze_Y.Checked = m_CFile.GetData_Bool(i++);
                chkFreeze_Z.Checked = m_CFile.GetData_Bool(i++);
                chkFreeze_Pan.Checked = m_CFile.GetData_Bool(i++);
                chkFreeze_Tilt.Checked = m_CFile.GetData_Bool(i++);
                chkFreeze_Swing.Checked = m_CFile.GetData_Bool(i++);
                txtSocket_Port.Text = m_CFile.GetData_String(i++);
                txtMotionCounter.Text = m_CFile.GetData_String(i++);
                txtChangeValue.Text = m_CFile.GetData_String(i++);
                txtPercent.Text = m_CFile.GetData_String(i++);
                txtBackAngle_X.Text = m_CFile.GetData_String(i++);
                txtBackAngle_Y.Text = m_CFile.GetData_String(i++);
                txtBackAngle_Z.Text = m_CFile.GetData_String(i++);
                chkTracking.Checked = m_CFile.GetData_Bool(i++);
                chkMp3.Checked = m_CFile.GetData_Bool(i++);
                txtMp3TimeDelay.Text = m_CFile.GetData_String(i++);
                chkFullSize.Checked = m_CFile.GetData_Bool(i++);
                chkDualMonitor.Checked = m_CFile.GetData_Bool(i++);
                txtID_FR.Text = m_CFile.GetData_String(i++);
                txtID_FL.Text = m_CFile.GetData_String(i++);
                txtID_RR.Text = m_CFile.GetData_String(i++);
                txtID_RL.Text = m_CFile.GetData_String(i++);

                txtID_0.Text = m_CFile.GetData_String(i++);
                txtID_1.Text = m_CFile.GetData_String(i++);
                txtID_2.Text = m_CFile.GetData_String(i++);

                //m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(m_CFile.GetData_String(i++));
                //m_strWorkDirectory_Mp3 = Ojw.CFile.GetPath(m_CFile.GetData_String(i++));
                m_strWorkDirectory_Dmt = m_CFile.GetData_String(i++);
                m_strWorkDirectory_Mp3 = m_CFile.GetData_String(i++);

                if (m_strWorkDirectory_Dmt == null) m_strWorkDirectory_Dmt = Application.StartupPath;
                if (m_strWorkDirectory_Mp3 == null) m_strWorkDirectory_Mp3 = Application.StartupPath;
                //////////////////////////////////////////////////
                if (txtID_FR.Text == txtID_FL.Text)
                {
                    txtID_FR.Text = "0";
                    txtID_FL.Text = "1";
                    txtID_RR.Text = "2";
                    txtID_RL.Text = "3";
                }
                if (txtID_0.Text == txtID_1.Text)
                {
                    txtID_0.Text = "0";
                    txtID_1.Text = "1";
                    txtID_2.Text = "2";
                }


                if (Ojw.CConvert.StrToInt(txtPort.Text) <= 0) txtPort.Text = "1";
                if (Ojw.CConvert.StrToInt(txtBaudrate.Text) <= 0) txtBaudrate.Text = "115200";
                int j = 0;
                bool bDigit = true;
                foreach (char cData in txtIp.Text) { if (cData == '.') j++; else if (Char.IsDigit(cData) == false) bDigit = false; }
                if ((j != 3) || (bDigit == false))
                {
                    txtIp.Text = "192.168.1.1";
                }
                if (Ojw.CConvert.StrToInt(txtPercent.Text) <= 0) txtPercent.Text = "70";
                if (bFile == false) { txtMotionCounter.Text = "1"; }
            }
            //if (m_C3d.FileOpen(@"16dof_ecoHead.ojw") == true) // 모델링 파일이 잘 로드 되었다면 
            //if (m_C3d.FileOpen(@"robolink-spider.ojw") == true) // 모델링 파일이 잘 로드 되었다면 
            //{
            
            //}

            // 그림을 그리기 위한 timer 가동
            tmrDraw.Enabled = true;

            // 설정된 마우스 이벤트를 사용할 것인지...(기본은 사용하도록 되어 있다. User의 Function만 사용하길 원한다면 false)
            //m_C3d.SetMouseEventEnable(true); // you can remove the default mouse events.

            

            //m_C3d.GridDraw_Init(dgAngle, 40);
            //InitGridView



            SaveParam();

            Ojw.CMessage.Write("Ready");
            //CheckForIllegalCrossThreadCalls = false;

            m_CTmr_Save.Set();

            SetToolTip();

            tmrMp3.Enabled = true;

            m_bLoadedForm = true;
        }

        private Ojw.CTimer m_CTmr_Save = new Ojw.CTimer();

        private bool m_bRequestPick = false;
        private void OjwMouseDown(object sender, MouseEventArgs e)
        {
            m_bRequestPick = true;
            
            //m_C3d.OjwMouseDown(sender, e);            
        }
        private void OjwMouseMove(object sender, MouseEventArgs e)
        {
            //m_C3d.OjwMouseMove(sender, e); 
        }
        private void OjwMouseUp(object sender, MouseEventArgs e)
        {
            //m_C3d.OjwMouseUp(sender, e); 
        }

        private bool m_bTimer = false;
        private void tmrDraw_Tick(object sender, EventArgs e)
        {
            if (m_bProgEnd == true) return;

            if (m_bTimer == true) return;
            m_bTimer = true;
            tmrDraw.Enabled = false;

            try
            {
                OjwDraw();
                if (m_bRequestPick == true)
                {
                    m_bRequestPick = false;

                    ShowData(m_bPick);
                }
            }
            catch (Exception ex)
            {
                FileInfo file = new FileInfo(Application.StartupPath + _STR_FILENAME);
                if (file.Exists) System.IO.File.Delete(Application.StartupPath + _STR_FILENAME);
                string strMsg = "Drawing Error - remove Param.Dat, please restart your program : " + ex.ToString();
                Ojw.CMessage.Write_Error(strMsg);
                MessageBox.Show(strMsg, "Program End", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }
#if true
            if ((m_CTmr_AutoBackup.Get() > 30000) && (m_C3d.GetFileName() != null) && (m_bStart == false)) // 5분에 한번 저장 -> 30초로 변경
            {
                int nVer = _V_10;//((chkFileVersionForSave.Checked == true) ? _V_11 : ((chkFileVersionForSave_1_0.Checked == true) ? _V_10 : _V_12));
                m_C3d.BinaryFileSave(nVer, Application.StartupPath + Ojw.C3d._STR_BACKUP_FILE, false, false);

                // 혹시 모르니 파라미터도 같이 저장하자.
                SaveParam();

                m_CTmr_AutoBackup.Set();
            }
#endif
            int nKey = m_C3d.KeyCommand_Get();
            if (nKey == (int)Keys.F5) { tmrRun.Enabled = true; }
            else if (nKey == (int)Keys.F4)
            {
                Cmd_InitPos(_INITPOS_DEFAULT, 2000);
            }
            else if (nKey == (int)Keys.Escape)
            {
                Stop();
            }

            tmrDraw.Enabled = true;
            m_bTimer = false;
        }

        private int m_nGroupA, m_nGroupB, m_nGroupC;
        private int m_nKinematicsNumber;
        private bool m_bPick;
        private bool m_bLimit;
        private void ShowData(bool bPick)
        {
            txtTest.Text = String.Empty;

            if (bPick == false)
            {
                Ojw.CMessage.Write2(txtTest, "There is no any parts for controlling");
                return;
            }
            // 클릭했으니 메세지를 한번 보여주자(show messages when it click)	
            Ojw.CMessage.Write2(txtTest, "Current Joint Group = " + Ojw.CConvert.IntToStr(m_nGroupA) + "\r\n");
            Ojw.CMessage.Write2(txtTest, "Current Motor Number = " + Ojw.CConvert.IntToStr(m_nGroupB) + "\r\n");
            Ojw.CMessage.Write2(txtTest, "Current Serve Group Number = " + Ojw.CConvert.IntToStr(m_nGroupC) + "\r\n");
            Ojw.CMessage.Write2(txtTest, "Connected Function number(but, 255 is None)=" + Ojw.CConvert.IntToStr(m_nKinematicsNumber) + "\r\n");
            Ojw.C3d.COjwDesignerHeader CHeader = m_C3d.GetHeader();

            // 수식부분이 선택된게 아니라면...(if there is no function number...)
            if ((m_nKinematicsNumber == 255) && (m_nGroupB < CHeader.nMotorCnt))
            {
                if (m_nGroupA > 0) // Is there a data?
                {
                    Ojw.CMessage.Write2(txtTest, Ojw.CConvert.IntToStr(m_nGroupB) + "번모터(Name : " + Ojw.CConvert.RemoveChar(CHeader.pSMotorInfo[m_nGroupB].strNickName, (char)0) + ")\r\n");
                    Ojw.CMessage.Write2(txtTest, "MotorID =" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nGroupB].nMotorID) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Direction =" + ((CHeader.pSMotorInfo[m_nGroupB].nMotorDir == 0) ? "Forward" : "Inverse"));
                    Ojw.CMessage.Write2(txtTest, "Limit(Max : but 0 -> there is no Limit)=" + ((CHeader.pSMotorInfo[m_nGroupB].fLimit_Up != 0.0f) ? Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nGroupB].fLimit_Up) + " 도" : "리미트 없음") + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Limit(Min : but 0 -> there is no Limit)=" + ((CHeader.pSMotorInfo[m_nGroupB].fLimit_Down != 0.0f) ? Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nGroupB].fLimit_Down) + " 도" : "리미트 없음") + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Center(EVD) : 0도에 해당하는 EVD 값 =" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nGroupB].nCenter_Evd) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Mech Mov=" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nGroupB].nMechMove) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Angle of Mech M =" + Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nGroupB].fMechAngle) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Initial Position =" + Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nGroupB].fInitAngle) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "NickName =" + Ojw.CConvert.RemoveChar(CHeader.pSMotorInfo[m_nGroupB].strNickName, (char)0) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Motor\'s Group Number =" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nGroupB].nGroupNumber) + "\r\n");
                    //Ojw.CMessage.Write2(txtTest, );
                    //Ojw.CMessage.Write2(txtTest, );

                    // Motor Check(relationship)
                    int nMotID = m_nGroupB;
                    if (CHeader.pSMotorInfo[nMotID].nAxis_Mirror == -1) Ojw.CMessage.Write2(txtTest, "이 모터는 Mirror 시 값의 변형을 주지 않는다.(No Changing when it has command [flip]");
                    else if (CHeader.pSMotorInfo[nMotID].nAxis_Mirror == -2) Ojw.CMessage.Write2(txtTest, "이 모터는 Mirror 시 Motor 의 Center Point 를 중심으로 뒤집도록 한다.(ex: -30 도 -> 30 도)");
                    else Ojw.CMessage.Write2(txtTest, "Current Motor number = " + Ojw.CConvert.IntToStr(nMotID) + ", Mirroring Motor number = " + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[nMotID].nAxis_Mirror));
                }
                else
                {
                    Ojw.CMessage.Write2(txtTest, "There is a part without controlling");
                }
            }
            else if (m_nKinematicsNumber != 255) // 수식 번호가 선택된 경우
            {
                float fX, fY, fZ;
                Ojw.CKinematics.CForward.CalcKinematics(CHeader.pDhParamAll[m_nKinematicsNumber], m_C3d.GetData(), out fX, out fY, out fZ);
                Ojw.CMessage.Write2(txtTest, "연동되는 수식의 번호(Connected Function Number) = " + Ojw.CConvert.IntToStr(m_nKinematicsNumber) + "\r\n");
                Ojw.CMessage.Write2(txtTest, "Current Position (x,y,z)=" + Ojw.CConvert.FloatToStr((float)Ojw.CMath.Round(fX, 3)) + "," + Ojw.CConvert.FloatToStr((float)Ojw.CMath.Round(fY, 3)) + "," + Ojw.CConvert.FloatToStr((float)Ojw.CMath.Round(fZ, 3)) + "\r\n");
            }
            else Ojw.CMessage.Write2(txtTest, "There is no any parts for controlling");
        }

        private bool m_bProgEnd = false;
        private bool ProgEnd()
        {
            if (m_bLoadedForm == false) return true;
            //tmrDraw.Enabled = false;
            //tmrMp3.Enabled = false;
            //tmrRun.Enabled = false;
            DialogResult dlgRet = MessageBox.Show("프로그램을 종료합니다.\r\n\r\n계속 하시겠습니까?", "Program 종료", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (dlgRet != DialogResult.OK)
            {
                tmrDraw.Enabled = true;
                tmrMp3.Enabled = true;
                tmrRun.Enabled = true;
            
                return false;
            }
            Stop();

            //tmrCheck.Enabled = false;
            //tmrMovingText.Enabled = false;
            tmrMp3.Enabled = false;

            // 환경파일 저장
            //DataFileSave_Config(_ENCODING_DEFAULT, Application.StartupPath + "\\Config.ini");

            m_bProgEnd = true;
            //m_bStop = true;

            //Ojw.CTimer.Wait(1000); // 백그라운드 작업이 다 완료될때까지 기다리는 편이 좋다.

            //DisConnect();

            //DestroyDesignHeader();

            // 정상적인 프로그램 종료시 백업한 파일 지우기
            FileInfo fileBack = new FileInfo(Application.StartupPath + Ojw.C3d._STR_BACKUP_FILE);
            if (fileBack.Exists) // 백업할 파일이 있는지 체크
            {
                // 없더라도 에러가 나지는 않는다. 굳이 에러처리가 필요 없음.
                fileBack.Delete();
            }

            // Save Param
            SaveParam();

            tmrMp3.Enabled = false;
            tmrDraw.Enabled = false;


            Ojw.CTimer.Wait(100); // 백그라운드 작업이 다 완료될때까지 기다리는 편이 좋다.


            // Serial
            if (m_C3d.m_CMotor.IsConnect() == true)
                Disconnect();

            return true;
        }
        private void SaveParam()
        {
            int i = 0;
            m_CFile.SetData_String(i++, m_C3d.GetFileName());
            m_CFile.SetData_String(i++, txtPort.Text);
            m_CFile.SetData_String(i++, txtBaudrate.Text);
            m_CFile.SetData_String(i++, txtIp.Text);
            m_CFile.SetData_String(i++, txtSocket_Port.Text);
            m_CFile.SetData_Bool(i++, chkFreeze_X.Checked);
            m_CFile.SetData_Bool(i++, chkFreeze_Y.Checked);
            m_CFile.SetData_Bool(i++, chkFreeze_Z.Checked);
            m_CFile.SetData_Bool(i++, chkFreeze_Pan.Checked);
            m_CFile.SetData_Bool(i++, chkFreeze_Tilt.Checked);
            m_CFile.SetData_Bool(i++, chkFreeze_Swing.Checked);
            m_CFile.SetData_String(i++, txtSocket_Port.Text);
            m_CFile.SetData_String(i++, txtMotionCounter.Text);
            m_CFile.SetData_String(i++, txtChangeValue.Text);
            m_CFile.SetData_String(i++, txtPercent.Text);
            m_CFile.SetData_String(i++, txtBackAngle_X.Text);
            m_CFile.SetData_String(i++, txtBackAngle_Y.Text);
            m_CFile.SetData_String(i++, txtBackAngle_Z.Text);
            m_CFile.SetData_Bool(i++, chkTracking.Checked);
            m_CFile.SetData_Bool(i++, chkMp3.Checked);
            m_CFile.SetData_String(i++, txtMp3TimeDelay.Text);
            m_CFile.SetData_Bool(i++, chkFullSize.Checked);
            m_CFile.SetData_Bool(i++, chkDualMonitor.Checked);

            m_CFile.SetData_String(i++, txtID_FR.Text);
            m_CFile.SetData_String(i++, txtID_FL.Text);
            m_CFile.SetData_String(i++, txtID_RR.Text);
            m_CFile.SetData_String(i++, txtID_RL.Text);

            m_CFile.SetData_String(i++, txtID_0.Text);
            m_CFile.SetData_String(i++, txtID_1.Text);
            m_CFile.SetData_String(i++, txtID_2.Text);
            
            m_CFile.SetData_String(i++, m_strWorkDirectory_Dmt);
            m_CFile.SetData_String(i++, m_strWorkDirectory_Mp3);

            m_CFile.Save(Application.StartupPath + _STR_FILENAME);
        }
        private readonly string _STR_FILENAME = "\\Param.dat";
        private void frmMotionEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_bProgEnd = true;
            bool bRet = ProgEnd();
            if (bRet == false)
            {
                e.Cancel = true;
                return;
            }
        }

        private void btnValueIncrement_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Plus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueDecrement_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Minus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueMul_Click(object sender, EventArgs e)
        {
           m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Mul, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueDiv_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Div, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueStackIncrement_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Inc, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueStackDecrement_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Dec, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueChange_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Change, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueFlip_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Flip_Value, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnInterpolation_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Interpolation, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnInterpolation2_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._S_Curve, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnFlip_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Flip_Position, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Clear();
        }

        private void btnGroup1_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_SetSelectedGroup(1);
        }

        private void btnGroup2_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_SetSelectedGroup(2);
        }

        private void btnGroup3_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_SetSelectedGroup(3);
        }

        private void btnGroupDel_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_SetSelectedGroup(0);
        }
        
        //private bool m_bStop = false;
        private bool m_bStart = false;
        private bool m_bEms = false; // 비상정지 용, 아직 사용 안함
        private bool m_bMotionEnd = false;

        private Thread m_thRun;
        private void btnRun_Click(object sender, EventArgs e)
        {
            //btnRun.Enabled = false;
            Ojw.CTimer.Reset(); // Clear the Stop bit;
            
            //m_thRun = new Thread(new ThreadStart(Run));
            //m_thRun.Start();
            tmrRun.Enabled = true;


            //btnRun.Enabled = true;
        }
        
        private void Run()
        {
            btnRun.Enabled = false;
            btnSimul.Enabled = false;

            int nInterval = tmrDraw.Interval;
            if (m_C3d.GetSimulation_With_PlayFrame() == true)
                tmrDraw.Interval = 40;
            ///////////////////////////////////////////
            m_C3d.m_CGridMotionEditor.GetHandle().Focus();
            m_C3d.Start_Set();
            StartMotion();
            m_C3d.Start_Reset();
            ///////////////////////////////////////////            
            if (m_C3d.GetSimulation_With_PlayFrame() == true)
                tmrDraw.Interval = nInterval;
            m_C3d.SetSimulation_With_PlayFrame(false);
            btnRun.Enabled = true;
            btnSimul.Enabled = true;
        }
        private void StartMotion()
        {

            if (m_bStart == true)
            {
                lbMotion_Message.Text = "Motion Running...";//Motion 운전 중입니다.";
                Ojw.CMessage.Write(lbMotion_Message.Text);
                return;
            }
            //if (CheckWifi() == true)
            //{
            //    if (m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_check_Ems() == true)
            //    {
            //        lbMotion_Message.Text = "비상정지 알람이 켜져 있습니다.";
            //        return;
            //    }
            //}
            //else
            //{
            //    //if (m_C3d.m_CMotor.IsEms() == true)
            //    if ((m_C3d.m_CMotor.IsEms() == true) || (frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_check_Ems() == true))
            //    {
            //        lbMotion_Message.Text = "비상정지 알람이 켜져 있습니다.";
            //        return;
            //    }
            //    if ((m_C3d.m_CMotor.IsConnect() == false) && (frmMain.m_DrBluetooth.drbluetooth_client_connected() == false))
            //    {
            //        lbMotion_Message.Text = "Serial Port Error - Not Connected.";
            //        return;
            //    }
            //}

            //if (CheckWifi() == true) // 네트워크 버전이라면 파일을 전송해서 플레이하는 방식으로 한다.
            //{
            //    int nVer = ((chkFileVersionForSave.Checked == true) ? _V_11 : ((chkFileVersionForSave_1_0.Checked == true) ? _V_10 : _V_12));
            //    MakeBinaryFileStream(nVer);
            //    String strFile = m_strFileStream;
            //    DownLoad(m_nCurrentRobot, strFile);
            //    FileInfo fileStream = new FileInfo(strFile);
            //    //if (fileStream.Exists) // 지울 파일이 있는지 체크
            //    //{
            //    // 없더라도 에러가 나지는 않는다. 굳이 에러처리가 필요 없음.
            //    fileStream.Delete();
            //    //}
            //}

            if (m_C3d.m_CMotor.IsEms() == true)
            {
                lbMotion_Message.Text = "Check you Emergency Status";//비상정지 알람이 켜져 있습니다.";
                Ojw.CMessage.Write(lbMotion_Message.Text);
                return;
            }
            if (m_C3d.GetSimulation_With_PlayFrame() == false)
            {
                if (m_C3d.m_CMotor.IsConnect() == false)// && (frmMain.m_DrBluetooth.drbluetooth_client_connected() == false))
                {
                    lbMotion_Message.Text = "Serial Port Error - Not Connected.";
                    Ojw.CMessage.Write(lbMotion_Message.Text);
                    return;
                }
            }
           
            if (chkMp3.Checked == true)
            {
                //Mp3Stop();
                int nCell = m_C3d.m_CGridMotionEditor.m_nCurrntCell;
                if (nCell > 0)
                {
                    DialogResult dlgRet = MessageBox.Show(String.Format("Do you want to start from {0} line?", nCell), "Starting Position Check", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (dlgRet != DialogResult.OK)
                    {
                        dgAngle.CurrentCell = dgAngle.Rows[0].Cells[1];
                        nCell = 0;
                    }
                }
                ChangePos_Mp3Bar();

                // 일단 그리드의 타임값 등을 디스플레이 한다.
                Grid_DisplayTime();
                if ((nCell >= 0) && (dgAngle.RowCount > nCell))
                {
                    //prgMp3.Value = (int)(mpPlayer.Ctlcontrols.currentPosition / dTime * 100);
                    mpPlayer.Ctlcontrols.currentPosition = (double)m_lCalcTime[nCell] / 1000.0;
                }
                m_nMotion_Step = nCell;
            }
            else
                m_nMotion_Step = 0; // 메모리를 날린다. -> -_-

            string strMessage = "";


            // Auto Save 기능을 정지해야 하는데 혹시 몰라 시간초기화도 한다.
            m_CTmr_AutoBackup.Set();
            m_CTmr_AutoBackup.Kill();

            // 타이머 기능 정지
            if (m_C3d.GetSimulation_With_PlayFrame() == false) 
                tmrDraw.Enabled = false;
            //m_C3d.m_CMotor.SetAutoReturn(false);
            m_bStart = true;
            // 그리드 클릭 및 기타 이벤트 금지
            dgAngle.Enabled = false;
            //dgKinematics.Enabled = false;

            m_nMotion_Step = 0; // 메모리를 날린다. -> -_-

            m_bMotionEnd = false;
            m_C3d.m_CMotor.ResetStop();
            //if (CheckWifi() == true)
            //    m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_reset_stop();

            //frmMain.m_DrBluetooth.drbluetooth_set_id(frmMain.m_pnBluetoothAddress[m_nCurrentRobot]);
            //frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_reset_stop();

            lbMotion_Counter.Text = "0";
            int nCnt = 0;
            int nLimitCount = Ojw.CConvert.StrToInt(txtMotionCounter.Text);
            //// Motion Start ////
            //if (CheckWifi() == true)
            //    m_aDrSock[m_nCurrentRobot].drsock_client_request_motion_play(0, m_strFileStream);

            //frmMain.m_DrBluetooth.drbluetooth_set_id(frmMain.m_pnBluetoothAddress[m_nCurrentRobot]);
            //frmMain.m_DrBluetooth.drbluetooth_client_request_motion_play(0, m_strFileStream);
            
            if (chkMp3.Checked == true)
            {
                //#if _ENABLE_MEDIAPLAYER
                Mp3Play();
                //#endif
                Ojw.CTimer.Wait(Ojw.CConvert.StrToLong(txtMp3TimeDelay.Text));
            }

            m_C3d.m_CMotor.ResetStop();
            // Servo / Driver On
            m_C3d.m_CMotor.DrvSrv(true, true);
            int nStep = m_nMotion_Step;
            m_nLoop = 0;
            while (
                    ((nLimitCount <= 0) || (nLimitCount > nCnt)) &&
                    ((m_C3d.m_CMotor.IsEms() == false) && (m_C3d.m_CMotor.IsStop() == false)) &&
                    //((frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_check_stop() == false) && (frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_check_Ems() == false)) &&
                     (m_bMotionEnd == false)
                    )
            {
                if ((m_C3d.m_CMotor.IsEms() == false) && (m_C3d.m_CMotor.IsStop() == false))
                {
                    // 카운터 디스플레이
                    nCnt++;
                    lbMotion_Counter.Text = Ojw.CConvert.IntToStr(nCnt);

                    // 모션 실행
                    nStep = Motion(nStep);
                    
                    // 잔여 Simulation
                    if (m_C3d.GetSimulation_With_PlayFrame() == true)
                    {
                        if (m_C3d.m_nSimulTime_For_Last > 0)
                        {
                            m_C3d.SetSimulation_SetCurrentData();

                            m_C3d.SetSimulation_Calc(m_C3d.m_nSimulTime_For_Last, 1);
                            WaitAction_ByTimer(m_C3d.m_nSimulTime_For_Last);
                            for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) m_C3d.SetData(i, m_C3d.GetSimulation_Value_Next(i));
                        }
                    }

                    if ((nLimitCount <= 0) || (nLimitCount > nCnt))
                    {

                    }
                    else nStep = 0;
                    m_nLoop++;
                }
            }
            m_nMotion_Step = nStep;
            //if ((m_bMotionEnd == true) && (m_bMp3Play == true)) Mp3Stop();
            
            Mp3Stop();

            //// Motion End ////
            #region 종료처리
            if (m_C3d.m_CMotor.IsEms() == true)
            {
                lbMotion_Status.Text = "비상정지";
            }
            else if (m_C3d.m_CMotor.IsStop() == true)
            {
                strMessage = "Motion Stop";
                lbMotion_Status.Text = "일시정지";
            }
            else
            {
                strMessage = "Motion 완료";
                lbMotion_Status.Text = "Ready";
            }
            #endregion 종료처리


#if !_COLOR_GRID_IN_PAINT
            m_C3d.GridMotionEditor_SetColorGrid(dgAngle.CurrentCell.RowIndex, 1);
#endif

            if (strMessage != "")
            {
                lbMotion_Message.Text = strMessage;
            }

            m_bStart = false;
            // 타이머 기능 복원
            tmrDraw.Enabled = true;

            // 다시 Auto Save 를 활성화 한다.
            m_CTmr_AutoBackup.Set();

            // 그리드 클릭 및 기타 이벤트 금지 해제
            dgAngle.Enabled = true;

            //m_C3d.m_CMotor.SetAutoReturn(true);
        }
        private int m_nMotion_Step = 0;
        private int m_nLoop = 0;
        private int Motion(int nLine)
        {
            int nResult = 0;
            int temp_Line = 0;

            int nSize = dgAngle.RowCount;

            bool bAll = true;
            bool[] abSelected = new bool[nSize];
            abSelected.Initialize();
            int nTmpPos = 0;
            int nTmpCnt = 0;
            for (int i = 0; i < nSize; i++)
            {
                if ((m_C3d.GridMotionEditor_GetEnable(i) == true) && (dgAngle[0, i].Selected == true))
                {

                    bAll = false;
                    abSelected[i] = true;

                }

                for (int j = 0; j < dgAngle.ColumnCount; j++)
                {
                    if (dgAngle[j, i].Selected == true)
                    {
                        // 선택한 라인의 갯수를 셈 - 멀티선택인지, 단일선택인지 구분 가능
                        nTmpPos = i;
                        nTmpCnt++;
                        break;
                    }
                }

                //if ((m_C3d.GridMotionEditor_GetEnable(i) == true) && (dgAngle[0, i].Selected == true))
                //{
                //    bAll = false;
                //    break;
                //}
            }

            //if (CheckWifi() == true)
            //{
            //    m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_reset_stop();
            //    m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_drvsrv(true, true);
            //}
            //frmMain.m_DrBluetooth.drbluetooth_set_id(frmMain.m_pnBluetoothAddress[m_nCurrentRobot]);
            //frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_reset_stop();
            //frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_drvsrv(true, true);
            
            int nFirstLine = ((nTmpCnt == 1) && (chkMp3.Checked == true)) ? nTmpPos : 0; // 다중선택이면서 음원과 싱크를 맞춰 출력하려면...
            if (m_nLoop > 0) nFirstLine = 0;

            WaitAction_SetTimer();
            // float fVal;
            for (int i = nFirstLine; i < nSize; i++)
            {
                if (m_C3d.GridMotionEditor_GetEnable(i) == false) continue;
                if ((bAll == false) && (abSelected[i] == false)) continue;
                //if ((bAll == false) && (dgAngle[0, i].Selected == false)) continue;
                //if ((bAll == false) && (dgAngle.Rows[i].Selected == false)) continue;
                if (nLine == temp_Line)
                {
                    //if (m_C3d.GridMotionEditor_GetCommand(i) == 1)
                    if ((m_C3d.GridMotionEditor_GetCommand(i) == 1) || ((m_C3d.GridMotionEditor_GetCommand(i) >= 3) && (m_C3d.GridMotionEditor_GetCommand(i) <= 5))) // 반복문은 1, 3, 4, 5 가 있다.
                    {
                        int nFirst = i;
                        int nLast = (int)m_C3d.GridMotionEditor_GetData0(i);
                        int nRepeat = (int)m_C3d.GridMotionEditor_GetData1(i);
                        for (int k = 0; k < nRepeat; k++)
                        {
                            if (k >= nRepeat - 1) nLast = nFirst;
                            for (int j = nFirst; j <= nLast; j++)
                            {
                                if (m_C3d.GridMotionEditor_GetEnable(j) == false) continue;
                                if (nLine == temp_Line)
                                {
                                    PlayFrame(j);
                                    nLine++;
                                }
                                temp_Line++;
                                if (
#if false
                                    (m_C3d.m_CMotor.IsConnect() == false) ||
#else
                                    ((m_C3d.GetSimulation_With_PlayFrame() == false) && (m_C3d.m_CMotor.IsConnect() == false)) ||
#endif
                                    ((m_C3d.m_CMotor.IsEms() == true) || (m_C3d.m_CMotor.IsStop() == true)) ||
                                    (m_bStart == false)
                                    )
                                    return (temp_Line - 1);
                            }
                        }
                    }
                    else
                    {
                        PlayFrame(i);
                    }
                    nLine++;
                }
                temp_Line++;
                if (
#if false
                    (m_C3d.m_CMotor.IsConnect() == false) ||
#else
                    ((m_C3d.GetSimulation_With_PlayFrame() == false) && (m_C3d.m_CMotor.IsConnect() == false)) ||
#endif
                    //((m_C3d.m_CMotor.IsConnect() == false) && (frmMain.m_aDrSock[m_nCurrentRobot].drsock_client_connected() == false) && (frmMain.m_DrBluetooth.drbluetooth_client_connected() == false)) ||
                    //((frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_check_stop() == true) || (frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_check_Ems() == true)) ||
                    ((m_C3d.m_CMotor.IsEms() == true) || (m_C3d.m_CMotor.IsStop() == true)) ||
                    (m_bStart == false)
                    )
                    return (temp_Line - 1);
            }

            if (nLine == temp_Line)
            {
                //// 동작 ////


                //////////////
                nLine++;
            }
            temp_Line++;

            //m_C3d.m_CMotor.ResetStop();

            //if ((m_C3d.m_CMotor.IsEms() == true) || (m_C3d.m_CMotor.IsStop() == true)) return (temp_Line - 1);

            return nResult;
        }
        //private long m_lWaitActionTimer = 0;

        #region Timer ID - TID
        public const int _CNT_ROBOT = 20;
        public const int TID_MP3CHECK = 99;
        public const int TID_START = 98;
        public const int TID_TIMER = 97;
        //public const int TID_MOTION_BY_TIMER = 96;
        public const int TID_MOTIONS = 76; // 76 ~ 95
        public const int TID_MOTIONS_WAIT_TICK = 56; // 56 ~ 75
        public const int TID_SYNC = 36; // 36 ~ 55
        //public const int TID_FILEBACKUP = 35;
        public const int TID_MOTION2 = 34;
        #endregion Timer ID - TID

        private Ojw.CTimer m_CTmr_AutoBackup = new Ojw.CTimer();
        private void WaitAction_SetTimer()
        {
            //m_lWaitActionTimer = 0;
            m_C3d.WaitAction_SetTimer();
            return;
        }
        private bool WaitAction_ByTimer(long t) { return m_C3d.WaitAction_ByTimer(t); }
        private void PlayFrame(int nFrameNum) { m_C3d.PlayFrame(nFrameNum, 0); }
        
        private void btnStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void Stop()
        {
            //m_bStop = true;
            
            if (m_bMp3Play == true) Mp3Stop();
            m_C3d.m_CMotor.Stop();
            m_C3d.WaitAction_KillTimer();
            m_bMotionEnd = true;
            Ojw.CTimer.Stop();
            
            //if (CheckWifi() == true)
            //{
            //    m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_request_stop();
            //    m_aDrSock[m_nCurrentRobot].drsock_client_request_motion_stop();
            //}
            //frmMain.m_DrBluetooth.drbluetooth_set_id(frmMain.m_pnBluetoothAddress[m_nCurrentRobot]);
            //frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_request_stop();
            //frmMain.m_DrBluetooth.drbluetooth_client_request_motion_stop();
        }

        private void OjwDraw()
        {
            if (m_C3d.GetFileName() != null) 
                if (m_C3d != null) m_C3d.OjwDraw(out m_nGroupA, out m_nGroupB, out m_nGroupC, out m_nKinematicsNumber, out m_bPick, out m_bLimit);
        }
        private float[] m_afOrgAngleDispaly = new float[3];
        private float[] m_afSavedAngleDispaly = new float[3];
        private bool m_bControled_AngleDisplay = false;
        private void btnPos_Go_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            float fX = Ojw.CConvert.StrToFloat(txtBackAngle_X.Text);
            float fY = Ojw.CConvert.StrToFloat(txtBackAngle_Y.Text);
            float fZ = Ojw.CConvert.StrToFloat(txtBackAngle_Z.Text);
            ////////////////////////////////////////////////////////////
            if (chkFreeze_Tilt.Checked == true) fX = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fY = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fZ = fAngle_Z;
            m_C3d.SetAngle_Display(fY, fX, fZ);
            ////////////////////////////////////////////////////////////

            OjwDraw();
        }

        private void btnDisplay_RememberPos_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afSavedAngleDispaly[0] = fAngle_X;
                m_afSavedAngleDispaly[1] = fAngle_Y;
                m_afSavedAngleDispaly[2] = fAngle_Z;
            }
        }

        private void btnDisplay_GetThePose_Click(object sender, EventArgs e)
        {
            m_C3d.SetAngle_Display(m_afSavedAngleDispaly[1], m_afSavedAngleDispaly[0], m_afSavedAngleDispaly[2]);

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(m_afSavedAngleDispaly[0]);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(m_afSavedAngleDispaly[1]);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(m_afSavedAngleDispaly[2]);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_Front_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            m_C3d.SetAngle_Display(0, 0, 0);

            ////////////////////////////////////////////////////////////
            float fAngle_X2 = 0.0f, fAngle_Y2 = 0.0f, fAngle_Z2 = 0.0f;

            if (chkFreeze_Tilt.Checked == true) fAngle_X2 = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fAngle_Y2 = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fAngle_Z2 = fAngle_Z;
            m_C3d.SetAngle_Display(fAngle_Y2, fAngle_X2, fAngle_Z2);
            ////////////////////////////////////////////////////////////


            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(fAngle_X2);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(fAngle_Y2);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(fAngle_Z2);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_Bottom_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            float fX = -90.0f;
            float fY = 0.0f;
            float fZ = 0.0f;
            ////////////////////////////////////////////////////////////
            if (chkFreeze_Tilt.Checked == true) fX = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fY = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fZ = fAngle_Z;
            m_C3d.SetAngle_Display(fY, fX, fZ);
            ////////////////////////////////////////////////////////////

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(fX);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(fY);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(fZ);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_Top_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            float fX = 90.0f;
            float fY = 0.0f;
            float fZ = 0.0f;
            ////////////////////////////////////////////////////////////
            if (chkFreeze_Tilt.Checked == true) fX = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fY = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fZ = fAngle_Z;
            m_C3d.SetAngle_Display(fY, fX, fZ);
            ////////////////////////////////////////////////////////////

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(fX);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(fY);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(fZ);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_Right_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            float fX = 0.0f;
            float fY = 90.0f;
            float fZ = 0.0f;
            ////////////////////////////////////////////////////////////
            if (chkFreeze_Tilt.Checked == true) fX = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fY = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fZ = fAngle_Z;
            m_C3d.SetAngle_Display(fY, fX, fZ);
            ////////////////////////////////////////////////////////////

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(fX);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(fY);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(fZ);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_Left_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            float fX = 0.0f;
            float fY = -90.0f;
            float fZ = 0.0f;
            ////////////////////////////////////////////////////////////
            if (chkFreeze_Tilt.Checked == true) fX = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fY = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fZ = fAngle_Z;
            m_C3d.SetAngle_Display(fY, fX, fZ);
            ////////////////////////////////////////////////////////////

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(fX);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(fY);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(fZ);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_TurnBack_Click(object sender, EventArgs e)
        {
            m_bControled_AngleDisplay = false;

            m_C3d.SetAngle_Display(m_afOrgAngleDispaly[1], m_afOrgAngleDispaly[0], m_afOrgAngleDispaly[2]);

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(m_afOrgAngleDispaly[0]);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(m_afOrgAngleDispaly[1]);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(m_afOrgAngleDispaly[2]);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private bool m_bMouseClick = false;
        private Point m_pntMouse;
        private void frmMotionEditor_MouseDown(object sender, MouseEventArgs e)
        {
            m_bMouseClick = true;
            m_pntMouse.X = e.X;
            m_pntMouse.Y = e.Y;
        }

        private void frmMotionEditor_MouseMove(object sender, MouseEventArgs e)
        {
            if ((m_bMouseClick == true) && (m_pntMouse.Y < 31))
            {
                int nGap_X = m_pntMouse.X - e.X;
                int nGap_Y = m_pntMouse.Y - e.Y;
                this.Left -= nGap_X;
                this.Top -= nGap_Y;
            }
        }

        private void frmMotionEditor_MouseUp(object sender, MouseEventArgs e)
        {
            m_bMouseClick = false;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
            //Application.Exit();
        }
                
        private void btnConnect_Serial_Click(object sender, EventArgs e)
        {
            Connection();
        }
        private void Connect()
        {
            m_C3d.m_CMotor.Connect(Ojw.CConvert.StrToInt(txtPort.Text), Ojw.CConvert.StrToInt(txtBaudrate.Text));
            if (m_C3d.m_CMotor.IsConnect() == true)
            {
                btnConnect_Serial.Text = "Disconnect";
                Ojw.CMessage.Write("Connected");

            }
            else
            {
                m_C3d.m_CMotor.DisConnect();
                btnConnect_Serial.Text = "Connect";
                Ojw.CMessage.Write_Error("Connect Fail -> Check your COMPORT first");

                SetToolTip();
            }
            //m_C3d.OjwGrid_SetHandle_Herculex(m_CMotor);
        }
        private void Disconnect()
        {
            m_C3d.m_CMotor.DisConnect();

            btnConnect_Serial.Text = "Connect";
            Ojw.CMessage.Write("Disconnected");
        }
        private void Connection()
        {
            if (m_C3d.m_CMotor.IsConnect() == false) Connect();
            else Disconnect();
        }

        private void btnPercent_Click(object sender, EventArgs e)
        {
            DelayChange();
        }
        public void DelayChange()
        {
            int i;
            for (i = 0; i < dgAngle.RowCount; i++)
            {
                int nData = (int)Math.Abs(m_C3d.GridMotionEditor_GetTime(i));
                int nDelayValue = m_C3d.GridMotionEditor_GetDelay(i);
                int nPercenct = Ojw.CConvert.StrToInt(txtPercent.Text);
                if ((nDelayValue <= 0) && (nPercenct < 100))
                {
                    float fData0 = (float)nData;
                    float fData1 = (float)nPercenct;

                    nData = -(int)Math.Round((fData0 * ((100.0 - fData1) / 100.0)));
                    m_C3d.GridMotionEditor_SetDelay(i, nData);
                }
            }
        }

        //private AxWMPLib.AxWindowsMediaPlayer mpPlayer;
        private void chkMp3_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMp3.Checked == true)
            {
                ChangePos_Mp3Bar();
            }
        }
        private void ChangePos_Mp3Bar()
        {
            // 일단 그리드의 타임값 등을 디스플레이 한다.
            Grid_DisplayTime();

            int nCell = m_C3d.m_CGridMotionEditor.m_nCurrntCell;
            if ((nCell >= 0) && (dgAngle.RowCount > nCell))
            {
                //prgMp3.Value = (int)(mpPlayer.Ctlcontrols.currentPosition / dTime * 100);
                //#if _ENABLE_MEDIAPLAYER
                mpPlayer.Ctlcontrols.currentPosition = (double)m_lCalcTime[nCell] / 1000.0;
                //#endif
            }
        }
        private void ChangePos_Cell() 
        {
            // 일단 그리드의 타임값 등을 디스플레이 한다.
            Grid_DisplayTime();
            int nCol = 1;
            int nPos = (int)Math.Round(mpPlayer.Ctlcontrols.currentPosition * 1000.0f);
            int nIndex0 = -1, nIndex1 = -2, nIndex_Default = 0;
            for (int i = 0; i < dgAngle.RowCount; i++)
            {
                if (m_C3d.m_CGridMotionEditor.GetEnable(i) == true) nIndex_Default = i;
                if (m_lCalcTime[i] >= nPos) 
                {
                    
                    nIndex0 = i;
                    //break;
                    if (m_C3d.m_CGridMotionEditor.GetEnable(i) == true)
                    {
                        nIndex1 = i;
                        break;
                    }
                }
            }
            if (nIndex0 > 0)
            {
                if (nIndex1 >= nIndex0)
                    dgAngle.CurrentCell = dgAngle.Rows[Math.Max(nIndex0, nIndex1)].Cells[nCol];
            }
            else dgAngle.CurrentCell = dgAngle.Rows[nIndex_Default].Cells[nCol];
        }
        public string[] m_strCalcTime = new string[10000];
        public long[] m_lCalcTime = new long[10000];
        private void Grid_DisplayTime()
        {
            //return;
            Grid_CalcTimer();
            //for (int i = 0; i < dgAngle.RowCount; i++)
            //{
            //    if (Convert.ToString(dgAngle.Rows[i].Cells[dgAngle.ColumnCount - 2].Value) != m_strCalcTime[i])
            //    {
            //        dgAngle.Rows[i].Cells[dgAngle.ColumnCount - 2].Value = m_strCalcTime[i];
            //    }
            //}
        }
        private void Grid_DisplayTime(int nLine)
        {
            if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
            Grid_CalcTimer();
            if (Convert.ToString(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 2].Value) != m_strCalcTime[nLine])
            {
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 2].Value = m_strCalcTime[nLine];
            }
        }
        private void Grid_DisplayTime(int nLine, int nSize)
        {
            int nStart = Ojw.CConvert.Clip(dgAngle.RowCount, 0, nLine);
            int nEnd = Ojw.CConvert.Clip(dgAngle.RowCount, nLine, nLine + nSize);
            for (int i = nStart; i < nEnd; i++)
            {
                Grid_CalcTimer(i);
                if (Convert.ToString(dgAngle.Rows[i].Cells[dgAngle.ColumnCount - 2].Value) != m_strCalcTime[i])
                {
                    dgAngle.Rows[i].Cells[dgAngle.ColumnCount - 2].Value = m_strCalcTime[i];
                }
            }
        }
        private long Grid_CalcTimer(int nLine) //
        {
            if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;

            int i = nLine - 1;

            bool bNull = false;
            if (nLine <= 0) bNull = true;

            if (bNull == false)
            {
                // 시간값 계산
                // En
                short sData = (short)Ojw.CConvert.BoolToInt(m_C3d.GridMotionEditor_GetEnable(i)); // En
                //                short sData0 = Convert.ToInt16(rowData[i]["Data0"]);
                long nSpeed = 0;
                long nDelay = 0;
                long nTimer = 0;
                if (sData != 0) // En 이 되어있다면
                {
                    // Speed
                    nSpeed = (long)m_C3d.GridMotionEditor_GetTime(i);
                    // Delay
                    nDelay = (long)m_C3d.GridMotionEditor_GetDelay(i);
                    //nTimer += nSpeed * 10 + nDelay * 10;
                    nTimer += nSpeed + nDelay;
                }
                m_lCalcTime[nLine] = m_lCalcTime[i] + nTimer;
                int nMs = (int)(m_lCalcTime[nLine] % 1000);
                int nAllSec = (int)(m_lCalcTime[nLine] / 1000);
                int nS = nAllSec % 60;
                int nM = (nAllSec / 60) % 60;
                int nH = (nAllSec / 60) / 60;
                // Hour
                string strTmp = Ojw.CConvert.IntToStr(nH);
                if (strTmp.Length < 2) strTmp = "0" + strTmp;
                m_strCalcTime[nLine] = strTmp + ":";
                // Minute
                strTmp = Ojw.CConvert.IntToStr(nM);
                if (strTmp.Length < 2) strTmp = "0" + strTmp;
                m_strCalcTime[nLine] = m_strCalcTime[nLine] + strTmp + ":";
                // Second
                strTmp = Ojw.CConvert.IntToStr(nS);
                if (strTmp.Length < 2) strTmp = "0" + strTmp;
                m_strCalcTime[nLine] = m_strCalcTime[nLine] + strTmp + ".";
                // 1 Milli-Second
                strTmp = Ojw.CConvert.IntToStr(nMs);
                if (strTmp.Length < 2) strTmp = "0" + strTmp;
                m_strCalcTime[nLine] = m_strCalcTime[nLine] + strTmp;
            }
            else
            {
                m_strCalcTime[nLine] = "00:00:00.00";
                m_lCalcTime[nLine] = 0;
            }
            return m_lCalcTime[nLine];
        }
        private void Grid_CalcTimer() { for (int i = 0; i < dgAngle.RowCount; i++) Grid_CalcTimer(i); }

        private void chkTracking_CheckedChanged(object sender, EventArgs e)
        {
            m_C3d.m_bControl_Tracking = chkTracking.Checked;
        }

        private bool m_btmrRun = false;
        private void tmrRun_Tick(object sender, EventArgs e)
        {
            if (m_bProgEnd == true) return;

            tmrRun.Enabled = false;

            if (m_btmrRun == true) return;
            m_btmrRun = true;

            Run();

            m_btmrRun = false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            m_nMotion_Step = 0;
            m_C3d.m_CMotor.ResetEms();
            m_C3d.m_CMotor.ResetStop();
            m_C3d.m_CMotor.Reset();
        }

        private void btnMotionEnd_Click(object sender, EventArgs e)
        {
            m_bMotionEnd = true;
        }

        private void Ems()
        {
            Stop();
            //if (m_bMp3Play == true) Mp3Stop();
            m_C3d.m_CMotor.Ems();
        }
        private void btnEms_Click(object sender, EventArgs e)
        {
            Ems();
        }

        private void btnMode0_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(0);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnMode1_Click(object sender, EventArgs e)
        {

            m_C3d.Prop_Set_Main_MouseControlMode(1);
            m_C3d.Prop_Update_VirtualObject();
        }

        private String GetMotionFileName(String strFilePath)
        {
            String _STR_EXT = "dmt";
            String fileName = "";
            SaveFileDialog sdDialog = new SaveFileDialog();
            /////////////////////////////////////////
            // 파일 저장시 확장자가 ".dmt" 가 아니라면 새로 저장하도록...
            String strExe = Ojw.CFile.GetExe(strFilePath);
            if ((strExe == null) || (Ojw.CFile.GetExe(strFilePath).ToLower() != _STR_EXT.ToLower()))
            {
                sdDialog.FileName = null;
                sdDialog.DefaultExt = _STR_EXT.ToLower();
                if (sdDialog.ShowDialog() == DialogResult.OK)
                    fileName = sdDialog.FileName;
            }
            else fileName = strFilePath;
            /////////////////////////////////////////
            sdDialog.Dispose();

            //m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(fileName);
            //if (m_strWorkDirectory_Dmt == null) m_strWorkDirectory_Dmt = Application.StartupPath;
            return fileName;
        }
        private const int _V_10 = 0;
        private const int _V_11 = 1;
        private const int _V_12 = 2;
        private void btnTextSave_Click(object sender, EventArgs e)
        {
            //int nVer = ((chkFileVersionForSave.Checked == true) ? _V_11 : ((chkFileVersionForSave_1_0.Checked == true) ? _V_10 : _V_12));
            m_C3d.BinaryFileSave(_V_10, GetMotionFileName(txtFileName.Text), false);
            m_CTmr_Save.Set();            
        }

        private void chkFreeze_X_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_X(chkFreeze_X.Checked); }
        private void chkFreeze_Y_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_Y(chkFreeze_Y.Checked); }
        private void chkFreeze_Z_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_Z(chkFreeze_Z.Checked); }
        private void chkFreeze_Tilt_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_Tilt(chkFreeze_Tilt.Checked); }
        private void chkFreeze_Pan_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_Pan(chkFreeze_Pan.Checked); }
        private void chkFreeze_Swing_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_Swing(chkFreeze_Swing.Checked); }
        
        private bool SetDirectory(OpenFileDialog OpenFileDlg, String strDir)
        {
            try
            {
                //Directory.SetCurrentDirectory(strDir);
                OpenFileDlg.InitialDirectory = strDir;
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool m_bModify = false;
        private void Modify(bool bModify)
        {
            if ((bModify == true) && (m_bModify != bModify)) m_CTmr_AutoBackup.Set();
            m_bModify = bModify;
            lbModify.ForeColor = (bModify == true) ? Color.Red : Color.Green;
            lbModify.Text = (bModify == true) ? "수정중..." : "완료";
        }
        private void btnMotionFileOpen_Click(object sender, EventArgs e)
        {
            if (m_bModify == true)
            {
                DialogResult dlgRet = MessageBox.Show("문서를 저장하지 않고 다른파일을 열면 데이터가 사라집니다.\r\n\r\n그래도 파일을 여시겠습니까?", "파일열기", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dlgRet != DialogResult.OK)
                {
                    return;
                }
            }
            m_C3d.m_CGridMotionEditor.SetSelectedGroup(0);
            OpenFileDialog ofdMotion = new OpenFileDialog();
            ofdMotion.FileName = "*.dmt";
            ofdMotion.Filter = "모션 파일(*.dmt)|*.dmt";
            ofdMotion.DefaultExt = "dmt";
            SetDirectory(ofdMotion, m_strWorkDirectory_Dmt);
            if (ofdMotion.ShowDialog() == DialogResult.OK)
            {
                String fileName = ofdMotion.FileName;
                //m_strWorkDirectory_Dmt = Directory.GetCurrentDirectory();
                m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(fileName);
                if (m_strWorkDirectory_Dmt == null) m_strWorkDirectory_Dmt = Application.StartupPath;

                txtFileName.Text = fileName;
                if (m_C3d.DataFileOpen(fileName, null) == false)
                {
                    MessageBox.Show(ofdMotion.DefaultExt.ToUpper() + " 모션 파일이 아닙니다.");
                }
                else
                {
                    Modify(false);
                    Grid_DisplayTime();

                    txtTableName.Text = m_C3d.GetMotionFile_Title();
                    txtComment.Text = m_C3d.GetMotionFile_Comment();
                    cmbStartPosition.SelectedIndex = m_C3d.GetMotionFile_StartPosition();
                }
            }

            //m_strWorkDirectory_Dmt = Directory.GetCurrentDirectory();
            //WriteRegistry_Path(m_strWorkDirectory);
            ofdMotion.Dispose();
        }

        private void pnButton_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnBinarySave_Click(object sender, EventArgs e)
        {
            m_C3d.BinaryFileSave(_V_10, GetMotionFileName(txtFileName.Text), true);
            m_CTmr_Save.Set(); 
        }

        private void btnInitpos_Click(object sender, EventArgs e)
        {
            DialogResult dlgRet;            
            if (m_C3d.m_CGridMotionEditor.Clear_GetType() != 0)
            {
                dlgRet = MessageBox.Show("Do you want to change your clear type to [Default]?", "Setting Clear Type", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dlgRet != DialogResult.OK)
                {
                }
                else
                {
                    m_C3d.m_CGridMotionEditor.Clear_SetType(0);
                    Ojw.CMessage.Write("Changed Clear Type -> 0");
                }
            }
            if (m_C3d.m_CMotor.IsConnect() == true)
            {
                dlgRet = MessageBox.Show("Do you want to Move your InitPos?", "Move Init(Default) Motion", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dlgRet != DialogResult.OK)
                {
                }
                else
                {
                    m_C3d.m_CMotor.ResetStop();
                    m_C3d.m_CMotor.DrvSrv(true, true);
                    for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) { m_C3d.m_CMotor.SetCmd_Angle(i, m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle); }
                    m_C3d.m_CMotor.SetMot(1000);
                }
            }
        }

        private void btnInitpos2_Click(object sender, EventArgs e)
        {
            DialogResult dlgRet;
            if (m_C3d.m_CGridMotionEditor.Clear_GetType() != 1)
            {
                dlgRet = MessageBox.Show("Do you want to change your clear type to [Second Type]?", "Setting Clear Type", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dlgRet != DialogResult.OK)
                {
                }
                else
                {
                    m_C3d.m_CGridMotionEditor.Clear_SetType(1);
                    Ojw.CMessage.Write("Changed Clear Type -> 1");
                }
            }
            if (m_C3d.m_CMotor.IsConnect() == true)
            {
                dlgRet = MessageBox.Show("Do you want to Move your InitPos?", "Move Init(second) Motion", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dlgRet != DialogResult.OK)
                {
                }
                else
                {
                    m_C3d.m_CMotor.ResetStop();
                    m_C3d.m_CMotor.DrvSrv(true, true);
                    for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) { m_C3d.m_CMotor.SetCmd_Angle(i, m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle2); }
                    m_C3d.m_CMotor.SetMot(1000);
                }
            }
        }
        // 시간값 계산

        private const int _INITPOS_CLEAR = 0;
        private const int _INITPOS_DEFAULT = 1;
        private const int _INITPOS_SECOND = 2;
        // 초기모션
        private void Cmd_InitPos(int nInitNum, int nTime)
        {
            if (m_C3d.m_CHeader == null)
            {
                DialogResult dlgRet = MessageBox.Show("Please check your Modeling File.", "Fail to runing a Initial motion", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dlgRet != DialogResult.OK) return;
                return;
            }
            
            m_C3d.m_CMotor.ResetStop();
            m_C3d.m_CMotor.DrvSrv(true, true);
            
            //SetAxisParam(nRobot); // 로봇의 정보를 셋팅한다.

            for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++)
            {
                if ((m_C3d.m_CHeader.pSMotorInfo[i].nMotorControlType == 0) || (m_C3d.m_CHeader.pSMotorInfo[i].nMotorControlType == 2))
                {
                    m_C3d.m_CMotor.SetCmd_Flag_Mode(i, false); // 위치제어 모드
                    m_C3d.m_CMotor.SetCmd_Flag_NoAction(i, false);
                    //m_C3d.m_CMotor.SetCmd_Flag_Led(nAxis, bGreen, bBlue, bRed);                        
                    m_C3d.m_CMotor.SetCmd_Angle(i, ((nInitNum == 0) ? 0 : ((nInitNum == 1) ? m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle : m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle2)));
                }
                else
                {
                    m_C3d.m_CMotor.SetCmd_Flag_Mode(i, true); // 속도모드
                    m_C3d.m_CMotor.SetCmd_Flag_NoAction(i, false);
                    //OjwMotor.SetCmd_Flag_Stop(i, true);
                    m_C3d.m_CMotor.SetCmd(i, 0);
                    
                }
            }
            m_C3d.m_CMotor.SetMot(nTime);
        }

        private void SetToolTip()
        {       
            ToolTip tp = new ToolTip();
            tp.ToolTipTitle = String.Format("Serial Port");
            tp.ToolTipIcon = ToolTipIcon.Info;
            tp.IsBalloon = true;
            tp.ShowAlways = true;
            
            string[] pstrComport = null;
            Ojw.CRegistry.GetSerialPort(out pstrComport, true, true);
            if (pstrComport != null)
            {
                string strData = string.Empty;
                if (pstrComport.Length > 0)
                {
                    for (int i = 0; i < pstrComport.Length - 1; i++)
                    {
                        strData += pstrComport[i] + ", ";
                    }
                    strData += pstrComport[pstrComport.Length - 1];
                    tp.SetToolTip(txtPort, String.Format("You can choose your comport numbers = {0}", strData));
                    //tp.Show(String.Format("You can choose your comport numbers = {0}", strData), txtPort);
                }
                else
                {
                    tp.SetToolTip(txtPort, String.Format("I cannot find any serial port in this computer"));
                    //tp.Show(String.Format("I cannot find any serial port in this computer"), txtPort);
                }
            }
            //tp.Dispose();
        }

        private int GetInverseKinematicsNumber(int nMotor)
        {
            int nNum = -1;
            for (int i = 0; i < m_C3d.GetHeader_pSOjwCode().GetLength(0); i++)
            {
                // 실제 수식계산
                //Ojw.CKinematics.CInverse.CalcCode(ref m_C3d.GetHeader_pSOjwCode()[i]);
                int nMotCnt = m_C3d.GetHeader_pSOjwCode()[i].nMotor_Max;
                for (int j = 0; j < nMotCnt; j++)
                {
                    int nMotNum = m_C3d.GetHeader_pSOjwCode()[nNum].pnMotor_Number[j];
                    if (nMotNum == nMotor) { nNum = nMotNum; break; }
                }
            }
            return nNum;
        }
        private void btnX_Plus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._X_Plus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnX_Minus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._X_Minus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnY_Plus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Y_Plus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnY_Minus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Y_Minus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnZ_Plus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Z_Plus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnZ_Minus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Z_Minus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        #region Mp3
        public bool m_bScreen = true;
        private bool CheckAudioExist(String strData)
        {
            string strFilter = ".wma,.wax,.cda,.mp3,.m3u,.mid,.midi,.rmi,.air,.aifc,.aiff,.au,.snd";
            string[] pstrFilter;
            pstrFilter = strFilter.Split(',');
            return CheckFileExist(strData, pstrFilter);
        }

        private bool CheckMovieExist(String strData)
        {
            string strFilter = ".avi,.asf,.asx,.wpl,.wm,.wmx,.wmd,.wmz,.wmv,.wav,.mpeg,.mpg,.mpe,.m1v,.m2v,.mod,.mp2,.mpv2,.mp2v,.mpa,.mp4";
            string[] pstrFilter;
            pstrFilter = strFilter.Split(',');
            return CheckFileExist(strData, pstrFilter);
        }

        private bool CheckFileExist(String strData, string[] pstrFilter)
        {
            bool bFind = false;
            foreach (string strItem in pstrFilter)
            {
                if (strData.IndexOf(strItem) == (strData.Length - 4))
                {
                    bFind = true;
                    break;
                }
            }
            return bFind;
        }
        private void btnMp3Open_Click(object sender, EventArgs e)
        {
            m_dMp3_Max = 0;
            prgMp3.Value = 0;

            OpenFileDialog ofdMp3 = new OpenFileDialog();
            ofdMp3.FileName = null;
#if false
            // 동영상
            // avi,asf,asx,wpl,wm,wmx,wmd,wmz,wmv,wav,mpeg,mpg,mpe,m1v,m2v,mod,mp2,mpv2,mp2v,mpa,mp4
            strFilter = ".avi,.asf,.asx,.wpl,.wm,.wmx,.wmd,.wmz,.wmv,.wav,.mpeg,.mpg,.mpe,.m1v,.m2v,.mod,.mp2,.mpv2,.mp2v,.mpa,.mp4";
            // 오디오
            // wma,wax,cda,mp3,m3u,mid,midi,rmi,air,aifc,aiff,au,snd
            strFilter += ",.wma,.wax,.cda,.mp3,.m3u,.mid,.midi,.rmi,.air,.aifc,.aiff,.au,.snd";

            ofdMp3.FileName = "*.dmt";
            ofdMp3.Filter = "Audio 파일(*.dmt)|*.dmt";
#endif
            //ofdMp3.Filter = "AVI|*.avi|asf|*.asf|asx|*.asx|wpl|*.wpl|wm|*.wm|wmx|*.wmx|wmd|*.wmd|wmz|*.wmz|wmv|*.wmv|wav|*.wav|mpeg|*.mpeg|mpg|*.mpg|mpe|*.mpe|m1v|*.m1v|m2v|*.m2v|mod|*.mod|mp2|*.mp2|mpv2|*.mpv2|mp2v|*.mp2v|mpa|*.mpa|mp4|*.mp4|wma|*.wma|wax|*.wax|cda|*.cda|mp3|*.mp3|m3u|*.m3u|mid|*.mid|midi|*.midi|rmi|*.rmi|air|*.air|aifc|*.aifc|aiff|*.aiff|au|*.au|snd|*.snd";
            ofdMp3.Filter = "Media Files(*.avi,*.asf,*.asx,*.wpl,*.wm,*.wmx,*.wmd,*.wmz,*.wmv,*.wav,*.mpeg,*.mpg,*.mpe,*.m1v,*.m2v,*.mod,*.mp2,*.mpv2,*.mp2v,*.mpa,*.mp4,*.wma,*.wax,*.cda,*.mp3,*.m3u,*.mid,*.midi,*.rmi,*.air,*.aifc,*.aiff,*.au,*.snd)|*.avi;*.asf;*.asx;*.wpl;*.wm;*.wmx;*.wmd;*.wmz;*.wmv;*.wav;*.mpeg;*.mpg;*.mpe;*.m1v;*.m2v;*.mod;*.mp2;*.mpv2;*.mp2v;*.mpa;*.mp4;*.wma;*.wax;*.cda;*.mp3;*.m3u;*.mid;*.midi;*.rmi;*.air;*.aifc;*.aiff;*.au;*.snd";
            ofdMp3.DefaultExt = "mp3";
            ofdMp3.InitialDirectory = m_strWorkDirectory_Mp3;//Application.StartupPath + "\\music\\";
            if (ofdMp3.ShowDialog() == DialogResult.OK)
            {
                String fileName = ofdMp3.FileName;
                lbMp3File.Text = Ojw.CFile.GetTitle(fileName);
                String strExe = Ojw.CFile.GetExe(fileName);

                if ((strExe.ToUpper() != "MP3") && (strExe.ToUpper() != "WAV")) m_bScreen = true;
                else m_bScreen = false;
                InitMp3();

                mpPlayer.URL = fileName;
                mpPlayer.settings.volume = 0;
                mpPlayer.Ctlcontrols.play();
                Ojw.CTimer CTmr = new Ojw.CTimer();
                CTmr.Set();
                //while (m_dMp3_Max <= 0)
                while (true)
                {
                    if (mpPlayer.Ctlcontrols.currentItem != null)
                    {
                        if (mpPlayer.Ctlcontrols.currentItem.duration > 0)
                            break;
                    }
                    //OjwTmrMp3();
                    if (CTmr.Get() > 5000) break;
                    //COjwTimer.WaitTimer(50);
                    Application.DoEvents();
                }
                mpPlayer.Ctlcontrols.stop();
                mpPlayer.settings.volume = 100;
                chkMp3.Checked = true;

                m_strWorkDirectory_Mp3 = Ojw.CFile.GetPath(fileName);
                if (m_strWorkDirectory_Mp3 == null) m_strWorkDirectory_Mp3 = Application.StartupPath;

                //tmrMp3.Enabled = true;
            }
            else
            {
                //txtFileName.Text = "";
                //m_bFileOpened = false;
            }
            ofdMp3.Dispose();
        }
        private void SetPlayTime()
        {
            CheckTime();
            // 시간을 설정
            lbMp3Time.Text = String.Format("{0:D2}:", m_nMp3Minutes) + String.Format("{0:D2}.", m_nMp3Seconds) + String.Format("{0:D2}", m_nMp3MilliSeconds);
        }

        public int m_nMp3TmpValue = 0;
        public bool m_bMp3MouseClick = false;
        public bool m_bMp3Play = false;
        public int m_nMp3Seconds = 0;
        public int m_nMp3Minutes = 0;
        public int m_nMp3MilliSeconds = 0;
        // 플레이 시간 체크
        private void CheckTime()
        {
            double dTime;
            int nTime;

            // 현 위치의 시간을 얻는다.
            dTime = mpPlayer.Ctlcontrols.currentPosition;
            if (dTime < 0)
                dTime = 0.0;
            nTime = (int)dTime;
            // 시간을 설정
            m_nMp3Seconds = nTime % 60;
            m_nMp3Minutes = (int)(nTime / 60);
            m_nMp3MilliSeconds = (int)(dTime * 100.0) % 100;
        }
        
        private void InitMp3()
        {
            SetPlayTime(); // 진행시간 표시를 위해 시간 값 초기화
            //SetPlayState(0); // 플레이어 상태 설정 ( 0 - Ready, 1 - Paly, 2 - Pause, 3 - Stop, 4 - End, 5 - Eject )
        }

        private Point m_pntMain;
        private void Mp3Play()
        {
            try
            {
                //m_pntMain = this.Location;
                prbStatus.Visible = true;

                mpPlayer.Visible = true;
                // 동영상의 경우 Visible == true 이므로 이 경우에만 동영상 화면크기의 셋업을 실행하도록 한다.
                if (m_bScreen == true)
                {

                }

                //SetPlayState(1);

                mpPlayer.Ctlcontrols.play();
                m_bOneShotMp3Command = true;

                m_bMp3Play = true;

                Ojw.CMessage.Write("[Message] Playing Music...\r\n");

                //tmrMp3.Enabled = true;
            }
            catch (Exception error)
            {
                Ojw.CMessage.Write("[Message]" + error.ToString() + "\r\n");
                m_bMp3Play = false;
                mpPlayer.Visible = false;
                //SetPlayState(0);
            }
        }
        private void Mp3Stop()
        {
            try
            {
                mpPlayer.Ctlcontrols.stop();
                mpPlayer.Ctlcontrols.currentPosition = 0;
                m_bOneShotMp3Command = false;
                Ojw.CMessage.Write("[Message] Stoped Music...\r\n");
            }
            catch (Exception error)
            {
                Ojw.CMessage.Write("[Message]" + error.ToString() + "\r\n");
            }
            //SetPlayState(3);

            // 시간 초기화
            m_nMp3Seconds = 0;
            m_nMp3Minutes = 0;

            m_nMp3TmpValue = 0;

            // Play 상태 없앰
            m_bMp3Play = false;
            mpPlayer.Visible = false;

            prbStatus.Visible = false;

            //tmrMp3.Enabled = false;
        }
        private bool OjwMusicFileOpen(String strFileName)
        {
            // 파일 자체의 유무 확인
            bool bExist = ((CheckAudioExist(strFileName) == true) || (CheckMovieExist(strFileName) == true)) ? true : false;

            if (bExist == true) // 있다면 오픈
            {
                lbMp3File.Text = Ojw.CFile.GetTitle(strFileName);
                //m_strFileNameMp3 = fileName;
                String strExe = Ojw.CFile.GetExe(strFileName);

                if ((strExe.ToUpper() != "MP3") && (strExe.ToUpper() != "WAV")) m_bScreen = true;
                else m_bScreen = false;
                InitMp3();
                mpPlayer.URL = strFileName;
                // 파일 데이터 저장
                //TextConfigFileSave(m_strOrgDirectory + "\\ip.ini");
            }

            return bExist;
        }
        private void OjwTmrMp3()
        {
            SetPlayTime();

            // 파일의 전체 플레이 시간 가져오기
            double dTime = 0;
            int nTime;

            if (mpPlayer.Ctlcontrols.currentItem != null) dTime = mpPlayer.Ctlcontrols.currentItem.duration;


            if ((int)dTime < 0)
                dTime = 0.0;
            nTime = (int)dTime;
            m_dMp3_Max = dTime * 1000;
            int nTimeMilli = (int)(dTime * 100.0) % 100;
            // 시간을 설정
            lbMp3AllTime.Text = String.Format("{0:D2}:", nTime / 60) + String.Format("{0:D2}:", (int)(nTime % 60)) + String.Format("{0:D2}", nTimeMilli);
            if ((m_dMp3_Max > 0) && (dTime > 0))
            {
                prgMp3.Minimum = 0;
                prgMp3.Maximum = 100;// (int)m_dMp3_Max;
                prgMp3.Value = (int)(mpPlayer.Ctlcontrols.currentPosition / dTime * 100);//(int)(mpPlayer.Ctlcontrols.currentPosition * 1000);
            }
        }

        // Button
        frmPlayer frmPlayerForm;
        private void btnMp3Play_Click(object sender, EventArgs e)
        {
            Mp3Play();
        }

        private void btnMp3Stop_Click(object sender, EventArgs e)
        {
            Mp3Stop();
            if (frmPlayerForm != null) frmPlayerForm.CloseSplash();
        }
        private Ojw.CTimer m_CTmr_Start = new Ojw.CTimer();
        private void tmrMp3_Tick(object sender, EventArgs e)
        {
            if (m_bProgEnd == true) return;

            if (m_bStart == true)
            {
                //int nTimerValue = (int)m_CTmr_Start.Get();
                //prbStatus.Value = ((nTimerValue < prbStatus.Maximum) ? nTimerValue : prbStatus.Maximum);
            }

            if ((mpPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying) && (m_bOneShotMp3Command == true))
            {
                if (chkFullSize.Checked == true)
                {

                    Int32 currentMonitorCount = Screen.AllScreens.Length;

                    if ((currentMonitorCount < 2) || (chkDualMonitor.Checked == false))
                    {
                        //Put app in single screen mode.
                        frmPlayerForm = new frmPlayer();
                        frmPlayerForm.Show();

                        frmPlayerForm.Left = Screen.AllScreens[0].Bounds.Location.X;
                        frmPlayerForm.Top = Screen.AllScreens[0].Bounds.Location.Y;
                        frmPlayerForm.Size = Screen.AllScreens[0].Bounds.Size;//.WindowState = FormWindowState.Maximized;

                    }
                    else
                    {
                        //Put app in dual screen mode.
                        // 현재 내가 있는 화면의 반대편으로 이동하도록...
                        int nPos = 0;
                        if ((this.Left >= Screen.AllScreens[0].Bounds.Location.X) && ((this.Left <= (Screen.AllScreens[0].Bounds.Location.X + Screen.AllScreens[0].Bounds.Width))))
                            nPos = 1;
                        m_pntMain = this.Location;

                        frmPlayerForm = new frmPlayer();

                        //frmPlayerForm.BackColor = Color.Green;
                        //frmPlayerForm.Dock = DockStyle.Fill;
                        frmPlayerForm.Visible = false;
                        frmPlayerForm.Show();

                        frmPlayerForm.Left = Screen.AllScreens[nPos].Bounds.Location.X;
                        frmPlayerForm.Top = Screen.AllScreens[nPos].Bounds.Location.Y;
                        //frmPlayerForm.FormBorderStyle = FormBorderStyle.None;
                        frmPlayerForm.Size = Screen.AllScreens[nPos].Bounds.Size;//.WindowState = FormWindowState.Maximized;
                        frmPlayerForm.Visible = true;
                        this.Visible = false;
                        this.Location = Screen.AllScreens[nPos].Bounds.Location;
                    }


                    //mpPlayer.Left = sc[1].Bounds.Location.X;
                    //mpPlayer.Top = sc[1].Bounds.Location.Y;
                    //mpPlayer.SetBounds(sc[0].Bounds.Location.X, sc[0].Bounds.Location.Y, sc[0].Bounds.Width, sc[0].Bounds.Height);

                    //mpPlayer.playerApplication.switchToPlayerApplication();

                    mpPlayer.fullScreen = true;
                    this.Location = m_pntMain;
                    this.Visible = true;
                    m_bOneShotMp3Command = false;
                }
            }


            CheckPlayState();

            OjwTmrMp3();
        }

        private void CheckPlayState()
        {
            lbPlayState.Text = mpPlayer.status;
        }
        #endregion Mp3

        private bool m_bMouseDown = false;
        private double m_dMp3_Max = 0;
        private void prgMp3_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_dMp3_Max > 0)
            {
                m_bMouseDown = true;
                double dPos = (double)e.X / (double)prgMp3.Width * (double)m_dMp3_Max;//100;
                mpPlayer.Ctlcontrols.currentPosition = (double)(dPos / 1000.0);
            }
        }

        private void prgMp3_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDown == true)
            {
                double dPos = (double)e.X / (double)prgMp3.Width * (double)m_dMp3_Max;//100;
                mpPlayer.Ctlcontrols.currentPosition = (double)(dPos / 1000.0);
            }
        }

        private void prgMp3_MouseUp(object sender, MouseEventArgs e)
        {
            m_bMouseDown = false;
            if (chkMp3.Checked == true)
                ChangePos_Cell();
        }

        private void btnSync_Grid_Mp3_Click(object sender, EventArgs e)
        {
            ChangePos_Mp3Bar();
        }

        private void btnSync_Mp3_Grid_Click(object sender, EventArgs e)
        {
            ChangePos_Cell();
        }

        private void btnOpenDesignFile_Click(object sender, EventArgs e)
        {
            m_C3d.FileOpen();
        }

        private void btnCmd_Repeat_Click(object sender, EventArgs e)
        {
            String strValue = "1";
            if (Ojw.CInputBox.Show("Repeat Count", "Set the Repeat Count", ref strValue) == DialogResult.OK)
            {
                int nCnt = Ojw.CConvert.StrToInt(strValue);

                int nX_Limit = dgAngle.RowCount;
                int nY_Limit = dgAngle.ColumnCount;
                int nPos = nX_Limit;
                int nLineNumber = 0;
                for (int i = 0; i < nX_Limit; i++)
                {
                    for (int j = 0; j < nY_Limit; j++)
                    {
                        if (dgAngle[j, i].Selected == true)
                        {
                            if (i < nPos) nPos = i;
                            if (i > nLineNumber) nLineNumber = i;
                            break;
                        }
                    }
                }
                m_C3d.m_CGridMotionEditor.SetData0(nPos, nLineNumber);
                m_C3d.m_CGridMotionEditor.SetData1(nPos, nCnt);
                m_C3d.m_CGridMotionEditor.SetCommand(nPos, 1);

                // 색칠하기...
                m_C3d.GridMotionEditor_SetSelectedGroup(4);
                //m_C3d.m_CGridMotionEditor.SetColorGrid(0, dgAngle.RowCount);
            }
        }

        private void btnCmd_Clear_Click(object sender, EventArgs e)
        {
            int nX_Limit = dgAngle.RowCount;
            int nY_Limit = dgAngle.ColumnCount;
            for (int i = 0; i < nX_Limit; i++)
            {
                for (int j = 0; j < nY_Limit; j++)
                {
                    if (dgAngle[j, i].Selected == true)
                    {
                        m_C3d.m_CGridMotionEditor.SetCommand(i, 0);
                        m_C3d.m_CGridMotionEditor.SetData0(i, 0);
                        m_C3d.m_CGridMotionEditor.SetData1(i, 0);
                        if (m_C3d.m_CGridMotionEditor.GetGroup(i) == 4)
                        {
                            m_C3d.m_CGridMotionEditor.SetGroup(i, 0);
                        }
                        break;
                    }
                }
            }
            // 색칠하기...
            //m_C3d.GridMotionEditor_SetSelectedGroup(0);
        }


        #region For Wheel
        private void CheckWheel4Dir_Display(float f1, float f2, float f3, float f4)
        {
            int nDir = CheckGenieDir(f1, f2, f3);
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하
            switch (nDir)
            {
                case 1:
                    lbWheel4Dir.Text = "↑";
                    break;
                case 2:
                    lbWheel4Dir.Text = "↓";
                    break;
                case 3:
                    lbWheel4Dir.Text = "←";
                    break;
                case 4:
                    lbWheel4Dir.Text = "→";
                    break;
                case 5:
                    lbWheel4Dir.Text = "↖";
                    break;
                case 6:
                    lbWheel4Dir.Text = "↗";
                    break;
                case 7:
                    lbWheel4Dir.Text = "↙";
                    break;
                case 8:
                    lbWheel4Dir.Text = "↘";
                    break;
                default:
                    lbWheel4Dir.Text = "□";
                    break;
            }
        }
        
        private int CheckWheel4Dir(float f1, float f2, float f3, float f4)
        {
            int nDir = 0;
            float dist1, fVal, fMin = 0;
            if ((f1 * f1 + f2 * f2 + f3 * f3 + f4 * f4) != 0) // 정지가 아니라면
            {
                float cf1, cf2, cf3, cf4;
                for (int i = 1; i < 11; i++)
                {
                    CalcWheel4Dir(i, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out cf1, out cf2, out cf3, out cf4);
                    dist1 = (f1 - cf1) * (f1 - cf1) + (f2 - cf2) * (f2 - cf2) + (f3 - cf3) * (f3 - cf3) + (f4 - cf4) * (f4 - cf4);
                    fVal = (float)Math.Abs(dist1);
                    if ((nDir == 0) || (fVal < fMin))
                    {
                        nDir = i;
                        fMin = fVal;
                    }
                }
            }
            return nDir;
        }
        // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌턴(제자리), 4 - 우턴(제자리), 5 - 좌턴(전진하면서), 6 - 우턴(전진하면서), 7 - 좌턴(후진하면서), 8 - 우턴(후진하면서)
        private void CalcWheel4Dir(int nDir, float fSpeed, out float f1, out float f2, out float f3, out float f4)
        {
            float a;
            //float fRatio = 0.5f;// 0.75f;
            float fRatio_Org = 1.0f;// 0.75f;
            float fRatio_Turn = 2.0f;// 0.75f;
            float ff1 = 0.0f, ff2 = 0.0f, ff3 = 0.0f, ff4 = 0.0f;
            switch (nDir)
            {
                case 1: // 1 - 전진
                    a = (float)Math.Round(fSpeed);
                    ff1 = a; ff2 = a; ff3 = a; ff4 = a;
                    break;
                case 2: // 2 - 후진
                    a = -(float)Math.Round(fSpeed);
                    ff1 = a; ff2 = a; ff3 = a; ff4 = a;
                    break;
                case 3: // 3 - 좌턴(제자리)
                    a = (float)Math.Round(fSpeed);
                    ff1 = a; ff3 = a;
                    ff2 = -a; ff4 = -a;
                    break;
                case 4: // 4 - 우턴(제자리)
                    a = -(float)Math.Round(fSpeed);
                    ff1 = a; ff3 = a;
                    ff2 = -a; ff4 = -a;
                    break;
                case 5: // 5 - 좌턴(전진하면서)
                    a = (float)Math.Round(fSpeed);
                    ff1 = a * fRatio_Turn; ff3 = a * fRatio_Turn;
                    ff2 = a * fRatio_Org; ff4 = a * fRatio_Org;
                    break;
                case 6: // 6 - 우턴(전진하면서)
                    a = (float)Math.Round(fSpeed);
                    ff2 = a * fRatio_Turn; ff4 = a * fRatio_Turn;
                    ff1 = a * fRatio_Org; ff3 = a * fRatio_Org;
                    break;
                case 7: // 7 - 좌턴(후진하면서), 8 - 우턴(후진하면서)
                    a = -(float)Math.Round(fSpeed);
                    ff1 = a * fRatio_Turn; ff3 = a * fRatio_Turn;
                    ff2 = a * fRatio_Org; ff4 = a * fRatio_Org;
                    break;
                case 8: // 8 - 우턴(후진하면서)
                    a = -(float)Math.Round(fSpeed);
                    ff2 = a * fRatio_Turn; ff4 = a * fRatio_Turn;
                    ff1 = a * fRatio_Org; ff3 = a * fRatio_Org;
                    break;
                default: // 0 - 정지
                    ff1 = 0.0f;
                    ff2 = 0.0f;
                    ff3 = 0.0f;
                    ff4 = 0.0f;
                    break;
            }
            f1 = (float)Math.Round(ff1);
            f2 = (float)Math.Round(ff2);
            f3 = (float)Math.Round(ff3);
            f4 = (float)Math.Round(ff4);
        }
        private void btnWheel4_0_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(5, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }
        private void btnWheel4_1_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(1, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_2_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(6, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_3_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(3, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_4_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(0, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_5_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(4, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_6_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(7, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_7_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(2, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_8_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(8, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_9_Click(object sender, EventArgs e)
        {

        }

        private void btnWheel4_10_Click(object sender, EventArgs e)
        {

        }
        #endregion For Wheel

        #region For Genie
        private void btnGenie_0_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(5, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_1_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(1, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_2_Click(object sender, EventArgs e)
        {

            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(6, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_3_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(3, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_4_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(0, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_5_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(4, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_6_Click(object sender, EventArgs e)
        {

            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(7, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_7_Click(object sender, EventArgs e)
        {

            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(2, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_8_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(8, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_9_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(9, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_10_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(10, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void CheckGenieDir_Display(float f1, float f2, float f3)
        {
            int nDir = CheckGenieDir(f1, f2, f3);
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 우회전, 10 - 좌회전 
            switch (nDir)
            {
                case 1:
                    lbGenieDir.Text = "↑";
                    break;
                case 2:
                    lbGenieDir.Text = "↓";
                    break;
                case 3:
                    lbGenieDir.Text = "←";
                    break;
                case 4:
                    lbGenieDir.Text = "→";
                    break;
                case 5:
                    lbGenieDir.Text = "↖";
                    break;
                case 6:
                    lbGenieDir.Text = "↗";
                    break;
                case 7:
                    lbGenieDir.Text = "↙";
                    break;
                case 8:
                    lbGenieDir.Text = "↘";
                    break;
                case 9:
                    lbGenieDir.Text = "tL";
                    break;
                case 10:
                    lbGenieDir.Text = "tR";
                    break;
                default:
                    lbGenieDir.Text = "□";
                    break;
            }
        }

        private int CheckGenieDir(float f1, float f2, float f3)
        {
            int nDir = 0;
            float dist1, fVal, fMin = 0;
            if ((f1 * f1 + f2 * f2 + f3 * f3) != 0) // 정지가 아니라면
            {
                float cf1, cf2, cf3;
                for (int i = 1; i < 11; i++)
                {
                    CalcGenieDir(i, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out cf1, out cf2, out cf3);
                    dist1 = (f1 - cf1) * (f1 - cf1) + (f2 - cf2) * (f2 - cf2) + (f3 - cf3) * (f3 - cf3);
                    fVal = (float)Math.Abs(dist1);
                    if ((nDir == 0) || (fVal < fMin))
                    {
                        nDir = i;
                        fMin = fVal;
                    }
                }
            }
            return nDir;
        }

        // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
        private void CalcGenieDir(int nDir, float fSpeed, out float f1, out float f2, out float f3)
        {
            float a;
            float ff1 = 0.0f, ff2 = 0.0f, ff3 = 0.0f;
            switch (nDir)
            {
                case 1: // 1 - 전진
                    // F1 = -F2, F1 < 0, F3 = 0
                    a = (float)Ojw.CMath.Cos(60.0f) * fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = -a;
                    ff2 = a;
                    ff3 = 0.0f;
                    break;
                case 2: // 2 - 후진
                    a = (float)Ojw.CMath.Cos(60) * fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = a;
                    ff2 = -a;
                    ff3 = 0.0f;
                    break;
                case 3: // 3 - 좌
                    a = (float)Ojw.CMath.Cos(60) * (-fSpeed);
                    a = (float)Math.Round(a);
                    ff1 = a;
                    ff2 = a;
                    ff3 = (-2.0f) * (a);
                    break;
                case 4: // 4 - 우
                    a = (float)Ojw.CMath.Cos(60) * (fSpeed);
                    a = (float)Math.Round(a);
                    ff1 = a;
                    ff2 = a;
                    ff3 = (-2.0f) * (a);
                    break;
                case 5: // 5 - 좌상
                    a = (float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = -a * ((float)Math.Sqrt(3.0f) + 3.0f) / (3.0f * (float)Math.Sqrt(3.0f));
                    ff2 = -a * ((float)Math.Sqrt(3.0f) - 3.0f) / (3.0f * (float)Math.Sqrt(3.0f));
                    ff3 = a * 2.0f / 3.0f;
                    break;
                case 6: // 6 - 우상
                    a = (float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = a * ((float)Math.Sqrt(3.0f) - 3.0f) / (3.0f * (float)Math.Sqrt(3.0f));
                    ff2 = a * ((float)Math.Sqrt(3.0f) + 3.0f) / (3.0f * (float)Math.Sqrt(3.0f));
                    ff3 = a * (-2.0f) / 3.0f;
                    break;
                case 7: // 7 - 좌하
                    a = (float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = -(a * ((float)Math.Sqrt(3.0f) - 3.0f) / (3.0f * (float)Math.Sqrt(3.0f)));
                    ff2 = -(a * ((float)Math.Sqrt(3.0f) + 3.0f) / (3.0f * (float)Math.Sqrt(3.0f)));
                    ff3 = (-a * (-2.0f) / 3.0f);
                    break;
                case 8: // 8 - 우하
                    a = (float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = -(-a * ((float)Math.Sqrt(3.0f) + 3.0f) / (3.0f * (float)Math.Sqrt(3.0f)));
                    ff2 = -(-a * ((float)Math.Sqrt(3.0f) - 3.0f) / (3.0f * (float)Math.Sqrt(3.0f)));
                    ff3 = (-a * 2.0f / 3.0f);
                    break;
                case 9: // 9 - 좌회전
                    a = -(float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = a;
                    ff2 = a;
                    ff3 = a;
                    break;
                case 10: // 10 - 우회전 
                    a = (float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = a;
                    ff2 = a;
                    ff3 = a;
                    break;
                default: // 0 - 정지
                    ff1 = 0.0f;
                    ff2 = 0.0f;
                    ff3 = 0.0f;
                    break;
            }
            f1 = (float)Math.Round(ff1);
            f2 = (float)Math.Round(ff2);
            f3 = (float)Math.Round(ff3);
        }
        #endregion For Genie

        private void btnSimul_Click(object sender, EventArgs e)
        {
            Ojw.CTimer.Reset();
            m_C3d.SetSimulation_Smooth(chkSmooth.Checked);
            m_C3d.SetSimulation_With_PlayFrame(true);
            tmrRun.Enabled = true;
        }

        private void chkSmooth_CheckedChanged(object sender, EventArgs e)
        {
            m_C3d.SetSimulation_Smooth(chkSmooth.Checked);
        }

        private void frmMotionEditor_DragDrop(object sender, DragEventArgs e)
        {
            string[] file_name_array = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            int nCnt_Ojw = 0;
            int nCnt_Dmt = 0;
            int nCnt_Media = 0;
            foreach (string strItem in file_name_array)
            {
                #region Media File
                if (nCnt_Media == 0)
                {
                    if ((CheckMovieExist(strItem) == true) || (CheckAudioExist(strItem) == true))
                    {
                        lbMp3File.Text = Ojw.CFile.GetTitle(strItem);
                        String strExe = Ojw.CFile.GetExe(strItem);

                        if ((strExe.ToUpper() != "MP3") && (strExe.ToUpper() != "WAV")) m_bScreen = true;
                        else m_bScreen = false;
                        InitMp3();

                        mpPlayer.URL = strItem;
                        mpPlayer.settings.volume = 0;
                        mpPlayer.Ctlcontrols.play();
                        Ojw.CTimer CTmr = new Ojw.CTimer();
                        CTmr.Set();
                        //while (m_dMp3_Max <= 0)
                        while (true)
                        {
                            if (mpPlayer.Ctlcontrols.currentItem != null)
                            {
                                if (mpPlayer.Ctlcontrols.currentItem.duration > 0)
                                    break;
                            }
                            if (CTmr.Get() > 5000) break;
                            Application.DoEvents();
                        }
                        mpPlayer.Ctlcontrols.stop();
                        mpPlayer.settings.volume = 100;
                        chkMp3.Checked = true;

                        m_strWorkDirectory_Mp3 = Ojw.CFile.GetPath(strItem);
                        if (m_strWorkDirectory_Mp3 == null) m_strWorkDirectory_Mp3 = Application.StartupPath;

                        nCnt_Media++;
                    }
                }
                #endregion Media File
                #region Design File
                if (nCnt_Ojw == 0)
                {
                    if (strItem.ToLower().IndexOf(".ojw") > 0)
                    {
                        if (m_C3d.FileOpen(strItem) == true) // 모델링 파일이 잘 로드 되었다면 
                        {
                            Ojw.CMessage.Write("3d Modeling File Opened");

                            float[] afData = new float[3];
                            m_C3d.GetPos_Display(out afData[0], out afData[1], out afData[2]);
                            m_C3d.GetAngle_Display(out afData[0], out afData[1], out afData[2]);

                            m_C3d.m_strDesignerFilePath = Ojw.CFile.GetPath(strItem);
                            if (m_C3d.m_strDesignerFilePath == null) m_C3d.m_strDesignerFilePath = Application.StartupPath;

                            // File Restore
                            //m_C3d.FileRestore();


                            nCnt_Ojw++;
                        }
                    }
                }
                #endregion Design File
                #region DMT Motion File
                if (nCnt_Dmt == 0)
                {
                    if (strItem.ToLower().IndexOf(".dmt") > 0) // 논리적으로 0이 나올수가 없음.
                    {
                        if (m_bModify == true)
                        {
                            DialogResult dlgRet = MessageBox.Show("문서를 저장하지 않고 다른파일을 열면 데이터가 사라집니다.\r\n\r\n그래도 파일을 여시겠습니까?", "파일열기", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            if (dlgRet != DialogResult.OK)
                                return;
                        }
                        
                        txtFileName.Text = strItem;
                        if (m_C3d.DataFileOpen(strItem, null) == false)
                        {
                            MessageBox.Show("Dmt 모션 파일이 아닙니다.");
                        }
                        else
                        {
                            Modify(false);
                            Grid_DisplayTime();

                            txtTableName.Text = m_C3d.GetMotionFile_Title();
                            txtComment.Text = m_C3d.GetMotionFile_Comment();
                            cmbStartPosition.SelectedIndex = m_C3d.GetMotionFile_StartPosition();

                            m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(strItem);
                            if (m_strWorkDirectory_Dmt == null) m_strWorkDirectory_Dmt = Application.StartupPath;
                            
                            nCnt_Dmt++;
                        }
                    }
                }
                #endregion DMT Motion File
            }
        }

        private void frmMotionEditor_DragEnter(object sender, DragEventArgs e) { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; }
                
    }
}
