using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
#if _USING_DOTNET_3_5
#elif _USING_DOTNET_2_0
#else
using System.IO.MemoryMappedFiles;
#endif
using System.IO;
using System.Threading;
using Microsoft.Win32;
using System.Net;
using SKYPE4COMLib;

namespace OpenJigWare
{
    partial class Ojw
    {
        // if you make your class, just write in here
        public class CSystem
        {
            
            public const int WM_COPYDATA = 0x4A;


            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, ref COPYDATASTRUCT lParam);
            //[DllImport("user32.dll", EntryPoint = "SendMessage")]//, CharSet = CharSet.Auto)]
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, byte [] lParam);
            //[DllImport("user32.dll", CharSet = CharSet.Ansi)]
            //static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPStr)] String lParam);
            //[DllImport("user32.dll", SetLastError = true)]
            //static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

            [DllImport("kernel32.dll")]
            public static extern bool IsWow64Process(System.IntPtr hProcess, out bool lpSystemInfo);
            
            [DllImport("user32.dll", EntryPoint = "SetWindowText")]
            private static extern int SetWindowText(IntPtr hWnd, string text);

            [DllImport("user32.dll")]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [DllImportAttribute("user32.dll", EntryPoint = "FindWindowW")]
            public static extern System.IntPtr FindWindowW([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPTStr)] string lpClassName, [InAttribute()] [MarshalAsAttribute(UnmanagedType.LPTStr)] string lpWindowName);
            [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
            private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            [DllImport("User32.dll", EntryPoint = "SendMessage")]
            private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);
            
            [DllImport("user32.dll")]
            private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

#if false            
            public static void RunVirtualKeyboard(int nLeft, int nTop, int nRight, int nBottom)
            {
                IntPtr MainWindowHandle = FindWindowW("OSKMainClass","On-Screen Keyboard");
                bool success = SetWindowPos(MainWindowHandle, IntPtr.Zero, 0, 200, 500, 600, 0);
            }
#else
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

            #region ToolTip
            public static ToolTip m_tpToolTip = new ToolTip();
            public static void SetToolTip_Prop(Control ctrl, ToolTipIcon tpIcon, bool bBalloon, bool bShowAlways)
            {
                m_tpToolTip = new ToolTip();
                //m_tpToolTip.ToolTipTitle = strTitle;
                if (tpIcon != null) m_tpToolTip.ToolTipIcon = ToolTipIcon.Info;
                m_tpToolTip.IsBalloon = false;// bBalloon;
                //tp.De
                //m_tpToolTip.ReshowDelay = 1000;
                m_tpToolTip.ShowAlways = false;// bShowAlways;
            }
            public static void SetToolTip(Control ctrl, string strToolTip)
            {
                //ToolTip tp = new ToolTip();
                //tp.ToolTipTitle = strTitle;
                //tp.ToolTipIcon = ToolTipIcon.Info;
                //tp.IsBalloon = false;// bBalloon;
                //tp.De
                //tp.ReshowDelay = 1000;
                //tp.ShowAlways = false;// bShowAlways;
                //m_tpToolTip.Show(strToolTip, ctrl);
                m_tpToolTip.SetToolTip(ctrl, strToolTip);
            }
            public static void ShowToolTip(Control ctrl)
            {
                ToolTip tp = m_tpToolTip;
                tp.ShowAlways = false;
                tp.AutomaticDelay = 100;
                m_tpToolTip.Show(m_tpToolTip.GetToolTip(ctrl), ctrl);
            }
            public static void ShowToolTip(Control ctrl, string strToolTip)
            {
                //ToolTip tp = new ToolTip();
                //tp.ToolTipTitle = strTitle;
                //tp.ToolTipIcon = ToolTipIcon.Info;
                //tp.IsBalloon = false;// bBalloon;
                //tp.De
                //tp.ReshowDelay = 1000;
                //tp.ShowAlways = false;// bShowAlways;
                m_tpToolTip.Show(strToolTip, ctrl);
                //m_tpToolTip.SetToolTip(ctrl, strToolTip);
            }
            public static void SetToolTip(Control ctrl, string strTitle, string strTooltip)//, bool bBalloon, bool bShowAlways)
            {
                //ToolTip tp = new ToolTip();
                m_tpToolTip.ToolTipTitle = strTitle;
                //tp.ToolTipIcon = ToolTipIcon.Info;
                //tp.IsBalloon = false;// bBalloon;
                //tp.De
                //tp.ReshowDelay = 1000;
                //tp.ShowAlways = false;// bShowAlways;

                m_tpToolTip.SetToolTip(ctrl, strTooltip);
            }
            #endregion ToolTip

            public static void ScreenKeyboard()
            {
                m_ps = GetProcess("osk");
                if (m_ps != null) ScreenKeyboard_Kill();
                //else { KillProgram("osk"); }

                //if (m_ps != null) ScreenKeyboard_Kill();
                //else { KillProgram("osk"); }
                try { if (m_ps != null) m_ps.Dispose(); }
                catch (Exception ex) { Ojw.CMessage.Write_Error(ex.ToString()); }
                m_ps = null;
                string strPath = String.Format(@"{0}\System32\osk.exe", Ojw.CSystem.GetPath_Windows());
                m_ps = Ojw.CSystem.RunProgram(strPath);
                //ps.Dispose();
                //return ps;
            }
            public static void ScreenKeyboard(int nLeft, int nTop)
            {
                RegistryKey myKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Osk", true);

                myKey.SetValue("WindowLeft", nLeft, RegistryValueKind.DWord);
                myKey.SetValue("WindowTop", nTop, RegistryValueKind.DWord);

                m_ps = GetProcess("osk");
                if (m_ps != null) ScreenKeyboard_Kill();
                //else { KillProgram("osk"); }

                ////ScreenKeyboard_Kill();
                //if (m_ps != null) ScreenKeyboard_Kill();
                //else { KillProgram("osk"); }//Thread.Sleep(100); }
                try { if (m_ps != null) { m_ps.Dispose(); } } 
                catch (Exception ex) { Ojw.CMessage.Write_Error(ex.ToString()); }
                m_ps = null;

                string strPath = String.Format(@"{0}\System32\osk.exe", Ojw.CSystem.GetPath_Windows());
                m_ps = Ojw.CSystem.RunProgram(strPath);
                //ps.Dispose();
                //return ps;
            }
            private static Process m_ps = null;//new Process();
            public static void ScreenKeyboard_Kill() { KillProgram(m_ps); m_ps.Dispose(); }
            public static void ScreenKeyboard(int nLeft, int nTop, int nWidth, int nHeight)
            {
                RegistryKey myKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Osk", true);

                myKey.SetValue("WindowWidth", nWidth, RegistryValueKind.DWord);
                myKey.SetValue("WindowHeight", nHeight, RegistryValueKind.DWord);
                myKey.SetValue("WindowLeft", nLeft, RegistryValueKind.DWord);
                myKey.SetValue("WindowTop", nTop, RegistryValueKind.DWord);
#if false
#else
                m_ps = GetProcess("osk");
                if (m_ps != null) ScreenKeyboard_Kill();
                //else { KillProgram("osk"); }
                try { if (m_ps != null) m_ps.Dispose(); }
                catch (Exception ex) { Ojw.CMessage.Write_Error(ex.ToString()); }
                m_ps = null;
                            
                string strPath = String.Format(@"{0}\System32\osk.exe", Ojw.CSystem.GetPath_Windows());
                m_ps = Ojw.CSystem.RunProgram(strPath);
                //ps.Dispose();
                //return ps;
#endif
             }

            [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
            static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
            public enum WMessages : int
            {
                WM_LBUTTONDOWN = 0x201,
                WM_LBUTTONUP = 0x202,
                WM_KEYDOWN = 0x100,
                WM_KEYUP = 0x101,
                WH_KEYBOARD_LL = 13,
                WH_MOUSE_LL = 14,
            }
            public static bool IsScreenKeyboard2()
            {
                return IsRunningProgram(@"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe");
            }
            public static void Kill_ScreenKeyboard2()
            {
                KillProgram(@"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe");
            }
            public static void ScreenKeyboard2(bool bShow)
            {
                var trayWnd = FindWindow("Shell_TrayWnd", null);
                var nullIntPtr = new IntPtr(0);

                if (trayWnd != nullIntPtr)
                {
                    var trayNotifyWnd = FindWindowEx(trayWnd, nullIntPtr, "TrayNotifyWnd", null);

                    if (trayNotifyWnd != nullIntPtr)
                    {
                        if (bShow == true)
                        {
                            var tIPBandWnd = FindWindowEx(trayNotifyWnd, nullIntPtr, "TIPBand", null);
                            if (tIPBandWnd != nullIntPtr)
                            {
                                PostMessage(tIPBandWnd, (UInt32)WMessages.WM_LBUTTONDOWN, 1, 65537);
                                PostMessage(tIPBandWnd, (UInt32)WMessages.WM_LBUTTONUP, 1, 65537);
                            }
                            else
                            {
                                RunProgram(@"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe");
                                PostMessage(tIPBandWnd, (UInt32)WMessages.WM_LBUTTONDOWN, 1, 65537);
                                PostMessage(tIPBandWnd, (UInt32)WMessages.WM_LBUTTONUP, 1, 65537);
                                Ojw.CTimer CTmr = new CTimer();
                                CTmr.Set();
                                while (CTmr.Get() < 1000)
                                {
                                    tIPBandWnd = FindWindowEx(trayNotifyWnd, nullIntPtr, "TIPBand", null);

                                    if (tIPBandWnd != nullIntPtr)
                                    {
                                        Ojw.CTimer.Wait(100);
                                        PostMessage(tIPBandWnd, (UInt32)WMessages.WM_LBUTTONDOWN, 1, 65537);
                                        PostMessage(tIPBandWnd, (UInt32)WMessages.WM_LBUTTONUP, 1, 65537);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var tIPBandWnd = FindWindowEx(trayNotifyWnd, nullIntPtr, "TIPBand", null);
                            if (tIPBandWnd != nullIntPtr)
                            {
                                PostMessage(tIPBandWnd, (UInt32)WMessages.WM_LBUTTONDOWN, 1, 65537);
                                PostMessage(tIPBandWnd, (UInt32)WMessages.WM_LBUTTONUP, 1, 65537);
                            }
                        }
                    }
                }

            }
#endif
            #region 하드디스크 사용량 구하기 출처 : http://icodebroker.tistory.com/1447
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern bool GetDiskFreeSpaceEx(
                string directory,                   // 디렉토리
                out ulong availableFreeByteCount,   // 사용 가능 용량
                out ulong totalByteCount,           // 전체 용량
                out ulong totalFreeByteCount        // 전체 잔여 용량
            );
            public static ulong GetHdd_Space_Total(string strDrive) { ulong  availableFreeByteCount, totalByteCount, totalFreeByteCount; if (GetDiskFreeSpaceEx(strDrive, out availableFreeByteCount, out totalByteCount, out totalFreeByteCount) == false) return 0; return totalByteCount; }
            public static ulong GetHdd_Space_TotalFree(string strDrive) { ulong  availableFreeByteCount, totalByteCount, totalFreeByteCount; if (GetDiskFreeSpaceEx(strDrive, out availableFreeByteCount, out totalByteCount, out totalFreeByteCount) == false) return 0; return totalFreeByteCount; }
            public static ulong GetHdd_Space_Used(string strDrive) { ulong availableFreeByteCount, totalByteCount, totalFreeByteCount; if (GetDiskFreeSpaceEx(strDrive, out availableFreeByteCount, out totalByteCount, out totalFreeByteCount) == false) return 0; return (totalByteCount - totalFreeByteCount); }

            public static ulong GetHdd_Space_Total_GiGa(string strDrive) { ulong availableFreeByteCount, totalByteCount, totalFreeByteCount; if (GetDiskFreeSpaceEx(strDrive, out availableFreeByteCount, out totalByteCount, out totalFreeByteCount) == false) return 0; return (totalByteCount / 1000000000); }
            public static ulong GetHdd_Space_TotalFree_GiGa(string strDrive) { ulong availableFreeByteCount, totalByteCount, totalFreeByteCount; if (GetDiskFreeSpaceEx(strDrive, out availableFreeByteCount, out totalByteCount, out totalFreeByteCount) == false) return 0; return (totalFreeByteCount / 1000000000); }
            public static ulong GetHdd_Space_Used_GiGa(string strDrive) { ulong availableFreeByteCount, totalByteCount, totalFreeByteCount; if (GetDiskFreeSpaceEx(strDrive, out availableFreeByteCount, out totalByteCount, out totalFreeByteCount) == false) return 0; return ((totalByteCount - totalFreeByteCount) / 1000000000); }
            
            public static ulong GetHdd_Space_Total_MeGa(string strDrive) { ulong availableFreeByteCount, totalByteCount, totalFreeByteCount; if (GetDiskFreeSpaceEx(strDrive, out availableFreeByteCount, out totalByteCount, out totalFreeByteCount) == false) return 0; return (totalByteCount / 1000000); }
            public static ulong GetHdd_Space_TotalFree_MeGa(string strDrive) { ulong availableFreeByteCount, totalByteCount, totalFreeByteCount; if (GetDiskFreeSpaceEx(strDrive, out availableFreeByteCount, out totalByteCount, out totalFreeByteCount) == false) return 0; return (totalFreeByteCount / 1000000); }
            public static ulong GetHdd_Space_Used_MeGa(string strDrive) { ulong availableFreeByteCount, totalByteCount, totalFreeByteCount; if (GetDiskFreeSpaceEx(strDrive, out availableFreeByteCount, out totalByteCount, out totalFreeByteCount) == false) return 0; return ((totalByteCount - totalFreeByteCount) / 1000000); }
            #endregion 하드디스크 사용량 구하기 출처 : http://icodebroker.tistory.com/1447

            //string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            //string programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
            public static string GetPath_ProgramFiles() { return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).TrimEnd('\\'); }
#if _USING_DOTNET_3_5
#else
            public static string GetPath_ProgramFilesX86() { return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).TrimEnd('\\'); }
#endif
            public static string GetPath_Windows() { return Environment.GetEnvironmentVariable("windir"); }
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
            public static IntPtr GetProgram(string strProgram)
            {
                System.Diagnostics.Process ps = new System.Diagnostics.Process();
                //String strTitle = Ojw.CFile.GetTitle(strProgram);
                Process[] processes = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process process in processes) { if (process.ProcessName.ToLower() == strProgram.ToLower()) return process.MainWindowHandle; }
                return IntPtr.Zero;
            }
            public static Process GetProcess(string strProgram)
            {
                System.Diagnostics.Process ps = new System.Diagnostics.Process();
                //String strTitle = Ojw.CFile.GetTitle(strProgram);
                Process[] processes = System.Diagnostics.Process.GetProcesses();
                foreach (System.Diagnostics.Process process in processes) { if (process.ProcessName.ToLower() == strProgram.ToLower()) return process; }
                return null;
            }
            public static bool IsRunningProgram(String strProgram)
            {
                #region 중복실행 체크
                System.Diagnostics.Process ps = new System.Diagnostics.Process();
                String strTitle = Ojw.CFile.GetTitle(strProgram);
                Process[] processes = System.Diagnostics.Process.GetProcesses();//System.Diagnostics.Process.GetProcessesByName(Ojw.CFile.GetTitle(strProgram));
                bool bStarted = false;
                //foreach (System.Diagnostics.Process process in processes) { if (process.MainWindowTitle == strProgram) bStarted = true; }
                foreach (System.Diagnostics.Process process in processes) { if (process.ProcessName.ToLower() == strTitle.ToLower()) bStarted = true; }
                if (bStarted == true)
                {
                    //Ojw.CMessage.Write("[warning]Still program is running... Can't run it. Check process first");
                    //MessageBox.Show(Ojw.CMessage.GetLastErrorMessage());
                    //Application.Exit();
                    return true; // Error
                }
                else
                {
                    //Ojw.CMessage.Write("Program duplication checking : OK");
                }
                return false;
                #endregion 중복실행 체크
            }
            public static bool IsRunningProgram(Process ps)
            {
                #region 중복실행 체크
                if (ps != null)
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
            public static bool IsRunningProgram(IntPtr Handle)
            {
                #region 중복실행 체크

                IntPtr handle = FindWindowEx(Handle, new IntPtr(0), null, null);
                if (handle != null)
                //if (handle.ToInt32() > 0)
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
            public struct COPYDATASTRUCT
            {
                public IntPtr dwData;
                public int cbData;
                [MarshalAs(UnmanagedType.LPStr)]
                public string lpData;
            }
            [StructLayout(LayoutKind.Sequential)]
            public struct PCOPYDATASTRUCT
            {
                public IntPtr dwData;
                public int cbData;
                public IntPtr lpData;
            }
            public static bool SendMessage_Utf8(String strProgram, string strData)
            {
                COPYDATASTRUCT cds = new COPYDATASTRUCT();
                cds.dwData = IntPtr.Zero;
                //byte[] pbyte = CConvert.StrToBytes_UTF8(strData);
                byte[] pbyte = CConvert.StrToBytes_UTF8(strData);
                cds.cbData = pbyte.Length;// +1; //strData.Length + 1;
                cds.lpData = strData;

                System.Diagnostics.Process ps = new System.Diagnostics.Process();
                String strTitle = Ojw.CFile.GetName(strProgram); //Ojw.CFile.GetTitle(strProgram);
                Process[] processes = System.Diagnostics.Process.GetProcesses();
                bool bRunning = false;

                foreach (System.Diagnostics.Process process in processes)
                {

                    if (process.ProcessName.ToLower() == strTitle.ToLower())
                    {
                        SendMessage(process.MainWindowHandle, WM_COPYDATA, 0, ref cds);

                        bRunning = true;
                        //}
                        break;
                    }
                }
                return bRunning;
            }
            public static bool SendMessage(String strProgram, string strData)
            {
                COPYDATASTRUCT cds = new COPYDATASTRUCT();
                cds.dwData = IntPtr.Zero;
                //byte[] pbyte = CConvert.StrToBytes_UTF8(strData);
                byte[] pbyte = CConvert.StrToBytes(strData);
                cds.cbData = pbyte.Length;// +1; //strData.Length + 1;
                cds.lpData = strData;
                
                System.Diagnostics.Process ps = new System.Diagnostics.Process();
                String strTitle = Ojw.CFile.GetName(strProgram); //Ojw.CFile.GetTitle(strProgram);
                Process[] processes = System.Diagnostics.Process.GetProcesses();
                bool bRunning = false;

                foreach (System.Diagnostics.Process process in processes)
                {

                    if (process.ProcessName.ToLower() == strTitle.ToLower())
                    {
                        SendMessage(process.MainWindowHandle, WM_COPYDATA, 0, ref cds);

                        bRunning = true;
                        //}
                        break;
                    }
                }
                return bRunning;
            }
            public static bool SendMessage(String strProgram, uint uiMessageAddress, string strData)
            {
                return SendMessage(strProgram, uiMessageAddress, Ojw.CConvert.StrToBytes(strData));
            }
            public static bool SendMessage(String strProgram, uint uiMessageAddress, byte[] pData)
            {
                //String strTitle = Ojw.CFile.GetTitle(strProgram);
                //Process[] pro = Process.GetProcessesByName(strTitle);

                //SendMessage(pro[0].MainWindowHandle, uiMessageAddress, 0, ref cds);
                //return true;

                System.Diagnostics.Process ps = new System.Diagnostics.Process();
                String strTitle = Ojw.CFile.GetName(strProgram); //Ojw.CFile.GetTitle(strProgram);
                Process[] processes = System.Diagnostics.Process.GetProcesses();
                bool bRunning = false;

                foreach (System.Diagnostics.Process process in processes)
                {

                    if (process.ProcessName.ToLower() == strTitle.ToLower())
                    {
                        //IntPtr handle = FindWindowEx(process.MainWindowHandle, new IntPtr(0), strTitle, null);

                        //if (handle.ToInt32() > 0)
                        //{
                        //SendMessage(process.MainWindowHandle, uiMessageAddress, 0, pbyData);
                        SendMessage(process.MainWindowHandle, uiMessageAddress, 0, pData);

                        bRunning = true;
                        //}
                        break;
                    }
                }
                return bRunning;
            }
            //protected override void WndProc(ref Message m) { switch(m.Msg){} base.WndProc(ref m); } 에서 사용하는 함수
            public static bool CheckEvent_WndProc(Message m, out String strData)
            {
                bool bRet = false;
                strData = String.Empty;
                switch (m.Msg)
                {
                    case WM_COPYDATA:
                        //COPYDATASTRUCT cds = (COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT));
                        //strData = cds.lpData;
                        strData = ReceiveMsg(m);
                        bRet = true;
                        //Ojw.CMessage.Write(pbyteData);
                        //MessageBox.Show(pbyteData);//String.Format("{0}{1}{2}{3}{4}{5}{6}{7}", 
                        break;
                }
                return bRet;                
            }
            public static string ReceiveMsg(System.Windows.Forms.Message m)
            {
                PCOPYDATASTRUCT cds = new PCOPYDATASTRUCT();
                cds = (PCOPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(PCOPYDATASTRUCT));
                if (cds.cbData > 0)
                {
                    byte[] data = new byte[cds.cbData];
                    Marshal.Copy(cds.lpData, data, 0, cds.cbData);
                    int iMsgIndex = (int)cds.dwData;
                    //Ojw.CMessage.Write("{0}, {1}", Ojw.CConvert.BytesToStr(data), iMsgIndex.ToString());
                    m.Result = (IntPtr)1;
                    return Ojw.CConvert.BytesToStr(data);
                }
                return null;
            }
            public static string[] ReceiveMsg(System.Windows.Forms.Message m, char cSeparator, out string strCommand)
            {
                PCOPYDATASTRUCT cds = new PCOPYDATASTRUCT();
                cds = (PCOPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(PCOPYDATASTRUCT));
                if (cds.cbData > 0)
                {
                    byte[] data = new byte[cds.cbData];
                    Marshal.Copy(cds.lpData, data, 0, cds.cbData);
                    int iMsgIndex = (int)cds.dwData;
                    m.Result = (IntPtr)1;
                    strCommand = Ojw.CConvert.BytesToStr(data);
                    return strCommand.Split(cSeparator);
                }
                else strCommand = string.Empty;
                return null;
            }

            //[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
            //protected override void WndProc(ref Message m)
            //{
            //    base.WndProc(ref m);
            //}
            public static void CloseProgram(string strProgram)
            {
                try
                {
                    if (IsRunningProgram(strProgram) == true)
                    {
                        // 동일한 이름을 가진 Process를 모두 kill함.
                        String strTitle = Ojw.CFile.GetTitle(strProgram);
                        Process[] processes = System.Diagnostics.Process.GetProcesses();
                        foreach (System.Diagnostics.Process process in processes) { if (process.ProcessName.ToLower() == strTitle.ToLower()) { process.CloseMainWindow(); } }
                    }
                }
                catch (Exception ex)
                {
                    CMessage.Write_Error(ex.ToString());
                }
            }
            public static void KillProgram(string strProgram)
            {
                try
                {
                    if (IsRunningProgram(strProgram) == true)
                    {
                        // 동일한 이름을 가진 Process를 모두 kill함.
                        String strTitle = Ojw.CFile.GetTitle(strProgram);
                        Process[] processes = System.Diagnostics.Process.GetProcesses();
                        foreach (System.Diagnostics.Process process in processes) { if (process.ProcessName.ToLower() == strTitle.ToLower()) { process.Kill(); } }//{ process.CloseMainWindow(); } }
                    }
                }
                catch (Exception ex)
                {
                    CMessage.Write_Error(ex.ToString());
                }
            }
            public static void KillProgram(Process ps) { try { ps.Kill(); } catch (Exception ex) { CMessage.Write_Error(ex.ToString()); } }
            public static Process RunProgram(string strProgram) { return RunProgram(strProgram, null, 0); }
            // nRunningMode == 0 : Normal(중복허용, 안죽임), 1 : killothers(다른것 다 죽이고 혼자 살아남음), 2 : 중복시 안띄움)
            public static Process RunProgram(string strProgram, int nRunningMode) { return RunProgram(strProgram, null, nRunningMode); }
            public static Process RunProgram(string strProgram, string strArgument, int nRunningMode) { return RunProgram(IntPtr.Zero, strProgram, strArgument, nRunningMode); }

            public static Process RunProgram(IntPtr hWnd_Dest, string strProgram) { return RunProgram(hWnd_Dest, strProgram, null, 0); }
            // nRunningMode == 0 : Normal(중복허용, 안죽임), 1 : killothers(다른것 다 죽이고 혼자 살아남음), 2 : 중복시 안띄움)
            public static Process RunProgram(IntPtr hWnd_Dest, string strProgram, int nRunningMode) { return RunProgram(hWnd_Dest, strProgram, null, nRunningMode); }
            //public static Process RunProgram(IntPtr hWnd_Dest, string strProgram, string strArgument, int nRunningMode) { return RunProgram(hWnd_Dest, strProgram, null, nRunningMode); } 
            public static void WaitProgram(Process ps, int nWaitTime_ms_AfterRunning)
            {
                ps.WaitForInputIdle();
                ps.Dispose();
                if (nWaitTime_ms_AfterRunning > 0) Ojw.CTimer.Wait(nWaitTime_ms_AfterRunning);
            }
            public static Process RunProgram(IntPtr hWnd_Dest, string strProgram, string strArgument, int nRunningMode)
            {
#if true
                bool bRedirect = false;
                IntPtr ptr = new IntPtr();
                try
                {
                    // 출처 : http://blog.naver.com/PostView.nhn?blogId=koolsk8eryj&logNo=220169704224
                    //var query = from process in Process.GetProcesses()
                    //            where process.ProcessName == GetProcess("osk")
                    //            select process;
                    //var keyboardProcess = query.FirstOrDefault();
#if _USING_DOTNET_3_5
#else
                    if (System.Environment.Is64BitOperatingSystem)
                    {
                        bRedirect = Wow64DisableWow64FsRedirection(ref ptr);
                    }
#endif
#else                    
                try
                {
#endif

                    //string strFileName = Ojw.CFile.GetTitle(strProgram);
                    System.Diagnostics.Process ps = new System.Diagnostics.Process();
                    String strTitle = Ojw.CFile.GetTitle(strProgram);
                    //this.Cursor = Cursors.Hand;
                    Process[] processes = System.Diagnostics.Process.GetProcesses();//GetProcessesByName(strProgram);

                    // 동일한 이름을 가진 Process를 모두 kill함.
                    bool bRunning = false;
                    foreach (System.Diagnostics.Process process in processes) { if (process.ProcessName.ToLower() == strTitle.ToLower()) { if (nRunningMode == 1) process.Kill(); else bRunning = true; } }//process.CloseMainWindow(); else bRunning = true; } }
                    if (bRunning == true)
                    {
                        if (nRunningMode != 2)
                        {
                            ps.StartInfo.FileName = strProgram;
                            if (strArgument != null) ps.StartInfo.Arguments = strArgument;
                            ps.Start();
                            if (hWnd_Dest != IntPtr.Zero)
                            {
                                ps.Refresh();
                                if (ps.HasExited == true)
                                {
                                    return null;
                                }
                                try
                                {
                                    ps.WaitForInputIdle();
                                }
                                catch
                                {
                                    System.Threading.Thread.Sleep(500);
                                }

                                //IntPtr child = FindWindowEx(ps.MainWindowHandle, new IntPtr(0), null, null);
                                //SetParent(child, hWnd_Dest);
                                SetParent(ps.MainWindowHandle, hWnd_Dest);
                                //SetWindowPos(hWnd_Dest, IntPtr.Zero, 0, 0, 200, 200, 0);
                                SetWindowPos(ps.MainWindowHandle, IntPtr.Zero, 0, 0, 200, 200, 0);
                            }
                        }
                    }
                    else
                    {
                        ps.StartInfo.FileName = strProgram;
                        if (strArgument != null) ps.StartInfo.Arguments = strArgument;
                        ps.Start();
                        if (hWnd_Dest != IntPtr.Zero)
                        {
                            ps.Refresh();
                            try
                            {
                                ps.WaitForInputIdle();
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(500);
                            }
                            //IntPtr child = FindWindow(strProgram, null);
                            //IntPtr child = FindWindowEx(ps.MainWindowHandle, new IntPtr(0), null, null);
                            //SetParent(child, hWnd_Dest);
                            SetParent(ps.MainWindowHandle, hWnd_Dest);
                            RECT lpRect = new RECT();
                            GetWindowRect(hWnd_Dest, ref lpRect);
                            //SetWindowPos(hWnd_Dest, IntPtr.Zero, 0, 0, 200, 200, 0);
                            SetWindowPos(ps.MainWindowHandle, IntPtr.Zero, 0, 0, lpRect.Right - lpRect.Left, lpRect.Bottom - lpRect.Top, 0);
                            //IntPtr pParent = FindWindowEx(hWnd_Dest, new IntPtr(0), null, null);

                        }
                    }

                    if (bRunning == true)
                    {

                    }

#if _USING_DOTNET_3_5
#else
                    if (bRedirect == true)
                    {
                        Wow64RevertWow64FsRedirection(ptr);
                    }
#endif
                    return ps;//.MainWindowHandle;
                }
                catch (Exception ex)
                {
#if _USING_DOTNET_3_5
#else
                    if (bRedirect == true)
                    {
                        Wow64RevertWow64FsRedirection(ptr);
                    }
#endif
                    Ojw.CMessage.Write_Error(ex.ToString());
                    return null;
                }
            }
            public static void MoveProgram(IntPtr hWnd_Program, int nLeft, int nTop, int nRight, int nBottom) { SetWindowPos(hWnd_Program, IntPtr.Zero, nLeft, nTop, nRight - nLeft, nBottom - nTop, 0); }
            public static void MoveProgram(string strProgram, int nLeft, int nTop, int nRight, int nBottom) 
            {
                if (IsRunningProgram(strProgram) == true)
                {
                    String strTitle = Ojw.CFile.GetTitle(strProgram);
                    Process[] processes = System.Diagnostics.Process.GetProcesses();
                    foreach (System.Diagnostics.Process process in processes) { if (process.ProcessName.ToLower() == strTitle.ToLower()) { SetWindowPos(process.MainWindowHandle, IntPtr.Zero, nLeft, nTop, nRight - nLeft, nBottom - nTop, 0); } }
                }                 
            }
            public static void MoveProgram(Process ps, int nLeft, int nTop, int nRight, int nBottom) { if (IsRunningProgram(ps) == true) { SetWindowPos(ps.MainWindowHandle, IntPtr.Zero, nLeft, nTop, nRight - nLeft, nBottom - nTop, 0); } }

            // 출처: http://redreans.tistory.com/58
            public static string Console(string strCommand)
            {
                ProcessStartInfo cmd = new ProcessStartInfo();
                Process process = new Process();
                cmd.FileName = @"cmd";
                cmd.WindowStyle = ProcessWindowStyle.Hidden;             // cmd창이 숨겨지도록 하기
                cmd.CreateNoWindow = true;                               // cmd창을 띄우지 안도록 하기

                cmd.UseShellExecute = false;
                cmd.RedirectStandardOutput = true;        // cmd창에서 데이터를 가져오기
                cmd.RedirectStandardInput = true;          // cmd창으로 데이터 보내기
                cmd.RedirectStandardError = true;          // cmd창에서 오류 내용 가져오기

                process.EnableRaisingEvents = false;
                process.StartInfo = cmd;
                process.Start();
                process.StandardInput.Write(strCommand + Environment.NewLine);
                // 명령어를 보낼때는 꼭 마무리를 해줘야 한다. 그래서 마지막에 NewLine가 필요하다
                process.StandardInput.Close();

                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                process.Close();

                return (new StringBuilder(result)).ToString();//sb.ToString();
            }

            #region Monitor
            //작업 표시줄 포함 크기
            //int W = Screen.PrimaryScreen.Bounds.Width; //모니터 스크린 가로크기
            //int H = Screen.PrimaryScreen.Bounds.Height; //모니터 스크린 세로크기
 
            //작업표시줄 제외 크기
            //int W = Screen.PrimaryScreen.WorkingArea.Width; //작업영역 가로크기
            //int H = Screen.PrimaryScreen.WorkingArea.Height; // 작업영역 세로크기
 
 
            //primaryScreem --> 주모니터에 대한 크기만 가지고 옴
 
            //연결된 전체 모니터 사이즈. 
            //int W = System.Windows.Forms.SystemInformation.VirtualScreen.Width; 
            ////듀얼 모니터 가로 크기
            //int H = System.Windows.Forms.SystemInformation.VirtualScreen.Height; 
            ////듀얼 모니터 세로 크기
            //[출처] C# 모니터 사이즈, 모니터 해상도|작성자 할수있다
            public static int GetMonitor_Width(int nIndex)
            {
                if (nIndex >= Screen.AllScreens.Length) nIndex = Screen.AllScreens.Length - 1;
                if (nIndex < 0) nIndex = 0;
                return Screen.AllScreens[nIndex].Bounds.Width;
            }
            public static int GetMonitor_Height(int nIndex)
            {
                if (nIndex >= Screen.AllScreens.Length) nIndex = Screen.AllScreens.Length - 1;
                if (nIndex < 0) nIndex = 0;
                return Screen.AllScreens[nIndex].Bounds.Height;
            }
            public static Size GetMonitor_Size(int nIndex)
            {
                if (nIndex >= Screen.AllScreens.Length) nIndex = Screen.AllScreens.Length - 1;
                if (nIndex < 0) nIndex = 0;
                return Screen.AllScreens[nIndex].Bounds.Size;
            }
            public static int GetMonitor_Width() { return System.Windows.Forms.SystemInformation.VirtualScreen.Width; }
            public static int GetMonitor_Height() { return System.Windows.Forms.SystemInformation.VirtualScreen.Height; }
            public static Size GetMonitor_Size() { return System.Windows.Forms.SystemInformation.VirtualScreen.Size; }

            public static int GetMonitor_X(int nIndex)
            {
                if (nIndex >= Screen.AllScreens.Length) nIndex = Screen.AllScreens.Length - 1;
                if (nIndex < 0) nIndex = 0;
                return Screen.AllScreens[nIndex].Bounds.X;
            }
            public static int GetMonitor_Y(int nIndex)
            {
                if (nIndex >= Screen.AllScreens.Length) nIndex = Screen.AllScreens.Length - 1;
                if (nIndex < 0) nIndex = 0;
                return Screen.AllScreens[nIndex].Bounds.Y;
            }
            public static Point GetMonitor_Location(int nIndex)
            {
                if (nIndex >= Screen.AllScreens.Length) nIndex = Screen.AllScreens.Length - 1;
                if (nIndex < 0) nIndex = 0;
                return Screen.AllScreens[nIndex].Bounds.Location;
            }
            #endregion Monitor

            //public static Process RunProgram(IntPtr hWnd_Dest, string strProgram)
            //{
            //    //Process Pcs = Process.Start(new ProcessStartInfo(strProgram));
            //    //if (Pcs != null)
            //    //{
            //    //    Pcs.WaitForInputIdle();
            //    //}



            //    Process ps = RunProgram(hWnd_Dest, strProgram, 1);
            //    //if (ps != null)
            //    //{
            //    //    ps.WaitForInputIdle();
            //    //    SetParent(ps.MainWindowHandle, hWnd_Dest);
            //    //}

            //    return ps;
            //}
            public static void ShutDown() { ShutDown(0); }
            public static void ShutDown(int nTime)
            {
                if (nTime > 0)
                {
                    //m_bShutdown = true;
                    String strTmp = nTime.ToString();// Ojw.CConvert.FillString(nTime.ToString(), "0", 3, false);
                    String strTmpCmd = "/t " + strTmp;//
                    //   '/P' - 경고 없이 종료
                    //System.Diagnostics.Process.Start("shutdown", " /s /f " + strTmpCmd + " /c " + (char)34 + "YOUR COMPUTER WILL BE TURNED OFF IN " + strTmp + " seconds" + (char)34);
                    System.Diagnostics.Process.Start("shutdown", " /s /f " + strTmpCmd);
                    //System.Diagnostics.Process.Start("TSSHUTDN", "We will shutdown this computer now.");
                }
                else
                {
                    System.Diagnostics.Process.Start("shutdown", " /f /p");
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

            public static bool IsSubFolder(String strPath) { System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(strPath);/* + "\\")*/ try { foreach (System.IO.DirectoryInfo dirInfoSub in dirInfo.GetDirectories("*")) return true; } catch { return false; } return false; }

            #region Docking
            //private struct SPage_t
            //{
            //    public Panel pnPage;
            //    public Form frmPage;
            //}
            private static List<Panel> m_lstpnPage = null;//new List<SPage_t>();
            public static void Docking_Init()
            {
                if (m_lstpnPage != null)
                {
                    if (m_lstpnPage.Count > 0)
                    {
                        for (int i = 0; i < m_lstpnPage.Count; i++)
                        {
                            m_lstpnPage[i].Controls.Clear();
                        }
                    }
                }
                else m_lstpnPage = new List<Panel>();
                m_lstpnPage.Clear();
            }
            public static int Docking(bool bTopLevel, Form frmPage_From, ref Panel pnPage_To)
            {
                frmPage_From.TopLevel = bTopLevel;
                frmPage_From.Dock = System.Windows.Forms.DockStyle.Fill;
                pnPage_To.Controls.Add(frmPage_From);
                frmPage_From.Show();
                m_lstpnPage.Add(pnPage_To);
                return m_lstpnPage.Count - 1;
            }
            public static int Docking_Count()
            {
                return m_lstpnPage.Count;
            }
            public static Panel Docking_Get(int nIndex)
            {
                return m_lstpnPage[nIndex];
            }
            public static void Docking_Show(int nIndex, bool bShow)
            {
#if true
                m_lstpnPage[nIndex].Visible = bShow;
                if (bShow == true)
                {
                    //m_lstpnPage[nIndex].BringToFront();
                    m_lstpnPage[nIndex].Show();
                }
                else
                {
                    //m_lstpnPage[nIndex].SendToBack();
                    m_lstpnPage[nIndex].Hide();
                }
#else
                m_lstpnPage[nIndex].Visible = bShow;
#endif
            }
            private static int m_nIndex = -1;
            public static void Docking_DispPage(int nIndex)
            {
                if ((nIndex >= m_lstpnPage.Count) || (nIndex < 0)) return;
                Docking_Show(nIndex, true);
                for (int i = 0; i < m_lstpnPage.Count; i++)
                {
                    if (i != nIndex) 
                        Docking_Show(i, false);
                    //Docking_Show(i, (i == nIndex) ? true : false);
                }
                m_nIndex = nIndex;
            }
            public static int Docking_GetCurrentPageIndex() { return m_nIndex; }
            public static Control FindControl(Form frm, String strControl) { try { return frm.Controls.Find(String.Format(strControl), true)[0]; } catch { return null; } }
            #endregion Docking

            #region Skype
            public class CSkype
            {
                private static Skype m_skyCom = null;
                private static string m_strSkype = "Skype.exe";//@"C:\Program Files\Skype\Phone\Skype.exe";
                //public CSkype()
                //{
                //    m_skyCom = new Skype();
                //    RunningSkype();
                //}
                //~CSkype()
                //{
                //}
                //private static bool m_bOpen = false;
                public static bool IsOpen() { return ((m_skyCom != null) ? true : false); }//m_bOpen; }
                public static void Open() { m_skyCom = new Skype(); RunningSkype(); } //if (m_skyCom != null) m_bOpen = true;
                private static void RunningSkype()
                {
                    try
                    {
                        if (CSystem.IsRunningProgram(m_strSkype) == false)
                        {
                            System.Diagnostics.Process ps = CSystem.RunProgram(m_strSkype);//picDisp.Handle, m_strSkype);
                            ps.WaitForInputIdle();
                            CTimer.Wait(3000);
                            
                            CSystem.MoveProgram(ps.MainWindowHandle, 0, 0, 100, 100);
                            ps.Dispose();
                        }
                    }
                    catch
                    {
                        //m_bOpen = false;
                    }
                }
                public static string[]  GetUsers() { List<string> lstUsers = new List<string>(); lstUsers.Clear(); foreach (User user in (IUserCollection)m_skyCom.Friends) { lstUsers.Add(user.Handle); } return lstUsers.ToArray(); }
                public static string    GetImage_Address(string strUserName) { return string.Format("http://api.skype.com/users/{0}/profile/avatar", strUserName); }
                public static Stream    GetImage_Stream(string strUserName) { using (var wc = new WebClient()) { return new MemoryStream(wc.DownloadData(GetImage_Address(strUserName))); } }
                public static Image     GetImage(string strUserName) { using (var wc = new WebClient()) { return Image.FromStream(new MemoryStream(wc.DownloadData(GetImage_Address(strUserName)))); } }
                public static void      Call(string strUser) { RunningSkype(); string strCommand = string.Format("skype:{0}?call&video=true", strUser); System.Diagnostics.Process.Start(strCommand); }
                public static void      Off() { CSystem.KillProgram("Skype"); }
            }
            #endregion Skype

#if _USING_DOTNET_3_5
#elif _USING_DOTNET_2_0
#else
            public class CShm
            {
                public CShm()
                {
                }
                ~CShm()
                {
                    if (Shm_IsOpen() == true)
                    {
                        Shm_Close();
                    }
                }
                // https://msdn.microsoft.com/ko-kr/library/dd997372(v=vs.110).aspx 에서 참고
                private MemoryMappedFile m_mmfShredMemory = null;
                //private MemoryMappedViewAccessor m_accShm = null;
                private int m_nShmSize = 0;
                
                //public class CShm_t<T> where T : struct
                //public class CShm_t<T>
                //{
                //    T[] item;
                //    int nIndex = 0;
                //    public CShm_t() { item = new T[100]; }
                //    public void Push(T element) { item[++nIndex] = element; }
                //    public T Pop() { return item[nIndex--]; }
                //}    
                public bool Shm_IsOpen()
                {
                    return ((m_mmfShredMemory == null) ? false : true);
                }
                public void Shm_Open(string strName) { Shm_Open(strName, 1000); }
                public void Shm_Open(string strName, int nSize)
                {
                    try
                    {
                        m_mmfShredMemory = MemoryMappedFile.CreateOrOpen(strName, nSize);//.OpenExisting(strName);
                        m_nShmSize = nSize;
                        //m_accShm = m_mmfShredMemory.CreateViewAccessor();
                    }
                    catch
                    {
                        m_mmfShredMemory = null;
                    }
                }
                public void Shm_Close()
                {
                    if (Shm_IsOpen() == true)
                    {
                        //m_accShm.Dispose();
                        m_nShmSize = 0;
                        m_mmfShredMemory.Dispose();
                        m_mmfShredMemory = null;
                        //m_mmfShredMemory.Dispose();
                    }
                }
                private Mutex m_mtxShmWrite = new Mutex();
            #region Shm_Write()
                public void Shm_Write_Struct(long lPos, object DataStructure)
                {
                    if (Shm_IsOpen() == false) return;
                    m_mtxShmWrite.WaitOne();
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        int i = 0;
                        byte[] pbyteData = Ojw.CConvert.StructureToByte(DataStructure);
                        foreach (byte byData in pbyteData) { accessor.Write(lPos + i++, byData); }
                    }
                    m_mtxShmWrite.ReleaseMutex();
                }
                public void Shm_Write(long lPos, string Data)
                {
                    if (Shm_IsOpen() == false) return;
                    m_mtxShmWrite.WaitOne();
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        int i = 0;
                        foreach (byte byData in Data) { accessor.Write(lPos + i++, byData); }
                        accessor.Write(lPos + i++, (byte)0);
                    }
                    m_mtxShmWrite.ReleaseMutex();
                }
                public void Shm_Write(long lPos, byte [] Data)
                {
                    if (Shm_IsOpen() == false) return;
                    m_mtxShmWrite.WaitOne();
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        int i = 0;
                        foreach (byte byData in Data) { accessor.Write(lPos + i++, byData); }
                    }
                    m_mtxShmWrite.ReleaseMutex();
                }
                public void Shm_Write(long lPos, bool Data)
                {
                    if (Shm_IsOpen() == false) return;
                    m_mtxShmWrite.WaitOne();
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        accessor.Write(lPos, Data);
                    }
                    m_mtxShmWrite.ReleaseMutex();
                }
                public void Shm_Write(long lPos, char Data)
                {
                    if (Shm_IsOpen() == false) return;
                    m_mtxShmWrite.WaitOne();
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        accessor.Write(lPos, Data);
                    }
                    m_mtxShmWrite.ReleaseMutex();
                }
                public void Shm_Write(long lPos, byte Data)
                {
                    if (Shm_IsOpen() == false) return;
                    m_mtxShmWrite.WaitOne();
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        accessor.Write(lPos, Data);
                    }
                    m_mtxShmWrite.ReleaseMutex();
                }
                public void Shm_Write(long lPos, double Data)
                {
                    if (Shm_IsOpen() == false) return;
                    m_mtxShmWrite.WaitOne();
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        accessor.Write(lPos, Data);
                    }
                    m_mtxShmWrite.ReleaseMutex();
                }
                public void Shm_Write(long lPos, float Data)
                {
                    if (Shm_IsOpen() == false) return;
                    m_mtxShmWrite.WaitOne();
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        accessor.Write(lPos, Data);
                    }
                    m_mtxShmWrite.ReleaseMutex();
                }
                public void Shm_Write(long lPos, short Data)
                {
                    if (Shm_IsOpen() == false) return;
                    m_mtxShmWrite.WaitOne();
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        accessor.Write(lPos, Data);
                    }
                    m_mtxShmWrite.ReleaseMutex();
                }
                public void Shm_Write(long lPos, int Data)
                {
                    if (Shm_IsOpen() == false) return;
                    m_mtxShmWrite.WaitOne();
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        accessor.Write(lPos, Data);
                    }
                    m_mtxShmWrite.ReleaseMutex();
                }
                public void Shm_Write(long lPos, long Data)
                {
                    if (Shm_IsOpen() == false) return;
                    m_mtxShmWrite.WaitOne();
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        accessor.Write(lPos, Data);
                    }
                    m_mtxShmWrite.ReleaseMutex();
                }
                #endregion Shm_Write()

            #region Shm_Read()
                public object Shm_Read_Structure(long lPos, Type typeStructure)
                {
                    if (Shm_IsOpen() == false) return null;
                    int nSize = Marshal.SizeOf(typeStructure);
                    byte [] pbyteData = new byte[nSize];
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        string strData = String.Empty;
                        for (long i = 0; i < nSize; i++)
                        {
                            pbyteData[i] = accessor.ReadByte(lPos + i);                            
                        }
                    }

                    return Ojw.CConvert.ByteToStructure(pbyteData, typeStructure);
                }
                public string Shm_Read_Str(long lPos)
                {
                    if (Shm_IsOpen() == false) return null;
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        string strData = String.Empty;
                        for (int i = 0; i < (m_nShmSize - lPos); i++)
                        {
                            byte byData = accessor.ReadByte(lPos + i);
                            if (byData == 0) break;
                            else strData += (char)byData;
                        }
                        return strData;
                    }
                }
                public byte [] Shm_Read_Bytes(long lPos, int nSize)
                {
                    if (Shm_IsOpen() == false) return null;
                    byte [] pbyteData = new byte[nSize];
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        string strData = String.Empty;
                        for (long i = 0; i < nSize; i++)
                        {
                            pbyteData[i] = accessor.ReadByte(lPos + i);                            
                        }
                    }
                    return pbyteData;
                }
                public bool Shm_Read_Bool(long lPos)
                {
                    if (Shm_IsOpen() == false) return false;
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        return accessor.ReadBoolean(lPos);
                    }
                }
                public char Shm_Read_Char(long lPos)
                {
                    if (Shm_IsOpen() == false) return (char)0;
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        return accessor.ReadChar(lPos);
                    }
                }
                public byte Shm_Read_Byte(long lPos)
                {
                    if (Shm_IsOpen() == false) return 0;
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        return accessor.ReadByte(lPos);
                    }
                }
                public double Shm_Read_Double(long lPos)
                {
                    if (Shm_IsOpen() == false) return 0.0;
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        return accessor.ReadDouble(lPos);
                    }
                }
                public float Shm_Read_Float(long lPos)
                {
                    if (Shm_IsOpen() == false) return 0.0f;
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        return accessor.ReadSingle(lPos);
                    }
                }
                public short Shm_Read_Short(long lPos)
                {
                    if (Shm_IsOpen() == false) return 0;
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        return accessor.ReadInt16(lPos);
                    }
                }
                public int Shm_Read_Int(long lPos)
                {
                    if (Shm_IsOpen() == false) return 0;
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        return accessor.ReadInt32(lPos);
                    }
                }
                public long Shm_Read_Long(long lPos)
                {
                    if (Shm_IsOpen() == false) return 0;
                    using (MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor())
                    {
                        return accessor.ReadInt64(lPos);
                    }
                }
                #endregion Shm_Read()

                
#if false
                private void Shm_Create(string strName, int nSize)//, long lPos, object st)
                {
                    m_mmfShredMemory = MemoryMappedFile.CreateNew(strName, nSize);
                    //MemoryMappedViewAccessor accessor = m_mmfShredMemory.CreateViewAccessor();

                    //accessor.Write(lPos, ref obj);
                    //Console.WriteLine("Memory-mapped file created!");

                    //Console.ReadLine(); // pause till enter key is pressed

                    //// dispose of the memory-mapped file object and its accessor

                    //accessor.Dispose();

                    //m_mmfShredMemory.Dispose();
#if false
                //long offset = 0x10000000; // 256 megabytes
                //long length = 0x20000000; // 512 megabytes

                //string strTitle = Ojw.CFile.GetTitle(strFileName);
                m_mmfShredMemory = MemoryMappedFile.CreateNew("", 1000);//FromFile(strFileName, FileMode.Open, strTitle);
                using (var accessor = m_mmfShredMemory.CreateViewAccessor())
                {
                    int colorSize = Marshal.SizeOf(objStruct);
                    accessor.Write(
                    // Make changes to the view.
                    for (long i = 0; i < length; i += colorSize)
                    {
                        accessor.Read(i, out color);
                        color.Brighten(10);
                        accessor.Write(i, ref color);
                    }
                }

                //using (var mmf = MemoryMappedFile.CreateFromFile(strFileName, FileMode.Open, strTitle))
                //{
                //    int colorSize = Marshal.SizeOf(objStruct);
                    
                //    // Make changes to the view.
                //    for (long i = 0; i < 1500000; i += colorSize)
                //    {
                //        accessor.Read(i, out color);
                //        color.Brighten(20);
                //        accessor.Write(i, ref color);
                //    }

                //}
#endif
                }
#endif

            }


#if false
            public class CShm_Var
            {
                public CShm_Var()
                {
                }
                ~CShm_Var()
                {
                    Shm_DInit();
                }
                private IntPtr m_ptStructure;

                private System.Type m_Type;
                public object objSData;
                private object objSBack;// = new object();
                public void Shm_Init_Server(string strHandle, object varStructure) { Shm_Init_Server(strHandle, varStructure, _SIZE_SHM); }
                public void Shm_Init_Server(string strHandle, object varStructure, int nSize)
                {
                    m_bServer = true;
                    //Marshal.GetITypeInfoForType
                    //m_Type = TypeStructure.GetType();
                    //objSData = (TypeStructure.GetType());
                    //objSData = varStructure as object;
                    m_Type = varStructure.GetType();
                    m_ptStructure = Marshal.AllocHGlobal(Marshal.SizeOf(varStructure));
                    Marshal.StructureToPtr(varStructure, m_ptStructure, false);
                    objSData = Marshal.PtrToStructure(m_ptStructure, m_Type);

                    //m_ptStructure = Marshal.Get
                    Shm_Create(strHandle, nSize);
                    m_thShm = new Thread(new ThreadStart(Thread_Shm));
                    m_thShm.Start();
                }
                public void Shm_Init_Client(string strHandle, object varStructure) { Shm_Init_Client(strHandle, varStructure, _SIZE_SHM); }
                public void Shm_Init_Client(string strHandle, object varStructure, int nSize)
                {
                    m_bServer = false;

                    m_Type = varStructure.GetType();
                    m_ptStructure = Marshal.AllocHGlobal(Marshal.SizeOf(varStructure));
                    Marshal.StructureToPtr(varStructure, m_ptStructure, false);
                    objSData = Marshal.PtrToStructure(m_ptStructure, m_Type);

                    //objSData = varStructure as object;
                    //objSData = varStructure.G;
                    Shm_Create(strHandle, nSize);
                    m_thShm = new Thread(new ThreadStart(Thread_Shm));
                    m_thShm.Start();
                }
                public void Shm_DInit()
                {
                    Marshal.FreeHGlobal(m_ptStructure);

                    Shm_Close();
                }
                /// <summary>
                /// ////////////////////////////////////////////////////////
                /// </summary>
                private bool m_bShm_Thread = false;
                //private int m_nSeq_Back = 0;
                private const int _SIZE_SHM = 100;
                //private IntPtr m_ptStructure;
                private string m_strHandle = String.Empty;

                private Ojw.CSystem.CShm Shm = new Ojw.CSystem.CShm();
                private Thread m_thShm;

                private bool m_bServer = false;
                private bool m_bRun = false;
                private void Shm_Create(string strHandle, int nSize)
                {
                    if (nSize <= 0) nSize = _SIZE_SHM;
                    Shm.Shm_Open(strHandle, nSize);
                    //Shm_Update();

                    m_bRun = true;
                    Shm_Flush();
                    //Ojw.CMessage.Write("Shm={0}", Shm.Shm_Read_Str(0));
                }
                private void Shm_Flush() { Shm.Shm_Write_Struct(0, objSData); }
                private void Shm_Update()
                {
                    objSData = Shm.Shm_Read_Structure(0, m_Type);//objSData.GetType());
                    //SData = (SShm_t)Shm.Shm_Read_Structure(0, typeof(SShm_t));
                }
                private void Shm_Close()
                {
                    m_bRun = false;
                    Shm_Flush();
                    //Ojw.CMessage.Write("Shm -> Closed");
                    // Thread 가 동작상태가 아니면 직접 종료한다.
                    if (m_bShm_Thread == false)
                        Shm.Shm_Close();
                }

                private void Thread_Shm()
                {
                    try
                    {
                        m_bShm_Thread = true;
                        Ojw.CMessage.Write("[{0}]Thread -> Start", m_strHandle);

                        //objSData = Marshal.PtrToStructure(m_ptStructure, m_Type);
                        objSBack = objSData;
                        while (m_bRun != false)
                        {
                            //objSData = Marshal.PtrToStructure(m_ptStructure, m_Type);
                            if (m_bServer == false) Shm_Update();
                            else
                            {
                                if (objSBack.Equals(objSData) == false)
                                {
                                    objSBack = objSData;
                                    Shm_Flush();
                                }
                            }

                            Thread.Sleep(10);
                        }
                        Ojw.CMessage.Write("[{0}]Thread -> Start", m_strHandle);
                        Shm_Close();
                        Ojw.CMessage.Write("[{0}]Program End", m_strHandle);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
#endif
#endif
        }
    }
}
