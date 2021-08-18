#define _ENABLE_THREAD
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Windows.Forms;
//using System.IO;

namespace OpenJigWare
{
    partial class Ojw
    {
        public enum EType_t
        {
            _0101,
            _0102,
            _0201,
            _0202,
            _0401,
            _0402,
            _0601,
            _0602,
            //_0603,
            _Count
        }
        
        public struct SAxis_t
        {
            public int nAxis;
            public float fAngle;
            public SAxis_t(int nAxisValue, float nAngleValue) { this.nAxis = nAxisValue; this.fAngle = nAngleValue; }
        }

        // if you make your class, just write in here
        public class CHerculex
        {
            public static int _DRS_0101 = 0;
            public static int _DRS_0102 = 1;
            public static int _DRS_0201 = 2;
            public static int _DRS_0202 = 3;
            public static int _DRS_0401 = 4;
            public static int _DRS_0402 = 5;
            public static int _DRS_0601 = 6;
            public static int _DRS_0602 = 7;

            // if you make your class, just write in here
            private const int _MOTOR_MAX = 256;
            //private bool m_bEventInit = false;

            //private float m_Param_fMechMove = 1024.0f;
            //private float m_Param_fDegree = 333.333f;
            //private float m_Param_fCenterPos = 512.0f;
            private SMot_t[] m_pSMot = new SMot_t[_MOTOR_MAX];
            private const int _CNT_TIMER = 999;
            private Ojw.CTimer[] m_aCTmr = new CTimer[_CNT_TIMER];
            public CHerculex() // 생성자 초기화 함수
            {
                for (int i = 0; i < _CNT_TIMER; i++) m_aCTmr[i] = new CTimer();
                //CTimer.Init(999);
                //m_SerialPort = new SerialPort();
                SetModel(EType_t._0101);

                //m_bClassEnd = false;
            }
            ~CHerculex()
            {
                //CTimer.DInit();
                //m_bClassEnd = true;
                if (IsConnect()) DisConnect();
                //m_SerialPort.Dispose();
            }
            EType_t [] m_aEType = new EType_t[_MOTOR_MAX];
            private COjwMotor m_CMotor = new COjwMotor();
            public void SetModel(EType_t etype)//bool bEncodertype)
            {
                
                float fMechMove = 1024.0f;
                float fDegree = 333.333f;
                float fCenterPos = 512.0f;
                if (etype == EType_t._0102)
                {
                    fMechMove = 6391.605f;
                    fDegree = 360.0f;
                    fCenterPos = 2964.0f;
                }
                else if (etype == EType_t._0202)
                {
                    fMechMove = 6391.605f;
                    fDegree = 360.0f;
                    fCenterPos = 2964.0f;
                }
                else if (etype == EType_t._0401)
                {
                    fMechMove = 2048.0f;
                    fDegree = 333.3f;
                    fCenterPos = 1024.0f;
                }                
                else if (etype == EType_t._0402)
                {
                    fMechMove = 12962.099f;
                    fDegree = 360.0f;
                    fCenterPos = 16384.0f;
                }
                else if (etype == EType_t._0601)
                {
                    fMechMove = 2048.0f;
                    fDegree = 333.3f;
                    fCenterPos = 1024.0f;
                }                
                else if (etype == EType_t._0602)
                {
                    fMechMove = 12962.099f;
                    fDegree = 360.0f;
                    fCenterPos = 16384.0f;
                }

                for (int nAxis = 0; nAxis < _MOTOR_MAX; nAxis++)
                {
                    m_aEType[nAxis] = etype;
                    m_CMotor.SetParam_Axis(nAxis, nAxis, 0, 99999.0f, -99999.0f, fCenterPos, fMechMove, fDegree);
                    m_CMotor.SetSpeedType(nAxis, false);
                }
                m_CMotor.InitCmd();
            }
            public void SetParam_Axis(int nAxis, int nID, int nDir, float fLimitUp, float fLimitDn, int nCenterPos, float fMechMove, float fDegree)
            {
                m_CMotor.SetParam_Axis(nAxis, nID, nDir, fLimitUp, fLimitDn, nCenterPos, fMechMove, fDegree);
            }
            public bool SetSpeedType(int nAxis, bool bSpeedType)
            {
                return m_CMotor.SetSpeedType(nAxis, bSpeedType);
            }
        
            public EType_t [] GetModel()
            {
                return m_aEType;
            }
            public EType_t GetModel(int nAxis)
            {
                return m_aEType[nAxis];
            }
            public void SetModel(int nData)//bool bEncodertype)
            {
                float fMechMove = 1024.0f;
                float fDegree = 333.333f;
                float fCenterPos = 512.0f;
                if (nData == (int)EType_t._0102)
                {
                    fMechMove = 6391.605f;
                    fDegree = 360.0f;
                    fCenterPos = 2964.0f;
                }

                for (int nAxis = 0; nAxis < _MOTOR_MAX; nAxis++)
                {
                    m_aEType[nAxis] = (EType_t)nData;
                    m_CMotor.SetParam_Axis(nAxis, nAxis, 0, 99999.0f, -99999.0f, fCenterPos, fMechMove, fDegree);
                    m_CMotor.SetSpeedType(nAxis, false);
                }
                m_CMotor.InitCmd();
            }
            public void SetModel(int nAxis, EType_t etype)//bool bEncodertype)
            {
                float fMechMove = 1024.0f;
                float fDegree = 333.333f;
                float fCenterPos = 512.0f;
                if (etype == EType_t._0102)
                {
                    fMechMove = 6391.605f;
                    fDegree = 360.0f;
                    fCenterPos = 2964.0f;
                }
                m_aEType[nAxis] = etype;
                m_CMotor.SetParam_Axis(nAxis, nAxis, 0, 99999.0f, -99999.0f, fCenterPos, fMechMove, fDegree);
                m_CMotor.SetSpeedType(nAxis, false);
                m_CMotor.InitCmd();
            }
            public void SetModel(int nAxis, int nData)//bool bEncodertype)
            {
                float fMechMove = 1024.0f;
                float fDegree = 333.333f;
                float fCenterPos = 512.0f;
                if (nData == (int)EType_t._0102)
                {
                    fMechMove = 6391.605f;
                    fDegree = 360.0f;
                    fCenterPos = 2964.0f;
                }

                m_aEType[nAxis] = (EType_t)nData;
                m_CMotor.SetParam_Axis(nAxis, nAxis, 0, 99999.0f, -99999.0f, fCenterPos, fMechMove, fDegree);
                m_CMotor.SetSpeedType(nAxis, false);
                m_CMotor.InitCmd();
            }
            public void SetModel(int nAxis, EType_t etype, bool bSpeedType)//bool bEncodertype)
            {
                float fMechMove = 1024.0f;
                float fDegree = 333.333f;
                float fCenterPos = 512.0f;
                if (etype == EType_t._0102)
                {
                    fMechMove = 6391.605f;
                    fDegree = 360.0f;
                    fCenterPos = 2964.0f;
                }
                m_aEType[nAxis] = etype;
                m_CMotor.SetParam_Axis(nAxis, nAxis, 0, 99999.0f, -99999.0f, fCenterPos, fMechMove, fDegree);
                m_CMotor.SetSpeedType(nAxis, bSpeedType);
                m_CMotor.InitCmd();
            }
            public void SetModel(int nAxis, int nData, bool bSpeedType)//bool bEncodertype)
            {
                float fMechMove = 1024.0f;
                float fDegree = 333.333f;
                float fCenterPos = 512.0f;
                if (nData == (int)EType_t._0102)
                {
                    fMechMove = 6391.605f;
                    fDegree = 360.0f;
                    fCenterPos = 2964.0f;
                }

                m_aEType[nAxis] = (EType_t)nData;
                m_CMotor.SetParam_Axis(nAxis, nAxis, 0, 99999.0f, -99999.0f, fCenterPos, fMechMove, fDegree);
                m_CMotor.SetSpeedType(nAxis, bSpeedType);
                m_CMotor.InitCmd();
            }
            public void SetParam_Dir(int nAxis, bool bBackward) { m_CMotor.SetParam_Item_Dir(nAxis, ((bBackward) ? 1 : 0)); }
            // true : backward
            public bool GetParam_Dir(int nAxis) { return ((m_CMotor.GetParam_Item_Dir(nAxis) != 0) ? true : false); }
            public void SetParam_ID(int nAxis, int nID) { m_CMotor.SetParam_Item_ID(nAxis, nID); }
            public int GetParam_ID(int nAxis, int nID) { return m_CMotor.GetParam_Item_ID(nAxis); }
            public void SetParam_CenterPos(int nAxis, float fCenterPos) { m_CMotor.SetParam_Item_fCenterPos(nAxis, fCenterPos); }
            public float GetParam_CenterPos(int nAxis) { return m_CMotor.GetParam_Item_fCenterPos(nAxis); }
            public void SetParam_fMechMove(int nAxis, float fMechMove) { m_CMotor.SetParam_Item_fMechMove(nAxis, fMechMove); }
            public float GetParam_fMechMove(int nAxis) { return m_CMotor.GetParam_Item_fMechMove(nAxis); }
            public void SetParam_fDegree(int nAxis, float fDegree) { m_CMotor.SetParam_Item_fDegree(nAxis, fDegree); }
            public float GetParam_fDegree(int nAxis) { return m_CMotor.GetParam_Item_fDegree(nAxis); }
            public void SetParam_fLimitUp(int nAxis, float fLimitUp) { m_CMotor.SetParam_Item_fLimitUp(nAxis, fLimitUp); }
            public float GetParam_fLimitUp(int nAxis) { return m_CMotor.GetParam_Item_fLimitUp(nAxis); }
            public void SetParam_fLimitDn(int nAxis, float fLimitDn) { m_CMotor.SetParam_Item_fLimitDn(nAxis, fLimitDn); }
            public float GetParam_fLimitDn(int nAxis) { return m_CMotor.GetParam_Item_fLimitDn(nAxis); }
            
            #region Connect / Disconnect
            public bool Connect(int nComport, int nBaudRate)
            {
                bool bConnect = false;
                if (m_CMotor.IsConnect() == false)
                {
                    bConnect = m_CMotor.Connect(nComport, nBaudRate);
                    //if (bConnect == false) m_CMotor.SetAutoReturn(false);
                }
                return bConnect;
            }
            public void DisConnect() { if (m_CMotor.IsConnect() == true) m_CMotor.DisConnect(); }
            #endregion Connect / Disconnect
            public bool IsConnect() { return m_CMotor.IsConnect(); }
            private const int _WAIT_TIME = 100;
            public String Request_Version(int nAxis)
            {
                m_CMotor.ReadRom(nAxis, 0, 4);
                bool bOk = m_CMotor.WaitReceive(nAxis, _WAIT_TIME);
                if (bOk == true)
                    return m_CMotor.GetVersion_Str(nAxis);
                return null;
            }
            public String GetVersion_String(int nAxis) { return m_CMotor.GetVersion_Str(nAxis); }
            public int GetVersion(int nAxis) { return m_CMotor.GetVersion(nAxis); }
            public void DrvSrv(bool bDrv, bool bSrv) { m_CMotor.DrvSrv(bDrv, bSrv); }

            public const int _ADRESS_ROM_VERSION = 2;

            public const int _ADDRESS_CALIBRATED_POSITION = 58;
            public const int _ADDRESS_LED_CONTROL = 53;
            public const int _ADDRESS_PRESENT_CONTROL_MODE = 56;
            public const int _ADDRESS_TEMPERATURE = 55;
            public const int _ADDRESS_TICK = 57;
            public const int _ADDRESS_TORQUE_CONTROL = 52;
            public const int _ADDRESS_VOLTAGE = 54;
            public const int _STATUS_LED_BLUE = 2;
            public const int _STATUS_LED_GREEN = 1;
            public const int _STATUS_LED_NONE = 0;
            public const int _STATUS_LED_RED = 4;

            #region Motor Data
            public float GetAngle(int nAxis) { return m_CMotor.GetPos_Angle(nAxis); }
            public bool GetDrvSrv(int nAxis) { return m_CMotor.IsDrv(nAxis); }
            public bool GetLed_Red(int nAxis) { return m_CMotor.IsLed_Red(nAxis); }
            public bool GetLed_Blue(int nAxis) { return m_CMotor.IsLed_Blue(nAxis); }
            public bool GetLed_Green(int nAxis) { return m_CMotor.IsLed_Green(nAxis); }
            public int  GetVolt(int nAxis) { return m_CMotor.GetData_Ram(nAxis, _ADDRESS_TEMPERATURE); }
            public int  GetTemp(int nAxis) { return m_CMotor.GetData_Ram(nAxis, _ADDRESS_PRESENT_CONTROL_MODE); }
            public int  GetTick(int nAxis) { return m_CMotor.GetData_Ram(nAxis, _ADDRESS_TICK); }
            public int  GetEvd(int nAxis) { return m_CMotor.GetPos(nAxis); }
            public bool Request_MotorData(int nAxis)
            {
                m_CMotor.ReadMot(nAxis, _ADDRESS_TORQUE_CONTROL, 8);
                bool bOk = m_CMotor.WaitReceive(nAxis, _WAIT_TIME);
                if (bOk == true)
                {
                    // Drv Srv 
                    // Led(Red, Blue, Green)
                    // Volt, Temp, Mode, Tick
                    // Evd, Angle
                    return true;
                }
                return false;
            }
            #endregion Motor Data

            #region Led Command
            public void SetLed_Red(int nAxis, bool bOn) { m_CMotor.SetLed_Red(nAxis, bOn); }
            public void SetLed_Blue(int nAxis, bool bOn) { m_CMotor.SetLed_Blue(nAxis, bOn); }
            public void SetLed_Green(int nAxis, bool bOn) { m_CMotor.SetLed_Green(nAxis, bOn); }            
            #endregion Led Command

            public void Move(int nTime, int nAxis, float fAngle)
            {
                m_CMotor.SetCmd_Angle(nAxis, fAngle);
                m_CMotor.SetMot(nAxis, nTime);
            }
            private int m_nTimeValue = 0;
            public void Move(int nTime, params SAxis_t [] aSAsix)
            {
                for (int i = 0; i < aSAsix.Length; i++)
                {
                    m_CMotor.SetCmd_Angle(aSAsix[i].nAxis, aSAsix[i].fAngle);
                }
                m_CMotor.SetMot(nTime);
                m_nTimeValue = nTime;
            }
            public bool IsStop() { return m_CMotor.IsStop(); }
            public bool IsEmg() { return m_CMotor.IsEms(); }
            public void ResetFlag() { m_CMotor.ResetStop(); m_CMotor.ResetEms(); }
            public void Reset() { m_CMotor.Reset(); }
            public void Reset(int nAxis) { m_CMotor.Reset(nAxis); }
            public bool Wait(int nTime) 
            {
                CTimer CTmr = new CTimer();
                CTmr.Set();
                while ((IsStop() == false) && (IsEmg() == false))
                {
                    if (CTmr.Get() >= nTime) break;
                    Application.DoEvents();
                }
                if ((IsStop() == false) && (IsEmg() == false)) return true;
                return false;
            }
            public bool Wait_With_ReadPosition(int nAxis, int nTime)
            {
                CTimer CTmr = new CTimer();
                CTmr.Set();
                while ((IsStop() == false) && (IsEmg() == false))
                {
                    if (CTmr.Get() >= nTime) break;
                    Request_MotorData(nAxis);
                    Application.DoEvents();
                }
                if ((IsStop() == false) && (IsEmg() == false)) return true;
                return false;
            }
            public void Stop() { m_CMotor.Stop(); }
            public void Emg() { m_CMotor.Ems(); }


            #region Controller

            #region Mpsu Status 함수
            public int Controller_GetStatus1() { return m_CMotor.GetStatus1_Mpsu(); }
            public int Controller_GetStatus2() { return m_CMotor.GetStatus2_Mpsu(); }

            // Error = false, 정상 = true;
            public bool Controller_GetStatusMessage(out String strStatus1, out String strStatus2) { return m_CMotor.GetStatusMessage_Mpsu(out strStatus1, out strStatus2); }
            #endregion Mpsu Status 함수

            // 1 Tick 당 125 ms => 1:11.2=x:nTime => x = nTime / 11.2
            //private int Controller_CalcTime_ms_Remocon(int nTime) { return m_CMotor.CalcTime_ms_Remocon(nTime); }
            public void Controller_ReMocon(int nChannel_0x61_0x6a, int nTime_ms, int nKeyNumber) { m_CMotor.Mpsu_ReMocon(nChannel_0x61_0x6a, nTime_ms, nKeyNumber); }
            public void Controller_ReMocon(int nMpsuID, int nChannel_0x61_0x6a, int nTime_ms, int nKeyNumber) { m_CMotor.Mpsu_ReMocon(nMpsuID, nChannel_0x61_0x6a, nTime_ms, nKeyNumber); }
            public void Controller_Reboot(int nMpsuID) { m_CMotor.Mpsu_Reboot(nMpsuID); }
            public void Controller_Reboot() { m_CMotor.Mpsu_Reboot(); }
            public void Controller_Play_HeadLed_Buzz(int nHeadLedNum_1_63, int nBuzzNum_1_63) { m_CMotor.Mpsu_Play_HeadLed_Buzz(nHeadLedNum_1_63, nBuzzNum_1_63); }
            public void Controller_Play_HeadLed_Buzz(int nMpsuID, int nHeadLedNum_1_63, int nBuzzNum_1_63) { m_CMotor.Mpsu_Play_HeadLed_Buzz(nMpsuID, nHeadLedNum_1_63, nBuzzNum_1_63); }
            public void Controller_Play_Buzz(int nMpsuID, int nBuzzNum_1_63) { m_CMotor.Mpsu_Play_Buzz(nMpsuID, nBuzzNum_1_63); }
            public void Controller_Play_Buzz(int nBuzzNum_1_63) { m_CMotor.Mpsu_Play_Buzz(nBuzzNum_1_63); }
            public void Controller_Play_HeadLed(int nMpsuID, int nHeadLedNum_1_63) { m_CMotor.Mpsu_Play_HeadLed(nMpsuID, nHeadLedNum_1_63); }
            public void Controller_Play_HeadLed(int nHeadLedNum_1_63) { m_CMotor.Mpsu_Play_HeadLed(nHeadLedNum_1_63); }
            public void Controller_Play_Motion(int nMpsuID, int nMotionAddress, bool bReady) { m_CMotor.Mpsu_Play_Motion(nMpsuID, nMotionAddress, bReady); }
            public void Controller_Play_Motion(int nMotionAddress, bool bReady) { m_CMotor.Mpsu_Play_Motion(nMotionAddress, bReady); }
            public void Controller_Play_Motion(int nMotionAddress) { m_CMotor.Mpsu_Play_Motion(nMotionAddress); }
            public void Controller_Stop_Motion() { m_CMotor.Mpsu_Stop_Motion(); }
            //Debug Mode Flag를 1로 해서 보내면 디버깅 모드가 된다. 진입 후 Task No를 0x01로 한 패킷을 보내면 한 스텝 진행된다. 스텝이 진행될 때마다 현재 위치를 포함한 Ack 패킷을 보낸다.(AckPlcy가 1, 2일 때)
            //Task No를 0xFE로 한 패킷을 보내면 Task가 정지된다
            public void Controller_Play_Task(int nMpsuID, int nTaskNo) { m_CMotor.Mpsu_Play_Task(nMpsuID, nTaskNo); }
            public void Controller_Play_Task(bool bDebugingMode) { m_CMotor.Mpsu_Play_Task(bDebugingMode); }
            public void Controller_Play_Task() { m_CMotor.Mpsu_Play_Task(); }
            public void Controller_Stop_Task() { m_CMotor.Mpsu_Stop_Task(); }
            #endregion Controller
        }

        #region For Motor
        private class COjwMotor_Param
        {
            #region 생성자/소멸자
            public COjwMotor_Param() // 생성자 초기화 함수
            {
                m_SParam = new SParam_t();
                m_pSParam_Axis = new SParam_Axis_t[_MOTOR_MAX];
            }
            public void ReleaseClass()
            {
                m_pSParam_Axis = null;
            }
            ~COjwMotor_Param() // 소멸자
            {
                ReleaseClass();
            }
            #endregion

            #region parameter setup - 메소드 포함
            
            #region 구조체 변수 선언 - 파라미터 변수
            private const int _MOTOR_MAX = 512;
            private SParam_t m_SParam;// = new SParam_t();
            private SParam_Axis_t[] m_pSParam_Axis;// = new SParam_Axis_t[_MOTOR_MAX];
            #endregion 구조체 변수 선언 - 파라미터 변수

            // 함수들 ...
            public bool SetParam(int nPort, int nBaudRate, bool bCompleteStop, int nMotorMax)
            {
                #region // 통신속도 나열
                /*
                0: 600
                1: 1200
                2: 2400
                3: 4800
                4: 9600
                5: 14400
                6: 19200
                7: 38400
                8: 57600
                9: Axis9
                10: 115200
                11: 128000
                12: 200000
                13: 250000
                14: 256000
                15: 500000
                16: 1000000
                */
                #endregion // 통신속도 나열
                if (
                    (nBaudRate != 600) &&
                    (nBaudRate != 1200) &&
                    (nBaudRate != 2400) &&
                    (nBaudRate != 4800) &&
                    (nBaudRate != 9600) &&
                    (nBaudRate != 14400) &&
                    (nBaudRate != 19200) &&
                    (nBaudRate != 38400) &&
                    (nBaudRate != 57600) &&
                    (nBaudRate != 115200) &&
                    (nBaudRate != 128000) &&
                    (nBaudRate != 200000) &&
                    (nBaudRate != 250000) &&
                    (nBaudRate != 256000) &&
                    (nBaudRate != 500000) &&
                    (nBaudRate != 1000000)
                )
                {
                    return false;
                }
                if (nPort <= 0) return false;
                //if ((fMechMove_Max <= 0) || (fDegree_Max <= 0)) return false;

                m_SParam.nBaudRate = nBaudRate;
                m_SParam.nComPort = nPort;
                //m_SParam.fMechMove = fMechMove_Max;
                //m_SParam.fDegree = fDegree_Max;
                m_SParam.bCompleteStop = bCompleteStop;
                m_SParam.nMotorMax = nMotorMax;
                return true;
            }
            public bool SetParam(SParam_t SParam)
            {
                return SetParam(SParam.nComPort, SParam.nBaudRate, SParam.bCompleteStop, SParam.nMotorMax);
            }
            public bool SetParam_Axis(int nAxis, SParam_Axis_t SParam_Axis)
            {
                return SetParam_Axis(nAxis, SParam_Axis.nID, SParam_Axis.nDir, SParam_Axis.fLimitUp, SParam_Axis.fLimitDn, SParam_Axis.fCenterPos, SParam_Axis.fOffsetAngle_Display, SParam_Axis.fMechMove, SParam_Axis.fDegree);
            }
            public bool SetParam_Axis(int nAxis, int nID, int nDir, float fLimitUp, float fLimitDn, float fCenterPos, float fOffsetAngle_Display, float fMechMove, float fDegree)
            {
                if ((nAxis >= _MOTOR_MAX) || (nID >= _MOTOR_MAX)) return false;

                m_pSParam_Axis[nAxis].nID = nID;
                m_pSParam_Axis[nAxis].nDir = nDir;
                m_pSParam_Axis[nAxis].fLimitUp = fLimitUp;
                m_pSParam_Axis[nAxis].fLimitDn = fLimitDn;
                m_pSParam_Axis[nAxis].fCenterPos = fCenterPos;
                m_pSParam_Axis[nAxis].fOffsetAngle_Display = fOffsetAngle_Display;
                m_pSParam_Axis[nAxis].fMechMove = fMechMove;
                m_pSParam_Axis[nAxis].fDegree = fDegree;
                return true;
            }
            public bool SetParam_Axis(SParam_Axis_t[] pSParam_Axis)
            {
                bool bRet = true;
                for (int i = 0; i < pSParam_Axis.Length; i++)
                {
                    bool bTmp = SetParam_Axis(i, pSParam_Axis[i].nID, pSParam_Axis[i].nDir, pSParam_Axis[i].fLimitUp, pSParam_Axis[i].fLimitDn, pSParam_Axis[i].fCenterPos,
                                                 pSParam_Axis[i].fOffsetAngle_Display, pSParam_Axis[i].fMechMove, pSParam_Axis[i].fDegree);
                    if (bTmp == false) bRet = false;
                }
                return bRet;
            }
            public SParam_t GetParam() { return m_SParam; }
            public void GetParam(out int nPort, out int nBaudRate, out bool bCompleteStop)
            {
                nPort = m_SParam.nComPort;
                nBaudRate = m_SParam.nBaudRate;
                bCompleteStop = m_SParam.bCompleteStop;
            }
            public int GetMotorMax() { return m_SParam.nMotorMax; }
            public SParam_Axis_t GetParam_Axis(int nAxis) { return m_pSParam_Axis[nAxis]; }
            public SParam_Axis_t GetParam_Axis(int nAxis, out int nID, out int nDir, out float fLimitUp, out float fLimitDn, out float fCenterPos,
                                               out float fOffsetAngle_Display, out float fMechMove, out float fDegree)
            {
                nID = m_pSParam_Axis[nAxis].nID;
                nDir = m_pSParam_Axis[nAxis].nDir;
                fLimitUp = m_pSParam_Axis[nAxis].fLimitUp;
                fLimitDn = m_pSParam_Axis[nAxis].fLimitDn;
                fCenterPos = m_pSParam_Axis[nAxis].fCenterPos;

                fOffsetAngle_Display = m_pSParam_Axis[nAxis].fOffsetAngle_Display;
                fMechMove = m_pSParam_Axis[nAxis].fMechMove;
                fDegree = m_pSParam_Axis[nAxis].fDegree;

                return m_pSParam_Axis[nAxis];
            }
            public SParam_Axis_t[] GetParam_Axis() { return m_pSParam_Axis; }

            #region ParamInit
            private void ParamInit()
            {
                // Default Param
                m_SParam.nBaudRate = 115200;
                m_SParam.nComPort = 1;
                m_SParam.bCompleteStop = false;
                m_SParam.nMotorMax = 0;

                // Axis Param  
                for (int i = 0; i < _MOTOR_MAX; i++)
                {
                    m_pSParam_Axis[i].nID = 253;
                    m_pSParam_Axis[i].nDir = 0;
                    m_pSParam_Axis[i].fLimitUp = 0;
                    m_pSParam_Axis[i].fLimitDn = 0;
                    m_pSParam_Axis[i].fCenterPos = 512.0f;
                    m_pSParam_Axis[i].fOffsetAngle_Display = 0.0f;
                    m_pSParam_Axis[i].fMechMove = 1024.0f;
                    m_pSParam_Axis[i].fDegree = 333.3f;
                }
            }
            #endregion ParamInit
            #region ParamLoad
            public bool ParamLoad(String strFileName)
            {
                FileInfo f = new FileInfo(strFileName);
                String[] pstrLine = new String[1];
                String[] pstrTmp;
                StreamReader fsText;
                try
                {
                    fsText = f.OpenText();
                    int i = 0;

                    string strTmp;

                    //bOpen = true;
                    while (true)
                    {
                        strTmp = fsText.ReadLine();
                        //if ((strTmp == null) || (strTmp.Trim() == "")) break;
                        if (strTmp == null) break;
                        i++;
                        // 복사본 생성
                        pstrTmp = (String[])pstrLine.Clone();
                        // 메모리 재할당
                        pstrLine = new String[i];
                        if (i > 1)
                        {
                            // 복사
                            pstrTmp.CopyTo(pstrLine, 0);
                        }
                        pstrLine[i - 1] = strTmp;
                    }
                    fsText.Close();

                    int nPos = 0;

                    // Default Param
                    m_SParam.nBaudRate = CConvert.StrToInt(pstrLine[nPos++]);
                    m_SParam.nComPort = CConvert.StrToInt(pstrLine[nPos++]);

                    m_SParam.bCompleteStop = CConvert.StrToBool(pstrLine[nPos++]);
                    m_SParam.nMotorMax = CConvert.StrToInt(pstrLine[nPos++]);

                    //Reserve (30 개)
                    nPos++; nPos++; nPos++; nPos++; nPos++;
                    nPos++; nPos++; nPos++; nPos++; nPos++;

                    nPos++; nPos++; nPos++; nPos++; nPos++;
                    nPos++; nPos++; nPos++; nPos++; nPos++;

                    nPos++; nPos++; nPos++; nPos++; nPos++;
                    nPos++; nPos++; nPos++; nPos++; nPos++;

                    // Axis Param  
                    for (i = 0; i < _MOTOR_MAX; i++)
                    {
                        m_pSParam_Axis[i].nID = CConvert.StrToInt(pstrLine[nPos++]);
                        m_pSParam_Axis[i].nDir = CConvert.StrToInt(pstrLine[nPos++]);
                        m_pSParam_Axis[i].fLimitUp = CConvert.StrToFloat(pstrLine[nPos++]);
                        m_pSParam_Axis[i].fLimitDn = CConvert.StrToFloat(pstrLine[nPos++]);
                        m_pSParam_Axis[i].fCenterPos = CConvert.StrToFloat(pstrLine[nPos++]);
                        m_pSParam_Axis[i].fOffsetAngle_Display = CConvert.StrToFloat(pstrLine[nPos++]);
                        m_pSParam_Axis[i].fMechMove = CConvert.StrToFloat(pstrLine[nPos++]);
                        m_pSParam_Axis[i].fDegree = CConvert.StrToFloat(pstrLine[nPos++]);
                        SetParam_Axis(i, m_pSParam_Axis[i].nID, m_pSParam_Axis[i].nDir, m_pSParam_Axis[i].fLimitUp, m_pSParam_Axis[i].fLimitDn, m_pSParam_Axis[i].fCenterPos,
                                         m_pSParam_Axis[i].fOffsetAngle_Display, m_pSParam_Axis[i].fMechMove, m_pSParam_Axis[i].fDegree);

                        //Reserve (30 개)
                        nPos++; nPos++; nPos++; nPos++; nPos++;
                        nPos++; nPos++; nPos++; nPos++; nPos++;

                        nPos++; nPos++; nPos++; nPos++; nPos++;
                        nPos++; nPos++; nPos++; nPos++; nPos++;

                        nPos++; nPos++; nPos++; nPos++; nPos++;
                        nPos++; nPos++; nPos++; nPos++; nPos++;
                    }

                    //DispParam(cmbAxis.SelectedIndex);
                    return true;
                }
                catch
                {
                    //DispParam(cmbAxis.SelectedIndex);
                    return false;
                }
                finally
                {
                    pstrTmp = null;
                    pstrLine = null;
                    f = null;
                }
            }
            #endregion ParamLoad

            #region ParamSave
            public void ParamSave(String strFileName)
            {
                FileInfo f = new FileInfo(strFileName);
                StreamWriter fs = f.CreateText();

                // 스트림 버퍼를 비운다.
                fs.Flush();

                // Default Param

                // Default Param
                //             m_SParam.nBaudRate = CConvert.StrToInt(txtDefaultParam_BaudRate.Text);
                //             m_SParam.nComPort = CConvert.StrToInt(txtDefaultParam_ComPort.Text);
                //             m_SParam.fMechMove = CConvert.StrToFloat(txtDefaultParam_Mech.Text);
                //             m_SParam.fDegree = CConvert.StrToFloat(txtDefaultParam_Degree.Text);
                //             m_SParam.bCompleteStop = CConvert.StrToBool(txtDefaultParam_CompleteStop_Test.Text);

                fs.WriteLine(CConvert.IntToStr(m_SParam.nBaudRate));
                fs.WriteLine(CConvert.IntToStr(m_SParam.nComPort));

                fs.WriteLine(CConvert.BoolToStr(m_SParam.bCompleteStop));
                fs.WriteLine(CConvert.IntToStr(m_SParam.nMotorMax));

                // 30개 ( Reserve )
                int j = 0;
                fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);
                fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);

                fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);
                fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);

                fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);
                fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);

                // Axis Param            
                for (int i = 0; i < _MOTOR_MAX; i++)
                {
                    fs.WriteLine(CConvert.IntToStr(m_pSParam_Axis[i].nID));
                    fs.WriteLine(CConvert.IntToStr(m_pSParam_Axis[i].nDir));
                    fs.WriteLine(CConvert.FloatToStr(m_pSParam_Axis[i].fLimitUp));
                    fs.WriteLine(CConvert.FloatToStr(m_pSParam_Axis[i].fLimitDn));
                    fs.WriteLine(CConvert.FloatToStr(m_pSParam_Axis[i].fCenterPos));
                    fs.WriteLine(CConvert.FloatToStr(m_pSParam_Axis[i].fOffsetAngle_Display));
                    fs.WriteLine(CConvert.FloatToStr(m_pSParam_Axis[i].fMechMove));
                    fs.WriteLine(CConvert.FloatToStr(m_pSParam_Axis[i].fDegree));

                    SetParam_Axis(i, m_pSParam_Axis[i].nID, m_pSParam_Axis[i].nDir, m_pSParam_Axis[i].fLimitUp, m_pSParam_Axis[i].fLimitDn, m_pSParam_Axis[i].fCenterPos,
                                     m_pSParam_Axis[i].fOffsetAngle_Display, m_pSParam_Axis[i].fMechMove, m_pSParam_Axis[i].fDegree);
                    // 30개 ( Reserve )
                    j = 0;
                    fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);
                    fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);

                    fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);
                    fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);

                    fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);
                    fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++); fs.WriteLine("Reserve" + j++);
                }

                //DispParam(cmbAxis.SelectedIndex);

                fs.Close();
                f = null;
            }
            #endregion ParamSave

            #region [ParamCreate]파라미터가 있다면 로드하고 없다면 만든다. - 폼 로드시에 한번만 호출하면 됨
            public void ParamCreate(string strFileName)
            {
                FileInfo fileBack = new FileInfo(strFileName);
                if (fileBack.Exists) // 백업한 파일이 있는지 체크
                {
                    bool bOk = ParamLoad(strFileName);
                    if (bOk == false)
                    {
                        ParamInit();
                        ParamSave(strFileName);
                    }
                }
                else
                {
                    ParamInit();
                    ParamSave(strFileName);
                }
            }
            #endregion [ParamCreate]파라미터가 있다면 로드하고 없다면 만든다. - 폼 로드시에 한번만 호출하면 됨

            #endregion parameter setup - 메소드 포함
        }

        public class COjwMotor
        {
            #region 생성자/소멸자
            private const int _CNT_TIMER = 999;
            public COjwMotor() // 생성자 초기화 함수
            {
                for (int i = 0; i < _CNT_TIMER; i++) m_aCTmr[i] = new CTimer();

                m_SerialPort = new SerialPort();
                //델리게이트 생성
                //runDele d = new MainForm.runDele(this.initServer);
                //BeginInvoke 로 매개변수 전달
                //d.BeginInvoke("string value", null, null);

                SetParam_Axis_Default(false);

                m_bClassEnd = false;
            }

            public void ReleaseClass()
            {                
                m_bClassEnd = true;
                DisConnect();
                m_SerialPort.Dispose();
            }

            ~COjwMotor() // 소멸자
            {
                ReleaseClass();
            }
            #endregion

            private Thread Reader;             // 읽기 쓰레드
            
            private const int _ADDRESS_TORQUE_CONTROL = 52;
            private const int _ADDRESS_LED_CONTROL = 53;
            private const int _ADDRESS_VOLTAGE = 54;
            private const int _ADDRESS_TEMPERATURE = 55;
            private const int _ADDRESS_PRESENT_CONTROL_MODE = 56;
            private const int _ADDRESS_TICK = 57;
            private const int _ADDRESS_CALIBRATED_POSITION = 58;

            private Ojw.CTimer[] m_aCTmr = new CTimer[_CNT_TIMER];

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

            private SerialPort m_SerialPort = null;
            private bool m_bEventInit = false;
            //public static bool Open()

            #region SetAutoReturn / IsAutoReturn - 통신상태를 자동으로 업데이트 할 것인지를 결정
            // 통신상태를 자동으로 업데이트 할 것인지를 결정
            private bool m_bAutoReturn = false;
            private int m_nCmdAxis = 0;
            public void SetAutoReturn(bool bOk)
            {
                //if (IsAutoReturn() == false) ReadMot(0, 0, _SIZE_RAM);//_ADDRESS_TORQUE_CONTROL, 8);
                m_bAutoReturn = bOk;
            }
            public bool IsAutoReturn() { return m_bAutoReturn; }
            #endregion

            private bool[] m_abSpeedType = new bool[_MOTOR_MAX];
            public bool GetSpeedType(int nAxis) { return m_abSpeedType[nAxis]; }
            public bool SetSpeedType(int nAxis, bool bSpeedType) { return m_abSpeedType[nAxis] = bSpeedType; }

            #region 축과 ID 설정
            private int[] m_pnAxis_By_ID = new int[256]; // ID = 0 ~ 254 까지 가능, 단, 254 는 BroadCasting - 에러방지를 위해서는 가용 수치까지 전부 잡는다.
            public void SetAxis_By_ID(int nID, int nAxis) { m_pnAxis_By_ID[nID] = nAxis; }
            public int GetAxis_By_ID(int nID) { return (nID == 0xfe) ? 0xfe : m_pnAxis_By_ID[nID]; }
            public int GetID_By_Axis(int nAxis) { return (nAxis == 0xfe) ? 0xfe : m_pSMot[nAxis].nID; }
            public void SetParam_Axis_Default(bool bEncodertype)
            {
                float fMechMove = 1024.0f;
                float fDegree = 333.333f;
                float fCenterPos = 512.0f;
                if (bEncodertype)
                {
                    fMechMove = 6391.605f;
                    fDegree = 360.0f;
                    fCenterPos = 2964.0f;
                }

                for (int nAxis = 0; nAxis < _MOTOR_MAX; nAxis++)
                {
                    SetParam_Axis(nAxis, nAxis, 0, 99999.0f, -99999.0f, fCenterPos, fMechMove, fDegree);
                }
            }
            public void SetParam_Axis(int nAxis, int nID, int nDir, float fLimitUp, float fLimitDn, float fCenterPos, float fMechMove, float fDegree)
            {
                if (nID < 0) return;
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
                if (SParam_Axis.nID < 0) return;
                m_pSMot[nAxis].nID = SParam_Axis.nID;
                m_pSMot[nAxis].nDir = SParam_Axis.nDir;
                m_pSMot[nAxis].fCenterPos = SParam_Axis.fCenterPos;
                m_pSMot[nAxis].fMechMove = SParam_Axis.fMechMove;
                m_pSMot[nAxis].fDegree = SParam_Axis.fDegree;

                m_pSMot[nAxis].fLimitUp = SParam_Axis.fLimitUp;
                m_pSMot[nAxis].fLimitDn = SParam_Axis.fLimitDn;

                SetAxis_By_ID(SParam_Axis.nID, nAxis);
            }
            public void SetParam_Item_Dir(int nAxis, int nDir) { m_pSMot[nAxis].nDir = nDir; }
            public int GetParam_Item_Dir(int nAxis) { return m_pSMot[nAxis].nDir; }
            public void SetParam_Item_ID(int nAxis, int nID) { m_pSMot[nAxis].nID = nID; }
            public int GetParam_Item_ID(int nAxis) { return m_pSMot[nAxis].nID; }
            public void SetParam_Item_fCenterPos(int nAxis, float fCenterPos) { m_pSMot[nAxis].fCenterPos = fCenterPos; }
            public float GetParam_Item_fCenterPos(int nAxis) { return m_pSMot[nAxis].fCenterPos; }
            public void SetParam_Item_fMechMove(int nAxis, float fMechMove) { m_pSMot[nAxis].fMechMove = fMechMove; }
            public float GetParam_Item_fMechMove(int nAxis) { return m_pSMot[nAxis].fMechMove; }
            public void SetParam_Item_fDegree(int nAxis, float fDegree) { m_pSMot[nAxis].fDegree = fDegree; }
            public float GetParam_Item_fDegree(int nAxis) { return m_pSMot[nAxis].fDegree; }
            public void SetParam_Item_fLimitUp(int nAxis, float fLimitUp) { m_pSMot[nAxis].fLimitUp = fLimitUp; }
            public float GetParam_Item_fLimitUp(int nAxis) { return m_pSMot[nAxis].fLimitUp; }
            public void SetParam_Item_fLimitDn(int nAxis, float fLimitDn) { m_pSMot[nAxis].fLimitDn = fLimitDn; }
            public float GetParam_Item_fLimitDn(int nAxis) { return m_pSMot[nAxis].fLimitDn; }
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
            #endregion

            #region 제어변수 및 함수
            private const int _MOTOR_MAX = 256;
            private int m_nMotor_Max = _MOTOR_MAX;
            public void SetMotor_Max(int nMotorMaxNum) { m_nMotor_Max = nMotorMaxNum; }
            public int GetMotor_Max() { return m_nMotor_Max; }
            private SMot_t[] m_pSMot = new SMot_t[_MOTOR_MAX];
            // 통신 카운터변수
            private long[] m_plSendCount = new long[_MOTOR_MAX];
            private long[] m_plReceiveCount = new long[_MOTOR_MAX];

            //private int[,] m_pnRam = new int[_MOTOR_MAX, 100]; // 레지스터의 갯수가 58개 => 100개로 정함
            //private int[,] m_pnRom = new int[_MOTOR_MAX, 100]; // 레지스터의 갯수가 58개 => 100개로 정함
            private int[,] m_pnStatus = new int[4, _MOTOR_MAX]; // Status1, Status2, Status_Drv_Srv, LED
            private int[] m_pnStatus_Sensor = new int[4]; // Status1, Status2, Reserve, LED
            private int[] m_pnStatus_Mpsu = new int[4]; // Status1, Status2, Reserve, LED
            private int[] m_pnStatus_Head = new int[4]; // Status1, Status2, Reserve, LED

            private int[] m_pnPos = new int[_MOTOR_MAX];
            //         public int GetReg(int nAxis, int nAddress) { return m_pnRam[nAxis, nAddress]; }
            //         public int[,] GetReg() { return m_pnRam; }
            //         public int GetReg_Rom(int nAxis, int nAddress) { return m_pnRom[nAxis, nAddress]; }
            //         public int[,] GetReg_Rom() { return m_pnRom; }
            // 해당 축(Axis) 에 pbyteData 를 nStartAddress 번지부터 쓴다.
            public void WriteRom(int nAxis, int nStartAddress, byte byteData)
            {
                if (IsConnect() == false) return;

                int nID = GetID_By_Axis(nAxis);
                int nDefaultSize = _CHECKSUM2 + 1;

                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x01; // Rom 영역을 쓴다.

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize++] = (byte)(nStartAddress & 0xff);// 레지스터 번지

                ////////            
                pbyteBuffer[nDefaultSize++] = (byte)(0x01);   // 이후의 레지스터 사이즈
                pbyteBuffer[nDefaultSize++] = (byte)(byteData & 0xff);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)(nDefaultSize & 0xff);

                MakeCheckSum(nDefaultSize, pbyteBuffer);

                // 보내기 전에 Tick 을 Set 한다.
                SendPacket(pbyteBuffer, nDefaultSize);
                pbyteBuffer = null;
            }
            public void WriteRom(int nAxis, int nStartAddress, byte[] pbyteData)
            {
                if (IsConnect() == false) return;

                int nID = GetID_By_Axis(nAxis);
                int nDefaultSize = _CHECKSUM2 + 1;

                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x01; // Rom 영역을 쓴다.

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize++] = (byte)(nStartAddress & 0xff);// 레지스터 번지

                ////////            
                pbyteBuffer[nDefaultSize++] = (byte)(pbyteData.Length & 0xff);   // 이후의 레지스터 사이즈
                for (int i = 0; i < pbyteData.Length; i++)
                    pbyteBuffer[nDefaultSize++] = (byte)(pbyteData[i] & 0xff);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)(nDefaultSize & 0xff);

                MakeCheckSum(nDefaultSize, pbyteBuffer);

                // 보내기 전에 Tick 을 Set 한다.
                SendPacket(pbyteBuffer, nDefaultSize);
                pbyteBuffer = null;
            }
            public void WriteRom(int nAxis, int nStartAddress, byte[] pbyteData, out byte[] pbyteResult)
            {
                if (IsConnect() == false)
                {
                    pbyteResult = null;
                    return;
                }

                int nID = GetID_By_Axis(nAxis);
                int nDefaultSize = _CHECKSUM2 + 1;

                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x01; // Rom 영역을 쓴다.

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize++] = (byte)(nStartAddress & 0xff);// 레지스터 번지

                ////////            
                pbyteBuffer[nDefaultSize++] = (byte)(pbyteData.Length & 0xff);   // 이후의 레지스터 사이즈
                for (int i = 0; i < pbyteData.Length; i++)
                    pbyteBuffer[nDefaultSize++] = (byte)(pbyteData[i] & 0xff);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)(nDefaultSize & 0xff);

                MakeCheckSum(nDefaultSize, pbyteBuffer);

                // 보내기 전에 Tick 을 Set 한다.
                SendPacket(pbyteBuffer, nDefaultSize);
                pbyteResult = new byte[nDefaultSize];
                Array.Copy(pbyteBuffer, pbyteResult, nDefaultSize);
                pbyteBuffer = null;
            }
            public void WriteRam(int nAxis, int nStartAddress, byte byteData)
            {
                if (IsConnect() == false) return;

                int nID = GetID_By_Axis(nAxis);
                int nDefaultSize = _CHECKSUM2 + 1;

                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x03; // Ram 영역을 쓴다.

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize++] = (byte)(nStartAddress & 0xff);// 레지스터 번지

                ////////            
                pbyteBuffer[nDefaultSize++] = (byte)(0x01);   // 이후의 레지스터 사이즈
                pbyteBuffer[nDefaultSize++] = (byte)(byteData & 0xff);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)(nDefaultSize & 0xff);

                MakeCheckSum(nDefaultSize, pbyteBuffer);

                // 보내기 전에 Tick 을 Set 한다.
                SendPacket(pbyteBuffer, nDefaultSize);
                pbyteBuffer = null;
            }
            public void WriteRam(int nAxis, int nStartAddress, byte[] pbyteData)
            {
                if (IsConnect() == false) return;

                int nID = GetID_By_Axis(nAxis);
                int nDefaultSize = _CHECKSUM2 + 1;

                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x03; // Ram 영역을 쓴다.

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize++] = (byte)(nStartAddress & 0xff);// 레지스터 번지

                ////////            
                pbyteBuffer[nDefaultSize++] = (byte)(pbyteData.Length & 0xff);   // 이후의 레지스터 사이즈
                for (int i = 0; i < pbyteData.Length; i++)
                    pbyteBuffer[nDefaultSize++] = (byte)(pbyteData[i] & 0xff);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)(nDefaultSize & 0xff);

                MakeCheckSum(nDefaultSize, pbyteBuffer);

                // 보내기 전에 Tick 을 Set 한다.
                SendPacket(pbyteBuffer, nDefaultSize);
                pbyteBuffer = null;
            }
            #region Pos 함수
            private void SetPos(int nAxis, int nPos) { m_pnPos[nAxis] = nPos; }
            public int GetPos(int nAxis) { return m_pnPos[nAxis]; }
            public float GetPos_Angle(int nAxis) { return CalcEvd2Angle(nAxis, m_pnPos[nAxis]); }
            #endregion Pos 함수

            #region Calc - 계산함수(시간, 각도)
            // ms 의 값을 Raw 데이타(패킷용)으로 변환
            public int CalcTime_ms(int nTime)
            {
                // 1 Tick 당 11.2 ms => 1:11.2=x:nTime => x = nTime / 11.2
                return ((nTime <= 0) ? 1 : (int)Math.Round((float)nTime / 11.2f));
            }
            public int CalcAngle2Evd(int nAxis, float fValue)
            {
                try
                {
                    fValue *= ((m_pSMot[nAxis].nDir == 0) ? 1.0f : -1.0f);
                    int nData = 0;
                    if (GetCmd_Flag_Mode(nAxis) != 0)   // 속도제어
                    {
                        nData = (int)Math.Round(fValue);
                    }
                    else
                    {
                        // 위치제어
                        nData = (int)Math.Round((m_pSMot[nAxis].fMechMove * fValue) / m_pSMot[nAxis].fDegree);
                        nData = nData + (int)Math.Round(m_pSMot[nAxis].fCenterPos);
                        //if (nData < 0)
                        //{
                        //    nData += (int)Math.Round(m_pSMot[nAxis].fMechMove)*2;
                        //}
                    }

                    return nData;
                    //return (nData + _CENTER_POS);
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
                    if (GetCmd_Flag_Mode(nAxis) != 0)   // 속도제어
                        fValue2 = (float)nValue * fValue;
                    else                                // 위치제어
                        fValue2 = (float)(((m_pSMot[nAxis].fDegree * ((float)(nValue - (int)Math.Round(m_pSMot[nAxis].fCenterPos)))) / m_pSMot[nAxis].fMechMove) * fValue);
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
            public void SetLimitEn(bool bOn) { m_bIgnoredLimit = !bOn; }
            public bool GetLimitEn() { return !m_bIgnoredLimit; }
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

            //         public int CalcLimit(int nAxis, int nPos)
            //         {
            //             int nRet = nPos;
            //             // 관절 리미트 클리핑
            //             if (
            //                 (m_pSMot[nAxis].nLimitUp > m_pSMot[nAxis].nLimitDn)
            //             ) nRet = Clip(m_pSMot[nAxis].nLimitUp, m_pSMot[nAxis].nLimitDn, nRet);
            //             // 2차 클리핑
            //             nRet = Clip(1920, -127, nRet);
            // 
            //             return nRet;
            //         }

            public int CalcLimit_Evd(int nAxis, int nValue)
            {
                if ((GetCmd_Flag_Mode(nAxis) == 0) || (GetCmd_Flag_Mode(nAxis) == 2))
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
            public float CalcLimit_Angle(int nAxis, float fValue)
            {
                if ((GetCmd_Flag_Mode(nAxis) == 0) || (GetCmd_Flag_Mode(nAxis) == 2))
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
            #region [SMot_t]관절 파라미터 구조체 선언
            public struct SMot_t
            {
                public bool bEn;

                public int nDir;
                //중심위치
                public float fCenterPos;

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
            public void InitCmd() { for (int i = 0; i < m_nMotor_Max; i++) { m_pSMot[i].bEn = false; SetCmd_Flag(i, false, GetSpeedType(i), false, false, false, true); } }
            public void SetCmd(int nAxis, int nPos) { m_pSMot[nAxis].bEn = true; m_pSMot[nAxis].nPos = CalcLimit_Evd(nAxis, nPos); SetCmd_Flag_NoAction(nAxis, false); }
            public void SetCmd_Angle(int nAxis, float fAngle) { m_pSMot[nAxis].bEn = true; m_pSMot[nAxis].nPos = CalcLimit_Evd(nAxis, CalcAngle2Evd(nAxis, fAngle)); SetCmd_Flag_NoAction(nAxis, false); }
            public int GetCmd(int nAxis) { return m_pSMot[nAxis].nPos; }
            public float GetCmd_Angle(int nAxis) { return CalcEvd2Angle(nAxis, m_pSMot[nAxis].nPos); }
            public void SetCmd_Flag(int nAxis, int nFlag) { m_pSMot[nAxis].nFlag = nFlag; }
            public void SetCmd_Flag(int nAxis, bool bStop, bool bMode_Speed, bool bLed_Green, bool bLed_Blue, bool bLed_Red, bool bNoAction) { m_pSMot[nAxis].nFlag = ((bStop == true) ? _FLAG_STOP : 0) | ((bMode_Speed == true) ? _FLAG_MODE_SPEED : 0) | ((bLed_Green == true) ? _FLAG_LED_GREEN : 0) | ((bLed_Blue == true) ? _FLAG_LED_BLUE : 0) | ((bLed_Red == true) ? _FLAG_LED_RED : 0) | ((bNoAction == true) ? _FLAG_NO_ACTION : 0); }
            public int GetCmd_Flag(int nAxis) { return m_pSMot[nAxis].nFlag; }
            public void SetCmd_Flag_Stop(int nAxis, bool bStop) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xfe) | ((bStop == true) ? _FLAG_STOP : 0); }
            public void SetCmd_Flag_Mode(int nAxis, bool bMode_Speed) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xfd) | ((bMode_Speed == true) ? _FLAG_MODE_SPEED : 0); }
            public void SetCmd_Flag_Led(int nAxis, bool bGreen, bool bBlue, bool bRed) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xe3) | ((bGreen == true) ? _FLAG_LED_GREEN : 0) | ((bBlue == true) ? _FLAG_LED_BLUE : 0) | ((bRed == true) ? _FLAG_LED_RED : 0); }
            public void SetCmd_Flag_Led_Green(int nAxis, bool bGreen) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xfb) | ((bGreen == true) ? _FLAG_LED_GREEN : 0); }
            public void SetCmd_Flag_Led_Blue(int nAxis, bool bBlue) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xf7) | ((bBlue == true) ? _FLAG_LED_BLUE : 0); }
            public void SetCmd_Flag_Led_Red(int nAxis, bool bRed) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xef) | ((bRed == true) ? _FLAG_LED_RED : 0); }
            public void SetCmd_Flag_NoAction(int nAxis, bool bNoAction) { m_pSMot[nAxis].nFlag = (m_pSMot[nAxis].nFlag & 0xdf) | ((bNoAction == true) ? _FLAG_NO_ACTION : 0); }

            // 1111 1101
            public int GetCmd_Flag_Mode(int nAxis) { return (((m_pSMot[nAxis].nFlag & _FLAG_MODE_SPEED) != 0) ? 1 : 0); }

            public bool GetCmd_Flag_Led_Green(int nAxis) { return (((m_pSMot[nAxis].nFlag & 0x04) != 0) ? true : false); }
            public bool GetCmd_Flag_Led_Blue(int nAxis) { return (((m_pSMot[nAxis].nFlag & 0x08) != 0) ? true : false); }
            public bool GetCmd_Flag_Led_Red(int nAxis) { return (((m_pSMot[nAxis].nFlag & 0x10) != 0) ? true : false); }

            #endregion Cmd 함수

            #region Status 함수
            private void SetStatus1(int nAxis, int nStatus) { m_pnStatus[0, nAxis] = nStatus; }
            public int GetStatus1(int nAxis) { return m_pnStatus[0, nAxis]; }
            private void SetStatus2(int nAxis, int nStatus) { m_pnStatus[1, nAxis] = nStatus; }
            public int GetStatus2(int nAxis) { return m_pnStatus[1, nAxis]; }
            private void SetStatus_DrvSrv(int nAxis, int nStatus) { m_pnStatus[2, nAxis] = nStatus; }

            // Error = false, 정상 = true;
            public bool GetStatusMessage(int nAxis, out String strStatus1, out String strStatus2)
            {
                int nStatus1 = GetStatus1(nAxis);
                int nStatus2 = GetStatus2(nAxis);
                strStatus1 = "";
                strStatus2 = "";
                if ((nStatus1 & 0x01) != 0) strStatus1 += "[Exceed Input Voltage Limit]";
                if ((nStatus1 & 0x02) != 0) strStatus1 += "[Exceed Allowed POT Limit]";
                if ((nStatus1 & 0x04) != 0) strStatus1 += "[Exceed Temperature Limit]";
                if ((nStatus1 & 0x08) != 0) strStatus1 += "[Invalied Packet]";
                if ((nStatus1 & 0x10) != 0) strStatus1 += "[Overload detected]";
                if ((nStatus1 & 0x20) != 0) strStatus1 += "[Driver falult detected]";
                if ((nStatus1 & 0x40) != 0) strStatus1 += "[ROM REG Distorted]";
                if ((nStatus1 & 0x80) != 0) strStatus1 += "[reserved]";

                if ((nStatus2 & 0x01) != 0) strStatus2 += "[Moving flag]";
                if ((nStatus2 & 0x02) != 0) strStatus2 += "[Inposition flag]";
                if ((nStatus2 & 0x04) != 0) strStatus2 += "[Checksum Error]";
                if ((nStatus2 & 0x08) != 0) strStatus2 += "[Unknown Command]";
                if ((nStatus2 & 0x10) != 0) strStatus2 += "[Exceed REG range]";
                if ((nStatus2 & 0x20) != 0) strStatus2 += "[Garbage detected]";
                if ((nStatus2 & 0x40) != 0) strStatus2 += "[MOTOR_ON flag]";
                if ((nStatus2 & 0x80) != 0) strStatus2 += "[reserved]";
                return ((((nStatus1 & 0xff) + (nStatus2 & 0x3c)) != 0) ? false : true);
            }
            #endregion

            #region Version
            public String GetVersion_Str(int nAxis)
            {
                String strRet =
                    //"[" + CConvert.IntToHex(m_pnRom[nAxis, 0], 2) + "]" +
                    //"[" + CConvert.IntToHex(m_pnRom[nAxis, 1], 2) + "]" +
                                "[" + CConvert.IntToHex(m_pnRom[nAxis, 2], 2) + "]" +
                                "[" + CConvert.IntToHex(m_pnRom[nAxis, 3], 2) + "]";
                return strRet;
            }
            public int GetVersion(int nAxis)
            {
                return (int)((int)(m_pnRom[nAxis, 2]) * 256 + (int)m_pnRom[nAxis, 3]);
            }
            public String GetModelNum_Str(int nAxis)
            {
                String strRet =
                                "[" + CConvert.IntToHex(m_pnRom[nAxis, 0], 2) + "]" +
                                "[" + CConvert.IntToHex(m_pnRom[nAxis, 1], 2) + "]";// +
                //"[" + CConvert.IntToHex(m_pnRom[nAxis, 2], 2) + "]" +
                //"[" + CConvert.IntToHex(m_pnRom[nAxis, 3], 2) + "]";
                return strRet;
            }
            public int GetModelNum(int nAxis)
            {
                return (int)((int)(m_pnRom[nAxis, 0]) * 256 + (int)m_pnRom[nAxis, 1]);
            }
            #endregion Version

            #region LED
            private void SetStatus_Led(int nAxis, int nStatus) { m_pnStatus[3, nAxis] = nStatus; }
            public const int _STATUS_LED_NONE = 0;
            public const int _STATUS_LED_GREEN = 1;
            public const int _STATUS_LED_BLUE = 2;
            public const int _STATUS_LED_RED = 4;
            public int GetStatus_Led(int nAxis) { return m_pnStatus[3, nAxis]; }

            public void SetLed_Red(int nAxis, bool bOn)
            {
                SetCmd_Flag_Led_Red(nAxis, bOn); // Led 를 유지하기 위해...
                byte[] pbyteData = new byte[1];
                int i = 0;
                int nValue = ((GetCmd_Flag_Led_Green(nAxis) == true) ? 0x01 : 0);
                nValue |= ((GetCmd_Flag_Led_Blue(nAxis) == true) ? 0x02 : 0);
                nValue |= ((bOn == true) ? 0x04 : 0);
                pbyteData[i++] = (byte)(nValue & 0xff);
                WriteRam(nAxis, _ADDRESS_LED_CONTROL, pbyteData);
                pbyteData = null;
            }
            public void SetLed_Blue(int nAxis, bool bOn)
            {
                SetCmd_Flag_Led_Blue(nAxis, bOn); // Led 를 유지하기 위해...
                byte[] pbyteData = new byte[1];
                int i = 0;
                int nValue = ((GetCmd_Flag_Led_Green(nAxis) == true) ? 0x01 : 0);
                nValue |= ((bOn == true) ? 0x02 : 0);
                nValue |= ((GetCmd_Flag_Led_Red(nAxis) == true) ? 0x04 : 0);
                pbyteData[i++] = (byte)(nValue & 0xff);
                WriteRam(nAxis, _ADDRESS_LED_CONTROL, pbyteData);
                pbyteData = null;
            }
            public void SetLed_Green(int nAxis, bool bOn)
            {
                SetCmd_Flag_Led_Green(nAxis, bOn); // Led 를 유지하기 위해...
                byte[] pbyteData = new byte[1];
                int i = 0;
                int nValue = ((bOn == true) ? 0x01 : 0);
                nValue |= ((GetCmd_Flag_Led_Blue(nAxis) == true) ? 0x02 : 0);
                nValue |= ((GetCmd_Flag_Led_Red(nAxis) == true) ? 0x04 : 0);
                pbyteData[i++] = (byte)(nValue & 0xff);
                WriteRam(nAxis, _ADDRESS_LED_CONTROL, pbyteData);
                pbyteData = null;
            }
            #endregion LED

            #region 상태체크 모음 - 접속상태, 서보/드라이버 On/Off 상태 등...
            public bool IsLed_Green(int nAxis) { return (((m_pnStatus[3, nAxis] & _STATUS_LED_GREEN) != 0) ? true : false); }
            public bool IsLed_Blue(int nAxis) { return (((m_pnStatus[3, nAxis] & _STATUS_LED_BLUE) != 0) ? true : false); }
            public bool IsLed_Red(int nAxis) { return (((m_pnStatus[3, nAxis] & _STATUS_LED_RED) != 0) ? true : false); }
            public void CheckLed(int nAxis, out bool bGreen, out bool bBlue, out bool bRed) { bGreen = IsLed_Green(nAxis); bBlue = IsLed_Blue(nAxis); bRed = IsLed_Red(nAxis); }
            public bool IsDrv(int nAxis) { return (((m_pnStatus[2, nAxis] & 0x40) != 0) ? true : false); }
            public bool IsSrv(int nAxis) { return (((m_pnStatus[2, nAxis] & 0x20) != 0) ? true : false); }
            public bool IsDrvAll(int nMax) // 연결된 모터중 하나라도 false 면 false 를 리턴
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
            public bool IsSrvAll(int nMax) // 연결된 모터중 하나라도 false 면 false 를 리턴
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
            public bool IsDrvSrvAll(int nMax) // 연결된 모터중 하나라도 false 면 false 를 리턴
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
            public bool IsDrvSrv(int nAxis) // 연결된 모터가 Servo 나 Drv 중 하나라도 false 면 false 를 리턴
            {
                bool bRet = true;
                if (IsConnect() == true)
                {
                    if ((IsDrv(nAxis) == false) || (IsSrv(nAxis) == false))
                        bRet = false;
                }
                return bRet;
            }

            #region Counter - 통신에 관한 카운터 ( 보낸것, 받은 이벤트, Push 횟수, Pop 횟수 )
            private int m_nCntTick_Receive_Event_All = 0;
            private int m_nCntTick_Receive_Event_All_Back = 0;
            private int m_nCntTick_Receive_PushBuffer = 0;
            private int m_nCntTick_Receive_GetBuffer = 0;
            public int GetCounter_Tick_Reveive_Event(int nAxis) { return m_nCntTick_Receive_Event_All; }
            public int GetCounter_Tick_Reveive_Push() { return m_nCntTick_Receive_PushBuffer; }
            public int GetCounter_Tick_Reveive_Interpret() { return m_nCntTick_Receive_GetBuffer; }

            private int[] m_pnCntTick_Send = new int[_MOTOR_MAX];
            private int[] m_pnCntTick_Receive = new int[_MOTOR_MAX];
            private int[] m_pnCntTick_Receive_Back = new int[_MOTOR_MAX];
            private long Tick_GetTimer(int nAxis) { return m_aCTmr[nAxis].Get(); }
            private void Tick_Send(int nAxis) { if (IsConnect() == false) return; m_pnCntTick_Send[nAxis]++; m_aCTmr[nAxis].Set(); m_pnCntTick_Receive_Back[nAxis] = m_pnCntTick_Receive[nAxis]; }
            private void Tick_Receive(int nAxis) { m_pnCntTick_Receive[nAxis]++; m_aCTmr[nAxis].Set(); }
            public int GetCounter_Tick_Send(int nAxis) { return m_pnCntTick_Send[nAxis]; }
            public int GetCounter_Tick_Receive(int nAxis) { return m_pnCntTick_Receive[nAxis]; }
            public void ResetCounter()
            {
                m_nCntTick_Receive_Event_All = m_nCntTick_Receive_Event_All_Back = m_nCntTick_Receive_PushBuffer = m_nCntTick_Receive_GetBuffer = 0;
                Array.Clear(m_pnCntTick_Send, 0, m_pnCntTick_Send.Length);
                Array.Clear(m_pnCntTick_Receive, 0, m_pnCntTick_Receive.Length);
                //Array.Clear(m_pnCntTick_Receive_Back, 0, m_pnCntTick_Receive_Back.Length);
                for (int i = 0; i < 256; i++) m_pnCntTick_Receive_Back[i] = -1;//
            }
            // 틱을 이용한 ...
            // 통신 장애 시간 체크
            public long GetCounter_Timer(int nAxis) { return Tick_GetTimer(nAxis); }
            //public bool IsReceived(int nAxis) { return ((m_pnCntTick_Receive[nAxis] != m_pnCntTick_Receive_Back[nAxis]) || (m_aCTmr[nAxis].Get() > 100)) ? true : false; }
            public bool IsReceived(int nAxis) { return (m_pnCntTick_Receive[nAxis] != m_pnCntTick_Receive_Back[nAxis]) ? true : false; }
            public bool IsReceived_1Packet() { return (m_nCntTick_Receive_Event_All != m_nCntTick_Receive_Event_All_Back) ? true : false; }
            public bool WaitReceive_1Packet(long lWaitTimer)
            {
                CTimer tmrReceive = new CTimer();
                tmrReceive.Set();
                bool bError = true;
                bool bOver = false;
                while ((IsConnect() == true) && (m_bClassEnd == false) && (bOver == false))
                {
                    if (IsReceived_1Packet() == true)
                    {
                        bError = false;
                        break;
                    }
                    if (lWaitTimer > 0)
                    {
                        if (tmrReceive.Get() >= lWaitTimer)
                        {
                            bOver = true;
                        }
                    }
                    Thread.Sleep(1);
                }
                return ((bError == false) ? true : false);                
            }
            public bool WaitReceive(int nAxis, long lWaitTimer)  // true : Receive Ok, false : Fail
            {
                CTimer tmrReceive = new CTimer();
                tmrReceive.Set();
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
                        if (tmrReceive.Get() >= lWaitTimer)
                        {
                            bOver = true;
                        }
                    }
                    Thread.Sleep(1);
                    //Application.DoEvents();
                }
                return ((bError == false) ? true : false);
            }
            #endregion

            #region 버퍼관리 - 변수 - 함수
            private const int _CNT_BUFFER = 100;
            private int m_nCnt_ReceivedData = 0;
            //private bool m_bReceived_AllData = true; // 데이타를 전부 받지 못한 경우 이게 false가 되어 다음 데이터를 받기를 기다림
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

            #region 패킷명령
            #region for ZigBee
            private bool m_bZigBee = false;
            // 지그비를 사용할지 여부를 결정, 이게 셋 되면 내부에 저장된 지그비 어드레스로 데이터를 전송한다.
            public void SetZigBee(bool bUse) { m_bZigBee = bUse; }
            // 지그비 사용여부를 확인(true 시에 셋 되어 있음, 기본은 false)
            public bool IsZigBee() { return m_bZigBee; }

            private int m_nZigBee_Address = 0x1111;
            // 지그비 어드레스를 저장 - 지그비 셋(SetZigBee)을 한 경우 이 어드레스로 데이타를 강제 전송하게됨
            public void SetZigBee_Address(int nAddress) { m_nZigBee_Address = nAddress; }
            // 현재 저장된 지그비 어드레스를 리턴
            public int GetZigBee_Address() { return m_nZigBee_Address; }

            public void SendPacket_with_zigbee(int nAddress, byte[] buffer, int nLength)
            {
                int nLengthSum = 12 + nLength;
                int _ZIGBEE_CHECKSUM1 = 5;
                int _ZIGBEE_CHECKSUM2 = _ZIGBEE_CHECKSUM1 + 1;
                // ff ff [size] fc 25 {checksum1 checksum2} f3 address1 address2 ack nLength ............. 실제패킷 ..........
                byte[] pbyteBuffer = new byte[12 + nLength];
                int i = 0;
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xff;
                // SIZE = 12 ( decimal) + Length
                pbyteBuffer[i++] = (byte)(nLengthSum & 0xff);

                pbyteBuffer[i++] = 0xfc;
                pbyteBuffer[i++] = 0x25;

                // CheckSum
                i += 2;

                pbyteBuffer[i++] = 0xf3;
                // Address ( 주의! Big Endian )
                pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                pbyteBuffer[i++] = (byte)(nAddress & 0xff);

                // Ack (0) -> 1 인경우 리턴을 받음.
                pbyteBuffer[i++] = (byte)0;
                pbyteBuffer[i++] = (byte)(nLength & 0xff);

                // 실 데이타
                Array.Copy(buffer, 0, pbyteBuffer, i, nLength);

                // Make CheckSum
                pbyteBuffer[_ZIGBEE_CHECKSUM1] = (byte)(pbyteBuffer[2] ^ pbyteBuffer[3] ^ pbyteBuffer[4]);
                for (int j = _ZIGBEE_CHECKSUM2 + 1; j < nLengthSum; j++)
                {
                    pbyteBuffer[_ZIGBEE_CHECKSUM1] ^= pbyteBuffer[j];
                }
                pbyteBuffer[_ZIGBEE_CHECKSUM1] = (byte)(pbyteBuffer[_ZIGBEE_CHECKSUM1] & 0xfe);
                pbyteBuffer[_ZIGBEE_CHECKSUM2] = (byte)(~pbyteBuffer[_ZIGBEE_CHECKSUM1] & 0xfe);

                m_SerialPort.Write(pbyteBuffer, 0, nLengthSum);
            }
            #endregion for ZigBee

            public void SendPacket(byte[] buffer, int nLength)
            {
                if (IsConnect() == true)
                {
                    try
                    {
                        if (IsSocket() == true) m_CSocket.Send(buffer);
                        else if (IsZigBee() == true) SendPacket_with_zigbee(GetZigBee_Address(), buffer, nLength);
                        else m_SerialPort.Write(buffer, 0, nLength);
                    }
                    catch (Exception ex)
                    {
                        Ems();
                        Ojw.CMessage.Write_Error("Serial Connection Error - " + ex.ToString());
                    }
                }
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

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);

                SendPacket(pbyteBuffer, nDefaultSize + i);

                pbyteBuffer = null;
            }

            #region Checksum 관련
            private void MakeCheckSum(int nAllPacketLength, byte[] buffer)
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
            // 0 이상 - 이상 없음(양의 정수는 패킷의 유효 사이즈 - 이 이상 넘는 사이즈는 버리면 될듯)
            // -1 = 헤더이상
            // -2 = 사이즈 미달
            // -3 = 체크섬 이상
            private int CheckCompletePacket(string strPacket, out bool bSizeOver)
            {
                int nPacketSize = 7; // 0xff 를 포함한 갯수
                bool bOver = false;
                int nSize = 0;
                // 기본 패킷사이즈 확인
                if (strPacket.Length >= nPacketSize)
                {
                    // 헤더의 확인
                    if ((strPacket[_HEADER1] == 0xff) && (strPacket[_HEADER2] == 0xff))
                    {
                        // 해당 사이즈 만큼의 데이타가 있는지 확인
                        if (strPacket[_SIZE] <= strPacket.Length)
                        {
                            if (strPacket[_SIZE] != strPacket.Length) bOver = true; // 사이즈 초과 -> 에러는 아직 아님
                            // 체크섬 이상 체크
                            nSize = (int)strPacket[_SIZE];
                            byte[] pbyteBuffer = new byte[256];
                            for (int i = 0; i < nSize; i++)
                            {
                                pbyteBuffer[i] = (byte)strPacket[i];
                            }
                            if (IsValidCheckSum(pbyteBuffer) == false) nSize = -3;
                            pbyteBuffer = null;
                        }
                        else nSize = -2;
                    }
                    else nSize = -1;
                }

                // out
                bSizeOver = bOver;
                return nSize;
            }
            private int CheckCompletePacket(byte[] pbytePacket, out bool bSizeOver)
            {
                int nPacketSize = 7; // 0xff 를 포함한 갯수
                bool bOver = false;
                int nSize = 0;
                // 기본 패킷사이즈 확인
                if (pbytePacket.Length >= nPacketSize)
                {
                    // 헤더의 확인
                    if ((pbytePacket[_HEADER1] == 0xff) && (pbytePacket[_HEADER2] == 0xff))
                    {
                        // 해당 사이즈 만큼의 데이타가 있는지 확인
                        if (pbytePacket[_SIZE] <= pbytePacket.Length)
                        {
                            if (pbytePacket[_SIZE] != pbytePacket.Length) bOver = true; // 사이즈 초과 -> 에러는 아직 아님
                            // 체크섬 이상 체크
                            if (IsValidCheckSum(pbytePacket) == false) nSize = -3;
                            else nSize = (int)pbytePacket[_SIZE]; // 정상
                        }
                        else nSize = -2;
                    }
                    else nSize = -1;
                }

                // out
                bSizeOver = bOver;
                return nSize;
            }
            // 사이즈, 헤더, 기타 다른 상태적 이상은 사전에 이미 확인 되었다고 가정
            private bool IsValidCheckSum(byte[] buffer)
            {
                try
                {
                    int nAllPacketLength = buffer.Length;
                    int nHeadSize = _CHECKSUM2 + 1;

                    byte byCheckSum1 = (byte)((int)buffer[_SIZE] ^ (int)buffer[_ID] ^ (int)buffer[_CMD]);
                    for (int j = 0; j < nAllPacketLength - nHeadSize; j++)
                        byCheckSum1 ^= buffer[nHeadSize + j];
                    byCheckSum1 = (byte)(byCheckSum1 & 0xfe);
                    byte byCheckSum2 = (byte)(~byCheckSum1 & 0xfe);

                    if ((byCheckSum1 == buffer[_CHECKSUM1]) && (byCheckSum2 == buffer[_CHECKSUM2])) return true;

                    return false;
                }
                catch
                {
                    return false;
                }
            }
            #endregion Checksum 관련
            #endregion 패킷명령
            #endregion 상태체크 모음 - 접속상태, 서보/드라이버 On/Off 상태 등...

            #endregion 제어변수 및 함수

            #region Event
            private byte[] m_pbyteReceiveData = new byte[1];

            public bool IsValid_Rom(int nAxis, int nAddress) { return ((m_pnRom == null) ? false : ((nAddress >= _SIZE_ROM) ? false : true)); }
            public int GetData_Rom(int nAxis, int nAddress) { return m_pnRom[nAxis, nAddress]; } // 에러처리가 필요하다면 IsValid_Ram() 활용
            public int[,] GetData_Rom() { return m_pnRom; }
            public bool IsValid_Ram(int nAxis, int nAddress) { return ((m_pnRam == null) ? false : ((nAddress >= _SIZE_RAM) ? false : true)); }
            public int GetData_Ram(int nAxis, int nAddress) { return m_pnRam[nAxis, nAddress]; } // 에러처리가 필요하다면 IsValid_Ram() 활용
            public int[,] GetData_Ram() { return m_pnRam; }
            private void PacketDecoder1(int nAxis)
            {
                /////////// 해석 ////////////////
                // Srv / Drv
                SetStatus_DrvSrv(nAxis, GetData_Ram(nAxis, 52));
                // 위치
                //SetPos(nAxis, GetData_Ram(nAxis, 46));
                // Signed Data
                byte[] byteData = new byte[2];
                byteData[0] = (byte)(GetData_Ram(nAxis, _ADDRESS_CALIBRATED_POSITION) & 0xff);
                byteData[1] = (byte)(GetData_Ram(nAxis, _ADDRESS_CALIBRATED_POSITION + 1) & 0xff);
                // 0000 0000  0000 0000
                short sData = 0;
                sData = BitConverter.ToInt16(byteData, 0);
                //sData = (short)(((byteData[0] & 0x0f) << 8) | (byteData[1] << 0) | ((byteData[0] & 0x10) << (3 + 8)) | ((byteData[0] & 0x10) << (2 + 8)) | ((byteData[0] & 0x10) << (1 + 8)));
                SetPos(nAxis, (int)sData);
                byteData = null;
                /////////////////////////////////

                // 받고나서 처리 후 Tick 을 Set 한다.
                Tick_Receive(nAxis);
            }
            private byte m_byCheckSum = 0;
            private byte m_byCheckSum2 = 0;
            private int m_nRxIndex = 255;//0;
            private int m_nRxDataCount = 0;
            private int m_nRxId = 0;
            private int m_nRxCmd = 0;
            private int m_nRxAddress = 0;
            private int m_nRxAddressLen = 0;
            private int m_nRxCheckSum = 0;
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
            private int[,] m_pnRam = new int[256, _SIZE_RAM];
            private int[,] m_pnRom = new int[256, _SIZE_ROM];
            private int[] m_pnSensor = new int[_SIZE_SENSOR];
            private int[] m_pnMpsuRam = new int[_SIZE_MPSU_RAM];
            private int[] m_pnMpsuRom = new int[_SIZE_MPSU_ROM];
            private int[] m_pnHeadRam = new int[_SIZE_HEAD_RAM];
            private int[] m_pnHeadRom = new int[_SIZE_HEAD_ROM];
            public byte GetLastPacket() { return m_byteLastPacket; }
            private byte m_byteLastPacket = 0;
            private void ReceiveDataCallback()		/* Callback 함수 */
            {
                while (((IsConnect() == true) || (IsSocket() == true)) && (m_bClassEnd == false))
                {
                    try
                    {
                        if (IsAutoReturn() == true)
                        {
                            int nRequestAxis = m_nCmdAxis - 1;
                            if (nRequestAxis < 0) nRequestAxis = m_nMotor_Max - 1;
                            bool bReceived = IsReceived(nRequestAxis);
                            if (bReceived == true)// || (nTick++ > 10000))
                            {
                                //Thread.Sleep(10); // 통신리시브 이후 바로 전송명령을 주지 않기 위해 딜레이를 줌 ( 이러면 통신 유실율이 낮아진다. )
                                //OjwTimer.WaitTimer(1);
                                ReadMot(m_nCmdAxis++, 0, _SIZE_RAM);//ReadMot(nCmdAxis++, _ADDRESS_TORQUE_CONTROL, 8);
                                if (m_nCmdAxis >= m_nMotor_Max) m_nCmdAxis = 0;

                                //nTick = 0;
                            }
                        }

                        int i;
                        byte RxData;
                        int nPacketSize = 0;

                        int nLength = (IsSocket() == true) ? m_CSocket.GetBuffer_Length() : GetBuffer_Length();
                        
                        if (nLength > 0)
                        {
                            m_nCntTick_Receive_Event_All++;

                            // 통신이 이루어 진 것이므로 다음 명령을 받을 수 있도록 busy 를 먼저 클리어 한다.
                            m_bBusy = false;
                            //int nFirst = m_nReceiveData_Length;
                            //m_nReceiveData_Length += nLength;
                            //Array.Resize<byte>(ref m_pbyteReceiveData, m_nReceiveData_Length);

                            //for (int i = 0; i < nLength; i++) m_pbyteReceiveData[nFirst + i] = (byte)(m_SerialPort.ReadByte() & 0xff);
                            for (i = 0; i < nLength; i++)
                            {
                                if (IsSocket() == true)
                                    RxData = (byte)(m_CSocket.GetByte() & 0xff);
                                else
                                    RxData = (byte)(m_SerialPort.ReadByte() & 0xff);
                                m_byteLastPacket = RxData; // LastPacket
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
                                            continue;
                                        }
                                        break;
                                    case 1: //Get Data Size(Packet Size)
                                        if (RxData < 9)
                                        { // 패킷 최소사이즈 ( 7(기본사이즈) + 2(기본정보사이즈) )
                                            m_nRxIndex = 0xFF;
                                            continue;
                                        }
                                        nPacketSize = RxData;
                                        m_byCheckSum = RxData;
                                        break;
                                    case 2: //Get ID		
                                        m_nRxId = RxData;
                                        m_byCheckSum ^= RxData;
                                        break;
                                    case 3: // Get Cmd
                                        if (
                                            (RxData != _CMD_ROM) &&	// EEP_READ
                                            (RxData != _CMD_RAM) &&	// RAM_READ
                                            (RxData != _CMD_MPSU_ROM) &&	// MPSU_EEP_READ
                                            (RxData != _CMD_MPSU_RAM) &&	// MPSU_RAM_READ
                                            (RxData != _CMD_MPSU_ROM2) &&	// MPSU_EEP_READ
                                            (RxData != _CMD_MPSU_RAM2) &&	// MPSU_RAM_READ => MPSU 명령이 수정된것 같음. 나중에 철희에게 확인해 볼것. 기존에는 0x54 가 반송되도록 되어 있음.
                                            (RxData != _CMD_HEAD_ROM) &&	// MPSU_EEP_READ
                                            (RxData != _CMD_HEAD_RAM) &&	// MPSU_RAM_READ
                                            (RxData != _CMD_OPSU_SENSOR) // OPSU Sensor READ
                                            )
                                        {
                                            m_nRxIndex = 0xFF;
                                            continue;
                                        }
                                        m_nRxCmd = RxData;
                                        m_byCheckSum ^= RxData;
                                        break;
                                    case 4: 			// checksum 1
                                        m_nRxCheckSum = RxData;// & 0xFE;           
                                        break;
                                    case 5: 			// checksum 1
                                        m_byCheckSum2 = RxData;// & 0xFE;
                                        break;
                                    case 6: // Address
                                        m_nRxAddress = RxData;
                                        m_byCheckSum ^= RxData;
                                        break;
                                    case 7: //Get Data Size(Data Size)
                                        m_nRxAddressLen = RxData + 2; // Status Error, Status Details            
                                        m_byCheckSum ^= RxData;
                                        break;
                                    case 8: //Get Data				
                                        //gSensor_Rx.Data[(int)(m_nRxDataCount & 0xff)] = RxData;
                                        if (m_nRxCmd == _CMD_OPSU_SENSOR) // OPSU Sensor
                                        {
                                            if (m_nRxDataCount == m_nRxAddressLen - 2) SetStatus1_Sensor(RxData);
                                            else if (m_nRxDataCount == m_nRxAddressLen - 1) SetStatus2_Sensor(RxData);
                                            else
                                            {
                                                if (m_nRxDataCount < m_nRxAddressLen) m_pnSensor[(int)((m_nRxAddress + m_nRxDataCount) & 0xff)] = RxData;
                                                else { m_nRxIndex = 0xFF; continue; }
                                            }

                                            m_nRxDataCount++; // 번지 카운터 증가            				
                                            m_byCheckSum ^= RxData;

                                            if ((m_nRxDataCount < m_nRxAddressLen) && (m_nRxDataCount < _SIZE_SENSOR)) m_nRxIndex--;		//Fix Get Data Routine
                                            else
                                            {
                                                // CheckSum1
                                                if ((m_byCheckSum & 0xFE) != m_nRxCheckSum) { m_nRxIndex = 0xFF; continue; }
                                                else if ((~m_byCheckSum & 0xFE) != m_byCheckSum2) { m_nRxIndex = 0xFF; continue; }
                                                else { Tick_Receive_Sensor(); m_nRxIndex = 0xFF; }
                                            }
                                        }
                                        else if ((m_nRxCmd == _CMD_MPSU_RAM) || (m_nRxCmd == _CMD_MPSU_ROM)) // MPSU
                                        {
                                            int nAxis = GetAxis_By_ID(m_nRxId);
                                            if (m_nRxDataCount == m_nRxAddressLen - 2) SetStatus1_Mpsu(RxData);
                                            else if (m_nRxDataCount == m_nRxAddressLen - 1) SetStatus2_Mpsu(RxData);
                                            else
                                            {
                                                if ((m_nRxCmd == _CMD_MPSU_ROM) && (m_nRxDataCount < m_nRxAddressLen)) m_pnMpsuRom[(int)((m_nRxAddress + m_nRxDataCount) & 0xff)] = RxData;
                                                else if ((m_nRxCmd == _CMD_MPSU_RAM) && (m_nRxDataCount < m_nRxAddressLen)) m_pnMpsuRam[(int)((m_nRxAddress + m_nRxDataCount) & 0xff)] = RxData;
                                                else { m_nRxIndex = 0xFF; continue; }
                                            }

                                            m_nRxDataCount++; // 번지 카운터 증가            				
                                            m_byCheckSum ^= RxData;

                                            if ((m_nRxDataCount < m_nRxAddressLen) && (m_nRxDataCount < ((m_nRxCmd == _CMD_MPSU_RAM) ? _SIZE_MPSU_RAM + 2 : _SIZE_MPSU_ROM + 2)))
                                            {
                                                m_nRxIndex--;		//Fix Get Data Routine
                                            }
                                            else
                                            {
                                                // CheckSum1
                                                if ((m_byCheckSum & 0xFE) != m_nRxCheckSum)
                                                {
                                                    m_nRxIndex = 0xFF;
                                                    continue;
                                                }
                                                else if ((~m_byCheckSum & 0xFE) != m_byCheckSum2)
                                                {
                                                    m_nRxIndex = 0xFF;
                                                    continue;
                                                }
                                                else
                                                {
                                                    Tick_Receive_Mpsu();
                                                    m_nRxIndex = 0xFF;
                                                }
                                            }
                                        }
                                        else if ((m_nRxCmd == _CMD_HEAD_RAM) || (m_nRxCmd == _CMD_HEAD_ROM)) // HEAD LED
                                        {
                                            int nAxis = GetAxis_By_ID(m_nRxId);
                                            if (m_nRxDataCount == m_nRxAddressLen - 2) SetStatus1_Head(RxData);
                                            else if (m_nRxDataCount == m_nRxAddressLen - 1) SetStatus2_Head(RxData);
                                            else
                                            {
                                                if ((m_nRxCmd == _CMD_HEAD_ROM) && (m_nRxDataCount < m_nRxAddressLen)) m_pnHeadRom[(int)((m_nRxAddress + m_nRxDataCount) & 0xff)] = RxData;
                                                else if ((m_nRxCmd == _CMD_HEAD_RAM) && (m_nRxDataCount < m_nRxAddressLen)) m_pnHeadRam[(int)((m_nRxAddress + m_nRxDataCount) & 0xff)] = RxData;
                                                else { m_nRxIndex = 0xFF; continue; }
                                            }

                                            m_nRxDataCount++; // 번지 카운터 증가            				
                                            m_byCheckSum ^= RxData;

                                            if ((m_nRxDataCount < m_nRxAddressLen) && (m_nRxDataCount < ((m_nRxCmd == _CMD_HEAD_RAM) ? _SIZE_HEAD_RAM + 2 : _SIZE_HEAD_ROM + 2)))
                                            {
                                                m_nRxIndex--;		//Fix Get Data Routine
                                            }
                                            else
                                            {
                                                // CheckSum1
                                                if ((m_byCheckSum & 0xFE) != m_nRxCheckSum)
                                                {
                                                    m_nRxIndex = 0xFF;
                                                    continue;
                                                }
                                                else if ((~m_byCheckSum & 0xFE) != m_byCheckSum2)
                                                {
                                                    m_nRxIndex = 0xFF;
                                                    continue;
                                                }
                                                else
                                                {
                                                    Tick_Receive_Head();
                                                    m_nRxIndex = 0xFF;
                                                }
                                            }
                                        }
                                        else // Servo Motor
                                        {
                                            int nAxis = GetAxis_By_ID(m_nRxId);
                                            if (m_nRxDataCount == m_nRxAddressLen - 2) SetStatus1(nAxis, RxData);
                                            else if (m_nRxDataCount == m_nRxAddressLen - 1) SetStatus2(nAxis, RxData);
                                            else
                                            {
                                                if ((m_nRxCmd == _CMD_ROM) && (m_nRxDataCount < m_nRxAddressLen)) m_pnRom[nAxis, (int)((m_nRxAddress + m_nRxDataCount) & 0xff)] = RxData;
                                                else if ((m_nRxCmd == _CMD_RAM) && (m_nRxDataCount < m_nRxAddressLen)) m_pnRam[nAxis, (int)((m_nRxAddress + m_nRxDataCount) & 0xff)] = RxData;
                                                else { m_nRxIndex = 0xFF; continue; }
                                            }

                                            m_nRxDataCount++; // 번지 카운터 증가            				
                                            m_byCheckSum ^= RxData;

                                            if ((m_nRxDataCount < m_nRxAddressLen) && (m_nRxDataCount < ((m_nRxCmd == _CMD_RAM) ? _SIZE_RAM + 2 : _SIZE_ROM + 2)))
                                            {
                                                m_nRxIndex--;		//Fix Get Data Routine
                                            }
                                            else
                                            {
                                                // CheckSum1
                                                if ((m_byCheckSum & 0xFE) != m_nRxCheckSum)
                                                {
                                                    m_nRxIndex = 0xFF;
                                                    continue;
                                                }
                                                else if ((~m_byCheckSum & 0xFE) != m_byCheckSum2)
                                                {
                                                    m_nRxIndex = 0xFF;
                                                    continue;
                                                }
                                                else
                                                {
                                                    PacketDecoder1(nAxis);
                                                    m_nRxIndex = 0xFF;
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        m_nRxIndex = 0xFF;
                                        break;
                                }
                                if (m_nRxIndex != 0xFF) m_nRxIndex++;
                            }
                        }
                    }
                    catch
                    {
                    }
                    Thread.Sleep(10);
                }

            }

            private void serialPort1_DataReceived() { ReceiveDataCallback(); }
            #endregion Event

            #region IsConnect() - 접속상태 체크
            public bool IsConnect() { return m_SerialPort.IsOpen; }
            #endregion

            #region Connect
            /////////////////////////////////////////////////
            // Parity - 0 : None, 1 : Odd, 2 : Even, 3 : Mark, 4 : Space
            // StopBit - 0 : None, 1 : One, 2 : Two, 3 : OnePointFive
            public bool Connect(int nPort, int nBaudRate)//(int nPort, int nBaudRate, int nParity, int nDataBits, int nStopBits)
            {
                if (IsConnect() == false)
                {
                    ResetCounter(); // 카운터 클리어
                    ResetCounter_Sensor(); // 카운터 클리어
                    ResetCounter_Mpsu(); // 카운터 클리어
                    ResetCounter_Head(); // 카운터 클리어

                    if (m_bEventInit == false)
                    {
#if _ENABLE_THREAD
                    
#else
                        m_SerialPort.DataReceived += new SerialDataReceivedEventHandler(EventDataReceived);
#endif
                        m_bEventInit = true;
                    }

                    //String strPort = "COM" + CConvert.IntToStr(nPort);
                    //m_SerialPort = new SerialPort(strPort, nBaudRate, Parity.None, 8, StopBits.One);
                    m_SerialPort.PortName = "COM" + nPort.ToString();
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

                        //for (int i = 0; i < 256; i++) m_pnCntTick_Receive_Back[i] = m_pnCntTick_Receive[i];// -1;
#if _ENABLE_THREAD
                    if (IsConnect() == true)
                    {
                        // 통신 부하용 타이머 리셋
                        //for (int i = 0; i < m_nMotor_Max; i++) OjwTimer.TimerSet(i);
                        
                        Reader = new Thread(new ThreadStart(serialPort1_DataReceived));
                        Reader.Start();

                        //SetAutoReturn(true);
                    }
#endif
                    }
                    catch
                    {
                        //m_bConnect = false;
                    }
                }
                return IsConnect();
            }
            #endregion

            #region DisConnect() - 접속 해제 - Thread 정지
            public void DisConnect()
            {
                if (IsConnect() == true)
                {
                    m_SerialPort.Close();
                }
            }
            #endregion

            private CSocket m_CSocket = null;
            public void SetSocket(CSocket CSock) { m_CSocket = CSock; }
            public bool IsSocket() { return (m_CSocket == null) ? false : m_CSocket.IsConnect(); }

            private bool m_bBusy = false;
            // 통신의 혼선이 일지 않게 끔 외부적으로 Busy 상태인지 아닌지를 확인하는 함수
            public bool IsBusy() { return m_bBusy; }
            private bool[] m_pbRequest = new bool[_MOTOR_MAX];
            // nCmd 번지부터 nDataByteSize 개의 데이터 가져옴
            // ex) 40번지부터 8개의 데이터 가져옴 => ReadMot(nAxis, 40, 8)
            public void ReadRam(int nAxis, int nAddress, int nLength) { ReadMot(nAxis, nAddress, nLength); }
            public void ReadMot(int nAxis, int nCmd, int nDataByteSize) 
            {
                if (IsConnect() == false) return;// false;

                m_bBusy = true; // 통신 요청

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

                //OjwTimer.TimerSet(m_nMotor_Max);
                //m_pnCntTick_Send[nAxis]++; // Send 카운터 증가
                //             OjwTimer.TimerSet(nAxis);
                //             int nTestSize = 0;
                //             // 종료, 타임오버(1초) 가 아니라면 데이타가 올 때까지 대기
                //             while ((m_bClassEnd == false) && (OjwTimer.Timer(nAxis) < 1000))
                //             {
                //                 nTestSize = GetBuffer_Size();
                //                 if (nTestSize > 0) // 통신이 발생하면 ...
                //                 {
                //                     string strBuffer;
                //                     for (i = 0; i < nTestSize; i++)
                //                     {
                //                         strBuffer = GetBuffer();
                //                         CommInterPreter(strBuffer);
                //                     }
                //                     break;
                //                 }
                //             }
                // 
                //             pbyteBuffer = null;
                //             return (nTestSize > 0) ? true : false;
            }
            public void ReadRom(int nAxis, int nCmd, int nDataByteSize)
            {
                if (IsConnect() == false) return;// false;

                m_bBusy = true; // 통신 요청

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
                pbyteBuffer[_CMD] = 0x02; // EEP Rom 영역을 읽어온다.

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

                //OjwTimer.TimerSet(m_nMotor_Max);
                //m_pnCntTick_Send[nAxis]++; // Send 카운터 증가
                //             OjwTimer.TimerSet(nAxis);
                //             int nTestSize = 0;
                //             // 종료, 타임오버(1초) 가 아니라면 데이타가 올 때까지 대기
                //             while ((m_bClassEnd == false) && (OjwTimer.Timer(nAxis) < 1000))
                //             {
                //                 nTestSize = GetBuffer_Size();
                //                 if (nTestSize > 0) // 통신이 발생하면 ...
                //                 {
                //                     string strBuffer;
                //                     for (i = 0; i < nTestSize; i++)
                //                     {
                //                         strBuffer = GetBuffer();
                //                         CommInterPreter(strBuffer);
                //                     }
                //                     break;
                //                 }
                //             }
                // 
                //             pbyteBuffer = null;
                //             return (nTestSize > 0) ? true : false;
            }

            #region Read Position
            public void ReadMot_Angle(int nAxis)
            {
                ReadMot(nAxis, _ADDRESS_TORQUE_CONTROL, 8);
                //bool bOk = WaitReceive(nAxis, 40);
                //if (bOk == false)
                //{
                    
                //}
                //return GetPos_Angle(nAxis);
            }
            #endregion Read Position

            #region MPSU
            public void Mpsu_Write_Rom(int nMpsuID, int nStartAddress, byte byteData)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

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
                //pbyteBuffer[nDefaultSize + i++] = (byte)CConvert.BoolToInt(bDebugingMode);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void Mpsu_Write_Rom(int nMpsuID, int nStartAddress, byte[] pbyteData)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

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
                // Data
                for (int nData = 0; nData < nLength; nData++)
                {
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = (byte)(pbyteData[nData]);
                }

                ////////
                //pbyteBuffer[nDefaultSize + i++] = (byte)CConvert.BoolToInt(bDebugingMode);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void Mpsu_Write_Ram(int nMpsuID, int nStartAddress, byte byteData)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

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
                //pbyteBuffer[nDefaultSize + i++] = (byte)CConvert.BoolToInt(bDebugingMode);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void Mpsu_Write_Ram(int nMpsuID, int nStartAddress, byte[] pbyteData)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

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
                pbyteBuffer[nDefaultSize + i++] = (byte)(nLength & 0xff); // 한바이트만 보냄
                // Data
                for (int nData = 0; nData < nLength; nData++)
                {
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = (byte)(pbyteData[nData]);
                }

                ////////
                //pbyteBuffer[nDefaultSize + i++] = (byte)CConvert.BoolToInt(bDebugingMode);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }

            public void ReadMpsuRom(int nMpsuID, int nAddress, int nLength)
            {
                if (IsConnect() == false) return;// false;

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x12; // EEP Rom 영역을 읽어온다.

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize + i++] = (byte)(nAddress & 0xff);// 46번 레지스터 명령

                ////////
                pbyteBuffer[nDefaultSize + i++] = (byte)(nLength & 0xff);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void ReadMpsuRam(int nMpsuID, int nAddress, int nLength)
            {
                if (IsConnect() == false) return;// false;

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x14; // Ram 영역을 읽어온다.

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize + i++] = (byte)(nAddress & 0xff);

                ////////
                pbyteBuffer[nDefaultSize + i++] = (byte)(nLength & 0xff);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }

            public bool IsValid_Mpsu_Rom(int nAddress) { return ((m_pnMpsuRom == null) ? false : ((nAddress >= _SIZE_MPSU_ROM) ? false : true)); }
            public int GetData_Mpsu_Rom(int nAddress) { return m_pnMpsuRom[nAddress]; } // 에러처리가 필요하다면 IsValid_Mpsu() 활용
            public int[] GetData_Mpsu_Rom() { return m_pnMpsuRom; }
            public bool IsValid_Mpsu_Ram(int nAddress) { return ((m_pnMpsuRam == null) ? false : ((nAddress >= _SIZE_MPSU_RAM) ? false : true)); }
            public int GetData_Mpsu_Ram(int nAddress) { return m_pnMpsuRam[nAddress]; } // 에러처리가 필요하다면 IsValid_Mpsu() 활용
            public int[] GetData_Mpsu_Ram() { return m_pnMpsuRam; }

            private int m_nCntTick_Send_Mpsu = 0;
            private int m_nCntTick_Receive_Mpsu = 0;
            private int m_nCntTick_Receive_Mpsu_Back = 0;
            public long GetCounter_Timer_Mpsu() { return Tick_GetTimer(256); }
            private long Tick_GetTimer_Mpsu(int nAxis) { return m_aCTmr[256].Get(); }
            private void Tick_Send_Mpsu() { if (IsConnect() == false) return; m_nCntTick_Send_Mpsu++; m_aCTmr[256].Set(); m_nCntTick_Receive_Mpsu_Back = m_nCntTick_Receive_Mpsu; }
            private void Tick_Receive_Mpsu() { m_nCntTick_Receive_Mpsu++; m_aCTmr[256].Set(); }
            public void ResetCounter_Mpsu()
            {
                m_nCntTick_Send_Mpsu = 0;
                m_nCntTick_Receive_Mpsu = 0;
                m_nCntTick_Receive_Mpsu_Back = -1;
            }

            public bool IsReceived_Mpsu() { return (m_nCntTick_Receive_Mpsu != m_nCntTick_Receive_Mpsu_Back) ? true : false; }
            public bool WaitReceive_Mpsu(long lWaitTimer)  // true : Receive Ok, false : Fail
            {
                m_aCTmr[512 + 2].Set();
                bool bError = true;
                bool bOver = false;
                while ((IsConnect() == true) && (m_bClassEnd == false) && (bOver == false))
                {
                    if (IsReceived_Mpsu() == true)
                    {
                        bError = false;
                        break;
                    }
                    if (lWaitTimer > 0)
                    {
                        if (m_aCTmr[512 + 2].Get() >= lWaitTimer)
                        {
                            bOver = true;
                        }
                    }
                    Thread.Sleep(1);
                    //Application.DoEvents();
                }
                return ((bError == false) ? true : false);
            }
            #region Mpsu Status 함수
            private void SetStatus1_Mpsu(int nStatus) { m_pnStatus_Mpsu[0] = nStatus; }
            public int GetStatus1_Mpsu() { return m_pnStatus_Mpsu[0]; }
            private void SetStatus2_Mpsu(int nStatus) { m_pnStatus_Mpsu[1] = nStatus; }
            public int GetStatus2_Mpsu() { return m_pnStatus_Mpsu[1]; }

            // Error = false, 정상 = true;
            public bool GetStatusMessage_Mpsu(out String strStatus1, out String strStatus2)
            {
                int nStatus1 = GetStatus1_Mpsu();
                int nStatus2 = GetStatus2_Mpsu();
                strStatus1 = "";
                strStatus2 = "";
                if ((nStatus1 & 0x01) != 0) strStatus1 += "[Exceed Input Voltage Limit]";
                if ((nStatus1 & 0x02) != 0) strStatus1 += "[Exceed Temperature Limit]";
                if ((nStatus1 & 0x04) != 0) strStatus1 += "[Packet Error]";
                if ((nStatus1 & 0x08) != 0) strStatus1 += "[Servo Missing]";
                if ((nStatus1 & 0x10) != 0) strStatus1 += "[EEP Reg Distorted]";
                if ((nStatus1 & 0x20) != 0) strStatus1 += "[Servo Status Error]";
                if ((nStatus1 & 0x40) != 0) strStatus1 += "[Flash Data Error]";
                if ((nStatus1 & 0x80) != 0) strStatus1 += "[reserved]";

                if (nStatus2 == 0x01) strStatus2 += "[전압이 너무 낮음]";
                if (nStatus2 == 0x02) strStatus2 += "[전압이 너무 높음]";
                if (nStatus2 == 0x03) strStatus2 += "[온도가 너무 높음]";
                if (nStatus2 == 0x11) strStatus2 += "[상태 자동 확인에 의해 서보 레지스터 읽던 중 서보 응답 없음]";
                if (nStatus2 == 0x12) strStatus2 += "[Task 실행에 의해 서보 레지스터 읽던 중 서보 응답 없음]";
                if (nStatus2 == 0x21) strStatus2 += "[EEPROM의 모델명이 잘못 됨]";
                if (nStatus2 == 0x22) strStatus2 += "[EEPROM의 ID가 잘못 됨]";
                if (nStatus2 == 0x23) strStatus2 += "[EEPROM 데이터가 손상됨]";
                if (nStatus2 == 0x31) strStatus2 += "[서보 상태 에러]";
                if (nStatus2 == 0x33) strStatus2 += "[너무 많은 서보가 제어기에 연결됨]";
                if (nStatus2 == 0x41) strStatus2 += "[Zigbee Ack가 온전히 오지 않거나 Noise가 들어옴]";
                if (nStatus2 == 0x42) strStatus2 += "[Zigbee Ack에서 Check Sum Error]";
                if (nStatus2 == 0x43) strStatus2 += "[Zigbee Ack에서 Unknown Command]";
                if (nStatus2 == 0x44) strStatus2 += "[Zigbee Ack가 들어왔으나 ID가 0xFC가 아님]";
                if (nStatus2 == 0x45) strStatus2 += "[Zigbee Ack에서 사이즈가 너무 큰 패킷이 들어옴]";
                if (nStatus2 == 0x46) strStatus2 += "[Zigbee Ack에서 명령과 안 맞는 사이즈의 패킷이 들어옴]";
                if (nStatus2 == 0x47) strStatus2 += "[Zigbee Ack 가 오지 않음]";
                if (nStatus2 == 0x51) strStatus2 += "[Servo Ack 가 온전히 오지 않거나 Noise 가 들어옴]";
                if (nStatus2 == 0x52) strStatus2 += "[Servo Ack 에서 Check Sum Error]";
                if (nStatus2 == 0x53) strStatus2 += "[Servo Ack 에서 Unknown Command]";
                if (nStatus2 == 0x54) strStatus2 += "[Servo Ack 에서 Invalid ID 의 패킷이 들어옴]";
                if (nStatus2 == 0x56) strStatus2 += "[Servo Ack 에서 사이즈가 너무 큰 패킷이 들어옴]";
                if (nStatus2 == 0x57) strStatus2 += "[Servo Ack 에서 명령과 안 맞는 사이즈의 패킷이 들어옴]";
                if (nStatus2 == 0x58) strStatus2 += "[Servo Ack 를 받는 UART 버퍼가 꽉참]";
                if (nStatus2 == 0x59) strStatus2 += "[Servo 에 보낼 패킷을 저장하는 버퍼가 꽉참]";
                if (nStatus2 == 0x61) strStatus2 += "[PC측 패킷이 온전히 오지 않거나 Noise가 들어옴]";
                if (nStatus2 == 0x62) strStatus2 += "[PC측 패킷에서 Check Sum Error]";
                if (nStatus2 == 0x63) strStatus2 += "[PC측 패킷에서 Unknown Command]";
                if (nStatus2 == 0x64) strStatus2 += "[PC측 패킷에서 Invalid ID의 패킷이 들어옴]";
                if (nStatus2 == 0x66) strStatus2 += "[PC측 패킷에서 사이즈가 너무 큰 패킷이 들어옴]";
                if (nStatus2 == 0x67) strStatus2 += "[PC측 패킷에서 명령에 안 맞는 사이즈의 패킷이 들어옴]";
                if (nStatus2 == 0x68) strStatus2 += "[PC측 패킷을 받는 UART 버퍼가 꽉참]";
                if (nStatus2 == 0x71) strStatus2 += "[EEP/RAM 의 WRITE/READ 명령이 레지스터 범위를 벗어남]";
                if (nStatus2 == 0x72) strStatus2 += "[RAM_WRITE 에서 잘못된 값을 씀]";
                if (nStatus2 == 0x73) strStatus2 += "[RAM_WRITE 에서 Status 에 잘못된 값을 씀]";
                if (nStatus2 == 0x74) strStatus2 += "[CON_CHECK 에서 패킷에 잘못된 ID가 들어가 있음]";
                if (nStatus2 == 0x75) strStatus2 += "[PLAY_MOTION 에서 잘못된 모션 번호가 들어가 있음]";
                if (nStatus2 == 0x76) strStatus2 += "[PLAY_TASK 에서 잘못된 Instruction 이 들어가 있음]";
                if (nStatus2 == 0x77) strStatus2 += "[Remocon 에서 잘못된 Channel 이나 Length가 있어가 있음]";
                if (nStatus2 == 0x78) strStatus2 += "[ZIGBEE 에서 잘못된 Instruction 이 들어가 있음]";
                if (nStatus2 == 0x79) strStatus2 += "[PLAY_BUZZ 에서 잘못된 버저 번호가 들어가 있음]";
                if (nStatus2 == 0x81) strStatus2 += "[없는 Motion을 실행하라고 함]";
                if (nStatus2 == 0x82) strStatus2 += "[Motion 데이터에 문제 있음]";
                if (nStatus2 == 0x83) strStatus2 += "[Motion 데이터의 축 수와 현재 축 수가 다름]";
                if (nStatus2 == 0x84) strStatus2 += "[다음 프레임까지 시간이 음수인 프레임을 만남]";
                if (nStatus2 == 0x85) strStatus2 += "[너무 많은 Repeat 명령이 중첩되었음(최대 3개까지 가능)]";
                if (nStatus2 == 0x91) strStatus2 += "[Task 데이터에 문제 있음]";
                if (nStatus2 == 0x92) strStatus2 += "[계산식 처리 중 이상]";
                if (nStatus2 == 0x93) strStatus2 += "[프로그램 스택 오버플로우]";
                if (nStatus2 == 0x94) strStatus2 += "[MPSU Ram 에 Load 중 잘못된 레지스터 주소]";
                if (nStatus2 == 0x95) strStatus2 += "[MPSU Ram 에 Load 중 잘못된 레지스터 길이]";
                if (nStatus2 == 0x96) strStatus2 += "[Servo Ram 에 Load 중 잘못된 레지스터 주소]";
                if (nStatus2 == 0x97) strStatus2 += "[Servo Ram 에 Load 중 잘못된 레지스터 길이]";
                if (nStatus2 == 0x98) strStatus2 += "[Servo Ram 에 Load 중 잘못된 ID]";
                if (nStatus2 == 0x99) strStatus2 += "[MPSU Ram 에서 Read 중 잘못된 레지스터 길이]";
                if (nStatus2 == 0x9A) strStatus2 += "[Servo RAM 에서 Read 중 잘못된 레지스터 길이]";
                if (nStatus2 == 0x9B) strStatus2 += "[Servo RAM 에서 Read 중 잘못된 ID]";
                if (nStatus2 == 0xA1) strStatus2 += "[Motion 명령에 범위 벗어난 값]";
                if (nStatus2 == 0xA2) strStatus2 += "[Motion Ready 명령에 범위 벗어난 값]";
                if (nStatus2 == 0xA3) strStatus2 += "[서보 제어 명령에 범위 벗어난 값]";
                if (nStatus2 == 0xA5) strStatus2 += "[제어기 LED 제어 명령에 범위 벗어난 값]";
                if (nStatus2 == 0xA6) strStatus2 += "[버저 멜로디 명령에 범위 벗어난 값]";
                if (nStatus2 == 0xA7) strStatus2 += "[버저 음표 명령에 범위 벗어난 값]";
                if (nStatus2 == 0xB2) strStatus2 += "[없는 Buzzer를 실행하라고 함]";
                return ((((nStatus1 & 0xff) + (nStatus2 & 0x3c)) != 0) ? false : true);
            }
            #endregion Mpsu Status 함수

            public int CalcTime_ms_Remocon(int nTime)
            {
                // 1 Tick 당 125 ms => 1:11.2=x:nTime => x = nTime / 11.2
                return Clip(250, 0, (int)Math.Round((float)nTime / 125.0f));
            }
            public void Mpsu_ReMocon(int nChannel_0x61_0x6a, int nTime_ms, int nKeyNumber)
            {
                Mpsu_ReMocon(0xfe, nChannel_0x61_0x6a, nTime_ms, nKeyNumber);
            }
            public void Mpsu_ReMocon(int nMpsuID, int nChannel_0x61_0x6a, int nTime_ms, int nKeyNumber)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x1d; // Remocon

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize + i++] = (byte)(nChannel_0x61_0x6a & 0xff); // Channel
                pbyteBuffer[nDefaultSize + i++] = (byte)(CalcTime_ms_Remocon(nTime_ms) & 0xff); // Length
                pbyteBuffer[nDefaultSize + i++] = (byte)(nKeyNumber & 0xff); // Data
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void Mpsu_EnterBootloader()
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청
                byte[] pbyteBuffer = Ojw.CConvert.StrToBytes("S");

                SendPacket(pbyteBuffer, 1);
            }
            public void QuitBootloader()
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청
                byte[] pbyteBuffer = Ojw.CConvert.StrToBytes("Q");

                SendPacket(pbyteBuffer, 1);
            }
            // 주의 : 이후 제어기와의 통신은 할 수 없다. 오로지 모터와만 통신 가능
            public void Mpsu_Servo_Fw_UpdateMode(int nMpsuID)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x1E;

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void SetBusy() { m_bBusy = true; }
            public void ResetBusy() { m_bBusy = false; }
            public void Mpsu_Reboot(int nMpsuID)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nMpsuID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x1b; // Reboot

                /////////////////////////////////////////////////////
                // Data
                //pbyteBuffer[nDefaultSize + i++] = 0x00;
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void Mpsu_Reboot()
            {
                Mpsu_Reboot(0xfe);
            }
            public void Mpsu_Play_HeadLed_Buzz(int nHeadLedNum_1_63, int nBuzzNum_1_63)
            {
                Mpsu_Play_HeadLed_Buzz(0xfe, nHeadLedNum_1_63, nBuzzNum_1_63);
            }
            public void Mpsu_Play_HeadLed_Buzz(int nMpsuID, int nHeadLedNum_1_63, int nBuzzNum_1_63)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

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

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void Mpsu_Play_Buzz(int nMpsuID, int nBuzzNum_1_63)
            {
#if false
            if (IsConnect() == false) return;

            m_bBusy = true; // 통신 요청

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

            MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

            // 보내기 전에 Tick 을 Set 한다.
            //Tick_Send_Mpsu();
            SendPacket(pbyteBuffer, nDefaultSize + i);
#else
                Mpsu_Play_HeadLed_Buzz(nMpsuID, 0, nBuzzNum_1_63);
#endif
            }
            public void Mpsu_Play_Buzz(int nBuzzNum_1_63)
            {
                Mpsu_Play_Buzz(0xfe, nBuzzNum_1_63);
            }
            public void Mpsu_Play_HeadLed(int nMpsuID, int nHeadLedNum_1_63)
            {
                Mpsu_Play_HeadLed_Buzz(nMpsuID, nHeadLedNum_1_63, 0);
            }
            public void Mpsu_Play_HeadLed(int nHeadLedNum_1_63)
            {
                Mpsu_Play_HeadLed_Buzz(0xfe, nHeadLedNum_1_63, 0);
            }
            public void Mpsu_Play_Motion(int nMpsuID, int nMotionAddress, bool bReady)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

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
                pbyteBuffer[nDefaultSize + i++] = (byte)CConvert.BoolToInt(bReady);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void Mpsu_Play_Motion(int nMotionAddress, bool bReady)
            {
                Mpsu_Play_Motion(0xfe, nMotionAddress, bReady);
            }
            public void Mpsu_Play_Motion(int nMotionAddress)
            {
                Mpsu_Play_Motion(0xfe, nMotionAddress, false);
            }
            public void Mpsu_Stop_Motion()
            {
                Mpsu_Play_Motion(0xfe, 0xfe, false);
            }
            //Debug Mode Flag를 1로 해서 보내면 디버깅 모드가 된다. 진입 후 Task No를 0x01로 한 패킷을 보내면 한 스텝 진행된다. 스텝이 진행될 때마다 현재 위치를 포함한 Ack 패킷을 보낸다.(AckPlcy가 1, 2일 때)
            //Task No를 0xFE로 한 패킷을 보내면 Task가 정지된다
            public void Mpsu_Play_Task(int nMpsuID, int nTaskNo)//, bool bDebugingMode)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

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
                //pbyteBuffer[nDefaultSize + i++] = (byte)CConvert.BoolToInt(bDebugingMode);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Mpsu();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void Mpsu_Play_Task(bool bDebugingMode)
            {
                Mpsu_Play_Task(0xfe, CConvert.BoolToInt(bDebugingMode));
            }
            public void Mpsu_Play_Task()
            {
                Mpsu_Play_Task(0xfe, 0);
            }
            public void Mpsu_Stop_Task()
            {
                Mpsu_Play_Task(0xfe, 0xfe);
            }
            #endregion MPSU

            #region OPSU - Sensor
            public void ReadSensor(int nAddress, int nDataByteSize)
            {
                if (IsConnect() == false) return;

                // byte[] pbyteCmd = new byte[3] { 0x09, 0x33, 0x34 }; // Reset, Write, Read

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = 0xfa;
                // Cmd
                pbyteBuffer[_CMD] = 0x34; // 센서 데이타를 읽어온다.{ 0x09, 0x33, 0x34 } Reset, Write, Read

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize + i++] = (byte)(nAddress & 0xff);

                ////////
                pbyteBuffer[nDefaultSize + i++] = (byte)(nDataByteSize & 0xff);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                Tick_Send_Sensor();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }

            public bool IsValid_Sensor(int nAddress) { return ((m_pnSensor == null) ? false : ((nAddress >= _SIZE_SENSOR) ? false : true)); }
            public int GetData_Sensor(int nAddress) { return m_pnSensor[nAddress]; } // 에러처리가 필요하다면 IsValid_Sensor() 활용
            public int[] GetData_Sensor() { return m_pnSensor; }

            private int m_nCntTick_Send_Sensor = 0;
            private int m_nCntTick_Receive_Sensor = 0;
            private int m_nCntTick_Receive_Sensor_Back = 0;
            public long GetCounter_Timer_Sensor() { return Tick_GetTimer(256); }
            private long Tick_GetTimer_Sensor(int nAxis) { return m_aCTmr[256].Get(); }
            private void Tick_Send_Sensor() { if (IsConnect() == false) return; m_nCntTick_Send_Sensor++; m_aCTmr[256].Set(); m_nCntTick_Receive_Sensor_Back = m_nCntTick_Receive_Sensor; }
            private void Tick_Receive_Sensor() { m_nCntTick_Receive_Sensor++; m_aCTmr[256].Set(); }
            public void ResetCounter_Sensor()
            {
                m_nCntTick_Send_Sensor = 0;
                m_nCntTick_Receive_Sensor = 0;
                m_nCntTick_Receive_Sensor_Back = -1;
            }

            public bool IsReceived_Sensor() { return (m_nCntTick_Receive_Sensor != m_nCntTick_Receive_Sensor_Back) ? true : false; }
            public bool WaitReceive_Sensor(long lWaitTimer)  // true : Receive Ok, false : Fail
            {
                m_aCTmr[512 + 2].Set();
                bool bError = true;
                bool bOver = false;
                while ((IsConnect() == true) && (m_bClassEnd == false) && (bOver == false))
                {
                    if (IsReceived_Sensor() == true)
                    {
                        bError = false;
                        break;
                    }
                    if (lWaitTimer > 0)
                    {
                        if (m_aCTmr[512 + 2].Get() >= lWaitTimer)
                        {
                            bOver = true;
                        }
                    }
                    Thread.Sleep(1);
                    //Application.DoEvents();
                }
                return ((bError == false) ? true : false);
            }
            #region Sensor Status 함수
            private void SetStatus1_Sensor(int nStatus) { m_pnStatus_Sensor[0] = nStatus; }
            public int GetStatus1_Sensor() { return m_pnStatus_Sensor[0]; }
            private void SetStatus2_Sensor(int nStatus) { m_pnStatus_Sensor[1] = nStatus; }
            public int GetStatus2_Sensor() { return m_pnStatus_Sensor[1]; }

            // Error = false, 정상 = true;
            public bool GetStatusMessage_Sensor(out String strStatus1, out String strStatus2)
            {
                int nStatus1 = GetStatus1_Sensor();
                int nStatus2 = GetStatus2_Sensor();
                strStatus1 = "";
                strStatus2 = "";
                if ((nStatus1 & 0x01) != 0) strStatus1 += "[Vin Error]";
                if ((nStatus1 & 0x02) != 0) strStatus1 += "[Temp Error]";
                if ((nStatus1 & 0x04) != 0) strStatus1 += "[Packet Error]";
                if ((nStatus1 & 0x08) != 0) strStatus1 += "[Servo Missing]";
                if ((nStatus1 & 0x10) != 0) strStatus1 += "[EEP Error]";
                if ((nStatus1 & 0x20) != 0) strStatus1 += "[Servo Status Error]";
                if ((nStatus1 & 0x40) != 0) strStatus1 += "[Flash_Data_Error]";
                if ((nStatus1 & 0x80) != 0) strStatus1 += "[reserved]";

                if ((nStatus2 & 0x01) != 0) strStatus2 += "[CheckSum Error]";
                if ((nStatus2 & 0x02) != 0) strStatus2 += "[Unknown Command]";
                if ((nStatus2 & 0x04) != 0) strStatus2 += "[Reg Range Error]";
                if ((nStatus2 & 0x08) != 0) strStatus2 += "[Garbage Detected]";
                if ((nStatus2 & 0x10) != 0) strStatus2 += "[Dock Status]";
                if ((nStatus2 & 0x20) != 0) strStatus2 += "[reserved]";
                if ((nStatus2 & 0x40) != 0) strStatus2 += "[reserved]";
                if ((nStatus2 & 0x80) != 0) strStatus2 += "[reserved]";
                return ((((nStatus1 & 0xff) + (nStatus2 & 0x3c)) != 0) ? false : true);
            }
            #endregion Sensor Status 함수
            #endregion OPSU - Sensor


            #region HEAD - Default ID = 251
            private const byte _CMD_ROM_READ_HEAD = 0x4B;
            private const byte _CMD_RAM_READ_HEAD = 0x4D;
            private const byte _CMD_ROM_WRITE_HEAD = 0x4A;
            private const byte _CMD_RAM_WRITE_HEAD = 0x4C;
            public void Head_Write_Rom(int nHeadID, int nStartAddress, byte byteData)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nHeadID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x0A;// _CMD_ROM_WRITE_HEAD; // Rom Write

                /////////////////////////////////////////////////////
                // Address
                pbyteBuffer[nDefaultSize + i++] = (byte)(nStartAddress & 0xff);
                // Length
                pbyteBuffer[nDefaultSize + i++] = (byte)(1); // 한바이트만 보냄
                // Data
                pbyteBuffer[nDefaultSize + i++] = (byte)(byteData);

                ////////
                //pbyteBuffer[nDefaultSize + i++] = (byte)CConvert.BoolToInt(bDebugingMode);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Head();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void Head_Write_Rom(int nHeadID, int nStartAddress, byte[] pbyteData)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int nLength = pbyteData.Length;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nHeadID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x0A;// _CMD_ROM_WRITE_HEAD; // Rom Write

                /////////////////////////////////////////////////////
                // Address
                pbyteBuffer[nDefaultSize + i++] = (byte)(nStartAddress & 0xff);
                // Length
                pbyteBuffer[nDefaultSize + i++] = (byte)(nLength & 0xff); // 한바이트만 보냄
                // Data
                for (int nData = 0; nData < nLength; nData++)
                {
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = (byte)(pbyteData[nData]);
                }

                ////////
                //pbyteBuffer[nDefaultSize + i++] = (byte)CConvert.BoolToInt(bDebugingMode);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Head();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void Head_Write_Ram(int nHeadID, int nStartAddress, byte byteData)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nHeadID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x0C;// _CMD_RAM_WRITE_HEAD; // Ram Write

                /////////////////////////////////////////////////////
                // Address
                pbyteBuffer[nDefaultSize + i++] = (byte)(nStartAddress & 0xff);
                // Length
                pbyteBuffer[nDefaultSize + i++] = (byte)(1); // 한바이트만 보냄
                // Data
                pbyteBuffer[nDefaultSize + i++] = (byte)(byteData);

                ////////
                //pbyteBuffer[nDefaultSize + i++] = (byte)CConvert.BoolToInt(bDebugingMode);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Head();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }
            public void Head_Write_Ram(int nHeadID, int nStartAddress, byte[] pbyteData)
            {
                if (IsConnect() == false) return;

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int nLength = pbyteData.Length;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nHeadID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x0C;//_CMD_RAM_WRITE_HEAD; // Ram Write

                /////////////////////////////////////////////////////
                // Address
                pbyteBuffer[nDefaultSize + i++] = (byte)(nStartAddress & 0xff);
                // Length
                pbyteBuffer[nDefaultSize + i++] = (byte)(nLength & 0xff); // 한바이트만 보냄
                // Data
                for (int nData = 0; nData < nLength; nData++)
                {
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = (byte)(pbyteData[nData]);
                }

                ////////
                //pbyteBuffer[nDefaultSize + i++] = (byte)CConvert.BoolToInt(bDebugingMode);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                //Tick_Send_Head();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }

            public void ReadHeadRom(int nHeadID, int nAddress, int nLength)
            {
                if (IsConnect() == false) return;// false;

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nHeadID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = _CMD_ROM_READ_HEAD; // EEP Rom 영역을 읽어온다.

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize + i++] = (byte)(nAddress & 0xff);// 46번 레지스터 명령

                ////////
                pbyteBuffer[nDefaultSize + i++] = (byte)(nLength & 0xff);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                Tick_Send_Head();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }

            public void ReadHeadRam(int nHeadID, int nAddress, int nLength)
            {
                if (IsConnect() == false) return;// false;

                m_bBusy = true; // 통신 요청

                int nDefaultSize = _CHECKSUM2 + 1;

                int i = 0;
                // Header
                byte[] pbyteBuffer = new byte[256];
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nHeadID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = _CMD_RAM_READ_HEAD; // Ram 영역을 읽어온다.

                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize + i++] = (byte)(nAddress & 0xff);

                ////////
                pbyteBuffer[nDefaultSize + i++] = (byte)(nLength & 0xff);// 이후의 레지스터 사이즈
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                Tick_Send_Head();
                SendPacket(pbyteBuffer, nDefaultSize + i);
            }

            public bool IsValid_Head_Rom(int nAddress) { return ((m_pnHeadRom == null) ? false : ((nAddress >= _SIZE_HEAD_ROM) ? false : true)); }
            public int GetData_Head_Rom(int nAddress) { return m_pnHeadRom[nAddress]; } // 에러처리가 필요하다면 IsValid_Head() 활용
            public int[] GetData_Head_Rom() { return m_pnHeadRom; }
            public bool IsValid_Head_Ram(int nAddress) { return ((m_pnHeadRam == null) ? false : ((nAddress >= _SIZE_HEAD_RAM) ? false : true)); }
            public int GetData_Head_Ram(int nAddress) { return m_pnHeadRam[nAddress]; } // 에러처리가 필요하다면 IsValid_Head() 활용
            public int[] GetData_Head_Ram() { return m_pnHeadRam; }

            #region Head Led
            private int[] m_anLed_Eye = new int[32];
            public int Head_GetData_Head_Led(bool bRed, bool bLeft, int nIndex_0_7)
            {
                return Head_GetData_Head_Led(bRed, nIndex_0_7 + ((bLeft == true) ? 0 : 8));
            }
            public int Head_GetData_Head_Led(bool bRed, int nIndex_0_15)
            {
                nIndex_0_15 = nIndex_0_15 % 16;
                int nMul = 16 * ((bRed == true) ? 1 : 2);
                return m_pnHeadRam[12 + nMul - nIndex_0_15];
            }
            public int Head_GetCmd_Led_Eye(bool bRed, bool bLeft, int nPos_0_7)
            {
                int nRed_0_Blue_1 = CConvert.BoolToInt(!bRed);
                int nValue = CConvert.BoolToInt(!bLeft) * 8 + nPos_0_7 % 8;
                return (int)(m_anLed_Eye[nRed_0_Blue_1 * 16 + nValue] & 0x7f);
            }
            public int Head_GetCmd_Led_Eye(bool bRed, int nPos_0_15)
            {
                int nRed_0_Blue_1 = CConvert.BoolToInt(!bRed);

                return (int)(m_anLed_Eye[nRed_0_Blue_1 * 16 + nPos_0_15]);
            }
            public int Head_GetCmd_Led_Eye(int nPos_0_31)
            {
                return (int)(m_anLed_Eye[nPos_0_31]);
            }
            public void Head_SetCmd_Led_Eye(bool bRed, bool bLeft, int nPos_0_7, int nValue_0_7f)
            {
                int nRed_0_Blue_1 = CConvert.BoolToInt(!bRed);
                int nPos_0_15 = CConvert.BoolToInt(!bLeft) * 8 + nPos_0_7 % 8;
                Head_SetCmd_Led_Eye(bRed, nPos_0_15, nValue_0_7f);
            }
            public void Head_SetCmd_Led_Eye(bool bRed, int nPos_0_15, int nValue_0_7f)
            {
                int nRed_0_Blue_1 = CConvert.BoolToInt(!bRed);

                m_anLed_Eye[nRed_0_Blue_1 * 16 + nPos_0_15 % 16] = nValue_0_7f;
            }
            public void Head_SetCmd_Led_Eye(int nPos_0_31, int nValue_0_7f)
            {
                m_anLed_Eye[nPos_0_31] = nValue_0_7f;
            }
            public void Head_SetLed(int nPos, int nValue)
            {
                bool bRed = ((nPos / 16) == 0) ? true : false;
                nPos = nPos % 16 + ((bRed == true) ? 16 : 0);
                //SetCmd_Led_Eye(bRed, nPos % 16, nValue & 0x7f);
                Head_Write_Ram(251, 43 - nPos, (byte)(nValue & 0x7f)); // 43 번지부터 거꾸로 계산. -_-;
            }
            //public void Head_SetLed()
            //{
            //    for (int i = 0; i < 32; i++)
            //    {
            //        Head_SetLed(i, Head_GetCmd_Led_Eye(i));
            //    }
            //    //Head_Write_Ram(251, 12 + 32 - nPos, (byte)(nValue & 0x7f));
            //}
            public void Head_SetLed()
            {
                byte[] abyLed = new byte[32];
                for (int i = 0; i < 32; i++)
                {
                    //abyLed[31 - i] = (byte)(Head_GetCmd_Led_Eye(i) & 0xff);

                    bool bRed = ((i / 16 == 0) ? true : false);
                    //int nPos = i % 16 + 16 * (i / 16);
                    int nPos = i % 16 + (bRed == true ? 16 : 0);
                    abyLed[31 - nPos] = (byte)(Head_GetCmd_Led_Eye(i) & 0xff);
                }
                Head_Write_Ram(251, 12, abyLed);
            }
            #endregion Head Led

            private int m_nCntTick_Send_Head = 0;
            private int m_nCntTick_Receive_Head = 0;
            private int m_nCntTick_Receive_Head_Back = 0;
            public long GetCounter_Timer_Head() { return Tick_GetTimer(256); }
            private long Tick_GetTimer_Head(int nAxis) { return m_aCTmr[256].Get(); }
            private void Tick_Send_Head() { if (IsConnect() == false) return; m_nCntTick_Send_Head++; m_aCTmr[256].Set(); m_nCntTick_Receive_Head_Back = m_nCntTick_Receive_Head; }
            private void Tick_Receive_Head() { m_nCntTick_Receive_Head++; m_aCTmr[256].Set(); }
            public void ResetCounter_Head()
            {
                m_nCntTick_Send_Head = 0;
                m_nCntTick_Receive_Head = 0;
                m_nCntTick_Receive_Head_Back = -1;
            }

            public bool IsReceived_Head() { return (m_nCntTick_Receive_Head != m_nCntTick_Receive_Head_Back) ? true : false; }
            public bool WaitReceive_Head(long lWaitTimer)  // true : Receive Ok, false : Fail
            {
                m_aCTmr[512 + 2].Set();
                bool bError = true;
                bool bOver = false;
                while ((IsConnect() == true) && (m_bClassEnd == false) && (bOver == false))
                {
                    if (IsReceived_Head() == true)
                    {
                        bError = false;
                        break;
                    }
                    if (lWaitTimer > 0)
                    {
                        if (m_aCTmr[512 + 2].Get() >= lWaitTimer)
                        {
                            bOver = true;
                        }
                    }
                    Thread.Sleep(1);
                    //Application.DoEvents();
                }
                return ((bError == false) ? true : false);
            }
            #region Head Status 함수
            private void SetStatus1_Head(int nStatus) { m_pnStatus_Head[0] = nStatus; }
            public int GetStatus1_Head() { return m_pnStatus_Head[0]; }
            private void SetStatus2_Head(int nStatus) { m_pnStatus_Head[1] = nStatus; }
            public int GetStatus2_Head() { return m_pnStatus_Head[1]; }

            // Error = false, 정상 = true;
            public bool GetStatusMessage_Head(out String strStatus1, out String strStatus2)
            {
                int nStatus1 = GetStatus1_Head();
                int nStatus2 = GetStatus2_Head();
                strStatus1 = "";
                strStatus2 = "";
                if ((nStatus1 & 0x01) != 0) strStatus1 += "[전압에러]";
                if ((nStatus1 & 0x02) != 0) strStatus1 += "[Reserved]";
                if ((nStatus1 & 0x04) != 0) strStatus1 += "[온도에러]";
                if ((nStatus1 & 0x08) != 0) strStatus1 += "[패킷에러]";
                if ((nStatus1 & 0x10) != 0) strStatus1 += "[Reserved]";
                if ((nStatus1 & 0x20) != 0) strStatus1 += "[Reserved]";
                if ((nStatus1 & 0x40) != 0) strStatus1 += "[EEPROM 손상]";
                if ((nStatus1 & 0x80) != 0) strStatus1 += "[Reserved]";

                if (nStatus2 == 0x01) strStatus2 += "[Reserved]";
                if (nStatus2 == 0x02) strStatus2 += "[USART 버퍼 꽉참]";
                if (nStatus2 == 0x03) strStatus2 += "[체크섬 에러]";
                if (nStatus2 == 0x11) strStatus2 += "[모르는 명령값]";
                if (nStatus2 == 0x12) strStatus2 += "[레지스터 범위 벗어남]";
                if (nStatus2 == 0x21) strStatus2 += "[쓰레기 데이터 들어옴]";
                if (nStatus2 == 0x22) strStatus2 += "[터치인식(1:인식 O, 0:인식 X)]";
                if (nStatus2 == 0x23) strStatus2 += "[Reserved]";
                return ((((nStatus1 & 0xff) + (nStatus2 & 0x3c)) != 0) ? false : true);
            }
            #endregion Head Status 함수

            #endregion HEAD - Default ID = 251


            #region 기본제어 변수(Stop, Ems, Reset)
            private bool m_bEms = false;
            private bool m_bStop = false;
            public bool IsEms() { return m_bEms; }
            public bool IsStop() { return m_bStop; }
            #endregion

            #region Stop - 정지
            // 전체 정지 - 이걸로 멈추면 반드시 리셋이 필요(Stop 변수만 리셋하면 됨)
            public void Stop()
            {

#if true
#if true
                m_bStop = true;
                for (int i = 0; i < m_nMotor_Max; i++)
                {
                    //if (GetSpeedType(i) == true) SetCmd(i, 0);
                    SetCmd(i, 0);
                    SetCmd_Flag_Mode(i, true);
                    //SetCmd_Flag_Stop(i, true);
                }
                //bool bStop = m_bStop;
                //bool bEms = m_bEms;
                //m_bStop = false;
                //m_bEms = false;
                SetMot_Stop(0xfe);//SetMot(1000);

                //m_bEms = bEms;
#else
            int nPos = 0;
            int nTime = 0;

            int nDefaultSize = _CHECKSUM2 + 1;
            byte[] pbyteBuffer = new byte[4096];

            int nCalcTime = CalcTime_ms(nTime);

            int i = 0;
            // Header
            pbyteBuffer[_HEADER1] = 0xff;
            pbyteBuffer[_HEADER2] = 0xff;
            // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
            pbyteBuffer[_ID] = 0xfe;// (byte)(0xfe & 0xff);
            // Cmd
            pbyteBuffer[_CMD] = 0x05;

            nPos &= 0x03ff; // 10비트만 사용
            //nPos |= 0x400;  // 속도제어 
            //nPos |= _JOG_MODE_SPEED << 10;  // 속도제어 
            nPos |= _FLAG_STOP << 12;
            /////////////////////////////////////////////////////
            int nID;

            for (int nAxis = 0; nAxis < m_nMotor_Max; nAxis++)
            {
                if (m_pSMot[nAxis].bEn == true)
                {
                    nID = GetID_By_Axis(nAxis);
                    // Data
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nPos & 0xff);
                    pbyteBuffer[nDefaultSize + i++] = (byte)((nPos >> 8) & 0xff);
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nCalcTime & 0xff);
                    pbyteBuffer[nDefaultSize + i++] = (byte)((nCalcTime >> 8) & 0xff);
                    // - 모터당 아이디(후면에 붙는다)
                    pbyteBuffer[nDefaultSize + i++] = (byte)(nID & 0xff);
                }
            }
            /////////////////////////////////////////////////////

            //Packet Size
            pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

            MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

            SendPacket(pbyteBuffer, nDefaultSize + i);
            String strTest = "";
            int nSize = nDefaultSize + i;
            for (i = 0; i < nSize; i++)
            {
                strTest += "[" + CConvert.IntToStr(i) + "]" + CConvert.IntToHex(pbyteBuffer[i]) + "\r\n";
            }

            pbyteBuffer = null;
            //return strTest;
#endif
#else
            for (int nAxis = 0; nAxis < m_nMotor_Max; nAxis++) SetCmd_Flag_Stop(nAxis, true);
            SetMot(1000);
#endif
            }

            // 이건 그냥 멈추기만 할 뿐 변수를 셋하지는 않는다.
            public void Stop(int nAxis)
            {
                m_bStop = true;
                SetCmd_Flag_Stop(nAxis, true);
                SetCmd_Flag_Mode(nAxis, true);
                SetCmd(nAxis, 0);
                //bool bStop = m_bStop;
                //bool bEms = m_bEms;
                //m_bStop = false;
                //m_bEms = false;

                SetMot_Stop(nAxis);//nAxis, 1000);

                //m_bEms = bEms;
                //m_bStop = true;
            }
            #endregion

            #region Ems - 비상정지
            public void Ems()
            {
                Stop();
                DrvSrv(false, false);
                m_bEms = true;
            }
            #endregion

            #region Reset
            public void Reset(int nAxis)
            {
                if (nAxis < 0xfe)
                    SetCmd_Flag(nAxis, 0);
                else
                {
                    for (int i = 0; i < m_nMotor_Max; i++) SetCmd_Flag(i, 0);
                }

                int nID = GetID_By_Axis(nAxis);//m_pSMot[nAxis].nID;
                int nDefaultSize = _CHECKSUM2 + 1;
                byte[] pbyteBuffer = new byte[nDefaultSize];
                // Header
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x09; // Reset

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize) & 0xff);

                MakeCheckSum(nDefaultSize, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                SendPacket(pbyteBuffer, nDefaultSize);
                pbyteBuffer = null;
            }
            public void Rollback(int nAxis, bool bIgnoreID, bool bIgnoreBaudrate)
            {
                int i = 0;
                if (nAxis < 0xfe)
                    SetCmd_Flag(nAxis, 0);
                else
                {
                    for (i = 0; i < m_nMotor_Max; i++) SetCmd_Flag(i, 0);
                }
                int nID = GetID_By_Axis(nAxis);//m_pSMot[nAxis].nID;
                int nDefaultSize = _CHECKSUM2 + 1;
                byte[] pbyteBuffer = new byte[255];
                // Header
                pbyteBuffer[_HEADER1] = 0xff;
                pbyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = 0x08; // RollBack

                i = 0;
                /////////////////////////////////////////////////////
                // Data
                pbyteBuffer[nDefaultSize + i++] = (byte)((bIgnoreID == true) ? 1 : 0);
                ////////
                pbyteBuffer[nDefaultSize + i++] = (byte)((bIgnoreBaudrate == true) ? 1 : 0);
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

                SendPacket(pbyteBuffer, nDefaultSize + i);
                pbyteBuffer = null;
            }

            public void ResetStop() { m_bStop = false; for (int i = 0; i < m_nMotor_Max; i++) SetCmd_Flag_Stop(i, false); }
            public void ResetEms() { m_bEms = false; for (int i = 0; i < m_nMotor_Max; i++) SetCmd_Flag_Stop(i, false); }

            public void Reset()
            {
                ResetStop();
                ResetEms();
                Reset(0xfe);
            }
            #endregion

            #region Servo, Drive
            // Servo / Driver
            public void DrvSrv(bool bDriver, bool bServo)
            {
                //if ((m_bEms == true) && (bDriver == false) && (bServo == false)) return;
                DrvSrv(0xfe, bDriver, bServo);
            }

            public void DrvSrv(int nAxis, bool bDriver, bool bServo)
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
            }
            #endregion

            public void MovePConti(int nAxis, int nPos, int nTime)
            {
                if ((m_bStop == true) || (m_bEms == true)) return;
                int nID = GetID_By_Axis(nAxis);//m_pSMot[nAxis].nID;
                int i = 0;
                int nCalcTime = CalcTime_ms(nTime);
                ////////////////////////////////////////////////

                //// 관절 리미트 클리핑
                //if (
                //    (m_pSMot[nAxis].nLimitUp > m_pSMot[nAxis].nLimitDn) &&
                //    //(m_pSMot[nAxis].nLimitUp <= 1024) &&
                //    //(m_pSMot[nAxis].nLimitDn >= 0)
                //) nPos = Clip(m_pSMot[nAxis].nLimitUp, m_pSMot[nAxis].nLimitDn, nPos);
                //// 캘리브레이션
                //nPos -= (_CENTER_POS - m_pSMot[nAxis].nCenterPos);
                //nPos = Clip(1024, 0, nPos);

                // 클리핑 포함, 기록남기기
                SetCmd(nAxis, nPos);
                nPos = GetCmd(nAxis);

                //nPos |= _JOG_MODE_SPEED << 10;  // 속도제어 

                byte[] pbyteBuffer = new byte[5];//[255];
                pbyteBuffer[i++] = (byte)(nPos & 0xff);
                pbyteBuffer[i++] = (byte)((nPos >> 8) & 0xff);
                pbyteBuffer[i++] = (byte)(nCalcTime & 0xff);
                pbyteBuffer[i++] = (byte)((nCalcTime >> 8) & 0xff);
                // - 모터당 아이디(후면에 붙는다)
                pbyteBuffer[i++] = (byte)(nID & 0xff);
                ////////////////////////////////////////////////

                Make_And_Send_Packet(0xfe, 0x05, i, pbyteBuffer);

                pbyteBuffer = null;
            }

            // 전축 제어, 단 bEn 이 true 여야 한다. -> setcmd 를 하면 자동으로 셋됨. 단, initcmd 를 하거나 수동으로 리셋을 하면 이후 다시 셋 전까진 제외
            public void MovePConti(int nTime)
            {
                if ((m_bStop == true) || (m_bEms == true)) return;
                int nID;
                int i = 0;
                int nCalcTime = CalcTime_ms(nTime);
                ////////////////////////////////////////////////

                byte[] pbyteBuffer = new byte[255];
                int nPos;
                for (int nAxis = 0; nAxis < m_nMotor_Max; nAxis++)
                {
                    if (m_pSMot[nAxis].bEn == true)
                    {
                        nID = GetID_By_Axis(nAxis);

                        nPos = GetCmd(nAxis);
                        //nPos |= _JOG_MODE_SPEED << 10;  // 속도제어 

                        pbyteBuffer[i++] = (byte)(nPos & 0xff);
                        pbyteBuffer[i++] = (byte)((nPos >> 8) & 0xff);
                        pbyteBuffer[i++] = (byte)(nCalcTime & 0xff);
                        pbyteBuffer[i++] = (byte)((nCalcTime >> 8) & 0xff);
                        // - 모터당 아이디(후면에 붙는다)
                        pbyteBuffer[i++] = (byte)(nID & 0xff);
                        ////////////////////////////////////////////////
                    }
                }

                Make_And_Send_Packet(0xfe, 0x05, i, pbyteBuffer);

                pbyteBuffer = null;
            }

            public void SetMot(int nTime)
            {
                if ((m_bStop == true) || (m_bEms == true)) return;
                int nID;
                int i = 0;
                ////////////////////////////////////////////////

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
                        nPos = GetCmd(nAxis);
                        if (nPos < 0)
                        {
                            nPos *= -1;
                            nPos |= 0x4000;
                        }

                        pbyteBuffer[i++] = (byte)(nPos & 0xff);
                        pbyteBuffer[i++] = (byte)((nPos >> 8) & 0xff);
                        #endregion

                        #region Set-Flag
                        nFlag = GetCmd_Flag(nAxis);
                        pbyteBuffer[i++] = (byte)(nFlag & 0xff);
                        SetCmd_Flag_NoAction(nAxis, true); // 동작 후 모터 NoAction을 살려둔다.
                        #endregion Set-Flag

                        #region 모터당 아이디(후면에 붙는다)
                        nID = GetID_By_Axis(nAxis);
                        pbyteBuffer[i++] = (byte)(nID & 0xff);
                        #endregion 모터당 아이디(후면에 붙는다)
                        ////////////////////////////////////////////////
                    }
                    m_pSMot[nAxis].bEn = false;
                }

                //Make_And_Send_Packet(0xfe, 0x05, i, pbyteBuffer);
                Make_And_Send_Packet(0xfe, 0x06, i, pbyteBuffer);
                pbyteBuffer = null;
            }
            public void SetMot_Stop(int nAxis)
            {
                //int nID = 254;// GetID_By_Axis(nAxis);//m_pSMot[nAxis].nID;
                int i = 0;
                int nCalcTime = CalcTime_ms(1000);
                ////////////////////////////////////////////////

                byte[] pbyteBuffer = new byte[5];//[255];
                pbyteBuffer[i++] = 0;// (byte)(nPos & 0xff);
                pbyteBuffer[i++] = 0;// (byte)((nPos >> 8) & 0xff);
                pbyteBuffer[i++] = (byte)(_FLAG_MODE_SPEED | _FLAG_STOP);// (byte)(nCalcTime & 0xff);
                // - 모터당 아이디(후면에 붙는다)
                pbyteBuffer[i++] = (byte)(nAxis & 0xff);// (254 & 0xff);
                pbyteBuffer[i++] = (byte)((nCalcTime >> 8) & 0xff);
                ////////////////////////////////////////////////

                Make_And_Send_Packet(0xfe, 0x05, i, pbyteBuffer);

                pbyteBuffer = null;
            }

            public void SetMot(int nAxis, int nTime)
            {
                if ((m_bStop == true) || (m_bEms == true)) return;
                int nID;
                int i = 0;
                ////////////////////////////////////////////////

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
                    nPos = GetCmd(nAxis);
                    if (nPos < 0)
                    {
                        nPos *= -1;
                        nPos |= 0x4000;
                    }
                    pbyteBuffer[i++] = (byte)(nPos & 0xff);
                    pbyteBuffer[i++] = (byte)((nPos >> 8) & 0xff);
                    #endregion

                    #region Set-Flag
                    nFlag = GetCmd_Flag(nAxis);
                    pbyteBuffer[i++] = (byte)(nFlag & 0xff);
                    SetCmd_Flag_NoAction(nAxis, true); // 동작 후 모터 NoAction을 살려둔다.
                    #endregion Set-Flag

                    #region 모터당 아이디(후면에 붙는다)
                    nID = GetID_By_Axis(nAxis);
                    pbyteBuffer[i++] = (byte)(nID & 0xff);
                    #endregion 모터당 아이디(후면에 붙는다)
                    ////////////////////////////////////////////////
                }

                //Make_And_Send_Packet(0xfe, 0x05, i, pbyteBuffer);
                Make_And_Send_Packet(0xfe, 0x06, i, pbyteBuffer);
                pbyteBuffer = null;
            }

            public struct SMotion_t
            {
                public SMotion_Header_t SHeader;
                public SMotion_Frame_t[] pSFrame;
                public String strComment;
                public String[] pstrCaption;
            }
            public struct SMotion_Header_t
            {
                public String strVersion;
                public String strTitle;
                public int nStartPos;
                public int nSize_Frame;
                public int nSize_Comment;
                public int nSize_Caption;
                public int nPlayingTime;
                public int nRobotModelType;
                public int nCnt_Motor;
                public int[] pnIndex;
                public int[] pnIndex_Mirror;
            }
            public struct SMotion_Motor_t
            {
                public float fMotorValue;
                public int nFlag_Led;
                public int nFlag_Type;
                public int nFlag_NoAction;
            }
            public struct SMotion_Frame_t
            {
                public bool bEn;
                public SMotion_Motor_t[] pSMotor;
                public int nTime_Play;
                public int nTime_Delay;
                public int nGroup;
                public int nCommand;
                public int nData0;
                public int nData1;
                //public int nData2;
                //public int nData3;
                //public int nBuzz;
                //public int nLed;
                public float fTranslation_X;
                public float fTranslation_Y;
                public float fTranslation_Z;
                public float fRotation_X;
                public float fRotation_Y;
                public float fRotation_Z;
            }
            public bool DataFileOpen(String strFileName, byte[] byteArrayData, ref SMotion_t SMotion)//, bool bMessage)//, bool bTableOut)
            {
                bool bFile = false;

                if (byteArrayData == null) bFile = true;
                else bFile = false;

                bool bFileOpened = false;
                String _STR_EXT = "dmt";
                String _STR_VER_V_12 = "1.2";
                String _STR_VER_V_11 = "1.1";
                String _STR_VER_V_10 = "1.0";

                FileInfo f = null;
                FileStream fs = null;

                try
                {
                    int i, j;
                    byte[] byteData;
                    string strFileName2 = "";
                    if (bFile == true)
                    {
                        f = new FileInfo(strFileName);
                        fs = f.OpenRead();
                        byteData = new byte[fs.Length];
                        fs.Read(byteData, 0, byteData.Length);
                        strFileName2 = f.Name;
                    }
                    else
                    {
                        if (byteArrayData.Length <= 6) return false;
                        byteData = new byte[byteArrayData.Length];
                        byteData = byteArrayData;
                        strFileName2 = strFileName;
                    }

                    // 데이타 형식 구분
                    String strTmp = "";
                    strTmp += (char)byteData[0];
                    strTmp += (char)byteData[1];
                    strTmp += (char)byteData[2];
                    strTmp += (char)byteData[3];
                    strTmp += (char)byteData[4];
                    strTmp += (char)byteData[5];

                    SMotion.SHeader.strVersion = strTmp;

                    if (strTmp.ToUpper() == _STR_EXT.ToUpper() + _STR_VER_V_10)
                    {
                        #region FileOpen V1.0
                        int nPos = 6;   // 앞의 6개는 'DMT1.0' 에 할당

                        #region Header

                        #region 타이틀(21)
                        byte[] byteGetData = new byte[21];
                        for (i = 0; i < 21; i++) byteGetData[i] = 0;
                        for (i = 0; i < 21; i++)
                        {
                            if (byteData[i + nPos] == 0) break;
                            byteGetData[i] = byteData[i + nPos];
                        }
                        SMotion.SHeader.strTitle = System.Text.Encoding.Default.GetString(byteGetData);
                        nPos += 21;
                        byteGetData = null;
                        #endregion 타이틀(21)

                        #region Start Position(1)
                        int nStartPosition = (int)(byteData[nPos++]);
                        nStartPosition = (nStartPosition >= 0) ? nStartPosition : 0;
                        SMotion.SHeader.nStartPos = nStartPosition;
                        #endregion Start Position(1)

                        #region Size - MotionFrame(2), Comment(2), Caption(2), PlayTime(4), RobotModelNumber(2), MotorCnt(1)
                        // Size
                        int nFrameSize, nCnt_LineComment, nPlayTime, nCommentSize, nRobotModelNum, nMotorCnt;
                        nFrameSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nCommentSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nCnt_LineComment = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nPlayTime = (int)(byteData[nPos] + byteData[nPos + 1] * 256 + byteData[nPos + 2] * 256 * 256 + byteData[nPos + 3] * 256 * 256 * 256); nPos += 4;
                        nRobotModelNum = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nMotorCnt = (int)(byteData[nPos++]);

                        SMotion.SHeader.nSize_Frame = nFrameSize;
                        SMotion.SHeader.nSize_Comment = nCommentSize;
                        SMotion.SHeader.nSize_Caption = nCnt_LineComment;
                        SMotion.SHeader.nPlayingTime = nPlayTime;
                        SMotion.SHeader.nRobotModelType = nRobotModelNum;
                        SMotion.SHeader.nCnt_Motor = nMotorCnt;

                        #endregion Size - MotionFrame, Comment, Caption, PlayTime

                        #endregion Header

                        // 메모리 확보
                        SMotion.pSFrame = new SMotion_Frame_t[SMotion.SHeader.nSize_Frame];
                        SMotion.pstrCaption = new String[SMotion.SHeader.nSize_Frame];//SMotion.SHeader.nSize_Caption];

                        #region 실제 모션
                        int nH = nFrameSize;
                        int nData;
                        short sData;
                        float fValue;
                        for (j = 0; j < nH; j++)
                        {
                            //En
                            #region Enable
                            int nEn = byteData[nPos++];
                            bool bEn = ((nEn & 0x01) != 0) ? true : false;

                            SMotion.pSFrame[j].bEn = bEn;
                            #endregion Enable
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #region Motor
                            int nMotorCntMax = nMotorCnt;
                            // 0-Index, 1-En, 2 ~ 24, 25 - speed, 26 - delay, 27,28,29,30 - Data0-3, 31 - time, 32 - caption
                            for (int nAxis = 0; nAxis < nMotorCntMax; nAxis++)
                            {
                                nData = (int)(BitConverter.ToInt16(byteData, nPos)); nPos += 2;
                                sData = (short)(nData & 0x0fff);
                                if ((sData & 0x800) != 0) sData -= 0x1000;

                                SMotion.pSFrame[j].pSMotor[nAxis].nFlag_Led = (int)((nData >> 12) & 0x07);
                                SMotion.pSFrame[j].pSMotor[nAxis].nFlag_Type = (int)(nData & 0x8000);
                                SMotion.pSFrame[j].pSMotor[nAxis].nFlag_NoAction = (int)((sData == 0x7ff) ? 1 : 0);

                                if (sData == 0x7ff)
                                {
                                    SMotion.pSFrame[j].pSMotor[nAxis].fMotorValue = 0.0f;
                                }
                                else
                                {
                                    fValue = CalcEvd2Angle(nAxis, (int)sData);
                                    SMotion.pSFrame[j].pSMotor[nAxis].fMotorValue = fValue;
                                }

                                /* - Save
                                fValue = Grid_GetMot(i, j);
                                sData = (short)(OjwMotor.CalcAngle2Evd(j, fValue) & 0x03ff);
                                //sData |= 0x0400; // 속도모드인때 정(0-0x0000), 역(1-0x0400)
                                //sData |= LED;  // 00 - 0ff, 0x0800 - Red(01), 0x1000 - Blue(10), 0x1800 - Green(11)
                                //sData |= 제어타입 // 0 - 위치, 0x2000 - 속도
                                sData |= 0x4000; //Enable // 개별 Enable (0 - Disable, 0x4000 - Enable)
                                 */
                            }
                            #endregion Motor
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                            // Speed  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            SMotion.pSFrame[j].nTime_Play = nData;

                            // Delay  
                            nData = BitConverter.ToInt16(byteData, nPos); nPos += 2;
                            SMotion.pSFrame[j].nTime_Delay = nData;

                            // Group  
                            nData = (int)(byteData[nPos++]);
                            SMotion.pSFrame[j].nGroup = nData;

                            // Command  
                            nData = (int)(byteData[nPos++]);
                            SMotion.pSFrame[j].nCommand = nData;

                            // Data0  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            SMotion.pSFrame[j].nData0 = nData;
                            // Data1  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            SMotion.pSFrame[j].nData1 = nData;
                            #endregion Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region 이산에서 추가한 Frame 위치 및 자세

                            SMotion.pSFrame[j].fTranslation_X = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;
                            SMotion.pSFrame[j].fTranslation_Y = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;
                            SMotion.pSFrame[j].fTranslation_Z = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;

                            // Tilt, Pan, Swing
                            SMotion.pSFrame[j].fRotation_X = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;
                            SMotion.pSFrame[j].fRotation_Y = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;
                            SMotion.pSFrame[j].fRotation_Z = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;
                            #endregion 이산에서 추가한 Frame 위치 및 자세
                        }
                        #endregion 실제 모션

                        string strData_ME = "";
                        string strData_FE = "";

                        // 'M' 'E'
                        strData_ME += (char)(byteData[nPos++]);
                        strData_ME += (char)(byteData[nPos++]);

                        #region Comment Data
                        // Comment
                        byte[] pstrComment = new byte[nCommentSize];
                        for (j = 0; j < nCommentSize; j++)
                            pstrComment[j] = (byte)(byteData[nPos++]);
                        SMotion.strComment = System.Text.Encoding.Default.GetString(pstrComment);
                        pstrComment = null;
                        #endregion Comment Data

                        #region Caption
                        Array.Clear(SMotion.pstrCaption, 0, SMotion.pstrCaption.Length);

                        int nLineNum = 0;
                        string strLineComment;
                        byte[] byLine = new byte[46];
                        for (j = 0; j < nCnt_LineComment; j++)
                        {
                            nLineNum = (short)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            for (int k = 0; k < 46; k++)
                                byLine[k] = (byte)(byteData[nPos++]);
                            strLineComment = System.Text.Encoding.Default.GetString(byLine);
                            strLineComment = strLineComment.Trim((char)0);
                            //Grid_SetCaption(nLineNum, strLineComment);

                            SMotion.pstrCaption[nLineNum] = strLineComment;
                        }
                        byLine = null;
                        #endregion Caption

                        // 'T' 'E'
                        strData_FE += (char)(byteData[nPos++]);
                        strData_FE += (char)(byteData[nPos++]);
                        bFileOpened = true;
                        #endregion FileOpen V1.0
                    }
                    else if (strTmp.ToUpper() == _STR_EXT.ToUpper() + _STR_VER_V_11)
                    {
                        #region FileOpen V1.1
                        int nPos = 6;   // 앞의 6개는 'DMT1.0' 에 할당

                        #region Header

                        #region 타이틀(21)
                        byte[] byteGetData = new byte[21];
                        for (i = 0; i < 21; i++) byteGetData[i] = 0;
                        for (i = 0; i < 21; i++)
                        {
                            if (byteData[i + nPos] == 0) break;
                            byteGetData[i] = byteData[i + nPos];
                        }
                        SMotion.SHeader.strTitle = System.Text.Encoding.Default.GetString(byteGetData);
                        nPos += 21;
                        byteGetData = null;
                        #endregion 타이틀(21)

                        #region Start Position(1)
                        int nStartPosition = (int)(byteData[nPos++]);
                        nStartPosition = (nStartPosition >= 0) ? nStartPosition : 0;
                        SMotion.SHeader.nStartPos = nStartPosition;
                        #endregion Start Position(1)

                        #region Size - MotionFrame(2), Comment(2), Caption(2), PlayTime(4), RobotModelNumber(2), MotorCnt(1)
                        // Size
                        int nFrameSize, nCnt_LineComment, nPlayTime, nCommentSize, nRobotModelNum, nMotorCnt;
                        nFrameSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nCommentSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nCnt_LineComment = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nPlayTime = (int)(byteData[nPos] + byteData[nPos + 1] * 256 + byteData[nPos + 2] * 256 * 256 + byteData[nPos + 3] * 256 * 256 * 256); nPos += 4;
                        nRobotModelNum = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nMotorCnt = (int)(byteData[nPos++]);

                        SMotion.SHeader.nSize_Frame = nFrameSize;
                        SMotion.SHeader.nSize_Comment = nCommentSize;
                        SMotion.SHeader.nSize_Caption = nCnt_LineComment;
                        SMotion.SHeader.nPlayingTime = nPlayTime;
                        SMotion.SHeader.nRobotModelType = nRobotModelNum;
                        SMotion.SHeader.nCnt_Motor = nMotorCnt;

                        #endregion Size - MotionFrame, Comment, Caption, PlayTime

                        #endregion Header

                        // 메모리 확보
                        SMotion.pSFrame = new SMotion_Frame_t[SMotion.SHeader.nSize_Frame];
                        SMotion.pstrCaption = new String[SMotion.SHeader.nSize_Frame];//SMotion.SHeader.nSize_Caption];

                        #region 실제 모션
                        int nH = nFrameSize;
                        int nData, nData2;
                        //short sData;
                        float fValue;
                        for (j = 0; j < nH; j++)
                        {
                            //En
                            #region Enable
                            int nEn = byteData[nPos++];
                            bool bEn = ((nEn & 0x01) != 0) ? true : false;
                            SMotion.pSFrame[j].bEn = bEn;
                            #endregion Enable
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #region Motor
                            int nMotorCntMax = nMotorCnt;
                            // 0-Index, 1-En, 2 ~ 24, 25 - speed, 26 - delay, 27,28,29,30 - Data0-3, 31 - time, 32 - caption
                            for (int nAxis = 0; nAxis < nMotorCntMax; nAxis++)
                            {
                                nData = byteData[nPos++];
                                nData += byteData[nPos++] * 256;
                                nData += byteData[nPos++] * 256 * 256;

                                nData2 = nData & 0x3fff;

                                if ((nData & 0x4000) != 0) nData2 *= -1; // 부호비트 검사

                                // 엔코더 타입정의
                                // 일단 넘어간다.

                                // Stop Bit
                                // 넘어간다.

                                // Mode
                                SMotion.pSFrame[j].pSMotor[nAxis].nFlag_Type = (((nData & 0x20000) != 0) ? 1 : 0);
                                SMotion.pSFrame[j].pSMotor[nAxis].nFlag_Led = ((nData >> 18) & 0x07);
                                SMotion.pSFrame[j].pSMotor[nAxis].nFlag_NoAction = ((nData == 0x200000) ? 1 : 0);

                                if (SMotion.pSFrame[j].pSMotor[nAxis].nFlag_NoAction != 0) // NoAction 이라면
                                {
                                    SMotion.pSFrame[j].pSMotor[nAxis].fMotorValue = 0.0f;
                                }
                                else
                                {
                                    fValue = CalcEvd2Angle(nAxis, (int)nData2);
                                    SMotion.pSFrame[j].pSMotor[nAxis].fMotorValue = fValue;
                                }

                                /* - Save
                                fValue = Grid_GetMot(i, j);
                                sData = (short)(OjwMotor.CalcAngle2Evd(j, fValue) & 0x03ff);
                                //sData |= 0x0400; // 속도모드인때 정(0-0x0000), 역(1-0x0400)
                                //sData |= LED;  // 00 - 0ff, 0x0800 - Red(01), 0x1000 - Blue(10), 0x1800 - Green(11)
                                //sData |= 제어타입 // 0 - 위치, 0x2000 - 속도
                                sData |= 0x4000; //Enable // 개별 Enable (0 - Disable, 0x4000 - Enable)
                                 */
                            }
                            #endregion Motor
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                            // Speed  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            SMotion.pSFrame[j].nTime_Play = nData;

                            // Delay  
                            nData = BitConverter.ToInt16(byteData, nPos); nPos += 2;
                            SMotion.pSFrame[j].nTime_Delay = nData;

                            // Group  
                            nData = (int)(byteData[nPos++]);
                            SMotion.pSFrame[j].nGroup = nData;

                            // Command  
                            nData = (int)(byteData[nPos++]);
                            SMotion.pSFrame[j].nCommand = nData;

                            // Data0  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            SMotion.pSFrame[j].nData0 = nData;
                            // Data1  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            SMotion.pSFrame[j].nData1 = nData;
                            #endregion Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region 이산에서 추가한 Frame 위치 및 자세
                            SMotion.pSFrame[j].fTranslation_X = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;
                            SMotion.pSFrame[j].fTranslation_Y = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;
                            SMotion.pSFrame[j].fTranslation_Z = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;

                            SMotion.pSFrame[j].fRotation_X = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;
                            SMotion.pSFrame[j].fRotation_Y = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;
                            SMotion.pSFrame[j].fRotation_Z = (float)BitConverter.ToSingle(byteData, nPos); nPos += 4;
                            #endregion 이산에서 추가한 Frame 위치 및 자세
                        }
                        #endregion 실제 모션

                        string strData_ME = "";
                        string strData_FE = "";

                        // 'M' 'E'
                        strData_ME += (char)(byteData[nPos++]);
                        strData_ME += (char)(byteData[nPos++]);

                        #region Comment Data
                        // Comment
                        byte[] pstrComment = new byte[nCommentSize];
                        for (j = 0; j < nCommentSize; j++)
                            pstrComment[j] = (byte)(byteData[nPos++]);
                        SMotion.strComment = System.Text.Encoding.Default.GetString(pstrComment);
                        pstrComment = null;
                        #endregion Comment Data

                        #region Caption
                        int nLineNum = 0;
                        string strLineComment;
                        byte[] byLine = new byte[46];
                        for (j = 0; j < nCnt_LineComment; j++)
                        {
                            nLineNum = (short)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            for (int k = 0; k < 46; k++)
                                byLine[k] = (byte)(byteData[nPos++]);
                            strLineComment = System.Text.Encoding.Default.GetString(byLine);
                            strLineComment = strLineComment.Trim((char)0);
                            //Grid_SetCaption(nLineNum, strLineComment);

                            SMotion.pstrCaption[nLineNum] = strLineComment;
                        }
                        byLine = null;
                        #endregion Caption

                        // 'T' 'E'
                        strData_FE += (char)(byteData[nPos++]);
                        strData_FE += (char)(byteData[nPos++]);

                        //                     if (bMessage == true)
                        //                     {
                        //                         if (strData_ME != "ME") OjwMessage("Motion Table Error\r\n");
                        //                         else OjwMessage("Table Loaded");
                        //                         if (strData_FE != "TE") OjwMessage("File Error\r\n");
                        //                         else OjwMessage("Table Loaded");
                        //                     }

                        bFileOpened = true;
                        #endregion FileOpen V1.1
                    }
                    else if (strTmp.ToUpper() == _STR_EXT.ToUpper() + _STR_VER_V_12)
                    {
                        #region FileOpen V1.2
#if false
                    int nPos = 6;   // 앞의 6개는 'DMT1.2' 에 할당

                        #region Header

                        #region 타이틀(21)
                    byte[] byteGetData = new byte[21];
                    for (i = 0; i < 21; i++) byteGetData[i] = 0;
                    for (i = 0; i < 21; i++)
                    {
                        if (byteData[i + nPos] == 0) break;
                        byteGetData[i] = byteData[i + nPos];
                    }
                    txtTableName.Text = System.Text.Encoding.Default.GetString(byteGetData);
                    nPos += 21;
                    byteGetData = null;
                    #endregion 타이틀(21)

                        #region Start Position(1)
                    int nStartPosition = (int)(byteData[nPos++]);
                    nStartPosition = (nStartPosition >= 0) ? nStartPosition : 0;
                    cmbStartPosition.SelectedIndex = (((cmbStartPosition.Items.Count > nStartPosition) && (nStartPosition > 0)) ? nStartPosition : 0);
                    #endregion Start Position(1)

                        #region Size - MotionFrame(2), Comment(2), Caption(2), PlayTime(4), RobotModelNumber(2), MotorCnt(1), Motor Index(MC), Mirror Index(MC)
                    // Size
                    int nFrameSize, nCnt_LineComment, nPlayTime, nCommentSize, nRobotModelNum, nMotorCnt;
                    nFrameSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                    nCommentSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                    nCnt_LineComment = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                    nPlayTime = (int)(byteData[nPos] + byteData[nPos + 1] * 256 + byteData[nPos + 2] * 256 * 256 + byteData[nPos + 3] * 256 * 256 * 256); nPos += 4;
                    nRobotModelNum = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                    nMotorCnt = (int)(byteData[nPos++]);

                    // 모터의 인덱스
                    byte[] pbyteMotorIndex = new byte[nMotorCnt];
                    for (int nIndex = 0; nIndex < nMotorCnt; nIndex++) pbyteMotorIndex[nIndex] = byteData[nPos++];

                    // 모터의 Mirror 인덱스
                    byte[] pbyteMirrorIndex = new byte[nMotorCnt];
                    for (int nIndex = 0; nIndex < nMotorCnt; nIndex++) pbyteMirrorIndex[nIndex] = byteData[nPos++];

                    #endregion Size - MotionFrame(2), Comment(2), Caption(2), PlayTime(4), RobotModelNumber(2), MotorCnt(1), Motor Index(MC), Mirror Index(MC)

                    #endregion Header

                    // nRobotModelNum 를 읽고 해당 파일을 읽어들인다.
                        #region Header 검증
                    if (nMotorCnt != m_pCHeader[m_nCurrentRobot].nMotorCnt)
                    {
                        if (bFile == true)
                        {
                            fs.Close();
                            f = null;
                        }
                        this.Cursor = System.Windows.Forms.Cursors.Default;
                        MessageBox.Show("디자이너 파일의 모터 수량과 맞지 않습니다.(요구모터수량=" + CConvert.IntToStr(m_pCHeader[m_nCurrentRobot].nMotorCnt) + ", 모션파일에 정의된 모터수량=" + CConvert.IntToStr(nMotorCnt) + ")\n");// 해당 모델에 맞는 모션을 로드하십시오.");
                        DialogResult dlgRet = MessageBox.Show("무시하고 계속 열겠습니까?", "파일열기", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                        if (dlgRet == DialogResult.OK)
                        {
                            //MessageBox.Show("Yes");
                            //return;
                        }
                        else return false;
                    }
                    #endregion Header 검증

                    //Grid_ChangePos(dgAngle, 0, 0);
                    //Grid_ChangePos(dgKinematics, 0, 0);
                    //GridInit(nMotorCnt, nFrameSize, false);// + 50);
                    //GridInit(nMotorCnt, _SIZE_FRAME, false);

                    for (i = nFrameSize; i < dgAngle.RowCount - nFrameSize; i++) Grid_Clear(i);

                        #region 실제 모션
                    int nH = nFrameSize;
                    int nData;
                    short sData;
                    float fValue;
                    for (j = 0; j < nH; j++)
                    {
                        //En
                        #region Enable
                        int nEn = byteData[nPos++];
                        bool bEn = ((nEn & 0x01) != 0) ? true : false;
                        Grid_SetEnable(j, bEn);
                        #endregion Enable
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        #region Motor
                        int nMotorCntMax = (int)Math.Max(nMotorCnt, m_pCHeader[m_nCurrentRobot].nMotorCnt);
                        // 0-Index, 1-En, 2 ~ 24, 25 - speed, 26 - delay, 27,28,29,30 - Data0-3, 31 - time, 32 - caption
                        for (int nAxis = 0; nAxis < nMotorCntMax; nAxis++)
                        {
                            if (nAxis >= m_pCHeader[m_nCurrentRobot].nMotorCnt) nPos += 3;
                            else if (nAxis >= nMotorCnt) Grid_SetMot(j, nAxis, 0.0f);// 실 모터수량과 맞지 않다면 그 부분을 0 으로 채울 것
                            else
                            {
                                nData = (int)(BitConverter.ToInt16(byteData, nPos)); nPos += 2;
                                sData = (short)(nData & 0x3fff);
                                if ((nData & 0x4000) != 0) sData -= 0x1000;
                                // 엔코더 타입((0x8000) != 0)


                                ///////////////////////////
                                // Reserve(2), Noaction(1), LED(3-Red Blue Green), Mode(1), Stop Bit(1)
                                int byteTmp = byteData[nPos++];


                                ///////////////////////////












                                Grid_SetFlag_Led(j, nAxis, ((nData >> 12) & 0x07));
                                Grid_SetFlag_Type(j, nAxis, (((nData & 0x8000) != 0) ? true : false));
                                Grid_SetFlag_En(j, nAxis, ((sData == 0x7ff) ? false : true));

                                if (sData == 0x7ff)
                                {
                                    Grid_SetMot(j, nAxis, 0);
                                }
                                else
                                {
                                    fValue = CalcEvd2Angle(nAxis, (int)sData);
                                    Grid_SetMot(j, nAxis, fValue);
                                }



                                /* - Save
                                fValue = Grid_GetMot(i, j);
                                sData = (short)(OjwMotor.CalcAngle2Evd(j, fValue) & 0x03ff);
                                //sData |= 0x0400; // 속도모드인때 정(0-0x0000), 역(1-0x0400)
                                //sData |= LED;  // 00 - 0ff, 0x0800 - Red(01), 0x1000 - Blue(10), 0x1800 - Green(11)
                                //sData |= 제어타입 // 0 - 위치, 0x2000 - 속도
                                sData |= 0x4000; //Enable // 개별 Enable (0 - Disable, 0x4000 - Enable)
                                 */
                            }
                        }
                        #endregion Motor
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        #region Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                        // Speed  
                        nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        Grid_SetSpeed(j, nData);

                        // Delay  
                        nData = BitConverter.ToInt16(byteData, nPos); nPos += 2;
                        Grid_SetDelay(j, nData);

                        // Group  
                        nData = (int)(byteData[nPos++]);
                        Grid_SetGroup(j, nData);

                        // Command  
                        nData = (int)(byteData[nPos++]);
                        Grid_SetCommand(j, nData);

                        // Data0  
                        nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        Grid_SetData0(j, nData);
                        // Data1  
                        nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        Grid_SetData1(j, nData);
                        #endregion Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        #region 이산에서 추가한 Frame 위치 및 자세
                        SetFrame_X(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                        SetFrame_Y(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                        SetFrame_Z(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;

                        SetFrame_Pan(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                        SetFrame_Tilt(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                        SetFrame_Swing(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                        #endregion 이산에서 추가한 Frame 위치 및 자세
                    }
                    #endregion 실제 모션

#if !_COLOR_GRID_IN_PAINT
                    Grid_SetColorGrid(0, nFrameSize);
#endif
                    string strData_ME = "";
                    string strData_FE = "";

                    // 'M' 'E'
                    strData_ME += (char)(byteData[nPos++]);
                    strData_ME += (char)(byteData[nPos++]);

                        #region Comment Data
                    // Comment
                    byte[] pstrComment = new byte[nCommentSize];
                    for (j = 0; j < nCommentSize; j++)
                        pstrComment[j] = (byte)(byteData[nPos++]);
                    txtComment.Text = System.Text.Encoding.Default.GetString(pstrComment);
                    pstrComment = null;
                    #endregion Comment Data

                        #region Caption
                    int nLineNum = 0;
                    string strLineComment;
                    byte[] byLine = new byte[46];
                    for (j = 0; j < nCnt_LineComment; j++)
                    {
                        nLineNum = (short)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        for (int k = 0; k < 46; k++)
                            byLine[k] = (byte)(byteData[nPos++]);
                        strLineComment = System.Text.Encoding.Default.GetString(byLine);
                        strLineComment = strLineComment.Trim((char)0);
                        Grid_SetCaption(nLineNum, strLineComment);
                    }
                    byLine = null;
                    #endregion Caption

                    // 'T' 'E'
                    strData_FE += (char)(byteData[nPos++]);
                    strData_FE += (char)(byteData[nPos++]);

                    //                     if (bMessage == true)
                    //                     {
                    //                         if (strData_ME != "ME") OjwMessage("Motion Table Error\r\n");
                    //                         else OjwMessage("Table Loaded");
                    //                         if (strData_FE != "TE") OjwMessage("File Error\r\n");
                    //                         else OjwMessage("Table Loaded");
                    //                     }

                    pbyteMotorIndex = null;
                    pbyteMirrorIndex = null;

                    bFileOpened = true;
#endif
                        #endregion FileOpen V1.2
                    }
                    ////////////////////////////////////////////////////////////////////////////

                    if (bFile == true)
                    {
                        fs.Close();
                        f = null;
                    }
                    if (bFileOpened == true) return true;
                    return false;
                }
                catch
                {
                    if (bFile == true)
                    {
                        fs.Close();
                        f = null;
                    }
                    return false;
                }
            }
        }
        #endregion For Motor
    }
}
