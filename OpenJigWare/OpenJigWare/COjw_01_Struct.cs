using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenJigWare
{
    //using UserEventHandler = EventHandler;
    partial class Ojw
    {
        #region Structure
        public struct SEncryption_t
        {
            public byte[] byteEncryption;
        }
        #region Int(SPoint2D_t, SPoint_3D_t)
        public struct SPoint2D_t
        {
            public int Data;
            public int x;
            public int y;
        }
        public struct SPoint3D_t
        {
            public int x;
            public int y;
            public int z;
            public SPoint3D_t(int nX, int nY, int nZ) { this.x = nX; this.y = nY; this.z = nZ; }
        }
        #endregion Int(SPoint2D_t, SPoint_3D_t)
        #region Float(SVector_t)
        public struct SVector_t
        {
            public double x;
            public double y;
            public SVector_t(double x, double y) { this.x = x; this.y = y; }
                        
            public static SVector_t operator +(SVector_t v1, SVector_t v2) { return new SVector_t(v1.x + v2.x, v1.y + v2.y); }
            public static SVector_t operator -(SVector_t v1, SVector_t v2) { return new SVector_t(v1.x - v2.x, v1.y - v2.y); }
            public static SVector_t operator *(SVector_t v1, SVector_t v2) { return new SVector_t(v1.x * v2.x, v1.y * v2.y); }
            public static SVector_t operator /(SVector_t v1, SVector_t v2) { return new SVector_t(v1.x / v2.x, v1.y / v2.y); }
        }
        public struct SVector3D_t
        {
            public float x;
            public float y;
            public float z;
            public SVector3D_t(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

            //public static SVector3D_t operator =(SVector3D_t v1) { return new float[](v1.x + v2.x, v1.y + v2.y, v1.z + v2.z); }
            public static SVector3D_t operator +(SVector3D_t v1, SVector3D_t v2) { return new SVector3D_t(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z); }
            public static SVector3D_t operator -(SVector3D_t v1, SVector3D_t v2) { return new SVector3D_t(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z); }
            public static SVector3D_t operator *(SVector3D_t v1, SVector3D_t v2) { return new SVector3D_t(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z); }
            public static SVector3D_t operator /(SVector3D_t v1, SVector3D_t v2) { return new SVector3D_t(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z); }
        }
        public struct STrackD_t
        {
            public float fX;
            public float fY;
            public float fR;
            //float fX, fY, fR;
            public int nConnectedAxis; // 연결된 모터 번호
            public float fCenter_X;      // 회전 해야 할 좌표 기준 점(X)
            public float fCenter_Y;      // 회전 해야 할 좌표 기준 점(Y)
            public int nMode;          // Mode(0 : 변화없음, 1 : 회전, 2 : 축 이동(fAxis_X 각도 연관 - 나중에 구현하자. 지금 바빠)) //float fOffsetPan, float fOffsetTilt, float fOffsetSwing,    // Ratation(Offset)
            public int nDir;           // 방향(0 : 정, 1 : 반대)
            public STrackD_t(float x, float y, float z, int axis, float fCx, float fCy, int mode, int dir) 
            { 
                this.fX = x; 
                this.fY = y;
                this.fR = z;
                this.nConnectedAxis = axis;
                this.fCenter_X = fCx;
                this.fCenter_Y = fCy;
                this.nMode = mode;
                this.nDir = dir;
            }
        }
        #endregion Float(SVector_t)
        #region Double(SVertex_t)
        public struct SVertex_t
        {
            public double x;
            public double y;
            public SVertex_t(double x, double y) { this.x = x; this.y = y; }

            public static SVertex_t operator +(SVertex_t v1, SVertex_t v2) { return new SVertex_t(v1.x + v2.x, v1.y + v2.y); }
            public static SVertex_t operator -(SVertex_t v1, SVertex_t v2) { return new SVertex_t(v1.x - v2.x, v1.y - v2.y); }
            public static SVertex_t operator *(SVertex_t v1, SVertex_t v2) { return new SVertex_t(v1.x * v2.x, v1.y * v2.y); }
            public static SVertex_t operator /(SVertex_t v1, SVertex_t v2) { return new SVertex_t(v1.x / v2.x, v1.y / v2.y); }
        }
        public struct SVertex3D_t
        {
            public double x;
            public double y;
            public double z;
            public SVertex3D_t(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }

            public static SVertex3D_t operator +(SVertex3D_t v1, SVertex3D_t v2) { return new SVertex3D_t(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z); }
            public static SVertex3D_t operator -(SVertex3D_t v1, SVertex3D_t v2) { return new SVertex3D_t(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z); }
            public static SVertex3D_t operator *(SVertex3D_t v1, SVertex3D_t v2) { return new SVertex3D_t(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z); }
            public static SVertex3D_t operator /(SVertex3D_t v1, SVertex3D_t v2) { return new SVertex3D_t(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z); }
        }
        #endregion Double(SVertex_t)
        #region Angle(SAngle3D_t -> For Pan, Tilt, Swing)
        public struct SAngle3D_t
        {
            public float pan;
            public float tilt;
            public float swing;
            public SAngle3D_t(float pan, float tilt, float swing) { this.pan = pan; this.tilt = tilt; this.swing = swing; }
        }        
        #endregion Angle(SAngle3D_t -> For Pan, Tilt, Swing)
        #region Motion
        public struct SMotionTable_t
        {
            public bool bEn;
            public int[] anMot;
            public float[] afXyz; // reserve
            public int[] anLed;
            public bool[] abEn;
            public bool[] abType;
            public int nTime;
            public int nDelay;
            public int nGroup;
            public int nCmd;
            public int nData0;
            public int nData1;
            public int nData2;
            public int nData3;
            public int nData4;
            public int nData5;

            //public int nExtLed;
            //public int nExtBuzz;
        }
        public struct SMotion_t
        {
            //public int nCount;

            public int nFrameSize;
            public int nCnt_LineComment;
            public int nPlayTime;
            public int nCommentSize;
            public int nRobotModelNum;
            public int nMotorCnt;
            public int nStartPosition;
            public string strFileName;
            public string strTableName;
            //public string strComment;
            public SMotionTable_t[] STable;
        }
        #endregion Motion
        #region Define structure for motor(SParam_t. SParam_Axis_t)
#if false  
        public struct SParam_t
        {
            public int nBaudRate;
            public int nComPort;
            public bool bCompleteStop;
            public int nMotorMax;
        }
        public struct SParam_Axis_t
        {
            public int nID;
            public int nDir;

            public double dLimitUp;    // Axis limit(Max) - 0 : No use
            public double dLimitDn;    // Axis limit(Min) - 0 : No use
            // Center Position
            public double dCenterPos;

            public double dOffsetAngle_Display; // Angle Offset for display

            // Gear Ratio
            public double dMechMove;
            public double dDegree;
        }
#endif
        // for DH-Notion (Matrix)
        public struct SDhT_t
        {
            public double[,] adT;
        }
        public struct SDhT_Str_t
        {
            public String[,] aStrT;
        }
        // for compiling of math
        public struct SOjwCode_t
        {
            public bool bInit;
            public int nCnt_Operation;
            public long[] alOperation_Cmd;
            public double[] adOperation_Memory;
            public double[] adOperation_Data;

            // Informations
            public int nMotor_Max;
            public int[] pnMotor_Number;
            public int nVar_Max;
            public int[] pnVar_Number;
        }        
        #endregion Define structure for motor(SParam_t. SParam_Axis_t)
        #endregion Structure

        #region UserEvent
        public class CUserEventArgs : EventArgs
        {
            public string strMessage { get; set; }
            //public List<int> arg { get; set; }
            //public int nEvent { get; set; }
        }
        public class CUserEvent
        {
            //typeof EventHandler UserEvent;
            public event EventHandler UserEvent;
            //public CUserEvent()
            //{
            //    RunEvent();
            //}
            public void RunEvent()
            {
                try
                {
                    if (UserEvent != null)
                    {
                        CUserEventArgs CEventArg = new CUserEventArgs();
                        CEventArg.strMessage = "UserEvent";
                        //CEventArg.arg = new List<int>();
                        UserEvent(null, CEventArg);
                    }
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.ToString());
                }
            }
        }
        #endregion UserEvent
    }
}
