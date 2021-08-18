// ====[Using OpenSource - AForge.net] 원 저자 참조 - 다음의 주석 참조 ====


//== CStream_Server =============================
// I got this source from : http://www.codeproject.com/Articles/371955/Motion-JPEG-Streaming-Server
// made by Ragheed AI-Tayed
// -------------------------------------------------
// Developed By : Ragheed Al-Tayeb
// e-Mail       : ragheedemail@gmail.com
// Date         : April 2012
// -------------------------------------------------
//========================================
//== CStream =============================
// AForge FFMPEG Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright ?AForge.NET, 2009-2011
// contacts@aforgenet.com
//========================================

// app.config : 설정 탭에 들어가면 "이 프로젝트에는 기본 설정파일이 없습니다. 기본 설정 파일을 만들려면 여기를 클릭하십시오." 가 있다. 이걸 클릭하면 app.config 가 생성
//  - 여기서 이름 -> SettingSome, 값 -> SomeValue 로 하면 파일이 생성됨

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;

using AForge.Video;
//using AForge.Video.DirectShow;

using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using rtaNetworking.Streaming;
using OpenJigWare;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace OpenJigWare
{
    partial class Ojw
    {
        // 
        public class CStream_Server
        {
            public CStream_Server()
            {
            }
            ~CStream_Server()
            {
                Stop();
            }

            private ImageStreamingServer _Server = null;// = new ImageStreamingServer(0);
            //private ImageStreamingServer _Server_Cam = new ImageStreamingServer();
            private DateTime time = DateTime.MinValue;
            public void ChangeImageSource(Image img)
            {
                _Server.ChangeImageSource(img);
            }
            public bool IsRunning() 
            {
                if (_Server == null) return false;
                return _Server.IsRunning; 
            }
            public void Cutting(bool bEn, int nLeft, int nTop, int nWidth, int nHeight)
            {
                if (_Server != null) _Server.Cutting(bEn, nLeft, nTop, nWidth, nHeight);
            }
            public void Start(int nPort) { Start(nPort, 640, 480); }
            public void Start(int nPort, int nWidth, int nHeight)
            {
                if (_Server != null)
                {
                    if (_Server.IsRunning == true)
                    {
                        Stop();
                    }
                }
                _Server = new ImageStreamingServer();
                _Server.SetResolution(nWidth, nHeight);
                //string strAddress = string.Format("http://{0}:{1}", strIpAddress, nPort);//Environment.MachineName);
                
                _Server.Start(nPort);
            }
            public void Start(int nCameraIndex, int nPort) { Start(nCameraIndex, nPort, 640, 480); }
            public void Start(int nCameraIndex, int nPort, int nWidth, int nHeight)
            {
                if (_Server != null) //_Server = new ImageStreamingServer(nCameraIndex);
                {
                    if (_Server.IsRunning == true)
                    {
                        Stop();

                    }
                }
                _Server = new ImageStreamingServer(nCameraIndex, nWidth, nHeight);
                _Server.SetResolution(nWidth, nHeight);
                //string strAddress = string.Format("http://{0}:{1}", strIpAddress, nPort);//Environment.MachineName);

                _Server.Start(nPort);//_Camera(nCameraIndex, nPort);
            }
            public void Start(Control ctrlDisp, int nCameraIndex, int nPort) { Start(ctrlDisp, nCameraIndex, nPort, 640, 480); }
            public void Start(Control ctrlDisp, int nCameraIndex, int nPort, int nWidth, int nHeight)
            {
                if (_Server != null) //_Server = new ImageStreamingServer(nCameraIndex);
                {
                    if (_Server.IsRunning == true)
                    {
                        Stop();

                    }
                }
                _Server = new ImageStreamingServer(ctrlDisp, nCameraIndex);
                _Server.SetResolution(nWidth, nHeight);
                //string strAddress = string.Format("http://{0}:{1}", strIpAddress, nPort);//Environment.MachineName);

                _Server.Start(nPort);//_Camera(nCameraIndex, nPort);
            }

            public void SetImage(Bitmap bmp)
            {
                _Server.SetImage(bmp);
            }
            public void DestroyImage()
            {
                _Server.DestroyImage();
            }

            public Ojw.CCamera GetCamera() { return _Server.GetCamera(); }
            public void Stop()
            {
                if (_Server != null)
                {
                    if (_Server.IsRunning == true)
                    {
                        _Server.Stop();
                    }
                    //_Server = null;
                }
            }
            //private void tmrStreaming_Tick(object sender, EventArgs e)
            //{
            //    int count = (_Server.Clients != null) ? _Server.Clients.Count() : 0;

            //    this.sts.Text = "Clients: " + count.ToString();
            //}
        }

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
            // strAddress => "http://127.0.0.1:8080/?action=stream";
            public bool Start_Jpeg(Control ctrlDisp, string strAddress, int nX, int nY, int nWidth, int nHeight)
            {
                try
                {
                    if (IsReady == false) return false;
                    if (IsStreaming == false) return Open(ctrlDisp, new JPEGStream(strAddress), nX, nY, nWidth, nHeight);
                    return false;
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.Message);
                    return false;
                }
            }
            // strAddress => "http://127.0.0.1:8080/?action=stream";
            public bool Start_Jpeg(Control ctrlDisp, string strAddress, int nWidth, int nHeight) { return Start_Jpeg(ctrlDisp, strAddress, 0, 0, nWidth, nHeight); }
            public bool Start_MJpeg(Control ctrlDisp, string strAddress, int nX, int nY, int nWidth, int nHeight)
            {
                try
                {
                    if (IsReady == false) return false;
                    if (IsStreaming == false) return Open(ctrlDisp, new MJPEGStream(strAddress), nX, nY, nWidth, nHeight);
                    return false;
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error(ex.Message);
                    return false;
                }
            }
            public bool Start_MJpeg(Control ctrlDisp, string strAddress, int nWidth, int nHeight) { return Start_MJpeg(ctrlDisp, strAddress, 0, 0, nWidth, nHeight); }
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

            public Bitmap GetImage_FromStream() { return (IsStreaming) ? m_afgPlayer.GetCurrentVideoFrame() : null; }
            public bool SaveImage_FromStream(String strFileName) { try { Bitmap bmp = GetImage_FromStream(); bmp.Save(strFileName, System.Drawing.Imaging.ImageFormat.Bmp); if (bmp == null) return false; else return true; } catch { return false; } }
        }
    }
}

namespace rtaNetworking.Streaming
{

    /// <summary>
    /// Provides a streaming server that can be used to stream any images source
    /// to any client.
    /// </summary>
    public class ImageStreamingServer : IDisposable
    {
        private static int m_nResolution_Width = 640;
        private static int m_nResolution_Height = 480;



        private bool m_bCutting = false;
        private int m_nCutting_Left = -1;
        private int m_nCutting_Top = -1;
        private int m_nCutting_Width = -1;
        private int m_nCutting_Height = -1;
        public void Cutting(bool bEn, int nLeft, int nTop, int nWidth, int nHeight)
        {
            m_bCutting = bEn;
            if (bEn == false)
            {
                m_nCutting_Left = -1;
                m_nCutting_Top = -1;
                m_nCutting_Width = -1;
                m_nCutting_Height = -1;
            }
            else
            {
                m_nCutting_Left = nLeft;
                m_nCutting_Top = nTop;
                m_nCutting_Width = nWidth;
                m_nCutting_Height = nHeight;
            }
            Screen.ScreenCutting(bEn, nLeft, nTop, nWidth, nHeight);
        }


        private List<Socket> _Clients;
        private Thread _Thread;
        public void SetImage(Bitmap bmp)
        {
            Screen.SetImage(bmp);
        }
        public void DestroyImage()
        {
            Screen.DestroyImage();
        }
        public void SetResolution(int nWidth, int nHeight)
        {
            m_nResolution_Width = nWidth;
            m_nResolution_Height = nHeight;
        }
        public void GetResolution(out int nWidth, out int nHeight)
        {
            nWidth = m_nResolution_Width;
            nHeight = m_nResolution_Height;
        }
        public ImageStreamingServer()
            : this(Screen.Snapshots(m_nResolution_Width, m_nResolution_Height, true))
        {
            m_bCam = false;
        }
        public Ojw.CCamera m_CCam = new Ojw.CCamera();
        public ImageStreamingServer(int nCameraIndex)
        {
            m_bCam = true;
            m_CCam.Init(null, nCameraIndex);
            
            _Clients = new List<Socket>();
            _Thread = null;
            this.ImagesSource = rara();
            this.Interval = 50;
        }
        public ImageStreamingServer(Control ctrlDisp, int nCameraIndex)
        {
            m_bCam = true;
            m_CCam.Init(ctrlDisp, nCameraIndex);

            _Clients = new List<Socket>();
            _Thread = null;
            this.ImagesSource = rara();
            this.Interval = 50;
        }
        public ImageStreamingServer(int nCameraIndex, int nWidth, int nHeight)
        {
            m_nResolution_Width = nWidth;
            m_nResolution_Height = nHeight;

            m_bCam = true;
            m_CCam.Init(null, nCameraIndex, nWidth, nHeight);

            _Clients = new List<Socket>();
            _Thread = null;
            this.ImagesSource = rara();
            this.Interval = 50;
        }
        public ImageStreamingServer(Control ctrlDisp, int nCameraIndex, int nWidth, int nHeight)
        {
            m_nResolution_Width = nWidth;
            m_nResolution_Height = nHeight;

            m_bCam = true;
            m_CCam.Init(ctrlDisp, nCameraIndex, nWidth, nHeight);

            _Clients = new List<Socket>();
            _Thread = null;
            this.ImagesSource = rara();
            this.Interval = 50;
        }
        public ImageStreamingServer(int nWidth, int nHeight)
            : this(Screen.Snapshots(nWidth, nHeight, true))
        {
            m_nResolution_Width = nWidth;
            m_nResolution_Height = nHeight;
            m_bCam = false;
        }
        private bool m_bCam = false;
        private IEnumerable<System.Drawing.Image> rara()
        {
            // Your code must be here
            while(m_bExit == false){
                yield return m_CCam.Grab();
                /*if (m_CCam != null)
                {
                    if (m_CCam.IsRunning() == true)
                        yield return m_CCam.Grab();
                    else yield return null;
                }
                else yield return null;*/
            } 
        }
        public IEnumerable<System.Drawing.Image> ChangeImage(Image img)
        {
            yield return img;
        }
        public void ChangeImageSource(Image img)
        {
            this.ImagesSource = ChangeImage(img);
        }
        public ImageStreamingServer(IEnumerable<Image> imagesSource)
        {
            m_bCam = false;
            _Clients = new List<Socket>();
            _Thread = null;

            this.ImagesSource = imagesSource;
            this.Interval = 50;
        }


        /// <summary>
        /// Gets or sets the source of images that will be streamed to the 
        /// any connected client.
        /// </summary>
        public IEnumerable<Image> ImagesSource { get; set; }

        /// <summary>
        /// Gets or sets the interval in milliseconds (or the delay time) between 
        /// the each image and the other of the stream (the default is . 
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// Gets a collection of client sockets.
        /// </summary>
        public IEnumerable<Socket> Clients { get { return _Clients; } }

        /// <summary>
        /// Returns the status of the server. True means the server is currently 
        /// running and ready to serve any client requests.
        /// </summary>
        public bool IsRunning { get { return (_Thread != null && _Thread.IsAlive); } }

        /// <summary>
        /// Starts the server to accepts any new connections on the specified port.
        /// </summary>
        /// <param name="port"></param>
        public void Start(int nPort)
        {
            lock (this)
            {
                if (m_bCam == true) m_CCam.Start();
                _Thread = new Thread(new ParameterizedThreadStart(ServerThread));
                _Thread.IsBackground = true;
                _Thread.Start(nPort);
            }
        }
        public Ojw.CCamera GetCamera()
        {
            return m_CCam;
        }
        
        /// <summary>
        /// Starts the server to accepts any new connections on the default port (8080).
        /// </summary>
        public void Start()
        {
            this.Start(8081);
        }

        public void Stop()
        {            
            if (this.IsRunning)
            {
                try
                {
                    m_bExit = true;
                    //_Thread.Join();
                    //_Thread.Abort();
                }
                finally
                {

                    lock (_Clients)
                    {

                        foreach (var s in _Clients)
                        {
                            try
                            {
                                s.Close();
                            }
                            catch { }
                        }
                        _Clients.Clear();

                    }

                    if (m_bCam == true)
                    {
                        if (m_CCam != null)
                        {
                            if (m_CCam.IsRunning() == true)
                                m_CCam.Stop();
                        }
                    }
                    Thread.Sleep(100);
                    //Ojw.CTimer.Wait(1000);
                    _Thread = null;
                    
                }
            }
            if (m_Server != null) m_Server.Close();
            m_Server = null;
        }

        /// <summary>
        /// This the main thread of the server that serves all the new 
        /// connections from clients.
        /// </summary>
        /// <param name="state"></param>
        private Socket m_Server;
        private void ServerThread(object state)
        {
            try
            {
                m_Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                m_Server.Bind(new IPEndPoint(IPAddress.Any, (int)state));
                m_Server.Listen(10);

                System.Diagnostics.Debug.WriteLine(string.Format("Server started on port {0}.", state));

                m_bExit = false;
                
#if _USING_DOTNET_2_0 || _USING_DOTNET_3_5
                //ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), m_Server);
                foreach (Socket client in SocketExtensions.IncommingConnectoins(m_Server))
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), client);
#else
                foreach (Socket client in m_Server.IncommingConnectoins())
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), client);
#endif

            }
            catch(Exception ex) 
            {
                Ojw.CMessage.Write_Error("{0}", ex);
            }

            this.Stop();
        }
        //public static IEnumerable<Socket> IncommingConnectoins(this Socket server)
        //{
        //    while (true)
        //        yield return server.Accept();
        //}

        /// <summary>
        /// Each client connection will be served by this thread.
        /// </summary>
        /// <param name="client"></param>
        private bool m_bExit = false;
        private void ClientThread(object client)
        {

            Socket socket = (Socket)client;

            System.Diagnostics.Debug.WriteLine(string.Format("New client from {0}", socket.RemoteEndPoint.ToString()));

            lock (_Clients)
                _Clients.Add(socket);

            try
            {
                using (MjpegWriter wr = new MjpegWriter(new NetworkStream(socket, true)))
                {

                    // Writes the response header to the client.
                    wr.WriteHeader();

                    // Streams the images from the source to the client.
                    foreach (var imgStream in Screen.Streams(this.ImagesSource, m_nResolution_Width, m_nResolution_Height))
                    {
                        if (this.Interval > 0)
                            Thread.Sleep(this.Interval);

                        wr.Write(imgStream);
                        if (m_bExit == true) break;
                    }
                }
            }
            catch { }
            finally
            {
                lock (_Clients)
                    _Clients.Remove(socket);
            }
        }
        


        #region IDisposable Members

        public void Dispose()
        {
            this.Stop();
        }

        #endregion
    }

    static class SocketExtensions
    {

#if _USING_DOTNET_3_5
        public static IEnumerable<Socket> IncommingConnectoins(Socket server)
        {
            while (true)
                yield return server.Accept();
        }
#elif _USING_DOTNET_2_0
        public static IEnumerable<Socket> IncommingConnectoins(Socket server)
        {
            while (true)
                yield return server.Accept();
        }
#else
        public static IEnumerable<Socket> IncommingConnectoins(this Socket server)
        {
            while (true)
                yield return server.Accept();
        }
#endif
    }


    static class Screen
    {


        public static IEnumerable<Image> Snapshots()
        {
            return Screen.Snapshots(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, true);
        }

        /// <summary>
        /// Returns a 
        /// </summary>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        /// 
        public static bool m_bCopyImage = false;
        public static Bitmap m_bmpImage;// = new Bitmap();
        private static int m_nSeq = 0;
        private static int m_nSeq_Back = 0;
        public static void SetImage(Bitmap bmp)
        {
            if (m_nSeq != m_nSeq_Back) 
            {                
                Ojw.CTimer CTmr = new Ojw.CTimer();
                CTmr.Set();
                while (CTmr.Get() < 100)
                {
                    if (m_nSeq == m_nSeq_Back) break;
                    else Thread.Sleep(0);// Application.DoEvents();
                }
                if (CTmr.Get() >= 300) return;
            }
            
            //m_mtxGraphic.WaitOne();
            m_bCopyImage = true;
            m_bmpImage = bmp;//.Clone();
            //m_mtxGraphic.ReleaseMutex();
            m_nSeq++;
        }
        public static void DestroyImage()
        {
            //m_mtxGraphic.WaitOne();
            m_bCopyImage = false;
            m_bmpImage.Dispose();
            //m_mtxGraphic.ReleaseMutex();
        }
        //private static Mutex m_mtxGraphic = new Mutex();
        public static IEnumerable<Image> Snapshots(int width, int height, bool showCursor)
        {
            int nError = 0;
            Size size = new Size(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);

            Bitmap srcImage = new Bitmap(size.Width, size.Height);
            Graphics srcGraphics = Graphics.FromImage(srcImage);

            bool scaled = (width != size.Width || height != size.Height);

            Bitmap dstImage = srcImage;
            Graphics dstGraphics = srcGraphics;

            if (scaled)
            {
                dstImage = new Bitmap(width, height);
                dstGraphics = Graphics.FromImage(dstImage);
            }

            Rectangle src = new Rectangle(0, 0, size.Width, size.Height);
            Rectangle dst = new Rectangle(0, 0, width, height);
            Size curSize = new Size(32, 32);
            //Bitmap bmp;
            while (true)
            {
                try
                {
                    //m_mtxGraphic.WaitOne();
                    if (m_bCopyImage == true)
                    {
#if true
                        //bmp = m_bmpImage;
                        // memory error 발생 - 여기서 - ojw5014

                        if (m_nSeq != m_nSeq_Back)
                        {
                            //if (srcGraphics != null)
                            srcGraphics.DrawImage((Image)m_bmpImage, new Rectangle(0, 0, m_bmpImage.Width, m_bmpImage.Height));
                            m_nSeq_Back = m_nSeq;
                            if (scaled)
                            {
                                dstGraphics.DrawImage(srcImage,
                                    new Rectangle(0, 0, m_bmpImage.Width, m_bmpImage.Height),
                                    new Rectangle(0, 0, m_bmpImage.Width, m_bmpImage.Height),
                                    GraphicsUnit.Pixel);
                            }
                        }
                        //srcGraphics.DrawImage((Image)m_bmpImage, new Point(0, 0));
                        //showCursor = false;
                        //scaled = false;
                        //src = new Rectangle(0, 0, m_bmpImage.Width, m_bmpImage.Height);
                        //dst = new Rectangle(0, 0, m_bmpImage.Width, m_bmpImage.Height);
                        //showCursor = false;
#else
                        Image img = (Image)m_bmpImage;
                        Graphics srcGraphics2 = Graphics.FromImage(img);
                        srcGraphics2.DrawImage(img, 0, 0, img.Width, img.Height);
                        srcGraphics = srcGraphics2;
                        srcGraphics2.Dispose();
                        img.Dispose();
                        showCursor = false;
#endif
                    }
                    else
                    {
                        srcGraphics.CopyFromScreen(0, 0, 0, 0, size);

                        if (showCursor)
                            Cursors.Default.Draw(srcGraphics, new Rectangle(Cursor.Position, curSize));

                        if (scaled)
                            dstGraphics.DrawImage(srcImage, dst, src, GraphicsUnit.Pixel);
                    }
                    //m_mtxGraphic.ReleaseMutex();
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error("Streaming Error : {0}({1})", ex.ToString(), nError++ + 1);
                }
                yield return dstImage;
            }

            srcGraphics.Dispose();
            dstGraphics.Dispose();

            srcImage.Dispose();
            dstImage.Dispose();

            yield return null;//break;
        }

#if _USING_DOTNET_3_5
        internal static IEnumerable<MemoryStream> Streams(IEnumerable<Image> source)
        {
            MemoryStream ms = new MemoryStream();

            foreach (var img in source)
            {
                ms.SetLength(0);
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                yield return ms;
            }

            ms.Close();
            ms = null;

            yield break;
        }
#elif _USING_DOTNET_2_0
        internal static IEnumerable<MemoryStream> Streams(IEnumerable<Image> source)
        {
            MemoryStream ms = new MemoryStream();

            foreach (var img in source)
            {
                ms.SetLength(0);
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                yield return ms;
            }

            ms.Close();
            ms = null;

            yield break;
        }
#else
        public static Bitmap CuttingImage(Image image, int nLeft, int nTop, int nWidth, int nHeight)
        {
            var destRect = new Rectangle(0, 0, nWidth, nHeight);
            var destImage = new Bitmap(nWidth, nHeight);

            //destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                graphics.DrawImage(image, destRect, nLeft, nTop, nWidth, nHeight, GraphicsUnit.Pixel);
            }
            return destImage;
        }
        // source : https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
        public static Bitmap ResizeImage(Image image, int nLeft, int nTop, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, nLeft, nTop, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private static bool m_bScreenCutting = false;
        private static int m_nScreenCutting_Left = -1;
        private static int m_nScreenCutting_Top = -1;
        private static int m_nScreenCutting_Width = -1;
        private static int m_nScreenCutting_Height = -1;
        internal static void ScreenCutting(bool bEn, int nLeft, int nTop, int nWidth, int nHeight)
        {
            m_bScreenCutting = bEn;
            if (bEn == false)
            {
                m_nScreenCutting_Left = -1;
                m_nScreenCutting_Top = -1;
                m_nScreenCutting_Width = -1;
                m_nScreenCutting_Height = -1;
            }
            else
            {
                m_nScreenCutting_Left = nLeft;
                m_nScreenCutting_Top = nTop;
                m_nScreenCutting_Width = nWidth;
                m_nScreenCutting_Height = nHeight;
            }
        }
        internal static IEnumerable<MemoryStream> Streams(this IEnumerable<Image> source, int nWidth, int nHeight)
        {
            MemoryStream ms = new MemoryStream();
            bool bResize = false;
            foreach (var img in source)
            {
                ms.SetLength(0);

                if (m_bScreenCutting)
                {
                    int nX = ((m_nScreenCutting_Left < 0) ? 0 : m_nScreenCutting_Left);
                    int nY = ((m_nScreenCutting_Top < 0) ? 0 : m_nScreenCutting_Top);
                    int nW = ((m_nScreenCutting_Width < 0) ? nWidth : m_nScreenCutting_Width);
                    int nH = ((m_nScreenCutting_Height < 0) ? nHeight : m_nScreenCutting_Height);
                    if (nW > nWidth) nW = nWidth;
                    if (nH > nHeight) nH = nHeight;
                    ((Image)CuttingImage(img, nX, nY, nW, nH)).Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else
                {
                    if ((nWidth > 0) || (nHeight > 0))
                    {
                        if ((nHeight != img.Height) || (nWidth != img.Width))
                        {
                            bResize = true;
                            ((Image)ResizeImage(img, 0, 0, nWidth, nHeight)).Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                    }
                    if (bResize == false) img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                yield return ms;
            }

            ms.Close();
            ms = null;

            yield break;
        }
#endif
    }
}

namespace rtaNetworking.Streaming
{
    /// <summary>
    /// Provides a stream writer that can be used to write images as MJPEG 
    /// or (Motion JPEG) to any stream.
    /// </summary>
    public class MjpegWriter : IDisposable
    {

        private static byte[] CRLF = new byte[] { 13, 10 };
        private static byte[] EmptyLine = new byte[] { 13, 10, 13, 10 };

        private string _Boundary;

        public MjpegWriter(Stream stream)
            : this(stream, "--boundary")
        {

        }

        public MjpegWriter(Stream stream, string boundary)
        {

            this.Stream = stream;
            this.Boundary = boundary;
        }

        public string Boundary { get; private set; }
        public Stream Stream { get; private set; }

        public void WriteHeader()
        {

            Write(
                    "HTTP/1.1 200 OK\r\n" +
                    "Content-Type: multipart/x-mixed-replace; boundary=" +
                    this.Boundary +
                    "\r\n"
                 );

            this.Stream.Flush();
        }

        public void Write(Image image)
        {
            MemoryStream ms = BytesOf(image);
            this.Write(ms);
        }

        public void Write(MemoryStream imageStream)
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine(this.Boundary);
            sb.AppendLine("Content-Type: image/jpeg");
            sb.AppendLine("Content-Length: " + imageStream.Length.ToString());
            sb.AppendLine();

            Write(sb.ToString());
            imageStream.WriteTo(this.Stream);
            Write("\r\n");

            this.Stream.Flush();

        }

        private void Write(byte[] data)
        {
            this.Stream.Write(data, 0, data.Length);
        }

        private void Write(string text)
        {
            byte[] data = BytesOf(text);
            this.Stream.Write(data, 0, data.Length);
        }

        private static byte[] BytesOf(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        private static MemoryStream BytesOf(Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms;
        }

        public string ReadRequest(int length)
        {

            byte[] data = new byte[length];
            int count = this.Stream.Read(data, 0, data.Length);

            if (count != 0)
                return Encoding.ASCII.GetString(data, 0, count);

            return null;
        }

        #region IDisposable Members

        public void Dispose()
        {

            try
            {

                if (this.Stream != null)
                    this.Stream.Dispose();

            }
            finally
            {
                this.Stream = null;
            }
        }

        #endregion
    }
}
