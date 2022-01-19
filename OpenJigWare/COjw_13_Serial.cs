//#define _DEBUG_OJW

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Management;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CSerial//:SerialPort
        {
            public struct SPortInfo_t
            {
                public string strDescription;
                public int nPort;
            }
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
            public static string [] GetPortNames() { return SerialPort.GetPortNames(); }

            // https://social.msdn.microsoft.com/Forums/windows/en-US/c236cac4-a954-4a70-882d-bc20e2cc6e81/getting-more-information-about-a-serial-port-in-c?forum=winformsdesigner
            public static SPortInfo_t[] GetPortNames_In_Detail(params string[] pstrFilter)
            {
                ManagementClass processClass = new ManagementClass("Win32_PnPEntity");
                ManagementObjectCollection Ports = processClass.GetInstances();
                string device = "No recognized";
                List<SPortInfo_t> lstStrRes = new List<SPortInfo_t>();
                foreach (ManagementObject property in Ports)
                {
                    if (property.GetPropertyValue("Name") != null)
                    {
                        if (property.GetPropertyValue("Name").ToString().Contains("COM"))
                        {
                            int nPass = 0;
                            if (pstrFilter.Length <= 0) nPass = 1;
                            else { for (int i = 0; i < pstrFilter.Length; i++) { if (property.GetPropertyValue("Name").ToString().Contains(pstrFilter[i])) nPass++; } } 

                            if (nPass > 0)
                            {
                                SPortInfo_t SPort = new SPortInfo_t();
                                string str = property.GetPropertyValue("Name").ToString().ToUpper();
                                int nIndex = str.LastIndexOf("COM");
                                str = str.Substring(nIndex + 3);
                                str = str.Substring(0, str.Length - 1);
                                SPort.strDescription = property.GetPropertyValue("Name").ToString();
                                SPort.nPort = CConvert.StrToInt(str);
                                lstStrRes.Add(SPort);
                            }
                        }
                    }
                }
                if (lstStrRes.Count > 0) return lstStrRes.ToArray();
                return null;
            }








            SerialPort m_SerialPort = new SerialPort();
            private Thread Reader;             // reading thread

            private  List<Ojw.CPacket> m_lstPacket = new List<Ojw.CPacket>();

            private bool IsValid_Index(int nIndex) { return (((nIndex >= 0) && (nIndex < m_lstPacket.Count) && (m_lstPacket.Count > 0)) ? true : false); }
            public CPacket[] GetStringPacket() { return m_lstPacket.ToArray(); }
            public CPacket GetStringPacket(int nIndex) { if (IsValid_Index(nIndex) == true) return m_lstPacket[nIndex]; return null; }
            public byte[] GetStringPacket_Bytes(int nIndex) { if (IsValid_Index(nIndex) == true) return m_lstPacket[nIndex].GetBuffers(); return null; }
            public int GetStringPacket_ByteSize(int nIndex) { if (IsValid_Index(nIndex) == true) return m_lstPacket[nIndex].GetBuffers_Count(); return 0; }
            public int GetStringPacket_Size() { return m_lstPacket.Count; }

            #region IsConnect() - for connection checking
            public bool IsConnect() { if (m_SerialPort == null) return false; return m_SerialPort.IsOpen; }
            #endregion IsConnect() - for connection checking

            #region Connect
            private int m_nPort = 0;
            /////////////////////////////////////////////////
            // Parity - 0 : None, 1 : Odd, 2 : Even, 3 : Mark, 4 : Space
            // StopBit - 0 : None, 1 : One, 2 : Two, 3 : OnePointFive
            public bool Connect(int nPort, int nBaudRate)//(int nPort, int nBaudRate, int nParity, int nDataBits, int nStopBits)
            {
                m_SerialPort = new SerialPort();
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
                        m_nPort = nPort;   
                    }
                }
                catch(Exception ex)
                {
                    CMessage.Write_Error("Port Open Error - " + ex.ToString());
                    //m_bConnect = false;
                }
                return IsConnect();
            }
            public void SetDtr(bool bEn) { m_SerialPort.DtrEnable = bEn; }
            public void SetRts(bool bEn) { m_SerialPort.RtsEnable = bEn; }
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
            public bool Connect(int nPort, int nBaudRate, bool bDtrEnable, Parity prt)//(int nPort, int nBaudRate, int nParity, int nDataBits, int nStopBits)
            {
                m_SerialPort.PortName = "COM" + nPort.ToString();
                m_SerialPort.BaudRate = nBaudRate;
                m_SerialPort.Parity = prt;//Parity.None;
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
            public string GetPortName() { return m_SerialPort.PortName; }
            public int GetPortNumber() { return m_nPort; }
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
            public void DisConnect()
            {
                try
                {
                    if (IsConnect() == true)
                    {
                        m_SerialPort.Close();
                        m_SerialPort.Dispose();
                        m_SerialPort = null;
                    }
                }
                catch (Exception ex)
                {
                    m_SerialPort.Dispose();
                    m_SerialPort = null;
                }
            }
            #endregion DisConnect() - Thread Stop

            #region Read
            public byte GetByte() { return (byte)((IsConnect() == true) ? m_SerialPort.ReadByte() : 0); }
            public byte[] GetBytes()
            {
                if (IsConnect() == false) return null;
                int nCount = m_SerialPort.BytesToRead;
                byte[] abyteBuffer = new byte[nCount];
                m_SerialPort.Read(abyteBuffer, 0, nCount);
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
            public void SendStringPacket_Line(int nLine)
            {         
                if (IsConnect() == true)
                {
                    if (GetStringPacket_Size() >= nLine + 1)
                    {
                        //SendPacket(GetStringPacket(nLine));
                        SendPacket(GetStringPacket_Bytes(nLine));                    
                    }
                }
            }
            public void SendPacket(CPacket CPack)
            {
                byte[] buffer = CPack.lstBuffer.ToArray();
                if ((IsConnect() == true) && (m_bClassEnd == false))
                {
                    try
                    {
                        m_SerialPort.Write(buffer, 0, buffer.Length);
#if _DEBUG_OJW
                        Ojw.CMessage.Write("SendPacket");
#endif
                    }
                    catch (Exception e)
                    {
                        Ojw.CMessage.Write_Error("SendPacket - " + e.ToString());
                    }
                }
            }
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
                try
                {
                    if ((IsConnect() == true) && (m_bClassEnd == false))
                    {
                        m_SerialPort.Write(buffer, 0, nLength);
#if _DEBUG_OJW
                    Ojw.CMessage.Write("SendPacket");
#endif
                    }
                }
                catch(Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.ToString());
                    DisConnect();
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


            //////////
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
        }

        // Serial 클래스 내에서 사용
        public class CPacket
        {
            public string strLine;
            public string strLine_Bytes;
            public string strLine_Bytes_Disp;
            public List<byte> lstBuffer = new List<byte>();

            // for checksum
            public List<bool> lstChecksum_IsChecksum = new List<bool>();
            public List<string> lstChecksum_String = new List<string>();
            public List<byte> lstChecksum_Data = new List<byte>();

            public CPacket()
            {
                strLine = String.Empty;
                lstBuffer.Clear();

                lstChecksum_IsChecksum.Clear();
                lstChecksum_String.Clear();
                lstChecksum_Data.Clear();
            }
            ~CPacket()
            {
            }
            public string GetBuffer_StringBytes() { return strLine_Bytes; }
            public string GetBuffer_StringBytes_ForDisp() { return strLine_Bytes_Disp; }
            public bool IsCheckSumByte(int nIndex) { return lstChecksum_IsChecksum[nIndex]; }
            public byte[] GetBuffers() { return lstBuffer.ToArray(); }
            public int GetBuffers_Count() { return lstBuffer.Count; } 
            public static string MakeSeparation(string strData, char cStart, char cEnd)
            {
                string strRet = String.Empty;
                for (int i = 0; i < strData.Length; i++)
                {
                    if (strData[i] == cStart)
                    {
                        if (i == 0)
                            strRet += strData[i];
                        else
                            strRet += String.Format(",{0}", strData[i]);
                    }
                    else if (strData[i] == cEnd)
                    {
                        if (i == strData.Length - 1)
                            strRet += strData[i];
                        else
                            strRet += String.Format("{0},", strData[i]);
                    }
                    else if (strData[i] == ',') // 구분자와 같은 문자라면
                    {
                        strRet += String.Format(",(0x{0}),", Ojw.CConvert.IntToHex((int)(char)(',')));
                    }
                    else strRet += strData[i];
                }
                return strRet;
            }
            private string MakeSeparation2(string strData)
            {
                string strRet = String.Empty;
                for (int i = 0; i < strData.Length; i++)
                {
                    if (IsCommand(strData[i]) == true)
                    {
                        if (i == 0)
                            strRet += strData[i];
                        else
                            strRet += String.Format(",{0}", strData[i]);
                    }
                    else strRet += strData[i];
                }
                return strRet;
            }
            public static int CheckData(String strData, out byte byData)
            {
                int nRet = -1; // -1 : None, 0 : Hexa, 1 : 10진수, 2 : 문자
                byData = 0;
                try
                {
                    String strRet = "";
                    if (strData.Length >= 3)
                    {
                        // hexa 판단
                        if ((strData[0] == '0') && ((strData[1] == 'x') || (strData[1] == 'X')))
                        {
                            strRet = strData.Substring(2);
                            byData = (byte)(Ojw.CConvert.HexStrToInt(strRet) & 0xff);
                            nRet = 0;
                        }
                        // 10 진수 판단
                        else if (Ojw.CConvert.IsDigit(strData) == true)// if (strData[0] == '#')
                        {
                            //strRet = strData.Substring(1);
                            //byData = (byte)(Ojw.CConvert.StrToInt(strRet) & 0xff);
                            byData = (byte)(Ojw.CConvert.StrToInt(strData) & 0xff);
                            nRet = 1;
                        }
                        else nRet = 2; // 문자는 한글자여야 한다.
                    }
                    else
                    {
                        if (Ojw.CConvert.IsDigit(strData) == true)
                        {
                            //strRet = strData.Substring(1);
                            //byData = (byte)(Ojw.CConvert.StrToInt(strRet) & 0xff);
                            byData = (byte)(Ojw.CConvert.StrToInt(strData) & 0xff);
                            nRet = 1;
                        }
                        else
                        {
                            //byData = (byte)((byte)strData[0] & 0xff);
                            nRet = 2;
                        }
                    }

                    // hexa, 10진수, 문자 판단
                    //if ((nRet >= 0) && (nRet <= 2))
                    //{
                    //    strHex = "0x" + Ojw.CConvert.IntToHex((int)byData);
                    //    strInt = Ojw.CConvert.IntToStr((int)byData);
                    //}

                    return nRet;
                }
                catch// (Exception ex)
                {
                    return -1;
                }
            }
            private bool IsCommand(char cData)
            {
                bool bRet = false;
                if (
                    (cData == '+') ||
                    (cData == '-') ||
                    (cData == '*') ||
                    (cData == '/') ||
                    (cData == '%') ||
                    (cData == '|') ||
                    (cData == '&') ||
                    (cData == '^') ||
                    (cData == '<') ||
                    (cData == '>')
                    //(cData == '~') 
                    ) bRet = true;
                return bRet;
            }
            public byte CheckSum_Make(string strData, char cStart, char cEnd)
            {
                byte byteRet = 0;
                string strTmp = MakeSeparation2(strData.ToLower());
                if (IsCommand(strTmp[0]) == false) strTmp = "+" + strTmp;
                string[] pstrData = strTmp.Split(',');

                // 구분자는 ','
                // 연산자는 +, -, *, /, %(나머지). |(or), ^(xor), &(and), ~(비트not)
                // 지정어는 
                // p (position) -> p0  (첫번째(포지션 0) 데이타를 의미)

                //Ojw.CMessage.Write("***CheckSum(Start)***");
                int nValue = 0;
                int nValue2 = 0;
                foreach (string strItem in pstrData)
                {
                    byte byteData;
                    if (strItem.Length > 0)
                    {
                        if (strItem.Length > 2)
                        {
                            try
                            {
                                //bool bHex = false;
                                //if (strItem.IndexOf("0x") >= 0) bHex = true;
                                if (strItem[1] == 'p')
                                {
                                    CPacket.CheckData(strItem.Substring(2), out byteData);
                                    nValue2 = (int)lstBuffer[(int)byteData];
                                }
                                else
                                {
                                    CPacket.CheckData(strItem.Substring(1), out byteData);
                                    nValue2 = (int)byteData;
                                }
                            }
                            catch(Exception ex)
                            {
                                Ojw.CMessage.Write_Error("Checksum error->{0}", ex.ToString());
                            }
                        }
                        else
                        {
                            CPacket.CheckData(strItem.Substring(1), out byteData);
                            nValue2 = (int)byteData;
                        }

                        switch (strItem[0])
                        {
                            // 연산자는 +, -, *, /, %(나머지). |(or), ^(xor), &(and), <(shift bit-left), >(shift bit-right)
                            case '+': nValue += nValue2; break;
                            case '-': nValue -= nValue2; break;
                            case '*': nValue *= nValue2; break;
                            case '/': nValue /= nValue2; break;
                            case '%': nValue %= nValue2; break;
                            case '|': nValue |= nValue2; break;
                            case '^': nValue ^= nValue2; break;
                            case '&': nValue &= nValue2; break;
                            case '<': nValue = ((nValue << nValue2) & 0xff); break;
                            case '>': nValue = ((nValue >> nValue2) & 0xff); break;
                        }
                        //Ojw.CMessage.Write(strItem);
                    }
                }
                //Ojw.CMessage.Write("***CheckSum(End[{0}])***", nValue);

                //for (int i = 0; i < lstChecksum_IsChecksum.Count; i++)
                //{
                //    if (lstChecksum_IsChecksum[i] == true)
                //    {
                //        lstChecksum_
                //    }
                //}
                byteRet = (byte)(nValue & 0xff);
                return byteRet;
            }
        }
    }
}
