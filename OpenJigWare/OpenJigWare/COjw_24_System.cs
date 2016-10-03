using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace OpenJigWare
{
    partial class Ojw
    {
        // if you make your class, just write in here
        public class CSystem
        {
            [DllImport("kernel32.dll")]
            public static extern bool IsWow64Process(System.IntPtr hProcess, out bool lpSystemInfo);
            
            [DllImport("user32.dll", EntryPoint = "SetWindowText")]
            private static extern int SetWindowText(IntPtr hWnd, string text);

            [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
            private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            [DllImport("User32.dll", EntryPoint = "SendMessage")]
            private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

            public static bool Is64Bits_byApi()
            {                
                bool retVal = false;
                IsWow64Process(System.Diagnostics.Process.GetCurrentProcess().Handle, out retVal);
                return retVal;
            }
            public static bool Is32Bits_byApi()
            {
                bool retVal = false;
                IsWow64Process(System.Diagnostics.Process.GetCurrentProcess().Handle, out retVal);
                return !retVal;
            }

            public static bool Is64Bits()
             {
                 if ((IntPtr.Size == 8) || ((IntPtr.Size == 4) && (Is64Bits_byApi() == true))) return true;
                 return false;
             }
            public static bool Is32Bits() { return !Is64Bits(); }

            public static bool IsRunningProgram(String strProgram)
            {
                #region 중복실행 체크
                System.Diagnostics.Process ps = new System.Diagnostics.Process();
                String strTitle = Ojw.CFile.GetTitle(strProgram);
                Process[] processes = System.Diagnostics.Process.GetProcesses();//System.Diagnostics.Process.GetProcessesByName(Ojw.CFile.GetTitle(strProgram));
                bool bStarted = false;
                //foreach (System.Diagnostics.Process process in processes) { if (process.MainWindowTitle == strProgram) bStarted = true; }
                foreach (System.Diagnostics.Process process in processes) { if (process.ProcessName == strTitle) bStarted = true; }
                if (bStarted == true)
                {
                    Ojw.CMessage.Write("[warning]Still program is running... Can't run it. Check process first");
                    //MessageBox.Show(Ojw.CMessage.GetLastErrorMessage());
                    //Application.Exit();
                    return true; // Error
                }
                else
                {
                    Ojw.CMessage.Write("Program duplication checking : OK");
                }
                return false;
                #endregion 중복실행 체크
            }

            // nRunningMode == 0 : Normal(중복허용, 안죽임), 1 : killothers(다른것 다 죽이고 혼자 살아남음), 2 : 중복시 안띄움)
            public static void RunProgram(string strProgram, int nRunningMode) { RunProgram(strProgram, null, nRunningMode); }
            public static void RunProgram(string strProgram, string strArgument, int nRunningMode)
            {
                //string strFileName = Ojw.CFile.GetTitle(strProgram);
                System.Diagnostics.Process ps = new System.Diagnostics.Process();
                String strTitle = Ojw.CFile.GetTitle(strProgram);
                //this.Cursor = Cursors.Hand;
                Process[] processes = System.Diagnostics.Process.GetProcesses();//GetProcessesByName(strProgram);

                // 동일한 이름을 가진 Process를 모두 kill함.
                bool bRunning = false;
                foreach (System.Diagnostics.Process process in processes) { if (process.ProcessName == strTitle) { if (nRunningMode == 1) process.CloseMainWindow(); else bRunning = true; } }
                if (bRunning == true)
                {
                    if (nRunningMode != 2)
                    {
                        ps.StartInfo.FileName = strProgram;
                        if (strArgument != null) ps.StartInfo.Arguments = strArgument;
                        ps.Start();
                    }
                }
                else
                {
                    ps.StartInfo.FileName = strProgram;
                    if (strArgument != null) ps.StartInfo.Arguments = strArgument;
                    ps.Start();
                }
                

                if (bRunning == true)
                {

                }
            }

            // 출처 : http://stackoverflow.com/questions/7613576/how-to-open-text-in-notepad-from-net
            public static void SendText_To_Program(string strProgram, string strText = null, string strTitle = null)
            {
                Process Pcs = Process.Start(new ProcessStartInfo(strProgram));
                if (Pcs != null)
                {
                    Pcs.WaitForInputIdle();

                    if (!string.IsNullOrEmpty(strTitle))
                        SetWindowText(Pcs.MainWindowHandle, strTitle);

                    if (!string.IsNullOrEmpty(strText))
                    {
                        IntPtr child = FindWindowEx(Pcs.MainWindowHandle, new IntPtr(0), "Edit", null);
                        SendMessage(child, 0x000C, 0, strText);
                    }
                }
            }
        }
    }
}
