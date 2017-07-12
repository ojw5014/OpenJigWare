using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.IO.Ports;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CMx28// : Form
        {
            #region Define structure(SParam_t. SParam_Axis_t)
            public struct SParam_t
            {
                public int nBaudRate;
                public int nComPort;
                public bool bCompleteStop;
                public int nMotorMax;
            }
            public struct SParam_Axis_t
            {
                public int nID;

                public int nDir;

                public float fLimitUp;    // limit Max value - 0: No use
                public float fLimitDn;    // limit Min value - 0: No use
                // Center position(Evd : Engineering value of degree)
                public float fCenterPos;

                public float fOffsetAngle_Display; // 보여지는 화면상의 각도 Offset

                // gear ratio
                public float fMechMove;
                public float fDegree;
            }
            #endregion Define Structure(SParam_t. SParam_Axis_t)

            #region Constructor/Destructor
            public CMx28() // Constructor - Init Function
            {
                Ojw.CTimer.Init(1000);
                m_SerialPort = new SerialPort();
                SetParam_Axis_Default();
                m_bClassEnd = false;
            }

            public void ReleaseClass()
            {
                m_bClassEnd = true;
                DisConnect();
                m_SerialPort.Dispose();
            }

            ~CMx28() // Destructor
            {
                ReleaseClass();
            }
            #endregion Constructor/Destructor

            private Thread Reader;             // Read thread

            #region define packet(Header and so on...)
            private const int _HEADER1 = 0;
            private const int _HEADER2 = 1;
            private const int _ID = 2;
            private const int _SIZE = 3;
            private const int _CMD = 4; // Instruction
            private const int _SIZE_PACKET_HEADER = 5;
            #endregion

            private SerialPort m_SerialPort = null;
            private bool m_bEventInit = false;

            #region setting Axis and ID
            private int m_nMotorID = 0;
            private int[] m_pnAxis_By_ID = new int[256]; // ID = 0 ~ 254, but, 254 is BroadCasting
            public void SetAxis_By_ID(int nID, int nAxis) { m_pnAxis_By_ID[nID] = nAxis; }
            public int GetAxis_By_ID(int nID) { return (nID == 0xfe) ? 0xfe : m_pnAxis_By_ID[nID]; }
            public int GetID_By_Axis(int nAxis) { return (nAxis == 0xfe) ? 0xfe : m_pSMot[nAxis].nID; }
            public void SetParam_Axis_Default()
            {
                float fMechMove = 4096.0f;// 1024.0f;
                float fDegree = 360.0f;
                int nCenterPos = 2047;// 512;                

                for (int nAxis = 0; nAxis < _MOTOR_MAX; nAxis++)
                {
                    SetParam_Axis(nAxis, nAxis, 0, 99999.0f, -99999.0f, nCenterPos, fMechMove, fDegree);
                }
            }
            public void SetParam_Axis(int nAxis, int nID, int nDir, float fLimitUp, float fLimitDn, float fCenterPos, float fMechMove, float fDegree)
            {
                m_pSMot[nAxis].nID = nID;
                m_pSMot[nAxis].nDir = nDir;
                m_pSMot[nAxis].fCenterPos = fCenterPos;
                m_pSMot[nAxis].fMechMove = fMechMove;
                m_pSMot[nAxis].fDegree = fDegree;

                m_pSMot[nAxis].fLimitUp = fLimitUp;
                m_pSMot[nAxis].fLimitDn = fLimitDn;

                SetAxis_By_ID(nID, nAxis);
            }
            public void SetParam_Axis(int nAxis, SParam_Axis_t SParam_Axis)
            {
                m_pSMot[nAxis].nID = SParam_Axis.nID;
                m_pSMot[nAxis].nDir = SParam_Axis.nDir;
                m_pSMot[nAxis].fCenterPos = SParam_Axis.fCenterPos;
                m_pSMot[nAxis].fMechMove = SParam_Axis.fMechMove;
                m_pSMot[nAxis].fDegree = SParam_Axis.fDegree;

                m_pSMot[nAxis].fLimitUp = SParam_Axis.fLimitUp;
                m_pSMot[nAxis].fLimitDn = SParam_Axis.fLimitDn;

                SetAxis_By_ID(SParam_Axis.nID, nAxis);
            }
            public SParam_Axis_t GetParam_Axis(int nAxis)
            {
                SParam_Axis_t SParam_Axis = new SParam_Axis_t();
                SParam_Axis.nID = m_pSMot[nAxis].nID;
                SParam_Axis.nDir = m_pSMot[nAxis].nDir;
                SParam_Axis.fCenterPos = m_pSMot[nAxis].fCenterPos;
                SParam_Axis.fMechMove = m_pSMot[nAxis].fMechMove;
                SParam_Axis.fDegree = m_pSMot[nAxis].fDegree;

                SParam_Axis.fLimitUp = m_pSMot[nAxis].fLimitUp;
                SParam_Axis.fLimitDn = m_pSMot[nAxis].fLimitDn;
                return SParam_Axis;
            }
            #endregion setting Axis and ID

            #region control variables and functions
            #region Variables
            private const int _MOTOR_MAX = 256;
            private int m_nMotor_Max = _MOTOR_MAX;
            public void SetMotor_Max(int nMotorMaxNum) { m_nMotor_Max = nMotorMaxNum; }
            public int GetMotor_Max() { return m_nMotor_Max; }
            private SMot_t[] m_pSMot = new SMot_t[_MOTOR_MAX];
            // counter variables for communication
            private long[] m_plSendCount = new long[_MOTOR_MAX];
            private long[] m_plReceiveCount = new long[_MOTOR_MAX];

            private int[,] m_pnStatus = new int[4, _MOTOR_MAX]; // Status1, Status2, Status_Drv_Srv, LED

            private int[] m_pnPos = new int[_MOTOR_MAX];
            private int[] m_pnSpeed = new int[_MOTOR_MAX];
            private float[] m_pfVolt = new float[_MOTOR_MAX];
            private int[] m_pnTemp = new int[_MOTOR_MAX];
            // 해당 축(Axis) 에 pbyteData 를 nStartAddress 번지부터 쓴다.
            #endregion Variables

            #region Pos Function
            private void SetPos(int nAxis, int nPos) { m_pnPos[nAxis] = nPos; }
            public int GetPos(int nAxis) { return m_pnPos[nAxis]; }
            public float GetPos_Angle(int nAxis) { return CalcEvd2Angle(nAxis, m_pnPos[nAxis]); }
            #endregion Pos Function

            #region Speed Function
            private void SetSpeed(int nAxis, int nSpeed) { m_pnSpeed[nAxis] = nSpeed; }
            public int GetSpeed(int nAxis) { return m_pnSpeed[nAxis]; }
            #endregion Speed Function

            #region Volt Function
            private void SetVolt(int nAxis, float fVolt) { m_pfVolt[nAxis] = fVolt; }
            public float GetVolt(int nAxis) { return m_pfVolt[nAxis]; }
            #endregion Volt Function

            #region Temp Function
            private void SetTemp(int nAxis, int nTemp) { m_pnTemp[nAxis] = nTemp; }
            public int GetTemp(int nAxis) { return m_pnTemp[nAxis]; }
            #endregion Temp Function

            #region Calc - math function(time, angle)(Kor: 계산함수(시간, 각도))
#if false
            // ms 의 값을 Raw 데이타(패킷용)으로 변환
            //public int CalcTime_ms(int nTime)
            //{
            //    // 1 Tick 당 11.2 ms => 1:11.2=x:nTime => x = nTime / 11.2
            //    return (int)Math.Round((float)nTime / 11.2f);
            //}
#endif
            /*
0~1023 (0X3FF) 까지 사용되며, 단위는 약 0.114rpm입니다.
0으로 설정하면 속도 제어를 하지 않고 모터의 최대 rpm을 사용한다는 의미입니다.
1023의 경우 약 117.07rpm이 됩니다.
예를 들어, 300으로 설정된 경우 약 34.33rpm입니다. -> 로보티즈 e-manual 발췌
             */ 
            // 117.185:1024 (maybe...). -> estimation
            private const float _MAX_RPM = 117.07f;//185f;
            private const float _MAX_EV_RPM = 1024.0f;
            public int CalcRpm2Raw(float fRpm) { return (int)Math.Round(fRpm * _MAX_EV_RPM / _MAX_RPM); }
            public float CalcRaw2Rpm(int nValue) { return (float)(_MAX_RPM * (float)nValue / _MAX_EV_RPM); }
            public float CalcTime2Speed(float fDeltaAngle, float fTime)
            {
                if (fDeltaAngle == 0) fDeltaAngle = (float)CMath._ZERO;
                // _MAX_RPM * 360 : 60 seconds => 1024(_MAX_EV_RPM) 
                // =>  rotate [6 * 117.185(_MAX_RPM)] degree during 1 second, 1ms => 117.185 * 360 degrees / 60000ms = 0.70311 degree
                // => it needs 60000 / (117.185(_MAX_RPM) * 360) = 1.422252564 ms for 1 degree moving
                
                // moving time for 1 degree => fTime / fDeltaAngle
                // moving time per moving degree => moving time during 1 degree * fDeltaAngle

                // rpm = 60000/([moving time for 1 degree]*360)
                #region Kor
                // _MAX_RPM * 360 : 60 seconds => 1024(_MAX_EV_RPM) 일때 
                // => 1초간 6 * 117.185(_MAX_RPM) 도 회전, 1ms => 117.185 * 360도 / 60000ms = 0.70311 도 이동
                // => 1도 움직이는데 60000 / (117.185(_MAX_RPM) * 360) = 1.422252564 ms 가 필요

                // 1도 이동시간 => 60000 / (Rpm * 360)
                // 이동각도 당 이동시간 계산 => 1도 이동시간 * fDeltaAngle

                // 1도 이동시간 => fTime / fDeltaAngle
                #endregion Kor
                return 60000 / (fTime / fDeltaAngle * 360.0f);
            }
            public int CalcTime(float fDeltaAngle, float fTime)
            {
                int nRet = CalcRpm2Raw(CalcTime2Speed(fDeltaAngle, fTime));
                return ((nRet > 1023) ? 1023 : ((nRet <= 0) ? 1 : nRet)); // clipping data ( 0 ~ 1023 )
            }
            public int CalcAngle2Evd(int nAxis, float fValue)
            {
                try
                {
                    fValue *= ((m_pSMot[nAxis].nDir == 0) ? 1.0f : -1.0f);
                    int nData = 0;
                    //if (GetCmd_Flag_Mode(nAxis) != 0)   // Speed Control
                    //{
                    //    nData = (int)Math.Round(fValue);
                    //}
                    //else
                    //{
                    // Position Control
                    nData = (int)Math.Round((m_pSMot[nAxis].fMechMove * fValue) / m_pSMot[nAxis].fDegree);
                    nData = nData + (int)m_pSMot[nAxis].fCenterPos;
                    //}

                    return nData;
                }
                catch
                {
                    return 0;
                }
            }
            public float CalcEvd2Angle(int nAxis, int nValue)
            {
                try
                {
                    float fValue = ((m_pSMot[nAxis].nDir == 0) ? 1.0f : -1.0f);
                    // 1024:333.3 = pulse:angle
                    float fValue2 = 0.0f;
                    //if (GetCmd_Flag_Mode(nAxis) != 0)   // Speed Control
                    //    fValue2 = (float)nValue * fValue;
                    //else                                // Position Control
                    fValue2 = (float)(((m_pSMot[nAxis].fDegree * ((float)(nValue - (int)m_pSMot[nAxis].fCenterPos))) / m_pSMot[nAxis].fMechMove) * fValue);
                    return fValue2;// At the end, be multiplied by the sign variable.(Kor: 마지막에 부호변수를 곱함)
                }
                catch
                {
                    return 0.0f;
                }
            }
            #endregion Calc - math function(time, angle)(Kor: 계산함수(시간, 각도))

            #region Limit
            private bool m_bIgnoredLimit = false;
            public void SetLimitEn(bool bOn) { m_bIgnoredLimit = !bOn; }
            public bool GetLimitEn() { return !m_bIgnoredLimit; }
            private int Clip(int nLimitValue_Up, int nLimitValue_Dn, int nData)
            {
                if (GetLimitEn() == false) return nData;

                int nRet = ((nData > nLimitValue_Up) ? nLimitValue_Up : nData);
                return ((nRet < nLimitValue_Dn) ? nLimitValue_Dn : nRet);
            }
            private float Clip(float fLimitValue_Up, float fLimitValue_Dn, float fData)
            {
                if (GetLimitEn() == false) return fData;
                float fRet = ((fData > fLimitValue_Up) ? fLimitValue_Up : fData);
                return ((fRet < fLimitValue_Dn) ? fLimitValue_Dn : fRet);
            }

            public int CalcLimit_Evd(int nAxis, int nValue)
            {
                //if ((GetCmd_Flag_Mode(nAxis) == 0) || (GetCmd_Flag_Mode(nAxis) == 2))
                //{
                int nPulse = nValue & 0x4000;
                nValue &= 0x3fff;
                int nUp = 100000;
                int nDn = -nUp;
                if (m_pSMot[nAxis].fLimitUp != 0) nUp = CalcAngle2Evd(nAxis, m_pSMot[nAxis].fLimitUp);
                if (m_pSMot[nAxis].fLimitDn != 0) nDn = CalcAngle2Evd(nAxis, m_pSMot[nAxis].fLimitDn);
                if (nUp < nDn) { int nTmp = nUp; nUp = nDn; nDn = nTmp; }
                return (Clip(nUp, nDn, nValue) | nPulse);
                //}
                //return nValue;
            }
            public float CalcLimit_Angle(int nAxis, float fValue)
            {
                //if ((GetCmd_Flag_Mode(nAxis) == 0) || (GetCmd_Flag_Mode(nAxis) == 2))
                //{
                float fUp = 100000.0f;
                float fDn = -fUp;
                if (m_pSMot[nAxis].fLimitUp != 0) fUp = m_pSMot[nAxis].fLimitUp;
                if (m_pSMot[nAxis].fLimitDn != 0) fDn = m_pSMot[nAxis].fLimitDn;
                return Clip(fUp, fDn, fValue);
                //}
                //return fValue;
            }
            #endregion

            #region Cmd Function
            #region [SMot_t]Define Motor Parameter(Kor: [SMot_t]관절 파라미터 구조체 선언)
            public struct SMot_t
            {
                public bool bEn;

                public int nDir;
                // Center Position
                public float fCenterPos;

                // Gear ratio(Kor: 기어비)
                public float fMechMove;
                public float fDegree;

                public float fLimitUp;    // Limit of axis(0 - no use)(Kor - 관절 리미트 - 0 무효)
                public float fLimitDn;    // Limit of axis(0 - no use)(Kor - 관절 리미트 - 0 무효)

                public int nID;
                public int nPos;
                public int nPos_Prev;
                public int nTime;

                public int nFlag; // 76[543210] NoAction(5), Red(4), Blue(3), Green(2), Mode(    
            }
            #endregion [SMot_t]Define Motor Parameter(Kor: [SMot_t]관절 파라미터 구조체 선언)
            public void InitCmd() { for (int i = 0; i < m_nMotor_Max; i++) { m_pSMot[i].bEn = false; /*SetCmd_Flag(i, false, false, false, false, false, true);*/ } }
            public void SetCmd(int nAxis, int nPos) { m_pSMot[nAxis].bEn = true; m_pSMot[nAxis].nPos_Prev = m_pSMot[nAxis].nPos; m_pSMot[nAxis].nPos = CalcLimit_Evd(nAxis, nPos); /*SetCmd_Flag_NoAction(nAxis, false);*/ }
            public void SetCmd_Angle(int nAxis, float fAngle) { m_pSMot[nAxis].bEn = true; m_pSMot[nAxis].nPos_Prev = m_pSMot[nAxis].nPos; m_pSMot[nAxis].nPos = CalcLimit_Evd(nAxis, CalcAngle2Evd(nAxis, fAngle)); /*SetCmd_Flag_NoAction(nAxis, false);*/ }
            public int GetCmd(int nAxis) { return m_pSMot[nAxis].nPos; }
            public float GetCmd_Angle(int nAxis) { return CalcEvd2Angle(nAxis, m_pSMot[nAxis].nPos); }
            //public void SetCmd_Flag(int nAxis, int nFlag) { m_pSMot[nAxis].nFlag = nFlag; }
            //public void SetCmd_Flag(int nAxis, bool bStop, bool bMode_Speed, bool bLed_Green, bool bLed_Blue, bool bLed_Red, bool bNoAction) { m_pSMot[nAxis].nFlag = ((bStop == true) ? _FLAG_STOP : 0) | ((bMode_Speed == true) ? _FLAG_MODE_SPEED : 0) | ((bLed_Green == true) ? _FLAG_LED_GREEN : 0) | ((bLed_Blue == true) ? _FLAG_LED_BLUE : 0) | ((bLed_Red == true) ? _FLAG_LED_RED : 0) | ((bNoAction == true) ? _FLAG_NO_ACTION : 0); }
            //public int GetCmd_Flag(int nAxis) { return m_pSMot[nAxis].nFlag; }
            //public void SetCmd_Flag_Stop(int nAxis, bool bStop) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xfe) | ((bStop == true) ? _FLAG_STOP : 0); }
            //public void SetCmd_Flag_Mode(int nAxis, bool bMode_Speed) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xfd) | ((bMode_Speed == true) ? _FLAG_MODE_SPEED : 0); }
            //public void SetCmd_Flag_Led(int nAxis, bool bGreen, bool bBlue, bool bRed) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xe3) | ((bGreen == true) ? _FLAG_LED_GREEN : 0) | ((bBlue == true) ? _FLAG_LED_BLUE : 0) | ((bRed == true) ? _FLAG_LED_RED : 0); }
            //public void SetCmd_Flag_Led_Green(int nAxis, bool bGreen) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xfb) | ((bGreen == true) ? _FLAG_LED_GREEN : 0); }
            //public void SetCmd_Flag_Led_Blue(int nAxis, bool bBlue) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xf7) | ((bBlue == true) ? _FLAG_LED_BLUE : 0); }
            //public void SetCmd_Flag_Led_Red(int nAxis, bool bRed) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xef) | ((bRed == true) ? _FLAG_LED_RED : 0); }
            //public void SetCmd_Flag_NoAction(int nAxis, bool bNoAction) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xdf) | ((bNoAction == true) ? _FLAG_NO_ACTION : 0); }

            //// 1111 1101
            //public int GetCmd_Flag_Mode(int nAxis) { return (((m_pSMot[nAxis].nFlag & _FLAG_MODE_SPEED) != 0) ? 1 : 0); }

            //public bool GetCmd_Flag_Led_Green(int nAxis) { return (((m_pSMot[nAxis].nFlag & 0x04) != 0) ? true : false); }
            //public bool GetCmd_Flag_Led_Blue(int nAxis) { return (((m_pSMot[nAxis].nFlag & 0x08) != 0) ? true : false); }
            //public bool GetCmd_Flag_Led_Red(int nAxis) { return (((m_pSMot[nAxis].nFlag & 0x10) != 0) ? true : false); }

            #endregion Cmd Function

            #region Status Function
            private void SetStatus1(int nAxis, int nStatus) { m_pnStatus[0, nAxis] = nStatus; }
            public int GetStatus1(int nAxis) { return m_pnStatus[0, nAxis]; }
            private void SetStatus2(int nAxis, int nStatus) { m_pnStatus[1, nAxis] = nStatus; }
            public int GetStatus2(int nAxis) { return m_pnStatus[1, nAxis]; }
            private void SetStatus_MotTorq(int nAxis, int nStatus) { m_pnStatus[2, nAxis] = nStatus; }

            // Error = false, Normal = true;
            public bool GetStatusMessage(int nAxis, out String strStatus1)//, out String strStatus2)
            {
                int nStatus1 = GetStatus1(nAxis);
                //int nStatus2 = GetStatus2(nAxis);
                strStatus1 = "";
                //strStatus2 = "";
                if ((nStatus1 & 0x01) != 0) strStatus1 += "[Input Voltage Error]";
                if ((nStatus1 & 0x02) != 0) strStatus1 += "[Angle Limit Error]";
                if ((nStatus1 & 0x04) != 0) strStatus1 += "[OverHeating Error]";
                if ((nStatus1 & 0x08) != 0) strStatus1 += "[Range Error]";
                if ((nStatus1 & 0x10) != 0) strStatus1 += "[CheckSum Error]";
                if ((nStatus1 & 0x20) != 0) strStatus1 += "[Overload Error]";
                if ((nStatus1 & 0x40) != 0) strStatus1 += "[Instrction Error]";
                //if ((nStatus1 & 0x80) != 0) strStatus1 += "[]";

                //if ((nStatus2 & 0x01) != 0) strStatus2 += "[]";
                //if ((nStatus2 & 0x02) != 0) strStatus2 += "[]";
                //if ((nStatus2 & 0x04) != 0) strStatus2 += "[]";
                //if ((nStatus2 & 0x08) != 0) strStatus2 += "[]";
                //if ((nStatus2 & 0x10) != 0) strStatus2 += "[]";
                //if ((nStatus2 & 0x20) != 0) strStatus2 += "[]";
                //if ((nStatus2 & 0x40) != 0) strStatus2 += "[]";
                //if ((nStatus2 & 0x80) != 0) strStatus2 += "[]";
                //return ((((nStatus1 & 0xff) + (nStatus2 & 0x3c)) != 0) ? false : true);
                return (((nStatus1 & 0xff) != 0) ? false : true);
            }
            #endregion Status Function

            #region checking status - Connection, Torq On/Off status...(Kor: 상태체크 모음 - 접속상태, Torq On/Off 상태 등...)
            public bool IsDrv(int nAxis) { return (((m_pnStatus[2, nAxis] & 0x40) != 0) ? true : false); }
            public bool IsSrv(int nAxis) { return (((m_pnStatus[2, nAxis] & 0x20) != 0) ? true : false); }
            public bool IsDrvAll(int nMax) // One of the motors will [false], it will be return [false].(Kor: 연결된 모터중 하나라도 false 면 false 를 리턴)
            {
                bool bRet = true;

                if (IsConnect() == true)
                {
                    for (int i = 0; i < nMax; i++)
                    {
                        if (IsDrv(i) == false)
                            bRet = false;
                    }
                }
                return bRet;
            }
            public bool IsSrvAll(int nMax) // One of the motors will [false], it will be return [false].(Kor: 연결된 모터중 하나라도 false 면 false 를 리턴)
            {
                bool bRet = true;
                if (IsConnect() == true)
                {
                    for (int i = 0; i < nMax; i++)
                    {
                        if (IsSrv(i) == false)
                            bRet = false;
                    }
                }
                return bRet;
            }
            public bool IsMotTorqAll(int nMax) // One of the motors will [false], it will be return [false].(Kor: 연결된 모터중 하나라도 false 면 false 를 리턴)
            {
                bool bRet = true;
                if (IsConnect() == true)
                {
                    for (int i = 0; i < nMax; i++)
                    {
                        if ((IsDrv(i) == false) || (IsSrv(i) == false))
                            bRet = false;
                    }
                }
                return bRet;
            }
            public bool IsMotTorq(int nAxis) // One of the motors will [false], it will be return [false].(Kor: 연결된 모터중 하나라도 false 면 false 를 리턴)
            {
                bool bRet = true;
                if (IsConnect() == true)
                {
                    if ((IsDrv(nAxis) == false) || (IsSrv(nAxis) == false))
                        bRet = false;
                }
                return bRet;
            }

            #region Counter - traffic Counter(Sending, Received event, Push Counter, Pop Counter)(Kor: 통신에 관한 카운터 ( 보낸것, 받은 이벤트, Push 횟수, Pop 횟수 ))
            private int m_nCntTick_Receive_Event_All = 0;
            private int m_nCntTick_Receive_PushBuffer = 0;
            private int m_nCntTick_Receive_GetBuffer = 0;
            public int GetCounter_Tick_Reveive_Event(int nAxis) { return m_nCntTick_Receive_Event_All; }
            public int GetCounter_Tick_Reveive_Push() { return m_nCntTick_Receive_PushBuffer; }
            public int GetCounter_Tick_Reveive_Interpret() { return m_nCntTick_Receive_GetBuffer; }

            private int[] m_pnCntTick_Send = new int[_MOTOR_MAX];
            private int[] m_pnCntTick_Receive = new int[_MOTOR_MAX];
            private int[] m_pnCntTick_Receive_Back = new int[_MOTOR_MAX];
            private long Tick_GetTimer(int nAxis) { return Ojw.CTimer.Get(nAxis); }
            private void Tick_Send(int nAxis) { if (IsConnect() == false) return; m_pnCntTick_Send[nAxis]++; Ojw.CTimer.Set(nAxis); m_pnCntTick_Receive_Back[nAxis] = m_pnCntTick_Receive[nAxis]; }
            private void Tick_Receive(int nAxis) { m_pnCntTick_Receive[nAxis]++; Ojw.CTimer.Set(nAxis); }
            public int GetCounter_Tick_Send(int nAxis) { return m_pnCntTick_Send[nAxis]; }
            public int GetCounter_Tick_Receive(int nAxis) { return m_pnCntTick_Receive[nAxis]; }
            public void ResetCounter()
            {
                m_nCntTick_Receive_Event_All = m_nCntTick_Receive_PushBuffer = m_nCntTick_Receive_GetBuffer = 0;
                Array.Clear(m_pnCntTick_Send, 0, m_pnCntTick_Send.Length);
                Array.Clear(m_pnCntTick_Receive, 0, m_pnCntTick_Receive.Length);
                for (int i = 0; i < 256; i++) m_pnCntTick_Receive_Back[i] = -1;//
            }
            // by the tick(Kor: 틱을 이용한 ...)
            // Check the time during traffic jam(Kor: 통신 장애 시간 체크)
            public long GetCounter_Timer(int nAxis) { return Tick_GetTimer(nAxis); }
            //public bool IsReceived(int nAxis) { return ((m_pnCntTick_Receive[nAxis] != m_pnCntTick_Receive_Back[nAxis]) || (Ojw.CTimer.Get(nAxis) > 100)) ? true : false; }
            public bool IsReceived(int nAxis) { return (m_pnCntTick_Receive[nAxis] != m_pnCntTick_Receive_Back[nAxis]) ? true : false; }
            private bool m_bError = false;
            public bool WaitTime(int nAxis, long lWaitTime)
            {
                int nErrorTime = 500;
                if (m_bBusy == true) WaitReceive(nAxis, nErrorTime);

                Ojw.CTimer CTmr = new CTimer();
                bool bError = true;
                //bool bOver = false;
                CTmr.Set();
                while ((IsConnect() == true) && (m_bClassEnd == false))// && (bOver == false))
                {
                    if (
                        (IsStop() == true) ||
                        (IsEms() == true)
                        )
                    {
                        bError = false;
                        break;
                    }
                    if (lWaitTime > 0)
                    {
                        if (CTmr.Get() >= lWaitTime) break;
                    }
                    ReadMotAndWait(nAxis);
                    Thread.Sleep(1);
                }
                if (m_bError == true) bError = true;

                return ((bError == false) ? true : false);
            }
            public bool WaitTime(long lWaitTime)
            {
                return WaitTime(m_nMotorID, lWaitTime);
            }
            public bool WaitReceive(int nAxis, long lWaitTimer)  // true : Receive Ok, false : Fail
            {
                Ojw.CTimer.Set(512 + 1);
                bool bError = true;
                bool bOver = false;
                while ((IsConnect() == true) && (m_bClassEnd == false) && (bOver == false))
                {                   
                    if (IsReceived(nAxis) == true)
                    {
                        bError = false;
                        break;
                    }
                    if (lWaitTimer > 0)
                    {
                        if (Ojw.CTimer.Get(512 + 1) >= lWaitTimer)
                        {
                            bOver = true;
                        }
                    }
                    Thread.Sleep(1);
                }
                
                if (m_bError == true) bError = true;

                return ((bError == false) ? true : false);
            }
            #endregion Counter - traffic Counter(Sending, Received event, Push Counter, Pop Counter)(Kor: 통신에 관한 카운터 ( 보낸것, 받은 이벤트, Push 횟수, Pop 횟수 ))

            #region buffer management - Variable - function(Kor: 버퍼관리 - 변수 - 함수)
            private const int _CNT_BUFFER = 100;
            private int m_nCnt_ReceivedData = 0;
            private String[] m_pstrReceivedData = new string[_CNT_BUFFER];// Complete packet data after receiving(Kor: 완전한 형태의 받은 패킷 데이터)
            private int m_nIndexReceiveData = 0; // refer to next address(Kor: 다음에 받아야 할 번지를 가리킴)
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
                    return ((IsConnect() == true) ? m_SerialPort.BytesToRead : 0);
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
                    return ((IsConnect() == true) ? m_SerialPort.ReadByte() : 0);
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

                m_nCntTick_Receive_PushBuffer++; // increase event counter(Kor: 통신 이벤트 카운터 증가)
            }
            public int GetBuffer_Size() { return m_nCnt_ReceivedData; }
            // Comes out with emptying the buffer(Kor: 버퍼를 비우면서 꺼내온다.)
            public String GetBuffer()
            {
                m_nCntTick_Receive_GetBuffer++; // increase event counter(Kor: 통신 이벤트 카운터 증가)

                if (m_nCnt_ReceivedData > 0)
                {
                    int nIndex = GetBuffer_Index_First();
                    m_nCnt_ReceivedData--;
                    return m_pstrReceivedData[nIndex];
                }

                return null;
            }
            // Comes out without emptying the buffer.(Kor: 버퍼를 비우지 않고 꺼내온다.)
            public String GetBuffer_NoRemove(int nPos)
            {
                if (m_nCnt_ReceivedData > 0)
                {
                    int nIndex = GetBuffer_Index(nPos);
                    return m_pstrReceivedData[nIndex];
                }
                return null;
            }
            #endregion buffer management - Variable - function(Kor: 버퍼관리 - 변수 - 함수)

            #region Packet command
            public void SendPacket(byte[] buffer, int nLength)
            {
                if (IsConnect() == true)
                {
                    m_SerialPort.Write(buffer, 0, nLength);
                }
            }           
            #endregion Packet command
            #endregion checking status - Connection, Torq On/Off status...(Kor: 상태체크 모음 - 접속상태, Torq On/Off 상태 등...)

            #endregion control variables and functions

            #region Event

            private byte[] m_pbyteReceiveData = new byte[1];
            
            public bool IsValid_Ram(int nAxis, int nAddress) { return ((m_pnRam == null) ? false : ((nAddress >= _SIZE_RAM) ? false : true)); }
            public int GetData_Ram(int nAxis, int nAddress) { return m_pnRam[nAxis, nAddress]; } // you can use [IsValid_Ram()] when you need using Exception
            public int[,] GetData_Ram() { return m_pnRam; }
            private const int _REG_TORQ = 24;
            private const int _REG_CMD_POS = 30;
            private const int _REG_GET = 36;
            private const int _CMD_READ = 0x02;
            private const int _CMD_WRITE = 0x03;
            private const int _CMD_RESET = 0x06;
            private int m_nReg_Get_Address = _REG_GET;
            private int m_nReg_Get_Length = 8;
            private int m_nReg_Get_Address_Reserve = _REG_GET;
            private int m_nReg_Get_Length_Reserve = 8;
            private void PacketDecoder(int nAxis)
            {
                ///////////// parsing ////////////////
                int i = 0;
                byte[] byteData = new byte[2];
                #region Position
                byteData[0] = (byte)(GetData_Ram(nAxis, m_nReg_Get_Address + i++) & 0xff);
                byteData[1] = (byte)(GetData_Ram(nAxis, m_nReg_Get_Address + i++) & 0xff);
                // 0000 0000  0000 0000
                short sData = 0;
                sData = BitConverter.ToInt16(byteData, 0);
                SetPos(nAxis, (int)sData);
                #endregion Position

                #region Speed
                byteData[0] = (byte)(GetData_Ram(nAxis, m_nReg_Get_Address + i++) & 0xff);
                byteData[1] = (byte)(GetData_Ram(nAxis, m_nReg_Get_Address + i++) & 0xff);
                sData = 0;
                sData = BitConverter.ToInt16(byteData, 0);
                SetSpeed(nAxis, (int)sData);
                #endregion Speed

                #region Load - reserve
                i += 2;
                #endregion Load - reserve

                #region Volt
                float fVolt = (float)(GetData_Ram(nAxis, m_nReg_Get_Address + i++) & 0xff) / 10.0f;
                SetVolt(nAxis, fVolt);
                #endregion Volt

                #region Temp
                SetTemp(nAxis, (byte)(GetData_Ram(nAxis, m_nReg_Get_Address + i++) & 0xff));
                #endregion Volt

                String strError;
                bool bStatusOk = GetStatusMessage(nAxis, out strError);
                if (bStatusOk == false)
                {
                    Ojw.CMessage.Write_Error("Received ErrorCode -> " + strError);
                }
                byteData = null;
                ///////////////////////////////////
            }
            private byte m_byCheckSum = 0;
            private int m_nRxIndex = 0xff;
            private int m_nRxDataCount = 0;
            private int m_nRxId = 0;
            private int m_nRxAddress = 0;
            private int m_nRxAddressLen = 0;
            private int m_nRxCheckSum = 0;
            private const int _CMD_ROM = 0x42;
            private const int _CMD_RAM = 0x44;

            public const int _SIZE_RAM = 74;
            private int[,] m_pnRam = new int[256, _SIZE_RAM];
            private void ReceiveDataCallback()		/* Callback function */
            {
                while ((IsConnect() == true) && (m_bClassEnd == false))
                {
                    try
                    {
                        int i;
                        byte RxData;
                        //int nPacketSize = 0;

                        int nLength = GetBuffer_Length();
                        if (nLength > 0)
                        {
                            m_nCntTick_Receive_Event_All++;// Notice recive-event

                            // busy clear for next communications
                            m_bBusy = false;
                            for (i = 0; i < nLength; i++)
                            {
                                RxData = (byte)(m_SerialPort.ReadByte() & 0xff); // Get One byte from serial buffer

                                // Top Header Check
                                if ((RxData == 0xFF) && (m_nRxIndex == 0xFF))
                                {
                                    m_nRxIndex = 0;
                                    m_nRxDataCount = 0;
                                    continue;
                                }

                                switch (m_nRxIndex)
                                {
                                    case 0: // Header 2
                                        if (RxData != 0xFF)
                                        {
                                            m_nRxIndex = 0xFF;
                                            m_nRxDataCount = 0;
                                            continue;
                                        }
                                        break;
                                    case 1: //Get ID		
                                        m_nRxId = RxData;
                                        m_byCheckSum = RxData;
                                        break;
                                    case 2: //Get Data Size(Packet Size)
                                        m_nRxAddressLen = RxData;
                                        m_byCheckSum += RxData;
                                        break;
                                    case 3: // Error
                                        {
                                            SetStatus1(_HEADER1, (int)RxData);
                                            m_byCheckSum += RxData;
                                        }
                                        break;
                                    #region Param
                                    case 4: //Get Data				
                                        {
                                            if (m_nRxDataCount < (m_nRxAddressLen - 2))
                                            {
                                                int nAxis = GetAxis_By_ID(m_nRxId); // Get the Nickname from Actual Motor ID
                                                
                                                m_nReg_Get_Address = m_nReg_Get_Address_Reserve;
                                                m_nRxAddress = m_nReg_Get_Address;// _REG_CMD_POS + 6;// 36; // 

                                                // Set the memory map by packet data
                                                m_pnRam[nAxis, (int)((m_nRxAddress + m_nRxDataCount) & 0xff)] = RxData;
                                                //else { m_nRxIndex = 0xFF; continue; }

                                                m_nRxDataCount++; // Increase Address Counter
                                                m_byCheckSum += RxData;

                                                //if ((m_nRxDataCount < (m_nRxAddressLen - 2)) && (m_nRxDataCount < m_pnRom.Length))
                                                if (m_nRxDataCount < (m_nRxAddressLen - 2))
                                                {
                                                    m_nRxIndex--;		//Fix Get Data Routine
                                                }
                                            }
                                            if (m_nRxDataCount == 0) // CheckSum
                                            {
                                                m_nRxCheckSum = (int)RxData;
                                                m_byCheckSum = (byte)(~m_byCheckSum & 0xff);

                                                m_nRxIndex = 0xFF;

                                                if (m_byCheckSum == m_nRxCheckSum)
                                                {
                                                    PacketDecoder(GetAxis_By_ID(m_nRxId));

                                                    m_bError = false;
                                                    // Tick Set after receiveing and processing
                                                    Tick_Receive(m_nRxId);
                                                }
                                                else
                                                {
                                                    Ojw.CMessage.Write_Error("<=CheckSum Error : Received(0x" + Ojw.CConvert.IntToHex(m_nRxCheckSum, 2) + ") != Calc(" + Ojw.CConvert.IntToHex(m_byCheckSum, 2) + ")");

                                                    m_bError = true;
                                                    // Tick Set after receiveing and processing
                                                    Tick_Receive(m_nRxId);
                                                    continue;
                                                }
                                            }
                                        }
                                        break;
                                    case 5: // CheckSum
                                        {
                                            m_nRxCheckSum = (int)RxData;
                                            m_byCheckSum = (byte)(~m_byCheckSum & 0xff);

                                            m_nRxIndex = 0xFF;

                                            if (m_byCheckSum == m_nRxCheckSum)
                                            {
                                                PacketDecoder(GetAxis_By_ID(m_nRxId));

                                                m_bError = false;
                                                // Tick Set after receiveing and processing
                                                Tick_Receive(m_nRxId);
                                            }
                                            else
                                            {
                                                Ojw.CMessage.Write_Error("<=CheckSum Error : Received(0x" + Ojw.CConvert.IntToHex(m_nRxCheckSum, 2) + ") != Calc(" + Ojw.CConvert.IntToHex(m_byCheckSum, 2) + ")");

                                                m_bError = true;
                                                // Tick Set after receiveing and processing
                                                Tick_Receive(m_nRxId);
                                                continue;
                                            }
                                        }
                                        break;
                                    #endregion Param
                                    default:
                                        m_nRxIndex = 0xFF;
                                        break;
                                }
                                if (m_nRxIndex != 0xFF) m_nRxIndex++;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        m_nRxIndex = 0xFF;
                        Ojw.CMessage.Write_Error(e.ToString());
                    }
                    Thread.Sleep(10);
                }
            }

            private void serialPort1_DataReceived()
            {
                ReceiveDataCallback();
            }
            #endregion Event

            #region IsConnect() - Checking connection
            public bool IsConnect() { return m_SerialPort.IsOpen; }
            #endregion IsConnect() - Checking connection

            #region Connect
            /////////////////////////////////////////////////
            // Parity - 0 : None, 1 : Odd, 2 : Even, 3 : Mark, 4 : Space
            // StopBit - 0 : None, 1 : One, 2 : Two, 3 : OnePointFive
            public bool Connect(int nPort, int nBaudRate)//(int nPort, int nBaudRate, int nParity, int nDataBits, int nStopBits)
            {
                if (IsConnect() == false)
                {
                    ResetCounter(); // Counter Clear

                    if (m_bEventInit == false)
                    {
                        m_bEventInit = true;
                    }

                    m_SerialPort.PortName = "COM" + nPort.ToString();
                    m_SerialPort.BaudRate = nBaudRate;
                    m_SerialPort.Parity = Parity.None;
                    m_SerialPort.DataBits = 8;
                    m_SerialPort.StopBits = StopBits.One;
                    m_SerialPort.ReceivedBytesThreshold = 1;
                    try
                    {
                        
                        m_SerialPort.Open();
                        if (IsConnect() == true)
                        {
                            Reader = new Thread(new ThreadStart(serialPort1_DataReceived));
                            Reader.Start();
                        }

                    }
                    catch
                    {
                    }
                }
                return IsConnect();
            }
            #endregion

            #region DisConnect() - Thread Stop
            public void DisConnect()
            {
                if (IsConnect() == true)
                {
                    m_SerialPort.Close();
                }
            }
            #endregion DisConnect() - Thread Stop

            private bool m_bBusy = false;
            // checking For the traffic Jam
            public bool IsBusy() { return m_bBusy; }
            private bool[] m_pbRequest = new bool[_MOTOR_MAX];
            // Get some datas(Size->nDatabyteSize) from nCmd
            // ex) Get some datas(Size->8) from Address(40) => ReadMot(nMotorAxis, 40, 8)
            public void ReadData_SetAddress(int nAddress) { m_nReg_Get_Address_Reserve = nAddress; }
            public void ReadData_SetLength(int nLength) { m_nReg_Get_Length_Reserve = nLength; }
            public bool ReadMotAndWait(int nAxis)
            {
                int nErrorTime = 500;
                ReadMot(nAxis);
                return WaitReceive(nAxis, nErrorTime);
            }
            public void ReadMot(int nAxis)
            {
                if (IsConnect() == false) return;
                m_nMotorID = nAxis;
                m_nReg_Get_Address_Reserve = _REG_GET;
                m_nReg_Get_Length = m_nReg_Get_Length_Reserve;
                m_bBusy = true; // request data
#if false
                int nID = GetID_By_Axis(nAxis);//m_pSMot[nAxis].nID;
                int nDefaultSize = _CHECKSUM2 + 1;
                //int nSize = 255;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x04; // Ram 영역을 읽어온다.

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize + i++] = (byte)(nCmd & 0xff);// 46번 레지스터 명령

                ////////
                pbyteBuffer[nDefaultSize + i++] = (byte)(nDataByteSize & 0xff);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                Tick_Send(nAxis);
                SendPacket(pbyteBuffer, nDefaultSize + i);
#else
                int nCheckSum = 0;

                int nID = GetID_By_Axis(nAxis);// 0;
                int nLength = 2;
                byte[] aBuffer = new byte[nLength + 6];
                int i = 0;
                aBuffer[i++] = 0xff;
                aBuffer[i++] = 0xff;

                aBuffer[i++] = (byte)(nID & 0xff);// (m_C3d.m_CHeader.pSMotorInfo[nID].nMotorID & 0xff); // ID
                aBuffer[i++] = (byte)((nLength + 2) & 0xff);
                aBuffer[i++] = (byte)(_CMD_READ & 0xff);

                aBuffer[i++] = (byte)(m_nReg_Get_Address_Reserve & 0xff); // address(36:_REG_GET)

                aBuffer[i++] = (byte)(m_nReg_Get_Length & 0xff); // read 8 datas

                nCheckSum = aBuffer[2]; // ID
                for (int j = 3; j < i; j++)
                {
                    nCheckSum += aBuffer[j];
                }
                aBuffer[i++] = (byte)(~nCheckSum & 0xff);
                // Set a tick counter(Kor: 보내기 전에 Tick 을 Set 한다.)
                Tick_Send(nAxis);
                SendPacket(aBuffer, i);
#endif
            }

            #region Control variable(Stop, Ems, Reset)
            private bool m_bEms = false;
            private bool m_bStop = false;
            public bool IsEms() { return m_bEms; }
            public bool IsStop() { return m_bStop; }
            #endregion Control variable(Stop, Ems, Reset)

            #region Stop
            // Stop all - if you use it, you need to reset before any controling(resetstop() can do also)(Kor: 전체 정지 - 이걸로 멈추면 반드시 리셋이 필요(Stop 변수만 리셋하면 됨))
            public void Stop()
            {
                //for (int i = 0; i < m_nMotor_Max; i++)
                //    SetCmd_Flag_Stop(i, true);
                //SetMot(0xfe, 1000);
                MotTorq(false);
                //WaitReceive(
                MotTorq(true);
                m_bStop = true;
            }

            // only stop without setting up m_bStop(Kor: 이건 그냥 멈추기만 할 뿐 변수를 셋하지는 않는다.)
            public void Stop(int nAxis)
            {
                //m_nMotorID = nAxis;
                //SetCmd_Flag_Stop(nAxis, true);
                //SetMot(nAxis, 1000);
            }
            #endregion Stop

            #region Ems - emergency switch(Kor: 비상정지)
            public void Ems()
            {
                Stop();
                MotTorq(false);
                m_bEms = true;
            }
            #endregion Ems - emergency switch(Kor: 비상정지)

            #region Reset
            public void Rollback(int nAxis)
            {
                
            }
            
            public void ResetStop() { m_bStop = false; /*for (int i = 0; i < m_nMotor_Max; i++) SetCmd_Flag_Stop(i, false);*/ }
            public void ResetEms() { m_bEms = false; /*for (int i = 0; i < m_nMotor_Max; i++) SetCmd_Flag_Stop(i, false);*/ }

            public void Reset()
            {
                ResetStop();
                ResetEms();
                //Reset(0xfe);
            }
            #endregion

            #region Torq
            // Torq
            private bool[] m_abTorq = new bool[_MOTOR_MAX];
            public void MotTorq(bool bEn)
            {
                //if ((m_bEms == true) && (bDriver == false) && (bServo == false)) return;
                MotTorq(0xfe, bEn); // Broad Casting
                for (int nAxis = 0; nAxis < m_abTorq.Length; nAxis++) m_abTorq[nAxis] = bEn;
            }
            public void MotTorq(int nAxis, bool bEn)
            {
                if (((m_bStop == true) || (m_bEms == true)) && bEn == true) return; // Emergency Block
                m_bBusy = true; // request answer
                m_abTorq[nAxis] = bEn;
                int nCheckSum = 0;

                int nID = GetID_By_Axis(nAxis);// 0;
                int nLength = 2;
                byte[] aBuffer = new byte[nLength + 6];
                int i = 0;
                aBuffer[i++] = 0xff;
                aBuffer[i++] = 0xff;

                aBuffer[i++] = (byte)(nID & 0xff);//(m_C3d.m_CHeader.pSMotorInfo[nID].nMotorID & 0xff); // ID
                aBuffer[i++] = (byte)((nLength + 2) & 0xff);
                aBuffer[i++] = (byte)(_CMD_WRITE & 0xff);
                aBuffer[i++] = (byte)(_REG_TORQ & 0xff);
                aBuffer[i++] = (byte)((bEn == true) ? 1 : 0);

                nCheckSum = aBuffer[2]; // ID
                for (int j = 3; j < i; j++)
                {
                    nCheckSum += aBuffer[j];
                }
                aBuffer[i++] = (byte)(~nCheckSum & 0xff);
                
                // Set a Tick Count
                Tick_Send(nAxis);
                SendPacket(aBuffer, i);
            }
            #endregion
            public void SetMot_Rpm(int nAxis, float fAngle, float fRpm)
            {
                if ((m_bStop == true) || (m_bEms == true)) return;
                //////////////////////////////////////////////////
                m_bBusy = true; // request answer
                m_nMotorID = nAxis;

                SetCmd_Angle(nAxis, fAngle); // Angle to [Engineering value of degree]
                int nCheckSum = 0;

                int nID = GetID_By_Axis(nAxis); // Get the real Motor ID from Alias Name
                int nLength = 5;
                byte[] aBuffer = new byte[nLength + 6];
                int i = 0;
                aBuffer[i++] = 0xff;
                aBuffer[i++] = 0xff;

                aBuffer[i++] = (byte)(nID & 0xff);//(m_C3d.m_CHeader.pSMotorInfo[nID].nMotorID & 0xff); // ID
                aBuffer[i++] = (byte)((nLength + 2) & 0xff); // Length
                aBuffer[i++] = (byte)(_CMD_WRITE & 0xff); // Instruction
                aBuffer[i++] = (byte)(_REG_CMD_POS & 0xff);
                short sData = (short)GetCmd(nAxis); //(short)CalcAngle2Evd(nAxis, fAngle);
                byte[] abyTmp = BitConverter.GetBytes(sData);
                aBuffer[i++] = abyTmp[0];
                aBuffer[i++] = abyTmp[1];

                #region Time - Set the time value to motor speed
                //sData = (short)(CalcTime(((float)Math.Abs(GetPos_Angle(nAxis) - fAngle) % 360.0f), (float)nTime));
                sData = (short)CalcRpm2Raw(fRpm);
                if (sData > _MAX_EV_RPM - 1) sData = (short)(_MAX_EV_RPM - 1);
                if (sData <= 1) sData = 1;
                abyTmp = BitConverter.GetBytes(sData);
                aBuffer[i++] = abyTmp[0];
                aBuffer[i++] = abyTmp[1];
                #endregion Time - Set the time value to motor speed

                // Checksum ==== ////////////////////////
                nCheckSum = aBuffer[2]; // ID
                for (int j = 3; j < i; j++) { nCheckSum += aBuffer[j]; }
                aBuffer[i++] = (byte)(~nCheckSum & 0xff);
                // ==== Checksum ////////////////////////

                // Set a Tick Count
                Tick_Send(nAxis);
                SendPacket(aBuffer, i);

                //m_CTimer_Drawing.Set();
                //m_fTime = fTime;
                //m_fAngle = m_fData;

                //m_fData = fAngle;
            }     
            public void SetMot(int nAxis, float fAngle, int nTime)
            {
                if ((m_bStop == true) || (m_bEms == true)) return;
                //////////////////////////////////////////////////
                m_bBusy = true; // request answer
                m_nMotorID = nAxis;

                SetCmd_Angle(nAxis, fAngle); // Angle to [Engineering value of degree]
                int nCheckSum = 0;

                int nID = GetID_By_Axis(nAxis); // Get the real Motor ID from Alias Name
                int nLength = 5;
                byte[] aBuffer = new byte[nLength + 6];
                int i = 0;
                aBuffer[i++] = 0xff;
                aBuffer[i++] = 0xff;

                aBuffer[i++] = (byte)(nID & 0xff);//(m_C3d.m_CHeader.pSMotorInfo[nID].nMotorID & 0xff); // ID
                aBuffer[i++] = (byte)((nLength + 2) & 0xff); // Length
                aBuffer[i++] = (byte)(_CMD_WRITE & 0xff); // Instruction
                aBuffer[i++] = (byte)(_REG_CMD_POS & 0xff);
                short sData = (short)GetCmd(nAxis); //(short)CalcAngle2Evd(nAxis, fAngle);
                byte[] abyTmp = BitConverter.GetBytes(sData);
                aBuffer[i++] = abyTmp[0];
                aBuffer[i++] = abyTmp[1];

                #region Time - Set the time value to motor speed
                //sData = (short)(CalcTime(((float)Math.Abs(GetPos_Angle(nAxis) - fAngle) % 360.0f), (float)nTime));
                sData = (short)(CalcTime(((float)Math.Abs(CalcEvd2Angle(nAxis, m_pSMot[nAxis].nPos_Prev) - fAngle) % 360.0f), (float)nTime));
                if (sData > _MAX_EV_RPM - 1) sData = (short)(_MAX_EV_RPM - 1);
                if (sData <= 1) sData = 1;
                abyTmp = BitConverter.GetBytes(sData);
                aBuffer[i++] = abyTmp[0];
                aBuffer[i++] = abyTmp[1];
                #endregion Time - Set the time value to motor speed

                // Checksum ==== ////////////////////////
                nCheckSum = aBuffer[2]; // ID
                for (int j = 3; j < i; j++) { nCheckSum += aBuffer[j]; }
                aBuffer[i++] = (byte)(~nCheckSum & 0xff);
                // ==== Checksum ////////////////////////
                
                // Set a Tick Count
                Tick_Send(nAxis);
                SendPacket(aBuffer, i);

                //m_CTimer_Drawing.Set();
                //m_fTime = fTime;
                //m_fAngle = m_fData;

                //m_fData = fAngle;
            }            
        }
    }
}
