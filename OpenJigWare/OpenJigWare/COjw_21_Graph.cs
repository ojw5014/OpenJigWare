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
            public int GetCount_Model() { return m_nModelCount; }
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
