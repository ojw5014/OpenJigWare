using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenJigWare;

namespace OpenJigWare.Docking
{
    public partial class frmGridEditor : Form
    {
        public frmGridEditor()
        {
            InitializeComponent();
        }
        private bool m_bProgEnd = false;
        private Ojw.C3d m_C3d { get { return frmDesigner.m_C3d; } set { frmDesigner.m_C3d = value; } }
        private Ojw.CGridView m_CGridMotion = new Ojw.CGridView();
        private bool m_bGrid_Init = false;
        private void frmGridEditor_Load(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Init(dgGrid, 40, 999, false);
            m_C3d.Event_FileOpen.UserEvent += new EventHandler(FileOpen);
            m_C3d.Event_Compile.UserEvent += new EventHandler(Compile);
            //m_C3d.GridMotionEditor_Init_Panel(pnButton);
            
            //m_CGridMotion.Create(dgGrid, 0, 0, dgGrid.Width, dgGrid.Height, (new string[] {"Test", "1", "2", "Index3"}));
            //m_CGridMotion.Grid_Add(0, 2);

            GridMotionEditor_Init(dgGrid, 40, 999, false);

            m_bGrid_Init = true;
        }
        private void SetEvent()
        {
            if (m_bGrid_Init == false)
            {
                m_C3d.m_CGridMotionEditor.Events_Remove_KeyDown();
                m_C3d.m_CGridMotionEditor.Events_Remove_KeyUp();

                m_C3d.m_CGridMotionEditor.Events_Remove_CellEnter();
                m_C3d.m_CGridMotionEditor.GetHandle().CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(GridMotionEditor_Event_CellEnter);
#if false
                m_C3d.m_CGridMotionEditor.GetHandle().MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(GridMotionEditor_MouseDoubleClick);

                m_C3d.m_CGridMotionEditor.GetHandle().KeyDown += new System.Windows.Forms.KeyEventHandler(GridMotionEditor_Event_KeyDown);
                m_C3d.m_CGridMotionEditor.GetHandle().KeyUp += new System.Windows.Forms.KeyEventHandler(GridMotionEditor_Event_KeyUp);

                m_C3d.m_CGridMotionEditor.GetHandle().MouseDown += new System.Windows.Forms.MouseEventHandler(GridMotionEditor_MouseDown);
                m_C3d.m_CGridMotionEditor.GetHandle().MouseUp += new System.Windows.Forms.MouseEventHandler(GridMotionEditor_MouseUp);
                m_C3d.m_CGridMotionEditor.GetHandle().MouseMove += new System.Windows.Forms.MouseEventHandler(GridMotionEditor_MouseMove);

                m_C3d.m_CGridMotionEditor.GetHandle().Scroll += new ScrollEventHandler(GridMotionEditor_Scroll);
#endif
            }
        }
        public void GridMotionEditor_Event_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (m_C3d.m_CGridMotionEditor.GetHandle().Focused == true)// || (m_bStart == true))
            {
                int nLine = e.RowIndex;
                int nPos = e.ColumnIndex;
                m_C3d.m_CGridMotionEditor.SetChangeCurrentCol(nPos); // OJW5014_20161031
                m_C3d.m_CGridMotionEditor.SetChangeCurrentLine(nLine); // OJW5014_20161031
                if (m_C3d.m_CGridMotionEditor.GetHandle().Focused == true)
                {
                    GridMotionEditor_Draw(e.RowIndex);
                }
            }
        }
        public float GridMotionEditor_GetMotor(int nLine, int nMotorID)
        {
            try
            {
                int nIndex = m_C3d.m_CHeader.pSMotorInfo[nMotorID].nMotionEditor_Index - 1;
                float fValue = Convert.ToSingle(m_C3d.m_CGridMotionEditor.GetData(nLine, nIndex));
                return fValue;
                //return Convert.ToSingle(m_CGridMotionEditor.GetData(nLine, nMotorID));
            }
            catch
            {
                return 0.0f;
            }
        }
        public void GridMotionEditor_Draw(int nLine)
        {
            if (m_bGrid_Init == true)
            {
                for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++)
                {
                    int nID = m_C3d.m_CHeader.anMotorIDs[i];
                    if (nID < 0) continue;
                    if ((m_C3d.m_CHeader.pSMotorInfo[nID].nMotor_Enable < 0) || (m_C3d.m_CHeader.pSMotorInfo[nID].nMotor_Enable > 1))
                        continue;
                    //int nIndex = m_C3d.m_CHeader.pSMotorInfo[nID].nMotionEditor_Index;
                    try
                    {
                        m_C3d.SetData(nID, GridMotionEditor_GetMotor(nLine, nID));
                    }
                    catch
                    {
                        float fValue = m_C3d.GetData(nID);
                        m_C3d.SetData(nID, fValue);
                    }
                }
            }
        }
        private void FileOpen(object sender, EventArgs e)
        {
            // FileOpen 후에 생기는 이벤트 처리
            //GridMotionEditor_Init(dgGrid, 40, 999, true);

            m_C3d.SetMotionEditor_NoBuild();
        }
        private void Compile(object sender, EventArgs e)
        {
            // FileOpen 후에 생기는 이벤트 처리
            GridMotionEditor_Init(dgGrid, 40, 999, true);

            //m_C3d.SetMotionEditor_NoBuild();
        }

        //List<int> m_lstIDs_Motion = new List<int>();
        //List<int> m_lstCols_Motion = new List<int>();
        //private bool m_bGridInit = false;
        // 모션초기화시에 모션 인덱스를 고려하도록 수정(ojw5014_MotionIndex)
        //private List<int> lstIDs = new List<int>();
        public void GridMotionEditor_Init(DataGridView dgAngle, int nWidth, int nLines, bool bEventSet)
        {
            int nWidth_Interval = 11;
            int nWidth_Offset = 0;
            int nLedCnt = 3;

            //dgGrid = dgAngle;
            // 모터의 갯수
            int nCnt = m_C3d.m_CHeader.nMotorCnt;
            Ojw.SGridTable_t[] aSTable = new Ojw.SGridTable_t[nCnt + 2];// + 12];// Speed, Delay   // Speed, Delay, Time //2]; // Speed, Delay
            Color[] acColor = new Color[] { Color.Orange, Color.Green, Color.Yellow, Color.Cyan, Color.GreenYellow, Color.DarkOrange };
#if false
            if (m_pbtnEnable != null)
            {
                for (int i = 0; i < m_pbtnEnable.Length; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        m_pbtnLed[j, i].Dispose();
                        m_pbtnLed[j, i] = null;
                    }
                    m_pbtnEnable[i].Dispose();
                    m_pbtnEnable[i] = null;
                    m_pbtnType[i].Dispose();
                    m_pbtnType[i] = null;
                }
            }
            m_pbtnEnable = new Button[nCnt];
            m_pbtnType = new Button[nCnt];
            m_pbtnLed = new Button[nLedCnt, nCnt];
#endif

            int nTop = 55;// dgAngle.Top;

#if false // m_C3d 의 fileopen 에서 처리하도록...
            List<int> lstIDs = new List<int>();
            //List<int> lstIDs_Motion = new List<int>();
            lstIDs.Clear();
            m_C3d.m_lstIDs_Motion.Clear();
            //m_C3d.CompileDesign();
            for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++)
            {
                if ((m_C3d.m_CHeader.pSMotorInfo[i].nMotor_Enable < 0) || (m_C3d.m_CHeader.pSMotorInfo[i].nMotor_Enable > 1)) continue;
                int nAxis = m_C3d.m_CHeader.anMotorIDs[i];
                int nID = m_C3d.m_CHeader.pSMotorInfo[nAxis].nMotionEditor_Index;
                if (nID != 0)
                {
                    m_C3d.m_lstIDs_Motion.Add(nID);
                }
                else lstIDs.Add(nAxis);
                //if (m_C3d.m_CHeader.pSMotorInfo[i].nMotionEditor_Index 
                //lstIDs.Add();
            }
            bool bSet = false;
            do
            {
                bSet = false;
                int nValue;
                int nValue2;
                for (int i = 1; i < m_C3d.m_lstIDs_Motion.Count; i++)
                {
                    int nID = m_C3d.m_lstIDs_Motion[i - 1];
                    nValue = m_C3d.m_CHeader.pSMotorInfo[nID].nMotionEditor_Index;
                    int nID2 = m_C3d.m_lstIDs_Motion[i];
                    nValue2 = m_C3d.m_CHeader.pSMotorInfo[nID2].nMotionEditor_Index;
                    if (nValue > nValue2)
                    {
                        m_C3d.m_lstIDs_Motion[i - 1] = m_C3d.m_lstIDs_Motion[i];
                        m_C3d.m_lstIDs_Motion[i] = nID;
                        bSet = true;
                    }
                }
            }
            while (bSet);
            m_C3d.m_lstIDs_Motion.AddRange(lstIDs);
#endif
            nCnt = m_C3d.m_lstIDs_Motion.Count;
            //m_lstCols_Motion.Clear();
            for (int i = 0; i < nCnt; i++)
            {
                int nMotWidth = nWidth;
                int nMotHeight = 18;
                int nHeightOffset = -1;
#if false
                // Enable
                m_pbtnEnable[i] = new Button();
                m_pbtnEnable[i].Top = nTop - 52;
                m_pbtnEnable[i].Left = dgAngle.Left + (dgAngle.RowHeadersWidth + nWidth_Offset) + nWidth_Interval;
                m_pbtnEnable[i].Width = nMotWidth / 5 * 2;
                m_pbtnEnable[i].Height = (nMotHeight + nHeightOffset) * 3 / 2;
                m_pbtnEnable[i].Name = "btnServe_En" + Ojw.CConvert.IntToStr(i);
                m_pbtnEnable[i].Text = "E";
                m_pbtnEnable[i].Click += new System.EventHandler(btnServe_En_Click);
                m_pnButton.Controls.Add(m_pbtnEnable[i]);
                // Type
                m_pbtnType[i] = new Button();
                m_pbtnType[i].Top = nTop - 52 + m_pbtnEnable[i].Height;
                m_pbtnType[i].Left = dgAngle.Left + (dgAngle.RowHeadersWidth + nWidth_Offset) + nWidth_Interval;
                m_pbtnType[i].Width = nMotWidth / 5 * 2;
                m_pbtnType[i].Height = (nMotHeight + nHeightOffset) * 3 / 2;
                m_pbtnType[i].Name = "btnServe_Type" + Ojw.CConvert.IntToStr(i);
                m_pbtnType[i].Text = "T";
                m_pbtnType[i].Click += new System.EventHandler(btnServe_Type_Click);
                m_pnButton.Controls.Add(m_pbtnType[i]);
                // Led
                String[] pstrText = new String[3] { "R", "B", "G" };
                for (int j = 0; j < nLedCnt; j++)
                {
                    m_pbtnLed[j, i] = new Button();
                    m_pbtnLed[j, i].Top = m_pbtnEnable[i].Top + ((nMotHeight + ((j != 0) ? nHeightOffset : 0)) * j);
                    m_pbtnLed[j, i].Left = m_pbtnEnable[i].Left + nMotWidth / 5 * 2;
                    m_pbtnLed[j, i].Width = nMotWidth / 5 * 3;
                    m_pbtnLed[j, i].Height = nMotHeight;
                    m_pbtnLed[j, i].Name = "btnServe_Led" + Ojw.CConvert.IntToStr(i) + "_" + Ojw.CConvert.IntToStr(j);
                    m_pbtnLed[j, i].Text = pstrText[j];
                    m_pbtnLed[j, i].Click += new System.EventHandler(btnServe_Led_Click);
                    m_pnButton.Controls.Add(m_pbtnLed[j, i]);
                }
#endif
                int nID = m_C3d.m_lstIDs_Motion[i];
                
                // Save Cols
                //m_lstCols_Motion.Add(i);

                Color cColor = acColor[m_C3d.m_CHeader.pSMotorInfo[nID].nGroupNumber % acColor.Length];
                string strName = Ojw.CConvert.RemoveChar(m_C3d.m_CHeader.pSMotorInfo[nID].strNickName, '\0');
                strName = (strName.Length > 0) ? strName : "M" + Ojw.CConvert.IntToStr(nID, 2);
                aSTable[i] = new Ojw.SGridTable_t(strName, nWidth, m_C3d.m_CHeader.pSMotorInfo[nID].nGroupNumber, m_C3d.m_CHeader.pSMotorInfo[nID].nAxis_Mirror, cColor, m_C3d.m_CHeader.pSMotorInfo[nID].fInitAngle, m_C3d.m_CHeader.pSMotorInfo[nID].fInitAngle2);

                nWidth_Interval += nWidth + nWidth_Offset;
            }
            
            // Time
            aSTable[nCnt] = new Ojw.SGridTable_t("Speed", nWidth, 0, -1, Color.OrangeRed, 1000, 1000);
            // Delay
            aSTable[nCnt + 1] = new Ojw.SGridTable_t("Delay", nWidth, 0, -1, Color.Olive, 0, 0);
            //// Time
            //aSTable[nCnt + 2] = new SGridTable_t("Time", nWidth, 0, -1, Color.LightBlue, 0);

            m_CGridMotion = new Ojw.CGridView();
            m_CGridMotion.Create(dgAngle, nLines, bEventSet, aSTable);

            //m_CGridMotion.SetIDs(m_C3d.m_lstIDs_Motion.ToArray<int>());
#if false
            //int nLine;
            //int nMotPos;
            for (int i = 0; i < dgAngle.RowCount; i++)
            {
                for (int j = 0; j < m_C3d.m_CHeader.nMotorCnt; j++)
                {
                    //nMotPos = j + 1;

                    m_pnFlag[i, j] = (int)(
                        0x10 | // Enable
                        ((m_C3d.m_CHeader.pSMotorInfo[j].nMotorControlType != 0) ? 0x08 : 0x00) // 위치제어가 아니라면 //0x08 //| // MotorType
                        //0x07 // Led
                        );
                }
            }
#endif


            if (bEventSet == true)
            {
#if true
                if (m_bGrid_Init == false)
                {
                    SetEvent();
                }
#else
                if (m_bGridInit == false)
                {
                    m_CGridMotion.Events_Remove_KeyDown();
                    m_CGridMotion.Events_Remove_KeyUp();
                    m_CGridMotion.GetHandle().CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(GridMotionEditor_Event_CellEnter);
                    m_CGridMotion.GetHandle().MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(GridMotionEditor_MouseDoubleClick);

                    m_CGridMotion.GetHandle().KeyDown += new System.Windows.Forms.KeyEventHandler(GridMotionEditor_Event_KeyDown);
                    m_CGridMotion.GetHandle().KeyUp += new System.Windows.Forms.KeyEventHandler(GridMotionEditor_Event_KeyUp);

                    m_CGridMotion.GetHandle().MouseDown += new System.Windows.Forms.MouseEventHandler(GridMotionEditor_MouseDown);
                    m_CGridMotion.GetHandle().MouseUp += new System.Windows.Forms.MouseEventHandler(GridMotionEditor_MouseUp);
                    m_CGridMotion.GetHandle().MouseMove += new System.Windows.Forms.MouseEventHandler(GridMotionEditor_MouseMove);

                    //m_CGridMotion.Ignore_CellEnter(true); // 속도 버벅거림을 없애기 위해 모션 수행시만 이게 들어간다.

                    m_CGridMotion.GetHandle().Scroll += new ScrollEventHandler(GridMotionEditor_Scroll);
                }
#endif
            }

            //m_nWidth_GridItem = nWidth;

            //CheckFlag(0);

            //m_CGridMotion.SetHeader(this);

            //m_bGridInit = true;
            m_C3d.m_CGridMotionEditor.m_nGridMode = 1;
        }

        private void Connect_Serial()
        {
            if (m_C3d.m_CMonster.IsOpen() == false)
            {
                int nPort = Ojw.CConvert.StrToInt(txtPort.Text);
                int nBaudrate = 0;
                if (cmbBaud.SelectedIndex < 0) cmbBaud.SelectedIndex = 5; // 1000000
                nBaudrate = Ojw.CConvert.StrToInt(cmbBaud.Text);

                if (m_C3d.m_CMonster.Open(nPort, nBaudrate) == true)
                {
                    btnConnect_Serial.Text = "Disconnect";
                    btnConnect.Enabled = false;
                }
            }
        }
        private void Disconnect()
        {
            m_C3d.m_CMonster.Close();
            btnConnect.Text = "Connect";
            btnConnect_Serial.Text = "Connect";
            btnConnect.Enabled = true;
            btnConnect_Serial.Enabled = true;
        }
        private void btnConnect_Serial_Click(object sender, EventArgs e)
        {
            if (m_C3d.m_CMonster.IsOpen() == false) //(btnConnect_Serial.Text == "Connect")
            {
                Connect_Serial();
            }
            else Disconnect();
        }

        private void dgGrid_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dgGrid.Focused == true)
            {
                if (m_C3d.IsNoLoadedModelingFile() == false)
                {

                    int nLine = dgGrid.CurrentCell.RowIndex;
                    int nCol = dgGrid.CurrentCell.ColumnIndex;
                    if ((nCol > 0) && (dgGrid.ColumnCount > 20) && (nCol - 1 < m_C3d.m_lstIDs_Motion.Count))//dgGrid.ColumnCount - 20))
                    {
                        m_C3d.m_CGridMotionEditor.SelectMotor(m_C3d.m_lstIDs_Motion[nCol - 1]);



                        // 모터의 인덱스를 모션의 인덱스 순으로 정렬한 리스트를 이용해 인덱스 접근 => m_C3d.m_lstIDs_Motion

                        Ojw.printf("Line={0}, Col={1}, MotorID = {2}", dgGrid.CurrentCell.RowIndex, dgGrid.CurrentCell.ColumnIndex, m_C3d.m_lstIDs_Motion[nCol - 1]);//dgGrid.ColumnCount);
                        Ojw.newline();

                    }


                    float fVal;
                    int nID;
                    for (int i = 0; i < m_C3d.m_lstIDs_Motion.Count; i++)
                    {
                        nID = m_C3d.m_lstIDs_Motion[i];
                        try
                        {
                            fVal = Convert.ToSingle(dgGrid.Rows[nLine].Cells[i + 1].Value);
                        }
                        catch
                        {
                            fVal = 0;
                        }
                        m_C3d.SetData(nID, fVal);
                    }

                }
            }
        }

        private void frmGridEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_C3d.m_CGridMotionEditor.m_nGridMode = 0;
        }
        private void PlayFrame(int nLine) { PlayFrame(nLine, false, false); }
        private void PlayFrame(int nLine, bool bIgnoreEnable, bool bTorqOn)
        {
            int nTime, nDelay;
            nTime = GetData_Time(nLine);
            nDelay = GetData_Delay(nLine);
            if (bTorqOn == true) m_C3d.m_CMonster.SetTorq(true);
            bool bPass = (bIgnoreEnable == true) ? true : Ojw.CConvert.IntToBool(GetData_Raw(nLine, 0));
            if (bPass == true)
            {
                int nID;
                for (int i = 0; i < m_C3d.m_lstIDs_Motion.Count; i++) 
                {
                    nID = m_C3d.m_lstIDs_Motion[i];
                    m_C3d.m_CMonster.SetParam(nID, (Ojw.CMonster2.EMonster_t)m_C3d.m_CHeader.pSMotorInfo[nID].nHwMotor_Index);
                    m_C3d.m_CMonster.SetParam(nID, m_C3d.m_CHeader.pSMotorInfo[nID].nMotorID, m_C3d.m_CHeader.pSMotorInfo[nID].nMotorDir, m_C3d.m_CHeader.pSMotorInfo[nID].nAxis_Mirror, m_C3d.m_CHeader.pSMotorInfo[nID].nProtocolVersion, m_C3d.m_CHeader.pSMotorInfo[nID].nSerialType, m_C3d.m_CHeader.pSMotorInfo[nID].nMechMove, m_C3d.m_CHeader.pSMotorInfo[nID].fMechAngle, m_C3d.m_CHeader.pSMotorInfo[nID].nCenter_Evd, m_C3d.m_CHeader.pSMotorInfo[nID].fRpm, (Ojw.CMonster2.EMonster_t)m_C3d.m_CHeader.pSMotorInfo[nID].nHwMotor_Index);
                    m_C3d.m_CMonster.Set(nID, GetData_With_ID(nLine, nID)); 
                }
                m_C3d.m_CMonster.Send_Motor(nTime);
                m_C3d.m_CMonster.Wait(nTime + nDelay);
            }
        }
        private void SelectMotor(int nLine, int nCol)
        {
            if ((nCol > 0) && (dgGrid.ColumnCount > 20) && (nCol - 1 < m_C3d.m_lstIDs_Motion.Count))
            {
                m_C3d.m_CGridMotionEditor.SelectMotor(m_C3d.m_lstIDs_Motion[nCol - 1]);
            }
        }
        private int GetCol(int nID)
        { 
            int nResult = -1;
            for (int i = 0; i < m_C3d.m_lstIDs_Motion.Count; i++)
            {
                if (m_C3d.m_lstIDs_Motion[i] == nID)
                {
                    nResult = i + 1;
                    break;
                }
            }
            return nResult;
            //return m_lstCols_Motion[nID];
            //int nID = m_C3d.m_lstIDs_Motion[i];
        }
        private int GetID(int nCol)
        {
            int nNum = nCol - 1;
            if ((nNum < 0) || (nNum >= m_C3d.m_lstIDs_Motion.Count)) return -1;
            return m_C3d.m_lstIDs_Motion[nNum];
        }


        #region GetData / SetData
        private float   GetData_With_Col(int nLine, int nCol) { return Convert.ToSingle(dgGrid.Rows[nLine].Cells[nCol].Value); }        
        private float   GetData_With_ID(int nLine, int nID) { int nCol = GetCol(nID); if (nCol < 0) return 0; return Convert.ToSingle(dgGrid.Rows[nLine].Cells[nCol].Value); }
        private void    SetData_With_Col(int nLine, int nCol, float fAngle) { if (nCol < 0) return; dgGrid.Rows[nLine].Cells[nCol].Value = fAngle; }
        private void    SetData_With_ID(int nLine, int nID, float fAngle) { int nCol = GetCol(nID); if (nCol < 0) return; dgGrid.Rows[nLine].Cells[nCol].Value = fAngle; }
        
        private int     GetData_Raw(int nLine, int nCol) { return Convert.ToInt32(dgGrid.Rows[nLine].Cells[nCol].Value); }
        private void    SetData_Raw(int nLine, int nCol, int nValue) { dgGrid.Rows[nLine].Cells[nCol].Value = nValue; }
        
        private int     GetData_Time(int nLine) { return GetData_Raw(nLine, m_C3d.m_lstIDs_Motion.Count + 1); }
        private void    SetData_Time(int nLine, int nTime) { SetData_Raw(nLine, m_C3d.m_lstIDs_Motion.Count + 1, nTime); }
        private int     GetData_Delay(int nLine) { return GetData_Raw(nLine, m_C3d.m_lstIDs_Motion.Count + 2); }
        private void    SetData_Delay(int nLine, int nDelay) { SetData_Raw(nLine, m_C3d.m_lstIDs_Motion.Count + 2, nDelay); }
        #endregion GetData / SetData

        private void dgGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgGrid.Focused == true)
            {
                if (m_C3d.IsNoLoadedModelingFile() == false)
                {
                    int nLine = dgGrid.CurrentCell.RowIndex;
                    int nCol = dgGrid.CurrentCell.ColumnIndex;
                    if ((nCol > 0) && (dgGrid.ColumnCount > 20) && (nCol - 1 < m_C3d.m_lstIDs_Motion.Count))//dgGrid.ColumnCount - 20))
                    {
                        PlayFrame(nLine, true, true);
                    }
                }
            }
        }

        private int m_nRunMode = 0;
        private void btnRun_Click(object sender, EventArgs e)
        {            
#if false // for test
            //int nLine = dgGrid.CurrentCell.RowIndex;
            int nCol = dgGrid.CurrentCell.ColumnIndex;
            //m_nTestCol = (m_nTestCol + 1) % 20;
            int nID = GetID(nCol);
            int nCol2 = GetCol(nID < 0 ? nCol : nID);
            MessageBox.Show(String.Format("{0} : ID = {1}, Col = {2}", nCol, nID, nCol2));
#endif
            m_nRunMode = 0;
            //btnRun.Enabled = false;
            Ojw.CTimer.Reset(); // Clear the Stop bit;

            //test
            //m_C3d.SetSimulation_With_PlayFrame(true);



            // 위치 데이타를 받고 시작한다.(다이나믹셀)
            m_C3d.SetFirstMoving(true);

            //m_thRun = new Thread(new ThreadStart(Run));
            //m_thRun.Start();
            tmrRun.Enabled = true;


            //btnRun.Enabled = true;
        }

        private bool m_btmrRun = false;
        private void tmrRun_Tick(object sender, EventArgs e)
        {
            if (m_bProgEnd == true) return;

            tmrRun.Enabled = false;

            if (m_btmrRun == true) return;
            m_btmrRun = true;

            if (m_nRunMode == 0)
            {
                Ojw.CTimer CTmr = new Ojw.CTimer();
                CTmr.Set();
                Run();
                Ojw.CMessage.Write("PlayTime = {0}", CTmr.Get());
            }
            //else if (m_nRunMode == 1)
            //{
            //    Cm550(true);
            //}
            //else if (m_nRunMode == 2)
            //{
            //    Cm550(false);
            //}



            m_btmrRun = false;
        }
        private void Run()
        {
            btnRun.Enabled = false;
            btnSimul.Enabled = false;
            
            //int nInterval = tmrDraw.Interval;
            //if (m_C3d.GetSimulation_With_PlayFrame() == true)
            //    tmrDraw.Interval = 40;
            ///////////////////////////////////////////
            m_C3d.m_CGridMotionEditor.GetHandle().Focus();
            m_C3d.Start_Set();
            StartMotion();
            m_C3d.Start_Reset();
            ///////////////////////////////////////////            
            //if (m_C3d.GetSimulation_With_PlayFrame() == true)
            //    tmrDraw.Interval = nInterval;
            m_C3d.SetSimulation_With_PlayFrame(false);
            btnRun.Enabled = true;
            btnSimul.Enabled = true;
        }
        private bool m_bStart = false;
        private bool m_bMotionEnd = false;
        private int m_nMotion_Step = 0;
        private void StartMotion()
        {
            #region Way 1
            if (m_bStart == true)
            {
                lbMotion_Message.Text = "Motion Running...";//Motion 운전 중입니다.";
                Ojw.CMessage.Write(lbMotion_Message.Text);
                return;
            }

            if (m_C3d.m_CMonster.m_bEms == true)
            {
                lbMotion_Message.Text = "Check you Emergency Status";//비상정지 알람이 켜져 있습니다.";
                Ojw.CMessage.Write(lbMotion_Message.Text);
                return;
            }
            if (m_C3d.GetSimulation_With_PlayFrame() == false)
            {
                if (m_C3d.m_CMonster.IsOpen() == false)
                {
                    lbMotion_Message.Text = "Serial Port & Socket Error - Not Connected.";
                    Ojw.CMessage.Write(lbMotion_Message.Text);
                    return;
                }
            }

            string strMessage = "";

            // Auto Save 기능을 정지해야 하는데 혹시 몰라 시간초기화도 한다.
            m_CTmr_AutoBackup.Set();
            m_CTmr_AutoBackup.Kill();

            
            //m_C3d.m_CMotor.SetAutoReturn(false);
            m_bStart = true;
            // 그리드 클릭 및 기타 이벤트 금지
            dgGrid.Enabled = false;
            //dgKinematics.Enabled = false;

            m_nMotion_Step = 0; // 메모리를 날린다. -> -_-

            m_bMotionEnd = false;
            m_C3d.m_CMotor.ResetStop();

            lbMotion_Counter.Text = "0";
            int nCnt = 0;
            int nLimitCount = Ojw.CConvert.StrToInt(txtMotionCounter.Text);

            //// Motion Start ////

            //m_C3d.m_CMotor.ResetStop();
            // Servo / Driver On
            m_C3d.m_CMonster.SetTorq(true);
            int nStep = m_nMotion_Step;
            //m_nLoop = 0;
            while (
                    ((nLimitCount <= 0) || (nLimitCount > nCnt)) &&
                    ((m_C3d.m_CMonster.m_bEms == false) && (m_C3d.m_CMonster.m_bStop == false)) &&
                        (m_bMotionEnd == false)
                    )
            {
                if (
                    (m_C3d.m_CMonster.m_bEms == false) && (m_C3d.m_CMonster.m_bStop == false)
                    )
                {
                    // 카운터 디스플레이
                    nCnt++;
                    lbMotion_Counter.Text = Ojw.CConvert.IntToStr(nCnt);

                    // 모션 실행
                    nStep = Motion(nStep);
                    //Motion();

                    // 잔여 Simulation
                    if (m_C3d.GetSimulation_With_PlayFrame() == true)
                    {
                        if (m_C3d.m_nSimulTime_For_Last > 0)
                        {
                            m_C3d.SetSimulation_SetCurrentData();

                            m_C3d.SetSimulation_Calc(m_C3d.m_nSimulTime_For_Last, 1);
                            WaitAction_ByTimer(m_C3d.m_nSimulTime_For_Last);
                            for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) m_C3d.SetData(i, m_C3d.GetSimulation_Value_Next(i));
                        }
                    }

                    if ((nLimitCount <= 0) || (nLimitCount > nCnt))
                    {

                    }
                    //else nStep = 0;
                    //m_nLoop++;
                }
            }
            m_nMotion_Step = nStep;

            //// Motion End ////
            #region 종료처리
            if (m_C3d.m_CMonster.m_bEms == true)
            {
                lbMotion_Status.Text = "비상정지";
            }
            else if (m_C3d.m_CMonster.m_bStop == true)
            {
                strMessage = "Motion Stop";
                lbMotion_Status.Text = "일시정지";
            }
            else
            {
                strMessage = "Motion 완료";
                lbMotion_Status.Text = "Ready";
            }
            #endregion 종료처리

            if (strMessage != "")
            {
                lbMotion_Message.Text = strMessage;
            }

            m_bStart = false;
            // 타이머 기능 복원
            //tmrDraw.Enabled = true;
            //tmrCheckMotor.Enabled = true;

            // 다시 Auto Save 를 활성화 한다.
            m_CTmr_AutoBackup.Set();

            // 그리드 클릭 및 기타 이벤트 금지 해제
            dgGrid.Enabled = true;

            //m_C3d.m_CMotor.SetAutoReturn(true);
            #endregion Way 1
            
        }
        private int Motion(int nLine)
        {
            #region Dynamixel
            int nResult = 0;
            int temp_Line = 0;

            int nSize = dgGrid.RowCount;

            bool bAll = true;
            bool[] abSelected = new bool[nSize];
            abSelected.Initialize();
            int nTmpPos = 0;
            int nTmpCnt = 0;
            for (int i = 0; i < nSize; i++)
            {
                if ((m_C3d.GridMotionEditor_GetEnable(i) == true) && (dgGrid[0, i].Selected == true))
                {
                    bAll = false;
                    abSelected[i] = true;
                }

                for (int j = 0; j < dgGrid.ColumnCount; j++)
                {
                    if (dgGrid[j, i].Selected == true)
                    {
                        // 선택한 라인의 갯수를 셈 - 멀티선택인지, 단일선택인지 구분 가능
                        nTmpPos = i;
                        nTmpCnt++;
                        break;
                    }
                }
            }

            //int nFirstLine = ((nTmpCnt == 1) && (chkMp3.Checked == true)) ? nTmpPos : 0; // 다중선택이면서 음원과 싱크를 맞춰 출력하려면...
            //if (m_nLoop > 0)
            int nFirstLine = 0;

            WaitAction_SetTimer();
            for (int i = nFirstLine; i < nSize; i++)
            {
                if (m_C3d.GridMotionEditor_GetEnable(i) == false) continue;
                if ((bAll == false) && (abSelected[i] == false)) continue;
                if (nLine == temp_Line)
                {
                    if ((m_C3d.GridMotionEditor_GetCommand(i) == 1) || ((m_C3d.GridMotionEditor_GetCommand(i) >= 3) && (m_C3d.GridMotionEditor_GetCommand(i) <= 5))) // 반복문은 1, 3, 4, 5 가 있다.
                    {
                        int nFirst = i;
                        int nLast = (int)m_C3d.GridMotionEditor_GetData0(i);
                        int nRepeat = (int)m_C3d.GridMotionEditor_GetData1(i);
                        for (int k = 0; k < nRepeat; k++)
                        {
                            if (k >= nRepeat - 1) nLast = nFirst;
                            for (int j = nFirst; j <= nLast; j++)
                            {
                                if (m_C3d.GridMotionEditor_GetEnable(j) == false) continue;
                                if (nLine == temp_Line)
                                {
                                    PlayFrame(j);
                                    nLine++;
                                }
                                temp_Line++;
                                if (
                                    ((m_C3d.GetSimulation_With_PlayFrame() == false) && (m_C3d.m_CMonster.IsOpen() == false)) ||
                                    ((m_C3d.m_CMonster.m_bEms == true) || (m_C3d.m_CMonster.m_bStop == true)) ||
                                    (m_bStart == false)
                                    )
                                    return (temp_Line - 1);
                            }
                        }
                    }
                    else
                    {
                        PlayFrame(i);
                    }
                    nLine++;
                }
                temp_Line++;
                if (
                    ((m_C3d.GetSimulation_With_PlayFrame() == false) && (m_C3d.m_CMonster.IsOpen() == false)) ||
                    ((m_C3d.m_CMonster.m_bEms == true) || (m_C3d.m_CMonster.m_bStop == true)) ||
                    (m_bStart == false)
                    )
                    return (temp_Line - 1);

                Application.DoEvents();
            }

            if (nLine == temp_Line)
            {
                //// 동작 ////


                //////////////
                nLine++;
            }
            temp_Line++;
            return nResult;
            
            #endregion Dynamixel
        }
        private void WaitAction_SetTimer() { m_C3d.WaitAction_SetTimer(); return; }
        private bool WaitAction_ByTimer(long t) { return m_C3d.WaitAction_ByTimer(t); }

        private void btnSimul_Click(object sender, EventArgs e)
        {
            m_nRunMode = 0;
            Ojw.CTimer.Reset();
            m_C3d.SetSimulation_Smooth(chkSmooth.Checked);
            m_C3d.SetSimulation_With_PlayFrame(true);
            tmrRun.Enabled = true;
        }

        private void btnValueIncrement_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Plus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueDecrement_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Minus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueMul_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Mul, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueDiv_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Div, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }
        
        private void btnValueStackIncrement_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Inc, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueStackDecrement_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Dec, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueChange_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Change, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnValueFlip_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Flip_Value, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnFlip_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Flip_Position, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnInterpolation_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Interpolation, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnInterpolation2_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._S_Curve, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnPercent_Click(object sender, EventArgs e)
        {
            DelayChange();
        }
        public void DelayChange()
        {
            int i;
            for (i = 0; i < dgGrid.RowCount; i++)
            {
                int nData = (int)Math.Abs(m_C3d.GridMotionEditor_GetTime(i));
                int nDelayValue = m_C3d.GridMotionEditor_GetDelay(i);
                int nPercenct = Ojw.CConvert.StrToInt(txtPercent.Text);
                if ((nDelayValue <= 0) && (nPercenct < 100))
                {
                    float fData0 = (float)nData;
                    float fData1 = (float)nPercenct;

                    nData = -(int)Math.Round((fData0 * ((100.0 - fData1) / 100.0)));
                    m_C3d.GridMotionEditor_SetDelay(i, nData);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Clear();
        }

        private void btnX_Plus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._X_Plus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnX_Minus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._X_Minus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnX_Input_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._X_Input, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnY_Plus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Y_Plus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnY_Minus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Y_Minus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnY_Input_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Y_Input, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnZ_Plus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Z_Plus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnZ_Minus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Z_Minus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnZ_Input_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Z_Input, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnGroup1_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_SetSelectedGroup(1);
        }

        private void btnGroup2_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_SetSelectedGroup(2);
        }

        private void btnGroup3_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_SetSelectedGroup(3);
        }

        private void btnGroupDel_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_SetSelectedGroup(0);
        }

        private void btnCmd_Repeat_Click(object sender, EventArgs e)
        {
            String strValue = "1";
            if (Ojw.CInputBox.Show("Repeat Count", "Set the Repeat Count", ref strValue) == DialogResult.OK)
            {
                int nCnt = Ojw.CConvert.StrToInt(strValue);

                int nX_Limit = dgGrid.RowCount;
                int nY_Limit = dgGrid.ColumnCount;
                int nPos = nX_Limit;
                int nLineNumber = 0;
                for (int i = 0; i < nX_Limit; i++)
                {
                    for (int j = 0; j < nY_Limit; j++)
                    {
                        if (dgGrid[j, i].Selected == true)
                        {
                            if (i < nPos) nPos = i;
                            if (i > nLineNumber) nLineNumber = i;
                            break;
                        }
                    }
                }
                m_C3d.m_CGridMotionEditor.SetData0(nPos, nLineNumber);
                m_C3d.m_CGridMotionEditor.SetData1(nPos, nCnt);
                m_C3d.m_CGridMotionEditor.SetCommand(nPos, 1);

                // 색칠하기...
                m_C3d.GridMotionEditor_SetSelectedGroup(4);
                //m_C3d.m_CGridMotionEditor.SetColorGrid(0, dgGrid.RowCount);
            }
        }
        // 파일의 경로
        private String m_strWorkDirectory_Dmt = String.Empty;
        private String m_strWorkDirectory_Mp3 = String.Empty;
        private bool SetDirectory(OpenFileDialog OpenFileDlg, String strDir)
        {
            try
            {
                //Directory.SetCurrentDirectory(strDir);
                OpenFileDlg.InitialDirectory = strDir;
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool m_bModify = false;
        private void Modify(bool bModify)
        {
            if ((bModify == true) && (m_bModify != bModify)) m_CTmr_AutoBackup.Set();
            m_bModify = bModify;
            lbModify.ForeColor = (bModify == true) ? Color.Red : Color.Green;
            lbModify.Text = (bModify == true) ? "Keep modifying..." : "Done";
        }
        private void btnMotionFileOpen_Click(object sender, EventArgs e)
        {
            if (m_bModify == true)
            {
                DialogResult dlgRet = MessageBox.Show("You have not yet saved the file. The data will be lost.\r\n\r\nDo you still want to open the file?", "File Open", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dlgRet != DialogResult.OK)
                {
                    return;
                }
            }
            m_C3d.m_CGridMotionEditor.SetSelectedGroup(0);
            OpenFileDialog ofdMotion = new OpenFileDialog();
            string strExe = "ojwm";
            ofdMotion.FileName = "*." + strExe;
            ofdMotion.Filter = string.Format("모션 파일(*.{0})|*.{0}", strExe);
            ofdMotion.DefaultExt = strExe;
            SetDirectory(ofdMotion, m_strWorkDirectory_Dmt);
            if (ofdMotion.ShowDialog() == DialogResult.OK)
            {
                String fileName = ofdMotion.FileName;
                //m_strWorkDirectory_Dmt = Directory.GetCurrentDirectory();
                m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(fileName);
                if (m_strWorkDirectory_Dmt == null) m_strWorkDirectory_Dmt = Application.StartupPath;

                txtFileName.Text = fileName;


                if (m_C3d.DataFileOpen(false, fileName, null) == false)
                {
                    MessageBox.Show(ofdMotion.DefaultExt.ToUpper() + " 모션 파일이 아닙니다.");
                }
                else
                {
                    Modify(false);
#if false
                    Grid_DisplayTime();

                    txtComment.Text = m_C3d.GetMotionFile_Comment();
                    txtTableName.Text = m_C3d.GetMotionFile_Title();
                    cmbStartPosition.SelectedIndex = m_C3d.GetMotionFile_StartPosition();
#endif
                }
            }

            //m_strWorkDirectory_Dmt = Directory.GetCurrentDirectory();
            //WriteRegistry_Path(m_strWorkDirectory);
            ofdMotion.Dispose();
        }

        private Ojw.CTimer m_CTmr_AutoBackup = new Ojw.CTimer();
        private void tmrBack_Tick(object sender, EventArgs e)
        {
#if false
            if ((m_CTmr_AutoBackup.Get() > 30000) && (m_C3d.GetFileName() != null) && (m_bStart == false)) // 5분에 한번 저장 -> 30초로 변경
            {
                int nVer = _V_10;//((chkFileVersionForSave.Checked == true) ? _V_11 : ((chkFileVersionForSave_1_0.Checked == true) ? _V_10 : _V_12));
                m_C3d.BinaryFileSave(chkSaveAngle.Checked, nVer, Application.StartupPath + Ojw.C3d._STR_BACKUP_FILE, false, false);

                // 혹시 모르니 파라미터도 같이 저장하자.
                SaveParam();

                m_CTmr_AutoBackup.Set();
            }
#endif
        }

        private void btnTextSave_Click(object sender, EventArgs e)
        {
            //if (chkRmt.Checked == true)
            //{
            //    m_C3d.RmtFileSave(GetMotionFileName(txtFileName.Text));//, false);
            //}
            //else if (chkSaveArduino.Checked == true)
            //{
            //    m_C3d.ArduinoFileSave(GetMotionFileName(txtFileName.Text));//, false);
            //}
            //else
            //    m_C3d.BinaryFileSave(chkSaveAngle.Checked, _V_10, GetMotionFileName(txtFileName.Text), false);

            //m_C3d.BinaryFileSave(chkSaveAngle.Checked, _V_10, GetMotionFileName(txtFileName.Text), false);

            //m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(txtFileName.Text);
            //m_CTmr_Save.Set();  
        }

        private void btnBinarySave_Click(object sender, EventArgs e)
        {
            //if (chkRmt.Checked == true)
            //{
            //    m_C3d.RmtFileSave(GetMotionFileName(txtFileName.Text));//, true);
            //}
            //else if (chkSaveArduino.Checked == true)
            //{
            //    m_C3d.ArduinoFileSave(GetMotionFileName(txtFileName.Text));//, false);
            //}
            //else
            //    m_C3d.BinaryFileSave(chkSaveAngle.Checked, _V_10, GetMotionFileName(txtFileName.Text), true);
            //m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(txtFileName.Text);
            //m_CTmr_Save.Set(); 
        }
    }
}
