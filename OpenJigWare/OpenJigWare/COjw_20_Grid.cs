using System;
using System.Collections.Generic;
using System.Linq;
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
                    Color.Red, //m_gvGrid.RowHeadersDefaultCellStyle.ForeColor,
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
                m_propGrid.Name = "pnOjwProp_" + Ojw.CConvert.IntToStr((int)ctrlProp.Handle);
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
            }
            public void SetEvent_Changed(PropertyValueChangedEventHandler FFunction)
            {                
                m_propGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(FFunction);                
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
            //_11_Reserve,
            //_12_Reserve,
            //_13_Reserve,
            //_14_Reserve,
            //_15_Reserve,
            //_16_Reserve,
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
            public int nData0;
            public int nData1;
            public int nData2;
            public int nData3;
            public int nData4;
            public int nData5;
            public string strCaption;
            public SGridLineInfo_t(bool enable, int group, string caption, int d0, int d1, int d2, int d3, int d4, int d5)
            {
                this.bEn = enable;
                this.nGroupLine = group;
                this.strCaption = caption;
                this.nData0 = d0;
                this.nData1 = d1;
                this.nData2 = d2;
                this.nData3 = d3;
                this.nData4 = d4;
                this.nData5 = d5;
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
                dgAngle.MouseClick += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseClick);
                dgAngle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDoubleClick);
                dgAngle.MouseDown += new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDown);
            
            }
            private void Events_Remove()
            {
                dgAngle.RowPostPaint -= new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(dgAngle_RowPostPaint);
                dgAngle.CellEnter -= new System.Windows.Forms.DataGridViewCellEventHandler(dgAngle_CellEnter);
                dgAngle.KeyDown -= new System.Windows.Forms.KeyEventHandler(dgAngle_KeyDown);
                dgAngle.MouseClick -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseClick);
                dgAngle.MouseDoubleClick -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDoubleClick);
                dgAngle.MouseDown -= new System.Windows.Forms.MouseEventHandler(dgAngle_MouseDown);
            }
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
            public void Create(DataGridView dg, int nLineCnt, params SGridTable_t[] aSGridTable)
            {
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
                dgAngle.Columns[nPos].Width = 50;
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

                i = 0;
                for (i = 0; i < 6; i++)
                {
                    // 1 ~ 6 - Data0 ~ Data5
                    dgAngle.Columns.Add("Data" + i.ToString(), "Data" + i.ToString());
                    dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgAngle.Columns[nPos].Width = 10;
                    dgAngle.Columns[nPos++].Visible = false;
                }
                // 7 - Group
                dgAngle.Columns.Add("Group", "Group");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = 10;
                dgAngle.Columns[nPos++].Visible = false;
                // 8 - Caption
                dgAngle.Columns.Add("Caption", "Caption");
                dgAngle.Columns[nPos].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgAngle.Columns[nPos].Width = 10;
                dgAngle.Columns[nPos++].Visible = false;

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
                    Color.Red, //dgAngle.RowHeadersDefaultCellStyle.ForeColor,
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
            //        int nPos = Grid_GetIndex_byMotorAxis(m_pCHeader[m_nCurrentRobot].nMotorCnt - 1);
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

                    if ((dgGrid.CurrentCell.ColumnIndex == m_nCurrntColumn) && (dgGrid.CurrentCell.RowIndex == m_nCurrntCell)) return;
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
            public void Set(int nNum, object data) { Set(m_nCurrntCell, nNum, data); }
            public void Set(int nLine, int nNum, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[nNum + 1].Value = data;
            }
            public object Get(int nNum) { return Get(m_nCurrntCell, nNum); }
            public object Get(int nLine, int nNum)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return false;
                    return dgAngle.Rows[nLine].Cells[nNum + 1].Value.ToString();
                }
                catch// (System.Exception e)
                {
                    return false;
                }
            }
            public void SetData0(object data) { SetData0(m_nCurrntCell, data); }
            public void SetData0(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 8].Value = data;
            }
            public object GetData0() { return GetData0(m_nCurrntCell); }
            public object GetData0(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 8].Value.ToString();
                }
                catch// (System.Exception e)
                {
                    return false;
                }
            }
            public void SetData1(object data) { SetData1(m_nCurrntCell, data); }
            public void SetData1(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 7].Value = data;
            }
            public object GetData1() { return GetData1(m_nCurrntCell); }
            public object GetData1(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 7].Value.ToString();
                }
                catch// (System.Exception e)
                {
                    return false;
                }
            }
            public void SetData2(object data) { SetData2(m_nCurrntCell, data); }
            public void SetData2(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 6].Value = data;
            }
            public object GetData2() { return GetData2(m_nCurrntCell); }
            public object GetData2(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 6].Value.ToString();
                }
                catch// (System.Exception e)
                {
                    return false;
                }
            }
            public void SetData3(object data) { SetData3(m_nCurrntCell, data); }
            public void SetData3(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 5].Value = data;
            }
            public object GetData3() { return GetData3(m_nCurrntCell); }
            public object GetData3(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 5].Value.ToString();
                }
                catch// (System.Exception e)
                {
                    return false;
                }
            }
            public void SetData4(object data) { SetData4(m_nCurrntCell, data); }
            public void SetData4(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 4].Value = data;
            }
            public object GetData4() { return GetData4(m_nCurrntCell); }
            public object GetData4(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 4].Value.ToString();
                }
                catch// (System.Exception e)
                {
                    return false;
                }
            }
            public void SetData5(object data) { SetData5(m_nCurrntCell, data); }
            public void SetData5(int nLine, object data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 3].Value = data;
            }
            public object GetData5() { return GetData5(m_nCurrntCell); }
            public object GetData5(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return false;
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 3].Value.ToString();
                }
                catch// (System.Exception e)
                {
                    return false;
                }
            }
            public void SetGroup(int data) { SetGroup(m_nCurrntCell, data); }
            public void SetGroup(int nLine, int data)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 2].Value = data;
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

            private void SetColorGrid(int nIndex, int nCount)
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
                for (int j = 0; j < dgAngle.ColumnCount - (9 - 1); j++)
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
                    //                (j == (m_pCHeader[m_nCurrentRobot].nMotorCnt + 1)) || // Speed
                    //                (j == (m_pCHeader[m_nCurrentRobot].nMotorCnt + 2))    // Delay
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

            public int GetGroup() { return GetGroup(m_nCurrntCell); }
            public int GetGroup(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;
                    return Convert.ToInt32(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 2].Value);
                }
                catch// (System.Exception e)
                {
                    return 0;
                }
            }
            public void SetData(int nLine, int nNum, object value)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[nNum + 1].Value = value;
            }
            public object GetData(int nLine, int nNum) { return dgAngle.Rows[nLine].Cells[nNum + 1].Value; }
            public void SetCaption(String data) { SetCaption(m_nCurrntCell, data); }
            public void SetCaption(int nLine, String strValue)
            {
                if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 1].Value = strValue;
            }
            public String GetCaption() { return GetCaption(m_nCurrntCell); }
            public String GetCaption(int nLine)
            {
                try
                {
                    if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return "";
                    return dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 1].Value.ToString();
                }
                catch// (System.Exception e)
                {
                    return "";
                }
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
                                            int nMotCnt = m_pCHeader[m_nCurrentRobot].pSOjwCode[ii].nMotor_Max;
                                            for (int jj = 0; jj < nMotCnt; jj++)
                                            {
                                                int nMotNum = m_pCHeader[m_nCurrentRobot].pSOjwCode[ii].pnMotor_Number[jj];
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
                    //return m_pCHeader[m_nCurrentRobot].pSMotorInfo[nMotID].nAxis_Mirror;
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
                                if (m_txtDraw != null)
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
                                                    if (m_txtDraw.Lines.Length > 0)
                                                    {
                                                        for (int i = 0; i < m_txtDraw.Lines.Length; i++)
                                                        {
                                                            strDraw += m_txtDraw.Lines[i];

                                                            if (i == m_nCurrntCell)
                                                            {
                                                                for (int j = 0; j < nInsertCnt; j++)
                                                                {
                                                                    strDraw += "\r\n//";
                                                                }
                                                            }
                                                            if (i < m_txtDraw.Lines.Length - 1) strDraw += "\r\n";
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
                                                    m_txtDraw.Text = strDraw;
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
                                                    if (m_txtDraw.Lines.Length > 0)
                                                    {
                                                        for (int i = 0; i < m_txtDraw.Lines.Length; i++)
                                                        {
                                                            if (i == m_nCurrntCell)
                                                            {
                                                                for (int j = 0; j < nInsertCnt; j++)
                                                                {
                                                                    strDraw += "//\r\n";
                                                                }
                                                            }
                                                            strDraw += m_txtDraw.Lines[i];

                                                            if (i < m_txtDraw.Lines.Length - 1) strDraw += "\r\n";
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
                                                    m_txtDraw.Text = strDraw;

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
                    //            for (int nAxis = 0; nAxis < m_pCHeader[m_nCurrentRobot].nMotorCnt; nAxis++)
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
                                                                //if ((j > 0) && (j <= m_pCHeader[m_nCurrentRobot].nMotorCnt))
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
                                        if (m_txtDraw.Lines.Length > 0)
                                        {
                                            for (int i = 0; i < m_txtDraw.Lines.Length; i++)
                                            {
                                                if (i != m_nCurrntCell)
                                                {
                                                    strDraw += m_txtDraw.Lines[i];
                                                    if (i < m_txtDraw.Lines.Length - 1) strDraw += "\r\n";                                                    
                                                }
                                                else nTmp++;
                                            }
                                        }
                                        if (m_nCurrntCell > 0) m_nCurrntCell -= nTmp;

                                        //else strDraw = "//";
                                        m_bGridMotionEditing = true;
                                        m_txtDraw.Text = strDraw;
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
                                                    //Message(OjwConvert.IntToStr(nPos_X) + ", " + OjwConvert.IntToStr(nPos_Y));
                                                    bPass = true;
                                                    break;
                                                }
                                            }
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
                                            //Message("nW = " + OjwConvert.IntToStr(nW) + ", nH = " + OjwConvert.IntToStr(nH));

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
                                                    //int nValue = OjwConvert.StrToInt(txtChangeValue.Text);

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
                                                        if ((j >= 0) && (j <= m_pCHeader[m_nCurrentRobot].nMotorCnt))
                                                        {
                                                            int m = j;
                                                            if ((m >= 1) && (m <= m_pCHeader[m_nCurrentRobot].nMotorCnt))
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
            private bool m_bBlock = false;
            private TextBox m_txtDraw = null;
            public void dgAngle_Block_GridChange(TextBox txtDraw, bool bBlock) { m_txtDraw = txtDraw; m_bBlock = bBlock; }
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
            private void dgAngle_MouseDown(object sender, MouseEventArgs e)
            {
                DataGridView.HitTestInfo hti = dgAngle.HitTest(e.X, e.Y);
                int a = hti.RowIndex;
                int b = hti.ColumnIndex;

                if (a < 0) dgAngle.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
                else if (b < 0) dgAngle.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
            }
            private void dgAngle_MouseDoubleClick(object sender, MouseEventArgs e)
            {
                OjwGrid_CellMouseDoubleClick(dgAngle, e);
            }
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
