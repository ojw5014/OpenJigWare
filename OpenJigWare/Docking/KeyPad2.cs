using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenJigWare;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace OpenJigWare.Docking
{
    public partial class frmKeyPad : Form
    {
        #region For Users
        public void SetPosition(int nX, int nY) { this.Left = nX; this.Top = nY; }
        public void SetImage(Image img) { this.BackgroundImage = img; }
        private bool m_bCloseEvent_ApplicationExit = false;
        public void SetCloseEvent(bool bApplicationExit) { m_bCloseEvent_ApplicationExit = bApplicationExit; }

        #endregion For Users


        public frmKeyPad()
        {
            InitializeComponent();
            
            //Init();
        }
        #region API(DllImport)
        [DllImport("user32.dll")]                           private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]                           private static extern void SetWindowText(int h, String s);
        [DllImport("user32.dll")]                           private static extern IntPtr GetFocus();
        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)] private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);        
        [DllImport("user32.dll")]                           private static extern IntPtr GetForegroundWindow();
        [DllImport("User32.dll")]                           private static extern Int32 SetForegroundWindow(int hWnd);
        [DllImport("user32.dll")]                           private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("user32.dll")]                           private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]    private static extern uint GetCurrentThreadId();
        
        [DllImport("imm32.dll", CharSet = CharSet.Auto)]    private static extern int ImmGetContext(int hWnd);
        //[DllImport("Imm32.dll")]                          private static extern int ImmGetContext(IntPtr hWnd);
        //[DllImport("imm32.dll", SetLastError = true)]     private static extern int ImmGetContext(int hWnd);
        [DllImport("imm32.dll", CharSet = CharSet.Auto)]    private static extern int ImmReleaseContext(int hWnd, int hImc);
        [DllImport("imm32.dll", CharSet = CharSet.Auto)]    private static extern int ImmGetConversionStatus(int hImc, out int fdwConversion, out int fdwSentence);
        [DllImport("imm32.dll", CharSet = CharSet.Auto)]    private static extern bool ImmGetConversionStatus(IntPtr hIMC, out int conversion, out int sentence);
        [DllImport("imm32.dll", CharSet = CharSet.Auto)]    private static extern int ImmSetConversionStatus(IntPtr hIMC, int fdwConversion, int fdwSentence);
        [DllImport("imm32.dll", CharSet = CharSet.Auto)]    private static extern bool ImmSetConversionStatus(int hImc, int fdwConversion, int fdwSentence);
        
        [DllImport("user32.dll")]                           private static extern uint keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        // 이 밑의 건 WndProc 에서 사용
        //[DllImport("user32.dll", EntryPoint = "SendMessageA")]
        //private static extern int SendMessage(IntPtr handle, int wMsg, int wParam, int lParam);

        // 아래 두개가 쌍
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);
        // second overload of SendMessage
        //[DllImport("user32.dll")]
        //private static extern int SendMessage(IntPtr hWnd, uint Msg, out int wParam, out int lParam);
        
        [DllImport("user32.dll")]                           private static extern int FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]                           private static extern int FindWindowEx(int hWnd1, int hWnd2, string IPsz1, string IPsz2); //findwindowex API 선언

        // 마우스 좌표를 주면 hwnd를 return한다.
        [DllImport("user32.dll")]                           protected static extern IntPtr WindowFromPoint(int x, int y);

        #endregion API(DllImport)
        #region Keyboard Hooking
        #region 변수선언 및 API Import
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]                           private static extern short GetKeyState(int keyCode);  // Capslock 확인하기 위해
        
        #region For Mouse
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        [DllImport("user32.dll")]
        private static extern int GetCursorPos(int x, int y);

        //Declare wrapper managed POINT class.
        [StructLayout(LayoutKind.Sequential)]
        private class POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        private class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }
        #endregion For Mouse

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelProc m_proc_keyboard = HookCallback_KeyBoard;
        private static LowLevelProc m_proc_mouse = HookCallback_Mouse;
        private static IntPtr m_hookID_KeyBoard = IntPtr.Zero;
        private static IntPtr m_hookID_Mouse = IntPtr.Zero;

        #endregion 변수선언 및 API Import

        #region Hooking
        private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr HookCallback_KeyBoard(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //if (((int)wParam == 229) && (lParam.Equals(-2147483647)) || ((int)wParam == 229 && lParam.Equals(-2147483648.0)))
            //    return IntPtr.Zero;
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                if (m_nMouseCommand != 1)
                {
                    OjwClearMem();
                    OjwClearMem2();
                }
            }
            else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
#if false
                int vkCode = Marshal.ReadInt32(lParam);
                Console.WriteLine((Keys)vkCode);
                if (vkCode == 44)
                {
                    Clipboard.Clear();
                    MessageBox.Show("test");
                }
#else
                int vkCode = Marshal.ReadInt32(lParam);

                //MessageBox.Show(vkCode.ToString());

                //Clipboard.Clear();
                if (((Keys)vkCode == System.Windows.Forms.Keys.F7) && (Keys.Control == Control.ModifierKeys))
                {
                    Form.ActiveForm.Width = 672;
                    Form.ActiveForm.Height = 449;
                    //MessageBox.Show("F7이 눌렸습니다. - 테스트 모드 비활성화");
                    //SetHook_Mouse(false);
                    //m_bTestMode = true;


                }
                else if (((Keys)vkCode == System.Windows.Forms.Keys.F8) && (Keys.Control == Control.ModifierKeys))
                {
                    //String strData = "";
                    //if (Ojw.CInputBox.Show("Input your password", "This is [Super Editing Mode]. If you want to use this you need to input your exactly password", ref strData) == DialogResult.OK)
                    //{
                    //    if (strData == "ONJINWOOK")//"OJW5014")
                    //    {
                    //        Form.ActiveForm.Width = 982;
                    //        Form.ActiveForm.Height = 449;
                    //        //MessageBox.Show("F8이 눌렸습니다. - 테스트 모드 활성화");
                    //        //SetHook_Mouse(true);
                    //        m_bTestMode = true;
                    //    }
                    //}
                }
                //else MessageBox.Show("Key = " + vkCode.ToString());
#endif
            }
            return CallNextHookEx(m_hookID_KeyBoard, nCode, wParam, lParam);
        }
        private static Point m_pntStart = new Point();
        private static Point m_pntCurr = new Point();
        private static Point m_pntEnd = new Point();
        private static bool m_bMouseClicked = false;
        private static MouseHookStruct m_MouseHookStruct = null;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MOUSEMOVE = 0x200;
        private static Point m_pntMouse;
        private static int m_nMouseCommand = 0;
        private static IntPtr HookCallback_Mouse(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0)// && wParam == (IntPtr)WM_KEYUP)
                {

                    //if (((int)wParam == 229) && (lParam.Equals(-2147483647)) || ((int)wParam == 229 && lParam.Equals(-2147483648.0)))
                    //    return IntPtr.Zero;

                    //int vkCode = Marshal.ReadInt32(lParam);
                    m_MouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                    //Clipboard.Clear();
                    //Form tempForm = Form.ActiveForm;
                    //Label lbMessage = (Label)(tempForm.Controls.Find("lbMouse", true)[0]);
                    //lbMessage.Text = "X" + Convert.ToString(m_MouseHookStruct.pt.x) + ", Y" + Convert.ToString(m_MouseHookStruct.pt.y);

                    bool bUp = false;
                    bool bChanged = false;
                    if (wParam == (IntPtr)WM_LBUTTONDOWN)
                    {
                        //OjwMessage("마우스 Down");
                        if (m_bMouseClicked == false) bChanged = true;
                        m_bMouseClicked = true;
                        m_pntStart.X = m_MouseHookStruct.pt.x;
                        m_pntStart.Y = m_MouseHookStruct.pt.y;

                        m_pntMouse.X = m_MouseHookStruct.pt.x;
                        m_pntMouse.Y = m_MouseHookStruct.pt.y;
                    }
                    else if (wParam == (IntPtr)WM_MOUSEMOVE)
                    {
                        m_pntCurr.X = m_MouseHookStruct.pt.x;
                        m_pntCurr.Y = m_MouseHookStruct.pt.y;
                        if ((m_bMouseClicked == true) && ((m_pntCurr.Y < m_frmMain.Top + 20) && (m_pntCurr.Y >= m_frmMain.Top)))
                        {
                            int nGap_X = m_pntMouse.X - m_MouseHookStruct.pt.x;
                            int nGap_Y = m_pntMouse.Y - m_MouseHookStruct.pt.y;
                            m_frmMain.Left -= nGap_X;
                            m_frmMain.Top -= nGap_Y;
                            m_pntMouse.X = m_MouseHookStruct.pt.x;
                            m_pntMouse.Y = m_MouseHookStruct.pt.y;
                        }
                    }
                    else if (wParam == (IntPtr)WM_LBUTTONUP)
                    {
                        bUp = true;
                        if (m_bMouseClicked == true) bChanged = true;
                        m_bMouseClicked = false;
                        m_pntEnd.X = m_MouseHookStruct.pt.x;
                        m_pntEnd.Y = m_MouseHookStruct.pt.y;
                    }

                    if (bChanged == true)
                    {
                        m_nMouseCommand = 1;
                        m_pntCurr.X = m_MouseHookStruct.pt.x;
                        m_pntCurr.Y = m_MouseHookStruct.pt.y;

                        int nValue_X = m_pntCurr.X - m_pntStart.X;
                        int nValue_Y = m_pntCurr.Y - m_pntStart.Y;

                        //bool bClicked = false;
                        for (int i = 0; i < 4; i++)
                        {
                            for (int j = 0; j < 11; j++)
                            {
                                if ((i == 3) && (j == 10)) break;
                                CheckLbButton("lb_" + i.ToString() + "_" + j.ToString(), 4, m_pntStart.X, m_pntStart.Y, bUp,
                                m_aImage[i, j, m_nType, 0], // 여길...
                                m_aImage[i, j, m_nType, 1]);
                            }
                        }
                    }
                    else
                    {
                        if (m_frmMain != null)
                        {
                            if ( // 그래도 폼은 눌렸다면(포커스가 변경한게 아닌 것과 동일한 의미)
                                ((m_frmMain.Left <= m_MouseHookStruct.pt.x) && ((m_frmMain.Left + m_frmMain.Width) >= m_MouseHookStruct.pt.x)) &&
                                ((m_frmMain.Top <= m_MouseHookStruct.pt.y) && ((m_frmMain.Top + m_frmMain.Height) >= m_MouseHookStruct.pt.y)) &&
                                (m_nMouseCommand != 0) // 직전 명령이 무언가 있었다면
                            ) m_nMouseCommand = 2;
                            else
                            {
                                OjwClearMem();
                                OjwClearMem2();
                                m_nMouseCommand = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                Application.Exit();
            }
            return CallNextHookEx(m_hookID_Mouse, nCode, wParam, lParam);
        }
        private static bool CheckLbButton(String strButtonName, int nMessageCmd, int nX, int nY, bool bUp, Image bmpDown, Image bmpUp)
        {
            return CheckLbButton(m_frmMain, strButtonName, nMessageCmd, nX, nY, bUp, bmpDown, bmpUp);
        }

        private static void DoButtonEvent(String strButtonName)
        {
            int nFirst = strButtonName.IndexOf('_') + 1;
            int nLast = strButtonName.LastIndexOf('_') + 1;
            int nLength0 = nLast - nFirst - 1;
            int nLength1 = strButtonName.Length - nLast;
            int nLine = Convert.ToInt32(strButtonName.Substring(nFirst, nLength0));
            int nPos = Convert.ToInt32(strButtonName.Substring(nLast, nLength1));
            //OjwMessage(strButtonName.Substring(nFirst, nLength0) + "," + strButtonName.Substring(nLast, nLength1));

            if (m_nType == (int)EKeyType._KEY_ASCII)
            {
                #region _KEY_ASCII
                String strKey = "";
                switch (nLine)
                {
                    case 0:
                        switch (nPos)
                        {
                            case 0: strKey = "1"; break;
                            case 1: strKey = "2"; break;
                            case 2: strKey = "3"; break;
                            case 3: strKey = "4"; break;
                            case 4: strKey = "5"; break;
                            case 5: strKey = "6"; break;
                            case 6: strKey = "7"; break;
                            case 7: strKey = "8"; break;
                            case 8: strKey = "9"; break;
                            case 9: strKey = "0"; break;
                            case 10: strKey = "-"; break;
                        }
                        break;
                    case 1:
                        switch (nPos)
                        {
                            case 0: strKey = "＋"; break;
                            case 1: strKey = "×"; break;
                            case 2: strKey = "÷"; break;
                            case 3: strKey = "="; break;
                            case 4: strKey = "%"; break;
                            case 5: strKey = "☆"; break;
                            case 6: strKey = "★"; break;
                            case 7: strKey = "♡"; break;
                            case 8: strKey = "♥"; break;
                            case 9: strKey = "?"; break;
                            case 10: strKey = "{BS}"; break;
                        }
                        break;
                    case 2:
                        switch (nPos)
                        {
                            //case 0: strKey = "Q; break;
                            case 1: strKey = "!"; break;
                            case 2: strKey = "@"; break;
                            case 3: strKey = "#"; break;
                            case 4: strKey = "~"; break;
                            case 5: strKey = "/"; break;
                            case 6: strKey = "^"; break;
                            case 7: strKey = "&"; break;
                            case 8: strKey = "("; break;
                            case 9: strKey = ")"; break;
                            //case 10: strKey = ""; break;
                        }
                        break;
                    case 3:
                        switch (nPos)
                        {
                            case 2: strKey = "*"; break;
                            case 3: strKey = "\'"; break;
                            case 4: strKey = "\""; break;
                            case 5: strKey = " "; break;
                            case 6: strKey = ";"; break;
                            case 7: strKey = ":"; break;
                            case 8: strKey = ","; break;
                            case 9: strKey = "."; break;
                        }
                        break;
                }
                if (strKey != "")
                {
                    if (strKey == "{BS}")
                    {
                        keybd_event((byte)Keys.Back, 0, 0x00, 0); // Down
                        keybd_event((byte)Keys.Back, 0, 0x02, 0); // Up
                    }
                    else
                    {
                        OjwClearMem();
                        OjwSetMem(strKey);

                        ClearMem_AndCopyToObject();
                    }
                }
                #endregion _KEY_ASCII
            }
            else if (m_nType == (int)EKeyType._KEY_KOR)
            {
#if true
                #region _KEY_KOR
                String strKey = "";
                switch (nLine)
                {
                    case 0:
                        switch (nPos)
                        {
                            case 0: strKey = "1"; break;
                            case 1: strKey = "2"; break;
                            case 2: strKey = "3"; break;
                            case 3: strKey = "4"; break;
                            case 4: strKey = "5"; break;
                            case 5: strKey = "6"; break;
                            case 6: strKey = "7"; break;
                            case 7: strKey = "8"; break;
                            case 8: strKey = "9"; break;
                            case 9: strKey = "0"; break;
                            case 10: strKey = "-"; break;
                        }
                        break;
                    case 1:
                        switch (nPos)
                        {
                            case 0: strKey = "q"; break;
                            case 1: strKey = "w"; break;
                            case 2: strKey = "e"; break;
                            case 3: strKey = "r"; break;
                            case 4: strKey = "t"; break;
                            case 5: strKey = "y"; break;
                            case 6: strKey = "u"; break;
                            case 7: strKey = "i"; break;
                            case 8: strKey = "o"; break;
                            case 9: strKey = "p"; break;
                            case 10: strKey = "{BS}"; break;
                        }
                        break;
                    case 2:
                        switch (nPos)
                        {
                            //case 0: strKey = "Q; break;
                            case 1: strKey = "a"; break;
                            case 2: strKey = "s"; break;
                            case 3: strKey = "d"; break;
                            case 4: strKey = "f"; break;
                            case 5: strKey = "g"; break;
                            case 6: strKey = "h"; break;
                            case 7: strKey = "j"; break;
                            case 8: strKey = "k"; break;
                            case 9: strKey = "l"; break;
                            //case 10: strKey = ""; break;
                        }
                        break;
                    case 3:
                        switch (nPos)
                        {
                            //case 0: strKey = "Q; break;
                            //case 1: strKey = "W; break;
                            case 2: strKey = "z"; break;
                            case 3: strKey = "x"; break;
                            case 4: strKey = "c"; break;
                            case 5: strKey = " "; break;
                            case 6: strKey = "v"; break;
                            case 7: strKey = "b"; break;
                            case 8: strKey = "n"; break;
                            case 9: strKey = "m"; break;
                        }
                        break;
                }
                if (strKey != "")
                {
                    if (strKey == "{BS}")
                    {
                        keybd_event((byte)Keys.Back, 0, 0x00, 0); // Down
                        keybd_event((byte)Keys.Back, 0, 0x02, 0); // Up

                        OjwClearMem();
                        OjwClearMem2();
                    }
                    else if (strKey == " ")
                    {
                        OjwClearMem();
                        OjwClearMem2();
                    }
                    // 숫자 키
                    else if (
                        (strKey == "0") ||
                        (strKey == "1") ||
                        (strKey == "2") ||
                        (strKey == "3") ||
                        (strKey == "4") ||
                        (strKey == "5") ||
                        (strKey == "6") ||
                        (strKey == "7") ||
                        (strKey == "8") ||
                        (strKey == "9") ||
                        (strKey == "-")
                    )
                    {
                        OjwClearMem();
                        OjwClearMem2();
                        OjwSetMem(strKey);

                        ClearMem_AndCopyToObject();

                        OjwClearMem();
                    }
                    else
                    {
                        OjwSetMem(strKey);

                        DataToHangul();

                        ClearMem_AndCopyToObject();
                    }
                }
                #endregion _KEY_KOR
#endif
            }
            else if (m_nType == (int)EKeyType._KEY_KOR_DOUBLE)
            {
                #region _KEY_KOR_DOUBLE
                String strKey = "";
                switch (nLine)
                {
                    case 0:
                        switch (nPos)
                        {
                            case 0: strKey = "1"; break;
                            case 1: strKey = "2"; break;
                            case 2: strKey = "3"; break;
                            case 3: strKey = "4"; break;
                            case 4: strKey = "5"; break;
                            case 5: strKey = "6"; break;
                            case 6: strKey = "7"; break;
                            case 7: strKey = "8"; break;
                            case 8: strKey = "9"; break;
                            case 9: strKey = "0"; break;
                            case 10: strKey = "-"; break;
                        }
                        break;
                    case 1:
                        switch (nPos)
                        {
                            case 0: strKey = "Q"; break;
                            case 1: strKey = "W"; break;
                            case 2: strKey = "E"; break;
                            case 3: strKey = "R"; break;
                            case 4: strKey = "T"; break;
                            case 5: strKey = "y"; break;
                            case 6: strKey = "u"; break;
                            case 7: strKey = "i"; break;
                            case 8: strKey = "O"; break;
                            case 9: strKey = "P"; break;
                            case 10: strKey = "{BS}"; break;
                        }
                        break;
                    case 2:
                        switch (nPos)
                        {
                            //case 0: strKey = "Q; break;
                            case 1: strKey = "a"; break;
                            case 2: strKey = "s"; break;
                            case 3: strKey = "d"; break;
                            case 4: strKey = "f"; break;
                            case 5: strKey = "g"; break;
                            case 6: strKey = "h"; break;
                            case 7: strKey = "j"; break;
                            case 8: strKey = "k"; break;
                            case 9: strKey = "l"; break;
                            //case 10: strKey = ""; break;
                        }
                        break;
                    case 3:
                        switch (nPos)
                        {
                            //case 0: strKey = "Q; break;
                            //case 1: strKey = "W; break;
                            case 2: strKey = "z"; break;
                            case 3: strKey = "x"; break;
                            case 4: strKey = "c"; break;
                            case 5: strKey = " "; break;
                            case 6: strKey = "v"; break;
                            case 7: strKey = "b"; break;
                            case 8: strKey = "n"; break;
                            case 9: strKey = "m"; break;
                        }
                        break;
                }
                if (strKey != "")
                {
                    if (strKey == "{BS}")
                    {
                        keybd_event((byte)Keys.Back, 0, 0x00, 0); // Down
                        keybd_event((byte)Keys.Back, 0, 0x02, 0); // Up
                        OjwClearMem();
                        OjwClearMem2();
                    }
                    else if (strKey == " ")
                    {
                        keybd_event((byte)Keys.Space, 0, 0x00, 0); // Down
                        keybd_event((byte)Keys.Space, 0, 0x02, 0); // Up
                        OjwClearMem();
                        OjwClearMem2();
                    }
                    // 숫자 키
                    else if (
                        (strKey == "0") ||
                        (strKey == "1") ||
                        (strKey == "2") ||
                        (strKey == "3") ||
                        (strKey == "4") ||
                        (strKey == "5") ||
                        (strKey == "6") ||
                        (strKey == "7") ||
                        (strKey == "8") ||
                        (strKey == "9") ||
                        (strKey == "-")
                    )
                    {
                        OjwClearMem();
                        OjwClearMem2();
                        OjwSetMem(strKey);

                        ClearMem_AndCopyToObject();

                        OjwClearMem();
                    }
                    else
                    {
                        //OjwClearMem();
                        OjwSetMem(strKey);

                        DataToHangul();

                        ClearMem_AndCopyToObject();
                    }
                }
                #endregion _KEY_KOR_DOUBLE
            }
            else if (m_nType == (int)EKeyType._KEY_ENG)
            {
                byte byKey = 0;
                #region _KEY_ENG
                //int nData = nLine * 256 + nPos;
                switch (nLine)
                {
                    case 0:
                        switch (nPos)
                        {
                            case 0: byKey = (byte)Keys.D1; break;
                            case 1: byKey = (byte)Keys.D2; break;
                            case 2: byKey = (byte)Keys.D3; break;
                            case 3: byKey = (byte)Keys.D4; break;
                            case 4: byKey = (byte)Keys.D5; break;
                            case 5: byKey = (byte)Keys.D6; break;
                            case 6: byKey = (byte)Keys.D7; break;
                            case 7: byKey = (byte)Keys.D8; break;
                            case 8: byKey = (byte)Keys.D9; break;
                            case 9: byKey = (byte)Keys.D0; break;
                            case 10: byKey = (byte)Keys.Subtract; break;
                        }
                        break;
                    case 1:
                        switch (nPos)
                        {
                            case 0: byKey = (byte)Keys.Q; break;
                            case 1: byKey = (byte)Keys.W; break;
                            case 2: byKey = (byte)Keys.E; break;
                            case 3: byKey = (byte)Keys.R; break;
                            case 4: byKey = (byte)Keys.T; break;
                            case 5: byKey = (byte)Keys.Y; break;
                            case 6: byKey = (byte)Keys.U; break;
                            case 7: byKey = (byte)Keys.I; break;
                            case 8: byKey = (byte)Keys.O; break;
                            case 9: byKey = (byte)Keys.P; break;
                            case 10: byKey = (byte)Keys.Back; break;
                        }
                        break;
                    case 2:
                        switch (nPos)
                        {
                            //case 0: byKey = (byte)Keys.Q; break;
                            case 1: byKey = (byte)Keys.A; break;
                            case 2: byKey = (byte)Keys.S; break;
                            case 3: byKey = (byte)Keys.D; break;
                            case 4: byKey = (byte)Keys.F; break;
                            case 5: byKey = (byte)Keys.G; break;
                            case 6: byKey = (byte)Keys.H; break;
                            case 7: byKey = (byte)Keys.J; break;
                            case 8: byKey = (byte)Keys.K; break;
                            case 9: byKey = (byte)Keys.L; break;
                            //case 10: byKey = (byte)Keys.; break;
                        }
                        break;
                    case 3:
                        switch (nPos)
                        {
                            //case 0: byKey = (byte)Keys.Q; break;
                            //case 1: byKey = (byte)Keys.W; break;
                            case 2: byKey = (byte)Keys.Z; break;
                            case 3: byKey = (byte)Keys.X; break;
                            case 4: byKey = (byte)Keys.C; break;
                            case 5: byKey = (byte)Keys.Space; break;
                            case 6: byKey = (byte)Keys.V; break;
                            case 7: byKey = (byte)Keys.B; break;
                            case 8: byKey = (byte)Keys.N; break;
                            case 9: byKey = (byte)Keys.M; break;
                        }
                        break;
                }
                keybd_event(byKey, 0, 0x00, 0); // Down
                keybd_event(byKey, 0, 0x02, 0); // Up
                #endregion _KEY_ENG
            }
            else if (m_nType == (int)EKeyType._KEY_ENG_LARGE)
            {
                #region _KEY_ENG_LARGE
                String strKey = "";
                switch (nLine)
                {
                    case 0:
                        switch (nPos)
                        {
                            case 0: strKey = "1"; break;
                            case 1: strKey = "2"; break;
                            case 2: strKey = "3"; break;
                            case 3: strKey = "4"; break;
                            case 4: strKey = "5"; break;
                            case 5: strKey = "6"; break;
                            case 6: strKey = "7"; break;
                            case 7: strKey = "8"; break;
                            case 8: strKey = "9"; break;
                            case 9: strKey = "0"; break;
                            case 10: strKey = "-"; break;
                        }
                        break;
                    case 1:
                        switch (nPos)
                        {
                            case 0: strKey = "q"; break;
                            case 1: strKey = "w"; break;
                            case 2: strKey = "e"; break;
                            case 3: strKey = "r"; break;
                            case 4: strKey = "t"; break;
                            case 5: strKey = "y"; break;
                            case 6: strKey = "u"; break;
                            case 7: strKey = "i"; break;
                            case 8: strKey = "o"; break;
                            case 9: strKey = "p"; break;
                            case 10: strKey = "{BS}"; break;
                        }
                        break;
                    case 2:
                        switch (nPos)
                        {
                            //case 0: strKey = "Q; break;
                            case 1: strKey = "a"; break;
                            case 2: strKey = "s"; break;
                            case 3: strKey = "d"; break;
                            case 4: strKey = "f"; break;
                            case 5: strKey = "g"; break;
                            case 6: strKey = "h"; break;
                            case 7: strKey = "j"; break;
                            case 8: strKey = "k"; break;
                            case 9: strKey = "l"; break;
                            //case 10: strKey = ""; break;
                        }
                        break;
                    case 3:
                        switch (nPos)
                        {
                            //case 0: strKey = "Q; break;
                            //case 1: strKey = "W; break;
                            case 2: strKey = "z"; break;
                            case 3: strKey = "x"; break;
                            case 4: strKey = "c"; break;
                            case 5: strKey = " "; break;
                            case 6: strKey = "v"; break;
                            case 7: strKey = "b"; break;
                            case 8: strKey = "n"; break;
                            case 9: strKey = "m"; break;
                        }
                        break;
                }
                if (strKey != "")
                {
                    if (strKey == "{BS}")
                    {
                        keybd_event((byte)Keys.Back, 0, 0x00, 0); // Down
                        keybd_event((byte)Keys.Back, 0, 0x02, 0); // Up
                    }
                    else
                    {
                        OjwClearMem();
                        OjwSetMem(strKey.ToUpper());

                        ClearMem_AndCopyToObject();
                    }
                }
                #endregion _KEY_ENG_LARGE
            }
        }
        private static void DataToHangul()
        {
            String strResult = "";
            String strOrg = OjwGetMem();
            int nLength = Separate_For_Hangul(strOrg, out strResult);

            if ((strOrg.Length > 1) && (nLength > 0))
            {
                // bs
                keybd_event((byte)Keys.Back, 0, 0x00, 0); // Down
                keybd_event((byte)Keys.Back, 0, 0x02, 0); // Up
            }

            if (strOrg.Length != nLength)
            {
                String strOrg2 = strOrg.Substring(nLength);

                String strTmp0 = "";
                strTmp0 += (char)(strOrg[nLength - 1]);
                strTmp0 += (char)(strOrg[nLength]);
                String strTmp1 = "";
                // 이후 조합 체크
                if (Separate_For_Hangul(strTmp0, out strTmp1) > 1)
                {
                    nLength = Separate_For_Hangul(strOrg.Substring(0, nLength - 1), out strResult);
                    strOrg2 = strOrg.Substring(nLength);
                }

                OjwClearMem();
                OjwSetMem(strOrg2);

                // 메모리를 옮겨놓는다.
                OjwClearMem2();
                OjwSetMem2(strResult);

                String strResult2 = "";
                //String strOrg2 = OjwGetMem();
                int nLength2 = Separate_For_Hangul(strOrg2, out strResult2);
                OjwSetMem2(strResult2);
            }
            else
            {
                // 메모리를 옮겨놓는다.
                OjwClearMem2();
                OjwSetMem2(strResult);
            }
        }

        // true : 모음, false : 자음
        //private static bool CheckMoeum(char cData0, char cData1, out int nIndex1)            
        private static bool CheckMoeum(char cData0)
        {
            int nIndex1 = -1;
            //"ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ"
            if (cData0 == 'k') nIndex1 = 0;
            else if (cData0 == 'o') nIndex1 = 1;
            else if (cData0 == 'i') nIndex1 = 2;
            else if (cData0 == 'O') nIndex1 = 3;
            else if (cData0 == 'j') nIndex1 = 4;
            else if (cData0 == 'p') nIndex1 = 5;
            else if (cData0 == 'u') nIndex1 = 6;
            else if (cData0 == 'P') nIndex1 = 7;
            else if (cData0 == 'h') nIndex1 = 8;
            //else if ((cData0 == 'h') && (strEnglish[2] == 'k')) nIndex1 = 9;
            //else if ((cData0 == 'h') && (strEnglish[2] == 'o')) nIndex1 = 10;
            //else if ((cData0 == 'h') && (strEnglish[2] == 'l')) nIndex1 = 11;
            else if (cData0 == 'y') nIndex1 = 12;
            else if (cData0 == 'n') nIndex1 = 13;
            //else if ((cData0 == 'n') && (strEnglish[2] == 'j')) nIndex1 = 14;
            //else if ((cData0 == 'n') && (strEnglish[2] == 'p')) nIndex1 = 15;
            //else if ((cData0 == 'n') && (strEnglish[2] == 'l')) nIndex1 = 16;
            else if (cData0 == 'b') nIndex1 = 17;
            else if (cData0 == 'm') nIndex1 = 18;
            //else if ((cData0 == 'm') && (strEnglish[2] == 'l')) nIndex1 = 19;
            else if (cData0 == 'l') nIndex1 = 20;
            //else nIndex--;

            if (nIndex1 < 0) return false;
            return true;
        }
        private static int Separate_For_Hangul(String strEnglish, out String strResult)
        {
            strResult = "";
            int nLength = 0;
            int nIndex = 0;
            int nIndex0 = -1;
            int nIndex1 = -1;
            int nIndex2 = -1;
            //if (strEnglish.Length < 2) return -1;
            #region 초성검사
            if (nIndex == 0) // 초성
            {
                // "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ"
                //if (CheckMoeum(strEnglish[0]) == false)
                if (strEnglish.Length > 0)
                {
                    nIndex++;
                    if (strEnglish[0] == 'r') nIndex0 = 0;
                    else if (strEnglish[0] == 'R') nIndex0 = 1;
                    else if (strEnglish[0] == 's') nIndex0 = 2;
                    else if (strEnglish[0] == 'e') nIndex0 = 3;
                    else if (strEnglish[0] == 'E') nIndex0 = 4;
                    else if (strEnglish[0] == 'f') nIndex0 = 5;
                    else if (strEnglish[0] == 'a') nIndex0 = 6;
                    else if (strEnglish[0] == 'q') nIndex0 = 7;
                    else if (strEnglish[0] == 'Q') nIndex0 = 8;
                    else if (strEnglish[0] == 't') nIndex0 = 9;
                    else if (strEnglish[0] == 'T') nIndex0 = 10;
                    else if (strEnglish[0] == 'd') nIndex0 = 11;
                    else if (strEnglish[0] == 'w') nIndex0 = 12;
                    else if (strEnglish[0] == 'W') nIndex0 = 13;
                    else if (strEnglish[0] == 'c') nIndex0 = 14;
                    else if (strEnglish[0] == 'z') nIndex0 = 15;
                    else if (strEnglish[0] == 'x') nIndex0 = 16;
                    else if (strEnglish[0] == 'v') nIndex0 = 17;
                    else if (strEnglish[0] == 'g') nIndex0 = 18;
                    else nIndex--;
                }
                if (nIndex == 1) nLength++;
            }
            #endregion 초성검사
            #region 중성검사
            if (
                (nIndex == 1) ||
                (nIndex == 0) // 간혹 바로 모음이 들어오는 경우가 있다.
                ) // 중성
            {
                if (strEnglish.Length < (nLength + 1))
                {
                }
                else
                {
                    //nIndex++;
                    nIndex = 2;
                    if (strEnglish.Length > (nLength + 1))
                    {
                        //"ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ"
                        if (strEnglish[nLength] == 'l') nIndex1 = 20;
                        else if ((strEnglish[nLength] == 'm') && (strEnglish[nLength + 1] == 'l')) nIndex1 = 19;
                        else if (strEnglish[nLength] == 'm') nIndex1 = 18;
                        else if (strEnglish[nLength] == 'b') nIndex1 = 17;
                        else if ((strEnglish[nLength] == 'n') && (strEnglish[nLength + 1] == 'l')) nIndex1 = 16;
                        else if ((strEnglish[nLength] == 'n') && (strEnglish[nLength + 1] == 'p')) nIndex1 = 15;
                        else if ((strEnglish[nLength] == 'n') && (strEnglish[nLength + 1] == 'j')) nIndex1 = 14;
                        else if (strEnglish[nLength] == 'n') nIndex1 = 13;
                        else if (strEnglish[nLength] == 'y') nIndex1 = 12;
                        else if ((strEnglish[nLength] == 'h') && (strEnglish[nLength + 1] == 'l')) nIndex1 = 11;
                        else if ((strEnglish[nLength] == 'h') && (strEnglish[nLength + 1] == 'o')) nIndex1 = 10;
                        else if ((strEnglish[nLength] == 'h') && (strEnglish[nLength + 1] == 'k')) nIndex1 = 9;
                        else if (strEnglish[nLength] == 'h') nIndex1 = 8;
                        else if (strEnglish[nLength] == 'P') nIndex1 = 7;
                        else if (strEnglish[nLength] == 'u') nIndex1 = 6;
                        else if (strEnglish[nLength] == 'p') nIndex1 = 5;
                        else if (strEnglish[nLength] == 'j') nIndex1 = 4;
                        else if (strEnglish[nLength] == 'O') nIndex1 = 3;
                        else if (strEnglish[nLength] == 'i') nIndex1 = 2;
                        else if (strEnglish[nLength] == 'o') nIndex1 = 1;
                        else if (strEnglish[nLength] == 'k') nIndex1 = 0;
                        else nIndex--;
                    }
                    else //if (strEnglish.Length == nLength)
                    {
                        //"ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ"
                        if (strEnglish[nLength] == 'l') nIndex1 = 20;
                        else if (strEnglish[nLength] == 'm') nIndex1 = 18;
                        else if (strEnglish[nLength] == 'b') nIndex1 = 17;
                        else if (strEnglish[nLength] == 'n') nIndex1 = 13;
                        else if (strEnglish[nLength] == 'y') nIndex1 = 12;
                        else if (strEnglish[nLength] == 'h') nIndex1 = 8;
                        else if (strEnglish[nLength] == 'P') nIndex1 = 7;
                        else if (strEnglish[nLength] == 'u') nIndex1 = 6;
                        else if (strEnglish[nLength] == 'p') nIndex1 = 5;
                        else if (strEnglish[nLength] == 'j') nIndex1 = 4;
                        else if (strEnglish[nLength] == 'O') nIndex1 = 3;
                        else if (strEnglish[nLength] == 'i') nIndex1 = 2;
                        else if (strEnglish[nLength] == 'o') nIndex1 = 1;
                        else if (strEnglish[nLength] == 'k') nIndex1 = 0;
                        else nIndex--;
                    }
                    if (nIndex == 2)
                    {
                        nLength++;
                        if (
                            (nIndex1 == 9) ||
                            (nIndex1 == 10) ||
                            (nIndex1 == 11) ||
                            (nIndex1 == 14) ||
                            (nIndex1 == 15) ||
                            (nIndex1 == 16) ||
                            (nIndex1 == 19)
                        ) nLength++;
                    }
                }
            }
            #endregion 중성검사
            #region 종성검사
            if (
                (nIndex == 2) // 종성
                && (nIndex0 >= 0) // 종성의 경우 첫 자음이 들어오지 않았다면 연산하지 않도록...
            )
            {
                if (strEnglish.Length < (nLength + 1)) // 3)
                {
                }
                else
                {
                    nIndex++;
                    //"ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ"
                    if (strEnglish.Length > (nLength + 1))
                    {
                        if (strEnglish[nLength] == 'g') nIndex2 = 26;
                        else if (strEnglish[nLength] == 'v') nIndex2 = 25;
                        else if (strEnglish[nLength] == 'x') nIndex2 = 24;
                        else if (strEnglish[nLength] == 'z') nIndex2 = 23;
                        else if (strEnglish[nLength] == 'c') nIndex2 = 22;
                        else if (strEnglish[nLength] == 'w') nIndex2 = 21;
                        else if (strEnglish[nLength] == 'd') nIndex2 = 20;
                        else if (strEnglish[nLength] == 'T') nIndex2 = 19;
                        else if (strEnglish[nLength] == 't') nIndex2 = 18;
                        else if ((strEnglish[nLength] == 'q') && (strEnglish[nLength + 1] == 't')) nIndex2 = 17;
                        else if (strEnglish[nLength] == 'q') nIndex2 = 16;
                        else if (strEnglish[nLength] == 'a') nIndex2 = 15;
                        else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'g')) nIndex2 = 14;
                        else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'v')) nIndex2 = 13;
                        else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'x')) nIndex2 = 12;
                        else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 't')) nIndex2 = 11;
                        else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'q')) nIndex2 = 10;
                        else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'a')) nIndex2 = 9;
                        else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'r')) nIndex2 = 8;
                        else if (strEnglish[nLength] == 'f') nIndex2 = 7;
                        else if (strEnglish[nLength] == 'e') nIndex2 = 6;
                        else if ((strEnglish[nLength] == 's') && (strEnglish[nLength + 1] == 'g')) nIndex2 = 5;
                        else if ((strEnglish[nLength] == 's') && (strEnglish[nLength + 1] == 'w')) nIndex2 = 4;
                        else if (strEnglish[nLength] == 's') nIndex2 = 3;
                        else if ((strEnglish[nLength] == 'r') && (strEnglish[nLength + 1] == 't')) nIndex2 = 2;
                        else if (strEnglish[nLength] == 'R') nIndex2 = 1;
                        else if (strEnglish[nLength] == 'r') nIndex2 = 0;

                        else nIndex--;
                    }
                    else
                    {
                        if (strEnglish[nLength] == 'g') nIndex2 = 26;
                        else if (strEnglish[nLength] == 'v') nIndex2 = 25;
                        else if (strEnglish[nLength] == 'x') nIndex2 = 24;
                        else if (strEnglish[nLength] == 'z') nIndex2 = 23;
                        else if (strEnglish[nLength] == 'c') nIndex2 = 22;
                        else if (strEnglish[nLength] == 'w') nIndex2 = 21;
                        else if (strEnglish[nLength] == 'd') nIndex2 = 20;
                        else if (strEnglish[nLength] == 'T') nIndex2 = 19;
                        else if (strEnglish[nLength] == 't') nIndex2 = 18;
                        //else if ((strEnglish[nLength] == 'q') && (strEnglish[nLength + 1] == 't')) nIndex2 = 17;
                        else if (strEnglish[nLength] == 'q') nIndex2 = 16;
                        else if (strEnglish[nLength] == 'a') nIndex2 = 15;
                        //else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'g')) nIndex2 = 14;
                        //else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'v')) nIndex2 = 13;
                        //else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'x')) nIndex2 = 12;
                        //else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 't')) nIndex2 = 11;
                        //else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'q')) nIndex2 = 10;
                        //else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'a')) nIndex2 = 9;
                        //else if ((strEnglish[nLength] == 'f') && (strEnglish[nLength + 1] == 'r')) nIndex2 = 8;
                        else if (strEnglish[nLength] == 'f') nIndex2 = 7;
                        else if (strEnglish[nLength] == 'e') nIndex2 = 6;
                        //else if ((strEnglish[nLength] == 's') && (strEnglish[nLength + 1] == 'g')) nIndex2 = 5;
                        //else if ((strEnglish[nLength] == 's') && (strEnglish[nLength + 1] == 'w')) nIndex2 = 4;
                        else if (strEnglish[nLength] == 's') nIndex2 = 3;
                        //else if ((strEnglish[nLength] == 'r') && (strEnglish[nLength + 1] == 't')) nIndex2 = 2;
                        else if (strEnglish[nLength] == 'R') nIndex2 = 1;
                        else if (strEnglish[nLength] == 'r') nIndex2 = 0;

                        else nIndex--;
                    }
                    if (nIndex == 3)
                    {
                        nLength++;
                        if (
                            (nIndex2 == 2) ||
                            (nIndex2 == 4) ||
                            (nIndex2 == 5) ||
                            ((nIndex2 >= 8) && (nIndex2 <= 14)) ||
                            (nIndex2 == 17)
                        ) nLength++;
                    }
                }
            }
            #endregion 종성검사

            #region 한글 만들기
            //String strChosung = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
            //String strJoongsung = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
            //String strJongsung = "ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";
            int nIndex_Chosung = nIndex0;// strChosung.IndexOf("ㄱ");
            int nIndex_Joongsung = nIndex1;// strJoongsung.IndexOf("ㅏ");
            int nIndex_Jongsung = nIndex2;//strJongsung.IndexOf("");
            //int nResult = ((nIndex_Chosung * 588) + ((nIndex_Joongsung * 28) + nIndex_Jongsung)) + 44032;

            if (nIndex_Chosung >= 0)
            {
                if (nIndex_Joongsung >= 0)
                {
                    if (nIndex_Jongsung >= 0)
                    {
                        int nResult = ((nIndex_Chosung * 588) + ((nIndex_Joongsung * 28) + (nIndex_Jongsung + 1))) + 44032;
                        char cResult = Convert.ToChar(nResult);
                        strResult = cResult.ToString();
                    }
                    else
                    {
                        int nResult = ((nIndex_Chosung * 588) + ((nIndex_Joongsung * 28))) + 44032;
                        char cResult = Convert.ToChar(nResult);
                        strResult = cResult.ToString();
                    }
                }
                else
                {
                    String strChosung = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
                    strResult = strChosung[nIndex_Chosung].ToString();
                }
            }
            else
            {
                // 초성이 발견이 되지 않고 바로 중성이 오는 경우
                if (nIndex_Joongsung >= 0)
                {
                    String strJoongsung = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
                    strResult = strJoongsung[nIndex_Joongsung].ToString();
                }
            }
            #endregion 한글만들기

            return nLength;
        }
        private static void EngToHangul(String strEng)
        {
            String strChosung = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
            String strJoongsung = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
            String strJongsung = "ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";
            int nIndex_Chosung = strChosung.IndexOf("ㄱ");
            int nIndex_Joongsung = strJoongsung.IndexOf("ㅏ");
            int nIndex_Jongsung = strJongsung.IndexOf("");
            int nResult = ((nIndex_Chosung * 588) + ((nIndex_Joongsung * 28) + nIndex_Jongsung)) + 44032;
            char cResult = Convert.ToChar(nResult);
            MessageBox.Show(cResult.ToString());
        }
        //private static bool m_bClicked = false;
        private static void ClearMem_AndCopyToObject()
        {
            if (OjwGetMem() != "")
            {
                Clipboard.Clear();
                String strData = OjwGetMem2();
                if (strData.Length > 0)
                {
                    Clipboard.SetText(strData);
                }
                else
                    Clipboard.SetText(OjwGetMem());

                keybd_event((byte)Keys.ControlKey, 0, 0x00, 0); // Down
                keybd_event((byte)Keys.V, 0, 0x00, 0); // Down
                keybd_event((byte)Keys.V, 0, 0x02, 0); // Up
                keybd_event((byte)Keys.ControlKey, 0, 0x02, 0); // Up
            }
        }

        private static int m_nResult = 0;
        private static bool CheckLbButton(Form tempForm, String strButtonName, int nMessageCmd, int nX, int nY, bool bUp, Image bmpDown, Image bmpUp)
        {
            try
            {
                if (tempForm == null) return false;
                Label lbButton = (Label)(tempForm.Controls.Find(strButtonName, true)[0]);
                if (lbButton == null) return false;
                if (
                    (nX >= (lbButton.Left + tempForm.Left)) &&
                    (nX <= (lbButton.Left + tempForm.Left + lbButton.Width))
                    &&
                    (nY >= (lbButton.Top + tempForm.Top)) &&
                    (nY <= (lbButton.Top + tempForm.Top + lbButton.Height))
                    )
                {
                    if (bUp == false)
                    {
                        lbButton.Image = bmpUp;
                        DoButtonEvent(strButtonName);
                    }
                    else
                    {
                        lbButton.Image = bmpDown;
                    }
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }
        private static bool CheckLbButton(Label lbButton, int nX, int nY)
        {
            if (lbButton == null) return false;
            if (
                (nX >= (lbButton.Left + Form.ActiveForm.Left)) &&
                (nX <= (lbButton.Left + Form.ActiveForm.Left + lbButton.Width))
                &&
                (nY >= (lbButton.Top + Form.ActiveForm.Top)) &&
                (nY <= (lbButton.Top + Form.ActiveForm.Top + lbButton.Height))
                ) return true;
            return false;
        }
        #endregion Hooking

        #region Hook Fuction - Set ...
        private static IntPtr SetHook(bool bKeyBoard, LowLevelProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) { return SetWindowsHookEx(((bKeyBoard == true) ? WH_KEYBOARD_LL : WH_MOUSE_LL), proc, GetModuleHandle(curModule.ModuleName), 0); }
        }
        private static void SetHook_Keyboard(bool bOn)
        {
            m_hookID_KeyBoard = SetHook(true, m_proc_keyboard);
        }
        private static void SetHook_Mouse(bool bOn)
        {
            m_hookID_Mouse = SetHook(false, m_proc_mouse);
        }
        #endregion Hook Function - Set ...
        #endregion Keyboard Hooking

        private static bool m_bProgEnd = false;

        #region Message Box
        private static void OjwClearMem()
        {
            if (m_bProgEnd == true) return;
            try
            {
                TextBox txtMsg = (TextBox)(Form.FromHandle(m_hHandle).Controls.Find("txtMem", true)[0]);//
                txtMsg.Text = "";
            }
            catch (Exception e)
            {
                MessageBox.Show("[MemClear]" + e.ToString() + "\r\n");
            }
        }
        private static void OjwClearMem2()
        {
            if (m_bProgEnd == true) return;
            try
            {
                TextBox txtMsg2 = (TextBox)(Form.FromHandle(m_hHandle).Controls.Find("txtMem2", true)[0]);//
                txtMsg2.Text = "";
            }
            catch (Exception e)
            {
                MessageBox.Show("[Mem2Clear]" + e.ToString() + "\r\n");
            }
        }
        private static void OjwSetMem(string msg)
        {
            if (m_bProgEnd == true) return;
            try
            {
                TextBox txtMsg = (TextBox)(Form.FromHandle(m_hHandle).Controls.Find("txtMem", true)[0]);//
                txtMsg.AppendText(msg);       // 채팅 목록 창에 문자열 추가
                txtMsg.ScrollToCaret();                // 현재 캐럿의 위치로 스크롤 이동
            }
            catch (Exception e)
            {
                MessageBox.Show("[MemSet]" + e.ToString() + "\r\n");
            }
        }
        private static void OjwSetMem2(string msg)
        {
            if (m_bProgEnd == true) return;
            try
            {
                TextBox txtMsg = (TextBox)(Form.FromHandle(m_hHandle).Controls.Find("txtMem2", true)[0]);//
                txtMsg.AppendText(msg);       // 채팅 목록 창에 문자열 추가
                txtMsg.ScrollToCaret();                // 현재 캐럿의 위치로 스크롤 이동
            }
            catch (Exception e)
            {
                MessageBox.Show("[Mem2Set]" + e.ToString() + "\r\n");
            }
        }
        private static String OjwGetMem()
        {
            if (m_bProgEnd == true) return null;
            try
            {
                TextBox txtMsg = (TextBox)(Form.FromHandle(m_hHandle).Controls.Find("txtMem", true)[0]);//
                return txtMsg.Text;
            }
            catch (Exception e)
            {
                MessageBox.Show("[MemGet]" + e.ToString() + "\r\n");
                return "";
            }
        }
        private static String OjwGetMem2()
        {
            if (m_bProgEnd == true) return null;
            try
            {
                TextBox txtMsg = (TextBox)(Form.FromHandle(m_hHandle).Controls.Find("txtMem2", true)[0]);//
                return txtMsg.Text;
            }
            catch (Exception e)
            {
                MessageBox.Show("[Mem2Get]" + e.ToString() + "\r\n");
                return "";
            }
        }
        private static void SetError()
        {
            if (m_bProgEnd == true) return;
            OjwMessage("**************[Error]***********\n");
            //if (menu_Debug.Checked == true) this.ForeColor = Color.Red;
        }
        private static int m_nMessageStack = 0;
        private static void OjwMessage(string msg)
        {
            m_nMessageStack = 1;
            OjwDebugMessage(true, true, true, true, msg);
        }
        private static void OjwMessage_Error(string msg)
        {
            OjwMessage("**************[Error]***********\n");
            //if (menu_Debug.Checked == true) this.ForeColor = Color.Red;

            m_nMessageStack = 1;
            OjwDebugMessage(true, true, true, true, msg);
        }
        private static void OjwDebugMessage(bool bFile, bool bFunction, bool bLine, bool bTime, string msg)
        {
            if (m_bProgEnd == true) return;
            try
            {
                m_nMessageStack++;

                String strMsg = "";
                System.Reflection.MethodBase method = new System.Diagnostics.StackTrace(true).GetFrame(m_nMessageStack).GetMethod();
                System.Diagnostics.StackFrame sf = new System.Diagnostics.StackTrace(true).GetFrame(m_nMessageStack);
                //String strTime = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day.ToString() + "," + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString();
                if (bFile && (sf.GetFileName() != null)) strMsg += "{" + Ojw.CFile.GetName(sf.GetFileName()) + "}";
                if (bFunction && (method.Name != null)) strMsg += "{" + method.Name + "}";
                if (bLine) strMsg += "{" + sf.GetFileLineNumber().ToString() + "}";
                if (bTime) strMsg += "{" + DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day.ToString() + "," + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + "}";
                //Form frm = Form.ActiveForm;


                //TextBox txtMsg = (TextBox)(m_frmMain.Controls.Find("txtMessage", true)[0]);//
                TextBox txtMsg = (TextBox)(Form.FromHandle(m_hHandle).Controls.Find("txtMessage", true)[0]);//
                txtMsg.AppendText(strMsg + msg + "\r\n");       // 채팅 목록 창에 문자열 추가
                txtMsg.ScrollToCaret();                // 현재 캐럿의 위치로 스크롤 이동
                m_nMessageStack = 0;
            }
            catch (Exception e)
            {
                //MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                m_nMessageStack = 0;
            }
        }
        private static void OjwMessage(TextBox txtOjwMessage, string msg)
        {
            try
            {
                txtOjwMessage.AppendText(msg + "\r\n");       // 채팅 목록 창에 문자열 추가
                txtOjwMessage.ScrollToCaret();                // 현재 캐럿의 위치로 스크롤 이동
            }
            catch (Exception e)
            {
                MessageBox.Show("[Message]" + e.ToString() + "\r\n");
            }
        }
        private static DialogResult InputBox(String title, String promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
        #endregion Message Box


        private enum EKeyType
        {
            _KEY_ASCII = 0,
            _KEY_KOR,
            _KEY_KOR_DOUBLE,
            _KEY_ENG,
            _KEY_ENG_LARGE,
            _KEY_MAX
        };
        private static int m_nType = (int)EKeyType._KEY_KOR;

        private const int _CNT_LINE = 4;
        private const int _CNT_NUM = 11;
        private Label[,] m_albButton = new Label[_CNT_LINE, _CNT_NUM];
        private static Image[, , ,] m_aImage = new Image[_CNT_LINE, _CNT_NUM, 5, 2]; // 5(Type), 2(Normal / Over)

        // 실제 그림을 메모리에 매핑한다. image -> m_aImage[Line, Col, EKeyType, (Normal(0) or Over(1))]
        private void InitParseImage()
        {
            int nType = 0;
            int nOver = 0;
            int i = 0;
            int j = 0;
            for (nType = 0; nType < (int)EKeyType._KEY_MAX; nType++)
            {
                j = 0; nOver = 0;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_1;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_2;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_3;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_4;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_5;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_6;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_7;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_8;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_9;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_0;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_hyphen;

                j = 0; nOver = 1;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_1;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_2;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_3;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_4;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_5;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_6;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_7;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_8;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_9;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_0;
                m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_bar;
            }

            #region ASCII
            nType = (int)EKeyType._KEY_ASCII;
            i = 1; // q,w,e,r,t,y,u,i,o,p,backspace
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_1_0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_1_1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_1_2;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_1_3;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_1_4;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_1_5;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_1_6;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_1_7;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_1_8;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_1_9;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_1_10_Back;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_1_0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_1_1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_1_2;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_1_3;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_1_4;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_1_5;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_1_6;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_1_7;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_1_8;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_1_9;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_1_10_Back;

            i = 2; // shift, a, s, d, f, g, h, j, k, l, 확인
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_2_0_Shift;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_2_1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_2_2;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_2_3;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_2_4;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_2_5;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_2_6;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_2_7;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_2_8;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_2_9;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_2_10_확인;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_2_0_Shift;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_2_1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_2_2;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_2_3;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_2_4;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_2_5;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_2_6;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_2_7;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_2_8;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_2_9;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_2_10_확인;

            i = 3; // 기호, 한영, z, x, c, Spacebar, v, b, n, m 
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_3_0_기호;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_3_1_한영;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_3_2;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_3_3;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_3_4;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_3_5_Spacebar;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_3_6;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_3_7;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_3_8;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_normal_3_9;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_3_0_기호;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_3_1_한영;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_3_2;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_3_3;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_3_4;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_3_5_Spacebar;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_3_6;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_3_7;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_3_8;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.a_over_3_9;
            #endregion ASCII

            #region Korean(Normal)
            nType = (int)EKeyType._KEY_KOR;

            i = 1; // ㅂ, ㅈ, ㄷ, ㄱ, ㅅ, ㅛ, ㅕ, ㅑ, ㅐ, ㅔ, back
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅂ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅈ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㄷ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㄱ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅅ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅛ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅕ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅑ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅐ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅔ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_Back;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅂ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅈ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㄷ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㄱ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅅ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅛ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅕ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅑ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅐ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅔ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_Back;

            i = 2; // Shift, ㅁ, ㄴ, ㅇ, ㄹ, ㅎ, ㅗ, ㅓ, ㅏ, ㅣ, 확인
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_Shift;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅁ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㄴ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅇ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㄹ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅎ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅗ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅓ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅏ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅣ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_확인;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_Shift;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅁ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㄴ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅇ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㄹ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅎ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅗ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅓ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅏ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅣ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_확인;

            i = 3; // 기호, 한영, ㅋ, ㅌ, ㅊ, Spacebar, ㅍ, ㅠ, ㅜ, ㅡ
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_기호;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_한영;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅋ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅌ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅊ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_Spacebar;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅍ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅠ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅜ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅡ;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_기호;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_한영;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅋ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅌ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅊ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_Spacebar;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅍ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅠ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅜ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅡ;
            #endregion Korean(Normal)

            #region Korean(Double)
            nType = (int)EKeyType._KEY_KOR_DOUBLE;

            i = 1; // ㅃ, ㅉ, ㄸ, ㄲ, ㅆ, ㅛ, ㅕ, ㅑ, ㅒ, ㅖ, back
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅃ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅉ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㄸ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㄲ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅆ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅛ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅕ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅑ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅒ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅖ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_Back;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅃ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅉ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㄸ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㄲ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅆ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅛ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅕ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅑ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅒ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅖ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_Back;

            i = 2; // Shift, ㅁ, ㄴ, ㅇ, ㄹ, ㅎ, ㅗ, ㅓ, ㅏ, ㅣ, 확인
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_Shift;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅁ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㄴ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅇ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㄹ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅎ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅗ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅓ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅏ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅣ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_확인;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_Shift;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅁ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㄴ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅇ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㄹ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅎ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅗ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅓ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅏ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅣ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_확인;

            i = 3; // 기호, 한영, ㅋ, ㅌ, ㅊ, Spacebar, ㅍ, ㅠ, ㅜ, ㅡ
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_기호;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_한영;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅋ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅌ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅊ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_Spacebar;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅍ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅠ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅜ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_normal_ㅡ;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_기호;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_한영;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅋ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅌ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅊ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_Spacebar;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅍ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅠ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅜ;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.ks_over_ㅡ;
            #endregion Korean(Double)

            #region English(Small)
            nType = (int)EKeyType._KEY_ENG;

            i = 1; // q, w, e, r, t, y, u, i, o, p, Back
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_q;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_w;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_e;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_r;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_t;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_y;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_u;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_i;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_o;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_p;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_back;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_q;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_w;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_e;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_r;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_t;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_y;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_u;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_i;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_o;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_p;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_back;

            i = 2; // Shift, a, s, d, f, g, h, j, k, l, 확인
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_shift;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_05_25;// es_nomal_a;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_07_26;//es_nomal_s;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_72;//es_nomal_d;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_74;//es_nomal_f;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_76;//es_nomal_g;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_78;//es_nomal_h;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_80;//es_nomal_j;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_81;//es_nomal_k;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_83;//es_nomal_l;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_확인;//es_nomal_확인;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_up;// es_over_shift;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_a;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_s;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_d;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_f;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_g;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_h;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_j;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_k;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_l;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_확인;

            i = 3; // 기호, 한영, z, x, c, Spacebar, v, b, n, m
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_03_35;// es_nomal_기호;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_05_36;//es_nomal_한영;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_z;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_x;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_c;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_spacebar;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_v;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_b;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_n;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_m;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_기호;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_한영;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_z;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_x;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_c;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_spacebar;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_v;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_b;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_n;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_m;
            #endregion English(Small)

            #region English(Large)
            nType = (int)EKeyType._KEY_ENG_LARGE;

            i = 1; // q, w, e, r, t, y, u, i, o, p, Back
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_Q;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_W;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_E;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_R;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_T;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_Y;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_U;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_I;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_O;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_P;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_Back;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_Q;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_W;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_E;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_R;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_T;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_Y;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_U;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_I;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_O;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_P;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_Back;

            i = 2; // Shift, a, s, d, f, g, h, j, k, l, 확인
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_Shift;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_A;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_S;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_D;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_F;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_G;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_H;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_J;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_K;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_L;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_nomal_확인;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_Shift;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_A;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_S;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_D;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_F;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_G;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_H;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_J;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_K;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_L;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.es_over_확인;

            i = 3; // 기호, 한영, z, x, c, Spacebar, v, b, n, m
            j = 0; nOver = 0;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_기호;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_한영;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_Z;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_X;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_C;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_Spacebar;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_V;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_B;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_N;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_normal_M;

            j = 0; nOver = 1;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_기호;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_한영;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_Z;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_X;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_C;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_Spacebar;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_V;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_B;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_N;
            m_aImage[i, j++, nType, nOver] = global::OpenJigWare.Properties.Resources.eL_over_M;
            #endregion English(Large)
        }
        private void SetDisplay(int nType)
        {
            if (
                ((m_nType == (int)EKeyType._KEY_KOR) && (nType == (int)EKeyType._KEY_KOR_DOUBLE)) ||
                ((m_nType == (int)EKeyType._KEY_KOR_DOUBLE) && (nType == (int)EKeyType._KEY_KOR))
            )
            {
                // 이경우에는 메모리 클리어를 하지 않도록 한다.
            }
            else
            {
                OjwClearMem();
                OjwClearMem2();
            }
            m_nType = nType;

            #region Key_Num
            int i = 0;
            int j = 0;
            #endregion

            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 11; j++)
                {
                    if ((i == 3) && (j == 10))
                    {
                    }
                    else
                    {
                        if (((nType == (int)EKeyType._KEY_ENG_LARGE) || (nType == (int)EKeyType._KEY_KOR_DOUBLE)) && (i == 2) && (j == 0))
                        {
                            m_albButton[i, j].Image = m_aImage[i, j, nType, 1];
                        }
                        else
                            m_albButton[i, j].Image = m_aImage[i, j, nType, 0];
                    }
                }
            }
        }
        private static Form m_frmMain = null;
        private String m_strWorkPath = "";
        private static IntPtr m_hHandle = IntPtr.Zero;
        private void Init()
        {
            #region 버튼 매핑
            for (int i = 0; i < _CNT_LINE; i++)
                for (int j = 0; j < _CNT_NUM; j++)
                {
                    if ((i == _CNT_LINE - 1) && (j == _CNT_NUM - 1))
                    {
                        m_albButton[i, j] = new Label();
                    }
                    else
                    {
                        String strName = "lb_" + i.ToString() + "_" + j.ToString();
                        m_albButton[i, j] = (Label)(this.Controls.Find(strName, true)[0]);
                    }
                }

            // CapsLock 이 눌렸다면 해제
            if ((GetKeyState((int)Keys.CapsLock) & 0xffff) != 0)
            {
                keybd_event((byte)Keys.CapsLock, (byte)0, 0, 0);
                keybd_event((byte)Keys.CapsLock, (byte)0, 2, 0);
            }

            InitParseImage();


            SetDisplay((int)((m_bEnableHangul == true) ? EKeyType._KEY_KOR : EKeyType._KEY_ENG));
            #endregion 버튼 매핑
        }
        private void frmKeyPad_Load(object sender, EventArgs e)
        {
            m_frmMain = this;// Form.ActiveForm;
            m_hHandle = m_frmMain.Handle;
            m_strWorkPath = Application.StartupPath;

            Init();

            SetHook_Mouse(true); // 마우스 이벤트 활성화
            SetHook_Keyboard(true); // 키보드 이벤트 활성화
        }
        #region Windows Message 활용을 위한 DLL Load
        //private const int WM_KEYDOWN = 0x0100;
        //private const int WM_KEYUP = 0x0101;
        private const int WM_CHAR = 0x0102;
        private const int WM_IME_STARTCOMPOSITION = 0x010D;
        private const int WM_IME_COMPOSITION = 0x010F;
        private const int WM_IME_ENDCOMPOSITION = 0x010E;
        private const int WM_IME_SETCONTEXT = 0x0281;
        private const int WM_IME_NOTIFY = 0x0282;
        private const int WM_IME_CONTROL = 0x0283;
        private const int WM_IME_COMPOSITIONFULL = 0x0284;
        private const int WM_IME_SELECT = 0x0285;
        private const int WM_IME_CHAR = 0x0286;
        private const int WM_IME_KEYDOWN = 0x0290;
        private const int WM_IME_KEYUP = 0x0291;
        private const int WM_IME_REPORT = 0x0280;
        private const int WM_IME_REQUEST = 0x0288;
        //private const int WM_MOUSEACTIVATE = 0x0021;
        //private const int MA_NOACTIVATEANDEAT = 0x0004;
        //private const int WM_IME_ENDCOMPOSITION = 0x10E;
        private const uint ISC_SHOWUIALL = 0xC000000F;
        private const uint ISC_SHOWUIALLCANDIDATEWINDOW = 0xF;
        private const uint ISC_SHOWUICANDIDATEWINDOW = 0x1;
        private const uint ISC_SHOWUICOMPOSITIONWINDOW = 0x80000000;
        private const uint ISC_SHOWUIGUIDELINE = 0x40000000;

        private const int WM_APPCOMMAND = 0x319;
        //private const int APPCOMMAND_VOLUME_UP = 10;
        //private const int APPCOMMAND_VOLUME_DOWN = 9;
        //private const int APPCOMMAND_VOLUME_MUTE = 8; 

        //public const int KEYEVENT_KEYDOWN = 0x00;
        //public const int KEYEVENT_EXTENDEDKEY = 0x1;
        //public const int KEYEVENT_KEYUP = 0x02;

        public bool PreFilterMessage(ref Message m)
        {

            // TODO:  MyMessageFilter.PreFilterMessage 구현을 추가합니다.            
            switch (m.Msg)
            {
                case WM_IME_STARTCOMPOSITION:
                    OjwMessage("한글 조합 시작.." + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_COMPOSITION:
                    OjwMessage("한글 입력 중.." + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_ENDCOMPOSITION:
                    OjwMessage("한글 조합 완료.." + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_SETCONTEXT:
                    OjwMessage("WM_IME_SETCONTEXT" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_NOTIFY:
                    OjwMessage("WM_IME_NOTIFY" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_CONTROL:
                    OjwMessage("WM_IME_CONTROL" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_COMPOSITIONFULL:
                    OjwMessage("WM_IME_COMPOSITIONFULL" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_SELECT:
                    OjwMessage("WM_IME_SELECT" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_CHAR:
                    OjwMessage("WM_IME_CHAR" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_KEYDOWN:
                    OjwMessage("WM_IME_KEYDOWN" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_KEYUP:
                    OjwMessage("WM_IME_KEYUP" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_REPORT:
                    OjwMessage("WM_IME_REPORT" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_IME_REQUEST:
                    OjwMessage("WM_IME_REQUEST" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                case WM_KEYDOWN:
                    OjwMessage("WM_KEYDOWN" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                /*
                case WM_KEYUP:
                    OjwMessage("WM_KEYUP" + "\t"+ m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
                */
                case WM_CHAR:
                    OjwMessage("WM_CHAR" + "\t" + m.WParam.ToString() + "\t" + m.LParam.ToString());
                    break;
            }
            return false;
        }
        #endregion Windows Message 활용을 위한 DLL Load


        private void lb_2_10_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{ENTER}");
        }

        private void lb_2_0_Click(object sender, EventArgs e)
        {
            if (m_nType == (int)EKeyType._KEY_ENG)
            {
                SetDisplay((int)EKeyType._KEY_ENG_LARGE);
            }
            else if (m_nType == (int)EKeyType._KEY_ENG_LARGE)
            {
                SetDisplay((int)EKeyType._KEY_ENG);
            }
            else if (m_nType == (int)EKeyType._KEY_KOR)
            {
                SetDisplay((int)EKeyType._KEY_KOR_DOUBLE);
            }
            else if (m_nType == (int)EKeyType._KEY_KOR_DOUBLE)
            {
                SetDisplay((int)EKeyType._KEY_KOR);
            }
        }

        private void lb_1_10_Click(object sender, EventArgs e)
        {

        }

        private void lb_3_0_Click(object sender, EventArgs e)
        {
            SetDisplay((int)EKeyType._KEY_ASCII);
        }

        private bool m_bEnableHangul = true; // 0 : Korean, 1 : English
        public void EnableHangul(bool bMode)
        {
            m_bEnableHangul = bMode;
            lb_3_0.Visible = bMode;
            //if ((m_nType == (int)EKeyType._KEY_KOR) || (m_nType == (int)EKeyType._KEY_KOR_DOUBLE))
            //{
            //    SetDisplay((int)EKeyType._KEY_ENG);
            //}
        }
        private void lb_3_1_Click(object sender, EventArgs e)
        {
            if (m_bEnableHangul == false)
            {
                if (m_nType == (int)EKeyType._KEY_ASCII)
                {
                    SetDisplay((int)EKeyType._KEY_ENG);
                }
                else
                {
                    SetDisplay((int)EKeyType._KEY_ASCII);
                }
            }
            else
            {
                if ((m_nType == (int)EKeyType._KEY_ENG) || (m_nType == (int)EKeyType._KEY_ENG_LARGE))
                {
                    SetDisplay((int)EKeyType._KEY_KOR);
                }
                //else if (m_nType == (int)EKeyType._KEY_ASCII)
                //{
                //    SetDisplay((int)EKeyType._KEY_ENG);
                //}
                else
                {
                    SetDisplay((int)EKeyType._KEY_ENG);
                }
            }
        }
        #region NoActive 2/2
        // The WS_EX_NOACTIVATE value for dwExStyle prevents foreground
        // activation by the system.
        const long WS_EX_NOACTIVATE = 0x08000000L;
        protected override CreateParams CreateParams
        {
            [PermissionSetAttribute(SecurityAction.LinkDemand, Name = "FullTrust")]
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= (int)WS_EX_NOACTIVATE;
                return cp;
            }
        }
        #endregion NoActive 2/2

        private bool m_bMouseClick = false;
        private Point m_pntMouse_Main;
        private void frmKeyPad_MouseDown(object sender, MouseEventArgs e)
        {
            m_bMouseClick = true;
            m_pntMouse_Main.X = e.X;
            m_pntMouse_Main.Y = e.Y;
        }

        private void frmKeyPad_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseClick == true)
            {
                int nGap_X = m_pntMouse_Main.X - e.X;
                int nGap_Y = m_pntMouse_Main.Y - e.Y;
                this.Left -= nGap_X;
                this.Top -= nGap_Y;
            }
        }

        private void frmKeyPad_MouseUp(object sender, MouseEventArgs e)
        {
            m_bMouseClick = false;
        }

        private void frmKeyPad_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_bProgEnd = true;
            if (m_bCloseEvent_ApplicationExit == true)
                Application.Exit();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            m_bProgEnd = true;
            //Application.Exit();
            this.Close();
        }
    }

    #region NoActive 1/2
    internal static class UnsafeNativeMethods
    {
        /// <summary>
        /// Retrieve a handle to the foreground window.
        /// http://msdn.microsoft.com/en-us/library/ms633505(VS.85).aspx
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Bring the thread that created the specified window into the foreground
        /// and activates the window. 
        /// http://msdn.microsoft.com/en-us/library/ms633539(VS.85).aspx
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Determine whether the specified window handle identifies an existing window. 
        /// http://msdn.microsoft.com/en-us/library/ms633528(VS.85).aspx
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IsWindow(IntPtr hWnd);
    }
    #endregion NoActive 1/2
}
