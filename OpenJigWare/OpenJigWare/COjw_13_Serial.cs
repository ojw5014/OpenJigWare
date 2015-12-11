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
                catch(Exception ex)
                {
                    CMessage.Write_Error("Port Open Error - " + ex.ToString());
                    //m_bConnect = false;
                }
                return IsConnect();
            }
            public bool Connect(int nPort, int nBaudRate, bool bDtrEnable)//(int nPort, int nBaudRate, int nParity, int nDataBits, int nStopBits)
            {
                m_SerialPort.PortName = "COM" + nPort.ToString();
                m_SerialPort.BaudRate = nBaudRate;
                m_SerialPort.Parity = Parity.None;
                m_SerialPort.DataBits = 8;
                m_SerialPort.StopBits = StopBits.One;
                m_SerialPort.ReceivedBytesThreshold = 1;
                m_SerialPort.DtrEnable = bDtrEnable;
                //m_SerialPort.ReadExisting
                //m_SerialPort.ReadBufferSize = 256;
                try
                {
                    m_SerialPort.Open();

                    if (IsConnect() == true)
                    {

                    }
                }
                catch (Exception ex)
                {
                    CMessage.Write_Error("Port Open Error - " + ex.ToString());
                    //m_bConnect = false;
                }
                return IsConnect();
            }
            public bool SetThreadFunction(ThreadStart FThread)
            {
                return RunThread(FThread);
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

            #region Robolink
#if false
            private void SendPacket()
            {
                byte[] Packet_Buffer = new byte[10] { 0x0A, 0x55, 0x20, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x25 };
                if (keyValueL == 1)
                {
                    Packet_Buffer[6] = 0x00;
                    Packet_Buffer[7] = 0x40;
                }
                else if (keyValueL == 2)
                {
                    Packet_Buffer[6] = 0x00;
                    Packet_Buffer[7] = 0xC0;
                }
                else if (keyValueL == 4)
                {
                    Packet_Buffer[6] = 0xC0;
                    Packet_Buffer[7] = 0x00;
                }
                else if (keyValueL == 8)
                {
                    Packet_Buffer[6] = 0x40;
                    Packet_Buffer[7] = 0x00;

                }
                else if (keyValueL == 5)
                {
                    Packet_Buffer[6] = 0xC0;
                    Packet_Buffer[7] = 0x40;
                }
                else if (keyValueL == 9)
                {
                    Packet_Buffer[6] = 0x40;
                    Packet_Buffer[7] = 0x40;
                }
                else if (keyValueL == 6)
                {
                    Packet_Buffer[7] = 0xC0;
                    Packet_Buffer[6] = 0xC0;
                }
                else if (keyValueL == 10)
                {
                    Packet_Buffer[6] = 0x40;
                    Packet_Buffer[7] = 0xC0;
                }
                else if (keyValueL == 0)
                {
                    Packet_Buffer[6] = 0x00;
                    Packet_Buffer[7] = 0x00;
                }

                if (keyValueR == 1)
                {
                    Packet_Buffer[4] = 0x00;
                    Packet_Buffer[5] = 0x40;
                }
                else if (keyValueR == 2)
                {
                    Packet_Buffer[4] = 0x00;
                    Packet_Buffer[5] = 0xC0;
                }
                else if (keyValueR == 4)
                {
                    Packet_Buffer[4] = 0xC0;
                    Packet_Buffer[5] = 0x00;
                }
                else if (keyValueR == 8)
                {
                    Packet_Buffer[4] = 0x40;
                    Packet_Buffer[5] = 0x00;

                }
                else if (keyValueR == 5)
                {
                    Packet_Buffer[4] = 0xC0;
                    Packet_Buffer[5] = 0x40;
                }
                else if (keyValueR == 9)
                {
                    Packet_Buffer[4] = 0x40;
                    Packet_Buffer[5] = 0x40;
                }
                else if (keyValueR == 6)
                {
                    Packet_Buffer[4] = 0xC0;
                    Packet_Buffer[5] = 0xC0;
                }
                else if (keyValueR == 10)
                {
                    Packet_Buffer[4] = 0x40;
                    Packet_Buffer[5] = 0xC0;
                }
                else if (keyValueR == 0)
                {
                    Packet_Buffer[4] = 0x00;
                    Packet_Buffer[5] = 0x00;
                }

                if (keyValueC == 1) Packet_Buffer[8] = 0xB1;//ResetYaw
                else if (keyValueC == 2) Packet_Buffer[8] = 0x10;//미사일
                else if (keyValueC == 4) Packet_Buffer[8] = 0xA1;//착륙
                else if (keyValueC == 0) Packet_Buffer[8] = 0x00;
                else if (keyValueC == 8) Packet_Buffer[8] = 0x81;//착륙
                else if (keyValueC == 16) Packet_Buffer[8] = 0x82;
                else if (keyValueC == 32) Packet_Buffer[8] = 0x84;
                else if (keyValueC == 64) Packet_Buffer[8] = 0x83;//착륙
                else if (keyValueC == 128) Packet_Buffer[8] = 0x85;
                else if (keyValueC == 256) Packet_Buffer[8] = 0x86;//착륙
                else if (keyValueC == 512) Packet_Buffer[8] = 0x80;//착륙

                Packet_Buffer[9] = (byte)(Packet_Buffer[4] + Packet_Buffer[5] + Packet_Buffer[6] + Packet_Buffer[7] + Packet_Buffer[8] + 0x25);

                //label1.Text = String.Format("{0:X}", Packet_Buffer[0]);
                //label2.Text = String.Format("{0:X}", Packet_Buffer[1]);
                //label3.Text = String.Format("{0:X}", Packet_Buffer[2]);
                //label4.Text = String.Format("{0:X}", Packet_Buffer[3]);
                //label5.Text = String.Format("{0:X}", Packet_Buffer[4]);
                //label6.Text = String.Format("{0:X}", Packet_Buffer[5]);
                //label7.Text = String.Format("{0:X}", Packet_Buffer[6]);
                //label8.Text = String.Format("{0:X}", Packet_Buffer[7]);
                //label9.Text = String.Format("{0:X}", Packet_Buffer[8]);
                //label10.Text = String.Format("{0:X}", Packet_Buffer[9]);

                try
                {
                    SendPacket(Packet_Buffer, 10);
                    // if (serialPort1.IsOpen)
                    // {
                    //serialPort1.Write(Packet_Buffer, 0, 10);
                    // }
                }
                catch (Exception)
                {
                    //message_UART.Text = "Ready... ";
                    //richTextBox1.AppendText(" "+ex);
                }
            }

            enum EControl_t
            {

            }
            private void Form1_KeyDown(int nCommand) 
            {
                switch (nCommand)
                {
                    case Keys.E:             //RISING

                        if (KeyFlag.Rising == 0) { keyValueL += 1; KeyFlag.Rising = 1; }
                        break;

                    case Keys.S:			//LEFTTURN

                        if (KeyFlag.LeftTturn == 0) { keyValueL += 4; KeyFlag.LeftTturn = 1; }
                        break;

                    case Keys.D:			//FALLING

                        if (KeyFlag.Falling == 0) { keyValueL += 2; KeyFlag.Falling = 1; }
                        break;

                    case Keys.F:			//RIGHTTURN

                        if (KeyFlag.RightTurn == 0) { keyValueL += 8; KeyFlag.RightTurn = 1; }
                        break;

                    case Keys.Up:

                        if (KeyFlag.Forward == 0) { keyValueR += 1; KeyFlag.Forward = 1; }
                        break;

                    case Keys.Left:

                        if (KeyFlag.Left == 0) { keyValueR += 4; KeyFlag.Left = 1; }
                        break;

                    case Keys.Down:

                        if (KeyFlag.BackWard == 0) { keyValueR += 2; KeyFlag.BackWard = 1; }
                        break;

                    case Keys.Right:

                        if (KeyFlag.Right == 0) { keyValueR += 8; KeyFlag.Right = 1; }
                        break;

                    case Keys.T:	     //direction

                        if (KeyFlag.Direction == 0) { keyValueC += 1; KeyFlag.Direction = 1; }
                        break;

                    case Keys.Space:	// missile

                        if (KeyFlag.Missile == 0) { keyValueC += 2; KeyFlag.Missile = 1; }
                        break;

                    case Keys.I:	// Trim

                        if (KeyFlag.TrimForward == 0) { keyValueC += 8; KeyFlag.TrimForward = 1; }
                        break;
                    case Keys.K:	// Trim

                        if (KeyFlag.TrimBackWard == 0) { keyValueC += 16; KeyFlag.TrimBackWard = 1; }
                        break;
                    case Keys.U:	// Trim

                        if (KeyFlag.TrimLeftTurn == 0) { keyValueC += 32; KeyFlag.TrimLeftTurn = 1; }
                        break;
                    case Keys.O:	// Trim

                        if (KeyFlag.TrimRightTurn == 0) { keyValueC += 64; KeyFlag.TrimRightTurn = 1; }
                        break;
                    case Keys.J:	// Trim

                        if (KeyFlag.TrimLeft == 0) { keyValueC += 128; KeyFlag.TrimLeft = 1; }
                        break;
                    case Keys.L:	// Trim

                        if (KeyFlag.TrimRight == 0) { keyValueC += 256; KeyFlag.TrimRight = 1; }
                        break;
                    case Keys.Y:	// Trim

                        if (KeyFlag.TrimReset == 0) { keyValueC += 512; KeyFlag.TrimReset = 1; }
                        break;

                    case Keys.Enter:	// stop

                        if (KeyFlag.Stop == 0) { keyValueC += 4; KeyFlag.Stop = 1; }
                        break;
                }

                SendPacket();

            }
#endif
            #endregion Robolink
        }
    }
}
