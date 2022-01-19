#if true
//#define _TEST
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;

namespace OpenJigWare
{
    partial class Ojw
    {
        // Do not use this yet.

        // if you make your class, just write in here
        #region Grid
#if true
        public class CGrid : Form
        {
            public enum EType_t
            {
                _GRID_NORMAL = 0,
                _GRID_BUTTON,
                _GRID_IMAGE,
                _GRID_PROGRESSBAR,
                _GRID_CHECKBOX,
                _GRID_COMBOBOX,
                _COUNT
            }
            private int m_nMaxLine = 1000;
            private DataGridView m_gvGrid = null;//new DataGridView();
            public void Create(Control ctrlHandle, int nX, int nY, int nWidth, int nHeight, String[] pstrTitles)
            {
                Create(ctrlHandle, nX, nY, nWidth, nHeight, pstrTitles, null, null);
            }
            public void SetMax(int nMaxLine)
            {
                m_nMaxLine = nMaxLine;
            }
            public int GetMax() { return m_nMaxLine; }
            public DataGridView GetHandle() { return m_gvGrid; }
            public int [] m_pnType = null;
            public void Create(Control ctrlHandle, int nX, int nY, int nWidth, int nHeight, String[] pstrTitles, int[] pnWidth, int[] pnType)
            {
                #region Check Exception
                if (pnType == null)
                {
                    pnType = new int[pstrTitles.Length];
                    for (int i = 0; i < pnType.Length; i++) { pnType[i] = (int)EType_t._GRID_NORMAL; }
                }
                if (pnWidth == null)
                {
                    pnWidth = new int[pstrTitles.Length];
                    for (int i = 0; i < pnWidth.Length; i++) { pnWidth[i] = (nWidth - 60) / pnWidth.Length; }
                    //pnWidth = new int[pstrTitles.Length];
                    //for (int i = 0; i < pnWidth.Length; i++) { pnWidth[i] = 80; }
                }
                if (pstrTitles == null)
                {
                    CMessage.Write_Error("No titles in here");
                    return;
                }
                if (pstrTitles.Length != pnWidth.Length)
                {
                    CMessage.Write_Error("mismatch Grid Size");
                    return;
                }
                if (m_gvGrid == null) m_gvGrid = new DataGridView();
                else
                {
                    m_gvGrid.Rows.Clear();
                    m_gvGrid.Columns.Clear();
                }
                #endregion Check Exception
                m_gvGrid.Width = nWidth;
                m_gvGrid.Height = nHeight;

                m_gvGrid.Top = nY;
                m_gvGrid.Left = nX;
                m_pnType = pnType;
                for (int nPos = 0; nPos < pstrTitles.Length; nPos++)
                {
                    if (pnType[nPos] == (int)EType_t._GRID_BUTTON)
                    {
                        //DataGridViewButtonColumn clmBtnTmp = new DataGridViewButtonColumn();
                        COjwCellButton clmBtnTmp = new COjwCellButton();
                        clmBtnTmp.HeaderText = pstrTitles[nPos];
                        clmBtnTmp.SetFont = m_gvGrid.Font; // DisConnect 란 글자가 나타나면 나타나는 글자의 Font 를 결정한다.
                        clmBtnTmp.SetName = "DisConnect"; // DisConnect 란 글자가 나타나면 버튼의 색이 변하도록 한다.
                        clmBtnTmp.SetFontColor = Color.Yellow; // DisConnect 란 글자가 나타나면 변하는 버튼의 글자색을 정한다.
                        clmBtnTmp.SetButtonColor = Color.Blue; // DisConnect 란 글자가 나타나면 변하는 버튼의 바탕색을 정한다.
                        m_gvGrid.Columns.Add(clmBtnTmp);
                    }
                    else if (pnType[nPos] == (int)EType_t._GRID_CHECKBOX)
                    {
                        DataGridViewCheckBoxColumn comboCheckColumn = new DataGridViewCheckBoxColumn();
                        comboCheckColumn.HeaderText = pstrTitles[nPos];
                        m_gvGrid.Columns.Add(comboCheckColumn);
                    }
                    else if (pnType[nPos] == (int)EType_t._GRID_COMBOBOX)
                    {
                        DataGridViewComboBoxColumn clmComboTmp = new DataGridViewComboBoxColumn();
                        clmComboTmp.HeaderText = pstrTitles[nPos];
                        clmComboTmp.Name = "combo";
                        clmComboTmp.Items.AddRange("Test1", "Test2", "Test3", "Test4", "Test5");
                        m_gvGrid.Columns.Add(clmComboTmp);
                    }
                    else if (pnType[nPos] == (int)EType_t._GRID_PROGRESSBAR)// && (g_nControlMode != (int)EMode._MODE_BLUETOOTH))
                    {
                        //DataGridViewProgressColumn cImImgTmp = new DataGridViewProgressColumn();
                        COjwProgressBarColumn cImImgTmp = new COjwProgressBarColumn();
                        //cImImgTmp.HeaderText = pstrTitles[nPos];
                        //cImImgTmp.DefaultCellStyle.ForeColor = Color.Red; // 글자색 변경 가능(Default = 검은색)
                        cImImgTmp.LowValue = 30; // 30% 이하 값에서는 적색으로 되도록 한다.
                        cImImgTmp.LowColor = Color.Red;
                        m_gvGrid.Columns.Add(cImImgTmp);
                    }
                    else if (pnType[nPos] == (int)EType_t._GRID_IMAGE)
                    {
                        DataGridViewImageColumn cImImgTmp = new DataGridViewImageColumn();
                        cImImgTmp.ImageLayout = DataGridViewImageCellLayout.Stretch;
                        //cImImgTmp.Image = imageList1.Images[0];
                        //cImImgTmp.Image = m_aImage[0];// global::DanceManager.Properties.Resources.Pos_1;
                        m_gvGrid.Columns.Add(cImImgTmp);
                    }
                    else
                    {
                        m_gvGrid.Columns.Add(pstrTitles[nPos], pstrTitles[nPos]);
                    }
                    m_gvGrid.Columns[nPos].Width = pnWidth[nPos];
                    m_gvGrid.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                m_gvGrid.RowPostPaint += new DataGridViewRowPostPaintEventHandler(Grid_RowPostPaint);

                ctrlHandle.Controls.Add(m_gvGrid);
            }
            //public int m_nDisplayFirstLine = 0;
            private void Grid_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
            {
                //if (m_bStart == true) return;
                // RowPointPaint 이벤트핸들러
                // 행헤더 열영역에 행번호를 보여주기 위해 장방형으로 처리

                Rectangle rect = new Rectangle(e.RowBounds.Location.X,
                e.RowBounds.Location.Y,
                30,//m_gvGrid.RowHeadersWidth - 4,
                e.RowBounds.Height + 4);
                // 위에서 생성된 장방형내에 행번호를 보여주고 폰트색상 및 배경을 설정
                TextRenderer.DrawText(e.Graphics,
                    (e.RowIndex + 1).ToString() + ".",
                    m_gvGrid.RowHeadersDefaultCellStyle.Font,
                    rect,
                    m_gvGrid.RowHeadersDefaultCellStyle.ForeColor, //Color.Red, //m_gvGrid.RowHeadersDefaultCellStyle.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

                //if ((m_gvGrid != null) && (Grid_GetCaption(e.RowIndex) != ""))
                //{
                //    string strValue = Grid_GetCaption(e.RowIndex);
                //    rect.Inflate(70, 0);
                //    if (strValue != "")
                //    {
                //        TextRenderer.DrawText(e.Graphics,
                //             "        " + "                " + strValue,
                //            m_gvGrid.RowHeadersDefaultCellStyle.Font,
                //            rect,
                //            m_gvGrid.RowHeadersDefaultCellStyle.ForeColor,
                //            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                //    }
                //    /*
                //     * <DataGridViewRowPostPaintEventArgs 객체>
                //     * e.Graphics - Graphics객체
                //     * e.RowIndex - 표시중인 행번호 (0부터 시작하기 떄문에 +1필요) 
                //     * e.RowBounds.X 행헤더 열 왼쪽 위 X좌표
                //     * e.RowBounds.Y 행헤더 열 왼쪽 위 Y좌표
                //     * e.RowBounds.Height 행헤더 열높이
                //     * dbView.RowHeadersWidth 행헤더 셀 폭
                //     * dbView.RowHeadersDefaultCellStyle.Font 사용폰트
                //     * dbView.RowHeadersDefaultCellStyle.FontColor 폰트 색상
                //     */
                //}
#if _COLOR_GRID_IN_PAINT
            // Grid 그리기
            if (m_bStart == false) Grid_SetColorGrid(e.RowIndex);            
#endif
            }
            public object Grid_Get(int nX, int nY) 
            {

                //var vRet;
                //if (m_pnType[nX] == EType_t._GRID_CHECKBOX) vRet = false;
                return (m_gvGrid == null) ? 0 : (((m_gvGrid.RowCount <= nY) || (m_gvGrid.ColumnCount <= nX)) ? 0 : ((m_gvGrid.Rows[nY].Cells[nX].Value == null) ? 0 : m_gvGrid.Rows[nY].Cells[nX].Value)); 
            }
            public void Grid_Set(int nX, int nY, string strDataValue) 
            {
                if (m_pnType[nX] == (int)EType_t._GRID_PROGRESSBAR)
                {
                    m_gvGrid.Rows[nY].Cells[nX].Value = Ojw.CConvert.StrToInt(strDataValue);
                //    m_gvGrid.Rows[nY].Cells[nX].Value = DataValue;
                //    ////COjwProgressBarCell CBar = new COjwProgressBarCell();
                //    //((COjwProgressBarCell)(m_gvGrid.Rows[nY].Cells[nX])).Value = (int)DataValue;
                }
                else if (m_pnType[nX] == (int)EType_t._GRID_CHECKBOX)
                {
                    m_gvGrid.Rows[nY].Cells[nX].Value = Ojw.CConvert.StrToBool(strDataValue);
                }
                else
                    m_gvGrid.Rows[nY].Cells[nX].Value = strDataValue; 
            }
            //public void Grid_Set(int nX, int nY, object DataValue) { m_gvGrid.Rows[nY].Cells[nX].Value = DataValue; }
            public int Grid_Size_Lines() { return (m_gvGrid == null) ? 0 : ((m_gvGrid.CurrentCell == null) ? 0 : m_gvGrid.RowCount); }
            public int Grid_Size_Columns() { return (m_gvGrid == null) ? 0 : ((m_gvGrid.CurrentCell == null) ? 0 : m_gvGrid.ColumnCount); }
            public int Grid_Get_Y() { return (m_gvGrid == null) ? 0 : ((m_gvGrid.CurrentCell == null) ? 0 : m_gvGrid.CurrentCell.RowIndex); }
            public int Grid_Get_X() { return (m_gvGrid == null) ? 0 : ((m_gvGrid.CurrentCell == null) ? 0 : m_gvGrid.CurrentCell.ColumnIndex); }
            public void Grid_Insert(int nIndex, int nInsertCnt)
            {
                int nErrorNum = 0;
                try
                {
                    if (nIndex < 0) nIndex = 0;

                    nErrorNum++;
                    int nFirst = nIndex;
                    nErrorNum++;
                    m_gvGrid.Rows.Insert(nIndex, nInsertCnt);
                    nErrorNum++;
                    //for (int i = nFirst; i < nFirst + nInsertCnt; i++) Grid_Clear(i);
                    nErrorNum++;
                    int nPos = (m_gvGrid.CurrentCell == null) ? 0 : m_gvGrid.CurrentCell.ColumnIndex;

                    if (m_gvGrid.RowCount > m_nMaxLine)
                    {
                        for (int i = 0; i < ((m_gvGrid.RowCount - 1) - m_nMaxLine); i++)
                            Grid_Delete(m_gvGrid.RowCount - 1, 1);
                    }
                }
                catch (System.Exception e)
                {
                    CMessage.Write_Error("[" + CConvert.IntToStr(nErrorNum) + "]" + e.ToString());
                }
            }

            public void Grid_Add(int nIndex, int nInsertCnt)
            {
                if (nIndex < 0) nIndex = 0;
                nIndex++;
                if (nIndex < m_gvGrid.RowCount - 1)
                {
                    Grid_Insert(nIndex, nInsertCnt);
                    Grid_ChangePos(m_gvGrid, nIndex, m_gvGrid.CurrentCell.ColumnIndex);
                    return;
                }
                int nFirst = nIndex;

                m_gvGrid.Rows.Add(nInsertCnt);
                //for (int i = nFirst; i < nFirst + nInsertCnt; i++) Grid_Clear(i);
                Grid_ChangePos(m_gvGrid, nIndex, m_gvGrid.CurrentCell.ColumnIndex);

                if (m_gvGrid.RowCount > m_nMaxLine)
                {
                    for (int i = 0; i < (m_gvGrid.RowCount - m_nMaxLine); i++)
                        Grid_Delete(m_gvGrid.RowCount - 1, 1);
                }
            }
            public void Grid_ChangePos(DataGridView OjwGrid, int nLine, int nPos)
            {
                if (OjwGrid.Rows[nLine].Cells[nPos].Selected == false) OjwGrid.CurrentCell = OjwGrid.Rows[nLine].Cells[nPos];
            }
            public void Grid_ChangePos(int nX, int nY)
            {
                int nLine = nY;
                int nPos = nX;
                if (m_gvGrid.Rows[nLine].Cells[nPos].Selected == false) m_gvGrid.CurrentCell = m_gvGrid.Rows[nLine].Cells[nPos];
            }
            public void Grid_Delete(int nIndex, int nDeleteCnt)
            {
                for (int i = 0; i < nDeleteCnt; i++)
                {
                    if (nIndex < 0) nIndex = 0;
                    m_gvGrid.Rows.RemoveAt(nIndex);
                }
            }
        }

        //////////////////////////////////////
        private class COjwProgressBarColumn : DataGridViewTextBoxColumn
        {
            // Constructer
            public COjwProgressBarColumn()
            {
                this.CellTemplate = new COjwProgressBarCell();
            }

            //Set CellTemplate 
            public override DataGridViewCell CellTemplate
            {
                get
                {
                    return base.CellTemplate;
                }
                set
                {

                    if (!(value is COjwProgressBarCell))
                    {
                        throw new InvalidCastException(
                            "Set COjwProgressBarCell object");
                    }
                    base.CellTemplate = value;
                }
            }

            /// <summary>
            /// ProgressBar Max
            /// </summary>
            public int Maximum
            {
                get
                {
                    return ((COjwProgressBarCell)this.CellTemplate).Maximum;
                }
                set
                {
                    if (this.Maximum == value)
                        return;

                    ((COjwProgressBarCell)this.CellTemplate).Maximum =
                        value;

                    if (this.DataGridView == null)
                        return;
                    int rowCount = this.DataGridView.RowCount;
                    for (int i = 0; i < rowCount; i++)
                    {
                        DataGridViewRow r = this.DataGridView.Rows.SharedRow(i);
                        ((COjwProgressBarCell)r.Cells[this.Index]).Maximum =
                            value;
                    }
                }
            }

            public int LowValue
            {
                get
                {
                    return ((COjwProgressBarCell)this.CellTemplate).LowValue;
                }
                set
                {
                    ((COjwProgressBarCell)this.CellTemplate).LowValue = value;
                }
            }

            public Color LowColor
            {
                get
                {
                    return ((COjwProgressBarCell)this.CellTemplate).LowColor;
                }
                set
                {
                    ((COjwProgressBarCell)this.CellTemplate).LowColor = value;
                }
            }

            /// <summary>
            /// ProgressBar Min
            /// </summary>
            public int Mimimum
            {
                get
                {
                    return ((COjwProgressBarCell)this.CellTemplate).Mimimum;
                }
                set
                {
                    if (this.Mimimum == value)
                        return;

                    ((COjwProgressBarCell)this.CellTemplate).Mimimum =
                        value;

                    if (this.DataGridView == null)
                        return;
                    int rowCount = this.DataGridView.RowCount;
                    for (int i = 0; i < rowCount; i++)
                    {
                        DataGridViewRow r = this.DataGridView.Rows.SharedRow(i);
                        ((COjwProgressBarCell)r.Cells[this.Index]).Mimimum =
                            value;
                    }
                }
            }
        }

        /// <summary>
        /// ProgressBar in DataGridView
        /// </summary>
        private class COjwProgressBarCell : DataGridViewTextBoxCell
        {
            // Constructer
            public COjwProgressBarCell()
            {
                this.maximumValue = 100;
                this.mimimumValue = 0;
                this.LowValue = 0;
                this.LowColor = Color.Red;
            }

            private int maximumValue;
            public int Maximum
            {
                get
                {
                    return this.maximumValue;
                }
                set
                {
                    this.maximumValue = value;
                }
            }

            private int mimimumValue;
            public int Mimimum
            {
                get
                {
                    return this.mimimumValue;
                }
                set
                {
                    this.mimimumValue = value;
                }
            }

            //Set Cell's data type

            public override Type ValueType
            {
                get
                {
                    return typeof(int);
                }
            }

            //Set New Cell's data type
            public override object DefaultNewRowValue
            {
                get
                {
                    return 0;
                }
            }

            //Use Clone Function to add new property 
            public override object Clone()
            {
                COjwProgressBarCell cell =
                    (COjwProgressBarCell)base.Clone();
                cell.Maximum = this.Maximum;
                cell.Mimimum = this.Mimimum;
                cell.LowValue = this.LowValue;
                cell.LowColor = this.LowColor;
                return cell;
            }

            private Color lowcolor;// = Color.Red;
            public Color LowColor
            {
                get
                {
                    return this.lowcolor;
                }
                set
                {
                    this.lowcolor = value;
                }
            }
            private int lowvalue;// = 0;
            public int LowValue
            {
                get
                {
                    return this.lowvalue;
                }
                set
                {
                    this.lowvalue = value;
                }
            }

            protected override void Paint(Graphics graphics,
                Rectangle clipBounds, Rectangle cellBounds,

                int rowIndex, DataGridViewElementStates cellState,
                object value, object formattedValue, string errorText,
                DataGridViewCellStyle cellStyle,
                DataGridViewAdvancedBorderStyle advancedBorderStyle,
                DataGridViewPaintParts paintParts)
            {
                bool bOver = false;
                int intValue = 0;
                if (value is int)
                    intValue = (int)value;
                if (intValue < this.mimimumValue)
                    intValue = this.mimimumValue;
                if (intValue > this.maximumValue)
                {
                    intValue = this.maximumValue;
                    bOver = true;
                }

                double rate = (double)(intValue - this.mimimumValue) /
                    (this.maximumValue - this.mimimumValue);


                if ((paintParts & DataGridViewPaintParts.Border) ==
                    DataGridViewPaintParts.Border)
                {
                    this.PaintBorder(graphics, clipBounds, cellBounds,
                        cellStyle, advancedBorderStyle);
                }


                Rectangle borderRect = this.BorderWidths(advancedBorderStyle);
                Rectangle paintRect = new Rectangle(
                    cellBounds.Left + borderRect.Left,
                    cellBounds.Top + borderRect.Top,
                    cellBounds.Width - borderRect.Right,
                    cellBounds.Height - borderRect.Bottom);


                bool isSelected =
                    (cellState & DataGridViewElementStates.Selected) ==
                    DataGridViewElementStates.Selected;
                Color bkColor;
                if (isSelected &&
                    (paintParts & DataGridViewPaintParts.SelectionBackground) ==
                        DataGridViewPaintParts.SelectionBackground)
                {
                    bkColor = cellStyle.SelectionBackColor;
                }
                else
                {
                    bkColor = cellStyle.BackColor;
                }

                if ((paintParts & DataGridViewPaintParts.Background) ==
                    DataGridViewPaintParts.Background)
                {
                    using (SolidBrush backBrush = new SolidBrush(bkColor))
                    {
                        graphics.FillRectangle(backBrush, paintRect);
                    }
                }


                paintRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
                paintRect.Width -= cellStyle.Padding.Horizontal;
                paintRect.Height -= cellStyle.Padding.Vertical;


                // Drawing point for bar(Kor: 여기가 bar 가 그려지는 곳)
                if ((paintParts & DataGridViewPaintParts.ContentForeground) ==
                    DataGridViewPaintParts.ContentForeground)
                {
                    if (ProgressBarRenderer.IsSupported)
                    {
                        ProgressBarRenderer.DrawHorizontalBar(graphics, paintRect);

                        Rectangle barBounds = new Rectangle(
                            paintRect.Left + 3, paintRect.Top + 3,
                            paintRect.Width - 4, paintRect.Height - 6);
                        barBounds.Width = (int)Math.Round(barBounds.Width * rate);
                        ProgressBarRenderer.DrawHorizontalChunks(graphics, barBounds);

                        if ((barBounds.Width > 0) && (intValue <= this.LowValue)) // 30% 이하 값에서는 칼라를 바꾼다.
                        {
                            Brush brush = new System.Drawing.Drawing2D.LinearGradientBrush(barBounds, Color.White, this.LowColor, 90.0f);
                            graphics.FillRectangle(brush, barBounds);
                        }
                    }
                    else
                    {

                        graphics.FillRectangle(Brushes.White, paintRect);
                        graphics.DrawRectangle(Pens.Black, paintRect);
                        Rectangle barBounds = new Rectangle(
                            paintRect.Left + 1, paintRect.Top + 1,
                            paintRect.Width - 1, paintRect.Height - 1);
                        barBounds.Width = (int)Math.Round(barBounds.Width * rate);
                        graphics.FillRectangle(Brushes.Blue, barBounds);
                    }
                }


                if (this.DataGridView.CurrentCellAddress.X == this.ColumnIndex &&
                    this.DataGridView.CurrentCellAddress.Y == this.RowIndex &&
                    (paintParts & DataGridViewPaintParts.Focus) ==
                        DataGridViewPaintParts.Focus &&
                    this.DataGridView.Focused)
                {

                    Rectangle focusRect = paintRect;
                    focusRect.Inflate(-3, -3);
                    ControlPaint.DrawFocusRectangle(graphics, focusRect);

                }


                if ((paintParts & DataGridViewPaintParts.ContentForeground) ==
                    DataGridViewPaintParts.ContentForeground)
                {



                    TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter;

                    Color fColor = cellStyle.ForeColor;

                    paintRect.Inflate(-2, -2);
                    String txt;// = intValue.ToString() + "%";
                    if (bOver == true)
                    {
                        fColor = Color.Red;
                        txt = "충전";
                    }
                    else
                    {
                        txt = intValue.ToString() + "%";
                    }
                    TextRenderer.DrawText(graphics, txt, cellStyle.Font,
                       paintRect, fColor, flags);
                }


                if ((paintParts & DataGridViewPaintParts.ErrorIcon) ==
                    DataGridViewPaintParts.ErrorIcon &&
                    this.DataGridView.ShowCellErrors &&
                    !string.IsNullOrEmpty(errorText))
                {

                    Rectangle iconBounds = this.GetErrorIconBounds(
                        graphics, cellStyle, rowIndex);
                    iconBounds.Offset(cellBounds.X, cellBounds.Y);

                    this.PaintErrorIcon(graphics, iconBounds, cellBounds, errorText);
                }
            }
        }

        public class COjwCellButton : DataGridViewButtonColumn
        {
            public COjwCellButton()
            {
                this.CellTemplate = new COjwCell();
            }

            public bool IsSetted
            {
                get
                {
                    return ((COjwCell)this.CellTemplate).IsSetted;
                }
            }

            public String SetName
            {
                get
                {
                    return ((COjwCell)this.CellTemplate).SetName;
                }
                set
                {
                    ((COjwCell)this.CellTemplate).SetName = value;
                }
            }

            public Color SetButtonColor
            {
                get
                {
                    return ((COjwCell)this.CellTemplate).SetButtonColor;
                }
                set
                {
                    ((COjwCell)this.CellTemplate).SetButtonColor = value;
                }
            }

            public Color SetFontColor
            {
                get
                {
                    return ((COjwCell)this.CellTemplate).SetFontColor;
                }
                set
                {
                    ((COjwCell)this.CellTemplate).SetFontColor = value;
                }
            }

            public Font SetFont
            {
                get
                {
                    return ((COjwCell)this.CellTemplate).SetFont;
                }
                set
                {
                    ((COjwCell)this.CellTemplate).SetFont = value;
                }
            }
            //public bool SetBold
            //{
            //    get
            //    {
            //        return ((COjwCell)this.CellTemplate).SetBold;
            //    }
            //    set
            //    {
            //        ((COjwCell)this.CellTemplate).SetBold = value;
            //    }
            //}

            //Set CellTemplate 
            public override DataGridViewCell CellTemplate
            {
                get
                {
                    return base.CellTemplate;
                }
                set
                {

                    if (!(value is COjwCell))
                    {
                        throw new InvalidCastException(
                            "Set COjwCell object");
                    }
                    base.CellTemplate = value;
                }
            }
            //protected override void Paint(  
            //                                Graphics graphics, 
            //                                Rectangle clipBounds, Rectangle cellBounds, 
            //                                int rowIndex, DataGridViewElementStates elementState, 
            //                                object value, object formattedValue, string errorText, 
            //                                DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
            //{
            //    graphics.FillRectangle(Brushes.Red, clipBounds);

            //    base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
            //    //ButtonRenderer.DrawButton(graphics, cellBounds, formattedValue.ToString(), new Font("Comic Sans MS", 9.0f, FontStyle.Bold), true, System.Windows.Forms.VisualStyles.PushButtonState.Default);
            //}
        }

        public class COjwCell : DataGridViewButtonCell
        {
            public COjwCell()
            {
                this.m_strSetName = "DisConnect";
                this.m_cSetFontColor = Color.Yellow;
                this.m_cSetButtonColor = Color.Red;
                this.m_bIsSetted = false;
                //this.m_bSetFont = ;
            }
            public override object Clone()
            {
                COjwCell cell = (COjwCell)base.Clone();
                cell.m_strSetName = this.m_strSetName;
                cell.m_cSetFontColor = this.m_cSetFontColor;
                cell.m_cSetButtonColor = this.m_cSetButtonColor;
                cell.m_bIsSetted = this.m_bIsSetted;
                return cell;
            }

            private Font m_fntSetFont;
            public Font SetFont
            {
                get
                {
                    return this.m_fntSetFont;
                }
                set
                {
                    this.m_fntSetFont = value;
                }
            }
            //private bool m_bSetBold;
            //public bool SetBold
            //{
            //    get
            //    {
            //        return this.m_bSetBold;
            //    }
            //    set
            //    {
            //        this.m_bSetBold = value;
            //    }
            //}
            private bool m_bIsSetted;
            public bool IsSetted
            {
                get
                {
                    return this.m_bIsSetted;
                }
            }

            private String m_strSetName;
            public String SetName
            {
                get
                {
                    return this.m_strSetName;
                }
                set
                {
                    this.m_strSetName = value;
                }
            }
            private Color m_cSetButtonColor;
            public Color SetButtonColor
            {
                get
                {
                    return this.m_cSetButtonColor;
                }
                set
                {
                    this.m_cSetButtonColor = value;
                }
            }
            private Color m_cSetFontColor;
            public Color SetFontColor
            {
                get
                {
                    return this.m_cSetFontColor;
                }
                set
                {
                    this.m_cSetFontColor = value;
                }
            }

            //private bool m_bDown = false;
            protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
            {
                //m_bDown = true;
                base.OnMouseDown(e);
            }
            protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
            {
                //m_bDown = false;
                base.OnMouseUp(e);
            }
            protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
            {
                //base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

#if false// 선택이 된 경우 색을 달리하고 싶다면...
            bool isSelected =
                (elementState & DataGridViewElementStates.Selected) ==
                DataGridViewElementStates.Selected;
            Color bkColor;
            if (isSelected &&
                (paintParts & DataGridViewPaintParts.SelectionBackground) ==
                    DataGridViewPaintParts.SelectionBackground)
            {
                bkColor = cellStyle.SelectionBackColor;
            }
            else
            {
                bkColor = cellStyle.BackColor;
            }
#endif
                /*
                if (Value != null)
                {
                    Color bkColor = this.SetButtonColor;// Color.Red;
                    if (Value.ToString() == m_strSetName)
                    {
                        Rectangle borderRect = this.BorderWidths(advancedBorderStyle);
                        Rectangle paintRect = new Rectangle(
                            cellBounds.Left + borderRect.Left,
                            cellBounds.Top + borderRect.Top,
                            cellBounds.Width - borderRect.Right,
                            cellBounds.Height - borderRect.Bottom);
                        paintRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
                        paintRect.Width -= cellStyle.Padding.Horizontal;
                        paintRect.Height -= cellStyle.Padding.Vertical;

                        paintRect.Inflate(-1, -1);
                        Rectangle paintRect2 = paintRect;

                        if (m_bDown == true) paintRect.Inflate(-1, -1); // 클릭을 해준 경우 그림을 눌린 이벤트를 하는 것처럼 보이게...

                        // 여기가 그려지는 곳
                        if ((paintParts & DataGridViewPaintParts.ContentForeground) ==
                            DataGridViewPaintParts.ContentForeground)
                        {
                            //Brush brush = new System.Drawing.Drawing2D.LinearGradientBrush(cellBounds, Color.Gray, bkColor, 90.0f);
                            //graphics.FillRectangle(brush, cellBounds); 
                            Brush brush = new System.Drawing.Drawing2D.LinearGradientBrush(paintRect, SystemColors.Control, bkColor, 90.0f);
                            graphics.FillRectangle(brush, paintRect2);
                        }

                        // 여기가 글씨가 써지는 곳
                        if ((paintParts & DataGridViewPaintParts.ContentForeground) ==
                            DataGridViewPaintParts.ContentForeground)
                        {
                            TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                                TextFormatFlags.VerticalCenter;

                            Color fColor = this.SetFontColor;//cellStyle.ForeColor;


                            String txt = Value.ToString();
                            //if (this.m_bChecked == true) txt = "DisConnect";
                            //else txt = "Connect";
                            //Font fntValue = new Font(cellStyle.Font.FontFamily.Name, cellStyle.Font.Size, ((m_bSetBold == true) ? FontStyle.Bold : FontStyle.Regular) );
                            TextRenderer.DrawText(graphics, txt, this.SetFont, paintRect, fColor, flags);
                        }
                        m_bIsSetted = true;
                    }
                    else
                    {
                        base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
                        m_bIsSetted = false;
                    }
                }
                 * */
            }

        }
#endif
        #endregion Grid

        #region PropertyGrid
        public class CProperty
        {
            private PropertyGrid m_propGrid = null;//new PropertyGrid();
            //private object m_obj = null;     
            private Control m_pnProp_Selected = null;
            public void Destroy()
            {
                Destroy(m_pnProp_Selected);
            }
            public void Destroy(Control pnProp)
            {
                if (m_propGrid != null)
                {
                    pnProp.Controls.Remove(m_propGrid);
                    m_propGrid.Dispose();
                }
            }
            public void Create(Control ctrlProp, object objectClass)
            {
                m_propGrid = new PropertyGrid();
                m_propGrid.Dock = DockStyle.Fill;
                m_propGrid.Top = 0;
                m_propGrid.Left = 0;
                m_propGrid.Width = ctrlProp.Width;
                m_propGrid.Height = ctrlProp.Height;
                m_propGrid.Name = "pnOjwProp_User00";// "pnOjwProp_" + Ojw.CConvert.IntToStr((int)ctrlProp.Handle);
                //m_propGrid.LineColor = Color.Black;
                m_propGrid.ToolbarVisible = false;
                m_propGrid.LargeButtons = false;

                m_propGrid.PropertySort = PropertySort.Categorized;
                //CProp CProbItem = new CProp();
                //m_obj = objectClass;
                m_propGrid.SelectedObject = objectClass;// m_obj;// CProbItem;

                //m_propGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(propGrid_PropertyValueChanged);//SelectedGridItemChangedEventHandler(propertyGrid1_PropertyValueChanged);
                //m_propGrid.Text = "";
                //m_propGrid.TextChanged += new System.EventHandler(m_atxtPos_TextChanged);
                ctrlProp.Controls.Add(m_propGrid);
                m_pnProp_Selected = ctrlProp;


                // Event
                //foreach (Control c in m_propGrid.Controls)
                //{
                //    c.Enabled = true;
                //    c.MouseClick += new MouseEventHandler(m_propGrid_MouseClick);
                //}
            }
            //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            //{

            //}

            public void SetEvent_Changed(PropertyValueChangedEventHandler FFunction)
            {
                m_propGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(FFunction);
            }

            void m_propGrid_MouseClick(object sender, MouseEventArgs e)
            {
                //throw new NotImplementedException();
                //Ojw.ShowKeyPad_Number(sender);
            }
            public void RemoveEvent_Changed(PropertyValueChangedEventHandler FFunction)
            {
                m_propGrid.PropertyValueChanged -= new PropertyValueChangedEventHandler(FFunction);
            }

            public void Update()
            {
#if false
                object tmp = m_propGrid.SelectedObject;
                //m_propGrid.SelectedObject = null;
                m_propGrid.SelectedObject = tmp;
#else
                m_propGrid.Refresh();
#endif
            }

            private void propGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
            {

                MessageBox.Show(e.ChangedItem.Label + ":" + (String)e.ChangedItem.Value);
            }




        }
        #endregion PropertyGrid
        #region DynamicPropertyGrid
        public class CDynamicProperty
        {
            private PropertyGrid m_propGrid = null;//new PropertyGrid();
            //private object m_obj = null;
            public void Create(Panel pnProp)//, object objectClass)
            {
                m_propGrid = new PropertyGrid();
                m_propGrid.Dock = DockStyle.Fill;
                m_propGrid.Top = 0;
                m_propGrid.Left = 0;
                m_propGrid.Width = pnProp.Width;
                m_propGrid.Height = pnProp.Height;
                m_propGrid.Name = "pnOjwProp_" + Ojw.CConvert.IntToStr((int)pnProp.Handle);
                //m_propGrid.LineColor = Color.Black;
                m_propGrid.ToolbarVisible = false;
                m_propGrid.LargeButtons = false;

                m_propGrid.PropertySort = PropertySort.Categorized;
                //CProp CProbItem = new CProp();
                //m_obj = objectClass;
                m_propGrid.SelectedObject = myProperties;// objectClass;// m_obj;// CProbItem;

                //m_propGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(propGrid_PropertyValueChanged);//SelectedGridItemChangedEventHandler(propertyGrid1_PropertyValueChanged);
                //m_propGrid.Text = "";
                //m_propGrid.TextChanged += new System.EventHandler(m_atxtPos_TextChanged);
                pnProp.Controls.Add(m_propGrid);
            }
            public void SetEvent_Changed(PropertyValueChangedEventHandler FFunction)
            {
                m_propGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(FFunction);
            }
            public void RemoveEvent_Changed(PropertyValueChangedEventHandler FFunction)
            {
                m_propGrid.PropertyValueChanged -= new PropertyValueChangedEventHandler(FFunction);
            }

            private CustomClass myProperties = new CustomClass();
            public void Add(String strName, String strGroup, String strCaption, object value, bool bReadOnly, bool bVisible)
            {
                CustomProperty myProp = new CustomProperty(strName, strGroup, strCaption, value, bReadOnly, bVisible);
                myProperties.Add(myProp);
                m_propGrid.Refresh();
            }
            public void Remove(String strName)
            {
                myProperties.Remove(strName);
                m_propGrid.Refresh();
            }
            public void Update()
            {
                //object tmp = m_propGrid.SelectedObject;
                //m_propGrid.SelectedObject = null;
                //m_propGrid.SelectedObject = tmp;
                m_propGrid.Refresh();
            }

            public class CustomClass : CollectionBase, ICustomTypeDescriptor
            {
                /// <summary>
                /// Add CustomProperty to Collectionbase List
                /// </summary>
                /// <param name="Value"></param>
                public void Add(CustomProperty Value)
                {
                    base.List.Add(Value);
                }

                /// <summary>
                /// Remove item from List
                /// </summary>
                /// <param name="Name"></param>
                public void Remove(string Name)
                {
                    foreach (CustomProperty prop in base.List)
                    {
                        if (prop.Name == Name)
                        {
                            base.List.Remove(prop);
                            return;
                        }
                    }
                }

                /// <summary>
                /// Indexer
                /// </summary>
                public CustomProperty this[int index]
                {
                    get
                    {
                        return (CustomProperty)base.List[index];
                    }
                    set
                    {
                        base.List[index] = (CustomProperty)value;
                    }
                }


                #region "TypeDescriptor Implementation"
                /// <summary>
                /// Get Class Name
                /// </summary>
                /// <returns>String</returns>
                public String GetClassName()
                {
                    return TypeDescriptor.GetClassName(this, true);
                }

                /// <summary>
                /// GetAttributes
                /// </summary>
                /// <returns>AttributeCollection</returns>
                public AttributeCollection GetAttributes()
                {
                    return TypeDescriptor.GetAttributes(this, true);
                }

                /// <summary>
                /// GetComponentName
                /// </summary>
                /// <returns>String</returns>
                public String GetComponentName()
                {
                    return TypeDescriptor.GetComponentName(this, true);
                }

                /// <summary>
                /// GetConverter
                /// </summary>
                /// <returns>TypeConverter</returns>
                public TypeConverter GetConverter()
                {
                    return TypeDescriptor.GetConverter(this, true);
                }

                /// <summary>
                /// GetDefaultEvent
                /// </summary>
                /// <returns>EventDescriptor</returns>
                public EventDescriptor GetDefaultEvent()
                {
                    return TypeDescriptor.GetDefaultEvent(this, true);
                }
                /// <summary>
                /// GetDefaultProperty
                /// </summary>
                /// <returns>PropertyDescriptor</returns>
                public PropertyDescriptor GetDefaultProperty()
                {
                    return TypeDescriptor.GetDefaultProperty(this, true);
                }
                /// <summary>
                /// GetEditor
                /// </summary>
                /// <param name="editorBaseType">editorBaseType</param>
                /// <returns>object</returns>
                public object GetEditor(Type editorBaseType)
                {
                    return TypeDescriptor.GetEditor(this, editorBaseType, true);
                }
                public EventDescriptorCollection GetEvents(Attribute[] attributes)
                {
                    return TypeDescriptor.GetEvents(this, attributes, true);
                }
                public EventDescriptorCollection GetEvents()
                {
                    return TypeDescriptor.GetEvents(this, true);
                }
                public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
                {
                    PropertyDescriptor[] newProps = new PropertyDescriptor[this.Count];
                    for (int i = 0; i < this.Count; i++)
                    {

                        CustomProperty prop = (CustomProperty)this[i];
                        newProps[i] = new CustomPropertyDescriptor(ref prop, attributes);
                    }

                    return new PropertyDescriptorCollection(newProps);
                }
                public PropertyDescriptorCollection GetProperties()
                {

                    return TypeDescriptor.GetProperties(this, true);

                }
                public object GetPropertyOwner(PropertyDescriptor pd)
                {
                    return this;
                }
                #endregion
            }

            /// <summary>
            /// Custom property class 
            /// </summary>
            public class CustomProperty
            {
                private string sName = string.Empty;
                private string sGroup = string.Empty;
                private string sCaption = string.Empty;
                private bool bReadOnly = false;
                private bool bVisible = true;
                private object objValue = null;
                public CustomProperty(string sName, string strGroup, string strCaption, object value, bool bReadOnly, bool bVisible)
                {
                    this.sName = sName;
                    this.sGroup = strGroup;
                    this.sCaption = strCaption;
                    this.objValue = value;
                    this.bReadOnly = bReadOnly;
                    this.bVisible = bVisible;
                }
                public bool ReadOnly
                {
                    get
                    {
                        return bReadOnly;
                    }
                }
                public string Name
                {
                    get
                    {
                        return sName;
                    }
                }
                public string Group
                {
                    get
                    {
                        return sGroup;
                    }
                }
                public string Caption
                {
                    get
                    {
                        return sCaption;
                    }
                }
                public bool Visible
                {
                    get
                    {
                        return bVisible;
                    }
                }
                public object Value
                {
                    get
                    {
                        return objValue;
                    }
                    set
                    {
                        objValue = value;
                    }
                }
            }

            /// <summary>
            /// Custom PropertyDescriptor
            /// </summary>
            public class CustomPropertyDescriptor : PropertyDescriptor
            {
                CustomProperty m_Property;
                public CustomPropertyDescriptor(ref CustomProperty myProperty, Attribute[] attrs)
                    : base(myProperty.Name, attrs)
                {
                    m_Property = myProperty;
                }

                #region PropertyDescriptor specific
                public override bool CanResetValue(object component)
                {
                    return false;
                }
                public override Type ComponentType
                {
                    get
                    {
                        return null;
                    }
                }
                public override object GetValue(object component)
                {
                    return m_Property.Value;
                }
                public override string Description
                {
                    get
                    {
                        return m_Property.Caption;
                    }
                }
                public override string Category
                {
                    get
                    {
                        return m_Property.Group;
                    }
                }
                public override string DisplayName
                {
                    get
                    {
                        return m_Property.Name;
                    }
                }
                public override bool IsReadOnly
                {
                    get
                    {
                        return m_Property.ReadOnly;
                    }
                }
                public override void ResetValue(object component)
                {
                    //Have to implement
                }
                public override bool ShouldSerializeValue(object component)
                {
                    return false;
                }
                public override void SetValue(object component, object value)
                {
                    m_Property.Value = value;
                }
                public override Type PropertyType
                {
                    get { return m_Property.Value.GetType(); }
                }
                #endregion
            }
        }
        #endregion DynamicPropertyGrid


        #region GridTable
        // 0 - (+), 1 - (-), 2 - mul, 3 - div, 4 - increment, 
        // 5 - decrement, 6 - Change, 7 - Flip Value, 8 - Interpolation, 9 - 'S'Curve, 
        // 10 - Flip Position, 11 - Evd(+), 12 - Evd(-), 13 - EvdSet, 14 - Angle(+), 
        // 15 - Angle(-), 16 - AngleSet, 
        // 17, 18, 19 - Gravity Set(18 - Tilt 만 변화, 19 - Swing 만 변화)
        // 20, 21, 22 - LED Change(20-Red, 21-Green, 22-Blue) - 0 일때 클리어, 1 일때 동작
        // 23 - Motor Enable() - LED 와 동일
        // 24 - MotorType() - LED 와 동일
        // 25 - X(+), 26 - X(-), 27 - Y(+), 28 - (Y-), 29 - Z(+), 30 - Z(-)       
        public enum ECalc_t
        {
            _Plus = 0,
            _Minus,
            _Mul,
            _Div,
            _Inc,
            _Dec,
            _Change,
            _Flip_Value,
            _Interpolation,
            _S_Curve,
            _Flip_Position,
            _11_Reserve,
            _12_Reserve,
            _13_Reserve,
            _14_Reserve,
            _15_Reserve,
            _16_Reserve,
            _17_Reserve,
            _18_Reserve,
            _19_Reserve,
            _20_Reserve,
            _21_Reserve,
            _22_Reserve,
            _23_Reserve,
            _24_Reserve,
            _X_Plus,
            _X_Minus,
            _Y_Plus,
            _Y_Minus,
            _Z_Plus,
            _Z_Minus,
            _X_Input,
            _Y_Input,
            _Z_Input,
            _XYZ_Input
        }
        public struct SGridTable_t
        {
            public string strTitle;
            public int nGroupCol;
            public int nWidth;
            // -1 : 미러된 모터 없음(뒤집기 시 값의 변형 없음). 
            // 0 ~ : 미러링된 모터 번호            
            public int nMirror;
            public Color cColor;
            public object InitValue;
            public object InitValue2;
            public SGridTable_t(string title, int width, int group, int mirror, Color color, object initvalue, object initvalue2) { this.strTitle = title; this.nWidth = width; this.nGroupCol = group; this.nMirror = mirror; this.cColor = color; this.InitValue = initvalue; this.InitValue2 = initvalue2; }
        }
        //public struct SGridLineInfo_t
        //{
        //    public bool bEn;
        //    public int nGroupLine;
        //    public int nCommand;
        //    public float fData0;
        //    public float fData1;
        //    public float fData2;
        //    public float fData3;
        //    public float fData4;
        //    public float fData5;
        //    public float fX;
        //    public float fY;
        //    public float fZ;
        //    public float fPan;
        //    public float fTilt;
        //    public float fSwing;
        //    public string strCaption;
        //    public SGridLineInfo_t(bool enable, int group, int nCommand, float d0, float d1, float d2, float d3, float d4, float d5, string caption, float x, float y, float z, float pan, float tilt, float swing)
        //    {
        //        this.bEn = enable;
        //        this.nGroupLine = group;
        //        this.strCaption = caption;
        //        this.nCommand = nCommand;
        //        this.fData0 = d0;
        //        this.fData1 = d1;
        //        this.fData2 = d2;
        //        this.fData3 = d3;
        //        this.fData4 = d4;
        //        this.fData5 = d5;
        //        this.fX = x;
        //        this.fY = y;
        //        this.fZ = z;
        //        this.fPan = pan;
        //        this.fTilt = tilt;
        //        this.fSwing = swing;
        //    }
        //}
        public class CGridView
        {
            #region Var
            private int m_nTableCount = 0;
            private int m_nKey = 0;  // 키보드의 이벤트 키값을 기억할 변수
            public bool m_bKey_Ctrl = false;
            public bool m_bKey_Alt = false;
            public bool m_bKey_Shift = false;

            public int m_nCurrntCell = 0;
            public int m_nCurrntColumn = 0;
            private int m_nCnt_Col = 0;
            private SGridTable_t[] m_aSGridTable = null;
            //private List<SGridLineInfo_t> m_lstLineInfo = new List<SGridLineInfo_t>();
            private DataGridView dgAngle;
            private Color[] m_colorColData = new Color[10] {
                                            Color.White,
                                            Color.Thistle,  
                                            Color.LightBlue,
                                            Color.Tan,      
                                            Color.Violet,   
                                            Color.Cyan,     
                                            Color.Orange,   
                                            Color.Pink,     
                                            Color.Blue,     
                                            Color.Coral     
                                        };
            #endregion Var

            public int[] m_anIDs = null;
            public void SetIDs(int[] anIDs)
            {
                m_anIDs = anIDs;
            }

            // 그리드 테이블내의 모터 순서
            public int[] m_anIDs_For_Grid = null;
            public void SetIDs_For_Grid(int[] anIDs)
            {
                m_anIDs_For_Grid = anIDs;
            }

            public int GetID_In_Coll(int nCol_Index) { return ((nCol_Index < 0) || (nCol_Index >= m_anIDs_For_Grid.Length)) ? -1 : m_anIDs_For_Grid[nCol_Index]; }
            public int GetCol_In_ID(int nID)
            {
                List<int> lst = new List<int>();
                lst.AddRange(m_anIDs_For_Grid);
                return lst.IndexOf(nID);
            }            

            private bool m_bEventSet = false;
            private void Events_Set()
            {
                if (m_bEventSet == false)
                {
                    dgAngle.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(dgAngle_RowPostPaint);
                    dgAngle.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellEnter);
                    dgAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(dgAngle_KeyDown);
                    dgAngle.KeyUp += new System.Windows.Forms.KeyEventHandler(dgAngle_KeyUp);
                    dgAngle.MouseClick += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseClick);
                    dgAngle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDoubleClick);
                    dgAngle.MouseDown += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDown);
                    dgAngle.MouseMove += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseMove);
                    dgAngle.MouseUp += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseUp);

                    dgAngle.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellLeave);
                    m_bEventSet = true;
                }
            }
            public void Events_Remove()
            {
                //if (m_bEventSet == true)
                //{
                    dgAngle.RowPostPaint -= new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(dgAngle_RowPostPaint);
                    dgAngle.CellEnter -= new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellEnter);
                    dgAngle.KeyDown -= new System.Windows.Forms.KeyEventHandler(dgAngle_KeyDown);
                    dgAngle.KeyUp -= new System.Windows.Forms.KeyEventHandler(dgAngle_KeyUp);
                    dgAngle.MouseClick -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseClick);
                    dgAngle.MouseDoubleClick -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDoubleClick);
                    dgAngle.MouseDown -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDown);
                    dgAngle.MouseMove -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseMove);
                    dgAngle.MouseUp -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseUp);
                    dgAngle.CellLeave -= new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellLeave);
                    m_bEventSet = false;
                //}
            }
            public void Events_Remove_KeyDown() { dgAngle.KeyDown -= new System.Windows.Forms.KeyEventHandler(dgAngle_KeyDown); }
            public void Events_Set_KeyDown() { dgAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(dgAngle_KeyDown); }
            public void Events_Remove_KeyUp() { dgAngle.KeyUp -= new System.Windows.Forms.KeyEventHandler(dgAngle_KeyUp); }
            public void Events_Set_KeyUp() { dgAngle.KeyUp += new System.Windows.Forms.KeyEventHandler(dgAngle_KeyUp); }
            public void Events_Remove_RowPostPaint() { dgAngle.RowPostPaint -= new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(dgAngle_RowPostPaint); }
            public void Events_Set_RowPostPaint() { dgAngle.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(dgAngle_RowPostPaint); }
            public void Events_Remove_CellEnter() { dgAngle.CellEnter -= new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellEnter); }
            public void Events_Set_CellEnter() { dgAngle.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellEnter); }
            public void Events_Remove_MouseClick() { dgAngle.MouseClick -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseClick); }
            public void Events_Set_MouseClick() { dgAngle.MouseClick += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseClick); }
            public void Events_Remove_MouseDoubleClick() { dgAngle.MouseDoubleClick -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDoubleClick); }
            public void Events_Set_MouseDoubleClick() { dgAngle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDoubleClick); }
            public void Events_Remove_MouseDown() { dgAngle.MouseDown -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDown); }
            public void Events_Set_MouseDown() { dgAngle.MouseDown += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDown); }
            public void Events_Remove_MouseMove() { dgAngle.MouseMove -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseMove); }
            public void Events_Set_MouseMove() { dgAngle.MouseMove += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseMove); }
            public void Events_Remove_MouseUp() { dgAngle.MouseUp -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseUp); }
            public void Events_Set_MouseUp() { dgAngle.MouseUp += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseUp); }
            public void Events_Remove_CellLeave() { dgAngle.CellLeave -= new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellLeave); }
            public void Events_Set_CellLeave() { dgAngle.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellLeave); }
            
            public CGridView()
            {
                dgAngle = new DataGridView();
                m_nCnt_Col = 0;
                //m_lstLineInfo.Clear();
            }
            ~CGridView()
            {
                Events_Remove();
                m_tp.Dispose();
            }
            public int GetTableCount() { return m_nTableCount; }
            public int GetLineCount() { return dgAngle.RowCount; }
            //public void ReCreate()
            //{
            //    Create(dgAngle, m_nLineCnt, m_aSGridTable);
            //}

            private static readonly int m_nSize_Time = 1;
            private static readonly int m_nSize_Delay = 1;
            //private static readonly int m_nSize_Display = 1;
            private static readonly int m_nSize_Offset_Group = 1;
            private static readonly int m_nSize_Offset_Command = 1;

            private static readonly int m_nSize_Offset_Data = 6;
            private static readonly int m_nSize_Offset_Buzz = 1;
            private static readonly int m_nSize_Offset_Emoticon = 1;

            private static readonly int m_nSize_Offset_Caption = 1;
            private static readonly int m_nSize_Offset_Trans = 3;
            private static readonly int m_nSize_Offset_Rot = 3;

            private static readonly int m_nSize_Offset = m_nSize_Time + m_nSize_Delay +
                //m_nSize_Display + // Time
                m_nSize_Offset_Group +
                m_nSize_Offset_Command +
                m_nSize_Offset_Data +
                m_nSize_Offset_Buzz +
                m_nSize_Offset_Emoticon +
                m_nSize_Offset_Caption +
                m_nSize_Offset_Trans +
                m_nSize_Offset_Rot;

            //private static readonly int m_nPor_Dummy            = 0;//1;
            private static readonly int m_nPos_Time = m_nSize_Offset;//m_nPos_Delay - 1;//m_nSize_Offset - m_nPor_Dummy;
            private static readonly int m_nPos_Delay = m_nPos_Time - 1;//m_nSize_Offset - (m_nSize_Time + m_nPor_Dummy);
            //private static readonly int m_nPos_Display          = m_nPos_Delay - 1;//m_nSize_Offset - (m_nSize_Time + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Group = m_nPos_Delay - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Command = m_nPos_Offset_Group - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data0 = m_nPos_Offset_Command - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data1 = m_nPos_Offset_Data0 - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 1 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data2 = m_nPos_Offset_Data1 - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 2 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data3 = m_nPos_Offset_Data2 - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 3 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data4 = m_nPos_Offset_Data3 - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 4 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data5 = m_nPos_Offset_Data4 - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 5 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Buzz = m_nPos_Offset_Data5 - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 5 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Emt = m_nPos_Offset_Buzz - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 5 + m_nPor_Dummy);
            //private static readonly int m_nPos_Offset_Group     = m_nPos_Offset_Caption - ;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Caption = m_nPos_Offset_Emt - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Trans = m_nPos_Offset_Caption - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Trans_X = m_nPos_Offset_Trans;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + 0 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Trans_Y = m_nPos_Offset_Trans_X - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + 1 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Trans_Z = m_nPos_Offset_Trans_Y - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + 2 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Rot = m_nPos_Offset_Trans_Z - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + m_nSize_Offset_Trans + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Rot_Pan = m_nPos_Offset_Rot;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + m_nSize_Offset_Trans + 0 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Rot_Tilt = m_nPos_Offset_Rot_Pan - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + m_nSize_Offset_Trans + 1 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Rot_Swing = m_nPos_Offset_Rot_Tilt - 1;//m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + m_nSize_Offset_Trans + 2 + m_nPor_Dummy);

            private int m_nLineCnt = 0;
            public void Create(DataGridView dg, int nLineCnt, params SGridTable_t[] aSGridTable)
            {
                Create(dg, nLineCnt, true, aSGridTable);
            }
            public void Create(DataGridView dg, int nLineCnt, bool bEventSet, params SGridTable_t[] aSGridTable)
            {
                m_nGridMode = 0; // 1 인경우 신형 모션 툴

                int nDefaultWidth = 10;
                if (bEventSet == true) 
                    Events_Remove();
                m_nLineCnt = nLineCnt;
                int nOffsetSize = 2;// 9; // Speed, Delay

                int i;
                dgAngle = dg;
                m_aSGridTable = aSGridTable;
                m_nCnt_Col = aSGridTable.Length - nOffsetSize;

                m_nTableCount = aSGridTable.Length + 1; // Enable 까지

                int nPos = 0;
                int nWidth = 0;
                int nWidth_Offset = 0;

                // DataGridView 컨트롤의 SelectionMode가 ColumnHeaderSelect(으)로 설정되어 있으면 열의 SortMode를 Automatic(으)로 설정할 수 없습니다.
                dgAngle.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                dgAngle.Rows.Clear();
                //m_lstLineInfo.Clear();
                ///////////////

                dgAngle.Columns.Clear();
                dgAngle.AllowUserToAddRows = false; // 자동추가 방지
                dgAngle.RowHeadersWidth = 200;// 150;
                dgAngle.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // 0 - En
                dgAngle.Columns.Add("En", "En");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = 20;// 30;
                nWidth += dgAngle.Columns[nPos++].Width + nWidth_Offset;

                int nIndex = 0;
                for (i = 0; i < m_nCnt_Col; i++)
                {
                    if (m_anIDs != null) nIndex = m_anIDs[i];
                    else nIndex = i;
                    if (aSGridTable[nIndex].strTitle != null) dgAngle.Columns.Add(("Mot" + nIndex.ToString()), aSGridTable[nIndex].strTitle);
                    else dgAngle.Columns.Add("Mot" + nIndex.ToString(), "Mot" + nIndex.ToString());
                    dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                    //dgAngle.Columns[nPos].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;//DataGridViewCellStyle { }

                    dgAngle.Columns[nPos].Width = aSGridTable[i].nWidth; //aSGridTable[i].nWidth;
                    nWidth += dgAngle.Columns[nPos++].Width + nWidth_Offset;
                }

                //// 1 - Time
                dgAngle.Columns.Add("Time", "Time");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = aSGridTable[m_nCnt_Col].nWidth;//10;
                dgAngle.Columns[nPos++].Visible = true;

                //// 2 - Delay
                dgAngle.Columns.Add("Delay", "Delay");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = aSGridTable[m_nCnt_Col + 1].nWidth;//10;
                dgAngle.Columns[nPos++].Visible = true;

                // 3 - Group
                dgAngle.Columns.Add("Group", "Group");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = nDefaultWidth;
                dgAngle.Columns[nPos++].Visible = true;

                // 4 - Command
                dgAngle.Columns.Add("Command", "Command");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = nDefaultWidth;
                dgAngle.Columns[nPos++].Visible = true;

                i = 0;
                for (i = 0; i < 6; i++)
                {
                    // 5 ~ 10 - Data0 ~ Data5
                    dgAngle.Columns.Add("Data" + i.ToString(), "Data" + i.ToString());
                    dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgAngle.Columns[nPos].Width = nDefaultWidth;
                    dgAngle.Columns[nPos++].Visible = true;
                }

                // 11 - Buzz
                dgAngle.Columns.Add("Buzz", "Buzz");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = nDefaultWidth;
                dgAngle.Columns[nPos++].Visible = true;

                // 12 - Emoticon
                dgAngle.Columns.Add("Emoticon", "Emt");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = nDefaultWidth;
                dgAngle.Columns[nPos++].Visible = true;

                // 13 - Caption
                dgAngle.Columns.Add("Caption", "Caption");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = nDefaultWidth;
                dgAngle.Columns[nPos++].Visible = false;

                // 14 ~ 16 - Translate(X, Y, Z)
                string[] pstrTrans = new string[3] { "X", "Y", "Z" };
                for (i = 0; i < 3; i++)
                {
                    dgAngle.Columns.Add(pstrTrans[i], pstrTrans[i]);
                    dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgAngle.Columns[nPos].Width = nDefaultWidth;
                    dgAngle.Columns[nPos++].Visible = false;
                }
                // 17 ~ 19 - Rotate(Pan, tilt, Swing)
                string[] pstrRot = new string[3] { "Pan", "Tilt", "Swing" };
                for (i = 0; i < 3; i++)
                {
                    dgAngle.Columns.Add(pstrRot[i], pstrRot[i]);
                    dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgAngle.Columns[nPos].Width = nDefaultWidth;
                    dgAngle.Columns[nPos++].Visible = false;
                }
                if (nLineCnt > 0) Insert(0, nLineCnt);
                //return dgAngle.Width;

                SetColorGrid(0, 30);//nLineCnt);
                if (bEventSet == true) 
                    Events_Set();
            }
            private void dgAngle_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
            {
                //if (m_bStart == true) return;
                // RowPointPaint 이벤트핸들러
                // 행헤더 열영역에 행번호를 보여주기 위해 장방형으로 처리

                Rectangle rect = new Rectangle(e.RowBounds.Location.X,
                e.RowBounds.Location.Y,
                88,//30,//dgAngle.RowHeadersWidth - 4,
                e.RowBounds.Height + 4);
                // 위에서 생성된 장방형내에 행번호를 보여주고 폰트색상 및 배경을 설정
                TextRenderer.DrawText(e.Graphics,
                    (e.RowIndex + 1).ToString() + ":",
                    dgAngle.RowHeadersDefaultCellStyle.Font,
                    rect,
                    dgAngle.RowHeadersDefaultCellStyle.ForeColor, //Color.Red, //dgAngle.RowHeadersDefaultCellStyle.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

                if ((dgAngle != null) && (GetCaption(e.RowIndex) != ""))
                {
                    string strValue = GetCaption(e.RowIndex);
                    //rect.Inflate(70, 0);
                    //rect.Inflate(500, 0);
                    int nOffset = 88;
                    rect = new Rectangle(e.RowBounds.Location.X + nOffset,
                    e.RowBounds.Location.Y,
                    dgAngle.RowHeadersWidth - nOffset - 4, //200,//dgAngle.RowHeadersWidth - 4,
                    e.RowBounds.Height + 4);
                    if (strValue != "")
                    {
                        TextRenderer.DrawText(e.Graphics,
                            //"        " + "               " + 
                             strValue,
                            dgAngle.RowHeadersDefaultCellStyle.Font,
                            rect,
                            dgAngle.RowHeadersDefaultCellStyle.ForeColor,
                            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                    }
                    /*
                     * <DataGridViewRowPostPaintEventArgs 객체>
                     * e.Graphics - Graphics객체
                     * e.RowIndex - 표시중인 행번호 (0부터 시작하기 떄문에 +1필요) 
                     * e.RowBounds.X 행헤더 열 왼쪽 위 X좌표
                     * e.RowBounds.Y 행헤더 열 왼쪽 위 Y좌표
                     * e.RowBounds.Height 행헤더 열높이
                     * dbView.RowHeadersWidth 행헤더 셀 폭
                     * dbView.RowHeadersDefaultCellStyle.Font 사용폰트
                     * dbView.RowHeadersDefaultCellStyle.FontColor 폰트 색상
                     */
                }

                if (dgAngle != null)
                {
                    // Time Value
                    int nValue = 0;//= GetCaption(e.RowIndex);
                    for (int i = 0; i <= e.RowIndex; i++)
                    {
                        if (GetEnable(i) == true)
                            nValue += GetTime(i) + GetDelay(i);
                    }
                    int nMs = nValue % 1000;
                    int nS = (nValue / 1000) % 60;
                    int nM = (nValue / 60000) % 60;
                    int nH = (nValue / 3600000);
                    String strData = String.Format("{0}:{1}:{2}.{3}", CConvert.IntToStr(nH, 2), CConvert.IntToStr(nM, 2), CConvert.IntToStr(nS, 2), CConvert.IntToStr(nMs, 3));
                    //rect.Inflate(70, 0);
                    //rect.Inflate(500, 0);
                    int nOffset = -10;// 0;
                    rect = new Rectangle(e.RowBounds.Location.X + nOffset,
                    e.RowBounds.Location.Y,
                    dgAngle.RowHeadersWidth - nOffset - 4, //200,//dgAngle.RowHeadersWidth - 4,
                    e.RowBounds.Height + 4);

                    TextRenderer.DrawText(e.Graphics,
                        //"        " + "               " + 
                        strData,//    nValue.ToString(),
                        dgAngle.RowHeadersDefaultCellStyle.Font,
                        rect,
                        dgAngle.RowHeadersDefaultCellStyle.ForeColor,//Color.Blue,//dgAngle.RowHeadersDefaultCellStyle.ForeColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            }
            ////private int m_nLine_GroupNum
            //public void SetGroup_Line(int nLine, int nValue)
            //{
            //    m_lstLineInfo[nLine].nGroupLine = nValue;
            //    m_lstLineInfo[nLine].nGroupLine = nValue;
            //    //m_aSGridTable[nLine].n
            //}
            //private int Grid_GetGroup(int nLine)
            //{
            //    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;
            //    int nRet = 0;
            //    try
            //    {
            //        int nPos = Grid_GetIndex_byMotorAxis(m_C3d.m_CHeader.nMotorCnt - 1);
            //        if (nPos > 0) nRet = (int)Ojw.CConvertFunction.Ojw.CConvert.StrToFloat(dgAngle.Rows[nLine].Cells[nPos + 3].Value.ToString());
            //        return nRet;
            //    }
            //    catch //(System.Exception e)
            //    {
            //        return 0;
            //    }
            //}
            //private void Grid_SetColorGrid(int nIndex)
            //{
            //    if ((nIndex < 0) || (dgAngle.RowCount < nIndex)) return;

            //    int nColorIndex = 0;
            //    for (int j = 0; j < dgAngle.ColumnCount - 1; j++)
            //    {
            //        nColorIndex = 0; // White

            //        int nAxis = j;// Grid_GetAxisNum_byIndex(j);
            //        if ((nAxis >= 0) && (nAxis < m_nCnt_Col))
            //        {
            //            if ((m_aSGridTable[nAxis].nGroup != 0) && (m_aSGridTable[nAxis].nGroup != nColorIndex))
            //            {
            //                nColorIndex = m_aSGridTable[nAxis].nGroup;
            //            }
            //        }
            //        else
            //            return;

            //        Color cColor = m_colorData[nColorIndex];
            //        // Group
            //        int nGroup = Grid_GetGroup(nIndex);
            //        if (nGroup > 0)
            //        {
            //            //Color cColorGroup = Color.Gray;
            //            int nR = 0, nG = 0, nB = 0;
            //            int nColorValue = -50;
            //            if (nGroup == 1) { nR = nColorValue; nG = 0; nB = 0; }
            //            if (nGroup == 2) { nR = 0; nG = nColorValue; nB = 0; }
            //            if (nGroup == 3) { nR = 0; nG = 0; nB = nColorValue; }
            //            cColor = Color.FromArgb(Ojw.CConvert.Clip(255, 0, cColor.R + nR), Ojw.CConvert.Clip(255, 0, cColor.G + nG), Ojw.CConvert.Clip(255, 0, cColor.B + nB));
            //        }
            //        if (dgAngle[j, nIndex].Style.BackColor != m_colorData[nColorIndex])
            //            dgAngle[j, nIndex].Style.BackColor = cColor;
            //    }
            //}
            //private void Grid_SetColorGrid(int nIndex)
            //{
            //    if ((nIndex < 0) || (dgAngle.RowCount < nIndex)) return;

            //    int nColorIndex = 0;
            //    for (int j = 0; j < dgAngle.ColumnCount - 1; j++)
            //    {
            //        nColorIndex = 0; // White

            //        int nAxis = j;// Grid_GetAxisNum_byIndex(j);

            //        if ((m_aSGridTable[nAxis].nGroup != 0) && (m_aSGridTable[nAxis].nGroup != nColorIndex))
            //            nColorIndex = m_aSGridTable[nAxis].nGroup;

            //        for (int i = nIndex; i < nIndex; i++)
            //        {                        
            //            Color cColor = m_colorData[nColorIndex];

            //            // Group
            //            int nGroup = Grid_GetGroup(i);
            //            if (nGroup > 0)
            //            {
            //                //Color cColorGroup = Color.Gray;
            //                int nR = 0, nG = 0, nB = 0;
            //                int nColorValue = -50;
            //                if (nGroup == 1) { nR = nColorValue; nG = 0; nB = 0; }
            //                if (nGroup == 2) { nR = 0; nG = nColorValue; nB = 0; }
            //                if (nGroup == 3) { nR = 0; nG = 0; nB = nColorValue; }
            //                cColor = Color.FromArgb(Ojw.CConvert.Clip(255, 0, cColor.R + nR), Ojw.CConvert.Clip(255, 0, cColor.G + nG), Ojw.CConvert.Clip(255, 0, cColor.B + nB));
            //            }
            //            if (dgAngle[j, i].Style.BackColor != cColor) dgAngle[j, i].Style.BackColor = cColor;
            //        }
            //    }
            //}
            public void Delete() { dgAngle.Rows.Clear(); }
            public void Delete(int nDeleteCnt) { Delete(m_nCurrntCell, nDeleteCnt); }
            public void Delete(int nIndex, int nDeleteCnt)
            {
                for (int i = 0; i < nDeleteCnt; i++)
                {
                    if (nIndex < 0) nIndex = 0;

                    dgAngle.Rows.RemoveAt(nIndex);
                    //m_lstLineInfo.RemoveAt(nIndex);
                }
            }
            public void Insert(int nInsertCnt) { Insert(m_nCurrntCell, nInsertCnt); }
            public void Insert(int nIndex, int nInsertCnt)
            {
                int nErrorNum = 0;
                try
                {
                    if (nIndex < 0) nIndex = 0;
                    if (nIndex >= dgAngle.Rows.Count) nIndex = dgAngle.Rows.Count;
                    int nFirst = nIndex;
                    dgAngle.Rows.Insert(nIndex, nInsertCnt);
                    //m_lstLineInfo.Insert(nIndex, new SGridLineInfo_t(false, 0, "", 0, 0, 0, 0, 0, 0));
                    for (int i = nFirst; i < nFirst + nInsertCnt; i++) Clear(i);
#if !_TEST
                    SetColorGrid(nIndex, nInsertCnt);
#endif
                }
                catch (System.Exception e)
                {
                    Ojw.CMessage.Write_Error("[" + Ojw.CConvert.IntToStr(nErrorNum) + "]" + e.ToString());
                }
            }
            public void Add(int nInsertCnt) { Add(((dgAngle.RowCount == 0) ? 0 : dgAngle.RowCount - 1), nInsertCnt); }
            public void Add(int nIndex, int nInsertCnt)
            {
                if (nIndex < 0) nIndex = 0;

                if (dgAngle.RowCount > 0) nIndex++;
                else nIndex = 0;

                if (nIndex < dgAngle.RowCount - 1)
                {
                    Insert(nIndex, nInsertCnt);
                    //ChangePos(dgAngle, nIndex, dgAngle.CurrentCell.ColumnIndex);
                    return;
                }
                int nFirst = nIndex;

                dgAngle.Rows.Add(nInsertCnt);
                ////////////////
                for (int i = nFirst; i < nFirst + nInsertCnt; i++) Clear(i);
                ChangePos(dgAngle, nIndex, dgAngle.CurrentCell.ColumnIndex);
#if !_TEST
                SetColorGrid(nIndex, nInsertCnt);
#endif
                ////////////////
            }
            public void ChangePos(DataGridView OjwGrid, int nLine, int nPos) { OjwGrid.FirstDisplayedCell = OjwGrid.Rows[nLine].Cells[nPos]; } // OjwGrid.FirstDisplayedScrollingRowIndex;
            public void ChangePos(DataGridView OjwGrid, int nLine) { OjwGrid.FirstDisplayedScrollingRowIndex = nLine; } // OjwGrid.;
            //public void ChangePos_Command(DataGridView OjwGrid, int nLine, int nPos) { OjwGrid[OjwGrid.CurrentCell.ColumnIndex, OjwGrid.CurrentCell.RowIndex].Selected = false; OjwGrid[nPos, nLine].Selected = true; }//OjwGrid.Columns[nPos].Selected = true; } // OjwGrid.FirstDisplayedScrollingRowIndex;
            public void ChangePos_Command(DataGridView OjwGrid, int nLine, int nPos) { OjwGrid[OjwGrid.CurrentCell.ColumnIndex, OjwGrid.CurrentCell.RowIndex].Selected = false; OjwGrid[nPos, nLine].Selected = true; DataGridViewEditMode dgemEdit = OjwGrid.EditMode; OjwGrid.EditMode = DataGridViewEditMode.EditOnEnter; OjwGrid.BeginEdit(true); OjwGrid.EditMode = dgemEdit; }//OjwGrid.Columns[nPos].Selected = true; } // OjwGrid.FirstDisplayedScrollingRowIndex;
            public void ChangePos_Command(DataGridView OjwGrid, int nLine) { OjwGrid[OjwGrid.CurrentCell.ColumnIndex, OjwGrid.CurrentCell.RowIndex].Selected = false; OjwGrid.Rows[nLine].Selected = true; DataGridViewEditMode dgemEdit = OjwGrid.EditMode; OjwGrid.EditMode = DataGridViewEditMode.EditOnEnter; OjwGrid.BeginEdit(true); OjwGrid.EditMode = dgemEdit; } // OjwGrid.;
            private int m_nClear_Type = 0; // 0 = Default, 1 = Init2, -1 = Clear
            public int Clear_GetType() { return m_nClear_Type; } // 0 = Default, 1 = Init2, -1 = Clear
            public void Clear_SetType(int nType) { m_nClear_Type = nType; } // 0 = Default, 1 = Init2, -1 = Clear
            public void Clear(int nLine)
            {
#if false
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle[0, nLine].Value = 0;
                for (int i = 1; i < dgAngle.ColumnCount - 1; i++)
                {
                    if (((i - 1) >= 0) && ((i - 1) < m_aSGridTable.Length))
                        dgAngle.Rows[nLine].Cells[i].Value = m_aSGridTable[i - 1].InitValue;
                    else
                        dgAngle.Rows[nLine].Cells[i].Value = 0;
                }
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 1].Value = String.Empty;

                //Grid_DisplayLine(nLine, false);
#else
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle[0, nLine].Value = 0;
#if false
                int nOffset = 1;
                for (int i = nOffset; i < dgAngle.ColumnCount; i++)// - nOffset; i++)
                {

                    //if (i == dgAngle.ColumnCount - nOffset - m_nPos_Offset_Caption) { dgAngle.Rows[nLine].Cells[i].Value = "TEST"; continue; }
                    if (((i - nOffset) >= 0) && ((i - nOffset) < m_aSGridTable.Length))
                        dgAngle.Rows[nLine].Cells[i].Value = m_aSGridTable[i - nOffset].InitValue;
                    else
                    {
                        if (i == dgAngle.ColumnCount - m_nPos_Offset_Caption) dgAngle.Rows[nLine].Cells[i].Value = "";//"TEST";
                        else dgAngle.Rows[nLine].Cells[i].Value = 0;
                    }

                }
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - nOffset].Value = "";// String.Empty;
#else
                for (int i = 0; i < dgAngle.ColumnCount; i++)
                {
                    if (((i) >= 1) && (i <= dgAngle.ColumnCount - m_nPos_Delay))
                        dgAngle.Rows[nLine].Cells[i].Value = ((m_nClear_Type == 0) ? m_aSGridTable[i - 1].InitValue : ((m_nClear_Type == 1) ? m_aSGridTable[i - 1].InitValue2 : (0)));
                    else
                    {
                        if (i == dgAngle.ColumnCount - m_nPos_Offset_Caption) dgAngle.Rows[nLine].Cells[i].Value = "";//"TEST";
                        else 
                            dgAngle.Rows[nLine].Cells[i].Value = 0;
                    }
                }

#if false // For Test
                int nTest = 1;
                SetGroup(nLine, nTest++);
                SetCommand(nLine, nTest++);
                SetData0(nLine, nTest++);
                SetData1(nLine, nTest++);
                SetData2(nLine, nTest++);
                SetData3(nLine, nTest++);
                SetData4(nLine, nTest++);
                SetData5(nLine, nTest++);
                SetBuzz(nLine, nTest++);
                SetEmt(nLine, nTest++);
                SetCaption(nLine, "k");
                SetOffset_Trans_X(nLine, (float)nTest++);
                SetOffset_Trans_Y(nLine, (float)nTest++);
                SetOffset_Trans_Z(nLine, (float)nTest++);
                SetOffset_Rot_Pan(nLine, (float)nTest++);
                SetOffset_Rot_Tilt(nLine, (float)nTest++);
                SetOffset_Rot_Swing(nLine, (float)16.54);
#endif

#endif
                // Flag


                //Grid_DisplayLine(nLine, false);
#endif
            }
            // 그리드의 라인값을 초기화
            public void Clear()
            {
                for (int i = 0; i < dgAngle.RowCount; i++) Clear(i);
                //SetColorGrid(
            }
            public int OjwGrid_GetCurrentLine() { try { m_nCurrntCell = dgAngle.CurrentCell.RowIndex; return m_nCurrntCell; } catch (Exception ex) { CMessage.Write_Error(ex.ToString()); return 0; } }
            public int OjwGrid_GetCurrentColumn() { try { m_nCurrntColumn = dgAngle.CurrentCell.ColumnIndex; return m_nCurrntColumn; } catch (Exception ex) { CMessage.Write_Error(ex.ToString()); return 0; } }
            private bool m_bIgnore_CellEnter = false;
            public int m_nGridMode = 0;
            public void Ignore_CellEnter(bool bIgnore)
            {
                m_bIgnore_CellEnter = bIgnore;
            }
            private void OjwGrid_CellEnter(DataGridView dgGrid, DataGridViewCellEventArgs e)
            {
                if (m_bIgnore_CellEnter == false)
                {
                    //if (m_bStart == true) return; // 속도의 버벅거림이 없게하기 위해 모션 수행시 이 함수를 실행하지 못하도록 한다.

                    //if ((dgGrid.CurrentCell.ColumnIndex == m_nCurrntColumn) && (dgGrid.CurrentCell.RowIndex == m_nCurrntCell)) return;
                    m_nCurrntColumn = dgGrid.CurrentCell.ColumnIndex;
                    if (dgGrid.Focused == true)
                    {
                        m_nCurrntCell = dgGrid.CurrentCell.RowIndex;

                        if (m_C3d != null)
                        {
                            if (m_bMouseMove == false) SetToolTip(m_nCurrntCell, m_nCurrntColumn);

                            //Grid_DisplayLine(m_nCurrntCell);
                            //CheckFlag(m_nCurrntCell);
                            //lbMotion_Message.Text = "CellEnter=" + Ojw.CConvert.IntToStr(dgGrid.CurrentCell.RowIndex);
                            if (m_anIDs != null)
                            {
                                if ((m_nCurrntColumn > 0) && (m_nCurrntColumn <= m_C3d.m_CHeader.nMotorCnt)) SelectMotor(m_anIDs[m_nCurrntColumn - 1]);
                            }
                            else
                            {
                                if ((m_nCurrntColumn > 0) && (m_nCurrntColumn <= m_C3d.m_CHeader.nMotorCnt)) SelectMotor(m_nCurrntColumn - 1);
                            }
                        }
                    }
                }
            }
            public void SelectMotor(int nAxis)
            {
                Ojw.CMessage.Write("SelectMotor = {0}", nAxis);
                //CDef.g_C3d.SetMousePickEnable(false); // 클릭하는 모터의 색이 변하지 않게 한다.
                if (m_anIDs != null) m_C3d.SelectMotor(m_anIDs[nAxis]);
                else m_C3d.SelectMotor(nAxis);                                
                Ojw.CMessage.Write("SelectMotor = {0}", nAxis);                
            }
            public void SetChangeCurrentLine(int nLine)
            {
                m_nCurrntCell = nLine;
            }
            public void SetChangeCurrentCol(int nColumn)
            {
                m_nCurrntColumn = nColumn;
            }
            public void Grid_ChangePos(int nLine, int nPos)
            {
                if (dgAngle.Rows[nLine].Cells[nPos].Selected == false)
                {
                    m_nCurrntColumn = nPos;
                    m_nCurrntCell = nLine;
                    dgAngle.CurrentCell = dgAngle.Rows[nLine].Cells[nPos];
                }
            }

            #region Control Data
            public void SetEnable(bool data) { SetEnable(m_nCurrntCell, data); }
            public void SetEnable(int nLine, bool bEn)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[0].Value = Ojw.CConvert.BoolToInt(bEn);
            }
            public bool GetEnable() { return GetEnable(m_nCurrntCell); }
            public bool GetEnable(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return false;
                    return Ojw.CConvert.StrToBool(dgAngle.Rows[nLine].Cells[0].Value.ToString());
                }
                catch// (System.Exception e)
                {
                    return false;
                }
            }
            public void Set(int nNum, float data) { Set(m_nCurrntCell, nNum, data); }
            public void Set(int nLine, int nNum, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                if ((m_anIDs != null) && (nNum >= 0) && (nNum < m_anIDs.Length))
                    dgAngle.Rows[nLine].Cells[m_anIDs[nNum + 1]].Value = data;
                else dgAngle.Rows[nLine].Cells[nNum + 1].Value = data;
            }
            public float Get(int nNum) { return Get(m_nCurrntCell, nNum); }
            public float Get(int nLine, int nNum)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    if ((m_anIDs != null) && (nNum >= 0) && (nNum < m_anIDs.Length))
                        return Convert.ToSingle(dgAngle.Rows[nLine].Cells[m_anIDs[nNum + 1]].Value);
                    return Convert.ToSingle(dgAngle.Rows[nLine].Cells[nNum + 1].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;// false;
                }
            }
            public void SetTime(int data) { SetData0(m_nCurrntCell, data); }
            public void SetTime(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Time].Value = data;
            }
            public int GetTime() { return GetTime(m_nCurrntCell); }
            public int GetTime(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Time].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;// false;
                }
            }
            //public object GetSpeed() { return GetTime(); }
            //public object GetSpeed(int nLine) { return GetTime(nLine); }
            public void SetDelay(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Delay].Value = data;
            }
            public int GetDelay() { return GetDelay(m_nCurrntCell); }
            public int GetDelay(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Delay].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;// false;
                }
            }
            public void SetGroup(int data) { SetGroup(m_nCurrntCell, data); }
            public void SetGroup(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Group].Value = data;
            }
            public int GetGroup() { return GetGroup(m_nCurrntCell); }
            public int GetGroup(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Group].Value);
                }
                catch// (System.Exception e)
                {
                    return 0;
                }
            }
            public void SetCommand(int data) { SetData0(m_nCurrntCell, data); }
            public void SetCommand(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Command].Value = data;
            }
            public int GetCommand() { return GetCommand(m_nCurrntCell); }
            public int GetCommand(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Command].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData0(int data) { SetData0(m_nCurrntCell, data); }
            public void SetData0(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data0].Value = data;
            }
            public int GetData0() { return GetData0(m_nCurrntCell); }
            public int GetData0(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data0].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData1(int data) { SetData1(m_nCurrntCell, data); }
            public void SetData1(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data1].Value = data;
            }
            public int GetData1() { return GetData1(m_nCurrntCell); }
            public int GetData1(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data1].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData2(int data) { SetData2(m_nCurrntCell, data); }
            public void SetData2(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data2].Value = data;
            }
            public int GetData2() { return GetData2(m_nCurrntCell); }
            public int GetData2(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data2].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData3(int data) { SetData3(m_nCurrntCell, data); }
            public void SetData3(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data3].Value = data;
            }
            public int GetData3() { return GetData3(m_nCurrntCell); }
            public int GetData3(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data3].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData4(int data) { SetData4(m_nCurrntCell, data); }
            public void SetData4(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data4].Value = data;
            }
            public int GetData4() { return GetData4(m_nCurrntCell); }
            public int GetData4(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data4].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData5(int data) { SetData5(m_nCurrntCell, data); }
            public void SetData5(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data5].Value = data;
            }
            public int GetData5() { return GetData5(m_nCurrntCell); }
            public int GetData5(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data5].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }

            public void SetBuzz(int data) { SetBuzz(m_nCurrntCell, data); }
            public void SetBuzz(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Buzz].Value = data;
            }
            public int GetBuzz() { return GetBuzz(m_nCurrntCell); }
            public int GetBuzz(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Buzz].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetEmt(int data) { SetEmt(m_nCurrntCell, data); }
            public void SetEmt(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Emt].Value = data;
            }
            public int GetEmt() { return GetEmt(m_nCurrntCell); }
            public int GetEmt(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Emt].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }

            public void SetSelectedGroup(int nGroupNum) // 0 - Clear, 1 - Group1, 2 - Group2, 3 - Group3
            {
                // 0~3 까지 데이타 클리핑
                //int nCmd = Ojw.CConvert.Clip(3, 0, nGroupNum);
                int nCmd = Ojw.CConvert.Clip(4, 0, nGroupNum);

                int nX_Limit = dgAngle.RowCount;
                int nY_Limit = dgAngle.ColumnCount;
                //bool bRepeat = false;

                for (int i = 0; i < nX_Limit; i++)
                {
                    for (int j = 0; j < nY_Limit; j++)
                    {
                        if (dgAngle[j, i].Selected == true)
                        {
                            SetGroup(i, nCmd);
                            break;
                        }
                    }
                }

                // 색칠하기...
                SetColorGrid(0, dgAngle.RowCount);
            }

            public void SetCaption(String data) { SetCaption(m_nCurrntCell, data); }
            public void SetCaption(int nLine, String strValue)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Caption].Value = strValue;
            }
            public String GetCaption() { return GetCaption(m_nCurrntCell); }
            public String GetCaption(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return "";
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Caption].Value.ToString();
                }
                catch// (System.Exception e)
                {
                    return "";
                }
            }

            #region Trans / Rot(Offset)
            public void SetOffset_Trans_X(float fX) { SetOffset_Trans_X(m_nCurrntCell, fX); }
            public void SetOffset_Trans_X(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_X].Value = (int)Math.Round(data * 1000); //data;
            }
            public float GetOffset_Trans_X() { return GetOffset_Trans_X(m_nCurrntCell); }
            public float GetOffset_Trans_X(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    return (float)(Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_X].Value)) * 0.001f;
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }
            public void SetOffset_Trans_Y(float fX) { SetOffset_Trans_Y(m_nCurrntCell, fX); }
            public void SetOffset_Trans_Y(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_Y].Value = (int)Math.Round(data * 1000); //data;
            }
            public float GetOffset_Trans_Y() { return GetOffset_Trans_Y(m_nCurrntCell); }
            public float GetOffset_Trans_Y(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    return (float)(Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_Y].Value)) * 0.001f;
                    //return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_Y].Value);
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }
            public void SetOffset_Trans_Z(float fX) { SetOffset_Trans_Z(m_nCurrntCell, fX); }
            public void SetOffset_Trans_Z(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_Z].Value = (int)Math.Round(data * 1000); //data;
            }
            public float GetOffset_Trans_Z() { return GetOffset_Trans_Z(m_nCurrntCell); }
            public float GetOffset_Trans_Z(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    return (float)(Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_Z].Value)) * 0.001f;
                    //return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_Z].Value);
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }

            public void SetOffset_Rot_Pan(float fX) { SetOffset_Rot_Pan(m_nCurrntCell, fX); }
            public void SetOffset_Rot_Pan(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot].Value = (int)Math.Round(data * 1000); //data;
            }
            public float GetOffset_Rot_Pan() { return GetOffset_Rot_Pan(m_nCurrntCell); }
            public float GetOffset_Rot_Pan(int nLine)
            {
                try
                {
                    return (float)(Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Pan].Value)) * 0.001f;
                    //return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Pan].Value);
                    //return Ojw.CConvert.StrToFloat(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Pan].Value.ToString());
                    //if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    //return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Pan].Value);
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }
            public void SetOffset_Rot_Tilt(float fX) { SetOffset_Rot_Tilt(m_nCurrntCell, fX); }
            public void SetOffset_Rot_Tilt(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Tilt].Value = (int)Math.Round(data * 1000); //data;
            }
            public float GetOffset_Rot_Tilt() { return GetOffset_Rot_Tilt(m_nCurrntCell); }
            public float GetOffset_Rot_Tilt(int nLine)
            {
                try
                {
                    return (float)(Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Tilt].Value)) * 0.001f;
                    //return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Tilt].Value);
                    //return Ojw.CConvert.StrToFloat(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Tilt].Value.ToString());

                    //if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    //return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Tilt].Value);
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }
            public void SetOffset_Rot_Swing(float fX) { SetOffset_Rot_Swing(m_nCurrntCell, fX); }
            public void SetOffset_Rot_Swing(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Swing].Value = (int)Math.Round(data * 1000); //data;
            }
            public float GetOffset_Rot_Swing() { return GetOffset_Rot_Swing(m_nCurrntCell); }
            public float GetOffset_Rot_Swing(int nLine)
            {
                try
                {
                    return (float)(Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Swing].Value)) * 0.001f;
                    //return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Swing].Value);
                    //return Ojw.CConvert.StrToFloat(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Swing].Value.ToString());
                    //if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    //return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Swing].Value);
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }
            #endregion Trans / Rot(Offset)

            public void SetColorGrid(int nIndex, int nCount)
            {
                if ((nIndex < 0) || (dgAngle.RowCount < nIndex)) return;
                if (((nIndex + nCount) < 0) || (dgAngle.RowCount < (nIndex + nCount))) return;

                for (int j = 0; j < m_nTableCount; j++)// Enable 제외 //dgAngle.ColumnCount; j++)//dgAngle.ColumnCount - (m_nPos_Delay - 1); j++)
                {
                    //nColorIndex = 0; // White

                    int nAxis = j - 1;

                    for (int i = nIndex; i < nIndex + nCount; i++)
                    {
                        nAxis = j - 1;

                        Color cColor;
                        cColor = (nAxis >= 0) ? m_aSGridTable[nAxis].cColor : Color.White;

                        // Group
                        int nGroup = GetGroup(i);
                        if (nGroup > 0)
                        {
                            int nR = 0, nG = 0, nB = 0;
                            int nColorValue_R = -50; // G
                            int nColorValue_G = -50; // R
                            int nColorValue_B = -150; // B
                            int nColorValue_R2 = -30;
                            int nColorValue_G2 = -70;
                            int nColorValue_B2 = 30;
                            if (nGroup == 1) { nR = (((cColor.R + nColorValue_R) < 0) ? -nColorValue_R : nColorValue_R); }
                            if (nGroup == 2) { nG = (((cColor.G + nColorValue_G) < 0) ? -nColorValue_G : nColorValue_G); }
                            if (nGroup == 3) { nB = (((cColor.B + nColorValue_B) < 0) ? -nColorValue_B : nColorValue_B); }
                            if (nGroup == 4) {
                                nR = (((cColor.R + nColorValue_R2) < 0) ? -nColorValue_R2 : nColorValue_R2); 
                                nG = (((cColor.G + nColorValue_G2) < 0) ? -nColorValue_G2 : nColorValue_G2); 
                                nB = (((cColor.B + nColorValue_B2) < 0) ? -nColorValue_B2 : nColorValue_B2); 
                            }
                            //cColor = Color.FromArgb(Ojw.CConvert.Clip(255, 0, cColor.R + nR), Ojw.CConvert.Clip(255, 0, cColor.G + nG), Ojw.CConvert.Clip(255, 0, cColor.B + nB));
                            cColor = Color.FromArgb(
                                Ojw.CConvert.Clip(255, 0, cColor.R + nR),
                                Ojw.CConvert.Clip(255, 0, cColor.G + nG),
                                Ojw.CConvert.Clip(255, 0, cColor.B + nB)
                                );
                        }                                           
                        if (dgAngle[j, i].Style.BackColor != cColor)//colorData[nColorIndex])
                            dgAngle[j, i].Style.BackColor = cColor;
                        

#if false // test
                        int nMotNum = nAxis;
                        int nLint = i;

                        cColor = Color.FromArgb(0,255,0);
                        //    (((m_pnFlag[nLine, nMotNum] & 0x01) != 0) ? 255 : 0), // R
                        //    (((m_pnFlag[nLine, nMotNum] & 0x04) != 0) ? 255 : 0), // G
                        //    (((m_pnFlag[nLine, nMotNum] & 0x02) != 0) ? 255 : 0) // B
                        //);
                            //System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
                        

                        if (cColor != Color.FromArgb(0, 0, 0))//Color.Black)
                        {
                            //Font fnt = this.Font;//m_CGridMotionEditor.GetHandle().Rows[nLine].Cells[nMotNum + 1].Style.Font;
                            dgAngle[j, i].Style.Font = new Font(dgAngle.Font, FontStyle.Bold);
                            //m_CGridMotionEditor.GetHandle().Rows[nLine].Cells[nMotNum + 1].Style.Font = new Font(fnt.Name, fnt.Size, System.Drawing.FontStyle.Bold, fnt.Unit, fnt.GdiCharSet);
                        }
                        else// if (m_CGridMotionEditor.GetHandle().Rows[nLine].Cells[nMotNum + 1].Style.Font.Bold == true)
                        {
                            //Font fnt = this.Font;//m_CGridMotionEditor.GetHandle().Rows[nLine].Cells[nMotNum + 1].Style.Font;
                            dgAngle[j, i].Style.Font = new Font(dgAngle.Font, FontStyle.Regular);
                            //m_CGridMotionEditor.GetHandle().Rows[nLine].Cells[nMotNum + 1].Style.Font = new Font(fnt.Name, fnt.Size, System.Drawing.FontStyle.Regular, fnt.Unit, fnt.GdiCharSet);
                        }

                        dgAngle[j, i].Style.ForeColor = cColor;
                        //m_CGridMotionEditor.GetHandle().Rows[nLine].Cells[nMotNum + 1].Style.ForeColor = ((bEn == false) ? Color.Gray : cColor);
#endif
                    }
                }
            }
            public bool CheckMotorEn(int nMotorID)
            {
                return ((m_C3d.m_CHeader.pSMotorInfo[nMotorID].nMotor_Enable == 0) || (m_C3d.m_CHeader.pSMotorInfo[nMotorID].nMotor_Enable == 1)) ? true : false;
            }
            public int MotorID_2_GridIndex(int nMotorID)
            {
                //if (m_nGridMode == 1)
                //{
                //    return m_C3d.m_CHeader.pSMotorInfo[nMotorID].nMotionEditor_Index -1;
                //}
                return nMotorID;
            }
            public int GridIndex_2_MotorID(int nGridIndex)
            {
                //if (m_nGridMode == 1)
                //{
                //    for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++)
                //    {
                //        int nMotorID = m_C3d.m_CHeader.anMotorIDs[i];
                //        if (CheckMotorEn(nMotorID) == true)
                //        {
                //            if (nGridIndex + 1 == m_C3d.m_CHeader.pSMotorInfo[nMotorID].nMotionEditor_Index)
                //                return nMotorID;
                //        }
                //    }
                //}
                return nGridIndex;
            }
            public void SetData(int nLine, int nNum, object value)
            {
#if false
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                nNum = MotorID_2_GridIndex
                if ((m_anIDs != null) && (nNum >= 0) && (nNum < m_anIDs.Length))
                    dgAngle.Rows[nLine].Cells[m_anIDs[nNum + 1]].Value = value;
                else dgAngle.Rows[nLine].Cells[nNum + 1].Value = value;
#else
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                if ((m_anIDs != null) && (nNum >= 0) && (nNum < m_anIDs.Length))
                    dgAngle.Rows[nLine].Cells[m_anIDs[nNum + 1]].Value = value;
                else dgAngle.Rows[nLine].Cells[nNum + 1].Value = value;
#endif
            }
            public object GetData(int nLine, int nNum) 
            {
#if true
                //if (m_anIDs != null) && (nNum >= 0) && (nNum < m_anIDs.Length))
                    //return dgAngle.Rows[nLine].Cells[m_anIDs[nNum + 1]].Value;
                return dgAngle.Rows[nLine].Cells[nNum + 1].Value;
#else
                if ((m_anIDs != null) && (nNum >= 0) && (nNum < m_anIDs.Length))
                    return dgAngle.Rows[nLine].Cells[m_anIDs[nNum + 1]].Value;
                return dgAngle.Rows[nLine].Cells[nNum + 1].Value;
#endif
            }

            #endregion Control Data
            //private int m_nCurrentLine = -1;
            //private int m_nCurrentCol = -1;
            private void dgAngle_CellEnter(object sender, DataGridViewCellEventArgs e)
            {
                if (m_bIgnore_CellEnter == false)
                {
                    if (dgAngle.Focused == true)// || (m_bStart == true))
                    {
                        //m_bClick_dbAngle = true;
                        OjwGrid_CellEnter(dgAngle, e);
                    }
                }
            }
            private void dgAngle_CellLeave(object sender, DataGridViewCellEventArgs e)
            {
                //dgAngle.ShowCellToolTips = true;
                m_tp.Hide(dgAngle);
            }

            #region Calc
            // 0 - (+), 1 - (-), 2 - mul, 3 - div, 4 - increment, 5 - decrement, 6 - Change, 7 - Flip Value, 8 - Interpolation, 9 - 'S'Curve, 10 - Flip Position, 11 - Evd(+), 12 - Evd(-), 13 - EvdSet, 14 - Angle(+), 15 - Angle(-), 16 - AngleSet, 
            // 17, 18, 19 - Gravity Set(18 - Tilt 만 변화, 19 - Swing 만 변화)
            // 20, 21, 22 - LED Change(20-Red, 21-Green, 22-Blue) - 0 일때 클리어, 1 일때 동작
            // 23 - Motor Enable() - LED 와 동일
            // 24 - MotorType() - LED 와 동일
            // 25 - X(+), 26 - X(-), 27 - Y(+), 28 - (Y-), 29 - Z(+), 30 - Z(-) 
            // 31 - (X[Input]), 32 - (Y[Input]), 33 - (Z[Input]), 34 - (XYZ[Input])  
            public DataGridView GetHandle() { return dgAngle; }
            public void Calc(int nType, params float [] afValue) { Calc(dgAngle, nType, afValue); }
            public void Calc(DataGridView OjwDataGrid, int nType, params float [] afValue)
            {
                int nPos_Start_X = 0, nPos_Start_Y = 0;
                int nPos_End_X = 0, nPos_End_Y = 0;
                int nX_Limit = OjwDataGrid.RowCount;
                int nY_Limit = OjwDataGrid.ColumnCount - 1;
                // 첫 위치 찾아내기
                int k = 0;
                bool bStart = false;

                bool[,] abModify = new bool[nY_Limit, nX_Limit];

                bool[,] abEquation = new bool[999, 256];

                for (int j = 0; j < nY_Limit; j++) // 가로열
                {
                    bStart = false;
                    for (int i = 0; i < nX_Limit; i++) // 세로열
                    {
                        if (OjwDataGrid[j, i].Selected == true)
                        {
                            // Start
                            if (i == 0)
                            {
                                bStart = true;
                            }
                            else if (OjwDataGrid[j, i - 1].Selected == false)
                            {
                                bStart = true;
                            }
                            else bStart = false;

                            if (bStart == true)
                            {
                                nPos_Start_X = i; nPos_Start_Y = j;

                                for (k = i; k < nX_Limit; k++)
                                {
                                    if (k >= (nX_Limit - 1))
                                    {
                                        nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...
                                    }
                                    else
                                    {
                                        if (OjwDataGrid[j, k + 1].Selected == false)
                                        {
                                            nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...

                                            break;
                                        }
                                    }
                                }

                                if (
                                    (OjwDataGrid[nPos_Start_Y, nPos_Start_X].Value == null) ||
                                    (OjwDataGrid[nPos_End_Y, nPos_End_X].Value == null)
                                    ) continue;
                                // 위치와 값을 알고나서...
                                float fValue_Start = Ojw.CConvert.StrToFloat(OjwDataGrid[nPos_Start_Y, nPos_Start_X].Value.ToString());
                                float fValue_End = Ojw.CConvert.StrToFloat(OjwDataGrid[nPos_End_Y, nPos_End_X].Value.ToString());
                                int nLen = nPos_End_X - nPos_Start_X;

                                // 여기서 계산
                                //float fValue = Ojw.CConvert.StrToFloat(txtChangeValue.Text);
                                float fValueTmp;
                                float fTmp = 0;
                                for (k = nPos_Start_X; k <= nPos_End_X; k++)
                                {
                                    if (OjwDataGrid[j, k].Value == null) continue;
                                    //OjwDataGrid[j, k].Style.BackColor = Color.Blue;
                                    fValueTmp = Ojw.CConvert.StrToFloat(OjwDataGrid[j, k].Value.ToString());//(int)(Math.Round(, 0));
                                    #region 덧셈 (+)
                                    if (nType == 0)
                                    {
                                        OjwDataGrid[j, k].Value = fValueTmp + afValue[0];   // +
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    #endregion
                                    #region 뺄셈 (-)
                                    else if (nType == 1)
                                    {
                                        OjwDataGrid[j, k].Value = fValueTmp - afValue[0];  // -
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    #endregion
                                    #region 곱하기
                                    else if (nType == 2)
                                    {
                                        OjwDataGrid[j, k].Value = fValueTmp * afValue[0];  // *
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    #endregion
                                    #region 나누기
                                    else if (nType == 3)
                                    {
                                        if (afValue[0] != 0)
                                        {
                                            OjwDataGrid[j, k].Value = fValueTmp / afValue[0];  // /
                                            // 수정된 놈이 있으면 체크
                                            abModify[j, k] = true;
                                        }
                                    }
                                    #endregion
                                    #region + 누적
                                    else if (nType == 4) // Increment
                                    {
                                        if (k != nPos_Start_X)
                                        {
                                            fTmp += afValue[0];
                                            OjwDataGrid[j, k].Value = fValue_Start + fTmp;
                                            // 수정된 놈이 있으면 체크
                                            abModify[j, k] = true;
                                        }
                                    }
                                    #endregion
                                    #region - 누적
                                    else if (nType == 5) // Decrement
                                    {
                                        if (k != nPos_Start_X)
                                        {
                                            fTmp += afValue[0];
                                            OjwDataGrid[j, k].Value = fValue_Start - fTmp;
                                            // 수정된 놈이 있으면 체크
                                            abModify[j, k] = true;
                                        }
                                    }
                                    #endregion
                                    #region 변경
                                    else if (nType == 6) // Change
                                    {
                                        OjwDataGrid[j, k].Value = afValue[0];
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    #endregion
                                    #region Flip(지정값에 의한 반대값 취하기)
                                    else if (nType == 7) // Flip
                                    {
                                        // 현재 값
                                        float fTemp1 = afValue[0] - Ojw.CConvert.StrToFloat(OjwDataGrid[j, k].Value.ToString());
                                        float fTemp2 = afValue[0] + fTemp1;
                                        OjwDataGrid[j, k].Value = fTemp2;  // /
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    #endregion
                                    #region Interpolation
                                    else if (nType == 8) // Interpolation
                                    {
                                        afValue[0] = fValue_Start - fValue_End;
                                        float fTemp1 = 0;
                                        float fTemp2 = (float)(nPos_End_X - nPos_Start_X);
                                        if (((k - nPos_Start_X) != 0) && (fTemp2 > 0)) fTemp1 = (float)(afValue[0]) / fTemp2 * (float)(k - nPos_Start_X);
                                        OjwDataGrid[j, k].Value = fValue_Start - fTemp1;
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    #endregion
                                    #region 'S'Curve
                                    else if (nType == 9)
                                    {
                                        afValue[0] = (fValue_Start - fValue_End) / 2.0f;
                                        float fAngle = 0;
                                        if ((k - nPos_Start_X) <= nLen / 2)
                                        {
                                            fAngle = (float)(k - nPos_Start_X) * 90.0f / ((float)(nLen) / 2.0f);
                                            fTmp = afValue[0] - afValue[0] * (float)Ojw.CMath.Cos(fAngle);
                                        }
                                        else
                                        {
                                            fAngle = (float)((float)(k - nPos_Start_X) - (float)nLen / 2.0f) * 90.0f / ((float)(nLen) / 2);
                                            fTmp = afValue[0] + (afValue[0] * (float)Ojw.CMath.Sin(fAngle));
                                        }
                                        OjwDataGrid[j, k].Value = fValue_Start - fTmp;
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    #endregion
                                    #region Flip Position
                                    else if (nType == 10) // Flip Position
                                    {
                                        int nMotID = ((m_anIDs_For_Grid == null) ? j - 1 : GetID_In_Coll(j-1));//j - 1;// Grid_GetAxisNum_byIndex(j);
                                        if (nMotID >= 0)
                                        {
                                            int nMirrorID = CheckMirror(j - 1);
                                            int nMirror = GetCol_In_ID(nMirrorID);
                                            int nGap = nMirror - (j - 1);// nMotID;
                                            if (nMirrorID >= 0) // 축 플리핑
                                            {
                                                if (nMirror < (j - 1))
                                                {
                                                    // 이중복사 방지
                                                    if (OjwDataGrid[j + nGap, k].Selected == false)
                                                    {
                                                        float fTemp1 = Ojw.CConvert.StrToFloat(OjwDataGrid[j, k].Value.ToString());
                                                        //float fTemp2 = fTemp1;
                                                        OjwDataGrid[j, k].Value = OjwDataGrid[j + nGap, k].Value;
                                                        OjwDataGrid[j + nGap, k].Value = fTemp1;
                                                        // 수정된 놈이 있으면 체크
                                                        abModify[j, k] = true;
                                                    }
                                                }
                                                else
                                                {
                                                    float fTemp1 = Ojw.CConvert.StrToFloat(OjwDataGrid[j, k].Value.ToString());
                                                    //float fTemp2 = fTemp1;
                                                    OjwDataGrid[j, k].Value = OjwDataGrid[j + nGap, k].Value;
                                                    OjwDataGrid[j + nGap, k].Value = fTemp1;
                                                    // 수정된 놈이 있으면 체크
                                                    abModify[j, k] = true;
                                                }
                                            }
                                            else if (nMirrorID == -2) // 0을 기준으로 플리핑
                                            {
                                                // 현재 값
                                                OjwDataGrid[j, k].Value = -Ojw.CConvert.StrToFloat(OjwDataGrid[j, k].Value.ToString());
                                                // 수정된 놈이 있으면 체크
                                                abModify[j, k] = true;
                                            }
                                            else
                                            {
                                                // nMirror == -1 -> 변화 없음
                                            }
                                        }
                                    }
                                    #endregion
#if false
                                    #region Gravity Set
                                    if (nType == 17)
                                    {
                                        //Grid_SetGravity(k, true, true);
                                        Gravity(k, 2);
                                    }
                                    else if (nType == 18)
                                    {
                                        //Grid_SetGravity(k, true, false);
                                        Gravity(k, 0);
                                    }
                                    else if (nType == 19)
                                    {
                                        //Grid_SetGravity(k, false, true);
                                        Gravity(k, 1);
                                    }
                                    #endregion

                                    #region LED Change (Red, Green, Blue) - 0 이면 클리어 아니면 셋
                                    else if (nType == 20) // Red
                                    {
                                        int nMotID = Grid_GetAxisNum_byIndex(j);
                                        if (nMotID >= 0) Grid_SetFlag_Led_Red(k, nMotID, chkSetValue.Checked);
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    else if (nType == 21) // Green
                                    {
                                        int nMotID = Grid_GetAxisNum_byIndex(j);
                                        if (nMotID >= 0) Grid_SetFlag_Led_Green(k, nMotID, chkSetValue.Checked);
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    else if (nType == 22) // Blue
                                    {
                                        int nMotID = Grid_GetAxisNum_byIndex(j);
                                        if (nMotID >= 0) Grid_SetFlag_Led_Blue(k, nMotID, chkSetValue.Checked);
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    #endregion

                                    #region Motor Enable Change - 0 이면 클리어 아니면 셋
                                    else if (nType == 23)
                                    {
                                        int nMotID = Grid_GetAxisNum_byIndex(j);
                                        if (nMotID >= 0) Grid_SetFlag_En(k, nMotID, chkSetValue.Checked);
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    #endregion
                                    #region Motor Type Change - 0 이면 클리어 아니면 셋
                                    else if (nType == 24)
                                    {
                                        int nMotID = Grid_GetAxisNum_byIndex(j);
                                        if (nMotID >= 0) Grid_SetFlag_Type(k, nMotID, chkSetValue.Checked);
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    #endregion
#endif
#if true
                                    #region 25 - X(+), 26 - X(-), 27 - Y(+), 28 - (Y-), 29 - Z(+), 30 - Z(-), 31 - X(Input), 32 - Y(Input), 33 - Z(Input), 34 - X,Y,Z(Input)
                                    else if ((nType >= 25) && (nType <= 34))//((nType >= 25) && (nType <= 30))
                                    {
                                        //Array.Clear(abEquation, 0, abEquation.Length);

                                        int nLine = k;

                                        //// 수식 번호 알아내기
                                        int nMotID = j - 1;// Grid_GetAxisNum_byIndex(j);
                                        int nNum = -1;
                                        for (int ii = 0; ii < 256; ii++) // 256개중의 수식에서 찾아낸다.
                                        {
                                            int nMotCnt = m_C3d.m_CHeader.pSOjwCode[ii].nMotor_Max;
                                            for (int jj = 0; jj < nMotCnt; jj++)
                                            {
                                                int nMotNum = m_C3d.m_CHeader.pSOjwCode[ii].pnMotor_Number[jj];
                                                if (nMotNum == nMotID)
                                                {
                                                    nNum = ii;
                                                    break;
                                                }
                                            }
                                            if (nNum >= 0)
                                            {
                                                if (abEquation[nLine, nNum] == false)
                                                {
                                                    if ((nType >= 31) && (nType <= 33))
                                                    {
                                                        switch(nType)
                                                        {
                                                            case 31: Grid_Xyz2Angle_X(nLine, nNum, afValue[0]); break;
                                                            case 32: Grid_Xyz2Angle_Y(nLine, nNum, afValue[0]); break;
                                                            case 33: Grid_Xyz2Angle_Z(nLine, nNum, afValue[0]); break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //float fValue = Ojw.CConvert.StrToFloat(txtChangeValue.Text);
                                                        float fX = ((nType == 25) ? afValue[0] : ((nType == 26) ? -afValue[0] : 0));
                                                        float fY = ((nType == 27) ? afValue[0] : ((nType == 28) ? -afValue[0] : 0));
                                                        float fZ = ((nType == 29) ? afValue[0] : ((nType == 30) ? -afValue[0] : 0));
                                                        //Grid_Xyz2Angle_Inc(i, m_nInverseKinematicsNumber_Test, fX, fY, fZ);
                                                        Grid_Xyz2Angle_Inc(nLine, nNum, fX, fY, fZ);
                                                    }
                                                    // 수정된 놈이 있으면 체크
                                                    abModify[j, k] = true;

                                                    abEquation[nLine, nNum] = true;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                    #endregion 25 - X(+), 26 - X(-), 27 - Y(+), 28 - (Y-), 29 - Z(+), 30 - Z(-)
                                    //if (OjwDataGrid == dgAngle)
                                    //{
                                    //    // Forward
                                    //    int nGroup = CheckKinematicsMotor_ByIndex(j);
                                    //    if (nGroup >= 0)
                                    //    {
                                    //        Grid_ForwardKinematics_Separation(k, nGroup);
                                    //    }
                                    //}
                                    //else// dataGrid_XY2Angle(k);
                                    //{
                                    //    int nGroup = j / 3;
                                    //    Grid_InverseKinematics_Separation(k, nGroup);
                                    //}
#endif
                                }
                            }
                        }
                    }
                    /////////////////////
                }
            }
            public int GetInverseKinematicsNumber(int nLine, int nColumn)
            {
                //// 수식 번호 알아내기
                int nMotID = (m_anIDs != null) ? m_anIDs[nColumn - 1] : nColumn - 1;// Grid_GetAxisNum_byIndex(j);
                int nNum = -1;
                for (int ii = 0; ii < 256; ii++) // 256개중의 수식에서 찾아낸다.
                {
                    int nMotCnt = m_C3d.m_CHeader.pSOjwCode[ii].nMotor_Max;
                    for (int jj = 0; jj < nMotCnt; jj++)
                    {
                        int nMotNum = m_C3d.m_CHeader.pSOjwCode[ii].pnMotor_Number[jj];
                        if (nMotNum == nMotID)
                        {
                            nNum = ii;
                            return nNum;
                        }
                    }
                    //if (nNum >= 0)
                    //{
                    //    if (abEquation[nNum] == false)
                    //    {
                    //        //float fValue = Ojw.CConvert.StrToFloat(txtChangeValue.Text);
                    //        float fX = ((nType == 25) ? fValue : ((nType == 26) ? -fValue : 0));
                    //        float fY = ((nType == 27) ? fValue : ((nType == 28) ? -fValue : 0));
                    //        float fZ = ((nType == 29) ? fValue : ((nType == 30) ? -fValue : 0));
                    //        //Grid_Xyz2Angle_Inc(i, m_nInverseKinematicsNumber_Test, fX, fY, fZ);
                    //        Grid_Xyz2Angle_Inc(nLine, nNum, fX, fY, fZ);
                    //        // 수정된 놈이 있으면 체크
                    //        abModify[j, k] = true;

                    //        abEquation[nNum] = true;
                    //    }
                    //    break;
                    //}
                }
                return nNum;
            }
            public static void Grid_Xyz2Angle_X(C3d OjwC3d, int nLine, int nInverseKinematicsNumber, float fVal)
            {
                float[] afMot = new float[OjwC3d.m_CHeader.nMotorCnt];//Grid_GetMot(nLine);
                for (int i = 0; i < OjwC3d.m_CHeader.nMotorCnt; i++) afMot[i] = CConvert.StrToFloat(OjwC3d.m_CGridMotionEditor.GetData(nLine, i).ToString());
                // 현재 자세를 가져옴
                float fPos_X, fPos_Y, fPos_Z;
                Ojw.CKinematics.CForward.CalcKinematics(OjwC3d.m_CHeader.pDhParamAll[nInverseKinematicsNumber], afMot, out fPos_X, out fPos_Y, out fPos_Z);

                float fAngle_X, fAngle_Y, fAngle_Z;
                OjwC3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

                fPos_X = fVal;

                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                Ojw.CKinematics.CInverse.SetValue_X(fPos_X);
                Ojw.CKinematics.CInverse.SetValue_Y(fPos_Y);
                Ojw.CKinematics.CInverse.SetValue_Z(fPos_Z);
                Ojw.CKinematics.CInverse.SetValue_Motor(afMot); // 모터 현재값 넣어주기

                Ojw.CKinematics.CInverse.CalcCode(ref OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                int nMotCnt = OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].nMotor_Max;
                for (int i = 0; i < nMotCnt; i++)
                {
                    int nMotNum = OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].pnMotor_Number[i];
                    float fValue = (float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum);
                    OjwC3d.m_CGridMotionEditor.SetData(nLine, nMotNum, fValue);
                }
            }
            public static void Grid_Xyz2Angle_Y(C3d OjwC3d, int nLine, int nInverseKinematicsNumber, float fVal)
            {
                float[] afMot = new float[OjwC3d.m_CHeader.nMotorCnt];//Grid_GetMot(nLine);
                for (int i = 0; i < OjwC3d.m_CHeader.nMotorCnt; i++) afMot[i] = CConvert.StrToFloat(OjwC3d.m_CGridMotionEditor.GetData(nLine, i).ToString());
                // 현재 자세를 가져옴
                float fPos_X, fPos_Y, fPos_Z;
                Ojw.CKinematics.CForward.CalcKinematics(OjwC3d.m_CHeader.pDhParamAll[nInverseKinematicsNumber], afMot, out fPos_X, out fPos_Y, out fPos_Z);

                float fAngle_X, fAngle_Y, fAngle_Z;
                OjwC3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

                fPos_Y = fVal;

                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                Ojw.CKinematics.CInverse.SetValue_X(fPos_X);
                Ojw.CKinematics.CInverse.SetValue_Y(fPos_Y);
                Ojw.CKinematics.CInverse.SetValue_Z(fPos_Z);
                Ojw.CKinematics.CInverse.SetValue_Motor(afMot); // 모터 현재값 넣어주기

                Ojw.CKinematics.CInverse.CalcCode(ref OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                int nMotCnt = OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].nMotor_Max;
                for (int i = 0; i < nMotCnt; i++)
                {
                    int nMotNum = OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].pnMotor_Number[i];
                    float fValue = (float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum);
                    OjwC3d.m_CGridMotionEditor.SetData(nLine, nMotNum, fValue);
                }
            }
            public static void Grid_Xyz2Angle_Z(C3d OjwC3d, int nLine, int nInverseKinematicsNumber, float fVal)
            {
                float[] afMot = new float[OjwC3d.m_CHeader.nMotorCnt];//Grid_GetMot(nLine);
                for (int i = 0; i < OjwC3d.m_CHeader.nMotorCnt; i++) afMot[i] = CConvert.StrToFloat(OjwC3d.m_CGridMotionEditor.GetData(nLine, i).ToString());
                // 현재 자세를 가져옴
                float fPos_X, fPos_Y, fPos_Z;
                Ojw.CKinematics.CForward.CalcKinematics(OjwC3d.m_CHeader.pDhParamAll[nInverseKinematicsNumber], afMot, out fPos_X, out fPos_Y, out fPos_Z);

                float fAngle_X, fAngle_Y, fAngle_Z;
                OjwC3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

                fPos_Z = fVal;

                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                Ojw.CKinematics.CInverse.SetValue_X(fPos_X);
                Ojw.CKinematics.CInverse.SetValue_Y(fPos_Y);
                Ojw.CKinematics.CInverse.SetValue_Z(fPos_Z);
                Ojw.CKinematics.CInverse.SetValue_Motor(afMot); // 모터 현재값 넣어주기

                Ojw.CKinematics.CInverse.CalcCode(ref OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                int nMotCnt = OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].nMotor_Max;
                for (int i = 0; i < nMotCnt; i++)
                {
                    int nMotNum = OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].pnMotor_Number[i];
                    float fValue = (float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum);
                    OjwC3d.m_CGridMotionEditor.SetData(nLine, nMotNum, fValue);
                }
            }
            public static void Grid_Xyz2Angle(C3d OjwC3d, int nLine, int nInverseKinematicsNumber, float fX, float fY, float fZ)
            {
                float[] afMot = new float[OjwC3d.m_CHeader.nMotorCnt];//Grid_GetMot(nLine);
                for (int i = 0; i < OjwC3d.m_CHeader.nMotorCnt; i++) afMot[i] = CConvert.StrToFloat(OjwC3d.m_CGridMotionEditor.GetData(nLine, i).ToString());
                // 현재 자세를 가져옴
                float fPos_X, fPos_Y, fPos_Z;
                Ojw.CKinematics.CForward.CalcKinematics(OjwC3d.m_CHeader.pDhParamAll[nInverseKinematicsNumber], afMot, out fPos_X, out fPos_Y, out fPos_Z);

                float fAngle_X, fAngle_Y, fAngle_Z;
                OjwC3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

                fPos_X = fX;
                fPos_Y = fY;
                fPos_Z = fZ;

                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                Ojw.CKinematics.CInverse.SetValue_X(fPos_X);
                Ojw.CKinematics.CInverse.SetValue_Y(fPos_Y);
                Ojw.CKinematics.CInverse.SetValue_Z(fPos_Z);
                Ojw.CKinematics.CInverse.SetValue_Motor(afMot); // 모터 현재값 넣어주기

                Ojw.CKinematics.CInverse.CalcCode(ref OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                int nMotCnt = OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].nMotor_Max;
                for (int i = 0; i < nMotCnt; i++)
                {
                    int nMotNum = OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].pnMotor_Number[i];
                    float fValue = (float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum);
                    OjwC3d.m_CGridMotionEditor.SetData(nLine, nMotNum, fValue);
                }
            }
            public static void Grid_Xyz2Angle(CGridView CGrid, Ojw.C3d OjwC3d, int nLine, int nInverseKinematicsNumber, float fX, float fY, float fZ)
            {
                float[] afMot = new float[OjwC3d.m_CHeader.nMotorCnt];//Grid_GetMot(nLine);
                for (int i = 0; i < OjwC3d.m_CHeader.nMotorCnt; i++) afMot[i] = CConvert.StrToFloat(CGrid.GetData(nLine, i).ToString());
                // 현재 자세를 가져옴
                float fPos_X, fPos_Y, fPos_Z;
                Ojw.CKinematics.CForward.CalcKinematics(OjwC3d.m_CHeader.pDhParamAll[nInverseKinematicsNumber], afMot, out fPos_X, out fPos_Y, out fPos_Z);

                float fAngle_X, fAngle_Y, fAngle_Z;
                OjwC3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

                fPos_X = fX;
                fPos_Y = fY;
                fPos_Z = fZ;

                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                Ojw.CKinematics.CInverse.SetValue_X(fPos_X);
                Ojw.CKinematics.CInverse.SetValue_Y(fPos_Y);
                Ojw.CKinematics.CInverse.SetValue_Z(fPos_Z);
                Ojw.CKinematics.CInverse.SetValue_Motor(afMot); // 모터 현재값 넣어주기

                Ojw.CKinematics.CInverse.CalcCode(ref OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                int nMotCnt = OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].nMotor_Max;
                for (int i = 0; i < nMotCnt; i++)
                {
                    int nMotNum = OjwC3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].pnMotor_Number[i];
                    float fValue = (float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum);
                    CGrid.SetData(nLine, nMotNum, fValue);
                }
            }
            public void Grid_Xyz2Angle_X(int nLine, int nInverseKinematicsNumber, float fVal)
            {
                float[] afMot = new float[m_C3d.m_CHeader.nMotorCnt];//Grid_GetMot(nLine);
                for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) afMot[i] = CConvert.StrToFloat(m_C3d.m_CGridMotionEditor.GetData(nLine, i).ToString());
                // 현재 자세를 가져옴
                float fPos_X, fPos_Y, fPos_Z;
                Ojw.CKinematics.CForward.CalcKinematics(m_C3d.m_CHeader.pDhParamAll[nInverseKinematicsNumber], afMot, out fPos_X, out fPos_Y, out fPos_Z);

                float fAngle_X, fAngle_Y, fAngle_Z;
                m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

                fPos_X = fVal;

                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                Ojw.CKinematics.CInverse.SetValue_X(fPos_X);
                Ojw.CKinematics.CInverse.SetValue_Y(fPos_Y);
                Ojw.CKinematics.CInverse.SetValue_Z(fPos_Z);
                Ojw.CKinematics.CInverse.SetValue_Motor(afMot); // 모터 현재값 넣어주기

                Ojw.CKinematics.CInverse.CalcCode(ref m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                int nMotCnt = m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].nMotor_Max;
                for (int i = 0; i < nMotCnt; i++)
                {
                    int nMotNum = m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].pnMotor_Number[i];
                    float fValue = (float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum);
                    SetData(nLine, nMotNum, fValue);
                }
            }
            public void Grid_Xyz2Angle_Y(int nLine, int nInverseKinematicsNumber, float fVal)
            {
                float[] afMot = new float[m_C3d.m_CHeader.nMotorCnt];//Grid_GetMot(nLine);
                for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) afMot[i] = CConvert.StrToFloat(m_C3d.m_CGridMotionEditor.GetData(nLine, i).ToString());
                // 현재 자세를 가져옴
                float fPos_X, fPos_Y, fPos_Z;
                Ojw.CKinematics.CForward.CalcKinematics(m_C3d.m_CHeader.pDhParamAll[nInverseKinematicsNumber], afMot, out fPos_X, out fPos_Y, out fPos_Z);

                float fAngle_X, fAngle_Y, fAngle_Z;
                m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

                fPos_Y = fVal;

                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                Ojw.CKinematics.CInverse.SetValue_X(fPos_X);
                Ojw.CKinematics.CInverse.SetValue_Y(fPos_Y);
                Ojw.CKinematics.CInverse.SetValue_Z(fPos_Z);
                Ojw.CKinematics.CInverse.SetValue_Motor(afMot); // 모터 현재값 넣어주기

                Ojw.CKinematics.CInverse.CalcCode(ref m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                int nMotCnt = m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].nMotor_Max;
                for (int i = 0; i < nMotCnt; i++)
                {
                    int nMotNum = m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].pnMotor_Number[i];
                    float fValue = (float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum);
                    SetData(nLine, nMotNum, fValue);
                }
            }
            public void Grid_Xyz2Angle_Z(int nLine, int nInverseKinematicsNumber, float fVal)
            {
                float[] afMot = new float[m_C3d.m_CHeader.nMotorCnt];//Grid_GetMot(nLine);
                for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) afMot[i] = CConvert.StrToFloat(m_C3d.m_CGridMotionEditor.GetData(nLine, i).ToString());
                // 현재 자세를 가져옴
                float fPos_X, fPos_Y, fPos_Z;
                Ojw.CKinematics.CForward.CalcKinematics(m_C3d.m_CHeader.pDhParamAll[nInverseKinematicsNumber], afMot, out fPos_X, out fPos_Y, out fPos_Z);

                float fAngle_X, fAngle_Y, fAngle_Z;
                m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

                fPos_Z = fVal;

                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                Ojw.CKinematics.CInverse.SetValue_X(fPos_X);
                Ojw.CKinematics.CInverse.SetValue_Y(fPos_Y);
                Ojw.CKinematics.CInverse.SetValue_Z(fPos_Z);
                Ojw.CKinematics.CInverse.SetValue_Motor(afMot); // 모터 현재값 넣어주기

                Ojw.CKinematics.CInverse.CalcCode(ref m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                int nMotCnt = m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].nMotor_Max;
                for (int i = 0; i < nMotCnt; i++)
                {
                    int nMotNum = m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].pnMotor_Number[i];
                    float fValue = (float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum);
                    SetData(nLine, nMotNum, fValue);
                }
            }
            public void Grid_Xyz2Angle(int nLine, int nInverseKinematicsNumber, float fX, float fY, float fZ)
            {
                float[] afMot = new float[m_C3d.m_CHeader.nMotorCnt];//Grid_GetMot(nLine);
                for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) afMot[i] = CConvert.StrToFloat(m_C3d.m_CGridMotionEditor.GetData(nLine, i).ToString());
                // 현재 자세를 가져옴
                float fPos_X, fPos_Y, fPos_Z;
                //Ojw.CKinematics.CForward.CalcKinematics(m_C3d.m_CHeader.pDhParamAll[nInverseKinematicsNumber], afMot, out fPos_X, out fPos_Y, out fPos_Z);

                float fAngle_X, fAngle_Y, fAngle_Z;
                m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

                fPos_X = fX;
                fPos_Y = fY;
                fPos_Z = fZ;

                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                Ojw.CKinematics.CInverse.SetValue_X(fPos_X);
                Ojw.CKinematics.CInverse.SetValue_Y(fPos_Y);
                Ojw.CKinematics.CInverse.SetValue_Z(fPos_Z);
                Ojw.CKinematics.CInverse.SetValue_Motor(afMot); // 모터 현재값 넣어주기

                Ojw.CKinematics.CInverse.CalcCode(ref m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                int nMotCnt = m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].nMotor_Max;
                for (int i = 0; i < nMotCnt; i++)
                {
                    int nMotNum = m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].pnMotor_Number[i];
                    float fValue = (float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum);
                    SetData(nLine, nMotNum, fValue);
                }
            }
            public void Grid_Xyz2Angle_Inc(int nLine, int nInverseKinematicsNumber, float fX, float fY, float fZ)
            {
                float[] afMot = new float[m_C3d.m_CHeader.nMotorCnt];//Grid_GetMot(nLine);
                for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) afMot[i] = CConvert.StrToFloat(m_C3d.m_CGridMotionEditor.GetData(nLine, i).ToString());
                // 현재 자세를 가져옴
                float fPos_X, fPos_Y, fPos_Z;
                Ojw.CKinematics.CForward.CalcKinematics(m_C3d.m_CHeader.pDhParamAll[nInverseKinematicsNumber], afMot, out fPos_X, out fPos_Y, out fPos_Z);
                
                float fAngle_X, fAngle_Y, fAngle_Z;
                m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);
                //CMath.CalcRot(0, 0, -fAngle_Z, ref fPos_X, ref fPos_Y, ref fPos_Z);
                //CMath.CalcRot(0, -fAngle_Y, 0, ref fPos_X, ref fPos_Y, ref fPos_Z);
                //CMath.CalcRot(-fAngle_X, 0, 0, ref fPos_X, ref fPos_Y, ref fPos_Z);
                //m_C3d.Rotation(m_fDisp_Tilt, m_fDisp_Pan, m_fDisp_Swing, ref fPos_X, ref fPos_Y, ref fPos_Z);

                // 2차원을 3차원으로 회전 : (e.X - m_nX) 를 회전
                // Pan - Y축 회전
                // Tilt - X축 회전
                // Swing - Z축 회전

                //m_fPos_X += (chkFreeze_X.Checked == true) ? 0.0f : fPos_X;
                //m_fPos_Y += (chkFreeze_Y.Checked == true) ? 0.0f : fPos_Y;
                //m_fPos_Z += (chkFreeze_Z.Checked == true) ? 0.0f : fPos_Z;

                fPos_X += fX;
                fPos_Y += fY;
                fPos_Z += fZ;

                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                Ojw.CKinematics.CInverse.SetValue_X(fPos_X);
                Ojw.CKinematics.CInverse.SetValue_Y(fPos_Y);
                Ojw.CKinematics.CInverse.SetValue_Z(fPos_Z);
                // 원래의 각을 V100번지에 넣도록 한다.
                //for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) Ojw.CKinematics.CInverse.SetValue_V(100 + i, m_afOrgMot[i]);
                Ojw.CKinematics.CInverse.SetValue_Motor(afMot); // 모터 현재값 넣어주기

                //                 pnV[i++] = 0;                   // Pan - 현재 조작안함
                //                 pnV[i++] = (fY < 0) ? 0 : 1;    // Swing 선택변수(0-정, 1-역) => Default 1
                //                 pnV[i++] = 1;                   // 하박(무릎) 선택변수 - 해는 3개(v4=0, 1,2(1과 2는 같은 평면), 3) => 실험결과 0 이 정상, 1이 Overflow

                Ojw.CKinematics.CInverse.CalcCode(ref m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber]);
                //OjwMessage(Ojw.CKinematics.CInverse.GetValue_V(0).ToString() + ", " + Ojw.CKinematics.CInverse.GetValue_V(1).ToString() + ", " + Ojw.CKinematics.CInverse.GetValue_V(2).ToString());
                int nMotCnt = m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].nMotor_Max;
                for (int i = 0; i < nMotCnt; i++)
                {
                    int nMotNum = m_C3d.m_CHeader.pSOjwCode[nInverseKinematicsNumber].pnMotor_Number[i];
                    float fValue = (float)Ojw.CKinematics.CInverse.GetValue_Motor(nMotNum);
                    //m_C3d.SetData(nMotNum, fValue);
                    SetData(nLine, nMotNum, fValue);                    
                    //Grid_SetMot(nLine, nMotNum, GetMot(nMotNum));
                    //if (Grid_GetMot(nLine, nMotNum) != GetMot(nMotNum)) Grid_SetMot(nLine, nMotNum, GetMot(nMotNum));
                }
            }
            // -1 : 미러된 모터 없음(뒤집기 시 값의 변형 없음). 
            // -2 : 미러된 모터 없음(뒤집기 시 값을 '0' 을 기준으로 Flip), 
            // 0 ~ : 미러링된 모터 번호            
            public int CheckMirror(int nMotID)
            {
                int nRet = -1;
                //lbMot->Text = "";
                // Flip 가능한 그룹 알아내기			
                if ((nMotID >= 0) && (nMotID < m_aSGridTable.Length))
                {
                    return m_aSGridTable[nMotID].nMirror;
                    //return m_C3d.m_CHeader.pSMotorInfo[nMotID].nAxis_Mirror;
                }
                return nRet;
            }
            #endregion Calc

            private Ojw.C3d m_C3d = null;
            public void SetHeader(Ojw.C3d hC3d)
            {
                m_C3d = hC3d;
            }
            private ToolTip m_tp = new ToolTip();
            private void SetToolTip(int nLine, int nColumn)//int nInverseKinematicsNum)
            {
                if (m_C3d == null) return;
                if ((nColumn <= 0) || ((nColumn - 1) >= m_C3d.m_CHeader.nMotorCnt)) return; 
                int nInverseKinematicsNum = GetInverseKinematicsNumber(nLine, nColumn);
                if (nInverseKinematicsNum < 0) return;

                float[] afMot = new float[m_C3d.m_CHeader.nMotorCnt];//Grid_GetMot(nLine);
                for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++)
                {
                    try
                    {
                        afMot[i] = CConvert.StrToFloat(GetData(nLine, i).ToString());
                    }
                    catch (Exception ex)
                    {
                        afMot[i] = 0.0f;
                    }
                }
                // 현재 자세를 가져옴
                float fX, fY, fZ;
                Ojw.CKinematics.CForward.CalcKinematics(m_C3d.m_CHeader.pDhParamAll[nInverseKinematicsNum], afMot, out fX, out fY, out fZ);
                
                //ToolTip tp = new ToolTip();
                //m_tp = new ToolTip();
                m_tp.ToolTipTitle = String.Format("{0} - XYZ", m_C3d.m_CHeader.pstrGroupName[nInverseKinematicsNum]);
                m_tp.ToolTipIcon = ToolTipIcon.Info;
                m_tp.IsBalloon = true;
                                
                //tp.ShowAlways = true;
                //Rectangle rc = dgAngle[nColumn, nLine].ContentBounds;
                Rectangle rc = dgAngle.GetCellDisplayRectangle(nColumn, nLine, false);
                dgAngle.ShowCellToolTips = false;
                m_tp.Show(String.Format("X:{0}, Y:{1}, Z:{2}", fX, fY, fZ),
                    dgAngle,
                    rc.X + rc.Width / 2,
                    rc.Bottom,
                    2000
                    );
                //tp.SetToolTip(txtPort, String.Format("You can choose your comport numbers = {0}", strData));
                //tp.Show(String.Format("You can choose your comport numbers = {0}", strData), txtPort);
            }
            public bool m_bGridMotionEditing = false;
            //public bool m_bGridAdded = false;

            //int m_nFirstPos_Min_X = 9999999;
            //int m_nFirstPos_Min_Line = 9999999;
            private void dgAngle_KeyDown(object sender, KeyEventArgs e)
            {
                OjwGrid_KeyDown(dgAngle, e);
            }
            private void dgAngle_KeyUp(object sender, KeyEventArgs e)
            {
                OjwGrid_KeyUp(dgAngle, e);
            }
            private void OjwGrid_KeyDown(DataGridView dgGrid, KeyEventArgs e)
            {
                m_nKey = e.KeyValue;

                if (e.Control == true) m_bKey_Ctrl = true; else m_bKey_Ctrl = false;
                if (e.Alt == true) m_bKey_Alt = true; else m_bKey_Alt = false;
                if (e.Shift == true) m_bKey_Shift = true; else m_bKey_Shift = false;

                switch (e.KeyCode)
                {
                    #region Keys.Insert - 삽입
                    case Keys.Insert:
                        {
                            if (m_bBlock == false)
                            {
                                if (dgGrid.Focused == true)
                                {
                                    string strValue = "1";
                                    if (e.Control)
                                    {
                                        if (Ojw.CInputBox.Show("Insert", "뒤로 추가할 테이블의 수를 지정하시오", ref strValue) == DialogResult.OK)
                                        {
                                            int nInsertCnt = Ojw.CConvert.StrToInt(strValue);
                                            int nFirst = m_nCurrntCell;
                                            Add(m_nCurrntCell, nInsertCnt);
                                        }
                                    }
                                    else
                                    {
                                        if (Ojw.CInputBox.Show("Insert", "삽입할 테이블의 수를 지정하시오", ref strValue) == DialogResult.OK)
                                        {
                                            //m_bKeyInsert = true;
                                            int nInsertCnt = Ojw.CConvert.StrToInt(strValue);
                                            int nFirst = m_nCurrntCell;
                                            Insert(m_nCurrntCell, nInsertCnt);
                                            //Grid_Insert(nFirst, nInsertCnt);
                                            //m_bKeyInsert = false;
                                        }
                                    }
                                    //Grid_DisplayLine(m_nCurrntCell);
                                }
                            }
                            else
                            {
                                if (m_rtxtDraw != null)
                                {
                                    if (dgGrid.Focused == true)
                                    {
                                        string strValue = "1";
                                        if (e.Control)
                                        {
                                            if (Ojw.CInputBox.Show("Insert", "뒤로 추가할 테이블의 수를 지정하시오", ref strValue) == DialogResult.OK)
                                            {
                                                int nInsertCnt = Ojw.CConvert.StrToInt(strValue);
                                                if (nInsertCnt > 0)
                                                {
                                                    int nFirst = m_nCurrntCell;
                                                    string strDraw = String.Empty;
                                                    if (m_rtxtDraw.Lines.Length > 0)
                                                    {
                                                        for (int i = 0; i < m_rtxtDraw.Lines.Length; i++)
                                                        {
                                                            strDraw += m_rtxtDraw.Lines[i];

                                                            if (i == m_nCurrntCell)
                                                            {
                                                                for (int j = 0; j < nInsertCnt; j++)
                                                                {
                                                                    strDraw += "\r\n//";
                                                                }
                                                            }
                                                            if (i < m_rtxtDraw.Lines.Length - 1) strDraw += "\r\n";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int j = 0; j < nInsertCnt; j++)
                                                        {
                                                            strDraw += "//";
                                                            if (j < nInsertCnt - 1) strDraw += "\r\n";
                                                        }
                                                        //strDraw = "//";
                                                    }
                                                    m_bGridMotionEditing = true;
                                                    m_rtxtDraw.Text = strDraw;
                                                    //Add(m_nCurrntCell, nInsertCnt);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Ojw.CInputBox.Show("Insert", "삽입할 테이블의 수를 지정하시오", ref strValue) == DialogResult.OK)
                                            {
                                                //m_bKeyInsert = true;
                                                int nInsertCnt = Ojw.CConvert.StrToInt(strValue);
                                                if (nInsertCnt > 0)
                                                {
                                                    int nFirst = m_nCurrntCell;
                                                    string strDraw = String.Empty;
                                                    if (m_rtxtDraw.Lines.Length > 0)
                                                    {
                                                        for (int i = 0; i < m_rtxtDraw.Lines.Length; i++)
                                                        {
                                                            if (i == m_nCurrntCell)
                                                            {
                                                                for (int j = 0; j < nInsertCnt; j++)
                                                                {
                                                                    strDraw += "//\r\n";
                                                                }
                                                            }
                                                            strDraw += m_rtxtDraw.Lines[i];

                                                            if (i < m_rtxtDraw.Lines.Length - 1) strDraw += "\r\n";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int j = 0; j < nInsertCnt; j++)
                                                        {
                                                            strDraw += "//";
                                                            if (j < nInsertCnt - 1) strDraw += "\r\n";
                                                        }
                                                        //strDraw = "//";
                                                    }
                                                    m_bGridMotionEditing = true;
                                                    m_rtxtDraw.Text = strDraw;

                                                    //Insert(m_nCurrntCell, nInsertCnt);
                                                    //Grid_Insert(nFirst, nInsertCnt);
                                                    //m_bKeyInsert = false;
                                                }
                                            }
                                        }
                                        //Grid_DisplayLine(m_nCurrntCell);
                                        //m_bGridAdded = true; // For COjw_12_3D.cs only
                                    }
                                }
                            }

                        }
                        break;
                    #endregion Keys.Insert - 삽입
                    #region Keys.Escape - ESC : 긴급정지
                    //case Keys.Escape:
                    //    {
                    //        Stop();
                    //        //Cmd_Stop(m_nCurrentRobot);
                    //    }
                    //    break;
                    #endregion Keys.Escape - ESC : 긴급정지
                    #region Keys.F4 - 초기자세
                    //case Keys.F4:
                    //    {
                    //        frmMain.Cmd_InitPos(m_nCurrentRobot, frmMain._INITPOS_DEFAULT, 2000);
                    //        //DefaultPosition(1);
                    //    }
                    //    break;
                    #endregion Keys.F4 - 초기자세
                    #region Keys.F5 - 모션
                    //case Keys.F5:
                    //    {
                    //        if (m_bStart == false)
                    //            StartMotion();
                    //    }
                    //    break;
                    #endregion Keys.F5 - 모션
                    #region Keys.F - 주석 검색하기
                    case Keys.F:
                        {
                            String strFind = "";
                            if (Ojw.CInputBox.Show("검색", "검색할 주석의 키워드를 입력하시오", ref strFind) == DialogResult.OK)
                            {
                                int nLine = dgAngle.CurrentCell.RowIndex;
                                int nPos = 0;
                                string strValue = "";
                                bool bFind = false;
                                for (int i = nLine; i < dgAngle.RowCount; i++)
                                {
                                    strValue = GetCaption(i);

                                    // 원하는 문자열이 없다면 -1을 리턴
                                    if (strValue.IndexOf(strFind) >= 0)
                                    {
                                        nPos = i;
                                        bFind = true;
                                        break;
                                    }
                                }
                                if (bFind == true)
                                {
                                    dgAngle[0, nPos].Selected = true;
                                    ChangePos(dgAngle, nPos, 0);
                                }
                                else
                                {
                                    MessageBox.Show("결과 없음.");
                                }
                            }
                        }
                        break;
                    #endregion Keys.F - 주석 검색하기
                    #region Keys.F1 - 위치값 가져오기
                    //case Keys.F1:
                    //    {
                    //        if (dgGrid.Focused == true)
                    //        {
                    //            int _ADDRESS_TORQUE_CONTROL = 52;
                    //            for (int nAxis = 0; nAxis < m_C3d.m_CHeader.nMotorCnt; nAxis++)
                    //            {
                    //                if (m_abEnc[nAxis] == true)//((nAxis >= 6) && (nAxis <= 8)) // ojw5014_genie
                    //                {

                    //                    //Grid_SetMot(m_nCurrntCell, nAxis, 0);  -> Don't care 가 더 낳다.
                    //                }
                    //                else
                    //                {
                    //                    frmMain.OjwMotor.ReadMot(nAxis, _ADDRESS_TORQUE_CONTROL, 8);
                    //                    bool bOk = frmMain.OjwMotor.WaitReceive(nAxis, 1000);
                    //                    if (bOk == false)
                    //                    {
                    //                        //bPass = false;
                    //                    }
                    //                    else
                    //                    {
                    //                        Grid_SetMot(m_nCurrntCell, nAxis, frmMain.OjwMotor.GetPos_Angle(nAxis));
                    //                    }
                    //                }
                    //            }
                    //            //Grid_DisplayLine(m_nCurrntCell);
                    //        }
                    //    }
                    //    break;
                    #endregion Keys.F1 - 위치값 가져오기
                    #region Keys.Delete: - 삭제하기
                    case Keys.Delete:
                        {
                            if (m_bBlock == false)
                            {
                                if (dgGrid.Focused == true)
                                {
                                    if (e.Control)
                                    {
                                        Delete(m_nCurrntCell, 1);
                                    }
                                    else
                                    {
                                        int nPos_Start_X = 0, nPos_Start_Y = 0;
                                        int nPos_End_X = 0, nPos_End_Y = 0;
                                        int nX_Limit = dgGrid.RowCount;
                                        int nY_Limit = dgGrid.ColumnCount;
                                        // 첫 위치 찾아내기
                                        int k = 0;
                                        bool bStart = false;
                                        for (int j = 0; j < nY_Limit; j++)
                                        {
                                            bStart = false;
                                            for (int i = 0; i < nX_Limit; i++)
                                            {
                                                if (dgGrid[j, i].Selected == true)
                                                {
                                                    // Start
                                                    if (i == 0)
                                                    {
                                                        bStart = true;
                                                    }
                                                    else if (dgGrid[j, i - 1].Selected == false)
                                                    {
                                                        bStart = true;
                                                    }
                                                    else bStart = false;

                                                    if (bStart == true)
                                                    {
                                                        nPos_Start_X = i; nPos_Start_Y = j;

                                                        for (k = i; k < nX_Limit; k++)
                                                        {
                                                            if (k >= (nX_Limit - 1))
                                                            {
                                                                nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...
                                                            }
                                                            else
                                                            {
                                                                if (dgGrid[j, k + 1].Selected == false)
                                                                {
                                                                    nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...

                                                                    break;
                                                                }
                                                            }
                                                        }

                                                        for (k = nPos_Start_X; k <= nPos_End_X; k++)
                                                        {
                                                            dgGrid[j, k].Selected = true;
                                                            if (j == dgGrid.ColumnCount - 1) dgGrid[j, k].Value = ""; // Caption
                                                            //else if (j == 0) {} // Index
                                                            else
                                                            {
                                                                dgGrid[j, k].Value = 0;
                                                                //if ((j > 0) && (j <= m_C3d.m_CHeader.nMotorCnt))
                                                                //{
                                                                // Led만 클리어 한다.
                                                                //Grid_SetFlag_Led(k, j, 0);
                                                                //    m_pnFlag[k, j - 1] = (int)(m_pnFlag[k, j - 1] & 0x18);// | (int)(nLed & 0x07));
                                                                //}
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //Grid_DisplayLine(m_nCurrntCell);
                                    //CheckFlag(m_nCurrntCell);
                                }
                            }
                            else
                            {
                                if (dgGrid.Focused == true)
                                {
                                    if (e.Control)
                                    {
                                        int nTmp = 0;
                                        string strDraw = String.Empty;
                                        if (m_rtxtDraw.Lines.Length > 0)
                                        {
                                            for (int i = 0; i < m_rtxtDraw.Lines.Length; i++)
                                            {
                                                if (i != m_nCurrntCell)
                                                {
                                                    strDraw += m_rtxtDraw.Lines[i];
                                                    if (i < m_rtxtDraw.Lines.Length - 1) strDraw += "\r\n";
                                                }
                                                else nTmp++;
                                            }
                                        }
                                        if (m_nCurrntCell > 0) m_nCurrntCell -= nTmp;

                                        //else strDraw = "//";
                                        m_bGridMotionEditing = true;
                                        m_rtxtDraw.Text = strDraw;
                                    }
                                }
                            }
                        }
                        break;
                    #endregion Keys.Delete: - 삭제하기
                    #region Keys.V - 붙여넣기
                    case Keys.V:
                        {
                            if (m_bBlock == false)
                            {
                                if (dgGrid.Focused == true)
                                {
                                    try
                                    {
                                        //int nCntPos = 0;

                                        int nPos_X = 0, nPos_Y = 0;
                                        bool bPass = false;
                                        int nX_Limit = dgGrid.RowCount;
                                        int nY_Limit = dgGrid.ColumnCount;

                                        #region 첫 위치 찾아내기
                                        for (int i = 0; i < nX_Limit; i++)
                                        {
                                            for (int j = 0; j < nY_Limit; j++)
                                            {
                                                if ((dgGrid[j, i].Selected == true) && (bPass == false))
                                                {
                                                    nPos_X = i; nPos_Y = j;
                                                    //Message(CConvert.IntToStr(nPos_X) + ", " + CConvert.IntToStr(nPos_Y));
                                                    bPass = true;
                                                    break;
                                                }
                                            }
                                            if (bPass == true) break;
                                        }
                                        #endregion

                                        // 복사된 행의 열을 구하기 위하여 클립보드 사용.
                                        IDataObject iData = Clipboard.GetDataObject();
                                        string strClp = (string)iData.GetData(DataFormats.Text);

                                        if (strClp == null) break;

                                        string strClip = "";

                                        #region Tab, \r\n 의 개수를 셈
                                        int nCnt = 0;
                                        int nT_Cnt = 0;
                                        int nLine_Cnt = 0;
                                        string strDisp = "";
                                        for (int i = 0; i < strClp.Length; i++)
                                        {
                                            if (strClp[i] == '\t') nT_Cnt++;
                                            else if (strClp[i] == '\n') nLine_Cnt++;
                                            if (strClp[i] != '\r')
                                            {
                                                if ((i == strClp.Length - 1) && (strClp[i] < 0x20)) break;
                                                if ((strClp[i] >= 0x20) && (strClp[i] != '\t') && (strClp[i] != '\n'))
                                                {
                                                    nCnt++;
                                                    strDisp += strClp[i];
                                                }
                                                strClip += strClp[i];
                                            }
                                        }
                                        #endregion

                                        int nW = 0, nH = 0;
                                        int nAll = 0;
                                        if (strClip.Length > 0)
                                        {
                                            // strClip -> 이 데이타가 진짜
                                            //nW = 1; 
                                            nH = 1;
                                            nAll = 1;
                                            for (int i = 0; i < strClip.Length; i++)
                                            {
                                                // 가로열, 세로열 카운트
                                                if (strClip[i] == '\n') nH++;
                                                if ((strClip[i] == '\n') || (strClip[i] == '\t')) nAll++;
                                            }
                                            nW = (int)Math.Round((float)nAll / (float)nH, 0);
                                            //Message("nW = " + CConvert.IntToStr(nW) + ", nH = " + CConvert.IntToStr(nH));

                                            bool bW = false, bH = false;
                                            if (nW >= nY_Limit) bW = true;
                                            if (nH >= nX_Limit) bH = true;

                                            String[,] pstrValue = new string[nW, nH];
                                            bool[,] pbValid = new bool[nW, nH];
                                            int nX = 0, nY = 0;
                                            for (int i = 0; i < nW; i++) // 초기화
                                                for (int j = 0; j < nH; j++)
                                                {
                                                    pstrValue[i, j] = "";
                                                    pbValid[i, j] = false;
                                                }

                                            for (int i = 0; i < strClip.Length; i++)
                                            {
                                                if (strClip[i] == '\n') { nY++; nX = 0; }
                                                else if (strClip[i] == '\t') nX++;
                                                else
                                                {
                                                    pbValid[nX, nY] = true;
                                                    pstrValue[nX, nY] += strClip[i];
                                                }
                                            }

                                            if (e.Shift)
                                                Insert(m_nCurrntCell, nH);
                                            //Grid_Insert(nPos_X, nH);
                                            else
                                            {
                                                // 모자란 라인 채우기
                                                if (nH > dgGrid.RowCount)
                                                {
                                                    Insert(m_nCurrntCell, nH - dgGrid.RowCount);
                                                }
                                            }

                                            #region 실 데이타 저장
                                            ////// 실 데이타 저장 ///////
                                            // Display
                                            int nOffset_i = 0, nOffset_j = 0;
                                            if (bW == true) nOffset_i++;
                                            if (bH == true) nOffset_j++;
                                            string strTmp;

                                            for (int j = 0; j < nH - nOffset_j; j++)
                                                for (int i = 0; i < nW - nOffset_i; i++)
                                                {
                                                    strTmp = pstrValue[i + nOffset_i, j + nOffset_j];
                                                    if (((nPos_X + j) < dgGrid.RowCount) && ((nPos_Y + i) < nY_Limit))
                                                    {
                                                        if ((pbValid[i + nOffset_i, j + nOffset_j] == true))
                                                        {
                                                            //dgGrid[nPos_Y + i, nPos_X + j].Style.BackColor = Color.Blue;
                                                            // Data
                                                            dgGrid[nPos_Y + i, nPos_X + j].Value = strTmp;
                                                            dgGrid[nPos_Y + i, nPos_X + j].Selected = true;

                                                            //                                             if (dgGrid == dgAngle)
                                                            //                                             {
                                                            //                                                 // Forward
                                                            //                                                 int nGroup = CheckKinematicsMotor_ByIndex(nPos_X + j);
                                                            //                                                 if (nGroup >= 0)
                                                            //                                                 {
                                                            //                                                     Grid_ForwardKinematics_Separation(nPos_Y + i, nGroup);
                                                            //                                                 }
                                                            //                                             }
                                                            //                                             else// dataGrid_XY2Angle(k);
                                                            //                                             {
                                                            //                                                 int nGroup = j / 3;
                                                            //                                                 Grid_InverseKinematics_Separation(nPos_Y + i, nGroup);
                                                            //                                             }
                                                        }
                                                    }
                                                }
                                            //                                 for (int j = 0; j < nH - nOffset_j; j++)
                                            //                                 {
                                            //                                     if (dgGrid == dataGrid_Angle) dataGrid_Angle2XY(nPos_X + j);
                                            //                                     else dataGrid_XY2Angle(nPos_X + j);
                                            //                                 }
                                            #endregion



                                        }
                                        //m_nFirstPos_Min_X = 9999999;
                                        //m_nFirstPos_Min_Line = 9999999;
                                        //Grid_DisplayLine(m_nCurrntCell);
                                    }
                                    catch (Exception e2)
                                    {
                                        MessageBox.Show(e2.ToString());
                                    }
                                }
                            }
                        }
                        break;
                    #endregion Keys.V - 붙여넣기
                    #region Keys.C - 복사하기
                    case Keys.C:
                        {
                            if (dgGrid.Focused == true)
                            {
                                try
                                {
                                    //bool bLed = true;
                                    //DialogResult dlgRet = MessageBox.Show("Do you want to copy this with some LED Values?", "Copy", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                                    //if (dlgRet != DialogResult.OK)
                                    //{
                                    //    bLed = false;
                                    //}

                                    //m_nFirstPos_Min_X = 9999999;
                                    //m_nFirstPos_Min_Line = 9999999;

                                    int nPos_Start_X = 0, nPos_Start_Y = 0;
                                    int nPos_End_X = 0, nPos_End_Y = 0;
                                    int nX_Limit = dgGrid.RowCount;
                                    int nY_Limit = dgGrid.ColumnCount;
                                    // 첫 위치 찾아내기
                                    int k = 0;
                                    bool bStart = false;
                                    for (int j = 0; j < nY_Limit; j++)
                                    {
                                        bStart = false;
                                        for (int i = 0; i < nX_Limit; i++)
                                        {
                                            if (dgGrid[j, i].Selected == true)
                                            {
                                                // Start
                                                if (i == 0)
                                                {
                                                    bStart = true;
                                                }
                                                else if (dgGrid[j, i - 1].Selected == false)
                                                {
                                                    bStart = true;
                                                }
                                                else bStart = false;

                                                if (bStart == true)
                                                {
                                                    nPos_Start_X = i; nPos_Start_Y = j;

                                                    for (k = i; k < nX_Limit; k++)
                                                    {
                                                        if (k >= (nX_Limit - 1))
                                                        {
                                                            nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...
                                                        }
                                                        else
                                                        {
                                                            if (dgGrid[j, k + 1].Selected == false)
                                                            {
                                                                nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...

                                                                break;
                                                            }
                                                        }
                                                    }

                                                    // 위치와 값을 알고나서...
                                                    //nValue_Start = Convert.ToInt16(rowData[nPos_Start_X][pstrData[nPos_Start_Y]]);
                                                    //nValue_End = Convert.ToInt16(rowData[nPos_End_X][pstrData[nPos_End_Y]]);
                                                    //int nLen = nPos_End_X - nPos_Start_X;

                                                    // 여기서 계산
                                                    //int nValue = CConvert.StrToInt(txtChangeValue.Text);

                                                    //bool bFirst = true;
                                                    for (k = nPos_Start_X; k <= nPos_End_X; k++)
                                                    {
                                                        //dgGrid[j, k].Style.BackColor = Color.Blue;
                                                        dgGrid[j, k].Selected = true;
                                                        if (j == dgGrid.ColumnCount - 1) dgGrid[j, k].Value = ""; // Caption
                                                        //else if (j == 0) {} // Index
                                                        else
                                                        {

#if _COPY_FLAG
                                                        int nPosLine = k;
                                                        int nPosMotor = j;

                                                        m_pnFlag_Copy[nPosLine, nPosMotor] = (int)((m_pnFlag[nPosLine, nPosMotor] & 0x18) | (int)(m_pnFlag[nPosLine, nPosMotor] & 0x07));
                                                        m_nFirstPos_Min_Line = (m_nFirstPos_Min_Line > nPosLine) ? nPosLine : m_nFirstPos_Min_Line;
                                                        m_nFirstPos_Min_X = (m_nFirstPos_Min_X > nPosMotor) ? nPosMotor : m_nFirstPos_Min_X;

#if false
                                                        if ((j >= 0) && (j <= m_C3d.m_CHeader.nMotorCnt))
                                                        {
                                                            int m = j;
                                                            if ((m >= 1) && (m <= m_C3d.m_CHeader.nMotorCnt))
                                                            {
                                                                // Led만 복사 한다.
                                                                //Grid_SetFlag_Led(k, j, 0); 
                                                                m_pnFlag_Copy[k, m - 1] = (int)((m_pnFlag[k, m - 1] & 0x18) | (int)(m_pnFlag[k, m - 1] & 0x07));

                                                                m_nFirstPos_Min_Line = (m_nFirstPos_Min_Line > k) ? k : m_nFirstPos_Min_Line;
                                                                m_nFirstPos_Min_X = (m_nFirstPos_Min_X > (m - 1)) ? (m - 1) : m_nFirstPos_Min_X;

                                                                //m_pnFlag_Copy_Pos[k, m - 1, 0] = k;
                                                                //m_pnFlag_Copy_Pos[k, m - 1, 1] = m - 1;
                                                                //if (bFirst == true)
                                                                //{
                                                                    m_pnFlag_Offset_Num_Line[nCntPos] = k;
                                                                    m_pnFlag_Offset_Num_Motor[nCntPos] = m - 1;
                                                                    nCntPos++;
                                                                    //bFirst = false;
                                                                //}
                                                            }
                                                        }
#endif
#endif
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //Grid_DisplayLine(m_nCurrntCell);
                                }
                                catch (Exception e2)
                                {
                                    MessageBox.Show(e2.ToString());
                                }
                            }
                        }
                        break;
                    #endregion Keys.C - 복사하기
                }
            }
            private void OjwGrid_KeyUp(object sender, KeyEventArgs e)
            {
                m_nKey = 0;
                m_bKey_Ctrl = false;
                m_bKey_Alt = false;
                m_bKey_Shift = false;

                //m_bKeyDown = false;
            }
            private bool m_bBlock = false;
            private RichTextBox m_rtxtDraw = null;
            public void dgAngle_Block_GridChange(RichTextBox rtxtDraw, bool bBlock) { m_rtxtDraw = rtxtDraw; m_bBlock = bBlock; }
            private void dgAngle_MouseClick(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                {
                    DataGridView.HitTestInfo hti = dgAngle.HitTest(e.X, e.Y);
                    int a = hti.RowIndex;
                    int b = hti.ColumnIndex;
                    if ((b < 0) && (a >= 0) && (a < dgAngle.RowCount))
                    {
                        //Grid_CaptionControl(m_bKey_Ctrl); // 컨트롤 키가 눌리면 삽입, 아니라면 변경
                    }
                    else
                    {
                        //OjwGrid_SetMotion();
                    }
                }
            }
            private bool m_bMouseDown = false;
            private bool m_bMouseMove = false;
            private void dgAngle_MouseDown(object sender, MouseEventArgs e)
            {
                DataGridView.HitTestInfo hti = dgAngle.HitTest(e.X, e.Y);
                int a = hti.RowIndex;
                int b = hti.ColumnIndex;

                if (a < 0) dgAngle.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
                else if (b < 0) dgAngle.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;

                m_bMouseDown = true;
                m_bMouseMove = false;
            }
            private void dgAngle_MouseMove(object sender, MouseEventArgs e)
            {
                if (m_bMouseDown == true) m_bMouseMove = true;
            }
            private void dgAngle_MouseUp(object sender, MouseEventArgs e)
            {
                m_bMouseDown = false;
                m_bMouseMove = false;
            }            
            private void dgAngle_MouseDoubleClick(object sender, MouseEventArgs e)
            {
                OjwGrid_CellMouseDoubleClick(dgAngle, e);
            }
            //public void Add_CellMouseDoubleClick(DataGridViewCellMouseEventHandler FFunc) { dgAngle.CellMouseDoubleClick += (DataGridViewCellMouseEventHandler)FFunc; }
            //public void Sub_CellMouseDoubleClick(DataGridViewCellMouseEventHandler FFunc) { dgAngle.CellMouseDoubleClick -= (DataGridViewCellMouseEventHandler)FFunc; }
            private void OjwGrid_CellMouseDoubleClick(DataGridView dgData, MouseEventArgs e)
            {
                DataGridView.HitTestInfo hti = dgData.HitTest(e.X, e.Y);
                int a = hti.RowIndex;
                int b = hti.ColumnIndex;
                if ((b < 0) && (a >= 0) && (a < dgData.RowCount))
                {
                    CaptionControl(m_bKey_Ctrl); // 컨트롤 키가 눌리면 삽입, 아니라면 변경
                }
                else
                {
                    //OjwGrid_SetMotion();
                }
                m_bMouseMove = false;
            }
            private void CaptionControl(bool bInsert)
            {
                int nLine = m_nCurrntCell;
                int nCharSize = 46;
                string strValue = (bInsert == true) ? "" : GetCaption(nLine);
                string strMessage = (bInsert == true) ? "삽입할 주석의 내용을 입력하시오(한글 " + Ojw.CConvert.IntToStr(nCharSize / 2) + "글자, 영문 " + Ojw.CConvert.IntToStr(nCharSize) + "글자)" : "변경할 주석의 내용을 입력하시오(한글 " + Ojw.CConvert.IntToStr(nCharSize / 2) + "글자, 영문 " + Ojw.CConvert.IntToStr(nCharSize) + "글자)";
                if (Ojw.CInputBox.Show("주석", strMessage, ref strValue) == DialogResult.OK)
                {
                    int nX = dgAngle.CurrentCell.ColumnIndex;
                    int nY = dgAngle.CurrentCell.RowIndex;

                    // 주석용 테이블 삽입
                    if (bInsert == true)
                        Insert(m_nCurrntCell, 1);
                    String strFirst = "", strSecond = "";
                    int nLength = strValue.Length;
                    if (nLength > 0)
                    {
                        if (nLength > nCharSize)
                        {
                            strFirst = strValue.Substring(0, nCharSize);
                            strSecond = strValue.Substring(nCharSize + 1, (((nLength - nCharSize) > nCharSize) ? nCharSize : (nLength - nCharSize)));
                        }
                        else
                        {
                            strFirst = strValue;
                        }
                    }
                    else
                    {
                        strValue = "";
                    }
                    SetCaption(nLine, strValue);
                }
            }
        }
        #endregion GridTable
    }
}
#else
//#define _TEST
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;

namespace OpenJigWare
{
    partial class Ojw
    {
        // Do not use this yet.

        // if you make your class, just write in here
        #region Grid
#if true
        public class CGrid : Form
        {
            public enum EType_t
            {
                _GRID_NORMAL = 0,
                _GRID_BUTTON,
                _GRID_IMAGE,
                _GRID_PROGRESSBAR,
                _GRID_CHECKBOX,
                _GRID_COMBOBOX,
                _COUNT
            }
            private int m_nMaxLine = 1000;
            private DataGridView m_gvGrid = null;//new DataGridView();
            public void Create(Form frm, int nX, int nY, int nWidth, int nHeight, String[] pstrTitles)
            {
                Create(frm, nX, nY, nWidth, nHeight, pstrTitles, null, null);
            }
            public void Create(Form frm, int nX, int nY, int nWidth, int nHeight, String [] pstrTitles, int [] pnWidth, int [] pnType)
            {
                #region Check Exception
                if (pnType == null)
                {
                    pnType = new int[pstrTitles.Length];
                    for (int i = 0; i < pnType.Length; i++) { pnType[i] = (int)EType_t._GRID_NORMAL; }
                }
                if (pnWidth == null)
                {
                    pnWidth = new int[pstrTitles.Length];
                    for (int i = 0; i < pnWidth.Length; i++) { pnWidth[i] = (nWidth - 60) / pnWidth.Length; }
                    //pnWidth = new int[pstrTitles.Length];
                    //for (int i = 0; i < pnWidth.Length; i++) { pnWidth[i] = 80; }
                }
                if (pstrTitles == null)
                {
                    CMessage.Write_Error("No titles in here");
                    return;
                }
                if (pstrTitles.Length != pnWidth.Length)
                {
                    CMessage.Write_Error("mismatch Grid Size");
                    return;
                }
                if (m_gvGrid == null) m_gvGrid = new DataGridView();
                else
                {
                    m_gvGrid.Rows.Clear();
                    m_gvGrid.Columns.Clear();
                }
                #endregion Check Exception
                m_gvGrid.Width = nWidth;
                m_gvGrid.Height = nHeight;

                m_gvGrid.Top = nY;
                m_gvGrid.Left = nX;

                for (int nPos = 0; nPos < pstrTitles.Length; nPos++)
                {
                    if (pnType[nPos] == (int)EType_t._GRID_BUTTON)
                    {
                        //DataGridViewButtonColumn clmBtnTmp = new DataGridViewButtonColumn();
                        COjwCellButton clmBtnTmp = new COjwCellButton();
                        clmBtnTmp.HeaderText = pstrTitles[nPos];
                        clmBtnTmp.SetFont = m_gvGrid.Font; // DisConnect 란 글자가 나타나면 나타나는 글자의 Font 를 결정한다.
                        clmBtnTmp.SetName = "DisConnect"; // DisConnect 란 글자가 나타나면 버튼의 색이 변하도록 한다.
                        clmBtnTmp.SetFontColor = Color.Yellow; // DisConnect 란 글자가 나타나면 변하는 버튼의 글자색을 정한다.
                        clmBtnTmp.SetButtonColor = Color.Blue; // DisConnect 란 글자가 나타나면 변하는 버튼의 바탕색을 정한다.
                        m_gvGrid.Columns.Add(clmBtnTmp);
                    }
                    else if (pnType[nPos] == (int)EType_t._GRID_CHECKBOX)
                    {
                        DataGridViewComboBoxColumn comboCheckColumn = new DataGridViewComboBoxColumn();
                        comboCheckColumn.HeaderText = pstrTitles[nPos];
                        m_gvGrid.Columns.Add(comboCheckColumn);
                    }
                    else if (pnType[nPos] == (int)EType_t._GRID_COMBOBOX)
                    {
                        DataGridViewComboBoxColumn clmComboTmp = new DataGridViewComboBoxColumn();
                        clmComboTmp.HeaderText = pstrTitles[nPos];
                        clmComboTmp.Name = "combo";
                        clmComboTmp.Items.AddRange("Test1", "Test2", "Test3", "Test4", "Test5");
                        m_gvGrid.Columns.Add(clmComboTmp);
                    }
                    else if (pnType[nPos] == (int)EType_t._GRID_PROGRESSBAR)// && (g_nControlMode != (int)EMode._MODE_BLUETOOTH))
                    {
                        //DataGridViewProgressColumn cImImgTmp = new DataGridViewProgressColumn();
                        COjwProgressBarColumn cImImgTmp = new COjwProgressBarColumn();
                        //cImImgTmp.HeaderText = pstrTitles[nPos];
                        //cImImgTmp.DefaultCellStyle.ForeColor = Color.Red; // 글자색 변경 가능(Default = 검은색)
                        cImImgTmp.LowValue = 30; // 30% 이하 값에서는 적색으로 되도록 한다.
                        cImImgTmp.LowColor = Color.Red;
                        m_gvGrid.Columns.Add(cImImgTmp);
                    }
                    else if (pnType[nPos] == (int)EType_t._GRID_IMAGE)
                    {
                        DataGridViewImageColumn cImImgTmp = new DataGridViewImageColumn();
                        cImImgTmp.ImageLayout = DataGridViewImageCellLayout.Stretch;
                        //cImImgTmp.Image = imageList1.Images[0];
                        //cImImgTmp.Image = m_aImage[0];// global::DanceManager.Properties.Resources.Pos_1;
                        m_gvGrid.Columns.Add(cImImgTmp);
                    }
                    else
                    {
                        m_gvGrid.Columns.Add(pstrTitles[nPos], pstrTitles[nPos]);
                    }
                    m_gvGrid.Columns[nPos].Width = pnWidth[nPos];
                    m_gvGrid.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                m_gvGrid.RowPostPaint += new DataGridViewRowPostPaintEventHandler(Grid_RowPostPaint);
                frm.Controls.Add(m_gvGrid);

                Grid_Insert(0, m_nMaxLine);

               
            }
            private void Grid_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
            {
                //if (m_bStart == true) return;
                // RowPointPaint 이벤트핸들러
                // 행헤더 열영역에 행번호를 보여주기 위해 장방형으로 처리



                Rectangle rect = new Rectangle(e.RowBounds.Location.X,
                e.RowBounds.Location.Y,
                30,//m_gvGrid.RowHeadersWidth - 4,
                e.RowBounds.Height + 4);
                // 위에서 생성된 장방형내에 행번호를 보여주고 폰트색상 및 배경을 설정
                TextRenderer.DrawText(e.Graphics,
                    (e.RowIndex + 1).ToString() + ".",
                    m_gvGrid.RowHeadersDefaultCellStyle.Font,
                    rect,
                    dgAngle.RowHeadersDefaultCellStyle.ForeColor,//Color.Red, //m_gvGrid.RowHeadersDefaultCellStyle.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

                //if ((m_gvGrid != null) && (Grid_GetCaption(e.RowIndex) != ""))
                //{
                //    string strValue = Grid_GetCaption(e.RowIndex);
                //    rect.Inflate(70, 0);
                //    if (strValue != "")
                //    {
                //        TextRenderer.DrawText(e.Graphics,
                //             "        " + "                " + strValue,
                //            m_gvGrid.RowHeadersDefaultCellStyle.Font,
                //            rect,
                //            m_gvGrid.RowHeadersDefaultCellStyle.ForeColor,
                //            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                //    }
                //    /*
                //     * <DataGridViewRowPostPaintEventArgs 객체>
                //     * e.Graphics - Graphics객체
                //     * e.RowIndex - 표시중인 행번호 (0부터 시작하기 떄문에 +1필요) 
                //     * e.RowBounds.X 행헤더 열 왼쪽 위 X좌표
                //     * e.RowBounds.Y 행헤더 열 왼쪽 위 Y좌표
                //     * e.RowBounds.Height 행헤더 열높이
                //     * dbView.RowHeadersWidth 행헤더 셀 폭
                //     * dbView.RowHeadersDefaultCellStyle.Font 사용폰트
                //     * dbView.RowHeadersDefaultCellStyle.FontColor 폰트 색상
                //     */
                //}
#if _COLOR_GRID_IN_PAINT
            // Grid 그리기
            if (m_bStart == false) Grid_SetColorGrid(e.RowIndex);            
#endif
            }
            private void Grid_Insert(int nIndex, int nInsertCnt)
            {
                int nErrorNum = 0;
                try
                {
                    if (nIndex < 0) nIndex = 0;

                    nErrorNum++;
                    int nFirst = nIndex;
                    nErrorNum++;
                    m_gvGrid.Rows.Insert(nIndex, nInsertCnt);
                    nErrorNum++;
                    //for (int i = nFirst; i < nFirst + nInsertCnt; i++) Grid_Clear(i);
                    nErrorNum++;
                    int nPos = (m_gvGrid.CurrentCell == null) ? 0 : m_gvGrid.CurrentCell.ColumnIndex;

                    if (m_gvGrid.RowCount > m_nMaxLine)
                    {
                        for (int i = 0; i < ((m_gvGrid.RowCount - 1) - m_nMaxLine); i++)
                            Grid_Delete(m_gvGrid.RowCount - 1, 1);
                    }
                }
                catch (System.Exception e)
                {
                    CMessage.Write_Error("[" + CConvert.IntToStr(nErrorNum) + "]" + e.ToString());
                }
            }

            private void Grid_Add(int nIndex, int nInsertCnt)
            {
                if (nIndex < 0) nIndex = 0;
                nIndex++;
                if (nIndex < m_gvGrid.RowCount - 1)
                {
                    Grid_Insert(nIndex, nInsertCnt);
                    Grid_ChangePos(m_gvGrid, nIndex, m_gvGrid.CurrentCell.ColumnIndex);
                    return;
                }
                int nFirst = nIndex;

                m_gvGrid.Rows.Add(nInsertCnt);
                //for (int i = nFirst; i < nFirst + nInsertCnt; i++) Grid_Clear(i);
                Grid_ChangePos(m_gvGrid, nIndex, m_gvGrid.CurrentCell.ColumnIndex);

                if (m_gvGrid.RowCount > m_nMaxLine)
                {
                    for (int i = 0; i < (m_gvGrid.RowCount - m_nMaxLine); i++)
                        Grid_Delete(m_gvGrid.RowCount - 1, 1);
                }
            }
            private void Grid_ChangePos(DataGridView OjwGrid, int nLine, int nPos)
            {
                if (OjwGrid.Rows[nLine].Cells[nPos].Selected == false) OjwGrid.CurrentCell = OjwGrid.Rows[nLine].Cells[nPos];
            }
            private void Grid_Delete(int nIndex, int nDeleteCnt)
            {
                for (int i = 0; i < nDeleteCnt; i++)
                {
                    if (nIndex < 0) nIndex = 0;
                    m_gvGrid.Rows.RemoveAt(nIndex);
                }
            }
        }

        //////////////////////////////////////
        private class COjwProgressBarColumn : DataGridViewTextBoxColumn
        {
            // Constructer
            public COjwProgressBarColumn()
            {
                this.CellTemplate = new COjwProgressBarCell();
            }

            //Set CellTemplate 
            public override DataGridViewCell CellTemplate
            {
                get
                {
                    return base.CellTemplate;
                }
                set
                {

                    if (!(value is COjwProgressBarCell))
                    {
                        throw new InvalidCastException(
                            "Set COjwProgressBarCell object");
                    }
                    base.CellTemplate = value;
                }
            }

            /// <summary>
            /// ProgressBar Max
            /// </summary>
            public int Maximum
            {
                get
                {
                    return ((COjwProgressBarCell)this.CellTemplate).Maximum;
                }
                set
                {
                    if (this.Maximum == value)
                        return;

                    ((COjwProgressBarCell)this.CellTemplate).Maximum =
                        value;

                    if (this.DataGridView == null)
                        return;
                    int rowCount = this.DataGridView.RowCount;
                    for (int i = 0; i < rowCount; i++)
                    {
                        DataGridViewRow r = this.DataGridView.Rows.SharedRow(i);
                        ((COjwProgressBarCell)r.Cells[this.Index]).Maximum =
                            value;
                    }
                }
            }

            public int LowValue
            {
                get
                {
                    return ((COjwProgressBarCell)this.CellTemplate).LowValue;
                }
                set
                {
                    ((COjwProgressBarCell)this.CellTemplate).LowValue = value;
                }
            }

            public Color LowColor
            {
                get
                {
                    return ((COjwProgressBarCell)this.CellTemplate).LowColor;
                }
                set
                {
                    ((COjwProgressBarCell)this.CellTemplate).LowColor = value;
                }
            }

            /// <summary>
            /// ProgressBar Min
            /// </summary>
            public int Mimimum
            {
                get
                {
                    return ((COjwProgressBarCell)this.CellTemplate).Mimimum;
                }
                set
                {
                    if (this.Mimimum == value)
                        return;

                    ((COjwProgressBarCell)this.CellTemplate).Mimimum =
                        value;

                    if (this.DataGridView == null)
                        return;
                    int rowCount = this.DataGridView.RowCount;
                    for (int i = 0; i < rowCount; i++)
                    {
                        DataGridViewRow r = this.DataGridView.Rows.SharedRow(i);
                        ((COjwProgressBarCell)r.Cells[this.Index]).Mimimum =
                            value;
                    }
                }
            }
        }

        /// <summary>
        /// ProgressBar in DataGridView
        /// </summary>
        private class COjwProgressBarCell : DataGridViewTextBoxCell
        {
            // Constructer
            public COjwProgressBarCell()
            {
                this.maximumValue = 100;
                this.mimimumValue = 0;
                this.LowValue = 0;
                this.LowColor = Color.Red;
            }

            private int maximumValue;
            public int Maximum
            {
                get
                {
                    return this.maximumValue;
                }
                set
                {
                    this.maximumValue = value;
                }
            }

            private int mimimumValue;
            public int Mimimum
            {
                get
                {
                    return this.mimimumValue;
                }
                set
                {
                    this.mimimumValue = value;
                }
            }

            //Set Cell's data type

            public override Type ValueType
            {
                get
                {
                    return typeof(int);
                }
            }

            //Set New Cell's data type
            public override object DefaultNewRowValue
            {
                get
                {
                    return 0;
                }
            }

            //Use Clone Function to add new property 
            public override object Clone()
            {
                COjwProgressBarCell cell =
                    (COjwProgressBarCell)base.Clone();
                cell.Maximum = this.Maximum;
                cell.Mimimum = this.Mimimum;
                cell.LowValue = this.LowValue;
                cell.LowColor = this.LowColor;
                return cell;
            }

            private Color lowcolor;// = Color.Red;
            public Color LowColor
            {
                get
                {
                    return this.lowcolor;
                }
                set
                {
                    this.lowcolor = value;
                }
            }
            private int lowvalue;// = 0;
            public int LowValue
            {
                get
                {
                    return this.lowvalue;
                }
                set
                {
                    this.lowvalue = value;
                }
            }

            protected override void Paint(Graphics graphics,
                Rectangle clipBounds, Rectangle cellBounds,

                int rowIndex, DataGridViewElementStates cellState,
                object value, object formattedValue, string errorText,
                DataGridViewCellStyle cellStyle,
                DataGridViewAdvancedBorderStyle advancedBorderStyle,
                DataGridViewPaintParts paintParts)
            {
                bool bOver = false;
                int intValue = 0;
                if (value is int)
                    intValue = (int)value;
                if (intValue < this.mimimumValue)
                    intValue = this.mimimumValue;
                if (intValue > this.maximumValue)
                {
                    intValue = this.maximumValue;
                    bOver = true;
                }

                double rate = (double)(intValue - this.mimimumValue) /
                    (this.maximumValue - this.mimimumValue);


                if ((paintParts & DataGridViewPaintParts.Border) ==
                    DataGridViewPaintParts.Border)
                {
                    this.PaintBorder(graphics, clipBounds, cellBounds,
                        cellStyle, advancedBorderStyle);
                }


                Rectangle borderRect = this.BorderWidths(advancedBorderStyle);
                Rectangle paintRect = new Rectangle(
                    cellBounds.Left + borderRect.Left,
                    cellBounds.Top + borderRect.Top,
                    cellBounds.Width - borderRect.Right,
                    cellBounds.Height - borderRect.Bottom);


                bool isSelected =
                    (cellState & DataGridViewElementStates.Selected) ==
                    DataGridViewElementStates.Selected;
                Color bkColor;
                if (isSelected &&
                    (paintParts & DataGridViewPaintParts.SelectionBackground) ==
                        DataGridViewPaintParts.SelectionBackground)
                {
                    bkColor = cellStyle.SelectionBackColor;
                }
                else
                {
                    bkColor = cellStyle.BackColor;
                }

                if ((paintParts & DataGridViewPaintParts.Background) ==
                    DataGridViewPaintParts.Background)
                {
                    using (SolidBrush backBrush = new SolidBrush(bkColor))
                    {
                        graphics.FillRectangle(backBrush, paintRect);
                    }
                }


                paintRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
                paintRect.Width -= cellStyle.Padding.Horizontal;
                paintRect.Height -= cellStyle.Padding.Vertical;


                // Drawing point for bar(Kor: 여기가 bar 가 그려지는 곳)
                if ((paintParts & DataGridViewPaintParts.ContentForeground) ==
                    DataGridViewPaintParts.ContentForeground)
                {
                    if (ProgressBarRenderer.IsSupported)
                    {
                        ProgressBarRenderer.DrawHorizontalBar(graphics, paintRect);

                        Rectangle barBounds = new Rectangle(
                            paintRect.Left + 3, paintRect.Top + 3,
                            paintRect.Width - 4, paintRect.Height - 6);
                        barBounds.Width = (int)Math.Round(barBounds.Width * rate);
                        ProgressBarRenderer.DrawHorizontalChunks(graphics, barBounds);

                        if ((barBounds.Width > 0) && (intValue <= this.LowValue)) // 30% 이하 값에서는 칼라를 바꾼다.
                        {
                            Brush brush = new System.Drawing.Drawing2D.LinearGradientBrush(barBounds, Color.White, this.LowColor, 90.0f);
                            graphics.FillRectangle(brush, barBounds);
                        }
                    }
                    else
                    {

                        graphics.FillRectangle(Brushes.White, paintRect);
                        graphics.DrawRectangle(Pens.Black, paintRect);
                        Rectangle barBounds = new Rectangle(
                            paintRect.Left + 1, paintRect.Top + 1,
                            paintRect.Width - 1, paintRect.Height - 1);
                        barBounds.Width = (int)Math.Round(barBounds.Width * rate);
                        graphics.FillRectangle(Brushes.Blue, barBounds);
                    }
                }


                if (this.DataGridView.CurrentCellAddress.X == this.ColumnIndex &&
                    this.DataGridView.CurrentCellAddress.Y == this.RowIndex &&
                    (paintParts & DataGridViewPaintParts.Focus) ==
                        DataGridViewPaintParts.Focus &&
                    this.DataGridView.Focused)
                {

                    Rectangle focusRect = paintRect;
                    focusRect.Inflate(-3, -3);
                    ControlPaint.DrawFocusRectangle(graphics, focusRect);

                }


                if ((paintParts & DataGridViewPaintParts.ContentForeground) ==
                    DataGridViewPaintParts.ContentForeground)
                {



                    TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter;

                    Color fColor = cellStyle.ForeColor;

                    paintRect.Inflate(-2, -2);
                    String txt;// = intValue.ToString() + "%";
                    if (bOver == true)
                    {
                        fColor = Color.Red;
                        txt = "충전";
                    }
                    else
                    {
                        txt = intValue.ToString() + "%";
                    }
                    TextRenderer.DrawText(graphics, txt, cellStyle.Font,
                       paintRect, fColor, flags);
                }


                if ((paintParts & DataGridViewPaintParts.ErrorIcon) ==
                    DataGridViewPaintParts.ErrorIcon &&
                    this.DataGridView.ShowCellErrors &&
                    !string.IsNullOrEmpty(errorText))
                {

                    Rectangle iconBounds = this.GetErrorIconBounds(
                        graphics, cellStyle, rowIndex);
                    iconBounds.Offset(cellBounds.X, cellBounds.Y);

                    this.PaintErrorIcon(graphics, iconBounds, cellBounds, errorText);
                }
            }
        }

        public class COjwCellButton : DataGridViewButtonColumn
        {
            public COjwCellButton()
            {
                this.CellTemplate = new COjwCell();
            }

            public bool IsSetted
            {
                get
                {
                    return ((COjwCell)this.CellTemplate).IsSetted;
                }
            }

            public String SetName
            {
                get
                {
                    return ((COjwCell)this.CellTemplate).SetName;
                }
                set
                {
                    ((COjwCell)this.CellTemplate).SetName = value;
                }
            }

            public Color SetButtonColor
            {
                get
                {
                    return ((COjwCell)this.CellTemplate).SetButtonColor;
                }
                set
                {
                    ((COjwCell)this.CellTemplate).SetButtonColor = value;
                }
            }

            public Color SetFontColor
            {
                get
                {
                    return ((COjwCell)this.CellTemplate).SetFontColor;
                }
                set
                {
                    ((COjwCell)this.CellTemplate).SetFontColor = value;
                }
            }

            public Font SetFont
            {
                get
                {
                    return ((COjwCell)this.CellTemplate).SetFont;
                }
                set
                {
                    ((COjwCell)this.CellTemplate).SetFont = value;
                }
            }
            //public bool SetBold
            //{
            //    get
            //    {
            //        return ((COjwCell)this.CellTemplate).SetBold;
            //    }
            //    set
            //    {
            //        ((COjwCell)this.CellTemplate).SetBold = value;
            //    }
            //}

            //Set CellTemplate 
            public override DataGridViewCell CellTemplate
            {
                get
                {
                    return base.CellTemplate;
                }
                set
                {

                    if (!(value is COjwCell))
                    {
                        throw new InvalidCastException(
                            "Set COjwCell object");
                    }
                    base.CellTemplate = value;
                }
            }
            //protected override void Paint(  
            //                                Graphics graphics, 
            //                                Rectangle clipBounds, Rectangle cellBounds, 
            //                                int rowIndex, DataGridViewElementStates elementState, 
            //                                object value, object formattedValue, string errorText, 
            //                                DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
            //{
            //    graphics.FillRectangle(Brushes.Red, clipBounds);

            //    base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
            //    //ButtonRenderer.DrawButton(graphics, cellBounds, formattedValue.ToString(), new Font("Comic Sans MS", 9.0f, FontStyle.Bold), true, System.Windows.Forms.VisualStyles.PushButtonState.Default);
            //}
        }

        public class COjwCell : DataGridViewButtonCell
        {
            public COjwCell()
            {
                this.m_strSetName = "DisConnect";
                this.m_cSetFontColor = Color.Yellow;
                this.m_cSetButtonColor = Color.Red;
                this.m_bIsSetted = false;
                //this.m_bSetFont = ;
            }
            public override object Clone()
            {
                COjwCell cell = (COjwCell)base.Clone();
                cell.m_strSetName = this.m_strSetName;
                cell.m_cSetFontColor = this.m_cSetFontColor;
                cell.m_cSetButtonColor = this.m_cSetButtonColor;
                cell.m_bIsSetted = this.m_bIsSetted;
                return cell;
            }

            private Font m_fntSetFont;
            public Font SetFont
            {
                get
                {
                    return this.m_fntSetFont;
                }
                set
                {
                    this.m_fntSetFont = value;
                }
            }
            //private bool m_bSetBold;
            //public bool SetBold
            //{
            //    get
            //    {
            //        return this.m_bSetBold;
            //    }
            //    set
            //    {
            //        this.m_bSetBold = value;
            //    }
            //}
            private bool m_bIsSetted;
            public bool IsSetted
            {
                get
                {
                    return this.m_bIsSetted;
                }
            }

            private String m_strSetName;
            public String SetName
            {
                get
                {
                    return this.m_strSetName;
                }
                set
                {
                    this.m_strSetName = value;
                }
            }
            private Color m_cSetButtonColor;
            public Color SetButtonColor
            {
                get
                {
                    return this.m_cSetButtonColor;
                }
                set
                {
                    this.m_cSetButtonColor = value;
                }
            }
            private Color m_cSetFontColor;
            public Color SetFontColor
            {
                get
                {
                    return this.m_cSetFontColor;
                }
                set
                {
                    this.m_cSetFontColor = value;
                }
            }

            //private bool m_bDown = false;
            protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
            {
                //m_bDown = true;
                base.OnMouseDown(e);
            }
            protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
            {
                //m_bDown = false;
                base.OnMouseUp(e);
            }
            protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
            {
                //base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

#if false// 선택이 된 경우 색을 달리하고 싶다면...
            bool isSelected =
                (elementState & DataGridViewElementStates.Selected) ==
                DataGridViewElementStates.Selected;
            Color bkColor;
            if (isSelected &&
                (paintParts & DataGridViewPaintParts.SelectionBackground) ==
                    DataGridViewPaintParts.SelectionBackground)
            {
                bkColor = cellStyle.SelectionBackColor;
            }
            else
            {
                bkColor = cellStyle.BackColor;
            }
#endif
                /*
                if (Value != null)
                {
                    Color bkColor = this.SetButtonColor;// Color.Red;
                    if (Value.ToString() == m_strSetName)
                    {
                        Rectangle borderRect = this.BorderWidths(advancedBorderStyle);
                        Rectangle paintRect = new Rectangle(
                            cellBounds.Left + borderRect.Left,
                            cellBounds.Top + borderRect.Top,
                            cellBounds.Width - borderRect.Right,
                            cellBounds.Height - borderRect.Bottom);
                        paintRect.Offset(cellStyle.Padding.Right, cellStyle.Padding.Top);
                        paintRect.Width -= cellStyle.Padding.Horizontal;
                        paintRect.Height -= cellStyle.Padding.Vertical;
                        paintRect.Inflate(-1, -1);
                        Rectangle paintRect2 = paintRect;
                        if (m_bDown == true) paintRect.Inflate(-1, -1); // 클릭을 해준 경우 그림을 눌린 이벤트를 하는 것처럼 보이게...
                        // 여기가 그려지는 곳
                        if ((paintParts & DataGridViewPaintParts.ContentForeground) ==
                            DataGridViewPaintParts.ContentForeground)
                        {
                            //Brush brush = new System.Drawing.Drawing2D.LinearGradientBrush(cellBounds, Color.Gray, bkColor, 90.0f);
                            //graphics.FillRectangle(brush, cellBounds); 
                            Brush brush = new System.Drawing.Drawing2D.LinearGradientBrush(paintRect, SystemColors.Control, bkColor, 90.0f);
                            graphics.FillRectangle(brush, paintRect2);
                        }
                        // 여기가 글씨가 써지는 곳
                        if ((paintParts & DataGridViewPaintParts.ContentForeground) ==
                            DataGridViewPaintParts.ContentForeground)
                        {
                            TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                                TextFormatFlags.VerticalCenter;
                            Color fColor = this.SetFontColor;//cellStyle.ForeColor;
                            String txt = Value.ToString();
                            //if (this.m_bChecked == true) txt = "DisConnect";
                            //else txt = "Connect";
                            //Font fntValue = new Font(cellStyle.Font.FontFamily.Name, cellStyle.Font.Size, ((m_bSetBold == true) ? FontStyle.Bold : FontStyle.Regular) );
                            TextRenderer.DrawText(graphics, txt, this.SetFont, paintRect, fColor, flags);
                        }
                        m_bIsSetted = true;
                    }
                    else
                    {
                        base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
                        m_bIsSetted = false;
                    }
                }
                 * */
            }

        }
#endif
        #endregion Grid  

#region PropertyGrid
        public class CProperty
        {
            private PropertyGrid m_propGrid = null;//new PropertyGrid();
            //private object m_obj = null;     
            private Control m_pnProp_Selected = null;
            public void Destroy()
            {
                Destroy(m_pnProp_Selected);
            }
            public void Destroy(Control pnProp)
            {
                if (m_propGrid != null)
                {
                    pnProp.Controls.Remove(m_propGrid);
                    m_propGrid.Dispose();
                }
            }
            public void Create(Control ctrlProp, object objectClass)
            {
                m_propGrid = new PropertyGrid();
                m_propGrid.Dock = DockStyle.Fill;
                m_propGrid.Top = 0;
                m_propGrid.Left = 0;
                m_propGrid.Width = ctrlProp.Width;
                m_propGrid.Height = ctrlProp.Height;
                m_propGrid.Name = "pnOjwProp_User00";// "pnOjwProp_" + Ojw.CConvert.IntToStr((int)ctrlProp.Handle);
                //m_propGrid.LineColor = Color.Black;
                m_propGrid.ToolbarVisible = false;
                m_propGrid.LargeButtons = false;

                m_propGrid.PropertySort = PropertySort.Categorized;
                //CProp CProbItem = new CProp();
                //m_obj = objectClass;
                m_propGrid.SelectedObject = objectClass;// m_obj;// CProbItem;
                
                //m_propGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(propGrid_PropertyValueChanged);//SelectedGridItemChangedEventHandler(propertyGrid1_PropertyValueChanged);
                //m_propGrid.Text = "";
                //m_propGrid.TextChanged += new System.EventHandler(m_atxtPos_TextChanged);
                ctrlProp.Controls.Add(m_propGrid);
                m_pnProp_Selected = ctrlProp;

                
                // Event
                //foreach (Control c in m_propGrid.Controls)
                //{
                //    c.Enabled = true;
                //    c.MouseClick += new MouseEventHandler(m_propGrid_MouseClick);
                //}
            }
            //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            //{

            //}

            public void SetEvent_Changed(PropertyValueChangedEventHandler FFunction)
            {
                m_propGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(FFunction);                
            }

            void m_propGrid_MouseClick(object sender, MouseEventArgs e)
            {
                //throw new NotImplementedException();
                //Ojw.ShowKeyPad_Number(sender);
            }
            public void RemoveEvent_Changed(PropertyValueChangedEventHandler FFunction)
            {
                m_propGrid.PropertyValueChanged -= new PropertyValueChangedEventHandler(FFunction);
            }

            public void Update()
            {
                object tmp = m_propGrid.SelectedObject;
                m_propGrid.SelectedObject = null;
                m_propGrid.SelectedObject = tmp;
            }

            private void propGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
            {

                MessageBox.Show(e.ChangedItem.Label + ":" + (String)e.ChangedItem.Value);
            }




        }        
        #endregion PropertyGrid
#region DynamicPropertyGrid
        public class CDynamicProperty
        {
            private PropertyGrid m_propGrid = null;//new PropertyGrid();
            //private object m_obj = null;
            public void Create(Panel pnProp)//, object objectClass)
            {
                m_propGrid = new PropertyGrid();
                m_propGrid.Dock = DockStyle.Fill;
                m_propGrid.Top = 0;
                m_propGrid.Left = 0;
                m_propGrid.Width = pnProp.Width;
                m_propGrid.Height = pnProp.Height;
                m_propGrid.Name = "pnOjwProp_" + Ojw.CConvert.IntToStr((int)pnProp.Handle);
                //m_propGrid.LineColor = Color.Black;
                m_propGrid.ToolbarVisible = false;
                m_propGrid.LargeButtons = false;

                m_propGrid.PropertySort = PropertySort.Categorized;
                //CProp CProbItem = new CProp();
                //m_obj = objectClass;
                m_propGrid.SelectedObject = myProperties;// objectClass;// m_obj;// CProbItem;

                //m_propGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(propGrid_PropertyValueChanged);//SelectedGridItemChangedEventHandler(propertyGrid1_PropertyValueChanged);
                //m_propGrid.Text = "";
                //m_propGrid.TextChanged += new System.EventHandler(m_atxtPos_TextChanged);
                pnProp.Controls.Add(m_propGrid);
            }
            public void SetEvent_Changed(PropertyValueChangedEventHandler FFunction)
            {
                m_propGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(FFunction);
            }
            public void RemoveEvent_Changed(PropertyValueChangedEventHandler FFunction)
            {
                m_propGrid.PropertyValueChanged -= new PropertyValueChangedEventHandler(FFunction);
            }
            
            private CustomClass myProperties = new CustomClass();
            public void Add(String strName, String strGroup, String strCaption, object value, bool bReadOnly, bool bVisible)
            {
                CustomProperty myProp = new CustomProperty(strName, strGroup, strCaption, value, bReadOnly, bVisible);
                myProperties.Add(myProp);
                m_propGrid.Refresh();
            }
            public void Remove(String strName)
            {
                myProperties.Remove(strName);
                m_propGrid.Refresh();
            }
            public void Update()
            {
                //object tmp = m_propGrid.SelectedObject;
                //m_propGrid.SelectedObject = null;
                //m_propGrid.SelectedObject = tmp;
                m_propGrid.Refresh();
            }

            public class CustomClass : CollectionBase, ICustomTypeDescriptor
            {
                /// <summary>
                /// Add CustomProperty to Collectionbase List
                /// </summary>
                /// <param name="Value"></param>
                public void Add(CustomProperty Value)
                {
                    base.List.Add(Value);
                }

                /// <summary>
                /// Remove item from List
                /// </summary>
                /// <param name="Name"></param>
                public void Remove(string Name)
                {
                    foreach (CustomProperty prop in base.List)
                    {
                        if (prop.Name == Name)
                        {
                            base.List.Remove(prop);
                            return;
                        }
                    }
                }

                /// <summary>
                /// Indexer
                /// </summary>
                public CustomProperty this[int index]
                {
                    get
                    {
                        return (CustomProperty)base.List[index];
                    }
                    set
                    {
                        base.List[index] = (CustomProperty)value;
                    }
                }


#region "TypeDescriptor Implementation"
                /// <summary>
                /// Get Class Name
                /// </summary>
                /// <returns>String</returns>
                public String GetClassName()
                {
                    return TypeDescriptor.GetClassName(this, true);
                }

                /// <summary>
                /// GetAttributes
                /// </summary>
                /// <returns>AttributeCollection</returns>
                public AttributeCollection GetAttributes()
                {
                    return TypeDescriptor.GetAttributes(this, true);
                }

                /// <summary>
                /// GetComponentName
                /// </summary>
                /// <returns>String</returns>
                public String GetComponentName()
                {
                    return TypeDescriptor.GetComponentName(this, true);
                }

                /// <summary>
                /// GetConverter
                /// </summary>
                /// <returns>TypeConverter</returns>
                public TypeConverter GetConverter()
                {
                    return TypeDescriptor.GetConverter(this, true);
                }

                /// <summary>
                /// GetDefaultEvent
                /// </summary>
                /// <returns>EventDescriptor</returns>
                public EventDescriptor GetDefaultEvent()
                {
                    return TypeDescriptor.GetDefaultEvent(this, true);
                }
                /// <summary>
                /// GetDefaultProperty
                /// </summary>
                /// <returns>PropertyDescriptor</returns>
                public PropertyDescriptor GetDefaultProperty()
                {
                    return TypeDescriptor.GetDefaultProperty(this, true);
                }
                /// <summary>
                /// GetEditor
                /// </summary>
                /// <param name="editorBaseType">editorBaseType</param>
                /// <returns>object</returns>
                public object GetEditor(Type editorBaseType)
                {
                    return TypeDescriptor.GetEditor(this, editorBaseType, true);
                }
                public EventDescriptorCollection GetEvents(Attribute[] attributes)
                {
                    return TypeDescriptor.GetEvents(this, attributes, true);
                }
                public EventDescriptorCollection GetEvents()
                {
                    return TypeDescriptor.GetEvents(this, true);
                }
                public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
                {
                    PropertyDescriptor[] newProps = new PropertyDescriptor[this.Count];
                    for (int i = 0; i < this.Count; i++)
                    {

                        CustomProperty prop = (CustomProperty)this[i];
                        newProps[i] = new CustomPropertyDescriptor(ref prop, attributes);
                    }

                    return new PropertyDescriptorCollection(newProps);
                }
                public PropertyDescriptorCollection GetProperties()
                {

                    return TypeDescriptor.GetProperties(this, true);

                }
                public object GetPropertyOwner(PropertyDescriptor pd)
                {
                    return this;
                }
                #endregion
            }

            /// <summary>
            /// Custom property class 
            /// </summary>
            public class CustomProperty
            {
                private string sName = string.Empty;
                private string sGroup = string.Empty;
                private string sCaption = string.Empty;
                private bool bReadOnly = false;
                private bool bVisible = true;
                private object objValue = null;
                public CustomProperty(string sName, string strGroup, string strCaption, object value, bool bReadOnly, bool bVisible)
                {
                    this.sName = sName;
                    this.sGroup = strGroup;
                    this.sCaption = strCaption;
                    this.objValue = value;
                    this.bReadOnly = bReadOnly;
                    this.bVisible = bVisible;
                }
                public bool ReadOnly
                {
                    get
                    {
                        return bReadOnly;
                    }
                }
                public string Name
                {
                    get
                    {
                        return sName;
                    }
                }
                public string Group
                {
                    get
                    {
                        return sGroup;
                    }
                }
                public string Caption
                {
                    get
                    {
                        return sCaption;
                    }
                }
                public bool Visible
                {
                    get
                    {
                        return bVisible;
                    }
                }
                public object Value
                {
                    get
                    {
                        return objValue;
                    }
                    set
                    {
                        objValue = value;
                    }
                }
            }

            /// <summary>
            /// Custom PropertyDescriptor
            /// </summary>
            public class CustomPropertyDescriptor : PropertyDescriptor
            {
                CustomProperty m_Property;
                public CustomPropertyDescriptor(ref CustomProperty myProperty, Attribute[] attrs)
                    : base(myProperty.Name, attrs)
                {
                    m_Property = myProperty;
                }

#region PropertyDescriptor specific
                public override bool CanResetValue(object component)
                {
                    return false;
                }
                public override Type ComponentType
                {
                    get
                    {
                        return null;
                    }
                }
                public override object GetValue(object component)
                {
                    return m_Property.Value;
                }
                public override string Description
                {
                    get
                    {
                        return m_Property.Caption;
                    }
                }
                public override string Category
                {
                    get
                    {
                        return m_Property.Group;
                    }
                }
                public override string DisplayName
                {
                    get
                    {
                        return m_Property.Name;
                    }
                }
                public override bool IsReadOnly
                {
                    get
                    {
                        return m_Property.ReadOnly;
                    }
                }
                public override void ResetValue(object component)
                {
                    //Have to implement
                }
                public override bool ShouldSerializeValue(object component)
                {
                    return false;
                }
                public override void SetValue(object component, object value)
                {
                    m_Property.Value = value;
                }
                public override Type PropertyType
                {
                    get { return m_Property.Value.GetType(); }
                }
                #endregion
            }
        }
#endregion DynamicPropertyGrid


#region GridTable
        // 0 - (+), 1 - (-), 2 - mul, 3 - div, 4 - increment, 
        // 5 - decrement, 6 - Change, 7 - Flip Value, 8 - Interpolation, 9 - 'S'Curve, 
        // 10 - Flip Position, 11 - Evd(+), 12 - Evd(-), 13 - EvdSet, 14 - Angle(+), 
        // 15 - Angle(-), 16 - AngleSet, 
        // 17, 18, 19 - Gravity Set(18 - Tilt 만 변화, 19 - Swing 만 변화)
        // 20, 21, 22 - LED Change(20-Red, 21-Green, 22-Blue) - 0 일때 클리어, 1 일때 동작
        // 23 - Motor Enable() - LED 와 동일
        // 24 - MotorType() - LED 와 동일
        // 25 - X(+), 26 - X(-), 27 - Y(+), 28 - (Y-), 29 - Z(+), 30 - Z(-)       
        public enum ECalc_t
        {
            _Plus = 0,
            _Minus,
            _Mul,
            _Div,
            _Inc,
            _Dec,
            _Change,
            _Flip_Value,
            _Interpolation,
            _S_Curve,
            _Flip_Position,
            _11_Reserve,
            _12_Reserve,
            _13_Reserve,
            _14_Reserve,
            _15_Reserve,
            _16_Reserve,
            _17_Reserve,
            _18_Reserve,
            _19_Reserve,
            _20_Reserve,
            _21_Reserve,
            _22_Reserve,
            _23_Reserve,
            _24_Reserve,
            _X_Plus,
            _X_Minus,
            _Y_Plus,
            _Y_Minus,
            _Z_Plus,
            _Z_Minus
        }
        public struct SGridTable_t
        {
            public string strTitle;
            public int nGroupCol;
            public int nWidth;
            // -1 : 미러된 모터 없음(뒤집기 시 값의 변형 없음). 
            // 0 ~ : 미러링된 모터 번호            
            public int nMirror;
            public Color cColor;
            public object InitValue;
            public SGridTable_t(string title, int width, int group, int mirror, Color color, object initvalue) { this.strTitle = title; this.nWidth = width; this.nGroupCol = group; this.nMirror = mirror; this.cColor = color; this.InitValue = initvalue; }
        }
        public struct SGridLineInfo_t
        {
            public bool bEn;
            public int nGroupLine;
            public int nCommand;
            public float fData0;
            public float fData1;
            public float fData2;
            public float fData3;
            public float fData4;
            public float fData5;
            public float fX;
            public float fY;
            public float fZ;
            public float fPan;
            public float fTilt;
            public float fSwing;
            public string strCaption;
            public SGridLineInfo_t(bool enable, int group, int nCommand, float d0, float d1, float d2, float d3, float d4, float d5, string caption, float x, float y, float z, float pan, float tilt, float swing)
            {
                this.bEn = enable;
                this.nGroupLine = group;
                this.strCaption = caption;
                this.nCommand = nCommand;
                this.fData0 = d0;
                this.fData1 = d1;
                this.fData2 = d2;
                this.fData3 = d3;
                this.fData4 = d4;
                this.fData5 = d5;
                this.fX = x;
                this.fY = y;
                this.fZ = z;
                this.fPan = pan;
                this.fTilt = tilt;
                this.fSwing = swing;
            }
        }
        public class CGridView
        {
#region Var
            private int m_nTableCount = 0;
            private int m_nKey = 0;  // 키보드의 이벤트 키값을 기억할 변수
            public bool m_bKey_Ctrl = false;
            public bool m_bKey_Alt = false;
            public bool m_bKey_Shift = false;

            public int m_nCurrntCell = 0;
            public int m_nCurrntColumn = 0;
            private int m_nCnt_Col = 0;
            private SGridTable_t[] m_aSGridTable = null;
            //private List<SGridLineInfo_t> m_lstLineInfo = new List<SGridLineInfo_t>();
            private DataGridView dgAngle;
            private Color[] m_colorColData = new Color[10] {
                                            Color.White,
                                            Color.Thistle,  
                                            Color.LightBlue,
                                            Color.Tan,      
                                            Color.Violet,   
                                            Color.Cyan,     
                                            Color.Orange,   
                                            Color.Pink,     
                                            Color.Blue,     
                                            Color.Coral     
                                        };
            #endregion Var
            private void Events_Set()
            {
                dgAngle.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(dgAngle_RowPostPaint);
                dgAngle.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellEnter);
                dgAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(dgAngle_KeyDown);
                dgAngle.KeyUp += new System.Windows.Forms.KeyEventHandler(dgAngle_KeyUp);
                dgAngle.MouseClick += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseClick);
                dgAngle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDoubleClick);
                dgAngle.MouseDown += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDown);
            
            }
            private void Events_Remove()
            {
                dgAngle.RowPostPaint -= new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(dgAngle_RowPostPaint);
                dgAngle.CellEnter -= new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellEnter);
                dgAngle.KeyDown -= new System.Windows.Forms.KeyEventHandler(dgAngle_KeyDown);
                dgAngle.KeyUp -= new System.Windows.Forms.KeyEventHandler(dgAngle_KeyUp);
                dgAngle.MouseClick -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseClick);
                dgAngle.MouseDoubleClick -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDoubleClick);
                dgAngle.MouseDown -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDown);
            }
            public void Events_Remove_KeyDown() { dgAngle.KeyDown -= new System.Windows.Forms.KeyEventHandler(dgAngle_KeyDown); }
            public void Events_Set_KeyDown() { dgAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(dgAngle_KeyDown); }
            public void Events_Remove_KeyUp() { dgAngle.KeyUp -= new System.Windows.Forms.KeyEventHandler(dgAngle_KeyUp); }
            public void Events_Set_KeyUp() { dgAngle.KeyUp += new System.Windows.Forms.KeyEventHandler(dgAngle_KeyUp); }
            public void Events_Remove_RowPostPaint() { dgAngle.RowPostPaint -= new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(dgAngle_RowPostPaint); }
            public void Events_Set_RowPostPaint() { dgAngle.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(dgAngle_RowPostPaint); }
            public void Events_Remove_CellEnter() { dgAngle.CellEnter -= new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellEnter); }
            public void Events_Set_CellEnter() { dgAngle.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellEnter); }
            public void Events_Remove_MouseClick() { dgAngle.MouseClick -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseClick); }
            public void Events_Set_MouseClick() { dgAngle.MouseClick += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseClick); }
            public void Events_Remove_MouseDoubleClick() { dgAngle.MouseDoubleClick -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDoubleClick); }
            public void Events_Set_MouseDoubleClick() { dgAngle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDoubleClick); }
            public void Events_Remove_MouseDown() { dgAngle.MouseDown -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDown); }
            public void Events_Set_MouseDown() { dgAngle.MouseDown += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDown); }
            public void Events_Remove_MouseMove() { dgAngle.MouseMove -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseMove); }
            public void Events_Set_MouseMove() { dgAngle.MouseMove += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseMove); }
            public void Events_Remove_MouseUp() { dgAngle.MouseUp -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseUp); }
            public void Events_Set_MouseUp() { dgAngle.MouseUp += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseUp); }
            public CGridView()
            {
                dgAngle = new DataGridView();
                m_nCnt_Col = 0;
                //m_lstLineInfo.Clear();
            }
            ~CGridView()
            {
                Events_Remove();
            }
            public int GetTableCount() { return m_nTableCount; }
            public int GetLineCount() { return dgAngle.RowCount; }
            //public void ReCreate()
            //{
            //    Create(dgAngle, m_nLineCnt, m_aSGridTable);
            //}

            private static readonly int m_nSize_Time = 1;
            private static readonly int m_nSize_Delay = 1;
            private static readonly int m_nSize_Offset_Command = 1;
            private static readonly int m_nSize_Offset_Data = 6;
            private static readonly int m_nSize_Offset_Group = 1;
            private static readonly int m_nSize_Offset_Caption = 1;
            private static readonly int m_nSize_Offset_Trans = 3;
            private static readonly int m_nSize_Offset_Rot = 3;
            private static readonly int m_nSize_Offset = m_nSize_Time + m_nSize_Delay +
                m_nSize_Offset_Command +
                m_nSize_Offset_Data +
                m_nSize_Offset_Group +
                m_nSize_Offset_Caption +
                m_nSize_Offset_Trans +
                m_nSize_Offset_Rot;
            private static readonly int m_nPor_Dummy = 0;//1;
            private static readonly int m_nPos_Time = m_nSize_Offset - m_nPor_Dummy;
            private static readonly int m_nPos_Delay = m_nSize_Offset - (m_nSize_Time + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Command = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data0 = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data1 = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 1 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data2 = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 2 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data3 = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 3 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data4 = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 4 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Data5 = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + 5 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Group = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Caption = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Trans = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Trans_X = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + 0 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Trans_Y = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + 1 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Trans_Z = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + 2 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Rot = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + m_nSize_Offset_Trans + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Rot_Pan = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + m_nSize_Offset_Trans + 0 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Rot_Tilt = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + m_nSize_Offset_Trans + 1 + m_nPor_Dummy);
            private static readonly int m_nPos_Offset_Rot_Swing = m_nSize_Offset - (m_nSize_Time + m_nSize_Delay + m_nSize_Offset_Command + m_nSize_Offset_Data + m_nSize_Offset_Group + m_nSize_Offset_Caption + m_nSize_Offset_Trans + 2 + m_nPor_Dummy);

            private int m_nLineCnt = 0;
            public void Create(DataGridView dg, int nLineCnt, params SGridTable_t[] aSGridTable)
            {
                Events_Remove();
                m_nLineCnt = nLineCnt;
                int nOffsetSize = 9;
                int i;
                dgAngle = dg;
                m_aSGridTable = aSGridTable;
                m_nCnt_Col = aSGridTable.Length + nOffsetSize;

                m_nTableCount = aSGridTable.Length;

                int nPos = 0;
                int nWidth = 0;
                int nWidth_Offset = 0;

                // DataGridView 컨트롤의 SelectionMode가 ColumnHeaderSelect(으)로 설정되어 있으면 열의 SortMode를 Automatic(으)로 설정할 수 없습니다.
                dgAngle.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
                dgAngle.Rows.Clear();
                //m_lstLineInfo.Clear();
                ///////////////

                dgAngle.Columns.Clear();
                dgAngle.AllowUserToAddRows = false; // 자동추가 방지
                dgAngle.RowHeadersWidth = 150;
                dgAngle.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                
                // 0 - En
                dgAngle.Columns.Add("En", "En");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = 20;// 30;
                nWidth += dgAngle.Columns[nPos++].Width + nWidth_Offset;

                for (i = 0; i < m_nCnt_Col - nOffsetSize; i++)
                {
                    if (aSGridTable[i].strTitle != null) dgAngle.Columns.Add(("Mot" + i.ToString()), aSGridTable[i].strTitle);
                    else dgAngle.Columns.Add("Mot" + i.ToString(), "Mot" + i.ToString());
                    dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                    //dgAngle.Columns[nPos].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;//DataGridViewCellStyle { }

                    dgAngle.Columns[nPos].Width = aSGridTable[i].nWidth;
                    nWidth += dgAngle.Columns[nPos++].Width + nWidth_Offset;
                }

                //// 1 - Time
                //dgAngle.Columns.Add("Time", "Time");
                //dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                //dgAngle.Columns[nPos].Width = 10;
                //dgAngle.Columns[nPos++].Visible = true;

                //// 2 - Delay
                //dgAngle.Columns.Add("Delay", "Delay");
                //dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                //dgAngle.Columns[nPos].Width = 10;
                //dgAngle.Columns[nPos++].Visible = true;

                // 3 - Command
                dgAngle.Columns.Add("Command", "Command");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = 10;
                dgAngle.Columns[nPos++].Visible = false;

                i = 0;
                for (i = 0; i < 6; i++)
                {
                    // 4 ~ 9 - Data0 ~ Data5
                    dgAngle.Columns.Add("Data" + i.ToString(), "Data" + i.ToString());
                    dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgAngle.Columns[nPos].Width = 10;
                    dgAngle.Columns[nPos++].Visible = false;
                }
                // 10 - Group
                dgAngle.Columns.Add("Group", "Group");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = 10;
                dgAngle.Columns[nPos++].Visible = false;
                // 11 - Caption
                dgAngle.Columns.Add("Caption", "Caption");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = 10;
                dgAngle.Columns[nPos++].Visible = false;

                // 12 ~ 14 - Translate(X, Y, Z)
                string[] pstrTrans = new string[3] { "X", "Y", "Z" };
                for (i = 0; i < 3; i++)
                {
                    dgAngle.Columns.Add(pstrTrans[i], pstrTrans[i]);
                    dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgAngle.Columns[nPos].Width = 10;
                    dgAngle.Columns[nPos++].Visible = false;
                }
                // 15 ~ 17 - Rotate(Pan, tilt, Swing)
                string[] pstrRot = new string[3] { "Pan", "Tilt", "Swing" };
                for (i = 0; i < 3; i++)
                {
                    dgAngle.Columns.Add(pstrRot[i], pstrRot[i]);
                    dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgAngle.Columns[nPos].Width = 10;
                    dgAngle.Columns[nPos++].Visible = false;
                }
                if (nLineCnt > 0) Insert(0, nLineCnt);
                //return dgAngle.Width;

                Events_Set();
            }
            private void dgAngle_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
            {
                //if (m_bStart == true) return;
                // RowPointPaint 이벤트핸들러
                // 행헤더 열영역에 행번호를 보여주기 위해 장방형으로 처리
                
                Rectangle rect = new Rectangle(e.RowBounds.Location.X,
                e.RowBounds.Location.Y,
                30,//dgAngle.RowHeadersWidth - 4,
                e.RowBounds.Height + 4);
                // 위에서 생성된 장방형내에 행번호를 보여주고 폰트색상 및 배경을 설정
                TextRenderer.DrawText(e.Graphics,
                    (e.RowIndex + 1).ToString() + ":",
                    dgAngle.RowHeadersDefaultCellStyle.Font,
                    rect,
                    dgAngle.RowHeadersDefaultCellStyle.ForeColor,//Color.Red, //dgAngle.RowHeadersDefaultCellStyle.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

                if ((dgAngle != null) && (GetCaption(e.RowIndex) != ""))
                {
                    string strValue = GetCaption(e.RowIndex);
                    //rect.Inflate(70, 0);
                    //rect.Inflate(500, 0);
                    int nOffset = 40;
                    rect = new Rectangle(e.RowBounds.Location.X + nOffset,
                    e.RowBounds.Location.Y,
                    dgAngle.RowHeadersWidth - nOffset - 4, //200,//dgAngle.RowHeadersWidth - 4,
                    e.RowBounds.Height + 4);
                    if (strValue != "")
                    {
                        TextRenderer.DrawText(e.Graphics,
                             //"        " + "               " + 
                             strValue,
                            dgAngle.RowHeadersDefaultCellStyle.Font,
                            rect,
                            dgAngle.RowHeadersDefaultCellStyle.ForeColor,
                            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                    }
                    /*
                     * <DataGridViewRowPostPaintEventArgs 객체>
                     * e.Graphics - Graphics객체
                     * e.RowIndex - 표시중인 행번호 (0부터 시작하기 떄문에 +1필요) 
                     * e.RowBounds.X 행헤더 열 왼쪽 위 X좌표
                     * e.RowBounds.Y 행헤더 열 왼쪽 위 Y좌표
                     * e.RowBounds.Height 행헤더 열높이
                     * dbView.RowHeadersWidth 행헤더 셀 폭
                     * dbView.RowHeadersDefaultCellStyle.Font 사용폰트
                     * dbView.RowHeadersDefaultCellStyle.FontColor 폰트 색상
                     */
                }
            }
            ////private int m_nLine_GroupNum
            //public void SetGroup_Line(int nLine, int nValue)
            //{
            //    m_lstLineInfo[nLine].nGroupLine = nValue;
            //    m_lstLineInfo[nLine].nGroupLine = nValue;
            //    //m_aSGridTable[nLine].n
            //}
            //private int Grid_GetGroup(int nLine)
            //{
            //    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;
            //    int nRet = 0;
            //    try
            //    {
            //        int nPos = Grid_GetIndex_byMotorAxis(m_C3d.m_CHeader.nMotorCnt - 1);
            //        if (nPos > 0) nRet = (int)Ojw.CConvertFunction.Ojw.CConvert.StrToFloat(dgAngle.Rows[nLine].Cells[nPos + 3].Value.ToString());
            //        return nRet;
            //    }
            //    catch //(System.Exception e)
            //    {
            //        return 0;
            //    }
            //}
            //private void Grid_SetColorGrid(int nIndex)
            //{
            //    if ((nIndex < 0) || (dgAngle.RowCount < nIndex)) return;

            //    int nColorIndex = 0;
            //    for (int j = 0; j < dgAngle.ColumnCount - 1; j++)
            //    {
            //        nColorIndex = 0; // White

            //        int nAxis = j;// Grid_GetAxisNum_byIndex(j);
            //        if ((nAxis >= 0) && (nAxis < m_nCnt_Col))
            //        {
            //            if ((m_aSGridTable[nAxis].nGroup != 0) && (m_aSGridTable[nAxis].nGroup != nColorIndex))
            //            {
            //                nColorIndex = m_aSGridTable[nAxis].nGroup;
            //            }
            //        }
            //        else
            //            return;

            //        Color cColor = m_colorData[nColorIndex];
            //        // Group
            //        int nGroup = Grid_GetGroup(nIndex);
            //        if (nGroup > 0)
            //        {
            //            //Color cColorGroup = Color.Gray;
            //            int nR = 0, nG = 0, nB = 0;
            //            int nColorValue = -50;
            //            if (nGroup == 1) { nR = nColorValue; nG = 0; nB = 0; }
            //            if (nGroup == 2) { nR = 0; nG = nColorValue; nB = 0; }
            //            if (nGroup == 3) { nR = 0; nG = 0; nB = nColorValue; }
            //            cColor = Color.FromArgb(Ojw.CConvert.Clip(255, 0, cColor.R + nR), Ojw.CConvert.Clip(255, 0, cColor.G + nG), Ojw.CConvert.Clip(255, 0, cColor.B + nB));
            //        }
            //        if (dgAngle[j, nIndex].Style.BackColor != m_colorData[nColorIndex])
            //            dgAngle[j, nIndex].Style.BackColor = cColor;
            //    }
            //}
            //private void Grid_SetColorGrid(int nIndex)
            //{
            //    if ((nIndex < 0) || (dgAngle.RowCount < nIndex)) return;
                
            //    int nColorIndex = 0;
            //    for (int j = 0; j < dgAngle.ColumnCount - 1; j++)
            //    {
            //        nColorIndex = 0; // White

            //        int nAxis = j;// Grid_GetAxisNum_byIndex(j);
                    
            //        if ((m_aSGridTable[nAxis].nGroup != 0) && (m_aSGridTable[nAxis].nGroup != nColorIndex))
            //            nColorIndex = m_aSGridTable[nAxis].nGroup;

            //        for (int i = nIndex; i < nIndex; i++)
            //        {                        
            //            Color cColor = m_colorData[nColorIndex];

            //            // Group
            //            int nGroup = Grid_GetGroup(i);
            //            if (nGroup > 0)
            //            {
            //                //Color cColorGroup = Color.Gray;
            //                int nR = 0, nG = 0, nB = 0;
            //                int nColorValue = -50;
            //                if (nGroup == 1) { nR = nColorValue; nG = 0; nB = 0; }
            //                if (nGroup == 2) { nR = 0; nG = nColorValue; nB = 0; }
            //                if (nGroup == 3) { nR = 0; nG = 0; nB = nColorValue; }
            //                cColor = Color.FromArgb(Ojw.CConvert.Clip(255, 0, cColor.R + nR), Ojw.CConvert.Clip(255, 0, cColor.G + nG), Ojw.CConvert.Clip(255, 0, cColor.B + nB));
            //            }
            //            if (dgAngle[j, i].Style.BackColor != cColor) dgAngle[j, i].Style.BackColor = cColor;
            //        }
            //    }
            //}
            public void Delete() { dgAngle.Rows.Clear(); }
            public void Delete(int nDeleteCnt) { Delete(m_nCurrntCell, nDeleteCnt); }
            public void Delete(int nIndex, int nDeleteCnt)
            {
                for (int i = 0; i < nDeleteCnt; i++)
                {
                    if (nIndex < 0) nIndex = 0;
                                        
                    dgAngle.Rows.RemoveAt(nIndex);
                    //m_lstLineInfo.RemoveAt(nIndex);
                }
            }
            public void Insert(int nInsertCnt) { Insert(m_nCurrntCell, nInsertCnt); }
            public void Insert(int nIndex, int nInsertCnt)
            {
                int nErrorNum = 0;
                try
                {
                    if (nIndex < 0) nIndex = 0;
                    
                    int nFirst = nIndex;
                    dgAngle.Rows.Insert(nIndex, nInsertCnt);
                    //m_lstLineInfo.Insert(nIndex, new SGridLineInfo_t(false, 0, "", 0, 0, 0, 0, 0, 0));
                    for (int i = nFirst; i < nFirst + nInsertCnt; i++) Clear(i);
                    SetColorGrid(nIndex, nInsertCnt); 
                }
                catch (System.Exception e)
                {
                    Ojw.CMessage.Write_Error("[" + Ojw.CConvert.IntToStr(nErrorNum) + "]" + e.ToString());
                }
            }
            public void Add(int nInsertCnt) { Add(((dgAngle.RowCount == 0) ? 0 : dgAngle.RowCount - 1), nInsertCnt); }
            public void Add(int nIndex, int nInsertCnt)
            {
                if (nIndex < 0) nIndex = 0;

                if (dgAngle.RowCount > 0) nIndex++;
                else nIndex = 0;

                if (nIndex < dgAngle.RowCount - 1)
                {
                    Insert(nIndex, nInsertCnt);
                    ChangePos(dgAngle, nIndex, dgAngle.CurrentCell.ColumnIndex);
                    return;
                }
                int nFirst = nIndex;

                dgAngle.Rows.Add(nInsertCnt);
                ////////////////
                for (int i = nFirst; i < nFirst + nInsertCnt; i++) Clear(i);
                ChangePos(dgAngle, nIndex, dgAngle.CurrentCell.ColumnIndex);
                SetColorGrid(nIndex, nInsertCnt); 
                ////////////////
            }
            public void ChangePos(DataGridView OjwGrid, int nLine, int nPos) { OjwGrid.FirstDisplayedCell = OjwGrid.Rows[nLine].Cells[nPos]; }
            public void Clear(int nLine)
            {
#if false
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle[0, nLine].Value = 0;
                for (int i = 1; i < dgAngle.ColumnCount - 1; i++)
                {
                    if (((i - 1) >= 0) && ((i - 1) < m_aSGridTable.Length))
                        dgAngle.Rows[nLine].Cells[i].Value = m_aSGridTable[i - 1].InitValue;
                    else
                        dgAngle.Rows[nLine].Cells[i].Value = 0;
                }
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 1].Value = String.Empty;

                //Grid_DisplayLine(nLine, false);
#else
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle[0, nLine].Value = 0;
                int nOffset = 1;
                for (int i = nOffset; i < dgAngle.ColumnCount - nOffset; i++)
                {
                    //if (i == dgAngle.ColumnCount - nOffset - m_nPos_Offset_Caption) { dgAngle.Rows[nLine].Cells[i].Value = "TEST"; continue; }
                    if (((i - nOffset) >= 0) && ((i - nOffset) < m_aSGridTable.Length))
                        dgAngle.Rows[nLine].Cells[i].Value = m_aSGridTable[i - nOffset].InitValue;
                    else
                    {
                        if (i == dgAngle.ColumnCount - m_nPos_Offset_Caption) dgAngle.Rows[nLine].Cells[i].Value = "";//"TEST";
                        else dgAngle.Rows[nLine].Cells[i].Value = 0;
                    }
                }
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - nOffset].Value = String.Empty;

                //Grid_DisplayLine(nLine, false);
#endif
            }
            // 그리드의 라인값을 초기화
            public void Clear()
            {
                for (int i = 0; i < dgAngle.RowCount; i++) Clear(i);
            }
            public int OjwGrid_GetCurrentLine() { return m_nCurrntCell; }
            public int OjwGrid_GetCurrentColumn() { return m_nCurrntColumn; }
            private bool m_bIgnore_CellEnter = false;
            
            public void Ignore_CellEnter(bool bIgnore)
            {
                m_bIgnore_CellEnter = bIgnore;
            }
            private void OjwGrid_CellEnter(DataGridView dgGrid, DataGridViewCellEventArgs e)
            {
                if (m_bIgnore_CellEnter == false)
                {
                    //if (m_bStart == true) return; // 속도의 버벅거림이 없게하기 위해 모션 수행시 이 함수를 실행하지 못하도록 한다.

                    //if ((dgGrid.CurrentCell.ColumnIndex == m_nCurrntColumn) && (dgGrid.CurrentCell.RowIndex == m_nCurrntCell)) return;
                    m_nCurrntColumn = dgGrid.CurrentCell.ColumnIndex;
                    if (dgGrid.Focused == true)
                    {
                        m_nCurrntCell = dgGrid.CurrentCell.RowIndex;

                        //Grid_DisplayLine(m_nCurrntCell);
                        //CheckFlag(m_nCurrntCell);
                        //lbMotion_Message.Text = "CellEnter=" + Ojw.CConvert.IntToStr(dgGrid.CurrentCell.RowIndex);
                    }
                }
            }
            public void SetChangeCurrentLine(int nLine)
            {
                m_nCurrntCell = nLine;
            }
            public void SetChangeCurrentCol(int nColumn)
            {
                m_nCurrntColumn = nColumn;
            }

#region Control Data
            public void SetEnable(bool data) { SetEnable(m_nCurrntCell, data); }
            public void SetEnable(int nLine, bool bEn)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[0].Value = Ojw.CConvert.BoolToInt(bEn);
            }
            public bool GetEnable() { return GetEnable(m_nCurrntCell); }
            public bool GetEnable(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return false;
                    return Ojw.CConvert.StrToBool(dgAngle.Rows[nLine].Cells[0].Value.ToString());
                }
                catch// (System.Exception e)
                {
                    return false;
                }
            }
            public void Set(int nNum, float data) { Set(m_nCurrntCell, nNum, data); }
            public void Set(int nLine, int nNum, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[nNum + 1].Value = data;
            }
            public float Get(int nNum) { return Get(m_nCurrntCell, nNum); }
            public float Get(int nLine, int nNum)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return Convert.ToSingle(dgAngle.Rows[nLine].Cells[nNum + 1].Value);//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;// false;
                }
            }
            public void SetTime(object data) { SetData0(m_nCurrntCell, data); }
            public void SetTime(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Time].Value = data;
            }
            public object GetTime() { return GetData0(m_nCurrntCell); }
            public object GetTime(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Time].Value;//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;// false;
                }
            }
            public void SetDelay(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Delay].Value = data;
            }
            public object GetDelay() { return GetData0(m_nCurrntCell); }
            public object GetDelay(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Delay].Value;//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;// false;
                }
            }
            public void SetCommand(object data) { SetData0(m_nCurrntCell, data); }
            public void SetCommand(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Command].Value = data;
            }
            public object GetCommand() { return GetData0(m_nCurrntCell); }
            public object GetCommand(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Command].Value;//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData0(object data) { SetData0(m_nCurrntCell, data); }
            public void SetData0(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data0].Value = data;
            }
            public object GetData0() { return GetData0(m_nCurrntCell); }
            public object GetData0(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data0].Value;//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData1(object data) { SetData1(m_nCurrntCell, data); }
            public void SetData1(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data1].Value = data;
            }
            public object GetData1() { return GetData1(m_nCurrntCell); }
            public object GetData1(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data1].Value;//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData2(object data) { SetData2(m_nCurrntCell, data); }
            public void SetData2(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data2].Value = data;
            }
            public object GetData2() { return GetData2(m_nCurrntCell); }
            public object GetData2(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data2].Value;//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData3(object data) { SetData3(m_nCurrntCell, data); }
            public void SetData3(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data3].Value = data;
            }
            public object GetData3() { return GetData3(m_nCurrntCell); }
            public object GetData3(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data3].Value;//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData4(object data) { SetData4(m_nCurrntCell, data); }
            public void SetData4(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data4].Value = data;
            }
            public object GetData4() { return GetData4(m_nCurrntCell); }
            public object GetData4(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data4].Value;//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetData5(object data) { SetData5(m_nCurrntCell, data); }
            public void SetData5(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data5].Value = data;
            }
            public object GetData5() { return GetData5(m_nCurrntCell); }
            public object GetData5(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;// false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Data5].Value;//.ToString();
                }
                catch// (System.Exception e)
                {
                    return 0;//false;
                }
            }
            public void SetGroup(int data) { SetGroup(m_nCurrntCell, data); }
            public void SetGroup(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Group].Value = data;
            }
            public void SetSelectedGroup(int nGroupNum) // 0 - Clear, 1 - Group1, 2 - Group2, 3 - Group3
            {
                // 0~3 까지 데이타 클리핑
                int nCmd = Ojw.CConvert.Clip(3, 0, nGroupNum);

                int nX_Limit = dgAngle.RowCount;
                int nY_Limit = dgAngle.ColumnCount;
                //bool bRepeat = false;

                for (int i = 0; i < nX_Limit; i++)
                {
                    for (int j = 0; j < nY_Limit; j++)
                    {
                        if (dgAngle[j, i].Selected == true)
                        {
                            SetGroup(i, nCmd);
                            break;
                        }
                    }
                }

                // 색칠하기...
                SetColorGrid(0, dgAngle.RowCount);
            }
            
            public int GetGroup() { return GetGroup(m_nCurrntCell); }
            public int GetGroup(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Group].Value);
                }
                catch// (System.Exception e)
                {
                    return 0;
                }
            }
            public void SetCaption(String data) { SetCaption(m_nCurrntCell, data); }
            public void SetCaption(int nLine, String strValue)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Caption].Value = strValue;
            }
            public String GetCaption() { return GetCaption(m_nCurrntCell); }
            public String GetCaption(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return "";
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Caption].Value.ToString();
                }
                catch// (System.Exception e)
                {
                    return "";
                }
            }

#region Trans / Rot(Offset)
            public void SetOffset_Trans_X(float fX) { SetOffset_Trans_X(m_nCurrntCell, fX); }
            public void SetOffset_Trans_X(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_X].Value = data;
            }
            public float GetOffset_Trans_X() { return GetOffset_Trans_X(m_nCurrntCell); }
            public float GetOffset_Trans_X(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_X].Value);
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }
            public void SetOffset_Trans_Y(float fX) { SetOffset_Trans_Y(m_nCurrntCell, fX); }
            public void SetOffset_Trans_Y(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_Y].Value = data;
            }
            public float GetOffset_Trans_Y() { return GetOffset_Trans_Y(m_nCurrntCell); }
            public float GetOffset_Trans_Y(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_Y].Value);
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }
            public void SetOffset_Trans_Z(float fX) { SetOffset_Trans_Z(m_nCurrntCell, fX); }
            public void SetOffset_Trans_Z(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_Z].Value = data;
            }
            public float GetOffset_Trans_Z() { return GetOffset_Trans_Z(m_nCurrntCell); }
            public float GetOffset_Trans_Z(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Trans_Z].Value);
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }
            
            public void SetOffset_Rot_Pan(float fX) { SetOffset_Rot_Pan(m_nCurrntCell, fX); }
            public void SetOffset_Rot_Pan(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot].Value = data;
            }
            public float GetOffset_Rot_Pan() { return GetOffset_Rot_Pan(m_nCurrntCell); }
            public float GetOffset_Rot_Pan(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Pan].Value);
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }
            public void SetOffset_Rot_Tilt(float fX) { SetOffset_Rot_Tilt(m_nCurrntCell, fX); }
            public void SetOffset_Rot_Tilt(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Tilt].Value = data;
            }
            public float GetOffset_Rot_Tilt() { return GetOffset_Rot_Tilt(m_nCurrntCell); }
            public float GetOffset_Rot_Tilt(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Tilt].Value);
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }
            public void SetOffset_Rot_Swing(float fX) { SetOffset_Rot_Swing(m_nCurrntCell, fX); }
            public void SetOffset_Rot_Swing(int nLine, float data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Swing].Value = data;
            }
            public float GetOffset_Rot_Swing() { return GetOffset_Rot_Swing(m_nCurrntCell); }
            public float GetOffset_Rot_Swing(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0.0f;
                    return Convert.ToSingle(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - m_nPos_Offset_Rot_Swing].Value);
                }
                catch// (System.Exception e)
                {
                    return 0.0f;
                }
            }
#endregion Trans / Rot(Offset)

            public void SetColorGrid(int nIndex, int nCount)
            {
                if ((nIndex < 0) || (dgAngle.RowCount < nIndex)) return;
                if (((nIndex + nCount) < 0) || (dgAngle.RowCount < (nIndex + nCount))) return;

                //Color[] colorData = new Color[10] {
                //                            Color.White,
                //                            Color.Thistle,     // Index Color
                //                            Color.LightBlue,//Turquoise,    // Normal Color
                //                            Color.Tan,    // Left Front
                //                            Color.Violet,   // Right Front
                //                            Color.Cyan,     // Left Rear
                //                            Color.Orange,   // Right Rear
                //                            Color.Pink,     // Head
                //                            Color.Blue,     // Tail
                //                            Color.Coral     // Speed, Delay
                //                        };
                //int nColorIndex = 0;
                //for (int j = 1; j < dgAngle.ColumnCount - (9 - 1); j++)
                for (int j = 0; j < dgAngle.ColumnCount - (m_nPos_Delay - 1); j++)
                {
                    //nColorIndex = 0; // White

                    int nAxis = j - 1;// Grid_GetAxisNum_byIndex(j);
                    //if ((j >= 0) && (j < m_aSGridTable.Length))
                    //{
                    //    if (m_aSGridTable[j].cColor != null)
                    //    {
                    //        nColorIndex = m_aSGridTable[j].nGroupCol;
                    //    }
                    //}
                    //else if (
                    //                (j == (m_C3d.m_CHeader.nMotorCnt + 1)) || // Speed
                    //                (j == (m_C3d.m_CHeader.nMotorCnt + 2))    // Delay
                    //    )
                    //{
                    //    nColorIndex = 9;
                    //}

                    for (int i = nIndex; i < nIndex + nCount; i++)
                    {
                        nAxis = j - 1;// Grid_GetAxisNum_byIndex(j);
                        //int nAddColor = 0;// 30;

                        Color cColor;// = colorData[nColorIndex];
                        //if (m_aSGridTable[j].cColor != null)
                        //{
#if false
                        int nData = 0;
                        nAddColor = 30;
                        int nAdd_R = nAddColor;
                        int nAdd_G = nAddColor;
                        int nAdd_B = nAddColor;
                        cColor = m_aSGridTable[nAxis].cColor;
                        if (m_aSGridTable[nAxis].cColor.R > (255 - nAddColor)) nAdd_R = -nAddColor;
                        if (m_aSGridTable[nAxis].cColor.G > (255 - nAddColor)) nAdd_G = -nAddColor;
                        if (m_aSGridTable[nAxis].cColor.B > (255 - nAddColor)) nAdd_B = -nAddColor;
                        //nAddColor += nData;
                        //cColor = Color.FromArgb(
                        //                        Ojw.CConvert.Clip(255, 0, (int)(m_aSGridTable[nAxis].cColor.R + nAdd_R)),
                        //                        Ojw.CConvert.Clip(255, 0, (int)(m_aSGridTable[nAxis].cColor.G + nAdd_G)),
                        //                        Ojw.CConvert.Clip(255, 0, (int)(m_aSGridTable[nAxis].cColor.B + nAdd_B))
                        //                        );
#else
                        cColor = (nAxis >= 0) ? m_aSGridTable[nAxis].cColor : Color.White;
#endif
                        //}
                        //else
                        //{
                        //    cColor = Color.FromArgb(
                        //                            Ojw.CConvert.Clip(255, 0, (int)(colorData[nColorIndex].R + nAddColor)),
                        //                            Ojw.CConvert.Clip(255, 0, (int)(colorData[nColorIndex].G + nAddColor)),
                        //                            Ojw.CConvert.Clip(255, 0, (int)(colorData[nColorIndex].B + nAddColor))
                        //                            );
                        //}

                        //cColor = Color.FromArgb(
                        //                            Ojw.CConvert.Clip(255, 0, (int)(colorData[nColorIndex].R + nAddColor)),
                        //                            Ojw.CConvert.Clip(255, 0, (int)(colorData[nColorIndex].G + nAddColor)),
                        //                            Ojw.CConvert.Clip(255, 0, (int)(colorData[nColorIndex].B + nAddColor))
                        //                            );

                        // Group
                        int nGroup = GetGroup(i);
                        if (nGroup > 0)
                        {
                            int nR = 0, nG = 0, nB = 0;
                            int nColorValue = -60;
                            if (nGroup == 1) { nR = (((cColor.R + nColorValue) < 0) ? -nColorValue : nColorValue); }
                            if (nGroup == 2) { nG = (((cColor.G + nColorValue) < 0) ? -nColorValue : nColorValue); }
                            if (nGroup == 3) { nB = (((cColor.B + nColorValue) < 0) ? -nColorValue : nColorValue); }
                            //cColor = Color.FromArgb(Ojw.CConvert.Clip(255, 0, cColor.R + nR), Ojw.CConvert.Clip(255, 0, cColor.G + nG), Ojw.CConvert.Clip(255, 0, cColor.B + nB));
                            cColor = Color.FromArgb(
                                Ojw.CConvert.Clip(255, 0, cColor.R + nR), 
                                Ojw.CConvert.Clip(255, 0, cColor.G + nG), 
                                Ojw.CConvert.Clip(255, 0, cColor.B + nB)
                                );
                        }
                        if (dgAngle[j, i].Style.BackColor != cColor)//colorData[nColorIndex])
                            dgAngle[j, i].Style.BackColor = cColor;
                    }
                }
            }

            public void SetData(int nLine, int nNum, object value)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[nNum + 1].Value = value;
            }
            public object GetData(int nLine, int nNum) { return dgAngle.Rows[nLine].Cells[nNum + 1].Value; }
            
#endregion Control Data
            //private int m_nCurrentLine = -1;
            //private int m_nCurrentCol = -1;
            private void dgAngle_CellEnter(object sender, DataGridViewCellEventArgs e)
            {
                if (m_bIgnore_CellEnter == false)
                {
                    if (dgAngle.Focused == true)// || (m_bStart == true))
                    {
                        //m_bClick_dbAngle = true;
                        OjwGrid_CellEnter(dgAngle, e);
                    }
                }
            }

#region Calc
            // 0 - (+), 1 - (-), 2 - mul, 3 - div, 4 - increment, 5 - decrement, 6 - Change, 7 - Flip Value, 8 - Interpolation, 9 - 'S'Curve, 10 - Flip Position, 11 - Evd(+), 12 - Evd(-), 13 - EvdSet, 14 - Angle(+), 15 - Angle(-), 16 - AngleSet, 
            // 17, 18, 19 - Gravity Set(18 - Tilt 만 변화, 19 - Swing 만 변화)
            // 20, 21, 22 - LED Change(20-Red, 21-Green, 22-Blue) - 0 일때 클리어, 1 일때 동작
            // 23 - Motor Enable() - LED 와 동일
            // 24 - MotorType() - LED 와 동일
            // 25 - X(+), 26 - X(-), 27 - Y(+), 28 - (Y-), 29 - Z(+), 30 - Z(-)     
            public DataGridView GetHandle() { return dgAngle; }
            public void Calc(int nType, float fValue) { Calc(dgAngle, nType, fValue); }
            public void Calc(DataGridView OjwDataGrid, int nType, float fValue)
            {
                int nPos_Start_X = 0, nPos_Start_Y = 0;
                int nPos_End_X = 0, nPos_End_Y = 0;
                int nX_Limit = OjwDataGrid.RowCount;
                int nY_Limit = OjwDataGrid.ColumnCount - 1;
                // 첫 위치 찾아내기
                int k = 0;
                bool bStart = false;

                bool[,] abModify = new bool[nY_Limit, nX_Limit];

                bool[] abEquation = new bool[256];

                for (int j = 0; j < nY_Limit; j++) // 가로열
                {
                    bStart = false;
                    for (int i = 0; i < nX_Limit; i++) // 세로열
                    {
                        if (OjwDataGrid[j, i].Selected == true)
                        {
                            // Start
                            if (i == 0)
                            {
                                bStart = true;
                            }
                            else if (OjwDataGrid[j, i - 1].Selected == false)
                            {
                                bStart = true;
                            }
                            else bStart = false;

                            if (bStart == true)
                            {
                                nPos_Start_X = i; nPos_Start_Y = j;

                                for (k = i; k < nX_Limit; k++)
                                {
                                    if (k >= (nX_Limit - 1))
                                    {
                                        nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...
                                    }
                                    else
                                    {
                                        if (OjwDataGrid[j, k + 1].Selected == false)
                                        {
                                            nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...

                                            break;
                                        }
                                    }
                                }

                                if (
                                    (OjwDataGrid[nPos_Start_Y, nPos_Start_X].Value == null) ||
                                    (OjwDataGrid[nPos_End_Y, nPos_End_X].Value == null)
                                    ) continue;
                                // 위치와 값을 알고나서...
                                float fValue_Start = Ojw.CConvert.StrToFloat(OjwDataGrid[nPos_Start_Y, nPos_Start_X].Value.ToString());
                                float fValue_End = Ojw.CConvert.StrToFloat(OjwDataGrid[nPos_End_Y, nPos_End_X].Value.ToString());
                                int nLen = nPos_End_X - nPos_Start_X;

                                // 여기서 계산
                                //float fValue = Ojw.CConvert.StrToFloat(txtChangeValue.Text);
                                float fValueTmp;
                                float fTmp = 0;
                                for (k = nPos_Start_X; k <= nPos_End_X; k++)
                                {
                                    if (OjwDataGrid[j, k].Value == null) continue;
                                    //OjwDataGrid[j, k].Style.BackColor = Color.Blue;
                                    fValueTmp = Ojw.CConvert.StrToFloat(OjwDataGrid[j, k].Value.ToString());//(int)(Math.Round(, 0));
#region 덧셈 (+)
                                    if (nType == 0)
                                    {
                                        OjwDataGrid[j, k].Value = fValueTmp + fValue;   // +
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
#endregion
#region 뺄셈 (-)
                                    else if (nType == 1)
                                    {
                                        OjwDataGrid[j, k].Value = fValueTmp - fValue;  // -
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
#endregion
#region 곱하기
                                    else if (nType == 2)
                                    {
                                        OjwDataGrid[j, k].Value = fValueTmp * fValue;  // *
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
#endregion
#region 나누기
                                    else if (nType == 3)
                                    {
                                        if (fValue != 0)
                                        {
                                            OjwDataGrid[j, k].Value = fValueTmp / fValue;  // /
                                            // 수정된 놈이 있으면 체크
                                            abModify[j, k] = true;
                                        }
                                    }
#endregion
#region + 누적
                                    else if (nType == 4) // Increment
                                    {
                                        if (k != nPos_Start_X)
                                        {
                                            fTmp += fValue;
                                            OjwDataGrid[j, k].Value = fValue_Start + fTmp;
                                            // 수정된 놈이 있으면 체크
                                            abModify[j, k] = true;
                                        }
                                    }
#endregion
#region - 누적
                                    else if (nType == 5) // Decrement
                                    {
                                        if (k != nPos_Start_X)
                                        {
                                            fTmp += fValue;
                                            OjwDataGrid[j, k].Value = fValue_Start - fTmp;
                                            // 수정된 놈이 있으면 체크
                                            abModify[j, k] = true;
                                        }
                                    }
#endregion
#region 변경
                                    else if (nType == 6) // Change
                                    {
                                        OjwDataGrid[j, k].Value = fValue;
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
#endregion
#region Flip(지정값에 의한 반대값 취하기)
                                    else if (nType == 7) // Flip
                                    {
                                        // 현재 값
                                        float fTemp1 = fValue - Ojw.CConvert.StrToFloat(OjwDataGrid[j, k].Value.ToString());
                                        float fTemp2 = fValue + fTemp1;
                                        OjwDataGrid[j, k].Value = fTemp2;  // /
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
#endregion
#region Interpolation
                                    else if (nType == 8) // Interpolation
                                    {
                                        fValue = fValue_Start - fValue_End;
                                        float fTemp1 = 0;
                                        float fTemp2 = (float)(nPos_End_X - nPos_Start_X);
                                        if (((k - nPos_Start_X) != 0) && (fTemp2 > 0)) fTemp1 = (float)(fValue) / fTemp2 * (float)(k - nPos_Start_X);
                                        OjwDataGrid[j, k].Value = fValue_Start - fTemp1;
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
#endregion
#region 'S'Curve
                                    else if (nType == 9)
                                    {
                                        fValue = (fValue_Start - fValue_End) / 2.0f;
                                        float fAngle = 0;
                                        if ((k - nPos_Start_X) <= nLen / 2)
                                        {
                                            fAngle = (float)(k - nPos_Start_X) * 90.0f / ((float)(nLen) / 2.0f);
                                            fTmp = fValue - fValue * (float)Ojw.CMath.Cos(fAngle);
                                        }
                                        else
                                        {
                                            fAngle = (float)((float)(k - nPos_Start_X) - (float)nLen / 2.0f) * 90.0f / ((float)(nLen) / 2);
                                            fTmp = fValue + (fValue * (float)Ojw.CMath.Sin(fAngle));
                                        }
                                        OjwDataGrid[j, k].Value = fValue_Start - fTmp;
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
#endregion
#region Flip Position
                                    else if (nType == 10) // Flip Position
                                    {
                                        int nMotID = j - 1;// Grid_GetAxisNum_byIndex(j);
                                        int nMirror = CheckMirror(nMotID);
                                        int nGap = nMirror - nMotID;
                                        if (nMirror >= 0) // 축 플리핑
                                        {
                                            if (nMirror < nMotID)
                                            {
                                                // 이중복사 방지
                                                if (OjwDataGrid[j + nGap, k].Selected == false)
                                                {
                                                    float fTemp1 = Ojw.CConvert.StrToFloat(OjwDataGrid[j, k].Value.ToString());
                                                    //float fTemp2 = fTemp1;
                                                    OjwDataGrid[j, k].Value = OjwDataGrid[j + nGap, k].Value;
                                                    OjwDataGrid[j + nGap, k].Value = fTemp1;
                                                    // 수정된 놈이 있으면 체크
                                                    abModify[j, k] = true;
                                                }
                                            }
                                            else
                                            {
                                                float fTemp1 = Ojw.CConvert.StrToFloat(OjwDataGrid[j, k].Value.ToString());
                                                //float fTemp2 = fTemp1;
                                                OjwDataGrid[j, k].Value = OjwDataGrid[j + nGap, k].Value;
                                                OjwDataGrid[j + nGap, k].Value = fTemp1;
                                                // 수정된 놈이 있으면 체크
                                                abModify[j, k] = true;
                                            }
                                        }
                                        else if (nMirror == -2) // 0을 기준으로 플리핑
                                        {
                                            // 현재 값
                                            OjwDataGrid[j, k].Value = -Ojw.CConvert.StrToFloat(OjwDataGrid[j, k].Value.ToString());
                                            // 수정된 놈이 있으면 체크
                                            abModify[j, k] = true;
                                        }
                                        else
                                        {
                                            // nMirror == -1 -> 변화 없음
                                        }
                                    }
#endregion
#if false
#region Gravity Set
                                    if (nType == 17)
                                    {
                                        //Grid_SetGravity(k, true, true);
                                        Gravity(k, 2);
                                    }
                                    else if (nType == 18)
                                    {
                                        //Grid_SetGravity(k, true, false);
                                        Gravity(k, 0);
                                    }
                                    else if (nType == 19)
                                    {
                                        //Grid_SetGravity(k, false, true);
                                        Gravity(k, 1);
                                    }
#endregion

#region LED Change (Red, Green, Blue) - 0 이면 클리어 아니면 셋
                                    else if (nType == 20) // Red
                                    {
                                        int nMotID = Grid_GetAxisNum_byIndex(j);
                                        if (nMotID >= 0) Grid_SetFlag_Led_Red(k, nMotID, chkSetValue.Checked);
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    else if (nType == 21) // Green
                                    {
                                        int nMotID = Grid_GetAxisNum_byIndex(j);
                                        if (nMotID >= 0) Grid_SetFlag_Led_Green(k, nMotID, chkSetValue.Checked);
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
                                    else if (nType == 22) // Blue
                                    {
                                        int nMotID = Grid_GetAxisNum_byIndex(j);
                                        if (nMotID >= 0) Grid_SetFlag_Led_Blue(k, nMotID, chkSetValue.Checked);
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
#endregion

#region Motor Enable Change - 0 이면 클리어 아니면 셋
                                    else if (nType == 23)
                                    {
                                        int nMotID = Grid_GetAxisNum_byIndex(j);
                                        if (nMotID >= 0) Grid_SetFlag_En(k, nMotID, chkSetValue.Checked);
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
#endregion
#region Motor Type Change - 0 이면 클리어 아니면 셋
                                    else if (nType == 24)
                                    {
                                        int nMotID = Grid_GetAxisNum_byIndex(j);
                                        if (nMotID >= 0) Grid_SetFlag_Type(k, nMotID, chkSetValue.Checked);
                                        // 수정된 놈이 있으면 체크
                                        abModify[j, k] = true;
                                    }
#endregion
#region 25 - X(+), 26 - X(-), 27 - Y(+), 28 - (Y-), 29 - Z(+), 30 - Z(-)
                                    else if ((nType >= 25) && (nType <= 30))
                                    {
                                        int nLine = k;

                                        //// 수식 번호 알아내기
                                        int nMotID = j + 1;// Grid_GetAxisNum_byIndex(j);
                                        int nNum = -1;
                                        for (int ii = 0; ii < 256; ii++) // 256개중의 수식에서 찾아낸다.
                                        {
                                            int nMotCnt = m_C3d.m_CHeader.pSOjwCode[ii].nMotor_Max;
                                            for (int jj = 0; jj < nMotCnt; jj++)
                                            {
                                                int nMotNum = m_C3d.m_CHeader.pSOjwCode[ii].pnMotor_Number[jj];
                                                if (nMotNum == nMotID)
                                                {
                                                    nNum = ii;
                                                    break;
                                                }
                                            }
                                            if (nNum >= 0)
                                            {
                                                if (abEquation[nNum] == false)
                                                {
                                                    //float fValue = Ojw.CConvert.StrToFloat(txtChangeValue.Text);
                                                    float fX = ((nType == 25) ? fValue : ((nType == 26) ? -fValue : 0));
                                                    float fY = ((nType == 27) ? fValue : ((nType == 28) ? -fValue : 0));
                                                    float fZ = ((nType == 29) ? fValue : ((nType == 30) ? -fValue : 0));
                                                    //Grid_Xyz2Angle_Inc(i, m_nInverseKinematicsNumber_Test, fX, fY, fZ);
                                                    Grid_Xyz2Angle_Inc(nLine, nNum, fX, fY, fZ);
                                                    // 수정된 놈이 있으면 체크
                                                    abModify[j, k] = true;

                                                    abEquation[nNum] = true;
                                                }
                                                break;
                                            }
                                        }
                                    }
#endregion 25 - X(+), 26 - X(-), 27 - Y(+), 28 - (Y-), 29 - Z(+), 30 - Z(-)
                                    //if (OjwDataGrid == dgAngle)
                                    //{
                                    //    // Forward
                                    //    int nGroup = CheckKinematicsMotor_ByIndex(j);
                                    //    if (nGroup >= 0)
                                    //    {
                                    //        Grid_ForwardKinematics_Separation(k, nGroup);
                                    //    }
                                    //}
                                    //else// dataGrid_XY2Angle(k);
                                    //{
                                    //    int nGroup = j / 3;
                                    //    Grid_InverseKinematics_Separation(k, nGroup);
                                    //}
#endif
                                }
                            }
                        }
                    }
                    /////////////////////
                }
            }
            // -1 : 미러된 모터 없음(뒤집기 시 값의 변형 없음). 
            // -2 : 미러된 모터 없음(뒤집기 시 값을 '0' 을 기준으로 Flip), 
            // 0 ~ : 미러링된 모터 번호            
            public int CheckMirror(int nMotID)
            {
                int nRet = -1;
                //lbMot->Text = "";
                // Flip 가능한 그룹 알아내기			
                if (nMotID >= 0)
                {
                    return m_aSGridTable[nMotID].nMirror;
                    //return m_C3d.m_CHeader.pSMotorInfo[nMotID].nAxis_Mirror;
                }
                return nRet;
            }
#endregion Calc

            public bool m_bGridMotionEditing = false;
            //public bool m_bGridAdded = false;

            //int m_nFirstPos_Min_X = 9999999;
            //int m_nFirstPos_Min_Line = 9999999;
            private void dgAngle_KeyDown(object sender, KeyEventArgs e)
            {
                OjwGrid_KeyDown(dgAngle, e);
            }
            private void dgAngle_KeyUp(object sender, KeyEventArgs e)
            {
                OjwGrid_KeyUp(dgAngle, e);
            }
            private void OjwGrid_KeyDown(DataGridView dgGrid, KeyEventArgs e)
            {
                m_nKey = e.KeyValue;

                if (e.Control == true) m_bKey_Ctrl = true; else m_bKey_Ctrl = false;
                if (e.Alt == true) m_bKey_Alt = true; else m_bKey_Alt = false;
                if (e.Shift == true) m_bKey_Shift = true; else m_bKey_Shift = false;

                switch (e.KeyCode)
                {
#region Keys.Insert - 삽입
                    case Keys.Insert:
                        {
                            if (m_bBlock == false)
                            {
                                if (dgGrid.Focused == true)
                                {
                                    string strValue = "1";
                                    if (e.Control)
                                    {
                                        if (Ojw.CInputBox.Show("Insert", "뒤로 추가할 테이블의 수를 지정하시오", ref strValue) == DialogResult.OK)
                                        {
                                            int nInsertCnt = Ojw.CConvert.StrToInt(strValue);
                                            int nFirst = m_nCurrntCell;
                                            Add(m_nCurrntCell, nInsertCnt);
                                        }
                                    }
                                    else
                                    {
                                        if (Ojw.CInputBox.Show("Insert", "삽입할 테이블의 수를 지정하시오", ref strValue) == DialogResult.OK)
                                        {
                                            //m_bKeyInsert = true;
                                            int nInsertCnt = Ojw.CConvert.StrToInt(strValue);
                                            int nFirst = m_nCurrntCell;
                                            Insert(m_nCurrntCell, nInsertCnt);
                                            //Grid_Insert(nFirst, nInsertCnt);
                                            //m_bKeyInsert = false;
                                        }
                                    }
                                    //Grid_DisplayLine(m_nCurrntCell);
                                }
                            }
                            else
                            {
                                if (m_rtxtDraw != null)
                                {
                                    if (dgGrid.Focused == true)
                                    {
                                        string strValue = "1";
                                        if (e.Control)
                                        {
                                            if (Ojw.CInputBox.Show("Insert", "뒤로 추가할 테이블의 수를 지정하시오", ref strValue) == DialogResult.OK)
                                            {
                                                int nInsertCnt = Ojw.CConvert.StrToInt(strValue);
                                                if (nInsertCnt > 0)
                                                {
                                                    int nFirst = m_nCurrntCell;
                                                    string strDraw = String.Empty;
                                                    if (m_rtxtDraw.Lines.Length > 0)
                                                    {
                                                        for (int i = 0; i < m_rtxtDraw.Lines.Length; i++)
                                                        {
                                                            strDraw += m_rtxtDraw.Lines[i];

                                                            if (i == m_nCurrntCell)
                                                            {
                                                                for (int j = 0; j < nInsertCnt; j++)
                                                                {
                                                                    strDraw += "\r\n//";
                                                                }
                                                            }
                                                            if (i < m_rtxtDraw.Lines.Length - 1) strDraw += "\r\n";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int j = 0; j < nInsertCnt; j++)
                                                        {
                                                            strDraw += "//";
                                                            if (j < nInsertCnt - 1) strDraw += "\r\n";
                                                        }
                                                        //strDraw = "//";
                                                    }
                                                    m_bGridMotionEditing = true;
                                                    m_rtxtDraw.Text = strDraw;
                                                    //Add(m_nCurrntCell, nInsertCnt);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Ojw.CInputBox.Show("Insert", "삽입할 테이블의 수를 지정하시오", ref strValue) == DialogResult.OK)
                                            {
                                                //m_bKeyInsert = true;
                                                int nInsertCnt = Ojw.CConvert.StrToInt(strValue);
                                                if (nInsertCnt > 0)
                                                {
                                                    int nFirst = m_nCurrntCell;
                                                    string strDraw = String.Empty;
                                                    if (m_rtxtDraw.Lines.Length > 0)
                                                    {
                                                        for (int i = 0; i < m_rtxtDraw.Lines.Length; i++)
                                                        {
                                                            if (i == m_nCurrntCell)
                                                            {
                                                                for (int j = 0; j < nInsertCnt; j++)
                                                                {
                                                                    strDraw += "//\r\n";
                                                                }
                                                            }
                                                            strDraw += m_rtxtDraw.Lines[i];

                                                            if (i < m_rtxtDraw.Lines.Length - 1) strDraw += "\r\n";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int j = 0; j < nInsertCnt; j++)
                                                        {
                                                            strDraw += "//";
                                                            if (j < nInsertCnt - 1) strDraw += "\r\n"; 
                                                        }
                                                        //strDraw = "//";
                                                    }
                                                    m_bGridMotionEditing = true;
                                                    m_rtxtDraw.Text = strDraw;

                                                    //Insert(m_nCurrntCell, nInsertCnt);
                                                    //Grid_Insert(nFirst, nInsertCnt);
                                                    //m_bKeyInsert = false;
                                                }
                                            }
                                        }
                                        //Grid_DisplayLine(m_nCurrntCell);
                                        //m_bGridAdded = true; // For COjw_12_3D.cs only
                                    }
                                }
                            }

                        }
                        break;
#endregion Keys.Insert - 삽입
#region Keys.Escape - ESC : 긴급정지
                    //case Keys.Escape:
                    //    {
                    //        Stop();
                    //        //Cmd_Stop(m_nCurrentRobot);
                    //    }
                    //    break;
#endregion Keys.Escape - ESC : 긴급정지
#region Keys.F4 - 초기자세
                    //case Keys.F4:
                    //    {
                    //        frmMain.Cmd_InitPos(m_nCurrentRobot, frmMain._INITPOS_DEFAULT, 2000);
                    //        //DefaultPosition(1);
                    //    }
                    //    break;
#endregion Keys.F4 - 초기자세
#region Keys.F5 - 모션
                    //case Keys.F5:
                    //    {
                    //        if (m_bStart == false)
                    //            StartMotion();
                    //    }
                    //    break;
#endregion Keys.F5 - 모션
#region Keys.F - 주석 검색하기
                    case Keys.F:
                        {
                            String strFind = "";
                            if (Ojw.CInputBox.Show("검색", "검색할 주석의 키워드를 입력하시오", ref strFind) == DialogResult.OK)
                            {
                                int nLine = dgAngle.CurrentCell.RowIndex;
                                int nPos = 0;
                                string strValue = "";
                                bool bFind = false;
                                for (int i = nLine; i < dgAngle.RowCount; i++)
                                {
                                    strValue = GetCaption(i);

                                    // 원하는 문자열이 없다면 -1을 리턴
                                    if (strValue.IndexOf(strFind) >= 0)
                                    {
                                        nPos = i;
                                        bFind = true;
                                        break;
                                    }
                                }
                                if (bFind == true)
                                {
                                    dgAngle[0, nPos].Selected = true;
                                    ChangePos(dgAngle, nPos, 0);
                                }
                                else
                                {
                                    MessageBox.Show("결과 없음.");
                                }
                            }
                        }
                        break;
#endregion Keys.F - 주석 검색하기
#region Keys.F1 - 위치값 가져오기
                    //case Keys.F1:
                    //    {
                    //        if (dgGrid.Focused == true)
                    //        {
                    //            int _ADDRESS_TORQUE_CONTROL = 52;
                    //            for (int nAxis = 0; nAxis < m_C3d.m_CHeader.nMotorCnt; nAxis++)
                    //            {
                    //                if (m_abEnc[nAxis] == true)//((nAxis >= 6) && (nAxis <= 8)) // ojw5014_genie
                    //                {

                    //                    //Grid_SetMot(m_nCurrntCell, nAxis, 0);  -> Don't care 가 더 낳다.
                    //                }
                    //                else
                    //                {
                    //                    frmMain.OjwMotor.ReadMot(nAxis, _ADDRESS_TORQUE_CONTROL, 8);
                    //                    bool bOk = frmMain.OjwMotor.WaitReceive(nAxis, 1000);
                    //                    if (bOk == false)
                    //                    {
                    //                        //bPass = false;
                    //                    }
                    //                    else
                    //                    {
                    //                        Grid_SetMot(m_nCurrntCell, nAxis, frmMain.OjwMotor.GetPos_Angle(nAxis));
                    //                    }
                    //                }
                    //            }
                    //            //Grid_DisplayLine(m_nCurrntCell);
                    //        }
                    //    }
                    //    break;
#endregion Keys.F1 - 위치값 가져오기
#region Keys.Delete: - 삭제하기
                    case Keys.Delete:
                        {
                            if (m_bBlock == false)
                            {
                                if (dgGrid.Focused == true)
                                {
                                    if (e.Control)
                                    {
                                        Delete(m_nCurrntCell, 1);
                                    }
                                    else
                                    {
                                        int nPos_Start_X = 0, nPos_Start_Y = 0;
                                        int nPos_End_X = 0, nPos_End_Y = 0;
                                        int nX_Limit = dgGrid.RowCount;
                                        int nY_Limit = dgGrid.ColumnCount;
                                        // 첫 위치 찾아내기
                                        int k = 0;
                                        bool bStart = false;
                                        for (int j = 0; j < nY_Limit; j++)
                                        {
                                            bStart = false;
                                            for (int i = 0; i < nX_Limit; i++)
                                            {
                                                if (dgGrid[j, i].Selected == true)
                                                {
                                                    // Start
                                                    if (i == 0)
                                                    {
                                                        bStart = true;
                                                    }
                                                    else if (dgGrid[j, i - 1].Selected == false)
                                                    {
                                                        bStart = true;
                                                    }
                                                    else bStart = false;

                                                    if (bStart == true)
                                                    {
                                                        nPos_Start_X = i; nPos_Start_Y = j;

                                                        for (k = i; k < nX_Limit; k++)
                                                        {
                                                            if (k >= (nX_Limit - 1))
                                                            {
                                                                nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...
                                                            }
                                                            else
                                                            {
                                                                if (dgGrid[j, k + 1].Selected == false)
                                                                {
                                                                    nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...

                                                                    break;
                                                                }
                                                            }
                                                        }

                                                        for (k = nPos_Start_X; k <= nPos_End_X; k++)
                                                        {
                                                            dgGrid[j, k].Selected = true;
                                                            if (j == dgGrid.ColumnCount - 1) dgGrid[j, k].Value = ""; // Caption
                                                            //else if (j == 0) {} // Index
                                                            else
                                                            {
                                                                dgGrid[j, k].Value = 0;
                                                                //if ((j > 0) && (j <= m_C3d.m_CHeader.nMotorCnt))
                                                                //{
                                                                // Led만 클리어 한다.
                                                                //Grid_SetFlag_Led(k, j, 0);
                                                                //    m_pnFlag[k, j - 1] = (int)(m_pnFlag[k, j - 1] & 0x18);// | (int)(nLed & 0x07));
                                                                //}
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //Grid_DisplayLine(m_nCurrntCell);
                                    //CheckFlag(m_nCurrntCell);
                                }
                            }
                            else
                            {
                                if (dgGrid.Focused == true)
                                {
                                    if (e.Control)
                                    {
                                        int nTmp = 0;
                                        string strDraw = String.Empty;
                                        if (m_rtxtDraw.Lines.Length > 0)
                                        {
                                            for (int i = 0; i < m_rtxtDraw.Lines.Length; i++)
                                            {
                                                if (i != m_nCurrntCell)
                                                {
                                                    strDraw += m_rtxtDraw.Lines[i];
                                                    if (i < m_rtxtDraw.Lines.Length - 1) strDraw += "\r\n";                                                    
                                                }
                                                else nTmp++;
                                            }
                                        }
                                        if (m_nCurrntCell > 0) m_nCurrntCell -= nTmp;

                                        //else strDraw = "//";
                                        m_bGridMotionEditing = true;
                                        m_rtxtDraw.Text = strDraw;
                                    }
                                }
                            }
                        }
                        break;
#endregion Keys.Delete: - 삭제하기
#region Keys.V - 붙여넣기
                    case Keys.V:
                        {
                            if (m_bBlock == false)
                            {
                                if (dgGrid.Focused == true)
                                {
                                    try
                                    {
                                        //int nCntPos = 0;

                                        int nPos_X = 0, nPos_Y = 0;
                                        bool bPass = false;
                                        int nX_Limit = dgGrid.RowCount;
                                        int nY_Limit = dgGrid.ColumnCount;

#region 첫 위치 찾아내기
                                        for (int i = 0; i < nX_Limit; i++)
                                        {
                                            for (int j = 0; j < nY_Limit; j++)
                                            {
                                                if ((dgGrid[j, i].Selected == true) && (bPass == false))
                                                {
                                                    nPos_X = i; nPos_Y = j;
                                                    //Message(CConvert.IntToStr(nPos_X) + ", " + CConvert.IntToStr(nPos_Y));
                                                    bPass = true;
                                                    break;
                                                }
                                            }
                                            if (bPass == true) break;
                                        }
#endregion

                                        // 복사된 행의 열을 구하기 위하여 클립보드 사용.
                                        IDataObject iData = Clipboard.GetDataObject();
                                        string strClp = (string)iData.GetData(DataFormats.Text);

                                        if (strClp == null) break;

                                        string strClip = "";

#region Tab, \r\n 의 개수를 셈
                                        int nCnt = 0;
                                        int nT_Cnt = 0;
                                        int nLine_Cnt = 0;
                                        string strDisp = "";
                                        for (int i = 0; i < strClp.Length; i++)
                                        {
                                            if (strClp[i] == '\t') nT_Cnt++;
                                            else if (strClp[i] == '\n') nLine_Cnt++;
                                            if (strClp[i] != '\r')
                                            {
                                                if ((i == strClp.Length - 1) && (strClp[i] < 0x20)) break;
                                                if ((strClp[i] >= 0x20) && (strClp[i] != '\t') && (strClp[i] != '\n'))
                                                {
                                                    nCnt++;
                                                    strDisp += strClp[i];
                                                }
                                                strClip += strClp[i];
                                            }
                                        }
#endregion

                                        int nW = 0, nH = 0;
                                        int nAll = 0;
                                        if (strClip.Length > 0)
                                        {
                                            // strClip -> 이 데이타가 진짜
                                            //nW = 1; 
                                            nH = 1;
                                            nAll = 1;
                                            for (int i = 0; i < strClip.Length; i++)
                                            {
                                                // 가로열, 세로열 카운트
                                                if (strClip[i] == '\n') nH++;
                                                if ((strClip[i] == '\n') || (strClip[i] == '\t')) nAll++;
                                            }
                                            nW = (int)Math.Round((float)nAll / (float)nH, 0);
                                            //Message("nW = " + CConvert.IntToStr(nW) + ", nH = " + CConvert.IntToStr(nH));

                                            bool bW = false, bH = false;
                                            if (nW >= nY_Limit) bW = true;
                                            if (nH >= nX_Limit) bH = true;

                                            String[,] pstrValue = new string[nW, nH];
                                            bool[,] pbValid = new bool[nW, nH];
                                            int nX = 0, nY = 0;
                                            for (int i = 0; i < nW; i++) // 초기화
                                                for (int j = 0; j < nH; j++)
                                                {
                                                    pstrValue[i, j] = "";
                                                    pbValid[i, j] = false;
                                                }

                                            for (int i = 0; i < strClip.Length; i++)
                                            {
                                                if (strClip[i] == '\n') { nY++; nX = 0; }
                                                else if (strClip[i] == '\t') nX++;
                                                else
                                                {
                                                    pbValid[nX, nY] = true;
                                                    pstrValue[nX, nY] += strClip[i];
                                                }
                                            }

                                            if (e.Shift)
                                                Insert(m_nCurrntCell, nH);
                                            //Grid_Insert(nPos_X, nH);
                                            else
                                            {
                                                // 모자란 라인 채우기
                                                if (nH > dgGrid.RowCount)
                                                {
                                                    Insert(m_nCurrntCell, nH - dgGrid.RowCount);
                                                }
                                            }

#region 실 데이타 저장
                                            ////// 실 데이타 저장 ///////
                                            // Display
                                            int nOffset_i = 0, nOffset_j = 0;
                                            if (bW == true) nOffset_i++;
                                            if (bH == true) nOffset_j++;
                                            string strTmp;

                                            for (int j = 0; j < nH - nOffset_j; j++)
                                                for (int i = 0; i < nW - nOffset_i; i++)
                                                {
                                                    strTmp = pstrValue[i + nOffset_i, j + nOffset_j];
                                                    if (((nPos_X + j) < dgGrid.RowCount) && ((nPos_Y + i) < nY_Limit))
                                                    {
                                                        if ((pbValid[i + nOffset_i, j + nOffset_j] == true))
                                                        {
                                                            //dgGrid[nPos_Y + i, nPos_X + j].Style.BackColor = Color.Blue;
                                                            // Data
                                                            dgGrid[nPos_Y + i, nPos_X + j].Value = strTmp;
                                                            dgGrid[nPos_Y + i, nPos_X + j].Selected = true;

                                                            //                                             if (dgGrid == dgAngle)
                                                            //                                             {
                                                            //                                                 // Forward
                                                            //                                                 int nGroup = CheckKinematicsMotor_ByIndex(nPos_X + j);
                                                            //                                                 if (nGroup >= 0)
                                                            //                                                 {
                                                            //                                                     Grid_ForwardKinematics_Separation(nPos_Y + i, nGroup);
                                                            //                                                 }
                                                            //                                             }
                                                            //                                             else// dataGrid_XY2Angle(k);
                                                            //                                             {
                                                            //                                                 int nGroup = j / 3;
                                                            //                                                 Grid_InverseKinematics_Separation(nPos_Y + i, nGroup);
                                                            //                                             }
                                                        }
                                                    }
                                                }
                                            //                                 for (int j = 0; j < nH - nOffset_j; j++)
                                            //                                 {
                                            //                                     if (dgGrid == dataGrid_Angle) dataGrid_Angle2XY(nPos_X + j);
                                            //                                     else dataGrid_XY2Angle(nPos_X + j);
                                            //                                 }
#endregion



                                        }
                                        //m_nFirstPos_Min_X = 9999999;
                                        //m_nFirstPos_Min_Line = 9999999;
                                        //Grid_DisplayLine(m_nCurrntCell);
                                    }
                                    catch (Exception e2)
                                    {
                                        MessageBox.Show(e2.ToString());
                                    }
                                }
                            }
                        }
                        break;
#endregion Keys.V - 붙여넣기
#region Keys.C - 복사하기
                    case Keys.C:
                        {
                            if (dgGrid.Focused == true)
                            {
                                try
                                {
                                    //m_nFirstPos_Min_X = 9999999;
                                    //m_nFirstPos_Min_Line = 9999999;

                                    int nPos_Start_X = 0, nPos_Start_Y = 0;
                                    int nPos_End_X = 0, nPos_End_Y = 0;
                                    int nX_Limit = dgGrid.RowCount;
                                    int nY_Limit = dgGrid.ColumnCount;
                                    // 첫 위치 찾아내기
                                    int k = 0;
                                    bool bStart = false;
                                    for (int j = 0; j < nY_Limit; j++)
                                    {
                                        bStart = false;
                                        for (int i = 0; i < nX_Limit; i++)
                                        {
                                            if (dgGrid[j, i].Selected == true)
                                            {
                                                // Start
                                                if (i == 0)
                                                {
                                                    bStart = true;
                                                }
                                                else if (dgGrid[j, i - 1].Selected == false)
                                                {
                                                    bStart = true;
                                                }
                                                else bStart = false;

                                                if (bStart == true)
                                                {
                                                    nPos_Start_X = i; nPos_Start_Y = j;

                                                    for (k = i; k < nX_Limit; k++)
                                                    {
                                                        if (k >= (nX_Limit - 1))
                                                        {
                                                            nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...
                                                        }
                                                        else
                                                        {
                                                            if (dgGrid[j, k + 1].Selected == false)
                                                            {
                                                                nPos_End_X = k; nPos_End_Y = j; // j는 항상 같게...

                                                                break;
                                                            }
                                                        }
                                                    }

                                                    // 위치와 값을 알고나서...
                                                    //nValue_Start = Convert.ToInt16(rowData[nPos_Start_X][pstrData[nPos_Start_Y]]);
                                                    //nValue_End = Convert.ToInt16(rowData[nPos_End_X][pstrData[nPos_End_Y]]);
                                                    //int nLen = nPos_End_X - nPos_Start_X;

                                                    // 여기서 계산
                                                    //int nValue = CConvert.StrToInt(txtChangeValue.Text);

                                                    //bool bFirst = true;
                                                    for (k = nPos_Start_X; k <= nPos_End_X; k++)
                                                    {
                                                        //dgGrid[j, k].Style.BackColor = Color.Blue;
                                                        dgGrid[j, k].Selected = true;
                                                        if (j == dgGrid.ColumnCount - 1) dgGrid[j, k].Value = ""; // Caption
                                                        //else if (j == 0) {} // Index
                                                        else
                                                        {

#if _COPY_FLAG
                                                        int nPosLine = k;
                                                        int nPosMotor = j;

                                                        m_pnFlag_Copy[nPosLine, nPosMotor] = (int)((m_pnFlag[nPosLine, nPosMotor] & 0x18) | (int)(m_pnFlag[nPosLine, nPosMotor] & 0x07));
                                                        m_nFirstPos_Min_Line = (m_nFirstPos_Min_Line > nPosLine) ? nPosLine : m_nFirstPos_Min_Line;
                                                        m_nFirstPos_Min_X = (m_nFirstPos_Min_X > nPosMotor) ? nPosMotor : m_nFirstPos_Min_X;

#if false
                                                        if ((j >= 0) && (j <= m_C3d.m_CHeader.nMotorCnt))
                                                        {
                                                            int m = j;
                                                            if ((m >= 1) && (m <= m_C3d.m_CHeader.nMotorCnt))
                                                            {
                                                                // Led만 복사 한다.
                                                                //Grid_SetFlag_Led(k, j, 0); 
                                                                m_pnFlag_Copy[k, m - 1] = (int)((m_pnFlag[k, m - 1] & 0x18) | (int)(m_pnFlag[k, m - 1] & 0x07));

                                                                m_nFirstPos_Min_Line = (m_nFirstPos_Min_Line > k) ? k : m_nFirstPos_Min_Line;
                                                                m_nFirstPos_Min_X = (m_nFirstPos_Min_X > (m - 1)) ? (m - 1) : m_nFirstPos_Min_X;

                                                                //m_pnFlag_Copy_Pos[k, m - 1, 0] = k;
                                                                //m_pnFlag_Copy_Pos[k, m - 1, 1] = m - 1;
                                                                //if (bFirst == true)
                                                                //{
                                                                    m_pnFlag_Offset_Num_Line[nCntPos] = k;
                                                                    m_pnFlag_Offset_Num_Motor[nCntPos] = m - 1;
                                                                    nCntPos++;
                                                                    //bFirst = false;
                                                                //}
                                                            }
                                                        }
#endif
#endif
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //Grid_DisplayLine(m_nCurrntCell);
                                }
                                catch (Exception e2)
                                {
                                    MessageBox.Show(e2.ToString());
                                }
                            }
                        }
                        break;
#endregion Keys.C - 복사하기
                }
            }
            private void OjwGrid_KeyUp(object sender, KeyEventArgs e)
            {
                m_nKey = 0;
                m_bKey_Ctrl = false;
                m_bKey_Alt = false;
                m_bKey_Shift = false;

                //m_bKeyDown = false;
            }
            private bool m_bBlock = false;
            private RichTextBox m_rtxtDraw = null;
            public void dgAngle_Block_GridChange(RichTextBox rtxtDraw, bool bBlock) { m_rtxtDraw = rtxtDraw; m_bBlock = bBlock; }
            private void dgAngle_MouseClick(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                {
                    DataGridView.HitTestInfo hti = dgAngle.HitTest(e.X, e.Y);
                    int a = hti.RowIndex;
                    int b = hti.ColumnIndex;
                    if ((b < 0) && (a >= 0) && (a < dgAngle.RowCount))
                    {
                        //Grid_CaptionControl(m_bKey_Ctrl); // 컨트롤 키가 눌리면 삽입, 아니라면 변경
                    }
                    else
                    {
                        //OjwGrid_SetMotion();
                    }
                }
            }
            //private bool m_bMouseDown = false;
            //private bool m_bMouseMove = false;
            private void dgAngle_MouseDown(object sender, MouseEventArgs e)
            {
                DataGridView.HitTestInfo hti = dgAngle.HitTest(e.X, e.Y);
                int a = hti.RowIndex;
                int b = hti.ColumnIndex;

                if (a < 0) dgAngle.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
                else if (b < 0) dgAngle.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;

                //m_bMouseDown = true;
            }
            private void dgAngle_MouseMove(object sender, MouseEventArgs e)
            {
                //if (m_bMouseDown == true) m_bMouseMove = true;
            }
            private void dgAngle_MouseUp(object sender, MouseEventArgs e)
            {
                //m_bMouseDown = false;
                //m_bMouseMove = false;
            }

            private void dgAngle_MouseDoubleClick(object sender, MouseEventArgs e)
            {
                OjwGrid_CellMouseDoubleClick(dgAngle, e);
            }
            //public void Add_CellMouseDoubleClick(DataGridViewCellMouseEventHandler FFunc) { dgAngle.CellMouseDoubleClick += (DataGridViewCellMouseEventHandler)FFunc; }
            //public void Sub_CellMouseDoubleClick(DataGridViewCellMouseEventHandler FFunc) { dgAngle.CellMouseDoubleClick -= (DataGridViewCellMouseEventHandler)FFunc; }
            private void OjwGrid_CellMouseDoubleClick(DataGridView dgData, MouseEventArgs e)
            {
                DataGridView.HitTestInfo hti = dgData.HitTest(e.X, e.Y);
                int a = hti.RowIndex;
                int b = hti.ColumnIndex;
                if ((b < 0) && (a >= 0) && (a < dgData.RowCount))
                {
                    CaptionControl(m_bKey_Ctrl); // 컨트롤 키가 눌리면 삽입, 아니라면 변경
                }
                else
                {
                    //OjwGrid_SetMotion();
                }
            }
            private void CaptionControl(bool bInsert)
            {
                int nLine = m_nCurrntCell;
                int nCharSize = 46;
                string strValue = (bInsert == true) ? "" : GetCaption(nLine);
                string strMessage = (bInsert == true) ? "삽입할 주석의 내용을 입력하시오(한글 " + Ojw.CConvert.IntToStr(nCharSize / 2) + "글자, 영문 " + Ojw.CConvert.IntToStr(nCharSize) + "글자)" : "변경할 주석의 내용을 입력하시오(한글 " + Ojw.CConvert.IntToStr(nCharSize / 2) + "글자, 영문 " + Ojw.CConvert.IntToStr(nCharSize) + "글자)";
                if (Ojw.CInputBox.Show("주석", strMessage, ref strValue) == DialogResult.OK)
                {
                    int nX = dgAngle.CurrentCell.ColumnIndex;
                    int nY = dgAngle.CurrentCell.RowIndex;

                    // 주석용 테이블 삽입
                    if (bInsert == true)
                        Insert(m_nCurrntCell, 1);
                    String strFirst = "", strSecond = "";
                    int nLength = strValue.Length;
                    if (nLength > 0)
                    {
                        if (nLength > nCharSize)
                        {
                            strFirst = strValue.Substring(0, nCharSize);
                            strSecond = strValue.Substring(nCharSize + 1, (((nLength - nCharSize) > nCharSize) ? nCharSize : (nLength - nCharSize)));
                        }
                        else
                        {
                            strFirst = strValue;
                        }
                    }
                    else
                    {
                        strValue = "";
                    }
                    SetCaption(nLine, strValue);
                }
            }
        }
        #endregion GridTable
    }
}
#endif