using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CDynamixel
        {            
#if true
            #region Define
            // Address            

            // Model
            public const int _MODEL_NONE = 0;
            public const int _MODEL_XL_320 = 1; // 300도 1024 : 512 center : 0.111 RefRpm, 1023 Limit Rpm
            public const int _MODEL_XL_430 = 2; // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
            public const int _MODEL_AX_12 = 13; //
            public const int _MODEL_AX_18 = 14; //

            public readonly int _SIZE_MEMORY = 500;//1000;
            public readonly int _SIZE_MOTOR_MAX = 256;//254 + 1; // Bloadcasting 도 염두

            public readonly int _ID_BROADCASTING = 254;

            //public readonly int _FLAG_ENABLE 0x100


            public readonly int _HEADER1 = 0;
            public readonly int _HEADER2 = 1;
            public readonly int _HEADER3 = 2; // 0xFD
            public readonly int _RESERVED = 3; // 0x00
            public readonly int _ID = 4; // 0 ~ 252, 254 는 broadcast (253 - 0xfd, 255 - 0xff 는 사용 안함)
            public readonly int _SIZE_L = 5;
            public readonly int _SIZE_H = 6;
            public readonly int _CMD = 7; // instruction
            public readonly int _SIZE_PACKET_HEADER = 8;
            #endregion Define

            #region Var
            public SMotionTable_t m_SMotion_Pos = new SMotionTable_t();
            private bool[] m_abReceivedPos;// = new bool[_SIZE_MOTOR_MAX];

            private bool m_bProgEnd = false;
            public Ojw.CSerial m_CSerial = new CSerial();
            private byte[,] m_abyMem;

            private int[] m_anPos;
            private int[] m_anAxis_By_ID;


            private int m_nSeq_Receive;
            private int m_nSeq_Receive_Back;

            private int GetAxis_By_ID(int nID) { return (nID == 0xfe) ? 0xfe : m_anAxis_By_ID[nID]; }

            private bool m_bShowMessage;

            private int m_nSeq_Motor;
            private int m_nSeq_Motor_Back;
            private int m_nDelay;
            private CTimer m_CTmr = new CTimer();
            private CTimer m_CTmr_Timeout = new CTimer();
            private Thread m_thReceive;

            public CSocket m_CSocket = new CSocket();
            private Thread m_thSock;



            private int m_nModel;

            private int m_nTimeout;
            private bool m_bIgnoredLimit;

            private SRead_t[] m_aSRead;//[_SIZE_MOTOR_MAX];
            private int m_nReadCnt;
            private int m_nMotorCnt;
            private int m_nMotorCnt_Back;
            private SParam_Axis_t[] m_aSParam_Axis;//[256];
            private SMot_t[] m_aSMot_Prev;//[_SIZE_MOTOR_MAX];
            private SMot_t[] m_aSMot;//[_SIZE_MOTOR_MAX];
            private int[] m_anEn;//[_SIZE_MOTOR_MAX];
            private int m_nTorq = -1; // 
            private bool m_bStop;
            private bool m_bEms;
            private bool m_bMotionEnd;
            private bool m_bStart;
            #endregion Var

            private void Init()
            {
                m_abReceivedPos = new bool[_SIZE_MOTOR_MAX];

                m_abyMem = new byte[_SIZE_MOTOR_MAX, _SIZE_MEMORY];

                m_anPos = new int[_SIZE_MOTOR_MAX];
                //m_abyStatus1 = new byte[_SIZE_MOTOR_MAX];
                //m_abyStatus2 = new byte[_SIZE_MOTOR_MAX];
                m_anAxis_By_ID = new int[_SIZE_MOTOR_MAX];

                m_aSRead = new SRead_t[_SIZE_MOTOR_MAX];
                m_aSParam_Axis = new SParam_Axis_t[256];
                m_aSMot = new SMot_t[_SIZE_MOTOR_MAX];
                m_aSMot_Prev = new SMot_t[_SIZE_MOTOR_MAX];
                m_anEn = new int[_SIZE_MOTOR_MAX];

                m_bShowMessage = false;
                m_nTimeout = 200;
                m_nSeq_Motor = 0;
                m_nSeq_Motor_Back = 0;
                m_nDelay = 0;

                m_nTorq = -1;

                m_bIgnoredLimit = false;

                m_nModel = 0;

                m_nSeq_Receive = 0;

                //m_bOpen = false;
                m_bStop = false;
                m_bEms = false;
                m_bMotionEnd = false;
                m_bStart = false;

                m_nMotorCnt_Back = m_nMotorCnt = 0;


                Array.Clear(m_abyMem, 0, _SIZE_MOTOR_MAX * _SIZE_MEMORY);
                Array.Clear(m_aSParam_Axis, 0, _SIZE_MOTOR_MAX);
                Array.Clear(m_aSMot, 0, _SIZE_MOTOR_MAX);
                Array.Clear(m_aSMot_Prev, 0, _SIZE_MOTOR_MAX);

                //Array.Clear(m_abyStatus1, 0, _SIZE_MOTOR_MAX);
                //Array.Clear(m_abyStatus2, 0, _SIZE_MOTOR_MAX);
                Array.Clear(m_anAxis_By_ID, 0, _SIZE_MOTOR_MAX);

                Array.Clear(m_anEn, 0, _SIZE_MOTOR_MAX);
                Array.Clear(m_aSRead, 0, _SIZE_MOTOR_MAX);


                m_nReadCnt = 0;

                m_nSeq_Receive = 0;
                m_nSeq_Receive_Back = 0;


                // previous 추정을 위해 추가(SetParam 은 다른 용도로... -> 기존에 있던 코드)
                m_SMotion_Pos.abEn = new bool[_SIZE_MOTOR_MAX];
                m_SMotion_Pos.abType = new bool[_SIZE_MOTOR_MAX];
                m_SMotion_Pos.anLed = new int[_SIZE_MOTOR_MAX];
                m_SMotion_Pos.anMot = new int[_SIZE_MOTOR_MAX];
                for (int i = 0; i < _SIZE_MOTOR_MAX; i++)
                {
                    SetParam(i, _MODEL_XL_430);
                    m_SMotion_Pos.abEn[i] = false;
                    m_SMotion_Pos.abType[i] = false;
                    m_SMotion_Pos.anLed[i] = 0;
                    m_SMotion_Pos.anMot[i] = CalcAngle2Evd(i, CalcLimit_Angle(i, 0.0f));
                }

                m_bProgEnd = false;
            }
            public void Clone(out CDynamixel CMotor)
            {
                CMotor = new CDynamixel();
                Array.Copy(m_abReceivedPos, CMotor.m_abReceivedPos, m_abReceivedPos.Length);

                Array.Copy(m_abyMem, CMotor.m_abyMem, m_abyMem.Length);
                Array.Copy(m_anPos, CMotor.m_anPos, m_anPos.Length);
                //Array.Copy(m_abyStatus1, CMotor.m_abyStatus1, m_abyStatus1.Length);
                //Array.Copy(m_abyStatus2, CMotor.m_abyStatus2, m_abyStatus2.Length);

                Array.Copy(m_anAxis_By_ID, CMotor.m_anAxis_By_ID, m_anAxis_By_ID.Length);
                Array.Copy(m_aSRead, CMotor.m_aSRead, m_aSRead.Length);
                Array.Copy(m_aSParam_Axis, CMotor.m_aSParam_Axis, m_aSParam_Axis.Length);
                Array.Copy(m_aSMot, CMotor.m_aSMot, m_aSMot.Length);
                Array.Copy(m_aSMot_Prev, CMotor.m_aSMot_Prev, m_aSMot_Prev.Length);

                Array.Copy(m_anEn, CMotor.m_anEn, m_anEn.Length);

                CMotor.m_bShowMessage = m_bShowMessage;
                CMotor.m_nTimeout = m_nTimeout;
                CMotor.m_nSeq_Motor = m_nSeq_Motor;
                CMotor.m_nSeq_Motor_Back = m_nSeq_Motor_Back;
                CMotor.m_nDelay = m_nDelay;
                CMotor.m_bIgnoredLimit = m_bIgnoredLimit;
                CMotor.m_nModel = m_nModel;
                CMotor.m_nSeq_Receive = m_nSeq_Receive;
                CMotor.m_bStop = m_bStop;
                CMotor.m_bEms = m_bEms;
                CMotor.m_bMotionEnd = m_bMotionEnd;
                CMotor.m_bStart = m_bStart;
                CMotor.m_nMotorCnt = m_nMotorCnt;
                CMotor.m_nMotorCnt_Back = m_nMotorCnt_Back;

                CMotor.m_nReadCnt = m_nReadCnt;
                CMotor.m_nSeq_Receive = m_nSeq_Receive;
                CMotor.m_nSeq_Receive_Back = m_nSeq_Receive_Back;

                CMotor.m_bProgEnd = m_bProgEnd;
            }
            public void SetParam(int nAxis, int nRealID, int nDir, float fLimitUp, float fLimitDn, float fCenterPos, float fOffsetAngle_Display, float fMechMove, float fDegree)
            {
                //if ((nAxis >= _CNT_MAX_MOTOR) || (nID >= _MOTOR_MAX)) return false;

                m_aSParam_Axis[nAxis].nID = m_aSMot[nAxis].nID = m_aSMot_Prev[nAxis].nID = nRealID;
                m_aSParam_Axis[nAxis].nDir = m_aSMot[nAxis].nDir = m_aSMot_Prev[nAxis].nDir = nDir;
                m_aSParam_Axis[nAxis].fLimitUp = m_aSMot[nAxis].fLimitUp = m_aSMot_Prev[nAxis].fLimitUp = fLimitUp;
                m_aSParam_Axis[nAxis].fLimitDn = m_aSMot[nAxis].fLimitDn = m_aSMot_Prev[nAxis].fLimitDn = fLimitDn;
                m_aSParam_Axis[nAxis].fCenterPos = m_aSMot[nAxis].fCenterPos = m_aSMot_Prev[nAxis].fCenterPos = fCenterPos;
                m_aSParam_Axis[nAxis].fOffsetAngle_Display = fOffsetAngle_Display;
                m_aSParam_Axis[nAxis].fMechMove = m_aSMot[nAxis].fMechMove = m_aSMot_Prev[nAxis].fMechMove = fMechMove;
                m_aSParam_Axis[nAxis].fDegree = m_aSMot[nAxis].fDegree = m_aSMot_Prev[nAxis].fDegree = fDegree;
            }
            public int GetParam_RealID(int nAxis) { return m_aSParam_Axis[nAxis].nID; }
            public int GetParam_Axis_From_RealID(int nRealID) { for (int i = 0; i < 253; i++) if (nRealID == m_aSParam_Axis[i].nID) return i; return -1; }
            public void GetParam(int nAxis, out int nRealID, out int nDir, out float fLimitUp, out float fLimitDn, out float fCenterPos, out float fOffsetAngle_Display, out float fMechMove, out float fDegree)
            {
                //if ((nAxis >= _CNT_MAX_MOTOR) || (nID >= _MOTOR_MAX)) return false;

                nRealID = m_aSParam_Axis[nAxis].nID;
                nDir = m_aSParam_Axis[nAxis].nDir;
                fLimitUp = m_aSParam_Axis[nAxis].fLimitUp;
                fLimitDn = m_aSParam_Axis[nAxis].fLimitDn;
                fCenterPos = m_aSParam_Axis[nAxis].fCenterPos;
                fOffsetAngle_Display = m_aSParam_Axis[nAxis].fOffsetAngle_Display;
                fMechMove = m_aSParam_Axis[nAxis].fMechMove;
                fDegree = m_aSParam_Axis[nAxis].fDegree;
            }
            private bool m_bMultiTurn = false;
            public void SetParam(int nAxis, SParam_Axis_t SAxis)
            {
                if (nAxis >= _ID_BROADCASTING) nAxis = _ID_BROADCASTING;
                SetParam_RealID(nAxis, SAxis.nID);
                SetParam_Dir(nAxis, SAxis.nDir);
                SetParam_LimitUp(nAxis, SAxis.fLimitUp);
                SetParam_LimitDown(nAxis, SAxis.fLimitDn);
                SetParam_CenterEvdValue(nAxis, SAxis.fCenterPos);
                SetParam_Display(nAxis, SAxis.fOffsetAngle_Display);
                SetParam_MechMove(nAxis, SAxis.fMechMove);
                SetParam_Degree(nAxis, SAxis.fDegree);
                SetParam_Rpm(nAxis, SAxis.fRefRpm); // 기본 rpm 단위
                SetParam_LimitRpm_Raw(nAxis, SAxis.fLimitRpm);
                SetParam_ProtocolVersion(nAxis, SAxis.nProtocol_Version); // Version 2(0 해도 동일)
                SetParam_ModelNum(nAxis, SAxis.nModel); // 0번지에 모델번호 1060, XM430_W210 : 1030, XM430_W350 : 1020
                SetParam_Addr_Max(nAxis, SAxis.nAddr_Max);
                SetParam_Addr_Torq(nAxis, SAxis.nAddr_Torq);
                SetParam_Addr_Led(nAxis, SAxis.nAddr_Led);
                SetParam_Addr_Mode(nAxis, SAxis.nAddr_Mode); // 430 -> 10 address    [0 : 전류, 1 : 속도, 3(default) : 관절(위치제어), 4 : 확장위치제어(멀티턴:-256 ~ 256회전), 5 : 전류기반 위치제어, 16 : pwm 제어(voltage control mode)]
                SetParam_Addr_Pos_Speed(nAxis, SAxis.nAddr_Speed); // 430 -> 112 4 bytes
                SetParam_Addr_Pos_Speed_Size(nAxis, SAxis.nAddr_Speed_Size);
                SetParam_Addr_Pos(nAxis, SAxis.nAddr_Pos); // 430 -> 116 4 bytes
                SetParam_Addr_Pos_Size(nAxis, SAxis.nAddr_Pos_Size);
            }
            public void SetParam(int nModel) { for (int i = 0; i < _SIZE_MOTOR_MAX; i++) SetParam(i, nModel); }
            public void SetParam(int nAxis, int nModel)
            {
                if (nAxis >= _ID_BROADCASTING)
                {
                    nAxis = _ID_BROADCASTING;
                    for (int i = 0; i < _SIZE_MOTOR_MAX; i++)
                    {
                        m_aSMot[i].nControlMode = 0; // None
                        m_aSMot[i].nDriveMode = 0; // Rpm Based
                    }
                    m_nTorq = -1;
                }
                else
                {
                    m_aSMot[nAxis].nControlMode = 0; // None
                    m_nTorq = -1;
                }
                //Ojw.CMessage.Write("nAxis = {0}, nModel = {1}", nAxis, nModel);
                //if (nModel == _MODEL_XL_430) m_bMultiTurn = true;
                //else if (m_bMultiTurn == true) m_bMultiTurn = false;
                switch (nModel)
                {
                    case _MODEL_XL_430:
                        SetParam_RealID(nAxis, nAxis);
                        //SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 2048.0f);
                        SetParam_Display(nAxis, 0.0f);
                        SetParam_MechMove(nAxis, 4096.0f);
                        SetParam_Degree(nAxis, 360.0f);
                        SetParam_Rpm(nAxis, 0.229f); // 기본 rpm 단위
                        SetParam_LimitRpm_Raw(nAxis, 415);//480);
                        SetParam_ProtocolVersion(nAxis, 2); // Version 2(0 해도 동일)
                        SetParam_ModelNum(nAxis, 1060); // 0번지에 모델번호 1060, XM430_W210 : 1030, XM430_W350 : 1020
                        SetParam_Addr_Max(nAxis, 146);
                        SetParam_Addr_Torq(nAxis, 64);
                        SetParam_Addr_Led(nAxis, 65);
                        SetParam_Addr_Mode(nAxis, 10); // 430 -> 10 address    [0 : 전류, 1 : 속도, 3(default) : 관절(위치제어), 4 : 확장위치제어(멀티턴:-256 ~ 256회전), 5 : 전류기반 위치제어, 16 : pwm 제어(voltage control mode)]
                        SetParam_Addr_Speed(nAxis, 104); // 430 -> 104 4 bytes
                        SetParam_Addr_Speed_Size(nAxis, 4);
                        SetParam_Addr_Pos_Speed(nAxis, 112); // 430 -> 112 4 bytes
                        SetParam_Addr_Pos_Speed_Size(nAxis, 4);
                        SetParam_Addr_Pos(nAxis, 116); // 430 -> 116 4 bytes
                        SetParam_Addr_Pos_Size(nAxis, 4);
                        
                        break;
                    case _MODEL_XL_320: // 이 모델은 속도제어의 경우 10 번째 비트가 음의 Direction 을 결정
                        SetParam_RealID(nAxis, nAxis);
                        //SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 512.0f);
                        SetParam_Display(nAxis, 0.0f);
                        SetParam_MechMove(nAxis, 1024.0f);
                        SetParam_Degree(nAxis, 300.0f);
                        SetParam_Rpm(nAxis, 0.111f); // 기본 rpm 단위
                        SetParam_LimitRpm_Raw(nAxis, 1023);
                        SetParam_ProtocolVersion(nAxis, 2); // Version 2
                        SetParam_ModelNum(nAxis, 350); // 0번지에 모델번호 350
                        SetParam_Addr_Max(nAxis, 52);
                        SetParam_Addr_Torq(nAxis, 24);
                        SetParam_Addr_Led(nAxis, 25);
                        SetParam_Addr_Mode(nAxis, 11); // 320 -> 11            [1 : 속도, 2(default) : 관절]
                        SetParam_Addr_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Speed_Size(nAxis, 2);                        
                        SetParam_Addr_Pos_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Pos_Speed_Size(nAxis, 2);                        
                        SetParam_Addr_Pos(nAxis, 30); // 320 -> 30 2 bytes
                        SetParam_Addr_Pos_Size(nAxis, 2);
                        break;
                    case _MODEL_AX_18: 
                    case _MODEL_AX_12: // Protocol 1 : 이 모델은 속도제어의 경우 10 번째 비트가 음의 Direction 을 결정
                        SetParam_RealID(nAxis, nAxis);
                        //SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 512.0f);
                        SetParam_Display(nAxis, 0.0f);
                        SetParam_MechMove(nAxis, 1024.0f);
                        SetParam_Degree(nAxis, 300.0f);
                        SetParam_Rpm(nAxis, 0.111f); // 기본 rpm 단위
                        SetParam_LimitRpm_Raw(nAxis, 1023);
                        SetParam_ProtocolVersion(nAxis, 1); // Version 1
                        SetParam_ModelNum(nAxis, 12); // 0번지에 모델번호 12
                        SetParam_Addr_Max(nAxis, 52);
                        SetParam_Addr_Torq(nAxis, 24);
                        SetParam_Addr_Led(nAxis, 25);
                        SetParam_Addr_Mode(nAxis, 11); // 320 -> 11            [1 : 속도, 2(default) : 관절]
                        SetParam_Addr_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Pos_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos(nAxis, 30); // 320 -> 30 2 bytes
                        SetParam_Addr_Pos_Size(nAxis, 2);
                        break;
                }
            }

            public void SetParam_RealID(int nAxis, int nRealID) { m_aSParam_Axis[nAxis].nID = m_aSMot[nAxis].nID = nRealID; m_anAxis_By_ID[nRealID] = nAxis; }
            public void SetParam_Dir(int nAxis, int nDir) { m_aSParam_Axis[nAxis].nDir = m_aSMot[nAxis].nDir = nDir; }
            public void SetParam_LimitUp(int nAxis, float fLimitUp) { m_aSParam_Axis[nAxis].fLimitUp = m_aSMot[nAxis].fLimitUp = fLimitUp; }
            public void SetParam_LimitDown(int nAxis, float fLimitDn) { m_aSParam_Axis[nAxis].fLimitDn = m_aSMot[nAxis].fLimitDn = fLimitDn; }
            public void SetParam_CenterEvdValue(int nAxis, float fCenterPos) { m_aSParam_Axis[nAxis].fCenterPos = m_aSMot[nAxis].fCenterPos = fCenterPos; }
            public void SetParam_Display(int nAxis, float fOffsetAngle_Display) { m_aSParam_Axis[nAxis].fOffsetAngle_Display = fOffsetAngle_Display; }
            public void SetParam_MechMove(int nAxis, float fMechMove) { m_aSParam_Axis[nAxis].fMechMove = m_aSMot[nAxis].fMechMove = fMechMove; }
            public void SetParam_Degree(int nAxis, float fDegree) { m_aSParam_Axis[nAxis].fDegree = m_aSMot[nAxis].fDegree = fDegree; }
            public void SetParam_Rpm(int nAxis, float fRpm) { m_aSParam_Axis[nAxis].fRefRpm = m_aSMot[nAxis].fRefRpm = fRpm; }
            public void SetParam_LimitRpm_Raw(int nAxis, float fLimitRpm) { m_aSParam_Axis[nAxis].fLimitRpm = m_aSMot[nAxis].fLimitRpm = fLimitRpm; }
            // 0 : none(== version 2), 1 : version 1, 2 : version 2
            public void SetParam_ProtocolVersion(int nAxis, int nProtocol_Version) 
            {
                m_nProtocolVersion = nProtocol_Version; // 일단은 어떤 모터든 변경하면 그게 적용되도록 한다. 나중에는 모터 개별적으로 대응되도록 할 것
                m_aSParam_Axis[nAxis].nProtocol_Version = m_aSMot[nAxis].nProtocol_Version = nProtocol_Version; 
            }
            public void SetParam_HwMotorName(int nAxis, int nHwMotor_Index) { m_aSParam_Axis[nAxis].nHwMotor_Index = m_aSMot[nAxis].nHwMotor_Index = nHwMotor_Index; }
            // 430 -> 146 (0번지에 모델번호 1060, XM430_W210 : 1030, XM430_W350 : 1020)
            // 320 -> 52 (0번지에 모델번호 350)
            public void SetParam_ModelNum(int nAxis, int nModel) { m_aSParam_Axis[nAxis].nModel = m_aSMot[nAxis].nModel = nModel; }
            // 430 -> 64 address
            // 320 -> 24
            public void SetParam_Addr_Max(int nAxis, int nAddr_Max) { m_aSParam_Axis[nAxis].nAddr_Max = m_aSMot[nAxis].nAddr_Max = nAddr_Max; }
            public void SetParam_Addr_Torq(int nAxis, int nAddr_Torq) { m_aSParam_Axis[nAxis].nAddr_Torq = m_aSMot[nAxis].nAddr_Torq = nAddr_Torq; }
            // Torq + 1 번지
            public void SetParam_Addr_Led(int nAxis, int nAddr_Led) { m_aSParam_Axis[nAxis].nAddr_Led = m_aSMot[nAxis].nAddr_Led = nAddr_Led; }
            // 430 -> 10 address    [0 : 전류, 1 : 속도, 3(default) : 관절(위치제어), 4 : 확장위치제어(멀티턴:-256 ~ 256회전), 5 : 전류기반 위치제어, 16 : pwm 제어(voltage control mode)]
            // 320 -> 11            [1 : 속도, 2(default) : 관절]
            public void SetParam_Addr_Mode(int nAxis, int nAddr_Mode) { m_aSParam_Axis[nAxis].nAddr_Mode = m_aSMot[nAxis].nAddr_Mode = nAddr_Mode; }

            // 430 -> 104 4 bytes
            // 320 -> 32 2 bytes
            public void SetParam_Addr_Speed(int nAxis, int nAddr_Speed) { m_aSParam_Axis[nAxis].nAddr_Speed = m_aSMot[nAxis].nAddr_Speed = nAddr_Speed; }
            public void SetParam_Addr_Speed_Size(int nAxis, int nAddr_Speed_Size) { m_aSParam_Axis[nAxis].nAddr_Speed_Size = m_aSMot[nAxis].nAddr_Speed_Size = nAddr_Speed_Size; }
            
            
            
            // 430 -> 108 4 bytes
            // 320 -> 32 2 bytes
            public void SetParam_Addr_Pos_Speed(int nAxis, int nAddr_Pos_Speed) { m_aSParam_Axis[nAxis].nAddr_Pos_Speed = m_aSMot[nAxis].nAddr_Pos_Speed = nAddr_Pos_Speed; }
            public void SetParam_Addr_Pos_Speed_Size(int nAxis, int nAddr_Pos_Speed_Size) { m_aSParam_Axis[nAxis].nAddr_Pos_Speed_Size = m_aSMot[nAxis].nAddr_Pos_Speed_Size = nAddr_Pos_Speed_Size; }
            // 430 -> 116 4 bytes
            // 320 -> 30 2 bytes
            public void SetParam_Addr_Pos(int nAxis, int nAddr_Pos) { m_aSParam_Axis[nAxis].nAddr_Pos = m_aSMot[nAxis].nAddr_Pos = nAddr_Pos; }
            public void SetParam_Addr_Pos_Size(int nAxis, int nAddr_Pos_Size) { m_aSParam_Axis[nAxis].nAddr_Pos_Size = m_aSMot[nAxis].nAddr_Pos_Size = nAddr_Pos_Size; }
                        
            public void Stop(int nAxis) // no stop flag setting
            {
                //	if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                //Set_Turn(nAxis, 0);
                //	Set_Flag_Stop(nAxis, true);
                //Send_Motor(1000);
                //SetTorque(false);
                byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                Write(nAxis, 104, pbyTmp);
            }
            public void Stop()
            {
                //SetTorque(false);
                //Set_Turn(254, 0);

                // 속도
                byte [] pbyTmp = Ojw.CConvert.IntToBytes(0);
                Write(0xfe, 104, pbyTmp);
                //Send_Motor(100);
                m_bStop = true;
            }
            public void Ems()
            {
                Stop();
                SetTorque(false);
                m_bEms = true;
            }
            //public void Reset() { Reset(_ID_BROADCASTING); }
            public void Reset()//(int nAxis)
            {
                // Clear Variable
                m_bStop = false;
                m_bEms = false;

                //int nID = m_aSMot[nAxis].nID;
                //int i = 0;
                //byte[] pbyteBuffer = new byte[3];
                //// Data
                //pbyteBuffer[i++] = (byte)(48 + ((m_bMultiTurn == true) ? 4 : 0)); //48;// 48번 레지스터 명령
                //////////
                //pbyteBuffer[i++] = 0x01;// 이후의 레지스터 사이즈
                //pbyteBuffer[i++] = 0x00; // led value

                //Make_And_Send_Packet(nID, 0x03, i, pbyteBuffer);
                ////pbyteBuffer = null;
            }

            public bool IsOpen() { return m_CSerial.IsConnect(); }
            public bool IsStop() { return m_bStop; }
            public bool IsEms() { return m_bEms; }
            public bool Open(int nComport, int nBaudrate)//, int nModel)		//serial port open
            {
                if (IsOpen() == false)
                {
                    // Port Open                    
                    if (m_CSerial.Connect(nComport, nBaudrate) == false)
                    {
                        Ojw.CMessage.Write("Failed to open Serial Device{Comport:{0}, Baudrate:{1})", nComport, nBaudrate);
                        return false;
                    }
                    Ojw.CMessage.Write("Init Serial{0}", nComport);

                    // Thread
#if true
                    m_thReceive = new Thread(new ThreadStart(Thread_Receive));
                    m_thReceive.Start();
                    Ojw.CMessage.Write("Init Thread");

                    Clear_Flag();
#endif
                }

                if (IsOpen() == false)
                {
                    Ojw.CMessage.Write("[Open()][Error] Connection Fail - Comport {0}, {1}", nComport, nBaudrate);
                }
                return IsOpen();
            }
            public void Close()								//serial port close
            {
                if (IsOpen() == true)
                {
                    m_CSerial.DisConnect();
                    Ojw.CMessage.Write("Serial Port -> Closed");
                }
            }
            private void Thread_Receive()
            {
                byte[] buf;// = new char[256];
                Ojw.CMessage.Write("[Thread_Receive] Running Thread");
                while ((m_bProgEnd == false) && (IsOpen() == true))
                {
                    int nSize = m_CSerial.GetBuffer_Length();
                    if (nSize > 0)
                    {
                        buf = m_CSerial.GetBytes();
                        //Ojw.CMessage.Write("[Receive]");


                        if (m_nProtocolVersion == 1)
                        {
                            //continue;
                            Parsor1(buf, nSize);
                        }
                        else // (m_aSParam_Axis[nAxis].nProtocol_Version == 2) 
                            Parsor(buf, nSize);

                        //Ojw.CMessage.Write("");
                    }
                    Thread.Sleep(1);
                }

                Ojw.CMessage.Write("[Thread_Receive] Closed Thread");
            }
            //private int m_nIndex = 0;
            //private byte m_byCheckSum = 0;
            //private byte m_byCheckSum1 = 0;
            //private byte m_byCheckSum2 = 0;
            //private int m_nPacketSize = 0;
            //private int m_nDataLength = 0;
            //private int m_nIndexData = 0;
            //private int m_nCmd = 0;
            //private int m_nId = 0;
            //private int m_nData_Address = 0;
            //private int m_nData_Length = 0;
            //private bool m_bHeader = false;
            //private int m_nPack_Address = -1;
            private int m_nPack_ID = 0;
            private int m_nPack_Length = -1;
            private int m_nPack_Command = 0;
            private int m_nPack_Error = 0;
            private int m_nPack_CRC0 = 0;
            private int m_nPack_CRC1 = 0;
            private int m_nIndex = 0;
            private int m_nIndex2 = 0;
            private uint m_unHeader = 0;
            private void Parsor1(byte[] buf, int nSize)
            {

                for (int i = 0; i < nSize; i++)
                {
#if true
                    if (m_unHeader == 0xffff)
                    {
                        m_nIndex = 1;
                        m_nPack_Length = -1;
                        m_nIndex2 = 0;
                    }
                    m_unHeader = (((m_unHeader << 8) & 0xffff) | buf[i]);

                    switch (m_nIndex)
                    {
                        case 1: // 
                            {
                                m_nPack_ID = GetAxis_By_ID(buf[i]);//buf[i];
                                m_nIndex++;
                            }
                            break;
                        case 2: // Length
                            {
                                m_nPack_Length = buf[i];
                                if (m_nPack_Length >= m_abyMem.GetLength(1))
                                {
                                    // Packet Error
                                    break;
                                }
                                m_nIndex++;
                            }
                            break;
                        case 3:
                            {
                                m_nPack_Error = buf[i];
                                m_nIndex++;
                            }
                            break;
                        case 4:
                            {
                                int nAddress = 0;// ((m_nPack_Address < 0) ? 128 : m_nPack_Address);
                                m_abyMem[m_nPack_ID, nAddress + m_nIndex2] = buf[i];

                                if (m_nIndex2 < m_nPack_Length - 2 - 1) m_nIndex2++;
                                else m_nIndex++;
                            }
                            break;
                        case 6: // crc
                            {
                                m_nPack_CRC0 = buf[i];
                                m_nIndex = 0;

                                m_abReceivedPos[m_nPack_ID] = true;
                                m_nSeq_Receive++;

                                // 
                                //m_aSMot_Prev[m_nPack_ID].fPos = Get_Pos_Evd(m_nPack_ID);
#if true                                
                                Ojw.CMessage.Write("ID={0}, Length={1}, Pos={2}", m_nPack_ID, m_nPack_Length, Get_Pos_Angle(m_nPack_ID));                                
#endif
                            }
                            break;
                    }
                    //if (i == 0) Ojw.CMessage.Write2("=========>\r\n");
                    //Ojw.CMessage.Write2("({0})", Ojw.CConvert.IntToHex(buf[i]));
#else
                    #region Received
                    switch (m_nIndex)
                    {
#if true
                        case 0:
                            if (((m_bMultiTurn == true) && ((m_nIndex == 0) && (buf[i] == 0xff))) || ((m_bMultiTurn == false) && (buf[i] == 0xff)))
                            {
                                m_byCheckSum = 0;
                                m_nIndexData = 0;

                                m_nData_Address = 0;
                                m_nData_Length = 0;

                                m_nIndex++;
                            }
                            break;
#endif
                        case 1:
                            if (buf[i] == 0xff) m_nIndex++;
                            else m_nIndex = 0;//-1;
                            break;
                        case 2: // Packet Size
                            m_nPacketSize = buf[i];
                            m_nDataLength = m_nPacketSize - _SIZE_PACKET_HEADER - 2;
                            m_byCheckSum = buf[i];
                            if (m_nDataLength < 0) m_nIndex = 0;
                            else m_nIndex++;
                            break;
                        case 3: // ID
                            m_nId = GetAxis_By_ID(buf[i]);
                            m_byCheckSum ^= buf[i];
                            m_nIndex++;
                            break;
                        case 4: // Cmd
                            m_nCmd = buf[i];
                            m_byCheckSum ^= buf[i];
                            m_nIndex++;
                            break;
                        case 5: // CheckSum1
                            m_byCheckSum1 = buf[i];
                            m_nIndex++;
                            break;
                        case 6: // CheckSum2
                            m_byCheckSum2 = buf[i];
                            if ((~m_byCheckSum1 & 0xfe) != m_byCheckSum2) m_nIndex = 0;
                            else m_nIndex++;
                            break;
                        case 7: // Datas...
                            //Ojw.CMessage.Write("[DataLength={0}/{1}]", nIndexData, nDataLength);
                            if (m_nIndexData < m_nDataLength)
                            {
                                if (m_nIndexData == 0) m_nData_Address = buf[i];
                                else if (m_nIndexData == 1) m_nData_Length = buf[i];
                                else m_abyRam[m_nId, m_nData_Address + m_nIndexData - 2] = buf[i];




                                if (++m_nIndexData >= m_nDataLength) m_nIndex++;

                                m_byCheckSum ^= buf[i];
                            }
                            else
                            {
                                //Ojw.CMessage.Write("====== Status1=======");
                                m_abyStatus1[m_nId] = buf[i];
                                m_byCheckSum ^= buf[i];
                                m_nIndex += 2;
                            }
                            break;
                        case 8: // Status 1		
                            //Ojw.CMessage.Write("====== Status1=======");
                            m_abyStatus1[m_nId] = buf[i];
                            m_byCheckSum ^= buf[i];
                            m_nIndex++;


                            /*
                                                    0x01 : Exceed Input Voltage Limit
                                                    0x02 : Exceed allowed POT limit
                                                    0x04 : Exceed Temperature limit
                                                    0x08 : Invalid Packet
                                                    0x10 : Overload detected
                                                    0x20 : Driver fault detected
                                                    0x40 : EEP REG distorted
                                                    0x80 : reserved
                            */

                            break;
                        case 9: // Status 2		
                            //Ojw.CMessage.Write("====== Status2=======(Chk: 0x%02X , 0x%02X", byCheckSum, byCheckSum1);
                            m_abyStatus2[m_nId] = buf[i];
                            m_byCheckSum ^= buf[i];

                            /*
                                                    0x01 : Moving flag
                                                    0x02 : Inposition flag
                                                    0x04 : Checksum Error
                                                    0x08 : Unknown Command
                                                    0x10 : Exceed REG range
                                                    0x20 : Garbage detected
                                                    0x40 : MOTOR_ON flag
                                                    0x80 : reserved
                            */


                            ///////////////////
                            // Done
                            if ((m_byCheckSum & 0xFE) == m_byCheckSum1)
                            {
                                // test
                                byte[] abyteData = new byte[2];
                                abyteData[0] = (byte)(m_abyRam[m_nId, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0)] & 0xff);
                                abyteData[1] = (byte)(m_abyRam[m_nId, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0) + 1] & 0xff);
                                // 0000 0000  0000 0000
                                m_anPos[m_nId] = BitConverter.ToInt16(abyteData, 0);
                                //m_anPos[m_nId] = (short)(((abyteData[0] & 0x0f) << 8) | (abyteData[1] << 0) | ((abyteData[0] & 0x10) << (3 + 8)) | ((abyteData[0] & 0x10) << (2 + 8)) | ((abyteData[0] & 0x10) << (1 + 8)));

                                if (m_bShowMessage == true) Ojw.CMessage.Write("Data Received(<Address({0})Length({1})>Pos[{2}]={3}, Status1 = {4}, Status2 = {5})", m_nData_Address, m_nDataLength, m_nId, m_anPos[m_nId], m_abyStatus1[m_nId], m_abyStatus2[m_nId]);

                                m_abReceivedPos[m_nId] = true;
                                m_nSeq_Receive++;
                            }

                            m_nIndex = 0;
                            break;
                    }
                    #endregion Received
#endif
                }
            }
            private void Parsor(byte[] buf, int nSize)
            {

                for (int i = 0; i < nSize; i++)
                {
#if true
                    if (m_unHeader ==  0xfffffd00)
                    {
                        m_nIndex = 1;
                        m_nPack_Length = -1;
                        m_nIndex2 = 0;
                    }
                    m_unHeader = (((m_unHeader << 8) & 0xffffffff) | buf[i]);
                    
                    switch(m_nIndex)
                    {
                        case 1 : // 
                            {
                                m_nPack_ID = GetAxis_By_ID(buf[i]);//buf[i];
                                m_nIndex++;
                            }
                            break;
                        case 2: // Length
                            {
                                if (m_nPack_Length < 0)
                                {
                                    m_nPack_Length = buf[i];
                                }
                                else
                                {
                                    m_nPack_Length |= ((buf[i] << 8) & 0xff00);
                                    m_nIndex++;
                                }

                                if (m_nPack_Length > _SIZE_MEMORY)
                                {
                                    m_nIndex = 0;
                                    m_nSeq_Receive++; // 에러라고 해주어야 하지만 일단 그냥 가자.
                                }
                                // 메모리가 설마 256 바이트를 넘지는 않겠지?
                            }
                            break;
                        case 3: // Command : Instruction
                            {
                                m_nPack_Command = buf[i];
                                m_nIndex++;
                            }
                            break;
                        case 4:
                            {
                                m_nPack_Error = buf[i];
                                m_nIndex++;
                            }
                            break;
                        case 5:
                            {
                                int nAddress = 0;// ((m_nPack_Address < 0) ? 128 : m_nPack_Address);
                                m_abyMem[m_nPack_ID, nAddress + m_nIndex2] = buf[i];

                                if (m_nIndex2 < m_nPack_Length - 4 - 1) m_nIndex2++;
                                else m_nIndex++;
                            }
                            break;
                        case 6: // crc
                            {
                                m_nPack_CRC0 = buf[i];
                                m_nIndex++;
                            }
                            break;
                        case 7: // crc
                            {
                                m_nPack_CRC1 = buf[i];
                                m_nIndex = 0;

                                m_abReceivedPos[m_nPack_ID] = true;
                                m_nSeq_Receive++;

                                // 
                                //m_aSMot_Prev[m_nPack_ID].fPos = Get_Pos_Evd(m_nPack_ID);
#if false                                
                                Ojw.CMessage.Write("ID={0}, Length={1}, Pos={2}", m_nPack_ID, m_nPack_Length, Get_Pos_Angle(m_nPack_ID));                                
#endif
                            }
                            break;
                    }
                    //if (i == 0) Ojw.CMessage.Write2("=========>\r\n");
                    //Ojw.CMessage.Write2("({0})", Ojw.CConvert.IntToHex(buf[i]));
#else
                    #region Received
                    switch (m_nIndex)
                    {
#if true
                        case 0:
                            if (((m_bMultiTurn == true) && ((m_nIndex == 0) && (buf[i] == 0xff))) || ((m_bMultiTurn == false) && (buf[i] == 0xff)))
                            {
                                m_byCheckSum = 0;
                                m_nIndexData = 0;

                                m_nData_Address = 0;
                                m_nData_Length = 0;

                                m_nIndex++;
                            }
                            break;
#endif
                        case 1:
                            if (buf[i] == 0xff) m_nIndex++;
                            else m_nIndex = 0;//-1;
                            break;
                        case 2: // Packet Size
                            m_nPacketSize = buf[i];
                            m_nDataLength = m_nPacketSize - _SIZE_PACKET_HEADER - 2;
                            m_byCheckSum = buf[i];
                            if (m_nDataLength < 0) m_nIndex = 0;
                            else m_nIndex++;
                            break;
                        case 3: // ID
                            m_nId = GetAxis_By_ID(buf[i]);
                            m_byCheckSum ^= buf[i];
                            m_nIndex++;
                            break;
                        case 4: // Cmd
                            m_nCmd = buf[i];
                            m_byCheckSum ^= buf[i];
                            m_nIndex++;
                            break;
                        case 5: // CheckSum1
                            m_byCheckSum1 = buf[i];
                            m_nIndex++;
                            break;
                        case 6: // CheckSum2
                            m_byCheckSum2 = buf[i];
                            if ((~m_byCheckSum1 & 0xfe) != m_byCheckSum2) m_nIndex = 0;
                            else m_nIndex++;
                            break;
                        case 7: // Datas...
                            //Ojw.CMessage.Write("[DataLength={0}/{1}]", nIndexData, nDataLength);
                            if (m_nIndexData < m_nDataLength)
                            {
                                if (m_nIndexData == 0) m_nData_Address = buf[i];
                                else if (m_nIndexData == 1) m_nData_Length = buf[i];
                                else m_abyRam[m_nId, m_nData_Address + m_nIndexData - 2] = buf[i];




                                if (++m_nIndexData >= m_nDataLength) m_nIndex++;

                                m_byCheckSum ^= buf[i];
                            }
                            else
                            {
                                //Ojw.CMessage.Write("====== Status1=======");
                                m_abyStatus1[m_nId] = buf[i];
                                m_byCheckSum ^= buf[i];
                                m_nIndex += 2;
                            }
                            break;
                        case 8: // Status 1		
                            //Ojw.CMessage.Write("====== Status1=======");
                            m_abyStatus1[m_nId] = buf[i];
                            m_byCheckSum ^= buf[i];
                            m_nIndex++;


                            /*
                                                    0x01 : Exceed Input Voltage Limit
                                                    0x02 : Exceed allowed POT limit
                                                    0x04 : Exceed Temperature limit
                                                    0x08 : Invalid Packet
                                                    0x10 : Overload detected
                                                    0x20 : Driver fault detected
                                                    0x40 : EEP REG distorted
                                                    0x80 : reserved
                            */

                            break;
                        case 9: // Status 2		
                            //Ojw.CMessage.Write("====== Status2=======(Chk: 0x%02X , 0x%02X", byCheckSum, byCheckSum1);
                            m_abyStatus2[m_nId] = buf[i];
                            m_byCheckSum ^= buf[i];

                            /*
                                                    0x01 : Moving flag
                                                    0x02 : Inposition flag
                                                    0x04 : Checksum Error
                                                    0x08 : Unknown Command
                                                    0x10 : Exceed REG range
                                                    0x20 : Garbage detected
                                                    0x40 : MOTOR_ON flag
                                                    0x80 : reserved
                            */


                            ///////////////////
                            // Done
                            if ((m_byCheckSum & 0xFE) == m_byCheckSum1)
                            {
                                // test
                                byte[] abyteData = new byte[2];
                                abyteData[0] = (byte)(m_abyRam[m_nId, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0)] & 0xff);
                                abyteData[1] = (byte)(m_abyRam[m_nId, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0) + 1] & 0xff);
                                // 0000 0000  0000 0000
                                m_anPos[m_nId] = BitConverter.ToInt16(abyteData, 0);
                                //m_anPos[m_nId] = (short)(((abyteData[0] & 0x0f) << 8) | (abyteData[1] << 0) | ((abyteData[0] & 0x10) << (3 + 8)) | ((abyteData[0] & 0x10) << (2 + 8)) | ((abyteData[0] & 0x10) << (1 + 8)));

                                if (m_bShowMessage == true) Ojw.CMessage.Write("Data Received(<Address({0})Length({1})>Pos[{2}]={3}, Status1 = {4}, Status2 = {5})", m_nData_Address, m_nDataLength, m_nId, m_anPos[m_nId], m_abyStatus1[m_nId], m_abyStatus2[m_nId]);

                                m_abReceivedPos[m_nId] = true;
                                m_nSeq_Receive++;
                            }

                            m_nIndex = 0;
                            break;
                    }
                    #endregion Received
#endif
                }
            }
            public CDynamixel() // 생성자 초기화 함수
            {
                Init();
            }
            ~CDynamixel()
            {
                m_bProgEnd = true;
                Ojw.CTimer.Wait(100);
                if (IsOpen() == true) Close();
            }
            public void Clear_Flag() { for (int i = 0; i < 256; i++) { m_aSMot[i].nFlag = m_aSMot_Prev[i].nFlag = 0; m_aSMot[i].nLed = m_aSMot_Prev[i].nLed = 0; } }
            public void Clear_Flag(int nAxis) { m_aSMot[nAxis].nFlag = m_aSMot_Prev[nAxis].nFlag = 0; m_aSMot[nAxis].nLed = m_aSMot_Prev[nAxis].nLed = 0; }

            public void Set_Flag_Mode(int nAxis, int nMode_0Pos_1Spd_2Torq) { m_aSMot[nAxis].nFlag = ((m_aSMot[nAxis].nFlag & 0xff) | (nMode_0Pos_1Spd_2Torq & 0xff)); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public int Get_Flag_Mode(int nAxis) { return (m_aSMot[nAxis].nFlag & 0xff); }
            #region Reboot
            // Reboot 
            public void Reboot() { Reboot(_ID_BROADCASTING); }
            public void Reboot(int nAxis)
            {
                int i;
                if (nAxis < 0xfe) Clear_Flag(nAxis);
                else
                {
                    for (i = 0; i < _SIZE_MOTOR_MAX; i++) Clear_Flag(i);
                }

                if (m_aSParam_Axis[nAxis].nProtocol_Version != 1) Write_Command(nAxis, 0x08);

                Clear_Flag();

                // Initialize variable
                m_bStop = false;
                m_bEms = false;
                m_nMotorCnt_Back = m_nMotorCnt = 0;

            }
            private void MakeStuff(ref byte[] pBuff)
            {
                int nStuff = 0;
                int[] pnIndex = new int[pBuff.Length];
                Array.Clear(pnIndex, 0, pnIndex.Length);
                int nCnt = 0;
                for (int i = 5; i < pBuff.Length; i++)
                {
                    switch (nStuff)
                    {
                        case 0: { if (pBuff[i] == 0xff) nStuff++; } break;
                        case 1: { if (pBuff[i] == 0xff) nStuff++; else nStuff = 0; } break;
                        case 2:
                            {
                                if (pBuff[i] == 0xfd)
                                {
                                    nStuff++;
                                    pnIndex[nCnt++] = i;
                                }
                                else
                                {
                                    nStuff = 0;
                                }
                            }
                            break;
                    }
                }
                if (nCnt > 0)
                {
                    byte[] pBuff2 = new byte[pBuff.Length];
                    Array.Copy(pBuff, pBuff2, pBuff.Length);
                    Array.Resize<byte>(ref pBuff, pBuff2.Length + nCnt);
                    int nIndex = 0;
                    int nPos = 0;
                    foreach (byte byTmp in pBuff)
                    {
                        pBuff[nIndex + nPos] = pBuff2[nIndex];
                        if (nIndex == pnIndex[nPos])
                        {
                            pBuff[nIndex + nPos + 1] = 0xfd;
                            nPos++;
                        }
                        nIndex++;
                    }
                }
                pnIndex = null;
            }
            #endregion Reboot
            private int updateCRC(byte [] data_blk_ptr, int data_blk_size)
            {
                int i, j;
                int [] crc_table = new int[256] { 0x0000,
                0x8005, 0x800F, 0x000A, 0x801B, 0x001E, 0x0014, 0x8011,
                0x8033, 0x0036, 0x003C, 0x8039, 0x0028, 0x802D, 0x8027,
                0x0022, 0x8063, 0x0066, 0x006C, 0x8069, 0x0078, 0x807D,
                0x8077, 0x0072, 0x0050, 0x8055, 0x805F, 0x005A, 0x804B,
                0x004E, 0x0044, 0x8041, 0x80C3, 0x00C6, 0x00CC, 0x80C9,
                0x00D8, 0x80DD, 0x80D7, 0x00D2, 0x00F0, 0x80F5, 0x80FF,
                0x00FA, 0x80EB, 0x00EE, 0x00E4, 0x80E1, 0x00A0, 0x80A5,
                0x80AF, 0x00AA, 0x80BB, 0x00BE, 0x00B4, 0x80B1, 0x8093,
                0x0096, 0x009C, 0x8099, 0x0088, 0x808D, 0x8087, 0x0082,
                0x8183, 0x0186, 0x018C, 0x8189, 0x0198, 0x819D, 0x8197,
                0x0192, 0x01B0, 0x81B5, 0x81BF, 0x01BA, 0x81AB, 0x01AE,
                0x01A4, 0x81A1, 0x01E0, 0x81E5, 0x81EF, 0x01EA, 0x81FB,
                0x01FE, 0x01F4, 0x81F1, 0x81D3, 0x01D6, 0x01DC, 0x81D9,
                0x01C8, 0x81CD, 0x81C7, 0x01C2, 0x0140, 0x8145, 0x814F,
                0x014A, 0x815B, 0x015E, 0x0154, 0x8151, 0x8173, 0x0176,
                0x017C, 0x8179, 0x0168, 0x816D, 0x8167, 0x0162, 0x8123,
                0x0126, 0x012C, 0x8129, 0x0138, 0x813D, 0x8137, 0x0132,
                0x0110, 0x8115, 0x811F, 0x011A, 0x810B, 0x010E, 0x0104,
                0x8101, 0x8303, 0x0306, 0x030C, 0x8309, 0x0318, 0x831D,
                0x8317, 0x0312, 0x0330, 0x8335, 0x833F, 0x033A, 0x832B,
                0x032E, 0x0324, 0x8321, 0x0360, 0x8365, 0x836F, 0x036A,
                0x837B, 0x037E, 0x0374, 0x8371, 0x8353, 0x0356, 0x035C,
                0x8359, 0x0348, 0x834D, 0x8347, 0x0342, 0x03C0, 0x83C5,
                0x83CF, 0x03CA, 0x83DB, 0x03DE, 0x03D4, 0x83D1, 0x83F3,
                0x03F6, 0x03FC, 0x83F9, 0x03E8, 0x83ED, 0x83E7, 0x03E2,
                0x83A3, 0x03A6, 0x03AC, 0x83A9, 0x03B8, 0x83BD, 0x83B7,
                0x03B2, 0x0390, 0x8395, 0x839F, 0x039A, 0x838B, 0x038E,
                0x0384, 0x8381, 0x0280, 0x8285, 0x828F, 0x028A, 0x829B,
                0x029E, 0x0294, 0x8291, 0x82B3, 0x02B6, 0x02BC, 0x82B9,
                0x02A8, 0x82AD, 0x82A7, 0x02A2, 0x82E3, 0x02E6, 0x02EC,
                0x82E9, 0x02F8, 0x82FD, 0x82F7, 0x02F2, 0x02D0, 0x82D5,
                0x82DF, 0x02DA, 0x82CB, 0x02CE, 0x02C4, 0x82C1, 0x8243,
                0x0246, 0x024C, 0x8249, 0x0258, 0x825D, 0x8257, 0x0252,
                0x0270, 0x8275, 0x827F, 0x027A, 0x826B, 0x026E, 0x0264,
                0x8261, 0x0220, 0x8225, 0x822F, 0x022A, 0x823B, 0x023E,
                0x0234, 0x8231, 0x8213, 0x0216, 0x021C, 0x8219, 0x0208,
                0x820D, 0x8207, 0x0202 };
                int crc_accum = 0;
                for (j = 0; j < data_blk_size; j++)
                {
                    i = (byte)(((byte)(crc_accum >> 8) ^ data_blk_ptr[j]) & 0xFF);
                    crc_accum = (crc_accum << 8) ^ crc_table[i];
                }
                return crc_accum;
            }
            private void SendPacket(byte[] buffer, int nLength)
            {
                //	unsigned char *szPacket = (unsigned char *)malloc(sizeof(unsigned char) *( nLength + 2));
                if (IsOpen() == true)
                {
                    m_CSerial.SendPacket(buffer, nLength);
#if false
                    Ojw.CMessage.Write2("[SendPacket()]\r\n");
                    for (int nMsg = 0; nMsg < nLength; nMsg++)
                    {
                        Ojw.CMessage.Write2("0x%02X, ", buffer[nMsg]);
                    }
                    Ojw.CMessage.Write2("\r\n");
#endif
                }
                //////if (IsOpen_Socket() == true)
                //////{
                //////    m_CSocket.Send(buffer);
                //////}
            }

            //public void Read(int nAxis, int nCommand, int nAddress, params byte[] pbyDatas)
            //{
            //    int i;

            //    int nID = ((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);
            //    int nLength = 3 + 2 + pbyDatas.Length;
            //    int nDefaultSize = 7;
            //    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
            //    i = 0;
            //    pbyteBuffer[i++] = 0xff;
            //    pbyteBuffer[i++] = 0xff;
            //    pbyteBuffer[i++] = 0xfd;
            //    pbyteBuffer[i++] = 0x00;
            //    pbyteBuffer[i++] = (byte)(nID & 0xff);
            //    pbyteBuffer[i++] = (byte)(nLength & 0xff);
            //    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
            //    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
            //    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
            //    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
            //    foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;

            //    MakeStuff(ref pbyteBuffer);
            //    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
            //    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
            //    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

            //    //MakeCheckSum(nDefaultSize, pbyteBuffer);

            //    SendPacket(pbyteBuffer, pbyteBuffer.Length);

            //    Clear_Flag();

            //    // Initialize variable
            //    //m_bStop = false;
            //    //m_bEms = false;
            //    //m_nMotorCnt_Back = m_nMotorCnt = 0;
            //}
            public void Write_Command(int nAxis, int nCommand)
            {
                int i;
                
                int nID = ((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);
                if (m_aSParam_Axis[nAxis].nProtocol_Version == 1)
                {
                    int nLength = 1 + 2;
                    int nDefaultSize = 6;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                   
                    int nCrc = 0;
                    for (int j = 2; i < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                    SendPacket(pbyteBuffer, pbyteBuffer.Length);
                }
                else // if (m_aSParam_Axis[nAxis].nProtocol_Version == 2)
                {
                    int nLength = 3;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    pbyteBuffer[i++] = (byte)(nID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);
                
                    SendPacket(pbyteBuffer, pbyteBuffer.Length);
                }
            }
            private int m_nProtocolVersion = 2;
            public void Write(int nAxis, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                int i;

                int nID = ((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);
                i = 0;
                if (m_aSParam_Axis[nAxis].nProtocol_Version == 1)
                {
                    int nLength = 1 + 2 + pbyDatas.Length;
                    int nDefaultSize = 6;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                    if (pbyDatas != null)
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;

                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                    SendPacket(pbyteBuffer, pbyteBuffer.Length);
                }
                else // m_aSParam_Axis[nAxis].nProtocol_Version == 2
                {
                    int nLength = 3 + 2 + pbyDatas.Length;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    #endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                    if (pbyDatas != null)
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    SendPacket(pbyteBuffer, pbyteBuffer.Length);
                    //MakeCheckSum(nDefaultSize, pbyteBuffer);
                }

                //Clear_Flag();

                // Initialize variable
                //m_bStop = false;
                //m_bEms = false;
                //m_nMotorCnt_Back = m_nMotorCnt = 0;
            }
            public void Write(int nAxis, int nAddress, params byte [] pbyDatas)
            {
                Write(nAxis, 0x03, nAddress, pbyDatas);
            }
            // Motor Control - Torq On / Off
            public void SetTorque(int nAxis, bool bOn) 	//torque on / Off
            {
                if (nAxis == _ID_BROADCASTING) { m_nTorq = (bOn == true) ? 1 : 0; if (bOn == false) { for (int i = 0; i < m_aSMot.Length; i++) m_aSMot[i].bTorq = false; } }
                else m_aSMot[nAxis].bTorq = bOn;

                //if (m_aSParam_Axis[nAxis].nProtocol_Version == 1)
                //{
                //    // ax12a
                //    Write(nAxis, m_aSParam_Axis[nAxis].nAddr_Torq, (byte)((bOn == true) ? 1 : 0));
                //}
                //else
                //{
                    Write(nAxis, m_aSParam_Axis[nAxis].nAddr_Torq, (byte)((bOn == true) ? 1 : 0));
                //}
            }
            public void SetTorque(bool bOn) { SetTorque((int)_ID_BROADCASTING, bOn); }

            public void Writes(int nAddress, int nDataLength_without_ID, params byte[] pbyDatas) { Writes(nAddress, nDataLength_without_ID, pbyDatas.Length / (nDataLength_without_ID + 1), pbyDatas); }
            public void Writes(int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (m_nProtocolVersion != 1)
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write(0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else
                {
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write(0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null; 
                }
            }

            /////////////////////////////////////
            // Data Command(No motion) - just setting datas   		=> use with Send_Motor
            // ---- Position Control ----
            public void Set(int nAxis, int nEvd, float fRpm)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;

                //if (fRpm <= 0) return;

                Push_Id(nAxis);
                Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                Set_Flag_Mode(nAxis, 0);
                m_aSMot[nAxis].fPos = (float)CalcLimit_Evd(nAxis, nEvd);
                m_aSMot[nAxis].fRpm_Raw = CalcRpm2Raw(nAxis, fRpm);
                //Set_Flag_NoAction(nAxis, false);
                ////Push_Id(nAxis);	
            }
            public int Get(int nAxis) { return (int)Math.Round(m_aSMot[nAxis].fPos); }
            public void Set_Angle(int nAxis, float fAngle)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                int nControlMode = 3;
                if ((m_aSMot[nAxis].nControlMode != nControlMode) || (m_nTorq < 0))
                {
                    if (m_nTorq < 0) m_nTorq = 0;
                    bool bTorq = ((m_nTorq == 1) || (m_aSMot_Prev[nAxis].bTorq == true)) ? true : false;
                    SetTorque(nAxis, false);
                    // Protocol 1 계열 모터, XL-320 모터인 경우만
                    if (
                        (m_aSParam_Axis[nAxis].nProtocol_Version == 1) ||
                        (m_aSParam_Axis[nAxis].nModel == 350) // XL_320 checking
                        )
                    {
                        byte[] pbyteData = new byte[4];
                        int i = 0;
                        pbyteData[i++] = 0;
                        pbyteData[i++] = 0;
                        pbyteData[i++] = 0xff;
                        pbyteData[i++] = 0x03;
                        Write(nAxis, 0x06, pbyteData);
                        pbyteData = null;
                    }
                    else if (m_aSParam_Axis[nAxis].nProtocol_Version == 2)
                    {
                        byte[] pbyteData = new byte[1];
                        pbyteData[0] = (byte)nControlMode; // 3 : posture, 4 : multi-turn
                        Write(nAxis, 0x0b, pbyteData);
                        pbyteData = null;
                    }
                    if (bTorq == true) SetTorque(nAxis, true);
                }




                //if (fRpm <= 0) return;

                Push_Id(nAxis);
                Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                Set_Flag_Mode(nAxis, 0);
                m_aSMot[nAxis].fPos = CalcLimit_Evd(nAxis, CalcAngle2Evd(nAxis, fAngle));
                m_aSMot[nAxis].fRpm_Raw = -1f;// CalcRpm2Raw(nAxis, fRpm);
                m_aSMot[nAxis].nControlMode = 3; // Position
                //Set_Flag_NoAction(nAxis, false);
                ////Push_Id(nAxis);	

            }
            public void Set_Angle(int nAxis, float fAngle, float fRpm)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                int nControlMode = 3;
                if ((m_aSMot[nAxis].nControlMode != nControlMode) || (m_nTorq < 0))
                {
                    if (m_nTorq < 0) m_nTorq = 0;
                    bool bTorq = ((m_nTorq == 1) || (m_aSMot_Prev[nAxis].bTorq == true)) ? true : false;
                    SetTorque(nAxis, false);
                    // Protocol 1 계열 모터, XL-320 모터인 경우만
                    if (
                        (m_aSParam_Axis[nAxis].nProtocol_Version == 1) ||
                        (m_aSParam_Axis[nAxis].nModel == 350) // XL_320 checking
                        )
                    {
                        byte[] pbyteData = new byte[4];
                        int i = 0;
                        pbyteData[i++] = 0;
                        pbyteData[i++] = 0;
                        pbyteData[i++] = 0xff;
                        pbyteData[i++] = 0x03;
                        Write(nAxis, 0x06, pbyteData);
                        pbyteData = null;
                    }
                    else if (m_aSParam_Axis[nAxis].nProtocol_Version == 2)
                    {
                        byte[] pbyteData = new byte[1];
                        pbyteData[0] = (byte)nControlMode; // 3 : posture, 4 : multi-turn
                        Write(nAxis, 0x0b, pbyteData);
                        pbyteData = null;
                    }
                    if (bTorq == true) SetTorque(nAxis, true);
                }




                //if (fRpm <= 0) return;

                Push_Id(nAxis);
                Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                Set_Flag_Mode(nAxis, 0);
                m_aSMot[nAxis].fPos = CalcLimit_Evd(nAxis, CalcAngle2Evd(nAxis, fAngle));
                m_aSMot[nAxis].fRpm_Raw = CalcRpm2Raw(nAxis, fRpm);
                m_aSMot[nAxis].nControlMode = 3; // Position
                //Set_Flag_NoAction(nAxis, false);
                ////Push_Id(nAxis);	
                
            }
            public float Get_Angle(int nAxis) { return CalcEvd2Angle(nAxis, (int)m_aSMot[nAxis].fPos); }
            /////////////////////////////////
            public void SetLimitEn(bool bOn) { m_bIgnoredLimit = !bOn; }
            public bool GetLimitEn() { return !m_bIgnoredLimit; }

            public float Get_Speed(int nAxis) { return m_aSMot[nAxis].fRpm_Raw; }

            // 1111 1101
            public int Get_Flag(int nAxis) { return m_aSMot[nAxis].nFlag; }

            public int Clip(int nLimitValue_Up, int nLimitValue_Dn, int nData)
            {
                if (GetLimitEn() == false) return nData;

                int nRet = ((nData > nLimitValue_Up) ? nLimitValue_Up : nData);
                return ((nRet < nLimitValue_Dn) ? nLimitValue_Dn : nRet);
            }
            public float Clip(float fLimitValue_Up, float fLimitValue_Dn, float fData)
            {
                if (GetLimitEn() == false) return fData;
                float fRet = ((fData > fLimitValue_Up) ? fLimitValue_Up : fData);
                return ((fRet < fLimitValue_Dn) ? fLimitValue_Dn : fRet);
            }

            public int CalcLimit_Evd(int nAxis, int nValue)
            {
                if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                {
                    int nPulse = nValue;// &0x4000;
                    if (m_bMultiTurn == false)
                    {
                        //nValue &= 0x4000;
                        //nValue &= 0x3fff;
                    }

                    //nValue &= 0x3fff;
                    int nUp = 100000;
                    int nDn = -nUp;
                    if (m_aSMot[nAxis].fLimitUp != 0) nUp = CalcAngle2Evd(nAxis, m_aSMot[nAxis].fLimitUp);
                    if (m_aSMot[nAxis].fLimitDn != 0) nDn = CalcAngle2Evd(nAxis, m_aSMot[nAxis].fLimitDn);
                    if (nUp < nDn) { int nTmp = nUp; nUp = nDn; nDn = nTmp; }
                    return (Clip(nUp, nDn, nValue) | nPulse);
                }
                return nValue;
            }
            public float CalcLimit_Angle(int nAxis, float fValue)
            {
                if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                {
                    float fUp = 100000.0f;
                    float fDn = -fUp;
                    if (m_aSMot[nAxis].fLimitUp != 0) fUp = m_aSMot[nAxis].fLimitUp;
                    if (m_aSMot[nAxis].fLimitDn != 0) fDn = m_aSMot[nAxis].fLimitDn;
                    return Clip(fUp, fDn, fValue);
                }
                return fValue;
            }
            private int m_nOperatingMode = -1; // 이거 배열로 바꾸고 해야 함.
            // ---- Speed Control ----
            public void Set_Turn(int nAxis, int nEvd)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                int nControlMode = 1;
                if ((m_aSMot[nAxis].nControlMode != nControlMode) || (m_nTorq < 0))
                {
                    if (m_nTorq < 0) m_nTorq = 0;
                //if (m_aSMot[nAxis].nControlMode != 1)
                //{
                    bool bTorq = ((m_nTorq == 1) || (m_aSMot_Prev[nAxis].bTorq == true)) ? true : false;
                    SetTorque(nAxis, false);
                    // Protocol 1 계열 모터, XL-320 모터인 경우만
                    if (
                        (m_aSParam_Axis[nAxis].nProtocol_Version == 1) ||
                        (m_aSParam_Axis[nAxis].nModel == 350) // XL_320 checking
                        )
                    {
                        byte[] pbyteData = Ojw.CConvert.IntToBytes(0);
                        Write(nAxis, 0x06, pbyteData);
                        pbyteData = null;
                    }
                    else if (m_aSParam_Axis[nAxis].nProtocol_Version == 2)
                    {
                        byte[] pbyteData = new byte[1];
                        pbyteData[0] = 1; // 3 : posture, 4 : multi-turn
                        Write(nAxis, 0x0b, pbyteData);
                        pbyteData = null;
                    }
                    if (bTorq == true) SetTorque(nAxis, true);
                }

                Push_Id(nAxis);
                Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                Set_Flag_Mode(nAxis, 1);
                if ((m_aSMot[nAxis].nProtocol_Version != 2) || (m_aSMot[nAxis].nModel == 350))
                {
                    if (nEvd < 0) nEvd = (((nEvd * -1) & 0x2ff) | 0x400);
                }
                m_aSMot[nAxis].fPos = CalcLimit_Evd(nAxis, nEvd);
                m_aSMot[nAxis].fRpm_Raw = m_aSMot[nAxis].fPos;
                m_aSMot[nAxis].nControlMode = 1; // Position
                
                //Set_Flag_NoAction(nAxis, false);
                //Push_Id(nAxis);	
            }
            public int Get_Turn(int nAxis) { return (int)Math.Round(m_aSMot[nAxis].fRpm_Raw); }
            public int Get_Pos_Evd(int nAxis)
            {
                //int nValue = 0;

                //int nEvd = (int)((m_abyMem[m_nPack_ID, 132 + 0] & 0xff) |
                //                ((m_abyMem[m_nPack_ID, 132 + 1] << 8) & 0xff00) |
                //                ((m_abyMem[m_nPack_ID, 132 + 2] << 16) & 0xff0000) |
                //                ((m_abyMem[m_nPack_ID, 132 + 3] << 24) & 0xff000000));
                byte[] abyteData;
                if (m_aSParam_Axis[nAxis].nModel == 350) // XL_320 checking
                {
                    abyteData = new byte[2];
                    for (int i = 0; i < 2; i++) abyteData[i] = (byte)(m_abyMem[nAxis, 37 + i] & 0xff);
                    return (int)BitConverter.ToInt16(abyteData, 0);// *m_aSMot[nAxis].nDir;
                }
                else if (m_aSParam_Axis[nAxis].nModel == 12) // AX_12 checking
                {
                    abyteData = new byte[2];
                    for (int i = 0; i < 2; i++) abyteData[i] = (byte)(m_abyMem[nAxis, 36 + i] & 0xff);
                    return (int)BitConverter.ToInt16(abyteData, 0);// *m_aSMot[nAxis].nDir;
                }
                else
                {
                    abyteData = new byte[4];
                    for (int i = 0; i < 4; i++) abyteData[i] = (byte)(m_abyMem[nAxis, 132 + i] & 0xff);
                }

                // 0000 0000  0000 0000
                return BitConverter.ToInt32(abyteData, 0);// *m_aSMot[nAxis].nDir;
                //nValue = (int)(((abyteData[0] & 0x0f) << 8) | (abyteData[1] << 0) | ((abyteData[0] & 0x10) << (3 + 8)) | ((abyteData[0] & 0x10) << (2 + 8)) | ((abyteData[0] & 0x10) << (1 + 8)));	

                //return nValue;
            }
            public float Get_Pos_Angle(int nAxis) { return CalcEvd2Angle(nAxis, Get_Pos_Evd(nAxis)); }

            private void Push_Id(int nAxis) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; if (IsCmd(nAxis) == false) m_anEn[m_nMotorCnt++] = nAxis; }
            private int Pop_Id() { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return -1; if (m_nMotorCnt > 0) return m_anEn[--m_nMotorCnt]; return -1; }

            // Push Motor ID for checking(if you set a Motor ID with this function, you can get a feedback data with delay function)
            //public void Read_Motor_Push(int nAxis) { if (m_bProgEnd == true) return; if (Read_Motor_Index(nAxis) >= 0) return; if ((nAxis < 0) || (nAxis >= 254)) return; m_aSRead[m_nReadCnt].nID = nAxis; m_aSRead[m_nReadCnt].bEnable = true; m_aSRead[m_nReadCnt].nAddress_First = 132; m_aSRead[m_nReadCnt].nAddress_Length = 4; m_nReadCnt++; }
#if false 
            public void Read_Motor_Push(int nAxis) { if (m_bProgEnd == true) return; if (Read_Motor_Index(nAxis) >= 0) return; if ((nAxis < 0) || (nAxis >= 254)) return; m_aSRead[m_nReadCnt].nID = nAxis; m_aSRead[m_nReadCnt].bEnable = true; m_aSRead[m_nReadCnt].nAddress_First = 128; m_aSRead[m_nReadCnt].nAddress_Length = 8; m_nReadCnt++; }
#else
            public void Read_Motor_Push(int nAxis) { if (m_bProgEnd == true) return; if (Read_Motor_Index(nAxis) >= 0) return; if ((nAxis < 0) || (nAxis >= 254)) return; m_aSRead[m_nReadCnt].nID = nAxis; m_aSRead[m_nReadCnt].bEnable = true; m_aSRead[m_nReadCnt].nAddress_First = 0; m_aSRead[m_nReadCnt].nAddress_Length = m_aSParam_Axis[nAxis].nAddr_Max; m_nReadCnt++; }
#endif
            // You can check your Motor ID for feedback, which you set or not.
            public int Read_Motor_Index(int nAxis) { if (m_bProgEnd == true) return -1; for (int i = 0; i < m_nReadCnt; i++) { if (m_aSRead[i].nID == nAxis) return i; } return -1; }

            // use this when you don't want to get some motor datas.
            public void Read_Motor_Clear() { m_nReadCnt = 0; Array.Clear(m_aSRead, 0, _SIZE_MOTOR_MAX); }

            // detail option
            public void Read_Motor_Change_Address(int nAxis, int nAddress, int nLength) { int nIndex = Read_Motor_Index(nAxis); if (nIndex >= 0) { m_aSRead[nIndex].nAddress_First = nAddress; m_aSRead[nIndex].nAddress_Length = nLength; } }
            public void Read_Motor_ShowMessage(bool bTrue) { m_bShowMessage = bTrue; }
            public bool IsCmd(int nAxis)
            {
                for (int i = 0; i < m_nMotorCnt; i++) if (m_anEn[i] == nAxis) return true;
                return false;
            }
            public int CalcAngle2Evd(int nAxis, float fValue)
            {
                fValue *= ((m_aSMot[nAxis].nDir == 0) ? 1.0f : -1.0f);
                int nData = 0;
                //if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                //{
                //    nData = (int)Math.Round(fValue);
                //    //Ojw.CMessage.Write("Speed Turn");
                //}
                //else
                {
                    // 위치제어
                    nData = (int)Math.Round((m_aSMot[nAxis].fMechMove * fValue) / m_aSMot[nAxis].fDegree);
                    nData = nData + (int)Math.Round(m_aSMot[nAxis].fCenterPos);

                    //if (nAxis == 2)
                    //    Ojw.CMessage.Write("[{0}]Angle(%.2f), Mech(%.2f), Degree(%.2f), Center(%.2f), nData({1})", nAxis, fValue, m_aSMot[nAxis].fMechMove, m_aSMot[nAxis].fDegree, m_aSMot[nAxis].fCenterPos, nData);
                    //Ojw.CMessage.Write("[{0}]Angle(%.2f), Mech(%.2f), Degree(%.2f), Center(%.2f), nData({1})", nAxis, fValue, m_aSMot[nAxis].fMechMove, m_aSMot[nAxis].fDegree, m_aSMot[nAxis].fCenterPos, nData);
                }

                return nData;
            }
            public float CalcEvd2Angle(int nAxis, int nValue)
            {
                float fValue = ((m_aSMot[nAxis].nDir == 0) ? 1.0f : -1.0f);
                float fValue2 = 0.0f;
                //if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                //    fValue2 = (float)nValue * fValue;
                //else                                // 위치제어
                {
                    fValue2 = (float)(((m_aSMot[nAxis].fDegree * ((float)(nValue - (int)Math.Round(m_aSMot[nAxis].fCenterPos)))) / m_aSMot[nAxis].fMechMove) * fValue);
                    //if (nAxis == 2)
                    //    Ojw.CMessage.Write("[{0}]Angle(%.2f), Mech(%.2f), Degree(%.2f), Center(%.2f), nData({1})", nAxis, fValue, m_aSMot[nAxis].fMechMove, m_aSMot[nAxis].fDegree, m_aSMot[nAxis].fCenterPos, nData);
                }
                return fValue2;
            }

            #region RPM

            //private const float _RPM = 0.229f;
            //private const float _MAX_EV_RPM = 480.0f;
            //private const float _MAX_RPM = _MAX_EV_RPM * _RPM; //109.92f

            public float CalcRaw2Rpm(int nAxis, int nValue) { return (float)nValue * m_aSParam_Axis[nAxis].fRefRpm; }
            public int CalcRpm2Raw(int nAxis, float fRpm) { return (int)Math.Round(Clip(m_aSParam_Axis[nAxis].fLimitRpm, 0, fRpm / m_aSParam_Axis[nAxis].fRefRpm)); }

            public float CalcTime2Rpm(float fDeltaAngle, float fTime)
            {
                //if (fDeltaAngle == 0) fDeltaAngle = (float)CMath._ZERO;
                #region Kor
                // 1도 이동시간 => fTime / fDeltaAngle

                // 60 초 동안 _MAX_RPM 을 회전하는 것이 RPM, 1도 움직이는 것을 체크하려면 여기에 360 도 회전값을 고려해 주어야 한다.
                // _MAX_RPM 은 1분(60초) 동안 _MAX_RPM 바퀴 (즉, 360 * _MAX_RPM) 를 회전한 값
                // _MAX_RPM * 360 : 60 seconds => 480(_MAX_EV_RPM) 일때 
                // => 1초간 6 * _MAX_RPM 도 회전, 1ms => _MAX_RPM * 360도 / 60000ms = 0.65952 도 이동
                // => 1도 움직이는데 60000 / (_MAX_RPM * 360) = 1.516254246 ms 가 필요

                // 1도 이동시간 => 60000 / (Rpm * 360)
                // 이동각도 당 이동시간 계산 => 1도 이동시간 * fDeltaAngle
                #endregion Kor
                //return 60000 / (fTime / fDeltaAngle * 360.0f);
                return (60.0f * fDeltaAngle * 1000.0f) / (360.0f * fTime);
            }
            //public int CalcTime2Rpm_Raw(float fDeltaAngle, float fTime)
            //{
            //    //float fRpm = (60.0f * fDeltaAngle * fTime) / (360.0f * 1000.0f);
            //    int nRpmRaw = CalcRpm2Raw(CalcTime2Rpm(fDeltaAngle, fTime));
            //    return ((nRpmRaw > _MAX_EV_RPM) ? (int)_MAX_EV_RPM : nRpmRaw);
            //    //int nRet = CalcRpm2Raw(CalcTime2Rpm(fDeltaAngle, fTime));
            //    //return ((nRet > (int)_MAX_EV_RPM) ? (int)_MAX_EV_RPM : ((nRet <= 0) ? 1 : nRet)); // clipping data ( 0 ~ 1023 )
            //}
            #endregion RPM

            public void Send_Motor(int nAxis, float fEvd, float fRpm)
            {
                if (m_aSParam_Axis[nAxis].nProtocol_Version == 1)
                {
                    byte[] pbyTmp = new byte[2];
                    // 속도
                    pbyTmp = Ojw.CConvert.ShortToBytes((short)CalcRpm2Raw(nAxis, fRpm));
                    Write(nAxis, 30, pbyTmp);
                    // 위치
                    pbyTmp = Ojw.CConvert.ShortToBytes((short)Ojw.CMath.Round(fEvd));
                    Write(nAxis, 32, pbyTmp);
                }
                else
                {
                    if (m_aSParam_Axis[nAxis].nModel == 350) // XL_320 checking
                    {
                        byte[] pbyTmp = new byte[m_aSParam_Axis[nAxis].nAddr_Pos_Speed_Size];
                        // 속도
                        pbyTmp = Ojw.CConvert.ShortToBytes((short)CalcRpm2Raw(nAxis, fRpm));
                        Write(nAxis, m_aSParam_Axis[nAxis].nAddr_Pos_Speed, pbyTmp);
                        // 위치
                        pbyTmp = Ojw.CConvert.ShortToBytes((short)Ojw.CMath.Round(fEvd));
                        Write(nAxis, m_aSParam_Axis[nAxis].nAddr_Pos, pbyTmp);
                    }
                    else
                    {
                        byte[] pbyTmp = new byte[m_aSParam_Axis[nAxis].nAddr_Pos_Speed_Size];
                        // 속도
                        pbyTmp = Ojw.CConvert.IntToBytes((int)CalcRpm2Raw(nAxis, fRpm));
                        Write(nAxis, m_aSParam_Axis[nAxis].nAddr_Pos_Speed, pbyTmp);
                        // 위치
                        pbyTmp = Ojw.CConvert.IntToBytes((int)Ojw.CMath.Round(fEvd));
                        Write(nAxis, m_aSParam_Axis[nAxis].nAddr_Pos, pbyTmp);
                    }
                }
            }
            public void Send_Motor()//(int nMilliseconds)
            {
                if ((m_bStop == true) || (m_bEms == true)) return;

                m_nMotorCnt_Back = m_nMotorCnt; // 나중에 waitaction 에서 사용

#if true
                int nAddr_Pos = -1;
                int nAddr_Pos_Spd = -1;
                int nAddr_Spd = -1;
                int nAddr_Size = -1;// ;

                int nID;
                //int i = 0;
                ////////////////////////////////////////////////
                int nPos;
                //int nFlag;

                byte[] pbyTmp = new byte[4];
                byte[] pbyTmp_Short = new byte[2];
                int nBuffer = 0;

                for (int nAxis = 0; nAxis < m_nMotorCnt; nAxis++) { if (m_aSMot[nAxis].bEn == true) nBuffer++; }


                byte[] pbyteBuffer_Pos = new byte[1024];//[(nBuffer * (8 + 1))];
                byte[] pbyteBuffer_Spd = new byte[1024];

                //byte[] pbyteMode = new byte[m_nMotorCnt];
                // region S-Jog Time
                //int nCalcTime = CalcTime_ms(nMillisecond);
                int nRpm = 0;// (int)Ojw.CMath.Round(fRpm);
                //pbyteBuffer[i++] = (byte)(nRpm & 0xff);
                //if (m_bMultiTurn == true) pbyteBuffer[i++] = (byte)((nCalcTime >> 8) & 0xff);
                int nCnt = m_nMotorCnt;//_SIZE_MOTOR_MAX;//m_nMotorCnt;
                int nCnt2 = 0;
                int nMode = -1; // 0 - pos, 1 - speed, 2 - torq
                //int nMode_Curr = nMode;
                bool bChangedMode = false;
                int nIndex_Pos = 0;
                int nIndex_Spd = 0;
                for (int nAxis2 = 0; nAxis2 < nCnt; nAxis2++)
                {
                    int nAxis = Pop_Id();
                    if (m_aSMot[nAxis].bEn == true)
                    {
                        nCnt2++; // 
                        // Position
                        nPos = Get(nAxis);
                        
                        // 모터당 아이디
                        nID = GetID_By_Axis(nAxis);
                        pbyteBuffer_Pos[nIndex_Pos++] = (byte)(nID & 0xff);
                        pbyteBuffer_Spd[nIndex_Spd++] = (byte)(nID & 0xff);
                        
                        if (nAddr_Pos == -1) 
                        {
                            nAddr_Pos = m_aSParam_Axis[nAxis].nAddr_Pos;
                            nAddr_Pos_Spd = m_aSParam_Axis[nAxis].nAddr_Pos_Speed;
                            nAddr_Spd = m_aSParam_Axis[nAxis].nAddr_Speed;
                            nAddr_Size = m_aSParam_Axis[nAxis].nAddr_Pos_Size;
                        }
                        // ojw5014 - 여기 나중에 모터별로 가능하도록 수정해야 한다. 현재의 가정은 모든 모터가 동일한 프로토콜 버전의 가정...
                        if (m_aSParam_Axis[nAxis].nProtocol_Version == 1)
                        {
                            if (Get_Flag_Mode(nAxis) == 0) // Posture
                            {
                                // 속도
                                pbyTmp_Short = Ojw.CConvert.ShortToBytes((short)Math.Round(Get_Speed(nAxis)));
                                Array.Copy(pbyTmp_Short, 0, pbyteBuffer_Spd, nIndex_Spd, pbyTmp_Short.Length);
                                // 위치
                                nIndex_Spd += pbyTmp_Short.Length;
                                pbyTmp_Short = Ojw.CConvert.ShortToBytes((short)nPos);
                                Array.Copy(pbyTmp_Short, 0, pbyteBuffer_Pos, nIndex_Pos, pbyTmp_Short.Length);
                                nIndex_Pos += pbyTmp_Short.Length;                                
                            }
                            else
                            {
                                // 바퀴모드 설정
                                // 6,7,8,9 (AX12) 를 0 으로


                                // 속도
                                pbyTmp_Short = Ojw.CConvert.ShortToBytes((short)Math.Round(Get_Speed(nAxis)));
                                Array.Copy(pbyTmp_Short, 0, pbyteBuffer_Spd, nIndex_Spd, pbyTmp_Short.Length);
                                // 위치
                                nIndex_Spd += pbyTmp_Short.Length;
                            }
                        }
                        else
                        {
                            if (Get_Flag_Mode(nAxis) == 0) // Posture
                            {
                                if (m_aSParam_Axis[nAxis].nModel == 350) // XL_320 checking
                                {
                                    // 속도
                                    pbyTmp_Short = Ojw.CConvert.ShortToBytes((short)Math.Round(Get_Speed(nAxis)));
                                    Array.Copy(pbyTmp_Short, 0, pbyteBuffer_Spd, nIndex_Spd, pbyTmp_Short.Length);
                                    nIndex_Spd += pbyTmp_Short.Length;
                                    // 위치
                                    pbyTmp_Short = Ojw.CConvert.ShortToBytes((short)nPos);
                                    Array.Copy(pbyTmp_Short, 0, pbyteBuffer_Pos, nIndex_Pos, pbyTmp_Short.Length);
                                    nIndex_Pos += pbyTmp_Short.Length;
                                }
                                else
                                {
                                    // 속도
                                    pbyTmp = Ojw.CConvert.IntToBytes((int)Math.Round(Get_Speed(nAxis)));
                                    Array.Copy(pbyTmp, 0, pbyteBuffer_Spd, nIndex_Spd, pbyTmp.Length);
                                    nIndex_Spd += pbyTmp.Length;
                                    // 위치
                                    pbyTmp = Ojw.CConvert.IntToBytes((int)nPos);
                                    Array.Copy(pbyTmp, 0, pbyteBuffer_Pos, nIndex_Pos, pbyTmp.Length);
                                    nIndex_Pos += pbyTmp.Length;
                                }
                            }
                            else
                            {
                                // 나중에 10의 보수 처리 할 것
                                
                                // 속도
                                pbyTmp = Ojw.CConvert.IntToBytes((int)Math.Round(Get_Speed(nAxis)));
                                Array.Copy(pbyTmp, 0, pbyteBuffer_Spd, nIndex_Spd, pbyTmp.Length);
                                // 위치
                                nIndex_Spd += pbyTmp.Length;
                            }
                        }
                        if (nMode < 0)
                        {
                            nMode = Get_Flag_Mode(nAxis);
                            if (nMode != m_nOperatingMode)
                            {
                                m_nOperatingMode = nMode;
                                bChangedMode = true;
                            }
                        }
                        else
                        {
                            if (nMode != Get_Flag_Mode(nAxis))
                            {
                                Ojw.CMessage.Write_Error("Do not use Set_Pos with Set_Turn function");
                                return;
                            }
                        }
                        //nMode = Get_Flag_Mode(nAxis);
                        m_aSMot[nAxis].bEn = false;
                        ////////////////////////////////////////////////
                    }
                }
                if (bChangedMode == true)
                {
                    //Writes(11, 1, nCnt, pbyteBuffer);
                    //m_nOperatingMode
                }
                if ((nAddr_Pos > 0) && (nAddr_Spd > 0) && (nAddr_Size > 0))
                {
                    if (m_nProtocolVersion == 1)
                    {
                        switch (nMode)
                        {
                            case 0: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); Writes(nAddr_Pos, nAddr_Size, nCnt2, pbyteBuffer_Pos); break;
                            case 1: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); break;
                            //case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                            default: break;
                        }
                    }
                    else
                    {
                        
                        switch (nMode)
                        {
                            case 0: Writes(nAddr_Pos_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); Writes(nAddr_Pos, nAddr_Size, nCnt2, pbyteBuffer_Pos); break;
                            case 1: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); break;
                            //case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                            default: break;
                        }
                    }
                }
                Clear_Flag();
                
                Array.Copy(m_aSMot, m_aSMot_Prev, m_aSMot_Prev.Length);                
                
                //m_nSeq_Motor++; // reserve;
                //m_nDelay = nMillisecond;
                Wait_Ready();
#else
                    int nID;
                int i = 0;
                ////////////////////////////////////////////////
                int nPos;
                //int nFlag;
                
                byte[] pbyTmp = new byte[((m_aSParam_Axis[nAxis].nProtocol_Version == 1) ? 2 : 4)];
                int nBuffer = 0;

                for (int nAxis = 0; nAxis < m_nMotorCnt; nAxis++) { if (m_aSMot[nAxis].bEn == true) nBuffer++; }


                byte[] pbyteBuffer = new byte[1024];//[(nBuffer * (8 + 1))];

                //byte[] pbyteMode = new byte[m_nMotorCnt];
                // region S-Jog Time
                //int nCalcTime = CalcTime_ms(nMillisecond);
                int nRpm = 0;// (int)Ojw.CMath.Round(fRpm);
                //pbyteBuffer[i++] = (byte)(nRpm & 0xff);
                //if (m_bMultiTurn == true) pbyteBuffer[i++] = (byte)((nCalcTime >> 8) & 0xff);
                int nCnt = m_nMotorCnt;//_SIZE_MOTOR_MAX;//m_nMotorCnt;
                int nMode = -1; // 0 - pos, 1 - speed, 2 - torq
                //int nMode_Curr = nMode;
                bool bChangedMode = false;
                for (int nAxis2 = 0; nAxis2 < nCnt; nAxis2++)
                {
                    int nAxis = Pop_Id();
                    if (m_aSMot[nAxis].bEn == true)
                    {
                        // Position
                        nPos = Get(nAxis);
                        
                        // 모터당 아이디
                        nID = GetID_By_Axis(nAxis);
                        pbyteBuffer[i++] = (byte)(nID & 0xff);
                        if (m_aSParam_Axis[nAxis].nProtocol_Version == 1)
                        {
                            if (Get_Flag_Mode(nAxis) == 0) // Posture
                            {
                                // 속도
                                pbyTmp = Ojw.CConvert.ShortToBytes((short)Math.Round(Get_Speed(nAxis)));
                                Array.Copy(pbyTmp, 0, pbyteBuffer, i, pbyTmp.Length);
                                // 위치
                                i += pbyTmp.Length;
                                pbyTmp = Ojw.CConvert.ShortToBytes((short)nPos);
                                Array.Copy(pbyTmp, 0, pbyteBuffer, i, pbyTmp.Length);
                                i += pbyTmp.Length;
                            }
                            else
                            {
                                // 속도
                                pbyTmp = Ojw.CConvert.ShortToBytes((short)Math.Round(Get_Speed(nAxis)));
                                Array.Copy(pbyTmp, 0, pbyteBuffer, i, pbyTmp.Length);
                                // 위치
                                i += pbyTmp.Length;
                            }
                        }
                        else
                        {
                            if (m_aSParam_Axis[nAxis].nModel == 350) // XL_320 checking
                            {
                            if (Get_Flag_Mode(nAxis) == 0) // Posture
                            {
                                // 속도
                                //pbyTmp = Ojw.CConvert.IntToBytes(CalcRpm2Raw(Get_Speed(nAxis)));
                                pbyTmp = Ojw.CConvert.IntToBytes((int)Math.Round(Get_Speed(nAxis)));
                                Array.Copy(pbyTmp, 0, pbyteBuffer, i, pbyTmp.Length);
                                // 위치
                                i += pbyTmp.Length;
                                pbyTmp = Ojw.CConvert.IntToBytes(nPos);
                                Array.Copy(pbyTmp, 0, pbyteBuffer, i, pbyTmp.Length);
                                i += pbyTmp.Length;
                            }
                            else
                            {
                                // 속도
                                //pbyTmp = Ojw.CConvert.IntToBytes(CalcRpm2Raw(Get_Speed(nAxis)));
                                pbyTmp = Ojw.CConvert.IntToBytes((int)Math.Round(Get_Speed(nAxis)));
                                Array.Copy(pbyTmp, 0, pbyteBuffer, i, pbyTmp.Length);
                                // 위치
                                i += pbyTmp.Length;
                                //pbyTmp = Ojw.CConvert.IntToBytes(nPos);
                                //Array.Copy(pbyTmp, 0, pbyteBuffer, i, pbyTmp.Length);
                                ////Write(nAxis, 116, pbyTmp);
                                //i += pbyTmp.Length;
                            }
                        }
                        if (nMode < 0)
                        {
                            nMode = Get_Flag_Mode(nAxis);
                            if (nMode != m_nOperatingMode)
                            {
                                m_nOperatingMode = nMode;
                                bChangedMode = true;
                            }
                        }
                        else
                        {
                            if (nMode != Get_Flag_Mode(nAxis))
                            {
                                Ojw.CMessage.Write_Error("Do not use Set_Pos with Set_Turn function");
                                return;
                            }
                        }
                        //nMode = Get_Flag_Mode(nAxis);
                        m_aSMot[nAxis].bEn = false;
                        ////////////////////////////////////////////////
                    }
                }
                if (bChangedMode == true)
                {
                    //Writes(11, 1, nCnt, pbyteBuffer);
                    //m_nOperatingMode
                }
                if (m_aSParam_Axis[nAxis].nProtocol_Version == 1)
                {
                    switch (nMode)
                    {
                        case 0: Writes(30, 4, nCnt, pbyteBuffer); break;
                        case 1: Writes(32, 2, nCnt, pbyteBuffer); break;
                        //case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                        default: break;
                    }
                }
                else
                {
                    switch (nMode)
                    {
                        case 0: Writes(112, 8, nCnt, pbyteBuffer); break;
                        case 1: Writes(104, 4, nCnt, pbyteBuffer); break;
                        //case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                        default: break;
                    }
                }
                Clear_Flag();
                
                Array.Copy(m_aSMot, m_aSMot_Prev, m_aSMot_Prev.Length);                
                
                //m_nSeq_Motor++; // reserve;
                //m_nDelay = nMillisecond;
                Wait_Ready();
#endif
            }
            
            private int GetID_By_Axis(int nAxis) { return (nAxis == 0xfe) ? 0xfe : m_aSMot[nAxis].nID; }

            public void Wait_Ready()
            {
                m_CTmr.Set();
            }
#if false
            public bool Wait_Delay(int nMilliseconds)
            {
                m_CTmr.Set();                
                while (m_CTmr.Get() < nMilliseconds)
                {
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { Sync_Seq(); return false; }//m_nSeq_Motor_Back = m_nSeq_Motor; return false; }                    
                    Application.DoEvents();
                }
                Sync_Seq();
                return true;
            }
#else
            public void Read_Motor(int nAxis) 
            {
                if (m_aSParam_Axis[nAxis].nProtocol_Version == 1) //return; // 아직 프로토콜 1 은 대응 안함
                {
                    byte[] pbyLength = Ojw.CConvert.ShortToBytes((short)m_aSParam_Axis[nAxis].nAddr_Max);
                    Write(nAxis, 0x02, 0, pbyLength);
                    pbyLength = null;
                }
                else
                {
#if false
                //m_nPack_Address = nAxis;
                byte[] pbyLength = Ojw.CConvert.ShortToBytes(8);
                Write(nAxis, 0x02, 128, pbyLength);
                pbyLength = null;
                //Read_Ram(nAxis, _ADDRESS_TORQUE_CONTROL + ((m_bMultiTurn == true) ? 4 : 0), 16); 
#else
                    byte[] pbyLength = Ojw.CConvert.ShortToBytes((short)m_aSParam_Axis[nAxis].nAddr_Max);
                    Write(nAxis, 0x02, 0, pbyLength);
                    pbyLength = null;
#endif
                }
            }//8); }{ Read_Ram(nAxis, _ADDRESS_TORQUE_CONTROL, 8); }
            public bool Read_Motor_IsReceived()
            {
                if (m_nSeq_Receive_Back != m_nSeq_Receive)
                {
#if true
                    Sync_Seq();
#endif
                    //m_nSeq_Receive_Back = m_nSeq_Receive;
                    //Ojw.CMessage.Write("Read_Motor_IsReceived() == true");
                    return true;
                }
                return false;
            }
            public SIndex_t Read_Motor()
            {
                SIndex_t SIndex = new SIndex_t(-1, -1, -1);
                if (m_nReadCnt <= 0) return SIndex;
                m_nRetrieve = 0;

                SIndex.nPrev = m_nReadMotor_Index;
                m_nReadMotor_Index = (m_nReadMotor_Index + 1) % m_nReadCnt;
                SIndex.nCurr = m_nReadMotor_Index;
                SIndex.nNext = (m_nReadMotor_Index + 1) % m_nReadCnt;

                Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                return SIndex;
            }

            private const int _CNT_RETRIEVE = 5;
            private int m_nRetrieve = 0;
            public int m_nReadMotor_Index = 0;
            public bool Wait_Delay(int nMilliseconds)
            {
                m_CTmr.Set();
                if (Read_Motor_IsReceived() == true) Read_Motor();
                else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                {
                    if (m_CTmr_Timeout.Get() > m_nTimeout)
                    {
                        Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                        m_CTmr_Timeout.Set();
                        m_nRetrieve++;
                    }
                }
                else if (m_CTmr_Timeout.Get() > m_nTimeout)
                {
                    Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                    //m_nSeq_Receive_Back = m_nSeq_Receive;
                    Read_Motor();
                }
                while (m_CTmr.Get() < nMilliseconds)
                {
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { Sync_Seq(); return false; }//m_nSeq_Motor_Back = m_nSeq_Motor; return false; }
                    if (Read_Motor_IsReceived() == true) Read_Motor();
#if true
                    else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                    {
                        if (m_CTmr_Timeout.Get() > m_nTimeout)
                        {
                            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                            m_CTmr_Timeout.Set();
                            m_nRetrieve++;
                        }
                    }
                    else if (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
		            else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif
                    {
                        Ojw.CMessage.Write("[Receive-Wait_Delay(ms)] Time out Error({0} ms)- ID[{1}]Seq[{2}]Seq_Back[{3}]Index[{4}]\r\n", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID, m_nSeq_Receive, m_nSeq_Receive_Back, m_nReadMotor_Index);
                        Read_Motor();
                    }
                    Application.DoEvents();
                }
                //m_nSeq_Motor_Back = m_nSeq_Motor;
                Sync_Seq();
                return true;
            }
#endif
            private void Sync_Seq() { m_nSeq_Receive_Back = m_nSeq_Receive; }
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#else
            #region Define
            // Address            

            // Model
            public const int _MODEL_XL_430 = 0; // 360도 4096 : 2048 center

            public readonly int _SIZE_MEMORY = 200;//1000;
            public readonly int _SIZE_MOTOR_MAX = 256;//254 + 1; // Bloadcasting 도 염두

            public readonly int _ID_BROADCASTING = 254;

            //public readonly int _FLAG_ENABLE 0x100


            public readonly int _HEADER1 = 0;
            public readonly int _HEADER2 = 1;
            public readonly int _HEADER3 = 2; // 0xFD
            public readonly int _RESERVED = 3; // 0x00
            public readonly int _ID = 4; // 0 ~ 252, 254 는 broadcast (253 - 0xfd, 255 - 0xff 는 사용 안함)
            public readonly int _SIZE_L = 5;
            public readonly int _SIZE_H = 6;
            public readonly int _CMD = 7; // instruction
            public readonly int _SIZE_PACKET_HEADER = 8;
            #endregion Define


            #region Var
            private bool m_bProgEnd = false;
            private Ojw.CSerial m_CSerial = new CSerial();
            private byte[,] m_abyMem;

            private int[] m_anPos;//[_SIZE_MOTOR_MAX];
            //private byte[] m_abyStatus1;//[_SIZE_MOTOR_MAX];
            //private byte[] m_abyStatus2;//[_SIZE_MOTOR_MAX];
            private int[] m_anAxis_By_ID;//[_SIZE_MOTOR_MAX];


            private int m_nSeq_Receive;
            private int m_nSeq_Receive_Back;

            private int GetAxis_By_ID(int nID) { return (nID == 0xfe) ? 0xfe : m_anAxis_By_ID[nID]; }

            private bool m_bShowMessage;

            private int m_nSeq_Motor;
            private int m_nSeq_Motor_Back;
            private int m_nDelay;
            private CTimer m_CTmr = new CTimer();
            private CTimer m_CTmr_Timeout = new CTimer();
            private Thread m_thReceive;

            public CSocket m_CSocket = new CSocket();
            private Thread m_thSock;



            private int m_nModel;

            private int m_nTimeout;
            private bool m_bIgnoredLimit;

            private SRead_t[] m_aSRead;//[_SIZE_MOTOR_MAX];
            private int m_nReadCnt;
            private int m_nMotorCnt;
            private int m_nMotorCnt_Back;
            private SParam_Axis_t[] m_aSParam_Axis;//[256];
            private SMot_t[] m_aSMot;//[_SIZE_MOTOR_MAX];
            private int[] m_anEn;//[_SIZE_MOTOR_MAX];
            private bool m_bStop;
            private bool m_bEms;
            private bool m_bMotionEnd;
            private bool m_bStart;
            #endregion Var

            public CDynamixel() // 생성자 초기화 함수
            {
                Init();
            }
            ~CDynamixel()
            {
                m_bProgEnd = true;
                Ojw.CTimer.Wait(100);
                if (IsOpen() == true) Close();
            }
            private void Init()
            {
                m_abReceivedPos = new bool[_SIZE_MOTOR_MAX];

                m_abyMem = new byte[_SIZE_MOTOR_MAX, _SIZE_MEMORY];

                m_anPos = new int[_SIZE_MOTOR_MAX];
                //m_abyStatus1 = new byte[_SIZE_MOTOR_MAX];
                //m_abyStatus2 = new byte[_SIZE_MOTOR_MAX];
                m_anAxis_By_ID = new int[_SIZE_MOTOR_MAX];

                m_aSRead = new SRead_t[_SIZE_MOTOR_MAX];
                m_aSParam_Axis = new SParam_Axis_t[256];
                m_aSMot = new SMot_t[_SIZE_MOTOR_MAX];
                m_anEn = new int[_SIZE_MOTOR_MAX];

                m_bShowMessage = false;
                m_nTimeout = 200;
                m_nSeq_Motor = 0;
                m_nSeq_Motor_Back = 0;
                m_nDelay = 0;


                m_bIgnoredLimit = false;

                m_nModel = 0;

                m_nSeq_Receive = 0;

                //m_bOpen = false;
                m_bStop = false;
                m_bEms = false;
                m_bMotionEnd = false;
                m_bStart = false;

                m_nMotorCnt_Back = m_nMotorCnt = 0;


                Array.Clear(m_abyMem, 0, _SIZE_MOTOR_MAX * _SIZE_MEMORY);
                Array.Clear(m_aSParam_Axis, 0, _SIZE_MOTOR_MAX);
                Array.Clear(m_aSMot, 0, _SIZE_MOTOR_MAX);

                //Array.Clear(m_abyStatus1, 0, _SIZE_MOTOR_MAX);
                //Array.Clear(m_abyStatus2, 0, _SIZE_MOTOR_MAX);
                Array.Clear(m_anAxis_By_ID, 0, _SIZE_MOTOR_MAX);

                Array.Clear(m_anEn, 0, _SIZE_MOTOR_MAX);
                Array.Clear(m_aSRead, 0, _SIZE_MOTOR_MAX);


                m_nReadCnt = 0;

                m_nSeq_Receive = 0;
                m_nSeq_Receive_Back = 0;

                for (int i = 0; i < _SIZE_MOTOR_MAX; i++) SetParam(i, _MODEL_XL_430);
                m_bProgEnd = false;
            }

            public bool IsOpen() { return m_CSerial.IsConnect(); }
            public bool IsStop() { return m_bStop; }
            public bool IsEms() { return m_bEms; }
            public bool Open(int nComport, int nBaudrate)//, int nModel)		//serial port open
            {
                if (IsOpen() == false)
                {
                    // Port Open                    
                    if (m_CSerial.Connect(nComport, nBaudrate) == false)
                    {
                        Ojw.CMessage.Write("Failed to open Serial Device{Comport:{0}, Baudrate:{1})", nComport, nBaudrate);
                        return false;
                    }
                    Ojw.CMessage.Write("Init Serial{0}", nComport);

                    // Thread
                    m_thReceive = new Thread(new ThreadStart(Thread_Receive));
                    m_thReceive.Start();
                    Ojw.CMessage.Write("Init Thread");

                    Clear_Flag();
                }

                if (IsOpen() == false)
                {
                    Ojw.CMessage.Write("[Open()][Error] Connection Fail - Comport {0}, {1}", nComport, nBaudrate);

                    //return;
                }
                return IsOpen();
            }
            public void Close()								//serial port close
            {
                if (IsOpen() == true)
                {
                    m_CSerial.DisConnect();
                    Ojw.CMessage.Write("Serial Port -> Closed");
                }
            }

            public bool IsOpen_Socket() { return m_CSocket.IsConnect(); }
            public bool Open_Socket(string strIP, int nPort)
            {
                bool bRet = m_CSocket.Connect(strIP, nPort);
                if (m_CSocket.IsConnect() == true)
                {
                    Ojw.CMessage.Write("[CHerkuleX2] Socket Connected");
                    // Thread
                    //////m_thSock = new Thread(new ThreadStart(Thread_Socket));
                    //////m_thSock.Start();
                    //////Ojw.CMessage.Write("[CHerkuleX2] Init Thread for socket");
                }
                else
                {
                    Ojw.CMessage.Write_Error("[CHerkuleX2] Socket Connection Fail");
                }
                return bRet;
            }
            public void Close_Socket()
            {
                if (m_CSocket.IsConnect() == true)
                {
                    m_CSocket.DisConnect();
                    Ojw.CMessage.Write("[CHerkuleX2] Socket Closed");
                }
            }
            //////private void Thread_Socket()
            //////{
            //////    Ojw.CMessage.Write("[Thread_Socket] Running Thread");
            //////    while ((m_bProgEnd == false) && (m_CSocket.IsConnect() == true))
            //////    {
            //////        int nSize = m_CSocket.GetBuffer_Length();
            //////        if (nSize > 0)
            //////        {
            //////            byte[] buf = m_CSerial.GetBytes();
            //////            if (buf != null)
            //////                Parsor(buf, nSize);
            //////        }
            //////        Thread.Sleep(1);
            //////    }

            //////    Ojw.CMessage.Write("[Thread_Socket] Closed Thread");
            //////}
            //public void Clone(out CHerkulex2 CMotor)
            //{
            //    CMotor = new CHerkulex2();
            //    Array.Copy(m_abReceivedPos, CMotor.m_abReceivedPos, m_abReceivedPos.Length);

            //    Array.Copy(m_abyRam, CMotor.m_abyRam, m_abyRam.Length);
            //    Array.Copy(m_abyRom, CMotor.m_abyRom, m_abyRom.Length);
            //    Array.Copy(m_anPos, CMotor.m_anPos, m_anPos.Length);
            //    Array.Copy(m_abyStatus1, CMotor.m_abyStatus1, m_abyStatus1.Length);
            //    Array.Copy(m_abyStatus2, CMotor.m_abyStatus2, m_abyStatus2.Length);

            //    Array.Copy(m_anAxis_By_ID, CMotor.m_anAxis_By_ID, m_anAxis_By_ID.Length);
            //    Array.Copy(m_aSRead, CMotor.m_aSRead, m_aSRead.Length);
            //    Array.Copy(m_aSParam_Axis, CMotor.m_aSParam_Axis, m_aSParam_Axis.Length);
            //    Array.Copy(m_aSMot, CMotor.m_aSMot, m_aSMot.Length);
            //    Array.Copy(m_anEn, CMotor.m_anEn, m_anEn.Length);

            //    CMotor.m_bShowMessage = m_bShowMessage;
            //    CMotor.m_nTimeout = m_nTimeout;
            //    CMotor.m_nSeq_Motor = m_nSeq_Motor;
            //    CMotor.m_nSeq_Motor_Back = m_nSeq_Motor_Back;
            //    CMotor.m_nDelay = m_nDelay;
            //    CMotor.m_bIgnoredLimit = m_bIgnoredLimit;
            //    CMotor.m_nModel = m_nModel;
            //    CMotor.m_nSeq_Receive = m_nSeq_Receive;
            //    CMotor.m_bStop = m_bStop;
            //    CMotor.m_bEms = m_bEms;
            //    CMotor.m_bMotionEnd = m_bMotionEnd;
            //    CMotor.m_bStart = m_bStart;
            //    CMotor.m_nMotorCnt = m_nMotorCnt;
            //    CMotor.m_nMotorCnt_Back = m_nMotorCnt_Back;

            //    CMotor.m_nReadCnt = m_nReadCnt;
            //    CMotor.m_nSeq_Receive = m_nSeq_Receive;
            //    CMotor.m_nSeq_Receive_Back = m_nSeq_Receive_Back;

            //    CMotor.m_bProgEnd = m_bProgEnd;
            //}
            //////private void Thread_Receive()
            //////{
            //////    byte[] buf;// = new char[256];
            //////    Ojw.CMessage.Write("[Thread_Receive] Running Thread");
            //////    while ((m_bProgEnd == false) && (IsOpen() == true))
            //////    {
            //////        int nSize = m_CSerial.GetBuffer_Length();
            //////        if (nSize > 0)
            //////        {
            //////            buf = m_CSerial.GetBytes();
            //////            //Ojw.CMessage.Write("[Receive]");

            //////            Parsor(buf, nSize);

            //////            //Ojw.CMessage.Write("");
            //////        }
            //////        Thread.Sleep(1);
            //////    }

            //////    Ojw.CMessage.Write("[Thread_Receive] Closed Thread");
            //////}

            private int m_nIndex = 0;
            private byte m_byCheckSum = 0;
            private byte m_byCheckSum1 = 0;
            private byte m_byCheckSum2 = 0;
            private int m_nPacketSize = 0;
            private int m_nDataLength = 0;
            private int m_nIndexData = 0;
            private int m_nCmd = 0;
            private int m_nId = 0;
            private int m_nData_Address = 0;
            private int m_nData_Length = 0;
            private bool m_bHeader = false;

//////            private void Parsor(byte[] buf, int nSize)
//////            {

//////                for (int i = 0; i < nSize; i++)
//////                {
//////#if false
//////                    if ((buf[i] == 0xff) && (bHeader == false))
//////                    //if (((m_bMultiTurn == true) && ((nIndex == 0) && (buf[i] == 0xff))) || ((m_bMultiTurn == false) && (buf[i] == 0xff)))
//////                    {
//////                        byCheckSum = 0;
//////                        nIndexData = 0;
                         
//////                        nData_Address = 0;
//////                        nData_Length = 0;

//////                        nIndex = 1;
//////                        bHeader = true;
//////                        continue;
//////                    }
//////                    else bHeader = false;
//////#endif
//////                    //Ojw.CMessage.Write("[Index={0}]", nIndex);
//////                    //Ojw.CMessage.Write("0x%02X,", buf[i]);

//////                    switch (m_nIndex)
//////                    {
//////#if true
//////                        case 0:
//////                            //if (buf[i] == 0xff) 
//////                            if (((m_bMultiTurn == true) && ((m_nIndex == 0) && (buf[i] == 0xff))) || ((m_bMultiTurn == false) && (buf[i] == 0xff)))
//////                            {
//////                                m_byCheckSum = 0;
//////                                m_nIndexData = 0;

//////                                m_nData_Address = 0;
//////                                m_nData_Length = 0;

//////                                m_nIndex++;
//////                            }
//////                            break;
//////#endif
//////                        case 1:
//////                            if (buf[i] == 0xff) m_nIndex++;
//////                            else m_nIndex = 0;//-1;
//////                            break;
//////                        case 2: // Packet Size
//////                            m_nPacketSize = buf[i];
//////                            m_nDataLength = m_nPacketSize - _SIZE_PACKET_HEADER - 2;
//////                            m_byCheckSum = buf[i];
//////                            if (m_nDataLength < 0) m_nIndex = 0;
//////                            else m_nIndex++;
//////                            break;
//////                        case 3: // ID
//////                            m_nId = GetAxis_By_ID(buf[i]);
//////                            m_byCheckSum ^= buf[i];
//////                            m_nIndex++;
//////                            break;
//////                        case 4: // Cmd
//////                            m_nCmd = buf[i];
//////                            m_byCheckSum ^= buf[i];
//////                            m_nIndex++;
//////                            break;
//////                        case 5: // CheckSum1
//////                            m_byCheckSum1 = buf[i];
//////                            m_nIndex++;
//////                            break;
//////                        case 6: // CheckSum2
//////                            m_byCheckSum2 = buf[i];
//////                            if ((~m_byCheckSum1 & 0xfe) != m_byCheckSum2) m_nIndex = 0;
//////                            else m_nIndex++;
//////                            break;
//////                        case 7: // Datas...
//////                            //Ojw.CMessage.Write("[DataLength={0}/{1}]", nIndexData, nDataLength);
//////                            if (m_nIndexData < m_nDataLength)
//////                            {
//////                                if (m_nIndexData == 0) m_nData_Address = buf[i];
//////                                else if (m_nIndexData == 1) m_nData_Length = buf[i];
//////                                else m_abyRam[m_nId, m_nData_Address + m_nIndexData - 2] = buf[i];




//////                                if (++m_nIndexData >= m_nDataLength) m_nIndex++;

//////                                m_byCheckSum ^= buf[i];
//////                            }
//////                            else
//////                            {
//////                                //Ojw.CMessage.Write("====== Status1=======");
//////                                m_abyStatus1[m_nId] = buf[i];
//////                                m_byCheckSum ^= buf[i];
//////                                m_nIndex += 2;
//////                            }
//////                            break;
//////                        case 8: // Status 1		
//////                            //Ojw.CMessage.Write("====== Status1=======");
//////                            m_abyStatus1[m_nId] = buf[i];
//////                            m_byCheckSum ^= buf[i];
//////                            m_nIndex++;


//////                            /*
//////                                                    0x01 : Exceed Input Voltage Limit
//////                                                    0x02 : Exceed allowed POT limit
//////                                                    0x04 : Exceed Temperature limit
//////                                                    0x08 : Invalid Packet
//////                                                    0x10 : Overload detected
//////                                                    0x20 : Driver fault detected
//////                                                    0x40 : EEP REG distorted
//////                                                    0x80 : reserved
//////                            */

//////                            break;
//////                        case 9: // Status 2		
//////                            //Ojw.CMessage.Write("====== Status2=======(Chk: 0x%02X , 0x%02X", byCheckSum, byCheckSum1);
//////                            m_abyStatus2[m_nId] = buf[i];
//////                            m_byCheckSum ^= buf[i];

//////                            /*
//////                                                    0x01 : Moving flag
//////                                                    0x02 : Inposition flag
//////                                                    0x04 : Checksum Error
//////                                                    0x08 : Unknown Command
//////                                                    0x10 : Exceed REG range
//////                                                    0x20 : Garbage detected
//////                                                    0x40 : MOTOR_ON flag
//////                                                    0x80 : reserved
//////                            */


//////                            ///////////////////
//////                            // Done
//////                            if ((m_byCheckSum & 0xFE) == m_byCheckSum1)
//////                            {
//////                                // test
//////                                byte[] abyteData = new byte[2];
//////                                abyteData[0] = (byte)(m_abyRam[m_nId, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0)] & 0xff);
//////                                abyteData[1] = (byte)(m_abyRam[m_nId, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0) + 1] & 0xff);
//////                                // 0000 0000  0000 0000
//////                                m_anPos[m_nId] = BitConverter.ToInt16(abyteData, 0);
//////                                //m_anPos[m_nId] = (short)(((abyteData[0] & 0x0f) << 8) | (abyteData[1] << 0) | ((abyteData[0] & 0x10) << (3 + 8)) | ((abyteData[0] & 0x10) << (2 + 8)) | ((abyteData[0] & 0x10) << (1 + 8)));

//////                                if (m_bShowMessage == true) Ojw.CMessage.Write("Data Received(<Address({0})Length({1})>Pos[{2}]={3}, Status1 = {4}, Status2 = {5})", m_nData_Address, m_nDataLength, m_nId, m_anPos[m_nId], m_abyStatus1[m_nId], m_abyStatus2[m_nId]);

//////                                m_abReceivedPos[m_nId] = true;
//////                                m_nSeq_Receive++;
//////                            }

//////                            m_nIndex = 0;
//////                            break;
//////                    }
//////                }
//////            }
//////            private bool[] m_abReceivedPos;// = new bool[_SIZE_MOTOR_MAX];

            public SParam_Axis_t GetParam(int nAxis)
            {
                return m_aSParam_Axis[nAxis];
            }

            public void SetParam(int nAxis, int nRealID, int nDir, float fLimitUp, float fLimitDn, float fCenterPos, float fOffsetAngle_Display, float fMechMove, float fDegree)
            {
                //if ((nAxis >= _CNT_MAX_MOTOR) || (nID >= _MOTOR_MAX)) return false;

                m_aSParam_Axis[nAxis].nID = m_aSMot[nAxis].nID = nRealID;
                m_aSParam_Axis[nAxis].nDir = m_aSMot[nAxis].nDir = nDir;
                m_aSParam_Axis[nAxis].fLimitUp = m_aSMot[nAxis].fLimitUp = fLimitUp;
                m_aSParam_Axis[nAxis].fLimitDn = m_aSMot[nAxis].fLimitDn = fLimitDn;
                m_aSParam_Axis[nAxis].fCenterPos = m_aSMot[nAxis].fCenterPos = fCenterPos;
                m_aSParam_Axis[nAxis].fOffsetAngle_Display = fOffsetAngle_Display;
                m_aSParam_Axis[nAxis].fMechMove = m_aSMot[nAxis].fMechMove = fMechMove;
                m_aSParam_Axis[nAxis].fDegree = m_aSMot[nAxis].fDegree = fDegree;
            }
            private bool m_bMultiTurn = false;
            public void SetParam(int nAxis, int nModel)
            {
                if (nAxis > _ID_BROADCASTING) nAxis = _ID_BROADCASTING;
                //Ojw.CMessage.Write("nAxis = {0}, nModel = {1}", nAxis, nModel);
                if (nModel == _MODEL_XL_430) m_bMultiTurn = true;
                else if (m_bMultiTurn == true) m_bMultiTurn = false;
                switch (nModel)
                {
                    case _MODEL_XL_430 : 
                        SetParam_RealID(nAxis, nAxis);
                        SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 2048.0f);
                        SetParam_Display(nAxis, 0.0f);
                        SetParam_MechMove(nAxis, 4096.0f);
                        SetParam_Degree(nAxis, 360.0f);
                        break;
                }
            }

            public void SetParam_RealID(int nAxis, int nRealID) { m_aSParam_Axis[nAxis].nID = m_aSMot[nAxis].nID = nRealID; m_anAxis_By_ID[nRealID] = nAxis; }
            public void SetParam_Dir(int nAxis, int nDir) { m_aSParam_Axis[nAxis].nDir = m_aSMot[nAxis].nDir = nDir; }
            public void SetParam_LimitUp(int nAxis, float fLimitUp) { m_aSParam_Axis[nAxis].fLimitUp = m_aSMot[nAxis].fLimitUp = fLimitUp; }
            public void SetParam_LimitDown(int nAxis, float fLimitDn) { m_aSParam_Axis[nAxis].fLimitDn = m_aSMot[nAxis].fLimitDn = fLimitDn; }
            public void SetParam_CenterEvdValue(int nAxis, float fCenterPos) { m_aSParam_Axis[nAxis].fCenterPos = m_aSMot[nAxis].fCenterPos = fCenterPos; }
            public void SetParam_Display(int nAxis, float fOffsetAngle_Display) { m_aSParam_Axis[nAxis].fOffsetAngle_Display = fOffsetAngle_Display; }
            public void SetParam_MechMove(int nAxis, float fMechMove) { m_aSParam_Axis[nAxis].fMechMove = m_aSMot[nAxis].fMechMove = fMechMove; }
            public void SetParam_Degree(int nAxis, float fDegree) { m_aSParam_Axis[nAxis].fDegree = m_aSMot[nAxis].fDegree = fDegree; }

            public void Stop(int nAxis) // no stop flag setting
            {
                //	if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                Set_Turn(nAxis, 0);
                //	Set_Flag_Stop(nAxis, true);
                Send_Motor(1000);
            }
            public void Stop()
            {
                Set_Turn(254, 0);
                Send_Motor(100);
                m_bStop = true;
            }
            public void Ems()
            {
                Stop();
                SetTorque(false, false);
                m_bEms = true;
            }

            //////public byte GetErrorCode(int nAxis) { return m_abyStatus1[nAxis]; }
            //////public bool IsError(int nAxis)
            //////{
            //////    if (m_abyStatus1[nAxis] != 0)
            //////    {
            //////        return true;
            //////    }
            //////    return false;
            //////    /*
            //////        0x01 : Exceed Input Voltage Limit
            //////        0x02 : Exceed allowed POT limit
            //////        0x04 : Exceed Temperature limit
            //////        0x08 : Invalid Packet
            //////        0x10 : Overload detected
            //////        0x20 : Driver fault detected
            //////        0x40 : EEP REG distorted
            //////        0x80 : reserved
            //////    */
            //////}

            //////public bool IsWarning(int nAxis)
            //////{
            //////    // Status 2
            //////    if ((m_abyStatus2[nAxis] & 0x43) != 0)
            //////    {
            //////        return true;
            //////    }
            //////    return false;

            //////    /*
            //////        0x01 : Moving flag
            //////        0x02 : Inposition flag
            //////        0x04 : Checksum Error
            //////        0x08 : Unknown Command
            //////        0x10 : Exceed REG range
            //////        0x20 : Garbage detected
            //////        0x40 : MOTOR_ON flag
            //////        0x80 : reserved
            //////        */
            //////}
            //////////////////////////////////////////////////////////
            // Reboot 
            public void Reboot() { Reset(_ID_BROADCASTING); }
            public void Reboot(int nAxis)
            {
                int i;
                if (nAxis < 0xfe) Clear_Flag(nAxis);
                else
                {
                    for (i = 0; i < _SIZE_MOTOR_MAX; i++) Clear_Flag(i);
                }

                int nID = ((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);
                int nLength = 3;
                int nDefaultSize = 7;
                byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                i = 0;
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xfd;
                pbyteBuffer[i++] = 0x00;
                pbyteBuffer[i++] = (byte)(nID & 0xff);
                pbyteBuffer[i++] = (byte)(nLength & 0xff);
                pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                pbyteBuffer[i++] = 0x08;

                MakeStuff(ref pbyteBuffer);

                pbyteBuffer[pbyteBuffer.Length - 2] = (byte)((pbyteBuffer.Length - nDefaultSize) & 0xff);
                pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(((pbyteBuffer.Length - nDefaultSize) >> 8) & 0xff);
                
                //MakeCheckSum(nDefaultSize, pbyteBuffer);

                SendPacket(pbyteBuffer, nDefaultSize);

                Clear_Flag();
                
                // Initialize variable
                m_bStop = false;
                m_bEms = false;
                m_nMotorCnt_Back = m_nMotorCnt = 0;

            }
            private void MakeStuff(ref byte[] pBuff)
            {                
                int nStuff = 0;
                int[] pnIndex = new int[pBuff.Length];
                Array.Clear(pnIndex, 0, pnIndex.Length);
                int nCnt = 0;
                for (int i = 5; i < pBuff.Length; i++)
                {
                    switch (nStuff)
                    {
                        case 0: { if (pBuff[i] == 0xff) nStuff++; } break;
                        case 1: { if (pBuff[i] == 0xff) nStuff++; else nStuff = 0; } break;
                        case 2:
                            {
                                if (pBuff[i] == 0xfd)
                                {
                                    nStuff++;
                                    pnIndex[nCnt++] = i;
                                }
                                else
                                {
                                    nStuff = 0;
                                }
                            }
                            break;
                    }
                }
                if (nCnt > 0)
                {
                    byte[] pBuff2 = new byte[pBuff.Length];
                    Array.Copy(pBuff, pBuff2, pBuff.Length);
                    Array.Resize<byte>(ref pBuff, pBuff2.Length + nCnt);
                    int nIndex = 0;
                    int nPos = 0;
                    foreach (byte byTmp in pBuff)
                    {
                        pBuff[nIndex + nPos] = pBuff2[nIndex];
                        if (nIndex == pnIndex[nPos])
                        {
                            pBuff[nIndex + nPos + 1] = 0xfd;
                            nPos++;
                        }
                        nIndex++;
                    }
                }
                pnIndex = null;
            }

    
///////////////////////////////////
            // Motor Control - Reset
            //public void Reset() { Reset(_ID_BROADCASTING); }
            //public void Reset(int nAxis)
            //{
            //    int i;
            //    if (nAxis < 0xfe) Clear_Flag(nAxis);
            //    else
            //    {
            //        for (i = 0; i < _SIZE_MOTOR_MAX; i++) Clear_Flag(i);
            //    }

            //    int nID = ((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);
            //    int nLength = 3;
            //    int nDefaultSize = 7;
            //    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
            //    i = 0;
            //    pbyteBuffer[i++] = 0xff;
            //    pbyteBuffer[i++] = 0xff;
            //    pbyteBuffer[i++] = 0xfd;
            //    pbyteBuffer[i++] = 0x00;
            //    pbyteBuffer[i++] = (byte)(nID & 0xff);
            //    pbyteBuffer[i++] = (byte)(nLength & 0xff);
            //    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
            //    pbyteBuffer[i++] = 0x08;

            //    MakeStuff(ref pbyteBuffer);

            //    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)((pbyteBuffer.Length - nDefaultSize) & 0xff);
            //    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(((pbyteBuffer.Length - nDefaultSize) >> 8) & 0xff);
                
            //    //MakeCheckSum(nDefaultSize, pbyteBuffer);

            //    SendPacket(pbyteBuffer, nDefaultSize);

            //    Clear_Flag();
                
            //    // Initialize variable
            //    m_bStop = false;
            //    m_bEms = false;
            //}

            /////////////////////////////////
            public void SetLimitEn(bool bOn) { m_bIgnoredLimit = !bOn; }
            public bool GetLimitEn() { return !m_bIgnoredLimit; }

            // 1111 1101
            public int Get_Flag(int nAxis) { return m_aSMot[nAxis].nFlag; }

            public int Clip(int nLimitValue_Up, int nLimitValue_Dn, int nData)
            {
                if (GetLimitEn() == false) return nData;

                int nRet = ((nData > nLimitValue_Up) ? nLimitValue_Up : nData);
                return ((nRet < nLimitValue_Dn) ? nLimitValue_Dn : nRet);
            }
            public float Clip(float fLimitValue_Up, float fLimitValue_Dn, float fData)
            {
                if (GetLimitEn() == false) return fData;
                float fRet = ((fData > fLimitValue_Up) ? fLimitValue_Up : fData);
                return ((fRet < fLimitValue_Dn) ? fLimitValue_Dn : fRet);
            }

            public int CalcLimit_Evd(int nAxis, int nValue)
            {
                if ((Get_Flag_Mode(nAxis) == 0) || (Get_Flag_Mode(nAxis) == 2))
                {
                    int nPulse = nValue & 0x4000;
                    if (m_bMultiTurn == false)
                    {
                        //nValue &= 0x4000;
                        //nValue &= 0x3fff;
                    }

                    //nValue &= 0x3fff;
                    int nUp = 100000;
                    int nDn = -nUp;
                    if (m_aSMot[nAxis].fLimitUp != 0) nUp = CalcAngle2Evd(nAxis, m_aSMot[nAxis].fLimitUp);
                    if (m_aSMot[nAxis].fLimitDn != 0) nDn = CalcAngle2Evd(nAxis, m_aSMot[nAxis].fLimitDn);
                    if (nUp < nDn) { int nTmp = nUp; nUp = nDn; nDn = nTmp; }
                    return (Clip(nUp, nDn, nValue) | nPulse);
                }
                return nValue;
            }
            public float CalcLimit_Angle(int nAxis, float fValue)
            {
                if ((Get_Flag_Mode(nAxis) == 0) || (Get_Flag_Mode(nAxis) == 2))
                {
                    float fUp = 100000.0f;
                    float fDn = -fUp;
                    if (m_aSMot[nAxis].fLimitUp != 0) fUp = m_aSMot[nAxis].fLimitUp;
                    if (m_aSMot[nAxis].fLimitDn != 0) fDn = m_aSMot[nAxis].fLimitDn;
                    return Clip(fUp, fDn, fValue);
                }
                return fValue;
            }

            public int CalcTime_ms(int nTime)
            {
                // 1 Tick 당 11.2 ms => 1:11.2=x:nTime => x = nTime / 11.2
                return ((nTime <= 0) ? 1 : (int)Math.Round((float)nTime / 11.2f));
            }
            public int CalcAngle2Evd(int nAxis, float fValue)
            {
                fValue *= ((m_aSMot[nAxis].nDir == 0) ? 1.0f : -1.0f);
                int nData = 0;
                if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                {
                    nData = (int)Math.Round(fValue);
                    //Ojw.CMessage.Write("Speed Turn");
                }
                else
                {
                    // 위치제어
                    nData = (int)Math.Round((m_aSMot[nAxis].fMechMove * fValue) / m_aSMot[nAxis].fDegree);
                    nData = nData + (int)Math.Round(m_aSMot[nAxis].fCenterPos);

                    //if (nAxis == 2)
                    //    Ojw.CMessage.Write("[{0}]Angle(%.2f), Mech(%.2f), Degree(%.2f), Center(%.2f), nData({1})", nAxis, fValue, m_aSMot[nAxis].fMechMove, m_aSMot[nAxis].fDegree, m_aSMot[nAxis].fCenterPos, nData);
                    //Ojw.CMessage.Write("[{0}]Angle(%.2f), Mech(%.2f), Degree(%.2f), Center(%.2f), nData({1})", nAxis, fValue, m_aSMot[nAxis].fMechMove, m_aSMot[nAxis].fDegree, m_aSMot[nAxis].fCenterPos, nData);
                }

                return nData;
            }
            public float CalcEvd2Angle(int nAxis, int nValue)
            {
                float fValue = ((m_aSMot[nAxis].nDir == 0) ? 1.0f : -1.0f);
                float fValue2 = 0.0f;
                if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                    fValue2 = (float)nValue * fValue;
                else                                // 위치제어
                {
                    fValue2 = (float)(((m_aSMot[nAxis].fDegree * ((float)(nValue - (int)Math.Round(m_aSMot[nAxis].fCenterPos)))) / m_aSMot[nAxis].fMechMove) * fValue);
                    //if (nAxis == 2)
                    //    Ojw.CMessage.Write("[{0}]Angle(%.2f), Mech(%.2f), Degree(%.2f), Center(%.2f), nData({1})", nAxis, fValue, m_aSMot[nAxis].fMechMove, m_aSMot[nAxis].fDegree, m_aSMot[nAxis].fCenterPos, nData);
                }
                return fValue2;
            }
            ////////////////////////////////////
            // Motor Control - Torq On / Off
            public void SetTorque(int nAxis, bool bOn) 	//torque on / Off
            {
                int nID = m_aSMot[nAxis].nID;
                int i = 0;
                byte byOn = 0;
                byOn |= (byte)((bDrvOn == true) ? 0x40 : 0x00);
                byOn |= (byte)((bSrvOn == true) ? 0x20 : 0x00);
                byte[] pbyteBuffer = new byte[50];
                // Data
                pbyteBuffer[i++] = (byte)((_ADDRESS_TORQUE_CONTROL & 0xff) + ((m_bMultiTurn == true) ? 4 : 0));//;// 52번 레지스터 명령
                ////////
                pbyteBuffer[i++] = 0x01;// 이후의 레지스터 사이즈
                pbyteBuffer[i++] = byOn;

                Make_And_Send_Packet(nID, 0x03, i, pbyteBuffer);
            }
            public void SetTorque(bool bDrvOn, bool bSrvOn) { SetTorque((int)_ID_BROADCASTING, bDrvOn, bSrvOn); }

            /////////////////////////////////////
            // Data Command(No motion) - just setting datas   		=> use with Send_Motor
            // ---- Position Control ----
            public void Set(int nAxis, int nEvd)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push_Id(nAxis);
                Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                Set_Flag_Mode(nAxis, false);
                m_aSMot[nAxis].fPos = (float)CalcLimit_Evd(nAxis, nEvd);
                Set_Flag_NoAction(nAxis, false);
                //Push_Id(nAxis);	
            }
            public int Get(int nAxis) { return (int)Math.Round(m_aSMot[nAxis].fPos); }

            public void Set_Angle(int nAxis, float fAngle)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push_Id(nAxis);
                Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                Set_Flag_Mode(nAxis, false);
                m_aSMot[nAxis].fPos = CalcLimit_Evd(nAxis, CalcAngle2Evd(nAxis, fAngle));
                Set_Flag_NoAction(nAxis, false);
                //Push_Id(nAxis);	
            }
            public float Get_Angle(int nAxis) { return CalcEvd2Angle(nAxis, (int)m_aSMot[nAxis].fPos); }

            // ---- Speed Control ----
            public void Set_Turn(int nAxis, int nEvd)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push_Id(nAxis);
                Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                Set_Flag_Mode(nAxis, true);
                m_aSMot[nAxis].fPos = CalcLimit_Evd(nAxis, nEvd);
                Set_Flag_NoAction(nAxis, false);
                //Push_Id(nAxis);	
            }
            public int Get_Turn(int nAxis) { return (int)Math.Round(m_aSMot[nAxis].fPos); }
            public int Get_Pos_Evd(int nAxis)
            {
                int nValue = 0;
                if (m_bMultiTurn == false)
                {

                    byte[] abyteData = new byte[2];
                    abyteData[0] = (byte)(m_abyMem[nAxis, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0)] & 0xff);
                    abyteData[1] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0) + 1] & 0xff);
                    // 0000 0000  0000 0000
                    nValue = BitConverter.ToInt16(abyteData, 0);
                    //nValue = (int)(((abyteData[0] & 0x0f) << 8) | (abyteData[1] << 0) | ((abyteData[0] & 0x10) << (3 + 8)) | ((abyteData[0] & 0x10) << (2 + 8)) | ((abyteData[0] & 0x10) << (1 + 8)));	
                }
                else
                {

                    //bool bMinus = false;
                    //	0x10 00 00
                    byte[] abyteData = new byte[4];
                    abyteData[3] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 3] & 0xff);
                    abyteData[2] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 2] & 0xff);
                    /*if (((abyteData[2] & 0x80) != 0) || (abyteData[3] != 0))
                    {
                        bMinus = true;
                        //abyteData[3] |= 0xff;
                        //abyteData[2] |= 0xf0;
                    }*/
                    abyteData[1] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 1] & 0xff);
                    abyteData[0] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 0] & 0xff);
                    // 0000 0000  0000 0000
                    nValue = (int)(
                                    ((abyteData[3] & 0xff) << 24) |
                                    ((abyteData[2] & 0xff) << 16) |
                                    ((abyteData[1] & 0xff) << 8) |
                                    ((abyteData[0] & 0xff) << 0)
                                    );// | ( ? 0xfffffffffff00000 : 0);
                }
                return nValue;
            }
            public float Get_Pos_Angle(int nAxis) { return CalcEvd2Angle(nAxis, Get_Pos_Evd(nAxis)); }

            ///////////////////////////////////// -> Mode & NoAction : no use "PushId()"
            // Led Control   									=> use with Send_Motor
            public void Clear_Flag() { for (int i = 0; i < 256; i++) m_aSMot[i].nFlag = _FLAG_NO_ACTION; }
            public void Clear_Flag(int nAxis) { m_aSMot[nAxis].nFlag = _FLAG_NO_ACTION; }//{ m_aSMot[nAxis].nFlag = nFlag; Push_Id(nAxis); }
            public void Set_Flag(int nAxis, bool bStop, bool bMode_Speed, bool bLed_Green, bool bLed_Blue, bool bLed_Red, bool bNoAction) { m_aSMot[nAxis].nFlag = ((bStop == true) ? _FLAG_STOP : 0) | ((bMode_Speed == true) ? _FLAG_MODE_SPEED : 0) | ((bLed_Green == true) ? _FLAG_LED_GREEN : 0) | ((bLed_Blue == true) ? _FLAG_LED_BLUE : 0) | ((bLed_Red == true) ? _FLAG_LED_RED : 0) | ((bNoAction == true) ? _FLAG_NO_ACTION : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Stop(int nAxis, bool bStop) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xfe) | ((bStop == true) ? _FLAG_STOP : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Mode(int nAxis, bool bMode_Speed) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xfd) | ((bMode_Speed == true) ? _FLAG_MODE_SPEED : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Led(int nAxis, bool bGreen, bool bBlue, bool bRed) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xe3) | ((bGreen == true) ? _FLAG_LED_GREEN : 0) | ((bBlue == true) ? _FLAG_LED_BLUE : 0) | ((bRed == true) ? _FLAG_LED_RED : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Led_Green(int nAxis, bool bGreen) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xfb) | ((bGreen == true) ? _FLAG_LED_GREEN : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Led_Blue(int nAxis, bool bBlue) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xf7) | ((bBlue == true) ? _FLAG_LED_BLUE : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Led_Red(int nAxis, bool bRed) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xef) | ((bRed == true) ? _FLAG_LED_RED : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            // NoAction 은 push 안함
            public void Set_Flag_NoAction(int nAxis, bool bNoAction) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xdf) | ((bNoAction == true) ? _FLAG_NO_ACTION : 0); }

            // 1111 1101
            //public int 	Get_Flag(int nAxis) { return m_aSMot[nAxis].nFlag; }
            public int Get_Flag_Mode(int nAxis) { return (((m_aSMot[nAxis].nFlag & _FLAG_MODE_SPEED) != 0) ? 1 : 0); }

            public bool Get_Flag_Led_Green(int nAxis) { return (((m_aSMot[nAxis].nFlag & 0x04) != 0) ? true : false); }
            public bool Get_Flag_Led_Blue(int nAxis) { return (((m_aSMot[nAxis].nFlag & 0x08) != 0) ? true : false); }
            public bool Get_Flag_Led_Red(int nAxis) { return (((m_aSMot[nAxis].nFlag & 0x10) != 0) ? true : false); }

            //////////////////////////////////////
            // Motor Control - Move Motor(Action)

            public void Send_Motor(int nMillisecond)
            {
                if ((m_bStop == true) || (m_bEms == true)) return;

                m_nMotorCnt_Back = m_nMotorCnt; // 나중에 waitaction 에서 사용

                int nID;
                int i = 0;
                ////////////////////////////////////////////////

                byte[] pbyteBuffer = new byte[256];//[1 + 4 * m_nMotorCnt];
                int nPos;
                int nFlag;

                // region S-Jog Time
                int nCalcTime = CalcTime_ms(nMillisecond);
                pbyteBuffer[i++] = (byte)(nCalcTime & 0xff);
                if (m_bMultiTurn == true) pbyteBuffer[i++] = (byte)((nCalcTime >> 8) & 0xff);
                int nCnt = m_nMotorCnt;//_SIZE_MOTOR_MAX;//m_nMotorCnt;
                for (int nAxis2 = 0; nAxis2 < nCnt; nAxis2++)
                {
                    int nAxis = Pop_Id();//nAxis2;// i;//Pop_Id();
                    //Ojw.CMessage.Write("ID={0}", nAxis);
                    if (m_aSMot[nAxis].bEn == true)
                    {
                        //Ojw.CMessage.Write("ID={0}", nAxis);
                        //nPos |= _JOG_MODE_SPEED << 10;  // 속도제어 
                        // Position
                        nPos = Get(nAxis);
                        if (m_bMultiTurn == false)
                        {
                            if (nPos < 0)
                            {
                                nPos *= -1;
                                nPos |= 0x4000;
                            }
                        }
                        else
                        {
                        }

                        if (m_bMultiTurn == false)
                        {
                            pbyteBuffer[i++] = (byte)(nPos & 0xff);
                            pbyteBuffer[i++] = (byte)((nPos >> 8) & 0xff);
                        }
                        else
                        {
                            //printf("nPos=%d\r\n", nPos);
                            uint unPos = (uint)nPos;
                            pbyteBuffer[i++] = (byte)(unPos & 0xff);
                            pbyteBuffer[i++] = (byte)((unPos >> 8) & 0xff);
                            pbyteBuffer[i++] = (byte)((unPos >> 16) & 0xff);
                            pbyteBuffer[i++] = (byte)((unPos >> 24) & 0xff);
                        }
                        // Set-Flag
                        nFlag = Get_Flag(nAxis);
                        pbyteBuffer[i++] = (byte)(nFlag & 0xff);
                        Set_Flag_NoAction(nAxis, true); // 동작 후 모터 NoAction을 살려둔다.

                        // 모터당 아이디(후면에 붙는다)
                        nID = GetID_By_Axis(nAxis);
                        pbyteBuffer[i++] = (byte)(nID & 0xff);

                        m_aSMot[nAxis].bEn = false;
                        ////////////////////////////////////////////////
                    }

                }

                Make_And_Send_Packet(0xfe, 0x06, i, pbyteBuffer);
                //pbyteBuffer = null;

                Clear_Flag();

                //m_nSeq_Motor++; // reserve;
                m_nDelay = nMillisecond;
                Wait_Ready();
            }

            public void Wait_Ready()
            {
                m_CTmr.Set();
            }

            private const int _CNT_RETRIEVE = 5;
            private int m_nRetrieve = 0;
            public int m_nReadMotor_Index = 0;
            public bool Wait_Motor(int nMilliseconds)
            {
                if (Read_Motor_IsReceived() == true) Read_Motor();
#if true
                else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                {
                    if (m_CTmr_Timeout.Get() > m_nTimeout)
                    {
                        Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                        m_CTmr_Timeout.Set();
                        m_nRetrieve++;
                    }
                }
                else if (m_CTmr_Timeout.Get() > m_nTimeout)
#else
                else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif
                {
                    Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                    //m_nSeq_Receive_Back = m_nSeq_Receive;
                    Read_Motor();
                }
                while (m_CTmr.Get() < nMilliseconds)
                {
                    //if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { m_nSeq_Motor_Back = m_nSeq_Motor; return false; }
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { Sync_Seq(); return false; }

                    if (Read_Motor_IsReceived() == true) Read_Motor();
#if true
                    else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                    {
                        if (m_CTmr_Timeout.Get() > m_nTimeout)
                        {
                            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                            m_CTmr_Timeout.Set();
                            m_nRetrieve++;
                        }
                    }
                    else if (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
                    else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif
                    {
                        Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                        //m_nSeq_Receive_Back = m_nSeq_Receive;
                        Read_Motor();
                    }

                    Application.DoEvents();//Thread.Sleep(0);
                }
                Sync_Seq();
                //m_nSeq_Motor_Back = m_nSeq_Motor;
                return true;
            }
            public bool Wait_Motor()
            {
                if (Read_Motor_IsReceived() == true) Read_Motor();
                //else if (m_CTmr_Timeout.Get() > m_nTimeout)
#if true
                else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                {
                    if (m_CTmr_Timeout.Get() > m_nTimeout)
                    {
                        Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                        m_CTmr_Timeout.Set();
                        m_nRetrieve++;
                    }
                }
                else if (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
                else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif
                {
                    Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                    //m_nSeq_Receive_Back = m_nSeq_Receive;
                    Read_Motor();
                }
                while (m_CTmr.Get() < m_nDelay)
                {
                    //if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { m_nSeq_Motor_Back = m_nSeq_Motor; return false; }
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { Sync_Seq(); return false; }
                    if (Read_Motor_IsReceived() == true) Read_Motor();
                    //else if (m_CTmr_Timeout.Get() > m_nTimeout)
#if true
                    else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                    {
                        if (m_CTmr_Timeout.Get() > m_nTimeout)
                        {
                            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                            m_CTmr_Timeout.Set();
                            m_nRetrieve++;
                        }
                    }
                    else if (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
                    else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif
                    {
                        Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                        //m_nSeq_Receive_Back = m_nSeq_Receive;
                        Read_Motor();
                    }
                    Application.DoEvents();
                }
                Sync_Seq();//m_nSeq_Motor_Back = m_nSeq_Motor;
                return true;
            }

            private bool Wait_Position(int nAxis, float fTargetAngle, int nMilliseconds)
            {
                if (Read_Motor_IsReceived() == true) Read_Motor();
#if true
                else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                {
                    if (m_CTmr_Timeout.Get() > m_nTimeout)
                    {
                        Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                        m_CTmr_Timeout.Set();
                        m_nRetrieve++;
                    }
                }
                else if (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
                else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif
                {
                    Ojw.CMessage.Write("[Receive-Wait_Motor()*] Time out Error(%d ms)- ID[%d]Seq[%d]Seq_Back[%d]Index[%d]\r\n", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID, m_nSeq_Receive, m_nSeq_Receive_Back, m_nReadMotor_Index); ;
                    //m_nSeq_Receive_Back = m_nSeq_Receive;

                    Read_Motor();
                }
                float[] afMot = new float[_SIZE_MOTOR_MAX];
                bool[] abMot = new bool[_SIZE_MOTOR_MAX];
                while (m_CTmr.Get() < m_nDelay)
                {
                    if (abMot[nAxis] == false)
                    {
                        if (m_abReceivedPos[nAxis] == true)
                        {
                            abMot[nAxis] = true;
                            afMot[nAxis] = Get_Pos_Angle(nAxis);
                        }
                    }
                    else
                    {
                        if ((Get_Angle(nAxis) - afMot[nAxis]) >= 0)
                        {
                            if (Get_Pos_Angle(nAxis) >= fTargetAngle)
                            {
                                //printf("Break\n");
                                break;
                            }
                        }
                        else
                        {
                            if (Get_Pos_Angle(nAxis) <= fTargetAngle)
                            {
                                //printf("Break\n");
                                break;
                            }
                        }
                    }


                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { Sync_Seq(); return false; }
                    //if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { m_nSeq_Motor_Back = m_nSeq_Motor;; return false; }//m_nSeq_Motor_Back = m_nSeq_Motor; return false; }		
                    if (Read_Motor_IsReceived() == true) Read_Motor();
#if true
                    else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                    {
                        if (m_CTmr_Timeout.Get() > m_nTimeout)
                        {
                            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                            m_CTmr_Timeout.Set();
                            m_nRetrieve++;
                        }
                    }
                    else if (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
                    else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif
                    {
                        Ojw.CMessage.Write("[Receive-Wait_Motor()] Time out Error(%d ms)- ID[%d]Seq[%d]Seq_Back[%d]Index[%d]\r\n", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID, m_nSeq_Receive, m_nSeq_Receive_Back, m_nReadMotor_Index);
                        //m_nSeq_Receive_Back = m_nSeq_Receive;

                        Read_Motor();
                    }
                    Application.DoEvents();
                }

                Sync_Seq();//m_nSeq_Motor_Back = m_nSeq_Motor; 
                return true;
            }

            public bool Wait_Delay(int nMilliseconds)
            {
                m_CTmr.Set();
                if (Read_Motor_IsReceived() == true) Read_Motor();
                //else if (m_CTmr_Timeout.Get() > m_nTimeout)
#if true
                else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                {
                    if (m_CTmr_Timeout.Get() > m_nTimeout)
                    {
                        Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                        m_CTmr_Timeout.Set();
                        m_nRetrieve++;
                    }
                }
                else if (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
                else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif
                {
                    Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                    //m_nSeq_Receive_Back = m_nSeq_Receive;
                    Read_Motor();
                }
                while (m_CTmr.Get() < nMilliseconds)
                {
#if false
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { m_nSeq_Motor_Back = m_nSeq_Motor; return false; }
                    if (Read_Motor_IsReceived() == true) Read_Motor();
                    //else if (m_CTmr_Timeout.Get() > m_nTimeout)
#if true
                    else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                    {
                        if (m_CTmr_Timeout.Get() > m_nTimeout)
                        {
                            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                            m_CTmr_Timeout.Set();
                            m_nRetrieve++;
                        }
                    }
                    else if  (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
                    else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif	
                    {
                        Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                        //m_nSeq_Receive_Back = m_nSeq_Receive;
                        Read_Motor();
                    }
#else
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { Sync_Seq(); return false; }//m_nSeq_Motor_Back = m_nSeq_Motor; return false; }
                    if (Read_Motor_IsReceived() == true) Read_Motor();
#if true
                    else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                    {
                        if (m_CTmr_Timeout.Get() > m_nTimeout)
                        {
                            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                            m_CTmr_Timeout.Set();
                            m_nRetrieve++;
                        }
                    }
                    else if (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
                    else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif
                    {
                        Ojw.CMessage.Write("[Receive-Wait_Delay(ms)] Time out Error({0} ms)- ID[{1}]Seq[{2}]Seq_Back[{3}]Index[{4}]\r\n", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID, m_nSeq_Receive, m_nSeq_Receive_Back, m_nReadMotor_Index);
                        Read_Motor();
                    }
#endif
                    Application.DoEvents();
                }
                //m_nSeq_Motor_Back = m_nSeq_Motor;
                Sync_Seq();
                return true;
            }
            private void Sync_Seq() { m_nSeq_Receive_Back = m_nSeq_Receive; }

            public void Read_Ram(int nAxis, int nAddress, int nLength)
            {
                if (IsOpen() == false) return;

                //m_bBusy = true; // Request Data

                int nID = GetID_By_Axis(nAxis);
                int nDefaultSize = _CHECKSUM2 + 1;

                int i = 0;
                // Header
                byte[] abyteBuffer = new byte[256];
                abyteBuffer[_HEADER1] = 0xff;
                abyteBuffer[_HEADER2] = 0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                abyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                abyteBuffer[_CMD] = 0x04; // Ram 영역을 읽어온다.

                /////////////////////////////////////////////////////
                // Data
                abyteBuffer[nDefaultSize + i++] = (byte)(nAddress & 0xff);// Register address

                ////////
                abyteBuffer[nDefaultSize + i++] = (byte)(nLength & 0xff);// Data Size
                ////////
                /////////////////////////////////////////////////////

                //Packet Size
                abyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

                MakeCheckSum(nDefaultSize + i, abyteBuffer);//, out abyteBuffer[_CHECKSUM1], out abyteBuffer[_CHECKSUM2]);

                // 보내기 전에 Tick 을 Set 한다.
                Sync_Seq();
                //Tick_Send(nAxis);
                SendPacket(abyteBuffer, nDefaultSize + i);

                m_CTmr_Timeout.Set();
                //Ojw.CMessage.Write("Read_Ram({0})", nID);
            }

            public void Read_Motor(int nAxis) { Read_Ram(nAxis, _ADDRESS_TORQUE_CONTROL + ((m_bMultiTurn == true) ? 4 : 0), 16); }//8); }{ Read_Ram(nAxis, _ADDRESS_TORQUE_CONTROL, 8); }
            public bool Read_Motor_IsReceived()
            {
                if (m_nSeq_Receive_Back != m_nSeq_Receive)
                {
#if false
                    Sync_Seq();
#endif
                    //m_nSeq_Receive_Back = m_nSeq_Receive;
                    //Ojw.CMessage.Write("Read_Motor_IsReceived() == true");
                    return true;
                }
                return false;
            }
            public void Read_Motor()
            {
                if (m_nReadCnt <= 0) return;
                m_nRetrieve = 0;

                m_nReadMotor_Index = (m_nReadMotor_Index + 1) % m_nReadCnt;

                Read_Motor(m_aSRead[m_nReadMotor_Index].nID);

                //if ((m_nReadCnt > 0) && (m_nReadMotor_Index >= 0))
                //{
                //    m_nReadMotor_Index = (m_nReadMotor_Index + 1) % m_nReadCnt;
                //    //Ojw.CMessage.Write("Read_Motor()");
                //    m_nSeq_Receive_Back = m_nSeq_Receive;
                //    Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                //}
            }
            //////////////////////////////////////
            // Setting
            //void Set_Ram(int nId, 
            //void Set_Rom(int nId, 
            private void Push_Id(int nAxis) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; if (IsCmd(nAxis) == false) m_anEn[m_nMotorCnt++] = nAxis; }
            private int Pop_Id() { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return -1; if (m_nMotorCnt > 0) return m_anEn[--m_nMotorCnt]; return -1; }

            // Push Motor ID for checking(if you set a Motor ID with this function, you can get a feedback data with delay function)
            public void Read_Motor_Push(int nAxis) { if (m_bProgEnd == true) return; if (Read_Motor_Index(nAxis) >= 0) return; if ((nAxis < 0) || (nAxis >= 254)) return; m_aSRead[m_nReadCnt].nID = nAxis; m_aSRead[m_nReadCnt].bEnable = true; m_aSRead[m_nReadCnt].nAddress_First = _ADDRESS_TORQUE_CONTROL + ((m_bMultiTurn == true) ? 4 : 0); m_aSRead[m_nReadCnt].nAddress_Length = 8; m_nReadCnt++; }
            // You can check your Motor ID for feedback, which you set or not.
            public int Read_Motor_Index(int nAxis) { if (m_bProgEnd == true) return -1; for (int i = 0; i < m_nReadCnt; i++) { if (m_aSRead[i].nID == nAxis) return i; } return -1; }

            // use this when you don't want to get some motor datas.
            public void Read_Motor_Clear() { m_nReadCnt = 0; Array.Clear(m_aSRead, 0, _SIZE_MOTOR_MAX); }

            // detail option
            public void Read_Motor_Change_Address(int nAxis, int nAddress, int nLength) { int nIndex = Read_Motor_Index(nAxis); if (nIndex >= 0) { m_aSRead[nIndex].nAddress_First = nAddress; m_aSRead[nIndex].nAddress_Length = nLength; } }
            public void Read_Motor_ShowMessage(bool bTrue) { m_bShowMessage = bTrue; }
            public bool IsCmd(int nAxis)
            {
                for (int i = 0; i < m_nMotorCnt; i++) if (m_anEn[i] == nAxis) return true;
                return false;
            }

            private void SendPacket(byte[] buffer, int nLength)
            {
                //	unsigned char *szPacket = (unsigned char *)malloc(sizeof(unsigned char) *( nLength + 2));
                if (IsOpen() == true)
                {
                    m_CSerial.SendPacket(buffer, nLength);
#if false
                    Ojw.CMessage.Write2("[SendPacket()]\r\n");
                    for (int nMsg = 0; nMsg < nLength; nMsg++)
                    {
                        Ojw.CMessage.Write2("0x%02X, ", buffer[nMsg]);
                    }
                    Ojw.CMessage.Write2("\r\n");
#endif
                }
                if (IsOpen_Socket() == true)
                {
                    m_CSocket.Send(buffer);
                }
            }
            private void Make_And_Send_Packet(int nID, int nCmd, int nDataByteSize, byte[] pbytePacket)
            {
                int nDefaultSize = _CHECKSUM2 + 1;
                int nSize = nDefaultSize + nDataByteSize;

                byte[] pbyteBuffer = new byte[nSize];//(byte *)malloc(sizeof(byte) * nSize);

                // Header
                pbyteBuffer[_HEADER1] = (byte)0xff;
                pbyteBuffer[_HEADER2] = (byte)0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = (byte)(nCmd & 0xff);
                /////////////////////////////////////////////////////
                int i = 0;
                for (int j = 0; j < nDataByteSize; j++) pbyteBuffer[nDefaultSize + i++] = pbytePacket[j];
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)(nSize & 0xff);

                MakeCheckSum(nSize, pbyteBuffer);
#if false
Ojw.CMessage.Write2("[Make_And_Send_Packet()]\r\n");
for (int nMsg = 0; nMsg < nDefaultSize + i; nMsg++)
{
Ojw.CMessage.Write2("0x%02x, ", pbyteBuffer[nMsg]);
}
Ojw.CMessage.Write2("\r\n");
#endif
                SendPacket(pbyteBuffer, nSize);

                //free(pbyteBuffer);
            }

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

            private int GetID_By_Axis(int nAxis) { return (nAxis == 0xfe) ? 0xfe : m_aSMot[nAxis].nID; }

            #region MotionFile
            // Set3D 하면 모델링에서 가져온 상태로 강제 재셋팅, -> PlayFile(파일이름) 하면 된다.
            Ojw.C3d m_C3d = null;
            Ojw.C3d.COjwDesignerHeader m_CHeader = null;//new C3d.COjwDesignerHeader();
            public void SetHeader(Ojw.C3d.COjwDesignerHeader CHeader)
            {
                m_CHeader = CHeader;
                for (int i = 0; i < m_CHeader.nMotorCnt; i++)
                {
                    SetParam(
                        i,
                        m_CHeader.pSMotorInfo[i].nMotorID,
                        m_CHeader.pSMotorInfo[i].nMotorDir,
                        m_CHeader.pSMotorInfo[i].fLimit_Up,
                        m_CHeader.pSMotorInfo[i].fLimit_Down,
                        (float)m_CHeader.pSMotorInfo[i].nCenter_Evd,
                        0,
                        (float)m_CHeader.pSMotorInfo[i].nMechMove,
                        m_CHeader.pSMotorInfo[i].fMechAngle);
                }
            }
            public void Set3D(Ojw.C3d C3dModel) { m_C3d = C3dModel; SetHeader(m_C3d.GetHeader()); }
            public void PlayFrame(int nLine, SMotion_t SMotion)
            {
                if (SMotion.nFrameSize <= 0) return;
                if ((nLine < 0) || (nLine >= SMotion.nFrameSize)) return;

                if ((m_bStop == false) && (m_bEms == false) && (m_bMotionEnd == false))
                {
                    //m_bStop = false; 
                    for (int i = 0; i < _SIZE_MOTOR_MAX; i++) Set_Flag_Stop(i, false);
                    SetTorque(true, true);

                    for (int nAxis = 0; nAxis < m_nMotorCnt; nAxis++)
                    {
                        if (m_CHeader.pSMotorInfo[nAxis].nMotorControlType != 0) // 위치제어가 아니라면
                        {
                            SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            Set_Flag_Led(nAxis,
                                Get_Flag_Led_Green(SMotion.STable[nLine].anLed[nAxis]),
                                Get_Flag_Led_Blue(SMotion.STable[nLine].anLed[nAxis]),
                                Get_Flag_Led_Red(SMotion.STable[nLine].anLed[nAxis])
                                );

                            Set_Turn(nAxis, SMotion.STable[nLine].anMot[nAxis]);
                        }
                        else
                        {
                            SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            Set_Flag_Led(nAxis,
                                Get_Flag_Led_Green(SMotion.STable[nLine].anLed[nAxis]),
                                Get_Flag_Led_Blue(SMotion.STable[nLine].anLed[nAxis]),
                                Get_Flag_Led_Red(SMotion.STable[nLine].anLed[nAxis])
                                );

                            Set_Angle(nAxis, SMotion.STable[nLine].anMot[nAxis]);
                        }
                    }
                    Send_Motor(SMotion.STable[nLine].nTime);
                }
            }
            public void PlayFrame(SMotionTable_t STable)
            {
                if ((m_bStop == false) && (m_bEms == false) && (m_bMotionEnd == false))
                {
                    //m_bStop = false; 
                    for (int i = 0; i < _SIZE_MOTOR_MAX; i++) Set_Flag_Stop(i, false);
                    SetTorque(true, true);
                    for (int nAxis = 0; nAxis < m_CHeader.nMotorCnt; nAxis++)
                    {
                        if (m_CHeader.pSMotorInfo[nAxis].nMotorControlType != 0) // 위치제어가 아니라면
                        {
                            SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            Set_Flag_Led(nAxis,
                                Get_Flag_Led_Green(STable.anLed[nAxis]),
                                Get_Flag_Led_Blue(STable.anLed[nAxis]),
                                Get_Flag_Led_Red(STable.anLed[nAxis])
                                );

                            Set_Turn(nAxis, STable.anMot[nAxis]);
                        }
                        else
                        {
                            SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            Set_Flag_Led(nAxis,
                                Get_Flag_Led_Green(STable.anLed[nAxis]),
                                Get_Flag_Led_Blue(STable.anLed[nAxis]),
                                Get_Flag_Led_Red(STable.anLed[nAxis])
                                );

                            Set_Angle(nAxis, STable.anMot[nAxis]);
                        }
                    }
                    Send_Motor(STable.nTime);
                }
            }
            public void PlayFile(string strFileName)
            {
                try
                {
                    if (m_C3d == null) return;
                    SMotion_t SMotion = new SMotion_t();
                    if (m_C3d.BinaryFileOpen(strFileName, out SMotion) == true)
                    {
                        if (SMotion.nFrameSize > 0)
                        {
                            m_bStart = true;

                            m_C3d.WaitAction_SetTimer();

                            foreach (SMotionTable_t STable in SMotion.STable)
                            {
                                if (STable.bEn == true)
                                {
                                    PlayFrame(STable);

                                    int nDelay = STable.nTime + STable.nDelay;
                                    if (nDelay > 0) m_C3d.WaitAction_ByTimer(nDelay);
                                }
                            }
                            m_bStart = false;
                            m_bMotionEnd = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error("Error -> PlayMotion(), " + ex.ToString());
                }
            }
            #endregion MotionFile
#endif
        }
    }
}
