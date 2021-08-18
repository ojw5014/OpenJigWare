using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CMouse
        {
            [DllImport("user32")]
            public static extern bool GetMessage(ref Message lpMsg, IntPtr handle, uint mMsgFilterInMain, uint mMsgFilterMax);
            /* ex)
                Message msg = new Message();
                while (GetMessage(ref msg,IntPtr.Zero, 0, 0))
                {
                    if (msg.message == ...)
                    {
                        ...
                    }
                }
             */

            // if you make your class, just write in here
            [DllImport("user32", EntryPoint = "mouse_event")]
            private static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

            [DllImport("user32.dll")]
            private static extern int GetCursorPos(out Point ptMouse);
            [DllImport("user32.dll")] 
            private static extern int SetCursorPos(int x, int y);

            //Declare wrapper managed POINT class.
            [StructLayout(LayoutKind.Sequential)]
            public class POINT
            {
                public int x;
                public int y;
            }
            //[StructLayout(LayoutKind.Sequential)]
            //public class MouseHookStruct
            //{
            //    public POINT pt;
            //    public int hwnd;
            //    public int wHitTestCode;
            //    public int dwExtraInfo;
            //}
            [StructLayout(LayoutKind.Sequential)]
            private struct MouseHookStruct
            {
                public Point pt;
                public int mouseData;
                public int flags;
                public int time;
                public long dwExtraInfo;
            }

            [DllImport("user32.dll")]
            private static extern IntPtr LoadCursorFromFile(String str);
            //[DllImport("user32.dll")]
            //public static extern IntPtr SetCursor(IntPtr hCursor);
            
            public static System.Windows.Forms.Cursor GetCursor()
            {
                return System.Windows.Forms.Cursor.Current;
            }
            public static void SetCursor(Cursor csr)
            {
                System.Windows.Forms.Cursor.Current = csr;
            }

            public enum ECursor
            {
                IDC_APPSTARTING = 32650,//  화살표/모래시계 
                IDC_ARROW       = 32512,//  화살표 
                IDC_CROSS       = 32515,//  십자가 
                IDC_HAND        = 32649,//  손 
                IDC_HELP        = 32651,//  도움말 
                IDC_IBEAM       = 32513,//  텍스트(빔) 
                IDC_ICON        = 32641,//  아이콘 
                IDC_NO          = 32648,//  원형 
                IDC_SIZE        = 32640,//  크기조정 
                IDC_SIZEALL     = 32646,//  크기조정 
                IDC_SIZENESW    = 32643,//  좌우 크기조정 
                IDC_SIZENS      = 32645,//  세로 크기조정 
                IDC_SIZENWSE    = 32642,//  좌우 크기조정 
                IDC_SIZEWE      = 32644,//  가로 크기조정 
                IDC_UPARROW     = 32516,//  상단 화살표 
                IDC_WAIT        = 32541//  대기 
            }
            /*
IDC_APPSTARTING 32650  화살표/모래시계 
IDC_ARROW       32512  화살표 
IDC_CROSS       32515  십자가 
IDC_HAND        32649  손 
IDC_HELP        32651  도움말 
IDC_IBEAM       32513  텍스트(빔) 
IDC_ICON        32641  아이콘 
IDC_NO          32648  원형 
IDC_SIZE        32640  크기조정 
IDC_SIZEALL     32646  크기조정 
IDC_SIZENESW    32643  좌우 크기조정 
IDC_SIZENS      32645  세로 크기조정 
IDC_SIZENWSE    32642  좌우 크기조정 
IDC_SIZEWE      32644  가로 크기조정 
IDC_UPARROW     32516  상단 화살표 
IDC_WAIT        32541  대기 
            
             */
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
            public enum EMouseMessage_t
            {
                WM_NULL = 0x0000,
                WM_CREATE = 0x0001,
                WM_DESTROY = 0x0002,
                WM_MOVE = 0x0003,
                WM_SIZE = 0x0005,
                WM_ACTIVATE = 0x0006,
                WM_SETFOCUS = 0x0007,
                WM_KILLFOCUS = 0x0008,
                WM_ENABLE = 0x000A,
                WM_SETREDRAW = 0x000B,
                WM_SETTEXT = 0x000C,
                WM_GETTEXT = 0x000D,
                WM_GETTEXTLENGTH = 0x000E,
                WM_PAINT = 0x000F,
                WM_CLOSE = 0x0010,
                WM_QUERYENDSESSION = 0x0011,
                WM_QUERYOPEN = 0x0013,
                WM_ENDSESSION = 0x0016,
                WM_QUIT = 0x0012,
                WM_ERASEBKGND = 0x0014,
                WM_SYSCOLORCHANGE = 0x0015,
                WM_SHOWWINDOW = 0x0018,
                WM_WININICHANGE = 0x001A,
                WM_SETTINGCHANGE = WM_WININICHANGE,
                WM_DEVMODECHANGE = 0x001B,
                WM_ACTIVATEAPP = 0x001C,
                WM_FONTCHANGE = 0x001D,
                WM_TIMECHANGE = 0x001E,
                WM_CANCELMODE = 0x001F,
                WM_SETCURSOR = 0x0020,
                WM_MOUSEACTIVATE = 0x0021,
                WM_CHILDACTIVATE = 0x0022,
                WM_QUEUESYNC = 0x0023,
                WM_GETMINMAXINFO = 0x0024,
                WM_PAINTICON = 0x0026,
                WM_ICONERASEBKGND = 0x0027,
                WM_NEXTDLGCTL = 0x0028,
                WM_SPOOLERSTATUS = 0x002A,
                WM_DRAWITEM = 0x002B,
                WM_MEASUREITEM = 0x002C,
                WM_DELETEITEM = 0x002D,
                WM_VKEYTOITEM = 0x002E,
                WM_CHARTOITEM = 0x002F,
                WM_SETFONT = 0x0030,
                WM_GETFONT = 0x0031,
                WM_SETHOTKEY = 0x0032,
                WM_GETHOTKEY = 0x0033,
                WM_QUERYDRAGICON = 0x0037,
                WM_COMPAREITEM = 0x0039,
                WM_GETOBJECT = 0x003D,
                WM_COMPACTING = 0x0041,
                WM_COMMNOTIFY = 0x0044,
                WM_WINDOWPOSCHANGING = 0x0046,
                WM_WINDOWPOSCHANGED = 0x0047,
                WM_POWER = 0x0048,
                WM_COPYDATA = 0x004A,
                WM_CANCELJOURNAL = 0x004B,
                WM_NOTIFY = 0x004E,
                WM_INPUTLANGCHANGEREQUEST = 0x0050,
                WM_INPUTLANGCHANGE = 0x0051,
                WM_TCARD = 0x0052,
                WM_HELP = 0x0053,
                WM_USERCHANGED = 0x0054,
                WM_NOTIFYFORMAT = 0x0055,
                WM_CONTEXTMENU = 0x007B,
                WM_STYLECHANGING = 0x007C,
                WM_STYLECHANGED = 0x007D,
                WM_DISPLAYCHANGE = 0x007E,
                WM_GETICON = 0x007F,
                WM_SETICON = 0x0080,
                WM_NCCREATE = 0x0081,
                WM_NCDESTROY = 0x0082,
                WM_NCCALCSIZE = 0x0083,
                WM_NCHITTEST = 0x0084,
                WM_NCPAINT = 0x0085,
                WM_NCACTIVATE = 0x0086,
                WM_GETDLGCODE = 0x0087,
                WM_SYNCPAINT = 0x0088,
                WM_NCMOUSEMOVE = 0x00A0,
                WM_NCLBUTTONDOWN = 0x00A1,
                WM_NCLBUTTONUP = 0x00A2,
                WM_NCLBUTTONDBLCLK = 0x00A3,
                WM_NCRBUTTONDOWN = 0x00A4,
                WM_NCRBUTTONUP = 0x00A5,
                WM_NCRBUTTONDBLCLK = 0x00A6,
                WM_NCMBUTTONDOWN = 0x00A7,
                WM_NCMBUTTONUP = 0x00A8,
                WM_NCMBUTTONDBLCLK = 0x00A9,
                WM_NCXBUTTONDOWN = 0x00AB,
                WM_NCXBUTTONUP = 0x00AC,
                WM_NCXBUTTONDBLCLK = 0x00AD,
                WM_INPUT_DEVICE_CHANGE = 0x00FE,
                WM_INPUT = 0x00FF,
                WM_KEYFIRST = 0x0100,
                WM_KEYDOWN = 0x0100,
                WM_KEYUP = 0x0101,
                WM_CHAR = 0x0102,
                WM_DEADCHAR = 0x0103,
                WM_SYSKEYDOWN = 0x0104,
                WM_SYSKEYUP = 0x0105,
                WM_SYSCHAR = 0x0106,
                WM_SYSDEADCHAR = 0x0107,
                WM_UNICHAR = 0x0109,
                WM_KEYLAST = 0x0109,
                WM_IME_STARTCOMPOSITION = 0x010D,
                WM_IME_ENDCOMPOSITION = 0x010E,
                WM_IME_COMPOSITION = 0x010F,
                WM_IME_KEYLAST = 0x010F,
                WM_INITDIALOG = 0x0110,
                WM_COMMAND = 0x0111,
                WM_SYSCOMMAND = 0x0112,
                WM_TIMER = 0x0113,
                WM_HSCROLL = 0x0114,
                WM_VSCROLL = 0x0115,
                WM_INITMENU = 0x0116,
                WM_INITMENUPOPUP = 0x0117,
                WM_MENUSELECT = 0x011F,
                WM_MENUCHAR = 0x0120,
                WM_ENTERIDLE = 0x0121,
                WM_MENURBUTTONUP = 0x0122,
                WM_MENUDRAG = 0x0123,
                WM_MENUGETOBJECT = 0x0124,
                WM_UNINITMENUPOPUP = 0x0125,
                WM_MENUCOMMAND = 0x0126,
                WM_CHANGEUISTATE = 0x0127,
                WM_UPDATEUISTATE = 0x0128,
                WM_QUERYUISTATE = 0x0129,
                WM_CTLCOLORMSGBOX = 0x0132,
                WM_CTLCOLOREDIT = 0x0133,
                WM_CTLCOLORLISTBOX = 0x0134,
                WM_CTLCOLORBTN = 0x0135,
                WM_CTLCOLORDLG = 0x0136,
                WM_CTLCOLORSCROLLBAR = 0x0137,
                WM_CTLCOLORSTATIC = 0x0138,
                MN_GETHMENU = 0x01E1,
                WM_MOUSEFIRST = 0x0200,
                WM_MOUSEMOVE = 0x0200,
                WM_LBUTTONDOWN = 0x0201,
                WM_LBUTTONUP = 0x0202,
                WM_LBUTTONDBLCLK = 0x0203,
                WM_RBUTTONDOWN = 0x0204,
                WM_RBUTTONUP = 0x0205,
                WM_RBUTTONDBLCLK = 0x0206,
                WM_MBUTTONDOWN = 0x0207,
                WM_MBUTTONUP = 0x0208,
                WM_MBUTTONDBLCLK = 0x0209,
                WM_MOUSEWHEEL = 0x020A,
                WM_XBUTTONDOWN = 0x020B,
                WM_XBUTTONUP = 0x020C,
                WM_XBUTTONDBLCLK = 0x020D,
                WM_MOUSEHWHEEL = 0x020E,
                WM_PARENTNOTIFY = 0x0210,
                WM_ENTERMENULOOP = 0x0211,
                WM_EXITMENULOOP = 0x0212,
                WM_NEXTMENU = 0x0213,
                WM_SIZING = 0x0214,
                WM_CAPTURECHANGED = 0x0215,
                WM_MOVING = 0x0216,
                WM_POWERBROADCAST = 0x0218,
                WM_DEVICECHANGE = 0x0219,
                WM_MDICREATE = 0x0220,
                WM_MDIDESTROY = 0x0221,
                WM_MDIACTIVATE = 0x0222,
                WM_MDIRESTORE = 0x0223,
                WM_MDINEXT = 0x0224,
                WM_MDIMAXIMIZE = 0x0225,
                WM_MDITILE = 0x0226,
                WM_MDICASCADE = 0x0227,
                WM_MDIICONARRANGE = 0x0228,
                WM_MDIGETACTIVE = 0x0229,
                WM_MDISETMENU = 0x0230,
                WM_ENTERSIZEMOVE = 0x0231,
                WM_EXITSIZEMOVE = 0x0232,
                WM_DROPFILES = 0x0233,
                WM_MDIREFRESHMENU = 0x0234,
                WM_IME_SETCONTEXT = 0x0281,
                WM_IME_NOTIFY = 0x0282,
                WM_IME_CONTROL = 0x0283,
                WM_IME_COMPOSITIONFULL = 0x0284,
                WM_IME_SELECT = 0x0285,
                WM_IME_CHAR = 0x0286,
                WM_IME_REQUEST = 0x0288,
                WM_IME_KEYDOWN = 0x0290,
                WM_IME_KEYUP = 0x0291,
                WM_MOUSEHOVER = 0x02A1,
                WM_MOUSELEAVE = 0x02A3,
                WM_NCMOUSEHOVER = 0x02A0,
                WM_NCMOUSELEAVE = 0x02A2,
                WM_WTSSESSION_CHANGE = 0x02B1,
                WM_TABLET_FIRST = 0x02c0,
                WM_TABLET_LAST = 0x02df,
                WM_CUT = 0x0300,
                WM_COPY = 0x0301,
                WM_PASTE = 0x0302,
                WM_CLEAR = 0x0303,
                WM_UNDO = 0x0304,
                WM_RENDERFORMAT = 0x0305,
                WM_RENDERALLFORMATS = 0x0306,
                WM_DESTROYCLIPBOARD = 0x0307,
                WM_DRAWCLIPBOARD = 0x0308,
                WM_PAINTCLIPBOARD = 0x0309,
                WM_VSCROLLCLIPBOARD = 0x030A,
                WM_SIZECLIPBOARD = 0x030B,
                WM_ASKCBFORMATNAME = 0x030C,
                WM_CHANGECBCHAIN = 0x030D,
                WM_HSCROLLCLIPBOARD = 0x030E,
                WM_QUERYNEWPALETTE = 0x030F,
                WM_PALETTEISCHANGING = 0x0310,
                WM_PALETTECHANGED = 0x0311,
                WM_HOTKEY = 0x0312,
                WM_PRINT = 0x0317,
                WM_PRINTCLIENT = 0x0318,
                WM_APPCOMMAND = 0x0319,
                WM_THEMECHANGED = 0x031A,
                WM_CLIPBOARDUPDATE = 0x031D,
                WM_DWMCOMPOSITIONCHANGED = 0x031E,
                WM_DWMNCRENDERINGCHANGED = 0x031F,
                WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320,
                WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
                WM_GETTITLEBARINFOEX = 0x033F,
                WM_HANDHELDFIRST = 0x0358,
                WM_HANDHELDLAST = 0x035F,
                WM_AFXFIRST = 0x0360,
                WM_AFXLAST = 0x037F,
                WM_PENWINFIRST = 0x0380,
                WM_PENWINLAST = 0x038F,
                WM_APP = 0x8000,
                WM_USER = 0x0400,
                WM_REFLECT = WM_USER + 0x1C00,
            }
            //public static void Mouse_Cursor(int nCursor)
            //{                
            //    SetCursor((IntPtr)nCursor);
            //}
            public static Cursor Mouse_GetCursor(string strFile)
            {
                return new Cursor(LoadCursorFromFile(strFile));
            }
            //public static void Mouse_Cursor(string str)
            //{
            //    SetCursor(LoadCursorFromFile(str));
            //}
            public static void Mouse_Scroll(int nWheel) { mouse_event((uint)EMouseEventFlags_t.WHEEL, 0, 0, nWheel, new System.IntPtr()); }
            public static void Mouse_Down() { mouse_event((uint)EMouseEventFlags_t.LDOWN, 0, 0, 0, new System.IntPtr()); }
            public static void Mouse_Up() { mouse_event((uint)EMouseEventFlags_t.LUP, 0, 0, 0, new System.IntPtr()); }
            public static void Mouse_Move_Left(int nValue) { Mouse_Move_Inc(-nValue, 0); }
            public static void Mouse_Move_Right(int nValue) { Mouse_Move_Inc(nValue, 0); }
            public static void Mouse_Move_Up(int nValue) { Mouse_Move_Inc(0, -nValue); }
            public static void Mouse_Move_Down(int nValue) { Mouse_Move_Inc(0, nValue); }
            public static void Mouse_Move_Inc(int nX, int nY) { Cursor.Position = new Point(Cursor.Position.X + nX, Cursor.Position.Y + nY); }
            public static void Mouse_Move_Inc(Point pnt) { Cursor.Position = new Point(Cursor.Position.X + pnt.X, Cursor.Position.Y + pnt.Y); }
            public static void Mouse_Move_Abs(int nX, int nY) { SetCursorPos(nX, nY); }//{ Cursor.Position = new Point(nX, nY); }
            public static void Mouse_Move_Abs(Point pnt) { Cursor.Position = pnt; }

            public static Point Mouse_Get()
            {
                Point ptMouse = new Point();
                GetCursorPos(out ptMouse);
                return ptMouse;
            }


            ///////////////////////////////////////////////////////////////////////////////////////////
            //private static delegate void UserFunction(object sender, EventArgs args);
            public delegate void UserFunction(object sender, CUserEventArgs e);
            private static UserFunction m_FUser_Mouse;
            public static void Mouse_InitEvent(UserFunction FUser) 
            {
                if (FUser != null) 
                {
                    m_FUser_Mouse = new UserFunction(FUser);
                    Event_Mouse.ClearEvent();
                    Event_Mouse.UserEventForArgs += new EventHandler<CUserEventArgs>(m_FUser_Mouse);
                }
                SetHook_Mouse(true);
            }
            public static void Mouse_DInit()
            {
                Event_Mouse.ClearEvent();
            }
            
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
            
            private static MouseHookStruct m_MouseHookStruct;// = null;

            private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);
            private static LowLevelProc m_proc_mouse = HookCallback_Mouse;
            private static IntPtr m_hookID_Mouse = IntPtr.Zero;

            private static Ojw.CUserEvent Event_Mouse = new Ojw.CUserEvent();
            private static bool m_bEvent_Mouse = false;

            private const int WH_KEYBOARD_LL = 13;
            private const int WH_MOUSE_LL = 14;

            private static IntPtr SetHook(bool bKeyBoard, LowLevelProc proc)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule) { return SetWindowsHookEx(((bKeyBoard == true) ? WH_KEYBOARD_LL : WH_MOUSE_LL), proc, GetModuleHandle(curModule.ModuleName), 0); }
            }
            public static void SetHook_Mouse(bool bOn)
            {
                m_hookID_Mouse = SetHook(false, m_proc_mouse);
                m_bEvent_Mouse = true;
            }
                        
            private static IntPtr HookCallback_Mouse(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0)// && wParam == (IntPtr)WM_KEYUP)
                {
                    m_MouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                    //Clipboard.Clear();
                    //Form tempForm = Form.ActiveForm;
                    //Label lb = (Label)(tempForm.Controls.Find("lbPos", true)[0]);
                    //lbMessage.Text = "X" + Convert.ToString(m_MouseHookStruct.pt.x) + ", Y" + Convert.ToString(m_MouseHookStruct.pt.y);

                    //Ojw.printf("마우스 ({0}: {1}, {2})\r\n", (int)wParam, m_MouseHookStruct.pt.x, m_MouseHookStruct.pt.y);
                    //if (wParam == (IntPtr)Ojw.CMouse.EMouseMessage_t.WM_LBUTTONDOWN)
                    //{
                    //    Ojw.printf("마우스 Down({0}: {1}, {2})\r\n", (int)wParam, m_MouseHookStruct.pt.x, m_MouseHookStruct.pt.y);
                    //}
                    //else if (wParam == (IntPtr)Ojw.CMouse.EMouseMessage_t.WM_MOUSEMOVE)
                    //{
                    //}
                    //else if (wParam == (IntPtr)Ojw.CMouse.EMouseMessage_t.WM_LBUTTONUP)
                    //{
                    //}
                    
                    if (m_bEvent_Mouse == true)
                        Event_Mouse.RunEvent("<mouse>", (int)wParam, m_MouseHookStruct.pt.X, m_MouseHookStruct.pt.Y, m_MouseHookStruct.mouseData);

                    //printf("마우스 ({0}: {1}, {2}, [{3}, {4}])\r\n", (int)wParam, m_MouseHookStruct.pt.X, m_MouseHookStruct.pt.Y, m_MouseHookStruct.mouseData, m_MouseHookStruct.flags);
                }
                return CallNextHookEx(m_hookID_Mouse, nCode, wParam, lParam);
            }

        }
    }
}
