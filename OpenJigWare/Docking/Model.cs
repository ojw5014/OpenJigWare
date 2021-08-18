using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenJigWare;
using System.IO;

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

                m_CLeap.Set3D(m_C3d);

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

        private void btnMakeSstl_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd3d = new OpenFileDialog();
            ofd3d.Filter = "3d Files(*.stl,*.ase,*.obj,*.dat)|*.stl;*.ase;*.obj;*.dat";
            //ofd3d.DefaultExt = "mp3";
            ofd3d.InitialDirectory = Application.StartupPath + m_C3d.GetAseFile_Path();
            if (ofd3d.ShowDialog() == DialogResult.OK)
            {
                String fileName = ofd3d.FileName;

                if (fileName.ToLower().IndexOf(".stl") > 0) // 파일명 없이 확장자만 있는것도 에러
                {
                    m_C3d.OjwFileConvert_STL_to_SSTL(fileName);
                }
                else if (fileName.ToLower().IndexOf(".ase") > 0) // 파일명 없이 확장자만 있는것도 에러
                {
                    m_C3d.OjwFileConvert_ASE_to_SSTL(fileName);
                }
                else if (fileName.ToLower().IndexOf(".obj") > 0) // 파일명 없이 확장자만 있는것도 에러
                {
                    m_C3d.OjwFileConvert_OBJ_to_SSTL(fileName);
                }
                else if (fileName.ToLower().IndexOf(".dat") > 0) // 파일명 없이 확장자만 있는것도 에러
                {
                    m_C3d.OjwFileConvert_DAT_to_SSTL(fileName);
                }
            }            
        }

        private void btnMakeDat_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd3d = new OpenFileDialog();
            ofd3d.Filter = "3d Files(*.stl,*.ase,*.obj,*.dat)|*.stl;*.ase;*.obj;*.dat";
            //ofd3d.DefaultExt = "mp3";
            ofd3d.InitialDirectory = Application.StartupPath + m_C3d.GetAseFile_Path();
            if (ofd3d.ShowDialog() == DialogResult.OK)
            {
                String fileName = ofd3d.FileName;

                if (fileName.ToLower().IndexOf(".stl") > 0) // 파일명 없이 확장자만 있는것도 에러
                {
                    m_C3d.OjwFileConvert_STL_to_Dat(fileName);
                }
            }      
        }

        private Ojw.CLeap m_CLeap = new Ojw.CLeap();
        private void chkLeap_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLeap.Checked == true)
            {
                m_CLeap.InitLeap();
                tmrLeap.Enabled = true;
            }
            else
            {
                m_CLeap.DInitLeap();
                tmrLeap.Enabled = false;
            }
        }

        private void tmrLeap_Tick(object sender, EventArgs e)
        {

        }

        private void btnMakeJson_Click(object sender, EventArgs e)
        {
#if false // 17dof
            List<string> lstGuide = new List<string>();
            lstGuide.Clear();
            //////////////////////////////
            lstGuide.Add("오른어깨(Pitch)"); // OnGuide_Name
            lstGuide.Add("1"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("1"); // OnGuide_AxisType
            lstGuide.Add("0"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("1.1"); // OnGuide_3D_Scale
            lstGuide.Add("0.2"); // OnGuide_3D_Alpha
            lstGuide.Add("-10,-10,0,90,0,0"); // OnGuide_Pos(x/y/z,p/t/s)
            //lstGuide.Add("-1,-1,-1,4,-1,-1");
            lstGuide.Add("-1,-1,-1,17,-1,-1");// OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼어깨(Pitch)"); // OnGuide_Name
            lstGuide.Add("2"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("1"); // OnGuide_AxisType
            lstGuide.Add("0"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("1.1"); // OnGuide_3D_Scale
            lstGuide.Add("0.2"); // OnGuide_3D_Alpha
            lstGuide.Add("10,-10,0,-90,0,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,17,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            //////////////////////////////
            lstGuide.Add("오른어깨(Roll)"); // OnGuide_Name
            lstGuide.Add("3"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("1.1"); // OnGuide_3D_Scale
            lstGuide.Add("0.2"); // OnGuide_3D_Alpha
            lstGuide.Add("50,-10,0,0,0,0"); // OnGuide_Pos(x/y/z,p/t/s)
            //lstGuide.Add("-1,-1,-1,4,-1,-1");
            lstGuide.Add("-1,-1,-1,-1,-1,-1");// OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            //lstGuide.Add("-1,-1,-1,-1,1,-1");// OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,-1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼어깨(Roll)"); // OnGuide_Name
            lstGuide.Add("4"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("1.1"); // OnGuide_3D_Scale
            lstGuide.Add("0.2"); // OnGuide_3D_Alpha
            lstGuide.Add("-50,0,0,0,0,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            //lstGuide.Add("-1,-1,-1,-1,2,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,-1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            //////////////////////////////
            lstGuide.Add("오른손"); // OnGuide_Name
            lstGuide.Add("5"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("1.1"); // OnGuide_3D_Scale
            lstGuide.Add("0.2"); // OnGuide_3D_Alpha
            lstGuide.Add("50,-10,0,0,0,0"); // OnGuide_Pos(x/y/z,p/t/s)
            //lstGuide.Add("-1,-1,-1,4,-1,-1");
            lstGuide.Add("-1,-1,-1,-1,-1,-1");// OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            //lstGuide.Add("-1,-1,-1,-1,1,-1");// OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,-1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼손"); // OnGuide_Name
            lstGuide.Add("6"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("1.1"); // OnGuide_3D_Scale
            lstGuide.Add("0.2"); // OnGuide_3D_Alpha
            lstGuide.Add("-50,0,0,0,0,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            //lstGuide.Add("-1,-1,-1,-1,2,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,-1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("허리-좌우"); // OnGuide_Name
            lstGuide.Add("17"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("0"); // OnGuide_AxisType
            lstGuide.Add("1"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("1.5"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("0,-50,100,0,-30,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            //lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("오른다리(Roll)"); // OnGuide_Name
            lstGuide.Add("7"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("1"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("30,0,0,0,-20,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼다리(Roll)"); // OnGuide_Name
            lstGuide.Add("8"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("1"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("-30,0,0,0,-20,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("오른다리(Pitch)"); // OnGuide_Name
            lstGuide.Add("9"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("0"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("30,0,0,90,-20,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼다리(Pitch)"); // OnGuide_Name
            lstGuide.Add("10"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("0"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("-30,0,0,-90,-20,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("오른무릎"); // OnGuide_Name
            lstGuide.Add("11"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("0"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("30,0,0,90,-20,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼무릎"); // OnGuide_Name
            lstGuide.Add("12"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("0"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("-30,0,0,-90,-20,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("오른발목(Pitch)"); // OnGuide_Name
            lstGuide.Add("13"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("0"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("30,50,0,90,-10,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼발목(Pitch)"); // OnGuide_Name
            lstGuide.Add("14"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("0"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("-30,50,0,-90,-10,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,-1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("오른발목(Roll)"); // OnGuide_Name
            lstGuide.Add("15"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("1"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("30,50,0,0,-10,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼발목(Roll)"); // OnGuide_Name
            lstGuide.Add("16"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("1"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("-30,50,0,0,-10,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,-1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)



            List<string> lstGroupName = new List<string>();
            lstGroupName.Clear();
            //////////////////////////////
            lstGroupName.Add("1");
            lstGroupName.Add("오른팔");
            lstGroupName.Add("1,3,5");
            //////////////////////////////
            lstGroupName.Add("2");
            lstGroupName.Add("왼팔");
            lstGroupName.Add("2,4,6");
            //////////////////////////////
            lstGroupName.Add("3");
            lstGroupName.Add("오른다리");
            lstGroupName.Add("7,9,11,13,15");
            //////////////////////////////
            lstGroupName.Add("4");
            lstGroupName.Add("왼다리");
            lstGroupName.Add("8,10,12,14,16");
            //////////////////////////////
            lstGroupName.Add("5");
            lstGroupName.Add("허리-좌우");
            lstGroupName.Add("17");

            File_MakeJson("R-Tong 17Dof", "rtong_17dof.json", lstGuide.ToArray(), lstGroupName.ToArray());
#endif
#if false // 8dof
            List<string> lstGuide = new List<string>();
            lstGuide.Clear();
            //////////////////////////////
            lstGuide.Add("오른어깨"); // OnGuide_Name
            lstGuide.Add("1"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("1"); // OnGuide_AxisType
            lstGuide.Add("0"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("1.1"); // OnGuide_3D_Scale
            lstGuide.Add("0.2"); // OnGuide_3D_Alpha
            lstGuide.Add("-10,-10,0,90,0,0"); // OnGuide_Pos(x/y/z,p/t/s)
            //lstGuide.Add("-1,-1,-1,4,-1,-1");
            lstGuide.Add("-1,-1,-1,-1,-1,-1");// OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼어깨"); // OnGuide_Name
            lstGuide.Add("2"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("1"); // OnGuide_AxisType
            lstGuide.Add("0"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("1.1"); // OnGuide_3D_Scale
            lstGuide.Add("0.2"); // OnGuide_3D_Alpha
            lstGuide.Add("10,-10,0,-90,0,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            //////////////////////////////
            lstGuide.Add("오른손"); // OnGuide_Name
            lstGuide.Add("3"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("1.1"); // OnGuide_3D_Scale
            lstGuide.Add("0.2"); // OnGuide_3D_Alpha
            lstGuide.Add("50,-10,0,0,0,0"); // OnGuide_Pos(x/y/z,p/t/s)
            //lstGuide.Add("-1,-1,-1,4,-1,-1");
            lstGuide.Add("-1,-1,-1,-1,-1,-1");// OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            //lstGuide.Add("-1,-1,-1,-1,1,-1");// OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,-1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼손"); // OnGuide_Name
            lstGuide.Add("4"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("1.1"); // OnGuide_3D_Scale
            lstGuide.Add("0.2"); // OnGuide_3D_Alpha
            lstGuide.Add("-50,0,0,0,0,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            //lstGuide.Add("-1,-1,-1,-1,2,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,-1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("오른다리"); // OnGuide_Name
            lstGuide.Add("5"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("0"); // OnGuide_AxisType
            lstGuide.Add("1"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("30,0,0,0,-20,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼다리"); // OnGuide_Name
            lstGuide.Add("6"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("0"); // OnGuide_AxisType
            lstGuide.Add("1"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("-30,0,0,0,-20,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,-1,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("오른발목"); // OnGuide_Name
            lstGuide.Add("7"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("30,50,0,0,-10,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,5,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
            //////////////////////////////
            lstGuide.Add("왼발목"); // OnGuide_Name
            lstGuide.Add("8"); // OnGuide_ID
            lstGuide.Add("1"); // OnGuide_Event
            lstGuide.Add("2"); // OnGuide_AxisType
            lstGuide.Add("2"); // OnGuide_RingColorType
            lstGuide.Add("50"); // OnGuide_RingSize
            lstGuide.Add("10"); // OnGuide_RingThick
            lstGuide.Add("-1"); // OnGuide_RingDir(1[F],-1[B])
            lstGuide.Add("2.0"); // OnGuide_3D_Scale
            lstGuide.Add("1.0"); // OnGuide_3D_Alpha
            lstGuide.Add("-30,50,0,0,-10,0"); // OnGuide_Pos(x/y/z,p/t/s)
            lstGuide.Add("-1,-1,-1,6,-1,-1"); // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
            lstGuide.Add("1,1,1,-1,1,1"); // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)



            List<string> lstGroupName = new List<string>();
            lstGroupName.Clear();
            //////////////////////////////
            lstGroupName.Add("1");
            lstGroupName.Add("오른팔");
            lstGroupName.Add("1,3");
            //////////////////////////////
            lstGroupName.Add("2");
            lstGroupName.Add("왼팔");
            lstGroupName.Add("2,4");
            //////////////////////////////
            lstGroupName.Add("3");
            lstGroupName.Add("오른다리");
            lstGroupName.Add("5,7");
            //////////////////////////////
            lstGroupName.Add("4");
            lstGroupName.Add("왼다리");
            lstGroupName.Add("6,8");
            //////////////////////////////

            File_MakeJson("R-Tong", "rtong_8dof.json", lstGuide.ToArray(), lstGroupName.ToArray());
#endif
            string str = "noname.json";
            if (Ojw.CInputBox.Show("Json Export", "File Name(*.json) 을 정해 주세요", ref str) == System.Windows.Forms.DialogResult.OK)
            {
                m_C3d.Json_Export("rtong_8dof.json");
            }
        }
        private bool File_MakeJson(string strTitle, string strFileName)
        {
            List<string> lstGuide = new List<string>();
            lstGuide.Clear();
            int nID;
            List<int> lstGroupIndex = new List<int>();
            lstGroupIndex.Clear();
            for (int i = 0; i < m_C3d.m_CHeader.anMotorIDs.Length; i++)
            {
                nID = m_C3d.m_CHeader.anMotorIDs[i];
                // OnGuide_Name
                lstGuide.Add(m_C3d.m_CHeader.pSMotorInfo[nID].strNickName);
                // OnGuide_ID
                lstGuide.Add(Ojw.CConvert.IntToStr(nID));
                // OnGuide_Event
                lstGuide.Add(Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nID].nGuide_Event));
                // OnGuide_AxisType
                lstGuide.Add(Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nID].nGuide_AxisType));
                // OnGuide_RingColorType
                lstGuide.Add(Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nID].nGuide_RingColorType));
                // OnGuide_RingSize
                lstGuide.Add(Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nID].fGuide_RingSize));
                // OnGuide_RingThick
                lstGuide.Add(Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nID].fGuide_RingThick));
                // OnGuide_RingDir(1[F],-1[B])
                lstGuide.Add(Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nID].nGuide_RingDir));
                // OnGuide_3D_Scale
                lstGuide.Add(Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nID].fGuide_3D_Scale));
                // OnGuide_3D_Alpha
                lstGuide.Add(Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nID].fGuide_3D_Alpha));
                // OnGuide_Pos(x/y/z,p/t/s)
                lstGuide.Add(String.Format("{0},{1},{2},{3},{4},{5}", 
                    m_C3d.m_CHeader.pSMotorInfo[nID].afGuide_Pos[0],
                    m_C3d.m_CHeader.pSMotorInfo[nID].afGuide_Pos[1],
                    m_C3d.m_CHeader.pSMotorInfo[nID].afGuide_Pos[2],
                    m_C3d.m_CHeader.pSMotorInfo[nID].afGuide_Pos[3],
                    m_C3d.m_CHeader.pSMotorInfo[nID].afGuide_Pos[4],
                    m_C3d.m_CHeader.pSMotorInfo[nID].afGuide_Pos[5]));
                // OnGuide_Off_IDs(Axis...x/y/z/p/t/s)
                lstGuide.Add(String.Format("{0},{1},{2},{3},{4},{5}",
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_IDs[0],
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_IDs[1],
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_IDs[2],
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_IDs[3],
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_IDs[4],
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_IDs[5]));
                // OnGuide_Off_Dir(Axis...x/y/z/p/t/s)
                lstGuide.Add(String.Format("{0},{1},{2},{3},{4},{5}", 
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_Dir[0],
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_Dir[1],
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_Dir[2],
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_Dir[3],
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_Dir[4],
                    m_C3d.m_CHeader.pSMotorInfo[nID].anGuide_Off_Dir[5]));

                //int nGroupIndex = m_C3d.m_CHeader.pSMotorInfo[nID].nGroupNumber
                //if (lstGroupIndex.IndexOf(
            }

#if false
            //////////////////////////////
            lstGroupName.Add("1");
            lstGroupName.Add("오른팔");
            lstGroupName.Add("1,3");
            //////////////////////////////
            lstGroupName.Add("2");
            lstGroupName.Add("왼팔");
            lstGroupName.Add("2,4");
            //////////////////////////////
            lstGroupName.Add("3");
            lstGroupName.Add("오른다리");
            lstGroupName.Add("5,7");
            //////////////////////////////
            lstGroupName.Add("4");
            lstGroupName.Add("왼다리");
            lstGroupName.Add("6,8");
            //////////////////////////////
#else
            bool bStart = true;
            List<string> lstGroupName = new List<string>();
            lstGroupName.Clear();
            string str = string.Empty;
            for (int i = 0; i < m_C3d.m_CHeader.lstGroupInfo.Count; i++)
            {
                lstGroupName.Add(Ojw.CConvert.IntToStr(m_C3d.m_CHeader.lstGroupInfo[i].nNumber));
                lstGroupName.Add(m_C3d.m_CHeader.lstGroupInfo[i].strName);
                bStart = true;
                str = string.Empty;
                for (int j = 0; j < m_C3d.m_CHeader.anMotorIDs.Length; j++)
                {
                    if (bStart == true) bStart = false;
                    else str += ",";
                    nID = m_C3d.m_CHeader.anMotorIDs[j];
                    str += Ojw.CConvert.IntToStr(nID);
                }
                lstGroupName.Add(str);
            }
#endif
            return File_MakeJson(strTitle, strFileName, lstGuide.ToArray(), lstGroupName.ToArray());
        }
        private bool File_MakeJson(string strTitle, string strFileName, string [] astrGuide, string [] astrGroupName)
        {
            #region Json File
            FileInfo f = new FileInfo(strFileName);
            FileStream fs = f.Create();
            try
            {
                //string strVersion = m_C3d.m_CHeader.strVersion;

                // 스트림 버퍼를 비운다.
                fs.Flush();
                byte[] byteBuffer;
                // Start
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8("{\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);

                //m_CHeader.nMotorCnt
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"Design\" : \"{0}\",\r\n", m_C3d.m_CHeader.nModelNum)); fs.Write(byteBuffer, 0, byteBuffer.Length);

                string strVersion = "01.00.00";
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"Design_Version\" : \"{0}\",\r\n", strVersion)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                string strName = ((strTitle == "") ? m_C3d.m_CHeader.strModelName : strTitle);
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"Robot\" : \"{0}\",\r\n", strName)); fs.Write(byteBuffer, 0, byteBuffer.Length);

                // 새롭게 추가
                int nScanningQr = 0;
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"ScanningQr(1:Qr,0:Ble)\" : \"{0}\",\r\n", nScanningQr)); fs.Write(byteBuffer, 0, byteBuffer.Length);

                string str3D_Position = "0,0,0,0,0,0";
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"Scene_Position\" : \"{0}\",\r\n", str3D_Position)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                float fX, fY, fZ;
                float fP, fT, fS;
                m_C3d.GetPos_Display(out fX, out fY, out fZ);
                m_C3d.GetAngle_Display(out fP, out fT, out fS);
                str3D_Position = string.Format("{0},{1},{2},{3},{4},{5}", fX, fY, fZ, fP, fT, fS);
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"Camera_Position\" : \"{0}\",\r\n", str3D_Position)); fs.Write(byteBuffer, 0, byteBuffer.Length);

                fX = 0.0f; fY = 72.0f; fZ = 0.0f;
                fP = 0.0f; fT = 0.0f; fS = 0.0f;
                str3D_Position = string.Format("{0},{1},{2},{3},{4},{5}", fX, fY, fZ, fP, fT, fS);
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"Robot_Position\" : \"{0}\",\r\n", str3D_Position)); fs.Write(byteBuffer, 0, byteBuffer.Length);



                string strArg0, strArg1;

                strArg0 = "Robot_Scale"; strArg1 = "1.0";
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                strArg0 = "BackColor"; strArg1 = "0x" + Ojw.CConvert.IntToHex(m_C3d.GetBackColor().ToArgb(), 6);
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                strArg0 = "BackColor_Edit"; strArg1 = "0x" + Ojw.CConvert.IntToHex(m_C3d.GetBackColor().ToArgb() - 0x10000f, 6);
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                strArg0 = "PlaneColor"; strArg1 = "0x" + Ojw.CConvert.IntToHex(0xF0F0F0, 6);
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                strArg0 = "IsWireFrame"; strArg1 = "0";
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                strArg0 = "BleName"; strArg1 = "RB-100";
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                strArg0 = "Address_Motion"; strArg1 = "0xE2000";
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                strArg0 = "Address_Control"; strArg1 = "66";
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                ////
                // Guide_Group
                #region Guide_Group
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\"Guide_Group\" : [\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                int nItemCount = 13;
                for (int i = 0; i < astrGuide.Length; i += nItemCount)
                {
                    int j = i;
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t{\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    ////
                    strArg0 = "OnGuide_Name";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_ID";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_Event";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_AxisType";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_RingColorType";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_RingSize";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_RingThick";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_RingDir(1[F],-1[B])";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_3D_Scale";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_3D_Alpha";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_Pos(x/y/z,p/t/s)";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_Off_IDs(Axis...x/y/z/p/t/s)";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "OnGuide_Off_Dir(Axis...x/y/z/p/t/s)";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\"\r\n", strArg0, astrGuide[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    ////
                    if (j < astrGuide.Length - 1) { byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t},\r\n"); }
                    else { byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t}\r\n"); }
                    fs.Write(byteBuffer, 0, byteBuffer.Length);
                }
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t],\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                #endregion Guide_Group

                // GroupName
                #region GroupName
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\"GroupName\" : [\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                nItemCount = 3;
                for (int i = 0; i < astrGroupName.Length; i += nItemCount)
                {
                    int j = i;
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t{\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    ////
                    strArg0 = "Number";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGroupName[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Name";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, astrGroupName[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "IDs";
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\"\r\n", strArg0, astrGroupName[j++])); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    ////
                    if (j < astrGroupName.Length - 1) { byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t},\r\n"); }
                    else { byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t}\r\n"); }
                    fs.Write(byteBuffer, 0, byteBuffer.Length);
                }
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t],\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                #endregion GroupName
                
                // Draw
                #region Draw
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\"Draw\" : [\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                nItemCount = 27;
                string[] astrDrawLines = Ojw.CConvert.RemoveChar(
                                            Ojw.CConvert.RemoveChar(
                                                Ojw.CConvert.RemoveChar(m_C3d.m_CHeader.strDrawModel, '\r'), '['
                                                ), ']'
                                        ).Split('\n');
                List<string> lstDrawLines = new List<string>();
                for (int i = 0; i < astrDrawLines.Length; i++)
                {
                    string strDrawLine = Ojw.CConvert.RemoveChar(
                                            Ojw.CConvert.RemoveChar(
                                                Ojw.CConvert.RemoveCaption(astrDrawLines[i], true, true), '\r'
                                                ), '\n'
                                            );
                    if (strDrawLine.Length < 10) continue;
                    lstDrawLines.Add(strDrawLine);
                }
                List<int> lstIDs_In_3D = new List<int>();
                List<int> lstGroupA_In_3D = new List<int>();
                lstIDs_In_3D.Clear();
                lstGroupA_In_3D.Clear();
                for (int i = 0; i < lstDrawLines.Count; i++)
                {
                    int j = 0;
                    int nID = -1;
                    int nGroupA = -1;
                    string[] astrDraw = lstDrawLines[i].Split(',');
                    if (astrDraw.Length < 10) continue;
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t{\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    ////

                    strArg0 = "Caption"; strArg1 = ((astrDraw[astrDraw.Length - 1].Length > 0) ? astrDraw[astrDraw.Length - 1] : "");
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    
                    strArg0 = "Axis"; strArg1 = astrDraw[j++];
                    nID = Ojw.CConvert.StrToInt(strArg1);

                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Color"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Alpha"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Object"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Fill"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Multi"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Init"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "W/h/d/t/gap"; strArg1 = String.Format("{0},{1},{2},{3},{4}", astrDraw[j + 0], astrDraw[j + 1], astrDraw[j + 2], astrDraw[j + 3], astrDraw[j + 4]); j += 5;
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                                        
                    strArg0 = "AxisMotorType"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Dir"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Angle"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Angle_Offset"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);

                    strArg0 = "Offset(x/y/z,p/t/s)"; strArg1 = String.Format("{0},{1},{2},{3},{4},{5}", astrDraw[j + 0], astrDraw[j + 1], astrDraw[j + 2], astrDraw[j + 3], astrDraw[j + 4], astrDraw[j + 5]); j += 6;
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Move0(x/y/z,p/t/s)"; strArg1 = strArg1 = String.Format("{0},{1},{2},{3},{4},{5}", astrDraw[j + 0], astrDraw[j + 1], astrDraw[j + 2], astrDraw[j + 3], astrDraw[j + 4], astrDraw[j + 5]); j += 6;
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Move1(x/y/z,p/t/s)"; strArg1 = strArg1 = String.Format("{0},{1},{2},{3},{4},{5}", astrDraw[j + 0], astrDraw[j + 1], astrDraw[j + 2], astrDraw[j + 3], astrDraw[j + 4], astrDraw[j + 5]); j += 6;
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Move2(x/y/z,p/t/s)"; strArg1 = String.Format("{0},{1},{2},{3},{4},{5}", astrDraw[j + 0], astrDraw[j + 1], astrDraw[j + 2], astrDraw[j + 3], astrDraw[j + 4], astrDraw[j + 5]); j += 6;
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "MoveReserve0(x/y/z,p/t/s)"; strArg1 = String.Format("{0},{1},{2},{3},{4},{5}", astrDraw[j + 0], astrDraw[j + 1], astrDraw[j + 2], astrDraw[j + 3], astrDraw[j + 4], astrDraw[j + 5]); j += 6;
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "MoveReserve1(x/y/z,p/t/s)"; strArg1 = String.Format("{0},{1},{2},{3},{4},{5}", astrDraw[j + 0], astrDraw[j + 1], astrDraw[j + 2], astrDraw[j + 3], astrDraw[j + 4], astrDraw[j + 5]); j += 6;
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    
                    strArg0 = "Group"; strArg1 = astrDraw[j++]; // GroupA
                    nGroupA = Ojw.CConvert.StrToInt(strArg1);

                    if (nID >= 0)
                    {
                        int nIndex = lstIDs_In_3D.FindIndex(item => item == nID);
                        if (nIndex < 0)
                        {
                            lstIDs_In_3D.Add(nID);
                            lstGroupA_In_3D.Add(nID);
                        }
                        else
                        {
                            if (nGroupA > 0)
                                lstGroupA_In_3D[nIndex] = nGroupA;
                        }
                    }


                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "TargetMotor"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "GroupC"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "FunctionNumber"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Scale_Serve0"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Scale_Serve1"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "MotorType"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "MotorControl_MousePoint"; strArg1 = astrDraw[j++];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\"\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    ////
                    if (i < lstDrawLines.Count - 1) { byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t},\r\n"); }
                    else { byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t}\r\n"); }
                                        
                    fs.Write(byteBuffer, 0, byteBuffer.Length);
                }
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t],\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                #endregion Draw
#if true
                // Motor_Setting
                #region Motor_Setting
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\"Motor_Setting\" : [\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                nItemCount = 13;
                List<int> lstIDs = new List<int>();
                lstIDs.Clear();
                for (int j = 0; j < lstIDs_In_3D.Count; j++)
                {
                    int i = lstIDs_In_3D[j];
                    // 0: Dontcare, 1: Enable, -1: Disable => 이게 Disable 이면 모터 표시를 죽인다.
                    if (m_C3d.m_CHeader.pSMotorInfo[i].nMotor_Enable < 0) continue;
                    lstIDs.Add(i);
                }
                lstIDs.Sort();
                for (int j = 0; j < lstIDs.Count; j++)
                {
                    int i = lstIDs[j];
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t{\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    ////
                    strArg0 = "ID"; strArg1 = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[i].nMotorID);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Mirror"; strArg1 = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[i].nAxis_Mirror);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "LimitDn"; strArg1 = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[i].fLimit_Down);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "LimitUp"; strArg1 = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[i].fLimit_Up);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "InitAngle"; strArg1 = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "InitAngle2"; strArg1 = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle2);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Mech_Center"; strArg1 = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[i].nCenter_Evd);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Mech_Move"; strArg1 = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[i].nMechMove);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Mech_Angle"; strArg1 = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[i].fMechAngle);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    /*float fMulti = (float)((m_C3d.m_CHeader.pSMotorInfo[i].fGearRatio != 0) ? m_C3d.m_CHeader.pSMotorInfo[i].fGearRatio : 1.0f) *
                        (float)((m_C3d.m_CHeader.pSMotorInfo[i].nMotorDir == 0) ? 1f : -1f);*/
                    float fMulti = 
                        (float)((m_C3d.m_CHeader.pSMotorInfo[i].fGearRatio != 0) ? m_C3d.m_CHeader.pSMotorInfo[i].fGearRatio : 1.0f) *
                        (float)((m_C3d.m_CHeader.pSMotorInfo[i].nMotorDir == 0) ? 1 : -1);
                    strArg0 = "Multi"; strArg1 = Ojw.CConvert.FloatToStr(fMulti);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "ControlType[0-p,1-v]"; strArg1 = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[i].nMotorControlType);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    strArg0 = "Hw"; strArg1 = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[i].nHwMotor_Key);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\",\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);

                    int nIndex = m_C3d.m_CHeader.pSMotorInfo[i].nGroupNumber;// lstIDs_In_3D.FindIndex(item => item == m_C3d.m_CHeader.pSMotorInfo[i].nMotorID);
                    //int nIndex = lstIDs_In_3D.FindIndex(item => item == m_C3d.m_CHeader.pSMotorInfo[i].nMotorID);
                    int nVal = ((nIndex >= 0) ? nIndex : 0);
                    strArg0 = "GroupA"; strArg1 = Ojw.CConvert.IntToStr(nVal);
                    byteBuffer = Ojw.CConvert.StrToBytes_UTF8(String.Format("\t\t\t\"{0}\" : \"{1}\"\r\n", strArg0, strArg1)); fs.Write(byteBuffer, 0, byteBuffer.Length);
                    ////
                    if (j < lstIDs.Count - 1) { byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t},\r\n"); }
                    else { byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t\t}\r\n"); }
                    fs.Write(byteBuffer, 0, byteBuffer.Length);
                }
                //byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t],\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8("\t]\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                #endregion Motor_Setting
#endif

                // End
                byteBuffer = Ojw.CConvert.StrToBytes_UTF8("}\r\n"); fs.Write(byteBuffer, 0, byteBuffer.Length);
                
                fs.Close(); f = null; 
                return true;
            }
            catch
            {
                //Message("파일 저장 에러");
                fs.Close(); f = null; 
                return false;
            }
            #endregion Json File
        }
    }
}
