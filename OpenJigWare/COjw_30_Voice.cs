using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;

//using System.Speech.Recognition;

using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;

using System.Speech.Synthesis;
using System.Diagnostics;
using System.Windows.Forms;

namespace OpenJigWare
{
#if false
    partial class Ojw
    {
        public class CTts
        {
            public enum ELanguage_t
            {
                _KOREAN,
                _ENGLISH_US,
                _CHINA
            }
            private readonly string[] m_pstrLang_Val = new string[] { 
                "Microsoft Server Speech Text to Speech Voice (ko-KR, Heami)", 
                "Microsoft Server Speech Text to Speech Voice (en-US, Helen)", 
                "Microsoft Server Speech Text to Speech Voice (zh-CN, HuiHui)" 
            };
            private ELanguage_t m_ELang = ELanguage_t._ENGLISH_US;
            private System.Speech.Synthesis.SpeechSynthesizer m_ttsCmd_System;
            private Microsoft.Speech.Synthesis.SpeechSynthesizer m_ttsCmd;
            public void Init(int nLan) { if (nLan < 0) nLan = (int)ELanguage_t._KOREAN; else if (nLan >= 3) nLan = (int)ELanguage_t._CHINA; Init((ELanguage_t)nLan, 100); }
            public void Init(ELanguage_t ELan) { Init( ELan, 100); }
            public void Init(ELanguage_t ELan, int nVolum_100)
            {
                try
                {
                    if (nVolum_100 < 0) nVolum_100 = 0;
                    else if (nVolum_100 > 100) nVolum_100 = 100;
                    m_ELang = ELan;
                    if (ELan == ELanguage_t._CHINA)
                    {
                        m_ttsCmd_System = new System.Speech.Synthesis.SpeechSynthesizer();

                        m_ttsCmd_System.SelectVoice(m_pstrLang_Val[(int)ELan]);

                        m_ttsCmd_System.SetOutputToDefaultAudioDevice();
                        m_ttsCmd_System.Volume = nVolum_100;
                    }
                    else
                    {
                        m_ttsCmd = new Microsoft.Speech.Synthesis.SpeechSynthesizer();

                        m_ttsCmd.SelectVoice(m_pstrLang_Val[(int)ELan]);

                        m_ttsCmd.SetOutputToDefaultAudioDevice();
                        m_ttsCmd.Volume = nVolum_100;
                    }
                }
                catch (Exception ex)
                {
                    String strText = "Init() Error : " + ex.ToString();
                    CMessage.Write_Error(strText);
                }
            }
            public void Tts(string strData)
            {
                if (m_ELang == ELanguage_t._CHINA)
                {
                    m_ttsCmd_System.Speak(strData);
                }
                else
                {
                    m_ttsCmd.Speak(strData);
                }
                CMessage.Write(strData);
            }
        }
        public class CVoice
        {
            public enum EMode_t
            {
                // Cmd : No Keyword(Command Only)
                // Mix : [Keyword] & Command(Mix)
                // Keyword: Keyword & Command(No Command Only)
                _COMMAND = 0,
                _MIX,
                _KEYWORD
            }
            public enum ELanguage_t
            {
                _KOREAN,
                _ENGLISH_US,
                _CHINA
            }
            //public struct SVoice_t
            //{
            //    public int nIndex;
            //    public int nReturnValue;
            //    public string strCommand;
            //}
            public static   readonly Encoding[] pEncodings = new Encoding[] { Encoding.GetEncoding("utf-8"), Encoding.GetEncoding("utf-8"), Encoding.GetEncoding("euc-cn") };
            private         readonly string[] m_pstrEncoding = new string[] { "utf-8", "utf-8", "euc-cn" };
            private         readonly string[] m_pstrLang_Val = new string[] { "ko-KR", "en-US", "zh-CN" };
            
            public static   readonly string[] m_pstrLang_Cmt = new string[] { "Korean", "English(US)", "China" };

            public static   readonly string[] m_pstrMode_Cmt = new string[] { 
                "Cmd : No Keyword(Command Only)", 
                "Mix : [Keyword] & Command(Mix)", 
                "Keyword: Keyword & Command(No Command Only)"
            };
            public static bool IsVoice()
            {
                // C:\Program Files\Microsoft SDKs\Speech\v11.0\Assembly\Microsoft.Speech.dll
                if (CFile.IsFile(String.Format(@"{0}\Microsoft SDKs\Speech\v11.0\Assembly\Microsoft.Speech.dll", CSystem.GetPath_ProgramFiles())) == true)
                    return true;
                return false;
            }
    #region 
            
            private int m_nRecog = 0; // 0 : 키워드, 1 : command
            private string m_strKeywork = "Wall-e";
            //private int m_nCnt_Command = 0;
            //private List<int> m_lstVoice_Index = new List<int>();
            private List<String> m_lstVoice_Result = new List<String>();
            private List<String> m_lstVoice_Str = new List<String>();
            #endregion
    #region Voice
            private bool m_bInit = false;
            private int m_nReturnValue = 0;
            private int m_nResultType = -1;
            private string m_strReturnValue = String.Empty;
            public int nResult { get { return m_nReturnValue; } }
            // -1 : No Result, 0 : Int, 1 : string
            public int nResultType { get { return m_nResultType; } }
            public string strResult { get { return m_strReturnValue; } }
            private int m_nCnt = 0;
            private int m_nMode = -1;
            private bool m_bSetting = false;
            private CTimer m_CTmrRecog = new CTimer();
            //private CFile m_CFile_Voice = new CFile();
            private SpeechRecognitionEngine m_sRecognize;
            private string m_strLanguage = m_pstrLang_Cmt[1];
            //private bool m_bCmd = false;
            public void Init(ELanguage_t ELang, EMode_t EMode) // 0 - Korean, 1 - English(US), 2 - China
            {
                //m_bCmd = true;
                int nLang = (int)ELang;
                m_nMode = (int)EMode;
                Init(m_pstrLang_Val[nLang], m_nMode);
            }
            // Language : 0 - Korean, 1 - English(US), 2 - China
            // Keyword  : 0 - No Keyword, 1 - Mix, 2 - Keyword
            public void Init(int nLanguage, int nKeywordMode) 
            {
                //m_bCmd = true;
                int nLang = ((nLanguage < 0) ? 0 : ((nLanguage >= m_pstrLang_Cmt.Length) ? m_pstrLang_Cmt.Length : nLanguage));
                m_nMode = nKeywordMode;
                Init(m_pstrLang_Val[nLang], m_nMode);
            }
            private CUserEvent Event_VoiceRecog = new CUserEvent();
            public void Init(String strLanguage, int nMode)//, List<string> lst)
            {
                //if (m_bCmd == false) m_nMode = -1;
                //m_bCmd = false;

                if (m_bSetting == true) Stop();

                //m_lstVoice_Index.Clear();
                m_lstVoice_Result.Clear();
                m_lstVoice_Str.Clear();

                //SpeechSynthesizer sSynth = new SpeechSynthesizer();
                //PromptBuilder pBuilder = new PromptBuilder();
                m_sRecognize = new SpeechRecognitionEngine(new System.Globalization.CultureInfo(strLanguage));
                m_strLanguage = strLanguage;
                try
                {
                    if (m_bInit == true)
                    {
                        m_sRecognize.SpeechRecognized -= new EventHandler<SpeechRecognizedEventArgs>(Speech_Recognized);
                        Event_VoiceRecog.ClearEvent();
                    }

                    m_sRecognize.RequestRecognizerUpdate();
                    m_sRecognize.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Speech_Recognized);
                    Event_VoiceRecog.UserEvent += new EventHandler(Speech_Result);
                    m_sRecognize.SetInputToDefaultAudioDevice();
                    m_sRecognize.RecognizeAsync(RecognizeMode.Multiple);
                    CMessage.Write("FInitRecog() - {0}", strLanguage);
                }
                catch(Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.ToString());
                }
                m_bInit = true;
            }
            public CUserEvent Event() { return Event_VoiceRecog; }
            public void SetKeyword(string strKeyword)
            {
                m_strKeywork = strKeyword;
            }
            public void SetKeywordMode(int nMode)
            {
                m_nMode = nMode;
            }
            private void Speech_Result(object sender, EventArgs e)
            {
                CMessage.Write("Result => {0},{1}", m_nReturnValue, m_strReturnValue);
            }
    #region Unload - 이건 FloadGrammars 내에서만 사용한다.
            //public void LoadFile(string strFile)
            //{
            //    if (m_nMode >= 0)
            //    {
            //        m_CFile_Voice.EncodingType(Encoding.GetEncoding(m_pstrEncoding[m_nMode]));
            //        m_CFile_Voice.Load(strFile);
            //    }
            //    else
            //    {
            //        m_CFile_Voice.Load(strFile);
            //    }
            //    //Load(m_CFile_Voice.
            //}
            //public string GetData(int nIndex) { return m_CFile_Voice.Get(nIndex); }
            //public string[] GetData() { return m_CFile_Voice.Get(); }
            //public void Set(int nIndex, string strValue) { m_CFile_Voice.Set(nIndex, strValue); }
            //public void SaveFile(string strFile)
            //{
            //    m_CFile_Voice.Save(strFile);
            //}
            public void Stop()
            {
                m_sRecognize.RecognizeAsyncStop();
                m_sRecognize.UnloadAllGrammars();
                m_bSetting = false;
            }
            #endregion Unload - 이건 FloadGrammars 내에서만 사용한다.
            public void Add(params string[] pstrVoice)
            {
                //int i = 0;
                foreach (string strItem in pstrVoice)
                {
                    int nIndex = strItem.IndexOf(';');
                    if (nIndex >= 0)
                    {
                        //if (strItem.IndexOf('\"') >= 0)
                        //{
                        //}
                        m_lstVoice_Result.Add(strItem.Substring(0, nIndex));
                        m_lstVoice_Str.Add(strItem.Substring(nIndex + 1));
                    }
                    else
                    {
                        //m_lstVoice_Index.Add
                        //m_lstVoice_Result.Add(CConvert.IntToStr(i++));
                        m_lstVoice_Result.Add(CConvert.IntToStr(m_lstVoice_Str.Count));
                        m_lstVoice_Str.Add(strItem);
                    }
                }
            }
            public void Set_Result(int nIndex, string strNewReturnValue)
            {
                if ((nIndex >= 0) && (nIndex < m_lstVoice_Result.Count))
                {
                    m_lstVoice_Result[nIndex] = strNewReturnValue;
                }
            }
            public void Set_Command(int nIndex, string strNewCommand)
            {
                if ((nIndex >= 0) && (nIndex < m_lstVoice_Result.Count))
                {
                    m_lstVoice_Result[nIndex] = strNewCommand;
                }
            }
            public void Clear()
            {
                //m_nCnt_Command = 0;
                //m_lstVoice_Index.Clear();
                m_lstVoice_Result.Clear();
                m_lstVoice_Str.Clear();
            }
            private List<string> m_lstKeyword = new List<string>();
            public void Start()//(int nPage)
            {
                try
                {
                    List<string> lstVoice = new List<string>();
                    lstVoice.Clear();

                    string[] pstrKeyword = m_strKeywork.Split(';');
                    m_lstKeyword.Clear();
                    bool bList = false;
                    foreach (string strKeyword in pstrKeyword)
                    {
                        m_lstKeyword.Add(strKeyword);
                        //if (nPage < 0) nPage = 0;
                        int nMode = m_nMode;

                        //m_nPageCurr = nPage;

                        //txtList.Clear();
                        bool bKeyword = false;
                        if (nMode == 1) // Mix
                        {
                            if (strKeyword.Length > 0)
                            {
                                lstVoice.Add(strKeyword.ToLower());
                                bKeyword = true;
                                CMessage.Write2(string.Format("[Keyword:Mix Mode] => {0}", strKeyword));
                                CMessage.Write2("\r\n");
                            }
                            else
                            {
                                CMessage.Write2(string.Format("[Keyword:Mix Mode] => None"));
                                CMessage.Write2("\r\n");
                            }
                        }
                        else
                        {
                            if (nMode == 2)
                            {
                                CMessage.Write2("[Keyword Mode(Only way - Keyword using]");
                                bKeyword = true;
                            }
                            else // if (nMode == 0)
                            {
                                CMessage.Write2("[Cmd Mode(No Keyword)]");
                            }

                            //CMessage.Write2(txtList, ((nMode == 0) ? "[Cmd Mode(No Keyword)]" : "[Keyword Mode(Only way - Keyword using]"));
                            CMessage.Write2("\r\n");
                        }
                        //if ((m_pCParam_Voice.Length >= nPage) && (nPage >= 0))
                        //{
                        for (int i = 0; i < m_lstVoice_Str.Count; i++)
                        {
                            if (i < m_lstVoice_Str.Count)
                            {
                                if (m_lstVoice_Str[i].Length > 0)
                                {
                                    // Keyword & Command
                                    if (bKeyword == true) lstVoice.Add(String.Format("{0} {1}", strKeyword.ToLower(), m_lstVoice_Str[i].ToLower()));

                                    // Command - Keyword 만 사용하는 모드가 아니라면 일반 명령어 삽입
                                    if (nMode != 2) lstVoice.Add(m_lstVoice_Str[i].ToLower());

                                    CMessage.Write2(m_lstVoice_Str[i]);
                                    CMessage.Write2("\r\n");
                                    bList = true;
                                }
                            }
                        }
                        //}
                        
                    }
                    if (bList == true)
                    {
                        int nRetrieve = 3;
                        for (int i = 0; i < nRetrieve; i++)
                        {
                            if (Setting(lstVoice.ToArray()) == true)
                            {
                                break;
                            }
                            CMessage.Write_Error("Engine Retrieve Count = {0}", i + 1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CMessage.Write_Error(ex.ToString());//String.Format("nPage={0}, m_nMode = {1}, Msg={2}", ex.ToString()));
                }
            }
            public bool Setting(params string[] pstrList)
            {
                try
                {
                    if (pstrList.Length > 0)
                    {
                        Stop();
                        Choices sList = new Choices();
                        sList.Add(pstrList);//(strList);
                        GrammarBuilder gb = new GrammarBuilder(sList);
                        gb.Culture = new System.Globalization.CultureInfo(m_strLanguage);// ("en-US");
                        Grammar gr = new Grammar(gb);
                        //m_sRecognize.RequestRecognizerUpdate();
                        m_sRecognize.LoadGrammarAsync(gr);
                        //m_sRecognize.SetInputToDefaultAudioDevice();
                        m_sRecognize.RecognizeAsync(RecognizeMode.Multiple); //(RecognizeMode.Single); //(RecognizeMode.Multiple);

                        m_bSetting = true;
                        return true;
                    }
                    return false;
                }
                catch //(Exception ex)
                {
                    return false;
                }
            }
            private void Speech_Recognized(object sender, SpeechRecognizedEventArgs e)
            {
                string strSpeech = String.Empty;
                string strRecog = String.Empty;
                try
                {
                    strSpeech = e.Result.Text;
                    if (m_nRecog != 0)
                    {
                        if (m_CTmrRecog.Get() > 10000)
                        {
                            strRecog = "";
                            m_nRecog = 0;
                        }
                    }

                    //if (m_strKeywork == null) m_nRecog = 2;
                    //else if (m_strKeywork.Length == 0) m_nRecog = 2;
                    if (m_lstKeyword.Count == 0) m_nRecog = 2;
                    
                    int nList = -1;
                    if (m_nRecog == 0)
                    {
                        //int nKeyIndex = -1;
                        bool bKeyword = false;
                        string strKey = null;
#if true
                        if (m_strKeywork.IndexOf(';') >= 0)
                        {
                            string[] pstrKeyword = m_strKeywork.Split(';');
                            foreach (string strKeyword in pstrKeyword)
                            {
                                if (e.Result.Text.IndexOf('!') < 0) // 키워드에는 ! 를 넣으면 전부 부정
                                {
                                    if (e.Result.Text.IndexOf(strKeyword) == 0)
                                    {
                                        strKey = strKeyword;
                                    }
                                    if (strKeyword == e.Result.Text)
                                    {
                                        bKeyword = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (e.Result.Text == m_strKeywork)
                        {
                            if (e.Result.Text.IndexOf('!') < 0) // 키워드에는 ! 를 넣으면 전부 부정
                            {
                                bKeyword = true;
                                if (e.Result.Text.IndexOf(m_strKeywork) == 0)
                                {
                                    strKey = m_strKeywork;
                                }
                            }
                        }
                        else
                        {
                            if (e.Result.Text.IndexOf('!') < 0) // 키워드에는 ! 를 넣으면 전부 부정
                            {
                                if (e.Result.Text.IndexOf(m_strKeywork) == 0)
                                {
                                    strKey = m_strKeywork;
                                }
                            }
                        }
#else
                        foreach (string strKeyword in m_lstKeyword)
                        {
                            if ((strKeyword.IndexOf('!') >= 0) && (strKeyword == e.Result.Text))
                            {
                                bKeyword = true;
                                strRecog = "";
                                m_nRecog = 0;
                                //Ojw.CMessage.Write("No Result 
                                //strRecog = "";
                                //m_nRecog = 0;
                                m_strReturnValue = null;// "";
                                m_nResultType = -1;
                                CMessage.Write("No result=[{0}] {1}", strSpeech, strRecog);
                                break;
                            }
                            else if (strKeyword == e.Result.Text)
                            {
                                bKeyword = true;
                                break;
                            }
                            nKeyIndex++;
                        }
#endif
                        //if ()
                        {
                            if (bKeyword == true)//(e.Result.Text == m_strKeywork)
                            {
                                m_nRecog = 1;
                                m_CTmrRecog.Set();

                                strRecog = String.Format("Checking {0}...", m_nCnt++);
                            }
                            else// if (e.Result.Text.IndexOf(m_strKeywork) >= 0)
                            {
                                int nOffset = 0;
                                string strKey2 = "";
                                if (strKey != null) 
                                {
                                    strKey2 = String.Format("{0} ", strKey);
                                    nOffset = strKey.Length + 2;
                                }
                                //for (int i = 1; i < m_lstVoice.Count; i += 2)
                                for (int i = 0; i < m_lstVoice_Str.Count; i++)
                                {
                                    //if (e.Result.Text.ToLower().IndexOf(m_lstVoice_Str[i].ToLower()) >= 0)
                                    //if (e.Result.Text.ToLower().IndexOf(m_lstVoice_Str[i].ToLower()) == nOffset)
                                    if (String.Format("{0}{1}", strKey2, m_lstVoice_Str[i].ToLower()) == e.Result.Text.ToLower())
                                    //if (e.Result.Text.ToLower().Substring(m_lstKeyword[nKeyIndex] == m_lstVoice_Str[i].ToLower())
                                    {
                                        if (m_nMode == (int)EMode_t._MIX) nList = i;// / 2;
                                        else nList = i;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (m_nRecog != 0)//== 1)
                    {
                        //for (int i = 1; i < m_lstVoice.Count; i += 2)
                        for (int i = 0; i < m_lstVoice_Str.Count; i++)
                        {
                            if (e.Result.Text.ToLower() == m_lstVoice_Str[i].ToLower())
                            {
                                //nList = i;
                                if (m_nMode == (int)EMode_t._MIX) nList = i;// / 2;
                                else nList = i;
                                break;
                            }
                        }

                        m_nRecog = 0;

                        strRecog = "Done";
                    }

                    if (nList >= 0)
                    {
                        int nReturn = -1;
                        string strTmp = m_lstVoice_Result[nList];
                        
                        //if (strTmp[0] == (char)(0x02))
                        //{
                        int nFind = strTmp.IndexOf('!');
                        int nFindKey = strSpeech.IndexOf('!');
                        if (((nFind >= 0) && (nFind <= 1)) || (nFindKey == 0))
                        {
                            strRecog = "";
                            m_nRecog = 0;
                            m_strReturnValue = null;// "";
                            m_nResultType = -1;
                            CMessage.Write("No result=[{0}] {1}", strSpeech, strRecog);
                        }
                        else if (strTmp[0] == (char)('\"'))//.IndexOf('\"') >= 0)
                        {
                            m_nReturnValue = -1;
                            m_nResultType = 1;
                            if ((nFind >= 0) && (nFind <= 1))
                            {
                                strRecog = "";
                                m_nRecog = 0;
                                m_strReturnValue = null;// "";
                                m_nResultType = -1;
                                CMessage.Write("No result=[{0}] {1}", strSpeech, strRecog);
                            }
                            else
                                m_strReturnValue = strTmp.Substring(1, strTmp.Length - 2);
                        }
                        else
                        {
                            if ((strTmp.IndexOf('+') >= 0) || (strTmp.IndexOf('-') >= 0))
                            {
                                nReturn = m_nReturnValue + CConvert.StrToInt(strTmp);
                            }
                            else
                            {
                                nReturn = CConvert.StrToInt(strTmp);
                            }
                            //CMessage.Write("Goto => {0}", nReturn);
                            m_nReturnValue = nReturn;
                            m_strReturnValue = CConvert.IntToStr(nReturn);
                            m_nResultType = 0;
                        }

                        // Event 발생
                        if (m_strReturnValue != null)
                            Event_VoiceRecog.RunEvent();
                    }
                }
                catch (Exception ex)
                {
                    strRecog = String.Format("Fail[{0}]", ex.ToString());
                }
                CMessage.Write("[{0}] {1}", strSpeech, strRecog);
            }
    #endregion Voice
        }
        
#if false
        public class CVoice
        {
            public CVoice()
            {
                string strPath = Application.StartupPath;

                m_strWork = Application.StartupPath;
                //cmbLanguage.Items.Clear();
                //for (int i = 0; i < m_pstrLanguage_List.Length; i++)
                //{
                //    cmbLanguage.Items.Add(m_pstrLanguage_List[i]);
                //}
#if false
                tbVoice.TabPages.Clear();
                for (int i = 0; i < 20; i++)
                {
                    tbVoice.TabPages.Add(String.Format("tpVoice_{0}", CConvert.IntToStr(i, 3)));
                    TextBox txt = new TextBox();
                    txt.Name = String.Format("txtVoice_{0}", CConvert.IntToStr(i, 3));
                    txt.Multiline = true;
                    txt.Width = tbVoice.TabPages[i].Width - 8;
                    txt.Height = tbVoice.TabPages[i].Height - 8;
                    txt.Left = 4;
                    txt.Top = 4;
                    tbVoice.TabPages[i].Controls.Add(txt);
                }

    #region Param
                m_CParam = new CParam(
                    Encoding.GetEncoding("euc-cn"),//51949), // 중국어 간체
                    string.Format("{0}\\{1}", strPath, "param_voice.dat"),// "param.dat",
                    txtKeyword,
                    cmbLanguage,
                    cmbMode
                    );
                #endregion Param

    #region Param_Voice
                int nCount_List = 100;
                m_nCnt_Page_Voice = tbVoice.TabPages.Count;
                m_pCParam_Voice = new CParam[m_nCnt_Page_Voice];
                for (int i = 0; i < m_pCParam_Voice.Length; i++)
                {
                    m_pCParam_Voice[i] = new CParam(
                        Encoding.GetEncoding("euc-cn"),////Encoding.GetEncoding(51949), // 중국어 간체
                        string.Format("{0}\\{1}", strPath, string.Format("{0}_{1}", m_strPage_Voice, i)), nCount_List
                        );
                }
                //m_CParam_Voice = new CParam("voice.dat", 1000);

                #endregion Param_Voice
                ParamLoad();

                FInitRecog(m_pstrLanguage[m_nLanguage]);


                SetPage(0);

                m_nSeq = g_CShm_Main.SData.nVoice_Seq;
                tmrCheck.Enabled = true;
            }
            ~CVoice()
            {
                FUnloadGrammars();
            }
            private string[] m_pstrLanguage = new string[] { "ko-KR", "en-US", "zh-CN" };
            private string[] m_pstrLanguage_List = new string[] { "Korean", "English(US)", "China" };
            private CParam m_CParam;
            private int m_nCnt_Page_Voice = 0;
            private CParam[] m_pCParam_Voice;
            private const string m_strPage_Voice = "voice";
            private string m_strWork = String.Empty;
            private int m_nLanguage = 0; // 0 - Korean, 1 - English, 2 - China
            /*
                Cmd : No Keyword(Command Only)
                Mix : [Keyword] & Command(Mix)
                Keyword: Keyword & Command(No Command Only)
            */
            private int m_nMode = 0;
#else
            private string[] m_pstrLanguage = new string[] { "ko-KR", "en-US", "zh-CN" };
            private string[] m_pstrLanguage_List = new string[] { "Korean", "English(US)", "China" };
            private int m_nLanguage = 0; // 0 - Korean, 1 - English
            private bool m_bSetting = false;
            
            private SpeechRecognitionEngine m_sRecognize;
            private string m_strLanguage = "ko-KR";

            private int m_nMode = 0;

            private void FInitRecog(String strLanguage)//, List<string> lst)
            {
                SpeechSynthesizer sSynth = new SpeechSynthesizer();
                PromptBuilder pBuilder = new PromptBuilder();
                m_sRecognize = new SpeechRecognitionEngine(new System.Globalization.CultureInfo(strLanguage));
                m_strLanguage = strLanguage;
                try
                {
                    m_sRecognize.RequestRecognizerUpdate();
                    m_sRecognize.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Speech_Recognized);
                    m_sRecognize.SetInputToDefaultAudioDevice();
                    m_sRecognize.RecognizeAsync(RecognizeMode.Multiple);
                    CMessage.Write("FInitRecog() - {0}", strLanguage);
                }
                catch
                {
                }
            }
    #region Unload - 이건 FloadGrammars 내에서만 사용한다.
            private void FUnloadGrammars()
            {
                m_sRecognize.RecognizeAsyncStop();
                m_sRecognize.UnloadAllGrammars();
                m_bSetting = false;
            }
            #endregion Unload - 이건 FloadGrammars 내에서만 사용한다.
            private bool FloadGrammars(params string[] strList)
            {
                try
                {
                    FUnloadGrammars();
                    Choices sList = new Choices();
                    sList.Add(strList);
                    GrammarBuilder gb = new GrammarBuilder(sList);
                    gb.Culture = new System.Globalization.CultureInfo(m_strLanguage);// ("en-US");
                    Grammar gr = new Grammar(gb);
                    //m_sRecognize.RequestRecognizerUpdate();
                    m_sRecognize.LoadGrammarAsync(gr);
                    //m_sRecognize.SetInputToDefaultAudioDevice();
                    m_sRecognize.RecognizeAsync(RecognizeMode.Multiple); //(RecognizeMode.Single); //(RecognizeMode.Multiple);

                    m_bSetting = true;
                    return true;
                }
                catch //(Exception ex)
                {
                    return false;
                }
            }

            private int m_nRecog = 0; // 0 : 키워드, 1 : command
            private CTimer m_CTmrRecog = new CTimer();
            private string m_strKeywork = "월이야";
            private int m_nPageCurr = 0;
            //public void SetPage(int nPage) { m_nPageCurr = nPage; }

            public enum EMode_t
            {
                // Cmd : No Keyword(Command Only)
                // Mix : [Keyword] & Command(Mix)
                // Keyword: Keyword & Command(No Command Only)
                _COMMAND = 0,
                _MIX,
                _KEYWORD
            }

            private void SetPage(int nPage)
            {
                try
                {
                    if (nPage < 0) nPage = 0;
                    int nMode = m_nMode;
                    m_nPageCurr = nPage;

                    List<string> lstVoice = new List<string>();
                    lstVoice.Clear();
                    //txtList.Clear();
                    bool bKeyword = false;
                    if (nMode == 1) // Mix
                    {
                        if (m_strKeywork.Length > 0)
                        {
                            lstVoice.Add(m_strKeywork.ToLower());
                            bKeyword = true;
                            CMessage.Write2(string.Format("[Keyword:Mix Mode] => {0}", m_strKeywork));
                            CMessage.Write2("\r\n");
                        }
                        else
                        {
                            CMessage.Write2(string.Format("[Keyword:Mix Mode] => None"));
                            CMessage.Write2("\r\n");
                        }
                    }
                    else
                    {
                        if (nMode == 2)
                        {
                            CMessage.Write2("[Keyword Mode(Only way - Keyword using]");
                            bKeyword = true;
                        }
                        else // if (nMode == 0)
                        {
                            CMessage.Write2("[Cmd Mode(No Keyword)]");
                        }

                        //CMessage.Write2(txtList, ((nMode == 0) ? "[Cmd Mode(No Keyword)]" : "[Keyword Mode(Only way - Keyword using]"));
                        CMessage.Write2("\r\n");
                    }
                    bool bList = false;
                    if ((m_pCParam_Voice.Length >= nPage) && (nPage >= 0))
                    {
                        for (int i = 1; i < m_pCParam_Voice[nPage].m_nCount; i += 2)
                        {
                            if (m_pCParam_Voice[nPage].m_nCount > i)
                            {
                                if (m_pCParam_Voice[nPage].Get(i).Length > 0)
                                {
                                    // Keyword & Command
                                    if (bKeyword == true) lstVoice.Add(String.Format("{0} {1}", m_strKeywork.ToLower(), m_pCParam_Voice[nPage].Get(i).ToLower()));

                                    // Command - Keyword 만 사용하는 모드가 아니라면 일반 명령어 삽입
                                    if (nMode != 2) lstVoice.Add(m_pCParam_Voice[nPage].Get(i).ToLower());

                                    CMessage.Write2(m_pCParam_Voice[nPage].Get(i));
                                    CMessage.Write2("\r\n");
                                    bList = true;
                                }
                            }
                        }
                    }
                    if (bList == true)
                    {
                        int nRetrieve = 3;
                        for (int i = 0; i < nRetrieve; i++)
                        {
                            if (FloadGrammars(lstVoice.ToArray<string>()) == true)
                            {
                                break;
                            }
                            CMessage.Write_Error("Engine Retrieve Count = {0}", i + 1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CMessage.Write_Error(String.Format("nPage={0}, m_nMode = {1}, Msg={2}", ex.ToString()));
                }
            }
            private void Speech_Recognized(object sender, SpeechRecognizedEventArgs e)
            {
                int nPageCurr = m_nPageCurr;
                string strSpeech = String.Empty;
                string strRecog = String.Empty;
                try
                {
                    strSpeech = e.Result.Text;
                    if (m_nRecog != 0)
                    {
                        if (m_CTmrRecog.Get() > 10000)
                        {
                            strRecog = "";
                            m_nRecog = 0;
                        }
                    }

                    if (m_strKeywork.Length == 0)
                    {
                        m_nRecog = 2;
                    }

                    int nList = -1;
                    if (m_nRecog == 0)
                    {
                        if (e.Result.Text == m_strKeywork)
                        {
                            m_nRecog = 1;
                            m_CTmrRecog.Set();

                            strRecog = String.Format("인식 중{0}...", m_nCnt++);
                        }
                        else if (e.Result.Text.IndexOf(m_strKeywork) >= 0)
                        {
                            for (int i = 1; i < m_pCParam_Voice[nPageCurr].m_nCount; i += 2)
                            {
                                if (e.Result.Text.ToLower().IndexOf(m_pCParam_Voice[nPageCurr].Get(i).ToLower()) > 0)
                                {
                                    nList = i;
                                    break;
                                }
                            }
                        }
                    }
                    else if (m_nRecog != 0)//== 1)
                    {
                        for (int i = 1; i < m_pCParam_Voice[nPageCurr].m_nCount; i += 2)
                        {
                            if (e.Result.Text.ToLower() == m_pCParam_Voice[nPageCurr].Get(i).ToLower())
                            {
                                nList = i;
                                break;
                            }
                        }

                        m_nRecog = 0;

                        strRecog = "인식 완료";
                    }

                    if (nList > 0)
                    {
                        int nPage = -1;
                        string strTmp = m_pCParam_Voice[nPageCurr].Get(nList - 1);
                        if ((strTmp.IndexOf('+') >= 0) || (strTmp.IndexOf('-') >= 0))
                        {
                            nPage = nPageCurr + CConvert.StrToInt(strTmp);
                        }
                        else
                        {
                            nPage = CConvert.StrToInt(strTmp);
                        }
                        
                        CMessage.Write("Goto => {0}", nPage);
                    }
                }
                catch (Exception ex)
                {
                    strRecog = String.Format("인식실패[{0}]", ex.ToString());
                }
                CMessage.Write("[{0}] {1}", strSpeech, strRecog);
            }
#endif
        }
#endif
    }
#endif
}
