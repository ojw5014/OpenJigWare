/// Leap
#define _LEFT_HAND
#define _RIGHT_HAND
#define _TEST
#define _NORMAL_DIRECTION
#define _NO_MOUSE_CONTROL

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;

using Leap;
//using System.Windows.Forms;
//using MyoSharp.Poses;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CLeap
        {
            private bool m_bLeapDll = false;
            public CLeap()
            {
                if (CFile.IsFiles("LeapC.dll", "Leap.dll") == false)
                {
                    Ojw.CMessage.Write("[Warning] There's no any Leap Dll Files");
                }
                else m_bLeapDll = true;
                //if (m_bLeapDll == true)
                //    MessageBox.Show("Ok");
                //else
                //    MessageBox.Show("No Files");


                m_nOffset_Mouse_X = 0;
                m_nOffset_Mouse_Y = Ojw.CSystem.GetMonitor_Height(0);
            }

            public void InitLeap()
            {
                try
                {
                    using (Leap.IController controller = new Leap.Controller())
                    {
                        controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_ALLOW_PAUSE_RESUME);

                        // Set up our listener:
                        controller.Connect += FlmOnServiceConnect;
                        controller.Disconnect += FlmOnServiceDisconnect;
                        controller.FrameReady += FlmOnFrame;
                        controller.Device += FlmOnConnect;
                        controller.DeviceLost += FlmOnDisconnect;
                        controller.DeviceFailure += FlmOnDeviceFailure;
                        controller.LogMessage += FlmOnLogMessage;

                        // Keep this process running until Enter is pressed
                        //Ojw.CMessage.Write("Press any key to quit...");
                        //Console.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.ToString());
                }
            }
            public void DInitLeap()
            {
                try
                {
                    using (Leap.IController controller = new Leap.Controller())
                    {
                        // Set up our listener:
                        controller.Connect -= FlmOnServiceConnect;
                        controller.Disconnect -= FlmOnServiceDisconnect;
                        controller.FrameReady -= FlmOnFrame;
                        controller.Device -= FlmOnConnect;
                        controller.DeviceLost -= FlmOnDisconnect;
                        controller.DeviceFailure -= FlmOnDeviceFailure;
                        controller.LogMessage -= FlmOnLogMessage;
                    }
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.ToString());
                }
            }
            private Ojw.CTimer m_CTmr_Leap_OnFrame = new Ojw.CTimer();
            public void FlmOnConnect(object sender, DeviceEventArgs args)
            {
                Ojw.CMessage.Write("Connected");
                m_CTmr_Leap_OnFrame.Set();
            }

            public void FlmOnDisconnect(object sender, DeviceEventArgs args)
            {
                Ojw.CMessage.Write("Disconnected");
            }

            public void FlmOnServiceConnect(object sender, ConnectionEventArgs args)
            {
                Ojw.CMessage.Write("Service Connected");
            }

            public void FlmOnServiceDisconnect(object sender, ConnectionLostEventArgs args)
            {
                Ojw.CMessage.Write("Service Disconnected");
            }

            public void FlmOnServiceChange(Controller controller)
            {
                Ojw.CMessage.Write("Service Changed");
            }

            public void FlmOnDeviceFailure(object sender, DeviceFailureEventArgs args)
            {
                Ojw.CMessage.Write("Device Error");
                Ojw.CMessage.Write("  PNP ID:" + args.DeviceSerialNumber);
                Ojw.CMessage.Write("  Failure message:" + args.ErrorMessage);
            }

            public void FlmOnLogMessage(object sender, LogEventArgs args)
            {
                switch (args.severity)
                {
                    case Leap.MessageSeverity.MESSAGE_CRITICAL:
                        Ojw.CMessage.Write("[Critical]");
                        break;
                    case Leap.MessageSeverity.MESSAGE_WARNING:
                        Ojw.CMessage.Write("[Warning]");
                        break;
                    case Leap.MessageSeverity.MESSAGE_INFORMATION:
                        Ojw.CMessage.Write("[Info]");
                        break;
                    case Leap.MessageSeverity.MESSAGE_UNKNOWN:
                        Ojw.CMessage.Write("[Unknown]");
                        break;
                }
                Ojw.CMessage.Write("[{0}] {1}", args.timestamp, args.message);
            }
            private Ojw.C3d m_C3d = null;
            public void Set3D(Ojw.C3d OjwC3d)
            {
                if (OjwC3d != null) m_C3d = OjwC3d;
            }
            public void SetRotation(float fX, float fY, float fZ)
            {
                m_fAngle_X = fX;
                m_fAngle_Y = fY;
                m_fAngle_Z = fZ;
            }
            float m_fAngle_X = 0.0f;//90.0f;
            float m_fAngle_Y = 0.0f;
            float m_fAngle_Z = 0.0f;

            // 값이 0에 가까울 수록 둔감
            float m_fWeight_For_Lowpass = 0.2f;//0.1f;//0.6f;

            private int m_nOffset_Mouse_X = 0;//0;
            private int m_nOffset_Mouse_Y = 1024;
            private float m_fOffset_X = 0.0f;
            private float m_fOffset_Y = 0.0f;
            private float m_fOffset_Z = 0.0f;

            private int m_nCommand = 0;
            private int m_nReady = 0;

            private int m_nMouse = -1;
            private int m_nMouse_X_Prev = 0;
            private int m_nMouse_Y_Prev = 0;
            private int m_nMouse_X = 0;
            private int m_nMouse_Y = 0;
            private Ojw.CTimer m_CTmr_Click = new Ojw.CTimer();
            private Ojw.CTimer m_CTmr_Find = new Ojw.CTimer();
            private Ojw.CTimer m_CTmr_Message = new Ojw.CTimer();
            public void FlmOnFrame(object sender, FrameEventArgs args)
            {
                bool bShow = true;// this.Visible; // true : 그린다. false : 안그린다.
                // 손의 위치를 잃어버린 시간이 1초가 넘으면 클릭을 없앤다.
                if (m_CTmr_Click.Get() > 1000)
                {
                    if (m_nMouse > 0) // 이미 클릭한 상황이라면
                    {

                    }
                    m_nMouse = -1;
                    m_CTmr_Find.Set();
                }
                m_CTmr_Click.Set();

                //float fOffset_X = 0.0f;
                //float fOffset_Y = 0.0f;
                //float fOffset_Z = 0.0f;

                int nOffset_Mouse_X = m_nOffset_Mouse_X;
                int nOffset_Mouse_Y = m_nOffset_Mouse_Y;

                float fAngle_X = m_fAngle_X;
                float fAngle_Y = m_fAngle_Y;
                float fAngle_Z = m_fAngle_Z;

                float fVelocity_Down = 300;// 500;// 300;
                float fVelocity_Up = -100;

                bool bLimit_Up = false;
#if _TEST
            bool bLimit_Down = false;// true;
#else
                bool bLimit_Down = true;// false;// true;
#endif
                bool bLimit_Left = false;
                bool bLimit_Right = false;

                int nLimit_Up = 0;
#if _TEST
            int nLimit_Down = 1024 - 20;
#else
                int nLimit_Down = 4400;// 5700;// 3700;// 1900;
#endif
                int nLimit_Left = 0;
                int nLimit_Right = 0;

#if _TEST
            if (m_CTmr_Leap_OnFrame.Get() >= 10)//50)
#else
                if (m_CTmr_Leap_OnFrame.Get() >= 10)//50)
#endif
                {
                    m_CTmr_Leap_OnFrame.Set();
                    // Get the most recent frame and report some basic information
                    Frame frame = args.frame;
#if false
                if (frame.Hands.Count > 0)
                {
                    OjwClear();

                    foreach (Hand hand in frame.Hands)
                    {
#if false
                        //Leap.Controller Ctrl = new Controller(hand.Id);
                        Finger finger = hand.Fingers[0];
                        
                        //Screen screen = new Screen();//Ctrl.CalibratedScreens.ClosestScreenHit(finger);
                        
                        //var tipVelocity = (int)finger.TipVelocity.Magnitude;
                        OjwDraw(Color.Red, finger.TipPosition.x, finger.TipPosition.y, finger.TipPosition.z);
                        /*
                        foreach (Finger finger in hand.Fingers)
                        {
                            Bone bnData;
                            for (int i = 0; i < 4; i++)
                            {
                                bnData = finger.Bone((Bone.BoneType)i);
                                //Ojw.CMessage.Write(
                                //  "      Bone: {0}, start: {1}, end: {2}, direction: {3}",
                                //  bone.Type, bone.PrevJoint, bone.NextJoint, bone.Direction
                                //);
                                OjwDraw(Color.Red, bnData.PrevJoint.x, bnData.PrevJoint.y, bnData.PrevJoint.z);
                                OjwDraw(Color.Blue, bnData.PrevJoint.x, bnData.PrevJoint.y, bnData.PrevJoint.z);
                            }
                        }*/
                        //OjwDraw(Color.Red, hand.PalmNormal.x, hand.PalmNormal.y, hand.PalmNormal.z);
#else
                        foreach (Finger finger in hand.Fingers)
                        {
                            if (finger.Type == Finger.FingerType.TYPE_INDEX)
                            {
                                int nMulti = 5;
                                if (m_nMouse < 0)
                                {
                                    m_nMouse = 0;
                                    //Point pt = Ojw.CMouse.Mouse_Get();
                                    //m_nMouse_X_Prev = pt.X;
                                    //m_nMouse_Y_Prev = pt.Y;
                                    //m_nMouse_X_Prev = (int)Math.Round(finger.TipPosition.x);
                                    //m_nMouse_Y_Prev = (int)Math.Round(finger.TipPosition.y);
                                    //m_nMouse_X_Prev = -(int)Math.Round(finger.TipPosition.x) * 3;// *(int)Math.Round(Math.Abs(finger.TipVelocity.x));
                                    //m_nMouse_Y_Prev = (int)Math.Round(finger.TipPosition.y) * 3;// *(int)Math.Round(Math.Abs(finger.TipVelocity.y));

                                    m_nMouse_X_Prev = (int)Math.Round(finger.TipPosition.x) * nMulti + Ojw.CSystem.GetMonitor_Width(0) / 2; // m_nMouse_X_Prev - m_nMouse_X;
                                    m_nMouse_Y_Prev = Ojw.CSystem.GetMonitor_Height(0) - (int)Math.Round(finger.TipPosition.y) * nMulti + 600;//m_nMouse_Y_Prev - m_nMouse_Y;
                                }
                                else
                                {
                                    //Point pt = Ojw.CMouse.Mouse_Get();
                                    //m_nMouse_X = pt.X;
                                    //m_nMouse_Y = pt.Y;

                                    //m_nMouse_X = -(int)Math.Round(finger.TipPosition.x) * 3;// * (int)Math.Round(Math.Abs(finger.TipVelocity.x));
                                    //m_nMouse_Y = (int)Math.Round(finger.TipPosition.y) * 3;// * (int)Math.Round(Math.Abs(finger.TipVelocity.y));

                                    int ndX = (int)Math.Round(finger.TipPosition.x) * nMulti + Ojw.CSystem.GetMonitor_Width(0) / 2; // m_nMouse_X_Prev - m_nMouse_X;
                                    int ndY = Ojw.CSystem.GetMonitor_Height(0) - (int)Math.Round(finger.TipPosition.y) * nMulti + 600;//m_nMouse_Y_Prev - m_nMouse_Y;

                                    float fWeight = 0.9f;

                                    m_nMouse_X = (int)Math.Round(Ojw.CMath.LowPassFilter(fWeight, (float)m_nMouse_X_Prev, (float)ndX));
                                    m_nMouse_Y = (int)Math.Round(Ojw.CMath.LowPassFilter(fWeight, (float)m_nMouse_Y_Prev, (float)ndY));
                                    ndX = m_nMouse_X;
                                    ndY = m_nMouse_Y;
                                    Ojw.CMouse.Mouse_Move_Abs(ndX, ndY);

                                    if (m_CTmr_Message.Get() > 1000)
                                    {
                                        m_CTmr_Message.Set();
                                        //Ojw.CMessage.Write("dx={0}, dy={1}, Prev[{2}. {3}], ", ndX, ndY, m_nMouse_X_Prev, m_nMouse_Y_Prev);
                                    }

#if false // Velocity 를 이용한 방법
                                    if (
                                        (Math.Abs(finger.TipVelocity.z) > Math.Abs(finger.TipVelocity.x)) &&
                                        (Math.Abs(finger.TipVelocity.z) > Math.Abs(finger.TipVelocity.y))
                                        )
                                    {
                                        if ((finger.TipVelocity.z < -150) && (m_nMouse == 0))
                                        {
                                            m_nMouse = 1;
                                            Ojw.CMessage.Write("Mouse Down={0}, Mouse={1}", finger.TipVelocity, Ojw.CMouse.Mouse_Get());
                                        }
                                        else if ((m_nMouse == 1) && (finger.TipVelocity.z > 100))
                                        {
                                            m_nMouse = 0;
                                            Ojw.CMessage.Write("Mouse Up={0}, Mouse={1}", finger.TipVelocity, Ojw.CMouse.Mouse_Get());
                                        }
                                    }
#else // Position 을 이용한 방법
                                    if ((finger.TipPosition.z < 0) && (m_nMouse == 0))
                                    {
                                        m_nMouse = 1;
                                        Ojw.CMessage.Write("Mouse Down={0}, Mouse={1}", finger.TipPosition.z, Ojw.CMouse.Mouse_Get());
                                        Ojw.CMouse.Mouse_Down();
                                    }
                                    else if ((m_nMouse == 1) && (finger.TipPosition.z > 10))
                                    {
                                        m_nMouse = 0;
                                        Ojw.CMessage.Write("Mouse Up={0}, Mouse={1}", finger.TipPosition.z, Ojw.CMouse.Mouse_Get());
                                        Ojw.CMouse.Mouse_Up();
                                    }
#endif
                                    m_nMouse_X_Prev = m_nMouse_X;
                                    m_nMouse_Y_Prev = m_nMouse_Y;
                                }
                                OjwDraw(Color.Red, finger.TipPosition.x + fOffset_X, finger.TipPosition.y + fOffset_Y, finger.TipPosition.z + fOffset_Z);
                            }
                            else
                                OjwDraw(Color.Blue, finger.TipPosition.x + fOffset_X, finger.TipPosition.y + fOffset_Y, finger.TipPosition.z + fOffset_Z);
                        }
#endif
                        OjwDraw(Color.Green, vtHand.x + fOffset_X, vtHand.y + fOffset_Y, vtHand.z + fOffset_Z);

                    }
                    OjwFlush();
                }
#else
                    if (frame.Hands.Count > 0)
                    {
                        OjwClear();
                        int nCnt = 0;
#if _LEFT_HAND
                        bool bLeft = false;
#endif
#if _RIGHT_HAND
                        bool bRight = false;
#endif
                        bool bHand = false;
                        foreach (Hand hand in frame.Hands)
                        {
                            bHand = false;
                            bRight = false;
                            bLeft = false;

                            Vector vtHand = new Vector(hand.PalmPosition);
                            Ojw.CMath.Rotation(fAngle_X, fAngle_Y, fAngle_Z, ref vtHand.x, ref vtHand.y, ref vtHand.z);

                            if (vtHand.z > 500) continue;

                            Vector vtFinger_Velocity = new Vector();
                            Vector vtTmp_Velocity = new Vector();
                            Vector vtFinger_Position = new Vector();

                            if (nCnt++ >= 2) continue;

#if _RIGHT_HAND
                            // 오른손 2중 감지 방지
                            if (bRight == false) //continue;
                            {
                                if (hand.IsRight == true)
                                {
                                    bRight = true;
                                    bHand = true;
                                }
                            }
#endif

#if _LEFT_HAND
                            // 왼손 2중 감지 방지
                            if (bLeft == false) //continue;
                            {
                                if (hand.IsLeft == true)
                                {
                                    bLeft = true;
                                    bHand = true;
                                }
                            }
#endif

                            SVector3D_t[] SHand = new SVector3D_t[6]; // thumb, index, ... wrist
                            if (bHand == true)
                            {
                                vtTmp_Velocity.x = 0;
                                vtTmp_Velocity.y = 0;
                                vtTmp_Velocity.z = 0;

                                float fVel = 0.0f;
                                float fSum = 0.0f;
                                float fIndex = 0.0f;
                                int nSum = 0;
                                bool bIndex = false;
                                int nType = 0;
                                foreach (Finger finger in hand.Fingers)
                                {
                                    vtFinger_Velocity = new Vector(finger.TipVelocity);
                                    vtFinger_Position = new Vector(finger.TipPosition);
                                    Ojw.CMath.Rotation(fAngle_X, fAngle_Y, fAngle_Z, ref vtFinger_Velocity.x, ref vtFinger_Velocity.y, ref vtFinger_Velocity.z);
                                    Ojw.CMath.Rotation(fAngle_X, fAngle_Y, fAngle_Z, ref vtFinger_Position.x, ref vtFinger_Position.y, ref vtFinger_Position.z);

                                    if (finger.Type == Finger.FingerType.TYPE_INDEX)
                                    {
                                        bIndex = true;
                                        fIndex = vtFinger_Velocity.y;
                                        if (bShow == true) OjwDraw(Color.Red, vtFinger_Position.x, vtFinger_Position.y, vtFinger_Position.z);

                                        vtTmp_Velocity.x += vtFinger_Velocity.x;
                                        vtTmp_Velocity.y += vtFinger_Velocity.y;
                                        vtTmp_Velocity.z += vtFinger_Velocity.z;

                                        nType = 1;
                                        SHand[nType].x = vtFinger_Position.x;
                                        SHand[nType].y = vtFinger_Position.y;
                                        SHand[nType].z = vtFinger_Position.z;
                                    }
                                    else
                                    {
                                        if (finger.Type != Finger.FingerType.TYPE_THUMB)
                                        {
                                            nSum++;
                                            fSum += vtFinger_Velocity.y;


                                            vtTmp_Velocity.x -= vtFinger_Velocity.x;
                                            vtTmp_Velocity.y -= vtFinger_Velocity.y;
                                            vtTmp_Velocity.z -= vtFinger_Velocity.z;

                                            fVel = (float)Math.Sqrt(vtTmp_Velocity.x * vtTmp_Velocity.x + vtTmp_Velocity.y * vtTmp_Velocity.y + vtTmp_Velocity.z * vtTmp_Velocity.z);


                                            nType = ((finger.Type == Finger.FingerType.TYPE_MIDDLE) ? 2 : ((finger.Type == Finger.FingerType.TYPE_RING) ? 3 : 4));
                                            SHand[nType].x = vtFinger_Position.x;
                                            SHand[nType].y = vtFinger_Position.y;
                                            SHand[nType].z = vtFinger_Position.z;
                                        }
                                        else
                                        {
                                            nType = 0;
                                            SHand[nType].x = vtFinger_Position.x;
                                            SHand[nType].y = vtFinger_Position.y;
                                            SHand[nType].z = vtFinger_Position.z;
                                        }

                                        if (bShow == true) OjwDraw(Color.Blue, vtFinger_Position.x, vtFinger_Position.y, vtFinger_Position.z);
                                    }


                                }
                                if (bIndex == true)
                                {
                                    if (nSum > 0)//(hand.Fingers.Count - 1 > 0)
                                    {
                                        fSum /= nSum;// (hand.Fingers.Count - 1);
                                        fSum = fSum - fIndex;// Math.Abs(fSum - fIndex);
                                    }
                                    else
                                    {
                                        fSum = 0.0f;
                                        fVel = 0.0f;
                                    }

                                    int nMulti = 5;
                                    if (m_nMouse < 0)
                                    {
                                        m_nMouse = 0;
                                        m_CTmr_Find.Set();

                                        m_nMouse_X_Prev = (int)Math.Round(vtHand.x) * nMulti + Ojw.CSystem.GetMonitor_Width(0) / 2;
                                        m_nMouse_Y_Prev = Ojw.CSystem.GetMonitor_Height(0) - (int)Math.Round(vtHand.y) * nMulti + Ojw.CSystem.GetMonitor_Height(0) / 2;
                                    }
                                    else
                                    {
                                        int ndX = (int)Math.Round(vtHand.x) * nMulti + Ojw.CSystem.GetMonitor_Width(0) / 2;
                                        int ndY = Ojw.CSystem.GetMonitor_Height(0) - (int)Math.Round(vtHand.y) * nMulti + Ojw.CSystem.GetMonitor_Height(0) / 2;

                                        if (bLimit_Up == true)
                                        {
                                            if (ndY < nLimit_Up)
                                            {
                                                ndY = nLimit_Up;
                                                continue;
                                            }
                                        }
                                        if (bLimit_Down == true)
                                        {
                                            if (ndY > nLimit_Down)
                                            {
                                                ndY = nLimit_Down;
                                                continue;
                                            }
                                        }
                                        if (bLimit_Left == true)
                                        {
                                            if (ndX < nLimit_Left)
                                            {
                                                ndX = nLimit_Left;
                                                continue;
                                            }
                                        }
                                        if (bLimit_Right == true)
                                        {
                                            if (ndX > nLimit_Right)
                                            {
                                                ndX = nLimit_Right;
                                                continue;
                                            }
                                        }

                                        float fWeight = m_fWeight_For_Lowpass;

                                        m_nMouse_X = (int)Math.Round(Ojw.CMath.LowPassFilter(fWeight, (float)m_nMouse_X_Prev, (float)ndX));
                                        m_nMouse_Y = (int)Math.Round(Ojw.CMath.LowPassFilter(fWeight, (float)m_nMouse_Y_Prev, (float)ndY));
                                        ndX = m_nMouse_X;
                                        ndY = m_nMouse_Y;
                                        //ndX = m_nMouse_X + nOffset_Mouse_X;
                                        //ndY = m_nMouse_Y + nOffset_Mouse_Y;


                                        if (m_nCommand != 0)
                                        {
                                            float fPan, fTilt, fSwing;
                                            m_C3d.GetAngle_Display(out fPan, out fTilt, out fSwing);
                                            m_C3d.SetAngle_Display(fPan + (float)ndX, fTilt + (float)ndY, fSwing);
                                        }
#if !_NO_MOUSE_CONTROL
                                        Ojw.CMouse.Mouse_Move_Abs(ndX + nOffset_Mouse_X, ndY + nOffset_Mouse_Y);
#endif
                                        //Ojw.CMouse.Mouse_Move_Abs(ndX, ndY);
                                        
                                        // 프레임이 시작된 지 2초 후부터 명령을 받는다. - 체터링 방지
                                        if (m_CTmr_Find.Get() < 2000) return;

                                        if (m_CTmr_Message.Get() > 1000)
                                        {
                                            m_CTmr_Message.Set();
                                            if (bShow == true)
                                            {
                                                Ojw.CMessage.Write("***fSum={0}, fVel={1}***", fSum, fVel);
                                                Ojw.CMessage.Write("[Mouse({3}, {4}):[x={0}, y={1}, z={2}]", vtHand.x, vtHand.y, vtHand.z, m_nMouse_X, m_nMouse_Y);
                                                //Ojw.CMessage.Write("dx={0}, dy={1}, Prev[{2}. {3}], ", ndX, ndY, m_nMouse_X_Prev, m_nMouse_Y_Prev);
                                            }
                                        }

                                        if ((fSum > fVelocity_Down) && (m_nMouse == 0))
                                        {
                                            if (m_nReady != 0)
                                            {
                                                if (bLeft == true)
                                                {
                                                    m_nCommand = 1;
                                                }
                                            }
                                            // Click
                                            if (bRight == true)
                                            {
                                                m_nReady = 1;
                                            }


                                            if (bShow == true)
                                            {
                                                Ojw.CMessage.Write("Mouse Down[fSum={0}, fVel={1}] Ready({2}), Command({3})", fSum, fVel, m_nReady, m_nCommand);
                                                //Ojw.CKeyboard.Key_Click(Keys.PageDown);
                                            }

                                            
#if !_NO_MOUSE_CONTROL
                                            Ojw.CMouse.Mouse_Down();
#if _NORMAL_DIRECTION
#else
                                            Ojw.CMouse.Mouse_Up();
#endif
#endif
                                            m_nMouse = 1;
                                        }
                                        else if ((fSum < fVelocity_Up) && (m_nMouse == 1))
                                        {
                                            if (bShow == true)
                                            {
                                                Ojw.CMessage.Write("Mouse Up[fSum={0}, fVel={1}] Ready({2}), Command({3})", fSum, fVel, m_nReady, m_nCommand);
                                            }
                                            if (bRight == true)
                                            {
                                                m_nReady = 0;
                                                m_nCommand = 0;
                                            }
#if !_NO_MOUSE_CONTROL
                                            Ojw.CMouse.Mouse_Up();
#endif
                                            m_nMouse = 0;
                                        }
                                    }
                                }
                                m_nMouse_X_Prev = m_nMouse_X;
                                m_nMouse_Y_Prev = m_nMouse_Y;


                                //// Hand
                                nType = 5;
                                SHand[nType].x = vtFinger_Position.x;
                                SHand[nType].y = vtFinger_Position.y;
                                SHand[nType].z = vtFinger_Position.z;
                                // 0 - 엄지
                                // 5 - 손목
                                float fX = SHand[0].x - SHand[5].x;
                                float fY = SHand[0].y - SHand[5].y;

                                float fLength_X = 100.0f;
                                float fLength_Z = 300.0f;
                                float fAngleXY = (float)Ojw.CMath.ATan2(fX, fY);
                                if ((fAngleXY < 90 + 20) && (fAngleXY > 90 - 20))
                                {
                                    OjwDraw_X(Color.Red, 0, 0, 0, 0, 0, 90, 20, fLength_X);
                                    OjwDraw_Z(Color.Blue, 0, 0, 0, 180, 0, 0, 20, fLength_Z);
                                    //for (int i = 0; i < 10; i++) OjwDraw(Color.White, 0, i * 20, 0);
                                    //for (int i = 0; i < 30; i++) OjwDraw(Color.Green, 0, 0, -i * 20);
                                }
                                else if ((fAngleXY < 20) && (fAngleXY > -20))
                                {
                                    OjwDraw_X(Color.Red, 0, 0, 0, 0, 0, 0, 20, fLength_X);
                                    OjwDraw_Z(Color.Blue, 0, 0, 0, 180, 0, 0, 20, fLength_Z);
                                    //OjwDraw_X(Color.Red, 0, 0, 0, 0, 0, 0, 10, 100);
                                    //for (int i = 0; i < 10; i++) OjwDraw(Color.White, i * 20, 0, 0);
                                    //for (int i = 0; i < 30; i++) OjwDraw(Color.Green, 0, 0, -i * 20);
                                    //OjwDraw(Color.White, 0, 0, 0);
                                    //OjwDraw(Color.White, 20, 0, 0);
                                    //OjwDraw(Color.White, 40, 0, 0);
                                    //OjwDraw(Color.White, 60, 0, 0);
                                    //OjwDraw(Color.White, 80, 0, 0);
                                }
                                else if ((fAngleXY < 180+20) && (fAngleXY > 180-20))
                                {
                                    OjwDraw_X(Color.Red, 0, 0, 0, 0, 0, 180, 20, fLength_X);
                                    OjwDraw_Z(Color.Blue, 0, 0, 0, 180, 0, 0, 20, fLength_Z);
                                    //for (int i = 0; i < 10; i++) OjwDraw(Color.White, -i * 20, 0, 0);
                                    //for (int i = 0; i < 30; i++) OjwDraw(Color.Green, 0, 0, -i * 20);
                                    //OjwDraw(Color.White, 0, 0, 0);
                                    //OjwDraw(Color.White, -20, 0, 0);
                                    //OjwDraw(Color.White, -40, 0, 0);
                                    //OjwDraw(Color.White, -60, 0, 0);
                                    //OjwDraw(Color.White, -80, 0, 0);
                                }
                                else if (
                                    ((fAngleXY < -90 + 20) && (fAngleXY > -90-20)) ||
                                    ((fAngleXY < 270 + 20) && (fAngleXY > 270 - 20))
                                    )
                                {
                                    OjwDraw_X(Color.Red, 0, 0, 0, 0, 0, -90, 20, fLength_X);
                                    OjwDraw_Z(Color.Blue, 0, 0, 0, 180, 0, 0, 20, fLength_Z);
                                    //for (int i = 0; i < 10; i++) OjwDraw(Color.White, 0, -i * 20, 0);
                                    //for (int i = 0; i < 30; i++) OjwDraw(Color.Green, 0, 0, -i * 20);
                                    //OjwDraw(Color.White, 0, 0, 0);
                                    //OjwDraw(Color.White, 0, -20, 0);
                                    //OjwDraw(Color.White, 0, -40, 0);
                                    //OjwDraw(Color.White, 0, -60, 0);
                                    //OjwDraw(Color.White, 0, -80, 0);
                                }

                                if (CTmrTest.IsTimer() == false) CTmrTest.Set();
                                else
                                {
                                    if (CTmrTest.Get() > 2000)
                                    {
                                        CTmrTest.Set();
                                        //Ojw.CMessage.Write("Angle = {0}", fAngleXY);
                                        for (int i = 0; i < 6; i++)
                                            Ojw.CMessage.Write2("{0}[{1},{2},{3}]", i, (int)Math.Round(SHand[i].x), (int)Math.Round(SHand[i].y), (int)Math.Round(SHand[i].z));
                                        Ojw.CMessage.Write2("\r\n");
                                    }
                                }

                                if (bShow == true) OjwDraw(Color.Yellow, vtHand.x, vtHand.y, vtHand.z);
                            }
                        }
                        //if (bShow == true) OjwFlush();
                    }
                    else
                    {
                        
                    }
#endif
                }
            }
            private Ojw.CTimer CTmrTest = new CTimer();


            private void OjwClear()
            {
                if (m_C3d != null) m_C3d.User_Clear();
            }
            private void OjwDraw(Color cColor, float fX, float fY, float fZ)
            {

                Ojw.C3d.COjwDisp CDisp = new Ojw.C3d.COjwDisp();

                CDisp.InitData();
                CDisp.bInit = true;
                CDisp.cColor = cColor;
                CDisp.strDispObject = "#9";
                CDisp.fWidth_Or_Radius = 10.0f;
                CDisp.fHeight_Or_Depth = 10.0f;
                CDisp.fDepth_Or_Cnt = 10.0f;

                CDisp.afTrans[0].x = fX;
                CDisp.afTrans[0].y = fY;
                CDisp.afTrans[0].z = fZ;

                if (m_C3d != null) m_C3d.User_Add_Ex(CDisp);


            }
            private void OjwDraw_X(Color cColor, float fX, float fY, float fZ, float fPan, float fTilt, float fSwing, float fSize, float fLength)
            {
                Ojw.C3d.COjwDisp CDisp = new Ojw.C3d.COjwDisp();

                CDisp.InitData();
                CDisp.bInit = true;
                CDisp.cColor = cColor;
                CDisp.strDispObject = "#11";
                CDisp.fWidth_Or_Radius = fSize;
                CDisp.fHeight_Or_Depth = fLength;
                CDisp.fDepth_Or_Cnt = 10.0f;

                CDisp.afTrans[0].x = fX;
                CDisp.afTrans[0].y = fY;
                CDisp.afTrans[0].z = fZ;

                CDisp.SetData_Offset_Rotation(fPan, fTilt, fSwing);

                if (m_C3d != null) m_C3d.User_Add_Ex(CDisp);
            }
            private void OjwDraw_Y(Color cColor, float fX, float fY, float fZ, float fPan, float fTilt, float fSwing, float fSize, float fLength)
            {
                Ojw.C3d.COjwDisp CDisp = new Ojw.C3d.COjwDisp();

                CDisp.InitData();
                CDisp.bInit = true;
                CDisp.cColor = cColor;
                CDisp.strDispObject = "#12";
                CDisp.fWidth_Or_Radius = fSize;
                CDisp.fHeight_Or_Depth = fLength;
                CDisp.fDepth_Or_Cnt = 10.0f;

                CDisp.afTrans[0].x = fX;
                CDisp.afTrans[0].y = fY;
                CDisp.afTrans[0].z = fZ;

                CDisp.SetData_Offset_Rotation(fPan, fTilt, fSwing);

                if (m_C3d != null) m_C3d.User_Add_Ex(CDisp);
            }
            private void OjwDraw_Z(Color cColor, float fX, float fY, float fZ, float fPan, float fTilt, float fSwing, float fSize, float fLength)
            {
                Ojw.C3d.COjwDisp CDisp = new Ojw.C3d.COjwDisp();

                CDisp.InitData();
                CDisp.bInit = true;
                CDisp.cColor = cColor;
                CDisp.strDispObject = "#13";
                CDisp.fWidth_Or_Radius = fSize;
                CDisp.fHeight_Or_Depth = fLength;
                CDisp.fDepth_Or_Cnt = 10.0f;

                CDisp.afTrans[0].x = fX;
                CDisp.afTrans[0].y = fY;
                CDisp.afTrans[0].z = fZ;

                CDisp.SetData_Offset_Rotation(fPan, fTilt, fSwing);

                if (m_C3d != null) m_C3d.User_Add_Ex(CDisp);
            }

            private void OjwFlush()
            {
                if (m_C3d != null) m_C3d.OjwDraw();
            }

        }
    }
}
