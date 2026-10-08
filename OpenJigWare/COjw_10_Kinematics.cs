//#define _REMOVE_CLR_COMMAND // 변수 선언뒤에 CLR 을 사용하는 문법을 삭제
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;


#if _USING_DOTNET_3_5
#elif _USING_DOTNET_2_0
#else
// 참고소스 : http://www.secretgeek.net/host_ironpython
// IronPython.dll, IronPython.Modules.dll, Microsoft.Scripting.dll 을 참조해야 함.
using IronPython.Hosting;
using IronPython.Modules;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
//using Microsoft.Scripting

using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using System.Text.RegularExpressions;
//using IronPython.Runtime;
//using IronPython.Runtime.Operations;
//using IronPython.Runtime.Types;
//using System.Collections;

//using OjwPythonModules;
#endif

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CKinematics // Forward / Inverse
        {
            public class CForward
            {
                #region Class Control - DHParam
                public static string[] Make_XYZString_By_Forward(string strDH)
                {
                    return Make_XYZString_By_Forward(strDH, null);
                }
                private static double[] m_adColX = new double[3];
                private static double[] m_adColY = new double[3];
                private static double[] m_adColZ = new double[3];
                public static string[] Make_XYZString_By_Forward(string strDH, float [] afMotors)
                {
                    int i;
                    CDhParamAll COjwDhParamAll = new CDhParamAll();
                    Ojw.CKinematics.CForward.MakeDhParam(strDH, out COjwDhParamAll);
                    double dX, dY, dZ;
                    double[] dcolX;
                    double[] dcolY;
                    double[] dcolZ;

                    double[] adMot = new double[256];
                    Array.Clear(adMot, 0, adMot.Length);
                    if (afMotors == null) for (i = 0; i < 256; i++) adMot[i] = 0;// (double)OjwC3d.GetData(i);
                    else for (i = 0; i < afMotors.Length; i++) adMot[i] = afMotors[i];// (double)OjwC3d.GetData(i);

                    Ojw.CKinematics.CForward.CalcKinematics(COjwDhParamAll, adMot, out dcolX, out dcolY, out dcolZ, out dX, out dY, out dZ);
                    String strResult;
                    Ojw.CKinematics.CForward.CalcKinematics_ToString(COjwDhParamAll, adMot, out strResult);

                    string[] pstrResult = strResult.Split('\n');
                    string strRes = String.Empty;
                    int nIndex;
                    bool bRet = true;
                    for (int k = 0; k < 3; k++)
                    {
                        nIndex = k + pstrResult.Length - 3 - 1 - 3;
                        if ((nIndex < 0) || (nIndex >= pstrResult.Length))
                        {
                            strRes += "\r\n";
                            bRet = false;
                            continue;
                        }
                        // 뒤의 3개는 설명이라 그것 마저 제하면 -7 이 된다.
                        string strDatas = Ojw.CConvert.RemoveChar(pstrResult[nIndex], '\r');
                        string[] pstrDatas = strDatas.Split(' ');
                        int nPass = 0;
                        foreach (string strItem in pstrDatas)
                        {
                            if (strItem.Length > 0)
                            {
                                if (nPass < 3)
                                {
                                }
                                else
                                {
                                    strRes += pstrDatas[pstrDatas.Length - nPass - 1] + "\n";
                                }
                                nPass++;
                            }
                        }
                        strRes += pstrDatas[pstrDatas.Length - 1] + "\n";
                    }
#if false
                    #region Checking Direction(Vector)
            // 방향 확인
            float[] afX = new float[3];
            float[] afY = new float[3];
            float[] afZ = new float[3];

            double dLength = 1.0;
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
                    #endregion Checking Direction(Vector)
#endif
                    //i = 0;
                    //strRes += String.Format("Dir[{0}]: {1}, {2}, {3}\n", i, (float)Math.Round(dcolX[0], 1), (float)Math.Round(dcolX[1], 1), (float)Math.Round(dcolX[2], 1));

                    //i = 1;
                    //strRes += String.Format("Dir[{0}]: {1}, {2}, {3}\n", i, (float)Math.Round(dcolY[0], 1), (float)Math.Round(dcolY[1], 1), (float)Math.Round(dcolY[2], 1));

                    //i = 2;
                    //strRes += String.Format("Dir[{0}]: {1}, {2}, {3}\n", i, (float)Math.Round(dcolZ[0], 1), (float)Math.Round(dcolZ[1], 1), (float)Math.Round(dcolZ[2], 1));


                    strRes += String.Format("{0},{1},{2}\n", (float)Math.Round(dcolX[0], 1), (float)Math.Round(dcolX[1], 1), (float)Math.Round(dcolX[2], 1));
                    strRes += String.Format("{0},{1},{2}\n", (float)Math.Round(dcolY[0], 1), (float)Math.Round(dcolY[1], 1), (float)Math.Round(dcolY[2], 1));
                    strRes += String.Format("{0},{1},{2}\n", (float)Math.Round(dcolZ[0], 1), (float)Math.Round(dcolZ[1], 1), (float)Math.Round(dcolZ[2], 1));

                    //OjwC3d.SetTestDh_Angle(afX, afY, afZ);

                    List<String> lstRes = new List<string>();
                    lstRes.Clear();
                    string[] pstrRes = strRes.Split('\n');
                    for (i = 0; i < pstrRes.Length; i++)
                    {
                        if (pstrRes[i].Length > 0)
                        {
                            if (bRet == true) lstRes.Add(pstrRes[i]);
                            else lstRes.Add("");
                        }
                    }
                    return lstRes.ToArray();
                    //return strRes;
                }

                private static bool CalcDhParamAll_ToString(CDhParamAll DhParamAll, double[] adAxisValue, out String strResult)
                {
                    int i;
                    int nCnt = DhParamAll.GetCount();

                    strResult = "";
                    SDhT_t[] aSDhT = new SDhT_t[nCnt];
                    SDhT_t SDhT_Result = new SDhT_t();

                    if (nCnt <= 0)
                    {
                        strResult = "Error";
                        return false;
                    }

                    SDhT_Str_t[] aSDhT_Str = new SDhT_Str_t[nCnt];
                    SDhT_Str_t SDhT_Result_Str = new SDhT_Str_t();
                    // Initialize
                    CMath.CalcT(0, 0, 0, 0, out SDhT_Result.adT);
                    CMath.CalcT_Str(0, 0, 0, 0, null, out SDhT_Result_Str.aStrT);
                    for (int k = 0; k < 4; k++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            strResult += SDhT_Result_Str.aStrT[k, j] + ",";
                        }
                        strResult += "\r\n";
                    }
                    strResult += "// Start //\r\n==============\r\n";

                    double dTheta;
                    double dD;
                    CDhParam DhParam = new CDhParam();
                    double dAngleData = 0.0f;
                    //bool bNamed = false;
                    int nPos = 0;
                    for (i = 0; i < nCnt; i++)
                    {
                        bool bInv = false;
                        DhParam = DhParamAll.GetData(i);
                        //if (
                        //    (DhParam.dA == 0) &&
                        //    (DhParam.dD == 0) &&
                        //    (DhParam.dTheta == 0) &&
                        //    (DhParam.dAlpha == 0)
                        //    ) continue;
                        #region (dAngleData)Read Value of angle(Kor: 각도값 읽어오기)
                        //try
                        //{
                        //    if ((DhParam.nAxisNum >= 0) && (DhParam.nAxisNum < adAxisValue.Length)) bNamed = true;
                        //    else bNamed = false;
                        //}
                        //catch { dAngleData = 0; }
                        #endregion (fAngleData)Read Value of angle(Kor: 각도값 읽어오기)
#if false // CalcDhParamAll_ToString 에 Theta 가 아닌 D 값이 모터 변경값일 경우의 수식 적용 되도록 수정
                        dTheta = DhParam.dTheta + (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 0) ? 1.0f : -1.0f));
#else
                        // Theta
                        dTheta = DhParam.dTheta;
                        dD = DhParam.dD;
                        if ((DhParam.nAxisDir < 2) || (DhParam.nAxisDir >= 4)) // Dir 0,1: revolute, Dir 4,5: 바퀴형(continuous revolute)
                        {
                            dTheta += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * (((DhParam.nAxisDir == 0) || (DhParam.nAxisDir == 4)) ? 1.0f : -1.0f));
                            if ((DhParam.nAxisDir != 0) && (DhParam.nAxisDir != 4))
                            {
                                bInv = true;
                            }
                        }
                        // D
                        else // Dir 2,3: prismatic
                        {
                            dD += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 2) ? 1.0f : -1.0f));
                            if (DhParam.nAxisDir != 2)
                            {
                                bInv = true;
                            }
                        }
#endif                                             
                        //CMath.CalcT(DhParam.dA, DhParam.dAlpha, DhParam.dD, dTheta, out aSDhT[i].adT);
                        CMath.CalcT(DhParam.dA, DhParam.dAlpha, dD, dTheta, out aSDhT[i].adT);
                        //CMath.CalcT_Str(DhParam.dA, DhParam.dAlpha, DhParam.dD, DhParam.dTheta, ((DhParam.nAxisNum >= 0) ? "t" + CConvert.IntToStr(DhParam.nAxisNum) : ""), out aSDhT_Str[i].aStrT);
                        CMath.CalcT_Str(DhParam.dA, DhParam.dAlpha, DhParam.dD, DhParam.dTheta, ((bInv == true)?"-" : "") + ((DhParam.nAxisNum >= 0) ? "t" + CConvert.IntToStr(DhParam.nAxisNum) : ""), out aSDhT_Str[i].aStrT);
                        for (int k = 0; k < 4; k++)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                strResult += aSDhT_Str[i].aStrT[k, j] + ",";
                            }
                            strResult += "\r\n";
                        }
                        strResult += "// --" + CConvert.IntToStr(nPos++) + "//\r\n==============\r\n";

                    }
                    DhParam = null;
#if false
                    return true;
#else
                    // 첫 부분 계산
        //             CMath.CalcMatrix(4, SDhT_Result.afT, aSDhT[0].afT, out SDhT_Result.afT);
        //             for (i = 1; i < nCnt; i++)
        //                 CMath.CalcMatrix(4, SDhT_Result.afT, aSDhT[i].afT, out SDhT_Result.afT);

                    CMath.CalcMatrix_Str(4, SDhT_Result_Str.aStrT, aSDhT_Str[0].aStrT, out SDhT_Result_Str.aStrT);
                    for (i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            strResult += SDhT_Result_Str.aStrT[i, j] + ",";
                        }
                        strResult += "\r\n";
                    }
                    strResult += "// -- First Calc //\r\n==============\r\n";
                    bool bOverflow = false;
                    for (i = 1; i < nCnt; i++)
                    {
                        CMath.CalcMatrix_Str(4, SDhT_Result_Str.aStrT, aSDhT_Str[i].aStrT, out SDhT_Result_Str.aStrT);
                        for (int ii = 0; ii < 4; ii++)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                strResult += SDhT_Result_Str.aStrT[ii, j] + "   ";
                            }
                            if (strResult.Length > 10000000)
                            {
                                CMessage.Write("strResult.Length = " + strResult.Length + " , Memory Overflow");
                                strResult += "\r\n...(huge memory string)\r\n";
                                bOverflow = true;
                            }
                            if (bOverflow == true) break;
                            strResult += "\r\n";
                        }
                        if (bOverflow == true) break;
                        strResult += "// " + CConvert.IntToStr(i) + " //\r\n==============\r\n";
                    }

        //             int nDirX;// = 0;
        //             int nDirY;// = 1;
        //             int nDirZ;// = 2;
        //             int nAxisDir_X;
        //             int nAxisDir_Y;
        //             int nAxisDir_Z;
        //             DhParamAll.GetAxis_XYZ(out nDirX, out nAxisDir_X, out nDirY, out nAxisDir_Y, out nDirZ, out nAxisDir_Z);
        //             fX = SDhT_Result.afT[nDirX, 3] * ((nAxisDir_X == 0) ? 1 : -1);
        //             fY = SDhT_Result.afT[nDirY, 3] * ((nAxisDir_Y == 0) ? 1 : -1);
        //             fZ = SDhT_Result.afT[nDirZ, 3] * ((nAxisDir_Z == 0) ? 1 : -1);

                    for (i = 0; i < nCnt; i++) aSDhT_Str[i].aStrT = null;
                    aSDhT_Str = null;
                    SDhT_Result_Str.aStrT = null;

                    return true;
#endif
                }

                private static bool CalcDhParamAll(Ojw.C3d.COjwDesignerHeader CHeader, int nFunctionNumber, double[] adAxisValue, out double dX, out double dY, out double dZ)
                {
#if true
                    double[] dColX;
                    double[] dColY;
                    double[] dColZ;
                    double dDir_X, dDir_Y, dDir_Z;
                    return CalcDhParamAll(CHeader.pDhParamAll[nFunctionNumber], adAxisValue, out dColX, out dColY, out dColZ, out dX, out dY, out dZ, out dDir_X, out dDir_Y, out dDir_Z);
#else
                    //dX = dY = dZ = 0;
                    // DH 를 사용하는 대신 Function 을 사용하는 함수


                    // 집어넣기 전에 내부 메모리를 클리어 한다.
                    Ojw.CKinematics.CInverse.SetValue_ClearAll(ref CHeader.pSOjwCode[nFunctionNumber]);
                    //Ojw.CKinematics.CInverse.SetValue_X(dX);
                    //Ojw.CKinematics.CInverse.SetValue_Y(dY);
                    //Ojw.CKinematics.CInverse.SetValue_Z(dZ);

                    // 현재의 모터각을 전부 집어 넣도록 한다.
                    for (int i = 0; i < adMot.Length; i++)
                    {
                        // 모터값을 3D에 넣어주고
                        //SetData(i, Ojw.CConvert.StrToFloat(m_txtAngle[i].Text));
                        // 그 값을 꺼내 수식 계산에 넣어준다.
                        Ojw.CKinematics.CInverse.SetValue_Motor(i, adMot[i]);
                    }

                    // 실제 수식계산
                    Ojw.CKinematics.CInverse.CalcCode(ref CHeader.pSOjwCode[nFunctionNumber]);

                    dX = Ojw.CKinematics.CInverse.GetValue_X();
                    dY = Ojw.CKinematics.CInverse.GetValue_Y();
                    dZ = Ojw.CKinematics.CInverse.GetValue_Z();

                    return true;
#endif
                }
                // [0~255] After one of the step sizes DhParamAll variables into the function, the function receives the value of the result to x,y,z(Kor: 0~255 에 해당하는 DhParamAll 변수중 하나를 택해 함수에 넣은 후 그 결과 값을 x,y,z 로 받는 함수)
                // afAxisValue shall be to put the value of the motor(Put the Motor ID(DhParam.nAxisNum) into Header...).(Kor: afAxisValue 에는 모터의 값(모터의 ID(DhParam.nAxisNum)는 헤더에 기록...)을 넣도록 한다.)
                private static bool CalcDhParamAll(CDhParamAll DhParamAll, double[] adAxisValue, out double dX, out double dY, out double dZ)
                {
#if true
                    double[] dColX;
                    double[] dColY;
                    double[] dColZ;
                    double dDir_X, dDir_Y, dDir_Z;
                    return CalcDhParamAll(DhParamAll, adAxisValue, out dColX, out dColY, out dColZ, out dX, out dY, out dZ, out dDir_X, out dDir_Y, out dDir_Z);
#else
                    int i;//, j;
                    int nCnt = DhParamAll.GetCount();

                    SDhT_t[] aSDhT = new SDhT_t[nCnt];
                    SDhT_t SDhT_Result = new SDhT_t();
                    if (nCnt <= 0)
                    {
                        dX = dY = dZ = 0.0f;
                        return false;
                    }

                    // Initialize
                    CMath.CalcT(0, 0, 0, 0, out SDhT_Result.adT);

                    double dTheta, dA, dD;
                    CDhParam DhParam = new CDhParam();
                    double dAngleData = 0.0f;
                    for (i = 0; i < nCnt; i++)
                    {
                        DhParam = DhParamAll.GetData(i);
                        #region (dAngleData)Read Angle value(Kor: 각도값 읽어오기)
                        try
                        {
                            if ((DhParam.nAxisNum >= 0) && (DhParam.nAxisNum < adAxisValue.Length)) dAngleData = adAxisValue[DhParam.nAxisNum];
                            else dAngleData = 0;
                        }
                        catch { dAngleData = 0; }
                        #endregion (fAngleData)Read Angle value(Kor: 각도값 읽어오기)
                        //dTheta = DhParam.dTheta + (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 0) ? 1.0f : -1.0f));
                        dTheta = DhParam.dTheta;
                        dA = DhParam.dA;
                        dD = DhParam.dD;
                        // Theta
                        if ((DhParam.nAxisDir < 2) || (DhParam.nAxisDir >= 4)) dTheta += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * (((DhParam.nAxisDir == 0) || (DhParam.nAxisDir == 4)) ? 1.0f : -1.0f)); // Dir 0,1,4,5: revolute/continuous
                        // D
                        else dD += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 2) ? 1.0f : -1.0f)); // Dir 2,3: prismatic
                        CMath.CalcT(dA, DhParam.dAlpha, dD, dTheta, out aSDhT[i].adT);
                    }
                    DhParam = null;

                    // First Point(Kor: 첫 부분 계산)
                    CMath.CalcMatrix(4, SDhT_Result.adT, aSDhT[0].adT, out SDhT_Result.adT);
                    for (i = 1; i < nCnt; i++)
                        CMath.CalcMatrix(4, SDhT_Result.adT, aSDhT[i].adT, out SDhT_Result.adT);

                    int nDirX;// = 0;
                    int nDirY;// = 1;
                    int nDirZ;// = 2;
                    int nAxisDir_X;
                    int nAxisDir_Y;
                    int nAxisDir_Z;
                    DhParamAll.GetAxis_XYZ(out nDirX, out nAxisDir_X, out nDirY, out nAxisDir_Y, out nDirZ, out nAxisDir_Z);
                    dX = SDhT_Result.adT[nDirX, 3] * ((nAxisDir_X == 0) ? 1 : -1);
                    dY = SDhT_Result.adT[nDirY, 3] * ((nAxisDir_Y == 0) ? 1 : -1);
                    dZ = SDhT_Result.adT[nDirZ, 3] * ((nAxisDir_Z == 0) ? 1 : -1);

                    for (i = 0; i < nCnt; i++) aSDhT[i].adT = null;
                    aSDhT = null;
                    SDhT_Result.adT = null;

                    return true;
#endif
                }

                private static int m_nLimit_FunctionNumber = -1;
                private static int m_nLimit_Axis = -1;
                private static int m_nLimit_Index = -1;
                private static bool m_bLimit_Oneshot = true;
                // SetCalcLimit_Axis( 3 ) 이렇게 하면 3번 모터가 발견된 행렬까지만 forward kinematics 계산을 한다.
                public static void SetCalcLimit_Axis(int nAxis, bool bOneshot = true) { m_nLimit_Axis = nAxis; m_bLimit_Oneshot = bOneshot; }
                public static void SetCalcLimit_FunctionNumber(int nNumber, bool bOneshot = true) { m_nLimit_FunctionNumber = nNumber; m_bLimit_Oneshot = bOneshot; }
                // SetCalcLimit( 3 ) 이렇게 하면 3번째 행렬까지만 forward kinematics 계산을 한다.
                public static void SetCalcLimit(int nDepthIndex, bool bOneshot = true) { m_nLimit_Index = nDepthIndex; m_bLimit_Oneshot = bOneshot; }
                // CalcDhParamAll 이후 방향벡터를 가져올 수 있다.
                public static double[] GetDirectionVectors_after_Forward(int nX_0_Y_1_Z_2)
                {
                    return ((nX_0_Y_1_Z_2 == 1) ? m_adColY : ((nX_0_Y_1_Z_2 == 2) ? m_adColZ : m_adColX));
                }
                private static int m_nCalcResult = 0; // 0: None, 1: Limit ID, 2: Limit Function
                public static int CheckCalc() { return m_nCalcResult; }
                public static void CheckCalc_Reset() { m_nCalcResult = 0; }

                private static bool CalcDhParamAll(CDhParamAll DhParamAll, double[] adAxisValue, out double[] dColX, out double[] dColY, out double[] dColZ, out double dX, out double dY, out double dZ, out double dDir_X, out double dDir_Y, out double dDir_Z)
                {
                    m_nCalcResult = 0;

                    int i, j;
                    int nCnt = DhParamAll.GetCount();
                    int nCnt2 = -1;

                    SDhT_t[] aSDhT = new SDhT_t[nCnt];
                    SDhT_t SDhT_Result = new SDhT_t();

                    dColX = new double[3];
                    dColY = new double[3];
                    dColZ = new double[3];
                    Array.Clear(dColX, 0, dColX.Length);
                    Array.Clear(dColY, 0, dColY.Length);
                    Array.Clear(dColZ, 0, dColZ.Length);
                    /////////////////////////////////////////
                    for (i = 0; i < m_adColX.Length; i++)
                    {
                        m_adColX[i] = dColX[i];
                        m_adColY[i] = dColY[i];
                        m_adColZ[i] = dColZ[i];
                    }
                    /////////////////////////////////////////
                    

                    if (nCnt <= 0)
                    {
                        dX = dY = dZ = 0.0;
                        dDir_X = dDir_Y = dDir_Z = 0.0f;
                        return false;
                    }

                    if (m_nLimit_Index >= 0)
                    {
                        if (nCnt > m_nLimit_Index) nCnt = m_nLimit_Index + 1;
                        else m_nLimit_Index = -1;
                    }
                    
                    // Initialize
                    CMath.CalcT(0, 0, 0, 0, out SDhT_Result.adT);

                    //double dTheta;
                    double dTheta, dA, dD;
                    CDhParam DhParam = new CDhParam();
                    double dAngleData = 0.0;

                    int nCnt_Limit = -1;
                    for (i = 0; i < nCnt; i++)
                    {
                        DhParam = DhParamAll.GetData(i);


                        #region (dAngleData)Read Angle value(Kor: 각도값 읽어오기)
                        try
                        {
                            if ((DhParam.nAxisNum >= 0) && (DhParam.nAxisNum < adAxisValue.Length)) dAngleData = adAxisValue[DhParam.nAxisNum];
                            else dAngleData = 0;

                            if (m_nLimit_Axis >= 0)
                            {
                                if (DhParam.nAxisNum >= 0)
                                {
                                    if (DhParam.nAxisNum == m_nLimit_Axis)
                                    {
                                        nCnt2 = i;
                                    }
                                }
                            }
                        }
                        catch { dAngleData = 0; }
                        #endregion (dAngleData)Read Angle value(Kor: 각도값 읽어오기)
                        //dTheta = DhParam.dTheta + (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 0) ? 1.0f : -1.0f));
                        //CMath.CalcT(DhParam.dA, DhParam.dAlpha, DhParam.dD, dTheta, out aSDhT[i].adT);
                        dTheta = DhParam.dTheta;
                        dA = DhParam.dA;
                        dD = DhParam.dD;
                        // Theta
                        if ((DhParam.nAxisDir < 2) || (DhParam.nAxisDir >= 4)) dTheta += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * (((DhParam.nAxisDir == 0) || (DhParam.nAxisDir == 4)) ? 1.0f : -1.0f)); // Dir 0,1,4,5: revolute/continuous
                        // D
                        else dD += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 2) ? 1.0f : -1.0f)); // Dir 2,3: prismatic
                        CMath.CalcT(dA, DhParam.dAlpha, dD, dTheta, out aSDhT[i].adT);

                        if (nCnt2 >= 0)
                        {
                            m_nCalcResult = 1;
                            break;
                        }
#if true
                        if (m_nLimit_FunctionNumber >= 0)// && (m_nLimit_FunctionNumber != 255))
                        {
                            if (DhParam.nFunctionNumber == m_nLimit_FunctionNumber)
                            {
                                nCnt_Limit = i + 1;
                                m_nCalcResult = 2;
                                break; // 수식번호가 있는것이면 여기서 계산 종료
                            }
                        }
#else
                        if (m_nLimit_FunctionNumber >= 0)// && (m_nLimit_FunctionNumber != 255))
                        {
                            if (DhParam.nFunctionNumber >= 0)
                            {
                                nCnt_Limit = i + 1;
                                m_nCalcResult = 2;
                                break; // 수식번호가 있는것이면 여기서 계산 종료
                            }
                        }
#endif
                    }
                    if (nCnt_Limit >= 0)
                    {
                        nCnt = nCnt_Limit;
                    }
                    DhParam = null;

                    if (nCnt2 >= 0) nCnt = nCnt2;
                    if (m_bLimit_Oneshot)
                    {
                        m_nLimit_FunctionNumber = -1;
                        m_nLimit_Axis = -1;
                        m_nLimit_Index = -1;
                    }

                    // First point(Kor: 첫 부분 계산)
                    CMath.CalcMatrix(4, SDhT_Result.adT, aSDhT[0].adT, out SDhT_Result.adT);
                    for (i = 1; i < nCnt; i++)
                        CMath.CalcMatrix(4, SDhT_Result.adT, aSDhT[i].adT, out SDhT_Result.adT);

                    // Vector for direction(Kor: 방향벡터 계산)
                    for (i = 0; i < 3; i++)
                    {
                        dColX[i] = SDhT_Result.adT[i, 0];
                        dColY[i] = SDhT_Result.adT[i, 1];
                        dColZ[i] = SDhT_Result.adT[i, 2];
                    }
                    int nDirX = 0;
                    int nDirY = 1;
                    int nDirZ = 2;
                    dX = SDhT_Result.adT[nDirX, 3];
                    dY = SDhT_Result.adT[nDirY, 3];
                    dZ = SDhT_Result.adT[nDirZ, 3];

                    // Checking Direction(Kor: 방향 확인)
                    j = 0;
                    double[] adDir = new double[3];
                    for (j = 0; j < 3; j++) // Axis
                    {
                        int nAxisDirection = ((j == 0) ? nDirX : ((j == 1) ? nDirY : nDirZ));
                        for (i = 0; i < 3; i++)
                        {
                            if ((int)Math.Round(SDhT_Result.adT[i, nAxisDirection], 0) != 0) adDir[i] = (int)Math.Round(SDhT_Result.adT[i, nAxisDirection], 0) * (nAxisDirection + 1);
                        }
                    }
                    dDir_X = adDir[0];
                    dDir_Y = adDir[1];
                    dDir_Z = adDir[2];


                    for (i = 0; i < nCnt; i++) aSDhT[i].adT = null;
                    aSDhT = null;
                    SDhT_Result.adT = null;

                    /////////////////////////////////////////
                    for (i = 0; i < m_adColX.Length; i++)
                    {
                        m_adColX[i] = dColX[i];
                        m_adColY[i] = dColY[i];
                        m_adColZ[i] = dColZ[i];
                    }
                    /////////////////////////////////////////
                    
                    return true;
                }
                public static bool CalcDhParamAll_LastOf(CDhParamAll DhParamAll, double[] adAxisValue, out double[] dColX, out double[] dColY, out double[] dColZ, out double dX, out double dY, out double dZ, out double dDir_X, out double dDir_Y, out double dDir_Z)
                {
                    int i, j;
                    int nCnt = DhParamAll.GetCount();
                    int nCnt2 = -1;

                    SDhT_t[] aSDhT = new SDhT_t[nCnt];
                    SDhT_t SDhT_Result = new SDhT_t();

                    dColX = new double[3];
                    dColY = new double[3];
                    dColZ = new double[3];
                    Array.Clear(dColX, 0, dColX.Length);
                    Array.Clear(dColY, 0, dColY.Length);
                    Array.Clear(dColZ, 0, dColZ.Length);
                    /////////////////////////////////////////
                    for (i = 0; i < m_adColX.Length; i++)
                    {
                        m_adColX[i] = dColX[i];
                        m_adColY[i] = dColY[i];
                        m_adColZ[i] = dColZ[i];
                    }
                    /////////////////////////////////////////
                    

                    if (nCnt <= 0)
                    {
                        dX = dY = dZ = 0.0;
                        dDir_X = dDir_Y = dDir_Z = 0.0f;
                        return false;
                    }

                    if (m_nLimit_Index >= 0)
                    {
                        if (nCnt > m_nLimit_Index) nCnt = m_nLimit_Index + 1;
                        else m_nLimit_Index = -1;
                    }
                    
                    // Initialize
                    CMath.CalcT(0, 0, 0, 0, out SDhT_Result.adT);

                    //double dTheta;
                    double dTheta, dA, dD;
                    CDhParam DhParam = new CDhParam();
                    double dAngleData = 0.0;

                    int nCnt_Limit = -1;
                    for (i = 0; i < nCnt; i++)
                    {
                        DhParam = DhParamAll.GetData(nCnt - i - 1);
                        
                        #region (dAngleData)Read Angle value(Kor: 각도값 읽어오기)
                        try
                        {
                            if ((DhParam.nAxisNum >= 0) && (DhParam.nAxisNum < adAxisValue.Length)) dAngleData = adAxisValue[DhParam.nAxisNum];
                            else dAngleData = 0;

                            if (m_nLimit_Axis >= 0)
                            {
                                if (DhParam.nAxisNum >= 0)
                                {
                                    if (DhParam.nAxisNum == m_nLimit_Axis)
                                    {
                                        nCnt2 = i;
                                    }
                                }
                            }
                        }
                        catch { dAngleData = 0; }
                        #endregion (dAngleData)Read Angle value(Kor: 각도값 읽어오기)
                        //dTheta = DhParam.dTheta + (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 0) ? 1.0f : -1.0f));
                        //CMath.CalcT(DhParam.dA, DhParam.dAlpha, DhParam.dD, dTheta, out aSDhT[i].adT);
                        dTheta = DhParam.dTheta;
                        dA = DhParam.dA;
                        dD = DhParam.dD;
                        // Theta
                        if ((DhParam.nAxisDir < 2) || (DhParam.nAxisDir >= 4)) dTheta += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * (((DhParam.nAxisDir == 0) || (DhParam.nAxisDir == 4)) ? 1.0f : -1.0f)); // Dir 0,1,4,5: revolute/continuous
                        // D
                        else dD += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 2) ? 1.0f : -1.0f)); // Dir 2,3: prismatic
                        CMath.CalcT(dA, DhParam.dAlpha, dD, dTheta, out aSDhT[i].adT);

                        if (nCnt2 >= 0) break;

                        if (m_nLimit_FunctionNumber >= 0)// && (m_nLimit_FunctionNumber != 255))
                        {
                            if (DhParam.nFunctionNumber >= 0)
                            {
                                nCnt_Limit = i + 1;
                                break; // 수식번호가 있는것이면 여기서 계산 종료
                            }
                        }
                    }
                    if (nCnt_Limit >= 0)
                    {
                        nCnt = nCnt_Limit;
                    }
                    DhParam = null;

                    if (nCnt2 >= 0) nCnt = nCnt2;
                    if (m_bLimit_Oneshot)
                    {
                        m_nLimit_FunctionNumber = -1;
                        m_nLimit_Axis = -1;
                        m_nLimit_Index = -1;
                    }

                    // First point(Kor: 첫 부분 계산)
                    CMath.CalcMatrix(4, SDhT_Result.adT, aSDhT[0].adT, out SDhT_Result.adT);
                    for (i = 1; i < nCnt; i++)
                        CMath.CalcMatrix(4, SDhT_Result.adT, aSDhT[i].adT, out SDhT_Result.adT);

                    // Vector for direction(Kor: 방향벡터 계산)
                    for (i = 0; i < 3; i++)
                    {
                        dColX[i] = SDhT_Result.adT[i, 0];
                        dColY[i] = SDhT_Result.adT[i, 1];
                        dColZ[i] = SDhT_Result.adT[i, 2];
                    }
                    int nDirX = 0;
                    int nDirY = 1;
                    int nDirZ = 2;
                    dX = SDhT_Result.adT[nDirX, 3];
                    dY = SDhT_Result.adT[nDirY, 3];
                    dZ = SDhT_Result.adT[nDirZ, 3];

                    // Checking Direction(Kor: 방향 확인)
                    j = 0;
                    double[] adDir = new double[3];
                    for (j = 0; j < 3; j++) // Axis
                    {
                        int nAxisDirection = ((j == 0) ? nDirX : ((j == 1) ? nDirY : nDirZ));
                        for (i = 0; i < 3; i++)
                        {
                            if ((int)Math.Round(SDhT_Result.adT[i, nAxisDirection], 0) != 0) adDir[i] = (int)Math.Round(SDhT_Result.adT[i, nAxisDirection], 0) * (nAxisDirection + 1);
                        }
                    }
                    dDir_X = adDir[0];
                    dDir_Y = adDir[1];
                    dDir_Z = adDir[2];


                    for (i = 0; i < nCnt; i++) aSDhT[i].adT = null;
                    aSDhT = null;
                    SDhT_Result.adT = null;

                    /////////////////////////////////////////
                    for (i = 0; i < m_adColX.Length; i++)
                    {
                        m_adColX[i] = dColX[i];
                        m_adColY[i] = dColY[i];
                        m_adColZ[i] = dColZ[i];
                    }
                    /////////////////////////////////////////
                    
                    return true;
                }
                public static bool CalcKinematics_ToString(CDhParamAll DhParamAll, float[] afAxisValue, out String strResult)
                {
                    //float[][] floats = mtx.Select(r => r.Select(Convert.ToSingle).ToArray()).ToArray();
                    double[] adAxisValue = Array.ConvertAll(afAxisValue, element => (double)element);
                    return CalcKinematics_ToString(DhParamAll, adAxisValue, out strResult);
                }
                public static bool CalcKinematics_ToString(CDhParamAll DhParamAll, double[] adAxisValue, out String strResult)
                {
                    //dX = dY = dZ = 0.0;
                    strResult = "";
                    if (DhParamAll == null) return false;
                    if (DhParamAll.GetCount() <= 0) return false;
                    CalcDhParamAll_ToString(DhParamAll, adAxisValue, out strResult);
                    return true;
                }
                public static bool CalcKinematics(CDhParamAll DhParamAll, float[] afAxisValue, out float fX, out float fY, out float fZ)
                {
                    double[] adAxisValue = Array.ConvertAll(afAxisValue, element => (double)element);

                    double dX, dY, dZ;
                    bool bRet = CalcKinematics(DhParamAll, adAxisValue, out dX, out dY, out dZ);
                    fX = (float)dX;
                    fY = (float)dY;
                    fZ = (float)dZ;
                    return bRet;
                }
                public static bool CalcKinematics(CDhParamAll DhParamAll, double[] adAxisValue, out double dX, out double dY, out double dZ)
                {
                    dX = dY = dZ = 0.0;
                    if (DhParamAll == null) return false;
                    if (DhParamAll.GetCount() <= 0) return false;
                    CalcDhParamAll(DhParamAll, adAxisValue, out dX, out dY, out dZ);
                    return true;
                }
                public static bool CalcKinematics(CDhParamAll DhParamAll, float[] afAxisValue, out float[] afColX, out float[] afColY, out float[] afColZ, out float fX, out float fY, out float fZ)
                {
                    double[] adAxisValue = Array.ConvertAll(afAxisValue, element => (double)element);
                    double dX, dY, dZ;
                    double[] adColX;
                    double[] adColY;
                    double[] adColZ;
                    bool bRet = CalcKinematics(DhParamAll, adAxisValue, out adColX, out adColY, out adColZ, out dX, out dY, out dZ);
                    fX = (float)dX;
                    fY = (float)dY;
                    fZ = (float)dZ;

                    afColX = Array.ConvertAll(adColX, element => (float)element);
                    afColY = Array.ConvertAll(adColY, element => (float)element);
                    afColZ = Array.ConvertAll(adColZ, element => (float)element);

                    return bRet;
                }
                public static bool CalcKinematics(CDhParamAll DhParamAll, double[] adAxisValue, out double[] dColX, out double[] dColY, out double[] dColZ, out double dX, out double dY, out double dZ)
                {
                    dX = dY = dZ = 0.0;
                    if ((DhParamAll == null) || (DhParamAll.GetCount() <= 0))
                    {
                        dColX = new double[3];
                        dColY = new double[3];
                        dColZ = new double[3];
                        Array.Clear(dColX, 0, dColX.Length);
                        Array.Clear(dColY, 0, dColY.Length);
                        Array.Clear(dColZ, 0, dColZ.Length);
                        return false;
                    }
                    //if (DhParamAll.GetCount() <= 0) return false;
                    double dDir_X, dDir_Y, dDir_Z;
                    CalcDhParamAll(DhParamAll, adAxisValue, out dColX, out dColY, out dColZ, out dX, out dY, out dZ, out dDir_X, out dDir_Y, out dDir_Z);
                    return true;
                }
                public static bool CalcKinematics_LastOf(CDhParamAll DhParamAll, double[] adAxisValue, out double[] dColX, out double[] dColY, out double[] dColZ, out double dX, out double dY, out double dZ)
                {
                    dX = dY = dZ = 0.0;
                    if ((DhParamAll == null) || (DhParamAll.GetCount() <= 0))
                    {
                        dColX = new double[3];
                        dColY = new double[3];
                        dColZ = new double[3];
                        Array.Clear(dColX, 0, dColX.Length);
                        Array.Clear(dColY, 0, dColY.Length);
                        Array.Clear(dColZ, 0, dColZ.Length);
                        return false;
                    }
                    //if (DhParamAll.GetCount() <= 0) return false;
                    double dDir_X, dDir_Y, dDir_Z;
                    CalcDhParamAll_LastOf(DhParamAll, adAxisValue, out dColX, out dColY, out dColZ, out dX, out dY, out dZ, out dDir_X, out dDir_Y, out dDir_Z);
                    return true;
                }

                // To disable security and to use this function. (Kor: 보안까지 해제하려면 이 함수를 쓰도록 한다. 일반 스트링 함수는 보안해제를 안함)
                public static void MakeDhParam(byte[] pbyteDhData, out CDhParamAll DhParamAll)
                {
                    CEncryption.SetEncrypt("OJW5014"); // Put the master key(Kor: 암호화 해제는 보안이 필요)
                    String strDhData = Encoding.Default.GetString(CEncryption.Encryption(false, pbyteDhData));
                    MakeDhParam(strDhData, out DhParamAll);
                }
                public static void MakeDhParam(String strDhData, out CDhParamAll DhParamAll)
                {
                    // Remove the caption(Kor: 캡션 지우기)
                    strDhData = CConvert.RemoveCaption(strDhData, true, true);

                    // Initialize
                    DhParamAll = new CDhParamAll();
                    DhParamAll.DeleteAll();

                    CDhParam[] pCDhParam;
                    int[] pnAxis = new int[3];
                    int[] pnDir = new int[3];
                    String_To_CodeString_DHParam(strDhData, out pCDhParam, out pnAxis[0], out pnAxis[1], out pnAxis[2], out pnDir[0], out pnDir[1], out pnDir[2]);
                    if (pCDhParam != null)
                    {
                        for (int i = 0; i < pCDhParam.Length; i++)
                        {
                            //if (pCDhParam[i].nInit == 1) DhParamAll.DeleteAll();
                            if (pCDhParam[i].nInit == 1) { DhParamAll.DeleteAll(); DhParamAll.AddData(pCDhParam[i]); }
                            else                            DhParamAll.AddData(pCDhParam[i]);
                        }
                        pCDhParam = null;
                        DhParamAll.SetAxis_XYZ(pnAxis[0], pnDir[0], pnAxis[1], pnDir[1], pnAxis[2], pnDir[2]);
                    }
                    pnAxis = null;
                    pnDir = null;
                }

                public static bool StringLine_To_Class_DHParam(String strData, out CDhParam CDhParam)
                {
                    CDhParam = new CDhParam();
                    CDhParam.InitData();
                    if (Ojw.CConvert.RemoveChar(strData, ' ').IndexOf('@') == 0) return false;
                    try
                    {
                        bool bRet = false;

                        String[] pstrData = strData.Split(',');
                        if (pstrData.Length >= 6)
                        {
                            //int i = 0, j = 0, k = 0;
                            int nNum = 0;
                            int nAdd_Type = 0;
                            int nAdd_Number = 255;
                            foreach (string strItem in pstrData)
                            {
                                if      (nNum == 0) CDhParam.dA = CConvert.StrToDouble(strItem);
                                else if (nNum == 1) CDhParam.dD = CConvert.StrToDouble(strItem);
                                else if (nNum == 2) CDhParam.dTheta = CConvert.StrToDouble(strItem);
                                else if (nNum == 3) CDhParam.dAlpha = CConvert.StrToDouble(strItem);
                                else if (nNum == 4) CDhParam.nAxisNum = CConvert.StrToInt(strItem);
                                else if (nNum == 5) CDhParam.nAxisDir = CConvert.StrToInt(strItem);
                                else if (nNum == 6) CDhParam.nInit = CConvert.StrToInt(strItem);
                                else if (nNum == 7) nAdd_Type = CConvert.StrToInt(strItem);
                                else if (nNum == 8)
                                {
                                    if (nAdd_Type == 2) // 0: None, 1: Click Motor, 2: Click Function
                                    {
                                        nAdd_Number = CConvert.StrToInt(strItem);
                                        if ((nAdd_Number >= 0) && (nAdd_Number != 255))
                                        {
                                            CDhParam.nFunctionNumber = nAdd_Number;
                                        }
                                        else CDhParam.nFunctionNumber = -1;
                                    } 
                                    else if (nAdd_Type == 3) // 0: None, 1: Click Motor, 2: Click Function, 3: Calc Function(After Calc), 4. Set Angle(현재각도의 고정)
                                    {
                                        nAdd_Number = CConvert.StrToInt(strItem);
                                        if ((nAdd_Number >= 0) && (nAdd_Number != 255))
                                        {
                                            CDhParam.nFunctionNumber_AfterCalc = nAdd_Number;
                                        }
                                        else CDhParam.nFunctionNumber_AfterCalc = -1;
                                    }
                                    else if (nAdd_Type == 4)
                                    {

                                    }
                                }
                                nNum++;
                            }
                            bRet = true;
                        }
                        pstrData = null;

                        return bRet;
                    }
                    catch //(Exception e)
                    {
                        return false;
                    }
                }

                public static bool String_To_CodeString_DHParam(String strData, out CDhParam[] pCDhParam,
                                                                out int nAxis_X, out int nAxis_Y, out int nAxis_Z,
                                                                out int nDir_X, out int nDir_Y, out int nDir_Z)
                {
                    bool bRet;
                    TextBox txtDhData = new TextBox();
                    txtDhData.Text = strData;
                    bRet = TextBox_To_CodeString_DHParam(txtDhData, out pCDhParam,
                                                            out nAxis_X, out nAxis_Y, out nAxis_Z,
                                                            out nDir_X, out nDir_Y, out nDir_Z);
                    txtDhData.Dispose();
                    txtDhData = null;
                    return bRet;
                }

                //public static List<int> m_anIDs = new List<int>();
                //public static int[] GetMotors() { return m_anIDs.ToArray(); }
                //public static int GetMotors_Count() { return m_anIDs.Count; }
                
                public static bool TextBox_To_CodeString_DHParam(TextBox txtData, out CDhParam[] pCDhParam,
                                                                out int nAxis_X, out int nAxis_Y, out int nAxis_Z,
                                                                out int nDir_X, out int nDir_Y, out int nDir_Z)
                {
                    // draw datas with text in the txtData(Kor: txtData 의 값들을 그림)
                    bool bRet = false;
                    bool bRet2 = false;
                    String strData;
                    //String strTmp;
                    pCDhParam = null;

                    nAxis_X = 0;
                    nAxis_Y = 1;
                    nAxis_Z = 2;
                    nDir_X = nDir_Y = nDir_Z = 0;

                    txtData.Text = CConvert.RemoveCaption(txtData.Text, true, false);
                    
                    if (txtData.Lines.Length > 0)
                    {
                        bRet = true;
                        pCDhParam = new CDhParam[txtData.Lines.Length];
                        //int j = 0;
                        int nPos = 0;
                        // '!' 라인 이후 수식 계산 스킵 플래그 (Init=1 라인에서 재개)
                        bool bStopFormula = false;
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

                            if ((strData.ToUpper().IndexOf("XYZ=") == 0) && (CConvert.GetCnt(strData, ",") == 5))
                            {
                                // "XYZ=0,1,2"
                                nFind = strData.IndexOf("=") + 1;
                                String[] pstrData = strData.Substring(nFind, strData.Length - nFind).Split(',');
                                int nCnt = 0;
                                int[] pnAxis = new int[3];
                                int[] pnDir = new int[3];
                                foreach (String strItem in pstrData)
                                {
                                    if ((nCnt % 2) == 0) pnAxis[nCnt / 2] = CConvert.StrToInt(strItem);
                                    else if ((nCnt % 2) == 1) pnDir[nCnt / 2] = CConvert.StrToInt(strItem);
                                    nCnt++;
                                }
                                nAxis_X = pnAxis[0];
                                nAxis_Y = pnAxis[1];
                                nAxis_Z = pnAxis[2];
                                nDir_X = pnDir[0];
                                nDir_Y = pnDir[1];
                                nDir_Z = pnDir[2];
                                pnAxis = null;
                                pnDir = null;
                                continue;
                            }

                            // ! 라인: 현재 체인의 수식 계산 종료 마커
                            //   - 이 라인부터는 수식에 포함시키지 않음 (3D 시각화는 계속 진행)
                            //   - 다음 Init=1 라인을 만나면 새 체인 시작 (플래그 해제)
                            if (strData.Length > 0 && strData[0] == '!')
                            {
                                bStopFormula = true;
                                continue; // ! 라인 자체는 수식에 넣지 않음
                            }

                            // 수식 종료 상태에서 일반 라인은 건너뜀 (단, Init=1 만나면 재개)
                            if (bStopFormula)
                            {
                                CDhParam tmpParam;
                                if (StringLine_To_Class_DHParam(strData, out tmpParam) && tmpParam.nInit == 1)
                                {
                                    // 새 체인 시작 → 플래그 해제하고 정상 파싱 진행
                                    bStopFormula = false;
                                }
                                else
                                {
                                    continue; // 아직 ! 구간 내부 → 건너뜀
                                }
                            }

                            // real interpreter(Kor: 실제 해석)
                            bRet2 = StringLine_To_Class_DHParam(strData, out pCDhParam[nPos]);
                            pCDhParam[nPos].strCaption = strCaption;

                            if (bRet2 == false)
                            {
                                bRet = false;
                            }
                            else nPos++;
                        }

                        Array.Resize<CDhParam>(ref pCDhParam, nPos);
                    }
                    return bRet;
                }

                public static string ClassToString_DHParam(CDhParam OjwDhParam)
                {
                    String strData = "";
                    int nRoundPoint = 3; // it checks only thousandths(.000)(Kor: 소숫점 3자리 까지 허용)
                    strData =
                        "[" +
                        CConvert.DoubleToStr((double)Math.Round(OjwDhParam.dA, nRoundPoint)) + "," +
                        CConvert.DoubleToStr((double)Math.Round(OjwDhParam.dD, nRoundPoint)) + "," +
                        CConvert.DoubleToStr((double)Math.Round(OjwDhParam.dTheta, nRoundPoint)) + "," +
                        CConvert.DoubleToStr((double)Math.Round(OjwDhParam.dAlpha, nRoundPoint)) +
                        "]," +
                        "[" +
                        CConvert.IntToStr(OjwDhParam.nAxisNum) + "," +
                        CConvert.IntToStr(OjwDhParam.nAxisDir) + "," +
                        CConvert.IntToStr(OjwDhParam.nInit) +
                        "]";
                    try
                    {
                        if (OjwDhParam.strCaption != null)
                        {
                            if (OjwDhParam.strCaption.Trim() != "")
                            {
                                strData += " // " + OjwDhParam.strCaption.Trim();
                            }
                        }
                    }
                    finally
                    {

                    }
                    return strData;
                }


                public static void CalcF(ref C3d Ojw3d, int nFunctionNumber, out float fX, out float fY, out float fZ) { CalcF(ref Ojw3d, nFunctionNumber, out fX, out fY, out fZ, -1, false); }
                public static void CalcF(ref C3d Ojw3d, int nFunctionNumber, out float fX, out float fY, out float fZ, int nID) { CalcF(ref Ojw3d, nFunctionNumber, out fX, out fY, out fZ, nID, false); }
                public static void CalcF(ref C3d Ojw3d, int nFunctionNumber, out float fX, out float fY, out float fZ, bool bLimitFunction) { CalcF(ref Ojw3d, nFunctionNumber, out fX, out fY, out fZ, -1, bLimitFunction); }
                // nID = -1 이면 최종 포인트 까지 계산
                public static void CalcF(ref C3d Ojw3d, int nFunctionNumber, out float fX, out float fY, out float fZ, int nID, bool bLimitFunction)
                {
                    Ojw.CKinematics.CForward.SetCalcLimit_Axis(nID); // ..번 모터까지의 위치값 확인
                    if (bLimitFunction) 
                        Ojw.CKinematics.CForward.SetCalcLimit_FunctionNumber(nFunctionNumber); // 수식번호가 정의되었다면 거기까지 해석

                    int nNum = nFunctionNumber;
                    double dX, dY, dZ;
                    Ojw3d.GetData_Forward(nNum, out dX, out dY, out dZ);
                    fX = (float)dX; fY = (float)dY; fZ = (float)dZ;
                    //Ojw.CMessage.Write("[{0}]Calc X[{1}], Y[{2}], Z[{3}]", nAxis, Ojw.CMath.Round(fX, 1), Ojw.CMath.Round(fY, 1), Ojw.CMath.Round(fZ, 1));
                }
                #endregion Class Control - DHParam

#if false
                #region Math - 행렬연산, DH T 함수 만들기 함수
                // 연산이상이 발생하면 false 를 내보냄 - 정방행렬
                public static bool CalcMatrix(int nLine, double[,] adS0, double[,] adS1, out double[,] adRes)
                {
                    //bool bRet = true;
                    adRes = new double[nLine, nLine];
                    if ((adS0.Length < nLine * nLine) || (adS1.Length < nLine * nLine)) return false;
                    for (int i = 0; i < nLine; i++)
                        for (int j = 0; j < nLine; j++)
                        {
                            adRes[i, j] = 0.0f;
                            for (int k = 0; k < nLine; k++)
                                adRes[i, j] = adRes[i, j] + adS0[i, k] * adS1[k, j];
                        }
                    return true;
                }

                // DH- T 행렬을 만들어낸다.
                public static bool CalcT(double dA, double dAlpha, double dD, double dTheta, out double[,] adT)
                {
                    adT = new double[4, 4];
                    //double dAngle = 90.0f;
                    int i;
                    i = 0; adT[i, 0] = (double)CMath.Cos(dTheta); adT[i, 1] = -(double)CMath.Sin(dTheta) * (double)CMath.Cos(dAlpha); adT[i, 2] = (double)CMath.Sin(dTheta) * (double)CMath.Sin(dAlpha); adT[i, 3] = dA * (double)CMath.Cos(dTheta);
                    i = 1; adT[i, 0] = (double)CMath.Sin(dTheta); adT[i, 1] = (double)CMath.Cos(dTheta) * (double)CMath.Cos(dAlpha); adT[i, 2] = -(double)CMath.Cos(dTheta) * (double)CMath.Sin(dAlpha); adT[i, 3] = dA * (double)CMath.Sin(dTheta);
                    i = 2; adT[i, 0] = 0.0f; adT[i, 1] = (double)CMath.Sin(dAlpha); adT[i, 2] = (double)CMath.Cos(dAlpha); adT[i, 3] = dD;
                    i = 3; adT[i, 0] = 0.0f; adT[i, 1] = 0.0f; adT[i, 2] = 0.0f; adT[i, 3] = 1.0f;
                    return true;
                }

                // 회전 행렬을 만들어낸다.(4 by 4)
                public static bool CalcRot(double dAngleX, double dAngleY, double dAngleZ, double[,] adSrc, out double[,] adRot)
                {
                    double[,] adCalcX = new double[4, 4];
                    double[,] adCalcY = new double[4, 4];
                    double[,] adCalcZ = new double[4, 4];
                    adRot = new double[4, 4];
                    int i;

                    // x축 회전
                    i = 0; adCalcX[i, 0] = 1.0f; adCalcX[i, 1] = 0.0f; adCalcX[i, 2] = 0.0f; adCalcX[i, 3] = 0.0f;
                    i = 1; adCalcX[i, 0] = 0.0f; adCalcX[i, 1] = (double)CMath.Cos(dAngleX); adCalcX[i, 2] = -(double)CMath.Sin(dAngleX); adCalcX[i, 3] = 0.0f;
                    i = 2; adCalcX[i, 0] = 0.0f; adCalcX[i, 1] = (double)CMath.Sin(dAngleX); adCalcX[i, 2] = (double)CMath.Cos(dAngleX); adCalcX[i, 3] = 0.0f;
                    i = 3; adCalcX[i, 0] = 0.0f; adCalcX[i, 1] = 0.0f; adCalcX[i, 2] = 0.0f; adCalcX[i, 3] = 1.0f;

                    // y축 회전
                    i = 0; adCalcY[i, 0] = (double)CMath.Cos(dAngleY); adCalcY[i, 1] = 0.0f; adCalcY[i, 2] = (double)CMath.Sin(dAngleY); adCalcY[i, 3] = 0.0f;
                    i = 1; adCalcY[i, 0] = 0.0f; adCalcY[i, 1] = 1.0f; adCalcY[i, 2] = 0.0f; adCalcY[i, 3] = 0.0f;
                    i = 2; adCalcY[i, 0] = -(double)CMath.Sin(dAngleY); adCalcY[i, 1] = 0.0f; adCalcY[i, 2] = (double)CMath.Cos(dAngleY); adCalcY[i, 3] = 0.0f;
                    i = 3; adCalcY[i, 0] = 0.0f; adCalcY[i, 1] = 0.0f; adCalcY[i, 2] = 0.0f; adCalcY[i, 3] = 1.0f;


                    // z축 회전
                    i = 0; adCalcZ[i, 0] = (double)CMath.Cos(dAngleZ); adCalcZ[i, 1] = -(double)CMath.Sin(dAngleZ); adCalcZ[i, 2] = 0.0f; adCalcZ[i, 3] = 0.0f;
                    i = 1; adCalcZ[i, 0] = (double)CMath.Sin(dAngleZ); adCalcZ[i, 1] = (double)CMath.Cos(dAngleZ); adCalcZ[i, 2] = 0.0f; adCalcZ[i, 3] = 0.0f;
                    i = 2; adCalcZ[i, 0] = 0.0f; adCalcZ[i, 1] = 0.0f; adCalcZ[i, 2] = 1.0f; adCalcZ[i, 3] = 0.0f;
                    i = 3; adCalcZ[i, 0] = 0.0f; adCalcZ[i, 1] = 0.0f; adCalcZ[i, 2] = 0.0f; adCalcZ[i, 3] = 1.0f;


                    CalcMatrix(4, adSrc, adCalcX, out adRot);
                    CalcMatrix(4, adRot, adCalcY, out adRot);
                    CalcMatrix(4, adRot, adCalcZ, out adRot);

                    adCalcX = null;
                    adCalcY = null;
                    adCalcZ = null;
                    return true;
                }
                public static bool CalcRot(double dAngleX, double dAngleY, double dAngleZ, out double[,] adRot)
                {
                    double[,] adCalcI = new double[4, 4];
                    int i;
                    // 단위행렬
                    i = 0; adCalcI[i, 0] = 1.0; adCalcI[i, 1] = 0.0; adCalcI[i, 2] = 0.0; adCalcI[i, 3] = 0.0;
                    i = 1; adCalcI[i, 0] = 0.0; adCalcI[i, 1] = 1.0; adCalcI[i, 2] = 0.0; adCalcI[i, 3] = 0.0;
                    i = 2; adCalcI[i, 0] = 0.0; adCalcI[i, 1] = 0.0; adCalcI[i, 2] = 1.0; adCalcI[i, 3] = 0.0;
                    i = 3; adCalcI[i, 0] = 0.0; adCalcI[i, 1] = 0.0; adCalcI[i, 2] = 0.0; adCalcI[i, 3] = 1.0;

                    CalcRot(dAngleX, dAngleY, dAngleZ, adCalcI, out adRot);

                    adCalcI = null;
                    return true;
                }

                // 행렬 비교, 같으면 true; 같지 않으면 false; 단, 소숫점 3째자리까지만 확인
                public static bool CompareMatrix(int nLine, double[,] adSrc0, double[,] adSrc1)
                {
                    bool bRet = true;
                    // 랭크가 맞지 않는 경우의 에러처리 나중에 필요. - 현재는 귀찮아서 패스~~
                    try
                    {
                        int nPoint = 3;
                        for (int i = 0; i < nLine; i++)
                        {
                            for (int j = 0; j < nLine; j++)
                            {
                                if ((double)Math.Round(adSrc0[i, j], nPoint) != (double)Math.Round(adSrc1[i, j], nPoint))
                                {
                                    bRet = false;
                                    break;
                                }
                            }
                            if (bRet == false) break;
                        }
                    }
                    catch
                    {
                        bRet = false;
                    }
                    return bRet;
                }
                #endregion Math - 행렬연산, DH T 함수 만들기 함수
#endif
            }
#if false
            public class CPythonMath
            {



/* ****************************************************************************
*
* Copyright (c) Microsoft Corporation. 
*
* This source code is subject to terms and conditions of the Microsoft Public License. A 
* copy of the license can be found in the License.html file at the root of this distribution. If 
* you cannot locate the  Microsoft Public License, please send an email to 
* ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
* by the terms of the Microsoft Public License.
*
* You must not remove this notice, or any other, from this software.
*
*
* ***************************************************************************/

[assembly: PythonModule("math", typeof(IronPython.Modules.PythonMath))]
namespace IronPython.Modules {
public static partial class PythonMath {
public const string __doc__ = "Provides common mathematical functions.";

public const double pi = Math.PI;
public const double e = Math.E;

private const double degreesToRadians = Math.PI / 180.0;
private const int Bias = 0x3FE;

public static double degrees(double radians) {
    return Check(radians, radians / degreesToRadians);
}

public static double radians(double degrees) {
    return Check(degrees, degrees * degreesToRadians);
}

public static double fmod(double v, double w) {
    return Check(v, w, v % w);
}

public static double fsum(IEnumerable e) {
    IEnumerator en = e.GetEnumerator();
    double value = 0.0;
    while (en.MoveNext()) {
        value += Converter.ConvertToDouble(en.Current);
    }
    return value;
}

public static PythonTuple frexp(double v) {
    if (Double.IsInfinity(v) || Double.IsNaN(v)) {
        return PythonTuple.MakeTuple(v, 0.0);
    }
    int exponent = 0;
    double mantissa = 0;

    if (v == 0) {
        mantissa = 0;
        exponent = 0;
    } 
    else {
        byte[] vb = BitConverter.GetBytes(v);
        if (BitConverter.IsLittleEndian) {
            DecomposeLe(vb, out mantissa, out exponent);
        } else {
            throw new NotImplementedException();
        }
    }

    return PythonTuple.MakeTuple(mantissa, exponent);
}

public static PythonTuple modf(double v) {
    if (double.IsInfinity(v)) {
        return PythonTuple.MakeTuple(0.0, v);
    }
    double w = v % 1.0;
    v -= w;
    return PythonTuple.MakeTuple(w, v);
}

public static double ldexp(double v, BigInteger w) {
    if (v == 0.0 || double.IsInfinity(v)) {
        return v;
    }
    return Check(v, v * Math.Pow(2.0, w));
}

public static double hypot(double v, double w) {
00098             if (double.IsInfinity(v) || double.IsInfinity(w)) {
00099                 return double.PositiveInfinity;
00100             }
00101             return Check(v, w, Complex64.Hypot(v, w));
00102         }
00103 
00104         public static double pow(double v, double exp) {
00105             if (v == 1.0 || exp == 0.0) {
00106                 return 1.0;
00107             } else if (double.IsNaN(v) || double.IsNaN(exp)) {
00108                 return double.NaN;
00109             } else if (v == 0.0) {
00110                 if (exp > 0.0) {
00111                     return 0.0;
00112                 }
00113                 throw PythonOps.ValueError("math domain error");
00114             } else if (double.IsPositiveInfinity(exp)) {
00115                 if (v > 1.0 || v < -1.0) {
00116                     return double.PositiveInfinity;
00117                 } else if (v == -1.0) {
00118                     return 1.0;
00119                 } else {
00120                     return 0.0;
00121                 }
00122             } else if (double.IsNegativeInfinity(exp)) {
00123                 if (v > 1.0 || v < -1.0) {
00124                     return 0.0;
00125                 } else if (v == -1.0) {
00126                     return 1.0;
00127                 } else {
00128                     return double.PositiveInfinity;
00129                 }
00130             }
00131             return Check(v, exp, Math.Pow(v, exp));
00132         }
00133 
00134         public static double log(double v0) {
00135             if (v0 <= 0.0) {
00136                 throw PythonOps.ValueError("math domain error");
00137             }
00138             return Check(v0, Math.Log(v0));
00139         }
00140 
00141         public static double log(double v0, double v1) {
00142             if (v0 <= 0.0 || v1 == 0.0) {
00143                 throw PythonOps.ValueError("math domain error");
00144             } else if (v1 == 1.0) {
00145                 throw PythonOps.ZeroDivisionError("float division");
00146             } else if (v1 == Double.PositiveInfinity) {
00147                 return 0.0;
00148             }
00149             return Check(Math.Log(v0, v1));
00150         }
00151 
00152         public static double log(BigInteger value) {
00153             return Check(value.Log());
00154         }
00155 
00156         public static double log(object value) {
00157             // CPython tries float first, then double, so we need
00158             // an explicit overload which properly matches the order here
00159             double val;
00160             if (Converter.TryConvertToDouble(value, out val)) {
00161                 return log(val);
00162             } else {
00163                 return log(Converter.ConvertToBigInteger(value));
00164             }
00165         }
00166 
00167         public static double log(BigInteger value, double newBase) {
00168             if (newBase <= 0.0 || value <= 0) {
00169                 throw PythonOps.ValueError("math domain error");
00170             } else if (newBase == 1.0) {
00171                 throw PythonOps.ZeroDivisionError("float division");
00172             } else if (newBase == Double.PositiveInfinity) {
00173                 return 0.0;
00174             }
00175             return Check(value.Log(newBase));
00176         }
00177 
00178         public static double log(object value, double newBase) {
00179             // CPython tries float first, then double, so we need
00180             // an explicit overload which properly matches the order here
00181             double val;
00182             if (Converter.TryConvertToDouble(value, out val)) {
00183                 return log(val, newBase);
00184             } else {
00185                 return log(Converter.ConvertToBigInteger(value), newBase);
00186             }
00187         }
00188 
00189         public static double log10(double v0) {
00190             if (v0 <= 0.0) {
00191                 throw PythonOps.ValueError("math domain error");
00192             }
00193             return Check(v0, Math.Log10(v0));
00194         }
00195 
00196         public static double log10(BigInteger value) {
00197             return Check(value.Log10());
00198         }
00199 
00200         public static double log10(object value) {
00201             // CPython tries float first, then double, so we need
00202             // an explicit overload which properly matches the order here
00203             double val;
00204             if (Converter.TryConvertToDouble(value, out val)) {
00205                 return log10(val);
00206             } else {
00207                 return log10(Converter.ConvertToBigInteger(value));
00208             }
00209         }
00210 
00211         public static double log1p(double v0) {
00212             return log(v0 + 1.0);
00213         }
00214 
00215         public static double log1p(BigInteger value) {
00216             return log(value + (BigInteger)1);
00217         }
00218 
00219         public static double log1p(object value) {
00220             // CPython tries float first, then double, so we need
00221             // an explicit overload which properly matches the order here
00222             double val;
00223             if (Converter.TryConvertToDouble(value, out val)) {
00224                 return log(val + 1.0);
00225             } else {
00226                 return log(Converter.ConvertToBigInteger(value) + (BigInteger)1);
00227             }
00228         }
00229 
00230         public static double asinh(double v0) {
00231             if (v0 == 0.0 || double.IsInfinity(v0)) {
00232                 return v0;
00233             }
00234             // rewrote ln(v0 + sqrt(v0**2 + 1)) for precision
00235             if (Math.Abs(v0) > 1.0) {
00236                 return Math.Log(v0) + Math.Log(1.0 + Complex64.Hypot(1.0, 1.0 / v0));
00237             } else {
00238                 return Math.Log(v0 + Complex64.Hypot(1.0, v0));
00239             }
00240         }
00241 
00242         public static double asinh(BigInteger value) {
00243             if (value == 0) {
00244                 return 0;
00245             }
00246             // rewrote ln(v0 + sqrt(v0**2 + 1)) for precision
00247             if (value.Abs() > 1) {
00248                 return value.Log() + Math.Log(1.0 + Complex64.Hypot(1.0, 1.0 / value));
00249             } else {
00250                 return Math.Log(value + Complex64.Hypot(1.0, value));
00251             }
00252         }
00253 
00254         public static double asinh(object value) {
00255             // CPython tries float first, then double, so we need
00256             // an explicit overload which properly matches the order here
00257             double val;
00258             if (Converter.TryConvertToDouble(value, out val)) {
00259                 return asinh(val);
00260             } else {
00261                 return asinh(Converter.ConvertToBigInteger(value));
00262             }
00263         }
00264 
00265         public static double acosh(double v0) {
00266             if (v0 < 1.0) {
00267                 throw PythonOps.ValueError("math domain error");
00268             } else if (double.IsPositiveInfinity(v0)) {
00269                 return double.PositiveInfinity;
00270             }
00271             // rewrote ln(v0 + sqrt(v0**2 - 1)) for precision
00272             double c = Math.Sqrt(v0 + 1.0);
00273             return Math.Log(c) + Math.Log(v0 / c + Math.Sqrt(v0 - 1.0));
00274         }
00275 
00276         public static double acosh(BigInteger value) {
00277             if (value <= 0) {
00278                 throw PythonOps.ValueError("math domain error");
00279             }
00280             // rewrote ln(v0 + sqrt(v0**2 - 1)) for precision
00281             double c = Math.Sqrt(value + (BigInteger)1);
00282             return Math.Log(c) + Math.Log(value / c + Math.Sqrt(value - (BigInteger)1));
00283         }
00284 
00285         public static double acosh(object value) {
00286             // CPython tries float first, then double, so we need
00287             // an explicit overload which properly matches the order here
00288             double val;
00289             if (Converter.TryConvertToDouble(value, out val)) {
00290                 return acosh(val);
00291             } else {
00292                 return acosh(Converter.ConvertToBigInteger(value));
00293             }
00294         }
00295 
00296         public static double atanh(double v0) {
00297             if (v0 >= 1.0 || v0 <= -1.0) {
00298                 throw PythonOps.ValueError("math domain error");
00299             } else if (v0 == 0.0) {
00300                 // preserve +/-0.0
00301                 return v0;
00302             }
00303 
00304             return Math.Log((1.0 + v0) / (1.0 - v0)) * 0.5;
00305         }
00306 
00307         public static double atanh(BigInteger value) {
00308             if (value == 0) {
00309                 return 0;
00310             } else {
00311                 throw PythonOps.ValueError("math domain error");
00312             }
00313         }
00314 
00315         public static double atanh(object value) {
00316             // CPython tries float first, then double, so we need
00317             // an explicit overload which properly matches the order here
00318             double val;
00319             if (Converter.TryConvertToDouble(value, out val)) {
00320                 return atanh(val);
00321             } else {
00322                 return atanh(Converter.ConvertToBigInteger(value));
00323             }
00324         }
00325 
00326         public static double atan2(double v0, double v1) {
00327             if (double.IsNaN(v0) || double.IsNaN(v1)) {
00328                 return double.NaN;
00329             } else if (double.IsInfinity(v0)) {
00330                 if (double.IsPositiveInfinity(v1)) {
00331                     return pi * 0.25 * Math.Sign(v0);
00332                 } else if (double.IsNegativeInfinity(v1)) {
00333                     return pi * 0.75 * Math.Sign(v0);
00334                 } else {
00335                     return pi * 0.5 * Math.Sign(v0);
00336                 }
00337             } else if (double.IsInfinity(v1)) {
00338                 return v1 > 0.0 ? 0.0 : pi * DoubleOps.Sign(v0);
00339             }
00340             return Math.Atan2(v0, v1);
00341         }
00342 
00343         public static object factorial(double v0) {
00344             if ((BigInteger)v0 != v0) {
00345                 throw PythonOps.ValueError("factorial() only accepts integral values");
00346             }
00347             if (v0 < 0.0) {
00348                 throw PythonOps.ValueError("factorial() not defined for negative values");
00349             }
00350 
00351             BigInteger val = 1;
00352             for (BigInteger mul = (BigInteger)v0; mul > (BigInteger)1; mul -= (BigInteger)1) {
00353                 val *= mul;
00354             }
00355 
00356             if (val > SysModule.maxint) {
00357                 return val;
00358             }
00359             return (int)val;
00360         }
00361 
00362         public static object factorial(BigInteger value) {
00363             if (value < 0) {
00364                 throw PythonOps.ValueError("factorial() not defined for negative values");
00365             }
00366 
00367             BigInteger val = 1;
00368             for (BigInteger mul = value; mul > (BigInteger)1; mul -= (BigInteger)1) {
00369                 val *= mul;
00370             }
00371 
00372             if (val > SysModule.maxint) {
00373                 return val;
00374             }
00375             return (int)val;
00376         }
00377 
00378         public static object factorial(object value) {
00379             // CPython tries float first, then double, so we need
00380             // an explicit overload which properly matches the order here
00381             double val;
00382             if (Converter.TryConvertToDouble(value, out val)) {
00383                 return factorial(val);
00384             } else {
00385                 return factorial(Converter.ConvertToBigInteger(value));
00386             }
00387         }
00388 
00389         public static object trunc(CodeContext context, object value) {
00390             object func;
00391             if (PythonOps.TryGetBoundAttr(value, Symbols.Truncate, out func)) {
00392                 return PythonOps.CallWithContext(context, func);
00393             } else {
00394                 throw PythonOps.AttributeError("__trunc__");
00395             }
00396         }
00397 
00398         public static bool isinf(double v0) {
00399             return double.IsInfinity(v0);
00400         }
00401 
00402         public static bool isinf(BigInteger value) {
00403             return false;
00404         }
00405 
00406         public static bool isinf(object value) {
00407             // CPython tries float first, then double, so we need
00408             // an explicit overload which properly matches the order here
00409             double val;
00410             if (Converter.TryConvertToDouble(value, out val)) {
00411                 return isinf(val);
00412             }
00413             return false;
00414         }
00415 
00416         public static bool isnan(double v0) {
00417             return double.IsNaN(v0);
00418         }
00419 
00420         public static bool isnan(BigInteger value) {
00421             return false;
00422         }
00423 
00424         public static bool isnan(object value) {
00425             // CPython tries float first, then double, so we need
00426             // an explicit overload which properly matches the order here
00427             double val;
00428             if (Converter.TryConvertToDouble(value, out val)) {
00429                 return isnan(val);
00430             }
00431             return false;
00432         }
00433 
00434         public static double copysign(object x, object y) {
00435             double val, sign;
00436             if (!Converter.TryConvertToDouble(x, out val) ||
00437                 !Converter.TryConvertToDouble(y, out sign)) {
00438                 throw PythonOps.TypeError("TypeError: a float is required");
00439             }
00440             return DoubleOps.Sign(sign) * Math.Abs(val);
00441         }
00442 
00443         private static void SetExponentLe(byte[] v, int exp) {
00444             exp += Bias;
00445             ushort oldExp = LdExponentLe(v);
00446             ushort newExp = (ushort)(oldExp & 0x800f | (exp << 4));
00447             StExponentLe(v, newExp);
00448         }
00449 
00450         private static int IntExponentLe(byte[] v) {
00451             ushort exp = LdExponentLe(v);
00452             return ((int)((exp & 0x7FF0) >> 4) - Bias);
00453         }
00454 
00455         private static ushort LdExponentLe(byte[] v) {
00456             return (ushort)(v[6] | ((ushort)v[7] << 8));
00457         }
00458 
00459         private static long LdMantissaLe(byte[] v) {
00460             int i1 = (v[0] | (v[1] << 8) | (v[2] << 16) | (v[3] << 24));
00461             int i2 = (v[4] | (v[5] << 8) | ((v[6] & 0xF) << 16));
00462 
00463             return i1 | (i2 << 32);
00464         }
00465 
00466         private static void StExponentLe(byte[] v, ushort e) {
00467             v[6] = (byte)e;
00468             v[7] = (byte)(e >> 8);
00469         }
00470 
00471         private static bool IsDenormalizedLe(byte[] v) {
00472             ushort exp = LdExponentLe(v);
00473             long man = LdMantissaLe(v);
00474 
00475             return ((exp & 0x7FF0) == 0 && (man != 0));
00476         }
00477 
00478         private static void DecomposeLe(byte[] v, out double m, out int e) {
00479             if (IsDenormalizedLe(v)) {
00480                 throw new NotImplementedException();
00481             } else {
00482                 e = IntExponentLe(v);
00483                 SetExponentLe(v, 0);
00484                 m = BitConverter.ToDouble(v, 0);
00485             }
00486         }
00487 
00488         private static double Check(double v) {
0489             return PythonOps.CheckMath(v);
0490         }
0491 
0492         private static double Check(double input, double output) {
0493             if (double.IsInfinity(input) && double.IsInfinity(output) ||
0494                 double.IsNaN(input) && double.IsNaN(input)) {
0495                 return output;
0496             } else {
0497                 return PythonOps.CheckMath(output);
0498             }
0499         }
0500 
0501         private static double Check(double in0, double in1, double output) {
0502             if ((double.IsInfinity(in0) || double.IsInfinity(in1)) && double.IsInfinity(output) ||
0503                 (double.IsNaN(in0) || double.IsNaN(in1)) && double.IsNaN(output)) {
0504                 return output;
0505             } else {
506                 return PythonOps.CheckMath(output);
0507             }
0508         }
0509     }
}


















            }
#endif


#if _USING_DOTNET_3_5 || _USING_DOTNET_2_0
#else
            public class CPython
            {
                /// <summary>
                /// //////////////////////////////////////
                //public static bool Run_IronPython(string strData)
                //{
                //    ScriptEngine engine = Python.CreateEngine();
                //    ScriptScope scope = engine.Runtime.CreateScope();
                    
                //    scope.ContainsVariable("x");
                //    scope.ContainsVariable("y");
                //    scope.ContainsVariable("z");
                //    scope.SetVariable("x", x);
                //    scope.SetVariable("y", y);
                //    scope.SetVariable("z", z);
                //    int i;
                //    int nLength_v = 0;
                //    int nLength_t = 0;
                //    i = 0; foreach (double dv in SCode.pnVar_Number) { scope.ContainsVariable("v" + i.ToString()); scope.SetVariable("v" + i.ToString(), dv); i++; }
                //    nLength_v = i;
                //    i = 0; foreach (double dt in SCode.pnMotor_Number) { scope.ContainsVariable("t" + i.ToString()); scope.SetVariable("t" + i.ToString(), dt); i++; }
                //    nLength_t = i;

                //    int nNum = 0;
                //    for (i = 0; i < SCode.nMotor_Max; i++)
                //    {
                //        nNum = SCode.pnMotor_Number[i];
                //        scope.ContainsVariable("t" + nNum.ToString());
                //        scope.SetVariable("t" + nNum, adMot[nNum]);
                //    }

                //    for (i = 0; i < SCode.nVar_Max; i++)
                //    {
                //        nNum = SCode.pnVar_Number[i];
                //        scope.ContainsVariable("v" + nNum.ToString());
                //        scope.SetVariable("v" + nNum, adV[nNum]);
                //    }

                //    string code = SCode.strPython.Trim();
                //    string strTmp = "import sys\r\n";
                //    strTmp += "sys.path.append(r'";
                //    strTmp += Application.StartupPath;// +"\\lib";
                //    strTmp += "')\r\n";

                //    ScriptSource source = engine.CreateScriptSourceFromString(strTmp + code, Microsoft.Scripting.SourceCodeKind.AutoDetect);
                    
                //    source.Execute(scope);
                //}
                /// //////////////////////////////////////
                /// </summary>
                public const int _CNT_MOTOR = 1000;
                public const int _CNT_VAR_V = 1000;

                public static bool CalcCode(ref SOjwCode_t SCode, ref double x, ref double y, ref double z, ref double [] adV, ref double [] adMot, ref string strErrorMsg)
                {
                    if (SCode.bInit == false) return false;
                    //if (SCode.nMotor_Max <= 0) return false;

                    try
                    {
                        ScriptEngine engine = Python.CreateEngine();
                        ScriptScope scope = engine.Runtime.CreateScope();



                        scope.ContainsVariable("x");
                        scope.ContainsVariable("y");
                        scope.ContainsVariable("z");
                        scope.SetVariable("x", x);
                        scope.SetVariable("y", y);
                        scope.SetVariable("z", z);
                        int i;
                        int nLength_v = 0;
                        int nLength_t = 0;
                        i = 0; foreach (double dv in SCode.pnVar_Number) { scope.ContainsVariable("v" + i.ToString()); scope.SetVariable("v" + i.ToString(), dv); i++; }
                        nLength_v = i;
                        i = 0; foreach (double dt in SCode.pnMotor_Number) { scope.ContainsVariable("t" + i.ToString()); scope.SetVariable("t" + i.ToString(), dt); i++; }
                        nLength_t = i;

                        int nNum = 0;
                        for (i = 0; i < SCode.nMotor_Max; i++)
                        {
                            nNum = SCode.pnMotor_Number[i];
                            scope.ContainsVariable("t" + nNum.ToString());
                            scope.SetVariable("t" + nNum, adMot[nNum]);
                        }

                        for (i = 0; i < SCode.nVar_Max; i++)
                        {
                            nNum = SCode.pnVar_Number[i];
                            scope.ContainsVariable("v" + nNum.ToString());
                            scope.SetVariable("v" + nNum, adV[nNum]);
                        }
                        
                        string code = SCode.strPython.Trim();
                        string strTmp = "import sys\r\n";
                        strTmp += "sys.path.append(r'";
                        strTmp += Application.StartupPath;// +"\\lib";
                        strTmp += "')\r\n";
                        //string strTmp = "import System.Math as Math\r\n";// +  //"import math\r\n";// "import System.Math as math\r\n";
                       //string strTmp = "import numerics\r\nfrom math import *\r\nfrom System import Math\r\n\r\nfrom Extreme.Mathematics.Calculus import *\r\nfrom Extreme.Mathematics import *\r\n";
                        //strTmp += "import numerics\r\n";
                        //strTmp += "from math import *\r\n";

                        //"def Radians(val):\r\n" +
                        //"  return val * Math.PI() / 180.0\r\n" + 
                        //"def Degrees(val):\r\n" +
                        //"  return val * 180.0 / Math.PI()\r\n";
                        //ScriptSource source = engine.CreateScriptSourceFromString(code, Microsoft.Scripting.SourceCodeKind.AutoDetect);//SourceCodeKind.SingleStatement);
                        //ScriptSource source = engine.CreateScriptSourceFromString("import math\r\n" + code, Microsoft.Scripting.SourceCodeKind.AutoDetect);//.AutoDetect);//SourceCodeKind.SingleStatement);
                        //ScriptSource source = engine.CreateScriptSourceFromString("import System.Math as Math\r\n" + code, Microsoft.Scripting.SourceCodeKind.AutoDetect);
                        ScriptSource source = engine.CreateScriptSourceFromString(strTmp + code, Microsoft.Scripting.SourceCodeKind.AutoDetect);
                        //ScriptSource source = engine.CreateScriptSourceFromString("import Math\r\n" + code, Microsoft.Scripting.SourceCodeKind.AutoDetect);
                        
                        source.Execute(scope);
                        dynamic dy_x = scope.GetVariable("x");
                        dynamic dy_y = scope.GetVariable("y");
                        dynamic dy_z = scope.GetVariable("z");

                        x = (double)dy_x;
                        y = (double)dy_y;
                        z = (double)dy_z;


                        for (i = 0; i < SCode.nMotor_Max; i++)
                        {
                            nNum = SCode.pnMotor_Number[i];
                            adMot[nNum] = (double)scope.GetVariable("t" + nNum.ToString());
                        }

                        for (i = 0; i < SCode.nVar_Max; i++)
                        {
                            nNum = SCode.pnVar_Number[i];
                            adV[nNum] = (double)scope.GetVariable("v" + nNum.ToString());
                        }

                        //Ojw.CMessage.Write("Result = x[{0}], y[{1}], z[{2}]", dy_x, dy_y, dy_z);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        //Ojw.CMessage.Write_Error("Python compile error - {0}", ex.ToString());
                        strErrorMsg = ex.ToString();
                        return false;
                    }

                    //return true;
                }
            }
#endif
            // there are All compiling models (Kor: 수식구조를 컴파일 해서 바이너리 코드로 만드는 과정 전반이 여기 있다.)
            public class CInverse
            {
                #region Variable address(Kor: 변수 Address 정의)
                private const int _ADDRESS_MOTOR = 0x0000;
                private const int _ADDRESS_X = 0x1000;
                private const int _ADDRESS_Y = _ADDRESS_X + 1;
                private const int _ADDRESS_Z = _ADDRESS_X + 2;
                private const int _ADDRESS_V = _ADDRESS_X + 3;
                private const int _ADDRESS_M = 0x2000;

                private const int _CNT_ADDRESS = 0xfffff;//0xfffff;

                public const int _CNT_MOTOR = _ADDRESS_X - _ADDRESS_MOTOR;
                public const int _CNT_VAR_V = _ADDRESS_M - _ADDRESS_V;
                private const int _CNT_VAR_M = _CNT_ADDRESS - _ADDRESS_M;
                #endregion Variable address(Kor: 변수 Address 정의)

                #region Math Function Address(Kor: 수식 Address 정의)
                private const int _EQ = 0x0000001;
                private const int _PLUS = 0x0000002;
                private const int _MINUS = 0x0000004;
                private const int _MUL = 0x0000008;

                private const int _DIV = 0x0000010;
                private const int _SIN = 0x0000020;
                private const int _COS = 0x0000040;
                private const int _TAN = 0x0000080;

                private const int _ASIN = 0x0000100;
                private const int _ACOS = 0x0000200;
                private const int _ATAN = 0x0000400;
                private const int _SQRT = 0x0000800;

                private const int _POW = 0x0001000;
                private const int _ABS = 0x0002000;
                private const int _MOD = 0x0004000;
                //private const int _ROUND=0x0008000;

                private const int _BRACKET_SMALL_START = 0x0004000;
                private const int _BRACKET_SMALL_END = 0x0008000;
                private const int _BRACKET_MIDDLE_START = 0x0010000;

                private const int _BRACKET_MIDDLE_END = 0x0020000;
                private const int _BRACKET_LARGE_START = 0x0040000;
                private const int _BRACKET_LARGE_END = 0x0080000;
                private const int _COMMA = 0x0100000;

                private const int _DIGIT = 0x0200000;
                private const int _ALPHA = 0x0400000;
                private const int _ATAN2 = 0x0800000;
                private const int _ACOS2 = 0x1000000;
                private const int _ASIN2 = 0x2000000;
                //private const int _MOD   = 0x4000000;
                private const int _ROUND = 0x8000000;
                private const int _CALL = 0x10000000;
                private const int _IF = 0x20000000;
                private const int _INV = 0x04000000;  // @Inv() IK 함수 호출 (COjw_04_Convert.cs와 동일한 값)

                // 비교 연산자 (CalcCmd에서 사용하는 opcode)
                // 0x17: <, 0x18: >, 0x19: ==, 0x1A: !=, 0x1B: <=, 0x1C: >=
                // 0x1D: && (AND), 0x1E: || (OR), 0x1F: ! (NOT)

                private const int _COMMA2 = 0x0100000;
                #endregion Math Function Address(Kor: 수식 Address 정의)

                #region Internal Variable - Include error variable(Kor: 내부변수 - 컴파일 에러관련 변수 포함)
                private static int m_nVarNum = 0;              // For counting of _M Variable(Kor: _M 변수를 카운팅하기위해 쓰임)
                private static int m_nVarWNum = 0;             // For counting of _W Variable(Kor: _W 변수를 카운팅하기위해 쓰임_
                #region error variable for compiling(Kor: 컴파일 에러관련 변수)
                private static int m_nErrorCode = 0;           // 에러의 내용을 코드로 반환하기 위해 쓰임
                private const int _CNT_ERROR_CODE = 11;
                private static String[] m_pstrError = new String[_CNT_ERROR_CODE]
                                                {
                                                    "",                                                                                                     // No Error(Kor: 이상무)
                                                    "Cannot use \"_\"",                                                                                     //"문자 \"_\"는 사용할 수 없는 문자입니다.",
                                                    "There is no letter : (=)",                                                                             //"대입문(=)이 없습니다.",
                                                    "Cannot use \";\"",                                                                                     //"문자 \";\"는 사용할 수 없는 문자입니다.",
                                                    "You may use some functional letters or illegal sentence",                                              //"특수문자를 사용하거나 완전한 문장이 이루어지지 않았습니다.",
                                                    "Check \"(\" or \")\"",                                                                                 //"문장내 괄호의 숫자가 맞지 않습니다.",
                                                    "There is a problem with the syntax combinations. ex) sqrt(Value, power), pow(Value, power), round(value, digit)",//"문장내 문법 조합에 문제가 있습니다. ex) sqrt(값, 거듭제곱), pow(값, 거듭제곱)",
                                                    "There is a problem with the calculation combinations. ex) cannot use ++, --, +*, */, /+,/- ",          //"문장내 연산 조합에 문제가 있습니다. ex) ++, --, +*, */, /+,/- 등의 복합연산 사용불가[괄호를 이용할 것]",
                                                    "There is a problem with the calculation combinations. ex) cannot use it +,-,*,/ to tail of sentence",  //"문장내 연산 조합에 문제가 있습니다. => +-*/의 연산은 문장의 끝에사용할 수 없습니다.",
                                                    "Unknown errors",                                                                                       //"알수없는 에러 발생",
                                                    "There are no any letters in here"                                                                      //"수식에 사용할 문장이 없습니다."
                                                };
                private static String m_strError_Etc = "";  // for unknown errors(Kor: 알수 없는 에러 발생시 메세지를 담기위해...)
                private static String m_strCompilePath = ""; // All the messages during compiling(Kor: Compile 시 거쳐가는 함수를 전부 나타냄)
                #endregion error variable for compiling(Kor: 컴파일 에러관련 변수)
                #endregion Internal Variable - Include error variable(Kor: 내부변수 - 컴파일 에러관련 변수 포함)

                #region check the variable informations(IsVar, IsMotor, GetVar_Max, GetMotor_Max)(Kor: 변수 갯수 정보 조사(IsVar, IsMotor, GetVar_Max, GetMotor_Max))
                public static bool IsVar(SOjwCode_t SCode, int nVar_Num)
                {
                    try
                    {
                        if (SCode.bInit == false) return false;
                        if (SCode.nVar_Max <= 0) return false;

                        bool bRet = false;
                        for (int i = 0; i < SCode.nVar_Max; i++)
                        {
                            if (SCode.pnVar_Number[i] == nVar_Num)
                            {
                                bRet = true;
                                break;
                            }
                        }
                        return bRet;
                    }
                    catch //(System.Exception e)
                    {
                        return false;
                    }
                }
                public static bool IsMotor(SOjwCode_t SCode, int nMotor_Num)
                {
                    try
                    {
                        if (SCode.bInit == false) return false;
                        if (SCode.nMotor_Max <= 0) return false;

                        bool bRet = false;
                        for (int i = 0; i < SCode.nMotor_Max; i++)
                        {
                            if (SCode.pnMotor_Number[i] == nMotor_Num)
                            {
                                bRet = true;
                                break;
                            }
                        }
                        return bRet;
                    }
                    catch //(System.Exception e)
                    {
                        return false;
                    }
                }
                public static int GetVar_Max(SOjwCode_t SCode) { return SCode.nVar_Max; }
                public static int GetMotor_Max(SOjwCode_t SCode) { return SCode.nMotor_Max; }
                #endregion check the variable informations(IsVar, IsMotor, GetVar_Max, GetMotor_Max)(Kor: 변수 갯수 정보 조사(IsVar, IsMotor, GetVar_Max, GetMotor_Max))

                #region Compile Errors (GetErrorString_..., GetErrorCode, CheckCompileError_...)
                public static String GetErrorString_CompilePath() { return m_strCompilePath; }
                public static String GetErrorString_Error_Etc() { return m_strError_Etc; }
                public static String GetErrorString_By_ErrorCode(int nErrorCode) { return m_pstrError[nErrorCode]; }

                public static int GetErrorCode() { return m_nErrorCode; }

                public static String m_strMessage = "";
                public static int CheckCompileError(String strSource)
                {
                    return CheckCompileError(strSource, false, ref m_strMessage);
                }
                public static int CheckCompileError(String strSource, out String strMessage)
                {
                    strMessage = "";
                    return CheckCompileError(strSource, true, ref strMessage);
                }
                public static int CheckCompileError(String strSource, bool bOut, ref String strMessage)
                {
                    try
                    {
                        m_strError_Etc = "";
                        m_nErrorCode = 0;

                        // remove null, caption, space(Kor: Caption 을 없애고 널, 스페이스를 없앰)
                        strSource = CConvert.RemoveCaption(strSource.ToLower(), true, true);

                        String strTmp;
                        int nRet = 0;
                        #region Ret = 10 - There are no any letters in here(Kor: 수식에 사용할 문장이 없습니다.)
                        if (strSource.Length == 0)
                        {
                            nRet = 10;
                            m_nErrorCode = nRet;
                            if (bOut == true) strMessage = m_pstrError[m_nErrorCode];
                            return m_nErrorCode;
                        }
                        #endregion Ret = 10 - There are no any letters in here(Kor: 수식에 사용할 문장이 없습니다.)
                        #region Ret = 1 - Cannot use "_"(Kor: 문자 "_" 는 사용할 수 없는 문자입니다.)
                        int nIndex = strSource.IndexOf("_");
                        if (nIndex >= 0) nRet = 1;
                        #endregion Ret = 1 - Cannot use "_"(Kor: 문자 "_" 는 사용할 수 없는 문자입니다.)
                        #region Ret = 2 - 대입문(=)이 없습니다.
                        nIndex = strSource.IndexOf("=");
                        if (nIndex < 0) nRet = 2;
                        #endregion Ret = 2 - 대입문(=)이 없습니다.
                        #region Ret = 3 - Cannot use ";"(Kor: 문자 ";"는 사용할 수 없는 문자입니다.)
                        nIndex = strSource.IndexOf(";");
                        if (nIndex >= 0) nRet = 3;
                        #endregion Ret = 3 - Cannot use ";"(Kor: 문자 ";"는 사용할 수 없는 문자입니다.)
                        #region Ret = 4 - You may use some functional letters or illegal sentence(Kor: 특수문자를 사용하거나 완전한 문장이 이루어지지 않았습니다.)
                        strTmp = CConvert.RemoveString(strSource, "=");
                        strTmp = CConvert.RemoveString(strTmp, ".");
                        strTmp = CConvert.RemoveString(strTmp, ",");
                        strTmp = CConvert.RemoveString(strTmp, "(");
                        strTmp = CConvert.RemoveString(strTmp, ")");
                        strTmp = CConvert.RemoveString(strTmp, "-");
                        strTmp = CConvert.RemoveString(strTmp, "+");
                        strTmp = CConvert.RemoveString(strTmp, "*");
                        strTmp = CConvert.RemoveString(strTmp, "/");
                        if (CConvert.CheckCalc_Alpha(strTmp) == false) nRet = 4;
                        #endregion Ret = 4 - You may use some functional letters or illegal sentence(Kor: 특수문자를 사용하거나 완전한 문장이 이루어지지 않았습니다.)
                        #region Ret = 5 - Check "(" or ")"(Kor: 문장내 괄호의 숫자가 맞지 않습니다.)
                        int nFirst = CConvert.GetCnt(strSource, "(");
                        int nLast = CConvert.GetCnt(strSource, ")");
                        if (nFirst != nLast) nRet = 5;
                        #endregion Ret = 5 - Check "(" or ")"(Kor: 문장내 괄호의 숫자가 맞지 않습니다.)
                        #region Ret = 6 - There is a problem with the syntax combinations. ex) sqrt(Value, power), pow(Value, power)(Kor: 문장내 문법 조합에 문제가 있습니다. ex) sqrt(값, 거듭제곱), pow(값, 거듭제곱))
                        nFirst = CConvert.GetCnt(strSource, "sqrt");
                        nLast = CConvert.GetCnt(strSource, "pow");
                        int nAtan2 = CConvert.GetCnt(strSource, "atan2");
                        int nAcos2 = CConvert.GetCnt(strSource, "acos2");
                        int nAsin2 = CConvert.GetCnt(strSource, "asin2");
                        int nRound = CConvert.GetCnt(strSource, "round");
                        int nTmp = CConvert.GetCnt(strSource, ",");
                        if (nFirst + nLast + nAtan2 + nAcos2 + nAsin2 + nRound != nTmp) nRet = 6;
                        #endregion Ret = 6 - There is a problem with the syntax combinations. ex) sqrt(Value, power), pow(Value, power)(Kor: 문장내 문법 조합에 문제가 있습니다. ex) sqrt(값, 거듭제곱), pow(값, 거듭제곱))
                        #region Ret = 7 - There is a problem with the calculation combinations. ex) cannot use ++, --, +*, */, /+,/- (Kor: 문장내 연산 조합에 문제가 있습니다. ex) ++, --, +*, */, /+,/- 등의 복합연산 사용불가[괄호를 이용할 것])
                        // Check a double operation or illegal operation(Kor: 더블부호 및 이상부호 검증)
                        strTmp = CConvert.RemoveChar(strSource, '\r');
                        if (
                            (strTmp.IndexOf("++") >= 0) ||
                            (strTmp.IndexOf("+-") >= 0) ||
                            (strTmp.IndexOf("+*") >= 0) ||
                            (strTmp.IndexOf("+/") >= 0) ||
                            (strTmp.IndexOf("-+") >= 0) ||
                            (strTmp.IndexOf("--") >= 0) ||
                            (strTmp.IndexOf("-*") >= 0) ||
                            (strTmp.IndexOf("-/") >= 0) ||
                            (strTmp.IndexOf("*+") >= 0) ||
                            (strTmp.IndexOf("*-") >= 0) ||
                            (strTmp.IndexOf("**") >= 0) ||
                            (strTmp.IndexOf("*/") >= 0) ||
                            (strTmp.IndexOf("/+") >= 0) ||
                            (strTmp.IndexOf("/-") >= 0) ||
                            (strTmp.IndexOf("/*") >= 0) ||
                            (strTmp.IndexOf("//") >= 0)
                        ) nRet = 7;
                        #endregion Ret = 7 - There is a problem with the calculation combinations. ex) cannot use ++, --, +*, */, /+,/- (Kor: 문장내 연산 조합에 문제가 있습니다. ex) ++, --, +*, */, /+,/- 등의 복합연산 사용불가[괄호를 이용할 것])
                        #region Ret = 8 - There is a problem with the calculation combinations. ex) cannot use it +,-,*,/ to tail of sentence(Kor: 문장내 연산 조합에 문제가 있습니다. => +-*/의 연산은 문장의 끝에사용할 수 없습니다.)
                        if (
                            (strTmp.IndexOf("+\n") >= 0) ||
                            (strTmp.IndexOf("-\n") >= 0) ||
                            (strTmp.IndexOf("*\n") >= 0) ||
                            (strTmp.IndexOf("/\n") >= 0) ||
                            (strTmp.LastIndexOf("+") == strTmp.Length - 1) ||
                            (strTmp.LastIndexOf("-") == strTmp.Length - 1) ||
                            (strTmp.LastIndexOf("*") == strTmp.Length - 1) ||
                            (strTmp.LastIndexOf("/") == strTmp.Length - 1)
                        ) nRet = 8;
                        #endregion Ret = 8 - There is a problem with the calculation combinations. ex) cannot use it +,-,*,/ to tail of sentence(Kor: 문장내 연산 조합에 문제가 있습니다. => +-*/의 연산은 문장의 끝에사용할 수 없습니다.)

                        m_nErrorCode = nRet;
                        if (bOut == true) strMessage = m_pstrError[m_nErrorCode];
                        return m_nErrorCode;
                    }
                    catch (System.Exception e)
                    {
                        #region Ret = 9 - Unknown errors(Kor: 알수없는 에러 발생)
                        m_nErrorCode = 9;
                        m_strError_Etc = "[CheckCompileError]" + e.ToString();
                        if (bOut == true) strMessage = m_pstrError[m_nErrorCode] + m_strError_Etc;
                        #endregion Ret = 9 - Unknown errors(Kor: 알수없는 에러 발생)
                        return m_nErrorCode;
                    }
                }
                public static int CheckCompileError_Code() { return m_nErrorCode; }
                public static String CheckCompileError_String() { return m_pstrError[m_nErrorCode]; }
                #endregion Compile Errors (GetErrorString_..., GetErrorCode, CheckCompileError_...)

                #region If/Else Block Preprocessing
                // ------------------------------------------------------------
                // if/else 블록 전처리
                // 입력: if(조건) { 문장들 } else { 문장들 }
                // 출력: __IF_START__(조건변수)
                //       문장들
                //       __ELSE__
                //       문장들
                //       __ENDIF__
                // ------------------------------------------------------------
                // 비교 연산자를 함수 형태로 변환 (예: "t11 > 0" -> "cmpgt(t11, 0)")
                private static String ConvertComparisonToFunction(String strCondition)
                {
                    String strResult = strCondition;

                    // 논리 연산자 먼저 처리 (&&, ||)
                    // 복잡한 조건문은 단순화를 위해 단일 비교만 지원
                    // 추후 필요시 확장 가능

                    // 비교 연산자 처리 (순서 중요: <=, >=, ==, != 먼저, 그 다음 <, >)
                    // "a <= b" -> "cmple(a, b)"
                    string[] operators = new string[] { "<=", ">=", "==", "!=", "<", ">" };
                    string[] funcNames = new string[] { "cmple", "cmpge", "cmpeq", "cmpne", "cmplt", "cmpgt" };

                    for (int i = 0; i < operators.Length; i++)
                    {
                        int nPos = strResult.IndexOf(operators[i]);
                        if (nPos >= 0)
                        {
                            String strLeft = strResult.Substring(0, nPos).Trim();
                            String strRight = strResult.Substring(nPos + operators[i].Length).Trim();
                            strResult = funcNames[i] + "(" + strLeft + "," + strRight + ")";
                            break; // 하나만 처리
                        }
                    }

                    return strResult;
                }

                // call(x) -> call(x,) 변환 함수
                // call은 1인자 함수이므로 쉼표가 없어서 _UP6_ 마커가 생성되지 않음
                // 인자 뒤에 쉼표를 추가하여 2인자 함수처럼 처리되도록 함
                private static String PreprocessCallFunction(String strSrc)
                {
                    String strResult = strSrc;
                    int nSearchStart = 0;

                    while (true)
                    {
                        // call( 패턴 찾기 (대소문자 무시)
                        int nCallPos = strResult.IndexOf("call(", nSearchStart, StringComparison.OrdinalIgnoreCase);
                        if (nCallPos < 0) break;

                        // call( 다음의 여는 괄호 위치
                        int nOpenParen = nCallPos + 4; // "call" 길이

                        // 대응하는 닫는 괄호 찾기
                        int nDepth = 1;
                        int nCloseParen = nOpenParen + 1;
                        while (nCloseParen < strResult.Length && nDepth > 0)
                        {
                            if (strResult[nCloseParen] == '(') nDepth++;
                            else if (strResult[nCloseParen] == ')') nDepth--;
                            nCloseParen++;
                        }

                        if (nDepth == 0)
                        {
                            // nCloseParen은 ')' 다음 위치를 가리킴
                            // ')' 바로 앞에 ',' 삽입
                            strResult = strResult.Insert(nCloseParen - 1, ",");
                            nSearchStart = nCloseParen + 1; // ',' 추가되었으므로 +1
                        }
                        else
                        {
                            nSearchStart = nCallPos + 1;
                        }
                    }

                    return strResult;
                }

                // inv(x, y) -> inv(x,y,) 변환 함수
                // inv는 2인자 함수이지만 마지막에 쉼표를 추가하여 _UP9_ 마커가 생성되도록 함
                private static String PreprocessInvFunction(String strSrc)
                {
                    String strResult = strSrc;
                    int nSearchStart = 0;

                    while (true)
                    {
                        // inv( 패턴 찾기 (대소문자 무시)
                        int nInvPos = strResult.IndexOf("inv(", nSearchStart, StringComparison.OrdinalIgnoreCase);
                        if (nInvPos < 0) break;

                        // inv( 다음의 여는 괄호 위치
                        int nOpenParen = nInvPos + 3; // "inv" 길이

                        // 대응하는 닫는 괄호 찾기
                        int nDepth = 1;
                        int nCloseParen = nOpenParen + 1;
                        while (nCloseParen < strResult.Length && nDepth > 0)
                        {
                            if (strResult[nCloseParen] == '(') nDepth++;
                            else if (strResult[nCloseParen] == ')') nDepth--;
                            nCloseParen++;
                        }

                        if (nDepth == 0)
                        {
                            // nCloseParen은 ')' 다음 위치를 가리킴
                            // ')' 바로 앞에 ',' 삽입
                            strResult = strResult.Insert(nCloseParen - 1, ",");
                            nSearchStart = nCloseParen + 1; // ',' 추가되었으므로 +1
                        }
                        else
                        {
                            nSearchStart = nInvPos + 1;
                        }
                    }

                    return strResult;
                }

                private static String PreprocessIfElseBlocks(String strSrc)
                {
                    String strResult = strSrc;

                    // if( 패턴 찾기
                    int nSearchStart = 0;
                    while (true)
                    {
                        int nIfPos = strResult.IndexOf("if(", nSearchStart, StringComparison.OrdinalIgnoreCase);
                        if (nIfPos < 0)
                        {
                            nIfPos = strResult.IndexOf("if (", nSearchStart, StringComparison.OrdinalIgnoreCase);
                        }
                        if (nIfPos < 0) break;

                        // if( 다음의 조건 괄호 찾기
                        int nCondStart = strResult.IndexOf('(', nIfPos);
                        if (nCondStart < 0) break;

                        // 대응하는 닫는 괄호 찾기
                        int nDepth = 1;
                        int nCondEnd = nCondStart + 1;
                        while (nCondEnd < strResult.Length && nDepth > 0)
                        {
                            if (strResult[nCondEnd] == '(') nDepth++;
                            else if (strResult[nCondEnd] == ')') nDepth--;
                            nCondEnd++;
                        }
                        if (nDepth != 0) break;
                        nCondEnd--; // ')' 위치

                        String strCondition = strResult.Substring(nCondStart + 1, nCondEnd - nCondStart - 1);
                        // 비교 연산자를 함수 형태로 변환
                        strCondition = ConvertComparisonToFunction(strCondition);

                        // 조건 다음의 { 찾기
                        int nBlockStart = strResult.IndexOf('{', nCondEnd);
                        if (nBlockStart < 0) break;

                        // 대응하는 } 찾기 (중첩 고려)
                        nDepth = 1;
                        int nBlockEnd = nBlockStart + 1;
                        while (nBlockEnd < strResult.Length && nDepth > 0)
                        {
                            if (strResult[nBlockEnd] == '{') nDepth++;
                            else if (strResult[nBlockEnd] == '}') nDepth--;
                            nBlockEnd++;
                        }
                        if (nDepth != 0) break;
                        nBlockEnd--; // '}' 위치

                        String strIfBlock = strResult.Substring(nBlockStart + 1, nBlockEnd - nBlockStart - 1);

                        // else 블록 찾기
                        String strElseBlock = "";
                        int nElseEnd = nBlockEnd + 1;

                        // } 다음에 else가 있는지 확인
                        String strAfterBlock = strResult.Substring(nBlockEnd + 1).TrimStart();
                        if (strAfterBlock.StartsWith("else", StringComparison.OrdinalIgnoreCase))
                        {
                            int nElsePos = strResult.IndexOf("else", nBlockEnd, StringComparison.OrdinalIgnoreCase);
                            int nElseBlockStart = strResult.IndexOf('{', nElsePos);
                            if (nElseBlockStart >= 0)
                            {
                                nDepth = 1;
                                int nElseBlockEnd = nElseBlockStart + 1;
                                while (nElseBlockEnd < strResult.Length && nDepth > 0)
                                {
                                    if (strResult[nElseBlockEnd] == '{') nDepth++;
                                    else if (strResult[nElseBlockEnd] == '}') nDepth--;
                                    nElseBlockEnd++;
                                }
                                if (nDepth == 0)
                                {
                                    nElseBlockEnd--; // '}' 위치
                                    strElseBlock = strResult.Substring(nElseBlockStart + 1, nElseBlockEnd - nElseBlockStart - 1);
                                    nElseEnd = nElseBlockEnd + 1;
                                }
                            }
                        }

                        // 변환된 문자열 생성
                        String strConverted = "";
                        strConverted += "__IF_COND__=" + strCondition.Trim() + "\r\n";
                        strConverted += "__IF_START__\r\n";
                        strConverted += strIfBlock.Trim() + "\r\n";
                        if (strElseBlock.Length > 0)
                        {
                            strConverted += "__ELSE__\r\n";
                            strConverted += strElseBlock.Trim() + "\r\n";
                        }
                        strConverted += "__ENDIF__\r\n";

                        // 원본에서 if문 전체를 변환된 문자열로 교체
                        strResult = strResult.Substring(0, nIfPos) + strConverted + strResult.Substring(nElseEnd);

                        nSearchStart = nIfPos + strConverted.Length;
                    }

                    return strResult;
                }
                #endregion If/Else Block Preprocessing

                private static void StringSeparate(String strSrc, out String[] pstrData)
                {
#if true
                    String strError = CConvert.StringSeparate_In_Compiler(strSrc, out pstrData);
                    if (strError != null)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += "[StringSeparate]" + strError;
                    }
#else
            try
            {
                String[] pstrData2 = new string[256];
                pstrData2.Initialize();

                int nNum = 0;
                String strPrev = "";
                String strCurr = "";
                bool bSqrt = false;
                bool bPow = false;
                bool bAtan2 = false;
                bool bAcos2 = false;
                bool bAsin2 = false;
                bool bCall = false;
                bool bInv = false;

                for (int i = 0; i < strSrc.Length; i++)
                {
                    bool bChange = false;
                    if (i > 0)
                    {
                        //if (CheckSeparate(strPrev, strCurr) == true)
                        String strTmp = "";
                        strTmp += strSrc[i];
                        if (CConvert.CheckSeparate(strCurr, strTmp) == true)
                        {
                            if ((strCurr == "X") || (strCurr == "x")) strCurr = "_X";
                            if ((strCurr == "Y") || (strCurr == "y")) strCurr = "_Y";
                            if ((strCurr == "Z") || (strCurr == "z")) strCurr = "_Z";

                            if (i == strSrc.Length -1)
                            {
                                if ((strTmp.ToLower() == "x") || (strTmp.ToLower() == "y") || (strTmp.ToLower() == "z"))
                                {
                                    bChange = true;
                                }
                            }

                            /////////////////////
                            if (strCurr.Length > 1)
                            {
                                // 모터변수
                                int nCnt = 0;
                                if ((strCurr[0] == 't') || (strCurr[0] == 'T'))
                                {
                                    for (int j = 1; j < strCurr.Length; j++)
                                    {
                                        if (Char.IsDigit(strCurr, j) == true) nCnt++;
                                    }
                                    if ((nCnt > 0) && (nCnt == strCurr.Length - 1)) strCurr = "_T" + strCurr.Substring(1, strCurr.Length - 1);
                                }

                                // v 입력변수
                                //nCnt = 0;
                                else if ((strCurr[0] == 'v') || (strCurr[0] == 'V'))
                                {
                                    for (int j = 1; j < strCurr.Length; j++)
                                    {
                                        if (Char.IsDigit(strCurr, j) == true) nCnt++;
                                    }
                                    if ((nCnt > 0) && (nCnt == strCurr.Length - 1)) strCurr = "_K" + strCurr.Substring(1, strCurr.Length - 1);
                                }

                            }    
                            /////////////////////

                            if (
                                (strCurr.ToLower().IndexOf("sin") == 0) || 
                                (strCurr.ToLower().IndexOf("cos") == 0) || 
                                (strCurr.ToLower().IndexOf("tan") == 0) || 
                                (strCurr.ToLower().IndexOf("pow") == 0)
                                )
                            {
//                                 if (strCurr.Length > 3)
//                                 {
//                                     strCurr = "K" + strCurr;
//                                 }
                                if (Char.IsLetterOrDigit(strTmp, 0) == true)
                                {
                                    strCurr = "K" + strCurr;
                                }                                
                            }
                            else if (
                                ((strCurr.ToLower().IndexOf("atan") == 0) && (strCurr.ToLower().IndexOf("atan2") == 0)) || // atan2
                                ((strCurr.ToLower().IndexOf("atan") == 0) && (strCurr.ToLower().IndexOf("atan2") != 0)) || // atan

                                ((strCurr.ToLower().IndexOf("acos") == 0) && (strCurr.ToLower().IndexOf("acos2") == 0)) || // acos2
                                ((strCurr.ToLower().IndexOf("acos") == 0) && (strCurr.ToLower().IndexOf("acos2") != 0)) || // acos

                                ((strCurr.ToLower().IndexOf("asin") == 0) && (strCurr.ToLower().IndexOf("asin2") == 0)) || // asin2
                                ((strCurr.ToLower().IndexOf("asin") == 0) && (strCurr.ToLower().IndexOf("asin2") != 0)) || // asin

                                //(strCurr.ToLower().IndexOf("atan") == 0) ||
                                (strCurr.ToLower().IndexOf("sqrt") == 0)
                                )
                            {
//                                 if (strCurr.Length > 4)
//                                 {
//                                     strCurr = "K" + strCurr;
//                                 }
                                if (Char.IsLetterOrDigit(strTmp, 0) == true)
                                {
                                    strCurr = "K" + strCurr;
                                }
                            }


                            if (strCurr.ToLower() == "sqrt") bSqrt = true;
                            if (strCurr.ToLower() == "pow") bPow = true;
                            if (strCurr.ToLower() == "atan2") bAtan2 = true;
                            if (strCurr.ToLower() == "acos2") bAcos2 = true;
                            if (strCurr.ToLower() == "asin2") bAsin2 = true;
                            if (strCurr.ToLower() == "call") bCall = true;
                            if (strCurr.ToLower() == "inv") bInv = true;
                            if (strCurr == ",")
                            {
                                if (bSqrt == true) strCurr = ",_UP1_";
                                else if (bPow == true) strCurr = ",_UP0_";
                                else if (bAtan2 == true) strCurr = ",_UP2_";
                                else if (bAcos2 == true) strCurr = ",_UP3_";
                                else if (bAsin2 == true) strCurr = ",_UP4_";
                                else if (bCall == true) strCurr = ",_UP6_";
                                else if (bInv == true) strCurr = ",_UP9_";
                                bSqrt = false;
                                bPow = false;
                                bAtan2 = false;
                                bAcos2 = false;
                                bAsin2 = false;
                                bCall = false;
                                bInv = false;
                            }
                            pstrData2[nNum++] = strCurr;
                            strPrev = strCurr;
                            strCurr = "";
                        }
                    }
                    if (bChange == false) strCurr += strSrc[i];
                    else 
                    {
                        if ((strSrc[i] == 'X') || (strSrc[i] == 'x')) strCurr += "_X";
                        if ((strSrc[i] == 'Y') || (strSrc[i] == 'y')) strCurr += "_Y";
                        if ((strSrc[i] == 'Z') || (strSrc[i] == 'z')) strCurr += "_Z";
                    }
                    bChange = false;
                }
                if (strCurr != "")
                {
                    if (strCurr.Length > 1)
                    {
                        // 모터변수
                        int nCnt = 0;
                        if ((strCurr[0] == 't') || (strCurr[0] == 'T'))
                        {
                            for (int j = 1; j < strCurr.Length; j++)
                            {
                                if (Char.IsDigit(strCurr, j) == true) nCnt++;
                            }
                            if ((nCnt > 0) && (nCnt == strCurr.Length - 1)) strCurr = "_T" + strCurr.Substring(1, strCurr.Length - 1);
                        }

                        // v 입력변수
                        //nCnt = 0;
                        else if ((strCurr[0] == 'v') || (strCurr[0] == 'V'))
                        {
                            for (int j = 1; j < strCurr.Length; j++)
                            {
                                if (Char.IsDigit(strCurr, j) == true) nCnt++;
                            }
                            if ((nCnt > 0) && (nCnt == strCurr.Length - 1)) strCurr = "_K" + strCurr.Substring(1, strCurr.Length - 1);
                        }
                        
                    }     

                    pstrData2[nNum++] = strCurr;
                }
                pstrData = new string[nNum];
                Array.Copy(pstrData2, 0, pstrData, 0, nNum);
                pstrData2 = null;
            }
            catch (System.Exception e)
            {
                m_nErrorCode = 9;
                m_strError_Etc += "[StringSeparate]" + e.ToString();
                pstrData = null;
            }
#endif
                }

                #region previous compile(Kor: 컴파일 전단계)
                private static String Compile_Org2Basic(String strSrc)
                {
                    strSrc = CConvert.RemoveChar(strSrc, ' ');

                    // if/else 블록 제어 키워드는 그대로 통과
                    if (strSrc == "__if_start__" || strSrc == "__IF_START__")
                        return "__IF_START__=0";
                    if (strSrc == "__else__" || strSrc == "__ELSE__")
                        return "__ELSE__=0";
                    if (strSrc == "__endif__" || strSrc == "__ENDIF__")
                        return "__ENDIF__=0";

                    String[] pstrSrc;
                    String[] pstrAsm;
                    String[] pstrSort;
                    String[] pstrLineSort;
                    String[] pstrTmp;
                    int[] pnIndex;
                    int[] pnNum;

                    String strResult = "";
                    bool bError = false;
                    try
                    {
                        #region StringSeparate(strSrc, out pstrSrc); - split string data(Kor: 스트링 데이터를 조각조각 쪼개 놓는다.)
                        StringSeparate(strSrc, out pstrSrc);
                        // DEBUG: inv 함수 토큰 확인
                        #endregion StringSeparate(strSrc, out pstrSrc); - split string data(Kor: 스트링 데이터를 조각조각 쪼개 놓는다.)

                        #region define a new variable with datas in step(Kor: 쪼개진 데이타를 순서에 입각하여 변수정의한다.)
                        int nPush = 0;
                        int nMax = 1000000; // 100000; //ojw5014: 메모리 확장
                        pstrAsm = new string[nMax];

                        pstrAsm.Initialize();
                        int nNum = 0;
                        // Check sqrt and pow(Kor: sqrt 와 pow 를 구분)
                        foreach (String strItem in pstrSrc)
                        {
                            //nMax = ((nMax >= nPush + 2) ? nMax : nPush + 2);
                            //Array.Resize<String>(pstrAsm, nMax);
                            // Overflow Size
                            if (nPush >= nMax) { bError = true; break; }

                            if (
                                (strItem != "(") && (strItem != "{") && (strItem != "[") &&
                                (strItem != ")") && (strItem != "}") && (strItem != "]")
                                ) pstrAsm[nPush] += strItem;

                            if ((strItem == "(") || (strItem == "{") || (strItem == "["))
                            {
                                pstrAsm[nPush++] += "_V" + CConvert.IntToStr(nNum + m_nVarNum);
                                pstrAsm[nPush] += "_V" + CConvert.IntToStr(nNum++ + m_nVarNum) + "="; // add a variable at first point(Kor: 데이타 첫번째에 해당 변수 기록)
                                continue;
                            }
                            if ((strItem == ")") || (strItem == "}") || (strItem == "]"))
                            {
                                strResult += pstrAsm[nPush] + "\r\n";
                                pstrAsm[nPush--] = ""; // increase counter after remove data in stack(Kor: 스택에 있는 데이타를 삭제하고 카운터를 감소)
                                continue;
                            }
                        }

                        int nNum_Back = m_nVarNum;
                        m_nVarNum += nNum;
                        strResult += pstrAsm[0] + "\r\n";

                        pstrSrc = null;
                        pstrAsm = null;
                        #endregion define a new variable with datas in step(Kor: 쪼개진 데이타를 순서에 입각하여 변수정의한다.)

                        #region Arrange _V(variable number)variables(Kor: _V(변수넘버)를 순서에 맞게 정렬한다.)
                        String strTmp = strResult;
                        strTmp = CConvert.RemoveChar(strTmp, '\r');
                        pstrAsm = strTmp.Split('\n');
                        pstrSort = new string[pstrAsm.Length];
                        int nStart = 0;
                        int nEnd = 0;
                        bool bFind = false;
                        int nFind = 0;
                        int nPos = 0;
                        Array.Copy(pstrAsm, pstrSort, pstrAsm.Length);
                        foreach (String strItem in pstrAsm)
                        {
                            if (strItem.IndexOf("_V") == 0)
                            {
                                if (bFind == false)
                                {
                                    nFind++;
                                    bFind = true;
                                    nStart = nPos;
                                }
                            }
                            else if (bFind == true)
                            {
                                bFind = false;
                                nEnd = nPos;

                                for (int i = nStart; i < nEnd; i++)
                                {
                                    strTmp = pstrAsm[i];
                                    int nSortNum = CConvert.StrToInt(strTmp.Substring(2, strTmp.IndexOf("=") - 2)) - nNum_Back;
                                    pstrSort[(nEnd - 1) - nSortNum] = strTmp;
                                }
                            }
                            nPos++;
                        }
                        strResult = "";
                        for (int i = 0; i < pstrSort.Length; i++)
                        {
                            strResult += pstrSort[i] + "\r\n";
                        }
                        pstrAsm = null;
                        pstrSort = null;
                        #endregion Arrange _V(variable number)variables(Kor: _V(변수넘버)를 순서에 맞게 정렬한다.)

                        #region Re-ordered according to the sorted data operation priorities(*, / are most  first)(Kor: Sort 된 데이타를 연산 우선 순위에 따라 (*, / 우선) 다시 정렬)
                        strTmp = strResult;
                        strTmp = CConvert.RemoveChar(strTmp, '\r');
                        pstrAsm = strTmp.Split('\n');
                        nPos = 0;
                        pstrSort = new string[pstrAsm.Length];
                        Array.Copy(pstrAsm, pstrSort, pstrAsm.Length);
                        int nW = m_nVarWNum;
                        foreach (String strItem in pstrAsm)
                        {
                            if (strItem == "") continue;
                            StringSeparate(strItem, out pstrLineSort);
                            pstrTmp = new string[pstrLineSort.Length];
                            int nPos2 = 0;
                            bool bEq = false;
                            for (int i = 0; i < pstrLineSort.Length; i++)
                            {
                                if (bEq == false)
                                {
                                    pstrTmp[nPos2++] = pstrLineSort[i];
                                    if (pstrLineSort[i] == "=") bEq = true;
                                }
                                else
                                {
                                    // inv_V0, call_V0 등 함수명_V 형태는 함수로 인식해야 함
                                    bool bIsFuncWithVar = (pstrLineSort[i] != null) &&
                                        (pstrLineSort[i].StartsWith("inv_") || pstrLineSort[i].StartsWith("call_") ||
                                         pstrLineSort[i].StartsWith("sin_") || pstrLineSort[i].StartsWith("cos_") ||
                                         pstrLineSort[i].StartsWith("tan_") || pstrLineSort[i].StartsWith("asin_") ||
                                         pstrLineSort[i].StartsWith("acos_") || pstrLineSort[i].StartsWith("atan_") ||
                                         pstrLineSort[i].StartsWith("sqrt_") || pstrLineSort[i].StartsWith("pow_") ||
                                         pstrLineSort[i].StartsWith("abs_") || pstrLineSort[i].StartsWith("round_") ||
                                         pstrLineSort[i].StartsWith("atan2_") || pstrLineSort[i].StartsWith("acos2_") ||
                                         pstrLineSort[i].StartsWith("asin2_"));

                                    // 현재 토큰이 함수명인지 체크
                                    bool bIsFunction = (CConvert.CheckCalc_Compare(_SIN | _COS | _TAN | _ASIN | _ACOS | _ATAN | _POW | _SQRT | _ABS | _ATAN2 | _ACOS2 | _ASIN2 | _ROUND | _CALL | _INV, pstrLineSort[i]) != 0);
                                    // 이전 토큰이 함수명인지 체크
                                    bool bPrevIsFunction = (CConvert.CheckCalc_Compare(_SIN | _COS | _TAN | _ASIN | _ACOS | _ATAN | _POW | _SQRT | _ABS | _ATAN2 | _ACOS2 | _ASIN2 | _ROUND | _CALL | _INV, pstrLineSort[i - 1]) != 0);

                                    if (bPrevIsFunction)
                                    {
                                        // 이전 토큰이 함수명이면: +func_V0 형태로 결합 (함수명 + 현재토큰)
                                        // 콤마 마커(_UP9_, _UP6_ 등)는 StringSeparate에서 이미 _V0 내부에 포함됨
                                        string strPrefix = ((pstrLineSort[i - 2] == "=") ? "+" : pstrLineSort[i - 2]);
                                        pstrTmp[nPos2++] = strPrefix + pstrLineSort[i - 1] + pstrLineSort[i];
                                    }
                                    else if (bIsFunction && (pstrLineSort[i - 1] == "="))
                                    {
                                        // = 다음에 함수명이 오면: 스킵 (다음 루프에서 _V0와 함께 결합됨)
                                        // 아무것도 하지 않음 - 다음 반복에서 bPrevIsFunction=true가 되어 처리됨
                                    }
                                    else if (
                                        ((pstrLineSort[i - 1] == "=") && (pstrLineSort[i] != "-")) &&
                                        (!bIsFunction) &&
                                        (!bIsFuncWithVar)
                                    )
                                        pstrTmp[nPos2++] = "+" + pstrLineSort[i];
                                    else if (
                                        ((pstrLineSort[i - 1] == "=") && (pstrLineSort[i] == "-")) &&
                                        (!bIsFunction) &&
                                        (!bPrevIsFunction)
                                    )
                                        continue;
                                    else if (
                                          (CConvert.CheckCalc_Compare(_PLUS | _MINUS | _MUL | _DIV | _MOD | _COMMA, pstrLineSort[i - 1]) != 0) &&
                                          (!bIsFunction) &&
                                          (!bIsFuncWithVar)
                                      )
                                    {
                                        pstrTmp[nPos2++] = pstrLineSort[i - 1] + pstrLineSort[i];
                                    }
                                    else if (bIsFuncWithVar)
                                    {
                                        // inv_V0 등을 그대로 유지 (+ 붙이지 않음)
                                        pstrTmp[nPos2++] = pstrLineSort[i];
                                    }
                                }
                            }
                            Array.Resize<String>(ref pstrLineSort, nPos2);
                            Array.Copy(pstrTmp, pstrLineSort, nPos2);

                            pstrTmp = null;

                            strTmp = "";
                            pnIndex = new int[pstrLineSort.Length];
                            pnNum = new int[pstrLineSort.Length];
                            pnIndex.Initialize();
                            nFind = 0;
                            for (int i = 0; i < pstrLineSort.Length - 1; i++)
                            {
                                if (((pstrLineSort[i].IndexOf('*') < 0) && (pstrLineSort[i].IndexOf('/') < 0)) && ((pstrLineSort[i + 1].IndexOf('*') >= 0) || (pstrLineSort[i + 1].IndexOf('/') >= 0)))
                                {
                                    pnIndex[i] = ++nFind;
                                }
                                else if (((pstrLineSort[i].IndexOf('*') >= 0) || (pstrLineSort[i].IndexOf('/') >= 0)) && (i > 0)) pnIndex[i] = pnIndex[i - 1];
                                else pnIndex[i] = 0;
                            }
                            // last data(Kor: 마지막 데이타)
                            if ((pstrLineSort.Length > 0) && ((pstrLineSort[pstrLineSort.Length - 1].IndexOf('*') >= 0) || (pstrLineSort[pstrLineSort.Length - 1].IndexOf('/') >= 0))) pnIndex[pstrLineSort.Length - 1] = pnIndex[pstrLineSort.Length - 2];
                            else pnIndex[pstrLineSort.Length - 1] = 0;

                            #region Change(Kor: 치환)
                            strTmp = "";
                            pstrTmp = new String[nFind + 1];
                            pstrTmp.Initialize();
                            nFind = 0;
                            for (int i = 0; i < pstrLineSort.Length; i++)
                            {
                                if ((i >= 2) && (pnIndex[i] > 0))
                                {
                                    if (nFind != pnIndex[i])
                                    {
                                        nFind = pnIndex[i];
                                        pstrTmp[nFind - 1] = "_W" + CConvert.IntToStr(nW) + "=";
                                        strTmp += "+_W" + CConvert.IntToStr(nW);
                                        nW++;
                                    }
                                    pstrTmp[pnIndex[i] - 1] += pstrLineSort[i];
                                }
                                else
                                {
                                    strTmp += pstrLineSort[i];
                                }
                            }
                            pstrTmp[pstrTmp.Length - 1] = strTmp;

                            pstrSort[nPos] = "";
                            for (int i = 0; i < pstrTmp.Length; i++)
                            {
                                pstrSort[nPos] += pstrTmp[i] + "\r\n";
                            }
                            pstrTmp = null;
                            pnIndex = null;
                            pnNum = null;
                            #endregion Change(Kor: 치환)
                            nPos++;
                            pstrLineSort = null;
                        }
#if false
                        m_nVarWNum += nW;
#else
                        // ojw5014 : 수식버그 해결
                        m_nVarWNum = nW;
#endif
                        #endregion Re-ordered according to the sorted data operation priorities(*, / are most  first)(Kor: Sort 된 데이타를 연산 우선 순위에 따라 (*, / 우선) 다시 정렬)

                        strResult = "";
                        for (int i = 0; i < pstrSort.Length; i++)
                        {
                            if (pstrSort[i] != "")
                                strResult += pstrSort[i] + "\r\n";
                        }

                        pstrAsm = null;
                        pstrSort = null;

                        if (bError)
                        {
                            m_nErrorCode = 9;
                            m_strError_Etc += "[Compile_Org2Basic] Asem Code -> Memory OverFlow";
                        }
                    }
                    catch (System.Exception e)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += "[Compile_Org2Basic]" + e.ToString() + ((bError == true) ? " Asem Code -> Memory OverFlow" : "");
                        pstrSrc = null;
                        pstrAsm = null;
                        pstrSort = null;
                        pstrLineSort = null;
                        pstrTmp = null;
                        pnIndex = null;
                        pnNum = null;
                    }
                    return strResult;
                }

                private static String Compile_Basic2Asem(String strSrc, bool bTest)
                {
                    String strResult = "";

                    strSrc = CConvert.RemoveChar(strSrc, '\r');
                    strSrc = CConvert.RemoveChar(strSrc, '\t');

                    String[] pstrData = strSrc.Split('\n');
                    String[] pstrTmp = new String[1];
                    String[] pstrVar = new String[1];

                    try
                    {
                        int nCnt = 0;
                        foreach (String strItem in pstrData)
                        {
                            if (strItem == "") continue;

                            Array.Resize<String>(ref pstrVar, nCnt + 1); // num. of variable(Kor: 변수의 갯수)
                            Array.Resize<String>(ref pstrTmp, nCnt + 1); // num. of datas for backup(Kor: 백업받을 데이타의 갯수)
                            // Variable
                            int nIndex = strItem.IndexOf('=');
                            if (nIndex > 0)
                                pstrVar[nCnt] = strItem.Substring(0, nIndex);
                            else return "Error";
                            // backup data
                            nIndex++;
                            pstrTmp[nCnt] = strItem.Substring(nIndex, strItem.Length - nIndex);
                            nCnt++;
                        }
                        Array.Resize<String>(ref pstrData, nCnt);
                        Array.Copy(pstrTmp, pstrData, nCnt);
                        pstrTmp = null;

                        // change to assembler code(Kor: 어셈코드로 변경)
                        // change a variable name(Kor: 변수이름 치환)
                        int nMemNum = 0;
                        for (int i = 0; i < nCnt; i++)
                        {
                            // if/else 블록 키워드는 변수 치환에서 완전히 제외
                            if (pstrVar[i].StartsWith("__IF_") || pstrVar[i].StartsWith("__if_") ||
                                pstrVar[i].StartsWith("__ELSE") || pstrVar[i].StartsWith("__else") ||
                                pstrVar[i].StartsWith("__ENDIF") || pstrVar[i].StartsWith("__endif"))
                            {
                                continue;
                            }

                            //if (pstrVar[i].IndexOf("_T") < 0)
                            if ((pstrVar[i].IndexOf("_T") < 0) && (pstrVar[i].IndexOf("_M") < 0)
                                 && (pstrVar[i].IndexOf("_X") < 0) && (pstrVar[i].IndexOf("_Y") < 0) && (pstrVar[i].IndexOf("_Z") < 0)
                                 && (pstrVar[i].IndexOf("_K") < 0)
                                )
                            {
                                String strTmp = pstrVar[i];
                                if (bTest == false)
                                {
                                    String strTmp2 = pstrVar[i];
                                    pstrVar[i] = "_M" + CConvert.IntToStr(nMemNum); // => Replace all of the variable names.(Kor: 이걸 살리면 변수명을 일괄치환한다.)
                                    for (int j = 0; j < pstrVar.Length; j++)
                                    {
                                        if (pstrVar[j] == strTmp2)
                                        {
                                            pstrVar[j] = pstrVar[i];
                                        }
                                    }
                                }
                                for (int j = 0; j < nCnt; j++)
                                {
                                    //pstrData[j] = CConvert.ChangeString(pstrData[j], strTmp, pstrVar[i]);
                                    pstrData[j] = CConvert.ChangeString_In_MathFunction_By_Compiler(pstrData[j], strTmp, pstrVar[i]);
                                }
                                nMemNum++;
                            }
                        }

                        strResult = "";
                        String strLastCondVar = "_M0"; // 마지막 조건문 결과 저장 변수 (IF_START에서 참조)
                        for (int i = 0; i < nCnt; i++)
                        {
                            // if/else 블록 제어 키워드 처리
                            if (pstrVar[i] == "__IF_START__" || pstrVar[i] == "__if_start__")
                            {
                                // 직전 __IF_COND__의 결과 메모리 주소 사용
                                strResult += "IF_START," + strLastCondVar + "\r\n";
                                continue;
                            }
                            if (pstrVar[i] == "__ELSE__" || pstrVar[i] == "__else__")
                            {
                                strResult += "ELSE,0\r\n";
                                continue;
                            }
                            if (pstrVar[i] == "__ENDIF__" || pstrVar[i] == "__endif__")
                            {
                                strResult += "ENDIF,0\r\n";
                                continue;
                            }
                            // __IF_COND__ 조건문은 건너뜀 - 실제 비교 연산은 이미 별도 줄에서 수행됨
                            // strLastCondVar는 비교 연산(LE, LT 등) 수행 시 자동 설정됨
                            if (pstrVar[i] == "__IF_COND__" || pstrVar[i] == "__if_cond__")
                            {
                                continue;
                            }

#if _REMOVE_CLR_COMMAND
                            strResult += "VAR," + pstrVar[i] + "\r\n";
#else
                            strResult += "VAR," + pstrVar[i] + "\r\n" + "CLR," + pstrVar[i] + "\r\n";
#endif

                            String strTmp = pstrData[i];
                            String strEnd = "";
                            bool bSqrt = false;
                            bool bAtan2 = false;
                            bool bAcos2 = false;
                            bool bRound = false;
                            bool bCall = false;
                            bool bIf = false;
                            bool bInv = false;
                            //bool bAsin2 = false;
                            bool bPow = false;
                            // 비교 연산자 함수
                            bool bCmpLt = false;
                            bool bCmpGt = false;
                            bool bCmpEq = false;
                            bool bCmpNe = false;
                            bool bCmpLe = false;
                            bool bCmpGe = false;
                            //int nIndex = strTmp.IndexOf(",");
                            //if (nIndex < 0)
                            //{
                            //    nIndex = strTmp.IndexOf(";");
                            //    bSqrt = true;
                            //}
                            int nIndex = strTmp.IndexOf(",_UP0_"); // Pow
                            if (nIndex < 0)
                            {
                                nIndex = strTmp.IndexOf(",_UP1_");// Sqrt
                                if (nIndex < 0)
                                {
                                    nIndex = strTmp.IndexOf(",_UP2_");// Atan2
                                    if (nIndex < 0)
                                    {
                                        nIndex = strTmp.IndexOf(",_UP3_");// Acos2
                                        if (nIndex < 0)
                                        {
                                            nIndex = strTmp.IndexOf(",_UP4_");// Asin2
                                            if (nIndex < 0)
                                            {
                                                nIndex = strTmp.IndexOf(",_UP5_");// Round
                                                if (nIndex < 0)
                                                {
                                                    nIndex = strTmp.IndexOf(",_UP6_");// Call
                                                    if (nIndex < 0)
                                                    {
                                                        nIndex = strTmp.IndexOf(",_UP7_");// If
                                                        if (nIndex < 0)
                                                        {
                                                            nIndex = strTmp.IndexOf(",_UP8_");
                                                            if (nIndex < 0)
                                                            {
                                                                nIndex = strTmp.IndexOf(",_UP9_");// Inv
                                                                if (nIndex >= 0) bInv = true;
                                                            }
                                                            //if (nIndex >= 0) bRound = true;
                                                        }
                                                        else bIf = true;
                                                    }
                                                    else bCall = true;
                                                }
                                                else bRound = true;
                                            }
                                            //else bAsin2 = true;
                                        }
                                        else bAcos2 = true;
                                    }
                                    else bAtan2 = true;
                                }
                                else bSqrt = true;
                            }
                            else bPow = true;

                            // 비교 연산자 함수 검사
                            if (nIndex < 0)
                            {
                                nIndex = strTmp.IndexOf(",_UPLT_");
                                if (nIndex >= 0) bCmpLt = true;
                            }
                            if (nIndex < 0)
                            {
                                nIndex = strTmp.IndexOf(",_UPGT_");
                                if (nIndex >= 0) bCmpGt = true;
                            }
                            if (nIndex < 0)
                            {
                                nIndex = strTmp.IndexOf(",_UPEQ_");
                                if (nIndex >= 0) bCmpEq = true;
                            }
                            if (nIndex < 0)
                            {
                                nIndex = strTmp.IndexOf(",_UPNE_");
                                if (nIndex >= 0) bCmpNe = true;
                            }
                            if (nIndex < 0)
                            {
                                nIndex = strTmp.IndexOf(",_UPLE_");
                                if (nIndex >= 0) bCmpLe = true;
                            }
                            if (nIndex < 0)
                            {
                                nIndex = strTmp.IndexOf(",_UPGE_");
                                if (nIndex >= 0) bCmpGe = true;
                            }

                            if (nIndex >= 0)
                            {
                                strTmp = pstrData[i].Substring(0, nIndex);
                                strEnd = pstrData[i].Substring(nIndex, pstrData[i].Length - nIndex);
                                strEnd = CConvert.RemoveString(strEnd, ",_UP0_"); // pow
                                strEnd = CConvert.RemoveString(strEnd, ",_UP1_"); // sqrt
                                strEnd = CConvert.RemoveString(strEnd, ",_UP2_"); // atan2
                                strEnd = CConvert.RemoveString(strEnd, ",_UP3_"); // acos2
                                strEnd = CConvert.RemoveString(strEnd, ",_UP4_"); // asin2
                                strEnd = CConvert.RemoveString(strEnd, ",_UP5_"); // round
                                strEnd = CConvert.RemoveString(strEnd, ",_UP6_"); // call
                                strEnd = CConvert.RemoveString(strEnd, ",_UP7_"); // if
                                strEnd = CConvert.RemoveString(strEnd, ",_UP8_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UP9_"); // inv
                                // 비교 연산자 함수
                                strEnd = CConvert.RemoveString(strEnd, ",_UPLT_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UPGT_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UPEQ_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UPNE_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UPLE_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UPGE_");
                                // 괄호 제거
                                strEnd = CConvert.RemoveString(strEnd, "(");
                                strEnd = CConvert.RemoveString(strEnd, ")");
                            }

                            #region StringSeparate(strTmp, out pstrTmp); - Split the string data.(Kor: 스트링 데이터를 조각조각 쪼개 놓는다.)
                            StringSeparate(strTmp, out pstrTmp);
                            #endregion StringSeparate(strTmp, out pstrTmp); - Split the string data.(Kor: 스트링 데이터를 조각조각 쪼개 놓는다.)
                            //int nPow = 0; // 0 - Pow, 1 - sqrt
                            for (int j = 0; j < pstrTmp.Length; j++)
                            {
                                int nData = CConvert.CheckCalc_Compare(_PLUS | _MINUS | _MUL | _MOD | _DIV, pstrTmp[j]);
                                if (pstrTmp[j].IndexOf("sqrt") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "sqrt");
                                    //nPow = 1;
                                }
                                else if (pstrTmp[j].IndexOf("pow") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "pow");
                                    //nPow = 0;
                                }
                                else if (pstrTmp[j].IndexOf("atan2") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "atan2");
                                    //nPow = 0;
                                }
                                else if (pstrTmp[j].IndexOf("acos2") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "acos2");
                                }
                                else if (pstrTmp[j].IndexOf("asin2") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "asin2");
                                }
                                else if (pstrTmp[j].IndexOf("round") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "round");
                                    //nPow = 1;
                                }
                                else if (pstrTmp[j].IndexOf("call") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "call");
                                }
                                else if (pstrTmp[j].IndexOf("inv") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "inv");
                                }
                                else if (pstrTmp[j].IndexOf("if") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "if");
                                }
                                // 비교 연산자 함수 이름 제거
                                else if (pstrTmp[j].IndexOf("cmplt") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "cmplt");
                                }
                                else if (pstrTmp[j].IndexOf("cmpgt") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "cmpgt");
                                }
                                else if (pstrTmp[j].IndexOf("cmpeq") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "cmpeq");
                                }
                                else if (pstrTmp[j].IndexOf("cmpne") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "cmpne");
                                }
                                else if (pstrTmp[j].IndexOf("cmple") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "cmple");
                                }
                                else if (pstrTmp[j].IndexOf("cmpge") >= 0)
                                {
                                    pstrTmp[j] = CConvert.RemoveString(pstrTmp[j], "cmpge");
                                }

                                if ((nData & _PLUS) != 0)
                                {
                                    strResult += "ADD,";
                                }
                                else if ((nData & _MINUS) != 0)
                                {
                                    strResult += "SUB,";
                                }
                                else if ((nData & _MUL) != 0)
                                {
                                    strResult += "MUL,";
                                }
                                else if ((nData & _MOD) != 0)
                                {
                                    strResult += "MOD,";
                                }
                                else if ((nData & _DIV) != 0)
                                {
                                    strResult += "DIV,";
                                }
                                else
                                {
                                    strResult += pstrTmp[j];
                                    if (j == pstrTmp.Length - 1)
                                    {
                                        strResult += "\r\n";
                                    }
                                    else
                                    {
                                        if (CConvert.CheckCalc_Compare(_PLUS | _MINUS | _MUL | _MOD | _DIV, pstrTmp[j + 1]) != 0)
                                        {
                                            strResult += "\r\n";
                                        }
                                    }
                                }
                            }
                            if (nIndex >= 0)
                            {
                                //strResult += ((bSqrt == false) ? "POW," : "SQRT,") + strEnd + "\r\n";
                                String strOp = "ASIN2,"; // default
                                bool bIsCompare = false;
                                if (bPow == true) strOp = "POW,";
                                else if (bSqrt == true) strOp = "SQRT,";
                                else if (bAtan2 == true) strOp = "ATAN2,";
                                else if (bAcos2 == true) strOp = "ACOS2,";
                                else if (bRound == true) strOp = "ROUND,";
                                else if (bCall == true) strOp = "CALL,";
                                else if (bIf == true) strOp = "IF,";
                                else if (bInv == true) strOp = "INV,";
                                // 비교 연산자 함수 - pow와 같은 방식 (2차 연산)
                                else if (bCmpLt == true) { strOp = "LT,"; bIsCompare = true; }
                                else if (bCmpGt == true) { strOp = "GT,"; bIsCompare = true; }
                                else if (bCmpEq == true) { strOp = "EQ,"; bIsCompare = true; }
                                else if (bCmpNe == true) { strOp = "NE,"; bIsCompare = true; }
                                else if (bCmpLe == true) { strOp = "LE,"; bIsCompare = true; }
                                else if (bCmpGe == true) { strOp = "GE,"; bIsCompare = true; }
                                strResult += strOp + strEnd + "\r\n";
                                // 비교 연산 결과가 저장되는 변수를 IF_START에서 참조하도록 저장
                                if (bIsCompare) strLastCondVar = pstrVar[i];
                                //strResult += "POW," + strEnd + "\r\n";
                            }
                            //strResult += "LD," + strTmp + pstrData[i].IndexOf(1, pstrData[i].Length - 1));
                        }

                        pstrData = null;
                        pstrVar = null;
                        pstrTmp = null;
                    }
                    catch (System.Exception e)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += "[Compile_Basic2Asem]" + e.ToString();

                        pstrData = null;
                        pstrTmp = null;
                        pstrVar = null;
                    }

                    return strResult;
                }

                private static String Compile_Asem2Code(String strSrc)
                {
                    // All Variables are double type
                    // type motor/mem  1'st/2'st  => type ( 0 - number, 1 - xyz, 2 - Motor, 3 - Memory ), Only variables are as follows -> xyz(0-x, 1-y, 2-z), motor(0~255), mem(0~65535), Primarily those calculated, Secondarily calculated ones(After the first calculation)
                    #region Kor
                    // 전부  double 형
                    //  type motor/mem  1차/2차   => type ( 0 - 숫자, 1 - xyz, 2 - Motor, 3 - Memory ), 변수인 경우만 다음의 경우 성립 -> xyz(0-x, 1-y, 2-z), motor(0~255), mem(0~65535), 1차적으로 계산할 계산, 2차적으로 계산할 계산(1차계산 이후 계산)
                    #endregion Kor
                    // 0x 00   00 00     00 01 : var
                    // 0x 00   00 00     00 02 : add
                    // 0x 00   00 00     00 03 : sub
                    // 0x 00   00 00     00 04 : mul
                    // 0x 00   00 00     00 05 : div
                    // 0x 00   00 00     00 06 : sin
                    // 0x 00   00 00     00 07 : cos
                    // 0x 00   00 00     00 08 : tan
                    // 0x 00   00 00     00 09 : asin
                    // 0x 00   00 00     00 0a : acos
                    // 0x 00   00 00     00 0b : atan
                    // 0x 00   00 00     00 0c : sqrt
                    // 0x 00   00 00     00 0d : pow
                    // 0x 00   00 00     00 0e : Clear Data(Value = 0)
                    // 0x 00   00 00     00 0f : abs
                    // 0x 00   00 00     00 10 : atan2
                    // 0x 00   00 00     00 11 : acos2
                    // 0x 00   00 00     00 12 : asin2
                    // 0x 00   00 00     00 13 : mod
                    // 0x 00   00 00     00 14 : round
                    String strResult = "";
                    String strTmp = CConvert.RemoveChar(strSrc, '\r');

                    String[] pstrItem;
                    String[] pstrData = strTmp.Split('\n');
                    String[] pstrOperand = new String[3];
                    int[] pnCode = new int[pstrData.Length];

                    try
                    {
                        foreach (String strItem in pstrData)
                        {
                            int nWidth = 10; // width(Kor: 자릿수)
                            pstrItem = strItem.Split(',');
#if false
                            //if (strItem.IndexOf(",") >= 0)
                            //{
                            //    // Pow 의 경우
                            //    pstrOperand[0] = CConvert.IntToHex(0x0080, nWidth);                    
                            //}
                            //else
                            //{        
#endif
                            int nPos = 0;
                            foreach (String strCode in pstrItem)
                            {
                                pstrOperand[nPos] = strCode;
                                int nData = 0;
                                if (nPos == 0)
                                {
                                    if (strCode == "VAR") nData = 0x00000001;
                                    else if (strCode == "ADD") nData = 0x00000002;
                                    else if (strCode == "SUB") nData = 0x00000003;
                                    else if (strCode == "MUL") nData = 0x00000004;
                                    else if (strCode == "DIV") nData = 0x00000005;
                                    else if (strCode == "SIN") nData = 0x00000006;
                                    else if (strCode == "COS") nData = 0x00000007;
                                    else if (strCode == "TAN") nData = 0x00000008;
                                    else if (strCode == "ASIN") nData = 0x00000009;
                                    else if (strCode == "ACOS") nData = 0x0000000a;
                                    else if (strCode == "ATAN") nData = 0x0000000b;
                                    else if (strCode == "SQRT") nData = 0x0000000c;
                                    else if (strCode == "POW") nData = 0x0000000d;
                                    else if (strCode == "CLR") nData = 0x0000000e;
                                    else if (strCode == "ABS") nData = 0x0000000f;
                                    else if (strCode == "ATAN2") nData = 0x00000010;
                                    else if (strCode == "ACOS2") nData = 0x00000011;
                                    else if (strCode == "ASIN2") nData = 0x00000012;
                                    else if (strCode == "MOD") nData = 0x00000013;
                                    else if (strCode == "ROUND") nData = 0x00000014;
                                    else if (strCode == "CALL") nData = 0x00000015;
                                    else if (strCode == "IF") nData = 0x00000016;
                                    // 비교 연산자
                                    else if (strCode == "LT") nData = 0x00000017;   // <
                                    else if (strCode == "GT") nData = 0x00000018;   // >
                                    else if (strCode == "EQ") nData = 0x00000019;   // ==
                                    else if (strCode == "NE") nData = 0x0000001a;   // !=
                                    else if (strCode == "LE") nData = 0x0000001b;   // <=
                                    else if (strCode == "GE") nData = 0x0000001c;   // >=
                                    // 논리 연산자
                                    else if (strCode == "AND") nData = 0x0000001d;  // &&
                                    else if (strCode == "OR") nData = 0x0000001e;   // ||
                                    else if (strCode == "NOT") nData = 0x0000001f;  // !
                                    // IK 호출
                                    else if (strCode == "INV") nData = 0x00000020;
                                    // if/else/endif 블록 제어
                                    else if (strCode == "IF_START") nData = 0x00000021;
                                    else if (strCode == "ELSE") nData = 0x00000022;
                                    else if (strCode == "ENDIF") nData = 0x00000023;
                                    else continue;
                                    pstrOperand[0] = CConvert.IntToHex(nData, nWidth);
                                }
                                else if (nPos == 1)
                                {
                                    if (strCode.IndexOf("sin") == 0) nData = 0x00000600;
                                    else if (strCode.IndexOf("cos") == 0) nData = 0x00000700;
                                    else if (strCode.IndexOf("tan") == 0) nData = 0x00000800;

                                    else if (strCode.IndexOf("atan2") == 0) nData = 0x00001000;
                                    else if (strCode.IndexOf("acos2") == 0) nData = 0x00001100;
                                    else if (strCode.IndexOf("asin2") == 0) nData = 0x00001200;

                                    else if (strCode.IndexOf("asin") == 0) nData = 0x00000900;
                                    else if (strCode.IndexOf("acos") == 0) nData = 0x00000a00;
                                    else if (strCode.IndexOf("atan") == 0) nData = 0x00000b00;
                                    else if (strCode.IndexOf("sqrt") == 0) nData = 0x00000c00;
                                    else if (strCode.IndexOf("pow") == 0) nData = 0x00000d00;
                                    else if (strCode.IndexOf("clr") == 0) nData = 0x00000e00;
                                    else if (strCode.IndexOf("abs") == 0) nData = 0x00000f00;
                                    else if (strCode.IndexOf("mod") == 0) nData = 0x00000013;
                                    else if (strCode.IndexOf("round") == 0) nData = 0x00001400;
                                    else if (strCode.IndexOf("call") == 0) nData = 0x00001500;
                                    else if (strCode.IndexOf("inv") == 0) nData = 0x00002000;
                                    else if (strCode.IndexOf("if") == 0) nData = 0x00001600;

                                    int nIndex = 0;
                                    if (nData > 0)
                                    {
                                        nIndex = strCode.IndexOf("_");// +1;
                                        int nData1 = CConvert.StrToInt(pstrOperand[0]);
                                        pstrOperand[0] = CConvert.IntToHex(nData | nData1, nWidth);
                                        pstrOperand[nPos] = strCode.Substring(nIndex, strCode.Length - nIndex);
                                    }
                                    //int nType = 0;
                                    nIndex = 0;
                                    if ((strCode == "_X") || (strCode == "_Y") || (strCode == "_Z"))
                                    {
                                        //nType = 0x1000000000;
                                        nIndex = ((strCode == "_X") ? 0 : ((strCode == "_Y") ? 1 : 2));
                                    }
                                    else if (strCode.IndexOf("_K") >= 0)
                                    {
                                        nIndex = CConvert.StrToInt(CConvert.RemoveString(strCode, "_K"));// +3; // x(0), y(1), z(2), v(3~ )
                                    }
                                    //else if (strCode[0] == 't')
                                    else if (strCode.IndexOf("_T") >= 0)
                                    {
                                        //nType = 0x2000000000;
                                        nIndex = CConvert.StrToInt(CConvert.RemoveString(strCode, "_T"));
                                    }
                                    else if (strCode.IndexOf("_M") >= 0)
                                    {
                                        //nType = 0x3000000000;
                                        nIndex = CConvert.StrToInt(CConvert.RemoveString(strCode, "_M"));
                                    }
                                }

                                nPos++;
                            }
                            for (int i = 0; i < nPos; i++)
                            {
                                strResult += pstrOperand[i] + ((i == nPos - 1) ? "\r\n" : ",");
                            }

                            //}
                            pstrItem = null;
                        }
                        pnCode = null;

                    }
                    catch (System.Exception e)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += "[Compile_Asem2Code]" + e.ToString();

                        pstrItem = null;
                        pstrData = null;
                        pstrOperand = null;
                        pnCode = null;
                    }

                    return strResult;
                }

                private static void Compile_CodeString2Code(String strSrc, out SOjwCode_t SCode)//out double [] adCode0, out double [] adCode1)//, out double [] adCode2)
                {
                    // Define variables(Kor: 변수 정의)
                    // 0x000-0x0ff - Motor(More than 255 preliminary data)(Kor: 모터(255 이상은 예비))
                    // 0x100-0x1ff - 0x100(x), 0x101(y), 0x102(z) .... reserved(Kor: 나머지는 예비)
                    // 0x200-0xfff - _M variables
                    SCode.nCnt_Operation = 0;
                    SCode.adOperation_Memory = new double[_CNT_ADDRESS + 1];
                    SCode.alOperation_Cmd = new long[1];
                    SCode.adOperation_Data = new double[1];
                    //afCode2 = new double[1];
                    SCode.adOperation_Memory.Initialize();
                    SCode.alOperation_Cmd.Initialize();
                    SCode.adOperation_Data.Initialize();
                    SCode.bInit = true;
                    
                    // 20160816
                    SCode.bPython = false;
                    SCode.strPython = String.Empty;

                    SCode.nMotor_Max = 0;
                    SCode.nVar_Max = 0;
                    SCode.pnMotor_Number = null;
                    SCode.pnVar_Number = null;

                    //afCode2.Initialize();
                    String strTmp = CConvert.RemoveString(strSrc, "\r");

                    String[] pstrItem;
                    String[] pstrLine = strTmp.Split('\n');

                    //int nTmp_Cnt_Mot = 0;
                    //int nTmp_Cnt_Var = 0;
                    //int[] pnTmp_Mot = null;
                    //int[] pnTmp_Var = null;

                    try
                    {
                        int nLine = 0;
                        foreach (String strLine in pstrLine)
                        {
                            Array.Resize<long>(ref SCode.alOperation_Cmd, nLine + 1);
                            Array.Resize<double>(ref SCode.adOperation_Data, nLine + 1);
                            //Array.Resize<double>(ref afCode2, nLine + 1);
                            SCode.alOperation_Cmd[nLine] = 0;
                            SCode.adOperation_Data[nLine] = 0.0f;
                            //afCode2[nLine] = 0.0f;

                            pstrItem = strLine.Split(',');
                            int nPos = 0;
                            foreach (String strItem in pstrItem)
                            {
                                if (nPos == 0)
                                {
                                    SCode.alOperation_Cmd[nLine] = CConvert.HexStrToLong(strItem);
                                }
                                else if (nPos == 1)
                                {
                                    // type motor/mem 1st/2st   => type ( 0 - num, 1 - xyz, 2 - Motor, 3 - Memory ), Only variables are as follows -> xyz(0-x, 1-y, 2-z), motor(0~255), mem(0~65535), Primarily those calculated, Secondarily calculated ones(After the first calculation)
                                    // 0x 00   00 00     01 : var => change
                                    // 0x 00             01 : var
                                    // for example ) if we 'or' calculation with "1   00 00     00" -> xyz type
                                    #region Kor
                                    //    type motor/mem 1차/2차   => type ( 0 - 숫자, 1 - xyz, 2 - Motor, 3 - Memory ), 변수인 경우만 다음의 경우 성립 -> xyz(0-x, 1-y, 2-z), motor(0~255), mem(0~65535), 1차적으로 계산할 계산, 2차적으로 계산할 계산(1차계산 이후 계산)
                                    // 0x 00   00 00     01 : var => 변경
                                    // 0x 00             01 : var
                                    //     1   00 00     00 를 or 할 경우 xyz 타입의
                                    #endregion Kor
                                    long lAddr = 0;
                                    strTmp = "0";
                                    int nOp1 = -1;
                                    if (strItem.IndexOf("_M") >= 0)
                                    {
                                        strTmp = CConvert.RemoveString(strItem, "_M");
                                        nOp1 = _ADDRESS_M + CConvert.StrToInt(strTmp);
                                        lAddr = _ADDRESS_M + 1; // cannot start from 0 address(Kor: 0번지부터 시작하면 골치아파지므로...)
                                    }
                                    else if (strItem == "_X") { nOp1 = _ADDRESS_X; lAddr = _ADDRESS_X + 1; }
                                    else if (strItem == "_Y") { nOp1 = _ADDRESS_Y; lAddr = _ADDRESS_X + 1; }
                                    else if (strItem == "_Z") { nOp1 = _ADDRESS_Z; lAddr = _ADDRESS_X + 1; }
                                    else if (strItem.IndexOf("_K") >= 0)
                                    {
                                        strTmp = CConvert.RemoveString(strItem, "_K");
                                        nOp1 = _ADDRESS_V + CConvert.StrToInt(strTmp); // + 3; // x(0), y(1), z(2), v(3~ )
                                        lAddr = _ADDRESS_X + 1;


                                        int nVarNum = CConvert.StrToInt(strTmp);
                                        bool bFind = false;
                                        /*if (SCode.nVar_Max > 0)*/ { for (int i = 0; i < SCode.nVar_Max; i++) { if (SCode.pnVar_Number[i] == nVarNum) { bFind = true; break; } } }
                                        if (bFind == false)
                                        {
                                            SCode.nVar_Max++;
                                            Array.Resize<int>(ref SCode.pnVar_Number, SCode.nVar_Max);
                                            SCode.pnVar_Number[SCode.nVar_Max - 1] = CConvert.StrToInt(strTmp);
                                        }

                                        //SCode.nVar_Max++;
                                        //Array.Resize<int>(ref SCode.pnVar_Number, SCode.nVar_Max);
                                        //SCode.pnVar_Number[SCode.nVar_Max - 1] = CConvert.StrToInt(strTmp);

                                        //                                 nTmp_Cnt_Var++;
                                        //                                 Array.Resize<int>(ref pnTmp_Var, nTmp_Cnt_Var);
                                        //                                 pnTmp_Var[nTmp_Cnt_Var - 1] = CConvert.StrToInt(strTmp);
                                    }
                                    if (strItem.IndexOf("_T") >= 0)
                                    {
                                        strTmp = CConvert.RemoveString(strItem, "_T");
                                        nOp1 = CConvert.StrToInt(strTmp);
                                        lAddr = _ADDRESS_M + 1;


                                        int nMotorNum = CConvert.StrToInt(strTmp);
                                        bool bFind = false;
                                        if (SCode.alOperation_Cmd[nLine] != 1) bFind = true;
                                        else
                                        {
                                            for (int i = 0; i < SCode.nMotor_Max; i++)
                                            {
                                                if (SCode.pnMotor_Number[i] == nMotorNum)
                                                {
                                                    bFind = true; break;
                                                }
                                            }
                                        }

                                        if (bFind == false)
                                        {
                                            SCode.nMotor_Max++;
                                            Array.Resize<int>(ref SCode.pnMotor_Number, SCode.nMotor_Max);
                                            SCode.pnMotor_Number[SCode.nMotor_Max - 1] = nOp1;
                                        }

                                        //SCode.nMotor_Max++;
                                        //Array.Resize<int>(ref SCode.pnMotor_Number, SCode.nMotor_Max);
                                        //SCode.pnMotor_Number[SCode.nMotor_Max - 1] = nOp1;

                                        //                                 nTmp_Cnt_Mot++;
                                        //                                 Array.Resize<int>(ref pnTmp_Mot, nTmp_Cnt_Mot);
                                        //                                 pnTmp_Mot[nTmp_Cnt_Mot - 1] = nOp1;
                                    }
                                    //SCode.alOperation_Cmd[nLine] |= lAddr << 8;
                                    SCode.alOperation_Cmd[nLine] |= lAddr * 256 * 256;
                                    //strTmp = 
                                    if (nOp1 >= 0)
                                    {
                                        //SCode.adOperation_Memory[nLine] = nOp1;
                                        SCode.adOperation_Data[nLine] = (double)nOp1;
                                    }
                                    else
                                    {
                                        SCode.adOperation_Data[nLine] = CConvert.StrToDouble(strItem);
                                    }
                                    //SCode.adOperation_Data[nLine] = (double)nOp1;
                                    //afCode2[nLine] = 0;// 아직 안쓰임
                                }
                                //else
                                //{
                                //    afCode2[nLine] = CConvert.S_HexStrToInt(strItem);
                                //}
                                nPos++;
                            }
                            pstrItem = null;
                            nLine++;
                        }

                        pstrLine = null;



                        //                 SCode.nVar_Max = nTmp_Cnt_Var;
                        //                 SCode.pnMotor_Number = new int[1];
                        //                 SCode.pnVar_Number = new int[1];
                        //                 //Array.Copy(pnTmp_Mot, SCode.pnMotor_Number, SCode.nMotor_Max);
                        //                 //Array.Copy(pnTmp_Var, SCode.pnVar_Number, SCode.nVar_Max);
                        //                 SCode.nMotor_Max = 0;
                        //                 for (int i = 0; i < nTmp_Cnt_Mot; i++)
                        //                 {
                        //                     pnTmp_Mot[i]
                        //                     Array.Resize<int>(ref SCode.pnVar_Number, SCode.nVar_Max);
                        //                 }
                        //                 pnTmp_Mot = null;
                        //                 pnTmp_Var = null;

                        SCode.nCnt_Operation = nLine;
                    }
                    catch (System.Exception e)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += "[Compile_CodeString2Code]" + e.ToString();

                        pstrItem = null;
                        pstrLine = null;
                    }
                }

                // cannot decryption by this(Kor: 이건 암호화 해제는 못한다.)
#if false       // public static bool Compile(String[] pstrData, out SOjwCode_t[] pSCode, out String strMessage) // 512개 전부
                public static bool Compile( // 512개 전부
                                    String[] pstrData,
                                    out SOjwCode_t[] pSCode,
                                    out String strMessage
                                 )
                {
                    bool bRet = true;
                    pSCode = new SOjwCode_t[512];
                    strMessage = "";
                    try
                    {
                        for (int i = 0; i < 512; i++)
                        {
                            String strTmp;
                            String strCompile1, strCompile2, strCompile3, strCompile3_Debug, strCompile4;

                            int nResult = Compile(pstrData[i],
                                                out strCompile1, out strCompile2, out strCompile3, out strCompile3_Debug, out strCompile4,
                                                out pSCode[i], out strTmp);
                            if (nResult != 0)
                            {
                                bRet = false;
                                strMessage += "[" + CConvert.IntToStr(i) + "]" + strTmp;
                            }
                        }
                    }
                    catch
                    {
                        bRet = false;
                    }
                    return bRet;
                }
#endif
                // Decryption will be using the [byte Array] must manually disable it.(Kor: 암호화 해제는 반드시 byte Array 로 일일이 해제해 주어야 한다.)
                public static bool Compile(
                                    byte[] pbyteData,
                                    out SOjwCode_t SCode
                                 )
                {
                    CEncryption.SetEncrypt("OJW5014");
                    String strData = Encoding.Default.GetString(CEncryption.Encryption(false, pbyteData)); // Decryption(Kor: 암호화 해제)
                    return Compile(strData, out SCode);
                }

                public static bool Compile(
                                    String strData,
                                    out SOjwCode_t SCode
                                 )
                {
                    bool bRet = true;
                    String strTmp = "";
                    String strCompile1, strCompile2, strCompile3, strCompile3_Debug, strCompile4;
                    strCompile1 = "";
                    strCompile2 = "";
                    strCompile3 = "";
                    strCompile3_Debug = "";
                    strCompile4 = "";
                    int nResult = Compile(strData, out SCode,
                                        false, // No Check Compile Error
                                        ref strCompile1,
                                        ref strCompile2,
                                        ref strCompile3,
                                        false, ref strCompile3_Debug,
                                        ref strCompile4,
                                        false, ref strTmp);
                    if (nResult != 0)
                    {
                        bRet = false;
                    }
                    return bRet;
                }

                public static int Compile(
                                    String strData, out SOjwCode_t SCode,
                                    bool bCheckCompileError,
                                    ref String strCompile1,
                                    ref String strCompile2,
                                    ref String strCompile3,
                                    bool bOut3_Debug, ref String strCompile3_Debug,
                                    ref String strCompile4,
                                    bool bOut_Message, ref String strMessage
                                 )
                {
                    //EncryptionSet("OJW5014");
                    //strData = Encryption(false, strData); // Decryption(Kor: 암호화 해제)

                    #region Init
                    #region Init - Error
                    m_nErrorCode = 0;
                    m_strError_Etc = "";
                    #endregion Init - Error

                    // Init
                    strCompile1 = "";
                    strCompile2 = "";
                    strCompile3 = "";
                    if (bOut3_Debug == true) strCompile3_Debug = "";
                    strCompile4 = "";

                    if (bOut_Message == true)
                    {
                        m_strCompilePath = "[Compile";
                        strMessage = "";
                    }

                    #region Class Code structure Initialize
                    SCode.bInit = false;

                    // 20160816
                    SCode.bPython = false;
                    SCode.strPython = String.Empty;

                    SCode.nCnt_Operation = 0;
                    SCode.adOperation_Data = null;
                    SCode.alOperation_Cmd = null;
                    SCode.adOperation_Memory = null;

                    SCode.nMotor_Max = 0;
                    SCode.pnMotor_Number = null;
                    SCode.nVar_Max = 0;
                    SCode.pnVar_Number = null;
                    #endregion Class Code structure Initialize

                    #endregion Init

                    if (strData != null)
                    {
                        if (strData.Length > 0)
                        {
                            if (strData[0] == '!')
                            {
                                SCode.bPython = true;
                                SCode.strPython = strData.Substring(1);

                                string strTmp = SCode.strPython.ToLower();
                                strTmp = Ojw.CConvert.RemoveString(strTmp, "\r");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "\n", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, " ", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "=", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "+", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "-", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "*", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "/", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "%", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "sqrt", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "pow", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "sin", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "cos", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "tan", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "asin2", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "acos2", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "atan2", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "asin", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "acos", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "atan", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "=", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "(", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, ")", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "#", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "mod", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "abs", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "round", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "call", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "inv", ",");
                                strTmp = Ojw.CConvert.ChangeString(strTmp, "if", ",");
                                string [] pstrTmp = strTmp.Split(',');
                                // v 변수, mot 변수 체크
                                int[] pnV = new int[1];
                                int[] pnT = new int[1];
                                pnV[0] = -1;
                                pnT[0] = -1;
                                int nCnt_V = 0;
                                int nCnt_T = 0;
                                foreach (string strItem in pstrTmp)
                                {
                                    if (String.IsNullOrEmpty(strItem) == false)
                                    {
                                        if (strItem.Length > 1)
                                        {
                                            if (strItem[0] == 'v')
                                            {
                                                if (Ojw.CConvert.IsDigit(strItem.Substring(1)) == true)
                                                {
                                                    int nTmp = Ojw.CConvert.StrToInt(strItem.Substring(1));
                                                    bool bOk = true;
                                                    foreach (int nV in pnV)
                                                    {
                                                        if (nV == nTmp)
                                                        {
                                                            bOk = false;
                                                            break;
                                                        }
                                                    }
                                                    if (bOk == true)
                                                    {
                                                        Array.Resize<int>(ref pnV, nCnt_V+1);
                                                        pnV[nCnt_V++] = nTmp;
                                                    }
                                                }
                                            }
                                            else if (strItem[0] == 't')
                                            {
                                                if (Ojw.CConvert.IsDigit(strItem.Substring(1)) == true)
                                                {
                                                    int nTmp = Ojw.CConvert.StrToInt(strItem.Substring(1));
                                                    bool bOk = true;
                                                    foreach (int nT in pnT)
                                                    {
                                                        if (nT == nTmp)
                                                        {
                                                            bOk = false;
                                                            break;
                                                        }
                                                    }
                                                    if (bOk == true)
                                                    {
                                                        Array.Resize<int>(ref pnT, nCnt_T + 1);
                                                        pnT[nCnt_T++] = nTmp;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                SCode.nMotor_Max = nCnt_T;
                                SCode.pnMotor_Number = new int[((nCnt_T < 1) ? 1 : nCnt_T)];
                                SCode.nVar_Max = nCnt_V;
                                SCode.pnVar_Number = new int[((nCnt_V < 1) ? 1 : nCnt_V)];

                                Array.Copy(pnT, SCode.pnMotor_Number, nCnt_T);
                                Array.Copy(pnV, SCode.pnVar_Number, nCnt_V);

                                SCode.bInit = true;

                                pnT = null;
                                pnV = null;
                                return m_nErrorCode;
                            }
                        }
                        else
                        {
                            //"There are no any letters in here"                                                                      
                            //"수식에 사용할 문장이 없습니다."
                            m_nErrorCode = 10;
                            if (bOut_Message == true)
                            {
                                strMessage = m_pstrError[m_nErrorCode] + m_strError_Etc;
                                m_strCompilePath += "(Error" + CConvert.IntToStr(m_nErrorCode) + ")";
                            }
                            return m_nErrorCode;
                        }
                    }

                    try
                    {
                        #region Pre-Inspection(Kor: 사전검사)
                        if (bCheckCompileError == true)
                        {
                            if (bOut_Message == true) m_strCompilePath += "-PreInspection";// "-사전검사";
                            int nError = 0;
                            if (bOut_Message == true) nError = CheckCompileError(strData, out strMessage);
                            else nError = CheckCompileError(strData);

                            if (nError != 0)
                            {
                                m_nErrorCode = nError;
                                if (bOut_Message == true)
                                {
                                    strMessage = m_pstrError[nError] + m_strError_Etc;
                                    m_strCompilePath += "(Error" + CConvert.IntToStr(nError) + ")";
                                }
                                return nError;
                            }
                        }
                        #endregion Pre-Inspection(Kor: 사전검사)


                        #region Variable initialization(Kor: 변수초기화)
                        m_nVarNum = 0;
                        m_nVarWNum = 0;
                        #endregion Variable initialization(Kor: 변수초기화)

                        #region If/Else Block Preprocessing
                        // if(조건) { } else { } 블록을 내부 키워드로 변환
                        strData = PreprocessIfElseBlocks(strData);
                        #endregion If/Else Block Preprocessing

                        #region call() Preprocessing
                        // call(x) -> call(x,) 변환 (1인자 함수를 2인자처럼 처리하여 _UP6_ 마커 생성)
                        strData = PreprocessCallFunction(strData);
                        #endregion call() Preprocessing

                        #region inv() Preprocessing
                        // inv(x,y) 함수는 StringSeparate에서 콤마를 _UP9_ 마커로 변환
                        // 괄호 삽입 단계에서 inv() 내부 콤마는 건너뛰므로 전처리 불필요
                        // strData = PreprocessInvFunction(strData); // 비활성화
                        #endregion inv() Preprocessing

                        #region compile - 1st step(strCompile1)(Kor: 컴파일 - 1단계(strCompile1))
                        if (bOut_Message == true) m_strCompilePath += "-1st step(strCompile1)";// "-1단계";

                        ////////////////////////////////////////////////////////////////////////////////
                        // Filter them specified word(Kor: 지시어 걸러내기)
                        int nPos_Cmd_0 = strData.IndexOf('{');
                        int nPos_Cmd_1 = strData.IndexOf('}');
                        int nPos_Cmd_2 = strData.IndexOf("\r\n");
                        bool bSecret = false; // Checking encryption(Kor: 암호화가 있는 수식인지)
                        bool bWheel = false; // Checking wheel type(Kor: 바퀴형 제어수식인지)
                        if ((nPos_Cmd_0 >= 0) && (nPos_Cmd_1 >= 0))
                        {
                            String strCommand = strData.Substring(nPos_Cmd_0, nPos_Cmd_1 + 1);
                            strData = strData.Remove(nPos_Cmd_0, nPos_Cmd_2 + ((nPos_Cmd_2 >= 0) ? 2 : 0));
                            bSecret = ((strData.IndexOf('s') >= 0) || (strData.IndexOf('S') >= 0) ? true : false);
                            bWheel = ((strData.IndexOf('w') >= 0) || (strData.IndexOf('W') >= 0) ? true : false);
                        }
                        ////////////////////////////////////////////////////////////////////////////////
                        if (bSecret == true) // Decryption - Check 1~4 data is '5014'[master password](Kor: 암호화 해제 - 첫번째~4번째 글자가 '5014' 인지 반드시 확인해야 한다. 암호화시 앞에 넣도록 되어 있는 상수값)
                        {
                            if (strData.Length > 4)
                            {
                                if ((strData[0] == 0x05) && (strData[0] == 0x00) && (strData[0] == 0x01) && (strData[0] == 0x04))
                                {
                                    String strData2 = strData.Substring(4, strData.Length - 4); // 4 char 을 제거...
                                    strData = "";
                                    for (int i = 0; i < strData.Length; i++)
                                    {
                                        // input your decryption code(Kor: 여기에 암호화 해제 코드를 넣는다.)
                                        strData += CEncryption.LetterCode2Letter(i % 256, strData2[i]);
                                    }
                                }
                            }
                        }
#if false
                        //String strOrg = strData;
                        //String strTmp = CConvert.RemoveCaption(strOrg, true, true); // remove caption(Kor: 캡션 제거)
                        //strTmp = strTmp.ToLower(); // 전부 소문자로 변경

                        //if (bOut1 == true) strCompile1 = strTmp; // -> Result
#else
                        String strTmp;
                        strCompile1 = (CConvert.RemoveCaption(strData, true, true)).ToLower(); // remove caption(Kor: 캡션 제거)
#endif

                        if (m_nErrorCode != 0)
                        {
                            if (bOut_Message == true)
                            {
                                strMessage = m_pstrError[m_nErrorCode] + m_strError_Etc;
                                m_strCompilePath += "(Error" + CConvert.IntToStr(m_nErrorCode) + ")";
                            }
                            return m_nErrorCode;
                        }

                        // Load up the parentheses["("] following the comma[","] in sqrt, pow(Kor: sqrt, pow 의 "," 다음에 괄호"(" 집어넣기))
                        // inv() 함수 내부의 콤마는 건너뜀 (inv는 _UP9_ 마커를 사용)
                        int nTmp = -1;
                        bool bError = true;
                        while (bError == true)
                        {
                            strTmp = "";
                            bool bCheck = false;
                            int nInvDepth = -1; // inv() 함수 내부인지 추적 (-1: 아님, 0이상: 괄호 깊이)

                            strTmp += strCompile1[0];
                            for (int i = 1; i < strCompile1.Length; i++)
                            {
                                // inv( 패턴 감지
                                if (i >= 3 && strCompile1.Substring(i - 3, 4).ToLower() == "inv(")
                                {
                                    nInvDepth = 1; // inv( 시작
                                }
                                // inv 내부일 때 괄호 깊이 추적
                                if (nInvDepth >= 0)
                                {
                                    if (strCompile1[i] == '(') nInvDepth++;
                                    else if (strCompile1[i] == ')')
                                    {
                                        nInvDepth--;
                                        if (nInvDepth == 0) nInvDepth = -1; // inv() 함수 종료
                                    }
                                }

                                // inv() 내부가 아닐 때만 괄호 삽입
                                if ((nTmp < 0) && (strCompile1[i - 1] == ',') && (strCompile1[i] != '(') && (nInvDepth < 0))
                                {
                                    bCheck = true;
                                    nTmp = 0;
                                    strTmp += "(" + strCompile1[i];
                                }
                                else
                                {
                                    strTmp += strCompile1[i];
                                }
                                if (nTmp >= 0)
                                {
                                    if (strCompile1[i] == '(') nTmp++;
                                    else if (strCompile1[i] == ')') nTmp--;

                                    if (nTmp < 0)
                                    {
                                        strTmp += ")";
                                    }
                                }
                            }
                            bError = bCheck;

                            strCompile1 = strTmp;
                        }
                        #endregion compile - 1st step(strCompile1)(Kor: 컴파일 - 1단계(strCompile1))

                        #region compile - 2st step(strCompile2)(Kor: 컴파일 - 2단계(strCompile2))
                        if (bOut_Message == true) m_strCompilePath += "-2st step";// "-2단계";
                        strTmp = CConvert.RemoveChar(strCompile1, '\r');
                        String[] pstrTmp = strTmp.Split('\n');
                        //strTmp = "";
                        strCompile2 = "";
                        if (bOut_Message == true) m_strCompilePath += "-for:Compile_Org2Basic";
                        foreach (String strItem in pstrTmp) { strCompile2 += Compile_Org2Basic(strItem.ToLower()) + "\r\n"; }
                        // DEBUG: Org2Basic 결과 출력
                        if (m_nErrorCode != 0)
                        {
                            if (bOut_Message == true)
                            {
                                strMessage = m_pstrError[m_nErrorCode] + m_strError_Etc;
                                m_strCompilePath += "(Error" + CConvert.IntToStr(m_nErrorCode) + ")";
                            }
                            return m_nErrorCode;
                        }
                        #endregion compile - 2st step(strCompile2)(Kor: 컴파일 - 2단계(strCompile2))

                        #region compile - 3st step(strCompile3)(Kor: 컴파일 - 3단계(strCompile3))
                        if (bOut_Message == true) m_strCompilePath += "-3st step";// "-3단계(Compile_Basic2Asem)";
#if false
                        strTmp = Compile_Basic2Asem(strCompile2, false);
                        strCompile3 = strTmp; // -> Result
                        strCompile3_Debug = Compile_Basic2Asem(strCompile2, true); // Test
#else
                        strCompile3 = Compile_Basic2Asem(strCompile2, false);
                        if (bOut3_Debug == true)
                        {
                            strCompile3_Debug = Compile_Basic2Asem(strCompile2, true); // Test
                        }
                        else strCompile3_Debug = "";
#endif
                        if (m_nErrorCode != 0)
                        {
                            if (bOut_Message == true)
                            {
                                strMessage = m_pstrError[m_nErrorCode] + m_strError_Etc;
                                m_strCompilePath += "(Error" + CConvert.IntToStr(m_nErrorCode) + ")";
                            }
                            return m_nErrorCode;
                        }
                        #endregion compile - 3st step(strCompile3)(Kor: 컴파일 - 3단계(strCompile3))

                        #region compile - 4st step(strCompile4)(Kor: 컴파일 - 4단계(strCompile4))
                        if (bOut_Message == true) m_strCompilePath += "-4st step";// "-4단계(Compile_Asem2Code)";

#if false
                        strTmp = Compile_Asem2Code(strCompile3);
                        strCompile4 = strTmp; // -> Result
#else
                        strCompile4 = Compile_Asem2Code(strCompile3);
#endif
                        if (m_nErrorCode != 0)
                        {
                            if (bOut_Message == true)
                            {
                                strMessage = m_pstrError[m_nErrorCode] + m_strError_Etc;
                                m_strCompilePath += "(Error" + CConvert.IntToStr(m_nErrorCode) + ")";
                            }
                            return m_nErrorCode;
                        }
                        #endregion compile - 4st step(strCompile4)(Kor: 컴파일 - 4단계(strCompile4))

                        #region compile - Code Generation
                        if (bOut_Message == true) m_strCompilePath += "compile - Code Generation(Compile_CodeString2Code)";
                        Compile_CodeString2Code(strCompile4, out SCode);
                        if (m_nErrorCode != 0)
                        {
                            if (bOut_Message == true)
                            {
                                strMessage = m_pstrError[m_nErrorCode] + m_strError_Etc;
                                m_strCompilePath += "(Error" + CConvert.IntToStr(m_nErrorCode) + ")";
                            }
                            return m_nErrorCode;
                        }
                        #endregion compile - Code Generation

                        if (bOut_Message == true) m_strCompilePath += "]";

                        SCode.bInit = true;

                        return m_nErrorCode;
                    }
                    catch (System.Exception e)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += "[Compile]" + e.ToString();
                        if (bOut_Message == true)
                        {
                            strMessage = m_pstrError[m_nErrorCode] + m_strError_Etc;
                            m_strCompilePath += "]";
                        }
                        return m_nErrorCode;
                    }

                }
                #endregion previous compile(Kor: 컴파일 전단계)

                //public const int _MAX_VAR = _CNT_VAR_V;
                //public const int _MAX_MOTOR = _CNT_MOTOR;
                private static double m_dX;
                private static double m_dY;
                private static double m_dZ;
                private static double[] m_adV = new double[_CNT_VAR_V];
                private static double[] m_adMot = new double[_CNT_MOTOR];

                // CallFunction에서 사용할 pSOjwCode 배열 참조
                private static SOjwCode_t[] m_pSOjwCode = null;

                // CallFunction에서 메모리 동기화를 위한 현재 실행 중인 SCode 참조
                private static SOjwCode_t m_CurrentSCode;
                private static bool m_bHasCurrentSCode = false;

                // IK 함수 호출을 위한 C3d 객체 참조
                private static C3d m_pC3d = null;
                public static void SetC3d(C3d c3d) { m_pC3d = c3d; }
                public static C3d GetC3d() { return m_pC3d; }

                public static void SetValue_ClearAll(ref SOjwCode_t SCode)
                {
                    //SCode.adOperation_Memory.Initialize(); // 0x00~0x0ff(Motor), 0x100~0x102(x,y,z), 0x103~0x1ff(V변수(혹은 _K변수)
                    m_dX = m_dY = m_dZ = 0.0f; //m_afV.Initialize(); m_afMot.Initialize();
                    Array.Clear(m_adV, 0, m_adV.Length);
                    Array.Clear(m_adMot, 0, m_adMot.Length);
                }
                public static void SetValue_X(double dX) { m_dX = dX; }
                public static void SetValue_Y(double dY) { m_dY = dY; }
                public static void SetValue_Z(double dZ) { m_dZ = dZ; }
                public static void SetValue_V(int nIndex, double dValue) { if (nIndex < _CNT_VAR_V) { m_adV[nIndex] = dValue; } }
                public static void SetValue_Motor(int nIndex, double dValue) { if (nIndex < _CNT_MOTOR) { m_adMot[nIndex] = dValue; } }

                public static void SetValue_V(double[] adVar) { if (adVar.Length <= _CNT_VAR_V) { Array.Copy(adVar, 0, m_adV, 0, adVar.Length); } }
                public static void SetValue_Motor(double[] adMotor) { if (adMotor.Length <= _CNT_MOTOR) { Array.Copy(adMotor, 0, m_adMot, 0, adMotor.Length); } }
                public static void SetValue_V(float[] afVar) { SetValue_V(CConvert.FloatsToDoubles(afVar)); }
                public static void SetValue_Motor(float[] afMotor) { SetValue_Motor(CConvert.FloatsToDoubles(afMotor)); }

                // CallFunction에서 다른 수식을 호출할 수 있도록 pSOjwCode 배열 참조 설정
                public static void SetCodeArray(SOjwCode_t[] pSOjwCode) { m_pSOjwCode = pSOjwCode; }
                // C3d와 함께 설정 (inv 함수에서 C3d IK 알고리즘 사용을 위해)
                public static void SetCodeArray(SOjwCode_t[] pSOjwCode, C3d c3d) { m_pSOjwCode = pSOjwCode; m_pC3d = c3d; }

                public static double GetValue_X() { return m_dX; }
                public static double GetValue_Y() { return m_dY; }
                public static double GetValue_Z() { return m_dZ; }
                public static double GetValue_V(int nIndex) { if (nIndex >= _CNT_VAR_V) return 0; return m_adV[nIndex]; }
                public static double GetValue_Motor(int nIndex) { if (nIndex >= _CNT_MOTOR) return 0; return m_adMot[nIndex]; }

                public static double[] GetValue_V() { return m_adV; }
                public static double[] GetValue_Motor() { return m_adMot; }

                //public static bool CalcCode_Python(string strPython, ref SOjwCode_t SCode)
                //{
                //    if (SCode.bInit == false)
                //    {
                //        m_nErrorCode = 9;
                //        m_strError_Etc += "[CalcCode]" + "Variable[SOjwCode_t] is not initialized (the first compilation required)";// "SOjwCode_t 변수가 초기화(최초컴파일 필요)되지 않았습니다.";
                //        return false;
                //    }

                //    if (SCode.bPython == false)
                //    {                        
                //        m_nErrorCode = 9;
                //        m_strError_Etc += "[CalcCode]" + "the variable(bPython) is no checked";// "Python code 로 정의 되지 않았습니다."
                //        return false;                       
                //    }
                //    if (CPython.CalcCode(strPython, ref SCode, ref m_dX, ref m_dY, ref m_dZ, ref m_adV, ref m_adMot) == false)
                //    {
                //        m_nErrorCode = 9;
                //        m_strError_Etc += "[CalcCode-Python] Check your Python code";
                //        return false;
                //    }

                //    return true;
                //}
                public static bool CalcCode(ref SOjwCode_t SCode)
                {
                    if (SCode.bInit == false)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += "[CalcCode]" + "Variable[SOjwCode_t] is not initialized (the first compilation required)";// "SOjwCode_t 변수가 초기화(최초컴파일 필요)되지 않았습니다.";
                        return false;
                    }
                    
#if _USING_DOTNET_3_5 || _USING_DOTNET_2_0
#else
                    if (SCode.bPython == true)
                    {
                        m_strError_Etc = String.Empty;
                        //SCode.adOperation_Data = new double[1];
                        //SCode.adOperation_Memory = new double[1];
                        //SCode.alOperation_Cmd = new long[1];
                        //double dX = m_dX;
                        //double dY = m_dY;
                        //double dZ = m_dZ;
                        //double[] adV = new double[CPython._CNT_VAR_V];
                        //Array.Copy(m_adV, adV, CPython._CNT_VAR_V);
                        //double[] adT = new double[CPython._CNT_MOTOR];
                        //Array.Copy(m_adMot, adT, CPython._CNT_MOTOR);
                        //SOjwCode_t SCode2 = new SOjwCode_t();
                        //SCode2.bPython = SCode.bPython;
                        //SCode2.strPython = SCode.strPython;
                        //CPy Cp = new CPy();
                        String strErrorMsg = String.Empty;
                        if (CPython.CalcCode(ref SCode, ref m_dX, ref m_dY, ref m_dZ, ref m_adV, ref m_adMot, ref strErrorMsg
                                                //SCode.strPython//, //ref m_dX, ref m_dY, ref m_dZ,
                                                //SCode.nVar_Max, SCode.pnVar_Number, //ref adV, 
                                                //SCode.nMotor_Max, SCode.pnMotor_Number//, ref adT
                                                ) == false
                            )
                        {
                            m_nErrorCode = 9;
                            m_strError_Etc += String.Format("[CalcCode-Python] Check your Python code - {0}", strErrorMsg);
                            return false;
                        }
                        //Array.Copy(adV, m_adV, CPython._CNT_VAR_V);
                        //Array.Copy(adT, m_adMot, CPython._CNT_MOTOR);

                        
                    }
                    else
#endif
                    {
                        try
                        {
                            Array.Copy(m_adMot, 0, SCode.adOperation_Memory, _ADDRESS_MOTOR, _CNT_MOTOR);
                            SCode.adOperation_Memory[_ADDRESS_X] = GetValue_X();
                            SCode.adOperation_Memory[_ADDRESS_Y] = GetValue_Y();
                            SCode.adOperation_Memory[_ADDRESS_Z] = GetValue_Z();
                            Array.Copy(m_adV, 0, SCode.adOperation_Memory, _ADDRESS_V, _CNT_VAR_V);

                            // CallFunction에서 메모리 동기화를 위해 현재 SCode 저장
                            m_CurrentSCode = SCode;
                            m_bHasCurrentSCode = true;

                            int nSize = SCode.nCnt_Operation;
                            int nAddress = -1;
                            int i = 0;

                            while (i < nSize)
                            {
                                long lOperation_Cmd = SCode.alOperation_Cmd[i];

                                // ----------------------------
                                // 블록 제어 명령어 처리 (if/else/endif)
                                // ----------------------------
                                long lBlockCmd = (lOperation_Cmd & 0x0000ff);

                                // IF_START (0x21): 조건이 거짓이면 ELSE 또는 ENDIF로 점프
                                if (lBlockCmd == 0x21)
                                {
                                    // 조건값은 adOperation_Data[i]에 저장된 주소의 메모리 값
                                    int nCondAddr = (int)SCode.adOperation_Data[i];
                                    double dCondition = SCode.adOperation_Memory[nCondAddr];

                                    if (dCondition == 0.0) // 조건이 거짓
                                    {
                                        // ELSE(0x22) 또는 ENDIF(0x23)를 찾아서 점프
                                        int nDepth = 1;
                                        int j = i + 1;
                                        while (j < nSize && nDepth > 0)
                                        {
                                            long lSearchCmd = (SCode.alOperation_Cmd[j] & 0x0000ff);
                                            if (lSearchCmd == 0x21) nDepth++; // 중첩 if
                                            else if (lSearchCmd == 0x22 && nDepth == 1) break; // else 발견
                                            else if (lSearchCmd == 0x23) nDepth--; // endif
                                            j++;
                                        }
                                        i = j; // else 또는 endif 위치로 점프
                                        continue;
                                    }
                                    i++;
                                    continue;
                                }
                                // ELSE (0x22): IF 블록 실행 후 ENDIF로 점프
                                else if (lBlockCmd == 0x22)
                                {
                                    // if 블록이 실행되었으면 else 블록 건너뛰기
                                    int nDepth = 1;
                                    int j = i + 1;
                                    while (j < nSize && nDepth > 0)
                                    {
                                        long lSearchCmd = (SCode.alOperation_Cmd[j] & 0x0000ff);
                                        if (lSearchCmd == 0x21) nDepth++; // 중첩 if
                                        else if (lSearchCmd == 0x23) nDepth--; // endif
                                        j++;
                                    }
                                    i = j; // endif 다음으로 점프
                                    continue;
                                }
                                // ENDIF (0x23): 블록 끝, 아무것도 안함
                                else if (lBlockCmd == 0x23)
                                {
                                    i++;
                                    continue;
                                }

                                // ----------------------------
                                // 기존 명령어 처리
                                // ----------------------------
                                // If the [Var] address values are written to the data(Kor: Var 인 경우 해당 데이타엔 번지값이 기록)
                                if ((lOperation_Cmd & 0x0000ff) == 1) // 번지 측정은 0xff 에서만 하기에 ...
                                {
                                    // get the address(Kor: 번지값을 가져온다.)
                                    nAddress = (int)(SCode.adOperation_Data[i]);
                                    i++;
                                    continue;
                                }
                                if (nAddress < 0)
                                {
                                    m_nErrorCode = 9;
                                    m_strError_Etc += "[CalcCode]" + "Import address fails";// "번지값 가져오기 실패";
                                    return false;//Error
                                }

                                double dData;
                                //long lAddrCheck = lOperation_Cmd & 0xffff0000;
                                long lAddrCheck = lOperation_Cmd & 0x7fff0000;

                                lOperation_Cmd &= 0x0000ffff;
                                if (lAddrCheck != 0)
                                {
                                    int nTmp = (int)SCode.adOperation_Data[i];
                                    dData = SCode.adOperation_Memory[nTmp];
                                }
                                else dData = SCode.adOperation_Data[i];

                                // First computation(Kor: 1차연산)
                                long lCmd = ((lOperation_Cmd >> 8) & 0x00ff);
                                if (lCmd > 0)
                                {
                                    // call 명령어는 CalcCode 내에서 직접 처리 (메모리 동기화 필요)
                                    if (lCmd == 0x15)
                                    {
                                        // call의 인자(함수 번호)는 원래 adOperation_Data[i] 값을 사용
                                        int nCallNum = (int)SCode.adOperation_Data[i];
                                        // 호출 전: 현재 SCode 메모리를 static 변수로 동기화
                                        m_dX = SCode.adOperation_Memory[_ADDRESS_X];
                                        m_dY = SCode.adOperation_Memory[_ADDRESS_Y];
                                        m_dZ = SCode.adOperation_Memory[_ADDRESS_Z];
                                        Array.Copy(SCode.adOperation_Memory, _ADDRESS_MOTOR, m_adMot, 0, _CNT_MOTOR);
                                        Array.Copy(SCode.adOperation_Memory, _ADDRESS_V, m_adV, 0, _CNT_VAR_V);

                                        // 호출할 함수 실행
                                        if (m_pSOjwCode != null && nCallNum >= 0 && nCallNum < m_pSOjwCode.Length && m_pSOjwCode[nCallNum].bInit)
                                        {
                                            CalcCode(ref m_pSOjwCode[nCallNum]);
                                        }

                                        // 호출 후: 결과를 현재 SCode 메모리로 복사
                                        SCode.adOperation_Memory[_ADDRESS_X] = m_dX;
                                        SCode.adOperation_Memory[_ADDRESS_Y] = m_dY;
                                        SCode.adOperation_Memory[_ADDRESS_Z] = m_dZ;
                                        Array.Copy(m_adMot, 0, SCode.adOperation_Memory, _ADDRESS_MOTOR, _CNT_MOTOR);
                                        Array.Copy(m_adV, 0, SCode.adOperation_Memory, _ADDRESS_V, _CNT_VAR_V);
                                    }
                                    // inv 명령어 처리 (1차연산): inv(함수선택번호, 수식번호)
                                    else if (lCmd == 0x20)
                                    {
                                        // inv(funcType, funcNum) 형태
                                        // dData에는 첫 번째 인자(funcType)가 들어있음
                                        // 두 번째 인자(funcNum)는 2차연산에서 처리됨
                                        // 일단 funcType을 목적지 주소에 저장
                                        int nDestAddr = (int)SCode.adOperation_Data[i];
                                        SCode.adOperation_Memory[nDestAddr] = dData;
                                    }
                                    else
                                    {
                                        CalcCmd(lCmd, dData, ref SCode.adOperation_Memory[(int)SCode.adOperation_Data[i]]);
                                    }
                                    dData = SCode.adOperation_Memory[(int)SCode.adOperation_Data[i]];
                                }

                                // Second computation(Kor: 2차연산)
                                lCmd = (lOperation_Cmd & 0x00ff);

                                // call 명령어 처리
                                if (lCmd == 0x15)
                                {
                                    // call의 인자(함수 번호)는 dData에서 가져옴 (이미 메모리에서 읽어온 값)
                                    // adOperation_Data[i]에는 메모리 주소가 있고, 실제 함수 번호는 해당 메모리에 저장됨
                                    int nCallNum = (int)dData;
                                    double dCallResult = 0; // 실패 시 0

                                    // 호출 전: 현재 SCode 메모리를 static 변수로 동기화
                                    m_dX = SCode.adOperation_Memory[_ADDRESS_X];
                                    m_dY = SCode.adOperation_Memory[_ADDRESS_Y];
                                    m_dZ = SCode.adOperation_Memory[_ADDRESS_Z];
                                    Array.Copy(SCode.adOperation_Memory, _ADDRESS_MOTOR, m_adMot, 0, _CNT_MOTOR);
                                    Array.Copy(SCode.adOperation_Memory, _ADDRESS_V, m_adV, 0, _CNT_VAR_V);

                                    // 호출할 함수 실행
                                    if (m_pSOjwCode != null && nCallNum >= 0 && nCallNum < m_pSOjwCode.Length && m_pSOjwCode[nCallNum].bInit)
                                    {
                                        CalcCode(ref m_pSOjwCode[nCallNum]);
                                        dCallResult = 1; // 성공 시 1
                                    }

                                    // 호출 후: 결과를 현재 SCode 메모리로 복사
                                    SCode.adOperation_Memory[_ADDRESS_X] = m_dX;
                                    SCode.adOperation_Memory[_ADDRESS_Y] = m_dY;
                                    SCode.adOperation_Memory[_ADDRESS_Z] = m_dZ;
                                    Array.Copy(m_adMot, 0, SCode.adOperation_Memory, _ADDRESS_MOTOR, _CNT_MOTOR);
                                    Array.Copy(m_adV, 0, SCode.adOperation_Memory, _ADDRESS_V, _CNT_VAR_V);

                                    // call 결과값을 목적지 주소에 저장 (성공=1, 실패=0)
                                    SCode.adOperation_Memory[nAddress] = dCallResult;
                                }
                                // inv 명령어 처리: inv(함수선택번호, 수식번호)
                                else if (lCmd == 0x20)
                                {
                                    // inv(funcType, funcNum) 형태
                                    // dData에는 수식번호(funcNum)가 들어있음
                                    // 함수선택번호(funcType)는 이전에 처리된 값이므로 별도로 가져와야 함
                                    // INV 명령은 2인자 함수이므로 첫 번째 인자(funcType)가 먼저 처리되고
                                    // 두 번째 인자(funcNum)가 dData에 있음
                                    int nFuncNum = (int)dData;
                                    // 함수선택번호는 이전 명령에서 설정된 값을 사용
                                    // INV 어셈블리 형식: INV,_Mx (첫번째 인자가 _Mx에 저장됨)
                                    // 두 번째 인자는 dData
                                    int nFuncType = (int)SCode.adOperation_Memory[nAddress]; // 첫 번째 인자 (함수선택번호)

                                    double dInvResult = 0; // 실패 시 0

                                    // 호출 전: 현재 SCode 메모리를 static 변수로 동기화
                                    m_dX = SCode.adOperation_Memory[_ADDRESS_X];
                                    m_dY = SCode.adOperation_Memory[_ADDRESS_Y];
                                    m_dZ = SCode.adOperation_Memory[_ADDRESS_Z];
                                    Array.Copy(SCode.adOperation_Memory, _ADDRESS_MOTOR, m_adMot, 0, _CNT_MOTOR);
                                    Array.Copy(SCode.adOperation_Memory, _ADDRESS_V, m_adV, 0, _CNT_VAR_V);

                                    // IK 함수 호출
                                    if (CallInverse(nFuncType, nFuncNum))
                                    {
                                        dInvResult = 1; // 성공 시 1
                                    }

                                    // 호출 후: 결과를 현재 SCode 메모리로 복사
                                    SCode.adOperation_Memory[_ADDRESS_X] = m_dX;
                                    SCode.adOperation_Memory[_ADDRESS_Y] = m_dY;
                                    SCode.adOperation_Memory[_ADDRESS_Z] = m_dZ;
                                    Array.Copy(m_adMot, 0, SCode.adOperation_Memory, _ADDRESS_MOTOR, _CNT_MOTOR);
                                    Array.Copy(m_adV, 0, SCode.adOperation_Memory, _ADDRESS_V, _CNT_VAR_V);

                                    // inv 결과값을 목적지 주소에 저장 (성공=1, 실패=0)
                                    SCode.adOperation_Memory[nAddress] = dInvResult;
                                }
                                else
                                {
                                    CalcCmd(lCmd, dData, ref SCode.adOperation_Memory[nAddress]);
                                }
                                //lCmd = 0;

                                i++;
                            }
                            //Array.Copy(SCode.adOperation_Memory, m_afMot, _CNT_MOTOR);
                            m_dX = SCode.adOperation_Memory[_ADDRESS_X];
                            m_dY = SCode.adOperation_Memory[_ADDRESS_Y];
                            m_dZ = SCode.adOperation_Memory[_ADDRESS_Z];
                            Array.Copy(SCode.adOperation_Memory, _ADDRESS_MOTOR, m_adMot, 0, _CNT_MOTOR);
                            Array.Copy(SCode.adOperation_Memory, _ADDRESS_V, m_adV, 0, _CNT_VAR_V);


                            Array.Copy(SCode.adOperation_Memory, _ADDRESS_MOTOR, m_adMot, 0, _CNT_MOTOR);
                            SetValue_X(SCode.adOperation_Memory[_ADDRESS_X]);
                            SetValue_Y(SCode.adOperation_Memory[_ADDRESS_Y]);
                            SetValue_Z(SCode.adOperation_Memory[_ADDRESS_Z]);
                            Array.Copy(SCode.adOperation_Memory, _ADDRESS_V, m_adV, 0, _CNT_VAR_V);

                            // CalcCode 종료 시 현재 SCode 플래그 해제
                            m_bHasCurrentSCode = false;
                        }
                        catch (System.Exception e)
                        {
                            m_bHasCurrentSCode = false;
                            m_nErrorCode = 9;
                            m_strError_Etc += "[CalcCode]" + e.ToString();
                            return false;
                        }
                    }
                    return true;
                }

                // 결과값은 m_C3d.GetData(ID) 로 가져오면 됨
                public static void CalcAuto(ref C3d Ojw3d, int nFunctionNumber, int [] anMotorIDs, float fX, float fY, float fZ, int nRepeat = 1000, float fCutline = 0.0001f)
                {
                    int nID;
                    int nCnt = anMotorIDs.Length;
                    int nPos = nCnt;
                    float fXc, fYc, fZc;
                    float fXp, fYp, fZp;
                    float fAngle0 = 0;
                    float fP, fN, fC;
                    float fAngle, fAngle1, fAngle2;
                    List<float> lstAngles = new List<float>();
                    for (int i = 0; i < nCnt; i++)
                        lstAngles.Add(Ojw3d.GetData(anMotorIDs[i]));
                    float fRes = 0.0f;
                    for (int nCalc = 0; nCalc < nRepeat; nCalc++)
                    {
                        fRes = 1.0f;
                        nPos = nCnt;
                        while (nPos - 1 >= 0)
                        {
                            nID = anMotorIDs[nPos - 1];
                            fAngle0 = Ojw3d.GetData(nID);

                            CForward.CalcF(ref Ojw3d, nFunctionNumber, out fXc, out fYc, out fZc, -1, false);
                            CForward.CalcF(ref Ojw3d, nFunctionNumber, out fXp, out fYp, out fZp, nID, true);

                            fP = (float)Math.Sqrt(
                                Math.Pow(fX - fXc, 2) +
                                Math.Pow(fY - fYc, 2) +
                                Math.Pow(fZ - fZc, 2)
                                );
                            fN = (float)Math.Sqrt(
                                (float)Math.Pow(fXc - fXp, 2) +
                                (float)Math.Pow(fYc - fYp, 2) +
                                (float)Math.Pow(fZc - fZp, 2)
                                );
                            fC = (float)Math.Sqrt(
                                (float)Math.Pow(fX - fXp, 2) +
                                (float)Math.Pow(fY - fYp, 2) +
                                (float)Math.Pow(fZ - fZp, 2)
                                );
                            //float fAngle = 180.0f - (float)Ojw.CMath.ACos((fP * fP + fN * fN - fC * fC) / (2.0f * fP * fN)) - fAngle0;

                            fAngle1 = fAngle0 + (float)Ojw.CMath.ACos((fC * fC + fN * fN - fP * fP) / (2.0f * fC * fN));
                            fAngle2 = fAngle0 - ((float)Ojw.CMath.ACos((fC * fC + fN * fN - fP * fP) / (2.0f * fC * fN)));
                            if (Single.IsNaN(fAngle1)) fAngle1 = fAngle0;
                            if (Single.IsNaN(fAngle2)) fAngle2 = fAngle0;

                            fAngle1 = Ojw3d.CalcLimit(nID, fAngle1);
                            fAngle2 = Ojw3d.CalcLimit(nID, fAngle2);

                            // Distance 계산
                            Ojw3d.SetData(nID, fAngle1);
                            CForward.CalcF(ref Ojw3d, nFunctionNumber, out fXc, out fYc, out fZc, -1);
                            fC = (float)Math.Sqrt(
                                (float)Math.Pow(fX - fXc, 2) +
                                (float)Math.Pow(fY - fYc, 2) +
                                (float)Math.Pow(fZ - fZc, 2)
                                );

                            Ojw3d.SetData(nID, fAngle2);
                            CForward.CalcF(ref Ojw3d, nFunctionNumber, out fXp, out fYp, out fZp, -1);
                            fP = (float)Math.Sqrt(
                                (float)Math.Pow(fX - fXp, 2) +
                                (float)Math.Pow(fY - fYp, 2) +
                                (float)Math.Pow(fZ - fZp, 2)
                                );
                            fAngle = (fC < fP) ? fAngle1 : fAngle2;

                            Ojw3d.SetData(nID, fAngle);

                            fRes = 0;
                            for (int i = 0; i < nCnt; i++)
                            {
                                fRes += (float)Math.Abs(Ojw3d.GetData(anMotorIDs[i]) - lstAngles[i]);
                                lstAngles[i] = Ojw3d.GetData(anMotorIDs[i]);
                            }
                            nPos--;
                        }

                        if (fRes < fCutline) break;
                    }
                }

                public static bool CalcCode(ref SOjwCode_t SCode, ref double dX, ref double dY, ref double dZ, ref double[] adV, ref double[] adMot)
                {
                    bool bRet = false;
                    if (SCode.bInit == false)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += "[CalcCode]" + "Variable[SOjwCode_t] is not initialized (the first compilation required)";// "SOjwCode_t 변수가 초기화(최초컴파일 필요)되지 않았습니다.";
                        return false;
                    }
                    try
                    {
                        SetValue_X(dX);
                        SetValue_Y(dY);
                        SetValue_Z(dZ);
                        int nCnt_Mot = ((adMot.Length > _CNT_MOTOR) ? _CNT_MOTOR : adMot.Length);
                        Array.Copy(adMot, 0, SCode.adOperation_Memory, _ADDRESS_MOTOR, nCnt_Mot);
                        int nCnt_Var = ((adV.Length > _CNT_VAR_V) ? _CNT_VAR_V : adV.Length);
                        Array.Copy(adV, 0, SCode.adOperation_Memory, _ADDRESS_V, nCnt_Var);

                        bRet = CalcCode(ref SCode);

                        //Array.Copy(SCode.adOperation_Memory, _ADDRESS_V, afV, 0, nCnt_Var);
                        //Array.Copy(SCode.adOperation_Memory, _ADDRESS_MOTOR, afMot, 0, nCnt_Mot);

                        dX = SCode.adOperation_Memory[_ADDRESS_X];
                        dY = SCode.adOperation_Memory[_ADDRESS_Y];
                        dZ = SCode.adOperation_Memory[_ADDRESS_Z];
                        Array.Copy(SCode.adOperation_Memory, _ADDRESS_MOTOR, adMot, 0, _CNT_MOTOR);
                        Array.Copy(SCode.adOperation_Memory, _ADDRESS_V, adV, 0, _CNT_VAR_V);
                        //                 fX = GetValue_X();
                        //                 fY = GetValue_Y();
                        //                 fZ = GetValue_Z();
                        //                 afMot = GetValue_Motor();
                        //                 afV = GetValue_V();
                    }
                    catch (System.Exception e)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += "[CalcCode]" + e.ToString();
                        return false;
                    }
                    return bRet;
                }

                // ------------------------------------------------------------
                // CalcCode - pSOjwCode 배열을 받아서 call(N) 기능을 지원하는 버전
                // nNum: 실행할 수식 번호
                // pSOjwCode: 전체 수식 배열 (다른 함수 호출용)
                //
                // 사용법:
                //   CalcCode(ref pSOjwCode, 1);  // 1번 수식 실행 (내부에서 call(0) 등 가능)
                // ------------------------------------------------------------
                public static bool CalcCode(ref SOjwCode_t[] pSOjwCode, int nNum)
                {
                    if (nNum < 0 || nNum >= pSOjwCode.Length)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += String.Format("[CalcCode] nNum={0} is out of range (0~{1})", nNum, pSOjwCode.Length - 1);
                        return false;
                    }

                    // CallFunction에서 다른 함수를 호출할 수 있도록 배열 참조 설정
                    SetCodeArray(pSOjwCode);

                    return CalcCode(ref pSOjwCode[nNum]);
                }

                private static void CalcCmd(long lCmd, double dData, ref double dValue)
                {
                    try
                    {
                        // second computation(Kor: 2차연산)
                        // Cmd, Data, Ret            
                        if (lCmd == 2) // Add
                        {
                            dValue += dData;
                        }
                        else if (lCmd == 3) // Sub
                        {
                            dValue -= dData;
                        }
                        else if (lCmd == 4) // Mul
                        {
                            dValue *= dData;
                        }
                        else if (lCmd == 5) // Div
                        {
                            dValue /= ((dData == 0) ? (double)CMath.Zero() : dData);
                        }
                        else if (lCmd == 0x13) // Mod
                        {
                            dValue %= dData;
                        }
                        else if (lCmd == 6) // sin
                        {
                            dValue = (double)CMath.Sin(dValue);
                        }
                        else if (lCmd == 7) // cos
                        {
                            dValue = (double)CMath.Cos(dValue);
                        }
                        else if (lCmd == 8) // tan
                        {
                            dValue = (double)CMath.Tan(dValue);
                        }
                        else if (lCmd == 9) // asin
                        {
                            // NAN 방지
                            if (dValue > 1) dValue = 1.0f;
                            else if (dValue < -1) dValue = -1.0f;

                            dValue = (double)CMath.ASin(dValue);
                        }
                        else if (lCmd == 0x0a) // acos
                        {
                            // NAN 방지
                            if (dValue > 1) dValue = 1.0f;
                            else if (dValue < -1) dValue = -1.0f;

                            dValue = (double)CMath.ACos(dValue);
                        }
                        else if (lCmd == 0x0b) // atan
                        {
                            dValue = (double)CMath.ATan(dValue);
                        }
                        else if (lCmd == 0x0c) // sqrt
                        {
                            if (dData == 2)
                            {
                                dValue = (double)Math.Sqrt(dValue);
                                if (Double.IsNaN(dValue) == true) dValue = 0;
                            }
                            else dValue = (double)Math.Pow(dValue, 1.0f / ((dData == 0) ? (double)CMath.Zero() : dData));
                        }
                        else if (lCmd == 0x0d) // pow
                        {
                            dValue = (double)Math.Pow(dValue, dData);
                        }
                        else if (lCmd == 0x0e) // clear
                        {
                            dValue = 0.0f;
                        }
                        else if (lCmd == 0x0f) // abs
                        {
                            dValue = (double)Math.Abs(dValue);
                        }
                        else if (lCmd == 0x10) // atan2
                        {
                            //fValue = (double)Math.Atan2(fValue, fData);
                            // atan2(y, x)
                            //fValue = (double)CMath.Ojw_aTan2(((fData == 0) ? (double)CMath.Zero() : fData), fValue);
                            dValue = (double)CMath.ATan2(dValue, ((dData == 0) ? (double)CMath.Zero() : dData));
                        }
                        else if (lCmd == 0x11) // acos2
                        {
                            int nPlane = (int)Math.Round(dData);

                            if (dValue > 1) dValue = 1.0f;
                            else if (dValue < -1) dValue = -1.0f;

                            // asin(Angle, Plane) // - Plane 0~3(1,2,3,4 quadrants) so, 0 - x+,y+, 1 - x-,y+, 3 - x-,y-, 4 - x+,y-,
                            // Kor: asin(각도, 평면) // - 평면 0~3(각각 1,2,3,4분면) 즉, 0 - x+,y+, 1 - x-,y+, 3 - x-,y-, 4 - x+,y-,
                            dValue = (double)CMath.ACos_Plane(nPlane, (double)dValue);
                        }
                        else if (lCmd == 0x12) // asin2
                        {
                            int nPlane = (int)Math.Round(dData);

                            if (dValue > 1) dValue = 1.0f;
                            else if (dValue < -1) dValue = -1.0f;

                            // asin(Angle, Plane)  // - Plane 0~3(1,2,3,4 quadrants) so, 0 - x+,y+, 1 - x-,y+, 3 - x-,y-, 4 - x+,y-,
                            // Kor: asin(각도, 평면) // - 평면 0~3(각각 1,2,3,4분면) 즉, 0 - x+,y+, 1 - x-,y+, 3 - x-,y-, 4 - x+,y-
                            dValue = (double)CMath.ASin_Plane(nPlane, (double)dValue);

                        }
                        else if (lCmd == 0x14) // round
                        {
                            dValue = (double)Math.Round(dValue, (int)dData);
                        }
                        else if (lCmd == 0x15) // call
                        {
                            // call(nNum) - nNum번 수식 호출
                            int nNum = (int)dData;
                            CallFunction(nNum);
                        }
                        else if (lCmd == 0x16) // if (조건문은 CalcCode에서 별도 처리)
                        {
                            // if 조건문은 CalcCode의 메인 루프에서 처리
                            // 여기서는 조건 결과만 dValue에 저장 (0 또는 1)
                        }
                        // 비교 연산자
                        else if (lCmd == 0x17) // < (less than)
                        {
                            dValue = (dValue < dData) ? 1.0 : 0.0;
                        }
                        else if (lCmd == 0x18) // > (greater than)
                        {
                            dValue = (dValue > dData) ? 1.0 : 0.0;
                        }
                        else if (lCmd == 0x19) // == (equal)
                        {
                            dValue = (Math.Abs(dValue - dData) < 1e-9) ? 1.0 : 0.0;
                        }
                        else if (lCmd == 0x1A) // != (not equal)
                        {
                            dValue = (Math.Abs(dValue - dData) >= 1e-9) ? 1.0 : 0.0;
                        }
                        else if (lCmd == 0x1B) // <= (less or equal)
                        {
                            dValue = (dValue <= dData) ? 1.0 : 0.0;
                        }
                        else if (lCmd == 0x1C) // >= (greater or equal)
                        {
                            dValue = (dValue >= dData) ? 1.0 : 0.0;
                        }
                        // 논리 연산자
                        else if (lCmd == 0x1D) // && (AND)
                        {
                            dValue = (dValue != 0.0 && dData != 0.0) ? 1.0 : 0.0;
                        }
                        else if (lCmd == 0x1E) // || (OR)
                        {
                            dValue = (dValue != 0.0 || dData != 0.0) ? 1.0 : 0.0;
                        }
                        else if (lCmd == 0x1F) // ! (NOT) - 단항 연산
                        {
                            dValue = (dValue == 0.0) ? 1.0 : 0.0;
                        }
                        // @Inv() IK 함수 호출은 CalcCode에서 처리됨 (2인자 함수이므로)
#if false
                    // Test code - 없어도 상관없는 코드
                    //            if (Single.IsNaN(fValue) == true)
                    //            {
                    //                fValue = 0;
                    //////                 fTmp_Value = 0;
                    //////                 fTmp_Data = 0;
                    //////                 fValue = fTmp_Value + fTmp_Data;
                    //           }
#endif
#if false // for test
                        if (dValue == 0)
                        {
                            MessageBox.Show("[Warning]Check CalcCode()");
                            Ojw.CMessage.Write("[Warning]Check CalcCode()");
                        }
#endif

                        #region Error Exception
#if false
                    if (double.IsNaN(dValue) == true)
                    {
                        dValue = 0;
                    }
                    if (double.IsInfinity(dValue) == true)
                    {
                        dValue = 0;
                    }
#endif
                        #endregion Error Exception
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }

                // ------------------------------------------------------------
                // CallFunction - call 명령어에서 호출되는 함수
                // nNum: 호출할 수식 번호 (pSOjwCode[nNum])
                //
                // 사용법:
                //   1. CalcCode 호출 전에 SetCodeArray(pSOjwCode)로 배열 참조 설정
                //   2. 수식 내에서 call(N) 으로 N번 수식 호출
                //
                // 예제:
                //   0번 함수: tup = v0; if (tup >= 360) { tup = tup - 360 }; t1 = tup
                //   1번 함수: v0 = atan2(y, x); dummy = call(0)
                //   -> 1번 함수에서 계산 결과를 v0에 넣고 0번 함수를 호출하여 정규화된 값을 t1에 저장
                // ------------------------------------------------------------
                private static bool CallFunction(int nNum)
                {
                    // pSOjwCode 배열이 설정되었는지 확인
                    if (m_pSOjwCode == null)
                    {
                        Ojw.CMessage.Write("[CallFunction] Error: pSOjwCode not set. Call SetCodeArray() first.");
                        return false;
                    }

                    // 범위 체크
                    if (nNum < 0 || nNum >= m_pSOjwCode.Length)
                    {
                        Ojw.CMessage.Write("[CallFunction] Error: nNum={0} is out of range (0~{1})", nNum, m_pSOjwCode.Length - 1);
                        return false;
                    }

                    // 초기화 체크
                    if (m_pSOjwCode[nNum].bInit == false)
                    {
                        Ojw.CMessage.Write("[CallFunction] Error: pSOjwCode[{0}] is not initialized", nNum);
                        return false;
                    }

                    // -----------------------------------------------------
                    // 메모리 동기화: 호출 전
                    // 현재 실행 중인 SCode의 메모리를 m_adV, m_adMot 등으로 복사
                    // 이렇게 해야 호출된 함수에서 현재 함수의 변수 값을 사용할 수 있음
                    // -----------------------------------------------------
                    SOjwCode_t callerSCode = new SOjwCode_t();
                    bool bHasCaller = m_bHasCurrentSCode;
                    if (bHasCaller)
                    {
                        callerSCode = m_CurrentSCode;
                        // 현재 SCode의 메모리를 static 변수로 동기화
                        m_dX = callerSCode.adOperation_Memory[_ADDRESS_X];
                        m_dY = callerSCode.adOperation_Memory[_ADDRESS_Y];
                        m_dZ = callerSCode.adOperation_Memory[_ADDRESS_Z];
                        Array.Copy(callerSCode.adOperation_Memory, _ADDRESS_MOTOR, m_adMot, 0, _CNT_MOTOR);
                        Array.Copy(callerSCode.adOperation_Memory, _ADDRESS_V, m_adV, 0, _CNT_VAR_V);
                    }

                    // 호출할 함수 실행
                    bool bResult = CalcCode(ref m_pSOjwCode[nNum]);

                    // -----------------------------------------------------
                    // 메모리 동기화: 호출 후
                    // 호출된 함수의 결과를 호출자의 SCode로 복사
                    // 이렇게 해야 호출자 함수에서 호출된 함수의 결과를 사용할 수 있음
                    // -----------------------------------------------------
                    if (bHasCaller)
                    {
                        // 결과를 호출자의 메모리로 복사
                        callerSCode.adOperation_Memory[_ADDRESS_X] = m_dX;
                        callerSCode.adOperation_Memory[_ADDRESS_Y] = m_dY;
                        callerSCode.adOperation_Memory[_ADDRESS_Z] = m_dZ;
                        Array.Copy(m_adMot, 0, callerSCode.adOperation_Memory, _ADDRESS_MOTOR, _CNT_MOTOR);
                        Array.Copy(m_adV, 0, callerSCode.adOperation_Memory, _ADDRESS_V, _CNT_VAR_V);

                        // 호출자의 SCode를 현재 SCode로 복원
                        m_CurrentSCode = callerSCode;
                        m_bHasCurrentSCode = true;
                    }

                    return bResult;
                }

                // ------------------------------------------------------------
                // CopyMotorValuesFromC3d - C3d의 모터 값을 m_adMot에 복사
                // IK 함수 실행 후 결과를 수식 변수(t0~)로 전달하기 위해 사용
                // ------------------------------------------------------------
                private static void CopyMotorValuesFromC3d(int nFuncNum)
                {
                    if (m_pC3d == null)
                    {
                        Ojw.CMessage.Write("[CopyMotorValuesFromC3d] m_pC3d is null");
                        return;
                    }

                    try
                    {
                        // 모든 모터 값(0~31)을 m_adMot에 복사
                        // C3d의 m_afMot 배열에서 직접 값을 가져옴
                        for (int i = 0; i < _CNT_MOTOR; i++)
                        {
                            m_adMot[i] = m_pC3d.GetData(i);
                        }
                        // 디버그: 첫 4개 모터 값 출력 (성능: 매 IK 호출마다 로그 → 주석 처리)
                        //Ojw.CMessage.Write("[CopyMotorValuesFromC3d] t0={0:F2}, t1={1:F2}, t2={2:F2}, t3={3:F2}",
                        //    m_adMot[0], m_adMot[1], m_adMot[2], m_adMot[3]);
                    }
                    catch (Exception ex)
                    {
                        Ojw.CMessage.Write("[CopyMotorValuesFromC3d] Exception: {0}", ex.Message);
                    }
                }

                // ------------------------------------------------------------
                // CallInverse - @Inv 명령어에서 호출되는 IK 함수
                // nType: IK 함수 타입 번호
                //   0: 현재 함수의 역기구학 코드 실행 (CalcCode 사용)
                //   1: DC-CCD (CalcInv, 코사인법칙 ±α 2후보각 + FK 검증)
                //   2: TRAC-IK (ROS) (DLS-RR + LM 듀얼 솔버)
                //   3: Probe CCD (구 "Paper CCD-IK" / Wang & Chen 변형, damping 0.5)
                //   4: WangChen CCD (Wang & Chen 1991 원본)
                //   5: Kenwright CCD (Kenwright 2012, comfort factor)
                //   6: Paper FABRIK (Aristidou & Lasenby 2011)
                //   7: Paper Jacobian DLS PosOnly (3xN)
                //   8: Paper Jacobian Euler (6xN, 위치+자세)
                //  11: SolveIK_JacobianDLS_PosOnly (기존 Jacobian DLS)
                //  12: SolveIK_JacobianDLS_PosWithTool (기존 Jacobian DLS + 툴)
                //  13: SolveIK_JacobianDLS_PosWithToolGlobal (기존 Jacobian DLS + 툴 글로벌)
                // nFuncNum: 수식 번호 (m_pSOjwCode 배열의 인덱스)
                // ------------------------------------------------------------
                private static bool CallInverse(int nType, int nFuncNum)
                {
                    // 유효성 검사
                    if (m_pSOjwCode == null || nFuncNum < 0 || nFuncNum >= m_pSOjwCode.Length)
                    {
                        return false;
                    }
                    if (!m_pSOjwCode[nFuncNum].bInit)
                    {
                        return false;
                    }

                    bool bResult = false;

                    switch (nType)
                    {
                        case 0:
                            // 지정된 수식의 역기구학 코드 실행
                            m_pSOjwCode[nFuncNum].adOperation_Memory[_ADDRESS_X] = m_dX;
                            m_pSOjwCode[nFuncNum].adOperation_Memory[_ADDRESS_Y] = m_dY;
                            m_pSOjwCode[nFuncNum].adOperation_Memory[_ADDRESS_Z] = m_dZ;
                            Array.Copy(m_adMot, 0, m_pSOjwCode[nFuncNum].adOperation_Memory, _ADDRESS_MOTOR, _CNT_MOTOR);
                            Array.Copy(m_adV, 0, m_pSOjwCode[nFuncNum].adOperation_Memory, _ADDRESS_V, _CNT_VAR_V);
                            CalcCode(ref m_pSOjwCode[nFuncNum]);
                            bResult = true;
                            break;

                        // ---- 주요 IK 알고리즘 (1~2: 권장) ----

                        case 1: // DC-CCD (코사인법칙 ±α 2후보각 + FK 검증)
                            // v0: nRepeat (기본값: 10000), v1: fCutline (기본값: 0.001)
                            if (m_pC3d != null)
                            {
                                int nRepeat = (m_adV[0] > 0) ? (int)m_adV[0] : 10000;
                                float fCut = (m_adV[1] > 0) ? (float)m_adV[1] : 0.001f;
                                float[] afRes = m_pC3d.CalcInv(nFuncNum, (float)m_dX, (float)m_dY, (float)m_dZ, nRepeat, fCut);
                                bResult = (afRes != null);
                                if (bResult) CopyMotorValuesFromC3d(nFuncNum);
                            }
                            break;

                        case 2: // TRAC-IK (ROS) - DLS-RR + LM 듀얼 솔버
                            // v0: maxIter (기본값: 1000), v1: tolMm (기본값: 0.1)
                            if (m_pC3d != null)
                            {
                                int maxIter = (m_adV[0] > 0) ? (int)m_adV[0] : 1000;
                                float tolMm = (m_adV[1] > 0) ? (float)m_adV[1] : 0.1f;
                                bResult = m_pC3d.TracIK(nFuncNum, (float)m_dX, (float)m_dY, (float)m_dZ, maxIter, tolMm);
                                if (bResult) CopyMotorValuesFromC3d(nFuncNum);
                            }
                            break;

                        // ---- 논문 기반 CCD 변형 (3~5) ----

                        case 3: // Probe CCD (구 "Paper CCD-IK" / Wang & Chen 변형, damping 0.5)
                            // v0: nRepeat (기본값: 10000), v1: fCutline (기본값: 0.001)
                            if (m_pC3d != null)
                            {
                                int nRepeat = (m_adV[0] > 0) ? (int)m_adV[0] : 10000;
                                float fCut = (m_adV[1] > 0) ? (float)m_adV[1] : 0.001f;
                                m_pC3d.PaperCCDIK(nFuncNum, (float)m_dX, (float)m_dY, (float)m_dZ, nRepeat, fCut);
                                bResult = true; CopyMotorValuesFromC3d(nFuncNum);
                            }
                            break;

                        case 4: // WangChen CCD (1991 원본, 풀 스텝)
                            // v0: nRepeat (기본값: 10000), v1: fCutline (기본값: 0.001)
                            if (m_pC3d != null)
                            {
                                int nRepeat = (m_adV[0] > 0) ? (int)m_adV[0] : 10000;
                                float fCut = (m_adV[1] > 0) ? (float)m_adV[1] : 0.001f;
                                m_pC3d.WangChenCCDIK(nFuncNum, (float)m_dX, (float)m_dY, (float)m_dZ, nRepeat, fCut);
                                bResult = true; CopyMotorValuesFromC3d(nFuncNum);
                            }
                            break;

                        case 5: // Kenwright CCD (2012, comfort factor)
                            // v0: nRepeat (기본값: 10000), v1: fCutline (기본값: 0.001)
                            if (m_pC3d != null)
                            {
                                int nRepeat = (m_adV[0] > 0) ? (int)m_adV[0] : 10000;
                                float fCut = (m_adV[1] > 0) ? (float)m_adV[1] : 0.001f;
                                m_pC3d.KenwrightCCDIK(nFuncNum, (float)m_dX, (float)m_dY, (float)m_dZ, nRepeat, fCut);
                                bResult = true; CopyMotorValuesFromC3d(nFuncNum);
                            }
                            break;

                        // ---- FABRIK / Jacobian (6~8) ----

                        case 6: // Paper FABRIK (Aristidou & Lasenby 2011)
                            // v0: nRepeat (기본값: 10000), v1: fCutline (기본값: 0.001)
                            if (m_pC3d != null)
                            {
                                int nRepeat = (m_adV[0] > 0) ? (int)m_adV[0] : 10000;
                                float fCut = (m_adV[1] > 0) ? (float)m_adV[1] : 0.001f;
                                m_pC3d.PaperFABRIK(nFuncNum, (float)m_dX, (float)m_dY, (float)m_dZ, nRepeat, fCut);
                                bResult = true; CopyMotorValuesFromC3d(nFuncNum);
                            }
                            break;

                        case 7: // Paper Jacobian DLS PosOnly (3xN)
                            // v0: maxIter (기본값: 100), v1: tolMm (기본값: 0.1)
                            if (m_pC3d != null)
                            {
                                int maxIter = (m_adV[0] > 0) ? (int)m_adV[0] : 100;
                                float tolMm = (m_adV[1] > 0) ? (float)m_adV[1] : 0.1f;
                                bResult = m_pC3d.PaperJacobianDLS(nFuncNum, (float)m_dX, (float)m_dY, (float)m_dZ, maxIter, tolMm);
                                if (bResult) CopyMotorValuesFromC3d(nFuncNum);
                            }
                            break;

                        case 8: // Paper Jacobian Euler (6xN, 위치+자세)
                            // v0: rxDeg, v1: ryDeg, v2: rzDeg
                            // v3: maxIter (기본값: 100), v4: tolMm (기본값: 0.1)
                            if (m_pC3d != null)
                            {
                                float rxDeg = (float)m_adV[0];
                                float ryDeg = (float)m_adV[1];
                                float rzDeg = (float)m_adV[2];
                                int maxIter = (m_adV[3] > 0) ? (int)m_adV[3] : 100;
                                float tolMm = (m_adV[4] > 0) ? (float)m_adV[4] : 0.1f;
                                bResult = m_pC3d.PaperJacobianEuler(nFuncNum, (float)m_dX, (float)m_dY, (float)m_dZ, rxDeg, ryDeg, rzDeg, maxIter, tolMm);
                                if (bResult) CopyMotorValuesFromC3d(nFuncNum);
                            }
                            break;

                        // ---- 기존 Jacobian (11~13, 하위 호환) ----

                        case 11: // SolveIK_JacobianDLS_PosOnly (기존 case 1)
                            if (m_pC3d != null)
                            {
                                int maxIter = (m_adV[0] > 0) ? (int)m_adV[0] : 100;
                                float tolMm = (m_adV[1] > 0) ? (float)m_adV[1] : 0.1f;
                                float epsDeg = (m_adV[2] > 0) ? (float)m_adV[2] : 0.01f;
                                float lambda = (m_adV[3] > 0) ? (float)m_adV[3] : 0.5f;
                                float gain = (m_adV[4] > 0) ? (float)m_adV[4] : 1.0f;
                                float maxStepDeg = (m_adV[5] > 0) ? (float)m_adV[5] : 5.0f;
                                bResult = SolveIK_JacobianDLS_PosOnly(ref m_pC3d, nFuncNum, (float)m_dX, (float)m_dY, (float)m_dZ, maxIter, tolMm, epsDeg, lambda, gain, maxStepDeg);
                                if (bResult) CopyMotorValuesFromC3d(nFuncNum);
                            }
                            break;

                        case 12: // SolveIK_JacobianDLS_PosWithTool (기존 case 2)
                            if (m_pC3d != null)
                            {
                                bResult = SolveIK_JacobianDLS_PosWithTool(ref m_pC3d, nFuncNum, (float)m_dX, (float)m_dY, (float)m_dZ,
                                    (float)m_adV[0], (float)m_adV[1], (float)m_adV[2], (float)m_adV[3],
                                    (m_adV[4] > 0) ? (int)m_adV[4] : 100, (m_adV[5] > 0) ? (float)m_adV[5] : 0.1f,
                                    (m_adV[6] > 0) ? (float)m_adV[6] : 0.01f, (m_adV[7] > 0) ? (float)m_adV[7] : 0.5f,
                                    (m_adV[8] > 0) ? (float)m_adV[8] : 1.0f, (m_adV[9] > 0) ? (float)m_adV[9] : 5.0f);
                                if (bResult) CopyMotorValuesFromC3d(nFuncNum);
                            }
                            break;

                        case 13: // SolveIK_JacobianDLS_PosWithToolGlobal (기존 case 3)
                            if (m_pC3d != null)
                            {
                                bResult = SolveIK_JacobianDLS_PosWithToolGlobal(ref m_pC3d, nFuncNum, (float)m_dX, (float)m_dY, (float)m_dZ,
                                    (float)m_adV[0], (float)m_adV[1], (float)m_adV[2], (float)m_adV[3],
                                    (m_adV[4] > 0) ? (int)m_adV[4] : 100, (m_adV[5] > 0) ? (float)m_adV[5] : 0.1f,
                                    (m_adV[6] > 0) ? (float)m_adV[6] : 0.01f, (m_adV[7] > 0) ? (float)m_adV[7] : 0.5f,
                                    (m_adV[8] > 0) ? (float)m_adV[8] : 1.0f, (m_adV[9] > 0) ? (float)m_adV[9] : 5.0f);
                                if (bResult) CopyMotorValuesFromC3d(nFuncNum);
                            }
                            break;

                        default:
                            bResult = false;
                            break;
                    }

                    return bResult;
                }

                #region Inverse Jacob
#if false
                // ------------------------------------------------------------
                // Jacobian(DLS) IK solver (position only)
                // dq = J^T (J J^T + λ^2 I)^-1 e
                // - angles: degree (Ojw3d.SetData/GetData)
                // - position: mm (CalcF outputs)
                // ------------------------------------------------------------
                public static bool SolveIK_JacobianDLS_PosOnly(
                    ref Ojw.C3d c3d,
                    int fn,
                    //int[] jointIds,
                    float tx, float ty, float tz,
                    int maxIter,
                    float tolMm,
                    float epsDeg,
                    float lambda,
                    float gain,
                    float maxStepDeg
                )
                {
                    c3d.SetLastIK_Iter_JacobianIK(0);
                    //if (jointIds == null || jointIds.Length <= 0) return false;
                    int n = c3d.m_CHeader.pDhParamAll[fn].GetMotors_Count();
                    //int n = c3d.GetHeader_pSOjwCode()[fn].nMotor_Max;//  c3d.GetHeader_pSOjwCode()[fn].pnMotor_Number[i];// //jointIds.Length;

                    float[] q0 = new float[n];
                    float[,] J = new float[3, n];

                    for (int iter = 0; iter < maxIter; iter++)
                    {
                        c3d.SetLastIK_Iter_JacobianIK(iter + 1);
                        // current FK
                        float x, y, z;
                        CKinematics.CForward.CalcF(ref c3d, fn, out x, out y, out z); // :contentReference[oaicite:1]{index=1}
                        
                        float ex = tx - x;
                        float ey = ty - y;
                        float ez = tz - z;

                        float err = (float)Math.Sqrt(ex * ex + ey * ey + ez * ez);
                        if (err <= tolMm) return true;

                        // save current joint angles
                        for (int i = 0; i < n; i++)
                        {
                            q0[i] = c3d.GetData(c3d.m_CHeader.pDhParamAll[fn].GetMotors()[i]);
                        }

                        // numeric Jacobian
                        for (int i = 0; i < n; i++)
                        {
                            int id = c3d.m_CHeader.pDhParamAll[fn].GetMotors()[i];

                            c3d.SetData(id, q0[i] + epsDeg);

                            float xp, yp, zp;
                            CKinematics.CForward.CalcF(ref c3d, fn, out xp, out yp, out zp); // :contentReference[oaicite:2]{index=2}

                            // restore
                            c3d.SetData(id, q0[i]);

                            float inv = 1.0f / epsDeg;
                            J[0, i] = (xp - x) * inv;
                            J[1, i] = (yp - y) * inv;
                            J[2, i] = (zp - z) * inv;
                        }

                        // A = J J^T + λ^2 I (3x3)
                        float[,] A = new float[3, 3];
                        for (int r = 0; r < 3; r++)
                        {
                            for (int c = 0; c < 3; c++)
                            {
                                float sum = 0.0f;
                                for (int k = 0; k < n; k++)
                                    sum += J[r, k] * J[c, k];
                                A[r, c] = sum;
                            }
                            A[r, r] += lambda * lambda;
                        }

                        float[] e = new float[3] { ex, ey, ez };

                        // solve A * y = e
                        float[] yvec = Solve3x3(A, e);
                        if (yvec == null) return false;

                        // dq = J^T y
                        for (int i = 0; i < n; i++)
                        {
                            float dq = (J[0, i] * yvec[0] + J[1, i] * yvec[1] + J[2, i] * yvec[2]) * gain;

                            if (dq > maxStepDeg) dq = maxStepDeg;
                            else if (dq < -maxStepDeg) dq = -maxStepDeg;

                            c3d.SetData(c3d.m_CHeader.pDhParamAll[fn].GetMotors()[i], q0[i] + dq);
                        }
                    }

                    return false;
                }
#else
                // ------------------------------------------------------------
                // Jacobian(DLS) IK solver (position only)
                // dq = J^T (J J^T + λ^2 I)^-1 e
                // - angles: degree (Ojw3d.SetData/GetData)
                // - position: mm (CalcF outputs)
                // ------------------------------------------------------------
                public static bool SolveIK_JacobianDLS_PosOnly(
                    ref Ojw.C3d c3d,
                    int fn,
                    //int[] jointIds,
                    float tx, float ty, float tz,
                    int maxIter,
                    float tolMm,
                    float epsDeg,
                    float lambda,
                    float gain,
                    float maxStepDeg
                )
                {
                    c3d.SetLastIK_Iter_JacobianIK(0);
                    int n = c3d.m_CHeader.pDhParamAll[fn].GetMotors_Count();

                    float[] q0 = new float[n];
                    float[,] J = new float[3, n];

                    for (int iter = 0; iter < maxIter; iter++)
                    {
                        c3d.SetLastIK_Iter_JacobianIK(iter + 1);
                        // current FK
                        float x, y, z;
                        CKinematics.CForward.CalcF(ref c3d, fn, out x, out y, out z);

                        float ex = tx - x;
                        float ey = ty - y;
                        float ez = tz - z;

                        float err = (float)Math.Sqrt(ex * ex + ey * ey + ez * ez);
                        if (err <= tolMm) return true;

                        // save current joint angles
                        for (int i = 0; i < n; i++)
                        {
                            q0[i] = c3d.GetData(c3d.m_CHeader.pDhParamAll[fn].GetMotors()[i]);
                        }

                        // numeric Jacobian
                        for (int i = 0; i < n; i++)
                        {
                            int id = c3d.m_CHeader.pDhParamAll[fn].GetMotors()[i];

                            c3d.SetData(id, q0[i] + epsDeg);

                            float xp, yp, zp;
                            CKinematics.CForward.CalcF(ref c3d, fn, out xp, out yp, out zp);

                            // restore
                            c3d.SetData(id, q0[i]);

                            float inv = 1.0f / epsDeg;
                            J[0, i] = (xp - x) * inv;
                            J[1, i] = (yp - y) * inv;
                            J[2, i] = (zp - z) * inv;
                        }

                        // A = J J^T + λ^2 I (3x3)
                        float[,] A = new float[3, 3];
                        for (int r = 0; r < 3; r++)
                        {
                            for (int c = 0; c < 3; c++)
                            {
                                float sum = 0.0f;
                                for (int k = 0; k < n; k++)
                                    sum += J[r, k] * J[c, k];
                                A[r, c] = sum;
                            }
                            A[r, r] += lambda * lambda;
                        }

                        float[] e = new float[3] { ex, ey, ez };

                        // solve A * y = e
                        float[] yvec = Solve3x3(A, e);
                        if (yvec == null) return false;

                        // dq = J^T y
                        for (int i = 0; i < n; i++)
                        {
                            float dq = (J[0, i] * yvec[0] + J[1, i] * yvec[1] + J[2, i] * yvec[2]) * gain;

                            if (dq > maxStepDeg) dq = maxStepDeg;
                            else if (dq < -maxStepDeg) dq = -maxStepDeg;

                            c3d.SetData(c3d.m_CHeader.pDhParamAll[fn].GetMotors()[i], q0[i] + dq);
                        }
                    }

                    return false;
                }

                // ------------------------------------------------------------
                // Jacobian(DLS) IK solver (position with tool offset)
                // 목표 위치에서 툴 길이만큼 뒤로 물러난 손목 위치를 계산하여 IK 수행
                // - 정면 방향: 홈 자세의 colX 방향
                // - rxDeg, ryDeg, rzDeg: 홈 자세 기준 상대 회전 (ZYX 오일러)
                // - toolLength: 툴 길이 (mm), 정면(X축) 방향으로 오프셋
                // ------------------------------------------------------------
                public static bool SolveIK_JacobianDLS_PosWithTool(
                    ref Ojw.C3d c3d,
                    int fn,
                    float tx, float ty, float tz,              // 목표 위치 (툴 끝점)
                    float rxDeg, float ryDeg, float rzDeg,     // 툴 자세 (홈 기준 상대 회전)
                    float toolLength,                          // 툴 길이
                    int maxIter,
                    float tolMm,
                    float epsDeg,
                    float lambda,
                    float gain,
                    float maxStepDeg
                )
                {
                    int n = c3d.m_CHeader.pDhParamAll[fn].GetMotors_Count();
                    if (n <= 0) return false;

                    // ----------------------------
                    // 홈 포지션(관절 0도)에서의 기준 자세 계산
                    // ----------------------------
                    float[] homeX = new float[3];
                    float[] homeY = new float[3];
                    float[] homeZ = new float[3];
                    {
                        int[] motorIds = c3d.m_CHeader.pDhParamAll[fn].GetMotors();
                        float[] backup = new float[n];
                        for (int i = 0; i < n; i++)
                        {
                            backup[i] = c3d.GetData(motorIds[i]);
                            c3d.SetData(motorIds[i], 0.0f);
                        }

                        float hx, hy, hz;
                        CKinematics.CForward.CalcF(ref c3d, fn, out hx, out hy, out hz);
                        GetColsAfterForward(homeX, homeY, homeZ);

                        for (int i = 0; i < n; i++)
                            c3d.SetData(motorIds[i], backup[i]);
                    }

                    // ----------------------------
                    // 목표 자세 계산: R_tgt = R_home * R_user
                    // ----------------------------
                    float[] tgtX = new float[3];
                    float[] tgtY = new float[3];
                    float[] tgtZ = new float[3];
                    {
                        float[] userX = new float[3];
                        float[] userY = new float[3];
                        float[] userZ = new float[3];
                        EulerZYX_ToCols(rxDeg, ryDeg, rzDeg, userX, userY, userZ);

                        for (int i = 0; i < 3; i++)
                        {
                            tgtX[i] = homeX[i] * userX[0] + homeY[i] * userX[1] + homeZ[i] * userX[2];
                            tgtY[i] = homeX[i] * userY[0] + homeY[i] * userY[1] + homeZ[i] * userY[2];
                            tgtZ[i] = homeX[i] * userZ[0] + homeY[i] * userZ[1] + homeZ[i] * userZ[2];
                        }
                    }

                    // ----------------------------
                    // 손목 위치 계산: P_wrist = P_target - R_tgt * (toolLength, 0, 0)
                    // tgtX가 정면 방향이므로 tgtX * toolLength 만큼 뒤로
                    // ----------------------------
                    float wristX = tx - tgtX[0] * toolLength;
                    float wristY = ty - tgtX[1] * toolLength;
                    float wristZ = tz - tgtX[2] * toolLength;

                    // ----------------------------
                    // 손목 위치까지 PosOnly IK 수행
                    // ----------------------------
                    return SolveIK_JacobianDLS_PosOnly(
                        ref c3d, fn,
                        wristX, wristY, wristZ,
                        maxIter, tolMm, epsDeg, lambda, gain, maxStepDeg
                    );
                }

                // ------------------------------------------------------------
                // Jacobian(DLS) IK solver (position with tool offset - Global coordinates)
                // 목표 위치에서 글로벌 좌표계 기준으로 툴 오프셋만큼 뒤로 물러난 손목 위치를 계산
                // - rxDeg, ryDeg, rzDeg: 글로벌 좌표계 기준 절대 회전 (ZYX 오일러)
                // - toolLength: 툴 길이 (mm), 글로벌 회전 적용된 X축 방향으로 오프셋
                // ------------------------------------------------------------
                public static bool SolveIK_JacobianDLS_PosWithToolGlobal(
                    ref Ojw.C3d c3d,
                    int fn,
                    float tx, float ty, float tz,              // 목표 위치 (툴 끝점)
                    float rxDeg, float ryDeg, float rzDeg,     // 툴 자세 (글로벌 좌표계 절대 회전)
                    float toolLength,                          // 툴 길이
                    int maxIter,
                    float tolMm,
                    float epsDeg,
                    float lambda,
                    float gain,
                    float maxStepDeg
                )
                {
                    int n = c3d.m_CHeader.pDhParamAll[fn].GetMotors_Count();
                    if (n <= 0) return false;

                    // ----------------------------
                    // 글로벌 좌표계 기준 목표 자세 계산
                    // R = Rz(rz) * Ry(ry) * Rx(rx)
                    // ----------------------------
                    float[] tgtX = new float[3];
                    float[] tgtY = new float[3];
                    float[] tgtZ = new float[3];
                    EulerZYX_ToCols(rxDeg, ryDeg, rzDeg, tgtX, tgtY, tgtZ);

                    // ----------------------------
                    // 손목 위치 계산: P_wrist = P_target - R * (toolLength, 0, 0)
                    // tgtX가 글로벌 회전 적용된 정면 방향
                    // ----------------------------
                    float wristX = tx - tgtX[0] * toolLength;
                    float wristY = ty - tgtX[1] * toolLength;
                    float wristZ = tz - tgtX[2] * toolLength;

                    // ----------------------------
                    // 손목 위치까지 PosOnly IK 수행
                    // ----------------------------
                    return SolveIK_JacobianDLS_PosOnly(
                        ref c3d, fn,
                        wristX, wristY, wristZ,
                        maxIter, tolMm, epsDeg, lambda, gain, maxStepDeg
                    );
                }
#if false
                // 회전 정의: R = Rz(rz) * Ry(ry) * Rx(rx) (Yaw-Pitch-Roll, ZYX)
                // rxDeg, ryDeg, rzDeg는 "홈 포지션(관절 0도) 자세"에서의 상대 회전
                public static bool SolveIK_JacobianDLS_PosFixedEuler(
                        ref Ojw.C3d c3d,
                        int fn,
                        float tx, float ty, float tz,
                        float rxDeg, float ryDeg, float rzDeg,   // 홈 자세 기준 상대 회전, degree
                        int maxIter,
                        float posTolMm,
                        float rotTol,
                        float epsDeg,
                        float lambda,
                        float gain,
                        float maxStepDeg,
                        float rotWeight
                )
                {
                    int n = c3d.m_CHeader.pDhParamAll[fn].GetMotors_Count();
                    if (n <= 0) return false;

                    // ----------------------------
                    // 홈 포지션(관절 0도)에서의 기준 자세 계산
                    // ----------------------------
                    float[] homeX = new float[3];
                    float[] homeY = new float[3];
                    float[] homeZ = new float[3];
                    {
                        // 현재 관절각 백업
                        int[] motorIds = c3d.m_CHeader.pDhParamAll[fn].GetMotors();
                        float[] backup = new float[n];
                        for (int i = 0; i < n; i++)
                        {
                            backup[i] = c3d.GetData(motorIds[i]);
                            c3d.SetData(motorIds[i], 0.0f);  // 모든 관절 0도로 설정
                        }

                        // 홈 포지션에서 FK 계산
                        float hx, hy, hz;
                        CKinematics.CForward.CalcF(ref c3d, fn, out hx, out hy, out hz);
                        GetColsAfterForward(homeX, homeY, homeZ);

                        // 관절각 복원
                        for (int i = 0; i < n; i++)
                            c3d.SetData(motorIds[i], backup[i]);
                    }

                    // ----------------------------
                    // 목표 자세 = R_home * R_user
                    // R_user = Rz(rz) * Ry(ry) * Rx(rx)
                    // ----------------------------
                    float[] tgtX = new float[3];
                    float[] tgtY = new float[3];
                    float[] tgtZ = new float[3];
                    {
                        // 사용자 입력 회전행렬
                        float[] userX = new float[3];
                        float[] userY = new float[3];
                        float[] userZ = new float[3];
                        EulerZYX_ToCols(rxDeg, ryDeg, rzDeg, userX, userY, userZ);

                        // R_tgt = R_home * R_user
                        // R_home = [homeX, homeY, homeZ] (열벡터)
                        // R_user = [userX, userY, userZ] (열벡터)
                        // R_tgt의 각 열 = R_home * (R_user의 각 열)
                        for (int i = 0; i < 3; i++)
                        {
                            tgtX[i] = homeX[i] * userX[0] + homeY[i] * userX[1] + homeZ[i] * userX[2];
                            tgtY[i] = homeX[i] * userY[0] + homeY[i] * userY[1] + homeZ[i] * userY[2];
                            tgtZ[i] = homeX[i] * userZ[0] + homeY[i] * userZ[1] + homeZ[i] * userZ[2];
                        }
                    }

                    float[] q0 = new float[n];
                    float[,] J = new float[6, n];

                    float[] curX = new float[3];
                    float[] curY = new float[3];
                    float[] curZ = new float[3];

                    float[] perX = new float[3];
                    float[] perY = new float[3];
                    float[] perZ = new float[3];

                    // 디버그: 목표 자세 출력 (성능: 주석 처리)
                    //Ojw.CMessage.Write(String.Format(
                    //    "[IK Debug] tgtX=({0:F3},{1:F3},{2:F3}) tgtY=({3:F3},{4:F3},{5:F3}) tgtZ=({6:F3},{7:F3},{8:F3})",
                    //    tgtX[0], tgtX[1], tgtX[2], tgtY[0], tgtY[1], tgtY[2], tgtZ[0], tgtZ[1], tgtZ[2]));

                    for (int iter = 0; iter < maxIter; iter++)
                    {
                        // ----------------------------
                        // 현재 FK
                        // ----------------------------
                        float x, y, z;
                        CKinematics.CForward.CalcF(ref c3d, fn, out x, out y, out z);

                        GetColsAfterForward(curX, curY, curZ);

                        // 위치 오차
                        float ex = tx - x;
                        float ey = ty - y;
                        float ez = tz - z;

                        // 자세 오차(고정 목표)
                        float oex, oey, oez;
                        CalcOriError(curX, curY, curZ, tgtX, tgtY, tgtZ, out oex, out oey, out oez);

                        float posErr = (float)Math.Sqrt(ex * ex + ey * ey + ez * ez);
                        float rotErr = (float)Math.Sqrt(oex * oex + oey * oey + oez * oez);

                        // 디버그: 첫 3회 iteration 출력 (성능: 주석 처리)
                        //if (iter < 3)
                        //{
                        //    Ojw.CMessage.Write(String.Format(
                        //        "[IK Debug] iter={0} pos=({1:F2},{2:F2},{3:F2}) posErr={4:F2} rotErr={5:F4}",
                        //        iter, x, y, z, posErr, rotErr));
                        //    Ojw.CMessage.Write(String.Format(
                        //        "[IK Debug]   curX=({0:F3},{1:F3},{2:F3}) curY=({3:F3},{4:F3},{5:F3}) curZ=({6:F3},{7:F3},{8:F3})",
                        //        curX[0], curX[1], curX[2], curY[0], curY[1], curY[2], curZ[0], curZ[1], curZ[2]));
                        //    Ojw.CMessage.Write(String.Format(
                        //        "[IK Debug]   oriErr=({0:F4},{1:F4},{2:F4})", oex, oey, oez));
                        //}

                        if (posErr <= posTolMm && rotErr <= rotTol) return true;

                        // 현재 관절각 저장
                        for (int i = 0; i < n; i++)
                        {
                            int id = c3d.m_CHeader.pDhParamAll[fn].GetMotors()[i];
                            q0[i] = c3d.GetData(id);
                        }

                        float base_oex = oex, base_oey = oey, base_oez = oez;

                        // ----------------------------
                        // 수치미분 Jacobian (6xN)
                        // ----------------------------
                        for (int i = 0; i < n; i++)
                        {
                            int id = c3d.m_CHeader.pDhParamAll[fn].GetMotors()[i];

                            c3d.SetData(id, q0[i] + epsDeg);

                            float xp, yp, zp;
                            CKinematics.CForward.CalcF(ref c3d, fn, out xp, out yp, out zp);
                            GetColsAfterForward(perX, perY, perZ);

                            c3d.SetData(id, q0[i]);

                            float inv = 1.0f / epsDeg;

                            // position rows
                            J[0, i] = (xp - x) * inv;
                            J[1, i] = (yp - y) * inv;
                            J[2, i] = (zp - z) * inv;

                            // orientation error rows: d(eR)/dtheta
                            float poex, poey, poez;
                            CalcOriError(perX, perY, perZ, tgtX, tgtY, tgtZ, out poex, out poey, out poez);

                            J[3, i] = (poex - base_oex) * inv;
                            J[4, i] = (poey - base_oey) * inv;
                            J[5, i] = (poez - base_oez) * inv;
                        }

                        // ----------------------------
                        // 가중치(자세 고정 강도)
                        // ----------------------------
                        float[] e = new float[6];
                        e[0] = ex;
                        e[1] = ey;
                        e[2] = ez;
                        e[3] = oex * rotWeight;
                        e[4] = oey * rotWeight;
                        e[5] = oez * rotWeight;

                        for (int i = 0; i < n; i++)
                        {
                            J[3, i] *= rotWeight;
                            J[4, i] *= rotWeight;
                            J[5, i] *= rotWeight;
                        }

                        // ----------------------------
                        // DLS
                        // ----------------------------
                        float[,] A = new float[6, 6];
                        for (int r = 0; r < 6; r++)
                        {
                            for (int c = 0; c < 6; c++)
                            {
                                float sum = 0.0f;
                                for (int k = 0; k < n; k++)
                                    sum += J[r, k] * J[c, k];
                                A[r, c] = sum;
                            }
                            A[r, r] += lambda * lambda;
                        }

                        float[] yvec = Solve6x6(A, e);
                        if (yvec == null) return false;

                        // dq = J^T y
                        for (int i = 0; i < n; i++)
                        {
                            float dq = 0.0f;
                            for (int r = 0; r < 6; r++) dq += J[r, i] * yvec[r];
                            dq *= gain;

                            if (dq > maxStepDeg) dq = maxStepDeg;
                            else if (dq < -maxStepDeg) dq = -maxStepDeg;

                            int id = c3d.m_CHeader.pDhParamAll[fn].GetMotors()[i];
                            c3d.SetData(id, q0[i] + dq);
                        }
                    }

                    return false;
                }
#endif
                // ===== helpers (VS2010 compatible) =====
                private static void GetColsAfterForward(float[] colX, float[] colY, float[] colZ)
                {
                    double[] dx = CKinematics.CForward.GetDirectionVectors_after_Forward(0);
                    double[] dy = CKinematics.CForward.GetDirectionVectors_after_Forward(1);
                    double[] dz = CKinematics.CForward.GetDirectionVectors_after_Forward(2);

                    colX[0] = (float)dx[0]; colX[1] = (float)dx[1]; colX[2] = (float)dx[2];
                    colY[0] = (float)dy[0]; colY[1] = (float)dy[1]; colY[2] = (float)dy[2];
                    colZ[0] = (float)dz[0]; colZ[1] = (float)dz[1]; colZ[2] = (float)dz[2];
                }

                // SO(3) log map 기반 오리엔테이션 오차 계산 (월드 좌표계)
                // R_err = R_tgt * R_cur^T 의 log map으로 오차 벡터 추출
                // 이렇게 하면 오차가 월드 좌표계에서 직접 표현됨
                private static void CalcOriError(
                    float[] curX, float[] curY, float[] curZ,
                    float[] tgtX, float[] tgtY, float[] tgtZ,
                    out float oex, out float oey, out float oez)
                {
                    // R_cur = [curX, curY, curZ] (열벡터)
                    // R_tgt = [tgtX, tgtY, tgtZ] (열벡터)
                    // R_err = R_tgt * R_cur^T  (월드 좌표계 오차)

                    // R_cur^T의 행 = curX, curY, curZ (각각 행벡터로 사용)
                    // R_err[i,j] = dot(tgt_col_i, cur_col_j)
                    // R_err = R_tgt * R_cur^T 이므로:
                    // R_err[i,j] = sum_k(R_tgt[i,k] * R_cur^T[k,j]) = sum_k(R_tgt[i,k] * R_cur[j,k])
                    // R_tgt의 i행 = [tgtX[i], tgtY[i], tgtZ[i]]
                    // R_cur의 j열 = curX, curY, curZ 중 j번째

                    float r00 = tgtX[0] * curX[0] + tgtY[0] * curY[0] + tgtZ[0] * curZ[0];
                    float r01 = tgtX[0] * curX[1] + tgtY[0] * curY[1] + tgtZ[0] * curZ[1];
                    float r02 = tgtX[0] * curX[2] + tgtY[0] * curY[2] + tgtZ[0] * curZ[2];

                    float r10 = tgtX[1] * curX[0] + tgtY[1] * curY[0] + tgtZ[1] * curZ[0];
                    float r11 = tgtX[1] * curX[1] + tgtY[1] * curY[1] + tgtZ[1] * curZ[1];
                    float r12 = tgtX[1] * curX[2] + tgtY[1] * curY[2] + tgtZ[1] * curZ[2];

                    float r20 = tgtX[2] * curX[0] + tgtY[2] * curY[0] + tgtZ[2] * curZ[0];
                    float r21 = tgtX[2] * curX[1] + tgtY[2] * curY[1] + tgtZ[2] * curZ[1];
                    float r22 = tgtX[2] * curX[2] + tgtY[2] * curY[2] + tgtZ[2] * curZ[2];

                    // SO(3) log map
                    float trace = r00 + r11 + r22;
                    float cosTheta = (trace - 1.0f) * 0.5f;

                    // clamp to [-1, 1]
                    if (cosTheta > 1.0f) cosTheta = 1.0f;
                    else if (cosTheta < -1.0f) cosTheta = -1.0f;

                    float theta = (float)Math.Acos(cosTheta);

                    // 회전각이 매우 작으면 선형 근사
                    if (theta < 1e-6f)
                    {
                        // R ≈ I + [ω]× 이므로 ω ≈ 0.5 * (R - R^T)의 vee
                        oex = 0.5f * (r21 - r12);
                        oey = 0.5f * (r02 - r20);
                        oez = 0.5f * (r10 - r01);
                        return;
                    }

                    // 회전각이 π에 가까우면 특수 처리
                    if (theta > 3.1415926f - 1e-6f)
                    {
                        // 대각 요소에서 축 추출
                        float ax = (float)Math.Sqrt(Math.Max(0, (r00 + 1.0f) * 0.5f));
                        float ay = (float)Math.Sqrt(Math.Max(0, (r11 + 1.0f) * 0.5f));
                        float az = (float)Math.Sqrt(Math.Max(0, (r22 + 1.0f) * 0.5f));

                        // 부호 결정
                        if (r01 < 0) ay = -ay;
                        if (r02 < 0) az = -az;

                        oex = ax * theta;
                        oey = ay * theta;
                        oez = az * theta;
                        return;
                    }

                    // 일반적인 경우: log(R) = (theta / 2*sin(theta)) * (R - R^T)
                    float k = theta / (2.0f * (float)Math.Sin(theta));
                    oex = k * (r21 - r12);
                    oey = k * (r02 - r20);
                    oez = k * (r10 - r01);
                }

                private static void Cross(float[] a, float[] b, out float x, out float y, out float z)
                {
                    x = a[1] * b[2] - a[2] * b[1];
                    y = a[2] * b[0] - a[0] * b[2];
                    z = a[0] * b[1] - a[1] * b[0];
                }

                // R = Rz(rz)*Ry(ry)*Rx(rx), output columns
                // colX가 정면(접근 방향)일 때의 직관적 해석:
                // - rx: roll (정면 축 기준 회전)
                // - ry: pitch (위/아래)
                // - rz: yaw (좌/우)
                private static void EulerZYX_ToCols(float rxDeg, float ryDeg, float rzDeg,
                    float[] colX, float[] colY, float[] colZ)
                {
                    float rx = (float)(rxDeg * Math.PI / 180.0);
                    float ry = (float)(ryDeg * Math.PI / 180.0);
                    float rz = (float)(rzDeg * Math.PI / 180.0);

                    float cx = (float)Math.Cos(rx), sx = (float)Math.Sin(rx);
                    float cy = (float)Math.Cos(ry), sy = (float)Math.Sin(ry);
                    float cz = (float)Math.Cos(rz), sz = (float)Math.Sin(rz);

                    // R = Rz(rz) * Ry(ry) * Rx(rx)
                    // colX가 정면이므로, rz는 colZ 기준 회전(yaw), ry는 colY 기준(pitch)
                    float r00 = cz * cy;
                    float r01 = cz * sy * sx - sz * cx;
                    float r02 = cz * sy * cx + sz * sx;

                    float r10 = sz * cy;
                    float r11 = sz * sy * sx + cz * cx;
                    float r12 = sz * sy * cx - cz * sx;

                    float r20 = -sy;
                    float r21 = cy * sx;
                    float r22 = cy * cx;

                    colX[0] = r00; colX[1] = r10; colX[2] = r20;
                    colY[0] = r01; colY[1] = r11; colY[2] = r21;
                    colZ[0] = r02; colZ[1] = r12; colZ[2] = r22;
                }

                // 6x6 가우스 소거
                private static float[] Solve6x6(float[,] A, float[] b)
                {
                    int n = 6;
                    float[,] M = new float[n, n + 1];

                    for (int r = 0; r < n; r++)
                    {
                        for (int c = 0; c < n; c++) M[r, c] = A[r, c];
                        M[r, n] = b[r];
                    }

                    for (int k = 0; k < n; k++)
                    {
                        int piv = k;
                        float best = Abs(M[k, k]);
                        for (int r = k + 1; r < n; r++)
                        {
                            float v = Abs(M[r, k]);
                            if (v > best) { best = v; piv = r; }
                        }
                        if (best < 1e-8f) return null;

                        if (piv != k)
                        {
                            for (int c = k; c < n + 1; c++)
                            {
                                float tmp = M[k, c];
                                M[k, c] = M[piv, c];
                                M[piv, c] = tmp;
                            }
                        }

                        float div = M[k, k];
                        for (int c = k; c < n + 1; c++) M[k, c] /= div;

                        for (int r = 0; r < n; r++)
                        {
                            if (r == k) continue;
                            float f = M[r, k];
                            if (Abs(f) < 1e-12f) continue;
                            for (int c = k; c < n + 1; c++) M[r, c] -= f * M[k, c];
                        }
                    }

                    float[] x = new float[n];
                    for (int i = 0; i < n; i++) x[i] = M[i, n];
                    return x;
                }

#endif

                // ------------------------------------------------------------
                // Simple Gauss elimination for 3x3
                // ------------------------------------------------------------
                private static float[] Solve3x3(float[,] A, float[] b)
                {
                    float[,] M = new float[3, 4];

                    int r, c;

                    for (r = 0; r < 3; r++)
                    {
                        for (c = 0; c < 3; c++) M[r, c] = A[r, c];
                        M[r, 3] = b[r];
                    }

                    for (int k = 0; k < 3; k++)
                    {
                        // pivot
                        int piv = k;
                        float best = Abs(M[k, k]);
                        for (r = k + 1; r < 3; r++)
                        {
                            float v = Abs(M[r, k]);
                            if (v > best) { best = v; piv = r; }
                        }
                        if (best < 1e-8f) return null;

                        if (piv != k)
                        {
                            for (c = k; c < 4; c++)
                            {
                                float tmp = M[k, c];
                                M[k, c] = M[piv, c];
                                M[piv, c] = tmp;
                            }
                        }

                        float div = M[k, k];
                        for (c = k; c < 4; c++) M[k, c] /= div;

                        for (r = 0; r < 3; r++)
                        {
                            if (r == k) continue;
                            float f = M[r, k];
                            for (c = k; c < 4; c++) M[r, c] -= f * M[k, c];
                        }
                    }

                    return new float[3] { M[0, 3], M[1, 3], M[2, 3] };
                }

                private static float Abs(float v) { return (v < 0) ? -v : v; }
                #endregion Inverse Jacob
            }
        }
    }
}
