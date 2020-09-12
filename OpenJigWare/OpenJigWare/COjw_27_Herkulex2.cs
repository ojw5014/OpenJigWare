using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CHerkulex2
        {            
            #region Define
            // Address
            public readonly int _ADDRESS_TORQUE_CONTROL         = 52;
            public readonly int _ADDRESS_LED_CONTROL  			= 53;
            public readonly int _ADDRESS_VOLTAGE  				= 54;
            public readonly int _ADDRESS_TEMPERATURE  			= 55;
            public readonly int _ADDRESS_PRESENT_CONTROL_MODE  	= 56;
            public readonly int _ADDRESS_TICK  					= 57;
            public readonly int _ADDRESS_CALIBRATED_POSITION  	= 58;

            // Model
            public const int _MODEL_DRS_0101                    = 1;
            public const int _MODEL_DRS_0102                    = 2;
            public const int _MODEL_DRS_0201                    = 3;
            public const int _MODEL_DRS_0202                    = 4;
            public const int _MODEL_DRS_0401                    = 5;
            public const int _MODEL_DRS_0402                    = 6;
            public const int _MODEL_DRS_0601                    = 7;
            public const int _MODEL_DRS_0602                    = 8;
            public const int _MODEL_DRS_0603                    = 9;

            public readonly int _SIZE_MEMORY                    = 256;
            public readonly int _SIZE_MOTOR_MAX                 = 256;//254 + 1; // Bloadcasting 도 염두

            public readonly int _ID_BROADCASTING                = 254;

            //public readonly int _FLAG_ENABLE 0x100


            public readonly int _HEADER1 				        = 0;
            public readonly int _HEADER2 				        = 1;
            public readonly int _SIZE 					        = 2;
            public readonly int _ID 						    = 3;
            public readonly int _CMD 					        = 4;
            public readonly int _CHECKSUM1 				        = 5;
            public readonly int _CHECKSUM2 				        = 6;
            public readonly int _SIZE_PACKET_HEADER 	        = 7;

            public readonly int _FLAG_STOP 				        = 0x01;
            public readonly int _FLAG_MODE_SPEED  		        = 0x02;
            public readonly int _FLAG_LED_GREEN  	    	    = 0x04;
            public readonly int _FLAG_LED_BLUE  		        = 0x08;
            public readonly int _FLAG_LED_RED  			        = 0x10;
            public readonly int _FLAG_NO_ACTION 		        = 0x20;
            #endregion Define

            // if you make your class, just write in here
            #region Var
            private bool m_bProgEnd = false;
            private Ojw.CSerial m_CSerial = new CSerial();
            private byte [,] m_abyRam;//[_SIZE_MOTOR_MAX][_SIZE_MEMORY];
            private byte [,] m_abyRom;//[_SIZE_MOTOR_MAX][_SIZE_MEMORY];

            private int [] m_anPos;//[_SIZE_MOTOR_MAX];
            private byte [] m_abyStatus1;//[_SIZE_MOTOR_MAX];
            private byte [] m_abyStatus2;//[_SIZE_MOTOR_MAX];
            private int [] m_anAxis_By_ID;//[_SIZE_MOTOR_MAX];


            private int m_nSeq_Receive;	
            private int m_nSeq_Receive_Back;

            private int GetAxis_By_ID(int nID) { return (nID == 0xfe) ? 0xfe : m_anAxis_By_ID[nID]; }
            
            private bool m_bShowMessage;

	        private int m_nSeq_Motor;
	        private int m_nSeq_Motor_Back;
	        private int m_nDelay;
	        private CTimer m_CTmr = new CTimer();
	        private CTimer m_CTmr_Timeout = new CTimer();
	        private Thread m_thReceive;

            public CSocket m_CSocket = new CSocket();
            private Thread m_thSock;



	        private int m_nModel;

	        private int m_nTimeout;
	        private bool m_bIgnoredLimit;
	
	        private SRead_t [] m_aSRead;//[_SIZE_MOTOR_MAX];
	        private int m_nReadCnt;
	        private int m_nMotorCnt;
	        private int m_nMotorCnt_Back;
	        private SParam_Axis_t [] m_aSParam_Axis;//[256];
	        private SMot_t [] m_aSMot;//[_SIZE_MOTOR_MAX];
	        private int [] m_anEn;//[_SIZE_MOTOR_MAX];
	        private bool m_bStop;
	        private bool m_bEms;
            private bool m_bMotionEnd;
            private bool m_bStart;
            #endregion Var

            public CHerkulex2() // 생성자 초기화 함수
            {
                Init();
            }
            ~CHerkulex2()
            {
                m_bProgEnd = true;
                Ojw.CTimer.Wait(100);				
                if (IsOpen() == true) Close();
            }

            private void Init()
            {
                m_abReceivedPos = new bool[_SIZE_MOTOR_MAX];

                m_abyRam = new byte[_SIZE_MOTOR_MAX, _SIZE_MEMORY];
                m_abyRom = new byte[_SIZE_MOTOR_MAX, _SIZE_MEMORY];

                m_anPos = new int[_SIZE_MOTOR_MAX];
                m_abyStatus1 = new byte[_SIZE_MOTOR_MAX];
                m_abyStatus2 = new byte[_SIZE_MOTOR_MAX];
                m_anAxis_By_ID = new int[_SIZE_MOTOR_MAX];

                m_aSRead = new SRead_t[_SIZE_MOTOR_MAX];
                m_aSParam_Axis = new SParam_Axis_t[256];
                m_aSMot = new SMot_t[_SIZE_MOTOR_MAX];
                m_anEn = new int[_SIZE_MOTOR_MAX];

                m_bShowMessage = false;
                m_nTimeout = 200;	
                m_nSeq_Motor = 0;
                m_nSeq_Motor_Back = 0;
                m_nDelay = 0;
                	
                m_bIgnoredLimit = false;
                
                m_nModel = 0;
                	
                m_nSeq_Receive = 0;

                //m_bOpen = false;
                m_bStop = false;
                m_bEms = false;
                m_bMotionEnd = false;
                m_bStart = false;

                m_nMotorCnt_Back = m_nMotorCnt = 0;


                Array.Clear(m_abyRam, 0, _SIZE_MOTOR_MAX * _SIZE_MEMORY);
                Array.Clear(m_abyRom, 0, _SIZE_MOTOR_MAX * _SIZE_MEMORY);
                Array.Clear(m_aSParam_Axis, 0, _SIZE_MOTOR_MAX);
                Array.Clear(m_aSMot, 0, _SIZE_MOTOR_MAX);

                Array.Clear(m_abyStatus1, 0, _SIZE_MOTOR_MAX);
                Array.Clear(m_abyStatus2, 0, _SIZE_MOTOR_MAX);
                Array.Clear(m_anAxis_By_ID, 0, _SIZE_MOTOR_MAX);

                Array.Clear(m_anEn, 0, _SIZE_MOTOR_MAX);
                Array.Clear(m_aSRead, 0, _SIZE_MOTOR_MAX);


                m_nReadCnt = 0;

                m_nSeq_Receive = 0;
                m_nSeq_Receive_Back = 0;

                for (int i = 0; i < _SIZE_MOTOR_MAX; i++) SetParam(i, _MODEL_DRS_0101);
                m_bProgEnd = false;	
            }
            public void Clone(out CHerkulex2 CMotor)
            {
                CMotor = new CHerkulex2();
                Array.Copy(m_abReceivedPos, CMotor.m_abReceivedPos, m_abReceivedPos.Length);

                Array.Copy(m_abyRam, CMotor.m_abyRam, m_abyRam.Length);
                Array.Copy(m_abyRom, CMotor.m_abyRom, m_abyRom.Length);
                Array.Copy(m_anPos, CMotor.m_anPos, m_anPos.Length);
                Array.Copy(m_abyStatus1, CMotor.m_abyStatus1, m_abyStatus1.Length);
                Array.Copy(m_abyStatus2, CMotor.m_abyStatus2, m_abyStatus2.Length);

                Array.Copy(m_anAxis_By_ID, CMotor.m_anAxis_By_ID, m_anAxis_By_ID.Length);
                Array.Copy(m_aSRead, CMotor.m_aSRead, m_aSRead.Length);
                Array.Copy(m_aSParam_Axis, CMotor.m_aSParam_Axis, m_aSParam_Axis.Length);
                Array.Copy(m_aSMot, CMotor.m_aSMot, m_aSMot.Length);
                Array.Copy(m_anEn, CMotor.m_anEn, m_anEn.Length);

                CMotor.m_bShowMessage = m_bShowMessage;
                CMotor.m_nTimeout = m_nTimeout;
                CMotor.m_nSeq_Motor = m_nSeq_Motor;
                CMotor.m_nSeq_Motor_Back = m_nSeq_Motor_Back;
                CMotor.m_nDelay = m_nDelay;
                CMotor.m_bIgnoredLimit = m_bIgnoredLimit;
                CMotor.m_nModel = m_nModel;
                CMotor.m_nSeq_Receive = m_nSeq_Receive;
                CMotor.m_bStop = m_bStop;
                CMotor.m_bEms = m_bEms;
                CMotor.m_bMotionEnd = m_bMotionEnd;
                CMotor.m_bStart = m_bStart;
                CMotor.m_nMotorCnt = m_nMotorCnt;
                CMotor.m_nMotorCnt_Back = m_nMotorCnt_Back;

                CMotor.m_nReadCnt = m_nReadCnt;
                CMotor.m_nSeq_Receive = m_nSeq_Receive;
                CMotor.m_nSeq_Receive_Back = m_nSeq_Receive_Back;

                CMotor.m_bProgEnd = m_bProgEnd;
            }
            public bool IsOpen() { return m_CSerial.IsConnect(); }
	        public bool IsStop() { return m_bStop; }
	        public bool IsEms() { return m_bEms; }
            public bool Open(int nComport, int nBaudrate)//, int nModel)		//serial port open
            {
	            if (IsOpen() == false)
	            {
		            // Port Open                    
		            if(m_CSerial.Connect(nComport, nBaudrate) == false) {
			            Ojw.CMessage.Write("Failed to open Serial Device{Comport:{0}, Baudrate:{1})", nComport, nBaudrate);
		                return false;
		            }
		            Ojw.CMessage.Write("Init Serial{0}", nComport);

		            // Thread
                    m_thReceive = new Thread(new ThreadStart(Thread_Receive));
                    m_thReceive.Start();
		            Ojw.CMessage.Write("Init Thread");

		            Clear_Flag();

	            }

	            if (IsOpen() == false)
	            {
		            Ojw.CMessage.Write("[Open()][Error] Connection Fail - Comport {0}, {1}", nComport, nBaudrate);

		            //return;
	            }
                return IsOpen();
            }
            public void Close()								//serial port close
            {
	            if (IsOpen() == true) 
	            {
		            m_CSerial.DisConnect();
		            Ojw.CMessage.Write("Serial Port -> Closed");
	            }
            }

            public bool IsOpen_Socket() { return m_CSocket.IsConnect(); }
            public bool Open_Socket(string strIP, int nPort)
            {
                bool bRet = m_CSocket.Connect(strIP, nPort);
                if (m_CSocket.IsConnect() == true)
                {
                    Ojw.CMessage.Write("[CHerkuleX2] Socket Connected");
                    // Thread
                    m_thSock = new Thread(new ThreadStart(Thread_Socket));
                    m_thSock.Start();
                    Ojw.CMessage.Write("[CHerkuleX2] Init Thread for socket");
                }
                else
                {
                    Ojw.CMessage.Write_Error("[CHerkuleX2] Socket Connection Fail");
                }
                return bRet;
            }
            public void Close_Socket()
            {
                if (m_CSocket.IsConnect() == true)
                {
                    m_CSocket.DisConnect();
                    Ojw.CMessage.Write("[CHerkuleX2] Socket Closed");
                }
            }
            private void Thread_Socket()
            {
                Ojw.CMessage.Write("[Thread_Socket] Running Thread");
                while ((m_bProgEnd == false) && (m_CSocket.IsConnect() == true))
                {
                    int nSize = m_CSocket.GetBuffer_Length();
                    if (nSize > 0)
                    {
                        byte[] buf = m_CSerial.GetBytes();
                        if (buf != null)
                            Parsor(buf, nSize);
                    }
		            Thread.Sleep(1);
	            }

                Ojw.CMessage.Write("[Thread_Socket] Closed Thread");
            }
            private void Thread_Receive()
            {
                byte[] buf;// = new char[256];
                Ojw.CMessage.Write("[Thread_Receive] Running Thread");
	            while((m_bProgEnd == false) && (IsOpen() == true))
	            {
		            int nSize = m_CSerial.GetBuffer_Length();
		            if (nSize > 0)
		            {
                        buf = m_CSerial.GetBytes();
			            //Ojw.CMessage.Write("[Receive]");

                        Parsor(buf, nSize);

			            //Ojw.CMessage.Write("");
		            }
		            Thread.Sleep(1);
	            }

                Ojw.CMessage.Write("[Thread_Receive] Closed Thread");
            }

            private int m_nIndex = 0;
            private byte m_byCheckSum = 0;
            private byte m_byCheckSum1 = 0;
            private byte m_byCheckSum2 = 0;
            private int m_nPacketSize = 0;
            private int m_nDataLength = 0;
            private int m_nIndexData = 0;
            private int m_nCmd = 0;
            private int m_nId = 0;
            private int m_nData_Address = 0;
            private int m_nData_Length = 0;
            private bool m_bHeader = false;

            private void Parsor(byte[] buf, int nSize)
            {
                
                for (int i = 0; i < nSize; i++)
                {
#if false
                    if ((buf[i] == 0xff) && (bHeader == false))
                    //if (((m_bMultiTurn == true) && ((nIndex == 0) && (buf[i] == 0xff))) || ((m_bMultiTurn == false) && (buf[i] == 0xff)))
                    {
                        byCheckSum = 0;
                        nIndexData = 0;
                         
                        nData_Address = 0;
                        nData_Length = 0;

                        nIndex = 1;
                        bHeader = true;
                        continue;
                    }
                    else bHeader = false;
#endif
                    //Ojw.CMessage.Write("[Index={0}]", nIndex);
                    //Ojw.CMessage.Write("0x%02X,", buf[i]);

                    switch (m_nIndex)
                    {
#if true
                        case 0 : 
                            //if (buf[i] == 0xff) 
                            if (((m_bMultiTurn == true) && ((m_nIndex == 0) && (buf[i] == 0xff))) || ((m_bMultiTurn == false) && (buf[i] == 0xff)))
                            {
                                m_byCheckSum = 0;
                                m_nIndexData = 0;

                                m_nData_Address = 0;
                                m_nData_Length = 0;

                                m_nIndex++;
                            }
                            break;
#endif
                        case 1:
                            if (buf[i] == 0xff) m_nIndex++;
                            else m_nIndex = 0;//-1;
                            break;
                        case 2: // Packet Size
                            m_nPacketSize = buf[i];
                            m_nDataLength = m_nPacketSize - _SIZE_PACKET_HEADER - 2;
                            m_byCheckSum = buf[i];
                            if (m_nDataLength < 0) m_nIndex = 0;
                            else m_nIndex++;
                            break;
                        case 3: // ID
                            m_nId = GetAxis_By_ID(buf[i]);
                            m_byCheckSum ^= buf[i];
                            m_nIndex++;
                            break;
                        case 4: // Cmd
                            m_nCmd = buf[i];
                            m_byCheckSum ^= buf[i];
                            m_nIndex++;
                            break;
                        case 5: // CheckSum1
                            m_byCheckSum1 = buf[i];
                            m_nIndex++;
                            break;
                        case 6: // CheckSum2
                            m_byCheckSum2 = buf[i];
                            if ((~m_byCheckSum1 & 0xfe) != m_byCheckSum2) m_nIndex = 0;
                            else m_nIndex++;
                            break;
                        case 7: // Datas...
                            //Ojw.CMessage.Write("[DataLength={0}/{1}]", nIndexData, nDataLength);
                            if (m_nIndexData < m_nDataLength)
                            {
                                if (m_nIndexData == 0) m_nData_Address = buf[i];
                                else if (m_nIndexData == 1) m_nData_Length = buf[i];
                                else m_abyRam[m_nId, m_nData_Address + m_nIndexData - 2] = buf[i];




                                if (++m_nIndexData >= m_nDataLength) m_nIndex++;

                                m_byCheckSum ^= buf[i];
                            }
                            else
                            {
                                //Ojw.CMessage.Write("====== Status1=======");
                                m_abyStatus1[m_nId] = buf[i];
                                m_byCheckSum ^= buf[i];
                                m_nIndex += 2;
                            }
                            break;
                        case 8: // Status 1		
                            //Ojw.CMessage.Write("====== Status1=======");
                            m_abyStatus1[m_nId] = buf[i];
                            m_byCheckSum ^= buf[i];
                            m_nIndex++;


                            /*
                                                    0x01 : Exceed Input Voltage Limit
                                                    0x02 : Exceed allowed POT limit
                                                    0x04 : Exceed Temperature limit
                                                    0x08 : Invalid Packet
                                                    0x10 : Overload detected
                                                    0x20 : Driver fault detected
                                                    0x40 : EEP REG distorted
                                                    0x80 : reserved
                            */

                            break;
                        case 9: // Status 2		
                            //Ojw.CMessage.Write("====== Status2=======(Chk: 0x%02X , 0x%02X", byCheckSum, byCheckSum1);
                            m_abyStatus2[m_nId] = buf[i];
                            m_byCheckSum ^= buf[i];

                            /*
                                                    0x01 : Moving flag
                                                    0x02 : Inposition flag
                                                    0x04 : Checksum Error
                                                    0x08 : Unknown Command
                                                    0x10 : Exceed REG range
                                                    0x20 : Garbage detected
                                                    0x40 : MOTOR_ON flag
                                                    0x80 : reserved
                            */


                            ///////////////////
                            // Done
                            if ((m_byCheckSum & 0xFE) == m_byCheckSum1)
                            {
                                // test
                                byte[] abyteData = new byte[2];
                                abyteData[0] = (byte)(m_abyRam[m_nId, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0)] & 0xff);
                                abyteData[1] = (byte)(m_abyRam[m_nId, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0) + 1] & 0xff);
                                // 0000 0000  0000 0000
                                m_anPos[m_nId] = BitConverter.ToInt16(abyteData, 0);
                                //m_anPos[m_nId] = (short)(((abyteData[0] & 0x0f) << 8) | (abyteData[1] << 0) | ((abyteData[0] & 0x10) << (3 + 8)) | ((abyteData[0] & 0x10) << (2 + 8)) | ((abyteData[0] & 0x10) << (1 + 8)));
                                
                                if (m_bShowMessage == true) Ojw.CMessage.Write("Data Received(<Address({0})Length({1})>Pos[{2}]={3}, Status1 = {4}, Status2 = {5})", m_nData_Address, m_nDataLength, m_nId, m_anPos[m_nId], m_abyStatus1[m_nId], m_abyStatus2[m_nId]);

                                m_abReceivedPos[m_nId] = true;
                                m_nSeq_Receive++;
                            }

                            m_nIndex = 0;
                            break;
                    }
                }	
            }
            private bool [] m_abReceivedPos;// = new bool[_SIZE_MOTOR_MAX];

            public SParam_Axis_t GetParam(int nAxis)
            {
                return m_aSParam_Axis[nAxis];
            }
            
            public void SetParam(int nAxis, int nRealID, int nDir, float fLimitUp, float fLimitDn, float fCenterPos, float fOffsetAngle_Display, float fMechMove, float fDegree)
            {
                //if ((nAxis >= _CNT_MAX_MOTOR) || (nID >= _MOTOR_MAX)) return false;

                m_aSParam_Axis[nAxis].nID = m_aSMot[nAxis].nID = nRealID;
                m_aSParam_Axis[nAxis].nDir = m_aSMot[nAxis].nDir = nDir;
                m_aSParam_Axis[nAxis].fLimitUp = m_aSMot[nAxis].fLimitUp = fLimitUp;
                m_aSParam_Axis[nAxis].fLimitDn = m_aSMot[nAxis].fLimitDn = fLimitDn;
                m_aSParam_Axis[nAxis].fCenterPos = m_aSMot[nAxis].fCenterPos = fCenterPos;
                m_aSParam_Axis[nAxis].fOffsetAngle_Display = fOffsetAngle_Display;
                m_aSParam_Axis[nAxis].fMechMove = m_aSMot[nAxis].fMechMove = fMechMove;
                m_aSParam_Axis[nAxis].fDegree = m_aSMot[nAxis].fDegree = fDegree;
            }
            private bool m_bMultiTurn = false;
            public void SetParam(int nAxis, int nModel)
            {
                if (nAxis > _ID_BROADCASTING) nAxis = _ID_BROADCASTING;
                //Ojw.CMessage.Write("nAxis = {0}, nModel = {1}", nAxis, nModel);
                if (nModel == _MODEL_DRS_0603) m_bMultiTurn = true;
                else if (m_bMultiTurn == true) m_bMultiTurn = false;
                switch (nModel)
                {
                    case _MODEL_DRS_0101:
                        SetParam_RealID(nAxis, nAxis);
                        SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 512.0f);
                        SetParam_Display(nAxis, 0.0f);
                        SetParam_MechMove(nAxis, 1024.0f);
                        SetParam_Degree(nAxis, 333.3f);
                        break;
                    case _MODEL_DRS_0102:
                        SetParam_RealID(nAxis, nAxis);
                        SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 3196.0f);
                        SetParam_Display(nAxis, 0.0f);
                        //SetParam_MechMove(nAxis, 6392.0f);//6391.605f
                        SetParam_MechMove(nAxis, 6391.605f);
                        SetParam_Degree(nAxis, 360.0f);
                        break;
                    case _MODEL_DRS_0201:
                        SetParam_RealID(nAxis, nAxis);
                        SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 512);
                        SetParam_Display(nAxis, 0.0f);
                        SetParam_MechMove(nAxis, 1024.0f);
                        SetParam_Degree(nAxis, 333.3f);
                        break;
                    case _MODEL_DRS_0202:
                        SetParam_RealID(nAxis, nAxis);
                        SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 3196.0f);
                        SetParam_Display(nAxis, 0.0f);
                        //SetParam_MechMove(nAxis, 6392.0f);//6391.605f
                        SetParam_MechMove(nAxis, 6391.605f);
                        SetParam_Degree(nAxis, 360.0f);
                        break;
                    case _MODEL_DRS_0401:
                        SetParam_RealID(nAxis, nAxis);
                        SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 1024.0f);
                        SetParam_Display(nAxis, 0.0f);
                        SetParam_MechMove(nAxis, 2048.0f);
                        SetParam_Degree(nAxis, 333.3f);
                        break;
                    case _MODEL_DRS_0402:
                        SetParam_RealID(nAxis, nAxis);
                        SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 16384.0f);
                        SetParam_Display(nAxis, 0.0f);
                        SetParam_MechMove(nAxis, 12962.099f);
                        SetParam_Degree(nAxis, 360.0f);
                        break;
                    case _MODEL_DRS_0601:
                        SetParam_RealID(nAxis, nAxis);
                        SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 1024.0f);
                        SetParam_Display(nAxis, 0.0f);
                        SetParam_MechMove(nAxis, 2048.0f);
                        SetParam_Degree(nAxis, 333.3f);
                        break;
                    case _MODEL_DRS_0602:
                        SetParam_RealID(nAxis, nAxis);
                        SetParam_Dir(nAxis, 0);
                        SetParam_LimitUp(nAxis, 0.0f);
                        SetParam_LimitDown(nAxis, 0.0f);
                        SetParam_CenterEvdValue(nAxis, 16384.0f);
                        SetParam_Display(nAxis, 0.0f);
                        SetParam_MechMove(nAxis, 12962.099f);
                        SetParam_Degree(nAxis, 360.0f);
                        break;
                    //case _MODEL_DRS_0603:
                    //    SetParam_RealID(nAxis, nAxis);
                    //    SetParam_Dir(nAxis, 0);
                    //    SetParam_LimitUp(nAxis, 0);
                    //    SetParam_LimitDown(nAxis, 0);
                    //    SetParam_CenterEvdValue(nAxis, 0);
                    //    SetParam_Display(nAxis, 0);
                    //    SetParam_MechMove(nAxis, 12962.099);
                    //    SetParam_Degree(nAxis, 360);
                    //    break;
                }
            }

            public void SetParam_RealID(int nAxis, int nRealID) { m_aSParam_Axis[nAxis].nID = m_aSMot[nAxis].nID = nRealID; m_anAxis_By_ID[nRealID] = nAxis; }
            public void SetParam_Dir(int nAxis, int nDir) { m_aSParam_Axis[nAxis].nDir = m_aSMot[nAxis].nDir = nDir; }
            public void SetParam_LimitUp(int nAxis, float fLimitUp) { m_aSParam_Axis[nAxis].fLimitUp = m_aSMot[nAxis].fLimitUp = fLimitUp; }
            public void SetParam_LimitDown(int nAxis, float fLimitDn) { m_aSParam_Axis[nAxis].fLimitDn = m_aSMot[nAxis].fLimitDn = fLimitDn; }
            public void SetParam_CenterEvdValue(int nAxis, float fCenterPos) { m_aSParam_Axis[nAxis].fCenterPos = m_aSMot[nAxis].fCenterPos = fCenterPos; }
            public void SetParam_Display(int nAxis, float fOffsetAngle_Display) { m_aSParam_Axis[nAxis].fOffsetAngle_Display = fOffsetAngle_Display; }
            public void SetParam_MechMove(int nAxis, float fMechMove) { m_aSParam_Axis[nAxis].fMechMove = m_aSMot[nAxis].fMechMove = fMechMove; }
            public void SetParam_Degree(int nAxis, float fDegree) { m_aSParam_Axis[nAxis].fDegree = m_aSMot[nAxis].fDegree = fDegree; }

            public void Stop(int nAxis) // no stop flag setting
            {
                //	if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                Set_Turn(nAxis, 0);
                //	Set_Flag_Stop(nAxis, true);
                Send_Motor(1000);
            }
            public void Stop()
            {
                Set_Turn(254, 0);
                Send_Motor(100);
                m_bStop = true;
            }
            public void Ems()
            {
                Stop();
                SetTorque(false, false);
                m_bEms = true;
            }

            public byte GetErrorCode(int nAxis) { return m_abyStatus1[nAxis]; }
            public bool IsError(int nAxis)
            {
                if (m_abyStatus1[nAxis] != 0)
                {
                    return true;
                }
                return false;
                /*
                    0x01 : Exceed Input Voltage Limit
                    0x02 : Exceed allowed POT limit
                    0x04 : Exceed Temperature limit
                    0x08 : Invalid Packet
                    0x10 : Overload detected
                    0x20 : Driver fault detected
                    0x40 : EEP REG distorted
                    0x80 : reserved
                */
            }

            public bool IsWarning(int nAxis)
            {
                // Status 2
                if ((m_abyStatus2[nAxis] & 0x43) != 0)
                {
                    return true;
                }
                return false;

                /*
                    0x01 : Moving flag
                    0x02 : Inposition flag
                    0x04 : Checksum Error
                    0x08 : Unknown Command
                    0x10 : Exceed REG range
                    0x20 : Garbage detected
                    0x40 : MOTOR_ON flag
                    0x80 : reserved
                    */
            }
            //////////////////////////////////////////////////////////
            // Reboot 
            public void Reboot() { Reset(_ID_BROADCASTING); }
            public void Reboot(int nAxis)
            {
	            if (nAxis < 0xfe) Clear_Flag(nAxis);
                   else
	            {
	                for (int i = 0; i < _SIZE_MOTOR_MAX; i++) Clear_Flag(i);
	            }

	            int nID = m_aSMot[nAxis].nID;
	            int nDefaultSize = _CHECKSUM2 + 1;
	            byte [] pbyteBuffer = new byte[nDefaultSize];
	            //(char *)pbyteBuffer = (char
	            // Header
	            pbyteBuffer[_HEADER1] = 0xff;
	            pbyteBuffer[_HEADER2] = 0xff;
	            // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
	            pbyteBuffer[_ID] = (byte)(nID & 0xff);
	            // Cmd
	            pbyteBuffer[_CMD] = 0x09; // Reset

	            //Packet Size
	            pbyteBuffer[_SIZE] = (byte)((nDefaultSize) & 0xff);

	            MakeCheckSum(nDefaultSize, pbyteBuffer);//, out pbyteBuffer[_CHECKSUM1], out pbyteBuffer[_CHECKSUM2]);

	            SendPacket(pbyteBuffer, nDefaultSize);

	            Clear_Flag();


            // Initialize variable
	            m_bStop = false;
	            m_bEms = false;
	            m_nMotorCnt_Back = m_nMotorCnt = 0;

            }

            ///////////////////////////////////
            // Motor Control - Reset
            public void Reset() { Reset(_ID_BROADCASTING); }
            public void Reset(int nAxis) 
            {
	            // Clear Variable
	            m_bStop = false;
	            m_bEms = false;
	
	            int nID = m_aSMot[nAxis].nID;
	            int i = 0;
	            byte [] pbyteBuffer = new byte[3];
	            // Data
                pbyteBuffer[i++] = (byte)(48 + ((m_bMultiTurn == true) ? 4 : 0)); //48;// 48번 레지스터 명령
	            ////////
	            pbyteBuffer[i++] = 0x01;// 이후의 레지스터 사이즈
	            pbyteBuffer[i++] = 0x00; // led value

	            Make_And_Send_Packet(nID, 0x03, i, pbyteBuffer);
	            //pbyteBuffer = null;
            }

            /////////////////////////////////
            public void SetLimitEn(bool bOn) { m_bIgnoredLimit = !bOn; }
            public bool GetLimitEn() { return !m_bIgnoredLimit; }

            // 1111 1101
            public int Get_Flag(int nAxis) { return m_aSMot[nAxis].nFlag; }

            public int Clip(int nLimitValue_Up, int nLimitValue_Dn, int nData)
            {
                if (GetLimitEn() == false) return nData;

                int nRet = ((nData > nLimitValue_Up) ? nLimitValue_Up : nData);
                return ((nRet < nLimitValue_Dn) ? nLimitValue_Dn : nRet);
            }
            public float Clip(float fLimitValue_Up, float fLimitValue_Dn, float fData)
            {
                if (GetLimitEn() == false) return fData;
                float fRet = ((fData > fLimitValue_Up) ? fLimitValue_Up : fData);
                return ((fRet < fLimitValue_Dn) ? fLimitValue_Dn : fRet);
            }

            public int CalcLimit_Evd(int nAxis, int nValue)
            {
                if ((Get_Flag_Mode(nAxis) == 0) || (Get_Flag_Mode(nAxis) == 2))
                {
                    int nPulse = nValue & 0x4000;
                    if (m_bMultiTurn == false)
                    {
                        //nValue &= 0x4000;
                        //nValue &= 0x3fff;
                    }

                    //nValue &= 0x3fff;
                    int nUp = 100000;
                    int nDn = -nUp;
                    if (m_aSMot[nAxis].fLimitUp != 0) nUp = CalcAngle2Evd(nAxis, m_aSMot[nAxis].fLimitUp);
                    if (m_aSMot[nAxis].fLimitDn != 0) nDn = CalcAngle2Evd(nAxis, m_aSMot[nAxis].fLimitDn);
                    if (nUp < nDn) { int nTmp = nUp; nUp = nDn; nDn = nTmp; }
                    return (Clip(nUp, nDn, nValue) | nPulse);
                }
                return nValue;
            }
            public float CalcLimit_Angle(int nAxis, float fValue)
            {
                if ((Get_Flag_Mode(nAxis) == 0) || (Get_Flag_Mode(nAxis) == 2))
                {
                    float fUp = 100000.0f;
                    float fDn = -fUp;
                    if (m_aSMot[nAxis].fLimitUp != 0) fUp = m_aSMot[nAxis].fLimitUp;
                    if (m_aSMot[nAxis].fLimitDn != 0) fDn = m_aSMot[nAxis].fLimitDn;
                    return Clip(fUp, fDn, fValue);
                }
                return fValue;
            }

            public int CalcTime_ms(int nTime)
            {
                // 1 Tick 당 11.2 ms => 1:11.2=x:nTime => x = nTime / 11.2
                return ((nTime <= 0) ? 1 : (int)Math.Round((float)nTime / 11.2f));
            }
            public int CalcAngle2Evd(int nAxis, float fValue)
            {
                fValue *= ((m_aSMot[nAxis].nDir == 0) ? 1.0f : -1.0f);
                int nData = 0;
                if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                {
                    nData = (int)Math.Round(fValue);
                    //Ojw.CMessage.Write("Speed Turn");
                }
                else
                {
                    // 위치제어
                    nData = (int)Math.Round((m_aSMot[nAxis].fMechMove * fValue) / m_aSMot[nAxis].fDegree);
                    nData = nData + (int)Math.Round(m_aSMot[nAxis].fCenterPos);

                    //if (nAxis == 2)
                    //    Ojw.CMessage.Write("[{0}]Angle(%.2f), Mech(%.2f), Degree(%.2f), Center(%.2f), nData({1})", nAxis, fValue, m_aSMot[nAxis].fMechMove, m_aSMot[nAxis].fDegree, m_aSMot[nAxis].fCenterPos, nData);
                    //Ojw.CMessage.Write("[{0}]Angle(%.2f), Mech(%.2f), Degree(%.2f), Center(%.2f), nData({1})", nAxis, fValue, m_aSMot[nAxis].fMechMove, m_aSMot[nAxis].fDegree, m_aSMot[nAxis].fCenterPos, nData);
                }

                return nData;
            }
            public float CalcEvd2Angle(int nAxis, int nValue)
            {
                float fValue = ((m_aSMot[nAxis].nDir == 0) ? 1.0f : -1.0f);
                float fValue2 = 0.0f;
                if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                    fValue2 = (float)nValue * fValue;
                else                                // 위치제어
                {
                    fValue2 = (float)(((m_aSMot[nAxis].fDegree * ((float)(nValue - (int)Math.Round(m_aSMot[nAxis].fCenterPos)))) / m_aSMot[nAxis].fMechMove) * fValue);
                    //if (nAxis == 2)
                    //    Ojw.CMessage.Write("[{0}]Angle(%.2f), Mech(%.2f), Degree(%.2f), Center(%.2f), nData({1})", nAxis, fValue, m_aSMot[nAxis].fMechMove, m_aSMot[nAxis].fDegree, m_aSMot[nAxis].fCenterPos, nData);
                }
                return fValue2;
            }
            ////////////////////////////////////
            // Motor Control - Torq On / Off
            public void SetTorque(int nAxis, bool bDrvOn, bool bSrvOn) 	//torque on / Off
            {
	            int nID = m_aSMot[nAxis].nID;
	            int i = 0;
	            byte byOn = 0;
	            byOn |= (byte)((bDrvOn == true) ? 0x40 : 0x00);
	            byOn |= (byte)((bSrvOn == true) ? 0x20 : 0x00);
	            byte [] pbyteBuffer = new byte[50];
	            // Data
                pbyteBuffer[i++] = (byte)((_ADDRESS_TORQUE_CONTROL & 0xff) + ((m_bMultiTurn == true) ? 4 : 0));//;// 52번 레지스터 명령
	            ////////
	            pbyteBuffer[i++] = 0x01;// 이후의 레지스터 사이즈
	            pbyteBuffer[i++] = byOn;

	            Make_And_Send_Packet(nID, 0x03, i, pbyteBuffer);
            }
            public void SetTorque(bool bDrvOn, bool bSrvOn) { SetTorque((int)_ID_BROADCASTING, bDrvOn, bSrvOn); }

            /////////////////////////////////////
            // Data Command(No motion) - just setting datas   		=> use with Send_Motor
            // ---- Position Control ----
            public void Set(int nAxis, int nEvd)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push_Id(nAxis);
                Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                Set_Flag_Mode(nAxis, false);
                m_aSMot[nAxis].fPos = (float)CalcLimit_Evd(nAxis, nEvd);
                Set_Flag_NoAction(nAxis, false);
                //Push_Id(nAxis);	
            }
            public int Get(int nAxis) { return (int)Math.Round(m_aSMot[nAxis].fPos); }

            public void Set_Angle(int nAxis, float fAngle)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push_Id(nAxis);
                Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                Set_Flag_Mode(nAxis, false);
                m_aSMot[nAxis].fPos = CalcLimit_Evd(nAxis, CalcAngle2Evd(nAxis, fAngle));
                Set_Flag_NoAction(nAxis, false);
                //Push_Id(nAxis);	
            }
            public float Get_Angle(int nAxis) { return CalcEvd2Angle(nAxis, (int)m_aSMot[nAxis].fPos); }

            // ---- Speed Control ----
            public void Set_Turn(int nAxis, int nEvd)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push_Id(nAxis);
                Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                Set_Flag_Mode(nAxis, true);
                m_aSMot[nAxis].fPos = CalcLimit_Evd(nAxis, nEvd);
                Set_Flag_NoAction(nAxis, false);
                //Push_Id(nAxis);	
            }
            public int Get_Turn(int nAxis) { return (int)Math.Round(m_aSMot[nAxis].fPos); }
            public int Get_Pos_Evd(int nAxis)
            {
                int nValue = 0;
                if (m_bMultiTurn == false)
                {
#if false
                    byte[] abyteData = new byte[2];
                    abyteData[1] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION] & 0xff);
                    abyteData[0] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 1] & 0xff);
                    // 0000 0000  0000 0000
                    return (int)(((abyteData[0] & 0x0f) << 8) | (abyteData[1] << 0) | ((abyteData[0] & 0x10) << (3 + 8)) | ((abyteData[0] & 0x10) << (2 + 8)) | ((abyteData[0] & 0x10) << (1 + 8)));
#else
                    byte[] abyteData = new byte[2];
                    abyteData[0] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0)] & 0xff);
                    abyteData[1] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + ((m_bMultiTurn == true) ? 4 : 0) + 1] & 0xff);
		            // 0000 0000  0000 0000
                    nValue = BitConverter.ToInt16(abyteData, 0);
                    //nValue = (int)(((abyteData[0] & 0x0f) << 8) | (abyteData[1] << 0) | ((abyteData[0] & 0x10) << (3 + 8)) | ((abyteData[0] & 0x10) << (2 + 8)) | ((abyteData[0] & 0x10) << (1 + 8)));	
#endif
                }
                else
	            {
#if false
		            //bool bMinus = false;
                    //	0x10 00 00
                    byte[] abyteData = new byte[4];
                    abyteData[3] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 3] & 0xff);
                    abyteData[2] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 2] & 0xff);
		            /*if (((abyteData[2] & 0x80) != 0) || (abyteData[3] != 0))
		            {
			            bMinus = true;
			            //abyteData[3] |= 0xff;
			            //abyteData[2] |= 0xf0;
		            }*/
                    abyteData[1] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 1] & 0xff);
                    abyteData[0] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 0] & 0xff);
		            // 0000 0000  0000 0000
	 	            nValue = (int)(
	 					            ((abyteData[3] & 0xff) << 24) | 
	 					            ((abyteData[2] & 0xff) << 16) | 
	 					            ((abyteData[1] & 0xff) << 8) | 
	 					            ((abyteData[0] & 0xff) << 0) 
	 					            );// | ( ? 0xfffffffffff00000 : 0);
	 	            /*if ((bMinus == true) && (nValue > 0))
 		            {
 			            printf("nValue = %d, %d\r\n", nValue , nValue - 0x100000);
 			            nValue -= 0xf00000;
 		            }*/
		            //printf("0x%02x%02x%02x%02x:0x%08xf\r\n", abyteData[3], abyteData[2], abyteData[1], abyteData[0], nValue);
#else
                    //bool bMinus = false;
                    //	0x10 00 00
                    byte[] abyteData = new byte[4];
                    abyteData[3] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 3] & 0xff);
                    abyteData[2] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 2] & 0xff);
		            /*if (((abyteData[2] & 0x80) != 0) || (abyteData[3] != 0))
		            {
			            bMinus = true;
			            //abyteData[3] |= 0xff;
			            //abyteData[2] |= 0xf0;
		            }*/
                    abyteData[1] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 1] & 0xff);
                    abyteData[0] = (byte)(m_abyRam[nAxis, _ADDRESS_CALIBRATED_POSITION + 4 + 0] & 0xff);
		            // 0000 0000  0000 0000
	 	            nValue = (int)(
	 					            ((abyteData[3] & 0xff) << 24) | 
	 					            ((abyteData[2] & 0xff) << 16) | 
	 					            ((abyteData[1] & 0xff) << 8) | 
	 					            ((abyteData[0] & 0xff) << 0) 
	 					            );// | ( ? 0xfffffffffff00000 : 0);
	 	            /*if ((bMinus == true) && (nValue > 0))
 		            {
 			            printf("nValue = %d, %d\r\n", nValue , nValue - 0x100000);
 			            nValue -= 0xf00000;
 		            }*/
		            //printf("0x%02x%02x%02x%02x:0x%08xf\r\n", abyteData[3], abyteData[2], abyteData[1], abyteData[0], nValue);
#endif
	            }
                return nValue;
            }
            public float Get_Pos_Angle(int nAxis) { return CalcEvd2Angle(nAxis, Get_Pos_Evd(nAxis)); }

            ///////////////////////////////////// -> Mode & NoAction : no use "PushId()"
            // Led Control   									=> use with Send_Motor
            public void Clear_Flag() { for (int i = 0; i < 256; i++) m_aSMot[i].nFlag = _FLAG_NO_ACTION; }
            public void Clear_Flag(int nAxis) { m_aSMot[nAxis].nFlag = _FLAG_NO_ACTION; }//{ m_aSMot[nAxis].nFlag = nFlag; Push_Id(nAxis); }
            public void Set_Flag(int nAxis, bool bStop, bool bMode_Speed, bool bLed_Green, bool bLed_Blue, bool bLed_Red, bool bNoAction) { m_aSMot[nAxis].nFlag = ((bStop == true) ? _FLAG_STOP : 0) | ((bMode_Speed == true) ? _FLAG_MODE_SPEED : 0) | ((bLed_Green == true) ? _FLAG_LED_GREEN : 0) | ((bLed_Blue == true) ? _FLAG_LED_BLUE : 0) | ((bLed_Red == true) ? _FLAG_LED_RED : 0) | ((bNoAction == true) ? _FLAG_NO_ACTION : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Stop(int nAxis, bool bStop) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xfe) | ((bStop == true) ? _FLAG_STOP : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Mode(int nAxis, bool bMode_Speed) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xfd) | ((bMode_Speed == true) ? _FLAG_MODE_SPEED : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Led(int nAxis, bool bGreen, bool bBlue, bool bRed) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xe3) | ((bGreen == true) ? _FLAG_LED_GREEN : 0) | ((bBlue == true) ? _FLAG_LED_BLUE : 0) | ((bRed == true) ? _FLAG_LED_RED : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Led_Green(int nAxis, bool bGreen) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xfb) | ((bGreen == true) ? _FLAG_LED_GREEN : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Led_Blue(int nAxis, bool bBlue) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xf7) | ((bBlue == true) ? _FLAG_LED_BLUE : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            public void Set_Flag_Led_Red(int nAxis, bool bRed) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xef) | ((bRed == true) ? _FLAG_LED_RED : 0); Push_Id(nAxis); Read_Motor_Push(nAxis); }
            // NoAction 은 push 안함
            public void Set_Flag_NoAction(int nAxis, bool bNoAction) { m_aSMot[nAxis].nFlag = (m_aSMot[nAxis].nFlag & 0xdf) | ((bNoAction == true) ? _FLAG_NO_ACTION : 0); }

            // 1111 1101
            //public int 	Get_Flag(int nAxis) { return m_aSMot[nAxis].nFlag; }
            public int Get_Flag_Mode(int nAxis) { return (((m_aSMot[nAxis].nFlag & _FLAG_MODE_SPEED) != 0) ? 1 : 0); }

            public bool Get_Flag_Led_Green(int nAxis) { return (((m_aSMot[nAxis].nFlag & 0x04) != 0) ? true : false); }
            public bool Get_Flag_Led_Blue(int nAxis) { return (((m_aSMot[nAxis].nFlag & 0x08) != 0) ? true : false); }
            public bool Get_Flag_Led_Red(int nAxis) { return (((m_aSMot[nAxis].nFlag & 0x10) != 0) ? true : false); }

            //////////////////////////////////////
            // Motor Control - Move Motor(Action)

            public void Send_Motor(int nMillisecond)
            {
	            if ((m_bStop == true) || (m_bEms == true)) return;
	
	            m_nMotorCnt_Back = m_nMotorCnt; // 나중에 waitaction 에서 사용

	            int nID;
	            int i = 0;
	            ////////////////////////////////////////////////

	            byte [] pbyteBuffer = new byte[256];//[1 + 4 * m_nMotorCnt];
	            int nPos;
	            int nFlag;
		
	            // region S-Jog Time
	            int nCalcTime = CalcTime_ms(nMillisecond);
	            pbyteBuffer[i++] = (byte)(nCalcTime & 0xff);
                if (m_bMultiTurn == true) pbyteBuffer[i++] = (byte)((nCalcTime >> 8) & 0xff);
	            int nCnt = m_nMotorCnt;//_SIZE_MOTOR_MAX;//m_nMotorCnt;
	            for (int nAxis2 = 0; nAxis2 < nCnt; nAxis2++)
	            {
		            int nAxis = Pop_Id();//nAxis2;// i;//Pop_Id();
                    //Ojw.CMessage.Write("ID={0}", nAxis);
		            if (m_aSMot[nAxis].bEn == true)
		            {
                        //Ojw.CMessage.Write("ID={0}", nAxis);
			            //nPos |= _JOG_MODE_SPEED << 10;  // 속도제어 
			            // Position
			            nPos = Get(nAxis);
                        if (m_bMultiTurn == false)
                        {
                            if (nPos < 0)
                            {
                                nPos *= -1;
                                nPos |= 0x4000;
                            }
                        }
                        else
                        {
                        }

                        if (m_bMultiTurn == false)
                        {
                            pbyteBuffer[i++] = (byte)(nPos & 0xff);
                            pbyteBuffer[i++] = (byte)((nPos >> 8) & 0xff);
                        }
                        else
                        {
                            //printf("nPos=%d\r\n", nPos);
				            uint unPos = (uint)nPos;
				            pbyteBuffer[i++] = (byte)(unPos & 0xff);
				            pbyteBuffer[i++] = (byte)((unPos >> 8) & 0xff);
				            pbyteBuffer[i++] = (byte)((unPos >> 16) & 0xff);
				            pbyteBuffer[i++] = (byte)((unPos >> 24) & 0xff);
                        }
			            // Set-Flag
			            nFlag = Get_Flag(nAxis);
			            pbyteBuffer[i++] = (byte)(nFlag & 0xff);
			            Set_Flag_NoAction(nAxis, true); // 동작 후 모터 NoAction을 살려둔다.

			            // 모터당 아이디(후면에 붙는다)
			            nID = GetID_By_Axis(nAxis);
			            pbyteBuffer[i++] = (byte)(nID & 0xff);

			            m_aSMot[nAxis].bEn = false;
			            ////////////////////////////////////////////////
		            }
		
	            }

	            Make_And_Send_Packet(0xfe, 0x06, i, pbyteBuffer);
	            //pbyteBuffer = null;

	            Clear_Flag();
	
	            //m_nSeq_Motor++; // reserve;
                m_nDelay = nMillisecond;
	            Wait_Ready();
            }

            public void Wait_Ready()
            {
                m_CTmr.Set();
            }

            private const int _CNT_RETRIEVE = 5;
            private int m_nRetrieve = 0;
            public int m_nReadMotor_Index = 0;
            public bool Wait_Motor(int nMilliseconds)
            {
                if (Read_Motor_IsReceived() == true) Read_Motor();
#if true
                else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
                {
                    if (m_CTmr_Timeout.Get() > m_nTimeout)
                    {
                        Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                        m_CTmr_Timeout.Set();
                        m_nRetrieve++;
                    }
                }
                else if (m_CTmr_Timeout.Get() > m_nTimeout)
#else
                else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif
                {
                    Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                    //m_nSeq_Receive_Back = m_nSeq_Receive;
                    Read_Motor();
                }
                while (m_CTmr.Get() < nMilliseconds)
                {
                    //if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { m_nSeq_Motor_Back = m_nSeq_Motor; return false; }
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { Sync_Seq(); return false; }

                    if (Read_Motor_IsReceived() == true) Read_Motor();
#if true
		            else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
		            {
			            if (m_CTmr_Timeout.Get() > m_nTimeout)
			            {
				            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
				            m_CTmr_Timeout.Set();
				            m_nRetrieve++;
			            }
		            }
		            else if  (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
		            else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif	
                    {
                        Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                        //m_nSeq_Receive_Back = m_nSeq_Receive;
                        Read_Motor();
                    }

                    Application.DoEvents();//Thread.Sleep(0);
                }
                Sync_Seq();
                //m_nSeq_Motor_Back = m_nSeq_Motor;
                return true;
            }
            public bool Wait_Motor()
            {
                if (Read_Motor_IsReceived() == true) Read_Motor();
                //else if (m_CTmr_Timeout.Get() > m_nTimeout)
#if true
	            else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
	            {
		            if (m_CTmr_Timeout.Get() > m_nTimeout)
		            {
			            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
			            m_CTmr_Timeout.Set();
			            m_nRetrieve++;
		            }
	            }
	            else if  (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
	            else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif	
                {
                    Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                    //m_nSeq_Receive_Back = m_nSeq_Receive;
                    Read_Motor();
                }
                while (m_CTmr.Get() < m_nDelay)
                {
                    //if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { m_nSeq_Motor_Back = m_nSeq_Motor; return false; }
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { Sync_Seq(); return false; }
                    if (Read_Motor_IsReceived() == true) Read_Motor();
                    //else if (m_CTmr_Timeout.Get() > m_nTimeout)
#if true
		            else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
		            {
			            if (m_CTmr_Timeout.Get() > m_nTimeout)
			            {
				            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
				            m_CTmr_Timeout.Set();
				            m_nRetrieve++;
			            }
		            }
		            else if  (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
		            else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif	
                    {
                        Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                        //m_nSeq_Receive_Back = m_nSeq_Receive;
                        Read_Motor();
                    }
                    Application.DoEvents();
                }
                Sync_Seq();//m_nSeq_Motor_Back = m_nSeq_Motor;
                return true;
            }

            private bool Wait_Position(int nAxis, float fTargetAngle, int nMilliseconds)
            {
	            if (Read_Motor_IsReceived() == true) Read_Motor();	
#if true
	            else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
	            {
		            if (m_CTmr_Timeout.Get() > m_nTimeout)
		            {
			            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
			            m_CTmr_Timeout.Set();
			            m_nRetrieve++;
		            }
	            }
	            else if  (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
	            else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif
	            {
		            Ojw.CMessage.Write("[Receive-Wait_Motor()*] Time out Error(%d ms)- ID[%d]Seq[%d]Seq_Back[%d]Index[%d]\r\n", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID, m_nSeq_Receive, m_nSeq_Receive_Back, m_nReadMotor_Index);;
		            //m_nSeq_Receive_Back = m_nSeq_Receive;

		            Read_Motor();	
	            }
	            float [] afMot = new float[_SIZE_MOTOR_MAX];
                bool [] abMot = new bool[_SIZE_MOTOR_MAX];
                while (m_CTmr.Get() < m_nDelay) 
	            {
		            if (abMot[nAxis] == false) 
		            {
			            if (m_abReceivedPos[nAxis] == true) 
			            {
				            abMot[nAxis] = true;
				            afMot[nAxis] = Get_Pos_Angle(nAxis);
			            } 
		            }
		            else
		            {
			            if ((Get_Angle(nAxis) - afMot[nAxis]) >= 0)
			            {
				            if (Get_Pos_Angle(nAxis) >= fTargetAngle)
				            {
					            //printf("Break\n");
					            break;
				            }
			            }
			            else
			            {
				            if (Get_Pos_Angle(nAxis) <= fTargetAngle)
				            {
					            //printf("Break\n");
					            break;
				            }
			            }
		            }


                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { Sync_Seq(); return false; }
		            //if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { m_nSeq_Motor_Back = m_nSeq_Motor;; return false; }//m_nSeq_Motor_Back = m_nSeq_Motor; return false; }		
		            if (Read_Motor_IsReceived() == true) Read_Motor();	
#if true
		            else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
		            {
			            if (m_CTmr_Timeout.Get() > m_nTimeout)
			            {
				            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
				            m_CTmr_Timeout.Set();
				            m_nRetrieve++;
			            }
		            }
		            else if  (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
		            else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif		
		            {
			            Ojw.CMessage.Write("[Receive-Wait_Motor()] Time out Error(%d ms)- ID[%d]Seq[%d]Seq_Back[%d]Index[%d]\r\n", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID, m_nSeq_Receive, m_nSeq_Receive_Back, m_nReadMotor_Index);
			            //m_nSeq_Receive_Back = m_nSeq_Receive;

			            Read_Motor();	
		            }
		            Application.DoEvents();
	            }

                Sync_Seq();//m_nSeq_Motor_Back = m_nSeq_Motor; 
	            return true;
            }

            public bool Wait_Delay(int nMilliseconds)
            {
                m_CTmr.Set();
                if (Read_Motor_IsReceived() == true) Read_Motor();
                //else if (m_CTmr_Timeout.Get() > m_nTimeout)
#if true
	            else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
	            {
		            if (m_CTmr_Timeout.Get() > m_nTimeout)
		            {
			            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
			            m_CTmr_Timeout.Set();
			            m_nRetrieve++;
		            }
	            }
	            else if  (m_CTmr_Timeout.Get() > m_nTimeout)
            #else	
	            else if (m_CTmr_Timeout.Get() > m_nTimeout)
            #endif	
                {
                    Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                    //m_nSeq_Receive_Back = m_nSeq_Receive;
                    Read_Motor();
                }
                while (m_CTmr.Get() < nMilliseconds)
                {
#if false
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { m_nSeq_Motor_Back = m_nSeq_Motor; return false; }
                    if (Read_Motor_IsReceived() == true) Read_Motor();
                    //else if (m_CTmr_Timeout.Get() > m_nTimeout)
#if true
		            else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
		            {
			            if (m_CTmr_Timeout.Get() > m_nTimeout)
			            {
				            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
				            m_CTmr_Timeout.Set();
				            m_nRetrieve++;
			            }
		            }
		            else if  (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
		            else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif	
                    {
                        Ojw.CMessage.Write("[Receive] Time out Error({0} ms)- ID[{1}]", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID);
                        //m_nSeq_Receive_Back = m_nSeq_Receive;
                        Read_Motor();
                    }
#else
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) { Sync_Seq(); return false; }//m_nSeq_Motor_Back = m_nSeq_Motor; return false; }
		            if (Read_Motor_IsReceived() == true) Read_Motor();	
#if true
		            else if ((m_nRetrieve < _CNT_RETRIEVE) && (m_nReadCnt > 0))
		            {
			            if (m_CTmr_Timeout.Get() > m_nTimeout)
			            {
				            Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
				            m_CTmr_Timeout.Set();
				            m_nRetrieve++;
			            }
		            }
		            else if  (m_CTmr_Timeout.Get() > m_nTimeout)
#else	
		            else if (m_CTmr_Timeout.Get() > m_nTimeout)
#endif	
		            {
                        Ojw.CMessage.Write("[Receive-Wait_Delay(ms)] Time out Error({0} ms)- ID[{1}]Seq[{2}]Seq_Back[{3}]Index[{4}]\r\n", m_nTimeout, m_aSRead[m_nReadMotor_Index].nID, m_nSeq_Receive, m_nSeq_Receive_Back, m_nReadMotor_Index);
			            Read_Motor();	
		            }
#endif
                    Application.DoEvents();
                }
                //m_nSeq_Motor_Back = m_nSeq_Motor;
                Sync_Seq();
                return true;
            }
            private void Sync_Seq() { m_nSeq_Receive_Back= m_nSeq_Receive; }

            public void Read_Ram(int nAxis, int nAddress, int nLength)
            {
	            if (IsOpen() == false) return;

	            //m_bBusy = true; // Request Data

	            int nID = GetID_By_Axis(nAxis);
	            int nDefaultSize = _CHECKSUM2 + 1;

	            int i = 0;
	            // Header
	            byte [] abyteBuffer = new byte[256];
	            abyteBuffer[_HEADER1] = 0xff;
	            abyteBuffer[_HEADER2] = 0xff;
	            // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
	            abyteBuffer[_ID] = (byte)(nID & 0xff);
	            // Cmd
	            abyteBuffer[_CMD] = 0x04; // Ram 영역을 읽어온다.

	            /////////////////////////////////////////////////////
	            // Data
	            abyteBuffer[nDefaultSize + i++] = (byte)(nAddress & 0xff);// Register address

	            ////////
	            abyteBuffer[nDefaultSize + i++] = (byte)(nLength & 0xff);// Data Size
	            ////////
	            /////////////////////////////////////////////////////

	            //Packet Size
	            abyteBuffer[_SIZE] = (byte)((nDefaultSize + i) & 0xff);

	            MakeCheckSum(nDefaultSize + i, abyteBuffer);//, out abyteBuffer[_CHECKSUM1], out abyteBuffer[_CHECKSUM2]);

	            // 보내기 전에 Tick 을 Set 한다.
                Sync_Seq();
	            //Tick_Send(nAxis);
	            SendPacket(abyteBuffer, nDefaultSize + i);

	            m_CTmr_Timeout.Set();
                //Ojw.CMessage.Write("Read_Ram({0})", nID);
            }

            public void Read_Motor(int nAxis) { Read_Ram(nAxis, _ADDRESS_TORQUE_CONTROL + ((m_bMultiTurn == true) ? 4 : 0), 16); }//8); }{ Read_Ram(nAxis, _ADDRESS_TORQUE_CONTROL, 8); }
            public bool Read_Motor_IsReceived()
            {
                if (m_nSeq_Receive_Back != m_nSeq_Receive)
                {
#if false
                    Sync_Seq();
#endif
                    //m_nSeq_Receive_Back = m_nSeq_Receive;
                    //Ojw.CMessage.Write("Read_Motor_IsReceived() == true");
                    return true;
                }
                return false;
            }
            public void Read_Motor()
            {
                if (m_nReadCnt <= 0) return;
                m_nRetrieve = 0;

                m_nReadMotor_Index = (m_nReadMotor_Index + 1) % m_nReadCnt;

                Read_Motor(m_aSRead[m_nReadMotor_Index].nID);

                //if ((m_nReadCnt > 0) && (m_nReadMotor_Index >= 0))
                //{
                //    m_nReadMotor_Index = (m_nReadMotor_Index + 1) % m_nReadCnt;
                //    //Ojw.CMessage.Write("Read_Motor()");
                //    m_nSeq_Receive_Back = m_nSeq_Receive;
                //    Read_Motor(m_aSRead[m_nReadMotor_Index].nID);
                //}
            }
            //////////////////////////////////////
            // Setting
            //void Set_Ram(int nId, 
            //void Set_Rom(int nId, 
            private void Push_Id(int nAxis) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; if (IsCmd(nAxis) == false) m_anEn[m_nMotorCnt++] = nAxis; }
            private int Pop_Id() { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return -1; if (m_nMotorCnt > 0) return m_anEn[--m_nMotorCnt]; return -1; }

            // Push Motor ID for checking(if you set a Motor ID with this function, you can get a feedback data with delay function)
            public void Read_Motor_Push(int nAxis) { if (m_bProgEnd == true) return; if (Read_Motor_Index(nAxis) >= 0) return; if ((nAxis < 0) || (nAxis >= 254)) return; m_aSRead[m_nReadCnt].nID = nAxis; m_aSRead[m_nReadCnt].bEnable = true; m_aSRead[m_nReadCnt].nAddress_First = _ADDRESS_TORQUE_CONTROL + ((m_bMultiTurn == true) ? 4 : 0); m_aSRead[m_nReadCnt].nAddress_Length = 8; m_nReadCnt++; }
            // You can check your Motor ID for feedback, which you set or not.
            public int Read_Motor_Index(int nAxis) { if (m_bProgEnd == true) return -1; for (int i = 0; i < m_nReadCnt; i++) { if (m_aSRead[i].nID == nAxis) return i; } return -1; }

            // use this when you don't want to get some motor datas.
            public void Read_Motor_Clear() { m_nReadCnt = 0; Array.Clear(m_aSRead, 0, _SIZE_MOTOR_MAX); }

            // detail option
            public void Read_Motor_Change_Address(int nAxis, int nAddress, int nLength) { int nIndex = Read_Motor_Index(nAxis); if (nIndex >= 0) { m_aSRead[nIndex].nAddress_First = nAddress; m_aSRead[nIndex].nAddress_Length = nLength; } }
            public void Read_Motor_ShowMessage(bool bTrue) { m_bShowMessage = bTrue; }
            public bool IsCmd(int nAxis)
            {
                for (int i = 0; i < m_nMotorCnt; i++) if (m_anEn[i] == nAxis) return true;
                return false;
            }

            private void SendPacket(byte [] buffer, int nLength)
            {
                //	unsigned char *szPacket = (unsigned char *)malloc(sizeof(unsigned char) *( nLength + 2));
                if (IsOpen() == true)
                {
                    m_CSerial.SendPacket(buffer, nLength);
#if false
                    Ojw.CMessage.Write2("[SendPacket()]\r\n");
                    for (int nMsg = 0; nMsg < nLength; nMsg++)
                    {
                        Ojw.CMessage.Write2("0x%02X, ", buffer[nMsg]);
                    }
                    Ojw.CMessage.Write2("\r\n");
#endif
                }
                if (IsOpen_Socket() == true)
                {
                    m_CSocket.Send(buffer);                    
                }
            }
            private void Make_And_Send_Packet(int nID, int nCmd, int nDataByteSize, byte [] pbytePacket)
            {
                int nDefaultSize = _CHECKSUM2 + 1;
                int nSize = nDefaultSize + nDataByteSize;

                byte[] pbyteBuffer = new byte[nSize];//(byte *)malloc(sizeof(byte) * nSize);

                // Header
                pbyteBuffer[_HEADER1] = (byte)0xff;
                pbyteBuffer[_HEADER2] = (byte)0xff;
                // ID = 0xFE : 전체명령, 0xFD - 공장출하시 설정 아이디
                pbyteBuffer[_ID] = (byte)(nID & 0xff);
                // Cmd
                pbyteBuffer[_CMD] = (byte)(nCmd & 0xff);
                /////////////////////////////////////////////////////
                int i = 0;
                for (int j = 0; j < nDataByteSize; j++) pbyteBuffer[nDefaultSize + i++] = pbytePacket[j];
                /////////////////////////////////////////////////////

                //Packet Size
                pbyteBuffer[_SIZE] = (byte)(nSize & 0xff);

                MakeCheckSum(nSize, pbyteBuffer);
#if false
Ojw.CMessage.Write2("[Make_And_Send_Packet()]\r\n");
for (int nMsg = 0; nMsg < nDefaultSize + i; nMsg++)
{
Ojw.CMessage.Write2("0x%02x, ", pbyteBuffer[nMsg]);
}
Ojw.CMessage.Write2("\r\n");
#endif
                SendPacket(pbyteBuffer, nSize);

                //free(pbyteBuffer);
            }

            private void MakeCheckSum(int nAllPacketLength, byte [] buffer)
            {
                int nHeadSize = _CHECKSUM2 + 1;
                buffer[_CHECKSUM1] = (byte)(buffer[_SIZE] ^ buffer[_ID] ^ buffer[_CMD]);
                for (int j = 0; j < nAllPacketLength - nHeadSize; j++)
                {
                    buffer[_CHECKSUM1] ^= buffer[nHeadSize + j];
                }
                buffer[_CHECKSUM1] = (byte)(buffer[_CHECKSUM1] & 0xfe);
                buffer[_CHECKSUM2] = (byte)(~buffer[_CHECKSUM1] & 0xfe);
            }

            private int GetID_By_Axis(int nAxis) { return (nAxis == 0xfe) ? 0xfe : m_aSMot[nAxis].nID; }

            #region MotionFile
            // Set3D 하면 모델링에서 가져온 상태로 강제 재셋팅, -> PlayFile(파일이름) 하면 된다.
            Ojw.C3d m_C3d = null;
            Ojw.C3d.COjwDesignerHeader m_CHeader = null;//new C3d.COjwDesignerHeader();
            public void SetHeader(Ojw.C3d.COjwDesignerHeader CHeader)
            {
                m_CHeader = CHeader;
                for (int i = 0; i < m_CHeader.nMotorCnt; i++)
                {
                    SetParam(
                        i,
                        m_CHeader.pSMotorInfo[i].nMotorID,
                        m_CHeader.pSMotorInfo[i].nMotorDir,
                        m_CHeader.pSMotorInfo[i].fLimit_Up,
                        m_CHeader.pSMotorInfo[i].fLimit_Down,
                        (float)m_CHeader.pSMotorInfo[i].nCenter_Evd,
                        0,
                        (float)m_CHeader.pSMotorInfo[i].nMechMove,
                        m_CHeader.pSMotorInfo[i].fMechAngle);
                }
            }
            public void Set3D(Ojw.C3d C3dModel) { m_C3d = C3dModel; SetHeader(m_C3d.GetHeader()); }
            public void PlayFrame(int nLine, SMotion_t SMotion)
            {
                if (SMotion.nFrameSize <= 0) return;
                if ((nLine < 0) || (nLine >= SMotion.nFrameSize)) return;

                if ((m_bStop == false) && (m_bEms == false) && (m_bMotionEnd == false))
                {
                    //m_bStop = false; 
                    for (int i = 0; i < _SIZE_MOTOR_MAX; i++) Set_Flag_Stop(i, false);
                    SetTorque(true, true);

                    for (int nAxis = 0; nAxis < m_nMotorCnt; nAxis++)
                    {
                        if (m_CHeader.pSMotorInfo[nAxis].nMotorControlType != 0) // 위치제어가 아니라면
                        {
                            SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            Set_Flag_Led(nAxis,
                                Get_Flag_Led_Green(SMotion.STable[nLine].anLed[nAxis]),
                                Get_Flag_Led_Blue(SMotion.STable[nLine].anLed[nAxis]),
                                Get_Flag_Led_Red(SMotion.STable[nLine].anLed[nAxis])
                                );

                            Set_Turn(nAxis, SMotion.STable[nLine].anMot[nAxis]);
                        }
                        else
                        {
                            SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            Set_Flag_Led(nAxis,
                                Get_Flag_Led_Green(SMotion.STable[nLine].anLed[nAxis]),
                                Get_Flag_Led_Blue(SMotion.STable[nLine].anLed[nAxis]),
                                Get_Flag_Led_Red(SMotion.STable[nLine].anLed[nAxis])
                                );

                            Set_Angle(nAxis, SMotion.STable[nLine].anMot[nAxis]);
                        }
                    }
                    Send_Motor(SMotion.STable[nLine].nTime);
                }
            }
            public void PlayFrame(SMotionTable_t STable)
            {
                if ((m_bStop == false) && (m_bEms == false) && (m_bMotionEnd == false))
                {
                    //m_bStop = false; 
                    for (int i = 0; i < _SIZE_MOTOR_MAX; i++) Set_Flag_Stop(i, false);
                    SetTorque(true, true);
                    for (int nAxis = 0; nAxis < m_CHeader.nMotorCnt; nAxis++)
                    {
                        if (m_CHeader.pSMotorInfo[nAxis].nMotorControlType != 0) // 위치제어가 아니라면
                        {
                            SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            Set_Flag_Led(nAxis,
                                Get_Flag_Led_Green(STable.anLed[nAxis]),
                                Get_Flag_Led_Blue(STable.anLed[nAxis]),
                                Get_Flag_Led_Red(STable.anLed[nAxis])
                                );

                            Set_Turn(nAxis, STable.anMot[nAxis]);
                        }
                        else
                        {
                            SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            Set_Flag_Led(nAxis,
                                Get_Flag_Led_Green(STable.anLed[nAxis]),
                                Get_Flag_Led_Blue(STable.anLed[nAxis]),
                                Get_Flag_Led_Red(STable.anLed[nAxis])
                                );

                            Set_Angle(nAxis, STable.anMot[nAxis]);
                        }
                    }
                    Send_Motor(STable.nTime);
                }
            }
            public void PlayFile(string strFileName)
            {
                try
                {
                    if (m_C3d == null) return;
                    SMotion_t SMotion = new SMotion_t();
                    if (m_C3d.BinaryFileOpen(strFileName, out SMotion) == true)
                    {
                        if (SMotion.nFrameSize > 0)
                        {
                            m_bStart = true;

                            m_C3d.WaitAction_SetTimer();

                            foreach (SMotionTable_t STable in SMotion.STable)
                            {
                                if (STable.bEn == true)
                                {
                                    PlayFrame(STable);

                                    int nDelay = STable.nTime + STable.nDelay;
                                    if (nDelay > 0) m_C3d.WaitAction_ByTimer(nDelay);
                                }
                            }
                            m_bStart = false;
                            m_bMotionEnd = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error("Error -> PlayMotion(), " + ex.ToString());
                }
            }
            #endregion MotionFile
        }
    }
}
