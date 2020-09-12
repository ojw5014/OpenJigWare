#define _REMOVE_VOICE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenJigWare.Docking
{
    public partial class frmMsVoice : Form
    {
        public frmMsVoice()
        {
            InitializeComponent();
        }

        private Ojw.CParam m_CParam;
#if !_REMOVE_VOICE
        private Ojw.CVoice m_CVoice = new Ojw.CVoice();
#endif
        private void frmMsVoice_Load(object sender, EventArgs e)
        {
            Ojw.CMessage.Init(txtMessage);
#if !_REMOVE_VOICE
            cmbLanguage.Items.Clear(); for (int i = 0; i < Ojw.CVoice.m_pstrLang_Cmt.Length; i++) { cmbLanguage.Items.Add(Ojw.CVoice.m_pstrLang_Cmt[i]); }
            cmbMode.Items.Clear(); for (int i = 0; i < Ojw.CVoice.m_pstrMode_Cmt.Length; i++) { cmbMode.Items.Add(Ojw.CVoice.m_pstrMode_Cmt[i]); }
#endif            

            m_CParam = new Ojw.CParam(
                    //Ojw.CVoice.pEncodings[nLang],
                    "Param_Voice.dat",
                    cmbMode,
                    cmbLanguage,
                    txtKeyword,
                    txtVoice
                );
            if (cmbLanguage.SelectedIndex < 0) cmbLanguage.SelectedIndex = 0;
            if (cmbMode.SelectedIndex < 0) cmbMode.SelectedIndex = 0;
#if !_REMOVE_VOICE
            int nLang = cmbLanguage.SelectedIndex;
            if (nLang < 0) nLang = 0;
            m_CParam.SetEncoding(Ojw.CVoice.pEncodings[nLang]);
            if (nLang == (int)Ojw.CVoice.ELanguage_t._CHINA)
            {
                m_CParam = new Ojw.CParam(
                    Ojw.CVoice.pEncodings[nLang],
                    "Param_Voice.dat",
                    cmbMode,
                    cmbLanguage,
                    txtKeyword,
                    txtVoice
                );
            }

            m_CVoice.Init(cmbLanguage.SelectedIndex, cmbMode.SelectedIndex);
            
            if (Ojw.CVoice.IsVoice() == false) Ojw.CMessage.Write_Error("Cannot find Microsoft.Speech.dll");
#endif
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
#if !_REMOVE_VOICE
            m_CVoice.Init(cmbLanguage.SelectedIndex, cmbMode.SelectedIndex);
            // 사용할 키워드 저장 ex) 월이야
            m_CVoice.SetKeyword(txtKeyword.Text);

            // 따로 실행 함수를 정의하고 싶을때
            m_CVoice.Event().UserEvent += new EventHandler(Speech_Result);

            // 사용할 리스트 작성
            foreach (string strItem in txtVoice.Lines)
            {
                m_CVoice.Add(strItem);
            }
            m_CVoice.Start();
#endif
        }
        private void Speech_Result(object sender, EventArgs e)
        {
#if !_REMOVE_VOICE
            if (m_CVoice.nResultType >= 0)
            {
                // Integer return
                if (m_CVoice.nResultType == 0)
                {
                    Ojw.CMessage.Write("음성인식 결과[Integer] => {0}", m_CVoice.nResult);
                }
                // string return
                else if (m_CVoice.nResultType == 1)
                {
                    Ojw.CMessage.Write("음성인식 결과[string] => {0}", m_CVoice.strResult);
                }
                // Unknown type
                else
                {
                    Ojw.CMessage.Write("음성인식 결과[Unknown type] => {0}", m_CVoice.nResult);
                }
            }            
#endif
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
#if !_REMOVE_VOICE
            m_CVoice.Stop();
#endif
        }

        private void frmMsVoice_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_CParam.Param_Save();
        }

        private void btnSpeaking_Click(object sender, EventArgs e)
        {
#if !_REMOVE_VOICE            
            Ojw.CTts CTts = new Ojw.CTts();
            //CTts.Init(Ojw.CTts.ELanguage_t._KOREAN);
            CTts.Init(cmbLanguage.SelectedIndex);
            CTts.Tts(txtTts.Text);
#endif
        }

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
#if !_REMOVE_VOICE
            int nLang = cmbLanguage.SelectedIndex;
            if (nLang < 0) 
                nLang = 0;
            m_CParam.SetEncoding(Ojw.CVoice.pEncodings[nLang]);
#endif
        }
    }
}
