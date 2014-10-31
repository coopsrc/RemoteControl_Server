using Server.Function;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Server.Controller
{
    class ScreenConnection : Connection
    {
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private NetworkStream ns;
        private MemoryStream ms;
        private BinaryReader br;
        private BinaryWriter bw;
        private const int port = 10081;

        //private Thread t;
        private Thread thread;


        private RemoteScreen remoteScreen = new RemoteScreen();

        Thread sendThread;

        public void Start()
        {
            tcpListener = new TcpListener(Broadcast.LocalAddress, port);
            tcpListener.Start();
            thread = new Thread(Connect);
            thread.IsBackground = true;
            thread.Start();
        }

        private void Connect()
        {
            tcpClient = tcpListener.AcceptTcpClient();
            ns = tcpClient.GetStream();
            // ms = new MemoryStream();
            br = new BinaryReader(ns);
            bw = new BinaryWriter(ns);

            Receive();
        }

        private void Receive()
        {
            string recStr = null;
            while (true)
            {
                try
                {
                    recStr = br.ReadString();
                }
                catch
                {
                    MessageBox.Show("接受数据失败");
                }
                string[] splitStr = recStr.Split(',');
                Console.WriteLine(splitStr[0]);
                switch (splitStr[0])
                {
                    case "desktop":
                        Send_Image();
                        break;
                    case "pause":
                        ScreenPause();
                        break;
                    case "mouse":
                        remoteScreen.Mouse_Event(splitStr[1], splitStr[2], splitStr[3]);
                        break;
                    case "keyboard":
                        remoteScreen.Keyboard_Event(splitStr[1], splitStr[2]);
                        break;
                    case "talkbox":
                        remoteScreen.showBox(splitStr);
                        break;
                    case "close":
                        Disconnect();
                        break;
                }
            }
        }

        private void ScreenPause()
        {
            sendThread.Abort();
        }

        private void Send(Object obj) { }

        public void Disconnect()
        {

            try
            {
                sendThread.Abort();
                thread.Abort();

            }
            catch { }
            finally
            {
                br.Close();
                bw.Close();
                ms.Close();
                ns.Close();
                tcpClient.Close();
                tcpListener.Stop();
            }
        }


        private void Send_Image()
        {
            sendThread = new Thread(Send_One_Image);
            sendThread.IsBackground = true;
            sendThread.Start();
        }


        private void Send_One_Image()
        {
            Bitmap desktop;
            byte[] arrImage;
            Graphics g;
            int i = 0;
            while (true)
            {
                i++;
                ms = new MemoryStream();
                desktop = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                g = System.Drawing.Graphics.FromImage(desktop);
                g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);


                desktop.Save(ms, ImageFormat.Png);

                arrImage = ms.GetBuffer();
                Console.WriteLine(arrImage.Length);
                bw.Write(arrImage.Length);
                bw.Write(arrImage);

                // bw.Flush();
                desktop.Dispose();
                arrImage = null;
                Thread.Sleep(300);
            }
        }

    }
}
