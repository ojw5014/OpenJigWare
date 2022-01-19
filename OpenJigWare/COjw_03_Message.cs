

using System;
using System.Collections.Generic;

#if _USING_DOTNET_3_5 || _USING_DOTNET_2_0
#else
using System.Linq;
#endif
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenJigWare
{
    //static public void Printf()
    //{
    //}
#if false // 실험적 코드
    public class printf
    {
        #region printf
        public static void Init(TextBox txt, bool bShowVersion) { Ojw.CMessage.SetStackDepth(1); Ojw.CMessage.Init(txt, bShowVersion); }
        public static void Init(TextBox txt) { Init(txt, true); }

        // printf
        public static void Init_Error(TextBox txt) { Ojw.CMessage.SetStackDepth(1); Ojw.CMessage.Init_Error(txt); }
        public static void Init_File(bool bEn) { Ojw.CMessage.Init_File(bEn); }
        public static void Init_FilePath(String strPath) { Ojw.CMessage.Init_FilePath(strPath); }

        public printf(string msg, params object[] objects) { _printf(m_bDetail, msg, objects); }
        public printf(bool bShowDetail, string msg, params object[] objects) { if (bShowDetail) Ojw.CMessage.Write(msg, objects); else Ojw.CMessage.Write2(msg, objects); }
        public printf(bool bShowDetail, bool bError, string msg, params object[] objects)
        {
            if (bError)
            {
                //if (bShowDetail)
                Ojw.CMessage.Write_Error(msg, objects);
            }
            else
            {
                if (bShowDetail)
                    Ojw.CMessage.Write(msg, objects);
                else
                    Ojw.CMessage.Write2(msg, objects);
            }
        }
        public printf(TextBox txt, string msg, params object[] objects) { _printf(txt, m_bDetail, msg, objects); }
        public printf(TextBox txt, bool bShowDetail, string msg, params object[] objects) { if (bShowDetail) Ojw.CMessage.Write(txt, msg, objects); else Ojw.CMessage.Write2(msg, objects); }
        public printf(TextBox txt, bool bShowDetail, bool bError, string msg, params object[] objects)
        {
            if (bError)
            {
                //if (bShowDetail)
                Ojw.CMessage.Write_Error(txt, msg, objects);
            }
            else
            {
                if (bShowDetail)
                    Ojw.CMessage.Write(txt, msg, objects);
                else
                    Ojw.CMessage.Write2(txt, msg, objects);
            }
        }

        #region static _printf
        public static void _printf(string msg, params object[] objects) { _printf(m_bDetail, msg, objects); }
        public static void _printf(bool bShowDetail, string msg, params object[] objects) { if (bShowDetail) Ojw.CMessage.Write(msg, objects); else Ojw.CMessage.Write2(msg, objects); }
        public static void _printf(bool bShowDetail, bool bError, string msg, params object[] objects)
        {
            if (bError)
            {
                //if (bShowDetail)
                Ojw.CMessage.Write_Error(msg, objects);
            }
            else
            {
                if (bShowDetail)
                    Ojw.CMessage.Write(msg, objects);
                else
                    Ojw.CMessage.Write2(msg, objects);
            }
        }
        public static void _printf(TextBox txt, string msg, params object[] objects) { _printf(txt, m_bDetail, msg, objects); }
        public static void _printf(TextBox txt, bool bShowDetail, string msg, params object[] objects) { if (bShowDetail) Ojw.CMessage.Write(txt, msg, objects); else Ojw.CMessage.Write2(msg, objects); }
        public static void _printf(TextBox txt, bool bShowDetail, bool bError, string msg, params object[] objects)
        {
            if (bError)
            {
                //if (bShowDetail)
                Ojw.CMessage.Write_Error(txt, msg, objects);
            }
            else
            {
                if (bShowDetail)
                    Ojw.CMessage.Write(txt, msg, objects);
                else
                    Ojw.CMessage.Write2(txt, msg, objects);
            }
        }
        #endregion static _printf

        private static bool m_bDetail = false;
        public static void SetDetail(bool bDetail) { m_bDetail = bDetail; }
        public static void newline() { _printf("\r\n"); }
        public static void newline(TextBox txt) { _printf(txt, "\r\n"); }
        public static void Error(string msg, params object[] objects) { _printf(m_bDetail, true, msg, objects); }
        public static void Error(TextBox txt, string msg, params object[] objects) { _printf(txt, m_bDetail, true, msg, objects); }
        
        #endregion printf
    }
    public class scanf
    {
        public static string value
        {
            get
            {
                string strValue = value;//String.Empty;
                if (_scanf(null, ref strValue) == false) return null;
                if (strValue.Length > 0) return strValue;
                return null;
            }
            set
            {
            }
        }
        private static bool _scanf(ref string strValue) { return _scanf(null, ref strValue); }
        private static bool m_bSetScanfPos = false;
        private static Point m_pntScanf = new Point(0, 0);
        private static bool m_bScanfAnswer = true;
        public static void scanf_SetAnswer(bool bPrintAnswer) { m_bScanfAnswer = bPrintAnswer; }
        public static void scanf_SetPosition(int nX, int nY)
        {
            m_pntScanf.X = nX;
            m_pntScanf.X = nY;
            m_bSetScanfPos = true;
        }
        public static void scanf_RemovePosition()
        {
            m_bSetScanfPos = false;
        }
        public static bool _scanf(TextBox txt, ref string strValue)
        {
            bool bOk = true;
            string strTitle = "Input your value";
            if (m_bSetScanfPos == false)
            {
                if (Ojw.CInputBox.Show(strTitle, "Value", ref strValue) != DialogResult.OK) bOk = false;
            }
            else
            {
                if (Ojw.CInputBox.Show(m_pntScanf.X, m_pntScanf.Y, strTitle, "Value", ref strValue) != DialogResult.OK) bOk = false;
            }
            if (m_bScanfAnswer) printf._printf(strValue);
            return bOk;
        }
    }
#endif
   
    public partial class Ojw
    {
        private const int _WM_SETREDRAW = 11;
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

        // Init
        public static void Log_Init(TextBox txt, bool bShowVersion) { Ojw.CMessage.SetStackDepth(1); Ojw.CMessage.Init(txt, bShowVersion); }
        public static void Log_Init(TextBox txt) { Log_Init(txt, true); }
        public static void Log_Init_Error(TextBox txt) { Ojw.CMessage.SetStackDepth(1); Ojw.CMessage.Init_Error(txt); }
        public static void Log_Init_File(bool bEn) { Ojw.CMessage.Init_File(bEn); }
        public static void Log_Init_FilePath(String strPath) { Ojw.CMessage.Init_FilePath(strPath); }
        
        // Log
        public static void Log(string msg, params object[] objects) { Ojw.CMessage.Write(msg, objects); }
        public static void Log2(string msg, params object[] objects) { Ojw.CMessage.Write2(msg, objects); }
        public static void Log(TextBox txtOjwMessage, string msg, params object[] objects) { Ojw.CMessage.Write(txtOjwMessage, msg, objects); }
        public static void Log2(TextBox txtOjwMessage, string msg, params object[] objects) { Ojw.CMessage.Write2(txtOjwMessage, msg, objects); }
                
        // Error
        public static void LogErr(string msg, params object[] objects) { Ojw.CMessage.Write_Error(msg, objects); }
        public static void LogErr(TextBox txtOjwMessage, string msg, params object[] objects) { Ojw.CMessage.Write_Error(msg, objects); }

        public static void LogErr_ClearMsg() { Ojw.CMessage.ClearErrorMessage(); }
        public static String LogErr_GetMessaes() { return Ojw.CMessage.GetErrorMessaes(); }
        public static String LogErr_GetList(int nNum) { return Ojw.CMessage.GetErrorList(nNum); }
        public static List<String> LogErr_GetList() { return Ojw.CMessage.GetErrorList(); }
        public static int LogErr_Count() { return Ojw.CMessage.GetError_Count(); }
        public static void LogErr_Reset() { Ojw.CMessage.Reset(); }
        public static string LogErr_GetLastErrorMessage() { return Ojw.CMessage.GetLastErrorMessage(); }
        public static bool LogErr_IsError() { return Ojw.CMessage.IsError(); }
        public static bool IsLogErr() { return Ojw.CMessage.IsError(); }
        
        #region printf
        // printf
        public static void printf_Init(TextBox txt, bool bShowVersion) { Ojw.CMessage.SetStackDepth(1); Ojw.CMessage.Init(txt, bShowVersion); }
        public static void printf_Init(TextBox txt) { printf_Init(txt, true); }
        //public static void printf_Init(TextBox txt) { Ojw.CMessage.SetStackDepth(1); Ojw.CMessage.Init(txt); }
        public static void printf_Init_Error(TextBox txt) { Ojw.CMessage.SetStackDepth(1); Ojw.CMessage.Init_Error(txt); }
        public static void printf_Init_File(bool bEn) { Ojw.CMessage.Init_File(bEn); }
        public static void printf_Init_FilePath(String strPath) { Ojw.CMessage.Init_FilePath(strPath); }
        private static bool m_bDetail = false;
        public static void printf_SetDetail(bool bDetail) { m_bDetail = bDetail; }
        public static void printf(string msg, params object[] objects) { printf(m_bDetail, msg, objects); }
        //public static void printf(TextBox txt, string msg, params object[] objects) { printf(txt, false, msg, objects); }
        public static void newline() { printf("\r\n"); }
        public static void newline(TextBox txt) { printf(txt, "\r\n"); }
        public static void clear() { Ojw.CMessage.ClearScr(); }
        public static void clear_errorscreen() { Ojw.CMessage.ClearScr_For_Error(); }
        public static void clear(TextBox txt) { Ojw.CMessage.ClearScr(txt); }
        public static void printf(bool bShowDetail, string msg, params object[] objects) { if (bShowDetail) Ojw.CMessage.Write(msg, objects); else Ojw.CMessage.Write2(msg, objects); }
        public static void printf(bool bShowDetail, bool bError, string msg, params object[] objects)
        {
            if (bError)
            {
                //if (bShowDetail)
                    Ojw.CMessage.Write_Error(msg, objects);
            }
            else
            {
                if (bShowDetail)
                    Ojw.CMessage.Write(msg, objects);
                else
                    Ojw.CMessage.Write2(msg, objects);
            }
        }
        public static void printf_Error(string msg, params object[] objects) { printf(m_bDetail, true, msg, objects); }
        public static void printf_Error(TextBox txt, string msg, params object[] objects) { printf(txt, m_bDetail, true, msg, objects); }
        public static void printf(TextBox txt, string msg, params object[] objects) { printf(txt, m_bDetail, msg, objects); }
        public static void printf(TextBox txt, bool bShowDetail, string msg, params object[] objects) { if (bShowDetail) Ojw.CMessage.Write(txt, msg, objects); else Ojw.CMessage.Write2(txt, msg, objects); }
        public static void printf(TextBox txt, bool bShowDetail, bool bError, string msg, params object[] objects)
        {
            if (bError)
            {
                //if (bShowDetail)
                    Ojw.CMessage.Write_Error(txt, msg, objects);
            }
            else
            {
                if (bShowDetail)
                    Ojw.CMessage.Write(txt, msg, objects);
                else
                    Ojw.CMessage.Write2(txt, msg, objects);
            }
        }
        public static string scanf()
        {
            string strValue = String.Empty;
            if (scanf(null, ref strValue) == false) return null;
            if (strValue.Length > 0) return strValue;
            return null;
        }
        public static bool scanf(ref string strValue) { return scanf(null, ref strValue); }
        private static bool m_bSetScanfPos = false;
        private static Point m_pntScanf = new Point(0, 0);
        private static bool m_bScanfAnswer = false;//true;
        public static void scanf_SetAnswer(bool bPrintAnswer) { m_bScanfAnswer = bPrintAnswer; }
        public static void scanf_SetPosition(int nX, int nY)
        {
            m_pntScanf.X = nX;
            m_pntScanf.X = nY;
            m_bSetScanfPos = true;
        }
        public static void scanf_RemovePosition()
        {
            m_bSetScanfPos = false;
        }
        public static bool scanf(TextBox txt, ref string strValue)
        {
            bool bOk = true;
            string strTitle = "Input your value";
            if (m_bSetScanfPos == false)
            {
                if (Ojw.CInputBox.Show(strTitle, "Value", ref strValue) != DialogResult.OK) bOk = false;
            }
            else
            {
                if (Ojw.CInputBox.Show(m_pntScanf.X, m_pntScanf.Y, strTitle, "Value", ref strValue) != DialogResult.OK) bOk = false;
            }
            if (m_bScanfAnswer) printf(strValue);
            return bOk;
        }
        #endregion printf

        public class CMessage
        {
            private static bool m_bProgEnd = false;
            public CMessage()
            {
            }
            ~CMessage()
            {
                m_bProgEnd = true;
            }
            #region MessageBox(OjwMessage...)
            private static int m_nCount_DisplayStack = 2;
            public static void SetStackDepth(int nDepth) { m_nMessageStack = m_nMessageStack_InitValue = nDepth; }
            // set the stack degree. Default = 2
            public static void SetCount_DisplayStack(int nCount) { m_nCount_DisplayStack = nCount; } 
            // Limit Message Lines - Default : 999
            public static void SetLimitLines(int nLines) { m_nLimitLines = nLines; }
            public static int GetLimitLines() { return m_nLimitLines; }

            #region Initialize functions
            // File save or not(Default : Current path.)
            public static void Init_File(bool bEn) { m_bMsgFile = bEn; } // History File Save(Normal & Error)
            // Change file path(Default : Current path.)
            public static void Init_FilePath(String strPath) { m_strMsgFilePath = strPath; }
            public static void Init_FilePath_Second(String strPath) { m_strMsgFilePath2 = strPath; }
            // set the text box handle for writing history messages
            public static void Init(TextBox txt) 
            {
                if (txt == null)
                {
                    Init();
                }
                else
                {
                    m_txtMessage = txt;
                    m_nMessageStatus = 0;
                    Write2("==== Open Jig Ware Ver [{0}] ====\r\n", SVersion_T.strVersion);
                }
            }
            public static void Init()
            {
                m_txtMessage = new TextBox();
                m_nMessageStatus = 0;
                Write2("==== Open Jig Ware Ver [{0}] ====\r\n", SVersion_T.strVersion);
            }
            public static void Init(TextBox txt, bool bShowVersion)
            {
                if (txt == null)
                {
                    Init(bShowVersion);
                }
                else
                {
                    m_txtMessage = txt;
                    m_nMessageStatus = 0;
                    if (bShowVersion) Write2("==== Open Jig Ware Ver [{0}] ====\r\n", SVersion_T.strVersion);
                }
            }
            public static void Init(bool bShowVersion)
            {
                m_txtMessage = new TextBox();
                m_nMessageStatus = 0;
                if (bShowVersion) Write2("==== Open Jig Ware Ver [{0}] ====\r\n", SVersion_T.strVersion);
            }
            public static void Init_without_InitMessage(TextBox txt)
            {
                m_txtMessage = txt;
                m_nMessageStatus = 0;
            }
            // set the text box handle for errors only ...
            public static void Init_Error(TextBox txt) { m_txtMessage_Error = txt; }
            private static int m_nMessageStatus = 0;
            public static void Destroy()
            {
                m_nMessageStatus = -1;
            }
            #endregion Initialize functions

            // choose file save for errors only ...(default = false)
            private static bool m_bError_Accumulation = false;
            public static bool IsError_Accumulation() { return m_bError_Accumulation; }
            public static void SetErrorAccumulation(bool bEn) { m_bError_Accumulation = bEn; }
            // check error
            public static bool IsError() { return m_bErrorMessage; }
            // return all error messages
            public static string GetLastErrorMessage() { return GetErrorList(m_nErrorCnt - 1); } //{ return m_strErrorMessage; }
            // clear error status
            public static void Reset() { m_bErrorMessage = false; m_strErrorMessage = ""; m_nErrorCnt = 0; m_lstError.Clear(); }
            // get value of all errors
            public static int GetError_Count() { return m_nErrorCnt; }
            // get messages of all errors
            public static List<String> GetErrorList() { return m_lstError; }
            public static String GetErrorList(int nNum) { if ((nNum < 0) || (nNum >= m_nErrorCnt)) return null; return m_lstError[nNum]; }
            public static String GetErrorMessaes() { String strData = String.Empty; foreach (String strItem in GetErrorList()) { strData += strItem + "\r\n"; } return strData; }
            // clear only error messages
            public static void ClearErrorMessage() { m_strErrorMessage = ""; }

            private static bool m_bFile = true;
            private static bool m_bFunction = true;
            private static bool m_bLine = true;
            private static bool m_bTime = true;
            public static void SetupDisplay(bool bFile, bool bFunction, bool bLine, bool bTime)
            {
                m_bFile = bFile;
                m_bFunction = bFunction;
                m_bLine = bLine;
                m_bTime = bTime;
            }
            public static void SetupDisplay_File(bool bEn) { m_bFile = bEn; }
            public static void SetupDisplay_Function(bool bEn) { m_bFunction = bEn; }
            public static void SetupDisplay_Line(bool bEn) { m_bLine = bEn; }
            public static void SetupDisplay_Time(bool bEn) { m_bTime = bEn; }
            // write to text box which you refer to...
            // Write -> write all informations and enter
            // Write2 -> write only your letters(without enter, informations)
            public static void Write(string msg, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    if ((objects != null) && (m_nMessageStatus >= 0)) 
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                            sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                            sb.Remove(0, sb.Length);
#else
                            sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                            sb.AppendFormat(msg, objects);
                            msg = sb.ToString();
                        }
                    }
                    OjwDebugMessage(m_bFile, m_bFunction, m_bLine, m_bTime, true, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                }
            }
            public static void Write2(string msg, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    if ((objects != null) && (m_nMessageStatus >= 0))
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                            sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                            sb.Remove(0, sb.Length);
#else
                            sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                            sb.AppendFormat(msg, objects);
                            msg = sb.ToString();
                        }
                    } 
                    OjwDebugMessage(false, false, false, false, false, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                }
            }
            public static void Write(TextBox txtOjwMessage, string msg, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    if ((objects != null) && (m_nMessageStatus >= 0))
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                            sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                            sb.Remove(0, sb.Length);
#else
                            sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                            sb.AppendFormat(msg, objects);
                            msg = sb.ToString();
                        }
                    } 
                    OjwDebugMessage(txtOjwMessage, true, true, true, true, true, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                }
            }
            public static void Write2(TextBox txtOjwMessage, string msg, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    if ((objects != null) && (m_nMessageStatus >= 0))
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                            sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                            sb.Remove(0, sb.Length);
#else
                            sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                            sb.AppendFormat(msg, objects);
                            msg = sb.ToString();
                        }
                    } 
                    OjwDebugMessage(txtOjwMessage, false, false, false, false, false, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                }
            }
            public static void Write(RichTextBox rtxtOjwMessage, string msg, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    if ((objects != null) && (m_nMessageStatus >= 0))
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                            sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                            sb.Remove(0, sb.Length);
#else
                            sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                            sb.AppendFormat(msg, objects);
                            msg = sb.ToString();
                        }
                    }
                    OjwDebugMessage(rtxtOjwMessage, true, true, true, true, true, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                }
            }
            public static void Write2(RichTextBox rtxtOjwMessage, string msg, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    if ((objects != null) && (m_nMessageStatus >= 0))
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                            sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                            sb.Remove(0, sb.Length);
#else
                            sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                            sb.AppendFormat(msg, objects);
                            msg = sb.ToString();
                        }
                    }
                    OjwDebugMessage(rtxtOjwMessage, false, false, false, false, false, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                }
            }
            private static string m_strSecondFile = "";
            public static void Write_secondFile(TextBox txtOjwMessage, string msg, string strFileTitle, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    m_strSecondFile = strFileTitle;
                    m_bMsgFile_Second = true;
                    if ((objects != null) && (m_nMessageStatus >= 0))
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                            sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                            sb.Remove(0, sb.Length);
#else
                            sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                            sb.AppendFormat(msg, objects);
                            msg = sb.ToString();
                        }
                    } 
                    OjwDebugMessage(txtOjwMessage, true, true, true, true, true, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                }
            }
            public static void Write2_secondFile(TextBox txtOjwMessage, string msg, string strFileTitle, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    m_strSecondFile = strFileTitle;
                    m_bMsgFile_Second = true;
                    if ((objects != null) && (m_nMessageStatus >= 0))
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                            sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                            sb.Remove(0, sb.Length);
#else
                            sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                            sb.AppendFormat(msg, objects);
                            msg = sb.ToString();
                        }
                    } 
                    OjwDebugMessage(txtOjwMessage, false, false, false, false, false, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                }
            }
            public static void Write_withFileName(string msg, string strFileTitle, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    m_strSecondFile = strFileTitle;
                    m_bMsgFile_Second = true;
                    if ((objects != null) && (m_nMessageStatus >= 0))
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                            sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                            sb.Remove(0, sb.Length);
#else
                            sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                            sb.AppendFormat(msg, objects);
                            msg = sb.ToString();
                        }
                    } 
                    OjwDebugMessage(true, true, true, true, true, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                }
            }
            public static void Write2_withFileName(string msg, string strFileTitle, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    m_strSecondFile = strFileTitle;
                    m_bMsgFile_Second = true;
                    if ((objects != null) && (m_nMessageStatus >= 0))
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                            sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                            sb.Remove(0, sb.Length);
#else
                            sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                            sb.AppendFormat(msg, objects);
                            msg = sb.ToString();
                        }
                    } 
                    OjwDebugMessage(false, false, false, false, false, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                }
            }

            // Write -> write all informations and enter.(Error)
            public static void Write_Error(string msg, params object[] objects)
            {
                m_strErrorMessage += "\r\n------------------------\r\n" + (++m_nErrorCnt).ToString() + "\'st. " + msg + "\r\n------------------------\r\n";
                m_lstError.Add(m_nErrorCnt.ToString() + "\'st. " + msg);  // Add Error list
                msg = "\r\n**************[Error]***********\r\n" + msg + "\r\n********************************";
                m_bErrorMessage = true;

                m_nMessageStack++;
                if ((objects != null) && (m_nMessageStatus >= 0))
                {
                    if (objects.Length > 0)
                    {
                        StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                        sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                            sb.Remove(0, sb.Length);
#else
                            sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                        sb.AppendFormat(msg, objects);
                        msg = sb.ToString();
                    }
                } 
                OjwDebugMessage(true, true, true, true, true, msg);
                if (m_txtMessage_Error.IsDisposed == true) m_txtMessage_Error = new TextBox();
                OjwDebugMessage(m_txtMessage_Error, false, false, false, true, true, m_strErrorMessage);
            }
            public static void Write_Error(TextBox txtOjwMessage, string msg, params object[] objects)
            {
                m_strErrorMessage += "\r\n------------------------\r\n" + (++m_nErrorCnt).ToString() + "\'st. " + msg + "\r\n------------------------\r\n";
                m_lstError.Add(m_nErrorCnt.ToString() + "\'st. " + msg); // Add Error list
                msg = "\r\n**************[Error]***********\r\n" + msg + "********************************";
                m_bErrorMessage = true;

                m_nMessageStack++;// = 1;
                if ((objects != null) && (m_nMessageStatus >= 0))
                {
                    if (objects.Length > 0)
                    {
                        StringBuilder sb = new StringBuilder();
#if _USING_DOTNET_3_5
                        sb.Remove(0, sb.Length);
#elif _USING_DOTNET_2_0
                        sb.Remove(0, sb.Length);
#else
                        sb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                        sb.AppendFormat(msg, objects);
                        msg = sb.ToString();
                    }
                } 
                OjwDebugMessage(txtOjwMessage, true, true, true, true, true, msg);
                OjwDebugMessage(m_txtMessage_Error, false, false, false, true, true, m_strErrorMessage);
            }
            #endregion MessageBox

            #region Var
            private static int m_nMessageStack_InitValue = 0;
            private static int m_nLimitLines = 900;//999;
            private static int m_nMessageStack = m_nMessageStack_InitValue;
            private static TextBox m_txtMessage = new TextBox();
            private static TextBox m_txtMessage_Error = new TextBox();
            private static bool m_bMsgFile = false;
            private static bool m_bMsgFile_Second = false;
            private static String m_strMsgFilePath = Application.StartupPath;
            private static String m_strMsgFilePath2 = Application.StartupPath;
            private static bool m_bErrorMessage = false;
            private static string m_strErrorMessage = "";
            private static int m_nErrorCnt = 0;
            private static List<String> m_lstError = new List<string>();
            #endregion Var
            #region Private
            public static void ClearScr()
            {
                if (m_txtMessage.IsDisposed == true) m_txtMessage = new TextBox();
                ClearScr(m_txtMessage);
            }
            public static void ClearScr_For_Error()
            {
                if (m_txtMessage_Error.IsDisposed == true) m_txtMessage_Error = new TextBox();
                ClearScr(m_txtMessage_Error);
            }
            public static void ClearScr(TextBox txtOjwMessage)
            {
                if (txtOjwMessage.InvokeRequired)
                {
                    Ctrl_Clear_Involk CI = new Ctrl_Clear_Involk(ClearScr);
                    txtOjwMessage.Invoke(CI, txtOjwMessage);
                }
                else
                {
                    // 뮤텍스 대기(다중스레드 공유 위반 방지)
                    m_mtxMessage.WaitOne();

                    bool bValid = true;
                    try
                    {
                        #region Try
                        txtOjwMessage.Clear();
                        #endregion Try
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(String.Format("[Message:Lines({0})]{1}\r\n", txtOjwMessage.Lines.Length, e.ToString()));
                        if (bValid == true)
                        {
                            SendMessage(txtOjwMessage.Handle, _WM_SETREDRAW, true, 0); // set Redraw
                        }
                        m_nMessageStack = m_nMessageStack_InitValue;
                    }

                    // 뮤텍스 해제
                    m_mtxMessage.ReleaseMutex();
                }
            }
            private static void OjwDebugMessage(bool bFile, bool bFunction, bool bLine, bool bTime, bool bLinefeed, string msg)
            {
                try
                {
                    m_nMessageStack++;
                    if (m_txtMessage.IsDisposed == true) m_txtMessage = new TextBox();
                    OjwDebugMessage(m_txtMessage, bFile, bFunction, bLine, bTime, bLinefeed, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                    m_nMessageStack = m_nMessageStack_InitValue;
                }
            }
            private static Mutex m_mtxMessage = new Mutex();
            delegate void Ctrl_Involk(TextBox txtOjwMessage, bool bFile, bool bFunction, bool bLine, bool bTime, bool bLinefeed, string msg);
            delegate void Ctrl_Clear_Involk(TextBox txtOjwMessage);
            public static void SaveImageFile(String strFileName, Bitmap bmp)
            {
                string strPath = CFile.CheckAndMakeFolder(m_strMsgFilePath, false, true, true, true, false);
                string strName = strPath + "\\" + strFileName;
                SaveImage(bmp, strName);
            }
            private static bool SaveImage(System.Windows.Forms.PictureBox pictureBox, string path)
            {
                bool bRet = true;
                using (var bitmap = new Bitmap(pictureBox.Width, pictureBox.Height))
                {
                    pictureBox.DrawToBitmap(bitmap, pictureBox.ClientRectangle);
                    System.Drawing.Imaging.ImageFormat imageFormat = null;
                    var extension = System.IO.Path.GetExtension(@path);
                    switch (extension.ToLower())
                    {
                        case ".bmp":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                            break;
                        case ".png":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Png;
                            break;
                        case ".jpeg":
                        case ".jpg":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                        case ".gif":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Gif;
                            break;
                        case ".tiff":
                        case ".tif":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Tiff;
                            break;
                        default:
                            //bRet = false;
                            imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                            break;
                    }
                    //bitmap.Save(@path, imageFormat);
                    FileInfo file = new FileInfo(@path);
                    if (file.Exists)
                    {
                        System.IO.File.Delete(@path); // => 이 부분을 이걸로 지우니까 지워진다.                    
                    }
                    bitmap.Save(@path, imageFormat);
                }
                return bRet;
            }
            private static bool SaveImage(Bitmap image, string path)
            {
                bool bRet = true;
                using (var bitmap = new Bitmap(image.Width, image.Height))
                {
                    System.Drawing.Imaging.ImageFormat imageFormat = null;
                    var extension = System.IO.Path.GetExtension(path);
                    switch (extension.ToLower())
                    {
                        case ".bmp":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                            break;
                        case ".png":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Png;
                            break;
                        case ".jpeg":
                        case ".jpg":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                        case ".gif":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Gif;
                            break;
                        case ".tiff":
                        case ".tif":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Tiff;
                            break;
                        default:
                            //bRet = false;
                            imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                            break;
                    }
                    /*

                    FileInfo file = new FileInfo(@path);
                    if (file.Exists)
                    {
                        System.IO.File.Delete(@path); // => 이 부분을 이걸로 지우니까 지워진다.
                        image.Save(@path);
                    }
                    */

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        //FileStream fs = File.Create(@path);
                    }
                    image.Save(path, imageFormat);
                    /*

                    if (File.Exists(@path))
                        File.Delete(@path);
                    FileStream file = File.Open(@path, FileMode.Create);
                    //FileInfo file = new FileInfo(@path);
                    //if (file.Exists)
                    //{
                    //    file.Delete();
                    

                        image.Save(@path, imageFormat);
                    //}*/
                    //SaveFileDialog sd = new SaveFileDialog();
                    //sd.
                }
                return bRet;
            }
            private static void OjwDebugMessage(TextBox txtOjwMessage, bool bFile, bool bFunction, bool bLine, bool bTime, bool bLinefeed, string msg)
            {
                try
                {
                    if (m_nMessageStatus >= 0)
                    {
                        if (txtOjwMessage.InvokeRequired)
                        {
                            Ctrl_Involk CI = new Ctrl_Involk(OjwDebugMessage);
                            txtOjwMessage.Invoke(CI, txtOjwMessage, bFile, bFunction, bLine, bTime, bLinefeed, msg);
                        }
                        else
                        {
                            // 뮤텍스 대기(다중스레드 공유 위반 방지)
                            m_mtxMessage.WaitOne();

                            bool bValid = true;
                            try
                            {
                                #region Try

                                if (m_bProgEnd == true) return;

#if _NO_DISPLAY_WARNING // Warning message remover
                    if (msg.ToUpper().IndexOf("WARN") >= 0)
                    {
                        m_nMessageStack = m_nMessageStack_InitValue;
                        return;
                    }
#endif
                                #region Line Limit
                                int nLimit = m_nLimitLines;
                                if (txtOjwMessage.IsDisposed == false)
                                {
                                    int nLength = 0;
                                    try
                                    {
                                        nLength = txtOjwMessage.Lines.Length;
                                    }
                                    catch //(Exception e)
                                    {
                                        bValid = false;
                                        //MessageBox.Show(nLength.ToString() + ":[Message]" + e.ToString() + "\r\n");
                                    }
                                    if (bValid == true)//(txtOjwMessage.Text != "")
                                    {
                                        if (txtOjwMessage.Lines.Length > nLimit)
                                        {

#if _USING_DOTNET_3_5 || _USING_DOTNET_2_0
                                            List<String> tmpLines = new List<string>(txtOjwMessage.Lines);
                                            while ((tmpLines.Count - nLimit) > 0) { tmpLines.RemoveAt(0); }//.RemoveFirst(); }
                                            //txtOjwMessage.Clear();
                                            txtOjwMessage.Lines.Initialize();

                                            //tmpLines.CopyTo(txtOjwMessage.Lines, 0);
                                            txtOjwMessage.Lines = tmpLines.ToArray();

                                            //foreach (string strData in tmpLines)
                                            //{ 
                                            //    txtOjwMessage.Text += strData + "\r\n";
                                            //}
#else
                                            LinkedList<String> tmpLines = new LinkedList<string>(txtOjwMessage.Lines);
                                            while ((tmpLines.Count - nLimit) > 0) { tmpLines.RemoveFirst(); }
                                            txtOjwMessage.Lines = tmpLines.ToArray();
#endif
                                        }
                                    }
                                }
                                #endregion Line Limit
                                m_nMessageStack++;
                                String strMsg = String.Empty;
                                int nDepth = 0;
                                for (int i = 0; i < m_nCount_DisplayStack; i++)
                                {
                                    nDepth = m_nMessageStack - i;
                                    // Write(2) / Debug...(1) Invisible functions
                                    if (nDepth <= 2 + m_nMessageStack_InitValue) break;
                                    System.Diagnostics.StackFrame sf = new System.Diagnostics.StackTrace(true).GetFrame(nDepth);
                                    //System.Reflection.MethodBase method = sf.GetMethod();//new System.Diagnostics.StackTrace(true).GetFrame(nDepth).GetMethod();
                                    //String strTime = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day.ToString() + "," + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString();
                                    if (bFile && (sf.GetFileName() != null)) strMsg += "{" + Ojw.CFile.GetName(sf.GetFileName()) + "}";
                                    if (bFunction && (sf.GetMethod().Name != null)) strMsg += "{" + sf.GetMethod().Name + "}";//method.Name + "}";
                                    if (bLine) strMsg += "{" + sf.GetFileLineNumber().ToString() + "}";
                                }
                                if (bTime) strMsg += "{" + DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day.ToString() + "," + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + "}";
                                if (bLinefeed == true) msg += "\r\n";
                                if (bValid == true)
                                {
                                    SendMessage(txtOjwMessage.Handle, _WM_SETREDRAW, false, 0); // Block Redraw : Block Refresh
                                    //txtOjwMessage.AppendText(strMsg + msg + ((bLinefeed == true) ? "\r\n" : ""));       
                                    txtOjwMessage.AppendText(strMsg + msg);       // add your letters to chatting list window
                                    SendMessage(txtOjwMessage.Handle, _WM_SETREDRAW, true, 0); // set Redraw
                                    //txtOjwMessage.Select(txtOjwMessage.Text.Length, 0);
                                    txtOjwMessage.ScrollToCaret();                // 'scroll' -> move to current caret
                                }
                                m_nMessageStack = m_nMessageStack_InitValue;

                                if (m_bMsgFile == true)
                                {
                                    String strFile = ((m_bMsgFile_Second == true) ? m_strMsgFilePath2 : m_strMsgFilePath);
                                    String strTitle = ((m_bMsgFile_Second == true) ? m_strSecondFile : "history");
                                    //// File ////
                                    // get the folder name by time informations without tree
                                    string strPath = CFile.CheckAndMakeFolder(strFile, false, true, true, true, false);
                                    string strName = "";
                                    if (m_bMsgFile_Second == false)
                                    {
                                        strName = strPath + "\\" + strTitle + ".log";
                                    }
                                    else
                                    {
                                        strName = strPath + "\\" + strTitle;
                                    }
                                    MsgFileSave(strName, strMsg + msg);
                                    if (txtOjwMessage == m_txtMessage_Error)
                                    {
                                        //// File ////
                                        // get the folder name by time informations without tree
                                        strPath = CFile.CheckAndMakeFolder(strFile, false, true, true, true, false);
                                        strName = strPath + "\\" + strTitle + "_Error.log";
                                        MsgFileSave(strName, strMsg + msg);
                                        if (IsError_Accumulation() == false) ClearErrorMessage();
                                    }
                                    m_bMsgFile_Second = false;
                                }
                                #endregion Try
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(String.Format("[Message:Lines({0})]{1}\r\n", txtOjwMessage.Lines.Length, e.ToString()));
                                if (bValid == true)
                                {
                                    SendMessage(txtOjwMessage.Handle, _WM_SETREDRAW, true, 0); // set Redraw
                                }
                                m_nMessageStack = m_nMessageStack_InitValue;
                            }

                            // 뮤텍스 해제
                            m_mtxMessage.ReleaseMutex();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            private static void OjwDebugMessage(RichTextBox rtxtOjwMessage, bool bFile, bool bFunction, bool bLine, bool bTime, bool bLinefeed, string msg)
            {
                try
                {
                    if (m_nMessageStatus >= 0)
                    {
                        if (rtxtOjwMessage.InvokeRequired)
                        {
                            Ctrl_Involk CI = new Ctrl_Involk(OjwDebugMessage);
                            rtxtOjwMessage.Invoke(CI, rtxtOjwMessage, bFile, bFunction, bLine, bTime, bLinefeed, msg);
                        }
                        else
                        {
                            // 뮤텍스 대기(다중스레드 공유 위반 방지)
                            m_mtxMessage.WaitOne();

                            bool bValid = true;
                            try
                            {
                                #region Try

                                if (m_bProgEnd == true) return;

#if _NO_DISPLAY_WARNING // Warning message remover
                    if (msg.ToUpper().IndexOf("WARN") >= 0)
                    {
                        m_nMessageStack = m_nMessageStack_InitValue;
                        return;
                    }
#endif
                                #region Line Limit
                                //int nLimit = m_nLimitLines;
                                if (rtxtOjwMessage.IsDisposed == false)
                                {
                                    int nLength = 0;
                                    try
                                    {
                                        nLength = rtxtOjwMessage.Lines.Length;
                                    }
                                    catch //(Exception e)
                                    {
                                        bValid = false;
                                        //MessageBox.Show(nLength.ToString() + ":[Message]" + e.ToString() + "\r\n");
                                    }
                                    if (bValid == true)//(txtOjwMessage.Text != "")
                                    {
#if false // richedit 에서는 제한을 없앤다.
                                        if (rtxtOjwMessage.Lines.Length > nLimit)
                                        {
                                            LinkedList<String> tmpLines = new LinkedList<string>(rtxtOjwMessage.Lines);
                                            while ((tmpLines.Count - nLimit) > 0) { tmpLines.RemoveFirst(); }

#if _USING_DOTNET_3_5 || _USING_DOTNET_2_0
                                            rtxtOjwMessage.Clear();
                                            tmpLines.CopyTo(rtxtOjwMessage.Lines, 0);
                                            //foreach (string strData in tmpLines)
                                            //{
                                            //    rtxtOjwMessage.Text += strData + "\r\n";
                                            //}                                            
#else
                                            txtOjwMessage.Lines = tmpLines.ToArray();
#endif
                                        }
#endif
                                    }
                                }
                                #endregion Line Limit
                                m_nMessageStack++;
                                String strMsg = String.Empty;
                                int nDepth = 0;
                                for (int i = 0; i < m_nCount_DisplayStack; i++)
                                {
                                    nDepth = m_nMessageStack - i;
                                    // Write(2) / Debug...(1) Invisible functions
                                    if (nDepth <= 2 + m_nMessageStack_InitValue) break;
                                    System.Diagnostics.StackFrame sf = new System.Diagnostics.StackTrace(true).GetFrame(nDepth);
                                    //System.Reflection.MethodBase method = sf.GetMethod();//new System.Diagnostics.StackTrace(true).GetFrame(nDepth).GetMethod();
                                    //String strTime = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day.ToString() + "," + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString();
                                    if (bFile && (sf.GetFileName() != null)) strMsg += "{" + Ojw.CFile.GetName(sf.GetFileName()) + "}";
                                    if (bFunction && (sf.GetMethod().Name != null)) strMsg += "{" + sf.GetMethod().Name + "}";//method.Name + "}";
                                    if (bLine) strMsg += "{" + sf.GetFileLineNumber().ToString() + "}";
                                }
                                if (bTime) strMsg += "{" + DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day.ToString() + "," + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + "}";
                                if (bLinefeed == true) msg += "\r\n";
                                if (bValid == true)
                                {
                                    SendMessage(rtxtOjwMessage.Handle, _WM_SETREDRAW, false, 0); // Block Redraw : Block Refresh
                                    //txtOjwMessage.AppendText(strMsg + msg + ((bLinefeed == true) ? "\r\n" : ""));       
                                    rtxtOjwMessage.AppendText(strMsg + msg);       // add your letters to chatting list window
                                    SendMessage(rtxtOjwMessage.Handle, _WM_SETREDRAW, true, 0); // set Redraw
                                    //txtOjwMessage.Select(txtOjwMessage.Text.Length, 0);
                                    rtxtOjwMessage.ScrollToCaret();                // 'scroll' -> move to current caret
                                }
                                m_nMessageStack = m_nMessageStack_InitValue;

                                if (m_bMsgFile == true)
                                {
                                    String strFile = ((m_bMsgFile_Second == true) ? m_strMsgFilePath2 : m_strMsgFilePath);
                                    String strTitle = ((m_bMsgFile_Second == true) ? m_strSecondFile : "history");
                                    //// File ////
                                    // get the folder name by time informations without tree
                                    string strPath = CFile.CheckAndMakeFolder(strFile, false, true, true, true, false);
                                    string strName = "";
                                    if (m_bMsgFile_Second == false)
                                    {
                                        strName = strPath + "\\" + strTitle + ".log";
                                    }
                                    else
                                    {
                                        strName = strPath + "\\" + strTitle;
                                    }
                                    MsgFileSave(strName, strMsg + msg);
                                    
#if false
                                    if (rtxtOjwMessage == m_txtMessage_Error)
                                    {
                                        //// File ////
                                        // get the folder name by time informations without tree
                                        strPath = CFile.CheckAndMakeFolder(strFile, false, true, true, true, false);
                                        strName = strPath + "\\" + strTitle + "_Error.log";
                                        MsgFileSave(strName, strMsg + msg);
                                        if (IsError_Accumulation() == false) ClearErrorMessage();
                                    }
#endif
                                    
                                    m_bMsgFile_Second = false;
                                }
                                #endregion Try
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                                if (bValid == true)
                                {
                                    SendMessage(rtxtOjwMessage.Handle, _WM_SETREDRAW, true, 0); // set Redraw
                                }
                                m_nMessageStack = m_nMessageStack_InitValue;
                            }

                            // 뮤텍스 해제
                            m_mtxMessage.ReleaseMutex();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            private static bool MsgFileSave(String strFileName, String strMsg)
            {
                FileInfo f = null;
                StreamWriter fs = null;

                try
                {
                    f = new FileInfo(strFileName);
                    fs = new StreamWriter(strFileName, true, Encoding.Default);
                    fs.Flush(); // flush stream buffers

                    fs.Write(strMsg);

                    fs.Close();
                    return true;
                }
                catch
                {
                    if (fs != null) fs.Close();
                    return false;
                }
            }
            #endregion Private
        }
    }
}
