//#define _ENABLE_MEDIAPLAYER
//#define _VIEW_FRMFOLDER
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenJigWare;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

//#if _ENABLE_MEDIAPLAYER
//
#if false
도구 -> 도구상자 항목선택 -> COM 구성요소 -> Windows Media Player   : wmp.dll
Tools-> Choose Item -> Com -> Windows media player    : wmp.dll
#endif

//#endif

namespace OpenJigWare.Docking
{    
    public partial class frmMotionEditor : Form
    {
        public frmMotionEditor()
        {
            InitializeComponent();
//#if _ENABLE_MEDIAPLAYER

//#endif
        }

        #region UserFunction
        public delegate void UserFunction();
        private UserFunction m_FUser;
        public Form GetForm() { return this; }
        public int GetPort() { return Ojw.CConvert.StrToInt(txtPort.Text); }
        public int GetBaudrate() { return Ojw.CConvert.StrToInt(txtBaudrate.Text); }
        public Ojw.C3d GetC3d() { return m_C3d; }
        public void SetUserButton(UserFunction FUser) { if (FUser != null) { m_FUser = new UserFunction(FUser); btnUserButton.Visible = true; } }
        #endregion UserFunction

        //private Ojw.COjwMotor m_CMotor = new Ojw.COjwMotor();
        
        private Ojw.C3d m_C3d = new Ojw.C3d();
        private bool m_bLoadedForm = false;

        private Ojw.CFile m_CFile = new Ojw.CFile();
        private string m_strTitle = "Motion Editor";
        
        private bool m_bOneShotMp3Command = false;

        // 파일의 경로
        private String m_strWorkDirectory_Dmt = String.Empty;
        private String m_strWorkDirectory_Mp3 = String.Empty;
        
        public void SetTitleImage(Bitmap bmp, Rectangle rc) 
        {
            if (bmp == null)
            {
                picIcon.Visible = false;
                lbTitle.Left = m_nLeft_Title;
                lbTitle.Top = m_nTop_Title;
            }
            else
            {
                if (rc != null)
                {
                    int nLeft = (rc.Left == -1) ? picIcon.Left : rc.Left;
                    int nTop = (rc.Top == -1) ? picIcon.Top : rc.Top;
                    int nWidth = (rc.Width == 0) ? picIcon.Width : rc.Width;
                    int nHeight = (rc.Height == 0) ? picIcon.Height : rc.Height;
                    picIcon.Visible = true;
                    picIcon.Location = new Point(nLeft, nTop);
                    picIcon.Size = new Size(nWidth, nHeight);
                }
                lbTitle.Left = picIcon.Left + picIcon.Width + 10;
                picIcon.BackgroundImage = bmp;
            }
        }

        #region Treeview
        ////// treeview 탐색기 -- ///////
        // 프로그램 실행 API
        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        public static extern int ShellExecute(int hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);
        public const int SW_SHOWNORMAL = 1;

        frmFolder m_frmFolder;// 폴더 탐색 정보를 출력

        public struct SFileHeader_t
        {
            public String strFileVersion;
            public String strFileName;
            public bool bActFile;  // 액션 파일 여부
            public String strTitle;
            public String strComment;
            public int nStartPosition;
            public int nFrameSize;
            public int nFrameSize_Sound;
            public int nFrameSize_Emoticon;
        };
        SFileHeader_t[] m_pSFileHeader;
        public int m_nFileHeader;
        public int m_nDirCnt;

        // 리스트뷰에 폴더및 파일정보 삽입
        private void SetListView(String strPath)
        {
            string strFilter = "";
            string[] pstrFilter;
            // 동영상
            // avi,asf,asx,wpl,wm,wmx,wmd,wmz,wmv,wav,mpeg,mpg,mpe,m1v,m2v,mod,mp2,mpv2,mp2v,mpa
            strFilter = ".avi,.asf,.asx,.wpl,.wm,.wmx,.wmd,.wmz,.wmv,.wav,.mpeg,.mpg,.mpe,.m1v,.m2v,.mod,.mp2,.mpv2,.mp2v,.mpa";
            // 오디오
            // wma,wax,cda,mp3,m3u,mid,midi,rmi,air,aifc,aiff,au,snd
            strFilter += ",.wma,.wax,.cda,.mp3,.m3u,.mid,.midi,.rmi,.air,.aifc,.aiff,.au,.snd";
            // 파일
            strFilter += ",.dmt,.ojw,.dfh";
            pstrFilter = strFilter.Split(',');
            SetListView(strPath, pstrFilter);
            //SetListView(strPath, "*.*");
        }

        private bool CheckAudioExist(String strData)
        {
            string strFilter = ".wma,.wax,.cda,.mp3,.m3u,.mid,.midi,.rmi,.air,.aifc,.aiff,.au,.snd";
            string[] pstrFilter;
            pstrFilter = strFilter.Split(',');
            return CheckFileExist(strData, pstrFilter);
        }

        private bool CheckMovieExist(String strData)
        {
            string strFilter = ".avi,.asf,.asx,.wpl,.wm,.wmx,.wmd,.wmz,.wmv,.wav,.mpeg,.mpg,.mpe,.m1v,.m2v,.mod,.mp2,.mpv2,.mp2v,.mpa";
            string[] pstrFilter;
            pstrFilter = strFilter.Split(',');
            return CheckFileExist(strData, pstrFilter);
        }

        private bool CheckDmtExist(String strData)
        {
            if (strData.ToLower().IndexOf(".dmt") == (strData.Length - 4)) return true;
            return false;
        }

        private bool CheckFileExist(String strData, String[] pstrFilter)
        {
            bool bFind = false;
            foreach (string strItem in pstrFilter)
            {
                if (strData.IndexOf(strItem) == (strData.Length - 4))
                {
                    bFind = true;
                    break;
                }
            }
            return bFind;
        }

        private void SetListView(String strPath, String[] pstrFilter)
        {
            SystemImgList sysimglst = new SystemImgList();
            Icon icoParent;
            int nCnt;

            strPath = strPath + "\\";

            imglstSmall.Images.Clear();
            imglstBig.Images.Clear();
            lstInfo.Items.Clear();
            lstInfo.Sorting = SortOrder.None;

            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(strPath);

            //frmFolder에 출력하기 위해서 strPath의 icon을 얻는다. 
            icoParent = sysimglst.GetIcon(strPath, false, false);

            CallFrmFolder(strPath, icoParent);

            try
            {
                int nCnt2 = 0;
                nCnt = 0;
                // 폴더 정보를 얻기       
#if true
                foreach (System.IO.DirectoryInfo dirInfoSub in dirInfo.GetDirectories("*"))
                {
                    // 리스트뷰에 입력
                    imglstSmall.Images.Add(sysimglst.GetIcon(dirInfoSub.FullName, true, false));
                    imglstBig.Images.Add(sysimglst.GetIcon(dirInfoSub.FullName, false, false));
                    nCnt = imglstSmall.Images.Count;

                    lstInfo.Items.Add(dirInfoSub.Name, nCnt - 1);
                    lstInfo.Items[nCnt - 1].SubItems.Add("");
                    lstInfo.Items[nCnt - 1].SubItems.Add(dirInfoSub.LastWriteTime.ToString());

                    lstInfo.Update();//Application.DoEvents();
                    
                    ViewFrmFolder(dirInfoSub.Name, imglstBig.Images[nCnt - 1]);
                    nCnt++;
                }
#else
                lstInfoFolder.Sorting = SortOrder.None;
                lstInfoFolder.Clear();
                foreach (System.IO.DirectoryInfo dirInfoSub in dirInfo.GetDirectories("*"))
                {
                    // 리스트뷰에 입력
                    imglstSmall.Images.Add(sysimglst.GetIcon(dirInfoSub.FullName, true, false));
                    imglstBig.Images.Add(sysimglst.GetIcon(dirInfoSub.FullName, false, false));
                    nCnt = imglstSmall.Images.Count;

                    lstInfoFolder.Items.Add(dirInfoSub.Name, nCnt - 1);
                    lstInfoFolder.Items[nCnt - 1].SubItems.Add("");
                    lstInfoFolder.Items[nCnt - 1].SubItems.Add(dirInfoSub.LastWriteTime.ToString());

                    Application.DoEvents();
                    ViewFrmFolder(dirInfoSub.Name, imglstBig.Images[nCnt - 1]);
                    nCnt++;
                }
                lstInfoFolder.Sorting = SortOrder.Ascending;
#endif
                m_nDirCnt = nCnt;
                nCnt2 = 0;
                foreach (System.IO.FileInfo fileInfo in dirInfo.GetFiles("*.*")) nCnt2++;

                m_nFileHeader = nCnt2;// dirInfo.GetFiles().GetLength();
                nCnt2 = 0;
                m_pSFileHeader = new SFileHeader_t[m_nFileHeader];
                // 파일 정보(리스트뷰 입력)
                foreach (System.IO.FileInfo fileInfo in dirInfo.GetFiles("*.*"))
                {
                    if (CheckFileExist(fileInfo.FullName, pstrFilter) == true)
                    {
                        if (CheckDmtExist(fileInfo.FullName))
                        {
                            //String strTitle, strComment, strFileVersion;
                            //int nTmp;
                            //int nStartPosition, nFrameSize, nFrameSize_Sound, nFrameSize_Emoticon;

                            Ojw.SMotion_t SMotion = new Ojw.SMotion_t();
                            if (m_C3d.BinaryFileOpen(fileInfo.FullName, out SMotion) == true)
                            {
                                //nTmp = 1;// 3 + nStartPosition * 2;
                                //imglstSmall.Images.Add(imageList1.Images[nTmp]);//3 + nStartPosition * 2]);
                                //nTmp = 1;// 4 + nStartPosition * 2;
                                //imglstBig.Images.Add(imageList1.Images[nTmp]);//4 + nStartPosition * 2]);
                                imglstSmall.Images.Add(sysimglst.GetIcon(fileInfo.FullName, true, false));
                                imglstBig.Images.Add(sysimglst.GetIcon(fileInfo.FullName, false, false));

                                m_pSFileHeader[nCnt2].strFileName = fileInfo.Name;

                                m_pSFileHeader[nCnt2].bActFile = true;
                                m_pSFileHeader[nCnt2].strTitle = SMotion.strTableName;
                                m_pSFileHeader[nCnt2].strComment = SMotion.strComment;
                                m_pSFileHeader[nCnt2].nStartPosition = SMotion.nStartPosition;
                                m_pSFileHeader[nCnt2].nFrameSize = SMotion.nFrameSize;
                                m_pSFileHeader[nCnt2].nFrameSize_Sound = 0;//SMotion.nFrameSize_Sound;
                                m_pSFileHeader[nCnt2].nFrameSize_Emoticon = 0;//SMotion.nFrameSize_Emoticon;
                                m_pSFileHeader[nCnt2].strFileVersion = SMotion.strVersion;
                            }
                            else
                            {
                                imglstSmall.Images.Add(sysimglst.GetIcon(fileInfo.FullName, true, false));
                                imglstBig.Images.Add(sysimglst.GetIcon(fileInfo.FullName, false, false));
                                //imglstSmall.Images.Add(sysimglst.GetIcon(fileInfo.FullName, true, false));
                                //imglstBig.Images.Add(sysimglst.GetIcon(fileInfo.FullName, false, false));
                                m_pSFileHeader[nCnt2].bActFile = false;
                            }
                        } 
                        // 디자인 파일 아이콘
                        else if (
                        (fileInfo.FullName.ToLower().IndexOf(".ojw") > 0) ||
                        (fileInfo.FullName.ToLower().IndexOf(".dhf") > 0)
                        )
                        {
                            imglstSmall.Images.Add(sysimglst.GetIcon(fileInfo.FullName, true, false));
                            imglstBig.Images.Add(sysimglst.GetIcon(fileInfo.FullName, false, false));
                            //m_pSFileHeader[nCnt2].bActFile = false;
                        }
                        else
                        {
                            imglstSmall.Images.Add(sysimglst.GetIcon(fileInfo.FullName, true, false));
                            imglstBig.Images.Add(sysimglst.GetIcon(fileInfo.FullName, false, false));
                            //imglstSmall.Images.Add(sysimglst.GetIcon(fileInfo.FullName, true, false));
                            //imglstBig.Images.Add(sysimglst.GetIcon(fileInfo.FullName, false, false));
                            m_pSFileHeader[nCnt2].bActFile = false;
                        }
                        nCnt = imglstSmall.Images.Count;

                        lstInfo.Items.Add(fileInfo.Name, nCnt - 1);
#if true
                        lstInfo.Items[nCnt - 1].SubItems.Add(fileInfo.Length.ToString());
                        lstInfo.Items[nCnt - 1].SubItems.Add(fileInfo.LastWriteTime.ToString());
#else
                        ListViewItem lstviewItem = lstInfo.FindItemWithText(fileInfo.Name);//.Find(fileInfo.Name, false);
                        lstviewItem.SubItems.Add(fileInfo.Length.ToString());
                        lstviewItem.SubItems.Add(fileInfo.LastWriteTime.ToString());
#endif
                        nCnt2++;
                        lstInfo.Update();//Application.DoEvents();
                        ViewFrmFolder(fileInfo.Name, imglstBig.Images[nCnt - 1]);
                    }
                }

                //for (int i = 0; i < m_nDirCnt; i++)
                //{

                //}
                m_nFileHeader = nCnt2;
            }
            catch (Exception e)
            {
                KillFrmFolder();
                MessageBox.Show(e.ToString());
            }
            lstInfo.Sorting = SortOrder.Ascending;

            KillFrmFolder();

            lstInfo.SmallImageList = imglstSmall;
            lstInfo.LargeImageList = imglstBig;
        }

        // node에 하위 폴더 삽입
        private void SetTreeNode(TreeNode node)
        {
            String strPath;
            int nNode;
            SystemImgList sysimglst = new SystemImgList();

            strPath = node.FullPath;
            strPath = strPath + "\\";

            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(strPath);

            // 폴더 정보
            try
            {
                // 폴더 정보를 얻기
                List<String> strList = new List<String>();
                strList.Clear();
                foreach (System.IO.DirectoryInfo dirInfoSub in dirInfo.GetDirectories("*")) strList.Add(dirInfoSub.Name);
                strList.Sort();

                //foreach (System.IO.DirectoryInfo dirInfoSub in dirInfo.GetDirectories("*"))
                foreach (string strInfoSub in strList)
                {
                    nNode = node.Nodes.Add(new TreeNode(strInfoSub, imglstTree.Images.Count - 2, imglstTree.Images.Count - 1));
                    if (HasSubFolder(node.Nodes[nNode].FullPath))
                        node.Nodes[nNode].Nodes.Add("???");

                }
            }
            catch (Exception e)
            {
                KillFrmFolder();
                MessageBox.Show(e.ToString());
            }

        }

        // 트리 노드 찾기
        // 현재 선택된 노드의 자식들중에서 찾는다.
        private TreeNode FindNode(String strFolder)
        {

            TreeNode tnTmp;

            // 선택된 노드의 최초 자식을 찾는다.
            tnTmp = treeInfo.SelectedNode.FirstNode;

            while (tnTmp != null)
            {
                if (tnTmp.Text.Equals(strFolder)) return tnTmp;

                tnTmp = tnTmp.NextNode;
            }

            return null;
        }

        private const string _C_DRV_ = "C:";

        // 하위 폴더가 있는지 검사한다.
        private bool HasSubFolder(String strPath)
        {
            strPath = strPath + "\\";

            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(_C_DRV_ + "\\");

            try
            {
                foreach (System.IO.DirectoryInfo dirInfoSub in dirInfo.GetDirectories("*"))
                {
                    String strTmp = dirInfoSub.Name;
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        // 폴더 정보를 출력하는 폼을 생성한다.
        private void CallFrmFolder(String strParent, Icon icoParent)
        {
#if _VIEW_FRMFOLDER
            // 이미 출력된 상태면 
            if (m_frmFolder != null) return;

            // 출력
            m_frmFolder = new frmFolder();
            m_frmFolder.Show();
            m_frmFolder.CenterParentFrm(this);

            //경로입력
            m_frmFolder.lblParentPath.Text = strParent;
            //아이콘입력
            m_frmFolder.picParent.Image = Bitmap.FromHicon(icoParent.Handle);
#endif
        }

        // 폴더 정보를 출력하는 폼을 제거한다.
        private void KillFrmFolder()
        {
#if _VIEW_FRMFOLDER
            // 출력되기 전이라면 
            if (m_frmFolder == null) return;

            // 제거
            m_frmFolder.Close();
            m_frmFolder.Dispose();
            m_frmFolder = null;
#endif
        }

        // 폼에 정보를 출력한다.
        private void ViewFrmFolder(String strInfo, Image imgInfo)
        {
#if _VIEW_FRMFOLDER
            // 출력되기 전이라면 
            if (m_frmFolder == null) return;

            // 발견된 파일(폴더)이름
            m_frmFolder.lblInfo.Text = strInfo;
            // 이미지
            m_frmFolder.picInfo.Image = imgInfo;
#endif
        }
        ////// -- treeview 탐색기 ///////
        #endregion Treeview
        private float m_fScale = 1.0f;
        private int m_nLeft_Title = 0;
        private int m_nTop_Title = 0;
        private void frmMotionEditor_Load(object sender, EventArgs e)
        {
            m_strTitle = String.Format("{0} - Open Jig Ware Ver [{1}]", m_strTitle, SVersion_T.strVersion);
            lbTitle.Text = m_strTitle;
            m_nLeft_Title = lbTitle.Left;
            m_nTop_Title = lbTitle.Top;

            Ojw.CTimer.Init(100);
            // 이것만 선언하면 기본 선언은 끝.
            m_C3d.Init(picDisp);
            // 캐드파일의 경로를 임의의 경로로 바꾼다. -> 안 정하면 실행파일과 같은 경로
            m_C3d.SetAseFile_Path("ase");

            Ojw.CMessage.Init(txtMessage);

            // property window
            m_C3d.CreateProb_VirtualObject(pnProperty);

            // Add User Function
            m_C3d.AddMouseEvent_Down(OjwMouseDown);
            m_C3d.AddMouseEvent_Move(OjwMouseMove);
            m_C3d.AddMouseEvent_Up(OjwMouseUp);
            m_C3d.Prop_Set_Main_MouseControlMode(0);
            m_C3d.GridMotionEditor_Init(dgAngle, 40, 999, true);
            m_C3d.GridMotionEditor_Init_Panel(pnButton);
            //dgAngle.Scale(new SizeF(0.5f, 0.5f));
            m_C3d.SelectMotor_Sync_With_Mouse(true);

            if (m_CFile.Load(100, Application.StartupPath + _STR_FILENAME) > 0)
            {
                int i = 0;
                bool bFile = false;
                if (m_CFile.GetData_String(i).IndexOf(".ojw") > 0)
                {
                    if (m_C3d.FileOpen(m_CFile.GetData_String(i)) == true) // 모델링 파일이 잘 로드 되었다면 
                    {
                        Ojw.CMessage.Write("File Opened");
                        bFile = true;

                        //cmbVersion.SelectedIndex = m_C3d.m_strVersion - 11;
                        float[] afData = new float[3];
                        m_C3d.GetPos_Display(out afData[0], out afData[1], out afData[2]);
                        //int i = 0;
                        //txtDisplay_X.Text = Ojw.CConvert.FloatToStr(afData[i++]);
                        //txtDisplay_Y.Text = Ojw.CConvert.FloatToStr(afData[i++]);
                        //txtDisplay_Z.Text = Ojw.CConvert.FloatToStr(afData[i++]);
                        m_C3d.GetAngle_Display(out afData[0], out afData[1], out afData[2]);
                        //i = 0;                                    

                        m_C3d.m_strDesignerFilePath = Ojw.CFile.GetPath(m_CFile.GetData_String(i));


                        // File Restore
                        m_C3d.FileRestore();
                    }
                }
                i++;

                txtPort.Text = m_CFile.GetData_String(i++);
                txtBaudrate.Text = m_CFile.GetData_String(i++);
                txtIp.Text = m_CFile.GetData_String(i++);
                txtSocket_Port.Text = m_CFile.GetData_String(i++);
                chkFreeze_X.Checked = m_CFile.GetData_Bool(i++);
                chkFreeze_Y.Checked = m_CFile.GetData_Bool(i++);
                chkFreeze_Z.Checked = m_CFile.GetData_Bool(i++);
                chkFreeze_Pan.Checked = m_CFile.GetData_Bool(i++);
                chkFreeze_Tilt.Checked = m_CFile.GetData_Bool(i++);
                chkFreeze_Swing.Checked = m_CFile.GetData_Bool(i++);
                txtSocket_Port.Text = m_CFile.GetData_String(i++);
                txtMotionCounter.Text = m_CFile.GetData_String(i++);
                txtChangeValue.Text = m_CFile.GetData_String(i++);
                txtPercent.Text = m_CFile.GetData_String(i++);
                txtBackAngle_X.Text = m_CFile.GetData_String(i++);
                txtBackAngle_Y.Text = m_CFile.GetData_String(i++);
                txtBackAngle_Z.Text = m_CFile.GetData_String(i++);
                chkTracking.Checked = m_CFile.GetData_Bool(i++);
                chkMp3.Checked = m_CFile.GetData_Bool(i++);
                txtMp3TimeDelay.Text = m_CFile.GetData_String(i++);
                chkFullSize.Checked = m_CFile.GetData_Bool(i++);
                chkDualMonitor.Checked = m_CFile.GetData_Bool(i++);
                txtID_FR.Text = m_CFile.GetData_String(i++);
                txtID_FL.Text = m_CFile.GetData_String(i++);
                txtID_RR.Text = m_CFile.GetData_String(i++);
                txtID_RL.Text = m_CFile.GetData_String(i++);

                txtID_0.Text = m_CFile.GetData_String(i++);
                txtID_1.Text = m_CFile.GetData_String(i++);
                txtID_2.Text = m_CFile.GetData_String(i++);

                //m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(m_CFile.GetData_String(i++));
                //m_strWorkDirectory_Mp3 = Ojw.CFile.GetPath(m_CFile.GetData_String(i++));
                m_strWorkDirectory_Dmt = m_CFile.GetData_String(i++);
                m_strWorkDirectory_Mp3 = m_CFile.GetData_String(i++);

                if (m_strWorkDirectory_Dmt == null) m_strWorkDirectory_Dmt = Application.StartupPath;
                if (m_strWorkDirectory_Mp3 == null) m_strWorkDirectory_Mp3 = Application.StartupPath;
                //////////////////////////////////////////////////
                if (txtID_FR.Text == txtID_FL.Text)
                {
                    txtID_FR.Text = "0";
                    txtID_FL.Text = "1";
                    txtID_RR.Text = "2";
                    txtID_RL.Text = "3";
                }
                if (txtID_0.Text == txtID_1.Text)
                {
                    txtID_0.Text = "0";
                    txtID_1.Text = "1";
                    txtID_2.Text = "2";
                }


                if (Ojw.CConvert.StrToInt(txtPort.Text) <= 0) txtPort.Text = "1";
                if (Ojw.CConvert.StrToInt(txtBaudrate.Text) <= 0) txtBaudrate.Text = "115200";
                int j = 0;
                bool bDigit = true;
                foreach (char cData in txtIp.Text) { if (cData == '.') j++; else if (Char.IsDigit(cData) == false) bDigit = false; }
                if ((j != 3) || (bDigit == false))
                {
                    txtIp.Text = "192.168.1.1";
                }
                if (Ojw.CConvert.StrToInt(txtPercent.Text) <= 0) txtPercent.Text = "70";
                if (bFile == false) { txtMotionCounter.Text = "1"; }

                //float fScale = 0.5f;
                //SizeF SScale = new SizeF();
                //SScale.Width = fScale;
                //SScale.Height = fScale;
                //this.Scale(SScale);
                ////tabPage1.Scale(SScale);
                ////dgAngle.Scale(SScale);
            }
            //if (m_C3d.FileOpen(@"16dof_ecoHead.ojw") == true) // 모델링 파일이 잘 로드 되었다면 
            //if (m_C3d.FileOpen(@"robolink-spider.ojw") == true) // 모델링 파일이 잘 로드 되었다면 
            //{
            
            //}

            // 그림을 그리기 위한 timer 가동
            tmrDraw.Enabled = true;

            // 설정된 마우스 이벤트를 사용할 것인지...(기본은 사용하도록 되어 있다. User의 Function만 사용하길 원한다면 false)
            //m_C3d.SetMouseEventEnable(true); // you can remove the default mouse events.

            

            //m_C3d.GridDraw_Init(dgAngle, 40);
            //InitGridView



            SaveParam();

            Ojw.CMessage.Write("Ready");
            //CheckForIllegalCrossThreadCalls = false;

            m_CTmr_Save.Set();

            SetToolTip();
            
            tmrMp3.Enabled = true;

            m_bLoadedForm = true;

            // Folder
            SetTreeDrive();
        }
        // 트리뷰에 드라이브 정보 입력
        private void SetTreeDrive()
        {
            String[] strDrives;
            String strTmp;
            SystemImgList sysimglst = new SystemImgList();
            int nNode;

            strDrives = System.IO.Directory.GetLogicalDrives();
            imglstTree.Images.Clear();
            treeInfo.Nodes.Clear();

            // 얻이진 드라이브 목록을 트리뷰에 입력한다.
            foreach (string str in strDrives)
            {
                //일반(이미지)
                imglstTree.Images.Add(sysimglst.GetIcon(str, true, false));

                // 끝에있는 //\//를 삭제한다.
                strTmp = str.Substring(0, 2);
                nNode = treeInfo.Nodes.Add(new TreeNode(strTmp, imglstTree.Images.Count - 1, imglstTree.Images.Count - 1));

                // 하위폴더가 있으면 //???//를 임시로 저장한다.
                if (HasSubFolder(strTmp))
                    treeInfo.Nodes[nNode].Nodes.Add("???");

            }

            //최초에 찾는 폴더를 이용해 폴더 이미지를 얻는다.
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(_C_DRV_ + "\\");

            foreach (System.IO.DirectoryInfo dirInfoSub in dirInfo.GetDirectories("*"))
            {
                imglstTree.Images.Add(sysimglst.GetIcon(dirInfoSub.FullName, true, false));
                imglstTree.Images.Add(sysimglst.GetIcon(dirInfoSub.FullName, true, true));
            }
            treeInfo.ImageList = imglstTree;
            // C드라이브(루트 다음)를 선택 효과를 준다.
            treeInfo.SelectedNode = treeInfo.Nodes[0];
        }
        private Ojw.CTimer m_CTmr_Save = new Ojw.CTimer();

        private bool m_bRequestPick = false;
        private void OjwMouseDown(object sender, MouseEventArgs e)
        {
            m_bRequestPick = true;
            
            //m_C3d.OjwMouseDown(sender, e);            
        }
        private void OjwMouseMove(object sender, MouseEventArgs e)
        {
            //m_C3d.OjwMouseMove(sender, e); 
        }
        private void OjwMouseUp(object sender, MouseEventArgs e)
        {
            //m_C3d.OjwMouseUp(sender, e); 
        }

        private bool m_bTimer = false;
        private void tmrDraw_Tick(object sender, EventArgs e)
        {
            if (m_bProgEnd == true) return;

            if (m_bTimer == true) return;
            m_bTimer = true;
            tmrDraw.Enabled = false;

            try
            {
                OjwDraw();
                //if (m_bRequestPick == true)
                //{
                //    m_bRequestPick = false;
                if (m_C3d.GetEvent_Pick() == true)
                {
                    m_bPick = true;
                    m_C3d.GetEvent_Pick_Data(out m_nGroup, out m_nMotor, out m_nServeGroup, out m_nKinematicsNumber);
                    ShowData(m_bPick);
                }
                //}
            }
            catch (Exception ex)
            {
                FileInfo file = new FileInfo(Application.StartupPath + _STR_FILENAME);
                if (file.Exists) System.IO.File.Delete(Application.StartupPath + _STR_FILENAME);
                string strMsg = "Drawing Error - remove Param.Dat, please restart your program : " + ex.ToString();
                Ojw.CMessage.Write_Error(strMsg);
                MessageBox.Show(strMsg, "Program End", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }
#if true
            if ((m_CTmr_AutoBackup.Get() > 30000) && (m_C3d.GetFileName() != null) && (m_bStart == false)) // 5분에 한번 저장 -> 30초로 변경
            {
                int nVer = _V_10;//((chkFileVersionForSave.Checked == true) ? _V_11 : ((chkFileVersionForSave_1_0.Checked == true) ? _V_10 : _V_12));
                m_C3d.BinaryFileSave(nVer, Application.StartupPath + Ojw.C3d._STR_BACKUP_FILE, false, false);
                
                // 혹시 모르니 파라미터도 같이 저장하자.
                SaveParam();

                m_CTmr_AutoBackup.Set();
            }
#endif
            int nKey = m_C3d.KeyCommand_Get();
            if (nKey == (int)Keys.F5) { tmrRun.Enabled = true; }
            //else if (nKey == (int)Keys.F4)
            //{
            //    Cmd_InitPos(_INITPOS_DEFAULT, 2000);
            //}
            else if (nKey == (int)Keys.Escape)
            {
                Stop();
            }

            tmrDraw.Enabled = true;
            m_bTimer = false;
        }
        
        private int m_nGroup, m_nMotor, m_nServeGroup;
        private int m_nKinematicsNumber;
        private bool m_bPick;
        private bool m_bLimit;
        private void ShowData(bool bPick)
        {
            txtTest.Text = String.Empty;

            if (bPick == false)
            {
                Ojw.CMessage.Write2(txtTest, "There is no any parts for controlling");
                return;
            }
            // 클릭했으니 메세지를 한번 보여주자(show messages when it click)	
            Ojw.CMessage.Write2(txtTest, "Current Joint Group = " + Ojw.CConvert.IntToStr(m_nGroup) + "\r\n");
            Ojw.CMessage.Write2(txtTest, "Current Motor Number = " + Ojw.CConvert.IntToStr(m_nMotor) + "\r\n");
            Ojw.CMessage.Write2(txtTest, "Current Serve Group Number = " + Ojw.CConvert.IntToStr(m_nServeGroup) + "\r\n");
            Ojw.CMessage.Write2(txtTest, "Connected Function number(but, 255 is None)=" + Ojw.CConvert.IntToStr(m_nKinematicsNumber) + "\r\n");
            Ojw.C3d.COjwDesignerHeader CHeader = m_C3d.GetHeader();

            // 수식부분이 선택된게 아니라면...(if there is no function number...)
            if ((m_nKinematicsNumber == 255) && (m_nMotor < CHeader.nMotorCnt))
            {
                if (m_nGroup > 0) // Is there a data?
                {
                    Ojw.CMessage.Write2(txtTest, Ojw.CConvert.IntToStr(m_nMotor) + "번모터(Name : " + Ojw.CConvert.RemoveChar(CHeader.pSMotorInfo[m_nMotor].strNickName, (char)0) + ")\r\n");
                    Ojw.CMessage.Write2(txtTest, "MotorID =" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nMotor].nMotorID) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Direction =" + ((CHeader.pSMotorInfo[m_nMotor].nMotorDir == 0) ? "Forward" : "Inverse"));
                    Ojw.CMessage.Write2(txtTest, "Limit(Max : but 0 -> there is no Limit)=" + ((CHeader.pSMotorInfo[m_nMotor].fLimit_Up != 0.0f) ? Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nMotor].fLimit_Up) + " 도" : "리미트 없음") + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Limit(Min : but 0 -> there is no Limit)=" + ((CHeader.pSMotorInfo[m_nMotor].fLimit_Down != 0.0f) ? Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nMotor].fLimit_Down) + " 도" : "리미트 없음") + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Center(EVD) : 0도에 해당하는 EVD 값 =" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nMotor].nCenter_Evd) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Mech Mov=" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nMotor].nMechMove) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Angle of Mech M =" + Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nMotor].fMechAngle) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Initial Position =" + Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nMotor].fInitAngle) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "NickName =" + Ojw.CConvert.RemoveChar(CHeader.pSMotorInfo[m_nMotor].strNickName, (char)0) + "\r\n");
                    Ojw.CMessage.Write2(txtTest, "Motor\'s Group Number =" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nMotor].nGroupNumber) + "\r\n");
                    //Ojw.CMessage.Write2(txtTest, );
                    //Ojw.CMessage.Write2(txtTest, );

                    // Motor Check(relationship)
                    int nMotID = m_nMotor;
                    if (CHeader.pSMotorInfo[nMotID].nAxis_Mirror == -1) Ojw.CMessage.Write2(txtTest, "이 모터는 Mirror 시 값의 변형을 주지 않는다.(No Changing when it has command [flip]");
                    else if (CHeader.pSMotorInfo[nMotID].nAxis_Mirror == -2) Ojw.CMessage.Write2(txtTest, "이 모터는 Mirror 시 Motor 의 Center Point 를 중심으로 뒤집도록 한다.(ex: -30 도 -> 30 도)");
                    else Ojw.CMessage.Write2(txtTest, "Current Motor number = " + Ojw.CConvert.IntToStr(nMotID) + ", Mirroring Motor number = " + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[nMotID].nAxis_Mirror));
                }
                else
                {
                    Ojw.CMessage.Write2(txtTest, "There is a part without controlling");
                }
            }
            else if (m_nKinematicsNumber != 255) // 수식 번호가 선택된 경우
            {
                float fX, fY, fZ;
                Ojw.CKinematics.CForward.CalcKinematics(CHeader.pDhParamAll[m_nKinematicsNumber], m_C3d.GetData(), out fX, out fY, out fZ);
                Ojw.CMessage.Write2(txtTest, "연동되는 수식의 번호(Connected Function Number) = " + Ojw.CConvert.IntToStr(m_nKinematicsNumber) + "\r\n");
                Ojw.CMessage.Write2(txtTest, "Current Position (x,y,z)=" + Ojw.CConvert.FloatToStr((float)Ojw.CMath.Round(fX, 3)) + "," + Ojw.CConvert.FloatToStr((float)Ojw.CMath.Round(fY, 3)) + "," + Ojw.CConvert.FloatToStr((float)Ojw.CMath.Round(fZ, 3)) + "\r\n");
            }
            else Ojw.CMessage.Write2(txtTest, "There is no any parts for controlling");
        }

        private bool m_bProgEnd = false;
        public int m_nSeq_ProgEnd = 0;
        private bool ProgEnd()
        {
            if (m_bLoadedForm == false)
            {
                m_nSeq_ProgEnd = 2;
                return true;
            }
            //tmrDraw.Enabled = false;
            //tmrMp3.Enabled = false;
            //tmrRun.Enabled = false;
            m_nSeq_ProgEnd = 1;
            DialogResult dlgRet = MessageBox.Show("프로그램을 종료합니다.\r\n\r\n계속 하시겠습니까?", "Program 종료", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (dlgRet != DialogResult.OK)
            {
                tmrDraw.Enabled = true;
                tmrMp3.Enabled = true;
                tmrRun.Enabled = true;

                m_nSeq_ProgEnd = 0;
                return false;
            }
            Stop();

            //tmrCheck.Enabled = false;
            //tmrMovingText.Enabled = false;
            tmrMp3.Enabled = false;

            // 환경파일 저장
            //DataFileSave_Config(_ENCODING_DEFAULT, Application.StartupPath + "\\Config.ini");

            m_bProgEnd = true;
            //m_bStop = true;

            //Ojw.CTimer.Wait(1000); // 백그라운드 작업이 다 완료될때까지 기다리는 편이 좋다.

            //DisConnect();

            //DestroyDesignHeader();

            // 정상적인 프로그램 종료시 백업한 파일 지우기
            FileInfo fileBack = new FileInfo(Application.StartupPath + Ojw.C3d._STR_BACKUP_FILE);
            if (fileBack.Exists) // 백업할 파일이 있는지 체크
            {
                // 없더라도 에러가 나지는 않는다. 굳이 에러처리가 필요 없음.
                fileBack.Delete();
            }

            // Save Param
            SaveParam();

            tmrMp3.Enabled = false;
            tmrDraw.Enabled = false;


            Ojw.CTimer.Wait(100); // 백그라운드 작업이 다 완료될때까지 기다리는 편이 좋다.


            // Serial
            if (m_C3d.m_CMotor.IsConnect() == true)
                Disconnect();
            // Socket
            if (m_C3d.m_CMotor2.IsOpen_Socket() == true)
                m_C3d.m_CMotor2.Close_Socket();
            m_nSeq_ProgEnd = 2;
            return true;
        }
        private void SaveParam()
        {
            int i = 0;
            m_CFile.SetData_String(i++, m_C3d.GetFileName());
            m_CFile.SetData_String(i++, txtPort.Text);
            m_CFile.SetData_String(i++, txtBaudrate.Text);
            m_CFile.SetData_String(i++, txtIp.Text);
            m_CFile.SetData_String(i++, txtSocket_Port.Text);
            m_CFile.SetData_Bool(i++, chkFreeze_X.Checked);
            m_CFile.SetData_Bool(i++, chkFreeze_Y.Checked);
            m_CFile.SetData_Bool(i++, chkFreeze_Z.Checked);
            m_CFile.SetData_Bool(i++, chkFreeze_Pan.Checked);
            m_CFile.SetData_Bool(i++, chkFreeze_Tilt.Checked);
            m_CFile.SetData_Bool(i++, chkFreeze_Swing.Checked);
            m_CFile.SetData_String(i++, txtSocket_Port.Text);
            m_CFile.SetData_String(i++, txtMotionCounter.Text);
            m_CFile.SetData_String(i++, txtChangeValue.Text);
            m_CFile.SetData_String(i++, txtPercent.Text);
            m_CFile.SetData_String(i++, txtBackAngle_X.Text);
            m_CFile.SetData_String(i++, txtBackAngle_Y.Text);
            m_CFile.SetData_String(i++, txtBackAngle_Z.Text);
            m_CFile.SetData_Bool(i++, chkTracking.Checked);
            m_CFile.SetData_Bool(i++, chkMp3.Checked);
            m_CFile.SetData_String(i++, txtMp3TimeDelay.Text);
            m_CFile.SetData_Bool(i++, chkFullSize.Checked);
            m_CFile.SetData_Bool(i++, chkDualMonitor.Checked);

            m_CFile.SetData_String(i++, txtID_FR.Text);
            m_CFile.SetData_String(i++, txtID_FL.Text);
            m_CFile.SetData_String(i++, txtID_RR.Text);
            m_CFile.SetData_String(i++, txtID_RL.Text);

            m_CFile.SetData_String(i++, txtID_0.Text);
            m_CFile.SetData_String(i++, txtID_1.Text);
            m_CFile.SetData_String(i++, txtID_2.Text);
            
            m_CFile.SetData_String(i++, m_strWorkDirectory_Dmt);
            m_CFile.SetData_String(i++, m_strWorkDirectory_Mp3);

            m_CFile.Save(Application.StartupPath + _STR_FILENAME);
        }
        private readonly string _STR_FILENAME = "\\Param.dat";
        private void frmMotionEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_nSeq_ProgEnd = 1;
            m_bProgEnd = true;
            bool bRet = ProgEnd();
            if (bRet == false)
            {
                e.Cancel = true;
                return;
            }
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

        private void btnInterpolation_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Interpolation, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnInterpolation2_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._S_Curve, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnFlip_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Flip_Position, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Clear();
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
        
        //private bool m_bStop = false;
        private bool m_bStart = false;
        private bool m_bEms = false; // 비상정지 용, 아직 사용 안함
        private bool m_bMotionEnd = false;

        //private Thread m_thRun;
        private void btnRun_Click(object sender, EventArgs e)
        {
            //btnRun.Enabled = false;
            Ojw.CTimer.Reset(); // Clear the Stop bit;
            
            //m_thRun = new Thread(new ThreadStart(Run));
            //m_thRun.Start();
            tmrRun.Enabled = true;


            //btnRun.Enabled = true;
        }
        
        private void Run()
        {
            btnRun.Enabled = false;
            btnSimul.Enabled = false;

            int nInterval = tmrDraw.Interval;
            if (m_C3d.GetSimulation_With_PlayFrame() == true)
                tmrDraw.Interval = 40;
            ///////////////////////////////////////////
            m_C3d.m_CGridMotionEditor.GetHandle().Focus();
            m_C3d.Start_Set();
            StartMotion();
            m_C3d.Start_Reset();
            ///////////////////////////////////////////            
            if (m_C3d.GetSimulation_With_PlayFrame() == true)
                tmrDraw.Interval = nInterval;
            m_C3d.SetSimulation_With_PlayFrame(false);
            btnRun.Enabled = true;
            btnSimul.Enabled = true;
        }
        private void StartMotion()
        {
            bool bSock = m_C3d.m_CMotor2.IsOpen_Socket();
            if (m_bStart == true)
            {
                lbMotion_Message.Text = "Motion Running...";//Motion 운전 중입니다.";
                Ojw.CMessage.Write(lbMotion_Message.Text);
                return;
            }
            //if (CheckWifi() == true)
            //{
            //    if (m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_check_Ems() == true)
            //    {
            //        lbMotion_Message.Text = "비상정지 알람이 켜져 있습니다.";
            //        return;
            //    }
            //}
            //else
            //{
            //    //if (m_C3d.m_CMotor.IsEms() == true)
            //    if ((m_C3d.m_CMotor.IsEms() == true) || (frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_check_Ems() == true))
            //    {
            //        lbMotion_Message.Text = "비상정지 알람이 켜져 있습니다.";
            //        return;
            //    }
            //    if ((m_C3d.m_CMotor.IsConnect() == false) && (frmMain.m_DrBluetooth.drbluetooth_client_connected() == false))
            //    {
            //        lbMotion_Message.Text = "Serial Port Error - Not Connected.";
            //        return;
            //    }
            //}

            //if (CheckWifi() == true) // 네트워크 버전이라면 파일을 전송해서 플레이하는 방식으로 한다.
            //{
            //    int nVer = ((chkFileVersionForSave.Checked == true) ? _V_11 : ((chkFileVersionForSave_1_0.Checked == true) ? _V_10 : _V_12));
            //    MakeBinaryFileStream(nVer);
            //    String strFile = m_strFileStream;
            //    DownLoad(m_nCurrentRobot, strFile);
            //    FileInfo fileStream = new FileInfo(strFile);
            //    //if (fileStream.Exists) // 지울 파일이 있는지 체크
            //    //{
            //    // 없더라도 에러가 나지는 않는다. 굳이 에러처리가 필요 없음.
            //    fileStream.Delete();
            //    //}
            //}

            if ((m_C3d.m_CMotor.IsEms() == true) || (m_C3d.m_CMotor2.IsEms() == true))
            {
                lbMotion_Message.Text = "Check you Emergency Status";//비상정지 알람이 켜져 있습니다.";
                Ojw.CMessage.Write(lbMotion_Message.Text);
                return;
            }
            if (m_C3d.GetSimulation_With_PlayFrame() == false)
            {
                if ((m_C3d.m_CMotor.IsConnect() == false) && (bSock == false))// && (frmMain.m_DrBluetooth.drbluetooth_client_connected() == false))
                {
                    lbMotion_Message.Text = "Serial Port & Socket Error - Not Connected.";
                    Ojw.CMessage.Write(lbMotion_Message.Text);
                    return;
                }
            }
           
            if (chkMp3.Checked == true)
            {
                //Mp3Stop();
                int nCell = m_C3d.m_CGridMotionEditor.m_nCurrntCell;
                if (nCell > 0)
                {
                    DialogResult dlgRet = MessageBox.Show(String.Format("Do you want to start from {0} line?", nCell), "Starting Position Check", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (dlgRet != DialogResult.OK)
                    {
                        dgAngle.CurrentCell = dgAngle.Rows[0].Cells[1];
                        nCell = 0;
                    }
                }
                ChangePos_Mp3Bar();

                // 일단 그리드의 타임값 등을 디스플레이 한다.
                Grid_DisplayTime();
                if ((nCell >= 0) && (dgAngle.RowCount > nCell))
                {
                    //prgMp3.Value = (int)(mpPlayer.Ctlcontrols.currentPosition / dTime * 100);
                    mpPlayer.Ctlcontrols.currentPosition = (double)m_lCalcTime[nCell] / 1000.0;
                }
                m_nMotion_Step = nCell;
            }
            else
                m_nMotion_Step = 0; // 메모리를 날린다. -> -_-

            string strMessage = "";


            // Auto Save 기능을 정지해야 하는데 혹시 몰라 시간초기화도 한다.
            m_CTmr_AutoBackup.Set();
            m_CTmr_AutoBackup.Kill();

            // 타이머 기능 정지
            if (m_C3d.GetSimulation_With_PlayFrame() == false) 
                tmrDraw.Enabled = false;
            //m_C3d.m_CMotor.SetAutoReturn(false);
            m_bStart = true;
            // 그리드 클릭 및 기타 이벤트 금지
            dgAngle.Enabled = false;
            //dgKinematics.Enabled = false;

            m_nMotion_Step = 0; // 메모리를 날린다. -> -_-

            m_bMotionEnd = false;
            m_C3d.m_CMotor.ResetStop();
            if (bSock == true) m_C3d.m_CMotor2.Reset();
            //if (CheckWifi() == true)
            //    m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_reset_stop();

            //frmMain.m_DrBluetooth.drbluetooth_set_id(frmMain.m_pnBluetoothAddress[m_nCurrentRobot]);
            //frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_reset_stop();

            lbMotion_Counter.Text = "0";
            int nCnt = 0;
            int nLimitCount = Ojw.CConvert.StrToInt(txtMotionCounter.Text);
            //// Motion Start ////
            //if (CheckWifi() == true)
            //    m_aDrSock[m_nCurrentRobot].drsock_client_request_motion_play(0, m_strFileStream);

            //frmMain.m_DrBluetooth.drbluetooth_set_id(frmMain.m_pnBluetoothAddress[m_nCurrentRobot]);
            //frmMain.m_DrBluetooth.drbluetooth_client_request_motion_play(0, m_strFileStream);
            
            if (chkMp3.Checked == true)
            {
                //#if _ENABLE_MEDIAPLAYER
                Mp3Play();
                //#endif
                Ojw.CTimer.Wait(Ojw.CConvert.StrToLong(txtMp3TimeDelay.Text));
            }

            //m_C3d.m_CMotor.ResetStop();
            // Servo / Driver On
            m_C3d.m_CMotor.DrvSrv(true, true);
            if (bSock == true) m_C3d.m_CMotor2.SetTorque(true, true);
            int nStep = m_nMotion_Step;
            m_nLoop = 0;
            while (
                    ((nLimitCount <= 0) || (nLimitCount > nCnt)) &&
                    ((m_C3d.m_CMotor.IsEms() == false) && (m_C3d.m_CMotor.IsStop() == false)) &&
                    ((m_C3d.m_CMotor2.IsEms() == false) && (m_C3d.m_CMotor2.IsStop() == false)) &&
                    //((frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_check_stop() == false) && (frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_check_Ems() == false)) &&
                     (m_bMotionEnd == false)
                    )
            {
                if (
                    (m_C3d.m_CMotor.IsEms() == false) && (m_C3d.m_CMotor.IsStop() == false) &&
                    (m_C3d.m_CMotor2.IsEms() == false) && (m_C3d.m_CMotor2.IsStop() == false)
                    )
                {
                    // 카운터 디스플레이
                    nCnt++;
                    lbMotion_Counter.Text = Ojw.CConvert.IntToStr(nCnt);

                    // 모션 실행
                    nStep = Motion(nStep);
                    
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
                    else nStep = 0;
                    m_nLoop++;
                }
            }
            m_nMotion_Step = nStep;
            //if ((m_bMotionEnd == true) && (m_bMp3Play == true)) Mp3Stop();
            
            Mp3Stop();

            //// Motion End ////
            #region 종료처리
            if ((m_C3d.m_CMotor.IsEms() == true) || (m_C3d.m_CMotor2.IsEms() == true))
            {
                lbMotion_Status.Text = "비상정지";
            }
            else if ((m_C3d.m_CMotor.IsStop() == true) || (m_C3d.m_CMotor2.IsStop() == true))
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


#if !_COLOR_GRID_IN_PAINT
            m_C3d.GridMotionEditor_SetColorGrid(dgAngle.CurrentCell.RowIndex, 1);
#endif

            if (strMessage != "")
            {
                lbMotion_Message.Text = strMessage;
            }

            m_bStart = false;
            // 타이머 기능 복원
            tmrDraw.Enabled = true;

            // 다시 Auto Save 를 활성화 한다.
            m_CTmr_AutoBackup.Set();

            // 그리드 클릭 및 기타 이벤트 금지 해제
            dgAngle.Enabled = true;

            //m_C3d.m_CMotor.SetAutoReturn(true);
        }
        private int m_nMotion_Step = 0;
        private int m_nLoop = 0;
        private int Motion(int nLine)
        {
            int nResult = 0;
            int temp_Line = 0;

            int nSize = dgAngle.RowCount;

            bool bAll = true;
            bool[] abSelected = new bool[nSize];
            abSelected.Initialize();
            int nTmpPos = 0;
            int nTmpCnt = 0;
            for (int i = 0; i < nSize; i++)
            {
                if ((m_C3d.GridMotionEditor_GetEnable(i) == true) && (dgAngle[0, i].Selected == true))
                {

                    bAll = false;
                    abSelected[i] = true;

                }

                for (int j = 0; j < dgAngle.ColumnCount; j++)
                {
                    if (dgAngle[j, i].Selected == true)
                    {
                        // 선택한 라인의 갯수를 셈 - 멀티선택인지, 단일선택인지 구분 가능
                        nTmpPos = i;
                        nTmpCnt++;
                        break;
                    }
                }

                //if ((m_C3d.GridMotionEditor_GetEnable(i) == true) && (dgAngle[0, i].Selected == true))
                //{
                //    bAll = false;
                //    break;
                //}
            }

            //if (CheckWifi() == true)
            //{
            //    m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_reset_stop();
            //    m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_drvsrv(true, true);
            //}
            //frmMain.m_DrBluetooth.drbluetooth_set_id(frmMain.m_pnBluetoothAddress[m_nCurrentRobot]);
            //frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_reset_stop();
            //frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_drvsrv(true, true);
            
            int nFirstLine = ((nTmpCnt == 1) && (chkMp3.Checked == true)) ? nTmpPos : 0; // 다중선택이면서 음원과 싱크를 맞춰 출력하려면...
            if (m_nLoop > 0) nFirstLine = 0;

            WaitAction_SetTimer();
            // float fVal;
            for (int i = nFirstLine; i < nSize; i++)
            {
                if (m_C3d.GridMotionEditor_GetEnable(i) == false) continue;
                if ((bAll == false) && (abSelected[i] == false)) continue;
                //if ((bAll == false) && (dgAngle[0, i].Selected == false)) continue;
                //if ((bAll == false) && (dgAngle.Rows[i].Selected == false)) continue;
                if (nLine == temp_Line)
                {
                    //if (m_C3d.GridMotionEditor_GetCommand(i) == 1)
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
#if false
                                    (m_C3d.m_CMotor.IsConnect() == false) ||
#else
                                    ((m_C3d.GetSimulation_With_PlayFrame() == false) && (m_C3d.m_CMotor.IsConnect() == false) && (m_C3d.m_CMotor2.IsOpen_Socket() == false)) ||
#endif
                                    ((m_C3d.m_CMotor.IsEms() == true) || (m_C3d.m_CMotor.IsStop() == true)) ||
                                    ((m_C3d.m_CMotor2.IsEms() == true) || (m_C3d.m_CMotor2.IsStop() == true)) ||
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
#if false
                    (m_C3d.m_CMotor.IsConnect() == false) ||
#else
                    ((m_C3d.GetSimulation_With_PlayFrame() == false) && (m_C3d.m_CMotor.IsConnect() == false) && (m_C3d.m_CMotor2.IsOpen_Socket() == false)) ||
#endif
                    //((m_C3d.m_CMotor.IsConnect() == false) && (frmMain.m_aDrSock[m_nCurrentRobot].drsock_client_connected() == false) && (frmMain.m_DrBluetooth.drbluetooth_client_connected() == false)) ||
                    //((frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_check_stop() == true) || (frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_check_Ems() == true)) ||
                    ((m_C3d.m_CMotor.IsEms() == true) || (m_C3d.m_CMotor.IsStop() == true)) ||
                    ((m_C3d.m_CMotor2.IsEms() == true) || (m_C3d.m_CMotor2.IsStop() == true)) ||
                    (m_bStart == false)
                    )
                    return (temp_Line - 1);
            }

            if (nLine == temp_Line)
            {
                //// 동작 ////


                //////////////
                nLine++;
            }
            temp_Line++;

            //m_C3d.m_CMotor.ResetStop();

            //if ((m_C3d.m_CMotor.IsEms() == true) || (m_C3d.m_CMotor.IsStop() == true)) return (temp_Line - 1);

            return nResult;
        }
        //private long m_lWaitActionTimer = 0;

        #region Timer ID - TID
        public const int _CNT_ROBOT = 20;
        public const int TID_MP3CHECK = 99;
        public const int TID_START = 98;
        public const int TID_TIMER = 97;
        //public const int TID_MOTION_BY_TIMER = 96;
        public const int TID_MOTIONS = 76; // 76 ~ 95
        public const int TID_MOTIONS_WAIT_TICK = 56; // 56 ~ 75
        public const int TID_SYNC = 36; // 36 ~ 55
        //public const int TID_FILEBACKUP = 35;
        public const int TID_MOTION2 = 34;
        #endregion Timer ID - TID

        private Ojw.CTimer m_CTmr_AutoBackup = new Ojw.CTimer();
        private void WaitAction_SetTimer()
        {
            //m_lWaitActionTimer = 0;
            m_C3d.WaitAction_SetTimer();
            return;
        }
        private bool WaitAction_ByTimer(long t) { return m_C3d.WaitAction_ByTimer(t); }
        private void PlayFrame(int nFrameNum) { m_C3d.PlayFrame(nFrameNum, 0); }
        
        private void btnStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void Stop()
        {
            //m_bStop = true;
            
            if (m_bMp3Play == true) Mp3Stop();
            m_C3d.m_CMotor.Stop();
            if (m_C3d.m_CMotor2.IsOpen_Socket() == true) m_C3d.m_CMotor2.Stop();
            m_C3d.WaitAction_KillTimer();
            m_bMotionEnd = true;
            Ojw.CTimer.Stop();
            
            //if (CheckWifi() == true)
            //{
            //    m_aDrSock[m_nCurrentRobot].drsock_client_serial_motor_request_stop();
            //    m_aDrSock[m_nCurrentRobot].drsock_client_request_motion_stop();
            //}
            //frmMain.m_DrBluetooth.drbluetooth_set_id(frmMain.m_pnBluetoothAddress[m_nCurrentRobot]);
            //frmMain.m_DrBluetooth.drbluetooth_client_serial_motor_request_stop();
            //frmMain.m_DrBluetooth.drbluetooth_client_request_motion_stop();
        }

        private void OjwDraw()
        {
            if (m_C3d.GetFileName() != null)
            {
                if (m_C3d != null)
                {
                    //bool bPick = false;
                    //m_C3d.OjwDraw(out m_nGroupA, out m_nGroupB, out m_nGroupC, out m_nKinematicsNumber, out bPick, out m_bLimit);
                    m_C3d.OjwDraw();
                    //if (bPick == true)
                    //{
                    //    m_bPick = bPick;
                    //}
                }
            }
        }
        private float[] m_afOrgAngleDispaly = new float[3];
        private float[] m_afSavedAngleDispaly = new float[3];
        private bool m_bControled_AngleDisplay = false;
        private void btnPos_Go_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            float fX = Ojw.CConvert.StrToFloat(txtBackAngle_X.Text);
            float fY = Ojw.CConvert.StrToFloat(txtBackAngle_Y.Text);
            float fZ = Ojw.CConvert.StrToFloat(txtBackAngle_Z.Text);
            ////////////////////////////////////////////////////////////
            if (chkFreeze_Tilt.Checked == true) fX = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fY = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fZ = fAngle_Z;
            m_C3d.SetAngle_Display(fY, fX, fZ);
            ////////////////////////////////////////////////////////////

            OjwDraw();
        }

        private void btnDisplay_RememberPos_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afSavedAngleDispaly[0] = fAngle_X;
                m_afSavedAngleDispaly[1] = fAngle_Y;
                m_afSavedAngleDispaly[2] = fAngle_Z;
            }
        }

        private void btnDisplay_GetThePose_Click(object sender, EventArgs e)
        {
            m_C3d.SetAngle_Display(m_afSavedAngleDispaly[1], m_afSavedAngleDispaly[0], m_afSavedAngleDispaly[2]);

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(m_afSavedAngleDispaly[0]);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(m_afSavedAngleDispaly[1]);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(m_afSavedAngleDispaly[2]);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_Front_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            m_C3d.SetAngle_Display(0, 0, 0);

            ////////////////////////////////////////////////////////////
            float fAngle_X2 = 0.0f, fAngle_Y2 = 0.0f, fAngle_Z2 = 0.0f;

            if (chkFreeze_Tilt.Checked == true) fAngle_X2 = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fAngle_Y2 = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fAngle_Z2 = fAngle_Z;
            m_C3d.SetAngle_Display(fAngle_Y2, fAngle_X2, fAngle_Z2);
            ////////////////////////////////////////////////////////////


            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(fAngle_X2);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(fAngle_Y2);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(fAngle_Z2);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_Bottom_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            float fX = -90.0f;
            float fY = 0.0f;
            float fZ = 0.0f;
            ////////////////////////////////////////////////////////////
            if (chkFreeze_Tilt.Checked == true) fX = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fY = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fZ = fAngle_Z;
            m_C3d.SetAngle_Display(fY, fX, fZ);
            ////////////////////////////////////////////////////////////

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(fX);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(fY);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(fZ);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_Top_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            float fX = 90.0f;
            float fY = 0.0f;
            float fZ = 0.0f;
            ////////////////////////////////////////////////////////////
            if (chkFreeze_Tilt.Checked == true) fX = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fY = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fZ = fAngle_Z;
            m_C3d.SetAngle_Display(fY, fX, fZ);
            ////////////////////////////////////////////////////////////

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(fX);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(fY);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(fZ);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_Right_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            float fX = 0.0f;
            float fY = 90.0f;
            float fZ = 0.0f;
            ////////////////////////////////////////////////////////////
            if (chkFreeze_Tilt.Checked == true) fX = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fY = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fZ = fAngle_Z;
            m_C3d.SetAngle_Display(fY, fX, fZ);
            ////////////////////////////////////////////////////////////

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(fX);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(fY);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(fZ);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_Left_Click(object sender, EventArgs e)
        {
            float fAngle_X, fAngle_Y, fAngle_Z;
            m_C3d.GetAngle_Display(out fAngle_Y, out fAngle_X, out fAngle_Z);

            if (m_bControled_AngleDisplay == false)
            {
                m_afOrgAngleDispaly[0] = fAngle_X;
                m_afOrgAngleDispaly[1] = fAngle_Y;
                m_afOrgAngleDispaly[2] = fAngle_Z;

                m_bControled_AngleDisplay = true;
            }
            float fX = 0.0f;
            float fY = -90.0f;
            float fZ = 0.0f;
            ////////////////////////////////////////////////////////////
            if (chkFreeze_Tilt.Checked == true) fX = fAngle_X;
            if (chkFreeze_Pan.Checked == true) fY = fAngle_Y;
            if (chkFreeze_Swing.Checked == true) fZ = fAngle_Z;
            m_C3d.SetAngle_Display(fY, fX, fZ);
            ////////////////////////////////////////////////////////////

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(fX);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(fY);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(fZ);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private void btnPos_TurnBack_Click(object sender, EventArgs e)
        {
            // test
            //int nCommand = 3; // 1 // 0 - Get Motion List, 1 - Motion Download, 2 - Delete, 3 - Delete All
            //F_enter_bootloader(253, Ojw.CConvert.StrToInt(txtPort.Text), Ojw.CConvert.StrToInt(txtBaudrate.Text), nCommand);




            m_bControled_AngleDisplay = false;

            m_C3d.SetAngle_Display(m_afOrgAngleDispaly[1], m_afOrgAngleDispaly[0], m_afOrgAngleDispaly[2]);

            #region 현 회전각 표현하기...
            txtBackAngle_X.Text = Ojw.CConvert.FloatToStr(m_afOrgAngleDispaly[0]);
            txtBackAngle_Y.Text = Ojw.CConvert.FloatToStr(m_afOrgAngleDispaly[1]);
            txtBackAngle_Z.Text = Ojw.CConvert.FloatToStr(m_afOrgAngleDispaly[2]);
            #endregion 현 회전각 표현하기...

            OjwDraw();
        }

        private bool m_bMouseClick = false;
        private Point m_pntMouse;
        private void frmMotionEditor_MouseDown(object sender, MouseEventArgs e)
        {
            m_bMouseClick = true;
            m_pntMouse.X = e.X;
            m_pntMouse.Y = e.Y;
        }

        private void frmMotionEditor_MouseMove(object sender, MouseEventArgs e)
        {
            if ((m_bMouseClick == true) && (m_pntMouse.Y < 31))
            {
                int nGap_X = m_pntMouse.X - e.X;
                int nGap_Y = m_pntMouse.Y - e.Y;
                this.Left -= nGap_X;
                this.Top -= nGap_Y;
            }
        }

        private void frmMotionEditor_MouseUp(object sender, MouseEventArgs e)
        {
            m_bMouseClick = false;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
            //Application.Exit();
        }
                
        private void btnConnect_Serial_Click(object sender, EventArgs e)
        {
            Connection();
        }
        public void Connect()
        {
            m_C3d.m_CMotor.Connect(Ojw.CConvert.StrToInt(txtPort.Text), Ojw.CConvert.StrToInt(txtBaudrate.Text));
            if (m_C3d.m_CMotor.IsConnect() == true)
            {
                btnConnect_Serial.Text = "Disconnect";
                Ojw.CMessage.Write("Connected");
                btnConnect.Enabled = false;
            }
            else
            {
                m_C3d.m_CMotor.DisConnect();
                btnConnect.Enabled = true;
                btnConnect_Serial.Text = "Connect";
                Ojw.CMessage.Write_Error("Connect Fail -> Check your COMPORT first");

                SetToolTip();
            }
            //m_C3d.OjwGrid_SetHandle_Herculex(m_CMotor);
        }
        public void Disconnect()
        {
            m_C3d.m_CMotor.DisConnect();
            btnConnect.Enabled = true;

            btnConnect_Serial.Text = "Connect";
            Ojw.CMessage.Write("Disconnected");
        }
        public void Connection()
        {
            if (m_C3d.m_CMotor.IsConnect() == false) Connect();
            else Disconnect();
        }

        private void btnPercent_Click(object sender, EventArgs e)
        {
            DelayChange();
        }
        public void DelayChange()
        {
            int i;
            for (i = 0; i < dgAngle.RowCount; i++)
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

        //private AxWMPLib.AxWindowsMediaPlayer mpPlayer;
        private void chkMp3_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMp3.Checked == true)
            {
                ChangePos_Mp3Bar();
            }
        }
        private void ChangePos_Mp3Bar()
        {
            // 일단 그리드의 타임값 등을 디스플레이 한다.
            Grid_DisplayTime();

            int nCell = m_C3d.m_CGridMotionEditor.m_nCurrntCell;
            if ((nCell >= 0) && (dgAngle.RowCount > nCell))
            {
                //prgMp3.Value = (int)(mpPlayer.Ctlcontrols.currentPosition / dTime * 100);
                //#if _ENABLE_MEDIAPLAYER
                mpPlayer.Ctlcontrols.currentPosition = (double)m_lCalcTime[nCell] / 1000.0;
                //#endif
            }
        }
        private void ChangePos_Cell() 
        {
            // 일단 그리드의 타임값 등을 디스플레이 한다.
            Grid_DisplayTime();
            int nCol = 1;
            int nPos = (int)Math.Round(mpPlayer.Ctlcontrols.currentPosition * 1000.0f);
            int nIndex0 = -1, nIndex1 = -2, nIndex_Default = 0;
            for (int i = 0; i < dgAngle.RowCount; i++)
            {
                if (m_C3d.m_CGridMotionEditor.GetEnable(i) == true) nIndex_Default = i;
                if (m_lCalcTime[i] >= nPos) 
                {
                    
                    nIndex0 = i;
                    //break;
                    if (m_C3d.m_CGridMotionEditor.GetEnable(i) == true)
                    {
                        nIndex1 = i;
                        break;
                    }
                }
            }
            if (nIndex0 > 0)
            {
                if (nIndex1 >= nIndex0)
                    dgAngle.CurrentCell = dgAngle.Rows[Math.Max(nIndex0, nIndex1)].Cells[nCol];
            }
            else dgAngle.CurrentCell = dgAngle.Rows[nIndex_Default].Cells[nCol];
        }
        public string[] m_strCalcTime = new string[10000];
        public long[] m_lCalcTime = new long[10000];
        private void Grid_DisplayTime()
        {
            //return;
            Grid_CalcTimer();
            //for (int i = 0; i < dgAngle.RowCount; i++)
            //{
            //    if (Convert.ToString(dgAngle.Rows[i].Cells[dgAngle.ColumnCount - 2].Value) != m_strCalcTime[i])
            //    {
            //        dgAngle.Rows[i].Cells[dgAngle.ColumnCount - 2].Value = m_strCalcTime[i];
            //    }
            //}
        }
        private void Grid_DisplayTime(int nLine)
        {
            if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return;
            Grid_CalcTimer();
            if (Convert.ToString(dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 2].Value) != m_strCalcTime[nLine])
            {
                dgAngle.Rows[nLine].Cells[dgAngle.ColumnCount - 2].Value = m_strCalcTime[nLine];
            }
        }
        private void Grid_DisplayTime(int nLine, int nSize)
        {
            int nStart = Ojw.CConvert.Clip(dgAngle.RowCount, 0, nLine);
            int nEnd = Ojw.CConvert.Clip(dgAngle.RowCount, nLine, nLine + nSize);
            for (int i = nStart; i < nEnd; i++)
            {
                Grid_CalcTimer(i);
                if (Convert.ToString(dgAngle.Rows[i].Cells[dgAngle.ColumnCount - 2].Value) != m_strCalcTime[i])
                {
                    dgAngle.Rows[i].Cells[dgAngle.ColumnCount - 2].Value = m_strCalcTime[i];
                }
            }
        }
        private long Grid_CalcTimer(int nLine) //
        {
            if ((nLine < 0) || (dgAngle.RowCount <= nLine)) return 0;

            int i = nLine - 1;

            bool bNull = false;
            if (nLine <= 0) bNull = true;

            if (bNull == false)
            {
                // 시간값 계산
                // En
                short sData = (short)Ojw.CConvert.BoolToInt(m_C3d.GridMotionEditor_GetEnable(i)); // En
                //                short sData0 = Convert.ToInt16(rowData[i]["Data0"]);
                long nSpeed = 0;
                long nDelay = 0;
                long nTimer = 0;
                if (sData != 0) // En 이 되어있다면
                {
                    // Speed
                    nSpeed = (long)m_C3d.GridMotionEditor_GetTime(i);
                    // Delay
                    nDelay = (long)m_C3d.GridMotionEditor_GetDelay(i);
                    //nTimer += nSpeed * 10 + nDelay * 10;
                    nTimer += nSpeed + nDelay;
                }
                m_lCalcTime[nLine] = m_lCalcTime[i] + nTimer;
                int nMs = (int)(m_lCalcTime[nLine] % 1000);
                int nAllSec = (int)(m_lCalcTime[nLine] / 1000);
                int nS = nAllSec % 60;
                int nM = (nAllSec / 60) % 60;
                int nH = (nAllSec / 60) / 60;
                // Hour
                string strTmp = Ojw.CConvert.IntToStr(nH);
                if (strTmp.Length < 2) strTmp = "0" + strTmp;
                m_strCalcTime[nLine] = strTmp + ":";
                // Minute
                strTmp = Ojw.CConvert.IntToStr(nM);
                if (strTmp.Length < 2) strTmp = "0" + strTmp;
                m_strCalcTime[nLine] = m_strCalcTime[nLine] + strTmp + ":";
                // Second
                strTmp = Ojw.CConvert.IntToStr(nS);
                if (strTmp.Length < 2) strTmp = "0" + strTmp;
                m_strCalcTime[nLine] = m_strCalcTime[nLine] + strTmp + ".";
                // 1 Milli-Second
                strTmp = Ojw.CConvert.IntToStr(nMs);
                if (strTmp.Length < 2) strTmp = "0" + strTmp;
                m_strCalcTime[nLine] = m_strCalcTime[nLine] + strTmp;
            }
            else
            {
                m_strCalcTime[nLine] = "00:00:00.00";
                m_lCalcTime[nLine] = 0;
            }
            return m_lCalcTime[nLine];
        }
        private void Grid_CalcTimer() { for (int i = 0; i < dgAngle.RowCount; i++) Grid_CalcTimer(i); }

        private void chkTracking_CheckedChanged(object sender, EventArgs e)
        {
            m_C3d.m_bControl_Tracking = chkTracking.Checked;
        }

        private bool m_btmrRun = false;
        private void tmrRun_Tick(object sender, EventArgs e)
        {
            if (m_bProgEnd == true) return;

            tmrRun.Enabled = false;

            if (m_btmrRun == true) return;
            m_btmrRun = true;

            Run();

            m_btmrRun = false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            m_nMotion_Step = 0;
            m_C3d.m_CMotor.ResetEms();
            m_C3d.m_CMotor.ResetStop();
            m_C3d.m_CMotor.Reset();
            if (m_C3d.m_CMotor2.IsOpen_Socket() == true)
            {
                m_C3d.m_CMotor2.Reset();
            }
        }

        private void btnMotionEnd_Click(object sender, EventArgs e)
        {
            m_bMotionEnd = true;
        }

        private void Ems()
        {
            Stop();
            //if (m_bMp3Play == true) Mp3Stop();
            m_C3d.m_CMotor.Ems();
            m_C3d.m_CMotor2.Ems();
        }
        private void btnEms_Click(object sender, EventArgs e)
        {
            Ems();
        }

        private void btnMode0_Click(object sender, EventArgs e)
        {
            m_C3d.Prop_Set_Main_MouseControlMode(0);
            m_C3d.Prop_Update_VirtualObject();
        }

        private void btnMode1_Click(object sender, EventArgs e)
        {

            m_C3d.Prop_Set_Main_MouseControlMode(1);
            m_C3d.Prop_Update_VirtualObject();
        }

        private String GetMotionFileName(String strFilePath)
        {
            String _STR_EXT = "dmt";
            String fileName = "";
            SaveFileDialog sdDialog = new SaveFileDialog();
            /////////////////////////////////////////
            // 파일 저장시 확장자가 ".dmt" 가 아니라면 새로 저장하도록...
            String strExe = Ojw.CFile.GetExe(strFilePath);
            if ((strExe == null) || (Ojw.CFile.GetExe(strFilePath).ToLower() != _STR_EXT.ToLower()))
            {
                sdDialog.FileName = null;
                sdDialog.DefaultExt = _STR_EXT.ToLower();
                if (sdDialog.ShowDialog() == DialogResult.OK)
                    fileName = sdDialog.FileName;
            }
            else fileName = strFilePath;
            /////////////////////////////////////////
            sdDialog.Dispose();

            //m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(fileName);
            //if (m_strWorkDirectory_Dmt == null) m_strWorkDirectory_Dmt = Application.StartupPath;
            return fileName;
        }
        private const int _V_10 = 0;
        private const int _V_11 = 1;
        private const int _V_12 = 2;
        private void btnTextSave_Click(object sender, EventArgs e)
        {
            //int nVer = ((chkFileVersionForSave.Checked == true) ? _V_11 : ((chkFileVersionForSave_1_0.Checked == true) ? _V_10 : _V_12));
            if (chkRmt.Checked == true)
            {
                m_C3d.RmtFileSave(GetMotionFileName(txtFileName.Text));//, false);
            }
            else
                m_C3d.BinaryFileSave(_V_10, GetMotionFileName(txtFileName.Text), false);
            m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(txtFileName.Text);
            m_CTmr_Save.Set();            
        }

        private void chkFreeze_X_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_X(chkFreeze_X.Checked); }
        private void chkFreeze_Y_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_Y(chkFreeze_Y.Checked); }
        private void chkFreeze_Z_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_Z(chkFreeze_Z.Checked); }
        private void chkFreeze_Tilt_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_Tilt(chkFreeze_Tilt.Checked); }
        private void chkFreeze_Pan_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_Pan(chkFreeze_Pan.Checked); }
        private void chkFreeze_Swing_CheckedChanged(object sender, EventArgs e) { m_C3d.SetFreeze_Swing(chkFreeze_Swing.Checked); }
        
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
            string strExe = (chkRmt.Checked == true) ? "rmt" : "dmt";
            ofdMotion.FileName = "*." + strExe;
            ofdMotion.Filter = string.Format("모션 파일(*.{0})|*.{0}", strExe);
            ofdMotion.DefaultExt = strExe;// "dmt";
            SetDirectory(ofdMotion, m_strWorkDirectory_Dmt);
            if (ofdMotion.ShowDialog() == DialogResult.OK)
            {
                String fileName = ofdMotion.FileName;
                //m_strWorkDirectory_Dmt = Directory.GetCurrentDirectory();
                m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(fileName);
                if (m_strWorkDirectory_Dmt == null) m_strWorkDirectory_Dmt = Application.StartupPath;

                txtFileName.Text = fileName;


                if (chkRmt.Checked == true)
                {
                    //TextBox txtFile = new TextBox();
                    //Ojw.CFile.Read(fileName, txtFile);
                    //MessageBox.Show(txtFile.Text);

                    if (m_C3d.RmtFileOpen(fileName) == false)
                    {
                        MessageBox.Show(ofdMotion.DefaultExt.ToUpper() + " 모션 파일이 아닙니다.");
                    }
                    else
                    {
                        Modify(false);
                        Grid_DisplayTime();

                        txtTableName.Text = m_C3d.GetMotionFile_Title();
                        txtComment.Text = m_C3d.GetMotionFile_Comment();
                        cmbStartPosition.SelectedIndex = m_C3d.GetMotionFile_StartPosition();
                    }
                }
                else
                {

                    if (m_C3d.DataFileOpen(fileName, null) == false)
                    {
                        MessageBox.Show(ofdMotion.DefaultExt.ToUpper() + " 모션 파일이 아닙니다.");
                    }
                    else
                    {
                        Modify(false);
                        Grid_DisplayTime();

                        txtTableName.Text = m_C3d.GetMotionFile_Title();
                        txtComment.Text = m_C3d.GetMotionFile_Comment();
                        cmbStartPosition.SelectedIndex = m_C3d.GetMotionFile_StartPosition();
                    }
                }
            }

            //m_strWorkDirectory_Dmt = Directory.GetCurrentDirectory();
            //WriteRegistry_Path(m_strWorkDirectory);
            ofdMotion.Dispose();
        }

        private void pnButton_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnBinarySave_Click(object sender, EventArgs e)
        {
            if (chkRmt.Checked == true)
            {
                m_C3d.RmtFileSave(GetMotionFileName(txtFileName.Text));//, true);
            }
            else
                m_C3d.BinaryFileSave(_V_10, GetMotionFileName(txtFileName.Text), true);
            m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(txtFileName.Text);
            m_CTmr_Save.Set(); 
        }

        private void btnInitpos_Click(object sender, EventArgs e)
        {
            DialogResult dlgRet;            
            if (m_C3d.m_CGridMotionEditor.Clear_GetType() != 0)
            {
                dlgRet = MessageBox.Show("Do you want to change your clear type to [Default]?", "Setting Clear Type", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dlgRet != DialogResult.OK)
                {
                }
                else
                {
                    m_C3d.m_CGridMotionEditor.Clear_SetType(0);
                    Ojw.CMessage.Write("Changed Clear Type -> 0");
                }
            }
            if (m_C3d.m_CMotor.IsConnect() == true)
            {
                dlgRet = MessageBox.Show("Do you want to Move your InitPos?", "Move Init(Default) Motion", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dlgRet != DialogResult.OK)
                {
                }
                else
                {
                    m_C3d.m_CMotor.ResetStop();
                    m_C3d.m_CMotor.DrvSrv(true, true);
                    for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) { m_C3d.m_CMotor.SetCmd_Angle(i, m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle); }
                    m_C3d.m_CMotor.SetMot(1000);
                }
            }
            else if (m_C3d.m_CMotor2.IsOpen_Socket() == true)
            {
                dlgRet = MessageBox.Show("Do you want to Move your InitPos?", "Move Init(Default) Motion", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dlgRet != DialogResult.OK)
                {
                }
                else
                {
                    m_C3d.m_CMotor2.Reset();
                    m_C3d.m_CMotor2.SetTorque(true, true);
                    for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) { m_C3d.m_CMotor2.Set_Angle(i, m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle); }
                    m_C3d.m_CMotor2.Send_Motor(1000);
                }
            }
        }

        private void btnInitpos2_Click(object sender, EventArgs e)
        {
            DialogResult dlgRet;
            if (m_C3d.m_CGridMotionEditor.Clear_GetType() != 1)
            {
                dlgRet = MessageBox.Show("Do you want to change your clear type to [Second Type]?", "Setting Clear Type", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dlgRet != DialogResult.OK)
                {
                }
                else
                {
                    m_C3d.m_CGridMotionEditor.Clear_SetType(1);
                    Ojw.CMessage.Write("Changed Clear Type -> 1");
                }
            }
            if (m_C3d.m_CMotor.IsConnect() == true)
            {
                dlgRet = MessageBox.Show("Do you want to Move your InitPos?", "Move Init(second) Motion", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dlgRet != DialogResult.OK)
                {
                }
                else
                {
                    m_C3d.m_CMotor.ResetStop();
                    m_C3d.m_CMotor.DrvSrv(true, true);
                    for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) { m_C3d.m_CMotor.SetCmd_Angle(i, m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle2); }
                    m_C3d.m_CMotor.SetMot(1000);
                }
            }
            else if (m_C3d.m_CMotor2.IsOpen_Socket() == true)
            {
                dlgRet = MessageBox.Show("Do you want to Move your InitPos?", "Move Init(second) Motion", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dlgRet != DialogResult.OK)
                {
                }
                else
                {
                    m_C3d.m_CMotor2.Reset();
                    m_C3d.m_CMotor2.SetTorque(true, true);
                    for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++) { m_C3d.m_CMotor2.Set_Angle(i, m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle2); }
                    m_C3d.m_CMotor2.Send_Motor(1000);
                }
            }
        }
        // 시간값 계산

        private const int _INITPOS_CLEAR = 0;
        private const int _INITPOS_DEFAULT = 1;
        private const int _INITPOS_SECOND = 2;
        // 초기모션
        private void Cmd_InitPos(int nInitNum, int nTime)
        {
            if (m_C3d.m_CHeader == null)
            {
                DialogResult dlgRet = MessageBox.Show("Please check your Modeling File.", "Fail to runing a Initial motion", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dlgRet != DialogResult.OK) return;
                return;
            }
            
            m_C3d.m_CMotor.ResetStop();
            m_C3d.m_CMotor.DrvSrv(true, true);
            
            //SetAxisParam(nRobot); // 로봇의 정보를 셋팅한다.

            for (int i = 0; i < m_C3d.m_CHeader.nMotorCnt; i++)
            {
                if ((m_C3d.m_CHeader.pSMotorInfo[i].nMotorControlType == 0) || (m_C3d.m_CHeader.pSMotorInfo[i].nMotorControlType == 2))
                {
                    m_C3d.m_CMotor.SetCmd_Flag_Mode(i, false); // 위치제어 모드
                    m_C3d.m_CMotor.SetCmd_Flag_NoAction(i, false);
                    //m_C3d.m_CMotor.SetCmd_Flag_Led(nAxis, bGreen, bBlue, bRed);                        
                    m_C3d.m_CMotor.SetCmd_Angle(i, ((nInitNum == 0) ? 0 : ((nInitNum == 1) ? m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle : m_C3d.m_CHeader.pSMotorInfo[i].fInitAngle2)));
                }
                else
                {
                    m_C3d.m_CMotor.SetCmd_Flag_Mode(i, true); // 속도모드
                    m_C3d.m_CMotor.SetCmd_Flag_NoAction(i, false);
                    //OjwMotor.SetCmd_Flag_Stop(i, true);
                    m_C3d.m_CMotor.SetCmd(i, 0);
                    
                }
            }
            m_C3d.m_CMotor.SetMot(nTime);
        }

        private void SetToolTip()
        {       
            ToolTip tp = new ToolTip();
            tp.ToolTipTitle = String.Format("Serial Port");
            tp.ToolTipIcon = ToolTipIcon.Info;
            tp.IsBalloon = true;
            tp.ShowAlways = true;
            
            string[] pstrComport = null;
            Ojw.CRegistry.GetSerialPort(out pstrComport, true, true);
            if (pstrComport != null)
            {
                string strData = string.Empty;
                if (pstrComport.Length > 0)
                {
                    for (int i = 0; i < pstrComport.Length - 1; i++)
                    {
                        strData += pstrComport[i] + ", ";
                    }
                    strData += pstrComport[pstrComport.Length - 1];
                    tp.SetToolTip(txtPort, String.Format("You can choose your comport numbers = {0}", strData));
                    //tp.Show(String.Format("You can choose your comport numbers = {0}", strData), txtPort);
                }
                else
                {
                    tp.SetToolTip(txtPort, String.Format("I cannot find any serial port in this computer"));
                    //tp.Show(String.Format("I cannot find any serial port in this computer"), txtPort);
                }
            }
            //tp.Dispose();
        }

        private int GetInverseKinematicsNumber(int nMotor)
        {
            int nNum = -1;
            for (int i = 0; i < m_C3d.GetHeader_pSOjwCode().GetLength(0); i++)
            {
                // 실제 수식계산
                //Ojw.CKinematics.CInverse.CalcCode(ref m_C3d.GetHeader_pSOjwCode()[i]);
                int nMotCnt = m_C3d.GetHeader_pSOjwCode()[i].nMotor_Max;
                for (int j = 0; j < nMotCnt; j++)
                {
                    int nMotNum = m_C3d.GetHeader_pSOjwCode()[nNum].pnMotor_Number[j];
                    if (nMotNum == nMotor) { nNum = nMotNum; break; }
                }
            }
            return nNum;
        }
        private void btnX_Plus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._X_Plus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnX_Minus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._X_Minus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnY_Plus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Y_Plus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnY_Minus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Y_Minus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnZ_Plus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Z_Plus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        private void btnZ_Minus_Click(object sender, EventArgs e)
        {
            m_C3d.GridMotionEditor_Calc(Ojw.ECalc_t._Z_Minus, Ojw.CConvert.StrToFloat(txtChangeValue.Text));
        }

        #region Mp3
        public bool m_bScreen = true;
        private void btnMp3Open_Click(object sender, EventArgs e)
        {
            m_dMp3_Max = 0;
            prgMp3.Value = 0;

            OpenFileDialog ofdMp3 = new OpenFileDialog();
            ofdMp3.FileName = null;
#if false
            // 동영상
            // avi,asf,asx,wpl,wm,wmx,wmd,wmz,wmv,wav,mpeg,mpg,mpe,m1v,m2v,mod,mp2,mpv2,mp2v,mpa,mp4
            strFilter = ".avi,.asf,.asx,.wpl,.wm,.wmx,.wmd,.wmz,.wmv,.wav,.mpeg,.mpg,.mpe,.m1v,.m2v,.mod,.mp2,.mpv2,.mp2v,.mpa,.mp4";
            // 오디오
            // wma,wax,cda,mp3,m3u,mid,midi,rmi,air,aifc,aiff,au,snd
            strFilter += ",.wma,.wax,.cda,.mp3,.m3u,.mid,.midi,.rmi,.air,.aifc,.aiff,.au,.snd";

            ofdMp3.FileName = "*.dmt";
            ofdMp3.Filter = "Audio 파일(*.dmt)|*.dmt";
#endif
            //ofdMp3.Filter = "AVI|*.avi|asf|*.asf|asx|*.asx|wpl|*.wpl|wm|*.wm|wmx|*.wmx|wmd|*.wmd|wmz|*.wmz|wmv|*.wmv|wav|*.wav|mpeg|*.mpeg|mpg|*.mpg|mpe|*.mpe|m1v|*.m1v|m2v|*.m2v|mod|*.mod|mp2|*.mp2|mpv2|*.mpv2|mp2v|*.mp2v|mpa|*.mpa|mp4|*.mp4|wma|*.wma|wax|*.wax|cda|*.cda|mp3|*.mp3|m3u|*.m3u|mid|*.mid|midi|*.midi|rmi|*.rmi|air|*.air|aifc|*.aifc|aiff|*.aiff|au|*.au|snd|*.snd";
            ofdMp3.Filter = "Media Files(*.avi,*.asf,*.asx,*.wpl,*.wm,*.wmx,*.wmd,*.wmz,*.wmv,*.wav,*.mpeg,*.mpg,*.mpe,*.m1v,*.m2v,*.mod,*.mp2,*.mpv2,*.mp2v,*.mpa,*.mp4,*.wma,*.wax,*.cda,*.mp3,*.m3u,*.mid,*.midi,*.rmi,*.air,*.aifc,*.aiff,*.au,*.snd)|*.avi;*.asf;*.asx;*.wpl;*.wm;*.wmx;*.wmd;*.wmz;*.wmv;*.wav;*.mpeg;*.mpg;*.mpe;*.m1v;*.m2v;*.mod;*.mp2;*.mpv2;*.mp2v;*.mpa;*.mp4;*.wma;*.wax;*.cda;*.mp3;*.m3u;*.mid;*.midi;*.rmi;*.air;*.aifc;*.aiff;*.au;*.snd";
            ofdMp3.DefaultExt = "mp3";
            ofdMp3.InitialDirectory = m_strWorkDirectory_Mp3;//Application.StartupPath + "\\music\\";
            if (ofdMp3.ShowDialog() == DialogResult.OK)
            {
                String fileName = ofdMp3.FileName;
                lbMp3File.Text = Ojw.CFile.GetTitle(fileName);
                String strExe = Ojw.CFile.GetExe(fileName);

                if ((strExe.ToUpper() != "MP3") && (strExe.ToUpper() != "WAV")) m_bScreen = true;
                else m_bScreen = false;
                InitMp3();

                mpPlayer.URL = fileName;
                mpPlayer.settings.volume = 0;
                mpPlayer.Ctlcontrols.play();
                Ojw.CTimer CTmr = new Ojw.CTimer();
                CTmr.Set();
                //while (m_dMp3_Max <= 0)
                while (true)
                {
                    if (mpPlayer.Ctlcontrols.currentItem != null)
                    {
                        if (mpPlayer.Ctlcontrols.currentItem.duration > 0)
                            break;
                    }
                    //OjwTmrMp3();
                    if (CTmr.Get() > 5000) break;
                    //COjwTimer.WaitTimer(50);
                    Application.DoEvents();
                }
                mpPlayer.Ctlcontrols.stop();
                mpPlayer.settings.volume = 100;
                chkMp3.Checked = true;

                m_strWorkDirectory_Mp3 = Ojw.CFile.GetPath(fileName);
                if (m_strWorkDirectory_Mp3 == null) m_strWorkDirectory_Mp3 = Application.StartupPath;

                //tmrMp3.Enabled = true;
            }
            else
            {
                //txtFileName.Text = "";
                //m_bFileOpened = false;
            }
            ofdMp3.Dispose();
        }
        private void SetPlayTime()
        {
            CheckTime();
            // 시간을 설정
            lbMp3Time.Text = String.Format("{0:D2}:", m_nMp3Minutes) + String.Format("{0:D2}.", m_nMp3Seconds) + String.Format("{0:D2}", m_nMp3MilliSeconds);
        }

        public int m_nMp3TmpValue = 0;
        public bool m_bMp3MouseClick = false;
        public bool m_bMp3Play = false;
        public int m_nMp3Seconds = 0;
        public int m_nMp3Minutes = 0;
        public int m_nMp3MilliSeconds = 0;
        // 플레이 시간 체크
        private void CheckTime()
        {
            double dTime;
            int nTime;

            // 현 위치의 시간을 얻는다.
            dTime = mpPlayer.Ctlcontrols.currentPosition;
            if (dTime < 0)
                dTime = 0.0;
            nTime = (int)dTime;
            // 시간을 설정
            m_nMp3Seconds = nTime % 60;
            m_nMp3Minutes = (int)(nTime / 60);
            m_nMp3MilliSeconds = (int)(dTime * 100.0) % 100;
        }
        
        private void InitMp3()
        {
            SetPlayTime(); // 진행시간 표시를 위해 시간 값 초기화
            //SetPlayState(0); // 플레이어 상태 설정 ( 0 - Ready, 1 - Paly, 2 - Pause, 3 - Stop, 4 - End, 5 - Eject )
        }

        private Point m_pntMain;
        private void Mp3Play()
        {
            try
            {
                //m_pntMain = this.Location;
                prbStatus.Visible = true;

                mpPlayer.Visible = true;
                // 동영상의 경우 Visible == true 이므로 이 경우에만 동영상 화면크기의 셋업을 실행하도록 한다.
                if (m_bScreen == true)
                {

                }

                //SetPlayState(1);

                mpPlayer.Ctlcontrols.play();
                m_bOneShotMp3Command = true;

                m_bMp3Play = true;

                Ojw.CMessage.Write("[Message] Playing Music...\r\n");

                //tmrMp3.Enabled = true;
            }
            catch (Exception error)
            {
                Ojw.CMessage.Write("[Message]" + error.ToString() + "\r\n");
                m_bMp3Play = false;
                mpPlayer.Visible = false;
                //SetPlayState(0);
            }
        }
        private void Mp3Stop()
        {
            try
            {
                mpPlayer.Ctlcontrols.stop();
                mpPlayer.Ctlcontrols.currentPosition = 0;
                m_bOneShotMp3Command = false;
                Ojw.CMessage.Write("[Message] Stoped Music...\r\n");
            }
            catch (Exception error)
            {
                Ojw.CMessage.Write("[Message]" + error.ToString() + "\r\n");
            }
            //SetPlayState(3);

            // 시간 초기화
            m_nMp3Seconds = 0;
            m_nMp3Minutes = 0;

            m_nMp3TmpValue = 0;

            // Play 상태 없앰
            m_bMp3Play = false;
            mpPlayer.Visible = false;

            prbStatus.Visible = false;

            //tmrMp3.Enabled = false;
        }
        private bool OjwMusicFileOpen(String strFileName)
        {
            // 파일 자체의 유무 확인
            bool bExist = ((CheckAudioExist(strFileName) == true) || (CheckMovieExist(strFileName) == true)) ? true : false;

            if (bExist == true) // 있다면 오픈
            {
                lbMp3File.Text = Ojw.CFile.GetTitle(strFileName);
                //m_strFileNameMp3 = fileName;
                String strExe = Ojw.CFile.GetExe(strFileName);

                if ((strExe.ToUpper() != "MP3") && (strExe.ToUpper() != "WAV")) m_bScreen = true;
                else m_bScreen = false;
                InitMp3();
                mpPlayer.URL = strFileName;
                // 파일 데이터 저장
                //TextConfigFileSave(m_strOrgDirectory + "\\ip.ini");
            }

            return bExist;
        }
        private void OjwTmrMp3()
        {
            SetPlayTime();

            // 파일의 전체 플레이 시간 가져오기
            double dTime = 0;
            int nTime;

            if (mpPlayer.Ctlcontrols.currentItem != null) dTime = mpPlayer.Ctlcontrols.currentItem.duration;


            if ((int)dTime < 0)
                dTime = 0.0;
            nTime = (int)dTime;
            m_dMp3_Max = dTime * 1000;
            int nTimeMilli = (int)(dTime * 100.0) % 100;
            // 시간을 설정
            lbMp3AllTime.Text = String.Format("{0:D2}:", nTime / 60) + String.Format("{0:D2}:", (int)(nTime % 60)) + String.Format("{0:D2}", nTimeMilli);
            if ((m_dMp3_Max > 0) && (dTime > 0))
            {
                prgMp3.Minimum = 0;
                prgMp3.Maximum = 100;// (int)m_dMp3_Max;
                prgMp3.Value = (int)(mpPlayer.Ctlcontrols.currentPosition / dTime * 100);//(int)(mpPlayer.Ctlcontrols.currentPosition * 1000);
            }
        }

        // Button
        frmPlayer frmPlayerForm;
        private void btnMp3Play_Click(object sender, EventArgs e)
        {
            Mp3Play();
        }

        private void btnMp3Stop_Click(object sender, EventArgs e)
        {
            Mp3Stop();
            if (frmPlayerForm != null) frmPlayerForm.CloseSplash();
        }
        private Ojw.CTimer m_CTmr_Start = new Ojw.CTimer();
        private void tmrMp3_Tick(object sender, EventArgs e)
        {
            if (m_bProgEnd == true) return;

            if (m_bStart == true)
            {
                //int nTimerValue = (int)m_CTmr_Start.Get();
                //prbStatus.Value = ((nTimerValue < prbStatus.Maximum) ? nTimerValue : prbStatus.Maximum);
            }

            if ((mpPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying) && (m_bOneShotMp3Command == true))
            {
                if (chkFullSize.Checked == true)
                {

                    Int32 currentMonitorCount = Screen.AllScreens.Length;

                    if ((currentMonitorCount < 2) || (chkDualMonitor.Checked == false))
                    {
                        //Put app in single screen mode.
                        frmPlayerForm = new frmPlayer();
                        frmPlayerForm.Show();

                        frmPlayerForm.Left = Screen.AllScreens[0].Bounds.Location.X;
                        frmPlayerForm.Top = Screen.AllScreens[0].Bounds.Location.Y;
                        frmPlayerForm.Size = Screen.AllScreens[0].Bounds.Size;//.WindowState = FormWindowState.Maximized;

                    }
                    else
                    {
                        //Put app in dual screen mode.
                        // 현재 내가 있는 화면의 반대편으로 이동하도록...
                        int nPos = 0;
                        if ((this.Left >= Screen.AllScreens[0].Bounds.Location.X) && ((this.Left <= (Screen.AllScreens[0].Bounds.Location.X + Screen.AllScreens[0].Bounds.Width))))
                            nPos = 1;
                        m_pntMain = this.Location;

                        frmPlayerForm = new frmPlayer();

                        //frmPlayerForm.BackColor = Color.Green;
                        //frmPlayerForm.Dock = DockStyle.Fill;
                        frmPlayerForm.Visible = false;
                        frmPlayerForm.Show();

                        frmPlayerForm.Left = Screen.AllScreens[nPos].Bounds.Location.X;
                        frmPlayerForm.Top = Screen.AllScreens[nPos].Bounds.Location.Y;
                        //frmPlayerForm.FormBorderStyle = FormBorderStyle.None;
                        frmPlayerForm.Size = Screen.AllScreens[nPos].Bounds.Size;//.WindowState = FormWindowState.Maximized;
                        frmPlayerForm.Visible = true;
                        this.Visible = false;
                        this.Location = Screen.AllScreens[nPos].Bounds.Location;
                    }


                    //mpPlayer.Left = sc[1].Bounds.Location.X;
                    //mpPlayer.Top = sc[1].Bounds.Location.Y;
                    //mpPlayer.SetBounds(sc[0].Bounds.Location.X, sc[0].Bounds.Location.Y, sc[0].Bounds.Width, sc[0].Bounds.Height);

                    //mpPlayer.playerApplication.switchToPlayerApplication();

                    mpPlayer.fullScreen = true;
                    this.Location = m_pntMain;
                    this.Visible = true;
                    m_bOneShotMp3Command = false;
                }
            }


            CheckPlayState();

            OjwTmrMp3();
        }

        private void CheckPlayState()
        {
            lbPlayState.Text = mpPlayer.status;
        }
        #endregion Mp3

        private bool m_bMouseDown = false;
        private double m_dMp3_Max = 0;
        private void prgMp3_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_dMp3_Max > 0)
            {
                m_bMouseDown = true;
                double dPos = (double)e.X / (double)prgMp3.Width * (double)m_dMp3_Max;//100;
                mpPlayer.Ctlcontrols.currentPosition = (double)(dPos / 1000.0);
            }
        }

        private void prgMp3_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bMouseDown == true)
            {
                double dPos = (double)e.X / (double)prgMp3.Width * (double)m_dMp3_Max;//100;
                mpPlayer.Ctlcontrols.currentPosition = (double)(dPos / 1000.0);
            }
        }

        private void prgMp3_MouseUp(object sender, MouseEventArgs e)
        {
            m_bMouseDown = false;
            if (chkMp3.Checked == true)
                ChangePos_Cell();
        }

        private void btnSync_Grid_Mp3_Click(object sender, EventArgs e)
        {
            ChangePos_Mp3Bar();
        }

        private void btnSync_Mp3_Grid_Click(object sender, EventArgs e)
        {
            ChangePos_Cell();
        }

        private void btnOpenDesignFile_Click(object sender, EventArgs e)
        {
            m_C3d.FileOpen();
        }

        private void btnCmd_Repeat_Click(object sender, EventArgs e)
        {
            String strValue = "1";
            if (Ojw.CInputBox.Show("Repeat Count", "Set the Repeat Count", ref strValue) == DialogResult.OK)
            {
                int nCnt = Ojw.CConvert.StrToInt(strValue);

                int nX_Limit = dgAngle.RowCount;
                int nY_Limit = dgAngle.ColumnCount;
                int nPos = nX_Limit;
                int nLineNumber = 0;
                for (int i = 0; i < nX_Limit; i++)
                {
                    for (int j = 0; j < nY_Limit; j++)
                    {
                        if (dgAngle[j, i].Selected == true)
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
                //m_C3d.m_CGridMotionEditor.SetColorGrid(0, dgAngle.RowCount);
            }
        }

        private void btnCmd_Clear_Click(object sender, EventArgs e)
        {
            int nX_Limit = dgAngle.RowCount;
            int nY_Limit = dgAngle.ColumnCount;
            for (int i = 0; i < nX_Limit; i++)
            {
                for (int j = 0; j < nY_Limit; j++)
                {
                    if (dgAngle[j, i].Selected == true)
                    {
                        m_C3d.m_CGridMotionEditor.SetCommand(i, 0);
                        m_C3d.m_CGridMotionEditor.SetData0(i, 0);
                        m_C3d.m_CGridMotionEditor.SetData1(i, 0);
                        if (m_C3d.m_CGridMotionEditor.GetGroup(i) == 4)
                        {
                            m_C3d.m_CGridMotionEditor.SetGroup(i, 0);
                        }
                        break;
                    }
                }
            }
            // 색칠하기...
            //m_C3d.GridMotionEditor_SetSelectedGroup(0);
        }


        #region For Wheel
        private void CheckWheel4Dir_Display(float f1, float f2, float f3, float f4)
        {
            int nDir = CheckGenieDir(f1, f2, f3);
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하
            switch (nDir)
            {
                case 1:
                    lbWheel4Dir.Text = "↑";
                    break;
                case 2:
                    lbWheel4Dir.Text = "↓";
                    break;
                case 3:
                    lbWheel4Dir.Text = "←";
                    break;
                case 4:
                    lbWheel4Dir.Text = "→";
                    break;
                case 5:
                    lbWheel4Dir.Text = "↖";
                    break;
                case 6:
                    lbWheel4Dir.Text = "↗";
                    break;
                case 7:
                    lbWheel4Dir.Text = "↙";
                    break;
                case 8:
                    lbWheel4Dir.Text = "↘";
                    break;
                default:
                    lbWheel4Dir.Text = "□";
                    break;
            }
        }
        
        private int CheckWheel4Dir(float f1, float f2, float f3, float f4)
        {
            int nDir = 0;
            float dist1, fVal, fMin = 0;
            if ((f1 * f1 + f2 * f2 + f3 * f3 + f4 * f4) != 0) // 정지가 아니라면
            {
                float cf1, cf2, cf3, cf4;
                for (int i = 1; i < 11; i++)
                {
                    CalcWheel4Dir(i, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out cf1, out cf2, out cf3, out cf4);
                    dist1 = (f1 - cf1) * (f1 - cf1) + (f2 - cf2) * (f2 - cf2) + (f3 - cf3) * (f3 - cf3) + (f4 - cf4) * (f4 - cf4);
                    fVal = (float)Math.Abs(dist1);
                    if ((nDir == 0) || (fVal < fMin))
                    {
                        nDir = i;
                        fMin = fVal;
                    }
                }
            }
            return nDir;
        }
        // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌턴(제자리), 4 - 우턴(제자리), 5 - 좌턴(전진하면서), 6 - 우턴(전진하면서), 7 - 좌턴(후진하면서), 8 - 우턴(후진하면서)
        private void CalcWheel4Dir(int nDir, float fSpeed, out float f1, out float f2, out float f3, out float f4)
        {
            float a;
            //float fRatio = 0.5f;// 0.75f;
            float fRatio_Org = 1.0f;// 0.75f;
            float fRatio_Turn = 2.0f;// 0.75f;
            float ff1 = 0.0f, ff2 = 0.0f, ff3 = 0.0f, ff4 = 0.0f;
            switch (nDir)
            {
                case 1: // 1 - 전진
                    a = (float)Math.Round(fSpeed);
                    ff1 = a; ff2 = a; ff3 = a; ff4 = a;
                    break;
                case 2: // 2 - 후진
                    a = -(float)Math.Round(fSpeed);
                    ff1 = a; ff2 = a; ff3 = a; ff4 = a;
                    break;
                case 3: // 3 - 좌턴(제자리)
                    a = (float)Math.Round(fSpeed);
                    ff1 = a; ff3 = a;
                    ff2 = -a; ff4 = -a;
                    break;
                case 4: // 4 - 우턴(제자리)
                    a = -(float)Math.Round(fSpeed);
                    ff1 = a; ff3 = a;
                    ff2 = -a; ff4 = -a;
                    break;
                case 5: // 5 - 좌턴(전진하면서)
                    a = (float)Math.Round(fSpeed);
                    ff1 = a * fRatio_Turn; ff3 = a * fRatio_Turn;
                    ff2 = a * fRatio_Org; ff4 = a * fRatio_Org;
                    break;
                case 6: // 6 - 우턴(전진하면서)
                    a = (float)Math.Round(fSpeed);
                    ff2 = a * fRatio_Turn; ff4 = a * fRatio_Turn;
                    ff1 = a * fRatio_Org; ff3 = a * fRatio_Org;
                    break;
                case 7: // 7 - 좌턴(후진하면서), 8 - 우턴(후진하면서)
                    a = -(float)Math.Round(fSpeed);
                    ff1 = a * fRatio_Turn; ff3 = a * fRatio_Turn;
                    ff2 = a * fRatio_Org; ff4 = a * fRatio_Org;
                    break;
                case 8: // 8 - 우턴(후진하면서)
                    a = -(float)Math.Round(fSpeed);
                    ff2 = a * fRatio_Turn; ff4 = a * fRatio_Turn;
                    ff1 = a * fRatio_Org; ff3 = a * fRatio_Org;
                    break;
                default: // 0 - 정지
                    ff1 = 0.0f;
                    ff2 = 0.0f;
                    ff3 = 0.0f;
                    ff4 = 0.0f;
                    break;
            }
            f1 = (float)Math.Round(ff1);
            f2 = (float)Math.Round(ff2);
            f3 = (float)Math.Round(ff3);
            f4 = (float)Math.Round(ff4);
        }
        private void btnWheel4_0_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(5, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }
        private void btnWheel4_1_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(1, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_2_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(6, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_3_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(3, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_4_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(0, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_5_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(4, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_6_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(7, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_7_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(2, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_8_Click(object sender, EventArgs e)
        {
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), true);
            m_C3d.Grid_SetFlag_Type(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), true);
            // 0 - 정지, 
            // 1 - 전진,                2 - 후진, 
            // 3 - 좌턴(제자리),        4 - 우턴(제자리), 
            // 5 - 좌턴(전진하면서),    6 - 우턴(전진하면서), 
            // 7 - 좌턴(후진하면서),    8 - 우턴(후진하면서)
            float f1, f2, f3, f4;
            CalcWheel4Dir(8, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3, out f4);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FR.Text), (float)Ojw.CMath.Round(f1, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_FL.Text), (float)Ojw.CMath.Round(f2, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RR.Text), (float)Ojw.CMath.Round(f3, 0));
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_RL.Text), (float)Ojw.CMath.Round(f4, 0));
        }

        private void btnWheel4_9_Click(object sender, EventArgs e)
        {

        }

        private void btnWheel4_10_Click(object sender, EventArgs e)
        {

        }
        #endregion For Wheel

        #region For Genie
        private void btnGenie_0_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(5, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);

            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_1_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(1, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_2_Click(object sender, EventArgs e)
        {

            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(6, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_3_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(3, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_4_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(0, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_5_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(4, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_6_Click(object sender, EventArgs e)
        {

            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(7, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_7_Click(object sender, EventArgs e)
        {

            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(2, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_8_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(8, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_9_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(9, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void btnGenie_10_Click(object sender, EventArgs e)
        {
            float f1, f2, f3;
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
            CalcGenieDir(10, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out f1, out f2, out f3);
            float fF1 = (float)Ojw.CMath.Round(f1, 0);
            float fF2 = (float)Ojw.CMath.Round(f2, 0);
            float fF3 = (float)Ojw.CMath.Round(f3, 0);


            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_0.Text), f1);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_1.Text), f2);
            m_C3d.GridMotionEditor_SetMotor(m_C3d.m_CGridMotionEditor.m_nCurrntCell, Ojw.CConvert.StrToInt(txtID_2.Text), f3);
        }

        private void CheckGenieDir_Display(float f1, float f2, float f3)
        {
            int nDir = CheckGenieDir(f1, f2, f3);
            // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 우회전, 10 - 좌회전 
            switch (nDir)
            {
                case 1:
                    lbGenieDir.Text = "↑";
                    break;
                case 2:
                    lbGenieDir.Text = "↓";
                    break;
                case 3:
                    lbGenieDir.Text = "←";
                    break;
                case 4:
                    lbGenieDir.Text = "→";
                    break;
                case 5:
                    lbGenieDir.Text = "↖";
                    break;
                case 6:
                    lbGenieDir.Text = "↗";
                    break;
                case 7:
                    lbGenieDir.Text = "↙";
                    break;
                case 8:
                    lbGenieDir.Text = "↘";
                    break;
                case 9:
                    lbGenieDir.Text = "tL";
                    break;
                case 10:
                    lbGenieDir.Text = "tR";
                    break;
                default:
                    lbGenieDir.Text = "□";
                    break;
            }
        }

        private int CheckGenieDir(float f1, float f2, float f3)
        {
            int nDir = 0;
            float dist1, fVal, fMin = 0;
            if ((f1 * f1 + f2 * f2 + f3 * f3) != 0) // 정지가 아니라면
            {
                float cf1, cf2, cf3;
                for (int i = 1; i < 11; i++)
                {
                    CalcGenieDir(i, Ojw.CConvert.StrToFloat(txtChangeValue.Text), out cf1, out cf2, out cf3);
                    dist1 = (f1 - cf1) * (f1 - cf1) + (f2 - cf2) * (f2 - cf2) + (f3 - cf3) * (f3 - cf3);
                    fVal = (float)Math.Abs(dist1);
                    if ((nDir == 0) || (fVal < fMin))
                    {
                        nDir = i;
                        fMin = fVal;
                    }
                }
            }
            return nDir;
        }

        // 0 - 정지, 1 - 전진, 2 - 후진, 3 - 좌, 4 - 우, 5 - 좌상, 6 - 우상, 7 - 좌하, 8 - 우하, 9 - 좌회전, 10 - 우회전 
        private void CalcGenieDir(int nDir, float fSpeed, out float f1, out float f2, out float f3)
        {
            float a;
            float ff1 = 0.0f, ff2 = 0.0f, ff3 = 0.0f;
            switch (nDir)
            {
                case 1: // 1 - 전진
                    // F1 = -F2, F1 < 0, F3 = 0
                    a = (float)Ojw.CMath.Cos(60.0f) * fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = -a;
                    ff2 = a;
                    ff3 = 0.0f;
                    break;
                case 2: // 2 - 후진
                    a = (float)Ojw.CMath.Cos(60) * fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = a;
                    ff2 = -a;
                    ff3 = 0.0f;
                    break;
                case 3: // 3 - 좌
                    a = (float)Ojw.CMath.Cos(60) * (-fSpeed);
                    a = (float)Math.Round(a);
                    ff1 = a;
                    ff2 = a;
                    ff3 = (-2.0f) * (a);
                    break;
                case 4: // 4 - 우
                    a = (float)Ojw.CMath.Cos(60) * (fSpeed);
                    a = (float)Math.Round(a);
                    ff1 = a;
                    ff2 = a;
                    ff3 = (-2.0f) * (a);
                    break;
                case 5: // 5 - 좌상
                    a = (float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = -a * ((float)Math.Sqrt(3.0f) + 3.0f) / (3.0f * (float)Math.Sqrt(3.0f));
                    ff2 = -a * ((float)Math.Sqrt(3.0f) - 3.0f) / (3.0f * (float)Math.Sqrt(3.0f));
                    ff3 = a * 2.0f / 3.0f;
                    break;
                case 6: // 6 - 우상
                    a = (float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = a * ((float)Math.Sqrt(3.0f) - 3.0f) / (3.0f * (float)Math.Sqrt(3.0f));
                    ff2 = a * ((float)Math.Sqrt(3.0f) + 3.0f) / (3.0f * (float)Math.Sqrt(3.0f));
                    ff3 = a * (-2.0f) / 3.0f;
                    break;
                case 7: // 7 - 좌하
                    a = (float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = -(a * ((float)Math.Sqrt(3.0f) - 3.0f) / (3.0f * (float)Math.Sqrt(3.0f)));
                    ff2 = -(a * ((float)Math.Sqrt(3.0f) + 3.0f) / (3.0f * (float)Math.Sqrt(3.0f)));
                    ff3 = (-a * (-2.0f) / 3.0f);
                    break;
                case 8: // 8 - 우하
                    a = (float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = -(-a * ((float)Math.Sqrt(3.0f) + 3.0f) / (3.0f * (float)Math.Sqrt(3.0f)));
                    ff2 = -(-a * ((float)Math.Sqrt(3.0f) - 3.0f) / (3.0f * (float)Math.Sqrt(3.0f)));
                    ff3 = (-a * 2.0f / 3.0f);
                    break;
                case 9: // 9 - 좌회전
                    a = -(float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = a;
                    ff2 = a;
                    ff3 = a;
                    break;
                case 10: // 10 - 우회전 
                    a = (float)fSpeed;
                    a = (float)Math.Round(a);
                    ff1 = a;
                    ff2 = a;
                    ff3 = a;
                    break;
                default: // 0 - 정지
                    ff1 = 0.0f;
                    ff2 = 0.0f;
                    ff3 = 0.0f;
                    break;
            }
            f1 = (float)Math.Round(ff1);
            f2 = (float)Math.Round(ff2);
            f3 = (float)Math.Round(ff3);
        }
        #endregion For Genie

        private void btnSimul_Click(object sender, EventArgs e)
        {
            Ojw.CTimer.Reset();
            m_C3d.SetSimulation_Smooth(chkSmooth.Checked);
            m_C3d.SetSimulation_With_PlayFrame(true);
            tmrRun.Enabled = true;
        }

        private void chkSmooth_CheckedChanged(object sender, EventArgs e)
        {
            m_C3d.SetSimulation_Smooth(chkSmooth.Checked);
        }
        
        private void frmMotionEditor_DragDrop(object sender, DragEventArgs e)
        {
            string[] file_name_array = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            int nCnt_Ojw = 0;
            int nCnt_Dmt = 0;
            int nCnt_Media = 0;
            foreach (string strItem in file_name_array)
            {
                #region Media File
                if (nCnt_Media == 0)
                {
                    if ((CheckMovieExist(strItem) == true) || (CheckAudioExist(strItem) == true))
                    {
                        lbMp3File.Text = Ojw.CFile.GetTitle(strItem);
                        String strExe = Ojw.CFile.GetExe(strItem);

                        if ((strExe.ToUpper() != "MP3") && (strExe.ToUpper() != "WAV")) m_bScreen = true;
                        else m_bScreen = false;
                        InitMp3();

                        mpPlayer.URL = strItem;
                        mpPlayer.settings.volume = 0;
                        mpPlayer.Ctlcontrols.play();
                        Ojw.CTimer CTmr = new Ojw.CTimer();
                        CTmr.Set();
                        //while (m_dMp3_Max <= 0)
                        while (true)
                        {
                            if (mpPlayer.Ctlcontrols.currentItem != null)
                            {
                                if (mpPlayer.Ctlcontrols.currentItem.duration > 0)
                                    break;
                            }
                            if (CTmr.Get() > 5000) break;
                            Application.DoEvents();
                        }
                        mpPlayer.Ctlcontrols.stop();
                        mpPlayer.settings.volume = 100;
                        chkMp3.Checked = true;

                        m_strWorkDirectory_Mp3 = Ojw.CFile.GetPath(strItem);
                        if (m_strWorkDirectory_Mp3 == null) m_strWorkDirectory_Mp3 = Application.StartupPath;

                        nCnt_Media++;
                    }
                }
                #endregion Media File
                #region Design File
                if (nCnt_Ojw == 0)
                {
                    if (
                        (strItem.ToLower().IndexOf(".ojw") > 0) ||
                        (strItem.ToLower().IndexOf(".dhf") > 0)
                        )
                    {
                        if (m_C3d.FileOpen(strItem) == true) // 모델링 파일이 잘 로드 되었다면 
                        {
                            Ojw.CMessage.Write("3d Modeling File Opened");

                            float[] afData = new float[3];
                            m_C3d.GetPos_Display(out afData[0], out afData[1], out afData[2]);
                            m_C3d.GetAngle_Display(out afData[0], out afData[1], out afData[2]);

                            m_C3d.m_strDesignerFilePath = Ojw.CFile.GetPath(strItem);
                            if (m_C3d.m_strDesignerFilePath == null) m_C3d.m_strDesignerFilePath = Application.StartupPath;

                            // File Restore
                            //m_C3d.FileRestore();


                            nCnt_Ojw++;
                        }
                    }
                }
                #endregion Design File
                #region DMT Motion File
                if (nCnt_Dmt == 0)
                {
                    if (strItem.ToLower().IndexOf(".dmt") > 0) // 논리적으로 0이 나올수가 없음.
                    {
                        if (m_bModify == true)
                        {
                            DialogResult dlgRet = MessageBox.Show("You have not yet saved the file. The data will be lost.\r\n\r\nDo you still want to open the file", "File Open", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            if (dlgRet != DialogResult.OK)
                                return;
                        }
                        
                        txtFileName.Text = strItem;
                        if (m_C3d.DataFileOpen(strItem, null) == false)
                        {
                            MessageBox.Show("This is not a Dmt motion file.");
                        }
                        else
                        {
                            Modify(false);
                            Grid_DisplayTime();

                            txtTableName.Text = m_C3d.GetMotionFile_Title();
                            txtComment.Text = m_C3d.GetMotionFile_Comment();
                            cmbStartPosition.SelectedIndex = m_C3d.GetMotionFile_StartPosition();

                            m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(strItem);
                            if (m_strWorkDirectory_Dmt == null) m_strWorkDirectory_Dmt = Application.StartupPath;
                            
                            nCnt_Dmt++;
                        }
                    }
                }
                #endregion DMT Motion File
            }
        }

        private void frmMotionEditor_DragEnter(object sender, DragEventArgs e) { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; }

        private void btnUserButton_Click(object sender, EventArgs e)
        {
            m_FUser();
        }

#if true

        private Ojw.CSerial m_CSerial = new Ojw.CSerial();
        private bool F_flash_read(out int nValue)
        {
            bool bRet = false;
            nValue = -1;
            if (Command_And_Wait((byte)('R')) == true)
            {
                Ojw.CTimer CTmr = new Ojw.CTimer();
                CTmr.Set(); while (CTmr.Get() < 1000) { if (m_CSerial.GetBuffer_Length() >= 2) break; else Thread.Sleep(1); }
                if (m_CSerial.GetBuffer_Length() >= 2)
                {
                    nValue = (int)((int)m_CSerial.GetByte() | ((int)m_CSerial.GetByte() << 8));
                    bRet = true;
                }
            }
            return bRet;
        }
        private bool F_flash_add_read(out int nValue)
        {
            bool bRet = false;
            nValue = -1;
            if (Command_And_Wait((byte)('a')) == true)
            {
                Ojw.CTimer CTmr = new Ojw.CTimer();
                CTmr.Set(); while (CTmr.Get() < 1000) { if (m_CSerial.GetBuffer_Length() >= 2) break; else Thread.Sleep(1); }
                if (m_CSerial.GetBuffer_Length() >= 2)
                {
                    nValue = (int)((int)m_CSerial.GetByte() | ((int)m_CSerial.GetByte() << 8));                    
                    bRet = true;
                }
            }
            return bRet;
        }
        #region Define Const
        private const int _FLASH_MTN_DMT10_HEADER_LEN					= 17;//33byte -> 17word
        private const int _FLASH_MTN_DMT10_HEADER_TABLE_NAME_IDX		= 3; //6byte -> 3word
        private const int _FLASH_MTN_DMT10_HEADER_MOTION_FRAME_SIZE_IDX	= 14; // 28byte -> 14word
        private const int _FLASH_MTN_DMT10_HEADER_ROBOT_MODEL_TYPE_IDX	= 15; // 30byte -> 15word
        private const int _FLASH_MTN_DMT10_HEADER_MOTOR_CNT_IDX			= 16; // 32byte -> 16word
        
        private const int _FLASH_MTN_DMT11_HEADER_LEN					= 17;//33byte -> 17word
        private const int _FLASH_MTN_DMT11_HEADER_TABLE_NAME_IDX		= 3; //6byte -> 3word
        private const int _FLASH_MTN_DMT11_HEADER_MOTION_FRAME_SIZE_IDX	= 14; // 28byte -> 14word
        private const int _FLASH_MTN_DMT11_HEADER_ROBOT_MODEL_TYPE_IDX	= 15; // 30byte -> 15word
        private const int _FLASH_MTN_DMT11_HEADER_MOTOR_CNT_IDX			= 16; // 32byte -> 16word
                
        private const int _FLASH_MTN_DMT12_HEADER_LEN_WO_MOTORS			= 17;//33byte -> 17word
        private const int _FLASH_MTN_DMT12_HEADER_TABLE_NAME_IDX		= 3; //6byte -> 3word
        private const int _FLASH_MTN_DMT12_HEADER_MOTION_FRAME_SIZE_IDX	= 14; // 28byte -> 14word
        private const int _FLASH_MTN_DMT12_HEADER_ROBOT_MODEL_TYPE_IDX	= 15; // 30byte -> 15word
        private const int _FLASH_MTN_DMT12_HEADER_MOTOR_CNT_IDX         = 16; // 32byte -> 16word
        #endregion Define Const

        private int[] m_anMotionAddress = new int[_SIZE_PAGE];
        private int[] m_anMotionSize = new int[_SIZE_PAGE];
        private List<string> m_lstMotionList = new List<string>();
        private bool F_action(int nBootloaderCommand)
        {
            switch(nBootloaderCommand)
            {
                case 0: // GetList
                    #region Get File List
                    {
                        GetFileList();
                    }
                    break;
                #endregion Get File List
                case 1: // Motion Download
                    #region Motion Download
                    {
                        #region 다운로드 폴더 지정
                        List<String> lstFiles = new List<string>();
                        lstFiles.Clear();
                        FolderBrowserDialog dlg = new FolderBrowserDialog();
                        dlg.SelectedPath = m_strWorkDirectory_Dmt;

                        if (dlg.ShowDialog() != DialogResult.OK) return false;

                        string strPath = dlg.SelectedPath;

                        //string strTest = String.Empty;

                        DirectoryInfo dirInfo = new DirectoryInfo(strPath);
                        if (dirInfo.Exists == true)
                        {
                            lstFiles.Clear();
                            foreach (FileInfo fileInfo in dirInfo.GetFiles("*.dmt"))
                            {
                                lstFiles.Add(fileInfo.ToString());
                                //strTest += fileInfo.ToString() + "\r\n";
                            }
                        }

                        m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(strPath);

                        //MessageBox.Show(strPath + "\r\n" + strTest);
                        //return;
                        #endregion 다운로드 폴더 지정

                        if (lstFiles.Count > 0)
                        {
                            int nAddress, nSize;
                            #region Delete All Motions
                            prbStatus.Visible = true;
                            lbMsg.Visible = true;
                            lbMsg.Text = "Memory Clearing...";
                            lbMsg.ForeColor = Color.Red;
                            prbStatus.Maximum = _SIZE_PAGE;
                            prbStatus.Value = 0;
                            Application.DoEvents();

                            for (int i = 0; i < _SIZE_PAGE; i++)
                            {
                                prbStatus.Value++;
                                Command_And_Wait_FlashAddWrite(0x8000 + 2 * i, false);
                                F_flash_read(out nAddress);
                                F_flash_read(out nSize);
                                // Motion 이 없는 경우 0xffff
                                if (
                                    ((nAddress != 0) && (nSize != 0)) && 
                                    ((nAddress != 0xffff) && (nSize != 0xff)) 
                                )
                                {
                                    DeleteMtnData(i);
                                }
                            }
                            prbStatus.Visible = false;
                            lbMsg.Visible = false;
                            #endregion Delete All Motions

                            #region Download Motions
                            bool bRet = Command_And_Wait_DownloadMotions(strPath, lstFiles);
                            if (bRet == false) MessageBox.Show("Cannot download any files");
                            #endregion Download Motions

                            #region GetFile List
                            if (bRet == true) GetFileList();
                            #endregion GetFile List
                        }
                        else
                        {
                            MessageBox.Show("There is no file...");
                        }
                    }
                    break;
                    #endregion Motion Download
                case 2: // Delete
                    #region Delete Motion
                    {
                        string strValue = "0";
                        if (Ojw.CInputBox.Show("Test", "Input Delete File Number", ref strValue) == System.Windows.Forms.DialogResult.OK)
                        {
                            DeleteMtnData(Ojw.CConvert.StrToInt(strValue));
                        }
                    }
                    break;
                    #endregion Delete Motion
                case 3: // Delete all
                    #region Delete All
                    {
                        int nAddress, nSize;
                        for (int i = 0; i < _SIZE_PAGE; i++)
                        {
                            Command_And_Wait_FlashAddWrite( 0x8000 + 2 * i, false);
                            F_flash_read(out nAddress);
                            F_flash_read(out nSize);
                            // Motion 이 없는 경우 0xffff
                            //if ((nAddress != 0xffff) && (nSize != 0xff))
                            if (
                                    ((nAddress != 0) && (nSize != 0)) && 
                                    ((nAddress != 0xffff) && (nSize != 0xff))
                                )
                            {
                                DeleteMtnData(i);
                            }
                        }
                        //string strValue = "0";
                        //if (Ojw.CInputBox.Show("Test", "Input Delete File Number", ref strValue) == System.Windows.Forms.DialogResult.OK)
                        //{
                        //    DeleteMtnData(Ojw.CConvert.StrToInt(strValue));
                        //}
                    }
                    break;
                    #endregion Delete All
            }
            return true;
        }
        private bool GetFileList()
        {            
            prbStatus.Visible = true;
            prbStatus.Maximum = _SIZE_PAGE;
            
            lbMsg.Text = "File List Checking...";
            //lbMsg.Font = new System.Drawing.Font("굴림", 8F);
            lbMsg.ForeColor = Color.Yellow;
            lbMsg.Visible = true;
            Application.DoEvents();

            prbStatus.Value = 0;
            bool bRet = false;
            #region GetList
            //int nCnt_NoMotion = 0;
            m_lstMotionList.Clear();
            byte[] pbyteBuff = new byte[_SIZE_PAGE * 2];
            //int nCnt_Ret = _SIZE_PAGE;
            for (int i = 0; i < _SIZE_PAGE; i++)
            {
                prbStatus.Value++;

                m_anMotionAddress[i] = 0;
                m_anMotionSize[i] = 0;
                int nAddress = 0x8000 + 2 * i;
                int nSize = -1;
                if (Command_And_Wait_FlashAddWrite(nAddress, false) == false) break;
                if (F_flash_read(out nAddress) == false) break;
                if (F_flash_read(out nSize) == false) break;

                //if ((nAddress != 0xffff) && (nSize != 0xffff))
                if (
                    ((nAddress != 0) && (nSize != 0)) && 
                    ((nAddress != 0xffff) && (nSize != 0xff))
                )
                {
                    //Check Version & Check if the motion is correct
                    bool bMotionDistorted = false;

                    // Read Version
                    if (Command_And_Wait_FlashAddWrite(nAddress, false) == false) break;
                    int nTmp0, nTmp1, nTmp2;
                    if (F_flash_read(out nTmp0) == false) break;
                    if (F_flash_read(out nTmp1) == false) break;
                    if (F_flash_read(out nTmp2) == false) break;
                    string strVer = String.Format("{0}{1}{2}{3}{4}{5}",
                                        (char)((nTmp0 >> 0) & 0xff),
                                        (char)((nTmp0 >> 8) & 0xff),
                                        (char)((nTmp1 >> 0) & 0xff),
                                        (char)((nTmp1 >> 8) & 0xff),
                                        (char)((nTmp2 >> 0) & 0xff),
                                        (char)((nTmp2 >> 8) & 0xff)
                                        );
                    int nFlashTableNameIdx = _FLASH_MTN_DMT10_HEADER_TABLE_NAME_IDX;
                    int nFlashFrameSizeIdx = _FLASH_MTN_DMT10_HEADER_MOTION_FRAME_SIZE_IDX;
                    int nFlashMotorCntIdx = _FLASH_MTN_DMT10_HEADER_MOTOR_CNT_IDX;
                    if (strVer == "DMT1.0")
                    {
                        nFlashTableNameIdx = _FLASH_MTN_DMT10_HEADER_TABLE_NAME_IDX;
                        nFlashFrameSizeIdx = _FLASH_MTN_DMT10_HEADER_MOTION_FRAME_SIZE_IDX;
                        nFlashMotorCntIdx = _FLASH_MTN_DMT10_HEADER_MOTOR_CNT_IDX;
                    }
                    else if (strVer == "DMT1.1")
                    {
                        nFlashTableNameIdx = _FLASH_MTN_DMT11_HEADER_TABLE_NAME_IDX;
                        nFlashFrameSizeIdx = _FLASH_MTN_DMT11_HEADER_MOTION_FRAME_SIZE_IDX;
                        nFlashMotorCntIdx = _FLASH_MTN_DMT11_HEADER_MOTOR_CNT_IDX;
                    }
                    else if (strVer == "DMT1.2")
                    {
                        nFlashTableNameIdx = _FLASH_MTN_DMT12_HEADER_TABLE_NAME_IDX;
                        nFlashFrameSizeIdx = _FLASH_MTN_DMT12_HEADER_MOTION_FRAME_SIZE_IDX;
                        nFlashMotorCntIdx = _FLASH_MTN_DMT12_HEADER_MOTOR_CNT_IDX;
                    }
                    else bMotionDistorted = true;


                    if (Command_And_Wait_FlashAddWrite(nAddress + nSize - 1, false) == false) break;
                    if (F_flash_read(out nTmp0) == false) break;

                    if ((char)(nTmp0 & 0xFF) != 'M' || (char)((nTmp0 >> 8) & 0xFF) != 'E')
                    {
                        if ((char)(nTmp0 & 0xFF) == 'E')
                        {
                            if (Command_And_Wait_FlashAddWrite(nAddress + nSize - 2, false) == false) break;
                            if (F_flash_read(out nTmp0) == false) break;
                            if ((char)((nTmp0 >> 8) & 0xFF) != 'M')
                                bMotionDistorted = true;
                        }
                        else
                            bMotionDistorted = true;
                    }

                    if (bMotionDistorted == true)
                    {
                        //nCnt_NoMotion++;
                        //if (nCnt_NoMotion > 3) break;// 연속적으로 모션이 없을 때 탈출

#if false // 굳이 삭제 하지 말아보자
                        Ojw.CMessage.Write("Motion {0} was distorted. Deleting Motion {1}...", i, i);
                        if (Command_And_Wait_FlashWriteMtnData(0xFF, 0x8000 + 2 * i, ref pbyteBuff) == false) break;
                        if (Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pbyteBuff) == false) break;
                        if (Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pbyteBuff) == false) break;
                        if (Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pbyteBuff) == false) break;
                        if (Command_And_Wait_WritePageFromBuf(pbyteBuff) == false) break;
#endif
                        //continue;
                        // 모션이 연달아 있지않으면 그냥 종료하자.
                        //break;
                        continue;
                    }
                    //else nCnt_NoMotion = 0;

                    m_anMotionAddress[i] = nAddress;
                    m_anMotionSize[i] = nSize;

                    //Display Address & Size
                    m_lstMotionList.Add(String.Format("{0}", i));
                    string strTmp = String.Format("0x{0}", nAddress);
                    Ojw.CMessage.Write(strTmp);
                    m_lstMotionList.Add(Ojw.CConvert.IntToStr(nSize));
                    m_lstMotionList.Add(strVer);
                    //strgidMtnMgrList->Cells[1][i + 1] = strtmp;
                    //strgidMtnMgrList->Cells[2][i + 1] = MtnSize;
                    //strgidMtnMgrList->Cells[3][i + 1] = cVersion;

                    //Read Table name and display
                    Command_And_Wait_FlashAddWrite(nAddress + nFlashTableNameIdx, false);
                    strTmp = String.Empty;
                    for (int j = 0; j < 10; j++)
                    {
                        if (F_flash_read(out nTmp0) == false) break;
                        if ((char)(nTmp0 & 0xFF) != '\0')
                        {
                            strTmp += (char)(nTmp0 & 0xFF);
                        }
                        if ((char)((nTmp0 >> 8) & 0xFF) != '\0')
                        {
                            strTmp += (char)((nTmp0 >> 8) & 0xFF);
                        }
                    }
                    if (F_flash_read(out nTmp0) == false) break;
                    if ((char)(nTmp0 & 0xFF) != '\0')
                    {
                        strTmp += (char)(nTmp0 & 0xFF);
                    }
                    //strgidMtnMgrList->Cells[4][i + 1] = strtmp;
                    m_lstMotionList.Add(strTmp);

                    //Read frame count and display
                    if (Command_And_Wait_FlashAddWrite(nAddress + nFlashFrameSizeIdx, false) == false) break;
                    if (F_flash_read(out nTmp0) == false) break;
                    //strgidMtnMgrList->Cells[5][i + 1] = wordtmp;
                    m_lstMotionList.Add(Ojw.CConvert.IntToStr(nTmp0));

                    //Read motor count and display
                    if (Command_And_Wait_FlashAddWrite(nAddress + nFlashMotorCntIdx, false) == false) break;
                    if (F_flash_read(out nTmp0) == false) break;
                    //strgidMtnMgrList->Cells[6][i + 1] = wordtmp;
                    m_lstMotionList.Add(Ojw.CConvert.IntToStr(nTmp0));// + ";");
                }
            }
            prbStatus.Visible = false;
            lbMsg.Visible = false;
            #endregion GetList
            return bRet;
        }
        private bool DeleteMtnData(int nIndex)
        {
            byte [] pageBuf = new byte[_SIZE_PAGE * 2];
            Array.Clear(pageBuf, 0, pageBuf.Length);
            if (Command_And_Wait_FlashWriteMtnData(0xFF, 0x8000 + (2 * nIndex), ref pageBuf) == false) return false;
            if (Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pageBuf) == false) return false;
            if (Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pageBuf) == false) return false;
            if (Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pageBuf) == false) return false;
            if (Command_And_Wait_WritePageFromBuf(pageBuf) == false) return false;

            m_anMotionAddress[nIndex] = 0;
            m_anMotionSize[nIndex] = 0;
            
            return true;
        }

        #region Download Motions
        private bool Command_And_Wait_DownloadMotions(String strPath, List<String> strFiles)
        {
            bool bRet = false;

            prbStatus.Visible = true;
            lbMsg.Visible = true;
            lbMsg.Text = "Motion DownLoading...";
            lbMsg.ForeColor = Color.Green;
            prbStatus.Value = 0;
            prbStatus.Maximum = strFiles.Count;
            Application.DoEvents();

            int nDownload = 0;
            for (int i = 0; i < strFiles.Count; i++)
            {
                prbStatus.Value++;
                nDownload += (Command_And_Wait_DownLoadMotion(String.Format("{0}\\{1}", strPath, strFiles[i]), i) == true) ? 1 : 0;
                lbMotion_Status.Text = String.Format("{0} / {1}", i + 1, strFiles.Count);
            }
            if (nDownload == strFiles.Count) bRet = true;

            lbMsg.Visible = false;
            prbStatus.Visible = false;
            return bRet;
        }
        private bool Command_And_Wait_DownLoadMotion(String strFileName, int nIndex)
        {
#if true
            int i, j;
	        int nMotionAddressMax=0, nMotionSizeOfMax=0, nMtnAddress, nMtnSize;
	        String strtmp, strVersion;
	        int nTmp;
	        bool bMotionDistorted;
            byte [] pageBuf = new byte[_SIZE_PAGE * 2];
            Array.Clear(pageBuf, 0, pageBuf.Length);
	        //char cVersion[7];
	        //int nFlashTableNameIdx,nFlashFrameSizeIdx,nFlashMotorCntIdx;

            int nFlashTableNameIdx = _FLASH_MTN_DMT10_HEADER_TABLE_NAME_IDX;
            int nFlashFrameSizeIdx = _FLASH_MTN_DMT10_HEADER_MOTION_FRAME_SIZE_IDX;
            int nFlashMotorCntIdx = _FLASH_MTN_DMT10_HEADER_MOTOR_CNT_IDX;

	        for(i=0;i<128;i++){
                if (nMotionAddressMax < m_anMotionAddress[i])
                {
                    nMotionAddressMax = m_anMotionAddress[i];
                    nMotionSizeOfMax = m_anMotionSize[i];
		        }
	        }
	        if(nMotionAddressMax <= 0x8100){
		        nMotionAddressMax = 0x8100;
	        }
	        //Result->Lines->Add(nMotionAddressMax);
	        //Result->Lines->Add(nMotionSizeOfMax);
	        //Bootloader->FlashWriteHmt(edMtnMgrSrcFile->Text, nIndex, nMotionAddressMax+nMotionSizeOfMax);
            bool bRet = FlashWriteDmt(strFileName, nIndex, nMotionAddressMax + nMotionSizeOfMax);

	        m_anMotionAddress[nIndex] = 0;
            m_anMotionSize[nIndex] = 0;

	        Command_And_Wait_FlashAddWrite(0x8000+2*nIndex, false);
	        F_flash_read(out nMtnAddress);
	        F_flash_read(out nMtnSize);
	        if(nMtnAddress != 0xFFFF && nMtnSize != 0xFFFF){
		        //Check if the motion is correct
		        bMotionDistorted = false;
                strVersion = String.Empty;
		        Command_And_Wait_FlashAddWrite(nMtnAddress, false);
		        F_flash_read(out nTmp);
                strVersion += (char)(nTmp & 0xFF);
                strVersion += (char)((nTmp & 0xFF00) >> 8);
		        F_flash_read(out nTmp);
                strVersion += (char)(nTmp & 0xFF);
                strVersion += (char)((nTmp & 0xFF00) >> 8);
		        F_flash_read(out nTmp);
                strVersion += (char)(nTmp & 0xFF);
                strVersion += (char)((nTmp & 0xFF00) >> 8);
		        //cVersion[6] = '\0';

                //strVersion.sOjw.CMessage.Write("%s", cVersion);
                if (strVersion == "DMT1.0")
                {
                    nFlashTableNameIdx = _FLASH_MTN_DMT10_HEADER_TABLE_NAME_IDX;
                    nFlashFrameSizeIdx = _FLASH_MTN_DMT10_HEADER_MOTION_FRAME_SIZE_IDX;
                    nFlashMotorCntIdx = _FLASH_MTN_DMT10_HEADER_MOTOR_CNT_IDX;
		        }
                else if (strVersion == "DMT1.1")
                {
                    nFlashTableNameIdx = _FLASH_MTN_DMT11_HEADER_TABLE_NAME_IDX;
                    nFlashFrameSizeIdx = _FLASH_MTN_DMT11_HEADER_MOTION_FRAME_SIZE_IDX;
                    nFlashMotorCntIdx = _FLASH_MTN_DMT11_HEADER_MOTOR_CNT_IDX;
		        }
                else if (strVersion == "DMT1.2")
                {
                    nFlashTableNameIdx = _FLASH_MTN_DMT12_HEADER_TABLE_NAME_IDX;
                    nFlashFrameSizeIdx = _FLASH_MTN_DMT12_HEADER_MOTION_FRAME_SIZE_IDX;
                    nFlashMotorCntIdx = _FLASH_MTN_DMT12_HEADER_MOTOR_CNT_IDX;
		        }
		        else{
			        bMotionDistorted = true;
		        }

		        Command_And_Wait_FlashAddWrite(nMtnAddress+nMtnSize-1, false);
		        F_flash_read(out nTmp);
		        if((char)(nTmp & 0xFF) != 'M' || (char)((nTmp & 0xFF00)>>8) != 'E'){
			        if((char)(nTmp & 0xFF) == 'E'){
				        Command_And_Wait_FlashAddWrite(nMtnAddress+nMtnSize-2, false);
				        F_flash_read(out nTmp);
				        if((char)((nTmp & 0xFF00)>>8) != 'M'){
					        bMotionDistorted = true;
				        }
			        }
			        else{
				        bMotionDistorted = true;
			        }
		        }
		        if(bMotionDistorted){
			        strtmp = String.Format("Motion %d was distorted. Deleting Motion %d...", nIndex, nIndex);
			        Command_And_Wait_FlashWriteMtnData(0xFF, 0x8000+2*nIndex, ref pageBuf);
			        Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pageBuf);
			        Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pageBuf);
			        Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pageBuf);
                    Command_And_Wait_WritePageFromBuf(pageBuf);
			        //Result->Lines->Add(strtmp);
			        return false;
		        }

                m_anMotionAddress[nIndex] = nMtnAddress;
                m_anMotionSize[nIndex] = nMtnSize;

		        //Display Address & Size
                //strtmp = String.Format("0x%04X", nMtnAddress);
		        //strgidMtnMgrList->Cells[1][nIndex+1] = strtmp;
		        //strgidMtnMgrList->Cells[2][nIndex+1] = nMtnSize;
		        //strgidMtnMgrList->Cells[3][nIndex+1] = strVersion;

		        //Read Table name and display
                Command_And_Wait_FlashAddWrite(nMtnAddress + nFlashTableNameIdx, false);
		        strtmp = "";
		        for (j=0;j<10;j++){
			        F_flash_read(out nTmp);
			        if((char)(nTmp & 0xFF)!='\0'){
				        strtmp += (char)(nTmp & 0xFF);
			        }
			        if((char)((nTmp & 0xFF00)>>8) != '\0'){
				        strtmp += (char)((nTmp & 0xFF00)>>8);
			        }
		        }
		        F_flash_read(out nTmp);
		        if((char)(nTmp & 0xFF)!='\0'){
			        strtmp += (char)(nTmp & 0xFF);
		        }
		        //strgidMtnMgrList->Cells[4][nIndex+1] = strtmp;
                Ojw.CMessage.Write(strtmp);

		        //Read frame count and display
                Command_And_Wait_FlashAddWrite(nMtnAddress + nFlashFrameSizeIdx, false);
		        F_flash_read(out nTmp);
		        //strgidMtnMgrList->Cells[5][nIndex+1] = nTmp;
                Ojw.CMessage.Write("{0}", nTmp);

		        //Read motor count and display
                Command_And_Wait_FlashAddWrite(nMtnAddress + nFlashMotorCntIdx, false);
		        F_flash_read(out nTmp);
                nTmp = nTmp & 0xFF;
		        //strgidMtnMgrList->Cells[6][nIndex+1] = nTmp;
                Ojw.CMessage.Write("{0}", nTmp);
                return true;
	        }
            return false;
#else
            bool bRet = false;
            int nMax_Address = 0;
            int nMax_Size = 0;
            //bool bMotionDistorted = false;
            //주소상으로 가장 뒤에 있는 모션의 주소와 사이즈를 알아냄
            for (int i = 0; i < 128; i++)
            {
                if (nMax_Address < m_anMotionAddress[i])
                {
                    nMax_Address = m_anMotionAddress[i];
                    nMax_Size = m_anMotionSize[i];
                }
            }
            if (nMax_Address <= 0x8100) nMax_Address = 0x8100;

            //가장 뒤에 있는 모션의 뒤에 새로운 모션 파일을 씀
            bRet = FlashWriteDmt(strFileName, nIndex, nMax_Address + nMax_Size);
            return bRet;
#if false
            //이 후의 내용은 제대로 쓰여 졌는지 확인하고 Display하는 루틴임
            m_anMotionAddress[nIndex] = 0;
            m_anMotionSize[nIndex] = 0;
            bool bMotionDistorted = false;
            Command_And_Wait_FlashAddWrite(0x8000 + 2 * nIndex, false);
            int nAddress, nSize, nTmp;
            F_flash_read(out nAddress);
            F_flash_read(out nSize);
            if (nAddress != 0xFFFF && nSize != 0xFFFF)
            {
                //Check if the motion is correct
                bMotionDistorted = false;
                Command_And_Wait_FlashAddWrite(nAddress + nSize - 1, false);
                F_flash_read(out nTmp);
                if ((char)(nTmp & 0xFF) != 'M' || (char)((nTmp & 0xFF00) >> 8) != 'E')
                {
                    if ((char)(nTmp & 0xFF) == 'E')
                    {
                        Command_And_Wait_FlashAddWrite(nAddress + nSize - 2, false);
                        F_flash_read(out nTmp);
                        if ((char)((nTmp & 0xFF00) >> 8) != 'M')
                        {
                            bMotionDistorted = true;
                        }
                    }
                    else
                    {
                        bMotionDistorted = true;
                    }
                }
                if (bMotionDistorted)
                {
                    byte[] pageBuf = new byte[_SIZE_PAGE * 2];
                    Array.Clear(pageBuf, 0, pageBuf.Length);
                    // Motion was distorted. Deleting Motion i
                    Command_And_Wait_FlashWriteMtnData(0xFF, 0x8000 + 2 * nIndex, ref pageBuf);
                    Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pageBuf);
                    Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pageBuf);
                    Command_And_Wait_FlashWriteMtnData(0xFF, 0, ref pageBuf);
                    Command_And_Wait_WritePageFromBuf(pageBuf);
                    return false;
                }

                m_anMotionAddress[nIndex] = nAddress;
                m_anMotionSize[nIndex] = nSize;
            }
            return bRet;
#endif
#endif
        }
        private const int _BOOT_START = 0xF800;
        private bool FlashWriteDmt(string strFilename, int nMotionNo, int nAddress)
        {
            bool bRet = true;
            Ojw.SMotion_t SMotion = new Ojw.SMotion_t();
            int i, j;
            if (m_C3d.BinaryFileOpen(strFilename, out SMotion) == true)
            {

            }
            else return false;

            int nDataLen = 0;

            byte[] pbytePageBuf = new byte[256];

            /////////////수정 20120116/////////////
            int nMotionFrameSize = 0;//SMotion.nFrameSize;

            // 실제 Enable 된 프레임만 카운트 한다.
            foreach (Ojw.SMotionTable_t STable in SMotion.STable) { if (STable.bEn == true) nMotionFrameSize++; }


            int nMotorCnt = SMotion.nMotorCnt;

            if (nAddress + (33 + (2 * nMotorCnt + 9) * nMotionFrameSize + 2 + 1) / 2 > _BOOT_START)
            {
                return false;
            }
            String strMotion = "DMT1.0"; // 나중에 가변으로 ? 
            Command_And_Wait_FlashWriteMtnData((byte)strMotion[0], nAddress, ref pbytePageBuf);
            for (i = 1; i < strMotion.Length; i++) Command_And_Wait_FlashWriteMtnData((byte)strMotion[i], 0, ref pbytePageBuf);

            // File Name
            StringBuilder sb = new StringBuilder(Ojw.CFile.GetTitle(strFilename));//(SMotion.strTableName);
            for (i = 0; i < 20; i++)
            {
                byte byteData = (byte)((i < sb.Length) ? sb[i] : 0);
                Command_And_Wait_FlashWriteMtnData(byteData, 0, ref pbytePageBuf);
            }
            Command_And_Wait_FlashWriteMtnData((byte)0, 0, ref pbytePageBuf);

            Command_And_Wait_FlashWriteMtnData((byte)(SMotion.nStartPosition & 0xff), 0, ref pbytePageBuf);
            Command_And_Wait_FlashWriteMtnData((byte)(SMotion.nFrameSize & 0xff), 0, ref pbytePageBuf);
            Command_And_Wait_FlashWriteMtnData((byte)((SMotion.nFrameSize >> 8) & 0xff), 0, ref pbytePageBuf);
            Command_And_Wait_FlashWriteMtnData((byte)(SMotion.nRobotModelNum & 0xff), 0, ref pbytePageBuf);
            Command_And_Wait_FlashWriteMtnData((byte)((SMotion.nRobotModelNum >> 8) & 0xff), 0, ref pbytePageBuf);
            Command_And_Wait_FlashWriteMtnData((byte)(SMotion.nMotorCnt & 0xff), 0, ref pbytePageBuf);

            nDataLen += 33;

            for (i = 0; i < nMotionFrameSize; i++)
            {
                if (SMotion.STable[i].bEn == true)
                {
                    //Motor Data
                    for (j = 0; j < nMotorCnt; j++)
                    {
                        Command_And_Wait_FlashWriteMtnData((byte)(SMotion.STable[i].anMot[j] & 0xff), 0, ref pbytePageBuf);
                        Command_And_Wait_FlashWriteMtnData((byte)(SMotion.STable[i].anMot[j] >> 8 & 0xff), 0, ref pbytePageBuf);
                    }

                    //Playtime
                    Command_And_Wait_FlashWriteMtnData((byte)(SMotion.STable[i].nTime & 0xff), 0, ref pbytePageBuf);
                    Command_And_Wait_FlashWriteMtnData((byte)(SMotion.STable[i].nTime >> 8 & 0xff), 0, ref pbytePageBuf);

                    //Delay
                    Command_And_Wait_FlashWriteMtnData((byte)(SMotion.STable[i].nDelay & 0xff), 0, ref pbytePageBuf);
                    Command_And_Wait_FlashWriteMtnData((byte)(SMotion.STable[i].nDelay >> 8 & 0xff), 0, ref pbytePageBuf);

                    //Command
                    Command_And_Wait_FlashWriteMtnData((byte)(SMotion.STable[i].nCmd & 0xff), 0, ref pbytePageBuf);

                    //Data0
                    Command_And_Wait_FlashWriteMtnData((byte)(SMotion.STable[i].nData0 & 0xff), 0, ref pbytePageBuf);
                    Command_And_Wait_FlashWriteMtnData((byte)(SMotion.STable[i].nData0 >> 8 & 0xff), 0, ref pbytePageBuf);
                    //Data1
                    Command_And_Wait_FlashWriteMtnData((byte)(SMotion.STable[i].nData1 & 0xff), 0, ref pbytePageBuf);
                    Command_And_Wait_FlashWriteMtnData((byte)(SMotion.STable[i].nData1 >> 8 & 0xff), 0, ref pbytePageBuf);

                    nDataLen += nMotorCnt * 2 + 9;
                }
            }

            Command_And_Wait_FlashWriteMtnData((byte)('M'), 0, ref pbytePageBuf);
            Command_And_Wait_FlashWriteMtnData((byte)('E'), 0, ref pbytePageBuf);
            nDataLen += 2;

            Command_And_Wait_WritePageFromBuf(pbytePageBuf);
            nDataLen = (nDataLen + 1) / 2; //converting from byte address to word address

            int nPtrAddress = 0x8000 + 2 * nMotionNo;
            Command_And_Wait_FlashWriteMtnData((byte)(nAddress & 0x00FF), nPtrAddress, ref pbytePageBuf);
            Command_And_Wait_FlashWriteMtnData((byte)((nAddress & 0xFF00) >> 8), 0, ref pbytePageBuf);
            Command_And_Wait_FlashWriteMtnData((byte)(nDataLen & 0x00FF), 0, ref pbytePageBuf);
            Command_And_Wait_FlashWriteMtnData((byte)((nDataLen & 0xFF00) >> 8), 0, ref pbytePageBuf);
            Command_And_Wait_WritePageFromBuf(pbytePageBuf);
            return bRet;
        }
        #endregion Download Motions
        
        private int m_nCurrentAddress = 0;
        private int m_nBufIdx = 0;
        private const int _SIZE_PAGE = 128;
        private bool Command_And_Wait_FlashWriteMtnData(byte byteData, int nAddress, ref byte[] pbyteBuffer)
        {
            int nRet = 1;
            int nMaxTmp = -999999;
            
            if (nAddress != 0)
            {
                if (Command_And_Wait_FlashAddWrite(nAddress, false) == true)
                {
                    m_nCurrentAddress = nAddress;
                    m_nBufIdx = (nAddress % _SIZE_PAGE) * 2;
                    nRet += ((Command_And_Wait_LoadPageToBuf(ref pbyteBuffer) == true) ? 1 : nMaxTmp);
                }
            }

            pbyteBuffer[m_nBufIdx] = byteData;
            if (m_nBufIdx % 2 == 1)
            {
                m_nCurrentAddress++;
            }

            if (m_nBufIdx == (2 * _SIZE_PAGE - 1))
            {
                nRet += ((Command_And_Wait_WritePageFromBuf(pbyteBuffer) == true) ? 1 : nMaxTmp);
                nRet += ((Command_And_Wait_FlashAddWrite(m_nCurrentAddress, false) == true) ? 1 : nMaxTmp);
                nRet += ((Command_And_Wait_LoadPageToBuf(ref pbyteBuffer) == true) ? 1 : nMaxTmp);
                m_nBufIdx = 0;
            }
            else
            {
                m_nBufIdx++;
            }
            return ((nRet > 0) ? true : false);
        }
        private bool Command_And_Wait_WritePageFromBuf(byte[] pbyteData)
        {
            int nRet = 0;
            int nMaxTmp = -999999;

            int nAddress = 0;
            if (F_flash_add_read(out nAddress) == false) return false; 
            nRet += ((Command_And_Wait_FlashAddWrite((int)(nAddress / _SIZE_PAGE) * _SIZE_PAGE, false) == true) ? 1 : nMaxTmp);
            for (int i = 0; i < _SIZE_PAGE; i++)
            {
                if (nRet < 0) return false;
                if (Command_And_Wait((byte)('T')) == true)
                {
                    nRet += ((Command_And_Wait(pbyteData[2 * i]) == true) ? 1 : nMaxTmp);
                    if (nRet < 0) return false;
                    nRet += ((Command_And_Wait(pbyteData[2 * i + 1]) == true) ? 1 : nMaxTmp);
                    if (nRet < 0) return false;
                }
                else
                {
                    //nRet += nMaxTmp;
                    //break;
                    return false;                    
                }
            }
            nRet += ((Command_And_Wait_FlashAddWrite(nAddress, false) == true) ? 1 : nMaxTmp);
            nRet += ((Command_And_Wait_ErasePage() == true) ? 1 : nMaxTmp);
            nRet += ((Command_And_Wait_WritePage() == true) ? 1 : nMaxTmp);
            return ((nRet > 0) ? true : false);
        }
        private bool Command_And_Wait_WritePage()
        {
            bool bRet = false;
            if (SendPacket_And_Wait((byte)('W')) == true)
            {
                Ojw.CTimer CTmr = new Ojw.CTimer();
                CTmr.Set(); while (CTmr.Get() < 1000) { if (m_CSerial.GetBuffer_Length() >= 1) break; else Thread.Sleep(1); }
                if (m_CSerial.GetBuffer_Length() >= 1)
                {
                    byte byteData = m_CSerial.GetByte();
                    if (byteData == (byte)('?')) Ojw.CMessage.Write("Cannot Write booltloader area");
                    else if (byteData == (byte)('W')) Ojw.CMessage.Write("At WritePage :Unexpected Response");
                    else bRet = true;
                }
            }
            return bRet;
        }
        private bool Command_And_Wait_ErasePage()
        {
            bool bRet = false;
            if (SendPacket_And_Wait((byte)('E')) == true)
            {
                Ojw.CTimer CTmr = new Ojw.CTimer();
                CTmr.Set(); while (CTmr.Get() < 1000) { if (m_CSerial.GetBuffer_Length() >= 1) break; else Thread.Sleep(1); }
                if (m_CSerial.GetBuffer_Length() >= 1)
                {
                    byte byteData = m_CSerial.GetByte();
                    if (byteData == (byte)('?')) Ojw.CMessage.Write("Cannot Erase booltloader area");
                    else if (byteData == (byte)('E')) Ojw.CMessage.Write("At ErasePage :Unexpected Response");
                    else bRet = true;
                }
            }
            return bRet;
        }
        private bool Command_And_Wait_LoadPageToBuf(ref byte [] pbyteData)
        {
            int nRet = 0;
            int nMaxTmp = -999999;

            int nAddress = 0;
            nRet += ((F_flash_add_read(out nAddress) == true) ? 1 : nMaxTmp);
            nRet += ((Command_And_Wait_FlashAddWrite((int)(nAddress / _SIZE_PAGE) * _SIZE_PAGE, false) == true) ? 1 : nMaxTmp);

            for (int i = 0; i < _SIZE_PAGE; i++)
            {
                if (nRet < 0) return false;
                if (Command_And_Wait((byte)('R')) == true)
                {
                    Ojw.CTimer CTmr = new Ojw.CTimer();
                    CTmr.Set(); while (CTmr.Get() < 1000) { if (m_CSerial.GetBuffer_Length() >= 2) break; else Thread.Sleep(1); }
                    if (m_CSerial.GetBuffer_Length() >= 2)
                    {
                        pbyteData[2 * i] = m_CSerial.GetByte();
                        pbyteData[2 * i + 1] = m_CSerial.GetByte();
                        nRet++;
                    }                    
                }
                else
                {
                    //nRet += nMaxTmp;
                    //break;
                    return false;
                }
            }
            nRet += ((Command_And_Wait_FlashAddWrite(nAddress, false) == true) ? 1 : nMaxTmp);
            return ((nRet > 0) ? true : false);
        }
        
        private void F_enter_bootloader(int nMpsuID, int nPort, int nBaudrate, int nBootloaderCommand)
        {
            #region Timer 정지
            bool bTmrDraw = tmrDraw.Enabled;
            bool bTmrMp3 = tmrMp3.Enabled;
            bool bTmrRun = tmrRun.Enabled;
            tmrDraw.Enabled = false;
            tmrMp3.Enabled = false;
            tmrRun.Enabled = false;
            #endregion Timer 정지

            Ojw.CTimer.Wait(100);

            #region MPSU Motion DownLoad
            int nID = nMpsuID;
            m_C3d.m_CMotor.Mpsu_Reboot(nID);
            bool bRet = m_C3d.m_CMotor.WaitReceive_Mpsu(100);
            if (bRet == true)
            {
                m_C3d.m_CMotor.ResetCounter_Mpsu(); // 명령을 보내지 않았지만 부팅시 저절로 들어오는 패킷 명령을 받는 이벤트를 확인하기 위해...
                bRet = m_C3d.m_CMotor.WaitReceive_Mpsu(3000);
                if (bRet == true)
                {
                    // 포트를 따로 연다.
                    if (m_C3d.m_CMotor.IsConnect() == true)
                    {
                        int i;
                        Ojw.CTimer CTmr = new Ojw.CTimer();
                        m_CSerial = new Ojw.CSerial();
                        m_C3d.m_CMotor.DisConnect();

                        // 전체 기능 버튼 Disable 항목을 넣도록 한다.

                        //

                        m_CSerial.Connect(nPort, nBaudrate);
                        if (m_CSerial.IsConnect() == true)
                        {
                            bool bEntered = false;
                            if (Command_And_Wait((byte)('S')) == true)
                            {
                                //MessageBox.Show("We have entered bootloader");
                                bEntered = true;

                                F_action(nBootloaderCommand);
                            }
                            else
                            {
                                //MessageBox.Show("We could not enter bootloader");
                            }



                            if (bEntered == true)
                            {
                                // Quit Bootloader
                                if (Command_And_Wait((byte)('Q')) == true)
                                {
                                }
                            }
                        }

                        // 전체 기능 버튼 Restore 항목을 넣도록 한다.

                        //

                        // return
                        m_CSerial.DisConnect();
                        m_C3d.m_CMotor.Connect(nPort, nBaudrate);
                        //MessageBox.Show("Downloaded All motion files");
                    }
                }
                else MessageBox.Show("reboot fail");

                //MessageBox.Show("부트로더 진입 ok");
            }
            else //MessageBox.Show("부트로더 진입 fail");
            {
                MessageBox.Show("we could not enter the [bootloader]");
            }
            #endregion MPSU Motion DownLoad
            
            #region Timer 복구
            tmrDraw.Enabled = bTmrDraw;
            tmrMp3.Enabled = bTmrMp3;
            tmrRun.Enabled = bTmrRun;
            #endregion Timer 복구

            if (
                (nBootloaderCommand == 0) || // GetList
                (nBootloaderCommand == 1)    // Motion Download
                ) 
            {
                string strMsg = String.Empty;
                if (m_lstMotionList.Count > 0)
                {
                    int nMax = 6;
                    int nNum = -1;
                    m_lstDownload.Clear();
                    string strName = String.Empty;
                    for (int i = 0; i < m_lstMotionList.Count; i++)
                    {
                        int nIndex = i % nMax;
                        if (nIndex == 0) nNum = Ojw.CConvert.StrToInt(m_lstMotionList[i]);//{ strMsg += String.Format("[{0}] ", Ojw.CConvert.FillString(m_lstMotionList[i], "0", 3, false)); nNum = Ojw.CConvert.StrToInt(m_lstMotionList[i]); }
                        else if (nIndex == 3) strName = m_lstMotionList[i];//{ strMsg += String.Format("{0}", m_lstMotionList[i]); strName = m_lstMotionList[i]; }
                        else if (nIndex == (nMax - 1)) m_lstDownload.Add(new SDownloadList_t(nNum, strName)); //{ strMsg += "\r\n"; m_lstDownload.Add(new SDownloadList_t(nNum, strName)); }
                        //strMsg += String.Format("{0}{1}", Ojw.CConvert.FillString(m_lstMotionList[i], " ", 25, true), ((((i + 1) % 6) == 0) ? "\r\n" : "\t,"));
                    }
                    //foreach (String strItem in m_lstMotionList)
                    //{
                    //    strMsg += String.Format("0\r\n", strItem);
                    //}
                    //strMsg += "Done";
                    //listviewMotion
                    listviewMotion.Items.Clear();
                    foreach (SDownloadList_t SDown in m_lstDownload) listviewMotion.Items.Add(String.Format("[{0}] {1}", Ojw.CConvert.FillString(Ojw.CConvert.IntToStr(SDown.nIndex), "0", 3, false), SDown.strName));
                    tcControl.SelectedIndex = 2;
                }
                else { strMsg = String.Format("Cannot find any files."); MessageBox.Show(strMsg); }
            }
        }
        private struct SDownloadList_t { public int nIndex; public String strName; public SDownloadList_t(int index, String name) { nIndex = index; strName = name; } }
        private List<SDownloadList_t> m_lstDownload = new List<SDownloadList_t>();
        private bool Command_And_Wait_FlashAddWrite(int nAddress, bool bMessage)
        {
            bool bRet = false;
            if (Command_And_Wait((byte)('A')) == true)
            {
                // 하위주소번지를 던짐
                if (Command_And_Wait((byte)(nAddress & 0x00FF)) == true)
                {
                    // 상위주소번지를 던짐
                    if (Command_And_Wait((byte)((nAddress >> 8) & 0x00FF)) == true) bRet = true;
                }
            }
            if (bMessage == true)
            {
                if (bRet == true)   Ojw.CMessage.Write("Flash address changed to [{0}]", nAddress);
                else                Ojw.CMessage.Write("Setting Flash Address [{0}] failed", nAddress);
            }
            return bRet;
        }
        private bool Command_And_Wait(byte byteData)
        {
            bool bRet = false;
            if (SendPacket_And_Wait(byteData) == true)
            {
                if (m_CSerial.GetByte() == byteData) bRet = true;
            }
            return bRet;
        }
        private void SendPacket(byte[] pbyteData)
        {
            //if (m_CSock.IsConnect() == true) 
            //    m_CSock.Send(pbyteData);
            //else 
                m_CSerial.SendPacket(pbyteData);
        }
        private bool SendPacket_And_Wait(byte byteData)
        {
            if (m_CSerial.IsConnect() == false) return false;
            bool bRet = false;
            Ojw.CTimer CTmr = new Ojw.CTimer();
            byte[] pbyteData = new byte[1];
            pbyteData[0] = byteData;
            //m_CSerial.SendPacket(pbyteData);
            SendPacket(pbyteData);
            CTmr.Set();
            while (CTmr.Get() < 1000)
            {
                if (m_CSerial.GetBuffer_Length() > 0) { bRet = true; break; }
                else Thread.Sleep(1);// Application.DoEvents();
            }
            return bRet;
        }
        private bool SendPacket_And_Wait(byte[] pbyteData)
        {
            if (m_CSerial.IsConnect() == false) return false;
            bool bRet = false;
            Ojw.CTimer CTmr = new Ojw.CTimer();
            //m_CSerial.SendPacket(pbyteData);
            SendPacket(pbyteData);
            CTmr.Set();
            while (CTmr.Get() < 1000)
            {
                if (m_CSerial.GetBuffer_Length() > 0) { bRet = true; break; }
                else Thread.Sleep(1);// Application.DoEvents();
            }
            return bRet;
        }

        private void listviewMotion_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int nPos = listviewMotion.SelectedItems.Count - 1;
            if (nPos >= 0)
            {
                int nNum = listviewMotion.SelectedItems[nPos].Index;
                m_C3d.m_CMotor.Mpsu_Play_Motion(m_lstDownload[nNum].nIndex);
                //MessageBox.Show(String.Format("FileNumber - {0}", m_lstDownload[nNum].nIndex));
            }
        }

        private void btnMotion_Delete_Click(object sender, EventArgs e)
        {
            // test
            //int nCommand = 3; // 1 // 0 - Get Motion List, 1 - Motion Download, 2 - Delete, 3 - Delete All
            //F_enter_bootloader(253, Ojw.CConvert.StrToInt(txtPort.Text), Ojw.CConvert.StrToInt(txtBaudrate.Text), nCommand);
        }

        private void btnMotion_Download_Click(object sender, EventArgs e)
        {
            int nCommand = 1; // 1 // 0 - Get Motion List, 1 - Motion Download, 2 - Delete, 3 - Delete All
            F_enter_bootloader(253, Ojw.CConvert.StrToInt(txtPort.Text), Ojw.CConvert.StrToInt(txtBaudrate.Text), nCommand);
        }

        private void btnMotion_GetList_Click(object sender, EventArgs e)
        {
            int nCommand = 0; // 1 // 0 - Get Motion List, 1 - Motion Download, 2 - Delete, 3 - Delete All
            F_enter_bootloader(253, Ojw.CConvert.StrToInt(txtPort.Text), Ojw.CConvert.StrToInt(txtBaudrate.Text), nCommand);
        }

        private void btnMotion_Play_Click(object sender, EventArgs e)
        {
            int nPos = listviewMotion.SelectedItems.Count - 1;
            if (nPos >= 0)
            {
                int nNum = listviewMotion.SelectedItems[nPos].Index;
                m_C3d.m_CMotor.Mpsu_Play_Motion(m_lstDownload[nNum].nIndex);
                //MessageBox.Show(String.Format("FileNumber - {0}", m_lstDownload[nNum].nIndex));
            }
        }

        private void btnDirRefresh_Click(object sender, EventArgs e)
        {
            // Folder
            SetTreeDrive();
        }

        private void treeInfo_AfterSelect(object sender, TreeViewEventArgs e)
        {
            lblPath.Text = e.Node.FullPath;
            SetListView(e.Node.FullPath);
        }

        private void treeInfo_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            // 축소되는 노드의 하위 노드를 모두 삭제
            // 확장되는 노드의 하위 노드를 모두 삭제
            while (true)
            {
                if (e.Node.FirstNode == null) break;
                e.Node.Nodes[0].Remove();
            }

            // 하위폴더가 있으면 "???" 추가
            if (HasSubFolder(e.Node.FullPath))
                e.Node.Nodes.Add("???");


            // 축소한 노드가 선택된 효과를 준다.
            treeInfo.SelectedNode = e.Node;
        }

        private void treeInfo_AfterExpand(object sender, TreeViewEventArgs e)
        {
            // 확장되는 노드의 하위 노드를 모두 삭제
            while (true)
            {
                if (e.Node.FirstNode == null) break;
                e.Node.Nodes[0].Remove();
            }

            // 실제 하위 폴더를 찾아서 삽입
            SetTreeNode(e.Node);
        }

        private bool CheckHeader(String strItemName, out SFileHeader_t SFileHeader)
        {
            bool bRet = false;
            int nNum = 0;
            for (int i = 0; i < m_nFileHeader; i++)
            {
                if (m_pSFileHeader[i].bActFile)
                {
                    if (m_pSFileHeader[i].strFileName == strItemName)
                    {
                        bRet = true;
                        nNum = i;
                    }
                }
            }

            SFileHeader = m_pSFileHeader[nNum];

            return bRet;
        }

        private void lstInfo_DoubleClick(object sender, EventArgs e)
        {
            String strSel;
            String strSize;

            strSel = lstInfo.SelectedItems[0].SubItems[0].Text;
            strSize = lstInfo.SelectedItems[0].SubItems[1].Text;

            if (strSize.Equals("")) //폴더일 경우
            {
                TreeNode treeNodeTmp;

                // 현재 선택 노드의 확장 효과를 준다.
                treeInfo.SelectedNode.Expand();

                // 트리에서 동일한 이름의 노드를 찾는다.
                treeNodeTmp = FindNode(strSel);

                // 찾은 노드를 선택한 효과와 확장한 효과를 준다.
                treeInfo.SelectedNode = treeNodeTmp;
                treeNodeTmp.Expand();
            }
            else //파일일 경우
            {
                //ShellExecute(0, "open", lblPath.Text + "\\" + strSel, null, null, SW_SHOWNORMAL);
                if (CheckDmtExist(lblPath.Text + "\\" + strSel) == true)
                {
                    if (m_bModify == true)
                    {
                        DialogResult dlgRet = MessageBox.Show("You have not yet saved the file. The data will be lost.\r\n\r\nDo you still want to open the file?", "File Open", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                        if (dlgRet != DialogResult.OK)
                        {
                            //MessageBox.Show("Yes");
                            return;
                        }
                    }

                    String fileName = lblPath.Text + "\\" + strSel;
                    //m_strWorkDirectory = lblPath.Text;
                    //m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(lblPath.Text);
                    txtFileName.Text = fileName;
                    if (m_C3d.DataFileOpen(fileName, null) == false)
                    {
                        MessageBox.Show("This is not a Dmt motion file.");
                    }
                    else
                    {
                        // 파일 데이터 저장
                        //TextConfigFileSave(m_strOrgDirectory + "\\ip.ini");
                        //OjwCreateToolTip(btnFileOpen, m_strWorkDirectory);

                        Modify(false);
                        //DisplayTime();
                        Grid_DisplayTime();

                        txtTableName.Text = m_C3d.GetMotionFile_Title();
                        txtComment.Text = m_C3d.GetMotionFile_Comment();
                        cmbStartPosition.SelectedIndex = m_C3d.GetMotionFile_StartPosition();

                        m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(fileName);
                        if (m_strWorkDirectory_Dmt == null) m_strWorkDirectory_Dmt = Application.StartupPath;
                    }

                    //m_strWorkDirectory = Directory.GetCurrentDirectory();
                }
                else if (
                            (CheckAudioExist(lblPath.Text + "\\" + strSel) == true) ||
                            (CheckMovieExist(lblPath.Text + "\\" + strSel) == true)
                )
                {
                    String fileName = lblPath.Text + "\\" + strSel;
                    //m_strWorkDirectoryMp3 = lblPath.Text;
                    lbMp3File.Text = Ojw.CFile.GetTitle(fileName);
                    //m_strFileNameMp3 = fileName;
                    m_strWorkDirectory_Dmt = Ojw.CFile.GetPath(lblPath.Text);
                    String strExe = Ojw.CFile.GetExe(fileName);

                    if ((strExe.ToUpper() != "MP3") && (strExe.ToUpper() != "WAV")) mpPlayer.Visible = true;
                    else mpPlayer.Visible = false;
                    InitMp3();
                    mpPlayer.URL = fileName;

                    // 파일 데이터 저장
                    //TextConfigFileSave(m_strOrgDirectory + "\\ip.ini");

                    //m_strWorkDirectoryMp3 = Directory.GetCurrentDirectory();
                }
                else if (
                        ((lblPath.Text + "\\" + strSel).ToLower().IndexOf(".ojw") > 0) ||
                        ((lblPath.Text + "\\" + strSel).ToLower().IndexOf(".dhf") > 0)
                        )
                {
                    if (m_C3d.FileOpen((lblPath.Text + "\\" + strSel)) == true) // 모델링 파일이 잘 로드 되었다면 
                    {
                        Ojw.CMessage.Write("3d Modeling File Opened");

                        float[] afData = new float[3];
                        m_C3d.GetPos_Display(out afData[0], out afData[1], out afData[2]);
                        m_C3d.GetAngle_Display(out afData[0], out afData[1], out afData[2]);

                        m_C3d.m_strDesignerFilePath = Ojw.CFile.GetPath((lblPath.Text + "\\" + strSel));
                        if (m_C3d.m_strDesignerFilePath == null) m_C3d.m_strDesignerFilePath = Application.StartupPath;
                    }
                }
            }
        }

        private void lstInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                String strSel;
                String strSize;
                //listviewOjwList.FocusedItem.Text;
                strSel = lstInfo.SelectedItems[0].SubItems[0].Text;
                strSize = lstInfo.SelectedItems[0].SubItems[1].Text;

                if (strSize.Equals("") == false) //파일일 경우
                {
                    //ShellExecute(0, "open", lblPath.Text + "\\" + strSel, null, null, SW_SHOWNORMAL);
                    if (CheckDmtExist(lblPath.Text + "\\" + strSel) == true)
                    {
                        SFileHeader_t SFileHeader;
                        if (CheckHeader(strSel, out SFileHeader) == true)
                        {
                            //Message(Ojw.CConvert.IntToStr(SFileHeader.nStartPosition));
                            //picStartPosition.Image = imageList1.Images[25 + SFileHeader.nStartPosition];
                            lbList_Title.Text = SFileHeader.strTitle;
                            lbList_Comment.Text = SFileHeader.strComment;
                            //lbList_FrameSize.Text = "M[" + Ojw.CConvert.IntToStr(SFileHeader.nFrameSize) + "], S[" + Ojw.CConvert.IntToStr(SFileHeader.nFrameSize_Sound) + "], E[" + Ojw.CConvert.IntToStr(SFileHeader.nFrameSize_Emoticon) + "]";
                            lbList_FrameSize.Text = Ojw.CConvert.IntToStr(SFileHeader.nFrameSize);
                            lbList_FrameSize_Sound.Text = Ojw.CConvert.IntToStr(SFileHeader.nFrameSize_Sound);
                            lbList_FrameSize_Emoticon.Text = Ojw.CConvert.IntToStr(SFileHeader.nFrameSize_Emoticon);
                            lblVersion.Text = SFileHeader.strFileVersion;
                        }
                    }
                    else if (
                                (CheckAudioExist(lblPath.Text + "\\" + strSel) == true) ||
                                (CheckMovieExist(lblPath.Text + "\\" + strSel) == true)
                    )
                    {

                    }
                    else if (
                        ((lblPath.Text + "\\" + strSel).ToLower().IndexOf(".ojw") > 0) ||
                        ((lblPath.Text + "\\" + strSel).ToLower().IndexOf(".dhf") > 0)
                        )
                    {
                        Ojw.C3d.COjwDesignerHeader CHeader = new Ojw.C3d.COjwDesignerHeader();
                        if (m_C3d.FileOpen_Without_Event(lblPath.Text + "\\" + strSel, out CHeader) == true) // 모델링 파일이 잘 로드 되었다면 
                        {
                            lbList_Title.Text = CHeader.strModelName;
                            lbList_Comment.Text = String.Format("[Design({0}))]{1}", ((lblPath.Text + "\\" + strSel).ToLower().IndexOf(".ojw") > 0) ? "ojw" : "dhf", CHeader.strComment);
                            //lbList_FrameSize.Text = "M[" + Ojw.CConvert.IntToStr(SFileHeader.nFrameSize) + "], S[" + Ojw.CConvert.IntToStr(SFileHeader.nFrameSize_Sound) + "], E[" + Ojw.CConvert.IntToStr(SFileHeader.nFrameSize_Emoticon) + "]";
                            lbList_FrameSize.Text = Ojw.CConvert.IntToStr(CHeader.nMotorCnt) + "Axis";
                            lbList_FrameSize_Sound.Text = String.Empty;// Ojw.CConvert.IntToStr(SFileHeader.nFrameSize_Sound);
                            lbList_FrameSize_Emoticon.Text = String.Empty;//Ojw.CConvert.IntToStr(SFileHeader.nFrameSize_Emoticon);
                            lblVersion.Text = CHeader.strVersion;
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void btnAppendFile_Click(object sender, EventArgs e)
        {
            try
            {
                String strSel;
                String strSize;

                strSel = lstInfo.SelectedItems[0].SubItems[0].Text;
                strSize = lstInfo.SelectedItems[0].SubItems[1].Text;
                String strFileName = lblPath.Text + "\\" + strSel;
                if (strSize.Equals("") == false) //폴더가 아닐 경우
                {
                    Ojw.SMotion_t SMotion = new Ojw.SMotion_t();
                    if (m_C3d.BinaryFileOpen(lblPath.Text + "\\" + strSel, out SMotion) == true)
                    {
                        
                    }
                    int nLine = m_C3d.m_CGridMotionEditor.m_nCurrntCell;

                    #region Insert
                    // 프레임 사이즈 만큼 새 줄을 삽입한다.
                    //m_C3d.m_CGridMotionEditor.Insert(nLine, SMotion.nFrameSize);

                    int nInsertCnt = SMotion.nFrameSize;
                    int nFirst = m_C3d.m_CGridMotionEditor.m_nCurrntCell;

                    // 먼저 빼고
                    m_C3d.m_CGridMotionEditor.Delete(m_C3d.m_CGridMotionEditor.GetHandle().RowCount - nInsertCnt - 1, nInsertCnt);

                    m_C3d.Flag_Insert(m_C3d.m_CGridMotionEditor.m_nCurrntCell, nInsertCnt);
                    m_C3d.m_CGridMotionEditor.Insert(m_C3d.m_CGridMotionEditor.m_nCurrntCell, nInsertCnt);
                    //Grid_Insert(nFirst, nInsertCnt);
                    //m_bKeyInsert = false;

                    for (int i = m_C3d.m_CGridMotionEditor.m_nCurrntCell; i < m_C3d.m_CGridMotionEditor.m_nCurrntCell + nInsertCnt; i++)
                    {
                        for (int j = 0; j < m_C3d.m_CHeader.nMotorCnt; j++)
                        {
                            m_C3d.m_pnFlag[i, j] = (int)(
                                0x10 | // Enable
                                ((m_C3d.m_CHeader.pSMotorInfo[j].nMotorControlType != 0) ? 0x08 : 0x00)// 위치제어가 아니라면 //0x08 //| // MotorType
                                //0x07 // Led
                                );
                        }
                    }
                    //m_C3d.CheckFlag(m_C3d.m_CGridMotionEditor.m_nCurrntCell);
                    // 색칠하기...
                    //m_C3d.GridMotionEditor_SetColorGrid(0, dgGrid.RowCount);
                    #endregion Insert

                    // 모션 추가
                    int nMax = nLine + SMotion.nFrameSize;
                    if (nMax > 999) nMax = 999;
                    for (int i = 0; i < SMotion.nFrameSize; i++)
                    {
                        int nPos = i + nLine;
                        
                        //En
                        #region Enable
                        m_C3d.m_CGridMotionEditor.SetEnable(nPos, SMotion.STable[i].bEn);
                        #endregion Enable
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        #region Motor
                        for (int nAxis = 0; nAxis < SMotion.nMotorCnt; nAxis++)
                        {

                            m_C3d.Grid_SetFlag_Led(nPos, nAxis, SMotion.STable[i].anLed[nAxis]);
                            m_C3d.Grid_SetFlag_Type(nPos, nAxis, SMotion.STable[i].abType[nAxis]);
                            m_C3d.Grid_SetFlag_En(nPos, nAxis, SMotion.STable[i].abEn[nAxis]);

                            m_C3d.GridMotionEditor_SetMotor(nPos, nAxis, m_C3d.CalcEvd2Angle(nAxis, (int)SMotion.STable[i].anMot[nAxis]));
                        }
                        #endregion Motor
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        #region Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                        // Speed  
                        m_C3d.m_CGridMotionEditor.SetTime(nPos, SMotion.STable[i].nTime);

                        // Delay  
                        m_C3d.m_CGridMotionEditor.SetDelay(nPos, SMotion.STable[i].nDelay);

                        // Group  
                        m_C3d.m_CGridMotionEditor.SetGroup(SMotion.STable[i].nGroup);

                        // Command  
                        m_C3d.m_CGridMotionEditor.SetCommand(nPos, SMotion.STable[i].nCmd);

                        // Data0  
                        m_C3d.m_CGridMotionEditor.SetData0(nPos, SMotion.STable[i].nData0);
                        // Data1  
                        m_C3d.m_CGridMotionEditor.SetData1(nPos, SMotion.STable[i].nData1);
                        // Data2  
                        m_C3d.m_CGridMotionEditor.SetData2(nPos, SMotion.STable[i].nData2);
                        // Data3  
                        m_C3d.m_CGridMotionEditor.SetData3(nPos, SMotion.STable[i].nData3);
                        // Data4  
                        m_C3d.m_CGridMotionEditor.SetData4(nPos, SMotion.STable[i].nData4);
                        // Data5  
                        m_C3d.m_CGridMotionEditor.SetData5(nPos, SMotion.STable[i].nData5);
                        #endregion Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        
                        // Caption
                        m_C3d.m_CGridMotionEditor.SetCaption(nPos, SMotion.STable[i].strCaption);

                        #region 추가한 Frame 위치 및 자세



                        #endregion 추가한 Frame 위치 및 자세
                    }
                }

                m_C3d.CheckFlag(m_C3d.m_CGridMotionEditor.m_nCurrntCell);
                // 색칠하기...
                m_C3d.GridMotionEditor_SetColorGrid(0, 999);


                //txtTableName.Text = m_C3d.GetMotionFile_Title();
                //txtComment.Text = m_C3d.GetMotionFile_Comment();
                //cmbStartPosition.SelectedIndex = m_C3d.GetMotionFile_StartPosition();
            }
            catch
            {

            }
        }

        private void btnChangeEnable_Click(object sender, EventArgs e)
        {
            //int nCol = m_C3d.m_CGridMotionEditor.m_nCurrntColumn;
            //int nLine = m_C3d.m_CGridMotionEditor.m_nCurrntCell;
            //m_C3d.Grid_SetFlag_En(nLine, nCol + 1, b
        }

        private void txtTableName_TextChanged(object sender, EventArgs e)
        {

        }
        //// CHerkuleX2 변수에 소켓 몰아주기... 라즈베리처럼
        //Ojw.CSocket m_CSock = new Ojw.CSocket();
        //private Ojw.CHerkulex2 m_CMotor2 = new Ojw.CHerkulex2();
        private void btnConnect_Click(object sender, EventArgs e)
        {
            bool bClose = false;
            if (m_C3d.m_CMotor2.IsOpen_Socket() == false)
            {
                m_C3d.m_CMotor2.Open_Socket(txtIp.Text, 5002);
            }
            else
            {
                bClose = true;
                m_C3d.m_CMotor2.Close_Socket();
            }

            if (m_C3d.m_CMotor2.IsOpen_Socket() == true)
            {
                btnConnect_Serial.Enabled = false;
                //m_lstReceive.Clear();
                Ojw.CMessage.Write("Connected {0}(TCP)");
                btnConnect.Text = "DisConnect(Tcp)";
                //m_CSock.RunThread(FReceive_TCP);
            }
            else
            {
                btnConnect_Serial.Enabled = true;
                btnConnect.Text = "Connect(Tcp)";
                if (bClose)
                    Ojw.CMessage.Write("Disconnected {0}(TCP)");
                else
                    Ojw.CMessage.Write_Error("Cannot connect{0}(TCP)");
            }
        }

        private void txtPort_Click(object sender, EventArgs e)
        {

        }

        private void txtIp_Click(object sender, EventArgs e)
        {
            txtIp.Enabled = true;
            txtPort.Enabled = false;
            
            //bool bClose = false;
            //if (m_CSock0.IsConnect() == false)
            //{
            //    m_CSock0.Connect(txtTcp_Ip0.Text, Ojw.CConvert.StrToInt(txtTcp_Port0.Text));
            //}
            //else
            //{
            //    bClose = true;
            //    m_CSock0.DisConnect();
            //}

            //if (m_CSock0.IsConnect() == true)
            //{
            //    m_lstReceive.Clear();
            //    Ojw.CMessage.Write("Connected {0}(TCP)");
            //    btnConnect_Tcp0.Text = "DisConnect";
            //    m_CSock0.RunThread(FReceive_TCP);
            //}
            //else
            //{
            //    btnConnect_Tcp0.Text = "Connect";
            //    if (bClose)
            //        Ojw.CMessage.Write("Disconnected {0}(TCP)");
            //    else
            //        Ojw.CMessage.Write_Error("Cannot connect{0}(TCP)");
            //}
            
        }

        private void chkRmt_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRmt.Checked == true)
            {
                byte[] pBuff = new byte[256];
                pBuff[0] = 0xf1;
                m_C3d.m_CMotor.SendPacket(pBuff, 1);
                Ojw.CTimer.Wait(100);
                pBuff[0] = 0xf0;
                m_C3d.m_CMotor.SendPacket(pBuff, 1);
            }
        }
#else
        private void btnDownload_Click(object sender, EventArgs e)
        {
            List<String> lstFiles = new List<string>();
            lstFiles.Clear();
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;

            string strPath = dlg.SelectedPath;
            
            //string strTest = String.Empty;

            DirectoryInfo dirInfo = new DirectoryInfo(strPath);
            if (dirInfo.Exists == true)
            {
                lstFiles.Clear();
                foreach (FileInfo fileInfo in dirInfo.GetFiles("*.dmt"))
                {
                    lstFiles.Add(fileInfo.ToString());
                    //strTest += fileInfo.ToString() + "\r\n";
                }
            }
            //MessageBox.Show(strPath + "\r\n" + strTest);
            //return;


            bool bTmrDraw = tmrDraw.Enabled;
            bool bTmrMp3 = tmrMp3.Enabled;
            bool bTmrRun = tmrRun.Enabled;
            tmrDraw.Enabled = false;
            tmrMp3.Enabled = false;
            tmrRun.Enabled = false;

            Ojw.CTimer.Wait(100);

            #region MPSU Motion DownLoad
            int nID = 253;
            m_C3d.m_CMotor.Mpsu_Reboot(nID);
            bool bRet = m_C3d.m_CMotor.WaitReceive_Mpsu(100);
            if (bRet == true)
            {
                //Ojw.CTimer.Wait(1000); // 재부팅 시간 1초정도를 대기한다.
                
                m_C3d.m_CMotor.ResetCounter_Mpsu(); // 명령을 보내지 않았지만 부팅시 저절로 들어오는 패킷 명령을 받는 이벤트를 확인하기 위해...
                bRet = m_C3d.m_CMotor.WaitReceive_Mpsu(3000);
                if (bRet == true)
                {
#if false
                    // 재부팅 완료

                    // 잠시대기
                    Ojw.CTimer.Wait(100);
                    // 이벤트 체크를 위한 카운터 클리어
                    m_C3d.m_CMotor.ResetCounter(); 
                    // 부트로드 진입 명령
                    m_C3d.m_CMotor.Mpsu_EnterBootloader();
                    bRet = m_C3d.m_CMotor.WaitReceive_1Packet(3000);
                    if (bRet == true)
                    {
                        if (m_C3d.m_CMotor.GetLastPacket() == (byte)('S'))
                        {
                            //MessageBox.Show("부트로더 진입 ok");

                            // 포트를 따로 연다.
                            if (m_C3d.m_CMotor.IsConnect() == true)
                            {
                                Ojw.CSerial CSerial = new Ojw.CSerial();
                                m_C3d.m_CMotor.DisConnect();
                            }
                        }
                        else
                        {
                            //MessageBox.Show("부트로더 진입 리턴데이터 이상");
                        }
                    }
                    else
                    {
                        //MessageBox.Show("부트로더 진입 fail");
                    }
#else
                    // 포트를 따로 연다.
                    if (m_C3d.m_CMotor.IsConnect() == true)
                    {
                        int i;
                        Ojw.CTimer CTmr = new Ojw.CTimer();
                        Ojw.CSerial CSerial = new Ojw.CSerial();
                        m_C3d.m_CMotor.DisConnect();
                        CSerial.Connect(Ojw.CConvert.StrToInt(txtPort.Text), Ojw.CConvert.StrToInt(txtBaudrate.Text));
                        if (CSerial.IsConnect() == true)
                        {
                            bool bEntered = false;
                            if (Command_And_Wait(CSerial, (byte)('S')) == true)
                            {
                                //MessageBox.Show("We have entered bootloader");
                                bEntered = true;

                                // Delete Motion
                                Command_And_Wait_DeleteMotions(CSerial);
                                Command_And_Wait_DownloadMotions(CSerial, strPath, lstFiles);
                                Command_And_Wait_GetMotionList(CSerial);
                                String strList = String.Empty;
                                foreach (string strItem in m_lstMotion)
                                {
                                    strList += strItem + "\r\n";
                                }
                                MessageBox.Show(strList);
                            }
                            else
                            {
                                //MessageBox.Show("We could not enter bootloader");
                            }
                                    
                             

                            if (bEntered == true)
                            {
                                // Quit Bootloader
                                if (Command_And_Wait(CSerial, (byte)('Q')) == true)
                                {
                                }
                            }
                        }
                        
                        // return
                        CSerial.DisConnect();
                        m_C3d.m_CMotor.Connect(Ojw.CConvert.StrToInt(txtPort.Text), Ojw.CConvert.StrToInt(txtBaudrate.Text));
                        MessageBox.Show("Downloaded All motion files");
                    }
#endif
                }
                else MessageBox.Show("reboot fail");
                

                //MessageBox.Show("부트로더 진입 ok");
            }
            else //MessageBox.Show("부트로더 진입 fail");
            {
                MessageBox.Show("Reset fail");
            }
#if false
            if ( EnterBootloader() )
		    {
			    DeleteMotions();
			    DownloadMotions();
			    GetMotionList();
			    QuitBootloader();
		    }
#endif
            #endregion MPSU Motion DownLoad
            
            tmrDraw.Enabled = bTmrDraw;
            tmrMp3.Enabled = bTmrMp3;
            tmrRun.Enabled = bTmrRun;
        }

        #region Made by Chulhee Yun - 이 코드들은 전부 특정 위치에서 포트가 열릴 경우만 실행되게 되어 있으므로 Open 에러처리가 많이 필요하진 않다.
        private int [] m_anMotionAddress = new int[128];
        private int [] m_anMotionSize = new int[128];
        private const int _PAGESIZE = 128;
        private int m_nBufIdx = 0;
        private int m_nCurrentAddress = 0;
        private bool Command_And_Wait_FlashWriteMtnData(Ojw.CSerial CSerial, byte byteData, int nAddress, ref byte [] pbyteBuffer)
        {
            bool bRet = false;
            
            if (nAddress > 0)
            {
                if (Command_And_Wait_FlashAddWrite(CSerial, nAddress) == true)
                {
                    m_nCurrentAddress = nAddress;
                    m_nBufIdx = (nAddress % _PAGESIZE) * 2;
                    if (Command_And_Wait_LoadPageToBuf(CSerial, ref pbyteBuffer) == true)
                    {
                    }
                }
            }

            pbyteBuffer[m_nBufIdx] = byteData;
            if (m_nBufIdx % 2 == 1)
            {
                m_nCurrentAddress++;
            }

            if (m_nBufIdx == (2 * _PAGESIZE - 1))
            {
                Command_And_Wait_WritePageFromBuf(CSerial, pbyteBuffer);
                Command_And_Wait_FlashAddWrite(CSerial, m_nCurrentAddress);
                Command_And_Wait_LoadPageToBuf(CSerial, ref pbyteBuffer);
                m_nBufIdx = 0;
            }
            else
            {
                m_nBufIdx++;
            }
            return bRet;
        }
        private bool Command_And_Wait_FlashAddRead(Ojw.CSerial CSerial, out int nAddress)
        {
            bool bRet = false;
            nAddress = 0;
            if (Command_And_Wait(CSerial, (byte)('a')) == true)
            {
                Ojw.CTimer CTmr = new Ojw.CTimer();
                CTmr.Set(); while (CTmr.Get() < 1000) { if (CSerial.GetBuffer_Length() >= 2) break; else Application.DoEvents(); }
                if (CSerial.GetBuffer_Length() >= 2)
                {
                    nAddress = (int)((int)CSerial.GetByte() | ((int)CSerial.GetByte() << 8));
                }
            }
            return bRet;
        }
        private bool Command_And_Wait_FlashReadWord(Ojw.CSerial CSerial, out int nValue)
        {
            bool bRet = false;
            nValue = 0;
            if (Command_And_Wait(CSerial, (byte)('R')) == true)
            {
                Ojw.CTimer CTmr = new Ojw.CTimer();
                CTmr.Set(); while (CTmr.Get() < 1000) { if (CSerial.GetBuffer_Length() >= 2) break; else Application.DoEvents(); }
                if (CSerial.GetBuffer_Length() >= 2)
                {
                    nValue = (int)((int)CSerial.GetByte() | ((int)CSerial.GetByte() << 8));
                }
            }
            return bRet;
        }
        private bool Command_And_Wait_WritePage(Ojw.CSerial CSerial)
        {
            bool bRet = false;
            if (Command_And_Wait(CSerial, (byte)('W')) == true) bRet = true;
            return bRet;
        }
        private bool Command_And_Wait_ErasePage(Ojw.CSerial CSerial)
        {
            bool bRet = false;
            if (Command_And_Wait(CSerial, (byte)('E')) == true) bRet = true;
            return bRet;
        }
        private bool Command_And_Wait_WritePageFromBuf(Ojw.CSerial CSerial, byte[] pbyteData)
        {
            bool bRet = true;
            int nAddress = 0;
            Command_And_Wait_FlashAddRead(CSerial, out nAddress);
            Command_And_Wait_FlashAddWrite(CSerial, (nAddress / _PAGESIZE) * _PAGESIZE);
            for (int i = 0; i < _PAGESIZE; i++)
            {
                if (Command_And_Wait(CSerial, (byte)('T')) == true)
                {
                    Command_And_Wait(CSerial, pbyteData[2 * i]);
                    Command_And_Wait(CSerial, pbyteData[2 * i + 1]);
#if false
                    //if (Command_And_Wait(CSerial, pbyteData[2 * i]) == true)
                    //{
                    //}
                    //else
                    //{
                    //    bRet = false;
                    //    break;
                    //}
                    //if (Command_And_Wait(CSerial, pbyteData[2 * i + 1]) == true)
                    //{
                    //}
                    //else
                    //{
                    //    bRet = false;
                    //    break;
                    //}
#endif
                }
                //else
                //{
                //    bRet = false;
                //    break;
                //}
            }
            Command_And_Wait_FlashAddWrite(CSerial, nAddress);
            Command_And_Wait_ErasePage(CSerial);
            Command_And_Wait_WritePage(CSerial);            
            return bRet;
        }
        private bool Command_And_Wait_LoadPageToBuf(Ojw.CSerial CSerial, ref byte [] pbyteData)
        {
            bool bRet = true;
            int nAddress = 0;
            if (Command_And_Wait_FlashAddRead(CSerial, out nAddress) == true)
            {
                // 하위주소번지를 던짐
                byte byteData = (byte)(nAddress & 0x00FF);
                if (Command_And_Wait_FlashAddWrite(CSerial, ((nAddress / _PAGESIZE) * _PAGESIZE)) == true)
                {
                    for (int i = 0; i < _PAGESIZE; i++)
                    {
                        if (Command_And_Wait(CSerial, (byte)('R')) == true)
                        {
                            Ojw.CTimer CTmr = new Ojw.CTimer();
                            CTmr.Set(); while (CTmr.Get() < 1000) { if (CSerial.GetBuffer_Length() >= 2) break; else Application.DoEvents(); }
                            if (CSerial.GetBuffer_Length() >= 2)
                            {
                                pbyteData[2 * i] = CSerial.GetByte();
                                pbyteData[2 * i + 1] = CSerial.GetByte();
                            }
                            else
                            {
                                bRet = false;
                                break;
                            }
                        }
                        else
                        {
                            bRet = false;
                            break;
                        }
                    }
                }
                else bRet = false;
            }
            else bRet = false;
            return bRet;
        }
        private bool Command_And_Wait_DownloadMotions(Ojw.CSerial CSerial, String strPath, List<String> strFiles)
        {
            bool bRet = false;

            for (int i = 0; i < strFiles.Count; i++)
            {
                Command_And_Wait_DownLoadMotion(CSerial, String.Format("{0}\\{1}", strPath, strFiles[i]), i);
                lbMotion_Status.Text = String.Format("File Download -> {0} / {1}", i + 1, strFiles.Count);
            }

            return bRet;
        }
        private bool Command_And_Wait_DownLoadMotion(Ojw.CSerial CSerial, String strFileName, int nIndex)
        {
            bool bRet = false;
            int nMax_Address = 0;
            int nMax_Size = 0;
            //bool bMotionDistorted = false;
            //주소상으로 가장 뒤에 있는 모션의 주소와 사이즈를 알아냄
            for (int i = 0; i < 128; i++)
            {
                if (nMax_Address < m_anMotionAddress[i])
                {
                    nMax_Address = m_anMotionAddress[i];
                    nMax_Size = m_anMotionSize[i];
                }
            }
            if (nMax_Address <= 0x8100) nMax_Address = 0x8100;

            //가장 뒤에 있는 모션의 뒤에 새로운 모션 파일을 씀
            bRet = FlashWriteDmt(CSerial, strFileName, nIndex, nMax_Address + nMax_Size);
            return bRet;
#if false
            //이 후의 내용은 제대로 쓰여 졌는지 확인하고 Display하는 루틴임
            m_anMotionAddress[nIndex] = 0;
            m_anMotionSize[nIndex] = 0;

            Command_And_Wait_FlashAddWrite(CSerial, 0x8000 + 2 * nIndex);
            int nAddress = Command_And_Wait_FlashReadWord(CSerial);
            int nSize = Command_And_Wait_FlashReadWord(CSerial);
            if (nAddress != 0xFFFF && nSize != 0xFFFF)
            {
                //Check if the motion is correct
                bMotionDistorted = false;
                FlashAddWrite(nAddress + nSize - 1);
                nTmp = Command_And_Wait_FlashReadWord(CSerial);
                if ((char)(nTmp & 0xFF) != 'M' || (char)((nTmp & 0xFF00) >> 8) != 'E')
                {
                    if ((char)(nTmp & 0xFF) == 'E')
                    {
                        Command_And_Wait_FlashAddWrite(CSerial, nAddress + nSize - 2);
                        nTmp = Command_And_Wait_FlashReadWord(CSerial);
                        if ((char)((nTmp & 0xFF00) >> 8) != 'M')
                        {
                            bMotionDistorted = true;
                        }
                    }
                    else
                    {
                        bMotionDistorted = true;
                    }
                }
                if (bMotionDistorted)
                {
                    // Motion was distorted. Deleting Motion i
                    Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0x8000 + 2 * nIndex, pageBuf);
                    Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0, pageBuf);
                    Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0, pageBuf);
                    Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0, pageBuf);
                    Command_And_Wait_WritePageFromBuf(CSerial, pageBuf);
                    return false;
                }

                m_anMotionAddress[nIndex] = nAddress;
                m_anMotionSize[nIndex] = nSize;
            }
#endif
            return bRet;
        }
        private const int _BOOT_START = 0xF800;
        //DMT File formats////////////////////////////////////////
        private const int _HEADER_LEN	                = 41;
        private const int _HEADER_TABLE_NAME_IDX		= 6;
        private const int _HEADER_START_POS_IDX			= 27;
        private const int _HEADER_MOTION_FRAME_SIZE_IDX	= 28;
        private const int _HEADER_ROBOT_MODEL_TYPE_IDX	= 38;
        private const int _HEADER_MOTOR_CNT_IDX			= 40;
        //////////////////////////////////////////////////////////
        private bool FlashWriteDmt(Ojw.CSerial CSerial, string strFilename, int nMotionNo, int nAddress)
        {
            bool bRet = true;
            Ojw.SMotion_t SMotion = new Ojw.SMotion_t();
            int i, j;
            if (m_C3d.BinaryFileOpen(strFilename, out SMotion) == true)
            {
                
            }
            else return false;

            int nDataLen = 0;

	        byte [] pbytePageBuf = new byte[256];
            	        
	        /////////////수정 20120116/////////////
	        int nMotionFrameSize = 0;//SMotion.nFrameSize;
            
            // 실제 Enable 된 프레임만 카운트 한다.
            foreach(Ojw.SMotionTable_t STable in SMotion.STable) { if (STable.bEn == true) nMotionFrameSize++; }


	        int nMotorCnt = SMotion.nMotorCnt;

	        if(nAddress+(33+(2*nMotorCnt+9)*nMotionFrameSize+2+1)/2 > _BOOT_START){
		        return false;
	        }

            String strMotion = "DMT1.0"; // 나중에 가변으로 ? 
	        for ( i = 0; i < strMotion.Length; i++ ) Command_And_Wait_FlashWriteMtnData(CSerial, (byte)strMotion[i], 0, ref pbytePageBuf );

	        // Table Name
            StringBuilder sb = new StringBuilder(SMotion.strTableName);
	        for( i = 0; i < 20; i++ )
		        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)sb[i], 0, ref pbytePageBuf );
            Command_And_Wait_FlashWriteMtnData(CSerial, (byte)0, 0, ref pbytePageBuf );
            
	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.nStartPosition & 0xff), 0, ref pbytePageBuf );
	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.nFrameSize & 0xff), 0, ref pbytePageBuf );
	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)((SMotion.nFrameSize >> 8) & 0xff), 0, ref pbytePageBuf );
	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.nRobotModelNum & 0xff), 0, ref pbytePageBuf );
	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)((SMotion.nRobotModelNum >> 8) & 0xff), 0, ref pbytePageBuf );
	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.nMotorCnt & 0xff), 0, ref pbytePageBuf );

	        nDataLen += 33;
                            
	        for( i = 0; i < nMotionFrameSize; i++ )
	        {
                if (SMotion.STable[i].bEn == true)
                {
		            //Motor Data
		            for ( j = 0; j < nMotorCnt; j++ )
		            {
		                Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.STable[i].anMot[j] & 0xff), 0, ref pbytePageBuf );
		                Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.STable[i].anMot[j] >> 8 & 0xff), 0, ref pbytePageBuf );
		            }

		            //Playtime
		            Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.STable[i].nTime & 0xff), 0, ref pbytePageBuf );
		            Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.STable[i].nTime >> 8 & 0xff), 0, ref pbytePageBuf );

		            //Delay
		            Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.STable[i].nDelay & 0xff), 0, ref pbytePageBuf );
		            Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.STable[i].nDelay >> 8 & 0xff), 0, ref pbytePageBuf );

		            //Command
		            Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.STable[i].nCmd & 0xff), 0, ref pbytePageBuf );

		            //Data0
		            Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.STable[i].nData0 & 0xff), 0, ref pbytePageBuf );
		            Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.STable[i].nData0 >> 8 & 0xff), 0, ref pbytePageBuf );
		            //Data1
		            Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.STable[i].nData1 & 0xff), 0, ref pbytePageBuf );
		            Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(SMotion.STable[i].nData1 >> 8 & 0xff), 0, ref pbytePageBuf );

		            nDataLen += nMotorCnt * 2 + 9;
                }
	        }

	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)('M'), 0, ref pbytePageBuf );
	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)('E'), 0, ref pbytePageBuf );
	        nDataLen += 2;
            
	        Command_And_Wait_WritePageFromBuf(CSerial, pbytePageBuf );
	        nDataLen = ( nDataLen + 1 ) / 2; //converting from byte address to word address

	        int nPtrAddress = 0x8000 + 2 * nMotionNo;
	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)( nAddress & 0x00FF ), nPtrAddress, ref pbytePageBuf );
	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(( nAddress & 0xFF00 ) >> 8), 0, ref pbytePageBuf );
	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)( nDataLen & 0x00FF ), 0, ref pbytePageBuf );
	        Command_And_Wait_FlashWriteMtnData(CSerial, (byte)(( nDataLen & 0xFF00 ) >> 8), 0, ref pbytePageBuf );
	        Command_And_Wait_WritePageFromBuf(CSerial, pbytePageBuf );                
            return bRet;
        }
        private const int _FLASH_MTN_HEADER_LEN						= 17; //33byte -> 17word
        private const int _FLASH_MTN_HEADER_TABLE_NAME_IDX			= 3;  //6byte -> 3word
        private const int _FLASH_MTN_HEADER_START_POS_IDX			= 13; // 27byte -> 13word
        private const int _FLASH_MTN_HEADER_MOTION_FRAME_SIZE_IDX	= 14; // 28byte -> 14word
        private const int _FLASH_MTN_HEADER_ROBOT_MODEL_TYPE_IDX	= 15; // 30byte -> 15word
        private const int _FLASH_MTN_HEADER_MOTOR_CNT_IDX			= 16; // 32byte -> 16word
        private List<String> m_lstMotion = new List<string>();
        private void Command_And_Wait_GetMotionList(Ojw.CSerial CSerial)
        {
            m_lstMotion.Clear();
            int nAddress;
            int nSize;
            
		    int nTmp;
		    bool bMotionDistorted;
            byte [] pbytePageBuf = new byte[256];

		    //모션에 대한 pointer가 저장된 0x8000 ~ 0x80FF의 공간을 스캔하며 모션 주소와 크기를 읽음
		    for ( int i = 0; i < 128; i++ )
		    {
			    m_anMotionAddress[i] = 0;
			    m_anMotionSize[i] = 0;

			    Command_And_Wait_FlashAddWrite(CSerial, 0x8000 + 2 * i );
			    Command_And_Wait_FlashReadWord(CSerial, out nAddress);
			    Command_And_Wait_FlashReadWord(CSerial, out nSize);

			    //모션이 저장되어 있을 경우
			    if ( nAddress != 0xFFFF && nSize != 0xFFFF )
			    {
				    //Check if the motion is correct
				    bMotionDistorted = false;
				    Command_And_Wait_FlashAddWrite(CSerial, nAddress + nSize - 1 );
				    Command_And_Wait_FlashReadWord(CSerial, out nTmp);
				    if ( (char)(nTmp & 0xFF) != 'M' || (char)((nTmp & 0xFF00)>>8) != 'E' )
				    {
					    if ( (char)(nTmp & 0xFF) == 'E' )
					    {
						    Command_And_Wait_FlashAddWrite(CSerial, nAddress + nSize - 2 );
						    Command_And_Wait_FlashReadWord(CSerial, out nTmp);
						    if ( (char)((nTmp & 0xFF00)>>8) != 'M' )
						    {
							    bMotionDistorted = true;
						    }
					    }
					    else
					    {
						    bMotionDistorted = true;
					    }
				    }
				    if ( bMotionDistorted )
				    {
					    // Motion was distorted. Deleting Motion i
                        Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0x8000 + 2 * i, ref pbytePageBuf);
                        Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0, ref pbytePageBuf);
                        Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0, ref pbytePageBuf);
                        Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0, ref pbytePageBuf);
                        Command_And_Wait_WritePageFromBuf(CSerial, pbytePageBuf);
					    continue;
				    }
				    m_anMotionAddress[i] = nAddress;
				    m_anMotionSize[i] = nSize;

				    //Read Table name and display
				    Command_And_Wait_FlashAddWrite(CSerial, nAddress + _FLASH_MTN_HEADER_TABLE_NAME_IDX );
				    string strTmp = String.Empty;
				    for ( int j = 0; j < 10; j++ )
				    {
					    Command_And_Wait_FlashReadWord(CSerial, out nTmp);
					    if ( (char)(nTmp & 0xFF) != '\0' )
					    {
						    strTmp += (char)(nTmp & 0xFF);
					    }
					    if ( (char)( ( nTmp & 0xFF00 ) >> 8 ) != '\0' )
					    {
						    strTmp += (char)( (nTmp & 0xFF00) >> 8 );
					    }
				    }
				    Command_And_Wait_FlashReadWord(CSerial, out nTmp);
				    if ( (char)(nTmp & 0xFF)!='\0' )
				    {
					    strTmp += (char)(nTmp & 0xFF);
				    }
				    int len = strTmp.Length;

				    string str = String.Format("{0}. ", i);
				    str += strTmp;
                    m_lstMotion.Add(str);

				    //Read frame count and display
				    Command_And_Wait_FlashAddWrite(CSerial, nAddress + _FLASH_MTN_HEADER_MOTION_FRAME_SIZE_IDX );
				    Command_And_Wait_FlashReadWord(CSerial, out nTmp);

				    //Read motor count and display
				    Command_And_Wait_FlashAddWrite(CSerial, nAddress + _FLASH_MTN_HEADER_MOTOR_CNT_IDX );
				    Command_And_Wait_FlashReadWord(CSerial, out nTmp);
                    nTmp = (nTmp & 0xFF);
			    }
		    }
        }
        private bool Command_And_Wait_DeleteMotions(Ojw.CSerial CSerial)
        {
            bool bRet = true;
            for (int i = 0; i < 128; i++)
            {
                m_anMotionAddress[i] = 0;
                m_anMotionSize[i] = 0;

                #region Delete Motion
                // Flash Addwrite
                int nAddress = 0x8000 + i * 2;
                if (Command_And_Wait_FlashAddWrite(CSerial, nAddress) == true)
                {
                    // FlashReadWord
                    int nMotionAddress = 0;
                    int nMotionSize = 0;

                    #region Read MotionAddress
                    if (Command_And_Wait(CSerial, (byte)('R')) == true)
                    {
                        // 
                        Ojw.CTimer CTmr = new Ojw.CTimer();
                        CTmr.Set(); while (CTmr.Get() < 1000) { if (CSerial.GetBuffer_Length() >= 2) break; else Application.DoEvents(); }
                        if (CSerial.GetBuffer_Length() >= 2)
                        {
                            nMotionAddress = (int)((int)CSerial.GetByte() | ((int)CSerial.GetByte() << 8));
                            #region Read MotionSize
                            if (Command_And_Wait(CSerial, (byte)('R')) == true)
                            {
                                // 
                                CTmr.Set(); while (CTmr.Get() < 1000) { if (CSerial.GetBuffer_Length() >= 2) break; else Application.DoEvents(); }
                                if (CSerial.GetBuffer_Length() >= 2)
                                {
                                    nMotionSize = (int)((int)CSerial.GetByte() | ((int)CSerial.GetByte() << 8));
                                }
                                else bRet = false;
                            }
                            #endregion Read MotionSize
                        }
                        else bRet = false;
                    }
                    else bRet = false;
                    #endregion Read MotionAddress

                    // 모션이 저장되어 있는 경우
                    if ((nMotionAddress > 0) && (nMotionSize > 0))
                    {
                        byte[] pbytePageBuf = new byte[256];
                        Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0x8000 + 2 * i, ref pbytePageBuf);
                        Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0, ref pbytePageBuf);
                        Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0, ref pbytePageBuf);
                        Command_And_Wait_FlashWriteMtnData(CSerial, 0xFF, 0, ref pbytePageBuf);
                        Command_And_Wait_WritePageFromBuf(CSerial, pbytePageBuf);

                        m_anMotionAddress[i] = 0;
                        m_anMotionSize[i] = 0;
                    }
                    //else bRet = false;
                }
                else bRet = false;
                #endregion Delete Motion
            }
            return bRet;
        }
        private bool Command_And_Wait_FlashAddWrite(Ojw.CSerial CSerial, int nAddress)
        {
            bool bRet = false;
            if (Command_And_Wait(CSerial, (byte)('A')) == true)
            {
                // 하위주소번지를 던짐
                byte byteData = (byte)(nAddress & 0x00FF);
                if (Command_And_Wait(CSerial, byteData) == true)
                {
                    // 상위주소번지를 던짐
                    byteData = (byte)((nAddress >> 8) & 0x00FF);
                    if (Command_And_Wait(CSerial, byteData) == true)
                    {
                        bRet = true;
                    }
                }
            }
            return bRet;
        }
        private bool Command_And_Wait(Ojw.CSerial CSerial, byte byteData)
        {
            bool bRet = false;
            if (SendPacket_And_Wait(CSerial, byteData) == true)
            {
                if (CSerial.GetByte() == byteData) bRet = true;
            }
            return bRet;
        }
        private bool SendPacket_And_Wait(Ojw.CSerial CSerial, byte byteData)
        {
            bool bRet = false;
            Ojw.CTimer CTmr = new Ojw.CTimer();
            byte[] pbyteData = new byte[1];
            pbyteData[0] = byteData;
            CSerial.SendPacket(pbyteData);
            CTmr.Set();
            while (CTmr.Get() < 1000)
            {
                if (CSerial.GetBuffer_Length() > 0)
                {
                    bRet = true;
                    break;
                }
                else Application.DoEvents();
            }
            return bRet;
        }
        private bool SendPacket_And_Wait(Ojw.CSerial CSerial, byte [] pbyteData)
        {
            bool bRet = false;
            Ojw.CTimer CTmr = new Ojw.CTimer();
            CSerial.SendPacket(pbyteData); 
            CTmr.Set(); 
            while (CTmr.Get() < 1000) 
            {
                if (CSerial.GetBuffer_Length() > 0)
                {
                    bRet = true;
                    break;
                }
                else Application.DoEvents();
            }
            return bRet;
            //if (bRet == true)
            //{
            //    CSerial.GetBytes();
            //}
            //return null;
        }
        #endregion Made by Chulhee Yun
#endif
    }
    
    public class SystemImgList
    {
        ////// treeview 탐색기 -- ///////
        //[DllImport("Shell32.dll")]
        //private static extern int SHGetFileInfo(
        //                                            string pszPath, uint dwFileAttributes,
        //                                            ref SHFILEINFO psfi, uint cbfileInfo,
        //                                            SHGFI uFlags
        //    );
        [DllImport("Shell32.dll")]
        private static extern IntPtr SHGetFileInfo(
                                                    string pszPath, uint dwFileAttributes,
                                                    ref SHFILEINFO psfi, uint cbfileInfo,
                                                    uint uFlags
            );
        [DllImport("User32.dll")]
        public static extern int DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public SHFILEINFO(int i)
            {
                hIcon = IntPtr.Zero;
                iIcon = IntPtr.Zero; //0;
                dwAttributes = 0;
                szDisplayName = "";
                szTypeName = "";
            }
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            // WinApi 의 char 형 배열과 String 의 차이를 통합시키기 위해...
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
        private enum SHGFI
        {
            SmallIcon = 0x00000001,
            OpenIcon = 0x00000002,
            LargeIcon = 0x00000000,
            Icon = 0x00000100,
            DisplayName = 0x00000200,
            TypeName = 0x00000400,
            SysIconIndex = 0x00004000,
            LinkOverlay = 0x00008000,
            UseFileAttributes = 0x00000010,
        }
        // 파일 / 폴더에 맞는 아이콘을 얻는다.
        // 인자 1 : 파일 / 폴더 전체 경로
        // 인자 2 : 작은 / 큰 이미지
        // 인자 3 : 보통 / 선택 이미지
        public Icon GetIcon(String strPath, bool bSmall, bool bOpen)
        {
            try
            {
                SHFILEINFO info = new SHFILEINFO(0);

                // 구조체 크기 얻기
                int cbFileInfo = Marshal.SizeOf(info);
                uint flags;

                // 조건에 맞게 플래그 설정
                if (bSmall)
                    flags = (uint)SHGFI.Icon | (uint)SHGFI.SmallIcon;
                else
                    flags = (uint)SHGFI.Icon | (uint)SHGFI.LargeIcon;

                if (bOpen) flags = flags | (uint)SHGFI.OpenIcon;

                //// 아이콘 얻기
                //SHGetFileInfo(strPath, 256, ref info, (uint)cbFileInfo, flags);

                //// 형식을 변환해 아이콘 반환

                //return Icon.FromHandle(info.hIcon);

                SHFILEINFO shinfo = new SHFILEINFO();
                //IntPtr hImgSmall = SHGetFileInfo(strPath, 256, ref shinfo, (uint)cbFileInfo, flags);
                IntPtr hImgSmall = SHGetFileInfo(strPath, 0, ref shinfo, (uint)cbFileInfo, flags);

                Icon icon = (Icon)System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone();
                DestroyIcon(shinfo.hIcon);
                return icon;
            }
            catch(Exception ex)
            {
                Ojw.CMessage.Write_Error(ex.ToString());
                return null;
            }
        }
        ////// -- treeview 탐색기 ///////
    }
}
