﻿//#define _ARDUINO_OPENRB150
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using Leap;

#if !_ARDUINO_OPENRB150
namespace OpenJigWare
{
    partial class Ojw
    {
        public class CProtocol2
        {
            public CProtocol2()
            {
                for (int i = 0; i < m_aCParam.Length; i++)
                {
                    m_aCParam[i] = new CParam_t();
                    m_aCParam[i].SetParam(false);
                }
            }
            ~CProtocol2()
            {
                if (IsOpen()) Close();

            }
            //public void SetParam(Ojw.C3d COjw3d)
            //{
            //    m_C3d = COjw3d;
            //    for (int i = 0; i < 256; i++)
            //    {
            //        SetParam(i, ((m_C3d.m_CHeader.pSMotorInfo[i].nMotorDir == 0) ? false : true)
            //            //, m_C3d.m_CHeader.pSMotorInfo[i].fGearRatio
            //            //, false
            //            );
            //    }
            //}
            public void SetParam(SMotorInfo_t[] aSParams)
            {
                for (int i = 0; i < aSParams.Length; i++)
                {
                    SetParam(aSParams[i].nMotorID, ((aSParams[i].nMotorDir == 0) ? false : true), ((aSParams[i].fGearRatio == 0) ? 1.0f : aSParams[i].fGearRatio), ((aSParams[i].nMotor_HightSpec == 0) ? false : true));
                }
            }
            public void SetParam(int nID, bool bDirReverse = false, float fMulti = 1.0f, bool bHigh = false)
            {
                m_aCParam[nID].SetParam(bHigh);
                m_aCParam[nID].m_bDirReverse = bDirReverse;
                m_aCParam[nID].m_fMulti = fMulti;
            }
            public void SetParam_Dir(int nID, bool bDirReverse)
            {
                m_aCParam[nID].m_bDirReverse = bDirReverse;
             
            }
            public void SetParam_Model(int nID, EModel_t EModel)
            {
                m_aCParam[nID].SetParams_Model(EModel);
            }
            public bool[] m_abMot = new bool[256];
            public int[] m_anMot = new int[256];
            public float[] m_afMot = new float[256];
            public int[] m_anMot_Pose = new int[256];
            public float[] m_afMot_Pose = new float[256];
            public class CParam_t
            {
                //public bool m_bModel_High = false;
                public int m_nSet_Operation_Address = 11;
                public int m_nSet_Operation_Size = 1;

                public int m_nSet_GoalCurrent_Address = -1;//102; // XM에서만 동작, -1은 사용 안함
                public int m_nSet_GoalCurrent_Size = 2;
                public int m_nSet_GAIN_POS_P = 84;
                public int m_nSet_GAIN_POS_P_Size = 2;
                public int m_nSet_GAIN_POS_I = 82;
                public int m_nSet_GAIN_POS_I_Size = 2;
                public int m_nSet_GAIN_POS_D = 80;
                public int m_nSet_GAIN_POS_D_Size = 2;
                public int m_nSet_GAIN_VEL_P = 78;
                public int m_nSet_GAIN_VEL_P_Size = 2;
                public int m_nSet_GAIN_VEL_I = 76;
                public int m_nSet_GAIN_VEL_I_Size = 2;
                public int m_nSet_GAIN_VEL_D = -1;
                public int m_nSet_GAIN_VEL_D_Size = 2;

                public int m_nSet_Torq_Address = 64;
                public int m_nSet_Torq_Size = 1;
                public int m_nSet_Led_Address = 65;
                public int m_nSet_Led_Size = 1;
                public int m_nSet_Curr_Address = 102;
                public int m_nSet_Curr_Size = 2;
                public int m_nSet_Position_Speed_Address = 112;
                public int m_nSet_Position_Speed_Size = 4;
                public int m_nSet_Position_Address = 116;
                public int m_nSet_Position_Size = 4;
                public int m_nSet_Speed_Address = 104;
                public int m_nSet_Speed_Size = 4;

                public float m_fMechMove = 4096.0f;    // PH54-100 => -501,923 ~ 501,923, H54-200 => -501,923 ~ 501,923
                public float m_fCenter = 2048.0f;
                public float m_fMechAngle = 360;
                public float m_fJointRpm = 0.229f; // ph54-100 = 0.01, H54-200 = 0.01
                public bool m_bDirReverse = false;
                public float m_fMulti = 1.0f;

                public int m_nGet_Position_Address = 132;
                public int m_nGet_Position_Size = 4;
                public int m_nGet_Current_Address = 126;
                public int m_nGet_Current_Size = 2;
                public int m_nMax_Speed_For_Position = 0;

                public void SetParam_Address_Torq(int nVal = 64) { m_nSet_Torq_Address = nVal; }
                public void SetParam_Address_Size_Torq(int nVal = 1) { m_nSet_Torq_Size = nVal; }
                public void SetParam_Address_Curr(int nVal = 102) { m_nSet_Curr_Address = nVal; }
                public void SetParam_Address_Size_Curr(int nVal = 2) { m_nSet_Curr_Size = nVal; }
                public void SetParam_Address_Led(int nVal = 66) { m_nSet_Led_Address = nVal; }
                public void SetParam_Address_Size_Led(int nVal = 1) { m_nSet_Led_Size = nVal; }
                public void SetParam_Address_PositionSpeed(int nVal = 112) { m_nSet_Position_Speed_Address = nVal; }
                public void SetParam_Address_Size_PositionSpeed(int nVal = 4) { m_nSet_Position_Speed_Size = nVal; }
                public void SetParam_Address_Position(int nVal = 116) { m_nSet_Position_Address = nVal; }
                public void SetParam_Address_Size_Position(int nVal = 4) { m_nSet_Position_Size = nVal; }
                public void SetParam_Address_GetPosition(int nVal = 132) { m_nGet_Position_Address = nVal; }
                public void SetParam_Address_Size_GetPosition(int nVal = 4) { m_nGet_Position_Size = nVal; }
                public void SetParam_Address_Speed(int nVal = 104) { m_nSet_Position_Speed_Address = nVal; }
                public void SetParam_Address_Size_Speed(int nVal = 4) { m_nSet_Position_Speed_Size = nVal; }

                public void SetParam_Dir(bool bReverse = false) { m_bDirReverse = bReverse; }
                public void SetParam_Multi(float fMulti = 1.0f) { m_fMulti = fMulti; if (fMulti == 0) m_fMulti = 1.0f; }
                
                public void SetParams_Model(EModel_t EModel)
                {
                    switch(EModel)
                    {
                        case EModel_t.X:
                        case EModel_t._X_430_250:
                        case EModel_t._X_330_077:
                        case EModel_t._X_330_288:
                        case EModel_t._2X_430_250:
                        case EModel_t._X_540_150:
                        case EModel_t._X_540_270:
                        //case EModel_t._X_320:
                            {
                                //m_bModel_High = false;
                                m_nSet_Torq_Address = 64;
                                m_nSet_Torq_Size = 1;
                                m_nSet_Led_Address = 65;
                                m_nSet_Led_Size = 1;
                                m_nSet_Curr_Address = 102;
                                m_nSet_Curr_Size = 2;
                                m_nSet_Speed_Address = 104;
                                m_nSet_Speed_Size = 4;
                                m_nSet_Position_Speed_Address = 112;
                                m_nSet_Position_Speed_Size = 4;
                                m_nSet_Position_Address = 116;
                                m_nSet_Position_Size = 4;

                                m_fMechMove = 4096.0f;    // PH54-100 => -501,923 ~ 501,923, H54-200 => -501,923 ~ 501,923
                                m_fCenter = 2048.0f;
                                m_fMechAngle = 360;
                                m_fJointRpm = 0.229f; // ph54-100 = 0.01, H54-200 = 0.01
                                m_bDirReverse = false;
                                m_fMulti = 1.0f;
                                m_nGet_Position_Address = 132;
                                m_nGet_Position_Size = 4;

                                //
                                m_nSet_GoalCurrent_Address = -1;
                                m_nSet_GoalCurrent_Size = 2;
                                m_nSet_GAIN_POS_P = 84;
                                m_nSet_GAIN_POS_P_Size = 2;
                                m_nSet_GAIN_POS_I = 82;
                                m_nSet_GAIN_POS_I_Size = 2;
                                m_nSet_GAIN_POS_D = 80;
                                m_nSet_GAIN_POS_D_Size = 2;
                                m_nSet_GAIN_VEL_P = 78;
                                m_nSet_GAIN_VEL_P_Size = 2;
                                m_nSet_GAIN_VEL_I = 76;
                                m_nSet_GAIN_VEL_I_Size = 2;
                                m_nSet_GAIN_VEL_D = -1;
                                m_nSet_GAIN_VEL_D_Size = 2;

                                m_nGet_Current_Address = 126; // present load
                                m_nGet_Current_Size = 2;

                                m_nMax_Speed_For_Position = 0; // 0 일때 최고속력으로...
                            }
                            break;
                        case EModel_t.Y:
                        case EModel_t._Y_70_210_M001:
                        case EModel_t._Y_70_210_R051:
                        case EModel_t._Y_70_210_R099:
                        case EModel_t._Y_80_230_M001:
                        case EModel_t._Y_80_230_R051:
                        case EModel_t._Y_80_230_R099:
                            {
                                //m_bModel_High = false;
                                m_nSet_Torq_Address = 512;
                                m_nSet_Torq_Size = 1;
                                m_nSet_Led_Address = 513;
                                m_nSet_Led_Size = 1;
                                m_nSet_Curr_Address = 526;
                                m_nSet_Curr_Size = 2;
                                m_nSet_Speed_Address = 528;
                                m_nSet_Speed_Size = 4;
                                m_nSet_Position_Speed_Address = 528;// 244;// 528; // Y는 속도값을 속도제어모드와 같이 사용한다.
                                m_nSet_Position_Speed_Size = 4;
                                m_nSet_Position_Address = 532;
                                m_nSet_Position_Size = 4;

                                m_fMechMove = 524288.0f;// 51904512.0f;    // Resolution
                                m_fCenter = 0f;// 524288.0f / 2.0f;
                                m_fMechAngle = 360.0f;
                                m_fJointRpm = 0.01f;// Goal velocity 참조
                                m_bDirReverse = false;
                                m_fMulti = 1.0f;
                                m_nGet_Position_Address = 552;
                                m_nGet_Position_Size = 4;
                                //
                                
                                m_nSet_GoalCurrent_Address = 526;
                                m_nSet_GoalCurrent_Size = 2;
                                m_nSet_GAIN_POS_P = 232;
                                m_nSet_GAIN_POS_P_Size = 4;
                                m_nSet_GAIN_POS_I = 228;
                                m_nSet_GAIN_POS_I_Size = 4;
                                m_nSet_GAIN_POS_D = 224;
                                m_nSet_GAIN_POS_D_Size = 4;
                                m_nSet_GAIN_VEL_P = 216;
                                m_nSet_GAIN_VEL_P_Size = 4;
                                m_nSet_GAIN_VEL_I = 212;
                                m_nSet_GAIN_VEL_I_Size = 4;
                                m_nSet_GAIN_VEL_D = -1;
                                m_nSet_GAIN_VEL_D_Size = 4;
                                
                                m_nGet_Current_Address = 546;
                                m_nGet_Current_Size = 2;

                                m_nMax_Speed_For_Position = 2020; // 2020 일때 최고속력으로...
                            }
                            break;
                        case EModel_t.P:
                        case EModel_t._PH54_60_250:
                        case EModel_t._PH54_40_250:
                        case EModel_t._PH42_10_260:
                        case EModel_t._PM54_60_250:
                        case EModel_t._PM54_40_250:
                        case EModel_t._PM42_10_260:
                            {
                                //m_bModel_High = true;
                                m_nSet_Torq_Address = 512;
                                m_nSet_Torq_Size = 1;
                                m_nSet_Led_Address = 513;
                                m_nSet_Led_Size = 1;
                                m_nSet_Curr_Address = 550;
                                m_nSet_Curr_Size = 2;
                                m_nSet_Speed_Address = 552;
                                m_nSet_Speed_Size = 4;
                                m_nSet_Position_Speed_Address = 560;
                                m_nSet_Position_Speed_Size = 4;
                                m_nSet_Position_Address = 564;
                                m_nSet_Position_Size = 4;

                                m_fMechMove = 1003846.0f;    // PH54-100 => -501,923 ~ 501,923, H54-200 => -501,923 ~ 501,923
                                m_fCenter = 0f;
                                m_fMechAngle = 360;
                                m_fJointRpm = 0.01f; // ph54-100 = 0.01, H54-200 = 0.01
                                m_bDirReverse = false;
                                m_fMulti = 1.0f;
                                m_nGet_Position_Address = 580;
                                m_nGet_Position_Size = 4;

                                ////////////
                                m_nSet_GoalCurrent_Address = 550;
                                m_nSet_GoalCurrent_Size = 2;
                                m_nSet_GAIN_POS_P = 532;
                                m_nSet_GAIN_POS_P_Size = 2;
                                m_nSet_GAIN_POS_I = 530;
                                m_nSet_GAIN_POS_I_Size = 2;
                                m_nSet_GAIN_POS_D = 528;
                                m_nSet_GAIN_POS_D_Size = 2;
                                m_nSet_GAIN_VEL_P = 526;
                                m_nSet_GAIN_VEL_P_Size = 2;
                                m_nSet_GAIN_VEL_I = 524;
                                m_nSet_GAIN_VEL_I_Size = 2;
                                m_nSet_GAIN_VEL_D = -1;
                                m_nSet_GAIN_VEL_D_Size = 2;

                                m_nGet_Current_Address = 574;
                                m_nGet_Current_Size = 2;

                                m_nMax_Speed_For_Position = 0; // 0 일때 최고속력으로...
                            }
                            break;
                    }
                }
                public void SetParam(bool bSetHight = false)
                {
                    if (bSetHight == true) // PH, H54(Pro) ... 
                    {
                        //m_bModel_High = true;
                        m_nSet_Torq_Address = 512;
                        m_nSet_Torq_Size = 1;
                        m_nSet_Led_Address = 513;
                        m_nSet_Led_Size = 1;
                        m_nSet_Speed_Address = 552;
                        m_nSet_Speed_Size = 4;
                        m_nSet_Position_Speed_Address = 560;
                        m_nSet_Position_Speed_Size = 4;
                        m_nSet_Position_Address = 564;
                        m_nSet_Position_Size = 4;

                        m_fMechMove = 1003846.0f;    // PH54-100 => -501,923 ~ 501,923, H54-200 => -501,923 ~ 501,923
                        m_fCenter = 0f;
                        m_fMechAngle = 360;
                        m_fJointRpm = 0.01f; // ph54-100 = 0.01, H54-200 = 0.01
                        m_bDirReverse = false;
                        m_fMulti = 1.0f;
                        m_nGet_Position_Address = 580;
                        m_nGet_Position_Size = 4;
                    }
                    else
                    {
                        //m_bModel_High = false;
                        m_nSet_Torq_Address = 64;
                        m_nSet_Torq_Size = 1;
                        m_nSet_Led_Address = 65;
                        m_nSet_Led_Size = 1;
                        m_nSet_Speed_Address = 104;
                        m_nSet_Speed_Size = 4;
                        m_nSet_Position_Speed_Address = 112;
                        m_nSet_Position_Speed_Size = 4;
                        m_nSet_Position_Address = 116;
                        m_nSet_Position_Size = 4;

                        m_fMechMove = 4096.0f;    // PH54-100 => -501,923 ~ 501,923, H54-200 => -501,923 ~ 501,923
                        m_fCenter = 2048.0f;
                        m_fMechAngle = 360;
                        m_fJointRpm = 0.229f; // ph54-100 = 0.01, H54-200 = 0.01
                        m_bDirReverse = false;
                        m_fMulti = 1.0f;
                        m_nGet_Position_Address = 132;
                        m_nGet_Position_Size = 4;
                    }
                }
            }
            public CParam_t[] m_aCParam = new CParam_t[256];

            #region Open / Close / IsOpen
            private Ojw.CSocket m_CSock_Client = new CSocket();
            public Ojw.CSerial m_CSerial = new CSerial();
            public bool IsOpen() { return (m_CSerial.IsConnect() || m_CSock_Client.IsConnect()) ? true : false; }
            public bool Open(int nPort, string strAddress)
            {
                bool bConnected = m_CSock_Client.IsConnect();
                if (bConnected == false)
                {
                    if (m_CSock_Client.Connect(strAddress, nPort) == true)
                    {
                        return true;
                    }
                }
                Ojw.CMessage.Write_Error("Cannot Connect [Open_Socket({0}, {1})]", nPort, strAddress);
                return false;
            }
            private Ojw.CServer m_CServer = new CServer();
            private Thread m_thServer;
            private bool m_bWebSocket = false;
            private int m_nSocket_Mode = 0; // 0 : Normal Mode, 1 : Bypass Mode;
            public void Socket_BypassMode(bool bBypass) { m_nSocket_Mode = (bBypass) ? 1 : 0; }
            public bool IsSocket_BypassMode() { return (m_nSocket_Mode == 1) ? true : false; }
            //private List<Thread> m_lstThServer = new List<Thread>();
            public bool Open_Socket(int nPort) { return Open_Socket(nPort, null, false); }
            public bool Open_Socket(int nPort, bool bWebSocket) { return Open_Socket(nPort, null, bWebSocket); }
            public bool Open_Socket(int nPort, string strIP) { return Open_Socket(nPort, strIP, false); }
            public bool IsOpen_Socket() { return m_CServer.sock_started(); }
            public void Close_Socket()
            {
                m_bThreadStart = false;
                //Thread.Sleep(100);
                m_CServer.sock_stop();
            }
            public bool Open_Socket(int nPort, string strIP, bool bWebSocket)
            {
                if (m_CServer.sock_started() == false)
                {
                    if (strIP == null) m_CServer.sock_start(nPort);
                    else
                    {
                        if (strIP.Length < 7) m_CServer.sock_start(nPort);
                        m_CServer.sock_start(strIP, nPort); // 지정한 포트로 동작
                    }

                    if (m_CServer.sock_started() == true) // 서버가 잘 시작했다면...
                    {
                        //클라이언트 생성시 스레드를 생성한다.
                        m_bWebSocket = bWebSocket;
                        m_thServer = new Thread(new ThreadStart(ThreadServer));
                        m_thServer.Start();
                        //return true;
                    }
                    else
                    {
                        Ojw.printf_Error("서버 동작 실패");
                        Ojw.newline();
                    }
                }
                else
                {
                    Ojw.printf_Error("서버가 이미 동작하고 있습니다.");
                    Ojw.newline();
                }



                return IsOpen_Socket();//m_CServer.
            }


            private const int _SIZE_QUE = 3;
            private const int _SIZE_QUE_LENGTH = 512;
            private int m_nQue_Index_Next = 0;
            private int m_nQue_Index = 0;
            private int m_nQue_Count = 0;
            private byte[,] m_abyteQue = new byte[_SIZE_QUE, _SIZE_QUE_LENGTH];
            //private struct SQueByte_t{
            //    public byte[] buffer = new byte[_SIZE_QUE_LENGTH];
            //}
            //private List<SQueByte_t> m_alstQue = new List<SQueByte_t>();
#if true
            private bool m_bThreadStart = false;
            private void ThreadServer()
            {
                try
                {
                    m_bThreadStart = true;
                    for (int i = 0; i < _SIZE_QUE; i++)
                    {
                        for (int j = 0; j < _SIZE_QUE_LENGTH; j++)
                        {
                            m_abyteQue[i, j] = 0;
                        }
                    }
                    bool bShake = false;
                    Ojw.printf("ThreadServer()\r\n");
                    m_CServer.WaitClient(true);
                    Ojw.printf("ThreadServer() - Started\r\n");
                    while ((m_CServer.sock_started() == true) && (m_bThreadStart))
                    {
                        if (m_CServer.isClientConnected() == false)
                        {
                            Ojw.printf("소켓연결 끊어짐\r\n");
                            m_CServer.WaitClient(true);
                        }
                        /*byte[] buffer = m_CServer.request_bytes();
                        if (buffer != null)
                        {
                            string str = Encoding.UTF8.GetString(buffer);
                            printf("->{0}\r\n", str);
                        }*/
                        //m_CServer.sleep(1);



                        int nBufferSize = m_CServer.GetBuffer_Length();
                        if (nBufferSize > 0) // 무언가 데이타가 들어왔다면
                        {
                            //Ojw.Log("데이타 들어옴");
                            byte[] pbyData = m_CServer.sock_get_bytes(nBufferSize);

                            if (IsSocket_BypassMode() == true)
                            {
                                SendPacket(pbyData, pbyData.Length);
                            }
                            else
                            {
                                bool bStart = false;
                                bool bContinue = false;
                                //int nSize = 0;
                                int nSize2 = m_abyteQue[m_nQue_Index_Next, 0] + m_abyteQue[m_nQue_Index_Next, 1] * 256;
                                if (nSize2 < 0) nSize2 = 0;
                                else if (nSize2 > _SIZE_QUE_LENGTH) nSize2 = _SIZE_QUE_LENGTH;
                                if (m_bWebSocket == true)
                                {
                                    //Ojw.Log("Type 1");
                                    string str = Ojw.CConvert.BytesToStr_UTF8(pbyData);
                                    for (int i = 0; i < str.Length; i++)
                                    {
                                        if (str[i] == '!')
                                        {
                                            bStart = true;
                                            nSize2 = 0;
                                            bContinue = false;
                                        }
                                        else if (str[i] == ';')
                                        {
                                            bStart = false;
                                            bContinue = true;
                                            m_abyteQue[m_nQue_Index_Next, 0] = (byte)(nSize2 & 0xff);
                                            m_abyteQue[m_nQue_Index_Next, 1] = (byte)((nSize2 >> 8) & 0xff);
                                            m_nQue_Index = m_nQue_Index_Next;
                                            m_nQue_Index_Next++;
                                            m_nQue_Count++;
                                        }
                                        else
                                        {
                                            m_abyteQue[m_nQue_Index_Next, 2 + nSize2++] = (byte)str[i];
                                        }
                                    }
                                }
                                else
                                {
                                    //Ojw.Log("Type2");
                                    for (int i = 0; i < nBufferSize; i++)
                                    {
                                        if (pbyData[i] == 0x02)
                                        {
                                            bStart = true;
                                            nSize2 = 0;
                                            bContinue = false;
                                        }
                                        else if (pbyData[i] == 0x03)
                                        {
                                            bStart = false;
                                            bContinue = true;
                                            m_abyteQue[m_nQue_Index_Next, 0] = (byte)(nSize2 & 0xff);
                                            m_abyteQue[m_nQue_Index_Next, 1] = (byte)((nSize2 >> 8) & 0xff);
                                            m_nQue_Index = m_nQue_Index_Next;
                                            m_nQue_Index_Next++;
                                            m_nQue_Count++;
                                        }
                                        else
                                        {
                                            m_abyteQue[m_nQue_Index_Next, 2 + nSize2++] = pbyData[i];
                                        }
                                    }
                                }
                                if (m_nQue_Index_Next >= _SIZE_QUE)
                                {
                                    m_nQue_Index_Next = 0;
                                }
                                if (m_nQue_Count > _SIZE_QUE)
                                {
                                    m_nQue_Count = _SIZE_QUE;
                                }
                            }
                        }
                        if (m_nQue_Count > 0)
                        {
                            int nLen = m_abyteQue[m_nQue_Index, 0] + m_abyteQue[m_nQue_Index, 1] * 256;
                            string str = String.Empty;
                            byte[] pbyData = new byte[nLen];
                            //Array.Copy(m_abyteQue[m_nQue_Index], 2, pbyData, 0, nLen);
                            for (int i = 0; i < nLen; i++)
                            {
                                pbyData[i] = (m_abyteQue[m_nQue_Index, 2 + i]);
                            }
                            str = Ojw.CConvert.BytesToStr_UTF8(pbyData);
                            //char str[nLen + 1];
                            //memset(str, 0, sizeof(char) * (nLen + 1));
                            //memcpy(str, &m_abyteQue[m_nQue_Index,2], sizeof(char) * nLen);

                            PlayFrameString(str, true);
                            m_nQue_Count--;
                        }
                        //Thread.Sleep(10);
                        Thread.Sleep(1);
                    }
                }
                catch (Exception e)
                {
                    Ojw.printf_Error(e.ToString());
                }
            }
#else
            private void ThreadServer()
            {
                try
                {
                    //m_alstQue.Clear();
                    for(int i = 0; i < _SIZE_QUE; i++)
                    {
                        for (int j = 0; j < _SIZE_QUE_LENGTH; j++)
                        {
                            m_abyteQue[i,j] = 0;
                        }
                    }
                    bool bShake = false;
                    Ojw.printf("ThreadServer()\r\n");
                    m_CServer.WaitClient(true);
                    Ojw.printf("ThreadServer() - Started\r\n");
                    while (m_CServer.sock_started() == true)
                    {
                        if (m_CServer.isClientConnected() == false)
                        {
                            Ojw.printf("소켓연결 끊어짐\r\n");
                            bShake = false;
                            m_CServer.WaitClient(true);
                            //break;
                        }
                        int nBufferSize = m_CServer.GetBuffer_Length();
                        if (nBufferSize > 0) // 무언가 데이타가 들어왔다면
                        {
                            //Ojw.Log("데이타 들어옴");
                            byte[] pbyData = m_CServer.sock_get_bytes(nBufferSize);

                            string str = String.Empty;
                            str = Ojw.CConvert.BytesToStr_UTF8(pbyData);
                            // 받은 데이타를 클라이언트로 다시 한번 보내본다.(그냥... 클라이언트에서도 메세지 뜨라고...)
            #region Websocket
                            if (m_bWebSocket == true)
                            {
                                if (bShake == false)
                                {
                                    bShake = true;
                                    m_CServer.sock_send(HandShake(str));//pbyData);
                                    continue;
                                }
                                else
                                {
                                    bool fin = (bool)((pbyData[0] & 0x80) != 0);
                                    bool mask = (pbyData[1] & 0x80) != 0; // must be true, "All messages from the client to the server have this bit set"

                                    int opcode = pbyData[0] & 0x0f, // expecting 1 - text message
                                        msglen = pbyData[1] - 128, // & 0111 1111
                                        offset = 2;

                                    if (msglen == 126) { msglen = BitConverter.ToUInt16(new byte[] { pbyData[3], pbyData[2] }, 0); offset = 4; }
                                    if (msglen == 0)
                                    {
                                        Ojw.printf("연결 끊음, msglen == 0\r\n");
                                        bShake = false;
                                        m_CServer.m_tcpServer_Client.Client.Disconnect(false);
                                        //m_CServer.WaitClient(true);
                                    }
                                    else if (mask)
                                    {
                                        byte[] decoded = new byte[msglen];
                                        byte[] masks = new byte[4] { pbyData[offset], pbyData[offset + 1], pbyData[offset + 2], pbyData[offset + 3] };
                                        offset += 4;

                                        for (int i = 0; i < msglen; ++i)
                                            decoded[i] = (byte)(pbyData[offset + i] ^ masks[i % 4]);

                                        str = Encoding.UTF8.GetString(decoded);
                                        //Ojw.printf("{0}\r\n", str);
                                    }
                                }
                            }
                            #endregion Websocket

                            if (IsSocket_BypassMode() == true)
                            {
                                SendPacket(pbyData, pbyData.Length);
                            }
                            else
                            {
                                bool bStart = false;
                                bool bContinue = false;
                                //int nSize = 0;
                                int nSize2 = m_abyteQue[m_nQue_Index_Next,0] + m_abyteQue[m_nQue_Index_Next,1] * 256;
                                if (nSize2 < 0) nSize2 = 0;
                                else if (nSize2 > _SIZE_QUE_LENGTH) nSize2 = _SIZE_QUE_LENGTH;
                                if (m_bWebSocket == true)
                                {
                                    //str = Ojw.CConvert.BytesToStr_UTF8(pbyData);
                                    for (int i = 0; i < str.Length; i++)
                                    {
                                        if (str[i] == '!')
                                        {
                                            bStart = true;
                                            nSize2 = 0;
                                            bContinue = false;
                                        }
                                        else if (str[i] == ';')
                                        {
                                            bStart = false;
                                            bContinue = true;
                                            m_abyteQue[m_nQue_Index_Next, 0] = (byte)(nSize2 & 0xff);
                                            m_abyteQue[m_nQue_Index_Next, 1] = (byte)((nSize2 >> 8) & 0xff);
                                            m_nQue_Index = m_nQue_Index_Next;
                                            m_nQue_Index_Next++;
                                            m_nQue_Count++;
                                        }
                                        else
                                        {
                                            m_abyteQue[m_nQue_Index_Next, 2 + nSize2++] = (byte)str[i];
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < nBufferSize; i++)
                                    {
                                        if (pbyData[i] == 0x02)
                                        {
                                            bStart = true;
                                            nSize2 = 0;
                                            bContinue = false;
                                        }
                                        else if (pbyData[i] == 0x03)
                                        {
                                            bStart = false;
                                            bContinue = true;
                                            m_abyteQue[m_nQue_Index_Next, 0] = (byte)(nSize2 & 0xff);
                                            m_abyteQue[m_nQue_Index_Next, 1] = (byte)((nSize2 >> 8) & 0xff);
                                            m_nQue_Index = m_nQue_Index_Next;
                                            m_nQue_Index_Next++;
                                            m_nQue_Count++;
                                        }
                                        else
                                        {
                                            m_abyteQue[m_nQue_Index_Next, 2 + nSize2++] = pbyData[i];
                                        }
                                    }
                                }
                                if (m_nQue_Index_Next >= _SIZE_QUE)
                                {
                                    m_nQue_Index_Next = 0;
                                }
                                if (m_nQue_Count > _SIZE_QUE)
                                {
                                    m_nQue_Count = _SIZE_QUE;
                                }
                            }
                        }
                        if (m_nQue_Count > 0)
                        {
                            int nLen = m_abyteQue[m_nQue_Index,0] + m_abyteQue[m_nQue_Index,1] * 256;
                            string str = String.Empty;
                            byte [] pbyData = new byte[nLen];
                            //Array.Copy(m_abyteQue[m_nQue_Index], 2, pbyData, 0, nLen);
                            for (int i = 0; i < nLen; i++)
                            {
                                pbyData[i] = (m_abyteQue[m_nQue_Index, 2 + i]);
                            }
                            str = Ojw.CConvert.BytesToStr_UTF8(pbyData);
                            //char str[nLen + 1];
                            //memset(str, 0, sizeof(char) * (nLen + 1));
                            //memcpy(str, &m_abyteQue[m_nQue_Index,2], sizeof(char) * nLen);
                
                            PlayFrameString(str, true);
                            m_nQue_Count--;
                        }
                        Thread.Sleep(10);
                    }
                }
                catch (Exception e)
                {
                    Ojw.printf_Error(e.ToString());
                }
            }
            private int m_nHandUp = 0;
            private Byte[] HandShake(string data)
            {
                const string eol = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker

                Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                    + "Connection: Upgrade" + eol
                    + "Upgrade: websocket" + eol
                    + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                        System.Security.Cryptography.SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new System.Text.RegularExpressions.Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )
                        )
                    ) + eol
                    + eol);
                return response;
            }
#endif

            Ojw.C3d m_C3d = null;// = new C3d();

            //public bool Open(Ojw.C3d COjw3d, int nIndex=0)
            //{
            //    m_C3d = COjw3d;
            //    m_CSerial = m_C3d.m_CMonster.GetSerial(nIndex);

            //    bool bConnected = m_CSerial.IsConnect();
            //    if (bConnected == false)
            //    {
            //        return true;
            //    }
            //    Ojw.CMessage.Write_Error("Cannot Connect [Open_Serial(m_C3d.m_CMonster)]");
            //    return false;
            //}
            private bool m_bSyncRendering = false;
            public void SyncRendering(Ojw.C3d OjwC3d)
            {
                if (OjwC3d != null) m_bSyncRendering = true;
                else m_bSyncRendering = false;
                m_C3d = OjwC3d;
            }
            public bool Open(int nPort, int nBaudRate)
            {
                bool bConnected = m_CSerial.IsConnect();
                if (bConnected == false)
                {
                    if (m_CSerial.Connect(nPort, nBaudRate) == true)
                    {
                        return true;
                    }
                }
                Ojw.CMessage.Write_Error("Cannot Connect [Open_Serial({0}, {1})]", nPort, nBaudRate);
                return false;
            }
            public void Close()
            {
                if (m_CSerial.IsConnect()) m_CSerial.DisConnect();
                if (m_CSock_Client.IsConnect()) m_CSock_Client.DisConnect();
            }
            #endregion Open / Close / IsOpen

            #region Command
            public class CCommand_t
            {
                public int nID = 0;
                public float fVal = 0;
                public CCommand_t(int id, float val)
                {
                    nID = id;
                    fVal = val;
                }
            }
            private List<CCommand_t> m_lstCmdIDs = new List<CCommand_t>();
            public void Command_Clear() { m_lstCmdIDs.Clear(); }
            public void Command_Set(int nID, float fValue) { m_lstCmdIDs.Add(new CCommand_t(nID, fValue)); }
            public void Command_Set_Rpm(int nID, float fRpm) { m_lstCmdIDs.Add(new CCommand_t(nID, CalcRpm2Raw(nID, fRpm))); }
            public void Clear() { Command_Clear(); }
            public void Set(int nID, float fValue) { Command_Set(nID, fValue); }
            public void Set_Rpm(int nID, float fRpm) { Command_Set_Rpm(nID, fRpm); }
            public float GetAngle(int nID) { return m_afMot[nID]; }
            //public void SetTorq(params CCommand_t [] aCCommands)
            //{
            //    Sync_Clear();
            //    if (aCCommands.Length > 0)
            //    {
            //        //int nEn = (bEn == true) ? 1 : 0;
            //        for (int i = 0; i < aCCommands.Length; i++)
            //        {
            //            Sync_Push_Byte(aCCommands[i].nID, (int)Math.Round(aCCommands[i].fVal));
            //        }
            //        Sync_Flush(m_aCParam[aCCommands[0].nID].m_nSet_Torq_Address);
            //        //aCCommands.Clear();
            //    }
            //    else if (m_lstCmdIDs.Count > 0)
            //    {
            //        for (int i = 0; i < m_lstCmdIDs.Count; i++)
            //        {
            //            Sync_Push_Byte(m_lstCmdIDs[i].nID, (int)Math.Round(m_lstCmdIDs[i].fVal));
            //        }
            //        Sync_Flush(m_aCParam[m_lstCmdIDs[0].nID].m_nSet_Torq_Address);
            //    }
            //    Command_Clear();
            //}
            public void SetTorq(bool bOn)
            {
                Send(254, 0x03, 64, (byte)((bOn == true) ? 1 : 0));
            }
            public void SetTorq(params float [] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i*2], afVals[i*2 + 1]); }
                SetTorq(aCCommands);
            }

            public void SetTorq() { SetTorq(m_lstCmdIDs.ToArray()); }
            public void SetTorq(params CCommand_t[] aCCommands)
            {
                List<CCommand_t> lstSecond = new List<CCommand_t>();
                //for (int nIter = 0; nIter < 2; nIter++)
                while (true)
                {
                    Sync_Clear();
                    bool bRes = false;
                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    Command_Clear();
                    if (lstSecond.Count > 0)
                    {
                        CCmd = lstSecond.ToArray();
                        lstSecond.Clear();
                    }
                    if (CCmd == null) break;
                    if (CCmd.Length > 0)
                    {
                        for (int i = 0; i < CCmd.Length; i++)
                        {
                            if (m_aCParam[CCmd[0].nID].m_nSet_Torq_Address != m_aCParam[CCmd[i].nID].m_nSet_Torq_Address)
                            {
                                lstSecond.Add(new CCommand_t(CCmd[i].nID, CCmd[i].fVal));
                            }
                            else Sync_Push_Byte(CCmd[i].nID, (int)Math.Round(CCmd[i].fVal));
                        }
                        Sync_Flush(m_aCParam[CCmd[0].nID].m_nSet_Torq_Address);
                    }
                    if (lstSecond.Count == 0) break;
                }
                //Command_Clear();
            }
            public void SetCurr(int nID, int nValue)
            {
                //if (nValue < 0) nValue = 2250;//1750;
                //m_CMot.Writes_Frame(102, 2, 1, m_anIDs, m_anDatas);
                Send(nID, 0x03, m_aCParam[nID].m_nSet_Curr_Address, Ojw.CConvert.ShortToBytes((short)nValue));
            }
            public void SetLed(bool bOn)
            {
                int nID = 254;
                Send(nID, 0x03, m_aCParam[nID].m_nSet_Led_Address, (byte)((bOn == true) ? 1 : 0));
            }
            public void SetLed(params float [] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                //for (int i = 0; i < nLen; i += 2) { aCCommands[i] = new CCommand_t((int)afVals[i], afVals[i + 1]); }
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i * 2], afVals[i * 2 + 1]); }
                SetLed(aCCommands);
            }
            public void SetLed(params CCommand_t[] aCCommands)
            {
                List<CCommand_t> lstSecond = new List<CCommand_t>();
                //for (int nIter = 0; nIter < 2; nIter++)
                while (true)
                {
                    Sync_Clear();
                    bool bRes = false;
                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    Command_Clear();
                    if (lstSecond.Count > 0)
                    {
                        CCmd = lstSecond.ToArray();
                        lstSecond.Clear();
                    }
                    if (CCmd == null) break;
                    if (CCmd.Length > 0)
                    {
                        for (int i = 0; i < CCmd.Length; i++)
                        {
                            if (m_aCParam[CCmd[0].nID].m_nSet_Led_Address != m_aCParam[CCmd[i].nID].m_nSet_Led_Address)
                            {
                                lstSecond.Add(new CCommand_t(CCmd[i].nID, CCmd[i].fVal));
                            }
                            else Sync_Push_Byte(CCmd[i].nID, (int)Math.Round(CCmd[i].fVal));
                        }
                        Sync_Flush(m_aCParam[CCmd[0].nID].m_nSet_Led_Address);
                    }
                    if (lstSecond.Count == 0) break;
                }
                //Command_Clear();
            }
            public void SetOperation(int nID=254, int nMode=3)
            {
                SetCommand(nID, m_aCParam[nID].m_nSet_Operation_Address, nMode);
            }
            public void SetCommand(int nID, int nAddress, int nData, int nSize = 1)
            {
                if (nSize <= 1) Send(nID, 0x03, nAddress, (byte)nData);
                else
                {
                    byte[] buffer;
                    if (nSize == 2) 
                        buffer = Ojw.CConvert.ShortToBytes((short)nData);
                    else if (nSize == 4)
                        buffer = Ojw.CConvert.IntToBytes((short)nData);
                    else
                        buffer = Ojw.CConvert.LongToBytes((short)nData);
                    Send(nID, 0x03, nAddress, buffer);
                }
            }
            //private List<CCommand_t> lstCCommand = new List<CCommand_t>();
            //public void Move(int nTime_ms, int nDelay, params CCommand_t[] aCCommands)
            //{
            //    Ojw.CTimer CTmr = new CTimer();
            //    CTmr.Set();

            //    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
            //    Command_Clear();
            //    if (CCmd.Length > 0)
            //    {
            //        //int nTimer = nTime_ms + nDelay;
            //        List<int> lstIDs = new List<int>();
            //        lstIDs.Clear();
            //        for (int i = 0; i < CCmd.Length; i++) { lstIDs.Add(CCmd[i].nID); Command_Set(CCmd[i].nID, CalcPosition_Time(CCmd[i].nID, nTime_ms, nDelay, CCmd[i].fVal)); }
            //        SetPosition_Speed(m_lstCmdIDs.ToArray());

            //        for (int i = 0; i < CCmd.Length; i++) { Command_Set(CCmd[i].nID, CCmd[i].fVal); }
            //        SetPosition();

            //        //while (true)
            //        //{
            //        //    SyncRead(lstIDs.ToArray());
            //        //    if (CTmr.Get() >= nTimer) break;
            //        //}
            //    }
            //}
            public bool IsEms() { return m_bEms; }
            public void Reset()
            {
                m_bEms = false;
            }
            private int m_nRun_Time = 0;
            private int m_nRun_Delay = 0;
            private CCommand_t[] m_aCRun_Commands;
            /*public void Move_Conti(int nTime_ms, int nDelay, params CCommand_t[] aCCommands)
            {
                if (m_bEms == true) return;
                

                m_nRun_Time = nTime_ms;
                m_nRun_Delay = nDelay;
                m_aCRun_Commands = aCCommands;
                Thread th = new Thread(new ThreadStart(FThread_Run));
                th.Start();

                
            }
            public void Wait_Move(int nTime_ms, int nDelay, bool bMotionEnd = false)
            {
                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();
                m_bBreak2 = !bMotionEnd;
                while (m_bEms == false)
                {
                    float fGet = CTmr.Get();
                    if (m_bEms == true) break;

                    if (fGet >= nTime_ms + nDelay) break;
                    Ojw.CTimer.DoEvent();
                }
            }*/
            private void FThread_Run()
            {
                try
                {
                    Ojw.CMessage.Write("FThread_Run -> Start");

                    Ojw.CTimer CTmr = new CTimer();
                    CTmr.Set();

                    int nTime_ms = m_nRun_Time;
                    int nDelay = m_nRun_Delay;
                    CCommand_t[] aCCommands = m_aCRun_Commands;

                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    Command_Clear();
                    if (CCmd.Length > 0)
                    {
                        float[] afMot = new float[m_afMot.Length];
                        float[] afRes = new float[m_afMot.Length];
                        Array.Copy(m_afMot, afMot, m_afMot.Length);
                        m_bBreak = false;
                        while (true)
                        {
                            if ((m_bEms == true) || (m_bBreak == true))
                            {
                                if (m_bBreak == true) m_bBreak = false;
                                //SyncRead(lstIDs.ToArray());
                                for (int i = 0; i < CCmd.Length; i++) { m_afMot_Pose[CCmd[i].nID] = m_afMot[CCmd[i].nID] = afRes[CCmd[i].nID]; }
                                return;
                            }

                            //int nTimer = nTime_ms + nDelay;
                            List<int> lstIDs = new List<int>();
                            lstIDs.Clear();
                            for (int i = 0; i < CCmd.Length; i++) { lstIDs.Add(CCmd[i].nID); Command_Set(CCmd[i].nID, 0); } //CalcPosition_Time(CCmd[i].nID, nTime_ms, nDelay, CCmd[i].fVal)); }
                            SetPosition_Speed(m_lstCmdIDs.ToArray());

                            float fGet = CTmr.Get();
                            float fTmr = (fGet / (float)nTime_ms);
                            if (fTmr > 1f) fTmr = 1f;

                            // -Delay 탈출
                            if (m_bBreak2 == true)
                            {
                                m_bBreak2 = false;
                                m_bBreak = true;
                            }
                            if (m_bBreak == true)
                            {
                                //m_bBreak = false;
                                float fTmr_Sub = (nDelay >= 0) ? 0 : (fGet / (float)(nTime_ms + nDelay));
                                if (fTmr_Sub >= 1f) break;
                            }

                            for (int i = 0; i < CCmd.Length; i++) { afRes[CCmd[i].nID] = afMot[CCmd[i].nID] + (CCmd[i].fVal - afMot[CCmd[i].nID]) * fTmr; Command_Set(CCmd[i].nID, afRes[CCmd[i].nID]); }
                            SetPosition(m_lstCmdIDs.ToArray());

                            if (fTmr >= 1f) break;
                            //Thread.Sleep(1);
                            Ojw.CTimer.DoEvent();
                        }
                        for (int i = 0; i < CCmd.Length; i++) { m_afMot_Pose[CCmd[i].nID] = m_afMot[CCmd[i].nID] = afRes[CCmd[i].nID]; }

                        while (true)
                        {
                            //SyncRead(lstIDs.ToArray());
                            if (CTmr.Get() >= (nTime_ms + nDelay)) break;
                            //Thread.Sleep(1);
                            Ojw.CTimer.DoEvent();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            private bool m_bNext = false;
            public void PlayFile_Next() { m_bNext = true; }
            public void PlayFile(string strFileName, bool bNowait = false)
            {
                if (IsOpen() == false) return;

                Ojw.Log("Play - {0}", strFileName);

                Ojw.CFile CFile = new Ojw.CFile();
                if (CFile.Load(strFileName) == 0) return;

                int nCnt = CFile.Get_Count();
                int nMax = 0;
                for (int i = 0; i < nCnt; i++)
                {
                    string str = CFile.Get(i);
                    string[] pstr = str.Split(',');
                    int nEnable = Ojw.CConvert.StrToInt(pstr[0]);
                    if (nEnable > 0) nMax = i;
                }
                for (int i = 0; i <= nMax; i++)
                {
                    string str = CFile.Get(i);
                    if (str.Length > 0)
                    {
                        if (str[0] != 's') str = 's' + str;
                        PlayFrameString(str, bNowait);
                    }
                    //string[] pstr = str.Split(',');

                    //int nEnable = Ojw.CConvert.StrToInt(pstr[0]);
                    //int nTime = Ojw.CConvert.StrToInt(pstr[1]);
                    //int nDelay = Ojw.CConvert.StrToInt(pstr[2]);

                    //Command_Clear();
                    //for (int nIndex = 3; nIndex < pstr.Length; nIndex++)
                    //{
                    //    string[] pstrDatas = pstr[nIndex].Split(':');
                    //    if (pstrDatas.Length > 1)
                    //    {
                    //        int nID = Ojw.CConvert.StrToInt(pstrDatas[0]);
                    //        int nEvd = Ojw.CConvert.StrToInt(pstrDatas[1]);
                    //        Command_Set(nID, (float)Math.Round(CalcEvd2Angle(nID, nEvd)));
                    //    }
                    //}
                    //bool bContinue = (i < nMax) ? true : false;
                    //if (bContinue == true)
                    //{
                    //    if (m_bNext == true)
                    //    {
                    //        m_bNext = false;
                    //        bContinue = true;
                    //    }
                    //}
                    //if (bOneshot_Style)
                    //{
                    //    Move_NoWait(nTime, nDelay, bContinue);
                    //    Wait();
                    //}
                    //else
                    //{
                    //    Move(nTime, nDelay, bContinue);
                    //}
                }
                Ojw.Log("Done - {0}", strFileName);
                //for (int i = 0; i < anIDs.Length; i++) { m_CCom.Command_Set(anIDs[i], m_C3d.GetData(anIDs[i])); }
                //m_CCom.Move(m_CGrid.GetTime(nLine), m_CGrid.GetDelay(nLine), bContinue);
            }
            //public void Play(string strData, bool bNowait = false)
            //{
            //    int nOffset = 0;
            //    int start = 0;
            //    int end = strData.IndexOf(',');
            //    float[] afVals = new float[50];
            //    int nIndex = 0;
            //    String strData2;
            //    int nCmd = 0;
            //    bool bWheel = false;
            //    bool bAngle = false;
            //    bool bWheel_Rpm = false;
            //    bool bWrite = false;
            //    int nTime = 0;
            //    int nDelay = 0;
            //    while (end != -1) {
            //    strData2 = strData.Substring(start, end);
            //    if ((strData2.IndexOf('s') < 0) && (strData2.IndexOf('S') < 0))
            //    {
            //        afVals[nIndex] = CConvert.StrToFloat(strData2);
            //        if (nIndex == 0) { nTime = (int)afVals[nIndex]; if (afVals[nIndex] == 0) { bWheel = true; bWheel_Rpm = true; } } // Time
            //        else if (nIndex == 1) { nTime = (int)afVals[nIndex]; } // Delay
            //        else { if (nIndex % 2 != 0) { if (bWheel_Rpm == true) { Command_Set_Rpm((int)afVals[nIndex - 1], afVals[nIndex]); } } }
            //    }
            //    else
            //    {
            //        strData2.ToLower();
            //        if (strData2.CompareTo("s1") == 0) { nCmd = 1; }
            //        else if (strData2.CompareTo("s2") == 0) { nCmd = 2; bAngle = true; }
            //        else if (strData2.CompareTo("s3") == 0) { nCmd = 3; bWheel = false; }
            //        else if (strData2.CompareTo("s4") == 0) { nCmd = 4; bWheel = false; bWheel_Rpm = true; }
            //        else if (strData2.CompareTo("s5") == 0) { nCmd = 5; bWrite = true; }
            //        nOffset = 1; // 2개 패스하고 다음 데이터에서...
            //    }
            
            //    // if (nCmd == 0) { }
            
            //    start = end + 1;
            //    end = strData.IndexOf(',', start);
            //    nIndex++;
            //    if (nOffset > 0) { nOffset--; nIndex = 0; }
            //    }

            //    afVals[nIndex] = CConvert.StrToFloat(strData.Substring(start, end));
            //    nIndex++;
            //    if ((nCmd == 0) || (nCmd == 2)) { Move(afVals, nIndex, bNowait); } //{ Move(afVals, nIndex, bNowait); }
            //    else if (bWheel == true) { SetSpeed(afVals); } //{ SetSpeed(afVals, nIndex); }
            //}
            //bool Move(float []afVals, int nLength, bool bNowait = false)
            //{
            //    //int nLength = afVals.Length;
            //    int nLen_Off = nLength % 2;// * 2;
            //    int nLen = nLength - nLen_Off;
            //    if (nLen < 4) return false;
            //    int nTime_ms = (int)afVals[0];
            //    int nDelay = (int)afVals[1];
            //    if (nTime_ms > 0)
            //    {
            //        CCommand_t [] aCCommands = new CCommand_t[(int)(nLen / 2 - 1)];
            //        for (int i = 2; i < nLen; i += 2)
            //        {
            //            aCCommands[i / 2 - 1].nID = (int)afVals[i];
            //            aCCommands[i / 2 - 1].fVal = afVals[i + 1];
            //        }
            //        //Move(nTime_ms, nDelay, aCCommands, nLen / 2 - 1, bNowait); // -1은 Time, Delay 위치를 뺀 값
            //        Move(nTime_ms, nDelay, bNowait, aCCommands); // -1은 Time, Delay 위치를 뺀 값
            //    }
            //    else
            //    {
            //        Command_Clear();
            //        for (int i = 2; i < nLen; i += 2) Command_Set_Rpm((int)afVals[i], afVals[i + 1]);
            //        SetSpeed(m_lstCmdIDs.ToArray());//, m_nSize_Command);
            //    }
            //    return true;
            //}
            // bypass 모드가 아닌 경우에 발동
            public void Play_Stream(string str)
            {
                string[] pstr = str.Split(',');
                if (pstr.Length >= 3)
                {
                    int nTime_ms = Ojw.CConvert.StrToInt(pstr[1]);
                    int nDelay = Ojw.CConvert.StrToInt(pstr[2]);
                    m_nWait_Time = (nTime_ms + nDelay);
                }
                byte[] pbuff = Ojw.CConvert.StrToBytes_UTF8(str);
                byte[] pbyte = new byte[pbuff.Length + 2];
                pbyte[0] = 0x02;
                for (int i = 1; i < pbyte.Length - 1; i++)
                {
                    pbyte[i] = pbuff[i - 1];
                }
                pbyte[pbyte.Length - 1] = 0x03;
                m_CSock_Client.Send(pbyte);
            }
            // Time, Delay, [id, pos, id, pos, id, pos...]
            // Time <= 0 이면 속도모드 값으로 동작한다.
            public bool Play(String str)
            {
                string [] pstr = str.Split(',');
                float[] afDatas = new float[pstr.Length];
                for (int i = 0; i < pstr.Length; i++)
                {
                    afDatas[i] = CConvert.StrToFloat(pstr[i]);
                }
                return Play(afDatas);
            }
            public bool Play(params float[] afDatas)
            {
                if (afDatas.Length > 2)
                {
                    string strData = "";
                    try
                    {
                        if ((afDatas.Length % 2) == 0)
                        {
                            int nTime = (int)afDatas[0];
                            if (nTime <= 0) { strData = "s4,"; nTime = 0; }
                            else strData = "s2,";
                            int nDelay = (int)afDatas[1];
                            strData += Ojw.CConvert.IntToStr(nTime) + "," + Ojw.CConvert.IntToStr(nDelay) + ",";
                            for (int i = 2; i < afDatas.Length; i += 2)
                            {
                                strData += Ojw.CConvert.IntToStr((int)afDatas[i]) + ":" + Ojw.CConvert.FloatToStr(afDatas[i + 1]);
                                if ((i + 2) < afDatas.Length) strData += ",";
                            }

                            PlayFrameString(strData, false);
                            //Ojw.CTimer CTmr = new CTimer(); CTmr.Set(); while (true) { if (m_bEms) break; if (CTmr.Get() >= (nTime + nDelay)) break; }
                            return true;
                        }

                    }
                    catch (Exception e)
                    {
                        Ojw.CMessage.Write_Error(strData + ": " + e.ToString());
                        return false;
                    }
                }
                return false;
            }
            // Time, Delay, [id, pos, id, pos, id, pos...]
            // Time <= 0 이면 속도모드 값으로 동작한다.
            public bool Play_NoWait(params float [] afDatas)
            {
                if (afDatas.Length > 2)
                {
                    string strData = "";
                    try
                    {
                        if ((afDatas.Length % 2) == 0)
                        {
                            int nTime = (int)afDatas[0];
                            if (nTime <= 0) { strData = "s4,"; nTime = 0; } // s3 은 속도, s4 는 속도 인데 RPM
                            else strData = "s2,";
                            strData += Ojw.CConvert.IntToStr(nTime) + "," + Ojw.CConvert.IntToStr((int)afDatas[1]) + ",";
                            for (int i = 2; i < afDatas.Length; i += 2)
                            {
                                strData += Ojw.CConvert.IntToStr((int)afDatas[i]) + ":" + Ojw.CConvert.FloatToStr(afDatas[i + 1]);
                                if ((i + 2) < afDatas.Length) strData += ",";
                            }

                            PlayFrameString(strData, true);
                            return true;
                        }

                    }
                    catch (Exception e)
                    {
                        Ojw.CMessage.Write_Error(strData + ": " + e.ToString());
                        return false;
                    }
                }
                return false;
            }
            public bool m_bSimulOnly = false;
            public void SetSimulation_Only(bool bSimul)
            {
                m_bSimulOnly = bSimul;
            }
            public bool SetSimulation_Only()
            {
                return m_bSimulOnly;
            }
            //public void Play_Pos(int nTime, string buff, bool bNoWait = false) { PlayFrameString("s2," + Ojw.CConvert.IntToStr(nTime) + ",0," + buff, bNoWait);  }
            public void Play_Pos(string buff, bool bNoWait = false) { PlayFrameString("s2," + buff, bNoWait); }
            public void Play_Speed(string buff, bool bRpm = true) { PlayFrameString((bRpm?"s4,0,0,":"s3,0,0,") + buff, true); }
            public void PlayFrameString(string buff, bool bNoWait = false)
            {
                //if (IsOpen() == false) return;
                //Ojw.printf("TEST===>\r\n");
                //Ojw.printf("PlayFrameString({0})\r\n", buff);
                //Ojw.newline();

                bool bWheel = false;
                if (buff.Length > 1)
                {
                    if (
                        ((buff[1] == '1') || (buff[1] == '2')) // Enable
                        ||
                        ((buff[1] == '3') || (buff[1] == '4')) // Enable
                        || ((buff[1] == '5')) // Write
                    )
                    {
                        bool bAngle = false;
                        bool bWheel_Rpm = false;
                        bool bWrite = false;
                        if ((buff[1] == '3') || (buff[1] == '4')) bWheel = true;
                        if (buff[1] == '2') bAngle = true;
                        if (buff[1] == '4') bWheel_Rpm = true;
                        if (buff[1] == '5') bWrite = true;

                        string[] pstr = buff.Split(',');

                        int nEnable = Ojw.CConvert.StrToInt(pstr[0]);
                        int nTime = Ojw.CConvert.StrToInt(pstr[1]);
                        int nDelay = Ojw.CConvert.StrToInt(pstr[2]);

                        if (bWrite) // 해당 주소에 직접 지령
                        {
                            //int nAddress = nTime;
                            //int nSize = nDelay;
                            // ID : Address : Bytes...
                            for (int nIndex = 3; nIndex < pstr.Length; nIndex++)
                            {
                                //Ojw.CMessage.Write("kkkkkkkkkkk");
                                string[] pstrDatas = pstr[nIndex].Split(':');
                                if (pstrDatas.Length > 1) // id : value : size = 1
                                {
                                    int nID = Ojw.CConvert.StrToInt(pstrDatas[0]);
                                    int nAddress = Ojw.CConvert.StrToInt(pstrDatas[1]);
                                    List<byte> lstBytes = new List<byte>();
                                    lstBytes.Clear();
                                    for (int i = 2; i < pstrDatas.Length; i++)
                                    {
                                        lstBytes.Add((byte)(Ojw.CConvert.StrToInt(pstrDatas[i]) & 0xff));
                                    }
                                    Send(nID, 0x03, nAddress, lstBytes.ToArray());
                                    //CMessage.Write("test - > ID={0}, Address-{1}, Val={2}", nID, nAddress, lstBytes[0]);
                                }
                            }
                        }
                        else
                        {
                            Command_Clear();
                            int nCnt_Func = 256;
                            float[] afX = new float[nCnt_Func];
                            float[] afY = new float[nCnt_Func];
                            float[] afZ = new float[nCnt_Func];
                            bool[] abFunction = new bool[nCnt_Func];
                            Array.Clear(abFunction, 0, abFunction.Length);

                            for (int nIndex = 3; nIndex < pstr.Length; nIndex++)
                            {
                                string[] pstrDatas = pstr[nIndex].Split(':');
                                if (pstrDatas.Length > 1)
                                {
                                    int nXyz = -1;
                                    int nFunctionNumber = -1;
                                    // 좌표제어
                                    if (pstrDatas[0].ToUpper().IndexOf("X") == 0)       { nXyz = 0; } 
                                    else if (pstrDatas[0].ToUpper().IndexOf("Y") == 0)  { nXyz = 1; }
                                    else if (pstrDatas[0].ToUpper().IndexOf("Z") == 0)  { nXyz = 2; }
                                    #region etc
                                    else
                                    {
                                        int nID = Ojw.CConvert.StrToInt(pstrDatas[0]);

                                        float fEvd = Ojw.CConvert.StrToFloat(pstrDatas[1]);
                                        if ((bAngle) || (bWheel_Rpm))
                                        {
                                            //Ojw.printf("=====1->\r\n");
                                            if (bWheel_Rpm) Command_Set_Rpm(nID, fEvd);
                                            else
                                            {
                                                //Ojw.printf("=====123->\r\n");
                                                Command_Set(nID, fEvd);

                                                //3D
                                                if (m_bSyncRendering) m_C3d.SetData(nID, fEvd);
                                            }
                                        }
                                        else
                                        {
                                            //Ojw.printf("=====2->\r\n");
                                            int nEvd = Ojw.CConvert.StrToInt(pstrDatas[1]);
                                            float fAngle = (float)Math.Round(CalcEvd2Angle(nID, nEvd), 3);
                                            if (bWheel)
                                            {
                                                if (bWheel_Rpm) Command_Set(nID, nEvd);
                                                else Command_Set(nID, CalcRaw2Rpm(nID, nEvd));
                                            }
                                            else Command_Set(nID, fAngle);
                                            //3D
                                            if (m_bSyncRendering) m_C3d.SetData(nID, fAngle);
                                        }
                                    }
                                    #endregion etc

                                    if (nXyz >= 0)
                                    {
                                        nFunctionNumber = Ojw.CConvert.StrToInt(pstrDatas[0].Substring(1));
                                        float[] afFunc = ((nXyz == 0) ? afX : ((nXyz == 1) ? afY : afZ));
                                        if ((nFunctionNumber >= 0) && (nFunctionNumber < afX.Length))
                                        {
                                            abFunction[nFunctionNumber] = true;
                                            afFunc[nFunctionNumber] = Ojw.CConvert.StrToFloat(pstrDatas[1]);
                                        }
                                    }
                                }
                            }

                            ////
                            for (int i = 0; i < nCnt_Func; i++)
                            {
                                if (abFunction[i] == true)
                                {
                                    int[] anIDs;
                                    double[] adValues;
                                    float fX, fY, fZ;
                                    m_C3d.CalcF(i, false, out fX, out fY, out fZ);
                                    if (m_C3d.IsInverseFunction(i) == true)
                                    {
                                        m_C3d.GetData_Inverse(i, (double)fX, (double)fY, (double)fZ, out anIDs, out adValues);

                                    }
                                    else
                                    {
                                        m_C3d.CalcInv(i, fX, fY, fZ);
                                        anIDs = m_C3d.GetInfo_Forward_Motors(i);
                                        adValues = new double[anIDs.Length];
                                        for (int j = 0; j < anIDs.Length; j++)
                                        {
                                            adValues[anIDs[j]] = (double)m_C3d.GetData(anIDs[j]);
                                        }
                                    }
                                    for (int j = 0; j < anIDs.Length; j++)
                                    {
                                        Command_Set(anIDs[j], (float)adValues[j]);
                                        if (m_bSyncRendering) m_C3d.SetData(anIDs[j], (float)adValues[j]);
                                    }
                                }
                            }
                            ////
                            if (m_bSimulOnly == false)
                            {
                                if (bWheel)
                                {
                                    SetSpeed(m_lstCmdIDs.ToArray());
                                }
                                else
                                {
                                    if (bNoWait == false) Move(nTime, nDelay, m_lstCmdIDs.ToArray());
                                    else Move_NoWait(nTime, nDelay, m_lstCmdIDs.ToArray());
                                }
                            }
                        }

                        //if (m_bSyncRendering)
                        //    m_C3d.OjwDraw();
                    }
                }
            }
            // 마지막 모션이 아니라면 bContinue = false

            public bool Move(params float [] afVals)
            {
                //string strErr = "";
                //try
                //{
                    // (afVals.Length - 1) 은 안전장치
                    int nLen_Off = afVals.Length % 2 * 2;
                    int nLen = (int)(Math.Round((afVals.Length - nLen_Off) / 2.0f)) * 2;
                    if (nLen < 4) return false;
                    int nTime_ms = (int)afVals[0];
                    int nDelay = (int)afVals[1];
                    if (nTime_ms > 0)
                    {
                        CCommand_t[] aCCommands = new CCommand_t[nLen / 2 - 1];
                        //strErr += aCCommands.Length.ToString() + ",";
                        for (int i = 2; i < nLen; i += 2)
                        {
                            //strErr += i.ToString() + ","; 
                            aCCommands[i / 2 - 1] = new CCommand_t((int)afVals[i], afVals[i + 1]);
                        }
                        Move(nTime_ms, nDelay, aCCommands);
                    }
                    else
                    {
                        for (int i = 2; i < nLen; i += 2) Command_Set_Rpm((int)afVals[i], afVals[i + 1]);
                        SetSpeed(m_lstCmdIDs.ToArray());
                    }
                    return true;
                //    return "Good";
                //}
                //catch (Exception ex)
                //{
                //    return strErr + ":" + ex.ToString();
                //}
            }
            //public void Move(int nTime_ms, int nDelay, bool bContinue = false, params CCommand_t[] aCCommands)
            public void Move(int nTime_ms, int nDelay, params CCommand_t[] aCCommands) { Move(nTime_ms, nDelay, false, aCCommands); }
            public void Move(int nTime_ms, int nDelay, bool bContinue, params CCommand_t[] aCCommands)
            {
                m_nWait_Time = 0;
                if (IsOpen() == false) return;
                if (m_bEms == true) return;
                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();

                CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                Command_Clear();
                if (CCmd.Length > 0)
                {
                    float[] afMot = new float[m_afMot.Length];
                    float[] afRes = new float[m_afMot.Length];
                    Array.Copy(m_afMot, afMot, m_afMot.Length);
                    while (true)
                    {
                        if (m_bEms == true)
                        {
                            //SyncRead(lstIDs.ToArray());
                            for (int i = 0; i < CCmd.Length; i++) { m_afMot_Pose[CCmd[i].nID] = m_afMot[CCmd[i].nID] = afRes[CCmd[i].nID]; }
                            return;
                        }

                        //int nTimer = nTime_ms + nDelay;
                        List<int> lstIDs = new List<int>();
                        lstIDs.Clear();
                        for (int i = 0; i < CCmd.Length; i++) { lstIDs.Add(CCmd[i].nID); Command_Set(CCmd[i].nID, 0); } //CalcPosition_Time(CCmd[i].nID, nTime_ms, nDelay, CCmd[i].fVal)); }
                        SetPosition_Speed(m_lstCmdIDs.ToArray());

                        float fGet = CTmr.Get();
                        float fTmr = (fGet / (float)nTime_ms);
                        if (fTmr > 1f) fTmr = 1f;

                        // -Delay 탈출
                        //if (bContinue == true)
                        //{
                        //    float fTmr_Sub = (nDelay >= 0) ? 0 : (fGet / (float)(nTime_ms + nDelay));
                        //    if (fTmr_Sub >= 1f) break;
                        //}
                        




                        for (int i = 0; i < CCmd.Length; i++) { afRes[CCmd[i].nID] = afMot[CCmd[i].nID] + (CCmd[i].fVal - afMot[CCmd[i].nID]) * fTmr; Command_Set(CCmd[i].nID, afRes[CCmd[i].nID]); }
                        SetPosition(m_lstCmdIDs.ToArray());
                        
                        //if (nDelay < 0)
                        //{
                        if (fGet >= (nTime_ms + nDelay)) // 남은 시간값으로 마저 이동
                        {
                            lstIDs.Clear();
                            for (int i = 0; i < CCmd.Length; i++) { lstIDs.Add(CCmd[i].nID); Command_Set(CCmd[i].nID, CalcPosition_Time(CCmd[i].nID, (int)Math.Round(nTime_ms - fGet), 0, CCmd[i].fVal)); }//(int)Math.Round((nTime_ms - fGet) / nTime_ms)); }
                            SetPosition_Speed(m_lstCmdIDs.ToArray());

                            for (int i = 0; i < CCmd.Length; i++) { afRes[CCmd[i].nID] = CCmd[i].fVal; Command_Set(CCmd[i].nID, afRes[CCmd[i].nID]); }
                            SetPosition(m_lstCmdIDs.ToArray());
                            break;
                        }
                        //}
                        
                        if (fTmr >= 1f) break;
                        Ojw.CTimer.DoEvent();
                    }
                    for (int i = 0; i < CCmd.Length; i++) { m_afMot_Pose[CCmd[i].nID] = m_afMot[CCmd[i].nID] = afRes[CCmd[i].nID]; }

                    while (true)
                    {
                        //SyncRead(lstIDs.ToArray());
                        if (CTmr.Get() >= (nTime_ms + nDelay)) break;
                        Ojw.CTimer.DoEvent();
                    }
                }
            }

            private int m_nWait_Time = 0;
            public void Wait(int nTime = -1)
            {
                if (IsOpen() == false) return;
                if (m_bEms == true) return;

                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();

                int nWait = ((nTime < 0) ? m_nWait_Time : nTime);
                m_nWait_Time = 0;
                while (true) { if (CTmr.Get() >= nWait) break; Ojw.CTimer.DoEvent(); }
            }
            //public void Move_NoWait(int nTime_ms, int nDelay, params float[] afVals)
            //{
            //    int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
            //    CCommand_t[] aCCommands = new CCommand_t[nLen];
            //    for (int i = 0; i < nLen; i += 2) { aCCommands[i] = new CCommand_t((int)afVals[i], afVals[i + 1]); }
            //    Move_NoWait(nTime_ms, nDelay, aCCommands);
            //}
            public bool Move_NoWait(params float[] afVals)
            {
                //string strErr = "";
                //try
                //{
                // (afVals.Length - 1) 은 안전장치
                int nLen_Off = afVals.Length % 2 * 2;
                int nLen = (int)(Math.Round((afVals.Length - nLen_Off) / 2.0f)) * 2;
                if (nLen < 4) return false;
                int nTime_ms = (int)afVals[0];
                int nDelay = (int)afVals[1];
                if (nTime_ms > 0)
                {
                    CCommand_t[] aCCommands = new CCommand_t[nLen / 2 - 1];
                    //strErr += aCCommands.Length.ToString() + ",";
                    for (int i = 2; i < nLen; i += 2)
                    {
                        //strErr += i.ToString() + ","; 
                        aCCommands[i / 2 - 1] = new CCommand_t((int)afVals[i], afVals[i + 1]);
                    }
                    Move_NoWait(nTime_ms, nDelay, aCCommands);
                }
                else
                {
                    for (int i = 2; i < nLen; i += 2) Command_Set_Rpm((int)afVals[i], afVals[i + 1]);
                    SetSpeed(m_lstCmdIDs.ToArray());
                }
                return true;
            }
            public void Move_NoWait(int nTime_ms, int nDelay, params CCommand_t[] aCCommands)
            {
                if (IsOpen() == false) return;
                if (m_bEms == true) return;

                m_nWait_Time = (nTime_ms + nDelay);

                CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                Command_Clear();
                if (CCmd.Length > 0)
                {
                    float[] afMot = new float[m_afMot.Length];
                    float[] afRes = new float[m_afMot.Length];
                    Array.Copy(m_afMot, afMot, m_afMot.Length);

                    List<int> lstIDs = new List<int>();
                    lstIDs.Clear();
                    Command_Clear();
                    for (int i = 0; i < CCmd.Length; i++)
                    {
                        lstIDs.Add(CCmd[i].nID);
                        Command_Set(CCmd[i].nID, CalcPosition_Time(CCmd[i].nID, nTime_ms, nDelay, CCmd[i].fVal));
                    }
                    SetPosition_Speed(m_lstCmdIDs.ToArray());

                    Command_Clear();
                    for (int i = 0; i < CCmd.Length; i++)
                    {
                        Command_Set(CCmd[i].nID, CCmd[i].fVal);
                    }
                    SetPosition(m_lstCmdIDs.ToArray());
                }
            }
            //public void WaitMotion(int nTime, int [] anIDs = null)
            //{
            //    if (m_bEms == true) return;
            //    Ojw.CTimer CTmr = new CTimer();
            //    CTmr.Set();
            //    while (true)
            //    {
            //        if (m_bEms == true) return;
            //        if (anIDs != null) SyncRead(anIDs);
            //        if (CTmr.Get() >= nTime) break;
            //        Ojw.CTimer.DoEvent();
            //    }
            //}
            //public void WaitMotion(Ojw.CTimer CTmr, int nTime, int[] anIDs = null)
            //{
            //    if (m_bEms == true) return;
            //    while (true)
            //    {
            //        if (m_bEms == true) return;
            //        if (anIDs != null) SyncRead(anIDs);
            //        if (CTmr.Get() >= nTime) break;
            //        Ojw.CTimer.DoEvent();
            //    }
            //}
            //public void Wheel(params CCommand_t[] aCCommands)
            //{
            //}
            public void Wheel(float fRpm, params float[] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                //for (int i = 0; i < nLen; i += 2) { aCCommands[i] = new CCommand_t((int)afVals[i], afVals[i + 1]); }
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i * 2], afVals[i * 2 + 1]); }
                Wheel(fRpm, aCCommands);
            }
            public void Wheel(float fRpm, params CCommand_t[] aCCommands)
            {
                if (IsOpen() == false) return;
                if (m_bEms == true) return;

                CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                Command_Clear();
                if (CCmd.Length > 0)
                {
                    List<int> lstIDs = new List<int>();
                    lstIDs.Clear();
                    for (int i = 0; i < CCmd.Length; i++) { lstIDs.Add(CCmd[i].nID); }

                    for (int i = 0; i < CCmd.Length; i++) { Command_Set(CCmd[i].nID, 0); CalcRpm2Raw(CCmd[i].nID, fRpm); }
                    SetSpeed(m_lstCmdIDs.ToArray());
                }
            }
            public void SetSpeed(params float[] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                //for (int i = 0; i < nLen; i += 2) { aCCommands[i] = new CCommand_t((int)afVals[i], afVals[i + 1]); }
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i * 2], afVals[i * 2 + 1]); }
                SetSpeed(aCCommands);
            }
            public void SetSpeed(params CCommand_t[] aCCommands)
            {
                List<CCommand_t> lstSecond = new List<CCommand_t>();
                //for (int nIter = 0; nIter < 2; nIter++)
                while (true)
                {
                    Sync_Clear();
                    //bool bRes = false;
                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    Command_Clear();
                    if (lstSecond.Count > 0)
                    {
                        CCmd = lstSecond.ToArray();
                        lstSecond.Clear();
                    }
                    if (CCmd == null) break;
                    if (CCmd.Length > 0)
                    {
                        for (int i = 0; i < CCmd.Length; i++)
                        {
                            float fMul = m_aCParam[CCmd[i].nID].m_fMulti * ((m_aCParam[CCmd[i].nID].m_bDirReverse == false) ? 1 : -1);
                            if (m_aCParam[CCmd[0].nID].m_nSet_Speed_Address != m_aCParam[CCmd[i].nID].m_nSet_Speed_Address)
                            {
                                lstSecond.Add(new CCommand_t(CCmd[i].nID, CCmd[i].fVal * fMul));
                            }
                            else Sync_Push_Dword(CCmd[i].nID, (int)Math.Round(CCmd[i].fVal * fMul));
                        }
                        Sync_Flush(m_aCParam[CCmd[0].nID].m_nSet_Speed_Address);
                    }
                    if (lstSecond.Count == 0) break;
                }
                //Command_Clear();
            }

            public void SetPosition_Speed(params float[] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                //for (int i = 0; i < nLen; i += 2) { aCCommands[i] = new CCommand_t((int)afVals[i], afVals[i + 1]); }
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i * 2], afVals[i * 2 + 1]); }
                SetPosition_Speed(aCCommands);
            }
            public void SetPosition_Speed() { SetPosition_Speed(m_lstCmdIDs.ToArray()); }
            public void SetPosition_Speed(params CCommand_t[] aCCommands)
            {
                List<CCommand_t> lstSecond = new List<CCommand_t>();
                //for (int nIter = 0; nIter < 2; nIter++)
                while (true)
                {
                    Sync_Clear();
                    bool bRes = false;
                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    if (lstSecond.Count > 0)
                    {
                        CCmd = lstSecond.ToArray();
                        lstSecond.Clear();
                    }
                    if (CCmd == null) break;
                    if (CCmd.Length > 0)
                    {
                        for (int i = 0; i < CCmd.Length; i++)
                        {
                            if (m_aCParam[CCmd[0].nID].m_nSet_Position_Speed_Address != m_aCParam[CCmd[i].nID].m_nSet_Position_Speed_Address)
                            {
                                lstSecond.Add(new CCommand_t(CCmd[i].nID, CCmd[i].fVal));
                            }
                            else
                            {
                                int nVal = (int)Math.Round(CCmd[i].fVal);
                                
                                // ojw5014
                                if (nVal == 0) nVal = m_aCParam[CCmd[i].nID].m_nMax_Speed_For_Position;

                                Sync_Push_Dword(CCmd[i].nID, nVal);
                            }
                        }
                        Sync_Flush(m_aCParam[CCmd[0].nID].m_nSet_Position_Speed_Address);
                    }
                    if (lstSecond.Count == 0) break;
                }
                Command_Clear();
            }
            /*public void SetPosition_Speed(params CCommand_t [] aCCommands)
            {
                Sync_Clear();
                if (aCCommands.Length > 0)
                {
                    for (int i = 0; i < aCCommands.Length; i++)
                    {
                        Sync_Push_Dword(aCCommands[i].nID, (int)Math.Round(aCCommands[i].fVal));
                    }
                    Sync_Flush(m_aCParam[aCCommands[0].nID].m_nSet_Position_Speed_Address);
                    //aCCommands.Clear();
                }
                else if (m_lstCmdIDs.Count > 0)
                {
                    for (int i = 0; i < m_lstCmdIDs.Count; i++)
                    {
                        Sync_Push_Dword(m_lstCmdIDs[i].nID, (int)Math.Round(m_lstCmdIDs[i].fVal));
                    }
                    Sync_Flush(m_aCParam[m_lstCmdIDs[0].nID].m_nSet_Position_Speed_Address);
                }
                Command_Clear();
            }*/
            /*public void SetPosition(params CCommand_t [] aCCommands)
            {
                Sync_Clear();
                if (aCCommands.Length > 0)
                {
                    for (int i = 0; i < aCCommands.Length; i++)
                    {
                        Sync_Push_Dword(aCCommands[i].nID, CalcAngle2Evd(aCCommands[i].nID, aCCommands[i].fVal));
                    }
                    Sync_Flush(m_aCParam[aCCommands[0].nID].m_nSet_Position_Address);
                    //aCCommands.Clear();
                }
                else if (m_lstCmdIDs.Count > 0)
                {
                    for (int i = 0; i < m_lstCmdIDs.Count; i++)
                    {
                        Sync_Push_Dword(m_lstCmdIDs[i].nID, CalcAngle2Evd(m_lstCmdIDs[i].nID, m_lstCmdIDs[i].fVal));
                    }
                    Sync_Flush(m_aCParam[m_lstCmdIDs[0].nID].m_nSet_Position_Address);
                }
                Command_Clear();
            }*/
            public void SetPosition(params float [] afVals)
            {
                int nLen = (int)Math.Round(((float)afVals.Length - 0.5f) / 2.0f);
                CCommand_t[] aCCommands = new CCommand_t[nLen];
                //for (int i = 0; i < nLen; i += 2) { aCCommands[i] = new CCommand_t((int)afVals[i], afVals[i + 1]); }
                for (int i = 0; i < nLen; i++) { aCCommands[i] = new CCommand_t((int)afVals[i * 2], afVals[i * 2 + 1]); }
                SetPosition(aCCommands);
            }
            public void SetPosition() { SetPosition(m_lstCmdIDs.ToArray()); }
            public void SetPosition(params CCommand_t[] aCCommands)
            {
                List<CCommand_t> lstSecond = new List<CCommand_t>();
                //for (int nIter = 0; nIter < 2; nIter++)
                while (true)
                {
                    Sync_Clear();
                    bool bRes = false;
                    CCommand_t[] CCmd = ((aCCommands.Length > 0) ? aCCommands : ((m_lstCmdIDs.Count > 0) ? m_lstCmdIDs.ToArray() : null));
                    if (lstSecond.Count > 0)
                    {
                        CCmd = lstSecond.ToArray();
                        lstSecond.Clear();
                    }
                    if (CCmd == null) break;
                    if (CCmd.Length > 0)
                    {
                        for (int i = 0; i < CCmd.Length; i++)
                        {
                            if (m_aCParam[CCmd[0].nID].m_nSet_Position_Address != m_aCParam[CCmd[i].nID].m_nSet_Position_Address)
                            {
                                lstSecond.Add(new CCommand_t(CCmd[i].nID, CCmd[i].fVal));
                            }
                            else
                            {
                                m_afMot[CCmd[i].nID] = CCmd[i].fVal;
                                m_afMot_Pose[CCmd[i].nID] = CCmd[i].fVal;
                                Sync_Push_Dword(CCmd[i].nID, CalcAngle2Evd(CCmd[i].nID, CCmd[i].fVal));
                                //3D
                                if (m_bSyncRendering) m_C3d.SetData(CCmd[i].nID, CCmd[i].fVal);
                            }
                        }
                        Sync_Flush(m_aCParam[CCmd[0].nID].m_nSet_Position_Address);
                    }
                    if (lstSecond.Count == 0) break;
                }
                Command_Clear();
            }
            #endregion Command

            #region Calc
            public float CalcEvd2Angle(int nID, int nValue)
            {
                float fMul = m_aCParam[nID].m_fMulti * ((m_aCParam[nID].m_bDirReverse == false) ? 1 : -1);
                if (fMul == 0) fMul = 1;
                float fMechMove = m_aCParam[nID].m_fMechMove;//4096.0;
                float fCenterPos = m_aCParam[nID].m_fCenter;//2048.0;
                float fMechAngle = m_aCParam[nID].m_fMechAngle;//360.0;

                return (((fMechAngle * ((float)nValue - fCenterPos)) / fMechMove) * fMul);
            }
            public int CalcAngle2Evd(int nID, float fValue)
            {
                float fMul = m_aCParam[nID].m_fMulti * ((m_aCParam[nID].m_bDirReverse == false) ? 1 : -1);
                if (fMul == 0) fMul = 1;

                float fMechMove = m_aCParam[nID].m_fMechMove;//4096.0;
                float fCenterPos = m_aCParam[nID].m_fCenter;//2048.0;
                float fMechAngle = m_aCParam[nID].m_fMechAngle;//360.0;
                return (int)Math.Round(((fMechMove * fValue) / fMechAngle * fMul + fCenterPos));
            }
            public int CalcPosition_Time(int nAxis, int nTime, int nDelay, float fAngle)
            {
                //float fPer = 1f;
                //if (nDelay < 0) fPer = ((float)(nTime + nDelay) / (float)nTime); 

                float fRpm = (float)Math.Abs(CalcTime2Rpm(Math.Abs(fAngle - m_afMot_Pose[nAxis]), (float)nTime));
                //float fRpm = (float)Math.Abs(CalcTime2Rpm(Math.Abs(fAngle - m_afMot[nAxis]) * fPer, (float)nTime));
                return CalcRpm2Raw(nAxis, fRpm);
            }
            public float CalcRaw2Rpm(int nID, int nValue) { return (float)nValue * m_aCParam[nID].m_fJointRpm; }
            public int CalcRpm2Raw(int nID, float fRpm) { return (int)Math.Round(fRpm / m_aCParam[nID].m_fJointRpm); }
            public float CalcTime2Rpm(float fDeltaAngle, float fTime)
            {
                //if (fDeltaAngle == 0) fDeltaAngle = (float)CMath._ZERO;
                #region Kor
                // 1도 이동시간 => fTime / fDeltaAngle

                // 60 초 동안 _MAX_RPM 을 회전하는 것이 RPM, 1도 움직이는 것을 체크하려면 여기에 360 도 회전값을 고려해 주어야 한다.
                // _MAX_RPM 은 1분(60초) 동안 _MAX_RPM 바퀴 (즉, 360 * _MAX_RPM) 를 회전한 값
                // _MAX_RPM * 360 : 60 seconds => 480(_MAX_EV_RPM) 일때 
                // => 1초간 6 * _MAX_RPM 도 회전, 1ms => _MAX_RPM * 360도 / 60000ms = 0.65952 도 이동
                // => 1도 움직이는데 60000 / (_MAX_RPM * 360) = 1.516254246 ms 가 필요

                // 1도 이동시간 => 60000 / (Rpm * 360)
                // 이동각도 당 이동시간 계산 => 1도 이동시간 * fDeltaAngle
                #endregion Kor
                //return 60000 / (fTime / fDeltaAngle * 360.0f);
                return (60.0f * fDeltaAngle * 1000.0f) / (360.0f * fTime);
            }
            #endregion Calc

            #region Read Command
            private int m_nRequest_Address = 0;
            private int m_nRequest_Address_Size = 0;
            public void SyncRead_With_Address(int nAddress, int nSize, params int[] anIDs)
            {
                Request_Clear();
                for (int i = 0; i < anIDs.Length; i++) Request_Push(anIDs[i]);
                Request_Flush(nAddress, nSize);
                WaitReceive();
            }
            //public void Read(int nWaitTime, params int[] anIDs)
            //{
            //    Ojw.CTimer CTmr = new CTimer();
            //    CTmr.Set();
            //    SyncRead_NoWait(anIDs);
            //    int nTmr = 0;
            //    int nInterval = 100;
            //    while (nTmr <= nWaitTime)
            //    {
            //        if (CTmr.Get() >= nInterval)
            //        {
            //            nTmr += (int)CTmr.Get();
            //            CTmr.Set();
            //            SyncRead_NoWait(anIDs);
            //        }
            //        Ojw.CTimer.Wait(1);
            //    }
            //}
            //public void SyncRead_NoWait(params int[] anIDs)
            //{
            //    List<int> lstSecond = new List<int>();
            //    int[] anIDsCurr;
            //    while (true)
            //    {
            //        Request_Clear();
            //        anIDsCurr = ((lstSecond.Count > 0) ? lstSecond.ToArray() : anIDs);
            //        lstSecond.Clear();
            //        if (anIDsCurr.Length > 0)
            //        {
            //            for (int i = 0; i < anIDsCurr.Length; i++)
            //            {
            //                if (m_aCParam[anIDsCurr[0]].m_nGet_Position_Address != m_aCParam[anIDsCurr[i]].m_nGet_Position_Address) lstSecond.Add(anIDsCurr[i]);
            //                else Request_Push(anIDsCurr[i]);
            //            }
            //            Request_Flush(m_aCParam[anIDsCurr[0]].m_nGet_Position_Address, m_aCParam[anIDsCurr[0]].m_nGet_Position_Size);
            //        }
            //        else break;
            //        if (lstSecond.Count == 0) break;
            //    }
            //    Command_Clear();
            //}
            private List<int> lstRequestIDs = new List<int>();
            public bool SyncRead(params int[] anIDs)
            {
                bool bRet = false;
                List<int> lstSecond = new List<int>();
                while (true)
                {
                    Request_Clear();
                    int[] anIDsCurr = ((lstSecond.Count > 0) ? lstSecond.ToArray() : anIDs);
                    lstSecond.Clear();
                    if (anIDsCurr.Length > 0)
                    {　　
                        for (int i = 0; i < anIDsCurr.Length; i++)
                        {
                            //Request_Push(anIDsCurr[i]);
                            if (m_aCParam[anIDsCurr[0]].m_nGet_Position_Address != m_aCParam[anIDsCurr[i]].m_nGet_Position_Address)
                            {
                                lstSecond.Add(anIDsCurr[i]);
                            }
                            else Request_Push(anIDsCurr[i]);
                        }
                        Request_Flush(m_aCParam[anIDsCurr[0]].m_nGet_Position_Address, m_aCParam[anIDsCurr[0]].m_nGet_Position_Size);
                        bRet = WaitReceive(anIDs);
                    }
                    else break;
                    if (lstSecond.Count == 0) break;
                }
                Command_Clear();
                return bRet;
            }
            private void Request(int nMotor, int nCommand, byte[] pbyDatas) { Request_with_RealID(nMotor, nCommand, pbyDatas); }

            private List<int> m_lstRequestMotors = new List<int>();// = new int[];
            public void Request_Push(int nMotor) { m_lstRequestMotors.Add(nMotor); }
            public void Request_Clear() { m_lstRequestMotors.Clear(); }
            int m_nRequestMotors = 0;
            public void Request_Flush(int nAddress, int nSize)
            {
                byte[] pbyDatas = new byte[4 + m_lstRequestMotors.Count];
                int nPos = 0;
                pbyDatas[nPos++] = (byte)(nAddress & 0xff);
                pbyDatas[nPos++] = (byte)(((nAddress >> 8) & 0xff));
                pbyDatas[nPos++] = (byte)(nSize & 0xff);
                pbyDatas[nPos++] = (byte)(((nSize >> 8) & 0xff));
                for (int i = 0; i < m_lstRequestMotors.Count; i++)
                {
                    pbyDatas[nPos++] = (byte)(m_lstRequestMotors[i] & 0xff);
                }
                m_nRequestMotors = m_lstRequestMotors.Count;

                m_nRequest_Address = nAddress;
                m_nRequest_Address_Size = nSize;

                Request(254, 0x82, pbyDatas);
                Request_Clear();
            }
            private void Request_with_RealID(int nMotorRealID, int nCommand, byte[] pbyDatas)
            {
                int i = 0;
                int nDataLength = 0;
                if (pbyDatas != null)
                {
                    if (pbyDatas.Length > 0)
                        nDataLength = pbyDatas.Length;
                }
                byte[] pbyteBuffer;

                //bool bSend = false;

                int nLength = 3;
                nLength += ((nDataLength > 0) ? nDataLength + 0 : 0);
                int nDefaultSize = 7;
                pbyteBuffer = new byte[nDefaultSize + nLength];
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xff;
                // region Packet 2.0
                pbyteBuffer[i++] = 0xfd;
                pbyteBuffer[i++] = 0x00;
                // endregion Packet 2.0
                pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                pbyteBuffer[i++] = (byte)(nLength & 0xff);
                pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                if (nDataLength > 0)
                {
                    // pbyteBuffer[i++] = (nAddress & 0xff);
                    // pbyteBuffer[i++] = ((nAddress >> 8) & 0xff);

                    for (int j = 0; j < nDataLength; j++)
                        pbyteBuffer[i++] = pbyDatas[j];
                }
                MakeStuff(ref pbyteBuffer);
                int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);
                //bSend = true;

                SendPacket(pbyteBuffer, pbyteBuffer.Length);
            }
            #endregion Read Command

            #region Reboot / Reset
            private bool m_bEms = false; // emergency switch
            public void Ems()
            {
                m_bEms = true;
                Send(254, 0x03, 64, 0);
                Send(254, 0x03, 512, 0);

                Send(254, 0x03, 64, 1);
                Send(254, 0x03, 512, 1);

                //int[] anIDs = m_C3d.GetHeader().anMotorIDs;
                //for (int i = 0; i < anIDs.Length; i++) { Command_Set(anIDs[i], 0); }
                //SetTorq();
                //for (int i = 0; i < anIDs.Length; i++) { Command_Set(anIDs[i], 1); }
                //SetTorq();
            }
            private bool m_bBreak = false;
            private bool m_bBreak2 = false;
            private void SetBreak() { m_bBreak = true; }
            public void Reboot(int nMotor = 254)
            {
                if (nMotor == 254) { for (int i = 0; i < m_abMot.Length; i++) { m_abMot[i] = false; } }
                else { m_abMot[nMotor] = false; }
                Send_Command(nMotor, 0x08);
            }
            public void Send_Command(int nMotor, int nCommand)
            {
                Send(nMotor, nCommand, 0, null);
            }
            #endregion Reboot / Reset

            #region Read
            const int _WAIT_TIME = 5000; // ms
            private int m_nShowReturnPacket = 0;//1;//0; // 테스트... 나중에 0 으로 기본값 줄 것
            public void ShowPacketReturn(int nPacket_0_Disable_1_Enable) { m_nShowReturnPacket = nPacket_0_Disable_1_Enable; }

            public bool WaitReceive(int [] anIDs = null)
            {
                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();
                if (anIDs != null)
                {
                    lstRequestIDs.Clear();
                    lstRequestIDs.AddRange(anIDs);
                }
                while (true)
                {
                    if (m_CSock_Client.IsConnect())
                    {
                        if (m_CSock_Client.GetBuffer_Length() > 0)
                        {
                            ReceivedPacket(m_CSock_Client.GetBytes());
                            if (m_CSock_Client.GetBuffer_Length() == 0)
                            {
                                if (lstRequestIDs.Count == 0) return true;
                            }
                        }
                    }
                    else if (m_CSerial.IsConnect())
                    {
                        if (m_CSerial.GetBuffer_Length() > 0)
                        {
                            //return m_CSerial.GetBytes();
                            ReceivedPacket(m_CSerial.GetBytes());
                            if (m_CSerial.GetBuffer_Length() == 0)
                            {
                                if (lstRequestIDs.Count == 0) return true;
                            }
                        }
                    }
                    if (CTmr.Get() >= _WAIT_TIME)
                    {
                        Ojw.LogErr("대기시간 초과");
                        break;
                    }
                    Ojw.CTimer.DoEvent();
                }
                return false;
            }
            public bool WaitReceive_Ext_Sock(Ojw.CSocket CSock, int nWaitTime = 0)
            {
                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();
                if (nWaitTime <= 0) nWaitTime = _WAIT_TIME;
                while (true)
                {
                    if (CSock.IsConnect())
                    {
                        if (CSock.GetBuffer_Length() > 0)
                        {
                            ReceivedPacket(CSock.GetBytes());
                            return true;
                        }
                    }
                    if (CTmr.Get() >= nWaitTime)
                    {
                        Ojw.LogErr("대기시간 초과");
                        break;
                    }
                    Ojw.CTimer.DoEvent();
                }
                return false;
            }

            public bool WaitReceive_Ext_Serial(Ojw.CSerial CSerial, int nWaitTime = 0)
            {
                Ojw.CTimer CTmr = new CTimer();
                CTmr.Set();
                if (nWaitTime <= 0) nWaitTime = _WAIT_TIME;
                while (true)
                {
                    if (CSerial.IsConnect())
                    {
                        if (CSerial.GetBuffer_Length() > 0)
                        {
                            ReceivedPacket(CSerial.GetBytes());
                            return true;
                        }
                    }
                    if (CTmr.Get() >= nWaitTime)
                    {
                        Ojw.LogErr("대기시간 초과");
                        break;
                    }
                    Ojw.CTimer.DoEvent();
                }
                return false;
            }

            class CReceive_t
            {
                public int nID = 0;
                public int nCmd = 0;
                public int nLength_Data = 0;
                public int nError = 0;
                public List<int> lstDatas = new List<int>();
            }
            int m_nReceive_Header = 0;
            int m_nReceive_Index = 0;
            int m_nReceive_ID = 0;
            int m_nReceive_Length = 0;
            int m_nReceive_Cmd = 0;
            int m_nReceive_Length_Check = 0;
            int m_nReceive_Error = 0;
            public List<int> m_anReceive_Datas = new List<int>();
            public int[] GetBuffers_Int() { return m_anReceive_Datas.ToArray(); }
            public byte[] GetBuffers() { return Array.ConvertAll(m_anReceive_Datas.ToArray(), element => (byte)element); }
            public void ReceivedPacket(byte[] buffer)
            {
                bool bShow_StrLetter = false;
                bool bShow_Str = (m_nShowReturnPacket == 0) ? false : true;
                byte[] value = buffer;
                int nPaketLength = value.Length;
                string str = "";
                string strLetter = "";
                int nHeader = 0;
                byte val;
                for (int i = 0; i < nPaketLength; i++)
                {
                    val = value[i];//.toString(16);
                    if (bShow_StrLetter)
                    {
                        if ((val >= 0x20) && (val <= 127))
                        {
                            strLetter += (char)(val);
                        }
                        else
                        {
                            strLetter += "(0x" + ("0" + Ojw.CConvert.IntToHex(16, 2)) + ")";
                        }
                    }
                    if (bShow_Str)
                        str += " 0x" + ("0" + Ojw.CConvert.IntToHex(16, 2)) + ",";
                    byte byData = val;
                    int nTmp = m_nReceive_Header % 100;
                    if (byData == 0xff)
                    {
                        m_nReceive_Header++;
                        if ((nTmp > 2) && (nTmp < 10))
                        {
                            if (m_nReceive_Header >= 100) m_nReceive_Header = 102;
                            else m_nReceive_Header = 2;
                        }
                    }
                    else if (nTmp == 2)
                    {
                        if (byData == 0xfd)
                        {
                            if (m_nReceive_Header >= 100) m_nReceive_Header = 110;
                            else m_nReceive_Header = 10;
                        }
                    }
                    else if (nTmp == 10)
                    {
                        if (byData == 0x00)
                        {
                            m_nReceive_Header = 100;
                            m_nReceive_Index = 1;
                            m_nReceive_ID = 0;
                            m_nReceive_Length = 0;
                            m_nReceive_Cmd = 0;
                            m_nReceive_Length_Check = 0;
                            m_nReceive_Error = 0;
                            m_anReceive_Datas.Clear();
                        }
                    }
                    else
                    {
                        if (m_nReceive_Header >= 100) m_nReceive_Header = 100;
                        else m_nReceive_Header = 0;
                    }

                    if (m_nReceive_Header >= 100)
                    {
                        switch (m_nReceive_Index)
                        {
                            case 1:
                                m_nReceive_Index++;
                                break;
                            case 2:
                                m_nReceive_ID = byData;
                                m_nReceive_Index++;
                                break;
                            case 3:
                                m_nReceive_Length = byData;
                                m_nReceive_Index++;
                                break;
                            case 4:
                                m_nReceive_Length += byData * 256;
                                m_nReceive_Index++;
                                m_nReceive_Length_Check = 0;
                                break;
                            case 5:
                                m_nReceive_Length_Check++;
                                m_nReceive_Cmd = byData;
                                m_nReceive_Index++;
                                break;
                            case 6:
                                m_nReceive_Length_Check++;
                                if (m_nReceive_Length > 3)
                                {
                                    m_nReceive_Error = byData;
                                    if (m_nReceive_Error != 0)
                                    {
                                        Ojw.Log("[ID:" + m_nReceive_ID + "]Received: ++++++Error 발생[Code:" + GetError(m_nReceive_Error) + "]+++++++");
                                        Ojw.Log(GetError(m_nReceive_Error));
                                        m_nReceive_Header = 0;
                                        m_nReceive_Index = 0;
                                        break;
                                    }
                                }
                                m_nReceive_Index++;
                                break;
                            case 7:
                                m_nReceive_Length_Check++;
                                if (m_nReceive_Length_Check <= m_nReceive_Length - 2)
                                {
                                    m_anReceive_Datas.Add(byData);
                                }
                                else
                                {
                                    CReceive_t CReceive = new CReceive_t();

                                    CReceive.nID = m_nReceive_ID;
                                    CReceive.nCmd = m_nReceive_Cmd;
                                    CReceive.nLength_Data = m_anReceive_Datas.Count;

                                    //if (m_nReceived_Policy == 1) // m_anMot 변수에 모터의 각도값을 즉각 반영
                                    //{
                                    if (m_nRequestMotors > 0)
                                    {
                                        if ((m_nReceive_ID >= 0) && (m_nReceive_ID < 253))
                                        {
                                            if (lstRequestIDs.Count > 0)
                                            {
                                                int nRequest = lstRequestIDs.IndexOf(m_nReceive_ID);
                                                if (nRequest >= 0)
                                                {
                                                    lstRequestIDs.RemoveAt(nRequest);
                                                }
                                            }
                                            byte[] pbyData = new byte[m_anReceive_Datas.Count];
                                            for (int nBuffer = 0; nBuffer < m_anReceive_Datas.Count; nBuffer++) { pbyData[nBuffer] = (byte)(m_anReceive_Datas[nBuffer] & 0xff); }

                                            int nVal = 0;
                                            switch (CReceive.nLength_Data)
                                            {
                                                case 1: nVal = (byte)(pbyData[0]); break;
                                                case 2: nVal = Ojw.CConvert.BytesToShort(pbyData, 0); break;
                                                case 4: nVal = Ojw.CConvert.BytesToInt(pbyData, 0); break;
                                            }

                                            if (m_nRequest_Address == m_aCParam[m_nReceive_ID].m_nSet_Position_Address)
                                            {
                                                m_anMot[m_nReceive_ID] = nVal;
                                                m_afMot[m_nReceive_ID] = CalcEvd2Angle(CReceive.nID, nVal);
                                                //Ojw.Log("[Receive]Set:{0}번 -> {1}({2}도)", m_nReceive_ID, nVal, m_afMot[m_nReceive_ID]);
                                            }
                                            else if (m_nRequest_Address == m_aCParam[m_nReceive_ID].m_nGet_Position_Address)
                                            {
                                                m_anMot_Pose[m_nReceive_ID] = nVal;
                                                m_afMot_Pose[m_nReceive_ID] = CalcEvd2Angle(CReceive.nID, nVal);

                                                m_anMot[m_nReceive_ID] = m_anMot_Pose[m_nReceive_ID];
                                                m_afMot[m_nReceive_ID] = m_afMot_Pose[m_nReceive_ID];
                                                //Ojw.Log("[Receive]Get:{0}번 -> {1}({2}도)", m_nReceive_ID, nVal, m_afMot[m_nReceive_ID]);
                                            }
                                            else if (m_nRequest_Address == m_aCParam[m_nReceive_ID].m_nSet_Torq_Address)
                                            {
                                                m_abMot[m_nReceive_ID] = ((nVal == 0) ? false : true);
                                            }
                                        }

                                        /*
                                        //if (CReceive.nLength_Data == 4)
                                        {
                                            if ((m_nReceive_ID >= 0) && (m_nReceive_ID < 253))
                                            {
                                                byte [] pbyData = new byte[m_anReceive_Datas.Count];
                                                for(int nBuffer = 0; nBuffer < m_anReceive_Datas.Count; nBuffer++) { pbyData[nBuffer] = (byte)(m_anReceive_Datas[nBuffer] & 0xff); }

                                                int nVal = Ojw.CConvert.BytesToInt(pbyData, 0);
                                                m_anMot[CReceive.nID] = nVal;
                                                m_afMot[CReceive.nID] = CalcEvd2Angle(CReceive.nID, nVal);
                                                //int nVal = m_anReceive_Datas[3] * 256 * 256 * 256 + m_anReceive_Datas[2] * 256 * 256 + m_anReceive_Datas[1] * 256 + m_anReceive_Datas[0];
                                                //if (
                                                //    (nVal >= 0) && 
                                                //    (nVal <= 4096)//g_CRobot.aCDxl[m_nReceive_ID].fMech_Move) // 360 도 이상은 돌지 않는다고 가정
                                                //)
                                                //{
                                                //    m_anMot[CReceive.nID] = nVal;
                                                //}
                                            }
                                        }
                                        */
                                        m_nRequestMotors--;
                                        if (m_nRequestMotors <= 0)
                                        {
                                            //Ojw.Log("m_nRequestMotors == {0}", m_nRequestMotors);
                                            //m_nSeq_Motors++;
                                            //m_nReceived_Policy = 0;
                                        }
                                    }
                                    else
                                    {
                                        //m_nSeq_Motors++;
                                        //m_nReceived_Policy = 0;
                                    }
                                    //}
                                    //else
                                    //{
                                    //    if (m_nRequestMotors > 0)
                                    //        {
                                    //            if (CReceive.nLength_Data == 4)
                                    //            {
                                    //                if ((m_nReceive_ID >= 0) && (m_nReceive_ID < 253))
                                    //                {
                                    //                    int nVal = m_anReceive_Datas[3] * 256 * 256 * 256 + m_anReceive_Datas[2] * 256 * 256 + m_anReceive_Datas[1] * 256 + m_anReceive_Datas[0];
                                    //                    if (
                                    //                        (nVal >= 0) && 
                                    //                        (nVal <= 4096)//g_CRobot.aCDxl[m_nReceive_ID].fMech_Move) // 360 도 이상은 돌지 않는다고 가정
                                    //                    )
                                    //                    {
                                    //                        m_anMot[CReceive.nID] = nVal;
                                    //                    }
                                    //                }
                                    //            }
                                    //        }
                                    //        m_nRequestMotors--;
                                    //        if (m_nRequestMotors <= 0)
                                    //        {
                                    //            m_nSeq_Motors++;
                                    //            m_nReceived_Policy = 0;
                                    //        }
                                    //}

                                    CReceive.nError = m_nReceive_Error;
                                    //CReceive.lstDatas = m_anReceive_Datas.slice();
                                    CReceive.lstDatas.AddRange(m_anReceive_Datas.ToArray());

                                    /* m_aCReceive.Add(CReceive);
                                     //if (bShow_Str) log2("Seq:" + m_nSeq_Receive + ", length:" + CReceive.nLength_Data);
                                     if (m_aCReceive.length > 10) m_aCReceive.shift(); // 가장 먼저 왔던 맨 앞의 데이터를 날린다.

                                     //m_nSeq_Receive++; // Que 에서 처리할 Seq
                                     m_nSeq++; // User 가 처리할 Seq
                                     */
                                    // to the next...
                                    m_nReceive_Index++;
                                }
                                break;
                            case 8:
                                m_nReceive_Length_Check++;
                                if (m_nReceive_Length_Check >= m_nReceive_Length)
                                {
                                    if (byData == 0xff) m_nReceive_Header = 1;
                                    else m_nReceive_Header = 0;
                                    m_nReceive_Index = 0;
                                }
                                break;
                        }
                    }
                } // for
                if (bShow_StrLetter)
                    Ojw.Log("[˘︹˘ ][Letter] :" + strLetter);
                if (bShow_Str)
                    Ojw.Log("[˘︹˘ ] :" + str);
                // log2("[˘︹˘ ] :" + strLetter + "\r\n============\r\n" + str);
            }

            public string GetError(int nErrorNumber)
            {
                string strRes = "ErrNum[" + nErrorNumber + "]";
                switch (nErrorNumber)
                {
                    case 0x00: break;
                    case 0x01: strRes += "[Result Fail] 전송된 Instruction Packet 을 처리하는데 실패한 경우";
                        break;
                    case 0x02: strRes += "[Instruction Error]  정의되지 않은 Instruction 을 사용한 경우\r\n";
                        strRes += "Reg Write 없이 Action 을 사용한 경우";
                        break;
                    case 0x03: strRes += "[CRC Error]          전송된 Packet 의 CRC 값이 맞지 않는 경우";
                        break;
                    case 0x04: strRes += "[Data Range Error]   해당 Address 에 쓰려는 Data 가 최소/최대값의 범위를 벗어난 경우";
                        break;
                    case 0x05: strRes += "[Data Length Error]  해당 Address 의 데이터 길이보다 짧은 데이터를 적으려고 한 경우\r\n";
                        strRes += "(예: 4 byte로 정의된 항목의 2 byte 만 쓰려고 하는 경우)";
                        break;
                    case 0x06: strRes += "[Data Limit Error]   해당 Address 에 쓰려는 Data 가 Limit 값을 벗어난 경우";
                        break;
                    case 0x07: strRes += "[Access Errer]       Read Only 혹은 정의되지 않은 Address 에 값을 쓰려고 한 경우\r\n";
                        strRes += "Write Only 혹은 정의되지 않은 Address 에 값을 읽으려고 한 경우\r\n";
                        strRes += "Torque Enable(ROM Lock) 상태에서 ROM 영역에 값을 쓰려고 한 경우";
                        break;
                }
                return strRes;
            }
            /*
            0x01: [Result Fail]        전송된 Instruction Packet 을 처리하는데 실패한 경우
            0x02: [Instruction Error]  정의되지 않은 Instruction 을 사용한 경우
                                    Reg Write 없이 Action 을 사용한 경우
            0x03: [CRC Error]          전송된 Packet 의 CRC 값이 맞지 않는 경우
            0x04: [Data Range Error]   해당 Address 에 쓰려는 Data 가 최소/최대값의 범위를 벗어난 경우
            0x05: [Data Length Error]  해당 Address 의 데이터 길이보다 짧은 데이터를 적으려고 한 경우
                                    (예: 4 byte로 정의된 항목의 2 byte 만 쓰려고 하는 경우)
            0x06: [Data Limit Error]   해당 Address 에 쓰려는 Data 가 Limit 값을 벗어난 경우
            0x07: [Access Errer]       Read Only 혹은 정의되지 않은 Address 에 값을 쓰려고 한 경우
                                    Write Only 혹은 정의되지 않은 Address 에 값을 읽으려고 한 경우
                                    Torque Enable(ROM Lock) 상태에서 ROM 영역에 값을 쓰려고 한 경우
            */
            #endregion Read

            #region Protocol - basic(updateCRC, MakeStuff, SendPacket)
            public byte[] MakePacket(int nMotorRealID, int nCommand, int nAddress = 0, params byte[] pbyDatas)
            {
                int i;
                i = 0;
                //int nLength = 3 + ((pbyDatas != null) ? pbyDatas.Length + 2 : 0);
                int nLength = 3;
                bool bDatas = false;
                if (pbyDatas.Length > 0)
                {
                    nLength = nLength + pbyDatas.Length + 2; // Address의 2바이트 주소 추가
                    bDatas = true;
                }

                int nDefaultSize = 7;
                byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xff;
                #region Packet 2.0
                pbyteBuffer[i++] = 0xfd;
                pbyteBuffer[i++] = 0x00;
                #endregion Packet 2.0
                pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                pbyteBuffer[i++] = (byte)(nLength & 0xff);
                pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                if (bDatas == true)
                {
                    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                    foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;
                }
                MakeStuff(ref pbyteBuffer);
                int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);
                return pbyteBuffer;
            }
            public byte[] MakePacket_Without_Address(int nMotorRealID, int nCommand, params byte[] pbyDatas)
            {
                int i;
                i = 0;
                //int nLength = 3 + ((pbyDatas != null) ? pbyDatas.Length + 2 : 0);
                int nLength = 3;
                bool bDatas = false;
                if (pbyDatas.Length > 0)
                {
                    nLength = nLength + pbyDatas.Length;
                    bDatas = true;
                }

                int nDefaultSize = 7;
                byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xff;
                #region Packet 2.0
                pbyteBuffer[i++] = 0xfd;
                pbyteBuffer[i++] = 0x00;
                #endregion Packet 2.0
                pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                pbyteBuffer[i++] = (byte)(nLength & 0xff);
                pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                if (bDatas == true)
                {
                    //pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                    //pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                    foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;
                }
                MakeStuff(ref pbyteBuffer);
                int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);
                return pbyteBuffer;
            }
            public void Send(int nMotorRealID, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                int i;
                i = 0;

                int nLength = 3 + ((pbyDatas != null) ? pbyDatas.Length + 2 : 0);
                int nDefaultSize = 7;
                byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xff;
                #region Packet 2.0
                pbyteBuffer[i++] = 0xfd;
                pbyteBuffer[i++] = 0x00;
                #endregion Packet 2.0
                pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                pbyteBuffer[i++] = (byte)(nLength & 0xff);
                pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                if (pbyDatas != null)
                {
                    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                    foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;
                }
                MakeStuff(ref pbyteBuffer);
                int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                SendPacket(pbyteBuffer, pbyteBuffer.Length);
            }
            private int[] m_anCrcTable = new int[256] { 
                    0x0000, 0x8005, 0x800F, 0x000A, 0x801B, 0x001E, 0x0014, 0x8011, 0x8033, 0x0036, 0x003C, 0x8039, 0x0028, 0x802D, 0x8027, 0x0022, 0x8063, 0x0066, 0x006C, 0x8069, 0x0078, 0x807D, 0x8077, 0x0072, 0x0050, 0x8055, 0x805F, 0x005A, 0x804B, 0x004E, 0x0044, 0x8041, 0x80C3, 0x00C6, 0x00CC, 0x80C9, 0x00D8, 0x80DD, 0x80D7, 0x00D2, 
                    0x00F0, 0x80F5, 0x80FF, 0x00FA, 0x80EB, 0x00EE, 0x00E4, 0x80E1, 0x00A0, 0x80A5, 0x80AF, 0x00AA, 0x80BB, 0x00BE, 0x00B4, 0x80B1, 0x8093, 0x0096, 0x009C, 0x8099, 0x0088, 0x808D, 0x8087, 0x0082, 0x8183, 0x0186, 0x018C, 0x8189, 0x0198, 0x819D, 0x8197, 0x0192, 0x01B0, 0x81B5, 0x81BF, 0x01BA, 0x81AB, 0x01AE, 0x01A4, 0x81A1, 
                    0x01E0, 0x81E5, 0x81EF, 0x01EA, 0x81FB, 0x01FE, 0x01F4, 0x81F1, 0x81D3, 0x01D6, 0x01DC, 0x81D9, 0x01C8, 0x81CD, 0x81C7, 0x01C2, 0x0140, 0x8145, 0x814F, 0x014A, 0x815B, 0x015E, 0x0154, 0x8151, 0x8173, 0x0176, 0x017C, 0x8179, 0x0168, 0x816D, 0x8167, 0x0162, 0x8123, 0x0126, 0x012C, 0x8129, 0x0138, 0x813D, 0x8137, 0x0132,
                    0x0110, 0x8115, 0x811F, 0x011A, 0x810B, 0x010E, 0x0104, 0x8101, 0x8303, 0x0306, 0x030C, 0x8309, 0x0318, 0x831D, 0x8317, 0x0312, 0x0330, 0x8335, 0x833F, 0x033A, 0x832B, 0x032E, 0x0324, 0x8321, 0x0360, 0x8365, 0x836F, 0x036A, 0x837B, 0x037E, 0x0374, 0x8371, 0x8353, 0x0356, 0x035C, 0x8359, 0x0348, 0x834D, 0x8347, 0x0342, 
                    0x03C0, 0x83C5, 0x83CF, 0x03CA, 0x83DB, 0x03DE, 0x03D4, 0x83D1, 0x83F3, 0x03F6, 0x03FC, 0x83F9, 0x03E8, 0x83ED, 0x83E7, 0x03E2, 0x83A3, 0x03A6, 0x03AC, 0x83A9, 0x03B8, 0x83BD, 0x83B7, 0x03B2, 0x0390, 0x8395, 0x839F, 0x039A, 0x838B, 0x038E, 0x0384, 0x8381, 0x0280, 0x8285, 0x828F, 0x028A, 0x829B, 0x029E, 0x0294, 0x8291, 
                    0x82B3, 0x02B6, 0x02BC, 0x82B9, 0x02A8, 0x82AD, 0x82A7, 0x02A2, 0x82E3, 0x02E6, 0x02EC, 0x82E9, 0x02F8, 0x82FD, 0x82F7, 0x02F2, 0x02D0, 0x82D5, 0x82DF, 0x02DA, 0x82CB, 0x02CE, 0x02C4, 0x82C1, 0x8243, 0x0246, 0x024C, 0x8249, 0x0258, 0x825D, 0x8257, 0x0252, 0x0270, 0x8275, 0x827F, 0x027A, 0x826B, 0x026E, 0x0264, 0x8261, 
                    0x0220, 0x8225, 0x822F, 0x022A, 0x823B, 0x023E, 0x0234, 0x8231, 0x8213, 0x0216, 0x021C, 0x8219, 0x0208, 0x820D, 0x8207, 0x0202 };

            private int updateCRC(byte[] data_blk_ptr, int data_blk_size)
            {
                int nCrc_accum = 0;
                for (int i = 0; i < data_blk_size; i++) nCrc_accum = (nCrc_accum << 8) ^ m_anCrcTable[(((nCrc_accum >> 8) ^ data_blk_ptr[i]) & 0xFF)];
                return nCrc_accum;
            }
            private void MakeStuff(ref byte[] pBuff)
            {
                int nStuff = 0;
                int[] pnIndex = new int[pBuff.Length];
                Array.Clear(pnIndex, 0, pnIndex.Length);
                int nCnt = 0;
                // (0)0xff, (1)0xff, (2)0xfd, (3)0x00,     
                // (4)ID != 0xff 이니 검사할 필요 없다.
                for (int i = 5; i < pBuff.Length; i++)
                {
                    switch (nStuff)
                    {
                        case 0: { if (pBuff[i] == 0xff) nStuff++; } break;
                        case 1: { if (pBuff[i] == 0xff) nStuff++; else nStuff = 0; } break;
                        case 2: { nStuff = 0; if (pBuff[i] == 0xfd) { pnIndex[nCnt++] = i; } } break;
                    }
                }
                if (nCnt > 0)
                {
                    byte[] pBuff2 = (byte[])pBuff.Clone();
                    Array.Resize<byte>(ref pBuff, pBuff2.Length + nCnt);
                    int nIndex = 0;
                    int nPos = 0;
                    int i = 5;
                    // 내부의 패킷길이값 재 설정 
                    pBuff[i++] = (byte)((pBuff.Length - 7) & 0xff);
                    pBuff[i++] = (byte)(((pBuff.Length - 7) >> 8) & 0xff);
                    for (i = 7; i < pBuff.Length; i++)
                    {
                        pBuff[i + nPos] = pBuff2[i];
                        if (i == pnIndex[nPos])
                        {
                            pBuff[nIndex + nPos + 1] = 0xfd;
                            nPos++;
                        }
                    }
                    pBuff2 = null;
                }
                pnIndex = null;
            }
            public void SendPacket(byte[] buffer, int nLength)
            {
                if (m_CSerial.IsConnect() == true) m_CSerial.SendPacket(buffer, nLength);
                if (m_CSock_Client.IsConnect() == true) m_CSock_Client.SendPacket(buffer, nLength);
            }
            #endregion Protocol - basic(updateCRC, MakeStuff, SendPacket)

            #region Sync Write
            // Sync Write //
            private int m_nSync_Length = 0;
            private bool m_IsSync_Error = false;
            private List<byte> m_lstSync = new List<byte>();
            // // Sync_Set, Sync_Push, Sync_Flush
            public void Sync_Clear()
            {
                m_lstSync.Clear();
                //m_nSunc_Address = nAddress;
                m_nSync_Length = 0;
                m_IsSync_Error = false;
            }
            public void Sync_Push_Byte(int nID, int nData)
            {
                byte[] abyDatas = new byte[1];
                abyDatas[0] = (byte)(nData & 0xff);
                Sync_Push(nID, abyDatas);
            }
            public void Sync_Push_Word(int nID, int nData)
            {
                byte[] abyDatas = new byte[2];
                abyDatas[0] = (byte)(nData & 0xff);
                abyDatas[1] = (byte)((nData >> 8) & 0xff);
                Sync_Push(nID, abyDatas);
            }
            public void Sync_Push_Dword(int nID, int nData)
            {
                byte[] abyDatas = new byte[4];
                abyDatas[0] = (byte)(nData & 0xff);
                abyDatas[1] = (byte)((nData >> 8) & 0xff);
                abyDatas[2] = (byte)((nData >> 16) & 0xff);
                abyDatas[3] = (byte)((nData >> 24) & 0xff);
                Sync_Push(nID, abyDatas);
            }
            public void Sync_Push_Angle(int nID, float fAngle)
            {
                byte[] abyDatas = new byte[4];
                int nData = CalcAngle2Evd(nID, fAngle);
                abyDatas[0] = (byte)(nData & 0xff);
                abyDatas[1] = (byte)((nData >> 8) & 0xff);
                abyDatas[2] = (byte)((nData >> 16) & 0xff);
                abyDatas[3] = (byte)((nData >> 24) & 0xff);
                Sync_Push(nID, abyDatas);
            }
            public void Sync_Push(int nID, byte[] pbyDatas)
            {
                int nDataLength = pbyDatas.Length;

                if (nDataLength > 0)
                {
                    if (m_nSync_Length == 0)
                    {
                        m_nSync_Length = nDataLength;
                        m_lstSync.Add((byte)(nDataLength & 0xff));
                        m_lstSync.Add((byte)((nDataLength >> 8) & 0xff));
                    }
                    else if (m_nSync_Length != nDataLength)
                    {
                        Ojw.LogErr("Error(Sync_Push) - ID:" + nID);
                        m_IsSync_Error = true;
                        return;
                    }

                    m_lstSync.Add((byte)(nID & 0xff));
                    for (var i = 0; i < nDataLength; i++)
                    {
                        m_lstSync.Add((byte)pbyDatas[i]);
                    }
                }
            }
            public void Sync_Flush(int nAddress)
            {
                if (m_IsSync_Error == false)
                {
                    if (m_lstSync.Count > 0)
                    {
                        Send(254, 0x83, nAddress, m_lstSync.ToArray());
                    }
                }
                Sync_Clear();
            }
            #endregion Sync Write


            #region Delta
            public List<CDelta> m_lstCDelta = new List<CDelta>();
            public int Delta_Add(float fRot_Cw, int nID_Front, int nID_Left, int nID_Right, float fTop_Rad, float fTop_Length, float fBottom_Length, float fBottom_Rad)
            {
                m_lstCDelta.Add(new CDelta(fRot_Cw, nID_Front, nID_Left, nID_Right, fTop_Rad, fTop_Length, fBottom_Length, fBottom_Rad));
                return m_lstCDelta.Count;
            }
            public void Delta_Clear() { m_lstCDelta.Clear(); }
            public class CDelta
            {
                public CDelta(float fRot_Cw, int nID_Front, int nID_Left, int nID_Right, float fTop_Rad, float fTop_Length, float fBottom_Length, float fBottom_Rad)
                {
                    Init(fRot_Cw, nID_Front, nID_Left, nID_Right, fTop_Rad, fTop_Length, fBottom_Length, fBottom_Rad);
                }
                public bool IsValid = false;

                private float fRot = 0.0f;
                public int nId_Front = 0;
                public int nId_Left = 1;
                public int nId_Right = 2;

                private float fInit_Top_Radius = 0;
                private float fInit_Top_Length = 0;
                private float fInit_Bottom_Length = 0;
                private float fInit_Bottom_Radius = 0;

                public float[] m_afAngle = new float[3];
                public float x = 0.0f;
                public float y = 0.0f;
                public float z = 0.0f;
                public void Init(float fRot_Cw, int nID_Front, int nID_Left, int nID_Right, float fTop_Rad, float fTop_Length, float fBottom_Length, float fBottom_Rad)
                {
                    fRot = fRot_Cw;
                    nId_Front = nID_Front;
                    nId_Left = nID_Left;
                    nId_Right = nID_Right;
                    fInit_Top_Radius = fTop_Rad;
                    fInit_Top_Length = fTop_Length;
                    fInit_Bottom_Length = fBottom_Length;
                    fInit_Bottom_Radius = fBottom_Rad;
                    
                    IsValid = true;
                }

                public bool CalcXyzToAngle(float fPos_X, float fPos_Y, float fPos_Height, out float fAngle_Front, out float fAngle_Left, out float fAngle_Right)
                {
                    if (IsValid == false)
                    {
                        fAngle_Front = fAngle_Left = fAngle_Right = 0;
                        return false;
                    }
                    // 
                    if (fRot != 0)
                    {
                        if (Ojw.CMath.CalcRot(0.0f, 0.0f, fRot, ref fPos_X, ref fPos_Y, ref fPos_Height) == false)
                        {
                            fAngle_Front = fAngle_Left = fAngle_Right = 0;
                            return false;
                        }
                    }
                    double[] adVal = new double[3];
                    Ojw.CMath.Delta_Parallel_Init(fInit_Top_Radius, fInit_Top_Length, fInit_Bottom_Length, fInit_Bottom_Radius);
                    Ojw.CMath.Delta_Parallel_InverseKinematics(fPos_X, fPos_Y, fPos_Height, out adVal[0], out adVal[1], out adVal[2]);
                    fAngle_Front = (float)adVal[0];
                    fAngle_Left = (float)adVal[1];
                    fAngle_Right = (float)adVal[2];
                    ///////////
                    x = fPos_X;
                    y = fPos_Y;
                    z = fPos_Height;
                    m_afAngle[0] = (float)adVal[0];
                    m_afAngle[1] = (float)adVal[1];
                    m_afAngle[2] = (float)adVal[2];
                    return true;
                }
                public bool CalcAngleToXyz(float fAngle_Front, float fAngle_Left, float fAngle_Right, out float fX, out float fY, out float fHeight)
                {
                    if (IsValid == false)
                    {
                        fX = fY = fHeight = 0;
                        return false;
                    }
                    double[] adVal = new double[3];
                    Ojw.CMath.Delta_Parallel_Init(fInit_Top_Radius, fInit_Top_Length, fInit_Bottom_Length, fInit_Bottom_Radius);
                    bool bRet = Ojw.CMath.Delta_Parallel_ForwardKinematics(fAngle_Front, fAngle_Left, fAngle_Right, out adVal[0], out adVal[1], out adVal[2]);
                    fX = (float)adVal[0];
                    fY = (float)adVal[1];
                    fHeight = (float)adVal[2];
                    ///////////
                    m_afAngle[0] = fAngle_Front;
                    m_afAngle[1] = fAngle_Left;
                    m_afAngle[2] = fAngle_Right;
                    x = (float)adVal[0];
                    y = (float)adVal[1];
                    z = (float)adVal[2];

                    if (fRot != 0)
                    {
                        if (Ojw.CMath.CalcRot(0.0f, 0.0f, fRot, ref fX, ref fY, ref fHeight) == false)
                        {
                            fX = fY = fHeight = 0;
                            return false;
                        }
                    }
                    return bRet;
                }
            }
            public int GetDelta_Count() { return m_lstCDelta.Count; }
            public void SetDelta(int nIndex, float fX, float fY, float fHeight)
            {
                float[] afAngle = new float[3];
                if ((nIndex >= 0) && (nIndex < m_lstCDelta.Count)) m_lstCDelta[nIndex].CalcXyzToAngle(fX, fY, fHeight, out afAngle[0], out afAngle[1], out afAngle[2]);
                Set(m_lstCDelta[nIndex].nId_Front, afAngle[0]);
                Set(m_lstCDelta[nIndex].nId_Left, afAngle[1]);
                Set(m_lstCDelta[nIndex].nId_Right, afAngle[2]);
            }
            public bool GetDelta(int nIndex, out float fX, out float fY, out float fHeight)
            {
                if ((nIndex >= 0) && (nIndex < m_lstCDelta.Count))
                {
                    return m_lstCDelta[nIndex].CalcAngleToXyz(m_lstCDelta[nIndex].m_afAngle[0], m_lstCDelta[nIndex].m_afAngle[1], m_lstCDelta[nIndex].m_afAngle[2], out fX, out fY, out fHeight);
                    //return m_lstCDelta[nIndex].CalcAngleToXyz(GetAngle(m_lstCDelta[nIndex].nId_Front), GetAngle(m_lstCDelta[nIndex].nId_Left), GetAngle(m_lstCDelta[nIndex].nId_Right), out fX, out fY, out fHeight);
                }
                fX = fY = fHeight = 0.0f;
                return false;
            }
            public void Move_Delta(int nIndex, float fX, float fY, float fHeight, int nTime, int nDelay, bool bNoWait = false)
            {
                SetDelta(nIndex, fX, fY, fHeight);
                if (bNoWait == false) Move(nTime, nDelay, m_lstCmdIDs.ToArray());
                else Move_NoWait(nTime, nDelay, m_lstCmdIDs.ToArray());
                //Wait(nTime + nDelay);
            }
            public void Move_Delta(int nIndex, float fX, float fY, float fHeight, int nTime)
            {
                Move_Delta(nIndex, fX, fY, fHeight, nTime, 0);
            }
            #endregion Delta


        }
    }
}
#else

#endif