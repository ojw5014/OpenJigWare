using System;
using System.Collections.Generic;

#if _USING_DOTNET_3_5 || _USING_DOTNET_2_0
#else
using System.Linq;
#endif
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
                            else
                            {
                                //CMessage.Write("Test");
                                //if ((CConvert.IsDigit(strTmp0) == true) && (CConvert.IsDigit(strTmp1) == true)) 
                                //    CMessage.Write("Digit");
                                //else
                                //    CMessage.Write("No Digit");

                                //if ((strTmp0 == "-1") && (strTmp1 == "-1"))
                                //{
                                //    strTmp0 = "1";
                                //    strTmp1 = "1";
                                //}
                                //else 
                                if (strTmp0 == "-1")
                                {
                                    if (strTmp1[0] == '-')
                                    {
                                        strTmp1 = aStrS1[k, j].Substring(1);
                                    }
                                    else
                                    {
                                        strTmp1 = "-" + strTmp1;
                                    }
                                    strTmp0 = "1";
                                }
                                else if (strTmp1 == "-1")
                                {
                                    if (strTmp0[0] == '-')
                                    {
                                        strTmp0 = aStrS0[i, k].Substring(1);
                                    }
                                    else
                                    {
                                        strTmp0 = "-" + strTmp0;
                                    }
                                    strTmp1 = "1";
                                }

                                
                                if (strTmp0 == "1") strTmp = strTmp1;
                                else if (strTmp1 == "1") strTmp = strTmp0;
                                else if (strTmp0 == "0") strTmp = "0";
                                else if (strTmp1 == "0") strTmp = "0";
                                else
                                {
                                    //if ((strTmp0[0] != '(') || (strTmp0[strTmp0.Length - 1] != ')')) strTmp0 = "(" + strTmp0 + ")";
                                    //if ((strTmp1[0] != '(') || (strTmp1[strTmp1.Length - 1] != ')')) strTmp1 = "(" + strTmp1 + ")";
                                    //strTmp = strTmp0 + "*" + strTmp1;
                                    strTmp = "(" + strTmp0 + ")*(" + strTmp1 + ")";
                                }
                            }
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
            public static bool CalcRot(float fAngleX, float fAngleY, float fAngleZ, ref float fX, ref float fY, ref float fZ)
            {
                double [] adVal = new double[6];
                adVal[0] = fAngleX; 
                adVal[1] = fAngleY;
                adVal[2] = fAngleZ;
                adVal[3] = fX;
                adVal[4] = fY;
                adVal[5] = fZ;
                return CalcRot(adVal[0], adVal[1], adVal[2], ref adVal[3], ref adVal[4], ref adVal[5]);
            }
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
            #region Bezier
            // 참고 : https://msdn.microsoft.com/ko-kr/library/system.windows.media.beziersegment.beziersegment(v=vs.110).aspx
            // mu 는 1.0 이 최고값(도착점)
            public SVector3D_t Bezier3(SVector3D_t p0, SVector3D_t p1, SVector3D_t p2, float mu)
            {
                float mum1, mum12, mu2;
                SVector3D_t p;
                mu2 = mu * mu; mum1 = 1 - mu;
                mum12 = mum1 * mum1;
                p.x = p0.x * mum12 + 2 * p1.x * mum1 * mu + p2.x * mu2;
                p.y = p0.y * mum12 + 2 * p1.y * mum1 * mu + p2.y * mu2;
                p.z = p0.z * mum12 + 2 * p1.z * mum1 * mu + p2.z * mu2;
                return (p);
            }
            #endregion Bezier

            // Simply function to calculate only the result of the rotation
            // Kor: 단순히 회전의 결과값만 내 주는(계산해 주는) 함수
            #region Rotation
            public static void Rotation(float ax, float ay, float az, ref float x, ref float y, ref float z)
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
#if false
            // 생생한 게임 개발에 꼭 필요한 기본 물리 - myMath.h
            // 발췌 : http://www.devpia.com/MAEUL/Contents/Detail.aspx?BoardID=1965&MAEULNo=138&no=2331&ref=1255
            #region Quaternion <-> EulerAngles
            public static SVector_t MakeEulerAnglesFromQ(Quaternion q)
            {
                double    r11, r21, r31, r32, r33, r12, r13;
                double    q00, q11, q22, q33;
                double    tmp;
                SVector3D_t    u;
               
                q00 = q.n * q.n;
                q11 = q.v.x * q.v.x;
                q22 = q.v.y * q.v.y;
                q33 = q.v.z * q.v.z;

                r11 = q00 + q11 - q22 - q33;
                r21 = 2 * (q.v.x*q.v.y + q.n*q.v.z);
                r31 = 2 * (q.v.x*q.v.z - q.n*q.v.y);
                r32 = 2 * (q.v.y*q.v.z + q.n*q.v.x);
                r33 = q00 - q11 - q22 + q33;

                tmp = fabs(r31);
                if(tmp > 0.999999)
                {
                    r12 = 2 * (q.v.x*q.v.y - q.n*q.v.z);
                    r13 = 2 * (q.v.x*q.v.z + q.n*q.v.y);

                    u.x = RadiansToDegrees(0.0f); //roll
                    u.y = RadiansToDegrees((float) (-(pi/2) * r31/tmp)); // pitch
                    u.z = RadiansToDegrees((float) atan2(-r12, -r31*r13)); // yaw
                    return u;
                }

                u.x = RadiansToDegrees((float) atan2(r32, r33)); // roll
                u.y = RadiansToDegrees((float) asin(-r31));         // pitch
                u.z = RadiansToDegrees((float) atan2(r21, r11)); // yaw
                return u;
            }


            #endregion Quaternion <-> EulerAngles
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


            #region Another Parallel delta
            // 출처 : http://forums.trossenrobotics.com/tutorials/introduction-129/delta-robot-kinematics-3276/
#if false

             // robot geometry
 // (look at pics above for explanation)
 const float e = 115.0;     // end effector
 const float f = 457.3;     // base
 const float re = 232.0;
 const float rf = 112.0;
 
 // trigonometric constants
 const float sqrt3 = sqrt(3.0);
 const float pi = 3.141592653;    // PI
 const float sin120 = sqrt3/2.0;   
 const float cos120 = -0.5;        
 const float tan60 = sqrt3;
 const float sin30 = 0.5;
 const float tan30 = 1/sqrt3;
 
 // forward kinematics: (theta1, theta2, theta3) -> (x0, y0, z0)
 // returned status: 0=OK, -1=non-existing position
 int delta_calcForward(float theta1, float theta2, float theta3, float &x0, float &y0, float &z0) {
     float t = (f-e)*tan30/2;
     float dtr = pi/(float)180.0;
 
     theta1 *= dtr;
     theta2 *= dtr;
     theta3 *= dtr;
 
     float y1 = -(t + rf*cos(theta1));
     float z1 = -rf*sin(theta1);
 
     float y2 = (t + rf*cos(theta2))*sin30;
     float x2 = y2*tan60;
     float z2 = -rf*sin(theta2);
 
     float y3 = (t + rf*cos(theta3))*sin30;
     float x3 = -y3*tan60;
     float z3 = -rf*sin(theta3);
 
     float dnm = (y2-y1)*x3-(y3-y1)*x2;
 
     float w1 = y1*y1 + z1*z1;
     float w2 = x2*x2 + y2*y2 + z2*z2;
     float w3 = x3*x3 + y3*y3 + z3*z3;
     
     // x = (a1*z + b1)/dnm
     float a1 = (z2-z1)*(y3-y1)-(z3-z1)*(y2-y1);
     float b1 = -((w2-w1)*(y3-y1)-(w3-w1)*(y2-y1))/2.0;
 
     // y = (a2*z + b2)/dnm;
     float a2 = -(z2-z1)*x3+(z3-z1)*x2;
     float b2 = ((w2-w1)*x3 - (w3-w1)*x2)/2.0;
 
     // a*z^2 + b*z + c = 0
     float a = a1*a1 + a2*a2 + dnm*dnm;
     float b = 2*(a1*b1 + a2*(b2-y1*dnm) - z1*dnm*dnm);
     float c = (b2-y1*dnm)*(b2-y1*dnm) + b1*b1 + dnm*dnm*(z1*z1 - re*re);
  
     // discriminant
     float d = b*b - (float)4.0*a*c;
     if (d < 0) return -1; // non-existing point
 
     z0 = -(float)0.5*(b+sqrt(d))/a;
     x0 = (a1*z0 + b1)/dnm;
     y0 = (a2*z0 + b2)/dnm;
     return 0;
 }
 
 // inverse kinematics
 // helper functions, calculates angle theta1 (for YZ-pane)
 int delta_calcAngleYZ(float x0, float y0, float z0, float &theta) {
     float y1 = -0.5 * 0.57735 * f; // f/2 * tg 30
     y0 -= 0.5 * 0.57735    * e;    // shift center to edge
     // z = a + b*y
     float a = (x0*x0 + y0*y0 + z0*z0 +rf*rf - re*re - y1*y1)/(2*z0);
     float b = (y1-y0)/z0;
     // discriminant
     float d = -(a+b*y1)*(a+b*y1)+rf*(b*b*rf+rf); 
     if (d < 0) return -1; // non-existing point
     float yj = (y1 - a*b - sqrt(d))/(b*b + 1); // choosing outer point
     float zj = a + b*yj;
     theta = 180.0*atan(-zj/(y1 - yj))/pi + ((yj>y1)?180.0:0.0);
     return 0;
 }
 
 // inverse kinematics: (x0, y0, z0) -> (theta1, theta2, theta3)
 // returned status: 0=OK, -1=non-existing position
 int delta_calcInverse(float x0, float y0, float z0, float &theta1, float &theta2, float &theta3) {
     theta1 = theta2 = theta3 = 0;
     int status = delta_calcAngleYZ(x0, y0, z0, theta1);
     if (status == 0) status = delta_calcAngleYZ(x0*cos120 + y0*sin120, y0*cos120-x0*sin120, z0, theta2);  // rotate coords to +120 deg
     if (status == 0) status = delta_calcAngleYZ(x0*cos120 - y0*sin120, y0*cos120+x0*sin120, z0, theta3);  // rotate coords to -120 deg
     return status;
 }
#endif
            #endregion Another Parallel delta




            #region Delta Parallel 3 dof - 제작 : 이동현 차장(동부로봇)
            //////////////////////////////////////////////////////////////////////////////////
            // made by Donghyeon, Lee in dongbu-robot(Address Changed to DST Robot)
            // 참고 논문 - http://www.google.co.kr/url?sa=t&rct=j&q=&esrc=s&source=web&cd=10&ved=0ahUKEwiD2Nv_lsbKAhWBFqYKHe7FCzIQFghsMAk&url=http%3A%2F%2Fwww.jonmartinez.neositios.com%2Fdownloads.php%3Fid%3D1012189%26dId%3D0%26fId%3D61194&usg=AFQjCNENsRA6axGz4V_9XgRi7-QU03j-_A&bvm=bv.112454388,d.dGY&cad=rja
            //////////////////////////////////////////////////////////////////////////////////
            
            private static double m_dRad0 = 200.0f;//214.0f;
            private static double m_dL0 = 83.0f;//110.0f;//132.0f;//122.0f;//81.0f;//122.0f;//43.0f;//80.0f;
            private static double m_dL1 = 247.0f;//302.5f;//340.0f;//342.5f;//340.0f;//386.0f;//291.0f;//151.0f;//310.0f;

            private static double m_dRad1 = 38.0f;//28.0f;//37.0f;

            private static bool m_bInitMetal = false;
            public static void Delta_Parallel_Init(
                                    double dRadius_Up, // 윗판의 반지름
                                    double dLength_Up, // 윗 링크의 길이
                                    double dLength_Down, // 아래 링크의 길이
                                    double dRadius_Down // 아래판의 반지름
                )
            {                
               m_dRad0 = dRadius_Up;
               m_dL0 = dLength_Up;
               m_dL1 = dLength_Down;
               m_dRad1 = dRadius_Down;

               m_bInitMetal = true;
            }

            // X 가 앞뒤, Y 가 좌우, Z 가 상하
#if true
            // 새로운거 시도
            public static bool Delta_Parallel_InverseKinematics(double dX, double dY, double dZ, out double dAngle0, out double dAngle1, out double dAngle2)
            {
                dAngle0 = dAngle1 = dAngle2 = 0.0;
                float fX = (float)dX;
                float fY = (float)dY;
                float fZ = (float)dZ;
                float[] afAngle = new float[3];
                if (delta_calcInverse(fX, fY, fZ, out afAngle[0], out afAngle[1], out afAngle[2]) < 0)
                {
                    return false;
                }
                dAngle0 = (double)afAngle[0];
                dAngle1 = (double)afAngle[1];
                dAngle2 = (double)afAngle[2];

                return true;
            }
            private static int delta_calcAngleYZ(float x0, float y0, float z0, out float theta) {

                float e = (float)m_dRad1;// *2.0f;     // end effector // 아래판의 반지름
                float f = (float)m_dRad0;// *2.0f;     // base // 윗판의 반지름
                float re = (float)m_dL1;       // 아래 링크의 길이
                float rf = (float)m_dL0;       // 윗 링크의 길이
                 float y1 = -0.5f * 0.57735f * f; // f/2 * tg 30
                 y0 -= 0.5f * 0.57735f    * e;    // shift center to edge
                 // z = a + b*y
                 float a = (x0*x0 + y0*y0 + z0*z0 +rf*rf - re*re - y1*y1)/(2*z0);
                 float b = (y1-y0)/z0;
                 // discriminant
                 float d = -(a+b*y1)*(a+b*y1)+rf*(b*b*rf+rf);
                 if (d < 0)
                 {
                     theta = 0.0f;
                     return -1; // non-existing point
                 }
                 float yj = (y1 - a*b - (float)Math.Sqrt(d))/(b*b + 1); // choosing outer point
                 float zj = a + b*yj;
                 theta = 180.0f*(float)Math.Atan(-zj/(y1 - yj))/(float)Math.PI + ((yj>y1)?180.0f:0.0f);
                 return 0;
             }
 
             // inverse kinematics: (x0, y0, z0) -> (theta1, theta2, theta3)
             // returned status: 0=OK, -1=non-existing position
            private static int delta_calcInverse(float x0, float y0, float z0, out float theta1, out float theta2, out float theta3)
            {
                 theta1 = theta2 = theta3 = 0;

                 float sqrt3 = (float)Math.Sqrt(3.0);
                 float sin120 = sqrt3 / 2.0f;
                 float cos120 = -0.5f;
                 //float tan60 = sqrt3;
                 //float sin30 = 0.5f;
                 //float tan30 = 1 / sqrt3;

                 int status = delta_calcAngleYZ(x0, y0, z0, out theta1);
                 if (status == 0) status = delta_calcAngleYZ(x0 * cos120 + y0 * sin120, y0 * cos120 - x0 * sin120, z0, out theta2);  // rotate coords to +120 deg
                 if (status == 0) status = delta_calcAngleYZ(x0 * cos120 - y0 * sin120, y0 * cos120 + x0 * sin120, z0, out theta3);  // rotate coords to -120 deg
                 return status;
             }
#else
            public static bool Delta_Parallel_InverseKinematics(double dX, double dY, double dZ, out double dAngle0, out double dAngle1, out double dAngle2)
            {
                dAngle0 = dAngle1 = dAngle2 = 0.0;
                if (m_bInitMetal == false) return false;
#if true// 원래것
                // 축 방향이 반대
                dX = -dX;

                double x1p, y1p, x2p, y2p, x3p, y3p, zp;
                double xf1, xf2, xf3;
                //double yf1,yf2,yf3;
                double xj1, zj1, xj2, zj2, xj3, zj3;
                double theta, phi, a, b, c1, c2, dum;

                zp = dZ;


#if true
                x1p = dX;
                y1p = dY;

                x2p = dX * Math.Cos(D2R(-120.0)) - dY * Math.Sin(D2R(-120.0));
                y2p = dX * Math.Sin(D2R(-120.0)) + dY * Math.Cos(D2R(-120.0));

                x3p = dX * Math.Cos(D2R(120.0)) - dY * Math.Sin(D2R(120.0));
                y3p = dX * Math.Sin(D2R(120.0)) + dY * Math.Cos(D2R(120.0));
#else
	        x1p=dX*cos(D2R(60.0))-dY*sin(D2R(60.0));
	        y1p=dX*sin(D2R(60.0))+dY*cos(D2R(60.0));
	
	        x2p=dX*cos(D2R(-120.0))-dY*sin(D2R(-120.0));
	        y2p=dX*sin(D2R(-120.0))+dY*cos(D2R(-120.0));

	        x3p=dX*cos(D2R(180.0))-dY*sin(D2R(180.0));
	        y3p=dX*sin(D2R(180.0))+dY*cos(D2R(180.0));
#endif

                xf1 = m_dRad0; // m_dRad0 => mpbf->Mplength
                xf2 = m_dRad0;
                xf3 = m_dRad0;

                //a=m_dRad1-x1p;
                a = -m_dRad1 - x1p; // m_dRad1 => mpbf->Splength
                b = m_dRad0;
                c1 = m_dL0; // mpbf->Rflength
                c2 = m_dL1; // mpbf->Relength

                //phi=Math.Atan(zp/(a+b));
                phi = Math.Atan2(zp, (a + b));

                dum = ((c2 * c2 - zp * zp - y1p * y1p - (a + b) * (a + b) - c1 * c1) / (2.0 * c1 * Math.Sqrt((a + b) * (a + b) + zp * zp)));

                theta = Math.Acos((c2 * c2 - zp * zp - y1p * y1p - (a + b) * (a + b) - c1 * c1) / (2.0 * c1 * Math.Sqrt((a + b) * (a + b) + zp * zp))) - phi;
                //theta=acos((c2*c2-zp*zp-y1p*y1p-(a+b)*(a+b)-c1*c1)/(2.0*c1*sqrt((a+b)*(a+b)+zp*zp)))+phi;


                xj1 = c1 * Math.Cos(theta) + b;
                zj1 = c1 * Math.Sin(theta);
                //zj1=-c1*Math.Sin(theta);

                //dAngle0=Math.Atan(zj1/(xf1-xj1));
                dAngle0 = R2D(Math.Atan2(zj1, (xj1 - xf1)));

                //a=m_dRad1-x2p;
                a = -m_dRad1 - x2p;

                phi = Math.Atan2(zp, (a + b));

                dum = ((c2 * c2 - zp * zp - y2p * y2p - (a + b) * (a + b) - c1 * c1) / (2.0 * c1 * Math.Sqrt((a + b) * (a + b) + zp * zp)));

                theta = Math.Acos((c2 * c2 - zp * zp - y2p * y2p - (a + b) * (a + b) - c1 * c1) / (2.0 * c1 * Math.Sqrt((a + b) * (a + b) + zp * zp))) - phi;

                xj2 = c1 * Math.Cos(theta) + b;
                zj2 = c1 * Math.Sin(theta);

                //dAngle1=Math.Atan(zj2/(xf2-xj2));
                dAngle1 = R2D(Math.Atan2(zj2, (xj2 - xf2)));

                a = -m_dRad1 - x3p;

                //phi=Math.Atan(zp/(a+b));
                phi = Math.Atan2(zp, (a + b));

                dum = ((c2 * c2 - zp * zp - y3p * y3p - (a + b) * (a + b) - c1 * c1) / (2.0 * c1 * Math.Sqrt((a + b) * (a + b) + zp * zp)));

                theta = Math.Acos((c2 * c2 - zp * zp - y3p * y3p - (a + b) * (a + b) - c1 * c1) / (2.0 * c1 * Math.Sqrt((a + b) * (a + b) + zp * zp))) - phi;

                xj3 = c1 * Math.Cos(theta) + b;
                //zj3=c1*sin(theta);
                zj3 = c1 * Math.Sin(theta);

                //dAngle2=Math.Atan(zj3/(xf3-xj3));
                dAngle2 = R2D(Math.Atan2(zj3, (xj3 - xf3)));

                /**********************************via angle******************************/

#if false
            double[] adRx = new double[3];
            double[] adRy = new double[3];
            double[] adRz = new double[3];
            double[] adRTx = new double[3];
            double[] adRTy = new double[3];
	        double[] l3 = new double[3];
            double[] adPrj = new double[3];

            adRx[0] = dX + m_dRad1 - m_dRad0;
            adRy[0] = dY;
            adRz[0] = dZ;

            l3[0] = adRx[0] * adRx[0] + adRy[0] * adRy[0] + adRz[0] * adRz[0];

            adRx[1] = dX * Math.Cos(D2R(-120.0)) - dY * Math.Sin(D2R(-120.0));
            adRy[1] = dX * Math.Sin(D2R(-120.0)) + dY * Math.Cos(D2R(-120.0));
            adRz[1] = dZ;

            adRx[1] = adRx[1] + m_dRad1 - m_dRad0;

            l3[1] = adRx[1] * adRx[1] + adRy[1] * adRy[1] + adRz[1] * adRz[1];

            adRx[2] = dX * Math.Cos(D2R(120.0)) - dY * Math.Sin(D2R(120.0));
            adRy[2] = dX * Math.Sin(D2R(120.0)) + dY * Math.Cos(D2R(120.0));
            adRz[2] = dZ;

            adRx[2] = adRx[2] + m_dRad1 - m_dRad0;

            l3[2] = adRx[2] * adRx[2] + adRy[2] * adRy[2] + adRz[2] * adRz[2];

            m_afMot[3] = 180.0f - (float)R2D(Math.Acos((m_dL0 * m_dL0 + m_dL1 * m_dL1 - l3[0]) / (2.0 * m_dL0 * m_dL1)));
            m_afMot[4] = 180.0f - (float)R2D(Math.Acos((m_dL0 * m_dL0 + m_dL1 * m_dL1 - l3[1]) / (2.0 * m_dL0 * m_dL1)));
            m_afMot[5] = 180.0f - (float)R2D(Math.Acos((m_dL0 * m_dL0 + m_dL1 * m_dL1 - l3[2]) / (2.0 * m_dL0 * m_dL1)));
#if false
            ///////////////
            for (int i = 0; i < 3; i++)
            {
                adRTx[i] = m_dRad0 * Math.Cos(D2R(-120.0 * i)) - 0 * Math.Sin(D2R(-120.0 * i));
                adRTy[i] = m_dRad0 * Math.Sin(D2R(-120.0 * i)) + 0 * Math.Cos(D2R(-120.0 * i));

                adPrj[i] = Math.Sqrt((adRTx[i] - adRx[i]) * (adRTx[i] - adRx[i]) + (adRTy[i] - adRy[i]) * (adRTy[i] - adRy[i]));
            }
            double d = adRx[0] * adRx[0] + adRy[0] * adRy[0];
            //double d = dX * dX + dY * dY;

            m_afMot[6] = (float)R2D(Math.Acos((m_dRad0 * m_dRad0 + adPrj[0] * adPrj[0] - d) / (2.0 * m_dRad0 * adPrj[0])));
#endif
        /*************************************************************************/



#if false
            // ojw5014
            double dp2, d2, dMx, dMy, dMz;
            // d -> (0, m_dRad0, 0)  & (x + 0, y + m_dRad1, zj1) 
            dp2 = (x1p + m_dRad1 - m_dRad0) * (x1p + m_dRad1 - m_dRad0) + (y1p - 0) * (y1p - 0);
            d2 = dp2 + (zp - 0) * (zp - 0);
            m_afMot[3] = 180.0f - (float)R2D(Math.Acos((c1 * c1 + c2 * c2 - d2) / (2.0 * c1 * c2)));
            // 중간링크의 위치
            dMx =

            m_afMot[6] = (float)R2D(Math.Atan2((y1p - 0), -(x1p + m_dRad1 - m_dRad0)));
            //m_afMot[6] = (float)R2D(Math.Atan2((y1p - 0), (x1p + m_dRad1 - m_dRad0))) - 180.0f;
            //d * d = c1 * c1 + c2 * c2 - 2.0 * c1 * c2 * cos(dAngle0);

            // ojw5014
            d2 = (x2p + m_dRad1 - m_dRad0) * (x2p + m_dRad1 - m_dRad0) + (y2p - 0) * (y2p - 0) + (zp - 0) * (zp - 0);
            m_afMot[4] = 180.0f - (float)R2D(Math.Acos((c1 * c1 + c2 * c2 - d2) / (2.0 * c1 * c2)));

            // ojw5014
            d2 = (x3p + m_dRad1 - m_dRad0) * (x3p + m_dRad1 - m_dRad0) + (y3p - 0) * (y3p - 0) + (zp - 0) * (zp - 0);
            m_afMot[5] = 180.0f - (float)R2D(Math.Acos((c1 * c1 + c2 * c2 - d2) / (2.0 * c1 * c2)));
#endif
#else
                //double[] adRx = new double[3];
                //double[] adRy = new double[3];
                //double[] adRz = new double[3];
                //double[] l3 = new double[3];
                //double[] l3p = new double[3];
                //adRx[0] = dX + m_dRad1 - m_dRad0;
                //adRy[0] = dY;
                //adRz[0] = dZ;

                //l3[0] = adRx[0] * adRx[0] + adRy[0] * adRy[0] + adRz[0] * adRz[0];
                //l3p[0] = adRx[0] * adRx[0] + adRz[0] * adRz[0];

                //adRx[1] = dX * Math.Cos(D2R(-120.0)) - dY * Math.Sin(D2R(-120.0));
                //adRy[1] = dX * Math.Sin(D2R(-120.0)) + dY * Math.Cos(D2R(-120.0));
                //adRz[1] = dZ;

                //adRx[1] = adRx[1] + m_dRad1 - m_dRad0;

                //l3[1] = adRx[1] * adRx[1] + adRy[1] * adRy[1] + adRz[1] * adRz[1];
                //l3p[1] = adRx[1] * adRx[1] + adRz[1] * adRz[1];

                //adRx[2] = dX * Math.Cos(D2R(120.0)) - dY * Math.Sin(D2R(120.0));
                //adRy[2] = dX * Math.Sin(D2R(120.0)) + dY * Math.Cos(D2R(120.0));
                //adRz[2] = dZ;

                //adRx[2] = adRx[2] + m_dRad1 - m_dRad0;

                //l3[2] = adRx[2] * adRx[2] + adRy[2] * adRy[2] + adRz[2] * adRz[2];
                //l3p[2] = adRx[2] * adRx[2] + adRz[2] * adRz[2];

                //m_afMot[3] = 180.0f - (float)R2D(Math.Acos((m_dL0 * m_dL0 + m_dL1 * m_dL1 - l3p[0]) / (2.0 * m_dL0 * m_dL1)));
                //m_afMot[4] = 180.0f - (float)R2D(Math.Acos((m_dL0 * m_dL0 + m_dL1 * m_dL1 - l3p[1]) / (2.0 * m_dL0 * m_dL1)));
                //m_afMot[5] = 180.0f - (float)R2D(Math.Acos((m_dL0 * m_dL0 + m_dL1 * m_dL1 - l3p[2]) / (2.0 * m_dL0 * m_dL1)));

                //if (float.IsNaN(m_afMot[3])) return false;
                //if (float.IsNaN(m_afMot[4])) return false;
                //if (float.IsNaN(m_afMot[5])) return false;

                //m_afMot[6] = -(float)R2D(Math.Acos(l3p[0] / l3[0]));
                //m_afMot[7] = -(float)R2D(Math.Acos(l3p[1] / l3[1]));
                //m_afMot[8] = -(float)R2D(Math.Acos(l3p[2] / l3[2]));

                return true;
#endif

#else
            double a, b, c;
            double beta1, gamma1, beta2, gamma2, beta3, gamma3;
            double m1, m2, m3;

            double R = Ojw.CConvert.StrToDouble(txtRad0.Text);
            double sr = Ojw.CConvert.StrToDouble(txtRad1.Text);
            double h = 0;
            double L1 = Ojw.CConvert.StrToDouble(txtL0.Text);
            double L2 = Ojw.CConvert.StrToDouble(txtL1.Text);

            a = dX;
            b = dY;
            c = dZ;


            const double dOffset = 30;
            int nNum = 0;//0, 1, 2
            double dAngle = D2R((120 * (nNum++ % 3) + dOffset) % 360);
            double mmm1 = (a * Math.Cos(dAngle) - Math.Sin(dAngle) * b - sr + R);

            //double mmm1 = (a * Math.Sqrt(3)) / 2 - 0.5 * b - sr + R;

            beta1 = Math.Acos((Math.Pow(L1, 2) - Math.Pow(L2, 2) + Math.Pow(mmm1, 2) + Math.Pow(c - h, 2)) / (2 * L1 * Math.Sqrt(Math.Pow(mmm1, 2) + Math.Pow((c - h), 2))));

            gamma1 = Math.Acos(mmm1 / Math.Sqrt(Math.Pow(mmm1, 2) + Math.Pow((c - h), 2)));

            m1 = beta1 + gamma1 - 2 * Math.PI;

            //double mmm2 = (a * Math.Sqrt(3)) / 2 + 0.5 * b + sr + R;
            dAngle = D2R((120 * (nNum++ % 3) + dOffset) % 360);
            double mmm2 = (a * Math.Cos(dAngle) - Math.Sin(dAngle) * b - sr + R);

            beta2 = Math.Acos((Math.Pow(L1, 2) - Math.Pow(L2, 2) + Math.Pow(mmm2, 2) + Math.Pow(c - h, 2)) / (2 * L1 * Math.Sqrt(Math.Pow(mmm2, 2) + Math.Pow((c - h), 2))));

            gamma2 = Math.Acos(mmm2 / Math.Sqrt(Math.Pow(mmm2, 2) + Math.Pow((c - h), 2)));

            m2 = beta2 + gamma2 - 2 * Math.PI;

            //double mmm3 = b - sr + R;

            dAngle = D2R((120 * (nNum++ % 3) + dOffset) % 360);
            double mmm3 = (a * Math.Cos(dAngle) - Math.Sin(dAngle) * b - sr + R);

            beta3 = Math.Acos((Math.Pow(L1, 2) - Math.Pow(L2, 2) + Math.Pow(mmm3, 2) + Math.Pow(c - h, 2)) / (2 * L1 * Math.Sqrt(Math.Pow(mmm3, 2) + Math.Pow((c - h), 2))));

            gamma3 = Math.Acos(mmm3 / Math.Sqrt(Math.Pow(mmm3, 2) + Math.Pow((c - h), 2)));

            m3 = beta3 + gamma3 - 2 * Math.PI;



            dAngle0 = (180 - R2D(m1)) % 360;
            dAngle1 = (180 - R2D(m2)) % 360;
            dAngle2 = (180 - R2D(m3)) % 360;



            return true;

#endif

            }
#endif
            public static bool Delta_Parallel_ForwardKinematics(double dM0, double dM1, double dM2, out double dX, out double dY, out double dZ)
            {

#if false
                // Made by Donghyon-Lee
                double re2;
                double w1, w2, w3, d, al1, al2, be1, be2;
                double a, b, c;
                double min, max;


                //SVertex3D_t pc1, pc2, pc3;
                double pc1_x, pc1_y, pc1_z;
                double pc2_x, pc2_y, pc2_z;
                double pc3_x, pc3_y, pc3_z;

                pc1_x = (m_dRad0 - m_dRad1 + m_dL0 * Math.Cos(dM0)) * Math.Cos(D2R(0.0));
                pc2_x = (m_dRad0 - m_dRad1 + m_dL0 * Math.Cos(dM1)) * Math.Cos(D2R(-120.0));
                pc3_x = (m_dRad0 - m_dRad1 + m_dL0 * Math.Cos(dM2)) * Math.Cos(D2R(120.0));

                pc1_y = -(m_dRad0 - m_dRad1 + m_dL0 * Math.Cos(dM0)) * Math.Sin(D2R(0.0));
                pc2_y = -(m_dRad0 - m_dRad1 + m_dL0 * Math.Cos(dM1)) * Math.Sin(D2R(-120.0));
                pc3_y = -(m_dRad0 - m_dRad1 + m_dL0 * Math.Cos(dM2)) * Math.Sin(D2R(120.0));

                //pc1.z=-m_dL0*Math.Sin(dM0);
                //pc2.z=-m_dL0*Math.Sin(dM1);
                //pc3.z=-m_dL0*Math.Sin(dM2);

                pc1_z = m_dL0 * Math.Sin(dM0);
                pc2_z = m_dL0 * Math.Sin(dM1);
                pc3_z = m_dL0 * Math.Sin(dM2);

                re2 = m_dL1 * m_dL1;

                /*********************Gauss elimination method************************************/

                w1 = pc1_x * pc1_x + pc1_y * pc1_y + pc1_z * pc1_z;
                w2 = pc2_x * pc2_x + pc2_y * pc2_y + pc2_z * pc2_z;
                w3 = pc3_x * pc3_x + pc3_y * pc3_y + pc3_z * pc3_z;

                d = (pc2_x - pc1_x) * pc3_y - (pc3_x - pc1_x) * pc2_y;

                al1 = ((pc3_z - pc1_z) * pc2_y - (pc2_z - pc1_z) * pc3_y) / d;
                be1 = (((w2 - w1) * pc3_y - (w3 - w1) * pc2_y) / 2.0) / d;
                //be1=(((w1-w2)*pc3_y-(w1-w3)*pc2_y)/2.0)/d;

                al2 = -((pc2_x - pc1_x) * (pc3_z - pc1_z) - (pc3_x - pc1_x) * (pc2_z - pc1_z)) / d;
                be2 = -(((w2 - w1) * (pc3_x - pc1_x) - (w3 - w1) * (pc2_x - pc1_x)) / 2.0) / d;
                //be2=-(((w1-w2)*(pc3_x-pc1_x)-(w1-w3)*(pc2_x-pc1_x))/2.0)/d;
                /**********************************************************************************/

                /******************************Z value Quadratic formula***************************/
                a = (al1 * al1 + al2 * al2 + 1);
                b = 2.0 * ((be1 - pc1_x) * al1 + al2 * be2 - pc1_z);
                c = be2 * be2 + (pc1_x - be1) * (pc1_x - be1) + pc1_z * pc1_z - re2;

                max = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2.0 * a);
                min = (-b - Math.Sqrt(b * b - 4 * a * c)) / (2.0 * a);
                /**********************************************************************************/

                if (min > 0.0 && max > 0.0)
                {
                    if (min < max)
                        dZ = max;
                    else
                        dZ = min;
                }
                else
                {
                    if (min > 0.0)
                        dZ = min;
                    else if (max > 0.0)
                        dZ = max;
                    else
                    {
                        dX = dY = dZ = 0;
                        return false;
                        //Ojw.CMessage.Write("Error\n");
                    }
                }

                dX = al1 * (dZ) + be1;
                dY = al2 * (dZ) + be2;

                /**********************************via angle******************************/

                //XyPoint rxy[3];
                //double l3[3];

                //rxy[0].x=pobf->x+m_dRad1-m_dRad0;
                //rxy[0].y=pobf->y;
                //rxy[0].z=pobf->z;

                //l3[0]=rxy[0].x*rxy[0].x+rxy[0].y*rxy[0].y+rxy[0].z*rxy[0].z;

                //rxy[1].x=pobf->x*Math.Cos(DegreeToRadian(-120.0))-pobf->y*Math.Sin(DegreeToRadian(-120.0)); 
                //rxy[1].y=pobf->x*Math.Sin(DegreeToRadian(-120.0))+pobf->y*Math.Cos(DegreeToRadian(-120.0));
                //rxy[1].z=pobf->z;

                //rxy[1].x=rxy[1].x+m_dRad1-m_dRad0;

                //l3[1]=rxy[1].x*rxy[1].x+rxy[1].y*rxy[1].y+rxy[1].z*rxy[1].z;

                //rxy[2].x=pobf->x*Math.Cos(DegreeToRadian(120.0))-pobf->y*Math.Sin(DegreeToRadian(120.0));
                //rxy[2].y=pobf->x*Math.Sin(DegreeToRadian(120.0))+pobf->y*Math.Cos(DegreeToRadian(120.0));
                //rxy[2].z=pobf->z;

                //rxy[2].x=rxy[2].x+m_dRad1-m_dRad0;

                //l3[2]=rxy[2].x*rxy[2].x+rxy[2].y*rxy[2].y+rxy[2].z*rxy[2].z;

                //vjo->j1=Math.Acos((m_fL0*m_fL0+m_fL1*m_fL1-l3[0])/(2.0*m_fL0*m_fL1));
                //vjo->j2=Math.Acos((m_fL0*m_fL0+m_fL1*m_fL1-l3[1])/(2.0*m_fL0*m_fL1));
                //vjo->j3=Math.Acos((m_fL0*m_fL0+m_fL1*m_fL1-l3[2])/(2.0*m_fL0*m_fL1));

                /*************************************************************************/
                //}
#else
                // robot geometry
                // (look at pics above for explanation)
                double e = m_dRad1;     // end effector // 아래판의 반지름
                double f = m_dRad0;     // base // 윗판의 반지름
                double re = m_dL1;       // 아래 링크의 길이
                double rf = m_dL0;       // 윗 링크의 길이

                double t = (f - e) * Ojw.CMath.Tan(30) / 2;
                //float dtr = (float)Math.PI / 180.0;



                double y1 = -(t + rf * Ojw.CMath.Cos(-dM0));
                double z1 = -rf * Ojw.CMath.Sin(-dM0);

                double y2 = (t + rf * Ojw.CMath.Cos(-dM1)) * Ojw.CMath.Sin(30);
                double x2 = y2 * Ojw.CMath.Tan(60);
                double z2 = -rf * Ojw.CMath.Sin(-dM1);

                double y3 = (t + rf * Ojw.CMath.Cos(-dM2)) * Ojw.CMath.Sin(30);
                double x3 = -y3 * Ojw.CMath.Tan(60);
                double z3 = -rf * Ojw.CMath.Sin(-dM2);

                double dnm = (y2 - y1) * x3 - (y3 - y1) * x2;

                double w1 = y1 * y1 + z1 * z1;
                double w2 = x2 * x2 + y2 * y2 + z2 * z2;
                double w3 = x3 * x3 + y3 * y3 + z3 * z3;

                // x = (a1*z + b1)/dnm
                double a1 = (z2 - z1) * (y3 - y1) - (z3 - z1) * (y2 - y1);
                double b1 = -((w2 - w1) * (y3 - y1) - (w3 - w1) * (y2 - y1)) / 2.0;

                // y = (a2*z + b2)/dnm;
                double a2 = -(z2 - z1) * x3 + (z3 - z1) * x2;
                double b2 = ((w2 - w1) * x3 - (w3 - w1) * x2) / 2.0;

                // a*z^2 + b*z + c = 0
                double a = a1 * a1 + a2 * a2 + dnm * dnm;
                double b = 2 * (a1 * b1 + a2 * (b2 - y1 * dnm) - z1 * dnm * dnm);
                double c = (b2 - y1 * dnm) * (b2 - y1 * dnm) + b1 * b1 + dnm * dnm * (z1 * z1 - re * re);

                // discriminant
                double d = b * b - 4.0 * a * c;

                if (d < 0)
                {
                    dX = dY = dZ = 0;
                    return false; // non-existing point
                }

                double dHeight = -0.5 * (b + Math.Sqrt(d)) / a;
                dX = (a1 * dHeight + b1) / dnm;
                dY = (a2 * dHeight + b2) / dnm;
                dZ = -dHeight;
                
#endif
                return true;
            }
            #endregion Delta Parallel 3 dof - 제작 : 이동현 차장(동부로봇)

            #region Delta 2번째 - http://forums.trossenrobotics.com/tutorials/introduction-129/delta-robot-kinematics-3276/
            // 테스트 : https://www.marginallyclever.com/other/samples/fk-ik-test.html

            #endregion Delta 2번째
        }
        /// <summary>
        /// made by Dongjune Chang, 
        /// reference : 책 - "칼만필터의 이해" in Korea,
        /// </summary>
        #region Kalman Filter - made by Dongjune Chang from the book - "MATLAB 활용 칼만필터의 이해(저자 김성필)"
        public class CKalman
        {

            public class AvgFilter : FilterBase<double>
            {
                double prevAvg = 0.0;
                int k = 0;
                bool bFirstRun = true;

                public AvgFilter()
                {

                }

                public override void Init()
                {
                    k = 0;
                    prevAvg = 0.0;
                }

                public override bool Do(double input, ref double output)
                {
                    if (bFirstRun)
                    {
                        k = 1;
                        prevAvg = 0.0;
                        bFirstRun = false;
                    }

                    double x = input;
                    output = 0.0;

                    double alpha = (double)(k - 1) / (double)k;
                    double avg = alpha * prevAvg + (1 - alpha) * x;

                    output = avg;
                    prevAvg = avg;
                    k++;

                    return true;
                }
            }
            public class DeDvKalman : FilterBase<double[]>
            {
                bool bFirstRun = true;

                public double[,] A, H, Q, R;
                public double[] x;
                public double[,] P;
                public double[,] K;
                private int nDim = 1;
                private double dt = 0.1;

                public DeDvKalman(int nDim, double dt)
                {
                    this.nDim = nDim;
                    this.dt = dt;
                }

                public override void Init()
                {
                    A = new double[nDim, nDim];
                    H = new double[nDim, nDim];
                    Q = new double[nDim, nDim];
                    R = new double[nDim, nDim];
                    K = new double[nDim, nDim];

                    x = new double[nDim];
                    P = new double[nDim, nDim];
                }

                public void GetValue(ref double pos, ref double vel)
                {
                    pos = x[0];
                    vel = x[1];
                }

                public override bool Do(double[] input, ref double[] output)
                {
                    double[] z = (double[])input.Clone();

                    if (bFirstRun)
                    {
                        A = new double[,] { { 1, dt }, { 0, 1 } };
                        H = new double[,] { { 1, 0 } };
                        Q = new double[,] { { 1, 0 }, { 0, 3 } };
                        R = new double[,] { { 10 } };

                        x = new double[] { 0, 20 };
                        P = CMatrix.ScalarMultiply(5.0, new double[,] { { 1, 0 }, { 0, 1 } });

                        bFirstRun = false;
                    }
                    double[] xp = CMatrix.Multiply(A, x);
                    double[,] Pp = CMatrix.Add(
                        CMatrix.Multiply(CMatrix.Multiply(A, P), CMatrix.Trans(A)), Q);

                    double[,] Pp_H_T = CMatrix.Multiply(Pp, CMatrix.Trans(H));

                    //K = 1 / (Pp(1,1) + R) * [Pp(1,1) Pp(2,1)]';  % Pp*H'*inv(H*Pp*H' + R);
                    K = CMatrix.ScalarMultiply(1 / (Pp[0, 0] + R[0, 0]),
                        new double[,] { { Pp[0, 0] }, { Pp[1, 0] } }
                        );

                    x = CMatrix.Add(xp, CMatrix.Multiply(K,
                        CMatrix.Substract(z, CMatrix.Multiply(H, xp))
                        ));

                    P = CMatrix.Substract(Pp, CMatrix.Multiply(CMatrix.Multiply(K, H), Pp));

                    output = (double[])x.Clone();

                    return true;
                }
            }
            public class DvKalman : FilterBase<double[]>
            {
                bool bFirstRun = true;

                public double[,] A, H, Q, R;
                public double[] x;
                public double[,] P;
                public double[,] K;
                private int nDim = 1;
                private double dt = 0.1;

                public DvKalman(int nDim, double dt)
                {
                    this.nDim = nDim;
                    this.dt = dt;
                }

                public override void Init()
                {
                    A = new double[nDim, nDim];
                    H = new double[nDim, nDim];
                    Q = new double[nDim, nDim];
                    R = new double[nDim, nDim];
                    K = new double[nDim, nDim];

                    x = new double[nDim];
                    P = new double[nDim, nDim];
                }

                public void GetValue(ref double pos, ref double vel)
                {
                    pos = x[0];
                    vel = x[1];
                }

                public override bool Do(double[] input, ref double[] output)
                {
                    double[] z = (double[])input.Clone();

                    if (bFirstRun)
                    {
                        A = new double[,] { { 1, dt }, { 0, 1 } };
                        H = new double[,] { { 1, 0 } };
                        Q = new double[,] { { 1, 0 }, { 0, 3 } };
                        R = new double[,] { { 10 } };

                        x = new double[] { 0, 20 };
                        P = CMatrix.ScalarMultiply(5.0, new double[,] { { 1, 0 }, { 0, 1 } });

                        bFirstRun = false;
                    }
                    double[] xp = CMatrix.Multiply(A, x);
                    double[,] Pp = CMatrix.Add(
                        CMatrix.Multiply(CMatrix.Multiply(A, P), CMatrix.Trans(A)), Q);

                    double[,] Pp_H_T = CMatrix.Multiply(Pp, CMatrix.Trans(H));

                    K = CMatrix.Multiply(
                        Pp_H_T, CMatrix.Inverse(
                            CMatrix.Add(
                                CMatrix.Multiply(H, Pp_H_T), R)
                                )
                            );

                    x = CMatrix.Add(xp, CMatrix.Multiply(K,
                        CMatrix.Substract(z, CMatrix.Multiply(H, xp))
                        ));

                    P = CMatrix.Substract(Pp, CMatrix.Multiply(CMatrix.Multiply(K, H), Pp));

                    output = (double[])x.Clone();

                    return true;
                }
            }
            public class EulerEKF : FilterBase<double[]>
            {
                bool bFirstRun = true;

                public double[,] A, H, Q, R;
                public double[] x;
                public double[,] P;
                public double[,] K;
                private int nDim = 1;
                private double dt = 0.1;

                public EulerEKF(double dt)
                {
                    this.dt = dt;
                }

                public void SetCMatrix_A(double[,] val)
                {
                    A = (double[,])val.Clone();
                }

                public override void Init()
                {
                    H = new double[2, 3];
                    Q = new double[3, 3];
                    R = new double[2, 2];
                    K = new double[3, 3];

                    x = new double[3];
                    P = new double[3, 3];
                }


                public double[] fx(double[] xhat, double[] rates, double dt)
                {
                    double[] xp = new double[3];

                    double phi = xhat[0];
                    double theta = xhat[1];

                    double p = rates[0];
                    double q = rates[1];
                    double r = rates[2];

                    double[] xdot = new double[3];
                    double sec_theta = 1 / Math.Cos(theta);
                    xdot[0] = p + q * Math.Sin(phi) * Math.Tan(theta) + r * Math.Cos(phi) * Math.Tan(theta);
                    xdot[1] = q * Math.Cos(phi) - r * Math.Sin(phi);
                    xdot[2] = q * Math.Sin(phi) * sec_theta + r * Math.Cos(phi) * sec_theta;

                    for (int i = 0; i < 3; i++)
                    {
                        xp[i] = xhat[i] + xdot[i] * dt;
                    }
                    return xp;
                }
                public double[,] Ajacob(double[] xhat, double[] rates, double dt)
                {
                    double[,] A = new double[3, 3];

                    double phi = xhat[0];
                    double theta = xhat[1];

                    double p = rates[0];
                    double q = rates[1];
                    double r = rates[2];

                    double sec_theta = 1 / Math.Cos(theta);

                    A[0, 0] = q * Math.Cos(phi) * Math.Tan(theta) - r * Math.Sin(phi) * Math.Tan(theta);
                    A[0, 1] = q * Math.Sin(phi) * sec_theta * sec_theta + r * Math.Cos(phi) * sec_theta * sec_theta;
                    A[0, 2] = 0;

                    A[1, 0] = -q * Math.Sin(phi) - r * Math.Cos(phi);
                    A[1, 1] = 0;
                    A[1, 2] = 0;

                    A[2, 0] = q * Math.Cos(phi) * sec_theta - r * Math.Sin(phi) * sec_theta;
                    A[2, 1] = q * Math.Sin(phi) * sec_theta * Math.Tan(theta) + r * Math.Cos(phi) * sec_theta * Math.Tan(theta);
                    A[2, 2] = 0;

                    return CMatrix.Add(CMatrix.eye(3), CMatrix.ScalarMultiply(dt, A));
                }

                //public override bool Do(double[] input, ref double[] output)
                //{
                //    double[] z = (double[])input.Clone();

                //    if (bFirstRun)
                //    {
                //        H = new double[,] { { 1, 0, 0 }, { 0, 1, 0 } };
                //        Q = new double[,] { { 0.0001, 0, 0 }, { 0, 0.0001, 0 }, { 0, 0, 1 } };
                //        R = new double[,] { { 6, 0 }, { 0, 6 } };

                //        x = new double[] { 0, 0, 0 };
                //        P = CMatrix.ScalarMultiply(10.0, CMatrix.eye(3));

                //        bFirstRun = false;
                //    }

                //    A = Ajacob(x, rates, dt);

                //    double[] xp = CMatrix.Multiply(A, x);
                //    double[,] Pp = CMatrix.Add(
                //        CMatrix.Multiply(CMatrix.Multiply(A, P), CMatrix.Trans(A)), Q);

                //    double[,] Pp_H_T = CMatrix.Multiply(Pp, CMatrix.Trans(H));

                //    K = CMatrix.Multiply(
                //        Pp_H_T, CMatrix.Inverse(
                //            CMatrix.Add(
                //                CMatrix.Multiply(H, Pp_H_T), R)
                //                )
                //            );

                //    x = CMatrix.Add(xp, CMatrix.Multiply(K,
                //        CMatrix.Substract(z, CMatrix.Multiply(H, xp))
                //        ));

                //    P = CMatrix.Substract(Pp, CMatrix.Multiply(CMatrix.Multiply(K, H), Pp));

                //    output = (double[])x.Clone();

                //    return true;
                //}
            }
            public class EulerKalman : FilterBase<double[]>
            {
                bool bFirstRun = true;

                public double[,] A, H, Q, R;
                public double[] x;
                public double[,] P;
                public double[,] K;
                private int nDim = 4;
                private double dt = 0.1;

                public EulerKalman(int nDim, double dt)
                {
                    this.nDim = nDim;
                    this.dt = dt;
                }

                public override void Init()
                {
                    A = new double[nDim, nDim];
                    H = new double[nDim, nDim];
                    Q = new double[nDim, nDim];
                    R = new double[nDim, nDim];
                    K = new double[nDim, nDim];

                    x = new double[nDim];
                    P = new double[nDim, nDim];
                }

                public void GetValue(ref double pos, ref double vel)
                {
                    pos = x[0];
                    vel = x[1];
                }

                public void SetMatrix_A(double[,] val)
                {
                    A = (double[,])val.Clone();
                }

                public void GetEuler(double[] x, ref double phi, ref double theta, ref double psi)
                {
                    phi = Math.Atan2(2 * (x[2] * x[3] + x[0] * x[1]), 1 - 2 * (x[1] * x[1] + x[2] * x[2]));
                    theta = -Math.Asin(2 * (x[1] * x[3] - x[0] * x[2]));
                    psi = Math.Atan2(2 * (x[1] * x[2] + x[0] * x[3]), 1 - 2 * (x[2] * x[2] + x[3] * x[3]));
                }

                public override bool Do(double[] input, ref double[] output)
                {
                    double[] z = (double[])input.Clone();

                    if (bFirstRun)
                    {
                        H = CMatrix.eye(4);
                        Q = CMatrix.ScalarMultiply(0.0001, CMatrix.eye(4));
                        R = CMatrix.ScalarMultiply(10, CMatrix.eye(4));

                        x = new double[] { 1, 0, 0, 0 };
                        P = CMatrix.eye(4);

                        bFirstRun = false;
                    }
                    double[] xp = CMatrix.Multiply(A, x);
                    double[,] Pp = CMatrix.Add(
                        CMatrix.Multiply(CMatrix.Multiply(A, P), CMatrix.Trans(A)), Q);

                    double[,] Pp_H_T = CMatrix.Multiply(Pp, CMatrix.Trans(H));

                    K = CMatrix.Multiply(
                   Pp_H_T, CMatrix.Inverse(
                       CMatrix.Add(
                           CMatrix.Multiply(H, Pp_H_T), R)
                           )
                       );

                    x = CMatrix.Add(xp, CMatrix.Multiply(K,
                        CMatrix.Substract(z, CMatrix.Multiply(H, xp))
                        ));

                    P = CMatrix.Substract(Pp, CMatrix.Multiply(CMatrix.Multiply(K, H), Pp));

                    output = (double[])x.Clone();

                    return true;
                }
            }
            public class IntKalman : FilterBase<double[]>
            {
                bool bFirstRun = true;

                public double[,] A, H, Q, R;
                public double[] x;
                public double[,] P;
                public double[,] K;
                private int nDim = 1;
                private double dt = 0.1;

                public IntKalman(int nDim, double dt)
                {
                    this.nDim = nDim;
                    this.dt = dt;
                }

                public override void Init()
                {
                    A = new double[nDim, nDim];
                    H = new double[nDim, nDim];
                    Q = new double[nDim, nDim];
                    R = new double[nDim, nDim];
                    K = new double[nDim, nDim];

                    x = new double[nDim];
                    P = new double[nDim, nDim];
                }

                public void GetValue(ref double pos, ref double vel)
                {
                    pos = x[0];
                    vel = x[1];
                }

                public override bool Do(double[] input, ref double[] output)
                {
                    double[] z = (double[])input.Clone();

                    if (bFirstRun)
                    {
                        A = new double[,] { { 1, dt }, { 0, 1 } };
                        H = new double[,] { { 0, 1 } };
                        Q = new double[,] { { 1, 0 }, { 0, 3 } };
                        R = new double[,] { { 10 } };

                        x = new double[] { 0, 20 };
                        P = CMatrix.ScalarMultiply(5.0, new double[,] { { 1, 0 }, { 0, 1 } });

                        bFirstRun = false;
                    }
                    double[] xp = CMatrix.Multiply(A, x);
                    double[,] Pp = CMatrix.Add(
                        CMatrix.Multiply(CMatrix.Multiply(A, P), CMatrix.Trans(A)), Q);

                    double[,] Pp_H_T = CMatrix.Multiply(Pp, CMatrix.Trans(H));

                    K = CMatrix.Multiply(
                        Pp_H_T, CMatrix.Inverse(
                            CMatrix.Add(
                                CMatrix.Multiply(H, Pp_H_T), R)
                                )
                            );

                    x = CMatrix.Add(xp, CMatrix.Multiply(K,
                        CMatrix.Substract(z, CMatrix.Multiply(H, xp))
                        ));

                    P = CMatrix.Substract(Pp, CMatrix.Multiply(CMatrix.Multiply(K, H), Pp));

                    output = (double[])x.Clone();

                    return true;
                }
            }
            public class LPF : FilterBase<double>
            {
                double prevX = 0.0;
                bool bFirstRun = true;
                public double Alpha
                {
                    get;
                    set;
                }
                public LPF()
                {
                    Alpha = 0.7;
                }

                public override void Init()
                {
                    prevX = 0.0;
                }

                public override bool Do(double input, ref double output)
                {
                    if (bFirstRun)
                    {
                        prevX = 0.0;
                        bFirstRun = false;
                    }

                    output = Alpha * prevX + (1 - Alpha) * input;
                    prevX = output;

                    return true;
                }

            }
            public class MovingAvgFilter : FilterBase<double>
            {
                //double prevAvg = 0.0;
                int n = 10;
                bool bFirstRun = true;
                double[] xMABuf = null;

                public MovingAvgFilter(int nBuf)
                {
                    n = nBuf;
                }

                public override void Init()
                {
                    //prevAvg = 0.0;
                }

                public override void DeInit()
                {
                    if (xMABuf != null)
                    {
                        xMABuf = null;
                    }
                }

                public override bool Do(double input, ref double output)
                {
                    double x = input;

                    if (bFirstRun)
                    {
                        if (xMABuf == null)
                        {
                            xMABuf = new double[n];
                        }
                        //prevAvg = x;
                        bFirstRun = false;
                    }

                    // 값 새로 받고 Buffer 밀기
                    for (int m = 1; m < n; m++)
                    {
                        xMABuf[m - 1] = xMABuf[m];
                    }
                    xMABuf[n - 1] = x;

                    // MovAvgFilter.m
                    //{
                    //    double avg = prevAvg + (x - xMABuf[0]) / (double)n;
                    //}

                    // MovAvgFilter2.m

#if _USING_DOTNET_2_0 ||  _USING_DOTNET_3_5
                    double avg = 0;
                    foreach (double d in xMABuf) avg += d;
                    avg /= (double)n;
#else
                    double avg = xMABuf.ToList<double>().Sum() / (double)n;
#endif

                    output = avg;
                    //prevAvg = avg;

                    return true;
                }
            }
            public class SimpleKalman : FilterBase<double[]>
            {
                bool bFirstRun = true;

                public double[,] A, H, Q, R;
                public double[] x;
                public double[,] P;
                public double[,] K;
                private int nDim = 1;

                public SimpleKalman(int nDim)
                {
                    this.nDim = nDim;
                }

                public override void Init()
                {
                    A = new double[nDim, nDim];
                    H = new double[nDim, nDim];
                    Q = new double[nDim, nDim];
                    R = new double[nDim, nDim];
                    K = new double[nDim, nDim];

                    x = new double[nDim];
                    P = new double[nDim, nDim];
                }

                public void GetMatrix(ref double[,] Px, ref double[,] Kx)
                {
                    Px = (double[,])P.Clone();
                    Kx = (double[,])K.Clone();
                }

                public override bool Do(double[] input, ref double[] output)
                {
                    double[] z = (double[])input.Clone();

                    if (bFirstRun)
                    {
                        for (int i = 0; i < nDim; i++)
                        {
                            A[i, i] = 1;
                            H[i, i] = 1;

                            R[i, i] = 4;

                            x[i] = 14;
                            P[i, i] = 6;
                        }
                        bFirstRun = false;
                    }
                    double[] xp = CMatrix.Multiply(A, x);
                    double[,] Pp = CMatrix.Add(
                        CMatrix.Multiply(CMatrix.Multiply(A, P), CMatrix.Trans(A)), Q);

                    double[,] Pp_H_T = CMatrix.Multiply(Pp, CMatrix.Trans(H));

                    K = CMatrix.Multiply(
                        Pp_H_T, CMatrix.Inverse(
                            CMatrix.Add(
                                CMatrix.Multiply(H, Pp_H_T), R)
                                )
                            );

                    x = CMatrix.Add(xp, CMatrix.Multiply(K,
                        CMatrix.Substract(z, CMatrix.Multiply(H, xp))
                        ));

                    P = CMatrix.Substract(Pp, CMatrix.Multiply(CMatrix.Multiply(K, H), Pp));

                    output = (double[])x.Clone();

                    return true;
                }
            }
            private class CMatrix
            {
                //********************************************************/
                // Function for double values
                //********************************************************/
                public static double[,] Multiply(double[,] matrix1, double[,] matrix2)
                {
                    int mRow1 = matrix1.GetLength(0);
                    int mCol1 = matrix1.GetLength(1);
                    int mRow2 = matrix2.GetLength(0);
                    int mCol2 = matrix2.GetLength(1);

                    double[,] ansMat;

                    if (mCol1 == mRow2)
                    {
                        ansMat = new double[mRow1, mCol2];
                        for (int i = 0; i < mRow1; i++)
                        {
                            for (int j = 0; j < mCol2; j++)
                            {
                                for (int k = 0; k < mRow2; k++)
                                {
                                    ansMat[i, j] += matrix1[i, k] * matrix2[k, j];
                                }
                            }
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for multiplication");
                    }
                }

                // 이 함수는 Vector가 matrix size = A by 1 일 때 사용 가능
                public static double[] Multiply(double[,] matrix, double[] vector)
                {
                    int mRow1 = matrix.GetLength(0);
                    int mCol1 = matrix.GetLength(1);
                    int mRow2 = vector.GetLength(0);
                    //int mCol2 = vector.GetLength(1); //1

                    double[] ansMat;

                    if (mCol1 == mRow2)
                    {
                        ansMat = new double[mRow1];
                        for (int i = 0; i < mRow1; i++)
                        {
                            for (int k = 0; k < mRow2; k++)
                            {
                                ansMat[i] += matrix[i, k] * vector[k];
                            }
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for multiplication");
                    }
                }
                public static double[,] ScalarMultiply(double scalar, double[,] matrix)
                {
                    double[,] ansMat;
                    int mRow = matrix.GetLength(0);
                    int mCol = matrix.GetLength(1);

                    ansMat = new double[mRow, mCol];
                    for (int i = 0; i < mRow; i++)
                    {
                        for (int j = 0; j < mCol; j++)
                        {
                            ansMat[i, j] = scalar * matrix[i, j];
                        }
                    }
                    return ansMat;
                }

                public static double[,] eye(int iSize)
                {   // Identity matrix
                    double[,] ansMat = new double[iSize, iSize];
                    for (int i = 0; i < iSize; i++)
                    {
                        for (int j = 0; j < iSize; j++)
                        {
                            if (i == j) ansMat[i, j] = 1;
                        }
                    }
                    return ansMat;
                }

                public static double[] Add(double[] vector1, double[] vector2)
                {
                    int mRow1 = vector1.GetLength(0);
                    int mRow2 = vector2.GetLength(0);

                    double[] ansMat;

                    if (mRow1 == mRow2)
                    {
                        ansMat = new double[mRow1];
                        for (int i = 0; i < mRow1; i++)
                        {
                            ansMat[i] = vector1[i] + vector2[i];
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for Addition");
                    }
                }

                public static double[,] Add(double[,] matrix1, double[,] matrix2)
                {
                    int mRow1 = matrix1.GetLength(0);
                    int mCol1 = matrix1.GetLength(1);
                    int mRow2 = matrix2.GetLength(0);
                    int mCol2 = matrix2.GetLength(1);

                    double[,] ansMat;

                    if (mCol1 == mCol2 && mRow1 == mRow2)
                    {
                        ansMat = new double[mRow1, mCol1];
                        for (int i = 0; i < mRow1; i++)
                        {
                            for (int j = 0; j < mCol1; j++)
                            {
                                ansMat[i, j] = matrix1[i, j] + matrix2[i, j];
                            }
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for Addition");
                    }
                }

                public static double[] Substract(double[] vector1, double[] vector2)
                {
                    int mRow1 = vector1.GetLength(0);
                    int mRow2 = vector2.GetLength(0);

                    double[] ansMat;

                    if (mRow1 == mRow2)
                    {
                        ansMat = new double[mRow1];
                        for (int i = 0; i < mRow1; i++)
                        {
                            ansMat[i] = vector1[i] - vector2[i];
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for Substraction");
                    }
                }

                public static double[,] Substract(double[,] matrix1, double[,] matrix2)
                {
                    int mRow1 = matrix1.GetLength(0);
                    int mCol1 = matrix1.GetLength(1);
                    int mRow2 = matrix2.GetLength(0);
                    int mCol2 = matrix2.GetLength(1);

                    double[,] ansMat;

                    if (mCol1 == mCol2 && mRow1 == mRow2)
                    {
                        ansMat = new double[mRow1, mCol1];
                        for (int i = 0; i < mRow1; i++)
                        {
                            for (int j = 0; j < mCol1; j++)
                            {
                                ansMat[i, j] = matrix1[i, j] - matrix2[i, j];
                            }
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for Substraction");
                    }
                }

                public static double Determinant(double[,] matrix)
                {
                    double ans = 0;
                    if (matrix.GetLength(0) == matrix.GetLength(1))
                    {
                        int length = matrix.GetLength(0);
                        if (length > 2)
                        {
                            double[,] tempMat = new double[length - 1, length - 1];
                            for (int j = 0; j < length; j++)
                            {
                                int x = 0, y;
                                for (int i1 = 1; i1 < length; i1++)
                                {
                                    y = 0;
                                    for (int j1 = 0; j1 < length; j1++)
                                    {
                                        if (j1 != j)
                                        {
                                            tempMat[x, y] = matrix[i1, j1];
                                            y++;
                                        }
                                    }
                                    x++;
                                }
                                ans += Math.Pow(-1, j) * matrix[0, j] * Determinant(tempMat);
                            }
                            return ans;
                        }
                        else if (length == 2)
                        {
                            ans = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
                            return ans;
                        }
                        else if (length == 1)
                        {
                            return matrix[0, 0];
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        throw new CMatrixException("This Matrix doesn't has Determinant");
                    }
                }

                /*!
                 * Transpose current matrix
                 */
                //public static void Transpose(double[,] matrix)
                //{
                //    double tempVal;
                //    if (matrix.GetLength(0) == matrix.GetLength(1))
                //    {
                //        int length = matrix.GetLength(0);
                //        for (int i = 0; i < length; i++)
                //        {
                //            for (int j = i; j < length; j++)
                //            {
                //                tempVal = matrix[i, j];
                //                matrix[i, j] = matrix[j, i];
                //                matrix[j, i] = tempVal;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        throw new CMatrixException("This Matrix can't be transpose");
                //    }
                //}

                public static double[,] Trans(double[,] matrix)
                {
                    int mCol = matrix.GetLength(0);  // matrix의 Row
                    int mRow = matrix.GetLength(1); // matrix의 Column

                    double[,] ansMat = new double[mRow, mCol];

                    for (int i = 0; i < mRow; i++)
                    {
                        for (int j = 0; j < mCol; j++)
                        {
                            ansMat[i, j] = matrix[j, i];
                        }
                    }
                    return ansMat;
                }
                public static double[,] Copy(double[,] matrix)
                {
                    int mRow = matrix.GetLength(0);
                    int mCol = matrix.GetLength(1);

                    double[,] ansMat = new double[mRow, mCol];

                    for (int i = 0; i < mRow; i++)
                    {
                        for (int j = 0; j < mCol; j++)
                        {
                            ansMat[i, j] = matrix[i, j];
                        }
                    }
                    return ansMat;
                }

                public static double[,] Inverse(double[,] matrix)
                {
                    if (matrix.GetLength(0) == matrix.GetLength(1))
                    {
                        int length = matrix.GetLength(0);

                        if (length > 1)
                        {
                            double[,] tempAnsMat = new double[length, length];
                            double ans = 0;

                            double[,] tempMat = new double[length - 1, length - 1];

                            for (int i = 0; i < length; i++)
                            {
                                for (int j = 0; j < length; j++)
                                {
                                    int x = 0, y;
                                    for (int i1 = 0; i1 < length; i1++)
                                    {
                                        if (i != i1)
                                        {
                                            y = 0;
                                            for (int j1 = 0; j1 < length; j1++)
                                            {
                                                if (j1 != j)
                                                {
                                                    tempMat[x, y] = matrix[i1, j1];
                                                    y++;
                                                }
                                            }
                                            x++;
                                        }
                                    }
                                    //It is saved as transpose matrix in temperary matrix
                                    tempAnsMat[j, i] = Math.Pow(-1, i + j) * Determinant(tempMat);
                                    if (i == 0)
                                        ans += matrix[i, j] * tempAnsMat[j, i];
                                }
                            }
                            if (ans != 0)
                                return ScalarMultiply(1 / ans, tempAnsMat);
                            else
                                throw new CMatrixException("This matrix Determiant is 0. no inverse matrix");
                        }
                        else if (length == 1)
                        {
                            double dVal = 0.0;
                            dVal = matrix[0, 0];
                            if (Math.Abs(dVal) < 0.0001) return new double[,] { { 0 } };
                            else return new double[,] { { 1 / dVal } };
                        }
                        else
                            throw new CMatrixException("This is a Null matrix");
                    }
                    else
                    {
                        throw new CMatrixException("This Matrix can't be inverse");
                    }
                }

                //********************************************************/
                // Function for float values
                //********************************************************/
                public static float[,] Multiply(float[,] matrix1, float[,] matrix2)
                {
                    int mRow1 = matrix1.GetLength(0);
                    int mCol1 = matrix1.GetLength(1);
                    int mRow2 = matrix2.GetLength(0);
                    int mCol2 = matrix2.GetLength(1);

                    float[,] ansMat;

                    if (mCol1 == mRow2)
                    {
                        ansMat = new float[mRow1, mCol2];
                        for (int i = 0; i < mRow1; i++)
                        {
                            for (int j = 0; j < mCol2; j++)
                            {
                                for (int k = 0; k < mRow2; k++)
                                {
                                    ansMat[i, j] += matrix1[i, k] * matrix2[k, j];
                                }
                            }
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for multiplication");
                    }
                }

                public static float[,] ScalarMultiply(float scalar, float[,] matrix)
                {
                    float[,] ansMat;
                    int mRow = matrix.GetLength(0);
                    int mCol = matrix.GetLength(1);

                    ansMat = new float[mRow, mCol];
                    for (int i = 0; i < mRow; i++)
                    {
                        for (int j = 0; j < mCol; j++)
                        {
                            ansMat[i, j] = scalar * matrix[i, j];
                        }
                    }
                    return ansMat;
                }

                public static float[,] Add(float[,] matrix1, float[,] matrix2)
                {
                    int mRow1 = matrix1.GetLength(0);
                    int mCol1 = matrix1.GetLength(1);
                    int mRow2 = matrix2.GetLength(0);
                    int mCol2 = matrix2.GetLength(1);

                    float[,] ansMat;

                    if (mCol1 == mCol2 && mRow1 == mRow2)
                    {
                        ansMat = new float[mRow1, mCol1];
                        for (int i = 0; i < mRow1; i++)
                        {
                            for (int j = 0; j < mCol1; j++)
                            {
                                ansMat[i, j] = matrix1[i, j] + matrix2[i, j];
                            }
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for Addition");
                    }
                }

                public static float[,] Substract(float[,] matrix1, float[,] matrix2)
                {
                    int mRow1 = matrix1.GetLength(0);
                    int mCol1 = matrix1.GetLength(1);
                    int mRow2 = matrix2.GetLength(0);
                    int mCol2 = matrix2.GetLength(1);

                    float[,] ansMat;

                    if (mCol1 == mCol2 && mRow1 == mRow2)
                    {
                        ansMat = new float[mRow1, mCol1];
                        for (int i = 0; i < mRow1; i++)
                        {
                            for (int j = 0; j < mCol1; j++)
                            {
                                ansMat[i, j] = matrix1[i, j] - matrix2[i, j];
                            }
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for Substraction");
                    }
                }

                public static float Determinant(float[,] matrix)
                {
                    float ans = 0;
                    if (matrix.GetLength(0) == matrix.GetLength(1))
                    {
                        int length = matrix.GetLength(0);
                        if (length > 2)
                        {
                            float[,] tempMat = new float[length - 1, length - 1];
                            for (int j = 0; j < length; j++)
                            {
                                int x = 0, y;
                                for (int i1 = 1; i1 < length; i1++)
                                {
                                    y = 0;
                                    for (int j1 = 0; j1 < length; j1++)
                                    {
                                        if (j1 != j)
                                        {
                                            tempMat[x, y] = matrix[i1, j1];
                                            y++;
                                        }
                                    }
                                    x++;
                                }
                                ans += (float)Math.Pow(-1, j) * matrix[0, j] * Determinant(tempMat);
                            }
                            return ans;
                        }
                        else if (length == 2)
                        {
                            ans = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
                            return ans;
                        }
                        else if (length == 1)
                        {
                            return matrix[0, 0];
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        throw new CMatrixException("This Matrix doesn't has Determinant");
                    }
                }

                /*!
                 * Transpose current matrix
                 */
                public static void Transpose(float[,] matrix)
                {
                    float tempVal;
                    if (matrix.GetLength(0) == matrix.GetLength(1))
                    {
                        int length = matrix.GetLength(0);
                        for (int i = 0; i < length; i++)
                        {
                            for (int j = i; j < length; j++)
                            {
                                tempVal = matrix[i, j];
                                matrix[i, j] = matrix[j, i];
                                matrix[j, i] = tempVal;
                            }
                        }
                    }
                    else
                    {
                        throw new CMatrixException("This Matrix can't be transpose");
                    }
                }

                public static float[,] Inverse(float[,] matrix)
                {
                    if (matrix.GetLength(0) == matrix.GetLength(1))
                    {
                        int length = matrix.GetLength(0);
                        if (length > 1)
                        {
                            float[,] tempAnsMat = new float[length, length];
                            float ans = 0;

                            float[,] tempMat = new float[length - 1, length - 1];
                            for (int i = 0; i < length; i++)
                            {
                                for (int j = 0; j < length; j++)
                                {
                                    int x = 0, y;
                                    for (int i1 = 0; i1 < length; i1++)
                                    {
                                        if (i != i1)
                                        {
                                            y = 0;
                                            for (int j1 = 0; j1 < length; j1++)
                                            {
                                                if (j1 != j)
                                                {
                                                    tempMat[x, y] = matrix[i1, j1];
                                                    y++;
                                                }
                                            }
                                            x++;
                                        }
                                    }
                                    //It is saved as transpose matrix in temperary matrix
                                    tempAnsMat[j, i] = (float)Math.Pow(-1, i + j) * Determinant(tempMat);
                                    if (i == 0)
                                        ans += matrix[i, j] * tempAnsMat[j, i];
                                }
                            }
                            if (ans != 0)
                                return ScalarMultiply(1 / ans, tempAnsMat);
                            else
                                throw new CMatrixException("This matrix Determiant is 0. no inverse matrix");
                        }
                        else if (length == 1)
                            return new float[,] { { 0 } };
                        else
                            throw new CMatrixException("This is a Null matrix");
                    }
                    else
                    {
                        throw new CMatrixException("This Matrix can't be inverse");
                    }
                }

                //********************************************************/
                // Function for int values
                //********************************************************/
                public static int[,] Multiply(int[,] matrix1, int[,] matrix2)
                {
                    int mRow1 = matrix1.GetLength(0);
                    int mCol1 = matrix1.GetLength(1);
                    int mRow2 = matrix2.GetLength(0);
                    int mCol2 = matrix2.GetLength(1);

                    int[,] ansMat;

                    if (mCol1 == mRow2)
                    {
                        ansMat = new int[mRow1, mCol2];
                        for (int i = 0; i < mRow1; i++)
                        {
                            for (int j = 0; j < mCol2; j++)
                            {
                                for (int k = 0; k < mRow2; k++)
                                {
                                    ansMat[i, j] += matrix1[i, k] * matrix2[k, j];
                                }
                            }
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for multiplication");
                    }
                }

                public static int[,] ScalarMultiply(int scalar, int[,] matrix)
                {
                    int[,] ansMat;
                    int mRow = matrix.GetLength(0);
                    int mCol = matrix.GetLength(1);

                    ansMat = new int[mRow, mCol];
                    for (int i = 0; i < mRow; i++)
                    {
                        for (int j = 0; j < mCol; j++)
                        {
                            ansMat[i, j] = scalar * matrix[i, j];
                        }
                    }
                    return ansMat;
                }

                public static double[,] ScalarMultiply(double scalar, int[,] matrix)
                {
                    double[,] ansMat;
                    int mRow = matrix.GetLength(0);
                    int mCol = matrix.GetLength(1);

                    ansMat = new double[mRow, mCol];
                    for (int i = 0; i < mRow; i++)
                    {
                        for (int j = 0; j < mCol; j++)
                        {
                            ansMat[i, j] = scalar * (double)matrix[i, j];
                        }
                    }
                    return ansMat;
                }

                public static int[,] Add(int[,] matrix1, int[,] matrix2)
                {
                    int mRow1 = matrix1.GetLength(0);
                    int mCol1 = matrix1.GetLength(1);
                    int mRow2 = matrix2.GetLength(0);
                    int mCol2 = matrix2.GetLength(1);

                    int[,] ansMat;

                    if (mCol1 == mCol2 && mRow1 == mRow2)
                    {
                        ansMat = new int[mRow1, mCol1];
                        for (int i = 0; i < mRow1; i++)
                        {
                            for (int j = 0; j < mCol1; j++)
                            {
                                ansMat[i, j] = matrix1[i, j] + matrix2[i, j];
                            }
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for Addition");
                    }
                }

                public static int[,] Substract(int[,] matrix1, int[,] matrix2)
                {
                    int mRow1 = matrix1.GetLength(0);
                    int mCol1 = matrix1.GetLength(1);
                    int mRow2 = matrix2.GetLength(0);
                    int mCol2 = matrix2.GetLength(1);

                    int[,] ansMat;

                    if (mCol1 == mCol2 && mRow1 == mRow2)
                    {
                        ansMat = new int[mRow1, mCol1];
                        for (int i = 0; i < mRow1; i++)
                        {
                            for (int j = 0; j < mCol1; j++)
                            {
                                ansMat[i, j] = matrix1[i, j] - matrix2[i, j];
                            }
                        }
                        return ansMat;
                    }
                    else
                    {
                        throw new CMatrixException("Matrices are not supported for Substraction");
                    }
                }

                public static int Determinant(int[,] matrix)
                {
                    int ans = 0;
                    if (matrix.GetLength(0) == matrix.GetLength(1))
                    {
                        int length = matrix.GetLength(0);
                        if (length > 2)
                        {
                            int[,] tempMat = new int[length - 1, length - 1];
                            for (int j = 0; j < length; j++)
                            {
                                int x = 0, y;
                                for (int i1 = 1; i1 < length; i1++)
                                {
                                    y = 0;
                                    for (int j1 = 0; j1 < length; j1++)
                                    {
                                        if (j1 != j)
                                        {
                                            tempMat[x, y] = matrix[i1, j1];
                                            y++;
                                        }
                                    }
                                    x++;
                                }
                                ans += (int)Math.Pow(-1, j) * matrix[0, j] * Determinant(tempMat);
                            }
                            return ans;
                        }
                        else if (length == 2)
                        {
                            ans = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
                            return ans;
                        }
                        else if (length == 1)
                        {
                            return matrix[0, 0];
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        throw new CMatrixException("This Matrix doesn't has Determinant");
                    }
                }

                /*!
                 * Transpose current matrix
                 */
                public static void Transpose(int[,] matrix)
                {
                    int tempVal;
                    if (matrix.GetLength(0) == matrix.GetLength(1))
                    {
                        int length = matrix.GetLength(0);
                        for (int i = 0; i < length; i++)
                        {
                            for (int j = i; j < length; j++)
                            {
                                tempVal = matrix[i, j];
                                matrix[i, j] = matrix[j, i];
                                matrix[j, i] = tempVal;
                            }
                        }
                    }
                    else
                    {
                        throw new CMatrixException("This Matrix can't be transpose");
                    }
                }

                public static double[,] Inverse(int[,] matrix)
                {
                    if (matrix.GetLength(0) == matrix.GetLength(1))
                    {
                        int length = matrix.GetLength(0);
                        if (length > 1)
                        {
                            double[,] tempAnsMat = new double[length, length];
                            double ans = 0;

                            int[,] tempMat = new int[length - 1, length - 1];
                            for (int i = 0; i < length; i++)
                            {
                                for (int j = 0; j < length; j++)
                                {
                                    int x = 0, y;
                                    for (int i1 = 0; i1 < length; i1++)
                                    {
                                        if (i != i1)
                                        {
                                            y = 0;
                                            for (int j1 = 0; j1 < length; j1++)
                                            {
                                                if (j1 != j)
                                                {
                                                    tempMat[x, y] = matrix[i1, j1];
                                                    y++;
                                                }
                                            }
                                            x++;
                                        }
                                    }
                                    //It is saved as transpose matrix in temperary matrix
                                    tempAnsMat[j, i] = Math.Pow(-1, i + j) * Determinant(tempMat);
                                    if (i == 0)
                                        ans += matrix[i, j] * tempAnsMat[j, i];
                                }
                            }
                            if (ans != 0)
                                return ScalarMultiply(1 / ans, tempAnsMat);
                            else
                                throw new CMatrixException("This matrix Determiant is 0. no inverse matrix");
                        }
                        else if (length == 1)
                            return new double[,] { { 0 } };
                        else
                            throw new CMatrixException("This is a Null matrix");
                    }
                    else
                    {
                        throw new CMatrixException("This Matrix can't be inverse");
                    }
                }
            }
            public class CMatrixException : Exception
            {
                public CMatrixException()
                {
                    errorMsg = "Input matrix is wrong";
                }
                public CMatrixException(string message)
                {
                    errorMsg = message;
                }
                public string GetErrorMessage()
                {
                    return errorMsg;
                }
                private string errorMsg;
            }
            public void test()
            {
                
            }
        }
        public class FilterBase<T>
        {
            public FilterBase()
            {
                Init();
            }

            public virtual void Init()
            {

            }

            public virtual void DeInit()
            {

            }

            public virtual bool Do(T input, ref T output)
            {
                throw new NotImplementedException();
            }
        }
        #endregion Kalman Filter - made by Dongjune Chang from the book - "MATLAB 활용 칼만필터의 이해(저자 김성필)"
    }
}
