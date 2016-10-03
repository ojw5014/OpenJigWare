
using System;
using System.Collections.Generic;
using System.Linq;
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
                                //if (_ENCODING_UTF8 == nEncodingType)
                                //strItem = System.Text.Encoding.UTF8.GetString(pbyte, 0, ii);
                                //else // _ENCODING_DEFAULT
                                strItem = System.Text.Encoding.Default.GetString(pbyte, 0, ii);


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
            // Save file with list(Kor: 리스트의 내용을 파일로 저장한다.)
            public bool Save(String strFileName)
            {
                FileInfo f = null;
                StreamWriter fs = null;

                try
                {
                    f = new FileInfo(strFileName);
                    fs = new StreamWriter(strFileName, false, Encoding.Default);
                    //fs = new StreamWriter(strFileName, false, Encoding.UTF8);

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
            public String GetData_String(int nIndex) { try { String strItem = (String)m_lstFile[nIndex]; return strItem.Substring(1); } catch { return ""; } }
            public int GetData_Int(int nIndex) { return CConvert.StrToInt(GetData_String(nIndex)); }
            public float GetData_Float(int nIndex) { return CConvert.StrToFloat(GetData_String(nIndex)); }
            public double GetData_Double(int nIndex) { return CConvert.StrToDouble(GetData_String(nIndex)); }
            public bool GetData_Bool(int nIndex) { return CConvert.StrToBool(GetData_String(nIndex)); }
            public bool SetData_String(int nIndex, String strValue) { try { m_lstFile[nIndex] = "s" + strValue; return true; } catch { return false; } }
            public bool SetData_Int(int nIndex, int nValue) { try { m_lstFile[nIndex] = "n" + CConvert.IntToStr(nValue); return true; } catch { return false; } }
            public bool SetData_Float(int nIndex, float fValue) { try { m_lstFile[nIndex] = "f" + CConvert.FloatToStr(fValue); return true; } catch { return false; } }
            public bool SetData_double(int nIndex, double dValue) { try { m_lstFile[nIndex] = "d" + CConvert.DoubleToStr(dValue); return true; } catch { return false; } }
            public bool SetData_Bool(int nIndex, bool bValue) { try { m_lstFile[nIndex] = "b" + CConvert.BoolToStr(bValue); return true; } catch { return false; } }
            #endregion Files(Data) & List

            public static void Delete(String strFileName) { FileInfo file = new FileInfo(strFileName); if (file.Exists) { System.IO.File.Delete(strFileName); } }
            public static bool Write(String strFileName, String strMsg, bool bNew)
            {
                FileInfo f = null;
                StreamWriter fs = null;

                try
                {
                    f = new FileInfo(strFileName);
                    fs = new StreamWriter(strFileName, !bNew, Encoding.Default);
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
            public static bool Read(String strFileName, TextBox txtFile)
            {                
                FileInfo f = null;
                //StreamReader fs = null;
                FileStream fs = null;

                try
                {
                    f = new FileInfo(strFileName);
                    //fs = new StreamReader(strFileName, Encoding.Default);
                    fs = f.OpenRead();

                    byte [] pbyteData = new byte[fs.Length];
                    fs.Read(pbyteData, 0, pbyteData.Length);                    
                    foreach(byte byteData in pbyteData) { txtFile.Text += (char)byteData; }
                    fs.Close();

                    return true;
                }
                catch
                {
                    if (fs != null) fs.Close();
                    return false;
                }
            }
        }
        #endregion COjwFile
    }
}
