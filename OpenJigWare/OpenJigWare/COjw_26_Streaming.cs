// ====[Using OpenSource - AForge.net] 원 저자 참조 - 다음의 주석 참조 ====
// AForge FFMPEG Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright ?AForge.NET, 2009-2011
// contacts@aforgenet.com
//========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using AForge.Video;
//using AForge.Video.DirectShow;

using System.Windows.Forms;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CStream
        {
            public CStream()
            {
            }
            ~CStream() { if (m_ctrlDisp != null) Close(); }
            
            #region Variable
            private Control m_ctrlDisp = null;
            private readonly string _SUBSTRING = "/?action=stream";
            private AForge.Controls.VideoSourcePlayer m_afgPlayer = null;// new AForge.Controls.VideoSourcePlayer();
            #endregion Variable

            #region Private Function
            private bool Init(Control ctrlDisp, int nX, int nY, int nWidth, int nHeight)
            {
                try
                {
                    if (IsReady == false) return false;
                    m_afgPlayer.Left = nX;
                    m_afgPlayer.Top = nY;
                    m_afgPlayer.Width = nWidth;
                    m_afgPlayer.Height = nHeight;
                    m_afgPlayer.Visible = true;
                    m_ctrlDisp = ctrlDisp;
                    m_ctrlDisp.Controls.Add(m_afgPlayer);
                    return true;
                }
                catch(Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.Message);
                    return false;
                }
            }
            private bool Open(Control ctrlDisp, IVideoSource source, int nX, int nY, int nWidth, int nHeight)
            {
                try
                {
                    if (IsReady == false) return false;
                    if (m_afgPlayer == null) m_afgPlayer = new AForge.Controls.VideoSourcePlayer();
                    // set busy cursor
                    //this.Cursor = Cursors.WaitCursor;

                    // stop current video source
                    if (IsStreaming == true) Close();

                    if (Init(ctrlDisp, nX, nY, nWidth, nHeight) == false) return false;

                    // start new video source
                    m_afgPlayer.VideoSource = source;
                    m_afgPlayer.Start();

                    //this.Cursor = Cursors.Default;
                    return true;
                }
                catch(Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.Message);
                    return false;
                }
            }
            // Close video source if it is running
            private bool Close()
            {
                try
                {
                    if (IsReady == false) return false;
                    //ctrlDisp.Controls.IndexOf(m_afgPlayer);//.Remove(m_afgPlayer);
                    if (m_afgPlayer.VideoSource != null)
                    {
                        m_afgPlayer.SignalToStop();

                        // 스트리밍 정지 확인(3초간 대기)
                        Ojw.CTimer CTmr = new CTimer();
                        CTmr.Set();
                        while (IsStreaming) { if (CTmr.Get() < 3000) Ojw.CTimer.Wait(100); else break; }

                        if (IsStreaming) m_afgPlayer.Stop();

                        m_afgPlayer.VideoSource = null;
                        m_ctrlDisp.Controls.Clear();
                        m_ctrlDisp = null;
                        m_afgPlayer = null;
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.Message);
                    return false;
                }
            }
            #endregion Private Function

            #region Checking Variable
            public bool IsReady { get { if (CFile.IsFiles("AForge.Controls.dll", "AForge.Video.dll", "AForge.Imaging.dll", "AForge.dll") == false) return false; return true; } }
            public bool IsStreaming { get { return (m_afgPlayer == null) ? false : m_afgPlayer.IsRunning; } }
            #endregion Checking Variable

            #region Start Command
            public bool Start_Jpeg(Control ctrlDisp, string strIp, int nPort, int nX, int nY, int nWidth, int nHeight)
            {
                try
                {
                    if (IsReady == false) return false;
                    if (IsStreaming == false)
                    {
                        JPEGStream jpegSource = new JPEGStream(String.Format("http://{0}:{1}{2}", strIp, nPort, _SUBSTRING));
                        return Open(ctrlDisp, jpegSource, nX, nY, nWidth, nHeight);
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.Message);
                    return false;
                }
            }
            public bool Start_Jpeg(Control ctrlDisp, string strIp, int nPort, int nWidth, int nHeight) { return Start_Jpeg(ctrlDisp, strIp, nPort, 0, 0, nWidth, nHeight); }
            public bool Start_MJpeg(Control ctrlDisp, string strIp, int nPort, int nX, int nY, int nWidth, int nHeight)
            {
                try
                {
                    if (IsReady == false) return false;
                    if (IsStreaming == false)
                    {
                        MJPEGStream mjpegSource = new MJPEGStream(String.Format("http://{0}:{1}{2}", strIp, nPort, _SUBSTRING));
                        return Open(ctrlDisp, mjpegSource, nX, nY, nWidth, nHeight);
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.Message);
                    return false;
                }
            }
            public bool Start_MJpeg(Control ctrlDisp, string strIp, int nPort, int nWidth, int nHeight) { return Start_MJpeg(ctrlDisp, strIp, nPort, 0, 0, nWidth, nHeight); }
            #endregion Start Command

            #region Stop Command
            public bool Stop() 
            {
                try
                {
                    if (IsReady == false) return false;
                    if (IsStreaming == true)
                    {
                        return Close();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.Message);
                    return false;
                }
            }
            #endregion Stop Command
        }
    }
}
