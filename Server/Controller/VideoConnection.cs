using Server.Function;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Controller
{
    class VideoConnection
    {
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private BinaryWriter bw;
        private MemoryStream ms;
        private Thread thread;
        private const int port = 10082;

        private bool Capturing = false;

        const int VIDEODEVICE = 0; // 使用第一个视频设备
        const int VIDEOWIDTH = 640; // 捕捉视频的宽度
        const int VIDEOHEIGHT = 480; // 捕捉视频的高度
        const int VIDEOBITSPERPIXEL = 24; //视频分辨率

        Capture camera = null;
        IntPtr m_ip = IntPtr.Zero;

        private int FPS = 25;

        public VideoConnection()
        {

        }

        /// <summary>
        /// 开始Video
        /// </summary>
        public void Start()
        {
            thread = new Thread(Connect);
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 连接Video
        /// </summary>
        private void Connect()
        {
            tcpListener = new TcpListener(Broadcast.LocalAddress, port);
            tcpListener.Start();

            try
            {
                tcpClient = tcpListener.AcceptTcpClient();
            }
            catch
            {

            }

            networkStream = tcpClient.GetStream();
            bw = new BinaryWriter(networkStream);

            Receive();

        }

        /// <summary>
        /// 接收来自客户端的Video命令
        /// </summary>
        private void Receive()
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            StringBuilder message = new StringBuilder();


            Thread captureThread = null;

            while (true)
            {
                lock (networkStream)
                {
                    bytesRead = networkStream.Read(buffer, 0, 1024);
                    message.Clear();
                    message.AppendFormat("{0}", Encoding.Unicode.GetString(buffer, 0, bytesRead));
                }

                if (message.Length > 0)
                {
                    if (message.ToString().Equals("Connect"))
                    {
                        Console.WriteLine("************************");
                        Console.WriteLine("******Video Connect*****");
                        Console.WriteLine("************************");
                    }
                    else if (message.ToString().Equals("Open"))
                    {
                        captureThread = new Thread(StartCapture);
                        captureThread.IsBackground = true;
                        captureThread.Start();

                    }
                    else if (message.ToString().Equals("Pause"))
                    {
                        try
                        {
                            captureThread.Abort();
                        }
                        catch
                        {
                            throw;
                        }

                        StopCapture();
                    }
                    else if (message.ToString().Equals("Close"))
                    {
                        Console.WriteLine("************************");
                        Console.WriteLine("******Video Closedt*****");
                        Console.WriteLine("************************");
                    }
                    else if (message.ToString().Contains("VideoRate:"))
                    {
                        //设置FPS
                        FPS = Int32.Parse(message.ToString().Split(':')[1]);
                    }
                    else
                    {
                        Console.WriteLine(message);
                    }
                }
            }
        }

        private void StopCapture()
        {
            Capturing = false;
            if (camera != null)
            {
                camera.Dispose();
            }

            if (m_ip != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(m_ip);
                m_ip = IntPtr.Zero;
            }
        }

        private void StartCapture()
        {
            Capturing = true;

            #region 创建一个虚拟的Form
            Form form = new Form();
            form.Width = 656;
            form.Height = 519;
            PictureBox pictureBox = new PictureBox();
            form.Controls.Add(pictureBox);
            pictureBox.Dock = DockStyle.Fill;
            #endregion

            camera = new Capture(VIDEODEVICE, VIDEOWIDTH, VIDEOHEIGHT, VIDEOBITSPERPIXEL, pictureBox);
            Bitmap image;

            while (Capturing)
            {
                Cursor.Current = Cursors.WaitCursor;

                // 释放之前的缓冲区
                if (m_ip != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(m_ip);
                    m_ip = IntPtr.Zero;
                }

                //捕捉图像
                m_ip = camera.Click();
                image = new Bitmap(camera.Width, camera.Height, camera.Stride, PixelFormat.Format24bppRgb, m_ip);

                //如果图像上下颠倒，调整过来
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);


                //发图片送到客户端

                Send(image);
                image.Dispose();

                Thread.Sleep(1000 / FPS);

                Cursor.Current = Cursors.Default;
            }
        }


        /// <summary>
        /// 将图像发送到客户端
        /// </summary>
        /// <param name="obj">图片对象</param>
        private void Send(Object obj)
        {
            Image image = (Image)obj;

            ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);

            byte[] arrImage = ms.GetBuffer();
            try
            {
                bw.Write(arrImage.Length);
                bw.Write(arrImage);
            }
            catch
            {

            }

            // bw.Flush();
            image.Dispose();
            arrImage = null;
        }

        /// <summary>
        /// 断开Video连接
        /// </summary>
        public void Disconnect()
        {
            thread.Abort();
            networkStream.Close();
            try
            {
                bw.Close();
                ms.Close();
            }
            catch
            {

            }

            tcpClient.Close();
            tcpClient.Client.Close();
            tcpListener.Stop();
        }

    }
}
