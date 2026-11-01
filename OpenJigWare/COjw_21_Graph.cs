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
        public class CGraph
        {
            private int[,] m_anHistory = null;
            private Pen[] m_apenPen = null;
            private Pen m_penSeparation = null;
            private int m_nHistorySize = 0;
            private int m_nModelCount = 0;
            //private int m_nIndex_Start = 0;
            //private int m_nIndex_End = 0;
            private bool m_bValid = false;
            private Label m_lbDraw;
            private int[] m_anOffsetY = null;
            private Bitmap m_bmpBack = null;
            private bool m_bBmpLoaded = false;
            private Color m_cBackColor = Color.White;
            public CGraph(Label lbDraw, int nHistorySize, Color cBackColor, Bitmap bmpBack, params Color[] acModel)
            {
                if (lbDraw == null)
                {
                    Ojw.CMessage.Write_Error("Label == null");
                    return;
                }
                if (bmpBack != null)
                {
                    if ((bmpBack.Width == lbDraw.Width) && (bmpBack.Height == lbDraw.Height))
                    m_bmpBack = bmpBack;
                }
                m_cBackColor = cBackColor;

                m_nModelCount = acModel.Length;

                m_apenPen = new Pen[m_nModelCount];
                m_penSeparation = new Pen(Color.Black);
                m_nHistorySize = nHistorySize;
                //m_nIndex_Start = 0;
                //m_nIndex_End = m_nHistorySize;
                m_anHistory = new int[m_nModelCount, nHistorySize];
                //Array.Resize<string[]>(ref m_anHistory, m_nModelCount);
                //Array.Resize<string>(ref m_anHistory[1], nHistorySize);
                Array.Clear(m_anHistory, 0, m_nModelCount * nHistorySize);

                m_anOffsetY = new int[m_nModelCount];

                for (int i = 0; i < m_nModelCount; i++)
                {
                    m_apenPen[i] = new Pen(acModel[i]);
                    m_anOffsetY[i] = lbDraw.Height / (m_nModelCount + 1) * (i + 1);
                }

                m_lbDraw = lbDraw;
                m_gDraw = Graphics.FromHwnd(lbDraw.Handle);

                //if (m_bmpBack == null) bmpDisp = new Bitmap(m_lbDraw.Width, m_lbDraw.Height);//this.Width, this.Height);
                //else bmpDisp = m_bmpBack;
                if (m_bmpBack == null)
                {
                    m_bBmpLoaded = false;
                    m_bmpBack = new Bitmap(m_lbDraw.Width, m_lbDraw.Height);//this.Width, this.Height);
                }
                else m_bBmpLoaded = true;


                m_bValid = true;
            }
            ~CGraph()
            {
                m_bValid = false;
                if (m_apenPen != null)
                {
                    for (int i = 0; i < m_apenPen.Length; i++) m_apenPen[i].Dispose();
                }
                m_penSeparation.Dispose();
            }
            private int m_nIndex = 0;

            private bool m_bOver = false;
            public void Push(params int [] anData)
            {
                if (m_bValid)
                {
                    if (anData.Length >= m_nModelCount)
                    {
                        for (int i = 0; i < m_nModelCount; i++)
                            m_anHistory[i, m_nIndex] = anData[i];
                        lock (m_objMarkLock) m_lPushCount++;   // 마커가 붙을 샘플 번호 (0 부터)
                        if (m_bOver == false)
                        {
                            if ((m_nIndex + 1) == m_nHistorySize) m_bOver = true;
                            m_nIndex = (m_nIndex + 1) % m_nHistorySize;
                        }
                        else
                        {
                            //m_nIndex_Start = 
                            m_nIndex = (m_nIndex + 1) % m_nHistorySize;
                        }
                    }
                    else
                    {
                        Ojw.CMessage.Write("[Warning] Data Mismatch");
                    }
                }
            }
            // 선(모델)별 기준선 y 위치 지정 — 기본은 높이를 (모델 수 + 1) 로 균등 분할.
            //   두 선을 같은 기준선에 겹쳐 비교하거나(예: 측정값과 계산값), 레인 수와 선 수를 다르게 쓸 때 사용
            public void SetOffsetY(int nModel, int nOffsetY)
            {
                if ((m_anOffsetY != null) && (nModel >= 0) && (nModel < m_nModelCount)) m_anOffsetY[nModel] = nOffsetY;
            }
            public int GetOffsetY(int nModel)
            {
                if ((m_anOffsetY != null) && (nModel >= 0) && (nModel < m_nModelCount)) return m_anOffsetY[nModel];
                return 0;
            }
            public int GetCount_Model() { return m_nModelCount; }

            #region 마커(점) — 그래프 중간의 특정 샘플에 점 + 라벨 (예: 최솟값 "best 0.31 @ 9")
            // 점은 Push 한 샘플에 붙는다. 스크롤 모드에서는 데이터와 함께 흘러가고(덮어쓰기 모드에서는 그 칸에 머문다),
            // 그 샘플이 버퍼에서 밀려나면 점도 함께 사라진다.
            // 사용 예) 새 최솟값이 들어올 때마다 점을 옮기기:
            //     CGrp.Push(nTrain, nVal);
            //     if (nVal < nBest) { nBest = nVal; CGrp.ClearMark(1); CGrp.Mark(1, Color.Red, "best " + nBest); }
            //   지나간 샘플에 찍기: CGrp.Mark(1, 8, Color.Red, "best")  ← 8 번 전에 Push 한 샘플
            private class CMark_t
            {
                public int nModel;          // 선(모델) 번호
                public long lSeq;           // 몇 번째 Push 의 샘플인가 (0 부터)
                public int nValue;          // 점의 값 = 그 샘플의 값
                public Color cColor;
                public int nSize;           // 지름(픽셀)
                public string strLabel;     // 점 옆 글자 (null 또는 "" = 없음)
            }
            private readonly List<CMark_t> m_lstMark = new List<CMark_t>();
            private readonly object m_objMarkLock = new object();
            private long m_lPushCount = 0;  // 지금까지 받아들인 Push 횟수

            /// <summary>가장 최근에 Push 한 샘플(선 nModel)에 점을 찍는다. strLabel = 점 옆 글자, nSize = 지름(픽셀)</summary>
            public bool Mark(int nModel, Color cColor, string strLabel = null, int nSize = 8)
            {
                return Mark(nModel, 0, cColor, strLabel, nSize);
            }
            /// <summary>nAgo 번 전에 Push 한 샘플(0 = 가장 최근)에 점을 찍는다 — 지나고 나서 찾은 최솟값·최댓값 표시용.
            /// 버퍼에 없는 샘플(nAgo 가 음수이거나 기록 길이 이상)이면 false</summary>
            public bool Mark(int nModel, int nAgo, Color cColor, string strLabel = null, int nSize = 8)
            {
                if ((m_bValid == false) || (nModel < 0) || (nModel >= m_nModelCount)) return false;
                lock (m_objMarkLock)
                {
                    long lAvail = Math.Min(m_lPushCount, (long)m_nHistorySize);
                    if ((nAgo < 0) || (nAgo >= lAvail)) return false;
                    CMark_t CMark = new CMark_t();
                    CMark.nModel = nModel;
                    CMark.lSeq = m_lPushCount - 1 - nAgo;
                    CMark.nValue = m_anHistory[nModel, (int)(CMark.lSeq % m_nHistorySize)];
                    CMark.cColor = cColor;
                    CMark.nSize = Math.Max(2, nSize);
                    CMark.strLabel = strLabel;
                    m_lstMark.Add(CMark);
                }
                return true;
            }
            /// <summary>모든 점 지우기</summary>
            public void ClearMark() { lock (m_objMarkLock) m_lstMark.Clear(); }
            /// <summary>선 nModel 의 점만 지우기 (새 최솟값으로 점을 옮길 때)</summary>
            public void ClearMark(int nModel) { lock (m_objMarkLock) m_lstMark.RemoveAll(m => m.nModel == nModel); }
            /// <summary>화면에 남아 있는 점 개수</summary>
            public int GetCount_Mark()
            {
                lock (m_objMarkLock)
                {
                    RemoveExpiredMark();
                    return m_lstMark.Count;
                }
            }
            // 버퍼에서 밀려난 샘플의 점은 지운다 (lock 안에서 호출)
            private void RemoveExpiredMark()
            {
                long lCount = m_lPushCount;
                int nSize = m_nHistorySize;
                m_lstMark.RemoveAll(m => (lCount - 1 - m.lSeq) >= nSize);
            }
            private void DrawMark(Graphics g, float fStretchWidth, float fStretchHeight)
            {
                CMark_t[] aCMark;
                long lCount;
                lock (m_objMarkLock)
                {
                    RemoveExpiredMark();
                    if (m_lstMark.Count == 0) return;
                    aCMark = m_lstMark.ToArray();
                    lCount = m_lPushCount;
                }
                // 점 테두리 = 배경과 대비되는 색 (밝은 배경 → 검정, 어두운 배경 → 흰색)
                float fLum = 0.299f * m_cBackColor.R + 0.587f * m_cBackColor.G + 0.114f * m_cBackColor.B;
                Color cEdge = (fLum > 128.0f) ? Color.Black : Color.White;
                Font font = m_lbDraw.Font;
                int nWidth = m_lbDraw.Width;
                using (Pen penEdge = new Pen(cEdge, 1))
                {
                    foreach (CMark_t CMark in aCMark)
                    {
                        long lAge = lCount - 1 - CMark.lSeq;                       // 0 = 가장 최근 샘플
                        int nSlot = (int)(CMark.lSeq % m_nHistorySize);           // 버퍼 칸
                        // 선과 같은 규칙으로 위치를 잡는다: 스크롤 = 최근 샘플이 왼쪽(x=0) / 덮어쓰기 = 버퍼 칸 위치
                        float fX = (float)Math.Round((m_bOverwrite ? nSlot : lAge) * fStretchWidth, 0);
                        float fY = m_anOffsetY[CMark.nModel] - (float)Math.Round(CMark.nValue * fStretchHeight, 0);
                        float fR = CMark.nSize / 2.0f;
                        using (SolidBrush brush = new SolidBrush(CMark.cColor))
                        {
                            g.FillEllipse(brush, fX - fR, fY - fR, CMark.nSize, CMark.nSize);
                            g.DrawEllipse(penEdge, fX - fR, fY - fR, CMark.nSize, CMark.nSize);
                            if (string.IsNullOrEmpty(CMark.strLabel)) continue;
                            SizeF szText = g.MeasureString(CMark.strLabel, font);
                            float fTx = fX + fR + 3;                                    // 점 오른쪽 위
                            if (fTx + szText.Width > nWidth) fTx = fX - fR - 3 - szText.Width;   // 오른쪽 끝이면 왼쪽에
                            if (fTx < 0) fTx = 0;
                            float fTy = fY - fR - szText.Height;
                            if (fTy < 0) fTy = fY + fR;                                 // 위가 모자라면 아래에
                            g.DrawString(CMark.strLabel, font, brush, fTx, fTy);
                        }
                    }
                }
            }
            #endregion 마커(점)

            public int GetCount_History() { return m_nHistorySize; }
            public int Pop(int nModelNum, int nIndex) 
            {
                int nPos = (m_nIndex - nIndex) % m_nHistorySize;

                if (nModelNum < 0) nModelNum = 0;
                if (nModelNum >= m_nModelCount) nModelNum = m_nModelCount - 1;
                //if ((nIndex < 0) || (nIndex >= m_nHistorySize)) return 0;
                if (nPos < 0) nPos = nPos + m_nHistorySize;
                if (nPos >= m_nHistorySize) nPos = nPos - m_nHistorySize;
                return m_anHistory[nModelNum, nPos]; 
            }
            private int m_nBmpIndex = 0;
            private Bitmap[] m_abmpDisp = new Bitmap[2]; // 메모리 이미지 => 더블 버퍼링 기법
            private Graphics m_gDraw;
            public void OjwDraw()
            {
                if (m_bValid)
                {
                    HistoryToBmp();
                    if (m_abmpDisp[m_nBmpIndex] != null)
                    {
                        // Double buffering
                        m_gDraw.DrawImage(m_abmpDisp[m_nBmpIndex], 0, 0);
                    }
                }
            }
            //private float m_fGrade_Line_Normal = 100.0f;//
            //private float m_fGrade_Line_Dot = 50.0f;//
            private const int _INTERVAL_TIME_VALUE = 50;
            private float m_fStretch_Width = 1.0f;
            private float m_fStretch_Height = 1.0f;//0.8f;//1.0f;
            private bool m_bOverwrite = false;
            public void SetOverwrite(bool bOn) { m_bOverwrite = bOn; }
            private void HistoryToBmp()
            {
                if (m_bValid)
                {
                    //Bitmap bmpDisp = iThermo.Properties.Resources.Graph_Back;
                    using (Graphics g = Graphics.FromImage(m_bmpBack))
                    {
                        if (m_bBmpLoaded == false) g.Clear(m_cBackColor);

                        Pen pen;
                        float fStretchWidth = m_fStretch_Width;
                        float fStretchHeight = m_fStretch_Height;

                        if (m_bOverwrite)
                        {
                            for (int j = 0; j < m_nModelCount; j++)
                            {
                                for (int i = 1; i < m_nHistorySize; i++)
                                {
                                    int nPos0 = (int)Math.Round((i - 1) * fStretchWidth, 0);// m_nHistorySize - (int)Math.Round((i - 1) * fStretchWidth, 0);
                                    int nPos1 = (int)Math.Round(i * fStretchWidth, 0);//m_nHistorySize - (int)Math.Round(i * fStretchWidth, 0);
                                    g.DrawLine(m_apenPen[j], nPos0, m_anOffsetY[j] - (int)Math.Round(m_anHistory[j, i - 1] * fStretchHeight, 0), nPos1, m_anOffsetY[j] - (int)Math.Round(m_anHistory[j, i] * fStretchHeight, 0));
                                }
                            }
                        }
                        else
                        {
                            int nIndex_Curr = 0;
                            int nIndex_Prev = 0;
                            int nIndex = m_nIndex - 1;// +1;
                            for (int j = 0; j < m_nModelCount; j++)
                            {
                                for (int i = 0; i < m_nHistorySize; i++)
                                {
                                    nIndex_Curr = (nIndex - i) % m_nHistorySize;
                                    nIndex_Prev = (nIndex - (i - 1)) % m_nHistorySize;// i - 1;
                                    if (nIndex_Curr < 0) nIndex_Curr += m_nHistorySize;
                                    if (nIndex_Prev < 0) nIndex_Prev += m_nHistorySize;
                                    if ((nIndex_Prev - nIndex_Curr) != 1) continue;
                                    int nPos0 = (int)Math.Round((i - 1) * fStretchWidth, 0);// m_nHistorySize - (int)Math.Round((nIndex_Prev) * fStretchWidth, 0);
                                    int nPos1 = (int)Math.Round(i * fStretchWidth, 0);//m_nHistorySize - (int)Math.Round(nIndex_Curr * fStretchWidth, 0);
                                    g.DrawLine(m_apenPen[j], nPos0, m_anOffsetY[j] - (int)Math.Round(m_anHistory[j, nIndex_Prev] * fStretchHeight, 0), nPos1, m_anOffsetY[j] - (int)Math.Round(m_anHistory[j, nIndex_Curr] * fStretchHeight, 0));
                                }
                            }
                        }

                        SolidBrush brush = new SolidBrush(Color.Cyan);
                        Point pntData0 = new Point();
                        Point pntData1 = new Point();
                        Point pntData2 = new Point();

                        //int nHeight = 0;

                        pntData0.X = 0;
                        pntData1.X = m_nHistorySize;
                        pntData2.X = (pntData0.X + pntData1.X) / 2;//pntData1.X - 250;
                        #region 도트라인들...
                        //float fGrade = m_fGrade_Line_Dot;//50.0f;
                        //int nCnt = (int)Math.Round((m_fMax_Temp - m_fMin_Temp) / fGrade + 0.5f, 0);
                        //pen = new Pen(Color.Yellow, 1);
                        //pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
                        //for (int i = 1; i <= nCnt; i++)
                        //{
                        //    pntData0.Y = nOffsetY - (int)Math.Round(nHeight * fStretchHeight, 0);
                        //    pntData1.Y = pntData0.Y;

                        //    nHeight = (int)Math.Round(fGrade * i - m_fMin_Temp, 0);

                        //    //g.DrawLine(m_apenPen[1], pntData0.X, pntData0.Y, pntData1.X, pntData1.Y);
                        //    g.DrawLine(pen, pntData0.X, pntData0.Y, pntData1.X, pntData1.Y);

                        //    //pntData2.Y = pntData1.Y;
                        //    //g.DrawString(Ojw.CConvert.IntToStr((int)Math.Round((i - 1) * fGrade, 0)), font, brush, pntData2);
                        //}
                        #endregion 도트라인들...

                        #region 라인들...
                        //nHeight = 0;
                        //fGrade = m_fGrade_Line_Normal;
                        ////nCnt = (int)Math.Round((m_fMax_Temp - m_fMin_Temp) / fGrade + 0.5f, 0);
                        //nCnt = (int)Math.Round((m_fMax_Temp - m_fMin_Temp) / fGrade, 0) + 1;
                        //pen = new Pen(Color.Blue, 1);
                        //pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                        //for (int i = 1; i <= nCnt; i++)
                        //{
                        //    if (((i - 1) * fGrade) >= (m_fMax_Temp - m_fMin_Temp)) break;

                        //    pntData0.Y = nOffsetY - (int)Math.Round(nHeight * fStretchHeight, 0);
                        //    pntData1.Y = pntData0.Y;

                        //    nHeight = (int)Math.Round(fGrade * i - m_fMin_Temp, 0);

                        //    //g.DrawLine(m_apenPen[1], pntData0.X, pntData0.Y, pntData1.X, pntData1.Y);
                        //    g.DrawLine(pen, pntData0.X, pntData0.Y, pntData1.X, pntData1.Y);
                        //    pntData2.Y = pntData1.Y;
                        //    g.DrawString(Ojw.CConvert.IntToStr((int)Math.Round((i - 1) * fGrade, 0)), font, brush, pntData2);
                        //}
                        #endregion 라인들...

                        #region 세로라인들...
                        pen = new Pen(Color.Orange, 1);
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        int nX = 0;
                        float fTimeVal = 5000; // 초단위 격자 만들기
                        for (int i = 1; i <= 10; i++)
                        {
                            nX = (int)Math.Round((float)i * fTimeVal / (float)_INTERVAL_TIME_VALUE * fStretchWidth, 0);
                            g.DrawLine(pen, nX, 0, nX, m_lbDraw.Height - 1);
                        }
                        #endregion 세로라인들...
                        pen.Dispose();

                        DrawMark(g, fStretchWidth, fStretchHeight);   // 마커(점) — 선·격자 위에 그린다
                    } // using

                    if (m_abmpDisp[m_nBmpIndex] != null) m_abmpDisp[m_nBmpIndex].Dispose();
                    int nBitmapIndex = (m_nBmpIndex == 0) ? 1 : 0;
                    m_abmpDisp[nBitmapIndex] = (Bitmap)m_bmpBack.Clone();
                    m_nBmpIndex = nBitmapIndex;
                }
            }
        }
    }
}
