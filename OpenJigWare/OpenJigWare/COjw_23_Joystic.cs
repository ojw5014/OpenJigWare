﻿using System;
using System.Collections.Generic;
using System.Linq;
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

            public CJoystick(int id)
            {
                caps = new JOYCAPS();
                info = new JOYINFOEX
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(JOYINFOEX)),
                    dwFlags = ReturnType.All,
                };

                this.nID = id;
                this.IsValid = Update();

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
            }
            #endregion Define

            JOYINFOEX info;
            JOYCAPS caps;
            readonly Dictionary<PadKey, bool> isDown = new Dictionary<PadKey, bool>();
            // 좌상단
            public double dX0 { get { return (double)(info.dwXpos - caps.wXmin) / (caps.wXmax - caps.wXmin); } }
            public double dY0 { get { return (double)(info.dwYpos - caps.wYmin) / (caps.wYmax - caps.wYmin); } }
            public int nID { get; private set; }

            // Pad
            public double dX1 { get { return (double)(info.dwUpos - caps.wUmin) / (caps.wUmax - caps.wUmin); } }
            public double dY1 { get { return (double)(info.dwRpos - caps.wRmin) / (caps.wRmax - caps.wRmin); } }

            public double Slide { get { return (double)(info.dwZpos - caps.wZmin) / (caps.wZmax - caps.wZmin); } }
            public bool IsValid { get; private set; }
            public bool Update() 
            {
                bool bRet = false;
                if (this.nID >= 0)
                {
                    if (joyGetPosEx(this.nID, ref info) == 0)
                    {
                        foreach (PadKey i in Enum.GetValues(typeof(PadKey)))
                            isDown[i] = CheckIsDown(i);
                        bRet = true;
                    }
                }
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