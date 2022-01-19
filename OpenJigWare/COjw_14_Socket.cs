using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CServer
        {
            #region 변수
            //private byte m_pbyteServer;
            private bool m_bThread_Server = false;
            public TcpListener m_tcpServer;    // 서버
            public TcpClient m_tcpServer_Client; // 서버에 붙는 클라이언트
            //private Thread m_thServer;          // 스레드
            private BinaryWriter m_bwServer_outData;
            private BinaryReader m_bwServer_inData;
            //private String m_strOrgPath = "";
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
                //m_tcpServer_Client.Client.Poll(0, System.Net.Sockets.SelectMode.SelectRead);
                //if (m_tcpServer_Client.Client.Poll(0, System.Net.Sockets.SelectMode.SelectRead) == false) return false;
                if (sock_started() == false) return false;
                if (m_tcpServer_Client == null) return false;
                return isClientConnected();// m_tcpServer_Client.Client.Connected;
            }
            public void sock_close_client()
            {
                if (isClientConnected())
                {
                    m_tcpServer_Client.Client.Disconnect(false);
                }
            }
            // 출처 : http://stackoverflow.com/posts/33209626/edit
            public bool isClientConnected()
            {
                if (m_tcpServer_Client == null) return false;
                if (m_tcpServer_Client.Client == null) return false;

                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();
                foreach (TcpConnectionInformation c in tcpConnections)
                {
                    TcpState stateOfConnection = c.State;
                    if (c.LocalEndPoint.Equals(m_tcpServer_Client.Client.LocalEndPoint) && c.RemoteEndPoint.Equals(m_tcpServer_Client.Client.RemoteEndPoint))
                    {
                        if (stateOfConnection == TcpState.Established)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            }

            //서버 구동이 시작 되었는지 여부
            public bool sock_started()
            {
                if (m_tcpServer == null) return false;
                return true;//  m_tcpServer.Server.Connected; // true
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
            public bool sock_start(int nPort)
            {
                int nCnt = Ojw.CSocket.GetIpAddress_Cnt();
                if (nCnt > 0)
                {
                    int nIndex = -1;
                    for (int i = 0; i < nCnt; i++)
                    {
                        string strIp = Ojw.CSocket.GetIpAddress(i);
                        //if (strIp.IndexOf("192.") >= 0) //nIndex = i;
                        if ((strIp.ToLower().IndexOf("local") < 0) && (strIp.IndexOf(":") < 0))
                        {
                            nIndex = i;
                            //Ojw.CMessage.Write("http://{0}:{1}", strIp, nPort);
                            break;
                        }                        
                    }
                    if (nIndex >= 0)
                    {
                        return sock_start(Ojw.CSocket.GetIpAddress(nIndex), nPort);
                        //return true;
                    }                   
                    return false;
                }
                return false;
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
                    
                    _random = new Random((int)DateTime.Now.Ticks);
                }
                catch
                {
                    bRet = false;
                }
                return bRet;
            }
            public int GetBuffer_Length() { return sock_get_size_buffer(); }
            public int sock_get_size_buffer()
            {
                return m_tcpServer_Client.Available;//.ReceiveBufferSize;
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
                m_bWebSocket = false; // 일단은 웹소켓 판단을 '아님' 으로 한다.
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
            //public long GetBuffer_Length()
            //{
            //    try
            //    {
            //        return m_bwServer_inData.BaseStream.Length;
            //    }
            //    catch
            //    {
            //        return 0;
            //    }
            //}

            public byte [] request_bytes()
            {
                int nBufferSize = GetBuffer_Length();
                byte[] pbyData;
                if (nBufferSize > 0)
                {
                    pbyData = sock_get_bytes(nBufferSize);
                    if (sock_check_websocket(pbyData) == 0) 
                    {
                        CMessage.Write("Handshake...\r\n");
                        return null;
                    }
                    else if (sock_check_websocket(pbyData) == 1)
                    {
                        return websocket_read(pbyData);
                    }
                    else
                    {
                        return pbyData;
                    }
                }
                return null;
            }
            public string request_string()
            {   
                byte [] buffer = request_bytes();
                if (buffer != null) return Encoding.UTF8.GetString(buffer);
                return null;
            }
            // 출처: https://www.codeproject.com/Articles/1063910/WebSocket-Server-in-Csharp
            public enum WebSocketOpCode
            {
                ContinuationFrame = 0,
                TextFrame = 1,
                BinaryFrame = 2,
                ConnectionClose = 8,
                Ping = 9,
                Pong = 10
            }

            private Random _random;
            private bool _isClient = false;
            private bool isLastFrame = true;
            public const int MaskKeyLength = 4;
            // 출처: https://www.codeproject.com/Articles/1063910/WebSocket-Server-in-Csharp
            public void websocket_write(byte[] pbyData, bool bBinary=false)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte finBitSetAsByte = isLastFrame ? (byte)0x80 : (byte)0x00;
                    byte op = (bBinary) ? (byte)WebSocketOpCode.BinaryFrame : (byte)WebSocketOpCode.TextFrame;
                    byte byte1 = (byte)(finBitSetAsByte | op);
                    //byte op = (byte)WebSocketOpCode.BinaryFrame | (byte)WebSocketOpCode.TextFrame;
                    //byte byte1 = (byte)(finBitSetAsByte | op);
                    memoryStream.WriteByte(byte1);

                    // NB, set the mask flag if we are constructing a client frame
                    byte maskBitSetAsByte = _isClient ? (byte)0x80 : (byte)0x00;

                    // depending on the size of the length we want to write it as a byte, ushort or ulong
                    if (pbyData.Length < 126)
                    {
                        byte byte2 = (byte)(maskBitSetAsByte | (byte)pbyData.Length);
                        memoryStream.WriteByte(byte2);
                    }
                    else if (pbyData.Length <= ushort.MaxValue)
                    {
                        byte byte2 = (byte)(maskBitSetAsByte | 126);
                        memoryStream.WriteByte(byte2);
                        WriteUShort((ushort)pbyData.Length, memoryStream, false);
                    }
                    else
                    {
                        byte byte2 = (byte)(maskBitSetAsByte | 127);
                        memoryStream.WriteByte(byte2);
                        WriteULong((ulong)pbyData.Length, memoryStream, false);
                    }

                    // if we are creating a client frame then we MUST mack the pbyData as per the spec
                    if (_isClient)
                    {
                        byte[] maskKey = new byte[MaskKeyLength];
                        _random.NextBytes(maskKey);
                        memoryStream.Write(maskKey, 0, maskKey.Length);

                        // mask the pbyData
                        ToggleMask(maskKey, pbyData);
                    }

                    memoryStream.Write(pbyData, 0, pbyData.Length);
                    byte[] buffer = memoryStream.ToArray();
                    sock_send(buffer);
                }
            }

            // 출처: https://www.codeproject.com/Articles/1063910/WebSocket-Server-in-Csharp
            private static void WriteULong(ulong value, Stream stream, bool isLittleEndian)
            {
                byte[] buffer = BitConverter.GetBytes(value);
                if (BitConverter.IsLittleEndian && !isLittleEndian)
                {
                    Array.Reverse(buffer);
                }

                stream.Write(buffer, 0, buffer.Length);
            }

            // 출처: https://www.codeproject.com/Articles/1063910/WebSocket-Server-in-Csharp
            private static void WriteLong(long value, Stream stream, bool isLittleEndian)
            {
                byte[] buffer = BitConverter.GetBytes(value);
                if (BitConverter.IsLittleEndian && !isLittleEndian)
                {
                    Array.Reverse(buffer);
                }

                stream.Write(buffer, 0, buffer.Length);
            }

            // 출처: https://www.codeproject.com/Articles/1063910/WebSocket-Server-in-Csharp
            private static void WriteUShort(ushort value, Stream stream, bool isLittleEndian)
            {
                byte[] buffer = BitConverter.GetBytes(value);
                if (BitConverter.IsLittleEndian && !isLittleEndian)
                {
                    Array.Reverse(buffer);
                }

                stream.Write(buffer, 0, buffer.Length);
            }

            // 출처: https://www.codeproject.com/Articles/1063910/WebSocket-Server-in-Csharp
            public static void ToggleMask(byte[] maskKey, byte[] payload)
            {
                if (maskKey.Length != MaskKeyLength)
                {
                    //throw new Exception($"MaskKey key must be {MaskKeyLength} bytes");
                }

                // apply the mask key (this is a reversible process so no need to copy the payload)
                for (int i = 0; i < payload.Length; i++)
                {
                    payload[i] = (Byte)(payload[i] ^ maskKey[i % MaskKeyLength]);
                }
            }
            // 출처: https://www.codeproject.com/Articles/1063910/WebSocket-Server-in-Csharp
            public byte[] websocket_read(byte[] pbyData)
            {
                bool fin = (bool)((pbyData[0] & 0x80) != 0);
                bool mask = (pbyData[1] & 0x80) != 0;

                int opcode = pbyData[0] & 0x0f,
                    msglen = pbyData[1] - 128,
                    offset = 2;

                if (msglen == 126)
                {
                    msglen = BitConverter.ToUInt16(new byte[] { pbyData[3], pbyData[2] }, 0);
                    offset = 4;
                }
                //else if (msglen == 127)
                //{
                //    printf("TODO: msglen == 127, needs qword to store msglen");
                //}

                if (msglen == 0)
                {
                    // Ojw.printf("msglen == 0\r\n");
                }
                else if (mask)
                {
                    byte[] decoded = new byte[msglen];
                    byte[] masks = new byte[4] { pbyData[offset], pbyData[offset + 1], pbyData[offset + 2], pbyData[offset + 3] };
                    offset += 4;

                    for (int i = 0; i < msglen; ++i)
                        decoded[i] = (byte)(pbyData[offset + i] ^ masks[i % 4]);

                    //str = Encoding.UTF8.GetString(decoded);
                    //printf("{0}\r\n", str);
                    return decoded;
                }
                //else
                    //printf("mask bit not set\r\n");
                return null;
            }
            public bool m_bWebSocket = false;
            // -1 : No(Default)
            // 0 : Handshake...
            // 1 : Ok
            public int sock_check_websocket(byte[] buffer)
            {
                if (m_bWebSocket == true) return 1; 
                else
                {
                    string str = Ojw.CConvert.BytesToStr_UTF8(buffer);
                    if (Regex.IsMatch(str, "^GET"))
                    {
                        sock_send(HandShake(str));
                        m_bWebSocket = true;
                        return 0;
                    }
                }
                return -1;
            }
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
            public void sleep(int nMilliSecond = 1) { Thread.Sleep(nMilliSecond); }
            public byte[] sock_get_bytes(int nSize)
            {
                return m_bwServer_inData.ReadBytes(nSize);
                //if (sock_connected()) return inData.ReadBytes(nSize);
                //else return null;
            }

            public byte[] GetBytes(int nSize)
            {
                if (sock_connected()) return m_bwServer_inData.ReadBytes(nSize);
                else return null;
            }
            public byte[] GetBytes()
            {
                if (sock_connected())
                {
                    return m_bwServer_inData.ReadBytes(GetBuffer_Length());
                }
                else return null;
            }

            private Thread Reader;             // 읽기 쓰레드
            public bool RunThread(ThreadStart FThread)
            {
                if (sock_started() == false) return false;
                Reader = new Thread(new ThreadStart(FThread));
                Reader.Start();
                return true;
            }
            public bool SetThreadFunction(ThreadStart FThread)
            {
                return RunThread(FThread);
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
            public static int GetIpAddress_Cnt()
            {
                IPAddress[] ipAddressAll = Dns.GetHostAddresses(Dns.GetHostName());
                return ipAddressAll.Length;
                //foreach (IPAddress ipAddress in ipAddressAll)
                //{
                    
                //}
            }
            public static string GetIpAddress(int nIndex)
            {
                IPAddress[] ipAddressAll = Dns.GetHostAddresses(Dns.GetHostName());
                int nPos = ((nIndex < 0) ? 0 : ((nIndex >= ipAddressAll.Length) ? ipAddressAll.Length : nIndex));
                if (ipAddressAll.Length == 0) return string.Empty;
                return ipAddressAll[nIndex].ToString();
                //IPHostEntry hEnt = Dns.GetHostByName(IPAddress.Any.Address);
                //return ((IPHostEntry)Dns.GetHostByName(Dns.GetHostName())).AddressList[nIndex].ToString();
            }
            public static string GetMacAddress(int nIndex)
            {
                //IPAddress[] ipAddressAll = Dns.GetHostAddresses(Dns.GetHostName());

                NetworkInterface[] ntInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                int nPos = ((nIndex < 0) ? 0 : ((nIndex >= ntInterfaces.Length) ? ntInterfaces.Length : nIndex));

                string strMacAddress = string.Empty;
                //IPInterfaceProperties ipinterfaceProp = ntInterfaces[nPos].GetIPProperties();
                //ipinterfaceProp.DnsAddresses.ToString();
                return ntInterfaces[nPos].GetPhysicalAddress().ToString();
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
                catch(Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.ToString());
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

            #region String Packet
            private List<Ojw.CPacket> m_lstPacket = new List<Ojw.CPacket>();

            private bool IsValid_Index(int nIndex) { return (((nIndex >= 0) && (nIndex < m_lstPacket.Count) && (m_lstPacket.Count > 0)) ? true : false); }
            public CPacket[] GetStringPacket() { return m_lstPacket.ToArray(); }
            public CPacket GetStringPacket(int nIndex) { if (IsValid_Index(nIndex) == true) return m_lstPacket[nIndex]; return null; }
            public byte[] GetStringPacket_Bytes(int nIndex) { if (IsValid_Index(nIndex) == true) return m_lstPacket[nIndex].GetBuffers(); return null; }
            public int GetStringPacket_ByteSize(int nIndex) { if (IsValid_Index(nIndex) == true) return m_lstPacket[nIndex].GetBuffers_Count(); return 0; }
            public int GetStringPacket_Size() { return m_lstPacket.Count; }
            public void SendStringPacket_Line(int nLine)
            {
                if (IsConnect() == true)
                {
                    if (GetStringPacket_Size() >= nLine + 1)
                    {
                        //SendPacket(GetStringPacket(nLine));
                        Send(GetStringPacket_Bytes(nLine));
                    }
                }
            }
            public void MakingData(string strPacket, int nMode) // nMode == 0 : (), 1 : {}, 2 : []
            {
                char cStart = '(';
                char cEnd = ')';
                if (nMode == 1)
                {
                    cStart = '{';
                    cEnd = '}';
                }
                else if (nMode == 2)
                {
                    cStart = '[';
                    cEnd = ']';
                }
                char cSeparation = ',';
                //if (txtSeparation.Text != null)
                //{
                //    if (txtSeparation.Text.Length >= 1)
                //    {
                //        cSeparation = txtSeparation.Text[0];
                //        Ojw.CMessage.Write("Separation 문자 = \' {0} \'", cSeparation);
                //    }
                //    else// if (txtSeparation.Text.Length < 1)
                //    {
                //        cSeparation = ',';
                //        Ojw.CMessage.Write("[Warning]Separation Length < 0, Changed \' {0} \'", cSeparation);
                //    }
                //}
                m_lstPacket.Clear();
                string strData = CPacket.MakeSeparation(strPacket, cStart, cEnd);
                strData = Ojw.CConvert.ChangeString(strData, "\r\n", "\n");
                string[] pstrLine = strData.Split('\n');
                //int i = 0;

                byte byData;
                foreach (string strLine in pstrLine)
                {
                    strLine.TrimEnd('\r');
                    CPacket CPack = new CPacket();
                    CPack.strLine = strLine;
                    CPack.lstBuffer.Clear();

                    CPack.lstChecksum_IsChecksum.Clear();
                    CPack.lstChecksum_String.Clear();
                    CPack.lstChecksum_Data.Clear();
                    CPack.strLine_Bytes = String.Empty;
                    CPack.strLine_Bytes_Disp = String.Empty;

                    foreach (string strItem in strLine.Split(cSeparation))
                    {
                        //int nType = 0;
                        if (strItem != null)
                        {
                            if (strItem.Length > 0)
                            {
                                if (strItem[0] == cStart)
                                {
                                    if (strItem[strItem.Length - 1] == cEnd)
                                    {
                                        if (strItem[1] == '#')
                                        {
                                            // Checksum
                                            CPack.lstChecksum_IsChecksum.Add(true);
                                            CPack.lstChecksum_String.Add(strItem.Substring(2, strItem.Length - 3));

                                            byte byChecksum = CPack.CheckSum_Make(strItem.Substring(2, strItem.Length - 3), cStart, cEnd);
                                            CPack.lstChecksum_Data.Add(byChecksum);

                                            CPack.lstBuffer.Add(byChecksum);

                                            CPack.strLine_Bytes += String.Format("0x{0},", Ojw.CConvert.IntToHex(byChecksum, 2));
                                            CPack.strLine_Bytes_Disp += String.Format("(CHK:0x{0})", Ojw.CConvert.IntToHex(byChecksum, 2));
                                        }
                                        else
                                        {
                                            int nRet = CPacket.CheckData(strItem.Substring(1, strItem.Length - 2), out byData);
                                            if ((nRet == 0) || (nRet == 1))
                                            {
                                                //nType = 1;
                                                CPack.lstBuffer.Add(byData);
                                                CPack.strLine_Bytes += String.Format("0x{0},", Ojw.CConvert.IntToHex(byData, 2));
                                                CPack.strLine_Bytes_Disp += ((Ojw.CConvert.IsValidAlpha(byData) == true) ? String.Format("{0}", (char)byData) : String.Format("(0x{0})", Ojw.CConvert.IntToHex(byData, 2)));

                                                CPack.lstChecksum_IsChecksum.Add(false);
                                                CPack.lstChecksum_String.Add(string.Empty);
                                                CPack.lstChecksum_Data.Add(0);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (char cData in strItem)
                                    {
                                        byData = (byte)((byte)cData & 0xff);
                                        CPack.lstBuffer.Add(byData);
                                        CPack.strLine_Bytes += String.Format("0x{0},", Ojw.CConvert.IntToHex(byData, 2));
                                        CPack.strLine_Bytes_Disp += ((Ojw.CConvert.IsValidAlpha(byData) == true) ? String.Format("{0}", (char)byData) : String.Format("(0x{0})", Ojw.CConvert.IntToHex(byData, 2)));

                                        CPack.lstChecksum_IsChecksum.Add(false);
                                        CPack.lstChecksum_String.Add(string.Empty);
                                        CPack.lstChecksum_Data.Add(0);
                                    }
                                }
                            }
                        }
                    }


                    m_lstPacket.Add(CPack);

                    //m_lstPacket.Add(
                    //Ojw.CMessage.Write("[{0}]{1}", i++, strLine);
                }
            }
            #endregion String Packet

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
            public bool SendPacket(byte[] byteData) { return Send(byteData); }
            public bool SendPacket(byte[] byteData, int nLength)
            {
                try
                {
                    byte [] byteTmp = new byte[nLength];
                    if (!m_bConnect) return false;
                    Array.Copy(byteData, 0, byteTmp, 0, nLength);
                    if (m_bConnect) outData.Write(byteTmp);
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

            public int GetBuffer_Length()
            //{
            //    return m_nLength;
            //}
            {
                try
                {
                    //return stream.Length;
                    return m_tcpClient.Available;//inData.BaseStream.Length;
                }
                catch
                {
                    return -1;
                }
            }
            
            // Size 만큼을 읽어감
            public byte[] GetBytes(int nSize)
            {                
                if (m_bConnect) return inData.ReadBytes(nSize);
                else return null;
            }
            private int m_nLength = 0;
            public byte[] GetBytes()
            {
                byte[] buffer = new byte[GetBuffer_Length()];

                if (m_bConnect)
                {
                    int nLength = inData.Read(buffer, 0, buffer.Length);//inData.Read(;//m_tcpClient.Client.Receive(buffer);

                    //m_nLength = nLength;
                    return buffer;
                    //return inData.ReadBytes(nSize);
                }
                else return null;
            }
            public bool SetThreadFunction(ThreadStart FThread)
            {
                return RunThread(FThread);
            }
            private Thread Reader;             // 읽기 쓰레드
            public bool RunThread(ThreadStart FThread)
            {
                if (IsConnect() == false) return false;
                Reader = new Thread(new ThreadStart(FThread));
                Reader.Start();
                return true;
            }
        }
#if false
        public class CUdpServer
        {
            #region 변수
            //private byte m_pbyteServer;
            private bool m_bThread_Server = false;
            public UdpClient m_udpSock;    // 서버
            private Thread m_thServer;          // 스레드
            
            private BinaryWriter m_bwServer_outData;
            private BinaryReader m_bwServer_inData;
            
            //private String m_strOrgPath = "";
            //private int m_nServer_Seq = 0;
            //private int m_nReceived_ServerCmd = 0;
            //private int m_nCntAuth = 0;
            //private bool m_bAuth = false;
            #endregion 변수

            #region 공개
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

                    m_udpSock = new UdpClient(nPort);
                    IPEndPoint clientpnt = new IPEndPoint(IPAddress.Any, 0);
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
            public void sock_stop()
            {
                // 열려있는 포트 닫음(Server)
                //bool bConnected = sock_connected();
                //bool bStarted = sock_started();
                //if (bConnected == true) m_udpSock_Client.Close();
                //if (bStarted == true) m_udpSock.Stop();
                
                m_udpSock.Close();
                m_udpSock = null;


                //m_bAuth = false;
                //m_nCntAuth = 0;
            }
            #endregion 공개

        }
#endif


        public class CUDP
        {
            // 출처: http://nowonbun.tistory.com/165 [명월 일지]
            public void Server_Start(int nPort)
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, nPort);
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                server.Bind(ipep);
                Ojw.CMessage.Write("Server Start");
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = (EndPoint)(sender);

                byte[] _data = new byte[1024];
                server.ReceiveFrom(_data,ref remote);
                Ojw.CMessage.Write("{0} : Recieve: {1}", remote.ToString(), Encoding.Default.GetString(_data));

                _data = Encoding.Default.GetBytes("Test Return Message from Server");
                server.SendTo(_data,_data.Length,SocketFlags.None,remote);

                server.Close();                
            }
            // 출처: http://nowonbun.tistory.com/165 [명월 일지]
            public void Client_Start(string strIpAddress, int nPort)
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(strIpAddress), nPort);
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                byte[] _data = Encoding.Default.GetBytes("Test Message from Client");
                client.SendTo(_data, _data.Length, SocketFlags.None, ipep);

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = (EndPoint)(sender);

                _data = new byte[1024];
                client.ReceiveFrom(_data, ref remote);
                Ojw.CMessage.Write("{0} : Receive: {1}", remote.ToString(), Encoding.Default.GetString(_data));

                client.Close();
            }
        }

    }
}
