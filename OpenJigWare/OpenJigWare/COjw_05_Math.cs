using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CMath
        {
            #region Math Fuction
            public const double _ZERO = 0.000001;
            public const double _PI = Math.PI;
            public const double PI = Math.PI;

            public static double Zero() { return _ZERO; }

            public static double Round(double x) { return Math.Round(x); }
            public static double Round(double x, int nPos) { return Math.Round(x, nPos); }

            #region Radian <-> Degree : you can use any things when it comes to comfortable with you. All things are same.
            public static double Deg2Rad(double x) { return (x / 180.0 * PI); }
            public static double Rad2Deg(double x) { return (x * 180.0 / PI); }

            public static double D2R(double dValue) { return dValue * _PI / 180.0; }
            public static double R2D(double dValue) { return dValue * 180.0 / _PI; }
            
            public static double AngleToRadian(double t) { return (_PI / 180.0 * (double)(t)); }
            public static double RadianToAngle(double t) { return ((double)(t) / 180.0 * _PI); }
            #endregion Radian <-> Degree : you can use any things when it comes to comfortable with you. All things are same.
            
            #region sin/cos/tan function with degree value
            public static double Sin(double t) { return (Math.Sin(t / 180.0 * _PI)); }
            public static double Cos(double t) { return (Math.Cos(t / 180.0 * _PI)); }
            public static double Tan(double t) { return (Math.Tan(t / 180.0 * _PI)); }

            public static double ASin(double t) { return (Math.Asin(t) * 180.0 / _PI); }
            public static double ACos(double t) { return (Math.Acos(t) * 180.0 / _PI); }
            // get the length of hypotenuse
            public static double Hypotenuse(double x1, double y1) { return (Math.Sqrt(Math.Pow((x1), 2) + Math.Pow((y1), 2))); }
            // it is better for calcing 360 degree. but you should input point value instead of hipotenuse value
            public static double ASinXY(double x1, double y1) { return (x1 < 0.0) ? (180.0 - (Math.Asin(y1 / Hypotenuse(x1, y1)) * 180.0 / _PI)) : ((y1 < 0.0) ? (360.0 + (Math.Asin(y1 / Hypotenuse(x1, y1)) * 180.0 / _PI)) : (Math.Asin(y1 / Hypotenuse(x1, y1)) * 180.0 / _PI)); }
            public static double ACosXY(double x1, double y1) { return (y1 < 0.0) ? (360.0 - (Math.Acos(x1 / Hypotenuse(x1, y1)) * 180.0 / _PI)) : (Math.Acos(x1 / Hypotenuse(x1, y1)) * 180.0 / _PI); }


            //public static double aTan2(double y_Up_1, double x_Down_1) { return ((double)(x_Down_1) < 0 && (double)(y_Up_1) == 0) ? 180.0 : (((double)(y_Up_1) < 0) ? (aTan((double)(y_Up_1) / (double)(((double)(x_Down_1) == 0) ? _ZERO : (double)(x_Down_1))) + 180.0) : (aTan((double)(y_Up_1) / (double)(((double)(x_Down_1) == 0) ? _ZERO : (double)(x_Down_1))))); }

            // 2,3 dimension -> Sin Value
            public static double ASin_Plane_23(double t) { return (180.0 - ASin(t)); }
            // 4 dimension -> Sin Value
            public static double ASin_Plane_4(double t) { return (360.0 + ASin(t)); }
            // => All
            public static double ASin_Plane(int nPlane, double t) { return (((nPlane % 4) == 1) || ((nPlane % 4) == 2)) ? (180.0 - ASin(t)) : (((nPlane % 4) == 3) ? (360.0 + ASin(t)) : ASin(t)); }
            public static double ACos_Plane(int nPlane, double t) { return ((nPlane % 4) > 1) ? (360.0 - ACos(t)) : ACos(t); }
#if false
            //public static double aCos2(double x1, double y1) { return (y1 < 0.0) ? (360.0 - (Math.Acos(x1 / Hypotenuse(x1, y1)) * 180.0 / _PI)) : (Math.Acos(x1 / Hypotenuse(x1, y1)) * 180.0 / _PI); }
            //public static double aCos_(double x1, double y1) { return (y1 < 0.0) ? (360.0 - (Math.Acos(x1 / Hypotenuse(x1, y1)) * 180.0 / _PI)) : (Math.Acos(x1 / Hypotenuse(x1, y1)) * 180.0 / _PI); }
            
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            //#define pS_x(pS,pO)		((pS.x) - (pO.x))
            //#define pS_y(pS,pO)		((pS.y) - (pO.y))
            //#define pD_x(pD,pO)		((pD.x) - (pO.x))
            //#define pD_y(pD,pO)		((pD.y) - (pO.y))
            // 두 벡터의 사이각∠pSpD = pSㆍpD = (s1*d1) + (s2*d2), & pSㆍpD = |pS|ㆍ|pD| cosθ
            // 최종식은 cosθ = (pSㆍpD)/(|pS|ㆍ|pD|) = (s1d1+s2d2) / (sqrt(s1^2+s2^2) * sqrt(d1^2+d2^2))
            // ∴θ=arcCos( (s1d1+s2d2) / (sqrt(s1^2+s2^2)*sqrt(d1^2+d2^2)) ) 
            // float tmpTheta = aCos((s1d1+s2d2) / (sqrt(s1^2+s2^2)*sqrt(d1^2+d2^2)));
            // 논리적으론 10.0의 임의값보단 pO와 pD의 Distance를 구하는게 옳으나 계산복잡도를 줄이기 위해 임의의 값을 넣었음.
            //#define Angle_Vector(pS,pO,pD)				(float)aCos(((float)(pS_x(pS,pO))*(float)(pD_x(pD,pO))+(float)(pS_y(pS,pO))*(float)(pD_y(pD,pO))) / (Sqrt((float)pow((float)(pS_x(pS,pO)),2)+(float)pow((float)(pS_y(pS,pO)),2))*Sqrt((float)pow((float)(pD_x(pD,pO)),2)+(float)pow((float)(pD_y(pD,pO)),2))));
            // 위 식과 아래식의 계산은 같으나 위 식은 복잡하면서 각도의 작은 값을 리턴하는 것이 문제	
            //public static double Angle_Vector_Pre(double pS, double pO, double pD)				{return ((float)(Angle_Vector_From_Base(pO,pD))-(float)(Angle_Vector_From_Base(pO,pS)));}
            //public static double Angle_Vector(double pS, double pO, double pD)					{return (Angle_Vector_Pre(pS,pO,pD) < 0)? 360.0+Angle_Vector_Pre(pS,pO,pD):Angle_Vector_Pre(pS,pO,pD);}
            // 0도(화면상의 우측)를 기준으로 pD가 회전한 값(시계방향이 (+) 임)을 리턴
            //public static double Angle_Vector_From_Base(double pBase, double pD)	{return aCos2((float)((pD).x-(pBase).x),(float)((pD).y-(pBase).y));}
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
#endif
            public static double ATan_Calc(double t) { return (double)(Math.Atan((double)(t)) * 180.0 / _PI); }
            public static double ATan(double t) { return ((ATan_Calc((t)) < 0) ? ATan_Calc((t)) + 180.0 : ATan_Calc((t))); }
            /////////////////////// ( Top Number(denominator) , Bottom Number(numerator) ) ///////////
            public static double ATan2(double x_Down_1, double y_Up_1) { return ((double)(x_Down_1) < 0 && (double)(y_Up_1) == 0) ? 180.0 : (((double)(y_Up_1) < 0) ? (ATan((double)(y_Up_1) / (double)(((double)(x_Down_1) == 0) ? _ZERO : (double)(x_Down_1))) + 180.0) : (ATan((double)(y_Up_1) / (double)(((double)(x_Down_1) == 0) ? _ZERO : (double)(x_Down_1))))); }
            #endregion sin/cos/tan function with degree value
#if false
            // 역방향 사상을 위한 원 이미지의 포인트 계산
            //public static double GetSourceForRotated_X(x, y, t, nSourceHeight, Center_x, Center_y)	(int)Round(((y) - (Center_y)) * Sin((t)) + ((x) - (Center_x)) * Cos((t)) + (Center_x));
            //public static double GetSourceForRotated_Y(x, y, t, nSourceHeight, Center_x, Center_y)	(int)Round(((y) - (Center_y)) * Cos((t)) - ((x) - (Center_x)) * Sin((t)) + (Center_y));
            //public static double GetDestForRotated_X(x, y, t, Center_x, Center_y)	(int)Round(((x) - (Center_x)) * Cos((t)) - ((y) - (Center_y)) * Sin((t))) + (Center_x);
            //public static double GetDestForRotated_Y(x, y, t, Center_x, Center_y)	(int)Round(((x) - (Center_x)) * Sin((t)) + ((y) - (Center_y)) * Cos((t))) + (Center_y);
            // 회전 결과영상 크기 계산
            //public static double GetRotatedImageHeight(t, nSourceWidth, nSourceHeight)	((nSourceHeight) * Cos(180.0 - (t)) + (nSourceWidth) * Cos((t)))
            //public static double GetRotatedImageWidth(t, nSourceWidth, nSourceHeight)	((nSourceHeight) * Cos((t)) + (nSourceWidth) * Cos(180.0 - (t)))
            //////////////////
#endif

            public static double Distance(double x, double y) { return (double)Math.Sqrt((double)(x) * (double)(x) + (double)(y) * (double)(y)); }
            public static double VectorNorm(double x, double y) { return (double)Math.Sqrt((double)(x) * (double)(x) + (double)(y) * (double)(y)); }
            public static double Norm(double x, double y) { return Distance(x, y); }

            //public static double Dist_Point2D(Point x1, Point x2) {return (double)Math.Sqrt(pow(((double)(x1.x)-(double)(x2.x)),2) + pow(((double)(x1.y)-(double)(x2.y)),2));}
            //public static double Dist_Point2D_Non_SQRT_Non_Float(Point x1, Point x2)	{return (((x1.x-x2.x)*(x1.x-x2.x)) +   ((x1.y-x2.y)*(x1.y-x2.y)));}
            public static double Pow(double x) { return (x * x); }
            public static double Pow(double x, double s) { return (double)Math.Pow(x, s); }

#if false
            // LOG Mask를 구하기 위한 계산(입력값은 반드시 float 이나 double 로 할것. 아님 계산값이 이상해짐
            //public static double CalcForMask(Sigma)		(int)(3.35*(double)(Sigma)+3.35*(double)(Sigma)+1.66)
            //public static double CalcMaskSize(Sigma)			(CalcForMask((double)(Sigma))%2==0)?CalcForMask((double)(Sigma))-1:CalcForMask((double)(Sigma))
            //public static double CalculateLoG(x,y,Sigma)	(double)(1.0/(double)(_PI*(double)(Sigma)*(double)(Sigma)*(double)(Sigma)*(double)(Sigma)))*(double)(1.0-((double)((double)(x)*(double)(x)+(double)(y)*(double)(y))/(double)(2.0*(double)(Sigma)*(double)(Sigma)))) * exp((double)-1.0*((double)((double)(x)*(double)(x)+(double)(y)*(double)(y))/(double)(2.0*(double)(Sigma)*(double)(Sigma))))
#endif
            #region get distance with 2 lines

            public static double Dot(SVector3D_t u, SVector3D_t v) { return (u.x * v.x + u.y * v.y + u.z * v.z); }  // DotMatrix with 2 vetors
            public static double Norm(SVector3D_t v) { return (double)Math.Sqrt(Dot(v, v)); }
            public static double Distance(SVector3D_t u, SVector3D_t v) { return Norm(u - v); } // distance with 2 points

#if false
            //#define dot(u,v)    ((u).x * (v).x + (u).y * (v).y + (u).z * (v).z) 
            //#define norm(v)    sqrt( dot(v,v) )                                        
            //#define d(u,v)       norm(u-v)                          // 두 점 사이 크기
            //#define abs(x)     ((x) >= 0 ? (x) : -(x))           // 절대값 구하기

            // 두 라인 사이의 거리 구하기
            float dist3D_Line_to_Line(Line L1, Line L2)
            {
                Vector u = L1.P1 - L1.P0;
                Vector v = L2.P1 - L2.P0;
                Vector w = L1.P0 - L2.P0;

                float a = dot(u, u);        // 항상 >= 0
                float b = dot(u, v);
                float c = dot(v, v);          // 항상 >= 0
                float d = dot(u, w);
                float e = dot(v, w);
                float D = a * c - b * b;       // 항상 >= 0
                float sc, tc;

                // 이렇게 작은 값보다 작으면 거의 평행이라고 본다
                if (D < SMALL_NUM)
                {
                    sc = 0.0;
                    tc = (b > c ? d / b : e / c);   // 큰 값을 취함
                }
                else
                {
                    sc = (b * e - c * d) / D;
                    tc = (a * e - b * d) / D;
                }

                // 두 점 사이 크기    // = L1(sc) - L2(tc)
                Vector dP = w + (sc * u) - (tc * v);

                return norm(dP);   // 두 점 사이 거리
            } 
#endif
            #endregion get distance with 2 lines

            #region Math - matrix function, making a DH-T matrix function
            // return false when it has error - Diagonal matrix
            public static bool CalcMatrix(int nLine, double[,] adS0, double[,] adS1, out double[,] adRes)
            {
                adRes = new double[nLine, nLine];                
                if ((adS0.Length < nLine * nLine) || (adS1.Length < nLine * nLine)) return false;
                for (int i = 0; i < nLine; i++)
                    for (int j = 0; j < nLine; j++)
                    {
                        adRes[i, j] = 0.0;
                        for (int k = 0; k < nLine; k++)
                            adRes[i, j] = adRes[i, j] + adS0[i, k] * adS1[k, j];
                    }
                return true;
            }

            public static bool CalcMatrix_Str(int nLine, String[,] aStrS0, String[,] aStrS1, out String[,] aStrRes)
            {
                aStrRes = new String[nLine, nLine];
                if ((aStrS0.Length < nLine * nLine) || (aStrS1.Length < nLine * nLine)) return false;
                String strTmp0, strTmp1, strTmp;
                for (int i = 0; i < nLine; i++)
                    for (int j = 0; j < nLine; j++)
                    {
                        aStrRes[i, j] = "";
                        for (int k = 0; k < nLine; k++)
                        {
#if true
                            strTmp = String.Empty;
                            strTmp0 = aStrS0[i, k];
                            strTmp1 = aStrS1[k, j];
                            if ((CConvert.IsDigit(strTmp0) == true) && (CConvert.IsDigit(strTmp1) == true))
                            {
                                strTmp = CConvert.FloatToStr(CConvert.StrToFloat(strTmp0) * CConvert.StrToFloat(strTmp1));
                            }
                            else if (strTmp0 == "1") strTmp = strTmp1;
                            else if (strTmp1 == "1") strTmp = strTmp0;
                            else if (strTmp0 == "0") strTmp = "0";
                            else if (strTmp1 == "0") strTmp = "0";
                            else                     strTmp = strTmp0 + "*" + strTmp1;

                            //if ((aStrRes[i, j].Length == 0) || (aStrRes[i, j] == "0")) aStrRes[i, j] = (String)strTmp.Clone();
                            if (aStrRes[i, j].Length == 0) aStrRes[i, j] = (String)strTmp.Clone();
                            else
                            {
                                if (aStrRes[i, j] == "0") aStrRes[i, j] = (String)strTmp.Clone();
                                else
                                    aStrRes[i, j] += ((strTmp == "0") ? "" : (((strTmp.IndexOf('-') == 0) ? "" : "+") + strTmp));
                            }
#else
                            //strTmp = String.Empty;
                            //aStrRes[i, j] += (((aStrRes[i, j].Length > 0) && (aStrS0[i, k].Length > 0) && (aStrS1[k, j].Length > 0)) ? "+" : "");
                            strTmp = (((aStrRes[i, j].Length > 0) && (aStrS0[i, k].Length > 0) && (aStrS1[k, j].Length > 0)) ? "+" : "");
                            //strTmp0 = aStrRes[i, j].IndexOf('+)
                            if (
                                (aStrS0[i, k].Length > 0) &&
                                (aStrS0[i, k] != "0") &&
                                (aStrS1[k, j].Length > 0) &&
                                (aStrS1[k, j] != "0")
                                )
                            {
                                if (aStrS0[i, k] == "1") strTmp += aStrS1[k, j];
                                else if (aStrS0[i, k] == "0") strTmp += "0";
                                else
                                {
                                    if ((CConvert.IsDigit(aStrS0[i, k]) == true) && (CConvert.IsDigit(aStrS1[k, j]) == true))
                                    {
                                        strTmp += CConvert.FloatToStr(CConvert.StrToFloat(aStrS0[i, k]) * CConvert.StrToFloat(aStrS1[k, j]));
                                    }
                                    else
                                        strTmp += aStrS0[i, k] + "*" + aStrS1[k, j];
                                }
                                aStrRes[i, j] += strTmp;
                            }
                            //aStrRes[i, j] += ((j < nLine - 1) ? "," : "");
#endif
                        }
                    }
                return true;
            }

            // DH- making a T matrix.
            public static bool CalcT(double dA, double dAlpha, double dD, double dTheta, out double[,] adT)
            {
                adT = new double[4, 4];
                int i;
                i = 0; adT[i, 0] = (double)Cos(dTheta); adT[i, 1] = -(double)Sin(dTheta) * (double)Cos(dAlpha); adT[i, 2] = (double)Sin(dTheta) * (double)Sin(dAlpha); adT[i, 3] = dA * (double)Cos(dTheta);
                i = 1; adT[i, 0] = (double)Sin(dTheta); adT[i, 1] = (double)Cos(dTheta) * (double)Cos(dAlpha); adT[i, 2] = -(double)Cos(dTheta) * (double)Sin(dAlpha); adT[i, 3] = dA * (double)Sin(dTheta);
                i = 2; adT[i, 0] = 0.0; adT[i, 1] = (double)Sin(dAlpha); adT[i, 2] = (double)Cos(dAlpha); adT[i, 3] = dD;
                i = 3; adT[i, 0] = 0.0; adT[i, 1] = 0.0; adT[i, 2] = 0.0; adT[i, 3] = 1.0;
                return true;
            }
            public static bool CalcT_Str(double dA, double dAlpha, double dD, double dTheta, String strMotName, out String[,] astrT)
            {
                astrT = new String[4, 4];
                int i;
                if (strMotName == null) strMotName = "";
#if false
                String Ct = "C(" + ((strMotName.Length > 0) ? strMotName + ((dTheta >= 0) ? "+" : "") + CConvert.DoubleToStr(dTheta) : CConvert.DoubleToStr(dTheta)) + ")";
                String Ca = "C(" + CConvert.DoubleToStr(dAlpha) + ")";
                String St = "S(" + ((strMotName.Length > 0) ? strMotName + ((dTheta >= 0) ? "+" : "") + CConvert.DoubleToStr(dTheta) : CConvert.DoubleToStr(dTheta)) + ")";
                String Sa = "S(" + CConvert.DoubleToStr(dAlpha) + ")";
#else
                String Ct = "C(" + ((strMotName.Length > 0) ? strMotName + ((dTheta == 0) ? "" : ((dTheta >= 0) ? "+" : "") + CConvert.DoubleToStr(dTheta)) : CConvert.DoubleToStr(dTheta)) + ")";
                String Ca = "C(" + CConvert.DoubleToStr(dAlpha) + ")";
                String St = "S(" + ((strMotName.Length > 0) ? strMotName + ((dTheta == 0) ? "" : ((dTheta >= 0) ? "+" : "") + CConvert.DoubleToStr(dTheta)) : CConvert.DoubleToStr(dTheta)) + ")";
                String Sa = "S(" + CConvert.DoubleToStr(dAlpha) + ")";                
#endif
                if (Ct == "C(0)") Ct = "1";
                else if ((Ct == "C(90)") || (Ct == "C(-90)")) Ct = "0";
                if (Ca == "C(0)") Ca = "1";
                else if ((Ca == "C(90)") || (Ca == "C(-90)")) Ca = "0";

                if (St == "S(0)") St = "0";
                else if (St == "S(90)") St = "1";
                else if (St == "S(-90)") St = "-1";
                if (Sa == "S(0)") Sa = "0";
                else if (Sa == "S(90)") Sa = "1";
                else if (Sa == "S(-90)") Sa = "-1";

                i = 0;
                // -- astrT[i, 0] = "(" + Ct + ")"; astrT[i, 1] = "(-" + St + "*" + Ca + ")"; astrT[i, 2] = "(" + St + "*" + Sa + ")"; astrT[i, 3] = "(" + CConvert.DoubleToStr(a) + "*" + Ct + ")"; --//
                astrT[i, 0] = Ct;
                if (Ct == "0") astrT[i, 0] = "0";
                else if (Ct == "1") astrT[i, 0] = "1";
                //astrT[i, 1] = "(-" + St + "*" + Ca + ")";
                astrT[i, 1] = "-" + ((St != "1") ? St : "") + (((St != "1") && (Ca != "1")) ? "*" : "") + ((Ca != "1") ? Ca : "");
                if ((St == "0") || (Ca == "0")) astrT[i, 1] = "0";
                else if (
                            ((St == "1") && (Ca == "1")) ||
                            ((St == "-1") && (Ca == "-1"))
                    ) astrT[i, 1] = "-1";
                else if (
                            ((St == "-1") && (Ca == "1")) ||
                            ((St == "1") && (Ca == "-1"))
                    ) astrT[i, 1] = "1";
                //astrT[i, 2] = "(" + St + "*" + Sa + ")"; 
                astrT[i, 2] = ((St != "1") ? St : "") + (((St != "1") && (Sa != "1")) ? "*" : "") + ((Sa != "1") ? Sa : "");
                if ((St == "0") || (Sa == "0")) astrT[i, 2] = "0";
                else if (
                            ((St == "1") && (Sa == "1")) ||
                            ((St == "-1") && (Sa == "-1"))
                    ) astrT[i, 2] = "1";
                else if (
                            ((St == "-1") && (Sa == "1")) ||
                            ((St == "1") && (Sa == "-1"))
                    ) astrT[i, 2] = "-1";
                //astrT[i, 3] = "(" + CConvert.DoubleToStr(a) + "*" + Ct + ")";
                astrT[i, 3] = ((dA != 1) ? CConvert.DoubleToStr(dA) : "") + (((dA != 1) && (Ct != "1")) ? "*" : "") + ((Ct != "1") ? Ct : "");
                if ((dA == 0) || (Ct == "0")) astrT[i, 3] = "0";
                else if (
                            ((dA == 1) && (Ct == "1")) ||
                            ((dA == -1) && (Ct == "-1"))
                    ) astrT[i, 3] = "1";
                else if (
                            ((dA == -1) && (Ct == "1")) ||
                            ((dA == 1) && (Ct == "-1"))
                        ) astrT[i, 3] = "-1";

                i = 1;
                //-- astrT[i, 0] = "(" + St + ")"; astrT[i, 1] = "(" + Ct + "*" + Ca + ")"; astrT[i, 2] = "(-" + Ct + "*" + Sa + ")"; astrT[i, 3] = "(" + CConvert.DoubleToStr(a) + "*" + St + ")"; --//
                astrT[i, 0] = St;
                if (St == "0") astrT[i, 0] = "0";
                else if (St == "1") astrT[i, 0] = "1";

                //astrT[i, 1] = "(" + Ct + "*" + Ca + ")";
                astrT[i, 1] = ((Ct != "1") ? Ct : "") + (((Ct != "1") && (Ca != "1")) ? "*" : "") + ((Ca != "1") ? Ca : "");
                if ((Ct == "0") || (Ca == "0")) astrT[i, 1] = "0";
                else if (
                            ((Ct == "1") && (Ca == "1")) ||
                            ((Ct == "-1") && (Ca == "-1"))
                    ) astrT[i, 1] = "1";
                else if (
                            ((Ct == "-1") && (Ca == "1")) ||
                            ((Ct == "1") && (Ca == "-1"))
                    ) astrT[i, 1] = "-1";
                //astrT[i, 2] = "(-" + Ct + "*" + Sa + ")"; 
                astrT[i, 2] = "-" + ((Ct != "1") ? Ct : "") + (((Ct != "1") && (Sa != "1")) ? "*" : "") + ((Sa != "1") ? Sa : "");
                if ((Ct == "0") || (Sa == "0")) astrT[i, 2] = "0";
                else if (
                            ((Ct == "1") && (Sa == "1")) ||
                            ((Ct == "-1") && (Sa == "-1"))
                    ) astrT[i, 2] = "-1";
                else if (
                            ((Ct == "-1") && (Sa == "1")) ||
                            ((Ct == "1") && (Sa == "-1"))
                    ) astrT[i, 2] = "1";
                //astrT[i, 3] = "(" + CConvert.DoubleToStr(a) + "*" + St + ")";
                astrT[i, 3] = ((dA != 1) ? CConvert.DoubleToStr(dA) : "") + (((dA != 1) && (St != "1")) ? "*" : "") + ((St != "1") ? St : "");
                if ((dA == 0) || (St == "0")) astrT[i, 3] = "0";
                else if (
                                ((dA == 1) && (St == "1")) ||
                                ((dA == -1) && (St == "-1"))
                        ) astrT[i, 3] = "1";
                else if (
                           ((dA == -1) && (St == "1")) ||
                           ((dA == 1) && (St == "-1"))
                        ) astrT[i, 3] = "-1";
                i = 2;
                //-- astrT[i, 0] = "0"; astrT[i, 1] = "(" + Sa + ")"; astrT[i, 2] = "(" + Ca + ")"; astrT[i, 3] = "(" + CConvert.DoubleToStr(d) + ")"; --//
                astrT[i, 0] = "0";
                //astrT[i, 1] = "(" + Sa + ")";
                astrT[i, 1] = ((Sa != "1") ? Sa : "");
                if (Sa == "0") astrT[i, 1] = "0";
                else if (Sa == "1") astrT[i, 1] = "1";
                //astrT[i, 2] = "(" + Ca + ")";
                astrT[i, 2] = ((Ca != "1") ? Ca : "");
                if (Ca == "0") astrT[i, 2] = "0";
                else if (Ca == "1") astrT[i, 2] = "1";
                //astrT[i, 3] = "(" + CConvert.DoubleToStr(d) + ")";

                //// dD
                astrT[i, 3] = ((dD != 1) ? CConvert.DoubleToStr(dD) : "");
                if (dD == 0) astrT[i, 3] = "0";
                else if (dD == 1) astrT[i, 3] = "1";
                //
                //((strMotName.Length > 0) ? strMotName + ((dTheta == 0) ? "" : ((dTheta >= 0) ? "+" : "") + CConvert.DoubleToStr(dTheta)) : CConvert.DoubleToStr(dTheta));
                // D
                //if (DhParam.nAxisDir >= 2) astrT[i, 3] += (((DhParam.nAxisNum >= 0) ? dAngleData : 0) * ((DhParam.nAxisDir == 2) ? 1.0f : -1.0f));


                i = 3; astrT[i, 0] = "0"; astrT[i, 1] = "0"; astrT[i, 2] = "0"; astrT[i, 3] = "1";
                return true;
            }

            // making a rotation matrix.(4 by 4)
            public static bool CalcRot(double dAngleX, double dAngleY, double dAngleZ, double[,] adSrc, out double[,] adRot)
            {
                double[,] adCalcX = new double[4, 4];
                double[,] adCalcY = new double[4, 4];
                double[,] adCalcZ = new double[4, 4];
                adRot = new double[4, 4];
                int i;

                // rotation by axis(x)
                i = 0; adCalcX[i, 0] = 1.0; adCalcX[i, 1] = 0.0; adCalcX[i, 2] = 0.0; adCalcX[i, 3] = 0.0;
                i = 1; adCalcX[i, 0] = 0.0; adCalcX[i, 1] = (double)Cos(dAngleX); adCalcX[i, 2] = -(double)Sin(dAngleX); adCalcX[i, 3] = 0.0;
                i = 2; adCalcX[i, 0] = 0.0; adCalcX[i, 1] = (double)Sin(dAngleX); adCalcX[i, 2] = (double)Cos(dAngleX); adCalcX[i, 3] = 0.0;
                i = 3; adCalcX[i, 0] = 0.0; adCalcX[i, 1] = 0.0; adCalcX[i, 2] = 0.0; adCalcX[i, 3] = 1.0;

                // rotation by axis(y)
                i = 0; adCalcY[i, 0] = (double)Cos(dAngleY); adCalcY[i, 1] = 0.0; adCalcY[i, 2] = (double)Sin(dAngleY); adCalcY[i, 3] = 0.0;
                i = 1; adCalcY[i, 0] = 0.0; adCalcY[i, 1] = 1.0; adCalcY[i, 2] = 0.0; adCalcY[i, 3] = 0.0;
                i = 2; adCalcY[i, 0] = -(double)Sin(dAngleY); adCalcY[i, 1] = 0.0; adCalcY[i, 2] = (double)Cos(dAngleY); adCalcY[i, 3] = 0.0;
                i = 3; adCalcY[i, 0] = 0.0; adCalcY[i, 1] = 0.0; adCalcY[i, 2] = 0.0; adCalcY[i, 3] = 1.0;

                // rotation by axis(z)
                i = 0; adCalcZ[i, 0] = (double)Cos(dAngleY); adCalcZ[i, 1] = -(double)Sin(dAngleY); adCalcZ[i, 2] = 0.0; adCalcZ[i, 3] = 0.0;
                i = 1; adCalcZ[i, 0] = (double)Sin(dAngleY); adCalcZ[i, 1] = (double)Cos(dAngleY); adCalcZ[i, 2] = 0.0; adCalcZ[i, 3] = 0.0;
                i = 2; adCalcZ[i, 0] = 0.0; adCalcZ[i, 1] = 0.0; adCalcZ[i, 2] = 1.0; adCalcZ[i, 3] = 0.0;
                i = 3; adCalcZ[i, 0] = 0.0; adCalcZ[i, 1] = 0.0; adCalcZ[i, 2] = 0.0; adCalcZ[i, 3] = 1.0;


                CalcMatrix(4, adSrc, adCalcX, out adRot);
                CalcMatrix(4, adRot, adCalcY, out adRot);
                CalcMatrix(4, adRot, adCalcZ, out adRot);

                adCalcX = null;
                adCalcY = null;
                adCalcZ = null;
                return true;
            }
            // rotate cordination with rotation matrix
            public static bool CalcRot(double dAngleX, double dAngleY, double dAngleZ, ref double dX, ref double dY, ref double dZ)
            {
                double[,] adCalc = new double[4, 4];
                int i;

                double[,] adSrc = new double[4, 4];
                MakeMatrix(dX, dY, dZ, out adSrc); // make a matrix with X,Y,Z position

                if (dAngleX != 0)
                {
                    // rotation by axis(X)
                    i = 0; adCalc[i, 0] = 1.0; adCalc[i, 1] = 0.0; adCalc[i, 2] = 0.0; adCalc[i, 3] = 0.0;
                    i = 1; adCalc[i, 0] = 0.0; adCalc[i, 1] = (double)Cos(dAngleX); adCalc[i, 2] = -(double)Sin(dAngleX); adCalc[i, 3] = 0.0;
                    i = 2; adCalc[i, 0] = 0.0; adCalc[i, 1] = (double)Sin(dAngleX); adCalc[i, 2] = (double)Cos(dAngleX); adCalc[i, 3] = 0.0;
                    i = 3; adCalc[i, 0] = 0.0; adCalc[i, 1] = 0.0; adCalc[i, 2] = 0.0; adCalc[i, 3] = 1.0;
                    CalcMatrix(4, adCalc, adSrc, out adSrc);
                }

                if (dAngleY != 0)
                {
                    // rotation by axis(Y)
                    i = 0; adCalc[i, 0] = (double)Cos(dAngleY); adCalc[i, 1] = 0.0; adCalc[i, 2] = (double)Sin(dAngleY); adCalc[i, 3] = 0.0;
                    i = 1; adCalc[i, 0] = 0.0; adCalc[i, 1] = 1.0; adCalc[i, 2] = 0.0; adCalc[i, 3] = 0.0;
                    i = 2; adCalc[i, 0] = -(double)Sin(dAngleY); adCalc[i, 1] = 0.0; adCalc[i, 2] = (double)Cos(dAngleY); adCalc[i, 3] = 0.0;
                    i = 3; adCalc[i, 0] = 0.0; adCalc[i, 1] = 0.0; adCalc[i, 2] = 0.0; adCalc[i, 3] = 1.0;
                    CalcMatrix(4, adCalc, adSrc, out adSrc);
                }

                if (dAngleZ != 0)
                {
                    // rotation by axis(Z)
                    i = 0; adCalc[i, 0] = (double)Cos(dAngleY); adCalc[i, 1] = -(double)Sin(dAngleY); adCalc[i, 2] = 0.0; adCalc[i, 3] = 0.0;
                    i = 1; adCalc[i, 0] = (double)Sin(dAngleY); adCalc[i, 1] = (double)Cos(dAngleY); adCalc[i, 2] = 0.0; adCalc[i, 3] = 0.0;
                    i = 2; adCalc[i, 0] = 0.0; adCalc[i, 1] = 0.0; adCalc[i, 2] = 1.0; adCalc[i, 3] = 0.0;
                    i = 3; adCalc[i, 0] = 0.0; adCalc[i, 1] = 0.0; adCalc[i, 2] = 0.0; adCalc[i, 3] = 1.0;
                    CalcMatrix(4, adCalc, adSrc, out adSrc);
                }

                dX = adSrc[0, 3];
                dY = adSrc[1, 3];
                dZ = adSrc[2, 3];

                adCalc = null;
                adSrc = null;
                return true;
            }
            public static bool MakeMatrix_Rot(double dAngleX, double dAngleY, double dAngleZ, out double[,] adRot)
            {
                double[,] adCalcI = new double[4, 4];
                int i;
                // a unit matrix
                i = 0; adCalcI[i, 0] = 1.0; adCalcI[i, 1] = 0.0; adCalcI[i, 2] = 0.0; adCalcI[i, 3] = 0.0;
                i = 1; adCalcI[i, 0] = 0.0; adCalcI[i, 1] = 1.0; adCalcI[i, 2] = 0.0; adCalcI[i, 3] = 0.0;
                i = 2; adCalcI[i, 0] = 0.0; adCalcI[i, 1] = 0.0; adCalcI[i, 2] = 1.0; adCalcI[i, 3] = 0.0;
                i = 3; adCalcI[i, 0] = 0.0; adCalcI[i, 1] = 0.0; adCalcI[i, 2] = 0.0; adCalcI[i, 3] = 1.0;

                CalcRot(dAngleX, dAngleY, dAngleZ, adCalcI, out adRot);

                adCalcI = null;
                return true;
            }
            public static bool MakeMatrix(double dX, double dY, double dZ, out double[,] adMatrix)
            {
                adMatrix = new double[4, 4];
                int i;
                // a unit matrix
                i = 0; adMatrix[i, 0] = 1.0; adMatrix[i, 1] = 0.0; adMatrix[i, 2] = 0.0; adMatrix[i, 3] = dX;
                i = 1; adMatrix[i, 0] = 0.0; adMatrix[i, 1] = 1.0; adMatrix[i, 2] = 0.0; adMatrix[i, 3] = dY;
                i = 2; adMatrix[i, 0] = 0.0; adMatrix[i, 1] = 0.0; adMatrix[i, 2] = 1.0; adMatrix[i, 3] = dZ;
                i = 3; adMatrix[i, 0] = 0.0; adMatrix[i, 1] = 0.0; adMatrix[i, 2] = 0.0; adMatrix[i, 3] = 1.0;

                return true;
            }

            // comparing matrix, == true; != false; but it checks only thousandths(.000)(Kor: 소수점 3째자리까지 허용)
            public static bool CompareMatrix(int nLine, double[,] adSrc0, double[,] adSrc1)
            {
                bool bRet = true;
                #region to do list
#if false
                // 랭크가 맞지 않는 경우의 에러처리 나중에 필요. - 현재는 귀찮아서 패스~~
#endif
                #endregion to do list
                try
                {
                    //int nPoint = 3;
                    for (int i = 0; i < nLine; i++)
                    {
                        for (int j = 0; j < nLine; j++)
                        {
                            //if ((double)Math.Round(adSrc0[i, j], nPoint) != (double)Math.Round(adSrc1[i, j], nPoint))
                            if (((adSrc1[i, j] - adSrc0[i, j]) < -0.001) || ((adSrc0[i, j] - adSrc1[i, j]) > 0.001))
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
#if false
            // 연산이상이 발생하면 false 를 내보냄 - 정방행렬
            //public static bool CalcMatrix(int nLine, double[,] adS0, double[,] adS1, out double[,] adRes)
            //{
            //    //bool bRet = true;
            //    adRes = new double[nLine, nLine];
            //    if ((afS0.Length < nLine * nLine) || (afS1.Length < nLine * nLine)) return false;
            //    for (int i = 0; i < nLine; i++)
            //        for (int j = 0; j < nLine; j++)
            //        {
            //            adRes[i, j] = 0.0;
            //            for (int k = 0; k < nLine; k++)
            //                adRes[i, j] = adRes[i, j] + adS0[i, k] * adS1[k, j];

            //            adRes[h][i] = 0;
            //            for (j = 1; j <= k; j++)
            //                C[h][i] = C[h][i] + A[h][j] * B[j][i];

            //        }
            //    return true;
            //}
#endif
            #endregion Math - matrix function, making a DH-T matrix function

            #region Speed(RPM)
            public static float CalcRps(float fDeltaAngle, float fMillisecond)
            {
                if (fDeltaAngle == 0) fDeltaAngle = (float)CMath._ZERO;
                // _MAX_RPM * 360 : 60 seconds => 1024(_MAX_EV_RPM) 
                // =>  rotate [6 * 117.185(_MAX_RPM)] degree during 1 second, 1ms => 117.185 * 360 degrees / 60000ms = 0.70311 degree
                // => it needs 60000 / (117.185(_MAX_RPM) * 360) = 1.422252564 ms for 1 degree moving

                // moving time for 1 degree => fTime / fDeltaAngle
                // moving time per moving degree => moving time during 1 degree * fDeltaAngle

                // rpm = 60000/([moving time for 1 degree]*360)
                #region Kor
                // _MAX_RPM * 360 : 60 seconds => 1024(_MAX_EV_RPM) 일때 
                // => 1초간 6 * 117.185(_MAX_RPM) 도 회전, 1ms => 117.185 * 360도 / 60000ms = 0.70311 도 이동
                // => 1도 움직이는데 60000 / (117.185(_MAX_RPM) * 360) = 1.422252564 ms 가 필요

                // 1도 이동시간 => 60000 / (Rpm * 360)
                // 이동각도 당 이동시간 계산 => 1도 이동시간 * fDeltaAngle

                // 1도 이동시간 => fTime / fDeltaAngle
                #endregion Kor
                return Math.Abs(60000.0f / (fMillisecond / fDeltaAngle * 360.0f));
            }
            public static float CalcTime(float fDeltaAngle, float fRps)
            {
                //return 60000.0f * fDeltaAngle / (fRps * 360.0f);
                return Math.Abs(6000.0f * fDeltaAngle / (fRps * 36.0f)) * 1000.0f;
            }
            #endregion Speed(RPM)

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion Math Function
#if false
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            // 교차점 구하는 함수
            #region 교차점 구하는 함수 - 직선 : 직선
        public static bool CalcCrossPoint(SLine_t SLineFirst, SLine_t SLineSecond, out double dX, out double dY)
        {
            return CalcCrossPoint(SLineFirst.dX0, SLineFirst.dY0, SLineFirst.dX1, SLineFirst.dY1, SLineSecond.dX0, SLineSecond.dY0, SLineSecond.dX1, SLineSecond.dY1, out dX, out dY);
        }
        public static bool CalcCrossPoint(double dSx0, double dSy0, double dSx1, double dSy1, double dDx0, double dDy0, double dDx1, double dDy1, out double dX, out double dY)
        {
            dX = dY = 0;

            double t, s;

            double under = (dDy1 - dDy0) * (dSx1 - dSx0) - (dDx1 - dDx0) * (dSy1 - dSy0); 
	        if(under==0) return false;

            double _t = (dDx1 - dDx0) * (dSy0 - dDy0) - (dDy1 - dDy0) * (dSx0 - dDx0);
            double _s = (dSx1 - dSx0) * (dSy0 - dDy0) - (dSy1 - dSy0) * (dSx0 - dDx0);
            
            t = _t/under; 
	        s = _s/under; 
 
            // 교차점이 있는 점들인지 검사
	        if(t<0.0 || t>1.0 || s<0.0 || s>1.0) return false;
 	        if(_t==0 && _s==0) return false;

            dX = (dSx0 + t * (double)(dSx1 - dSx0));
            dY = (dSy0 + t * (double)(dSy1 - dSy0));

            return true;
        }
        #endregion 교차점 구하는 함수 - 직선 : 직선

            #region 라인분할함수
        public int CalcLine(SLine_t SLine_First, SLine_t SLine_Second, out SLine_t[] pSLine_Second)
        {
            pSLine_Second = new SLine_t[1];
            pSLine_Second[0].dX0 = SLine_First.dX0;
            pSLine_Second[0].dY0 = SLine_First.dY0;
            pSLine_Second[0].dX1 = SLine_First.dX1;
            pSLine_Second[0].dY1 = SLine_First.dY1;

            double[] dX = new double[4] { 0.0, 0.0, 0.0, 0.0 };
            double[] dY = new double[4] { 0.0, 0.0, 0.0, 0.0 };
            bool bLine0 = false;
            bool bLine1 = false;
            bool bOk = CalcSeparationPoint(
                                            5,
                                            SLine_First.dX0, SLine_First.dY0, SLine_First.dX1, SLine_First.dY1,
                                            SLine_Second.dX0, SLine_Second.dY0, SLine_Second.dX1, SLine_Second.dY1,
                                            ref bLine0, ref dX[0], ref dY[0], ref dX[1], ref dY[1],
                                            ref bLine1, ref dX[2], ref dY[2], ref dX[3], ref dY[3]);
            if (bOk == true)
            {
                int i = 1;

                if (bLine0 == true)
                {                    
                    Array.Resize<SLine_t>(ref pSLine_Second, i + 1);
                    pSLine_Second[i].dX0 = dX[0];
                    pSLine_Second[i].dY0 = dY[0];
                    pSLine_Second[i].dX1 = dX[1];
                    pSLine_Second[i].dY1 = dY[1];
                    i++;
                }
                if (bLine1 == true)
                {
                    Array.Resize<SLine_t>(ref pSLine_Second, i + 1);
                    pSLine_Second[i].dX0 = dX[2];
                    pSLine_Second[i].dY0 = dY[2];
                    pSLine_Second[i].dX1 = dX[3];
                    pSLine_Second[i].dY1 = dY[3];
                    i++;
                }

                if (pSLine_Second != null) return pSLine_Second.Length;
            }
            return 0;
        }
        /*
        public int CalcLines(SLine_t[] pSLine_Src, out SLine_t[] pSLine_Result)
        {
            pSLine_Result = new SLine_t[1];
            try
            {
                if (pSLine_Src.Length <= 1)
                {
                    pSLine_Result = pSLine_Src;
                    return pSLine_Src.Length;
                }
                
                SLine_t [] pSLine = new SLine_t[1];
                for (int i = 1; i < pSLine_Src.Length; i++)
                {
                    CalcLine(pSLine_Src[i - 1], pSLine_Src[i], out pSLine);
                    Array.Resize<SLine_t>(ref pSLine_Result, pS
                }
                return pSLine_Result.Length;
            }
            catch
            {
                return 0;
            }
        }
        */
        #endregion 라인분할함수
        public static bool CalcSeparationPoint(double dPenWidth, SLine_t SFirst, SLine_t SDest,
                                                ref bool bFrontLine_Ok, ref SLine_t SDest_0,
                                                ref bool bRearLine_Ok, ref SLine_t SDest_1)
        {
            SDest_0.nType = 0;
            SDest_1.nType = 0;
            return CalcSeparationPoint(dPenWidth, SFirst.dX0, SFirst.dY0, SFirst.dX1, SFirst.dY1, SDest.dX0, SDest.dY0, SDest.dX1, SDest.dY1,
                                                ref bFrontLine_Ok, ref SDest_0.dX0, ref SDest_0.dY0, ref SDest_0.dX1, ref SDest_0.dY1,
                                                ref bRearLine_Ok, ref SDest_1.dX0, ref SDest_1.dY0, ref SDest_1.dX1, ref SDest_1.dY1);
        }
        public static bool CalcSeparationPoint(
                                                double dPenWidth,
                                                double dSx0, double dSy0, double dSx1, double dSy1, double dDx0, double dDy0, double dDx1, double dDy1,
                                                ref bool bFrontLine_Ok, ref double dFx0, ref double dFy0, ref double dFx1, ref double dFy1,
                                                ref bool bRearLine_Ok, ref double dRx0, ref double dRy0, ref double dRx1, ref double dRy1
            )
        {
            dFx0 = dFy0 = 0.0;
            dFx1 = dFy1 = 0.0;
            dRx0 = dRy0 = 0.0;
            dRx1 = dRy1 = 0.0;
            bFrontLine_Ok = true;
            bRearLine_Ok = true;
            double dCx, dCy;
            bool bRet = CalcCrossPoint(dSx0, dSy0, dSx1, dSy1, dDx0, dDy0, dDx1, dDy1, out dCx, out dCy);
            if (bRet == true)
            {
                double dAngle = aTan2((dDx1 - dDx0), (dDy1 - dDy0));
                double dCX_front = dCx + (dPenWidth * Cos(dAngle));
                double dCY_front = dCy + (dPenWidth * Sin(dAngle));
                double dCX_Rear = dCx - (dPenWidth * Cos(dAngle));
                double dCY_Rear = dCy - (dPenWidth * Sin(dAngle));


                double[] adX = new double[4];
                double[] adY = new double[4];
                //double[] adDist = new double[4];
                int i = 0, j;

                adX[i] = dDx0; adY[i] = dDy0; i++;
                //adDist[i++] = (double)Math.Sqrt(dDx0 * dDx0 + dDy0 * dDy0);
                adX[i] = dCX_front; adY[i] = dCY_front; i++;
                //adDist[i++] = (double)Math.Sqrt(dCX_front * dCX_front + dCY_front * dCY_front);
                adX[i] = dCX_Rear; adY[i] = dCY_Rear; i++;
                //adDist[i++] = (double)Math.Sqrt(dCX_Rear * dCX_Rear + dCY_Rear * dCY_Rear);
                adX[i] = dDx1; adY[i] = dDy1; i++;
                //adDist[i++] = (double)Math.Sqrt(dDx1 * dDx1 + dDy1 * dDy1);


                // bubble sorting
                for (i = 0; i < adX.Length; i++) // 배열길이 만큼 포 루프문 수행
                {
                    for (j = 1; j < adX.Length - i; j++) //초기값 1로 해서 (배열길이 - i) 만큼 
                    {
                        if (adX[j - 1] < adX[j]) // 초기값 1로 (배열길이-1) 만큼 j-1과 j 인덱스값 비교
                        // 후 j-1이 크면 값 교환                                                 
                        {
                            double temp_x = 0;
                            temp_x = adX[j - 1];
                            adX[j - 1] = adX[j];
                            adX[j] = temp_x;

                            double temp_y = 0;
                            temp_y = adY[j - 1];
                            adY[j - 1] = adY[j];
                            adY[j] = temp_y;
                        }
                    }
                }
                // 마지막에는 구한 좌표점이 해당 선분위에 있는 점인지를 반드시 체크(x만 비교해 보면 됨)
                double dMax_x = (dDx0 >= dDx1) ? dDx0 : dDx1;
                double dMin_x = (dDx0 < dDx1) ? dDx0 : dDx1;
                                
                // Front Line Check
                int nPos0 = 0;
                int nPos1 = 1;
                dFx0 = adX[nPos0]; dFy0 = adY[nPos0];
                dFx1 = adX[nPos1]; dFy1 = adY[nPos1];
                bool bOk = true;
                for (i = nPos0; i <= nPos1; i++) 
                    if ((adX[i] > dMax_x) || (adX[i] < dMin_x)) bOk = false;
                if (bOk == false) bFrontLine_Ok = false;
                else bFrontLine_Ok = true;

                // Rear Line Check
                nPos0 = 2;
                nPos1 = 3;
                dRx0 = adX[nPos0]; dRy0 = adY[nPos0];
                dRx1 = adX[nPos1]; dRy1 = adY[nPos1];
                bOk = true;
                for (i = nPos0; i <= nPos1; i++) 
                    if ((adX[i] > dMax_x) || (adX[i] < dMin_x)) bOk = false;
                if (bOk == false) bRearLine_Ok = false;
                else bRearLine_Ok = true;                
            }
            return bRet;
        }

        private static int m_nCnt_Item = 0;
        private static int[,] m_pnVirtualImage;
        private static double m_dScale = 1.0;
        public static void Item_Scale(double dScale)
        {
            m_dScale = dScale;
        }
        public static void Item_Init(int nW, int nH)
        {
            m_nCnt_Item = 0;
            nW = (int)Math.Round(nW * m_dScale);
            nH = (int)Math.Round(nH * m_dScale);
            m_pnVirtualImage = new int[nW, nH];
            Item_Clear();
        }
        public static void Item_Clear()
        {
            m_nCnt_Item = 0;
            m_pnVirtualImage.Initialize();
        }
        public static int Item_Add(SLine_t SLine)
        {
            try
            {
                double dW = (double)Math.Abs(SLine.dX1 - SLine.dX0);
                double dH = (double)Math.Abs(SLine.dY1 - SLine.dY0);
                int nMax = (int)Math.Round(((dW >= dH) ? dW : dH) * m_dScale);
                for (int i = 0; i < nMax; i++)
                {
                    int nX = (int)Math.Round((dW * (double)i / (double)nMax) * m_dScale);
                    int nY = (int)Math.Round((dH * (double)i / (double)nMax) * m_dScale);
                    m_pnVirtualImage[nX, nY] = m_nCnt_Item;
                }
                m_nCnt_Item++;
            }
            catch
            {
            }
            return m_nCnt_Item;
        }
        public static int[,] Item_Get() { return m_pnVirtualImage; }
        public static int Item_Get(int nX, int nY) { return m_pnVirtualImage[nX, nY]; }
        public static int Item_Get(double dX, double dY) { return m_pnVirtualImage[(int)Math.Round(dX * m_dScale), (int)Math.Round(dY * m_dScale)]; }
#endif
            #region Filter
            // Lowpass
            // y[i] := y[i-1] + α * (x[i] - y[i-1]) = α*x[i] + (1-α)*y[i-1]
            // High Pass
            // y[i] := α * (y[i-1] + x[i] - x[i-1])

            public static float LowPassFilter(float fAlpha, float fResult_Prev, float fCurrent_Value)
            {
                return (fAlpha * fCurrent_Value + (1 - fAlpha) * fResult_Prev);
            }
            public static double LowPassFilter_vd(double dAlpha, double dResult_Prev, double dCurrent_Value)
            {
                return (dAlpha * dCurrent_Value + (1 - dAlpha) * dResult_Prev);
            }
            public static float HighPassFilter(float fAlpha, float fResult_Prev, float fCurrent_Value, float fPrevious_Value)
            {
                return fAlpha * (fResult_Prev + fCurrent_Value - fPrevious_Value);
            }
            public static double HighPassFilter_vd(double dAlpha, double dResult_Prev, double dCurrent_Value, double dPrevious_Value)
            {
                return dAlpha * (dResult_Prev + dCurrent_Value - dPrevious_Value);
            }
            #region Kalman Filter
#if false
            public static double KalmanFilter(double dPrevious_Value, double dCurrent_Value, double dP, double dQ, double dNoise)
            {
                // F, H, R,, Q are constant

                //http://en.wikipedia.org/wiki/Kalman_filter
                //run kalman filtering
                //x_k = Ax_{k-1} + Bu_k + w_k
                //z_k = Hx_k+v_k
                //time update
                //x_k = Ax_{k-1} + Uu_k
                //P_k = AP_{k-1}A^T + Q
                //measurement update
                //K_k = P_k H^T(HP_kH^T + R)^T
                //x_k = x_k + K_k(z_k - Hx_k)
                //P_k = (I - K_kH)P_k

                double A = dPrevValue; //factor of real value to previous real value
                //double B = 0; //factor of real value to real control signal
                double H = dCurrent_Value; //factor of measured value to real value
                double P = double.Parse(textBoxP.Text);
                double Q = double.Parse(textBoxQ.Text);
                double R = double.Parse(textBoxR.Text); //environment noise
                double K;
                double z;
                double x;

                data2 = new List<double>();

                //assign to first measured value
                x = data[0];
                for (int i = 0; i < data.Count; i++)
                {
                    //get current measured value
                    z = data[i];

                    //time update - prediction
                    x = A * x;
                    P = A * P * A + Q;

                    //measurement update - correction
                    K = P * H / (H * P * H + R);
                    x = x + K * (z - H * x);
                    P = (1 - K * H) * P;

                    //estimated value
                    data2.Add(x);
                }

            }
#endif
            #endregion Kalman Filter

            #region Complementary Filter - from Pieter-jan.com
            // http://www.pieter-jan.com/node/11
            private const float _ACCELEROMETER_SENSITIVITY = 8192.0f;
            private const float _GYROSCOPE_SENSITIVITY = 65.536f;
            public static void ComplementaryFilter(short[] asAcc, short[] gyrData, ref float fRoll, ref float fPitch)
            {
                float fAcc_Pitch, fAcc_Roll;               
                float fDt = 0.01f;
                // Integrate the gyroscope data -> int(angularSpeed) = angle
                fPitch += ((float)gyrData[0] / _GYROSCOPE_SENSITIVITY) * fDt; // Angle around the X-axis
                fRoll -= ((float)gyrData[1] / _GYROSCOPE_SENSITIVITY) * fDt;    // Angle around the Y-axis
 
                // Compensate for drift with accelerometer data if !bullshit
                // Sensitivity = -2 to 2 G at 16Bit -> 2G = 32768 && 0.5G = 8192
                int forceMagnitudeApprox = (int)Math.Round((float)Math.Abs(asAcc[0]) + (float)Math.Abs(asAcc[1]) + (float)Math.Abs(asAcc[2]));
                if (forceMagnitudeApprox > 8192 && forceMagnitudeApprox < 32768)
                {
	                // Turning around the X axis results in a vector on the Y-axis
                    fAcc_Pitch = (float)Math.Atan2((float)asAcc[1], (float)asAcc[2]) * 180.0f / (float)Math.PI;
                    fPitch = fPitch * 0.98f + fAcc_Pitch * 0.02f;
 
	            // Turning around the Y axis results in a vector on the X-axis
                    fAcc_Roll = (float)Math.Atan2((float)asAcc[0], (float)asAcc[2]) * 180 / (float)Math.PI;
                    fRoll = fRoll * 0.98f + fAcc_Roll * 0.02f;
                }
            }
            //public static void ComplementaryFilter(short[] asAcc, short[] gyrData, ref float fRoll, ref float fPitch, ref float fYaw)
            //{
            //    float fAcc_Roll, fAcc_Pitch, fAcc_Yaw;
            //    float fDt = 0.01f;
            //    // Integrate the gyroscope data -> int(angularSpeed) = angle
            //    fPitch += ((float)gyrData[0] / _GYROSCOPE_SENSITIVITY) * fDt; // Angle around the X-axis
            //    fRoll -= ((float)gyrData[1] / _GYROSCOPE_SENSITIVITY) * fDt;    // Angle around the Y-axis
            //    fYaw += ((float)gyrData[2] / _GYROSCOPE_SENSITIVITY) * fDt;    // Angle around the Y-axis

            //    // Compensate for drift with accelerometer data if !bullshit
            //    // Sensitivity = -2 to 2 G at 16Bit -> 2G = 32768 && 0.5G = 8192
            //    int forceMagnitudeApprox = (int)Math.Round((float)Math.Abs(asAcc[0]) + (float)Math.Abs(asAcc[1]) + (float)Math.Abs(asAcc[2]));
            //    if (forceMagnitudeApprox > 8192 && forceMagnitudeApprox < 32768)
            //    {


            //        // Turning around the X axis results in a vector on the Y-axis
            //        fAcc_Pitch = (float)Math.Atan2((float)asAcc[1], (float)asAcc[2]) * 180.0f / (float)Math.PI;
            //        fPitch = fPitch * 0.98f + fAcc_Pitch * 0.02f;

            //        // Turning around the Y axis results in a vector on the X-axis
            //        fAcc_Roll = (float)Math.Atan2((float)asAcc[0], (float)asAcc[2]) * 180 / (float)Math.PI;
            //        fRoll = fRoll * 0.98f + fAcc_Roll * 0.02f;
            //    }
            //} 
            #endregion Complementary Filter
            #endregion Filter
        }
    }
}
