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
        // if you make your class, just write in here
        public class CKeyboard
        {
            private const int _KEY_DOWN = 0;
            private const int _KEY_UP = 2;
                //SendKeys.Send("{ENTER}");
                //keybd_event((byte)Keys.CapsLock, (byte)0, 0, 0);
                //keybd_event((byte)Keys.CapsLock, (byte)0, 2, 0);
            [DllImport("user32.dll")]                           private static extern uint keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

            public static void Key_Down(Keys keyValue)
            {
                keybd_event((byte)keyValue, (byte)0, _KEY_DOWN, 0);
            }
        
            public static void Key_Up(Keys keyValue)
            {
                keybd_event((byte)keyValue, (byte)0, _KEY_UP, 0);
            }
    
            public static void Key_Click(Keys keyValue)
            {
                keybd_event((byte)keyValue, (byte)0, _KEY_DOWN, 0);
                keybd_event((byte)keyValue, (byte)0, _KEY_UP, 0);             
            }


            ///////////////////////////////////////////////////////////////////////////////////////////
            //private static delegate void UserFunction(object sender, EventArgs args);
            public delegate void UserFunction(object sender, CUserEventArgs e);
            private static UserFunction m_FUser_Mouse, m_FUser_Keyboard;
            public static void Keyboard_InitEvent(UserFunction FUser)
            {
                if (FUser != null)
                {
                    m_FUser_Keyboard = new UserFunction(FUser);
                    Event_Keyboard.ClearEvent();
                    Event_Keyboard.UserEventForArgs += new EventHandler<CUserEventArgs>(m_FUser_Keyboard);
                }
                SetHook_Keyboard(true);
            }
            public static void Keyboard_DInit()
            {
                Event_Keyboard.ClearEvent();
            }
            
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
            
            private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);
            private static LowLevelProc m_proc_keyboard = HookCallback_KeyBoard;
            private static IntPtr m_hookID_KeyBoard = IntPtr.Zero;
           
            private static Ojw.CUserEvent Event_Keyboard = new Ojw.CUserEvent();
            private static bool m_bEvent_Keyboard = false;

            private const int WH_KEYBOARD_LL = 13;
            private const int WH_MOUSE_LL = 14;
           
            private static IntPtr SetHook(bool bKeyBoard, LowLevelProc proc)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule) { return SetWindowsHookEx(((bKeyBoard == true) ? WH_KEYBOARD_LL : WH_MOUSE_LL), proc, GetModuleHandle(curModule.ModuleName), 0); }
            }
            public static void SetHook_Keyboard(bool bOn)
            {
                m_hookID_KeyBoard = SetHook(true, m_proc_keyboard);
                m_bEvent_Keyboard = true;
            }
            
            private static IntPtr HookCallback_KeyBoard(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0)
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    //if (((Keys)vkCode == System.Windows.Forms.Keys.F7) && (Keys.Control == Control.ModifierKeys))
                    //{
                    //    Ojw.printf("F7이 눌렸습니다.\r\n");
                    //}
                    //else Ojw.printf("Key = {0}\r\n", vkCode.ToString());
                    if (m_bEvent_Keyboard == true)
                        Event_Keyboard.RunEvent("<Keyboard>", (int)wParam, vkCode);
                }
                return CallNextHookEx(m_hookID_KeyBoard, nCode, wParam, lParam);
            }            
        }
    }
}
