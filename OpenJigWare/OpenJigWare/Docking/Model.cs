using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenJigWare;

namespace OpenJigWare.Docking
{
    public partial class frmModel : Form
    {
        public frmModel()
        {
            InitializeComponent();
        }
        private bool m_bInit = false;
        private Ojw.C3d m_C3d = null;
        public void Init(Ojw.C3d C3d_Model)
        {
            if (C3d_Model != null)
            {
                m_C3d = C3d_Model;
                float fX, fY, fZ;
                float fPan, fTilt, fSwing;
                m_C3d.GetPos_Display(out fX, out fY, out fZ);
                m_C3d.GetAngle_Display(out fPan, out fTilt, out fSwing);
                txtX.Text = fX.ToString();
                txtY.Text = fY.ToString();
                txtZ.Text = fZ.ToString();
                txtPan.Text = fPan.ToString();
                txtTilt.Text = fTilt.ToString();
                txtSwing.Text = fSwing.ToString();
                m_bInit = true;
            }
        }
        private void frmModel_Load(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(0);
            m_C3d.Prop_Update_VirtualObject();
        }
        private void SetModel(String strObject)
        {
            if (m_bInit == true)
            {
                if (rdMode_Virtual.Checked == true)
                {
                    m_C3d.Prop_Set_DispObject(strObject);
                    m_C3d.Prop_Update_VirtualObject();
                }
                else
                {
                    m_C3d.Prop_Set_DispObject_Selected(strObject);
                    m_C3d.Prop_Update_Selected();
                }
            }
        }
        private void btnModel_Default_00_Click(object sender, EventArgs e) { SetModel("#0"); }
        private void btnModel_Default_01_Click(object sender, EventArgs e) { SetModel("#1"); }
        private void btnModel_Default_02_Click(object sender, EventArgs e) { SetModel("#2"); }
        private void btnModel_Default_03_Click(object sender, EventArgs e) { SetModel("#3"); }
        private void btnModel_Default_04_Click(object sender, EventArgs e) { SetModel("#4"); }
        private void btnModel_Default_05_Click(object sender, EventArgs e) { SetModel("#5"); }
        private void btnModel_Default_06_Click(object sender, EventArgs e) { SetModel("#6"); }
        private void btnModel_Default_07_Click(object sender, EventArgs e) { SetModel("#7"); }
        private void btnModel_Default_08_Click(object sender, EventArgs e) { SetModel("#8"); }
        private void btnModel_Default_09_Click(object sender, EventArgs e) { SetModel("#9"); }
        private void btnModel_Default_10_Click(object sender, EventArgs e) { SetModel("#10"); }
        private void btnModel_Default_11_Click(object sender, EventArgs e) { SetModel("#11"); }
        private void btnModel_Default_12_Click(object sender, EventArgs e) { SetModel("#12"); }
        private void btnModel_Default_13_Click(object sender, EventArgs e) { SetModel("#13"); }
        private void btnModel_Default_14_Click(object sender, EventArgs e) { SetModel("#14"); }

        private void txtX_TextChanged(object sender, EventArgs e)
        {
            m_C3d.SetPos_Display(Ojw.CConvert.StrToFloat(txtX.Text), Ojw.CConvert.StrToFloat(txtY.Text), Ojw.CConvert.StrToFloat(txtZ.Text));
        }

        private void txtY_TextChanged(object sender, EventArgs e)
        {
            m_C3d.SetPos_Display(Ojw.CConvert.StrToFloat(txtX.Text), Ojw.CConvert.StrToFloat(txtY.Text), Ojw.CConvert.StrToFloat(txtZ.Text));
        }

        private void txtZ_TextChanged(object sender, EventArgs e)
        {
            m_C3d.SetPos_Display(Ojw.CConvert.StrToFloat(txtX.Text), Ojw.CConvert.StrToFloat(txtY.Text), Ojw.CConvert.StrToFloat(txtZ.Text));
        }

        private void txtPan_TextChanged(object sender, EventArgs e)
        {
            //m_C3d.SelectMotor(1); // test
            m_C3d.SetAngle_Display(Ojw.CConvert.StrToFloat(txtPan.Text), Ojw.CConvert.StrToFloat(txtTilt.Text), Ojw.CConvert.StrToFloat(txtSwing.Text));
        }

        private void txtTilt_TextChanged(object sender, EventArgs e)
        {
            m_C3d.SetAngle_Display(Ojw.CConvert.StrToFloat(txtPan.Text), Ojw.CConvert.StrToFloat(txtTilt.Text), Ojw.CConvert.StrToFloat(txtSwing.Text));
        }

        private void txtSwing_TextChanged(object sender, EventArgs e)
        {
            m_C3d.SetAngle_Display(Ojw.CConvert.StrToFloat(txtPan.Text), Ojw.CConvert.StrToFloat(txtTilt.Text), Ojw.CConvert.StrToFloat(txtSwing.Text));
        }

        private Ojw.CMyo m_CMyo = new Ojw.CMyo();
        private void chkMyo_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMyo.Checked == true)
            {
                m_CMyo.InitMyo();
                tmrMyo.Enabled = true;
            }
            else
            {
                m_CMyo.DInitMyo();
                tmrMyo.Enabled = false;
            }
        }
        private int m_nControlMode = -1;
        private float m_fWidth = 0.0f;
        private float m_fWidth_Init = 0.0f;
        private float m_fWidth_Object = 0.0f;
        private float m_fHeight = 0.0f;
        private float m_fHeight_Init = 0.0f;
        private float m_fHeight_Object = 0.0f;
        private float m_fDepth = 0.0f;
        private float m_fDepth_Init = 0.0f;
        private float m_fDepth_Object = 0.0f;
        private string[] m_pstrObject = new string[] 
        {
            "#0", "#1", "#2", "#3", "#4", "#5", "#6", "#7", "#8", "#9", "#10", "#11", "#12", "#13", "#14",// "#15", "#16", "#17", 
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", 
            "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", 
            "42", "42"
        };
        private bool m_bRightCommand = false;
        private void tmrMyo_Tick(object sender, EventArgs e)
        {
            if (m_CMyo.IsInit() == true)
            {
                bool bRight, bLeft;
                float fR_Pan, fR_Tilt, fR_Swing;
                float fL_Pan, fL_Tilt, fL_Swing;
                MyoSharp.Poses.Pose ER_Pose = new MyoSharp.Poses.Pose();
                MyoSharp.Poses.Pose EL_Pose = new MyoSharp.Poses.Pose();
                bool bRet = m_CMyo.GetData(
                                        out bRight, out fR_Pan, out fR_Tilt, out fR_Swing, out ER_Pose, 
                                        out bLeft, out fL_Pan, out fL_Tilt, out fL_Swing, out EL_Pose);
                if (bRet == true)
                {
                    if ((bRight == true) && (bLeft == true))
                    {
                        if (
                            (ER_Pose != MyoSharp.Poses.Pose.Rest)
                            &&
                            (EL_Pose == MyoSharp.Poses.Pose.Fist)
                            )
                        {
                            if (m_bRightCommand == true) return;
                        }
                        else
                            m_bRightCommand = false;
                        if (
                            (ER_Pose == MyoSharp.Poses.Pose.FingersSpread)
                            &&
                            (EL_Pose == MyoSharp.Poses.Pose.FingersSpread)
                            )
                        {

                            //Ojw.CMessage.Write("Spread!!!");

                            if (m_nControlMode < 0)
                            {
                                m_nControlMode = 0;
                                m_fWidth = 0.0f;
                                m_fWidth_Init = fR_Pan - fL_Pan;//m_C3d.Prop_Get_Width_Or_Radius();
                                m_fWidth_Object = m_C3d.Prop_Get_Width_Or_Radius();
                                m_fHeight = 0.0f;
                                m_fHeight_Init = fR_Tilt - fL_Tilt;//m_C3d.Prop_Get_Width_Or_Radius();
                                m_fHeight_Object = m_C3d.Prop_Get_Height_Or_Depth();
                                m_fDepth = 0.0f;
                                m_fDepth_Init = fR_Swing - fL_Swing;
                                m_fDepth_Object = m_C3d.Prop_Get_Depth_Or_Cnt();
                            }
                            else
                            {
                                float fWidth = fR_Pan - fL_Pan - m_fWidth_Init;//m_C3d.Prop_Get_Width_Or_Radius() - m_fWidth_Init;
                                float fHeight = fR_Tilt - fL_Tilt - m_fHeight_Init;
                                float fDepth = fR_Swing - fL_Swing - m_fDepth_Init;
                                if (Math.Abs(fDepth) > 0.1f)
                                {
                                    m_fDepth = fDepth - 0.1f;
                                    if ((Math.Abs(fDepth) >= 0.1f) && (m_fDepth_Object + m_fDepth * 100.0f > 0))
                                    {
                                        m_C3d.Prop_Set_Depth_Or_Cnt(m_fDepth_Object + m_fDepth * 100.0f);
                                    }
                                }
                                else if ((Math.Abs(fWidth) - Math.Abs(fHeight)) > 0.1f)
                                {
                                    m_fWidth = fWidth - 0.1f;
                                    if ((Math.Abs(fWidth) >= 0.1f) && (m_fWidth_Object - m_fWidth * 100.0f > 0))
                                    {
                                        m_C3d.Prop_Set_Width_Or_Radius(m_fWidth_Object - m_fWidth * 100.0f);
                                    }
                                }
                                else if ((Math.Abs(fHeight) - Math.Abs(fWidth)) > 0.1f)
                                {
                                    m_fHeight = fHeight - 0.1f;
                                    if ((Math.Abs(fHeight) >= 0.1f) && (m_fHeight_Object + m_fHeight * 100.0f > 0))
                                    {
                                        m_C3d.Prop_Set_Height_Or_Depth(m_fHeight_Object + m_fHeight * 100.0f);
                                    }
                                }
                                m_C3d.Prop_Update_VirtualObject();
                                //Ojw.CMessage.Write("Width={0}", m_fWidth);
                            }
                        }
                        else if (EL_Pose == MyoSharp.Poses.Pose.Fist)
                        {
                            if (ER_Pose == MyoSharp.Poses.Pose.WaveOut)
                            {
                                m_bRightCommand = true;

                                string strObject = m_C3d.Prop_Get_DispObject();
                                int nObject = -1;
                                for (int i = 0; i < m_pstrObject.Length; i++)
                                {
                                    if (strObject == m_pstrObject[i])
                                    {
                                        nObject = i + 1;
                                        break;
                                    }
                                }
                                if (nObject >= 0)
                                {
                                    m_C3d.Prop_Set_DispObject(m_pstrObject[nObject]);
                                }
                                m_C3d.Prop_Update_VirtualObject();
                                //Ojw.CTimer.Wait(1000);
                            }
                            else if (ER_Pose == MyoSharp.Poses.Pose.WaveIn)
                            {
                                m_bRightCommand = true;

                                string strObject = m_C3d.Prop_Get_DispObject();
                                int nObject = -1;
                                for (int i = 0; i < m_pstrObject.Length; i++)
                                {
                                    if (strObject == m_pstrObject[i])
                                    {
                                        nObject = i - 1;
                                        break;
                                    }
                                }
                                if ((nObject >= 0) && (nObject < m_pstrObject.Length))
                                {
                                    m_C3d.Prop_Set_DispObject(m_pstrObject[nObject]);
                                }
                                m_C3d.Prop_Update_VirtualObject();
                                //Ojw.CTimer.Wait(1000);
                            }
                            else if (ER_Pose == MyoSharp.Poses.Pose.FingersSpread)
                            {
                                if (m_nControlMode < 0)
                                {
                                    m_nControlMode = 0;
                                    m_fWidth = 0.0f;
                                    m_fWidth_Init = fR_Pan;// -fL_Pan;//m_C3d.Prop_Get_Width_Or_Radius();
                                    Ojw.SVector3D_t SVec = m_C3d.Prop_Get_Trans_1();
                                    m_fWidth_Object = SVec.x;
                                    m_fHeight = 0.0f;
                                    m_fHeight_Init = fR_Tilt;// -fL_Tilt;//m_C3d.Prop_Get_Width_Or_Radius();
                                    m_fHeight_Object = SVec.y;
                                    m_fDepth = 0.0f;
                                    m_fDepth_Init = fR_Swing;// -fL_Swing;
                                    m_fDepth_Object = SVec.z;
                                    Ojw.CMessage.Write("************");
                                }
                                else
                                {
                                    float fWidth = fR_Pan - m_fWidth_Init;//m_C3d.Prop_Get_Width_Or_Radius() - m_fWidth_Init;
                                    float fHeight = fR_Tilt - m_fHeight_Init;
                                    float fDepth = fR_Swing - m_fDepth_Init;
                                    if (Math.Abs(fDepth) > 0.1f)
                                    {
                                        m_fDepth = fDepth - 0.1f;
                                        if (Math.Abs(fDepth) >= 0.1f)
                                        {
                                            //Ojw.SVector3D_t SVec = new Ojw.SVector3D_t(m_fWidth_Object, m_fHeight_Object, m_fDepth_Object + m_fDepth * 100.0f);
                                            Ojw.SVector3D_t SVec = new Ojw.SVector3D_t(m_fWidth_Object, m_fHeight_Object, m_fDepth_Object + m_fDepth * 100.0f);
                                            m_C3d.Prop_Set_Trans_1(SVec);
                                        }
                                    }
                                    else if ((Math.Abs(fWidth) - Math.Abs(fHeight)) > 0.1f)
                                    {
                                        m_fWidth = fWidth - 0.1f;
                                        if (Math.Abs(fWidth) >= 0.1f)
                                        {
                                            Ojw.SVector3D_t SVec = new Ojw.SVector3D_t(m_fWidth_Object - m_fWidth * 100.0f, m_fHeight_Object, m_fDepth_Object);
                                            m_C3d.Prop_Set_Trans_1(SVec);                                            
                                        }
                                    }
                                    else if ((Math.Abs(fHeight) - Math.Abs(fWidth)) > 0.1f)
                                    {
                                        m_fHeight = fHeight - 0.1f;
                                        if (Math.Abs(fHeight) >= 0.1f)
                                        {
                                            //Ojw.SVector3D_t SVec = new Ojw.SVector3D_t(m_fWidth_Object, m_fHeight_Object + m_fHeight * 100.0f, m_fDepth_Object);
                                            Ojw.SVector3D_t SVec = new Ojw.SVector3D_t(m_fWidth_Object, m_fHeight_Object + m_fHeight * 100.0f, m_fDepth_Object);
                                            m_C3d.Prop_Set_Trans_1(SVec);
                                        }
                                    }
                                    m_C3d.Prop_Update_VirtualObject();
                                    //Ojw.CMessage.Write("Width={0}", m_fWidth);
                                }
                            }
                            else if (ER_Pose == MyoSharp.Poses.Pose.DoubleTap)
                            {
                                m_C3d.AddVirtualClassToReal();
                            }
                            else
                            {
                                m_nControlMode = -1;
                            }
                        }
                        
                        
                        
                        //else if (ER_Pose == MyoSharp.Poses.Pose.FingersSpread)
                        //{
                        //}
                        //else if (EL_Pose == MyoSharp.Poses.Pose.FingersSpread)
                        //{
                        //}
                        else
                        {
                            m_nControlMode = -1;
                        }
                    }
                }
            }
        }

        private void btnModel_Ase_00_Click(object sender, EventArgs e) { SetModel("0"); }
        private void btnModel_Ase_01_Click(object sender, EventArgs e) { SetModel("1"); }
        private void btnModel_Ase_02_Click(object sender, EventArgs e) { SetModel("2"); }
        private void btnModel_Ase_03_Click(object sender, EventArgs e) { SetModel("3"); }
        private void btnModel_Ase_04_Click(object sender, EventArgs e) { SetModel("4"); }
        private void btnModel_Ase_05_Click(object sender, EventArgs e) { SetModel("5"); }
        private void btnModel_Ase_06_Click(object sender, EventArgs e) { SetModel("6"); }
        private void btnModel_Ase_07_Click(object sender, EventArgs e) { SetModel("7"); }
        private void btnModel_Ase_08_Click(object sender, EventArgs e) { SetModel("8"); }
        private void btnModel_Ase_09_Click(object sender, EventArgs e) { SetModel("9"); }
        private void btnModel_Ase_14_Click(object sender, EventArgs e) { SetModel("14"); }
        private void btnModel_Ase_24_Click(object sender, EventArgs e) { SetModel("24"); }
        private void btnModel_Ase_17_Click(object sender, EventArgs e) { SetModel("17"); }

        private void btnModel_Ase_10_Click(object sender, EventArgs e) { SetModel("10"); }
        private void btnModel_Ase_11_Click(object sender, EventArgs e) { SetModel("11"); }
        private void btnModel_Ase_12_Click(object sender, EventArgs e) { SetModel("12"); }
        private void btnModel_Ase_13_Click(object sender, EventArgs e) { SetModel("13"); }
        private void btnModel_Ase_15_Click(object sender, EventArgs e) { SetModel("15"); }
        private void btnModel_Ase_16_Click(object sender, EventArgs e) { SetModel("16"); }
        private void btnModel_Ase_18_Click(object sender, EventArgs e) { SetModel("18"); }
        private void btnModel_Ase_19_Click(object sender, EventArgs e) { SetModel("19"); }
        private void btnModel_Ase_20_Click(object sender, EventArgs e) { SetModel("20"); }
        private void btnModel_Ase_21_Click(object sender, EventArgs e) { SetModel("21"); }
        private void btnModel_Ase_22_Click(object sender, EventArgs e) { SetModel("22"); }
        private void btnModel_Ase_23_Click(object sender, EventArgs e) { SetModel("23"); }
        private void btnModel_Ase_25_Click(object sender, EventArgs e) { SetModel("25"); }
        private void btnModel_Ase_26_Click(object sender, EventArgs e) { SetModel("26"); }
        private void btnModel_Ase_27_Click(object sender, EventArgs e) { SetModel("27"); }
        private void btnModel_Ase_28_Click(object sender, EventArgs e) { SetModel("28"); }
        private void btnModel_Ase_29_Click(object sender, EventArgs e) { SetModel("29"); }
        private void btnModel_Ase_30_Click(object sender, EventArgs e) { SetModel("30"); }

        private void btnModel_Ase_31_Click(object sender, EventArgs e) { SetModel("31"); }
        private void btnModel_Ase_32_Click(object sender, EventArgs e) { SetModel("32"); }
        private void btnModel_Ase_33_Click(object sender, EventArgs e) { SetModel("33"); }
        private void btnModel_Ase_34_Click(object sender, EventArgs e) { SetModel("34"); }

        private void btnModel_Ase_35_Click(object sender, EventArgs e) { SetModel("35"); }
        private void btnModel_Ase_36_Click(object sender, EventArgs e) { SetModel("36"); }
        private void btnModel_Ase_37_Click(object sender, EventArgs e) { SetModel("37"); }
        private void btnModel_Ase_38_Click(object sender, EventArgs e) { SetModel("38"); }
        private void btnModel_Ase_39_Click(object sender, EventArgs e) { SetModel("39"); }
        private void btnModel_Ase_40_Click(object sender, EventArgs e) { SetModel("40"); }
        private void btnModel_Ase_41_Click(object sender, EventArgs e) { SetModel("41"); }
        private void btnModel_Ase_42_Click(object sender, EventArgs e) { SetModel("42"); }

        private void txtScale_TextChanged(object sender, EventArgs e)
        {
            m_C3d.SetScale(Ojw.CConvert.StrToFloat(txtScale.Text));
        }
        public void SetScale(float fValue)
        {
            txtScale.Text = Ojw.CConvert.FloatToStr(fValue);
        }
        public void SetPosition(float fX, float fY, float fZ)
        {
            txtX.Text = Ojw.CConvert.FloatToStr(fX);
            txtY.Text = Ojw.CConvert.FloatToStr(fY);
            txtZ.Text = Ojw.CConvert.FloatToStr(fZ);
        }
        public void SetAngle(float fPan, float fTilt, float fSwing)
        {
            txtPan.Text = Ojw.CConvert.FloatToStr(fPan);
            txtTilt.Text = Ojw.CConvert.FloatToStr(fTilt);
            txtSwing.Text = Ojw.CConvert.FloatToStr(fSwing);
        }

        private void btnMouseMode_Disp_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(0);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnMouseMode_Control_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(1);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnOffset_Trans_X_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(4);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnOffset_Trans_Y_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(5);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnOffset_Trans_Z_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(6);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnOffset_Rot_X_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(7);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnOffset_Rot_Y_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(8);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnOffset_Rot_Z_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(9);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnTrans_X_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(10);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnTrans_Y_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(11);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnTrans_Z_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(12);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnRot_X_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(13);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnRot_Y_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(14);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnRot_Z_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(15);
            m_C3d.Prop_Update_VirtualObject();
        }
    }
}
