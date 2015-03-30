#define _DEF_ORIENTATION
#define _DEF_EMG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
//using MyoSharp.Poses;

namespace OpenJigWare
{
    partial class Ojw
    {
#if false
#if _DEF_EMG
        private Ojw.CGraph m_CGrap = null;
#endif
        public class CMyo
        {
            private static CTimer m_CTId_GetData = new CTimer();
            private static CTimer m_CTId_GetOrient = new CTimer();
            private static CTimer m_CTId_GetEmg = new CTimer();
            private static IChannel m_myoChannel;
            private static IHub m_myoHub;
            public CMyo()
            {                
            }
            ~CMyo()
            {
            }

            public static void Init()
            {
                InitJesture();

                // if you make your class, just write in here
                m_myoChannel = Channel.Create(ChannelDriver.Create(ChannelBridge.Create(), MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));
                m_myoHub = Hub.Create(m_myoChannel);

                // Add Event    
                m_myoHub.MyoConnected += new EventHandler<MyoEventArgs>(myoHub_MyoConnected);
                m_myoHub.MyoDisconnected += new EventHandler<MyoEventArgs>(myoHub_MyoDisconnected);
            }

            private static void myoHub_MyoConnected(object sender, MyoEventArgs e)
            {
                m_CTId_GetData.Set();
                m_CTId_GetOrient.Set();
                m_CTId_GetEmg.Set();
                Ojw.CMessage.Write(String.Format("Myo {0} has connected!", e.Myo.Handle)); 
                e.Myo.Vibrate(VibrationType.Short);

                e.Myo.Unlock(UnlockType.Hold);

                //Ojw.CMessage.Write("Connected");
#if _DEF_ORIENTATION
                e.Myo.OrientationDataAcquired += Myo_OrientationDataAcquired;
#endif
#if _DEF_EMG
                e.Myo.EmgDataAcquired += Myo_EmgDataAcquired;
                e.Myo.SetEmgStreaming(true);
                //tmrPulse.Enabled = true;
#endif
            }
            private static void myoHub_MyoDisconnected(object sender, MyoEventArgs e)
            {
#if _DEF_ORIENTATION
                e.Myo.OrientationDataAcquired -= Myo_OrientationDataAcquired;
#endif
#if _DEF_EMG
                e.Myo.SetEmgStreaming(false);
                e.Myo.EmgDataAcquired -= Myo_EmgDataAcquired;
                //tmrPulse.Enabled = false;
#endif
                Ojw.CMessage.Write("Disconnected");
                e.Myo.Vibrate(VibrationType.Short);
                CTimer.Wait(500);
                e.Myo.Vibrate(VibrationType.Short);
            }
            
#if _DEF_ORIENTATION
            private static float[] m_afRot = new float[3];
            private static void Myo_OrientationDataAcquired(object sender, OrientationDataEventArgs e)
            {
                const float PI = (float)System.Math.PI;

                int nDev = 1; //10;
                // convert the values to a 0-9 scale (for easier digestion/understanding)
                var roll = (int)((e.Roll + PI) / (PI * 2.0f) * nDev);
                var pitch = (int)((e.Pitch + PI) / (PI * 2.0f) * nDev);
                var yaw = (int)((e.Yaw + PI) / (PI * 2.0f) * nDev);

                //if (m_CTId_GetOrient.Get() >= 1000)
                //{
                //    m_CTId_GetOrient.Set();
                //    float fX = (float)Math.Round(Ojw.CMath.R2D(e.Orientation.X), 3);
                //    float fY = (float)Math.Round(Ojw.CMath.R2D(e.Orientation.Y), 3);
                //    float fW = (float)Math.Round(Ojw.CMath.R2D(e.Orientation.W), 3);
                //    float fSwing = (float)Math.Round(Ojw.CMath.R2D(e.Roll), 3);
                //    float fTilt = (float)Math.Round(Ojw.CMath.R2D(e.Pitch), 3);
                //    float fPan = (float)Math.Round(Ojw.CMath.R2D(e.Yaw), 3);
                //    Ojw.CMessage.Write("Pan[" + Ojw.CConvert.FillString(Ojw.CConvert.FloatToStr(fPan), " ", 7, false) +
                //        "], Tilt[" + Ojw.CConvert.FillString(Ojw.CConvert.FloatToStr(fTilt), " ", 7, false) +
                //        "], Swing[" + Ojw.CConvert.FillString(Ojw.CConvert.FloatToStr(fSwing), " ", 7, false) + "]" +
                //        "Oriental(X[" + Ojw.CConvert.FillString(Ojw.CConvert.FloatToStr(fX), " ", 7, false) +
                //        "], Y[" + Ojw.CConvert.FillString(Ojw.CConvert.FloatToStr(fY), " ", 7, false) +
                //        "], W[" + Ojw.CConvert.FillString(Ojw.CConvert.FloatToStr(fW), " ", 7, false) + "])");
                //    //Ojw.CMessage.Write("Oriental(Swing[" + Ojw.CConvert.IntToStr(roll) + "], Tilt[" + Ojw.CConvert.IntToStr(pitch) + "], Pan[" + Ojw.CConvert.IntToStr(yaw) + "], X[" + Ojw.CConvert.FillString(Ojw.CConvert.FloatToStr(fX), " ", 7, false) + "], Y[" + Ojw.CConvert.FillString(Ojw.CConvert.FloatToStr(fY), " ", 7, false) + "], W[" + Ojw.CConvert.FillString(Ojw.CConvert.FloatToStr(fW), " ", 7, false) + "]");
                //    //Ojw.CMessage.Write(String.Format("Oriental(Swing, Tilt, Pan): {0}\t{1}\t{2}\t*****]t{3}\t{4}\t{5}", roll, pitch, yaw, e.Orientation.X, e.Orientation.Y, e.Orientation.W));
                //}
            }
#endif

#if _DEF_EMG
            private static int[] m_anIndex = new int[8];
            //private bool bThumb = false;
            //private bool 
            public enum EMyoPos_t
            {
                Spread,
                Fist,
                V,
                FirstFinger,
                MiddleFinger,
                LittleFinger,
                _Count
            }
            
            public static int GetJesture()
            {
                return m_nJesture;
            }
            private static int[,] m_anJesture = new int[(int)EMyoPos_t._Count, 8];//{{},
            private static int CheckJesture(params int[] anData)
            {
                int nRet = -1;
                List<int> lstTmp = new List<int>();
                int nCnt = 0;
                int nCnt_Prev = 0;
                for (int i = 0; i < anData.Length; i++)
                {
                    if (anData[i] == 0)
                    {
                        nCnt++;
                    }
                    else
                    {
                        nCnt++;
                        if (nCnt_Prev == 0)
                            nCnt_Prev = nCnt;
                        else
                            lstTmp.Add(nCnt);
                        nCnt = 0;
                    }
                    //if ((anData[i] > 0) && (anData[i - 1] > 0))
                    //{
                    //    nCnt++;
                    //}
                    //else
                    //{
                    //}
                    
                }
                if (nCnt_Prev != 0) lstTmp.Add(nCnt_Prev);

                if (lstTmp.Count > 0)
                {
                    //nRet = -1;
                    for (int nIndex = 0; nIndex < (int)EMyoPos_t._Count; nIndex++)
                    {
                        int k = 0;
                        bool bPass = true;
                        for (int i = 0; i < m_anJesture[nIndex, 0]; i++)
                        {
                            for (int j = 0; j < m_anJesture[nIndex, 0]; j++)
                            {
                                if (m_anJesture[nIndex, i + 1] != lstTmp[(j + k) % m_anJesture[nIndex, 0]]) { bPass = false; break; }
                            }
                            k++;
                        }
                        if (bPass == true)
                        {
                            nRet = nIndex;
                            break;
                        }
                    }
                }

                return nRet;
            }
            private static void InitJesture()
            {
                int i = 0;
                // Spread
                m_anJesture[(int)EMyoPos_t.Spread, i++] = 3;
                m_anJesture[(int)EMyoPos_t.Spread, i++] = 1;
                m_anJesture[(int)EMyoPos_t.Spread, i++] = 2;
                m_anJesture[(int)EMyoPos_t.Spread, i++] = 5;
                m_anJesture[(int)EMyoPos_t.Spread, i++] = 0;
                m_anJesture[(int)EMyoPos_t.Spread, i++] = 0;
                m_anJesture[(int)EMyoPos_t.Spread, i++] = 0;
                m_anJesture[(int)EMyoPos_t.Spread, i++] = 0;
                i = 0;
                // Fist
                m_anJesture[(int)EMyoPos_t.Fist, i++] = 5;
                m_anJesture[(int)EMyoPos_t.Fist, i++] = 1;
                m_anJesture[(int)EMyoPos_t.Fist, i++] = 2;
                m_anJesture[(int)EMyoPos_t.Fist, i++] = 1;
                m_anJesture[(int)EMyoPos_t.Fist, i++] = 1;
                m_anJesture[(int)EMyoPos_t.Fist, i++] = 3;
                m_anJesture[(int)EMyoPos_t.Fist, i++] = 0;
                m_anJesture[(int)EMyoPos_t.Fist, i++] = 0;
                i = 0;
                // V
                m_anJesture[(int)EMyoPos_t.V, i++] = 3;
                m_anJesture[(int)EMyoPos_t.V, i++] = 4;
                m_anJesture[(int)EMyoPos_t.V, i++] = 3;
                m_anJesture[(int)EMyoPos_t.V, i++] = 1;
                m_anJesture[(int)EMyoPos_t.V, i++] = 0;
                m_anJesture[(int)EMyoPos_t.V, i++] = 0;
                m_anJesture[(int)EMyoPos_t.V, i++] = 0;
                m_anJesture[(int)EMyoPos_t.V, i++] = 0;
                i = 0;
                // MiddleFinger
                m_anJesture[(int)EMyoPos_t.MiddleFinger, i++] = 4;
                m_anJesture[(int)EMyoPos_t.MiddleFinger, i++] = 3;
                m_anJesture[(int)EMyoPos_t.MiddleFinger, i++] = 1;
                m_anJesture[(int)EMyoPos_t.MiddleFinger, i++] = 3;
                m_anJesture[(int)EMyoPos_t.MiddleFinger, i++] = 1;
                m_anJesture[(int)EMyoPos_t.MiddleFinger, i++] = 0;
                m_anJesture[(int)EMyoPos_t.MiddleFinger, i++] = 0;
                m_anJesture[(int)EMyoPos_t.MiddleFinger, i++] = 0;
                i = 0;
                // FirstFinger
                m_anJesture[(int)EMyoPos_t.FirstFinger, i++] = 4;
                m_anJesture[(int)EMyoPos_t.FirstFinger, i++] = 3;
                m_anJesture[(int)EMyoPos_t.FirstFinger, i++] = 2;
                m_anJesture[(int)EMyoPos_t.FirstFinger, i++] = 2;
                m_anJesture[(int)EMyoPos_t.FirstFinger, i++] = 1;
                m_anJesture[(int)EMyoPos_t.FirstFinger, i++] = 0;
                m_anJesture[(int)EMyoPos_t.FirstFinger, i++] = 0;
                m_anJesture[(int)EMyoPos_t.FirstFinger, i++] = 0;
                i = 0;
                // LittleFinger
                m_anJesture[(int)EMyoPos_t.LittleFinger, i++] = 2;
                m_anJesture[(int)EMyoPos_t.LittleFinger, i++] = 3;
                m_anJesture[(int)EMyoPos_t.LittleFinger, i++] = 5;
                m_anJesture[(int)EMyoPos_t.LittleFinger, i++] = 0;
                m_anJesture[(int)EMyoPos_t.LittleFinger, i++] = 0;
                m_anJesture[(int)EMyoPos_t.LittleFinger, i++] = 0;
                m_anJesture[(int)EMyoPos_t.LittleFinger, i++] = 0;
                m_anJesture[(int)EMyoPos_t.LittleFinger, i++] = 0;
            }
            private static void InitData()
            {
                Array.Clear(m_anData, 0, m_anData.Length);
                Array.Clear(m_anData_Back, 0, m_anData.Length);
                Array.Clear(m_anData_Set, 0, m_anData.Length);
                Array.Clear(m_anValue_Back, 0, m_anData.GetLength(0) * m_anData.GetLength(1));
            }
            private static bool m_bFilter = true;
            private static bool m_bStep1 = true;
            private static bool m_bStep2 = true;
            private static int m_nThread = 30;
            public static void SetFilterThread(int value) { m_nThread = value; }
            public static void SetFilterValue(float value) { m_fAlpha = value; }
            public static float GetFilterValue() { return m_fAlpha; }
            private static float m_fAlpha = 0.1f;
            private static int[] m_anData = new int[8];
            private static int[] m_anData_Back = new int[8];
            private const int _DATA_HISTORY = 10;
            private static bool[] m_anData_Set = new bool[8];
            private static int[,] m_anValue_Back = new int[8, _DATA_HISTORY];
            private static int m_nJesture = -1;
            private static void Myo_EmgDataAcquired(object sender, EmgDataEventArgs e)
            {
                //if (m_CTId2.Get() >= 1000)
                //{
                //    m_CTId2.Set();
                //    // TODO: write your code to interpret EMG data!
                //    Ojw.CMessage.Write(String.Format("Emg = {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}",
                //        e.EmgData.GetDataForSensor(0),
                //        e.EmgData.GetDataForSensor(1),
                //        e.EmgData.GetDataForSensor(2),
                //        e.EmgData.GetDataForSensor(3),
                //        e.EmgData.GetDataForSensor(4),
                //        e.EmgData.GetDataForSensor(5),
                //        e.EmgData.GetDataForSensor(6),
                //        e.EmgData.GetDataForSensor(7)));
                //}
                if (m_CTId_GetEmg.Get() >= 100)
                {
                    m_CTId_GetEmg.Set();

                    float fAlpha = m_fAlpha;
                    for (int i = 0; i < 8; i++)
                    {
                        //m_anData[i] = e.EmgData.GetDataForSensor(i);
                        if (m_bFilter == true)
                        {
                            if (m_bStep1 == true)
                                m_anData[i] = (int)Ojw.CMath.LowPassFilter(fAlpha, (float)m_anData[i], (int)Math.Abs(e.EmgData.GetDataForSensor(i)) + (int)Math.Abs(e.EmgData.GetDataForSensor(i) - m_anData_Back[i]));
                            else
                                m_anData[i] = (int)Ojw.CMath.LowPassFilter(fAlpha, (float)m_anData[i], (float)e.EmgData.GetDataForSensor(i));
                        }
                        else
                        {
                            if (m_bStep1 == true)
                                m_anData[i] = (int)Math.Abs(e.EmgData.GetDataForSensor(i)) + (int)Math.Abs(e.EmgData.GetDataForSensor(i) - m_anData_Back[i]);
                            else
                                m_anData[i] = e.EmgData.GetDataForSensor(i);
                        }

                        m_anData_Back[i] = e.EmgData.GetDataForSensor(i);
                    }
                    if (m_bStep2 == true)
                    {
                        int nValue = m_nThread;// 30;// 50;
                        //int nRange = 25;
                        //m_CGrap.Push(
                        //    (int)(m_anData[0] / nValue) * nValue,
                        //    (int)(m_anData[1] / nValue) * nValue,
                        //    (int)(m_anData[2] / nValue) * nValue,
                        //    (int)(m_anData[3] / nValue) * nValue,
                        //    (int)(m_anData[4] / nValue) * nValue,
                        //    (int)(m_anData[5] / nValue) * nValue,
                        //    (int)(m_anData[6] / nValue) * nValue,
                        //    (int)(m_anData[7] / nValue) * nValue
                        //);
                        int[] anValue = new int[m_anData.Length];
                        //Array.Copy(m_anValue_Back, 1, m_anValue_Back.Le

                        for (int i = 0; i < m_anData.Length; i++)
                        {
                            int nCnt = 0;
                            for (int j = 0; j < _DATA_HISTORY - 1; j++)
                            {
                                m_anValue_Back[i, _DATA_HISTORY - 1 - j] = m_anValue_Back[i, _DATA_HISTORY - 1 - (j + 1)];
                            }
                            anValue[i] = (int)(m_anData[i] / nValue) * nValue;
                            if (anValue[i] >= nValue) anValue[i] = nValue;
                            else anValue[i] = 0;
                            m_anValue_Back[i, 0] = anValue[i];
                            for (int j = 0; j < _DATA_HISTORY; j++)
                            {
                                if (m_anValue_Back[i, j] != 0) nCnt++;
                            }


                            if (m_anData_Set[i] == false)
                            {
                                if (nCnt >= 7) m_anData_Set[i] = true;
                            }
                            else
                            {
                                if (nCnt <= 2) m_anData_Set[i] = false;
                            }
                        }

                        int nPos = CheckJesture(
                            ((m_anData_Set[0] == true) ? nValue : 0),
                            ((m_anData_Set[1] == true) ? nValue : 0),
                            ((m_anData_Set[2] == true) ? nValue : 0),
                            ((m_anData_Set[3] == true) ? nValue : 0),
                            ((m_anData_Set[4] == true) ? nValue : 0),
                            ((m_anData_Set[5] == true) ? nValue : 0),
                            ((m_anData_Set[6] == true) ? nValue : 0),
                            ((m_anData_Set[7] == true) ? nValue : 0)
                        );
                        m_nJesture = nPos;
                        if (nPos >= 0)
                        {
                            if (nPos == (int)EMyoPos_t.FirstFinger)
                            {
                                Ojw.CMessage.Write("First Finger");
                            }
                            else if (nPos == (int)EMyoPos_t.MiddleFinger)
                            {
                                Ojw.CMessage.Write("Middle Finger");
                            }
                            else if (nPos == (int)EMyoPos_t.LittleFinger)
                            {
                                Ojw.CMessage.Write("Little Finger");
                            }
                            else if (nPos == (int)EMyoPos_t.Fist)
                            {
                                Ojw.CMessage.Write("Fist");
                            }
                            else if (nPos == (int)EMyoPos_t.Spread)
                            {
                                Ojw.CMessage.Write("Spread");
                            }
                            else if (nPos == (int)EMyoPos_t.V)
                            {
                                Ojw.CMessage.Write("Jesture V");
                            }
                        }
                        //m_CGrap.Push(
                        //    ((m_anData_Set[0] == true) ? nValue : 0),
                        //    ((m_anData_Set[1] == true) ? nValue : 0),
                        //    ((m_anData_Set[2] == true) ? nValue : 0),
                        //    ((m_anData_Set[3] == true) ? nValue : 0),
                        //    ((m_anData_Set[4] == true) ? nValue : 0),
                        //    ((m_anData_Set[5] == true) ? nValue : 0),
                        //    ((m_anData_Set[6] == true) ? nValue : 0),
                        //    ((m_anData_Set[7] == true) ? nValue : 0)
                        //);
                    }
                    else
                    {
                        //m_CGrap.Push(
                        //    m_anData[0],
                        //    m_anData[1],
                        //    m_anData[2],
                        //    m_anData[3],
                        //    m_anData[4],
                        //    m_anData[5],
                        //    m_anData[6],
                        //    m_anData[7]
                        //);
                    }
                    //m_CGrap.OjwDraw();
                }
            }
#endif
        }
#endif
    }
}
