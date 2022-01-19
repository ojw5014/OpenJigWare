
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OpenJigWare
{
    partial class Ojw
    {
        #region COjwFile
        public class CFile
        {
            #region File Management

            #region File Check(IsFile)
            public static bool IsFile(String strFile)
            {
                System.IO.FileInfo f = new System.IO.FileInfo(strFile);
                if (f.Exists == true) return true;
                return false;
            }
            public static long GetFile_Size(String strFile)
            {
                System.IO.FileInfo f = new System.IO.FileInfo(strFile);
                return f.Length;
            }
            public static DateTime GetFile_LastWriteTime(String strFile)
            {
                System.IO.FileInfo f = new System.IO.FileInfo(strFile);
                return f.LastWriteTime;
            }
            public static DateTime GetFile_LastAccessTime(String strFile)
            {
                System.IO.FileInfo f = new System.IO.FileInfo(strFile);
                return f.LastAccessTime;
            }
            public static DateTime GetFile_CreationTime(String strFile)
            {
                System.IO.FileInfo f = new System.IO.FileInfo(strFile);
                return f.CreationTime;
            }
            public static FileAttributes GetFile_Attributes(String strFile)
            {
                System.IO.FileInfo f = new System.IO.FileInfo(strFile);
                return f.Attributes;
            }
            public static bool IsFiles(params String[] pstrFile)
            {
                int nCnt = 0;
                foreach (string strItem in pstrFile) { if (IsFile(strItem) == true) nCnt++; }
                if (nCnt == pstrFile.Length) return true;
                return false;
            }
            #endregion File Check(IsFile)

            #region GetTitle(String strPath) - Get the file name(without file extention) only(Kor: 파일의 이름만 얻는 함수)
            // Get the file name without extension(Kor: 파일의 이름만 얻는 함수)
            public static String GetTitle(String strPath)
            {
                int nLen;
                int nFind;
                String strTitle;

                int nFindPos = strPath.LastIndexOf(".");
                int nPosLast = strPath.Length - 1 - nFindPos;
                
                nLen = strPath.Length;
                if (nLen <= 0) return null;
                nFind = strPath.LastIndexOf("\\");
                if (nFind == nLen)
                {
                    nFind = 0;
                    strTitle = strPath;
                }
                else
                    strTitle = strPath.Substring(nFind + 1);
                if (nFindPos >= 0)
                    strTitle = strTitle.Substring(0, strTitle.Length - nPosLast - 1);
                return strTitle;
            }
            // Get the file name with extension(Kor: 파일의 이름만 얻는 함수(확장자 포함))
            public static String GetName(String strPath)
            {
                int nLen;
                int nFind;
                String strTitle;
                
                nLen = strPath.Length;
                if (nLen <= 0) return null;
                nFind = strPath.LastIndexOf("\\");
                if (nFind == nLen)
                {
                    nFind = 0;
                    strTitle = strPath;
                }
                else
                    strTitle = strPath.Substring(nFind + 1);
                return strTitle;
            }
            #endregion GetTitle(String strPath) - Get the file name(without file extention) only(Kor: 파일의 이름만 얻는 함수)

            #region GetPath(String strPath) - Get the file path only(without file name)(Kor: 파일의 경로만 얻는 함수)
            // Get the file path only(without file name)(Kor: 파일의 경로만 얻는 함수)
            public static String GetPath(String strPath)
            {
                int nLen;
                int nFind;
                String strTitle;

                nLen = strPath.Length;
                if (nLen <= 0) return null;
                nFind = strPath.LastIndexOf("\\");
                if ((nFind == nLen) || (nFind < 0)) return null;
                else                                strTitle = strPath.Substring(0, nFind);
                return strTitle;
            }
            #endregion GetPath(String strPath) - Get the file path only(without file name)(Kor: 파일의 경로만 얻는 함수)

            #region GetExe(String strPath) - Get the file extension only(without file title)(Kor: 파일의 확장자만 얻는 함수)
            // Get the file extension only(without file title)(Kor: 파일의 확장자만 얻는 함수)
            public static String GetExe(String strPath)
            {
                try
                {
                    int nLen;
                    int nFind;
                    String strTitle;

                    int nFindPos = strPath.LastIndexOf(".");
                    int nPosLast = strPath.Length - 1 - nFindPos;

                    nLen = strPath.Length;
                    if ((nFindPos < 0) || (nLen <= 0)) return null;
                    nFind = strPath.LastIndexOf("\\");
                    if (nFind == nLen)
                    {
                        nFind = 0;
                        strTitle = strPath;
                    }
                    else
                        strTitle = strPath.Substring(nFind + 1);
                    strTitle = strTitle.Substring(strTitle.Length - nPosLast, nPosLast);
                    return strTitle;
                }
                catch
                {
                    return "";
                }
            }
            #endregion GetExe(String strPath) - Get the file extension only(without file title)(Kor: 파일의 확장자만 얻는 함수)


            #endregion File Management

            #region File & Folder Management Function[Kor: 파일 및 폴더관리]
            // next especialy path which you set...(Kor: 특정 경로(C:\project\Web\FileRoot\ATTACH\) 밑에)
            // return the path after making a folder (yyyyMMdd : year/Month/Date) (Kor: 일자별로 yyyyMMdd 형식으로 폴더를 만들고 경로 반환.)
            // just return path only if there is folder like that(kor: 폴더가 있다면 그냥 경로만 반환.)
            #region Make a folder(Kor: 폴더 생성)
            // It'll make the folder if there is no one
            public static string CheckAndMakeFolder(string strPath)
            {
                if (strPath.Substring(strPath.Length - 1) != "\\")
                    strPath += "\\";
                if (!System.IO.Directory.Exists(strPath))
                    System.IO.Directory.CreateDirectory(strPath);

                return strPath;
            }

            // It'll make the folder if there is no one
            // make the KeyFileName or (DirectoryPath + KeyFileName)
            // -> bSeparation == true  : "2015\\01\\15\\173300\\" return. (15th/Jan/2015, 05:33:00 pm)
            // -> bSeparation == false : "20150115173300" return. (15th/Jan/2015, 05:33:00 pm)
            public static string CheckAndMakeFolder(string strPath, bool bSeparation, bool bYear4, bool bMonth2, bool bDay2, bool bTime)
            {
                if (strPath.Substring(strPath.Length - 1) != "\\")
                {
                    strPath += "\\";
                }
                StringBuilder strb = new StringBuilder();
                //strb.Remove(0, strb.Length);
#if _USING_DOTNET_3_5
                strb.Remove(0, strb.Length);
#elif _USING_DOTNET_2_0
                strb.Remove(0, strb.Length);
#else
                strb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
                //////strb.Append(strPath).Append(System.DateTime.Today.ToString(@"yyyyMMdd"));
                ////strb.Append(strPath).Append(System.DateTime.Today.ToString(@"yyyy")).Append(@"\");
                ////strb.Append(CConvert.FillString(CConvert.IntToStr(CTimer.GetMonth()), "0", 2, false)).Append(@"\");
                ////strb.Append(CConvert.FillString(CConvert.IntToStr(1), "0", 2, false)).Append(@"\").Append("hhmmss.ms");
                //strb.Append(strPath).Append(System.DateTime.Today.ToString(@"yyyy")).Append(@"\");
                //strb.Append(System.DateTime.Today.ToString(@"MM")).Append(@"\");
                //strb.Append(System.DateTime.Today.ToString(@"dd")).Append(@"\");
                //strb.Append(System.DateTime.Now.ToString(@"hhmmss.M")).Append(@"\");
                strb.Append(strPath).Append(MakeKey(bSeparation, bYear4, bMonth2, bDay2, bTime));
                if (!System.IO.Directory.Exists(strb.ToString()))
                    System.IO.Directory.CreateDirectory(strb.ToString());

                return strb.ToString();
            }

            // make the KeyFileName or (DirectoryPath + KeyFileName)
            // -> bSeparation == true  : "2015\\01\\15\\173300\\" return. (15th/Jan/2015, 05:33:00 pm)
            // -> bSeparation == false : "20150115173300" return. (15th/Jan/2015, 05:33:00 pm)
            public static string MakeKey(bool bSeparation, bool bYear4, bool bMonth2, bool bDay2, bool bTime)
            {
                StringBuilder strb = new StringBuilder();
                
#if _USING_DOTNET_3_5
                strb.Remove(0, strb.Length);
#elif _USING_DOTNET_2_0
                strb.Remove(0, strb.Length);
#else
                strb.Clear(); // Dotnet 4.0 이상에서만 사용
#endif

                /*CConvert.FillString(CConvert.IntToStr(CTimer.GetYear()), "0", 4, false) + "-" +
                               CConvert.FillString(CConvert.IntToStr(CTimer.GetMonth()), "0", 2, false) + "-" +
                               CConvert.FillString(CConvert.IntToStr(CTimer.GetDay()), "0", 2, false) + ", " +
                               CConvert.FillString(CConvert.IntToStr(CTimer.GetHour()), "0", 2, false) + "::" +
                               CConvert.FillString(CConvert.IntToStr(CTimer.GetMinute()), "0", 2, false) + "::" +
                               CConvert.FillString(CConvert.IntToStr(CTimer.GetSecond()), "0", 2, false);
                 * */
                if (bYear4 == true)
                {
                    strb.Append(CConvert.FillString(CConvert.IntToStr(CTimer.GetYear()), "0", 4, false));
                    if (bSeparation == true) strb.Append(@"\");
                }

                if (bMonth2 == true)
                {
                    strb.Append(CConvert.FillString(CConvert.IntToStr(CTimer.GetMonth()), "0", 2, false));
                    if (bSeparation == true) strb.Append(@"\");
                }

                if (bDay2 == true)
                {
                    strb.Append(CConvert.FillString(CConvert.IntToStr(CTimer.GetDay()), "0", 2, false));
                    if (bSeparation == true) strb.Append(@"\");
                }

                if (bTime == true)
                {
                    strb.Append(
                                CConvert.FillString(CConvert.IntToStr(CTimer.GetHour()), "0", 2, false) +
                                CConvert.FillString(CConvert.IntToStr(CTimer.GetMinute()), "0", 2, false) +
                                CConvert.FillString(CConvert.IntToStr(CTimer.GetSecond()), "0", 2, false)
                               );
                    if (bSeparation == true) strb.Append(@"\");
                }

                return strb.ToString();
            }
            #endregion Make a folder(Kor: 폴더 생성)
            #endregion File & Folder Management Function[Kor: 파일 및 폴더관리]

            //private int m_nEncType = 0; // 0: default, 1: UTF8, 2: Unicode, 3: 중국어 간체
            private static Encoding m_Enc = null;
            public void EncodingType(Encoding Enc) { m_Enc = Enc; }

            #region Files(Data) & List
            private List<String> m_lstFile = new List<string>();
            // Move file datas to list memory(Kor: 파일의 내용을 리스트 메모리로 옮겨준다.)
            public int Load(int nCnt, String strFileName)
            {
                m_lstFile.Clear();

                int nLines = 0;
                byte[] aByteData;
                FileInfo f = null;
                FileStream fs = null;
                try
                {
                    f = new FileInfo(strFileName);
                    fs = f.OpenRead();
                    aByteData = new byte[fs.Length];
                    fs.Read(aByteData, 0, aByteData.Length);

                    fs.Close();
                    fs = null;
                    f = null;
                    ////////////////////
                    //int nLines = 0;
                    //lstParam.Items.Clear();
                    String strItem = "";
                    byte[] pbyte = new byte[1024];
                    int ii = 0;
                    foreach (byte byteItem in aByteData)
                    {
#if true
                        if (byteItem != 10)
                        {
                            //strItem += (char)byteItem;
                            if ((byteItem != 0) && (byteItem != 13))
                                pbyte[ii++] = byteItem;
                        }
                        else
                        {
                            //if (strItem != null)
                            if (pbyte.Length > 0)
                            {
#if false
                                //if (_ENCODING_UTF8 == nEncodingType)
                                //strItem = System.Text.Encoding.UTF8.GetString(pbyte, 0, ii);
                                //else // _ENCODING_DEFAULT
                                strItem = System.Text.Encoding.Default.GetString(pbyte, 0, ii);
#else
                                if (m_Enc == null) strItem = System.Text.Encoding.Default.GetString(pbyte, 0, ii);
                                else strItem = m_Enc.GetString(pbyte, 0, ii);
#endif
                                strItem = strItem.Trim();
                                if (strItem.Length >= 1) // o Type 1자리 // x Index 4자리, Type 1자리
                                    m_lstFile.Add(strItem);
                                nLines++;
                            }
                            //strItem = "";
                            Array.Clear(pbyte, 0, pbyte.Length);
                            ii = 0;
                        }
#else
                    if (byteItem != 10) strItem += (char)byteItem;
                    else
                    {
                        if (strItem != null)
                        {
                            strItem = strItem.Trim();
                            if (strItem.Length >= 1) // o Type 1자리 // x Index 4자리, Type 1자리
                                m_lstFile.Add(strItem);
                            nLines++;
                        }
                        strItem = "";
                    }
#endif
                    }
                    if (nLines < nCnt)
                    {
                        for (int i = 0; i < nCnt - nLines; i++)
                            m_lstFile.Add("s0");
                    }
                    return nLines;
                }
                catch
                {
                    if (nLines < nCnt)
                    {
                        for (int i = nLines; i < nCnt - nLines; i++)
                            m_lstFile.Add("s0");
                    }
                    return nLines;
                }
            }
            public int Load(String strFileName)
            {
                m_lstFile.Clear();

                int nLines = 0;
                byte[] aByteData;
                FileInfo f = null;
                FileStream fs = null;
                try
                {
                    f = new FileInfo(strFileName);
                    fs = f.OpenRead();
                    aByteData = new byte[fs.Length];
                    fs.Read(aByteData, 0, aByteData.Length);

                    fs.Close();
                    fs = null;
                    f = null;
                    ////////////////////
                    //int nLines = 0;
                    //lstParam.Items.Clear();
                    String strItem = "";
                    byte[] pbyte = new byte[1024];
                    int ii = 0;
                    foreach (byte byteItem in aByteData)
                    {
#if true
                        if (byteItem != 10)
                        {
                            //strItem += (char)byteItem;
                            if ((byteItem != 0) && (byteItem != 13))
                                pbyte[ii++] = byteItem;
                        }
                        else
                        {
                            //if (strItem != null)
                            if (pbyte.Length > 0)
                            {
#if false
                                //if (_ENCODING_UTF8 == nEncodingType)
                                //strItem = System.Text.Encoding.UTF8.GetString(pbyte, 0, ii);
                                //else // _ENCODING_DEFAULT
                                strItem = System.Text.Encoding.Default.GetString(pbyte, 0, ii);
#else
                                if (m_Enc == null) strItem = System.Text.Encoding.Default.GetString(pbyte, 0, ii);
                                else strItem = m_Enc.GetString(pbyte, 0, ii);
#endif
                                strItem = strItem.Trim();
                                if (strItem.Length >= 1) // o Type 1자리 // x Index 4자리, Type 1자리
                                {
                                    m_lstFile.Add(strItem);
                                    nLines++;
                                }
                            }
                            //strItem = "";
                            Array.Clear(pbyte, 0, pbyte.Length);
                            ii = 0;
                        }
#else
                    if (byteItem != 10) strItem += (char)byteItem;
                    else
                    {
                        if (strItem != null)
                        {
                            strItem = strItem.Trim();
                            if (strItem.Length >= 1) // o Type 1자리 // x Index 4자리, Type 1자리
                                m_lstFile.Add(strItem);
                            nLines++;
                        }
                        strItem = "";
                    }
#endif
                    }
                    // line == 0 인경우
                    if ((nLines == 0) && (aByteData.Length > 0))
                    {
                        strItem = strItem.Trim();
                        if (strItem.Length >= 1) // o Type 1자리 // x Index 4자리, Type 1자리
                            m_lstFile.Add(strItem);
                        nLines++;
                    }
                    return nLines;
                }
                catch
                {
                    return nLines;
                }
            }
            public bool Load(String strFileName, out byte[] aByteData)
            {
                aByteData = null;
                FileInfo f = null;
                FileStream fs = null;
                try
                {
                    f = new FileInfo(strFileName);
                    fs = f.OpenRead();
                    aByteData = new byte[fs.Length];
                    fs.Read(aByteData, 0, aByteData.Length);

                    fs.Close();
                    fs = null;
                    f = null;
                    if (aByteData != null)
                    {
                        if (aByteData.Length > 0)
                            return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
            // Save file with list(Kor: 리스트의 내용을 파일로 저장한다.)
            public bool Save(String strFileName)
            {
                FileInfo f = null;
                StreamWriter fs = null;

                try
                {
                    f = new FileInfo(strFileName);
#if false
                    fs = new StreamWriter(strFileName, false, Encoding.Default);
                    //fs = new StreamWriter(strFileName, false, Encoding.UTF8);
                    //fs = new StreamWriter(strFileName, false, Encoding.Unicode);
#else
                    if (m_Enc == null) fs = new StreamWriter(strFileName, false, Encoding.Default);
                    else fs = new StreamWriter(strFileName, false, m_Enc);
#endif
                    fs.Flush(); // Flush the stream buffers

                    for (int i = 0; i < m_lstFile.Count; i++) fs.WriteLine((String)m_lstFile[i]);

                    fs.Close();
                    return true;
                }
                catch
                {
                    if (fs != null) fs.Close();
                    return false;
                }
            }
            //public bool Save_Csv(String strFileName)
            //{
            //    FileInfo f = null;
            //    StreamWriter fs = null;

            //    try
            //    {
            //        f = new FileInfo(strFileName);
            //        if (m_Enc == null) fs = new StreamWriter(strFileName, false, Encoding.Default);
            //        else fs = new StreamWriter(strFileName, false, m_Enc);

            //        fs.Flush(); // Flush the stream buffers

            //        for (int i = 0; i < m_lstFile.Count; i++) fs.WriteLine((String)m_lstFile[i]);

            //        fs.Close();
            //        return true;
            //    }
            //    catch
            //    {
            //        if (fs != null) fs.Close();
            //        return false;
            //    }
            //}
            
            // return b,n,f,s   
            // return "" when it has error(Kor: b,n,f,s 를 반환 "" 이면 에러)
            public bool GetData_ListBox(ref ListBox lst) { try { lst.Items.Clear(); for (int i = 0; i < m_lstFile.Count; i++) lst.Items.Add(m_lstFile[i]); return true; } catch { return false; } }
            public String CheckData(int nIndex)
            {
                try
                {
                    String strItem = (String)m_lstFile[nIndex];
                    return strItem.Substring(0, 1);
                }
                catch
                {
                    return "";
                }
            }
            public int Get_Count() { return m_lstFile.Count; }
            public String[] Get() { return m_lstFile.ToArray(); }
            public String Get(int nIndex) { return GetData_String(nIndex); }
            public String GetData(int nIndex) { return GetData_String(nIndex); }
            //public String GetData_String(int nIndex) { try { String strItem = (string)m_lstFile[nIndex].Clone(); strItem = strItem.Remove(0, 1); return strItem; } catch { return ""; } }
            public String GetData_String(int nIndex) { try { String strItem = (String)m_lstFile[nIndex].Substring(1, m_lstFile[nIndex].Length - 1); return strItem; } catch { return ""; } }
            public String GetData_Raw(int nIndex) { try { return (String)m_lstFile[nIndex]; } catch { return ""; } }

            public int GetData_Int(int nIndex) { return CConvert.StrToInt(GetData_String(nIndex)); }
            public float GetData_Float(int nIndex) { return CConvert.StrToFloat(GetData_String(nIndex)); }
            public double GetData_Double(int nIndex) { return CConvert.StrToDouble(GetData_String(nIndex)); }
            public bool GetData_Bool(int nIndex) { return CConvert.StrToBool(GetData_String(nIndex)); }
            public bool Set(int nIndex, String strValue) { return SetData_String(nIndex, strValue); }
            public bool SetData(int nIndex, String strValue) { return SetData_String(nIndex, strValue); }

            public void Clear() { m_lstFile.Clear(); }
            public bool Add(String strValue) { return Add_Raw("s" + strValue); }
            public bool Add_Raw(String strValue) { try { m_lstFile.Add(strValue); return true; } catch { return false; } }
            
            public bool SetData_Raw(int nIndex, String strValue) { try { m_lstFile[nIndex] = strValue; return true; } catch { return false; } }
            public bool SetData_String(int nIndex, String strValue) { try { if (m_lstFile.Count < nIndex + 1) { for (int i = m_lstFile.Count; i <= nIndex; i++) Add(""); } m_lstFile[nIndex] = "s" + strValue; return true; } catch {  return false;  } }
            public bool SetData_Int(int nIndex, int nValue) { try { if (m_lstFile.Count < nIndex + 1) { for (int i = m_lstFile.Count; i <= nIndex; i++) Add(""); } m_lstFile[nIndex] = "n" + CConvert.IntToStr(nValue); return true; } catch { return false; } }
            public bool SetData_Float(int nIndex, float fValue) { try { if (m_lstFile.Count < nIndex + 1) { for (int i = m_lstFile.Count; i <= nIndex; i++) Add(""); } m_lstFile[nIndex] = "f" + CConvert.FloatToStr(fValue); return true; } catch { return false; } }
            public bool SetData_double(int nIndex, double dValue) { try { if (m_lstFile.Count < nIndex + 1) { for (int i = m_lstFile.Count; i <= nIndex; i++) Add(""); } m_lstFile[nIndex] = "d" + CConvert.DoubleToStr(dValue); return true; } catch { return false; } }
            public bool SetData_Bool(int nIndex, bool bValue) { try { if (m_lstFile.Count < nIndex + 1) { for (int i = m_lstFile.Count; i <= nIndex; i++) Add(""); } m_lstFile[nIndex] = "b" + CConvert.BoolToStr(bValue); return true; } catch { return false; } }
            #endregion Files(Data) & List

            public static void Delete(String strFileName) { FileInfo file = new FileInfo(strFileName); if (file.Exists) { System.IO.File.Delete(strFileName); } }
            public static bool Write(String strFileName, string strMsg, bool bNew) { return Write(Encoding.Unicode, strFileName, strMsg, bNew); }
            public static bool Write(Encoding Enc, String strFileName, String strMsg, bool bNew)
            {
                FileInfo f = null;
                StreamWriter fs = null;

                try
                {
                    f = new FileInfo(strFileName);
#if false
                    fs = new StreamWriter(strFileName, !bNew, Encoding.Default);
                    //fs = new StreamWriter(strFileName, !bNew, Encoding.UTF8);
                    //fs = new StreamWriter(strFileName, !bNew, Encoding.Unicode);
#else
                    if (Enc == null) fs = new StreamWriter(strFileName, !bNew, Encoding.Default);
                    else fs = new StreamWriter(strFileName, !bNew, Enc);
#endif


                    fs.Flush(); 

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
            public static bool Read(String strFileName, TextBox txtFile) { return Read(Encoding.Unicode,  strFileName,  txtFile); }
            public static bool Read(Encoding Enc, String strFileName, TextBox txtFile)
            {
                FileInfo f = null;
                //StreamReader fs = null;
                FileStream fs = null;

                try
                {
                    f = new FileInfo(strFileName);
                    //fs = new StreamReader(strFileName, Encoding.Default);
                    fs = f.OpenRead();

                    byte[] pbyteData = new byte[fs.Length];
                    fs.Read(pbyteData, 0, pbyteData.Length);
                    
#if true
                    if (Enc == null) txtFile.Text = System.Text.Encoding.Default.GetString(pbyteData, 0, pbyteData.Length);
                    else txtFile.Text = Enc.GetString(pbyteData, 0, pbyteData.Length);
#else
                    foreach (byte byteData in pbyteData) { txtFile.Text += (char)byteData; }
#endif               
                    fs.Close();

                    return true;
                }
                catch
                {
                    if (fs != null) fs.Close();
                    return false;
                }
            }
            public static bool Read(String strFileName, ref String strReturn) { return Read(Encoding.Unicode, strFileName, ref strReturn); }
            public static bool Read(Encoding Enc, String strFileName, ref String strReturn)
            {
                FileInfo f = null;
                FileStream fs = null;

                try
                {
                    f = new FileInfo(strFileName);
                    fs = f.OpenRead();

                    byte[] pbyteData = new byte[fs.Length];
                    fs.Read(pbyteData, 0, pbyteData.Length);

#if true
                    if (Enc == null) strReturn = System.Text.Encoding.Default.GetString(pbyteData, 0, pbyteData.Length);
                    else strReturn = Enc.GetString(pbyteData, 0, pbyteData.Length);
#else
                    foreach (byte byteData in pbyteData) { strReturn += (char)byteData; }
#endif
                    fs.Close();

                    return true;
                }
                catch
                {
                    if (fs != null) fs.Close();
                    return false;
                }
            }

            // binary
            public static byte[] Read(string strFileName)
            {
                FileInfo f = null;
                FileStream fs = null;

                try
                {
                    f = new FileInfo(strFileName);
                    fs = f.OpenRead();

                    byte[] pbyteData = new byte[fs.Length];
                    fs.Read(pbyteData, 0, pbyteData.Length);
                    fs.Close();

                    return pbyteData;
                }
                catch
                {
                    if (fs != null) fs.Close();
                    return null;
                }
            }
            public static bool Write(String strFileName, byte[] pbyData)
            {
                FileInfo f = new FileInfo(strFileName);
                FileStream fs = f.Create();
                try
                {
                    fs.Flush();
                    fs.Write(pbyData, 0, pbyData.Length);
                    fs.Close();
                    return true;
                }
                catch
                {
                    if (fs != null) fs.Close();
                    return false;
                }
            }


            public static bool IsValue(string[] pstrOrg, string strValue) { foreach (string strItem in pstrOrg) { if (strItem == strValue) return true; } return false; }
            public static string[] GetDirectory_list(string strSource, bool bSubDirs)
            {
                List<string> lstDirectories = new List<string>();
                lstDirectories.Clear();

                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(strSource);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + strSource);
                }

                lstDirectories.Add(dir.FullName);

                DirectoryInfo[] dirs = dir.GetDirectories();

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    lstDirectories.Add(String.Format("  \"{0}\"", file.Name));
                    //Ojw.CMessage.Write2(txtInfo, "  \"{0}\"\r\n", file.Name);
                }

                // If copying subdirectories, copy them and their contents to new location.
                if (bSubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string[] pstrList = GetDirectory_list(subdir.FullName, bSubDirs);
                        foreach (string strList in pstrList) lstDirectories.Add(strList);
                    }
                }
                if (lstDirectories.Count > 0) return lstDirectories.ToArray();
                return null;
            }
            
            public static bool CompareDirectory_List_Both(string strSrc, string strDst, bool bSubDirs, bool bAddStatusString, ref List<string> lstSrc, ref List<string> lstDst)
            {
                bool bChanged = false;
                if (CompareDirectory_List(strSrc, strDst, bSubDirs, bAddStatusString, ref lstSrc, ref lstDst) == true) bChanged = true;
                if (CompareDirectory_List(strDst, strSrc, bSubDirs, bAddStatusString, ref lstDst, ref lstSrc) == true) bChanged = true;
                return bChanged;
            }
            public static bool IsDirectory(string strPath) {
                try
                {
                    DirectoryInfo dirSrc = new DirectoryInfo(strPath); return dirSrc.Exists;
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write("[Warning] {0}", ex.ToString());
                    return false;
                }
            }
            public static string[] GetFileNames(string strExe = "", string strPath = "", bool bRemovePathString = true)
            {
                List<String> lst = new List<string>();
                bool bExe = (strExe.Length > 0) ? true : false;
                if (strPath.Length == 0) strPath = Application.StartupPath;
                DirectoryInfo dir = new DirectoryInfo(strPath);
                if (dir.Exists)
                {                    
                    FileInfo[] filesSrc = dir.GetFiles();
                    foreach (FileInfo file in filesSrc)
                    {
                        if (bExe) { if (file.Extension.Substring(1) != strExe) continue; }

                        if (bRemovePathString) lst.Add(GetName(file.FullName));
                        else lst.Add(file.FullName);
                    }
                }
                return lst.ToArray();
            }
            public static bool CompareDirectory_List(string strSrc, string strDst, bool bSubDirs, bool bAddStatusString, ref List<string> lstSrc, ref List<string> lstDst)
            {
                if ((lstSrc == null) || (lstDst == null)) return false;

                bool bChanged = false;
                //List<string> lstDirectories = new List<string>();
                //lstDirectories.Clear();

                // Get the subdirectories for the specified directory.
                DirectoryInfo dirSrc = new DirectoryInfo(strSrc);
                DirectoryInfo dirDst = new DirectoryInfo(strDst);


                if ((!dirSrc.Exists) && (!dirDst.Exists)) return false;

                if (!dirSrc.Exists)
                {
                    bChanged = true;
                    // 소스의 폴더가 없는 경우
                    FileInfo[] filesSrc = dirDst.GetFiles();
                    foreach (FileInfo file in filesSrc)
                    {
#if false
                        if (bAddStatusString == true)
                        {
                            if (IsValue(lstSrc.ToArray(), string.Format("[n]{0}", file.FullName)) == false)
                                lstSrc.Add(string.Format("[n]{0}", file.FullName));
                        }
                        else
                        {
                            if (IsValue(lstSrc.ToArray(), file.FullName) == false)
                                lstSrc.Add(String.Format("{0}", file.FullName));
                        }
#else
                        if (bAddStatusString == true)
                        {
                            if (IsValue(lstDst.ToArray(), string.Format("[n]{0}", file.FullName)) == false)
                                lstDst.Add(string.Format("[n]{0}", file.FullName));
                        }
                        else
                        {
                            if (IsValue(lstDst.ToArray(), file.FullName) == false)
                                lstDst.Add(String.Format("{0}", file.FullName));
                        }
#endif
                    } 
                    if (bSubDirs)
                    {
                        DirectoryInfo[] pdirDst = dirDst.GetDirectories();
                        foreach (DirectoryInfo subdir in pdirDst)
                        {
                            string tmpSrc = Path.Combine(strSrc, subdir.Name);
                            string tmpDst = Path.Combine(strDst, subdir.Name);
                            //string[] pstrList = GetDirectory_list(subdir.FullName, bSubDirs);
                            //foreach (string strList in pstrList) lstDirectories.Add(strList);
                            if (CompareDirectory_List(tmpSrc, tmpDst, bSubDirs, bAddStatusString, ref lstSrc, ref lstDst) == true) bChanged = true;
                        }
                    }
                    return bChanged;
                    //bChanged = true;
                    //return true;
                }
                if (!dirDst.Exists)
                {
                    bChanged = true;
                    // 타겟의 폴더가 없는 경우
                    FileInfo[] filesDst = dirSrc.GetFiles();
                    foreach (FileInfo file in filesDst)
                    {
#if false
                        if (bAddStatusString == true)
                        {
                            if (IsValue(lstDst.ToArray(), string.Format("[n]{0}", file.FullName)) == false)
                                lstDst.Add(string.Format("[n]{0}", file.FullName));
                        }
                        else
                        {
                            if (IsValue(lstDst.ToArray(), file.FullName) == false)
                                lstDst.Add(String.Format("{0}", file.FullName));
                        }
#else
                        if (bAddStatusString == true)
                        {
                            if (IsValue(lstSrc.ToArray(), string.Format("[n]{0}", file.FullName)) == false)
                                lstSrc.Add(string.Format("[n]{0}", file.FullName));
                        }
                        else
                        {
                            if (IsValue(lstSrc.ToArray(), file.FullName) == false)
                                lstSrc.Add(String.Format("{0}", file.FullName));
                        }
#endif
                    }
                    if (bSubDirs)
                    {
                        DirectoryInfo[] pdirSrc = dirSrc.GetDirectories();
                        foreach (DirectoryInfo subdir in pdirSrc)
                        {
                            string tmpSrc = Path.Combine(strSrc, subdir.Name);
                            string tmpDst = Path.Combine(strDst, subdir.Name);
                            //string[] pstrList = GetDirectory_list(subdir.FullName, bSubDirs);
                            //foreach (string strList in pstrList) lstDirectories.Add(strList);
                            if (CompareDirectory_List(tmpSrc, tmpDst, bSubDirs, bAddStatusString, ref lstSrc, ref lstDst) == true) bChanged = true;
                        }
                    }
                    return bChanged;
                    //bChanged = true;
                    //return true;
                }


                //if (bChanged == false)
                {
                    DirectoryInfo[] pdirSrc = dirSrc.GetDirectories();
                    DirectoryInfo[] pdirDst = dirDst.GetDirectories();

                    FileInfo[] filesSrc = dirSrc.GetFiles();
                    FileInfo[] filesDst = dirDst.GetFiles();
                    if (filesDst.Length > filesSrc.Length)
                    {
                        int nError = 0;
                        for (int j = 0; j < filesDst.Length; j++)
                        {
                            int nSrc = -1;
                            int nDst = -1;
                            bool bIsFile = false;
                            bool bFind = false;
                            for (int i = 0; i < filesSrc.Length; i++)
                            {
                                if (filesSrc[i].Name == filesDst[j].Name)
                                {
                                    if (Ojw.CFile.GetFile_LastWriteTime(filesSrc[i].FullName) != Ojw.CFile.GetFile_LastWriteTime(filesDst[j].FullName))
                                    {
                                        bFind = true;
                                        nSrc = i;
                                        nDst = j;
                                        break;
                                    }
                                }
                            }
                            if (bFind == true)
                            {
                                if (Ojw.CFile.GetFile_LastWriteTime(filesSrc[nSrc].FullName).Ticks > Ojw.CFile.GetFile_LastWriteTime(filesDst[nDst].FullName).Ticks)
                                {
                                    if (bAddStatusString == true)
                                    {
                                        if (IsValue(lstSrc.ToArray(), string.Format("[m]{0}", filesSrc[nSrc].FullName)) == false)
                                            lstSrc.Add(string.Format("[m]{0}", filesSrc[nSrc].FullName));
                                    }
                                    else
                                    {
                                        if (IsValue(lstSrc.ToArray(), filesSrc[nSrc].FullName) == false)
                                            lstSrc.Add(filesSrc[nSrc].FullName);
                                    }
                                }
                                else
                                {
                                    if (bAddStatusString == true)
                                    {
                                        if (IsValue(lstDst.ToArray(), string.Format("[m]{0}", filesDst[nDst].FullName)) == false)
                                            lstDst.Add(string.Format("[m]{0}", filesDst[nDst].FullName));
                                    }
                                    else
                                    {
                                        if (IsValue(lstDst.ToArray(), filesDst[nDst].FullName) == false)
                                            lstDst.Add(filesDst[nDst].FullName);
                                    }
                                }
                            }
                            else if (bIsFile == false)
                            {
                                if (bAddStatusString == true)
                                {
                                    // [n] new, [m] modify
                                    if (IsValue(lstSrc.ToArray(), string.Format("[n]{0}", filesDst[j].FullName)) == false)
                                        lstSrc.Add(string.Format("[n]{0}", filesDst[j].FullName));
                                }
                                else
                                {
                                    if (IsValue(lstSrc.ToArray(), filesDst[j].FullName) == false)
                                        lstSrc.Add(filesDst[j].FullName);
                                }
                                nError++;
                            }
                        }
                    }
                    else if (filesDst.Length < filesSrc.Length)
                    {
                        int nError = 0;
                        for (int i = 0; i < filesSrc.Length; i++)
                        {
                            int nSrc = -1;
                            int nDst = -1;
                            bool bIsFile = false;
                            bool bFind = false;
                            for (int j = 0; j < filesDst.Length; j++)
                            {
                                if (filesSrc[i].Name == filesDst[j].Name)
                                {
                                    if (Ojw.CFile.GetFile_LastWriteTime(filesSrc[i].FullName) != Ojw.CFile.GetFile_LastWriteTime(filesDst[j].FullName))
                                    {
                                        bFind = true;
                                        nSrc = i;
                                        nDst = j;
                                        break;
                                    }
                                    //bIsFile = true;
                                }
                            }
                            if (bFind == true)
                            {
                                if (Ojw.CFile.GetFile_LastWriteTime(filesSrc[nSrc].FullName).Ticks > Ojw.CFile.GetFile_LastWriteTime(filesDst[nDst].FullName).Ticks)
                                {
                                    if (bAddStatusString == true)
                                    {
                                        if (IsValue(lstSrc.ToArray(), string.Format("[m]{0}", filesSrc[nSrc].FullName)) == false)
                                            lstSrc.Add(string.Format("[m]{0}", filesSrc[nSrc].FullName));
                                    }
                                    else
                                    {
                                        if (IsValue(lstSrc.ToArray(), filesSrc[nSrc].FullName) == false)
                                            lstSrc.Add(filesSrc[nSrc].FullName);
                                    }
                                }
                                else
                                {
                                    if (bAddStatusString == true)
                                    {
                                        if (IsValue(lstDst.ToArray(), string.Format("[m]{0}", filesDst[nDst].FullName)) == false)
                                            lstDst.Add(string.Format("[m]{0}", filesDst[nDst].FullName));
                                    }
                                    else
                                    {
                                        if (IsValue(lstDst.ToArray(), filesDst[nDst].FullName) == false)
                                            lstDst.Add(filesDst[nDst].FullName);
                                    }
                                }
                            }
                            else if (bIsFile == false)
                            {
                                if (bAddStatusString == true)
                                {
                                    // [n] new, [m] modify
                                    if (IsValue(lstSrc.ToArray(), string.Format("[n]{0}", filesSrc[i].FullName)) == false)
                                        lstSrc.Add(string.Format("[n]{0}", filesSrc[i].FullName));
                                }
                                else
                                {
                                    if (IsValue(lstSrc.ToArray(), filesSrc[i].FullName) == false)
                                        lstSrc.Add(filesSrc[i].FullName);
                                }
                                nError++;
                            }
                        }
                    }
                    else
                    {
                        int nError = 0;
                        for (int i = 0; i < filesSrc.Length; i++)
                        {
                            int nSrc = -1;
                            int nDst = -1;
                            bool bIsFile = false;
                            bool bFind = false;
                            for (int j = 0; j < filesDst.Length; j++)
                            {
                                if (filesSrc[i].Name == filesDst[j].Name)
                                {
                                    if (Ojw.CFile.GetFile_LastWriteTime(filesSrc[i].FullName) != Ojw.CFile.GetFile_LastWriteTime(filesDst[j].FullName))
                                    {
                                        bFind = true;
                                        nSrc = i;
                                        nDst = j;
                                        break;
                                    }
                                    bIsFile = true;
                                }
                            }
                            //if (bIsFile == false)
                            if (bFind == true)
                            {
                                bChanged = true;
                                if (Ojw.CFile.GetFile_LastWriteTime(filesSrc[nSrc].FullName).Ticks > Ojw.CFile.GetFile_LastWriteTime(filesDst[nDst].FullName).Ticks)
                                {
                                    if (bAddStatusString == true)
                                    {
                                        if (IsValue(lstSrc.ToArray(), string.Format("[m]{0}", filesSrc[nSrc].FullName)) == false)
                                            lstSrc.Add(string.Format("[m]{0}", filesSrc[nSrc].FullName));
                                    }
                                    else
                                    {
                                        if (IsValue(lstSrc.ToArray(), filesSrc[nSrc].FullName) == false)
                                            lstSrc.Add(filesSrc[nSrc].FullName);
                                    }
                                }
                                else
                                {
                                    if (bAddStatusString == true)
                                    {
                                        if (IsValue(lstDst.ToArray(), string.Format("[m]{0}", filesDst[nDst].FullName)) == false)
                                            lstDst.Add(string.Format("[m]{0}", filesDst[nDst].FullName));
                                    }
                                    else
                                    {
                                        if (IsValue(lstDst.ToArray(), filesDst[nDst].FullName) == false)
                                            lstDst.Add(filesDst[nDst].FullName);
                                    }
                                }
                            }
                            else if (bIsFile == false)
                            {
                                if (bAddStatusString == true)
                                {
                                    // [n] new, [m] modify
                                    if (IsValue(lstSrc.ToArray(), string.Format("[n]{0}", filesSrc[i].FullName)) == false)
                                        lstSrc.Add(string.Format("[n]{0}", filesSrc[i].FullName));
                                }
                                else
                                {
                                    if (IsValue(lstSrc.ToArray(), filesSrc[i].FullName) == false)
                                        lstSrc.Add(filesSrc[i].FullName);
                                }
                                nError++;
                            }
                        }
                        if (nError > 0)
                        {
                            // 파일이 갯수가 같지만 동일하지 않은 경우 매칭을 역으로 진행해서 확인해야 한다.
                            for (int i = 0; i < filesDst.Length; i++)
                            {
                                bool bFind = false;
                                for (int j = 0; j < filesSrc.Length; j++)
                                {
                                    if (filesSrc[i].FullName == filesDst[j].FullName)
                                    {
                                        if (Ojw.CFile.GetFile_LastWriteTime(filesSrc[i].FullName) != Ojw.CFile.GetFile_LastWriteTime(filesDst[j].FullName))
                                        {
                                            bFind = true;
                                            break;
                                        }
                                    }
                                }
                                if (bFind == false)
                                {
                                    if (bAddStatusString == true)
                                    {
                                        // [n] new, [m] modify
                                        if (IsValue(lstDst.ToArray(), string.Format("[n]{0}", filesDst[i].FullName)) == false)
                                            lstDst.Add(string.Format("[n]{0}", filesDst[i].FullName));
                                    }
                                    else
                                    {
                                        if (IsValue(lstDst.ToArray(), filesDst[i].FullName) == false)
                                            lstDst.Add(filesDst[i].FullName);
                                    }
                                }
                            }
                        }
                    }
                    // If copying subdirectories, copy them and their contents to new location.
                    if (bSubDirs)
                    {
                        foreach (DirectoryInfo subdir in pdirSrc)
                        {
                            string tmpSrc = Path.Combine(strSrc, subdir.Name);
                            string tmpDst = Path.Combine(strDst, subdir.Name);
                            //string[] pstrList = GetDirectory_list(subdir.FullName, bSubDirs);
                            //foreach (string strList in pstrList) lstDirectories.Add(strList);
                            if (CompareDirectory_List(tmpSrc, tmpDst, bSubDirs, bAddStatusString, ref lstSrc, ref lstDst) == true) bChanged = true;
                        }
                    }
                }
                return bChanged;
            }
            // https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
            public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
            {
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirName);
                }

                DirectoryInfo[] dirs = dir.GetDirectories();
                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, true);
                }

                // If copying subdirectories, copy them and their contents to new location.
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }
            }
            public static string[] FileCopy(string destDirName, string[] pstrFileNames)
            {
                List<string> lstMissing = new List<string>();
                lstMissing.Clear();

                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                // Get the files in the directory and copy them to the new location.
                //string[] pstrFiles = pstrFiles;//dir.GetFiles();
                foreach (string strFile in pstrFileNames)
                {
                    FileInfo file = new FileInfo(strFile);
                    if (file.Exists == false)
                    {
                        lstMissing.Add(strFile);
                        continue;
                    }
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, true);
                }

                return (string[])lstMissing.ToArray();
            }
            public static string [] FileCopy(string sourceDirName, string destDirName, string [] pstrFileNames)
            {
                List<string> lstMissing = new List<string>();
                lstMissing.Clear();
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirName);
                }

                DirectoryInfo[] dirs = dir.GetDirectories();
                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                // Get the files in the directory and copy them to the new location.
                //string[] pstrFiles = pstrFiles;//dir.GetFiles();
                foreach (string strFile in pstrFileNames)
                {
                    FileInfo file = new FileInfo(String.Format("{0}\\{1}", sourceDirName, strFile));
                    if (file.Exists == false)
                    {
                        lstMissing.Add(strFile);
                        continue;
                    }
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, true);
                }
                
                return (string [])lstMissing.ToArray();
            }

            
            public static string FindFile(string strFileName) { return FindFile(null, strFileName); }
            public static string FindFile(string strPath, string strFileName)
            {
                try
                {
                    string strResult = String.Empty;
                    if (strPath == null) strPath = Application.StartupPath;
                    foreach (string strDirectory in Directory.GetDirectories(strPath))
                    {
                        strResult = String.Format("{0}\\{1}", strDirectory, strFileName);
                        //foreach (string strFile in Directory.GetFiles(strDirectory, strFileName))
                        //{
                        //    if (IsFile(strFile) == true) { return String.Format("{0}\\{1}", strDirectory, strFileName); }
                        //}
                        if (IsFile(strResult) == true) return strResult;
                        else
                        {
                            strResult = FindFile(strDirectory, strFileName);
                            if (IsFile(strResult) == true) return strResult;
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    return null;// String.Empty;
                }
            }
        }
        #endregion COjwFile
    }
}
