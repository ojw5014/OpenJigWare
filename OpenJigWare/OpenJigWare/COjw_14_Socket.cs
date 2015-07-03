using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Net.Sockets;

namespace OpenJigWare
{
    partial class Ojw
    {
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

                try
                {
                    // 서버 IP 주소와 포트 번호를 이용해 접속 시도
                    m_tcpClient.Connect(strIP, nPort);
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
