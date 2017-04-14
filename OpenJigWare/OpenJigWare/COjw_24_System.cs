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
            private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

#if false            
            public static void RunVirtualKeyboard(int nLeft, int nTop, int nRight, int nBottom)
            {
                IntPtr MainWindowHandle = FindWindowW("OSKMainClass","On-Screen Keyboard");
                bool success = SetWindowPos(MainWindowHandle, IntPtr.Zero, 0, 200, 500, 600, 0);
            }
#else
            public static void ScreenKeyboard(int nLeft, int nTop)
            {
                //ScreenKeyboard_Kill();
                if (m_ps != null) ScreenKeyboard_Kill();
                else { KillProgram("osk"); }//Thread.Sleep(100); }
                try
                {
                    if (m_ps != null)
                    {
                        m_ps.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.ToString());
                }
                m_ps = null;
                RegistryKey myKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Osk", true);

                myKey.SetValue("WindowLeft", nLeft, RegistryValueKind.DWord);
                myKey.SetValue("WindowTop", nTop, RegistryValueKind.DWord);

                string strPath = String.Format(@"{0}\System32\osk.exe", Ojw.CSystem.GetPath_Windows());
                m_ps = Ojw.CSystem.RunProgram(strPath);
                //ps.Dispose();
                //return ps;
            }
            private static Process m_ps = null;//new Process();
            public static void ScreenKeyboard_Kill() { KillProgram(m_ps); m_ps.Dispose(); }
            public static void ScreenKeyboard(int nLeft, int nTop, int nWidth, int nHeight)
            {
                //ScreenKeyboard_Kill();
                if (m_ps != null) ScreenKeyboard_Kill();
                else { KillProgram("osk"); }//Thread.Sleep(100); }
                try
                {
                    if (m_ps != null)
                    {
                        m_ps.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.ToString());
                }
                m_ps = null;

                RegistryKey myKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Osk", true);

                myKey.SetValue("WindowWidth", nWidth, RegistryValueKind.DWord);
                myKey.SetValue("WindowHeight", nHeight, RegistryValueKind.DWord);
                myKey.SetValue("WindowLeft", nLeft, RegistryValueKind.DWord);
                myKey.SetValue("WindowTop", nTop, RegistryValueKind.DWord);

                string strPath = String.Format(@"{0}\System32\osk.exe", Ojw.CSystem.GetPath_Windows());
                m_ps = Ojw.CSystem.RunProgram(strPath);
                //ps.Dispose();
                //return ps;
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
            public static string GetPath_ProgramFilesX86() { return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).TrimEnd('\\'); }
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
                        COPYDATASTRUCT cds = (COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT));
                        strData = cds.lpData;
                        bRet = true;
                        //Ojw.CMessage.Write(pbyteData);
                        //MessageBox.Show(pbyteData);//String.Format("{0}{1}{2}{3}{4}{5}{6}{7}", 
                        break;
                }
                return bRet;                
            }

            //[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
            //protected override void WndProc(ref Message m)
            //{
            //    base.WndProc(ref m);
            //}
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
            public static Process RunProgram(string strProgram, string strArgument, int nRunningMode) { return RunProgram(IntPtr.Zero, strProgram, null, nRunningMode); }

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
                return ps;//.MainWindowHandle;
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
            public static void ShutDown(int nTime)
            {
                if (nTime > 0)
                {
                    //m_bShutdown = true;
                    String strTmp = Ojw.CConvert.FillString(nTime.ToString(), "0", 3, false);
                    String strTmpCmd = "/t " + strTmp;//
                    System.Diagnostics.Process.Start("shutdown", " /s /f " + strTmpCmd + " /c " + (char)34 + "YOUR COMPUTER WILL BE TURNED OFF IN " + strTmp + " seconds" + (char)34);
                    //System.Diagnostics.Process.Start("TSSHUTDN", "We will shutdown this computer now.");
                }
                else
                {
                    System.Diagnostics.Process.Start("shutdown", " /f /P");
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
