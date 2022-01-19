using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Linq;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CConvert
        {
            // https://stackoverflow.com/questions/6088241/is-there-a-way-to-check-whether-unicode-text-is-in-a-certain-language
            public static bool IsChinese(string text) 
            {
		        var charArray = text.ToCharArray ();
		        var isChineseTextPresent = false;

		        foreach (var character in charArray) {
			        var cat = char.GetUnicodeCategory (character);

			        if (cat != UnicodeCategory.OtherLetter) {
				        continue;
			        }

			        isChineseTextPresent = true;
			        break;
		        }

		        return isChineseTextPresent;
            }
            // RichTextBox / TextBox 의 현재 커서가 있는 위치의 라인의 위치를 읽어오기
            public static int GetCurrentLine_From_RichTextBox(System.Windows.Forms.RichTextBox rtxt) { return rtxt.GetLineFromCharIndex(rtxt.GetFirstCharIndexOfCurrentLine()); }
            public static int GetCurrentLine_From_TextBox(System.Windows.Forms.TextBox txt) { return txt.GetLineFromCharIndex(txt.GetFirstCharIndexOfCurrentLine()); }
            public static bool IsValidAlpha(byte byData) { return (((char)(byData) < (char)(' ')) || (byData >= 127)) ? false : true; }
            // Check Numeric or ...
            public static bool IsDigit(char cValue) { if (!Char.IsNumber(cValue)) return false; else return true; }
            public static bool IsDigit(string strValue)
            {               
                bool bSimbol = false;
                bool bPoint = false;
                int nPoint = -1;
                int i = 0;
                foreach (char cItem in strValue) 
                {
                    if ((cItem == '-') || (cItem == '+'))
                    {
                        if (bSimbol == true) return false;
                        else if (bPoint == true) return false;
                        else if (i != 0) return false;
                        bSimbol = true;
                    }
                    else
                    {
                        if (nPoint >= 0)
                        {
                            if (cItem == '.')
                            {
                                nPoint++;
                                bPoint = true;
                            }
                            else bPoint = false;
                        }

                        if (cItem != '.')
                        {
                            bSimbol = false;
                            if (!Char.IsNumber(cItem))
                                return false;
                        }
                    }
                    if (nPoint < 0) nPoint = 0;
                    i++;
                }
                if (bSimbol == true) return false;
                else if (nPoint > 1) return false;
                else if (bPoint == true) return false;
                return true;
            }
            
            #region Check String Separation(Kor: 스트링 구분 및 판단하기) - compiler only

            private static int _EQ = 0x0000001;
            private static int _PLUS = 0x0000002;
            private static int _MINUS = 0x0000004;
            private static int _MUL = 0x0000008;

            private static int _DIV = 0x0000010;
            private static int _SIN = 0x0000020;
            private static int _COS = 0x0000040;
            private static int _TAN = 0x0000080;

            private static int _ASIN = 0x0000100;
            private static int _ACOS = 0x0000200;
            private static int _ATAN = 0x0000400;
            private static int _SQRT = 0x0000800;

            private static int _POW = 0x0001000;
            private static int _ABS = 0x0002000;
            private static int _MOD = 0x0004000;

            private static int _BRACKET_SMALL_START = 0x0004000;
            private static int _BRACKET_SMALL_END = 0x0008000;
            private static int _BRACKET_MIDDLE_START = 0x0010000;

            private static int _BRACKET_MIDDLE_END = 0x0020000;
            private static int _BRACKET_LARGE_START = 0x0040000;
            private static int _BRACKET_LARGE_END = 0x0080000;
            private static int _COMMA = 0x0100000;

            private static int _DIGIT = 0x0200000;
            private static int _ALPHA = 0x0400000;
            private static int _ATAN2 = 0x0800000;
            private static int _ACOS2 = 0x1000000;
            private static int _ASIN2 = 0x2000000;
            //private static int _MOD = 0x4000000; // 이미 위에 선언
            private static int _ROUND = 0x8000000;
            private static int _CALL  =0x10000000;

            private static int _COMMA2 = 0x0100000;

            public static void StringSeparate(String strSrc, out String[] pstrData)
            {
                try
                {
                    String[] pstrData2 = new string[256];
                    pstrData2.Initialize();

                    int nNum = 0;
                    String strPrev = "";
                    String strCurr = "";
                    for (int i = 0; i < strSrc.Length; i++)
                    {
                        if (i > 0)
                        {
                            //if (CheckSeparate(strPrev, strCurr) == true)
                            String strTmp = "";
                            strTmp += strSrc[i];
                            if (CheckSeparate(strCurr, strTmp) == true)
                            {
                                pstrData2[nNum++] = strCurr;
                                strPrev = strCurr;
                                strCurr = "";
                            }
                        }
                        strCurr += strSrc[i];
                    }
                    if (strCurr != "") pstrData2[nNum++] = strCurr;
                    pstrData = new string[nNum];
                    Array.Copy(pstrData2, 0, pstrData, 0, nNum);
                    pstrData2 = null;
                }
                catch// (System.Exception e)
                {
                    pstrData = null;
                }
            }
            public static void StringSeparate_Comma(String strSrc, out String[] pstrData)
            {
                try
                {
                    String[] pstrData2 = new string[256];
                    pstrData2.Initialize();

                    int nNum = 0;
                    String strPrev = "";
                    String strCurr = "";
                    for (int i = 0; i < strSrc.Length; i++)
                    {
                        if (i > 0)
                        {
                            //if (CheckSeparate(strPrev, strCurr) == true)
                            String strTmp = "";
                            strTmp += strSrc[i];
                            if (CheckSeparate_Comma(strCurr, strTmp) == true)
                            {
                                pstrData2[nNum++] = strCurr;
                                strPrev = strCurr;
                                strCurr = "";
                            }
                        }
                        strCurr += strSrc[i];
                    }
                    if (strCurr != "") pstrData2[nNum++] = strCurr;
                    pstrData = new string[nNum];
                    Array.Copy(pstrData2, 0, pstrData, 0, nNum);
                    pstrData2 = null;
                }
                catch// (System.Exception e)
                {
                    pstrData = null;
                }
            }
            public static String StringSeparate_In_Compiler(String strSrc, out String[] pstrData)
            {
                try
                {
                    String[] pstrData2 = new string[1024];//256];
                    pstrData2.Initialize();

                    int nNum = 0;
                    String strPrev = "";
                    String strCurr = "";
                    bool bSqrt = false;
                    bool bPow = false;
                    bool bRound = false;
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
                            if (CheckSeparate(strCurr, strTmp) == true)
                            {
                                if ((strCurr == "X") || (strCurr == "x")) strCurr = "_X";
                                if ((strCurr == "Y") || (strCurr == "y")) strCurr = "_Y";
                                if ((strCurr == "Z") || (strCurr == "z")) strCurr = "_Z";

                                if (i == strSrc.Length - 1)
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
                                    (strCurr.ToLower().IndexOf("sqrt") == 0) ||
                                    (strCurr.ToLower().IndexOf("round") == 0)
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
                                if (strCurr.ToLower() == "round") bRound = true;
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
                                    else if (bRound == true) strCurr = ",_UP5_";
                                    bSqrt = false;
                                    bPow = false;
                                    bAtan2 = false;
                                    bAcos2 = false;
                                    bAsin2 = false;
                                    bRound = false;
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
                    return null;
                }
                catch (System.Exception e)
                {
                    //m_nErrorCode = 9;
                    //m_strError_Etc += "[StringSeparate]" + e.ToString();
                    pstrData = null;
                    return e.ToString();
                }
            }
            public static bool CheckOperation(String strData)
            {
                return ((strData == "+") || (strData == "-") || (strData == "*") || (strData == "/")) ? true : false;
            }
            public static bool CheckOperation(char cData)
            {
                return ((cData == '+') || (cData == '-') || (cData == '*') || (cData == '/')) ? true : false;
            }

            public static bool CheckSeparate_Comma(String strPrev, String strCurr)
            {
                return (CheckCalc_Compare(_COMMA, strPrev) != 0) ? true : false;
            }
            // it refers to no separation when it makes a "Digit-Alpha" or "Alpha-Digit". (This is Alpha)
            // Kor: Digit 다음에 Alpha 가 오는 경우, 혹은 Alpha 다음에 Digit이 오는 경우( 이 경우는 Alpha ) Separate 가 발생하지 않은 걸로 친다.
            public static bool CheckSeparate(String strPrev, String strCurr)
            {
                bool bSeparate = false;

                bool bCalc_0 = false;
                bool bCalc_1 = false;
                bool bBracket_0 = false;
                bool bBracket_1 = false;
                bool bFunction_0 = false;
                bool bFunction_1 = false;
                bool bDigit_0 = false;
                bool bDigit_1 = false;
                bool bAlpha_0 = false;
                bool bAlpha_1 = false;
                bool bComma_0 = false;
                bool bComma_1 = false;

                bool bComma2_0 = false;
                bool bComma2_1 = false;

                bool bComplete = false; // _SIN | _COS | _TAN | _ASIN | _ACOS | _SQRT | _POW | _ABS | _ATAN2;
                int nComplete = _SIN | _COS | _TAN | _ASIN | _ACOS | _SQRT | _POW | _ABS | _ATAN2 | _ACOS2 | _ASIN2 | _ROUND | _CALL;

                int nEq = _EQ | _PLUS | _MINUS | _MUL | _DIV | _MOD;
                int nBracket = _BRACKET_SMALL_START | _BRACKET_SMALL_END | _BRACKET_MIDDLE_START | _BRACKET_MIDDLE_END | _BRACKET_LARGE_START | _BRACKET_LARGE_END;
                int nFunction = _SIN | _COS | _TAN | _ASIN | _ACOS | _ATAN | _SQRT | _POW | _ABS | _ATAN2 | _ACOS2 | _ASIN2 | _ROUND | _CALL;
                if (strPrev == null) return false;
                else if (strCurr == "\r\n") return true;
                else if (strCurr == "\r") return true;
                else if (strCurr == "\n") return true;
                //else if (strPrev == "=") return true;
                else if (strCurr == "=") return true;
                else
                {
                    bCalc_0 = (CheckCalc_Compare(nEq, strPrev) != 0) ? true : false;
                    bCalc_1 = (CheckCalc_Compare(nEq, strCurr) != 0) ? true : false;
                    bBracket_0 = (CheckCalc_Compare(nBracket, strPrev) != 0) ? true : false;
                    bBracket_1 = (CheckCalc_Compare(nBracket, strCurr) != 0) ? true : false;
                    bFunction_0 = (CheckCalc_Compare(nFunction, strPrev) != 0) ? true : false;
                    bFunction_1 = (CheckCalc_Compare(nFunction, strCurr) != 0) ? true : false;
                    bDigit_0 = CheckCalc_Digit(strPrev);
                    bDigit_1 = CheckCalc_Digit(strCurr);
                    bAlpha_0 = CheckCalc_Alpha(strPrev);
                    bAlpha_1 = CheckCalc_Alpha(strCurr);
                    bComma_0 = (CheckCalc_Compare(_COMMA, strPrev) != 0) ? true : false;
                    bComma_1 = (CheckCalc_Compare(_COMMA, strCurr) != 0) ? true : false;

                    bComma2_0 = (CheckCalc_Compare(_COMMA2, strPrev) != 0) ? true : false;
                    bComma2_1 = (CheckCalc_Compare(_COMMA2, strCurr) != 0) ? true : false;

                    // 완전한 문장이 앞에 있다면 구분
                    bComplete = (CheckCalc_Compare(nComplete, strPrev) != 0) ? true : false;
                }

                // Error 상황
                //if ((bCalc_0 == true) && (bCalc_1 == true)) bSeparate = false;

                // 정상상황
                //if ((bCalc_0 == true) && (bCalc_1 == false)) bSeparate = true;
                //if ((bCalc_0 == false) && (bCalc_1 == true)) bSeparate = true;
                if ((bCalc_0 == true) || (bCalc_1 == true)) bSeparate = true;
                if ((bComma_0 == true) || (bComma_1 == true)) bSeparate = true;
                if ((bBracket_0 == true) || (bBracket_1 == true)) bSeparate = true;
                //if ((bFunction_0 == true) || (bFunction_1 == true)) bSeparate = true;
                if ((bFunction_0 == true) && (bBracket_1 == true)) bSeparate = true;
                if ((bComma2_0 == true) || (bComma2_1 == true)) bSeparate = true;
                if ((bComplete == true) && (bBracket_1 == true)) bSeparate = true;
                if ((bFunction_0 == true) && (strCurr == "_")) bSeparate = true;

                return bSeparate;
            }
            public static bool CheckCalc_Alpha(String strData)
            {
                if (strData == null) return false;
                if (strData.Length <= 0) return false;
                bool bRet = false;
                bool bAlpha = false;
                //bool bDigit = false;
                bool bFirstDigit = false;
                bool bPoint = false;
                bool bDoublePoint = false;
                for (int i = 0; i < strData.Length; i++)
                {
                    if ((strData[i] >= 0x30) && (strData[i] <= 0x39)) { /*bDigit = true;*/ if (i == 0) bFirstDigit = true; }
                    if ((strData[i] == '_') || ((strData[i] >= 0x41) && (strData[i] <= 0x5A)) || ((strData[i] >= 0x61) && (strData[i] <= 0x7a)))
                    {
                        if (bFirstDigit == false) bAlpha = true;
                    }
                    if (strData[i] == '.')
                    {
                        if (bPoint == false) bPoint = true;
                        else bDoublePoint = true;
                    }
                }
                if (bDoublePoint == false)
                {
                    if ((bAlpha == true) && (bPoint == false)) bRet = true;
                }
                return bRet;
            }
            public static bool CheckCalc_Digit(String strData)
            {
                if (strData == null) return false;
                if (strData.Length <= 0) return false;
                bool bRet = false;
                bool bAlpha = false;
                bool bDigit = false;
                //bool bFirstDigit = false;
                bool bPoint = false;
                bool bDoublePoint = false;
                for (int i = 0; i < strData.Length; i++)
                {
                    //if ((strData[i] >= 0x30) && (strData[i] <= 0x39)) { bDigit = true; if (i == 0) bFirstDigit = true; }
                    if ((strData[i] == '_') || ((strData[i] >= 0x41) && (strData[i] <= 0x5A)) || ((strData[i] >= 0x61) && (strData[i] <= 0x7a))) bAlpha = true;
                    if (strData[i] == '.')
                    {
                        if (bPoint == false) bPoint = true;
                        else bDoublePoint = true;
                    }
                }
                if (bDoublePoint == false)
                {
                    //                 if (bAlpha == true)
                    //                 {
                    //                     if (bPoint == false) bRet = true;
                    //                 }
                    //                 else if (bDigit == true) bRet = true;
                    if ((bDigit == true) && (bAlpha == false)) bRet = true;
                }
                return bRet;
            }
            public static int CheckCalc_Compare(int nCompare, String strData)
            {
                int nRet = 0x00; // 0b ___0 0000 0000 0000 
                //bool bFunction = false;
                //
                if ((nCompare & _EQ) != 0) { if (strData == "=") nRet |= _EQ; }
                if ((nCompare & _PLUS) != 0) { if (strData == "+") nRet |= _PLUS; }
                if ((nCompare & _MINUS) != 0) { if (strData == "-") nRet |= _MINUS; }
                if ((nCompare & _MUL) != 0) { if (strData == "*") nRet |= _MUL; }
                if ((nCompare & _DIV) != 0) { if (strData == "/") nRet |= _DIV; } // 0x0b [____ ____] [____ ____] [___1 1111]
                if ((nCompare & _MOD) != 0) { if (strData == "%") nRet |= _MOD; } 

                if ((nCompare & _SIN) != 0) { if (strData == "sin") nRet |= _SIN; }
                if ((nCompare & _COS) != 0) { if (strData == "cos") nRet |= _COS; }
                if ((nCompare & _TAN) != 0) { if (strData == "tan") nRet |= _TAN; }
                //
                if ((nCompare & _ASIN) != 0) { if (strData == "asin") nRet |= _ASIN; }
                if ((nCompare & _ACOS) != 0) { if (strData == "acos") nRet |= _ACOS; }
                if ((nCompare & _ATAN) != 0) { if (strData == "atan") nRet |= _ATAN; }
                if ((nCompare & _ATAN2) != 0) { if (strData == "atan2") nRet |= _ATAN2; }
                if ((nCompare & _ACOS2) != 0) { if (strData == "acos2") nRet |= _ACOS2; }
                if ((nCompare & _ASIN2) != 0) { if (strData == "asin2") nRet |= _ASIN2; }
                if ((nCompare & _ROUND) != 0) { if (strData == "round") nRet |= _ROUND; }
                if ((nCompare & _SQRT) != 0) { if (strData == "sqrt") nRet |= _SQRT; }
                if ((nCompare & _POW) != 0) { if (strData == "pow") nRet |= _POW; } // 0x0b [____ ____] [___1 1111] [111_ ____]
                if ((nCompare & _ABS) != 0) { if (strData == "abs") nRet |= _ABS; }
                if ((nCompare & _CALL) != 0) { if (strData == "call") nRet |= _CALL; }
                

                //if ((strData == "sin") || (strData == "cos") || (strData == "tan") || (strData == "asin") || (strData == "acos") || (strData == "atan") || (strData == "sqrt") || (strData == "pow") || (strData == "abs")) bFunction = true;

                if ((nCompare & _BRACKET_SMALL_START) != 0) { if (strData == "(") nRet |= _BRACKET_SMALL_START; }
                if ((nCompare & _BRACKET_SMALL_END) != 0) { if (strData == ")") nRet |= _BRACKET_SMALL_END; }
                if ((nCompare & _BRACKET_MIDDLE_START) != 0) { if (strData == "{") nRet |= _BRACKET_MIDDLE_START; }
                //
                if ((nCompare & _BRACKET_MIDDLE_END) != 0) { if (strData == "}") nRet |= _BRACKET_MIDDLE_END; }
                if ((nCompare & _BRACKET_LARGE_START) != 0) { if (strData == "[") nRet |= _BRACKET_LARGE_START; }
                if ((nCompare & _BRACKET_LARGE_END) != 0) { if (strData == "]") nRet |= _BRACKET_LARGE_END; } // 0x0b [____ _111] [111_ ____] [____ ____]
                if ((nCompare & _COMMA) != 0) { if (strData == ",") nRet |= _COMMA; } // 0x0b [____ 1___] [____ ____] [____ ____]

                if ((nCompare & _DIGIT) != 0) { if (CheckCalc_Digit(strData) == true) nRet |= _DIGIT; } // Digit
                if ((nCompare & _ALPHA) != 0) { if (CheckCalc_Alpha(strData) == true) nRet |= _ALPHA; } // Alpha('_' 포함)
                // 0x0b [__11 ____] [____ ____] [____ ____]

                if ((nRet & (_SIN | _COS | _TAN | _ASIN | _ACOS | _ATAN | _SQRT | _POW | _ABS | _ATAN2 | _ACOS2 | _ASIN2 | _ROUND | _CALL)) != 0)
                    nRet &= ((0x7fffffff ^ (_DIGIT | _ALPHA)) & 0x7fffffff);

                if ((nCompare & _COMMA2) != 0) { if (strData == ";") nRet |= _COMMA2; } // 0x0b [___1] [____ ____] [____ ____] [____ ____]

                return nRet;
            }
            #endregion Check String Separation(Kor: 스트링 구분 및 판단하기)

            // Color
            public static int ColorToInt(Color cColor) { return cColor.ToArgb(); }
            public static Color IntToColor(int nColor) { return Color.FromArgb(nColor); }

            // Clipping function
            public static int Clip(int nLimitValue_Up, int nLimitValue_Dn, int nData)
            {
                int nRet = ((nData > nLimitValue_Up) ? nLimitValue_Up : nData);
                return ((nRet < nLimitValue_Dn) ? nLimitValue_Dn : nRet);
            }
            public static float Clip(float fLimitValue_Up, float fLimitValue_Dn, float fData)
            {
                float fRet = ((fData > fLimitValue_Up) ? fLimitValue_Up : fData);
                return ((fRet < fLimitValue_Dn) ? fLimitValue_Dn : fRet);
            }
            public static double Clip(double dLimitValue_Up, double dLimitValue_Dn, double dData)
            {
                double dRet = ((dData > dLimitValue_Up) ? dLimitValue_Up : dData);
                return ((dRet < dLimitValue_Dn) ? dLimitValue_Dn : dRet);
            }

            // Remove all from "//"
            // Kor: 라인주석 "//" 이후를 없애는 명령어
            public static String RemoveCaption(String strData, bool bRemoveSpace, bool bRemoveNullLine)
            {
                return RemoveCaption("//", strData, bRemoveSpace, bRemoveNullLine);
            }
            public static String RemoveCaption(
                string strDefinedCaption, // 지정한 캡션 지우기
                String strData, bool bRemoveSpace, bool bRemoveNullLine)
            {
                string strRet = "";
                String strTmp = strData;//.ToUpper();
                if (bRemoveSpace == true) strTmp = RemoveChar(strTmp, ' ');
                strTmp = RemoveChar(strTmp, '\r');
                String[] pstrTmp = strTmp.Split('\n');
                int nIndex;
                foreach (String strItem in pstrTmp)
                {
                    nIndex = strItem.IndexOf(strDefinedCaption);
                    if (nIndex >= 0)
                    {
                        strTmp = strItem.Substring(0, nIndex);
                    }
                    else strTmp = strItem;
                    if (bRemoveNullLine == true)
                    {
                        if (strTmp.Length > 0) strRet += strTmp + "\r\n";
                    }
                    else strRet += strTmp + "\r\n";
                }
                pstrTmp = null;
                return strRet;
            }

            // Check the number of letters - redundant except(Kor: 해당 문자가 몇개나 있는지 갯수 세기 - 중복 제외)
            public static int GetCnt(String strText, String strCheck)
            {
                int nWidth = strCheck.Length;
                int nCnt = 0;
                String strData = "";
                for (int i = 0; i < strText.Length; i++)
                {
                    strData += strText[i];
                    int nIndex = strData.IndexOf(strCheck);
                    if (nIndex >= 0)
                    {
                        strData = strData.Remove(nIndex);
                        nCnt++;
                    }
                }
                return nCnt;
            }

            // Partially replacing characters(Kor: 부분적 문자 삭제)
            public static string RemoveChar(string strText, char cRemoveChar)
            {
                string strResult = strText;
                int nIndex;
                while ((nIndex = strResult.IndexOf(cRemoveChar)) >= 0)
                {
                    strResult = strResult.Remove(nIndex, 1);
                }
                return strResult;
            }
            public static string ChangeChar(string strText, char cRemoveChar, char cNewChar)
            {
                return strText.Replace(cRemoveChar, cNewChar);
            }

            // Partially replacing characters(Kor: 부분적 문자 삭제)
            public static string RemoveString(string strText, String strRemoveData)
            {
                string strResult = strText;
                int nIndex;
                int nStart = 0;
                int nW = strRemoveData.Length;
                while ((nIndex = strResult.IndexOf(strRemoveData, nStart)) >= 0)
                {
                    StringBuilder stb = new StringBuilder(strResult);

                    strResult = stb.Remove(nIndex, nW).ToString();// strResult.Remove(nIndex, nW);
                    if (nIndex >= nStart)
                        nStart = nIndex;
                    else break;
                }
                return strResult;
            }

            // Partially replacing characters(Caution example: 'V1' == 'V11')
            // Kor: 부분적 문자 교체(v1 v11 의 경우 같은 문자로 취급될 수 있다.)
            public static string ChangeString(string strText, String strRemoveData, String strChangeData)
            {                
#if true
                return strText.Replace(strRemoveData, strChangeData);
#else
                
                string strResult = strText;
                int nIndex;
                int nStart = 0;
                int nW = strRemoveData.Length;
                while ((nIndex = strResult.IndexOf(strRemoveData, nStart)) >= 0)
                {
                    //strResult = strResult.Remove(nIndex, nW);
                    strResult = strResult.Substring(0, nIndex) + strChangeData + strResult.Substring(nIndex + nW, strResult.Length - nIndex - nW);
                    if (nIndex > nStart)
                        nStart = nIndex;
                    else break;
                }
                return strResult;
#endif
            }

            // Partially replacing characters(Caution example: 'V1' != 'V11')
            // Kor: 부분적 문자 교체(v1 v11 을 같은 문자로 취급하지 않는다.)
            public static string ChangeString_In_MathFunction(string strText, String strRemoveData, String strChangeData)
            {
                String[] pstrTmp;
                StringSeparate(strText, out pstrTmp);
                String strRet = "";
                foreach (String strItem in pstrTmp)
                {
                    strRet += ((strItem == strRemoveData) ? strChangeData : strItem);
                }
                pstrTmp = null;
                return strRet;
            }

            // Partially replacing characters(Caution example: 'V1' != 'V11') - only using with compiler
            // Kor: 부분적 문자 교체(v1 v11 을 같은 문자로 취급하지 않는다.) - 컴파일러 전용
            public static string ChangeString_In_MathFunction_By_Compiler(string strText, String strRemoveData, String strChangeData)
            {
                String strExcept = "_UP";
                String[] pstrTmp;
                StringSeparate(strText, out pstrTmp);
                String strRet = "";
                foreach (String strItem in pstrTmp)
                {
                    if (strItem.IndexOf(strExcept) >= 0)
                    {
                        String strTmp = CConvert.RemoveString(strItem, strExcept);

                        String strFirst = strItem.Substring(0, strExcept.Length);
                        strFirst += strTmp.Substring(0, strTmp.IndexOf("_") + 1);

                        strTmp = strTmp.Substring(strTmp.IndexOf("_") + 1);
                        strRet += strFirst + ((strTmp == strRemoveData) ? strChangeData : strTmp);
                    }
                    else strRet += ((strItem == strRemoveData) ? strChangeData : strItem);
                }
                pstrTmp = null;
                return strRet;
            }

            public static string FillString(String strOrg, String strSpace, int nStringWidth, bool bRight)
            {
                String strRet = ""; ;

                for (int i = 0; i < (nStringWidth - strOrg.Length); i++) strRet += strSpace;
                if (bRight == true) strRet = strOrg + strRet;
                else strRet = strRet + strOrg;

                return strRet;
            }

            public static bool[] Array_Bool_Init(bool value, int nLength) { return Enumerable.Repeat<bool>(value, nLength).ToArray<bool>(); }
            public static int[] Array_Int_Init(int value, int nLength) { return Enumerable.Repeat<int>(value, nLength).ToArray<int>(); }
            public static float[] Array_Float_Init(float value, int nLength) { return Enumerable.Repeat<float>(value, nLength).ToArray<float>(); }
            public static short[] Array_Short_Init(short value, int nLength) { return Enumerable.Repeat<short>(value, nLength).ToArray<short>(); }
            public static string[] Array_String_Init(string value, int nLength) { return Enumerable.Repeat<string>(value, nLength).ToArray<string>(); }

            public static void Array_Bool_Init(ref bool [] Datas, bool value, int nLength) { Datas = Enumerable.Repeat<bool>(value, nLength).ToArray<bool>(); }
            public static void Array_Int_Init(ref int[] Datas, int value, int nLength) { Datas = Enumerable.Repeat<int>(value, nLength).ToArray<int>(); }
            public static void Array_Float_Init(ref float[] Datas, float value, int nLength) { Datas = Enumerable.Repeat<float>(value, nLength).ToArray<float>(); }
            public static void Array_Short_Init(ref short[] Datas, short value, int nLength) { Datas = Enumerable.Repeat<short>(value, nLength).ToArray<short>(); }
            public static void Array_String_Init(ref string[] Datas, string value, int nLength) { Datas = Enumerable.Repeat<string>(value, nLength).ToArray<string>(); }
            
            //public static object[] ArrayInit(object value, int nLength) { return Enumerable.Repeat<object>(value, nLength).ToArray<object>(); }
            // Byte Array -> String
            public static string BytesToStr(byte[] byteArrayData, int nCnt)
            {
                byte[] byteArrayData2 = new byte[nCnt];
                for (int i = 0; i < nCnt; i++)
                    byteArrayData2[i] = byteArrayData[i];
                String strTmp = System.Text.Encoding.Default.GetString(byteArrayData2);
                byteArrayData2 = null;
                return strTmp;
            }

            // Byte Array -> String
            public static string BytesToStr(byte[] byteArrayData, int nPos, int nCnt)
            {
                byte[] byteArrayData2 = new byte[nCnt];
                for (int i = 0; i < nCnt; i++)
                    byteArrayData2[i] = byteArrayData[i + nPos];

                String strTmp = System.Text.Encoding.Default.GetString(byteArrayData2);
                byteArrayData2 = null;
                return strTmp;
            }

            #region From Koreadtech user.. 2015/01/26 => http://koreatech.tistory.com/264
            #region BytesToStruct
            /*
            // However, should adhere to the rules below, the structure declaration.
            // Kor: 단, 구조체 선언시 하기의 규칙을 지켜야 한다.
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
            struct PACKET_DATA
            {
                public int PacketType;         // 패킷 타입
                public int TotalBytes;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)] 
                public byte[] Data;//byte배열 길이 1024
            }
            */
            //public static object Dictionary_GetKey(Dictionary<object, object> dic, object value)
            //{
            //    return dic.FirstOrDefault(x => x.Value == value).Key;
            //}
            //public static object Dictionary_GetValue(Dictionary<object, object> dic, object key)
            //{
            //    return dic[key];
            //}

            // ByteToStructure(bytebuffer[], typeof(SUser_t))
            public static object ByteToStructure(byte[] data, Type type)
            {
                IntPtr buff = Marshal.AllocHGlobal(data.Length); // by the size of the array to allocate memory in unmanaged memory.(Kor: 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당한다.)
                Marshal.Copy(data, 0, buff, data.Length); // Copy the memory area allocated to data stored on the array.(Kor: 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사한다.)
                object obj = Marshal.PtrToStructure(buff, type); // And converts the data copied to the structure(Kor: 복사된 데이터를 구조체 객체로 변환한다.)
                Marshal.FreeHGlobal(buff); // free unmanaged memory(Kor: 비관리 메모리 영역에 할당했던 메모리를 해제함)

                if (Marshal.SizeOf(obj) != data.Length) return null;
                return obj; 
            }
            #endregion BytesToStruct
            #region StructToBytes
            public static byte[] StructureToByte(object obj)
            {
                int datasize = Marshal.SizeOf(obj);//((PACKET_DATA)obj).TotalBytes; // get the structure memory size (Kor: 구조체에 할당된 메모리의 크기를 구한다.)
                IntPtr buff = Marshal.AllocHGlobal(datasize); // Allocates memory as much as the size of the structure in unmanaged memory.(Kor: 비관리 메모리 영역에 구조체 크기만큼의 메모리를 할당한다.)
                Marshal.StructureToPtr(obj, buff, false); // To obtain the address of the allocated variables.(Kor: 할당된 구조체 객체의 주소를 구한다.)
                byte[] data = new byte[datasize]; 
                Marshal.Copy(buff, data, 0, datasize); // structure -> array
                Marshal.FreeHGlobal(buff); // free unmanaged memory(Kor: 비관리 메모리 영역에 할당했던 메모리를 해제함)
                return data; 
            }
            #endregion StructToBytes
            #endregion From Koreadtech user.. 2015/01/26 => http://koreatech.tistory.com/264

            // To Upper
            public static string ToUpper(string strText) { return strText.ToUpper(); }

            // To Lower
            public static string ToLower(string strText) { return strText.ToLower(); }

            // remove letters '$' and space(Kor: '$' 로 표시된 문자에서 공백과 '$' 문자 제거)
            public static String S_StrTrim(String strData)
            {
                try
                {
                    String strTmp;
                    strTmp = strData.Trim();
                    strTmp = strTmp.Trim('$');
                    strTmp = strTmp.Trim();

                    strTmp = strTmp.PadLeft(2, '0');
                    //strTmp = strTmp.Substring(strTmp.Length - 2);

                    return strTmp;
                }
                catch
                {
                    return "";
                }
            }

            /////////// Hexadecimal characters(Kor: 16진수문자)(HexStr) ////////////
            // Change this number(include '$') to hexadecimal(Kor: '$' 로 표시된 문자를 Hexa로 인식하고 숫자로 변경)
            public static int S_HexStrToInt(String strData)
            {
                try
                {
                    String strTmp;
                    int nTmp;

                    strTmp = strData.Trim();

                    nTmp = strTmp.IndexOf('$');
                    if (nTmp >= 0)
                    {
                        strTmp = S_StrTrim(strTmp);
                        return int.Parse(strTmp, System.Globalization.NumberStyles.HexNumber);
                    }
                    else return int.Parse(strTmp);
                }
                catch
                {
                    return -1;
                }
            }
            public static long S_HexStrToLong(String strData)
            {
                try
                {
                    String strTmp;
                    long lTmp;

                    strTmp = strData.Trim();

                    lTmp = strTmp.IndexOf('$');
                    if (lTmp >= 0)
                    {
                        strTmp = S_StrTrim(strTmp);
                        return long.Parse(strTmp, System.Globalization.NumberStyles.HexNumber);
                    }
                    else return long.Parse(strTmp);
                }
                catch
                {
                    return -1;
                }
            }

            // '$' characters(Hexa characters) -> byte(Kor: '$'로 표시된 문자를 Hexa로 인식하고 Byte로 변경)
            public static Byte S_HexStrToByte(String strData)
            {
                try
                {
                    String strTmp;
                    int nIndex;

                    strTmp = strData.Trim();

                    nIndex = strTmp.IndexOf('$');
                    if (nIndex >= 0)
                    {
                        strTmp = S_StrTrim(strTmp);
                        return Byte.Parse(strTmp, System.Globalization.NumberStyles.HexNumber);
                    }
                    else return Byte.Parse(strTmp);
                }
                catch
                {
                    return 0;
                }
            }

            // HEXA string -> Numeric
            private static string _Remove_0x(string str)
            {
                if (CConvert.IsDigit(str) == false)
                {
                    int nFind = str.IndexOf("0x");
                    if (nFind >= 0) return str.Substring(nFind + 2);
                }
                return str;
            }
            public static int HexStrToInt(String strData)
            {
                try
                {
                    return int.Parse(_Remove_0x(S_StrTrim(strData)), System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    return 0;
                }
            }
            public static long HexStrToLong(String strData)
            {
                try
                {
                    return long.Parse(_Remove_0x(S_StrTrim(strData)), System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    return 0;
                }
            }
            public static Byte HexStrToByte(String strData)
            {
                try
                {
                    return Byte.Parse(_Remove_0x(S_StrTrim(strData)), System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    return 0;
                }
            }

            // Changing the Hexa-value to the character of the two-digit(Kor: Hexa 값을 두자릿수의 문자로 바꿈)
            public static String IntToHex(int nData)
            {
                String strTmp = Convert.ToString(nData, 16);

                strTmp = strTmp.PadLeft(2, '0');
                //strTmp = strTmp.Substring(strTmp.Length - 2);
                strTmp = strTmp.Trim();

                return strTmp;
            }
            public static String LongToHex(long lData)
            {
                String strTmp = Convert.ToString(lData, 16);

                strTmp = strTmp.PadLeft(2, '0');
                //strTmp = strTmp.Substring(strTmp.Length - 2);
                strTmp = strTmp.Trim();

                return strTmp;
            }

            // Hexa(int) -> Hexa string(Kor: Hexa값을 지정한 자릿수만큼의 문자로 바꿈)
            public static String IntToHex(int nData, int nWidth)
            {
                String strTmp = Convert.ToString(nData, 16);

                strTmp = strTmp.PadLeft(nWidth, '0');
                strTmp = strTmp.Substring(strTmp.Length - nWidth);
                strTmp = strTmp.Trim();

                return strTmp;
            }
            // Hexa(Long) -> Hexa string(Kor: Hexa값을 지정한 자릿수만큼의 문자로 바꿈)
            public static String LongToHex(long lData, int nWidth)
            {
                String strTmp = Convert.ToString(lData, 16);

                strTmp = strTmp.PadLeft(nWidth, '0');
                strTmp = strTmp.Substring(strTmp.Length - nWidth);
                strTmp = strTmp.Trim();

                return strTmp;
            }

            // Hexa -> 8 digit binary characters(Kor: Hexa 값을 8자리의 바이너리 문자로 바꿈)
            public static String IntToBinary(int nData) // 나중에 고칠것
            {
                String strTmp = "";
                strTmp += IntToStr((nData >> 7) & 0x01);
                strTmp += IntToStr((nData >> 6) & 0x01);
                strTmp += IntToStr((nData >> 5) & 0x01);
                strTmp += IntToStr((nData >> 4) & 0x01);
                strTmp += IntToStr((nData >> 3) & 0x01);
                strTmp += IntToStr((nData >> 2) & 0x01);
                strTmp += IntToStr((nData >> 1) & 0x01);
                strTmp += IntToStr(nData & 0x01);

                return strTmp;
            }
            public static String LongToBinary(long lData) 
            {
                String strTmp = "";
                strTmp += IntToStr((lData >> 7) & 0x01);
                strTmp += IntToStr((lData >> 6) & 0x01);
                strTmp += IntToStr((lData >> 5) & 0x01);
                strTmp += IntToStr((lData >> 4) & 0x01);
                strTmp += IntToStr((lData >> 3) & 0x01);
                strTmp += IntToStr((lData >> 2) & 0x01);
                strTmp += IntToStr((lData >> 1) & 0x01);
                strTmp += IntToStr(lData & 0x01);

                return strTmp;
            }
            public static byte[] IntToBytes(int nData)
            {
                try
                {
                    return BitConverter.GetBytes(nData);
                }
                catch
                {
                    return null;
                }
            }
            public static byte[] ShortToBytes(short sData)
            {
                try
                {
                    return BitConverter.GetBytes(sData);
                }
                catch
                {
                    return null;
                }
            }
            public static byte[] LongToBytes(long lData)
            {
                try
                {
                    return BitConverter.GetBytes(lData);
                }
                catch
                {
                    return null;
                }
            }
            public static byte[] FloatToBytes(float fData)
            {
                try
                {
                    return BitConverter.GetBytes(fData);
                }
                catch
                {
                    return null;
                }
            }
            public static byte[] DoubleToBytes(double dData)
            {
                try
                {
                    return BitConverter.GetBytes(dData);
                }
                catch
                {
                    return null;
                }
            }

            public static int BytesToInt(byte[] pbyData, int nStartIndex)
            {
                try
                {
                    return BitConverter.ToInt32(pbyData, nStartIndex);
                }
                catch
                {
                    return 0;
                }
            }
            public static short BytesToShort(byte[] pbyData, int nStartIndex)
            {
                try
                {
                    return BitConverter.ToInt16(pbyData, nStartIndex);
                }
                catch
                {
                    return 0;
                }
            }
            public static long BytesToLong(byte[] pbyData, int nStartIndex)
            {
                try
                {
                    return BitConverter.ToInt64(pbyData, nStartIndex);
                }
                catch
                {
                    return 0;
                }
            }
            public static double BytesToDouble(byte[] pbyData, int nStartIndex)
            {
                try
                {
                    return BitConverter.ToDouble(pbyData, nStartIndex);
                }
                catch
                {
                    return 0.0;
                }
            }
            public static float BytesToFloat(byte[] pbyData, int nStartIndex)
            {
                try
                {
                    return BitConverter.ToSingle(pbyData, nStartIndex);
                }
                catch
                {
                    return 0.0f;
                }
            }
            public static uint BytesToUInt(byte[] pbyData, int nStartIndex)
            {
                try
                {
                    return BitConverter.ToUInt32(pbyData, nStartIndex);
                }
                catch
                {
                    return 0;
                }
            }
            public static ushort BytesToUShort(byte[] pbyData, int nStartIndex)
            {
                try
                {
                    return BitConverter.ToUInt16(pbyData, nStartIndex);
                }
                catch
                {
                    return 0;
                }
            }
            public static ulong BytesToULong(byte[] pbyData, int nStartIndex)
            {
                try
                {
                    return BitConverter.ToUInt64(pbyData, nStartIndex);
                }
                catch
                {
                    return 0;
                }
            }
            /////////////////////////////////////////////////
            /////////////////////////////////////////////////
            public static String BoolToStr(bool bData)
            {
                try
                {
                    return (bData == true) ? "1" : "0";
                }
                catch
                {
                    return "0";
                }
            }
            public static bool StrToBool(string strData)
            {
                try
                {
                    return (StrToInt(strData) == 0) ? false : true;
                }
                catch
                {
                    return false;
                }
            }
            public static int BoolToInt(bool bData)
            {
                try
                {
                    return (bData == true) ? 1 : 0;
                }
                catch
                {
                    return 0;
                }
            }
            public static bool IntToBool(int nData)
            {
                try
                {
                    return (nData == 0) ? false : true;
                }
                catch
                {
                    return false;
                }
            }
            public static long BoolToLong(bool bData)
            {
                try
                {
                    return (bData == true) ? 1 : 0;
                }
                catch
                {
                    return 0;
                }
            }
            public static bool LongToBool(long lData)
            {
                try
                {
                    return (lData == 0) ? false : true;
                }
                catch
                {
                    return false;
                }
            }
            ////////// Numeric(int) ////////////////////////////
            ////////// String /////////////////////////////////
            public static int StrToInt(String strData)
            {
                try
                {
                    return int.Parse(strData);
                }
                catch
                {
                    return 0;
                }
            }
            public static long StrToLong(String strData)
            {
                try
                {
                    return long.Parse(strData);
                }
                catch
                {
                    return 0;
                }
            }
            public static int StrToInt(String strData, int nDefault)
            {
                try
                {
                    return int.Parse(strData);
                }
                catch
                {
                    return nDefault;
                }
            }
            public static long StrToLong(String strData, long lDefault)
            {
                try
                {
                    return long.Parse(strData);
                }
                catch
                {
                    return lDefault;
                }
            }
            public static float StrToFloat(String strData)
            {
                try
                {
                    return float.Parse(strData);
                }
                catch
                {
                    return 0.0f;
                }
            }
            public static float StrToFloat(String strData, float fDefault)
            {
                try
                {
                    return float.Parse(strData);
                }
                catch
                {
                    return fDefault;
                }
            }
            public static double StrToDouble(String strData)
            {
                try
                {
                    return double.Parse(strData);
                }
                catch
                {
                    return 0.0;
                }
            }
            public static double StrToDouble(String strData, double dDefault)
            {
                try
                {
                    return double.Parse(strData);
                }
                catch
                {
                    return dDefault;
                }
            }
            public static string BytesToStr_UTF8(byte[] pbyteData)
            {
                try
                {
                    return System.Text.Encoding.UTF8.GetString(pbyteData);
                }
                catch
                {
                    return null;
                }
            }
            public static string BytesToStr_Unicode(byte[] pbyteData)
            {
                try
                {
                    return System.Text.Encoding.Unicode.GetString(pbyteData);
                }
                catch
                {
                    return null;
                }
            }
            public static string BytesToStr(byte[] pbyteData)
            {
                try
                {
                    return System.Text.Encoding.Default.GetString(pbyteData);
                }
                catch
                {
                    return null;
                }
            }
            public static byte[] StrToBytes(String strData)
            {
                try
                {
                    return System.Text.Encoding.Default.GetBytes(strData);
                }
                catch
                {
                    return null;
                }
            }
            public static byte[] StrToBytes_Unicode(String strData)
            {
                try
                {
                    return System.Text.Encoding.Unicode.GetBytes(strData);
                }
                catch
                {
                    return null;
                }
            }
            public static byte[] StrToBytes_UTF8(String strData)
            {
                try
                {
                    return System.Text.Encoding.UTF8.GetBytes(strData);
                }
                catch
                {
                    return null;
                }
            }

            /////////////////////////////////////////////////
            ////////// Numeric(int) ////////////////////////////
            // Numeric(Int) -> String
            public static String IntToStr(long lData)
            {
                try
                {
                    return Convert.ToString(lData);
                }
                catch
                {
                    return null;
                }
            }
            // Numeric(Int) -> String(Kor: 숫자를 지정한 자릿수만큼의 문자로 바꿈)
            public static String IntToStr(int nData, int nWidth)
            {
                String strTmp = IntToStr(nData);

                strTmp = strTmp.PadLeft(nWidth, '0');
                strTmp = strTmp.Substring(strTmp.Length - nWidth);
                strTmp = strTmp.Trim();

                return strTmp;
            }
            public static String IntToStr(int nData)
            {
                return IntToStr((long)nData);
            }
            public static String LongToStr(long lData)
            {
                return IntToStr(lData);
            }
            /////////////////////////////////////////////////
            public static String FloatToStr(float fData)
            {
                try
                {
                    return Convert.ToString(fData);
                }
                catch
                {
                    return null;
                }
            }
            public static String FloatToStr(float fData, int nDigits)
            {
                try
                {
                    return Convert.ToString((float)Math.Round(fData, nDigits));
                }
                catch
                {
                    return null;
                }
            }
            public static String DoubleToStr(double dData, int nDigits)
            {
                try
                {
                    return Convert.ToString((double)Math.Round(dData, nDigits));
                }
                catch
                {
                    return null;
                }
            }
            public static String DoubleToStr(double dData)
            {
                try
                {
                    return Convert.ToString(dData);
                }
                catch
                {
                    return null;
                }
            }
            ////////// Numeric(int) ////////////////////////////
            public static byte CheckSum(String strData, int nType)
            {
                if (nType == CDef._CHKSUM_NONE) return (byte)0;
                Byte nData = (Byte)(strData[0]);
                for (int i = 1; i < strData.Length; i++)
                {
                    if (nType == CDef._CHKSUM_AND) nData &= (Byte)(strData[i]);
                    else if (nType == CDef._CHKSUM_OR) nData |= (Byte)(strData[i]);
                    else if (nType == CDef._CHKSUM_XOR) nData ^= (Byte)(strData[i]);
                    else if (nType == CDef._CHKSUM_SUM) nData += (Byte)(strData[i]);
                }
                return (byte)(nData & 0xff);
            }
            public static byte CheckSum(byte[] byteData, int nType)
            {
                if (nType == CDef._CHKSUM_NONE) return (byte)0;
                Byte nData = (Byte)(byteData[0]);
                for (int i = 1; i < byteData.Length; i++)
                {
                    if (nType == CDef._CHKSUM_AND) nData &= (Byte)(byteData[i]);
                    else if (nType == CDef._CHKSUM_OR) nData |= (Byte)(byteData[i]);
                    else if (nType == CDef._CHKSUM_XOR) nData ^= (Byte)(byteData[i]);
                    else if (nType == CDef._CHKSUM_SUM) nData += (Byte)(byteData[i]);
                }
                return (byte)(nData & 0xff);
            }
            public static byte CheckSum(byte byteData, byte byteData2, int nType)
            {
                if (nType == CDef._CHKSUM_NONE) return (byte)0;
                byteData2 = (Byte)(byteData);

                if (nType == CDef._CHKSUM_AND) byteData2 &= (Byte)(byteData);
                else if (nType == CDef._CHKSUM_OR) byteData2 |= (Byte)(byteData);
                else if (nType == CDef._CHKSUM_XOR) byteData2 ^= (Byte)(byteData);
                else if (nType == CDef._CHKSUM_SUM) byteData2 += (Byte)(byteData);

                return (byte)(byteData2 & 0xff);
            }
            /////////// String Builder ////////////////////////////////////////////
            public static int StrBuild_IndexOf(StringBuilder strb, String strValue)
            {
                return strb.ToString().IndexOf(strValue);
            }
            public static int StrBuild_IndexOf(StringBuilder strb, char cValue)
            {
#if false
                int nPos = -1;                
                for (int i = 0; i < strb.Length; i++)
                {
                    if (strb[i] == cValue)
                    {
                        nPos = i;
                        break;
                    }
                }
                return nPos;
#else
                return strb.ToString().IndexOf(cValue);
#endif
            }
            //public static List<StringBuilder> StrBuild_Split(StringBuilder strb, char cValue)
            //{
            //    List<StringBuilder> lst = new List<StringBuilder>();
            //    for (int i = 0; i < strb.Length; i++)
            //    {
            //        string builder
            //        lst.Add
            //    }
            //    return lst;
            //}
            public static double[] FloatsToDoubles(float[] afFloats) { return Array.ConvertAll(afFloats, element => (double)element); }
            public static float[] DoublesToFloats(float[] adDoubles) { return Array.ConvertAll(adDoubles, element => (float)element); }
            public static bool Sort_ListArray(int nStandardPosition, ref List<int[]> lst)
            {
                try
                {
                    if (nStandardPosition < 0) return false;
                    if (lst.Count <= 0) return false;

                    //lst.Sort(CompareInt);
                    //lst.Sort(x, y => x[nStandardPosition].CompareTo(y[nStandardPosition]));
                    lst.Sort(delegate(int[] x, int[] y) { return x[nStandardPosition].CompareTo(y[nStandardPosition]); });
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static bool Sort_ListArray(int nStandardPosition, ref List<string[]> lst)
            {
                try
                {
                    if (nStandardPosition < 0) return false;
                    if (lst.Count <= 0) return false;

                    //lst.Sort(CompareInt);
                    //lst.Sort(x, y => x[nStandardPosition].CompareTo(y[nStandardPosition]));
                    lst.Sort(delegate(string[] x, string[] y) { return StrToInt(x[nStandardPosition]).CompareTo(StrToInt(y[nStandardPosition])); });
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            // 내림차순으로 비교
            //private static int CompareInt(int[] a, int[] b) { return a[0].CompareTo(b[0]); }
            //private static int CompareString(string[] a, string[] b) { return StrToInt(a[0]).CompareTo(StrToInt(b[0])); }
        }
        public class CConvert<T>
        {           ////////// switching Array functions ///////////////////////////
            public static T[] Array2To1(T[,] t)
            {
                int nLine = t.GetLength(0);
                int nLength = t.GetLength(1);

                T[] temp = new T[nLine * nLength];
                for (int i = 0; i < nLine; i++)
                {
                    for (int j = 0; j < nLength; j++)
                    {
                        temp[i * nLength + j] = t[i, j];
                    }
                }
                return temp;
            }

            public static T[,] Array1To2(T[] t, int nLine, int nLength)
            {
                T[,] t1 = new T[nLine, nLength];
                int nValue = t.Length;
                for (int i = 0; i < nValue; i++)
                    t1[i / nLength, i % nLength] = t[i];
                return t1;
            }
            public static void ArrayCopy2(T[,] Src, int nLine_Src, int nCol_Src, ref T[,] Dest, int nLine_Dest, int nCol_Dest, int nLineCnt, int nColCnt)
            {
                for (int i = 0; i < nLineCnt; i++)
                {
                    for (int j = 0; j < nColCnt; j++)
                    {
                        Dest[i + nLine_Dest, j + nCol_Dest] = Src[i + nLine_Src, j + nCol_Src];
                    }
                }
            }

            //public static T[] Array_Init(object value, int nLength) { return Enumerable.Repeat<value>(value, nLength).ToArray<object>(); }
        }
    }
}
