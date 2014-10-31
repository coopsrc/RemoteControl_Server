
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using Server.Controller;

namespace Server.Function
{

    /// <summary>
    ///对本机的分区信息的处理相关方法-------by CY
    /// </summary>
    static class DrivAndDireInfo
    {
        //获取分区、文件目录信息
        private static TcpListener listener_CreatTree;
        private static TcpClient client_CreateTree;
        private static NetworkStream netStream_CreateTree;
        private const int port_CreatTree = 10083;



        private static string[] drives;

        static TreeNode nodeDir;

        /// <summary>
        /// 打开用于创建文件信息树（结点）的连接
        /// </summary>
        public static void StartConnection_Creation()
        {
            listener_CreatTree = new TcpListener(Broadcast.LocalAddress, port_CreatTree);
            listener_CreatTree.Start();
            int tryTimes = 0;
            while (tryTimes <= 50)
            {
                try
                {
                    client_CreateTree = listener_CreatTree.AcceptTcpClient();
                    ControlConnection.isFileManageConnected = true;
                    break;
                }
                catch
                {
                    tryTimes++;
                    continue;
                }
            }

            netStream_CreateTree = client_CreateTree.GetStream();
        }

        /// <summary>
        /// 获取目录或文件的大小，创建和最后一次修改时间
        /// </summary>
        /// <param name="path">文件或目录的路径</param>
        /// <returns></returns>
        private static string GetOtherFileInfo(string path)
        {
            StringBuilder infoBuilder = new StringBuilder();
            infoBuilder.Append(",");
            //大小
            try
            {
                FileInfo fi = new FileInfo(path);
                long size = fi.Length;
                infoBuilder.Append(size.ToString());
            }
            catch
            {
                infoBuilder.Append(" ,");
            }
            infoBuilder.Append(",");
            //创建时间
            try
            {
                infoBuilder.Append(File.GetCreationTime(path).ToString());
            }
            catch
            {
                infoBuilder.Append("获取创建时间失败");
            }
            infoBuilder.Append(",");
            //修改时间
            try
            {
                infoBuilder.Append(File.GetLastWriteTime(path).ToString());
            }
            catch
            {
                infoBuilder.Append("获取修改时间失败");
            }

            return infoBuilder.ToString();
        }
        /// <summary>
        /// 获取本机的分区和文件夹信息
        /// </summary>
        public static TreeNode GetDrivesInfo(string dirPath)
        {
            drives = Directory.GetLogicalDrives();

            if (dirPath != Dns.GetHostName())
            {
                nodeDir = new TreeNode(dirPath);
                nodeDir.Tag = dirPath;
                if (File.Exists(dirPath))
                {
                    nodeDir = null;
                }
                else //if (Directory.Exists(dirPath))
                {
                    try
                    {
                        foreach (string dirName in Directory.GetDirectories((string)nodeDir.Text))
                        {
                            TreeNode temp = new TreeNode(dirName);
                            temp.Text = dirName;
                            temp.Tag = "directory" + GetOtherFileInfo(dirName);
                            nodeDir.Nodes.Add(temp);
                        }
                        foreach (string fileName in Directory.GetFiles((string)nodeDir.Tag))
                        {
                            TreeNode temp = new TreeNode(fileName);
                            temp.Text = fileName;
                            temp.Tag = "file" + GetOtherFileInfo(fileName);
                            nodeDir.Nodes.Add(temp);
                        }
                    }
                    catch
                    {
                        nodeDir.Tag = "ReadOnly";
                    }
                }
            }
            else
            {
                nodeDir = new TreeNode();
                nodeDir.Tag = Dns.GetHostName();
                nodeDir.Text = Dns.GetHostName();
                foreach (string drivName in Directory.GetLogicalDrives())
                {
                    TreeNode temp = new TreeNode(drivName);
                    temp.Tag = Dns.GetHostName();
                    nodeDir.Nodes.Add(temp);
                }
            }
            return nodeDir;
        }
        /// <summary>
        /// 序列化包含文件目录信息的结点
        /// </summary>
        /// <returns>返回序列化后的byte数组</returns>
        private static byte[] Serialize()
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, nodeDir);
            byte[] temp = ms.ToArray();
            ms.Close();
            return temp;
        }
        /// <summary>
        /// 向客户端发送序列化后的包含文件目录信息的结点
        /// </summary>
        public static void SendDrivesInfo()
        {
            byte[] serializedNode = Serialize();
            int left = serializedNode.Length;
            int bufferSize = client_CreateTree.SendBufferSize;
            int startIndex = 0;
            while (left > 0)
            {
                if (left > bufferSize)
                {
                    netStream_CreateTree.Write(serializedNode, startIndex, bufferSize);
                }
                else
                {
                    netStream_CreateTree.Write(serializedNode, startIndex, left);
                }
                left -= bufferSize;
                startIndex += bufferSize;
            }

        }
        /// <summary>
        /// 接收要查找的目录路径信息并返回查找到的信息
        /// </summary>
        public static void Receive()
        {
            string node_Text = null;
            int bufferSize = client_CreateTree.ReceiveBufferSize;
            //char[] buffer = new char[client_CreateTree.ReceiveBufferSize];
            byte[] buffer = new byte[client_CreateTree.ReceiveBufferSize];

            //StreamReader sr = new StreamReader(netStream_CreateTree);
            while (true)
            {
                //int read = sr.Read(buffer, 0, bufferSize);
                int read = netStream_CreateTree.Read(buffer, 0, bufferSize);
                node_Text += Encoding.Unicode.GetString(buffer, 0, read);

                if (read < bufferSize)
                {
                    if (GetDrivesInfo(node_Text) != null)
                    {
                        SendDrivesInfo();
                        node_Text = null;
                    }
                }
            }
        }
    }
}
