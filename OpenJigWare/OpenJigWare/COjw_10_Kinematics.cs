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
        public class CKinematics // Forward / Inverse
        {
            public class CForward
            {
                #region Class Control - DHParam

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
                        if (DhParam.nAxisDir < 2)
                        {
                            dTheta += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 0) ? 1.0f : -1.0f));
                            if (DhParam.nAxisDir != 0)
                            {
                                bInv = true;
                            }
                        }
                        // D
                        else
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
            for (i = 1; i < nCnt; i++)
            {
                CMath.CalcMatrix_Str(4, SDhT_Result_Str.aStrT, aSDhT_Str[i].aStrT, out SDhT_Result_Str.aStrT);
                for (int ii = 0; ii < 4; ii++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        strResult += SDhT_Result_Str.aStrT[ii, j] + "   ";
                    }
                    strResult += "\r\n";
                }
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
                // [0~255] After one of the step sizes DhParamAll variables into the function, the function receives the value of the result to x,y,z(Kor: 0~255 에 해당하는 DhParamAll 변수중 하나를 택해 함수에 넣은 후 그 결과 값을 x,y,z 로 받는 함수)
                // afAxisValue shall be to put the value of the motor(Put the Motor ID(DhParam.nAxisNum) into Header...).(Kor: afAxisValue 에는 모터의 값(모터의 ID(DhParam.nAxisNum)는 헤더에 기록...)을 넣도록 한다.)
                private static bool CalcDhParamAll(CDhParamAll DhParamAll, double[] adAxisValue, out double dX, out double dY, out double dZ)
                {
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
                        if (DhParam.nAxisDir < 2) dTheta += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 0) ? 1.0f : -1.0f));
                        // D
                        else dD += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 2) ? 1.0f : -1.0f));
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
                }

                private static bool CalcDhParamAll(CDhParamAll DhParamAll, double[] adAxisValue, out double[] dColX, out double[] dColY, out double[] dColZ, out double dX, out double dY, out double dZ, out double dDir_X, out double dDir_Y, out double dDir_Z)
                {
                    int i, j;
                    int nCnt = DhParamAll.GetCount();

                    SDhT_t[] aSDhT = new SDhT_t[nCnt];
                    SDhT_t SDhT_Result = new SDhT_t();

                    dColX = new double[3];
                    dColY = new double[3];
                    dColZ = new double[3];
                    Array.Clear(dColX, 0, dColX.Length);
                    Array.Clear(dColY, 0, dColY.Length);
                    Array.Clear(dColZ, 0, dColZ.Length);

                    if (nCnt <= 0)
                    {
                        dX = dY = dZ = 0.0;
                        dDir_X = dDir_Y = dDir_Z = 0.0f;
                        return false;
                    }

                    // Initialize
                    CMath.CalcT(0, 0, 0, 0, out SDhT_Result.adT);

                    //double dTheta;
                    double dTheta, dA, dD;
                    CDhParam DhParam = new CDhParam();
                    double dAngleData = 0.0;
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
                        #endregion (dAngleData)Read Angle value(Kor: 각도값 읽어오기)
                        //dTheta = DhParam.dTheta + (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 0) ? 1.0f : -1.0f));
                        //CMath.CalcT(DhParam.dA, DhParam.dAlpha, DhParam.dD, dTheta, out aSDhT[i].adT);
                        dTheta = DhParam.dTheta;
                        dA = DhParam.dA;
                        dD = DhParam.dD;
                        // Theta
                        if (DhParam.nAxisDir < 2) dTheta += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 0) ? 1.0f : -1.0f));
                        // D
                        else dD += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 2) ? 1.0f : -1.0f));
                        CMath.CalcT(dA, DhParam.dAlpha, dD, dTheta, out aSDhT[i].adT);
                    }
                    DhParam = null;

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
                            DhParamAll.AddData(pCDhParam[i]);
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

                    try
                    {
                        bool bRet = false;

                        String[] pstrData = strData.Split(',');
                        if (pstrData.Length >= 6)
                        {
                            //int i = 0, j = 0, k = 0;
                            int nNum = 0;

                            foreach (string strItem in pstrData)
                            {
                                if (nNum == 0) CDhParam.dA = CConvert.StrToDouble(strItem);
                                else if (nNum == 1) CDhParam.dD = CConvert.StrToDouble(strItem);
                                else if (nNum == 2) CDhParam.dTheta = CConvert.StrToDouble(strItem);
                                else if (nNum == 3) CDhParam.dAlpha = CConvert.StrToDouble(strItem);
                                else if (nNum == 4) CDhParam.nAxisNum = CConvert.StrToInt(strItem);
                                else if (nNum == 5) CDhParam.nAxisDir = CConvert.StrToInt(strItem);
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
                        CConvert.IntToStr(OjwDhParam.nAxisDir) +
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
                    i = 0; adCalcZ[i, 0] = (double)CMath.Cos(dAngleY); adCalcZ[i, 1] = -(double)CMath.Sin(dAngleY); adCalcZ[i, 2] = 0.0f; adCalcZ[i, 3] = 0.0f;
                    i = 1; adCalcZ[i, 0] = (double)CMath.Sin(dAngleY); adCalcZ[i, 1] = (double)CMath.Cos(dAngleY); adCalcZ[i, 2] = 0.0f; adCalcZ[i, 3] = 0.0f;
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

                private const int _CNT_ADDRESS = 0xffffff;//0xfffff;

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
                //private const int _MOD = 0x0004000;
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
                private const int _MOD   = 0x4000000;
                private const int _ROUND = 0x8000000;

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
                        strSource = CConvert.RemoveCaption(strSource, true, true);

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
                            if (strCurr == ",")
                            {
                                if (bSqrt == true) strCurr = ",_UP1_";
                                else if (bPow == true) strCurr = ",_UP0_";
                                else if (bAtan2 == true) strCurr = ",_UP2_";
                                else if (bAcos2 == true) strCurr = ",_UP3_";
                                else if (bAsin2 == true) strCurr = ",_UP4_";
                                bSqrt = false;
                                bPow = false;
                                bAtan2 = false;
                                bAcos2 = false;
                                bAsin2 = false;
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
                                    if (
                                        ((pstrLineSort[i - 1] == "=") && (pstrLineSort[i] != "-")) &&
                                        (CConvert.CheckCalc_Compare(_SIN | _COS | _TAN | _ASIN | _ACOS | _ATAN | _POW | _SQRT | _ABS | _ATAN2 | _ACOS2 | _ASIN2 | _ROUND, pstrLineSort[i]) == 0)
                                    )
                                        pstrTmp[nPos2++] = "+" + pstrLineSort[i];
                                    else if (
                                        ((pstrLineSort[i - 1] == "=") && (pstrLineSort[i] == "-")) &&
                                        (CConvert.CheckCalc_Compare(_SIN | _COS | _TAN | _ASIN | _ACOS | _ATAN | _POW | _SQRT | _ABS | _ATAN2 | _ACOS2 | _ASIN2 | _ROUND, pstrLineSort[i]) == 0) &&
                                        (CConvert.CheckCalc_Compare(_SIN | _COS | _TAN | _ASIN | _ACOS | _ATAN | _POW | _SQRT | _ABS | _ATAN2 | _ACOS2 | _ASIN2 | _ROUND, pstrLineSort[i - 1]) == 0)
                                    )
                                        continue;
                                    else if (
                                          (CConvert.CheckCalc_Compare(_PLUS | _MINUS | _MUL | _DIV | _MOD | _COMMA, pstrLineSort[i - 1]) != 0) &&
                                          (CConvert.CheckCalc_Compare(_SIN | _COS | _TAN | _ASIN | _ACOS | _ATAN | _POW | _SQRT | _ABS | _ATAN2 | _ACOS2 | _ASIN2 | _ROUND, pstrLineSort[i]) == 0)
                                      )
                                    {
                                        pstrTmp[nPos2++] = pstrLineSort[i - 1] + pstrLineSort[i];
                                    }
                                    else if (CConvert.CheckCalc_Compare(_SIN | _COS | _TAN | _ASIN | _ACOS | _ATAN | _POW | _SQRT | _ABS | _ATAN2 | _ACOS2 | _ASIN2 | _ROUND, pstrLineSort[i - 1]) != 0)
                                    {
                                        pstrTmp[nPos2++] = ((pstrLineSort[i - 2] == "=") ? "+" : pstrLineSort[i - 2]) + pstrLineSort[i - 1] + pstrLineSort[i];
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
                        m_nVarWNum += nW;
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
                        for (int i = 0; i < nCnt; i++)
                        {

                            strResult += "VAR," + pstrVar[i] + "\r\n" + "CLR," + pstrVar[i] + "\r\n";
                            //strResult += "VAR," + pstrVar[i] + "\r\n";
                            String strTmp = pstrData[i];
                            String strEnd = "";
                            bool bSqrt = false;
                            bool bAtan2 = false;
                            bool bAcos2 = false;
                            bool bRound = false;
                            //bool bAsin2 = false;
                            bool bPow = false;
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
                                                    nIndex = strTmp.IndexOf(",_UP6_");// 
                                                    //if (nIndex >= 0) bRound = true;
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

                            if (nIndex >= 0)
                            {
                                strTmp = pstrData[i].Substring(0, nIndex);
                                strEnd = pstrData[i].Substring(nIndex, pstrData[i].Length - nIndex);
                                strEnd = CConvert.RemoveString(strEnd, ",_UP0_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UP1_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UP2_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UP3_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UP4_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UP5_");
                                strEnd = CConvert.RemoveString(strEnd, ",_UP6_");
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
                                strResult += ((bPow == true) ? "POW," : ((bSqrt == true) ? "SQRT," : ((bAtan2 == true) ? "ATAN2," : ((bAcos2 == true) ? "ACOS2," : ((bRound == true) ? "ROUND," : "ASIN2,"))))) + strEnd + "\r\n";
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
                                    else continue;
                                    pstrOperand[0] = CConvert.IntToHex(nData, nWidth);
                                }
                                else if (nPos == 1)
                                {
                                    if (strCode.IndexOf("sin") == 0) nData = 0x00000600;
                                    else if (strCode.IndexOf("cos") == 0) nData = 0x00000700;
                                    else if (strCode.IndexOf("tan") == 0) nData = 0x00000800;
                                    else if (strCode.IndexOf("asin") == 0) nData = 0x00000900;
                                    else if (strCode.IndexOf("acos") == 0) nData = 0x00000a00;
                                    else if (strCode.IndexOf("atan") == 0) nData = 0x00000b00;
                                    else if (strCode.IndexOf("sqrt") == 0) nData = 0x00000c00;
                                    else if (strCode.IndexOf("pow") == 0) nData = 0x00000d00;
                                    else if (strCode.IndexOf("clr") == 0) nData = 0x00000e00;
                                    else if (strCode.IndexOf("abs") == 0) nData = 0x00000f00;
                                    else if (strCode.IndexOf("atan2") == 0) nData = 0x00001000;
                                    else if (strCode.IndexOf("acos2") == 0) nData = 0x00001100;
                                    else if (strCode.IndexOf("asin2") == 0) nData = 0x00001200;
                                    else if (strCode.IndexOf("round") == 0) nData = 0x00001400;

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
                        int nTmp = -1;
                        bool bError = true;
                        while (bError == true)
                        {
                            strTmp = "";
                            bool bCheck = false;

                            strTmp += strCompile1[0];
                            for (int i = 1; i < strCompile1.Length; i++)
                            {
                                if ((nTmp < 0) && (strCompile1[i - 1] == ',') && (strCompile1[i] != '('))
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

                public static double GetValue_X() { return m_dX; }
                public static double GetValue_Y() { return m_dY; }
                public static double GetValue_Z() { return m_dZ; }
                public static double GetValue_V(int nIndex) { if (nIndex >= _CNT_VAR_V) return 0; return m_adV[nIndex]; }
                public static double GetValue_Motor(int nIndex) { if (nIndex >= _CNT_MOTOR) return 0; return m_adMot[nIndex]; }

                public static double[] GetValue_V() { return m_adV; }
                public static double[] GetValue_Motor() { return m_adMot; }

                public static bool CalcCode(ref SOjwCode_t SCode)
                {
                    if (SCode.bInit == false)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += "[CalcCode]" + "Variable[SOjwCode_t] is not initialized (the first compilation required)";// "SOjwCode_t 변수가 초기화(최초컴파일 필요)되지 않았습니다.";
                        return false;
                    }
                    try
                    {
                        Array.Copy(m_adMot, 0, SCode.adOperation_Memory, _ADDRESS_MOTOR, _CNT_MOTOR);
                        SCode.adOperation_Memory[_ADDRESS_X] = GetValue_X();
                        SCode.adOperation_Memory[_ADDRESS_Y] = GetValue_Y();
                        SCode.adOperation_Memory[_ADDRESS_Z] = GetValue_Z();
                        Array.Copy(m_adV, 0, SCode.adOperation_Memory, _ADDRESS_V, _CNT_VAR_V);

                        int nSize = SCode.nCnt_Operation;
                        int nAddress = -1;
                        for (int i = 0; i < nSize; i++)
                        {
                            long lOperation_Cmd = SCode.alOperation_Cmd[i];
                            // If the [Var] address values are written to the data(Kor: Var 인 경우 해당 데이타엔 번지값이 기록)
                            if ((lOperation_Cmd & 0x0000ff) == 1) // 번지 측정은 0xff 에서만 하기에 ...
                            {
                                // get the address(Kor: 번지값을 가져온다.)
                                nAddress = (int)(SCode.adOperation_Data[i]);
                                continue;
                            }
                            if (nAddress < 0)
                            {
                                m_nErrorCode = 9;
                                m_strError_Etc += "[CalcCode]" + "Import address fails";// "번지값 가져오기 실패";
                                return false;//Error
                            }

                            double dData;
                            long lAddrCheck = lOperation_Cmd & 0xffff0000;

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
                                CalcCmd(lCmd, dData, ref SCode.adOperation_Memory[(int)SCode.adOperation_Data[i]]);
                                dData = SCode.adOperation_Memory[(int)SCode.adOperation_Data[i]];
                            }

                            // Second computation(Kor: 2차연산)
                            lCmd = (lOperation_Cmd & 0x00ff);
                            CalcCmd(lCmd, dData, ref SCode.adOperation_Memory[nAddress]);
                            //lCmd = 0;
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
                    }
                    catch (System.Exception e)
                    {
                        m_nErrorCode = 9;
                        m_strError_Etc += "[CalcCode]" + e.ToString();
                        return false;
                    }
                    return true;
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
                            if (dValue > 1) dValue = 1.0f;
                            else if (dValue < -1) dValue = -1.0f;

                            dValue = (double)CMath.ASin(dValue);
                        }
                        else if (lCmd == 0x0a) // acos
                        {
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
            }
        }
    }
}
