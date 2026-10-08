//#define _ARDUINO_OPENRB150
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

#if !_ARDUINO_OPENRB150
namespace OpenJigWare
{
    partial class Ojw
    {
        // ============================================================================
        // CProtocol — Dynamixel Protocol 1.0 + 2.0 통합 제어 클래스
        //
        //   CProtocol2 (COjw_37) 와 공개 인터페이스 동일 (기본 제어방식 = Protocol 2).
        //   모터별로 프로토콜을 선언 (선언 안하면 2):
        //     SetParam_Protocol(nID, 1);                          // 프로토콜만 지정
        //     SetParam_Model(nID, EModel_t._AX_12);               // 모델 지정 → 프로토콜 자동
        //   통신 포트는 하나 (같은 버스에 P1/P2 혼재). 같은 명령으로 동시 제어:
        //     Set(1, 45); Set(11, -30); Move(1000, 0);            // 1=AX(P1), 11=XL430(P2)
        //   → 내부에서 프로토콜/주소별로 자동 분리되어 같은 버스에 연속 송출 (CMonster2 방식).
        //     한 패킷에 두 프로토콜을 담는 것은 물리적으로 불가하지만, back-to-back 전송이라
        //     사용자 관점에서는 완전한 동시 제어 (수 ms 이내).
        //   P1 서보는 P2 패킷을 체크섬 불일치로 무시하고 그 반대도 같아서 혼재 버스가 성립함.
        //
        //   기존 Set*/Move/SyncRead 의 lstSecond 그룹핑 루프 (주소가 다른 모터를 다음 라운드로
        //   분리) 가 P1(30)/P2(116) 주소 차이로 프로토콜 분리까지 자동 수행 — 구조 변경 최소화.
        // ============================================================================
        public class CProtocol
        {
            public CProtocol()
            {
                for (int i = 0; i < m_aCParam.Length; i++)
                {
                    m_aCParam[i] = new CParam_t();
                    m_aCParam[i].SetParam(false); // 기본 = Protocol 2 (X 시리즈)
                }
            }
            ~CProtocol()
            {
                if (IsOpen()) Close();
            }
            public void SetParam(SMotorInfo_t[] aSParams)
            {
                for (int i = 0; i < aSParams.Length; i++)
                {
                    SetParam(aSParams[i].nMotorID, ((aSParams[i].nMotorDir == 0) ? false : true), ((aSParams[i].fGearRatio == 0) ? 1.0f : aSParams[i].fGearRatio), ((aSParams[i].nMotor_HightSpec == 0) ? false : true));
                    SetParam_Limit(aSParams[i].nMotorID, aSParams[i].fLimit_Up, aSParams[i].fLimit_Down);
                }
            }
            // bHigh: P2 에서는 PRO(H54) 프리셋, P1 로 선언된 모터에서는 MX 프리셋
            // (SetParam 은 현재 선언된 프로토콜을 유지함 — 선언 순서에 관대)
            public void SetParam(int nID, bool bDirReverse = false, float fMulti = 1.0f, bool bHigh = false)
            {
                m_aCParam[nID].SetParam(bHigh);
                m_aCParam[nID].m_bDirReverse = bDirReverse;
                m_aCParam[nID].m_fMulti = fMulti;
            }
            // ★ 모터별 프로토콜 선언 (1 또는 2, 기본 2). 프로토콜 기본 프리셋 주소로 초기화됨.
            public void SetParam_Protocol(int nID, int nProtocol)
            {
                m_aCParam[nID].SetParam_Protocol(nProtocol);
            }
            public int GetParam_Protocol(int nID) { return m_aCParam[nID].m_nProtocol; }
            public void SetParam_Dir(int nID, bool bDirReverse)
            {
                m_aCParam[nID].m_bDirReverse = bDirReverse;
            }
            // 통합 모델 enum (P1+P2) — 모델 선언 시 프로토콜 자동 결정
            public void SetParam_Model(int nID, EModel_t EModel)
            {
                m_aCParam[nID].SetParams_Model(EModel);
            }
            // 기존 CProtocol2 코드 호환용 (네임스페이스 레벨 Ojw.EModel_t → P2 기종)
            public void SetParam_Model(int nID, Ojw.EModel_t EModel)
            {
                switch (EModel)
                {
                    case Ojw.EModel_t.X: m_aCParam[nID].SetParams_Model(EModel_t.X); break;
                    case Ojw.EModel_t._X_430_250: m_aCParam[nID].SetParams_Model(EModel_t._X_430_250); break;
                    case Ojw.EModel_t._X_330_077: m_aCParam[nID].SetParams_Model(EModel_t._X_330_077); break;
                    case Ojw.EModel_t._X_330_288: m_aCParam[nID].SetParams_Model(EModel_t._X_330_288); break;
                    case Ojw.EModel_t._2X_430_250: m_aCParam[nID].SetParams_Model(EModel_t._2X_430_250); break;
                    case Ojw.EModel_t._X_540_150: m_aCParam[nID].SetParams_Model(EModel_t._X_540_150); break;
                    case Ojw.EModel_t._X_540_270: m_aCParam[nID].SetParams_Model(EModel_t._X_540_270); break;
                    case Ojw.EModel_t.Y: m_aCParam[nID].SetParams_Model(EModel_t.Y); break;
                    case Ojw.EModel_t._Y_70_210_M001: m_aCParam[nID].SetParams_Model(EModel_t._Y_70_210_M001); break;
                    case Ojw.EModel_t._Y_70_210_R051: m_aCParam[nID].SetParams_Model(EModel_t._Y_70_210_R051); break;
                    case Ojw.EModel_t._Y_70_210_R099: m_aCParam[nID].SetParams_Model(EModel_t._Y_70_210_R099); break;
                    case Ojw.EModel_t._Y_80_230_M001: m_aCParam[nID].SetParams_Model(EModel_t._Y_80_230_M001); break;
                    case Ojw.EModel_t._Y_80_230_R051: m_aCParam[nID].SetParams_Model(EModel_t._Y_80_230_R051); break;
                    case Ojw.EModel_t._Y_80_230_R099: m_aCParam[nID].SetParams_Model(EModel_t._Y_80_230_R099); break;
                    case Ojw.EModel_t.P: m_aCParam[nID].SetParams_Model(EModel_t.P); break;
                    case Ojw.EModel_t._PH54_60_250: m_aCParam[nID].SetParams_Model(EModel_t._PH54_60_250); break;
                    case Ojw.EModel_t._PH54_40_250: m_aCParam[nID].SetParams_Model(EModel_t._PH54_40_250); break;
                    case Ojw.EModel_t._PH42_10_260: m_aCParam[nID].SetParams_Model(EModel_t._PH42_10_260); break;
                    case Ojw.EModel_t._PM54_60_250: m_aCParam[nID].SetParams_Model(EModel_t._PM54_60_250); break;
                    case Ojw.EModel_t._PM54_40_250: m_aCParam[nID].SetParams_Model(EModel_t._PM54_40_250); break;
                    case Ojw.EModel_t._PM42_10_260: m_aCParam[nID].SetParams_Model(EModel_t._PM42_10_260); break;
                }
            }
            public void SetParam_Limit(int nID, float fLimitUp, float fLimitDown)
            {
                m_aCParam[nID].m_fLimit_Up = fLimitUp;      // 0: disable
                m_aCParam[nID].m_fLimit_Down = fLimitDown;   // 0: disable
            }
            public float CalcLimit(int nID, float fValue)
            {
                if ((m_aCParam[nID].m_fLimit_Down != 0) && (m_aCParam[nID].m_fLimit_Down >= fValue)) fValue = m_aCParam[nID].m_fLimit_Down;
                if ((m_aCParam[nID].m_fLimit_Up != 0) && (m_aCParam[nID].m_fLimit_Up <= fValue)) fValue = m_aCParam[nID].m_fLimit_Up;
                return fValue;
            }
            public bool IsLimit(int nID, float fValue)
            {
                if ((m_aCParam[nID].m_fLimit_Down != 0) && (m_aCParam[nID].m_fLimit_Down >= fValue)) return true;
                if ((m_aCParam[nID].m_fLimit_Up != 0) && (m_aCParam[nID].m_fLimit_Up <= fValue)) return true;
                return false;
            }
            public byte GetMap(int nID, int nAddress)
            {
                return m_abyMap[nID * _SIZE_MAP + nAddress];
            }
            public short GetMap_Short(int nID, int nAddress)
            {
                return CConvert.BytesToShort(m_abyMap, nID * _SIZE_MAP + nAddress);
            }
            public int GetMap_Int(int nID, int nAddress)
            {
                return CConvert.BytesToInt(m_abyMap, nID * _SIZE_MAP + nAddress);
            }
            public bool[] m_abMot = new bool[256];
            public const int _SIZE_MAP = 600; // P2 Y/P 시리즈 주소(552/580+) 대응 (P1 은 ~80 이하)
            public byte[] m_abyMap = new byte[256 * _SIZE_MAP];
            public int[] m_anMot = new int[256];
            public int[] m_anMot_Seq = new int[256];
            public int[] m_anMot_Curr = new int[256];
            public int[] m_anMot_Seq_Back = new int[256];
            public bool IsReceived_Angle(int nID)
            {
                if (m_anMot_Seq[nID] != m_anMot_Seq_Back[nID])
                {
                    m_anMot_Seq_Back[nID] = m_anMot_Seq[nID];
                    return true;
                }
                return false;
            }
            public float[] m_afMot = new float[256];
            public int[] m_anMot_Pose = new int[256];
            public float[] m_afMot_Pose = new float[256];

            // 통합 기종 enum — P2(X/Y/P) + P1(AX/DX/RX/EX/MX) 전부. 모델 = 프로토콜 자동 결정.
            public enum EModel_t
            {
                // ── Protocol 2.0 ──
                X = 0,
                _X_430_250,
                _X_330_077,
                _X_330_288,
                _2X_430_250,
                _X_540_150,
                _X_540_270,
                Y,
                _Y_70_210_M001,
                _Y_70_210_R051,
                _Y_70_210_R099,
                _Y_80_230_M001,
                _Y_80_230_R051,
                _Y_80_230_R099,
                P,
                _PH54_60_250,
                _PH54_40_250,
                _PH42_10_260,
                _PM54_60_250,
                _PM54_40_250,
                _PM42_10_260,
                // ── Protocol 1.0 ──
                AX = 100,
                _AX_12,
                _AX_18,
                _AX_12W,
                DX,
                _DX_113,
                _DX_116,
                _DX_117,
                RX,
                _RX_10,
                _RX_24F,
                _RX_28,
                _RX_64,
                EX,
                _EX_106,
                _EX_106P,
                MX,     // MX 1.0 펌웨어
                _MX_12,
                _MX_28,
                _MX_64,
                _MX_106,
            }
            public class CParam_t
            {
                public int m_nProtocol = 2; // ★ 1 = Protocol 1.0, 2 = Protocol 2.0 (기본)

                // 주소 필드 (기본값 = P2 X 시리즈)
                public int m_nSet_Operation_Address = 11;
                public int m_nSet_Operation_Size = 1;

                public int m_nSet_GoalCurrent_Address = -1;
                public int m_nSet_GoalCurrent_Size = 2;
                public int m_nSet_GAIN_POS_P = 84;
                public int m_nSet_GAIN_POS_P_Size = 2;
                public int m_nSet_GAIN_POS_I = 82;
                public int m_nSet_GAIN_POS_I_Size = 2;
                public int m_nSet_GAIN_POS_D = 80;
                public int m_nSet_GAIN_POS_D_Size = 2;
                public int m_nSet_GAIN_VEL_P = 78;
                public int m_nSet_GAIN_VEL_P_Size = 2;
                public int m_nSet_GAIN_VEL_I = 76;
                public int m_nSet_GAIN_VEL_I_Size = 2;
                public int m_nSet_GAIN_VEL_D = -1;
                public int m_nSet_GAIN_VEL_D_Size = 2;

                public int m_nSet_Torq_Address = 64;
                public int m_nSet_Torq_Size = 1;
                public int m_nSet_Led_Address = 65;
                public int m_nSet_Led_Size = 1;
                public int m_nSet_Curr_Address = 102;
                public int m_nSet_Curr_Size = 2;
                public int m_nSet_Position_Speed_Address = 112;
                public int m_nSet_Position_Speed_Size = 4;
                public int m_nSet_Position_Address = 116;
                public int m_nSet_Position_Size = 4;
                public int m_nSet_Speed_Address = 104;
                public int m_nSet_Speed_Size = 4;

                public float m_fMechMove = 4096.0f;
                public float m_fCenter = 2048.0f;
                public float m_fMechAngle = 360;
                public float m_fJointRpm = 0.229f;
                public bool m_bDirReverse = false;
                public float m_fMulti = 1.0f;

                public float m_fLimit_Up = 0;       // Limit(+) - 0: Ignore
                public float m_fLimit_Down = 0;     // Limit(-) - 0: Ignore

                public int m_nGet_Position_Address = 132;
                public int m_nGet_Position_Size = 4;
                public int m_nGet_Current_Address = 126;
                public int m_nGet_Current_Size = 2;
                public int m_nMax_Speed_For_Position = 0;

                public void SetParam_Dir(bool bReverse = false) { m_bDirReverse = bReverse; }
                public void SetParam_Multi(float fMulti = 1.0f) { m_fMulti = fMulti; if (fMulti == 0) m_fMulti = 1.0f; }

                // 프로토콜 선언 → 해당 프로토콜의 기본 프리셋 적용 (2=X 시리즈, 1=AX 계열)
                public void SetParam_Protocol(int nProtocol)
                {
                    if (nProtocol == 1) SetParams_Model(EModel_t.AX);
                    else SetParams_Model(EModel_t.X);
                }
                public void SetParams_Model(EModel_t EModel)
                {
                    switch (EModel)
                    {
                        #region Protocol 2.0 (CProtocol2 와 동일 프리셋)
                        case EModel_t.X:
                        case EModel_t._X_430_250:
                        case EModel_t._X_330_077:
                        case EModel_t._X_330_288:
                        case EModel_t._2X_430_250:
                        case EModel_t._X_540_150:
                        case EModel_t._X_540_270:
                            {
                                m_nProtocol = 2;
                                m_nSet_Operation_Address = 11;
                                m_nSet_Operation_Size = 1;
                                m_nSet_Torq_Address = 64;
                                m_nSet_Torq_Size = 1;
                                m_nSet_Led_Address = 65;
                                m_nSet_Led_Size = 1;
                                m_nSet_Curr_Address = 102;
                                m_nSet_Curr_Size = 2;
                                m_nSet_Speed_Address = 104;
                                m_nSet_Speed_Size = 4;
                                m_nSet_Position_Speed_Address = 112;
                                m_nSet_Position_Speed_Size = 4;
                                m_nSet_Position_Address = 116;
                                m_nSet_Position_Size = 4;

                                m_fMechMove = 4096.0f;
                                m_fCenter = 2048.0f;
                                m_fMechAngle = 360;
                                m_fJointRpm = 0.229f;
                                m_bDirReverse = false;
                                m_fMulti = 1.0f;
                                m_nGet_Position_Address = 132;
                                m_nGet_Position_Size = 4;

                                m_nSet_GoalCurrent_Address = -1;
                                m_nSet_GoalCurrent_Size = 2;
                                m_nSet_GAIN_POS_P = 84;
                                m_nSet_GAIN_POS_P_Size = 2;
                                m_nSet_GAIN_POS_I = 82;
                                m_nSet_GAIN_POS_I_Size = 2;
                                m_nSet_GAIN_POS_D = 80;
                                m_nSet_GAIN_POS_D_Size = 2;
                                m_nSet_GAIN_VEL_P = 78;
                                m_nSet_GAIN_VEL_P_Size = 2;
                                m_nSet_GAIN_VEL_I = 76;
                                m_nSet_GAIN_VEL_I_Size = 2;
                                m_nSet_GAIN_VEL_D = -1;
                                m_nSet_GAIN_VEL_D_Size = 2;

                                m_nGet_Current_Address = 126; // present load
                                m_nGet_Current_Size = 2;

                                m_nMax_Speed_For_Position = 0;
                            }
                            break;
                        case EModel_t.Y:
                        case EModel_t._Y_70_210_M001:
                        case EModel_t._Y_70_210_R051:
                        case EModel_t._Y_70_210_R099:
                        case EModel_t._Y_80_230_M001:
                        case EModel_t._Y_80_230_R051:
                        case EModel_t._Y_80_230_R099:
                            {
                                m_nProtocol = 2;
                                m_nSet_Operation_Address = 33; // Y: Operating Mode
                                m_nSet_Operation_Size = 1;
                                m_nSet_Torq_Address = 512;
                                m_nSet_Torq_Size = 1;
                                m_nSet_Led_Address = 513;
                                m_nSet_Led_Size = 1;
                                m_nSet_Curr_Address = 526;
                                m_nSet_Curr_Size = 2;
                                m_nSet_Speed_Address = 528;
                                m_nSet_Speed_Size = 4;
                                m_nSet_Position_Speed_Address = 528; // Y는 속도값을 속도제어모드와 같이 사용
                                m_nSet_Position_Speed_Size = 4;
                                m_nSet_Position_Address = 532;
                                m_nSet_Position_Size = 4;

                                m_fMechMove = 524288.0f;
                                m_fCenter = 0f;
                                m_fMechAngle = 360.0f;
                                m_fJointRpm = 0.01f;
                                m_bDirReverse = false;
                                m_fMulti = 1.0f;
                                m_nGet_Position_Address = 552;
                                m_nGet_Position_Size = 4;

                                m_nSet_GoalCurrent_Address = 526;
                                m_nSet_GoalCurrent_Size = 2;
                                m_nSet_GAIN_POS_P = 232;
                                m_nSet_GAIN_POS_P_Size = 4;
                                m_nSet_GAIN_POS_I = 228;
                                m_nSet_GAIN_POS_I_Size = 4;
                                m_nSet_GAIN_POS_D = 224;
                                m_nSet_GAIN_POS_D_Size = 4;
                                m_nSet_GAIN_VEL_P = 216;
                                m_nSet_GAIN_VEL_P_Size = 4;
                                m_nSet_GAIN_VEL_I = 212;
                                m_nSet_GAIN_VEL_I_Size = 4;
                                m_nSet_GAIN_VEL_D = -1;
                                m_nSet_GAIN_VEL_D_Size = 4;

                                m_nGet_Current_Address = 546;
                                m_nGet_Current_Size = 2;

                                m_nMax_Speed_For_Position = 2020;
                            }
                            break;
                        case EModel_t.P:
                        case EModel_t._PH54_60_250:
                        case EModel_t._PH54_40_250:
                        case EModel_t._PH42_10_260:
                        case EModel_t._PM54_60_250:
                        case EModel_t._PM54_40_250:
                        case EModel_t._PM42_10_260:
                            {
                                m_nProtocol = 2;
                                m_nSet_Operation_Address = 11;
                                m_nSet_Operation_Size = 1;
                                m_nSet_Torq_Address = 512;
                                m_nSet_Torq_Size = 1;
                                m_nSet_Led_Address = 513;
                                m_nSet_Led_Size = 1;
                                m_nSet_Curr_Address = 550;
                                m_nSet_Curr_Size = 2;
                                m_nSet_Speed_Address = 552;
                                m_nSet_Speed_Size = 4;
                                m_nSet_Position_Speed_Address = 560;
                                m_nSet_Position_Speed_Size = 4;
                                m_nSet_Position_Address = 564;
                                m_nSet_Position_Size = 4;

                                m_fMechMove = 1003846.0f;
                                m_fCenter = 0f;
                                m_fMechAngle = 360;
                                m_fJointRpm = 0.01f;
                                m_bDirReverse = false;
                                m_fMulti = 1.0f;
                                m_nGet_Position_Address = 580;
                                m_nGet_Position_Size = 4;

                                m_nSet_GoalCurrent_Address = 550;
                                m_nSet_GoalCurrent_Size = 2;
                                m_nSet_GAIN_POS_P = 532;
                                m_nSet_GAIN_POS_P_Size = 2;
                                m_nSet_GAIN_POS_I = 530;
                                m_nSet_GAIN_POS_I_Size = 2;
                                m_nSet_GAIN_POS_D = 528;
                                m_nSet_GAIN_POS_D_Size = 2;
                                m_nSet_GAIN_VEL_P = 526;
                                m_nSet_GAIN_VEL_P_Size = 2;
                                m_nSet_GAIN_VEL_I = 524;
                                m_nSet_GAIN_VEL_I_Size = 2;
                                m_nSet_GAIN_VEL_D = -1;
                                m_nSet_GAIN_VEL_D_Size = 2;

                                m_nGet_Current_Address = 574;
                                m_nGet_Current_Size = 2;

                                m_nMax_Speed_For_Position = 0;
                            }
                            break;
                        #endregion Protocol 2.0

                        #region Protocol 1.0 (CProtocol1 과 동일 프리셋)
                        case EModel_t.AX:
                        case EModel_t._AX_12:
                        case EModel_t._AX_18:
                        case EModel_t._AX_12W:
                        case EModel_t.DX:
                        case EModel_t._DX_113:
                        case EModel_t._DX_116:
                        case EModel_t._DX_117:
                        case EModel_t.RX:
                        case EModel_t._RX_10:
                        case EModel_t._RX_24F:
                        case EModel_t._RX_28:
                        case EModel_t._RX_64:
                            {
                                SetParam_Common_P1();
                                m_fMechMove = 1024.0f;
                                m_fCenter = 512.0f;
                                m_fMechAngle = 300;
                                m_fJointRpm = 0.111f;
                                m_nSet_GAIN_POS_P = -1; // 컴플라이언스(26~29) 방식 — PID 없음
                                m_nSet_GAIN_POS_I = -1;
                                m_nSet_GAIN_POS_D = -1;
                            }
                            break;
                        case EModel_t.EX:
                        case EModel_t._EX_106:
                        case EModel_t._EX_106P:
                            {
                                SetParam_Common_P1();
                                m_fMechMove = 4095.0f;
                                m_fCenter = 2047.0f;
                                m_fMechAngle = 250.92f;
                                m_fJointRpm = 0.111f;
                                m_nSet_GAIN_POS_P = -1;
                                m_nSet_GAIN_POS_I = -1;
                                m_nSet_GAIN_POS_D = -1;
                            }
                            break;
                        case EModel_t.MX:
                        case EModel_t._MX_12:
                        case EModel_t._MX_28:
                        case EModel_t._MX_64:
                        case EModel_t._MX_106:
                            {
                                SetParam_Common_P1();
                                m_fMechMove = 4096.0f;
                                m_fCenter = 2048.0f;
                                m_fMechAngle = 360;
                                m_fJointRpm = (EModel == EModel_t._MX_12) ? 0.916f : 0.114f;
                                // MX(1.0) PID: D=26, I=27, P=28 (각 1B)
                                m_nSet_GAIN_POS_D = 26;
                                m_nSet_GAIN_POS_D_Size = 1;
                                m_nSet_GAIN_POS_I = 27;
                                m_nSet_GAIN_POS_I_Size = 1;
                                m_nSet_GAIN_POS_P = 28;
                                m_nSet_GAIN_POS_P_Size = 1;
                                if ((EModel == EModel_t._MX_64) || (EModel == EModel_t._MX_106))
                                {
                                    m_nSet_GoalCurrent_Address = 71; // Goal Torque
                                    m_nSet_GoalCurrent_Size = 2;
                                }
                            }
                            break;
                        #endregion Protocol 1.0
                    }
                }
                // P1 공통 주소 (COjw_40 CProtocol1 과 동일)
                private void SetParam_Common_P1()
                {
                    m_nProtocol = 1;
                    m_nSet_Operation_Address = 6;   // CW/CCW Angle Limit — SetOperation 참조
                    m_nSet_Operation_Size = 4;
                    m_nSet_Torq_Address = 24;
                    m_nSet_Torq_Size = 1;
                    m_nSet_Led_Address = 25;
                    m_nSet_Led_Size = 1;
                    m_nSet_Curr_Address = 34;       // Torque Limit
                    m_nSet_Curr_Size = 2;
                    m_nSet_Speed_Address = 32;
                    m_nSet_Speed_Size = 2;
                    m_nSet_Position_Speed_Address = 32;
                    m_nSet_Position_Speed_Size = 2;
                    m_nSet_Position_Address = 30;
                    m_nSet_Position_Size = 2;
                    m_nGet_Position_Address = 36;
                    m_nGet_Position_Size = 2;
                    m_nGet_Current_Address = 40;    // Present Load
                    m_nGet_Current_Size = 2;
                    m_bDirReverse = false;
                    m_fMulti = 1.0f;
                    m_nSet_GoalCurrent_Address = -1;
                    m_nSet_GAIN_VEL_P = -1;
                    m_nSet_GAIN_VEL_I = -1;
                    m_nSet_GAIN_VEL_D = -1;
                    m_nMax_Speed_For_Position = 0;
                }
                public void SetParam(bool bSetHight = false)
                {
                    // 현재 선언된 프로토콜 유지 (P1 선언 후 SetParam(SMotorInfo_t[]) 가 와도 P1 유지)
                    if (m_nProtocol == 1)
                    {
                        SetParams_Model((bSetHight == true) ? EModel_t.MX : EModel_t.AX);
                    }
                    else if (bSetHight == true) // PH, H54(Pro) ...
                    {
                        SetParams_Model(EModel_t.P);
                    }
                    else
                    {
                        SetParams_Model(EModel_t.X);
                    }
                }
            }
            public CParam_t[] m_aCParam = new CParam_t[256];

            // ==== 이하 골격은 CProtocol2 (COjw_37) 와 동일 — 패킷 계층만 프로토콜 분기 ====

            #region Open / Close / IsOpen
            private Ojw.CSocket m_CSock_Client = new CSocket();
            public Ojw.CSerial m_CSerial = new CSerial();
            public bool IsOpen() { return (m_CSerial.IsConnect() || m_CSock_Client.IsConnect()) ? true : false; }
            public bool Open(int nPort, string strAddress)
            {
                bool bConnected = m_CSock_Client.IsConnect();
                if (bConnected == false)
                {
                    if (m_CSock_Client.Connect(strAddress, nPort) == true)
                    {
                        return true;
                    }
                }
                Ojw.CMessage.Write_Error("Cannot Connect [Open_Socket({0}, {1})]", nPort, strAddress);
                return false;
            }
            public bool Open(int nPort, int nBaudRate)
            {
                bool bConnected = m_CSerial.IsConnect();
                if (bConnected == false)
                {
                    // 최대 3회 재시도 (포트 해제 지연 대응)
                    for (int nRetry = 0; nRetry < 3; nRetry++)
                    {
                        if (m_CSerial.Connect(nPort, nBaudRate) == true)
                        {
                            // 파서 상태 초기화
                            Receive_ResetParser();
                            return true;
                        }
                        System.Threading.Thread.Sleep(200);
                    }
                }
                Ojw.CMessage.Write_Error("Cannot Connect [Open_Serial({0}, {1})]", nPort, nBaudRate);
                return false;
            }
            public void Close()
            {
                if (m_CSerial.IsConnect()) m_CSerial.DisConnect();
                if (m_CSock_Client.IsConnect()) m_CSock_Client.DisConnect();
            }

            Ojw.C3d m_C3d = null;
            private bool m_bSyncRendering = false;
            public void SyncRendering(Ojw.C3d OjwC3d)
            {
                if (OjwC3d != null) m_bSyncRendering = true;
                else m_bSyncRendering = false;
                m_C3d = OjwC3d;
            }
            #endregion Open / Close / IsOpen

            #region Socket Server (CProtocol2 와 동일)
            private Ojw.CServer m_CServer = new CServer();
            private Thread m_thServer;
            private bool m_bWebSocket = false;
            private int m_nSocket_Mode = 0; // 0 : Normal Mode, 1 : Bypass Mode;
            public void Socket_BypassMode(bool bBypass) { m_nSocket_Mode = (bBypass) ? 1 : 0; }
            public bool IsSocket_BypassMode() { return (m_nSocket_Mode == 1) ? true : false; }
            public bool Open_Socket(int nPort) { return Open_Socket(nPort, null, false); }
            public bool Open_Socket(int nPort, bool bWebSocket) { return Open_Socket(nPort, null, bWebSocket); }
            public bool Open_Socket(int nPort, string strIP) { return Open_Socket(nPort, strIP, false); }
            public bool IsOpen_Socket() { return m_CServer.sock_started(); }
            public void Close_Socket()
            {
                m_bThreadStart = false;
                m_CServer.sock_stop();
            }
            public bool Open_Socket(int nPort, string strIP, bool bWebSocket)
            {
                if (m_CServer.sock_started() == false)
                {
                    if (strIP == null) m_CServer.sock_start(nPort);
                    else
                    {
                        if (strIP.Length < 7) m_CServer.sock_start(nPort);
                        else m_CServer.sock_start(strIP, nPort);
                    }

                    if (m_CServer.sock_started() == true)
                    {
                        m_bWebSocket = bWebSocket;
                        m_thServer = new Thread(new ThreadStart(ThreadServer));
                        m_thServer.Start();
                    }
                    else
                    {
                        Ojw.printf_Error("서버 동작 실패");
                        Ojw.newline();
                    }
                }
                else
                {
                    Ojw.printf_Error("서버가 이미 동작하고 있습니다.");
                    Ojw.newline();
                }
                return IsOpen_Socket();
            }

            private const int _SIZE_QUE = 3;
            private const int _SIZE_QUE_LENGTH = 512;
            private int m_nQue_Index_Next = 0;
            private int m_nQue_Index = 0;
            private int m_nQue_Count = 0;
            private byte[,] m_abyteQue = new byte[_SIZE_QUE, _SIZE_QUE_LENGTH];
            private bool m_bThreadStart = false;
            private void ThreadServer()
            {
                try
                {
                    m_bThreadStart = true;
                    for (int i = 0; i < _SIZE_QUE; i++)
                        for (int j = 0; j < _SIZE_QUE_LENGTH; j++)
                            m_abyteQue[i, j] = 0;
                    Ojw.printf("ThreadServer()\r\n");
                    m_CServer.WaitClient(true);
                    Ojw.printf("ThreadServer() - Started\r\n");
                    while ((m_CServer.sock_started() == true) && (m_bThreadStart))
                    {
                        if (m_CServer.isClientConnected() == false)
                        {
                            Ojw.printf("소켓연결 끊어짐\r\n");
                            m_CServer.WaitClient(true);
                        }

                        int nBufferSize = m_CServer.GetBuffer_Length();
                        if (nBufferSize > 0)
                        {
                            byte[] pbyData = m_CServer.sock_get_bytes(nBufferSize);

                            if (IsSocket_BypassMode() == true)
                            {
                                SendPacket(pbyData, pbyData.Length);
                            }
                            else
                            {
                                int nSize2 = m_abyteQue[m_nQue_Index_Next, 0] + m_abyteQue[m_nQue_Index_Next, 1] * 256;
                                if (nSize2 < 0) nSize2 = 0;
                                else if (nSize2 > _SIZE_QUE_LENGTH) nSize2 = _SIZE_QUE_LENGTH;
                                if (m_bWebSocket == true)
                                {
                                    string str = Ojw.CConvert.BytesToStr_UTF8(pbyData);
                                    for (int i = 0; i < str.Length; i++)
                                    {
                                        if (str[i] == '!')
                                        {
                                            nSize2 = 0;
                                        }
                                        else if (str[i] == ';')
                                        {
                                            m_abyteQue[m_nQue_Index_Next, 0] = (byte)(nSize2 & 0xff);
                                            m_abyteQue[m_nQue_Index_Next, 1] = (byte)((nSize2 >> 8) & 0xff);
                                            m_nQue_Index = m_nQue_Index_Next;
                                            m_nQue_Index_Next++;
                                            m_nQue_Count++;
                                        }
                                        else
                                        {
                                            m_abyteQue[m_nQue_Index_Next, 2 + nSize2++] = (byte)str[i];
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < nBufferSize; i++)
                                    {
                                        if (pbyData[i] == 0x02)
                                        {
                                            nSize2 = 0;
                                        }
                                        else if (pbyData[i] == 0x03)
                                        {
                                            m_abyteQue[m_nQue_Index_Next, 0] = (byte)(nSize2 & 0xff);
                                            m_abyteQue[m_nQue_Index_Next, 1] = (byte)((nSize2 >> 8) & 0xff);
                                            m_nQue_Index = m_nQue_Index_Next;
                                            m_nQue_Index_Next++;
                                            m_nQue_Count++;
                                        }
                                        else
                                        {
                                            m_abyteQue[m_nQue_Index_Next, 2 + nSize2++] = pbyData[i];
                                        }
                                    }
                                }
                                if (m_nQue_Index_Next >= _SIZE_QUE)
                                {
                                    m_nQue_Index_Next = 0;
                                }
                                if (m_nQue_Count > _SIZE_QUE)
                                {
                                    m_nQue_Count = _SIZE_QUE;
                                }
                            }
                        }
                        if (m_nQue_Count > 0)
                        {
                            int nLen = m_abyteQue[m_nQue_Index, 0] + m_abyteQue[m_nQue_Index, 1] * 256;
                            string str = String.Empty;
                            byte[] pbyData = new byte[nLen];
                            for (int i = 0; i < nLen; i++)
                            {
                                pbyData[i] = (m_abyteQue[m_nQue_Index, 2 + i]);
                            }
                            str = Ojw.CConvert.BytesToStr_UTF8(pbyData);

                            PlayFrameString(str, true);
                            m_nQue_Count--;
                        }
                        Thread.Sleep(1);
                    }
                }
                catch (Exception e)
                {
                    Ojw.printf_Error(e.ToString());
                }
            }
            #endregion Socket Server

            #region Command
            public class CCommand_t
            {
                public int nID = 0;
                public float fVal = 0;
                public CCommand_t(int id, float val)
                {
                    nID = id;
                    fVal = val;
                }
            }
            private List<CCommand_t> m_lstCmdIDs = new List<CCommand_t>();
            public void Command_Clear() { m_lstCmdIDs.Clear(); }
            public void Command_Set(int nID, float fValue) { m_lstCmdIDs.Add(new CCommand_t(nID, CalcLimit(nID, fValue))); }
            public void Command_Set_Rpm(int nID, float fRpm) { m_lstCmdIDs.Add(new CCommand_t(nID, CalcRpm2Raw(nID, fRpm))); }
            public void Clear() { Command_Clear(); }
            public void Set(int nID, float fValue) { Command_Set(nID, fValue); }
            public void Set_Rpm(int nID, float fRpm) { Command_Set_Rpm(nID, fRpm); }
            public float GetAngle(int nID) { return m_afMot[nID]; }
            public void SetTorq(bool bOn)
            {
                // 브로드캐스트: 두 프로토콜 모두 전송 (P2 X=64 / P1=24)
                byte byVal = (byte)((bOn == true) ? 1 : 0);
                Send_P2(254, 0x03, 64, byVal);
                Send_P1(254, 0x03, 24, byVal);
            }
            public void SetTorq(params float[] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i * 2], afVals[i * 2 + 1]); }
                SetTorq(aCCommands);
            }
            public void SetTorq() { SetTorq(m_lstCmdIDs.ToArray()); }
            public void SetTorq(params CCommand_t[] aCCommands)
            {
                // 혼재 ID 는 lstSecond 루프가 주소(=프로토콜) 별로 자동 분리 → 연속 송출
                List<CCommand_t> lstSecond = new List<CCommand_t>();
                while (true)
                {
                    Sync_Clear();
                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    Command_Clear();
                    if (lstSecond.Count > 0)
                    {
                        CCmd = lstSecond.ToArray();
                        lstSecond.Clear();
                    }
                    if (CCmd == null) break;
                    if (CCmd.Length > 0)
                    {
                        for (int i = 0; i < CCmd.Length; i++)
                        {
                            if (m_aCParam[CCmd[0].nID].m_nSet_Torq_Address != m_aCParam[CCmd[i].nID].m_nSet_Torq_Address)
                            {
                                lstSecond.Add(new CCommand_t(CCmd[i].nID, CCmd[i].fVal));
                            }
                            else Sync_Push_Byte(CCmd[i].nID, (int)Math.Round(CCmd[i].fVal));
                        }
                        Sync_Flush(m_aCParam[CCmd[0].nID].m_nSet_Torq_Address);
                    }
                    if (lstSecond.Count == 0) break;
                }
            }
            public void SetCurr(int nID, int nValue)
            {
                // P2 = Goal Current, P1 = Torque Limit(34) — 모두 2바이트 (주소는 모터 파라미터)
                Send(nID, 0x03, m_aCParam[nID].m_nSet_Curr_Address, Ojw.CConvert.ShortToBytes((short)nValue));
            }
            public void SetLed(bool bOn)
            {
                byte byVal = (byte)((bOn == true) ? 1 : 0);
                Send_P2(254, 0x03, 65, byVal);
                Send_P1(254, 0x03, 25, byVal);
            }
            public void SetLed(params float[] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i * 2], afVals[i * 2 + 1]); }
                SetLed(aCCommands);
            }
            public void SetLed(params CCommand_t[] aCCommands)
            {
                List<CCommand_t> lstSecond = new List<CCommand_t>();
                while (true)
                {
                    Sync_Clear();
                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    Command_Clear();
                    if (lstSecond.Count > 0)
                    {
                        CCmd = lstSecond.ToArray();
                        lstSecond.Clear();
                    }
                    if (CCmd == null) break;
                    if (CCmd.Length > 0)
                    {
                        for (int i = 0; i < CCmd.Length; i++)
                        {
                            if (m_aCParam[CCmd[0].nID].m_nSet_Led_Address != m_aCParam[CCmd[i].nID].m_nSet_Led_Address)
                            {
                                lstSecond.Add(new CCommand_t(CCmd[i].nID, CCmd[i].fVal));
                            }
                            else Sync_Push_Byte(CCmd[i].nID, (int)Math.Round(CCmd[i].fVal));
                        }
                        Sync_Flush(m_aCParam[CCmd[0].nID].m_nSet_Led_Address);
                    }
                    if (lstSecond.Count == 0) break;
                }
            }
            public void SetOperation(int nID = 254, int nMode = 3)
            {
                // P2: Operating Mode 레지스터 기록 / P1: CW/CCW Angle Limit(6/8) 로 모드 결정
                //   (P1 은 EEPROM — 토크 OFF 상태에서 설정할 것)
                if (nID == 254)
                {
                    Send_P2(254, 0x03, 11, (byte)nMode);
                    SetOperation_P1(254, nMode, 0); // 브로드캐스트: AX 분해능 기준
                    return;
                }
                if (m_aCParam[nID].m_nProtocol == 1) SetOperation_P1(nID, nMode, nID);
                else SetCommand(nID, m_aCParam[nID].m_nSet_Operation_Address, nMode);
            }
            private void SetOperation_P1(int nID, int nMode, int nRefID)
            {
                // nMode 1 = wheel(0/0), 3 = joint(0/res-1), 4 = 멀티턴(4095/4095)
                int nMax = (int)Math.Round(m_aCParam[nRefID].m_fMechMove) - 1;
                int nCw = 0, nCcw = nMax;
                if (nMode == 1) { nCw = 0; nCcw = 0; }
                else if (nMode == 4) { nCw = 4095; nCcw = 4095; }
                byte[] abyDatas = new byte[4];
                abyDatas[0] = (byte)(nCw & 0xff);
                abyDatas[1] = (byte)((nCw >> 8) & 0xff);
                abyDatas[2] = (byte)(nCcw & 0xff);
                abyDatas[3] = (byte)((nCcw >> 8) & 0xff);
                Send_P1(nID, 0x03, 6, abyDatas);
            }
            public void SetCommand(int nID, int nAddress, int nData, int nSize = 1)
            {
                if (nSize <= 1) Send(nID, 0x03, nAddress, (byte)nData);
                else
                {
                    byte[] buffer;
                    if (nSize == 2)
                        buffer = Ojw.CConvert.ShortToBytes((short)nData);
                    else if (nSize == 4)
                        buffer = Ojw.CConvert.IntToBytes((int)nData);
                    else
                        buffer = Ojw.CConvert.LongToBytes((long)nData);
                    Send(nID, 0x03, nAddress, buffer);
                }
            }
            public bool IsEms() { return m_bEms; }
            public void Reset()
            {
                m_bEms = false;
            }

            // P1 휠모드 속도값: 10bit 크기 + bit10(0x400) = 방향 (P2 는 부호있는 값 그대로)
            private int CalcWheelRaw(int nID, int nVal)
            {
                if (m_aCParam[nID].m_nProtocol != 1) return nVal;
                if (nVal < 0)
                {
                    nVal = -nVal;
                    if (nVal > 0x3FF) nVal = 0x3FF;
                    nVal |= 0x400;
                }
                else if (nVal > 0x3FF) nVal = 0x3FF;
                return nVal;
            }
            // 데이터 크기별 Sync push (P1 위치/속도 = 2바이트, P2 = 4바이트)
            private void Sync_Push_BySize(int nID, int nData, int nSize)
            {
                if (nSize <= 1) Sync_Push_Byte(nID, nData);
                else if (nSize == 2) Sync_Push_Word(nID, nData);
                else Sync_Push_Dword(nID, nData);
            }
            public void SetSpeed(params float[] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i * 2], afVals[i * 2 + 1]); }
                SetSpeed(aCCommands);
            }
            public void SetSpeed(params CCommand_t[] aCCommands)
            {
                List<CCommand_t> lstSecond = new List<CCommand_t>();
                while (true)
                {
                    Sync_Clear();
                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    Command_Clear();
                    if (lstSecond.Count > 0)
                    {
                        CCmd = lstSecond.ToArray();
                        lstSecond.Clear();
                    }
                    if (CCmd == null) break;
                    if (CCmd.Length > 0)
                    {
                        for (int i = 0; i < CCmd.Length; i++)
                        {
                            float fMul = m_aCParam[CCmd[i].nID].m_fMulti * ((m_aCParam[CCmd[i].nID].m_bDirReverse == false) ? 1 : -1);
                            if (m_aCParam[CCmd[0].nID].m_nSet_Speed_Address != m_aCParam[CCmd[i].nID].m_nSet_Speed_Address)
                            {
                                lstSecond.Add(new CCommand_t(CCmd[i].nID, CCmd[i].fVal * fMul));
                            }
                            else Sync_Push_BySize(CCmd[i].nID, CalcWheelRaw(CCmd[i].nID, (int)Math.Round(CCmd[i].fVal * fMul)), m_aCParam[CCmd[i].nID].m_nSet_Speed_Size);
                        }
                        Sync_Flush(m_aCParam[CCmd[0].nID].m_nSet_Speed_Address);
                    }
                    if (lstSecond.Count == 0) break;
                }
            }

            public void SetPosition_Speed(params float[] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i * 2], afVals[i * 2 + 1]); }
                SetPosition_Speed(aCCommands);
            }
            public void SetPosition_Speed() { SetPosition_Speed(m_lstCmdIDs.ToArray()); }
            public void SetPosition_Speed(params CCommand_t[] aCCommands)
            {
                List<CCommand_t> lstSecond = new List<CCommand_t>();
                while (true)
                {
                    Sync_Clear();
                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    if (lstSecond.Count > 0)
                    {
                        CCmd = lstSecond.ToArray();
                        lstSecond.Clear();
                    }
                    if (CCmd == null) break;
                    if (CCmd.Length > 0)
                    {
                        for (int i = 0; i < CCmd.Length; i++)
                        {
                            if (m_aCParam[CCmd[0].nID].m_nSet_Position_Speed_Address != m_aCParam[CCmd[i].nID].m_nSet_Position_Speed_Address)
                            {
                                lstSecond.Add(new CCommand_t(CCmd[i].nID, CCmd[i].fVal));
                            }
                            else
                            {
                                int nVal = (int)Math.Round(CCmd[i].fVal);

                                if (nVal == 0) nVal = m_aCParam[CCmd[i].nID].m_nMax_Speed_For_Position;
                                if (m_aCParam[CCmd[i].nID].m_nProtocol == 1)
                                {
                                    // P1 관절모드 이동속도: 0~1023 (0 = 최대), 방향비트 없음
                                    if (nVal < 0) nVal = -nVal;
                                    if (nVal > 0x3FF) nVal = 0x3FF;
                                }

                                Sync_Push_BySize(CCmd[i].nID, nVal, m_aCParam[CCmd[i].nID].m_nSet_Position_Speed_Size);
                            }
                        }
                        Sync_Flush(m_aCParam[CCmd[0].nID].m_nSet_Position_Speed_Address);
                    }
                    if (lstSecond.Count == 0) break;
                }
                Command_Clear();
            }
            public void SetPosition(params float[] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i * 2], afVals[i * 2 + 1]); }
                SetPosition(aCCommands);
            }
            public void SetPosition() { SetPosition(m_lstCmdIDs.ToArray()); }
            public void SetPosition(params CCommand_t[] aCCommands)
            {
                // 혼재 ID (예: AX + XL430) → 주소 차이로 자동 분리, P1/P2 패킷 연속 송출 = 동시 제어
                List<CCommand_t> lstSecond = new List<CCommand_t>();
                while (true)
                {
                    Sync_Clear();
                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    if (lstSecond.Count > 0)
                    {
                        CCmd = lstSecond.ToArray();
                        lstSecond.Clear();
                    }
                    if (CCmd == null) break;
                    if (CCmd.Length > 0)
                    {
                        for (int i = 0; i < CCmd.Length; i++)
                        {
                            if (m_aCParam[CCmd[0].nID].m_nSet_Position_Address != m_aCParam[CCmd[i].nID].m_nSet_Position_Address)
                            {
                                lstSecond.Add(new CCommand_t(CCmd[i].nID, CCmd[i].fVal));
                            }
                            else
                            {
                                m_afMot[CCmd[i].nID] = CCmd[i].fVal;
                                m_afMot_Pose[CCmd[i].nID] = CCmd[i].fVal;
                                Sync_Push_BySize(CCmd[i].nID, CalcAngle2Evd(CCmd[i].nID, CCmd[i].fVal), m_aCParam[CCmd[i].nID].m_nSet_Position_Size);
                                //3D
                                if (m_bSyncRendering) m_C3d.SetData(CCmd[i].nID, CCmd[i].fVal);
                            }
                        }
                        Sync_Flush(m_aCParam[CCmd[0].nID].m_nSet_Position_Address);
                    }
                    if (lstSecond.Count == 0) break;
                }
                Command_Clear();
            }
            #endregion Command

            #region Calc
            public float CalcEvd2Angle(int nID, int nValue)
            {
                float fMul = m_aCParam[nID].m_fMulti * ((m_aCParam[nID].m_bDirReverse == false) ? 1 : -1);
                if (fMul == 0) fMul = 1;
                float fMechMove = m_aCParam[nID].m_fMechMove;
                float fCenterPos = m_aCParam[nID].m_fCenter;
                float fMechAngle = m_aCParam[nID].m_fMechAngle;

                return (((fMechAngle * ((float)nValue - fCenterPos)) / fMechMove) * fMul);
            }
            public int CalcAngle2Evd(int nID, float fValue)
            {
                float fMul = m_aCParam[nID].m_fMulti * ((m_aCParam[nID].m_bDirReverse == false) ? 1 : -1);
                if (fMul == 0) fMul = 1;

                float fMechMove = m_aCParam[nID].m_fMechMove;
                float fCenterPos = m_aCParam[nID].m_fCenter;
                float fMechAngle = m_aCParam[nID].m_fMechAngle;
                return (int)Math.Round(((fMechMove * fValue) / fMechAngle * fMul + fCenterPos));
            }
            public int CalcPosition_Time(int nAxis, int nTime, int nDelay, float fAngle)
            {
                float fRpm = (float)Math.Abs(CalcTime2Rpm(Math.Abs(fAngle - m_afMot_Pose[nAxis]), (float)nTime));
                return CalcRpm2Raw(nAxis, fRpm);
            }
            public float CalcRaw2Rpm(int nID, int nValue) { return (float)nValue * m_aCParam[nID].m_fJointRpm; }
            public int CalcRpm2Raw(int nID, float fRpm) { return (int)Math.Round(fRpm / m_aCParam[nID].m_fJointRpm); }
            public float CalcTime2Rpm(float fDeltaAngle, float fTime)
            {
                // 1도 이동시간 => 60000 / (Rpm * 360)
                return (60.0f * fDeltaAngle * 1000.0f) / (360.0f * fTime);
            }
            #endregion Calc

            #region Reboot / Ems
            private bool m_bEms = false; // emergency switch
            public void Ems()
            {
                m_bEms = true;
                // P2 (X=64, PRO/Y=512) + P1 (24) 모두 off → on
                Send_P2(254, 0x03, 64, 0);
                Send_P2(254, 0x03, 512, 0);
                Send_P1(254, 0x03, 24, 0);

                Send_P2(254, 0x03, 64, 1);
                Send_P2(254, 0x03, 512, 1);
                Send_P1(254, 0x03, 24, 1);
            }
            private bool m_bBreak = false;
            private bool m_bBreak2 = false;
            private void SetBreak() { m_bBreak = true; }
            public void Reboot(int nMotor = 254)
            {
                // REBOOT(0x08) 는 P2 전용 — P1 모터는 무시함 (P1 은 전원 재인가 필요)
                if (nMotor == 254) { for (int i = 0; i < m_abMot.Length; i++) { m_abMot[i] = false; } }
                else { m_abMot[nMotor] = false; }
                Send_Command(nMotor, 0x08);
            }
            public void Send_Command(int nMotor, int nCommand)
            {
                Send(nMotor, nCommand, 0, null);
            }
            #endregion Reboot / Ems

            #region Move / Play (CProtocol2 와 동일 — 프로토콜 무관 상위 로직)
            private int m_nRun_Time = 0;
            private int m_nRun_Delay = 0;
            private CCommand_t[] m_aCRun_Commands;
            private void FThread_Run()
            {
                try
                {
                    Ojw.CMessage.Write("FThread_Run -> Start");

                    Ojw.CTimer CTmr = new CTimer();
                    CTmr.Set();

                    int nTime_ms = m_nRun_Time;
                    int nDelay = m_nRun_Delay;
                    CCommand_t[] aCCommands = m_aCRun_Commands;

                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    Command_Clear();
                    if (CCmd.Length > 0)
                    {
                        float[] afMot = new float[m_afMot.Length];
                        float[] afRes = new float[m_afMot.Length];
                        Array.Copy(m_afMot, afMot, m_afMot.Length);
                        m_bBreak = false;
                        while (true)
                        {
                            if ((m_bEms == true) || (m_bBreak == true))
                            {
                                if (m_bBreak == true) m_bBreak = false;
                                for (int i = 0; i < CCmd.Length; i++) { m_afMot_Pose[CCmd[i].nID] = m_afMot[CCmd[i].nID] = afRes[CCmd[i].nID]; }
                                return;
                            }

                            List<int> lstIDs = new List<int>();
                            lstIDs.Clear();
                            for (int i = 0; i < CCmd.Length; i++) { lstIDs.Add(CCmd[i].nID); Command_Set(CCmd[i].nID, 0); }
                            SetPosition_Speed(m_lstCmdIDs.ToArray());

                            float fGet = CTmr.Get();
                            float fTmr = (fGet / (float)nTime_ms);
                            if (fTmr > 1f) fTmr = 1f;

                            // -Delay 탈출
                            if (m_bBreak2 == true)
                            {
                                m_bBreak2 = false;
                                m_bBreak = true;
                            }
                            if (m_bBreak == true)
                            {
                                float fTmr_Sub = (nDelay >= 0) ? 0 : (fGet / (float)(nTime_ms + nDelay));
                                if (fTmr_Sub >= 1f) break;
                            }

                            for (int i = 0; i < CCmd.Length; i++) { afRes[CCmd[i].nID] = afMot[CCmd[i].nID] + (CCmd[i].fVal - afMot[CCmd[i].nID]) * fTmr; Command_Set(CCmd[i].nID, afRes[CCmd[i].nID]); }
                            SetPosition(m_lstCmdIDs.ToArray());

                            if (fTmr >= 1f) break;
                            Ojw.CTimer.DoEvent();
                        }
                        for (int i = 0; i < CCmd.Length; i++) { m_afMot_Pose[CCmd[i].nID] = m_afMot[CCmd[i].nID] = afRes[CCmd[i].nID]; }

                        while (true)
                        {
                            if (CTmr.Get() >= (nTime_ms + nDelay)) break;
                            Ojw.CTimer.DoEvent();
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            private bool m_bNext = false;
            public void PlayFile_Next() { m_bNext = true; }
            public void PlayFile(string strFileName, bool bNowait = false)
            {
                if (IsOpen() == false) return;

                Ojw.Log("Play - {0}", strFileName);

                Ojw.CFile CFile = new Ojw.CFile();
                if (CFile.Load(strFileName) == 0) return;

                int nCnt = CFile.Get_Count();
                int nMax = 0;
                for (int i = 0; i < nCnt; i++)
                {
                    string str = CFile.Get(i);
                    string[] pstr = str.Split(',');
                    int nEnable = Ojw.CConvert.StrToInt(pstr[0]);
                    if (nEnable > 0) nMax = i;
                }
                for (int i = 0; i <= nMax; i++)
                {
                    string str = CFile.Get(i);
                    if (str.Length > 0)
                    {
                        if (str[0] != 's') str = 's' + str;
                        PlayFrameString(str, bNowait);
                    }
                }
                Ojw.Log("Done - {0}", strFileName);
            }
            // bypass 모드가 아닌 경우에 발동
            public void Play_Stream(string str)
            {
                string[] pstr = str.Split(',');
                if (pstr.Length >= 3)
                {
                    int nTime_ms = Ojw.CConvert.StrToInt(pstr[1]);
                    int nDelay = Ojw.CConvert.StrToInt(pstr[2]);
                    m_nWait_Time = (nTime_ms + nDelay);
                }
                byte[] pbuff = Ojw.CConvert.StrToBytes_UTF8(str);
                byte[] pbyte = new byte[pbuff.Length + 2];
                pbyte[0] = 0x02;
                for (int i = 1; i < pbyte.Length - 1; i++)
                {
                    pbyte[i] = pbuff[i - 1];
                }
                pbyte[pbyte.Length - 1] = 0x03;
                m_CSock_Client.Send(pbyte);
            }
            // Time, Delay, [id, pos, id, pos, id, pos...]
            // Time <= 0 이면 속도모드 값으로 동작한다.
            public bool Play(String str)
            {
                string[] pstr = str.Split(',');
                float[] afDatas = new float[pstr.Length];
                for (int i = 0; i < pstr.Length; i++)
                {
                    afDatas[i] = CConvert.StrToFloat(pstr[i]);
                }
                return Play(afDatas);
            }
            public bool Play(params float[] afDatas)
            {
                if (afDatas.Length > 2)
                {
                    string strData = "";
                    try
                    {
                        if ((afDatas.Length % 2) == 0)
                        {
                            int nTime = (int)afDatas[0];
                            if (nTime <= 0) { strData = "s4,"; nTime = 0; }
                            else strData = "s2,";
                            int nDelay = (int)afDatas[1];
                            strData += Ojw.CConvert.IntToStr(nTime) + "," + Ojw.CConvert.IntToStr(nDelay) + ",";
                            for (int i = 2; i < afDatas.Length; i += 2)
                            {
                                strData += Ojw.CConvert.IntToStr((int)afDatas[i]) + ":" + Ojw.CConvert.FloatToStr(afDatas[i + 1]);
                                if ((i + 2) < afDatas.Length) strData += ",";
                            }

                            PlayFrameString(strData, false);
                            return true;
                        }

                    }
                    catch (Exception e)
                    {
                        Ojw.CMessage.Write_Error(strData + ": " + e.ToString());
                        return false;
                    }
                }
                return false;
            }
            public bool Play_NoWait(params float[] afDatas)
            {
                if (afDatas.Length > 2)
                {
                    string strData = "";
                    try
                    {
                        if ((afDatas.Length % 2) == 0)
                        {
                            int nTime = (int)afDatas[0];
                            if (nTime <= 0) { strData = "s4,"; nTime = 0; } // s3 은 속도, s4 는 속도 인데 RPM
                            else strData = "s2,";
                            strData += Ojw.CConvert.IntToStr(nTime) + "," + Ojw.CConvert.IntToStr((int)afDatas[1]) + ",";
                            for (int i = 2; i < afDatas.Length; i += 2)
                            {
                                strData += Ojw.CConvert.IntToStr((int)afDatas[i]) + ":" + Ojw.CConvert.FloatToStr(afDatas[i + 1]);
                                if ((i + 2) < afDatas.Length) strData += ",";
                            }

                            PlayFrameString(strData, true);
                            return true;
                        }

                    }
                    catch (Exception e)
                    {
                        Ojw.CMessage.Write_Error(strData + ": " + e.ToString());
                        return false;
                    }
                }
                return false;
            }
            public bool m_bSimulOnly = false;
            public void SetSimulation_Only(bool bSimul)
            {
                m_bSimulOnly = bSimul;
            }
            public bool SetSimulation_Only()
            {
                return m_bSimulOnly;
            }
            public void Play_Pos(string buff, bool bNoWait = false) { PlayFrameString("s2," + buff, bNoWait); }
            public void Play_Speed(string buff, bool bRpm = true) { PlayFrameString((bRpm ? "s4,0,0," : "s3,0,0,") + buff, true); }
            public void PlayFrameString(string buff, bool bNoWait = false)
            {
                bool bWheel = false;
                if (buff.Length > 1)
                {
                    if (
                        ((buff[1] == '1') || (buff[1] == '2')) // Enable
                        ||
                        ((buff[1] == '3') || (buff[1] == '4')) // Enable
                        || ((buff[1] == '5')) // Write
                    )
                    {
                        bool bAngle = false;
                        bool bWheel_Rpm = false;
                        bool bWrite = false;
                        if ((buff[1] == '3') || (buff[1] == '4')) bWheel = true;
                        if (buff[1] == '2') bAngle = true;
                        if (buff[1] == '4') bWheel_Rpm = true;
                        if (buff[1] == '5') bWrite = true;

                        string[] pstr = buff.Split(',');

                        int nEnable = Ojw.CConvert.StrToInt(pstr[0]);
                        int nTime = Ojw.CConvert.StrToInt(pstr[1]);
                        int nDelay = Ojw.CConvert.StrToInt(pstr[2]);

                        if (bWrite) // 해당 주소에 직접 지령
                        {
                            // ID : Address : Bytes...
                            for (int nIndex = 3; nIndex < pstr.Length; nIndex++)
                            {
                                string[] pstrDatas = pstr[nIndex].Split(':');
                                if (pstrDatas.Length > 1) // id : value : size = 1
                                {
                                    int nID = Ojw.CConvert.StrToInt(pstrDatas[0]);
                                    int nAddress = Ojw.CConvert.StrToInt(pstrDatas[1]);
                                    List<byte> lstBytes = new List<byte>();
                                    lstBytes.Clear();
                                    for (int i = 2; i < pstrDatas.Length; i++)
                                    {
                                        lstBytes.Add((byte)(Ojw.CConvert.StrToInt(pstrDatas[i]) & 0xff));
                                    }
                                    Send(nID, 0x03, nAddress, lstBytes.ToArray());
                                }
                            }
                        }
                        else
                        {
                            Command_Clear();
                            int nCnt_Func = 256;
                            float[] afX = new float[nCnt_Func];
                            float[] afY = new float[nCnt_Func];
                            float[] afZ = new float[nCnt_Func];
                            bool[] abFunction = new bool[nCnt_Func];
                            Array.Clear(abFunction, 0, abFunction.Length);

                            for (int nIndex = 3; nIndex < pstr.Length; nIndex++)
                            {
                                string[] pstrDatas = pstr[nIndex].Split(':');
                                if (pstrDatas.Length > 1)
                                {
                                    int nXyz = -1;
                                    int nFunctionNumber = -1;
                                    // 좌표제어
                                    if (pstrDatas[0].ToUpper().IndexOf("X") == 0) { nXyz = 0; }
                                    else if (pstrDatas[0].ToUpper().IndexOf("Y") == 0) { nXyz = 1; }
                                    else if (pstrDatas[0].ToUpper().IndexOf("Z") == 0) { nXyz = 2; }
                                    #region etc
                                    else
                                    {
                                        int nID = Ojw.CConvert.StrToInt(pstrDatas[0]);

                                        float fEvd = Ojw.CConvert.StrToFloat(pstrDatas[1]);
                                        if ((bAngle) || (bWheel_Rpm))
                                        {
                                            if (bWheel_Rpm) Command_Set_Rpm(nID, fEvd);
                                            else
                                            {
                                                Command_Set(nID, fEvd);

                                                //3D
                                                if (m_bSyncRendering) m_C3d.SetData(nID, fEvd);
                                            }
                                        }
                                        else
                                        {
                                            int nEvd = Ojw.CConvert.StrToInt(pstrDatas[1]);
                                            float fAngle = (float)Math.Round(CalcEvd2Angle(nID, nEvd), 3);
                                            if (bWheel)
                                            {
                                                if (bWheel_Rpm) Command_Set(nID, nEvd);
                                                else Command_Set(nID, CalcRaw2Rpm(nID, nEvd));
                                            }
                                            else Command_Set(nID, fAngle);
                                            //3D
                                            if (m_bSyncRendering) m_C3d.SetData(nID, fAngle);
                                        }
                                    }
                                    #endregion etc

                                    if (nXyz >= 0)
                                    {
                                        nFunctionNumber = Ojw.CConvert.StrToInt(pstrDatas[0].Substring(1));
                                        float[] afFunc = ((nXyz == 0) ? afX : ((nXyz == 1) ? afY : afZ));
                                        if ((nFunctionNumber >= 0) && (nFunctionNumber < afX.Length))
                                        {
                                            abFunction[nFunctionNumber] = true;
                                            afFunc[nFunctionNumber] = Ojw.CConvert.StrToFloat(pstrDatas[1]);
                                        }
                                    }
                                }
                            }

                            ////
                            for (int i = 0; i < nCnt_Func; i++)
                            {
                                if (abFunction[i] == true)
                                {
                                    int[] anIDs;
                                    double[] adValues;
                                    float fX, fY, fZ;
                                    m_C3d.CalcF(i, false, out fX, out fY, out fZ);
                                    if (m_C3d.IsInverseFunction(i) == true)
                                    {
                                        m_C3d.GetData_Inverse(i, (double)fX, (double)fY, (double)fZ, out anIDs, out adValues);

                                    }
                                    else
                                    {
                                        m_C3d.CalcInv(i, fX, fY, fZ);
                                        anIDs = m_C3d.GetInfo_Forward_Motors(i);
                                        adValues = new double[anIDs.Length];
                                        for (int j = 0; j < anIDs.Length; j++)
                                        {
                                            adValues[anIDs[j]] = (double)m_C3d.GetData(anIDs[j]);
                                        }
                                    }
                                    for (int j = 0; j < anIDs.Length; j++)
                                    {
                                        Command_Set(anIDs[j], (float)adValues[j]);
                                        if (m_bSyncRendering) m_C3d.SetData(anIDs[j], (float)adValues[j]);
                                    }
                                }
                            }
                            ////
                            if (m_bSimulOnly == false)
                            {
                                if (bWheel)
                                {
                                    SetSpeed(m_lstCmdIDs.ToArray());
                                }
                                else
                                {
                                    if (bNoWait == false) Move(nTime, nDelay, m_lstCmdIDs.ToArray());
                                    else Move_NoWait(nTime, nDelay, m_lstCmdIDs.ToArray());
                                }
                            }
                        }
                    }
                }
            }

            public bool Move(params float[] afVals)
            {
                // (afVals.Length - 1) 은 안전장치
                int nLen_Off = afVals.Length % 2 * 2;
                int nLen = (int)(Math.Round((afVals.Length - nLen_Off) / 2.0f)) * 2;
                if (nLen < 4) return false;
                int nTime_ms = (int)afVals[0];
                int nDelay = (int)afVals[1];
                if (nTime_ms > 0)
                {
                    CCommand_t[] aCCommands = new CCommand_t[nLen / 2 - 1];
                    for (int i = 2; i < nLen; i += 2)
                    {
                        aCCommands[i / 2 - 1] = new CCommand_t((int)afVals[i], afVals[i + 1]);
                    }
                    Move(nTime_ms, nDelay, aCCommands);
                }
                else
                {
                    for (int i = 2; i < nLen; i += 2) Command_Set_Rpm((int)afVals[i], afVals[i + 1]);
                    SetSpeed(m_lstCmdIDs.ToArray());
                }
                return true;
            }
            public void Move(int nTime_ms, int nDelay, params CCommand_t[] aCCommands) { Move(nTime_ms, nDelay, false, aCCommands); }
            public void Move(int nTime_ms, int nDelay, bool bContinue, params CCommand_t[] aCCommands)
            {
                m_nWait_Time = 0;
                if (IsOpen() == false) return;
                if (m_bEms == true) return;
                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();

                CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                Command_Clear();
                if (CCmd.Length > 0)
                {
                    float[] afMot = new float[m_afMot.Length];
                    float[] afRes = new float[m_afMot.Length];
                    Array.Copy(m_afMot, afMot, m_afMot.Length);
                    while (true)
                    {
                        if (m_bEms == true)
                        {
                            for (int i = 0; i < CCmd.Length; i++) { m_afMot_Pose[CCmd[i].nID] = m_afMot[CCmd[i].nID] = afRes[CCmd[i].nID]; }
                            return;
                        }

                        List<int> lstIDs = new List<int>();
                        lstIDs.Clear();
                        for (int i = 0; i < CCmd.Length; i++) { lstIDs.Add(CCmd[i].nID); Command_Set(CCmd[i].nID, 0); }
                        SetPosition_Speed(m_lstCmdIDs.ToArray());

                        float fGet = CTmr.Get();
                        float fTmr = (fGet / (float)nTime_ms);
                        if (fTmr > 1f) fTmr = 1f;

                        for (int i = 0; i < CCmd.Length; i++) { afRes[CCmd[i].nID] = afMot[CCmd[i].nID] + (CCmd[i].fVal - afMot[CCmd[i].nID]) * fTmr; Command_Set(CCmd[i].nID, afRes[CCmd[i].nID]); }
                        SetPosition(m_lstCmdIDs.ToArray());

                        if (fGet >= (nTime_ms + nDelay)) // 남은 시간값으로 마저 이동
                        {
                            lstIDs.Clear();
                            for (int i = 0; i < CCmd.Length; i++) { lstIDs.Add(CCmd[i].nID); Command_Set(CCmd[i].nID, CalcPosition_Time(CCmd[i].nID, (int)Math.Round(nTime_ms - fGet), 0, CCmd[i].fVal)); }
                            SetPosition_Speed(m_lstCmdIDs.ToArray());

                            for (int i = 0; i < CCmd.Length; i++) { afRes[CCmd[i].nID] = CCmd[i].fVal; Command_Set(CCmd[i].nID, afRes[CCmd[i].nID]); }
                            SetPosition(m_lstCmdIDs.ToArray());
                            break;
                        }

                        if (fTmr >= 1f) break;
                        Ojw.CTimer.DoEvent();
                    }
                    for (int i = 0; i < CCmd.Length; i++) { m_afMot_Pose[CCmd[i].nID] = m_afMot[CCmd[i].nID] = afRes[CCmd[i].nID]; }

                    while (true)
                    {
                        if (CTmr.Get() >= (nTime_ms + nDelay)) break;
                        Ojw.CTimer.DoEvent();
                    }
                }
            }

            private int m_nWait_Time = 0;
            public void Wait(int nTime = -1)
            {
                if (IsOpen() == false) return;
                if (m_bEms == true) return;

                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();

                int nWait = ((nTime < 0) ? m_nWait_Time : nTime);
                m_nWait_Time = 0;
                while (true) { if (CTmr.Get() >= nWait) break; Ojw.CTimer.DoEvent(); }
            }
            public bool Move_NoWait(params float[] afVals)
            {
                int nLen_Off = afVals.Length % 2 * 2;
                int nLen = (int)(Math.Round((afVals.Length - nLen_Off) / 2.0f)) * 2;
                if (nLen < 4) return false;
                int nTime_ms = (int)afVals[0];
                int nDelay = (int)afVals[1];
                if (nTime_ms > 0)
                {
                    CCommand_t[] aCCommands = new CCommand_t[nLen / 2 - 1];
                    for (int i = 2; i < nLen; i += 2)
                    {
                        aCCommands[i / 2 - 1] = new CCommand_t((int)afVals[i], afVals[i + 1]);
                    }
                    Move_NoWait(nTime_ms, nDelay, aCCommands);
                }
                else
                {
                    for (int i = 2; i < nLen; i += 2) Command_Set_Rpm((int)afVals[i], afVals[i + 1]);
                    SetSpeed(m_lstCmdIDs.ToArray());
                }
                return true;
            }
            public void Move_NoWait(int nTime_ms, int nDelay, params CCommand_t[] aCCommands)
            {
                if (IsOpen() == false) return;
                if (m_bEms == true) return;

                m_nWait_Time = (nTime_ms + nDelay);

                CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                Command_Clear();
                if (CCmd.Length > 0)
                {
                    float[] afMot = new float[m_afMot.Length];
                    float[] afRes = new float[m_afMot.Length];
                    Array.Copy(m_afMot, afMot, m_afMot.Length);

                    List<int> lstIDs = new List<int>();
                    lstIDs.Clear();
                    Command_Clear();
                    for (int i = 0; i < CCmd.Length; i++)
                    {
                        lstIDs.Add(CCmd[i].nID);
                        Command_Set(CCmd[i].nID, CalcPosition_Time(CCmd[i].nID, nTime_ms, nDelay, CCmd[i].fVal));
                    }
                    SetPosition_Speed(m_lstCmdIDs.ToArray());

                    Command_Clear();
                    for (int i = 0; i < CCmd.Length; i++)
                    {
                        Command_Set(CCmd[i].nID, CCmd[i].fVal);
                    }
                    SetPosition(m_lstCmdIDs.ToArray());
                }
            }

            #region Stream (모션 스텝마다 Goal 을 연속 전송하는 실시간 스트리밍)
            // 문제(2026-09-22 실측): Move_NoWait(40) 로 스트리밍하면 Profile Velocity = |목표-직전명령|/40ms 인데 실제 틱은
            //   15~31ms 라 상한이 목표 진행속도의 37~81% → 실물이 밀리다가, 끝의 감속 구간에서 raw 가 0(=무제한)이 되며
            //   밀린 양을 최대속도로 한 번에 따라잡는다(구간 끝 급가속). 실물 위치 피드백 없이 직전 명령값만 보는 탓.
            // 해법: (1) 다음 틱까지의 시간(horizon)은 실측 틱 간격(EMA)으로, (2) 실물 위치는 "직전 명령을 향해 직전 상한속도로
            //   전진했다"고 추정해 그 추정치→새 목표 거리로 상한을 정한다(틱이 이르면 남은 거리가 자동 가산 = 자기보정),
            //   (3) raw 는 최소 1 (0 = 무제한 금지), 최대 27.5 rpm(165 deg/s, X 시리즈 Velocity Limit 기본값 최소 128 raw 이하).
            // 실물 읽기는 넣지 않는다(FTDI 왕복 지연으로 틱이 느려짐).
            private readonly float[] m_afStream_Est = new float[256];      // 실물 추정 위치(도)
            private readonly float[] m_afStream_DegPerSec = new float[256]; // 직전 틱에 써 준 상한(도/s, raw 양자화 후)
            private long m_lStream_LastTs = 0;                              // 직전 전송 시각(Stopwatch tick), 0 = 없음
            private float m_fStream_Interval_ms = 0;                        // 틱 간격 EMA(ms), 0 = 미측정
            public float Stream_Early = 1.3f;        // horizon 배율: 1.3 = 다음 틱보다 늦게 도착하도록 — 서보가 멈추기 전에 다음 목표가 온다
            //   (2026-09-28 실측, OMX XL430 z=90 주행: 0.9 는 서보가 틱마다 목표에 먼저 닿아 즉시 정지 → 다음 틱에 재출발(직사각형 속도
            //    프로파일, Profile Acceleration 0) — 초당 ~25회 '부들거림'. 1.3 에서 PWM 요동 절반 이하(5.6/5.9 → 3.4/1.7%),
            //    주행 중 끝 오차는 그대로(2.44 → 2.31mm). 1.5 도 비슷. Profile Acceleration 100·200 은 이득 없음)
            public float Stream_MinHorizon_ms = 15;  // horizon 클램프
            public float Stream_MaxHorizon_ms = 60;
            public float Stream_Gap_ms = 150;        // 이보다 오래 쉬면 새 모션(추정 위치 = 직전 명령값)
            public float Stream_MaxRpm = 27.5f;      // 상한속도 캡(rpm) = 165 deg/s
            public float Stream_LastHorizon_ms { get { return m_fStream_LastHorizon; } }
            private float m_fStream_LastHorizon = 0;

            /// <summary>스트리밍 상태 리셋 — 토크 ON·모션 시작 등 실물이 직전 명령값에 있다고 볼 수 있을 때</summary>
            public void Stream_Reset() { m_lStream_LastTs = 0; }

            /// <summary>스트리밍 한 틱: 목표(도)들을 "다음 틱에 도착"하는 상한속도와 함께 전송한다.
            /// nHintMs = 틱 간격을 아직 모를 때(첫 틱) 쓸 horizon. 명령이 비어 있으면 Command_Set 으로 쌓인 것을 쓴다.</summary>
            public void Move_Stream(int nHintMs, params CCommand_t[] aCCommands)
            {
                if (IsOpen() == false) return;
                if (m_bEms == true) return;
                CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                Command_Clear();
                if (CCmd == null || CCmd.Length == 0) return;

                long lNow = System.Diagnostics.Stopwatch.GetTimestamp();
                float fDt = (m_lStream_LastTs == 0) ? -1f : (float)((lNow - m_lStream_LastTs) * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
                bool bFresh = (fDt < 0) || (fDt > Stream_Gap_ms);
                if (!bFresh) m_fStream_Interval_ms = (m_fStream_Interval_ms <= 0) ? fDt : (0.7f * m_fStream_Interval_ms + 0.3f * fDt);
                float fHorizon = (m_fStream_Interval_ms > 0) ? m_fStream_Interval_ms : (float)nHintMs;
                fHorizon *= Stream_Early;
                if (fHorizon < Stream_MinHorizon_ms) fHorizon = Stream_MinHorizon_ms;
                if (fHorizon > Stream_MaxHorizon_ms) fHorizon = Stream_MaxHorizon_ms;
                m_fStream_LastHorizon = fHorizon;

                for (int i = 0; i < CCmd.Length; i++)
                {
                    int nID = CCmd[i].nID;
                    if (bFresh) m_afStream_Est[nID] = m_afMot_Pose[nID];
                    else
                    {
                        // 직전 명령을 향해 직전 상한속도로 fDt 만큼 전진 (도달했으면 그 자리)
                        float fRem = m_afMot_Pose[nID] - m_afStream_Est[nID];
                        float fMax = m_afStream_DegPerSec[nID] * fDt * 0.001f;
                        if (fRem > fMax) fRem = fMax; else if (fRem < -fMax) fRem = -fMax;
                        m_afStream_Est[nID] += fRem;
                    }
                    float fDegPerSec = Math.Abs(CCmd[i].fVal - m_afStream_Est[nID]) / (fHorizon * 0.001f);
                    int nRaw = CalcRpm2Raw(nID, fDegPerSec / 6.0f);
                    int nCap = Math.Max(1, (int)(Stream_MaxRpm / m_aCParam[nID].m_fJointRpm));
                    if (nRaw < 1) nRaw = 1;              // 0 = 무제한 → 금지
                    if (nRaw > nCap) nRaw = nCap;
                    m_afStream_DegPerSec[nID] = nRaw * m_aCParam[nID].m_fJointRpm * 6.0f;
                    m_lstCmdIDs.Add(new CCommand_t(nID, nRaw));   // Command_Set 은 각도 리밋을 속도 raw 에도 적용해 버리므로 직접 추가
                }
                SetPosition_Speed(m_lstCmdIDs.ToArray());   // Profile Velocity(112) / P1 Moving Speed
                Command_Clear();
                for (int i = 0; i < CCmd.Length; i++) Command_Set(CCmd[i].nID, CCmd[i].fVal);
                SetPosition(m_lstCmdIDs.ToArray());         // Goal Position(116) — m_afMot_Pose 갱신
                m_lStream_LastTs = lNow;
            }
            #endregion Stream

            public void Wheel(float fRpm, params float[] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i * 2], afVals[i * 2 + 1]); }
                Wheel(fRpm, aCCommands);
            }
            public void Wheel(float fRpm, params CCommand_t[] aCCommands)
            {
                if (IsOpen() == false) return;
                if (m_bEms == true) return;

                CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                Command_Clear();
                if (CCmd.Length > 0)
                {
                    List<int> lstIDs = new List<int>();
                    lstIDs.Clear();
                    for (int i = 0; i < CCmd.Length; i++) { lstIDs.Add(CCmd[i].nID); }

                    for (int i = 0; i < CCmd.Length; i++) { Command_Set(CCmd[i].nID, 0); CalcRpm2Raw(CCmd[i].nID, fRpm); }
                    SetSpeed(m_lstCmdIDs.ToArray());
                }
            }
            #endregion Move / Play

            #region Read Command
            private int m_nRequest_Address = 0;
            private int m_nRequest_Address_Size = 0;
            private bool m_bRequest_AllOk = false; // Request_Flush 전체 성공 여부
            public int ReadDWord(int nID, int nAddress)
            {
                if (SyncRead_With_Address(nAddress, 4, nID))
                    return GetMap_Int(nID, nAddress);
                return 0;
            }
            public short ReadWord(int nID, int nAddress)
            {
                if (SyncRead_With_Address(nAddress, 2, nID))
                    return GetMap_Short(nID, nAddress);
                return 0;
            }
            public byte ReadByte(int nID, int nAddress)
            {
                if (SyncRead_With_Address(nAddress, 1, nID))
                    return GetMap(nID, nAddress);
                return 0;
            }
            public bool SyncRead_With_Address(int nAddress, int nSize, params int[] anIDs)
            {
                // 혼재 ID 는 프로토콜별로 분리하여 순차 처리 (주소 의미가 프로토콜마다 다르므로
                // 같은 주소로 P1/P2 혼합 요청은 P1 그룹/P2 그룹으로 나눠 각각 수행)
                List<int> lstP1 = new List<int>();
                List<int> lstP2 = new List<int>();
                for (int i = 0; i < anIDs.Length; i++)
                {
                    if (m_aCParam[anIDs[i]].m_nProtocol == 1) lstP1.Add(anIDs[i]);
                    else lstP2.Add(anIDs[i]);
                }
                bool bRet = true;
                if (lstP2.Count > 0)
                {
                    Request_Clear();
                    for (int i = 0; i < lstP2.Count; i++) Request_Push(lstP2[i]);
                    Request_Flush(nAddress, nSize);
                    if (m_bRequest_AllOk == false) bRet = false;
                }
                if (lstP1.Count > 0)
                {
                    Request_Clear();
                    for (int i = 0; i < lstP1.Count; i++) Request_Push(lstP1[i]);
                    Request_Flush(nAddress, nSize);
                    if (m_bRequest_AllOk == false) bRet = false;
                }
                return bRet;
            }
            private List<int> lstRequestIDs = new List<int>();
            public bool SyncRead(params int[] anIDs)
            {
                // 주소(=프로토콜/기종) 가 다른 ID 는 lstSecond 로 분리 → 그룹별 요청
                bool bRet = false;
                List<int> lstSecond = new List<int>();
                while (true)
                {
                    Request_Clear();
                    int[] anIDsCurr = ((lstSecond.Count > 0) ? lstSecond.ToArray() : anIDs);
                    lstSecond.Clear();
                    if (anIDsCurr.Length > 0)
                    {
                        for (int i = 0; i < anIDsCurr.Length; i++)
                        {
                            if (m_aCParam[anIDsCurr[0]].m_nGet_Position_Address != m_aCParam[anIDsCurr[i]].m_nGet_Position_Address)
                            {
                                lstSecond.Add(anIDsCurr[i]);
                            }
                            else Request_Push(anIDsCurr[i]);
                        }
                        Request_Flush(m_aCParam[anIDsCurr[0]].m_nGet_Position_Address, m_aCParam[anIDsCurr[0]].m_nGet_Position_Size);
                        // Flush 내부에서 응답 대기까지 완료 (P2 = SyncRead 0x82 일괄, P1 = ID별 순차)
                        bRet = m_bRequest_AllOk;
                    }
                    else break;
                    if (lstSecond.Count == 0) break;
                }
                Command_Clear();
                return bRet;
            }
            private List<int> m_lstRequestMotors = new List<int>();
            public void Request_Push(int nMotor) { m_lstRequestMotors.Add(nMotor); }
            public void Request_Clear() { m_lstRequestMotors.Clear(); }
            int m_nRequestMotors = 0;
            public void Request_Flush(int nAddress, int nSize)
            {
                // ★ CProtocol2 와 달리 응답 대기까지 이 함수에서 수행됨 (m_bRequest_AllOk 에 결과).
                //   그룹의 프로토콜은 첫 모터 기준 (SyncRead 의 주소 그룹핑이 프로토콜 통일 보장).
                if (m_lstRequestMotors.Count == 0) { m_bRequest_AllOk = false; return; }
                int nProtocol = m_aCParam[m_lstRequestMotors[0]].m_nProtocol;

                m_nRequest_Address = nAddress;
                m_nRequest_Address_Size = nSize;
                m_bRequest_AllOk = true;

                int[] anIDs = m_lstRequestMotors.ToArray();
                Request_Clear();

                if (nProtocol == 2)
                {
                    // P2: Sync Read(0x82) 일괄 요청 → 모든 ID 응답 대기
                    m_nParse_Protocol = 2;
                    byte[] pbyDatas = new byte[4 + anIDs.Length];
                    int nPos = 0;
                    pbyDatas[nPos++] = (byte)(nAddress & 0xff);
                    pbyDatas[nPos++] = (byte)(((nAddress >> 8) & 0xff));
                    pbyDatas[nPos++] = (byte)(nSize & 0xff);
                    pbyDatas[nPos++] = (byte)(((nSize >> 8) & 0xff));
                    for (int i = 0; i < anIDs.Length; i++)
                    {
                        pbyDatas[nPos++] = (byte)(anIDs[i] & 0xff);
                    }
                    m_nRequestMotors = anIDs.Length;
                    SendRaw_P2(254, 0x82, pbyDatas);
                    if (WaitReceive(anIDs) == false) m_bRequest_AllOk = false;
                }
                else
                {
                    // P1: Sync Read 없음 → ID 별 READ(0x02) 전송 + 응답 대기 순차 반복
                    m_nParse_Protocol = 1;
                    for (int i = 0; i < anIDs.Length; i++)
                    {
                        m_nRequestMotors = 1;
                        Send_P1(anIDs[i], 0x02, nAddress, (byte)(nSize & 0xff));
                        if (WaitReceive(new int[] { anIDs[i] }) == false) m_bRequest_AllOk = false;
                    }
                }
            }
            #endregion Read Command

            #region Read (WaitReceive / 수신 파서 P1+P2)
            public void Set_WaitTime(int nMillisec) { m_nWaitTime_Limit = nMillisec; }
            public int Get_WaitTime() { return m_nWaitTime_Limit; }
            const int _WAIT_TIME = 50; // ms
            int m_nWaitTime_Limit = _WAIT_TIME;
            const int _WAIT_TIME_SOCK = 5000; // ms
            private int m_nShowReturnPacket = 0;
            public void ShowPacketReturn(int nPacket_0_Disable_1_Enable) { m_nShowReturnPacket = nPacket_0_Disable_1_Enable; }

            public bool WaitReceive(int[] anIDs = null)
            {
                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();
                if (anIDs != null)
                {
                    lstRequestIDs.Clear();
                    lstRequestIDs.AddRange(anIDs);
                }
                while (true)
                {
                    if (m_CSock_Client.IsConnect())
                    {
                        if (m_CSock_Client.GetBuffer_Length() > 0)
                        {
                            ReceivedPacket(m_CSock_Client.GetBytes());
                            if (m_CSock_Client.GetBuffer_Length() == 0)
                            {
                                if (lstRequestIDs.Count == 0) return true;
                            }
                        }
                    }
                    else if (m_CSerial.IsConnect())
                    {
                        if (m_CSerial.GetBuffer_Length() > 0)
                        {
                            ReceivedPacket(m_CSerial.GetBytes());
                            if (m_CSerial.GetBuffer_Length() == 0)
                            {
                                if (lstRequestIDs.Count == 0) return true;
                            }
                        }
                    }
                    if (CTmr.Get() >= Get_WaitTime())
                    {
                        m_nTimeout_Count++;
                        Ojw.LogErr("대기시간 초과");
                        Receive_ResetParser();
                        break;
                    }
                    Ojw.CTimer.DoEvent();
                }
                return false;
            }
            public bool WaitReceive_Ext_Sock(Ojw.CSocket CSock, int nWaitTime = 0)
            {
                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();
                if (nWaitTime <= 0) nWaitTime = _WAIT_TIME_SOCK;
                while (true)
                {
                    if (CSock.IsConnect())
                    {
                        if (CSock.GetBuffer_Length() > 0)
                        {
                            ReceivedPacket(CSock.GetBytes());
                            return true;
                        }
                    }
                    if (CTmr.Get() >= nWaitTime)
                    {
                        Ojw.LogErr("대기시간 초과");
                        break;
                    }
                    Ojw.CTimer.DoEvent();
                }
                return false;
            }
            public bool WaitReceive_Ext_Serial(Ojw.CSerial CSerial, int nWaitTime = 0)
            {
                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();
                if (nWaitTime <= 0) nWaitTime = Get_WaitTime();
                while (true)
                {
                    if (CSerial.IsConnect())
                    {
                        if (CSerial.GetBuffer_Length() > 0)
                        {
                            ReceivedPacket(CSerial.GetBytes());
                            return true;
                        }
                    }
                    if (CTmr.Get() >= nWaitTime)
                    {
                        Ojw.LogErr("대기시간 초과");
                        break;
                    }
                    Ojw.CTimer.DoEvent();
                }
                return false;
            }

            public class CReceive_t
            {
                public int nID = 0;
                public int nCmd = 0;
                public int nLength_Data = 0;
                public int nError = 0;
                public List<int> lstDatas = new List<int>();
            }
            // 공용 상태
            int m_nParse_Protocol = 2; // 현재 기대하는 응답 프로토콜 (요청 시점에 설정)
            int m_nReceive_ID = 0;
            int m_nReceive_Length = 0;
            int m_nReceive_Cmd = 0;
            int m_nReceive_Length_Check = 0;
            int m_nReceive_Error = 0;
            public int m_nSeq = 0;
            public int m_nSeq_Raw = 0;
            public int m_nCRC_Error_Count = 0;   // CRC(P2)/체크섬(P1) 오류 카운트
            public int m_nTimeout_Count = 0;
            // P2 파서 상태
            int m_nReceive_Header = 0;
            int m_nReceive_Index = 0;
            int m_nReceive_CRC_Accum = 0;
            int m_nReceive_CRC_Low = 0;
            int m_nReceive_Stuffing = 0;
            // P1 파서 상태
            int m_nP1_Header = 0;
            int m_nP1_Index = 0;
            int m_nP1_Checksum = 0;
            private void Receive_ResetParser()
            {
                m_nReceive_Header = 0;
                m_nReceive_Index = 0;
                m_nP1_Header = 0;
                m_nP1_Index = 0;
            }
            public List<CReceive_t> m_aCReceive = new List<CReceive_t>();
            public List<int> m_anReceive_Datas = new List<int>();
            public int[] GetBuffers_Int() { return m_anReceive_Datas.ToArray(); }
            public byte[] GetBuffers() { return Array.ConvertAll(m_anReceive_Datas.ToArray(), element => (byte)element); }

            public void ReceivedPacket(byte[] buffer)
            {
                // 응답은 요청 직후에만 수신 (request-scoped) → 요청한 프로토콜의 파서로 라우팅
                if (m_nParse_Protocol == 1) ReceivedPacket_P1(buffer);
                else ReceivedPacket_P2(buffer);
                m_nSeq++;
            }
            // 패킷 완성 공통 처리 (양 파서 공용): 검증 통과 시 map/모터배열 반영
            private void Receive_Complete(bool bCheckOk)
            {
                CReceive_t CReceive = new CReceive_t();
                CReceive.nID = m_nReceive_ID;
                CReceive.nCmd = m_nReceive_Cmd;
                CReceive.nLength_Data = m_anReceive_Datas.Count;

                if (m_nRequestMotors > 0)
                {
                    if ((m_nReceive_ID >= 0) && (m_nReceive_ID < 253))
                    {
                        // ★ 검증 실패 시 lstRequestIDs 에서 제거하지 않음 → WaitReceive 타임아웃 → 상위 재시도
                        if (bCheckOk && lstRequestIDs.Count > 0)
                        {
                            int nRequest = lstRequestIDs.IndexOf(m_nReceive_ID);
                            if (nRequest >= 0)
                            {
                                lstRequestIDs.RemoveAt(nRequest);
                            }
                        }

                        // P2: 에러코드 0 요구 / P1: 에러는 알람 비트필드라 데이터는 유효 → 체크섬만 요구
                        bool bDataOk = bCheckOk &&
                            ((m_nParse_Protocol == 1) ? true : ((m_nReceive_Error & 0x7F) == 0));
                        if (bDataOk)
                        {
                            for (int nBuffer = 0; nBuffer < m_anReceive_Datas.Count; nBuffer++)
                            {
                                int nMapIdx = m_nReceive_ID * _SIZE_MAP + m_nRequest_Address + nBuffer;
                                if (nMapIdx >= 0 && nMapIdx < m_abyMap.Length)
                                    m_abyMap[nMapIdx] = (byte)(m_anReceive_Datas[nBuffer] & 0xff);
                            }

                            byte[] pbyData = new byte[m_anReceive_Datas.Count];
                            for (int nBuffer = 0; nBuffer < m_anReceive_Datas.Count; nBuffer++) { pbyData[nBuffer] = (byte)(m_anReceive_Datas[nBuffer] & 0xff); }

                            int nVal = 0;
                            switch (CReceive.nLength_Data)
                            {
                                case 1: nVal = (byte)(pbyData[0]); break;
                                case 2: nVal = Ojw.CConvert.BytesToShort(pbyData, 0); break;
                                case 4: nVal = Ojw.CConvert.BytesToInt(pbyData, 0); break;
                            }

                            if (m_nRequest_Address == m_aCParam[m_nReceive_ID].m_nSet_Position_Address)
                            {
                                m_anMot[m_nReceive_ID] = nVal;
                                m_afMot[m_nReceive_ID] = CalcEvd2Angle(CReceive.nID, nVal);
                            }
                            else if (m_nRequest_Address == m_aCParam[m_nReceive_ID].m_nGet_Position_Address)
                            {
                                m_anMot_Seq[m_nReceive_ID]++;

                                m_anMot_Pose[m_nReceive_ID] = nVal;
                                m_afMot_Pose[m_nReceive_ID] = CalcEvd2Angle(CReceive.nID, nVal);

                                m_anMot[m_nReceive_ID] = m_anMot_Pose[m_nReceive_ID];
                                m_afMot[m_nReceive_ID] = m_afMot_Pose[m_nReceive_ID];
                            }
                            else if (m_nRequest_Address == m_aCParam[m_nReceive_ID].m_nGet_Current_Address)
                            {
                                m_anMot_Seq[m_nReceive_ID]++;
                                m_anMot_Curr[m_nReceive_ID] = nVal;
                            }
                            else if (m_nRequest_Address == m_aCParam[m_nReceive_ID].m_nSet_Torq_Address)
                            {
                                m_abMot[m_nReceive_ID] = ((nVal == 0) ? false : true);
                            }
                        }
                    }

                    m_nRequestMotors--;
                }

                CReceive.nError = m_nReceive_Error;
                CReceive.lstDatas.AddRange(m_anReceive_Datas.ToArray());

                if (m_aCReceive.Count >= 10) m_aCReceive.RemoveAt(0);
                m_aCReceive.Add(CReceive);

                m_nSeq_Raw++;
            }

            // ── P1 파서: FF FF ID LEN ERR [DATA] CHKSUM (COjw_40 CProtocol1 과 동일) ──
            private void ReceivedPacket_P1(byte[] buffer)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    byte byData = buffer[i];

                    if (m_nP1_Index == 0)
                    {
                        if (byData == 0xff)
                        {
                            m_nP1_Header++;
                            if (m_nP1_Header > 2) m_nP1_Header = 2; // 연속 FF → 최근 2개 유지
                        }
                        else if (m_nP1_Header >= 2)
                        {
                            m_nReceive_ID = byData;
                            m_nReceive_Length = 0;
                            m_nReceive_Cmd = 0;
                            m_nReceive_Error = 0;
                            m_nReceive_Length_Check = 0;
                            m_anReceive_Datas.Clear();
                            m_nP1_Checksum = byData;
                            m_nP1_Index = 1;
                            m_nP1_Header = 0;
                        }
                        else
                        {
                            m_nP1_Header = 0;
                        }
                    }
                    else
                    {
                        switch (m_nP1_Index)
                        {
                            case 1: // LENGTH
                                m_nReceive_Length = byData;
                                m_nP1_Checksum += byData;
                                if (m_nReceive_Length < 2)
                                {
                                    Ojw.LogErr("[ID:" + m_nReceive_ID + "] 비정상 패킷 Length=" + m_nReceive_Length);
                                    m_nP1_Header = 0;
                                    m_nP1_Index = 0;
                                    break;
                                }
                                m_nP1_Index++;
                                break;
                            case 2: // ERROR (비트필드)
                                m_nReceive_Error = byData;
                                m_nP1_Checksum += byData;
                                if (m_nReceive_Error != 0)
                                {
                                    Ojw.Log("[ID:" + m_nReceive_ID + "]Received: ++++++Error 발생[Code:" + GetError(m_nReceive_Error, 1) + "]+++++++");
                                }
                                m_nReceive_Length_Check = 0;
                                if (m_nReceive_Length <= 2) m_nP1_Index = 4;
                                else m_nP1_Index++;
                                break;
                            case 3: // DATA
                                m_anReceive_Datas.Add(byData);
                                m_nP1_Checksum += byData;
                                m_nReceive_Length_Check++;
                                if (m_nReceive_Length_Check >= m_nReceive_Length - 2) m_nP1_Index++;
                                break;
                            case 4: // CHECKSUM
                                {
                                    bool bChkOk = (((byte)(~m_nP1_Checksum & 0xff)) == byData);
                                    if (!bChkOk)
                                    {
                                        m_nCRC_Error_Count++;
                                        Ojw.LogErr("[ID:" + m_nReceive_ID + "] Checksum 불일치 (수신:0x" + byData.ToString("X2") + " 계산:0x" + ((byte)(~m_nP1_Checksum & 0xff)).ToString("X2") + ")");
                                    }
                                    Receive_Complete(bChkOk);
                                    m_nP1_Header = ((byData == 0xff) ? 1 : 0);
                                    m_nP1_Index = 0;
                                }
                                break;
                        }
                    }
                }
            }

            // ── P2 파서: FF FF FD 00 ... CRC16 (COjw_37 CProtocol2 와 동일 — 디스터핑/CRC 포함) ──
            private void ReceivedPacket_P2(byte[] buffer)
            {
                byte[] value = buffer;
                int nPaketLength = value.Length;
                byte val;
                for (int i = 0; i < nPaketLength; i++)
                {
                    val = value[i];
                    byte byData = val;

                    // 헤더 감지: 항상 동작 (P2 byte stuffing 으로 데이터에 FF FF FD 불가)
                    int nTmp = m_nReceive_Header % 100;
                    if (byData == 0xff)
                    {
                        m_nReceive_Header++;
                        if ((nTmp >= 2) && (nTmp < 10))
                        {
                            if (m_nReceive_Header >= 100) m_nReceive_Header = 102;
                            else m_nReceive_Header = 2;
                        }
                    }
                    else if (nTmp == 2)
                    {
                        if (byData == 0xfd)
                        {
                            if (m_nReceive_Header >= 100) m_nReceive_Header = 110;
                            else m_nReceive_Header = 10;
                        }
                    }
                    else if (nTmp == 10)
                    {
                        if (byData == 0x00)
                        {
                            m_nReceive_Header = 100;
                            m_nReceive_Index = 1;
                            m_nReceive_ID = 0;
                            m_nReceive_Length = 0;
                            m_nReceive_Cmd = 0;
                            m_nReceive_Length_Check = 0;
                            m_nReceive_Error = 0;
                            m_anReceive_Datas.Clear();
                            m_nReceive_Stuffing = 0;
                            m_nReceive_CRC_Accum = 0;
                            m_nReceive_CRC_Accum = updateCRC_Byte(m_nReceive_CRC_Accum, 0xFF);
                            m_nReceive_CRC_Accum = updateCRC_Byte(m_nReceive_CRC_Accum, 0xFF);
                            m_nReceive_CRC_Accum = updateCRC_Byte(m_nReceive_CRC_Accum, 0xFD);
                            m_nReceive_CRC_Accum = updateCRC_Byte(m_nReceive_CRC_Accum, 0x00);
                        }
                    }
                    else
                    {
                        if (m_nReceive_Header >= 100) m_nReceive_Header = 100;
                        else m_nReceive_Header = 0;
                    }

                    if (m_nReceive_Header >= 100)
                    {
                        switch (m_nReceive_Index)
                        {
                            case 1:
                                m_nReceive_Index++;
                                break;
                            case 2:
                                m_nReceive_ID = byData;
                                m_nReceive_CRC_Accum = updateCRC_Byte(m_nReceive_CRC_Accum, byData);
                                m_nReceive_Index++;
                                break;
                            case 3:
                                m_nReceive_Length = byData;
                                m_nReceive_CRC_Accum = updateCRC_Byte(m_nReceive_CRC_Accum, byData);
                                m_nReceive_Index++;
                                break;
                            case 4:
                                m_nReceive_Length += byData * 256;
                                m_nReceive_CRC_Accum = updateCRC_Byte(m_nReceive_CRC_Accum, byData);
                                m_nReceive_Index++;
                                m_nReceive_Length_Check = 0;
                                break;
                            case 5:
                                m_nReceive_Length_Check++;
                                m_nReceive_Cmd = byData;
                                m_nReceive_CRC_Accum = updateCRC_Byte(m_nReceive_CRC_Accum, byData);
                                m_nReceive_Index++;
                                break;
                            case 6:
                                m_nReceive_Length_Check++;
                                m_nReceive_CRC_Accum = updateCRC_Byte(m_nReceive_CRC_Accum, byData);
                                if (m_nReceive_Length <= 3)
                                {
                                    Ojw.LogErr("[ID:" + m_nReceive_ID + "] 비정상 패킷 Length=" + m_nReceive_Length);
                                    m_nReceive_Header = 0;
                                    m_nReceive_Index = 0;
                                    break;
                                }
                                m_nReceive_Error = byData;
                                if (m_nReceive_Error != 0)
                                {
                                    Ojw.Log("[ID:" + m_nReceive_ID + "]Received: ++++++Error 발생[Code:" + GetError(m_nReceive_Error, 2) + "]+++++++");
                                }
                                m_nReceive_Index++;
                                break;
                            case 7:
                                m_nReceive_Length_Check++;

                                if (m_nReceive_Length_Check <= m_nReceive_Length - 2)
                                {
                                    m_nReceive_CRC_Accum = updateCRC_Byte(m_nReceive_CRC_Accum, byData);

                                    // 바이트 디스터핑 (FF FF FD FD → FF FF FD)
                                    bool bStuffingByte = false;
                                    if (m_nReceive_Stuffing == 3)
                                    {
                                        if (byData == 0xFD)
                                        {
                                            bStuffingByte = true;
                                        }
                                        m_nReceive_Stuffing = 0;
                                    }
                                    else if (byData == 0xFF)
                                    {
                                        if (m_nReceive_Stuffing < 2) m_nReceive_Stuffing++;
                                    }
                                    else if (m_nReceive_Stuffing == 2 && byData == 0xFD)
                                    {
                                        m_nReceive_Stuffing = 3;
                                    }
                                    else
                                    {
                                        m_nReceive_Stuffing = 0;
                                    }

                                    if (!bStuffingByte)
                                    {
                                        m_anReceive_Datas.Add(byData);
                                    }
                                }
                                else
                                {
                                    m_nReceive_CRC_Low = byData;
                                    m_nReceive_Index++;
                                }
                                break;
                            case 8:
                                m_nReceive_Length_Check++;
                                if (m_nReceive_Length_Check >= m_nReceive_Length)
                                {
                                    int nCRC_Received = (byData << 8) | m_nReceive_CRC_Low;
                                    int nCRC_Calculated = m_nReceive_CRC_Accum & 0xFFFF;
                                    bool bCrcOk = (nCRC_Received == nCRC_Calculated);
                                    if (!bCrcOk)
                                    {
                                        m_nCRC_Error_Count++;
                                        Ojw.LogErr("[ID:" + m_nReceive_ID + "] CRC 불일치 (수신:0x" + nCRC_Received.ToString("X4") + " 계산:0x" + nCRC_Calculated.ToString("X4") + ")");
                                    }

                                    Receive_Complete(bCrcOk);

                                    if (byData == 0xff) m_nReceive_Header = 1;
                                    else m_nReceive_Header = 0;
                                    m_nReceive_Index = 0;
                                }
                                break;
                        }
                    }
                } // for
            }

            public string GetError(int nErrorNumber) { return GetError(nErrorNumber, m_nParse_Protocol); }
            public string GetError(int nErrorNumber, int nProtocol)
            {
                string strRes = "ErrNum[" + nErrorNumber + "]";
                if (nProtocol == 1)
                {
                    // Protocol 1.0: 비트필드
                    if (nErrorNumber == 0) return strRes;
                    if ((nErrorNumber & 0x01) != 0) strRes += "[Input Voltage Error] 인가 전압이 동작 전압 범위를 벗어남\r\n";
                    if ((nErrorNumber & 0x02) != 0) strRes += "[Angle Limit Error]  Goal Position 이 CW/CCW Angle Limit 범위를 벗어남\r\n";
                    if ((nErrorNumber & 0x04) != 0) strRes += "[Overheating Error]  내부 온도가 설정된 최고온도를 초과\r\n";
                    if ((nErrorNumber & 0x08) != 0) strRes += "[Range Error]        사용 범위를 벗어난 명령\r\n";
                    if ((nErrorNumber & 0x10) != 0) strRes += "[Checksum Error]     전송된 Packet 의 체크섬 불일치\r\n";
                    if ((nErrorNumber & 0x20) != 0) strRes += "[Overload Error]     설정된 최대토크로 제어할 수 없는 부하 발생\r\n";
                    if ((nErrorNumber & 0x40) != 0) strRes += "[Instruction Error]  정의되지 않은 Instruction\r\n";
                    return strRes;
                }
                // Protocol 2.0: 에러코드
                switch (nErrorNumber)
                {
                    case 0x00: break;
                    case 0x01: strRes += "[Result Fail] 전송된 Instruction Packet 을 처리하는데 실패한 경우";
                        break;
                    case 0x02: strRes += "[Instruction Error]  정의되지 않은 Instruction 을 사용한 경우\r\n";
                        strRes += "Reg Write 없이 Action 을 사용한 경우";
                        break;
                    case 0x03: strRes += "[CRC Error]          전송된 Packet 의 CRC 값이 맞지 않는 경우";
                        break;
                    case 0x04: strRes += "[Data Range Error]   해당 Address 에 쓰려는 Data 가 최소/최대값의 범위를 벗어난 경우";
                        break;
                    case 0x05: strRes += "[Data Length Error]  해당 Address 의 데이터 길이보다 짧은 데이터를 적으려고 한 경우";
                        break;
                    case 0x06: strRes += "[Data Limit Error]   해당 Address 에 쓰려는 Data 가 Limit 값을 벗어난 경우";
                        break;
                    case 0x07: strRes += "[Access Error]       Read Only / Write Only / ROM Lock 접근 위반";
                        break;
                }
                return strRes;
            }
            #endregion Read

            #region Protocol - basic (P1/P2 프레이밍, CRC, SendPacket)
            // 디버그/테스트 tap — 전송 직전 패킷을 관찰 (null 이면 무시). 하네스/버스 모니터용.
            public Action<byte[]> OnSendPacket_Debug = null;

            // ── 공용 Send: 모터의 선언 프로토콜로 프레이밍. 254(브로드캐스트) = 양쪽 모두 송출 ──
            public void Send(int nMotorRealID, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                if (nMotorRealID == 254)
                {
                    // CMonster2 방식: 브로드캐스트는 두 프로토콜 프레임을 연속 송출
                    Send_P2(254, nCommand, nAddress, pbyDatas);
                    Send_P1(254, nCommand, nAddress, pbyDatas);
                    return;
                }
                if (m_aCParam[nMotorRealID].m_nProtocol == 1) Send_P1(nMotorRealID, nCommand, nAddress, pbyDatas);
                else Send_P2(nMotorRealID, nCommand, nAddress, pbyDatas);
            }
            public void SendRaw(int nMotorRealID, int nCommand, params byte[] pbyDatas)
            {
                if (nMotorRealID == 254)
                {
                    SendRaw_P2(254, nCommand, pbyDatas);
                    SendRaw_P1(254, nCommand, pbyDatas);
                    return;
                }
                if (m_aCParam[nMotorRealID].m_nProtocol == 1) SendRaw_P1(nMotorRealID, nCommand, pbyDatas);
                else SendRaw_P2(nMotorRealID, nCommand, pbyDatas);
            }

            // ── Protocol 1.0 프레이밍: FF FF ID LEN INST [ADDR(1B)] [DATA] ~SUM ──
            public void Send_P1(int nMotorRealID, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = 2 + ((pbyDatas != null) ? pbyDatas.Length + 1 : 0);
                byte[] pbyteBuffer = new byte[4 + nLength];
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                pbyteBuffer[i++] = (byte)(nLength & 0xff);
                pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                if (pbyDatas != null)
                {
                    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                    foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;
                }
                int nCrc = 0;
                for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(~nCrc & 0xff);

                SendPacket(pbyteBuffer, pbyteBuffer.Length);
            }
            public void SendRaw_P1(int nMotorRealID, int nCommand, params byte[] pbyDatas)
            {
                int i = 0;
                int nDataLength = ((pbyDatas != null) ? pbyDatas.Length : 0);
                int nLength = 2 + nDataLength;
                byte[] pbyteBuffer = new byte[4 + nLength];
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                pbyteBuffer[i++] = (byte)(nLength & 0xff);
                pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                for (int j = 0; j < nDataLength; j++) pbyteBuffer[i++] = pbyDatas[j];
                int nCrc = 0;
                for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                SendPacket(pbyteBuffer, pbyteBuffer.Length);
            }

            // ── Protocol 2.0 프레이밍: FF FF FD 00 ID LEN_L LEN_H INST [ADDR(2B)] [DATA] CRC16 ──
            public void Send_P2(int nMotorRealID, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = 3 + ((pbyDatas != null) ? pbyDatas.Length + 2 : 0);
                int nDefaultSize = 7;
                byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xfd;
                pbyteBuffer[i++] = 0x00;
                pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                pbyteBuffer[i++] = (byte)(nLength & 0xff);
                pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                if (pbyDatas != null)
                {
                    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                    foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;
                }
                MakeStuff(ref pbyteBuffer);
                int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                SendPacket(pbyteBuffer, pbyteBuffer.Length);
            }
            public void SendRaw_P2(int nMotorRealID, int nCommand, params byte[] pbyDatas)
            {
                int i = 0;
                int nDataLength = ((pbyDatas != null) ? pbyDatas.Length : 0);
                int nLength = 3 + nDataLength;
                int nDefaultSize = 7;
                byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xfd;
                pbyteBuffer[i++] = 0x00;
                pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                pbyteBuffer[i++] = (byte)(nLength & 0xff);
                pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                for (int j = 0; j < nDataLength; j++) pbyteBuffer[i++] = pbyDatas[j];
                MakeStuff(ref pbyteBuffer);
                int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                SendPacket(pbyteBuffer, pbyteBuffer.Length);
            }

            private int[] m_anCrcTable = new int[256] {
                    0x0000, 0x8005, 0x800F, 0x000A, 0x801B, 0x001E, 0x0014, 0x8011, 0x8033, 0x0036, 0x003C, 0x8039, 0x0028, 0x802D, 0x8027, 0x0022, 0x8063, 0x0066, 0x006C, 0x8069, 0x0078, 0x807D, 0x8077, 0x0072, 0x0050, 0x8055, 0x805F, 0x005A, 0x804B, 0x004E, 0x0044, 0x8041, 0x80C3, 0x00C6, 0x00CC, 0x80C9, 0x00D8, 0x80DD, 0x80D7, 0x00D2,
                    0x00F0, 0x80F5, 0x80FF, 0x00FA, 0x80EB, 0x00EE, 0x00E4, 0x80E1, 0x00A0, 0x80A5, 0x80AF, 0x00AA, 0x80BB, 0x00BE, 0x00B4, 0x80B1, 0x8093, 0x0096, 0x009C, 0x8099, 0x0088, 0x808D, 0x8087, 0x0082, 0x8183, 0x0186, 0x018C, 0x8189, 0x0198, 0x819D, 0x8197, 0x0192, 0x01B0, 0x81B5, 0x81BF, 0x01BA, 0x81AB, 0x01AE, 0x01A4, 0x81A1,
                    0x01E0, 0x81E5, 0x81EF, 0x01EA, 0x81FB, 0x01FE, 0x01F4, 0x81F1, 0x81D3, 0x01D6, 0x01DC, 0x81D9, 0x01C8, 0x81CD, 0x81C7, 0x01C2, 0x0140, 0x8145, 0x814F, 0x014A, 0x815B, 0x015E, 0x0154, 0x8151, 0x8173, 0x0176, 0x017C, 0x8179, 0x0168, 0x816D, 0x8167, 0x0162, 0x8123, 0x0126, 0x012C, 0x8129, 0x0138, 0x813D, 0x8137, 0x0132,
                    0x0110, 0x8115, 0x811F, 0x011A, 0x810B, 0x010E, 0x0104, 0x8101, 0x8303, 0x0306, 0x030C, 0x8309, 0x0318, 0x831D, 0x8317, 0x0312, 0x0330, 0x8335, 0x833F, 0x033A, 0x832B, 0x032E, 0x0324, 0x8321, 0x0360, 0x8365, 0x836F, 0x036A, 0x837B, 0x037E, 0x0374, 0x8371, 0x8353, 0x0356, 0x035C, 0x8359, 0x0348, 0x834D, 0x8347, 0x0342,
                    0x03C0, 0x83C5, 0x83CF, 0x03CA, 0x83DB, 0x03DE, 0x03D4, 0x83D1, 0x83F3, 0x03F6, 0x03FC, 0x83F9, 0x03E8, 0x83ED, 0x83E7, 0x03E2, 0x83A3, 0x03A6, 0x03AC, 0x83A9, 0x03B8, 0x83BD, 0x83B7, 0x03B2, 0x0390, 0x8395, 0x839F, 0x039A, 0x838B, 0x038E, 0x0384, 0x8381, 0x0280, 0x8285, 0x828F, 0x028A, 0x829B, 0x029E, 0x0294, 0x8291,
                    0x82B3, 0x02B6, 0x02BC, 0x82B9, 0x02A8, 0x82AD, 0x82A7, 0x02A2, 0x82E3, 0x02E6, 0x02EC, 0x82E9, 0x02F8, 0x82FD, 0x82F7, 0x02F2, 0x02D0, 0x82D5, 0x82DF, 0x02DA, 0x82CB, 0x02CE, 0x02C4, 0x82C1, 0x8243, 0x0246, 0x024C, 0x8249, 0x0258, 0x825D, 0x8257, 0x0252, 0x0270, 0x8275, 0x827F, 0x027A, 0x826B, 0x026E, 0x0264, 0x8261,
                    0x0220, 0x8225, 0x822F, 0x022A, 0x823B, 0x023E, 0x0234, 0x8231, 0x8213, 0x0216, 0x021C, 0x8219, 0x0208, 0x820D, 0x8207, 0x0202 };

            private int updateCRC(byte[] data_blk_ptr, int data_blk_size)
            {
                int nCrc_accum = 0;
                for (int i = 0; i < data_blk_size; i++) nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ data_blk_ptr[i]) & 0xFF)];
                return nCrc_accum;
            }
            private int updateCRC_Byte(int nCrc_accum, byte byData)
            {
                return (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ byData) & 0xFF)];
            }
            private void MakeStuff(ref byte[] pBuff)
            {
                // Dynamixel Protocol 2.0 Byte Stuffing: FF FF FD → FF FF FD FD
                // (ROBOTIS SDK 와 같은 3-byte sliding window 방식 — COjw_37 과 동일)
                if ((pBuff == null) || (pBuff.Length < 8)) return;

                List<byte> lstStuffed = new List<byte>(pBuff.Length + 4);
                for (int i = 0; i < 8; i++) lstStuffed.Add(pBuff[i]);

                for (int i = 8; i < pBuff.Length - 2; i++)
                {
                    byte b = pBuff[i];
                    lstStuffed.Add(b);
                    if (i >= 10 && pBuff[i - 2] == 0xFF && pBuff[i - 1] == 0xFF && b == 0xFD)
                    {
                        lstStuffed.Add(0xFD);
                    }
                }

                lstStuffed.Add(0x00); lstStuffed.Add(0x00);   // CRC placeholder

                pBuff = lstStuffed.ToArray();
                int nLength = pBuff.Length - 7;
                pBuff[5] = (byte)(nLength & 0xff);
                pBuff[6] = (byte)((nLength >> 8) & 0xff);
            }
            public void SendPacket(byte[] buffer, int nLength)
            {
                if (OnSendPacket_Debug != null)
                {
                    byte[] abyCopy = new byte[nLength];
                    Array.Copy(buffer, abyCopy, nLength);
                    OnSendPacket_Debug(abyCopy);
                }
                if (m_CSerial.IsConnect() == true) m_CSerial.SendPacket(buffer, nLength);
                if (m_CSock_Client.IsConnect() == true) m_CSock_Client.SendPacket(buffer, nLength);
            }
            #endregion Protocol - basic

            #region Sync Write
            // 그룹 프로토콜은 첫 push 모터에서 결정 (주소 그룹핑이 프로토콜 통일 보장).
            // P1: FF FF FE LEN 83 ADDR(1B) LEN_PER(1B) [ID D..]... ~SUM
            // P2: FF FF FD 00 FE LEN 83 ADDR(2B) LEN_PER(2B) [ID D..]... CRC16
            private int m_nSync_Length = 0;
            private int m_nSync_Protocol = 2;
            private bool m_IsSync_Error = false;
            private List<byte> m_lstSync = new List<byte>();
            public void Sync_Clear()
            {
                m_lstSync.Clear();
                m_nSync_Length = 0;
                m_nSync_Protocol = 2;
                m_IsSync_Error = false;
            }
            public void Sync_Push_Byte(int nID, int nData)
            {
                byte[] abyDatas = new byte[1];
                abyDatas[0] = (byte)(nData & 0xff);
                Sync_Push(nID, abyDatas);
            }
            public void Sync_Push_Word(int nID, int nData)
            {
                byte[] abyDatas = new byte[2];
                abyDatas[0] = (byte)(nData & 0xff);
                abyDatas[1] = (byte)((nData >> 8) & 0xff);
                Sync_Push(nID, abyDatas);
            }
            public void Sync_Push_Dword(int nID, int nData)
            {
                byte[] abyDatas = new byte[4];
                abyDatas[0] = (byte)(nData & 0xff);
                abyDatas[1] = (byte)((nData >> 8) & 0xff);
                abyDatas[2] = (byte)((nData >> 16) & 0xff);
                abyDatas[3] = (byte)((nData >> 24) & 0xff);
                Sync_Push(nID, abyDatas);
            }
            public void Sync_Push_Angle(int nID, float fAngle)
            {
                fAngle = CalcLimit(nID, fAngle);
                int nData = CalcAngle2Evd(nID, fAngle);
                Sync_Push_BySize(nID, nData, m_aCParam[nID].m_nSet_Position_Size);
            }
            public void Sync_Push(int nID, byte[] pbyDatas)
            {
                int nDataLength = pbyDatas.Length;

                if (nDataLength > 0)
                {
                    if (m_nSync_Length == 0)
                    {
                        m_nSync_Length = nDataLength;
                        m_nSync_Protocol = m_aCParam[nID].m_nProtocol;
                        // per-motor 데이터길이 필드: P1 = 1바이트, P2 = 2바이트
                        m_lstSync.Add((byte)(nDataLength & 0xff));
                        if (m_nSync_Protocol != 1)
                            m_lstSync.Add((byte)((nDataLength >> 8) & 0xff));
                    }
                    else if (m_nSync_Length != nDataLength)
                    {
                        Ojw.LogErr("Error(Sync_Push) - ID:" + nID);
                        m_IsSync_Error = true;
                        return;
                    }
                    else if (m_nSync_Protocol != m_aCParam[nID].m_nProtocol)
                    {
                        // 한 sync 패킷에 두 프로토콜 혼입 불가 — Set* 의 주소 그룹핑이 지켜주지만 방어
                        Ojw.LogErr("Error(Sync_Push) - Protocol 혼입 ID:" + nID);
                        m_IsSync_Error = true;
                        return;
                    }

                    m_lstSync.Add((byte)(nID & 0xff));
                    for (var i = 0; i < nDataLength; i++)
                    {
                        m_lstSync.Add((byte)pbyDatas[i]);
                    }
                }
            }
            public void Sync_Flush(int nAddress)
            {
                if (m_IsSync_Error == false)
                {
                    if (m_lstSync.Count > 0)
                    {
                        if (m_nSync_Protocol == 1) Send_P1(254, 0x83, nAddress, m_lstSync.ToArray());
                        else Send_P2(254, 0x83, nAddress, m_lstSync.ToArray());
                    }
                }
                Sync_Clear();
            }
            #endregion Sync Write

            #region Delta
            public List<CDelta> m_lstCDelta = new List<CDelta>();
            public int Delta_Add(float fRot_Cw, int nID_Front, int nID_Left, int nID_Right, float fTop_Rad, float fTop_Length, float fBottom_Length, float fBottom_Rad)
            {
                m_lstCDelta.Add(new CDelta(fRot_Cw, nID_Front, nID_Left, nID_Right, fTop_Rad, fTop_Length, fBottom_Length, fBottom_Rad));
                return m_lstCDelta.Count;
            }
            public void Delta_Clear() { m_lstCDelta.Clear(); }
            public class CDelta
            {
                public CDelta(float fRot_Cw, int nID_Front, int nID_Left, int nID_Right, float fTop_Rad, float fTop_Length, float fBottom_Length, float fBottom_Rad)
                {
                    Init(fRot_Cw, nID_Front, nID_Left, nID_Right, fTop_Rad, fTop_Length, fBottom_Length, fBottom_Rad);
                }
                public bool IsValid = false;

                private float fRot = 0.0f;
                public int nId_Front = 0;
                public int nId_Left = 1;
                public int nId_Right = 2;

                private float fInit_Top_Radius = 0;
                private float fInit_Top_Length = 0;
                private float fInit_Bottom_Length = 0;
                private float fInit_Bottom_Radius = 0;

                public float[] m_afAngle = new float[3];
                public float x = 0.0f;
                public float y = 0.0f;
                public float z = 0.0f;
                public void Init(float fRot_Cw, int nID_Front, int nID_Left, int nID_Right, float fTop_Rad, float fTop_Length, float fBottom_Length, float fBottom_Rad)
                {
                    fRot = fRot_Cw;
                    nId_Front = nID_Front;
                    nId_Left = nID_Left;
                    nId_Right = nID_Right;
                    fInit_Top_Radius = fTop_Rad;
                    fInit_Top_Length = fTop_Length;
                    fInit_Bottom_Length = fBottom_Length;
                    fInit_Bottom_Radius = fBottom_Rad;

                    IsValid = true;
                }

                public bool CalcXyzToAngle(float fPos_X, float fPos_Y, float fPos_Height, out float fAngle_Front, out float fAngle_Left, out float fAngle_Right)
                {
                    if (IsValid == false)
                    {
                        fAngle_Front = fAngle_Left = fAngle_Right = 0;
                        return false;
                    }
                    if (fRot != 0)
                    {
                        if (Ojw.CMath.CalcRot(0.0f, 0.0f, fRot, ref fPos_X, ref fPos_Y, ref fPos_Height) == false)
                        {
                            fAngle_Front = fAngle_Left = fAngle_Right = 0;
                            return false;
                        }
                    }
                    double[] adVal = new double[3];
                    Ojw.CMath.Delta_Parallel_Init(fInit_Top_Radius, fInit_Top_Length, fInit_Bottom_Length, fInit_Bottom_Radius);
                    Ojw.CMath.Delta_Parallel_InverseKinematics(fPos_X, fPos_Y, fPos_Height, out adVal[0], out adVal[1], out adVal[2]);
                    fAngle_Front = (float)adVal[0];
                    fAngle_Left = (float)adVal[1];
                    fAngle_Right = (float)adVal[2];
                    ///////////
                    x = fPos_X;
                    y = fPos_Y;
                    z = fPos_Height;
                    m_afAngle[0] = (float)adVal[0];
                    m_afAngle[1] = (float)adVal[1];
                    m_afAngle[2] = (float)adVal[2];
                    return true;
                }
                public bool CalcAngleToXyz(float fAngle_Front, float fAngle_Left, float fAngle_Right, out float fX, out float fY, out float fHeight)
                {
                    if (IsValid == false)
                    {
                        fX = fY = fHeight = 0;
                        return false;
                    }
                    double[] adVal = new double[3];
                    Ojw.CMath.Delta_Parallel_Init(fInit_Top_Radius, fInit_Top_Length, fInit_Bottom_Length, fInit_Bottom_Radius);
                    bool bRet = Ojw.CMath.Delta_Parallel_ForwardKinematics(fAngle_Front, fAngle_Left, fAngle_Right, out adVal[0], out adVal[1], out adVal[2]);
                    fX = (float)adVal[0];
                    fY = (float)adVal[1];
                    fHeight = (float)adVal[2];
                    ///////////
                    m_afAngle[0] = fAngle_Front;
                    m_afAngle[1] = fAngle_Left;
                    m_afAngle[2] = fAngle_Right;
                    x = (float)adVal[0];
                    y = (float)adVal[1];
                    z = (float)adVal[2];

                    if (fRot != 0)
                    {
                        if (Ojw.CMath.CalcRot(0.0f, 0.0f, fRot, ref fX, ref fY, ref fHeight) == false)
                        {
                            fX = fY = fHeight = 0;
                            return false;
                        }
                    }
                    return bRet;
                }
            }
            public int GetDelta_Count() { return m_lstCDelta.Count; }
            public void SetDelta(int nIndex, float fX, float fY, float fHeight)
            {
                float[] afAngle = new float[3];
                if ((nIndex >= 0) && (nIndex < m_lstCDelta.Count)) m_lstCDelta[nIndex].CalcXyzToAngle(fX, fY, fHeight, out afAngle[0], out afAngle[1], out afAngle[2]);
                Set(m_lstCDelta[nIndex].nId_Front, afAngle[0]);
                Set(m_lstCDelta[nIndex].nId_Left, afAngle[1]);
                Set(m_lstCDelta[nIndex].nId_Right, afAngle[2]);
            }
            public bool GetDelta(int nIndex, out float fX, out float fY, out float fHeight)
            {
                if ((nIndex >= 0) && (nIndex < m_lstCDelta.Count))
                {
                    return m_lstCDelta[nIndex].CalcAngleToXyz(m_lstCDelta[nIndex].m_afAngle[0], m_lstCDelta[nIndex].m_afAngle[1], m_lstCDelta[nIndex].m_afAngle[2], out fX, out fY, out fHeight);
                }
                fX = fY = fHeight = 0.0f;
                return false;
            }
            public void Move_Delta(int nIndex, float fX, float fY, float fHeight, int nTime, int nDelay, bool bNoWait = false)
            {
                SetDelta(nIndex, fX, fY, fHeight);
                if (bNoWait == false) Move(nTime, nDelay, m_lstCmdIDs.ToArray());
                else Move_NoWait(nTime, nDelay, m_lstCmdIDs.ToArray());
            }
            public void Move_Delta(int nIndex, float fX, float fY, float fHeight, int nTime)
            {
                Move_Delta(nIndex, fX, fY, fHeight, nTime, 0);
            }
            #endregion Delta
        }
    }
}
#else

#endif
