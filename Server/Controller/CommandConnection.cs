using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Server.Function;

namespace Server.Controller
{
    public class CommandConnection : Connection
    {
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private Thread thread;
        private const int port = 10086;

        /// <summary>
        /// 开始Command
        /// </summary>
        public void Start()
        {
            thread = new Thread(Connect);
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 连接Command
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

            Receive();

        }

        /// <summary>
        /// 接收来自客户端的Command命令
        /// </summary>
        private void Receive()
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            StringBuilder message = new StringBuilder();

            while (true)
            {
                lock (networkStream)
                {
                    bytesRead = networkStream.Read(buffer, 0, 1024);
                    message.Clear();
                    message.AppendFormat("{0}", Encoding.Unicode.GetString(buffer, 0, bytesRead));
                }

                if (message != null)
                {
                    Console.WriteLine(message);

                    if (message.ToString().Equals("Connect"))
                    {
                        Console.WriteLine("************************");
                        Console.WriteLine("*****Command Connect****");
                        Console.WriteLine("************************");
                    }
                    else if (message.ToString().Equals("Closed"))
                    {
                        Console.WriteLine("************************");
                        Console.WriteLine("*****Command Closed*****");
                        Console.WriteLine("************************");
                    }
                    else
                    {

                        string str = Command.RunCommand(message.ToString());

                        Send(str);
                    }
                }
            }
        }


        /// <summary>
        /// 向客户端返回信息
        /// </summary>
        /// <param name="obj">返回信息的内容</param>
        private void Send(Object obj)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(obj.ToString());
            try
            {
                int length = buffer.Length;
                int left = length;
                int write = 0;
                int bufferSize = tcpClient.SendBufferSize;
                while (left > 0)
                {
                    if (left > length)
                    {
                        networkStream.Write(buffer, write, bufferSize);
                    }
                    else
                    {
                        networkStream.Write(buffer, write, left);
                    }
                    write += bufferSize;
                    left -= bufferSize;

                }
                //lock (networkStream)
                //{
                //    networkStream.Write(buffer, 0, buffer.Length);
                //}
            }
            catch
            {
                //发送失败
            }
        }

        /// <summary>
        /// 断开Command连接
        /// </summary>
        public void Disconnect()
        {
            thread.Abort();
            networkStream.Close();
            tcpClient.Close();
            tcpClient.Client.Close();
            tcpListener.Stop();
        }
    }
}
