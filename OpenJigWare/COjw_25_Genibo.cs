using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.IO.Ports;
using System.IO;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CGenibo_Bt
        {
            #region 생성자/소멸자
        public CGenibo_Bt()
        {
            m_SerialPort = new SerialPort();
            m_strOrgPath = Directory.GetCurrentDirectory();
        }
        ~CGenibo_Bt()
        {
            disconnect();
        }
        #endregion 생성자/소멸자

        #region 선언

        public const int _RET_RECEIVE_OK = 0;
        public const int _RET_PROCESS_END = 1;
        public const int _RET_PROCESS_FAILED = 2;
        public const int _RET_CHECKSUM_ERROR = 3;
        public const int _RET_TIME_OUT_ERROR = 4;
        public const int _RET_LENGTH_ERROR = 5;

        public const int _SIZE_FILENAME = 51;

        private const int _CODE_BLUETOOTH = 0xb2; // 블루투스 코드넘버

        #endregion 선언

        #region 변수정의
        private SerialPort m_SerialPort = null;
        private bool m_bEventInit = false;
        private Thread Reader;                  // 읽기 쓰레드

        private int m_nFileList_master = 0;                // 파일갯수
        private byte[] m_pbyteFileList_master = null;      // 파일이름
        private int m_nFileList_user = 0;                // 파일갯수
        private byte[] m_pbyteFileList_user = null;      // 파일이름

        private int m_nClient_Seq = 0;
        private String m_strOrgPath = "";

        //private bool m_bThread_Client = false;
        //private TcpClient m_tcpClient = null;      // Client
        //private Thread m_thClient;          // Thread
        private int m_nProcess_Client = 0;
        //private bool m_bClientAuth = false;

        //private NetworkStream m_streamClient;            // 네트워크 스트림
        //private BinaryWriter m_bwClient_outData;
        //private BinaryReader m_bwClient_inData;
        //private int m_nClientId = 0xC0;                   //Packet ID;
        private int m_nTimeSec = 0;                      //읽어들인 서버 시간(sec)
        private int m_nTimeUSec = 0;                     //읽어들인 서버 시간(micro sec)
        private int m_nRobotBatt = 0;                    //읽어들인 로봇 배터리(%)
        private int m_nMIDBatt = 0;                      //읽어들인 MID 배터리(%)
        private int m_nRmcLength = 0;                    //읽어들인 리모컨 길이(0.125s/tick)
        private int m_nRmcData = 0;                      //읽어들인 리모컨 데이터
        private bool m_bPlaying = false;                 //읽어들인 task/motion 실행 여부
        #endregion 변수정의



        #region 축과 ID 설정
            #region [SMot_t]관절 파라미터 구조체 선언
            public struct SMot_t
            {
                public bool bEn;

                public int nDir;
                //중심위치
                public int nCenterPos;

                // 기어비
                public float fMechMove;
                public float fDegree;

                public float fLimitUp;    // 관절 리미트 - 0 무효
                public float fLimitDn;    // 관절 리미트 - 0 무효

                public int nID;
                public int nPos;
                public int nTime;

                public int nFlag; // 76[543210] NoAction(5), Red(4), Blue(3), Green(2), Mode(    
            }
            #endregion [SMot_t]관절 파라미터 구조체 선언

        private SMot_t[] m_pSMot = new SMot_t[256];
        private int[] m_pnAxis_By_ID = new int[256]; // ID = 0 ~ 254 까지 가능, 단, 254 는 BroadCasting - 에러방지를 위해서는 가용 수치까지 전부 잡는다.
        private void SetAxis_By_ID(int nID, int nAxis) { m_pnAxis_By_ID[nID] = nAxis; }
        private int GetAxis_By_ID(int nID) { return (nID == 0xfe) ? 0xfe : m_pnAxis_By_ID[nID]; }
        private int GetID_By_Axis(int nAxis) { return (nAxis == 0xfe) ? 0xfe : m_pSMot[nAxis].nID; }
        public void set_param(int nAxis, int nID, int nDir, float fLimitUp, float fLimitDn, int nCenterPos, float fMechMove, float fDegree)
        {
            m_pSMot[nAxis].nID = nID;
            m_pSMot[nAxis].nDir = nDir;
            m_pSMot[nAxis].nCenterPos = nCenterPos;
            m_pSMot[nAxis].fMechMove = fMechMove;
            m_pSMot[nAxis].fDegree = fDegree;

            m_pSMot[nAxis].fLimitUp = fLimitUp;
            m_pSMot[nAxis].fLimitDn = fLimitDn;

            SetAxis_By_ID(nID, nAxis);
        }
        #endregion
        
        
            #region Calc - 계산함수(시간, 각도)
            // ms 의 값을 Raw 데이타(패킷용)으로 변환
            private int CalcTime_ms(int nTime)
            {
                // 1 Tick 당 11.2 ms => 1:11.2=x:nTime => x = nTime / 11.2
                return (int)Math.Round((float)nTime / 11.2f);
            }
            private int CalcAngle2Evd(int nAxis, float fValue)
            {
                try
                {
                    fValue *= ((m_pSMot[nAxis].nDir == 0) ? 1.0f : -1.0f);
                    int nData = 0;
                    if (get_cmd_flag_mode(nAxis) != 0)   // 속도제어
                    {
                        nData = (int)Math.Round(fValue);
                    }
                    else
                    {
                        // 위치제어
                        nData = (int)Math.Round((m_pSMot[nAxis].fMechMove * fValue) / m_pSMot[nAxis].fDegree);
                        nData = nData + m_pSMot[nAxis].nCenterPos;
                    }

                    return nData;
                    //return (nData + _CENTER_POS);
                }
                catch
                {
                    return 0;
                }
            }
            private float CalcEvd2Angle(int nAxis, int nValue)
            {
                try
                {
                    float fValue = ((m_pSMot[nAxis].nDir == 0) ? 1.0f : -1.0f);
                    // 1024:333.3 = pulse:angle
                    float fValue2 = 0.0f;
                    if (get_cmd_flag_mode(nAxis) != 0)   // 속도제어
                        fValue2 = (float)nValue * fValue;
                    else                                // 위치제어
                        fValue2 = (float)(((m_pSMot[nAxis].fDegree * ((float)(nValue - m_pSMot[nAxis].nCenterPos))) / m_pSMot[nAxis].fMechMove) * fValue);
                    //float fValue2 = (float)(((m_pSMot[nAxis].fDegree * ((float)(nValue - m_pSMot[nAxis].nCenterPos))) / m_pSMot[nAxis].fMechMove) * fValue);
                    return fValue2;// 마지막에 부호변수를 곱함
                    //return (float)(((m_pSMot[nAxis].fDegree * ((float)(nValue - _CENTER_POS))) / m_pSMot[nAxis].fMechMove) * fValue);// 마지막에 부호변수를 곱함                
                }
                catch
                {
                    return 0.0f;
                }
            }
            #endregion

            #region Limit
            private bool m_bIgnoredLimit = false;
            // 현재로서는 외부공개를 안하나 언젠가 하게 되면 이름을 공통사용하는 이름으로 바꾸도록 한다.
            private void SetLimitEn(bool bOn) { m_bIgnoredLimit = !bOn; }
            private bool GetLimitEn() { return !m_bIgnoredLimit; }

            private int Clip(int nLimitValue_Up, int nLimitValue_Dn, int nData)
            {
                if (GetLimitEn() == false) return nData;

                int nRet = ((nData > nLimitValue_Up) ? nLimitValue_Up : nData);
                //nRet = ((nRet < nLimitValue_Dn) ? nLimitValue_Dn : nRet);
                //return nRet;
                return ((nRet < nLimitValue_Dn) ? nLimitValue_Dn : nRet);
            }
            private float Clip(float fLimitValue_Up, float fLimitValue_Dn, float fData)
            {
                if (GetLimitEn() == false) return fData;
                float fRet = ((fData > fLimitValue_Up) ? fLimitValue_Up : fData);
                //fRet = ((fRet < fLimitValue_Dn) ? fLimitValue_Dn : fRet);
                //return fRet;
                return ((fRet < fLimitValue_Dn) ? fLimitValue_Dn : fRet);
            }
            private int CalcLimit_Evd(int nAxis, int nValue)
            {
                if ((get_cmd_flag_mode(nAxis) == 0) || (get_cmd_flag_mode(nAxis) == 2))
                {
                    int nPulse = nValue & 0x4000;
                    nValue &= 0x3fff;
                    int nUp = 100000;
                    int nDn = -nUp;
                    if (m_pSMot[nAxis].fLimitUp != 0) nUp = CalcAngle2Evd(nAxis, m_pSMot[nAxis].fLimitUp);
                    if (m_pSMot[nAxis].fLimitDn != 0) nDn = CalcAngle2Evd(nAxis, m_pSMot[nAxis].fLimitDn);
                    if (nUp < nDn) { int nTmp = nUp; nUp = nDn; nDn = nTmp; }
                    return (Clip(nUp, nDn, nValue) | nPulse);
                }
                return nValue;
            }
            private float CalcLimit_Angle(int nAxis, float fValue)
            {
                if ((get_cmd_flag_mode(nAxis) == 0) || (get_cmd_flag_mode(nAxis) == 2))
                {
                    float fUp = 100000.0f;
                    float fDn = -fUp;
                    if (m_pSMot[nAxis].fLimitUp != 0) fUp = m_pSMot[nAxis].fLimitUp;
                    if (m_pSMot[nAxis].fLimitDn != 0) fDn = m_pSMot[nAxis].fLimitDn;
                    return Clip(fUp, fDn, fValue);
                }
                return fValue;
            }
            #endregion

        #region Cmd 함수
        private int m_nMotor_Max = 30;
        public void     set_max(int nMotorMax) { m_nMotor_Max = nMotorMax; }
        public int      get_max() { return m_nMotor_Max; }
        public void     InitCmd() { for (int i = 0; i < m_nMotor_Max; i++) { m_pSMot[i].bEn = false; set_cmd_flag(i, false, false, false, false, false, true); } }
        public void     set_cmd(int nAxis, int nPos) { m_pSMot[nAxis].bEn = true; m_pSMot[nAxis].nPos = nPos; set_cmd_flag_no_action(nAxis, false); }
        public void     set_cmd_angle(int nAxis, float fAngle) { m_pSMot[nAxis].bEn = true; m_pSMot[nAxis].nPos = CalcLimit_Evd(nAxis, CalcAngle2Evd(nAxis, fAngle)); set_cmd_flag_no_action(nAxis, false); }
        public int      get_cmd(int nAxis) { return m_pSMot[nAxis].nPos; }
        public float    get_cmd_angle(int nAxis) { return CalcEvd2Angle(nAxis, m_pSMot[nAxis].nPos); }
        public void     set_cmd_flag(int nAxis, int nFlag) { m_pSMot[nAxis].nFlag = nFlag; }
        public void     set_cmd_flag(int nAxis, bool bStop, bool bMode_Speed, bool bLed_Green, bool bLed_Blue, bool bLed_Red, bool bNoAction) { m_pSMot[nAxis].nFlag = ((bStop == true) ? _FLAG_STOP : 0) | ((bMode_Speed == true) ? _FLAG_MODE_SPEED : 0) | ((bLed_Green == true) ? _FLAG_LED_GREEN : 0) | ((bLed_Blue == true) ? _FLAG_LED_BLUE : 0) | ((bLed_Red == true) ? _FLAG_LED_RED : 0) | ((bNoAction == true) ? _FLAG_NO_ACTION : 0); }
        public int      get_cmd_flag(int nAxis) { return m_pSMot[nAxis].nFlag; }
        public void     set_cmd_flag_stop(int nAxis, bool bStop) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xfe) | ((bStop == true) ? _FLAG_STOP : 0); }
        public void     set_cmd_flag_mode(int nAxis, bool bMode_Speed) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xfd) | ((bMode_Speed == true) ? _FLAG_MODE_SPEED : 0); }
        public void     set_cmd_flag_led(int nAxis, bool bGreen, bool bBlue, bool bRed) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xe3) | ((bGreen == true) ? _FLAG_LED_GREEN : 0) | ((bBlue == true) ? _FLAG_LED_BLUE : 0) | ((bRed == true) ? _FLAG_LED_RED : 0); }
        public void     set_cmd_flag_led_green(int nAxis, bool bGreen) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xfb) | ((bGreen == true) ? _FLAG_LED_GREEN : 0); }
        public void     set_cmd_flag_led_blue(int nAxis, bool bBlue) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xf7) | ((bBlue == true) ? _FLAG_LED_BLUE : 0); }
        public void     set_cmd_flag_led_red(int nAxis, bool bRed) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xef) | ((bRed == true) ? _FLAG_LED_RED : 0); }
        public void     set_cmd_flag_no_action(int nAxis, bool bNoAction) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xdf) | ((bNoAction == true) ? _FLAG_NO_ACTION : 0); }

        // 1111 1101
        public int get_cmd_flag_mode(int nAxis) { return (((m_pSMot[nAxis].nFlag & _FLAG_MODE_SPEED) != 0) ? 1 : 0); }

        public bool get_cmd_flag_led_green(int nAxis) { return (((m_pSMot[nAxis].nFlag & 0x04) != 0) ? true : false); }
        public bool get_cmd_flag_led_blue(int nAxis) { return (((m_pSMot[nAxis].nFlag & 0x08) != 0) ? true : false); }
        public bool get_cmd_flag_led_red(int nAxis) { return (((m_pSMot[nAxis].nFlag & 0x10) != 0) ? true : false); }
        #endregion Cmd 함수

        public bool request_move(int nTime)
        {
            if ((m_bStop == true) || (m_bEms == true)) return _RESULT_FAIL;
            int nID;
            int i = 0;
            ////////////////////////////////////////////////


            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                byte[] pbyteBuffer = new byte[1 + 4 * m_nMotor_Max];
                int nPos;
                int nFlag;

                #region S-Jog Time
                int nCalcTime = CalcTime_ms(nTime);
                pbyteBuffer[i++] = (byte)(nCalcTime & 0xff);
                #endregion S-Jog Time

                for (int nAxis = 0; nAxis < m_nMotor_Max; nAxis++)
                {
                    if (m_pSMot[nAxis].bEn == true)
                    {
                        //nPos |= _JOG_MODE_SPEED << 10;  // 속도제어 
                        #region Position
                        nPos = get_cmd(nAxis);
                        pbyteBuffer[i++] = (byte)(nPos & 0xff);
                        pbyteBuffer[i++] = (byte)((nPos >> 8) & 0xff);
                        #endregion

                        #region Set-Flag
                        nFlag = get_cmd_flag(nAxis);
                        pbyteBuffer[i++] = (byte)(nFlag & 0xff);
                        set_cmd_flag_no_action(nAxis, true); // 동작 후 모터 NoAction을 살려둔다.
                        #endregion Set-Flag

                        #region 모터당 아이디(후면에 붙는다)
                        nID = GetID_By_Axis(nAxis);
                        pbyteBuffer[i++] = (byte)(nID & 0xff);
                        #endregion 모터당 아이디(후면에 붙는다)
                        ////////////////////////////////////////////////
                    }
                    m_pSMot[nAxis].bEn = false;
                }
                Make_And_Send_Packet(0xfe, 0x06, i, pbyteBuffer);
                pbyteBuffer = null;
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_move(int nAxis, int nTime)
        {
            if ((m_bStop == true) || (m_bEms == true)) return _RESULT_FAIL;
            int nID;
            int i = 0;
            ////////////////////////////////////////////////


            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                byte[] pbyteBuffer = new byte[1 + 4 * m_nMotor_Max];
                int nPos;
                int nFlag;

                #region S-Jog Time
                int nCalcTime = CalcTime_ms(nTime);
                pbyteBuffer[i++] = (byte)(nCalcTime & 0xff);
                #endregion S-Jog Time

                if (m_pSMot[nAxis].bEn == true)
                {
                    //nPos |= _JOG_MODE_SPEED << 10;  // 속도제어 
                    #region Position
                    nPos = get_cmd(nAxis);
                    pbyteBuffer[i++] = (byte)(nPos & 0xff);
                    pbyteBuffer[i++] = (byte)((nPos >> 8) & 0xff);
                    #endregion

                    #region Set-Flag
                    nFlag = get_cmd_flag(nAxis);
                    pbyteBuffer[i++] = (byte)(nFlag & 0xff);
                    set_cmd_flag_no_action(nAxis, true); // 동작 후 모터 NoAction을 살려둔다.
                    #endregion Set-Flag

                    #region 모터당 아이디(후면에 붙는다)
                    nID = GetID_By_Axis(nAxis);
                    pbyteBuffer[i++] = (byte)(nID & 0xff);
                    #endregion 모터당 아이디(후면에 붙는다)
                    ////////////////////////////////////////////////
                }

                m_pSMot[nAxis].bEn = false;
                Make_And_Send_Packet(0xfe, 0x06, i, pbyteBuffer);
                pbyteBuffer = null;
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        #region Stop - 정지
        // 전체 정지 - 이걸로 멈추면 반드시 리셋이 필요(Stop 변수만 리셋하면 됨)
        public bool request_stop()
        {
            m_bStop = true;

            for (int i = 0; i < m_nMotor_Max; i++)
                set_cmd_flag_stop(i, true);
            return request_move(1000);
        }

        // 이건 그냥 멈추기만 할 뿐 변수를 셋하지는 않는다.
        public bool request_stop(int nAxis)
        {
            set_cmd_flag_stop(nAxis, true);
            return request_move(nAxis, 1000);
        }
        #endregion

        #region Ems - 비상정지
        public bool request_ems()
        {
            bool bRet = request_stop();
            drvsrv(false, false);
            m_bEms = true;
            return bRet;
        }
        #endregion

        #region Reset
        public bool request_reset(int nAxis)
        {
            if ((m_bStop == true) || (m_bEms == true)) return _RESULT_FAIL;
            //int nID;
            int i = 0;
            ////////////////////////////////////////////////


            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                if (nAxis < 0xfe)
                    set_cmd_flag(nAxis, 0);
                else
                {
                    for (int j = 0; j < m_nMotor_Max; j++) set_cmd_flag(j, 0);
                }

                int nDefaultSize = _CHECKSUM2 + 1;
                byte[] pbyteBuffer = new byte[nDefaultSize];

                // Header
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nAxis & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x09; // Reset

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize) & 0xff);

                Make_And_Send_Packet(0xfe, 0x06, i, pbyteBuffer);
                pbyteBuffer = null;
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        //public void Rollback(int nAxis, bool bIgnoreID, bool bIgnoreBaudrate)
        //{
        //    int i = 0;
        //    if (nAxis < 0xfe)
        //        SetCmd_Flag(nAxis, 0);
        //    else
        //    {
        //        for (i = 0; i < m_nMotor_Max; i++) SetCmd_Flag(i, 0);
        //    }
        //    int nID = GetID_By_Axis(nAxis);//m_pSMot[nAxis].nID;
        //    int nDefaultSize = _CHECKSUM2 + 1;
        //    byte[] pbyteBuffer = new byte[255];
        //    // Header
        //    pbyteBuffer[_HEADER1] = 0xff;
        //    pbyteBuffer[_HEADER2] = 0xff;
        //    // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
        //    pbyteBuffer[_ID] = (byte)(nID & 0xff);
        //    // Cmd
        //    pbyteBuffer[_CMD] = 0x08; // RollBack

        //    i = 0;
        //    /////////////////////////////////////////////////////
        //    // Data
        //    pbyteBuffer[nDefaultSize + i++] = (byte)((bIgnoreID == true) ? 1 : 0);
        //    ////////
        //    pbyteBuffer[nDefaultSize + i++] = (byte)((bIgnoreBaudrate == true) ? 1 : 0);
        //    ////////
        //    /////////////////////////////////////////////////////

        //    //Packet Size
        //    pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

        //    MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

        //    SendPacket(pbyteBuffer, nDefaultSize + i);
        //    pbyteBuffer = null;
        //}

        public void reset_stop() { m_bStop = false; for (int i = 0; i < m_nMotor_Max; i++) set_cmd_flag_stop(i, false); }
        public void reset_ems() { m_bEms = false; for (int i = 0; i < m_nMotor_Max; i++) set_cmd_flag_stop(i, false); }

        public void reset()
        {
            reset_stop();
            reset_ems();
            request_reset(0xfe);
        }
        #endregion
        
        #region 기본제어 변수(Stop, Ems, Reset)
        private bool m_bEms = false;
        private bool m_bStop = false;
        public bool check_Ems() { return m_bEms; }
        public bool check_stop() { return m_bStop; }
        #endregion 기본제어 변수(Stop, Ems, Reset)

        #region Counter - 통신에 관한 카운터 ( 보낸것, 받은 이벤트, Push 횟수, Pop 횟수 )

        #region 변수선언
        private const int _MOTOR_MAX = 256;
            private int m_nCntTick_Receive_Event_All = 0;
            private int m_nCntTick_Receive_PushBuffer = 0;
            private int m_nCntTick_Receive_GetBuffer = 0;
            public int GetCounter_Tick_Reveive_Event() { return m_nCntTick_Receive_Event_All; }
            public int GetCounter_Tick_Reveive_Push() { return m_nCntTick_Receive_PushBuffer; }
            public int GetCounter_Tick_Reveive_Interpret() { return m_nCntTick_Receive_GetBuffer; }

            private int[] m_pnCntTick_Send = new int[_MOTOR_MAX];
            private int[] m_pnCntTick_Receive = new int[_MOTOR_MAX];
            private int[] m_pnCntTick_Receive_Back = new int[_MOTOR_MAX];
            private Ojw.CTimer [] m_pCTimer = new Ojw.CTimer[_MOTOR_MAX];
            #endregion 변수선언

            #region Tick
            private long Tick_GetTimer(int nAxis) { return m_pCTimer[nAxis].Get(); }
            private void Tick_Send(int nAxis) { if (connected() == false) return; m_pnCntTick_Send[nAxis]++; m_pCTimer[nAxis].Set(); m_pnCntTick_Receive_Back[nAxis] = m_pnCntTick_Receive[nAxis]; }
            private void Tick_Receive(int nAxis) { m_pnCntTick_Receive[nAxis]++; m_pCTimer[nAxis].Set(); }
            public int GetCounter_Tick_Send(int nAxis) { return m_pnCntTick_Send[nAxis]; }
            public int GetCounter_Tick_Receive(int nAxis) { return m_pnCntTick_Receive[nAxis]; }
            #endregion Tick
        public void ResetCounter()
        {
            m_nCntTick_Receive_Event_All = m_nCntTick_Receive_PushBuffer = m_nCntTick_Receive_GetBuffer = 0;
            Array.Clear(m_pnCntTick_Send, 0, m_pnCntTick_Send.Length);
            Array.Clear(m_pnCntTick_Receive, 0, m_pnCntTick_Receive.Length);
            //Array.Clear(m_pnCntTick_Receive_Back, 0, m_pnCntTick_Receive_Back.Length);
            for (int i = 0; i < 256; i++) m_pnCntTick_Receive_Back[i] = -1;//
        }
        // 틱을 이용한 ...
        // 통신 장애 시간 체크
        public long GetCounter_Timer(int nAxis) { return Tick_GetTimer(nAxis); }
        //public bool IsReceived(int nAxis) { return ((m_pnCntTick_Receive[nAxis] != m_pnCntTick_Receive_Back[nAxis]) || (m_pCTimer[nAxis].Get() > 100)) ? true : false; }
        public bool IsReceived(int nAxis) { return (m_pnCntTick_Receive[nAxis] != m_pnCntTick_Receive_Back[nAxis]) ? true : false; }
        private Ojw.CTimer m_CTmr = new CTimer();
        public bool WaitReceive(int nAxis, long lWaitTimer)  // true : Receive Ok, false : Fail
        {
            m_CTmr.Set();
            bool bError = true;
            bool bOver = false;
            while ((connected() == true) && (m_bClassEnd == false) && (bOver == false))
            {
                if (IsReceived(nAxis) == true)
                {
                    bError = false;
                    break;
                }
                if (lWaitTimer > 0)
                {
                    if (m_CTmr.Get() >= lWaitTimer)
                    {
                        bOver = true;
                    }
                }
                //Thread.Sleep(1);
                Application.DoEvents();
            }
            return ((bError == false) ? true : false);
        }
        #endregion

        #region 버퍼관리 - 변수 - 함수
        private const int _CNT_BUFFER = 100;
        private int m_nCnt_ReceivedData = 0;
        private bool m_bReceived_AllData = true; // 데이타를 전부 받지 못한 경우 이게 false가 되어 다음 데이터를 받기를 기다림
        private String[] m_pstrReceivedData = new string[_CNT_BUFFER];// 완전한 형태의 받은 패킷 데이터
        private int m_nIndexReceiveData = 0; // 다음에 받아야 할 번지를 가리킴
        private bool m_bClassEnd = false;
        public int GetBuffer_Index_Last()
        {
            if (m_nCnt_ReceivedData > 0)
                return ((m_nIndexReceiveData > 0) ? (m_nIndexReceiveData - 1) : (_CNT_BUFFER - 1));
            return -1;
        }
        public int GetBuffer_Index_First()
        {
            if (m_nCnt_ReceivedData > 0)
            {
                return (_CNT_BUFFER + GetBuffer_Index_Last() - m_nCnt_ReceivedData + 1) % _CNT_BUFFER;
            }
            return -1;
        }
        // 0 -> first
        public int GetBuffer_Index(int nPos)
        {
            if ((m_nCnt_ReceivedData > 0) && (nPos < m_nCnt_ReceivedData) && (nPos >= 0))
            {
                return (_CNT_BUFFER + GetBuffer_Index_Last() - m_nCnt_ReceivedData + 1 + nPos) % _CNT_BUFFER;
            }
            return -1;
        }
        public int GetBuffer_Length()
        {
            try
            {
                return ((connected() == true) ? m_SerialPort.BytesToRead : 0);
            }
            catch
            {
                return 0;
            }
        }
        public int GetBuffer_OneData()
        {
            try
            {
                return ((connected() == true) ? m_SerialPort.ReadByte() : 0);
            }
            catch
            {
                return 0;
            }
        }
        private void PushBuffer(string strData)
        {
            m_pstrReceivedData[m_nIndexReceiveData++] = strData;
            if (m_nCnt_ReceivedData < _CNT_BUFFER) m_nCnt_ReceivedData++;
            if (m_nIndexReceiveData >= _CNT_BUFFER) m_nIndexReceiveData = 0;

            m_nCntTick_Receive_PushBuffer++; // 통신 이벤트 카운터 증가
        }
        public int GetBuffer_Size() { return m_nCnt_ReceivedData; }
        // 버퍼를 비우면서 꺼내온다.
        public String GetBuffer()
        {
            m_nCntTick_Receive_GetBuffer++; // 통신 이벤트 카운터 증가

            if (m_nCnt_ReceivedData > 0)
            {
                int nIndex = GetBuffer_Index_First();
                m_nCnt_ReceivedData--;
                return m_pstrReceivedData[nIndex];
            }

            return null;
        }
        // 버퍼를 비우지 않고 꺼내온다.
        public String GetBuffer_NoRemove(int nPos)
        {
            if (m_nCnt_ReceivedData > 0)
            {
                int nIndex = GetBuffer_Index(nPos);
                return m_pstrReceivedData[nIndex];
            }
            return null;
        }
        #endregion

        #region connected() - 접속상태 체크
        public bool connected() { if (m_SerialPort == null) return false; return m_SerialPort.IsOpen; }
        #endregion connected

        /////////////////////////////////////////////////
        // Parity - 0 : None, 1 : Odd, 2 : Even, 3 : Mark, 4 : Space
        // StopBit - 0 : None, 1 : One, 2 : Two, 3 : OnePointFive
        #region connect
        public bool connect(int nBluetoothSppPort, int nBaudRate)//(int nPort, int nBaudRate, int nParity, int nDataBits, int nStopBits)
        {
            try
            {
                if (connected() == false)
                {
                    ResetCounter(); // 카운터 클리어

                    if (m_bEventInit == false)
                    {
                        m_bEventInit = true;
                    }

                    //String strPort = "COM" + OjwConvert.IntToStr(nBluetoothSppPort);
                    //m_SerialPort = new SerialPort(strPort, nBaudRate, Parity.None, 8, StopBits.One);
                    m_SerialPort.PortName = "COM" + nBluetoothSppPort.ToString();
                    m_SerialPort.BaudRate = nBaudRate;
                    m_SerialPort.Parity = Parity.None;
                    m_SerialPort.DataBits = 8;
                    m_SerialPort.StopBits = StopBits.One;
                    m_SerialPort.ReceivedBytesThreshold = 1;
                    //m_SerialPort.ReadExisting
                    //m_SerialPort.ReadBufferSize = 256;
                    try
                    {
                        m_SerialPort.Open();

                        if (connected() == true)
                        {
                            m_bClassEnd = false;
                            Reader = new Thread(new ThreadStart(ReceiveDataCallback));
                            Reader.Start();
                        }

                    }
                    catch
                    {
                        //m_bConnect = false;
                        return _RESULT_FAIL;
                    }
                }
                return connected();
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        #endregion connect

        #region disconnect() - 접속 해제 - Thread 정지
        public void disconnect()
        {
            if (connected() == true)
            {
                m_bClassEnd = true;
                //Reader.Abort();
                m_SerialPort.Close();
            }
        }
        #endregion disconnect

        #region Bluetooth ID
        private int m_nRobotID = 0x00;
        public void drbluetooth_set_id(int nRobotID) { m_nRobotID = nRobotID; }
        public int drbluetooth_get_id() { return m_nRobotID; }
        #endregion Bluetooth ID

        #region Test 용
        private bool m_bServerOn = false;
        public void drbluetooth_set_param_servermode(bool bOn) { m_bServerOn = bOn; } // 이게 셋 되면 테스트 모드로 서버처럼 동작한다. - 테스트 용도외엔 쓸일 없는 함수
        public bool drbluetooth_get_param_servermode() { return m_bServerOn; }
        //private int m_nCode = _CODE_BLUETOOTH;
        //public void drbluetooth_set_code(int nCode) { m_nCode = nCode; }
        public int drbluetooth_get_code() { return _CODE_BLUETOOTH; }// m_nCode; }
        #endregion Test 용        
       
        private bool m_bBusy = false;
        // 통신의 혼선이 일지 않게 끔 외부적으로 Busy 상태인지 아닌지를 확인하는 함수
        public bool IsBusy() { return m_bBusy; }
        private bool[] m_pbRequest = new bool[_MOTOR_MAX];
        private int m_nRxIndex = 0xff;
        //private int m_nRxDataCount = 0;
        //private int m_nRxId = 0;
        //private int m_nRxCmd = 0;
        private byte m_byCheckSum = 0;
        private byte m_byCheckSum_add = 0;
        private byte[] m_pbyteData;
        private int m_nSeq = 0;
        private int m_nSeq_Rx = 0;
        #region Memory
        private const int _CMD_ROM = 0x42;
        private const int _CMD_RAM = 0x44;
        private const int _CMD_OPSU_SENSOR = 0x74;
        private const int _CMD_MPSU_ROM = 0x52;
        private const int _CMD_MPSU_RAM = 0x54;
        private const int _CMD_MPSU_ROM2 = 0x12;
        private const int _CMD_MPSU_RAM2 = 0x14;
        private const int _CMD_HEAD_ROM = 0x4B;
        private const int _CMD_HEAD_RAM = 0x4D;

        public const int _SIZE_ROM = 54;
        public const int _SIZE_RAM = 74;
        public const int _SIZE_SENSOR = 22;
        public const int _SIZE_MPSU_ROM = 23;
        public const int _SIZE_MPSU_RAM = 234;
        public const int _SIZE_HEAD_ROM = 22;
        public const int _SIZE_HEAD_RAM = 56;
        private byte[,] m_pbyRam = new byte[256, _SIZE_RAM];
        private byte[,] m_pbyRom = new byte[256, _SIZE_ROM];
        private byte[] m_pbySensor = new byte[_SIZE_SENSOR];
        private byte[] m_pbyMpsuRam = new byte[_SIZE_MPSU_RAM];
        private byte[] m_pbyMpsuRom = new byte[_SIZE_MPSU_ROM];
        private byte[] m_pbyHeadRam = new byte[_SIZE_HEAD_RAM];
        private byte[] m_pbyHeadRom = new byte[_SIZE_HEAD_ROM];
        public byte drbluetooth_mpsu_get_data_ram(int nAddress) { return m_pbyMpsuRam[nAddress]; }
        public byte[] drbluetooth_mpsu_get_data_ram() { return m_pbyMpsuRam; }
        #endregion Memory

        private bool m_bHeader = false;
        private bool m_bMpsu = false;
        private int m_nDataIndex = 0;
        private void ReceiveDataCallback()
        {
            m_bHeader = false;
            m_bMpsu = false;
            byte RxData;
            // 일단 하나의 완전한 패킷 형태로 만든다.
            while ((connected() == true) && (m_bClassEnd == false))
            {
                try
                {
                    int nPacketLength = GetBuffer_Length();
                    if (nPacketLength > 0)
                    {
                        m_nCntTick_Receive_Event_All++;
                        
                        // 통신이 이루어 진 것이므로 다음 명령을 받을 수 있도록 busy 를 먼저 클리어 한다.
                        m_bBusy = false;
                        for (int i = 0; i < nPacketLength; i++)
                        {
                            RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                            //m_nRxIndex++;

                            if ((RxData == 0xFF) && (m_nRxIndex == 0xFF)) // 데이타 준비상태이면서 첫번째 헤더가 0xff 라면
                            {
                                // 초기화...
                                m_nRxIndex = 0;
                                m_nPacket_Length = 0;
                                m_bHeader = true;

                                m_nDataIndex = 0;
                                continue;
                            }

                            if (m_bHeader == true)
                            {
                                m_bHeader = false;
                                if (RxData == 0xff)
                                {
                                    m_bMpsu = true;
                                    continue;
                                }
                                else m_bMpsu = false;
                            }

                            // Mpsu 패킷과 일반 패킷을 구분
                            if (m_bMpsu == true)
                            {
                                #region MPSU Packet(Switch Case)
                                switch (m_nRxIndex)
                                {
                                    #region Packet Size(0)
                                    case 0:
                                        m_nPacket_Length = (int)RxData;
                                        if (m_nPacket_Length < 9)
                                        {
                                            m_nRxIndex = 0xFF;
                                            continue;
                                        }
                                        m_nPacket_Pos = m_nPacket_Length - (9 - 2); // Status 2 bytes
                                        m_pbyteData = new byte[m_nPacket_Pos];

                                        m_byCheckSum = RxData;
                                        break;
                                    #endregion Packet Size(0)
                                    #region ID(1)
                                    case 1:
                                        m_nPacket_ID = (int)RxData; // 0x40 ~ 0x5f
                                        if ((RxData < 0x40) || (RxData > 0x5f))
                                        {
                                            m_nRxIndex = 0xFF;
                                            continue;
                                        }
                                        m_byCheckSum ^= RxData;
                                        break;
                                    #endregion ID(1)
                                    #region Command(2)
                                    case 2:
                                        m_nPacket_Cmd = (int)RxData;
                                        if (                                // 분류는 여러가지이지만 현재 MPSU_RAM 만 고려되어 있다.
                                            (RxData != _CMD_MPSU_ROM) &&	// MPSU_EEP_READ
                                            (RxData != _CMD_MPSU_RAM) &&	// MPSU_RAM_READ -> 이것만...
                                            (RxData != _CMD_MPSU_ROM2) &&	// MPSU_EEP_READ
                                            (RxData != _CMD_MPSU_RAM2) 	    // MPSU_RAM_READ -> 이것만... => MPSU 명령이 수정된것 같음. 나중에 철희에게 확인해 볼것. 기존에는 0x54 가 반송되도록 되어 있음.
                                        )
                                        {
                                            m_nRxIndex = 0xFF;
                                            continue;
                                        }
                                        m_byCheckSum ^= RxData;
                                        break;
                                    #endregion Command(2)
                                    #region CheckSum 1(3)
                                    case 3:
                                        m_nPacket_Checksum1 = (int)RxData;
                                        break;
                                    #endregion CheckSum 1(3)
                                    #region CheckSum 2(4)
                                    case 4:
                                        m_nPacket_Checksum2 = (int)RxData;
                                        break;
                                    #endregion CheckSum 2(4)
                                    #region Data and Status(5)
                                    case 5:
                                        if (m_nPacket_Pos > 0)
                                        {
                                            int nPos = ((m_nPacket_Length - 7) - m_nPacket_Pos);
                                            if (m_nPacket_Pos <= 2)
                                            {
                                                // Status
                                                m_pbyteData[nPos] = (byte)RxData;
                                            }
                                            else
                                            {
                                                // Data
                                                m_pbyteData[nPos] = (byte)RxData;
                                            }
                                            m_byCheckSum ^= RxData;
                                            m_nPacket_Pos--;
                                            m_nRxIndex--;//
                                        }

                                        if (m_nPacket_Pos == 0)
                                        {
                                            // 데이타 수집이 완료되었다면
                                            if ((m_nPacket_Cmd == _CMD_MPSU_RAM) || (m_nPacket_Cmd == _CMD_MPSU_RAM2))
                                            {
                                                // 체크섬을 조사해 보고 이상유무를 파악
                                                if (((m_byCheckSum & 0xfe) == m_nPacket_Checksum1) && ((~m_nPacket_Checksum1 & 0xfe) == m_nPacket_Checksum2))
                                                {
                                                    // 체크섬 이상 없음.
                                                    m_nSeq_Rx++;
                                                    // 다시 데이터를 받을 준비...
                                                    m_nRxIndex = 0xFF;
                                                }
                                            }
                                        }
                                        break;
                                    #endregion Data and Status(5)                                        
                                    #region Etc(default) 의 경우 - 초기화
                                    default:
                                        m_nRxIndex = 0xFF;
                                        break;
                                    #endregion Etc(default) 의 경우 - 초기화
                                }
                                #endregion MPSU Packet(Switch Case)
                            }
                            else
                            {
                                #region Normal Packet

                                switch (m_nRxIndex)
                                {
                                    #region ID          -> nID
                                    case 0:
                                        m_nPacket_ID = RxData;
                                        m_byCheckSum = RxData;
                                        break;
                                    #endregion ID       -> nID
                                    #region Cmd         -> nCmd
                                    case 1:
                                        m_nPacket_Cmd = RxData;
                                        m_byCheckSum ^= RxData;
                                        break;
                                    #endregion Cmd      -> nCmd
                                    #region Packet Size(0)
                                    case 2:
                                    case 3:
                                    case 4:
                                    case 5:
                                        m_nPacket_Length += ((int)RxData << ((3 - (m_nRxIndex - 2)) * 8));                                        
                                        m_byCheckSum ^= RxData;
                                        if (m_nPacket_Length > 0)
                                            m_pbyteData = new byte[m_nPacket_Length];
                                        break;
                                    #endregion Packet Size(0)
                                    #region Data & CheckSum
                                    case 6:
                                        if (m_nPacket_Length > 0)
                                        {
                                            m_pbyteData[m_nDataIndex++] = (byte)RxData;
                                            m_byCheckSum ^= RxData;                                            
                                        }
                                        if (m_nPacket_Length != m_nDataIndex) m_nRxIndex--;
                                        break;
                                    case 7:
                                        if ((m_byCheckSum & 0x7f) == RxData)
                                        {
                                            // 체크섬 이상 없음.
                                            m_nSeq_Rx++;                                            
                                        }
                                        // 다시 데이터를 받을 준비...
                                        m_nRxIndex = 0xFF;
                                        break;
                                    #endregion Data & CheckSum
                                    #region Etc(default) 의 경우 - 초기화
                                    default:
                                        m_nRxIndex = 0xFF;
                                        break;
                                    #endregion Etc(default) 의 경우 - 초기화
                                }
                                #endregion Normal Packet
                            }

                            if (m_nRxIndex != 0xFF) m_nRxIndex++;


                            // 데이타가 깨지지 않은 경우 ...
                            if (m_nSeq != m_nSeq_Rx)
                            {
                                m_nSeq = m_nSeq_Rx; // 일단 시퀀스 동기화

                                if (
                                    (drbluetooth_get_id() == m_nPacket_ID) || // 해당 로봇을 지칭하는 경우에만 명령 수행, drbluetooth_get_id 는 테스트로 할 경우 미리 셋팅해 놓도록 한다.
                                    (m_nPacket_ID == 0xfe) // BroadCasting
                                )
                                {
                                    if (((m_nPacket_ID >= 0x40) && (m_nPacket_ID < 0x60)) || (m_nPacket_ID == 253))
                                    {
                                        if (m_nPacket_Cmd == _CMD_MPSU_RAM)
                                        {
                                            int nAddress = m_pbyteData[0];
                                            int nLength = m_pbyteData[1];
                                            // 그냥 메모리에 넣는다.
                                            Array.Copy(m_pbyteData, 0, m_pbyMpsuRam, nAddress, nLength);
                                            m_nClient_Seq++; // Parser() 함수에는 있으나 여기는 없기 때문에 busy 를 클리어하려면 여기에 이 부분을 넣어준다.
                                        }
                                    }
                                    else
                                        Parser(m_nPacket_Cmd, m_nPacket_Length, m_pbyteData, m_byCheckSum);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // 다시 데이터를 받을 준비...
                    m_nRxIndex = 0xFF;
                }
                Thread.Sleep(10);
            }
        }
        private void ReceiveDataCallback2()		/* Callback 함수 */
        {
            while ((connected() == true) && (m_bClassEnd == false))
            {
                try
                {                   
                    int i;
                    //int nCode = 0;
                    int nID = 0;
                    int nCmd = 0;
                    int nLength = 0;
                    int nAllLength = 0;
                    byte RxData = 0;
                    int nPacketLength = GetBuffer_Length();
                    int nChecksum1 = 0;
                    int nChecksum2 = 0;
                    int nAddress = 0;
                    if (nPacketLength > 0)
                    {
                        m_nCntTick_Receive_Event_All++;

                        // 통신이 이루어 진 것이므로 다음 명령을 받을 수 있도록 busy 를 먼저 클리어 한다.
                        m_bBusy = false;

                        int nPos = 0;

                        for (i = 0; i < nPacketLength; i++)
                        {








                            RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                            if (connected() == false) break;

                            if (RxData == 0xff) // 리턴해서 돌아오는 패킷에 헤더를 제외하고 ff 는 없다고 가정
                            {
                                // 패킷이 깨지는 경우 염두에 두지 않고 간략한 형태로만 넣었음. 문제되면 넣도록 할것.
                                RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                if (RxData == 0xff) // MPSU
                                {
                                    int nIndex = 0;
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    #region Size
                                    // PacketSize
                                    //if ((RxData & 0xf0) == 0xf0) break; // 잘못된 데이타 사이즈의 경우 패스(0xf0 로 시작된다면 음수)
                                    nAllLength = RxData;
                                    m_byCheckSum = RxData;
                                    nIndex++;
                                    #endregion Size
                                    #region Id
                                    // ID
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    nID = RxData;
                                    m_byCheckSum ^= RxData;
                                    nIndex++;
                                    #endregion Id
                                    #region Cmd
                                    // Cmd
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    if ( // 분류는 여러가지이지만 현재 MPSU_RAM 만 고려되어 있다.
                                        (RxData != _CMD_ROM) &&	// EEP_READ
                                        (RxData != _CMD_RAM) &&	// RAM_READ
                                        (RxData != _CMD_MPSU_ROM) &&	// MPSU_EEP_READ
                                        (RxData != _CMD_MPSU_RAM) &&	// MPSU_RAM_READ -> 이것만...
                                        (RxData != _CMD_MPSU_ROM2) &&	// MPSU_EEP_READ
                                        (RxData != _CMD_MPSU_RAM2) &&	// MPSU_RAM_READ -> 이것만... => MPSU 명령이 수정된것 같음. 나중에 철희에게 확인해 볼것. 기존에는 0x54 가 반송되도록 되어 있음.
                                        (RxData != _CMD_HEAD_ROM) &&	// MPSU_EEP_READ
                                        (RxData != _CMD_HEAD_RAM) &&	// MPSU_RAM_READ
                                        (RxData != _CMD_OPSU_SENSOR) // OPSU Sensor READ
                                        )
                                    {
                                        break;
                                    }
                                    nCmd = RxData;
                                    m_byCheckSum ^= RxData;
                                    nIndex++;
                                    #endregion Cmd

                                    #region checksum 1
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    nChecksum1 = RxData;
                                    #endregion checksum 1

                                    #region checksum 2
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    nChecksum2 = RxData;
                                    #endregion checksum 2

                                    #region Address
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    nAddress = RxData;
                                    m_byCheckSum ^= RxData;
                                    nIndex++;
                                    #endregion Addreass

                                    #region Data Length
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    nLength = RxData;
                                    m_byCheckSum ^= RxData;
                                    nIndex++;
                                    #endregion Data Length

                                    #region Data & CheckSum
                                    m_pbyteData = new byte[nLength]; // 메모리 설정
                                    byte[] pbyteData = get_bytes(nLength);
                                    if (pbyteData.Length != nLength) // 패킷 사이즈가 틀리다면...
                                        break;

                                    #region Status ( 2 bytes )
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    //m_byCheckSum ^= RxData;
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    //m_byCheckSum ^= RxData;
                                    #endregion Status

                                    // CheckSum
                                    for (int nCheckPos = 0; nCheckPos < nLength; nCheckPos++) m_byCheckSum ^= pbyteData[nCheckPos];
                                    m_byCheckSum = (byte)(m_byCheckSum & 0xfe);

                                    if ((nChecksum1 == m_byCheckSum) && (nChecksum2 == (~m_byCheckSum & 0xfe)))
                                    {
                                        // 체크섬 이상 없음.
                                        Array.Copy(pbyteData, m_pbyteData, nLength);
                                        m_nSeq_Rx++;
                                    }
                                    //else 
                                    //    break;
                                    #endregion Data & CheckSum
                                    break;                                    
                                }
                                else
                                {
                                    #region ID          -> nID
                                    nID = RxData;
                                    m_byCheckSum = RxData;
                                    #endregion ID       -> nID

                                    #region Cmd         -> nCmd
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    nCmd = RxData;
                                    m_byCheckSum ^= RxData;
                                    #endregion Cmd      -> nCmd

                                    #region Size        -> nLength
                                    byte byData0 = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    m_byCheckSum ^= byData0;
                                    byte byData1 = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    m_byCheckSum ^= byData1;
                                    byte byData2 = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    m_byCheckSum ^= byData2;
                                    byte byData3 = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    m_byCheckSum ^= byData3;
                                    nLength = get_int32(byData0, byData1, byData2, byData3);
                                    #endregion Size     -> nLength

                                    #region Data & CheckSum
                                    m_pbyteData = new byte[nLength]; // 메모리 설정
                                    byte[] pbyteData = get_bytes(nLength);
                                    if (pbyteData.Length != nLength) // 패킷 사이즈가 틀리다면...
                                        break;

                                    for (int nCheckPos = 0; nCheckPos < nLength; nCheckPos++)
                                    {
                                        m_byCheckSum ^= pbyteData[nCheckPos];
                                    }

                                    m_byCheckSum = (byte)(m_byCheckSum & 0x7f);
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                    if (RxData == m_byCheckSum)
                                    {
                                        // 체크섬 이상 없음.
                                        Array.Copy(pbyteData, m_pbyteData, nLength);
                                        m_nSeq_Rx++;
                                    }
                                    //else 
                                    //    break;
                                    break;
                                    #endregion Data & CheckSum
                                }
                            }
#if false
                            RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                            if (connected() == false) // return 보다 continue 로 해서 쓰레기 데이타를 처리하도록 한다.
                            {
                                m_nRxIndex = 0xff;
                                continue;
                            }

                            if ((RxData == 0xFF) && (m_nRxIndex == 0xFF)) // 데이타 준비상태이면서 첫번째 헤더가 0xff 라면
                            {
                                // 초기화...
                                m_nRxIndex = 0;
                                nLength = 0;
                                //m_byCheckSum = 0;
                                nPos = 0;
                                continue;
                            }
                            
                            switch (m_nRxIndex)
                            {
                                #region Header      -> 잘못되면 m_nRxIndex 를 0xFF 으로 초기화 -> 정대로 0xff 가 두번 나올리 없다.
                                case 0: // Header
                                    if (RxData != 0xFF)
                                    {
                                        m_nRxIndex = 0xFF;
                                        continue;
                                    }
                                    break;
                                #endregion Header   -> 잘못되면 m_nRxIndex 를 0xFF 으로 초기화 -> 정대로 0xff 가 두번 나올리 없다.
                                #region Code        -> nCode
                                case 1: //Get Code
                                    nCode = RxData;
                                    if (nCode != _CODE_BLUETOOTH)
                                    {
                                        m_nRxIndex = 0xFF;
                                        continue;
                                    }
                                    m_byCheckSum = RxData;
                                    break;
                                #endregion Code     -> nCode
                                #region ID          -> nID
                                case 2: //Get ID
                                    nID = RxData;
                                    m_byCheckSum ^= RxData;
                                    break;
                                #endregion ID       -> nID
                                #region Cmd         -> nCmd
                                case 3: // Get Cmd
                                    nCmd = RxData;
                                    m_byCheckSum ^= RxData;
                                    break;
                                #endregion Cmd      -> nCmd
                                #region Size        -> nLength
                                case 4: //Get Data Size(Packet Size)
                                    nLength = RxData;
                                    m_byCheckSum ^= RxData;

                                    m_pbyteData = new byte[RxData]; // 메모리 설정
                                    break;
                                #endregion Size     -> nLength
                                #region Data        -> m_pbyteData[] 에 저장
                                case 5: //Get Data				
                                    if (nPos < nLength - 1) // 마지막 데이터가 되기 전까지는 이 부분에서 계속 데이타를 얻어가도록 한다.
                                        m_nRxIndex--;

                                    m_pbyteData[nPos++] = RxData;
                                    m_byCheckSum ^= RxData;                                     
                                    break;
                                #endregion Data     -> m_pbyteData[] 에 저장
                                #region CheckSum    -> m_byCheckSum
                                case 6: 			// checksum
                                    m_byCheckSum = (byte)(m_byCheckSum & 0x7f);
                                    if (RxData != m_byCheckSum)
                                    {
                                        // 체크섬 이상 벌견
                                        m_nRxIndex = 0xFF;
                                    }
                                    else
                                        m_nSeq_Rx++;
                                    break;
                                #endregion CheckSum -> m_byCheckSum;

                                #region Etc(default) 의 경우 - 초기화
                                default:
                                        m_nRxIndex = 0xFF;
                                    break;
                                #endregion Etc(default) 의 경우 - 초기화
                            }
                            if (m_nRxIndex != 0xFF) m_nRxIndex++;
#endif
                        }

                        // 데이타가 깨지지 않은 경우 ...
                        if (m_nSeq != m_nSeq_Rx)
                        {
                            m_nSeq = m_nSeq_Rx; // 일단 시퀀스 동기화

                            // 여기부터 파서 가동
                            //if (drbluetooth_get_param_servermode() == false)
                            //{
                            //    Parser(nCmd, nLength, m_pbyteData, m_byCheckSum);
                            //}
                            //else
                            //{
                                if ((drbluetooth_get_id() == nID) || // 해당 로봇을 지칭하는 경우에만 명령 수행, drbluetooth_get_id 는 테스트로 할 경우 미리 셋팅해 놓도록 한다.

                                    //(nID == 0xc0) || // for test

                                    (nID == 0xfe)) // BroadCasting
                                {
                                    if (((nID >= 0x40) && (nID < 0x60)) || (nID == 253))
                                    {
                                        if (nCmd == _CMD_MPSU_RAM)
                                        {
                                            // 그냥 메모리에 넣는다.
                                            Array.Copy(m_pbyteData, 0, m_pbyMpsuRam, nAddress, nLength);
                                            m_nClient_Seq++; // Parser() 함수에는 있으나 여기는 없기 때문에 busy 를 클리어하려면 여기에 이 부분을 넣어준다.
                                        }
                                    }
                                    else 
                                        Parser(nCmd, nLength, m_pbyteData, m_byCheckSum);
                                }
                            //}
                        }
                    }
                }
                catch
                {
                }
                Thread.Sleep(10);
            }

        }
           
        private void Parser(int nCmd, int nLength, byte [] pbyteData, byte byteCheckSum)
        {
            #region Parser
            //if (m_bAuth == true)
            
            if (nCmd == 0xf0)
            {
                //_RET_RECEIVE_OK, ...
                if (pbyteData.Rank >= 1)
                {
                    if      ((byte)(pbyteData[1]) == 0) m_nProcess_Client = _RET_RECEIVE_OK;
                    else if ((byte)(pbyteData[1]) == 1) m_nProcess_Client = _RET_PROCESS_END;
                    else if ((byte)(pbyteData[1]) == 2) m_nProcess_Client = _RET_PROCESS_FAILED;
                    else if ((byte)(pbyteData[1]) == 3) m_nProcess_Client = _RET_CHECKSUM_ERROR;
                    else if ((byte)(pbyteData[1]) == 4) m_nProcess_Client = _RET_TIME_OUT_ERROR;
                    else if ((byte)(pbyteData[1]) == 5) m_nProcess_Client = _RET_LENGTH_ERROR;
                }
            }
            else if (nCmd == 0xf1)
            {
                if (nLength == 0)
                    m_nProcess_Client = _RET_RECEIVE_OK;
                else
                    m_nProcess_Client = _RET_LENGTH_ERROR;
            }
            else
            {
                switch (nCmd)
                {
                    case 0x31:
                        {
                            // 파일목록 확인
                            if (nLength >= 3)
                            {
                                bool bMaster = (pbyteData[0] == 0) ? true : false;
                                int nFileListSize = (pbyteData[1] << 8) + pbyteData[2];
                                int nPos = 3;
                                if (pbyteData.Length == (3 + _SIZE_FILENAME * nFileListSize))
                                {
                                    if (bMaster == true)
                                    {
                                        m_nFileList_master = nFileListSize;
                                        m_pbyteFileList_master = new byte[_SIZE_FILENAME * nFileListSize];
                                        Array.Copy(pbyteData, nPos, m_pbyteFileList_master, 0, _SIZE_FILENAME * nFileListSize);
                                    }
                                    else
                                    {
                                        m_nFileList_user = nFileListSize;
                                        m_pbyteFileList_user = new byte[_SIZE_FILENAME * nFileListSize];
                                        Array.Copy(pbyteData, nPos, m_pbyteFileList_user, 0, _SIZE_FILENAME * nFileListSize);
                                    }
                                }
                                m_nProcess_Client = _RET_RECEIVE_OK;
                            }
                            else
                                m_nProcess_Client = _RET_LENGTH_ERROR;
                        }
                        break;
                    case 0x33: // Request
                        {
                            if (nLength > (1 + _SIZE_FILENAME))
                            {
                                int nFileSize = nLength - (1 + _SIZE_FILENAME);
                                int nPos = 0;
                                int nMaster = pbyteData[nPos++];
                                if ((nMaster >= 0) || (nMaster <= 1))
                                {
                                    String strFileName = Encoding.Default.GetString(pbyteData, nPos, _SIZE_FILENAME);
                                    nPos += _SIZE_FILENAME;
                                    byte[] pbyteFile = new byte[nFileSize];
                                    Array.Copy(pbyteData, nPos, pbyteFile, 0, nFileSize);

                                    //String strDir = ((m_nCurrentPort == 7000) ? "task\\" : "motion\\") + ((nMaster == 0) ? "master\\" : "user\\");
                                    String strDir = "motion\\" + ((nMaster == 0) ? "master\\" : "user\\");
                                    DirectoryInfo dirinfoMasterUser = new DirectoryInfo(strDir);
                                    if (dirinfoMasterUser.Exists == false)
                                    {
                                        dirinfoMasterUser.Create();

                                    }
                                    string strFile = strDir + strFileName.Trim('\0');
                                    FileStream fs = new FileStream(strFile, FileMode.Create);
                                    fs.Write(pbyteFile, 0, nFileSize);
                                    fs.Close();
                                }
                                m_nProcess_Client = _RET_RECEIVE_OK;
                            }
                            else
                                m_nProcess_Client = _RET_LENGTH_ERROR;
                        }
                        break;
                    case 0x41:
                        {
                            if (nLength == 1)
                            {
                                m_bPlaying = Convert.ToBoolean(pbyteData[0]);
                                m_nProcess_Client = _RET_RECEIVE_OK;
                            }
                            else
                                m_nProcess_Client = _RET_LENGTH_ERROR;
                        }
                        break;
                    case 0x62:
                        {
                            if (nLength == 8)
                            {
                                m_nTimeSec = (pbyteData[0] << 24) | (pbyteData[1] << 16) | (pbyteData[2] << 8) | (pbyteData[3]);
                                m_nTimeUSec = (pbyteData[4] << 24) | (pbyteData[5] << 16) | (pbyteData[6] << 8) | (pbyteData[7]);
                                m_nProcess_Client = _RET_RECEIVE_OK;
                            }
                            else
                                m_nProcess_Client = _RET_LENGTH_ERROR;
                        }
                        break;
                    case 0x63:
                        {
                            if (nLength == 2)
                            {
                                m_nRobotBatt = pbyteData[0];
                                m_nMIDBatt = pbyteData[1];
                                m_nProcess_Client = _RET_RECEIVE_OK;
                            }
                            else
                                m_nProcess_Client = _RET_LENGTH_ERROR;
                        }
                        break;
                    case 0x65:
                        {
                            if (nLength == 2)
                            {
                                m_nRmcLength = pbyteData[0];
                                m_nRmcData = pbyteData[1];
                                m_nProcess_Client = _RET_RECEIVE_OK;
                            }
                            else
                                m_nProcess_Client = _RET_LENGTH_ERROR;
                        }
                        break;
                    //case 0x71:
                    //    {
                    //        if (nLength == 1)
                    //        {
                    //            m_bClientAuth = (pbyteData[0] == 1) ? true : false;
                    //            m_nProcess_Client = _RET_RECEIVE_OK;
                    //        }
                    //        else
                    //            m_nProcess_Client = _RET_LENGTH_ERROR;
                    //    }
                    //    break;
                }
            }

            m_nClient_Seq++;
            #endregion Parser
        }
                
        #region 공개
        public static bool _RESULT_OK = true;
        public static bool _RESULT_FAIL = false;
        public String get_application_path() { return m_strOrgPath; }
        //public bool thread_started() { return m_bThread_Client; }
        public int packet_status() { return m_nProcess_Client; }
        public static string packet_status_tostring(int nPacketStat)
        {
            switch (nPacketStat)
            {
                case _RET_RECEIVE_OK:
                    return "Receive OK";
                //break;
                case _RET_PROCESS_END:
                    return "Process End";
                //break;
                case _RET_PROCESS_FAILED:
                    return "Process Failed";
                //break;
                case _RET_CHECKSUM_ERROR:
                    return "Check Sum Error";
                //break;
                case _RET_TIME_OUT_ERROR:
                    return "Time Out Error";
                //break;
                case _RET_LENGTH_ERROR:
                    return "Length Error";
                //break;
                default:
                    return "Unknown Status";
                //break;
            }
        }
        //public bool is_auth() { return m_bClientAuth; }
        public int seq() { return m_nClient_Seq; }
        //public bool connected()
        //{
        //    if (m_tcpClient == null) return _RESULT_FAIL;
        //    return m_tcpClient.Connected;// ((m_tcpClient.Connected == true) ? _RESULT_OK : _RESULT_FAIL);
        //}
        //public bool connect(int nIp_A, int nIp_B, int nIp_C, int nIp_D, int nPort)
        //{
        //    String strIP = nIp_A.ToString() + "." + nIp_B.ToString() + "." + nIp_C.ToString() + "." + nIp_D.ToString();
        //    return connect(strIP, nPort);
        //}
        //public bool connect(int nIp_A, int nIp_B, int nIp_C, int nIp_D, int nPort, int nId)
        //{
        //    String strIP = nIp_A.ToString() + "." + nIp_B.ToString() + "." + nIp_C.ToString() + "." + nIp_D.ToString();
        //    return connect(strIP, nPort, nId);
        //}
#if false
        public bool connect(String strIP, int nPort)
        {
            try
            {
                m_tcpClient = new TcpClient();       // TCP 클라이언트 생성

                try
                {
                    // 서버 IP 주소와 포트 번호를 이용해 접속 시도
                    m_tcpClient.Connect(strIP, nPort);

                    // 현재 포트와 아이피 저장
                    m_nCurrentPort = nPort;
                    m_strCurrentIp = strIP;

                    //m_tcpClient = new TcpClient(strIP, nPort);
                }
                catch//(Exception e)
                {
                    //string strData = e.ToString();
                    //strData += ".\r\n");

                    return _RESULT_FAIL;
                }

                // 스레드를 생성한다.
                m_thClient = new Thread(new ThreadStart(ThreadClient));

                m_streamClient = m_tcpClient.GetStream();    // 스트림 가져오기
                m_bwClient_outData = new BinaryWriter(new BufferedStream(m_tcpClient.GetStream()));
                m_bwClient_inData = new BinaryReader(new BufferedStream(m_tcpClient.GetStream()));

                // 스레드 시작
                m_thClient.Start();

                return connected();
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public bool connect(String strIP, int nPort, int nId)
        {
            try
            {
                m_nClientId = nId;
                m_tcpClient = new TcpClient();       // TCP 클라이언트 생성

                try
                {
                    // 서버 IP 주소와 포트 번호를 이용해 접속 시도
                    m_tcpClient.Connect(strIP, nPort);

                    // 현재 포트와 아이피 저장
                    m_nCurrentPort = nPort;
                    m_strCurrentIp = strIP;

                    //m_tcpClient = new TcpClient(strIP, nPort);
                }
                catch//(Exception e)
                {
                    //string strData = e.ToString();
                    //strData += ".\r\n");

                    return _RESULT_FAIL;
                }

                // 스레드를 생성한다.
                m_thClient = new Thread(new ThreadStart(ThreadClient));

                m_streamClient = m_tcpClient.GetStream();    // 스트림 가져오기
                m_bwClient_outData = new BinaryWriter(new BufferedStream(m_tcpClient.GetStream()));
                m_bwClient_inData = new BinaryReader(new BufferedStream(m_tcpClient.GetStream()));

                // 스레드 시작
                m_thClient.Start();

                return connected();
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public bool disconnect()
        {
            try
            {
                //bool connected() = IsConnect();
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL; //  IsConnect 플래그 체크

                m_bwClient_inData.Close();           // 입력 스트림 닫기
                m_bwClient_outData.Close();          // 출력 스트림 닫기

                m_streamClient.Close();             // 스트림 종료

                //Reader.Abort();                   // 쓰레드 종료
                m_tcpClient.Close();
                m_tcpClient = null;
                return _RESULT_OK;
            }
            catch//(Exception e)
            {
                return _RESULT_FAIL;
                //MessageBox.Show("[Message]" + e.ToString() + "\r\n");
            }
        }
#endif        
        // 벌크데이타를 보냄
        public bool send(byte[] byteData)
        {
            try
            {
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL;

                if (connected() == _RESULT_OK) m_SerialPort.Write(byteData, 0, byteData.Length);//m_bwClient_outData.Write(byteData);
                //if (connected() == _RESULT_OK) m_SerialPort.m_bwClient_outData.Flush();
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public byte get_byte()
        {
            try
            {
                if (connected() == _RESULT_OK) return (byte)m_SerialPort.ReadByte();
                return 0;
            }
            catch
            {
                return 0;
            }
        }
        // 2 Byte 의 정수값을 반환
        public int get_int16()
        {
            //if (connected() == _RESULT_OK) return inData.ReadInt16(); //2 Bytes
            //else return 0;
            int nData;
            byte[] byteData = get_bytes(2);
            nData = (byteData[2] << 8);
            nData += (byteData[3]);
            if (connected() == _RESULT_OK) return nData; //2 Bytes
            else return 0;
        }
        // 4 Byte 의 정수값을 반환
        public int get_int32()
        {
            int nData;
            byte[] byteData = get_bytes(4);
            nData = (byteData[0] << 24);
            nData += (byteData[1] << 16);
            nData += (byteData[2] << 8);
            nData += (byteData[3]);
            //if (connected() == _RESULT_OK) return inData.ReadInt32(); //4 Bytes
            if (connected() == _RESULT_OK) return nData; //4 Bytes
            else return 0;
        }
        // 4 Byte 의 정수값을 반환
        public int get_int32(byte byData0, byte byData1, byte byData2, byte byData3)
        {
            int nData;
            nData =  (byData0 << 24);
            nData += (byData1 << 16);
            nData += (byData2 << 8);
            nData += (byData3);
            return nData; //4 Bytes
        }
        // Size 만큼을 읽어감
        public byte[] get_bytes(int nSize)
        {
            byte [] pbyteData = new byte[nSize];
            for(int i = 0; i < nSize; i++)
                pbyteData[i] = (byte)m_SerialPort.ReadByte();
            return pbyteData;//m_bwClient_inData.ReadBytes(nSize);
            //if (connected() == _RESULT_OK) return inData.ReadBytes(nSize);
            //else return null;
        }

        #region SendData
        public void send_command(int nCommand)
        {
#if false
            int i, nNum;

            // 프레임 수
            int nSize = 0;
            int nPacketSize = nSize + 8 + 2;
            byte[] byteData = new byte[nPacketSize];

            nNum = 0;
            byteData[nNum++] = 0xff;
            byteData[nNum++] = 0xff;
            // Code
            byteData[nNum++] = (byte)(_CODE_BLUETOOTH & 0xff);
            // ID
            byteData[nNum++] = (byte)(drbluetooth_get_id() & 0xff);
            // Cmd
            byteData[nNum++] = (Byte)(nCommand & 0xff);

            // Data Size
            byteData[nNum++] = (byte)((nSize >> 24) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 16) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 8) & 0xff);
            byteData[nNum++] = (byte)(nSize & 0xff);

            // CheckSum
            byte byteCheckSum = byteData[2];
            for (i = 3; i < nPacketSize - 1; i++)
            {
                byteCheckSum ^= byteData[i];
            }
            byteData[nNum++] = (byte)(byteCheckSum & 0x7f);
            send(byteData);
#else
            int nSize = 0;
            //byte[] byteArrayData = new byte[nSize];
            //int i = 0;
            //byteArrayData[i++] = byteData1;
            //byteArrayData[i++] = byteData2;
            //byteArrayData[i++] = byteData3;
            //byteArrayData[i++] = byteData4;
            send_data(nCommand, 0, null);
            //byteArrayData = null;
#endif
        }
        public void send_1_data(int nCommand, byte byteOneData)
        {
#if false
            int i, nNum;

            // 프레임 수
            int nSize = 1;
            int nPacketSize = nSize + 8 + 2;
            byte[] byteData = new byte[nPacketSize];

            nNum = 0;
            byteData[nNum++] = 0xff;
            byteData[nNum++] = 0xff;
            // Code
            byteData[nNum++] = (byte)(_CODE_BLUETOOTH & 0xff);
            // ID
            byteData[nNum++] = (byte)(drbluetooth_get_id() & 0xff);
            // Cmd
            byteData[nNum++] = (Byte)(nCommand & 0xff);

            // Data Size
            byteData[nNum++] = (byte)((nSize >> 24) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 16) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 8) & 0xff);
            byteData[nNum++] = (byte)(nSize & 0xff);

            // Data
            byteData[nNum++] = (byte)(byteOneData & 0xff);

            // CheckSum
            byte byteCheckSum = byteData[2];
            for (i = 3; i < nPacketSize - 1; i++)
            {
                byteCheckSum ^= byteData[i];
            }
            byteData[nNum++] = (byte)(byteCheckSum & 0x7f);

            send(byteData);
#else
            int nSize = 1;
            byte[] byteArrayData = new byte[nSize];
            int i = 0;
            byteArrayData[i++] = byteOneData;
            send_data(nCommand, nSize, byteArrayData);
            byteArrayData = null;
#endif
        }
        public void send_2_data(int nCommand, byte byteData1, byte byteData2)
        {
#if false
            int i, nNum;

            // 프레임 수
            int nSize = 2;
            int nPacketSize = nSize + 8 + 2;
            byte[] byteData = new byte[nPacketSize];

            nNum = 0;
            byteData[nNum++] = 0xff;
            byteData[nNum++] = 0xff;
            // Code
            byteData[nNum++] = (byte)(_CODE_BLUETOOTH & 0xff);
            // ID
            byteData[nNum++] = (byte)(drbluetooth_get_id() & 0xff);
            // Cmd
            byteData[nNum++] = (Byte)(nCommand & 0xff);

            // Data Size
            byteData[nNum++] = (byte)((nSize >> 24) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 16) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 8) & 0xff);
            byteData[nNum++] = (byte)(nSize & 0xff);

            // Data
            byteData[nNum++] = (byte)(byteData1 & 0xff);
            byteData[nNum++] = (byte)(byteData2 & 0xff);

            // CheckSum
            byte byteCheckSum = byteData[2];
            for (i = 3; i < nPacketSize - 1; i++)
            {
                byteCheckSum ^= byteData[i];
            }
            byteData[nNum++] = (byte)(byteCheckSum & 0x7f);

            send(byteData);
#else
            int nSize = 2;
            byte[] byteArrayData = new byte[nSize];
            int i = 0;
            byteArrayData[i++] = byteData1;
            byteArrayData[i++] = byteData2;
            send_data(nCommand, nSize, byteArrayData);
            byteArrayData = null;
#endif
        }
        public void send_3_data(int nCommand, byte byteData1, byte byteData2, byte byteData3)
        {
#if false
            int i, nNum;

            // 프레임 수
            int nSize = 3;
            int nPacketSize = nSize + 8 + 2;
            byte[] byteData = new byte[nPacketSize];

            nNum = 0;
            byteData[nNum++] = 0xff;
            byteData[nNum++] = 0xff;
            // Code
            byteData[nNum++] = (byte)(_CODE_BLUETOOTH & 0xff);
            // ID
            byteData[nNum++] = (byte)(drbluetooth_get_id() & 0xff);
            // Cmd
            byteData[nNum++] = (Byte)(nCommand & 0xff);

            // Data Size
            byteData[nNum++] = (byte)((nSize >> 24) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 16) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 8) & 0xff);
            byteData[nNum++] = (byte)(nSize & 0xff);

            // Data
            byteData[nNum++] = (byte)(byteData1 & 0xff);
            byteData[nNum++] = (byte)(byteData2 & 0xff);
            byteData[nNum++] = (byte)(byteData3 & 0xff);

            // CheckSum
            byte byteCheckSum = byteData[2];
            for (i = 3; i < nPacketSize - 1; i++)
            {
                byteCheckSum ^= byteData[i];
            }
            byteData[nNum++] = (byte)(byteCheckSum & 0x7f);

            send(byteData);
#else
            int nSize = 3;
            byte[] byteArrayData = new byte[nSize];
            int i = 0;
            byteArrayData[i++] = byteData1;
            byteArrayData[i++] = byteData2;
            byteArrayData[i++] = byteData3;
            send_data(nCommand, nSize, byteArrayData);
            byteArrayData = null;
#endif
        }
        public void send_4_data(int nCommand, byte byteData1, byte byteData2, byte byteData3, byte byteData4)
        {
#if false
            // 시퀀스 이벤트 대기
            //set_busy();

            int i, nNum;

            // 프레임 수
            int nSize = 4;
            int nPacketSize = nSize + 8 + 2;
            byte[] byteData = new byte[nPacketSize];

            nNum = 0;
            byteData[nNum++] = 0xff;
            byteData[nNum++] = 0xff;
            // Code
            byteData[nNum++] = (byte)(_CODE_BLUETOOTH & 0xff);
            // ID
            byteData[nNum++] = (byte)(drbluetooth_get_id() & 0xff);
            // Cmd
            byteData[nNum++] = (Byte)(nCommand & 0xff);

            // Data Size
            byteData[nNum++] = (byte)((nSize >> 24) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 16) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 8) & 0xff);
            byteData[nNum++] = (byte)(nSize & 0xff);

            // Data
            byteData[nNum++] = (byte)(byteData1 & 0xff);
            byteData[nNum++] = (byte)(byteData2 & 0xff);
            byteData[nNum++] = (byte)(byteData3 & 0xff);
            byteData[nNum++] = (byte)(byteData4 & 0xff);

            // CheckSum
            byte byteCheckSum = byteData[2];
            for (i = 3; i < nPacketSize - 1; i++)
            {
                byteCheckSum ^= byteData[i];
            }
            byteData[nNum++] = (byte)(byteCheckSum & 0x7f);

            send(byteData);
#else
            int nSize = 4;
            byte[] byteArrayData = new byte[nSize];
            int i = 0;
            byteArrayData[i++] = byteData1;
            byteArrayData[i++] = byteData2;
            byteArrayData[i++] = byteData3;
            byteArrayData[i++] = byteData4;
            send_data(nCommand, nSize, byteArrayData);
            byteArrayData = null;
#endif
        }
        public void send_data(int nCommand, int nSize, byte[] byteArrayData)
        {
            int i, nNum;

            // 프레임 수
            int nPacketSize = nSize + 8;// +2;
            byte[] byteData = new byte[nPacketSize];

            int nCheckSumStartPos = 0;

            nNum = 0;
            byteData[nNum++] = 0xff;
            //byteData[nNum++] = 0xff;
            // Code
            //nCheckSumStartPos = nNum;
            //byteData[nNum++] = (byte)(_CODE_BLUETOOTH & 0xff);
            // ID
            nCheckSumStartPos = nNum;
            byteData[nNum++] = (byte)(drbluetooth_get_id() & 0xff);
            // Cmd
            byteData[nNum++] = (Byte)(nCommand & 0xff);

            // Data Size
            byteData[nNum++] = (byte)((nSize >> 24) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 16) & 0xff);
            byteData[nNum++] = (byte)((nSize >> 8) & 0xff);
            byteData[nNum++] = (byte)(nSize & 0xff);

            for (i = 0; i < nSize; i++)
            {
                byteData[nNum++] = byteArrayData[i];
            }

            // CheckSum
            byte byteCheckSum = byteData[nCheckSumStartPos++];
            for (i = nCheckSumStartPos; i < nPacketSize - 1; i++)
            {
                byteCheckSum ^= byteData[i];
            }
            byteData[nNum++] = (byte)(byteCheckSum & 0x7f);

            send(byteData);
        }


#if true
        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="nCommand"></param>
        public void drbluetooth_mpsu_send_command(int nCommand)
        {
            drbluetooth_mpsu_send_data(nCommand, 0, null);
        }
        public void drbluetooth_mpsu_send_1_data(int nCommand, byte byteOneData)
        {
            int nSize = 1;
            byte[] byteArrayData = new byte[nSize];
            int i = 0;
            byteArrayData[i++] = byteOneData;
            drbluetooth_mpsu_send_data(nCommand, nSize, byteArrayData);
            byteArrayData = null;
        }
        public void drbluetooth_mpsu_send_2_data(int nCommand, byte byteData1, byte byteData2)
        {
            int nSize = 2;
            byte[] byteArrayData = new byte[nSize];
            int i = 0;
            byteArrayData[i++] = byteData1;
            byteArrayData[i++] = byteData2;
            drbluetooth_mpsu_send_data(nCommand, nSize, byteArrayData);
            byteArrayData = null;
        }
        public void drbluetooth_mpsu_send_3_data(int nCommand, byte byteData1, byte byteData2, byte byteData3)
        {
            int nSize = 3;
            byte[] byteArrayData = new byte[nSize];
            int i = 0;
            byteArrayData[i++] = byteData1;
            byteArrayData[i++] = byteData2;
            byteArrayData[i++] = byteData3;
            drbluetooth_mpsu_send_data(nCommand, nSize, byteArrayData);
            byteArrayData = null;
        }
        public void drbluetooth_mpsu_send_4_data(int nCommand, byte byteData1, byte byteData2, byte byteData3, byte byteData4)
        {
            int nSize = 4;
            byte[] byteArrayData = new byte[nSize];
            int i = 0;
            byteArrayData[i++] = byteData1;
            byteArrayData[i++] = byteData2;
            byteArrayData[i++] = byteData3;
            byteArrayData[i++] = byteData4;
            drbluetooth_mpsu_send_data(nCommand, nSize, byteArrayData);
            byteArrayData = null;
        }

        private const int _MPSU_HEADER1 = 0;
        private const int _MPSU_HEADER2 = 1;
        private const int _MPSU_SIZE = 2;
        private const int _MPSU_ID = 3;
        private const int _MPSU_CMD = 4;
        private const int _MPSU_CHECKSUM1 = 5;
        private const int _MPSU_CHECKSUM2 = 6;
        private const int _MPSU_SIZE_PACKET_HEADER = 7;
        public void drbluetooth_mpsu_send_data(int nCommand, int nSize, byte[] byteArrayData)
        {
            int i;

            // 프레임 수
            int nPacketSize = nSize + _MPSU_SIZE_PACKET_HEADER;
            byte[] byteData = new byte[nPacketSize];

            int nCheckSumStartPos = 0;

            // Header
            byteData[_MPSU_HEADER1] = 0xff;
            byteData[_MPSU_HEADER2] = 0xff;
            // Data Size
            byteData[_MPSU_SIZE]    = (byte)(nPacketSize & 0xff);
            // ID
            byteData[_MPSU_ID]      = (byte)(drbluetooth_get_id() & 0xff);
            // Cmd
            byteData[_MPSU_CMD]     = (byte)(nCommand & 0xff);

            // 데이타 사이즈가 잘못된 건지 검증
            if (byteArrayData.Length < nSize) return;

            // Data
            for (i = 0; i < nSize; i++) byteData[_MPSU_SIZE_PACKET_HEADER + i] = byteArrayData[i];

            // CheckSum
            nCheckSumStartPos = byteData[_MPSU_SIZE] ^ byteData[_MPSU_ID] ^ byteData[_MPSU_CMD];
            for (i = _MPSU_SIZE_PACKET_HEADER; i < nPacketSize; i++) nCheckSumStartPos ^= byteData[i];
            // CheckSum
            byteData[_MPSU_CHECKSUM1] = (byte)(nCheckSumStartPos & 0xfe);
            byteData[_MPSU_CHECKSUM2] = (byte)(~nCheckSumStartPos & 0xfe);

            send(byteData);
        }
#endif
        #endregion SendData

        #region 실제 명령어
        private int m_nClient_Seq_Back = 0;
        private const int _TID_WAIT_TIMER = 50;
        private const int _WAIT_TIMER = 100;

        public bool is_busy()
        {
            if (connected() == _RESULT_FAIL) return false;// _RESULT_FAIL;
            return ((m_nClient_Seq_Back != m_nClient_Seq) ? false : true);
        }
        public bool wait()
        {
            return wait(_WAIT_TIMER);
        }
        public bool wait(int nMilliSeconds)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                CdrOjwTimer.TimerSet(_TID_WAIT_TIMER);
                bool bPass = _RESULT_FAIL;
                while (connected() == _RESULT_OK)
                {
                    //if (m_nClient_Seq_Back != m_nClient_Seq)
                    if (is_busy() == false)
                    {
                        bPass = _RESULT_OK;
                        // 1. 여기 나중에 시간값 오바해도 빠져나가게 바꿔야 한다. => 완료
                        // 2. 리턴시 process_ok 등의 값은 true, 아니면 false가 되게...   => 완료                     
                        break;
                    }
                    else if (CdrOjwTimer.Timer(_TID_WAIT_TIMER) > nMilliSeconds) break;
                    //else if (CdrOjwTimer.Timer(_TID_WAIT_TIMER) > _WAIT_TIMER) break;

                    Application.DoEvents();
                }
                return bPass;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        // 0 : master, 1 : user
        public int get_FileList_Count(int nMaster_User) { return (nMaster_User == 0) ? m_nFileList_master : m_nFileList_user; }
        public String get_FileList(int nMaster_User, int nFileIndex)
        {
            byte[] pbyteData = new byte[_SIZE_FILENAME];
            int nSize = (nMaster_User == 0) ? m_nFileList_master : m_nFileList_user;
            if (nFileIndex < nSize)
            {
                Array.Copy(((nMaster_User == 0) ? m_pbyteFileList_master : m_pbyteFileList_user), nFileIndex * _SIZE_FILENAME, pbyteData, 0, _SIZE_FILENAME);
                return System.Text.Encoding.Default.GetString(pbyteData);   //pbyteData.ToString();
            }
            return null;
        }

        public bool get_is_playing() { return m_bPlaying; }

        public int get_time_sec() { return m_nTimeSec; }
        public int get_time_usec() { return m_nTimeUSec; }
        public int get_battery_robot() { return m_nRobotBatt; }
        public int get_battery_MID() { return m_nMIDBatt; }
        public int get_remocon_length() { return m_nRmcLength; }
        public int get_remocon_data() { return m_nRmcData; }
        public void set_busy() { m_nClient_Seq_Back = m_nClient_Seq; }
        private bool m_bIgnoreParser = false;
        public void set_IgnoreParser(bool bIgnore) { m_bIgnoreParser = bIgnore; }
        public bool get_IgnoreParser() { return m_bIgnoreParser; }
        private byte[] m_pbytePacketData;
        private int m_nPacket_Cmd = -1;
        private int m_nPacket_Length = -1;
        private int m_nPacket_Pos = -1;
        private int m_nPacket_ID = -1;
        private int m_nPacket_Checksum1 = -1;
        private int m_nPacket_Checksum2 = -1;
        public bool get_packetdata(out int nCmd, out int nLength, out byte[] pbyteData)
        {
            bool bRet = false;
            nCmd = m_nPacket_Cmd;
            nLength = m_nPacket_Length;
            pbyteData = null;
            if (nCmd < 0) return false;
            if (nLength < 0) return false;
            else if (nLength == 0) return true;
            else //if (nLength > 0)
            {
                if (m_pbytePacketData != null)
                {
                    if (nLength == m_pbytePacketData.Length)
                    {
                        pbyteData = new byte[m_pbytePacketData.Length];
                        Array.Copy(m_pbytePacketData, pbyteData, m_pbytePacketData.Length);
                        bRet = true;
                    }
                }
            }
            return bRet;
        }
        #region Request
        //public const int _REQUEST_OK = 0;
        //public const int _REQUEST_FAIL = -1;

        public bool request_auth(String strID, String strPassWord)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                int i;
                int nSize1 = 21;
                int nSize2 = 21;// 11;
                int nSize = nSize1 + nSize2;
                byte[] byteData = new byte[nSize];
                for (i = 0; i < nSize; i++)
                {
                    byteData[i] = 0;
                }
                byte[] byteId = Encoding.Default.GetBytes(strID);
                for (i = 0; i < byteId.Length; i++)
                {
                    if (i > (nSize1 - 1)) break;
                    byteData[i] = byteId[i];
                }
                byte[] bytePasswd = Encoding.Default.GetBytes(strPassWord);
                for (i = 0; i < bytePasswd.Length; i++)
                {
                    if (i > (nSize2 - 1)) break;
                    byteData[i + nSize1] = bytePasswd[i];
                }

                send_data(0x71, nSize, byteData);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_heartbeat()
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                send_command(0xF1);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_task_play(int nMaster_or_User, String strTaskFileName)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                //set_busy();

                int i;
                int nSize = _SIZE_FILENAME + 1;
                byte[] byteData = new byte[nSize];
                for (i = 0; i < nSize; i++)
                {
                    byteData[i] = 0;
                }
                byteData[0] = (byte)(nMaster_or_User & 0xff);
                byte[] byteId = Encoding.Default.GetBytes(strTaskFileName);
                for (i = 0; i < byteId.Length; i++)
                {
                    if (i >= (nSize - 1)) break;
                    byteData[i + 1] = byteId[i];
                }

                send_data(0x40, nSize, byteData);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public bool request_task_is_playing()
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                send_command(0x41);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public bool request_task_stop()
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                //set_busy();

                send_command(0x42);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public bool Action_Play(int nMaster_or_User, string strActFileName) { return request_motion_play(nMaster_or_User, strActFileName); }
        public bool request_motion_play(int nMaster_or_User, String strMotionFileName)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                int i;
                int nSize = _SIZE_FILENAME + 9;
                byte[] byteData = new byte[nSize];
                for (i = 0; i < nSize; i++)
                {
                    byteData[i] = 0;
                }
                byteData[0] = (byte)(nMaster_or_User & 0xff);
                byte[] byteId = Encoding.Default.GetBytes(strMotionFileName);
                for (i = 0; i < byteId.Length; i++)
                {
                    if (i >= _SIZE_FILENAME - 1) break;
                    byteData[i + 1] = byteId[i];
                }
                byteData[_SIZE_FILENAME] = (byte)'\0';

                byte[] byte_sec = { 0, 0, 0, 0 };
                byte[] byte_usec = { 0, 0, 0, 0 };
                Array.Copy(byte_sec, 0, byteData, _SIZE_FILENAME + 1, 4);
                Array.Copy(byte_usec, 0, byteData, _SIZE_FILENAME + 5, 4);

                send_data(0x40, nSize, byteData);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
#if true
#if false
        public bool drbluetooth_motor_request_drvsrv(bool bDriver, bool bServo)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                byte byOn = 0;
                byOn |= (byte)((bDriver == true) ? 0x40 : 0x00);
                byOn |= (byte)((bServo == true) ? 0x20 : 0x00);
                // 0x52 -> _ADDRESS_TORQUE_CONTROL
                drbluetooth_mpsu_send_3_data(0x03, (byte)(52), (byte)(0x01), (byte)(byOn & 0xff));
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
#endif
        public bool drbluetooth_mpsu_request_battery()
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                byte byAddress = 70;
                byte byLength = 7;
                // 시퀀스 이벤트 대기
                set_busy();
                drbluetooth_mpsu_send_2_data(0x14, byAddress, byLength);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public bool drbluetooth_mpsu_motor_request_move(int nTime)
        {
            if ((m_bStop == true) || (m_bEms == true)) return _RESULT_FAIL;
            int nID;
            int i = 0;
            ////////////////////////////////////////////////


            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                byte[] pbyteBuffer = new byte[1 + 4 * m_nMotor_Max];
                int nPos;
                int nFlag;

                #region S-Jog Time
                int nCalcTime = CalcTime_ms(nTime);
                pbyteBuffer[i++] = (byte)(nCalcTime & 0xff);
                #endregion S-Jog Time

                for (int nAxis = 0; nAxis < m_nMotor_Max; nAxis++)
                {
                    if (m_pSMot[nAxis].bEn == true)
                    {
                        //nPos |= _JOG_MODE_SPEED << 10;  // 속도제어 
                        #region Position
                        nPos = get_cmd(nAxis);
                        pbyteBuffer[i++] = (byte)(nPos & 0xff);
                        pbyteBuffer[i++] = (byte)((nPos >> 8) & 0xff);
                        #endregion

                        #region Set-Flag
                        nFlag = get_cmd_flag(nAxis);
                        pbyteBuffer[i++] = (byte)(nFlag & 0xff);
                        set_cmd_flag_no_action(nAxis, true); // 동작 후 모터 NoAction을 살려둔다.
                        #endregion Set-Flag

                        #region 모터당 아이디(후면에 붙는다)
                        nID = GetID_By_Axis(nAxis);
                        pbyteBuffer[i++] = (byte)(nID & 0xff);
                        #endregion 모터당 아이디(후면에 붙는다)
                        ////////////////////////////////////////////////
                    }
                    m_pSMot[nAxis].bEn = false;
                }
                Make_And_Send_Packet((byte)(drbluetooth_get_id() & 0xff), 0x0f, i, pbyteBuffer);
                pbyteBuffer = null;
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public bool drbluetooth_mpsu_request_motion_play(int nMotionIndex, bool bOnlyReadyPos)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                drbluetooth_mpsu_send_2_data(0x16, (byte)(nMotionIndex & 0xff), (byte)((bOnlyReadyPos == true) ? 1 : 0));
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public bool drbluetooth_mpsu_request_motion_stop()
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                drbluetooth_mpsu_send_2_data(0x16, (byte)(0xfe), 0);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public void Cmd_Bluetooth_Play_Mpsu(int nRobot, int nMotionNumber)
        {
            if (nRobot == 0xfe) drbluetooth_set_id(0xfe);
            else drbluetooth_set_id(nRobot);
            drbluetooth_mpsu_request_motion_play(nMotionNumber, false);
        }
        public void Cmd_Bluetooth_Play(int nRobot, int nMaster, String strFileName)
        {
            //if (nRobot == 0xfe) nRobot = 0xf2;
            //if (nRobot == 0xf2) m_DrBluetooth.drbluetooth_set_id(0xf2);
            if (nRobot == 0xfe) drbluetooth_set_id(0xfe);
            else drbluetooth_set_id(nRobot);
            request_motion_play_now(nMaster, strFileName);
        }
        public void Cmd_Bluetooth_Play(int nRobot, int nMaster, String strFileName, DateTime dateTime, int nAddMillisecond)
        {
            //if (nRobot == 0xfe) nRobot = 0xf2;
            //if (nRobot == 0xf2) m_DrBluetooth.drbluetooth_set_id(0xf2);
            if (nRobot == 0xfe) drbluetooth_set_id(0xfe);
            else drbluetooth_set_id(nRobot);
            //m_DrBluetooth.drbluetooth_set_id(m_pnBluetoothAddress[nRobot]);
            request_motion_play(nMaster, strFileName, dateTime, nAddMillisecond);
        }
        public bool drbluetooth_mpsu_request_motion_play_reserve(int nMotionIndex, bool bOnlyReadyPos, int nMillisecond)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                drbluetooth_mpsu_send_4_data(0x16, (byte)(nMotionIndex & 0xff), (byte)((bOnlyReadyPos == true) ? 1 : 0), (byte)(nMillisecond & 0xff), (byte)((nMillisecond >> 8) & 0xff));
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }        
#endif

        public bool request_motion_play(int nMaster_or_User, String strMotionFileName, DateTime dtTime, int nAddMillisecond)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                int i;
                int nSize = _SIZE_FILENAME + 9;
                byte[] byteData = new byte[nSize];
                for (i = 0; i < nSize; i++)
                {
                    byteData[i] = 0;
                }
                byteData[0] = (byte)(nMaster_or_User & 0xff);
                byte[] byteId = Encoding.Default.GetBytes(strMotionFileName);
                for (i = 0; i < byteId.Length; i++)
                {
                    if (i >= _SIZE_FILENAME - 1) break;
                    byteData[i + 1] = byteId[i];
                }
                byteData[_SIZE_FILENAME] = (byte)'\0';

                if (dtTime != null)
                    dtTime = dtTime.AddMilliseconds((double)nAddMillisecond);

                long lTime = (dtTime == null) ? 0 : dtTime.ToFileTime();
                byte[] byte_sec = new byte[4];
                byte[] byte_usec = new byte[4];
                // Windows time to Unix Time
                long lCurr = (dtTime == null) ? 0 : (long)(lTime / 10000000 - 11644473600);
                long lUsec = (dtTime == null) ? 0 : (long)((lTime / 10) % 1000000);

                byte_sec[0] = (byte)((lCurr >> 24) & 0xff);
                byte_sec[1] = (byte)((lCurr >> 16) & 0xff);
                byte_sec[2] = (byte)((lCurr >> 8) & 0xff);
                byte_sec[3] = (byte)((lCurr >> 0) & 0xff);

                byte_usec[0] = (byte)((lUsec >> 24) & 0xff);
                byte_usec[1] = (byte)((lUsec >> 16) & 0xff);
                byte_usec[2] = (byte)((lUsec >> 8) & 0xff);
                byte_usec[3] = (byte)((lUsec >> 0) & 0xff);

                Array.Copy(byte_sec, 0, byteData, _SIZE_FILENAME + 1, 4);
                Array.Copy(byte_usec, 0, byteData, _SIZE_FILENAME + 5, 4);

                send_data(0x40, nSize, byteData);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_motion_play_now(int nMaster_or_User, String strMotionFileName)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                int i;
                int nSize = _SIZE_FILENAME + 9;
                byte[] byteData = new byte[nSize];
                for (i = 0; i < nSize; i++)
                {
                    byteData[i] = 0;
                }
                byteData[0] = (byte)(nMaster_or_User & 0xff);
                byte[] byteId = Encoding.Default.GetBytes(strMotionFileName);
                for (i = 0; i < byteId.Length; i++)
                {
                    if (i >= _SIZE_FILENAME - 1) break;
                    byteData[i + 1] = byteId[i];
                }
                byteData[_SIZE_FILENAME] = (byte)'\0';

                byte[] byte_sec = new byte[4];
                byte[] byte_usec = new byte[4];

                Array.Clear(byte_sec, 0, 4);
                Array.Clear(byte_usec, 0, 4);

                Array.Copy(byte_sec, 0, byteData, _SIZE_FILENAME + 1, 4);
                Array.Copy(byte_usec, 0, byteData, _SIZE_FILENAME + 5, 4);

                send_data(0x40, nSize, byteData);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_motion_is_playing()
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                send_command(0x41);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_motion_stop()
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                //set_busy();

                send_command(0x42);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_filelist(int nMaster_or_User)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                send_1_data(0x31, (byte)(nMaster_or_User & 0xff));
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        // 0:master, 1:user,    전체경로를 포함한 보낼 파일,       저장될 이름
        public bool request_file_download(int n_0_Master_or_1_User, String strFile_Path_Name_Exe_in_PC, String strFileName_in_Robot)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                if ((n_0_Master_or_1_User < 0) || (n_0_Master_or_1_User > 1)) return _RESULT_FAIL;
                else if (strFileName_in_Robot == null) return _RESULT_FAIL;
                else if (strFile_Path_Name_Exe_in_PC == null) return _RESULT_FAIL;
                else if (strFileName_in_Robot.Length <= 0) return _RESULT_FAIL;

                // 시퀀스 이벤트 대기
                set_busy();

                bool bRet = _RESULT_OK;

                FileStream fs = null;
                byte[] pbyteFile;
                byte[] pbyteBuffer;

                FileInfo fileInfo = new FileInfo(strFile_Path_Name_Exe_in_PC);
                if (fileInfo.Exists == true)
                {
                    fs = fileInfo.OpenRead();
                    pbyteFile = new byte[fs.Length];
                    fs.Read(pbyteFile, 0, pbyteFile.Length);
                    fs.Close();

                    pbyteBuffer = new byte[5 + _SIZE_FILENAME + pbyteFile.Length];

                    byte[] pbyteFileName = Encoding.Default.GetBytes(strFileName_in_Robot);
                    pbyteBuffer[0] = (byte)(n_0_Master_or_1_User & 0xff);
                    pbyteBuffer[1] = 0;
                    pbyteBuffer[2] = 0;
                    pbyteBuffer[3] = 0;
                    pbyteBuffer[4] = 1;
                    int nFileNameSize = (pbyteFileName.Length >= _SIZE_FILENAME) ? _SIZE_FILENAME - 1 : pbyteFileName.Length;
                    Array.Copy(pbyteFileName, 0, pbyteBuffer, 5, nFileNameSize);

                    pbyteBuffer[4 + _SIZE_FILENAME] = (byte)('\0'); // 5 + (_SIZE_FILENAME - 1) 번지에 기록
                    Array.Copy(pbyteFile, 0, pbyteBuffer, 5 + _SIZE_FILENAME, pbyteFile.Length);

                    send_data(0x32, 5 + _SIZE_FILENAME + pbyteFile.Length, pbyteBuffer);

                    pbyteBuffer = null;
                    pbyteFile = null;
                }
                else bRet = _RESULT_FAIL;

                return bRet;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public bool request_file_download(int n_0_Master_or_1_User, int nIndexCurrFile, int nNumTotalFile, String strFile_Path_Name_Exe_in_PC, String strFileName_in_Robot)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                if ((n_0_Master_or_1_User < 0) || (n_0_Master_or_1_User > 1)) return _RESULT_FAIL;
                else if (strFileName_in_Robot == null) return _RESULT_FAIL;
                else if (strFile_Path_Name_Exe_in_PC == null) return _RESULT_FAIL;
                else if (strFileName_in_Robot.Length <= 0) return _RESULT_FAIL;

                // 시퀀스 이벤트 대기
                set_busy();

                bool bRet = _RESULT_OK;

                FileStream fs = null;
                byte[] pbyteFile;
                byte[] pbyteBuffer;

                FileInfo fileInfo = new FileInfo(strFile_Path_Name_Exe_in_PC);
                if (fileInfo.Exists == true)
                {
                    fs = fileInfo.OpenRead();
                    pbyteFile = new byte[fs.Length];
                    fs.Read(pbyteFile, 0, pbyteFile.Length);
                    fs.Close();

                    pbyteBuffer = new byte[5 + _SIZE_FILENAME + pbyteFile.Length];

                    byte[] pbyteFileName = Encoding.Default.GetBytes(strFileName_in_Robot);
                    pbyteBuffer[0] = (byte)(n_0_Master_or_1_User & 0xff);
                    pbyteBuffer[1] = (byte)((nIndexCurrFile >> 8) & 0xff);
                    pbyteBuffer[2] = (byte)(nIndexCurrFile & 0xff);
                    pbyteBuffer[3] = (byte)((nNumTotalFile >> 8) & 0xff);
                    pbyteBuffer[4] = (byte)(nNumTotalFile & 0xff);
                    int nFileNameSize = (pbyteFileName.Length >= _SIZE_FILENAME) ? _SIZE_FILENAME - 1 : pbyteFileName.Length;
                    Array.Copy(pbyteFileName, 0, pbyteBuffer, 5, nFileNameSize);

                    pbyteBuffer[4 + _SIZE_FILENAME] = (byte)('\0'); // 5 + (_SIZE_FILENAME - 1) 번지에 기록
                    Array.Copy(pbyteFile, 0, pbyteBuffer, 5 + _SIZE_FILENAME, pbyteFile.Length);

                    send_data(0x32, 5 + _SIZE_FILENAME + pbyteFile.Length, pbyteBuffer);

                    pbyteBuffer = null;
                    pbyteFile = null;
                }
                else bRet = _RESULT_FAIL;

                return bRet;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public bool request_file_delete(int nMaster_or_User, String strTaskFileName)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                int i;
                int nSize = _SIZE_FILENAME + 1;
                byte[] byteData = new byte[nSize];
                for (i = 0; i < nSize; i++)
                {
                    byteData[i] = 0;
                }
                byteData[0] = (byte)(nMaster_or_User & 0xff);
                byte[] byteId = Encoding.Default.GetBytes(strTaskFileName);
                for (i = 0; i < byteId.Length; i++)
                {
                    if (i >= (nSize - 1)) break;
                    byteData[i + 1] = byteId[i];
                }

                send_data(0x34, nSize, byteData);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }
        public bool request_file_delete(int nMaster_or_User)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                send_1_data(0x35, (byte)(nMaster_or_User & 0xff));
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_file_upload(int nMaster_or_User, String strTaskFileName)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                int i;
                int nSize = _SIZE_FILENAME + 1;
                byte[] byteData = new byte[nSize];
                for (i = 0; i < nSize; i++)
                {
                    byteData[i] = 0;
                }
                byteData[0] = (byte)(nMaster_or_User & 0xff);
                byte[] byteId = Encoding.Default.GetBytes(strTaskFileName);
                for (i = 0; i < byteId.Length; i++)
                {
                    if (i >= (nSize - 1)) break;
                    byteData[i + 1] = byteId[i];
                }

                send_data(0x33, nSize, byteData);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_set_time(DateTime dtTime)
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                int i;
                int nSize = 8;
                byte[] byteData = new byte[nSize];
                for (i = 0; i < nSize; i++)
                {
                    byteData[i] = 0;
                }


                //dtTime = dtTime.AddHours(9.0); // 9시간을 더해보자
                //dtTime = dtTime.AddMinutes(20.0); // 20분을 더해보자
                long lTime = dtTime.ToFileTime();
                // Windows time to Unix Time
                long lCurr = (long)(lTime / 10000000 - 11644473600);
                long lUsec = (long)((lTime / 10) % 1000000);

                byteData[0] = (byte)((lCurr >> 24) & 0xff);
                byteData[1] = (byte)((lCurr >> 16) & 0xff);
                byteData[2] = (byte)((lCurr >> 8) & 0xff);
                byteData[3] = (byte)((lCurr >> 0) & 0xff);

                byteData[4] = (byte)((lUsec >> 24) & 0xff);
                byteData[5] = (byte)((lUsec >> 16) & 0xff);
                byteData[6] = (byte)((lUsec >> 8) & 0xff);
                byteData[7] = (byte)((lUsec >> 0) & 0xff);

                send_data(0x61, nSize, byteData);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_time()
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                send_command(0x62);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_battery()
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                send_command(0x63);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_charge()
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                send_command(0x64);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        public bool request_remocon()
        {
            if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
            try
            {
                // 시퀀스 이벤트 대기
                set_busy();

                send_command(0x65);
                return _RESULT_OK;
            }
            catch
            {
                return _RESULT_FAIL;
            }
        }

        //private int m_nCurrentPort = 0;
        //private String m_strCurrentIp = "";
        //public int get_port() { return m_nCurrentPort; }
        //public String get_ip() { return m_strCurrentIp; }
        #endregion Request

            #region For Serial ...
            #region Define Serial Command
            private const int _ADDRESS_TORQUE_CONTROL = 52;
            private const int _ADDRESS_LED_CONTROL = 53;
            private const int _ADDRESS_VOLTAGE = 54;
            private const int _ADDRESS_TEMPERATURE = 55;
            private const int _ADDRESS_PRESENT_CONTROL_MODE = 56;
            private const int _ADDRESS_TICK = 57;
            private const int _ADDRESS_CALIBRATED_POSITION = 58;
            #endregion Define Serial Command
            #region 패킷 정의(Header 등...)
                private const int _HEADER1 = 0;
                private const int _HEADER2 = 1;
                private const int _SIZE = 2;
                private const int _ID = 3;
                private const int _CMD = 4;
                private const int _CHECKSUM1 = 5;
                private const int _CHECKSUM2 = 6;
                private const int _SIZE_PACKET_HEADER = 7;

                private const int _FLAG_STOP = 0x01;
                private const int _FLAG_MODE_SPEED = 0x02;
                private const int _FLAG_LED_GREEN = 0x04;
                private const int _FLAG_LED_BLUE = 0x08;
                private const int _FLAG_LED_RED = 0x10;
                private const int _FLAG_NO_ACTION = 0x20;
                #endregion
            // 체크섬 데이타 만들기
            public void serial_make_checksum(int nAllPacketLength, byte[] buffer)
            {
                int nHeadSize = _CHECKSUM2 + 1;
                buffer[_CHECKSUM1] = (byte)(buffer[_SIZE] ^ buffer[_ID] ^ buffer[_CMD]);
                for (int j = 0; j < nAllPacketLength - nHeadSize; j++)
                {
                    buffer[_CHECKSUM1] ^= buffer[nHeadSize + j];
                }
                buffer[_CHECKSUM1] = (byte)(buffer[_CHECKSUM1] & 0xfe);
                buffer[_CHECKSUM2] = (byte)(~buffer[_CHECKSUM1] & 0xfe);
            }
            // 패킷의 아이디와 커맨드로 데이타 만들기, 내부의 패킷사이즈 - 기본사이즈인 7바이트는 제외
            public void Make_And_Send_Packet(int nID, int nCmd, int nDataByteSize, byte[] pbytePacket)
            {
                int nDefaultSize = _CHECKSUM2 + 1;
                int nSize = nDefaultSize + nDataByteSize;

                byte[] pbyteBuffer = new byte[nSize];

                int i = 0;
                // Header
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = (byte)(nCmd & 0xff);
                /////////////////////////////////////////////////////
                for (int j = 0; j < nDataByteSize; j++) pbyteBuffer[nDefaultSize + i++] = pbytePacket[j];
                //for (int j = 0; j < nDataByteSize; j++) pbyteBuffer[nDefaultSize - 2 + i++] = pbytePacket[j];
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                serial_make_checksum(nDefaultSize + i, pbyteBuffer);

                //SendPacket(pbyteBuffer, nDefaultSize + i);
                send_data(0x81, nDefaultSize + i, pbyteBuffer);

                pbyteBuffer = null;
            }

            #region MPSU
            public bool serial_mpsu_play_headled_buzz(int nMpsuID, int nHeadLedNum_1_63, int nBuzzNum_1_63)
            {
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
                try
                {
                    int nDefaultSize = _CHECKSUM2 + 1;

                    int i = 0;
                    // Header
                    byte[] pbyteBuffer = new byte[256];
                    pbyteBuffer[_HEADER1] = 0xff;
                    pbyteBuffer[_HEADER2] = 0xff;
                    // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                    pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                    // Cmd
                    pbyteBuffer[_CMD] = 0x18; // Play Buzz

                    /////////////////////////////////////////////////////
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nHeadLedNum_1_63 & 0xff);

                    ////////
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nBuzzNum_1_63 & 0xff);// 이후의 레지스터 사이즈
                    ////////
                    /////////////////////////////////////////////////////

                    //Packet Size
                    pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                    serial_make_checksum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                    // 보내기 전에 Tick 을 Set 한다.
                    //Tick_Send_Mpsu();
                    //SendPacket(pbyteBuffer, nDefaultSize + i);

                    send_data(0x81, nDefaultSize + i, pbyteBuffer);

                    return _RESULT_OK;
                }
                catch
                {
                    return _RESULT_FAIL;
                }
            }
            public bool serial_mpsu_play_headled(int nMpsuID, int nHeadLedNum_1_63)
            {
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
                try
                {
                    int nDefaultSize = _CHECKSUM2 + 1;

                    int i = 0;
                    // Header
                    byte[] pbyteBuffer = new byte[256];
                    pbyteBuffer[_HEADER1] = 0xff;
                    pbyteBuffer[_HEADER2] = 0xff;
                    // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                    pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                    // Cmd
                    pbyteBuffer[_CMD] = 0x18; // Play Buzz

                    /////////////////////////////////////////////////////
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nHeadLedNum_1_63 & 0xff);

                    ////////
                    pbyteBuffer[nDefaultSize + i++] = 0x00;
                    ////////
                    /////////////////////////////////////////////////////

                    //Packet Size
                    pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                    serial_make_checksum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                    // 보내기 전에 Tick 을 Set 한다.
                    //Tick_Send_Mpsu();
                    //SendPacket(pbyteBuffer, nDefaultSize + i);

                    send_data(0x81, nDefaultSize + i, pbyteBuffer);

                    return _RESULT_OK;
                }
                catch
                {
                    return _RESULT_FAIL;
                }
            }
            public bool serial_mpsu_play_buzz(int nMpsuID, int nBuzzNum_1_63)
            {
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
                try
                {
                    int nDefaultSize = _CHECKSUM2 + 1;

                    int i = 0;
                    // Header
                    byte[] pbyteBuffer = new byte[256];
                    pbyteBuffer[_HEADER1] = 0xff;
                    pbyteBuffer[_HEADER2] = 0xff;
                    // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                    pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                    // Cmd
                    pbyteBuffer[_CMD] = 0x18; // Play Buzz

                    /////////////////////////////////////////////////////
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = 0x00;

                    ////////
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nBuzzNum_1_63 & 0xff);// 이후의 레지스터 사이즈
                    ////////
                    /////////////////////////////////////////////////////

                    //Packet Size
                    pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                    serial_make_checksum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                    // 보내기 전에 Tick 을 Set 한다.
                    //Tick_Send_Mpsu();
                    //SendPacket(pbyteBuffer, nDefaultSize + i);

                    send_data(0x81, nDefaultSize + i, pbyteBuffer);

                    return _RESULT_OK;
                }
                catch
                {
                    return _RESULT_FAIL;
                }
            }
            public bool serial_mpsu_play_task(int nMpsuID, int nTaskNo)//, bool bDebugingMode)
            {
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
                try
                {
                    int nDefaultSize = _CHECKSUM2 + 1;

                    int i = 0;
                    // Header
                    byte[] pbyteBuffer = new byte[256];
                    pbyteBuffer[_HEADER1] = 0xff;
                    pbyteBuffer[_HEADER2] = 0xff;
                    // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                    pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                    // Cmd
                    pbyteBuffer[_CMD] = 0x17;

                    /////////////////////////////////////////////////////
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nTaskNo & 0xff);

                    ////////
                    //pbyteBuffer[nDefaultSize + i++] = (byte)Ojw.CConvert.BoolToInt(bDebugingMode);// 이후의 레지스터 사이즈
                    ////////
                    /////////////////////////////////////////////////////

                    //Packet Size
                    pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                    serial_make_checksum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                    // 보내기 전에 Tick 을 Set 한다.
                    //Tick_Send_Mpsu();
                    send_data(0x81, nDefaultSize + i, pbyteBuffer);
                    return _RESULT_OK;
                }
                catch
                {
                    return _RESULT_FAIL;
                }
            }
            public bool serial_mpsu_play_motion(int nMpsuID, int nMotionAddress, bool bReady)
            {
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
                try
                {
                    int nDefaultSize = _CHECKSUM2 + 1;

                    int i = 0;
                    // Header
                    byte[] pbyteBuffer = new byte[256];
                    pbyteBuffer[_HEADER1] = 0xff;
                    pbyteBuffer[_HEADER2] = 0xff;
                    // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                    pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                    // Cmd
                    pbyteBuffer[_CMD] = 0x16;

                    /////////////////////////////////////////////////////
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nMotionAddress & 0xff);

                    ////////
                    pbyteBuffer[nDefaultSize + i++] = (byte)((bReady == true) ? 1 : 0);//Ojw.CConvert.BoolToInt(bReady);// 이후의 레지스터 사이즈
                    ////////
                    /////////////////////////////////////////////////////

                    //Packet Size
                    pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                    serial_make_checksum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                    // 보내기 전에 Tick 을 Set 한다.
                    //Tick_Send_Mpsu();
                    send_data(0x81, nDefaultSize + i, pbyteBuffer);
                    return _RESULT_OK;
                }
                catch
                {
                    return _RESULT_FAIL;
                }
            }
            #endregion MPSU

            public bool drvsrv(bool bDriver, bool bServo)
            {
                //if ((m_bEms == true) && (bDriver == false) && (bServo == false)) return;
                return drvsrv(0xfe, bDriver, bServo);
            }

            public bool drvsrv(int nAxis, bool bDriver, bool bServo)
            {
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
                try
                {
                    //if ((m_bEms == true) && (bDriver == false) && (bServo == false)) return;
                    int nID = GetID_By_Axis(nAxis);//m_pSMot[nAxis].nID;
                    int i = 0;
                    byte byOn = 0;
                    byOn |= (byte)((bDriver == true) ? 0x40 : 0x00);
                    byOn |= (byte)((bServo == true) ? 0x20 : 0x00);
                    byte[] pbyteBuffer = new byte[256];
                    // Data
                    pbyteBuffer[i++] = _ADDRESS_TORQUE_CONTROL;// 52번 레지스터 명령
                    ////////
                    pbyteBuffer[i++] = 0x01;// 이후의 레지스터 사이즈
                    pbyteBuffer[i++] = byOn;

                    Make_And_Send_Packet(nID, 0x03, i, pbyteBuffer);
                    pbyteBuffer = null;
                    return _RESULT_OK;
                }
                catch
                {
                    return _RESULT_FAIL;
                }
            }

            #region For Rom
            public bool serial_mpsu_write_rom(int nMpsuID, int nStartAddress, byte byteData)
            {
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
                try
                {
                    int nDefaultSize = _CHECKSUM2 + 1;

                    int i = 0;
                    // Header
                    byte[] pbyteBuffer = new byte[256];
                    pbyteBuffer[_HEADER1] = 0xff;
                    pbyteBuffer[_HEADER2] = 0xff;
                    // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                    pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                    // Cmd
                    pbyteBuffer[_CMD] = 0x11; // Rom Write

                    /////////////////////////////////////////////////////
                    // Address
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nStartAddress & 0xff);
                    // Length
                    pbyteBuffer[nDefaultSize + i++] = (byte)(1); // 한바이트만 보냄
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = (byte)(byteData);

                    ////////
                    /////////////////////////////////////////////////////

                    //Packet Size
                    pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                    serial_make_checksum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                    // 보내기 전에 Tick 을 Set 한다.
                    //Tick_Send_Mpsu();
                    send_data(0x81, nDefaultSize + i, pbyteBuffer);
                    return _RESULT_OK;
                }
                catch
                {
                    return _RESULT_FAIL;
                }
            }
            public bool serial_mpsu_write_rom(int nMpsuID, int nStartAddress, byte[] pbyteData)
            {
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
                try
                {
                    int nDefaultSize = _CHECKSUM2 + 1;

                    int nLength = pbyteData.Length;

                    int i = 0;
                    // Header
                    byte[] pbyteBuffer = new byte[256];
                    pbyteBuffer[_HEADER1] = 0xff;
                    pbyteBuffer[_HEADER2] = 0xff;
                    // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                    pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                    // Cmd
                    pbyteBuffer[_CMD] = 0x11; // Rom Write

                    /////////////////////////////////////////////////////
                    // Address
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nStartAddress & 0xff);
                    // Length
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nLength & 0xff); // 한바이트만 보냄

                    for (int nData = 0; nData < nLength; nData++)
                    {
                        // Data
                        pbyteBuffer[nDefaultSize + i++] = (byte)(pbyteData[nData]);
                    }

                    ////////
                    /////////////////////////////////////////////////////

                    //Packet Size
                    pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                    serial_make_checksum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                    // 보내기 전에 Tick 을 Set 한다.
                    //Tick_Send_Mpsu();
                    send_data(0x81, nDefaultSize + i, pbyteBuffer);
                    return _RESULT_OK;
                }
                catch
                {
                    return _RESULT_FAIL;
                }
            }
            #endregion For Rom

            #region For Ram
            public bool serial_mpsu_write_ram(int nMpsuID, int nStartAddress, byte byteData)
            {
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
                try
                {
                    int nDefaultSize = _CHECKSUM2 + 1;

                    int i = 0;
                    // Header
                    byte[] pbyteBuffer = new byte[256];
                    pbyteBuffer[_HEADER1] = 0xff;
                    pbyteBuffer[_HEADER2] = 0xff;
                    // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                    pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                    // Cmd
                    pbyteBuffer[_CMD] = 0x13; // Ram Write

                    /////////////////////////////////////////////////////
                    // Address
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nStartAddress & 0xff);
                    // Length
                    pbyteBuffer[nDefaultSize + i++] = (byte)(1); // 한바이트만 보냄
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = (byte)(byteData);

                    ////////
                    /////////////////////////////////////////////////////

                    //Packet Size
                    pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                    serial_make_checksum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                    // 보내기 전에 Tick 을 Set 한다.
                    //Tick_Send_Mpsu();
                    send_data(0x81, nDefaultSize + i, pbyteBuffer);
                    return _RESULT_OK;
                }
                catch
                {
                    return _RESULT_FAIL;
                }
            }
            public bool serial_mpsu_write_ram(int nMpsuID, int nStartAddress, byte[] pbyteData)
            {
                if (connected() == _RESULT_FAIL) return _RESULT_FAIL;
                try
                {
                    int nDefaultSize = _CHECKSUM2 + 1;

                    int nLength = pbyteData.Length;

                    int i = 0;
                    // Header
                    byte[] pbyteBuffer = new byte[256];
                    pbyteBuffer[_HEADER1] = 0xff;
                    pbyteBuffer[_HEADER2] = 0xff;
                    // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                    pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                    // Cmd
                    pbyteBuffer[_CMD] = 0x13; // Ram Write

                    /////////////////////////////////////////////////////
                    // Address
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nStartAddress & 0xff);
                    // Length
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nLength); // 한바이트만 보냄

                    for (int nData = 0; nData < nLength; nData++)
                    {
                        // Data
                        pbyteBuffer[nDefaultSize + i++] = (byte)(pbyteData[nData]);
                    }

                    ////////
                    /////////////////////////////////////////////////////

                    //Packet Size
                    pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                    serial_make_checksum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                    // 보내기 전에 Tick 을 Set 한다.
                    //Tick_Send_Mpsu();
                    send_data(0x81, nDefaultSize + i, pbyteBuffer);
                    return _RESULT_OK;
                }
                catch
                {
                    return _RESULT_FAIL;
                }
            }
            #endregion For Ram
            #endregion For Serial ...

        #endregion 실제명령어

        #endregion 공개

        #region Timer
        private class CdrOjwTimer
        {
            private static bool[] m_bTimer = new bool[1000];

            private static long[] m_Timer = new long[1000];

            public static void InitTimer(int nCnt) // 안하면 기존 1000 개의 메모리로 고정된다.
            {
                Array.Resize<bool>(ref m_bTimer, nCnt);
                Array.Resize<long>(ref m_Timer, nCnt);
                Array.Clear(m_bTimer, 0, nCnt);
                Array.Clear(m_Timer, 0, nCnt);
            }

            // Handle = 0~99
            // Timer 생성
            public static void TimerSet(int nHandle)
            {
                DateTime tmrTemp = DateTime.Now;
                long temp = (long)tmrTemp.Ticks * 100 / 1000000;

                m_bTimer[nHandle] = true;
                m_Timer[nHandle] = temp;
            }

            // Handle = 0~99
            // Timer Destroy ( 생성을 했으면 반드시 Destroy를 하도록 한다. )
            public static void TimerDestroy(int nHandle)
            {
                m_bTimer[nHandle] = false;
                m_Timer[nHandle] = 0;
            }

            // Handle = 0~99
            // Timer 생성 후 현재까지의 시간 값을 return
            public static long Timer(int nHandle)
            {
                if (m_bTimer[nHandle] == true)
                {
                    DateTime tmrTemp = DateTime.Now;
                    long temp = (long)tmrTemp.Ticks * 100 / 1000000;

                    long temp_Gap = temp - m_Timer[nHandle];
                    return temp_Gap;
                }
                else return 0;
            }

            // Handle = 0~99
            // Timer 생성 후 지정한 시간(t)를 넘지 않거나 Timer가 생성되지 않으면 FALSE(0)를 return
            // Timer 생성 후 지정한 시간(t)를 넘었다면 TRUE(1) 를 return
            public static bool TimerCheck(int nHandle, long t)
            {
                if (m_bTimer[nHandle])
                {
                    DateTime tmrTemp = DateTime.Now;
                    long temp = (long)tmrTemp.Ticks * 100 / 1000000;

                    long temp_Gap = temp - m_Timer[nHandle];
                    if (temp_Gap < t) return false;
                    else return true;
                }
                else return false;
            }

            public static int GetYear() { return DateTime.Now.Year; }
            public static int GetMonth() { return DateTime.Now.Month; }
            public static int GetDay() { return DateTime.Now.Day; }
            public static int GetHour() { return DateTime.Now.Hour; }
            public static int GetMinute() { return DateTime.Now.Minute; }
            public static int GetSecond() { return DateTime.Now.Second; }
            // 0 - 일, 1 - 월, 2 - 화, 3 - 수, 4 - 목, 5 - 금, 6 - 토, => -1 - 에러
            public static int GetWeek()
            {
                try
                {
                    return int.Parse(DateTime.Now.DayOfWeek.ToString("d"));
                }
                catch
                {
                    return 0;
                }
            }
        }
        #endregion Timer
        }

        // if you make your class, just write in here
        public class CGenibo
        {
            public CGenibo()
            {
                for (int i = 0; i < 6; i++) OjwSock[i] = new CSocket();
            }
            private const int C_TCP_FILENAME_SIZE = 16;

            private int _PORT = 6100;
            private int _ROBOT = 0;
            private int _SOUND = 1;
            private int _VISION = 2;
            private int _MOTION = 3;
            private int _SENSOR = 4;
            private int _EMOTICON = 5;

            private const int _CNT_PORT = 6;

            // Board ID
            private const int LEFT_FRONT_ID = 19;
            private const int RIGHT_FRONT_ID = 20;
            private const int LEFT_REAR_ID = 21;
            private const int RIGHT_REAR_ID = 22;
            // Motor ID
            private const int LEFT_FRONT_WING_ID = 0;
            private const int LEFT_FRONT_DOWN_ID = 1;
            private const int LEFT_FRONT_UP_ID = 2;
            private const int RIGHT_FRONT_WING_ID = 3;
            private const int RIGHT_FRONT_DOWN_ID = 4;
            private const int RIGHT_FRONT_UP_ID = 5;
            private const int LEFT_REAR_WING_ID = 6;
            private const int LEFT_REAR_DOWN_ID = 7;
            private const int LEFT_REAR_UP_ID = 8;
            private const int RIGHT_REAR_WING_ID = 9;
            private const int RIGHT_REAR_DOWN_ID = 10;
            private const int RIGHT_REAR_UP_ID = 11;
            //private const int MOTOR_TEMP_ID		= 12;
            //private const int HEAD_YAW_ID       = 13;
            private const int HEAD_MOUTH_ID = 14;
            private const int HEAD_PAN_ID = 15;
            private const int HEAD_TILT_ID = 16;
            private const int HIPS_TAIL_ID = 17;
            //private const int HEAD_TILTUP_ID = 13; // 일단은 Yaw 대신...

            private const int HEAD_TILTUP_ID = 18;
            private const int HEAD_LEFT_EAR_ID = 19;
            private const int HEAD_RIGHT_EAR_ID = 20;

            // 소켓
            private CSocket[] OjwSock = new CSocket[_CNT_PORT];
            private bool [] m_achkEn = new bool[_CNT_PORT];
            
            private Thread[] Reader = new Thread[6];             // 읽기 쓰레드

            public void Auth(string strID, string strPassword)
            {
                Ojw.CMessage.Write("Authorization Check");

                int i;
                int nSize1 = 21;
                int nSize2 = 21;// 11;
                int nSize = nSize1 + nSize2;
                byte[] byteData = new byte[nSize];
                for (i = 0; i < nSize; i++)
                {
                    byteData[i] = 0;
                }
                byte[] byteId = Encoding.Default.GetBytes(strID);// Encoding.ASCII.GetBytes(txtId.Text);
                for (i = 0; i < byteId.Length; i++)
                {
                    if (i > (nSize1 - 1)) break;
                    byteData[i] = byteId[i];
                }
                byte[] bytePasswd = Encoding.Default.GetBytes(strPassword);// Encoding.ASCII.GetBytes(mtxtPasswd.Text);
                for (i = 0; i < bytePasswd.Length; i++)
                {
                    if (i > (nSize2 - 1)) break;
                    byteData[i + nSize1] = bytePasswd[i];
                }

                for (i = _ROBOT; i <= _EMOTICON; i++)
                    SendData(i, 0x71, nSize, byteData);
                //SendData(_EMOTICON, 0x71, nSize, byteData);
            }

            public bool ActionPlay(bool bMaster, String strFileName, bool bSndEn, bool bEmtEn, int nPercentValue)
            {
                try
                {
                    //if (chkMp3Sync.Checked == true) Mp3Play();

                    // 문자열 코드의 아스키 변환
                    byte[] byteData1 = Encoding.Default.GetBytes(strFileName);// Encoding.ASCII.GetBytes(strFileName);
                    int nSize = C_TCP_FILENAME_SIZE + 3;
                    int nStrLength = strFileName.Length;
                    //if (nStrLength >= C_TCP_FILENAME_SIZE) byteData1[15] = 0;

                    int nNum = 0;
                    byte[] byteData = new byte[nSize];
                    for (int i = 0; i < C_TCP_FILENAME_SIZE - 1; i++)
                    {
                        if (i < nStrLength) byteData[nNum++] = byteData1[i];
                        else byteData[nNum++] = 0;
                    }
                    // 널 종료문자
                    byteData[nNum++] = 0;

                    // Sound Enable
                    byteData[nNum++] = (byte)((bSndEn == true) ? 1 : 0);
                    // Emoticon Enable
                    byteData[nNum++] = (byte)((bEmtEn == true) ? 1 : 0);
                    // Speed Percent
                    byteData[nNum++] = (byte)(nPercentValue & 0xff);

                    int nCmd = 0x34;
                    if (bMaster) nCmd = 0x33;
                    SendData(_MOTION, nCmd, nSize, byteData);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            public void SendData(int nType, int nCommand, int nSize, byte[] byteArrayData)
            {
                int i, nNum;

                // 프레임 수
                //int nSize = nSize;
                int nPacketSize = nSize + 8;
                byte[] byteData = new byte[nPacketSize];

                nNum = 0;
                byteData[nNum++] = 0xff;
                // ID
                byteData[nNum++] = (Byte)((0x90 + nType) & 0xff);
                // Cmd
                byteData[nNum++] = (Byte)(nCommand & 0xff);

                // Data Size
                byteData[nNum++] = (byte)((nSize >> 24) & 0xff);
                byteData[nNum++] = (byte)((nSize >> 16) & 0xff);
                byteData[nNum++] = (byte)((nSize >> 8) & 0xff);
                byteData[nNum++] = (byte)(nSize & 0xff);

                //byteArrayData.CopyTo(byteData, nNum);
                //nNum += nSize;
                for (i = 0; i < nSize; i++)
                {
                    byteData[nNum++] = byteArrayData[i];
                }

                // CheckSum
                byte byteCheckSum = byteData[1];
                for (i = 2; i < nPacketSize - 1; i++)
                {
                    byteCheckSum ^= byteData[i];
                }
                byteData[nNum++] = (byte)(byteCheckSum & 0x7f);

                //String strData;
                //strData = "nPacketSize = " + OjwConvert.IntToStr(nPacketSize) +
                //          ", nNum = " + OjwConvert.IntToStr(nNum);
                //Ojw.CMessage.Write(strData);
                OjwSock[nType].Send(byteData);
                //if (chkSycnIPEn.Checked == true) for (int nSync = 0; nSync < nCntSyncIP; nSync++) { if (OjwSockSync[nSync * 6 + nType].m_bConnect) OjwSockSync[nSync * 6 + nType].Send(byteData); }

                //strData = OjwConvert.IntToHex(byteData[0]) + " ";
                //for (i = 1; i < nNum; i++)
                //{
                //    strData += OjwConvert.IntToHex(byteData[i]) + " ";
                //}
                //Ojw.CMessage.Write(strData);
                //Ojw.CMessage.Write("CheckSum=" + OjwConvert.IntToHex((byte)(byteCheckSum & 0x7f)));
                //Ojw.CMessage.Write("AllDataSize = " + OjwConvert.IntToStr(nNum));

            }
            public bool Connect(string strIp)
            {
                int i, j, k;
                bool bOk = true;
                int nRetry = 1;
                bool bConnect = false;

                ////
                for (i = 0; i < _CNT_PORT; i++)
                {
                    m_achkEn[i] = true;
                }
                ////

                for (i = 0; i < nRetry; i++)
                {
                    for (j = 0; j < _CNT_PORT; j++)
                    {
                        bConnect = OjwSock[j].m_bConnect;
                        if ((!bConnect) && (m_achkEn[j]))
                        {
                            OjwSock[j].Connect(strIp, _PORT + j);

                            if (OjwSock[j].m_bConnect == true)
                            {
                                if (j == _ROBOT) Reader[j] = new Thread(new ThreadStart(Receive_Robot));
                                else if (j == _SOUND) Reader[j] = new Thread(new ThreadStart(Receive_Sound));
                                else if (j == _VISION) Reader[j] = new Thread(new ThreadStart(Receive_Vision));
                                else if (j == _MOTION) Reader[j] = new Thread(new ThreadStart(Receive_Motion));
                                else if (j == _SENSOR) Reader[j] = new Thread(new ThreadStart(Receive_Sensor));
                                else if (j == _EMOTICON) Reader[j] = new Thread(new ThreadStart(Receive_Emoticon));
                                Reader[j].Start();
                            }
                            else
                            {
                                for (k = 0; k < _CNT_PORT; k++)
                                {
                                    bConnect = OjwSock[k].m_bConnect;
                                    if (bConnect && (m_achkEn[k]))
                                    {
                                        OjwSock[k].DisConnect();
                                        Reader[k].Abort();         // 쓰레드 종료
                                    }
                                    m_achkEn[k] = true;
                                }
                                CMessage.Write_Error("서버와 연결 실패");
                                return false;
                            }
                        }
                    }          
                }
                for (i = 0; i < _CNT_PORT; i++)
                    if (m_achkEn[i]) bOk &= OjwSock[i].m_bConnect;

                if (bOk)
                {
                    //btnConnect.Text = "DisConnect";

                    for (i = 0; i < _CNT_PORT; i++)
                    {
                        bConnect = OjwSock[i].m_bConnect;
                        if ((bConnect) && (m_achkEn[i]))
                        {
                            CMessage.Write_Error("[{0}] Task Connected", i);
                        }
                        m_achkEn[i] = false;
                    }
                    //tmrHeartBeat.Enabled = true;
                    //tmrCheckAll.Enabled = true;

                    // 파일 데이터 저장
                    //TextConfigFileSave(m_strOrgDirectory + "\\ip.ini");
                }
                else
                {
                    for (i = 0; i < _CNT_PORT; i++)
                    {
                        bConnect = OjwSock[i].m_bConnect;
                        if (bConnect && (m_achkEn[i]))
                        {
                            OjwSock[i].DisConnect();
                            Reader[i].Abort();         // 쓰레드 종료
                        }
                    }
                    for (i = 0; i < 6; i++) m_achkEn[i] = true;

                    CMessage.Write_Error("서버와 연결 실패");
                    return false;
                }
                return true;
            }

            public void DisConnect()
            {
                int i;
                bool bConnect = false;
                for (i = 0; i < 6; i++)
                {
                    bConnect = OjwSock[i].m_bConnect;
                    if (bConnect && (m_achkEn[i]))
                        OjwSock[i].DisConnect();
                }
                //if (nCntSyncIP > 0) for (int nSync = 0; nSync < nCntSyncIP; nSync++) { if (OjwSockSync[nCntSyncIP].m_bConnect) OjwSockSync[nCntSyncIP].DisConnect(); }

                for (i = 0; i < 6; i++) m_achkEn[i] = true;

                //if (m_bClosed == false) btnConnect.Text = "Connect";
            }
            private String[] m_strType = new String[6] { "Robot", "Sound", "Vision", "Action", "Sensor", "Emoticon" };
#if true
            public void Receive_Robot()
            {
                int nType = _ROBOT;

                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    string strMessage0, strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            strMessage0 = "-------------------------\r\n<=" + m_strType[nType];
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;

                            string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                            strMessage0 += "(0x" + CConvert.IntToHex(byteData) + ")";
                            strMessage0 += "(0x" + CConvert.IntToHex(nCmd) + ")";
                            strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 24) & 0xff) + ")";
                            strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 16) & 0xff) + ")";
                            strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 8) & 0xff) + ")";
                            strMessage0 += "(0x" + CConvert.IntToHex(nLength & 0xff) + ")";

                            int i;
                            string[] pstrData;

                            strTmp = "";
                            for (i = 0; i < nLength; i++)
                                strTmp += "(0x" + CConvert.IntToHex(byteArrayData[i]) + ")";

                            strMessage0 += strTmp + "\r\n";
                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1) strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process OK]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "\r\n";
                                CMessage.Write(strMessage1);
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                //if (chkDispHearBeat.Checked)
                                //{
                                //    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]\r\n";
                                //    CMessage.Write(strMessage1);
                                //}
                                //else continue;
                            }
                            else
                            {
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                switch (nCmd)
                                {
                                    case 0x01:
                                        {
                                            //// 로봇 이름 확인
                                            //txtRobotID.Text = System.Text.Encoding.Default.GetString(byteArrayData, 0, ACTION_NAME_SIZE);
                                        }
                                        break;
                                    case 0x03:
                                        {
                                            //// 로봇 PassWd 확인
                                            //txtRobotPasswd.Text = System.Text.Encoding.Default.GetString(byteArrayData, 0, ACTION_NAME_SIZE);
                                        }
                                        break;
                                    case 0x11:
                                        {
                                            //// 현재 시간 확인
                                            //int i = 0;
                                            i = 0;
                                            string[] pstrWeek = new string[7] { "일", "월", "화", "수", "목", "금", "토" };
                                            if (nLength < 7) break;
                                            string strRobot_Year = CConvert.IntToStr(byteArrayData[i++] + 2000);
                                            string strRobot_Month = CConvert.IntToStr(byteArrayData[i++]);
                                            string strRobot_Date = CConvert.IntToStr(byteArrayData[i++]);
                                            i++;//cmbWeek.SelectedIndex = (int)(byteArrayData[i++] - 1);
                                            String strHour = CConvert.IntToStr(byteArrayData[i++]);
                                            String strMinute = CConvert.IntToStr(byteArrayData[i++]);
                                            String strSecond = CConvert.IntToStr(byteArrayData[i++]);
                                            Ojw.CMessage.Write("{0}-{1}-{2}", strRobot_Year, strRobot_Month, strRobot_Date);
                                            //monthCalendarRobot.SetDate(Convert.ToDateTime(
                                            //    strRobot_Year + "-" +
                                            //    strRobot_Month + "-" +
                                            //    strRobot_Date
                                            //    ));
                                        }
                                        break;
                                    case 0x41:
                                        {
                                            //int nPos = 0;
                                            //byte byTmp = 0;
                                            //m_nSchedule_Size = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //listviewSchedule.Items.Clear();
                                            //for (i = 0; i < m_nSchedule_Size; i++)
                                            //{
                                            //    m_pSSchedule[i].sIndex = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    m_pSSchedule[i].szTitle = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 59 + 2, C_SIZE_SCHEDULE_TITLE); nPos += C_SIZE_SCHEDULE_TITLE;
                                            //    m_pSSchedule[i].cFlag = byteArrayData[nPos++];

                                            //    m_pSSchedule[i].SDateStart.cYear = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cMonth = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cDay = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cWeek = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cHour = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cMinute = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cSecond = byteArrayData[nPos++];

                                            //    m_pSSchedule[i].cAlarm_Type = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].szAlarm_Name = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 59 + 2 + C_SIZE_SCHEDULE_TITLE + 9, C_SIZE_SCHEDULE_ALARM_NAME); nPos += C_SIZE_SCHEDULE_ALARM_NAME;
                                            //    m_pSSchedule[i].cRepeat_Type = byteArrayData[nPos++];

                                            //    m_pSSchedule[i].cRepeat_Week = byteArrayData[nPos++];

                                            //    byTmp = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].cRepeat_Count = (byte)((byTmp >> 4) & 0x0f);
                                            //    m_pSSchedule[i].cRepeat_Time = (byte)(byTmp & 0x0f);

                                            //    m_pSSchedule[i].SDateEnd.cYear = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cMonth = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cDay = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cWeek = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cHour = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cMinute = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cSecond = byteArrayData[nPos++];

                                            //    m_pSSchedule[i].cSection = byteArrayData[nPos++];

                                            //    // ListView;
                                            //    listviewSchedule.Items.Add(m_pSSchedule[i].szTitle);
                                            //}
                                            //ScheduleDisp(0);
                                        }
                                        break;
                                    case 0x43:
                                        {
                                            string[] strDay = new string[7] { "일", "월", "화", "수", "목", "금", "토" };
                                            i = 0;
                                            int nData = (int)(byteArrayData[i++]);
                                            if (nData != 0) strData = "[Enable]";
                                            else strData = "[Disable]";

                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "년";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "월";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "일";
                                            nData = (int)(byteArrayData[i++]) - 1;
                                            if (nData < 0) nData = 0;
                                            strData += strDay[nData] + "요일";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "시";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "분";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "초";
                                            CMessage.Write(strData);
                                        }
                                        break;
                                    case 0x45:
                                        {
                                            string[] strDay = new string[7] { "일", "월", "화", "수", "목", "금", "토" };
                                            i = 0;
                                            int nData = (int)(byteArrayData[i++]);
                                            if (nData != 0) strData = "[Enable]";
                                            else strData = "[Disable]";

                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "년";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "월";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "일";
                                            nData = (int)(byteArrayData[i++]) - 1;
                                            if (nData < 0) nData = 0;
                                            strData += strDay[nData] + "요일";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "시";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "분";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "초";
                                            strData += "지역코드=";
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            CMessage.Write(strData);
                                        }
                                        break;
                                    case 0x57:
                                        {
                                            strMessage1 += "모드체크\n";
                                            if (byteArrayData[0] == 0) CMessage.Write("자동모드");
                                            else CMessage.Write("수동모드");
                                        }
                                        break;
                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";
                                            }
                                        }
                                        break;
                                    case 0x84:
                                        {
                                            //int nPos = 0;
                                            //int nAiSize = (int)(byteArrayData[nPos++]);
                                            //int nCode;
                                            //short sData;
                                            //for (i = 0; i < nAiSize; i++)
                                            //{
                                            //    nCode = (int)(byteArrayData[nPos++]);
                                            //    txtRobotAI[i].Text = System.Text.Encoding.Default.GetString(byteArrayData, nPos, C_SIZE_TITLE); nPos += C_SIZE_TITLE;
                                            //    sData = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    lbRobotAI[i].Text = CConvert.IntToStr(sData);
                                            //    tbRobotAI[i].Value = (int)sData;
                                            //}
                                        }
                                        break;
                                    case 0x86:
                                        {
                                            //int nPos = 0;
                                            //int nEmotionValue = (int)(byteArrayData[nPos++]);
                                            //if (nEmotionValue == 1)
                                            //    txtRobotEmotion.Text = "기쁨";
                                            //else if (nEmotionValue == 2)
                                            //    txtRobotEmotion.Text = "즐거움";
                                            //else if (nEmotionValue == 3)
                                            //    txtRobotEmotion.Text = "슬픔";
                                            //else if (nEmotionValue == 4)
                                            //    txtRobotEmotion.Text = "놀람";
                                            //else if (nEmotionValue == 5)
                                            //    txtRobotEmotion.Text = "화남";
                                            //else if (nEmotionValue == 6)
                                            //    txtRobotEmotion.Text = "심심";
                                            //else if (nEmotionValue == 7)
                                            //    txtRobotEmotion.Text = "졸림";
                                            //else txtRobotEmotion.Text = "알수없음";
                                        }
                                        break;
                                    case 0x91:
                                        {
                                            //int nPos = 0;
                                            //int nAiSize = (int)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]);
                                            //nPos += 2;
                                            //short sData;
                                            //for (i = 0; i < nAiSize; i++)
                                            //{
                                            //    sData = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    //m_rowData_Contents[i]["Index"] = CConvert.IntToStr(sData);
                                            //    CMessage.Write("Index=" + CConvert.IntToStr(sData));
                                            //    m_rowData_Contents[i]["Contents"] = System.Text.Encoding.Default.GetString(byteArrayData, nPos, C_SIZE_TITLE).Trim('\0'); nPos += C_SIZE_TITLE;
                                            //    sData = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    m_rowData_Contents[i]["Remocon"] = CConvert.IntToStr(sData);//(int)(byteArrayData[nPos++]);
                                            //    m_rowData_Contents[i]["Action"] = System.Text.Encoding.Default.GetString(byteArrayData, nPos, C_SIZE_FILE_NAME).Trim('\0'); nPos += C_SIZE_FILE_NAME;
                                            //    sData = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    m_rowData_Contents[i]["Scenario"] = CConvert.IntToStr(sData);
                                            //    m_rowData_Contents[i]["Mp3"] = System.Text.Encoding.Default.GetString(byteArrayData, nPos, C_SIZE_FILE_NAME).Trim('\0'); nPos += C_SIZE_FILE_NAME;
                                            //}
                                        }
                                        break;
                                    case 0x93:
                                        {
                                            // ListView;                                        
                                            //int nPos = 0;
                                            //int nAiSize = (int)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]);
                                            //nPos += 2;
                                            //int nCode;
                                            //short sData;
                                            //listviewRobotScenario.Items.Clear();
                                            //String strTitle;
                                            //for (i = 0; i < nAiSize; i++)
                                            //{
                                            //    sData = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    nCode = sData;
                                            //    strTitle = System.Text.Encoding.Default.GetString(byteArrayData, nPos, C_SIZE_TITLE); nPos += C_SIZE_TITLE;
                                            //    //m_rowData_Contents[i]["Contents"] = strTitle;
                                            //    listviewRobotScenario.Items.Add(strTitle);
                                            //}
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            //strMessage1 += "[Version Get]\r\n";

                                            //strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            //lbVersion_Robot.Text = strTmp;
                                        }
                                        break;

                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                CMessage.Write(strMessage1);

                            }
                            CMessage.Write(strMessage0);
                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }

            public void Receive_Sound()
            {
                int nType = _SOUND;

                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    byte byteCheckSum = 0;
                    string strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;
                            // CheckSum Data Get
                            byteCheckSum = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;

                            string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                            int i;
                            string[] pstrData;

                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1) strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process Fail]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "\r\n";
                                CMessage.Write(strMessage1);
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                //if (chkDispHearBeat.Checked)
                                //{
                                //    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]\r\n";
                                //    CMessage.Write(strMessage1);
                                //}
                                //else continue;
                            }
                            else
                            {
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                switch (nCmd)
                                {
                                    case 0x01:
                                        {
                                            //listviewGenibo_Voice.Clear();
                                            //strMessage1 += "[음성반응 목록확인]\r\n";
                                            //int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                            //int nIndex;
                                            //m_nVoiceListCode_Sound = new int[nListCount];
                                            //m_nVoiceListCount_Sound = nListCount;
                                            //for (i = 0; i < nListCount; i++)
                                            //{
                                            //    nIndex = byteArrayData[2 + i * 26] * 256 + byteArrayData[2 + i * 26 + 1];
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 26 + 2, 21);
                                            //    //int nVoiceType = byteArrayData[2 + i * 26 + 2 + 21];
                                            //    //int nVoiceAction = byteArrayData[2 + i * 26 + 2 + 22] * 256 + byteArrayData[2 + i * 26 + 2 + 22 + 1];
                                            //    listviewGenibo_Voice.Items.Add(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                            //    m_nVoiceListCode_Sound[i] = nIndex;
                                            //    m_strVoiceRecog[nIndex] = strTmp.Trim('\0');
                                            //}
                                        }
                                        break;
                                    case 0x04:
                                        {
                                            ////short sIndex = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            //short sIndex = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));
                                            //if (sIndex <= 0) lbVoiceRecog.Text = "인식실패";
                                            ////else if (sIndex > listviewGenibo_Voice.Columns.Count)
                                            ////{
                                            ////    lbVoiceRecog.Text = "인식결과값 알 수 없음";
                                            ////}
                                            //else
                                            //{
                                            //    //String strFileName = listviewGenibo_Voice.Items[sIndex].Text;
                                            //    //strFileName = strFileName.Substring(0, strFileName.IndexOf("["));
                                            //    String strFileName = m_strVoiceRecog[sIndex];
                                            //    lbVoiceRecog.Text = strFileName;
                                            //}
                                        }
                                        break;
                                    case 0x13:
                                        {
#if true
                                            //listviewGenibo_Sound.Clear();
                                            //strMessage1 += "[음향 목록확인]\r\n";

                                            //short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            ////short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            //int nIndex;
                                            ////m_nMotionListCode = new int[sListCount];
                                            //for (i = 0; i < sListCount; i++)
                                            //{
                                            //    // Title & FileName
                                            //    // FileName
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 4) + 21, 16);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "[";
                                            //    // Title
                                            //    strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 4), 21);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "]";

                                            //    listviewGenibo_Sound.Items.Add(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                            //}
#else
                                        listviewGenibo_Sound.Clear();
                                        strMessage1 += "[음향 목록확인]\r\n";
                                        int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                        int nIndex;
                                        m_nSoundListCode_Sound = new int[nListCount];
                                        m_nSoundListCount_Sound = nListCount;
                                        for (i = 0; i < nListCount; i++)
                                        {
                                            nIndex = byteArrayData[2 + i * 23] * 256 + byteArrayData[2 + i * 23 + 1];
                                            strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 23 + 2, 21);
                                            listviewGenibo_Sound.Items.Add(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                            m_nSoundListCode_Sound[i] = nIndex;
                                        }
#endif
                                        }
                                        break;
                                    case 0x21:
                                        {
                                            //listviewGenibo_SoundMemo.Clear();
                                            //strMessage1 += "[음성메모 목록확인]\r\n";

                                            //short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            //int nIndex;
                                            //for (i = 0; i < sListCount; i++)
                                            //{
                                            //    // Title & FileName
                                            //    // FileName(16)
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 7) + 21, 16);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "[";
                                            //    // Title(21)
                                            //    strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 7), 21);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "]";
                                            //    // Date(7)
                                            //    // ....

                                            //    listviewGenibo_SoundMemo.Items.Add(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                            //}
                                        }
                                        break;
                                    case 0x31:
#if true
                                        {
                                            //listviewGenibo_MP3.Clear();
                                            //strMessage1 += "[MP3 모션목록확인]\r\n";

                                            //short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            ////short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            //int nIndex;
                                            ////m_nMotionListCode = new int[sListCount];
                                            //for (i = 0; i < sListCount; i++)
                                            //{
                                            //    // Title & FileName
                                            //    // FileName
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 4) + 21, 16);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "[";
                                            //    // Title
                                            //    strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 4), 21);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "]";

                                            //    // PlayTime
                                            //    //....

                                            //    listviewGenibo_MP3.Items.Add(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                            //}
                                        }
#else
                                    {
                                        listviewGenibo_MP3.Clear();
                                        strMessage1 += "[MP3 목록확인]\r\n";
                                        int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                        int nIndex;
                                        m_nMP3ListCode_Sound = new int[nListCount];
                                        m_nMP3ListCount_Sound = nListCount;
                                        for (i = 0; i < nListCount; i++)
                                        {
                                            nIndex = byteArrayData[2 + i * (255 + 2)] * 256 + byteArrayData[2 + i * (255 + 2) + 1];
                                            strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (255 + 2) + 2, 255);
                                            listviewGenibo_MP3.Items.Add(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                            m_nMP3ListCode_Sound[i] = nIndex;
                                        }
                                    }
#endif
                                        break;

                                    //case 0x03:
                                    //    try
                                    //    {
                                    //        strMessage1 += "[Emoticon 업로드]\r\n";
                                    //        short sData;
                                    //        i = 0;
                                    //        int j;
                                    //        // Index
                                    //        //sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                    //        sData = (short)(byteArrayData[i++] * 256);
                                    //        sData += byteArrayData[i++];
                                    //        int nIndex = (int)sData;
                                    //        strMessage1 += "[Index=" + CConvert.IntToStr(nIndex) + "]";
                                    //        txtEmoticonIndex.Text = CConvert.IntToStr(nIndex);

                                    //        // Name
                                    //        txtEmoticonName.Text = System.Text.Encoding.Default.GetString(byteArrayData, i, 20);
                                    //        //txtTableName.Text = System.Text.Encoding.Default.GetString(byteArrayTitle, 0, 20);
                                    //        i += 21;

                                    //        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

                                    //        //// Data
                                    //        for (j = 0; j < 8; j++)
                                    //        {
                                    //            // Left
                                    //            SetEmoticon(_L_, j, byteArrayData[i++]);
                                    //        }
                                    //        for (j = 0; j < 8; j++)
                                    //        {
                                    //            // Right
                                    //            SetEmoticon(_R_, j, byteArrayData[i++]);
                                    //        }
                                    //    }
                                    //    catch
                                    //    {
                                    //        CMessage.Write("업로드 Error\n");
                                    //    }
                                    //    break;

                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\r\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";
                                            }
                                        }
                                        break;
                                    case 0x84:
                                        {
                                            ////tbSoundSpk.Value = (100 - byteArrayData[0]);
                                            //tbSoundSpk.Value = byteArrayData[0];
                                            //tbSoundSpk.Enabled = true;
                                            //lbSoundSpk.BackColor = Color.Yellow;
                                        }
                                        break;
                                    case 0x85:
                                        {
                                            ////tbSoundMic.Value = (100 - byteArrayData[0]);
                                            //tbSoundMic.Value = byteArrayData[0];
                                            //tbSoundMic.Enabled = true;
                                            //lbSoundMic.BackColor = Color.Yellow;
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            //strMessage1 += "[Version Get]\r\n";

                                            //strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            //lbVersion_Sound.Text = strTmp;
                                        }
                                        break;
                                    case 0xE2:
                                        strData = "    =>(Data[0]=0x" + CConvert.IntToHex(byteArrayData[0]) + ")\r\n" +
                                                  "    =>(Data[1]=0x" + CConvert.IntToHex(byteArrayData[1]) + ")\r\n" +
                                                  "    =>(Data[2]=0x" + CConvert.IntToHex(byteArrayData[2]) + ")\r\n" +
                                                  "    =>(Data[3]=0x" + CConvert.IntToHex(byteArrayData[3]) + ")\r\n";
                                        strMessage1 += "[Reserve]\r\n" + strData;
                                        break;
                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                CMessage.Write(strMessage1);

                            }
                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }

            private long m_lTick_Frame = 0;
            //private float m_fCalcFrame = 0.0f;
            public void Receive_Vision()
            {
                int nType = _VISION;
                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    //byte byteData_test= 0;
                    byte byteCheckSum = 0;
                    //string strMessage0;
                    string strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            //strMessage0 = "-------------------------\r\n<=" + m_strType[nType];
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;

                            //CMessage.Write("사이즈 - " + CConvert.IntToStr(OjwSock[nType].GetLength()));
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;
                            // CheckSum Data Get
                            byteCheckSum = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            
                            int i;
                            string[] pstrData;
                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1) strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process OK]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "\r\n";
                                CMessage.Write(strMessage1);
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                ////if (chkDispHearBeat.Checked)
                                ////{
                                ////    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]\r\n";
                                ////    CMessage.Write(strMessage1);
                                ////}
                                ////else continue;
                            }
                            else if (nCmd == 0x01)  // 정지영상 목록 확인
                            {
                                //int nFileNameSize = 16;
                                //listviewGenibo_Jpg.Clear();
                                //strMessage1 = "[촬영영상 목록확인]\r\n";
                                //int nIndex = 0;
                                //int nPos = 0;
                                //short sSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos));//BitConverter.ToInt16(byteArrayData, 0);
                                //nPos += 2;
                                //m_nVisionListCode_Jpg = new int[sSize];
                                //m_nVisionListCount_Jpg = sSize;
                                //for (i = 0; i < sSize; i++)
                                //{
                                //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, nPos, nFileNameSize);
                                //    strTmp = strTmp.Trim('\0') + ".jpg";
                                //    //string strTmp_Real = "";
                                //    //for (int j = 0; j < strTmp.Length; j++)
                                //    //{
                                //    //    if (strTmp[j] >= 0x30)
                                //    //        strTmp_Real += strTmp[j];
                                //    //}
                                //    nPos += nFileNameSize;
                                //    listviewGenibo_Jpg.Items.Add(CConvert.IntToStr(nIndex++), strTmp, 0);
                                //    m_nVisionListCode_Jpg[i] = nIndex;
                                //}
                            }
                            else if (nCmd == 0x02)  // GrabConti
                            {
                                //try
                                //{
                                //    int nFileNameSize = 16;
                                //    CMessage.Write("저장된 영상 로드(0x02)");
                                //    int nPos = 0;
                                //    short sSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos));//BitConverter.ToInt16(byteArrayData, 0);
                                //    nPos += 2;
                                //    string strFileName = System.Text.Encoding.Default.GetString(byteArrayData, nPos, nFileNameSize);
                                //    lbVisionFileName.Text = strFileName.Trim('\0') + ".jpg";
                                //    nPos += nFileNameSize;
                                //    MemoryStream streamData = new MemoryStream(byteArrayData, nPos, sSize);//byteArrayData.Length);
                                //    streamData.Read(byteArrayData, nPos, sSize);//nLength);

                                //    //Image image;
                                //    //Graphics gr = CreateGraphics();


                                //    m_image = Image.FromStream(streamData);
                                //    m_gr.DrawImage(m_image, 1, 1, m_image.Width, m_image.Height);

                                //    //gr.Dispose();
                                //}
                                //catch
                                //{
                                //    CMessage.Write_Error("저장된 영상 로드(0x02) - 실패");
                                //}
                            }
                            else if ((nCmd == 0x04) || (nCmd == 0x06)) // GrabConti
                            {
                                //try
                                //{
                                //    //CMessage.Write("0x04, 0x06");
                                //    //strMessage1 += "[연속 영상 확인]\r\n";

                                //    MemoryStream streamData = new MemoryStream(byteArrayData);
                                //    streamData.Read(byteArrayData, 0, nLength);

                                //    //Image image;
                                //    //Graphics gr = CreateGraphics();


                                //    m_image = Image.FromStream(streamData);
                                //    m_gr.DrawImage(m_image, 1, 1, m_image.Width, m_image.Height);

                                //    //gr.Dispose();

                                //    if (chkFrame.Checked == true)
                                //    {
                                //        if (m_bCheckFrameStart == true)
                                //        {
                                //            m_bCheckFrameStart = false;
                                //            OjwTimer.TimerSet(TID_CHECK_FRAME);
                                //            m_lTick_Frame = 0;
                                //        }
                                //        else if (OjwTimer.Timer(TID_CHECK_FRAME) >= m_lCheckFrameTimer)
                                //        {
                                //            //m_fCalcFrame = 
                                //            lbCheckTime.Text = String.Format("{0} Frames / {1} ms", m_lTick_Frame, OjwTimer.Timer(TID_CHECK_FRAME));
                                //            OjwTimer.TimerSet(TID_CHECK_FRAME);
                                //            m_lTick_Frame = 0;
                                //        }
                                //        else m_lTick_Frame++;
                                //    }

                                //}
                                //catch
                                //{
                                //    CMessage.Write_Error("GrabConti - 실패");
                                //}
                            }
                            else if ((nCmd == 0x14) || (nCmd == 0x16)) // GrabConti
                            {
                                //try
                                //{

                                //    //CMessage.Write("0x14, 0x16");
                                //    //strMessage1 += "[연속 영상 확인]\r\n";

                                //    //MemoryStream streamData = new MemoryStream(byteArrayData);
                                //    //streamData.Read(byteArrayData, 0, nLength);

                                //    //Image image;
                                //    //Graphics gr = CreateGraphics();

                                //    Color cData;
                                //    Bitmap bmpData = new Bitmap(320, 240);
                                //    for (int y = 0; y < 240; y++)
                                //        for (int x = 0; x < 320; x++)
                                //        {
                                //            cData = Color.FromArgb(byteArrayData[x + 320 * y], byteArrayData[x + 320 * y], byteArrayData[x + 320 * y]);
                                //            bmpData.SetPixel(x, y, cData);
                                //        }
                                //    //m_image = Image.FromStream(streamData);
                                //    m_gr.DrawImage(bmpData, 1, 1, bmpData.Width, bmpData.Height);

                                //    //gr.Dispose();

                                //}
                                //catch
                                //{
                                //    CMessage.Write_Error("GrabConti - 실패");
                                //}
                            }
                            else
                            {
                                string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";

                                switch (nCmd)
                                {
                                    ////case 0x01:
                                    ////    strMessage1 += "[모션재생]\r\n";
                                    ////    break;

                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\r\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";
                                            }
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            strMessage1 += "[Version Get]\r\n";

                                            strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            //lbVersion_Vision.Text = strTmp;
                                        }
                                        break;
                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                CMessage.Write(strMessage1);
                            }
                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }
            // 받고나서 보내야 하는 데이터가 있을 경우 이걸로 true 를 확인하고 보낸다. 단, 보내기전 이 변수를 false 로 변경할것...
            bool m_bReceiveOk = true;
            public int m_nCalibration = _CALIBRATION_SAVE;
            public const int _CALIBRATION_SAVE = 0;// Mode : 0 - save, 1 - reload,  2 - clear 
            public const int _CALIBRATION_LOAD = 1;
            public const int _CALIBRATION_CLEAR = 2;
            public void Receive_Motion()
            {
                int nType = _MOTION;

                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    byte byteCheckSum = 0;
                    string strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;
                            // CheckSum Data Get
                            byteCheckSum = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;

                            string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                            int i;
                            string[] pstrData;

                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    m_bReceiveOk = true; // 일단 데이터를 받으면 그냥 줌...
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1)
                                        strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process Fail]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex((byte)(byteArrayData[0])) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "size=" + CConvert.IntToHex(nLength) + "\r\n";
                                CMessage.Write(strMessage1);
                                // Calibration
                                if ((((byte)(byteArrayData[0])) == 0x40 + 36) || (((byte)(byteArrayData[0])) == 0x3C))
                                {
                                    if ((byte)(byteArrayData[1]) == 0)
                                    {
                                        if (m_nCalibration == _CALIBRATION_SAVE)
                                        {
                                            strTmp = "[저장 성공]";
                                        }
                                        else if (m_nCalibration == _CALIBRATION_LOAD)
                                        {
                                            strTmp = "[ReLoaded Calibration Data]";
                                        }
                                        else if (m_nCalibration == _CALIBRATION_CLEAR)
                                        {
                                            strTmp = "[Cleared Calibration Data]";
                                        }
                                        else
                                        {
                                            strTmp = "캘리브레이션 문제 발생 - 리턴값 종류 확인 불가(윈도우 프로그램[m_nCalibration] 문제)";
                                        }

                                    }
                                    else if ((byte)(byteArrayData[1]) == 1)
                                        strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[실패 - 다시 시도하시오.]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";

                                    MessageBox.Show("[Calibration] => " + ((m_nCalibration == _CALIBRATION_CLEAR) ? "Clear" : ((m_nCalibration == _CALIBRATION_LOAD) ? "Load" : ((m_nCalibration == _CALIBRATION_SAVE) ? "Save" : "Unknown"))) + strTmp);
                                }
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                //if (chkDispHearBeat.Checked)
                                //{
                                //    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]size=" + CConvert.IntToHex(nLength) + "\r\n";
                                //    CMessage.Write(strMessage1);
                                //}
                                //else continue;
                            }
                            else
                            {
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                switch (nCmd)
                                {
                                    //case 0x01:
                                    //    strMessage1 += "[모션재생]\r\n";
                                    //    break;
                                    #region 모션목록확인
                                    case 0x02: // 모션목록확인
                                        {
#if false
                                            strMessage1 += "[모션목록확인]\r\n";
                                            int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                            int nIndex;
                                            m_nMotionListCode = new int[nListCount];
                                            m_nMotionListCount = nListCount;

                                            for (i = 0; i < nListCount; i++)
                                            {
                                                nIndex = byteArrayData[2 + i * 23] * 256 + byteArrayData[2 + i * 23 + 1];
                                                //strMessage1 += "[Index=" + CConvert.IntToStr(nIndex) + "]";
                                                strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 23 + 2, 21);
                                                //strMessage1 += strTmp.Trim('\0') + "\r\n";
                                                listviewGenibo.Items.Add(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                                m_nMotionListCode[i] = nIndex;
                                            }
#else
                                            CMessage.Write("[모션목록확인]");
                                            int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                            int nIndex;
                                            //m_nMotionListCode = new int[nListCount];
                                            //m_nMotionListCount = nListCount;

                                            for (i = 0; i < nListCount; i++)
                                            {
                                                nIndex = byteArrayData[2 + i * 23] * 256 + byteArrayData[2 + i * 23 + 1];
                                                strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 23 + 2, 21);
                                                CMessage.Write(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                                //m_nMotionListCode[i] = nIndex;
                                            }
#endif
                                        }
                                        break;
                                    #endregion 모션목록확인
                                    #region 모션 Upload
                                    case 0x03:
                                        {
                                            try
                                            {
                                                strMessage1 += "[모션 업로드]\r\n";
                                                //short sData;
                                                i = 0;
                                                //int j;
                                                // Index
                                                int nIndex;
                                                nIndex = ((byteArrayData[i++] & 0xff) * 256);
                                                nIndex += (byteArrayData[i++] & 0xff);

                                                strMessage1 += "[Index=" + CConvert.IntToStr(nIndex) + "]";

                                                ///*if (nIndex == 0)
                                                //{
                                                //    sData = (short)((byteArrayData[i++] & 0xff) * 256);
                                                //    sData += (short)(byteArrayData[i++] & 0xff);

                                                //    i += 21;

                                                //    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                //    //// Data                                            
                                                //    int nTmp = sData;

                                                //    if (m_nPage == DISP_XY) j = dataGrid_XY.CurrentCell.RowIndex;
                                                //    else if (m_nPage == DISP_ANGLE) j = dataGrid_Angle.CurrentCell.RowIndex;
                                                //    else j = dataGrid_Evd.CurrentCell.RowIndex;
                                                //    DataClear(DISP_XY, j);
                                                //    //Index
                                                //    m_rowData_XY[j]["Index"] = j;
                                                //    // En
                                                //    //if (j < nTmp)
                                                //        m_rowData_XY[j]["En"] = 1;
                                                //    // Action Data
                                                //    //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                                //    string[] pstrData2 = new string[4] { "Lf", "Lr", "Rf", "Rr" };
                                                //    string strTmp2;
                                                //    for (int k = 0; k < 4; k++)
                                                //    {
                                                //        sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                //        strTmp2 = pstrData2[k] + "X"; m_rowData_XY[j][strTmp2] = sData;
                                                //        sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                //        strTmp2 = pstrData2[k] + "Y"; m_rowData_XY[j][strTmp2] = sData;
                                                //        sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                //        strTmp2 = pstrData2[k] + "W"; m_rowData_XY[j][strTmp2] = Math.Round(CalcEvd2Angle(sData*4), 1);
                                                //        //sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                //        strTmp2 = pstrData2[k] + "Ovr"; m_rowData_XY[j][strTmp2] = 0;
                                                //    }

                                                //    // Tail
                                                //    sData = BitConverter.ToInt16(byteArrayData, i); i += 2; m_rowData_XY[j]["Tail"] = Math.Round(CalcEvd2Angle(sData * 4), 1);
                                                //    // Pan
                                                //    sData = BitConverter.ToInt16(byteArrayData, i); i += 2; m_rowData_XY[j]["Pan"] = Math.Round(CalcEvd2Angle(sData * 4), 1);
                                                //    // Tilt                                                
                                                //    sData = BitConverter.ToInt16(byteArrayData, i); i += 2; m_rowData_XY[j]["Tilt"] = Math.Round(CalcEvd2Angle(sData * 4), 1);

                                                //    TableMove_XY2Evd(j);
                                                //    TableMove_Evd2Angle(j);
                                                //}*/
                                                ////else
                                                ////{
                                                ////    // Size
                                                ////    sData = (short)((byteArrayData[i++] & 0xff) * 256);
                                                ////    sData += (short)(byteArrayData[i++] & 0xff);

                                                ////    m_nDataSize = C_MAX_MOTION_FRAME_SIZE;// (int)sData;
                                                ////    // Title
                                                ////    txtTableName.Text = System.Text.Encoding.Default.GetString(byteArrayData, i, 20);
                                                ////    //txtTableName.Text = System.Text.Encoding.Default.GetString(byteArrayTitle, 0, 20);
                                                ////    i += 21;

                                                ////    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                ////    TableClear(DISP_XY);
                                                ////    DataClear();

                                                ////    //// Data                                            
                                                ////    int nTmp = sData;
                                                ////    for (j = 0; j < m_nDataSize; j++)
                                                ////    {
                                                ////        //Index
                                                ////        m_rowData[j][m_pstrData[0]] = j;
                                                ////        // En
                                                ////        if (j < nTmp)
                                                ////            m_rowData[j][m_pstrData[1]] = 1;
                                                ////        // Action Data
                                                ////        //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                                ////        for (int k = 0; k < 3; k++)
                                                ////        {
                                                ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                                ////        }
                                                ////        for (int k = 6; k < 9; k++)
                                                ////        {
                                                ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                                ////        }
                                                ////        for (int k = 3; k < 6; k++)
                                                ////        {
                                                ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                                ////        }
                                                ////        for (int k = 9; k < 21; k++)
                                                ////        {
                                                ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                                ////        }
                                                ////        //i += 2;
                                                ////    }
                                                ////}                                           
                                            }
                                            catch
                                            {
                                                CMessage.Write_Error("업로드 Error\n");
                                            }
                                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                                        }
                                        break;
                                        #endregion 모션 Upload
                                    #region Evd8 Data Get
                                    case 0x06:
                                        {
                                            strMessage1 += "[Evd 8 Data Get]\r\n";
                                            //// 12 - Temp, 13 - Yaw, 14 - Mouth, 15~17 - Pan, Tilt, Tail
                                            ////i = 0;
                                            ////lbLfW.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbLfDn.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbLfUp.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                            ////lbRfW.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbRfDn.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbRfUp.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                            ////lbLrW.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbLrDn.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbLrUp.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                            ////lbRrW.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbRrDn.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbRrUp.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                            //////i++; // Temp
                                            ////lbTiltUp.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbMouth.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                            ////lbPan.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbTilt.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbTail.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                        }
                                        break;
                                    #endregion Evd8 Data Get
                                    #region 모션목록확인
                                    case 0x0c:
                                        {
                                            ///*listviewGenibo_By_FileName.Clear();
                                            //strMessage1 += "[모션목록확인]\r\n";
                                            //int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                            ////int nIndex;
                                            ////m_nMotionListCode = new int[nListCount];
                                            ////m_nMotionListCount = nListCount;
                                            //for (i = 0; i < nListCount; i++)
                                            //{
                                            //    //strMessage1 += "[Index=" + CConvert.IntToStr(nIndex) + "]";
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, i * 256 + 2, 256);
                                            //    //strMessage1 += strTmp.Trim('\0') + "\r\n";

                                            //    pstrData = strTmp.Split('/');
                                            //    int j = 0;
                                            //    //string strTmp2;
                                            //    foreach (string strItem in pstrData)
                                            //    {
                                            //        if (String.Compare(strItem, "master") == 0)
                                            //            j = 0;
                                            //        else if (String.Compare(strItem, "user") == 0)
                                            //            j = 1;
                                            //        else if (String.Compare(strItem, "memory") == 0)
                                            //            j = 2;
                                            //        //    else strTmp2 = strItem;
                                            //    }

                                            //    listviewGenibo_By_FileName.Items.Add(CConvert.IntToStr(i), strTmp.Trim('\0'), j);
                                            //    //m_nMotionListCode[i] = nIndex;
                                            //}*/
                                        }
                                        break;
                                    #endregion 모션목록확인
                                    #region Motion Upload
                                    case 0x0d:
                                        {
                                            ////try
                                            ////{
                                            ////    strMessage1 += "[모션 업로드]\r\n";
                                            ////    short sData;
                                            ////    i = 0;
                                            ////    int j;

                                            ////    // Size
                                            ////    sData = (short)((byteArrayData[i++] & 0xff) * 256);
                                            ////    sData += (short)(byteArrayData[i++] & 0xff);
                                            ////    m_nDataSize = C_MAX_MOTION_FRAME_SIZE;// (int)sData;
                                            ////    txtTableName.Text = System.Text.Encoding.Default.GetString(byteArrayData, i, 20);
                                            ////    i += 21;

                                            ////    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

                                            ////    TableClear(DISP_XY);
                                            ////    DataClear();

                                            ////    // Data
                                            ////    int nTmp = sData;
                                            ////    for (j = 0; j < m_nDataSize; j++)
                                            ////    {
                                            ////        //Index
                                            ////        m_rowData[j][m_pstrData[0]] = j;
                                            ////        // En
                                            ////        if (j < nTmp)
                                            ////            m_rowData[j][m_pstrData[1]] = 1;
                                            ////        // Action Data
                                            ////        //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            ////        for (int k = 0; k < 3; k++)
                                            ////        {
                                            ////            //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                            ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                            ////        }
                                            ////        for (int k = 6; k < 9; k++)
                                            ////        {
                                            ////            //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                            ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                            ////        }
                                            ////        for (int k = 3; k < 6; k++)
                                            ////        {
                                            ////            //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                            ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                            ////        }
                                            ////        for (int k = 9; k < 21; k++)
                                            ////        {
                                            ////            //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                            ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                            ////        }
                                            ////        //i += 2;
                                            ////    }
                                            ////    // XY Grid => Evd8 Grid
                                            ////    XYGrid2Evd8Grid();
                                            ////}
                                            ////catch
                                            ////{
                                            ////    XYGrid2Evd8Grid();
                                            ////    CMessage.Write("업로드 Error\n");
                                            ////}
                                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                                        }
                                        break;
                                    #endregion Motion Upload
                                    #region Angle Data Get
                                    case 0x12:
                                        {
                                            nID = byteArrayData[0];
                                            strMessage1 += "[Angle(" + CConvert.IntToStr(nID) + ") Get]\r\n";
                                            int nValue = (int)(byteArrayData[1] * 256 * 256 * 256 + byteArrayData[2] * 256 * 256 + byteArrayData[3] * 256 + byteArrayData[4]);
                                            nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            strTmp = CConvert.IntToStr(nValue);

                                            //if (nID == LEFT_FRONT_DOWN_ID) txtAngle_LfDn.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_UP_ID) txtAngle_LfUp.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_WING_ID) txtAngle_LfW.Text = strTmp;
                                            //else if (nID == LEFT_REAR_DOWN_ID) txtAngle_LrDn.Text = strTmp;
                                            //else if (nID == LEFT_REAR_UP_ID) txtAngle_LrUp.Text = strTmp;
                                            //else if (nID == LEFT_REAR_WING_ID) txtAngle_LrW.Text = strTmp;

                                            //else if (nID == RIGHT_FRONT_DOWN_ID) txtAngle_RfDn.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_UP_ID) txtAngle_RfUp.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_WING_ID) txtAngle_RfW.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_DOWN_ID) txtAngle_RrDn.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_UP_ID) txtAngle_RrUp.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_WING_ID) txtAngle_RrW.Text = strTmp;

                                            //else if (nID == HEAD_TILTUP_ID) txtAngle_TiltUp.Text = strTmp;
                                            //else if (nID == HEAD_MOUTH_ID) txtAngle_Mouth.Text = strTmp;
                                            //else if (nID == HEAD_PAN_ID) txtAngle_Pan.Text = strTmp;
                                            //else if (nID == HEAD_TILT_ID) txtAngle_Tilt.Text = strTmp;
                                            //else if (nID == HIPS_TAIL_ID) txtAngle_Tail.Text = strTmp;
                                        }
                                        break;
                                    #endregion Angle Data Get
                                    #region Angle Data Get(All)
                                    case 0x22:
                                        {
                                            strMessage1 += "[Angle(All) Get]\r\n";
                                            //int j = 0, nValue;
                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LfW.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LfDn.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LfUp.Text = strTmp;


                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RfW.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RfDn.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RfUp.Text = strTmp;


                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LrW.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LrDn.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LrUp.Text = strTmp;


                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RrW.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RrDn.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RrUp.Text = strTmp;

                                            //j++; // Temp
                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_TiltUp.Text = strTmp;
                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_Mouth.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_Pan.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_Tilt.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_Tail.Text = strTmp;

                                            //if (m_bDisp == true) Angle_Display();
                                            //m_bDisp = false;
                                        }
                                        break;
                                    #endregion Angle Data Get(All)
                                    #region 마스터 모션목록확인
                                    case 0x31:
                                        {
#if false
                                            listviewGenibo.Clear();
                                            strMessage1 += "[마스터 모션목록확인]\r\n";

                                            short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            //short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            int nIndex;
                                            m_nMotionListCode = new int[sListCount];
                                            string strTmpFileName = "";
                                            string strTmpTitleName = "";

                                            for (i = 0; i < sListCount; i++)
                                            {
                                                try
                                                {
                                                    // Title & FileName
                                                    // Title
                                                    strTmpTitleName = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16), 21);
                                                    nIndex = strTmpTitleName.IndexOf('\0');
                                                    if (nIndex > 0)
                                                        strTmpTitleName = strTmpTitleName.Remove(nIndex);
                                                    // FileName
                                                    strTmpFileName = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16) + 21, 16);
                                                    nIndex = strTmpFileName.IndexOf('\0');
                                                    if (nIndex > 0)
                                                        strTmpFileName = strTmpFileName.Remove(nIndex);
                                                    else if (nIndex < 0)
                                                    {
                                                        nIndex = strTmpFileName.IndexOf(".act");
                                                        if (nIndex > 0)
                                                            strTmpFileName = strTmpFileName.Remove(nIndex);
                                                    }
                                                    strTmpFileName += ".act";

                                                    strTmp = strTmpFileName;//
                                                    strTmp += "[";
                                                    if (strTmpTitleName != "") strTmp += strTmpTitleName;
                                                    strTmp += "]";

                                                    //// FileName
                                                    //strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16) + 21, 16);
                                                    //nIndex = strTmp.IndexOf('\0');
                                                    //if (nIndex == 0)
                                                    //    continue;
                                                    //strTmp = strTmp.Remove(nIndex);
                                                    //strTmp += ".act[";
                                                    //// Title
                                                    //strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16), 21);
                                                    //nIndex = strTmp.IndexOf('\0');
                                                    //strTmp = strTmp.Remove(nIndex);
                                                    //strTmp += "]";

                                                    listviewGenibo.Items.Add(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                                }
                                                catch
                                                {
                                                    CMessage.Write_Error("ListGet Error");
                                                }
                                            }
#else
                                            //listviewGenibo.Clear();
                                            strMessage1 += "[마스터 모션목록확인]\r\n";

                                            short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            //short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            int nIndex;
                                            //m_nMotionListCode = new int[sListCount];
                                            string strTmpFileName = "";
                                            string strTmpTitleName = "";

                                            for (i = 0; i < sListCount; i++)
                                            {
                                                try
                                                {
                                                    // Title & FileName
                                                    // Title
                                                    strTmpTitleName = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16), 21);
                                                    nIndex = strTmpTitleName.IndexOf('\0');
                                                    if (nIndex > 0)
                                                        strTmpTitleName = strTmpTitleName.Remove(nIndex);
                                                    // FileName
                                                    strTmpFileName = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16) + 21, 16);
                                                    nIndex = strTmpFileName.IndexOf('\0');
                                                    if (nIndex > 0)
                                                        strTmpFileName = strTmpFileName.Remove(nIndex);
                                                    else if (nIndex < 0)
                                                    {
                                                        nIndex = strTmpFileName.IndexOf(".act");
                                                        if (nIndex > 0)
                                                            strTmpFileName = strTmpFileName.Remove(nIndex);
                                                    }
                                                    strTmpFileName += ".act";

                                                    strTmp = strTmpFileName;//
                                                    strTmp += "[";
                                                    if (strTmpTitleName != "") strTmp += strTmpTitleName;
                                                    strTmp += "]";

                                                    CMessage.Write(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                                }
                                                catch
                                                {
                                                    CMessage.Write_Error("ListGet Error");
                                                }
                                            }
#endif
                                        }
                                        break;
                                    #endregion 마스터 모션목록확인
                                    #region 유저 모션목록확인
                                    case 0x32:
                                        {
#if false
                                            listviewGenibo_User.Clear();
                                            strMessage1 += "[유저 모션목록확인]\r\n";

                                            short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            //short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            int nIndex;
                                            m_nMotionListCode = new int[sListCount];
                                            for (i = 0; i < sListCount; i++)
                                            {
                                                // Title & FileName
                                                // FileName
                                                strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + C_TCP_FILENAME_SIZE) + 21, C_TCP_FILENAME_SIZE);
                                                nIndex = strTmp.IndexOf('\0');
                                                strTmp = strTmp.Remove(nIndex);
                                                strTmp += ".act[";
                                                // Title
                                                strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + C_TCP_FILENAME_SIZE), 21);
                                                nIndex = strTmp.IndexOf('\0');
                                                strTmp = strTmp.Remove(nIndex);
                                                strTmp += "]";

                                                //// Title
                                                //strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16), 21);
                                                //nIndex = strTmp.IndexOf('\0');
                                                //strTmp = strTmp.Remove(nIndex);
                                                //strTmp += "[";
                                                //// FileName
                                                //strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16) + 21, 16);
                                                //nIndex = strTmp.IndexOf('\0');
                                                //strTmp = strTmp.Remove(nIndex);
                                                //strTmp += ".act]";

                                                listviewGenibo_User.Items.Add(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                            }
#else
                                            //listviewGenibo_User.Clear();
                                            strMessage1 += "[유저 모션목록확인]\r\n";

                                            short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            //short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            int nIndex;
                                            //m_nMotionListCode = new int[sListCount];
                                            for (i = 0; i < sListCount; i++)
                                            {
                                                // Title & FileName
                                                // FileName
                                                strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + C_TCP_FILENAME_SIZE) + 21, C_TCP_FILENAME_SIZE);
                                                nIndex = strTmp.IndexOf('\0');
                                                strTmp = strTmp.Remove(nIndex);
                                                strTmp += ".act[";
                                                // Title
                                                strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + C_TCP_FILENAME_SIZE), 21);
                                                nIndex = strTmp.IndexOf('\0');
                                                strTmp = strTmp.Remove(nIndex);
                                                strTmp += "]";
                                                
                                                CMessage.Write(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                            }
#endif
                                        }
                                        break;
                                    #endregion 유저 모션목록확인
                                    #region Motion Load(master)
                                    case 0x35:
                                        {
                                            string strFileName = System.Text.Encoding.Default.GetString(byteArrayData, 0, C_TCP_FILENAME_SIZE);
//#if false
//                                        strFileName = m_strWorkDirectory + "\\" + strFileName.Trim('\0') + ".act";
//                                        FileInfo f = new FileInfo(strFileName);
//                                        FileStream fs = f.Create();
//                                        fs.Write(byteArrayData, C_TCP_FILENAME_SIZE, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                        fs.Close();
//#else
//                                            byte[] pbyteBuffer = new byte[byteArrayData.Length - C_TCP_FILENAME_SIZE];

//                                            Array.Copy(byteArrayData, C_TCP_FILENAME_SIZE, pbyteBuffer, 0, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                            //byteArrayData.CopyTo(pbyteBuffer, C_TCP_FILENAME_SIZE);//, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                            if (DataFileOpen(strFileName.Trim('\0'), pbyteBuffer, true, true) == false)
//                                                CMessage.Write_Error("Data File Receive Error");
//#endif
//                                            // m_strWorkDirectory + "\\";
                                        }
                                        break;
                                    #endregion Motion Load(master)
                                    #region Motion Load(User)
                                    case 0x36:
                                        {
                                            string strFileName = System.Text.Encoding.Default.GetString(byteArrayData, 0, C_TCP_FILENAME_SIZE);
//#if false
//                                        strFileName = m_strWorkDirectory + "\\" + strFileName.Trim('\0') + ".act";
//                                        FileInfo f = new FileInfo(strFileName);
//                                        FileStream fs = f.Create();
//                                        fs.Write(byteArrayData, C_TCP_FILENAME_SIZE, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                        fs.Close();
//#else
//                                            byte[] pbyteBuffer = new byte[byteArrayData.Length - C_TCP_FILENAME_SIZE];

//                                            Array.Copy(byteArrayData, C_TCP_FILENAME_SIZE, pbyteBuffer, 0, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                            //byteArrayData.CopyTo(pbyteBuffer, C_TCP_FILENAME_SIZE);//, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                            if (DataFileOpen(strFileName.Trim('\0'), pbyteBuffer, true, true) == false)
//                                                CMessage.Write_Error("Data File Receive Error");
//#endif
//                                            // m_strWorkDirectory + "\\";
                                        }
                                        break;
                                    #endregion Motion Load(User)
                                    #region Qwm3(Weight)
                                    case 0x3b:
                                        {
                                            //int nPos = 0;
                                            //float[] fValue = new float[5];
                                            //for (i = 0; i < 5; i++)
                                            //{
                                            //    fValue[i] = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_TurnWeight = fValue[i];
                                            //}
                                            //DispQwm3Data_Only_Weight();
                                        }
                                        break;
                                    #endregion Qwm3(Weight)
                                    case 0x3d:
                                        {
                                            if (nLength != 2 + ((15 + 3) * sizeof(short))) { MessageBox.Show("패킷 사이즈 에러"); break; }

                                            ////if ((byte)(byteArrayData[1]) == 0)
                                            ////{
                                            ////    if (m_nCalibration == _CALIBRATION_SAVE)
                                            ////    {
                                            ////        strTmp = "[저장 성공]";
                                            ////    }
                                            ////    else if (m_nCalibration == _CALIBRATION_LOAD)
                                            ////    {
                                            ////        strTmp = "[ReLoaded Calibration Data]";
                                            ////    }
                                            ////    else if (m_nCalibration == _CALIBRATION_CLEAR)
                                            ////    {
                                            ////        strTmp = "[Cleared Calibration Data]";
                                            ////    }
                                            ////    else
                                            ////    {
                                            ////        strTmp = "캘리브레이션 문제 발생 - 리턴값 종류 확인 불가(윈도우 프로그램[m_nCalibration] 문제)";
                                            ////    }

                                            ////}
                                            ////else if ((byte)(byteArrayData[1]) == 1)
                                            ////    strTmp = "[Process End]";
                                            ////else if ((byte)(byteArrayData[1]) == 2) strTmp = "[실패 - 다시 시도하시오.]";
                                            ////else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                            ////else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";

                                            ////MessageBox.Show("[Calibration] => " + strTmp);


                                            int nPos = 0;
                                            int nMode = byteArrayData[nPos++]; // Mode : 0 - save, 1 - reload,  2 - clear 
                                            int nResult = byteArrayData[nPos++]; // Result : 0 - Error, 1 - OK
                                            int nErrorType = byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            int nErrorRange = byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            int nErrorVibrationRange = byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            int[] nValue = new int[15];
                                            for (i = 0; i < 15; i++)
                                            {
                                                nValue[i] = byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            }

                                            String strError = "";
                                            if (nMode == _CALIBRATION_SAVE)
                                            {
                                                strError = "[Save " + ((nResult == 1) ? "Ok]" : "Fail]") + "\n";

                                                if (nErrorType > 0)
                                                {
                                                    strError += ((nErrorType == 1) ? "관절 조립 상태 불량" : ((nErrorType == 2) ? "모터가 흔들리지 않게 고정 하고 다시 시도하십시오." : "알 수 없는 에러 발생"));
                                                    strError += "\n기준값 = " + CConvert.IntToStr(((nErrorType == 1) ? nErrorRange : nErrorVibrationRange)) + "\n";
                                                    for (i = 0; i < 11; i++) // 까지만 검사하자 - 머리 불필요
                                                    {
                                                        if (nValue[i] != 0) strError += "[" + CConvert.IntToStr(i) + "번 모터(" + CConvert.IntToStr(nValue[i]) + ")]\n";
                                                    }
                                                }
                                            }
                                            else if (nMode == _CALIBRATION_LOAD)
                                            {
                                                strError = "[ReLoaded Calibration Data - " + ((nResult == 1) ? "Ok]" : "Fail]");
                                            }
                                            else if (nMode == _CALIBRATION_CLEAR)
                                            {
                                                strError = "[Cleared Calibration Data]";
                                            }
                                            else
                                            {
                                                strError = "캘리브레이션 문제 발생 - 리턴값 종류 확인 불가";
                                            }

                                            MessageBox.Show(strError);
                                        }
                                        break;
                                    case 0x3e:
                                        {
                                            ////if (nLength != 3 + ) { MessageBox.Show("패킷 사이즈 에러"); break; }

                                            //int nPos = 0;
                                            //int nMode = byteArrayData[nPos++]; // 0 - 개별모터 동작, 1 - 모션을 이용하여 동작
                                            //int nResult = byteArrayData[nPos++]; // Result : 0 - Error, 1 - OK
                                            //int nMotID = byteArrayData[nPos++]; // Motor ID
                                            //short sDiff_Max = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //short sDiff_Min = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //short sTemp_Max = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //short sTemp_Min = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //short sVolt_Max = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //short sVolt_Min = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //int nFileSize = (int)(
                                            //                        byteArrayData[nPos] * 256 * 256 * 256 +
                                            //                        byteArrayData[nPos + 1] * 256 * 256 +
                                            //                        byteArrayData[nPos + 2] * 256 +
                                            //                        byteArrayData[nPos + 3]); nPos += 4;
                                            //String strError = (nMode == 0) ? "[모터체크 - 개별모터 동작]" : "[모터체크 - 모션을 이용한 동작]\n";
                                            //strError += CConvert.IntToStr(nMotID) + "번 모터 : " + ((nResult != 0) ? "Ok" : "Fail") + "\n";
                                            //strError += "Diff[Max = " + CConvert.IntToStr(sDiff_Max) + ", Min = " + CConvert.IntToStr(sDiff_Min) + "]\n";
                                            //strError += "Temp[Max = " + CConvert.IntToStr(sTemp_Max) + ", Min = " + CConvert.IntToStr(sTemp_Min) + "]\n";
                                            //strError += "Volt[Max = " + CConvert.IntToStr(sVolt_Max) + ", Min = " + CConvert.IntToStr(sVolt_Min) + "]\n";

                                            //MessageBox.Show(strError);

                                            //if (nFileSize > 0)
                                            //{
                                            //    String strSaveFileName = "c:\\test.csv";
                                            //    if (CInputBox.Show("File", "저장할 파일의 이름을 입력하시오", ref strSaveFileName) == DialogResult.OK)
                                            //    {
                                            //        byte[] pFile = new byte[nFileSize];
                                            //        Array.Copy(byteArrayData, nPos, pFile, 0, nFileSize);
                                            //        BinaryTestFileSave(strSaveFileName, nFileSize, pFile);
                                            //        pFile = null;
                                            //    }
                                            //}
                                        }
                                        break;
                                    case (0x40 + 20):
                                        {
                                            //switch (byteArrayData[0])
                                            //{
                                            //    case 2: // 1.33 ~ 1.70
                                            //        {
                                            //            CMessage.Write("ACT_CMD_GET_PARAM_QWM3");
                                            //            int j;
                                            //            int nPos = 1;
                                            //            for (i = 0; i < 2; i++)
                                            //                for (j = 0; j < 73; j++)
                                            //                {
                                            //                    g_afParamQwm[i, j] = BitConverter.ToSingle(byteArrayData, nPos);
                                            //                    nPos += sizeof(float);
                                            //                }
                                            //            DispQwm3Data();

                                            //            SendCommand(_MOTION, 0x3B);// 일반 가중치도 가져온다.
                                            //        }
                                            //        break;
                                            //    case 4: // 1.71
                                            //        {
                                            //            CMessage.Write("ACT_CMD_GET_PARAM_QWM3_V_1_71");
                                            //            int j;
                                            //            int nPos = 1;
                                            //            for (i = 0; i < 2; i++)
                                            //                for (j = 0; j < (73 + 10); j++)
                                            //                {
                                            //                    g_afParamQwm[i, j] = BitConverter.ToSingle(byteArrayData, nPos);
                                            //                    nPos += sizeof(float);
                                            //                }
                                            //            DispQwm3Data();

                                            //            SendCommand(_MOTION, 0x3B);// 일반 가중치도 가져온다.
                                            //        }
                                            //        break;
                                            //    default:
                                            //        CMessage.Write("(ETC)알수 없는 명령");
                                            //        break;
                                            //}
                                        }
                                        break;
                                    ////                                 case (0x40 + 36):
                                    ////                                     {
                                    ////                                         if (byteArrayData.Rank >= 1)
                                    ////                                         {
                                    ////                                             m_bReceiveOk = true; // 일단 데이터를 받으면 그냥 줌...
                                    ////                                             if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    ////                                             else if ((byte)(byteArrayData[1]) == 1)
                                    ////                                                 strTmp = "[Process End]";
                                    ////                                             else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process Fail]";
                                    ////                                             else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    ////                                             else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                    ////                                         }
                                    ////                                         //strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "size=" + CConvert.IntToHex(nLength) + "\r\n";
                                    ////                                         //CMessage.Write(strMessage1);
                                    //// 
                                    ////                                         if (nCmd == 0x40 + 36)
                                    ////                                         {
                                    ////                                             MessageBox.Show(strTmp);
                                    ////                                         }
                                    ////                                     }
                                    ////                                     break;
                                    case (0x40 + 42):
                                        {
                                            //int nPos = 0;
                                            //for (i = 0; i < 5; i++)
                                            //{
                                            //    g_aSParamQwm[i].SReady.fDownH_Leg_F = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fDownH_Leg_R = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fDownGap = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fLeg_LR_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fLeg_LR_D = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fLeg_RF_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fLeg_RF_D = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fSway_Left = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fSway_Right = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fSway_Back = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.nSpeed = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SReady.fPercent = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);

                                            //    g_aSParamQwm[i].SWalking.fLeg_F_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_R_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_D = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_Turn = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_TurnWeight = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_Damp = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fSway = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.nSpeed_1 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.nDelay_1 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.nSpeed_2 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.nDelay_2 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.nSpeed_3 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.nDelay_3 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.fPercent = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);

                                            //    g_aSParamQwm[i].SEnd.fLeg_F_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SEnd.fLeg_R_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SEnd.fSway = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SEnd.nSpeed = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SEnd.fPercent = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //}
                                            //DispQwmData();
                                        }
                                        break;
                                    case 0x40 + 47:
                                        {
                                            //int nPos = 0;
                                            //bool bCliff = (byteArrayData[nPos] == 0) ? false : true; nPos++;
                                            //bool bObstacle = (byteArrayData[nPos] == 0) ? false : true; nPos++;
                                            ////////////////////
                                            //DisplayCliffEn(bCliff);
                                            //DisplayObstacleEn(bObstacle);
                                            //int j;
                                            //int nCnt = 3;
                                            //for (i = 0; i < 2; i++)
                                            //    for (j = 0; j < nCnt; j++)
                                            //    {
                                            //        m_anCliff[i, j] = byteArrayData[2 + i * nCnt + j];
                                            //        m_anCliffValue[i * nCnt + j] = m_anCliff[i, j];
                                            //    }
                                            //DisplayCliff();

                                            //rbQwmCliff_Dispaly(false, m_nCliffDir_Manual);
                                            //rbQwmCliff_Dispaly(true, m_nCliffDir_Auto);
                                        }
                                        break;
                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\r\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";

                                                ////SendCommand(_MOTION, 0x40 + 42);
                                                //// 인증이 성공 되면 QWM 파라미터를 가져온다.
                                                ////if (tabPageQwm3.Focused == true)
                                                ////{
                                                //if (chkNoGetData_Qwm3.Checked == false)
                                                //{
                                                //    if (chkOldQwmParam.Checked == true) SendCmd_Etc(2); // (tabPageQwm3)Get Qwm3-Param
                                                //    else SendCmd_Etc(4);

                                                //    OjwTimer.WaitTimer(100);
                                                //    // 절벽감지의 상태를 업로드
                                                //    SendCommand(_MOTION, 0x40 + 47);
                                                //}
                                                ////}
                                                ////tbQwmWeight.Enabled = true;
                                            }
                                        }
                                        break;
                                    case 0x82:
                                        {
                                            ////strMessage1 += "[Evd 8 Get]\r\n";
                                            ////nID = byteArrayData[0];
                                            ////strTmp = CConvert.IntToStr(byteArrayData[1]);
                                            ////if (nID == LEFT_FRONT_DOWN_ID) txtLf8Dn.Text = strTmp;
                                            ////else if (nID == LEFT_FRONT_UP_ID) txtLf8Up.Text = strTmp;
                                            ////else if (nID == LEFT_FRONT_WING_ID) txtLf8W.Text = strTmp;
                                            ////else if (nID == LEFT_REAR_DOWN_ID) txtLr8Dn.Text = strTmp;
                                            ////else if (nID == LEFT_REAR_UP_ID) txtLr8Up.Text = strTmp;
                                            ////else if (nID == LEFT_REAR_WING_ID) txtLr8W.Text = strTmp;

                                            ////else if (nID == RIGHT_FRONT_DOWN_ID) txtRf8Dn.Text = strTmp;
                                            ////else if (nID == RIGHT_FRONT_UP_ID) txtRf8Up.Text = strTmp;
                                            ////else if (nID == RIGHT_FRONT_WING_ID) txtRf8W.Text = strTmp;
                                            ////else if (nID == RIGHT_REAR_DOWN_ID) txtRr8Dn.Text = strTmp;
                                            ////else if (nID == RIGHT_REAR_UP_ID) txtRr8Up.Text = strTmp;
                                            ////else if (nID == RIGHT_REAR_WING_ID) txtRr8W.Text = strTmp;

                                            ////else if (nID == HEAD_PAN_ID) txtPan8.Text = strTmp;
                                            ////else if (nID == HEAD_TILT_ID) txtTilt8.Text = strTmp;
                                            ////else if (nID == HIPS_TAIL_ID) txtTail8.Text = strTmp;
                                        }
                                        break;
                                    case 0x84:
                                        {
                                            //strMessage1 += "[Evd 10 Get]\r\n";
                                            //nID = byteArrayData[0];
                                            //strTmp = CConvert.IntToStr(byteArrayData[1] * 256 + byteArrayData[2]);
                                            //if (nID == LEFT_FRONT_DOWN_ID) txtLfDn.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_UP_ID) txtLfUp.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_WING_ID) txtLfW.Text = strTmp;
                                            //else if (nID == LEFT_REAR_DOWN_ID) txtLrDn.Text = strTmp;
                                            //else if (nID == LEFT_REAR_UP_ID) txtLrUp.Text = strTmp;
                                            //else if (nID == LEFT_REAR_WING_ID) txtLrW.Text = strTmp;

                                            //else if (nID == RIGHT_FRONT_DOWN_ID) txtRfDn.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_UP_ID) txtRfUp.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_WING_ID) txtRfW.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_DOWN_ID) txtRrDn.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_UP_ID) txtRrUp.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_WING_ID) txtRrW.Text = strTmp;

                                            //else if (nID == HEAD_PAN_ID) txtPan.Text = strTmp;
                                            //else if (nID == HEAD_TILT_ID) txtTilt.Text = strTmp;
                                            //else if (nID == HIPS_TAIL_ID) txtTail.Text = strTmp;
                                        }
                                        break;
                                    case 0x85:
                                        {
                                            //strMessage1 += "[Status Get]\r\n";
                                            //nID = byteArrayData[0];
                                            //strTmp = CConvert.IntToBinary(byteArrayData[1]);
                                            //if (nID == LEFT_FRONT_DOWN_ID) lbLfDn_Status.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_UP_ID) lbLfUp_Status.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_WING_ID) lbLfW_Status.Text = strTmp;
                                            //else if (nID == LEFT_REAR_DOWN_ID) lbLrDn_Status.Text = strTmp;
                                            //else if (nID == LEFT_REAR_UP_ID) lbLrUp_Status.Text = strTmp;
                                            //else if (nID == LEFT_REAR_WING_ID) lbLrW_Status.Text = strTmp;

                                            //else if (nID == RIGHT_FRONT_DOWN_ID) lbRfDn_Status.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_UP_ID) lbRfUp_Status.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_WING_ID) lbRfW_Status.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_DOWN_ID) lbRrDn_Status.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_UP_ID) lbRrUp_Status.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_WING_ID) lbRrW_Status.Text = strTmp;

                                            //else if (nID == HEAD_PAN_ID) lbPan_Status.Text = strTmp;
                                            //else if (nID == HEAD_TILT_ID) lbTilt_Status.Text = strTmp;
                                            //else if (nID == HIPS_TAIL_ID) lbTail_Status.Text = strTmp;

                                        }
                                        break;
                                    case 0x94:
                                        {
                                            //strMessage1 += "[Gain Get]\r\n";
                                            ////lbGainID.Text = "ID=" + CConvert.IntToHex(byteArrayData[0]);
                                            //txtKp.Text = CConvert.IntToStr(byteArrayData[1] * 256 + byteArrayData[2]);
                                            //txtKi.Text = CConvert.IntToStr(byteArrayData[3] * 256 + byteArrayData[4]);
                                            //txtKd.Text = CConvert.IntToStr(byteArrayData[5] * 256 + byteArrayData[6]);
                                            ////txtGrad.Text = CConvert.IntToStr(byteArrayData[7]);
                                        }
                                        break;
                                    case 0xA2:
                                        {
                                            strMessage1 += "[Evd 8 Data Get]\r\n";
                                            //// 12 - Temp, 13 - Yaw, 14 - Mouth, 15~17 - Pan, Tilt, Tail
                                            ////i = 0;
                                            ////txtLf8W.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_WING_ID]));
                                            ////txtLf8Dn.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_DOWN_ID]));
                                            ////txtLf8Up.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_UP_ID]));

                                            ////txtLr8W.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_WING_ID]));
                                            ////txtLr8Dn.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_DOWN_ID]));
                                            ////txtLr8Up.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_UP_ID]));

                                            ////txtRf8W.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_WING_ID]));
                                            ////txtRf8Dn.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_DOWN_ID]));
                                            ////txtRf8Up.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_UP_ID]));

                                            ////txtRr8W.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_WING_ID]));
                                            ////txtRr8Dn.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_DOWN_ID]));
                                            ////txtRr8Up.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_UP_ID]));

                                            ////txtPan8.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_PAN_ID]));
                                            ////txtTilt8.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_TILT_ID]));
                                            ////txtTail8.Text = CConvert.IntToStr((int)(byteArrayData[HIPS_TAIL_ID]));

                                            ////txtYaw8.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_YAW_ID]));
                                            ////txtMouth8.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_MOUTH_ID]));

                                        }
                                        break;
                                    case 0xA4:
                                        {
                                            strMessage1 += "[Evd 10 Data Get]\r\n";

                                            ////if ((dataGrid_XY.Focused == true) || (dataGrid_Angle.Focused == true) || (dataGrid_Evd.Focused == true))
                                            //if (m_bGetCurrentPosition == true)
                                            //{
                                            //    m_bGetCurrentPosition = false;
                                            //    //////// Data /////////////////////////////////////////////////////////////////////////////////////////////
                                            //    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                                            //    //// Data                                            
                                            //    //int nTmp = sData;
                                            //    int j = 0;
                                            //    if (m_nPage == DISP_XY) j = dataGrid_XY.CurrentCell.RowIndex;
                                            //    else if (m_nPage == DISP_ANGLE) j = dataGrid_Angle.CurrentCell.RowIndex;
                                            //    else j = dataGrid_Evd.CurrentCell.RowIndex;
                                            //    DataClear(DISP_EVD, j);
                                            //    //Index
                                            //    m_rowData_Evd[j]["Index"] = j;
                                            //    // En
                                            //    //if (j < nTmp)
                                            //    m_rowData_Evd[j]["En"] = 1;
                                            //    // Action Data
                                            //    //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            //    string[] pstrData2 = new string[4] { "Lf", "Rf", "Lr", "Rr" };
                                            //    string strTmp2;
                                            //    for (int k = 0; k < 4; k++)
                                            //    {
                                            //        strTmp2 = pstrData2[k] + "W"; m_rowData_Evd[j][strTmp2] = (short)(byteArrayData[(k * 3 + LEFT_FRONT_WING_ID) * 2] * 256 + byteArrayData[(k * 3 + LEFT_FRONT_WING_ID) * 2 + 1]);
                                            //        strTmp2 = pstrData2[k] + "Dn"; m_rowData_Evd[j][strTmp2] = (short)(byteArrayData[(k * 3 + LEFT_FRONT_DOWN_ID) * 2] * 256 + byteArrayData[(k * 3 + LEFT_FRONT_DOWN_ID) * 2 + 1]);
                                            //        strTmp2 = pstrData2[k] + "Up"; m_rowData_Evd[j][strTmp2] = (short)(byteArrayData[(k * 3 + LEFT_FRONT_UP_ID) * 2] * 256 + byteArrayData[(k * 3 + LEFT_FRONT_UP_ID) * 2 + 1]);
                                            //    }

                                            //    // Tail
                                            //    m_rowData_Evd[j]["Tail"] = (short)(byteArrayData[HIPS_TAIL_ID * 2] * 256 + byteArrayData[HIPS_TAIL_ID * 2 + 1]);
                                            //    // Pan
                                            //    m_rowData_Evd[j]["Pan"] = (short)(byteArrayData[HEAD_PAN_ID * 2] * 256 + byteArrayData[HEAD_PAN_ID * 2 + 1]);
                                            //    // Tilt                                                
                                            //    m_rowData_Evd[j]["Tilt"] = (short)(byteArrayData[HEAD_TILT_ID * 2] * 256 + byteArrayData[HEAD_TILT_ID * 2 + 1]);

                                            //    //Data0
                                            //    m_rowData_Evd[j]["Data0"] = m_rowData_XY[j]["Data0"];
                                            //    m_rowData_Evd[j]["Data1"] = m_rowData_XY[j]["Data1"];
                                            //    TableMove_Evd2Angle(j);
                                            //    TableMove_Evd2XY(j);
                                            //}
                                            //else
                                            //{
                                            //    // 12 - Temp, 13 - Yaw, 14 - Mouth, 15~17 - Pan, Tilt, Tail
                                            //    i = 0;
                                            //    txtLfW.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_WING_ID * 2] * 256 + byteArrayData[LEFT_FRONT_WING_ID * 2 + 1]));
                                            //    txtLfDn.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_DOWN_ID * 2] * 256 + byteArrayData[LEFT_FRONT_DOWN_ID * 2 + 1]));
                                            //    txtLfUp.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_UP_ID * 2] * 256 + byteArrayData[LEFT_FRONT_UP_ID * 2 + 1]));

                                            //    txtLrW.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_WING_ID * 2] * 256 + byteArrayData[LEFT_REAR_WING_ID * 2 + 1]));
                                            //    txtLrDn.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_DOWN_ID * 2] * 256 + byteArrayData[LEFT_REAR_DOWN_ID * 2 + 1]));
                                            //    txtLrUp.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_UP_ID * 2] * 256 + byteArrayData[LEFT_REAR_UP_ID * 2 + 1]));

                                            //    txtRfW.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_WING_ID * 2] * 256 + byteArrayData[RIGHT_FRONT_WING_ID * 2 + 1]));
                                            //    txtRfDn.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_DOWN_ID * 2] * 256 + byteArrayData[RIGHT_FRONT_DOWN_ID * 2 + 1]));
                                            //    txtRfUp.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_UP_ID * 2] * 256 + byteArrayData[RIGHT_FRONT_UP_ID * 2 + 1]));

                                            //    txtRrW.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_WING_ID * 2] * 256 + byteArrayData[RIGHT_REAR_WING_ID * 2 + 1]));
                                            //    txtRrDn.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_DOWN_ID * 2] * 256 + byteArrayData[RIGHT_REAR_DOWN_ID * 2 + 1]));
                                            //    txtRrUp.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_UP_ID * 2] * 256 + byteArrayData[RIGHT_REAR_UP_ID * 2 + 1]));

                                            //    txtPan.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_PAN_ID * 2] * 256 + byteArrayData[HEAD_PAN_ID * 2 + 1]));
                                            //    txtTilt.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_TILT_ID * 2] * 256 + byteArrayData[HEAD_TILT_ID * 2 + 1]));
                                            //    txtTail.Text = CConvert.IntToStr((int)(byteArrayData[HIPS_TAIL_ID * 2] * 256 + byteArrayData[HIPS_TAIL_ID * 2 + 1]));

                                            //    txtTiltUp.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_TILTUP_ID * 2] * 256 + byteArrayData[HEAD_TILTUP_ID * 2 + 1]));
                                            //    txtMouth.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_MOUTH_ID * 2] * 256 + byteArrayData[HEAD_MOUTH_ID * 2 + 1]));

                                            //}
                                            //if (m_bDisp == true) Evd_Display();
                                            //m_bDisp = false;
                                        }
                                        break;
                                    case 0xA5:
                                        {
                                            strMessage1 += "[All Status Get]\r\n";
                                            //// 12 - Temp, 13 - Yaw, 14 - Mouth, 15~17 - Pan, Tilt, Tail
                                            //i = 0;
                                            //lbLfW_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbLfDn_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbLfUp_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));

                                            //lbLrW_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbLrDn_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbLrUp_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));

                                            //lbRfW_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbRfDn_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbRfUp_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));

                                            //lbRrW_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbRrDn_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbRrUp_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //i += 3;
                                            //lbPan_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbTilt_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbTail_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));

                                            //lbTiltUp_Status.Text = "";//"0x" + CConvert.IntToHex((int)(byteArrayData[HEAD_TILTUP_ID]));
                                            //lbMouth_Status.Text = "";//"0x" + CConvert.IntToHex((int)(byteArrayData[HEAD_MOUTH_ID]));
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            strMessage1 += "[Version Get]\r\n";

                                            strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            CMessage.Write(strTmp);
                                        }
                                        break;
                                    case 0xE2:
                                        strData = "    =>(Data[0]=0x" + CConvert.IntToHex(byteArrayData[0]) + ")\r\n" +
                                                  "    =>(Data[1]=0x" + CConvert.IntToHex(byteArrayData[1]) + ")\r\n" +
                                                  "    =>(Data[2]=0x" + CConvert.IntToHex(byteArrayData[2]) + ")\r\n" +
                                                  "    =>(Data[3]=0x" + CConvert.IntToHex(byteArrayData[3]) + ")\r\n";
                                        strMessage1 += "[Reserve]\r\n" + strData;
                                        break;
                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                CMessage.Write(strMessage1);
                            }
                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }
            //private void FileTranse()
            //{
            //    if (OjwSock[_MOTION].m_bConnect)
            //    {
            //        openFileDialog1.Multiselect = true;
            //        openFileDialog1.FileName = null;
            //        if (openFileDialog1.ShowDialog() == DialogResult.OK)
            //        {
            //            progressFileConvert.Minimum = 0;
            //            progressFileConvert.Maximum = openFileDialog1.FileNames.Length;
            //            progressFileConvert.Value = 0;
            //            //Ojw.CMessage.Write(OjwConvert.IntToStr(openFileDialog1.FileNames.Length));
            //            int i = 0;
            //            int nFileNum = 0;
            //            bool bGetList = false;
            //            string strFileName = "";
            //            string fileName2 = "";
            //            foreach (string fileName in openFileDialog1.FileNames)
            //            {
            //                fileName2 = fileName;
            //                CMessage.Write("openFileDialog1.FileNames.Length={0}", openFileDialog1.FileNames.Length.ToString());
            //                CMessage.Write("openFileDialog1.FileNames.Rank=", openFileDialog1.FileNames.Rank.ToString());
            //                //if (i > 10) break; // 에러가 10개 이상이면 종요하도록...
            //                FileInfo f = new FileInfo(fileName);
            //                strFileName = f.Name;
            //                string strFileExt = f.Extension;
            //                if (strFileExt == ".act")
            //                {
            //                    strFileName = strFileName.Substring(0, strFileName.Length - 4);
            //                    int nFileNameSize = C_TCP_FILENAME_SIZE;
            //                    if (strFileName.Length < nFileNameSize)
            //                    {
            //                        nFileNum++;
            //                        int nType = ((rbMaster_FileDownLoad.Checked == true) ? 1 : ((rbUser_FileDownLoad.Checked == true) ? 0 : 2));
            //                        if ((nType == 2) && (nFileNum >= openFileDialog1.FileNames.Length))
            //                        {
            //                            Ojw.CMessage.Write("End");
            //                            nType = 3;
            //                            bGetList = true;
            //                        }
            //                        OjwFileTranse(nType, strFileName, fileName);

            //                        m_bReceiveOk = false;
            //                        OjwTimer.TimerSet(0);
            //                        while (m_bReceiveOk != true)
            //                        {
            //                            Application.DoEvents();
            //                            if (OjwTimer.Timer(0) > 5000)
            //                            {
            //                                i++;
            //                                break;
            //                            }
            //                        }
            //                        OjwTimer.WaitTimer(1);
            //                    }
            //                    else
            //                    {
            //                        nFileNum++;
            //                        i++;
            //                    }
            //                }
            //                else i++;

            //                progressFileConvert.Value++;
            //                Application.DoEvents();
            //            }
            //            if (bGetList == false) OjwFileTranse(3, strFileName, fileName2);
            //            // 로드 실패가 한번이라도 있으면 표시
            //            if (i > 0)
            //                CMessage.Write_Error("데이타 로드 실패횟수 = " + OjwConvert.IntToStr(i));
            //        }
            //        openFileDialog1.Multiselect = false;
            //        progressFileConvert.Value = 0;
            //    }
            //}
#if true
            public void Receive_Sensor()
            {
                int nType = _SENSOR;
                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    byte byteCheckSum = 0;
                    //string strMessage0;
                    string strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;
                            // CheckSum Data Get
                            byteCheckSum = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;

                            string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                            int i;
                            string[] pstrData;

                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1) strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process Fail]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "\r\n";
                                //CMessage.Write(strMessage1);
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                //if (chkDispHearBeat.Checked)
                                //{
                                //    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]\r\n";
                                //    //CMessage.Write(strMessage1);
                                //}
                                //else continue;
                            }
                            else
                            {
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                switch (nCmd)
                                {
                                    case 0x01:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[Sensor 상태확인]\r\n";
                                            byte bytePsd = byteArrayData[nPos++];
#if false
                                        byte [] pbyteTmp = new byte[4];
                                        for (int jj = 0; jj < 4; jj++) pbyteTmp[jj] = byteArrayData[nPos + 4 - 1 - jj];
                                        float fTiltX = BitConverter.ToSingle(pbyteTmp, 0); nPos += 4;
                                        for (int jj = 0; jj < 4; jj++) pbyteTmp[jj] = byteArrayData[nPos + 4 - 1 - jj];
                                        float fTiltY = BitConverter.ToSingle(pbyteTmp, 0); nPos += 4;
                                        pbyteTmp = null;
#else
                                            //                                          float fTiltX = BitConverter.ToSingle(byteArrayData, nPos); nPos += 4;
                                            //                                         fTiltX = (float)IPAddress.NetworkToHostOrder(fTiltX);
                                            // 
                                            //                                         //for (int jj = 0; jj < 4; jj++) pbyteTmp[jj] = byteArrayData[nPos + 4 - 1 - jj];
                                            //                                         float fTiltY = BitConverter.ToSingle(byteArrayData, nPos); nPos += 4;
                                            //                                         fTiltY = (float)IPAddress.NetworkToHostOrder(fTiltY);

                                            //byte [] pbyteTmp = new byte[4];
                                            //for (int jj = 0; jj < 4; jj++) pbyteTmp[jj] = byteArrayData[nPos + 4 - 1 - jj];
                                            Array.Reverse(byteArrayData, nPos, 4);
                                            float fTiltX = BitConverter.ToSingle(byteArrayData, nPos); nPos += 4;
                                            //for (int jj = 0; jj < 4; jj++) pbyteTmp[jj] = byteArrayData[nPos + 4 - 1 - jj];
                                            Array.Reverse(byteArrayData, nPos, 4);
                                            float fTiltY = BitConverter.ToSingle(byteArrayData, nPos); nPos += 4;
                                            //pbyteTmp = null;
#endif
                                            //ushort usTouch = IPAddress.NetworkToHostOrder(BitConverter.ToUInt16(byteArrayData, nPos)); nPos += 2;
                                            ushort usTouch = (ushort)((byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]) & 0xffff); nPos += 2;

                                            byte bytePow = byteArrayData[nPos++];
                                            byte byteImpact = byteArrayData[nPos++];
                                            //ushort usSoundLocal = BitConverter.ToUInt16(byteArrayData, nPos); nPos += 2;
                                            byte bytePir = byteArrayData[nPos++];
                                            byte byteBatteryLevel = byteArrayData[nPos++];
                                            
                                            //txtPsd.Text = CConvert.IntToStr(bytePsd);
                                            //txtTiltX.Text = Convert.ToString(fTiltX);
                                            //txtTiltY.Text = Convert.ToString(fTiltY);
                                            //chkSensorJaw.Text = CConvert.IntToBinary(usTouch >> 8 & 0xff) + " " + CConvert.IntToBinary(usTouch & 0xff);
                                            //int ii = 0;
                                            //chkSensorH.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);

                                            //chkSensorB1.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            //chkSensorB2.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            //chkSensorB3.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            //chkSensorB4.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);

                                            //chkSensorL.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            //chkSensorR.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);

                                            //chkSensorLS.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            //chkSensorRS.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);

                                            //chkSensorJaw.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            ///////////////////////////////////////////////

                                            //chkSensorRR.Checked = ((((bytePow >> 3) & 0x01) > 0) ? false : true);
                                            //chkSensorLR.Checked = ((((bytePow >> 2) & 0x01) > 0) ? false : true);
                                            //chkSensorRF.Checked = ((((bytePow >> 1) & 0x01) > 0) ? false : true);
                                            //chkSensorLF.Checked = ((((bytePow >> 0) & 0x01) > 0) ? false : true);
#if false
                                        chkSensorR.Checked = ((((byteTouch >> 6) & 0x01) > 0) ? true : false);
                                        chkSensorL.Checked = ((((byteTouch >> 5) & 0x01) > 0) ? true : false);
                                        chkSensorB4.Checked = ((((byteTouch >> 4) & 0x01) > 0) ? true : false);
                                        chkSensorB3.Checked = ((((byteTouch >> 3) & 0x01) > 0) ? true : false);
                                        chkSensorB2.Checked = ((((byteTouch >> 2) & 0x01) > 0) ? true : false);
                                        chkSensorB1.Checked = ((((byteTouch >> 1) & 0x01) > 0) ? true : false);
                                        chkSensorH.Checked = ((((byteTouch >> 0) & 0x01) > 0) ? true : false);

                                        chkSensorRR.Checked = ((((bytePow >> 3) & 0x01) > 0) ? false : true);
                                        chkSensorLR.Checked = ((((bytePow >> 2) & 0x01) > 0) ? false : true);
                                        chkSensorRF.Checked = ((((bytePow >> 1) & 0x01) > 0) ? false : true);
                                        chkSensorLF.Checked = ((((bytePow >> 0) & 0x01) > 0) ? false : true);
#endif
                                            //txtPir.Text = CConvert.IntToStr(bytePir);
                                            ////txtSoundLocal.Text = CConvert.IntToStr(usSoundLocal);
                                            //txtBattery.Text = CConvert.IntToStr(byteBatteryLevel);
                                        }
                                        break;

                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\r\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";
                                            }
                                        }
                                        break;

                                    case 0xE2:
                                        strData = "    =>(Data[0]=0x" + CConvert.IntToHex(byteArrayData[0]) + ")\r\n" +
                                                  "    =>(Data[1]=0x" + CConvert.IntToHex(byteArrayData[1]) + ")\r\n" +
                                                  "    =>(Data[2]=0x" + CConvert.IntToHex(byteArrayData[2]) + ")\r\n" +
                                                  "    =>(Data[3]=0x" + CConvert.IntToHex(byteArrayData[3]) + ")\r\n";
                                        strMessage1 += "[Reserve]\r\n" + strData;
                                        break;

                                    case 0x81:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[Sensor 상태확인]\r\n";
                                            int nSensorStatus_L = byteArrayData[nPos++];
                                            int nSensorStatus_H = byteArrayData[nPos++];//byteArrayData[nPos++] & 0x0f;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nTiltX = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nTiltY = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nTiltZ = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nGyroX = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nGyroY = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nGyroZ = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            //int nTiltX = byteArrayData[nPos++];
                                            //int nTiltY = byteArrayData[nPos++];
                                            int nBatteryLevel = byteArrayData[nPos++];
                                            int nPsd = byteArrayData[nPos++];
                                            int nImpact = byteArrayData[nPos++];
                                            int nReserve = byteArrayData[nPos++];

                                            int nPowerDrv = (nSensorStatus_H >> 2) & 0x03;
                                            int nStatus_Pir = (nSensorStatus_H >> 1) & 0x01;
                                            int nStatus_BatteryLevel = (nSensorStatus_H & 0x01);

                                            int nStatus_TiltSW = (nSensorStatus_H >> 4) & 0x01;
                                            int nStatus_Obstacle = (nSensorStatus_H >> 5) & 0x03;

                                            int nStatus_Battery = (nSensorStatus_L >> 6) & 0x03;
                                            int nStatus_Psd = ((nSensorStatus_L >> 5) & 0x01);
                                            int nStatus_SoundLoc = ((nSensorStatus_L >> 4) & 0x01);
                                            int nStatus_TiltY = ((nSensorStatus_L >> 3) & 0x01);
                                            int nStatus_TiltX = ((nSensorStatus_L >> 2) & 0x01);
                                            int nStatus_AddaptorIn = ((nSensorStatus_L >> 1) & 0x01);
                                            int nStatus_StopBtn = ((nSensorStatus_L >> 0) & 0x01);

                                            //txtObstacle.Text = CConvert.IntToStr(nStatus_Obstacle);
                                            //txtTiltSW.Text = CConvert.IntToStr(nStatus_TiltSW);
                                            //txtResult_PowerDrv.Text = CConvert.IntToStr(nPowerDrv);
                                            //txtResult_Pir.Text = CConvert.IntToStr(nStatus_Pir);
                                            //txtResult_BatterySt.Text = CConvert.IntToStr(nStatus_Battery);
                                            //txtResult_BatteryLv.Text = CConvert.IntToStr(nStatus_BatteryLevel);
                                            //txtResult_Psd.Text = CConvert.IntToStr(nStatus_Psd);
                                            //txtResult_SoundLoc.Text = CConvert.IntToStr(nStatus_SoundLoc);
                                            //txtResult_TiltY.Text = CConvert.IntToStr(nStatus_TiltY);
                                            //txtResult_TiltX.Text = CConvert.IntToStr(nStatus_TiltX);
                                            //txtResult_AdaptorIn.Text = CConvert.IntToStr(nStatus_AddaptorIn);
                                            //txtResult_StopBtn.Text = CConvert.IntToStr(nStatus_StopBtn);

                                            //txtResult_Value_TiltX.Text = CConvert.FloatToStr(nTiltX);
                                            //txtResult_Value_TiltY.Text = CConvert.FloatToStr(nTiltY);
                                            //txtResult_Value_TiltZ.Text = CConvert.FloatToStr(nTiltZ);
                                            //txtResult_Value_GyroX.Text = CConvert.FloatToStr(nGyroX);
                                            //txtResult_Value_GyroY.Text = CConvert.FloatToStr(nGyroY);
                                            //txtResult_Value_GyroZ.Text = CConvert.FloatToStr(nGyroZ);
                                            //txtResult_Value_BatteryLv.Text = CConvert.IntToStr(nBatteryLevel);
                                            //txtResult_Value_Psd.Text = CConvert.IntToStr(nPsd);
                                            //txtResult_Value_SoundLoc.Text = (nImpact > 0) ? "충격발생(" + CConvert.IntToStr(nImpact) + ")" : "충격없음(0)";//CConvert.IntToStr(usSoundLocal);
                                        }
                                        break;
                                    case 0x91:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[Rtc Status 확인]\r\n";
                                            int nWakeUp = ((byteArrayData[nPos] >> 1) & 0x01);
                                            int nSleep = ((byteArrayData[nPos++] >> 0) & 0x01);

                                            int nYear = byteArrayData[nPos++];
                                            int nMonth = byteArrayData[nPos++];
                                            int nDate = byteArrayData[nPos++];
                                            int nDay = byteArrayData[nPos++];
                                            int nHour = byteArrayData[nPos++];
                                            int nMin = byteArrayData[nPos++];
                                            int nSec = byteArrayData[nPos++];

                                            //txtResult_WakeUp.Text = CConvert.IntToStr(nWakeUp);
                                            //txtResult_Sleep.Text = CConvert.IntToStr(nSleep);

                                            //txtYear.Text = CConvert.IntToStr(nYear);
                                            //txtMonth.Text = CConvert.IntToStr(nMonth);
                                            //txtDate.Text = CConvert.IntToStr(nDate);
                                            //txtDay.Text = CConvert.IntToStr(nDay);
                                            //txtHour.Text = CConvert.IntToStr(nHour);
                                            //txtMin.Text = CConvert.IntToStr(nMin);
                                            //txtSecond.Text = CConvert.IntToStr(nSec);
                                        }
                                        break;
                                    case 0x92:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[WakeUp Alarm 응답]\r\n";
                                            int nMonth = byteArrayData[nPos++];
                                            int nDate = byteArrayData[nPos++];
                                            int nHour = byteArrayData[nPos++];
                                            int nMinute = byteArrayData[nPos++];

                                            //txtWakeUpAlarm_Month.Text = CConvert.IntToStr(nMonth);
                                            //txtWakeUpAlarm_Date.Text = CConvert.IntToStr(nDate);
                                            //txtWakeUpAlarm_Hour.Text = CConvert.IntToStr(nHour);
                                            //txtWakeUpAlarm_Minute.Text = CConvert.IntToStr(nMinute);
                                        }
                                        break;
                                    case 0x93:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[Sleep Alarm 응답]\r\n";
                                            int nMonth = byteArrayData[nPos++];
                                            int nDate = byteArrayData[nPos++];
                                            int nHour = byteArrayData[nPos++];
                                            int nMinute = byteArrayData[nPos++];

                                            //txtSleepAlarm_Month.Text = CConvert.IntToStr(nMonth);
                                            //txtSleepAlarm_Date.Text = CConvert.IntToStr(nDate);
                                            //txtSleepAlarm_Hour.Text = CConvert.IntToStr(nHour);
                                            //txtSleepAlarm_Minute.Text = CConvert.IntToStr(nMinute);
                                        }
                                        break;
                                    case 0xA1:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[리모콘 정보 응답]\r\n";
                                            int nData1 = byteArrayData[nPos++];
                                            int nData2 = byteArrayData[nPos++];

                                            //txtResult_Ir_Tx.Text = CConvert.IntToStr((nData1 >> 0) & 0x03);
                                            //txtResult_Ir_Rx.Text = CConvert.IntToStr((nData1 >> 2) & 0x03);

                                            //txtResult_Value_Ir_Rx.Text = CConvert.IntToStr(nData2);
                                        }
                                        break;
                                    case 0xA3:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[현재 세팅된 리모콘 제품 Key 응답]\r\n";
                                            int nData1 = byteArrayData[nPos++];

                                            //txtGetKey.Text = CConvert.IntToStr(nData1);
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            strMessage1 += "[Version Get]\r\n";

                                            strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            //lbVersion_Sensor.Text = strTmp;
                                        }
                                        break;
                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                //CMessage.Write(strMessage1);

                            }

                            //if (chkDisplayPacket.Checked == true)
                            //{
                            //    strTmp = "";
                            //    for (i = 0; i < nLength; i++)
                            //        strTmp += "(0x" + CConvert.IntToHex(byteArrayData[i]) + ")";
                            //    strMessage0 = "-------------------------\r\n<=" + m_strType[nType];
                            //    strMessage0 += "(0x" + CConvert.IntToHex(byteData) + ")";
                            //    strMessage0 += "(0x" + CConvert.IntToHex(nCmd) + ")";
                            //    strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 24) & 0xff) + ")";
                            //    strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 16) & 0xff) + ")";
                            //    strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 8) & 0xff) + ")";
                            //    strMessage0 += "(0x" + CConvert.IntToHex(nLength & 0xff) + ")";
                            //    strMessage0 += strTmp + "\r\n";

                            //    CMessage.Write(strMessage0);
                            //}
                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }

            public void Receive_Emoticon()
            {
                int nType = _EMOTICON;

                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    byte byteCheckSum = 0;
                    //string strMessage0;
                    string strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;
                            // CheckSum Data Get
                            byteCheckSum = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;

                            string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                            int i;
                            string[] pstrData;

                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1) strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process Fail]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "\r\n";
                                CMessage.Write(strMessage1);
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                //if (chkDispHearBeat.Checked)
                                //{
                                //    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]\r\n";
                                //    CMessage.Write(strMessage1);
                                //}
                                //else continue;
                            }
                            else
                            {
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                switch (nCmd)
                                {
                                    case 0x01:
                                        {
                                            //listviewGenibo_Emoticon.Clear();
                                            //strMessage1 += "[Emoticon 목록확인]\r\n";
                                            //int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                            //int nIndex;
                                            //m_nMotionListCode_Emoticon = new int[nListCount];
                                            //m_nMotionListCount_Emoticon = nListCount;
                                            //for (i = 0; i < nListCount; i++)
                                            //{
                                            //    nIndex = byteArrayData[2 + i * 23] * 256 + byteArrayData[2 + i * 23 + 1];
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 23 + 2, 21);
                                            //    listviewGenibo_Emoticon.Items.Add(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                            //    m_nMotionListCode_Emoticon[i] = nIndex;
                                            //}
                                        }
                                        break;
                                    case 0x03:
#if false
                                    try
                                    {
                                        strMessage1 += "[Emoticon 업로드]\r\n";
                                        short sData;
                                        i = 0;
                                        int j;
                                        // Index
                                        //sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                        sData = (short)(byteArrayData[i++] * 256);
                                        sData += byteArrayData[i++];
                                        int nIndex = (int)sData;
                                        strMessage1 += "[Index=" + CConvert.IntToStr(nIndex) + "]";
                                        txtEmoticonIndex.Text = CConvert.IntToStr(nIndex);

                                        // Name
                                        txtEmoticonName.Text = System.Text.Encoding.Default.GetString(byteArrayData, i, 20);
                                        //txtTableName.Text = System.Text.Encoding.Default.GetString(byteArrayTitle, 0, 20);
                                        i += 21;

                                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

                                        //// Data
                                        for (j = 0; j < 8; j++)
                                        {
                                            // Left
                                            SetEmoticon(_L_, j, byteArrayData[i++]);
                                        }
                                        for (j = 0; j < 8; j++)
                                        {
                                            // Right
                                            SetEmoticon(_R_, j, byteArrayData[i++]);
                                        }
                                    }
                                    catch
                                    {
                                        CMessage.Write_Error("업로드 Error\n");
                                    }
#endif
                                        break;

                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\r\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";
                                            }
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            strMessage1 += "[Version Get]\r\n";

                                            strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            //lbVersion_Emoticon.Text = strTmp;
                                        }
                                        break;
                                    case 0xE2:
                                        strData = "    =>(Data[0]=0x" + CConvert.IntToHex(byteArrayData[0]) + ")\r\n" +
                                                  "    =>(Data[1]=0x" + CConvert.IntToHex(byteArrayData[1]) + ")\r\n" +
                                                  "    =>(Data[2]=0x" + CConvert.IntToHex(byteArrayData[2]) + ")\r\n" +
                                                  "    =>(Data[3]=0x" + CConvert.IntToHex(byteArrayData[3]) + ")\r\n";
                                        strMessage1 += "[Reserve]\r\n" + strData;
                                        break;
                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                CMessage.Write(strMessage1);

                            }

                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }
#endif
#endif

        }
    }
}
