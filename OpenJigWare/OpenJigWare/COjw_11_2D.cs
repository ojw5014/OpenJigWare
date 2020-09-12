using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace OpenJigWare
{
    partial class Ojw
    {
        // Add the [Name Space] used by passing the [Canvas Handle].(Kor: Name Space를 추가후 Canvas 의 핸들을 넘겨서 사용하도록 한다.)
        // An error occurs if you do not run a function after you create.(Kor: 생성 후 Create 함수를 하지 않으면 에러...)
        public class C2D
        {
            // memory image -> double buffering(Kor: 메모리 이미지 => 더블 버퍼링 기법)
            private Bitmap m_bmpBackground;

            private double m_dAngleX;
            private double m_dAngleY;
            private double m_dAngleZ;

            private double m_dScale;    // 1.0 이 100%

            private double m_dX = 0, m_dY = 0, m_dZ = 0;

            private bool m_bIsValid;

            private PictureBox m_picDC;
            private Graphics m_gr;
            private bool m_bImage;
            //private Graphics m_gr_Image;
            private Color m_Color = Color.Blue;
            private Color m_BackColor;
            private float m_PenWidth;
            private Pen m_penDisp;

            public void SetAngleX(double AngleX) { m_dAngleX = AngleX; }
            public void SetAngleY(double AngleY) { m_dAngleY = AngleY; }
            public void SetAngleZ(double AngleZ) { m_dAngleZ = AngleZ; }
            public void SetScale(double Scale) { m_dScale = Scale; }

            public void SetColor(Color cData) { m_Color = cData; m_penDisp = new Pen(m_Color, m_PenWidth); }
            public void SetBackColor(Color cData) { m_BackColor = cData; }
            public void SetPenWidth(float fWidth) { m_PenWidth = fWidth; m_penDisp = new Pen(m_Color, m_PenWidth); }

            public double GetAngleX() { return m_dAngleX; }
            public double GetAngleY() { return m_dAngleY; }
            public double GetAngleZ() { return m_dAngleZ; }
            public double GetScale(double Scale) { return m_dScale; }

            public void Flush()
            {
                try
                {
                    Graphics m_gr2 = m_picDC.CreateGraphics();
                    m_gr2.DrawImage(m_bmpBackground, 0, 0);
                    m_gr2.Dispose();
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }

            public C2D()
            {
                m_bIsValid = false;
                m_dAngleX = 15;
                m_dAngleY = -30;
                m_dAngleZ = 0;

                m_dScale = 1.0; // 100%

                m_dX = 0; m_dY = 0; m_dZ = 0;

                m_Color = Color.Blue;
                m_BackColor = Color.Black;
                m_PenWidth = 1.0f;
                m_penDisp = new Pen(m_Color, m_PenWidth);
            }

            ~C2D()
            {
                Destroy();
            }

            public void Create(PictureBox picDC)
            {
                try
                {
                    m_bIsValid = true;
                    m_picDC = picDC;
                    //m_gr = picDC.CreateGraphics();

                    m_bmpBackground = new Bitmap(m_picDC.Width, m_picDC.Height);
                    m_gr = Graphics.FromImage(m_bmpBackground);

                    m_bIsValid = true;
                }
                catch
                {
                    m_bIsValid = false;
                }
            }
            private Bitmap m_bmpFile;
            public void Load(Bitmap bmp)
            {
                m_bImage = true;
                m_bmpFile = bmp;
                //m_gr_Image = Graphics.FromImage(bmp);
            }
            public void Load(Bitmap bmp, float fScale_X, float fScale_Y)
            {
                if (fScale_X == 0) fScale_X = 1.0f;
                if (fScale_Y == 0) fScale_Y = 1.0f;
                m_bImage = true;
                bmp.SetResolution((float)bmp.Width * (1.0f / fScale_X), (float)bmp.Height * (1.0f / fScale_Y));
                m_bmpFile = bmp;
                //m_gr_Image = Graphics.FromImage(bmp);
            }
            public void Load(String strImageFile)
            {
                m_bImage = true;
                //m_gr_Image = Graphics.FromImage(new Bitmap(strImageFile));//(Image.FromFile(strImageFile));
                Bitmap bmp = new Bitmap(strImageFile);
                m_bmpFile = bmp;
            }
            public void Load(String strImageFile, float fScale_X, float fScale_Y)
            {
                if (fScale_X == 0) fScale_X = 1.0f;
                if (fScale_Y == 0) fScale_Y = 1.0f;
                m_bImage = true; // 나중엔 에러처리까지 하도록... 꼭. 잊어버리지 말고!!! ojw5014
                Bitmap bmp = new Bitmap(strImageFile); //Image.FromFile(strImageFile);
                //bmp.
                bmp.SetResolution((float)bmp.Width * (1.0f / fScale_X), (float)bmp.Height * (1.0f / fScale_Y));
                //m_gr_Image = Graphics.FromImage(bmp);
                m_bmpFile = bmp;
            }
            
            public bool IsImage() { return m_bImage; }

            public bool IsValid() { return m_bIsValid; }

            public void Destroy()
            {
                if (m_bIsValid == true) m_gr.Dispose();
            }

            public void Rotation(double ax, double ay, double az, ref double x, ref double y, ref double z)
            {
                ax = ax * 3.14159 / 180;
                ay = ay * 3.14159 / 180;
                az = az * 3.14159 / 180;

                //    →X(left), ↑Y(up), ●Z(front)
                // rotation(Z)(Roll)
                /*
                 cos, -sin, 0, 0
                 sin,  cos, 0, 0
                   0,    0, 1, 0
                   0,    0, 0, 1
                 */

                // rotation(X)(Pitch)
                /*
                 1,   0,    0, 0
                 0, cos, -sin, 0
                 0, sin,  cos, 0
                 0,   0,    0, 1
                 */

                // rotation(Y)(Yaw)
                /*
                 cos, 0, -sin, 0
                   0, 1,    0, 0
                 sin, 0,  cos, 0
                   0, 0,    0, 1
                 */

                double x1, y1, z1, x2, y2, z2;

                x1 = x * Math.Cos(ay) + z * Math.Sin(ay);   // rotation(y)
                y1 = y;
                z1 = -x * Math.Sin(ay) + z * Math.Cos(ay);

                x2 = x1;    // rotation(x)
                y2 = y1 * Math.Cos(ax) - z1 * Math.Sin(ax);
                z2 = y1 * Math.Sin(ax) + z1 * Math.Cos(ax);

                x = x2 * Math.Cos(az) - y2 * Math.Sin(az);    // rotation(z)
                y = x2 * Math.Sin(az) + y2 * Math.Cos(az);
                z = z2;
            }

            public void Rotation(double ax, double ay, double az, double x, double y, double z, out int nX, out int nY)
            {
                double x1, y1, z1, x2, y2;

                ax = ax * 3.14159 / 180;
                ay = ay * 3.14159 / 180;
                az = az * 3.14159 / 180;

                x1 = x * Math.Cos(ay) + z * Math.Sin(ay);   // rotation(y)
                y1 = y;
                z1 = -x * Math.Sin(ay) + z * Math.Cos(ay);

                x2 = x1;    // rotation(x)
                y2 = y1 + Math.Cos(ax) - z1 * Math.Sin(ax);

                nX = m_picDC.Width / 2 + (int)Math.Round((x2 * Math.Cos(az) - y2 * Math.Sin(az)) * m_dScale);   // rotation(z)
                nY = m_picDC.Height / 2 - (int)Math.Round((x2 * Math.Sin(az) + y2 * Math.Cos(az)) * m_dScale);
            }

            public void Clear()
            {
                try
                {
                    if (IsImage() == true)
                        m_gr.DrawImage(m_bmpFile, 0, 0);
                    else
                        m_gr.Clear(m_BackColor);
                    //SolidBrush brush = new SolidBrush(m_BackColor);
                    //m_gr.FillRectangle(brush, 0, 0, m_picDC.Width, m_picDC.Width);
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }

            public void Line(double x1, double y1, double z1, double x2, double y2, double z2)
            {
                try
                {
                    int nX1, nY1;
                    int nX2, nY2;

                    Rotation(m_dAngleX, m_dAngleY, m_dAngleZ, x1, y1, z1, out nX1, out nY1);
                    Rotation(m_dAngleX, m_dAngleY, m_dAngleZ, x2, y2, z2, out nX2, out nY2);

                    m_dX = x2; m_dY = y2; m_dZ = z2;

                    m_gr.DrawLine(m_penDisp, nX1, nY1, nX2, nY2);
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }
            public void LineC(Color cColor, double x1, double y1, double z1, double x2, double y2, double z2)
            {
                try
                {
                    int nX1, nY1;
                    int nX2, nY2;

                    Rotation(m_dAngleX, m_dAngleY, m_dAngleZ, x1, y1, z1, out nX1, out nY1);
                    Rotation(m_dAngleX, m_dAngleY, m_dAngleZ, x2, y2, z2, out nX2, out nY2);

                    m_dX = x2; m_dY = y2; m_dZ = z2;
                    m_gr.DrawLine(new Pen(cColor, m_PenWidth), nX1, nY1, nX2, nY2);
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }

            public void LineConti(double x, double y, double z)
            {
                try
                {
                    int nX1, nY1;
                    int nX2, nY2;

                    Rotation(m_dAngleX, m_dAngleY, m_dAngleZ, m_dX, m_dY, m_dZ, out nX1, out nY1);
                    Rotation(m_dAngleX, m_dAngleY, m_dAngleZ, x, y, z, out nX2, out nY2);

                    m_dX = x; m_dY = y; m_dZ = z;

                    m_gr.DrawLine(m_penDisp, nX1, nY1, nX2, nY2);
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }

            public void Polygon(Color cColor, double[] pX, double[] pY, double[] pZ)
            {
                try
                {
                    int nCnt = pX.Length;
                    Point[] ppnt = new Point[nCnt];
                    int nX, nY;
                    int i = 0;
                    for (i = 0; i < nCnt; i++)
                    {
                        Rotation(m_dAngleX, m_dAngleY, m_dAngleZ, pX[i], pY[i], pZ[i], out nX, out nY); ppnt[i].X = nX; ppnt[i].Y = nY;
                    }
                    i = nCnt - 1;
                    m_dX = pX[i];
                    m_dY = pY[i];
                    m_dZ = pZ[i];
#if false
                i = 0;
                int nLeft = ppnt[i].X;
                int nRight = ppnt[i].X;
                int nTop = ppnt[i].Y;
                int nBottom = ppnt[i].Y;
                for (i = 0; i < nCnt; i++)
                {
                    if (ppnt[i].X < nLeft) nLeft = ppnt[i].X;
                    if (ppnt[i].X > nRight) nRight = ppnt[i].X;
                    if (ppnt[i].Y < nTop) nTop = ppnt[i].Y;
                    if (ppnt[i].Y > nBottom) nBottom = ppnt[i].Y;
                }
                int nWidth = nRight - nLeft;
                if (nWidth <= 0) nWidth = 1;
                int nHeight = nBottom - nTop;
                if (nHeight <= 0) nHeight = 1;
                LinearGradientBrush Lbrush = null;
                Rectangle _rectangle = new Rectangle(nLeft, nTop, nWidth, nHeight);
                Color cSecond = Color.FromArgb(m_penDisp.Color.R, m_penDisp.Color.B, m_penDisp.Color.G);
                Lbrush = new LinearGradientBrush(_rectangle, m_penDisp.Color, cSecond, LinearGradientMode.Vertical);
                m_gr.FillPolygon(Lbrush, ppnt);
#else
                    SolidBrush br = new SolidBrush(cColor);
                    m_gr.FillPolygon(br, ppnt);
#endif
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }
            public void Polygon(Color cColor, Color cSecondColor, double[] pX, double[] pY, double[] pZ)
            {
                try
                {
                    int nCnt = pX.Length;
                    Point[] ppnt = new Point[nCnt];
                    int nX, nY;
                    int i = 0;
                    for (i = 0; i < nCnt; i++)
                    {
                        Rotation(m_dAngleX, m_dAngleY, m_dAngleZ, pX[i], pY[i], pZ[i], out nX, out nY); ppnt[i].X = nX; ppnt[i].Y = nY;
                    }
                    i = nCnt - 1;
                    m_dX = pX[i];
                    m_dY = pY[i];
                    m_dZ = pZ[i];
#if true
                i = 0;
                int nLeft = ppnt[i].X;
                int nRight = ppnt[i].X;
                int nTop = ppnt[i].Y;
                int nBottom = ppnt[i].Y;
                for (i = 0; i < nCnt; i++)
                {
                    if (ppnt[i].X < nLeft) nLeft = ppnt[i].X;
                    if (ppnt[i].X > nRight) nRight = ppnt[i].X;
                    if (ppnt[i].Y < nTop) nTop = ppnt[i].Y;
                    if (ppnt[i].Y > nBottom) nBottom = ppnt[i].Y;
                }
                int nWidth = nRight - nLeft;
                if (nWidth <= 0) nWidth = 1;
                int nHeight = nBottom - nTop;
                if (nHeight <= 0) nHeight = 1;
                LinearGradientBrush Lbrush = null;
                Rectangle _rectangle = new Rectangle(nLeft, nTop, nWidth, nHeight);
                Color cSecond = Color.FromArgb(cSecondColor.R, cSecondColor.G, cSecondColor.B);
                Lbrush = new LinearGradientBrush(_rectangle, cColor, cSecond, LinearGradientMode.Vertical);
                m_gr.FillPolygon(Lbrush, ppnt);
#else
                    SolidBrush br = new SolidBrush(m_penDisp.Color);
                    m_gr.FillPolygon(br, ppnt);
#endif
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }

            public void CenterBox(double nCx, double nCy, double nCz, double w, double h, double d)
            {
                //    Line(nCx - w/2, nCy - h/2, nCz - d/2, )
            }

            // Axis of rotation [ax, ay, az],    start position(Top position), [Cx,Cy,Cz],   Size [w,h,d]
            // Kor: 회전축 ax, ay, az,    시작위치(맨 위 정점), Cx,Cy,Cz,   크기 w,h,d
            public void TopCBox(Color cData, double ax, double ay, double az, double nCx, double nCy, double nCz, double w, double h, double d)
            {
                try
                {
                    Color cTmp = m_Color;
                    SetColor(cData);
                    TopBox(ax, ay, az, nCx, nCy, nCz, w, h, d);
                    SetColor(cTmp);
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }

            public void TextC(Color cData, string str, double dCx, double dCy, double dCz, bool bCenter)
            {
                try
                {
                    SolidBrush br = new SolidBrush(cData);
                    int nX, nY;
                    Rotation(m_dAngleX, m_dAngleY, m_dAngleZ, dCx, dCy, dCz, out nX, out nY);
                    if (bCenter)
                    {
                        ///////// Center ////////////////////
                        StringFormat sf = new StringFormat();
                        sf.LineAlignment = StringAlignment.Center;
                        sf.Alignment = StringAlignment.Center;
                        /////////////////////////////////////
                        m_gr.DrawString(str, new Font("Georgia", 9, System.Drawing.FontStyle.Regular), br, nX, nY, sf);
                        //m_gr.DrawString(str, new Font("굴림", 9, System.Drawing.FontStyle.Regular), br, nX, nY, sf);
                    }
                    else
                        m_gr.DrawString(str, new Font("Georgia", 9, System.Drawing.FontStyle.Regular), br, nX, nY);
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }
            public void TextC(Color cData, int nFontSize, string str, double dCx, double dCy, double dCz, bool bCenter)
            {
                try
                {
                    SolidBrush br = new SolidBrush(cData);
                    int nX, nY;
                    Rotation(m_dAngleX, m_dAngleY, m_dAngleZ, dCx, dCy, dCz, out nX, out nY);
                    if (bCenter)
                    {
                        ///////// Center ////////////////////
                        StringFormat sf = new StringFormat();
                        sf.LineAlignment = StringAlignment.Center;
                        sf.Alignment = StringAlignment.Center;
                        /////////////////////////////////////
                        m_gr.DrawString(str, new Font("Georgia", nFontSize, System.Drawing.FontStyle.Regular), br, nX, nY, sf);
                        //m_gr.DrawString(str, new Font("굴림", 9, System.Drawing.FontStyle.Regular), br, nX, nY, sf);
                    }
                    else
                        m_gr.DrawString(str, new Font("Georgia", nFontSize, System.Drawing.FontStyle.Regular), br, nX, nY);
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }
            public void TextC(Color cData, string strFontName, int nFontSize, string str, double dCx, double dCy, double dCz, bool bCenter)
            {
                try
                {
                    SolidBrush br = new SolidBrush(cData);
                    int nX, nY;
                    Rotation(m_dAngleX, m_dAngleY, m_dAngleZ, dCx, dCy, dCz, out nX, out nY);
                    if (bCenter)
                    {
                        ///////// Center ////////////////////
                        StringFormat sf = new StringFormat();
                        sf.LineAlignment = StringAlignment.Center;
                        sf.Alignment = StringAlignment.Center;
                        /////////////////////////////////////
                        m_gr.DrawString(str, new Font(strFontName, nFontSize, System.Drawing.FontStyle.Regular), br, nX, nY, sf);
                        //m_gr.DrawString(str, new Font("굴림", 9, System.Drawing.FontStyle.Regular), br, nX, nY, sf);
                    }
                    else
                        m_gr.DrawString(str, new Font(strFontName, nFontSize, System.Drawing.FontStyle.Regular), br, nX, nY);
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }
            public void TopBox(double ax, double ay, double az, double nCx, double nCy, double nCz, double w, double h, double d)
            {
                try
                {
                    int nCnt = 18;
                    double[] pdX = new double[nCnt];
                    double[] pdY = new double[nCnt];
                    double[] pdZ = new double[nCnt];
                    double dx1, dy1, dz1;
                    double dx2, dy2, dz2;

                    h = -h;

                    dx1 = -w / 2;
                    dy1 = 0;
                    dz1 = d / 2;

                    dx2 = w / 2;
                    dy2 = 0;
                    dz2 = d / 2;
                    Rotation(ax, ay, az, ref dx1, ref dy1, ref dz1);
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    int i = 0;
                    pdX[i] = (double)nCx + dx1;
                    pdY[i] = (double)nCy + dy1;
                    pdZ[i] = (double)nCz + dz1;
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////
                    dx2 = w / 2;
                    dy2 = 0;
                    dz2 = -d / 2;
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //LineConti(nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////


                    dx2 = -w / 2;
                    dy2 = 0;
                    dz2 = -d / 2;
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //LineConti(nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////

                    dx2 = -w / 2;
                    dy2 = 0;
                    dz2 = d / 2;
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //LineConti(nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////



                    //Polygon(m_Color, pdX, pdY, pdZ);




                    ///////////////////////////////
                    dx1 = -w / 2;
                    dy1 = h;
                    dz1 = d / 2;

                    dx2 = w / 2;
                    dy2 = h;
                    dz2 = d / 2;
                    Rotation(ax, ay, az, ref dx1, ref dy1, ref dz1);
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    i++;
                    pdX[i] = (double)nCx + dx1;
                    pdY[i] = (double)nCy + dy1;
                    pdZ[i] = (double)nCz + dz1;
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////

                    dx2 = w / 2;
                    dy2 = h;
                    dz2 = -d / 2;
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //LineConti(nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////

                    dx2 = -w / 2;
                    dy2 = h;
                    dz2 = -d / 2;
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //LineConti(nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////

                    dx2 = -w / 2;
                    dy2 = h;
                    dz2 = d / 2;
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //LineConti(nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////

                    //Polygon(m_Color, pdX, pdY, pdZ);

                    /////////////////////////////////////

                    dx1 = -w / 2;
                    dy1 = 0;
                    dz1 = -d / 2;

                    dx2 = -w / 2;
                    dy2 = h;
                    dz2 = -d / 2;
                    Rotation(ax, ay, az, ref dx1, ref dy1, ref dz1);
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    i++;
                    pdX[i] = (double)nCx + dx1;
                    pdY[i] = (double)nCy + dy1;
                    pdZ[i] = (double)nCz + dz1;
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////


                    dx1 = -w / 2;
                    dy1 = 0;
                    dz1 = d / 2;

                    dx2 = -w / 2;
                    dy2 = h;
                    dz2 = d / 2;
                    Rotation(ax, ay, az, ref dx1, ref dy1, ref dz1);
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    i++;
                    pdX[i] = (double)nCx + dx1;
                    pdY[i] = (double)nCy + dy1;
                    pdZ[i] = (double)nCz + dz1;
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////


                    dx1 = w / 2;
                    dy1 = 0;
                    dz1 = -d / 2;

                    dx2 = w / 2;
                    dy2 = h;
                    dz2 = -d / 2;
                    Rotation(ax, ay, az, ref dx1, ref dy1, ref dz1);
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    i++;
                    pdX[i] = (double)nCx + dx1;
                    pdY[i] = (double)nCy + dy1;
                    pdZ[i] = (double)nCz + dz1;
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////


                    dx1 = w / 2;
                    dy1 = 0;
                    dz1 = d / 2;

                    dx2 = w / 2;
                    dy2 = h;
                    dz2 = d / 2;
                    Rotation(ax, ay, az, ref dx1, ref dy1, ref dz1);
                    Rotation(ax, ay, az, ref dx2, ref dy2, ref dz2);
                    //Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);
                    //
                    i++;
                    pdX[i] = (double)nCx + dx1;
                    pdY[i] = (double)nCy + dy1;
                    pdZ[i] = (double)nCz + dz1;
                    //
                    i++;
                    pdX[i] = (double)nCx + dx2;
                    pdY[i] = (double)nCy + dy2;
                    pdZ[i] = (double)nCz + dz2;
                    /////////////////////////////////////////////////
                    //Polygon(m_Color, pdX, pdY, pdZ);

                    //for (i = 1; i < nCnt; i++)
                    //{
                    //    Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //}
                    //i = 1; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //i = 2; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //i = 3; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //i = 4; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //i = 5; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);

                    //i = 6; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //i = 7; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //i = 8; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //i = 9; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //i = 10; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //i = 11; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //i = 12; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);

                    //i = 13; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    //i = 14; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);

                    //i = 15; Line(pdX[i - 1], pdY[i - 1], pdZ[i - 1], pdX[i], pdY[i], pdZ[i]);
                    int nCnt2 = 4;
                    int[] pPnt = new int[nCnt2];
                    double[] pdX2 = new double[nCnt2];
                    double[] pdY2 = new double[nCnt2];
                    double[] pdZ2 = new double[nCnt2];
                    int nPnt;
                    #region Back
                    i = 0;
                    pPnt[i++] = 3;
                    pPnt[i++] = 8;
                    pPnt[i++] = 7;
                    pPnt[i++] = 2;
                    for (i = 0; i < nCnt2; i++)
                    {
                        pdX2[i] = pdX[pPnt[i]];
                        pdY2[i] = pdY[pPnt[i]];
                        pdZ2[i] = pdZ[pPnt[i]];

                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);

                        //LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    Polygon(m_Color, pdX2, pdY2, pdZ2);
                    for (i = 0; i < nCnt2; i++)
                    {
                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);
                        LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    #endregion Back
                    #region Right
                    i = 0;
                    pPnt[i++] = 6;
                    pPnt[i++] = 7;
                    pPnt[i++] = 2;
                    pPnt[i++] = 1;
                    for (i = 0; i < nCnt2; i++)
                    {
                        pdX2[i] = pdX[pPnt[i]];
                        pdY2[i] = pdY[pPnt[i]];
                        pdZ2[i] = pdZ[pPnt[i]];

                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);

                        //LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    Polygon(m_Color, pdX2, pdY2, pdZ2);
                    for (i = 0; i < nCnt2; i++)
                    {
                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);
                        LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    #endregion Right
                    #region Bottom
                    i = 0;
                    pPnt[i++] = 5;
                    pPnt[i++] = 6;
                    pPnt[i++] = 7;
                    pPnt[i++] = 8;
                    for (i = 0; i < nCnt2; i++)
                    {
                        pdX2[i] = pdX[pPnt[i]];
                        pdY2[i] = pdY[pPnt[i]];
                        pdZ2[i] = pdZ[pPnt[i]];

                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);

                        //LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    Polygon(m_Color, pdX2, pdY2, pdZ2);
                    for (i = 0; i < nCnt2; i++)
                    {
                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);
                        LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    #endregion Bottom
                    #region Top
                    i = 0;
                    pPnt[i++] = 0;
                    pPnt[i++] = 1;
                    pPnt[i++] = 2;
                    pPnt[i++] = 3;
                    for (i = 0; i < nCnt2; i++)
                    {
                        pdX2[i] = pdX[pPnt[i]];
                        pdY2[i] = pdY[pPnt[i]];
                        pdZ2[i] = pdZ[pPnt[i]];

                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);

                        //LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    Polygon(m_Color, pdX2, pdY2, pdZ2);
                    for (i = 0; i < nCnt2; i++)
                    {
                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);
                        LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    #endregion Top
                    #region Left
                    i = 0;
                    pPnt[i++] = 5;
                    pPnt[i++] = 8;
                    pPnt[i++] = 3;
                    pPnt[i++] = 0;
                    for (i = 0; i < nCnt2; i++)
                    {
                        pdX2[i] = pdX[pPnt[i]];
                        pdY2[i] = pdY[pPnt[i]];
                        pdZ2[i] = pdZ[pPnt[i]];

                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);

                        //LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    Polygon(m_Color, pdX2, pdY2, pdZ2);
                    for (i = 0; i < nCnt2; i++)
                    {
                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);
                        LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    #endregion Left
                    #region Front
                    i = 0;
                    pPnt[i++] = 5;
                    pPnt[i++] = 6;
                    pPnt[i++] = 1;
                    pPnt[i++] = 0;
                    for (i = 0; i < nCnt2; i++)
                    {
                        pdX2[i] = pdX[pPnt[i]];
                        pdY2[i] = pdY[pPnt[i]];
                        pdZ2[i] = pdZ[pPnt[i]];

                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);
                        //LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    Polygon(m_Color, pdX2, pdY2, pdZ2);
                    for (i = 0; i < nCnt2; i++)
                    {
                        nPnt = ((i == 0) ? nCnt2 - 1 : i - 1);
                        LineC(Color.Black, pdX2[nPnt], pdY2[nPnt], pdZ2[nPnt], pdX2[i], pdY2[i], pdZ2[i]);
                    }
                    #endregion Front
                    //for (i = 0; i < 5; i++)
                    //{
                    //    pdX2[i] = pdX[i + nPos];
                    //    pdY2[i] = pdY[i + nPos];
                    //    pdZ2[i] = pdZ[i + nPos];
                    //}
                    //Polygon(m_Color, pdX2, pdY2, pdZ2);
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }
            // Axis of rotation [ax, ay, az],    start position(Top position), [Cx,Cy,Cz],   Size [w,h,d]
            // Kor: 회전축 ax, ay, az,    시작위치(맨 위 정점), Cx,Cy,Cz,   크기 w,h,d
            public void TopCBox_Seq(Color cData, double ax1, double ay1, double az1, double ax2, double ay2, double az2, double nCx, double nCy, double nCz, double w, double h, double d)
            {
                try
                {
                    Color cTmp = m_Color;
                    SetColor(cData);
                    TopBox_Seq(ax1, ay1, az1, ax2, ay2, az2, nCx, nCy, nCz, w, h, d);
                    SetColor(cTmp);
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }

            public void TopBox_Seq(double ax1, double ay1, double az1, double ax2, double ay2, double az2, double nCx, double nCy, double nCz, double w, double h, double d)
            {
                try
                {
                    double dx1, dy1, dz1;
                    double dx2, dy2, dz2;

                    h = -h;

                    dx1 = -w / 2;
                    dy1 = 0;
                    dz1 = d / 2;

                    dx2 = w / 2;
                    dy2 = 0;
                    dz2 = d / 2;
                    Rotation(ax1, ay1, az1, ref dx1, ref dy1, ref dz1);
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);

                    Rotation(ax2, ay2, az2, ref dx1, ref dy1, ref dz1);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);

                    Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);

                    dx2 = w / 2;
                    dy2 = 0;
                    dz2 = -d / 2;
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);
                    LineConti(nCx + dx2, nCy + dy2, nCz + dz2);


                    dx2 = -w / 2;
                    dy2 = 0;
                    dz2 = -d / 2;
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);
                    LineConti(nCx + dx2, nCy + dy2, nCz + dz2);

                    dx2 = -w / 2;
                    dy2 = 0;
                    dz2 = d / 2;
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);
                    LineConti(nCx + dx2, nCy + dy2, nCz + dz2);

                    ///////////////////////////////
                    dx1 = -w / 2;
                    dy1 = h;
                    dz1 = d / 2;

                    dx2 = w / 2;
                    dy2 = h;
                    dz2 = d / 2;
                    Rotation(ax1, ay1, az1, ref dx1, ref dy1, ref dz1);
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);

                    Rotation(ax2, ay2, az2, ref dx1, ref dy1, ref dz1);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);
                    Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);

                    dx2 = w / 2;
                    dy2 = h;
                    dz2 = -d / 2;
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);
                    LineConti(nCx + dx2, nCy + dy2, nCz + dz2);

                    dx2 = -w / 2;
                    dy2 = h;
                    dz2 = -d / 2;
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);
                    LineConti(nCx + dx2, nCy + dy2, nCz + dz2);

                    dx2 = -w / 2;
                    dy2 = h;
                    dz2 = d / 2;
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);
                    LineConti(nCx + dx2, nCy + dy2, nCz + dz2);
                    
                    /////////////////////////////////////

                    dx1 = -w / 2;
                    dy1 = 0;
                    dz1 = -d / 2;

                    dx2 = -w / 2;
                    dy2 = h;
                    dz2 = -d / 2;
                    Rotation(ax1, ay1, az1, ref dx1, ref dy1, ref dz1);
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);

                    Rotation(ax2, ay2, az2, ref dx1, ref dy1, ref dz1);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);
                    Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);
                    
                    dx1 = -w / 2;
                    dy1 = 0;
                    dz1 = d / 2;

                    dx2 = -w / 2;
                    dy2 = h;
                    dz2 = d / 2;
                    Rotation(ax1, ay1, az1, ref dx1, ref dy1, ref dz1);
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);

                    Rotation(ax2, ay2, az2, ref dx1, ref dy1, ref dz1);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);
                    Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);


                    dx1 = w / 2;
                    dy1 = 0;
                    dz1 = -d / 2;

                    dx2 = w / 2;
                    dy2 = h;
                    dz2 = -d / 2;
                    Rotation(ax1, ay1, az1, ref dx1, ref dy1, ref dz1);
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);

                    Rotation(ax2, ay2, az2, ref dx1, ref dy1, ref dz1);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);
                    Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);


                    dx1 = w / 2;
                    dy1 = 0;
                    dz1 = d / 2;

                    dx2 = w / 2;
                    dy2 = h;
                    dz2 = d / 2;
                    Rotation(ax1, ay1, az1, ref dx1, ref dy1, ref dz1);
                    Rotation(ax1, ay1, az1, ref dx2, ref dy2, ref dz2);

                    Rotation(ax2, ay2, az2, ref dx1, ref dy1, ref dz1);
                    Rotation(ax2, ay2, az2, ref dx2, ref dy2, ref dz2);
                    Line(nCx + dx1, nCy + dy1, nCz + dz1, nCx + dx2, nCy + dy2, nCz + dz2);
                }
                catch (Exception e)
                {
                    Ojw.CMessage.Write(e.ToString());
                }
            }

            /////////////
            #region Capture
            public static Bitmap ScreenCapture(Rectangle rect)
            {
                //// 픽셀 포맷 정보 얻기 (Optional)
                //int bitsPerPixel = Screen.PrimaryScreen.BitsPerPixel;
                //PixelFormat pixelFormat = PixelFormat.Format32bppArgb;
                //if (bitsPerPixel <= 16)
                //{
                //    pixelFormat = PixelFormat.Format16bppRgb565;
                //}
                //if (bitsPerPixel == 24)
                //{
                //    pixelFormat = PixelFormat.Format24bppRgb;
                //}

#if true
                Bitmap bmp = new Bitmap(rect.Width, rect.Height);//, pixelFormat);
                Graphics gDc = Graphics.FromImage(bmp);
                //gDc.CopyFromScreen(new Point(rect.X, rect.Y), new Point(0, 0), rect.Size);



                gDc.CopyFromScreen(new Point(rect.X, rect.Y), new Point(0, 0), rect.Size, CopyPixelOperation.SourceCopy);//CopyPixelOperation.SourcePaint);
                gDc.Dispose(); gDc = null;
                return bmp;
#else

                using (Bitmap bmp = new Bitmap(rect.Width, rect.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(new Point(rect.Left, rect.Top), Point.Empty, rect.Size);
                    }
                    return bmp;
                }
#endif


            }
            public static Bitmap ScreenCapture_Full() { return ScreenCapture_Full(0); }
            public static Bitmap ScreenCapture_Full(int nIndex) { return ScreenCapture(Screen.AllScreens[0].WorkingArea); }
            #endregion Capture
        }
    }
}
