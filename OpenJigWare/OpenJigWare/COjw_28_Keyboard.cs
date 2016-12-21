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
        }
    }
}
