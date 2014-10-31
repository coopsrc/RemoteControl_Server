using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Server.Function;

namespace Server.Controller
{
    public class Broadcast
    {
        private Thread thread;
        private Socket socket;
        private IPEndPoint ipEndPoint;
        private string message;

        private const int port = 1008;
        public static IPAddress LocalAddress;

        public Broadcast()
        {
            IPAddress[] address = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress item in address)
            {
                if (item.AddressFamily == AddressFamily.InterNetwork)
                {
                    LocalAddress = item;
                }
            }

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        }

        public void Start()
        {
            thread = new Thread(Connect);
            thread.IsBackground = true;
            thread.Start();
        }

        private void Connect()
        {
            //格式：主机IP地址，所在地区，主机名，用户名，操作系统，cpu，内存，硬盘，显示器，视频，状态（连接、断开）
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(LocalAddress + ",");
            stringBuilder.Append(HostInfo.Location() + ",");
            stringBuilder.Append(HostInfo.HostName() + ",");
            stringBuilder.Append(HostInfo.UserName() + ",");
            stringBuilder.Append(HostInfo.OSName() + ",");
            stringBuilder.Append(HostInfo.CPUType() + ",");
            stringBuilder.Append(HostInfo.MemorySize() + ",");
            stringBuilder.Append(HostInfo.HardDiskSize() + ",");
            stringBuilder.Append(HostInfo.ScreenSize() + ",");
            stringBuilder.Append(HostInfo.VideoStatus() + ",");
            stringBuilder.Append(HostInfo.ConnectStatus() + ",");

            message = stringBuilder.ToString();

            byte[] data = Encoding.Unicode.GetBytes(message);

            while (true)
            {
                socket.SendTo(data, ipEndPoint);

                //每五秒钟向局域网广播一次
                Thread.Sleep(5000);
            }
        }

        public void Disconnect()
        {
         

            //格式：主机IP地址，所在地区，主机名，用户名，操作系统，cpu，内存，硬盘，显示器，视频，状态
            StringBuilder mesBuilder = new StringBuilder();
            mesBuilder.Append(LocalAddress + ",");
            mesBuilder.Append(HostInfo.Location() + ",");
            mesBuilder.Append(HostInfo.HostName() + ",");
            mesBuilder.Append(HostInfo.UserName() + ",");
            mesBuilder.Append(HostInfo.OSName() + ",");
            mesBuilder.Append(HostInfo.CPUType() + ",");
            mesBuilder.Append(HostInfo.MemorySize() + ",");
            mesBuilder.Append(HostInfo.HardDiskSize() + ",");
            mesBuilder.Append(HostInfo.ScreenSize() + ",");
            mesBuilder.Append(HostInfo.VideoStatus() + ",");
            mesBuilder.Append("断开,");

            message = mesBuilder.ToString();

            byte[] data = Encoding.Unicode.GetBytes(message);
            socket.SendTo(data, ipEndPoint);



            thread.Abort();
            socket.Close();
        }


    }
}
