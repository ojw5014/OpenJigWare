using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Threading;

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
            //public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, byte [] lParam);
            //[DllImport("user32.dll", CharSet = CharSet.Ansi)]
            //static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPStr)] String lParam);
            //[DllImport("user32.dll", SetLastError = true)]
            //static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

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
                cds.cbData = strData.Length + 1;
                cds.lpData = strData;

                //String strTitle = Ojw.CFile.GetTitle(strProgram);
                //Process[] pro = Process.GetProcessesByName(strTitle);

                //SendMessage(pro[0].MainWindowHandle, WM_COPYDATA, 0, ref cds);
                //return true;
                
                System.Diagnostics.Process ps = new System.Diagnostics.Process();
                String strTitle = Ojw.CFile.GetTitle(strProgram);
                Process[] processes = System.Diagnostics.Process.GetProcesses();
                bool bRunning = false;

                foreach (System.Diagnostics.Process process in processes)
                {
                    
                    if (process.ProcessName == strTitle)
                    {
                        //IntPtr handle = FindWindowEx(process.MainWindowHandle, new IntPtr(0), strTitle, null);

                        //if (handle.ToInt32() > 0)
                        //{
                        //SendMessage(process.MainWindowHandle, WM_COPYDATA, 0, pbyData);
                        SendMessage(process.MainWindowHandle, WM_COPYDATA, 0, ref cds);

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
                m_lstpnPage[nIndex].Visible = bShow;
            }
            private static int m_nIndex = -1;
            public static void Docking_DispPage(int nIndex)
            {
                if ((nIndex >= m_lstpnPage.Count) || (nIndex < 0)) return;
                for (int i = 0; i < m_lstpnPage.Count; i++) Docking_Show(i, (i == nIndex) ? true : false);
                m_nIndex = nIndex;
            }
            public static int Docking_GetCurrentPageIndex() { return m_nIndex; }
            public static Control FindControl(Form frm, String strControl) { try { return frm.Controls.Find(String.Format(strControl), true)[0]; } catch { return null; } }
            #endregion Docking

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
        }
    }
}
