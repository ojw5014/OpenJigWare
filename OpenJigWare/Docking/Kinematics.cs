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
    public partial class frmKinematics : Form
    {
        public frmKinematics()
        {
            InitializeComponent();
        }

        private List<int> m_lstMotor_Key_for_dic = new List<int>();
        private List<string> m_lstMotor_Value_for_dic = new List<string>();
        private void frmKinematics_Load(object sender, EventArgs e)
        {
            txtDHColor.Text = Ojw.CConvert.IntToStr(Color.Red.ToArgb());
            cmbDh.Items.Clear();
            for (int j = 0; j < 512; j++)
            {
                cmbDh.Items.Add(j.ToString());
            }
            cmbDH_Test_Index.Items.Clear();
            for (int j = 0; j < 512; j++)
            {
                cmbDH_Test_Index.Items.Add(j.ToString());
            }
            cmbDH_Test_Index.SelectedIndex = 0;

            cmbDh.SelectedIndex = 0;
            cmbDH_Test_Index.SelectedIndex = 0;

            cmbMotorName.Items.Clear();
            Ojw.CMonster2 CMon = new Ojw.CMonster2();
            //m_lstMotor_Key_for_dic.Clear();
            //m_lstMotor_Value_for_dic.Clear();

            foreach (KeyValuePair<int, string> Item in CMon.dicMonster)
            {
                cmbMotorName.Items.Add(Item.Value);
                //Ojw.printf("{0}\r\n", (string)Ojw.CConvert.Dictionary_GetKey((Dictionary<object, object>)CMon.dicMonster, (object)Item.Value));
            }

            //m_C3d.Event_FileOpen.UserEvent -= new EventHandler(FileOpen);
            m_C3d.Event_FileOpen.UserEvent += new EventHandler(FileOpen);

            InitTestDrawing();

            this.Left = frmDesigner.m_frm3D.Right;// +5;//Screen.AllScreens[0].Bounds.Right - this.Width;
            this.Top = 0;

            //Array.Clear(m_IsMotors, 0, m_IsMotors.Length);

            //m_IsMotors.Clear();
            //m_lstSMotors.Clear();
            lstboxMotorIndex.Items.Clear();
            for (int i = 0; i < 254; i++)
            {
                //m_lstSMotors.Add(new SMotorInfo_t());
                //m_lstIsMotors.Add(false);
                m_IsMotors[i] = false;
                lstboxMotorIndex.Items.Add(i.ToString());
            }
            for (int i = 0; i < 254; i++)
                DisplayIndex(i);
        }
        private void InitTestDrawing()
        {
            CDhParam CDh = new CDhParam();
            CDh.InitData();
            txtDH_Param_A.Text = Ojw.CConvert.DoubleToStr(CDh.dA);
            txtDH_Param_D.Text = Ojw.CConvert.DoubleToStr(CDh.dD);
            txtDH_Param_Theta.Text = Ojw.CConvert.DoubleToStr(CDh.dTheta);
            txtDH_Param_Alpha.Text = Ojw.CConvert.DoubleToStr(CDh.dAlpha);
            txtDH_Param_Axis.Text = Ojw.CConvert.DoubleToStr(CDh.nAxisNum);

            txtAdd_StlFile.Text = "";
            txtAdd_Alpha.Text = "1.0";
            txtAdd_Color.Text = "-1";
            txtAdd_Motor.Text = "-1";
            txtAdd_X.Text = "0";
            txtAdd_Y.Text = "0";
            txtAdd_Z.Text = "0";
            txtAdd_Pan.Text = "0";
            txtAdd_Tilt.Text = "0";
            txtAdd_Swing.Text = "0";
            cmbAdd_Motor.SelectedIndex = 0;
            cmbDH_Init.SelectedIndex = 0;
            cmdDH_Param_Dir.SelectedIndex = 0;
        }
        private void FileOpen(object sender, EventArgs e)
        {
            // FileOpen 후에 생기는 이벤트 처리

            cmbDh.SelectedIndex = 0;
            DhRefresh();
        }
        private void cmbDH_Init_SelectedIndexChanged(object sender, EventArgs e)
        {
            frmDesigner.m_COjwDhParam.nInit = cmbDH_Init.SelectedIndex;
        }
        public void DhRefresh()
        {
            cmbDhRefresh(cmbDh.SelectedIndex);
        }
        private void cmbDh_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if ((cmbDh.Focused == true) && (cmbDh.SelectedIndex >= 0))
            //{
            if (cmbDh.SelectedIndex >= 0) cmbDhRefresh(cmbDh.SelectedIndex);
            //}
        }
        private Ojw.C3d m_C3d { get { return frmDesigner.m_C3d; } set { frmDesigner.m_C3d = value; } }

        private void cmbDhRefresh(int nNum)
        {
#if true
            if ((nNum >= 0) && (nNum < cmbDh.Items.Count))
            {
                Ojw.CEncryption.SetEncrypt("OJW5014"); // 암호화 해제는 보안이 필요
                txtDH_Tab_Forward.Text = Encoding.Default.GetString(Ojw.CEncryption.Encryption(false, m_C3d.GetHeader_pSEncryptKinematics_encryption()[nNum].byteEncryption));

                Ojw.CEncryption.SetEncrypt("OJW5014"); // 암호화 해제는 보안이 필요
                txtDH_Tab_Inverse.Text = Encoding.Default.GetString(Ojw.CEncryption.Encryption(false, m_C3d.GetHeader_pSEncryptInverseKinematics_encryption()[nNum].byteEncryption));

                // Function Name(Caption) : 예전이름 m_txtGroupName
                txtDH_Caption.Text = m_C3d.GetHeader_pstrGroupName()[nNum];
                // 수식 번호(인덱스) : m_cmbDh
                cmbDh.SelectedIndex = nNum;

                // Encryption : m_cmbSecret
                cmbDH_Encryption.SelectedIndex = m_C3d.GetHeader_pnSecret()[nNum];
                // Function Type : m_cmbKinematicsType
                cmbDH_FunctionType.SelectedIndex = m_C3d.GetHeader_pnType()[nNum];
                // 파이썬 : m_cmbPython
                cmbDH_Python.SelectedIndex = Ojw.CConvert.BoolToInt(m_C3d.GetHeader_pbPython()[nNum]);

                // 새로 추가되는 것들을 넣는다.
                txtDH_Offset_Trans_X.Text = Ojw.CConvert.FloatToStr(m_C3d.GetHeader().pSOffset_Trans[nNum].x);
                txtDH_Offset_Trans_Y.Text = Ojw.CConvert.FloatToStr(m_C3d.GetHeader().pSOffset_Trans[nNum].y);
                txtDH_Offset_Trans_Z.Text = Ojw.CConvert.FloatToStr(m_C3d.GetHeader().pSOffset_Trans[nNum].z);

                // Roll(x), Pitch(y), Yaw(z) 의 개념은 x 축이 전방을 향할때이므로 Z 축이 전방을 바라보는 오픈지그웨어와는 맞지 않는다. 그렇게 하지 않고 여기서는 Pan(도리각(도리도리)), Tilt(끄덕각), Swing(갸웃각)으로 정의
                txtDH_Offset_Rot_Pan.Text = Ojw.CConvert.FloatToStr(m_C3d.GetHeader().pSOffset_Rot[nNum].x);
                txtDH_Offset_Rot_Tilt.Text = Ojw.CConvert.FloatToStr(m_C3d.GetHeader().pSOffset_Rot[nNum].y);
                txtDH_Offset_Rot_Swing.Text = Ojw.CConvert.FloatToStr(m_C3d.GetHeader().pSOffset_Rot[nNum].z);
            }
#endif
        }

        private void cmbDH_Python_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((cmbDH_Python.Focused == true) && (cmbDh.SelectedIndex >= 0))
            {
                int nNum = cmbDh.SelectedIndex;
                m_C3d.GetHeader_pbPython()[nNum] = Ojw.CConvert.IntToBool(cmbDH_Python.SelectedIndex);
            }
        }

        private void txtDH_Caption_TextChanged(object sender, EventArgs e)
        {
            if ((txtDH_Caption.Focused == true) && (cmbDh.SelectedIndex >= 0))
            {
                int nNum = cmbDh.SelectedIndex;
                m_C3d.GetHeader_pstrGroupName()[nNum] = txtDH_Caption.Text;
            }
        }

        private void cmbDH_Encryption_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((cmbDH_Encryption.Focused == true) && (cmbDh.SelectedIndex >= 0))
            {
                int nNum = cmbDh.SelectedIndex;
                m_C3d.GetHeader_pnSecret()[nNum] = cmbDH_Encryption.SelectedIndex;
            }
        }

        private void cmbDH_FunctionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((cmbDH_FunctionType.Focused == true) && (cmbDh.SelectedIndex >= 0))
            {
                int nNum = cmbDh.SelectedIndex;
                m_C3d.GetHeader_pnType()[nNum] = cmbDH_FunctionType.SelectedIndex;
            }
        }

        private void txtDH_Offset_Trans_X_TextChanged(object sender, EventArgs e)
        {
            if ((txtDH_Offset_Trans_X.Focused == true) && (cmbDh.SelectedIndex >= 0))
            {
                int nNum = cmbDh.SelectedIndex;
                m_C3d.GetHeader().pSOffset_Trans[nNum].x = Ojw.CConvert.StrToFloat(txtDH_Offset_Trans_X.Text);
            }
        }

        private void txtDH_Offset_Trans_Y_TextChanged(object sender, EventArgs e)
        {
            if ((txtDH_Offset_Trans_Y.Focused == true) && (cmbDh.SelectedIndex >= 0))
            {
                int nNum = cmbDh.SelectedIndex;
                m_C3d.GetHeader().pSOffset_Trans[nNum].y = Ojw.CConvert.StrToFloat(txtDH_Offset_Trans_Y.Text);
            }
        }

        private void txtDH_Offset_Trans_Z_TextChanged(object sender, EventArgs e)
        {
            if ((txtDH_Offset_Trans_Z.Focused == true) && (cmbDh.SelectedIndex >= 0))
            {
                int nNum = cmbDh.SelectedIndex;
                m_C3d.GetHeader().pSOffset_Trans[nNum].z = Ojw.CConvert.StrToFloat(txtDH_Offset_Trans_Z.Text);
            }
        }

        private void txtDH_Offset_Rot_Pan_TextChanged(object sender, EventArgs e)
        {
            if ((txtDH_Offset_Rot_Pan.Focused == true) && (cmbDh.SelectedIndex >= 0))
            {
                int nNum = cmbDh.SelectedIndex;
                m_C3d.GetHeader().pSOffset_Rot[nNum].x = Ojw.CConvert.StrToFloat(txtDH_Offset_Rot_Pan.Text);
            }
        }

        private void txtDH_Offset_Rot_Tilt_TextChanged(object sender, EventArgs e)
        {
            if ((txtDH_Offset_Rot_Tilt.Focused == true) && (cmbDh.SelectedIndex >= 0))
            {
                int nNum = cmbDh.SelectedIndex;
                m_C3d.GetHeader().pSOffset_Rot[nNum].y = Ojw.CConvert.StrToFloat(txtDH_Offset_Rot_Tilt.Text);
            }
        }

        private void txtDH_Offset_Rot_Swing_TextChanged(object sender, EventArgs e)
        {
            if ((txtDH_Offset_Rot_Swing.Focused == true) && (cmbDh.SelectedIndex >= 0))
            {
                int nNum = cmbDh.SelectedIndex;
                m_C3d.GetHeader().pSOffset_Rot[nNum].z = Ojw.CConvert.StrToFloat(txtDH_Offset_Rot_Swing.Text);
            }
        }

        private void btnCompile_Click(object sender, EventArgs e)
        {
            m_C3d.CheckForward();
            m_C3d.CheckInverse();
        }

        private void txtDH_Tab_Forward_TextChanged(object sender, EventArgs e)
        {
            int nNum = cmbDh.SelectedIndex;
            if (txtDH_Tab_Forward.Focused == true)
            {
                m_C3d.GetHeader_pstrKinematics()[nNum] = txtDH_Tab_Forward.Text;

                byte[] byteData = Encoding.Default.GetBytes(txtDH_Tab_Forward.Text);
                m_C3d.GetHeader_pSEncryptKinematics_encryption()[nNum].byteEncryption = Ojw.CEncryption.Encryption(true, byteData);
                byteData = null;
            }
        }

        private void txtDH_Tab_Inverse_TextChanged(object sender, EventArgs e)
        {
            int nNum = cmbDh.SelectedIndex;
            if (txtDH_Tab_Inverse.Focused == true)
            {
                m_C3d.GetHeader_pstrInverseKinematics()[nNum] = txtDH_Tab_Inverse.Text;

                byte[] byteData = Encoding.Default.GetBytes(txtDH_Tab_Inverse.Text);
                m_C3d.GetHeader_pSEncryptInverseKinematics_encryption()[nNum].byteEncryption = Ojw.CEncryption.Encryption(true, byteData);
                byteData = null;
            }
        }

        private void chkDH_View_Skeleton_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDH_View_Skeleton.Focused == true)
            {
                m_C3d.SetSkeletonView(chkDH_View_Skeleton.Checked);
                if (chkDH_View_Skeleton.Checked == true)
                    MakeDHSkeleton(true, Ojw.CConvert.StrToFloat(txtDHSize.Text), Color.FromArgb(Ojw.CConvert.StrToInt(txtDHColor.Text)), txtDh.Text,
                        Ojw.CConvert.StrToInt(txtDH_StartGroup.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_X.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_Y.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_Z.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Rot_Pan.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Rot_Tilt.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Rot_Swing.Text)
                        );
            }
        }

        private bool m_bVisibleSkeleton = false;
#if false
        private String MakeDHSkeleton(
            float fSize, Color cColor, 
            string strString, 
            int nStartGroupNumber,
            float fOffset_X, float fOffset_Y, float fOffset_Z,
            float fOffset_Pan, float fOffset_Tilt, float fOffset_Swing)
        {
            String strModel_Bar = (m_bVisibleSkeleton == true) ? "#8" : "#19";
            String strModel_Ball = (m_bVisibleSkeleton == true) ? "#9" : "#20";
            String strData = String.Empty;
            String strDisp = String.Empty;
            //SetHeader_strDrawModel(String.Empty);
            String strConvert0 = Ojw.CConvert.RemoveCaption(strString, true, true);
            String strConvert1 = Ojw.CConvert.RemoveChar(strConvert0, '[');
            String strConvert2 = Ojw.CConvert.RemoveChar(strConvert1, ']');
            String strConvert3 = Ojw.CConvert.RemoveChar(strConvert2, '\r');
            String[] pstrLines = strConvert3.Split('\n');

            m_C3d.User_Clear();

            m_C3d.User_Set_Init(true); // 초기점
            m_C3d.User_Set_Translation(0, fOffset_X, fOffset_Y, fOffset_Z);
            m_C3d.User_Set_Rotation(0, fOffset_Pan, fOffset_Tilt, fOffset_Z);

            foreach (string strLine in pstrLines)
            {
                float fA = 0.0f;
                float fD = 0.0f;
                float fTheta = 0.0f;
                float fAlpha = 0.0f;
                int nAxis = -1;
                int nDir = 0;
                int nInit = 0;

        #region Items
                String[] pstrItems = strLine.Split(',');
                int i = 0;
                foreach (string strItem in pstrItems)
                {
                    try
                    {
                        switch (i)
                        {
                            case 0: fA = Ojw.CConvert.StrToFloat(strItem); break;
                            case 1: fD = Ojw.CConvert.StrToFloat(strItem); break;
                            case 2: fTheta = Ojw.CConvert.StrToFloat(strItem); break;
                            case 3: fAlpha = Ojw.CConvert.StrToFloat(strItem); break;
                            case 4: nAxis = Ojw.CConvert.StrToInt(strItem); break;
                            case 5: nDir = Ojw.CConvert.StrToInt(strItem); break;
                            case 6: nInit = Ojw.CConvert.StrToInt(strItem); break;
                        }
                    }
                    catch
                    {
                        switch (i)
                        {
                            case 0: fA = 0.0f; break;
                            case 1: fD = 0.0f; break;
                            case 2: fTheta = 0.0f; break;
                            case 3: fAlpha = 0.0f; break;
                            case 4: nAxis = 0; break;
                            case 5: nDir = 0; break;
                            case 6: nInit = 0; break;
                        }
                    }
                    i++;
                }
                //if (i != 6) return null; // 해석 에러
                //if (i != 7) continue; // 해석 에러 => 굳이 초기화 없이도 해석 되게...
                #endregion Items

                //Prop_Set_DispObject(strModel_Ball);
                //Prop_Set_Width_Or_Radius(fSize);
                //AddVirtualClassToReal();
                if (nInit > 0)
                {
                    m_C3d.User_Set_Init(true);
                }
                m_C3d.User_Set_Model(strModel_Ball);
                m_C3d.User_Set_Color(cColor);
                m_C3d.User_Set_Width_Or_Radius(fSize);
                m_C3d.User_Add();


                //Prop_Set_DispObject(strModel_Bar);
                m_C3d.User_Set_Model(strModel_Bar);
                m_C3d.User_Set_Color(cColor);
                m_C3d.User_Set_Width_Or_Radius(fSize);
                if (fD < 0)
                {
                    ////Prop_Set_Offset_Trans(new Ojw.SVector3D_t(fA, 0, 0));
                    //Prop_Set_Offset_Rot(new Ojw.SAngle3D_t(180, 0, 0));
                    //Prop_Set_Width_Or_Radius(fSize);
                    //Prop_Set_Height_Or_Depth(-fD);
                    m_C3d.User_Set_Offset_Rotation(180, 0, 0);
                    m_C3d.User_Set_Height_Or_Depth(-fD);
                }
                else m_C3d.User_Set_Height_Or_Depth(fD);
                m_C3d.User_Add();


                //Prop_Set_DispObject(strModel_Ball);
                //Prop_Set_Width_Or_Radius(fSize);
                //Prop_Set_Offset_Trans(new Ojw.SVector3D_t(0, 0, fD));
                m_C3d.User_Set_Model(strModel_Ball);
                m_C3d.User_Set_Color(cColor);
                m_C3d.User_Set_Width_Or_Radius(fSize);
                m_C3d.User_Set_Offset_Translation(0, 0, fD);

                m_C3d.User_Set_AxisName(nAxis);

                // [0 ~ 2(Pan, Tilt, Swing), 3~5(x,y,z), 6(cw), 7(ccw)]
                if ((nDir == 0) || (nDir == 1)) m_C3d.User_Set_AxisMoveType(2);
                else if ((nDir == 2) || (nDir == 3)) m_C3d.User_Set_AxisMoveType(5);
                if ((nDir == 0) || (nDir == 2)) m_C3d.User_Set_Dir(0);
                else if ((nDir == 1) || (nDir == 3)) m_C3d.User_Set_Dir(1);

                m_C3d.User_Add();

                if ((nDir == 2) || (nDir == 3))
                {
                    m_C3d.User_Set_Model(strModel_Bar);
                    m_C3d.User_Set_Color(Color.FromArgb(cColor.R / 2, cColor.G / 2, cColor.B / 2));
                    m_C3d.User_Set_Width_Or_Radius(fSize);

                    if (nDir == 2) m_C3d.User_Set_Offset_Rotation(180, 0, 0);

                    m_C3d.User_Set_Height_Or_Depth(m_C3d.GetData(nAxis));

                    m_C3d.User_Add();
                }

                m_C3d.User_Set_Model(strModel_Bar);
                m_C3d.User_Set_Color(cColor);
                if (fA < 0)
                {
                    m_C3d.User_Set_Offset_Translation(0, 0, fD);
                    m_C3d.User_Set_Offset_Rotation(-90, fTheta, 0);
                    m_C3d.User_Set_Width_Or_Radius(fSize);
                    m_C3d.User_Set_Height_Or_Depth(-fA);
                }
                else
                {
                    m_C3d.User_Set_Offset_Translation(0, 0, fD);
                    m_C3d.User_Set_Offset_Rotation(90, -fTheta, 0);
                    m_C3d.User_Set_Width_Or_Radius(fSize);
                    m_C3d.User_Set_Height_Or_Depth(fA);
                }
                m_C3d.User_Add();

                m_C3d.User_Set_Model(strModel_Ball);
                m_C3d.User_Set_Color(cColor);

                m_C3d.User_Set_Width_Or_Radius(fSize);
                int nIndex = 0;
                m_C3d.User_Set_Translation(nIndex, 0, 0, fD);
                m_C3d.User_Set_Rotation(nIndex, 0, 0, fTheta);
                nIndex++;
                m_C3d.User_Set_Translation(nIndex, fA, 0, 0);
                m_C3d.User_Set_Rotation(nIndex, 0, fAlpha, 0);

                m_C3d.User_Add();





                //strDisp = GetHeader_strDrawModel();

            }
        #region Skeleton
            StringBuilder sbResult = new StringBuilder();
#if _USING_DOTNET_3_5
                sbResult.Remove(0, sbResult.Length);
#elif _USING_DOTNET_2_0
                sbResult.Remove(0, sbResult.Length);
#else
            sbResult.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
            int nGroupNum = nStartGroupNumber;
            int nMotorNum = -1;
            int nCnt = m_C3d.User_GetCnt();
            Color[] acColor = new Color[7] { Color.Orange, Color.Cyan, Color.Blue, Color.Lime, Color.Yellow, Color.LightGreen, Color.Magenta };
            for (int i = 0; i < nCnt; i++) // Except 3 Directions
            {
                String strResult = String.Empty;
                if (m_C3d.User_Get_AxisName() >= 0)
                {
                    nGroupNum++;
                    nMotorNum = m_C3d.User_Get_AxisName();
                    sbResult.Append(String.Format("// Group = {0}, Motor = {1} //////////////////////////\r\n", nGroupNum, nMotorNum));
                }
        #region
                Ojw.C3d.COjwDisp CDisp = m_C3d.User_Get(i);
                CDisp.SetData_Color(acColor[nGroupNum % acColor.Length]);
                CDisp.SetData_nPickGroup_A(nGroupNum);
                CDisp.SetData_nPickGroup_B(nMotorNum);
                //User_Set(i, CDisp);
                #endregion
                //Convert_CDisp_To_String(User_Get(i), ref strResult);
                m_C3d.Convert_CDisp_To_String(CDisp, ref strResult);
                //if (i >= nCnt - 3) continue;
                sbResult.Append(strResult);
            }
            txtDH_Tab_Skeleton.Text = sbResult.ToString();
            //strResult = Ojw.CConvert.RemoveString(strResult, "\r\n");
            //AddHeader_strDrawModel(strResult);
            //CompileDesign();
            #endregion Skeleton

            //////////////// Direction
            float fMulti = fSize / 10.0f * 2.0f;
            float fLength = 30 * fMulti;

            m_C3d.User_Set_Model(strModel_Ball);
            m_C3d.User_Set_Color(Color.Red);
            m_C3d.User_Set_Width_Or_Radius(fSize);
            m_C3d.User_Set_Offset_Translation(fLength, 0, 0);
            m_C3d.User_Add();

            m_C3d.User_Set_Model(strModel_Ball);
            m_C3d.User_Set_Color(Color.Green);
            m_C3d.User_Set_Width_Or_Radius(fSize);
            m_C3d.User_Set_Offset_Translation(0, fLength, 0);
            m_C3d.User_Add();

            m_C3d.User_Set_Model(strModel_Ball);
            m_C3d.User_Set_Color(Color.Blue);
            m_C3d.User_Set_Width_Or_Radius(fSize);
            m_C3d.User_Set_Offset_Translation(0, 0, fLength);
            m_C3d.User_Add();
            //////////////////////////

            return sbResult.ToString(); // null;// strDisp;
        }
#endif
        private void chkDH_Test_Show_CheckedChanged(object sender, EventArgs e)
        {
            int nNum = Ojw.CConvert.StrToInt(txtDH_Tab_Inverse.Text);
            float fX = Ojw.CConvert.StrToFloat(txtDH_Test_X.Text);
            float fY = Ojw.CConvert.StrToFloat(txtDH_Test_Y.Text);
            float fZ = Ojw.CConvert.StrToFloat(txtDH_Test_Z.Text);

            // 테스트 시작
            m_C3d.SetTestCircle(chkDH_Test_Show.Checked);
            //Settes
            m_C3d.SetColor_Test(Color.Red);
            // 테스트 값 입력
            m_C3d.SetSize_Test(Ojw.CConvert.StrToFloat(txtDH_Test_BallSize.Text));
            m_C3d.SetPos_Test(fX, fY, fZ);


            m_C3d.SetTestCircle(chkDH_Test_Show.Checked);
        }

        private void chkDH_View_DHObject_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDH_View_DHObject.Focused == true)
            {
                m_C3d.SetTestDh(chkDH_View_DHObject.Checked);
            }
        }

        private void btnDH_Test_Get_Forward_Click(object sender, EventArgs e)
        {
            if (txtDepthIndex.Text.Length == 0) txtDepthIndex.Text = "-1";
            int nDepthIndex = Ojw.CConvert.StrToInt(txtDepthIndex.Text);
            if (nDepthIndex >= 0) Ojw.CKinematics.CForward.SetCalcLimit(nDepthIndex);

            int nNum = Ojw.CConvert.StrToInt(cmbDH_Test_Index.Text);

            int i;
            //CDhParamAll COjwDhParamAll = new CDhParamAll();
            //Ojw.CKinematics.CForward.MakeDhParam(m_CHeader.pstrKinematics[nNum], out COjwDhParamAll);

            double dX, dY, dZ;
            double[] dcolX;
            double[] dcolY;
            double[] dcolZ;

            double[] adMot = new double[256];
            Array.Clear(adMot, 0, adMot.Length);
            for (i = 0; i < m_C3d.GetHeader_nMotorCnt(); i++) adMot[i] = (double)m_C3d.GetData(i);
            Ojw.CKinematics.CForward.CalcKinematics(m_C3d.m_CHeader.pDhParamAll[nNum], adMot, out dcolX, out dcolY, out dcolZ, out dX, out dY, out dZ);

            txtDH_Test_X.Text = Ojw.CConvert.DoubleToStr(dX);
            txtDH_Test_Y.Text = Ojw.CConvert.DoubleToStr(dY);
            txtDH_Test_Z.Text = Ojw.CConvert.DoubleToStr(dZ);

            // 테스트 시작
            m_C3d.SetTestCircle(chkDH_Test_Show.Checked);
            //Settes
            m_C3d.SetColor_Test(Color.Red);
            // 테스트 값 입력
            m_C3d.SetSize_Test(Ojw.CConvert.StrToFloat(txtDH_Test_BallSize.Text));
            m_C3d.SetPos_Test((float)dX, (float)dY, (float)dZ);

            m_C3d.SetTestCircle(chkDH_Test_Show.Checked);

            //txtDepthIndex.Text = "-1";
        }

        private void btnDH_Test_Go_Inverse_Click(object sender, EventArgs e)
        {
            int nNum = Ojw.CConvert.StrToInt(cmbDH_Test_Index.Text);
            float fX = Ojw.CConvert.StrToFloat(txtDH_Test_X.Text);
            float fY = Ojw.CConvert.StrToFloat(txtDH_Test_Y.Text);
            float fZ = Ojw.CConvert.StrToFloat(txtDH_Test_Z.Text);

            // 집어넣기 전에 내부 메모리를 클리어 한다.
            Ojw.CKinematics.CInverse.SetValue_ClearAll(ref m_C3d.GetHeader_pSOjwCode()[nNum]);
            Ojw.CKinematics.CInverse.SetValue_X(fX);
            Ojw.CKinematics.CInverse.SetValue_Y(fY);
            Ojw.CKinematics.CInverse.SetValue_Z(fZ);


            // 테스트 시작
            m_C3d.SetTestCircle(chkDH_Test_Show.Checked);
            //Settes
            m_C3d.SetColor_Test(Color.Red);
            // 테스트 값 입력
            m_C3d.SetSize_Test(Ojw.CConvert.StrToFloat(txtDH_Test_BallSize.Text));
            m_C3d.Rotation(m_C3d.GetHeader().pSOffset_Rot[nNum].y, m_C3d.GetHeader().pSOffset_Rot[nNum].x, m_C3d.GetHeader().pSOffset_Rot[nNum].z, ref fX, ref fY, ref fZ);
            m_C3d.SetPos_Test(
                fX + m_C3d.GetHeader().pSOffset_Trans[nNum].x,//Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_X.Text),
                fY + m_C3d.GetHeader().pSOffset_Trans[nNum].y,//Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_Y.Text),
                fZ + m_C3d.GetHeader().pSOffset_Trans[nNum].z//Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_Z.Text)
                );

            // 현재의 모터각을 전부 집어 넣도록 한다.
            //UpdateMotorCommand();
            for (int i = 0; i < 256; i++)
            {
                // 모터값을 3D에 넣어주고
                //SetData(i, Ojw.CConvert.StrToFloat(m_txtAngle[i].Text));
                // 그 값을 꺼내 수식 계산에 넣어준다.
                Ojw.CKinematics.CInverse.SetValue_Motor(i, m_C3d.GetData(i));
            }

            // 실제 수식계산
            if (Ojw.CKinematics.CInverse.CalcCode(ref m_C3d.GetHeader_pSOjwCode()[nNum]) == false) MessageBox.Show(String.Format("Compile Error - {0}", Ojw.CKinematics.CInverse.GetErrorString_Error_Etc()));

            txtForwardKinematics_Message.Clear();
            txtInverseKinematics_Message.Clear();
            //m_lbV.Text = String.Empty;
            for (int i = 0; i < 10; i++)
            {
                Ojw.CMessage.Write("V" + i.ToString() + ":" + Ojw.CConvert.DoubleToStr(Ojw.CKinematics.CInverse.GetValue_V(i)));
                //m_lbV.Text += "V" + i.ToString() + ":" + Ojw.CConvert.DoubleToStr(Ojw.CKinematics.CInverse.GetValue_V(i)) + ",";
                Ojw.CMessage.Write(txtForwardKinematics_Message, "V" + i.ToString() + ":" + Ojw.CConvert.DoubleToStr(Ojw.CKinematics.CInverse.GetValue_V(i)));
                Ojw.CMessage.Write(txtInverseKinematics_Message, "V" + i.ToString() + ":" + Ojw.CConvert.DoubleToStr(Ojw.CKinematics.CInverse.GetValue_V(i)));
            }
            // 나온 결과값을 옮긴다.
            int nMotCnt = m_C3d.GetHeader_pSOjwCode()[nNum].nMotor_Max;
            for (int i = 0; i < nMotCnt; i++)
            {
                int nMotNum = m_C3d.GetHeader_pSOjwCode()[nNum].pnMotor_Number[i];
                m_C3d.SetData(nMotNum, (float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum));

                frmDesigner.m_atxtAngle[nMotNum].Text = Ojw.CConvert.FloatToStr((float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum));
                Ojw.CMessage.Write(txtForwardKinematics_Message, "T" + nMotNum.ToString() + ":" + frmDesigner.m_atxtAngle[nMotNum].Text);
                Ojw.CMessage.Write(txtInverseKinematics_Message, "T" + nMotNum.ToString() + ":" + frmDesigner.m_atxtAngle[nMotNum].Text);
            }
            //BlockUpdate(true);
            //for (int i = 0; i < 256; i++)
            //{
            //    m_txtAngle[i].Text = Ojw.CConvert.FloatToStr(GetData(i));
            //}
            //BlockUpdate(false);
        }

        private void btnDH_MoveToPoint_Visible_Click(object sender, EventArgs e)
        {
            chkMakeVisibleSkeleton.Checked = true;
            float fSize = Ojw.CConvert.StrToFloat(txtDHSize.Text);
            MakeDHSkeleton(true, fSize, Color.Violet, txtDh.Text);
            MakeString_For_ForwardKinematics();
            //m_C3d.User_Clear();

            //m_C3d.SetHeader_strDrawModel(txtDH_Tab_Skeleton.Text);
            //m_C3d.CompileDesign();
        }
        private void MakeString_For_ForwardKinematics()
        {
            //double dX, dY, dZ;
            //double[] colX;
            //double[] colY;
            //double[] colZ;
            double[] adAngle = Array.ConvertAll(m_C3d.GetData(), element => (double)element);
            //Ojw.CKinematics.CForward.CalcKinematics_ToString(m_CDhAll, adAngle, out colX, out colY, out colZ, out dX, out dY, out dZ);
            String strResult;
            Ojw.CKinematics.CForward.CalcKinematics_ToString(m_CDhAll, adAngle, out strResult);

            //txtError.Text = strResult;
            txtDH_Tab_Result.Text = strResult;
        }

        private void btnDH_MoveToPoint_InVisible_Click(object sender, EventArgs e)
        {
            chkMakeVisibleSkeleton.Checked = false;
            float fSize = Ojw.CConvert.StrToFloat(txtDHSize.Text);
            MakeDHSkeleton(false, fSize, Color.Violet, txtDh.Text);
            MakeString_For_ForwardKinematics();
        }

        private void txtDHSize_TextChanged(object sender, EventArgs e)
        {
            m_C3d.SetTestDh_Size(Ojw.CConvert.StrToFloat(txtDHSize.Text));
        }

        private void txtDHColor_TextChanged(object sender, EventArgs e)
        {
            m_C3d.SetTestDh_Color(Color.FromArgb(Ojw.CConvert.StrToInt(txtDHColor.Text)));
        }

        private void txtDH_Test_BallSize_TextChanged(object sender, EventArgs e)
        {
            // 테스트 값 입력
            m_C3d.SetSize_Test(Ojw.CConvert.StrToFloat(txtDH_Test_BallSize.Text));
        }

        private void txtDHAlpha_TextChanged(object sender, EventArgs e)
        {
            m_C3d.SetTestDh_Alpha(Ojw.CConvert.StrToFloat(txtDHAlpha.Text));
        }

        private void btnDHColor_Click(object sender, EventArgs e)
        {
            ColorDialog cdDlg = new ColorDialog();
            cdDlg.AllowFullOpen = true;
            cdDlg.ShowHelp = true;

            int nColor = Ojw.CConvert.StrToInt(txtDHColor.Text);
            Color cColor = Color.FromArgb(nColor);

            cdDlg.Color = cColor;
            if (cdDlg.ShowDialog() == DialogResult.OK)
                cColor = cdDlg.Color;
            txtDHColor.Text = Ojw.CConvert.IntToStr(cColor.ToArgb());
            cdDlg.Dispose();
        }
        private CDhParamAll m_CDhAll = new CDhParamAll();
        private void AddDevice(CDhParam CDh)
        {
            int nAxisNum = CDh.nAxisNum;
            ////CDh.nAxisNum = nAxisNum;
            if (nAxisNum >= 0) CDh.strCaption += " - Axis" + Ojw.CConvert.IntToStr(nAxisNum);
            String strData = Ojw.CKinematics.CForward.ClassToString_DHParam(CDh);

            txtDh.Text += strData + "\r\n";

            Ojw.CKinematics.CForward.MakeDhParam(txtDh.Text, out m_CDhAll);
            double dX, dY, dZ;
            double[] colX;
            double[] colY;
            double[] colZ;
            double[] adAngle = Array.ConvertAll(m_C3d.GetData(), element => (double)element);
            Ojw.CKinematics.CForward.CalcKinematics(m_CDhAll, adAngle, out colX, out colY, out colZ, out dX, out dY, out dZ);

            CDh.InitData();

            float fSize = Ojw.CConvert.StrToFloat(txtDHSize.Text);

            MoveToDhPosition(fSize, 1.0f, Color.Yellow);

            //if (m_chkSkeletonView.Checked == true)
            MakeDHSkeleton(chkMakeVisibleSkeleton.Checked, fSize, Color.Violet, txtDh.Text);


            //m_C3d.User_Clear();

            //m_C3d.SetHeader_strDrawModel(txtDH_Tab_Skeleton.Text);
            //m_C3d.CompileDesign();
        }
        public string MakeDHSkeleton(bool bVisible, float fSize, Color cColor, String strDh)
        {
            //strDh = Ojw.CConvert.RemoveCaption(Ojw.CConvert.RemoveChar(Ojw.CConvert.RemoveChar(strDh, ']'), '['), true, true);
            //string [] pstrDh = strDh.Split(',');
            int nStartGroupNumber = Ojw.CConvert.StrToInt(txtDH_StartGroup.Text);
            float fOffset_X = Ojw.CConvert.StrToFloat(txtDH_Offset_Trans_X.Text);
            float fOffset_Y = Ojw.CConvert.StrToFloat(txtDH_Offset_Trans_Y.Text);
            float fOffset_Z = Ojw.CConvert.StrToFloat(txtDH_Offset_Trans_Z.Text);
            return MakeDHSkeleton(bVisible, fSize, cColor, strDh, nStartGroupNumber,
                fOffset_X, fOffset_Y, fOffset_Z);
        }
        public string MakeDHSkeleton(bool bVisible, float fSize, Color cColor, string strString, int nStartGroupNumber, float fOffset_X, float fOffset_Y, float fOffset_Z)
        {
            return MakeDHSkeleton(bVisible, fSize, cColor, strString, nStartGroupNumber, fOffset_X, fOffset_Y, fOffset_Z, 0.0f, 0.0f, 0.0f);
        }
        public string MakeDHSkeleton(bool bVisible, float fSize, Color cColor, string strString, int nStartGroupNumber, float fOffset_X, float fOffset_Y, float fOffset_Z, float fOffset_Pan, float fOffset_Tilt, float fOffset_Swing)
        {
            return m_C3d.MakeDHSkeleton(bVisible, fSize, cColor, strString, nStartGroupNumber, fOffset_X, fOffset_Y, fOffset_Z, fOffset_Pan, fOffset_Tilt, fOffset_Swing, ref txtDH_Tab_Skeleton);

            // "@" 가 들어간 명령어는 순서데로 [StlFileName, Color, Alpha, GroupA, GroupB, Function, X, Y, Z, P, T, S]
            String strModel_Bar = (bVisible == true) ? "#8" : "#19";
            String strModel_Ball = (bVisible == true) ? "#9" : "#20";
            string strModel_End = (bVisible == true) ? "#8" : "#19";
            String strData = String.Empty;
            String strDisp = String.Empty;
            //SetHeader_strDrawModel(String.Empty);
            String strConvert0 = Ojw.CConvert.RemoveCaption(strString, true, true);
            //String strConvert0 = Ojw.CConvert.RemoveCaption("@", Ojw.CConvert.RemoveCaption("//", strString, true, true), true, true);
            String strConvert1 = Ojw.CConvert.RemoveChar(strConvert0, '[');
            String strConvert2 = Ojw.CConvert.RemoveChar(strConvert1, ']');
            String strConvert3 = Ojw.CConvert.RemoveChar(strConvert2, '\r');
            String[] pstrLines = strConvert3.Split('\n');

            m_C3d.User_Clear();

            m_C3d.User_Set_Init(true); // 초기점
            m_C3d.User_Set_Translation(0, fOffset_X, fOffset_Y, fOffset_Z);
            m_C3d.User_Set_Rotation(0, fOffset_Pan, fOffset_Tilt, fOffset_Swing);

            int nLine = 0;
            bool bEnd = false;
            foreach (string strLine in pstrLines)
            {
                if (nLine == pstrLines.Length - 1) bEnd = true;
                else bEnd = false;

                nLine++;
                float fA = 0.0f;
                float fD = 0.0f;
                float fTheta = 0.0f;
                float fAlpha = 0.0f;
                int nAxis = -1;
                int nDir = 0;
                int nInit = 0;

                #region Items
                String[] pstrItems = strLine.Split(',');
                bool bModeling = false;
                bool bCalc_After = false;
                if (strLine.IndexOf('@') == 0)
                {
                    bModeling = true;
                }
                else if (strLine.IndexOf('$') == 0)
                {
                    bCalc_After = true;
                }
                int i = 0;
                string strAdd_Model = "#8";
                string strOffset = "";
                Color cAdd_Color = Color.White;
                float fAdd_Alpha = 1.0f;
                int nAdd_Type = 0;
                int nAdd_Num = 255;
                float[] fTrans = new float[3];
                float[] fRot = new float[3];
                foreach (string strItem in pstrItems)
                {
                    if (bModeling == true)
                    {
                        // [model,color,alpha,type,Num,x,y,z,pan,tilt,swing]
                        try
                        {
                            switch (i)
                            {
                                case 0: strAdd_Model = strItem.Substring(1); break;
                                case 1: cAdd_Color = Color.FromArgb(Ojw.CConvert.StrToInt(strItem)); break;
                                case 2: fAdd_Alpha = Ojw.CConvert.StrToFloat(strItem); break;
                                case 3: nAdd_Type = Ojw.CConvert.StrToInt(strItem); break;
                                case 4: nAdd_Num = Ojw.CConvert.StrToInt(strItem); break;
                                case 5: fTrans[0] = Ojw.CConvert.StrToFloat(strItem); break;
                                case 6: fTrans[1] = Ojw.CConvert.StrToFloat(strItem); break;
                                case 7: fTrans[2] = Ojw.CConvert.StrToFloat(strItem); break;
                                case 8: fRot[0] = Ojw.CConvert.StrToFloat(strItem); break;
                                case 9: fRot[1] = Ojw.CConvert.StrToFloat(strItem); break;
                                case 10: fRot[2] = Ojw.CConvert.StrToFloat(strItem); break;
                                case 11: strOffset = strItem; break;
                            }
                        }
                        catch
                        {
                            switch (i)
                            {
                                case 0: strAdd_Model = "#8"; break;
                                case 1: cAdd_Color = Color.White; break;
                                case 2: fAdd_Alpha = 1.0f; break;
                                case 3: nAdd_Type = 0; break;
                                case 4: nAdd_Num = 255; break;
                                case 5: fTrans[0] = 0.0f; break;
                                case 6: fTrans[1] = 0.0f; break;
                                case 7: fTrans[2] = 0.0f; break;
                                case 8: fRot[0] = 0.0f; break;
                                case 9: fRot[1] = 0.0f; break;
                                case 10: fRot[2] = 0.0f; break;
                            }
                        }
                    }
                    else if (bCalc_After == true)
                    {
                        // [model,color,alpha,type,Num,x,y,z,pan,tilt,swing]
                        try
                        {
                            if (i == 0)
                            {

                            }
                            else
                            {
                            }
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        try
                        {
                            switch (i)
                            {
                                case 0: fA = Ojw.CConvert.StrToFloat(strItem); break;
                                case 1: fD = Ojw.CConvert.StrToFloat(strItem); break;
                                case 2: fTheta = Ojw.CConvert.StrToFloat(strItem); break;
                                case 3: fAlpha = Ojw.CConvert.StrToFloat(strItem); break;
                                case 4: nAxis = Ojw.CConvert.StrToInt(strItem); break;
                                case 5: nDir = Ojw.CConvert.StrToInt(strItem); break;
                                case 6: nInit = Ojw.CConvert.StrToInt(strItem); break;
                            }
                        }
                        catch
                        {
                            switch (i)
                            {
                                case 0: fA = 0.0f; break;
                                case 1: fD = 0.0f; break;
                                case 2: fTheta = 0.0f; break;
                                case 3: fAlpha = 0.0f; break;
                                case 4: nAxis = 0; break;
                                case 5: nDir = 0; break;
                                case 6: nInit = 0; break;
                            }
                        }
                    }
                    i++;
                }
                //if (i != 6) return null; // 해석 에러
                //if (i != 7) continue; // 해석 에러 => 굳이 초기화 없이도 해석 되게...
                #endregion Items

                #region 추가된 모델링
                if (bModeling == true)
                {
                    /*
                        strAdd_Model = "#8";  break;
                        cAdd_Color = Color.White; break;
                        fAdd_Alpha = 1.0f; break;
                        nAdd_Type = 0; break;
                        nAdd_Num = 255; break;
                        fTrans[0] = 0.0f; break;
                        fTrans[1] = 0.0f; break;
                        fTrans[2] = 0.0f; break;
                        fRot[0] = 0.0f; break;
                        fRot[1] = 0.0f; break;
                        fRot[2] = 0.0f; break;
                     */
                    if (nInit > 0)
                        m_C3d.User_Set_Init(true);
                    m_C3d.User_Set_Model(strAdd_Model);
                    m_C3d.User_Set_Color(cAdd_Color);
                    m_C3d.User_Set_Alpha(fAdd_Alpha);

                    m_C3d.User_Set_Angle_Offset(strOffset);

                    //m_C3d.User_Set_nPickGroup_A(nStartGroupNumber);
                    if (nAdd_Type == 0)
                    {
                        m_C3d.User_Set_nPickGroup_A(0);
                        m_C3d.User_Set_nPickGroup_B(0);
                    }
                    else if (nAdd_Type == 1)
                    {
                        m_C3d.User_Set_nPickGroup_A(0);
                        m_C3d.User_Set_nPickGroup_B(nAdd_Num);
                    }
                    else if (nAdd_Type == 2)
                    {
                        m_C3d.User_Set_nInverseKinematicsNumber(nAdd_Num);
                    }
                    //m_C3d.User_Set_nPickGroup_A(nAdd_Num);


                    m_C3d.User_Set_Offset_Translation(fTrans[0], fTrans[1], fTrans[2]);
                    m_C3d.User_Set_Offset_Rotation(fRot[0], fRot[1], fRot[2]);

                    m_C3d.User_Add();
                    continue;//break;//continue;
                }
                #endregion 추가된 모델링
                //Prop_Set_DispObject(strModel_Ball);
                //Prop_Set_Width_Or_Radius(fSize);
                //AddVirtualClassToReal();
                if (nInit > 0)
                {
                    m_C3d.User_Set_Init(true);
                }
                if (fD != 0)
                {
                    m_C3d.User_Set_Model(strModel_Ball);
                    m_C3d.User_Set_Color(cColor);
                    m_C3d.User_Set_Width_Or_Radius(fSize);
                    m_C3d.User_Add();


                    m_C3d.User_Set_Model(strModel_Bar);
                    m_C3d.User_Set_Color(cColor);
                    m_C3d.User_Set_Width_Or_Radius(fSize);
                    if (fD < 0)
                    {
                        m_C3d.User_Set_Offset_Rotation(180, 0, 0);
                        m_C3d.User_Set_Height_Or_Depth(-fD);
                    }
                    else m_C3d.User_Set_Height_Or_Depth(fD);
                    m_C3d.User_Add();
                }
                if (nAxis >= 0)
                {
                    m_C3d.User_Set_Model(strModel_Ball);
                    m_C3d.User_Set_Color(cColor);
                    m_C3d.User_Set_Width_Or_Radius(fSize);
                    m_C3d.User_Set_Offset_Translation(0, 0, fD);

                    m_C3d.User_Set_AxisName(nAxis);

                    // [0 ~ 2(Pan, Tilt, Swing), 3~5(x,y,z), 6(cw), 7(ccw)]
                    if ((nDir == 0) || (nDir == 1)) m_C3d.User_Set_AxisMoveType(2);
                    else if ((nDir == 2) || (nDir == 3)) m_C3d.User_Set_AxisMoveType(5);
                    if ((nDir == 0) || (nDir == 2)) m_C3d.User_Set_Dir(0);
                    else if ((nDir == 1) || (nDir == 3)) m_C3d.User_Set_Dir(1);

                    m_C3d.User_Add();
                    if ((nDir == 2) || (nDir == 3))
                    {
                        m_C3d.User_Set_Model(strModel_Bar);
                        m_C3d.User_Set_Color(Color.FromArgb(cColor.R / 2, cColor.G / 2, cColor.B / 2));
                        m_C3d.User_Set_Width_Or_Radius(fSize);

                        if (nDir == 2) m_C3d.User_Set_Offset_Rotation(180, 0, 0);

                        m_C3d.User_Set_Height_Or_Depth(m_C3d.GetData(nAxis));

                        m_C3d.User_Add();
                    }
                }

                if (fA != 0)
                {
                    m_C3d.User_Set_Model(strModel_Bar);
                    m_C3d.User_Set_Color(cColor);
                    if (fA < 0)
                    {
                        m_C3d.User_Set_Offset_Translation(0, 0, fD);
                        m_C3d.User_Set_Offset_Rotation(-90, fTheta, 0);
                        m_C3d.User_Set_Width_Or_Radius(fSize);
                        m_C3d.User_Set_Height_Or_Depth(-fA);
                    }
                    else
                    {
                        m_C3d.User_Set_Offset_Translation(0, 0, fD);
                        m_C3d.User_Set_Offset_Rotation(90, -fTheta, 0);
                        m_C3d.User_Set_Width_Or_Radius(fSize);
                        m_C3d.User_Set_Height_Or_Depth(fA);
                    }
                    m_C3d.User_Add();
                }

                //if (bEnd == true)
                //{
                //    m_C3d.User_Set_Model(strModel_End);
                //}
                //else
                //{
                m_C3d.User_Set_Model(strModel_Ball);
                //}
                m_C3d.User_Set_Color(cColor);
#if false // end position
                m_C3d.User_Set_Width_Or_Radius(fSize * ((bEnd == true) ? 2.0f : 1.0f));
#else
                m_C3d.User_Set_Width_Or_Radius(fSize * ((bEnd == true) ? 1.0f : 1.0f));
#endif


                int nIndex = 0;
                m_C3d.User_Set_Translation(nIndex, 0, 0, fD);
                m_C3d.User_Set_Rotation(nIndex, 0, 0, fTheta);
                nIndex++;
                m_C3d.User_Set_Translation(nIndex, fA, 0, 0);
                m_C3d.User_Set_Rotation(nIndex, 0, fAlpha, 0);

                m_C3d.User_Add();




                //strDisp = GetHeader_strDrawModel();

            }
            #region Skeleton
            StringBuilder sbResult = new StringBuilder();
#if _USING_DOTNET_3_5
                sbResult.Remove(0, sbResult.Length);
#elif _USING_DOTNET_2_0
                sbResult.Remove(0, sbResult.Length);
#else
            sbResult.Clear(); // Dotnet 4.0 이상에서만 사용
#endif
            int nGroupNum = nStartGroupNumber;
            int nMotorNum = -1;
            int nCnt = m_C3d.User_GetCnt();
            Color[] acColor = new Color[7] { Color.Orange, Color.Cyan, Color.Blue, Color.Lime, Color.Yellow, Color.LightGreen, Color.Magenta };
            for (int i = 0; i < nCnt; i++) // Except 3 Directions
            {
                String strResult = String.Empty;
#if true
                if (m_C3d.User_Get_AxisName() >= 0)
                {
                    nGroupNum++;

                    nMotorNum = m_C3d.User_Get_AxisName();
                    sbResult.Append(String.Format("// Group = {0}, Motor = {1} //////////////////////////\r\n", nGroupNum, nMotorNum));
                }
#else
                #region EndPosition                
                if ((m_C3d.User_Get_AxisName() >= 0) || (nCnt - 1 == i))
                {
                    nGroupNum++;

                    if (i < (nCnt - 1))
                    {
                        nMotorNum = m_C3d.User_Get_AxisName();
                        sbResult.Append(String.Format("// Group = {0}, Motor = {1} //////////////////////////\r\n", nGroupNum, nMotorNum));
                    }
                    else sbResult.Append(String.Format("// End Position\r\n"));
                }
                #endregion EndPosition
#endif

                #region
                Ojw.C3d.COjwDisp CDisp = m_C3d.User_Get(i);

                if (CDisp.strDispObject.IndexOf('#') == 0)
                {
                    if (CDisp.bInit == true)
                    {
                        CDisp.SetData_Color(acColor[0]);
                        CDisp.SetData_nPickGroup_A(0);
                        CDisp.SetData_nPickGroup_B(0);
                    }
                    else
                    {
                        CDisp.SetData_Color(acColor[nGroupNum % acColor.Length]);
                        CDisp.SetData_nPickGroup_A(nGroupNum);
                        CDisp.SetData_nPickGroup_B(nMotorNum);
                    }
                }
                else
                {
                    CDisp.SetData_nPickGroup_A(nGroupNum);
                    CDisp.SetData_nPickGroup_B(nMotorNum);
                    //CDisp.User_Set_nInverseKinematicsNumber(nAdd_Num);
                }
                //if (m_C3d.User_Get_Model()


                //if (bEnd == true)
                //{
                //    CDisp.SetData_nInverseKinematicsNumber();
                //}
                //User_Set(i, CDisp);
                #endregion
                //Convert_CDisp_To_String(User_Get(i), ref strResult);
                m_C3d.Convert_CDisp_To_String(CDisp, ref strResult);
                sbResult.Append(strResult);
            }
            txtDH_Tab_Skeleton.Text = sbResult.ToString();
            //strResult = Ojw.Ojw.CConvert.RemoveString(strResult, "\r\n");
            //AddHeader_strDrawModel(strResult);
            //CompileDesign();
            #endregion Skeleton

            //////////////// Direction
            float fMulti = fSize / 10.0f * 2.0f;
            float fLength = 30 * fMulti;

            m_C3d.User_Set_Model(strModel_Ball);
            m_C3d.User_Set_Color(Color.Red);
            m_C3d.User_Set_Width_Or_Radius(fSize);
            m_C3d.User_Set_Offset_Translation(fLength, 0, 0);
            m_C3d.User_Add();

            m_C3d.User_Set_Model(strModel_Ball);
            m_C3d.User_Set_Color(Color.Green);
            m_C3d.User_Set_Width_Or_Radius(fSize);
            m_C3d.User_Set_Offset_Translation(0, fLength, 0);
            m_C3d.User_Add();

            m_C3d.User_Set_Model(strModel_Ball);
            m_C3d.User_Set_Color(Color.Blue);
            m_C3d.User_Set_Width_Or_Radius(fSize);
            m_C3d.User_Set_Offset_Translation(0, 0, fLength);
            m_C3d.User_Add();
            //////////////////////////

            return sbResult.ToString();
        }

        private void btnDH_Param_Add_Click(object sender, EventArgs e)
        {
            CDhParam CDh = new CDhParam();
            CDh.InitData();
            CDh.nInit = (cmbDH_Init.SelectedIndex > 0) ? 1 : 0;
            CDh.nAxisDir = (cmdDH_Param_Dir.SelectedIndex > 0) ? 1 : 0;
            CDh.dA = Ojw.CConvert.StrToDouble(txtDH_Param_A.Text);
            CDh.dD = Ojw.CConvert.StrToDouble(txtDH_Param_D.Text);
            CDh.dTheta = Ojw.CConvert.StrToDouble(txtDH_Param_Theta.Text);
            CDh.dAlpha = Ojw.CConvert.StrToDouble(txtDH_Param_Alpha.Text);
            CDh.nAxisNum = Ojw.CConvert.StrToInt(txtDH_Param_Axis.Text);
            AddDevice(CDh);

            InitTestDrawing();
#if false
            int nAxisNum = Ojw.CConvert.StrToInt(txtDH_Param_Axis.Text);
            frmDesigner.m_COjwDhParam.strCaption = txtDH_Caption.Text;
            if (nAxisNum >= 0) frmDesigner.m_COjwDhParam.strCaption += " - Axis" + Ojw.CConvert.IntToStr(nAxisNum);
            String strData = Ojw.CKinematics.CForward.ClassToString_DHParam(frmDesigner.m_COjwDhParam);

            txtDH.Text += strData + "\r\n"; //strData + frmDesigner.m_COjwDhParam.strCaption + "\r\n";

            Ojw.CKinematics.CForward.MakeDhParam(txtDH.Text, out frmDesigner.m_COjwDhParamAll);
            double dX, dY, dZ;
            double[] colX;
            double[] colY;
            double[] colZ;
            double[] adAngle = Array.ConvertAll(m_C3d.GetData(), element => (double)element);
            Ojw.CKinematics.CForward.CalcKinematics(frmDesigner.m_COjwDhParamAll, adAngle, out colX, out colY, out colZ, out dX, out dY, out dZ);

            frmDesigner.m_COjwDhParam.InitData();
            // 초기화
            txtDH_Param_A.Text = Ojw.CConvert.DoubleToStr(frmDesigner.m_COjwDhParam.dA);
            txtDH_Param_D.Text = Ojw.CConvert.DoubleToStr(frmDesigner.m_COjwDhParam.dD);
            txtDH_Param_Theta.Text = Ojw.CConvert.DoubleToStr(frmDesigner.m_COjwDhParam.dTheta);
            txtDH_Param_Alpha.Text = Ojw.CConvert.DoubleToStr(frmDesigner.m_COjwDhParam.dAlpha);
            txtDH_Param_Axis.Text = Ojw.CConvert.IntToStr(frmDesigner.m_COjwDhParam.nAxisNum);
            txtDH_Caption.Text = frmDesigner.m_COjwDhParam.strCaption;
            cmdDH_Param_Dir.SelectedIndex = frmDesigner.m_COjwDhParam.nAxisDir;

            //txtDH_StartGroup.Text = Ojw.CConvert.IntToStr(frmDesigner.m_COjwDhParam.nStartGroup);
            //txtDH_Offset_X.Text = Ojw.CConvert.DoubleToStr(frmDesigner.m_COjwDhParam.dOffset_X);
            //txtDH_Offset_Y.Text = Ojw.CConvert.DoubleToStr(frmDesigner.m_COjwDhParam.dOffset_Y);
            //txtDH_Offset_Z.Text = Ojw.CConvert.DoubleToStr(frmDesigner.m_COjwDhParam.dOffset_Z);
            cmbDH_Init.SelectedIndex = frmDesigner.m_COjwDhParam.nInit;

            MoveToDhPosition(Ojw.CConvert.StrToFloat(txtDHSize.Text), 1.0f, Color.FromArgb(Ojw.CConvert.StrToInt(txtDHColor.Text)));

            if ((chkDH_View_Skeleton.Checked == true) || (chkDH_ViewAll.Checked == true))
            {
                MakeDHSkeleton(Ojw.CConvert.StrToFloat(txtDHSize.Text), Color.FromArgb(Ojw.CConvert.StrToInt(txtDHColor.Text)), txtDH_StartGroup.Text,
                        Ojw.CConvert.StrToInt(txtDH_StartGroup.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_X.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_Y.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_Z.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Rot_Pan.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Rot_Tilt.Text),
                        Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Rot_Swing.Text)
                        );
            }
#endif
        }

        private void MoveToDhPosition(float fSize, float fAlpha, Color cColor)
        {
            ///////////////////
            float fOffset_X = Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_X.Text);
            float fOffset_Y = Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_Y.Text);
            float fOffset_Z = Ojw.CConvert.StrToFloat(txtDH_Param_Offset_Trans_Z.Text);
            ///////////////////

            int i;
            CDhParamAll COjwDhParamAll = new CDhParamAll();
            Ojw.CKinematics.CForward.MakeDhParam(txtDh.Text, out COjwDhParamAll);
            double dX, dY, dZ;
            double[] dcolX;
            double[] dcolY;
            double[] dcolZ;

            double[] adMot = new double[256];
            Array.Clear(adMot, 0, adMot.Length);
            for (i = 0; i < m_C3d.GetHeader_nMotorCnt(); i++) adMot[i] = (double)m_C3d.GetData(i);// Ojw.CConvert.StrToDouble(m_txtAngle[i].Text);

            Ojw.CKinematics.CForward.CalcKinematics(COjwDhParamAll, adMot, out dcolX, out dcolY, out dcolZ, out dX, out dY, out dZ);
            String strResult;
            Ojw.CKinematics.CForward.CalcKinematics_ToString(COjwDhParamAll, adMot, out strResult);

            //txtError.Text = strResult;
            txtDH_Tab_Result.Text = strResult;

            //Ojw.CMessage.Write_Error(strResult);

            //m_afTestPoint[0] = dX;
            //m_afTestPoint[1] = dY;
            //m_afTestPoint[2] = dZ;
            //m_C3d.SetTestDh(true);
            m_C3d.SetTestDh_Size(fSize);
            m_C3d.SetTestDh_Color(cColor);
            m_C3d.SetTestDh_Alpha(fAlpha);
            m_C3d.SetTestDh_Pos((float)dX + fOffset_X, (float)dY + fOffset_Y, (float)dZ + fOffset_Z);

            //m_lbTestDh.Text = "[x=" + Ojw.CConvert.DoubleToStr((double)Math.Round(dX, 3)) + ", y=" + Ojw.CConvert.DoubleToStr((double)Math.Round(dY, 3)) + ", z=" + Ojw.CConvert.DoubleToStr((double)Math.Round(dZ, 3)) + "]";

            #region Checking Direction
            // 방향 확인
            float[] afX = new float[3];
            float[] afY = new float[3];
            float[] afZ = new float[3];

#if true
            // X     dcolX[0] dcolY[0] dcolZ[0]   a
            // Y     dcolX[1] dcolY[1] dcolZ[1]   b
            // Z     dcolX[2] dcolY[2] dcolZ[2]   c

            //i = 0;
            //afX[0] = (float)Ojw.CMath.ACos(dcolX[0]);
            //afX[1] = (float)Ojw.CMath.ACos(dcolX[1]);
            //afX[2] = (float)Ojw.CMath.ACos(dcolX[2]);
            ////i++;
            //afY[0] = (float)Ojw.CMath.ACos(dcolY[0]);
            //afY[1] = (float)Ojw.CMath.ACos(dcolY[1]);
            //afY[2] = (float)Ojw.CMath.ACos(dcolY[2]);
            ////i++;
            //afZ[0] = (float)Ojw.CMath.ACos(dcolZ[0]);
            //afZ[1] = (float)Ojw.CMath.ACos(dcolZ[1]);
            //afZ[2] = (float)Ojw.CMath.ACos(dcolZ[2]);
            double dLength = 30.0;
            dX = dLength;
            dY = 0.0f;
            dZ = 0.0f;
            for (i = 0; i < 3; i++) afX[i] = (float)(dcolX[i] * dX + dcolY[i] * dY + dcolZ[i] * dZ);
            dX = 0.0f;
            dY = dLength;
            dZ = 0.0f;
            for (i = 0; i < 3; i++) afY[i] = (float)(dcolX[i] * dX + dcolY[i] * dY + dcolZ[i] * dZ);
            dX = 0.0f;
            dY = 0.0f;
            dZ = dLength;
            for (i = 0; i < 3; i++) afZ[i] = (float)(dcolX[i] * dX + dcolY[i] * dY + dcolZ[i] * dZ);

#else
            afX[0] = (float)Ojw.CMath.ACos(dcolX[0]);
            afX[1] = (float)Ojw.CMath.ACos(dcolY[0]);
            afX[2] = (float)Ojw.CMath.ACos(dcolZ[0]);

            afY[0] = (float)Ojw.CMath.ACos(dcolX[1]);
            afY[1] = (float)Ojw.CMath.ACos(dcolY[1]);
            afY[2] = (float)Ojw.CMath.ACos(dcolZ[1]);

            afZ[0] = (float)Ojw.CMath.ACos(dcolX[2]);
            afZ[1] = (float)Ojw.CMath.ACos(dcolY[2]);
            afZ[2] = (float)Ojw.CMath.ACos(dcolZ[2]);
#endif
            m_C3d.SetTestDh_Angle(afX, afY, afZ);

            #endregion Checking Direction
        }

        private void frmKinematics_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_C3d.Event_FileOpen.UserEvent -= new EventHandler(FileOpen);
        }

        private void btnFindStlFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Application.StartupPath + "\\ase";
            dlg.Multiselect = false;
            if (dlg.ShowDialog() != DialogResult.OK) return;
            txtAdd_StlFile.Text = dlg.FileName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ColorDialog cdDlg = new ColorDialog();
            cdDlg.AllowFullOpen = true;
            cdDlg.ShowHelp = true;

            int nColor = Ojw.CConvert.StrToInt(txtAdd_Color.Text);
            Color cColor = Color.FromArgb(nColor);

            cdDlg.Color = cColor;
            if (cdDlg.ShowDialog() == DialogResult.OK)
                cColor = cdDlg.Color;
            txtAdd_Color.Text = Ojw.CConvert.IntToStr(cColor.ToArgb());
            cdDlg.Dispose();
        }

        private void btnAdd_Model_Click(object sender, EventArgs e)
        {
            if (txtAdd_StlFile.Text.IndexOf(".stl") > 0)
            {
                if (txtDh.Lines.Length > 0)
                {
                    List<String> lstStrDh = new List<String>();
                    lstStrDh.Clear();
                    lstStrDh.AddRange(txtDh.Lines);
                    //[model,color,alpha,type,Num],[x,y,z],[pan,tilt,swing]
                    lstStrDh.Add(string.Format("@[{0},{1},{2}],[{3},{4}],[{5},{6},{7}],[{8},{9},{10}]",
                        Ojw.CFile.GetName(txtAdd_StlFile.Text), txtAdd_Color.Text, txtAdd_Alpha.Text,
                        cmbAdd_Motor.SelectedIndex, txtAdd_Motor.Text,
                        txtAdd_X.Text, txtAdd_Y.Text, txtAdd_Z.Text,
                        txtAdd_Pan.Text, txtAdd_Tilt.Text, txtAdd_Swing.Text));
                    txtDh.Clear();
                    for (int i = 0; i < lstStrDh.Count; i++)
                    {
                        txtDh.Text += lstStrDh[i] + "\r\n";
                    }
                }
                //String.Format(
            }

            /////////////////////////////
            float fSize = Ojw.CConvert.StrToFloat(txtDHSize.Text);
            MakeDHSkeleton(true, fSize, Color.Violet, txtDh.Text);
            MakeString_For_ForwardKinematics();
            /////////////////////////////

            InitTestDrawing();
        }

        //private List<SMotorInfo_t> m_lstSMotors = new List<SMotorInfo_t>();
        //private List<bool> m_lstIsMotors = new List<bool>();
        private bool[] m_IsMotors = new bool[256];
        private void lstboxMotorIndex_DrawItem(object sender, DrawItemEventArgs e)
        {
            OjwDrawItem(m_IsMotors, sender, e);
        }
        private void OjwDrawItem(bool[] IsChecked, object sender, DrawItemEventArgs e)
        {
            if ((((ListBox)sender).Items.Count > 0) && (IsChecked.Length > 0))
            {
                //if (e.Item != null)
                //{
                //    e.Graphics.DrawString(
                //        item.Message,
                //        listBox1.Font,
                //        new SolidBrush(Color.Red),
                //        0,
                //        e.ItemIndex * lstviewUnit.It
                //    );
                //}
                //else
                //{
                //    // The item isn't a MyListBoxItem, do something about it
                //}
                // Draw the background of the ListBox control for each item.
                e.DrawBackground();
                // Define the default color of the brush as black.
                Brush myBrush = Brushes.Black;

                // Determine the color of the brush to draw each item based 
                // on the index of the item to draw.

                if (IsChecked[e.Index] == true) myBrush = Brushes.Red;
                else myBrush = Brushes.DarkGray;

                // Draw the current item text based on the current Font 
                // and the custom brush settings.
                e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(),
                    e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
                // If the ListBox has focus, draw a focus rectangle around the selected item.
                e.DrawFocusRectangle();
            }
        }

        //frmDrawText m_frmDraw = new frmDrawText();
        private void btnGetMotors_From_3d_Click(object sender, EventArgs e)
        {
            GetMotors();
        }
        private void ClearMotor()
        {
            //m_C3d.m_lstMotors.Clear();
            Array.Clear(m_IsMotors, 0, m_IsMotors.Length);
        }
        private void GetMotors()
        {
            int nCount;
            int[] anMotors;// = new int[256];
            //int[] anMotors_Old = m_C3d.m_lstMotors.ToArray<int>();
            string strError;

            Array.Clear(m_IsMotors, 0, m_IsMotors.Length);

            /*for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++)
            {
                int nAxis = m_C3d.m_CHeader.anMotorIDs[i];
                if ((m_C3d.m_CHeader.pSMotorInfo[nAxis].nMotor_Enable < 0) || (m_C3d.m_CHeader.pSMotorInfo[nAxis].nMotor_Enable > 1)) continue;
                int nID = m_C3d.m_CHeader.pSMotorInfo[nAxis].nMotionEditor_Index;
                m_IsMotors[nID] = true;
                DisplayIndex(nID);
            }*/

            string strDraw = frmDesigner.m_C3d.GetHeader_strDrawModel();// m_frmDraw.rtxtDraw.Text;
            m_C3d.CompileDesign(strDraw, out nCount, out anMotors, out strError);

            for (int i = 0; i < nCount; i++)
            {
                int nID = anMotors[i];
                if (m_IsMotors[nID] == false)
                {
                    m_IsMotors[nID] = true;
                    //m_C3d.m_lstMotors.Add(nID);
                    //m_C3d.m_CHeader.pSMotorInfo[nID].nMotor_Enable = 0; // 0: Dontcare, 1: Enable, -1: Disable => 이게 Disable 이면 모터 표시를 죽인다.
                    DisplayIndex(nID);
                }
            }
            lstboxMotorIndex.Refresh();
        }

        private void lstboxMotorIndex_DoubleClick(object sender, EventArgs e)
        {
            if (lstboxMotorIndex.SelectedIndex >= 0)
            {
                CheckItem_Unit(lstboxMotorIndex.SelectedIndex);
            }
        }
        private void CheckItem_Unit(int nItemIndex)
        {
            bool bRet = ((m_IsMotors[nItemIndex] == false) ? true : false);
            if (nItemIndex >= 0) m_IsMotors[nItemIndex] = bRet;


        }

        private void Axis_Select(int nIndex)
        {
            if (nIndex < m_C3d.m_CHeader.pSMotorInfo.Length)
            {
                //m_C3d.SetPick_ColorMode(true);
                //m_C3d.SetPick_ColorValue(Color.Green); // 클릭된 부분을 녹색으로 설정
                //m_C3d.SetPick_AlphaMode(true); // 투명 모드 활성화
                //m_C3d.SetPick_AlphaValue(0.5f); // 클릭된 부분을 반투명으로 한다.
                //m_C3d.SelectObject_Clear();
                //m_C3d.SelectObject_Add(m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotorID);
                m_C3d.SelectMotor(m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotorID);

                txtAxis_ID_Real.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotorID);
                txtAxis_ID_Mirror.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nAxis_Mirror);
                txtAxis_LmitUp.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fLimit_Up);
                txtAxis_LmitDn.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fLimit_Down);
                txtAxis_Mech_Center.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nCenter_Evd);
                txtAxis_Mech_Move.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nMechMove);
                txtAxis_Mech_Angle.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fMechAngle);
                txtAxis_InitAngle.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fInitAngle);
                txtAxis_InitAngle2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fInitAngle2);
                cmbAxis_Dir.SelectedIndex = m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotorDir;
                cmbAxis_ControlType.SelectedIndex = m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotorControlType;
                txtAxis_Group.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nGroupNumber);
                txtAxis_NickName.Text = m_C3d.m_CHeader.pSMotorInfo[nIndex].strNickName;

                int nTmp = m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotorEnable_For_RPTask;
                cmbAxis_MotorEnable_For_RpTask3.SelectedIndex = (nTmp < 0) ? 2 : nTmp;
                nTmp = m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotor_Enable;
                cmbAxis_MotorEnable.SelectedIndex = (nTmp < 0) ? 2 : nTmp;

                nTmp = m_C3d.m_CHeader.pSMotorInfo[nIndex].nHwMotor_Index;
                cmbMotorName.SelectedIndex = (nTmp < 0) ? 2 : nTmp;
                m_C3d.m_CHeader.pSMotorInfo[nIndex].nHwMotor_Key = m_C3d.m_CMonster.DicMotor_Key(cmbMotorName.SelectedIndex);

                nTmp = m_C3d.m_CHeader.pSMotorInfo[nIndex].nProtocolVersion;
                cmbAxis_ProtocolVersion.SelectedIndex = (nTmp < 0) ? 2 : nTmp;

                txtAxis_Reserve_Int_2.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotionEditor_Index);

                cmbAxis_IsHighSpecMotor.SelectedIndex = m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotor_HightSpec;

                txtAxis_Reserve_Int_4.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_4);
                txtAxis_Reserve_Int_5.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_5);
                txtAxis_Reserve_Int_6.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_6);
                txtAxis_Reserve_Int_7.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_7);
                txtAxis_Reserve_Int_8.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_8);
                txtAxis_Reserve_Int_9.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_9);

                txtAxis_GearRatio.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fGearRatio);

                txtAxis_RobotisConvertingVar.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fRobotisConvertingVar);
                txtAxis_Reserve_Float_2.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_2);
                txtAxis_Reserve_Float_3.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_3);
                txtAxis_Reserve_Float_4.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_4);
                txtAxis_Reserve_Float_5.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_5);
                txtAxis_Reserve_Float_6.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_6);
                txtAxis_Reserve_Float_7.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_7);
                txtAxis_Reserve_Float_8.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_8);
                txtAxis_Reserve_Float_9.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_9);


                nTmp = m_C3d.m_CHeader.pSMotorInfo[nIndex].nGuide_Event;
                cmbGuide_Event.SelectedIndex = (nTmp < 0) ? 0 : nTmp;

                nTmp = m_C3d.m_CHeader.pSMotorInfo[nIndex].nGuide_AxisType;
                cmbGuide_AxisType.SelectedIndex = (nTmp < 0) ? 0 : nTmp;

                nTmp = m_C3d.m_CHeader.pSMotorInfo[nIndex].nGuide_RingColorType;
                cmbGuide_RingColorType.SelectedIndex = (nTmp < 0) ? 0 : nTmp;



                txtGuide_RingSize.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fGuide_RingSize);
                txtGuide_RingThick.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fGuide_RingThick);


                //nTmp = m_C3d.m_CHeader.pSMotorInfo[nIndex].nGuide_RingDir;
                //cmbGuide_RingDir.SelectedIndex = (nTmp < 0) ? 0 : nTmp;
                nTmp = m_C3d.m_CHeader.pSMotorInfo[nIndex].nGuide_RingDir;
                if (nTmp == 0) nTmp = 1;
                txtGuide_RingDir.Text = Ojw.CConvert.FloatToStr(nTmp);

                txtGuide_3D_Scale.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fGuide_3D_Scale);
                txtGuide_3D_Alpha.Text = Ojw.CConvert.FloatToStr(m_C3d.m_CHeader.pSMotorInfo[nIndex].fGuide_3D_Alpha);


                if (m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos.Length != 6)
                {
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos = new float[6];
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs = new int[6];
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir = new int[6];
                    Array.Clear(m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos, 0, m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos.Length);
                    Array.Clear(m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs, 0, m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs.Length);
                    Array.Clear(m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir, 0, m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir.Length);
                }
                txtGuide_Pos.Text = String.Format("{0},{1},{2},{3},{4},{5}",
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos[0],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos[1],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos[2],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos[3],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos[4],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos[5]
                    );
                txtGuide_Off_IDs.Text = String.Format("{0},{1},{2},{3},{4},{5}",
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs[0],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs[1],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs[2],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs[3],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs[4],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs[5]
                    );
                txtGuide_Off_Dir.Text = String.Format("{0},{1},{2},{3},{4},{5}",
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir[0],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir[1],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir[2],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir[3],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir[4],
                    m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir[5]
                    );
            }
        }
        private void lstboxMotorIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstboxMotorIndex.SelectedIndex >= 0)
            {
                Axis_Select(lstboxMotorIndex.SelectedIndex);
                //int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
                //Axis_Select(nIndex);
            }
        }

        private void btnCopyToDraw_Click(object sender, EventArgs e)
        {
            frmDesigner.m_frmDrawText.rtxtDraw.Text += String.Format("\r\n//===>Add Skeleton=========================\r\n{0}\r\n", txtDH_Tab_Skeleton.Text);
        }

        private void DisplayIndex(int nIndex)
        {
            string strData = string.Format("{0}[{1}<G{2}/M{3}>:{4}]<=>{5}",
                nIndex,
                m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotorID,
                m_C3d.m_CHeader.pSMotorInfo[nIndex].nGroupNumber,
                m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotionEditor_Index,
                Ojw.CConvert.RemoveChar((m_C3d.m_CHeader.pSMotorInfo[nIndex].strNickName), '\0'),
                m_C3d.m_CHeader.pSMotorInfo[nIndex].nAxis_Mirror
                );
            lstboxMotorIndex.Items[nIndex] = strData;
        }

        private void txtAxis_LmitUp_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fLimit_Up = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_LmitDn_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fLimit_Down = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_ID_Real_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotorID = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }
        private void txtAxis_ID_Mirror_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nAxis_Mirror = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Mech_Center_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nCenter_Evd = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_InitAngle_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fInitAngle = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_InitAngle2_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fInitAngle2 = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Mech_Move_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nMechMove = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Mech_Angle_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fMechAngle = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void cmbAxis_Dir_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotorDir = ((ComboBox)sender).SelectedIndex;
            DisplayIndex(nIndex);
        }

        private void cmbAxis_ControlType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotorControlType = ((ComboBox)sender).SelectedIndex;
            DisplayIndex(nIndex);
        }

        private void txtAxis_Group_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nGroupNumber = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_NickName_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].strNickName = ((TextBox)sender).Text;
            DisplayIndex(nIndex);
        }

        private void cmbAxis_MotorEnable_For_RpTask3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotorEnable_For_RPTask = ((ComboBox)sender).SelectedIndex;
            DisplayIndex(nIndex);
        }

        private void cmbAxis_MotorEnable_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotor_Enable = ((ComboBox)sender).SelectedIndex;
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Int_2_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotionEditor_Index = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        // ojw5014 
        private void cmbAxis_IsHighSpecMotor_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotor_HightSpec = ((ComboBox)sender).SelectedIndex;
            DisplayIndex(nIndex);

            //int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            //m_C3d.m_CHeader.pSMotorInfo[nIndex].nMotor_HightSpec = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            //DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Int_4_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_4 = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Int_5_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_5 = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Int_6_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_6 = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Int_7_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_7 = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Int_8_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_8 = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Int_9_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nReserve_9 = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_GearRatio_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fGearRatio = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_RobotisConvertingVar_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fRobotisConvertingVar = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Float_2_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_2 = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Float_3_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_3 = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Float_4_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_4 = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Float_5_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_5 = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Float_6_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_6 = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Float_7_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_7 = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Float_8_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_8 = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void txtAxis_Reserve_Float_9_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fReserve_9 = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            DisplayIndex(nIndex);
        }

        private void lstboxMotorIndex_Click(object sender, EventArgs e)
        {

        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl2.SelectedIndex == 1)
            {
                GetMotors();
            }
        }

        private void btnClearMotors_Click(object sender, EventArgs e)
        {
            ClearMotor();
            lstboxMotorIndex.Refresh();
        }

        private void cmbMotorName_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nHwMotor_Index = ((ComboBox)sender).SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nHwMotor_Key = m_C3d.m_CMonster.DicMotor_Key(((ComboBox)sender).SelectedIndex);
            DisplayIndex(nIndex);
        }

        private void btnMirror_None_Click(object sender, EventArgs e)
        {
            txtAxis_ID_Mirror.Text = "-1";
        }

        private void btnMirror_Self_Click(object sender, EventArgs e)
        {
            txtAxis_ID_Mirror.Text = "-2";
        }

        private void btnMirror_Sync_Click(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            int nMirror = Ojw.CConvert.StrToInt(txtAxis_ID_Mirror.Text);
            m_C3d.m_CHeader.pSMotorInfo[nMirror].nAxis_Mirror = nIndex;
            //txtAxis_ID_Mirror.Text = "";
            lstboxMotorIndex.Refresh();
        }

        private void btnSetByMotorName_Click(object sender, EventArgs e)
        {
            int nSelectedIndex = cmbMotorName.SelectedIndex;
            if (nSelectedIndex < 0) return;

            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nHwMotor_Index = nSelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nHwMotor_Key = m_C3d.m_CMonster.DicMotor_Key(nSelectedIndex);

            DisplayIndex(nIndex);

            Ojw.CMonster2 CMon = new Ojw.CMonster2();
            float fCenter, fMechMove, fMechAngle, fJointRpm;
            int nProtocolVer;
            CMon.GetParam_by_HwNum(m_C3d.m_CHeader.pSMotorInfo[nIndex].nHwMotor_Key, out fCenter, out fMechMove, out fMechAngle, out fJointRpm, out nProtocolVer);

            txtAxis_Mech_Center.Text = Ojw.CConvert.FloatToStr(fCenter);
            txtAxis_Mech_Move.Text = Ojw.CConvert.FloatToStr(fMechMove);
            txtAxis_Mech_Angle.Text = Ojw.CConvert.FloatToStr(fMechAngle);
            cmbAxis_ProtocolVersion.SelectedIndex = nProtocolVer;
        }

        private void cmbAxis_ProtocolVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nProtocolVersion = ((ComboBox)sender).SelectedIndex;
            //DisplayIndex(nIndex);
        }

        private void cmbGuide_Event_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nGuide_Event = ((ComboBox)sender).SelectedIndex;
            //DisplayIndex(nIndex);
        }

        private void cmbGuide_AxisType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nGuide_AxisType = ((ComboBox)sender).SelectedIndex;
            //DisplayIndex(nIndex);
        }

        private void cmbGuide_RingColorType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nGuide_RingColorType = ((ComboBox)sender).SelectedIndex;
            //DisplayIndex(nIndex);
        }

        private void txtGuide_RingSize_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fGuide_RingSize = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            //DisplayIndex(nIndex);
        }

        private void txtGuide_RingThick_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fGuide_RingThick = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            //DisplayIndex(nIndex);
        }

        private void txtGuide_3D_Scale_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fGuide_3D_Scale = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            //DisplayIndex(nIndex);
        }

        private void txtGuide_3D_Alpha_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].fGuide_3D_Alpha = Ojw.CConvert.StrToFloat(((TextBox)sender).Text);
            //DisplayIndex(nIndex);
        }

        private void txtGuide_Pos_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            string[] pstrItems = ((TextBox)sender).Text.Split(',');
            Array.Clear(m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos, 0, m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos.Length);
            for (int nGuide = 0; nGuide < pstrItems.Length; nGuide++)
            {
                m_C3d.m_CHeader.pSMotorInfo[nIndex].afGuide_Pos[nGuide] = Ojw.CConvert.StrToFloat(pstrItems[nGuide]);
            }
            //DisplayIndex(nIndex);
        }

        private void txtGuide_Off_IDs_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            string[] pstrItems = ((TextBox)sender).Text.Split(',');
            Array.Clear(m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs, 0, m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs.Length);
            for (int nGuide = 0; nGuide < pstrItems.Length; nGuide++)
            {
                m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_IDs[nGuide] = Ojw.CConvert.StrToInt(pstrItems[nGuide]);
            }
            //DisplayIndex(nIndex);
        }

        private void txtGuide_Off_Dir_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            string[] pstrItems = ((TextBox)sender).Text.Split(',');
            Array.Clear(m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir, 0, m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir.Length);
            int nMax = pstrItems.Length;
            if (nMax > 6) nMax = 6;
            for (int nGuide = 0; nGuide < nMax; nGuide++)
            {
                m_C3d.m_CHeader.pSMotorInfo[nIndex].anGuide_Off_Dir[nGuide] = Ojw.CConvert.StrToInt(pstrItems[nGuide]);
            }
            //DisplayIndex(nIndex);
        }

        private void txtGuide_RingDir_TextChanged(object sender, EventArgs e)
        {
            int nIndex = (lstboxMotorIndex.SelectedIndex < 0) ? 0 : lstboxMotorIndex.SelectedIndex;
            m_C3d.m_CHeader.pSMotorInfo[nIndex].nGuide_RingDir = Ojw.CConvert.StrToInt(((TextBox)sender).Text);
            //DisplayIndex(nIndex);
        }

        private void txtRobotName_TextChanged(object sender, EventArgs e)
        {
            m_C3d.m_CHeader.strModelName = txtRobotName.Text;
        }

        private void txtModelNumber_TextChanged(object sender, EventArgs e)
        {
            m_C3d.m_CHeader.nModelNum = Ojw.CConvert.StrToInt(txtModelNumber.Text);
            m_C3d.m_CHeader.strModelNum = txtModelNumber.Text;
        }

        private void txtComment_TextChanged(object sender, EventArgs e)
        {
            m_C3d.m_CHeader.strComment = txtModelNumber.Text;
        }


        private void cmbGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            Show_Group(cmbGroup.SelectedIndex);
        }

        List<int> lstGroupNumber = new List<int>();
        List<int> lstGroupName = new List<int>();
        private void tabPage9_Enter(object sender, EventArgs e)
        {
            //Refresh_Group(-1);
            Make_GroupInfo();
            if (cmbGroup.Items.Count > 0)
            {
                if (cmbGroup.SelectedIndex < 0) cmbGroup.SelectedIndex = 0;
                Show_Group(cmbGroup.SelectedIndex);
            }

            txtRobotName.Text = m_C3d.m_CHeader.strModelName;
            txtModelNumber.Text = m_C3d.m_CHeader.strModelNum = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.nModelNum);
            txtJson_Qr.Text = String.Format("{0}", m_C3d.m_CHeader.SJson.nQr);
            txtJson_Scene.Text = String.Format("{0},{1},{2},{3},{4},{5}",
                m_C3d.m_CHeader.SJson.afScene_Position[0],
                m_C3d.m_CHeader.SJson.afScene_Position[1],
                m_C3d.m_CHeader.SJson.afScene_Position[2],
                m_C3d.m_CHeader.SJson.afScene_Position[3],
                m_C3d.m_CHeader.SJson.afScene_Position[4],
                m_C3d.m_CHeader.SJson.afScene_Position[5]
                );
            txtJson_Camera.Text = String.Format("{0},{1},{2},{3},{4},{5}",
                m_C3d.m_CHeader.SJson.afCamera_Position[0],
                m_C3d.m_CHeader.SJson.afCamera_Position[1],
                m_C3d.m_CHeader.SJson.afCamera_Position[2],
                m_C3d.m_CHeader.SJson.afCamera_Position[3],
                m_C3d.m_CHeader.SJson.afCamera_Position[4],
                m_C3d.m_CHeader.SJson.afCamera_Position[5]
                );
            txtJson_Robot.Text = String.Format("{0},{1},{2},{3},{4},{5}",
                m_C3d.m_CHeader.SJson.afRobot_Position[0],
                m_C3d.m_CHeader.SJson.afRobot_Position[1],
                m_C3d.m_CHeader.SJson.afRobot_Position[2],
                m_C3d.m_CHeader.SJson.afRobot_Position[3],
                m_C3d.m_CHeader.SJson.afRobot_Position[4],
                m_C3d.m_CHeader.SJson.afRobot_Position[5]
                );
            txtJson_Robot_Scale.Text = String.Format("{0}", m_C3d.m_CHeader.SJson.fRobot_Scale);
            txtJson_BackColor.Text = String.Format("0x{0}", Ojw.CConvert.IntToHex(m_C3d.m_CHeader.SJson.cBackColor.ToArgb()));
            txtJson_BackColor_Edit.Text = String.Format("0x{0}", Ojw.CConvert.IntToHex(m_C3d.m_CHeader.SJson.cBackColor_Edit.ToArgb()));
            txtJson_PlaneColor.Text = String.Format("0x{0}", Ojw.CConvert.IntToHex(m_C3d.m_CHeader.SJson.cPlaneColor.ToArgb()));

            txtJson_IsWireFrame.Text = String.Format("{0}", ((m_C3d.m_CHeader.SJson.IsWireFrame == true) ? "1" : "0"));
            txtJson_BleName.Text = m_C3d.m_CHeader.SJson.strBleName;
            txtJson_Address_Motion.Text = String.Format("0x{0}", Ojw.CConvert.IntToHex(m_C3d.m_CHeader.SJson.nAddress_Motion));
            txtJson_Address_Control.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.SJson.nAddress_Control);
        }

        private void Make_GroupInfo()
        {
            cmbGroup.Items.Clear();
            lstGroupNumber.Clear();
            int nID, nGroup;
            List<SGroupInfo_t> lstGroupInfo = new List<SGroupInfo_t>();
            lstGroupInfo.Clear();
            m_C3d.m_CHeader.lstGroupInfo.Clear();
            if (m_C3d.m_CHeader.anMotorIDs != null)
            {
                for (int i = 0; i < m_C3d.m_CHeader.anMotorIDs.Length; i++)
                {
                    nID = m_C3d.m_CHeader.anMotorIDs[i];
                    nGroup = m_C3d.m_CHeader.pSMotorInfo[nID].nGroupNumber;
                    if (nGroup > 0)
                    {
                        if (lstGroupNumber.IndexOf(nGroup) < 0)
                        {
                            lstGroupNumber.Add(nGroup);
                            cmbGroup.Items.Add(String.Format("Group[{0}]", nGroup));

                            SGroupInfo_t SGroup = m_C3d.Get_GroupInfo(nGroup);
                            if (SGroup.nNumber >= 0)
                            {

                            }
                            else
                            {
                                SGroup.nNumber = nGroup;
                                SGroup.strName = "";
                            }
                            lstGroupInfo.Add(SGroup);
                        }
                    }
                }
                m_C3d.m_CHeader.lstGroupInfo.AddRange(lstGroupInfo);
            }
        }

        private void Show_Group(int nIndex)
        {
            int nID;
            if (nIndex < m_C3d.m_CHeader.lstGroupInfo.Count)
            {
                txtGroupNumber.Text = Ojw.CConvert.IntToStr(m_C3d.m_CHeader.lstGroupInfo[nIndex].nNumber);
                txtGroupName.Text = m_C3d.m_CHeader.lstGroupInfo[nIndex].strName;
            }

            ///////////////////////////////////
            txtGroupInfo.Clear();
            string str = string.Empty;
            //str += "====================================\r\n";
            //str += "====================================\r\n";
            //for (nIndex = 0; nIndex < m_C3d.m_CHeader.lstGroupInfo.Count; nIndex++)
            {
                str += String.Format("Group Number = {0}, Name = {1}\r\nMotors = ",
                    m_C3d.m_CHeader.lstGroupInfo[nIndex].nNumber,
                    m_C3d.m_CHeader.lstGroupInfo[nIndex].strName
                    );
                bool bStart = true;
                for (int i = 0; i < m_C3d.m_CHeader.anMotorIDs.Length; i++)
                {
                    nID = m_C3d.m_CHeader.anMotorIDs[i];
                    if (m_C3d.m_CHeader.pSMotorInfo[nID].nGroupNumber == m_C3d.m_CHeader.lstGroupInfo[nIndex].nNumber)
                    {
                        if (bStart == true) bStart = false;
                        else str += ", ";

                        str += String.Format("{0}[{1}]", nID, Ojw.CConvert.RemoveChar(m_C3d.m_CHeader.pSMotorInfo[nID].strNickName, '\0'));
                    }
                }
                str += "\r\n";
                //str += "*******************\r\n";
            }
            txtGroupInfo.Text = str;
        }

        private void txtGroupName_TextChanged(object sender, EventArgs e)
        {
            //m_C3d.m_CHeader.pSMotorInfo[] = txtGroupName.Text;
            //int nGroup = lstGroupNumber[cmbGroup.SelectedIndex];
            //m_C3d.m_CHeader.lstGroupInfo[nGroup].strName = txtGroupName.Text;

            if ((cmbGroup.SelectedIndex < 0) || (cmbGroup.SelectedIndex >= cmbGroup.Items.Count)) return;
            int nGroup = lstGroupNumber[cmbGroup.SelectedIndex];
            SGroupInfo_t SGroup_Old = m_C3d.Get_GroupInfo(nGroup);
            SGroupInfo_t SGroup = new SGroupInfo_t();
            SGroup.strName = txtGroupName.Text;
            SGroup.nNumber = SGroup_Old.nNumber;
            m_C3d.m_CHeader.lstGroupInfo[cmbGroup.SelectedIndex] = SGroup;
        }

        private void txtGroupNumber_TextChanged(object sender, EventArgs e)
        {
            //return;
            if ((cmbGroup.SelectedIndex < 0) || (cmbGroup.SelectedIndex >= cmbGroup.Items.Count)) return;
            int nGroup = lstGroupNumber[cmbGroup.SelectedIndex];
            SGroupInfo_t SGroup = m_C3d.Get_GroupInfo(nGroup);
            SGroup.nNumber = Ojw.CConvert.StrToInt(txtGroupNumber.Text);
            //m_C3d.m_CHeader.lstGroupInfo[cmbGroup.SelectedIndex] = SGroup;
        }

        private void txtJson_Qr_TextChanged(object sender, EventArgs e)
        {
            m_C3d.m_CHeader.SJson.nQr = Ojw.CConvert.StrToInt(txtJson_Qr.Text);
        }

        private void txtJson_Scene_TextChanged(object sender, EventArgs e)
        {
            string[] pstrPos = txtJson_Scene.Text.Split(',');
            if (pstrPos.Length < 6) return;

            for (int i = 0; i < 6; i++) m_C3d.m_CHeader.SJson.afScene_Position[i] = Ojw.CConvert.StrToFloat(pstrPos[i]);
        }

        private void txtJson_Camera_TextChanged(object sender, EventArgs e)
        {
            string[] pstrPos = txtJson_Camera.Text.Split(',');
            if (pstrPos.Length < 6) return;

            for (int i = 0; i < 6; i++) m_C3d.m_CHeader.SJson.afCamera_Position[i] = Ojw.CConvert.StrToFloat(pstrPos[i]);
        }

        private void txtJson_Robot_TextChanged(object sender, EventArgs e)
        {
            string[] pstrPos = txtJson_Robot.Text.Split(',');
            if (pstrPos.Length < 6) return;

            for (int i = 0; i < 6; i++) m_C3d.m_CHeader.SJson.afRobot_Position[i] = Ojw.CConvert.StrToFloat(pstrPos[i]);
        }

        private void txtJson_Robot_Scale_TextChanged(object sender, EventArgs e)
        {
            m_C3d.m_CHeader.SJson.fRobot_Scale = Ojw.CConvert.StrToFloat(txtJson_Robot_Scale.Text);
        }

        private void txtJson_BackColor_TextChanged(object sender, EventArgs e)
        {
            int nVal = 0;
            string str = txtJson_BackColor.Text;
            if (str.IndexOf("0x") >= 0) nVal = Ojw.CConvert.HexStrToInt(str);
            else nVal = Ojw.CConvert.StrToInt(str);
            //if (Ojw.CConvert.IsDigit(str) == true)
            //{
            //    nVal = Ojw.CConvert.StrToInt(str);
            //}
            //else
            //{
            //    if (str.IndexOf("0x") == 0)
            //    {
            //        nVal = Ojw.CConvert.HexStrToInt(str.Substring(2));
            //    }
            //}
            m_C3d.m_CHeader.SJson.cBackColor = Color.FromArgb(nVal);
        }

        private void txtJson_BackColor_Edit_TextChanged(object sender, EventArgs e)
        {
            int nVal = 0;
            string str = txtJson_BackColor_Edit.Text;
            if (str.IndexOf("0x") >= 0) nVal = Ojw.CConvert.HexStrToInt(str);
            else nVal = Ojw.CConvert.StrToInt(str);
            m_C3d.m_CHeader.SJson.cBackColor_Edit = Color.FromArgb(nVal);
        }

        private void txtJson_PlaneColor_TextChanged(object sender, EventArgs e)
        {
            int nVal = 0;
            string str = txtJson_PlaneColor.Text;
            if (str.IndexOf("0x") >= 0) nVal = Ojw.CConvert.HexStrToInt(str);
            else nVal = Ojw.CConvert.StrToInt(str);
            m_C3d.m_CHeader.SJson.cPlaneColor = Color.FromArgb(nVal);
        }

        private void txtJson_IsWireFrame_TextChanged(object sender, EventArgs e)
        {
            int nTmp = Ojw.CConvert.StrToInt(txtJson_IsWireFrame.Text);
            m_C3d.m_CHeader.SJson.IsWireFrame = (nTmp == 0) ? false : true;
        }

        private void txtJson_BleName_TextChanged(object sender, EventArgs e)
        {
            m_C3d.m_CHeader.SJson.strBleName = txtJson_BleName.Text;
        }

        private void txtJson_Address_Motion_TextChanged(object sender, EventArgs e)
        {
            int nVal = 0;
            string str = txtJson_Address_Motion.Text;
            if (str.IndexOf("0x") >= 0) nVal = Ojw.CConvert.HexStrToInt(str);
            else nVal = Ojw.CConvert.StrToInt(str);
            m_C3d.m_CHeader.SJson.nAddress_Motion = nVal;
        }

        private void txtJson_Address_Control_TextChanged(object sender, EventArgs e)
        {
            int nVal = 0;
            string str = txtJson_Address_Control.Text;
            if (str.IndexOf("0x") >= 0) nVal = Ojw.CConvert.HexStrToInt(str);
            else nVal = Ojw.CConvert.StrToInt(str);
            m_C3d.m_CHeader.SJson.nAddress_Control = nVal;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}
