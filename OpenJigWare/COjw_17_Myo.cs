/// Myo
#define _DEF_ORIENTATION
#define _DEF_EMG

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;

using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Poses;

//using System.Windows.Forms;
//using MyoSharp.Poses;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CMyo
        {
            private bool m_bMyoDll = false;
            public CMyo()
            {
                if (CFile.IsFiles("Microsoft.Contracts.dll", "MyoSharp.dll", ((CSystem.Is64Bits() == true) ? "x64\\myo.dll" : "x86\\myo.dll")) == false)
                {
                    Ojw.CMessage.Write("[Warning] There's no any Myo Dll Files");
                }
                else m_bMyoDll = true;
                //if (m_bMyoDll == true)
                //    MessageBox.Show("Ok");
                //else
                //    MessageBox.Show("No Files");
            }

            #region Myo Step(1)
            private IChannel m_myoChannel;
            private IHub m_myoHub;
            private IHeldPose m_myoPos;
            private bool m_bInit = false;
            public void InitMyo()
            {
                m_bInit = false;
                if (m_bMyoDll == false) return;
                try
                {
                    m_myoChannel = Channel.Create(ChannelDriver.Create(ChannelBridge.Create(), MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));
                    m_myoHub = Hub.Create(m_myoChannel);

                    // 이벤트 등록            
                    m_myoHub.MyoConnected += new EventHandler<MyoEventArgs>(myoHub_MyoConnected); // 접속했을 때 myoHub_MyoConnected() 함수가 동작하도록 등록
                    m_myoHub.MyoDisconnected += new EventHandler<MyoEventArgs>(myoHub_MyoDisconnected); // 접속했을 때 myoHub_MyoDisconnected() 함수가 동작하도록 등록

                    // start listening for Myo data
                    m_myoChannel.StartListening();
                    m_bInit = true;
                }
                catch (Exception ex)
                {
                    m_bInit = false;
                    Ojw.CMessage.Write_Error("Error -> {0}", ex.ToString());
                }
            }
            public bool IsInit() { return m_bInit; }
            public void DInitMyo()
            {                
                m_bRight = false;
                m_bLeft = false;
                if (m_bMyoDll == false) return;
                m_myoChannel.StopListening();

                m_myoHub.Dispose();
                m_myoChannel.Dispose();
            }
            private void Myo_Unlocked(object sender, MyoEventArgs e)
            {
                Ojw.CMessage.Write("UnLocked", e.Myo.Handle);
            }
            private void Myo_Locked(object sender, MyoEventArgs e)
            {
                Ojw.CMessage.Write("Locked", e.Myo.Handle);
            }
            private Pose m_ER_Pose = new Pose();
            private Pose m_EL_Pose = new Pose();
            private void Myo_PoseChanged(object sender, MyoEventArgs e)
            {
                Ojw.CMessage.Write("{0} arm Myo detected {1} pose!", e.Myo.Arm, e.Myo.Pose);
                if (e.Myo.Arm == Arm.Right)
                {
                    m_ER_Pose = e.Myo.Pose;
                }
                else if (e.Myo.Arm == Arm.Left)
                {
                    m_EL_Pose = e.Myo.Pose;
                }
                nSeq_Pos++;
            }
            private bool m_bRight = false;
            private bool m_bLeft = false;
            public bool GetData(
                                out bool bRight, out float fR_Pan, out float fR_Tilt, out float fR_Swing, out Pose ER_Pose,
                                out bool bLeft, out float fL_Pan, out float fL_Tilt, out float fL_Swing, out Pose EL_Pose)
            {
                if (IsInit() == false)
                {
                    bRight = bLeft = false;
                    fR_Pan = 0.0f;
                    fR_Tilt = 0.0f;
                    fR_Swing = 0.0f;
                    fL_Pan = 0.0f;
                    fL_Tilt = 0.0f;
                    fL_Swing = 0.0f;
                    ER_Pose = Pose.Unknown;
                    EL_Pose = Pose.Unknown;
                    return false;
                }

                bRight = m_bRight;
                bLeft = m_bLeft;
                fR_Pan = m_fR_Yaw;
                fR_Tilt = m_fR_Pitch;
                fR_Swing = m_fR_Roll;
                fL_Pan = m_fL_Yaw;
                fL_Tilt = m_fL_Pitch;
                fL_Swing = m_fL_Roll;

                ER_Pose = m_ER_Pose;
                EL_Pose = m_EL_Pose;

                return true;
            }
            private int nSeq_Pos = 0;
            private int nSeq_Pos_Back = 0;
            private void Pose_Triggered(object sender, PoseEventArgs e)
            {
                m_nPos = (int)e.Pose;
                //Ojw.CMessage.Write("{0} arm Myo detected [{1}] pose", e.Myo.Arm, m_nPos);
            }
            private Ojw.CTimer m_CTId0 = new Ojw.CTimer();
            private float[] m_afInitAngle = new float[3];
            //private bool m_bFirst = true;
            //public void GetOrientationData(out float fRoll, out float fPitch, out float fYaw, out float fX, out float fY, out float fW)
            //{
            //    fRoll = m_fRoll;
            //    fPitch = m_fPitch;
            //    fYaw = m_fYaw;
            //    fX = m_fX;
            //    fY = m_fY;
            //    fW = m_fW;
            //}

            private float m_fR_Roll = 0.0f;
            private float m_fR_Pitch = 0.0f;
            private float m_fR_Yaw = 0.0f;
            private float m_fR_X = 0.0f;
            private float m_fR_Y = 0.0f;
            private float m_fR_W = 0.0f;

            private float m_fL_Roll = 0.0f;
            private float m_fL_Pitch = 0.0f;
            private float m_fL_Yaw = 0.0f;
            private float m_fL_X = 0.0f;
            private float m_fL_Y = 0.0f;
            private float m_fL_W = 0.0f;
            
            private void Myo_OrientationDataAcquired(object sender, OrientationDataEventArgs e)
            {
                if (e.Myo.Arm == Arm.Right)
                {
                    m_bRight = true;


                    const float PI = (float)System.Math.PI;

                    int nDev = 1; //10;
                    // convert the values to a 0-9 scale (for easier digestion/understanding)
                    float fRoll = (float)((e.Roll + PI) / (PI * 2.0f) * nDev);
                    float fPitch = (float)((e.Pitch + PI) / (PI * 2.0f) * nDev);
                    float fYaw = (float)((e.Yaw + PI) / (PI * 2.0f) * nDev);

                    //if (m_CTId0.Get() >= 1000)
                    //{
                    //m_CTId0.Set();
                    //if (m_bFirst == true)
                    //{
                    //    m_afInitAngle[0] = e.Orientation.X;
                    //    m_afInitAngle[1] = e.Orientation.Y;
                    //    m_afInitAngle[2] = e.Orientation.W;
                    //}
                    float fX = (float)Math.Round(Ojw.CMath.R2D(e.Orientation.X - m_afInitAngle[0]), 3);
                    float fY = (float)Math.Round(Ojw.CMath.R2D(e.Orientation.Y - m_afInitAngle[1]), 3);
                    float fW = (float)Math.Round(Ojw.CMath.R2D(e.Orientation.W - m_afInitAngle[2]), 3);
                    //float fSwing = (float)Math.Round(Ojw.CMath.R2D(e.Roll), 3);
                    //float fTilt = (float)Math.Round(Ojw.CMath.R2D(e.Pitch), 3);
                    //float fPan = (float)Math.Round(Ojw.CMath.R2D(e.Yaw), 3);
                    //m_C3d.SetRobot_Rot(fPan - 90.0f, -fTilt, -fSwing);
                    //Ojw.CMessage.Write("[{6}]Roll[{0}], Pitch[{1}], Yaw[{2}] || X[{3}], Y[{4}], W[{5}]", nRoll, nPitch, nYaw, fX, fY, fW, e.Myo.Handle);
                    //}


                    m_fR_Roll = fRoll;
                    m_fR_Pitch = fPitch;
                    m_fR_Yaw = fYaw;
                    m_fR_X = fX;
                    m_fR_Y = fY;
                    m_fR_W = fW;
                }
                else if (e.Myo.Arm == Arm.Left)
                {
                    m_bLeft = true;


                    const float PI = (float)System.Math.PI;

                    int nDev = 1; //10;
                    // convert the values to a 0-9 scale (for easier digestion/understanding)
                    float fRoll = (float)((e.Roll + PI) / (PI * 2.0f) * nDev);
                    float fPitch = (float)((e.Pitch + PI) / (PI * 2.0f) * nDev);
                    float fYaw = (float)((e.Yaw + PI) / (PI * 2.0f) * nDev);

                    //if (m_CTId0.Get() >= 1000)
                    //{
                    //m_CTId0.Set();
                    //if (m_bFirst == true)
                    //{
                    //    m_afInitAngle[0] = e.Orientation.X;
                    //    m_afInitAngle[1] = e.Orientation.Y;
                    //    m_afInitAngle[2] = e.Orientation.W;
                    //}
                    float fX = (float)Math.Round(Ojw.CMath.R2D(e.Orientation.X - m_afInitAngle[0]), 3);
                    float fY = (float)Math.Round(Ojw.CMath.R2D(e.Orientation.Y - m_afInitAngle[1]), 3);
                    float fW = (float)Math.Round(Ojw.CMath.R2D(e.Orientation.W - m_afInitAngle[2]), 3);
                    //float fSwing = (float)Math.Round(Ojw.CMath.R2D(e.Roll), 3);
                    //float fTilt = (float)Math.Round(Ojw.CMath.R2D(e.Pitch), 3);
                    //float fPan = (float)Math.Round(Ojw.CMath.R2D(e.Yaw), 3);
                    //m_C3d.SetRobot_Rot(fPan - 90.0f, -fTilt, -fSwing);
                    //Ojw.CMessage.Write("[{6}]Roll[{0}], Pitch[{1}], Yaw[{2}] || X[{3}], Y[{4}], W[{5}]", nRoll, nPitch, nYaw, fX, fY, fW, e.Myo.Handle);
                    //}


                    m_fL_Roll = fRoll;
                    m_fL_Pitch = fPitch;
                    m_fL_Yaw = fYaw;
                    m_fL_X = fX;
                    m_fL_Y = fY;
                    m_fL_W = fW;
                }
            }
            private void myoHub_MyoConnected(object sender, MyoEventArgs e)
            {
                Ojw.CMessage.Write("Myo [{0}, {1}, {2}] has connected!", e.Myo.Handle, e.Myo.XDirectionOnArm.ToString(), e.Myo.Arm.ToString());

                e.Myo.Vibrate(VibrationType.Short); // 접속에 성공했으니 짧게 진동 출력


                e.Myo.Locked += Myo_Locked;
                e.Myo.Unlocked += Myo_Unlocked;
                //e.Myo.Locked += new EventHandler<MyoEventArgs>(Myo_Locked);
                //e.Myo.Unlocked += new EventHandler<MyoEventArgs>(Myo_Unlocked);
                #region Pose(Edge - 자세가 변하는 순간에만 기록)
                e.Myo.PoseChanged += Myo_PoseChanged;
                #endregion Pose(Edge - 자세가 변하는 순간에만 기록)

                #region Pose(자세가 계속적으로 기록...)
                // setup for the pose we want to watch for
                m_myoPos = HeldPose.Create(e.Myo, Pose.Fist, Pose.FingersSpread, Pose.WaveIn, Pose.WaveOut, Pose.Rest);
                // set the interval for the event to be fired as long as 
                // the pose is held by the user
                m_myoPos.Interval = TimeSpan.FromSeconds(0.5);

                m_myoPos.Start();
                m_myoPos.Triggered += Pose_Triggered;
                #endregion Pose(자세가 계속적으로 기록...)

                e.Myo.Unlock(UnlockType.Hold); // 이걸 마지막에 선언하면 Myo 가 내버려 두어도 Lock 이 되지 않는다.

                #region Orientation
                e.Myo.OrientationDataAcquired += Myo_OrientationDataAcquired;
                #endregion Orientation

#if false //_DEF_EMG
            e.Myo.EmgDataAcquired += Myo_EmgDataAcquired;
            e.Myo.SetEmgStreaming(true);
            //tmrPulse.Enabled = true;
#endif
            }
            //private float[] m_afAcc = new float[3];
            //private float[] m_afGyro = new float[3];
            //private float[] m_afAngle = new float[3];
            private float[] m_afOrientation = new float[3];
            private int m_nPos = -1;
            private void myoHub_MyoDisconnected(object sender, MyoEventArgs e)
            {
                e.Myo.Locked -= Myo_Locked;
                e.Myo.Unlocked -= Myo_Unlocked;
                e.Myo.PoseChanged -= Myo_PoseChanged;

                m_myoPos.Stop();
                m_myoPos.Triggered -= Pose_Triggered;

                #region Orientation
                e.Myo.OrientationDataAcquired -= Myo_OrientationDataAcquired;
                #endregion Orientation
#if false // _DEF_EMG
                e.Myo.SetEmgStreaming(false);
                e.Myo.EmgDataAcquired -= Myo_EmgDataAcquired;
                //tmrPulse.Enabled = false;
#endif
                Ojw.CMessage.Write("접속해제");
            }
            #endregion Myo Step(1)
        }
    }
}
