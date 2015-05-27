/* This is a Open Source for Jig tools
 * 
 * made by Jinwook-On (ojw5014@hanmail.net)
 * supported by Daesung-Choi, Ceolhea-yoon, Dongjoon-Chang (Advise)
 * supported by Mohssin (icons)
 */

#region 나중에 참고
#if false
http://blog.daum.net/toyship/112
마우스로 클릭한 위치의 3차원 계산
#endif
#endregion 나중에 참고

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading;

using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform.Windows;
using System.Drawing.Imaging;

#region help - If you want to use my "OpenJigWare.dll"... 
#if false
if you have some errors like this...
'Tao.Platform.Windows.SimpleOpenGlControl' blablablablablabla...
 => 1. You need to copy all DLLs() to the Release folder(freeglut.dll, tao.dll, Tao.FreeGlut.dll, Tao.Ode.dll, Tao.OpenGl.dll, Tao.Platform.Windows.dll, OpenJigWare.dll)
    2. Include almost dll to the Reference(but freeglut.dll) - tao.dll, Tao.FreeGlut.dll, Tao.Ode.dll, Tao.OpenGl.dll, Tao.Platform.Windows.dll, OpenJigWare.dll
#endif
#endregion help - If you want to use my "OpenJigWare.dll"...

// Todo : 
// 1. 디자이너에서 Init, File Open, 마우스 이벤트만 연결하면 되도록 수정할 것. - Done
// 2. 클래스 구분 정의 - Done
// 3. 외부적으로 오브젝트들을 임의 추가가능하도록 기능 추가 - Done
// 4. 모델링 에서 백그라운드 색상 및 빛 계열들도 저장해서 출력시 이게 적용이 되도록 기능 추가.
// 5. 암호화 파일 상태로 저장 및 복원
// 6. 모델링 파일을 숫자가 아닌 문자로 적용되도록... - Done
// 7. 2D 모델의 픽킹 기능 추가
// 8. 2D 모델의 3D 모델링과 유사기능 적용하도록 수정.
// 9. 3D 모델링 데이타 압축 변환기능 추가


// rule 1 : Class => You must set a letter 'C' to the head(But Top class 'Ojw')
// rule 2 : Structure => Set the header letter is 'S', tail letter is '_t'.
// rule 3 : Enum => Set the header letter is 'E', tail letter is '_t'.
// rule 4 : if you make a new class, you should make yours in [User Class,-for example :COjw_19_User.cs]
//          and.. you can change it all names when you made all things completely.(check it from like COjw_00_~)
namespace OpenJigWare
{
    #region Version
    #region Cautions **********************************************************

    #endregion Cautions **********************************************************
    public struct SVersion_T
    {
        public const string strVersion = "01.01.40";
        public const string strHistory = (String)(
                "[V01.01.40]" + "\r\n" +
                " - Fixed some picking bug" + "\r\n" +
                " - Removed glViewPoint() fungtion" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.30]" + "\r\n" +
                " - Invoke feature added for threading" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.20]" + "\r\n" +
                " - 12_3D.cs - Init(Panel -> Control), InitProperty...(Label -> Control)" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.10]" + "\r\n" +
                " - Message Param feature added" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.00]" + "\r\n" +
                " - Joystics features added" + "\r\n" +
                " - COjw_xx_Users.cs file added" + "\r\n" +
                "========================================\r\n" +
                "[V01.00.10]" + "\r\n" +
                " - Debugging and some features are added" + "\r\n" + 
                "========================================\r\n" +
                "[V01.00.00]" + "\r\n" +
                " - build" + "\r\n" +
                "========================================\r\n" +
                "[Build History]\r\n" +
                "========================================");
    }
    #endregion Version

    #region Class
    #region MotorInfo (SMotorInfo_t)
    public struct SMotorInfo_t // Motor information
    {
        public int nMotorID;                    // Motor ID
        public int nMotorDir;                   // Direction of Axis (0 - forward, 1 - inverse)
        public float fLimit_Up;                 // Max Angle(+)
        public float fLimit_Down;               // Min Angle(-)
        public int nCenter_Evd;                 // Pulse(Engineering value for 0 degree(Center Position))

        public int nMechMove;                   // Maximum Position ( Maximum Pulse value(Evd) )
        public float fMechAngle;                // It is a Degree when it moves in Maximum Position

        public float fInitAngle;                // Init position which you want it
        public float fInitAngle2;               // Init position which you want it(2'st)


        // Interference Axis(No use)
        public int nInterference_Axis;          // reserve(No use) - 이게 (-)값이면 간섭 축 없음.
        public float fW;                        // reserve(No use) - Side 에 붙은 축의 크기(넓이)
        public float fInterference_W;           // reserve(No use) - 간섭축이 앞뒤로 붙었다고 가정하고 해당 간섭축의 크기(넓이)

        public float fPos_Right;                // reserve(No use) - 축의 오른쪽 위치
        public float fPos_Left;                 // reserve(No use) - 축의 왼쪽 위치

        public float fInterference_Pos_Front;   // reserve(No use) - 해당 간섭축의 앞쪽 위치
        public float fInterference_Pos_Rear;    // reserve(No use) - 해당 간섭축의 뒤쪽 위치

        // NickName
        public String strNickName;              // Nickname(32char)

        public int nGroupNumber;                // Group Number

        public int nAxis_Mirror;                // 0 ~ 253 : Motor ID of Mirroring one
                                                // -1      : there is no mirror motor.
                                                // -2 : there is no mirror motor(but it can has flip-direction(for using center), flip it from '0')

        public int nMotorControlType;           // Motor Control type => 0: Position, 1: Speed type
    }
    #endregion MotorInfo (SMotorInfo_t)
    #region DH Parameter (SDhT_t, CDhParam, CDhParamAll)
    public class CDhParam
    {
        public CDhParam() // Constructor
        {
            dA = 0;
            dD = 0;
            dTheta = 0;
            nAxisNum = -1;
            nAxisDir = 0;
            strCaption = "";
        }

        public double dA;
        public double dD;
        public double dTheta;
        public double dAlpha;
        public int nAxisNum; // Motor Number(Similar with Virtual ID) : It means there is no motor when it use minus value(-)
        public int nAxisDir; // 0 - Forward, 1 - Inverse

        public string strCaption;
        public void InitData()
        {
            SetData(0, 0, 0, 0, -1, 0, "");
        }
        public void SetData(CDhParam OjwDhParam)
        {
            SetData(OjwDhParam.dA, OjwDhParam.dD, OjwDhParam.dTheta, OjwDhParam.dAlpha, OjwDhParam.nAxisNum, OjwDhParam.nAxisDir, OjwDhParam.strCaption);
        }
        public void SetData(double dDh_a, double dDh_d, double dDh_theta, double dDh_alpha, int nDh_AxisNum, int nDh_AxisDir, string strDh_Caption)
        {
            dA = dDh_a;
            dD = dDh_d;
            dTheta = dDh_theta;
            dAlpha = dDh_alpha;
            nAxisNum = nDh_AxisNum;
            nAxisDir = nDh_AxisDir;
            strCaption = strDh_Caption;
        }
    }
    public class CDhParamAll
    {
        public CDhParamAll() // Constructor
        {
            m_pnDir[0] = 0;
            m_pnDir[1] = 0;
            m_pnDir[2] = 0;
        }
        private CDhParam[] pSDhParam;
        public int GetCount() { return (pSDhParam == null) ? 0 : pSDhParam.Length; }
        private int m_nAxis_X = 0;
        private int m_nAxis_Y = 1;
        private int m_nAxis_Z = 2;
        private int[] m_pnDir = new int[3];
        public void SetAxis_XYZ(int nX, int nX_Dir, int nY, int nY_Dir, int nZ, int nZ_Dir) // Define Motor Axis Number(Default 0, 1, 2)
        {
            if (((nX * nX + nY * nY + nZ * nZ) == 5) && (((nX + 1) * (nY + 1) * (nZ + 1)) == 6))
            {
                if (((nX_Dir >= 0) && (nX_Dir <= 1)) && ((nY_Dir >= 0) && (nY_Dir <= 1)) && ((nZ_Dir >= 0) && (nZ_Dir <= 1)))
                {
                    m_nAxis_X = nX; m_nAxis_Y = nY; m_nAxis_Z = nZ;
                    m_pnDir[m_nAxis_X] = nX_Dir;
                    m_pnDir[m_nAxis_Y] = nY_Dir;
                    m_pnDir[m_nAxis_Z] = nZ_Dir;
                }
            }
        }
        public void GetAxis_XYZ(out int nX, out int nX_Dir, out int nY, out int nY_Dir, out int nZ, out int nZ_Dir)
        {
            nX = m_nAxis_X; nY = m_nAxis_Y; nZ = m_nAxis_Z;
            nX_Dir = m_pnDir[m_nAxis_X];
            nY_Dir = m_pnDir[m_nAxis_Y];
            nZ_Dir = m_pnDir[m_nAxis_Z];
        }
        public CDhParam GetData(int nIndex)
        {
            if ((nIndex >= pSDhParam.Length) || (nIndex < 0)) return null;
            else return pSDhParam[nIndex];
        }
        public bool SetData(int nIndex, CDhParam OjwDhParam)
        {
            if ((nIndex >= pSDhParam.Length) || (nIndex < 0)) return false;

            pSDhParam[nIndex].SetData(OjwDhParam);
            return true;
        }
        public bool AddData(CDhParam OjwDhParam)
        {
            int nCnt = (pSDhParam == null) ? 1 : pSDhParam.Length + 1;
            Array.Resize(ref pSDhParam, nCnt);
            pSDhParam[nCnt - 1] = new CDhParam();
            pSDhParam[nCnt - 1].SetData(OjwDhParam);

            return true;
        }
        public void DeleteAll()
        {
            if (pSDhParam != null)
            {
                for (int i = 0; i < pSDhParam.Length; i++)
                    pSDhParam[i] = null;
                Array.Resize(ref pSDhParam, 0);
            }
        }
    }
    #endregion DH Parameter (SDhT_t, CDhParam, CDhParamAll)
    #endregion Class

    public partial class Ojw
    {
        #region Define(private)
        private class CDef
        {
            public static int _CHKSUM_NONE = 0;
            public static int _CHKSUM_AND = 1;
            public static int _CHKSUM_OR = 2;
            public static int _CHKSUM_XOR = 3;
            public static int _CHKSUM_SUM = 4;
        }
        #endregion Define    
    }
}
