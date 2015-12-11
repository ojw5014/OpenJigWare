using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Net;

namespace OpenJigWare
{
    partial class Ojw
    {
        // if you make your class, just write in here
        public class CGenibo
        {
            public CGenibo()
            {
                for (int i = 0; i < 6; i++) OjwSock[i] = new CSocket();
            }
            private const int C_TCP_FILENAME_SIZE = 16;

            private int _PORT = 6100;
            private int _ROBOT = 0;
            private int _SOUND = 1;
            private int _VISION = 2;
            private int _MOTION = 3;
            private int _SENSOR = 4;
            private int _EMOTICON = 5;

            private const int _CNT_PORT = 6;

            // Board ID
            private const int LEFT_FRONT_ID = 19;
            private const int RIGHT_FRONT_ID = 20;
            private const int LEFT_REAR_ID = 21;
            private const int RIGHT_REAR_ID = 22;
            // Motor ID
            private const int LEFT_FRONT_WING_ID = 0;
            private const int LEFT_FRONT_DOWN_ID = 1;
            private const int LEFT_FRONT_UP_ID = 2;
            private const int RIGHT_FRONT_WING_ID = 3;
            private const int RIGHT_FRONT_DOWN_ID = 4;
            private const int RIGHT_FRONT_UP_ID = 5;
            private const int LEFT_REAR_WING_ID = 6;
            private const int LEFT_REAR_DOWN_ID = 7;
            private const int LEFT_REAR_UP_ID = 8;
            private const int RIGHT_REAR_WING_ID = 9;
            private const int RIGHT_REAR_DOWN_ID = 10;
            private const int RIGHT_REAR_UP_ID = 11;
            //private const int MOTOR_TEMP_ID		= 12;
            //private const int HEAD_YAW_ID       = 13;
            private const int HEAD_MOUTH_ID = 14;
            private const int HEAD_PAN_ID = 15;
            private const int HEAD_TILT_ID = 16;
            private const int HIPS_TAIL_ID = 17;
            //private const int HEAD_TILTUP_ID = 13; // 일단은 Yaw 대신...

            private const int HEAD_TILTUP_ID = 18;
            private const int HEAD_LEFT_EAR_ID = 19;
            private const int HEAD_RIGHT_EAR_ID = 20;

            // 소켓
            private CSocket[] OjwSock = new CSocket[_CNT_PORT];
            private bool [] m_achkEn = new bool[_CNT_PORT];
            
            private Thread[] Reader = new Thread[6];             // 읽기 쓰레드

            public void Auth(string strID, string strPassword)
            {
                Ojw.CMessage.Write("Authorization Check");

                int i;
                int nSize1 = 21;
                int nSize2 = 21;// 11;
                int nSize = nSize1 + nSize2;
                byte[] byteData = new byte[nSize];
                for (i = 0; i < nSize; i++)
                {
                    byteData[i] = 0;
                }
                byte[] byteId = Encoding.Default.GetBytes(strID);// Encoding.ASCII.GetBytes(txtId.Text);
                for (i = 0; i < byteId.Length; i++)
                {
                    if (i > (nSize1 - 1)) break;
                    byteData[i] = byteId[i];
                }
                byte[] bytePasswd = Encoding.Default.GetBytes(strPassword);// Encoding.ASCII.GetBytes(mtxtPasswd.Text);
                for (i = 0; i < bytePasswd.Length; i++)
                {
                    if (i > (nSize2 - 1)) break;
                    byteData[i + nSize1] = bytePasswd[i];
                }

                for (i = _ROBOT; i <= _EMOTICON; i++)
                    SendData(i, 0x71, nSize, byteData);
                //SendData(_EMOTICON, 0x71, nSize, byteData);
            }
            public void SendData(int nType, int nCommand, int nSize, byte[] byteArrayData)
            {
                int i, nNum;

                // 프레임 수
                //int nSize = nSize;
                int nPacketSize = nSize + 8;
                byte[] byteData = new byte[nPacketSize];

                nNum = 0;
                byteData[nNum++] = 0xff;
                // ID
                byteData[nNum++] = (Byte)((0x90 + nType) & 0xff);
                // Cmd
                byteData[nNum++] = (Byte)(nCommand & 0xff);

                // Data Size
                byteData[nNum++] = (byte)((nSize >> 24) & 0xff);
                byteData[nNum++] = (byte)((nSize >> 16) & 0xff);
                byteData[nNum++] = (byte)((nSize >> 8) & 0xff);
                byteData[nNum++] = (byte)(nSize & 0xff);

                //byteArrayData.CopyTo(byteData, nNum);
                //nNum += nSize;
                for (i = 0; i < nSize; i++)
                {
                    byteData[nNum++] = byteArrayData[i];
                }

                // CheckSum
                byte byteCheckSum = byteData[1];
                for (i = 2; i < nPacketSize - 1; i++)
                {
                    byteCheckSum ^= byteData[i];
                }
                byteData[nNum++] = (byte)(byteCheckSum & 0x7f);

                //String strData;
                //strData = "nPacketSize = " + OjwConvert.IntToStr(nPacketSize) +
                //          ", nNum = " + OjwConvert.IntToStr(nNum);
                //Ojw.CMessage.Write(strData);
                OjwSock[nType].Send(byteData);
                //if (chkSycnIPEn.Checked == true) for (int nSync = 0; nSync < nCntSyncIP; nSync++) { if (OjwSockSync[nSync * 6 + nType].m_bConnect) OjwSockSync[nSync * 6 + nType].Send(byteData); }

                //strData = OjwConvert.IntToHex(byteData[0]) + " ";
                //for (i = 1; i < nNum; i++)
                //{
                //    strData += OjwConvert.IntToHex(byteData[i]) + " ";
                //}
                //Ojw.CMessage.Write(strData);
                //Ojw.CMessage.Write("CheckSum=" + OjwConvert.IntToHex((byte)(byteCheckSum & 0x7f)));
                //Ojw.CMessage.Write("AllDataSize = " + OjwConvert.IntToStr(nNum));

            }
            public bool Connect(string strIp)
            {
                int i, j, k;
                bool bOk = true;
                int nRetry = 1;
                bool bConnect = false;

                ////
                for (i = 0; i < _CNT_PORT; i++)
                {
                    m_achkEn[i] = true;
                }
                ////

                for (i = 0; i < nRetry; i++)
                {
                    for (j = 0; j < _CNT_PORT; j++)
                    {
                        bConnect = OjwSock[j].m_bConnect;
                        if ((!bConnect) && (m_achkEn[j]))
                        {
                            OjwSock[j].Connect(strIp, _PORT + j);

                            if (OjwSock[j].m_bConnect == true)
                            {
                                if (j == _ROBOT) Reader[j] = new Thread(new ThreadStart(Receive_Robot));
                                else if (j == _SOUND) Reader[j] = new Thread(new ThreadStart(Receive_Sound));
                                else if (j == _VISION) Reader[j] = new Thread(new ThreadStart(Receive_Vision));
                                else if (j == _MOTION) Reader[j] = new Thread(new ThreadStart(Receive_Motion));
                                else if (j == _SENSOR) Reader[j] = new Thread(new ThreadStart(Receive_Sensor));
                                else if (j == _EMOTICON) Reader[j] = new Thread(new ThreadStart(Receive_Emoticon));
                                Reader[j].Start();
                            }
                            else
                            {
                                for (k = 0; k < _CNT_PORT; k++)
                                {
                                    bConnect = OjwSock[k].m_bConnect;
                                    if (bConnect && (m_achkEn[k]))
                                    {
                                        OjwSock[k].DisConnect();
                                        Reader[k].Abort();         // 쓰레드 종료
                                    }
                                    m_achkEn[k] = true;
                                }
                                CMessage.Write_Error("서버와 연결 실패");
                                return false;
                            }
                        }
                    }          
                }
                for (i = 0; i < _CNT_PORT; i++)
                    if (m_achkEn[i]) bOk &= OjwSock[i].m_bConnect;

                if (bOk)
                {
                    //btnConnect.Text = "DisConnect";

                    for (i = 0; i < _CNT_PORT; i++)
                    {
                        bConnect = OjwSock[i].m_bConnect;
                        if ((bConnect) && (m_achkEn[i]))
                        {
                            CMessage.Write_Error("[{0}] Task Connected", i);
                        }
                        m_achkEn[i] = false;
                    }
                    //tmrHeartBeat.Enabled = true;
                    //tmrCheckAll.Enabled = true;

                    // 파일 데이터 저장
                    //TextConfigFileSave(m_strOrgDirectory + "\\ip.ini");
                }
                else
                {
                    for (i = 0; i < _CNT_PORT; i++)
                    {
                        bConnect = OjwSock[i].m_bConnect;
                        if (bConnect && (m_achkEn[i]))
                        {
                            OjwSock[i].DisConnect();
                            Reader[i].Abort();         // 쓰레드 종료
                        }
                    }
                    for (i = 0; i < 6; i++) m_achkEn[i] = true;

                    CMessage.Write_Error("서버와 연결 실패");
                    return false;
                }
                return true;
            }

            public void DisConnect()
            {
                int i;
                bool bConnect = false;
                for (i = 0; i < 6; i++)
                {
                    bConnect = OjwSock[i].m_bConnect;
                    if (bConnect && (m_achkEn[i]))
                        OjwSock[i].DisConnect();
                }
                //if (nCntSyncIP > 0) for (int nSync = 0; nSync < nCntSyncIP; nSync++) { if (OjwSockSync[nCntSyncIP].m_bConnect) OjwSockSync[nCntSyncIP].DisConnect(); }

                for (i = 0; i < 6; i++) m_achkEn[i] = true;

                //if (m_bClosed == false) btnConnect.Text = "Connect";
            }
            private String[] m_strType = new String[6] { "Robot", "Sound", "Vision", "Action", "Sensor", "Emoticon" };
#if true
            public void Receive_Robot()
            {
                int nType = _ROBOT;

                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    string strMessage0, strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            strMessage0 = "-------------------------\r\n<=" + m_strType[nType];
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;

                            string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                            strMessage0 += "(0x" + CConvert.IntToHex(byteData) + ")";
                            strMessage0 += "(0x" + CConvert.IntToHex(nCmd) + ")";
                            strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 24) & 0xff) + ")";
                            strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 16) & 0xff) + ")";
                            strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 8) & 0xff) + ")";
                            strMessage0 += "(0x" + CConvert.IntToHex(nLength & 0xff) + ")";

                            int i;
                            string[] pstrData;

                            strTmp = "";
                            for (i = 0; i < nLength; i++)
                                strTmp += "(0x" + CConvert.IntToHex(byteArrayData[i]) + ")";

                            strMessage0 += strTmp + "\r\n";
                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1) strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process OK]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "\r\n";
                                CMessage.Write(strMessage1);
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                //if (chkDispHearBeat.Checked)
                                //{
                                //    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]\r\n";
                                //    CMessage.Write(strMessage1);
                                //}
                                //else continue;
                            }
                            else
                            {
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                switch (nCmd)
                                {
                                    case 0x01:
                                        {
                                            //// 로봇 이름 확인
                                            //txtRobotID.Text = System.Text.Encoding.Default.GetString(byteArrayData, 0, ACTION_NAME_SIZE);
                                        }
                                        break;
                                    case 0x03:
                                        {
                                            //// 로봇 PassWd 확인
                                            //txtRobotPasswd.Text = System.Text.Encoding.Default.GetString(byteArrayData, 0, ACTION_NAME_SIZE);
                                        }
                                        break;
                                    case 0x11:
                                        {
                                            //// 현재 시간 확인
                                            //int i = 0;
                                            i = 0;
                                            string[] pstrWeek = new string[7] { "일", "월", "화", "수", "목", "금", "토" };
                                            if (nLength < 7) break;
                                            string strRobot_Year = CConvert.IntToStr(byteArrayData[i++] + 2000);
                                            string strRobot_Month = CConvert.IntToStr(byteArrayData[i++]);
                                            string strRobot_Date = CConvert.IntToStr(byteArrayData[i++]);
                                            i++;//cmbWeek.SelectedIndex = (int)(byteArrayData[i++] - 1);
                                            String strHour = CConvert.IntToStr(byteArrayData[i++]);
                                            String strMinute = CConvert.IntToStr(byteArrayData[i++]);
                                            String strSecond = CConvert.IntToStr(byteArrayData[i++]);
                                            Ojw.CMessage.Write("{0}-{1}-{2}", strRobot_Year, strRobot_Month, strRobot_Date);
                                            //monthCalendarRobot.SetDate(Convert.ToDateTime(
                                            //    strRobot_Year + "-" +
                                            //    strRobot_Month + "-" +
                                            //    strRobot_Date
                                            //    ));
                                        }
                                        break;
                                    case 0x41:
                                        {
                                            //int nPos = 0;
                                            //byte byTmp = 0;
                                            //m_nSchedule_Size = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //listviewSchedule.Items.Clear();
                                            //for (i = 0; i < m_nSchedule_Size; i++)
                                            //{
                                            //    m_pSSchedule[i].sIndex = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    m_pSSchedule[i].szTitle = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 59 + 2, C_SIZE_SCHEDULE_TITLE); nPos += C_SIZE_SCHEDULE_TITLE;
                                            //    m_pSSchedule[i].cFlag = byteArrayData[nPos++];

                                            //    m_pSSchedule[i].SDateStart.cYear = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cMonth = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cDay = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cWeek = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cHour = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cMinute = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateStart.cSecond = byteArrayData[nPos++];

                                            //    m_pSSchedule[i].cAlarm_Type = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].szAlarm_Name = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 59 + 2 + C_SIZE_SCHEDULE_TITLE + 9, C_SIZE_SCHEDULE_ALARM_NAME); nPos += C_SIZE_SCHEDULE_ALARM_NAME;
                                            //    m_pSSchedule[i].cRepeat_Type = byteArrayData[nPos++];

                                            //    m_pSSchedule[i].cRepeat_Week = byteArrayData[nPos++];

                                            //    byTmp = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].cRepeat_Count = (byte)((byTmp >> 4) & 0x0f);
                                            //    m_pSSchedule[i].cRepeat_Time = (byte)(byTmp & 0x0f);

                                            //    m_pSSchedule[i].SDateEnd.cYear = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cMonth = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cDay = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cWeek = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cHour = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cMinute = byteArrayData[nPos++];
                                            //    m_pSSchedule[i].SDateEnd.cSecond = byteArrayData[nPos++];

                                            //    m_pSSchedule[i].cSection = byteArrayData[nPos++];

                                            //    // ListView;
                                            //    listviewSchedule.Items.Add(m_pSSchedule[i].szTitle);
                                            //}
                                            //ScheduleDisp(0);
                                        }
                                        break;
                                    case 0x43:
                                        {
                                            string[] strDay = new string[7] { "일", "월", "화", "수", "목", "금", "토" };
                                            i = 0;
                                            int nData = (int)(byteArrayData[i++]);
                                            if (nData != 0) strData = "[Enable]";
                                            else strData = "[Disable]";

                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "년";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "월";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "일";
                                            nData = (int)(byteArrayData[i++]) - 1;
                                            if (nData < 0) nData = 0;
                                            strData += strDay[nData] + "요일";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "시";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "분";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "초";
                                            CMessage.Write(strData);
                                        }
                                        break;
                                    case 0x45:
                                        {
                                            string[] strDay = new string[7] { "일", "월", "화", "수", "목", "금", "토" };
                                            i = 0;
                                            int nData = (int)(byteArrayData[i++]);
                                            if (nData != 0) strData = "[Enable]";
                                            else strData = "[Disable]";

                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "년";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "월";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "일";
                                            nData = (int)(byteArrayData[i++]) - 1;
                                            if (nData < 0) nData = 0;
                                            strData += strDay[nData] + "요일";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "시";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "분";
                                            strData += CConvert.IntToStr(byteArrayData[i++]) + "초";
                                            strData += "지역코드=";
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            strData += (char)(byteArrayData[i++]);
                                            CMessage.Write(strData);
                                        }
                                        break;
                                    case 0x57:
                                        {
                                            strMessage1 += "모드체크\n";
                                            if (byteArrayData[0] == 0) CMessage.Write("자동모드");
                                            else CMessage.Write("수동모드");
                                        }
                                        break;
                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";
                                            }
                                        }
                                        break;
                                    case 0x84:
                                        {
                                            //int nPos = 0;
                                            //int nAiSize = (int)(byteArrayData[nPos++]);
                                            //int nCode;
                                            //short sData;
                                            //for (i = 0; i < nAiSize; i++)
                                            //{
                                            //    nCode = (int)(byteArrayData[nPos++]);
                                            //    txtRobotAI[i].Text = System.Text.Encoding.Default.GetString(byteArrayData, nPos, C_SIZE_TITLE); nPos += C_SIZE_TITLE;
                                            //    sData = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    lbRobotAI[i].Text = CConvert.IntToStr(sData);
                                            //    tbRobotAI[i].Value = (int)sData;
                                            //}
                                        }
                                        break;
                                    case 0x86:
                                        {
                                            //int nPos = 0;
                                            //int nEmotionValue = (int)(byteArrayData[nPos++]);
                                            //if (nEmotionValue == 1)
                                            //    txtRobotEmotion.Text = "기쁨";
                                            //else if (nEmotionValue == 2)
                                            //    txtRobotEmotion.Text = "즐거움";
                                            //else if (nEmotionValue == 3)
                                            //    txtRobotEmotion.Text = "슬픔";
                                            //else if (nEmotionValue == 4)
                                            //    txtRobotEmotion.Text = "놀람";
                                            //else if (nEmotionValue == 5)
                                            //    txtRobotEmotion.Text = "화남";
                                            //else if (nEmotionValue == 6)
                                            //    txtRobotEmotion.Text = "심심";
                                            //else if (nEmotionValue == 7)
                                            //    txtRobotEmotion.Text = "졸림";
                                            //else txtRobotEmotion.Text = "알수없음";
                                        }
                                        break;
                                    case 0x91:
                                        {
                                            //int nPos = 0;
                                            //int nAiSize = (int)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]);
                                            //nPos += 2;
                                            //short sData;
                                            //for (i = 0; i < nAiSize; i++)
                                            //{
                                            //    sData = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    //m_rowData_Contents[i]["Index"] = CConvert.IntToStr(sData);
                                            //    CMessage.Write("Index=" + CConvert.IntToStr(sData));
                                            //    m_rowData_Contents[i]["Contents"] = System.Text.Encoding.Default.GetString(byteArrayData, nPos, C_SIZE_TITLE).Trim('\0'); nPos += C_SIZE_TITLE;
                                            //    sData = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    m_rowData_Contents[i]["Remocon"] = CConvert.IntToStr(sData);//(int)(byteArrayData[nPos++]);
                                            //    m_rowData_Contents[i]["Action"] = System.Text.Encoding.Default.GetString(byteArrayData, nPos, C_SIZE_FILE_NAME).Trim('\0'); nPos += C_SIZE_FILE_NAME;
                                            //    sData = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    m_rowData_Contents[i]["Scenario"] = CConvert.IntToStr(sData);
                                            //    m_rowData_Contents[i]["Mp3"] = System.Text.Encoding.Default.GetString(byteArrayData, nPos, C_SIZE_FILE_NAME).Trim('\0'); nPos += C_SIZE_FILE_NAME;
                                            //}
                                        }
                                        break;
                                    case 0x93:
                                        {
                                            // ListView;                                        
                                            //int nPos = 0;
                                            //int nAiSize = (int)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]);
                                            //nPos += 2;
                                            //int nCode;
                                            //short sData;
                                            //listviewRobotScenario.Items.Clear();
                                            //String strTitle;
                                            //for (i = 0; i < nAiSize; i++)
                                            //{
                                            //    sData = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;
                                            //    nCode = sData;
                                            //    strTitle = System.Text.Encoding.Default.GetString(byteArrayData, nPos, C_SIZE_TITLE); nPos += C_SIZE_TITLE;
                                            //    //m_rowData_Contents[i]["Contents"] = strTitle;
                                            //    listviewRobotScenario.Items.Add(strTitle);
                                            //}
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            //strMessage1 += "[Version Get]\r\n";

                                            //strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            //lbVersion_Robot.Text = strTmp;
                                        }
                                        break;

                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                CMessage.Write(strMessage1);

                            }
                            CMessage.Write(strMessage0);
                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }

            public void Receive_Sound()
            {
                int nType = _SOUND;

                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    byte byteCheckSum = 0;
                    string strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;
                            // CheckSum Data Get
                            byteCheckSum = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;

                            string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                            int i;
                            string[] pstrData;

                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1) strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process Fail]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "\r\n";
                                CMessage.Write(strMessage1);
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                //if (chkDispHearBeat.Checked)
                                //{
                                //    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]\r\n";
                                //    CMessage.Write(strMessage1);
                                //}
                                //else continue;
                            }
                            else
                            {
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                switch (nCmd)
                                {
                                    case 0x01:
                                        {
                                            //listviewGenibo_Voice.Clear();
                                            //strMessage1 += "[음성반응 목록확인]\r\n";
                                            //int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                            //int nIndex;
                                            //m_nVoiceListCode_Sound = new int[nListCount];
                                            //m_nVoiceListCount_Sound = nListCount;
                                            //for (i = 0; i < nListCount; i++)
                                            //{
                                            //    nIndex = byteArrayData[2 + i * 26] * 256 + byteArrayData[2 + i * 26 + 1];
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 26 + 2, 21);
                                            //    //int nVoiceType = byteArrayData[2 + i * 26 + 2 + 21];
                                            //    //int nVoiceAction = byteArrayData[2 + i * 26 + 2 + 22] * 256 + byteArrayData[2 + i * 26 + 2 + 22 + 1];
                                            //    listviewGenibo_Voice.Items.Add(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                            //    m_nVoiceListCode_Sound[i] = nIndex;
                                            //    m_strVoiceRecog[nIndex] = strTmp.Trim('\0');
                                            //}
                                        }
                                        break;
                                    case 0x04:
                                        {
                                            ////short sIndex = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            //short sIndex = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));
                                            //if (sIndex <= 0) lbVoiceRecog.Text = "인식실패";
                                            ////else if (sIndex > listviewGenibo_Voice.Columns.Count)
                                            ////{
                                            ////    lbVoiceRecog.Text = "인식결과값 알 수 없음";
                                            ////}
                                            //else
                                            //{
                                            //    //String strFileName = listviewGenibo_Voice.Items[sIndex].Text;
                                            //    //strFileName = strFileName.Substring(0, strFileName.IndexOf("["));
                                            //    String strFileName = m_strVoiceRecog[sIndex];
                                            //    lbVoiceRecog.Text = strFileName;
                                            //}
                                        }
                                        break;
                                    case 0x13:
                                        {
#if true
                                            //listviewGenibo_Sound.Clear();
                                            //strMessage1 += "[음향 목록확인]\r\n";

                                            //short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            ////short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            //int nIndex;
                                            ////m_nMotionListCode = new int[sListCount];
                                            //for (i = 0; i < sListCount; i++)
                                            //{
                                            //    // Title & FileName
                                            //    // FileName
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 4) + 21, 16);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "[";
                                            //    // Title
                                            //    strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 4), 21);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "]";

                                            //    listviewGenibo_Sound.Items.Add(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                            //}
#else
                                        listviewGenibo_Sound.Clear();
                                        strMessage1 += "[음향 목록확인]\r\n";
                                        int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                        int nIndex;
                                        m_nSoundListCode_Sound = new int[nListCount];
                                        m_nSoundListCount_Sound = nListCount;
                                        for (i = 0; i < nListCount; i++)
                                        {
                                            nIndex = byteArrayData[2 + i * 23] * 256 + byteArrayData[2 + i * 23 + 1];
                                            strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 23 + 2, 21);
                                            listviewGenibo_Sound.Items.Add(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                            m_nSoundListCode_Sound[i] = nIndex;
                                        }
#endif
                                        }
                                        break;
                                    case 0x21:
                                        {
                                            //listviewGenibo_SoundMemo.Clear();
                                            //strMessage1 += "[음성메모 목록확인]\r\n";

                                            //short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            //int nIndex;
                                            //for (i = 0; i < sListCount; i++)
                                            //{
                                            //    // Title & FileName
                                            //    // FileName(16)
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 7) + 21, 16);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "[";
                                            //    // Title(21)
                                            //    strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 7), 21);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "]";
                                            //    // Date(7)
                                            //    // ....

                                            //    listviewGenibo_SoundMemo.Items.Add(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                            //}
                                        }
                                        break;
                                    case 0x31:
#if true
                                        {
                                            //listviewGenibo_MP3.Clear();
                                            //strMessage1 += "[MP3 모션목록확인]\r\n";

                                            //short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            ////short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            //int nIndex;
                                            ////m_nMotionListCode = new int[sListCount];
                                            //for (i = 0; i < sListCount; i++)
                                            //{
                                            //    // Title & FileName
                                            //    // FileName
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 4) + 21, 16);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "[";
                                            //    // Title
                                            //    strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16 + 4), 21);
                                            //    nIndex = strTmp.IndexOf('\0');
                                            //    strTmp = strTmp.Remove(nIndex);
                                            //    strTmp += "]";

                                            //    // PlayTime
                                            //    //....

                                            //    listviewGenibo_MP3.Items.Add(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                            //}
                                        }
#else
                                    {
                                        listviewGenibo_MP3.Clear();
                                        strMessage1 += "[MP3 목록확인]\r\n";
                                        int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                        int nIndex;
                                        m_nMP3ListCode_Sound = new int[nListCount];
                                        m_nMP3ListCount_Sound = nListCount;
                                        for (i = 0; i < nListCount; i++)
                                        {
                                            nIndex = byteArrayData[2 + i * (255 + 2)] * 256 + byteArrayData[2 + i * (255 + 2) + 1];
                                            strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (255 + 2) + 2, 255);
                                            listviewGenibo_MP3.Items.Add(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                            m_nMP3ListCode_Sound[i] = nIndex;
                                        }
                                    }
#endif
                                        break;

                                    //case 0x03:
                                    //    try
                                    //    {
                                    //        strMessage1 += "[Emoticon 업로드]\r\n";
                                    //        short sData;
                                    //        i = 0;
                                    //        int j;
                                    //        // Index
                                    //        //sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                    //        sData = (short)(byteArrayData[i++] * 256);
                                    //        sData += byteArrayData[i++];
                                    //        int nIndex = (int)sData;
                                    //        strMessage1 += "[Index=" + CConvert.IntToStr(nIndex) + "]";
                                    //        txtEmoticonIndex.Text = CConvert.IntToStr(nIndex);

                                    //        // Name
                                    //        txtEmoticonName.Text = System.Text.Encoding.Default.GetString(byteArrayData, i, 20);
                                    //        //txtTableName.Text = System.Text.Encoding.Default.GetString(byteArrayTitle, 0, 20);
                                    //        i += 21;

                                    //        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

                                    //        //// Data
                                    //        for (j = 0; j < 8; j++)
                                    //        {
                                    //            // Left
                                    //            SetEmoticon(_L_, j, byteArrayData[i++]);
                                    //        }
                                    //        for (j = 0; j < 8; j++)
                                    //        {
                                    //            // Right
                                    //            SetEmoticon(_R_, j, byteArrayData[i++]);
                                    //        }
                                    //    }
                                    //    catch
                                    //    {
                                    //        CMessage.Write("업로드 Error\n");
                                    //    }
                                    //    break;

                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\r\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";
                                            }
                                        }
                                        break;
                                    case 0x84:
                                        {
                                            ////tbSoundSpk.Value = (100 - byteArrayData[0]);
                                            //tbSoundSpk.Value = byteArrayData[0];
                                            //tbSoundSpk.Enabled = true;
                                            //lbSoundSpk.BackColor = Color.Yellow;
                                        }
                                        break;
                                    case 0x85:
                                        {
                                            ////tbSoundMic.Value = (100 - byteArrayData[0]);
                                            //tbSoundMic.Value = byteArrayData[0];
                                            //tbSoundMic.Enabled = true;
                                            //lbSoundMic.BackColor = Color.Yellow;
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            //strMessage1 += "[Version Get]\r\n";

                                            //strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            //lbVersion_Sound.Text = strTmp;
                                        }
                                        break;
                                    case 0xE2:
                                        strData = "    =>(Data[0]=0x" + CConvert.IntToHex(byteArrayData[0]) + ")\r\n" +
                                                  "    =>(Data[1]=0x" + CConvert.IntToHex(byteArrayData[1]) + ")\r\n" +
                                                  "    =>(Data[2]=0x" + CConvert.IntToHex(byteArrayData[2]) + ")\r\n" +
                                                  "    =>(Data[3]=0x" + CConvert.IntToHex(byteArrayData[3]) + ")\r\n";
                                        strMessage1 += "[Reserve]\r\n" + strData;
                                        break;
                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                CMessage.Write(strMessage1);

                            }
                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }

            private long m_lTick_Frame = 0;
            //private float m_fCalcFrame = 0.0f;
            public void Receive_Vision()
            {
                int nType = _VISION;
                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    //byte byteData_test= 0;
                    byte byteCheckSum = 0;
                    //string strMessage0;
                    string strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            //strMessage0 = "-------------------------\r\n<=" + m_strType[nType];
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;

                            //CMessage.Write("사이즈 - " + CConvert.IntToStr(OjwSock[nType].GetLength()));
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;
                            // CheckSum Data Get
                            byteCheckSum = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            
                            int i;
                            string[] pstrData;
                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1) strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process OK]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "\r\n";
                                CMessage.Write(strMessage1);
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                ////if (chkDispHearBeat.Checked)
                                ////{
                                ////    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]\r\n";
                                ////    CMessage.Write(strMessage1);
                                ////}
                                ////else continue;
                            }
                            else if (nCmd == 0x01)  // 정지영상 목록 확인
                            {
                                //int nFileNameSize = 16;
                                //listviewGenibo_Jpg.Clear();
                                //strMessage1 = "[촬영영상 목록확인]\r\n";
                                //int nIndex = 0;
                                //int nPos = 0;
                                //short sSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos));//BitConverter.ToInt16(byteArrayData, 0);
                                //nPos += 2;
                                //m_nVisionListCode_Jpg = new int[sSize];
                                //m_nVisionListCount_Jpg = sSize;
                                //for (i = 0; i < sSize; i++)
                                //{
                                //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, nPos, nFileNameSize);
                                //    strTmp = strTmp.Trim('\0') + ".jpg";
                                //    //string strTmp_Real = "";
                                //    //for (int j = 0; j < strTmp.Length; j++)
                                //    //{
                                //    //    if (strTmp[j] >= 0x30)
                                //    //        strTmp_Real += strTmp[j];
                                //    //}
                                //    nPos += nFileNameSize;
                                //    listviewGenibo_Jpg.Items.Add(CConvert.IntToStr(nIndex++), strTmp, 0);
                                //    m_nVisionListCode_Jpg[i] = nIndex;
                                //}
                            }
                            else if (nCmd == 0x02)  // GrabConti
                            {
                                //try
                                //{
                                //    int nFileNameSize = 16;
                                //    CMessage.Write("저장된 영상 로드(0x02)");
                                //    int nPos = 0;
                                //    short sSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos));//BitConverter.ToInt16(byteArrayData, 0);
                                //    nPos += 2;
                                //    string strFileName = System.Text.Encoding.Default.GetString(byteArrayData, nPos, nFileNameSize);
                                //    lbVisionFileName.Text = strFileName.Trim('\0') + ".jpg";
                                //    nPos += nFileNameSize;
                                //    MemoryStream streamData = new MemoryStream(byteArrayData, nPos, sSize);//byteArrayData.Length);
                                //    streamData.Read(byteArrayData, nPos, sSize);//nLength);

                                //    //Image image;
                                //    //Graphics gr = CreateGraphics();


                                //    m_image = Image.FromStream(streamData);
                                //    m_gr.DrawImage(m_image, 1, 1, m_image.Width, m_image.Height);

                                //    //gr.Dispose();
                                //}
                                //catch
                                //{
                                //    CMessage.Write_Error("저장된 영상 로드(0x02) - 실패");
                                //}
                            }
                            else if ((nCmd == 0x04) || (nCmd == 0x06)) // GrabConti
                            {
                                //try
                                //{
                                //    //CMessage.Write("0x04, 0x06");
                                //    //strMessage1 += "[연속 영상 확인]\r\n";

                                //    MemoryStream streamData = new MemoryStream(byteArrayData);
                                //    streamData.Read(byteArrayData, 0, nLength);

                                //    //Image image;
                                //    //Graphics gr = CreateGraphics();


                                //    m_image = Image.FromStream(streamData);
                                //    m_gr.DrawImage(m_image, 1, 1, m_image.Width, m_image.Height);

                                //    //gr.Dispose();

                                //    if (chkFrame.Checked == true)
                                //    {
                                //        if (m_bCheckFrameStart == true)
                                //        {
                                //            m_bCheckFrameStart = false;
                                //            OjwTimer.TimerSet(TID_CHECK_FRAME);
                                //            m_lTick_Frame = 0;
                                //        }
                                //        else if (OjwTimer.Timer(TID_CHECK_FRAME) >= m_lCheckFrameTimer)
                                //        {
                                //            //m_fCalcFrame = 
                                //            lbCheckTime.Text = String.Format("{0} Frames / {1} ms", m_lTick_Frame, OjwTimer.Timer(TID_CHECK_FRAME));
                                //            OjwTimer.TimerSet(TID_CHECK_FRAME);
                                //            m_lTick_Frame = 0;
                                //        }
                                //        else m_lTick_Frame++;
                                //    }

                                //}
                                //catch
                                //{
                                //    CMessage.Write_Error("GrabConti - 실패");
                                //}
                            }
                            else if ((nCmd == 0x14) || (nCmd == 0x16)) // GrabConti
                            {
                                //try
                                //{

                                //    //CMessage.Write("0x14, 0x16");
                                //    //strMessage1 += "[연속 영상 확인]\r\n";

                                //    //MemoryStream streamData = new MemoryStream(byteArrayData);
                                //    //streamData.Read(byteArrayData, 0, nLength);

                                //    //Image image;
                                //    //Graphics gr = CreateGraphics();

                                //    Color cData;
                                //    Bitmap bmpData = new Bitmap(320, 240);
                                //    for (int y = 0; y < 240; y++)
                                //        for (int x = 0; x < 320; x++)
                                //        {
                                //            cData = Color.FromArgb(byteArrayData[x + 320 * y], byteArrayData[x + 320 * y], byteArrayData[x + 320 * y]);
                                //            bmpData.SetPixel(x, y, cData);
                                //        }
                                //    //m_image = Image.FromStream(streamData);
                                //    m_gr.DrawImage(bmpData, 1, 1, bmpData.Width, bmpData.Height);

                                //    //gr.Dispose();

                                //}
                                //catch
                                //{
                                //    CMessage.Write_Error("GrabConti - 실패");
                                //}
                            }
                            else
                            {
                                string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";

                                switch (nCmd)
                                {
                                    ////case 0x01:
                                    ////    strMessage1 += "[모션재생]\r\n";
                                    ////    break;

                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\r\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";
                                            }
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            strMessage1 += "[Version Get]\r\n";

                                            strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            //lbVersion_Vision.Text = strTmp;
                                        }
                                        break;
                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                CMessage.Write(strMessage1);
                            }
                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }
            // 받고나서 보내야 하는 데이터가 있을 경우 이걸로 true 를 확인하고 보낸다. 단, 보내기전 이 변수를 false 로 변경할것...
            bool m_bReceiveOk = true;
            public int m_nCalibration = _CALIBRATION_SAVE;
            public const int _CALIBRATION_SAVE = 0;// Mode : 0 - save, 1 - reload,  2 - clear 
            public const int _CALIBRATION_LOAD = 1;
            public const int _CALIBRATION_CLEAR = 2;
            public void Receive_Motion()
            {
                int nType = _MOTION;

                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    byte byteCheckSum = 0;
                    string strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;
                            // CheckSum Data Get
                            byteCheckSum = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;

                            string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                            int i;
                            string[] pstrData;

                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    m_bReceiveOk = true; // 일단 데이터를 받으면 그냥 줌...
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1)
                                        strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process Fail]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex((byte)(byteArrayData[0])) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "size=" + CConvert.IntToHex(nLength) + "\r\n";
                                CMessage.Write(strMessage1);
                                // Calibration
                                if ((((byte)(byteArrayData[0])) == 0x40 + 36) || (((byte)(byteArrayData[0])) == 0x3C))
                                {
                                    if ((byte)(byteArrayData[1]) == 0)
                                    {
                                        if (m_nCalibration == _CALIBRATION_SAVE)
                                        {
                                            strTmp = "[저장 성공]";
                                        }
                                        else if (m_nCalibration == _CALIBRATION_LOAD)
                                        {
                                            strTmp = "[ReLoaded Calibration Data]";
                                        }
                                        else if (m_nCalibration == _CALIBRATION_CLEAR)
                                        {
                                            strTmp = "[Cleared Calibration Data]";
                                        }
                                        else
                                        {
                                            strTmp = "캘리브레이션 문제 발생 - 리턴값 종류 확인 불가(윈도우 프로그램[m_nCalibration] 문제)";
                                        }

                                    }
                                    else if ((byte)(byteArrayData[1]) == 1)
                                        strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[실패 - 다시 시도하시오.]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";

                                    MessageBox.Show("[Calibration] => " + ((m_nCalibration == _CALIBRATION_CLEAR) ? "Clear" : ((m_nCalibration == _CALIBRATION_LOAD) ? "Load" : ((m_nCalibration == _CALIBRATION_SAVE) ? "Save" : "Unknown"))) + strTmp);
                                }
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                //if (chkDispHearBeat.Checked)
                                //{
                                //    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]size=" + CConvert.IntToHex(nLength) + "\r\n";
                                //    CMessage.Write(strMessage1);
                                //}
                                //else continue;
                            }
                            else
                            {
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                switch (nCmd)
                                {
                                    //case 0x01:
                                    //    strMessage1 += "[모션재생]\r\n";
                                    //    break;
                                    #region 모션목록확인
                                    case 0x02: // 모션목록확인
                                        {
#if false
                                            strMessage1 += "[모션목록확인]\r\n";
                                            int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                            int nIndex;
                                            m_nMotionListCode = new int[nListCount];
                                            m_nMotionListCount = nListCount;

                                            for (i = 0; i < nListCount; i++)
                                            {
                                                nIndex = byteArrayData[2 + i * 23] * 256 + byteArrayData[2 + i * 23 + 1];
                                                //strMessage1 += "[Index=" + CConvert.IntToStr(nIndex) + "]";
                                                strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 23 + 2, 21);
                                                //strMessage1 += strTmp.Trim('\0') + "\r\n";
                                                listviewGenibo.Items.Add(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                                m_nMotionListCode[i] = nIndex;
                                            }
#else
                                            CMessage.Write("[모션목록확인]");
                                            int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                            int nIndex;
                                            //m_nMotionListCode = new int[nListCount];
                                            //m_nMotionListCount = nListCount;

                                            for (i = 0; i < nListCount; i++)
                                            {
                                                nIndex = byteArrayData[2 + i * 23] * 256 + byteArrayData[2 + i * 23 + 1];
                                                strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 23 + 2, 21);
                                                CMessage.Write(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                                //m_nMotionListCode[i] = nIndex;
                                            }
#endif
                                        }
                                        break;
                                    #endregion 모션목록확인
                                    #region 모션 Upload
                                    case 0x03:
                                        {
                                            try
                                            {
                                                strMessage1 += "[모션 업로드]\r\n";
                                                //short sData;
                                                i = 0;
                                                //int j;
                                                // Index
                                                int nIndex;
                                                nIndex = ((byteArrayData[i++] & 0xff) * 256);
                                                nIndex += (byteArrayData[i++] & 0xff);

                                                strMessage1 += "[Index=" + CConvert.IntToStr(nIndex) + "]";

                                                ///*if (nIndex == 0)
                                                //{
                                                //    sData = (short)((byteArrayData[i++] & 0xff) * 256);
                                                //    sData += (short)(byteArrayData[i++] & 0xff);

                                                //    i += 21;

                                                //    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                //    //// Data                                            
                                                //    int nTmp = sData;

                                                //    if (m_nPage == DISP_XY) j = dataGrid_XY.CurrentCell.RowIndex;
                                                //    else if (m_nPage == DISP_ANGLE) j = dataGrid_Angle.CurrentCell.RowIndex;
                                                //    else j = dataGrid_Evd.CurrentCell.RowIndex;
                                                //    DataClear(DISP_XY, j);
                                                //    //Index
                                                //    m_rowData_XY[j]["Index"] = j;
                                                //    // En
                                                //    //if (j < nTmp)
                                                //        m_rowData_XY[j]["En"] = 1;
                                                //    // Action Data
                                                //    //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                                //    string[] pstrData2 = new string[4] { "Lf", "Lr", "Rf", "Rr" };
                                                //    string strTmp2;
                                                //    for (int k = 0; k < 4; k++)
                                                //    {
                                                //        sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                //        strTmp2 = pstrData2[k] + "X"; m_rowData_XY[j][strTmp2] = sData;
                                                //        sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                //        strTmp2 = pstrData2[k] + "Y"; m_rowData_XY[j][strTmp2] = sData;
                                                //        sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                //        strTmp2 = pstrData2[k] + "W"; m_rowData_XY[j][strTmp2] = Math.Round(CalcEvd2Angle(sData*4), 1);
                                                //        //sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                //        strTmp2 = pstrData2[k] + "Ovr"; m_rowData_XY[j][strTmp2] = 0;
                                                //    }

                                                //    // Tail
                                                //    sData = BitConverter.ToInt16(byteArrayData, i); i += 2; m_rowData_XY[j]["Tail"] = Math.Round(CalcEvd2Angle(sData * 4), 1);
                                                //    // Pan
                                                //    sData = BitConverter.ToInt16(byteArrayData, i); i += 2; m_rowData_XY[j]["Pan"] = Math.Round(CalcEvd2Angle(sData * 4), 1);
                                                //    // Tilt                                                
                                                //    sData = BitConverter.ToInt16(byteArrayData, i); i += 2; m_rowData_XY[j]["Tilt"] = Math.Round(CalcEvd2Angle(sData * 4), 1);

                                                //    TableMove_XY2Evd(j);
                                                //    TableMove_Evd2Angle(j);
                                                //}*/
                                                ////else
                                                ////{
                                                ////    // Size
                                                ////    sData = (short)((byteArrayData[i++] & 0xff) * 256);
                                                ////    sData += (short)(byteArrayData[i++] & 0xff);

                                                ////    m_nDataSize = C_MAX_MOTION_FRAME_SIZE;// (int)sData;
                                                ////    // Title
                                                ////    txtTableName.Text = System.Text.Encoding.Default.GetString(byteArrayData, i, 20);
                                                ////    //txtTableName.Text = System.Text.Encoding.Default.GetString(byteArrayTitle, 0, 20);
                                                ////    i += 21;

                                                ////    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

                                                ////    TableClear(DISP_XY);
                                                ////    DataClear();

                                                ////    //// Data                                            
                                                ////    int nTmp = sData;
                                                ////    for (j = 0; j < m_nDataSize; j++)
                                                ////    {
                                                ////        //Index
                                                ////        m_rowData[j][m_pstrData[0]] = j;
                                                ////        // En
                                                ////        if (j < nTmp)
                                                ////            m_rowData[j][m_pstrData[1]] = 1;
                                                ////        // Action Data
                                                ////        //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                                ////        for (int k = 0; k < 3; k++)
                                                ////        {
                                                ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                                ////        }
                                                ////        for (int k = 6; k < 9; k++)
                                                ////        {
                                                ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                                ////        }
                                                ////        for (int k = 3; k < 6; k++)
                                                ////        {
                                                ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                                ////        }
                                                ////        for (int k = 9; k < 21; k++)
                                                ////        {
                                                ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                                ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                                ////        }
                                                ////        //i += 2;
                                                ////    }
                                                ////}                                           
                                            }
                                            catch
                                            {
                                                CMessage.Write_Error("업로드 Error\n");
                                            }
                                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                                        }
                                        break;
                                        #endregion 모션 Upload
                                    #region Evd8 Data Get
                                    case 0x06:
                                        {
                                            strMessage1 += "[Evd 8 Data Get]\r\n";
                                            //// 12 - Temp, 13 - Yaw, 14 - Mouth, 15~17 - Pan, Tilt, Tail
                                            ////i = 0;
                                            ////lbLfW.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbLfDn.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbLfUp.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                            ////lbRfW.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbRfDn.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbRfUp.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                            ////lbLrW.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbLrDn.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbLrUp.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                            ////lbRrW.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbRrDn.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbRrUp.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                            //////i++; // Temp
                                            ////lbTiltUp.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbMouth.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                            ////lbPan.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbTilt.Text = CConvert.IntToStr((int)(byteArrayData[i++]));
                                            ////lbTail.Text = CConvert.IntToStr((int)(byteArrayData[i++]));

                                        }
                                        break;
                                    #endregion Evd8 Data Get
                                    #region 모션목록확인
                                    case 0x0c:
                                        {
                                            ///*listviewGenibo_By_FileName.Clear();
                                            //strMessage1 += "[모션목록확인]\r\n";
                                            //int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                            ////int nIndex;
                                            ////m_nMotionListCode = new int[nListCount];
                                            ////m_nMotionListCount = nListCount;
                                            //for (i = 0; i < nListCount; i++)
                                            //{
                                            //    //strMessage1 += "[Index=" + CConvert.IntToStr(nIndex) + "]";
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, i * 256 + 2, 256);
                                            //    //strMessage1 += strTmp.Trim('\0') + "\r\n";

                                            //    pstrData = strTmp.Split('/');
                                            //    int j = 0;
                                            //    //string strTmp2;
                                            //    foreach (string strItem in pstrData)
                                            //    {
                                            //        if (String.Compare(strItem, "master") == 0)
                                            //            j = 0;
                                            //        else if (String.Compare(strItem, "user") == 0)
                                            //            j = 1;
                                            //        else if (String.Compare(strItem, "memory") == 0)
                                            //            j = 2;
                                            //        //    else strTmp2 = strItem;
                                            //    }

                                            //    listviewGenibo_By_FileName.Items.Add(CConvert.IntToStr(i), strTmp.Trim('\0'), j);
                                            //    //m_nMotionListCode[i] = nIndex;
                                            //}*/
                                        }
                                        break;
                                    #endregion 모션목록확인
                                    #region Motion Upload
                                    case 0x0d:
                                        {
                                            ////try
                                            ////{
                                            ////    strMessage1 += "[모션 업로드]\r\n";
                                            ////    short sData;
                                            ////    i = 0;
                                            ////    int j;

                                            ////    // Size
                                            ////    sData = (short)((byteArrayData[i++] & 0xff) * 256);
                                            ////    sData += (short)(byteArrayData[i++] & 0xff);
                                            ////    m_nDataSize = C_MAX_MOTION_FRAME_SIZE;// (int)sData;
                                            ////    txtTableName.Text = System.Text.Encoding.Default.GetString(byteArrayData, i, 20);
                                            ////    i += 21;

                                            ////    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

                                            ////    TableClear(DISP_XY);
                                            ////    DataClear();

                                            ////    // Data
                                            ////    int nTmp = sData;
                                            ////    for (j = 0; j < m_nDataSize; j++)
                                            ////    {
                                            ////        //Index
                                            ////        m_rowData[j][m_pstrData[0]] = j;
                                            ////        // En
                                            ////        if (j < nTmp)
                                            ////            m_rowData[j][m_pstrData[1]] = 1;
                                            ////        // Action Data
                                            ////        //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            ////        for (int k = 0; k < 3; k++)
                                            ////        {
                                            ////            //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                            ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                            ////        }
                                            ////        for (int k = 6; k < 9; k++)
                                            ////        {
                                            ////            //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                            ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                            ////        }
                                            ////        for (int k = 3; k < 6; k++)
                                            ////        {
                                            ////            //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                            ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                            ////        }
                                            ////        for (int k = 9; k < 21; k++)
                                            ////        {
                                            ////            //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            ////            sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                            ////            m_rowData[j][m_pstrData[k + 2]] = sData;
                                            ////        }
                                            ////        //i += 2;
                                            ////    }
                                            ////    // XY Grid => Evd8 Grid
                                            ////    XYGrid2Evd8Grid();
                                            ////}
                                            ////catch
                                            ////{
                                            ////    XYGrid2Evd8Grid();
                                            ////    CMessage.Write("업로드 Error\n");
                                            ////}
                                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                                        }
                                        break;
                                    #endregion Motion Upload
                                    #region Angle Data Get
                                    case 0x12:
                                        {
                                            nID = byteArrayData[0];
                                            strMessage1 += "[Angle(" + CConvert.IntToStr(nID) + ") Get]\r\n";
                                            int nValue = (int)(byteArrayData[1] * 256 * 256 * 256 + byteArrayData[2] * 256 * 256 + byteArrayData[3] * 256 + byteArrayData[4]);
                                            nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            strTmp = CConvert.IntToStr(nValue);

                                            //if (nID == LEFT_FRONT_DOWN_ID) txtAngle_LfDn.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_UP_ID) txtAngle_LfUp.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_WING_ID) txtAngle_LfW.Text = strTmp;
                                            //else if (nID == LEFT_REAR_DOWN_ID) txtAngle_LrDn.Text = strTmp;
                                            //else if (nID == LEFT_REAR_UP_ID) txtAngle_LrUp.Text = strTmp;
                                            //else if (nID == LEFT_REAR_WING_ID) txtAngle_LrW.Text = strTmp;

                                            //else if (nID == RIGHT_FRONT_DOWN_ID) txtAngle_RfDn.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_UP_ID) txtAngle_RfUp.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_WING_ID) txtAngle_RfW.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_DOWN_ID) txtAngle_RrDn.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_UP_ID) txtAngle_RrUp.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_WING_ID) txtAngle_RrW.Text = strTmp;

                                            //else if (nID == HEAD_TILTUP_ID) txtAngle_TiltUp.Text = strTmp;
                                            //else if (nID == HEAD_MOUTH_ID) txtAngle_Mouth.Text = strTmp;
                                            //else if (nID == HEAD_PAN_ID) txtAngle_Pan.Text = strTmp;
                                            //else if (nID == HEAD_TILT_ID) txtAngle_Tilt.Text = strTmp;
                                            //else if (nID == HIPS_TAIL_ID) txtAngle_Tail.Text = strTmp;
                                        }
                                        break;
                                    #endregion Angle Data Get
                                    #region Angle Data Get(All)
                                    case 0x22:
                                        {
                                            strMessage1 += "[Angle(All) Get]\r\n";
                                            //int j = 0, nValue;
                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LfW.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LfDn.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LfUp.Text = strTmp;


                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RfW.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RfDn.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RfUp.Text = strTmp;


                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LrW.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LrDn.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_LrUp.Text = strTmp;


                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RrW.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RrDn.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_RrUp.Text = strTmp;

                                            //j++; // Temp
                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_TiltUp.Text = strTmp;
                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_Mouth.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_Pan.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_Tilt.Text = strTmp;

                                            //i = j++ * 4;
                                            //nValue = (int)(byteArrayData[i] * 256 * 256 * 256 + byteArrayData[i + 1] * 256 * 256 + byteArrayData[i + 2] * 256 + byteArrayData[i + 3]);
                                            //nValue = (nValue == 0) ? 0 : (int)Math.Round((double)nValue / 1000.0);
                                            //strTmp = CConvert.IntToStr(nValue);
                                            //txtAngle_Tail.Text = strTmp;

                                            //if (m_bDisp == true) Angle_Display();
                                            //m_bDisp = false;
                                        }
                                        break;
                                    #endregion Angle Data Get(All)
                                    #region 마스터 모션목록확인
                                    case 0x31:
                                        {
#if false
                                            listviewGenibo.Clear();
                                            strMessage1 += "[마스터 모션목록확인]\r\n";

                                            short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            //short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            int nIndex;
                                            m_nMotionListCode = new int[sListCount];
                                            string strTmpFileName = "";
                                            string strTmpTitleName = "";

                                            for (i = 0; i < sListCount; i++)
                                            {
                                                try
                                                {
                                                    // Title & FileName
                                                    // Title
                                                    strTmpTitleName = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16), 21);
                                                    nIndex = strTmpTitleName.IndexOf('\0');
                                                    if (nIndex > 0)
                                                        strTmpTitleName = strTmpTitleName.Remove(nIndex);
                                                    // FileName
                                                    strTmpFileName = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16) + 21, 16);
                                                    nIndex = strTmpFileName.IndexOf('\0');
                                                    if (nIndex > 0)
                                                        strTmpFileName = strTmpFileName.Remove(nIndex);
                                                    else if (nIndex < 0)
                                                    {
                                                        nIndex = strTmpFileName.IndexOf(".act");
                                                        if (nIndex > 0)
                                                            strTmpFileName = strTmpFileName.Remove(nIndex);
                                                    }
                                                    strTmpFileName += ".act";

                                                    strTmp = strTmpFileName;//
                                                    strTmp += "[";
                                                    if (strTmpTitleName != "") strTmp += strTmpTitleName;
                                                    strTmp += "]";

                                                    //// FileName
                                                    //strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16) + 21, 16);
                                                    //nIndex = strTmp.IndexOf('\0');
                                                    //if (nIndex == 0)
                                                    //    continue;
                                                    //strTmp = strTmp.Remove(nIndex);
                                                    //strTmp += ".act[";
                                                    //// Title
                                                    //strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16), 21);
                                                    //nIndex = strTmp.IndexOf('\0');
                                                    //strTmp = strTmp.Remove(nIndex);
                                                    //strTmp += "]";

                                                    listviewGenibo.Items.Add(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                                }
                                                catch
                                                {
                                                    CMessage.Write_Error("ListGet Error");
                                                }
                                            }
#else
                                            //listviewGenibo.Clear();
                                            strMessage1 += "[마스터 모션목록확인]\r\n";

                                            short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            //short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            int nIndex;
                                            //m_nMotionListCode = new int[sListCount];
                                            string strTmpFileName = "";
                                            string strTmpTitleName = "";

                                            for (i = 0; i < sListCount; i++)
                                            {
                                                try
                                                {
                                                    // Title & FileName
                                                    // Title
                                                    strTmpTitleName = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16), 21);
                                                    nIndex = strTmpTitleName.IndexOf('\0');
                                                    if (nIndex > 0)
                                                        strTmpTitleName = strTmpTitleName.Remove(nIndex);
                                                    // FileName
                                                    strTmpFileName = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16) + 21, 16);
                                                    nIndex = strTmpFileName.IndexOf('\0');
                                                    if (nIndex > 0)
                                                        strTmpFileName = strTmpFileName.Remove(nIndex);
                                                    else if (nIndex < 0)
                                                    {
                                                        nIndex = strTmpFileName.IndexOf(".act");
                                                        if (nIndex > 0)
                                                            strTmpFileName = strTmpFileName.Remove(nIndex);
                                                    }
                                                    strTmpFileName += ".act";

                                                    strTmp = strTmpFileName;//
                                                    strTmp += "[";
                                                    if (strTmpTitleName != "") strTmp += strTmpTitleName;
                                                    strTmp += "]";

                                                    CMessage.Write(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                                }
                                                catch
                                                {
                                                    CMessage.Write_Error("ListGet Error");
                                                }
                                            }
#endif
                                        }
                                        break;
                                    #endregion 마스터 모션목록확인
                                    #region 유저 모션목록확인
                                    case 0x32:
                                        {
#if false
                                            listviewGenibo_User.Clear();
                                            strMessage1 += "[유저 모션목록확인]\r\n";

                                            short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            //short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            int nIndex;
                                            m_nMotionListCode = new int[sListCount];
                                            for (i = 0; i < sListCount; i++)
                                            {
                                                // Title & FileName
                                                // FileName
                                                strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + C_TCP_FILENAME_SIZE) + 21, C_TCP_FILENAME_SIZE);
                                                nIndex = strTmp.IndexOf('\0');
                                                strTmp = strTmp.Remove(nIndex);
                                                strTmp += ".act[";
                                                // Title
                                                strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + C_TCP_FILENAME_SIZE), 21);
                                                nIndex = strTmp.IndexOf('\0');
                                                strTmp = strTmp.Remove(nIndex);
                                                strTmp += "]";

                                                //// Title
                                                //strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16), 21);
                                                //nIndex = strTmp.IndexOf('\0');
                                                //strTmp = strTmp.Remove(nIndex);
                                                //strTmp += "[";
                                                //// FileName
                                                //strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + 16) + 21, 16);
                                                //nIndex = strTmp.IndexOf('\0');
                                                //strTmp = strTmp.Remove(nIndex);
                                                //strTmp += ".act]";

                                                listviewGenibo_User.Items.Add(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                            }
#else
                                            //listviewGenibo_User.Clear();
                                            strMessage1 += "[유저 모션목록확인]\r\n";

                                            short sListCount = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, 0));//BitConverter.ToInt16(byteArrayData, 0);
                                            //short sListCount = (short)(byteArrayData[0] * 256 + byteArrayData[1]);
                                            int nIndex;
                                            //m_nMotionListCode = new int[sListCount];
                                            for (i = 0; i < sListCount; i++)
                                            {
                                                // Title & FileName
                                                // FileName
                                                strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + C_TCP_FILENAME_SIZE) + 21, C_TCP_FILENAME_SIZE);
                                                nIndex = strTmp.IndexOf('\0');
                                                strTmp = strTmp.Remove(nIndex);
                                                strTmp += ".act[";
                                                // Title
                                                strTmp += System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * (21 + C_TCP_FILENAME_SIZE), 21);
                                                nIndex = strTmp.IndexOf('\0');
                                                strTmp = strTmp.Remove(nIndex);
                                                strTmp += "]";
                                                
                                                CMessage.Write(CConvert.IntToStr(i) + '/' + CConvert.IntToStr(sListCount), strTmp, 0);
                                            }
#endif
                                        }
                                        break;
                                    #endregion 유저 모션목록확인
                                    #region Motion Load(master)
                                    case 0x35:
                                        {
                                            string strFileName = System.Text.Encoding.Default.GetString(byteArrayData, 0, C_TCP_FILENAME_SIZE);
//#if false
//                                        strFileName = m_strWorkDirectory + "\\" + strFileName.Trim('\0') + ".act";
//                                        FileInfo f = new FileInfo(strFileName);
//                                        FileStream fs = f.Create();
//                                        fs.Write(byteArrayData, C_TCP_FILENAME_SIZE, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                        fs.Close();
//#else
//                                            byte[] pbyteBuffer = new byte[byteArrayData.Length - C_TCP_FILENAME_SIZE];

//                                            Array.Copy(byteArrayData, C_TCP_FILENAME_SIZE, pbyteBuffer, 0, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                            //byteArrayData.CopyTo(pbyteBuffer, C_TCP_FILENAME_SIZE);//, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                            if (DataFileOpen(strFileName.Trim('\0'), pbyteBuffer, true, true) == false)
//                                                CMessage.Write_Error("Data File Receive Error");
//#endif
//                                            // m_strWorkDirectory + "\\";
                                        }
                                        break;
                                    #endregion Motion Load(master)
                                    #region Motion Load(User)
                                    case 0x36:
                                        {
                                            string strFileName = System.Text.Encoding.Default.GetString(byteArrayData, 0, C_TCP_FILENAME_SIZE);
//#if false
//                                        strFileName = m_strWorkDirectory + "\\" + strFileName.Trim('\0') + ".act";
//                                        FileInfo f = new FileInfo(strFileName);
//                                        FileStream fs = f.Create();
//                                        fs.Write(byteArrayData, C_TCP_FILENAME_SIZE, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                        fs.Close();
//#else
//                                            byte[] pbyteBuffer = new byte[byteArrayData.Length - C_TCP_FILENAME_SIZE];

//                                            Array.Copy(byteArrayData, C_TCP_FILENAME_SIZE, pbyteBuffer, 0, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                            //byteArrayData.CopyTo(pbyteBuffer, C_TCP_FILENAME_SIZE);//, byteArrayData.Length - C_TCP_FILENAME_SIZE);
//                                            if (DataFileOpen(strFileName.Trim('\0'), pbyteBuffer, true, true) == false)
//                                                CMessage.Write_Error("Data File Receive Error");
//#endif
//                                            // m_strWorkDirectory + "\\";
                                        }
                                        break;
                                    #endregion Motion Load(User)
                                    #region Qwm3(Weight)
                                    case 0x3b:
                                        {
                                            //int nPos = 0;
                                            //float[] fValue = new float[5];
                                            //for (i = 0; i < 5; i++)
                                            //{
                                            //    fValue[i] = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_TurnWeight = fValue[i];
                                            //}
                                            //DispQwm3Data_Only_Weight();
                                        }
                                        break;
                                    #endregion Qwm3(Weight)
                                    case 0x3d:
                                        {
                                            if (nLength != 2 + ((15 + 3) * sizeof(short))) { MessageBox.Show("패킷 사이즈 에러"); break; }

                                            ////if ((byte)(byteArrayData[1]) == 0)
                                            ////{
                                            ////    if (m_nCalibration == _CALIBRATION_SAVE)
                                            ////    {
                                            ////        strTmp = "[저장 성공]";
                                            ////    }
                                            ////    else if (m_nCalibration == _CALIBRATION_LOAD)
                                            ////    {
                                            ////        strTmp = "[ReLoaded Calibration Data]";
                                            ////    }
                                            ////    else if (m_nCalibration == _CALIBRATION_CLEAR)
                                            ////    {
                                            ////        strTmp = "[Cleared Calibration Data]";
                                            ////    }
                                            ////    else
                                            ////    {
                                            ////        strTmp = "캘리브레이션 문제 발생 - 리턴값 종류 확인 불가(윈도우 프로그램[m_nCalibration] 문제)";
                                            ////    }

                                            ////}
                                            ////else if ((byte)(byteArrayData[1]) == 1)
                                            ////    strTmp = "[Process End]";
                                            ////else if ((byte)(byteArrayData[1]) == 2) strTmp = "[실패 - 다시 시도하시오.]";
                                            ////else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                            ////else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";

                                            ////MessageBox.Show("[Calibration] => " + strTmp);


                                            int nPos = 0;
                                            int nMode = byteArrayData[nPos++]; // Mode : 0 - save, 1 - reload,  2 - clear 
                                            int nResult = byteArrayData[nPos++]; // Result : 0 - Error, 1 - OK
                                            int nErrorType = byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            int nErrorRange = byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            int nErrorVibrationRange = byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            int[] nValue = new int[15];
                                            for (i = 0; i < 15; i++)
                                            {
                                                nValue[i] = byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            }

                                            String strError = "";
                                            if (nMode == _CALIBRATION_SAVE)
                                            {
                                                strError = "[Save " + ((nResult == 1) ? "Ok]" : "Fail]") + "\n";

                                                if (nErrorType > 0)
                                                {
                                                    strError += ((nErrorType == 1) ? "관절 조립 상태 불량" : ((nErrorType == 2) ? "모터가 흔들리지 않게 고정 하고 다시 시도하십시오." : "알 수 없는 에러 발생"));
                                                    strError += "\n기준값 = " + CConvert.IntToStr(((nErrorType == 1) ? nErrorRange : nErrorVibrationRange)) + "\n";
                                                    for (i = 0; i < 11; i++) // 까지만 검사하자 - 머리 불필요
                                                    {
                                                        if (nValue[i] != 0) strError += "[" + CConvert.IntToStr(i) + "번 모터(" + CConvert.IntToStr(nValue[i]) + ")]\n";
                                                    }
                                                }
                                            }
                                            else if (nMode == _CALIBRATION_LOAD)
                                            {
                                                strError = "[ReLoaded Calibration Data - " + ((nResult == 1) ? "Ok]" : "Fail]");
                                            }
                                            else if (nMode == _CALIBRATION_CLEAR)
                                            {
                                                strError = "[Cleared Calibration Data]";
                                            }
                                            else
                                            {
                                                strError = "캘리브레이션 문제 발생 - 리턴값 종류 확인 불가";
                                            }

                                            MessageBox.Show(strError);
                                        }
                                        break;
                                    case 0x3e:
                                        {
                                            ////if (nLength != 3 + ) { MessageBox.Show("패킷 사이즈 에러"); break; }

                                            //int nPos = 0;
                                            //int nMode = byteArrayData[nPos++]; // 0 - 개별모터 동작, 1 - 모션을 이용하여 동작
                                            //int nResult = byteArrayData[nPos++]; // Result : 0 - Error, 1 - OK
                                            //int nMotID = byteArrayData[nPos++]; // Motor ID
                                            //short sDiff_Max = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //short sDiff_Min = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //short sTemp_Max = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //short sTemp_Min = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //short sVolt_Max = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //short sVolt_Min = (short)(byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]); nPos += 2;
                                            //int nFileSize = (int)(
                                            //                        byteArrayData[nPos] * 256 * 256 * 256 +
                                            //                        byteArrayData[nPos + 1] * 256 * 256 +
                                            //                        byteArrayData[nPos + 2] * 256 +
                                            //                        byteArrayData[nPos + 3]); nPos += 4;
                                            //String strError = (nMode == 0) ? "[모터체크 - 개별모터 동작]" : "[모터체크 - 모션을 이용한 동작]\n";
                                            //strError += CConvert.IntToStr(nMotID) + "번 모터 : " + ((nResult != 0) ? "Ok" : "Fail") + "\n";
                                            //strError += "Diff[Max = " + CConvert.IntToStr(sDiff_Max) + ", Min = " + CConvert.IntToStr(sDiff_Min) + "]\n";
                                            //strError += "Temp[Max = " + CConvert.IntToStr(sTemp_Max) + ", Min = " + CConvert.IntToStr(sTemp_Min) + "]\n";
                                            //strError += "Volt[Max = " + CConvert.IntToStr(sVolt_Max) + ", Min = " + CConvert.IntToStr(sVolt_Min) + "]\n";

                                            //MessageBox.Show(strError);

                                            //if (nFileSize > 0)
                                            //{
                                            //    String strSaveFileName = "c:\\test.csv";
                                            //    if (CInputBox.Show("File", "저장할 파일의 이름을 입력하시오", ref strSaveFileName) == DialogResult.OK)
                                            //    {
                                            //        byte[] pFile = new byte[nFileSize];
                                            //        Array.Copy(byteArrayData, nPos, pFile, 0, nFileSize);
                                            //        BinaryTestFileSave(strSaveFileName, nFileSize, pFile);
                                            //        pFile = null;
                                            //    }
                                            //}
                                        }
                                        break;
                                    case (0x40 + 20):
                                        {
                                            //switch (byteArrayData[0])
                                            //{
                                            //    case 2: // 1.33 ~ 1.70
                                            //        {
                                            //            CMessage.Write("ACT_CMD_GET_PARAM_QWM3");
                                            //            int j;
                                            //            int nPos = 1;
                                            //            for (i = 0; i < 2; i++)
                                            //                for (j = 0; j < 73; j++)
                                            //                {
                                            //                    g_afParamQwm[i, j] = BitConverter.ToSingle(byteArrayData, nPos);
                                            //                    nPos += sizeof(float);
                                            //                }
                                            //            DispQwm3Data();

                                            //            SendCommand(_MOTION, 0x3B);// 일반 가중치도 가져온다.
                                            //        }
                                            //        break;
                                            //    case 4: // 1.71
                                            //        {
                                            //            CMessage.Write("ACT_CMD_GET_PARAM_QWM3_V_1_71");
                                            //            int j;
                                            //            int nPos = 1;
                                            //            for (i = 0; i < 2; i++)
                                            //                for (j = 0; j < (73 + 10); j++)
                                            //                {
                                            //                    g_afParamQwm[i, j] = BitConverter.ToSingle(byteArrayData, nPos);
                                            //                    nPos += sizeof(float);
                                            //                }
                                            //            DispQwm3Data();

                                            //            SendCommand(_MOTION, 0x3B);// 일반 가중치도 가져온다.
                                            //        }
                                            //        break;
                                            //    default:
                                            //        CMessage.Write("(ETC)알수 없는 명령");
                                            //        break;
                                            //}
                                        }
                                        break;
                                    ////                                 case (0x40 + 36):
                                    ////                                     {
                                    ////                                         if (byteArrayData.Rank >= 1)
                                    ////                                         {
                                    ////                                             m_bReceiveOk = true; // 일단 데이터를 받으면 그냥 줌...
                                    ////                                             if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    ////                                             else if ((byte)(byteArrayData[1]) == 1)
                                    ////                                                 strTmp = "[Process End]";
                                    ////                                             else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process Fail]";
                                    ////                                             else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    ////                                             else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                    ////                                         }
                                    ////                                         //strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "size=" + CConvert.IntToHex(nLength) + "\r\n";
                                    ////                                         //CMessage.Write(strMessage1);
                                    //// 
                                    ////                                         if (nCmd == 0x40 + 36)
                                    ////                                         {
                                    ////                                             MessageBox.Show(strTmp);
                                    ////                                         }
                                    ////                                     }
                                    ////                                     break;
                                    case (0x40 + 42):
                                        {
                                            //int nPos = 0;
                                            //for (i = 0; i < 5; i++)
                                            //{
                                            //    g_aSParamQwm[i].SReady.fDownH_Leg_F = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fDownH_Leg_R = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fDownGap = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fLeg_LR_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fLeg_LR_D = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fLeg_RF_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fLeg_RF_D = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fSway_Left = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fSway_Right = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.fSway_Back = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SReady.nSpeed = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SReady.fPercent = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);

                                            //    g_aSParamQwm[i].SWalking.fLeg_F_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_R_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_D = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_Turn = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_TurnWeight = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fLeg_Damp = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.fSway = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SWalking.nSpeed_1 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.nDelay_1 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.nSpeed_2 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.nDelay_2 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.nSpeed_3 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.nDelay_3 = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SWalking.fPercent = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);

                                            //    g_aSParamQwm[i].SEnd.fLeg_F_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SEnd.fLeg_R_H = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SEnd.fSway = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //    g_aSParamQwm[i].SEnd.nSpeed = BitConverter.ToInt32(byteArrayData, nPos); nPos += sizeof(int);
                                            //    g_aSParamQwm[i].SEnd.fPercent = BitConverter.ToSingle(byteArrayData, nPos); nPos += sizeof(float);
                                            //}
                                            //DispQwmData();
                                        }
                                        break;
                                    case 0x40 + 47:
                                        {
                                            //int nPos = 0;
                                            //bool bCliff = (byteArrayData[nPos] == 0) ? false : true; nPos++;
                                            //bool bObstacle = (byteArrayData[nPos] == 0) ? false : true; nPos++;
                                            ////////////////////
                                            //DisplayCliffEn(bCliff);
                                            //DisplayObstacleEn(bObstacle);
                                            //int j;
                                            //int nCnt = 3;
                                            //for (i = 0; i < 2; i++)
                                            //    for (j = 0; j < nCnt; j++)
                                            //    {
                                            //        m_anCliff[i, j] = byteArrayData[2 + i * nCnt + j];
                                            //        m_anCliffValue[i * nCnt + j] = m_anCliff[i, j];
                                            //    }
                                            //DisplayCliff();

                                            //rbQwmCliff_Dispaly(false, m_nCliffDir_Manual);
                                            //rbQwmCliff_Dispaly(true, m_nCliffDir_Auto);
                                        }
                                        break;
                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\r\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";

                                                ////SendCommand(_MOTION, 0x40 + 42);
                                                //// 인증이 성공 되면 QWM 파라미터를 가져온다.
                                                ////if (tabPageQwm3.Focused == true)
                                                ////{
                                                //if (chkNoGetData_Qwm3.Checked == false)
                                                //{
                                                //    if (chkOldQwmParam.Checked == true) SendCmd_Etc(2); // (tabPageQwm3)Get Qwm3-Param
                                                //    else SendCmd_Etc(4);

                                                //    OjwTimer.WaitTimer(100);
                                                //    // 절벽감지의 상태를 업로드
                                                //    SendCommand(_MOTION, 0x40 + 47);
                                                //}
                                                ////}
                                                ////tbQwmWeight.Enabled = true;
                                            }
                                        }
                                        break;
                                    case 0x82:
                                        {
                                            ////strMessage1 += "[Evd 8 Get]\r\n";
                                            ////nID = byteArrayData[0];
                                            ////strTmp = CConvert.IntToStr(byteArrayData[1]);
                                            ////if (nID == LEFT_FRONT_DOWN_ID) txtLf8Dn.Text = strTmp;
                                            ////else if (nID == LEFT_FRONT_UP_ID) txtLf8Up.Text = strTmp;
                                            ////else if (nID == LEFT_FRONT_WING_ID) txtLf8W.Text = strTmp;
                                            ////else if (nID == LEFT_REAR_DOWN_ID) txtLr8Dn.Text = strTmp;
                                            ////else if (nID == LEFT_REAR_UP_ID) txtLr8Up.Text = strTmp;
                                            ////else if (nID == LEFT_REAR_WING_ID) txtLr8W.Text = strTmp;

                                            ////else if (nID == RIGHT_FRONT_DOWN_ID) txtRf8Dn.Text = strTmp;
                                            ////else if (nID == RIGHT_FRONT_UP_ID) txtRf8Up.Text = strTmp;
                                            ////else if (nID == RIGHT_FRONT_WING_ID) txtRf8W.Text = strTmp;
                                            ////else if (nID == RIGHT_REAR_DOWN_ID) txtRr8Dn.Text = strTmp;
                                            ////else if (nID == RIGHT_REAR_UP_ID) txtRr8Up.Text = strTmp;
                                            ////else if (nID == RIGHT_REAR_WING_ID) txtRr8W.Text = strTmp;

                                            ////else if (nID == HEAD_PAN_ID) txtPan8.Text = strTmp;
                                            ////else if (nID == HEAD_TILT_ID) txtTilt8.Text = strTmp;
                                            ////else if (nID == HIPS_TAIL_ID) txtTail8.Text = strTmp;
                                        }
                                        break;
                                    case 0x84:
                                        {
                                            //strMessage1 += "[Evd 10 Get]\r\n";
                                            //nID = byteArrayData[0];
                                            //strTmp = CConvert.IntToStr(byteArrayData[1] * 256 + byteArrayData[2]);
                                            //if (nID == LEFT_FRONT_DOWN_ID) txtLfDn.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_UP_ID) txtLfUp.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_WING_ID) txtLfW.Text = strTmp;
                                            //else if (nID == LEFT_REAR_DOWN_ID) txtLrDn.Text = strTmp;
                                            //else if (nID == LEFT_REAR_UP_ID) txtLrUp.Text = strTmp;
                                            //else if (nID == LEFT_REAR_WING_ID) txtLrW.Text = strTmp;

                                            //else if (nID == RIGHT_FRONT_DOWN_ID) txtRfDn.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_UP_ID) txtRfUp.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_WING_ID) txtRfW.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_DOWN_ID) txtRrDn.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_UP_ID) txtRrUp.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_WING_ID) txtRrW.Text = strTmp;

                                            //else if (nID == HEAD_PAN_ID) txtPan.Text = strTmp;
                                            //else if (nID == HEAD_TILT_ID) txtTilt.Text = strTmp;
                                            //else if (nID == HIPS_TAIL_ID) txtTail.Text = strTmp;
                                        }
                                        break;
                                    case 0x85:
                                        {
                                            //strMessage1 += "[Status Get]\r\n";
                                            //nID = byteArrayData[0];
                                            //strTmp = CConvert.IntToBinary(byteArrayData[1]);
                                            //if (nID == LEFT_FRONT_DOWN_ID) lbLfDn_Status.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_UP_ID) lbLfUp_Status.Text = strTmp;
                                            //else if (nID == LEFT_FRONT_WING_ID) lbLfW_Status.Text = strTmp;
                                            //else if (nID == LEFT_REAR_DOWN_ID) lbLrDn_Status.Text = strTmp;
                                            //else if (nID == LEFT_REAR_UP_ID) lbLrUp_Status.Text = strTmp;
                                            //else if (nID == LEFT_REAR_WING_ID) lbLrW_Status.Text = strTmp;

                                            //else if (nID == RIGHT_FRONT_DOWN_ID) lbRfDn_Status.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_UP_ID) lbRfUp_Status.Text = strTmp;
                                            //else if (nID == RIGHT_FRONT_WING_ID) lbRfW_Status.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_DOWN_ID) lbRrDn_Status.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_UP_ID) lbRrUp_Status.Text = strTmp;
                                            //else if (nID == RIGHT_REAR_WING_ID) lbRrW_Status.Text = strTmp;

                                            //else if (nID == HEAD_PAN_ID) lbPan_Status.Text = strTmp;
                                            //else if (nID == HEAD_TILT_ID) lbTilt_Status.Text = strTmp;
                                            //else if (nID == HIPS_TAIL_ID) lbTail_Status.Text = strTmp;

                                        }
                                        break;
                                    case 0x94:
                                        {
                                            //strMessage1 += "[Gain Get]\r\n";
                                            ////lbGainID.Text = "ID=" + CConvert.IntToHex(byteArrayData[0]);
                                            //txtKp.Text = CConvert.IntToStr(byteArrayData[1] * 256 + byteArrayData[2]);
                                            //txtKi.Text = CConvert.IntToStr(byteArrayData[3] * 256 + byteArrayData[4]);
                                            //txtKd.Text = CConvert.IntToStr(byteArrayData[5] * 256 + byteArrayData[6]);
                                            ////txtGrad.Text = CConvert.IntToStr(byteArrayData[7]);
                                        }
                                        break;
                                    case 0xA2:
                                        {
                                            strMessage1 += "[Evd 8 Data Get]\r\n";
                                            //// 12 - Temp, 13 - Yaw, 14 - Mouth, 15~17 - Pan, Tilt, Tail
                                            ////i = 0;
                                            ////txtLf8W.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_WING_ID]));
                                            ////txtLf8Dn.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_DOWN_ID]));
                                            ////txtLf8Up.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_UP_ID]));

                                            ////txtLr8W.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_WING_ID]));
                                            ////txtLr8Dn.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_DOWN_ID]));
                                            ////txtLr8Up.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_UP_ID]));

                                            ////txtRf8W.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_WING_ID]));
                                            ////txtRf8Dn.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_DOWN_ID]));
                                            ////txtRf8Up.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_UP_ID]));

                                            ////txtRr8W.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_WING_ID]));
                                            ////txtRr8Dn.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_DOWN_ID]));
                                            ////txtRr8Up.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_UP_ID]));

                                            ////txtPan8.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_PAN_ID]));
                                            ////txtTilt8.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_TILT_ID]));
                                            ////txtTail8.Text = CConvert.IntToStr((int)(byteArrayData[HIPS_TAIL_ID]));

                                            ////txtYaw8.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_YAW_ID]));
                                            ////txtMouth8.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_MOUTH_ID]));

                                        }
                                        break;
                                    case 0xA4:
                                        {
                                            strMessage1 += "[Evd 10 Data Get]\r\n";

                                            ////if ((dataGrid_XY.Focused == true) || (dataGrid_Angle.Focused == true) || (dataGrid_Evd.Focused == true))
                                            //if (m_bGetCurrentPosition == true)
                                            //{
                                            //    m_bGetCurrentPosition = false;
                                            //    //////// Data /////////////////////////////////////////////////////////////////////////////////////////////
                                            //    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                                            //    //// Data                                            
                                            //    //int nTmp = sData;
                                            //    int j = 0;
                                            //    if (m_nPage == DISP_XY) j = dataGrid_XY.CurrentCell.RowIndex;
                                            //    else if (m_nPage == DISP_ANGLE) j = dataGrid_Angle.CurrentCell.RowIndex;
                                            //    else j = dataGrid_Evd.CurrentCell.RowIndex;
                                            //    DataClear(DISP_EVD, j);
                                            //    //Index
                                            //    m_rowData_Evd[j]["Index"] = j;
                                            //    // En
                                            //    //if (j < nTmp)
                                            //    m_rowData_Evd[j]["En"] = 1;
                                            //    // Action Data
                                            //    //(x, y, w) * 4 + tail + pan + tilt + yaw + mouth + timer + delay + sound + emoticon
                                            //    string[] pstrData2 = new string[4] { "Lf", "Rf", "Lr", "Rr" };
                                            //    string strTmp2;
                                            //    for (int k = 0; k < 4; k++)
                                            //    {
                                            //        strTmp2 = pstrData2[k] + "W"; m_rowData_Evd[j][strTmp2] = (short)(byteArrayData[(k * 3 + LEFT_FRONT_WING_ID) * 2] * 256 + byteArrayData[(k * 3 + LEFT_FRONT_WING_ID) * 2 + 1]);
                                            //        strTmp2 = pstrData2[k] + "Dn"; m_rowData_Evd[j][strTmp2] = (short)(byteArrayData[(k * 3 + LEFT_FRONT_DOWN_ID) * 2] * 256 + byteArrayData[(k * 3 + LEFT_FRONT_DOWN_ID) * 2 + 1]);
                                            //        strTmp2 = pstrData2[k] + "Up"; m_rowData_Evd[j][strTmp2] = (short)(byteArrayData[(k * 3 + LEFT_FRONT_UP_ID) * 2] * 256 + byteArrayData[(k * 3 + LEFT_FRONT_UP_ID) * 2 + 1]);
                                            //    }

                                            //    // Tail
                                            //    m_rowData_Evd[j]["Tail"] = (short)(byteArrayData[HIPS_TAIL_ID * 2] * 256 + byteArrayData[HIPS_TAIL_ID * 2 + 1]);
                                            //    // Pan
                                            //    m_rowData_Evd[j]["Pan"] = (short)(byteArrayData[HEAD_PAN_ID * 2] * 256 + byteArrayData[HEAD_PAN_ID * 2 + 1]);
                                            //    // Tilt                                                
                                            //    m_rowData_Evd[j]["Tilt"] = (short)(byteArrayData[HEAD_TILT_ID * 2] * 256 + byteArrayData[HEAD_TILT_ID * 2 + 1]);

                                            //    //Data0
                                            //    m_rowData_Evd[j]["Data0"] = m_rowData_XY[j]["Data0"];
                                            //    m_rowData_Evd[j]["Data1"] = m_rowData_XY[j]["Data1"];
                                            //    TableMove_Evd2Angle(j);
                                            //    TableMove_Evd2XY(j);
                                            //}
                                            //else
                                            //{
                                            //    // 12 - Temp, 13 - Yaw, 14 - Mouth, 15~17 - Pan, Tilt, Tail
                                            //    i = 0;
                                            //    txtLfW.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_WING_ID * 2] * 256 + byteArrayData[LEFT_FRONT_WING_ID * 2 + 1]));
                                            //    txtLfDn.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_DOWN_ID * 2] * 256 + byteArrayData[LEFT_FRONT_DOWN_ID * 2 + 1]));
                                            //    txtLfUp.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_FRONT_UP_ID * 2] * 256 + byteArrayData[LEFT_FRONT_UP_ID * 2 + 1]));

                                            //    txtLrW.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_WING_ID * 2] * 256 + byteArrayData[LEFT_REAR_WING_ID * 2 + 1]));
                                            //    txtLrDn.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_DOWN_ID * 2] * 256 + byteArrayData[LEFT_REAR_DOWN_ID * 2 + 1]));
                                            //    txtLrUp.Text = CConvert.IntToStr((int)(byteArrayData[LEFT_REAR_UP_ID * 2] * 256 + byteArrayData[LEFT_REAR_UP_ID * 2 + 1]));

                                            //    txtRfW.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_WING_ID * 2] * 256 + byteArrayData[RIGHT_FRONT_WING_ID * 2 + 1]));
                                            //    txtRfDn.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_DOWN_ID * 2] * 256 + byteArrayData[RIGHT_FRONT_DOWN_ID * 2 + 1]));
                                            //    txtRfUp.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_FRONT_UP_ID * 2] * 256 + byteArrayData[RIGHT_FRONT_UP_ID * 2 + 1]));

                                            //    txtRrW.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_WING_ID * 2] * 256 + byteArrayData[RIGHT_REAR_WING_ID * 2 + 1]));
                                            //    txtRrDn.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_DOWN_ID * 2] * 256 + byteArrayData[RIGHT_REAR_DOWN_ID * 2 + 1]));
                                            //    txtRrUp.Text = CConvert.IntToStr((int)(byteArrayData[RIGHT_REAR_UP_ID * 2] * 256 + byteArrayData[RIGHT_REAR_UP_ID * 2 + 1]));

                                            //    txtPan.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_PAN_ID * 2] * 256 + byteArrayData[HEAD_PAN_ID * 2 + 1]));
                                            //    txtTilt.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_TILT_ID * 2] * 256 + byteArrayData[HEAD_TILT_ID * 2 + 1]));
                                            //    txtTail.Text = CConvert.IntToStr((int)(byteArrayData[HIPS_TAIL_ID * 2] * 256 + byteArrayData[HIPS_TAIL_ID * 2 + 1]));

                                            //    txtTiltUp.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_TILTUP_ID * 2] * 256 + byteArrayData[HEAD_TILTUP_ID * 2 + 1]));
                                            //    txtMouth.Text = CConvert.IntToStr((int)(byteArrayData[HEAD_MOUTH_ID * 2] * 256 + byteArrayData[HEAD_MOUTH_ID * 2 + 1]));

                                            //}
                                            //if (m_bDisp == true) Evd_Display();
                                            //m_bDisp = false;
                                        }
                                        break;
                                    case 0xA5:
                                        {
                                            strMessage1 += "[All Status Get]\r\n";
                                            //// 12 - Temp, 13 - Yaw, 14 - Mouth, 15~17 - Pan, Tilt, Tail
                                            //i = 0;
                                            //lbLfW_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbLfDn_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbLfUp_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));

                                            //lbLrW_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbLrDn_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbLrUp_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));

                                            //lbRfW_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbRfDn_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbRfUp_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));

                                            //lbRrW_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbRrDn_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbRrUp_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //i += 3;
                                            //lbPan_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbTilt_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));
                                            //lbTail_Status.Text = "0x" + CConvert.IntToHex((int)(byteArrayData[i++]));

                                            //lbTiltUp_Status.Text = "";//"0x" + CConvert.IntToHex((int)(byteArrayData[HEAD_TILTUP_ID]));
                                            //lbMouth_Status.Text = "";//"0x" + CConvert.IntToHex((int)(byteArrayData[HEAD_MOUTH_ID]));
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            strMessage1 += "[Version Get]\r\n";

                                            strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            CMessage.Write(strTmp);
                                        }
                                        break;
                                    case 0xE2:
                                        strData = "    =>(Data[0]=0x" + CConvert.IntToHex(byteArrayData[0]) + ")\r\n" +
                                                  "    =>(Data[1]=0x" + CConvert.IntToHex(byteArrayData[1]) + ")\r\n" +
                                                  "    =>(Data[2]=0x" + CConvert.IntToHex(byteArrayData[2]) + ")\r\n" +
                                                  "    =>(Data[3]=0x" + CConvert.IntToHex(byteArrayData[3]) + ")\r\n";
                                        strMessage1 += "[Reserve]\r\n" + strData;
                                        break;
                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                CMessage.Write(strMessage1);
                            }
                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }
            //private void FileTranse()
            //{
            //    if (OjwSock[_MOTION].m_bConnect)
            //    {
            //        openFileDialog1.Multiselect = true;
            //        openFileDialog1.FileName = null;
            //        if (openFileDialog1.ShowDialog() == DialogResult.OK)
            //        {
            //            progressFileConvert.Minimum = 0;
            //            progressFileConvert.Maximum = openFileDialog1.FileNames.Length;
            //            progressFileConvert.Value = 0;
            //            //Ojw.CMessage.Write(OjwConvert.IntToStr(openFileDialog1.FileNames.Length));
            //            int i = 0;
            //            int nFileNum = 0;
            //            bool bGetList = false;
            //            string strFileName = "";
            //            string fileName2 = "";
            //            foreach (string fileName in openFileDialog1.FileNames)
            //            {
            //                fileName2 = fileName;
            //                CMessage.Write("openFileDialog1.FileNames.Length={0}", openFileDialog1.FileNames.Length.ToString());
            //                CMessage.Write("openFileDialog1.FileNames.Rank=", openFileDialog1.FileNames.Rank.ToString());
            //                //if (i > 10) break; // 에러가 10개 이상이면 종요하도록...
            //                FileInfo f = new FileInfo(fileName);
            //                strFileName = f.Name;
            //                string strFileExt = f.Extension;
            //                if (strFileExt == ".act")
            //                {
            //                    strFileName = strFileName.Substring(0, strFileName.Length - 4);
            //                    int nFileNameSize = C_TCP_FILENAME_SIZE;
            //                    if (strFileName.Length < nFileNameSize)
            //                    {
            //                        nFileNum++;
            //                        int nType = ((rbMaster_FileDownLoad.Checked == true) ? 1 : ((rbUser_FileDownLoad.Checked == true) ? 0 : 2));
            //                        if ((nType == 2) && (nFileNum >= openFileDialog1.FileNames.Length))
            //                        {
            //                            Ojw.CMessage.Write("End");
            //                            nType = 3;
            //                            bGetList = true;
            //                        }
            //                        OjwFileTranse(nType, strFileName, fileName);

            //                        m_bReceiveOk = false;
            //                        OjwTimer.TimerSet(0);
            //                        while (m_bReceiveOk != true)
            //                        {
            //                            Application.DoEvents();
            //                            if (OjwTimer.Timer(0) > 5000)
            //                            {
            //                                i++;
            //                                break;
            //                            }
            //                        }
            //                        OjwTimer.WaitTimer(1);
            //                    }
            //                    else
            //                    {
            //                        nFileNum++;
            //                        i++;
            //                    }
            //                }
            //                else i++;

            //                progressFileConvert.Value++;
            //                Application.DoEvents();
            //            }
            //            if (bGetList == false) OjwFileTranse(3, strFileName, fileName2);
            //            // 로드 실패가 한번이라도 있으면 표시
            //            if (i > 0)
            //                CMessage.Write_Error("데이타 로드 실패횟수 = " + OjwConvert.IntToStr(i));
            //        }
            //        openFileDialog1.Multiselect = false;
            //        progressFileConvert.Value = 0;
            //    }
            //}
#if true
            public void Receive_Sensor()
            {
                int nType = _SENSOR;
                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    byte byteCheckSum = 0;
                    //string strMessage0;
                    string strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;
                            // CheckSum Data Get
                            byteCheckSum = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;

                            string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                            int i;
                            string[] pstrData;

                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1) strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process Fail]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "\r\n";
                                //CMessage.Write(strMessage1);
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                //if (chkDispHearBeat.Checked)
                                //{
                                //    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]\r\n";
                                //    //CMessage.Write(strMessage1);
                                //}
                                //else continue;
                            }
                            else
                            {
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                switch (nCmd)
                                {
                                    case 0x01:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[Sensor 상태확인]\r\n";
                                            byte bytePsd = byteArrayData[nPos++];
#if false
                                        byte [] pbyteTmp = new byte[4];
                                        for (int jj = 0; jj < 4; jj++) pbyteTmp[jj] = byteArrayData[nPos + 4 - 1 - jj];
                                        float fTiltX = BitConverter.ToSingle(pbyteTmp, 0); nPos += 4;
                                        for (int jj = 0; jj < 4; jj++) pbyteTmp[jj] = byteArrayData[nPos + 4 - 1 - jj];
                                        float fTiltY = BitConverter.ToSingle(pbyteTmp, 0); nPos += 4;
                                        pbyteTmp = null;
#else
                                            //                                          float fTiltX = BitConverter.ToSingle(byteArrayData, nPos); nPos += 4;
                                            //                                         fTiltX = (float)IPAddress.NetworkToHostOrder(fTiltX);
                                            // 
                                            //                                         //for (int jj = 0; jj < 4; jj++) pbyteTmp[jj] = byteArrayData[nPos + 4 - 1 - jj];
                                            //                                         float fTiltY = BitConverter.ToSingle(byteArrayData, nPos); nPos += 4;
                                            //                                         fTiltY = (float)IPAddress.NetworkToHostOrder(fTiltY);

                                            //byte [] pbyteTmp = new byte[4];
                                            //for (int jj = 0; jj < 4; jj++) pbyteTmp[jj] = byteArrayData[nPos + 4 - 1 - jj];
                                            Array.Reverse(byteArrayData, nPos, 4);
                                            float fTiltX = BitConverter.ToSingle(byteArrayData, nPos); nPos += 4;
                                            //for (int jj = 0; jj < 4; jj++) pbyteTmp[jj] = byteArrayData[nPos + 4 - 1 - jj];
                                            Array.Reverse(byteArrayData, nPos, 4);
                                            float fTiltY = BitConverter.ToSingle(byteArrayData, nPos); nPos += 4;
                                            //pbyteTmp = null;
#endif
                                            //ushort usTouch = IPAddress.NetworkToHostOrder(BitConverter.ToUInt16(byteArrayData, nPos)); nPos += 2;
                                            ushort usTouch = (ushort)((byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]) & 0xffff); nPos += 2;

                                            byte bytePow = byteArrayData[nPos++];
                                            byte byteImpact = byteArrayData[nPos++];
                                            //ushort usSoundLocal = BitConverter.ToUInt16(byteArrayData, nPos); nPos += 2;
                                            byte bytePir = byteArrayData[nPos++];
                                            byte byteBatteryLevel = byteArrayData[nPos++];
                                            
                                            //txtPsd.Text = CConvert.IntToStr(bytePsd);
                                            //txtTiltX.Text = Convert.ToString(fTiltX);
                                            //txtTiltY.Text = Convert.ToString(fTiltY);
                                            //chkSensorJaw.Text = CConvert.IntToBinary(usTouch >> 8 & 0xff) + " " + CConvert.IntToBinary(usTouch & 0xff);
                                            //int ii = 0;
                                            //chkSensorH.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);

                                            //chkSensorB1.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            //chkSensorB2.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            //chkSensorB3.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            //chkSensorB4.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);

                                            //chkSensorL.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            //chkSensorR.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);

                                            //chkSensorLS.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            //chkSensorRS.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);

                                            //chkSensorJaw.Checked = ((((usTouch >> ii++) & 0x01) > 0) ? true : false);
                                            ///////////////////////////////////////////////

                                            //chkSensorRR.Checked = ((((bytePow >> 3) & 0x01) > 0) ? false : true);
                                            //chkSensorLR.Checked = ((((bytePow >> 2) & 0x01) > 0) ? false : true);
                                            //chkSensorRF.Checked = ((((bytePow >> 1) & 0x01) > 0) ? false : true);
                                            //chkSensorLF.Checked = ((((bytePow >> 0) & 0x01) > 0) ? false : true);
#if false
                                        chkSensorR.Checked = ((((byteTouch >> 6) & 0x01) > 0) ? true : false);
                                        chkSensorL.Checked = ((((byteTouch >> 5) & 0x01) > 0) ? true : false);
                                        chkSensorB4.Checked = ((((byteTouch >> 4) & 0x01) > 0) ? true : false);
                                        chkSensorB3.Checked = ((((byteTouch >> 3) & 0x01) > 0) ? true : false);
                                        chkSensorB2.Checked = ((((byteTouch >> 2) & 0x01) > 0) ? true : false);
                                        chkSensorB1.Checked = ((((byteTouch >> 1) & 0x01) > 0) ? true : false);
                                        chkSensorH.Checked = ((((byteTouch >> 0) & 0x01) > 0) ? true : false);

                                        chkSensorRR.Checked = ((((bytePow >> 3) & 0x01) > 0) ? false : true);
                                        chkSensorLR.Checked = ((((bytePow >> 2) & 0x01) > 0) ? false : true);
                                        chkSensorRF.Checked = ((((bytePow >> 1) & 0x01) > 0) ? false : true);
                                        chkSensorLF.Checked = ((((bytePow >> 0) & 0x01) > 0) ? false : true);
#endif
                                            //txtPir.Text = CConvert.IntToStr(bytePir);
                                            ////txtSoundLocal.Text = CConvert.IntToStr(usSoundLocal);
                                            //txtBattery.Text = CConvert.IntToStr(byteBatteryLevel);
                                        }
                                        break;

                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\r\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";
                                            }
                                        }
                                        break;

                                    case 0xE2:
                                        strData = "    =>(Data[0]=0x" + CConvert.IntToHex(byteArrayData[0]) + ")\r\n" +
                                                  "    =>(Data[1]=0x" + CConvert.IntToHex(byteArrayData[1]) + ")\r\n" +
                                                  "    =>(Data[2]=0x" + CConvert.IntToHex(byteArrayData[2]) + ")\r\n" +
                                                  "    =>(Data[3]=0x" + CConvert.IntToHex(byteArrayData[3]) + ")\r\n";
                                        strMessage1 += "[Reserve]\r\n" + strData;
                                        break;

                                    case 0x81:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[Sensor 상태확인]\r\n";
                                            int nSensorStatus_L = byteArrayData[nPos++];
                                            int nSensorStatus_H = byteArrayData[nPos++];//byteArrayData[nPos++] & 0x0f;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nTiltX = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nTiltY = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nTiltZ = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nGyroX = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nGyroY = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            Array.Reverse(byteArrayData, nPos, 2);
                                            int nGyroZ = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(byteArrayData, nPos)); nPos += 2;//byteArrayData[nPos] * 256 + byteArrayData[nPos + 1]; nPos += 2;
                                            //int nTiltX = byteArrayData[nPos++];
                                            //int nTiltY = byteArrayData[nPos++];
                                            int nBatteryLevel = byteArrayData[nPos++];
                                            int nPsd = byteArrayData[nPos++];
                                            int nImpact = byteArrayData[nPos++];
                                            int nReserve = byteArrayData[nPos++];

                                            int nPowerDrv = (nSensorStatus_H >> 2) & 0x03;
                                            int nStatus_Pir = (nSensorStatus_H >> 1) & 0x01;
                                            int nStatus_BatteryLevel = (nSensorStatus_H & 0x01);

                                            int nStatus_TiltSW = (nSensorStatus_H >> 4) & 0x01;
                                            int nStatus_Obstacle = (nSensorStatus_H >> 5) & 0x03;

                                            int nStatus_Battery = (nSensorStatus_L >> 6) & 0x03;
                                            int nStatus_Psd = ((nSensorStatus_L >> 5) & 0x01);
                                            int nStatus_SoundLoc = ((nSensorStatus_L >> 4) & 0x01);
                                            int nStatus_TiltY = ((nSensorStatus_L >> 3) & 0x01);
                                            int nStatus_TiltX = ((nSensorStatus_L >> 2) & 0x01);
                                            int nStatus_AddaptorIn = ((nSensorStatus_L >> 1) & 0x01);
                                            int nStatus_StopBtn = ((nSensorStatus_L >> 0) & 0x01);

                                            //txtObstacle.Text = CConvert.IntToStr(nStatus_Obstacle);
                                            //txtTiltSW.Text = CConvert.IntToStr(nStatus_TiltSW);
                                            //txtResult_PowerDrv.Text = CConvert.IntToStr(nPowerDrv);
                                            //txtResult_Pir.Text = CConvert.IntToStr(nStatus_Pir);
                                            //txtResult_BatterySt.Text = CConvert.IntToStr(nStatus_Battery);
                                            //txtResult_BatteryLv.Text = CConvert.IntToStr(nStatus_BatteryLevel);
                                            //txtResult_Psd.Text = CConvert.IntToStr(nStatus_Psd);
                                            //txtResult_SoundLoc.Text = CConvert.IntToStr(nStatus_SoundLoc);
                                            //txtResult_TiltY.Text = CConvert.IntToStr(nStatus_TiltY);
                                            //txtResult_TiltX.Text = CConvert.IntToStr(nStatus_TiltX);
                                            //txtResult_AdaptorIn.Text = CConvert.IntToStr(nStatus_AddaptorIn);
                                            //txtResult_StopBtn.Text = CConvert.IntToStr(nStatus_StopBtn);

                                            //txtResult_Value_TiltX.Text = CConvert.FloatToStr(nTiltX);
                                            //txtResult_Value_TiltY.Text = CConvert.FloatToStr(nTiltY);
                                            //txtResult_Value_TiltZ.Text = CConvert.FloatToStr(nTiltZ);
                                            //txtResult_Value_GyroX.Text = CConvert.FloatToStr(nGyroX);
                                            //txtResult_Value_GyroY.Text = CConvert.FloatToStr(nGyroY);
                                            //txtResult_Value_GyroZ.Text = CConvert.FloatToStr(nGyroZ);
                                            //txtResult_Value_BatteryLv.Text = CConvert.IntToStr(nBatteryLevel);
                                            //txtResult_Value_Psd.Text = CConvert.IntToStr(nPsd);
                                            //txtResult_Value_SoundLoc.Text = (nImpact > 0) ? "충격발생(" + CConvert.IntToStr(nImpact) + ")" : "충격없음(0)";//CConvert.IntToStr(usSoundLocal);
                                        }
                                        break;
                                    case 0x91:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[Rtc Status 확인]\r\n";
                                            int nWakeUp = ((byteArrayData[nPos] >> 1) & 0x01);
                                            int nSleep = ((byteArrayData[nPos++] >> 0) & 0x01);

                                            int nYear = byteArrayData[nPos++];
                                            int nMonth = byteArrayData[nPos++];
                                            int nDate = byteArrayData[nPos++];
                                            int nDay = byteArrayData[nPos++];
                                            int nHour = byteArrayData[nPos++];
                                            int nMin = byteArrayData[nPos++];
                                            int nSec = byteArrayData[nPos++];

                                            //txtResult_WakeUp.Text = CConvert.IntToStr(nWakeUp);
                                            //txtResult_Sleep.Text = CConvert.IntToStr(nSleep);

                                            //txtYear.Text = CConvert.IntToStr(nYear);
                                            //txtMonth.Text = CConvert.IntToStr(nMonth);
                                            //txtDate.Text = CConvert.IntToStr(nDate);
                                            //txtDay.Text = CConvert.IntToStr(nDay);
                                            //txtHour.Text = CConvert.IntToStr(nHour);
                                            //txtMin.Text = CConvert.IntToStr(nMin);
                                            //txtSecond.Text = CConvert.IntToStr(nSec);
                                        }
                                        break;
                                    case 0x92:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[WakeUp Alarm 응답]\r\n";
                                            int nMonth = byteArrayData[nPos++];
                                            int nDate = byteArrayData[nPos++];
                                            int nHour = byteArrayData[nPos++];
                                            int nMinute = byteArrayData[nPos++];

                                            //txtWakeUpAlarm_Month.Text = CConvert.IntToStr(nMonth);
                                            //txtWakeUpAlarm_Date.Text = CConvert.IntToStr(nDate);
                                            //txtWakeUpAlarm_Hour.Text = CConvert.IntToStr(nHour);
                                            //txtWakeUpAlarm_Minute.Text = CConvert.IntToStr(nMinute);
                                        }
                                        break;
                                    case 0x93:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[Sleep Alarm 응답]\r\n";
                                            int nMonth = byteArrayData[nPos++];
                                            int nDate = byteArrayData[nPos++];
                                            int nHour = byteArrayData[nPos++];
                                            int nMinute = byteArrayData[nPos++];

                                            //txtSleepAlarm_Month.Text = CConvert.IntToStr(nMonth);
                                            //txtSleepAlarm_Date.Text = CConvert.IntToStr(nDate);
                                            //txtSleepAlarm_Hour.Text = CConvert.IntToStr(nHour);
                                            //txtSleepAlarm_Minute.Text = CConvert.IntToStr(nMinute);
                                        }
                                        break;
                                    case 0xA1:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[리모콘 정보 응답]\r\n";
                                            int nData1 = byteArrayData[nPos++];
                                            int nData2 = byteArrayData[nPos++];

                                            //txtResult_Ir_Tx.Text = CConvert.IntToStr((nData1 >> 0) & 0x03);
                                            //txtResult_Ir_Rx.Text = CConvert.IntToStr((nData1 >> 2) & 0x03);

                                            //txtResult_Value_Ir_Rx.Text = CConvert.IntToStr(nData2);
                                        }
                                        break;
                                    case 0xA3:
                                        {
                                            int nPos = 0;
                                            strMessage1 += "[현재 세팅된 리모콘 제품 Key 응답]\r\n";
                                            int nData1 = byteArrayData[nPos++];

                                            //txtGetKey.Text = CConvert.IntToStr(nData1);
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            strMessage1 += "[Version Get]\r\n";

                                            strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            //lbVersion_Sensor.Text = strTmp;
                                        }
                                        break;
                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                //CMessage.Write(strMessage1);

                            }

                            //if (chkDisplayPacket.Checked == true)
                            //{
                            //    strTmp = "";
                            //    for (i = 0; i < nLength; i++)
                            //        strTmp += "(0x" + CConvert.IntToHex(byteArrayData[i]) + ")";
                            //    strMessage0 = "-------------------------\r\n<=" + m_strType[nType];
                            //    strMessage0 += "(0x" + CConvert.IntToHex(byteData) + ")";
                            //    strMessage0 += "(0x" + CConvert.IntToHex(nCmd) + ")";
                            //    strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 24) & 0xff) + ")";
                            //    strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 16) & 0xff) + ")";
                            //    strMessage0 += "(0x" + CConvert.IntToHex((nLength >> 8) & 0xff) + ")";
                            //    strMessage0 += "(0x" + CConvert.IntToHex(nLength & 0xff) + ")";
                            //    strMessage0 += strTmp + "\r\n";

                            //    CMessage.Write(strMessage0);
                            //}
                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }

            public void Receive_Emoticon()
            {
                int nType = _EMOTICON;

                string strName = m_strType[nType];
                try
                {
                    int nCmd;
                    int nID;
                    int nLength = 0;
                    byte byteData;
                    byte byteCheckSum = 0;
                    //string strMessage0;
                    string strMessage1;
                    String strTmp;
                    while (OjwSock[nType].m_bConnect)
                    {
                        byteData = (Byte)OjwSock[nType].GetByte();
                        if (!OjwSock[nType].m_bConnect) return;
                        strTmp = CConvert.IntToHex(byteData);

                        // Header 검색
                        if (byteData == 0xff)
                        {
                            // ID
                            nID = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Command
                            nCmd = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;
                            // Data Length
                            nLength = OjwSock[nType].GetInt32(); if (!OjwSock[nType].m_bConnect) return;
                            // 실제 데이타 Get
                            byte[] byteArrayData = OjwSock[nType].GetBytes(nLength); if (!OjwSock[nType].m_bConnect) return;
                            // CheckSum Data Get
                            byteCheckSum = OjwSock[nType].GetByte(); if (!OjwSock[nType].m_bConnect) return;

                            string strData = System.Text.Encoding.Default.GetString(byteArrayData);

                            int i;
                            string[] pstrData;

                            if (nCmd == 0xf0)
                            {
                                if (byteArrayData.Rank >= 1)
                                {
                                    if ((byte)(byteArrayData[1]) == 0) strTmp = "[Receive OK]";
                                    else if ((byte)(byteArrayData[1]) == 1) strTmp = "[Process End]";
                                    else if ((byte)(byteArrayData[1]) == 2) strTmp = "[Process Fail]";
                                    else if ((byte)(byteArrayData[1]) == 3) strTmp = "[Check Sum Error]";
                                    else if ((byte)(byteArrayData[1]) == 4) strTmp = "[Time Out Error]";
                                }
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]" + strTmp + "\r\n";
                                CMessage.Write(strMessage1);
                            }
                            else if (nCmd == 0xf1) // HeartBeat
                            {
                                //if (chkDispHearBeat.Checked)
                                //{
                                //    strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "][HeartBeat]\r\n";
                                //    CMessage.Write(strMessage1);
                                //}
                                //else continue;
                            }
                            else
                            {
                                strMessage1 = "<=[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                switch (nCmd)
                                {
                                    case 0x01:
                                        {
                                            //listviewGenibo_Emoticon.Clear();
                                            //strMessage1 += "[Emoticon 목록확인]\r\n";
                                            //int nListCount = byteArrayData[0] * 256 + byteArrayData[1];
                                            //int nIndex;
                                            //m_nMotionListCode_Emoticon = new int[nListCount];
                                            //m_nMotionListCount_Emoticon = nListCount;
                                            //for (i = 0; i < nListCount; i++)
                                            //{
                                            //    nIndex = byteArrayData[2 + i * 23] * 256 + byteArrayData[2 + i * 23 + 1];
                                            //    strTmp = System.Text.Encoding.Default.GetString(byteArrayData, 2 + i * 23 + 2, 21);
                                            //    listviewGenibo_Emoticon.Items.Add(CConvert.IntToStr(nIndex), strTmp.Trim('\0'), ((nIndex <= 10000) ? 0 : 1));
                                            //    m_nMotionListCode_Emoticon[i] = nIndex;
                                            //}
                                        }
                                        break;
                                    case 0x03:
#if false
                                    try
                                    {
                                        strMessage1 += "[Emoticon 업로드]\r\n";
                                        short sData;
                                        i = 0;
                                        int j;
                                        // Index
                                        //sData = BitConverter.ToInt16(byteArrayData, i); i += 2;
                                        sData = (short)(byteArrayData[i++] * 256);
                                        sData += byteArrayData[i++];
                                        int nIndex = (int)sData;
                                        strMessage1 += "[Index=" + CConvert.IntToStr(nIndex) + "]";
                                        txtEmoticonIndex.Text = CConvert.IntToStr(nIndex);

                                        // Name
                                        txtEmoticonName.Text = System.Text.Encoding.Default.GetString(byteArrayData, i, 20);
                                        //txtTableName.Text = System.Text.Encoding.Default.GetString(byteArrayTitle, 0, 20);
                                        i += 21;

                                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

                                        //// Data
                                        for (j = 0; j < 8; j++)
                                        {
                                            // Left
                                            SetEmoticon(_L_, j, byteArrayData[i++]);
                                        }
                                        for (j = 0; j < 8; j++)
                                        {
                                            // Right
                                            SetEmoticon(_R_, j, byteArrayData[i++]);
                                        }
                                    }
                                    catch
                                    {
                                        CMessage.Write_Error("업로드 Error\n");
                                    }
#endif
                                        break;

                                    case 0x71:
                                        {
                                            strMessage1 += "[인증]\r\n";
                                            if (byteArrayData[0] == 0)
                                            {
                                                strMessage1 += "=>False";
                                            }
                                            else
                                            {
                                                strMessage1 += "=>OK";
                                            }
                                        }
                                        break;
                                    case 0xD9:
                                        {
                                            strMessage1 += "[Version Get]\r\n";

                                            strTmp = "[ID=" + CConvert.IntToHex(byteArrayData[0]) + "]" + System.Text.Encoding.Default.GetString(byteArrayData, 1, 9);
                                            //lbVersion_Emoticon.Text = strTmp;
                                        }
                                        break;
                                    case 0xE2:
                                        strData = "    =>(Data[0]=0x" + CConvert.IntToHex(byteArrayData[0]) + ")\r\n" +
                                                  "    =>(Data[1]=0x" + CConvert.IntToHex(byteArrayData[1]) + ")\r\n" +
                                                  "    =>(Data[2]=0x" + CConvert.IntToHex(byteArrayData[2]) + ")\r\n" +
                                                  "    =>(Data[3]=0x" + CConvert.IntToHex(byteArrayData[3]) + ")\r\n";
                                        strMessage1 += "[Reserve]\r\n" + strData;
                                        break;
                                    default:
                                        strMessage1 += "테스트 명령[Cmd=" + CConvert.IntToHex(nCmd) + "][ID=" + CConvert.IntToHex(nID) + "]";
                                        pstrData = strData.Split('\0');
                                        i = 0;
                                        foreach (string strItem in pstrData)
                                        {
                                            if ((strItem != "") && (strItem.Length >= 2))
                                            {
                                                i++;
                                                strMessage1 += strItem + "\r\n";
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            strMessage1 += "[Length=" + Convert.ToString(strData.Length) + "]\r\n";
                                        }

                                        break;

                                }
                                CMessage.Write(strMessage1);

                            }

                        }
                    }
                }
                catch
                {
                    CMessage.Write_Error(strName + "데이터를 읽는 과정에서 오류 발생");
                    DisConnect();
                }
            }
#endif
#endif

        }
    }
}
