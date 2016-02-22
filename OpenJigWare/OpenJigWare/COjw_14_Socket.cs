using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CServer
        {
            #region 변수
            //private byte m_pbyteServer;
            private bool m_bThread_Server = false;
            private TcpListener m_tcpServer;    // 서버
            private TcpClient m_tcpServer_Client; // 서버에 붙는 클라이언트
            private Thread m_thServer;          // 스레드
            private BinaryWriter m_bwServer_outData;
            private BinaryReader m_bwServer_inData;
            private String m_strOrgPath = "";
            private int m_nServer_Seq = 0;
            private int m_nReceived_ServerCmd = 0;
            private int m_nCntAuth = 0;
            private bool m_bAuth = false;
            #endregion 변수

            #region 공개
            //Server thread가 돌고 있는지 여부
            public bool sock_thread_started() { return m_bThread_Server; }
            //서버가 클라이언트와 연결되어 있는지 여부
            public bool sock_connected()
            {
                if (sock_started() == false) return false;
                if (m_tcpServer_Client == null) return false;
                return m_tcpServer_Client.Connected;
            }
            //서버 구동이 시작 되었는지 여부
            public bool sock_started()
            {
                if (m_tcpServer == null) return false;
                return true;// m_tcpServer.Server.Connected;
            }
            //seq값은 패킷을 하나 받을 때마다 자동으로 1씩 증가. seq 값 읽어오기.
            public int sock_seq() { return m_nServer_Seq; }
            //서버로 들어온 Cmd 값을 읽기
            public int sock_get_result_cmd()
            {
                return m_nReceived_ServerCmd;
            }
            //인증이 되어 있는지 여부
            public bool sock_get_auth() { return m_bAuth; }
            //ack을 보내기
            //public void sock_send_ack(int nCurrentCmd, int nStatus)
            //{
            //    sock_send_2_data(0xf0, (byte)(nCurrentCmd & 0xff), (byte)(nStatus & 0xff));
            //}
            public bool sock_start(int nA, int nB, int nC, int nD, int nPort)
            {
                String strIP = nA.ToString() + "," + nB.ToString() + "," + nC.ToString() + "," + nD.ToString();
                return sock_start(strIP, nPort);
            }
            public bool sock_start(String strIP, int nPort)
            {
                bool bRet = false;
                try
                {
                    byte[] pbyIp = new byte[4];
                    String[] pstrIP = strIP.Split('.');
                    int i = 0;
                    foreach (String strItem in pstrIP)
                    {
                        int nIp = Convert.ToInt32(strItem);
                        pbyIp[i++] = (byte)(nIp & 0xff);
                    }
                    if (i != 4)
                    {
                        return false;
                    }

                    IPAddress addr = new IPAddress(pbyIp);

                    // Server
                    m_tcpServer = new TcpListener(addr, nPort);
                    m_tcpServer.Start();
                                        
                    //클라이언트 생성시 스레드를 생성한다.
                    //m_thServer = new Thread(new ThreadStart(ThreadServer));
                    //m_thServer.Start();

                    pbyIp = null;
                    addr = null;

                    bRet = true;
                }
                catch
                {
                    bRet = false;
                }
                return bRet;
            }
            public int sock_get_size_buffer()
            {
                return m_tcpServer_Client.ReceiveBufferSize;
            }
            #region 비공개 - thread ...
            public void WaitClient(bool bBlockingMode)
            {
                // 클라이언트가 붙으면 담당 소켓을 생성한다.
                m_tcpServer_Client = m_tcpServer.AcceptTcpClient();
                m_tcpServer_Client.Client.Blocking = bBlockingMode;
                m_tcpServer_Client.NoDelay = false;
                m_bwServer_outData = new BinaryWriter(new BufferedStream(m_tcpServer_Client.GetStream()));
                m_bwServer_inData = new BinaryReader(new BufferedStream(m_tcpServer_Client.GetStream()));

                CMessage.Write("준비완료");

                m_nServer_Seq = 0;

                m_nCntAuth = 0;
                m_bAuth = false;

                m_bThread_Server = true;
                //string strData = "";
            }
            //private void ThreadServer()
            //{
            //    try
            //    {
            //        // 클라이언트가 붙으면 담당 소켓을 생성한다.
            //        m_tcpServer_Client = m_tcpServer.AcceptTcpClient();
            //        m_tcpServer_Client.NoDelay = false;
            //        m_bwServer_outData = new BinaryWriter(new BufferedStream(m_tcpServer_Client.GetStream()));
            //        m_bwServer_inData = new BinaryReader(new BufferedStream(m_tcpServer_Client.GetStream()));
                    
            //        CMessage.Write("준비완료");

            //        m_nServer_Seq = 0;

            //        m_nCntAuth = 0;
            //        m_bAuth = false;

            //        m_bThread_Server = true;
            //        //string strData = "";
            //        while (sock_connected() == true)
            //        {
            //            byte byteData = sock_get_byte();
            //            if (sock_connected() == false) return;

            //            //if (byteData != 0)
            //            //{
            //            //    strData += (char)(byteData);
            //            //    if (byteData == 10)
            //            //    {
            //            //        Ojw.CMessage.Write(strData);
            //            //        strData = String.Empty;
            //            //    }
            //            //}


            //            if (byteData != 0)
            //            {
            //                CMessage.Write2("{0}", (char)(byteData));
            //                sock_send(byteData);
            //            }
                        

            //            //if (byteData != 0)
            //            //{
            //            //    //if ((char)(byteData) == 'f')
            //            //    //{
            //            //    //    CMessage.Write("전진");
            //            //    //}
            //            //    //else if ((char)(byteData) == 'b')
            //            //    //{
            //            //    //    CMessage.Write("후진");
            //            //    //}
            //            //    //else if ((char)(byteData) == 'l')
            //            //    //{
            //            //    //    CMessage.Write("좌회전");
            //            //    //}
            //            //    //else if ((char)(byteData) == 'r')
            //            //    //{
            //            //    //    CMessage.Write("우회전");
            //            //    //}
            //            //    //else
            //            //    //{
            //            //    //    //CMessage.Write("{0}", (char)(byteData));
            //            //    //    CMessage.Write("알지못하는 명령 입력={0}", (char)byteData);
            //            //    //}
            //            //    CMessage.Write2("{0}", (char)byteData);
            //            //}
            //        }

            //        CMessage.Write("Connected {0}", m_tcpServer_Client.ToString());
            //        m_bThread_Server = false;
            //    }
            //    catch// (Exception e)
            //    {
            //    }
            //}
            #endregion 비공개 - thread ...   
            public void sock_stop()
            {
                // 스레드 종료
                //m_bStartedServerThread = false;
                // 열려있는 포트 닫음(Server)
                //if (m_bStartedServer == true) m_tcpServer.Stop();
                //if (m_tcpServer.Server.Connected == true) m_tcpServer.Stop();
                bool bConnected = sock_connected();
                bool bStarted = sock_started();
                if (bConnected == true) m_tcpServer_Client.Close();
                if (bStarted == true) m_tcpServer.Stop();

                m_tcpServer = null;


                m_bAuth = false;
                m_nCntAuth = 0;
            }
            // 벌크데이타를 보냄
            //public bool sock_send(byte[] byteData)
            //{
            //    try
            //    {
            //        if (!sock_connected()) return false;

            //        if (sock_connected()) m_bwServer_outData.Write(byteData);
            //        if (sock_connected()) m_bwServer_outData.Flush();
            //        return true;
            //    }
            //    catch
            //    {
            //        return false;
            //    }
            //}
            public bool sock_send(params byte[] byteData)
            {
                try
                {
                    if (!sock_connected()) return false;

                    if (byteData != null)
                    {
                        if (byteData.Length > 0)
                        {
                            if (sock_connected()) m_bwServer_outData.Write(byteData);
                            if (sock_connected()) m_bwServer_outData.Flush();
                            return true;
                        }
                        return false;
                    }
                    return false;          
                }
                catch
                {
                    return false;
                }
            }
            private int m_nSeq_Error = 0;
            public int sock_get_seq_error() { return m_nSeq_Error; }
            public byte sock_get_byte()
            {
                try
                {
                    if (sock_connected()) return m_bwServer_inData.ReadByte();
                    else return 0;
                }
                catch
                {
                    m_nSeq_Error++;
                    return 0;
                }
            }
            // 2 Byte 의 정수값을 반환
            public int sock_get_int16()
            {
                //if (sock_connected()) return inData.ReadInt16(); //2 Bytes
                //else return 0;
                int nData;
                byte[] byteData = sock_get_bytes(2);
                nData = (byteData[2] << 8);
                nData += (byteData[3]);
                if (sock_connected()) return nData; //2 Bytes
                else return 0;
            }
            // 4 Byte 의 정수값을 반환
            public int sock_get_int32()
            {
                int nData;
                byte[] byteData = sock_get_bytes(4);
                nData = (byteData[0] << 24);
                nData += (byteData[1] << 16);
                nData += (byteData[2] << 8);
                nData += (byteData[3]);
                //if (sock_connected()) return inData.ReadInt32(); //4 Bytes
                if (sock_connected()) return nData; //4 Bytes
                else return 0;
            }
            // Size 만큼을 읽어감
            public byte[] sock_get_bytes(int nSize)
            {
                return m_bwServer_inData.ReadBytes(nSize);
                //if (sock_connected()) return inData.ReadBytes(nSize);
                //else return null;
            }
            #endregion 공개
        }
        
        public class CSocket
        {
            public CSocket()
            {
            }
            private bool m_bClassEnd = false;
            ~CSocket()
            {
                if (m_bClassEnd == true) m_bClassEnd = true;
                //if (IsConnect())
                //    DisConnect();
            }

            private int _CHKSUM_NONE = 0;
            private int _CHKSUM_AND = 1;
            private int _CHKSUM_OR = 2;
            private int _CHKSUM_XOR = 3;
            private int _CHKSUM_SUM = 4;

            public NetworkStream stream;            // 네트워크 스트림
            public int m_nPort = 0;                        // 포트번호

            public bool m_bConnect = false;          // 서버 접속 플래그
            TcpClient m_tcpClient;                       // TCP 클라이언트
            BinaryWriter outData;
            BinaryReader inData;
            public bool IsConnect() { return (m_tcpClient == null) ? false : m_tcpClient.Connected; }
            public bool Connect(String strIP, int nPort)
            {
                m_tcpClient = new TcpClient();       // TCP 클라이언트 생성
                IAsyncResult IResult = null;
                //ManualResetEvent MWait = new ManualResetEvent(false);
                try
                {
                    // 서버 IP 주소와 포트 번호를 이용해 접속 시도
                    //m_tcpClient.Connect(strIP, nPort); // - 이건 랙을 유발시킨다.
                    //m_tcpClient.Connect(IPAddress.Parse(strIP), nPort);

                    IResult = m_tcpClient.BeginConnect(IPAddress.Parse(strIP), nPort, null, null);
                    //if (IResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1)) == false)
                    //{
                    //    // 서버에 접속 실패시
                    //    m_bConnect = false;
                    //    return false;
                    //}            
                    if (IResult.AsyncWaitHandle.WaitOne(100) == false) // ms
                    {
                        // 서버에 접속 실패시
                        m_bConnect = false;
                        return false;
                    }
                    m_tcpClient.EndConnect(IResult);
                    //MWait.WaitOne(
                }
                catch
                {
                    // 서버에 접속 실패시
                    m_bConnect = false;                    
                    return false;
                }

                m_bConnect = true;

                stream = m_tcpClient.GetStream();    // 스트림 가져오기
                outData = new BinaryWriter(new BufferedStream(m_tcpClient.GetStream()));
                inData = new BinaryReader(new BufferedStream(m_tcpClient.GetStream()));


                return true;
            }
            public bool Connect(String strIP, int nPort, bool bBlockingMode)
            {
                m_tcpClient = new TcpClient();       // TCP 클라이언트 생성

                try
                {
                    // 서버 IP 주소와 포트 번호를 이용해 접속 시도
                    m_tcpClient.Connect(strIP, nPort);
                    m_tcpClient.Client.Blocking = bBlockingMode;
                }
                catch
                {
                    // 서버에 접속 실패시
                    m_bConnect = false;
                    return false;
                }

                m_bConnect = true;

                stream = m_tcpClient.GetStream();    // 스트림 가져오기
                outData = new BinaryWriter(new BufferedStream(m_tcpClient.GetStream()));
                inData = new BinaryReader(new BufferedStream(m_tcpClient.GetStream()));


                return true;
            }
            public bool DisConnect()
            {
                try
                {
                    //bool m_bConnect = IsConnect();
                    if (!m_bConnect) return false; //  IsConnect 플래그 체크

                    m_bConnect = false;

                    inData.Close();           // 입력 스트림 닫기
                    outData.Close();          // 출력 스트림 닫기

                    stream.Close();         // 스트림 종료

                    //Reader.Abort();         // 쓰레드 종료
                    return true;
                }
                catch//(Exception e)
                {
                    return false;
                    //MessageBox.Show("[Message]" + e.ToString() + "\r\n");
                }
            }

            public byte CheckSum(String strData, int nStartPos, int nType)
            {
                if (nType == _CHKSUM_NONE) return (byte)0;
                Byte nData = (Byte)(strData[nStartPos]);
                for (int i = nStartPos + 1; i < strData.Length; i++)
                {
                    if (nType == _CHKSUM_AND) nData &= (Byte)(strData[i]);
                    else if (nType == _CHKSUM_OR) nData |= (Byte)(strData[i]);
                    else if (nType == _CHKSUM_XOR) nData ^= (Byte)(strData[i]);
                    else if (nType == _CHKSUM_SUM) nData += (Byte)(strData[i]);
                }
                return (byte)(nData & 0xff);
            }

            public byte CheckSum(byte[] byteData, int nStartPos, int nType)
            {
                if (nType == _CHKSUM_NONE) return (byte)0;
                Byte nData = (Byte)(byteData[nStartPos]);
                for (int i = nStartPos + 1; i < byteData.Length; i++)
                {
                    if (nType == _CHKSUM_AND) nData &= (Byte)(byteData[i]);
                    else if (nType == _CHKSUM_OR) nData |= (Byte)(byteData[i]);
                    else if (nType == _CHKSUM_XOR) nData ^= (Byte)(byteData[i]);
                    else if (nType == _CHKSUM_SUM) nData += (Byte)(byteData[i]);
                }
                return (byte)(nData & 0xff);
            }

            public byte CheckSum(byte byteData, byte byteData2, int nType)
            {
                if (nType == _CHKSUM_NONE) return (byte)0;
                byteData2 = (Byte)(byteData);

                if (nType == _CHKSUM_AND) byteData2 &= (Byte)(byteData);
                else if (nType == _CHKSUM_OR) byteData2 |= (Byte)(byteData);
                else if (nType == _CHKSUM_XOR) byteData2 ^= (Byte)(byteData);
                else if (nType == _CHKSUM_SUM) byteData2 += (Byte)(byteData);

                return (byte)(byteData2 & 0xff);
            }

            // Data, CheckSum의 사용여부, 사용시 타입(And, Xor, Or...)
            public bool Send(byte[] byteData, int nCheckSumStartPos, bool bCheckSum, int nCheckSumType, int nSecondCheckSumData, int nSecondCheckSumType)
            {
                try
                {
                    if (!m_bConnect) return false;

                    int nLength = byteData.Length;
                    int nCheckSumLength = 0;
                    byte byteCheckSum = 0;
                    if (bCheckSum)
                    {
                        //// &0x7f ////
                        //byteCheckSum = (byte)(CheckSum(byteData, nCheckSumStartPos, nCheckSumType) & nAndCheckSumData);
                        if (nSecondCheckSumType == _CHKSUM_AND) byteCheckSum = (byte)(CheckSum(byteData, nCheckSumStartPos, nCheckSumType) & nSecondCheckSumData);
                        else if (nSecondCheckSumType == _CHKSUM_OR) byteCheckSum = (byte)(CheckSum(byteData, nCheckSumStartPos, nCheckSumType) | nSecondCheckSumData);
                        else if (nSecondCheckSumType == _CHKSUM_XOR) byteCheckSum = (byte)(CheckSum(byteData, nCheckSumStartPos, nCheckSumType) ^ nSecondCheckSumData);
                        else if (nSecondCheckSumType == _CHKSUM_SUM) byteCheckSum = (byte)(CheckSum(byteData, nCheckSumStartPos, nCheckSumType) + nSecondCheckSumData);
                        else byteCheckSum = (byte)CheckSum(byteData, nCheckSumStartPos, nCheckSumType);
                        nCheckSumLength = 1;
                    }

                    byte[] byteSend = new byte[nLength + nCheckSumLength];
                    for (int i = 0; i < nLength; i++)
                    {
                        byteSend[i] = byteData[i];
                    }
                    if (bCheckSum)
                    {
                        byteSend[nLength + nCheckSumLength - 1] = byteCheckSum;
                    }

                    if (m_bConnect) outData.Write(byteSend);
                    if (m_bConnect) outData.Flush();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            // 벌크데이타를 보냄
            public bool Send(byte[] byteData)
            {
                try
                {
                    if (!m_bConnect) return false;

                    if (m_bConnect) outData.Write(byteData);
                    if (m_bConnect) outData.Flush();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public byte GetByte()
            {
                if (m_bConnect) return inData.ReadByte();
                else return 0;
            }

            // 2 Byte 의 정수값을 반환
            public int GetInt16()
            {
                //if (m_bConnect) return inData.ReadInt16(); //2 Bytes
                //else return 0;
                int nData;
                byte[] byteData = GetBytes(2);
                nData = (byteData[2] << 8);
                nData += (byteData[3]);
                if (m_bConnect) return nData; //2 Bytes
                else return 0;
            }

            // 4 Byte 의 정수값을 반환
            public int GetInt32()
            {
                int nData;
                byte[] byteData = GetBytes(4);
                nData = (byteData[0] << 24);
                nData += (byteData[1] << 16);
                nData += (byteData[2] << 8);
                nData += (byteData[3]);
                //if (m_bConnect) return inData.ReadInt32(); //4 Bytes
                if (m_bConnect) return nData; //4 Bytes
                else return 0;
            }

            // Size 만큼을 읽어감
            public byte[] GetBytes(int nSize)
            {
                if (m_bConnect) return inData.ReadBytes(nSize);
                else return null;
            }
        }
    }
}
