//#define _DEBUG_OJW

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Threading;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CSerial
        {
            public CSerial()
            {
            }
            private bool m_bClassEnd = false;
            ~CSerial()
            {
                m_bClassEnd = true;
                if (IsConnect())
                    DisConnect();
            }

            SerialPort m_SerialPort = new SerialPort();
            private Thread Reader;             // reading thread

            #region IsConnect() - for connection checking
            public bool IsConnect() { if (m_SerialPort == null) return false; return m_SerialPort.IsOpen; }
            #endregion IsConnect() - for connection checking

            #region Connect
            /////////////////////////////////////////////////
            // Parity - 0 : None, 1 : Odd, 2 : Even, 3 : Mark, 4 : Space
            // StopBit - 0 : None, 1 : One, 2 : Two, 3 : OnePointFive
            public bool Connect(int nPort, int nBaudRate)//(int nPort, int nBaudRate, int nParity, int nDataBits, int nStopBits)
            {
                m_SerialPort.PortName = "COM" + nPort.ToString();
                m_SerialPort.BaudRate = nBaudRate;
                m_SerialPort.Parity = Parity.None;
                m_SerialPort.DataBits = 8;
                m_SerialPort.StopBits = StopBits.One;
                m_SerialPort.ReceivedBytesThreshold = 1;
                //m_SerialPort.ReadExisting
                //m_SerialPort.ReadBufferSize = 256;
                try
                {
                    m_SerialPort.Open();

                    if (IsConnect() == true)
                    {
                        
                    }
                }
                catch
                {
                    //m_bConnect = false;
                }
                return IsConnect();
            }
            public bool RunThread(ThreadStart FThread)
            {
                if (IsConnect() == false) return false;
                Reader = new Thread(new ThreadStart(FThread));
                Reader.Start();
                return true;
            }
            #endregion

            #region DisConnect() - Thread Stop
            public void DisConnect() { if (IsConnect() == true) { m_SerialPort.Close(); } }
            #endregion DisConnect() - Thread Stop

            #region Read
            public byte GetByte() { return (byte)((IsConnect() == true) ? m_SerialPort.ReadByte() : 0); }
            public byte[] GetBytes()
            {
                if (IsConnect() == false) return null;
                byte[] abyteBuffer = new byte[m_SerialPort.BytesToRead];
                m_SerialPort.Read(abyteBuffer, 0, m_SerialPort.BytesToRead);
                return abyteBuffer;
            }
            public int GetBuffer_Length()
            {
                try
                {
                    return (((IsConnect() == true) && (m_bClassEnd == false)) ? m_SerialPort.BytesToRead : 0);
                }
                catch
                {
                    return 0;
                }
            }
            #endregion Read

            #region Write
            public void SendPacket(byte[] buffer)
            {
                if ((IsConnect() == true) && (m_bClassEnd == false))
                {
                    try
                    {
                        m_SerialPort.Write(buffer, 0, buffer.Length);
#if _DEBUG_OJW
                        Ojw.CMessage.Write("SendPacket");
#endif
                    }
                    catch(Exception e)
                    {
                        Ojw.CMessage.Write_Error("SendPacket - " + e.ToString());
                    }
                }
            }
            public void SendPacket(byte[] buffer, int nLength)
            {
                if ((IsConnect() == true) && (m_bClassEnd == false))
                {
                    m_SerialPort.Write(buffer, 0, nLength);
#if _DEBUG_OJW
                    Ojw.CMessage.Write("SendPacket");
#endif
                }
            }
            #endregion Write

        }
    }
}
