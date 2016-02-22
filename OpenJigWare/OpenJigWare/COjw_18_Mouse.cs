using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CMouse
        {
            // if you make your class, just write in here
            [DllImport("user32", EntryPoint = "mouse_event")]
            private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);

            [DllImport("user32.dll")]
            private static extern int GetCursorPos(int x, int y);

            //Declare wrapper managed POINT class.
            [StructLayout(LayoutKind.Sequential)]
            public class POINT
            {
                public int x;
                public int y;
            }
            [StructLayout(LayoutKind.Sequential)]
            public class MouseHookStruct
            {
                public POINT pt;
                public int hwnd;
                public int wHitTestCode;
                public int dwExtraInfo;
            }

            private enum EMouseEventFlags_t
            {
                WM_MOUSEMOVE = 0x201,
                WM_LBUTTONDOWN = 0x202,
                WM_LBUTTONUP = 0x204,

                MOVE = 0x0001,
                LDOWN = 0x0002,
                LUP = 0x0004,
                RDOWN = 0x0008,
                RUP = 0x0010,
                MDOWN = 0x0020,
                MUP = 0x0040,
                WHEEL = 0x0800,
                ABSOLUTE = 0x8000,
            }

            public static void Mouse_Down() { mouse_event((uint)EMouseEventFlags_t.LDOWN, 0, 0, 0, new System.IntPtr()); }
            public static void Mouse_Up() { mouse_event((uint)EMouseEventFlags_t.LUP, 0, 0, 0, new System.IntPtr()); }
            public static void Mouse_Move_Left(int nValue) { Mouse_Move_Inc(-nValue, 0); }
            public static void Mouse_Move_Right(int nValue) { Mouse_Move_Inc(nValue, 0); }
            public static void Mouse_Move_Up(int nValue) { Mouse_Move_Inc(0, -nValue); }
            public static void Mouse_Move_Down(int nValue) { Mouse_Move_Inc(0, nValue); }
            public static void Mouse_Move_Inc(int nX, int nY) { Cursor.Position = new Point(Cursor.Position.X + nX, Cursor.Position.Y + nY); }
            public static void Mouse_Move_Inc(Point pnt) { Cursor.Position = new Point(Cursor.Position.X + pnt.X, Cursor.Position.Y + pnt.Y); }
            public static void Mouse_Move_Abs(int nX, int nY) { Cursor.Position = new Point(nX, nY); }
            public static void Mouse_Move_Abs(Point pnt) { Cursor.Position = pnt; }
        }
    }
}
