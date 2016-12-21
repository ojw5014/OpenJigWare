using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

#if true
using AForge.Controls;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Video.DirectShow;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
#endif

namespace OpenJigWare
{
    partial class Ojw
    {
#if true
        public class CCamera
        {
            public CCamera()
            {
            }
            ~CCamera()
            {

            }

            #region AForge
            private Control m_ctrlDisp;
            private VideoSourcePlayer m_vsPlayer = new VideoSourcePlayer();
            public void Init(Control ctrlDisp, int nCameraIndex)
            {
                m_ctrlDisp = ctrlDisp;

                m_vsPlayer.Size = (m_ctrlDisp != null) ? m_ctrlDisp.Size : new Size(320, 240);
                m_vsPlayer.Location = new Point(0, 0);
                if (m_ctrlDisp != null) m_ctrlDisp.Controls.Add(m_vsPlayer);


                int nID = nCameraIndex;
                // AForge 초기화
                //디바이스 설정
                FilterInfoCollection device = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                //카메라 설정 :: 디바이스 0 번째를 가져다( 카메라가 하나일경우 )
                if (nID >= device.Count) nID = device.Count - 1;
                VideoCaptureDevice cam = new VideoCaptureDevice(device[nID].MonikerString);
                //카메라 fps 설정 :: 안해도 됨
                //cam.DesiredFrameRate = 30;

                //플레이어에 적용 / 시작
                m_vsPlayer.VideoSource = cam;
                //m_vsPlayer.Start();
                //Ojw.CMessage.Write(txtMessage, "AForge 0 Device - Started");
            }
            public bool IsRunning() { try {return m_vsPlayer.IsRunning;} catch(Exception ex) {return false;} }
            public Bitmap Grab()
            {
                return m_vsPlayer.GetCurrentVideoFrame();
            }
            public void Start()
            {
                if (IsRunning() == true) Stop();
                m_vsPlayer.Start();
            }
            public void Stop()
            {
                if (IsRunning() == true)
                {
                    m_vsPlayer.SignalToStop();
                    m_vsPlayer.WaitForStop();
                }
            }
            #endregion AForge
        }
#endif
    }
}
