//#define _USE_TEXTBOX_ONLY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace OpenJigWare
{
    partial class Ojw
    {
        // if you make your class, just write in here
#if _USE_TEXTBOX_ONLY
        public class CParam
        {
            private Ojw.CFile m_CFile = new Ojw.CFile();
            public string m_strFileName = "param.dat";
            public int m_nCount = 0;
            public TextBox[] m_atxt = new TextBox[100];
            public CParam()
            {
                Param_Load(m_strFileName, 100);
            }
            public CParam(int nParamCount)
            {
                Param_Load(m_strFileName, nParamCount);
            }
            public CParam(string strName)
            {
                Param_Load(strName, 100);
            }
            public CParam(string strName, int nParamCount)
            {
                Param_Load(strName, nParamCount);
            }
            public CParam(string strName, params TextBox[] txts)
            {
                Param_Load(strName, txts);
            }
            public CParam(params TextBox[] txts)
            {
                Param_Load(m_strFileName, txts);
            }
            ~CParam()
            {
                //Param_Save();
            }
            private List<TextBox> m_lstObject = new List<TextBox>();
            private void SetTextBoxAll(params TextBox[] objects) 
            {
                //if (m_bNoLoad == true)
                //{

                //}
                m_lstObject.Clear();
                for (int i = 0; i < objects.Length; i++)
                {
                    if (m_bNoLoad == true)
                    {
                        objects[i] = new TextBox();
                        objects[i].Text = m_CFile.GetData_String(i);
                    }
                    else
                    {
                    }
                    objects[i].TextChanged += new EventHandler(obj_TextChanged);
                    m_lstObject.Add(objects[i]); 
                }
                m_bNoLoad = false;
            }

            public bool IsValid(int nNum) { if ((nNum < 0) || (nNum >= m_lstObject.Count)) return false; return true; }
            public bool IsValid(TextBox txt) { if (GetObject(txt) < 0) return false; return true; }

            public void Set(TextBox txt, string strParam) { if (IsValid(txt)) { m_lstObject[GetObject(txt)].Text = strParam; } }
            public void Set(int nNum, string strParam) { if (IsValid(nNum)) { m_lstObject[nNum].Text = strParam; } }
            public String Get(int nNum) { return m_lstObject[nNum].Text; }

            public Control GetObject(int nNum) { if ((nNum < 0) || (nNum >= m_lstObject.Count)) return null; return m_lstObject[nNum]; }
            private int GetObject(Control ctrl)
            {
                int nNum = -1; for (int i = 0; i < m_lstObject.Count; i++) { if (ctrl == m_lstObject[i]) { nNum = i; break; } }
                return nNum;
            }

            private void obj_TextChanged(object sender, EventArgs e)
            {
                if (m_bLoading == false)
                {
                    int nNum = GetObject((System.Windows.Forms.TextBox)sender);
                    if (nNum >= 0) { m_CFile.SetData_String(nNum, m_lstObject[nNum].Text); }
                }
            }
            private bool m_bNoLoad = false;
            public void Param_Load(string strName, int nCount)
            {
                m_bNoLoad = true;
                m_atxt = new TextBox[nCount];
                Param_Load(strName, m_atxt);
            }
            private bool m_bLoading = false;

            public void Param_Load(string strName, params TextBox[] txts)
            {
                m_bLoading = true;
                m_nCount = txts.Length;
                m_strFileName = strName;
                int nCnt = m_CFile.Load(m_nCount, strName);

                if (m_lstObject.Count == 0) SetTextBoxAll(txts);
                if (nCnt <= 0)
                {
                    Ojw.CMessage.Write("[Warning] No parameter file Error : make new one[{0}]", String.Format("{0}.dat", strName));
                    Param_Save();
                }
                else
                {
                    for (int i = 0; i < m_lstObject.Count; i++)
                    {
                        m_lstObject[i].Text = m_CFile.GetData_String(i);
                    }
                    //int i = 0;
                    //g_frmPage3.txtIp.Text = m_CFile.GetData_String(i++);        // 0 : _PARAM_IP
                    //g_frmPage3.txtPort.Text = m_CFile.GetData_String(i++);      // 1 : _PARAM_PORT
                    Ojw.CMessage.Write("Loaded Param data[{0}]", m_lstObject.Count);
                }
                m_bLoading = false;
            }

            public void Param_Load() { Param_Load(m_strFileName, m_nCount); }
            public void Param_Save()
            {
                for (int i = 0; i < m_lstObject.Count; i++) { m_CFile.SetData_String(i, m_lstObject[i].Text); }
                m_CFile.Save(m_strFileName);
                Ojw.CMessage.Write("Loaded Param data[{0}]", m_nCount);
            }
        }
#else
        public class CParam
        {
            private Ojw.CFile m_CFile = new Ojw.CFile();
            public string m_strFileName = "param.dat";
            public int m_nCount = 0;
            //public TextBox[] m_atxt = new TextBox[100];
            public Control[] m_aCtrl = new Control[100];
            public CParam()
            {
                Param_Load(m_strFileName, 100);
            }
            public CParam(int nParamCount)
            {
                Param_Load(m_strFileName, nParamCount);
            }
            public CParam(string strName)
            {
                Param_Load(strName, 100);
            }
            public CParam(string strName, int nParamCount)
            {
                Param_Load(strName, nParamCount);
            }
            public CParam(string strName, params Control [] ctrl)
            {
                Param_Load(strName, ctrl);
            }
            public CParam(params Control [] ctrl)
            {
                Param_Load(m_strFileName, ctrl);
            }
            ~CParam()
            {
                //Param_Save();
            }
            private List<Control> m_lstObject = new List<Control>();
            private void SetControlsAll(params Control[] objects) 
            {
                //if (m_bNoLoad == true)
                //{

                //}
                m_lstObject.Clear();
                for (int i = 0; i < objects.Length; i++)
                {
                    if (m_bNoLoad == true)
                    {
                        if (objects[i] is TextBox)
                        {
                            objects[i] = new TextBox();
                            ((ComboBox)objects[i]).Text = m_CFile.GetData_String(i);
                            objects[i].TextChanged += new EventHandler(obj_TextChanged);
                        }
                        else if (objects[i] is ComboBox)
                        {
                            objects[i] = new ComboBox();
                            ((ComboBox)objects[i]).SelectedIndex = m_CFile.GetData_Int(i);
                            objects[i].TextChanged += new EventHandler(obj_SelectedIndexChanged);
                        }
                        else if (objects[i] is RadioButton)
                        {
                            objects[i] = new RadioButton();
                            ((RadioButton)objects[i]).Checked = m_CFile.GetData_Bool(i);
                            ((RadioButton)objects[i]).CheckedChanged += new EventHandler(obj_CheckedChanged);
                        }
                        else if (objects[i] is CheckBox)
                        {
                            objects[i] = new CheckBox();
                            ((CheckBox)objects[i]).Checked = m_CFile.GetData_Bool(i);
                            ((CheckBox)objects[i]).CheckedChanged += new EventHandler(obj_CheckedChanged);
                        }
                        else
                        {
                            objects[i] = new TextBox();
                            objects[i].Text = m_CFile.GetData_String(i);
                            objects[i].TextChanged += new EventHandler(obj_TextChanged);
                        }
                    }
                    else
                    {
                    }
                    m_lstObject.Add(objects[i]); 
                }
                m_bNoLoad = false;
            }
            public bool IsValid(int nNum) { if ((nNum < 0) || (nNum >= m_lstObject.Count)) return false; return true; }
            public bool IsValid(Control ctrl) { if (GetObject(ctrl) < 0) return false; return true; }

            public void Set(Control ctrl, string strParam) 
            { 
                if (IsValid(ctrl))
                {
                    int nNum = GetObject(ctrl);
                    if (m_lstObject[nNum] is TextBox)
                    {
                        m_lstObject[nNum].Text = strParam;
                    }
                    else if (m_lstObject[nNum] is ComboBox)
                    {
                        int nValue = Ojw.CConvert.StrToInt(strParam);
                        int nCount = ((ComboBox)m_lstObject[nNum]).Items.Count;
                        if (nValue < 0) nValue = 0;
                        else if (nValue >= nCount) nValue = nCount - 1;
                        ((ComboBox)m_lstObject[nNum]).SelectedIndex = nValue;
                    }
                    else if (m_lstObject[nNum] is RadioButton)
                    {
                        int nValue = ((strParam.ToLower() == "true") ? 1 : ((strParam.ToLower() == "false") ? 0 : Ojw.CConvert.StrToInt(strParam)));
                        ((RadioButton)m_lstObject[nNum]).Checked = ((nValue == 0) ? false : true);
                    }
                    else if (m_lstObject[nNum] is CheckBox)
                    {
                        int nValue = ((strParam.ToLower() == "true") ? 1 : ((strParam.ToLower() == "false") ? 0 : Ojw.CConvert.StrToInt(strParam)));
                        ((CheckBox)m_lstObject[nNum]).Checked = ((nValue == 0) ? false : true);
                    }
                    else
                    {
                        m_lstObject[nNum].Text = strParam;
                    }
                } 
            }
            public void Set(int nNum, string strParam) 
            { 
                if (IsValid(nNum)) 
                { 
                    if (m_lstObject[nNum] is TextBox)
                    {
                        m_lstObject[nNum].Text = strParam;
                    }
                    else if (m_lstObject[nNum] is ComboBox)
                    {
                        int nValue = Ojw.CConvert.StrToInt(strParam);
                        int nCount = ((ComboBox)m_lstObject[nNum]).Items.Count;
                        if (nValue < 0) nValue = 0;
                        else if (nValue >= nCount) nValue = nCount - 1;
                        ((ComboBox)m_lstObject[nNum]).SelectedIndex = nValue;
                    }
                    else if (m_lstObject[nNum] is RadioButton)
                    {
                        int nValue = ((strParam.ToLower() == "true") ? 1 : ((strParam.ToLower() == "false") ? 0 : Ojw.CConvert.StrToInt(strParam)));
                        ((RadioButton)m_lstObject[nNum]).Checked = ((nValue == 0) ? false : true);
                    }
                    else if (m_lstObject[nNum] is CheckBox)
                    {
                        int nValue = ((strParam.ToLower() == "true") ? 1 : ((strParam.ToLower() == "false") ? 0 : Ojw.CConvert.StrToInt(strParam)));
                        ((CheckBox)m_lstObject[nNum]).Checked = ((nValue == 0) ? false : true);
                    }
                    else
                    {
                        m_lstObject[nNum].Text = strParam;
                    }
                }
            }
            public void Set(int nNum, int nValue) 
            {
                if (IsValid(nNum)) 
                {
                    if (m_lstObject[nNum] is TextBox)
                    {
                        m_lstObject[nNum].Text = CConvert.IntToStr(nValue);
                    }
                    else if (m_lstObject[nNum] is ComboBox)
                    {
                        int nCount = ((ComboBox)m_lstObject[nNum]).Items.Count;
                        if (nValue < 0) nValue = 0;
                        else if (nValue >= nCount) nValue = nCount - 1;
                        ((ComboBox)m_lstObject[nNum]).SelectedIndex = nValue;
                    }
                    else if (m_lstObject[nNum] is RadioButton)
                    {
                        ((RadioButton)m_lstObject[nNum]).Checked = ((nValue == 0) ? false : true);
                    }
                    else if (m_lstObject[nNum] is CheckBox)
                    {
                        ((CheckBox)m_lstObject[nNum]).Checked = ((nValue == 0) ? false : true);
                    }
                    else
                    {
                        m_lstObject[nNum].Text = CConvert.IntToStr(nValue);
                    }
                }
            }
            public String Get(int nNum)
            {
                if (m_lstObject[nNum] is TextBox)
                {
                    return m_lstObject[nNum].Text;
                }
                else if (m_lstObject[nNum] is ComboBox)
                {
                    return CConvert.IntToStr(((ComboBox)m_lstObject[nNum]).SelectedIndex);
                }
                else if (m_lstObject[nNum] is RadioButton)
                {
                    return ((((RadioButton)m_lstObject[nNum]).Checked == false) ? "0" : "1");
                }
                else if (m_lstObject[nNum] is CheckBox)
                {
                    return ((((CheckBox)m_lstObject[nNum]).Checked == false) ? "0" : "1");
                }
                return m_lstObject[nNum].Text; 
            }
            public int Get_Int(int nNum) 
            {
                if (m_lstObject[nNum] is TextBox)
                {
                    return CConvert.StrToInt(m_lstObject[nNum].Text);
                }
                else if (m_lstObject[nNum] is ComboBox)
                {
                    return ((ComboBox)m_lstObject[nNum]).SelectedIndex;
                }
                else if (m_lstObject[nNum] is RadioButton)
                {
                    return ((((RadioButton)m_lstObject[nNum]).Checked == false) ? 0 : 1);
                }
                else if (m_lstObject[nNum] is CheckBox)
                {
                    return ((((CheckBox)m_lstObject[nNum]).Checked == false) ? 0 : 1);
                }
                return CConvert.StrToInt(m_lstObject[nNum].Text);
            }
            public float Get_Float(int nNum)
            {
                if (m_lstObject[nNum] is TextBox)
                {
                    return CConvert.StrToFloat(m_lstObject[nNum].Text);
                }
                else if (m_lstObject[nNum] is ComboBox)
                {
                    return (float)(((ComboBox)m_lstObject[nNum]).SelectedIndex);
                }
                else if (m_lstObject[nNum] is RadioButton)
                {
                    return ((((RadioButton)m_lstObject[nNum]).Checked == false) ? 0.0f : 1.0f);
                }
                else if (m_lstObject[nNum] is CheckBox)
                {
                    return ((((CheckBox)m_lstObject[nNum]).Checked == false) ? 0.0f : 1.0f);
                }
                return CConvert.StrToFloat(m_lstObject[nNum].Text);
            }
            public bool Get_Bool(int nNum)
            {
                if (m_lstObject[nNum] is TextBox)
                {
                    return CConvert.StrToBool(m_lstObject[nNum].Text);
                }
                else if (m_lstObject[nNum] is ComboBox)
                {
                    return ((((ComboBox)m_lstObject[nNum]).SelectedIndex == 0) ? false : true);
                }
                else if (m_lstObject[nNum] is RadioButton)
                {
                    return ((RadioButton)m_lstObject[nNum]).Checked;
                }
                else if (m_lstObject[nNum] is CheckBox)
                {
                    return ((CheckBox)m_lstObject[nNum]).Checked;
                }
                return CConvert.StrToBool(m_lstObject[nNum].Text);
            }

            public Control GetObject(int nNum) { if ((nNum < 0) || (nNum >= m_lstObject.Count)) return null; return m_lstObject[nNum]; }
            private int GetObject(Control ctrl)
            {
                int nNum = -1; for (int i = 0; i < m_lstObject.Count; i++) { if (ctrl == m_lstObject[i]) { nNum = i; break; } }
                return nNum;
            }

            private void obj_TextChanged(object sender, EventArgs e)
            {
                if (m_bLoading == false)
                {
                    int nNum = GetObject((System.Windows.Forms.TextBox)sender);
                    if (nNum >= 0) { m_CFile.SetData_String(nNum, m_lstObject[nNum].Text); }
                }
            }
            private void obj_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (m_bLoading == false)
                {
                    int nNum = GetObject((System.Windows.Forms.ComboBox)sender);
                    if (nNum >= 0) { m_CFile.SetData_Int(nNum, ((ComboBox)m_lstObject[nNum]).SelectedIndex); }
                }
            }
            private void obj_CheckedChanged(object sender, EventArgs e)
            {
                if (m_bLoading == false)
                {
                    int nNum = GetObject((Control)sender);
                    if (nNum >= 0)
                    {
                        if (m_lstObject[nNum] is RadioButton)
                        {
                            m_CFile.SetData_Bool(nNum, ((RadioButton)m_lstObject[nNum]).Checked);
                        }
                        else if (m_lstObject[nNum] is CheckBox)
                        {
                            m_CFile.SetData_Bool(nNum, ((RadioButton)m_lstObject[nNum]).Checked);
                        }
                    }
                }
            }
            private bool m_bNoLoad = false;
            public void Param_Load(string strName, int nCount)
            {
                m_bNoLoad = true;
                m_aCtrl = new Control[nCount];
                Param_Load(strName, m_aCtrl);
                //m_atxt = new TextBox[nCount];
                //Param_Load(strName, m_atxt);
            }
            private bool m_bLoading = false;

            public void Param_Load(string strName, params Control[] ctrl)
            {
                m_bLoading = true;
                m_nCount = ctrl.Length;
                m_strFileName = strName;
                int nCnt = m_CFile.Load(m_nCount, strName);

                if (m_lstObject.Count == 0) SetControlsAll(ctrl);
                if (nCnt <= 0)
                {
                    Ojw.CMessage.Write("[Warning] No parameter file Error : make new one[{0}]", String.Format("{0}.dat", strName));
                    Param_Save();
                }
                else
                {
                    for (int i = 0; i < m_lstObject.Count; i++)
                    {
                        if (m_lstObject[i] is TextBox)
                        {
                            m_lstObject[i].Text = m_CFile.GetData_String(i);
                        }
                        else if (m_lstObject[i] is ComboBox)
                        {
                            ((ComboBox)m_lstObject[i]).SelectedIndex = m_CFile.GetData_Int(i);
                        }
                        else if (m_lstObject[i] is RadioButton)
                        {
                            ((RadioButton)m_lstObject[i]).Checked = m_CFile.GetData_Bool(i);
                        }
                        else if (m_lstObject[i] is CheckBox)
                        {
                            ((CheckBox)m_lstObject[i]).Checked = m_CFile.GetData_Bool(i);
                        }
                        else
                        {
                            m_lstObject[i].Text = m_CFile.GetData_String(i);
                        }
                    }
                    //int i = 0;
                    //g_frmPage3.txtIp.Text = m_CFile.GetData_String(i++);        // 0 : _PARAM_IP
                    //g_frmPage3.txtPort.Text = m_CFile.GetData_String(i++);      // 1 : _PARAM_PORT
                    Ojw.CMessage.Write("Loaded Param data[{0}]", m_lstObject.Count);
                }
                m_bLoading = false;
            }
            public void Param_Load() { Param_Load(m_strFileName, m_nCount); }
            public void Param_Save()
            {
                for (int i = 0; i < m_lstObject.Count; i++)
                {
                    if (m_lstObject[i] is TextBox)
                    {
                        m_CFile.SetData_String(i, m_lstObject[i].Text);
                    }
                    else if (m_lstObject[i] is ComboBox)
                    {
                        m_CFile.SetData_Int(i, ((ComboBox)m_lstObject[i]).SelectedIndex);
                    }
                    else if (m_lstObject[i] is RadioButton)
                    {
                        m_CFile.SetData_Int(i, Ojw.CConvert.BoolToInt(((RadioButton)m_lstObject[i]).Checked));
                    }
                    else if (m_lstObject[i] is CheckBox)
                    {
                        m_CFile.SetData_Int(i, Ojw.CConvert.BoolToInt(((CheckBox)m_lstObject[i]).Checked));
                    }
                    else
                    {
                        m_CFile.SetData_String(i, m_lstObject[i].Text); 
                    }
                }
                m_CFile.Save(m_strFileName);
                Ojw.CMessage.Write("Loaded Param data[{0}]", m_nCount);
            }
        }
#endif
    }
}
