using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenJigWare
{
    partial class Ojw
    {
        private const int _WM_SETREDRAW = 11;
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

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
                m_txtMessage = txt;
                Write2("==== Open Jig Ware Ver {0} ====", SVersion_T.strVersion);
            }
            // set the text box handle for errors only ...
            public static void Init_Error(TextBox txt) { m_txtMessage_Error = txt; }
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
                    if (objects != null) 
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Clear();
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
                    if (objects != null)
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Clear();
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
                    if (objects != null)
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Clear();
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
                    if (objects != null)
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Clear();
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
            private static string m_strSecondFile = "";
            public static void Write(TextBox txtOjwMessage, string msg, string strFileTitle, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    m_strSecondFile = strFileTitle;
                    m_bMsgFile_Second = true;
                    if (objects != null)
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Clear();
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
            public static void Write2(TextBox txtOjwMessage, string msg, string strFileTitle, params object[] objects)
            {
                try
                {
                    m_nMessageStack++;
                    m_strSecondFile = strFileTitle;
                    m_bMsgFile_Second = true;
                    if (objects != null)
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Clear();
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
                    if (objects != null)
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Clear();
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
                    if (objects != null)
                    {
                        if (objects.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Clear();
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
                if (objects != null)
                {
                    if (objects.Length > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Clear();
                        sb.AppendFormat(msg, objects);
                        msg = sb.ToString();
                    }
                } 
                OjwDebugMessage(true, true, true, true, true, msg);
                OjwDebugMessage(m_txtMessage_Error, false, false, false, true, true, m_strErrorMessage);
            }
            public static void Write_Error(TextBox txtOjwMessage, string msg, params object[] objects)
            {
                m_strErrorMessage += "\r\n------------------------\r\n" + (++m_nErrorCnt).ToString() + "\'st. " + msg + "\r\n------------------------\r\n";
                m_lstError.Add(m_nErrorCnt.ToString() + "\'st. " + msg); // Add Error list
                msg = "\r\n**************[Error]***********\r\n" + msg + "********************************";
                m_bErrorMessage = true;

                m_nMessageStack++;// = 1;
                if (objects != null)
                {
                    if (objects.Length > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Clear();
                        sb.AppendFormat(msg, objects);
                        msg = sb.ToString();
                    }
                } 
                OjwDebugMessage(txtOjwMessage, true, true, true, true, true, msg);
                OjwDebugMessage(m_txtMessage_Error, false, false, false, true, true, m_strErrorMessage);
            }
            #endregion MessageBox

            #region Var
            private static int m_nLimitLines = 999;
            private static int m_nMessageStack = 0;
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
            private static void OjwDebugMessage(bool bFile, bool bFunction, bool bLine, bool bTime, bool bLinefeed, string msg)
            {
                try
                {
                    m_nMessageStack++;
                    OjwDebugMessage(m_txtMessage, bFile, bFunction, bLine, bTime, bLinefeed, msg);
                }
                catch (Exception e)
                {
                    MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                    m_nMessageStack = 0;
                }
            }
            private static Mutex m_mtxMessage = new Mutex();
            delegate void Ctrl_Involk(TextBox txtOjwMessage, bool bFile, bool bFunction, bool bLine, bool bTime, bool bLinefeed, string msg);

            private static void OjwDebugMessage(TextBox txtOjwMessage, bool bFile, bool bFunction, bool bLine, bool bTime, bool bLinefeed, string msg)
            {
                try
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
                        m_nMessageStack = 0;
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
                                        LinkedList<String> tmpLines = new LinkedList<string>(txtOjwMessage.Lines);
                                        while ((tmpLines.Count - nLimit) > 0) { tmpLines.RemoveFirst(); }
                                        txtOjwMessage.Lines = tmpLines.ToArray();
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
                                if (nDepth <= 2) break;
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
                            m_nMessageStack = 0;

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
                            MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                            if (bValid == true)
                            {
                                SendMessage(txtOjwMessage.Handle, _WM_SETREDRAW, true, 0); // set Redraw
                            }
                            m_nMessageStack = 0;
                        }

                        // 뮤텍스 해제
                        m_mtxMessage.ReleaseMutex();
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
