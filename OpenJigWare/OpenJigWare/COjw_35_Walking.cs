using System;
using System.Collections.Generic;
//using System.Linq;

using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class COjwWalking
        {
            public string[] m_pstrParams = new string[] {
                "첫프레임 속도(종료시 제외)",
                "Sway 고정(발들어 올리는 시점)",
                "1점지지 시작구간",
                "시작점(Sway Sequence Offset)",
                "1점지지 Step 수",
                "2점지지 Step 수",
                "Sway",
                "발들어 올리는 높이",
                "전진량",
                "수식 Leg(Right)",
                "수식 Leg(Left)",
                "전진성 타원(앞)",
                "전진성 타원(뒤)",
                "상체 좌우기울임(상체회전각)",
                "",
                "무게중심(+전진)",
                "걸음새를 위한 앉기",
                "다리벌리기 ",
                "들어올리는 다리 벌리기",
                "엉치(W) ID(Right)",
                "엉치(W) ID(Left)",
                "발목(W) ID(Right)",
                "발목(W) ID(Left)",
                "발목SwayOffset",
                "속도시간",
                "딜레이",
                "Sway Shift",
                "팔Up ID(Right)",
                "팔Up ID(Left)",
                "팔Up Shift",
                "팔Up 동작각",
                "팔W ID(Right)",
                "팔W ID(Left)",
                "팔W Shift",
                "팔W 동작각",
                "걸음시 궤적높이(Elastic)",
                "평행회전각(대각이동)",
                "슬립 카운터(1점지지보다 숫자가 낮아야 한다.)",
                "",
                "고관절 Tilt ID(Right)",
                "고관절 Tilt ID(Left)",
                "고관절 숙이기",
                "발목 Tilt ID(Right)",
                "발목 Tilt ID(Left)",
                "뜨는 발바닥 롤링 각",
                "발목 숙이기",
                "고관절 회전을 이용한 회전각",
                "고관절 Pan(오른다리) ID",
                "고관절 Pan(왼다리) ID",
                "덧붙임명령어",
                "허리ID",
                "허리 동작각",
                "목 ID",
                "목 동작각"
            };
            private string[] m_pstrParams_Value_Start = new string[] {
            "500", // 첫프레임속도
            "", // 첫프레임 속도(종료시 제외",
            "", // Sway 고정(발들어 올리는 시점)",
            "", // 1점지지 시작구간",
            "", // 시작점(Sway Sequence Offset)",
            "", // 1점지지 Step 수",
            "", // 2점지지 Step 수",
            "", // Sway",
            "", // 발들어 올리는 높이",
            "", // 전진량",
            "", // 수식 Leg(Right)",
            "", // 수식 Leg(Left)",
            "", // 전진성 타원(앞)",
            "", // 전진성 타원(뒤)",
            "", // 상체 좌우기울임(상체회전각)",
            "",
            "", // 무게중심(+전진)",
            "", // 걸음새를 위한 앉기",
            "", // 다리벌리기 ",
            "", // 들어올리는 다리 벌리기",
            "", // 엉치(W) ID(Right)",
            "", // 엉치(W) ID(Left)",
            "", // 발목(W) ID(Right)",
            "", // 발목(W) ID(Left)",
            "", // 발목SwayOffset",
            "", // 속도시간",
            "", // 딜레이",
            "", // Sway Shift",
            "", // 팔Up ID(Right)",
            "", // 팔Up ID(Left)",
            "", // 팔Up Shift",
            "", // 팔Up 동작각",
            "", // 팔W ID(Right)",
            "", // 팔W ID(Left)",
            "", // 팔W Shift",
            "", // 팔W 동작각",
            "", // 걸음시 궤적높이(Elastic)",
            "", // 평행회전각(대각이동)",
            "", // 슬립 카운터(1점지지보다 숫자가 낮아야 한다.)",
            "",
            "", // 고관절 Tilt ID(Right)",
            "", // 고관절 Tilt ID(Left)",
            "", // 고관절 숙이기",
            "", // 발목 Tilt ID(Right)",
            "", // 발목 Tilt ID(Left)",
            "", // 뜨는 발바닥 롤링 각",
            "", // 발목 숙이기",
            "", // 고관절 회전을 이용한 회전각",
            "", // 고관절 Pan(오른다리) ID",
            "", // 고관절 Pan(왼다리) ID",
            "",  // 덧붙임명령어"
            "", // 허리ID
            "", // 허리 동작각
            "", // 목 ID
            "" // 목 동작각
        };
            private string[] m_pstrParams_Value_Ing = new string[] {
            "", // 첫프레임속도
            "1",
            "0",
            "0",
            "4",
            "2",
            "10",
            "50",
            "40",
            "0",
            "1",
            "60",
            "30",
            "0",// 상체좌우
            "",  
            "5",
            "10",
            "0",
            "2",
            "5",
            "7",
            "10",
            "12",
            "4",
            "50",
            "0",
            "0",
            "1",
            "3",
            "0",
            "0",
            "2",
            "4",
            "-5",
            "0",
            "0",
            "0",
            "0", // 슬립카운터
            "",
            "", // 고관절 TiltID(Right)
            "", // 고관절 TiltID(Left)
            "10",
            "",
            "",
            "-10", // 뜨는 발바닥 롤링각
            "",
            "",
            "",
            "",
            "", // 덧붙임 명령어
            "", // 허리ID
            "", // 허리 동작각
            "", // 허리ID
            "" // 허리 동작각
        };
            private string[] m_pstrParams_Value_End = new string[] {
            "", // 첫프레임속도
            "", // 첫프레임 속도(종료시 제외",
            "", // Sway 고정(발들어 올리는 시점)",
            "", // 1점지지 시작구간",
            "", // 시작점(Sway Sequence Offset)",
            "", // 1점지지 Step 수",
            "", // 2점지지 Step 수",
            "", // Sway",
            "", // 발들어 올리는 높이",
            "", // 전진량",
            "", // 수식 Leg(Right)",
            "", // 수식 Leg(Left)",
            "", // 전진성 타원(앞)",
            "", // 전진성 타원(뒤)",
            "", // 상체 좌우기울임(상체회전각)",
            "",
            "", // 무게중심(+전진)",
            "", // 걸음새를 위한 앉기",
            "", // 다리벌리기 ",
            "", // 들어올리는 다리 벌리기",
            "", // 엉치(W) ID(Right)",
            "", // 엉치(W) ID(Left)",
            "", // 발목(W) ID(Right)",
            "", // 발목(W) ID(Left)",
            "", // 발목SwayOffset",
            "", // 속도시간",
            "", // 딜레이",
            "", // Sway Shift",
            "", // 팔Up ID(Right)",
            "", // 팔Up ID(Left)",
            "", // 팔Up Shift",
            "", // 팔Up 동작각",
            "", // 팔W ID(Right)",
            "", // 팔W ID(Left)",
            "", // 팔W Shift",
            "", // 팔W 동작각",
            "", // 걸음시 궤적높이(Elastic)",
            "", // 평행회전각(대각이동)",
            "", // 슬립 카운터(1점지지보다 숫자가 낮아야 한다.)",
            "",
            "", // 고관절 Tilt ID(Right)",
            "", // 고관절 Tilt ID(Left)",
            "", // 고관절 숙이기",
            "", // 발목 Tilt ID(Right)",
            "", // 발목 Tilt ID(Left)",
            "", // 뜨는 발바닥 롤링 각",
            "", // 발목 숙이기",
            "", // 고관절 회전을 이용한 회전각",
            "", // 고관절 Pan(오른다리) ID",
            "", // 고관절 Pan(왼다리) ID",
            "",  // 덧붙임명령어"
            "",  // 허리ID
            "",  // 허리 동작각
            "",  // 머리ID
            ""  // 머리 동작각
        };
            public string[,] m_aStrParam = new string[3, 100];
            public string[,] m_aStrParam_Old = new string[3, 100];

            public float[,] m_afParam = new float[3, 100];
            public bool[,] m_abParam = new bool[3, 100];

            private readonly string _STR_FILE_NAME_FOR_WALKING = "WalkingParam.dat";

            public CGrid m_COjwGrid = new CGrid();
            public void Grid_Init_Param(Control ctrlHandle,
                Color cLineColor,
                String strHeader_Param, int nWidth_Param, Color cBack_Param, Color cFont_Param,
                String strHeader_Start, int nWidth_Start, Color cBack_Start, Color cFont_Start,
                String strHeader_Repeat, int nWidth_Repeat, Color cBack_Repeat, Color cFont_Repeat,
                String strHeader_End, int nWidth_End, Color cBack_End, Color cFont_End
                )
            {
                string[] pstrTitle = new string[] { strHeader_Param, strHeader_Start, strHeader_Repeat, strHeader_End };
                int[] pnWidth = new int[] { nWidth_Param, nWidth_Start, nWidth_Repeat, nWidth_End };
                int[] pnType = new int[] { 0, 0, 0, 0 };
                m_COjwGrid.Create(ctrlHandle, 0, 0, ctrlHandle.Width, ctrlHandle.Height, pstrTitle, pnWidth, pnType);
                DataGridView OjwGrid = m_COjwGrid.GetHandle();

                OjwGrid.RowHeadersDefaultCellStyle.ForeColor = cLineColor;


                Font fnt = new Font(OjwGrid.Font.Name, OjwGrid.Font.Size, FontStyle.Bold);
                int nPos = 0;
                //OjwGrid.Font = new Font(OjwGrid.Font.Name, OjwGrid.Columns[nPos].DefaultCellStyle.Font.Size, FontStyle.Bold);
                OjwGrid.Columns[nPos].DefaultCellStyle.ForeColor = cFont_Param;// Color.Black;
                OjwGrid.Columns[nPos].DefaultCellStyle.BackColor = cBack_Param;// Color.FromArgb(255, 251, 243); //Color.Orange;
                nPos++;

                //Font fnt = new Font(OjwGrid.Columns[nPos].DefaultCellStyle.Font.Name, OjwGrid.Columns[nPos].DefaultCellStyle.Font.SizeInPoints, System.Drawing.FontStyle.Bold);
                OjwGrid.Columns[nPos].DefaultCellStyle.Font = fnt;
                OjwGrid.Columns[nPos].DefaultCellStyle.ForeColor = cFont_Start;// Color.Red;// Color.Yellow;
                OjwGrid.Columns[nPos].DefaultCellStyle.BackColor = cBack_Start;//Color.Red;
                nPos++;

                OjwGrid.Columns[nPos].DefaultCellStyle.ForeColor = cFont_Repeat;// Color.Blue;// Color.Black;
                OjwGrid.Columns[nPos].DefaultCellStyle.BackColor = cBack_Repeat;// Color.White;
                nPos++;


                //OjwGrid.Columns[nPos].HeaderCell.Style.ForeColor = Color.Blue;
                //OjwGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Blue;



                OjwGrid.Columns[nPos].DefaultCellStyle.Font = fnt;
                OjwGrid.Columns[nPos].DefaultCellStyle.ForeColor = cFont_End;// Color.Green;// Color.Violet; //Color.Yellow;
                OjwGrid.Columns[nPos].DefaultCellStyle.BackColor = cBack_End;//Color.Violet;
                nPos++;

                m_COjwGrid.Grid_Add(0, m_pstrParams.Length);

                //m_COjwGrid.GetHandle(). = Color.FromArgb(235, 236, 239);
                m_COjwGrid.GetHandle().GridColor = Color.FromArgb(235, 236, 239);
                InitValue();
                for (int j = 0; j < m_pstrParams.Length; j++)
                {
                    m_COjwGrid.Grid_Set(0, j, m_pstrParams[j]);
                    if (j < m_pstrParams_Value_Start.Length) m_COjwGrid.Grid_Set(1, j, m_pstrParams_Value_Start[j]);
                    if (j < m_pstrParams_Value_Ing.Length)   m_COjwGrid.Grid_Set(2, j, m_pstrParams_Value_Ing[j]);
                    if (j < m_pstrParams_Value_End.Length)   m_COjwGrid.Grid_Set(3, j, m_pstrParams_Value_End[j]);                    
                }
            }
            private void InitValue()
            {
                for (int j = 0; j < m_pstrParams.Length; j++)
                {
                    //m_COjwGrid.Grid_Set(0, j, m_pstrParams[j]);
                    int i = 0;
                    if (j < m_pstrParams_Value_Start.Length)
                    {
                        //m_COjwGrid.Grid_Set(i + 1, j, m_pstrParams_Value_Start[j]);
                        m_afParam[i, j] = Ojw.CConvert.StrToFloat((m_pstrParams_Value_Start[j].Length > 0) ? m_pstrParams_Value_Start[i] : m_pstrParams_Value_Ing[i]);
                        m_aStrParam[i, j] = (m_pstrParams_Value_Start[j].Length > 0) ? m_pstrParams_Value_Start[j] : m_pstrParams_Value_Ing[j];// String.Empty;
                        if (m_pstrParams_Value_Start[j].Length > 0)
                        {
                            //    m_afParam[i, j] = Ojw.CConvert.StrToFloat(m_pstrParams_Value_Start[i]);
                            m_abParam[i, j] = true;
                        }
                        else m_abParam[i, j] = false;
                    }
                    i++;
                    if (j < m_pstrParams_Value_Ing.Length)
                    {
                        //m_COjwGrid.Grid_Set(i + 1, j, m_pstrParams_Value_Ing[j]);
                        m_afParam[i, j] = Ojw.CConvert.StrToFloat(m_pstrParams_Value_Ing[j]);
                        m_aStrParam[i, j] = m_pstrParams_Value_Ing[j];
                        if (m_pstrParams_Value_Ing[j].Length > 0)
                        {
                            //    m_afParam[i, j] = Ojw.CConvert.StrToFloat(m_pstrParams_Value_Ing[i]);
                            m_abParam[i, j] = true;
                        }
                        else m_abParam[i, j] = false;
                    }
                    i++;
                    if (j < m_pstrParams_Value_End.Length)
                    {
                        //m_COjwGrid.Grid_Set(i + 1, j, m_pstrParams_Value_End[j]);
                        m_afParam[i, j] = Ojw.CConvert.StrToFloat((m_pstrParams_Value_End[j].Length > 0) ? m_pstrParams_Value_End[i] : m_pstrParams_Value_Ing[i]);
                        m_aStrParam[i, j] = (m_pstrParams_Value_End[j].Length > 0) ? m_pstrParams_Value_End[j] : m_pstrParams_Value_Ing[j];
                        if (m_pstrParams_Value_End[j].Length > 0)
                        {
                            //    m_afParam[i, j] = Ojw.CConvert.StrToFloat(m_pstrParams_Value_End[i]);
                            m_abParam[i, j] = true;
                        }
                        else m_abParam[i, j] = false;
                    }
                }
            }
            public bool ParamSave()
            {
                try
                {
                    Ojw.CFile OjwCFile = new Ojw.CFile();
                    OjwCFile.Clear();
                    for (int i = 0; i < m_aStrParam.GetLength(1); i++)
                    {
                        OjwCFile.Add(String.Format("{0}:{1}:{2}", m_aStrParam[0, i], m_aStrParam[1, i], m_aStrParam[2, i]));
                    }
                    return OjwCFile.Save(String.Format("{0}\\{1}", Application.StartupPath, _STR_FILE_NAME_FOR_WALKING));
                }
                catch (Exception ex)
                {
                    Ojw.LogErr("File({0}) Save Error", _STR_FILE_NAME_FOR_WALKING);
                    return false;
                }
            }
            public bool ParamLoad()
            {
                Ojw.CFile OjwCFile = new Ojw.CFile();
                int nRet = OjwCFile.Load(String.Format("{0}\\{1}", Application.StartupPath, _STR_FILE_NAME_FOR_WALKING));
                int nSize = OjwCFile.Get_Count();
                if (nSize > m_aStrParam.GetLength(1)) nSize = m_aStrParam.GetLength(1);

                for (int i = 0; i < nSize; i++)
                {
                    string[] pstrDatas = OjwCFile.GetData(i).Split(':');
                    if (pstrDatas.Length == 3)
                    {
                        for (int j = 0; j < 3; j++)
                            m_aStrParam[j, i] = pstrDatas[j];
                    }
                }

                return ((nRet > 0) ? true : false);
            }


            public int m_nFrameSize = 0;
            public List<String> m_lstMotion = new List<string>();

            public COjwWalking()
            {
                //for (int i = 0; i < m_afMap.GetLength(0); i++)
                //    for (int j = 0; j < m_afMap.GetLength(1); j++)
                //    {
                //        m_afMap[i, j] = new float();
                //        m_afMap[i, j] = 0.0f;
                //    }
                //for (int nLine = 0; nLine < m_pstrParams_Value_Ing.Length; nLine++)//m_aStrParam.GetLength(1); nLine++)
                //{
                //    m_afParam[1, nLine] = Ojw.CConvert.StrToFloat(m_pstrParams_Value_Ing[nLine]);
                //    m_abParam[1, nLine] = (m_pstrParams_Value_Ing[nLine].Length > 0) ? true : false;

                //    m_afParam[0, nLine] = Ojw.CConvert.StrToFloat(m_pstrParams_Value_Start[nLine]);
                //    m_abParam[0, nLine] = (m_pstrParams_Value_Start[nLine].Length > 0) ? true : false;

                //    m_afParam[2, nLine] = Ojw.CConvert.StrToFloat(m_pstrParams_Value_End[nLine]);
                //    m_abParam[2, nLine] = (m_pstrParams_Value_End[nLine].Length > 0) ? true : false;
                //}
                InitValue();
                //for (int i = 0; i < m_aStrParam.GetLength(0); i++)
                //{
                    
                        //m_pstrParams_Value_Start
                    //for (int j = 0; j < m_aStrParam.GetLength(1); j++)
                    //{
                        //m_afParam                        
                        
                        //m_aStrParam[i, j] = Ojw.CConvert.FloatToStr(m_afParam[i, j]);// String.Empty;
                    //}
                //}
            }
            ~COjwWalking()
            {
            }
            //public float GetData(int nStart_0_Repeat_1_End_2, int nIndex) { return m_afParam[nStart_0_Repeat_1_End_2, nIndex]; }
            public string GetData_Str(int nStart_0_Repeat_1_End_2, int nIndex) { return m_aStrParam[nStart_0_Repeat_1_End_2, nIndex]; }
            public int SetData_Str(int nStart_0_Repeat_1_End_2, string[] pstrData)
            {
                int nRet = 0;
                int nMode = nStart_0_Repeat_1_End_2;
                //string[] pstrOldParam = new string[m_aStrParam.GetLength(1)];
                //for (int i = 0; i < pstrData.GetLength(0); i++)
                //    pstrOldParam[i] = m_aStrParam[nMode, i];
                if (nMode != 1)
                {
                    for (int i = 0; i < pstrData.GetLength(0); i++)
                    {
                        m_aStrParam[nMode, i] = "";
                    }
                }

                for (int i = 0; i < pstrData.GetLength(0); i++)
                {
                    if (nStart_0_Repeat_1_End_2 == 0)
                    {
                        if ((pstrData[i] != "") && (pstrData[i] != null))
                        {
                            if (m_aStrParam_Old[nMode, i] != pstrData[i]) nRet++;
                            m_aStrParam[nMode, i] = pstrData[i];
                        }
                        else m_aStrParam[nMode, i] = m_aStrParam[1, i];
                    }
                    else if (nStart_0_Repeat_1_End_2 == 1)
                    {
                        if (m_aStrParam_Old[nMode, i] != pstrData[i]) nRet++;
                        m_aStrParam[nMode, i] = pstrData[i];

                        if (m_aStrParam[0, i] != "")
                        {
                            m_aStrParam[0, i] = m_aStrParam[nMode, i];
                        }
                        else if (m_aStrParam[1, i] != "")
                        {
                            m_aStrParam[1, i] = m_aStrParam[nMode, i];
                        }
                        else m_aStrParam[nMode, i] = m_aStrParam[1, i];
                    }
                    else
                    {
                        if ((pstrData[i] != "") && (pstrData[i] != null))
                        {
                            if (m_aStrParam_Old[nMode, i] != pstrData[i]) nRet++;
                            m_aStrParam[nMode, i] = pstrData[i];
                        }
                        else m_aStrParam[nMode, i] = m_aStrParam[1, i];
                        //m_aStrParam[nMode, i] = pstrData[i];
                    }
                }

                return nRet;
            }
#if false
            public int SetData(int nStart_0_Repeat_1_End_2, float[] pfData)
            {
                int nRet = 0;
                int nMode = nStart_0_Repeat_1_End_2;

                for (int i = 0; i < pfData.GetLength(0); i++) m_afParam[nMode, i] = pfData[i];
                return nRet;
            }

            public void Sync_FloatToString(int nStart_0_Repeat_1_End_2, int nIndex)
            {
                if (nStart_0_Repeat_1_End_2 != 1)
                {
                    m_afParam[nStart_0_Repeat_1_End_2, nIndex]
                }
                else
                {
                }
            }
            
#endif
            //public float GetData(int nStart_0_Repeat_1_End_2, int nIndex, float fData) { return m_afParam[nStart_0_Repeat_1_End_2, nIndex]; }
            //public void SetData(int nStart_0_Repeat_1_End_2, int nIndex, float fData) { m_afParam[nStart_0_Repeat_1_End_2, nIndex] = fData; }
            private int m_nWalkingCount = 1;
            public int SetWalkingCount(int nWalkingCount)
            {
                if (m_nWalkingCount == nWalkingCount) return 0; // 변화없음
                //m_nWalkingCount = (nWalkingCount < 0 ? 0 : nWalkingCount);
                m_nWalkingCount = (nWalkingCount < 1 ? 1 : nWalkingCount);
                return 1;
            }
            public int GetWalkingCount() { return m_nWalkingCount; }
            public void SetData(int nStart_0_Repeat_1_End_2, int nNum, string strData) { m_aStrParam[nStart_0_Repeat_1_End_2, nNum] = strData; }

            public int Size_Gate(int nStart_0_Repeat_1_End_2) { return (int)(Ojw.CConvert.StrToInt(m_aStrParam[nStart_0_Repeat_1_End_2, 4]) + Ojw.CConvert.StrToInt(m_aStrParam[nStart_0_Repeat_1_End_2, 5])); }
            public int Size_Frame(int nStart_0_Repeat_1_End_2) { return Size_Gate(nStart_0_Repeat_1_End_2) * ((nStart_0_Repeat_1_End_2 == 1) ? 2 : 1); }
            public List<String> GenerateWalking(int nWalkingCount = 1)
            {
                m_lstMotion.Clear();

                m_lstMotion.AddRange(MakeWalkingMotion(0));
                //if (GetWalkingCount() > 0) 
                if (nWalkingCount > 0)
                    m_lstMotion.AddRange(MakeWalkingMotion(1));
                m_lstMotion.AddRange(MakeWalkingMotion(2));

                return m_lstMotion;
            }

            private int m_nStep = 0;
            private int m_nStart_0_Repeat_1_End_2 = 0; // 0 - start, 1 - ing, 2 - end
            public void GetString_Init()
            {
                m_nStep = 0;
                m_nStart_0_Repeat_1_End_2 = 0;
            }
            public int GetStatus() { return m_nStart_0_Repeat_1_End_2; }
            public string GetString_Walk()
            {
                string strResult = String.Empty;
                int nSize = Size_Frame(1);
                int nGap = (m_nStart_0_Repeat_1_End_2 == 1) ? 0 : nSize / 2;
                m_nStep++;
                strResult = MakeWalkingMotion(m_nStart_0_Repeat_1_End_2, (m_nStart_0_Repeat_1_End_2 == 2) ? nGap + m_nStep : m_nStep);
                
                if (m_nStep >= nSize - nGap)
                {
                    m_nStep = 0;
                    m_nStart_0_Repeat_1_End_2++;
                    if (m_nStart_0_Repeat_1_End_2 > 2)
                    {
                        m_nStart_0_Repeat_1_End_2 = 0; // 정지
                        //return null;
                    }
                }
                
                return strResult;
            }
#if false // making ... ojw5014
            public int MakeWalkingMotion(int nStart_0_Repeat_1_End_2, int nStep, ref int [] anId, ref float [] afMots)
            {
                // 1점지지 : m_afParam[nMode, 3]            
                // 2점지지 : m_afParam[nMode, 4]
                int nSize_Frame = Size_Frame(1);

                if ((nStep < 1) || (nStep > nSize_Frame)) return 0;

                int nMode = nStart_0_Repeat_1_End_2;
                string strDatas;

#if true
                #region Var
                float fQ4 = 0.5f;
                float fK;
                float fL;
                float fM;
                float fN;
                float fO = m_afParam[nStart_0_Repeat_1_End_2, 1];
                float fP = -m_afParam[nStart_0_Repeat_1_End_2, 10 - 4]; // sway
                float fQ = fP * fQ4; // 0.5f(fQ4) -> H41 의 대치값 비율
                float fR;
                float fS = m_afParam[nStart_0_Repeat_1_End_2, 7 - 4];
                float fT = -m_afParam[nStart_0_Repeat_1_End_2, 30 - 4];
                float fU;
                float fV;
                float fW = -m_afParam[nStart_0_Repeat_1_End_2, 6 - 4];
                float fX = m_afParam[nStart_0_Repeat_1_End_2, 8 - 4];
                float fY = m_afParam[nStart_0_Repeat_1_End_2, 9 - 4];
                float fZ = fX + fY;
                // 초기/종료 동작시엔 미러링동작이라 부호가 반대로 된다.
                float fAA = m_afParam[nStart_0_Repeat_1_End_2, 41 - 4];
                float fAB = -fAA;
                fAA = (fAA > 0 ? fAA : 0.0f);//(m_afParam[nStart_0_Repeat_1_End_2, 41 - 4]) > 0 ? m_afParam[nStart_0_Repeat_1_End_2, 41 - 4]) : 0.0f) * (nStart_0_Repeat_1_End_2 == 1 ? 1 : -1);
                fAB = (fAB > 0 ? fAB : 0.0f);
                float fAC;
                float fAD;
                float fAE;
                float fAF;
                float fAG;
                float fAH;
                float fAI;
                float fAJ;
                float fAK;
                float fAL;
                float fAM;
                float fAN;
                float fAO;
                float fAP;
                float fAQ;
                float fAR;
                float fAS;
                float fAT = m_afParam[nStart_0_Repeat_1_End_2, 27 - 4];
                float fAU = fAT * fQ4;
                float fAV;
                float fAW;
                float fAX;

                float fF25 = 1.0f; // 패턴 결정(1-전체, 2-1점지지만)
                float fD25 = 1.0f; // +가 바깥으로 되게 숫자를 조정

                float fAY = fF25; // RST
                float fAZ = fD25; // DIR

                float fBA;
                float fBB;
                float fBC;
                float fF26 = 1.0f; // 패턴 결정(1-전체, 2-1점지지만)
                float fD26 = 1.0f; // +가 바깥으로 되게 숫자를 조정
                float fBD = fF26;
                float fBE = fD26;

                float fBF;
                float fBG = m_afParam[nStart_0_Repeat_1_End_2, 22 - 4];
                float fD23 = -1; // +가 바깥으로 되게 숫자를 조정
                float fBH = fD23;

                float fD24 = -1; // +가 바깥으로 되게 숫자를 조정
                float fBJ = fD24;

                float fD50 = m_afParam[nStart_0_Repeat_1_End_2, 50 - 4]; // (+오른쪽, -왼쪽)
                float fD51 = fD50; // (+오른쪽, -왼쪽)
                float fBR = fD50;
                float fBS = fD51;
                float fBW = m_afParam[nStart_0_Repeat_1_End_2, 11 - 4];
                float fE19 = 72;// 발바닥 사이간격
                float fF19 = m_afParam[nStart_0_Repeat_1_End_2, 21 - 4] + fE19;

                float fI19 = fF19 / 2 * (float)Math.Tan(m_afParam[nStart_0_Repeat_1_End_2, 17 - 4] / 180.0f * Math.PI); // Offset R
                float fI20 = -fI19; // Offset L

                // 초기 동작시엔 CB, CC 가 상수가 아닌 변수가 된다.
                float fCB = m_afParam[nStart_0_Repeat_1_End_2, 20 - 4];
                float fCC = fI19;
                // 초기 동작시
                float fCB_Walk = m_afParam[1, 20 - 4]; // 반복보행의 파라미터 필요
                float fCB_Start_End = m_afParam[nStart_0_Repeat_1_End_2, 20 - 4]; // 현재보행의 파라미터
                float fCB_Result;
                // 종료 동작시


                float fCD = fI20;

                float fCE = fCB + fCC;
                float fCF = fCB + fCD;

                float fCG = m_afParam[nStart_0_Repeat_1_End_2, 12 - 4];
                float fCH = fCG;
                float fCI = fCG;
                float fCN = m_afParam[nStart_0_Repeat_1_End_2, 15 - 4]; // B15;
                float fCO = m_afParam[nStart_0_Repeat_1_End_2, 16 - 4]; //B16;
                float fE17 = -m_afParam[nStart_0_Repeat_1_End_2, 17 - 4]; //-B17;
                float fE18 = m_afParam[nStart_0_Repeat_1_End_2, 17 - 4]; //B17;
                if (nStart_0_Repeat_1_End_2 != 1)
                {
                    fE17 *= -1.0f;
                    fE18 *= -1.0f;
                }
                float fCX = fE17;
                float fCY = fE18;
                float fCZ = m_afParam[nStart_0_Repeat_1_End_2, 19 - 4]; //B19;

                float fDA = -fCG;
                float fDB = fCG;

                if (nStart_0_Repeat_1_End_2 != 1)
                {
                    fDA = fDB = 0.0f;
                }

                float fDK = -m_afParam[nStart_0_Repeat_1_End_2, 21 - 4]; //-B21;
                float fDL = -fDK;
                float fDQ = m_afParam[nStart_0_Repeat_1_End_2, 37 - 4]; //B37;
                float fDR = m_afParam[nStart_0_Repeat_1_End_2, 38 - 4]; //B38;

                float fD13 = 1;
                float fE13 = 1;
                float fF13 = 1;
                float fD14 = 1;
                float fE14 = 1;
                float fF14 = 1;

                float fDW = m_afParam[nStart_0_Repeat_1_End_2, 40 - 4] * ((nStart_0_Repeat_1_End_2 != 1) ? -1 : 1); //B40;
                float fDX = fDW / 180.0f * (float)Math.PI;
                float fDY = fD13;
                float fDZ = fE13;

                float fEA = fF13;
                float fEE = fD14;
                float fEF = fE14;
                float fEG = fF14;
                float fEH = fDL;
                float fEQ = m_afParam[nStart_0_Repeat_1_End_2, 49 - 4]; //B49;
                float fER = m_afParam[nStart_0_Repeat_1_End_2, 48 - 4]; //B48;
                float fBI;
                float fBK;
                float fBL;
                float fBM = m_afParam[nStart_0_Repeat_1_End_2, 51 - 4]; //B51;
                float fBN = m_afParam[nStart_0_Repeat_1_End_2, 52 - 4]; //B52;
                float fBO;
                float fBP;
                float fBQ;
                float fBT;
                float fBU;
                //float fBV; 
                float fBX;
                float fBY;
                float fBZ;


                float fCA;
                float fCJ;
                float fCK;
                float fCL;
                float fCM;
                float fCP;
                float fCQ;
                float fCR;
                float fCS;
                float fCT;
                float fCU;
                float fCV;
                float fCW;
                float fDC;
                float fDD;
                float fDE;
                //float fDF;
                float fDG;
                float fDH;
                float fDI;
                //float fDJ;

                float fDM = -m_afParam[nStart_0_Repeat_1_End_2, 33 - 4]; //-B33;
                float fDN = (m_afParam[nStart_0_Repeat_1_End_2, 12 - 4] >= 0) ? m_afParam[nStart_0_Repeat_1_End_2, 34 - 4] : -m_afParam[nStart_0_Repeat_1_End_2, 34 - 4]; //IF(CG8 >= 0, B34, -B34);
                float fDO;
                float fDP;
                float fDS;
                float fDT;
                float fDU = m_afParam[nStart_0_Repeat_1_End_2, 39 - 4]; //B39;
                float fDV;

                float fEB = fDK; // X 값, 상수지만 나중 어떻게 될지는...
                float fEC;
                float fED;
                float fEI;
                float fEJ;


                float fEK;
                float fEL;
                float fEM;
                float fEN;
                float fEO;
                float fEP;


                float fES;
                float fET;
                // 허리동작
                float fIB = m_afParam[nStart_0_Repeat_1_End_2, 55 - 4]; //B55;
                float fIC;
#if false
           
            

            float fEV;
            float fEW;
            float fEX;
            float fEY;
            float fEZ;
            
            
            float fA;
            float fB;
            float fC;
            float fD;
            float fE;
            float fF;
            float fG;
            float fH;
            float fI;
            float fJ;
            float fK;
            float fL;
            float fM;
            float fN;
            float fO;
            float fP;
            float fQ;
            float fR;
            float fS;
            float fT;
            float fU;
            float fV;
            float fW;
            float fX;
            float fY;
            float fZ;

            
            float fA;
            float fB;
            float fC;
            float fD;
            float fE;
            float fF;
            float fG;
            float fH;
            float fI;
            float fJ;
            float fK;
            float fL;
            float fM;
            float fN;
            float fO;
            float fP;
            float fQ;
            float fR;
            float fS;
            float fT;
            float fU;
            float fV;
            float fW;
            float fX;
            float fY;
            float fZ;
#endif
                #endregion Var

                int i = nStep;
                #region For
                strDatas = String.Empty;

                fK = (i <= nSize_Frame ? 1.0f : 0.0f); // En
                fL = i;
                fM = i * fK;
                //fU = (float)Math.Round(fR * Math.Sin((((fO == 0 ? fM : fN) - fS) / (fZ) * 180.0f) / 180.0f * Math.PI), 3) + fT;
                //fO
                fV = fM - 1.0f;
                fAC = ((float)((int)fV % (int)fZ) + 1.0f) * fK;
                fAH = (((fAC - (fY + fW)) <= fX && fAC > (fY + fW)) ? 1 : 0);
                fAE = (fAC - (fY + fW)) * fAH;
                fAF = (float)Math.Round((fV + 0.001f) / fZ - 0.5f, 0);
                fAG = (float)Math.Pow(-1.0f, fAF) * fK;
                fAI = fAH * fAG;
                fAJ = (fAI > 0 ? 1 : 0);
                fAN = (fAI < 0 ? 1 : 0);
                // fAP 수식변경(20190515)
                fAP = (fAN == 0 ? 0 : fAE); //(fAN == 0 ? 0 : fAN + fAP8); fAP8 = fAP;
                // fAL 수식변경(20190515)
                fAL = (fAJ == 0 ? 0 : fAE);// (fAJ == 0 ? 0 : fAJ + fAL8); fAL8 = fAL;

                fAS = (fAG < 0 ? 1.0f : 0.0f);
                fAR = (fAG > 0 ? 1.0f : 0.0f);
                // fAK 수식변경(20190515)
                fAK = (fAR == 0 ? 0 : fAC); //(fAR == 0 ? 0 : fAR + fAK8); fAK8 = fAK;
                // fAO 수식변경(20190515)
                fAO = (fAS == 0 ? 0 : fAC); //(fAS == 0 ? 0 : fAS + fAO8); fAO8 = fAO;
                // fN 수식 변경(20190515), 
                //fN = fAK - fAL + fAO - fAP + (fAL + fAP > 0 ? 1.0f : 0.0f) + fK * fAF * fZ;//((fAL + fAP > 0 && fAL + fAP != fX) ? fN8 : fM); fN8 = fN;
                // fN 수식 변경-통합(20190701),     
                fN = (fO == 1 ? (fM >= fZ ? fZ : fAK - fAL) + (fM >= fZ * 2.0f ? fZ : fAO - fAP) : fAK - fAL + fAO - fAP + (fAL + fAP > 0 ? 1.0f : 0.0f) + fK * fAF * fZ);
                //fN = ((fO == 1) ? ((fAL + fAP > 0 && fAL + fAP != fX) ? fN8 : fM) : (fAK - fAL + fAO - fAP + (fAL + fAP > 0 ? 1.0f : 0.0f) + fK * fAF * fZ));
                fR = (fAA + fAB != 0 ? ((fM <= fZ && fAB != 0) ? fP : fQ) * fAR + ((fM > fZ && fAA != 0) ? fP : fQ) * fAS : fP * fK); // fAS, fAR
                fU = (float)(fR * (float)Math.Sin((((fO == 0 ? fM : fN) - fS) / (fZ) * 180.0f) / 180.0f * Math.PI)) + fT;

                fAD = fAF * fZ + fAG * fAC + fAF;
                fAM = (float)Math.Round((fAL > fAA ? (fAL - fAA) / (fX - fAA) * fX : 0), 3);
                fAQ = (float)Math.Round((fAP > fAB ? (fAP - fAB) / (fX - fAB) * fX : 0.0f), 3);
                fAV = (fAA + fAB != 0 ? ((fM <= fZ && fAB != 0) ? fAT : fAU) * fAR + ((fM > fZ && fAA != 0) ? fAT : fAU) * fAS : fAT * fK);
                fAW = fAV * (float)Math.Sin(((fO == 0 ? fAK : fN) / fZ * 180.0f) / 180.0f * Math.PI);
                fAX = fAV * (float)Math.Sin((fAL / fX * 180.0f) / 180.0f * Math.PI);

                fBA = (fAY == 1 ? fAW : fAX) * fAZ;
                fBB = fAV * (float)Math.Sin((fAO / fZ * 180.0f) / 180.0f * Math.PI);
                fBC = fAV * (float)Math.Sin((fAP / fX * 180.0f) / 180.0f * Math.PI);
                fBF = (fBD == 1 ? fBB : fBC) * fBE;

                fBI = fBG * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI) * fBH;
                fBK = fBG * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI) * fBJ;

                fBL = (fAD - (fY + fW)) * fK;
                fBO = (fAR + fAJ + fAS + fAN - 1.0f) * fAG;
                // fBP 수식변경(20190515)
                fBP = (fBL < 0 ? 0 : (fBL > fX ? fX : fBL)); //fBP8 + fBO; fBP8 = fBP;
                fBQ = (fBO < 0 ? fBP + 1.0f : fBP);



                //////////////////////////////////////////////////////////////////
                fBT = (fBR * fBQ / fX);
                fBU = (fBS * fBQ / fX);

                fBX = fAJ * fBW * fK;
                fBY = (float)Math.Round(fBX * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI), 3);
                fBZ = fAN * fBW * fK;


                fCA = (float)Math.Round(fBZ * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI), 3);

                // fCJ 수식변경(20190515)
                fCJ = (fM > (fZ + fW) ? fCG * 2.0f : fCG / fX * fAL * 2.0f);//(fAL > 0 ? fCH / fX * fAL * 2.0f : fCJ8); fCJ8 = fCJ;
                // fCK 수식변경(20190515)
                fCK = (fM > (fZ + fW + fY + fX) ? fCG * 2.0f : fCG / fX * fAP * 2.0f);//(fAP > 0 ? fCI / fX * fAP * 2.0f : fCK8); fCK8 = fCK;

                fCL = fCH * (fM / (fZ * 2.0f) * 2.0f);
                fCM = fCI * (fM / (fZ * 2.0f) * 2.0f);

                fCP = fCN * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI);
                fCQ = fCO * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI);
                fCR = fAM / fZ * 2.0f;
                fCS = (fCR * fCP - (2.0f - fCR) * fCQ);
                fCT = fCN * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI);
                fCU = fCO * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI);
                fCV = fAQ / fZ * 2.0f;
                fCW = (fCV * fCT - (2.0f - fCV) * fCU);


                if (nStart_0_Repeat_1_End_2 != 1)
                {
                    fCZ = m_afParam[nStart_0_Repeat_1_End_2, 19 - 4];
                    float fCZ_1 = m_afParam[1, 19 - 4];
                    fCZ = (fCZ + (fCZ_1 - fCZ) / fZ * fAD) * fK;
                }

                fDC = (fCJ - fCL) + fCS;
                fDD = fDC * ((nStart_0_Repeat_1_End_2 != 1) ? 1.0f : 2.0f) + fDA;
                fDE = fDC;
                //fDF;
                fDG = (fCK - fCM) + fCW;
                fDH = fDG * ((nStart_0_Repeat_1_End_2 != 1) ? 1.0f : 2.0f) + fDB;
                fDI = fDG;
                //fDJ;


                if (nStart_0_Repeat_1_End_2 != 1)
                {
                    fDN = (m_afParam[nStart_0_Repeat_1_End_2, 12 - 4] >= 0) ? m_afParam[nStart_0_Repeat_1_End_2, 34 - 4] : -m_afParam[nStart_0_Repeat_1_End_2, 34 - 4]; //IF(CG8 >= 0, B34, -B34);
                    float fDN_1 = (m_afParam[1, 12 - 4] >= 0) ? m_afParam[1, 34 - 4] : -m_afParam[1, 34 - 4]; //IF(CG8 >= 0, B34, -B34);
                    fDN = (fDN + (fDN_1 - fDN) / fZ * fAD) * fK;
                }


                fDO = fDN * (float)Math.Sin((fM / (fZ * 2.0f) * 180.0f) / 180.0f * (float)Math.PI) - fDN / 2.0f;
                fDP = -fDO;

                fDS = (float)Math.Abs(fDR * (float)Math.Sin((fM / (fZ) * 180.0f) / 180.0f * (float)Math.PI)) + fDQ;
                fDT = fDS;
                fDV = -(float)Math.Abs(fDU * (float)Math.Sin((fM / (fZ) * 180.0f) / 180.0f * (float)Math.PI)) * (fDU >= 0 ? 1.0f : -1.0f);

                // 시작/종료 보행시는 아래의 파라미터가 가변이 된다.
                if (nStart_0_Repeat_1_End_2 != 1)
                {
                    fCB_Result = (fCB_Start_End + (fCB_Walk - fCB_Start_End) / fZ * fAD) * fK;
                    //fCB_Result = (fCB_Start_End + (fCB_Walk - fCB_Start_End) / fZ * fAD) * fK;

                    // 앉기(CB) + Offset(R : CC)
                    // 앉기(CB) + Offset(L : CD)
                    fCE = fCB_Result + fCC;
                    fCF = fCB_Result + fCD;
                }

                fEC = fBY + fCE + fDV;
                fED = (float)Math.Round(fDD, 3);


                fEI = fCA + fCF + fDV;
                fEJ = (float)Math.Round(fDH, 3);


                if (nStart_0_Repeat_1_End_2 != 1)
                {
                    float fDK_0 = -m_afParam[1, 21 - 4];
                    float fDK_1 = -m_afParam[nStart_0_Repeat_1_End_2, 21 - 4];
                    fDK = (fDK_1 + (fDK_0 - fDK_1) / fX * fBQ) * fK;
                    fDL = -fDK;
                    fEB = fDK;
                    fEH = fDL;

                    // 팔 Up Shift
                    float fDM_0 = -m_afParam[1, 33 - 4]; //-B33;
                    float fDM_1 = -m_afParam[nStart_0_Repeat_1_End_2, 33 - 4]; //-B33;
                    fDM = (fDM_1 + (fDM_0 - fDM_1) / fZ * fAD) * fK;

                    // 팔 Up 동작각
                    float fDQ_0 = m_afParam[1, 37 - 4]; //B37;
                    float fDQ_1 = m_afParam[nStart_0_Repeat_1_End_2, 37 - 4]; //B37;
                    fDQ = (fDQ_1 + (fDQ_0 - fDQ_1) / fZ * fAD) * fK;

                    // ojw5014
                    // 발목 숙이기 - 미 검증
                    float fEQ_0 = m_afParam[1, 49 - 4]; //B49;
                    float fEQ_1 = m_afParam[nStart_0_Repeat_1_End_2, 49 - 4]; //B49;
                    fEQ = (fEQ_1 + (fEQ_0 - fEQ_1) / fZ * fAD) * fK;
                }

                fEK = fU + ((float)Math.Cos(fDX) * fEB + (float)Math.Sin(fDX) * fED) * fDY;
                fEL = fEC * fDZ;




                fEM = (-(float)Math.Sin(fDX) * fEB + (float)Math.Cos(fDX) * fED) * fEA - fCZ;
                fEN = fU + ((float)Math.Cos(fDX) * fEH + (float)Math.Sin(fDX) * fEJ) * fEE;
                fEO = fEI * fEF;
                fEP = (-(float)Math.Sin(fDX) * fEH + (float)Math.Cos(fDX) * fEJ) * fEG - fCZ;


                fES = (fAL > 0 ? fER * (float)Math.Sin((fAL / fX * 180.0f) / 180.0f * (float)Math.PI) : 0.0f) + fEQ;
                fET = (fAP > 0 ? fER * (float)Math.Sin((fAP / fX * 180.0f) / 180.0f * (float)Math.PI) : 0.0f) + fEQ;

                // 허리
                fIC = fIB * (float)Math.Sin((fM / fZ * 180.0f) / 180.0f * Math.PI);

                // fEU;

                List<float> lstfId = new List<float>();
                List<float> lstfMot = new List<float>();
                ///////////////////////////////////////////////////////
                int j = 0;
                // EV: "E", EW: "1" 은 당연히 붙는 것
                //=IF(B13="","",CONCATENATE("I",B13))
                bool bData = false;
                if ((GetWalkingCount() <= 0) && (nStart_0_Repeat_1_End_2 == 1))
                {
                }
                else
                {

                    #region Read Forward
                    // 결과가 나오기 보다는 결과를 메모리에 올리기만 한다.
                    Ojw.CKinematics.CForward.CalcKinematics(m_C3d.GetHeader().pDhParamAll[(int)m_afParam[nStart_0_Repeat_1_End_2, 13 - 4]], afMots, out afX[nNum], out afY[nNum], out afZ[nNum]);
                    #endregion Read Forward
                    if (bInc == true)
                        afZ[nNum] += Ojw.CConvert.StrToFloat(strTmp);
                    else
                        afZ[nNum] = Ojw.CConvert.StrToFloat(strTmp);
                    Ojw.CGridView.Grid_Xyz2Angle(CGrid, m_C3d, nLine, nNum, afX[nNum], afY[nNum], afZ[nNum]);

                    // I2 X Y Z
                    bData = (m_aStrParam[nStart_0_Repeat_1_End_2, 13 - 4] == "" ? false : true);
                    if (bData == true) { strDatas += String.Format("I{0}\t{1}\t{2}\t{3}\t", m_afParam[nStart_0_Repeat_1_End_2, 13 - 4], fEK, fEL, fEM); }




                    // I3 X Y Z
                    bData = (m_aStrParam[nStart_0_Repeat_1_End_2, 14 - 4] == "" ? false : true);
                    if (bData == true) { strDatas += String.Format("I{0}\t{1}\t{2}\t{3}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 14 - 4], fEN, fEO, fEP); }

                    if ((i == 1) && (nStart_0_Repeat_1_End_2 == 0))
                    {
                        // S 50 D 0
                        strDatas += String.Format("S\t{0}\tD\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 4 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 29 - 4]);
                    }
                    else
                    {
                        // S 50 D 0
                        strDatas += String.Format("S\t{0}\tD\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 28 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 29 - 4]);
                    }

                    // 발목
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 26 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 26 - 4], fBA + fCX); }
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 25 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 25 - 4], fBF + fCY); }

                    // 엉치
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4], fBI); }
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 24 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 24 - 4], fBK); }

                    // 팔Up
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 31 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 31 - 4], fDO + fDM); }
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 32 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 32 - 4], fDP + fDM); }

                    // 팔Wing
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 35 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 35 - 4], fDS + fDQ); }
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 36 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 36 - 4], fDT + fDQ); }


                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 46 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 46 - 4], fES); }
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 47 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 47 - 4], fET); }
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 43 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 43 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]); } // P6


                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        // ojw5014
                        // 고관절 Tilt 숙이기 - 미 검증
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4] != "")
                        {
                            float fFD_0 = m_afParam[1, 45 - 4]; //B45;
                            float fFD_1 = m_afParam[nStart_0_Repeat_1_End_2, 45 - 4]; //B45;
                            float fFD = (fFD_1 + (fFD_0 - fFD_1) / fZ * fAD) * fK;

                            strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4], fFD);
                        } // GE = B45{m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]}                    
                    }
                    else
                    {
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]); } // GE = B45{m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]}
                    }

                    //if (m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4], FA9); }


                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 51 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 51 - 4], fBT); } // BM8 = B51
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 52 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 52 - 4], fBU); } // BN8 = B52


                    // 허리
                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 54 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 54 - 4], fIC); }

                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        strDatas += String.Format("X\t-1\t");
                    }
                    strDatas += String.Format("G\t{0}\t", nStart_0_Repeat_1_End_2 + 1);


                    if (m_bMirror == true)
                    {
                        strDatas += String.Format("X\t-1\t");
                    }

                    // 반복 패턴
                    if ((nStart_0_Repeat_1_End_2 == 1) && (GetWalkingCount() > 1) && (i == 1))
                    {
                        if (fK == 1)
                        {
                            strDatas += String.Format("@SET_COMMAND,1\t@SET_DATA0,{0}\t@SET_DATA1,{1}\t", nSize_Frame - 1, GetWalkingCount());
                        }
                    }

                    if (m_aStrParam[nStart_0_Repeat_1_End_2, 58 - 4] != "") { strDatas += String.Format("{0}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 58 - 4]); } // B58

                    strDatas += Ojw.CConvert.ChangeChar(m_aStrParam[nStart_0_Repeat_1_End_2, 53 - 4], ' ', '\t'); // B53 - 덧붙임 명령어
                    //lstStrFrame.Add(strDatas);
                }
                #endregion For
                return strDatas;// lstStrFrame;
#endif

            }
#endif
            public string MakeWalkingMotion(int nStart_0_Repeat_1_End_2, int nStep)
            {
                // 1점지지 : m_afParam[nMode, 3]            
                // 2점지지 : m_afParam[nMode, 4]
                int nSize_Frame = Size_Frame(1);// Size_Frame(nStart_0_Repeat_1_End_2);

                if ((nStep < 1) || (nStep > nSize_Frame)) return String.Empty;
                //if ((nStep < 0) || (nStep >= nSize_Frame)) return String.Empty;


                int nMode = nStart_0_Repeat_1_End_2;
                string strDatas;
                //List<string> lstStrFrame = new List<string>();
                //lstStrFrame.Clear();


#if true
                // fL => i;
                #region Var
                float fQ4 = 0.5f;
                float fK;
                float fL;
                float fM;
                float fN;
                float fO = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 1]);
                float fP = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 10 - 4]); // sway
                float fQ = fP * fQ4; // 0.5f(fQ4) -> H41 의 대치값 비율
                float fR;
                float fS = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 7 - 4]);
                float fT = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 30 - 4]);
                float fU;
                float fV;
                float fW = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 6 - 4]);
                float fX = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 8 - 4]);
                float fY = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 9 - 4]);
                float fZ = fX + fY;
                // 초기/종료 동작시엔 미러링동작이라 부호가 반대로 된다.
                float fAA = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 41 - 4]);
                float fAB = -fAA;
                fAA = (fAA > 0 ? fAA : 0.0f);//(Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 41 - 4]) > 0 ? Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 41 - 4]) : 0.0f) * (nStart_0_Repeat_1_End_2 == 1 ? 1 : -1);
                fAB = (fAB > 0 ? fAB : 0.0f);
                float fAC;
                float fAD;
                float fAE;
                float fAF;
                float fAG;
                float fAH;
                float fAI;
                float fAJ;
                float fAK;
                float fAL;
                float fAM;
                float fAN;
                float fAO;
                float fAP;
                float fAQ;
                float fAR;
                float fAS;
                float fAT = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 27 - 4]);
                float fAU = fAT * fQ4;
                float fAV;
                float fAW;
                float fAX;

                float fF25 = 1.0f; // 패턴 결정(1-전체, 2-1점지지만)
                float fD25 = 1.0f; // +가 바깥으로 되게 숫자를 조정

                float fAY = fF25; // RST
                float fAZ = fD25; // DIR

                float fBA;
                float fBB;
                float fBC;
                float fF26 = 1.0f; // 패턴 결정(1-전체, 2-1점지지만)
                float fD26 = 1.0f; // +가 바깥으로 되게 숫자를 조정
                float fBD = fF26;
                float fBE = fD26;

                float fBF;
                float fBG = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 22 - 4]);
                float fD23 = -1; // +가 바깥으로 되게 숫자를 조정
                float fBH = fD23;

                float fD24 = -1; // +가 바깥으로 되게 숫자를 조정
                float fBJ = fD24;

                float fD50 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 50 - 4]); // (+오른쪽, -왼쪽)
                float fD51 = fD50; // (+오른쪽, -왼쪽)
                float fBR = fD50;
                float fBS = fD51;
                float fBW = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 11 - 4]);
                float fE19 = 72;// 발바닥 사이간격
                float fF19 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 21 - 4] + fE19);

                float fI19 = fF19 / 2 * (float)Math.Tan((Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 17 - 4])) / 180.0f * Math.PI); // Offset R
                float fI20 = -fI19; // Offset L

                // 초기 동작시엔 CB, CC 가 상수가 아닌 변수가 된다.
                float fCB = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 20 - 4]);
                float fCC = fI19;
                // 초기 동작시
                float fCB_Walk = Ojw.CConvert.StrToFloat(m_aStrParam[1, 20 - 4]); // 반복보행의 파라미터 필요
                float fCB_Start_End = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 20 - 4]); // 현재보행의 파라미터
                float fCB_Result;
                // 종료 동작시


                float fCD = fI20;

                float fCE = fCB + fCC;
                float fCF = fCB + fCD;

                float fCG = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 12 - 4]);
                float fCH = fCG;
                float fCI = fCG;
                float fCN = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 15 - 4]); // B15;
                float fCO = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 16 - 4]); //B16;
                float fE17 = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 17 - 4]); //-B17;
                float fE18 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 17 - 4]); //B17;
                if (nStart_0_Repeat_1_End_2 != 1)
                {
                    fE17 *= -1.0f;
                    fE18 *= -1.0f;
                }
                float fCX = fE17;
                float fCY = fE18;
                float fCZ = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 19 - 4]); //B19;

                float fDA = -fCG;
                float fDB = fCG;

                if (nStart_0_Repeat_1_End_2 != 1)
                {
                    fDA = fDB = 0.0f;
                }

                float fDK = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 21 - 4]); //-B21;
                float fDL = -fDK;
                float fDQ = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 37 - 4]); //B37;
                float fDR = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 38 - 4]); //B38;

                float fD13 = 1;
                float fE13 = 1;
                float fF13 = 1;
                float fD14 = 1;
                float fE14 = 1;
                float fF14 = 1;

                float fDW = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 40 - 4]) * ((nStart_0_Repeat_1_End_2 != 1) ? -1 : 1); //B40;
                float fDX = fDW / 180.0f * (float)Math.PI;
                float fDY = fD13;
                float fDZ = fE13;

                float fEA = fF13;
                float fEE = fD14;
                float fEF = fE14;
                float fEG = fF14;
                float fEH = fDL;
                float fEQ = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 49 - 4]); //B49;
                float fER = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 48 - 4]); //B48;
                float fBI;
                float fBK;
                float fBL;
                float fBM = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 51 - 4]); //B51;
                float fBN = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 52 - 4]); //B52;
                float fBO;
                float fBP;
                float fBQ;
                float fBT;
                float fBU;
                //float fBV; 
                float fBX;
                float fBY;
                float fBZ;


                float fCA;
                float fCJ;
                float fCK;
                float fCL;
                float fCM;
                float fCP;
                float fCQ;
                float fCR;
                float fCS;
                float fCT;
                float fCU;
                float fCV;
                float fCW;
                float fDC;
                float fDD;
                float fDE;
                //float fDF;
                float fDG;
                float fDH;
                float fDI;
                //float fDJ;

                float fDM = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 33 - 4]); //-B33;
                float fDN = (Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 12 - 4]) >= 0) ? Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 34 - 4]) : -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 34 - 4]); //IF(CG8 >= 0, B34, -B34);
                float fDO;
                float fDP;
                float fDS;
                float fDT;
                float fDU = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 39 - 4]); //B39;
                float fDV;

                float fEB = fDK; // X 값, 상수지만 나중 어떻게 될지는...
                float fEC;
                float fED;
                float fEI;
                float fEJ;


                float fEK;
                float fEL;
                float fEM;
                float fEN;
                float fEO;
                float fEP;


                float fES;
                float fET;
                // 허리동작
                float fIB = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 55 - 4]); //B55;
                float fIC;
                // 머리동작
                float fIE = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 57 - 4]); //B57;
                float fIF;
#if false
           
            

            float fEV;
            float fEW;
            float fEX;
            float fEY;
            float fEZ;
            
            
            float fA;
            float fB;
            float fC;
            float fD;
            float fE;
            float fF;
            float fG;
            float fH;
            float fI;
            float fJ;
            float fK;
            float fL;
            float fM;
            float fN;
            float fO;
            float fP;
            float fQ;
            float fR;
            float fS;
            float fT;
            float fU;
            float fV;
            float fW;
            float fX;
            float fY;
            float fZ;

            
            float fA;
            float fB;
            float fC;
            float fD;
            float fE;
            float fF;
            float fG;
            float fH;
            float fI;
            float fJ;
            float fK;
            float fL;
            float fM;
            float fN;
            float fO;
            float fP;
            float fQ;
            float fR;
            float fS;
            float fT;
            float fU;
            float fV;
            float fW;
            float fX;
            float fY;
            float fZ;
#endif
                #endregion Var

                int i = nStep;
                //for (int i = 1; i <= nSize_Frame; i++)
                //{
                    #region For
                    strDatas = String.Empty;

                    fK = (i <= nSize_Frame ? 1.0f : 0.0f); // En
                    fL = i;
                    fM = i * fK;
                    //fU = (float)Math.Round(fR * Math.Sin((((fO == 0 ? fM : fN) - fS) / (fZ) * 180.0f) / 180.0f * Math.PI), 3) + fT;
                    //fO
                    fV = fM - 1.0f;
                    fAC = ((float)((int)fV % (int)fZ) + 1.0f) * fK;
                    fAH = (((fAC - (fY + fW)) <= fX && fAC > (fY + fW)) ? 1 : 0);
                    fAE = (fAC - (fY + fW)) * fAH;
                    fAF = (float)Math.Round((fV + 0.001f) / fZ - 0.5f, 0);
                    fAG = (float)Math.Pow(-1.0f, fAF) * fK;
                    fAI = fAH * fAG;
                    fAJ = (fAI > 0 ? 1 : 0);
                    fAN = (fAI < 0 ? 1 : 0);
                    // fAP 수식변경(20190515)
                    fAP = (fAN == 0 ? 0 : fAE); //(fAN == 0 ? 0 : fAN + fAP8); fAP8 = fAP;
                    // fAL 수식변경(20190515)
                    fAL = (fAJ == 0 ? 0 : fAE);// (fAJ == 0 ? 0 : fAJ + fAL8); fAL8 = fAL;

                    fAS = (fAG < 0 ? 1.0f : 0.0f);
                    fAR = (fAG > 0 ? 1.0f : 0.0f);
                    // fAK 수식변경(20190515)
                    fAK = (fAR == 0 ? 0 : fAC); //(fAR == 0 ? 0 : fAR + fAK8); fAK8 = fAK;
                    // fAO 수식변경(20190515)
                    fAO = (fAS == 0 ? 0 : fAC); //(fAS == 0 ? 0 : fAS + fAO8); fAO8 = fAO;
                    // fN 수식 변경(20190515), 
                    //fN = fAK - fAL + fAO - fAP + (fAL + fAP > 0 ? 1.0f : 0.0f) + fK * fAF * fZ;//((fAL + fAP > 0 && fAL + fAP != fX) ? fN8 : fM); fN8 = fN;
                    // fN 수식 변경-통합(20190701),     
                    fN = (fO == 1 ? (fM >= fZ ? fZ : fAK - fAL) + (fM >= fZ * 2.0f ? fZ : fAO - fAP) : fAK - fAL + fAO - fAP + (fAL + fAP > 0 ? 1.0f : 0.0f) + fK * fAF * fZ);
                    //fN = ((fO == 1) ? ((fAL + fAP > 0 && fAL + fAP != fX) ? fN8 : fM) : (fAK - fAL + fAO - fAP + (fAL + fAP > 0 ? 1.0f : 0.0f) + fK * fAF * fZ));
                    fR = (fAA + fAB != 0 ? ((fM <= fZ && fAB != 0) ? fP : fQ) * fAR + ((fM > fZ && fAA != 0) ? fP : fQ) * fAS : fP * fK); // fAS, fAR
                    fU = (float)(fR * (float)Math.Sin((((fO == 0 ? fM : fN) - fS) / (fZ) * 180.0f) / 180.0f * Math.PI)) + fT;

                    fAD = fAF * fZ + fAG * fAC + fAF;
                    fAM = (float)Math.Round((fAL > fAA ? (fAL - fAA) / (fX - fAA) * fX : 0), 3);
                    fAQ = (float)Math.Round((fAP > fAB ? (fAP - fAB) / (fX - fAB) * fX : 0.0f), 3);
                    fAV = (fAA + fAB != 0 ? ((fM <= fZ && fAB != 0) ? fAT : fAU) * fAR + ((fM > fZ && fAA != 0) ? fAT : fAU) * fAS : fAT * fK);
                    fAW = fAV * (float)Math.Sin(((fO == 0 ? fAK : fN) / fZ * 180.0f) / 180.0f * Math.PI);
                    fAX = fAV * (float)Math.Sin((fAL / fX * 180.0f) / 180.0f * Math.PI);

                    fBA = (fAY == 1 ? fAW : fAX) * fAZ;
                    fBB = fAV * (float)Math.Sin((fAO / fZ * 180.0f) / 180.0f * Math.PI);
                    fBC = fAV * (float)Math.Sin((fAP / fX * 180.0f) / 180.0f * Math.PI);
                    fBF = (fBD == 1 ? fBB : fBC) * fBE;

                    fBI = fBG * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI) * fBH;
                    fBK = fBG * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI) * fBJ;

                    fBL = (fAD - (fY + fW)) * fK;
                    fBO = (fAR + fAJ + fAS + fAN - 1.0f) * fAG;
                    // fBP 수식변경(20190515)
                    fBP = (fBL < 0 ? 0 : (fBL > fX ? fX : fBL)); //fBP8 + fBO; fBP8 = fBP;
                    fBQ = (fBO < 0 ? fBP + 1.0f : fBP);



                    //////////////////////////////////////////////////////////////////
                    fBT = (fBR * fBQ / fX);
                    fBU = (fBS * fBQ / fX);

                    fBX = fAJ * fBW * fK;
                    fBY = (float)Math.Round(fBX * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI), 3);
                    fBZ = fAN * fBW * fK;


                    fCA = (float)Math.Round(fBZ * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI), 3);

                    // fCJ 수식변경(20190515)
                    fCJ = (fM > (fZ + fW) ? fCG * 2.0f : fCG / fX * fAL * 2.0f);//(fAL > 0 ? fCH / fX * fAL * 2.0f : fCJ8); fCJ8 = fCJ;
                    // fCK 수식변경(20190515)
                    fCK = (fM > (fZ + fW + fY + fX) ? fCG * 2.0f : fCG / fX * fAP * 2.0f);//(fAP > 0 ? fCI / fX * fAP * 2.0f : fCK8); fCK8 = fCK;

                    fCL = fCH * (fM / (fZ * 2.0f) * 2.0f);
                    fCM = fCI * (fM / (fZ * 2.0f) * 2.0f);

                    fCP = fCN * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI);
                    fCQ = fCO * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI);
                    fCR = fAM / fZ * 2.0f;
                    fCS = (fCR * fCP - (2.0f - fCR) * fCQ);
                    fCT = fCN * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI);
                    fCU = fCO * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI);
                    fCV = fAQ / fZ * 2.0f;
                    fCW = (fCV * fCT - (2.0f - fCV) * fCU);


                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        fCZ = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 19 - 4]);
                        float fCZ_1 = Ojw.CConvert.StrToFloat(m_aStrParam[1, 19 - 4]);
                        fCZ = (fCZ + (fCZ_1 - fCZ) / fZ * fAD) * fK;
                    }

                    fDC = (fCJ - fCL) + fCS;
                    fDD = fDC * ((nStart_0_Repeat_1_End_2 != 1) ? 1.0f : 2.0f) + fDA;
                    fDE = fDC;
                    //fDF;
                    fDG = (fCK - fCM) + fCW;
                    fDH = fDG * ((nStart_0_Repeat_1_End_2 != 1) ? 1.0f : 2.0f) + fDB;
                    fDI = fDG;
                    //fDJ;


                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        fDN = (Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 12 - 4]) >= 0) ? Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 34 - 4]) : -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 34 - 4]); //IF(CG8 >= 0, B34, -B34);
                        float fDN_1 = (Ojw.CConvert.StrToFloat(m_aStrParam[1, 12 - 4]) >= 0) ? Ojw.CConvert.StrToFloat(m_aStrParam[1, 34 - 4]) : -Ojw.CConvert.StrToFloat(m_aStrParam[1, 34 - 4]); //IF(CG8 >= 0, B34, -B34);
                        fDN = (fDN + (fDN_1 - fDN) / fZ * fAD) * fK;
                    }


                    fDO = fDN * (float)Math.Sin((fM / (fZ * 2.0f) * 180.0f) / 180.0f * (float)Math.PI) - fDN / 2.0f;
                    fDP = -fDO;

                    fDS = (float)Math.Abs(fDR * (float)Math.Sin((fM / (fZ) * 180.0f) / 180.0f * (float)Math.PI)) + fDQ;
                    fDT = fDS;
                    fDV = -(float)Math.Abs(fDU * (float)Math.Sin((fM / (fZ) * 180.0f) / 180.0f * (float)Math.PI)) * (fDU >= 0 ? 1.0f : -1.0f);

                    // 시작/종료 보행시는 아래의 파라미터가 가변이 된다.
                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        fCB_Result = (fCB_Start_End + (fCB_Walk - fCB_Start_End) / fZ * fAD) * fK;
                        //fCB_Result = (fCB_Start_End + (fCB_Walk - fCB_Start_End) / fZ * fAD) * fK;

                        // 앉기(CB) + Offset(R : CC)
                        // 앉기(CB) + Offset(L : CD)
                        fCE = fCB_Result + fCC;
                        fCF = fCB_Result + fCD;
                    }

                    fEC = fBY + fCE + fDV;
                    fED = (float)Math.Round(fDD, 3);


                    fEI = fCA + fCF + fDV;
                    fEJ = (float)Math.Round(fDH, 3);


                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        float fDK_0 = -Ojw.CConvert.StrToFloat(m_aStrParam[1, 21 - 4]);
                        float fDK_1 = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 21 - 4]);
                        fDK = (fDK_1 + (fDK_0 - fDK_1) / fX * fBQ) * fK;
                        fDL = -fDK;
                        fEB = fDK;
                        fEH = fDL;

                        // 팔 Up Shift
                        float fDM_0 = -Ojw.CConvert.StrToFloat(m_aStrParam[1, 33 - 4]); //-B33;
                        float fDM_1 = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 33 - 4]); //-B33;
                        fDM = (fDM_1 + (fDM_0 - fDM_1) / fZ * fAD) * fK;

                        // 팔 Up 동작각
                        float fDQ_0 = Ojw.CConvert.StrToFloat(m_aStrParam[1, 37 - 4]); //B37;
                        float fDQ_1 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 37 - 4]); //B37;
                        fDQ = (fDQ_1 + (fDQ_0 - fDQ_1) / fZ * fAD) * fK;

                        // ojw5014
                        // 발목 숙이기 - 미 검증
                        float fEQ_0 = Ojw.CConvert.StrToFloat(m_aStrParam[1, 49 - 4]); //B49;
                        float fEQ_1 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 49 - 4]); //B49;
                        fEQ = (fEQ_1 + (fEQ_0 - fEQ_1) / fZ * fAD) * fK;
                    }

                    //fEK = fU + ((float)Math.Cos(fDX) * fEB + (float)Math.Sin(fDX) * fED) * fDY;
                    fEK = fU + (fEB + (float)Math.Sin(fDX) * fED) * fDY;
                    fEL = fEC * fDZ;




                    //fEM = (-(float)Math.Sin(fDX) * fEB + (float)Math.Cos(fDX) * fED) * fEA - fCZ;
                    fEM = ((float)Math.Cos(fDX) * fED) * fEA - fCZ;
                    //fEN = fU + ((float)Math.Cos(fDX) * fEH + (float)Math.Sin(fDX) * fEJ) * fEE;
                    fEN = fU + (fEH + (float)Math.Sin(fDX) * fEJ) * fEE;
                    fEO = fEI * fEF;
                    //fEP = (-(float)Math.Sin(fDX) * fEH + (float)Math.Cos(fDX) * fEJ) * fEG - fCZ;
                    fEP = ((float)Math.Cos(fDX) * fEJ) * fEG - fCZ;


                    fES = (fAL > 0 ? fER * (float)Math.Sin((fAL / fX * 180.0f) / 180.0f * (float)Math.PI) : 0.0f) + fEQ;
                    fET = (fAP > 0 ? fER * (float)Math.Sin((fAP / fX * 180.0f) / 180.0f * (float)Math.PI) : 0.0f) + fEQ;
                    
                    // 허리
                    if (nStart_0_Repeat_1_End_2 == 1)
                        fIC = fIB * (float)Math.Sin(((fM / fZ * 180.0f) + 90.0f) / 180.0f * Math.PI);
                    else
                        fIC = fIB * (float)Math.Sin((-fM / (fZ * 2.0f) * 180.0f) / 180.0f * Math.PI);

                    // 머리
                    if (nStart_0_Repeat_1_End_2 == 1)
                        fIF = fIE * (float)Math.Sin(((fM / fZ * 180.0f) + 90.0f) / 180.0f * Math.PI);
                    else
                        fIF = fIE * (float)Math.Sin((-fM / (fZ * 2.0f) * 180.0f) / 180.0f * Math.PI);
                
                    // fEU;

                    int j = 0;
                    // EV: "E", EW: "1" 은 당연히 붙는 것
                    //=IF(B13="","",CONCATENATE("I",B13))
                    bool bData = false;
                    if ((GetWalkingCount() <= 0) && (nStart_0_Repeat_1_End_2 == 1))
                    {
                    }
                    else
                    {
                        strDatas += String.Format("E\t1\t");

                        // I2 X Y Z
                        bData = (m_aStrParam[nStart_0_Repeat_1_End_2, 13 - 4] == "" ? false : true);
                        if (bData == true) { strDatas += String.Format("I{0}\t{1}\t{2}\t{3}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 13 - 4], fEK, fEL, fEM); }

                        // I3 X Y Z
                        bData = (m_aStrParam[nStart_0_Repeat_1_End_2, 14 - 4] == "" ? false : true);
                        if (bData == true) { strDatas += String.Format("I{0}\t{1}\t{2}\t{3}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 14 - 4], fEN, fEO, fEP); }

                        if ((i == 1) && (nStart_0_Repeat_1_End_2 == 0))
                        {
                            // S 50 D 0
                            strDatas += String.Format("S\t{0}\tD\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 4 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 29 - 4]);
                        }
                        else
                        {
                            // S 50 D 0
                            strDatas += String.Format("S\t{0}\tD\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 28 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 29 - 4]);
                        }

                        // 발목
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 26 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 26 - 4], fBA + fCX); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 25 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 25 - 4], fBF + fCY); }

                        // 엉치
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4], fBI); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 24 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 24 - 4], fBK); }

                        // 팔Up
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 31 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 31 - 4], fDO + fDM); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 32 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 32 - 4], fDP + fDM); }

                        // 팔Wing
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 35 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 35 - 4], fDS + fDQ); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 36 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 36 - 4], fDT + fDQ); }


                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 46 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 46 - 4], fES); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 47 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 47 - 4], fET); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 43 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 43 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]); } // P6


                        if (nStart_0_Repeat_1_End_2 != 1)
                        {
                            // ojw5014
                            // 고관절 Tilt 숙이기 - 미 검증
                            if (m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4] != "")
                            {
                                float fFD_0 = Ojw.CConvert.StrToFloat(m_aStrParam[1, 45 - 4]); //B45;
                                float fFD_1 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]); //B45;
                                float fFD = (fFD_1 + (fFD_0 - fFD_1) / fZ * fAD) * fK;

                                strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4], fFD);
                            } // GE = B45{m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]}                    
                        }
                        else
                        {
                            if (m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]); } // GE = B45{m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]}
                        }

                        //if (m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4], FA9); }


                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 51 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 51 - 4], fBT); } // BM8 = B51
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 52 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 52 - 4], fBU); } // BN8 = B52


                        // 허리
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 54 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 54 - 4], fIC); }
                        // 머리
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 56 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 56 - 4], fIE); } 
                        
                        if (nStart_0_Repeat_1_End_2 != 1)
                        {
                            strDatas += String.Format("X\t-1\t");
                        }
                        strDatas += String.Format("G\t{0}\t", nStart_0_Repeat_1_End_2 + 1);
                        
                        if (m_bMirror == true)
                        {
                            strDatas += String.Format("X\t-1\t");
                        }

                        // 반복 패턴
                        if ((nStart_0_Repeat_1_End_2 == 1) && (GetWalkingCount() > 1) && (i == 1))
                        {
                            if (fK == 1)
                            {
                                strDatas += String.Format("@SET_COMMAND,1\t@SET_DATA0,{0}\t@SET_DATA1,{1}\t", nSize_Frame - 1, GetWalkingCount());
                            }
                        }

                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 58 - 4] != "") { strDatas += String.Format("{0}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 58 - 4]); } // B58

                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 53 - 4] != null)
                            strDatas += Ojw.CConvert.ChangeChar(m_aStrParam[nStart_0_Repeat_1_End_2, 53 - 4], ' ', '\t'); // B53 - 덧붙임 명령어
                        //lstStrFrame.Add(strDatas);
                    }
                    #endregion For
                //}

                //for (int i = 0; i < m_aStrParam.GetLength(1); i++)
                //{
                    //m_aStrParam_Old[nStart_0_Repeat_1_End_2, i] = m_aStrParam[nStart_0_Repeat_1_End_2, i];
                //}
                return strDatas;// lstStrFrame;
#endif

            }
            public List<String> MakeWalkingMotion(int nStart_0_Repeat_1_End_2)//, int nStep)
            {
                int nMode = nStart_0_Repeat_1_End_2;
                List<string> lstStrFrame = new List<string>();
                lstStrFrame.Clear();

                // 1점지지 : m_afParam[nMode, 3]            
                // 2점지지 : m_afParam[nMode, 4]
                int nSize_Frame = Size_Frame(1);// Size_Frame(nStart_0_Repeat_1_End_2);

#if false
                string strDatas;

                // fL => i;
                #region Var
                float fQ4 = 0.5f;
                float fK;
                float fL;
                float fM;
                float fN;
                float fO = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 1]);
                float fP = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 10 - 4]); // sway
                float fQ = fP * fQ4; // 0.5f(fQ4) -> H41 의 대치값 비율
                float fR;
                float fS = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 7 - 4]);
                float fT = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 30 - 4]);
                float fU;
                float fV;
                float fW = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 6 - 4]);
                float fX = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 8 - 4]);
                float fY = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 9 - 4]);
                float fZ = fX + fY;
                // 초기/종료 동작시엔 미러링동작이라 부호가 반대로 된다.
                float fAA = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 41 - 4]);
                float fAB = -fAA;
                fAA = (fAA > 0 ? fAA : 0.0f);//(Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 41 - 4]) > 0 ? Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 41 - 4]) : 0.0f) * (nStart_0_Repeat_1_End_2 == 1 ? 1 : -1);
                fAB = (fAB > 0 ? fAB : 0.0f);
                float fAC;
                float fAD;
                float fAE;
                float fAF;
                float fAG;
                float fAH;
                float fAI;
                float fAJ;
                float fAK;
                float fAL;
                float fAM;
                float fAN;
                float fAO;
                float fAP;
                float fAQ;
                float fAR;
                float fAS;
                float fAT = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 27 - 4]);
                float fAU = fAT * fQ4;
                float fAV;
                float fAW;
                float fAX;

                float fF25 = 1.0f; // 패턴 결정(1-전체, 2-1점지지만)
                float fD25 = 1.0f; // +가 바깥으로 되게 숫자를 조정

                float fAY = fF25; // RST
                float fAZ = fD25; // DIR

                float fBA;
                float fBB;
                float fBC;
                float fF26 = 1.0f; // 패턴 결정(1-전체, 2-1점지지만)
                float fD26 = 1.0f; // +가 바깥으로 되게 숫자를 조정
                float fBD = fF26;
                float fBE = fD26;

                float fBF;
                float fBG = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 22 - 4]);
                float fD23 = -1; // +가 바깥으로 되게 숫자를 조정
                float fBH = fD23;

                float fD24 = -1; // +가 바깥으로 되게 숫자를 조정
                float fBJ = fD24;

                float fD50 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 50 - 4]); // (+오른쪽, -왼쪽)
                float fD51 = fD50; // (+오른쪽, -왼쪽)
                float fBR = fD50;
                float fBS = fD51;
                float fBW = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 11 - 4]);
                float fE19 = 72;// 발바닥 사이간격
                float fF19 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 21 - 4] + fE19);

                float fI19 = fF19 / 2 * (float)Math.Tan((Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 17 - 4])) / 180.0f * Math.PI); // Offset R
                float fI20 = -fI19; // Offset L

                // 초기 동작시엔 CB, CC 가 상수가 아닌 변수가 된다.
                float fCB = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 20 - 4]);
                float fCC = fI19;
                // 초기 동작시
                float fCB_Walk = Ojw.CConvert.StrToFloat(m_aStrParam[1, 20 - 4]); // 반복보행의 파라미터 필요
                float fCB_Start_End = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 20 - 4]); // 현재보행의 파라미터
                float fCB_Result;
                // 종료 동작시


                float fCD = fI20;

                float fCE = fCB + fCC;
                float fCF = fCB + fCD;

                float fCG = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 12 - 4]);
                float fCH = fCG;
                float fCI = fCG;
                float fCN = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 15 - 4]); // B15;
                float fCO = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 16 - 4]); //B16;
                float fE17 = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 17 - 4]); //-B17;
                float fE18 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 17 - 4]); //B17;
                if (nStart_0_Repeat_1_End_2 != 1)
                {
                    fE17 *= -1.0f;
                    fE18 *= -1.0f;
                }
                float fCX = fE17;
                float fCY = fE18;
                float fCZ = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 19 - 4]); //B19;

                float fDA = -fCG;
                float fDB = fCG;

                if (nStart_0_Repeat_1_End_2 != 1)
                {
                    fDA = fDB = 0.0f;
                }

                float fDK = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 21 - 4]); //-B21;
                float fDL = -fDK;
                float fDQ = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 37 - 4]); //B37;
                float fDR = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 38 - 4]); //B38;

                float fD13 = 1;
                float fE13 = 1;
                float fF13 = 1;
                float fD14 = 1;
                float fE14 = 1;
                float fF14 = 1;

                float fDW = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 40 - 4]) * ((nStart_0_Repeat_1_End_2 != 1) ? -1 : 1); //B40;
                float fDX = fDW / 180.0f * (float)Math.PI;
                float fDY = fD13;
                float fDZ = fE13;

                float fEA = fF13;
                float fEE = fD14;
                float fEF = fE14;
                float fEG = fF14;
                float fEH = fDL;
                float fEQ = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 49 - 4]); //B49;
                float fER = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 48 - 4]); //B48;
                float fBI;
                float fBK;
                float fBL;
                float fBM = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 51 - 4]); //B51;
                float fBN = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 52 - 4]); //B52;
                float fBO;
                float fBP;
                float fBQ;
                float fBT;
                float fBU;
                //float fBV; 
                float fBX;
                float fBY;
                float fBZ;


                float fCA;
                float fCJ;
                float fCK;
                float fCL;
                float fCM;
                float fCP;
                float fCQ;
                float fCR;
                float fCS;
                float fCT;
                float fCU;
                float fCV;
                float fCW;
                float fDC;
                float fDD;
                float fDE;
                //float fDF;
                float fDG;
                float fDH;
                float fDI;
                //float fDJ;

                float fDM = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 33 - 4]); //-B33;
                float fDN = (Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 12 - 4]) >= 0) ? Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 34 - 4]) : -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 34 - 4]); //IF(CG8 >= 0, B34, -B34);
                float fDO;
                float fDP;
                float fDS;
                float fDT;
                float fDU = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 39 - 4]); //B39;
                float fDV;

                float fEB = fDK; // X 값, 상수지만 나중 어떻게 될지는...
                float fEC;
                float fED;
                float fEI;
                float fEJ;


                float fEK;
                float fEL;
                float fEM;
                float fEN;
                float fEO;
                float fEP;


                float fES;
                float fET;
#if false
           
            

            float fEV;
            float fEW;
            float fEX;
            float fEY;
            float fEZ;
            
            
            float fA;
            float fB;
            float fC;
            float fD;
            float fE;
            float fF;
            float fG;
            float fH;
            float fI;
            float fJ;
            float fK;
            float fL;
            float fM;
            float fN;
            float fO;
            float fP;
            float fQ;
            float fR;
            float fS;
            float fT;
            float fU;
            float fV;
            float fW;
            float fX;
            float fY;
            float fZ;

            
            float fA;
            float fB;
            float fC;
            float fD;
            float fE;
            float fF;
            float fG;
            float fH;
            float fI;
            float fJ;
            float fK;
            float fL;
            float fM;
            float fN;
            float fO;
            float fP;
            float fQ;
            float fR;
            float fS;
            float fT;
            float fU;
            float fV;
            float fW;
            float fX;
            float fY;
            float fZ;
#endif
                #endregion Var
                //float fAL8 = 0.0f;
                //float fAP8 = 0.0f;
                //float fAK8 = 0.0f;
                //float fAO8 = 0.0f;
                //float fN8 = 0.0f;
                //float fBP8 = 0.0f;
                //float fCJ8 = 0.0f;
                //float fCK8 = 0.0f;
                for (int i = 1; i <= nSize_Frame; i++)
                {
                    strDatas = String.Empty;

                    fK = (i <= nSize_Frame ? 1.0f : 0.0f); // En
                    fL = i;
                    fM = i * fK;
                    //fU = (float)Math.Round(fR * Math.Sin((((fO == 0 ? fM : fN) - fS) / (fZ) * 180.0f) / 180.0f * Math.PI), 3) + fT;
                    //fO
                    fV = fM - 1.0f;
                    fAC = ((float)((int)fV % (int)fZ) + 1.0f) * fK;
                    fAH = (((fAC - (fY + fW)) <= fX && fAC > (fY + fW)) ? 1 : 0);
                    fAE = (fAC - (fY + fW)) * fAH;
                    fAF = (float)Math.Round((fV + 0.001f) / fZ - 0.5f, 0);
                    fAG = (float)Math.Pow(-1.0f, fAF) * fK;
                    fAI = fAH * fAG;
                    fAJ = (fAI > 0 ? 1 : 0);
                    fAN = (fAI < 0 ? 1 : 0);
                    // fAP 수식변경(20190515)
                    fAP = (fAN == 0 ? 0 : fAE); //(fAN == 0 ? 0 : fAN + fAP8); fAP8 = fAP;
                    // fAL 수식변경(20190515)
                    fAL = (fAJ == 0 ? 0 : fAE);// (fAJ == 0 ? 0 : fAJ + fAL8); fAL8 = fAL;

                    fAS = (fAG < 0 ? 1.0f : 0.0f);
                    fAR = (fAG > 0 ? 1.0f : 0.0f);
                    // fAK 수식변경(20190515)
                    fAK = (fAR == 0 ? 0 : fAC); //(fAR == 0 ? 0 : fAR + fAK8); fAK8 = fAK;
                    // fAO 수식변경(20190515)
                    fAO = (fAS == 0 ? 0 : fAC); //(fAS == 0 ? 0 : fAS + fAO8); fAO8 = fAO;
                    // fN 수식 변경(20190515)
                    fN = fAK - fAL + fAO - fAP + (fAL + fAP > 0 ? 1.0f : 0.0f) + fK * fAF * fZ;//((fAL + fAP > 0 && fAL + fAP != fX) ? fN8 : fM); fN8 = fN;
                    fR = (fAA + fAB != 0 ? ((fM <= fZ && fAB != 0) ? fP : fQ) * fAR + ((fM > fZ && fAA != 0) ? fP : fQ) * fAS : fP * fK); // fAS, fAR
                    fU = (float)(fR * (float)Math.Sin((((fO == 0 ? fM : fN) - fS) / (fZ) * 180.0f) / 180.0f * Math.PI)) + fT;

                    fAD = fAF * fZ + fAG * fAC + fAF;
                    fAM = (float)Math.Round((fAL > fAA ? (fAL - fAA) / (fX - fAA) * fX : 0), 3);
                    fAQ = (float)Math.Round((fAP > fAB ? (fAP - fAB) / (fX - fAB) * fX : 0.0f), 3);
                    fAV = (fAA + fAB != 0 ? ((fM <= fZ && fAB != 0) ? fAT : fAU) * fAR + ((fM > fZ && fAA != 0) ? fAT : fAU) * fAS : fAT * fK);
                    fAW = fAV * (float)Math.Sin(((fO == 0 ? fAK : fN) / fZ * 180.0f) / 180.0f * Math.PI);
                    fAX = fAV * (float)Math.Sin((fAL / fX * 180.0f) / 180.0f * Math.PI);

                    fBA = (fAY == 1 ? fAW : fAX) * fAZ;
                    fBB = fAV * (float)Math.Sin((fAO / fZ * 180.0f) / 180.0f * Math.PI);
                    fBC = fAV * (float)Math.Sin((fAP / fX * 180.0f) / 180.0f * Math.PI);
                    fBF = (fBD == 1 ? fBB : fBC) * fBE;

                    fBI = fBG * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI) * fBH;
                    fBK = fBG * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI) * fBJ;

                    fBL = (fAD - (fY + fW)) * fK;
                    fBO = (fAR + fAJ + fAS + fAN - 1.0f) * fAG;
                    // fBP 수식변경(20190515)
                    fBP = (fBL < 0 ? 0 : (fBL > fX ? fX : fBL)); //fBP8 + fBO; fBP8 = fBP;
                    fBQ = (fBO < 0 ? fBP + 1.0f : fBP);



                    //////////////////////////////////////////////////////////////////
                    fBT = (fBR * fBQ / fX);
                    fBU = (fBS * fBQ / fX);

                    fBX = fAJ * fBW * fK;
                    fBY = (float)Math.Round(fBX * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI), 3);
                    fBZ = fAN * fBW * fK;


                    fCA = (float)Math.Round(fBZ * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI), 3);

                    // fCJ 수식변경(20190515)
                    fCJ = (fM > (fZ + fW) ? fCG * 2.0f : fCG / fX * fAL * 2.0f);//(fAL > 0 ? fCH / fX * fAL * 2.0f : fCJ8); fCJ8 = fCJ;
                    // fCK 수식변경(20190515)
                    fCK = (fM > (fZ + fW + fY + fX) ? fCG * 2.0f : fCG / fX * fAP * 2.0f);//(fAP > 0 ? fCI / fX * fAP * 2.0f : fCK8); fCK8 = fCK;

                    fCL = fCH * (fM / (fZ * 2.0f) * 2.0f);
                    fCM = fCI * (fM / (fZ * 2.0f) * 2.0f);

                    fCP = fCN * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI);
                    fCQ = fCO * (float)Math.Sin((fAM / fX * 180.0f) / 180.0f * (float)Math.PI);
                    fCR = fAM / fZ * 2.0f;
                    fCS = (fCR * fCP - (2.0f - fCR) * fCQ);
                    fCT = fCN * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI);
                    fCU = fCO * (float)Math.Sin((fAQ / fX * 180.0f) / 180.0f * (float)Math.PI);
                    fCV = fAQ / fZ * 2.0f;
                    fCW = (fCV * fCT - (2.0f - fCV) * fCU);


                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        fCZ = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 19 - 4]);
                        float fCZ_1 = Ojw.CConvert.StrToFloat(m_aStrParam[1, 19 - 4]);
                        fCZ = (fCZ + (fCZ_1 - fCZ) / fZ * fAD) * fK;
                    }

                    fDC = (fCJ - fCL) + fCS;
                    fDD = fDC * ((nStart_0_Repeat_1_End_2 != 1) ? 1.0f : 2.0f) + fDA;
                    fDE = fDC;
                    //fDF;
                    fDG = (fCK - fCM) + fCW;
                    fDH = fDG * ((nStart_0_Repeat_1_End_2 != 1) ? 1.0f : 2.0f) + fDB;
                    fDI = fDG;
                    //fDJ;


                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        fDN = (Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 12 - 4]) >= 0) ? Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 34 - 4]) : -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 34 - 4]); //IF(CG8 >= 0, B34, -B34);
                        float fDN_1 = (Ojw.CConvert.StrToFloat(m_aStrParam[1, 12 - 4]) >= 0) ? Ojw.CConvert.StrToFloat(m_aStrParam[1, 34 - 4]) : -Ojw.CConvert.StrToFloat(m_aStrParam[1, 34 - 4]); //IF(CG8 >= 0, B34, -B34);
                        fDN = (fDN + (fDN_1 - fDN) / fZ * fAD) * fK;
                    }


                    fDO = fDN * (float)Math.Sin((fM / (fZ * 2.0f) * 180.0f) / 180.0f * (float)Math.PI) - fDN / 2.0f;
                    fDP = -fDO;

                    fDS = (float)Math.Abs(fDR * (float)Math.Sin((fM / (fZ) * 180.0f) / 180.0f * (float)Math.PI)) + fDQ;
                    fDT = fDS;
                    fDV = -(float)Math.Abs(fDU * (float)Math.Sin((fM / (fZ) * 180.0f) / 180.0f * (float)Math.PI)) * (fDU >= 0 ? 1.0f : -1.0f);

                    // 시작/종료 보행시는 아래의 파라미터가 가변이 된다.
                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        fCB_Result = (fCB_Start_End + (fCB_Walk - fCB_Start_End) / fZ * fAD) * fK;
                        //fCB_Result = (fCB_Start_End + (fCB_Walk - fCB_Start_End) / fZ * fAD) * fK;

                        // 앉기(CB) + Offset(R : CC)
                        // 앉기(CB) + Offset(L : CD)
                        fCE = fCB_Result + fCC;
                        fCF = fCB_Result + fCD;
                    }

                    fEC = fBY + fCE + fDV;
                    fED = (float)Math.Round(fDD, 3);


                    fEI = fCA + fCF + fDV;
                    fEJ = (float)Math.Round(fDH, 3);


                    if (nStart_0_Repeat_1_End_2 != 1)
                    {
                        float fDK_0 = -Ojw.CConvert.StrToFloat(m_aStrParam[1, 21 - 4]);
                        float fDK_1 = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 21 - 4]);
                        fDK = (fDK_1 + (fDK_0 - fDK_1) / fX * fBQ) * fK;
                        fDL = -fDK;
                        fEB = fDK;
                        fEH = fDL;

                        // 팔 Up Shift
                        float fDM_0 = -Ojw.CConvert.StrToFloat(m_aStrParam[1, 33 - 4]); //-B33;
                        float fDM_1 = -Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 33 - 4]); //-B33;
                        fDM = (fDM_1 + (fDM_0 - fDM_1) / fZ * fAD) * fK;

                        // 팔 Up 동작각
                        float fDQ_0 = Ojw.CConvert.StrToFloat(m_aStrParam[1, 37 - 4]); //B37;
                        float fDQ_1 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 37 - 4]); //B37;
                        fDQ = (fDQ_1 + (fDQ_0 - fDQ_1) / fZ * fAD) * fK;

                        // ojw5014
                        // 발목 숙이기 - 미 검증
                        float fEQ_0 = Ojw.CConvert.StrToFloat(m_aStrParam[1, 49 - 4]); //B49;
                        float fEQ_1 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 49 - 4]); //B49;
                        fEQ = (fEQ_1 + (fEQ_0 - fEQ_1) / fZ * fAD) * fK;
                    }

                    fEK = fU + ((float)Math.Cos(fDX) * fEB + (float)Math.Sin(fDX) * fED) * fDY;
                    fEL = fEC * fDZ;




                    fEM = (-(float)Math.Sin(fDX) * fEB + (float)Math.Cos(fDX) * fED) * fEA - fCZ;
                    fEN = fU + ((float)Math.Cos(fDX) * fEH + (float)Math.Sin(fDX) * fEJ) * fEE;
                    fEO = fEI * fEF;
                    fEP = (-(float)Math.Sin(fDX) * fEH + (float)Math.Cos(fDX) * fEJ) * fEG - fCZ;


                    fES = (fAL > 0 ? fER * (float)Math.Sin((fAL / fX * 180.0f) / 180.0f * (float)Math.PI) : 0.0f) + fEQ;
                    fET = (fAP > 0 ? fER * (float)Math.Sin((fAP / fX * 180.0f) / 180.0f * (float)Math.PI) : 0.0f) + fEQ;
                    // fEU;

                    int j = 0;
                    // EV: "E", EW: "1" 은 당연히 붙는 것
                    //=IF(B13="","",CONCATENATE("I",B13))
                    bool bData = false;
                    if ((GetWalkingCount() <= 0) && (nStart_0_Repeat_1_End_2 == 1))
                    {
                    }
                    else
                    {
                        strDatas += String.Format("E\t1\t");

                        // I2 X Y Z
                        bData = (m_aStrParam[nStart_0_Repeat_1_End_2, 13 - 4] == "" ? false : true);
                        if (bData == true) { strDatas += String.Format("I{0}\t{1}\t{2}\t{3}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 13 - 4], fEK, fEL, fEM); }

                        // I3 X Y Z
                        bData = (m_aStrParam[nStart_0_Repeat_1_End_2, 14 - 4] == "" ? false : true);
                        if (bData == true) { strDatas += String.Format("I{0}\t{1}\t{2}\t{3}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 14 - 4], fEN, fEO, fEP); }

                        if ((i == 1) && (nStart_0_Repeat_1_End_2 == 0))
                        {
                            // S 50 D 0
                            strDatas += String.Format("S\t{0}\tD\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 4 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 29 - 4]);
                        }
                        else
                        {
                            // S 50 D 0
                            strDatas += String.Format("S\t{0}\tD\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 28 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 29 - 4]);
                        }

                        // 발목
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 26 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 26 - 4], fBA + fCX); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 25 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 25 - 4], fBF + fCY); }

                        // 엉치
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4], fBI); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 24 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 24 - 4], fBK); }

                        // 팔Up
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 31 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 31 - 4], fDO + fDM); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 32 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 32 - 4], fDP + fDM); }

                        // 팔Wing
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 35 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 35 - 4], fDS + fDQ); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 36 - 4] != "") { strDatas += String.Format("T{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 36 - 4], fDT + fDQ); }


                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 46 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 46 - 4], fES); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 47 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 47 - 4], fET); }
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 43 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 43 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]); } // P6


                        if (nStart_0_Repeat_1_End_2 != 1)
                        {
                            // ojw5014
                            // 고관절 Tilt 숙이기 - 미 검증
                            if (m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4] != "")
                            {
                                float fFD_0 = Ojw.CConvert.StrToFloat(m_aStrParam[1, 45 - 4]); //B45;
                                float fFD_1 = Ojw.CConvert.StrToFloat(m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]); //B45;
                                float fFD = (fFD_1 + (fFD_0 - fFD_1) / fZ * fAD) * fK;

                                strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4], fFD);
                            } // GE = B45{m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]}                    
                        }
                        else
                        {
                            if (m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 44 - 4], m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]); } // GE = B45{m_aStrParam[nStart_0_Repeat_1_End_2, 45 - 4]}
                        }

                        //if (m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 23 - 4], FA9); }


                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 51 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 51 - 4], fBT); } // BM8 = B51
                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 52 - 4] != "") { strDatas += String.Format("P{0}\t{1}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 52 - 4], fBU); } // BN8 = B52

                        if (nStart_0_Repeat_1_End_2 != 1)
                        {
                            strDatas += String.Format("X\t-1\t");
                        }
                        strDatas += String.Format("G\t{0}\t", nStart_0_Repeat_1_End_2 + 1);


                        if (m_bMirror == true)
                        {
                            strDatas += String.Format("X\t-1\t");
                        }

                        // 반복 패턴
                        if ((nStart_0_Repeat_1_End_2 == 1) && (GetWalkingCount() > 1) && (i == 1))
                        {
                            if (fK == 1)
                            {
                                strDatas += String.Format("@SET_COMMAND,1\t@SET_DATA0,{0}\t@SET_DATA1,{1}\t", nSize_Frame - 1, GetWalkingCount());
                            }
                        }

                        if (m_aStrParam[nStart_0_Repeat_1_End_2, 58 - 4] != "") { strDatas += String.Format("{0}\t", m_aStrParam[nStart_0_Repeat_1_End_2, 58 - 4]); } // B58

                        strDatas += Ojw.CConvert.ChangeChar(m_aStrParam[nStart_0_Repeat_1_End_2, 53 - 4], ' ', '\t'); // B53 - 덧붙임 명령어
                        lstStrFrame.Add(strDatas);
                    }
                }

                for (int i = 0; i < m_aStrParam.GetLength(1); i++)
                {
                    m_aStrParam_Old[nStart_0_Repeat_1_End_2, i] = m_aStrParam[nStart_0_Repeat_1_End_2, i];
                }
                return lstStrFrame;
#else
                for (int i = 1; i <= nSize_Frame; i++)
                {
                    lstStrFrame.Add(MakeWalkingMotion(nMode, i));
                }
                for (int i = 0; i < m_aStrParam.GetLength(1); i++)
                {
                    m_aStrParam_Old[nStart_0_Repeat_1_End_2, i] = m_aStrParam[nStart_0_Repeat_1_End_2, i];
                }
                return lstStrFrame;
#endif

            }
            private bool m_bMirror = false;
            public void SetMirror(bool bMirror) { m_bMirror = bMirror; }
            public bool GetMirror() { return m_bMirror; }
            public SWalking_t[] m_SWalking;// = new SWalking_t[];
            public struct SWalking_t
            {
                public List<String> lstStrDatas;// = new List<String>();
            }
        }
    }
}
