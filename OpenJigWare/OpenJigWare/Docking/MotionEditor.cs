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

#if _ENABLE_MEDIAPLAYER
//
#if false
도구 -> 도구상자 항목선택 -> COM 구성요소 -> Windows Media Player   : wmp.dll
Tools-> Choose Item -> Com -> Windows media player    : wmp.dll
#endif

#endif

namespace OpenJigWare.Docking
{
    public partial class frmMotionEditor : Form
    {
        public frmMotionEditor()
        {
            InitializeComponent();
#if _ENABLE_MEDIAPLAYER

#endif
        }
        
        //private Ojw.COjwMotor m_CMotor = new Ojw.COjwMotor();
        
        private Ojw.C3d m_C3d = new Ojw.C3d();
        private bool m_bLoadedForm = false;
        private void frmMotionEditor_Load(object sender, EventArgs e)
        {
            

            Ojw.CTimer.Init(100);
            // 이것만 선언하면 기본 선언은 끝.
            m_C3d.Init(picDisp);
            // 캐드파일의 경로를 임의의 경로로 바꾼다. -> 안 정하면 실행파일과 같은 경로
            //m_C3d.SetAseFile_Path("Cad");

            Ojw.CMessage.Init(txtMessage);

            // property window
            m_C3d.CreateProb_VirtualObject(pnProperty);

            //if (m_C3d.FileOpen(@"16dof_ecoHead.ojw") == true) // 모델링 파일이 잘 로드 되었다면 
            //if (m_C3d.FileOpen(@"robolink-spider.ojw") == true) // 모델링 파일이 잘 로드 되었다면 
            //{
            Ojw.CMessage.Write("File Opened");
            //}

            // 그림을 그리기 위한 timer 가동
            tmrDraw.Enabled = true;

            // 설정된 마우스 이벤트를 사용할 것인지...(기본은 사용하도록 되어 있다. User의 Function만 사용하길 원한다면 false)
            //m_C3d.SetMouseEventEnable(true); // you can remove the default mouse events.

            // Add User Function
            m_C3d.AddMouseEvent_Down(OjwMouseDown);
            m_C3d.AddMouseEvent_Move(OjwMouseMove);
            m_C3d.AddMouseEvent_Up(OjwMouseUp);
            m_C3d.Prop_Set_Main_MouseControlMode(0);
            m_C3d.GridMotionEditor_Init(dgAngle, 40, 999);
            m_C3d.GridMotionEditor_Init_Panel(pnButton);
            //m_C3d.GridDraw_Init(dgAngle, 40);
            //InitGridView

            Ojw.CMessage.Write("Ready");
            //CheckForIllegalCrossThreadCalls = false;

            m_bLoadedForm = true;
        }

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
            if (m_bTimer == true) return;
            m_bTimer = true;

            OjwDraw();
            if (m_bRequestPick == true)
            {
                m_bRequestPick = false;

                ShowData(m_bPick);
            }
#if false
            if ((Ojw.CTimer.Timer(frmMain.TID_FILEBACKUP) > 300000) && (m_bModelOpened == true) && (m_bStart == false)) // 5분에 한번 저장
            {
                int nVer = ((chkFileVersionForSave.Checked == true) ? _V_11 : ((chkFileVersionForSave_1_0.Checked == true) ? _V_10 : _V_12));
                BinaryFileSave(nVer, Ojw.C3d._STR_BACKUP_FILE, false);

                Ojw.CTimer.TimerSet(frmMain.TID_FILEBACKUP);
            }
#endif

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
            DialogResult dlgRet = MessageBox.Show("프로그램을 종료합니다.\r\n\r\n계속 하시겠습니까?", "Program 종료", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (dlgRet != DialogResult.OK)
            {
                return false;
            }
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

            return true;
        }
        private void frmMotionEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_bProgEnd = true;
            bool bRet = ProgEnd();
            if (bRet == false)
            {
                e.Cancel = true;
                return;
            }

            // 정상적인 프로그램 종료시 백업한 파일 지우기
            FileInfo fileBack = new FileInfo(Ojw.C3d._STR_BACKUP_FILE);
            if (fileBack.Exists) // 백업할 파일이 있는지 체크
            {
                // 없더라도 에러가 나지는 않는다. 굳이 에러처리가 필요 없음.
                fileBack.Delete();
            }


            tmrMp3.Enabled = false;
            tmrDraw.Enabled = false;


            Ojw.CTimer.Wait(1000); // 백그라운드 작업이 다 완료될때까지 기다리는 편이 좋다.


            // Serial
            if (m_C3d.m_CMotor.IsConnect() == true)
                Disconnect();
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
            ///////////////////////////////////////////

            StartMotion();

            ///////////////////////////////////////////
            btnRun.Enabled = true;
        }
        private void StartMotion()
        {

            if (m_bStart == true)
            {
                lbMotion_Message.Text = "Motion 운전 중입니다.";
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
                lbMotion_Message.Text = "비상정지 알람이 켜져 있습니다.";
                return;
            }
            if (m_C3d.m_CMotor.IsConnect() == false)// && (frmMain.m_DrBluetooth.drbluetooth_client_connected() == false))
            {
                lbMotion_Message.Text = "Serial Port Error - Not Connected.";
                return;
            }

            if (chkMp3.Checked == true)
            {
                // 일단 그리드의 타임값 등을 디스플레이 한다.
                Grid_DisplayTime();
                int nCell = m_C3d.m_CGridMotionEditor.m_nCurrntCell;
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
            //COjwTimer.TimerSet(frmMain.TID_FILEBACKUP);
            //COjwTimer.TimerDestroy(frmMain.TID_FILEBACKUP);

            // 타이머 기능 정지
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
                    m_nMotion_Step = Motion(m_nMotion_Step);
                }
            }
            //if ((m_bMotionEnd == true) && (m_bMp3Play == true)) Mp3Stop();
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
            //COjwTimer.TimerSet(frmMain.TID_FILEBACKUP);


            // 그리드 클릭 및 기타 이벤트 금지 해제
            dgAngle.Enabled = true;

            //m_C3d.m_CMotor.SetAutoReturn(true);
        }
        private int m_nMotion_Step = 0;
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
            m_C3d.m_CMotor.ResetStop();
            // Servo / Driver On
            m_C3d.m_CMotor.DrvSrv(true, true);

            //if (CheckWifi() == true)
            //{
            //    m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_reset_stop();
            //    m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_drvsrv(true, true);
            //}
            //frmMain.m_DrBluetooth.drbluetooth_set_id(frmMain.m_pnBluetoothAddress[m_nCurrentRobot]);
            //frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_reset_stop();
            //frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_drvsrv(true, true);

            if (chkMp3.Checked == true)
            {
#if _ENABLE_MEDIAPLAYER
                Mp3Play();
#endif
                Ojw.CTimer.Wait(Ojw.CConvert.StrToLong(txtMp3TimeDelay.Text));
            }

            int nFirstLine = ((nTmpCnt == 1) && (chkMp3.Checked == true)) ? nTmpPos : 0; // 다중선택이면서 음원과 싱크를 맞춰 출력하려면...

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
                                    (m_C3d.m_CMotor.IsConnect() == false) ||
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
                    (m_C3d.m_CMotor.IsConnect() == false) ||
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
        public const int TID_MOTION_BY_TIMER = 96;
        public const int TID_MOTIONS = 76; // 76 ~ 95
        public const int TID_MOTIONS_WAIT_TICK = 56; // 56 ~ 75
        public const int TID_SYNC = 36; // 36 ~ 55
        public const int TID_FILEBACKUP = 35;
        public const int TID_MOTION2 = 34;
        #endregion Timer ID - TID

        private void WaitAction_SetTimer()
        {
            //m_lWaitActionTimer = 0;
            //Ojw.CTimer.Set(TID_MOTION_BY_TIMER);
            m_C3d.WaitAction_SetTimer();
            return;
        }
        private bool WaitAction_ByTimer(long t)
        {
            return m_C3d.WaitAction_ByTimer(t);
            //if (t <= 0) return true;	// t 값이 0 보다 작다면 대기문이 필요없으므로 완료를 보냄.
            //m_lWaitActionTimer += t;

            //while (
            //        (Ojw.CTimer.Check(TID_MOTION_BY_TIMER, m_lWaitActionTimer) == false) && (m_bMotionEnd == false)// && (m_bEms == false)// && (m_bPause == FALSE) && 
            //    //(g_bMainRun_Action)
            //    )
            //{
            //    Application.DoEvents();
            //}
            //return true;
        }
        private void PlayFrame(int nFrameNum)
        {
            m_C3d.PlayFrame(nFrameNum, 0);
        }
        
        private void btnStop_Click(object sender, EventArgs e)
        {
            //m_bStop = true;
            m_C3d.m_CMotor.Stop();
            m_bMotionEnd = true;
            Ojw.CTimer.Stop();
        }

        private void OjwDraw()
        {
            m_C3d.OjwDraw(out m_nGroupA, out m_nGroupB, out m_nGroupC, out m_nKinematicsNumber, out m_bPick, out m_bLimit);
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

        private AxWMPLib.AxWindowsMediaPlayer mpPlayer;
        private void chkMp3_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMp3.Checked == true)
            {
                // 일단 그리드의 타임값 등을 디스플레이 한다.
                Grid_DisplayTime();
                int nCell = m_C3d.m_CGridMotionEditor.m_nCurrntCell;
                if ((nCell >= 0) && (dgAngle.RowCount > nCell))
                {
                    //prgMp3.Value = (int)(mpPlayer.Ctlcontrols.currentPosition / dTime * 100);
#if _ENABLE_MEDIAPLAYER
                    mpPlayer.Ctlcontrols.currentPosition = (double)m_lCalcTime[nCell] / 1000.0;
#endif
                }
            }
        }
        public string[] m_strCalcTime = new string[10000];
        public long[] m_lCalcTime = new long[10000];
        private void Grid_DisplayTime()
        {
            Grid_CalcTimer();
            for (int i = 0; i < dgAngle.RowCount; i++)
            {
                if (Convert.ToString(dgAngle.Rows[i].Cells[dgAngle.ColumnCount - 2].Value) != m_strCalcTime[i])
                {
                    dgAngle.Rows[i].Cells[dgAngle.ColumnCount - 2].Value = m_strCalcTime[i];
                }
            }
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
            tmrRun.Enabled = false;

            if (m_btmrRun == true) return;
            m_btmrRun = true;

            Run();

            m_btmrRun = false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            m_nMotion_Step = 0;
            m_C3d.m_CMotor.Reset();
        }

        private void btnMotionEnd_Click(object sender, EventArgs e)
        {
            m_bMotionEnd = true;
        }

        private void Ems()
        {
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

        private void btnTextSave_Click(object sender, EventArgs e)
        {

        }
        // 시간값 계산

    }
}
