// sorry I forgot where I got this key software from.
// if you know who is key software (Joystick) from, please let me know.
using System;
using System.Collections.Generic;

#if _USING_DOTNET_3_5 || _USING_DOTNET_2_0
#else
using System.Linq;
#endif
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

namespace OpenJigWare
{
    partial class Ojw
    {
        // if you make your class, just write in here
        public class CJoystick
        {
            public static int _ID_0 = 0;
            public static int _ID_1 = 1;
            public static int _ID_2 = 2;
            public static int _ID_3 = 3;
            public static int _ID_4 = 4;
            public static int _ID_5 = 5;

            public CXBox_t CXBox = new CXBox_t();
            public class CXBox_t
            {
                //public int nLockKey; // 0 : normal, 1 : for control, 2 : for special control(ex-tilt)
                public float Slide_Left;
                public float Slide_Right;

                public float fStick0_X;
                public float fStick0_Y;
                public float fStick0_Angle;
                public float fStick0_Length;
                public bool bStick0_Click;

                public float fStick1_X;
                public float fStick1_Y;
                public float fStick1_Angle;
                public float fStick1_Length;
                public bool bStick1_Click;
                
                public bool bKey_Front_Left;
                public bool bKey_Front_Right;

                public bool bKey_Start;
                public bool bKey_Back;

                public bool bKey_Up;
                public bool bKey_Down;
                public bool bKey_Left;
                public bool bKey_Right;

                public bool bPad_Up;
                public bool bPad_Down;
                public bool bPad_Left;
                public bool bPad_Right;

                public CXBox_t()
                {
                    Slide_Left = 0.0f;
                    Slide_Right = 0.0f;

                    fStick0_X = 0.0f;
                    fStick0_Y = 0.0f;
                    fStick0_Angle = 0.0f;
                    fStick0_Length = 0.0f;
                    bStick0_Click = false;

                    fStick1_X = 0.0f;
                    fStick1_Y = 0.0f;
                    fStick1_Angle = 0.0f;
                    fStick1_Length = 0.0f;
                    bStick1_Click = false;


                    bKey_Front_Left = false;
                    bKey_Front_Right = false;

                    bKey_Start = false;
                    bKey_Back = false;

                    bKey_Up = false;
                    bKey_Down = false;
                    bKey_Left = false;
                    bKey_Right = false;

                    bPad_Up = false;
                    bPad_Down = false;
                    bPad_Left = false;
                    bPad_Right = false;
                }
            }
            
            public CJoystick(int id)
            {
                caps = new JOYCAPS();
                caps.wXmax = 65535;
                caps.wYmax = 65535;
                caps.wZmax = 65535;
                caps.wRmax = 65535;
                caps.wUmax = 65535;
                caps.wVmax = 65535;

                info = new JOYINFOEX
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(JOYINFOEX)),
                    dwFlags = ReturnType.All,
                };

                this.nID = id;
                //this.IsValid = Update();
                Update();

                if (id != -1)
                    joyGetDevCaps(id, ref caps, Marshal.SizeOf(typeof(JOYCAPS)));
            }
            ~CJoystick()
            {
            }
            
            #region Define
            [SuppressUnmanagedCodeSecurity, DllImport("winmm")]
            static extern int joyGetNumDevs();
            [SuppressUnmanagedCodeSecurity, DllImport("winmm")]
            static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji);
            [SuppressUnmanagedCodeSecurity, DllImport("winmm")]
            static extern int joyGetDevCaps(int uJoyID, ref JOYCAPS pjc, int cbjc);

            [StructLayout(LayoutKind.Sequential)]
            private struct JOYINFOEX
            {
                public uint dwSize;
                public ReturnType dwFlags;
                public uint dwXpos;
                public uint dwYpos;
                public uint dwZpos;
                public uint dwRpos;
                public uint dwUpos;
                public uint dwVpos;
                public uint dwButtons;
                public uint dwButtonNumber;
                public POV dwPOV;
                public uint dwReserved1;
                public uint dwReserved2;
            }
            enum ReturnType : uint
            {
                None = 0,
                X = 1 << 0,
                Y = 1 << 1,
                Z = 1 << 2,
                R = 1 << 3,
                U = 1 << 4,
                V = 1 << 5,
                POV = 1 << 6,
                Buttons = 1 << 7,
                RawData = 1 << 8,
                POVContinuous = 1 << 9,
                Centered = 1 << 10,
                UseDeadZone = 1 << 11,
                All = X | Y | Z | R | U | V | POV | Buttons,
            }
            //private void Test()
            //{
            //    int nTest = 0;
            //    Tao.Platform.Windows.Winmm.JOYINFOEX test = new Tao.Platform.Windows.Winmm.JOYINFOEX();
            //    nTest = test.dwSize;
            //    nTest = test.dwFlags;
            //    nTest = test.dwXpos;
            //    nTest = test.dwYpos;
            //    nTest = test.dwZpos;
            //    nTest = test.dwRpos;
            //    nTest = test.dwUpos;
            //    nTest = test.dwVpos;
            //    nTest = test.dwButtons;
            //    nTest = test.dwButtonNumber;
            //    nTest = test.dwPOV;
            //    nTest = test.dwReserved1;
            //    nTest = test.dwReserved2;


            //    Tao.Platform.Windows.Winmm.JOYCAPS test2 = new Tao.Platform.Windows.Winmm.JOYCAPS();
                
            //    nTest = test2.wMid;
            //    nTest = test2.wPid;
            //    nTest = test2.szPname;
            //    nTest = test2.wXmin;
            //    nTest = test2.wXmax;
            //    nTest = test2.wYmin;
            //    nTest = test2.wYmax;
            //    nTest = test2.wZmin;
            //    nTest = test2.wZmax;
            //    nTest = test2.wNumButtons;
            //    nTest = test2.wPeriodMin;
            //    nTest = test2.wPeriodMax;
            //    nTest = test2.wRmin;
            //    nTest = test2.wRmax;
            //    nTest = test2.wUmin;
            //    nTest = test2.wUmax;
            //    nTest = test2.wVmin;
            //    nTest = test2.wVmax;
            //    nTest = test2.wCaps;
            //    nTest = test2.wMaxAxes;
            //    nTest = test2.wNumAxes;
            //    nTest = test2.wMaxButtons;
            //    nTest = test2.szRegKey;
            //    nTest = test2.szOEMVxD;
            //}
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            struct JOYCAPS
            {
                public ushort wMid;
                public ushort wPid;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szPname;
                public uint wXmin;
                public uint wXmax;
                public uint wYmin;
                public uint wYmax;
                public uint wZmin;
                public uint wZmax;
                public uint wNumButtons;
                public uint wPeriodMin;
                public uint wPeriodMax;
                public uint wRmin;
                public uint wRmax;
                public uint wUmin;
                public uint wUmax;
                public uint wVmin;
                public uint wVmax;
                public uint wCaps;
                public uint wMaxAxes;
                public uint wNumAxes;
                public uint wMaxButtons;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szRegKey;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szOEMVxD;
            }
            enum POV : uint
            {
                None = 65535,
                Up = 0,
                UpRight = 4500,
                Right = 9000,
                DownRight = 13500,
                Down = 18000,
                DownLeft = 22500,
                Left = 27000,
                UpLeft = 31500,
            }

            public enum PadKey : byte
            {
                None,
                Button1,
                Button2,
                Button3,
                Button4,
                Button5,
                Button6,
                Button7,
                Button8,
                Button9,
                Button10,
                Button11,
                Button12,
                Button13,
                Button14,
                Button15,
                Button16,
                Button17,
                Button18,
                Button19,
                Button20,
                Button21,
                Button22,
                Button23,
                Button24,
                Button25,
                Button26,
                Button27,
                Button28,
                Button29,
                Button30,
                StickUp,
                StickDown,
                StickLeft,
                StickRight,
                POVUp,
                POVDown,
                POVLeft,
                POVRight,
                SpinUp,
                SpinDown,
                SpinLeft,
                SpinRight,
                
#if _USING_DOTNET_2_0 || _USING_DOTNET_3_5
                _Count
#else
#endif
            }
            #endregion Define

            JOYINFOEX info;
            JOYCAPS caps;
            readonly Dictionary<PadKey, bool> isDown = new Dictionary<PadKey, bool>();
            private int[] m_anClickEvent_Up = new int[100];//(int)(PadKey.SpinRight) + 1];
            private int[] m_anClickEvent_Down = new int[100];//(int)(PadKey.SpinRight) + 1];
#if true
            //// 좌상단
            //public double dX0 { get { return (double)(((caps.wXmax - caps.wXmin) == 0) ? 0.5 : (double)(info.dwXpos - caps.wXmin) / (caps.wXmax - caps.wXmin)); } }
            //public double dY0 { get { return (double)(((caps.wYmax - caps.wYmin) == 0) ? 0.5 : (double)(info.dwYpos - caps.wYmin) / (caps.wYmax - caps.wYmin)); } }
            //public int nID { get; private set; }

            //// Pad
            //public double dX1 { get { return (double)(((caps.wUmax - caps.wUmin) == 0) ? 0.5 : (double)(info.dwUpos - caps.wUmin) / (caps.wUmax - caps.wUmin)); } }
            //public double dY1 { get { return (double)(((caps.wRmax - caps.wRmin) == 0) ? 0.5 : (double)(info.dwRpos - caps.wRmin) / (caps.wRmax - caps.wRmin)); } }

            //public double Slide { get { return (double)((caps.wZmax - caps.wZmin == 0) ? 0.5 : ((double)(info.dwZpos - caps.wZmin) / (caps.wZmax - caps.wZmin))); } }
            
            // 좌상단
            public double dX0 { get { return (double)(info.dwXpos - caps.wXmin) / (caps.wXmax - caps.wXmin); } }
            public double dY0 { get { return (double)(info.dwYpos - caps.wYmin) / (caps.wYmax - caps.wYmin); } }
            public int nID { get; private set; }

            // Pad
            public double dX1 { get { return (double)(info.dwUpos - caps.wUmin) / (caps.wUmax - caps.wUmin); } }
            public double dY1 { get { return (double)(info.dwRpos - caps.wRmin) / (caps.wRmax - caps.wRmin); } }

            public double Slide { get { return (double)(info.dwZpos - caps.wZmin) / (caps.wZmax - caps.wZmin); } }

            #region 위의 데이타들은 XBox 기준으로 생성이 되어 있으므로 Default 설정으로 재설정
            public double GetPos0 { get { return (double)(info.dwXpos - caps.wXmin) / (((caps.wXmax == 0) ? 65535 : caps.wXmax) - caps.wXmin); } }
            public double GetPos1 { get { return (double)(info.dwYpos - caps.wYmin) / (((caps.wYmax == 0) ? 65535 : caps.wXmax) - caps.wYmin); } }
            public double GetPos2 { get { return (double)(info.dwZpos - caps.wZmin) / (((caps.wZmax == 0) ? 65535 : caps.wXmax) - caps.wZmin); } }
            public double GetPos3 { get { return (double)(info.dwRpos - caps.wRmin) / (((caps.wRmax == 0) ? 65535 : caps.wXmax) - caps.wRmin); } }
            public double GetPos4 { get { return (double)(info.dwUpos - caps.wUmin) / (((caps.wUmax == 0) ? 65535 : caps.wXmax) - caps.wUmin); } }
            public double GetPos5 { get { return (double)(info.dwVpos - caps.wVmin) / (((caps.wVmax == 0) ? 65535 : caps.wXmax) - caps.wVmin); } }
            public double GetPos(int nNum)
            {
                double dValue = 0;
                switch (nNum)
                {
                    case 0: dValue = GetPos0; break;
                    case 1: dValue = GetPos1; break;
                    case 2: dValue = GetPos2; break;
                    case 3: dValue = GetPos3; break;
                    case 4: dValue = GetPos4; break;
                    case 5: dValue = GetPos5; break;
                    default: break;
                }
                return dValue;
            }
            #endregion


            private bool m_bValid = false;
            public bool IsValid //{ get; private set; }
            {
                get
                {
                    //return (
                    //        (
                    //            (caps.wXmax != 0) ||
                    //            (caps.wYmax != 0) ||
                    //            (caps.wUmax != 0) ||
                    //            (caps.wRmax != 0) ||
                    //            (caps.wZmax != 0)
                    //        ) 
                    //        &&
                    //        (m_bValid == true)
                    //    );
                    return m_bValid;
                }
                private set
                {
                    m_bValid = value;
                }
            }
#else
            // 좌상단
            public double dX0 { get { return (double)(info.dwXpos - caps.wXmin) / (caps.wXmax - caps.wXmin); } }
            public double dY0 { get { return (double)(info.dwYpos - caps.wYmin) / (caps.wYmax - caps.wYmin); } }
            public int nID { get; private set; }

            // Pad
            public double dX1 { get { return (double)(info.dwUpos - caps.wUmin) / (caps.wUmax - caps.wUmin); } }
            public double dY1 { get { return (double)(info.dwRpos - caps.wRmin) / (caps.wRmax - caps.wRmin); } }

            public double Slide { get { return (double)(info.dwZpos - caps.wZmin) / (caps.wZmax - caps.wZmin); } }
            public bool IsValid { get; private set; }
#endif
            public bool Update() 
            {                
                bool bRet = false;
                if (this.nID >= 0)
                {
                    if (joyGetPosEx(this.nID, ref info) == 0)
                    {
                        foreach (PadKey i in Enum.GetValues(typeof(PadKey)))
                        {
                            isDown[i] = CheckIsDown(i);
                            int nIndex = (int)i;
                            // Down Event
                            if ((isDown[i] == true) && (m_anClickEvent_Down[nIndex] == 0)) m_anClickEvent_Down[nIndex] = 1;
                            else if ((isDown[i] == false) && (m_anClickEvent_Down[nIndex] != 0)) m_anClickEvent_Down[nIndex] = 0;
                            
                            // Up Event
                            if ((isDown[i] == true) && (m_anClickEvent_Up[nIndex] == 0)) m_anClickEvent_Up[nIndex] = 1;
                            else if ((isDown[i] == false) && (m_anClickEvent_Up[nIndex] == 1)) m_anClickEvent_Up[nIndex] = 2;
                        }
                        bRet = true;
                    }
                }

                ////////
                #region XBox

                CXBox.bPad_Left = IsDown(Ojw.CJoystick.PadKey.POVLeft);
                CXBox.bPad_Right = IsDown(Ojw.CJoystick.PadKey.POVRight);
                CXBox.bPad_Up = IsDown(Ojw.CJoystick.PadKey.POVUp);
                CXBox.bPad_Down = IsDown(Ojw.CJoystick.PadKey.POVDown);

                // 슬라이드
                double dMul = 1.0;
                CXBox.Slide_Left = ((Slide >= 0.5) ? (float)(dMul * (0.5 - Slide)) : 0.0f);
                CXBox.Slide_Right = ((Slide <= 0.5) ? (float)(dMul * (Slide - 0.5)) : 0.0f);
                
                #region 계산
                float fValueX = (float)((dX0 - 0.5));
                float fValueY = (float)((dY0 - 0.5));
                float fLength = (float)Math.Sqrt(fValueX * fValueX + fValueY * fValueY);
                float fTheta = (fLength > 0.2f) ? (float)(Ojw.CMath.ATan2(fValueX, fValueY) + 90.0f) % 360.0f : 0.0f;
                if (fTheta > 180.0f) fTheta -= 360.0f; // 이걸 살리면 +-180 으로 데이타가 변형된다.

                CXBox.fStick0_Angle = fTheta;
                CXBox.fStick0_Length = fLength;
                CXBox.fStick0_X = (float)dX0;
                CXBox.fStick0_Y = (float)dY0;
                CXBox.bStick0_Click = IsDown(Ojw.CJoystick.PadKey.Button9);
                
                fValueX = (float)((dX1 - 0.5));
                fValueY = (float)((dY1 - 0.5));
                fLength = (float)Math.Sqrt(fValueX * fValueX + fValueY * fValueY);
                fTheta = (fLength > 0.2f) ? (float)(Ojw.CMath.ATan2(fValueX, fValueY) + 90.0f) % 360.0f : 0.0f;
                if (fTheta > 180.0f) fTheta -= 360.0f; // 이걸 살리면 +-180 으로 데이타가 변형된다.

                CXBox.fStick1_Angle = fTheta;
                CXBox.fStick1_Length = fLength;
                CXBox.fStick1_X = (float)dX1;
                CXBox.fStick1_Y = (float)dY1;
                CXBox.bStick1_Click = IsDown(Ojw.CJoystick.PadKey.Button10);
                #endregion 계산

                CXBox.bKey_Left = IsDown(Ojw.CJoystick.PadKey.Button2);
                CXBox.bKey_Right = IsDown(Ojw.CJoystick.PadKey.Button3);
                CXBox.bKey_Up = IsDown(Ojw.CJoystick.PadKey.Button4);
                CXBox.bKey_Down = IsDown(Ojw.CJoystick.PadKey.Button1);

                CXBox.bKey_Start = IsDown(Ojw.CJoystick.PadKey.Button8);
                CXBox.bKey_Back = IsDown(Ojw.CJoystick.PadKey.Button7);

                CXBox.bKey_Front_Left = IsDown(Ojw.CJoystick.PadKey.Button5);
                CXBox.bKey_Front_Right = IsDown(Ojw.CJoystick.PadKey.Button6);
                #endregion XBox

                IsValid = bRet;
                return bRet;
            }

            private bool CheckIsDown(PadKey key)
            {
                const double stickThreshold = 0.2;
                const double stickReverseThreshold = 1 - stickThreshold;

                switch (key)
                {
                    case PadKey.None:
                        return false;
                    case PadKey.StickUp:
                        return this.dY0 < stickThreshold;
                    case PadKey.StickDown:
                        return this.dY0 > stickReverseThreshold;
                    case PadKey.StickLeft:
                        return this.dX0 < stickThreshold;
                    case PadKey.StickRight:
                        return this.dX0 > stickReverseThreshold;
                    case PadKey.POVUp:
                        return info.dwPOV == POV.Up 
                            || info.dwPOV == POV.UpLeft 
                            || info.dwPOV == POV.UpRight;
                    case PadKey.POVDown:
                        return info.dwPOV == POV.Down
                            || info.dwPOV == POV.DownLeft
                            || info.dwPOV == POV.DownRight;
                    case PadKey.POVLeft:
                        return info.dwPOV == POV.Left
                            || info.dwPOV == POV.UpLeft
                            || info.dwPOV == POV.DownLeft;
                    case PadKey.POVRight:
                        return info.dwPOV == POV.Right
                            || info.dwPOV == POV.UpRight
                            || info.dwPOV == POV.DownRight;
                    //case PadKey.SpinUp:
                    //    return this.SpinY < stickThreshold;
                    //case PadKey.SpinDown:
                    //    return this.SpinY > stickReverseThreshold;
                    //case PadKey.SpinLeft:
                    //    return this.SpinX < stickThreshold;
                    //case PadKey.SpinRight:
                    //    return this.SpinX > stickReverseThreshold;
                    default:
                        return (info.dwButtons & (uint)Math.Pow(2, (int)key - 1)) != 0;
                }
            }
            
#if _USING_DOTNET_2_0 || _USING_DOTNET_3_5

            public bool IsDown(IEnumerable<PadKey> all)
            {
                foreach (PadKey key in all) { if (isDown[key] == false) return false; }
                return true;
            }

            public bool IsDown(params PadKey[] all)
            {
                foreach (PadKey key in all) { if (isDown[key] == false) return false; }
                return true;
            }

            public bool IsDown(PadKey key)
            {
                if (isDown.Count > 0)
                    return isDown[key];
                return false;
            }
#else
            public bool IsDown(IEnumerable<PadKey> all)
            {
                return all.All(IsDown);
            }

            public bool IsDown(params PadKey[] all)
            {
                return all.All(IsDown);
            }

            public bool IsDown(PadKey key)
            {
                if (isDown.Count > 0)
                    return isDown[key];
                return false;
            }
            public bool IsDown(int key)
            {
                if (isDown.Count > 0)
                    return isDown[(PadKey)key];
                return false;
            }
#endif
            public bool IsDown_Event(PadKey key)
            {
                if (m_anClickEvent_Down[(int)key] == 1)
                {
                    m_anClickEvent_Down[(int)key] = 2;
                    return true;
                }
                return false;
            }
            public bool IsDown_Event(int key)
            {
                if (m_anClickEvent_Down[key] == 1)
                {
                    m_anClickEvent_Down[key] = 2;
                    return true;
                }
                return false;
            }
#if _USING_DOTNET_2_0 || _USING_DOTNET_3_5

            public bool IsUp(IEnumerable<PadKey> all)
            {
                foreach (PadKey key in all) { if (!isDown[key] == false) return false; }
                return true;
            }

            public bool IsUp(params PadKey[] all)
            {
                foreach (PadKey key in all) { if (!isDown[key] == false) return false; }
                return true;
            }

            public bool IsUp(PadKey key)
            {
                if (isDown.Count > 0)
                    return !isDown[key];
                return false;
            }
#else
            public bool IsUp(IEnumerable<PadKey> all)
            {
                return all.All(IsUp);
            }

            public bool IsUp(params PadKey[] all)
            {
                return all.All(IsUp);
            }

            public bool IsUp(PadKey key)
            {
                return !isDown[key];
            }
            public bool IsUp(int key)
            {
                return !isDown[(PadKey)key];
            }
#endif

            public bool IsUp_Event(PadKey key)
            {
                if (m_anClickEvent_Up[(int)key] == 2)
                {
                    m_anClickEvent_Up[(int)key] = 0;
                    return true;
                }
                return false;
            }
            public bool IsUp_Event(int key)
            {
                if (m_anClickEvent_Up[key] == 2)
                {
                    m_anClickEvent_Up[key] = 0;
                    return true;
                }
                return false;
            }
            //public static IEnumerable<CJoystick> GetAvailableGamePads()
            //{
            //    var max = joyGetNumDevs();

            //    for (int i = 0; i < max; i++)
            //    {
            //        var rt = new CJoystick(i);
            //        if (rt.IsValid())
            //            yield return rt;
            //    }
            //}
        }
    }
}
