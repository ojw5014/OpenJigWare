//#define _STL_CW
#define _OLD_PROP
#define _DHF_FILE
//#define _GL_FLAT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using System.IO;

using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform.Windows;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Reflection;

namespace OpenJigWare
{
    partial class Ojw
    {
        #region SetCursor - Disable
#if false
        [DllImport("user32.dll")]
        public static extern int SetCursor(int hCursor);
        private const int IDC_APPSTARTING   = 32650; // Arrow/Wait 화살표/모래시계 
        private const int IDC_ARROW         = 32512; // 화살표 
        private const int IDC_CROSS         = 32515; // 십자가 
        private const int IDC_HAND          = 32649; // 손 
        private const int IDC_HELP          = 32651; // 도움말 
        private const int IDC_IBEAM         = 32513; // 텍스트(빔) 
        private const int IDC_ICON          = 32641; // 아이콘 
        private const int IDC_NO            = 32648; // 원형 
        private const int IDC_SIZE          = 32640; // 크기조정 
        private const int IDC_SIZEALL       = 32646; // 크기조정 
        private const int IDC_SIZENESW      = 32643; // 좌우 크기조정 
        private const int IDC_SIZENS        = 32645; // 세로 크기조정 
        private const int IDC_SIZENWSE      = 32642; // 좌우 크기조정 
        private const int IDC_SIZEWE        = 32644; //Size Width 가로 크기조정 
        private const int IDC_UPARROW       = 32516; // Arrow(Up)
        private const int IDC_WAIT          = 32541; // Wait 대기
#endif
        #endregion SetCursor
        public class C3d : SimpleOpenGlControl
        {
            #region PropertyGrid(Main)
            private CDynamicProperty m_CDynamicProp = new CDynamicProperty();
            #endregion PropertyGrid(Main)

            #region PropertyGrid
            private CProp_User m_CPropAll = new CProp_User();
            private CProperty m_CProperty = new CProperty();
            //private CProp m_CProp_Main = new CProp();
            public void CreateProb(Panel pnProp)
            {
                m_CDynamicProp.Create(pnProp);
                bool bEn = true;

                Prop_Add("Test", "TestGroup", "TestCaption", (bool)bEn, false, true); 


                //m_CDynamicProp.SetEvent_Changed(Prop_PropertyValueChanged);
            }
            public void Prop_SetEvent_Changed(PropertyValueChangedEventHandler FFunction)
            {
                m_CDynamicProp.SetEvent_Changed(FFunction);
            }
            public void Prop_Add(String strName, String strGroup, String strCaption, object value, bool bReadOnly, bool bVisible)
            {
                m_CDynamicProp.Add(strName, strGroup, strCaption, value, bReadOnly, bVisible);
            }
            public void Prop_Remove(String strName)
            {
                m_CDynamicProp.Remove(strName);
            }

            private class CProp_t
            {
                private class CProp_Sub
                {
                    public CProp_Sub()
                    {
                    }
                    ~CProp_Sub()
                    {
                    }
                    public void Create(COjwDisp OjwDisp)
                    {
                        CDisp = OjwDisp;
                    }
                    COjwDisp CDisp = null;
                    private const string strGroup = "[1]Object";
                    
                    #region Item add : Step 2/4
                    // item add : step 2 / 4
                    [DisplayName(m_pstrProp_0),
                    Browsable(true),
                    CategoryAttribute(strGroup),
                    DescriptionAttribute("Axis Number(-1:None, 0~253)")]
                    public int nAxisName { get { return CDisp.nName; } set { CDisp.nName = value; } }

                    [DisplayName(m_pstrProp_1),
                    Browsable(true),
                    CategoryAttribute(strGroup),
                    DescriptionAttribute("Object Color")]
                    public Color cColor { get { return CDisp.cColor; } set { CDisp.cColor = value; } }

                    private CSlider<float> m_fSliderAlpha = new CSlider<float>(1.0f, 0.0f, 1.0f, 0.1f);
                    [DisplayName(m_pstrProp_Alpha),
                    Browsable(true),
                    CategoryAttribute(strGroup),
                    DescriptionAttribute("~1.0 : Default 1.0")]
                    public float fAlpha { get { return CDisp.fAlpha; } set { CDisp.fAlpha = value; } }
                    
                    [DisplayName(m_pstrProp_2),
                    Browsable(true),
                    CategoryAttribute(strGroup),
                    DescriptionAttribute("#0~#14 : Default, and FileName")]
                    public String strDispObject { get { return CDisp.strDispObject; } set { CDisp.strDispObject = value; } }

                    [DisplayName(m_pstrProp_3),
                    Browsable(true),
                    CategoryAttribute(strGroup),
                    DescriptionAttribute("Fill & Empty")]
                    public bool bFill { get { return CDisp.bFilled; } set { CDisp.bFilled = value; } }

                    [DisplayName(m_pstrProp_4),
                    Browsable(false),
                    ReadOnlyAttribute(true),
                    CategoryAttribute(strGroup),
                    DescriptionAttribute("reserve...")]
                    public int nTexture { get { return CDisp.nTexture; } set { CDisp.nTexture = value; } }

                    [DisplayName(m_pstrProp_5),
                    Browsable(true),
                    CategoryAttribute(strGroup),
                    DescriptionAttribute("Initialize Position & Angle")]
                    public bool bInit { get { return CDisp.bInit; } set { CDisp.bInit = value; } }

                    [DisplayName(m_pstrProp_6),
                    Browsable(true),
                    CategoryAttribute(strGroup),
                    DescriptionAttribute("")]
                    public float fWidth_Or_Radius { get { return CDisp.fWidth_Or_Radius; } set { CDisp.fWidth_Or_Radius = value; } }

                    [DisplayName(m_pstrProp_7), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public float fHeight_Or_Depth { get { return CDisp.fHeight_Or_Depth; } set { CDisp.fHeight_Or_Depth = value; } }
                    [DisplayName(m_pstrProp_8), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public float fDepth_Or_Cnt { get { return CDisp.fDepth_Or_Cnt; } set { CDisp.fDepth_Or_Cnt = value; } }
                    [DisplayName(m_pstrProp_9), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public float fThickness { get { return CDisp.fThickness; } set { CDisp.fThickness = value; } }
                    [DisplayName(m_pstrProp_10), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public float fGap { get { return CDisp.fGap; } set { CDisp.fGap = value; } }
                    [DisplayName(m_pstrProp_11), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public string strCaption { get { return CDisp.strCaption; } set { CDisp.strCaption = value; } }
                    [DisplayName(m_pstrProp_12), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public int nAxisMoveType { get { return CDisp.nAxisMoveType; } set { CDisp.nAxisMoveType = value; } }
                    [DisplayName(m_pstrProp_13), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public int nDir { get { return CDisp.nDir; } set { CDisp.nDir = value; } }
                    [DisplayName(m_pstrProp_14), Browsable(true), ReadOnlyAttribute(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public float fAngle { get { return CDisp.fAngle; } set { CDisp.fAngle = value; } }
                    [DisplayName(m_pstrProp_15), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public float fAngle_Offset { get { return CDisp.fAngle_Offset; } set { CDisp.fAngle_Offset = value; } }

                    private const string strGroup1 = "[2]Offset Rotation/Translation";
                    [DisplayName(m_pstrProp_16), Browsable(true), CategoryAttribute(strGroup1), DescriptionAttribute("Offset Translation"), TypeConverter(typeof(CVector3DConvert))]
                    public SVector3D_t SOffset_Trans { get { return CDisp.SOffset_Trans; } set { CDisp.SOffset_Trans = value; } }
                    public class CVector3DConvert : TypeConverter
                    {
                        //// http://kindtis.tistory.com/458 참조
                        // string 으로 부터 변환이 가능한가?
                        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                        {
                            if (sourceType == typeof(string))
                                return true;
                            return base.CanConvertFrom(context, sourceType);
                        }

                        // string 으로 부터 vector3로 변환
                        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
                        {
                            if (value is string)
                            {
                                string[] v = ((string)value).Split(new char[] { ',' });
                                return new SVector3D_t(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
                            }
                            return base.ConvertFrom(context, culture, value);
                        }

                        // vector3 에서 string으로 변환
                        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
                        {
                            if (destinationType == typeof(string))
                                return ((SVector3D_t)value).x + "," + ((SVector3D_t)value).y + "," + ((SVector3D_t)value).z;
                            return base.ConvertTo(context, culture, value, destinationType);
                        }
                    }
                    public class CAngle3DConvert : TypeConverter
                    {
                        //// http://kindtis.tistory.com/458 참조
                        // string 으로 부터 변환이 가능한가?
                        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                        {
                            if (sourceType == typeof(string))
                                return true;
                            return base.CanConvertFrom(context, sourceType);
                        }

                        // string 으로 부터 angle3로 변환
                        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
                        {
                            if (value is string)
                            {
                                string[] v = ((string)value).Split(new char[] { ',' });
                                return new SAngle3D_t(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
                            }
                            return base.ConvertFrom(context, culture, value);
                        }

                        // angle3 에서 string으로 변환
                        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
                        {
                            if (destinationType == typeof(string))
                                return ((SAngle3D_t)value).pan + "," + ((SAngle3D_t)value).tilt + "," + ((SAngle3D_t)value).swing;
                            return base.ConvertTo(context, culture, value, destinationType);
                        }
                    }
                    public class CPoint3DConvert : TypeConverter
                    {
                        //// http://kindtis.tistory.com/458 참조
                        // string 으로 부터 변환이 가능한가?
                        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                        {
                            if (sourceType == typeof(string))
                                return true;
                            return base.CanConvertFrom(context, sourceType);
                        }

                        // string 으로 부터 Point3d로 변환
                        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
                        {
                            if (value is string)
                            {
                                string[] v = ((string)value).Split(new char[] { ',' });
                                return new SPoint3D_t(int.Parse(v[0]), int.Parse(v[1]), int.Parse(v[2]));
                            }
                            return base.ConvertFrom(context, culture, value);
                        }

                        // Point3d 에서 string으로 변환
                        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
                        {
                            if (destinationType == typeof(string))
                                return ((SPoint3D_t)value).x + "," + ((SPoint3D_t)value).y + "," + ((SPoint3D_t)value).z;
                            return base.ConvertTo(context, culture, value, destinationType);
                        }
                    }

                    private const string strGroup2 = strGroup1;//"[2]Offset Rotation";
                    [DisplayName(m_pstrProp_19), Browsable(true), CategoryAttribute(strGroup2), DescriptionAttribute("Offset Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                    public SAngle3D_t SOffset_Rot { get { return CDisp.SOffset_Rot; } set { CDisp.SOffset_Rot = value; } }

                    private const string strGroup3 = "[3]Rotation/Translation";
                    [DisplayName(m_pstrProp_22), Browsable(true), CategoryAttribute(strGroup3), DescriptionAttribute("1\'st Translation"), TypeConverter(typeof(CVector3DConvert))]
                    public SVector3D_t STrans_1 { get { return CDisp.afTrans[0]; } set { CDisp.afTrans[0] = value; } }

                    private const string strGroup4 = strGroup3;//"[3]1\'st Rotation";
                    [DisplayName(m_pstrProp_25), Browsable(true), CategoryAttribute(strGroup4), DescriptionAttribute("1\'st Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                    public SAngle3D_t SRot_1 { get { return CDisp.afRot[0]; } set { CDisp.afRot[0] = value; } }

                    private const string strGroup5 = strGroup3;//"[4]2\'st Translation";
                    [DisplayName(m_pstrProp_28), Browsable(true), CategoryAttribute(strGroup5), DescriptionAttribute("2\'st Translation"), TypeConverter(typeof(CVector3DConvert))]
                    public SVector3D_t STrans_2 { get { return CDisp.afTrans[1]; } set { CDisp.afTrans[1] = value; } }

                    private const string strGroup6 = strGroup3;//"[4]2\'st Rotation";
                    [DisplayName(m_pstrProp_31), Browsable(true), CategoryAttribute(strGroup6), DescriptionAttribute("2\'st Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                    public SAngle3D_t SRot_2 { get { return CDisp.afRot[1]; } set { CDisp.afRot[1] = value; } }

                    private const string strGroup7 = strGroup3;//"[5]3\'st Translation";
                    [DisplayName(m_pstrProp_34), Browsable(true), CategoryAttribute(strGroup7), DescriptionAttribute("3\'st Translation"), TypeConverter(typeof(CVector3DConvert))]
                    public SVector3D_t STrans_3 { get { return CDisp.afTrans[2]; } set { CDisp.afTrans[2] = value; } }

                    private const string strGroup8 = strGroup3;//"[5]3\'st Rotation";
                    [DisplayName(m_pstrProp_37), Browsable(true), CategoryAttribute(strGroup8), DescriptionAttribute("3\'st Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                    public SAngle3D_t SRot_3 { get { return CDisp.afRot[2]; } set { CDisp.afRot[2] = value; } }

                    private const string strGroup9 = strGroup3;//"[6]4\'st Translation";
                    [DisplayName(m_pstrProp_40), Browsable(true), CategoryAttribute(strGroup9), DescriptionAttribute("4\'st Translation"), TypeConverter(typeof(CVector3DConvert))]
                    public SVector3D_t STrans_4 { get { return CDisp.afTrans[3]; } set { CDisp.afTrans[3] = value; } }

                    private const string strGroup10 = strGroup3;//"[6]4\'st Rotation";
                    [DisplayName(m_pstrProp_43), Browsable(true), CategoryAttribute(strGroup10), DescriptionAttribute("4\'st Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                    public SAngle3D_t SRot_4 { get { return CDisp.afRot[3]; } set { CDisp.afRot[3] = value; } }

                    private const string strGroup11 = strGroup3;//"[7]5\'st Translation";
                    [DisplayName(m_pstrProp_46), Browsable(true), CategoryAttribute(strGroup11), DescriptionAttribute("5\'st Translation"), TypeConverter(typeof(CVector3DConvert))]
                    public SVector3D_t STrans_5 { get { return CDisp.afTrans[4]; } set { CDisp.afTrans[4] = value; } }

                    private const string strGroup12 = strGroup3;//"[7]5\'st Rotation";
                    [DisplayName(m_pstrProp_49), Browsable(true), CategoryAttribute(strGroup12), DescriptionAttribute("5\'st Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                    public SAngle3D_t SRot_5 { get { return CDisp.afRot[4]; } set { CDisp.afRot[4] = value; } }

                    [DisplayName(m_pstrProp_52), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute(""), TypeConverter(typeof(CPoint3DConvert))]
                    public SPoint3D_t SPickGroup { get { return new SPoint3D_t(CDisp.nPickGroup_A, CDisp.nPickGroup_B, CDisp.nPickGroup_C); } set { CDisp.nPickGroup_A = value.x; CDisp.nPickGroup_B = value.y; CDisp.nPickGroup_C = value.z; } }

                    [DisplayName(m_pstrProp_55), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public int nInverseKinematicsNumber { get { return CDisp.nInverseKinematicsNumber; } set { CDisp.nInverseKinematicsNumber = value; } }
                    [DisplayName(m_pstrProp_56), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public float fScale_Serve0 { get { return CDisp.fScale_Serve0; } set { CDisp.fScale_Serve0 = value; } }
                    [DisplayName(m_pstrProp_57), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public float fScale_Serve1 { get { return CDisp.fScale_Serve1; } set { CDisp.fScale_Serve1 = value; } }
                    [DisplayName(m_pstrProp_58), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public int nMotorType { get { return CDisp.nMotorType; } set { CDisp.nMotorType = value; } }
                    [DisplayName(m_pstrProp_59), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public int nMotorControl_MousePoint { get { return CDisp.nMotorControl_MousePoint; } set { CDisp.nMotorControl_MousePoint = value; } }
                    [DisplayName(m_pstrProp_60), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    public String strPickGroup_Comment { get { return CDisp.strPickGroup_Comment; } set { CDisp.strPickGroup_Comment = value; } }

                    //// if you want to add , just do like below ////

                    //[DisplayName(m_pstrProp_), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                    //public { get { return CDisp.; } set { CDisp. = value; } }

                    #endregion Item add : Step 2/4
                    #region Set
                
                
                    #endregion Set
                }
            }

            public void VisibleProp(String strName, bool bVisible) { m_CPropAll.Visible(strName, bVisible); }
            public void CreateProb_VirtualObject(Panel pnProp)
            {
                m_CPropAll.Create(OjwVirtualDisp);//m_CDisp);
                m_CProperty.Create(pnProp, m_CPropAll);//m_CProp_Main);
                m_CProperty.SetEvent_Changed(Prop_PropertyValueChanged);
            }
            private void CheckObjectModelFile(String strName)
            {
                Ojw.C3d.COjwDisp CDisp = GetVirtualClass_Data();
                CDisp.strDispObject = strName;
                // if you never loaded this file. it would be loaded
#if false
                if (OjwAse_GetIndex(CDisp.strDispObject) < 0)
                {
                    this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
                    if (OjwFileOpen_3D_ASE(m_strOrgDirectory + "\\ase\\" + CDisp.strDispObject + ".ase") == false)
                    {
                        Ojw.CMessage.Write_Error("ASE(" + CDisp.strDispObject + ".ase" + ") File Loading Error");
                    }
                    this.Cursor = System.Windows.Forms.Cursors.Default;
                }
#else
                if (CDisp.strDispObject.Length > 0)
                {
                    if (CDisp.strDispObject.IndexOf('#') < 0)
                    {
                        if (OjwAse_GetIndex(CDisp.strDispObject) < 0)
                        {
                            //this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
#if true
                            String strFileName = Application.StartupPath.Trim('\\') + GetAseFile_Path() + CDisp.strDispObject + ((CDisp.strDispObject.IndexOf('.') < 0) ? ".ase" : "");
                            FileInfo f = new FileInfo(strFileName);
                            if (f.Exists == true)
                            {
                                if (CFile.GetExe(strFileName).ToUpper() == "ASE")
                                    OjwFileOpen_3D_ASE(strFileName);
                                else if (CFile.GetExe(strFileName).ToUpper() == "STL")
                                    OjwFileOpen_3D_STL(strFileName);
                            }
#else
                            String strFileName = Application.StartupPath + "\\" + GetAseFile_Path() + CDisp.strDispObject + ".ase";
                            FileInfo f = new FileInfo(strFileName);
                            if (f.Exists == true) OjwFileOpen_3D_ASE(strFileName);
                            else Ojw.CMessage.Write_Error("ASE(" + CDisp.strDispObject + ((CDisp.strDispObject.IndexOf(",") < 0) ? ".ase" : "") + ") File Loading Error");
#endif
                            //this.Cursor = System.Windows.Forms.Cursors.Default;
                        }
                    }
                }
#endif
            }
            private void Prop_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
            {
                // item add : step 4 / 4
                switch(e.ChangedItem.Label)
                {
                    #region Main
#if _OLD_PROP
                    //case strProp_Main0:
                        //break;
                    case m_strProp_Main1:
                        Prop_Set_Main_MouseControlMode((int)e.ChangedItem.Value);
                        break;
                    case m_strProp_Main2:
                        Prop_Set_Main_Empty((bool)e.ChangedItem.Value);
                        break;
                    case m_strProp_Main3:
                        Prop_Set_Main_Alpha((float)e.ChangedItem.Value);
                        break;
                    case m_strProp_Main4:
                        Prop_Set_Main_Light((bool)e.ChangedItem.Value);
                        break;
                    case m_strProp_Main5:
                        Prop_Set_Main_ShowStandardAxis((bool)e.ChangedItem.Value);
                        break;
                    case m_strProp_Main6:
                        Prop_Set_Main_ShowVirtualAxis((bool)e.ChangedItem.Value);
                        break;
                    case m_strProp_Main7:
                        Prop_Set_Main_DefaultFunctionNum((int)e.ChangedItem.Value);
                        break;
                //        case strProp_Main0 = "Version";
                //private const String strProp_Main1 = "Mouse Mode";
                //private const String strProp_Main2 = "Empty";
                //private const String strProp_Main3 = "Alpha(Main)";
#endif
                    #endregion Main
                    #region Sub
                    #region AxisName
                    case m_pstrProp_0:
                        Prop_Set_Name((int)e.ChangedItem.Value);
                        break;
                    #endregion AxisName
                    #region Color
                    case m_pstrProp_1:                        
                        Prop_Set_Color((Color)e.ChangedItem.Value);
                        break;
                    #endregion Color
                    #region Model
                    case m_pstrProp_2: // Model                        
                        Prop_Set_DispObject((String)e.ChangedItem.Value);
                        break;
                    #endregion Model
                    #region Fill
                    case m_pstrProp_3: // Fill                        
                        Prop_Set_Fill((bool)e.ChangedItem.Value);
                        break;
                    #endregion Fill
                    #region Texture
                    case m_pstrProp_4: // Texture                        
                        Prop_Set_Texture((int)e.ChangedItem.Value);
                        break;
                    #endregion Texture
                    #region Init
                    case m_pstrProp_5: // Init
                        Prop_Set_Init((bool)e.ChangedItem.Value);
                        break;
                    #endregion Init
                    #region Width or Radius
                    case m_pstrProp_6:
                        Prop_Set_Width_Or_Radius((float)e.ChangedItem.Value);
                        break;
                    #endregion Width or Radius
                    #region Width or Radius
                    case m_pstrProp_7:
                        Prop_Set_Height_Or_Depth((float)e.ChangedItem.Value);
                        break;
                    #endregion Width or Radius
                    #region Depth or Cnt
                    case m_pstrProp_8:
                        Prop_Set_Depth_Or_Cnt((float)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region Thickness
                    case m_pstrProp_9:
                        Prop_Set_Thickness((float)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region fGap
                    case m_pstrProp_10:
                        Prop_Set_Gap((float)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region strCaption
                    case m_pstrProp_11:
                        Prop_Set_Caption((string)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region nAxisMoveType
                    case m_pstrProp_12:
                        Prop_Set_AxisMoveType((int)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region nDir
                    case m_pstrProp_13:
                        Prop_Set_Dir((int)e.ChangedItem.Value);
                        break;
                    #endregion Dir
                    #region fAngle
                    case m_pstrProp_14:
                        Prop_Set_Angle((float)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region fAngle_Offset
                    case m_pstrProp_15:
                        Prop_Set_Angle_Offset((float)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region Offset_Trans
                    case m_pstrProp_16:
                        Prop_Set_Offset_Trans((SVector3D_t)e.ChangedItem.Value);
                        break;
                    //case m_pstrProp_17:
                    //    Prop_Set_Offset_Trans_Y((float)e.ChangedItem.Value);
                    //    break;
                    //case m_pstrProp_18:
                    //    Prop_Set_Offset_Trans_Z((float)e.ChangedItem.Value);
                    //    break;
                    #endregion
                    #region Offset_Rot
                    case m_pstrProp_19:
                        ;
                        Prop_Set_Offset_Rot((SAngle3D_t)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region Trans/Rot 1st
                    case m_pstrProp_22:
                        Prop_Set_Trans_1((SVector3D_t)e.ChangedItem.Value);
                        break;
                    case m_pstrProp_25:
                        Prop_Set_Rot_1((SAngle3D_t)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region Trans/Rot 2st
                    case m_pstrProp_28:
                        Prop_Set_Trans_2((SVector3D_t)e.ChangedItem.Value);
                        break;
                    case m_pstrProp_31:
                        Prop_Set_Rot_2((SAngle3D_t)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region Trans/Rot 3st
                    case m_pstrProp_34:
                        Prop_Set_Trans_3((SVector3D_t)e.ChangedItem.Value);
                        break;
                    case m_pstrProp_37:
                        Prop_Set_Rot_3((SAngle3D_t)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region region Trans/Rot 4st
                    case m_pstrProp_40:
                        Prop_Set_Trans_4((SVector3D_t)e.ChangedItem.Value);
                        break;
                    case m_pstrProp_43:
                        Prop_Set_Rot_4((SAngle3D_t)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region region Trans/Rot 5st
                    case m_pstrProp_46:
                        Prop_Set_Trans_5((SVector3D_t)e.ChangedItem.Value);
                        break;
                    case m_pstrProp_49:
                        Prop_Set_Rot_5((SAngle3D_t)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region nPickGroup
                    case m_pstrProp_52:
                        Prop_Set_PickGroup((SPoint3D_t)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region InverseKinematicsNumber
                    case m_pstrProp_55:
                        Prop_Set_InverseKinematicsNumber((int)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region Scale_Serve
                    case m_pstrProp_56:                        
                        Prop_Set_Scale_Serve0((float)e.ChangedItem.Value);
                        break;
                    case m_pstrProp_57:
                        Prop_Set_Scale_Serve1((float)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region MotorType
                    case m_pstrProp_58:
                        Prop_Set_MotorType((int)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region MotorControl_MousePoint
                    case m_pstrProp_59:
                        Prop_Set_MotorControl_MousePoint((int)e.ChangedItem.Value);
                        break;
                    #endregion
                    #region PickGroup_Comment
                    case m_pstrProp_60:
                        Prop_Set_PickGroup_Comment((string)e.ChangedItem.Value);
                        break;
                    #endregion
                    //case m_pstrProp_:
                    //    OjwVirtualDisp. = ()e.ChangedItem.Value;
                    //    Prop_Set_(OjwVirtualDisp.);
                    //    break;
                    //case m_pstrProp_:
                    //    OjwVirtualDisp. = ()e.ChangedItem.Value;
                    //    Prop_Set_(OjwVirtualDisp.);
                    //    break;
                    #endregion Sub
                }
                Prop_Update_VirtualObject();
            }
            public void Prop_SetServe_Test(int nTest) 
            {
                //TypeDescriptor.GetProperties(m_CPropAll)["OpenJigWare"].SetValue(m_CPropAll.nTest, nTest);
                //m_CPropAll.nTest = nTest;
                m_CProperty.Update();
            }
            public void Prop_Update_VirtualObject() { m_CProperty.Update(); }

            #region Item add : Step 3/4
            // Main
            #region Main
#if _OLD_PROP
            public void Prop_Set_Main_MouseControlMode(int value) { m_CPropAll.nMain_MouseMode = m_nMouseControlMode = value; }
            public int Prop_Get_Main_MouseControlMode() { return m_CPropAll.nMain_MouseMode; }

            public void Prop_Set_Main_Empty(bool value) { m_CPropAll.bMain_Empty = m_bEmptyBody = value; }
            public bool Prop_Get_Main_Empty() { return m_CPropAll.bMain_Empty; }

            public void Prop_Set_Main_Alpha(float value) { m_CPropAll.fMain_Alpha = value; SetAlpha(value); }
            public float Prop_Get_Main_Alpha() { return m_CPropAll.fMain_Alpha; }

            public void Prop_Set_Main_Light(bool value) { m_CPropAll.bMain_Light = m_bEnable_Light = value; }
            public bool Prop_Get_Main_Light() { return m_CPropAll.bMain_Light; }


            public void Prop_Set_Main_ShowStandardAxis(bool value) { m_CPropAll.bMain_ShowStandardAxis = m_bStandardAxis = value; }
            public bool Prop_Get_Main_ShowStandardAxis() { return m_CPropAll.bMain_ShowStandardAxis; }

            public void Prop_Set_Main_ShowVirtualAxis(bool value) { m_CPropAll.bMain_ShowVirtualAxis = m_bVirtualClass = value; }
            public bool Prop_Get_Main_ShowVirtualAxis() { return m_CPropAll.bMain_ShowVirtualAxis; }

            //public void Prop_Set_Main_DefaultFunctionNum(int value) { m_CPropAll.nMain_DefaultFunctionNum = m_nFunctionNumber = value; }
            public void Prop_Set_Main_DefaultFunctionNum(int value) { m_CPropAll.nMain_DefaultFunctionNum = m_CHeader.nDefaultFunctionNumber = value; }
            public int Prop_Get_Main_DefaultFunctionNum() { return m_CPropAll.nMain_DefaultFunctionNum; }
#endif
            #endregion Main
            // item add : step 3 / 4
            #region Set
            public void Prop_Set_Name(int value) { m_CPropAll.nAxisName = OjwVirtualDisp.nName = value; }
            public void Prop_Set_Color(Color value) { m_CPropAll.cColor = OjwVirtualDisp.cColor = value; }
            public void Prop_Set_DispAlpha(float value) { m_CPropAll.fAlpha = OjwVirtualDisp.fAlpha = value; }
            public void Prop_Set_DispObject(string value) { m_CPropAll.strDispObject = OjwVirtualDisp.strDispObject = value; CheckObjectModelFile(m_CPropAll.strDispObject); }
            public void Prop_Set_Fill(bool value) { m_CPropAll.bFill = OjwVirtualDisp.bFilled = value; }
            public void Prop_Set_Texture(int value) { m_CPropAll.nTexture = OjwVirtualDisp.nTexture = value; }
            public void Prop_Set_Init(bool value) { m_CPropAll.bInit = OjwVirtualDisp.bInit = value; }
            public void Prop_Set_Width_Or_Radius(float value) { m_CPropAll.fWidth_Or_Radius = OjwVirtualDisp.fWidth_Or_Radius = value; }
            public void Prop_Set_Height_Or_Depth(float value) { m_CPropAll.fHeight_Or_Depth = OjwVirtualDisp.fHeight_Or_Depth = value; }
            public void Prop_Set_Depth_Or_Cnt(float value) { m_CPropAll.fDepth_Or_Cnt = OjwVirtualDisp.fDepth_Or_Cnt = value; }
            public void Prop_Set_Thickness(float value) { m_CPropAll.fThickness = OjwVirtualDisp.fThickness = value; }
            public void Prop_Set_Gap(float value) { m_CPropAll.fGap = OjwVirtualDisp.fGap = value; }
            public void Prop_Set_Caption(string value) { m_CPropAll.strCaption = OjwVirtualDisp.strCaption = value; }

            public void Prop_Set_AxisMoveType(int value) { m_CPropAll.nAxisMoveType = OjwVirtualDisp.nAxisMoveType = value; }
            public void Prop_Set_Dir(int value) { m_CPropAll.nDir = OjwVirtualDisp.nDir = value; }
            public void Prop_Set_Angle(float value) { m_CPropAll.fAngle = OjwVirtualDisp.fAngle = value; }
            public void Prop_Set_Angle_Offset(float value) { m_CPropAll.fAngle_Offset = OjwVirtualDisp.fAngle_Offset = value; }
            //public void Prop_Set_Offset_Trans_X(float value) { m_CPropAll.SOffset_Trans_X = OjwVirtualDisp.SOffset_Trans.x = value; }
            public void Prop_Set_Offset_Trans(SVector3D_t value) { m_CPropAll.SOffset_Trans = OjwVirtualDisp.SOffset_Trans = value; }
            public void Prop_Set_Offset_Rot(SAngle3D_t value) { m_CPropAll.SOffset_Rot = OjwVirtualDisp.SOffset_Rot = value; }

            public void Prop_Set_Trans_1(SVector3D_t value) { m_CPropAll.STrans_1 = OjwVirtualDisp.afTrans[0] = value; }
            public void Prop_Set_Rot_1(SAngle3D_t value) { m_CPropAll.SRot_1 = OjwVirtualDisp.afRot[0] = value; }

            public void Prop_Set_Trans_2(SVector3D_t value) { m_CPropAll.STrans_2 = OjwVirtualDisp.afTrans[1] = value; }
            public void Prop_Set_Rot_2(SAngle3D_t value) { m_CPropAll.SRot_2 = OjwVirtualDisp.afRot[1] = value; }

            public void Prop_Set_Trans_3(SVector3D_t value) { m_CPropAll.STrans_3 = OjwVirtualDisp.afTrans[2] = value; }
            public void Prop_Set_Rot_3(SAngle3D_t value) { m_CPropAll.SRot_3 = OjwVirtualDisp.afRot[2] = value; }

            public void Prop_Set_Trans_4(SVector3D_t value) { m_CPropAll.STrans_4 = OjwVirtualDisp.afTrans[3] = value; }
            public void Prop_Set_Rot_4(SAngle3D_t value) { m_CPropAll.SRot_4 = OjwVirtualDisp.afRot[3] = value; }
            
            public void Prop_Set_Trans_5(SVector3D_t value) { m_CPropAll.STrans_5 = OjwVirtualDisp.afTrans[4] = value; }
            public void Prop_Set_Rot_5(SAngle3D_t value) { m_CPropAll.SRot_5 = OjwVirtualDisp.afRot[4] = value; }

            public void Prop_Set_PickGroup(SPoint3D_t value) { m_CPropAll.SPickGroup = value; OjwVirtualDisp.nPickGroup_A = value.x; OjwVirtualDisp.nPickGroup_B = value.y; OjwVirtualDisp.nPickGroup_C = value.z; }
            
            public void Prop_Set_InverseKinematicsNumber(int value) { m_CPropAll.nInverseKinematicsNumber = OjwVirtualDisp.nInverseKinematicsNumber = value; }
            public void Prop_Set_Scale_Serve0(float value) { m_CPropAll.fScale_Serve0 = OjwVirtualDisp.fScale_Serve0 = value; }
            public void Prop_Set_Scale_Serve1(float value) { m_CPropAll.fScale_Serve1 = OjwVirtualDisp.fScale_Serve1 = value; }
            public void Prop_Set_MotorType(int value) { m_CPropAll.nMotorType = OjwVirtualDisp.nMotorType = value; }
            public void Prop_Set_MotorControl_MousePoint(int value) { m_CPropAll.nMotorControl_MousePoint = OjwVirtualDisp.nMotorControl_MousePoint = value; }
            public void Prop_Set_PickGroup_Comment(string value) { m_CPropAll.strPickGroup_Comment = OjwVirtualDisp.strPickGroup_Comment = value; }

            //// if you want to add , just do like below ////

            //public void Prop_Set_( value) { m_CPropAll. = value; }            
            #endregion Set

            #region Get
            public int Prop_Get_Name() { return m_CPropAll.nAxisName; }
            public Color Prop_Get_Color() { return m_CPropAll.cColor; }
            public float Prop_Get_DispAlpha() { return m_CPropAll.fAlpha; }
            public string Prop_Get_DispObject() { return m_CPropAll.strDispObject; }
            public bool Prop_Get_Fill() { return m_CPropAll.bFill; }
            public int Prop_Get_Texture() { return m_CPropAll.nTexture; }
            public bool Prop_Get_Init() { return m_CPropAll.bInit; }
            public float Prop_Get_Width_Or_Radius() { return m_CPropAll.fWidth_Or_Radius; }
            public float Prop_Get_Height_Or_Depth() { return m_CPropAll.fHeight_Or_Depth; }
            public float Prop_Get_Depth_Or_Cnt() { return m_CPropAll.fDepth_Or_Cnt; }
            public float Prop_Get_Thickness() { return m_CPropAll.fThickness; }
            public float Prop_Get_Gap() { return m_CPropAll.fGap; }
            public string Prop_Get_Caption() { return m_CPropAll.strCaption; }
            public int Prop_Get_AxisMoveType() { return m_CPropAll.nAxisMoveType; }
            public int Prop_Get_Dir() { return m_CPropAll.nDir; }
            public float Prop_Get_Angle() { return m_CPropAll.fAngle; }
            public float Prop_Get_Angle_Offset() { return m_CPropAll.fAngle_Offset; }
            public SVector3D_t Prop_Get_Offset_Trans_X() { return m_CPropAll.SOffset_Trans; }
            public SAngle3D_t Prop_Get_Offset_Rot_Pan() { return m_CPropAll.SOffset_Rot; }

            public SVector3D_t Prop_Get_Trans_X1() { return m_CPropAll.STrans_1; }
            public SAngle3D_t Prop_Get_Rot_Pan1() { return m_CPropAll.SRot_1; }

            public SVector3D_t Prop_Get_Trans_X2() { return m_CPropAll.STrans_2; }
            public SAngle3D_t Prop_Get_Rot_Pan2() { return m_CPropAll.SRot_2; }

            public SVector3D_t Prop_Get_Trans_X3() { return m_CPropAll.STrans_3; }
            public SAngle3D_t Prop_Get_Rot_Pan3() { return m_CPropAll.SRot_3; }

            public SVector3D_t Prop_Get_Trans_X4() { return m_CPropAll.STrans_4; }
            public SAngle3D_t Prop_Get_Rot_Pan4() { return m_CPropAll.SRot_4; }

            public SVector3D_t Prop_Get_Trans_X5() { return m_CPropAll.STrans_5; }
            public SAngle3D_t Prop_Get_Rot_Pan5() { return m_CPropAll.SRot_5; }
            public SPoint3D_t Prop_Get_PickGroup_A() { return m_CPropAll.SPickGroup; }
            public int Prop_Get_InverseKinematicsNumber() { return m_CPropAll.nInverseKinematicsNumber; }
            public float Prop_Get_Scale_Serve0() { return m_CPropAll.fScale_Serve0; }
            public float Prop_Get_Scale_Serve1() { return m_CPropAll.fScale_Serve1; }
            public int Prop_Get_MotorType() { return m_CPropAll.nMotorType; }
            public int Prop_Get_MotorControl_MousePoint() { return m_CPropAll.nMotorControl_MousePoint; }
            public string Prop_Get_PickGroup_Comment() { return m_CPropAll.strPickGroup_Comment; }
            #endregion Get
            //// if you want to add , just do like below ////

            //public  Prop_Get_() { return m_CPropAll.; }
            #endregion Item add : Step 3/4
#if true
            #region Prop Name
            #region Item add : Step 1/4
            // item add : step 1 / 4
            private const string m_pstrProp_0 = "Name(Axis Number)";
            private const string m_pstrProp_1 = "Color";
            private const string m_pstrProp_Alpha = "Alpha";
            private const string m_pstrProp_2 = "DispObject";
            private const string m_pstrProp_3 = "Fill";
            private const string m_pstrProp_4 = "Texture";
            private const string m_pstrProp_5 = "Init";
            private const string m_pstrProp_6 = "Width_Or_Radius";
            private const string m_pstrProp_7 = "Height_Or_Depth";
            private const string m_pstrProp_8 = "Depth_Or_Cnt";
            private const string m_pstrProp_9 = "Thickness";
            private const string m_pstrProp_10 = "Gap";
            private const string m_pstrProp_11 = "Caption";
            private const string m_pstrProp_12 = "AxisMoveType";
            private const string m_pstrProp_13 = "Dir";
            private const string m_pstrProp_14 = "Angle";
            private const string m_pstrProp_15 = "Angle_Offset";
            private const string m_pstrProp_16 = "Offset_Trans(X,Y,Z)";
            private const string m_pstrProp_19 = "Offset_Rot(Pan,Tilt,Swing)";
            private const string m_pstrProp_22 = "1st Trans(X,Y,Z)";
            private const string m_pstrProp_25 = "1st Rot(Pan,Tilt,Swing)";
            private const string m_pstrProp_28 = "2st Trans(X,Y,Z)";
            private const string m_pstrProp_31 = "2st Rot(Pan,Tilt,Swing)";
            private const string m_pstrProp_34 = "3st Trans(X,Y,Z)";
            private const string m_pstrProp_37 = "3st Rot(Pan,Tilt,Swing)";
            private const string m_pstrProp_40 = "4st Trans(X,Y,Z)";
            private const string m_pstrProp_43 = "4st Rot(Pan,Tilt,Swing)";
            private const string m_pstrProp_46 = "5st Trans(X,Y,Z)";
            private const string m_pstrProp_49 = "5st Rot(Pan,Tilt,Swing)";
            private const string m_pstrProp_52 = "nPickGroup(A,B,C)";
            private const string m_pstrProp_55 = "nInverseKinematicsNumber";
            private const string m_pstrProp_56 = "fScale_Serve0";
            private const string m_pstrProp_57 = "fScale_Serve1";
            private const string m_pstrProp_58 = "nMotorType";
            private const string m_pstrProp_59 = "nMotorControl_MousePoint";
            private const string m_pstrProp_60 = "strPickGroup_Comment";
            #endregion Item add : Step 1/4
            #endregion Prop Name

            public class CSlider<T>
            {
                public CSlider(T value, T min, T max, T step)
                {
                    Value = value;
                    Min = min;
                    Max = max;
                    Step = step;
                }

                public T Value { get; set; }
                public T Max { get; set; }
                public T Min { get; set; }
                public T Step { get; set; }
            }

            [DefaultPropertyAttribute("OpenJigWare")]
            [RefreshProperties(RefreshProperties.All)]
#if _OLD_PROP
            private  class CProp_User: CProp_Main
#else
            private  class CProp_User
#endif
            {
                public CProp_User()
                {
                    //m_fSliderAlpha.Max = 1.0f;
                    //m_fSliderAlpha.Min = 0.0f;
                    //m_fSliderAlpha.Step = 0.1f;
                }
                ~CProp_User()
                {
                }
                public void Create(COjwDisp OjwDisp)
                {
                    CDisp = OjwDisp;
                }
                public void Visible(String strName, bool bVisible)
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.GetType())[strName];
                    BrowsableAttribute attrib = (BrowsableAttribute)descriptor.Attributes[typeof(BrowsableAttribute)];
                    FieldInfo isBrow = attrib.GetType().GetField("browsable", BindingFlags.NonPublic | BindingFlags.Instance);
                    isBrow.SetValue(attrib, bVisible);
                }

                COjwDisp CDisp = null;
                private const string strGroup = "[1]Object";
                //private int _nTest = 0;
                //[DisplayName("Test"), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("Test Main")]
                //public int nTest { get { return _nTest; } set { _nTest = value; } }
                
                #region Item add : Step 2/4
                // item add : step 2 / 4
                [DisplayName(m_pstrProp_0), 
                Browsable(true), 
                CategoryAttribute(strGroup), 
                DescriptionAttribute("Axis Number(-1:None, 0~253)")]
                public int nAxisName { get { return CDisp.nName; } set { CDisp.nName = value; } }

                [DisplayName(m_pstrProp_1), 
                Browsable(true), 
                CategoryAttribute(strGroup), 
                DescriptionAttribute("Object Color")]
                public Color cColor { get { return CDisp.cColor; } set { CDisp.cColor = value; } }

                private CSlider<float> m_fSliderAlpha = new CSlider<float>(1.0f, 0.0f, 1.0f, 0.1f);
                [DisplayName(m_pstrProp_Alpha), 
                Browsable(true),
                CategoryAttribute(strGroup),
                //Editor(typeof(CSlider<float>), typeof(CSlider<float>)),
                //Editor(typeof(CustomSlider), typeof(PropertyValueEditor)),
                DescriptionAttribute("~1.0 : Default 1.0")]
                public float fAlpha { get { return CDisp.fAlpha; } set { CDisp.fAlpha = value; } }
//                public CSlider<float> fSliderAlpha { get { return m_fSliderAlpha; } set { m_fSliderAlpha = value; } }
//                public class CustomSlider : ExtendedPropertyValueEditor
//                {
//                    public CustomSlider()
//                    {
//                        // Template for normal view
//                        string template1 = @"
//                        <DataTemplate
//                            xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
//                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
//                            xmlns:pe='clr-namespace:System.Activities.Presentation.PropertyEditing;assembly=System.Activities.Presentation' 
//                            xmlns:wpg='clr-namespace:PropertyGrid;assembly=PropertyGrid' > 
//                            <DockPanel LastChildFill='True'>
//                                    <TextBox Text='{Binding Path=Value.Value, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}' Width='40' TextAlignment='Center' />
//                                    <Slider x:Name='slider1' Value='{Binding Path=Value.Value, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}' Margin='2,0,0,0' Minimum='{Binding Value.Min}' Maximum='{Binding Value.Max}' />
//                            </DockPanel>
//                        </DataTemplate>";

//                        // Load templates
//                        using (var sr = new MemoryStream(Encoding.UTF8.GetBytes(template1)))
//                        {
//                            this.InlineEditorTemplate = XamlReader.Load(sr) as DataTemplate;
//                        }
//                    }
//                }
                
                [DisplayName(m_pstrProp_2), 
                Browsable(true), 
                CategoryAttribute(strGroup), 
                DescriptionAttribute("#0~#14 : Default, and FileName")]                
                public String strDispObject { get { return CDisp.strDispObject; } set { CDisp.strDispObject = value; } }

                [DisplayName(m_pstrProp_3), 
                Browsable(true), 
                CategoryAttribute(strGroup), 
                DescriptionAttribute("Fill & Empty")]
                public bool bFill { get { return CDisp.bFilled; } set { CDisp.bFilled = value; } }

                [DisplayName(m_pstrProp_4), 
                Browsable(false), 
                ReadOnlyAttribute(true), 
                CategoryAttribute(strGroup), 
                DescriptionAttribute("reserve...")]
                public int nTexture { get { return CDisp.nTexture; } set { CDisp.nTexture = value; } }

                [DisplayName(m_pstrProp_5),
                Browsable(true), 
                CategoryAttribute(strGroup),
                DescriptionAttribute("Initialize Position & Angle")]
                public bool bInit { get { return CDisp.bInit; } set { CDisp.bInit = value; } }

                [DisplayName(m_pstrProp_6), 
                Browsable(true), 
                CategoryAttribute(strGroup), 
                DescriptionAttribute("")]
                public float fWidth_Or_Radius { get { return CDisp.fWidth_Or_Radius; } set { CDisp.fWidth_Or_Radius = value; } }

                [DisplayName(m_pstrProp_7), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public float fHeight_Or_Depth{ get { return CDisp.fHeight_Or_Depth; } set { CDisp.fHeight_Or_Depth = value; } }
                [DisplayName(m_pstrProp_8), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public float fDepth_Or_Cnt{ get { return CDisp.fDepth_Or_Cnt; } set { CDisp.fDepth_Or_Cnt = value; } }
                [DisplayName(m_pstrProp_9), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public float fThickness{ get { return CDisp.fThickness; } set { CDisp.fThickness = value; } }
                [DisplayName(m_pstrProp_10), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public float fGap{ get { return CDisp.fGap; } set { CDisp.fGap = value; } }
                [DisplayName(m_pstrProp_11), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public string strCaption{ get { return CDisp.strCaption; } set { CDisp.strCaption = value; } }
                [DisplayName(m_pstrProp_12), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public int nAxisMoveType { get { return CDisp.nAxisMoveType; } set { CDisp.nAxisMoveType = value; } }
                [DisplayName(m_pstrProp_13), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public int nDir{ get { return CDisp.nDir; } set { CDisp.nDir = value; } }
                [DisplayName(m_pstrProp_14), Browsable(true), ReadOnlyAttribute(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public float fAngle{ get { return CDisp.fAngle; } set { CDisp.fAngle = value; } }
                [DisplayName(m_pstrProp_15), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public float fAngle_Offset{ get { return CDisp.fAngle_Offset; } set { CDisp.fAngle_Offset = value; } }

                private const string strGroup1 = "[2]Offset Rotation/Translation";
                [DisplayName(m_pstrProp_16), Browsable(true), CategoryAttribute(strGroup1), DescriptionAttribute("Offset Translation"), TypeConverter(typeof(CVector3DConvert))]
                public SVector3D_t SOffset_Trans { get { return CDisp.SOffset_Trans; } set { CDisp.SOffset_Trans = value; } }
                public class CVector3DConvert : TypeConverter
                {
                    //// http://kindtis.tistory.com/458 참조
                    // string 으로 부터 변환이 가능한가?
                    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                    {
                        if (sourceType == typeof(string))
                            return true;
                        return base.CanConvertFrom(context, sourceType);
                    }

                    // string 으로 부터 vector3로 변환
                    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
                    {
                        if (value is string)
                        {
                            string[] v = ((string)value).Split(new char[] { ',' });
                            return new SVector3D_t(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
                        }
                        return base.ConvertFrom(context, culture, value);
                    }

                    // vector3 에서 string으로 변환
                    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
                    {
                        if (destinationType == typeof(string))
                            return ((SVector3D_t)value).x + "," + ((SVector3D_t)value).y + "," + ((SVector3D_t)value).z;
                        return base.ConvertTo(context, culture, value, destinationType);
                    }
                }
                public class CAngle3DConvert : TypeConverter
                {
                    //// http://kindtis.tistory.com/458 참조
                    // string 으로 부터 변환이 가능한가?
                    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                    {
                        if (sourceType == typeof(string))
                            return true;
                        return base.CanConvertFrom(context, sourceType);
                    }

                    // string 으로 부터 angle3로 변환
                    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
                    {
                        if (value is string)
                        {
                            string[] v = ((string)value).Split(new char[] { ',' });
                            return new SAngle3D_t(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
                        }
                        return base.ConvertFrom(context, culture, value);
                    }

                    // angle3 에서 string으로 변환
                    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
                    {
                        if (destinationType == typeof(string))
                            return ((SAngle3D_t)value).pan + "," + ((SAngle3D_t)value).tilt + "," + ((SAngle3D_t)value).swing;
                        return base.ConvertTo(context, culture, value, destinationType);
                    }
                }
                public class CPoint3DConvert : TypeConverter
                {
                    //// http://kindtis.tistory.com/458 참조
                    // string 으로 부터 변환이 가능한가?
                    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                    {
                        if (sourceType == typeof(string))
                            return true;
                        return base.CanConvertFrom(context, sourceType);
                    }

                    // string 으로 부터 Point3d로 변환
                    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
                    {
                        if (value is string)
                        {
                            string[] v = ((string)value).Split(new char[] { ',' });
                            return new SPoint3D_t(int.Parse(v[0]), int.Parse(v[1]), int.Parse(v[2]));
                        }
                        return base.ConvertFrom(context, culture, value);
                    }

                    // Point3d 에서 string으로 변환
                    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
                    {
                        if (destinationType == typeof(string))
                            return ((SPoint3D_t)value).x + "," + ((SPoint3D_t)value).y + "," + ((SPoint3D_t)value).z;
                        return base.ConvertTo(context, culture, value, destinationType);
                    }
                }
                
                private const string strGroup2 = strGroup1;//"[2]Offset Rotation";
                [DisplayName(m_pstrProp_19), Browsable(true), CategoryAttribute(strGroup2), DescriptionAttribute("Offset Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                public SAngle3D_t SOffset_Rot{ get { return CDisp.SOffset_Rot; } set { CDisp.SOffset_Rot = value; } }

                private const string strGroup3 = "[3]Rotation/Translation";
                [DisplayName(m_pstrProp_22), Browsable(true), CategoryAttribute(strGroup3), DescriptionAttribute("1\'st Translation"), TypeConverter(typeof(CVector3DConvert))]
                public SVector3D_t STrans_1 { get { return CDisp.afTrans[0]; } set { CDisp.afTrans[0] = value; } }
                
                private const string strGroup4 = strGroup3;//"[3]1\'st Rotation";
                [DisplayName(m_pstrProp_25), Browsable(true), CategoryAttribute(strGroup4), DescriptionAttribute("1\'st Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                public SAngle3D_t SRot_1 { get { return CDisp.afRot[0]; } set { CDisp.afRot[0] = value; } }

                private const string strGroup5 = strGroup3;//"[4]2\'st Translation";
                [DisplayName(m_pstrProp_28), Browsable(true), CategoryAttribute(strGroup5), DescriptionAttribute("2\'st Translation"), TypeConverter(typeof(CVector3DConvert))]
                public SVector3D_t STrans_2 { get { return CDisp.afTrans[1]; } set { CDisp.afTrans[1] = value; } }

                private const string strGroup6 = strGroup3;//"[4]2\'st Rotation";
                [DisplayName(m_pstrProp_31), Browsable(true), CategoryAttribute(strGroup6), DescriptionAttribute("2\'st Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                public SAngle3D_t SRot_2 { get { return CDisp.afRot[1]; } set { CDisp.afRot[1] = value; } }

                private const string strGroup7 = strGroup3;//"[5]3\'st Translation";
                [DisplayName(m_pstrProp_34), Browsable(true), CategoryAttribute(strGroup7), DescriptionAttribute("3\'st Translation"), TypeConverter(typeof(CVector3DConvert))]
                public SVector3D_t STrans_3 { get { return CDisp.afTrans[2]; } set { CDisp.afTrans[2] = value; } }

                private const string strGroup8 = strGroup3;//"[5]3\'st Rotation";
                [DisplayName(m_pstrProp_37), Browsable(true), CategoryAttribute(strGroup8), DescriptionAttribute("3\'st Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                public SAngle3D_t SRot_3 { get { return CDisp.afRot[2]; } set { CDisp.afRot[2] = value; } }

                private const string strGroup9 = strGroup3;//"[6]4\'st Translation";
                [DisplayName(m_pstrProp_40), Browsable(true), CategoryAttribute(strGroup9), DescriptionAttribute("4\'st Translation"), TypeConverter(typeof(CVector3DConvert))]
                public SVector3D_t STrans_4 { get { return CDisp.afTrans[3]; } set { CDisp.afTrans[3] = value; } }

                private const string strGroup10 = strGroup3;//"[6]4\'st Rotation";
                [DisplayName(m_pstrProp_43), Browsable(true), CategoryAttribute(strGroup10), DescriptionAttribute("4\'st Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                public SAngle3D_t SRot_4 { get { return CDisp.afRot[3]; } set { CDisp.afRot[3] = value; } }

                private const string strGroup11 = strGroup3;//"[7]5\'st Translation";
                [DisplayName(m_pstrProp_46), Browsable(true), CategoryAttribute(strGroup11), DescriptionAttribute("5\'st Translation"), TypeConverter(typeof(CVector3DConvert))]
                public SVector3D_t STrans_5 { get { return CDisp.afTrans[4]; } set { CDisp.afTrans[4] = value; } }

                private const string strGroup12 = strGroup3;//"[7]5\'st Rotation";
                [DisplayName(m_pstrProp_49), Browsable(true), CategoryAttribute(strGroup12), DescriptionAttribute("5\'st Rotation"), TypeConverter(typeof(CAngle3DConvert))]
                public SAngle3D_t SRot_5{ get { return CDisp.afRot[4]; } set { CDisp.afRot[4] = value; } }

                [DisplayName(m_pstrProp_52), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute(""), TypeConverter(typeof(CPoint3DConvert))]
                public SPoint3D_t SPickGroup { get { return new SPoint3D_t(CDisp.nPickGroup_A, CDisp.nPickGroup_B, CDisp.nPickGroup_C); } set { CDisp.nPickGroup_A = value.x; CDisp.nPickGroup_B = value.y; CDisp.nPickGroup_C = value.z; } }
                
                [DisplayName(m_pstrProp_55), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public int nInverseKinematicsNumber { get { return CDisp.nInverseKinematicsNumber; } set { CDisp.nInverseKinematicsNumber = value; } }
                [DisplayName(m_pstrProp_56), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public float fScale_Serve0 { get { return CDisp.fScale_Serve0; } set { CDisp.fScale_Serve0 = value; } }
                [DisplayName(m_pstrProp_57), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public float fScale_Serve1 { get { return CDisp.fScale_Serve1; } set { CDisp.fScale_Serve1 = value; } }
                [DisplayName(m_pstrProp_58), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public int nMotorType { get { return CDisp.nMotorType; } set { CDisp.nMotorType = value; } }
                [DisplayName(m_pstrProp_59), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public int nMotorControl_MousePoint { get { return CDisp.nMotorControl_MousePoint; } set { CDisp.nMotorControl_MousePoint = value; } }
                [DisplayName(m_pstrProp_60), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                public String strPickGroup_Comment { get { return CDisp.strPickGroup_Comment; } set { CDisp.strPickGroup_Comment = value; } }

                //// if you want to add , just do like below ////
                
                //[DisplayName(m_pstrProp_), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("")]
                //public { get { return CDisp.; } set { CDisp. = value; } }

                #endregion Item add : Step 2/4
#if false
                #region Set
                
                
                #endregion Set
#endif
            }


            private const String m_strProp_Main0 = "Version";
            private const String m_strProp_Main1 = "Mouse Mode";
            private const String m_strProp_Main2 = "Empty";
            private const String m_strProp_Main3 = "Alpha(Main)";
            private const String m_strProp_Main4 = "Light";
            private const String m_strProp_Main5 = "Show StandardAxis";
            private const String m_strProp_Main6 = "Show VirtualAxis";
            private const string m_strProp_Main7 = "DefaultFunction";

            [DefaultPropertyAttribute("OpenJigWare")]
            private class CProp_Main
            {
                private const String strGroup = "[0]Main";

                public CProp_Main()
                {
                }
                ~CProp_Main()
                {
                }
                #region #### Var(Group0) ####
                private string _strVersion = "1.0.0";
                private int _nMouseMode = 0;
                private bool _bEmpty = false;
                private bool _bLight = true;
                private bool _bShowStandardAxis = true;
                private bool _bShowVirtualAxis = true;
                private int _nDefaultFunctionNum = -1;
                private float _fAlpha_All = 1.0f;
                // Robot Rot, Mov
                // Display Rot, Mov
                // Display Scale
                // Background Color
                // BackGround Light- Position[4], Diffuse[4], Specular[4], Ambient[4], Spot, Shiness, Exponent, Direction[3], Diffuse_Mat[4], Specular_Mat[4], Ambient_Mat[4]
                #endregion #### Var(Group0) ####

                #region #### Group0 ####
                [DisplayName(m_strProp_Main0), Browsable(true), CategoryAttribute(strGroup), ReadOnlyAttribute(true), DescriptionAttribute("Version(Open Jig Ware)")]
                public string strMain_Version { get { return _strVersion; } set { _strVersion = value; } }
                #region MouseMode(false)
                [DisplayName(m_strProp_Main1), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("Mouse Mode")]
                public int nMain_MouseMode { get { return _nMouseMode; } set { _nMouseMode = value; } }
                #endregion MouseMode
                #region Empty(false)
                [DisplayName(m_strProp_Main2), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("Empty"),
                TypeConverter(typeof(CEmptyOrNot))]
                public bool bMain_Empty { get { return _bEmpty; } set { _bEmpty = value; } }
                class CEmptyOrNot : BooleanConverter
                {
                    public override object  ConvertTo(ITypeDescriptorContext context, 
                        System.Globalization.CultureInfo culture, 
                        object value, 
                        Type destinationType)
                    {
                        return (bool)value ? "Empty" : "Fill";
                        //return base.ConvertTo(context, culture, value, destinationType);
                    }
                    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
                    {
                        return ((string)value == "Empty");
                        //return base.ConvertFrom(context, culture, value);
                    }
                }
                #endregion Empty
                #region Alpha(1.0f)
                [DisplayName(m_strProp_Main3), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("Alpha(1.0f")]
                public float fMain_Alpha { get { return _fAlpha_All; } set { _fAlpha_All = value; } }
                #endregion Alpha

                #region Light(true)
                [DisplayName(m_strProp_Main4), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("Light"),
                TypeConverter(typeof(CTrueOrNot))]
                public bool bMain_Light { get { return _bLight; } set { _bLight = value; } }
                class CTrueOrNot : BooleanConverter
                {
                    public override object ConvertTo(ITypeDescriptorContext context,
                        System.Globalization.CultureInfo culture,
                        object value,
                        Type destinationType)
                    {
                        return (bool)value ? "true" : "false";
                    }
                    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
                    {
                        return ((string)value == "true");
                    }
                }
                #endregion Light(true)
                #region ShowStandardAxis(false)
                [DisplayName(m_strProp_Main5), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("Show StandardAxis"),
                TypeConverter(typeof(CTrueOrNot))]
                public bool bMain_ShowStandardAxis { get { return _bShowStandardAxis; } set { _bShowStandardAxis = value; } }
                #endregion ShowStandardAxis(false)
                #region ShowVirtualAxis(false)
                [DisplayName(m_strProp_Main6), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("Show VirtualAxis"),
                TypeConverter(typeof(CTrueOrNot))]
                public bool bMain_ShowVirtualAxis { get { return _bShowVirtualAxis; } set { _bShowVirtualAxis = value; } }
                #endregion ShowStandardAxis(false)
                #region ShowVirtualAxis(false)
                [DisplayName(m_strProp_Main7), Browsable(true), CategoryAttribute(strGroup), DescriptionAttribute("Default Function(-1: None, 0 ~ 511)")]
                public int nMain_DefaultFunctionNum { get { return _nDefaultFunctionNum; } set { _nDefaultFunctionNum = value; } }
                #endregion ShowStandardAxis(false)

                

                #endregion #### Group0 ####
            }

#if false
            [DefaultPropertyAttribute("OpenJigWare(Serv0)")]
            private class CProp_Serve0
            {
                private const String strGroup = "Model";
                public CProp_Serve0()
                {
                }
                ~CProp_Serve0()
                {
                }

                #region region #### Var(Group1) ####
                private int _nAxisNumber = -1;
                private Color _cColor = Color.Green;
                private float _fAlpha = 1.0f;
                private String _strModelName = "#0";
                #endregion #### Var(Group1) ####

                #region #### Group1 ####
                #region AxisNumber(-1)
                [CategoryAttribute(strGroup), DescriptionAttribute("Axis Number(-1:None, 0~253)")]
                public int nAxisNumber { get { return _nAxisNumber; } set { _nAxisNumber = value; } }
                //enum EAxisNum_t
                //{
                //    [Description("None")]
                //    _None = -1,
                //    [Description("Axis0")]
                //    _Axis0 = 0,
                //    [Description("Axis1")]
                //    _Axis1,
                //    [Description("Axis2")]
                //    _Axis2,
                //    [Description("Axis3")]
                //    _Axis3,
                //}
                #endregion AxisNumber
#if false
                //CDisp.SetData(-1, Color.White, 1.0f, "#0", 
                ("Fill(0:Empty, 1:Filled") true, 
                ("Texture Number(-1:None, 0~")-1, 
                ("Init Coordinate")false, 
                10, 10, 20, 4, 0, "", -1, 0, 0, 0, pSVector[0], pSAngle[0], pSVector, pSAngle, 0, 0, 0, 255, 0.35f, 0.35f, 0, 0, "");
#endif
                #region Color(Green)
                [CategoryAttribute(strGroup), DescriptionAttribute("Color")]
                public Color cColor { get { return _cColor; } set { _cColor = value; } }
                #endregion Color
                #region Alpha(1.0f)
                [CategoryAttribute(strGroup), DescriptionAttribute("Alpha Value ( 0 ~ 1.0)")]
                public float fAlpha { get { return _fAlpha; } set { _fAlpha = value; } }
                #endregion Alpha
                #region ModelName("#0")
                [CategoryAttribute(strGroup),
                DescriptionAttribute("Model Name (Default: #0 ~ #14, FileName)"),
                DefaultValueAttribute("#0")]
                public String strModelName { get { return _strModelName; } set { _strModelName = value; } }
                #endregion ModelName
                #endregion #### Group1 ####
            }
            
            [DefaultPropertyAttribute("OpenJigWare")]
            private class CProp
            {
                public CProp()
                {
                    //_nAxisNumber = -1;
                    //_cColor = Color.Green;
                    //_fAlpha = 1.0f;
                    //_strModelName = "#0";
                }
                ~CProp()
                {
                }

                #region Property

                #region GroupName
                private const String strGroup0 = "Main";
                private const String strGroup1 = "Model";
                #endregion GroupName

                #region Var
                #region #### Var(Group0) ####
                private string _strVersion = "1.0.0";
                private bool _bMouseMode = false;
                private bool _bEmpty = false;
                private float _fAlpha_All = 1.0f;
                #endregion #### Var(Group0) ####

                #region region #### Var(Group1) ####
                private int _nAxisNumber = -1;
                private Color _cColor = Color.Green;
                private float _fAlpha = 1.0f;
                private String _strModelName = "#0";
                #endregion #### Var(Group1) ####
                #endregion Var

                #region #### Group0 ####
                [CategoryAttribute(strGroup0), ReadOnlyAttribute(true), DescriptionAttribute("Version(Open Jig Ware)")]
                public string strVersion { get { return _strVersion; } set { _strVersion = value; } }
                #region MouseMode(false)
                [CategoryAttribute(strGroup0), DescriptionAttribute("Mouse Mode")]
                public bool bMouseMode { get { return _bMouseMode; } set { _bMouseMode = value; } }
                #endregion MouseMode
                #region Empty(false)
                [CategoryAttribute(strGroup0), DescriptionAttribute("Empty")]
                public bool bEmpty { get { return _bEmpty; } set { _bEmpty = value; } }
                #endregion Empty
                #region Alpha(1.0f)
                [CategoryAttribute(strGroup0), DescriptionAttribute("Alpha(1.0f")]
                public float fAlpha_All { get { return _fAlpha_All; } set { _fAlpha_All = value; } }
                #endregion Alpha
                #endregion #### Group0 ####

                #region #### Group1 ####
                #region AxisNumber(-1)
                [CategoryAttribute(strGroup1), DescriptionAttribute("Axis Number(-1:None, 0~253)")]
                public int nAxisNumber { get { return _nAxisNumber; } set { _nAxisNumber = value; } }
                #endregion AxisNumber
#if false
                //CDisp.SetData(-1, Color.White, 1.0f, "#0", 
                ("Fill(0:Empty, 1:Filled") true, 
                ("Texture Number(-1:None, 0~")-1, 
                ("Init Coordinate")false, 
                10, 10, 20, 4, 0, "", -1, 0, 0, 0, pSVector[0], pSAngle[0], pSVector, pSAngle, 0, 0, 0, 255, 0.35f, 0.35f, 0, 0, "");
#endif
                #region Color(Green)
                [CategoryAttribute(strGroup1), DescriptionAttribute("Color")]
                public Color cColor { get { return _cColor; } set { _cColor = value; } }
                #endregion Color
                #region Alpha(1.0f)
                [CategoryAttribute(strGroup1), DescriptionAttribute("Alpha Value ( 0 ~ 1.0)")]
                public float fAlpha { get { return _fAlpha; } set { _fAlpha = value; } }
                #endregion Alpha
                #region ModelName("#0")
                [CategoryAttribute(strGroup1),
                DescriptionAttribute("Model Name (Default: #0 ~ #14, FileName)"),
                DefaultValueAttribute("#0")]
                public String strModelName { get { return _strModelName; } set { _strModelName = value; } }
                #endregion ModelName
                #endregion #### Group1 ####

                #endregion Property
                //m_lstMenu.Add;
                //m_lstMenu.Add("Width/[Radius]");
                //m_lstMenu.Add("Height/[Depth]");
                //m_lstMenu.Add("Depth/[Line Count]");
                //m_lstMenu.Add("Thickness");
                //m_lstMenu.Add("Gap");
                //m_lstMenu.Add("Rotation Axis(0:Pan, 1:Tilt, 2: Swing, 3: Linear X, 4: Linear Y, 5: Linear Z");
                //m_lstMenu.Add("[Rotation Direction] 0(Forward), 1(Inverse)");
                //m_lstMenu.Add("[Real Angle Offset] - Don't use");
                //m_lstMenu.Add("[Rotation Angle for Display(Offset)]");
                //m_lstMenu.Add("[Offset XYZ] X");
                //m_lstMenu.Add("[Offset XYZ] Y");
                //m_lstMenu.Add("[Offset XYZ] Z");
                //m_lstMenu.Add("[Offset Angle] Pan");
                //m_lstMenu.Add("[Offset Angle] Tilt");
                //m_lstMenu.Add("[Offset Angle] Swing");
                //m_lstMenu.Add("[Translation] X_0");
                //m_lstMenu.Add("[Translation] Y_0");
                //m_lstMenu.Add("[Translation] Z_0");
                //m_lstMenu.Add("[Rotation] Pan_0");
                //m_lstMenu.Add("[Rotation] Tilt_0");
                //m_lstMenu.Add("[Rotation] Swing_0");
                //m_lstMenu.Add("[Translation] X_1");
                //m_lstMenu.Add("[Translation] Y_1");
                //m_lstMenu.Add("[Translation] Z_1");
                //m_lstMenu.Add("[Rotation] Pan_1");
                //m_lstMenu.Add("[Rotation] Tilt_1");
                //m_lstMenu.Add("[Rotation] Swing_1");
                //m_lstMenu.Add("[Translation] X_2");
                //m_lstMenu.Add("[Translation] Y_2");
                //m_lstMenu.Add("[Translation] Z_2");
                //m_lstMenu.Add("[Rotation] Pan_2");
                //m_lstMenu.Add("[Rotation] Tilt_2");
                //m_lstMenu.Add("[Rotation] Swing_2");
                //m_lstMenu.Add("[1\'st Sub-Translation] X");
                //m_lstMenu.Add("[1\'st Sub-Translation] Y");
                //m_lstMenu.Add("[1\'st Sub-Translation] Z");
                //m_lstMenu.Add("[1\'st Sub-Rotation] Pan");
                //m_lstMenu.Add("[1\'st Sub-Rotation] Tilt");
                //m_lstMenu.Add("[1\'st Sub-Rotation] Swing");
                //m_lstMenu.Add("[2\'st Sub-Translation] X");
                //m_lstMenu.Add("[2\'st Sub-Translation] Y");
                //m_lstMenu.Add("[2\'st Sub-Translation] Z");
                //m_lstMenu.Add("[2\'st Sub-Rotation] Pan");
                //m_lstMenu.Add("[2\'st Sub-Rotation] Tilt");
                //m_lstMenu.Add("[2\'st Sub-Rotation] Swing");
                //m_lstMenu.Add("[Group A]");
                //m_lstMenu.Add("[Group B]");
                //m_lstMenu.Add("[Group C]");
                //m_lstMenu.Add("[Connected Kinematics Group]");
                //m_lstMenu.Add("[1\'st Group Scale] Max=1");
                //m_lstMenu.Add("[2\'st Group Scale] Max=1");
                //m_lstMenu.Add("[Motor Type] 0(Position Control), 1(Speed Control)");
                //m_lstMenu.Add("[Mouse Drag Direction]0(x+),1(x-),2(y+),3(y-)");
                //m_lstMenu.Add("[Comment]");







#if false


                public bool _bTest = true;
                private string _strTest = "응용 프로그램을 시작합니다.";
                private int _nTest = 4;
                private float _fTest = 10.0f;
                //private bool settingsChanged = false;
                //[CategoryAttribute("ID Settings"), DescriptionAttribute("the customer2")]
                //private string appVersion = "1.0";
                [CategoryAttribute("테스트 1"), DescriptionAttribute("the customer1")]
                public bool _1_bTest
                {
                    get { return _bTest; }
                    set { _bTest = value; }
                }
                [CategoryAttribute("테스트 1"), DescriptionAttribute("the customer2")]
                public string _2_strTest
                {
                    get { return _strTest; }
                    set { _strTest = value; }
                }
                [CategoryAttribute("테스트 2"), DescriptionAttribute("the customer3")]
                public int _3_nTest
                {
                    get { return _nTest; }
                    set { _nTest = value; }
                }
                [CategoryAttribute("테스트 2"), DescriptionAttribute("the customer4")]
                public float _4_fTest
                {
                    get { return _fTest; }
                    set { _fTest = value; }
                }
#endif
            }
#endif
#endif
            #endregion PropertyGrid


            public void SetAlpha_Display_Enalbe(bool bEn) { m_bAlpha = bEn; }
            public bool GetAlpha_Display_Enalbe() { return m_bAlpha; }
            private bool m_bAlpha = true;
            private float m_fAlpha = 1.0f;
            // The higher this number, the boundary line becomes dark.
            // Kor: 이 숫자를 높일수록 경계라인이 진해진다.
            private const int _COLOR_GAP = 30;

            public COjwDesignerHeader m_CHeader = new COjwDesignerHeader();
            private const int _SIZE_MAX_MOT = 256;
            private float[] m_afMot = new float[_SIZE_MAX_MOT];
            private float CalcLimit(int nMot, float fValue) 
            {
                try
                {
                    if (m_CHeader.pSMotorInfo == null) return fValue;
                    if (m_CHeader.pSMotorInfo.Length <= nMot) return fValue;
                    if ((m_CHeader.pSMotorInfo[nMot].fLimit_Down != 0) && (m_CHeader.pSMotorInfo[nMot].fLimit_Down >= fValue)) fValue = m_CHeader.pSMotorInfo[nMot].fLimit_Down;
                    if ((m_CHeader.pSMotorInfo[nMot].fLimit_Up != 0) && (m_CHeader.pSMotorInfo[nMot].fLimit_Up <= fValue)) fValue = m_CHeader.pSMotorInfo[nMot].fLimit_Up;
                    return fValue;
                }
                catch //(Exception ex)
                {
                    //CMessage.Write_Error();
                }
                return fValue;
            }
            public void SetData(int nMot, float fValue) { if (nMot < m_afMot.Length) m_afMot[nMot] = CalcLimit(nMot, fValue); }
            public float GetData(int nMot) { if (nMot < m_afMot.Length) return m_afMot[nMot]; return 0; }
            public float [] GetData() { return m_afMot; }

            #region CsGL Class / The actual drawing and initialization functions are all based here.(Kor: CsGL Class / 실제 그리기 및 초기화(즉, Main) 함수 모음)

                private List<COjwAse> m_lstOjwAse = new List<COjwAse>();
                private List<String> m_lstModel = new List<string>();

                #region BaseVar

                #region Variable and init value(Kor: 기본 변수 & 초기값)
                private float m_fX = 0.0f;
                private float m_fY = 0.0f;
                private float m_fZ = 0.0f;
                private float m_fPan = -10.0f;
                private float m_fTilt = 10.0f;
                private float m_fSwing = 0.0f;

                private float m_fX_Robot = 0.0f;
                private float m_fY_Robot = 0.0f;
                private float m_fZ_Robot = 0.0f;
                private float m_fPan_Robot = 0.0f;
                private float m_fTilt_Robot = 0.0f;
                private float m_fSwing_Robot = 0.0f;

                private float m_fScale = 0.35f;    // 1.0 = 100%

                private Color m_Color = Color.Red;
                private Color m_BackColor = Color.DarkGray;

                private float[] m_fColor = new float[4];
                private float[] m_fColor_Back = new float[3];
                #endregion

                public void SetRobot_Trans_X(float fValue) { m_fX_Robot = fValue; }
                public void SetRobot_Trans_Y(float fValue) { m_fY_Robot = fValue; }
                public void SetRobot_Trans_Z(float fValue) { m_fZ_Robot = fValue; }
                public void SetRobot_Trans(float fX, float fY, float fZ) { m_fX_Robot = fX; m_fY_Robot = fY; m_fZ_Robot = fZ; }
                public void SetRobot_Rot_Pan(float fValue) { m_fPan_Robot = fValue; }
                public void SetRobot_Rot_Tilt(float fValue) { m_fTilt_Robot = fValue; }
                public void SetRobot_Rot_Swing(float fValue) { m_fSwing_Robot = fValue; }
                public void SetRobot_Rot(float fPan, float fTilt, float fSwing) { m_fPan_Robot = fPan; m_fTilt_Robot = fTilt; m_fSwing_Robot = fSwing; }

                public float GetRobot_Trans_X() { return m_fX_Robot; }
                public float GetRobot_Trans_Y() { return m_fY_Robot; }
                public float GetRobot_Trans_Z() { return m_fZ_Robot; }
                public void GetRobot_Trans(out float fX, out float fY, out float fZ) { fX = m_fX_Robot; fY = m_fY_Robot; fZ = m_fZ_Robot; }
                public float GetRobot_Rot_Pan() { return m_fPan_Robot; }
                public float GetRobot_Rot_Tilt() { return m_fTilt_Robot; }
                public float GetRobot_Rot_Swing() { return m_fSwing_Robot; }
                public void GetRobot_Rot(out float fPan, out float fTilt, out float fSwing) { fPan = m_fPan_Robot; fTilt = m_fTilt_Robot; fSwing = m_fSwing_Robot; }
            
                #region Set function settings for the entire screen(Kor: 전체화면의 설정을 Set/Get 하는 함수)
                // Set rotation/translation settings for the entire screen(Kor: 화면 전체의 회전각/위치값 결정)
                public void SetAngle_Display(float fPan, float fTilt, float fSwing) { m_fPan = fPan; m_fTilt = fTilt; m_fSwing = fSwing; }
                public void SetPos_Display(float fX, float fY, float fZ) { m_fX = fX; m_fY = fY; m_fZ = fZ; }

                public void GetAngle_Display(out float fPan, out float fTilt, out float fSwing) { fPan = m_fPan; fTilt = m_fTilt; fSwing = m_fSwing; }
                public void GetPos_Display(out float fX, out float fY, out float fZ) { fX = m_fX; fY = m_fY; fZ = m_fZ; }

                // Get the rotation of the entire screen(Kor: 화면 전체의 회전각을 가져오기)
                public float GetPan() { return m_fPan; }
                public float GetTilt() { return m_fTilt; }
                public float GetSwing() { return m_fSwing; }

                // Set the magnification of the entire screen(Kor: 화면 전체의 확대비율 결정)
                public void SetScale(float fScale) { m_fScale = fScale; }
                // Get the magnification of the entire screen(Kor: 화면 전체의 확대비율을 가져오기)
                public float GetScale() { return m_fScale; }

                // Set transparent percentage of the whole screen(Kor: 화면 전체의 투명비율 결정)
                public void SetAlpha(float fAlpha) { m_bAlpha = true; m_fAlpha = fAlpha; }
                // Import transparent percentage of the whole screen(Kor: 화면 전체의 투명비율 가져오기)
                public float GetAlpha(float fAlpha) { return m_fAlpha; }

                // Color
                public void SetColor(Color cData) { m_Color = cData; }
                public void SetBackColor(Color cData) { m_BackColor = cData; }

                public Color GetColor() { return m_Color; }
                public Color GetBackColor() { return m_BackColor; }
                #endregion Set function settings for the entire screen(Kor: 전체화면의 설정을 Set/Get 하는 함수)

                #endregion BaseVar

                #region Mouse Control
                //// Mouse Control ////
                // It is possible to control up to 10(Kor: 10개 까지 컨트롤 가능) //
                private bool m_bMouseClick = false;
                private bool m_bMouseLeftClick = true;
                private int m_nMouse_X_Left = 0;
                private int m_nMouse_Y_Left = 0;
                private int m_nMouse_X_Right = 0;
                private int m_nMouse_Y_Right = 0;

                private bool m_bPickMouseClick = false;
                private bool m_bPickMouseClick_Reserve = false;
                public void SetPickCheck_OneShot(int nX, int nY) { m_nMouse_X_Left = nX; m_nMouse_Y_Left = nY; m_bPickMouseClick_Reserve = true; }
                public void OjwMouseDown(object sender, MouseEventArgs e) { OjwMouseDown(e); }
                public double m_dPos_X = 0.0f;
                public double m_dPos_Y = 0.0f;
                public double m_dPos_Z = 0.0f;
                public void OjwMouseDown(MouseEventArgs e)
                {
                    if (m_bMouseClick == false)
                    {
                        SelectObject_Clear();
                        int nInverseNum = m_nSelected_InverseKinematicsNumber;// m_nStatus_InverseKinematicsNumber;
                        m_bMouseClick = true;
                        if (e.Button == MouseButtons.Left)
                        {
                            SetPickCheck_OneShot(e.X, e.Y);
                            
                            //OjwDraw();

                            m_bMouseLeftClick = true;
                            m_nMouse_X_Left = e.X;
                            m_nMouse_Y_Left = e.Y;                            
                        }
                        else
                        {
                            m_bMouseLeftClick = false;
                            m_nMouse_X_Right = e.X;
                            m_nMouse_Y_Right = e.Y;
                            if ((GetMouseMode() == 0) && (e.Button == MouseButtons.Right)) m_nMenuStatus = 1;
                        }
                                                
                        #region Forward
                        double[] adMot = new double[m_afMot.Length];
                        for (int i = 0; i < m_afMot.Length; i++) adMot[i] = (double)m_afMot[i];
                        if (m_CHeader != null)
                            CKinematics.CForward.CalcKinematics(m_CHeader.pDhParamAll[nInverseNum], adMot, out m_dPos_X, out m_dPos_Y, out m_dPos_Z);

                        adMot = null;
                        #endregion Forward
                    }
                }
                public void OjwMouseUp(object sender, MouseEventArgs e) { OjwMouseUp(e); }
                ContextMenuStrip m_ctxmenuMouse = new ContextMenuStrip();
                private void m_ctxmenuMouse_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
                {
                    if (e.ClickedItem != m_ctxmenuMouse.Items[1])
                        MessageBox.Show(e.ClickedItem.Text);                                            
                }
                private void m_ctxmenuMouse_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
                {
                    MessageBox.Show("[Sub]" + e.ClickedItem.Text);
                }
                private void PopupMenu()
                {
                    m_ctxmenuMouse.Items.Clear();

                    m_ctxmenuMouse.Items.Add("Test0");
                    m_ctxmenuMouse.Items.Add("Test1");
                    m_ctxmenuMouse.Items.Add("Test2");
                    m_ctxmenuMouse.Items.Add("Test3");
                    m_ctxmenuMouse.Items.Add("Test4");
                    m_ctxmenuMouse.Items.Add("Test5");
                    m_ctxmenuMouse.Items.Add("Test6");
                    (m_ctxmenuMouse.Items[1] as ToolStripMenuItem).DropDownItems.Add("SubTest0");
                    (m_ctxmenuMouse.Items[1] as ToolStripMenuItem).DropDownItems.Add("SubTest1");
                    (m_ctxmenuMouse.Items[1] as ToolStripMenuItem).DropDownItemClicked += new ToolStripItemClickedEventHandler(m_ctxmenuMouse_DropDownItemClicked);
                    //m_ctxmenuMouse.Items[1].Click += new EventHandler(m_ctxmenuMouse);

                    m_ctxmenuMouse.Show(Cursor.Position.X, Cursor.Position.Y);
                }
                public void OjwMouseUp(MouseEventArgs e)
                {

                    //OjwDraw();
                    //m_nMouse_X_Left = e.X;
                    //m_nMouse_Y_Left = e.Y; 
                    //m_nMouse_X_Right = e.X;
                    //m_nMouse_Y_Right = e.Y;


                    //m_bPickMouseClick = true;
                    //m_bPickMouseClick_Reserve = true;
                    //SetPickCheck_OneShot(e.X, e.Y);

#if true
                    if (m_bMouseClick == true)
                    {
                        if (e.Button == MouseButtons.Right)
                        {
                            //m_fLeg0 = m_fMouseMove - e.X;
                            if (m_bShowPopup == true)
                            {
                                if ((GetMouseMode() == 0) && (m_nMenuStatus != 0))
                                {
                                    PopupMenu();
                                }
                            }
                        }
                    }
#endif
                    m_bMouseClick = false;
                }
            
                private int m_nMouseControlMode = 0;
                public void SetMouseMode_Move() { SetMouseMode(0); }
                public void SetMouseMode_Control() { SetMouseMode(1); }
                public void SetMouseMode_Ext() { SetMouseMode(2); }
                public void SetMouseMode(int nControl) { m_nMouseControlMode = nControl; }
                public int GetMouseMode() { return m_nMouseControlMode; }
                private bool m_bFreeze_X = false;
                private bool m_bFreeze_Y = false;
                private bool m_bFreeze_Z = false;
                public void SetFreeze_X(bool bFreeze) { m_bFreeze_X = bFreeze; }
                public void SetFreeze_Y(bool bFreeze) { m_bFreeze_Y = bFreeze; }
                public void SetFreeze_Z(bool bFreeze) { m_bFreeze_Z = bFreeze; }
                public bool GetFreeze_X() { return m_bFreeze_X; }
                public bool GetFreeze_Y() { return m_bFreeze_Y; }
                public bool GetFreeze_Z() { return m_bFreeze_Z; }
                public void OjwMouseMove(object sender, MouseEventArgs e) { OjwMouseMove(e); }
                public int m_nMenuStatus = 0;
                public bool m_bShowPopup = false;
                public void ShowPopup(bool bEn) { m_bShowPopup = bEn; }
                public void OjwMouseMove(MouseEventArgs e)
                {
                    if (m_bMouseClick == true)
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            //m_fLeg0 = m_fMouseMove - e.X;
                            if (GetMouseMode() == 0)
                            {
                                #region View Change
                                m_fX -= (m_nMouse_X_Left - e.X);
                                m_fY += (m_nMouse_Y_Left - e.Y);
                                #endregion View Change
                            }
                            else if (GetMouseMode() == 1)
                            {
                                int nInverseNum = m_nSelected_InverseKinematicsNumber;// m_nStatus_InverseKinematicsNumber;
                                #region Motor Control
                                if (nInverseNum == 255)
                                {
                                    SetData(m_anSelectedGroup[1], GetData(m_anSelectedGroup[1]) + e.X - m_nMouse_X_Left);
                                }
                                #endregion Motor Control
                                #region Kinematics Control
                                else
                                {
                                    //float fR_T = GetMot(8) - GetMot(7);// -GetMot(9);
                                    //float fR_S = GetMot(6) - GetMot(10);
                                    //float fL_T = GetMot(13) - GetMot(12);// -GetMot(14);
                                    //float fL_S = GetMot(11) - GetMot(15);
                                    double dPos_X = (e.X - m_nMouse_X_Left);
                                    double dPos_Y = (m_nMouse_Y_Left - e.Y);
                                    double dPos_Z = 0;
                                    float fAngle_X, fAngle_Y, fAngle_Z;
                                    GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);
                                    CMath.CalcRot(0, 0, (double)(-fAngle_Z), ref dPos_X, ref dPos_Y, ref dPos_Z);
                                    CMath.CalcRot(0, (double)(-fAngle_Y), 0, ref dPos_X, ref dPos_Y, ref dPos_Z);
                                    CMath.CalcRot((double)(-fAngle_X), 0, 0, ref dPos_X, ref dPos_Y, ref dPos_Z);
                                    //m_COjwCsgl.Rotation(m_fDisp_Tilt, m_fDisp_Pan, m_fDisp_Swing, ref fPos_X, ref fPos_Y, ref fPos_Z);

                                    // 2D -> 3D rotation : rotate (e.X - m_nX) 
                                    // Pan - Rotation(Y)
                                    // Tilt - Rotation(X)
                                    // Swing - Rotation(Z)
                                    #region Kor
                                    // 2차원을 3차원으로 회전 : (e.X - m_nX) 를 회전
                                    // Pan - Y축 회전
                                    // Tilt - X축 회전
                                    // Swing - Z축 회전
                                    #endregion Kor
#if true
                                    m_dPos_X += (GetFreeze_X() == true) ? 0.0f : dPos_X;
                                    m_dPos_Y += (GetFreeze_Y() == true) ? 0.0f : dPos_Y;
                                    m_dPos_Z += (GetFreeze_Z() == true) ? 0.0f : dPos_Z;
#else
                                    m_dPos_X += dPos_X;
                                    m_dPos_Y += dPos_Y;
                                    m_dPos_Z += dPos_Z;
#endif
                                    CKinematics.CInverse.SetValue_ClearAll(ref m_CHeader.pSOjwCode[nInverseNum]);
                                    CKinematics.CInverse.SetValue_X(m_dPos_X);
                                    CKinematics.CInverse.SetValue_Y(m_dPos_Y);
                                    CKinematics.CInverse.SetValue_Z(m_dPos_Z);

                                    double[] adMot = new double[m_afMot.Length];
                                    //Buffer.BlockCopy(GetData(), 0, adMot, 0, adMot.Length);
                                    for (int i = 0; i < m_afMot.Length; i++) adMot[i] = (double)m_afMot[i];
                                    CKinematics.CInverse.SetValue_Motor(adMot);
                                    adMot = null;

                                    CKinematics.CInverse.CalcCode(ref m_CHeader.pSOjwCode[nInverseNum]);
                                    //OjwMessage(CKinematics.CInverse.GetValue_V(0).ToString() + ", " + CKinematics.CInverse.GetValue_V(1).ToString() + ", " + CKinematics.CInverse.GetValue_V(2).ToString());
                                    int nMotCnt = m_CHeader.pSOjwCode[nInverseNum].nMotor_Max;
                                    for (int i = 0; i < nMotCnt; i++)
                                    {
                                        int nMotNum = m_CHeader.pSOjwCode[nInverseNum].pnMotor_Number[i];
                                        SetData(nMotNum, (float)CKinematics.CInverse.GetValue_Motor(nMotNum));
                                    }
                                }
                                #endregion Kinematics Control
                            }
                            else if (GetMouseMode() == 2) // 2
                            {
                                #region Robot Control(Rotation)
                                m_fPan_Robot -= (m_nMouse_X_Left - e.X);
                                m_fTilt_Robot += (m_nMouse_Y_Left - e.Y);
                                #endregion Robot Control(Rotation)
                            }
                            else //if (GetMouseMode() == 3) // 3
                            {
                                #region Robot Control(Translation)
                                m_fX_Robot -= (m_nMouse_X_Left - e.X);
                                m_fY_Robot += (m_nMouse_Y_Left - e.Y);
                                #endregion Robot Control(Translation)
                            }
                            m_nMouse_X_Left = e.X;
                            m_nMouse_Y_Left = e.Y;                            
                        }
                        else if (e.Button == MouseButtons.Right)
                        {
                            m_fPan -= (m_nMouse_X_Right - e.X);
                            m_fTilt -= (m_nMouse_Y_Right - e.Y);
                            if (m_fTilt < 0) m_fTilt += 360;
                            if (m_fPan < 0) m_fPan += 360;
                            if (m_fTilt >= 360) m_fTilt -= 360;
                            if (m_fPan >= 360) m_fPan -= 360;
                            if (GetMouseMode() == 0)
                            {
                                //if ((m_nMouse_X_Right == e.X) && (m_nMouse_Y_Right == e.Y)) m_nMenuStatus = 1;
                                //else m_nMenuStatus = 0;
                                if ((m_nMouse_X_Right != e.X) || (m_nMouse_Y_Right != e.Y)) m_nMenuStatus = 0;
                            }
                            else
                            {
                                m_nMenuStatus = 0;
                            }
                            m_nMouse_X_Right = e.X;
                            m_nMouse_Y_Right = e.Y;
                        }
                        else
                        {
                        }
                        //OjwDraw();
                    }
                }
#if false
                public void OjwMouseMove2(MouseEventArgs e, bool bMoveZ, bool bMoveSwing)
                {
                    if (m_bMouseClick == true)
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            //m_fLeg0 = m_fMouseMove - e.X;
                            if (bMoveZ == true) m_fZ -= (m_nMouse_X_Left - e.X);
                            else m_fX -= (m_nMouse_X_Left - e.X);
                            m_fY += (m_nMouse_Y_Left - e.Y);
                            m_nMouse_X_Left = e.X;
                            m_nMouse_Y_Left = e.Y;
                        }
                        else //if (m_abMouseClick == true)
                        {
                            if (bMoveSwing == true) m_fSwing -= (m_nMouse_X_Right - e.X);
                            else m_fPan -= (m_nMouse_X_Right - e.X);
                            m_fTilt -= (m_nMouse_Y_Right - e.Y);
                            if (m_fTilt < 0) m_fTilt += 360;
                            if (m_fPan < 0) m_fPan += 360;
                            if (m_fSwing < 0) m_fSwing += 360;
                            if (m_fTilt >= 360) m_fTilt -= 360;
                            if (m_fPan >= 360) m_fPan -= 360;
                            if (m_fSwing >= 360) m_fSwing -= 360;
                            m_nMouse_X_Right = e.X;
                            m_nMouse_Y_Right = e.Y;
                        }
                    }
                }
#endif
                public void OjwMouseDoubleClick(object sender, MouseEventArgs e) { OjwMouseDoubleClick(e); }
                public void OjwMouseDoubleClick(MouseEventArgs e)
                {

                    if (e.Button == MouseButtons.Left)
                    {
                        //SetScale()
                    }
                }

                public void OjwMouseWheel(object sender, MouseEventArgs e) { OjwMouseWheel(false, e); }
                public void OjwMouseWheel(bool bSmallValue, MouseEventArgs e)
                {
                    if (m_bMouseClick == true)
                    {
                        // so if a key is depressed to move a little bit.
                        // Kor: Shift Key 가 눌렸다면 작게(1) 움직이도록...
                        float fDelta = ((bSmallValue == true) ? 0.005f : 0.01f); 
                        float fData = (e.Delta > 0) ? -fDelta : fDelta;

                        if (m_bMouseLeftClick == false)
                        {
                            if ((m_fScale + fData) > 0) m_fScale += fData;
                            if (GetMouseMode() == 0) m_nMenuStatus = 0;
                        }
                        else
                        {
                            float fMul = 1000.0f;
                            if (GetMouseMode() == 0) m_fZ += fData * fMul;
                            else if (GetMouseMode() == 2) // 2
                            {
                                #region Robot Control(Rotation)
                                m_fSwing_Robot += fData * fMul;
                                #endregion Robot Control(Rotation)
                            }
                            else if (GetMouseMode() == 3) // 3
                            {
                                #region Robot Control(Translation)
                                m_fZ_Robot += fData * fMul;
                                #endregion Robot Control(Translation)
                            }
                        }
                        //OjwDraw();
                    }
                }
                #endregion Mouse Control
                ///////////////////////

                //// CSGL - 2 ////
                /// Default Constructor
                /// 

                #region There are basic functions for initialization.(Kor: OpenGL 을 처음 실행시 초기화 하는 함수등 OpenGL 기본적 구현 함수)

                // contructor, destructor
                #region Initialize
                private bool m_bClassEnd = false;
                private bool m_bMouseEventEnable = false;
                public C3d()//CCsgl()
                    : base()
                {
                    for (int i = 0; i < 3; i++) m_fColor[i] = 1.0f;
                    InitGLContext();
                    //this.MouseDown += (MouseEventHandler)OjwMouseDown;
                    //this.MouseMove += (MouseEventHandler)OjwMouseMove;
                    //this.MouseUp += (MouseEventHandler)OjwMouseUp;
                    //this.MouseWheel += (MouseEventHandler)OjwMouseWheel;
                    //m_bMouseEventEnable = true;
                    SetMouseEventEnable(true);

                    //m_CTId_Pan.Set();
                    //m_CTId_Tilt.Set();
                    //m_CTId_Swing.Set();
                    InitVirtualClass();

                    SelectObject_Clear();

                    m_ctxmenuMouse.ItemClicked += new ToolStripItemClickedEventHandler(m_ctxmenuMouse_ItemClicked);                    
                }
                public void AddMouseEvent(MouseEventHandler FDown, MouseEventHandler FMove, MouseEventHandler FUp, MouseEventHandler FWheel)
                {
                    this.MouseDown += (MouseEventHandler)FDown;
                    this.MouseMove += (MouseEventHandler)FMove;
                    this.MouseUp += (MouseEventHandler)FUp;
                    this.MouseWheel += (MouseEventHandler)FWheel;
                }
                public void AddMouseEvent_Down(MouseEventHandler FFunc) { this.MouseDown += (MouseEventHandler)FFunc; }
                public void AddMouseEvent_Move(MouseEventHandler FFunc) { this.MouseMove += (MouseEventHandler)FFunc; }
                public void AddMouseEvent_Up(MouseEventHandler FFunc) { this.MouseUp += (MouseEventHandler)FFunc; }
                public void AddMouseEvent_Wheel(MouseEventHandler FFunc) { this.MouseWheel += (MouseEventHandler)FFunc; }

                public void RemoveMouseEvent(MouseEventHandler FDown, MouseEventHandler FMove, MouseEventHandler FUp, MouseEventHandler FWheel)
                {
                    this.MouseDown -= (MouseEventHandler)FDown;
                    this.MouseMove -= (MouseEventHandler)FMove;
                    this.MouseUp -= (MouseEventHandler)FUp;
                    this.MouseWheel -= (MouseEventHandler)FWheel;
                }
                public void RemoveMouseEvent_Down(MouseEventHandler FFunc) { this.MouseDown -= (MouseEventHandler)FFunc; }
                public void RemoveMouseEvent_Move(MouseEventHandler FFunc) { this.MouseMove -= (MouseEventHandler)FFunc; }
                public void RemoveMouseEvent_Up(MouseEventHandler FFunc) { this.MouseUp -= (MouseEventHandler)FFunc; }
                public void RemoveMouseEvent_Wheel(MouseEventHandler FFunc) { this.MouseWheel -= (MouseEventHandler)FFunc; }

                public void SetMouseEventEnable(bool bEn)
                {
                    if (bEn != m_bMouseEventEnable)
                    {
                        m_bMouseEventEnable = bEn;
                        if (bEn == true)
                        {
                            this.MouseDown += (MouseEventHandler)OjwMouseDown;
                            this.MouseMove += (MouseEventHandler)OjwMouseMove;
                            this.MouseUp += (MouseEventHandler)OjwMouseUp;
                            this.MouseWheel += (MouseEventHandler)OjwMouseWheel;
                        }
                        else
                        {
                            this.MouseDown -= (MouseEventHandler)OjwMouseDown;
                            this.MouseMove -= (MouseEventHandler)OjwMouseMove;
                            this.MouseUp -= (MouseEventHandler)OjwMouseUp;
                            this.MouseWheel -= (MouseEventHandler)OjwMouseWheel;
                        }
                    }
                }

                ~C3d()//CCsgl()
                {
                    //model.Destroy();
                    if (m_bClassEnd == true) m_bClassEnd = true;
                }
            
                public void InitGLContext()
                {
#if _GL_FLAT
                    Gl.glShadeModel(Gl.GL_FLAT);							    // Enable Flat Shading
#else
                    Gl.glShadeModel(Gl.GL_SMOOTH);							    // Enable Smooth Shading                    
#endif
                    //glShadeModel(GL_FLAT);							        // Enable Smooth Shading
                    Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.5f);				    // Black Background

                    Gl.glClearDepth(1.0f);									    // Depth Buffer Setup
#if true
                    Gl.glEnable(Gl.GL_DEPTH_TEST);							    // Enables Depth Testing
                    Gl.glDepthFunc(Gl.GL_LEQUAL);								// The Type Of Depth Testing To Do
#else
                    Gl.glDisable(Gl.GL_DEPTH_TEST);
#endif
                    Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);	// Really Nice Perspective Calculations

                    // Texture
                    Gl.glEnable(Gl.GL_TEXTURE_2D);									// Enable Texture Mapping
                }
                #endregion Initialize

                public void Init(PictureBox picMain)
                {
                    this.Parent = picMain;
                    this.Dock = DockStyle.Fill;
                    picMain.Controls.Add(this);
                    this.InitializeContexts();
                    //m_szDisplaySize = picMain.Size;
                    //SizeChange(m_szDisplaySize);
                    SizeChange(this.Size);
                }

                #region LoadTextures()
                /// <summary>
                /// Loads and creates the texture.
                /// </summary>
                /// 

                private const int _CNT_TEXTURE = 10;
                private uint[] m_puiTexture = new uint[_CNT_TEXTURE];
                public bool LoadTextures(String[] strFileName)
                {
                    bool bRet = true;
                    // The Files To Load
                    //string[] filename = {@"..\..\data\NeHeLesson17\Font.bmp", @"..\..\data\NeHeLesson17\Bumps.bmp"};
                    Bitmap bitmap = null;														// The Bitmap Image For Our Texture
                    Rectangle rectangle;														// The Rectangle For Locking The Bitmap In Memory
                    BitmapData bitmapData = null;												// The Bitmap's Pixel Data

                    int nCnt = strFileName.Length;
                    m_puiTexture = new uint[nCnt];

                    // Load The Bitmaps
                    try
                    {
                        Gl.glGenTextures(nCnt, m_puiTexture);									// Create 2 Textures

                        for (int i = 0; i < nCnt; i++)
                        {
                            bitmap = new Bitmap(strFileName[i]);								// Load The File As A Bitmap
                            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);					// Flip The Bitmap Along The Y-Axis
                            rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);		// Select The Whole Bitmap

                            // Get The Pixel Data From The Locked Bitmap
                            bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                            // Create Linear Filtered Texture
                            Gl.glBindTexture(Gl.GL_TEXTURE_2D, m_puiTexture[i]);
                            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, (int)Gl.GL_RGB8, bitmap.Width, bitmap.Height, 0, Gl.GL_BGR_EXT, Gl.GL_UNSIGNED_BYTE, bitmapData.Scan0);
                        }
                    }
                    catch// (Exception e)
                    {
                        bRet = false;
                        // Handle Any Exceptions While Loading Textures, Exit App
                        // 				string errorMsg = "An Error Occurred While Loading Texture:\n\t" + strFileName + "\n" + "\n\nStack Trace:\n\t" + e.StackTrace + "\n";
                        // 				MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        // 				App.Terminate();
                    }
                    finally
                    {
                        if (bitmap != null)
                        {
                            bitmap.UnlockBits(bitmapData);										// Unlock The Pixel Data From Memory
                            bitmap.Dispose();													// Clean Up The Bitmap
                        }
                    }
                    return bRet;
                }
                #endregion LoadTextures()

                // The top function is first called to the drawing.(Kor: 그리기를 하기위해 맨 처음 호출하는 함수)
                #region glDraw_Ready
                public void GetPickMousePoint(out int nX, out int nY)
                {
                    nX = m_nMouse_X_Left;
                    nY = m_nMouse_Y_Left;
                }

                public void glFlush() { Gl.glFlush(); }
                public void glDraw_Ready()
                {
                    //m_szDisplaySize = this.Size;
                    //SizeChange(m_szDisplaySize);
                    SizeChange(this.Size);

                    if (m_bPickMouseClick_Reserve == true)
                    {
                        m_bPickMouseClick = true;
                        m_bPickMouseClick_Reserve = false;
                    }

                    if (m_bPickMouseClick == true)
                    {
                        //Refresh();
                        Picking_Ready(m_nMouse_X_Left, m_nMouse_Y_Left);
                    }
                    else
                    {
                        //Refresh();
                        // Switch to normal rendering mode(Kor: 보통의 렌더링 모드로 전환)
                        //GL.glMatrixMode(GL.GL_MODELVIEW);

                        m_fColor_Back[0] = ((float)(m_BackColor.R) / 255.0f);  // R
                        m_fColor_Back[1] = ((float)(m_BackColor.G) / 255.0f);  // G
                        m_fColor_Back[2] = ((float)(m_BackColor.B) / 255.0f);  // B
                        Gl.glClearColor(m_fColor_Back[0], m_fColor_Back[1], m_fColor_Back[2], 1.0f);
                        Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
                        //Gl.glLoadIdentity();
                        //SetLight();
                    }
                }
                #endregion glDraw_Ready()

                #region OjwDraw()
                private int m_nStatus_GroupA = 0;
                private int m_nStatus_GroupB = 0;
                private int m_nStatus_GroupC = 0;
                private int m_nStatus_InverseKinematicsNumber = 255;
                private int[] m_anSelectedGroup = new int[3];
                private int m_nSelected_InverseKinematicsNumber = 255;
                private bool m_bStatus_Pick = false;
                private bool m_bStatus_Limit = false;
                public int GetStatus_GroupA() { return m_nStatus_GroupA; }
                public int GetStatus_GroupB() { return m_nStatus_GroupB; }
                public int GetStatus_GroupC() { return m_nStatus_GroupC; }
                public int GetStatus_Selected_InverseKinematicsNumber() { return m_nStatus_InverseKinematicsNumber; }
                public bool GetStatus_IsPicked() { return m_bStatus_Pick; }
                public bool GetStatus_IsLimit() { return m_bStatus_Limit; }
                //public bool OjwDraw(float[] afData, COjwDesignerHeader CHeader, ref String strMessage, ref int nGroupA, ref int nGroupB, ref int nGroupC, ref String strPick)
                private bool m_bDrawing = false;
                private List<int> m_lstSelect = new List<int>();
                public void SelectObject_Add(int nLine)
                {
                    m_lstSelect.Add(nLine);
                }
                private bool m_bSelectObjectEnabled = true;
                public bool SelectObject_Enable() { return m_bSelectObjectEnabled; }
                public void SelectObject_Enable(bool bEn) { m_bSelectObjectEnabled = bEn; }
                public void SelectObject_Clear()
                {
                    m_lstSelect.Clear();
                }
                public bool SelectObject_Check(int nLine)
                {
                    bool bRet = false;
                    foreach (int nItem in m_lstSelect)
                    {
                        if (nItem == nLine)
                        {
                            bRet = true; 
                            break;
                        }
                    }
                    return bRet;
                }

                private bool m_bJoystic = false;
                public void InitJoystic()
                {
                    Glut.glutInit();
                    m_bJoystic = true;
                    int nRet = Glut.glutDeviceGet(Glut.GLUT_HAS_JOYSTICK);
                    if (nRet != 0)
                    {
                        Glut.glutJoystickFunc(new Glut.JoystickCallback(JoystickFunc), 100);
                        Glut.glutMainLoop();
                    }
                }

                private void JoystickFunc(int buttonMask, int x, int y, int z)
                {
                    CMessage.Write("Joystick:" + buttonMask.ToString());
                }

                //private int m_nFunctionNumber = -1;
                public void OjwDraw(float[] afData, COjwDesignerHeader CHeader, out int nGroupA, out int nGroupB, out int nGroupC, out int nInverseKinematicsNumber, out bool bPick, out bool bLimit)
                {
                    //if (m_bJoystic == true)
                    //{
                    //    CMessage.Write2(
                    //        " joystick " + Glut.glutDeviceGet(Glut.GLUT_HAS_JOYSTICK).ToString() +
                    //        " - buttons " + Glut.glutDeviceGet(Glut.GLUT_JOYSTICK_BUTTONS).ToString() +
                    //        " - axes " + Glut.glutDeviceGet(Glut.GLUT_JOYSTICK_AXES).ToString() + " \r\n"
                    //        );
                    //}
                    bLimit = false;
                    bPick = false;
                    nGroupA = nGroupB = nGroupC = 0;
                    nInverseKinematicsNumber = 255;

                    if (m_bFileOpening == true) return;
                    if (m_bDrawing == true) return;
                    m_bDrawing = true;

                    try
                    {
                        int i;
                        #region Pre-Checking the function control
                        //int nInverseNum = m_nFunctionNumber;
                        if (GetFunctionNumber() >= 0)
                        {
                            if (m_CHeader.pSOjwCode.Length > GetFunctionNumber())
                            {
                                if (m_CHeader.pSOjwCode[GetFunctionNumber()].nMotor_Max > 0)
                                {
                                    CKinematics.CInverse.SetValue_ClearAll(ref m_CHeader.pSOjwCode[GetFunctionNumber()]);
                                    //CKinematics.CInverse.SetValue_X(m_dPos_X);
                                    //CKinematics.CInverse.SetValue_Y(m_dPos_Y);
                                    //CKinematics.CInverse.SetValue_Z(m_dPos_Z);

                                    double[] adMot = new double[m_afMot.Length];
                                    //Buffer.BlockCopy(GetData(), 0, adMot, 0, adMot.Length);
                                    for (i = 0; i < m_afMot.Length; i++) adMot[i] = (double)m_afMot[i];
                                    CKinematics.CInverse.SetValue_Motor(adMot);
                                    adMot = null;

                                    CKinematics.CInverse.CalcCode(ref m_CHeader.pSOjwCode[GetFunctionNumber()]);
                                    int nMotCnt = m_CHeader.pSOjwCode[GetFunctionNumber()].nMotor_Max;
                                    for (i = 0; i < nMotCnt; i++)
                                    {
                                        int nMotNum = m_CHeader.pSOjwCode[GetFunctionNumber()].pnMotor_Number[i];
                                        SetData(nMotNum, (float)CKinematics.CInverse.GetValue_Motor(nMotNum));
                                    }
                                }
                            }
                        }
                        #endregion Pre-Checking the function control

                        glDraw_Ready();

                        #region Dh-notation ball Drawing
                        if (IsTestDh() == true)
                        {
                            InitPosAngle();
                            OjwTranslate(m_afDhPoint[0] + m_afDhInitPoint[0], m_afDhPoint[1] + m_afDhInitPoint[1], m_afDhPoint[2] + m_afDhInitPoint[2]);
                            OjwBall_Outside(false, m_cDh, m_fDhAlpha, m_fDhSize, 20, 0, 0, 0, 0, 0, 0);

                            float fSize = m_fDhSize;// 10.0f;
                            float fMulti = fSize / 10.0f * 2.0f;
                            InitPosAngle();
                            OjwTranslate(m_afDhPoint[0] + m_afDhInitPoint[0], m_afDhPoint[1] + m_afDhInitPoint[1], m_afDhPoint[2] + m_afDhInitPoint[2]);

                            OjwTranslate(m_afDhAngle_X[0] * fMulti, m_afDhAngle_X[1] * fMulti, m_afDhAngle_X[2] * fMulti);
                            OjwBall(true, Color.Red, m_fDhAlpha, fSize, 50);

                            InitPosAngle();
                            OjwTranslate(m_afDhPoint[0] + m_afDhInitPoint[0], m_afDhPoint[1] + m_afDhInitPoint[1], m_afDhPoint[2] + m_afDhInitPoint[2]);

                            OjwTranslate(m_afDhAngle_Y[0] * fMulti, m_afDhAngle_Y[1] * fMulti, m_afDhAngle_Y[2] * fMulti);
                            OjwBall(true, Color.Green, m_fDhAlpha, fSize, 50);

                            //Axis_Y(true, Color.Green, m_fDhAlpha, 5, 20);

                            InitPosAngle();
                            OjwTranslate(m_afDhPoint[0] + m_afDhInitPoint[0], m_afDhPoint[1] + m_afDhInitPoint[1], m_afDhPoint[2] + m_afDhInitPoint[2]);
                            //OjwRotation(m_afDhAngle[0, 0], m_afDhAngle[0, 1], m_afDhAngle[0, 2]);

                            OjwTranslate(m_afDhAngle_Z[0] * fMulti, m_afDhAngle_Z[1] * fMulti, m_afDhAngle_Z[2] * fMulti);
                            OjwBall(true, Color.Blue, m_fDhAlpha, fSize, 50);

                            // Display axis and ball
                            //OjwBall_Outside(false, m_cDh, m_fDhAlpha, m_fDhSize, 20, 0, 0, 0, 0, 0, 0);

                            //Axis_X(true, Color.Coral, 1.0f, 5, 20);
                            //Axis_Y(true, Color.Green, 1.0f, 5, 20);
                            //Axis_Z(true, Color.Blue, 1.0f, 5, 20);
                            ////
                            //////////////
                        }
                        /////////////////////////
                        #endregion Dh-notation ball Drawing

                        InitPosAngle_WithAxis();

                        //// The actual part to be drawn(Kor: 실제 그려질 부분) ////
                        COjwDisp CDisp = new COjwDisp();
                        if (_CNT_FILEOPEN != m_nBackupCnt_HistoryFileOpen)
                        {
                            m_nBackupCnt_HistoryFileOpen = _CNT_FILEOPEN;
                            Array.Clear(m_abMake, 0, 4096);
                        }
                        
                        int nCnt = OjwDispAll.GetCount();
                        if (IsVirtualClass() == true) nCnt++;

                        #region Main Drawing
                        //for (i = 0; i < OjwDispAll.GetCount(); i++)
                        for (i = 0; i < nCnt; i++)
                        {
                            float fAlpha = m_fAlpha;
                            if ((IsVirtualClass() == true) && ((nCnt - 1) == i))
                            {
                                //m_fAlpha /= 2.0f; // ojw5014(Set alpha for virtual object
                                CDisp = OjwVirtualDisp;
                            }
                            else CDisp = OjwDispAll.GetData(i);
                            if (CDisp != null)
                            {
                                if ((CDisp.nName >= 0) && (afData.Length > CDisp.nName))
                                    CDisp.fAngle = afData[CDisp.nName];// +GetData(CDisp.nName);

                                // Limit Check(Kor: 각도의 Limit 체크) //
                                if (CDisp.nName >= 0)
                                {
                                    if ((CHeader.pSMotorInfo[CDisp.nName].fLimit_Down != 0) && (CHeader.pSMotorInfo[CDisp.nName].fLimit_Down >= CDisp.fAngle)) bLimit = true;
                                    if ((CHeader.pSMotorInfo[CDisp.nName].fLimit_Up != 0) && (CHeader.pSMotorInfo[CDisp.nName].fLimit_Up <= CDisp.fAngle)) bLimit = true;
                                }
                                m_nDrawClass_Pos = i;
                                OjwDraw_Class(CDisp);
                            }
                            if (m_fAlpha != fAlpha) m_fAlpha = fAlpha;
                        }
                        #endregion Main Drawing

                        #region Test Drawing
                        if (IsTestCircle() == true)
                        {
                            float fX, fY, fZ;
                            float fSize = m_fTestSize;// 10.0f;
                            float fAlpha = 1.0f;
                            int nAxisLength = 20;
                            InitPosAngle();
                            GetPos_Test(out fX, out fY, out fZ);
                            OjwTranslate(fX, fY, fZ);
                            GetAngle_Test(out fX, out fY, out fZ);
                            OjwRotation(fX, fY, fZ);
                            OjwBall_Outside(true, GetColor_Test(), fAlpha, fSize, nAxisLength, 0, 0, 0, 0, 0, 0);
                            if (IsTestAxis() == true)
                            {
                                Axis_X(true, Color.Coral, 1.0f, 5, 20);
                                Axis_Y(true, Color.Green, 1.0f, 5, 20);
                                Axis_Z(true, Color.Blue, 1.0f, 5, 20);
                            }
                        }
                        #endregion Test Drawing
                                                
                        #region User
                        int nCntUser = OjwDispAll_User.GetCount();
                        for (i = 0; i < nCntUser; i++)
                        {
                            float fAlpha = m_fAlpha;
                            CDisp = OjwDispAll_User.GetData(i);
                            if (CDisp != null)
                            {
                                if ((CDisp.nName >= 0) && (afData.Length > CDisp.nName))
                                    CDisp.fAngle = afData[CDisp.nName];// +GetData(CDisp.nName);

                                // Limit Check(Kor: 각도의 Limit 체크) //
                                if (CDisp.nName >= 0)
                                {
                                    if ((CHeader.pSMotorInfo[CDisp.nName].fLimit_Down != 0) && (CHeader.pSMotorInfo[CDisp.nName].fLimit_Down >= CDisp.fAngle)) bLimit = true;
                                    if ((CHeader.pSMotorInfo[CDisp.nName].fLimit_Up != 0) && (CHeader.pSMotorInfo[CDisp.nName].fLimit_Up <= CDisp.fAngle)) bLimit = true;
                                }

                                OjwDraw_Class(CDisp);
                            }
                            if (m_fAlpha != fAlpha) m_fAlpha = fAlpha;
                        }
                        //#region Init Again
                        //InitPosAngle();
                        //m_afCalcPos[0] = 0;
                        //m_afCalcPos[1] = 0;
                        //m_afCalcPos[2] = 0;
                        //m_afCalcAngle[0] = 0;
                        //m_afCalcAngle[1] = 0;
                        //m_afCalcAngle[2] = 0;
                        //#endregion Init Again
                        #endregion User

                        

                        long lRet = glDraw_End();
                        bPick = IsPicking();
                        if (bPick == true)
                        {
                            if (lRet > 0)
                            {
                                int nX, nY;
                                GetPickMousePoint(out nX, out nY);
                                //int nGroupA, nGroupB, nGroupC;
                                GetPickingData(out nGroupA, out nGroupB, out nGroupC, out nInverseKinematicsNumber);
                                //                         if (strPick != null)
                                //                         {
                                //                             String strPickComment = OjwDispAll.GetString_PickingComment(nGroupA, nGroupB, nGroupC);
                                //                             strPick = "Object Name = " + CConvert.IntToStr(nGroupA) + ", " + CConvert.IntToStr(nGroupB) + ", " + CConvert.IntToStr(nGroupC) + "," + strPickComment + "[" + CConvert.IntToStr(nX) + ", " + CConvert.IntToStr(nY) + "]";
                                //                         }
                                if (nGroupA > 0)
                                {
                                    m_anSelectedGroup[0] = nGroupA;
                                    m_anSelectedGroup[1] = nGroupB;
                                    m_anSelectedGroup[2] = nGroupC;
                                    m_nSelected_InverseKinematicsNumber = nInverseKinematicsNumber;
                                }
                                
                            }
                            else if (lRet == 0)
                            {
                                int nX, nY;
                                GetPickMousePoint(out nX, out nY);
                                //if (strPick != null) strPick = "Object Name = Unknown Object[" + CConvert.IntToStr(nX) + ", " + CConvert.IntToStr(nY) + "]";

                                nGroupA = nGroupB = nGroupC = 0;
                            }
                            else
                            {
                                int nX, nY;
                                GetPickMousePoint(out nX, out nY);
                                //if (strPick != null) strPick = "Object Name = No Found[" + CConvert.IntToStr(nX) + ", " + CConvert.IntToStr(nY) + "]";
                            }                            
                            Clear_IsPicking();
                        }
                        CDisp = null;

                        m_nStatus_GroupA = nGroupA;
                        m_nStatus_GroupB = nGroupB;
                        m_nStatus_GroupC = nGroupC; 
                        m_nStatus_InverseKinematicsNumber = nInverseKinematicsNumber;
                        
                        m_bStatus_Pick = bPick;
                        m_bStatus_Limit = bLimit;
                        
                        m_bDrawing = false;
                    }
                    catch (System.Exception e)
                    {
                        MessageBox.Show(e.ToString());
                        
                        m_bDrawing = false;
                    }                    
                }
                public void OjwDraw(COjwDesignerHeader CHeader, out int nGroupA, out int nGroupB, out int nGroupC, out int nInverseKinematicsNumber, out bool bPick, out bool bLimit)
                {
                    OjwDraw(m_afMot, CHeader, out nGroupA, out nGroupB, out nGroupC, out nInverseKinematicsNumber, out bPick, out bLimit);
                }
                public void OjwDraw(out int nGroupA, out int nGroupB, out int nGroupC, out int nInverseKinematicsNumber, out bool bPick, out bool bLimit)
                {
                    OjwDraw(m_afMot, m_CHeader, out nGroupA, out nGroupB, out nGroupC, out nInverseKinematicsNumber, out bPick, out bLimit);
                }
                public void OjwDraw()
                {
                    OjwDraw(m_afMot, m_CHeader, out m_nStatus_GroupA, out m_nStatus_GroupB, out m_nStatus_GroupC, out m_nStatus_InverseKinematicsNumber, out m_bStatus_Pick, out m_bStatus_Limit);
                }
                public void OjwDraw(float[] afData, COjwDesignerHeader CHeader)
                {
                    OjwDraw(afData, CHeader, out m_nStatus_GroupA, out m_nStatus_GroupB, out m_nStatus_GroupC, out m_nStatus_InverseKinematicsNumber, out m_bStatus_Pick, out m_bStatus_Limit);
                }
                public void OjwDraw(COjwDesignerHeader CHeader)
                {
                    OjwDraw(m_afMot, CHeader, out m_nStatus_GroupA, out m_nStatus_GroupB, out m_nStatus_GroupC, out m_nStatus_InverseKinematicsNumber, out m_bStatus_Pick, out m_bStatus_Limit);
                }
                public void OjwDraw(float[] afData)
                {
                    OjwDraw(afData, m_CHeader, out m_nStatus_GroupA, out m_nStatus_GroupB, out m_nStatus_GroupC, out m_nStatus_InverseKinematicsNumber, out m_bStatus_Pick, out m_bStatus_Limit);
                }
                public void OjwDraw_Serve(int nCmd_GroupType, // 0, 1, 2
                                            int nCmd_GroupNum,
                    //int nCmd_GroupA, int nCmd_GroupA_Or_1, int nCmd_GroupA_Or_2, // Possible combination of 3 groups.(Kor: 3개의 그룹을 조합가능하다.)
                    //int nCmd_GroupB, int nCmd_GroupB_Or_1, int nCmd_GroupB_Or_2, // Possible combination of 3 groups.(Kor: 3개의 그룹을 조합가능하다.)
                    //int nCmd_GroupC, int nCmd_GroupC_Or_1, int nCmd_GroupC_Or_2, // Possible combination of 3 groups.(Kor: 3개의 그룹을 조합가능하다.)
                                            float[] afData, COjwDesignerHeader CHeader,
                                            bool bSecondDrawingMode,
                                            out int nGroupA, out int nGroupB, out int nGroupC,
                                            out int nInverseKinematicsNumber,
                                            out bool bPick, out bool bLimit)
                {
                    bLimit = false;
                    bPick = false;
                    nGroupA = nGroupB = nGroupC = 0;
                    nInverseKinematicsNumber = 0;
                    float fMainScale = (float)GetScale();
                    try
                    {
                        if (m_bPickMouseClick_Reserve == true)
                        {
                            m_bPickMouseClick = true;
                            m_bPickMouseClick_Reserve = false;
                        }
                        // Clipping
                        if (nCmd_GroupType > 2) nCmd_GroupType = 2;
                        if (nCmd_GroupType < 0) nCmd_GroupType = 0;

                        COjwDisp CDisp = new COjwDisp();
                        int nStart = -1;
                        int nEnd = OjwDispAll.GetCount();
                        int nDrawNum = -1;
                        float[] fInitAngle = new float[3];
                        float[] fInitPos = new float[3];
                        float fScale = fMainScale;
                        for (int i = 0; i < OjwDispAll.GetCount(); i++)
                        {
#if false
                            //                     #region EtcGrouping // 앞 뒤로 2개까지 같은 그룹이 있다면 설사 다른 그룹이더라도 같은 그룹으로 묶어서 그리도록 한다.
                            //                     bool bEtcGroup = false;
                            //                     #endregion EtcGrouping
#endif
                            CDisp = OjwDispAll.GetData(i);
                            if (CDisp != null)
                            {
                                if (
                                    //((nCmd_GroupType == 0) && ((CDisp.nPickGroup_A == nCmd_GroupNum)) || (CDisp.nPickGroup_C > 0)) ||
                                    ((nCmd_GroupType == 0) && (CDisp.nPickGroup_A == nCmd_GroupNum)) ||
                                    ((nCmd_GroupType == 1) && (CDisp.nPickGroup_B == nCmd_GroupNum)) ||
                                    //((nCmd_GroupType == 2) && (CDisp.nPickGroup_C == nCmd_GroupNum))
                                    ((nCmd_GroupType == 2) && ((CDisp.nPickGroup_A == nCmd_GroupNum) && (CDisp.nPickGroup_C > 0)))
                                    //                             (
                                    //                                 (CDisp.nPickGroup_A > 0) &&
                                    //                                 (
                                    //                                     ((nCmd_GroupType == 0) && ((CDisp.nPickGroup_A == nCmd_GroupA) || (CDisp.nPickGroup_A == nCmd_GroupA_Or_1) || (CDisp.nPickGroup_A == nCmd_GroupA_Or_2))) ||
                                    //                                     ((nCmd_GroupType == 1) && (CDisp.nPickGroup_A == nCmd_GroupA) && ((CDisp.nPickGroup_B == nCmd_GroupB) || (CDisp.nPickGroup_B == nCmd_GroupB_Or_1) || (CDisp.nPickGroup_B == nCmd_GroupB_Or_2))) ||
                                    //                                     ((nCmd_GroupType == 2) && (CDisp.nPickGroup_A == nCmd_GroupA) && (CDisp.nPickGroup_B == nCmd_GroupB) && ((CDisp.nPickGroup_C == nCmd_GroupC) || (CDisp.nPickGroup_C == nCmd_GroupC_Or_1) || (CDisp.nPickGroup_C == nCmd_GroupC_Or_2)))
                                    //                                 )
                                    //                             )
                                    //||
                                    //(bEtcGroup == true)
                                )
                                {
                                    if (nStart < 0)
                                    {
                                        nStart = i;

                                        //nDrawNum = (bSecondDrawingMode == false)?3:4;
                                        if (nCmd_GroupType != 1)
                                        {
                                            nDrawNum = (bSecondDrawingMode == false) ? 3 : 4;

                                            fInitAngle[0] = CDisp.afRot[nDrawNum].pan;
                                            fInitAngle[1] = CDisp.afRot[nDrawNum].tilt;
                                            fInitAngle[2] = CDisp.afRot[nDrawNum].swing;
                                            fInitPos[0] = CDisp.afTrans[nDrawNum].x;
                                            fInitPos[1] = CDisp.afTrans[nDrawNum].y;
                                            fInitPos[2] = CDisp.afTrans[nDrawNum].z;
                                            //fScale = (bSecondDrawingMode == false) ? CDisp.fScale_Serve0 : CDisp.fScale_Serve1;
                                            fScale = (nCmd_GroupType == 0) ? CDisp.fScale_Serve0 : CDisp.fScale_Serve1;
                                            if (fScale <= 0) fScale = fMainScale;
                                        }
                                    }
                                    nEnd = i;
                                }
                            }
                        }

                        //Refresh();
                        SetLight_Position(-50, 0, 10, -10);
                        SetLight_Ambient(0.7f, 0.7f, 0.7f, 0.5f);

                        SetScale(fScale);
                        glDraw_Ready();

                        // Display Axis(Kor: 축 표시)
                        if ((nDrawNum == 3) || (nDrawNum == 4))
                        {
                            Gl.glLoadIdentity();
                            SetLight(); // ojw5014
                            OjwRotation(fInitAngle[0], fInitAngle[1], fInitAngle[2]);
                            OjwTranslate(fInitPos[0], fInitPos[1], fInitPos[2]);
                        }
                        else
                        {
                            InitPosAngle(); //=> Instead of above coding(Kor: 위의 코딩으로 대치)
                        }
                        fInitAngle = null;
                        fInitPos = null;

                        //// The actual part to be drawn(Kor: 실제 그려질 부분) ////
                        // => Only to be drawn into the same group so laced unconditionally.
                        // Kor: => 무조건 같은 그룹으로만 엮이도록 그려야 한다.
                        if ((nStart > 0) && (nEnd > 0)) 
                        {
                            // Actually drawing the part to be drawn(Kor: 그려야 할 부분을 실제로 그리기)
                            for (int i = nStart; i <= nEnd; i++)
                            {
                                CDisp = OjwDispAll.GetData(i);
                                if (CDisp != null)
                                {

                                    if ((CDisp.nName >= 0) && (afData.Length > CDisp.nName))
                                        CDisp.fAngle = afData[CDisp.nName];// +GetData(CDisp.nName);

                                    // Limit Check(Kor: 각도의 Limit 체크) //
                                    if (CDisp.nName >= 0)
                                    {
                                        if ((CHeader.pSMotorInfo[CDisp.nName].fLimit_Down != 0) && (CHeader.pSMotorInfo[CDisp.nName].fLimit_Down >= CDisp.fAngle)) bLimit = true;
                                        if ((CHeader.pSMotorInfo[CDisp.nName].fLimit_Up != 0) && (CHeader.pSMotorInfo[CDisp.nName].fLimit_Up <= CDisp.fAngle)) bLimit = true;
                                    }
                                    OjwDraw_Class(CDisp);
                                }
                            }
                        }

                        long lRet = glDraw_End();
                        bPick = IsPicking();
                        if (bPick == true)
                        {
                            if (lRet > 0)
                            {
                                int nX, nY;
                                GetPickMousePoint(out nX, out nY);
                                //int nGroutA, nGroupB, nGroupC;
                                GetPickingData(out nGroupA, out nGroupB, out nGroupC, out nInverseKinematicsNumber);
                            }
                            else if (lRet == 0)
                            {
                                int nX, nY;
                                GetPickMousePoint(out nX, out nY);
                            }
                            else
                            {
                                int nX, nY;
                                GetPickMousePoint(out nX, out nY);
                            }
                            Clear_IsPicking();
                        }
                        CDisp = null;
                        SetScale(fMainScale);
                    }
                    catch //(System.Exception e)
                    {
                        SetScale(fMainScale);
                    }
                }
                #endregion OjwDraw()

                #region glDraw_End()
                public long glDraw_End()
                {

                    if (m_bPickMouseClick == true)
                    {
                        Picking_End();
                        //////////////////////////////////

                        uint unName = Picking_Check();
                        glFlush();
                        if (unName > 0)
                        {
                            return (long)unName;
                        }
                        return 0;
                    }
                    else
                    {
                        glFlush();
                    }
                    Refresh();
                    return -1;
                }
                #endregion glDraw_End()

                #region SizeChange
                private float m_fW;
                private float m_fH;
                private void Ortho(float w, float h, float fRange)
                {
#if false
                    m_fW = w;
                    m_fH = h;
                    if (w <= h)
                        Gl.glOrtho(-fRange, fRange, -fRange, fRange * h / w, -fRange, fRange);
                    else
                        Gl.glOrtho(-fRange * , fRange * w / h, -fRange, fRange, -fRange, fRange);
#else
                    m_fW = w;
                    m_fH = h;
                    float fRatio = (h / ((w == 0) ? 1 : w));
                    if (w <= h)
                        Gl.glOrtho(-fRange, fRange, -fRange * fRatio, fRange * fRatio, -fRange, fRange);
                    else
                        Gl.glOrtho(-fRange / fRatio, fRange / fRatio, -fRange, fRange, -fRange, fRange);
                    //glOrtho (-fRange, fRange*w/h, -fRange, fRange, -fRange, 1.00);
#endif
                }
                private void Ortho(float fRange)
                {
#if false
                    float w = m_fW;
                    float h = m_fH;
                    if (w <= h)
                        Gl.glOrtho(-fRange, fRange, -fRange, fRange * h / w, -fRange, fRange);
                    else
                        Gl.glOrtho(-fRange, fRange * w / h, -fRange, fRange, -fRange, fRange);
                    //glOrtho (-fRange, fRange*w/h, -fRange, fRange, -fRange, 1.00);
#else
                    float w = m_fW;
                    float h = m_fH;
                    float fRatio = (h / ((w == 0) ? 1 : w));
                    if (w <= h)
                        Gl.glOrtho(-fRange, fRange, -fRange * fRatio, fRange * fRatio, -fRange, fRange);
                    else
                        Gl.glOrtho(-fRange / fRatio, fRange / fRatio, -fRange, fRange, -fRange, fRange);
#endif
                }

                private int m_nWidth = 0;
                private int m_nHeight = 0;
                private const float _RATIO = 800.0f;
                public void SizeChange(Size s)
                {
                    m_nWidth = s.Width;
                    m_nHeight = s.Height;
                    double dW = (double)(s.Width);
                    double dH = (double)(s.Height);

                    float fRatio = _RATIO * m_fScale;

                    Gl.glViewport(0, 0, s.Width, s.Height);
                    Gl.glMatrixMode(Gl.GL_PROJECTION);
                    Gl.glLoadIdentity();
                    Ortho((float)s.Width, (float)s.Height, fRatio);
                    Gl.glDepthRange(-fRatio, fRatio);
                    Gl.glMatrixMode(Gl.GL_MODELVIEW);
                    Gl.glLoadIdentity();

                    SetLight();
                }
                #endregion SizeChange

                // Set the lights(Kor: 빛 설정)
                #region Light
                private bool m_bEnable_Light = true;
                public void Enable_Light(bool bEnable) { m_bEnable_Light = bEnable; }

                private float[] m_light0_position = new float[4] { 0.0f, 0.0f, -300.0f, 1.0f };
                private float[] m_light1_position = new float[4] { 10.0f, 20.0f, 1.0f, 1.0f };

                private float[] m_light0_direction = new float[3] { -0.7f, 0.2f, 0.7f };
                private float[] m_light1_direction = new float[3] { -0.5f, -0.5f, 1.0f };

                private float[] m_ambient = new float[4] { 0.4f, 0.4f, 0.4f, 1.0f };
                private float[] m_diffuseLight = new float[4] { 1.0f, 1.0f, 1.0f, 1.0f };
                private float[] m_specular = new float[4] { 0.8f, 0.8f, 0.8f, 1.0f };

                private float[] m_mat_diffuse = new float[4] { 1.0f, 1.0f, 1.0f, 1.0f };//{ 0.9f, 0.9f, 0.9f, 1.0f };
                private float[] m_mat_specular = new float[4] { 0.35f, 0.35f, 0.35f, 1.0f };
                private float[] m_mat_ambient = new float[4] { 0.7f, 0.7f, 0.7f, 0.7f };
                private float[] m_mat_shiness = new float[1] { 80.0f };

                //private int m_nShiness = 128;
                private float m_fSpot = 85.0f;
                private float m_fExponent = 2.0f;//1.0f;
                public void SetLight_Position(float fA, float fB, float fC, float fD) { m_light0_position[0] = fA; m_light0_position[1] = fB; m_light0_position[2] = fC; m_light0_position[3] = fD; }
                public void SetLight_Ambient(float fA, float fB, float fC, float fD) { m_ambient[0] = fA; m_ambient[1] = fB; m_ambient[2] = fC; m_ambient[3] = fD; }
                public void SetLight_diffuseLight(float fA, float fB, float fC, float fD) { m_diffuseLight[0] = fA; m_diffuseLight[1] = fB; m_diffuseLight[2] = fC; m_diffuseLight[3] = fD; }
                public void SetLight_Specular(float fA, float fB, float fC, float fD) { m_specular[0] = fA; m_specular[1] = fB; m_specular[2] = fC; m_specular[3] = fD; }
                public void SetLight_Direction(float fA, float fB, float fC) { m_light0_direction[0] = fA; m_light0_direction[1] = fB; m_light0_direction[2] = fC; }
            
                public void SetMaterial_Ambient(float fA, float fB, float fC, float fD) { m_mat_ambient[0] = fA; m_mat_ambient[1] = fB; m_mat_ambient[2] = fC; m_mat_ambient[3] = fD; }
                public void SetMaterial_diffuse(float fA, float fB, float fC, float fD) { m_mat_diffuse[0] = fA; m_mat_diffuse[1] = fB; m_mat_diffuse[2] = fC; m_mat_diffuse[3] = fD; }
                public void SetMaterial_Specular(float fA, float fB, float fC, float fD) { m_mat_ambient[0] = fA; m_mat_ambient[1] = fB; m_mat_ambient[2] = fC; m_mat_ambient[3] = fD; }

                public void SetLight_Exponent(float fValue) { m_fExponent = fValue; }
                public void SetLight_Shiness(float fShiness) { m_mat_shiness[0] = fShiness; }
                public void SetLight_Spot(float fSpot) { m_fSpot = fSpot; }

                private void SetLight2()
                {
                    //Gl.glEnable(Gl.GL_LIGHTING);     // Enable lighting(Kor: 조명 활성화)
                    Gl.glEnable(Gl.GL_LIGHT1);

                    Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, m_light1_position);
                    Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_DIFFUSE, m_mat_diffuse);
                    Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_SPECULAR, m_mat_specular);
                    Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_AMBIENT, m_mat_ambient);
                    Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_SPOT_DIRECTION, m_light1_direction);// Direction of light(Kor: 빛의 방향)

                    // Set the exponent - The higher this value is increasing rapidly in the outer darkness at the center axis.
                    // Kor: 승수 설정 - 이 값이 커질수록 중심축 방향에서 외곽으로 갈수록 급격히 어두워 진다.
                    Gl.glLightf(Gl.GL_LIGHT1, Gl.GL_SPOT_EXPONENT, m_fExponent);//+ 1); 

                    Gl.glLightf(Gl.GL_LIGHT1, Gl.GL_SPOT_CUTOFF, m_fSpot);//60.0f); // Set the spot value(Kor: 조명각 설정(스포트라이트))
#if false
                    Gl.glMaterialfv(Gl.GL_FRONT_FACE, Gl.GL_DIFFUSE, m_mat_diffuse);
                    Gl.glMaterialfv(Gl.GL_FRONT_FACE, Gl.GL_SPECULAR, m_mat_specular);
                    //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT, m_mat_ambient);
                    Gl.glMaterialfv(Gl.GL_FRONT_FACE, Gl.GL_SHININESS, m_mat_shiness);
                    //Gl.glMateriali(Gl.GL_FRONT, Gl.GL_SHININESS, m_nShiness);//128); // 1 - 128  
                    Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, m_mat_ambient);
#endif
                }
                private bool m_bDisplay_Edge = true;
                private bool m_bDetail = true;
                public void SetDisplay_Edge(bool bEn) { m_bDisplay_Edge = bEn; }
                public void SetDisplay_Detail(bool bEn) { m_bDetail = bEn; }
                private int m_nCWMode = Gl.GL_CW;
                private void SetLight()
                {
                    if (m_bEnable_Light == true)
                    {
#if true
                        Gl.glEnable(Gl.GL_LIGHTING);     // Enable Light
                        Gl.glEnable(Gl.GL_LIGHT0);

                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, m_light0_position);
                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPOT_DIRECTION, m_light0_direction); // Direction of light(Kor: 빛의 방향)
                        Gl.glLightf(Gl.GL_LIGHT0, Gl.GL_SPOT_CUTOFF, m_fSpot);//60.0f); // Set the spot value(Kor: 조명각 설정(스포트라이트))
                        // Set the exponent - The higher this value is increasing rapidly in the outer darkness at the center axis.
                        // Kor: 승수 설정 - 이 값이 커질수록 중심축 방향에서 외곽으로 갈수록 급격히 어두워 진다.
                        Gl.glLightf(Gl.GL_LIGHT0, Gl.GL_SPOT_EXPONENT, m_fExponent);

                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, m_diffuseLight);
                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, m_specular);
                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, m_ambient);

                        // The light weakens with distance.(Kor: 거리에 따른 빛의 약화)
                        //float fValue = 1.0f;
                        //Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_CONSTANT_ATTENUATION, ref fValue);
                        //fValue = 2.0f; Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_LINEAR_ATTENUATION, ref fValue);
                        //fValue = 3.0f; Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_QUADRATIC_ATTENUATION, ref fValue);

                        // Material properties are set to follow glColor value(Kor: 재질 속성이 glColor 값을 따르게끔 설정)
                        Gl.glColorMaterial(Gl.GL_FRONT_FACE, Gl.GL_AMBIENT_AND_DIFFUSE);
                        //Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, m_diffuseLight);
                        // Enable color tracking(Kor: 색상 트래킹을 사용하게끔 설정)
                        Gl.glEnable(Gl.GL_COLOR_MATERIAL);

                        //Gl.glEnable(Gl.GL_CULL_FACE);    // Not drawn to overlap the back(Kor: 겹치는 뒷면을 그리지 않음)
                        Gl.glEnable(Gl.GL_DEPTH_TEST);	// Depth buffer enable - Clear face hidden(Kor: 깊이 버퍼 활성화 - 숨겨진 면 지우기)
                        Gl.glLightModeli(Gl.GL_LIGHT_MODEL_TWO_SIDE, Gl.GL_TRUE);

#if _GL_FLAT
                        Gl.glShadeModel(Gl.GL_FLAT);							    // Enable Flat Shading
#else
                        Gl.glShadeModel(Gl.GL_SMOOTH);							    // Enable Smooth Shading                                                     
#endif
                        //Gl.glEnable(Gl.GL_POINT_SMOOTH);
                        //Gl.glHint(Gl.GL_POINT_SMOOTH_HINT, Gl.GL_FASTEST);
                        //Gl.glEnable(Gl.GL_LINE_SMOOTH);
                        //Gl.glHint(Gl.GL_LINE_SMOOTH_HINT, Gl.GL_FASTEST);
                        //Gl.glEnable(Gl.GL_POLYGON_SMOOTH);
                        //Gl.glHint(Gl.GL_POLYGON_SMOOTH_HINT, Gl.GL_FASTEST);

                        //Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST); // Really Nice Perspective Calculations


                        Gl.glEnable(Gl.GL_NORMALIZE);

                        Gl.glFrontFace(m_nCWMode);//Gl.GL_CW);       // CCW 
                        
                        // Set Alpha Environment
                        Gl.glEnable(Gl.GL_BLEND);
                        Gl.glEnable(Gl.GL_ALPHA_TEST);
                        //Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                        Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA); // 1
                        //Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_DST_ALPHA);

                        //Gl.glEnable(Gl.GL_CULL_FACE);   
                        //Gl.glCullFace(Gl.GL_BACK);   

                        //Gl.glEdgeFlag(Gl.GL_FALSE );
                        Gl.glEdgeFlag((m_bDisplay_Edge == true) ? Gl.GL_TRUE : Gl.GL_FALSE);//Gl.GL_TRUE); // 1
#if false //20150116
                Gl.glMaterialfv(Gl.GL_FRONT_FACE, Gl.GL_DIFFUSE, m_mat_diffuse);
                Gl.glMaterialfv(Gl.GL_FRONT_FACE, Gl.GL_SPECULAR, m_mat_specular);
                //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT, m_mat_ambient);
                Gl.glMaterialfv(Gl.GL_FRONT_FACE, Gl.GL_SHININESS, m_mat_shiness);
                //Gl.glMateriali(Gl.GL_FRONT, Gl.GL_SHININESS, m_nShiness);//128); // 1 - 128  
                Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, m_mat_ambient);
#endif

#else
#if true
                        ////Gl.glEnable(Gl.GL_MULTISAMPLE);
                        Gl.glEnable(Gl.GL_BLEND);
                        Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

                        ////
                        //Gl.glEnable(Gl.GL_TEXTURE_2D);

                        Gl.glEnable(Gl.GL_POINT_SMOOTH);
                        Gl.glHint(Gl.GL_POINT_SMOOTH_HINT, Gl.GL_FASTEST);
                        //Gl.glHint(Gl.GL_POINT_SMOOTH_HINT, Gl.GL_FASTEST);
                        Gl.glEnable(Gl.GL_LINE_SMOOTH);
                        Gl.glHint(Gl.GL_LINE_SMOOTH_HINT, Gl.GL_FASTEST);
                        Gl.glEnable(Gl.GL_POLYGON_SMOOTH);
                        Gl.glHint(Gl.GL_POLYGON_SMOOTH_HINT, Gl.GL_FASTEST);

                        //Gl.glEnable(Gl.GL_AUTO_NORMAL);


                        //// 조명을 사용하게 끔 설정n
                        //Gl.glShadeModel(Gl.GL_SMOOTH);	//구로 셰이딩
                        //Gl.glShadeModel(Gl.GL_FLAT);	//구로 셰이딩
                        Gl.glEnable(Gl.GL_LIGHTING);     //조명 활성화
                        Gl.glEnable(Gl.GL_LIGHT0);

                        ////Gl.glDepthFunc(Gl.GL_ALWAYS);								// The Type Of Depth Testing To Do
                        //Gl.glDepthFunc(Gl.GL_LEQUAL);								// The Type Of Depth Testing To Do
                        //Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_FASTEST);	// Really Nice Perspective Calculations


                        //Gl.glFrontFace(Gl.GL_CCW);       // 반시계 방향의 와인딩 적용

                        // 약간의 주변광을 넣어 물체가 보이도록 한다.
                        //SetLight_diffuseLight(0.2f, 0.2f, 0.2f, 1.0f); // ojw5014
                        Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, m_diffuseLight);

                        // 조명 설정
                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, m_light0_position);
                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, m_diffuseLight);
                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, m_specular);
                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, m_ambient);
                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPOT_DIRECTION, m_light0_direction);// 빛의 방향

                        Gl.glLightf(Gl.GL_LIGHT0, Gl.GL_SPOT_EXPONENT, m_fExponent); // 승수 설정 - 이 값이 커질수록 중심축 방향에서 외곽으로 갈수록 급격히 어두워 진다.

                        Gl.glLightf(Gl.GL_LIGHT0, Gl.GL_SPOT_CUTOFF, m_fSpot);//60.0f); // 조명각 설정(스포트라이트)

#if false //20150116
                Gl.glMaterialfv(Gl.GL_FRONT_FACE, Gl.GL_DIFFUSE, m_mat_diffuse);
                Gl.glMaterialfv(Gl.GL_FRONT_FACE, Gl.GL_SPECULAR, m_mat_specular);
                //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT, m_mat_ambient);
                Gl.glMaterialfv(Gl.GL_FRONT_FACE, Gl.GL_SHININESS, m_mat_shiness);
                //Gl.glMateriali(Gl.GL_FRONT, Gl.GL_SHININESS, m_nShiness);//128); // 1 - 128  
                Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, m_mat_ambient);
#endif


                        //float[] Emission = new float[4] { 0.3f, 0.3f, 0.3f, 0.5f };//발광색(R,G,B,A)
                        //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_EMISSION, Emission);// 발광체 선언

                        Gl.glLightModeli(Gl.GL_LIGHT_MODEL_TWO_SIDE, Gl.GL_TRUE);

                        // 재질 속성이 glColor 값을 따르게끔 설정
                        Gl.glColorMaterial(Gl.GL_FRONT_FACE, Gl.GL_AMBIENT_AND_DIFFUSE);


                        //Gl.glEnable(Gl.GL_TEXTURE_2D);
                        //Gl.glBindTexture(Gl.GL_TEXTURE_2D, g_Texture[pObject->materialID]);


                        // 색상 트래킹을 사용하게끔 설정
                        Gl.glEnable(Gl.GL_COLOR_MATERIAL);

                        //Gl.glEnable(Gl.GL_CULL_FACE);    // 겹치는 뒷면을 그리지 않음
                        //Gl.glFrontFace(Gl.GL_CCW);       // 시계 방향의 와인딩 적용
                        Gl.glEnable(Gl.GL_DEPTH_TEST);	//깊이 버퍼 활성화 - 숨겨진 면 지우기
                        //float [] fcolor = new float[4] {0.1f, 0.1f, 0.1f, 0.1f};
                        //Gl.glEnable(Gl.GL_FOG);

                        //Gl.glFog(Gl.GL_FOG, fcolor);
#else
                Gl.glEnable(GL.GL_DEPTH_TEST);
                Gl.glEnable(GL.GL_CULL_FACE);
                Gl.glFrontFace(GL.GL_CW);

                float[] ambLight = new float[4] { 0.5f, 0.5f, 0.5f, 0.5f };
                float[] specular = new float[4] { 1.0f, 1.0f, 1.0f, 1.0f };
                float[] specref = new float[4] { 1.0f, 1.0f, 1.0f, 1.0f }; 
                float [] lightPos = new float[4] {0.0f, 100.0f, 0.0f, 1.0f}; //조명의 위치를 설정한다. 

                Gl.glEnable(GL.GL_LIGHTING); //조명을 사용하도록 한다. 
                Gl.glLightModelfv(GL.GL_LIGHT_MODEL_AMBIENT, ambLight); //저장된 값으로 주변광을 설정한다. 
                Gl.glLightfv(GL.GL_LIGHT0, Gl.GL_DIFFUSE, ambLight);
                Gl.glLightfv(GL.GL_LIGHT0, Gl.GL_SPECULAR, specular);
                Gl.glLightfv(GL.GL_LIGHT0, Gl.GL_POSITION, lightPos); //조명 위치 설정 
                Gl.glEnable(GL.GL_COLOR_MATERIAL); //재질에 영향을 받도록 한다. 

                Gl.glColorMaterial(GL.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE); //재질 색상을 오브젝트와 텍스쳐 섹을 함께 사용한다. 

                Gl.glMaterialfv(GL.GL_FRONT, Gl.GL_SPECULAR, specref);
                Gl.glMateriali(GL.GL_FRONT, Gl.GL_SHININESS, 70);

                Gl.glEnable(GL.GL_LIGHT0); //0번 조명 사용 
#if _GL_FLAT
                    Gl.glShadeModel(Gl.GL_FLAT);							    // Enable Flat Shading
#else
                    Gl.glShadeModel(Gl.GL_SMOOTH);							    // Enable Smooth Shading                    
#endif


#endif
#endif
                        //SetLight2();
                    }
                    else
                    {
                        Gl.glEnable(Gl.GL_MULTISAMPLE);                        
                        Gl.glDisable(Gl.GL_LIGHTING);
                        Gl.glDisable(Gl.GL_LIGHT0);

                        // Enable color tracking(Kor: 색상 트래킹을 사용하게끔 설정)
                        Gl.glEnable(Gl.GL_COLOR_MATERIAL);
                    }
                }
                #endregion Light

                #endregion There are basic functions for initialization.(Kor: OpenGL 을 처음 실행시 초기화 하는 함수등 OpenGL 기본적 구현 함수)

                #region OpenGL: Rotate, move, reset, etc. The basic control functions(Kor: OpenGL 회전, 이동, 초기화등 기본제어함수)
                private float[] m_afCalcAngle = new float[3] { 0.0f, 0.0f, 0.0f };
                private float[] m_afCalcPos = new float[3] { 0.0f, 0.0f, 0.0f };
                //public float[] m_afCalcPos_Event = new float[16];// { 0.0f, 0.0f, 0.0f };
                // Init position
                #region InitPosAngle
                private void InitPosAngle_WithAxis()
                {
                    Gl.glLoadIdentity();
                    OjwRotation(m_fPan, m_fTilt, m_fSwing);
                    OjwTranslate(m_fX, m_fY, m_fZ);
                    SetLight();
                    
                    // display axis
                    if (IsStandardAxis() == true)
                    {
                        float fSize = 2.0f;
                        float fSize2 = 5.0f;
                        Axis(true, Color.Red, 1.0f, Color.Green, 1.0f, Color.Blue, 1.0f, fSize, 40000);
                        Axis(true, Color.Red, 1.0f, Color.Green, 1.0f, Color.Blue, 1.0f, fSize2, 200);
                    }

                    OjwRotation(m_fPan_Robot, m_fTilt_Robot, m_fSwing_Robot);
                    OjwTranslate(m_fX_Robot, m_fY_Robot, m_fZ_Robot);
                }
                public void InitPosAngle()
                {
                    Gl.glLoadIdentity();
                    OjwRotation(m_fPan, m_fTilt, m_fSwing);
                    OjwTranslate(m_fX, m_fY, m_fZ);
                    SetLight();
                    OjwRotation(m_fPan_Robot, m_fTilt_Robot, m_fSwing_Robot);
                    OjwTranslate(m_fX_Robot, m_fY_Robot, m_fZ_Robot);
                }

                public void InitPosAngle(float fPan, float fTilt, float fSwing, float fX, float fY, float fZ)
                {
                    //             Gl.glColor3f(1.0f, 1.0f, 1.0f);
                    //             Gl.glRasterPos2i(20, 60);
                    //             PrintString("THE QUICK BROWN FOX JUMPS");

                    SetAngle_Display(fPan, fTilt, fSwing);
                    SetPos_Display(fX, fY, fZ);
                    Gl.glLoadIdentity();
                    SetLight(); // ojw5014

                    OjwRotation(m_fPan, m_fTilt, m_fSwing);
                    OjwTranslate(m_fX, m_fY, m_fZ);
                }
                #endregion InitPosAngle

                #region OjwTranslate
                public void OjwTranslate(float fX, float fY, float fZ)
                {
                    Gl.glTranslatef(fX, fY, fZ);	// viewport = 0 0 0 and this is 6 deep
                }
                #endregion

                #region OjwRotation
                public void OjwRotation(float fPan, float fTilt, float fSwing)
                {
                    if (fPan != 0)
                    {
                        Gl.glRotatef(fPan, 0.0f, 1.0f, 0.0f);
                        //Rotation(0.0f, fPan, 0.0f, ref m_afCalcPos[0], ref m_afCalcPos[1], ref m_afCalcPos[2]);
                    }
                    if (fTilt != 0)
                    {
                        Gl.glRotatef(fTilt, 1.0f, 0.0f, 0.0f);
                        //Rotation(fTilt, 0.0f, 0.0f, ref m_afCalcPos[0], ref m_afCalcPos[1], ref m_afCalcPos[2]);
                    }
                    if (fSwing != 0)
                    {
                        Gl.glRotatef(fSwing, 0.0f, 0.0f, 1.0f);
                        //Rotation(0.0f, 0.0f, fSwing, ref m_afCalcPos[0], ref m_afCalcPos[1], ref m_afCalcPos[2]);
                    }
                }
                #endregion

                // Simply function to calculate only the result of the rotation
                // Kor: 단순히 회전의 결과값만 내 주는(계산해 주는) 함수
                #region Rotation
                public void Rotation(float ax, float ay, float az, ref float x, ref float y, ref float z)
                {
                    //float fr = 3.14159f / 180.0f;
                    //float ax2 = ax * fr;
                    //float ay2 = ay * fr;
                    //float az2 = az * fr;

                    float ax2 = ax * 0.01745f;
                    float ay2 = ay * 0.01745f;
                    float az2 = az * 0.01745f;

                    //    →X(Left), ↑Y(Up), ●Z(Front)
                    // Rotation(Z)(Roll)
                    /*
                     cos, -sin, 0, 0
                     sin,  cos, 0, 0
                       0,    0, 1, 0
                       0,    0, 0, 1
                     */

                    // Rotation(X)(Pitch)
                    /*
                     1,   0,    0, 0
                     0, cos, -sin, 0
                     0, sin,  cos, 0
                     0,   0,    0, 1
                     */

                    // Rotation(Y)(Yaw)
                    /*
                     cos, 0, -sin, 0
                       0, 1,    0, 0
                     sin, 0,  cos, 0
                       0, 0,    0, 1
                     */

                    float z1, x2, y2;
                    float fCx = (float)Math.Cos(ax2);
                    float fCy = (float)Math.Cos(ay2);
                    float fCz = (float)Math.Cos(az2);
                    float fSx = (float)Math.Sin(ax2);
                    float fSy = (float)Math.Sin(ay2);
                    float fSz = (float)Math.Sin(az2);

                    //x1 = x * fCy + z * fSy;   // Rotation(y)
                    //y1 = y;
                    //z1 = -x * fSy + z * fCy;

                    //x2 = x1;    // Rotation(x)
                    //y2 = y1 * fCx - z1 * fSx;
                    //z2 = y1 * fSx + z1 * fCx;

                    //x = x2 * fCz - y2 * fSz;    // Rotation(z)
                    //y = x2 * fSz + y2 * fCz;
                    //z = z2;

                    x2 = x * fCy + z * fSy;   // Rotation(y)
                    z1 = -x * fSy + z * fCy;

                    y2 = y * fCx - z1 * fSx;

                    z = y * fSx + z1 * fCx;
                    x = x2 * fCz - y2 * fSz;    // Rotation(z)
                    y = x2 * fSz + y2 * fCz;
                }
                #endregion Rotation

                #endregion OpenGL: Rotate, move, reset, etc. The basic control functions(Kor: OpenGL 회전, 이동, 초기화등 기본제어함수)

                #region Class Control
                private bool m_bEmptyBody = false;
                public void SetClass_EmptyBody(bool bEmpty)
                {
                    m_bEmptyBody = bEmpty;
                }

                private bool m_bAseFake = false;
                private int m_nAseMulti = 1;
                public void SetClass_AseFake(bool bFake, int nMulti)
                {
                    m_bAseFake = bFake;
                    m_nAseMulti = ((nMulti < 1) ? 1 : nMulti);
                }

                private const int _CNT_SECOND_COLOR = 10;
                private int m_nSecondColor_Num = 0;
                private bool[] m_abSecondColor = new bool[_CNT_SECOND_COLOR];
                private Color[] m_acSecondColor = new Color[_CNT_SECOND_COLOR];
                private int[] m_anSecondColor_GroupA = new int[_CNT_SECOND_COLOR];
                private int[] m_anSecondColor_GroupB = new int[_CNT_SECOND_COLOR];
                private int[] m_anSecondColor_GroupC = new int[_CNT_SECOND_COLOR];
                // save up to 10(Kor: 10개 까지만 저장)
                public bool SetColor_Second(int nNum, int nGroupA, int nGroupB, int nGroupC, Color cColor)
                {
                    if ((nNum < 0) || (nNum >= _CNT_SECOND_COLOR)) return false;
                    m_abSecondColor[nNum] = true;
                    m_acSecondColor[nNum] = cColor;
                    m_anSecondColor_GroupA[nNum] = nGroupA;
                    m_anSecondColor_GroupB[nNum] = nGroupB;
                    m_anSecondColor_GroupC[nNum] = nGroupC;
                    return true;
                }
                public bool AddColor_Second(int nGroupA, int nGroupB, int nGroupC, Color cColor)
                {
                    if (m_nSecondColor_Num >= _CNT_SECOND_COLOR) return false;
                    return SetColor_Second(m_nSecondColor_Num++, nGroupA, nGroupB, nGroupC, cColor);
                }
                public void ClearColor_Second()
                {
                    /*m_bSecondColor = false;*/
                    //m_anSecondColor_Group.Initialize();
                    for (int i = 0; i < _CNT_SECOND_COLOR; i++) m_abSecondColor[i] = false;
                    m_nSecondColor_Num = 0;
                }

                private bool m_bPickColor = true;
                private bool m_bPickAlpha = true;
                private Color m_cPickColor = Color.FromArgb(50, 50, 255);
                private float m_fPickAlpha = 1.0f;
                public void SetPick_ColorMode(bool bOn)
                {
                    m_bPickColor = bOn;
                }
                public void SetPick_AlphaMode(bool bOn)
                {
                    m_bPickAlpha = bOn;
                }
                public void SetPick_ColorValue(Color cColor)
                {
                    m_cPickColor = cColor;
                }
                public void SetPick_AlphaValue(float fAlpha)
                {
                    m_fPickAlpha = fAlpha;
                }

                private int m_nFastMode = 0;
                private bool[] m_abMake = new bool[4096]; // 0x1000 ~ 0x1fff
                public void SetDrawFastMode(int nMode) // 0 - Normal, 1 - fastmode(make & draw)
                {
                    m_nFastMode = nMode;
                }
                public int GetDrawFastMode()
                {
                    return m_nFastMode;
                }

                //private float m_fRot_Pan = 0.0f;
                //private float m_fRot_Tilt = 0.0f;
                //private float m_fRot_Swing = 0.0f;
                //CTimer m_CTId_Pan = new CTimer();
                //CTimer m_CTId_Tilt = new CTimer();
                //CTimer m_CTId_Swing = new CTimer();
                //private 
                private float m_fPanDisplay_Rot_Limit = 360.0f;
                private float m_fTiltDisplay_Rot_Limit = 360.0f;
                private float m_fSwingDisplay_Rot_Limit = 360.0f;
                private int m_nBackupCnt_HistoryFileOpen = 0;
                private int m_nDrawClass_Pos = 0;
                public void OjwDraw_Class(COjwDisp OjwDisp)
                {                    
                    // 0x 1111 1111(Formular group)   1111 1111(group A) 1 1111 1111(group B(0~255:motor, 256~511:etc group))  111 1111(group C(0~127))
                    //uint nObjectName = (uint)OjwDisp.nPickGroup_A * 256 * 256 + (uint)OjwDisp.nPickGroup_B * 256 + (uint)OjwDisp.nPickGroup_C;
                    //uint unObjectName = (uint)(OjwDisp.nInverseKinematicsNumber & 0xff) * 256 * 256 * 256 + (uint)(OjwDisp.nPickGroup_A & 0xff) * 256 * 256 + ((uint)OjwDisp.nPickGroup_B & 0x1ff) * 128 + ((uint)OjwDisp.nPickGroup_C & 0x7f);
                    #region Kor
                    // 0x 1111 1111(수식그룹)   1111 1111(그룹 A) 1 1111 1111(그룹 B(0~255:모터, 256~511:기타그룹))  111 1111(그룹 C(0~127))
                    //uint nObjectName = (uint)OjwDisp.nPickGroup_A * 256 * 256 + (uint)OjwDisp.nPickGroup_B * 256 + (uint)OjwDisp.nPickGroup_C;
                    //uint unObjectName = (uint)(OjwDisp.nInverseKinematicsNumber & 0xff) * 256 * 256 * 256 + (uint)(OjwDisp.nPickGroup_A & 0xff) * 256 * 256 + ((uint)OjwDisp.nPickGroup_B & 0x1ff) * 128 + ((uint)OjwDisp.nPickGroup_C & 0x7f);
                    #endregion Kor
                    uint unObjectName = (uint)((OjwDisp.nInverseKinematicsNumber & 0xff) << 24) + ((uint)(OjwDisp.nPickGroup_A & 0xff) << 16) + (((uint)OjwDisp.nPickGroup_B & 0x1ff) << 7) + ((uint)OjwDisp.nPickGroup_C & 0x7f);
                    if ((m_bPickMode == true) && (unObjectName > 0)) { PushName(unObjectName); }

                    int nGroupA, nGroupB, nGroupC, nInverseKinematics;
                    Color cColor = OjwDisp.cColor;
                    GetPickingData(out nGroupA, out nGroupB, out nGroupC, out nInverseKinematics);
                    float fAlpha = (m_bAlpha) ? m_fAlpha : OjwDisp.fAlpha;
                    bool bPicked = false;
                    if (m_bPickColor == true)
                    {
                        if(
                            (((nGroupA + nGroupB) > 0) &&
                        (OjwDisp.nPickGroup_A == nGroupA) &&
                        (OjwDisp.nPickGroup_B == nGroupB)
                        ) 
                            ||
                            ((SelectObject_Check(m_nDrawClass_Pos) == true) && (SelectObject_Enable() == true))
                        )
                        {
                            if (m_bPickColor) cColor = m_cPickColor;// Color.FromArgb(50, 50, 255);// Color.FromArgb(100, 0, 0);
                            if (m_bPickAlpha) fAlpha = m_fPickAlpha;
                            bPicked = true;
                        }                        
                    }

                    for (int i = 0; i < _CNT_SECOND_COLOR; i++)
                    {
                        if (m_abSecondColor[i] == true)
                        {
                            int nGA = m_anSecondColor_GroupA[i];
                            int nGB = m_anSecondColor_GroupB[i];
                            if (((nGA + nGB) > 0) &&
                            (nGA == OjwDisp.nPickGroup_A) &&
                            (nGB == OjwDisp.nPickGroup_B)
                            )
                            {
                                cColor = m_acSecondColor[i];
                                break;
                            }
                        }
                    }

                    if (OjwDisp.bInit == true)
                    {
                        InitPosAngle();
                        m_afCalcPos[0] = 0;
                        m_afCalcPos[1] = 0;
                        m_afCalcPos[2] = 0;
                        m_afCalcAngle[0] = 0;
                        m_afCalcAngle[1] = 0;
                        m_afCalcAngle[2] = 0;
                    }

                    //Gl.glEnable(Gl.GL_BLEND);
                    //Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                    //Gl.glEnable(Gl.GL_ALPHA_TEST);

                    //SetLight2();

                    for (int j = 0; j < 3; j++)
                    {
                        OjwTranslate(OjwDisp.afTrans[j].x, OjwDisp.afTrans[j].y, OjwDisp.afTrans[j].z);
                        OjwRotation(OjwDisp.afRot[j].pan, OjwDisp.afRot[j].tilt, OjwDisp.afRot[j].swing);

                        m_afCalcAngle[0] += OjwDisp.afRot[j].pan;
                        m_afCalcAngle[1] += OjwDisp.afRot[j].tilt;
                        m_afCalcAngle[2] += OjwDisp.afRot[j].swing;
                        Rotation(m_afCalcAngle[1], m_afCalcAngle[0], m_afCalcAngle[2], ref m_afCalcPos[0], ref m_afCalcPos[1], ref m_afCalcPos[2]);
                        float fCalcX = OjwDisp.afTrans[j].x;
                        float fCalcY = OjwDisp.afTrans[j].y;
                        float fCalcZ = OjwDisp.afTrans[j].z;
                        Rotation(m_afCalcAngle[1], m_afCalcAngle[0], m_afCalcAngle[2], ref fCalcX, ref fCalcY, ref fCalcZ);
                        m_afCalcPos[0] += fCalcX;
                        m_afCalcPos[1] += fCalcY;
                        m_afCalcPos[2] += fCalcZ;
                    }

                    //             if (bPicking == true)
                    //             {
                    //                 m_afCalcPos_Event[0] = m_afCalcPos[0];
                    //                 m_afCalcPos_Event[1] = m_afCalcPos[1];
                    //                 m_afCalcPos_Event[2] = m_afCalcPos[2];
                    //             }

                    bool bFilled = ((m_bEmptyBody == true) ? false : OjwDisp.bFilled);

                    //
                    SAngle3D_t SAngle = new SAngle3D_t();
                    SAngle3D_t SAngle_Offset = new SAngle3D_t();
                    SVector3D_t SVector = new SVector3D_t();
                    SAngle3D_t SRot = new SAngle3D_t();
                    SAngle.pan = ((OjwDisp.nAxisMoveType == 0) ? OjwDisp.fAngle : 0) * (int)Math.Pow(-1, OjwDisp.nDir);
                    SAngle.tilt = ((OjwDisp.nAxisMoveType == 1) ? OjwDisp.fAngle : 0) * (int)Math.Pow(-1, OjwDisp.nDir);
                    SAngle.swing = ((OjwDisp.nAxisMoveType == 2) ? OjwDisp.fAngle : 0) * (int)Math.Pow(-1, OjwDisp.nDir);
                    SVector.x = ((OjwDisp.nAxisMoveType == 3) ? OjwDisp.fAngle : 0) * (int)Math.Pow(-1, OjwDisp.nDir);
                    SVector.y = ((OjwDisp.nAxisMoveType == 4) ? OjwDisp.fAngle : 0) * (int)Math.Pow(-1, OjwDisp.nDir);
                    SVector.z = ((OjwDisp.nAxisMoveType == 5) ? OjwDisp.fAngle : 0) * (int)Math.Pow(-1, OjwDisp.nDir);
                    SRot.pan = ((OjwDisp.nAxisMoveType == 6) ? OjwDisp.fAngle : 0) * (int)Math.Pow(-1, OjwDisp.nDir);
                    SRot.tilt = ((OjwDisp.nAxisMoveType == 7) ? OjwDisp.fAngle : 0) * (int)Math.Pow(-1, OjwDisp.nDir);
                    SRot.swing = ((OjwDisp.nAxisMoveType == 8) ? OjwDisp.fAngle : 0) * (int)Math.Pow(-1, OjwDisp.nDir);

                    if (OjwDisp.nAxisMoveType < 3)
                    {
                        // val2 = val % 60000
                        // result = val2 * 360 / 60000      (rpm)
                        SAngle_Offset.pan = ((OjwDisp.nAxisMoveType == 0) ? OjwDisp.fAngle_Offset : 0) * (int)Math.Pow(-1, OjwDisp.nDir);
                        SAngle_Offset.tilt = ((OjwDisp.nAxisMoveType == 1) ? OjwDisp.fAngle_Offset : 0) * (int)Math.Pow(-1, OjwDisp.nDir);
                        SAngle_Offset.swing = ((OjwDisp.nAxisMoveType == 2) ? OjwDisp.fAngle_Offset : 0) * (int)Math.Pow(-1, OjwDisp.nDir);

                        OjwRotation(SAngle.pan + SAngle_Offset.pan, SAngle.tilt + SAngle_Offset.tilt, SAngle.swing + SAngle_Offset.swing);

                        if (bPicked == true)
                        {
                            //float fThick = 10.0f;
                            //float fLength = 50.0f;
                            //Axis_X(true, Color.Red, 1.0f, fThick, fLength);
                            //Axis_Y(true, Color.Green, 1.0f, fThick, fLength);
                            //Axis_Z(true, Color.Blue, 1.0f, fThick, fLength);

                        }
                    }
                    else if ((OjwDisp.nAxisMoveType >= 3) && (OjwDisp.nAxisMoveType < 6))
                    {
                        OjwTranslate(SVector.x, SVector.y, SVector.z);

                        if (bPicked == true)
                        {
                            float fThick = 10.0f;
                            float fLength = 50.0f;
                            Axis_X(true, Color.Red, 1.0f, fThick, fLength);
                            Axis_Y(true, Color.Green, 1.0f, fThick, fLength);
                            Axis_Z(true, Color.Blue, 1.0f, fThick, fLength);
                        }
                    }
                    else // CW & CCW
                    {
                        if (m_fPanDisplay_Rot_Limit != 0.0f) SRot.pan %= m_fPanDisplay_Rot_Limit;
                        if (m_fTiltDisplay_Rot_Limit != 0.0f) SRot.tilt %= m_fTiltDisplay_Rot_Limit;
                        if (m_fSwingDisplay_Rot_Limit != 0.0f) SRot.swing %= m_fSwingDisplay_Rot_Limit;

                        SRot.pan = (float)(CTimer.GetCurrentTime() % 60000) * SRot.pan * 360.0f / 60000.0f;// (float)(((double)(SAngle.pan / 360.0) * (double)CTimer.GetCurrentTime() / 60000.0) % 360.0);
                        SRot.tilt = (float)(CTimer.GetCurrentTime() % 60000) * SRot.tilt * 360.0f / 60000.0f;// (float)(((double)(SAngle.tilt / 360.0) * (double)CTimer.GetCurrentTime() / 60000.0) % 360.0);
                        SRot.swing = (float)(CTimer.GetCurrentTime() % 60000) * SRot.swing * 360.0f / 60000.0f;// (float)(((double)(SAngle.swing / 360.0) * (double)CTimer.GetCurrentTime() / 60000.0) % 360.0);

                        OjwRotation(SRot.pan + SAngle_Offset.pan, SRot.tilt + SAngle_Offset.tilt, SRot.swing + SAngle_Offset.swing);
                    }

                    if      (OjwDisp.strDispObject == "#0") 
                        OjwBox(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, OjwDisp.fDepth_Or_Cnt);
                    else if (OjwDisp.strDispObject == "#1")
                        OjwCircle(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, (int)Math.Round(OjwDisp.fDepth_Or_Cnt));
                    else if (OjwDisp.strDispObject == "#2")
                        OjwBall(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, (int)Math.Round(OjwDisp.fDepth_Or_Cnt));
                    else if (OjwDisp.strDispObject == "#3")
                        OjwCase(bFilled, cColor, fAlpha, true, false, OjwDisp.fGap, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, OjwDisp.fDepth_Or_Cnt, OjwDisp.fThickness, OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    else if (OjwDisp.strDispObject == "#4")
                        OjwCase(bFilled, cColor, fAlpha, true, true, OjwDisp.fGap, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, OjwDisp.fDepth_Or_Cnt, OjwDisp.fThickness, OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    else if (OjwDisp.strDispObject == "#5")
                        OjwCase_half(bFilled, cColor, fAlpha, true, false, OjwDisp.fGap, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, OjwDisp.fDepth_Or_Cnt, OjwDisp.fThickness, OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    else if (OjwDisp.strDispObject == "#6")
                        OjwCase_half(bFilled, cColor, fAlpha, true, true, OjwDisp.fGap, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, OjwDisp.fDepth_Or_Cnt, OjwDisp.fThickness, OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    else if (OjwDisp.strDispObject == "#7")
                        OjwBox_Outside(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, OjwDisp.fDepth_Or_Cnt, OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    else if (OjwDisp.strDispObject == "#8")
                        OjwCircle_Outside(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, (int)Math.Round(OjwDisp.fDepth_Or_Cnt), OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    else if (OjwDisp.strDispObject == "#9")
                        OjwBall_Outside(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, (int)Math.Round(OjwDisp.fDepth_Or_Cnt), OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    else if (OjwDisp.strDispObject == "#10")
                        OjwCone_Outside(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, (int)Math.Round(OjwDisp.fDepth_Or_Cnt), OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    else if (OjwDisp.strDispObject == "#11")
                        Axis_X(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    else if (OjwDisp.strDispObject == "#12")
                        Axis_Y(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    else if (OjwDisp.strDispObject == "#13")
                        Axis_Z(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    else if (OjwDisp.strDispObject == "#14")
                        Axis(bFilled, cColor, fAlpha, cColor, fAlpha, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z);
                    // Added

                    else if (OjwDisp.strDispObject == "#15")
                    {                        
                        OjwPoint(cColor, fAlpha,
                            OjwDisp.Points[0].x, OjwDisp.Points[0].y, OjwDisp.Points[0].z,
                            OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing,
                            OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z
                                    );
                    }
                    else if (OjwDisp.strDispObject == "#16")
                    {
                        OjwPoints(cColor, fAlpha,
                                OjwDisp.Points,
                                OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing,
                                OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z
                                        );
                    }
                    else if (OjwDisp.strDispObject == "#17")
                    {
                        if (OjwDisp.Points.Count >= 2)
                        {
                            OjwLine(cColor, fAlpha,
                                OjwDisp.Points[0].x, OjwDisp.Points[0].y, OjwDisp.Points[0].z,
                                OjwDisp.Points[1].x, OjwDisp.Points[1].y, OjwDisp.Points[1].z,
                                OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing,
                                OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z
                                        );
                        }
                    }
                    else if (OjwDisp.strDispObject == "#18")
                    {
                        // For Test
                        //OjwDisp.Points.Clear();
                        //OjwDisp.Points.Add(new SVector3D_t(10, 10, 10));
                        //OjwDisp.Points.Add(new SVector3D_t(10, 10+30, 10));
                        //OjwDisp.Points.Add(new SVector3D_t(10, 10+30, 10+30));
                        ////OjwDisp.Points.Add(new SVector3D_t(10, 10, 10));
                        /////////////////////////////////////////////

                        OjwLines(bFilled, cColor, fAlpha,
                                OjwDisp.Points,
                                OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing,
                                OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z
                                        );
                    }
                    else
                    {
                        //if (GetDrawFastMode() == 0)
                        //{
                        OjwAse_Outside(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, (int)Math.Round(OjwDisp.fDepth_Or_Cnt), OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z, OjwDisp.strDispObject);
                        //}
                    }
                    //else if ((OjwDisp.nDispModel >= 0x1000) && (OjwDisp.nDispModel < 0x2000))
                    //{
                    //    if (GetDrawFastMode() == 0)
                    //    {
                    //        //OjwAse_Outside(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, (int)Math.Round(OjwDisp.fDepth_Or_Cnt), OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z, (int)(OjwDisp.nDispModel & 0xfff));
                    //        OjwAse_Outside(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, (int)Math.Round(OjwDisp.fDepth_Or_Cnt), OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z, OjwDisp.strDispObject);
                    //    }
                    //    else
                    //    {
                    //        if (m_abMake[OjwDisp.nDispModel - 0x1000] == false)
                    //        {
                    //            OjwAse_Outside_make(OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, (int)Math.Round(OjwDisp.fDepth_Or_Cnt), OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z, (int)(OjwDisp.nDispModel & 0xfff));
                    //            m_abMake[OjwDisp.nDispModel - 0x1000] = true;
                    //        }
                    //        OjwAse_Outside2(bFilled, cColor, fAlpha, OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z, (int)(OjwDisp.nDispModel & 0xfff));
                    //    }

                    //}
                    //else
                    //    OjwBox(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, OjwDisp.fDepth_Or_Cnt);

                    OjwRotation(-SAngle_Offset.pan, -SAngle_Offset.tilt, -SAngle_Offset.swing);

                    //Rotation(-SAngle_Offset.tilt, -SAngle_Offset.pan, -SAngle_Offset.swing, ref m_afCalcPos[0], ref m_afCalcPos[1], ref m_afCalcPos[2]);


                    //if (bPicking == true)
                    //{
                    //m_afCalcPos_Event[0] = m_afCalcPos[0];
                    //m_afCalcPos_Event[1] = m_afCalcPos[1];
                    //m_afCalcPos_Event[2] = m_afCalcPos[2];

                    //Gl.glLoadMatrixf(m_afCalcPos_Event);
                    //Gl.glLoadMatrixf(m_afCalcPos_Event);

                    //bPicking = false;
                    //}

                    if ((m_bPickMode == true) && (unObjectName > 0))
                    {
                        PopName();
                        //Gl.glPopMatrix();
                    }
                }
                //unsafe public float* pM = malloc(sizeof(float) * 16);
                // nDispModel(0-Box, 1-Circle), nInit(0 - none, 1 - Init), nEmpty(1 - Empty, 0 - Fill), nTexture(-1 - None, 0~ - Texture Number),  
                public bool StringLine_To_Class(String strData, out COjwDisp CDisp)
                {
                    float[,] afTrans2 = new float[5, 3];
                    float[,] afRot2 = new float[5, 3];
                    System.Array.Clear(afTrans2, 0, afTrans2.Length);
                    System.Array.Clear(afRot2, 0, afRot2.Length);
                    CDisp = new COjwDisp();
                    System.Array.Clear(CDisp.afTrans, 0, CDisp.afTrans.Length);
                    System.Array.Clear(CDisp.afRot, 0, CDisp.afRot.Length);

                    CDisp.cColor = Color.White;
                    CDisp.fAlpha = 1.0f;
                    CDisp.strDispObject = "#-1";
                    CDisp.bInit = false;
                    CDisp.bFilled = false;
                    CDisp.nTexture = 0;

                    CDisp.SOffset_Rot.pan = CDisp.SOffset_Rot.tilt = CDisp.SOffset_Rot.swing = 0.0f;
                    CDisp.SOffset_Trans.x = CDisp.SOffset_Trans.y = CDisp.SOffset_Trans.z = 0.0f;

                    CDisp.fWidth_Or_Radius = CDisp.fHeight_Or_Depth = CDisp.fDepth_Or_Cnt = CDisp.fThickness = CDisp.fGap = 0;

                    //CDisp.nAxis = CDisp.nName = CDisp.nDispModel = -1;
                    CDisp.nAxisMoveType = CDisp.nName = -1;
                    CDisp.nDir = 0;
                    CDisp.fAngle = CDisp.fAngle_Offset = 0.0f;
                    CDisp.strCaption = "";

                    // Picking
                    CDisp.nPickGroup_A = 0;
                    CDisp.nPickGroup_B = 0;
                    CDisp.nPickGroup_C = 0;
                    CDisp.nInverseKinematicsNumber = 0;
                    CDisp.fScale_Serve0 = 0.35f;
                    CDisp.fScale_Serve1 = 0.35f;
                    CDisp.nInverseKinematicsNumber = 0;
                    CDisp.strPickGroup_Comment = "";

                    try
                    {
                        bool bRet = false;
                        String[] pstrData = strData.Split(',');
                        if (pstrData.Length >= 54)
                        {
                            /*
                                    16 + 3 + 3 + 3 * 5 + 3 * 5 = 52
                                    int nName, Color cColorData, float fAlpha(new one), int nModel, bool bFill, int nTextureData, bool bInitialize, float fW, float fH, float fD, float fT, float fGapData, string strMessage,
                                    int nAxisData, int nDirData, float fAngleData, float fAngle_OffsetData
                                    SVector3D_t fOffset_Translation, SAngle3D_t fOffset_Rotation, 
                                    SVector3D_t[] afTranslation, SAngle3D_t[] afRotation, 
                                    nObjectPickGroup_A, nObjectPickGroup_B, nObjectPickGroup_C, strObjectPickGroup_Comment
                            */

                            int i = 0, j = 0, k = 0;
                            bool bName = false;
                            bool bColor = false;
                            bool bAlpha = false;
                            bool bDispObject = false;
                            bool bEmpty = false;
                            bool bTexture = false;
                            bool bInit = false;

                            bool bW = false;
                            bool bH = false;
                            bool bD = false;
                            bool bT = false;
                            bool bGap = false;

                            bool bAxis = false;
                            bool bDir = false;
                            bool bAngle = false;
                            bool bAngle_Offset = false;

                            // Offset
                            bool bOffsetX = false;
                            bool bOffsetY = false;
                            bool bOffsetZ = false;
                            bool bOffsetPan = false;
                            bool bOffsetTilt = false;
                            bool bOffsetSwing = false;

                            // Picking
                            bool bObjectPickGroup_A = false;
                            bool bObjectPickGroup_B = false;
                            bool bObjectPickGroup_C = false;
                            bool bObjectPickGroup_InverseKinematics = false;
                            bool bObjectScale_Serve0 = false;
                            bool bObjectScale_Serve1 = false;
                            bool bMotorType = false;
                            bool bMotorControl_MousePoint = false;
                            bool bObjectPickGroup_Comment = false;
                            int nReserveData = 0;
                            foreach (string strItem in pstrData)
                            {
                                if (bName == false)
                                {
                                    bName = true;
                                    CDisp.nName = CConvert.StrToInt(strItem);
                                }
                                else if (bColor == false)
                                {
                                    bColor = true;
                                    CDisp.cColor = Color.FromArgb(CConvert.StrToInt(strItem));
                                }
                                else if (bAlpha == false)
                                {
                                    bAlpha = true;
                                    CDisp.fAlpha = CConvert.StrToFloat(strItem);
                                }
                                else if (bDispObject == false)
                                {
                                    bDispObject = true;
                                    //if (Ojw.CConvert.IsDigit(strItem) == false) CDisp.nDispModel = 0x1000;
                                    //else CDisp.nDispModel = CConvert.StrToInt(strItem);
#if false
                                    if (CDisp.nDispModel < 0x1000)
                                    {
                                        // Normal command
                                        CDisp.strDispObject = String.Empty;
                                    }
                                    else
                                    {
                                        // ASE Data
                                        CDisp.strDispObject = strItem;
                                    }
#else
#if _DHF_FILE
                                    //if (m_CHeader.strVersion != null)
                                    //{
                                    //    if (m_CHeader.strVersion.ToUpper().IndexOf("DHF") >= 0)
                                    //    {
                                    //        int nData = CConvert.StrToInt(strItem);
                                    //        if (nData >= 0x1000) CDisp.strDispObject = CConvert.IntToStr(nData - 0x1000);
                                    //        else CDisp.strDispObject = "#" + strItem;
                                    //    }
                                    //    else CDisp.strDispObject = strItem;
                                    //}
                                    //else 
                                        CDisp.strDispObject = strItem;
#else
                                    CDisp.strDispObject = strItem;
#endif

#endif
                                    ////if ((CDisp.nDispModel >= 0x1000) && (CDisp.nDispModel < 0x2000)) CDisp.strDispObject = CConvert.IntToStr(CConvert.StrToInt(strItem) - 0x1000);
                                    ////else CDisp.strDispObject = strItem;
                                    //if (m_CHeader.strVersion != null)
                                    //{
                                    //    if (m_CHeader.strVersion.ToUpper().IndexOf("DHF") >= 0)
                                    //    {
                                    //        if ((CDisp.nDispModel >= 0x1000) && (CDisp.nDispModel < 0x2000))
                                    //        {
                                    //            CDisp.strDispObject = CConvert.IntToStr(CConvert.StrToInt(strItem) - 0x1000);
                                    //            CDisp.nDispModel = 0x1000;
                                    //        }
                                    //        else
                                    //        {

                                    //        }
                                    //    }
                                    //    else
                                    //    {
                                    //        m_CHeader.strVersion = _STR_EXT.ToUpper() + C3d._STR_EXT_VERSION;
                                    //        CDisp.strDispObject = strItem;
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    m_CHeader.strVersion = _STR_EXT.ToUpper() + C3d._STR_EXT_VERSION;
                                    //    CDisp.strDispObject = strItem;
                                    //}
                                }
                                else if (bEmpty == false)
                                {
                                    bEmpty = true;
                                    CDisp.bFilled = CConvert.StrToBool(strItem);
                                }
                                else if (bTexture == false)
                                {
                                    bTexture = true;
                                    CDisp.nTexture = CConvert.StrToInt(strItem);
                                }
                                else if (bInit == false)
                                {
                                    bInit = true;
                                    CDisp.bInit = CConvert.StrToBool(strItem);
                                }

                                else if (bW == false)
                                {
                                    bW = true;
                                    CDisp.fWidth_Or_Radius = CConvert.StrToFloat(strItem);
                                }
                                else if (bH == false)
                                {
                                    bH = true;
                                    CDisp.fHeight_Or_Depth = CConvert.StrToFloat(strItem);
                                }
                                else if (bD == false)
                                {
                                    bD = true;
                                    CDisp.fDepth_Or_Cnt = CConvert.StrToFloat(strItem);
                                }
                                else if (bT == false)
                                {
                                    bT = true;
                                    CDisp.fThickness = CConvert.StrToFloat(strItem);
                                }
                                else if (bGap == false)
                                {
                                    bGap = true;
                                    CDisp.fGap = CConvert.StrToFloat(strItem);
                                }

                                else if (bAxis == false)
                                {
                                    bAxis = true;
                                    CDisp.nAxisMoveType = CConvert.StrToInt(strItem);
                                }
                                else if (bDir == false)
                                {
                                    bDir = true;
                                    CDisp.nDir = CConvert.StrToInt(strItem);
                                }
                                else if (bAngle == false)
                                {
                                    bAngle = true;
                                    CDisp.fAngle = CConvert.StrToFloat(strItem);
                                }
                                else if (bAngle_Offset == false)
                                {
                                    bAngle_Offset = true;
                                    CDisp.fAngle_Offset = CConvert.StrToFloat(strItem);
                                }

                                // Offset
                                else if (bOffsetX == false)
                                {
                                    bOffsetX = true;
                                    CDisp.SOffset_Trans.x = CConvert.StrToFloat(strItem);
                                }
                                else if (bOffsetY == false)
                                {
                                    bOffsetY = true;
                                    CDisp.SOffset_Trans.y = CConvert.StrToFloat(strItem);
                                }
                                else if (bOffsetZ == false)
                                {
                                    bOffsetZ = true;
                                    CDisp.SOffset_Trans.z = CConvert.StrToFloat(strItem);
                                }
                                else if (bOffsetPan == false)
                                {
                                    bOffsetPan = true;
                                    CDisp.SOffset_Rot.pan = CConvert.StrToFloat(strItem);
                                }
                                else if (bOffsetTilt == false)
                                {
                                    bOffsetTilt = true;
                                    CDisp.SOffset_Rot.tilt = CConvert.StrToFloat(strItem);
                                }
                                else if (bOffsetSwing == false)
                                {
                                    bOffsetSwing = true;
                                    CDisp.SOffset_Rot.swing = CConvert.StrToFloat(strItem);
                                }

                                else if (nReserveData < (5 * 2 * 3))
                                {
                                    nReserveData++;
                                    if (k == 0)
                                    {
                                        // T3, R3,    T3, R3,  ...... , T3, R3
                                        afTrans2[i, j] = (float)CConvert.StrToFloat(strItem);
                                        if (j >= 2) { k = 1; j = 0; }
                                        else j++;
                                    }
                                    else
                                    {
                                        afRot2[i, j] = (float)CConvert.StrToFloat(strItem);
                                        if (j >= 2) { k = 0; i++; j = 0; }
                                        else j++;
                                    }
                                }

                                else if (bObjectPickGroup_A == false)
                                {
                                    bObjectPickGroup_A = true;
                                    CDisp.nPickGroup_A = CConvert.StrToInt(strItem);
                                }
                                else if (bObjectPickGroup_B == false)
                                {
                                    bObjectPickGroup_B = true;
                                    CDisp.nPickGroup_B = CConvert.StrToInt(strItem);
                                }
                                else if (bObjectPickGroup_C == false)
                                {
                                    bObjectPickGroup_C = true;
                                    CDisp.nPickGroup_C = CConvert.StrToInt(strItem);
                                }
                                else if (bObjectPickGroup_InverseKinematics == false)
                                {
                                    bObjectPickGroup_InverseKinematics = true;
                                    CDisp.nInverseKinematicsNumber = CConvert.StrToInt(strItem);
                                }
                                else if (bObjectScale_Serve0 == false)
                                {
                                    bObjectScale_Serve0 = true;
                                    CDisp.fScale_Serve0 = CConvert.StrToFloat(strItem);
                                }
                                else if (bObjectScale_Serve1 == false)
                                {
                                    bObjectScale_Serve1 = true;
                                    CDisp.fScale_Serve1 = CConvert.StrToFloat(strItem);
                                }
                                else if (bMotorType == false)
                                {
                                    bMotorType = true;
                                    CDisp.nMotorType = CConvert.StrToInt(strItem);
                                }
                                else if (bMotorControl_MousePoint == false)
                                {
                                    bMotorControl_MousePoint = true;
                                    CDisp.nMotorControl_MousePoint = CConvert.StrToInt(strItem);
                                }
                                else if (bObjectPickGroup_Comment == false)
                                {
                                    bObjectPickGroup_Comment = true;
                                    CDisp.strPickGroup_Comment = strItem;
                                }
                            }
                            bRet = true;
                        }

                        for (int i = 0; i < 5; i++)
                        {
                            CDisp.afTrans[i].x = afTrans2[i, 0];
                            CDisp.afTrans[i].y = afTrans2[i, 1];
                            CDisp.afTrans[i].z = afTrans2[i, 2];

                            CDisp.afRot[i].pan = afRot2[i, 0];
                            CDisp.afRot[i].tilt = afRot2[i, 1];
                            CDisp.afRot[i].swing = afRot2[i, 2];
                        }

                        pstrData = null;
                        afTrans2 = null;
                        afRot2 = null;

                        return bRet;
                    }
                    catch //(Exception e)
                    {
                        //                 for (int i = 0; i < 5; i++)
                        //                 {
                        //                     OjwDisp.afTrans[i].x = afTrans2[i, 0];
                        //                     OjwDisp.afTrans[i].y = afTrans2[i, 1];
                        //                     OjwDisp.afTrans[i].z = afTrans2[i, 2];
                        // 
                        //                     OjwDisp.afRot[i].pan = afRot2[i, 0];
                        //                     OjwDisp.afRot[i].tilt = afRot2[i, 1];
                        //                     OjwDisp.afRot[i].swing = afRot2[i, 2];
                        //                 }

                        afTrans2 = null;
                        afRot2 = null;

                        return false;
                    }
                }

                private const int _X = 0;
                private const int _Y = 1;
                private const int _Z = 2;
                private const int _PAN = 0;
                private const int _TILT = 1;
                private const int _SWING = 2;
                public bool TextBox_To_CodeString(TextBox txtData, out COjwDisp[] pCDisp)
                {
                    // Draw the values of the txtData.(Kor: txtData 의 값들을 그림)
                    bool bRet = false;
                    bool bRet2 = false;
                    String strData;
                    //String strTmp;
                    pCDisp = null;
                    //int nLine = 0;
                    if (txtData.Lines.Length > 0)
                    {
                        bRet = true;
                        pCDisp = new COjwDisp[txtData.Lines.Length];
                        for (int i = 0; i < txtData.Lines.Length; i++)
                        {
                            String strCaption = "";
                            strData = txtData.Lines[i];
                            int nFind = strData.IndexOf("//");
                            if (nFind >= 0)
                            {
                                strCaption = strData.Substring(nFind + 2, strData.Length - nFind - 2);

                                strData = strData.Substring(0, nFind);
                            }
                            strData = strData.Trim();
                            strData = CConvert.RemoveChar(strData, '[');
                            strData = CConvert.RemoveChar(strData, ']');

                            // Actual interpretation(Kor: 실제 해석)
                            //if (strData != "")
                            //{
                                //bRet2 = StringLine_To_Class(strData, out pCDisp[nLine]);
                                //pCDisp[nLine].strCaption = strCaption;
                                //nLine++;
                            //}

                            bRet2 = StringLine_To_Class(strData, out pCDisp[i]);
                            pCDisp[i].strCaption = strCaption;

                            if (bRet2 == false) bRet = false;
                        }
                    }
                    return bRet;
                }

                public String TextBox_To_CodeString(TextBox txtData)
                {
                    String strMessage = "";
                    // Draw the values of the txtData.(Kor: txtData 의 값들을 그림)
                    bool bRet = false;
                    String strData;
                    String strTmp;
                    COjwDisp CDisp = new COjwDisp();
                    if (txtData.Lines.Length > 0)
                    {
                        for (int i = 0; i < txtData.Lines.Length; i++)
                        {
                            String strCaption = "";
                            strData = txtData.Lines[i];
                            int nFind = strData.IndexOf("//");
                            if (nFind >= 0)
                            {
                                strCaption = strData.Substring(nFind + 2, strData.Length - nFind - 2);

                                strData = strData.Substring(0, nFind);
                                CDisp.strCaption = strCaption;
                            }
                            strData = strData.Trim();
                            strData = CConvert.RemoveChar(strData, '[');
                            strData = CConvert.RemoveChar(strData, ']');

                            // Actual interpretation(Kor: 실제 해석)
                            bRet = StringLine_To_Class(strData, out CDisp);
                            if (CDisp.nAxisMoveType >= 0)
                            {
                                string strTmpCaption = "(";
                                strTmpCaption += ((CDisp.nAxisMoveType == 0) ? "[P]," : "P,");
                                strTmpCaption += ((CDisp.nAxisMoveType == 1) ? "[T]," : "T,");
                                strTmpCaption += ((CDisp.nAxisMoveType == 2) ? "[S]" : "S");
                                //strTmpCaption += ") - 축번호 : Axis" + CConvert.IntToStr(CDisp.nName) + ((CDisp.nName < 0) ? "<동작 축 설정에 이상발견(-). 확인요망>" : "");
                                strTmpCaption += ") - Axis Number : Axis" + CConvert.IntToStr(CDisp.nName) + ((CDisp.nName < 0) ? "<Found over operation axis settings(-). Check it.>" : "");
                                if (strCaption.IndexOf(strTmpCaption, 0) < 0) strCaption += strTmpCaption;
                                if (CDisp.strCaption.IndexOf(strTmpCaption, 0) < 0) CDisp.strCaption += strTmpCaption;
                                strTmpCaption = null;
                            }

                            if (bRet == true)
                            {
                                strMessage += "// ======================================\r\n// [" + CDisp.cColor.Name.ToString() + "][" + strCaption.Trim() + "]\r\n// ======================================\r\n";

                                if (CDisp.bInit == true) strMessage += "InitPosAngle();\r\n";
                                for (int j = 0; j < 5; j++)
                                {
                                    strTmp = "";
                                    if ((CDisp.afTrans[j].x != 0) || (CDisp.afTrans[j].y != 0) || (CDisp.afTrans[j].x != 0)) strTmp = "OjwTranslate(" + CConvert.FloatToStr(CDisp.afTrans[j].x) + "f, " + CConvert.FloatToStr(CDisp.afTrans[j].y) + "f, " + CConvert.FloatToStr(CDisp.afTrans[j].z) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                    strMessage += strTmp;
                                    strTmp = "";
                                    if ((CDisp.afRot[j].pan != 0) || (CDisp.afRot[j].tilt != 0) || (CDisp.afRot[j].swing != 0)) strTmp = "OjwRotation(" + CConvert.FloatToStr(CDisp.afRot[j].pan) + "f, " + CConvert.FloatToStr(CDisp.afRot[j].tilt) + "f, " + CConvert.FloatToStr(CDisp.afRot[j].swing) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                    strMessage += strTmp;
                                }
                                if      (CDisp.strDispObject == "#0") 
                                    strMessage += "OjwBox( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#1")
                                    strMessage += "OjwCircle( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#2")
                                    strMessage += "OjwBall( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + ", [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#3")
                                    strMessage += "OjwCase( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], true, false, " + CConvert.FloatToStr(CDisp.fGap) + "f, " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + "f, " + CConvert.FloatToStr(CDisp.fThickness) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#4")
                                    strMessage += "OjwCase( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], true, true, " + CConvert.FloatToStr(CDisp.fGap) + "f, " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + "f, " + CConvert.FloatToStr(CDisp.fThickness) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#5")
                                    strMessage += "OjwCase_half( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], true, false, " + CConvert.FloatToStr(CDisp.fGap) + "f, " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + "f, " + CConvert.FloatToStr(CDisp.fThickness) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#6")
                                    strMessage += "OjwCase_half( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], true, true, " + CConvert.FloatToStr(CDisp.fGap) + "f, " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + "f, " + CConvert.FloatToStr(CDisp.fThickness) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#7")
                                    strMessage += "OjwBox_Outside( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Trans.x) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Trans.y) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Trans.z) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.pan) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.tilt) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.swing) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";

                                else if (CDisp.strDispObject == "#8")
                                    strMessage += "OjwCircle_Outside( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + ", " + CConvert.FloatToStr(CDisp.SOffset_Trans.x) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Trans.y) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Trans.z) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.pan) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.tilt) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.swing) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#9")
                                    strMessage += "OjwBall_Outside( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + ", " + CConvert.FloatToStr(CDisp.SOffset_Trans.x) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Trans.y) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Trans.z) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.pan) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.tilt) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.swing) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#10")
                                    strMessage += "OjwCone_Outside( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + ", " + CConvert.FloatToStr(CDisp.SOffset_Trans.x) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Trans.y) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Trans.z) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.pan) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.tilt) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.swing) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#11")
                                    strMessage += "Axis_X( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#12")
                                    strMessage += "Axis_Y( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#13")
                                    strMessage += "Axis_Z( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else if (CDisp.strDispObject == "#14")
                                    strMessage += "Axis( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + "]);\r\n";
                                else //if ((CDisp.nDispModel >= 0x1000) && (CDisp.nDispModel < 0x2000))
                                    strMessage += "OjwAse_Outside( " + ((CDisp.bFilled == true) ? "true" : "false") + ", " + "cColor[" + CDisp.cColor.Name.ToString() + "], " + CConvert.FloatToStr(CDisp.fWidth_Or_Radius) + "f, " + CConvert.FloatToStr(CDisp.fHeight_Or_Depth) + "f, " + CConvert.FloatToStr(CDisp.fDepth_Or_Cnt) + ", " + CConvert.FloatToStr(CDisp.SOffset_Trans.x) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Trans.y) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Trans.z) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.pan) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.tilt) + "f, " + CConvert.FloatToStr(CDisp.SOffset_Rot.swing) + "f, [" + CConvert.IntToStr(CDisp.nPickGroup_A) + ", " + CConvert.IntToStr(CDisp.nPickGroup_B) + ", " + CConvert.IntToStr(CDisp.nPickGroup_C) + ", " + ((CDisp.strPickGroup_Comment == null) ? "None" : CDisp.strPickGroup_Comment) + ", 0x1000 + " + CDisp.strDispObject + "]);\r\n";
                                //OjwAse_Outside(bFilled, cColor, fAlpha, OjwDisp.fWidth_Or_Radius, OjwDisp.fHeight_Or_Depth, (int)Math.Round(OjwDisp.fDepth_Or_Cnt), OjwDisp.SOffset_Rot.pan, OjwDisp.SOffset_Rot.tilt, OjwDisp.SOffset_Rot.swing, OjwDisp.SOffset_Trans.x, OjwDisp.SOffset_Trans.y, OjwDisp.SOffset_Trans.z, (int)(OjwDisp.nDispModel & 0xfff));
                                
                                    

                            }
                        }
                    }

                    ///////////////////////
                    //OjwMessage(txtDraw_String, strMessage);
                    CDisp = null;
                    return strMessage;

                    //             afTrans = null;
                    //             afRot = null;
                }

                public string ClassToString(COjwDisp OjwDisp)
                {
                    String strData = "";
                    // Color, T,R, T,R, T,R, T,R, T,R
                    int nRoundPoint = 3; //Color.FromName("");


                    /*
                                    int nName, Color cColorData, int nModel, bool bFill, int nTextureData, bool bInitialize, float fW, float fH, float fD, float fT, float fGapData, string strMessage,
                                    int nAxisData, int nDirData, float fAngleData, float fAngle_OffsetData
                                    SVector3D_t fOffset_Translation, SAngle3D_t fOffset_Rotation, 
                                    SVector3D_t[] afTranslation, SAngle3D_t[] afRotation, 
                                    Picking Group A(int), B(int), C(int) ,     Comment(String),
                     */


                    // ID, Model(Circle, ...), ColorR, ColorG, ColorB
                    strData =
                        "[" +
                        CConvert.IntToStr(OjwDisp.nName) +
                        "]," +
                        CConvert.IntToStr(OjwDisp.cColor.ToArgb()) + "," + 
                        // new item(alpha) added in here
                        Ojw.CConvert.FloatToStr(OjwDisp.fAlpha) + "," +
                        OjwDisp.strDispObject + "," + //CConvert.IntToStr(OjwDisp.nDispModel) + "," +
                        // bFilled, Texture, bInit
                        CConvert.BoolToStr(OjwDisp.bFilled) + "," + CConvert.IntToStr(OjwDisp.nTexture) + "," + CConvert.BoolToStr(OjwDisp.bInit) + "," +

                        "[" +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.fWidth_Or_Radius, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.fHeight_Or_Depth, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.fDepth_Or_Cnt, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.fThickness, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.fGap, nRoundPoint)) +
                        "]," +

                        "[" +
                        // Axis(None, Pan, Tilt, Swing), Dir, Angle, Angle_Offset
                        CConvert.IntToStr(OjwDisp.nAxisMoveType) + "," + CConvert.IntToStr(OjwDisp.nDir) + "," + CConvert.FloatToStr((float)Math.Round(OjwDisp.fAngle, nRoundPoint)) + "," + CConvert.FloatToStr((float)Math.Round(OjwDisp.fAngle_Offset, nRoundPoint)) +
                        "]," +

                        // Offset(X,Y,Z,  P,T,S)
                        "[" +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.SOffset_Trans.x, nRoundPoint)) + "," + CConvert.FloatToStr((float)Math.Round(OjwDisp.SOffset_Trans.y, nRoundPoint)) + "," + CConvert.FloatToStr((float)Math.Round(OjwDisp.SOffset_Trans.z, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.SOffset_Rot.pan, nRoundPoint)) + "," + CConvert.FloatToStr((float)Math.Round(OjwDisp.SOffset_Rot.tilt, nRoundPoint)) + "," + CConvert.FloatToStr((float)Math.Round(OjwDisp.SOffset_Rot.swing, nRoundPoint)) +
                        "]," +

                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[0].x, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[0].y, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[0].z, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[0].pan, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[0].tilt, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[0].swing, nRoundPoint)) + "," +

                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[1].x, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[1].y, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[1].z, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[1].pan, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[1].tilt, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[1].swing, nRoundPoint)) + "," +

                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[2].x, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[2].y, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[2].z, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[2].pan, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[2].tilt, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[2].swing, nRoundPoint)) + "," +

                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[3].x, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[3].y, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[3].z, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[3].pan, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[3].tilt, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[3].swing, nRoundPoint)) + "," +

                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[4].x, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[4].y, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afTrans[4].z, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[4].pan, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[4].tilt, nRoundPoint)) + "," +
                        CConvert.FloatToStr((float)Math.Round(OjwDisp.afRot[4].swing, nRoundPoint)) + "],[" +
                        CConvert.IntToStr(OjwDisp.nPickGroup_A) + "," +
                        CConvert.IntToStr(OjwDisp.nPickGroup_B) + "," +
                        CConvert.IntToStr(OjwDisp.nPickGroup_C) + ",[" +
                        CConvert.IntToStr(OjwDisp.nInverseKinematicsNumber) + "],[" +
                        CConvert.FloatToStr(OjwDisp.fScale_Serve0) + "," +
                        CConvert.FloatToStr(OjwDisp.fScale_Serve1) + "],[" +
                        CConvert.IntToStr(OjwDisp.nMotorType) + "," +
                        CConvert.IntToStr(OjwDisp.nMotorControl_MousePoint) + "]," +
                        ((OjwDisp.strPickGroup_Comment == null) ? "None" : OjwDisp.strPickGroup_Comment) +
                        "]";
                    return strData;
                }
                #endregion Class Control
            
                #region Picking
                private bool m_bPickMode = false;
                private const int _PICK_BUFFER_SIZE = 64;
                private uint[] m_puiPick = new uint[_PICK_BUFFER_SIZE];
                public void Picking_Ready(int nX, int nY)
                {
                    m_bPickMode = true;

                    m_unObjectName = 0; // Init variables for picking group(Kor: 픽킹 그룹핑 변수 초기화)

                    int[] viewport = new int[4];

                    // get the view port(Kor: 뷰포트 얻기)
                    Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

                    // select buffer(Kor: 선택 버퍼 설정)
                    Gl.glSelectBuffer(_PICK_BUFFER_SIZE, m_puiPick);

                    // Init Names(Kor: 이름 초기화)
                    InitNames();

                    // Switch to the projection. And storage arrays(Kor: 투영으로 전환하고 배열을 저장)
                    Gl.glMatrixMode(Gl.GL_PROJECTION);
                    Gl.glPushMatrix();

                    // Switch to the rendering mode(Kor: 렌더링 모드 변경)
                    Gl.glRenderMode(Gl.GL_SELECT);

                    // Making the size of the unit cube clipping region in which the mouse pointer [x, y], the horizontal and vertical expansion by two pixels.
                    // Kor: 마우스 포인터가 있는 x,y 에 단위 크기의 육면체 클리핑 영역을 만들고, 수평 수직 방향으로 두 픽셀씩 확장한다.
                    Gl.glLoadIdentity();
                    //Gl.gluPickMatrix(nX, viewport[3] - nY, 2, 2, viewport);
                    Glu.gluPickMatrix((double)nX, (double)(viewport[3] - nY), 1.0, 1.0, viewport);

                    // Application of projection matrices(Kor: 투영 행렬의 적용)
#if false
            int nRatio = 100;
            //Gl.gluPerspective(65.0f, aspect_ratio, 0, -nRatio);
            //Gl.glOrtho(-nRatio, nRatio, -nRatio, nRatio, -nRatio, nRatio);
            Ortho(nRatio);
            Gl.glDepthRange(-nRatio, nRatio);
#else
                    int nRatio = (int)Math.Round(_RATIO * m_fScale);
                    Ortho((float)m_nWidth, (float)m_nHeight, nRatio);
                    Gl.glDepthRange(-nRatio, nRatio);
#endif
                    // In the case of Perspective projection(Kor: 원근투영의 경우)
                    /*
                    float fAspect = (float)viewport[2] / (float)viewport[3];
                    Gl.gluPerspective(45.0f, fAspect, 1.0, 425.0);
                    */
                    //

                    Gl.glMatrixMode(Gl.GL_MODELVIEW);

                    // Should begin rendering the scene.(Kor: 이후 장면 렌더링을 시작해야 한다.)

                    // Initialize
                    viewport = null;
                }

                public uint Picking_Check()
                {
                    int nHits = Gl.glRenderMode(Gl.GL_RENDER); // get the hit counts(Kor: 히트 수 수집)
                    // If one of the hit once, outputs information(Kor: 한번의 히트일 경우, 정보를 출력)
                    if (nHits > 0)
                    {
                        // m_puiPick[0] // the number of collisions with object(Kor: Object 와의 충돌횟수)
                        // m_puiPick[1] // Min means a value of the object collided with the collision point and the set area.(Kor: 설정된 공간과 충돌한 object 와의 충돌 지점의 Min값을 의미한다.)
                        // m_puiPick[2] // Max means a value of the object collided with the collision point and the set area.(Kor: 설정된 공간과 충돌한 object 와의 충돌 지점의 Max값을 의미한다.)
                        // m_puiPick[3 ...] The total number of records [m_puiPick[0]](Kor: m_puiPick[0] 에 기록된 수 만큼...)
                        // => repeat all(Kor: 이게 반복)

                        //uint nNearID = (uint)m_puiPick[3];
                        //uint nNearDepth = (uint)m_puiPick[1];
                        int nPos = 0;
#if false
                        // 16 = 64 / 14
                        for (int i = 0; i < nHits; i++)
                        {
                            if (m_puiPick[nPos * 4 + 1] > m_puiPick[i * 4 + 1])
                            {
                                nPos = i;
                                //break;
                                //nNearDepth = m_puiPick[i * 4 + 1];
                                //nNearID = m_puiPick[i * 4 + 3];
                            }
                        }
                        m_unObjectName = (uint)m_puiPick[nPos * 4 + 3];// nNearID;// m_puiPick[3]; // 선택된 Object의 ID
                        return (uint)m_puiPick[nPos * 4 + 3];//nNearID;// m_puiPick[3];
#else
                        int nObj = 0;
                        bool bFind = false;
                        uint unObjMin = 0xffffffff;
                        //uint unObjMax = 0;
                        for (int i = 0; i < nHits; i++)
                        {
                            int nSize = (int)m_puiPick[nPos++];
                            // Min
                            uint unMin = (uint)m_puiPick[nPos];
                            nPos++;
                            // Max
                            uint unMax = (uint)m_puiPick[nPos];
                            nPos++;
#if false
                            //for (int j = 0; j < nSize; j++)
                            {
                                //if (m_puiPick[nPos++] > nObj)
                                //{
                                //    nObj = nPos - 1;
                                //}
                                if ((bFind == false) || (unObjMin >= unMin))
                                //if ((bFind == false) || (unObjMax < unMax))
                                {
                                    unObjMin = unMin;
                                    //unObjMax = unMax;
                                    bFind = true;
                                    nObj = nPos;

                                    nPos += nSize;
                                }
                            }
#else
                            for (int j = 0; j < nSize; j++)
                            {
                                if ((bFind == false) || (unObjMin >= unMin))
                                {
                                    unObjMin = unMin;
                                    bFind = true;

                                    if (m_puiPick[nPos] >= m_puiPick[nObj])
                                    {
                                        nObj = nPos;
                                    }
                                }
                                nPos++;
                            }
#endif
                        }
                        m_unObjectName = (uint)m_puiPick[nObj];// nNearID;// m_puiPick[3]; // Selected Object's ID(Kor: 선택된 Object의 ID)
                        return m_unObjectName;
#endif
                    }
                    return 0;
                }

                public void Picking_End()
                {
                    // Projection matrix recovery(Kor: 투영행렬 복구)
                    Gl.glMatrixMode(Gl.GL_PROJECTION);
                    Gl.glPopMatrix();
                    //Gl.glFlush();
                    // Switch to normal rendering mode(Kor: 보통의 렌더링 모드로 전환)
                    Gl.glMatrixMode(Gl.GL_MODELVIEW);
                    m_bPickMode = false;
                    m_bPickMouseClick = false;
                    m_bPickingData = true;
                }

                private bool m_bPickingData = false;
                public bool IsPicking()
                {
                    return m_bPickingData;
                }
                public void Clear_IsPicking()
                {
                    m_bPickingData = false;
                }

                uint m_unObjectName = 0;
                public bool GetPickingData(out int nGroupA, out int nGroupB, out int nGroupC, out int nInverseKinematicsNumber)
                {
                    // 0x 1111 1111(Formular group)   1111 1111(group A) 1 1111 1111(group B(0~255:motor, 256~511:etc group))  111 1111(group C(0~127))
                    // Kor: 0x 1111 1111(수식그룹)   1111 1111(그룹 A) 1 1111 1111(그룹 B(0~255:모터, 256~511:기타그룹))  111 1111(그룹 C(0~127))
                    nInverseKinematicsNumber = (int)((m_unObjectName >> 24) & 0xff); // High byte group number formula(Kor: 상위 바이트는 수식그룹번호)
                    nGroupA = (int)((m_unObjectName >> 16) & 0xff); // High byte group number formula(Kor: 상위 바이트는 수식그룹번호)
                    //nGroupB = (int)((m_unObjectName >> 8) & 0xff);
                    //nGroupC = (int)(m_unObjectName & 0xff);
                    nGroupB = (int)((m_unObjectName >> 7) & 0x1ff);
                    nGroupC = (int)(m_unObjectName & 0x7f);
                    return (((m_unObjectName & 0xffffff) != 0) ? true : false);
                }
                #endregion Picking

                #region SetName
                public void InitNames()
                {
                    if (m_bPickMode == true)
                    {
                        Gl.glInitNames();
                        Gl.glPushName(0);
                        //m_unName = 1;
                    }
                }
                public void SetName(uint i)
                {
                    if (m_bPickMode == true)
                        Gl.glLoadName(i);
                }
                public void PushName(uint i)
                {
                    if (m_bPickMode == true)
                        Gl.glPushName(i);
                }
                public void PopName()
                {
                    if (m_bPickMode == true)
                        Gl.glPopName();
                }
                #endregion SetName

                #region Push / Pop
                public void Push()
                {
                    Gl.glPushMatrix();
                }
                public void Pop()
                {
                    Gl.glPopMatrix();
                }
                #endregion Push / Pop

                #region Collection functions of OpenGL actually draw(Kor: OpenGL을 실제로 그리는 함수 모음)
                // Box which rotates around the center of the upper surface(Kor: 윗면의 중심을 기준으로 회전하는 상자)
                #region OjwBox_Text
                //public void OjwPolygon(int a, int b, int c, int d)
                //{
                //    float[,] vertices = new float[8,3];
                //    int i = 0;
                //    vertices[i, 0] = -1.0f;
                //    vertices[i, 1] = -1.0f;
                //    vertices[i, 2] = 1.0f;
                //    i++;

                //    Gl.glVertex3fv(vertices[a]
                //}
                public void OjwBox_Text(bool bFill, Color color,
                                    float fW, float fH, float fD            // put the size(Kor: Size 기입)
                                )
                {
                    m_fColor[0] = ((float)(color.R) / 255.0f);  // R
                    m_fColor[1] = ((float)(color.G) / 255.0f);  // G
                    m_fColor[2] = ((float)(color.B) / 255.0f);  // B
                    Gl.glColor3fv(m_fColor); // Color with an array of floats
                    float fX1 = -fW / 2.0f;
                    float fX2 = fW / 2.0f;
                    float fY1 = 0;
                    float fY2 = -fH;
                    float fZ1 = -fD / 2.0f;
                    float fZ2 = fD / 2.0f;
                    // x2 -> direction : right(Kor: 증분방향 : 오른쪽)
                    // Y2 -> direction : Up(Kor: 증분방향 : 위쪽)
                    // Z2 -> direction : The inside of the screen(Kor: 증분방향 : 화면의 안쪽)
                    // Criteria screen : See picture below(Kor: 기준화면 : 밑 그림 참조)
                    // 0, 0, 0 -> State in the middle of the screen as a starting point and went inside by a factor of 5
                    // Kor: 0, 0, 0 -> 화면의 가운데를 시작점으로 하고 안쪽으로 5만큼 들어간 상태
                    
                    /*
                        000000000
                      0 0 ★  0 0
                    000000000   0
                    0   0   0   0
                    0   000000000  
                    0 0     0 0
                    000000000  
                    ( Center position )
                    */

                    //Gl.glBindTexture(Gl.GL_TEXTURE_2D, m_puiTexture[0]);									// Select Our Texture

                    int uiType = (bFill == true) ? Gl.GL_POLYGON : Gl.GL_LINE_LOOP;//Gl.GL_LINE_LOOP;// Gl.GL_QUADS;//

                    Gl.glBegin(uiType);															// Draw A Cube Using Quads
#if false
            // Front Face
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(-1.0f, -1.0f, 1.0f);				// Bottom Left Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(1.0f, -1.0f, 1.0f);				// Bottom Right Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(1.0f, 1.0f, 1.0f);				// Top Right Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(-1.0f, 1.0f, 1.0f);				// Top Left Of The Texture and Quad
            // Back Face
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(-1.0f, -1.0f, -1.0f);				// Bottom Right Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(-1.0f, 1.0f, -1.0f);				// Top Right Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(1.0f, 1.0f, -1.0f);				// Top Left Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(1.0f, -1.0f, -1.0f);				// Bottom Left Of The Texture and Quad
            // Top Face
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(-1.0f, 1.0f, -1.0f);				// Top Left Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(-1.0f, 1.0f, 1.0f);				// Bottom Left Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(1.0f, 1.0f, 1.0f);				// Bottom Right Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(1.0f, 1.0f, -1.0f);				// Top Right Of The Texture and Quad
            // Bottom Face
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(-1.0f, -1.0f, -1.0f);				// Top Right Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(1.0f, -1.0f, -1.0f);				// Top Left Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(1.0f, -1.0f, 1.0f);				// Bottom Left Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(-1.0f, -1.0f, 1.0f);				// Bottom Right Of The Texture and Quad
            // Right Face
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(1.0f, -1.0f, -1.0f);				// Bottom Right Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(1.0f, 1.0f, -1.0f);				// Top Right Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(1.0f, 1.0f, 1.0f);				// Top Left Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(1.0f, -1.0f, 1.0f);				// Bottom Left Of The Texture and Quad
            // Left Face
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(-1.0f, -1.0f, -1.0f);				// Bottom Left Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(-1.0f, -1.0f, 1.0f);				// Bottom Right Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(-1.0f, 1.0f, 1.0f);				// Top Right Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(-1.0f, 1.0f, -1.0f);				// Top Left Of The Texture and Quad
#else
                    //byte[] buffer = new byte[];
                    //IntPtr 
                    //Gl.glDrawElements(Gl.GL_LINE_LOOP, 4, Gl.GL_UNSIGNED_BYTE, ip);
                    // Front Face
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Top Left Of The Texture and Quad
                    // Back Face
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Left Of The Texture and Quad
                    // Top Face
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Right Of The Texture and Quad
                    // Bottom Face
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                    // Right Face
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                    // Left Face
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad
#endif
                    Gl.glEnd();
                }
                #endregion OjwBox_Text

                private int m_uiMode = Gl.GL_RENDER;
                public void OjwSetMode(int uiMode)
                {
                    m_uiMode = uiMode;
                }

                #region StandardAxis
                private bool m_bStandardAxis = false;
                public bool IsStandardAxis() { return m_bStandardAxis; }
                public void SetStandardAxis(bool bEn) { m_bStandardAxis = bEn; }
                #endregion StandardAxis

                #region VirtualvClass
                private bool m_bVirtualClass = false;
                COjwDisp OjwVirtualDisp = new COjwDisp();
                public void SetVirtualClass_Enable(bool bOn) { m_bVirtualClass = bOn; }
                public void SetVirtualClass_Data(COjwDisp CDisp) { OjwVirtualDisp = CDisp; }
                public COjwDisp GetVirtualClass_Data() { return OjwVirtualDisp.Clone(); }
                public void InitVirtualClass() { OjwVirtualDisp.InitData(); }
                public bool IsVirtualClass() { return m_bVirtualClass; }
                public void AddVirtualClassToReal()
                {
                    String strResult = String.Empty;

                    Convert_CDisp_To_String(GetVirtualClass_Data(), ref strResult);

                    AddHeader_strDrawModel(strResult);
                    CompileDesign();

                    //DispModelString();
                    InitVirtualClass();
                    Prop_Update_VirtualObject();
                }


                #endregion TestClass

                #region TestDh
                private bool m_bTestDh = false;
                private float[] m_afDhInitPoint = new float[3] { 0.0f, 0.0f, 0.0f };
                private float[] m_afDhPoint = new float[3] { 0.0f, 0.0f, 0.0f };
                //private float[] m_afDhAngle = new float[3] { 0.0f, 0.0f, 0.0f };
                private float[] m_afDhAngle_X = new float[3];// { 0.0f, 0.0f, 0.0f };
                private float[] m_afDhAngle_Y = new float[3];// { 0.0f, 0.0f, 0.0f };
                private float[] m_afDhAngle_Z = new float[3];// { 0.0f, 0.0f, 0.0f };
                private float m_fDhSize = 10.0f;
                private float m_fDhAlpha = 1.0f;
                private Color m_cDh = Color.Red;
                public void SetTestDh(bool bEn) { m_bTestDh = bEn; }
                public bool IsTestDh() { return m_bTestDh; }
                public void SetTestDh_InitPos(float fX, float fY, float fZ) { m_afDhInitPoint[0] = fX; m_afDhInitPoint[1] = fY; m_afDhInitPoint[2] = fZ; }
                public void SetTestDh_Pos(float fX, float fY, float fZ) { m_afDhPoint[0] = fX; m_afDhPoint[1] = fY; m_afDhPoint[2] = fZ; }
                //public void SetTestDh_Angle(float fX, float fY, float fZ) { m_afDhAngle[0] = fX; m_afDhAngle[1] = fY; m_afDhAngle[2] = fZ; }
                public void SetTestDh_Angle(float[] afX, float[] afY, float[] afZ)
                {
                    Array.Copy(afX, 0, m_afDhAngle_X, 0, 3);
                    Array.Copy(afY, 0, m_afDhAngle_Y, 0, 3);
                    Array.Copy(afZ, 0, m_afDhAngle_Z, 0, 3);
                    //int _X = 0;
                    //int _Y = 1;
                    //int _Z = 2;
                    //for (int i = 0; i < 3; i++)
                    //{
                    //    m_afDhAngle[i, _X] = afX[i];
                    //    m_afDhAngle[i, _Y] = afY[i];
                    //    m_afDhAngle[i, _Z] = afZ[i];
                    //}
                }
                public void SetTestDh_Size(float fSize) { m_fDhSize = fSize; }
                public void SetTestDh_Color(Color cColor) { m_cDh = cColor; }
                public void SetTestDh_Alpha(float fAlpha) { m_fDhAlpha = fAlpha; }
                #endregion TestDh

                #region TestCircle
                private bool m_bTestCircle = false;
                private bool m_bTestAxis = false;
                private Color m_cColor_Test = Color.Red;
                public float[] m_afPos_Test = new float[3];
                public float[] m_afAngle_Test = new float[3];
                public void SetTestCircle(bool bOn)
                {
                    m_bTestCircle = bOn;
                }
                public void SetTestAxis(bool bOn)
                {
                    m_bTestAxis = bOn;
                }

                public bool IsTestCircle() { return m_bTestCircle; }
                public bool IsTestAxis() { return m_bTestAxis; }
                public void SetColor_Test(Color cColor) { m_cColor_Test = cColor; }
                public Color GetColor_Test() { return m_cColor_Test; }
                private float m_fTestSize = 10.0f;
                public void SetSize_Test(float fSize) { m_fTestSize = fSize; }
                public void SetPos_Test(float fX, float fY, float fZ)
                {
                    m_afPos_Test[0] = fX;
                    m_afPos_Test[1] = fY;
                    m_afPos_Test[2] = fZ;
                }
                public void GetPos_Test(out float fX, out float fY, out float fZ)
                {
                    fX = m_afPos_Test[0];
                    fY = m_afPos_Test[1];
                    fZ = m_afPos_Test[2];
                }
                public void SetAngle_Test(float fPan, float fTilt, float fSwing)
                {
                    m_afAngle_Test[0] = fPan;
                    m_afAngle_Test[1] = fTilt;
                    m_afAngle_Test[2] = fSwing;
                }
                public void GetAngle_Test(out float fX, out float fY, out float fZ)
                {
                    fX = m_afAngle_Test[0];
                    fY = m_afAngle_Test[1];
                    fZ = m_afAngle_Test[2];
                }
                #endregion TestCircle

                #region OpenNurbs
                public void OjwModeling()
                {
                    return;
                    //             for (int i = 0; i < model.m_object_table.Count(); i++)
                    //             {
                    //                 // Get a model object
                    //                 IOnXModel_Object model_obj = model.m_object_table[i];
                    //                 if (model_obj == null) continue;
                    //                 
                    //                 // Get the geometry of the object
                    //                 IOnObject obj = model_obj.m_object;
                    //                 if (obj == null) continue;
                    // 
                    //                 // Is the geometry a curve?
                    //                 IOnCurve crv = OnCurve.ConstCast(obj);
                    //                 if (crv != null)
                    //                 {
                    //                     // OnCurve is a virtual class. So, if we want the curve's data,
                    //                     // then we need to cast the curve object as one of the implemented
                    //                     // curve types
                    // 
                    //                     // Is the curve a line?
                    //                     IOnLineCurve line_crv = OnLineCurve.ConstCast(crv);
                    //                     if (line_crv != null)
                    //                     {
                    //                         // TODO: process line curve
                    //                         continue;
                    //                     }
                    // 
                    //                     // Is the curve a polyline?
                    //                     IOnPolylineCurve pline_crv = OnPolylineCurve.ConstCast(crv);
                    //                     if (pline_crv != null)
                    //                     {
                    //                         // TODO: process polyline curve
                    //                         continue;
                    //                     }
                    // 
                    //                     // Is the curve an arc (or circle)?
                    //                     IOnArcCurve arc_crv = OnArcCurve.ConstCast(crv);
                    //                     if (arc_crv != null)
                    //                     {
                    //                         // TODO: process arc curve
                    //                         continue;
                    //                     }
                    // 
                    //                     // Is the curve a polycurve?
                    //                     IOnPolyCurve poly_crv = OnPolyCurve.ConstCast(crv);
                    //                     if (poly_crv != null)
                    //                     {
                    //                         // TODO: process poly curve
                    //                         continue;
                    //                     }
                    // 
                    //                     // Is the curve a NURBS curve?
                    //                     IOnNurbsCurve nurbs_crv = OnNurbsCurve.ConstCast(crv);
                    //                     if (nurbs_crv != null)
                    //                     {
                    //                         // TODO: process NURBS curve
                    //                         continue;
                    //                     }
                    //                 }
                    //             }
                }
                #endregion OpenNurbs

                // Box which rotates around the center of the upper surface(Kor: 윗면의 중심을 기준으로 회전하는 상자)
                #region OjwBox
                public void OjwBox(bool bFill, Color color, float fAlpha,
                                    float fW, float fH, float fD            // Input the Size(Kor: Size 기입)
                                )
                {
                    int nLoopCount = ((bFill == true) ? 2 : 1);
                    for (int nLoop = 0; nLoop < nLoopCount; nLoop++)
                    {
                        int nSub = _COLOR_GAP;
                        Color cColor = color;// ((nLoop == 0) ? color : Color.DarkGray);
                        m_fColor[0] = ((float)((nLoop == 0) ? cColor.R : cColor.R - nSub) / 255.0f);  // R
                        m_fColor[1] = ((float)((nLoop == 0) ? cColor.G : cColor.G - nSub) / 255.0f);  // G
                        m_fColor[2] = ((float)((nLoop == 0) ? cColor.B : cColor.B - nSub) / 255.0f);  // B
                        m_fColor[3] = fAlpha;// m_fAlpha;//
                        for (int j = 0; j < 3; j++)
                        {
                            if (m_fColor[j] < 0) m_fColor[j] = ((float)(((j == 0) ? cColor.R : ((j == 1) ? cColor.G : cColor.B)) + nSub) / 255.0f);//0.0f;
                        }
                        Gl.glColor4fv(m_fColor);
                        //Gl.glPolygonMode(Gl.GL_FRONT, (int)((bFill == true) ? Gl.GL_FILL : Gl.GL_POINT));
                        //Gl.glPolygonMode(Gl.GL_BACK, (int)((bFill == true) ? Gl.GL_LINE : Gl.GL_LINE));
                        //Gl.glColor3fv(m_fColor); // Color with an array of floats

#if _ABS_POS
                        float fX1 = (-fW + m_fAbsPos_X) / 2.0f;
                        float fX2 = (fW + m_fAbsPos_X) / 2.0f;// fX / 2.0f;
                        float fY1 = m_fAbsPos_Y;// fY / 2.0f;
                        float fY2 = (-fH + m_fAbsPos_Y); // / 2.0f;
                        float fZ1 = (-fD + m_fAbsPos_Z) / 2.0f;
                        float fZ2 = (fD + m_fAbsPos_Z) / 2.0f;
#else
                        float fX1 = -fW / 2.0f;
                        float fX2 = fW / 2.0f;
                        float fY1 = 0;
                        float fY2 = -fH;
                        float fZ1 = -fD / 2.0f;
                        float fZ2 = fD / 2.0f;
#endif
                        // x2 -> direction : right(Kor: 증분방향 : 오른쪽)
                        // Y2 -> direction : Up(Kor: 증분방향 : 위쪽)
                        // Z2 -> direction : The inside of the screen(Kor: 증분방향 : 화면의 안쪽)
                        // Criteria screen : See picture below(Kor: 기준화면 : 밑 그림 참조)
                        // 0, 0, 0 -> State in the middle of the screen as a starting point and went inside by a factor of 5
                        // Kor: 0, 0, 0 -> 화면의 가운데를 시작점으로 하고 안쪽으로 5만큼 들어간 상태
                        /*
                            000000000
                          0 0 ★  0 0
                        000000000   0
                        0   0   0   0
                        0   000000000  
                        0 0     0 0
                        000000000  
                        ( Center position )
                        */

                        //int uiType = (bFill == true) ? Gl.GL_QUADS : Gl.GL_LINE_LOOP;//Gl.GL_LINE_LOOP;// Gl.GL_QUADS;//
                        //int uiType = (bFill == true) ? Gl.GL_POLYGON : Gl.GL_LINE_LOOP;
                        
                        int uiType = ((bFill == true) ? ((nLoop == 0) ? Gl.GL_POLYGON : Gl.GL_LINE_LOOP) : Gl.GL_LINE_LOOP);
                        int uiTypeTop = (bFill == true) ? Gl.GL_FILL : Gl.GL_LINE;
                        //int uiType = Gl.GL_TRIANGLE_FAN;// (bFill == true) ? Gl.GL_TRIANGLE_FAN : Gl.GL_LINE_LOOP;
                        Gl.glPolygonMode(Gl.GL_BACK, uiTypeTop);
                        //Gl.glPolygonMode(Gl.GL_FRONT, (int)((bFill == true) ? Gl.GL_FILL : Gl.GL_POINT));
                        //Gl.glPolygonMode(Gl.GL_BACK, (int)((bFill == true) ? Gl.GL_LINE : Gl.GL_LINE));
                        Gl.glBegin(uiType);

                        //if (m_uiMode == Gl.GL_SELECT) Gl.glLoadName(111);

                        //Gl.glBindTexture(Gl.GL_TEXTURE_2D, m_puiTexture[0]);									// Select Our Texture
                        //Gl.glBegin(uiType);
#if true
                        // Front Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Left Of The Texture and Quad

                        Gl.glEnd();// end drawing the cube	
                        Gl.glBegin(uiType);

                        // Back Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Right Of The Texture and Quad

                        Gl.glEnd();// end drawing the cube	
                        Gl.glBegin(uiType);

                        // Top Face
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad

                        Gl.glEnd();// end drawing the cube	
                        Gl.glBegin(uiType);

                        // Bottom Face
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Top Right Of The Texture and Quad

                        Gl.glEnd();// end drawing the cube	
                        Gl.glBegin(uiType);

                        // Right Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Right Of The Texture and Quad

                        Gl.glEnd();// end drawing the cube	
                        Gl.glBegin(uiType);

                        // Left Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Left Of The Texture and Quad
#else
                        // Front Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Top Left Of The Texture and Quad
                        // Back Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Left Of The Texture and Quad
                        // Top Face
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Right Of The Texture and Quad
                        // Bottom Face
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                        // Right Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                        // Left Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad
#endif
                        Gl.glEnd();// end drawing the cube	
                    }
                }
                #endregion OjwBox;

                // Box which rotates around the center of the upper surface(Making remotely moving box by offset)
                // Kor: 윗면의 중심을 기준으로 회전하는 상자(원격 상자 만들기)
                #region OjwBox_Outside
                public void OjwBox_Outside(bool bFill, Color color, float fAlpha,
                                    float fW, float fH, float fD,            
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,   // Rotation Axis(Offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ // Offset Position
                                )
                {
                    int nLoopCount = ((bFill == true) ? 2 : 1);
                    for (int nLoop = 0; nLoop < nLoopCount; nLoop++)
                    {
                        int nSub = _COLOR_GAP;
                        Color cColor = color;// ((nLoop == 0) ? color : Color.DarkGray);
                        m_fColor[0] = ((float)((nLoop == 0) ? cColor.R : cColor.R - nSub) / 255.0f);  // R
                        m_fColor[1] = ((float)((nLoop == 0) ? cColor.G : cColor.G - nSub) / 255.0f);  // G
                        m_fColor[2] = ((float)((nLoop == 0) ? cColor.B : cColor.B - nSub) / 255.0f);  // B
                        m_fColor[3] = fAlpha;// m_fAlpha;//
                        for (int j = 0; j < 3; j++)
                        {
                            if (m_fColor[j] < 0) m_fColor[j] = ((float)(((j == 0) ? cColor.R : ((j == 1) ? cColor.G : cColor.B)) + nSub) / 255.0f);//0.0f;
                        }
                        Gl.glColor4fv(m_fColor);
                        //Gl.glColor3fv(m_fColor); // Color with an array of floats

                        Gl.glPushMatrix();
                        float fX1 = -fW / 2.0f;
                        float fX2 = fW / 2.0f;
                        float fY1 = 0;
                        float fY2 = -fH;
                        float fZ1 = -fD / 2.0f;
                        float fZ2 = fD / 2.0f;
                        // x2 -> direction : right(Kor: 증분방향 : 오른쪽)
                        // Y2 -> direction : Up(Kor: 증분방향 : 위쪽)
                        // Z2 -> direction : The inside of the screen(Kor: 증분방향 : 화면의 안쪽)
                        // Criteria screen : See picture below(Kor: 기준화면 : 밑 그림 참조)
                        // 0, 0, 0 -> State in the middle of the screen as a starting point and went inside by a factor of 5
                        // Kor: 0, 0, 0 -> 화면의 가운데를 시작점으로 하고 안쪽으로 5만큼 들어간 상태
                        /*
                            000000000
                          0 0 ★  0 0
                        000000000   0
                        0   0   0   0
                        0   000000000  
                        0 0     0 0
                        000000000  
                        ( center position )
                        */

                        //int uiType = (bFill == true) ? Gl.GL_QUADS : Gl.GL_LINE_LOOP;//Gl.GL_LINE_LOOP;// Gl.GL_QUADS;//
                        ////int uiType = (bFill == true) ? Gl.GL_POLYGON : Gl.GL_LINE_LOOP;//Gl.GL_LINE_LOOP;// Gl.GL_QUADS;//
                        
                        
                        int uiType = ((bFill == true) ? ((nLoop == 0) ? Gl.GL_POLYGON : Gl.GL_LINE_LOOP) : Gl.GL_LINE_LOOP);
                        int uiTypeTop = (bFill == true) ? Gl.GL_FILL : Gl.GL_LINE;
                        //int uiType = Gl.GL_TRIANGLE_FAN;// (bFill == true) ? Gl.GL_TRIANGLE_FAN : Gl.GL_LINE_LOOP;
                        Gl.glPolygonMode(Gl.GL_BACK, uiTypeTop);
                        //Gl.glBegin(uiType);

                        SVector3D_t[] aSPos = new SVector3D_t[24];
                        int i = 0;
                        // Front Face		
                        aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Left Of The Texture and Quad
                        aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Right Of The Texture and Quad
                        aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Top Right Of The Texture and Quad
                        aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Top Left Of The Texture and Quad

                        // Back Face			
                        aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Right Of The Texture and Quad
                        aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Left Of The Texture and Quad
                        aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Bottom Left Of The Texture and Quad
                        aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Bottom Right Of The Texture and Quad

                        // Top Face		
                        aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Left Of The Texture and Quad
                        aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Bottom Left Of The Texture and Quad
                        aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Bottom Right Of The Texture and Quad
                        aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Right Of The Texture and Quad

                        // Bottom Face		
                        aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Top Left Of The Texture and Quad
                        aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Left Of The Texture and Quad
                        aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Right Of The Texture and Quad
                        aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Top Right Of The Texture and Quad

                        // Right Face		
                        aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Bottom Right Of The Texture and Quad
                        aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Right Of The Texture and Quad
                        aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Top Left Of The Texture and Quad
                        aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Left Of The Texture and Quad
                        // Left Face				
                        aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Right Of The Texture and Quad
                        aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Top Right Of The Texture and Quad
                        aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Left Of The Texture and Quad
                        aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Bottom Left Of The Texture and Quad
                        int nCnt = i;

                        for (i = 0; i < nCnt; i++)
                        {
                            // Rotation
                            Rotation(fOffsetTilt, fOffsetPan, fOffsetSwing, ref aSPos[i].x, ref aSPos[i].y, ref aSPos[i].z);

                            // Translation
                            aSPos[i].x += fOffsetX;
                            aSPos[i].y += fOffsetY;
                            aSPos[i].z += fOffsetZ;
                        }
                        i = 0;

                        float[] afObject = new float[4] { (float)(color.R), (float)(color.G), (float)(color.B), 1.0f };

                        Gl.glBegin(uiType);

                        //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, afObject); // Object attributes assigned(Kor: 물체 특성할당)

#if true

                        // Front Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;			// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i - 4].x, aSPos[i - 4].y, aSPos[i - 4].z); 			// Bottom Left Of The Texture and Quad

                        Gl.glEnd();// end drawing the cube	
                        Gl.glBegin(uiType);
                        //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, afObject); // Object attributes assigned(Kor: 물체 특성할당)

                        // Back Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i - 4].x, aSPos[i - 4].y, aSPos[i - 4].z); 				// Bottom Right Of The Texture and Quad

                        Gl.glEnd();// end drawing the cube	
                        Gl.glBegin(uiType);
                        //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, afObject); // Object attributes assigned(Kor: 물체 특성할당)

                        // Top Face
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i - 4].x, aSPos[i - 4].y, aSPos[i - 4].z); 				// Top Left Of The Texture and Quad

                        Gl.glEnd();// end drawing the cube	
                        Gl.glBegin(uiType);
                        //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, afObject); // Object attributes assigned(Kor: 물체 특성할당)

                        // Bottom Face
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i - 4].x, aSPos[i - 4].y, aSPos[i - 4].z); 				// Top Right Of The Texture and Quad

                        Gl.glEnd();// end drawing the cube	
                        Gl.glBegin(uiType);
                        //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, afObject); // Object attributes assigned(Kor: 물체 특성할당)

                        // Right Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i - 4].x, aSPos[i - 4].y, aSPos[i - 4].z); 				// Bottom Right Of The Texture and Quad

                        Gl.glEnd();// end drawing the cube	
                        Gl.glBegin(uiType);
                        //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, afObject); // Object attributes assigned(Kor: 물체 특성할당)

                        // Left Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i - 4].x, aSPos[i - 4].y, aSPos[i - 4].z); 				// Bottom Left Of The Texture and Quad

#else
                        // Front Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;			// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
                        // Back Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
                        // Top Face
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        // Bottom Face
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        // Right Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
                        // Left Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
#endif
                        Gl.glEnd();// end drawing the cube	
                        Gl.glPopMatrix();
                    }
                }
                #endregion OjwBox_Outside

                // Case which rotates around the center of the upper surface
                // Kor: 윗면의 중심을 기준으로 회전하는 'ㄷ' 자형 상자
                #region OjwCase
                public void OjwCase(bool bFill, Color color, float fAlpha, bool bRound, bool bInverseType, float fAspectGap,
                                    float fW, float fH, float fD, float fThickness,            
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // Ratation(Offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ              // Translation(Offset)
                                )
                {
                    int nLoopCount = 1;// ((bFill == true) ? 2 : 1);
                    for (int nLoop = 0; nLoop < nLoopCount; nLoop++)
                    {
                        int nSub = _COLOR_GAP;
                        Color cColor = color;// ((nLoop == 0) ? color : Color.DarkGray);
                        m_fColor[0] = ((float)((nLoop == 0) ? cColor.R : cColor.R - nSub) / 255.0f);  // R
                        m_fColor[1] = ((float)((nLoop == 0) ? cColor.G : cColor.G - nSub) / 255.0f);  // G
                        m_fColor[2] = ((float)((nLoop == 0) ? cColor.B : cColor.B - nSub) / 255.0f);  // B
                        m_fColor[3] = fAlpha;// m_fAlpha;//
                        for (int j = 0; j < 3; j++)
                        {
                            if (m_fColor[j] < 0) m_fColor[j] = ((float)(((j == 0) ? cColor.R : ((j == 1) ? cColor.G : cColor.B)) + nSub) / 255.0f);//0.0f;
                        }
                        Gl.glColor4fv(m_fColor);
                        //Gl.glColor3fv(m_fColor); // Color with an array of floats

                        Gl.glPushMatrix();

                        OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                        OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);


                        float fT = fThickness;
                        // x2 -> direction : right(Kor: 증분방향 : 오른쪽)
                        // Y2 -> direction : Up(Kor: 증분방향 : 위쪽)
                        // Z2 -> direction : The inside of the screen(Kor: 증분방향 : 화면의 안쪽)
                        // Criteria screen : See picture below(Kor: 기준화면 : 밑 그림 참조)
                        // 0, 0, 0 -> State in the middle of the screen as a starting point and went inside by a factor of 5
                        // Kor: 0, 0, 0 -> 화면의 가운데를 시작점으로 하고 안쪽으로 5만큼 들어간 상태
                        /*
                            000000000
                          0 0 ★  0 0
                        000000000   0
                        0   0   0   0
                        0   000000000  
                        0 0     0 0
                        000000000  
                        ( Center Position )
                        */

                        //uint uiType = (bFill == true) ? Gl.GL_SMOOTH : Gl.GL_LINE_LOOP;//Gl.GL_QUADS : Gl.GL_LINE_LOOP;//Gl.GL_LINE_LOOP;// Gl.GL_QUADS;//
                        //int uiType = (bFill == true) ? Gl.GL_QUADS : Gl.GL_LINE_LOOP;//Gl.GL_QUADS : Gl.GL_LINE_LOOP;//Gl.GL_LINE_LOOP;// Gl.GL_QUADS;//
                        //int uiType = ((bFill == true) ? ((nLoop == 0) ? Gl.GL_QUADS : Gl.GL_LINE_LOOP) : Gl.GL_LINE_LOOP);

                        //int uiType = ((bFill == true) ? ((nLoop == 0) ? Gl.GL_POLYGON : Gl.GL_POLYGON_STIPPLE) : Gl.GL_LINE_LOOP);
                        int uiType = ((bFill == true) ? ((nLoop == 0) ? Gl.GL_POLYGON : Gl.GL_LINE_LOOP) : Gl.GL_LINE_LOOP);
                        int uiTypeTop = (bFill == true) ? Gl.GL_FILL : Gl.GL_LINE;
                        //int uiType = Gl.GL_TRIANGLE_FAN;// (bFill == true) ? Gl.GL_TRIANGLE_FAN : Gl.GL_LINE_LOOP;
                        Gl.glPolygonMode(Gl.GL_BACK, uiTypeTop);
                        //Gl.glBegin(uiType);


                        //Gl.GL_POLYGON : Gl.GL_TRIANGLES
                        float fMinus = (fW >= 0) ? 1.0f : -1.0f;
                        float fX1 = -(fW + fT * 2 * fMinus) / 2.0f;
                        float fX2 = (fW + fT * 2 * fMinus) / 2.0f;
                        float fY1 = (bInverseType == false) ? 0.0f : -(fH - fT);
                        float fY2 = (bInverseType == false) ? (-fT) : (-fH);
                        float fZ1 = -fD / 2.0f + ((bInverseType == true) ? fAspectGap : 0);
                        float fZ2 = fD / 2.0f + ((bInverseType == true) ? fAspectGap : 0);

                        //// Top or Bottom ////
                        Gl.glBegin(uiType);

                        //OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                        //OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);

                        // Front Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                        // Back Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                        // Top Face
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad
                        // Bottom Face
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Top Right Of The Texture and Quad
                        // Right Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                        // Left Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Left Of The Texture and Quad

                        Gl.glEnd();

                        fX1 = (-fW - fT * 2 * fMinus) / 2.0f;
                        fX2 = -fW / 2.0f;// fX / 2.0f;
                        fY1 = -((bInverseType == false) ? fT : 0);
                        fY2 = -(fH - ((bInverseType == true) ? fT : 0));
                        fZ1 = -fD / 2.0f;
                        fZ2 = fD / 2.0f;

                        if (bInverseType == true) fAspectGap = -fAspectGap;
                        float fTop = 0;// (bInverseType == true) ? fAspectGap * 0 : 0;
                        float fBottom = (bInverseType == false) ? fAspectGap : -fAspectGap;
                        //// Left ////
                        Gl.glBegin(uiType);

                        // Front Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2 + fBottom);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2 + fBottom);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2 + fTop);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2 + fTop);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2 + fBottom);				// Bottom Left Of The Texture and Quad
                        // Back Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1 + fTop);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1 + fTop);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1 + fBottom);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Bottom Right Of The Texture and Quad
                        // Top Face
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1 + fTop);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY1, fZ2 + fTop);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY1, fZ2 + fTop);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1 + fTop);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1 + fTop);				// Top Left Of The Texture and Quad
                        // Bottom Face
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY2, fZ1 + fBottom);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2 + fBottom);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2 + fBottom);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Top Right Of The Texture and Quad
                        // Right Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1 + fBottom);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1 + fTop);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2 + fTop);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2 + fBottom);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1 + fBottom);				// Bottom Right Of The Texture and Quad
                        // Left Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2 + fBottom);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2 + fTop);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1 + fTop);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Bottom Left Of The Texture and Quad

                        Gl.glEnd();

                        //Gl.glBegin(uiType);
                        if ((bRound == true) && (nLoop == 0))
                        {
                            float fH2 = (bInverseType == false) ? fH : 0;
                            float fGap = 0.1f;
                            float fT2 = (fThickness - fGap * 2.0f) * fMinus;
                            Gl.glPushMatrix();
                            OjwTranslate((-fW / 2.0f - ((fMinus == 1.0f) ? fT2 : -fGap * 2.0f) - fGap), -fH2, ((bInverseType == false) ? fAspectGap : 0));
                            OjwRotation(90, 0, 0);
                            OjwCircle(bFill, color, fAlpha, fD / 2.0f, fT2 * fMinus, 30);

                            OjwTranslate(0, 0, (fW + fT2 + fGap * 2.0f * fMinus));
                            OjwCircle(bFill, color, fAlpha, fD / 2.0f, fT2 * fMinus, 30);
                            Gl.glPopMatrix();
                        }
                        //Gl.glEnd();

                        fX1 = (fW + fT * 2 * fMinus) / 2.0f;
                        fX2 = fW / 2.0f;// fX / 2.0f;
                        fY1 = -((bInverseType == false) ? fT : 0);
                        fY2 = -(fH - ((bInverseType == true) ? fT : 0));
                        fZ1 = -fD / 2.0f;
                        fZ2 = fD / 2.0f;

                        //// Right ////
                        Gl.glBegin(uiType);

                        // Front Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2 + fBottom);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2 + fBottom);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2 + fTop);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2 + fTop);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2 + fBottom);				// Bottom Left Of The Texture and Quad
                        // Back Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1 + fTop);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1 + fTop);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1 + fBottom);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Bottom Right Of The Texture and Quad
                        // Top Face
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1 + fTop);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY1, fZ2 + fTop);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY1, fZ2 + fTop);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1 + fTop);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1 + fTop);				// Top Left Of The Texture and Quad
                        // Bottom Face
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY2, fZ1 + fBottom);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2 + fBottom);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2 + fBottom);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Top Right Of The Texture and Quad
                        // Right Face
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1 + fBottom);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1 + fTop);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2 + fTop);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2 + fBottom);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1 + fBottom);				// Bottom Right Of The Texture and Quad
                        // Left Face
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Bottom Left Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2 + fBottom);				// Bottom Right Of The Texture and Quad
                        Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2 + fTop);				// Top Right Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1 + fTop);				// Top Left Of The Texture and Quad
                        Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Bottom Left Of The Texture and Quad

                        Gl.glEnd();

                        //if (bRound == true)
                        //{
                        //    float fH2 = (bInverseType == false) ? fH : 0;
                        //    float fGap = 0.1f;
                        //    float fT2 = (fThickness - fGap * 2.0f) * fMinus;
                        //    Gl.glPushMatrix();
                        //    OjwTranslate((fW / 2.0f - ((fMinus == 1.0f) ? fT2 : -fGap * 2.0f) + fT2 - fGap + fGap * 2.0f * fMinus), -fH2, ((bInverseType == false) ? fAspectGap : 0));
                        //    //OjwTranslate((fW / 2.0f - ((fMinus == 1.0f) ? fT2 : -fGap * 2.0f) - fGap), -fH2, ((bInverseType == false) ? fAspectGap : 0));
                        //    OjwRotation(90, 0, 0);
                        //    OjwCircle(bFill, color, fD / 2.0f, fT2 * fMinus, 30);

                        //    //OjwCircle(bFill, color, fD / 2.0f, fT2 * fMinus, 30);
                        //    Gl.glPopMatrix();
                        //}

                        Gl.glPopMatrix();
                    }
                }
                #endregion OjwCase

                // half-Case which rotates around the center of the upper surface
                // Kor: 윗면의 중심을 기준으로 회전하는 'ㄷ' 자형 상자 반쪽
                #region OjwCase_half
                public void OjwCase_half(bool bFill, Color color, float fAlpha, bool bRound, bool bInverseType, float fAspectGap,
                                    float fW, float fH, float fD, float fThickness,
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // Ratation(Offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ              // Translation(Offset)
                                )
                {
                    Gl.glPushMatrix();

                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);



                    m_fColor[0] = ((float)(color.R) / 255.0f);  // R
                    m_fColor[1] = ((float)(color.G) / 255.0f);  // G
                    m_fColor[2] = ((float)(color.B) / 255.0f);  // B
                    m_fColor[3] = fAlpha;  // B
                    Gl.glColor4fv(m_fColor); // Color with an array of floats

                    fW *= 2.0f;
                    float fT = fThickness;
                    fW -= (fW < 0) ? fT * 2 : 0;

                    int uiType = (bFill == true) ? Gl.GL_QUADS : Gl.GL_LINE_LOOP;//Gl.GL_LINE_LOOP;// Gl.GL_QUADS;//
                    int uiTypeTop = (bFill == true) ? Gl.GL_FILL : Gl.GL_LINE;
                    //int uiType = Gl.GL_TRIANGLE_FAN;// (bFill == true) ? Gl.GL_TRIANGLE_FAN : Gl.GL_LINE_LOOP;
                    Gl.glPolygonMode(Gl.GL_BACK, uiTypeTop);
                    //Gl.glBegin(uiType);


                    float fMinus = (fW >= 0) ? 1.0f : -1.0f;
                    float fX1 = -(fW + fT * 2 * fMinus) / 2.0f;
                    float fX2 = 0;
                    float fY1 = (bInverseType == false) ? 0.0f : -(fH - fT);
                    float fY2 = (bInverseType == false) ? (-fT) : (-fH);
                    float fZ1 = -fD / 2.0f + ((bInverseType == true) ? fAspectGap : 0);
                    float fZ2 = fD / 2.0f + ((bInverseType == true) ? fAspectGap : 0);

                    if (fW < 0) fX1 -= fT;

                    //// Top or Bottom ////
                    Gl.glBegin(uiType);

                    // Front Face
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Top Left Of The Texture and Quad
                    // Back Face
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Left Of The Texture and Quad
                    // Top Face
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Right Of The Texture and Quad
                    // Bottom Face
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                    // Right Face
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2);				// Bottom Left Of The Texture and Quad
                    // Left Face
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1);				// Top Left Of The Texture and Quad

                    Gl.glEnd();

                    //if (bStandard == true)
                    //{
                    fX1 = (-fW - fT * 2 * fMinus) / 2.0f;
                    fX2 = -fW / 2.0f;
                    fY1 = -((bInverseType == false) ? fT : 0);
                    fY2 = -(fH - ((bInverseType == true) ? fT : 0));
                    fZ1 = -fD / 2.0f;
                    fZ2 = fD / 2.0f;

                    if (fW < 0) fX1 -= fT * 2;

                    if (bInverseType == true) fAspectGap = -fAspectGap;
                    float fTop = 0;// (bInverseType == true) ? fAspectGap * 0 : 0;
                    float fBottom = (bInverseType == false) ? fAspectGap : -fAspectGap;
                    //// Left  or Right ////
                    Gl.glBegin(uiType);

                    // Front Face
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2 + fBottom);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2 + fBottom);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2 + fTop);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2 + fTop);				// Top Left Of The Texture and Quad
                    // Back Face
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1 + fTop);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1 + fTop);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1 + fBottom);				// Bottom Left Of The Texture and Quad
                    // Top Face
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1 + fTop);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY1, fZ2 + fTop);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY1, fZ2 + fTop);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1 + fTop);				// Top Right Of The Texture and Quad
                    // Bottom Face
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY2, fZ1 + fBottom);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2 + fBottom);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2 + fBottom);				// Bottom Right Of The Texture and Quad
                    // Right Face
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ1 + fBottom);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ1 + fTop);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX2, fY1, fZ2 + fTop);				// Top Left Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX2, fY2, fZ2 + fBottom);				// Bottom Left Of The Texture and Quad
                    // Left Face
                    Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ1 + fBottom);				// Bottom Left Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(fX1, fY2, fZ2 + fBottom);				// Bottom Right Of The Texture and Quad
                    Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ2 + fTop);				// Top Right Of The Texture and Quad
                    Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(fX1, fY1, fZ1 + fTop);				// Top Left Of The Texture and Quad

                    Gl.glEnd();

                    if (bRound == true)
                    {
                        float fH2 = (bInverseType == false) ? fH : 0;
                        float fGap = 0.1f;
                        float fT2 = (fThickness - fGap * 2.0f) * fMinus;
                        Gl.glPushMatrix();

                        OjwTranslate((-fW / 2.0f - ((fMinus == 1.0f) ? (fT2) : (-fGap * 2.0f + fT)) - fGap), -fH2, ((bInverseType == false) ? fAspectGap : 0));
                        OjwRotation(90, 0, 0);
                        OjwCircle(bFill, color, fAlpha, fD / 2.0f, fT2 * fMinus, 30);

                        Gl.glPopMatrix();
                    }

                    Gl.glPopMatrix();
                }
                #endregion OjwCase_half

                // Circle which rotates around the center of the upper surface윗면의 중심을 기준으로 회전하는 Circle
                #region OjwCircle
                public void OjwCircle(bool bFill, Color color, float fAlpha, float fR, float fD, int nSolidCnt)
                {

                    //fR *= fScale;
                    //fD *= fScale;

                    Glu.GLUquadric quadObj;
                    // Create quadric object
                    int nLoopCount = ((bFill == true) ? 2 : 1);
                    for (int nLoop = 0; nLoop < nLoopCount; nLoop++)
                    {
                        int nSub = _COLOR_GAP;
                        Color cColor = color;// ((i == 0) ? color : Color.DarkGray);
                        m_fColor[0] = ((float)((nLoop == 0) ? cColor.R : cColor.R - nSub) / 255.0f);  // R
                        m_fColor[1] = ((float)((nLoop == 0) ? cColor.G : cColor.G - nSub) / 255.0f);  // G
                        m_fColor[2] = ((float)((nLoop == 0) ? cColor.B : cColor.B - nSub) / 255.0f);  // B
                        m_fColor[3] = fAlpha;// m_fAlpha;//
                        for (int j = 0; j < 3; j++)
                        {
                            if (m_fColor[j] < 0) m_fColor[j] = ((float)(((j == 0) ? cColor.R : ((j == 1) ? cColor.G : cColor.B)) + nSub) / 255.0f);//0.0f;
                        }
                        Gl.glColor4fv(m_fColor);
                        //Gl.glColor3fv(m_fColor); // Color with an array of floats

                        quadObj = Glu.gluNewQuadric();

                        Gl.glPushMatrix();

                        //Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                        //Gl.glEnable(Gl.GL_BLEND);
                        //Gl.glEnable(Gl.GL_LINE_SMOOTH);

                        // GLU_LINE -> Wire Frame
                        // GLU_FILL -> Solid
                        // GLU_POINT -> Point
                        // GLU_SILHOUETTE -> Only the outer edges with a line figure(Kor: 선으로 외부모서리만을 그림)
                        Glu.gluQuadricDrawStyle(quadObj, (bFill == true) ? ((nLoop == 0) ? Glu.GLU_FILL : Glu.GLU_SILHOUETTE) : Glu.GLU_LINE);

                        // GLU_NONE -> Do not generate normal vector(Kor: 법선벡터 생성 안함)
                        // GLU_FLAT -> The surface normal vector to produce it seem shorn(Kor: 면이 깎인 것처럼 보이도록 법선벡터 생성)
                        // GLU_SMOOTH -> The normal vector of the corner to create an object looks smooth(Kor: 물체의 모서리가 부드럽게 보이도록 법선벡터 생성)
                        Glu.gluQuadricNormals(quadObj, Glu.GLU_SMOOTH);

                        // Specifies the direction of the normal vector(Kor: 법선벡터의 방향을 지정)
                        // GLU_INSIDE -> Makes the direction of the normal vector to the inside.(Kor: 법선벡터의 방향을 안쪽으로)
                        // GLU_OUTSIDE -> Makes the direction of the normal vector to the outside.(Kor: 법선벡터의 방향을 바깥쪽으로)
                        Glu.gluQuadricOrientation(quadObj, Glu.GLU_INSIDE);
                        Glu.gluDisk(quadObj, 0, fR, nSolidCnt, 1);

                        Glu.gluQuadricOrientation(quadObj, Glu.GLU_OUTSIDE);
                        Glu.gluCylinder(quadObj, fR, fR, fD, nSolidCnt, 2);
#if false
                        // 시작원의 반지름(도우넛 모양이 가능 - 이 값이 클수록 구멍이 넓어짐), 외곽원의 반지름, 원을 이루는 선의 갯수
                        //Glu.gluDisk(quadObj, 0, fR, nSolidCnt, 1);
#endif
                        Gl.glTranslated(0, 0, fD);

                        Glu.gluQuadricOrientation(quadObj, Glu.GLU_OUTSIDE);
                        Glu.gluDisk(quadObj, 0, fR, nSolidCnt, 1);

                        //if (m_bPickMode == true)
                        //{
                        //    PopName();
                        //}

                        Gl.glPopMatrix();

                        Glu.gluDeleteQuadric(quadObj); // remove object
                    }
                    //if (m_bPickMode == true) PopName();

                }
                #endregion OjwCircle

                // A cylinder which rotates around the center of the upper surface(Kor: 윗면의 중심을 기준으로 회전하는 원기둥)
                #region OjwCircle_Outside
                public void OjwCircle_Outside(bool bFill, Color color, float fAlpha, float fR, float fD, int nSolidCnt,
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // Rotate(Offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ              // Translate(Offset)
                                    )
                {
                    int nLoopCount = ((bFill == true) ? 2 : 1);
                    for (int nLoop = 0; nLoop < nLoopCount; nLoop++)
                    {
                        int nSub = _COLOR_GAP;
                        Color cColor = color;// ((nLoop == 0) ? color : Color.DarkGray);
                        m_fColor[0] = ((float)((nLoop == 0) ? cColor.R : cColor.R - nSub) / 255.0f);  // R
                        m_fColor[1] = ((float)((nLoop == 0) ? cColor.G : cColor.G - nSub) / 255.0f);  // G
                        m_fColor[2] = ((float)((nLoop == 0) ? cColor.B : cColor.B - nSub) / 255.0f);  // B
                        m_fColor[3] = fAlpha;// m_fAlpha;//
                        for (int j = 0; j < 3; j++)
                        {
                            if (m_fColor[j] < 0) m_fColor[j] = ((float)(((j == 0) ? cColor.R : ((j == 1) ? cColor.G : cColor.B)) + nSub) / 255.0f);//0.0f;
                        }
                        Gl.glColor4fv(m_fColor);
                        //Gl.glColor3fv(m_fColor); // Color with an array of floats

                        //fR *= fScale;
                        //fD *= fScale;


                        Glu.GLUquadric quadObj;


                        // Create quadric object
                        quadObj = Glu.gluNewQuadric();

                        Gl.glPushMatrix();

                        OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                        OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);

                        //Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                        //Gl.glEnable(Gl.GL_BLEND);
                        //Gl.glEnable(Gl.GL_LINE_SMOOTH);

                        // GLU_LINE -> Wire Frame
                        // GLU_FILL -> Solid
                        // GLU_POINT -> Point
                        // GLU_SILHOUETTE -> Only the outer edges with a line figure(Kor: 선으로 외부모서리만을 그림)
                        //Glu.gluQuadricDrawStyle(quadObj, (bFill == true) ? Glu.GLU_FILL : Glu.GLU_LINE);
                        Glu.gluQuadricDrawStyle(quadObj, (bFill == true) ? ((nLoop == 0) ? Glu.GLU_FILL : Glu.GLU_SILHOUETTE) : Glu.GLU_LINE);

                        // GLU_NONE -> Do not generate normal vector(Kor: 법선벡터 생성 안함)
                        // GLU_FLAT -> The surface normal vector to produce it seem shorn(Kor: 면이 깎인 것처럼 보이도록 법선벡터 생성)
                        // GLU_SMOOTH -> The normal vector of the corner to create an object looks smooth(Kor: 물체의 모서리가 부드럽게 보이도록 법선벡터 생성)
                        Glu.gluQuadricNormals(quadObj, Glu.GLU_SMOOTH);

                        // Specifies the direction of the normal vector(Kor: 법선벡터의 방향을 지정)
                        // GLU_INSIDE -> Makes the direction of the normal vector to the inside.(Kor: 법선벡터의 방향을 안쪽으로)
                        // GLU_OUTSIDE -> Makes the direction of the normal vector to the outside.(Kor: 법선벡터의 방향을 바깥쪽으로)
                        Glu.gluQuadricOrientation(quadObj, Glu.GLU_INSIDE);
                        // The radius of the circle(The higher the number, the hole is widened)(Kor: 시작원의 반지름(도우넛 모양이 가능 - 이 값이 클수록 구멍이 넓어짐), 외곽원의 반지름, 원을 이루는 선의 갯수)
                        Glu.gluDisk(quadObj, 0, fR, nSolidCnt, 1);

                        Glu.gluQuadricOrientation(quadObj, Glu.GLU_OUTSIDE);
                        Glu.gluCylinder(quadObj, fR, fR, fD, nSolidCnt, 1);

                        Gl.glTranslated(0, 0, fD);

                        Glu.gluQuadricOrientation(quadObj, Glu.GLU_OUTSIDE);
                        Glu.gluDisk(quadObj, 0, fR, nSolidCnt, 1);

                        //OjwTranslate(-fOffsetX, -fOffsetY, -fOffsetZ);
                        //OjwRotation(-fOffsetPan, -fOffsetTilt, -fOffsetSwing);

                        Gl.glPopMatrix();

                        Glu.gluDeleteQuadric(quadObj);
                        //if (m_bPickMode == true) PopName();
                    }
                }
                #endregion OjwCircle_Outside

                // Ball to the middle of the center(Kor: 가운데를 중심으로 하는 공)
                #region OjwBall
                public void OjwBall(bool bFill, Color color, float fAlpha, float fR, int nSolidCnt)
                {
                    //if ((nTexture >= 0) && (nTexture < _CNT_TEXTURE)) Gl.glBindTexture(Gl.GL_TEXTURE_2D, m_puiTexture[nTexture]);									// Select Texture

                    m_fColor[0] = ((float)(color.R) / 255.0f);  // R
                    m_fColor[1] = ((float)(color.G) / 255.0f);  // G
                    m_fColor[2] = ((float)(color.B) / 255.0f);  // B
                    m_fColor[3] = fAlpha;  // B
                    Gl.glColor4fv(m_fColor); // Color with an array of floats

                    //fR *= fScale;
                    //#if false//_TAO
                    //#else
                    Glu.GLUquadric quadObj;
                    // Create quadric object
                    quadObj = Glu.gluNewQuadric();

                    Glu.gluQuadricDrawStyle(quadObj, (bFill == true) ? Glu.GLU_FILL : Glu.GLU_LINE);
                    Glu.gluSphere(quadObj, fR, nSolidCnt, 16);										// Draw Another Sphere Using New Texture
                    Glu.gluDeleteQuadric(quadObj);
                    //#endif
                }
                #endregion OjwBall

                // Ball to the middle of the center(Kor: 가운데를 중심으로 하는 공)
                #region OjwBall_Outside
                public void OjwBall_Outside(bool bFill, Color color, float fAlpha, float fR, int nSolidCnt,
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // Rotate(Offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ              // Translate(Offset)
                                    )
                {
                    m_fColor[0] = ((float)(color.R) / 255.0f);  // R
                    m_fColor[1] = ((float)(color.G) / 255.0f);  // G
                    m_fColor[2] = ((float)(color.B) / 255.0f);  // B
                    m_fColor[3] = fAlpha;
                    Gl.glColor4fv(m_fColor); // Color with an array of floats

                    Glu.GLUquadric quadObj;
                    // Create quadric object
                    quadObj = Glu.gluNewQuadric();

                    Gl.glPushMatrix();
                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);
                    Glu.gluQuadricDrawStyle(quadObj, (bFill == true) ? Glu.GLU_FILL : Glu.GLU_LINE);
                    Glu.gluSphere(quadObj, fR, nSolidCnt, 16);										// Draw Another Sphere Using New Texture
                    Gl.glPopMatrix();

                    Glu.gluDeleteQuadric(quadObj); 
                }
                #endregion OjwBall_Outside

                // Cone to the center of the bottom of the center(Kor: 밑면의 가운데를 중심으로 하는 원뿔)
                #region OjwCone_Outside
                public void OjwCone_Outside(bool bFill, Color color, float fAlpha, float fR, float fHeight, int nSolidCnt,
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // Rotate(Offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ              // Translate(Offset)
                                    )
                {
                    //if ((nTexture >= 0) && (nTexture < _CNT_TEXTURE)) Gl.glBindTexture(Gl.GL_TEXTURE_2D, m_puiTexture[nTexture]);									// Select Texture

                    m_fColor[0] = ((float)(color.R) / 255.0f);  // R
                    m_fColor[1] = ((float)(color.G) / 255.0f);  // G
                    m_fColor[2] = ((float)(color.B) / 255.0f);  // B
                    m_fColor[3] = fAlpha;
                    Gl.glColor3fv(m_fColor); // Color with an array of floats

                    //fR *= fScale;
                    //fHeight *= fScale;

                    Gl.glPushMatrix();


                    int uiTypeTop = (bFill == true) ? Gl.GL_FILL : Gl.GL_LINE;
                    //    glPolygonMode(GL_BACK, GL_LINE);
                    Gl.glPolygonMode(Gl.GL_BACK, uiTypeTop);
                    
                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);

                    float angle;
                    float x, y;

                    int uiType = Gl.GL_TRIANGLE_FAN;// (bFill == true) ? Gl.GL_TRIANGLE_FAN : Gl.GL_LINE_LOOP;
                    Gl.glBegin(uiType);
                    // Center of fan is at the origin
                    Gl.glVertex3f(0.0f, 0.0f, fHeight);
                    for (angle = 0.0f; angle < (2.0f * (float)Math.PI); angle += ((float)Math.PI / ((float)nSolidCnt / 2.0f)))
                    {
                        // Calculate x and y position of the next vertex
                        x = fR * (float)Math.Sin(-angle);
                        y = fR * (float)Math.Cos(-angle);
                        // Specify the next vertex for the triangle fan
                        Gl.glVertex2f(x, y);
                    }
                    angle = 2.0f * (float)Math.PI;
                    x = fR * (float)Math.Sin(angle);
                    y = fR * (float)Math.Cos(angle);
                    Gl.glVertex2f(x, y);
                    // Done drawing the fan that covers the bottom
                    Gl.glEnd();



                    // Begin a new triangle fan to cover the bottom
                    Gl.glBegin(uiType);
                    // Center of fan is at the origin
                    Gl.glVertex3f(0.0f, 0.0f, 0.0f);
                    for (angle = 0.0f; angle < (2.0f * (float)Math.PI); angle += ((float)Math.PI / ((float)nSolidCnt / 2.0f)))
                    {
                        // Calculate x and y position of the next vertex
                        x = fR * (float)Math.Sin(angle);
                        y = fR * (float)Math.Cos(angle);
                        // Specify the next vertex for the triangle fan
                        Gl.glVertex3f(x, y, 0.0f);
                    }
                    angle = 2.0f * (float)Math.PI;
                    x = fR * (float)Math.Sin(angle);
                    y = fR * (float)Math.Cos(angle);
                    Gl.glVertex2f(x, y);
                    Gl.glEnd();

                    Gl.glPopMatrix();
                }
                #endregion OjwCone_Outside

                // Axis to set up a collection of functions for display(Kor: 디스플레이용 축 설정 관련 함수 모음)
                #region Axis-All
                public void Axis(bool bFill, Color cX, float fAlphaX, Color cY, float fAlphaY, Color cZ, float fAlphaZ, float fThick, float fLength)
                {
                    Axis_X(bFill, cX, fAlphaX, fThick, fLength, 0, 0, 0, 0, 0, 0);
                    Axis_Y(bFill, cY, fAlphaY, fThick, fLength, 0, 0, 0, 0, 0, 0);
                    Axis_Z(bFill, cZ, fAlphaZ, fThick, fLength, 0, 0, 0, 0, 0, 0);
                }

                public void Axis(bool bFill, Color cX, float fAlphaX, Color cY, float fAlphaY, Color cZ, float fAlphaZ, float fThick, float fLength,
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // rotate(offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ              // translate(offset)
                                    )
                {
                    Gl.glPushMatrix();
                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);
                    Axis_X(bFill, cX, fAlphaX, fThick, fLength, 0, 0, 0, 0, 0, 0);
                    Axis_Y(bFill, cY, fAlphaY, fThick, fLength, 0, 0, 0, 0, 0, 0);
                    Axis_Z(bFill, cZ, fAlphaZ, fThick, fLength, 0, 0, 0, 0, 0, 0);
                    Gl.glPopMatrix();
                }

                public void Axis_X(bool bFill, Color color, float fAlpha, float fThick, float fLength)
                {
                    Axis_X(bFill, color, fAlpha, fThick, fLength, 0, 0, 0, 0, 0, 0);
                }

                public void Axis_Y(bool bFill, Color color, float fAlpha, float fThick, float fLength)
                {
                    Axis_Y(bFill, color, fAlpha, fThick, fLength, 0, 0, 0, 0, 0, 0);
                }

                public void Axis_Z(bool bFill, Color color, float fAlpha, float fThick, float fLength)
                {
                    Axis_Z(bFill, color, fAlpha, fThick, fLength, 0, 0, 0, 0, 0, 0);
                }

                public void Axis_X(bool bFill, Color color, float fAlpha, float fThick, float fLength,
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // rotate(offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ              // translate(offset)
                                    )
                {

                    Gl.glPushMatrix();
                    float fDir = ((fLength < 0) ? -1.0f : 1.0f);
                    if (fLength < 0) fLength = -fLength;
                    float fLength2 = fThick * 1.5f;
                    
                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);

                    OjwBall(bFill, color, fAlpha, fThick / 2.0f, 30);
                    OjwCircle_Outside(bFill, color, fAlpha, fThick / 2.0f, fLength, 30, 90.0f * fDir, 0, 0, 0, 0, 0);
                    OjwCone_Outside(bFill, color, fAlpha, fThick, fLength2, 30, 90.0f * fDir, 0, 0, fLength * fDir, 0, 0);
                    
                    Gl.glPopMatrix();
                }

                public void Axis_Y(bool bFill, Color color, float fAlpha, float fThick, float fLength,
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // rotate(offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ              // translate(offset)
                                    )
                {
                    Gl.glPushMatrix();
                    float fDir = ((fLength < 0) ? -1.0f : 1.0f);
                    if (fLength < 0) fLength = -fLength;
                    float fLength2 = fThick * 1.5f;

                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);

                    OjwRotation(0, 0, 90);
                    OjwBall(bFill, color, fAlpha, fThick / 2.0f, 30);
                    OjwCircle_Outside(bFill, color, fAlpha, fThick / 2.0f, fLength, 30, 90.0f * fDir, 0, 0, 0, 0, 0);
                    OjwCone_Outside(bFill, color, fAlpha, fThick, fLength2, 30, 90.0f * fDir, 0, 0, fLength * fDir, 0, 0);
                    
                    //OjwCircle_Outside(bFill, color, fAlpha, fThick / 2.0f, fLength, 30, fOffsetPan, fOffsetTilt - 90.0f * fDir, fOffsetSwing, fOffsetX, fOffsetY, fOffsetZ);
                    //OjwCone_Outside(bFill, color, fAlpha, fThick, fLength2, 30, fOffsetPan, fOffsetTilt - 90.0f * fDir, fOffsetSwing, fOffsetX, fOffsetY + fLength * fDir, fOffsetZ);
                    Gl.glPopMatrix();
                }

                public void Axis_Z(bool bFill, Color color, float fAlpha, float fThick, float fLength,
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // rotate(offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ              // translate(offset)
                                    )
                {
#if false
                    Gl.glPushMatrix();
                    //float fDir = ((fLength < 0) ? -1.0f : 1.0f);
                    //float fAngle = ((fDir < 0) ? 180.0f : 0.0f);
                    if (fLength < 0)
                    {
                        fLength = -fLength;
                        OjwRotation(180, 0, 0);
                    }
                    //float fDir = ((fLength < 0) ? 180.0f : 1.0f);
                    //if (fLength < 0) fLength = -fLength;
                    float fLength2 = fThick * 1.5f;

                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);

                    OjwBall(bFill, color, fAlpha, fThick / 2.0f, 30);
                    OjwCircle_Outside(bFill, color, fAlpha, fThick / 2.0f, fLength, 30, fOffsetPan, fOffsetTilt, fOffsetSwing, fOffsetX, fOffsetY, fOffsetZ);
                    OjwCone_Outside(bFill, color, fAlpha, fThick, fLength2, 30, fOffsetPan, fOffsetTilt, fOffsetSwing, fOffsetX, fOffsetY, fOffsetZ + fLength);
                    Gl.glPopMatrix();
#else
                    Gl.glPushMatrix();
                    float fDir = ((fLength < 0) ? -1.0f : 1.0f);
                    if (fLength < 0) fLength = -fLength;
                    float fLength2 = fThick * 1.5f;

                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);

                    OjwRotation(-90, 0, 0);

                    OjwBall(bFill, color, fAlpha, fThick / 2.0f, 30);
                    OjwCircle_Outside(bFill, color, fAlpha, fThick / 2.0f, fLength, 30, 90.0f * fDir, 0, 0, 0, 0, 0);
                    OjwCone_Outside(bFill, color, fAlpha, fThick, fLength2, 30, 90.0f * fDir, 0, 0, fLength * fDir, 0, 0);

                    Gl.glPopMatrix();
#endif
                }
                #endregion Axis-All
                            
                #region OjwAse_Outside
                #region File(by Name)
                public int OjwAse_GetIndex(String strIndex)
                {
#if true
                    int nIndex = -1;
                    // Default -> ase
                    if (strIndex.IndexOf('.') < 0) strIndex += ".ase";
                    for (int i = 0; i < m_lstModel.Count; i++)
                    {
                        if (m_lstModel[i] == strIndex) { nIndex = i; break; }
                    }
                    return nIndex;
#else
                    int nIndex = -1;
                    for (int i = 0; i < m_lstModel.Count; i++)
                    {
                        if (m_lstModel[i] == strIndex) { nIndex = i; break; }
                    }
                    return nIndex;
#endif
                }
                public void OjwAse_Outside(bool bFill, Color color, float fAlpha,
                                    float fW, float fH, float fD,            
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,   // rotate(offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ, // translate(offset)
                                    String strIndex_Ase  // File Index name(Kor: 파일 인덱싱 이름)
                                )
                {
                    if (strIndex_Ase.Length == 0) return;

                    int nIndex_Ase = OjwAse_GetIndex(strIndex_Ase);
                    if ((nIndex_Ase >= m_nCnt_Obj_Ase) || (nIndex_Ase < 0)) return;
                                    
                    bool bStl = false;
                    if (strIndex_Ase.ToUpper().IndexOf(".STL") >= 0) bStl = true;

                    Color cColor = color;
                    m_fColor[0] = ((float)cColor.R / 255.0f);  // R
                    m_fColor[1] = ((float)cColor.G / 255.0f);  // G
                    m_fColor[2] = ((float)cColor.B / 255.0f);  // B
                    m_fColor[3] = fAlpha;// m_fAlpha;
#if _STL_CW
                    Gl.glFrontFace((bStl == true) ? Gl.GL_CW : Gl.GL_CCW);
#else
                    Gl.glFrontFace((bStl == true) ? Gl.GL_CCW : Gl.GL_CW);
#endif
#if true // 1
#if _STL_CW
                    Gl.glPolygonMode(Gl.GL_BACK,
                            (int)(
                                (bFill == true) ?
                                    ((m_bDetail == true) ? Gl.GL_LINE : Gl.GL_FILL) : ((m_bDetail == true) ? Gl.GL_POINT : Gl.GL_LINE)
                            )
                        );
                    Gl.glPolygonMode(Gl.GL_FRONT, (int)((bFill == true) ? Gl.GL_FILL : Gl.GL_LINE));
#else       // original                    
                    Gl.glPolygonMode(Gl.GL_FRONT,
                            (int)(
                                (bFill == true) ?
                                    ((m_bDetail == true) ? Gl.GL_POINT : Gl.GL_FILL) : ((m_bDetail == true) ? Gl.GL_POINT : Gl.GL_LINE)
                            )
                        );
                    //Gl.glPolygonMode(Gl.GL_FRONT, (int)((bFill == true) ? Gl.GL_POINT : Gl.GL_POINT));
                    Gl.glPolygonMode(Gl.GL_BACK, (int)((bFill == true) ? Gl.GL_FILL : Gl.GL_LINE));
#endif
#else
                    //Gl.glEdgeFlag(Gl.GL_TRUE);
                    Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_FILL);
                    Gl.glPolygonMode(Gl.GL_BACK, Gl.GL_FILL);
                    //Gl.glPolygonMode(Gl.GL_FRONT, (int)((bFill == true) ? Gl.GL_FILL : Gl.GL_LINE));
                    //Gl.glPolygonMode(Gl.GL_BACK, (int)((bFill == true) ? Gl.GL_FILL : Gl.GL_LINE));
                    //Gl.glPolygonMode(Gl.GL_FRONT, (int)((bFill == true) ? Gl.GL_LINES : Gl.GL_POINT));
                    //Gl.glPolygonMode(Gl.GL_BACK, (int)((bFill == true) ? Gl.GL_FILL : Gl.GL_LINE));
                    //Gl.glPolygonMode(Gl.GL_BACK, (int)((bFill == true) ? Gl.GL_LINE : Gl.GL_LINE));
#endif
                    int uiType = Gl.GL_TRIANGLES;//(int)((bFill == true) ? Gl.GL_TRIANGLES : Gl.GL_LINE_STRIP);//Gl.GL_TRIANGLES;// 

                    Gl.glColor4fv(m_fColor);

                    Gl.glPushMatrix();

                    float fX1 = -fW / 2.0f;
                    float fX2 = fW / 2.0f;
                    float fY2 = -fH;
                    float fZ1 = -fD / 2.0f;
                    float fZ2 = fD / 2.0f;

                    if (m_lstOjwAse[nIndex_Ase].Data_GetCnt() <= 0) return;
#if false
                    //SVector3D_t[] aSPos = new SVector3D_t[m_lstOjwAse[nIndex_Ase].Face_GetCnt() * 3];
                    //int nPos = 0;
                    //int nPos2;
                    //foreach (SPoint3D_t SPnt in pSData)
                    //{
                    //    nPos2 = nPos * 3;
                    //    aSPos[nPos2] = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.x);
                    //    aSPos[nPos2 + 1] = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.y);
                    //    aSPos[nPos2 + 2] = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.z);
                    //    nPos++;
                    //}
#endif
                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(0, 0, fOffsetSwing);
                    OjwRotation(0, fOffsetTilt, 0);
                    OjwRotation(fOffsetPan, 0, 0);

                    Gl.glBegin(uiType);
                    //Gl.glEdgeFlag(Gl.GL_FALSE);  
                    // Draw        
#if false
                    foreach (SVector3D_t SVector3D in aSPos)
                    {
                        Gl.glVertex3f(SVector3D.x, SVector3D.y, SVector3D.z);
                    }
                    aSPos = null;
#else
#if true
                    //CTimer CTmr = new CTimer();
                    //CTmr.Set();
#if false
                    SPoint3D_t[] pSData = m_lstOjwAse[nIndex_Ase].Face_Get();
                    foreach (SPoint3D_t SPnt in pSData)
                    {
                        SVector3D_t SVtx_x = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.x);
                        SVector3D_t SVtx_y = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.y);
                        SVector3D_t SVtx_z = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.z);

                        Gl.glVertex3f(SVtx_x.x, SVtx_x.y, SVtx_x.z);
                        Gl.glVertex3f(SVtx_y.x, SVtx_y.y, SVtx_y.z);
                        Gl.glVertex3f(SVtx_z.x, SVtx_z.y, SVtx_z.z);

                        //Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.x).x, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.x).y, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.x).z);
                        //Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.y).x, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.y).y, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.y).z);
                        //Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.z).x, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.z).y, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.z).z);
                    }
#else
                    //Gl.glEdgeFlag(Gl.GL_FALSE);
                    if (bStl == true)
                    {
#if false

                        //Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
                        //Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, (IntPtr)m_lstOjwAse[nIndex_Ase].Data_Get());
                        //Gl.glDrawArrays(uiType, 0, m_lstOjwAse[nIndex_Ase].Data_GetCnt());

                        //for (int i = 0; i < m_lstOjwAse[nIndex_Ase].Data_GetCnt(); i++)
                        //    Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(i).x, m_lstOjwAse[nIndex_Ase].Data_Get(i).y, m_lstOjwAse[nIndex_Ase].Data_Get(i).z);
#else
                        SVector3D_t[] pSVec = m_lstOjwAse[nIndex_Ase].Data_Get();
                        foreach (SVector3D_t SVec in pSVec) Gl.glVertex3f(SVec.x, SVec.y, SVec.z);
#endif
                        
                        //for (int i = 0; i < pSVec.Length; i += 3)
                        //{
                        //    //Gl.glBegin(uiType);
                        //    Gl.glVertex3f(pSVec[i].x, pSVec[i].y, pSVec[i].z);
                        //    Gl.glVertex3f(pSVec[i + 1].x, pSVec[i + 1].y, pSVec[i + 1].z);
                        //    Gl.glVertex3f(pSVec[i + 2].x, pSVec[i + 2].y, pSVec[i + 2].z);
                        //    //Gl.glEnd();// end drawing
                        //}
                        //Gl.glFrontFace(Gl.GL_CCW);
                    }
                    else
                    {
#if true
                        for (int i = 0; i < m_lstOjwAse[nIndex_Ase].Face_GetCnt(); i++)
                        {                            
                            Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).x).x, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).x).y, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).x).z);
                            Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).y).x, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).y).y, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).y).z);
                            Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).z).x, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).z).y, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).z).z);
                        }
#else
                        SPoint3D_t[] pSData = m_lstOjwAse[nIndex_Ase].Face_Get();
                        foreach (SPoint3D_t SPnt in pSData)
                        {
#if false
                            SVector3D_t SVtx_x = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.x);
                            SVector3D_t SVtx_y = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.y);
                            SVector3D_t SVtx_z = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.z);
                            //Gl.glBegin(uiType);
                            Gl.glVertex3f(SVtx_x.x, SVtx_x.y, SVtx_x.z);
                            Gl.glVertex3f(SVtx_y.x, SVtx_y.y, SVtx_y.z);
                            Gl.glVertex3f(SVtx_z.x, SVtx_z.y, SVtx_z.z);
                            //Gl.glEnd();// end drawing
#else                            
                            Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.x).x, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.x).y, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.x).z);
                            Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.y).x, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.y).y, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.y).z);
                            Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.z).x, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.z).y, m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.z).z);                          
#endif
                        }
#endif
                    }
                    //Gl.glEdgeFlag(Gl.GL_TRUE);
#endif
                    //CMessage.Write("[" + CConvert.IntToStr(nIndex_Ase) + "]" + CConvert.IntToStr(CTmr.GetTick()));
#else
                    //CTimer CTmr = new CTimer();
                    //CTmr.Set();
                    int nCnt = m_lstOjwAse[nIndex_Ase].Face_GetCnt();
                    for (int i = 0; i < nCnt; i++)
                    {
#if true
                        SPoint3D_t SPnt = m_lstOjwAse[nIndex_Ase].Face_Get(i);
                        SVector3D_t SVtx_x = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.x);
                        SVector3D_t SVtx_y = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.y);
                        SVector3D_t SVtx_z = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.z);
                        
                        Gl.glVertex3f(SVtx_x.x, SVtx_x.y, SVtx_x.z);
                        Gl.glVertex3f(SVtx_y.x, SVtx_y.y, SVtx_y.z);
                        Gl.glVertex3f(SVtx_z.x, SVtx_z.y, SVtx_z.z);
#else
                        Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).x).x, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).x).y, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).x).z);
                        Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).y).x, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).y).y, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).y).z);
                        Gl.glVertex3f(m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).z).x, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).z).y, m_lstOjwAse[nIndex_Ase].Data_Get(m_lstOjwAse[nIndex_Ase].Face_Get(i).z).z);
#endif
                    }
                    //CMessage.Write("[" + CConvert.IntToStr(nIndex_Ase) + "]" + CConvert.IntToStr(CTmr.GetTick()));
#endif
#endif
                    //Gl.glEdgeFlag(Gl.GL_TRUE);  
                    Gl.glEnd();// end drawing the cube
                    Gl.glPopMatrix();

                    //if (bStl == true) Gl.glFrontFace(Gl.GL_CW);
                    Gl.glFrontFace(m_nCWMode);//
                }
                #endregion File(by Name)
                #endregion OjwAse_Outside
                // Copy Data Modeling(Kor: 모델링데이타 복사)
                public void OjwFileOpen_3D_OBJ(int nCnt_Obj, int nCnt_Ase, COjwAse[] pObjAse)
                {
                    m_nCnt_Obj = nCnt_Obj;
                    m_nCnt_Ase = nCnt_Ase;
                    m_nCnt_Obj_Ase = nCnt_Obj + nCnt_Ase;

                    for (int i = 0; i < pObjAse.Length; i++) m_lstOjwAse.Add(pObjAse[i]);
                }
                // ASE 3D Modeling data #0
                #region Ojw3D_ASE_0
                public int m_nCnt_Obj_Ase = 0; // The number of loading ASE file(Kor: ASE 파일의 로딩 갯수)
                public int m_nCnt_Obj = 0;
                public int m_nCnt_Ase = 0;
                public void OjwFileOpen_3D_OBJ(String strFileName)
                {
                    //SetCursor(IDC_APPSTARTING);
                    //this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
                    try
                    {
                        const int nHide = 10;
                        FileInfo f = new FileInfo(strFileName);
                        StreamReader fs = f.OpenText();
                        float[] afPos = new float[3] { 0, 0, 0 };
                        float[] afPos2 = new float[3];
                        bool bStartPos = true;
                        int nTemp = 0;
                        while (true)
                        {
                            String str = fs.ReadLine();
                            if (str == null) break;
                            if (str.IndexOf("mtllib") == 0)
                            {
                                // New model
                                COjwAse CAse = new COjwAse();
                                CAse.Data_Clear();
                                CAse.Data_Type_Set(0);
                                m_lstOjwAse.Add(CAse);
                                //m_lstModel.Add(Ojw.CFile.GetTitle(strFileName));
                                m_lstModel.Add(Ojw.CFile.GetName(strFileName));
                                m_nCnt_Obj_Ase++;
                                m_nCnt_Obj++;
                            }

                            //if (str.IndexOf("vt") == 0) break;

                            if (str.IndexOf("v ") == 0)
                            {

                                if (nTemp == 0)
                                {
                                    str = str.Substring(2, str.Length - 2);
                                    str = str.Trim();
                                    int nPos1 = str.IndexOf(' ');
                                    afPos2[0] = CConvert.StrToFloat(str.Substring(0, nPos1));
                                    str = str.Substring((nPos1 + 1), str.Length - (nPos1 + 1));
                                    nPos1 = str.IndexOf(' ');
                                    afPos2[2] = CConvert.StrToFloat(str.Substring(0, nPos1));
                                    str = str.Substring((nPos1 + 1), str.Length - (nPos1 + 1));
                                    afPos2[1] = CConvert.StrToFloat(str);

                                    if (bStartPos == true)
                                    {
                                        bStartPos = false;
                                        afPos[0] = 0;// afPos2[0];
                                        afPos[1] = 0;//afPos2[1];
                                        afPos[2] = 0;//afPos2[2];
                                    }

                                    m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Add(afPos2[0] - afPos[0], afPos2[1] - afPos[1], afPos2[2] - afPos[2]);
                                }

                                nTemp++;
                                if (nTemp >= nHide) nTemp = 0;
                            }

                            if (str.IndexOf("f ") == 0)
                            {

                                if (nTemp == 0)
                                {
                                    int[] anPos = new int[3];
                                    str = str.Substring(2, str.Length - 2);
                                    str = str.Trim();
                                    int nPos0 = 0;// str.IndexOf(' ');
                                    int nPos1 = str.IndexOf('/') - nPos0;
                                    anPos[0] = CConvert.StrToInt(str.Substring(nPos0, nPos1));
                                    str = str.Substring((nPos1 + 1), str.Length - (nPos1 + 1));
                                    nPos1 = str.IndexOf('/');
                                    anPos[2] = CConvert.StrToInt(str.Substring(nPos0, nPos1));
                                    str = str.Substring((nPos1 + 1), str.Length - (nPos1 + 1));
                                    nPos1 = str.IndexOf(' ');
                                    anPos[1] = CConvert.StrToInt(str.Substring(nPos0, nPos1)); //CConvert.StrToFloat(str);
                                    
                                    if (bStartPos == true)
                                    {
                                        bStartPos = false;
                                        Array.Clear(afPos, 0, afPos.Length);
                                    }
                                    m_lstOjwAse[m_nCnt_Obj_Ase - 1].Face_Add(anPos[0], anPos[1], anPos[2]);
                                }

                                nTemp++;
                                if (nTemp >= nHide) nTemp = 0;
                            }
                        }
                        //SVector3D_t SVec = m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Get(0);
                        //m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Add(SVec.x, SVec.y, SVec.z);
                        fs.Close();
                    }
                    catch //(Exception e)
                    {
                        //MessageBox.Show(e.ToString());
                        m_nCnt_Obj_Ase = 0;
                        m_nCnt_Obj = 0;
                        m_nCnt_Ase = 0;
                    }
                    //SetCursor(IDC_ARROW);
                    //this.Cursor = System.Windows.Forms.Cursors.Default;
                }
                #region STL



                public bool OjwFileOpen_3D_STL(String strFileName)
                {
                    int nTmp = 0;
                    String strName = "";
                    int nTmpAll = 0;
                    try
                    {
                        string header;
                        //STLTriangle[] mesh;



                        // ojw5014 stl(solid ascii)
                        FileInfo f = new FileInfo(strFileName);
                        FileStream fs_Ascii = f.OpenRead();
                        long lHeaderSize = fs_Ascii.Length;
                        byte[] byteData = new byte[lHeaderSize];
                        
                        #region Moved by opening the file into memory(Kor: 파일을 열어서 메모리로 옮김)
                        fs_Ascii.Read(byteData, 0, (int)lHeaderSize);//byteData.Length); // for Check 11 bytes in header
                        fs_Ascii.Close();
                        #endregion Moved by opening the file into memory(Kor: 파일을 열어서 메모리로 옮김)
                        if (Encoding.ASCII.GetString(byteData, 0, 80).IndexOf("solid ascii") >= 0)
                        {
                            Ojw.CMessage.Write("solid ascii");
                            float[] afData = new float[3];
                            float[] afPos = new float[3];
                            String[] pstrSplit = Encoding.ASCII.GetString(byteData, 0, (int)lHeaderSize).Split('\n');
                            bool bFirst = true;

                            COjwAse CAse = new COjwAse();
                            CAse.Data_Clear();
                            CAse.Data_Type_Set(1);
                            m_lstOjwAse.Add(CAse);
                            m_lstModel.Add(Ojw.CFile.GetName(strFileName)); // Add a Model Name
                            m_nCnt_Obj_Ase++;
                            m_nCnt_Ase++;

                            foreach (string strLine in pstrSplit)
                            {
                                int nIndex = strLine.IndexOf("vertex");
                                if (nIndex >= 0)
                                {
                                    String[] pstrItems = strLine.Substring(nIndex + 6).Split(' ');
                                    int nPos = 0;
                                    foreach (string strItem in pstrItems)
                                    {
                                        if (strItem.Length > 0)
                                            afData[nPos++] = Ojw.CConvert.StrToFloat(strItem);
                                    }
                                    if (bFirst)
                                    {
                                        bFirst = false;
                                        afPos[0] = afData[0]; //0;//fA;
                                        afPos[2] = afData[2]; //0;//fB;
                                        afPos[1] = afData[1]; //0;//fC;
                                    }
                                    m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Add(afData[0] - afPos[0], afData[2] - afPos[2], afData[1] - afPos[1]);                                            
                                }
                            }
                            pstrSplit = null;
                        }
                        else
                        {
                            using (var fs = new BinaryReader(File.OpenRead(strFileName), Encoding.ASCII))
                            {
                                header = Encoding.ASCII.GetString(fs.ReadBytes(80));
                                var unCount = fs.ReadUInt32();

                                nTmpAll = (int)unCount;
                                strName = header;
                                //mesh = br.BaseStream.ReadUnmanagedStructRange<STLTriangle>((int)triCount);

                                COjwAse CAse = new COjwAse();
                                CAse.Data_Clear();
                                CAse.Data_Type_Set(1);
                                m_lstOjwAse.Add(CAse);
                                m_lstModel.Add(Ojw.CFile.GetName(strFileName)); // Add a Model Name
                                m_nCnt_Obj_Ase++;
                                m_nCnt_Ase++;

                                //float fA, fB, fC;
                                byte byTmp0, byTmp1;

                                float[] afPos = new float[3];
                                Array.Clear(afPos, 0, afPos.Length);

                                int _x = 0;
                                int _y = 1;
                                int _z = 2;
                                float[] afTmp = new float[3];
                                for (int i = 0; i < nTmpAll; i++)
                                {
                                    nTmp++;
                                    int nMax = 4;
                                    for (int j = 0; j < nMax; j++)
                                    {
                                        afTmp[_x] = fs.ReadSingle();
                                        afTmp[_y] = fs.ReadSingle();
                                        afTmp[_z] = fs.ReadSingle();

                                        if (j == 0) // normal
                                        {                                            
                                            //m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Add(fA - afPos[0], fB - afPos[1], fC - afPos[2]);
                                            //continue;
                                        }
                                        else
                                        {
                                            if ((i == 0) && (j == 1))// First data
                                            {
                                                afPos[_x] = afTmp[_x]; //0;// fA; //0;//fA;
                                                afPos[_y] = afTmp[_y]; //0;// fC; //0;//fB;
                                                afPos[_z] = afTmp[_z]; //0;// fB; //0;//fC;
                                            }
                                            //m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Add(fA - afPos[0], fB - afPos[1], fC - afPos[2]);
                                            m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Add(afTmp[_x] - afPos[_x], afTmp[_y] - afPos[_y], afTmp[_z] - afPos[_z]);
                                            //m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Add(fA - afPos[0], fC - afPos[2], fB - afPos[1]);
                                            //m_lstOjwAse[m_nCnt_Obj_Ase - 1].Face_Add((int)fA - (int)afPos[0], (int)fB - (int)afPos[1], (int)fC - (int)afPos[2]);
                                            //m_lstOjwAse[m_nCnt_Obj_Ase - 1].Face_Add((int)fA - (int)afPos[0], (int)fB - (int)afPos[1], (int)fC - (int)afPos[2]);
                                        }
                                    }
                                    // unused
                                    byTmp0 = fs.ReadByte();
                                    byTmp1 = fs.ReadByte();
                                }
                            }
                        }
                        return true;
                    }
                    catch(Exception e)
                    {
                        m_nCnt_Obj_Ase = 0;
                        MessageBox.Show(nTmp.ToString() + "/" + nTmpAll.ToString() + " : " + strName + "=>" + e.ToString());
                        return false;
                    }
                }
                #endregion STL
                public bool OjwFileOpen_3D_ASE(String strFileName)
                {
                    //SetCursor(IDC_APPSTARTING);
                    try
                    {
                        const int nHide = 1; // 10;
                        FileInfo f = new FileInfo(strFileName);
                        if (f.Exists == false) return false;

                        StreamReader fs = f.OpenText();
                        bool bOk = false;
                        bool bOk_Face = false;
                        float[] afPos = new float[3];
                        float[] afPos2 = new float[3];
                        int nTemp = 0;
                        bool bStart = false;
                        while (true)
                        {
                            String str = fs.ReadLine();
                            if (str == null) break;

                            if (bStart == false)
                            {
                                if (str.IndexOf("*GEOMOBJECT") >= 0) bStart = true;
                            }
                            else
                            {
                                // Do not load 2'st data
                                if (str.IndexOf("*GEOMOBJECT") >= 0) break;
                            }

                            //if (str.IndexOf("*MESH_FACE_LIST") >= 0) break;

                            if ((bStart == true) && (str.IndexOf("*TM_POS") >= 0))
                            {
#if false
                                str = str.Trim();
                                int nPos0 = str.IndexOf(' ');
                                int nPos1 = str.IndexOf('\t') - nPos0;
                                afPos[0] = CConvert.StrToFloat(str.Substring(nPos0, nPos1));
                                str = str.Substring((nPos1 + 1 + nPos0), str.Length - (nPos1 + 1 + nPos0));
                                nPos1 = str.IndexOf('\t');
                                afPos[2] = CConvert.StrToFloat(str.Substring(0, nPos1));
                                str = str.Substring((nPos1 + 1), str.Length - (nPos1 + 1));
                                afPos[1] = CConvert.StrToFloat(str);
#else
                                //StringBuilder strb = new StringBuilder(str.Trim());
                                str = str.Trim();
                                int nPos0 = str.IndexOf(' ');
                                int nPos1 = str.IndexOf('\t') - nPos0;
                                afPos[0] = CConvert.StrToFloat(str.Substring(nPos0, nPos1));
                                str = str.Substring((nPos1 + 1 + nPos0), str.Length - (nPos1 + 1 + nPos0));
                                nPos1 = str.IndexOf('\t');
                                afPos[2] = CConvert.StrToFloat(str.Substring(0, nPos1));
                                afPos[1] = CConvert.StrToFloat(str.Substring((nPos1 + 1), str.Length - (nPos1 + 1)));

#endif
                                // Add a new model
                                COjwAse CAse = new COjwAse();
                                CAse.Data_Clear();
                                CAse.Data_Type_Set(1);
                                m_lstOjwAse.Add(CAse);
                                //m_lstModel.Add(Ojw.CFile.GetTitle(strFileName)); // Add a Model Name
                                m_lstModel.Add(Ojw.CFile.GetName(strFileName));
                                m_nCnt_Obj_Ase++;
                                m_nCnt_Ase++;

                                Array.Clear(afPos, 0, afPos.Length);
                                //afPos[0] = 0;
                                //afPos[1] = 0;
                                //afPos[2] = 0;                     
                            }

                            if (str.IndexOf("*MESH_VERTEX_LIST") >= 0)
                            {
                                bOk = true;
                                continue;
                            }
                            else if (str.IndexOf("*MESH_FACE_LIST") >= 0)
                            {
                                bOk_Face = true;
                                continue;
                            }


                            if (bOk == true)
                            {
                                if (str.IndexOf("}") >= 0) bOk = false;
                                else
                                {
                                    if (nTemp == 0)
                                    {
                                        // read in the order of [x, z, y](Kor: x, z, y의 순으로 읽음)
                                        str = str.Trim();
                                        int nPos0 = str.IndexOf(' ');
                                        int nPos1 = str.IndexOf('\t') - nPos0;
                                        int nIndex = CConvert.StrToInt(str.Substring(nPos0, nPos1));
                                        str = str.Substring((nPos1 + 1 + nPos0), str.Length - (nPos1 + 1 + nPos0));
                                        nPos1 = str.IndexOf('\t');
                                        afPos2[0] = CConvert.StrToFloat(str.Substring(0, nPos1));
                                        str = str.Substring(nPos1 + 1, str.Length - (nPos1 + 1));
                                        nPos1 = str.IndexOf('\t');
                                        afPos2[2] = CConvert.StrToFloat(str.Substring(0, nPos1));
                                        str = str.Substring(nPos1 + 1, str.Length - (nPos1 + 1));
                                        afPos2[1] = CConvert.StrToFloat(str);


                                        // put the actual data.(Kor: 실제의 데이타를 넣자.)
                                        //m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Add(afPos2[0] - afPos[0], afPos2[1] - afPos[1], afPos2[2] - afPos[2]);
                                        m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Add((afPos2[0] - afPos[0]), (afPos2[1] - afPos[2]), -(afPos2[2] - afPos[1]));
                                    }

                                    nTemp++;
                                    if (nTemp >= nHide) nTemp = 0;
                                }


                                //if (str.IndexOf("*MESH_FACE_LIST") >= 0) bOk = false;
                            }
                            else if (bOk_Face == true)
                            {
                                if (str.IndexOf("}") >= 0) bOk_Face = false;
                                else
                                {
                                    if (nTemp == 0)
                                    {
                                        // read in the order of [x, z, y](Kor: x, z, y의 순으로 읽음)
                                        str = str.Trim();
                                        int nPos0 = str.IndexOf(' ');
                                        int nPos1 = str.IndexOf(":") - nPos0;
                                        int nIndex = CConvert.StrToInt(str.Substring(nPos0, nPos1));
                                        nPos0 = str.IndexOf("A:") + 2;
                                        nPos1 = str.IndexOf("B:") - nPos0;
                                        int A = CConvert.StrToInt(str.Substring(nPos0, nPos1));
                                        nPos0 = str.IndexOf("B:") + 2;
                                        nPos1 = str.IndexOf("C:") - nPos0;
                                        int B = CConvert.StrToInt(str.Substring(nPos0, nPos1));
                                        nPos0 = str.IndexOf("C:") + 2;
                                        nPos1 = str.IndexOf("AB:") - nPos0;
                                        int C = CConvert.StrToInt(str.Substring(nPos0, nPos1));
                                        //str = str.Substring((nPos1 + 1 + nPos0), str.Length - (nPos1 + 1 + nPos0));
                                        //nPos1 = str.IndexOf("C:");
                                        //int A = CConvert.StrToInt(str.Substring(0, nPos1));
                                        //str = str.Substring(nPos1 + 1, str.Length - (nPos1 + 1));
                                        //nPos1 = str.IndexOf("AB:");
                                        //int B = CConvert.StrToInt(str.Substring(0, nPos1));

                                        //nPos1 = str.IndexOf("AB:");
                                        //str = str.Substring(nPos1 + 1, str.Length - (nPos1 + 1));
                                        //int C = CConvert.StrToInt(str);

                                        // put the actual data.(Kor: 실제의 데이타를 넣자.)
                                        m_lstOjwAse[m_nCnt_Obj_Ase - 1].Face_Add(A - (int)afPos[0], B - (int)afPos[1], C - (int)afPos[2]);
                                    }

                                    nTemp++;
                                    if (nTemp >= nHide) nTemp = 0;
                                }
                            }
                        }
                        //SVector3D_t SVec = m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Get(0);
                        //m_lstOjwAse[m_nCnt_Obj_Ase - 1].Data_Add(SVec.x, SVec.y, SVec.z);
                        fs.Close();

                        return true;
                    }
                    catch
                    {
                        m_nCnt_Obj_Ase = 0;
                        return false;
                    }
                    //SetCursor(IDC_ARROW);
                }
#if false       
        public void OjwBox_Outside(bool bFill, Color color,
                            float fW, float fH, float fD,            // Size 기입
                            float fOffsetPan, float fOffsetTilt, float fOffsetSwing,   // 회전할 축
                            float fOffsetX, float fOffsetY, float fOffsetZ // 임의의 그려질 위치
                        )
        {
            m_fColor[0] = ((float)(color.R) / 255.0f);  // R
            m_fColor[1] = ((float)(color.G) / 255.0f);  // G
            m_fColor[2] = ((float)(color.B) / 255.0f);  // B
            Gl.glColor3fv(m_fColor); // Color with an array of floats
            //float fX1 = 0;
            //float fX2 = fW * m_fScale;// fX / 2.0f;
            //float fY1 = 0;// fY / 2.0f;
            //float fY2 = fH * m_fScale; ;// / 2.0f;
            //float fZ1 = 0;
            //float fZ2 = (fD * m_fScale); 
            float fX1 = -fW * m_fScale / 2.0f;
            float fX2 = fW * m_fScale / 2.0f;// fX / 2.0f;
            float fY1 = 0;// fY / 2.0f;
            float fY2 = -fH * m_fScale; // / 2.0f;
            float fZ1 = -fD * m_fScale / 2.0f;
            float fZ2 = fD * m_fScale / 2.0f;
            // x2 -> direction : right(Kor: 증분방향 : 오른쪽)
            // Y2 -> direction : Up(Kor: 증분방향 : 위쪽)
            // Z2 -> direction : The inside of the screen(Kor: 증분방향 : 화면의 안쪽)
            // Criteria screen : See picture below(Kor: 기준화면 : 밑 그림 참조)
            // 0, 0, 0 -> State in the middle of the screen as a starting point and went inside by a factor of 5
            // Kor: 0, 0, 0 -> 화면의 가운데를 시작점으로 하고 안쪽으로 5만큼 들어간 상태
            /*
                000000000
              0 0 ★  0 0
            000000000   0
            0   0   0   0
            0   000000000  
            0 0     0 0
            000000000  
            ( 중심위치 )
            */

            uint uiType = (bFill == true) ? Gl.GL_QUADS : Gl.GL_LINE_LOOP;//Gl.GL_LINE_LOOP;// Gl.GL_QUADS;//

            SVector3D_t[] aSPos = new SVector3D_t[24];
            int i = 0;
            // Front Face		
            aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Left Of The Texture and Quad
            aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Right Of The Texture and Quad
            aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Top Right Of The Texture and Quad
            aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Top Left Of The Texture and Quad

            // Back Face			
            aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Bottom Right Of The Texture and Quad
            aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Right Of The Texture and Quad
            aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Left Of The Texture and Quad
            aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Bottom Left Of The Texture and Quad

            // Top Face		
            aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Left Of The Texture and Quad
            aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Bottom Left Of The Texture and Quad
            aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Bottom Right Of The Texture and Quad
            aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Right Of The Texture and Quad

            // Bottom Face		
            aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Top Right Of The Texture and Quad
            aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Top Left Of The Texture and Quad
            aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Left Of The Texture and Quad
            aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Right Of The Texture and Quad

            // Right Face		
            aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Bottom Right Of The Texture and Quad
            aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Right Of The Texture and Quad
            aSPos[i].x = fX2; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Top Left Of The Texture and Quad
            aSPos[i].x = fX2; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Left Of The Texture and Quad
            // Left Face				
            aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ1; i++;// Bottom Left Of The Texture and Quad
            aSPos[i].x = fX1; aSPos[i].y = fY2; aSPos[i].z = fZ2; i++;// Bottom Right Of The Texture and Quad
            aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ2; i++;// Top Right Of The Texture and Quad
            aSPos[i].x = fX1; aSPos[i].y = fY1; aSPos[i].z = fZ1; i++;// Top Left Of The Texture and Quad
            int nCnt = i;

            for (i = 0; i < nCnt; i++)
            {
                // 좌표 회전
                Rotation(fOffsetTilt, fOffsetPan, fOffsetSwing, ref aSPos[i].x, ref aSPos[i].y, ref aSPos[i].z);

                // 좌표 이동
                aSPos[i].x += fOffsetX * m_fScale;
                aSPos[i].y += fOffsetY * m_fScale;
                aSPos[i].z += fOffsetZ * m_fScale;
            }
            i = 0;

            float[] afObject = new float[4] { (float)(color.R), (float)(color.G), (float)(color.B), 1.0f };

            Gl.glBegin(uiType);

            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, afObject);//물체 특성할당

            // Front Face
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;			// Bottom Left Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
            // Back Face
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
            // Top Face
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
            // Bottom Face
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
            // Right Face
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
            // Left Face
            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Left Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Bottom Right Of The Texture and Quad
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Right Of The Texture and Quad
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(aSPos[i].x, aSPos[i].y, aSPos[i].z); i++;				// Top Left Of The Texture and Quad

            Gl.glEnd();// end drawing the cube	
        }
#endif
                #endregion Ojw3D_ASE_0


                #region OjwLine / OjwLines
                public void OjwPoint(Color color, float fAlpha, float fX, float fY, float fZ,
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // Rotate(Offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ              // Translate(Offset)
                                    )
                {
                    m_fColor[0] = ((float)(color.R) / 255.0f);  // R
                    m_fColor[1] = ((float)(color.G) / 255.0f);  // G
                    m_fColor[2] = ((float)(color.B) / 255.0f);  // B
                    m_fColor[3] = fAlpha;
                    Gl.glColor4fv(m_fColor); // Color with an array of floats
                                        
                    Gl.glPushMatrix();
                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);

                    int uiType = Gl.GL_POINTS;// (bFill == true) ? Gl.GL_POLYGON : Gl.GL_LINE_LOOP;//Gl.GL_LINE_LOOP;// Gl.GL_QUADS;//

                    Gl.glBegin(uiType);

                    Gl.glVertex3f(fX, fY, fZ);

                    Gl.glEnd();

                    Gl.glPopMatrix();                    
                }
                public void OjwLine(Color color, float fAlpha, float fX0, float fY0, float fZ0, float fX1, float fY1, float fZ1,
                                    float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // Rotate(Offset)
                                    float fOffsetX, float fOffsetY, float fOffsetZ              // Translate(Offset)
                                    )
                {
                    m_fColor[0] = ((float)(color.R) / 255.0f);  // R
                    m_fColor[1] = ((float)(color.G) / 255.0f);  // G
                    m_fColor[2] = ((float)(color.B) / 255.0f);  // B
                    m_fColor[3] = fAlpha;
                    Gl.glColor4fv(m_fColor); // Color with an array of floats
                                        
                    Gl.glPushMatrix();
                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);

                    int uiType = Gl.GL_LINES;// (bFill == true) ? Gl.GL_POLYGON : Gl.GL_LINE_LOOP;//Gl.GL_LINE_LOOP;// Gl.GL_QUADS;//

                    Gl.glBegin(uiType);

                    Gl.glVertex3f(fX0, fY0, fZ0);
                    Gl.glVertex3f(fX1, fY1, fZ1);

                    Gl.glEnd();

                    Gl.glPopMatrix();                    
                }
                public void OjwPoints(Color color, float fAlpha, List<SVector3D_t> lstLines,
                                       float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // Rotate(Offset)
                                       float fOffsetX, float fOffsetY, float fOffsetZ              // Translate(Offset)
                                       )
                {
                    m_fColor[0] = ((float)(color.R) / 255.0f);  // R
                    m_fColor[1] = ((float)(color.G) / 255.0f);  // G
                    m_fColor[2] = ((float)(color.B) / 255.0f);  // B
                    m_fColor[3] = fAlpha;
                    Gl.glColor4fv(m_fColor); // Color with an array of floats

                    Gl.glPushMatrix();
                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);

                    
                    Gl.glBegin(Gl.GL_POINTS);

                    for (int i = 1; i < lstLines.Count; i++)
                    {
                        Gl.glVertex3f(lstLines[i - 1].x, lstLines[i - 1].y, lstLines[i - 1].z);
                        Gl.glVertex3f(lstLines[i].x, lstLines[i].y, lstLines[i].z);
                    }

                    Gl.glEnd();

                    Gl.glPopMatrix();
                }
                public void OjwLines(bool bFill, Color color, float fAlpha, List<SVector3D_t> lstLines,
                                        float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // Rotate(Offset)
                                        float fOffsetX, float fOffsetY, float fOffsetZ              // Translate(Offset)
                                        )
                {
                    m_fColor[0] = ((float)(color.R) / 255.0f);  // R
                    m_fColor[1] = ((float)(color.G) / 255.0f);  // G
                    m_fColor[2] = ((float)(color.B) / 255.0f);  // B
                    m_fColor[3] = fAlpha;
                    Gl.glColor4fv(m_fColor); // Color with an array of floats

                    Gl.glPushMatrix();
                    OjwTranslate(fOffsetX, fOffsetY, fOffsetZ);
                    OjwRotation(fOffsetPan, fOffsetTilt, fOffsetSwing);

                    if (bFill == true)
                    {
                        Gl.glBegin(Gl.GL_POLYGON);
                        foreach (SVector3D_t Points in lstLines) Gl.glVertex3f(Points.x, Points.y, Points.z);
                        Gl.glEnd();
                    }
                    else
                    {
                        Gl.glBegin(Gl.GL_LINES);

                        for (int i = 1; i < lstLines.Count; i++)
                        {
                            Gl.glVertex3f(lstLines[i - 1].x, lstLines[i - 1].y, lstLines[i - 1].z);
                            Gl.glVertex3f(lstLines[i].x, lstLines[i].y, lstLines[i].z);
                        }

                        Gl.glEnd();
                    }

                    Gl.glPopMatrix();
                }
                #endregion OjwLine / OjwLines








                #endregion Collection functions of OpenGL actually draw(Kor: OpenGL을 실제로 그리는 함수 모음)

                public COjwDispAll OjwDispAll = new COjwDispAll();
                public COjwDispAll OjwDispAll_User = new COjwDispAll();
                public bool CopyDispAllClassFrom(COjwDispAll OjwDispAllSource)
                {
                    if (OjwDispAllSource != null)
                    {
                        OjwDispAll.DeleteAll();
                        if (OjwDispAllSource.GetCount() > 0)
                        {
                            for (int i = 0; i < OjwDispAllSource.GetCount(); i++)
                            {
                                OjwDispAll.AddData(OjwDispAllSource.GetData(i));
                            }
                        }
                    }
                    else OjwDispAll = OjwDispAllSource;
                    return false;
                }
            
                #region User Part
                private COjwDisp m_CDisp = new COjwDisp();
                private int m_nUserIndex = 0;
                private String AxisCheck(COjwDisp CDisp)
                {
                    if (CDisp.nAxisMoveType >= 0)
                    {
                        string strTmpCaption = "(";
                        strTmpCaption += ((CDisp.nAxisMoveType == 0) ? "[P]," : "P,");
                        strTmpCaption += ((CDisp.nAxisMoveType == 1) ? "[T]," : "T,");
                        strTmpCaption += ((CDisp.nAxisMoveType == 2) ? "[S]" : "S");
                        //strTmpCaption += ") - 축번호 : Axis" + Ojw.CConvert.IntToStr(CDisp.nName) + ((CDisp.nName < 0) ? "<동작 축 설정에 이상발견(-). 확인요망>" : "");
                        strTmpCaption += ") - Axis Number : Axis" + CConvert.IntToStr(CDisp.nName) + ((CDisp.nName < 0) ? "<Found over operation axis settings(-). Check it.>" : "");
                                
                        return strTmpCaption;
                    }
                    else
                    {
                        return String.Empty;
                    }
                }
                public void Convert_CDisp_To_String()
                {
                    Convert_CDisp_To_String(OjwVirtualDisp, ref m_CHeader.strDrawModel);
                }
                public void Convert_CDisp_To_String(COjwDisp CDisp, ref COjwDesignerHeader CHeader)
                {
                    Convert_CDisp_To_String(OjwVirtualDisp, ref CHeader.strDrawModel);
                }
                public void Convert_CDisp_To_String(COjwDisp CDisp, ref String strResult) // Convert Object To StringModelData for drawing
                {
                    String strAxisCheck = AxisCheck(CDisp);
                    String strData = ClassToString(CDisp) + (((CDisp.strCaption.Length > 0) || (strAxisCheck.Length > 0)) ? " // " : "") +
                            ((CDisp.strCaption.Length > 0) ? CDisp.strCaption : "") +
                            ((strAxisCheck.Length > 0) ? strAxisCheck : "");

                    strResult += strData + "\r\n";
#if false
                    //// 초기화
                    //User_Clear();

                    ////InterPret_DrawMessage(false);

                    //int nMotorCnt = 0;
                    //String strError = String.Empty;
                    //bool bRet = CompileDesign(m_CHeader.strDrawModel, out nMotorCnt, out strError);
                    //if (bRet == true)
                    //{
                    //    Ojw.CMessage.Write_Error(strError);
                    //}
#endif
                }
                public int User_GetCnt() { return OjwDispAll_User.GetCount(); }
                public void User_Clear() { m_nUserIndex = 0; OjwDispAll_User.DeleteAll(); }
                public void User_Add_Ex(COjwDisp CDisp) { m_nUserIndex = User_GetCnt(); OjwDispAll_User.AddData(CDisp.Clone()); }
                public void User_Add() { m_nUserIndex = User_GetCnt(); OjwDispAll_User.AddData(m_CDisp.Clone()); m_CDisp.InitData(); }
                public void User_Delete(int nIndex) { m_nUserIndex = 0; OjwDispAll_User.DeleteData(nIndex); m_CDisp.InitData(); }
                public void User_Delete() { OjwDispAll_User.DeleteData(m_nUserIndex); m_nUserIndex = 0; m_CDisp.InitData(); }
                public COjwDisp User_Get(int nIndex) { m_nUserIndex = nIndex; m_CDisp = OjwDispAll_User.GetData(nIndex % OjwDispAll_User.GetCount()); return m_CDisp; }
                public int User_Get_Index() { return m_nUserIndex; }
                public bool User_Set(int nIndex, COjwDisp CDisp) { m_nUserIndex = nIndex; return OjwDispAll_User.SetData((nIndex % OjwDispAll_User.GetCount()), CDisp); }
                public bool User_Set() { return User_Set(m_nUserIndex, m_CDisp); }

                #region Set
                // Determine the internal handle, if (value < 0) then "No ID" => In other words, when determining the name of the OpenGL picking
                // Kor: 내부적 핸들을 결정, 단, 0보다 작으면(-) ID 없음. => 즉, OpenGL 의 픽킹 시 이름을 결정
                public void User_Set_AxisName(int nValue) { m_CDisp.nName = nValue; }
                public void User_Set_Color(Color cValue) { m_CDisp.cColor = cValue; }
                public void User_Set_Model(String strValue) { m_CDisp.strDispObject = strValue; CheckObjectModelFile(strValue); } // Recording the type of data to be drawn Modeling(Kor: 그려질 모델링 데이타의 종류를 기록 - 사각형, 원형, 구, ...)
                public void User_Set_Fill(bool bValue) { m_CDisp.bFilled = bValue; }    // Determining the populate the attributes of the picture(Kor: 그림의 속을 채울지를 결정)
                public void User_Set_Texture(int nValue) { m_CDisp.nTexture = nValue; }
                public void User_Set_Init(bool bValue) { m_CDisp.bInit = bValue; }

                //private List<SVertex3D_t> lstPoints = new List<SVertex3D_t>();
                public void User_Set_Points_Clear() { m_CDisp.Points.Clear(); }
                public void User_Set_Points_Add(SVector3D_t SVect) { m_CDisp.Points.Add(SVect); }
                public void User_Set_Points_Add(float fX, float fY, float fZ) { m_CDisp.Points.Add(new SVector3D_t(fX, fY, fZ)); }

                public void User_Set_Width_Or_Radius(float fValue) { m_CDisp.fWidth_Or_Radius = fValue; }
                public void User_Set_Height_Or_Depth(float fValue) { m_CDisp.fHeight_Or_Depth = fValue; }
                public void User_Set_Depth_Or_Cnt(float fValue) { m_CDisp.fDepth_Or_Cnt = fValue; }
                public void User_Set_Thickness(float fValue) { m_CDisp.fThickness = fValue; }
                public void User_Set_Gap(float fValue) { m_CDisp.fGap = fValue; }
                public void User_Set_Caption(String strValue) { m_CDisp.strCaption = strValue; }
                public void User_Set_AxisMoveType(int nValue) { m_CDisp.nAxisMoveType = nValue; }
                public void User_Set_Dir(int nValue) { m_CDisp.nDir = nValue; }
                public void User_Set_Angle(float fValue) { m_CDisp.fAngle = fValue; }
                public void User_Set_Angle_Offset(float fValue) { m_CDisp.fAngle_Offset = fValue; }
                public void User_Set_Offset_Translation(float fX, float fY, float fZ) { m_CDisp.SOffset_Trans.x = fX; m_CDisp.SOffset_Trans.y = fY; m_CDisp.SOffset_Trans.z = fZ; }
                public void User_Set_Offset_Rotation(float fPan, float fTilt, float fSwing) { m_CDisp.SOffset_Rot.pan = fPan; m_CDisp.SOffset_Rot.tilt = fTilt; m_CDisp.SOffset_Rot.swing = fSwing; }
                public void User_Set_Translation(int nIndex, float fX, float fY, float fZ) { if ((nIndex >= 0) && (nIndex < m_CDisp.afTrans.Length)) { m_CDisp.afTrans[nIndex].x = fX; m_CDisp.afTrans[nIndex].y = fY; m_CDisp.afTrans[nIndex].z = fZ; } }
                public void User_Set_Rotation(int nIndex, float fPan, float fTilt, float fSwing) { if ((nIndex >= 0) && (nIndex < m_CDisp.afRot.Length)) { m_CDisp.afRot[nIndex].pan = fPan; m_CDisp.afRot[nIndex].tilt = fTilt; m_CDisp.afRot[nIndex].swing = fSwing; } }
                public void User_Set_nPickGroup_A(int nValue) { m_CDisp.nPickGroup_A = nValue; }
                public void User_Set_nPickGroup_B(int nValue) { m_CDisp.nPickGroup_B = nValue; }
                public void User_Set_nPickGroup_C(int nValue) { m_CDisp.nPickGroup_C = nValue; }
                public void User_Set_nInverseKinematicsNumber(int nValue) { m_CDisp.nInverseKinematicsNumber = nValue; }
                public void User_Set_fScale_Serve0(float fValue) { m_CDisp.fScale_Serve0 = fValue; }
                public void User_Set_fScale_Serve1(float fValue) { m_CDisp.fScale_Serve1 = fValue; }
                public void User_Set_nMotorType(int nValue) { m_CDisp.nMotorType = nValue; }
                public void User_Set_nMotorControl_MousePoint(int nValue) { m_CDisp.nMotorControl_MousePoint = nValue; }
                public void User_Set_strPickGroup_Comment(String strValue) { m_CDisp.strPickGroup_Comment = strValue; }
                #endregion Set

                #region Get
                // Determine the internal handle, if (value < 0) then "No ID" => In other words, when determining the name of the OpenGL picking
                // Kor: 내부적 핸들을 결정, 단, 0보다 작으면(-) ID 없음. => 즉, OpenGL 의 픽킹 시 이름을 결정
                public int User_Get_AxisName() { return m_CDisp.nName; }
                public Color User_Get_Color() { return m_CDisp.cColor; }
                public String User_Get_DispObject() { return m_CDisp.strDispObject; }
                public bool User_Get_Fill() { return m_CDisp.bFilled; }
                public int User_Get_Texture() { return m_CDisp.nTexture; }
                public bool User_Get_Init() { return m_CDisp.bInit; }

                public float User_Get_Width_Or_Radius() { return m_CDisp.fWidth_Or_Radius; }
                public float User_Get_Height_Or_Depth() { return m_CDisp.fHeight_Or_Depth; }
                public float User_Get_Depth_Or_Cnt() { return m_CDisp.fDepth_Or_Cnt; }
                public float User_Get_Thickness() { return m_CDisp.fThickness; }
                public float User_Get_Gap() { return m_CDisp.fGap; }
                public String User_Get_Caption() { return m_CDisp.strCaption; }
                public int User_Get_AxisMoveType() { return m_CDisp.nAxisMoveType; }
                public int User_Get_Dir() { return m_CDisp.nDir; }
                public float User_Get_Angle() { return m_CDisp.fAngle; }
                public float User_Get_Angle_Offset() { return m_CDisp.fAngle_Offset; }
                public SVector3D_t User_Get_Offset_Translation() { return m_CDisp.SOffset_Trans; }
                public SAngle3D_t User_Get_Offset_Rotation() { return m_CDisp.SOffset_Rot; }
                public SVector3D_t User_Get_Translation(int nIndex) { return m_CDisp.afTrans[nIndex % m_CDisp.afTrans.Length]; }
                public SAngle3D_t User_Get_Rotation(int nIndex) { return m_CDisp.afRot[nIndex % m_CDisp.afRot.Length]; }
                public int User_Get_nPickGroup_A() { return m_CDisp.nPickGroup_A; }
                public int User_Get_nPickGroup_B() { return m_CDisp.nPickGroup_B; }
                public int User_Get_nPickGroup_C() { return m_CDisp.nPickGroup_C; }
                public int User_Get_nInverseKinematicsNumber() { return m_CDisp.nInverseKinematicsNumber; }
                public float User_Get_fScale_Serve0() { return m_CDisp.fScale_Serve0; }
                public float User_Get_fScale_Serve1() { return m_CDisp.fScale_Serve1; }
                public int User_Get_nMotorType() { return m_CDisp.nMotorType; }
                public int User_Get_nMotorControl_MousePoint() { return m_CDisp.nMotorControl_MousePoint; }
                public String User_Get_strPickGroup_Comment() { return m_CDisp.strPickGroup_Comment; }
                #endregion Get
                #endregion User Part

                #region Header
                #region Set
                public void SetHeader_bDisplay_Axis(bool bValue) { m_CHeader.bDisplay_Axis = bValue; }
                public void SetHeader_bDisplay_Invisible(bool bValue) { m_CHeader.bDisplay_Invisible = bValue; }
                public void SetHeader_bDisplay_Light(bool bValue) { m_CHeader.bDisplay_Light = bValue; }
                public void SetHeader_cBackColor(Color cValue) { m_CHeader.cBackColor = cValue; }
                public void SetHeader_fInitScale(float fValue) { m_CHeader.fInitScale = fValue; }
                public void SetHeader_nModelNum(int nValue) { m_CHeader.nModelNum = nValue; }
                public void SetHeader_strModelNum(String strValue) { m_CHeader.strModelNum = strValue; }
                public void SetHeader_nMotorCnt(int nValue) { m_CHeader.nMotorCnt = nValue; }
                public void SetHeader_nVersion(int nValue) { m_CHeader.nVersion = nValue; }
                public void SetHeader_nWheelCounter_2(int nValue) { m_CHeader.nWheelCounter_2 = nValue; }
                public void SetHeader_nWheelCounter_3(int nValue) { m_CHeader.nWheelCounter_3 = nValue; }
                public void SetHeader_nWheelCounter_4(int nValue) { m_CHeader.nWheelCounter_4 = nValue; }
                //public CDhParamAll[] SetHeader_pDhParamAll() { m_CHeader.pDhParamAll = bValue; } //
                //public int[] SetHeader_pnSecret() { m_CHeader.pnSecret = bValue; }
                //public int[] SetHeader_pnType() { m_CHeader.pnType = bValue; }
                //public SEncryption_t[] GetHeader_pSEncryptInverseKinematics_encryption() { m_CHeader.pSEncryptInverseKinematics_encryption = bValue; }
                //public SEncryption_t[] GetHeader_pSEncryptKinematics_encryption() { m_CHeader.pSEncryptKinematics_encryption = bValue; }
                public void SetHeader_pSMotorInfo(int nAxis, SMotorInfo_t value) { m_CHeader.pSMotorInfo[nAxis] = value; }
                //public SOjwCode_t[] GetHeader_pSOjwCode() { m_CHeader.pSOjwCode = bValue; }
                //public String[] GetHeader_pstrGroupName() { m_CHeader.pstrGroupName = bValue; }
                //public String[] GetHeader_pstrInverseKinematics() { m_CHeader.pstrInverseKinematics = bValue; }
                //public String[] GetHeader_pstrKinematics() { m_CHeader.pstrKinematics = bValue; }
                public void SetHeader_SInitAngle(SAngle3D_t SValue) { m_CHeader.SInitAngle = SValue; }
                public void SetHeader_SInitPos(SVector3D_t SValue) { m_CHeader.SInitPos = SValue; }
                public void SetHeader_strComment(String strValue) { m_CHeader.strComment = strValue; }
                public void SetHeader_strDrawModel(String strValue) { m_CHeader.strDrawModel = strValue; }
                public void AddHeader_strDrawModel(String strValue) 
                {
                    String str = CConvert.RemoveChar((String)m_CHeader.strDrawModel.Clone(), ' ');
                    //if (str.Length > 2)
                    if ((str.Length > 0) && ((str.Length - 1) == str.IndexOf('\n')))
                        m_CHeader.strDrawModel += "\r\n";

                    m_CHeader.strDrawModel += strValue;       
                }
                public void SetHeader_strModelName(String strValue) { m_CHeader.strModelName = strValue; }
                public void SetHeader_strVersion(String strValue) { m_CHeader.strVersion = strValue; }
                #endregion Set
                #region Get
                public bool GetHeader_bDisplay_Axis() { return m_CHeader.bDisplay_Axis; }
                public bool GetHeader_bDisplay_Invisible() { return m_CHeader.bDisplay_Invisible; }
                public bool GetHeader_bDisplay_Light() { return m_CHeader.bDisplay_Light; }
                public Color GetHeader_cBackColor() { return m_CHeader.cBackColor; }
                public float GetHeader_fInitScale() { return m_CHeader.fInitScale; }
                public int GetHeader_nModelNum() { return m_CHeader.nModelNum; }
                public String GetHeader_strModelNum() { return m_CHeader.strModelNum; }
                public int GetHeader_nMotorCnt() { return m_CHeader.nMotorCnt; }
                public int GetHeader_nVersion() { return m_CHeader.nVersion; }
                public int GetHeader_nWheelCounter_2() { return m_CHeader.nWheelCounter_2; }
                public int GetHeader_nWheelCounter_3() { return m_CHeader.nWheelCounter_3; }
                public int GetHeader_nWheelCounter_4() { return m_CHeader.nWheelCounter_4; }
                public CDhParamAll[] GetHeader_pDhParamAll() { return m_CHeader.pDhParamAll; } //
                public int[] GetHeader_pnSecret() { return m_CHeader.pnSecret; }
                public int[] GetHeader_pnType() { return m_CHeader.pnType; }
                public SEncryption_t[] GetHeader_pSEncryptInverseKinematics_encryption() { return m_CHeader.pSEncryptInverseKinematics_encryption; }
                public SEncryption_t[] GetHeader_pSEncryptKinematics_encryption() { return m_CHeader.pSEncryptKinematics_encryption; }
                public SMotorInfo_t[] GetHeader_pSMotorInfo() { return m_CHeader.pSMotorInfo; }
                public SMotorInfo_t GetHeader_pSMotorInfo(int nAxis) { return m_CHeader.pSMotorInfo[nAxis]; }
                public SOjwCode_t[] GetHeader_pSOjwCode() { return m_CHeader.pSOjwCode; }
                public String[] GetHeader_pstrGroupName() { return m_CHeader.pstrGroupName; }
                public String[] GetHeader_pstrInverseKinematics() { return m_CHeader.pstrInverseKinematics; }
                public String[] GetHeader_pstrKinematics() { return m_CHeader.pstrKinematics; }
                public SAngle3D_t GetHeader_SInitAngle() { return m_CHeader.SInitAngle; }
                public SVector3D_t GetHeader_SInitPos() { return m_CHeader.SInitPos; }
                public String GetHeader_strComment() { return m_CHeader.strComment; }
                public String GetHeader_strDrawModel() { return m_CHeader.strDrawModel; }
                public String GetHeader_strModelName() { return m_CHeader.strModelName; }
                public String GetHeader_strVersion() { return m_CHeader.strVersion; }
                #endregion Get
                #endregion Header

                public bool CompileDesign()
                {
                    String strError = String.Empty;
                    return CompileDesign(m_CHeader.strDrawModel, out m_CHeader.nMotorCnt, out strError);
                }
                private string m_strAseFilePath = "\\";
                public void SetAseFile_Path(String strPath) 
                { 
                    m_strAseFilePath = ("\\" + strPath.Trim('\\') + "\\");
                }//("\\" + strPath.Trim('\\') + "\\"); }
                public string GetAseFile_Path() { return m_strAseFilePath; }
                public bool CompileDesign(String strDraw, out int nMotorCount, out String strError)
                {
                    bool bRet = true;
                    strError = "";
                    nMotorCount = 0;

                    OjwDispAll.DeleteAll();
                    COjwDisp[] pCDisp;
                    TextBox txtDraw = new TextBox();
                    txtDraw.Text = strDraw;
                    TextBox_To_CodeString(txtDraw, out pCDisp);
                    txtDraw = null;
                    if (pCDisp != null)
                    {
                        bool bDuplication = false;
                        bool bInsufficient = false;
                        int nName_Max = 0;
                        int nName_Cnt = 0;
                        int[] pnName_List = new int[1];
                        for (int i = 0; i < pCDisp.Length; i++)
                        {
                            // if you never had loaded ase files, you need to load it in here
                            if (pCDisp[i].strDispObject.Length > 0)
                            {
                                if (pCDisp[i].strDispObject.IndexOf('#') < 0)
                                {
                                    if (OjwAse_GetIndex(pCDisp[i].strDispObject) < 0)
                                    {
                                        //this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
                                        String strFileName = Application.StartupPath.Trim('\\') + GetAseFile_Path() + pCDisp[i].strDispObject + ((pCDisp[i].strDispObject.IndexOf('.') < 0) ? ".ase" : "");
                                        FileInfo f = new FileInfo(strFileName);
                                        if (f.Exists == true)
                                        {
                                            if (CFile.GetExe(strFileName).ToUpper() == "ASE")
                                                OjwFileOpen_3D_ASE(strFileName);
                                            else if (CFile.GetExe(strFileName).ToUpper() == "STL")
                                                OjwFileOpen_3D_STL(strFileName);
                                        }

                                        //this.Cursor = System.Windows.Forms.Cursors.Default;
                                    }
                                }
                            }

                            if (pCDisp[i].nName >= 0)
                            {
                                bDuplication = false;
                                for (int j = 0; j < nName_Cnt; j++)
                                {
                                    if (pCDisp[i].nName == pnName_List[j]) bDuplication = true; // Find duplicates(Kor: 중복 발견)                            
                                }
                                if (bDuplication == false)
                                {
                                    Array.Resize<int>(ref pnName_List, nName_Cnt + 1);
                                    pnName_List[nName_Cnt++] = pCDisp[i].nName;
                                }
                                if (pCDisp[i].nName > nName_Max) nName_Max = pCDisp[i].nName;    // Check the number of the entire motor.(Kor: 전체 모터의 갯수를 파악)
                            }
                            OjwDispAll.AddData(pCDisp[i]);
                        }
                        if (nName_Cnt != (nName_Max + 1)) bInsufficient = true;                 // The number of motor error(The number of motor inconsistency)(Kor: 모터의 갯수가 모자람 (모터의 Max Number 와 갯수가 불일치))

                        //// 
                        nMotorCount = nName_Cnt;
                        //if ((bInsufficient == true) || (bDuplication == true))
                        if (bInsufficient == true)
                        {
                            //bRet = false; // 모터개수 맞지 않을 경우 에러
                            //strError = "[Warning] 모터의 번호 확인요망" + ((bInsufficient == true) ? ", 갯수 모자름" : "") + ((bDuplication == true) ? ", 모터 번호 중복 발견" : "");
                            //strError = "[Warning] 모터의 번호 확인요망" + ((bInsufficient == true) ? ", 갯수 모자름" : "");
                            strError = "[Warning] Check the motor ID" + ((bInsufficient == true) ? ", The number of the motor is not consistent." : "");
                        }
                        else strError = "The number of motor coincide.";
                        //else strError = "모터 갯수 이상없음";
                    }
                    pCDisp = null;

                    return bRet;
                }
                //}
            #endregion CsGL Class / The actual drawing and initialization functions are all based here.(Kor: CsGL Class / 실제 그리기 및 초기화(즉, Main) 함수 모음)

            public void SetFunctionNumber(int nNum)
            {
                //m_CHeader.nDefaultFunctionNumber = m_CPropAll.nMain_DefaultFunctionNum = m_nFunctionNumber = nNum;
                m_CHeader.nDefaultFunctionNumber = m_CPropAll.nMain_DefaultFunctionNum = nNum;
            }
            public int GetFunctionNumber() { return m_CHeader.nDefaultFunctionNumber; }// m_nFunctionNumber; }

            public class COjwFileList
            {
                private int m_nCnt;
                private String[] m_pstrFileList;
                private int[] m_pnModelNumber;
                private String[] m_pstrModelName;
                #region Init/Resize
                public COjwFileList()
                {
                    Init(0);
                }
                public COjwFileList(int nCnt)
                {
                    Init(nCnt);
                }
                public void Init()
                {
                    Init(0);
                }
                public void Init(int nCnt)
                {

                    m_nCnt = nCnt;
                    if (nCnt == 0)
                    {
                        m_pstrFileList = null;
                        m_pnModelNumber = null;
                        m_pstrModelName = null;
                    }
                    else
                    {
                        m_pstrFileList = new String[nCnt];
                        m_pnModelNumber = new int[nCnt];
                        m_pstrModelName = new String[nCnt];
                    }
                }
                private void Resize(int nCnt)
                {
                    if (nCnt > 0)
                    {
                        if (nCnt != m_nCnt)
                        {
                            Array.Resize<String>(ref m_pstrFileList, nCnt);
                            Array.Resize<int>(ref m_pnModelNumber, nCnt);
                            Array.Resize<String>(ref m_pstrModelName, nCnt);
                            m_nCnt = nCnt;
                        }
                    }
                }
                #endregion Init/Resize
                #region use
                public bool Add(FileInfo fileInfo)
                {
                    FileStream fs = null;
                    int nCnt = m_nCnt;
                    try
                    {
                        int i = m_nCnt++;
                        Array.Resize<String>(ref m_pstrFileList, m_nCnt);
                        Array.Resize<int>(ref m_pnModelNumber, m_nCnt);
                        Array.Resize<String>(ref m_pstrModelName, m_nCnt);
                        m_pstrFileList[i] = fileInfo.FullName;


                        FileInfo f = new FileInfo(fileInfo.FullName);
                        fs = f.OpenRead();

                        byte[] byteData = new byte[fs.Length];

                        #region Moved by opening the file into memory(Kor: 파일을 열어서 메모리로 옮김)
                        fs.Read(byteData, 0, byteData.Length);
                        fs.Close();
                        #endregion Moved by opening the file into memory(Kor: 파일을 열어서 메모리로 옮김)

                        int nPos = 6;   // for header 'HMT1.1' (Kor: 앞의 6개는 'HMT1.1' 에 할당)

                        #region Model type ( 2 Bytes )
                        m_pnModelNumber[i] = (int)(short)(BitConverter.ToInt16(byteData, nPos));
                        nPos += 2;
                        #endregion Model type  ( 2 Bytes )

                        #region Title ( 21 Bytes )
                        m_pstrModelName[i] = Encoding.Default.GetString(byteData, nPos, 21);
                        //nPos += 21;
                        #endregion Title ( 21 Bytes )

                        byteData = null;

                        return true;
                    }
                    catch
                    {
                        m_nCnt = nCnt;
                        if (fs != null) fs.Close();
                        return false;
                    }
                }
                public int Get_Size()
                {
                    return m_nCnt;
                }
                public bool IsValid(int nIndex)
                {
                    if ((nIndex >= 0) && (nIndex < m_nCnt))
                    {
                        if (m_pstrFileList != null)
                        {
                            if (m_pstrFileList.Length >= m_nCnt)
                            {
                                if (m_pnModelNumber != null)
                                {
                                    if (m_pnModelNumber.Length >= m_nCnt)
                                    {
                                        if (m_pstrModelName != null)
                                        {
                                            if (m_pstrModelName.Length >= m_nCnt) return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return false;
                }

                #region Find information as stored procedure(Kor: 저장된 순서대로 정보찾기)
                public String Get_FileName(int nIndex)
                {
                    if (IsValid(nIndex) == true)
                    {
                        return m_pstrFileList[nIndex];
                    }
                    return null;
                }
                public int Get_Model_Number(int nIndex)
                {
                    if (IsValid(nIndex) == true)
                    {
                        return m_pnModelNumber[nIndex];
                    }
                    return -1;
                }
                public String Get_Model_Name(int nIndex)
                {
                    if (IsValid(nIndex) == true)
                    {
                        return m_pstrModelName[nIndex];
                    }
                    return null;
                }
                #endregion Find information as stored procedure(Kor: 저장된 순서대로 정보찾기)

                #region Find information to Model Number(Kor: Model Number로 정보찾기)
                public String Get_FileName_by_Model_Number(int nModelNumber)
                {
                    int nCnt = Get_Size();
                    int nNum;
                    for (int i = 0; i < nCnt; i++)
                    {
                        nNum = Get_Model_Number(i);
                        if (nNum == nModelNumber)
                        {
                            return Get_FileName(i);
                        }
                    }
                    return null;
                }
                public String Get_Model_Name_by_Model_Number(int nModelNumber)
                {
                    int nCnt = Get_Size();
                    int nNum;
                    for (int i = 0; i < nCnt; i++)
                    {
                        nNum = Get_Model_Number(i);
                        if (nNum == nModelNumber)
                        {
                            return Get_Model_Name(i);
                        }
                    }
                    return null;
                }
                #endregion Find information to Model Number(Kor: Model Number로 정보찾기)

                #region Find information to Model Name(Kor: Model Name으로 정보찾기)
                public String Get_FileName_by_Model_Name(String strModelName)
                {
                    int nCnt = Get_Size();
                    String strTmp;
                    for (int i = 0; i < nCnt; i++)
                    {
                        strTmp = Get_Model_Name(i);
                        if (strTmp == strModelName)
                        {
                            return Get_FileName(i);
                        }
                    }
                    return null;
                }
                public int Get_Model_Number_by_Model_Name(String strModelName)
                {
                    int nCnt = Get_Size();
                    String strTmp;
                    for (int i = 0; i < nCnt; i++)
                    {
                        strTmp = Get_Model_Name(i);
                        if (strTmp == strModelName)
                        {
                            return Get_Model_Number(i);
                        }
                    }
                    return -1;
                }
                #endregion Find information to Model Name(Kor: Model Name으로 정보찾기)

                #endregion use
                //public void Add(
            }

            #region Designer class(COjwDesignerHeader)

                #region Version - Version Designer History header file(Kor: 디자이너 헤더 파일의 버전 기록)
                public static String _STR_EXT = "ojw"; // OpenJigWare File

                //public static String _STR_EXT_VERSION = "01.00.00";
                public static String _STR_EXT_VERSION = "01.01.00";
                #endregion Version - Version Designer History header file(Kor: 디자이너 헤더 파일의 버전 기록)

                public String m_strVersion = "";
                public static int _CNT_FILEOPEN = 0;
                public int Get_File_List(String strPath, out COjwFileList CFileList)
                {
                    int nRet = 0;
                    CFileList = null;
                    try
                    {
                        CFileList = new COjwFileList();
                        DirectoryInfo dirInfo = new DirectoryInfo(strPath);
                        if (dirInfo.Exists == true)
                        {
                            //int nCnt = 0;
                            //int i = 0;
                            CFileList.Init();
                            foreach (FileInfo fileInfo in dirInfo.GetFiles("*.ojw"))
                            {
                                CFileList.Add(fileInfo);
                            }
                            nRet = CFileList.Get_Size();
                        }
                        return nRet;
                    }
                    catch
                    {
                        return nRet;
                    }
                }
                bool m_bFileOpening = false;
                public bool FileOpen(String strFileName)
                {
                    bool bRet = FileOpen(strFileName, out m_CHeader);
                    if (bRet == true)
                    {
                        int nMotorCount = 0;
                        String strError = "";

                        #region Compile
                        // Forward
                        CheckForward();
                        // Inverse
                        CheckInverse();
                        #endregion Compile

                        return CompileDesign(m_CHeader.strDrawModel, out nMotorCount, out strError);

                    }
                    return false;
                }                
                public void CheckForward()
                {
                    int nPos = 0;
                    foreach (String strItem in m_CHeader.pstrKinematics)
                    {
                        if (strItem != null)
                        {
                            if (strItem.Length > 0)
                                CKinematics.CForward.MakeDhParam(strItem, out m_CHeader.pDhParamAll[nPos]);
                        }
                        nPos++;
                    }
                }
                public void CheckInverse()
                {
                    int nPos = 0;
                    int nCnt_InverseKinematics = 0;
                    foreach (String strItem in m_CHeader.pstrInverseKinematics)
                    {
                        if (strItem != null)
                        {
                            if (strItem.Length > 0)
                                CKinematics.CInverse.Compile(strItem, out m_CHeader.pSOjwCode[nPos]);
                        }
                        if (m_CHeader.pSOjwCode[nPos].nMotor_Max > 0) nCnt_InverseKinematics++;
                        nPos++;
                    }
                    m_nCnt_InverseKinematics = nCnt_InverseKinematics;
                }
                private int m_nCnt_InverseKinematics = 0;
                public void FileSave(String strFileName, COjwDesignerHeader CHeader)
                {
                    //int nVersion = 
                    if (strFileName == "")
                    {
                        Ojw.CMessage.Write_Error("File Save Error - Null FileName");
                        return;
                    }

                    FileInfo f = new FileInfo(strFileName);
                    FileStream fs = f.Create();

                    try
                    {
                        int i, j;
                        byte[] byteData, byteData2;

                        String strVerstion = _STR_EXT.ToUpper() + _STR_EXT_VERSION.ToUpper(); // save a new released version(Kor: 최신버전 저장)
                        // Empty the stream buffer.(Kor: 스트림 버퍼를 비운다.)
                        fs.Flush();

                        #region Set a identification code(Kor: 식별코드 부여) // OJW1.0.0 ( 11 Bytes )
                        if ((_STR_EXT.Length != 3) || (_STR_EXT_VERSION.Length != 8))
                        {
                            Ojw.CMessage.Write_Error("Incorrect File Version");
                            fs.Close();
                            return;
                        }
                        for (i = 0; i < (_STR_EXT.Length + _STR_EXT_VERSION.Length); i++) fs.WriteByte((byte)(strVerstion[i]));

                        #endregion Set a identification code(Kor: 식별코드 부여) // OJW1.0.0 ( 11 Bytes )

                        #region From Version 1.1.0(_STR_EXT_VERSION = "01.01.00")( 4 Bytes )
                        byteData = BitConverter.GetBytes((int)CHeader.nDefaultFunctionNumber);
                        fs.Write(byteData, 0, 4);
                        byteData = null;
                        #endregion From Version 1.1.0(_STR_EXT_VERSION = "01.01.00")( 4 Bytes )

                        #region Model number ( 2 Bytes )
                        byteData = BitConverter.GetBytes((short)CHeader.nModelNum);
                        fs.Write(byteData, 0, 2);
                        byteData = null;
                        #endregion Model number ( 2 Bytes )

                        #region Title  ( 21 Bytes )
                        // Name
                        //byteData = Encoding.Default.GetBytes(m_CHeader.strModelName);
                        byteData = Encoding.Default.GetBytes(CHeader.strModelName);
                        for (i = 0; i < 20; i++)
                        {
                            if (i < byteData.Length) fs.WriteByte(byteData[i]);
                            else fs.WriteByte(0);
                        }
                        // Additional terminating null character(Kor: 널 종료문자 추가)
                        fs.WriteByte(0);
                        byteData = null;
                        #endregion Title ( 21 Bytes )

                        #region BackColor - Screen Background Color ( 4 Bytes )
                        byteData = BitConverter.GetBytes((int)GetBackColor().ToArgb());
                        fs.Write(byteData, 0, 4);
                        byteData = null;
                        #endregion BackColor - Screen Background Color ( 4 Bytes )

                        #region Number of motors ( 2 Bytes )
                        byteData = BitConverter.GetBytes((short)CHeader.nMotorCnt);
                        fs.Write(byteData, 0, 2);
                        byteData = null;
                        #endregion Number of motors ( 2 Bytes )

                        #region The initial angle
                        #region The initial angle - Pan ( 4 Bytes )
                        byteData = BitConverter.GetBytes((float)GetPan());
                        fs.Write(byteData, 0, 4);
                        byteData = null;
                        #endregion The initial angle - Pan ( 4 Bytes )

                        #region The initial angle - Tilt ( 4 Bytes )
                        byteData = BitConverter.GetBytes((float)GetTilt());
                        fs.Write(byteData, 0, 4);
                        byteData = null;
                        #endregion The initial angle - Tilt ( 4 Bytes )

                        #region The initial angle - Swing ( 4 Bytes )
                        byteData = BitConverter.GetBytes((float)GetSwing());
                        fs.Write(byteData, 0, 4);
                        byteData = null;
                        #endregion The initial angle - Swing ( 4 Bytes )
                        #endregion The initial angle

                        #region The initial position
                        float fX, fY, fZ;
                        GetPos_Display(out fX, out fY, out fZ);
                        #region The initial position - x ( 4 Bytes )
                        byteData = BitConverter.GetBytes(fX);
                        fs.Write(byteData, 0, 4);
                        byteData = null;
                        #endregion The initial position - x ( 4 Bytes )

                        #region The initial position - y ( 4 Bytes )
                        byteData = BitConverter.GetBytes(fY);
                        fs.Write(byteData, 0, 4);
                        byteData = null;
                        #endregion The initial position - y ( 4 Bytes )

                        #region The initial position - z ( 4 Bytes )
                        byteData = BitConverter.GetBytes(fZ);
                        fs.Write(byteData, 0, 4);
                        byteData = null;
                        #endregion The initial position - z ( 4 Bytes )
                        #endregion The initial position

                        #region The initial Scale - 100% = 1.0 ( 4 Bytes )
                        byteData = BitConverter.GetBytes((float)GetScale());
                        fs.Write(byteData, 0, 4);
                        byteData = null;
                        #endregion The initial Scale - 100% = 1.0 ( 4 Bytes )


                        #region 2 Wheel Counter ( 1 Bytes )

                        fs.WriteByte((byte)(CHeader.nWheelCounter_2 & 0xff));
                        #endregion 2 Wheel Counter ( 1 Bytes )

                        #region 3 Wheel Counter ( 1 Bytes )
                        fs.WriteByte((byte)(CHeader.nWheelCounter_3 & 0xff));
                        #endregion 3 Wheel Counter ( 1 Bytes )

                        #region 4 Wheel Counter ( 1 Bytes )
                        fs.WriteByte((byte)(CHeader.nWheelCounter_4 & 0xff));
                        #endregion 4 Wheel Counter ( 1 Bytes )

                        for (i = 0; i < 256; i++)
                        {
                            #region Axis Info

                            #region Motor ID ( 2 Bytes )
                            byteData = BitConverter.GetBytes((short)(m_CHeader.pSMotorInfo[i].nMotorID));
                            fs.Write(byteData, 0, 2);
                            byteData = null;
                            #endregion Motor ID ( 2 Bytes )

                            #region [Motor Direction] Direction - 0 - Forward, 1 - Inverse ( 2 Bytes )
                            byteData = BitConverter.GetBytes((short)(m_CHeader.pSMotorInfo[i].nMotorDir));
                            fs.Write(byteData, 0, 2);
                            byteData = null;
                            #endregion [Motor Direction] Direction - 0 - Forward, 1 - Inverse ( 2 Bytes )

                            #region Max Angle(+) ( 4 Bytes )
                            byteData = BitConverter.GetBytes(m_CHeader.pSMotorInfo[i].fLimit_Up);
                            fs.Write(byteData, 0, 4);
                            byteData = null;
                            #endregion Max Angle(+) ( 4 Bytes )
                            #region Min Angle(-) ( 4 Bytes )
                            byteData = BitConverter.GetBytes(m_CHeader.pSMotorInfo[i].fLimit_Down);
                            fs.Write(byteData, 0, 4);
                            byteData = null;
                            #endregion Min Angle(-) ( 4 Bytes )

                            #region Center Position - Evd ( 2 Bytes )
                            byteData = BitConverter.GetBytes((int)m_CHeader.pSMotorInfo[i].nCenter_Evd);
                            fs.Write(byteData, 0, 2);
                            byteData = null;
                            #endregion Center Position - Evd ( 2 Bytes )

                            #region Mech Move - maximum Evd value( 2 Bytes )
                            byteData = BitConverter.GetBytes((int)m_CHeader.pSMotorInfo[i].nMechMove);
                            fs.Write(byteData, 0, 2);
                            byteData = null;
                            #endregion Mech Move - maximum Evd value ( 2 Bytes )
                            #region Mech Angle - Angle of Mech Mov [ The maximum pulse corresponding to the angle value(Kor: 최대치 펄스에 해당하는 각도값 (분주각))]( 4 Bytes )
                            byteData = BitConverter.GetBytes(m_CHeader.pSMotorInfo[i].fMechAngle);
                            fs.Write(byteData, 0, 4);
                            byteData = null;
                            #endregion Mech Angle - Angle of Mech Mov [ The maximum pulse corresponding to the angle value(Kor: 최대치 펄스에 해당하는 각도값 (분주각))]( 4 Bytes )

                            #region Init Angle - Used for the initial position of the data in an arbitrary position( 4 Bytes )(Kor: 데이타의 초기자세를 임의의 자세로 하기 위해 사용( 4 Bytes ))
                            byteData = BitConverter.GetBytes(m_CHeader.pSMotorInfo[i].fInitAngle);
                            fs.Write(byteData, 0, 4);
                            byteData = null;
                            byteData = BitConverter.GetBytes(m_CHeader.pSMotorInfo[i].fInitAngle2);
                            fs.Write(byteData, 0, 4);
                            byteData = null;
                            #endregion Init Angle - Used for the initial position of the data in an arbitrary position( 4 Bytes )(Kor: 데이타의 초기자세를 임의의 자세로 하기 위해 사용( 4 Bytes ))

                            #region Interference axis number(Kor: 간섭 축 번호) ( 2 Bytes )
                            byteData = BitConverter.GetBytes((short)(m_CHeader.pSMotorInfo[i].nInterference_Axis));
                            fs.Write(byteData, 0, 2);
                            byteData = null;
                            #endregion Interference axis number(Kor: 간섭 축 번호) ( 2 Bytes )
                            #region Axis Width ( 4 Bytes )
                            byteData = BitConverter.GetBytes(m_CHeader.pSMotorInfo[i].fW);
                            fs.Write(byteData, 0, 4);
                            byteData = null;
                            #endregion Axis Width ( 4 Bytes )
                            #region Interference axis Width ( 4 Bytes )
                            byteData = BitConverter.GetBytes(m_CHeader.pSMotorInfo[i].fInterference_W);
                            fs.Write(byteData, 0, 4);
                            byteData = null;
                            #endregion Interference axis Width ( 4 Bytes )

                            #region Axis Side(Right) ( 4 Bytes )
                            byteData = BitConverter.GetBytes(m_CHeader.pSMotorInfo[i].fPos_Right);
                            fs.Write(byteData, 0, 4);
                            byteData = null;
                            #endregion Axis Side(Right) ( 4 Bytes )
                            #region Axis Side(Left) ( 4 Bytes )
                            byteData = BitConverter.GetBytes(m_CHeader.pSMotorInfo[i].fPos_Left);
                            fs.Write(byteData, 0, 4);
                            byteData = null;
                            #endregion Axis Side(Left) ( 4 Bytes )
                            #region Interference axis Front ( 4 Bytes )
                            byteData = BitConverter.GetBytes(m_CHeader.pSMotorInfo[i].fInterference_Pos_Front);
                            fs.Write(byteData, 0, 4);
                            byteData = null;
                            #endregion Interference axis Front ( 4 Bytes )
                            #region Interference axis Rear ( 4 Bytes )
                            byteData = BitConverter.GetBytes(m_CHeader.pSMotorInfo[i].fInterference_Pos_Rear);
                            fs.Write(byteData, 0, 4);
                            byteData = null;
                            #endregion Interference axis Rear ( 4 Bytes )

                            #region NickName  ( 32 Bytes )
                            if (m_CHeader.pSMotorInfo[i].strNickName != null) byteData = Encoding.Default.GetBytes(m_CHeader.pSMotorInfo[i].strNickName);
                            else
                            {
                                byteData = new byte[32];
                                Array.Clear(byteData, 0, 32);
                            }
                            for (j = 0; j < 31; j++)
                            {
                                if (j < byteData.Length) fs.WriteByte(byteData[j]);
                                else fs.WriteByte(0);
                            }
                            // Additional terminating null character(Kor: 널 종료문자 추가)
                            fs.WriteByte(0);
                            byteData = null;
                            #endregion NickName  ( 32 Bytes )

                            #region Group Number ( 2 Bytes )
                            byteData = BitConverter.GetBytes((int)m_CHeader.pSMotorInfo[i].nGroupNumber);
                            fs.Write(byteData, 0, 2);
                            byteData = null;
                            #endregion Group Number ( 2 Bytes )

                            #region Mirror axis number ( 2 Bytes )
                            byteData = BitConverter.GetBytes((short)(m_CHeader.pSMotorInfo[i].nAxis_Mirror));
                            fs.Write(byteData, 0, 2);
                            byteData = null;
                            #endregion Mirror axis number ( 2 Bytes )

                            #region motor control type ( 2 Bytes )
                            byteData = BitConverter.GetBytes((short)(m_CHeader.pSMotorInfo[i].nMotorControlType));
                            fs.Write(byteData, 0, 2);
                            byteData = null;
                            #endregion motor control type ( 2 Bytes )

                            #endregion Axis Info
                        }
                        #region Set the separation code(Kor: 구분 코드 부여) // HE - Header End ( 2 Bytes )
                        fs.WriteByte((byte)('H'));  // Header 
                        fs.WriteByte((byte)('E'));  // End
                        #endregion Set the separation code(Kor: 구분 코드 부여) // HE - Header End ( 2 Bytes )

                        for (i = 0; i < 512; i++)
                        {
                            #region V1.4
                            
                            #region Secret
                            fs.WriteByte((byte)(m_CHeader.pnSecret[i] & 0xff));
                            #endregion Secret
                            #region Type - Wheel or not
                            fs.WriteByte((byte)(m_CHeader.pnType[i] & 0xff));
                            #endregion Type - Wheel or not
                            
                            #endregion V1.4

                            #region Char - GroupName
                            byteData = Encoding.Default.GetBytes(m_CHeader.pstrGroupName[i]); // The name of the group that are listed in the string(Kor: 스트링으로 적혀있는 해당 그룹의 이름)
                            #region Char Size - Forward ( 2 Bytes )
                            byteData2 = BitConverter.GetBytes((short)(byteData.Length));
                            fs.Write(byteData2, 0, 2);
                            byteData2 = null;
                            #endregion Char Size - Forward ( 2 Bytes )
                            for (j = 0; j < byteData.Length; j++) fs.WriteByte(byteData[j]);
                            byteData = null;
                            #endregion Char - GroupName

                            #region Kinematics/InverseKinematics String
                            #region Char - Forward
                            
                            bool bSecret = (m_CHeader.pnSecret[i] > 0) ? true : false;
                            if (bSecret == false) byteData = Encoding.Default.GetBytes(m_CHeader.pstrKinematics[i]);
                            else
                            {
                                byteData = Encoding.Default.GetBytes(m_CHeader.pstrKinematics[i]);
                                byteData = CEncryption.Encryption(bSecret, byteData);
                            }             
                           
                            #region Char Size - Forward ( 2 Bytes )
                            byteData2 = BitConverter.GetBytes((short)(byteData.Length));
                            fs.Write(byteData2, 0, 2);
                            byteData2 = null;
                            #endregion Char Size - Forward ( 2 Bytes )
                            for (j = 0; j < byteData.Length; j++) fs.WriteByte(byteData[j]);
                            byteData = null;
                            #endregion Char - Forward

                            #region Char - Inverse
                            if (bSecret == false) byteData = Encoding.Default.GetBytes(m_CHeader.pstrInverseKinematics[i]);
                            else
                            {
                                byteData = Encoding.Default.GetBytes(m_CHeader.pstrInverseKinematics[i]);
                                byteData = CEncryption.Encryption(bSecret, byteData);
                            }
                            #region Char Size - Inverse ( 2 Bytes )
                            byteData2 = BitConverter.GetBytes((short)(byteData.Length));
                            fs.Write(byteData2, 0, 2);
                            byteData2 = null;
                            #endregion Char Size - Inverse ( 2 Bytes )
                            for (j = 0; j < byteData.Length; j++) fs.WriteByte(byteData[j]);
                            byteData = null;
                            #endregion Char - Inverse
                            #endregion Kinematics/InverseKinematics String
                        }
                        #region Set the separation code(Kor: 구분 코드 부여) // KE - Kinematics End ( 2 Bytes )
                        fs.WriteByte((byte)('K'));  // Kinematics 
                        fs.WriteByte((byte)('E'));  // End
                        #endregion Set the separation code(Kor: 구분 코드 부여) // HE - Kinematics End ( 2 Bytes )

                        #region actual design string(Kor: 실 디자인 스트링)

                        #region Char - actual design string(Kor: 실 디자인 스트링)
                        byteData = Encoding.Default.GetBytes(CHeader.strDrawModel);
                        #region Char Size - actual design string(Kor: 실 디자인 스트링) ( 2 Bytes )
                        byteData2 = BitConverter.GetBytes((ushort)(byteData.Length));
                        fs.Write(byteData2, 0, 2);
                        byteData2 = null;
                        #endregion Char Size - actual design string(Kor: 실 디자인 스트링) ( 2 Bytes )

                        for (j = 0; j < byteData.Length; j++) fs.WriteByte(byteData[j]);
                        byteData = null;
                        #endregion Char - actual design string(Kor: 실 디자인 스트링)
                        #endregion actual design string(Kor: 실 디자인 스트링)

                        #region Comment String
                        #region Char - Comment
                        if (CHeader.strComment == null)
                        {
                            #region Char Size - Comment ( 2 Bytes )
                            byteData2 = BitConverter.GetBytes((short)(0));
                            fs.Write(byteData2, 0, 2);
                            byteData2 = null;
                            #endregion Char Size - Comment ( 2 Bytes )
                        }
                        else
                        {
                            byteData = Encoding.Default.GetBytes(CHeader.strComment);
                            #region Char Size - Comment ( 2 Bytes )
                            byteData2 = BitConverter.GetBytes((short)(byteData.Length));
                            fs.Write(byteData2, 0, 2);
                            byteData2 = null;
                            #endregion Char Size - Comment ( 2 Bytes )
                            for (j = 0; j < CHeader.strComment.Length; j++) fs.WriteByte(byteData[j]);
                            byteData = null;
                        }
                        #endregion Char - Comment
                        #endregion Comment String

                        #region Set the separation code(Kor: 구분 코드 부여) // FE - File End ( 2 Bytes )
                        fs.WriteByte((byte)('F'));  // File 
                        fs.WriteByte((byte)('E'));  // End
                        #endregion Set the separation code(Kor: 구분 코드 부여) // HE - File End ( 2 Bytes )

                        fs.Close();

                        //if (m_bAutoSaved == false) Modify(false);

                        byteData = null;
                        byteData2 = null;

                        //////////////////////////////
                        // Memory available for Dh(Kor: Dh 를 위한 메모리 확보)
                        m_CHeader.pDhParamAll = new CDhParamAll[512];
                        int nCnt_InverseKinematics = 0;
                        for (i = 0; i < 512; i++)
                        {
                            if (m_CHeader.pstrKinematics[i] != null)
                            {
                                if (m_CHeader.pstrKinematics[i].Length > 0)
                                {
                                    // Forward
                                    Ojw.CKinematics.CForward.MakeDhParam(m_CHeader.pstrKinematics[i], out m_CHeader.pDhParamAll[i]);
                                    // Inverse
                                    bool bError = Ojw.CKinematics.CInverse.Compile(m_CHeader.pstrInverseKinematics[i], out m_CHeader.pSOjwCode[i]);

                                    /////////////
                                    if (m_CHeader.pSOjwCode[i].nMotor_Max > 0) nCnt_InverseKinematics++;
                                }
                            }
                        }
                    }
                    catch
                    {
                        //Message("File save error");
                        fs.Close();
                    }
                }
                public bool FileOpen(String strFileName, out COjwDesignerHeader CHeader)
                {
                    m_bFileOpening = true;
                    _CNT_FILEOPEN++;
                    bool bFileOpened = true;
                    CHeader = null;
                    m_strVersion = "";
                    try
                    {
                        int i;//, j;

                        FileInfo f = new FileInfo(strFileName);
                        FileStream fs = f.OpenRead();

                        byte[] byteData = new byte[fs.Length];
                        string strFileName2 = "";
                        string strData = "";

                        #region Moved by opening the file into memory(Kor: 파일을 열어서 메모리로 옮김)
                        fs.Read(byteData, 0, byteData.Length);
                        strFileName2 = f.Name;
                        fs.Close();
                        #endregion Moved by opening the file into memory(Kor: 파일을 열어서 메모리로 옮김)

                        #region separation code - OJW1.0 ( (_STR_EXT.Length + _STR_EXT_VERSION.Length) Bytes )
                        String strTmp = String.Empty;
                        for (i = 0; i < (_STR_EXT.Length + _STR_EXT_VERSION.Length); i++) strTmp += ((char)byteData[i]).ToString().ToUpper();
                        //strTmp = strTmp.ToUpper();

                        strData = (_STR_EXT.ToUpper() + _STR_EXT_VERSION.ToUpper());
                        #endregion separation code - OJW1.0 ( (_STR_EXT.Length + _STR_EXT_VERSION.Length) Bytes )
                        COjwDesignerHeader CDesignHeder = new COjwDesignerHeader();
                        #region OjwVersion
#if _DHF_FILE
                        if ((strTmp[0] == 'D') & (strTmp[1] == 'H') & (strTmp[2] == 'F'))
                        {
                            m_strVersion += (char)byteData[3];
                            m_strVersion += (char)byteData[4];
                            m_strVersion += (char)byteData[5];

                            // In version 1.1, there is a second position(Kor: 1.1 버전에서는 2번째 자세가 없다.)
                            bool bNoSecondPos = false;
                            bool bNoAxisMirror = false;
                            int nVersion = 11;

                            if (strTmp.Substring(0, 6) == "DHF1.1") nVersion = 11;
                            else if (strTmp.Substring(0, 6) == "DHF1.2") nVersion = 12;
                            else if (strTmp.Substring(0, 6) == "DHF1.3") nVersion = 13;
                            else if (strTmp.Substring(0, 6) == "DHF1.4") nVersion = 14;

                            CDesignHeder.strVersion = strTmp.Substring(0, 6);
                            CDesignHeder.nVersion = nVersion;

                            if (nVersion < 12)
                            {
                                bNoSecondPos = true;
                                bNoAxisMirror = true;
                            }
                            else if (nVersion == 12)
                            {
                                bNoAxisMirror = true;
                            }
                            //else
                            //{
                            //}

                            //if (bMessage == true) OjwMessage("[" + strData + " Binary File Data(" + strTmp + ")] Opened");
                            int nPos = 6;   // 'HMT1.1'

                            #region From Version 1.1.0(_STR_EXT_VERSION = "01.01.00")( 4 Bytes )
                            CDesignHeder.nDefaultFunctionNumber = -1;// no use it in DHF
                            #endregion From Version 1.1.0(_STR_EXT_VERSION = "01.01.00")( 4 Bytes )

                            #region Model type ( 2 Bytes )
                            CDesignHeder.nModelNum = (int)(short)(BitConverter.ToInt16(byteData, nPos));
                            nPos += 2;
                            #endregion Model type ( 2 Bytes )

                            #region Title ( 21 Bytes )
                            CDesignHeder.strModelName = Encoding.Default.GetString(byteData, nPos, 21);
                            nPos += 21;
                            #endregion Title ( 21 Bytes )

                            #region BackColor - Background color ( 4 Bytes)
                            CDesignHeder.cBackColor = Color.FromArgb(BitConverter.ToInt32(byteData, nPos));
                            nPos += 4;
                            #endregion BackColor - Background color ( 4 Bytes)

                            #region Number of the motor ( 2 Bytes )
                            CDesignHeder.nMotorCnt = (int)(short)(BitConverter.ToInt16(byteData, nPos));
                            nPos += 2;
                            #endregion Number of the motor ( 2 Bytes )
                            #region initial angle
                            #region initial angle - Pan ( 4 Bytes )
                            CDesignHeder.SInitAngle.pan = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion initial angle - Pan ( 4 Bytes )
                            #region initial angle - Tilt ( 4 Bytes )
                            CDesignHeder.SInitAngle.tilt = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion initial angle - Tilt ( 4 Bytes )
                            #region initial angle - Swing ( 4 Bytes )
                            CDesignHeder.SInitAngle.swing = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion initial angle - Swing ( 4 Bytes )
                            #endregion initial angle
                            #region Init position
                            #region Init position - x ( 4 Bytes )
                            CDesignHeder.SInitPos.x = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion Init position - x ( 4 Bytes )
                            #region Init position - y ( 4 Bytes )
                            CDesignHeder.SInitPos.y = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion Init position - y ( 4 Bytes )
                            #region Init position - z ( 4 Bytes )
                            CDesignHeder.SInitPos.z = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion Init position - z ( 4 Bytes )
                            #endregion Init position

                            #region Init Scale - 100% = 1.0 ( 4 Bytes )
                            CDesignHeder.fInitScale = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion Init Scale - 100% = 1.0 ( 4 Bytes )

                            if (nVersion >= 14)
                            {
                                #region 2 Wheel Counter ( 1 Bytes )
                                CDesignHeder.nWheelCounter_2 = (int)byteData[nPos++];
                                #endregion 2 Wheel Counter ( 1 Bytes )

                                #region 3 Wheel Counter ( 1 Bytes )
                                CDesignHeder.nWheelCounter_3 = (int)byteData[nPos++];
                                #endregion 3 Wheel Counter ( 1 Bytes )

                                #region 4 Wheel Counter ( 1 Bytes )
                                CDesignHeder.nWheelCounter_4 = (int)byteData[nPos++];
                                #endregion 4 Wheel Counter ( 1 Bytes )
                            }

                            for (i = 0; i < 256; i++)
                            {
                                #region Axis MotorInfo

                                #region Motor ID ( 2 Byte )
                                CDesignHeder.pSMotorInfo[i].nMotorID = (int)(short)BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Motor ID ( 2 Byte )

                                #region Direction - 0 - Forward, 1 - Inverse ( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nMotorDir = (int)(short)BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Direction - 0 - Forward, 1 - Inverse ( 2 Bytes )

                                #region Max Angle (+) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fLimit_Up = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Max Angle (+) ( 4 Bytes )

                                #region Min Angle (-) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fLimit_Down = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Min Angle (-) ( 4 Bytes )

                                #region Center Position - Evd ( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nCenter_Evd = BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Center Position - Evd ( 2 Bytes )

                                #region Mech Move - Maximum pulse Evd ( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nMechMove = BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Mech Move - Maximum pulse Evd ( 2 Bytes )

                                #region Mech Angle - Angle of Mech Mov [ The maximum pulse corresponding to the angle value(Kor: 최대치 펄스에 해당하는 각도값 (분주각))]( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fMechAngle = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Mech Angle - Angle of Mech Mov [ The maximum pulse corresponding to the angle value(Kor: 최대치 펄스에 해당하는 각도값 (분주각))]( 4 Bytes )

                                #region Init Angle - Used for the initial position of the data in an arbitrary position( 4 Bytes )(Kor: 데이타의 초기자세를 임의의 자세로 하기 위해 사용( 4 Bytes ))
                                CDesignHeder.pSMotorInfo[i].fInitAngle = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                if (bNoSecondPos == false)
                                {
                                    CDesignHeder.pSMotorInfo[i].fInitAngle2 = BitConverter.ToSingle(byteData, nPos);
                                    nPos += 4;
                                }
                                #endregion Init Angle - Used for the initial position of the data in an arbitrary position( 4 Bytes )(Kor: 데이타의 초기자세를 임의의 자세로 하기 위해 사용( 4 Bytes ))

                                #region Interference axis number(Kor: 간섭 축 번호) ( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nInterference_Axis = BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Interference axis number(Kor: 간섭 축 번호) ( 2 Bytes )

                                #region axis Width ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fW = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion axis Width ( 4 Bytes )

                                #region Interference axis Width ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fInterference_W = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Interference axis Width ( 4 Bytes )

                                #region axis Side ( Right ) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fPos_Right = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion axis Side ( Right ) ( 4 Bytes )

                                #region axis Side ( Left) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fPos_Left = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion axis Side ( Left) ( 4 Bytes )

                                #region Interference axis ( Front ) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fInterference_Pos_Front = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Interference axis ( Front ) ( 4 Bytes )

                                #region Interference axis ( Rear ) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fInterference_Pos_Rear = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Interference axis ( Rear ) ( 4 Bytes )

                                #region NickName ( 32 Bytes )
                                CDesignHeder.pSMotorInfo[i].strNickName = Encoding.Default.GetString(byteData, nPos, 32);
                                nPos += 32;
                                #endregion NickName ( 32 Bytes )

                                #region Group Number ( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nGroupNumber = BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Group Number ( 2 Bytes )

                                #region mirroring axis number ( 2 Bytes )
                                if (bNoAxisMirror == false)
                                {
                                    CDesignHeder.pSMotorInfo[i].nAxis_Mirror = BitConverter.ToInt16(byteData, nPos);
                                    nPos += 2;
                                }
                                #endregion mirroring axis number ( 2 Bytes )
                                #region V1.4
                                #region Motor control type ( 2 Bytes )
                                if (nVersion >= 14)
                                {
                                    CDesignHeder.pSMotorInfo[i].nMotorControlType = BitConverter.ToInt16(byteData, nPos);
                                    nPos += 2;
                                }
                                #endregion mirroring axis number ( 2 Bytes )
                                #endregion V1.4

                                #endregion Axis MotorInfo
                            }

                            #region set the separation code [ HE - Header End ( 2 Bytes ) ]
                            strData = "";
                            strData += (char)(byteData[nPos++]);
                            strData += (char)(byteData[nPos++]);
                            if (strData != "HE") bFileOpened = false;
                            #endregion set the separation code [ HE - Header End ( 2 Bytes ) ]

                            int nSize_GroupName;
                            int nSize_0;
                            int nSize_1;
                            for (i = 0; i < 512; i++)
                            {
                                #region V1.4
                                if (nVersion >= 14)
                                {
                                    #region Secret Mode
                                    // Verify that the encryption code -> If this is the encryption code is set to '1'.
                                    // Kor: // 암호화 코드인지 확인 -> 암호화 코드라면 '1'
                                    CDesignHeder.pnSecret[i] = (int)(byte)(byteData[nPos++]); 
                                    #endregion Secret Mode
                                    #region Type
                                    // check formulas Control Type -> If the wheel-type control '1', if the position type control '0'
                                    // Kor: 수식 제어타입 확인 -> 바퀴형 제어라면 '1', 위치형 제어라면 '0'
                                    CDesignHeder.pnType[i] = (int)(byte)(byteData[nPos++]); 
                                    #endregion Type
                                }
                                #endregion V1.4

                                #region GroupName String

                                #region Size - Kinematics ( 2 Bytes )
                                nSize_GroupName = (int)(short)(BitConverter.ToInt16(byteData, nPos));
                                nPos += 2;
                                #endregion Size - Kinematics ( 2 Bytes )

                                #region String - Kinematics
                                CDesignHeder.pstrGroupName[i] = Encoding.Default.GetString(byteData, nPos, nSize_GroupName);
                                nPos += nSize_GroupName;
                                #endregion String - Kinematics

                                #endregion GroupName String

                                #region Kinematics/InverseKinematics String
                                #region Size - Kinematics ( 2 Bytes )
                                nSize_0 = (int)(short)(BitConverter.ToInt16(byteData, nPos));
                                nPos += 2;
                                #endregion Size - Kinematics ( 2 Bytes )

                                #region String - Kinematics
                                //CDesignHeder.pstrKinematics[i] = "";
                                //for (int j = 0; j < nSize_0; j++) CDesignHeder.pstrKinematics[i] += (char)(byteData[nPos++]);
                                CDesignHeder.pstrKinematics[i] = Encoding.Default.GetString(byteData, nPos, nSize_0);
                                // Since then loads the data may be encrypted.(Kor: 암호화 데이터일 수도 있으므로 로딩)
                                CDesignHeder.pSEncryptKinematics_encryption[i].byteEncryption = new byte[nSize_0];
                                Array.Copy(byteData, nPos, CDesignHeder.pSEncryptKinematics_encryption[i].byteEncryption, 0, nSize_0);

                                nPos += nSize_0;
                                #endregion String - Kinematics

                                #region Size - InverseKinematics ( 2 Bytes )
                                nSize_1 = (int)(short)(int)(short)(BitConverter.ToInt16(byteData, nPos));
                                nPos += 2;
                                #endregion Size - InverseKinematics ( 2 Bytes )

                                #region String - InverseKinematics
                                CDesignHeder.pstrInverseKinematics[i] = Encoding.Default.GetString(byteData, nPos, nSize_1);
                                // Since then loads the data may be encrypted.(Kor: 암호화 데이터일 수도 있으므로 로딩)
                                CDesignHeder.pSEncryptInverseKinematics_encryption[i].byteEncryption = new byte[nSize_1];
                                Array.Copy(byteData, nPos, CDesignHeder.pSEncryptInverseKinematics_encryption[i].byteEncryption, 0, nSize_1);

                                nPos += nSize_1;
                                #endregion String - InverseKinematics
                                #endregion Kinematics/InverseKinematics String
                            }

                            #region set the separation code [ KE - Kinematics End ( 2 Bytes ) ]
                            strData = "";
                            strData += (char)(byteData[nPos++]);
                            strData += (char)(byteData[nPos++]);
                            if (strData != "KE") bFileOpened = false;
                            #endregion set the separation code [ KE - Kinematics End ( 2 Bytes ) ]

                            #region Actual design string
                            #region Size - Actual design string ( 2 Bytes )
                            nSize_0 = (int)(ushort)(BitConverter.ToUInt16(byteData, nPos));
                            nPos += 2;
                            #endregion Size - Actual design string ( 2 Bytes )

                            #region String - Actual design string
                            //CDesignHeder.strDrawModel = "";
                            CDesignHeder.strDrawModel = Encoding.Default.GetString(byteData, nPos, nSize_0);//CConvert.RemoveChar(Encoding.Default.GetString(byteData, nPos, nSize_0), '\r');
                            nPos += nSize_0;
                            // Dhf -> Ojw(Convert)
                            String[] pstrLine = CDesignHeder.strDrawModel.Split('\n');
                            int nCnt = 0;
                            StringBuilder sbAll = new StringBuilder();
                            sbAll.Clear();
                            foreach (string strLine in pstrLine)
                            {
                                String[] pstrTmp = strLine.Split(',');
                                //if (pstrTmp.Length > 10)
                                //{
                                    nCnt = 0;
                                    bool bCaption = false;
                                    int nModelPosition = 2;
                                    foreach (string strItem in pstrTmp)
                                    {
                                        if (strItem.IndexOf("//") >= 0)
                                        {
                                            bCaption = true;
                                        }
                                        else if (bCaption == false)
                                        {
                                            if (nCnt++ == nModelPosition)
                                            {
                                                sbAll.Append("1.0,");
                                                int nData = CConvert.StrToInt(strItem);
                                                if (nData >= 0x1000) sbAll.Append(CConvert.IntToStr(nData - 0x1000));
                                                else sbAll.Append("#" + CConvert.RemoveChar(strItem, ' '));                                                
                                            }
                                        }
                                        if (nCnt != (nModelPosition + 1)) { sbAll.Append(CConvert.RemoveChar(strItem, ' ')); }
                                        else nCnt++;

                                        if (strItem.IndexOf('\r') < 0) sbAll.Append(',');
                                        else sbAll.Append("\n");
                                    }
                                //}
                            }
                            CDesignHeder.strDrawModel = sbAll.ToString();
                            // Set a new version
                            CDesignHeder.strVersion = _STR_EXT.ToUpper() + C3d._STR_EXT_VERSION.ToUpper();
                            #endregion String - Actual design string
                            #endregion Actual design string


                            #region Comment
                            #region Size - Comment ( 2 Bytes )
                            nSize_0 = (int)(short)(int)(short)(BitConverter.ToInt16(byteData, nPos));
                            nPos += 2;
                            #endregion Size - Comment ( 2 Bytes )

                            #region String - Comment
                            CDesignHeder.strComment = Encoding.Default.GetString(byteData, nPos, nSize_0);
                            nPos += nSize_0;
                            #endregion String - Comment
                            #endregion Comment

                            #region set the separation code [ FE - File End ( 2 Bytes ) ]
                            strData = "";
                            strData += (char)(byteData[nPos++]);
                            strData += (char)(byteData[nPos++]);
                            if (strData != "FE") bFileOpened = false;
                            #endregion set the separation code [ FE - File End ( 2 Bytes ) ]

                            //fs.Close();
                            //bFileOpened = true;

                    
                        }
                        else
#endif // _DHF_FILE
#if true
                        if ((strTmp[0] == 'O') & (strTmp[1] == 'J') & (strTmp[2] == 'W'))//(strTmp == strData) 
                        {
                            int nVersion = 0;// 010000; // 01.00.00
                            for (i = 3; i < (_STR_EXT.Length + _STR_EXT_VERSION.Length); i++) 
                            {
                                m_strVersion += (char)byteData[i];
                                if ((char)byteData[i] != '.')
                                {
                                    nVersion = nVersion * 10 + (int)(byteData[i] - 0x30);
                                }
                            }
                            
                            CDesignHeder.strVersion = strTmp;
                            CDesignHeder.nVersion = nVersion;

                            int nPos = (_STR_EXT.Length + _STR_EXT_VERSION.Length);   // 앞의 (_STR_EXT.Length + _STR_EXT_VERSION.Length)개는 'OJW01.00.00' 에 할당

                            #region From Version 1.1.0(_STR_EXT_VERSION = "01.01.00")( 4 Bytes )
                            if (nVersion > 010000)
                            {
                                CDesignHeder.nDefaultFunctionNumber = (int)(BitConverter.ToInt32(byteData, nPos));
                                //SetFunctionNumber(CDesignHeder.nDefaultFunctionNumber);                                
                                //Prop_Set_Main_DefaultFunctionNum(CDesignHeder.nDefaultFunctionNumber);
                                nPos += 4;
                            }
                            #endregion From Version 1.1.0(_STR_EXT_VERSION = "01.01.00")( 4 Bytes )

                            #region Model type ( 2 Bytes )
                            CDesignHeder.nModelNum = (int)(short)(BitConverter.ToInt16(byteData, nPos));
                            nPos += 2;
                            #endregion Model type ( 2 Bytes )

                            #region Title ( 21 Bytes )
                            CDesignHeder.strModelName = Encoding.Default.GetString(byteData, nPos, 21);
                            nPos += 21;
                            #endregion Title ( 21 Bytes )

                            #region BackColor - background color ( 4 Bytes)
                            CDesignHeder.cBackColor = Color.FromArgb(BitConverter.ToInt32(byteData, nPos));
                            nPos += 4;
                            #endregion BackColor - background color ( 4 Bytes)

                            #region Num. of Motor ( 2 Bytes )
                            CDesignHeder.nMotorCnt = (int)(short)(BitConverter.ToInt16(byteData, nPos));
                            nPos += 2;
                            #endregion Num. of Motor ( 2 Bytes )
                            #region Init Angle
                            #region Init Angle - Pan ( 4 Bytes )
                            CDesignHeder.SInitAngle.pan = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion Init Angle - Pan ( 4 Bytes )
                            #region Init Angle - Tilt ( 4 Bytes )
                            CDesignHeder.SInitAngle.tilt = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion Init Angle - Tilt ( 4 Bytes )
                            #region Init Angle - Swing ( 4 Bytes )
                            CDesignHeder.SInitAngle.swing = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion Init Angle - Swing ( 4 Bytes )
                            #endregion Init Angle
                            #region Init Position
                            #region Init Position - x ( 4 Bytes )
                            CDesignHeder.SInitPos.x = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion Init Position - x ( 4 Bytes )
                            #region Init Position - y ( 4 Bytes )
                            CDesignHeder.SInitPos.y = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion Init Position - y ( 4 Bytes )
                            #region Init Position - z ( 4 Bytes )
                            CDesignHeder.SInitPos.z = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion Init Position - z ( 4 Bytes )
                            #endregion Init Position

                            #region Init Scale - 100% = 1.0 ( 4 Bytes )
                            CDesignHeder.fInitScale = BitConverter.ToSingle(byteData, nPos);
                            nPos += 4;
                            #endregion Init Scale - 100% = 1.0 ( 4 Bytes )

                            #region 2 Wheel Counter ( 1 Bytes )
                            CDesignHeder.nWheelCounter_2 = (int)byteData[nPos++];
                            #endregion 2 Wheel Counter ( 1 Bytes )

                            #region 3 Wheel Counter ( 1 Bytes )
                            CDesignHeder.nWheelCounter_3 = (int)byteData[nPos++];
                            #endregion 3 Wheel Counter ( 1 Bytes )

                            #region 4 Wheel Counter ( 1 Bytes )
                            CDesignHeder.nWheelCounter_4 = (int)byteData[nPos++];
                            #endregion 4 Wheel Counter ( 1 Bytes )

                            for (i = 0; i < 256; i++)
                            {
                                #region Axis MotorInfo

                                #region Motor ID ( 2 Byte )
                                CDesignHeder.pSMotorInfo[i].nMotorID = (int)(short)BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Motor ID ( 2 Byte )

                                #region Direction - 0 - Forward, 1 - Inverse ( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nMotorDir = (int)(short)BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Direction - 0 - Forward, 1 - Inverse ( 2 Bytes )

                                #region Max Angle (+) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fLimit_Up = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Max Angle (+) ( 4 Bytes )

                                #region Min Angle (-) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fLimit_Down = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Min Angle (-) ( 4 Bytes )

                                #region Center Position - Evd ( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nCenter_Evd = BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Center Position - Evd ( 2 Bytes )

                                #region Mech Move - Maximum Pulse value(Evd)(Kor: 최대치 펄스값 Evd)( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nMechMove = BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Mech Move - Maximum Pulse value(Evd)(Kor: 최대치 펄스값 Evd)( 2 Bytes )

                                #region Mech Angle - Angle of Mech Mov [ The maximum pulse corresponding to the angle value(Kor: 최대치 펄스에 해당하는 각도값 (분주각))]( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fMechAngle = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Mech Angle - Angle of Mech Mov [ The maximum pulse corresponding to the angle value(Kor: 최대치 펄스에 해당하는 각도값 (분주각))]( 4 Bytes )

                                #region Init Angle - Used for the initial position of the data in an arbitrary position( 4 Bytes )(Kor: 데이타의 초기자세를 임의의 자세로 하기 위해 사용( 4 Bytes ))
                                // First Posture
                                CDesignHeder.pSMotorInfo[i].fInitAngle = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                // Second Posture
                                CDesignHeder.pSMotorInfo[i].fInitAngle2 = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Init Angle - Used for the initial position of the data in an arbitrary position( 4 Bytes )(Kor: 데이타의 초기자세를 임의의 자세로 하기 위해 사용( 4 Bytes ))

                                #region Interference axis number(Kor: 간섭 축 번호) ( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nInterference_Axis = BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Interference axis number(Kor: 간섭 축 번호) ( 2 Bytes )

                                #region axis Width ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fW = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion axis Width ( 4 Bytes )

                                #region Interference axis Width ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fInterference_W = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Interference axis Width ( 4 Bytes )

                                #region Axis Side ( Right ) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fPos_Right = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Axis Side ( Right ) ( 4 Bytes )

                                #region Axis Side ( Left) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fPos_Left = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Axis Side ( Left) ( 4 Bytes )

                                #region Interference axis ( Front ) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fInterference_Pos_Front = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Interference axis ( Front ) ( 4 Bytes )

                                #region Interference axis ( Rear ) ( 4 Bytes )
                                CDesignHeder.pSMotorInfo[i].fInterference_Pos_Rear = BitConverter.ToSingle(byteData, nPos);
                                nPos += 4;
                                #endregion Interference axis ( Rear ) ( 4 Bytes )

                                #region NickName ( 32 Bytes )
                                CDesignHeder.pSMotorInfo[i].strNickName = Encoding.Default.GetString(byteData, nPos, 32);
                                nPos += 32;
                                #endregion NickName ( 32 Bytes )

                                #region Group Number ( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nGroupNumber = BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion Group Number ( 2 Bytes )

                                #region mirroring axis number ( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nAxis_Mirror = BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion mirroring axis number ( 2 Bytes )
                                #region V1.0.0
                                #region Motor control type ( 2 Bytes )
                                CDesignHeder.pSMotorInfo[i].nMotorControlType = BitConverter.ToInt16(byteData, nPos);
                                nPos += 2;
                                #endregion mirroring axis number ( 2 Bytes )
                                #endregion V1.0.0

                                #endregion Axis MotorInfo
                            }

                            #region set the separation code [ HE - Header End ( 2 Bytes ) ]
                            strData = "";
                            strData += (char)(byteData[nPos++]);
                            strData += (char)(byteData[nPos++]);
                            if (strData != "HE") bFileOpened = false;
                            #endregion set the separation code [ HE - Header End ( 2 Bytes ) ]

                            int nSize_GroupName;
                            int nSize_0;
                            int nSize_1;
                            for (i = 0; i < 512; i++)
                            {
                                #region V1.0.0
                                #region Secret Mode
                                // Verify that the encryption code -> If this is the encryption code is set to '1'.
                                // Kor: // 암호화 코드인지 확인 -> 암호화 코드라면 '1'
                                CDesignHeder.pnSecret[i] = (int)(byte)(byteData[nPos++]); 
                                #endregion Secret Mode
                                #region Type
                                // check formulas Control Type -> If the wheel-type control '1', if the position type control '0'
                                    // Kor: 수식 제어타입 확인 -> 바퀴형 제어라면 '1', 위치형 제어라면 '0'
                                    CDesignHeder.pnType[i] = (int)(byte)(byteData[nPos++]);
                                #endregion Type
                                #endregion V1.0.0

                                #region GroupName String

                                #region Size - Kinematics ( 2 Bytes )
                                nSize_GroupName = (int)(short)(BitConverter.ToInt16(byteData, nPos));
                                nPos += 2;
                                #endregion Size - Kinematics ( 2 Bytes )

                                #region String - Kinematics
                                CDesignHeder.pstrGroupName[i] = Encoding.Default.GetString(byteData, nPos, nSize_GroupName);
                                nPos += nSize_GroupName;
                                #endregion String - Kinematics

                                #endregion GroupName String

                                #region Kinematics/InverseKinematics String
                                #region Size - Kinematics ( 2 Bytes )
                                nSize_0 = (int)(short)(BitConverter.ToInt16(byteData, nPos));
                                nPos += 2;
                                #endregion Size - Kinematics ( 2 Bytes )

                                #region String - Kinematics
                                //CDesignHeder.pstrKinematics[i] = "";
                                //for (int j = 0; j < nSize_0; j++) CDesignHeder.pstrKinematics[i] += (char)(byteData[nPos++]);
                                CDesignHeder.pstrKinematics[i] = Encoding.Default.GetString(byteData, nPos, nSize_0);
                                // Since then loads the data may be encrypted.(Kor: 암호화 데이터일 수도 있으므로 로딩)
                                CDesignHeder.pSEncryptKinematics_encryption[i].byteEncryption = new byte[nSize_0];
                                Array.Copy(byteData, nPos, CDesignHeder.pSEncryptKinematics_encryption[i].byteEncryption, 0, nSize_0);

                                nPos += nSize_0;
                                #endregion String - Kinematics

                                #region Size - InverseKinematics ( 2 Bytes )
                                nSize_1 = (int)(short)(int)(short)(BitConverter.ToInt16(byteData, nPos));
                                nPos += 2;
                                #endregion Size - InverseKinematics ( 2 Bytes )

                                #region String - InverseKinematics
                                CDesignHeder.pstrInverseKinematics[i] = Encoding.Default.GetString(byteData, nPos, nSize_1);
                                // Since then loads the data may be encrypted.(Kor: 암호화 데이터일 수도 있으므로 로딩)
                                CDesignHeder.pSEncryptInverseKinematics_encryption[i].byteEncryption = new byte[nSize_1];
                                Array.Copy(byteData, nPos, CDesignHeder.pSEncryptInverseKinematics_encryption[i].byteEncryption, 0, nSize_1);

                                nPos += nSize_1;
                                #endregion String - InverseKinematics
                                #endregion Kinematics/InverseKinematics String
                            }

                            #region set the separation code [ KE - Kinematics End ( 2 Bytes ) ]
                            strData = "";
                            strData += (char)(byteData[nPos++]);
                            strData += (char)(byteData[nPos++]);
                            if (strData != "KE") bFileOpened = false;
                            #endregion set the separation code [ KE - Kinematics End ( 2 Bytes ) ]

                            #region Actual design string
                            #region Size - Actual design string ( 2 Bytes )
                            nSize_0 = (int)(ushort)(BitConverter.ToUInt16(byteData, nPos));
                            nPos += 2;
                            #endregion Size - Actual design string ( 2 Bytes )

                            #region String - Actual design string
                            //CDesignHeder.strDrawModel = "";
                            CDesignHeder.strDrawModel = Encoding.Default.GetString(byteData, nPos, nSize_0);


                            // ojw5014


                            nPos += nSize_0;
                            #endregion String - Actual design string
                            #endregion Actual design string


                            #region Comment
                            #region Size - Comment ( 2 Bytes )
                            nSize_0 = (int)(short)(int)(short)(BitConverter.ToInt16(byteData, nPos));
                            nPos += 2;
                            #endregion Size - Comment ( 2 Bytes )

                            #region String - Comment
                            CDesignHeder.strComment = Encoding.Default.GetString(byteData, nPos, nSize_0);
                            nPos += nSize_0;
                            #endregion String - Comment
                            #endregion Comment

                            #region set the separation code [ FE - File End ( 2 Bytes ) ]
                            strData = "";
                            strData += (char)(byteData[nPos++]);
                            strData += (char)(byteData[nPos++]);
                            if (strData != "FE") bFileOpened = false;
                            #endregion set the separation code [ FE - File End ( 2 Bytes ) ]

                            //Prop_Update_VirtualObject();
                        }
                        else bFileOpened = false;
#endif
                        #endregion OjwVersion
                        ////////////////////////////////////////////////////////////////////////////

                        if (bFileOpened == true)
                        {
                            CHeader = CDesignHeder;
                            CHeader.pDhParamAll = new CDhParamAll[512];

                            CDesignHeder = null;

                            m_bFileOpening = false;
                            return true;
                        }

                        m_strVersion = "";
                        CDesignHeder = null;
                        m_bFileOpening = false;
                        return false;
                    }
                    catch
                    {
                        m_strVersion = "";
                        m_bFileOpening = false;
                        return false;
                    }
                }
                //}
            #endregion Designer class(COjwDesignerHeader)


            #region Designer header class(COjwDesignerHeader)
            public class COjwDesignerHeader
            {
                public int nVersion;
                public String strVersion;

                public int nDefaultFunctionNumber = -1;

                public int nModelNum = 0;                               // The name of the actual model with the at least 1(Kor: 1 이상의 값을 가지는 실제적인 모델의 이름)
                public String strModelNum = String.Empty;               // 
                public string strModelName = "";

                public SAngle3D_t SInitAngle = new SAngle3D_t();        // The default angle facing the screen(Kor: 화면을 바라보는 기본 각도)
                public SVector3D_t SInitPos = new SVector3D_t();        // The default position of the object present in the screen(Kor: 화면내에 존재하는 오브젝트의 기본 위치)

                public float fInitScale = 0.35f;                        // Size ratio in the initial screen(Kor: 초기 화면의 크기 비율)

                public bool bDisplay_Light = true;                      // To use the light(Kor: 빛을 사용할 것인지...)
                public bool bDisplay_Invisible = false;                 // Transparent material, regardless of whether the ball(Kor: 재질과 상관없이 투명하게 볼 것인지...)
                public bool bDisplay_Axis = false;                      // Look what the reference axis(Kor: 기준축을 보일 건지)

                public Color cBackColor = Color.FromArgb(-5658199);     // backgroud color(Kor: 배경 색)

                public SMotorInfo_t[] pSMotorInfo = new SMotorInfo_t[256];          // This limit is necessary because it reflects axes up to 256 axes(Kor: 리미트가 필요한 축은 최대 256축 이므로 이를 반영)

                public string[] pstrGroupName = new string[512];          // Group name(Kor: 지정한 그룹의 이름)
                public CDhParamAll[] pDhParamAll = new CDhParamAll[512]; // (0~255 Group)DH Param
                public int[] pnSecret = new int[512];                   // 0: Normal, 1: Secret Letter
                public int[] pnType = new int[512];                     // 0: Normal, 1: Wheel Control Type
                public string[] pstrKinematics = new string[512];
                public SEncryption_t[] pSEncryptKinematics_encryption = new SEncryption_t[512];
                public string[] pstrInverseKinematics = new string[512];
                public SEncryption_t[] pSEncryptInverseKinematics_encryption = new SEncryption_t[512];

                public SOjwCode_t[] pSOjwCode = new SOjwCode_t[512];

                public string strDrawModel;                                 // String that contains the actual data model(Kor: 실제 모델 데이타가 들어있는 스트링)
                // The number of motors in internal (However, be sure to order 0,1,2, ... must be created in order)
                // Kor: 내부에 들어있는 모터의 갯수 ( 단, 반드시 순서대로 0,1,2,... 순으로 작성해야 한다. )
                public int nMotorCnt;                                   

                public string strComment;                               // comment(Kor: 부가설명)

                public int nWheelCounter_2 = 0;                         // The number of 2-wheel wheels(Kor: 2륜 디바이스의 개수)
                public int nWheelCounter_3 = 0;                         // The number of 3-wheel wheels(Kor: 3륜 디바이스의 개수)
                public int nWheelCounter_4 = 0;                         // The number of 4-wheel wheels(Kor: 4륜 디바이스의 개수)

                public COjwDesignerHeader() // 생성자
                {                    
                    SInitAngle.pan = -10.0f;
                    SInitAngle.tilt = 10.0f;
                    SInitAngle.swing = 0.0f;

                    SInitPos.x = 0.0f;
                    SInitPos.y = 0.0f;
                    SInitPos.z = 0.0f;

                    pSMotorInfo.Initialize();
                    for (int i = 0; i < 256; i++)
                    {
                        pSMotorInfo[i].nInterference_Axis = -1;

                        // Alloc memory(Kor: 메모리 확보)
                        pDhParamAll[i] = new CDhParamAll();
                        pDhParamAll[i].DeleteAll();

                        pSMotorInfo[i].strNickName = "";

                        pstrGroupName[i] = "";
                        pstrKinematics[i] = "";
                        //pbyteKinematics_encryption
                        pstrInverseKinematics[i] = "";
                        //pbyteInverseKinematics_encryption

                        pSEncryptKinematics_encryption[i].byteEncryption = new byte[0];
                        pSEncryptInverseKinematics_encryption[i].byteEncryption = new byte[0];
                    }
                    for (int i = 0; i < 256; i++)
                    {
                        pstrGroupName[256 + i] = "";
                        pstrKinematics[256 + i] = "";
                        pstrInverseKinematics[256 + i] = "";
                        pSEncryptKinematics_encryption[256 + i].byteEncryption = new byte[0];
                        pSEncryptInverseKinematics_encryption[256 + i].byteEncryption = new byte[0];
                    }
                    strDrawModel = "";
                    nMotorCnt = 0;
                    nDefaultFunctionNumber = -1; // No Choice
                }
            }
            #endregion Designer header class(COjwDesignerHeader)
            
            public class COjwDisp
            {
                public COjwDisp()
                {
                    cColor = Color.White;
                    for (int i = 0; i < 5; i++)
                    {
                        afTrans[i].x = 0;
                        afTrans[i].y = 0;
                        afTrans[i].z = 0;

                        afRot[i].pan = 0;
                        afRot[i].tilt = 0;
                        afRot[i].swing = 0;
                    }
                }

                // Only use variables inside the class(Kor: 클래스 내부에서만 사용할 변수)
                private Color color = Color.White;
                public float fAlpha = 1.0f;
                private float[] afColor = new float[3];

                // Determine the internal handle, if (value < 0) then "No ID" => In other words, when determining the name of the OpenGL picking
                // Kor: 내부적 핸들을 결정, 단, 0보다 작으면(-) ID 없음. => 즉, OpenGL 의 픽킹 시 이름을 결정
                public int nName = -1;
                //public int nDispModel = 0;      // Recording the type of data to be drawn Modeling(Kor: 그려질 모델링 데이타의 종류를 기록 - 사각형, 원형, 구, ...)
                public string strDispObject = "#0"; // Recording the type of data to be drawn Modeling(Kor: 그려질 모델링 데이타의 종류를 기록) - User ASE/OBJ
                public bool bFilled = true;     // Determining the populate the attributes of the picture(Kor: 그림의 속을 채울지를 결정)

                // 
                #region Color Set / Get
                public float fColor_R // To fill in colors(Kor: 속을 채울 색상)
                {
                    get { return afColor[0]; }
                    set
                    {
                        color = Color.FromArgb((int)(((value > 1) || (value < 0)) ? (int)255 : (int)(Math.Round(afColor[0] * 255.0f))), (int)(Math.Round(afColor[1] * 255.0f)), (int)(Math.Round(afColor[2] * 255.0f)));
                        afColor[0] = ((float)(color.R) / 255.0f);  // R
                    }
                }
                public float fColor_G
                {
                    get { return afColor[1]; }
                    set
                    {
                        color = Color.FromArgb((int)(Math.Round(afColor[0] * 255.0f)), (int)(((value > 1) || (value < 0)) ? (int)255 : (int)(Math.Round(afColor[1] * 255.0f))), (int)(Math.Round(afColor[2] * 255.0f)));
                        afColor[1] = ((float)(color.G) / 255.0f);  // G
                    }
                }
                public float fColor_B
                {
                    get { return afColor[2]; }
                    set
                    {
                        color = Color.FromArgb((int)(Math.Round(afColor[0] * 255.0f)), (int)(Math.Round(afColor[1] * 255.0f)), (int)(((value > 1) || (value < 0)) ? (int)255 : (int)(Math.Round(afColor[2] * 255.0f))));
                        afColor[2] = ((float)(color.B) / 255.0f);  // B
                    }
                }
                public Color cColor
                {
                    get { return color; }
                    set
                    {
                        color = value;
                        afColor[0] = ((float)(color.R) / 255.0f);  // R
                        afColor[1] = ((float)(color.G) / 255.0f);  // G
                        afColor[2] = ((float)(color.B) / 255.0f);  // B
                    }
                }
                #endregion Color Set / Get

                public int nTexture = 0; // The index of the texture loading - reserve(Kor: 텍스쳐의 로딩 인덱스( 아직 안씀 ))

                public bool bInit = false; // Determine whether to re-initialize the position(Kor: 위치를 다시 초기화 할지를 결정)

                public float fWidth_Or_Radius = 10.0f; // Width / radius
                public float fHeight_Or_Depth = 4.0f; // Height / depth
                public float fDepth_Or_Cnt = 10.0f;    // Depth / line count
                public float fThickness = 4.0f;       // thickness
                public float fGap = 0.0f; // If you need to draw a [case] - do not need the another things.(Kor: case 를 그리거나 할 경우 필요 - 나머진 필요 없다.)

                public string strCaption = ""; // comment

                // Offset
                public SVector3D_t SOffset_Trans = new SVector3D_t();
                public SAngle3D_t SOffset_Rot = new SAngle3D_t();

                // translation / Rotation
                public SVector3D_t[] afTrans = new SVector3D_t[5]; // [3] - T/R : 1st sub-screen conversion(Kor: 1차 서브화면 변환), [4] - T/R : 2st sub-screen conversion(Kor: 2차 서브화면 변환)
                public SAngle3D_t[] afRot = new SAngle3D_t[5];

                // Determining the actual rotation axis(Kor: 실 회전 축 결정)[0 ~ 2(Pan, Tilt, Swing), 3~5(x,y,z), 6(cw), 7(ccw)]
                public int nAxisMoveType;
                public int nDir;       // direction - 0 : forward, 1 : inverse
                public float fAngle; // actual angle value
                public float fAngle_Offset; // angle Offset

                public int nMotorType;  // Motor control type - 0 : Position Control, 1 : Speed Control
                public int nMotorControl_MousePoint; // Direction of the mouse-drag application(Kor: 마우스 드래그 시의 방향 적용) : 0 - x+, 1 - x-, 2 - y+, 3 - y-

                //////////////////////////////
                #region Picking Var
                public int nPickGroup_A;
                public int nPickGroup_B;
                public int nPickGroup_C;
                public int nInverseKinematicsNumber;

                public float fScale_Serve0;
                public float fScale_Serve1;

                public string strPickGroup_Comment;
                #endregion Picking

                public List<SVector3D_t> Points = new List<SVector3D_t>();

                public void InitData()
                {
                    SVector3D_t[] pSVector = new SVector3D_t[5];
                    SAngle3D_t[] pSAngle = new SAngle3D_t[5];
                    pSVector.Initialize();
                    pSAngle.Initialize();
                    List<SVector3D_t> tmpPoints = new List<SVector3D_t>();
                    //tmpPoints.Clear();
                    //tmpPoints.Add(new SVector3D_t(0, 0, 0));
                    //tmpPoints.Add(new SVector3D_t(10, 0, 0));
                    SetData(-1, Color.White, 1.0f, "#0", true, -1, false, 10, 10, 20, 4, 0, "", -1, 0, 0, 0, pSVector[0], pSAngle[0], pSVector, pSAngle, 0, 0, 0, 255, 0.35f, 0.35f, 0, 0, "", tmpPoints);                    
                    pSVector = null;
                    pSAngle = null;
                    Points.Clear();
                }

                public void SetData(COjwDisp OjwDispData)
                {
                    SetData(OjwDispData.nName, OjwDispData.cColor, OjwDispData.fAlpha, OjwDispData.strDispObject, OjwDispData.bFilled, OjwDispData.nTexture, OjwDispData.bInit, OjwDispData.fWidth_Or_Radius, OjwDispData.fHeight_Or_Depth, OjwDispData.fDepth_Or_Cnt,
                            OjwDispData.fThickness, OjwDispData.fGap, OjwDispData.strCaption,
                            OjwDispData.nAxisMoveType, OjwDispData.nDir, OjwDispData.fAngle, OjwDispData.fAngle_Offset,
                            OjwDispData.SOffset_Trans, OjwDispData.SOffset_Rot, OjwDispData.afTrans, OjwDispData.afRot,
                            OjwDispData.nPickGroup_A, OjwDispData.nPickGroup_B, OjwDispData.nPickGroup_C, OjwDispData.nInverseKinematicsNumber,
                            OjwDispData.fScale_Serve0, OjwDispData.fScale_Serve1,
                            OjwDispData.nMotorType, OjwDispData.nMotorControl_MousePoint,
                            OjwDispData.strPickGroup_Comment,
                            OjwDispData.Points
                            );
                }

                public void SetData(int nAxisName, Color cColorData, float fAlphaData, String strDrawObject, bool bFill, int nTextureData, bool bInitialize, float fW, float fH, float fD, float fT, float fGapData, string strMessage,
                                    int nAxisData, int nDirData, float fAngleData, float fAngle_OffsetData,
                                    SVector3D_t SOffset_Translation, SAngle3D_t SOffset_Rotation, SVector3D_t[] afTranslation, SAngle3D_t[] afRotation,
                                    int nObjectPickGroup_A, int nObjectPickGroup_B, int nObjectPickGroup_C, int nObjectPickGroup_InverseKinematics,
                                    float fObjectScale_Serve0, float fObjectScale_Serve1,
                                    int nObjectMotorType, int nObjectMotorControl_MousePoint,
                                    String strObjectPickGroup_Comment,
                                    List<SVector3D_t> lstPoints
                                    )
                {
                    cColor = cColorData;
                    fAlpha = fAlphaData;
                    // Determine the internal handle, if (value < 0) then "No ID" => In other words, when determining the name of the OpenGL picking
                    // Kor: 내부적 핸들을 결정, 단, 0보다 작으면(-) ID 없음. => 즉, OpenGL 의 픽킹 시 이름을 결정
                    nName = nAxisName;
                    //nDispModel = nModel; // Recording the type of data to be drawn Modeling(Kor: 그려질 모델링 데이타의 종류를 기록 - 사각형, 원형, 구, ...)
                    strDispObject = strDrawObject;
                    bFilled = bFill;   // Determining the populate the attributes of the picture(Kor: 그림의 속을 채울지를 결정)

                    nTexture = nTextureData;

                    bInit = bInitialize;
                    fWidth_Or_Radius = fW;
                    fHeight_Or_Depth = fH;
                    fDepth_Or_Cnt = fD;
                    fThickness = fT;
                    fGap = fGapData;
                    strCaption = strMessage;

                    SOffset_Trans.x = SOffset_Translation.x;
                    SOffset_Trans.y = SOffset_Translation.y;
                    SOffset_Trans.z = SOffset_Translation.z;

                    SOffset_Rot.pan = SOffset_Rotation.pan;
                    SOffset_Rot.tilt = SOffset_Rotation.tilt;
                    SOffset_Rot.swing = SOffset_Rotation.swing;

                    for (int i = 0; i < 5; i++)
                    {
                        afTrans[i].x = afTranslation[i].x;
                        afTrans[i].y = afTranslation[i].y;
                        afTrans[i].z = afTranslation[i].z;

                        afRot[i].pan = afRotation[i].pan;
                        afRot[i].tilt = afRotation[i].tilt;
                        afRot[i].swing = afRotation[i].swing;
                    }

                    nAxisMoveType = nAxisData;
                    nDir = nDirData;
                    fAngle = fAngleData;
                    fAngle_Offset = fAngle_OffsetData;

                    // Picking Data
                    nPickGroup_A = nObjectPickGroup_A;
                    nPickGroup_B = nObjectPickGroup_B;
                    nPickGroup_C = nObjectPickGroup_C;
                    nInverseKinematicsNumber = nObjectPickGroup_InverseKinematics;
                    fScale_Serve0 = fObjectScale_Serve0;
                    fScale_Serve1 = fObjectScale_Serve1;
                    strPickGroup_Comment = strObjectPickGroup_Comment;

                    nMotorType = nObjectMotorType;
                    nMotorControl_MousePoint = nObjectMotorControl_MousePoint;

                    Points = lstPoints;
                }
                #region Set
                // Determine the internal handle, if (value < 0) then "No ID" => In other words, when determining the name of the OpenGL picking
                // Kor: 내부적 핸들을 결정, 단, 0보다 작으면(-) ID 없음. => 즉, OpenGL 의 픽킹 시 이름을 결정
                public void         SetData_AxisName(int nValue) { nName = nValue; }     
                public void         SetData_Color(Color cValue) { cColor = cValue; }
                public void         SetData_DispObject(String strValue) { strDispObject = strValue; } // Recording the type of data to be drawn Modeling(Kor: 그려질 모델링 데이타의 종류를 기록 - 사각형, 원형, 구, ...)
                public void         SetData_Fill(bool bValue) { bFilled = bValue; } // Determining the populate the attributes of the picture(Kor: 그림의 속을 채울지를 결정)
                public void         SetData_Texture(int nValue) { nTexture = nValue; } 
                public void         SetData_Init(bool bValue) { bInit = bValue; }
                
                public void         SetData_Width_Or_Radius(float fValue) { fWidth_Or_Radius = fValue; } 
                public void         SetData_Height_Or_Depth(float fValue) { fHeight_Or_Depth = fValue; } 
                public void         SetData_Depth_Or_Cnt(float fValue) { fDepth_Or_Cnt = fValue; } 
                public void         SetData_Thickness(float fValue) { fThickness = fValue; } 
                public void         SetData_Gap(float fValue) { fGap = fValue; }
                public void         SetData_Caption(String strValue) { strCaption = strValue; }
                public void         SetData_Axis(int nValue) { nAxisMoveType = nValue; } 
                public void         SetData_Dir(int nValue) { nDir = nValue; } 
                public void         SetData_Angle(float fValue) { fAngle = fValue; } 
                public void         SetData_Angle_Offset(float fValue) { fAngle_Offset = fValue; }
                public void         SetData_Offset_Translation(float fX, float fY, float fZ) { SOffset_Trans.x = fX; SOffset_Trans.y = fY; SOffset_Trans.z = fZ; }
                public void         SetData_Offset_Rotation(float fPan, float fTilt, float fSwing) { SOffset_Rot.pan = fPan; SOffset_Rot.tilt = fTilt; SOffset_Rot.swing = fSwing; }
                public void         SetData_Translation(int nIndex, float fX, float fY, float fZ) { if ((nIndex >= 0) && (nIndex < afTrans.Length)) { afTrans[nIndex].x = fX; afTrans[nIndex].y = fY; afTrans[nIndex].z = fZ; } }
                public void         SetData_Rotation(int nIndex, float fPan, float fTilt, float fSwing) { if ((nIndex >= 0) && (nIndex < afRot.Length)) { afRot[nIndex].pan = fPan; afRot[nIndex].tilt = fTilt; afRot[nIndex].swing = fSwing; } }
                public void         SetData_nPickGroup_A(int nValue) { nPickGroup_A = nValue; } 
                public void         SetData_nPickGroup_B(int nValue) { nPickGroup_B = nValue; } 
                public void         SetData_nPickGroup_C(int nValue) { nPickGroup_C = nValue; } 
                public void         SetData_nInverseKinematicsNumber(int nValue) { nInverseKinematicsNumber = nValue; } 
                public void         SetData_fScale_Serve0(float fValue) { fScale_Serve0 = fValue; } 
                public void         SetData_fScale_Serve1(float fValue) { fScale_Serve1 = fValue; } 
                public void         SetData_nMotorType(int nValue) { nMotorType = nValue; } 
                public void         SetData_nMotorControl_MousePoint(int nValue) { nMotorControl_MousePoint = nValue; } 
                public void         SetData_strPickGroup_Comment(String strValue) { strPickGroup_Comment = strValue; }
                #endregion Set

                #region Get
                // Determine the internal handle, if (value < 0) then "No ID" => In other words, when determining the name of the OpenGL picking
                // Kor: 내부적 핸들을 결정, 단, 0보다 작으면(-) ID 없음. => 즉, OpenGL 의 픽킹 시 이름을 결정
                public int          GetData_AxisName() { return nName; }     
                public Color        GetData_Color() { return cColor; }
                public String       GetData_DispObject() { return strDispObject; } // Recording the type of data to be drawn Modeling(Kor: 그려질 모델링 데이타의 종류를 기록 - 사각형, 원형, 구, ...)
                public bool         GetData_Fill() { return bFilled; } // Determining the populate the attributes of the picture(Kor: 그림의 속을 채울지를 결정)
                public int          GetData_Texture() { return nTexture; }
                public bool         GetData_Init() { return bInit; }

                public float        GetData_Width_Or_Radius() { return fWidth_Or_Radius; }
                public float        GetData_Height_Or_Depth() { return fHeight_Or_Depth; }
                public float        GetData_Depth_Or_Cnt() { return fDepth_Or_Cnt; }
                public float        GetData_Thickness() { return fThickness; }
                public float        GetData_Gap() { return fGap; }
                public String       GetData_Caption() { return strCaption; }
                public int          GetData_Axis() { return nAxisMoveType; }
                public int          GetData_Dir() { return nDir; }
                public float        GetData_Angle() { return fAngle; }
                public float        GetData_Angle_Offset() { return fAngle_Offset; }
                public SVector3D_t  GetData_Offset_Translation() { return SOffset_Trans; }
                public SAngle3D_t   GetData_Offset_Rotation() { return SOffset_Rot; }
                public SVector3D_t  GetData_Translation(int nIndex) { return afTrans[nIndex % afTrans.Length]; }
                public SAngle3D_t   GetData_Rotation(int nIndex) { return afRot[nIndex % afRot.Length]; }
                public int          GetData_nPickGroup_A() { return nPickGroup_A; }
                public int          GetData_nPickGroup_B() { return nPickGroup_B; }
                public int          GetData_nPickGroup_C() { return nPickGroup_C; }
                public int          GetData_nInverseKinematicsNumber() { return nInverseKinematicsNumber; }
                public float        GetData_fScale_Serve0() { return fScale_Serve0; }
                public float        GetData_fScale_Serve1() { return fScale_Serve1; }
                public int          GetData_nMotorType() { return nMotorType; }
                public int          GetData_nMotorControl_MousePoint() { return nMotorControl_MousePoint; }
                public String       GetData_strPickGroup_Comment() { return strPickGroup_Comment; }
                #endregion Get

                public COjwDisp Clone()
                {
                    COjwDisp obj = new COjwDisp();
                    obj.cColor = this.cColor;
                    obj.fAlpha = this.fAlpha;
                    // Determine the internal handle, if (value < 0) then "No ID" => In other words, when determining the name of the OpenGL picking
                    // Kor: 내부적 핸들을 결정, 단, 0보다 작으면(-) ID 없음. => 즉, OpenGL 의 픽킹 시 이름을 결정
                    obj.nName = this.nName;
                    obj.strDispObject = (String)this.strDispObject.Clone(); // Recording the type of data to be drawn Modeling(Kor: 그려질 모델링 데이타의 종류를 기록 - 사각형, 원형, 구, ...)
                    obj.bFilled = this.bFilled;   // Determining the populate the attributes of the picture(Kor: 그림의 속을 채울지를 결정)

                    obj.nTexture = this.nTexture;

                    obj.bInit = this.bInit;
                    obj.fWidth_Or_Radius = this.fWidth_Or_Radius;
                    obj.fHeight_Or_Depth = this.fHeight_Or_Depth;
                    obj.fDepth_Or_Cnt = this.fDepth_Or_Cnt;
                    obj.fThickness = this.fThickness;
                    obj.fGap = this.fGap;
                    obj.strCaption = (String)this.strCaption.Clone();

                    obj.SOffset_Trans.x = this.SOffset_Trans.x;
                    obj.SOffset_Trans.y = this.SOffset_Trans.y;
                    obj.SOffset_Trans.z = this.SOffset_Trans.z;

                    obj.SOffset_Rot.pan = this.SOffset_Rot.pan;
                    obj.SOffset_Rot.tilt = this.SOffset_Rot.tilt;
                    obj.SOffset_Rot.swing = this.SOffset_Rot.swing;

                    for (int i = 0; i < 5; i++)
                    {
                        obj.afTrans[i].x = this.afTrans[i].x;
                        obj.afTrans[i].y = this.afTrans[i].y;
                        obj.afTrans[i].z = this.afTrans[i].z;

                        obj.afRot[i].pan = this.afRot[i].pan;
                        obj.afRot[i].tilt = this.afRot[i].tilt;
                        obj.afRot[i].swing = this.afRot[i].swing;
                    }

                    obj.nAxisMoveType = this.nAxisMoveType;
                    obj.nDir = this.nDir;
                    obj.fAngle = this.fAngle;
                    obj.fAngle_Offset = this.fAngle_Offset;

                    // Picking Data
                    obj.nPickGroup_A = this.nPickGroup_A;
                    obj.nPickGroup_B = this.nPickGroup_B;
                    obj.nPickGroup_C = this.nPickGroup_C;
                    obj.nInverseKinematicsNumber = this.nInverseKinematicsNumber;
                    obj.fScale_Serve0 = this.fScale_Serve0;
                    obj.fScale_Serve1 = this.fScale_Serve1;
                    obj.strPickGroup_Comment = (this.strPickGroup_Comment == null) ? "" : (String)this.strPickGroup_Comment.Clone();

                    obj.nMotorType = this.nMotorType;
                    obj.nMotorControl_MousePoint = this.nMotorControl_MousePoint;
                    
                    obj.Points.Clear();
                    for (int i = 0; i < this.Points.Count; i++)
                    {
                        obj.Points.Add(this.Points[i]);
                    }
                    
                    return obj;
                }
            }

            public class COjwDispAll
            {
#if true
                private List<COjwDisp> lstOjwDisp;
                public int GetCount() { return (lstOjwDisp == null) ? 0 : lstOjwDisp.Count; }
                public COjwDisp GetData(int nIndex)
                {
                    if (lstOjwDisp == null) return null;
                    else if ((nIndex >= lstOjwDisp.Count) || (nIndex < 0)) return null;
                    else return lstOjwDisp[nIndex];
                }
                public bool SetData(int nIndex, COjwDisp OjwDisp)
                {
                    if ((nIndex >= lstOjwDisp.Count) || (nIndex < 0)) return false;
                    
                    lstOjwDisp[nIndex].SetData(OjwDisp);
                    return true;
                }
                public bool AddData(COjwDisp OjwDisp)
                {
                    if (lstOjwDisp == null) lstOjwDisp = new List<COjwDisp>();
                    lstOjwDisp.Add(OjwDisp);
                    return true;
                }
                public bool DeleteData(int nIndex)
                {
                    if (lstOjwDisp == null) return false;// lstOjwDisp = new List<COjwDisp>();
                    try
                    {
                        lstOjwDisp.RemoveAt(nIndex);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                public bool DeleteData(COjwDisp OjwDisp)
                {
                    if (lstOjwDisp == null) return false;
                    try
                    {
                        lstOjwDisp.Remove(OjwDisp);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                public void DeleteAll()
                {
                    if (lstOjwDisp != null)
                        lstOjwDisp.Clear();
                }
                public String GetString_PickingComment(int nGroupA, int nGroupB, int nGroupC)
                {
                    string strComment = null;
                    COjwDisp CDisp = new COjwDisp();
                    for (int i = 0; i < GetCount(); i++)
                    {
                        CDisp = GetData(i);
                        if (CDisp != null)
                        {
                            if ((CDisp.nPickGroup_A == nGroupA) && (CDisp.nPickGroup_B == nGroupB) && (CDisp.nPickGroup_C == nGroupC))
                            {
                                strComment = CDisp.strPickGroup_Comment;
                                break;
                            }
                        }
                    }
                    return strComment;
                }
#else
                #region 여기까지
                private COjwDisp[] pOjwDisp;
                public int GetCount() { return (pOjwDisp == null) ? 0 : pOjwDisp.Length; }

                public COjwDisp GetData(int nIndex)
                {
                    if (pOjwDisp == null) return null;
                    else if ((nIndex >= pOjwDisp.Length) || (nIndex < 0)) return null;
                    else return pOjwDisp[nIndex];
                }
                public bool SetData(int nIndex, COjwDisp OjwDisp)
                {
                    if ((nIndex >= pOjwDisp.Length) || (nIndex < 0)) return false;
                    pOjwDisp[nIndex].SetData(OjwDisp);
                    return true;
                }
                public bool AddData(COjwDisp OjwDisp)
                {
                    int nCnt = (pOjwDisp == null) ? 1 : pOjwDisp.Length + 1;
                    Array.Resize(ref pOjwDisp, nCnt);
                    //pOjwDisp[nCnt - 1] = OjwDisp;
                    pOjwDisp[nCnt - 1] = new COjwDisp();
                    pOjwDisp[nCnt - 1].SetData(OjwDisp);

                    return true;
                }
                public void DeleteAll()
                {
                    if (pOjwDisp != null)
                    {
                        for (int i = 0; i < pOjwDisp.Length; i++)
                            pOjwDisp[i] = null;
                        Array.Resize(ref pOjwDisp, 0);
                    }
                }
                public String GetString_PickingComment(int nGroupA, int nGroupB, int nGroupC)
                {
                    string strComment = null;
                    COjwDisp CDisp = new COjwDisp();
                    for (int i = 0; i < GetCount(); i++)
                    {
                        CDisp = GetData(i);
                        if (CDisp != null)
                        {
                            if ((CDisp.nPickGroup_A == nGroupA) && (CDisp.nPickGroup_B == nGroupB) && (CDisp.nPickGroup_C == nGroupC))
                            {
                                strComment = CDisp.strPickGroup_Comment;
                                break;
                            }
                        }
                    }
                    return strComment;
                }
                #endregion 여기까지

                

                

                
#endif
            }

            public class COjwAse
            {
                public int nModel;
                public COjwAse()
                {
                    afPos = new float[3];
                    m_lstSVec3D = new List<SVector3D_t>();
                    m_lstSSVec3D_Result = new List<SVector3D_t>();
                    m_lstSFace = new List<SPoint3D_t>();
                    m_nCnt = 0;
                    m_nCnt_Result = 0;
                    m_nCnt_Face = 0;
                    nModel = 0;
                }
                private int m_nCnt;
                private int m_nCnt_Result;
                public float[] afPos;
                private List<SVector3D_t> m_lstSVec3D;
                private List<SVector3D_t> m_lstSSVec3D_Result;
                //private SVector3D_t[] SVec3D;
                //private SVector3D_t[] SVec3D_Result;
                //private SPoint3D_t[] SFace;
                private List<SPoint3D_t> m_lstSFace;// = new List<SPoint3D_t>();
                private int m_nCnt_Face;

                public void Data_Type_Set(int nOBJ_ASE)
                {
                    nModel = nOBJ_ASE;
                }
                public int Data_Get_Type() { return nModel; }


#if false
                    SVector3D_t[] aSPos = new SVector3D_t[m_lstOjwAse[nIndex_Ase].Face_GetCnt() * 3];

                    int nPos = 0;

                    int nPos2;
                    SPoint3D_t[] pSData = m_lstOjwAse[nIndex_Ase].Face_Get();
                    foreach (SPoint3D_t SPnt in pSData)
                    {
                        nPos2 = nPos * 3;
                        aSPos[nPos2] = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.x);
                        aSPos[nPos2 + 1] = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.y);
                        aSPos[nPos2 + 2] = m_lstOjwAse[nIndex_Ase].Data_Get(SPnt.z);
                        nPos++;
                    }
#endif

                public void Data_Clear()
                {
                    m_lstSVec3D.Clear();// SVec3D = null;
                    m_lstSSVec3D_Result.Clear();// = null;
                    m_lstSFace.Clear();
                    m_nCnt = 0;
                    m_nCnt_Result = 0;
                    m_nCnt_Face = 0;
                }
                public int Data_GetCnt()
                {
                    return m_nCnt;
                }
                public int Data_Add(float fX, float fY, float fZ)
                {
                    try
                    {
#if false
                        m_nCnt = (SVec3D == null) ? 1 : m_nCnt + 1;
                        Array.Resize(ref SVec3D, m_nCnt);
                        SVec3D[m_nCnt - 1] = new SVector3D_t();
                        SVec3D[m_nCnt - 1].x = fX;
                        SVec3D[m_nCnt - 1].y = fY;
                        SVec3D[m_nCnt - 1].z = fZ;
#else
                        m_lstSVec3D.Add(new SVector3D_t(fX, fY, fZ));                        
                        m_nCnt = m_lstSVec3D.Count;
#endif
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    return m_nCnt;
                }
                public SVector3D_t Data_Get(int nIndex)
                {
                    //if ((nIndex >= 0) && (nIndex < m_nCnt))
                        return m_lstSVec3D[nIndex];
                    //return m_lstSVec3D[0];
                }
                public SVector3D_t [] Data_Get()
                {
                    return m_lstSVec3D.ToArray();
                }
                //public SVector3D_t[] Data_Get()
                //{
                //    return m_lstSVec3D.ToArray();
                //}
                public void Data_Set(int nIndex, float fX, float fY, float fZ)
                {
                    if ((nIndex >= 0) && (nIndex < m_nCnt))
                    {
                        m_lstSVec3D[nIndex] = (new SVector3D_t(fX, fY, fZ));
                    }
                }

                public void DataResult_Clear()
                {
                    m_lstSSVec3D_Result.Clear();// = null;
                    m_nCnt_Result = 0;
                }
                public int DataResult_GetCnt()
                {
                    return m_nCnt_Result;
                }
                public int DataResult_Add(float fX, float fY, float fZ)
                {
                    try
                    {
                        m_lstSSVec3D_Result[m_nCnt_Result - 1] = (new SVector3D_t(fX, fY, fZ));
                        m_nCnt_Result = m_lstSSVec3D_Result.Count;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    return m_nCnt_Result;
                }
                public SVector3D_t[] DataResult_Get() { return m_lstSSVec3D_Result.ToArray(); }
                public SVector3D_t DataResult_Get(int nIndex)
                {
                    if ((nIndex >= 0) && (nIndex < m_nCnt_Result))
                        return m_lstSSVec3D_Result[nIndex];
                    return m_lstSSVec3D_Result[0];
                }
                public void DataResult_Set(int nIndex, float fX, float fY, float fZ)
                {
                    if ((nIndex >= 0) && (nIndex < m_nCnt_Result))
                    {
                        m_lstSSVec3D_Result[nIndex] = (new SVector3D_t(fX, fY, fZ));
                    }
                }

                public int Face_GetCnt()
                {
                    return m_nCnt_Face;
                }

                public int Face_Add(int A, int B, int C)
                {
                    try
                    {
                        m_lstSFace.Add(new SPoint3D_t(A, B, C));
                        m_nCnt_Face = m_lstSFace.Count;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    return m_nCnt_Face;
                }
                public SPoint3D_t[] Face_Get() { return m_lstSFace.ToArray(); }
                public SPoint3D_t Face_Get(int nIndex) { return m_lstSFace[nIndex]; }
                public void Face_Get(int nIndex, out int A, out int B, out int C)
                {
                    if ((nIndex >= 0) && (nIndex < m_nCnt_Face))
                    {
                        //return SFace[nIndex];
                        A = m_lstSFace[nIndex].x;
                        B = m_lstSFace[nIndex].y;
                        C = m_lstSFace[nIndex].z;
                        return;
                    }
                    A = B = C = 0;
                    //return SVec3D[0];
                }
                public void Face_Set(int nIndex, int A, int B, int C)
                {
                    if ((nIndex >= 0) && (nIndex < m_nCnt_Face))
                    {
                        SPoint3D_t SPnt = new SPoint3D_t(A, B, C);
                        m_lstSFace[nIndex] = SPnt;
                    }
                }
                public void Face_Set(int nIndex, SPoint3D_t SPnt)
                {
                    if ((nIndex >= 0) && (nIndex < m_nCnt_Face))
                    {
                        m_lstSFace[nIndex] = SPnt;
                    }
                }
            }
        }
    }
}
