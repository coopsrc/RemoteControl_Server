using Server.Function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Controller
{
    public class ControlConnection : Connection
    {
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private Thread thread;
        private const int port = 10080;

        private CommandConnection commandConnection;
        private VideoConnection videoConnection;
        private ScreenConnection screenConnection;

        public static bool isFileManageConnected = false;

        public void Start()
        {
            thread = new Thread(Connect);
            thread.IsBackground = true;
            thread.Start();
        }

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

        private void Receive()
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            StringBuilder message = new StringBuilder();


            while (true)
            {
                try
                {
	                bytesRead = networkStream.Read(buffer, 0, 1024);
	                message.Clear();
	                message.AppendFormat("{0}", Encoding.Unicode.GetString(buffer, 0, bytesRead));
                }
                catch
                {
                	
                }

                if (message.Length > 0)
                {
                    Console.WriteLine(message);
                    switch (message.ToString())
                    {
                        case "Connect":
                            break;
                        case "Disconnect":
                            this.Disconnect();
                            break;
                        case "Command":
                            commandConnection = new CommandConnection();
                            commandConnection.Start();
                            break;
                        case "Command Closed":
                            commandConnection.Disconnect();
                            break;
                        case "Video":
                            videoConnection = new VideoConnection();
                            videoConnection.Start();
                            break;
                        case "Video Closed":
                            videoConnection.Disconnect();
                            break;
                        case "Screen":
                            screenConnection = new ScreenConnection();
                            screenConnection.Start();
                            break;
                        case "Screen Closed":
                            screenConnection.Disconnect();
                            break;
                        case "FileManage":
                            if (!isFileManageConnected)
                            {
                                DrivAndDireInfo.StartConnection_Creation();
                                DrivAndDireInfo.GetDrivesInfo(Dns.GetHostName());
                                DrivAndDireInfo.SendDrivesInfo();
                                FileOperation.StartFileConnection_Operation();
                                Thread receiveThread = new Thread(DrivAndDireInfo.Receive);
                                Thread receiveOpCmdThread = new Thread(FileOperation.ReceiveOperationCommand);
                                //receiveThread.IsBackground = true;
                                receiveThread.Start();
                                receiveOpCmdThread.Start();
                            }
                            break;
                        case "FileManage Closed":
                            break;
                        case "Chat":
                            ChatForm chatForm = new ChatForm();
                            chatForm.SetConnect();
                            chatForm.ShowDialog();
                            break;
                        case "Chat Closed":
                            break;
                        case "PushScreen":
                            PushScreenForm pushForm = new PushScreenForm();
                            pushForm.SetConnect();
                            pushForm.ShowDialog();
                            break;
                        case "PushScreen Closed":
                            //form.Disconnect();
                            break;
                    }
                }
            }
        }

        private void Send(Object obj)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(obj.ToString());
            try
            {
                lock (networkStream)
                {
                    networkStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch
            {
                //发送失败
            }
        }

        public void Disconnect()
        {

            //thread.Abort();
            try
            {
                networkStream.Close();
                tcpClient.Close();
                tcpClient.Client.Close();
                tcpListener.Stop();
            }
            catch
            {

            }

        }
    }
}
