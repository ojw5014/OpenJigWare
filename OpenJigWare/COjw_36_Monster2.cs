using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CMonster2
        {
            public List<int> lstHwKey = new List<int>();
            public List<string> lstHwName = new List<string>();
            public Dictionary<int, string> dicMonster = new Dictionary<int, string>();
            public CMonster2()
            {
                dicMonster.Clear();
                dicMonster.Add(0, "NONE");
            
                // Default
                dicMonster.Add(350, "XL_320"); // 300도 1024 : 512 center : 0.111 RefRpm, 1023 Limit Rpm
                dicMonster.Add(1060, "XL_430"); // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
                dicMonster.Add(1090, "XL_2XL");

                dicMonster.Add(1010, "XH_430_W210");
                dicMonster.Add(1000, "XH_430_W350");
                dicMonster.Add(1050, "XH_430_V210");
                dicMonster.Add(1040, "XH_430_V350");
                
                dicMonster.Add(1130, "XM_540_W150");
                dicMonster.Add(1120, "XM_540_W270");
                dicMonster.Add(1030, "XM_430_W210");
                dicMonster.Add(1020, "XM_430_W350");

                dicMonster.Add(300, "AX_12W"); 
                dicMonster.Add(12, "AX_12"); // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                dicMonster.Add(18, "AX_18"); //
                                
                dicMonster.Add(113, "DX_113"); //
                dicMonster.Add(116, "DX_116"); //
                dicMonster.Add(117, "DX_117"); //
                
                dicMonster.Add(10, "RX_10"); // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                dicMonster.Add(24, "RX_24F"); // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                dicMonster.Add(28, "RX_28"); // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                dicMonster.Add(64, "RX_64"); // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                
                dicMonster.Add(106, "EX_106"); // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 
                dicMonster.Add(107, "EX_106P"); // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 
                
                // protocol 2.0
                dicMonster.Add(360, "MX_12"); // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.916rpm, 다중회전가능(각 7바퀴:-28,672~28,672)
                dicMonster.Add(29, "MX_28_2"); // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                dicMonster.Add(30, "MX_28"); // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                dicMonster.Add(311, "MX_64"); // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                dicMonster.Add(321, "MX_106"); // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                //MX_", 17, //

                //dicMonster.Add(321, "PH_54"); // 24v, 0~360, c=0, mx=501923 * 2, mn=0, drpm = 33.1rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  

                dicMonster.Add(10000, "SG_90");

                for (int i = 0; i < m_aCMot.Length; i++)
                {
                    m_aCMot[i] = new CMot();
                    SetParam(i, EMonster_t.XL_430);
                    m_aCMot[i].bEnable = false;
                }
                m_lstMotors.Clear(); // SetParam 을 하면 모터 리스트가 추가되기 때문에 여기서 없애 준다.

                lstHwKey.Clear();
                lstHwName.Clear();
                foreach (KeyValuePair<int, string> Item in dicMonster)
                {
                    lstHwKey.Add(Item.Key);
                    lstHwName.Add(Item.Value);
                }
                m_lstPush.Clear();
            }
            ~CMonster2()
            {
                Close();
            }
            public CSerial GetSerial(int nIndex = 0)
            {
                if ((nIndex < 0) || (nIndex >= m_lstConnect.Count))
                {
                    return null;
                }
                return m_lstConnect[nIndex].CSerial;
            }
            public int DicMotor_Key(int nIndex) { return lstHwKey[nIndex]; }
            public string DicMotor_Name(int nIndex) { return lstHwName[nIndex]; }
            //public int Dic_Find_Key(string strValue) { return dicMonster.FirstOrDefault(x => x.Value == strValue).Key; }
            // 전체 포트의 인덱스 수를 리턴
            public int GetCount_Connection() { return m_lstConnect.Count; }
            public bool IsOpen()
            {
                return (GetCount_Connection() > 0) ? true : false;
            }
            public bool IsOpen(int nPort)
            {
                foreach (CConnection_t CConnect in m_lstConnect) { if (CConnect.CSerial.GetPortNumber() == nPort) { return true; } }
                return false;
            }
            public int FindSerialIndex_by_Port(int nPort)
            {
                for (int i = 0; i < m_lstConnect.Count; i++)
                {
                    if (m_lstConnect[i].CSerial.GetPortNumber() == nPort)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public int GetProtocol_by_HwNum(EMonster_t EMon)
            {
                switch (EMon)
                {
                    case EMonster_t.AX_12: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMonster_t.AX_12W:
                    case EMonster_t.AX_18:
                    case EMonster_t.DX_113:
                    case EMonster_t.DX_116:
                    case EMonster_t.DX_117:

                    case EMonster_t.RX_10: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMonster_t.RX_24F: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMonster_t.RX_28: // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMonster_t.RX_64: // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)

                    case EMonster_t.MX_28_2: // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  

                    case EMonster_t.EX_106: // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 
                    case EMonster_t.EX_106P: return 1;

                    // protocol 2.0
                    case EMonster_t.MX_12: // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.916rpm, 다중회전가능(각 7바퀴:-28,672~28,672)
                    case EMonster_t.MX_28: // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                    case EMonster_t.MX_64: // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                    case EMonster_t.MX_106: // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                    //MX_ = 17, //

                    case EMonster_t.XL_320: 

                    case EMonster_t.XH_430_W210:
                    case EMonster_t.XH_430_W350:
                    case EMonster_t.XH_430_V210:
                    case EMonster_t.XH_430_V350:

                    case EMonster_t.XM_540_W150:
                    case EMonster_t.XM_540_W270:
                    case EMonster_t.XM_430_W210:
                    case EMonster_t.XM_430_W350:
                    case EMonster_t.XL_430: // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
                    case EMonster_t.XL_2XL: return 2;
                }
                return 0;
            }
            public enum EMonster_t
            {
                NONE = 0,
            
                // Default
                XL_320 = 350, // 300도 1024 : 512 center : 0.111 RefRpm, 1023 Limit Rpm
                XL_430 = 1060, // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
                XL_2XL = 1090,

                XH_430_W210 = 1010,
                XH_430_W350 = 1000,
                XH_430_V210 = 1050,
                XH_430_V350 = 1040,
                
                XM_540_W150 = 1130,
                XM_540_W270 = 1120,
                XM_430_W210 = 1030,
                XM_430_W350 = 1020,
                
                AX_12 = 12, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                AX_18 = 18, //
                AX_12W = 300, //
                                
                DX_113 = 113, //
                DX_116 = 116, //
                DX_117 = 117, //
                
                RX_10 = 10, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_24F = 24, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_28 = 28, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_64 = 64, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                
                EX_106 = 106, // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 
                EX_106P = 107, // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 
                
                // protocol 2.0
                MX_12 = 360, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.916rpm, 다중회전가능(각 7바퀴:-28,672~28,672)
                MX_28_2 = 29, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_28 = 30, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_64 = 311, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_106 = 321, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                //MX_ = 17, //
                //PH_54 = 
                SG_90 = 10000
            }
            public class CMot
            {
                //public enum EOperation_t
                //{
                //    _WHEEL = 1,
                //    _JOINT = 3,
                //    _JOINT_MULTI = 4
                //}
                public int nSeq = 0;
                public int nSeq_Back = 0;
                public bool bEnable = false;
                public int nDir = 0; // 0 : Forward, 1 : Inverse
                public int nCommIndex;
                public float fMechMove = 4096.0f;
                public float fMechAngle = 360.0f;
                public float fJointRpm = 0.229f;
                public float fCenterPos = 2048.0f;
                public float fLimitUp = 0.0f;
                public float fLimitDown = 0.0f;
                public int nMirrorID = -1;
                public byte[] abyMap = new byte[1000];

                private int nPos_Previous = 0;
                //private float fAngle_Previous = 0.0f;
                private bool bPrev = false;

                public int nProtocol = 0;
                public CMot()
                {
                    nRealID = -1;
                    nFirmwareVer = 0;
                }
                ~CMot()
                {
                }
                public bool IsXL320
                {
                    get
                    {
                        return (nHwNum == (int)EMonster_t.XL_320) ? true : false;
                    }
                }
                public bool IsOldAddress
                {
                    get
                    {
                        return (IsXL320) ? true : ((nProtocol == 1) ? true : false);
                    }
                }
                #region Address(공통)
                public int _Address_Torq { get { return ((IsOldAddress) ? 24 : 64); } }
                public int _Address_Led { get { return ((IsOldAddress) ? 25 : 65); } }
                public int _Address_GoalPos_Speed { get { return ((IsOldAddress) ? 32 : 112); } }
                public int _Address_GoalPos { get { return ((IsOldAddress) ? 30 : 116); } }
                public int _Address_GoalSpeed { get { return ((IsOldAddress) ? 32 : 104); } }
                public int _Address_Get_Pos { get { return ((IsOldAddress) ? 36 : 132); } }
                public int _Address_Get_Speed { get { return ((IsOldAddress) ? 38 : 128); } }
                public int _Address_Get_Load { get { return ((IsOldAddress) ? 40 : 126); } }
                public int _Address_Get_Volt { get { return ((IsOldAddress) ? 42 : 144); } }
                public int _Address_Get_Temp { get { return ((IsOldAddress) ? 43 : 146); } }
                public int _Address_Get_IsMoving { get { return ((IsOldAddress) ? 46 : 122); } }
                
                public int _Address_Size_Torq { get { return 1; } }
                public int _Address_Size_Led { get { return 1; } }
                public int _Address_Size_GoalPos_Speed { get { return ((IsOldAddress) ? 2 : 4); } }
                public int _Address_Size_GoalPos { get { return ((IsOldAddress) ? 2 : 4); } }
                public int _Address_Size_GoalSpeed { get { return ((IsOldAddress) ? 2 : 4); } }
                public int _Address_Size_Get_Pos { get { return ((IsOldAddress) ? 2 : 4); } }
                public int _Address_Size_Get_Speed { get { return ((IsOldAddress) ? 2 : 4); } }

                public int _Address_Size_Get_Load { get { return ((IsOldAddress) ? 2 : 2); } }
                public int _Address_Size_Get_Volt { get { return ((IsOldAddress) ? 1 : 2); } }
                public int _Address_Size_Get_Temp { get { return 1; }}// return ((IsOldAddress) ? 1 : 1); } }
                public int _Address_Size_Get_IsMoving { get { return 1; } }// return ((IsOldAddress) ? 1 : 1); } }
                #endregion Address(공통)
                
                #region Address
                // Protocol 1
                public int _Address_TorqPercent { get { return 34; } }

                // Protocol 2
                public int _Address_DriveMode { get { return 10; } }
                public int _Address_OperationMode { get { return 11; } }
                public int _Address_SecondaryID { get { return 12; } }
                public int _Address_HomeOffset { get { return 20; } }
                public int _Address_Pid_Speed_I { get { return 76; } }
                public int _Address_Pid_Speed_P { get { return 78; } }
                public int _Address_Pid_Pos_D { get { return 80; } }
                public int _Address_Pid_Pos_I { get { return 82; } }
                public int _Address_Pid_Pos_P { get { return 84; } }
                public int _Address_GoalPwm { get { return 100; } }
                public int _Address_Get_Pwm { get { return 124; } }
                public int _Address_GoalPos_Acc { get { return 108; } }
                public int _Address_ErrorStatus { get { return 70; } }
                
                public int _Address_Size_DriveMode { get { return ((IsOldAddress) ? 0 : 1); } }
                public int _Address_Size_OperationMode { get { return ((IsOldAddress) ? 0 : 1); } }
                public int _Address_Size_SecondaryID { get { return ((IsOldAddress) ? 0 : 1); } }
                public int _Address_Size_HomeOffset { get { return ((IsOldAddress) ? 0 : 4); } }
                public int _Address_Size_Pid_Speed_I { get { return ((IsOldAddress) ? 0 : 2); } }
                public int _Address_Size_Pid_Speed_P { get { return ((IsOldAddress) ? 0 : 2); } }
                public int _Address_Size_Pid_Pos_D { get { return ((IsOldAddress) ? 0 : 2); } }
                public int _Address_Size_Pid_Pos_I { get { return ((IsOldAddress) ? 0 : 2); } }
                public int _Address_Size_Pid_Pos_P { get { return ((IsOldAddress) ? 0 : 2); } }
                public int _Address_Size_GoalPwm { get { return ((IsOldAddress) ? 0 : 2); } }
                public int _Address_Size_Get_Pwm { get { return ((IsOldAddress) ? 0 : 2); } }
                public int _Address_Size_GoalPos_Acc { get { return ((IsOldAddress) ? 0 : 4); } }
                public int _Address_Size_ErrorStatus { get { return ((IsOldAddress) ? 0 : 1); } }
                #endregion Address

                #region Var
                public int nHwNum
                {
                    get
                    {
                        return Get_Short(0);
                    }
                    set
                    {
                        Set_Short(0, (short)value);
                    }
                }
                public int nFirmwareVer
                {
                    get
                    {
                        return Get((IsOldAddress) ? 2 : 6);
                    }
                    set
                    {
                        Set((IsOldAddress) ? 2 : 6, (byte)value);
                    }
                }
                public int nRealID
                {
                    get
                    {
                        return Get((IsOldAddress) ? 3 : 7);
                    }
                    set
                    {
                        if (nTorq == 0)
                           Set((IsOldAddress) ? 3 : 7, (byte)value);
                    }
                }
                public int nBaudrate
                {
                    get
                    {
                        return Get((IsOldAddress) ? 4 : 8);
                    }
                    set
                    {
                        if (nTorq == 0)
                            Set((IsOldAddress) ? 4 : 8, (byte)value);
                    }
                }
                public int nDriveMode
                {
                    get
                    {
                        return Get(_Address_DriveMode);
                    }
                    set
                    {
                        if (nTorq == 0)
                           if (!IsOldAddress) Set(_Address_DriveMode, (byte)value);
                    }
                }
                public bool IsWheel
                {
                    get
                    {
                        if (IsOldAddress)
                        {
                            if (nHwNum == (int)EMonster_t.XL_320)
                            {
                                if (Get(11) == 1) return true;
                            }
                            else
                            {
                                if (Get_Int(6) == 0) return true;
                            }
                        }
                        return (Get(_Address_OperationMode) == 1) ? true : false;
                    }
                }
                public bool IsJoint
                {
                    get
                    {
                        if (IsOldAddress)
                        {
                            if (nHwNum == (int)EMonster_t.XL_320)
                            {
                                if (Get(11) == 2) return true;
                            }
                            else
                            {
                                if (Get_Int(6) != 0) return true;
                            }
                        }
                        return ((Get(_Address_OperationMode) == 3) || (Get(_Address_OperationMode) == 4)) ? true : false;
                    }
                }
                public bool IsJoint_Multi
                {
                    get
                    {
                        if (IsOldAddress)
                        {
                            if (nHwNum == (int)EMonster_t.XL_320)
                            {
                                return false;
                            }
                            else
                            {
                                if (Get_Int(6) == 0x0fff0fff) return true;
                            }
                        }
                        return (Get(_Address_OperationMode) == 4) ? true : false;
                    }
                }
                public int nOperationMode
                {
                    get
                    {
                        if (IsOldAddress)
                        {
                            if (nHwNum == (int)EMonster_t.XL_320)
                            {
                                if (Get(11) == 1) return 1; // wheel
                                else if (Get(11) == 2) return 3; // Position
                            }
                            else
                            {
                                if (Get_Int(6) == 0) return 1; // wheel
                                else if (Get_Int(6) == 0x0fff0fff) return 4; // multi turn(MX)
                                return 3; // position
                            }
                        }
                        return Get(_Address_OperationMode);
                    }
                    set
                    {
                        if (nTorq == 0)
                        {
                            if (!IsOldAddress) Set(_Address_OperationMode, (byte)value);
                            else if (nHwNum == (int)EMonster_t.XL_320)
                            {
                                if (value == 1) Set(11, 1);
                                else Set(11, 2);
                            }
                            else if (value == 1) Set_Int(6, 0);
                            else if (value == 4) { Set_Short(6, 4095); Set_Short(8, 4095); }
                            else { Set_Short(6, 0); Set_Short(8, 1023); }
                        }
                    }
                }
                public int nTorq
                {
                    get
                    {
                        return Get(_Address_Torq);
                    }
                    set
                    {
                        Set(_Address_Torq, _Address_Size_Torq, value);
                    }
                }
                public int nLed
                {
                    get
                    {
                        return Get(_Address_Led);
                    }
                    set
                    {
                        Set(_Address_Led, _Address_Size_Led, value);
                    }
                }
                public int nGoalPos
                {
                    get
                    {
                        return Get(_Address_GoalPos, _Address_Size_GoalPos); // (IsOldAddress) ? Get_Short(_Address_GoalPos) : Get_Int(_Address_GoalPos);
                    }
                    set
                    {
                        Set(_Address_GoalPos, _Address_Size_GoalPos, (int)value);//if (IsOldAddress) Set_Short(_Address_GoalPos, (short)value); else Set_Int(_Address_GoalPos, (int)value);
                        //nPos_Prev = (int)value;
                    }
                }
                public int nGoalPos_Speed
                {
                    get
                    {
                        return Get(_Address_GoalPos_Speed, _Address_Size_GoalPos_Speed); 
                    }
                    set
                    {
                        Set(_Address_GoalPos_Speed, _Address_Size_GoalPos_Speed, (int)value);
                    }
                }
                public int nGoalPos_Acc
                {
                    get
                    {
                        return Get(_Address_GoalPos_Acc, _Address_Size_GoalPos_Acc);
                    }
                    set
                    {
                        Set(_Address_GoalPos_Acc, _Address_Size_GoalPos_Acc, (int)Math.Abs(value));
                    }
                }
                public int nGoalSpeed
                {
                    get
                    {
                        return Get(_Address_GoalSpeed, _Address_Size_GoalSpeed);
                    }
                    set
                    {
                        Set(_Address_GoalSpeed, _Address_Size_GoalSpeed, (int)value);
                    }
                }
                public int nPos
                {
                    get
                    {
                        return nGoalPos;
                    }
                    set
                    {
                        nGoalPos = value;                        
                    }
                }
                public int nGetPos
                {
                    get
                    {
                        return Get(_Address_Get_Pos, _Address_Size_Get_Pos);
                    }
                    set
                    {
                        //int nValue = value;
                        //if (m_bIgnoredLimit == false)
                        //{
                        //    if (fLimitUp != 0)
                        //    {
                        //        CalcAngle2Evd(fLimitUp) * ((nDir == 0) ? 1.0f : -1.0f);
                        //    }                        
                        //}
                        Set(_Address_Get_Pos, _Address_Size_Get_Pos, (int)value);
                        nPos_Prev = (int)value;
                    }
                }
                public int nSpeed
                {
                    get
                    {
                        return nGoalSpeed;
                    }
                    set
                    {
                        nGoalSpeed = value;
                    }
                }
                public float fSpeed
                {
                    get
                    {
                        return CalcRaw2Rpm(nGoalSpeed);
                    }
                    set
                    {
                        nGoalSpeed = CalcRpm2Raw(value);
                    }
                }
                public int nGetSpeed
                {
                    get
                    {
                        return Get(_Address_Get_Speed, _Address_Size_Get_Speed);
                    }
                    set
                    {
                        Set(_Address_Get_Speed, _Address_Size_Get_Speed, (int)value);
                    }
                }
                public int nLoad
                {
                    get
                    {
                        return Get(_Address_Get_Load, _Address_Size_Get_Load);
                    }
                    set
                    {
                        Set(_Address_Get_Load, _Address_Size_Get_Load, (int)value);
                    }
                }
                public int nVolt
                {
                    get
                    {
                        return Get(_Address_Get_Volt, _Address_Size_Get_Volt);
                    }
                    set
                    {
                        Set(_Address_Get_Volt, _Address_Size_Get_Volt, (int)value);
                    }
                }
                public int nTemp
                {
                    get
                    {
                        return Get(_Address_Get_Temp, _Address_Size_Get_Temp);
                    }
                    set
                    {
                        Set(_Address_Get_Temp, _Address_Size_Get_Temp, (int)value);
                    }
                }

                public float fAngle
                {
                    get
                    {
                        return CalcEvd2Angle(nGoalPos);
                    }
                    set
                    {
                        nGoalPos = CalcAngle2Evd(value);
                    }
                }
                public float fGetAngle
                {
                    get
                    {
                        return CalcEvd2Angle(nGetPos);
                    }
                    set
                    {
                        nGetPos = CalcAngle2Evd(value);
                    }
                }

                public float fTime
                {
                    get
                    {
                        return (nGoalPos_Speed);
                    }
                    set
                    {
                        if (IsTimeControl) nGoalPos_Speed = (int)Math.Round(value);
                        else nGoalPos_Speed = Math.Abs(CalcRpm2Raw(CalcTime2Rpm((CalcEvd2Angle_Raw(nGoalPos - ((bPrev) ? nPos_Prev : nGetPos))), value)));
                    }
                }

                public int nPos_Prev
                {
                    get
                    {
                        return nPos_Previous;
                    }
                    set
                    {
                        if (bPrev == false) bPrev = true;
                        nPos_Previous = (int)value;
                    }
                }
                public float fAngle_Prev
                {
                    get
                    {
                        return CalcEvd2Angle(nPos_Previous);
                    }
                    set
                    {
                        nPos_Previous = CalcAngle2Evd((float)value);
                    }
                }
                
                #region Calc
                //  Move  ///////////////////////////
                private bool m_bIgnoredLimit = false;
                public void SetLimitEn(bool bOn) { m_bIgnoredLimit = !bOn; }
                public bool GetLimitEn() { return !m_bIgnoredLimit; }
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

                public int CalcLimit_Evd(int nValue)
                {
                    //if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                    //{
                    int nPulse = nValue;// &0x4000;
                    //if (m_bMultiTurn == false)
                    //{
                    //nValue &= 0x4000;
                    //nValue &= 0x3fff;
                    //}

                    //nValue &= 0x3fff;
                    int nUp = 10000000;
                    int nDn = -nUp;
                    if (fLimitUp != 0) nUp = CalcAngle2Evd(fLimitUp);
                    if (fLimitDown != 0) nDn = CalcAngle2Evd(fLimitDown);
                    if (nUp < nDn) { int nTmp = nUp; nUp = nDn; nDn = nTmp; }
                    return (Clip(nUp, nDn, nValue) | nPulse);
                    //}
                    //return nValue;
                }
                public float CalcLimit_Angle(float fValue)
                {
                    //if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                    //{
                    float fUp = 1000000.0f;
                    float fDn = -fUp;
                    if (fLimitUp != 0) fUp = fLimitUp;
                    if (fLimitDown != 0) fDn = fLimitDown;
                    return Clip(fUp, fDn, fValue);
                    //}
                    //return fValue;
                }
                public int CalcAngle2Evd(float fValue)
                {
                    fValue *= ((nDir == 0) ? 1.0f : -1.0f);
                    int nData = 0;
                    //if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                    //{
                    //    nData = (int)Math.Round(fValue);
                    //    //Ojw.CMessage.Write("Speed Turn");
                    //}
                    //else
                    //{
                    // 위치제어
                    nData = (int)Math.Round((fMechMove * fValue) / fMechAngle + fCenterPos);
                    //}
                    return nData;
                }
                public float CalcEvd2Angle_Raw(int nValue)
                {
                    float fValue = ((nDir == 0) ? 1.0f : -1.0f);
                    float fValue2 = 0.0f;
                    fValue2 = (float)(((fMechAngle * (float)nValue) / fMechMove) * fValue);
                    return fValue2;
                }
                public float CalcEvd2Angle(int nValue)
                {
                    float fValue = ((nDir == 0) ? 1.0f : -1.0f);
                    float fValue2 = 0.0f;
                    //if (Get_Flag_Mode(nMotor) != 0)   // 속도제어
                    //    fValue2 = (float)nValue * fValue;
                    //else                                // 위치제어
                    //{
                    fValue2 = (float)(((fMechAngle * ((float)(nValue - (int)Math.Round(fCenterPos)))) / fMechMove) * fValue);
                    //}
                    return fValue2;
                }
                private float CalcRaw2Rpm(int nValue) { return (float)nValue * fJointRpm; }
                //private int CalcRpm2Raw(float fRpm) { return (int)Math.Round(Clip(m_aCParam[nMotor].fLimitRpm, -m_aCParam[nMotor].fLimitRpm, fRpm / fJointRpm)); }
                private int CalcRpm2Raw(float fRpm) { return (int)Math.Round(fRpm / fJointRpm); }

                private float CalcTime2Rpm(float fDeltaAngle, float fTime)
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
                #endregion Calc

                public bool IsMoving
                {
                    get
                    {
                        return Ojw.CConvert.IntToBool(Get(_Address_Get_IsMoving, _Address_Size_Get_IsMoving));
                    }
                    set
                    {
                        Set(_Address_Get_IsMoving, _Address_Size_Get_IsMoving, Ojw.CConvert.BoolToInt(value));
                    }
                }
                public bool IsTimeControl
                {
                    get
                    {
                        return (IsOldAddress) ? false : Ojw.CConvert.IntToBool(Get(_Address_DriveMode, _Address_Size_DriveMode) & 0x04);
                    }
                    set
                    {
                        if (IsOldAddress)
                            Set(_Address_Get_IsMoving, _Address_Size_Get_IsMoving, (Get(_Address_DriveMode, _Address_Size_DriveMode) & 0x01) | (Ojw.CConvert.BoolToInt(value) << 2));
                    }
                }
                //public int nError
                //{
                //    get
                //    {
                //        return Get(_Address_ErrorStatus, _Address_Size_ErrorStatus);
                //    }
                //    set
                //    {
                //        Set(_Address_ErrorStatus, _Address_Size_ErrorStatus, (int)value);
                //    }
                //}
                #endregion Var

                #region 표준함수
                public byte Get(int nIndex) { if ((nIndex >= 0) && (nIndex < abyMap.Length)) return abyMap[nIndex]; return 0; }
                public short Get_Short(int nIndex) { if ((nIndex >= 0) && (nIndex < abyMap.Length - (sizeof(short) - 1))) return Ojw.CConvert.BytesToShort(abyMap, nIndex); return 0; }
                public int Get_Int(int nIndex) { if ((nIndex >= 0) && (nIndex < abyMap.Length - (sizeof(int) - 1))) return Ojw.CConvert.BytesToInt(abyMap, nIndex); return 0; }

                public int Get(int nIndex, int nSize)
                {
                    if (nSize == 1) return Get(nIndex);
                    else if (nSize == 2) return Get_Short(nIndex);
                    else if (nSize == 4) return Get_Int(nIndex);
                    return 0;
                }
                public void Set(int nIndex, int nSize, int nValue) 
                {
                    if (nSize == 1) Set(nIndex, (byte)nValue);
                    else if (nSize == 2) Set_Short(nIndex, (short)nValue);
                    else if (nSize == 4) Set_Int(nIndex, nValue);
                }
                
                public void Set(int nIndex, byte byValue) { if ((nIndex >= 0) && (nIndex < abyMap.Length)) abyMap[nIndex] = byValue; }
                public void Set_Short(int nIndex, short sValue) 
                {
                    if ((nIndex >= 0) && (nIndex < abyMap.Length))
                    {
                        byte[] abyTmp = Ojw.CConvert.ShortToBytes(sValue);
                        abyMap[nIndex] = abyTmp[0];
                        abyMap[nIndex+1] = abyTmp[1];
                    }
                }
                public void Set_Int(int nIndex, int nValue)
                {
                    if ((nIndex >= 0) && (nIndex < abyMap.Length))
                    {
                        byte[] abyTmp = Ojw.CConvert.IntToBytes(nValue);
                        for (int i = 0; i < sizeof(int); i++) abyMap[nIndex + i] = abyTmp[i];
                    }
                }
                #endregion 표준함수
            }
            public CMot[] m_aCMot = new CMot[256];
            //public struct SSync_t
            //{
            //    int nID;
            //    int nAddress;
            //    int nSize;
            //    public SSync_t(int id, int address, int size)
            //    {
            //        nID = id;
            //        nAddress = address;
            //        nSize = size;
            //    }
            //    //int nData1;
            //    //int nData2;
            //    //int nData3;
            //}
            //public List<SSync_t> m_lstPush = new List<SSync_t>();
            private List<int> m_lstPush = new List<int>();
            private int m_nCnt_Push_1 = 0;
            private int m_nCnt_Push_2 = 0;
            private int m_nCnt_Push_2_Wheel = 0;
            private int m_nCnt_Push_xl320 = 0;
            //public List<CMot> m_lstMot = new List<CMot>();

            public class CConnection_t
            {
                public CConnection_t()
                {
                }
                ~CConnection_t()
                {
                }
                public int nProtocol = 0; // (0 - 2, 1 - 1, 2 - 2, -2 - xl320)
                public int nType = 0; // 0 - None, 1 - serial, 2 - tcp/ip
                public Ojw.CSerial CSerial = new CSerial();
                public Ojw.CSocket CSock = new CSocket();
                public int nRequestData = 0;
            }
            private List<CConnection_t> m_lstConnect = new List<CConnection_t>();
            
            public bool Open(ref CProtocol2 CPrt, int nPort = -1, int nBaudRate = 1000000)
            {
                bool bConnected = CPrt.IsOpen();
                bool bDuplicated = false;
                // 시리얼 끼리 비교해서 중복 처리
                foreach (CConnection_t CConnect in m_lstConnect) { if (CConnect.nType == 1) { if (CConnect.CSerial.GetPortNumber() == CPrt.m_CSerial.GetPortNumber()) { bDuplicated = true; break; } } }
                
                if (bDuplicated == false)
                {
                    if (bConnected == false)
                    {
                        if (CPrt.Open(nPort, nBaudRate) == false)
                        {
                            Ojw.CMessage.Write_Error("[Monster Class]Cannot Connect [Open_Serial(CProtocol2: {0}, {1})]", nPort, nBaudRate);
                            return false;
                        }
                    }
                    CConnection_t CConnect = new CConnection_t();
                    CConnect.CSerial = CPrt.m_CSerial;
                    if (CConnect.CSerial.IsConnect() == true)
                    {
                        CConnect.nType = 1;
                        m_lstConnect.Add(CConnect);
                        return true;
                    }
                    else
                    {
                        Ojw.CMessage.Write_Error("[Monster Class]Cannot Connect [Open_Serial({0}, {1})]", nPort, nBaudRate);
                        return false;
                    }
                }
                Ojw.CMessage.Write_Error("[Monster Class]Cannot Connect [Open_Serial({0}, {1})]", nPort, nBaudRate);
                return false;
            }
            public bool Open(int nPort, int nBaudRate) { return Open_Serial(nPort, nBaudRate); }
            public bool Open(int nPort, string strIpAddress) { return Open_Socket(nPort, strIpAddress); }
            private bool Open_Socket(int nPort, string strIpAddress)
            {
                bool bConnected = false;
                // 중복 처리
                foreach (CConnection_t CConnect in m_lstConnect) { if (CConnect.nType == 2) { if (CConnect.CSock.m_nPort == nPort) { bConnected = CConnect.CSock.IsConnect(); break; } } }
                // 중복이 없다면
                if (bConnected == false)
                {
                    CConnection_t CConnect = new CConnection_t();
                    if (CConnect.CSock.Connect(strIpAddress, nPort, false) == true)
                    {
                        CConnect.nType = 2;
                        m_lstConnect.Add(CConnect);
                        return true;
                    }
                    else
                    {
                        Ojw.CMessage.Write_Error("[Monster Class]Cannot Connect [Open_Socket({0}, {1})]", nPort, strIpAddress);
                        return false;
                    }
                }

                Ojw.CMessage.Write_Error("[Monster Class]Cannot Connect [Open_Socket({0}, {1})]", nPort, strIpAddress);
                return false;
            }
            private bool Open_Serial(int nPort, int nBaudRate)
            {
                bool bConnected = false;
                // 시리얼 끼리 비교해서 중복 처리
                foreach (CConnection_t CConnect in m_lstConnect) { if (CConnect.nType == 1) { if (CConnect.CSerial.GetPortNumber() == nPort) { bConnected = CConnect.CSerial.IsConnect(); break; } } }

                if (bConnected == false)
                {
                    CConnection_t CConnect = new CConnection_t();
                    if (CConnect.CSerial.Connect(nPort, nBaudRate) == true)
                    {
                        CConnect.nType = 1;
                        m_lstConnect.Add(CConnect);
                        return true;
                    }
                    else
                    {
                        Ojw.CMessage.Write_Error("[Monster Class]Cannot Connect [Open_Serial({0}, {1})]", nPort, nBaudRate);
                        return false;
                    }
                }
                Ojw.CMessage.Write_Error("[Monster Class]Cannot Connect [Open_Serial({0}, {1})]", nPort, nBaudRate);
                return false;
            }
            // 지정한 포트를 닫는다.
            public void Close(int nIndex)
            {
                if ((nIndex < 0) || (nIndex >= m_lstConnect.Count)) return;
                switch (m_lstConnect[nIndex].nType)
                {
                    case 1:
                        {
                            if (m_lstConnect[nIndex].CSerial.IsConnect()) m_lstConnect[nIndex].CSerial.DisConnect();
                        }
                        break;
                    case 2:
                        {
                            if (m_lstConnect[nIndex].CSock.IsConnect()) m_lstConnect[nIndex].CSock.DisConnect();
                        }
                        break;
                }
                m_lstConnect[nIndex].nType = 0;
                m_lstConnect.RemoveAt(nIndex);
            }
            // 전체 포트를 닫는다.
            public void Close()
            {
                int nCount = m_lstConnect.Count;
                for (int i = 0; i < nCount; i++)
                {
                    switch (m_lstConnect[i].nType)
                    {
                        case 1:
                            {
                                if (m_lstConnect[i].CSerial.IsConnect()) m_lstConnect[i].CSerial.DisConnect();
                            }
                            break;
                        case 2:
                            {
                                if (m_lstConnect[i].CSock.IsConnect()) m_lstConnect[i].CSock.DisConnect();
                            }
                            break;
                    }
                    m_lstConnect[i].nType = 0;
                }
                m_lstConnect.Clear();
            }
            public class CMonsterPacket_t
            {
                public int nSeq = 0;
                public int nSeq_Back = 0;
                public int nProtocol = 0;
                public int nIndex = 0;
                public int nHeader = 0; // 2 = protocol1, 4 = protocol2
                public int nLength = 0;
                public int nID = 0;
                public int nCommand = 0;
                public int nDataLength = 0;
                public byte[] abyData = new byte[2048];
                public int nChecksumOk = 0;
                public bool bOk = false; // Checksum
                public int nError = 0;
                public int nCrcTmp = 0;
                //public byte[] abyCrc = new byte[200];
                public void Clear()
                {
                    nSeq = 0;
                    nSeq_Back = 0;
                    nProtocol = 0;
                    nIndex = 0;
                    nHeader = 0; // 2 = protocol1, 4 = protocol2
                    nLength = 0;
                    nDataLength = 0;
                    nID = 0;
                    nCommand = 0;
                    nChecksumOk = 0;
                    bOk = false; // Checksum                
                    nError = 0;
                    nCrcTmp = 0;
                    //Array.Clear(abyCrc, 0, abyCrc.Length);
                }
            }

            private CMonsterPacket_t m_CPacket = new CMonsterPacket_t();
           

            public CMonsterPacket_t[] PacketToClass(int nProtocol_Version, byte [] abyData)
            {
                List<CMonsterPacket_t> lstCPacket = new List<CMonsterPacket_t>();
                lstCPacket.Clear();

    
        

                if (false)//(abyData != null)
                {
#if false
                        // test
                        for (int i = 0; i < abyData.Length; i++)
                        {
                            printf("{0} ", abyData[i]);
                        }
                        newline();
#endif
                    int nBreak = 0;
                    for (int i = 0; i < abyData.Length; i++)
                    {
#if false
                            if (nProtocol_Version == 1)
                            {
                                if (CPacket.nHeader == 2)
                                {
                                    switch (nHeader)
                                    {
                                        case 0: if (abyData[i] == 0xff) nHeader++; break;
                                        case 1: if (abyData[i] == 0xff) nHeader++; break;
                                    }
                                }
                            }
                            else
                            {
                                if (CPacket.nHeader == 4)
                                {
                                }
                            }
#endif
                        
                        switch (m_CPacket.nIndex)
                        {
                            case 0:
                                if (abyData[i] == 0xff)
                                    m_CPacket.nHeader++;
                                else
                                {
                                    m_CPacket.Clear();
                                    continue;
                                }
                                break;
                            case 1:
                                if (abyData[i] == 0xff)
                                {
                                    m_CPacket.nHeader++;
                                    if (nProtocol_Version == 1) m_CPacket.nIndex += 2;
                                }
                                else
                                {
                                    m_CPacket.Clear();
                                    continue;
                                }
                                break;
                            case 2:
                                if (abyData[i] == 0xfd)
                                    m_CPacket.nHeader++;
                                else
                                {
                                    m_CPacket.Clear();
                                    continue;
                                }
                                break;
                            case 3:
                                if (abyData[i] == 0x00)
                                    m_CPacket.nHeader++;
                                else
                                {
                                    m_CPacket.Clear();
                                    continue;
                                }
                                break;
                            case 4: m_CPacket.nID = abyData[i]; break;
                            case 5:
                                m_CPacket.nLength = abyData[i];
                                if (nProtocol_Version == 1) m_CPacket.nIndex++;
                                break;
                            case 6: m_CPacket.nLength += abyData[i] * 256; break;
                            case 7:
                                if (nProtocol_Version == 1)
                                {
                                    m_CPacket.nError = abyData[i];
                                    m_CPacket.nIndex++;
                                    if ((m_CPacket.nLength - 2) <= 0) m_CPacket.nIndex++;
                                    //m_CPacket.abyData[m_CPacket.nDataLength++] = abyData[i];
                                }
                                else
                                {
                                    m_CPacket.nCommand = abyData[i];
                                }
                                break;
                            case 8:
                                m_CPacket.nError = abyData[i];
                                if ((m_CPacket.nLength - 4) <= 0) m_CPacket.nIndex++;
                                break;
                            case 9:
                                m_CPacket.abyData[m_CPacket.nDataLength++] = abyData[i];
                                if (nProtocol_Version == 1)
                                {
                                    if (m_CPacket.nDataLength < m_CPacket.nLength - 2) m_CPacket.nIndex--;
                                }
                                else
                                {
                                    if (m_CPacket.nDataLength < m_CPacket.nLength - 4) m_CPacket.nIndex--;
                                }
                                break;
                            case 10:
                                // CheckSum
                                m_CPacket.nCrcTmp = abyData[i];
                                if (nProtocol_Version == 1)
                                {
                                    #region CRC
                                    int nCrc = m_CPacket.nID + m_CPacket.nLength + m_CPacket.nCommand;
                                    for (int j = 0; j < m_CPacket.nDataLength; j++) nCrc += m_CPacket.abyData[j];
                                    //Ojw.printf("CRC-P1:Receive:{0} - Calc:{1}\r\n", m_CPacket.nCrcTmp, (byte)(~nCrc & 0xff));
                                    if ((byte)(~nCrc & 0xff) != m_CPacket.nCrcTmp)
                                    {
                                        m_CPacket.Clear();
                                        return null;
                                    }
                                    #endregion CRC

                                    m_CPacket.nProtocol = nProtocol_Version;

                                    m_CPacket.bOk = true;
                                    m_CPacket.nChecksumOk = 1;
                                    m_CPacket.nSeq++;

                                    lstCPacket.Add(m_CPacket);
                                    m_CPacket = new CMonsterPacket_t();
                                    m_CPacket.Clear();
                                    continue;
                                }
                                break;
                            case 11:

                                int nCrc_accum = 0;
                                nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ 0xff) & 0xFF)];
                                nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ 0xff) & 0xFF)];
                                nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ 0xfd) & 0xFF)];
                                nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ 0x00) & 0xFF)];
                                nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ m_CPacket.nID) & 0xFF)];
                                nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ m_CPacket.nLength) & 0xFF)];
                                nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ (m_CPacket.nLength >> 8) & 0xFF) & 0xFF)];
                                nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ m_CPacket.nCommand) & 0xFF)];
                                nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ m_CPacket.nError) & 0xFF)];
                                for (int nCrcIndex = 0; nCrcIndex < m_CPacket.nDataLength; nCrcIndex++) nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ m_CPacket.abyData[nCrcIndex]) & 0xFF)];

                                //Ojw.printf("CRC-P2:Receive:{0}.{1} - Calc:{2},{3}\r\n", m_CPacket.nCrcTmp, abyData[i], (byte)(nCrc_accum & 0xff), (byte)((nCrc_accum >> 8) & 0xff));
                                if (m_CPacket.nCrcTmp != (nCrc_accum & 0xff))
                                {
                                    if (abyData[i] != ((nCrc_accum >> 8) & 0xff))
                                    {
                                        m_CPacket.Clear();
                                        return null;
                                    }
                                }


                                // CheckSum
                                m_CPacket.nProtocol = nProtocol_Version;
                                m_CPacket.bOk = true;
                                m_CPacket.nChecksumOk = 2;
                                m_CPacket.nSeq++;

                                lstCPacket.Add(m_CPacket);
                                m_CPacket = new CMonsterPacket_t();
                                m_CPacket.Clear();
                                //continue;

                                break;
                        }
                        m_CPacket.nIndex++;
                    }
                }
                return lstCPacket.ToArray();
            }
            public bool PacketToMap(int nProtocol_Version, int nAddress, byte[] abyData)
            {
                bool bRet = false;
                
                if (abyData != null)
                {
#if false
                        // test
                        for (int i = 0; i < abyData.Length; i++)
                        {
                            printf("{0} ", abyData[i]);
                        }
                        newline();
#endif

                    for (int i = 0; i < abyData.Length; i++)
                    {
                        switch (m_CPacket.nIndex)
                        {
                            case 0:
                                if (abyData[i] == 0xff)
                                    m_CPacket.nHeader++;
                                else
                                {
                                    m_CPacket.Clear();
                                    continue;
                                }
                                break;
                            case 1:
                                if (abyData[i] == 0xff)
                                {
                                    m_CPacket.nHeader++;
                                    if (nProtocol_Version == 1) m_CPacket.nIndex += 2;
                                }
                                else
                                {
                                    m_CPacket.Clear();
                                    continue;
                                }
                                break;
                            case 2:
                                if (abyData[i] == 0xfd)
                                    m_CPacket.nHeader++;
                                else
                                {
                                    m_CPacket.Clear();
                                    continue;
                                }
                                break;
                            case 3:
                                if (abyData[i] == 0x00)
                                    m_CPacket.nHeader++;
                                else
                                {
                                    m_CPacket.Clear();
                                    continue;
                                }
                                break;
                            case 4: m_CPacket.nID = abyData[i]; break;
                            case 5:
                                m_CPacket.nLength = abyData[i];
                                if (nProtocol_Version == 1) m_CPacket.nIndex++;
                                break;
                            case 6: m_CPacket.nLength += abyData[i] * 256; break;
                            case 7:
                                if (nProtocol_Version == 1)
                                {
                                    m_CPacket.nError = abyData[i];
                                    m_CPacket.nIndex++;
                                    if ((m_CPacket.nLength - 2) <= 0) m_CPacket.nIndex++;
                                    //m_CPacket.abyData[m_CPacket.nDataLength++] = abyData[i];
                                }
                                else
                                {
                                    m_CPacket.nCommand = abyData[i];
                                }
                                break;
                            case 8:
                                m_CPacket.nError = abyData[i];
                                if ((m_CPacket.nLength - 4) <= 0) m_CPacket.nIndex++;
                                break;
                            case 9:
                                m_CPacket.abyData[m_CPacket.nDataLength++] = abyData[i];
                                if (nProtocol_Version == 1)
                                {
                                    if (m_CPacket.nDataLength < m_CPacket.nLength - 2) m_CPacket.nIndex--;
                                }
                                else
                                {
                                    if (m_CPacket.nDataLength < m_CPacket.nLength - 4) m_CPacket.nIndex--;
                                }
                                break;
                            case 10:
                                // CheckSum
                                m_CPacket.nCrcTmp = abyData[i];
                                if (nProtocol_Version == 1)
                                {
                                    #region CRC
                                    int nCrc = m_CPacket.nID + m_CPacket.nLength + m_CPacket.nCommand;
                                    for (int j = 0; j < m_CPacket.nDataLength; j++) nCrc += m_CPacket.abyData[j];
                                    //Ojw.printf("CRC-P1:Receive:{0} - Calc:{1}\r\n", m_CPacket.nCrcTmp, (byte)(~nCrc & 0xff));
                                    if ((byte)(~nCrc & 0xff) != m_CPacket.nCrcTmp)
                                    {
                                        m_CPacket.Clear();
                                        return false;
                                    }
                                    #endregion CRC

                                    m_CPacket.nProtocol = nProtocol_Version;

                                    m_CPacket.bOk = true;
                                    m_CPacket.nChecksumOk = 1;
                                    m_CPacket.nSeq++;

                                    //lstCPacket.Add(m_CPacket);
                                    //m_CPacket = new CMonsterPacket_t();
                                    if ((m_CPacket.nID >= 0) && (m_CPacket.nID < 255))
                                    {
                                        Array.Copy(m_CPacket.abyData, 0, m_aCMot[m_CPacket.nID].abyMap, nAddress, m_CPacket.nDataLength);
                                        bRet = true;
                                    }

                                    m_CPacket.Clear();
                                    continue;
                                }
                                break;
                            case 11:
                                if (nProtocol_Version == 2)
                                {
                                    int nCrc_accum = 0;
                                    nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ 0xff) & 0xFF)];
                                    nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ 0xff) & 0xFF)];
                                    nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ 0xfd) & 0xFF)];
                                    nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ 0x00) & 0xFF)];
                                    nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ m_CPacket.nID) & 0xFF)];
                                    nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ m_CPacket.nLength) & 0xFF)];
                                    nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ (m_CPacket.nLength >> 8) & 0xFF) & 0xFF)];
                                    nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ m_CPacket.nCommand) & 0xFF)];
                                    nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ m_CPacket.nError) & 0xFF)];
                                    for (int nCrcIndex = 0; nCrcIndex < m_CPacket.nDataLength; nCrcIndex++) nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ m_CPacket.abyData[nCrcIndex]) & 0xFF)];

                                    //Ojw.printf("CRC-P2:Receive:{0}.{1} - Calc:{2},{3}\r\n", m_CPacket.nCrcTmp, abyData[i], (byte)(nCrc_accum & 0xff), (byte)((nCrc_accum >> 8) & 0xff));
                                    if (m_CPacket.nCrcTmp != (nCrc_accum & 0xff))
                                    {
                                        if (abyData[i] != ((nCrc_accum >> 8) & 0xff))
                                        {
                                            m_CPacket.Clear();
                                            return false;
                                        }
                                    }


                                    // CheckSum
                                    m_CPacket.nProtocol = nProtocol_Version;
                                    m_CPacket.bOk = true;
                                    m_CPacket.nChecksumOk = 2;
                                    m_CPacket.nSeq++;

                                    if ((m_CPacket.nID >= 0) && (m_CPacket.nID < 255))
                                    {
                                        Array.Copy(m_CPacket.abyData, 0, m_aCMot[m_CPacket.nID].abyMap, nAddress, m_CPacket.nDataLength);
                                        bRet = true;
                                    }
                                    m_CPacket.Clear();
                                    continue;                                     
                                }
                                else { m_CPacket.Clear(); return false; }
                                break;
                        }
                        m_CPacket.nIndex++;
                    }
                }
                return bRet;
            }

            public bool m_bStop = false;
            public bool m_bEms = false;
            public bool m_bProgEnd = false;

            public float Get(int nMotor) 
            {
                if (m_aCMot[nMotor].IsWheel) return m_aCMot[nMotor].fSpeed;
                return m_aCMot[nMotor].fAngle;            
            }
            public void Set(int nMotor, float fValue)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                //if (m_aCMot[nMotor].nOperationMode == 1) // Wheel Mode
                //{
                //    SetTorq(nMotor, false);
                //    Send(
                //}
                if (m_aCMot[nMotor].IsWheel) m_aCMot[nMotor].fSpeed = (int)fValue;
                else m_aCMot[nMotor].fAngle = fValue;
                if (m_lstPush.FindIndex(x => x == nMotor) < 0)
                {
                    m_lstPush.Add(nMotor);
                    if (m_aCMot[nMotor].nProtocol == 1)
                    {
                        m_nCnt_Push_1++;
                    }
                    else if (m_aCMot[nMotor].IsXL320 == true)
                    {
                        m_nCnt_Push_xl320++;
                    }
                    else
                    {
                        if (m_aCMot[nMotor].IsWheel)
                            m_nCnt_Push_2_Wheel++;
                        else 
                            m_nCnt_Push_2++;
                    }
                }
                //m_lstPush.Add(new SSync_t(nMotor, (m_aCMot[nMotor].IsWheel) ? ));          
            }
            public void Set_Raw(int nMotor, int nValue)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                //if (m_aCMot[nMotor].nOperationMode == 1) // Wheel Mode
                //{
                //    SetTorq(nMotor, false);
                //    Send(
                //}
                if (m_aCMot[nMotor].IsWheel) m_aCMot[nMotor].nGoalSpeed = nValue;
                else m_aCMot[nMotor].nGoalPos = nValue;
                if (m_lstPush.FindIndex(x => x == nMotor) < 0)
                {
                    m_lstPush.Add(nMotor);
                    if (m_aCMot[nMotor].nProtocol == 1)
                    {
                        m_nCnt_Push_1++;
                    }
                    else if (m_aCMot[nMotor].IsXL320 == true)
                    {
                        m_nCnt_Push_xl320++;
                    }
                    else
                    {
                        if (m_aCMot[nMotor].IsWheel)
                            m_nCnt_Push_2_Wheel++;
                        else
                            m_nCnt_Push_2++;
                    }
                }
                //m_lstPush.Add(new SSync_t(nMotor, (m_aCMot[nMotor].IsWheel) ? ));          
            }
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                Write_Sync(nProtocol_Version, nSerialIndex, nAddress, nDataLength_without_ID, nMotorCnt, pbyDatas);
            }
            public void Write_Sync(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;

                byte[] pbyteBuffer = new byte[((nProtocol_Version == 1) ? 1 : 2) + nLength];
                if (nProtocol_Version == 1)
                {
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                }
                else
                {
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                }
                Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                pbyteBuffer = null;
            }
            private int m_nRequestMotors = 0;
            List<int> m_anRequestMotors = new List<int>();
            public void Request_Push(int nMotor) { m_anRequestMotors.Add(nMotor); }
            public void Request_Clear() { m_anRequestMotors.Clear(); }
            public void  Request_Flush(int nProtocol_Version, int nSerialIndex, int nAddress, int nSize)
            {
                byte[] pbyDatas = new byte[4 + m_anRequestMotors.Count];
                int nPos = 0;
                pbyDatas[nPos++] = (byte)(nAddress & 0xff);
                pbyDatas[nPos++] = (byte)((nAddress >> 8) & 0xff);
                pbyDatas[nPos++] = (byte)(nSize & 0xff);
                pbyDatas[nPos++] = (byte)((nSize >> 8) & 0xff);
                for (int i = 0; i < m_anRequestMotors.Count; i++)
                {
                    pbyDatas[nPos++] = (byte)(m_anRequestMotors[i]);
                }
                m_nRequestMotors = m_anRequestMotors.Count;

                Request_with_RealID(nProtocol_Version, nSerialIndex, 254, 0x82, pbyDatas);
                Request_Clear();
            }  
            
            //private struct SSyncData_t
            //{
            //    public int nAddress;
            //    public int nID;
            //    public int nSize;
            //    public int nData;
            //    //public SSyncData_t(int id, int data, int address, int size)
            //    public SSyncData_t(int id, int address, int size)
            //    {
            //        nSize = size;
            //        nID = id;
            //        //nData = ((nSize == 1) ? data & 0xff : ((nSize == 2) ? data & 0xffff : data));
            //        nAddress = address;
            //    }
            //}
            //private List<SSyncData_t> m_lstSSync_1 = new List<SSyncData_t>();
            //private List<SSyncData_t> m_lstSSync_2 = new List<SSyncData_t>();
            //private List<SSyncData_t> m_lstSSync_Xl320 = new List<SSyncData_t>();
                        
            //public void Sync_Push(int nID, int nData, int nAddress, int nSize)
            //{
            //    if (m_aCMot[nID].IsXL320) m_lstSSync_Xl320.Add(new SSyncData_t(nID, nData, nAddress, nSize));
            //    else if (m_aCMot[nID].nProtocol == 1) m_lstSSync_1.Add(new SSyncData_t(nID, nData, nAddress, nSize));
            //    else m_lstSSync_2.Add(new SSyncData_t(nID, nData, nAddress, nSize));
            //}
            //public void Sync_Push(int nID, int nData1, int nData2, int nAddress, int nSize)
            //{
            //    if (m_aCMot[nID].IsXL320) m_lstSSync_Xl320.Add(new SSyncData_t(nID, nData & 0xff, nAddress, nSize * 2));
            //    else if (m_aCMot[nID].nProtocol == 1) m_lstSSync_1.Add(new SSyncData_t(nID, nData, nAddress, nSize * 2));
            //    else m_lstSSync_2.Add(new SSyncData_t(nID, nData, nAddress, nSize));
            //}
            //public void Sync_Clear() { m_lstSSync_1.Clear(); m_lstSSync_2.Clear(); m_lstSSync_Xl320.Clear(); }
            //public void Set(int nID, float fAngle)
            //{
            //    m_aCMot[nID].nPos = m_aCMot[nID].CalcAngle2Evd(fAngle);
            //}
            
            public void Send_Motor(int nTime)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                byte [] buffer0 = new byte[(4 + 1) * m_nCnt_Push_xl320];
                byte[] buffer1 = new byte[(4 + 1) * m_nCnt_Push_1];
                byte[] buffer2 = new byte[(8 + 1) * m_nCnt_Push_2];
                byte[] buffer2_Wheel = new byte[(4 + 1) * m_nCnt_Push_2_Wheel];
                int nIndex0 = 0;
                int nIndex1 = 0;
                int nIndex2 = 0;
                int nIndex2_Wheel = 0;

                int nCnt_Push_1 = m_nCnt_Push_1;
                int nCnt_Push_2 = m_nCnt_Push_2;
                int nCnt_Push_2_Wheel = m_nCnt_Push_2_Wheel;
                int nCnt_Push_xl320 = m_nCnt_Push_xl320;

                if (nCnt_Push_1 > 0)
                {
#if false
                    for (int nSerial = 0; nSerial < m_lstConnect.Count; nSerial++)
                    {
                        for (int i = 0; i < m_lstPush.Count; i++)
                        {
                            if ((m_aCMot[m_lstPush[i]].nProtocol == 1) && (m_aCMot[m_lstPush[i]].nCommIndex == nSerial))
                            {
                                buffer1[nIndex1++] = (byte)m_lstPush[i];
                                Array.Copy(m_aCMot[m_lstPush[i]].abyMap,
                                    m_aCMot[m_lstPush[i]]._Address_GoalPos_Speed,
                                    buffer1,
                                    nIndex1,
                                    m_aCMot[m_lstPush[i]]._Address_Size_GoalPos_Speed
                                    );
                                nIndex1 += m_aCMot[m_lstPush[i]]._Address_Size_GoalPos_Speed;

                                if (--nCnt_Push_1 == 0) break;
                            }
                        }
                    }
#else
                    int nID = -1;
                    for (int i = 0; i < m_lstPush.Count; i++)
                    {
                        if (m_aCMot[m_lstPush[i]].nProtocol == 1)
                        {
                            if (nID < 0) nID = m_lstPush[i];

                            if (!m_aCMot[m_lstPush[i]].IsWheel)
                                m_aCMot[m_lstPush[i]].fTime = nTime;

                            buffer1[nIndex1++] = (byte)m_lstPush[i]; // ID
                            Array.Copy(m_aCMot[m_lstPush[i]].abyMap,
                                m_aCMot[m_lstPush[i]]._Address_GoalPos, // 30
                                buffer1,
                                nIndex1,
                                m_aCMot[m_lstPush[i]]._Address_Size_GoalPos // 2
                                );
                            nIndex1 += m_aCMot[m_lstPush[i]]._Address_Size_GoalPos;                            
                            Array.Copy(m_aCMot[m_lstPush[i]].abyMap,
                                m_aCMot[m_lstPush[i]]._Address_GoalPos_Speed, // 32
                                buffer1,
                                nIndex1,
                                m_aCMot[m_lstPush[i]]._Address_Size_GoalPos_Speed // 2
                                );
                            nIndex1 += m_aCMot[m_lstPush[i]]._Address_Size_GoalPos_Speed;

                            // position
                            m_aCMot[m_lstPush[i]].nPos_Prev = m_aCMot[m_lstPush[i]].nGoalPos;

                            if (--nCnt_Push_1 == 0) break;
                        }
                    }
                    for (int nSerial = 0; nSerial < m_lstConnect.Count; nSerial++)
                    {
                        Write_Sync(1, nSerial, m_aCMot[nID]._Address_GoalPos, 4, m_nCnt_Push_1, buffer1);
                    }
#endif
                }
                if (nCnt_Push_2 > 0)
                {
                    int nID = -1;
                    for (int i = 0; i < m_lstPush.Count; i++)
                    {
                        if (m_aCMot[m_lstPush[i]].nProtocol == 2)
                        {
                            if (nID < 0) nID = m_lstPush[i];

                            if (!m_aCMot[m_lstPush[i]].IsWheel)
                                m_aCMot[m_lstPush[i]].fTime = nTime;

                            buffer2[nIndex2++] = (byte)m_lstPush[i]; // ID
                            
                            Array.Copy(m_aCMot[m_lstPush[i]].abyMap,
                                m_aCMot[m_lstPush[i]]._Address_GoalPos_Speed, // 112
                                buffer2,
                                nIndex2,
                                m_aCMot[m_lstPush[i]]._Address_Size_GoalPos_Speed // 2
                                );
                            nIndex2 += m_aCMot[m_lstPush[i]]._Address_Size_GoalPos_Speed;

                            Array.Copy(m_aCMot[m_lstPush[i]].abyMap,
                                m_aCMot[m_lstPush[i]]._Address_GoalPos, // 116
                                buffer2,
                                nIndex2,
                                m_aCMot[m_lstPush[i]]._Address_Size_GoalPos // 2
                                );
                            nIndex2 += m_aCMot[m_lstPush[i]]._Address_Size_GoalPos;
                            
                            // position
                            m_aCMot[m_lstPush[i]].nPos_Prev = m_aCMot[m_lstPush[i]].nGoalPos;

                            if (--nCnt_Push_2 == 0) break;
                        }
                    }
                    for (int nSerial = 0; nSerial < m_lstConnect.Count; nSerial++)
                    {
                        Write_Sync(2, nSerial, m_aCMot[nID]._Address_GoalPos_Speed, 8, m_nCnt_Push_2, buffer2);
                    }
                }
                if (nCnt_Push_2_Wheel > 0)
                {
                    int nID = -1;
                    for (int i = 0; i < m_lstPush.Count; i++)
                    {
                        if (m_aCMot[m_lstPush[i]].nProtocol == 2)
                        {
                            if (nID < 0) nID = m_lstPush[i];
                            buffer2_Wheel[nIndex2_Wheel++] = (byte)m_lstPush[i]; // ID
                            
                            Array.Copy(m_aCMot[m_lstPush[i]].abyMap,
                                m_aCMot[m_lstPush[i]]._Address_GoalSpeed, // 32
                                buffer2_Wheel,
                                nIndex2_Wheel,
                                m_aCMot[m_lstPush[i]]._Address_Size_GoalSpeed // 2
                                );
                            nIndex2_Wheel += m_aCMot[m_lstPush[i]]._Address_Size_GoalPos_Speed;

                            if (--nCnt_Push_2_Wheel == 0) break;
                        }
                    }
                    for (int nSerial = 0; nSerial < m_lstConnect.Count; nSerial++)
                    {
                        Write_Sync(2, nSerial, m_aCMot[nID]._Address_GoalSpeed, 4, m_nCnt_Push_2_Wheel, buffer2_Wheel);
                    }
                }
                if (nCnt_Push_xl320 > 0)
                {
                    int nID = -1;
                    for (int i = 0; i < m_lstPush.Count; i++)
                    {
                        if (m_aCMot[m_lstPush[i]].IsXL320 == true)
                        {
                            if (nID < 0) nID = m_lstPush[i];

                            if (!m_aCMot[m_lstPush[i]].IsWheel)
                                m_aCMot[m_lstPush[i]].fTime = nTime;

                            buffer0[nIndex0++] = (byte)m_lstPush[i]; // ID
                            Array.Copy(m_aCMot[m_lstPush[i]].abyMap,
                                m_aCMot[m_lstPush[i]]._Address_GoalPos, // 1
                                buffer0,
                                nIndex0,
                                m_aCMot[m_lstPush[i]]._Address_Size_GoalPos // 2
                                );
                            nIndex0 += m_aCMot[m_lstPush[i]]._Address_Size_GoalPos;
                            Array.Copy(m_aCMot[m_lstPush[i]].abyMap,
                                m_aCMot[m_lstPush[i]]._Address_GoalPos_Speed, // 32
                                buffer0,
                                nIndex0,
                                m_aCMot[m_lstPush[i]]._Address_Size_GoalPos_Speed // 2
                                );
                            nIndex0 += m_aCMot[m_lstPush[i]]._Address_Size_GoalPos_Speed;

                            // position
                            m_aCMot[m_lstPush[i]].nPos_Prev = m_aCMot[m_lstPush[i]].nGoalPos;

                            if (--nCnt_Push_xl320 == 0) break;
                        }
                    }
                    for (int nSerial = 0; nSerial < m_lstConnect.Count; nSerial++)
                    {
                        Write_Sync(2, nSerial, m_aCMot[nID]._Address_GoalPos, 4, m_nCnt_Push_xl320, buffer0);
                    }
                }

                m_nCnt_Push_1 = 0;
                m_nCnt_Push_2 = 0;
                m_nCnt_Push_2_Wheel = 0;
                m_nCnt_Push_xl320 = 0;
                m_lstPush.Clear();
#if false
                // 정보수집
                List<byte> lstCnt = new List<byte>();
                

                //byte[] pbyBuffer = new byte[anIndex[nPos]];
                //List<byte> lstBuffer = new List<byte>();
                //lstBuffer.Clear();
                for (int i = 0; i < m_lstPush.Count; i++)
                {

                    for (int i = 0; i < anIndex[nPos]; i++)
                        pbyBuffer[i] = buffer[nPos, i];
                }
                Writes(m_aCMot[m_lstPush[i]].nProtocol,
                    m_aCMot[m_lstPush[i]].nCommIndex,
                    m_aCMot[m_lstPush[i]]._Address_GoalPos_Speed,
                    m_aCMot[m_lstPush[i]]._Address_Size_GoalPos_Speed,
                    anMotorCount[nPos], pbyBuffer);//buffer[j, nPos]);
                /*
                 for (int nConnection = 0; nConnection < m_lstConnect.Count; nConnection++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        int nPos = nConnection * 2 + j;                        
                        if (anIndex[nPos] > 0)
                        {
                            byte[] pbyBuffer = new byte[anIndex[nPos]];
                            for (int i = 0; i < anIndex[nPos]; i++)
                                pbyBuffer[i] = buffer[nPos, i];
                            Writes(j + 1, nConnection, anAddress[nPos], anAddress_Size[nPos], anMotorCount[nPos], pbyBuffer);//buffer[j, nPos]);
                        }
                    }
                }
                 */
#endif
            }
            public void Move(params float[] afMotors)
            {
                if (afMotors.Length < 3)
                {
                    Ojw.CMessage.Write_Error("Check Motor Parameters(Length < 3:ID, Time, Delay)");
                    return;
                }
                // ID, Angle, ID, Angle, Time, Delay
                int nDelaySpace = (((afMotors.Length % 2) == 0) ? 1 : 0);
                int nPos_Time = afMotors.Length - 1 - nDelaySpace;
                int nCount = (nPos_Time / 2);
                int nPos_Delay = ((nDelaySpace > 0) ? afMotors.Length - 1 : -1);
                int nTime = (int)afMotors[nPos_Time];
                int nDelay = ((nDelaySpace > 0) ? (int)afMotors[nPos_Delay] : nTime);
                for (int i = 0; i < nCount; i++)
                {
                    Set((int)afMotors[i * 2], afMotors[i * 2 + 1]);
                }
                Send_Motor(nTime);
                if (nDelay > 0) Wait(nTime);
            }
            public List<int> m_lstMotors = new List<int>();
            public void ClearMotor()
            {
                m_lstMotors.Clear();
                Array.Clear(m_aCMot, 0, m_aCMot.Length);
            }
            #region GetParam
            public void GetParam_by_HwNum(EMonster_t EMon, out float fCenter, out float fMechEvd, out float fMechAngle, out float fJointRpm, out int nProtocolVer)
            {
                GetParam_by_HwNum((int)EMon, out fCenter, out fMechEvd, out fMechAngle, out fJointRpm, out nProtocolVer);
            }
            public void GetParam_by_HwNum(int nHwNum, out float fCenter, out float fMechEvd, out float fMechAngle, out float fJointRpm, out int nProtocolVer)
            {
                fCenter = 2048.0f; fMechEvd = 4096.0f; fMechAngle = 360.0f; fJointRpm = 0.229f; nProtocolVer = 0;
                switch ((EMonster_t)nHwNum)
                {
                    case EMonster_t.AX_12: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMonster_t.AX_12W:
                    case EMonster_t.AX_18:
                    case EMonster_t.DX_113:
                    case EMonster_t.DX_116:
                    case EMonster_t.DX_117:

                    case EMonster_t.RX_10: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMonster_t.RX_24F: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMonster_t.RX_28: // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMonster_t.RX_64: // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)

                    case EMonster_t.XL_320: fCenter = 512.0f; fMechEvd = 1024.0f; fMechAngle = 300.0f; fJointRpm = 0.111f; nProtocolVer = ((EMonster_t)nHwNum == EMonster_t.XL_320) ? 2 : 1; break;

                    case EMonster_t.EX_106: // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 
                    case EMonster_t.EX_106P: fCenter = 2048.0f; fMechEvd = 4096.0f; fMechAngle = 251.0f; fJointRpm = 0.111f; nProtocolVer = 1; break;

                    // protocol 2.0
                    case EMonster_t.MX_12: // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.916rpm, 다중회전가능(각 7바퀴:-28,672~28,672)
                        fCenter = 2048.0f; fMechEvd = 4096.0f; fMechAngle = 360.0f; fJointRpm = 0.916f; nProtocolVer = 2; break;
                    case EMonster_t.MX_28: // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                    case EMonster_t.MX_28_2: // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                    case EMonster_t.MX_64: // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                    case EMonster_t.MX_106: // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                        fCenter = 2048.0f; fMechEvd = 4096.0f; fMechAngle = 360.0f; fJointRpm = 0.114f; nProtocolVer = 2; break;
                    //MX_ = 17, //

                    case EMonster_t.XH_430_W210:
                    case EMonster_t.XH_430_W350:
                    case EMonster_t.XH_430_V210:
                    case EMonster_t.XH_430_V350:

                    case EMonster_t.XM_540_W150:
                    case EMonster_t.XM_540_W270:
                    case EMonster_t.XM_430_W210:
                    case EMonster_t.XM_430_W350:
                    case EMonster_t.XL_430: // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
                    case EMonster_t.XL_2XL: fCenter = 2048.0f; fMechEvd = 4096.0f; fMechAngle = 360.0f; fJointRpm = 0.229f; nProtocolVer = 2; break;
                }
            }
            public CMot GetParam(int nID) { return m_aCMot[nID]; }
            public CMot[] GetParam() { return m_aCMot; }

            public int   GetParam_RealID(int nID, int nRealID) { return m_aCMot[nID].nRealID; }
            public int   GetParam_Dir(int nID, int nDirection) { return m_aCMot[nID].nDir; }
            public int   GetParam_MirrorID(int nID, int nMirrorID) { return m_aCMot[nID].nMirrorID; }
            public int   GetParam_Protocol(int nID, int nProtocol) { return m_aCMot[nID].nProtocol; }
            public int   GetParam_SerialPort(int nID, int nCommIndex) { return m_aCMot[nID].nCommIndex; }
            public float GetParam_fJointRpm(int nID, float fJointRpm) { return m_aCMot[nID].fJointRpm; }

            public float GetParam_CenterPos(int nID, float fCenterPos) { return m_aCMot[nID].fCenterPos; }
            public float GetParam_MechMove(int nID, float fMechMove) { return m_aCMot[nID].fMechMove; }
            public float GetParam_MechAngle(int nID, float fMechAngle) { return m_aCMot[nID].fMechAngle; }
            public void GetParam_GearRatio(int nID, out float fCenterPos, out float fMechMove, out float fMechAngle)
            {
                fCenterPos = m_aCMot[nID].fCenterPos;
                fMechMove = m_aCMot[nID].fMechMove;
                fMechAngle = m_aCMot[nID].fMechAngle;
            }
            #endregion GetParam
            #region SetParam
            public void SetParam(int nID, int nRealID, EMonster_t EMon, int nSerialIndex)
            {
                SetParam(nID, nID, 0, -1, EMon, GetProtocol_by_HwNum(EMon), nSerialIndex);
            }
            public void SetParam(int nID, EMonster_t EMon, int nSerialIndex)
            {
                SetParam(nID, nID, 0, -1, EMon, GetProtocol_by_HwNum(EMon), nSerialIndex);
            }
            public void SetParam(int nID, int nRealID, EMonster_t EMon)
            {
                SetParam(nID, nID, 0, -1, EMon, GetProtocol_by_HwNum(EMon), m_aCMot[nID].nCommIndex);
            }
            public void SetParam(int nID, EMonster_t EMon)
            {
                SetParam(nID, nID, 0, -1, EMon, GetProtocol_by_HwNum(EMon), m_aCMot[nID].nCommIndex);
            }

            public void SetParam_RealID(int nID, int nRealID) { m_aCMot[nID].nRealID = nRealID; }
            public void SetParam_Dir(int nID, int nDirection) { m_aCMot[nID].nDir = nDirection; }
            public void SetParam_MirrorID(int nID, int nMirrorID) { m_aCMot[nID].nMirrorID = nMirrorID; }
            public void SetParam_Protocol(int nID, int nProtocol) { m_aCMot[nID].nProtocol = nProtocol; }
            public void SetParam_SerialPort(int nID, int nCommIndex) { m_aCMot[nID].nCommIndex = nCommIndex; }
            public void SetParam_fJointRpm(int nID, float fJointRpm) { m_aCMot[nID].fJointRpm = fJointRpm; }

            public void SetParam_CenterPos(int nID, float fCenterPos)   { m_aCMot[nID].fCenterPos   = fCenterPos;   }
            public void SetParam_MechMove(int nID, float fMechMove)     { m_aCMot[nID].fMechMove    = fMechMove;    }
            public void SetParam_MechAngle(int nID, float fMechAngle)   { m_aCMot[nID].fMechAngle   = fMechAngle;   }
            public void SetParam_GearRatio(int nID, float fCenterPos, float fMechMove, float fMechAngle)
            {
                m_aCMot[nID].fCenterPos = fCenterPos;
                m_aCMot[nID].fMechMove = fMechMove;
                m_aCMot[nID].fMechAngle = fMechAngle;
            }
            public void SetParam(int nID, int nRealID, int nDir, int nMirrorID, EMonster_t EMon, int nProtocol, int nCommIndex)
            {
                float fCenter, fMechMove, fMechAngle, fJointRpm;
                int nProtocolVer;
                GetParam_by_HwNum(EMon, out fCenter, out fMechMove, out fMechAngle, out fJointRpm, out nProtocolVer);
                if (nProtocol == 0) nProtocol = nProtocolVer;
                // 나중에 MX 인 경우 nProtocolVer 이 바꿀 수 있도록 한다.
                SetParam(nID, nRealID, nDir, nMirrorID, nProtocol, nCommIndex, fMechMove, fMechAngle, fCenter, fJointRpm, EMon);            
            }
            public void SetParam(int nID, int nRealID, int nDir, int nMirrorID, int nProtocol, int nCommIndex, float fMechMove, float fMechAngle, float fCenterPos, float fJointRpm, EMonster_t EMon)
            {
                m_aCMot[nID].nCommIndex = nCommIndex;
                m_aCMot[nID].nProtocol = nProtocol;
                m_aCMot[nID].nHwNum = (int)EMon;
                m_aCMot[nID].nRealID = nRealID;
                m_aCMot[nID].nDir = nDir; // 0 : Forward, 1 : Inverse
                m_aCMot[nID].nMirrorID = nMirrorID; 

                m_aCMot[nID].fCenterPos = fCenterPos;
                m_aCMot[nID].fMechMove = fMechMove;
                m_aCMot[nID].fMechAngle = fMechAngle;
                m_aCMot[nID].fJointRpm = fJointRpm;
                if (m_aCMot[nID].bEnable == false)
                {
                    m_aCMot[nID].bEnable = true;
                    if (m_lstMotors.Contains(nID) == false)
                        m_lstMotors.Add(nID);
                }
            }
            #endregion SetParam
#if true
            public void AutoSet() { AutoSet(false, false); }
            public void AutoSet(bool bDisplayMessage, bool bDisplayReceivedDatas)
            {
                System.Windows.Forms.Cursor csr = Ojw.CMouse.GetCursor();
                Ojw.CMouse.SetCursor(System.Windows.Forms.Cursors.WaitCursor);

                bool bDisp = false;
                for (int nSerialIndex = 0; nSerialIndex < m_lstConnect.Count; nSerialIndex++)
                {
                    for (int nProtocol = 1; nProtocol <= 2; nProtocol++)
                    {
                        if (bDisplayReceivedDatas)
                            if ((nSerialIndex + nProtocol) == (2 + m_lstConnect.Count - 1))
                                bDisp = bDisplayReceivedDatas;
                        AutoSet(nProtocol, nSerialIndex, bDisplayMessage, bDisp);
                    }
                }
                Ojw.CMouse.SetCursor(csr);
            }
            public void AutoSet(int nProtocolVersion, int nSerialIndex, bool bDisplayMessage, bool bDisplayReceivedDatas)
            {   
                List<Ojw.CMonster2.CMonsterPacket_t> lstCPacket = new List<Ojw.CMonster2.CMonsterPacket_t>();
                for (int nRetrieve = 0; nRetrieve < 3; nRetrieve++)
                {
                    m_CMonsterTmp = Write_Ping(nProtocolVersion, nSerialIndex, 254);
                    if (m_CMonsterTmp != null)
                    {
                        lstCPacket.AddRange(m_CMonsterTmp);
                        break;
                    }
                    else CMessage.Write("[Warning][AutoSet] Retrieve = {0}", nRetrieve + 1);
                }

                if (bDisplayMessage)
                {
                    Ojw.printf("==========\r\n");
                    for (int i = 0; i < lstCPacket.Count; i++)
                    {
                        if (lstCPacket[i].bOk == true)
                        {
                            Ojw.printf("ID={0}[Protocol {1}, Error={2}],Header={3},Cmd={4},Length={5},DataLength={6}",
                                lstCPacket[i].nID,
                                lstCPacket[i].nProtocol,
                                lstCPacket[i].nError,
                                lstCPacket[i].nHeader,
                                lstCPacket[i].nCommand,
                                lstCPacket[i].nLength,
                                lstCPacket[i].nDataLength
                                );
                            //Ojw.printf("[HW={0}, FW={1}, Data=", Ojw.CConvert.BytesToShort(lstCPacket[i].abyData, 0), lstCPacket[i].abyData[2]);
                            Ojw.printf("[HW={0}, FW={1}", Ojw.CConvert.BytesToShort(lstCPacket[i].abyData, 0), lstCPacket[i].abyData[2]);
                            /*for (int j = 0; j < lstCPacket[i].nDataLength; j++)
                            {
                                 Ojw.printf("{0} ", lstCPacket[i].abyData[j]);
                             }*/
                            Ojw.newline();
                        }
                    }
                }
                for (int i = 0; i < m_lstMotors.Count; i++)
                {
                    Request(m_lstMotors[i], 0, (m_aCMot[m_lstMotors[i]].nProtocol == 1 ? 60 : 135), bDisplayReceivedDatas);
                    SetParam(m_lstMotors[i], m_lstMotors[i], m_aCMot[m_lstMotors[i]].nMirrorID, m_aCMot[m_lstMotors[i]].nDir, (EMonster_t)(m_aCMot[m_lstMotors[i]].nHwNum), m_aCMot[m_lstMotors[i]].nProtocol, m_aCMot[m_lstMotors[i]].nCommIndex);
                }
            }
#else
            public void AutoSet(int nProtocol_Version) { AutoSet(nProtocol_Version, false); }
            public void AutoSet(int nProtocol_Version, bool bDisplayMessage)
            {
                //List<CMonsterPacket_t> lstCPacket = new List<CMonsterPacket_t>();
                //lstCPacket.Clear();

                Ojw.CTimer CTmr = new CTimer();
                byte[] abyTmp;
                byte[] pbyteBuffer;
                int nTimeout = 40;// 1000;// 20;
                CMonsterPacket_t[] aCMonsterPacket = null;
                for (int nSerialIndex = 0; nSerialIndex < m_lstConnect.Count; nSerialIndex++)
                {
                    pbyteBuffer = MakePingPacket(254, nProtocol_Version);
                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);

                    CTmr.Set();
                    while (true)
                    {
                        abyTmp = Receive(nSerialIndex);
                        if (abyTmp != null)
                        {
#if true
                            aCMonsterPacket = PacketToClass(nProtocol_Version, abyTmp);
                            if (aCMonsterPacket != null)
                            {
                                for (int i = 0; i < aCMonsterPacket.Length; i++)
                                {
                                    //m_aCMot[aCMonsterPacket[i].nID].nRealID = aCMonsterPacket[i].nID;
                                    m_aCMot[aCMonsterPacket[i].nID].abyMap[0] = aCMonsterPacket[i].abyData[0]; // HwNum
                                    m_aCMot[aCMonsterPacket[i].nID].abyMap[1] = aCMonsterPacket[i].abyData[1];
                                    m_aCMot[aCMonsterPacket[i].nID].abyMap[6] = aCMonsterPacket[i].abyData[2]; // Firmware Version
                                    m_aCMot[aCMonsterPacket[i].nID].nCommIndex = nSerialIndex;
                                    m_aCMot[aCMonsterPacket[i].nID].nProtocol = nProtocol_Version;
                                    SetParam(aCMonsterPacket[i].nID, (EMonster_t)m_aCMot[aCMonsterPacket[i].nID].nHwNum);

                                    //if (Request(aCMonsterPacket[i].nID, 0, (nProtocol_Version == 1) ? 60 : 135, bDisplayMessage) == true)
                                    //{
                                        //if (bDisplayMessage)
                                            //CMessage.Write("Received Data => ID={0} Done", aCMonsterPacket[i].nID);
                                    //}
                                }
                                //if (aCMonsterPacket.Length > 0) break;
                            }
#else
                            lstCPacket.AddRange(PacketToClass(nProtocol_Version, abyTmp));
                            if (lstCPacket.Count > 0)
                            {
                                for (int i = 0; i < lstCPacket.Count; i++)
                                {
                                    if (Request(lstCPacket[i].nID, 0, (nProtocol_Version == 1) ? 60 : 135, bDisplayMessage) == true)
                                    {
                                        if (bDisplayMessage)
                                            CMessage.Write("Received Data => ID={0} Done", lstCPacket[i].nID);
                                    }
                                }
                                //if (lstCPacket.Count > 0) break;
                            }
#endif
                            CTmr.Set();
                        }
                        else
                        {
                            //if (lstCPacket.Count > 0) break;
                            if (aCMonsterPacket != null)
                                if (aCMonsterPacket.Length > 0) break;
                            //if (bGet == true)
                            //    break;
                            if (CTmr.Get() >= nTimeout)//((CTmr.Get() >= nTimeout) && (bGet == true))
                            {
                                break; // TimeOver
                            }
                        }
                    }
#if true
                    //if (aCMonsterPacket.Length > 0)
                    //{
                    //    for (int i = 0; i < aCMonsterPacket.Length; i++)
                    //    {
                    //        int nMotID = aCMonsterPacket[i].nID;

                    //        m_aCMot[nMotID].abyMap[0] = aCMonsterPacket[i].abyData[0]; // HwNum
                    //        m_aCMot[nMotID].abyMap[1] = aCMonsterPacket[i].abyData[1];
                    //        m_aCMot[nMotID].abyMap[6] = aCMonsterPacket[i].abyData[2]; // Firmware Version
                    //        m_aCMot[nMotID].nCommIndex = nSerialIndex;
                    //        m_aCMot[nMotID].nProtocol = nProtocol_Version;
                    //        SetParam(nMotID, (EMonster_t)m_aCMot[nMotID].nHwNum);
                    //    }
                    //}
#else
                     if (lstCPacket.Count > 0)
                    {
                        for (int i = 0; i < lstCPacket.Count; i++)
                        {
                            int nMotID = lstCPacket[i].nID;

                            m_aCMot[nMotID].abyMap[0] = lstCPacket[i].abyData[0]; // HwNum
                            m_aCMot[nMotID].abyMap[1] = lstCPacket[i].abyData[1];
                            m_aCMot[nMotID].abyMap[6] = lstCPacket[i].abyData[2]; // Firmware Version
                            m_aCMot[nMotID].nCommIndex = nSerialIndex;
                            m_aCMot[nMotID].nProtocol = nProtocol_Version;
                            SetParam(nMotID, (EMonster_t)m_aCMot[nMotID].nHwNum);
                        }
                    }
#endif

                }

                Request(bDisplayMessage);
                

                pbyteBuffer = null;                
            }
#endif
            private int m_nTimeOut_For_Autoset = 1500;
            private CMonsterPacket_t[] m_CMonsterTmp = null;
            public CMonsterPacket_t[] Write_Ping(int nProtocol_Version, int nSerialIndex, int nID)
            {
                List<CMonsterPacket_t> lstCPacket = new List<CMonsterPacket_t>();
                lstCPacket.Clear();
                int nTimeout = m_nTimeOut_For_Autoset;
                if (nID == 254) nTimeout = 1500;

                CMonsterPacket_t CPacket = new CMonsterPacket_t();
                CPacket.Clear();

                byte[] pbyteBuffer = MakePingPacket(nID, nProtocol_Version);
                SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                pbyteBuffer = null;

                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();
                //bool bGet = false;
                while (true)
                {
                    byte[] abyTmp = Receive(nSerialIndex);
                    if (abyTmp != null)
                    {
                        //lstCPacket.AddRange(PacketToClass(nProtocol_Version, abyTmp));                        
                        m_CMonsterTmp = PacketToClass(nProtocol_Version, abyTmp);
                        if (m_CMonsterTmp == null)
                        {
                            return null;
                        }
                        else lstCPacket.AddRange(m_CMonsterTmp);
                        CTmr.Set();
                        //bGet = true;
                    }
                    else
                    {
                        if (lstCPacket.Count > 0) break;
                        //if (bGet == true)
                        //    break;
                        if (CTmr.Get() >= nTimeout)//((CTmr.Get() >= nTimeout) && (bGet == true))
                        {
                            break; // TimeOver
                        }
                    }
                }

                if (lstCPacket.Count > 0)
                {
                    for (int i = 0; i < lstCPacket.Count; i++)
                    {
                        int nMotID = lstCPacket[i].nID;
                        
                        m_aCMot[nMotID].abyMap[0] = lstCPacket[i].abyData[0]; // HwNum
                        m_aCMot[nMotID].abyMap[1] = lstCPacket[i].abyData[1]; 
                        m_aCMot[nMotID].abyMap[6] = lstCPacket[i].abyData[2]; // Firmware Version
                        m_aCMot[nMotID].nProtocol = nProtocol_Version;
                        m_aCMot[nMotID].nCommIndex = nSerialIndex;

                        EMonster_t EHwNum = ((((EMonster_t)m_aCMot[nMotID].nHwNum) == EMonster_t.NONE) ? (nProtocol_Version == 1 ? EMonster_t.AX_12 : EMonster_t.XL_430) : ((EMonster_t)m_aCMot[nMotID].nHwNum));
                        SetParam(nMotID, EHwNum);
                    }
                }
                return lstCPacket.ToArray();
            }
            public CMonsterPacket_t[] GetRecieved(int nProtocol_Version, int nSerialIndex, int nTimeout = 1000)
            {
                List<CMonsterPacket_t> lstCPacket = new List<CMonsterPacket_t>();
                lstCPacket.Clear();
                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();
                while (true)
                {
                    byte[] abyTmp = Receive(nSerialIndex);
                    if (abyTmp != null)
                    {
                        //lstCPacket.AddRange(PacketToClass(nProtocol_Version, abyTmp));                        
                        m_CMonsterTmp = PacketToClass(nProtocol_Version, abyTmp);
                        if (m_CMonsterTmp == null)
                        {
                            return null;
                        }
                        else lstCPacket.AddRange(m_CMonsterTmp);
                        CTmr.Set();
                        //bGet = true;
                    }
                    else
                    {
                        //if (lstCPacket.Count > 0) break;
                        //if (bGet == true)
                        //    break;
                        if (CTmr.Get() >= nTimeout)//((CTmr.Get() >= nTimeout) && (bGet == true))
                        {
                            break; // TimeOver
                        }
                    }
                }
                return lstCPacket.ToArray();
            }

            public CMonsterPacket_t[] Write_Bulk(int nProtocol_Version, int nSerialIndex, int nID, params byte [] pBytes)
            {
                List<CMonsterPacket_t> lstCPacket = new List<CMonsterPacket_t>();
                lstCPacket.Clear();
                int nTimeout = m_nTimeOut_For_Autoset;
                if (nID == 254) nTimeout = 1500;

                CMonsterPacket_t CPacket = new CMonsterPacket_t();
                CPacket.Clear();

                byte[] pbyteBuffer = MakePingPacket(nID, nProtocol_Version);
                SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                pbyteBuffer = null;

                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();
                //bool bGet = false;
                while (true)
                {
                    byte[] abyTmp = Receive(nSerialIndex);
                    if (abyTmp != null)
                    {
                        //lstCPacket.AddRange(PacketToClass(nProtocol_Version, abyTmp));                        
                        m_CMonsterTmp = PacketToClass(nProtocol_Version, abyTmp);
                        if (m_CMonsterTmp == null)
                        {
                            return null;
                        }
                        else lstCPacket.AddRange(m_CMonsterTmp);
                        CTmr.Set();
                        //bGet = true;
                    }
                    else
                    {
                        if (lstCPacket.Count > 0) break;
                        //if (bGet == true)
                        //    break;
                        if (CTmr.Get() >= nTimeout)//((CTmr.Get() >= nTimeout) && (bGet == true))
                        {
                            break; // TimeOver
                        }
                    }
                }

                if (lstCPacket.Count > 0)
                {
                    for (int i = 0; i < lstCPacket.Count; i++)
                    {
                        int nMotID = lstCPacket[i].nID;

                        m_aCMot[nMotID].abyMap[0] = lstCPacket[i].abyData[0]; // HwNum
                        m_aCMot[nMotID].abyMap[1] = lstCPacket[i].abyData[1];
                        m_aCMot[nMotID].abyMap[6] = lstCPacket[i].abyData[2]; // Firmware Version
                        m_aCMot[nMotID].nProtocol = nProtocol_Version;
                        m_aCMot[nMotID].nCommIndex = nSerialIndex;

                        EMonster_t EHwNum = ((((EMonster_t)m_aCMot[nMotID].nHwNum) == EMonster_t.NONE) ? (nProtocol_Version == 1 ? EMonster_t.AX_12 : EMonster_t.XL_430) : ((EMonster_t)m_aCMot[nMotID].nHwNum));
                        SetParam(nMotID, EHwNum);
                    }
                }
                return lstCPacket.ToArray();
            }

            public byte[] Receive(int nSerialIndex)
            {
                if (nSerialIndex < m_lstConnect.Count)
                {
                    Ojw.CTimer CTmr = new CTimer();
                    CTmr.Set();
                    while (true)
                    {
                        if (m_lstConnect[nSerialIndex].CSerial.GetBuffer_Length() > 0)
                        {
                            return m_lstConnect[nSerialIndex].CSerial.GetBytes();
                        }
                        if (CTmr.Get() >= m_nTimeOut_For_Autoset) break;
                    }
                }
                return null;
            }
            private byte[] MakePingPacket(int nMotorRealID, int nProtocol_Version)
            {
                int i = 0;
                byte[] pbyteBuffer;
                if (nProtocol_Version == 1)
                {
                    int nLength = 2; // 파라미터의 갯수 + 2
                    int nDefaultSize = 4;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(0x01);

                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);
                }
                else // 0, 2 - protocol2, -2 - xl320
                {
                    int nLength = 3;
                    int nDefaultSize = 7;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    #endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(0x01);

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    //SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                return pbyteBuffer;
            }
            public void Request(bool bDisplayMessage)
            {
                for (int i = 0; i < m_lstMotors.Count; i++) 
                {
                    Request(m_lstMotors[i], 0, (m_aCMot[m_lstMotors[i]].IsOldAddress == true) ? 60 : 135, bDisplayMessage); 
                }
            }
            public void Request(int nAddress, int nLength, bool bDisplayMessage) { for (int i = 0; i < m_lstMotors.Count; i++) { Request(m_lstMotors[i], nAddress, nLength, bDisplayMessage); } }
            //public bool Request(int nMotor, int nAddress, int nLength)
            //{
                //return Request(nMotor, nAddress, nLength, false);
            //}
            public byte [] Request(int nMotor, int nAddress, int nLength)
            {
                if (Request(nMotor, nAddress, nLength, false) == true)
                    return m_aCMot[nMotor].abyMap;
                return null;
            }
            public bool Request(int nMotor, int nAddress, int nLength, bool bDisplayMessage)
            {
                bool bRet = false;// true;
                for (int nRetrieve = 0; nRetrieve < 3; nRetrieve++)
                {
                    if (m_aCMot[nMotor].nProtocol == 1)
                    {
                        Send(nMotor, 0x02, nAddress, (byte)nLength);
                    }
                    else
                    {
                        Send(nMotor, 0x02, nAddress, Ojw.CConvert.ShortToBytes((short)nLength));
                    }
                    Ojw.CTimer CTmr = new CTimer();
                    CTmr.Set();
                    byte[] abyTmp;
                    while (true)
                    {
                        abyTmp = Receive(m_aCMot[nMotor].nCommIndex);
                        if (abyTmp != null)
                        {
                            if (PacketToMap(m_aCMot[nMotor].nProtocol, nAddress, abyTmp) == true)
                            {
                                bRet = true;
                                break;
                            }
                            else
                                CTmr.Set();
                        }
                        else
                        {
                            if (CTmr.Get() >= m_nTimeOut_For_Autoset)//3000)
                            {
                                //bRet = false;
                                break; // TimeOver
                            }
                        }
                    }
                    if (bRet == true)
                    {
                        m_aCMot[nMotor].nSeq_Back++;
                        if (bDisplayMessage == true)//(m_lstMotors.Count > 0)
                        {
                            CMessage.Write("ID={0}[Hw[{1}:{2}],Fw[{3}], Pos={4}({5})",
                                m_aCMot[nMotor].nRealID,
                                dicMonster[m_aCMot[nMotor].nHwNum],
                                m_aCMot[nMotor].nHwNum,
                                m_aCMot[nMotor].nFirmwareVer,
                                m_aCMot[nMotor].nPos,
                                m_aCMot[nMotor].fAngle
                                );
                        }
                        //else bRet = false;
                        break;
                    }
                }
                return bRet;// lstCPacket.ToArray();     
            }
            private void Request_with_RealID(int nProtocol_Version, int nSerialIndex, int nMotorRealID, int nCommand, byte [] pbyDatas)
            {
                int i = 0;
                int nDataLength = 0;
                if (pbyDatas != null)
                {
                    if (pbyDatas.Length > 0)
                        nDataLength = pbyDatas.Length; 
                }
                byte [] pbyteBuffer;

                bool bSend = false;
                if (nProtocol_Version == 1)
                {  
                    int nLength = 2;
                    nLength += ((nDataLength > 0) ? nDataLength + 0 : 0);

                    int nDefaultSize = 4;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    if (nDataLength > 0)
                    {
                        for (int j = 0; j < nDataLength; j++)
                            pbyteBuffer[i++] = pbyDatas[j];
                    }
                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    bSend = true;
                }
                else if (nProtocol_Version == 1.5) // XL-320
                {
                    // -> 오류일 가능성 있다. 검증 안해봄
                    int nLength = 3;
                        nLength += ((nDataLength > 0) ? nDataLength + 0 : 0);

                    int nDefaultSize = 7;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    //region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    //endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    // pbyteBuffer[i++] = (nAddress & 0xff);
                    // pbyteBuffer[i++] = ((nAddress >> 8) & 0xff);
                    if (nDataLength > 0)
                        for (int j = 0; j < nDataLength; j++)
                            pbyteBuffer[i++] = pbyDatas[j];

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    bSend = true;
                }
                else
                {
                    int nLength = 3;
                    nLength += ((nDataLength > 0) ? nDataLength + 0 : 0);
                    int nDefaultSize = 7;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    // region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    // endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    if (nDataLength > 0)
                    {
                        // pbyteBuffer[i++] = (nAddress & 0xff);
                        // pbyteBuffer[i++] = ((nAddress >> 8) & 0xff);
                
                        for (int j = 0; j < nDataLength; j++)
                            pbyteBuffer[i++] = pbyDatas[j];
                    }
                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);
                    bSend = true;
                }
                if (bSend)
                {     
                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            // 기존 라이브러리와의 호환을 위해...
            public void Write_Command(int nMotor, int nCommand) { Send_Command(nMotor, nCommand); }
            public void Send_Command(int nMotor, int nCommand)
            {
                Send(nMotor, nCommand, 0, null);
            }

            #region Reg / Action
            public void Action_Set(int nProtocol_Version, int nSerialIndex, int nMotorReallID, int nAddress, params byte[] pbyDatas)
            {
                Write2(nProtocol_Version, nSerialIndex, nMotorReallID, 0x04, nAddress, pbyDatas);
            }
            #endregion Reg / Action

            public void Write(int nMotor, int nCommand, int nAddress, params byte[] pbyDatas) { Send(nMotor, nCommand, nAddress, pbyDatas); }
            private void Send(int nMotor, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                if (nMotor == 254)
                {
                    if (pbyDatas != null)
                    {
                        for (int i = 0; i < m_lstMotors.Count; i++)
                        {
                            Array.Copy(pbyDatas, 0, m_aCMot[m_lstMotors[i]].abyMap, nAddress, pbyDatas.Length);
                        }
                    }
                    for (int nCommIndex = 0; nCommIndex < m_lstConnect.Count; nCommIndex++)
                        for (int nProtocol = 1; nProtocol <= 2; nProtocol++)
                            Send_with_ReallID(nProtocol, nCommIndex, 254, nCommand, nAddress, pbyDatas);
                }
                else
                    Send_with_ReallID(m_aCMot[nMotor].nProtocol, m_aCMot[nMotor].nCommIndex, m_aCMot[nMotor].nRealID, nCommand, nAddress, pbyDatas);
            }
            // 기존 라이브러리와의 호환을 위해...
            private void Write2(int nProtocol_Version, int nSerialIndex, int nMotorRealID, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                Send_with_ReallID(nProtocol_Version, nSerialIndex, nMotorRealID, nCommand, nAddress, pbyDatas);
            }

            public void Send_RawData(int nProtocol_Version, int nSerialIndex, int nMotorRealID, int nCommand, params byte[] pbyDatas)
            {
                int i;
                i = 0;
                //int nBufferLength = (pbyDatas == null) ? 0 : pbyDatas.Length;
                if (nProtocol_Version == 1)
                {
                    int nLength = 2 + ((pbyDatas != null) ? pbyDatas.Length + 1 : 0);
                    int nDefaultSize = 4;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    if (pbyDatas != null)
                    {
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;
                    }
                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                //else if (nProtocol_Version == 0) // XL-320
                //{
                //    int nLength = 3 + 2 + pbyDatas.Length;
                //    int nDefaultSize = 7;
                //    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                //    pbyteBuffer[i++] = 0xff;
                //    pbyteBuffer[i++] = 0xff;
                //    #region Packet 2.0
                //    pbyteBuffer[i++] = 0xfd;
                //    pbyteBuffer[i++] = 0x00;
                //    #endregion Packet 2.0
                //    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                //    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                //    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                //    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                //    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                //    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                //    if (pbyDatas != null)
                //        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;

                //    MakeStuff(ref pbyteBuffer);
                //    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                //    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                //    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                //    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                //}
                else // m_aCParam_Axis[nAxis].nProtocol_Version == 2
                {
                    int nLength = 3 + ((pbyDatas != null) ? pbyDatas.Length + 2 : 0);
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    #endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    if (pbyDatas != null)
                    {
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;
                    }
                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            private void Send_with_ReallID(int nProtocol_Version, int nSerialIndex, int nMotorRealID, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                int i;
                i = 0;
                //int nBufferLength = (pbyDatas == null) ? 0 : pbyDatas.Length;
                if (nProtocol_Version == 1)
                {
                    int nLength = 2 + ((pbyDatas != null) ? pbyDatas.Length + 1 : 0);
                    int nDefaultSize = 4;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
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
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                //else if (nProtocol_Version == 0) // XL-320
                //{
                //    int nLength = 3 + 2 + pbyDatas.Length;
                //    int nDefaultSize = 7;
                //    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                //    pbyteBuffer[i++] = 0xff;
                //    pbyteBuffer[i++] = 0xff;
                //    #region Packet 2.0
                //    pbyteBuffer[i++] = 0xfd;
                //    pbyteBuffer[i++] = 0x00;
                //    #endregion Packet 2.0
                //    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                //    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                //    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                //    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                //    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                //    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                //    if (pbyDatas != null)
                //        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;

                //    MakeStuff(ref pbyteBuffer);
                //    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                //    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                //    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                //    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                //}
                else // m_aCParam_Axis[nAxis].nProtocol_Version == 2
                {
                    int nLength = 3 + ((pbyDatas != null) ? pbyDatas.Length + 2 : 0);
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    #endregion Packet 2.0
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

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            //public void Read_Motor(int nProtocol, int nMotor, int nAddress, int nLength)
            //{
            //    if (nProtocol == 1)
            //    {
            //        Send_with_ReallID(nProtocol, m_aCMot[nMotor].nCommIndex, m_aCMot[nMotor].nRealID, 0x02, nAddress, (byte)(nLength & 0xff));
            //    }
            //    else
            //    {
            //        Send_with_ReallID(nProtocol, m_aCMot[nMotor].nCommIndex, m_aCMot[nMotor].nRealID, 0x02, nAddress, Ojw.CConvert.ShortToBytes((short)nLength));
            //    }
            //    //Received(m_aCParam[nMotor].nCommIndex, 200, 1);
            //}
            #region Protocol - basic(updateCRC, MakeStuff, SendPacket)
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
            private void MakeStuff(ref byte[] pBuff)
            {
                int nStuff = 0;
                int[] pnIndex = new int[pBuff.Length];
                Array.Clear(pnIndex, 0, pnIndex.Length);
                int nCnt = 0;
                // (0)0xff, (1)0xff, (2)0xfd, (3)0x00,     
                // (4)ID != 0xff 이니 검사할 필요 없다.
                for (int i = 5; i < pBuff.Length; i++)
                {
                    switch (nStuff)
                    {
                        case 0: { if (pBuff[i] == 0xff) nStuff++; } break;
                        case 1: { if (pBuff[i] == 0xff) nStuff++; else nStuff = 0; } break;
                        case 2: { nStuff = 0; if (pBuff[i] == 0xfd) { pnIndex[nCnt++] = i; } } break;
                    }
                }
                if (nCnt > 0)
                {
                    byte[] pBuff2 = (byte[])pBuff.Clone();
                    Array.Resize<byte>(ref pBuff, pBuff2.Length + nCnt);
                    int nIndex = 0;
                    int nPos = 0;
                    int i = 5;
                    // 내부의 패킷길이값 재 설정 
                    pBuff[i++] = (byte)((pBuff.Length - 7) & 0xff);
                    pBuff[i++] = (byte)(((pBuff.Length - 7) >> 8) & 0xff);
                    for (i = 7; i < pBuff.Length; i++)
                    {
                        pBuff[i + nPos] = pBuff2[i];
                        if (i == pnIndex[nPos])
                        {
                            pBuff[nIndex + nPos + 1] = 0xfd;
                            nPos++;
                        }
                    }
                    pBuff2 = null;
                }
                pnIndex = null;
            }
            public void SendPacket(int nIndex_Connection, byte[] buffer, int nLength)
            {
                if (m_lstConnect.Count > 0)
                {
                    // Serial
                    if (m_lstConnect[nIndex_Connection].nType == 1)
                    {
                        if (m_lstConnect[nIndex_Connection].CSerial.IsConnect() == true) m_lstConnect[nIndex_Connection].CSerial.SendPacket(buffer, nLength);
                    }
                    // Socket
                    else if (m_lstConnect[nIndex_Connection].nType == 2)
                    {
                        if (m_lstConnect[nIndex_Connection].CSock.IsConnect() == true) m_lstConnect[nIndex_Connection].CSock.SendPacket(buffer, nLength);
                    }
                    else
                    {
                    }
                }
            }
            #endregion Protocol - basic(updateCRC, MakeStuff, SendPacket)


            #region Reboot / Reset
            public void SetTorq(bool bOn)
            {
                SetTorq(254, bOn);
            }
            public void SetTorq(int nID, bool bOn)
            {
                if (nID == 254)
                {
                    for (int i = 0; i < m_lstMotors.Count; i++)
                    {
                        m_aCMot[m_lstMotors[i]].nTorq = Ojw.CConvert.BoolToInt(bOn);
                    }
                }
                else
                {
                    m_aCMot[nID].nTorq = Ojw.CConvert.BoolToInt(bOn);
                }
                Send(nID, 0x03, m_aCMot[nID]._Address_Torq, (byte)((bOn == true) ? 1 : 0));
            }

            public void SetLed(int nID, int nValue)
            {
                if (nID == 254)
                {
                    for (int i = 0; i < m_lstMotors.Count; i++)
                    {
                        m_aCMot[m_lstMotors[i]].nLed = nValue;
                    }
                }
                else
                {
                    m_aCMot[nID].nTorq = nValue;
                }
                Send(nID, 0x03, m_aCMot[nID]._Address_Led, (byte)(nValue));
            }
            public void Reboot() { Reboot(254); }
            public void Reboot(int nMotor)
            {
                if (nMotor == 254)
                {
                    for (int i = 0; i < m_lstMotors.Count; i++)
                    {
                        m_aCMot[m_lstMotors[i]].nTorq = 0;
                    }
                }
                else
                {
                    m_aCMot[nMotor].nTorq = 0;
                }
                Send_Command(nMotor, 0x08);
            }
            #endregion Reboot / Reset
            #region Timer
            private int m_nWait = 0;
            public void Delay(int nMilliseconds)
            {
                CTimer CTmr = new CTimer();
                CTmr.Set(); while (CTmr.Get() < nMilliseconds)
                {
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) break;
                    //Ojw.CTimer.Wait(0);
                    DoEvent();
                }
            }
            public void Wait()
            {
                if (m_nWait <= 0) return;
                Wait(m_nWait);
            }
            public void Wait_Per(float fPercent_0_1)
            {
                Wait((int)Math.Round((float)m_nWait * fPercent_0_1));
            }
            public void Wait(int nMilliseconds)
            {
                CTimer CTmr = new CTimer();
                CTmr.Set(); while (CTmr.Get() < nMilliseconds)
                {
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) break;
                    //Ojw.CTimer.Wait(0);

                    DoEvent();
                }
            }
            public void DoEvent()
            {
                System.Windows.Forms.Application.DoEvents();
            }
            #endregion Timer

            private C3d.COjwDesignerHeader m_CHeader = null;//new C3d.COjwDesignerHeader();
            public void SetHeader(C3d.COjwDesignerHeader CHeader) 
            { 
                m_CHeader = CHeader; 
            }
            public C3d.COjwDesignerHeader GetHeader() { return m_CHeader; }
            public int SetXyz(int nNum, float fX, float fY, float fZ, out float [] afMots)
            {
                if (m_CHeader == null)
                {
                    afMots = null;
                    return -1;
                }
                int[] anMotorID = new int[256];
                double[] adValue = new double[256];
                int nCnt = Get_Inverse(nNum, fX, fY, fZ, out anMotorID, out adValue);
                afMots = new float[nCnt];
                for (int i = 0; i < nCnt; i++)
                {
                    afMots[i] = (float)adValue[i];
                    //Set(anMotorID[i], (float)adValue[i]);
                }
                return nCnt;
            }
            public int SetXyz(int nNum, float fX, float fY, float fZ)
            {
                if (m_CHeader == null) return -1;
                int[] anMotorID = new int[256];
                double[] adValue = new double[256];
                int nCnt = Get_Inverse(nNum, fX, fY, fZ, out anMotorID, out adValue);
                for (int i = 0; i < nCnt; i++)
                {
                    Set(anMotorID[i], (float)adValue[i]);
                    //CMon.Set(anMotorID[i], (float)adValue[i]);
                }
                return nCnt;
                //Send_Motor(nMilliSeconds);
            }
            public int Get_Inverse(int nNum, float fX, float fY, float fZ, out int[] anMotorID, out double[] adValue)
            {
                // 집어넣기 전에 내부 메모리를 클리어 한다.
                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref m_CHeader.pSOjwCode[nNum]);
                Ojw.CKinematics.CInverse.SetValue_X(fX);
                Ojw.CKinematics.CInverse.SetValue_Y(fY);
                Ojw.CKinematics.CInverse.SetValue_Z(fZ);

                // 현재의 모터각을 전부 집어 넣도록 한다.
                for (int i = 0; i < m_CHeader.nMotorCnt; i++)
                {
                    // 모터값을 3D에 넣어주고
                    //SetData(i, Ojw.CConvert.StrToFloat(m_txtAngle[i].Text));
                    // 그 값을 꺼내 수식 계산에 넣어준다.
                    Ojw.CKinematics.CInverse.SetValue_Motor(i, Get(i));
                }

                // 실제 수식계산
                Ojw.CKinematics.CInverse.CalcCode(ref m_CHeader.pSOjwCode[nNum]);


                // 나온 결과값을 옮긴다.
                int nMotCnt = m_CHeader.pSOjwCode[nNum].nMotor_Max;
                if (nMotCnt <= 0)
                {
                    anMotorID = null;
                    adValue = null;
                    return 0;
                }
                anMotorID = new int[nMotCnt];
                adValue = new double[nMotCnt];
                for (int i = 0; i < nMotCnt; i++)
                {
                    anMotorID[i] = m_CHeader.pSOjwCode[nNum].pnMotor_Number[i];
                    adValue[i] = Ojw.CKinematics.CInverse.GetValue_Motor(anMotorID[i]);
                }
                return nMotCnt;
            }

            public int Delta_Add(float fRot_Cw, int nID_Front, int nID_Left, int nID_Right, float fTop_Rad, float fTop_Length, float fBottom_Length, float fBottom_Rad)
            {
                m_lstCDelta.Add(new CDelta(fRot_Cw, nID_Front, nID_Left, nID_Right, fTop_Rad, fTop_Length, fBottom_Length, fBottom_Rad));
                return m_lstCDelta.Count;
            }            
            public void Delta_Clear() { m_lstCDelta.Clear(); }
            #region Delta
            public List<CDelta> m_lstCDelta = new List<CDelta>();
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
                    // 
                    if (fRot != 0)
                    {
                        if (Ojw.CMath.CalcRot(0.0f, 0.0f, fRot, ref fPos_X, ref fPos_Y, ref fPos_Height) == false)
                        {
                            fAngle_Front = fAngle_Left = fAngle_Right = 0;
                            return false;
                        }
                    }
                    double [] adVal = new double[3];
                    Ojw.CMath.Delta_Parallel_Init(fInit_Top_Radius, fInit_Top_Length, fInit_Bottom_Length, fInit_Bottom_Radius);
                    Ojw.CMath.Delta_Parallel_InverseKinematics(fPos_X, fPos_Y, fPos_Height, out adVal[0], out adVal[1], out adVal[2]);
                    fAngle_Front = (float)adVal[0];
                    fAngle_Left  = (float)adVal[1];
                    fAngle_Right = (float)adVal[2];
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
                    fY  = (float)adVal[1];
                    fHeight = (float)adVal[2];
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
            #endregion Delta
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
                    return m_lstCDelta[nIndex].CalcAngleToXyz(Get(m_lstCDelta[nIndex].nId_Front), Get(m_lstCDelta[nIndex].nId_Left), Get(m_lstCDelta[nIndex].nId_Right), out fX, out fY, out fHeight);
                }
                fX = fY = fHeight = 0.0f;
                return false;
            }
            public void Move_Delta(int nIndex, float fX, float fY, float fHeight, int nTime, int nDelay)
            {
                SetDelta(nIndex, fX, fY, fHeight);
                Send_Motor(nTime);
                Wait(nTime + nDelay);
            }
            public void Move_Delta(int nIndex, float fX, float fY, float fHeight, int nTime)
            {
                Move_Delta(nIndex, fX, fY, fHeight, nTime, 0);
            }
        }

    }
}
