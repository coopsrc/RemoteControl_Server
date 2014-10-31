using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Server.Controller;

namespace Server.Function
{
    static class FileOperation
    {
        //文件操作
        private static TcpListener listener_FileOperation;
        private static TcpClient client_FileOperation;
        private static NetworkStream netStream_FileOperation;
        private const int port_FileOperation = 20083;

        private static string paste_oldPath = "";

        private static StreamReader sr;
        private static StreamWriter sw;

        //文件传输专用
        private static TcpListener listener_FileTransport;
        private static TcpClient client_FileTransport;
        private static NetworkStream netStream_FileTransport;
        private const int port_FileTransport = 20084;
        private static bool isFileTransportStarted = false;

        /// <summary>
        /// 打开用于文件操作命令的链接
        /// </summary>
        public static void StartFileConnection_Operation()
        {
            listener_FileOperation = new TcpListener(Broadcast.LocalAddress, port_FileOperation);
            listener_FileOperation.Start();
            int tryTimes = 0;
            while (tryTimes <= 50)
            {
                try
                {
                    client_FileOperation = listener_FileOperation.AcceptTcpClient();
                    break;
                }
                catch
                {
                    tryTimes++;
                    continue;
                }
            }
            netStream_FileOperation = client_FileOperation.GetStream();
            sw = new StreamWriter(netStream_FileOperation);
            sr = new StreamReader(netStream_FileOperation);
        }
        /// <summary>
        /// 接收文件操作命令
        /// </summary>
        public static void ReceiveOperationCommand()
        {
            string msg = "";
            //byte[] b = new byte[1024];
            while (true)
            {
                //netStream_FileOperation.Read(b, 0, 1024);
                msg = sr.ReadLine();
                string[] splitStr = msg.Split(',');
                if (splitStr[0] == "command")
                    switch (splitStr[1])
                    {
                        //msg格式：command，Delete，路径
                        case "Delete":
                            if (Delete(splitStr[2]))
                            {
                                sw.WriteLine("success");
                                sw.Flush();
                            }
                            else
                            {
                                sw.WriteLine("fail");
                                sw.Flush();
                            }
                            break;
                        //msg格式：command,ReName,所在文件夹路径,原文件名,新文件名
                        case "ReName":
                            string result = ReName(splitStr[2], splitStr[3], splitStr[4]);
                            sw.WriteLine(result);
                            sw.Flush();
                            break;
                        //msg格式：command,download,路径
                        case "Download":
                            if (!isFileTransportStarted)
                            {
                                try
                                {
                                    listener_FileTransport = new TcpListener(Broadcast.LocalAddress, port_FileTransport);
                                    listener_FileTransport.Start();
                                    isFileTransportStarted = true;
                                }
                                catch
                                {
                                    //已启动lisener
                                }
                            }
                            StartFileTransportConnection(splitStr[2]);
                            break;
                        //msg格式：command,Upload,文件大小
                        case "Upload":
                            if (!isFileTransportStarted)
                            {
                                try
                                {
                                    listener_FileTransport = new TcpListener(Broadcast.LocalAddress, port_FileTransport);
                                    listener_FileTransport.Start();
                                    isFileTransportStarted = true;
                                }
                                catch
                                {
                                    //已启动lisener
                                }
                            }
                            client_FileTransport = listener_FileTransport.AcceptTcpClient();
                            netStream_FileTransport = client_FileTransport.GetStream();
                            Thread t = new Thread(ReceiveFile);
                            t.Start(splitStr[2] + "," + splitStr[3]);
                            break;
                        //msg格式：command,Cut,被剪切文件路径
                        case "Cut":
                            if (File.Exists(splitStr[2]) || Directory.Exists(splitStr[2]))
                            {
                                paste_oldPath = splitStr[2];
                                sw.WriteLine("Ready");
                                sw.Flush();
                            }
                            break;
                        //msg格式：command,Paste,粘贴文件路径
                        case "Paste":
                            sw.WriteLine(CutPaste(paste_oldPath, splitStr[2]));
                            sw.Flush();
                            break;
                        //msg格式：command,NewFile,Type,目录路径，文件名
                        case "NewFile":
                            if (splitStr[2] == "txt")
                            {
                                string fileName = splitStr[4];
                                int count = 1;
                                string filePath = "";
                                while (true)
                                {
                                    filePath = splitStr[3] + "\\" + fileName + ".txt";
                                    if (File.Exists(filePath))
                                    {
                                        fileName = fileName + "(" + count + ")";
                                        count++;
                                        continue;
                                    }
                                    else
                                        break;
                                }
                                try
                                {
                                    FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                                    fs.Close();
                                    sw.WriteLine("success"); sw.Flush();
                                }
                                catch { sw.WriteLine("fail"); sw.Flush(); }

                            }
                            else if (splitStr[2] == "doc")
                            {
                                string fileName = splitStr[4];
                                int count = 1;
                                string filePath = "";
                                while (true)
                                {
                                    filePath = splitStr[3] + "\\" + fileName + ".doc";
                                    if (File.Exists(filePath))
                                    {
                                        fileName = fileName + "(" + count + ")";
                                        count++;
                                        continue;
                                    }
                                    else
                                        break;
                                }
                                try
                                {
                                    FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                                    fs.Close();
                                    sw.WriteLine("success"); sw.Flush();
                                }
                                catch { sw.WriteLine("fail"); sw.Flush(); }
                            }
                            else if (splitStr[2] == "directory")
                            {
                                string DirName = splitStr[4];
                                int count = 1;
                                string DirPath = "";
                                while (true)
                                {
                                    DirPath = splitStr[3] + "\\" + DirName;
                                    if (Directory.Exists(DirPath))
                                    {
                                        DirName = DirName + "(" + count + ")";
                                        count++;
                                        continue;
                                    }
                                    else
                                        break;
                                }
                                try
                                {
                                    Directory.CreateDirectory(DirPath);
                                    sw.WriteLine("success"); sw.Flush();
                                }
                                catch { sw.WriteLine("fail"); sw.Flush(); }
                            }
                            break;
                    }
                else
                {
                    Console.WriteLine(".....");
                }
            }


        }

        /// <summary>
        /// 删除目录或文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>是否删除成功</returns>
        private static bool Delete(string path)
        {
            bool deleted = false;
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    deleted = true;
                }
                catch
                {
                    deleted = false;
                }

            }
            else if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path);
                    deleted = true;
                }
                catch
                {
                    deleted = false;
                }
            }
            return deleted;
        }

        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="dirPath">文件所在目录路径</param>
        /// <param name="oldName">原文件名</param>
        /// <param name="newName">新文件名</param>
        /// <returns>是否重命名成功</returns>
        private static string ReName(string dirPath, string oldName, string newName)
        {
            string path = dirPath + "\\" + oldName;
            string rename_result = "fail,重命名失败";
            if (File.Exists(path))
            {
                string newPath = dirPath + "\\" + newName;
                try
                {
                    if (!File.Exists(newPath))
                    {
                        FileInfo fi = new FileInfo(path);
                        int len = (int)fi.Length;
                        byte[] buffer = new byte[len];
                        FileStream fs_r = new FileStream(path, FileMode.Open, FileAccess.Read);
                        fs_r.Read(buffer, 0, len);
                        fs_r.Close();
                        FileStream fs_w = new FileStream(newPath, FileMode.Create, FileAccess.Write);
                        fs_w.Write(buffer, 0, buffer.Length);
                        fs_w.Close();

                        File.Delete(path);
                        rename_result = "success";
                    }
                    else
                    {
                        rename_result = "fail,该文件已存在！";
                    }
                }
                catch
                {
                    rename_result = "fail,重命名失败！";
                }
            }

            if (Directory.Exists(path))
            {
                string newPath = dirPath + "\\" + newName;
                try
                {
                    if (!Directory.Exists(newPath))
                    {
                        //Directory.CreateDirectory(newPath);
                        Directory.Move(path, newPath);

                        //Directory.Delete(path);
                        rename_result = "success";
                    }
                    else
                    {
                        rename_result = "fail,该文件夹已存在！";
                    }
                }
                catch
                {
                    rename_result = "fail,重命名失败！";
                }
            }
            return rename_result;
        }

        /// <summary>
        /// 剪切-粘贴
        /// </summary>
        /// <param name="oldPath">剪切路径</param>
        /// <param name="newPath">粘贴路径</param>
        /// <returns></returns>
        private static string CutPaste(string oldPath, string newPath)
        {
            string result = "";
            if (!File.Exists(newPath) && !Directory.Exists(newPath))
            {
                if (File.Exists(oldPath)) //如果要剪切的路径是一个文件
                {
                    FileInfo fi = new FileInfo(oldPath);
                    int len = (int)fi.Length;
                    byte[] buffer = new byte[len];
                    FileStream fs_r = new FileStream(oldPath, FileMode.Open, FileAccess.Read);
                    fs_r.Read(buffer, 0, len);
                    fs_r.Close();
                    FileStream fs_w = new FileStream(newPath, FileMode.Create, FileAccess.Write);
                    fs_w.Write(buffer, 0, buffer.Length);
                    fs_w.Close();
                    try
                    {
                        File.Delete(oldPath);
                        result = "success";
                    }
                    catch
                    {
                        File.Delete(newPath);
                        result = "fail,该文件不能被移动";
                    }
                }
                else if (Directory.Exists(oldPath))//如果要剪切的路径是一个目录
                {
                    try
                    {
                        Directory.Move(oldPath, newPath);
                        result = "success";
                    }
                    catch
                    {
                        result = "fail,该文件不能被移动";
                    }
                }
            }
            else
            {
                result = "fail,该目录下已经存在同名文件";
            }
            return result;
        }

        /// <summary>
        /// 打开文件传输连接
        /// </summary>
        /// <param name="obj"></param>
        private static void StartFileTransportConnection(string path)
        {
            while (true)
            {
                try
                {
                    client_FileTransport = listener_FileTransport.AcceptTcpClient();
                    netStream_FileTransport = client_FileTransport.GetStream();
                    Thread t = new Thread(SendFile);
                    t.Start(path);
                    break;
                }
                catch
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="path">文件路径</param>
        private static void SendFile(object obj)
        {
            string path = (string)obj;
            if (File.Exists(path))
            {
                FileStream fs_r = new FileStream(path, FileMode.Open, FileAccess.Read);
                FileInfo fi = new FileInfo(path);

                byte[] buffer = new byte[client_FileTransport.SendBufferSize];
                int len = buffer.Length;
                int left = (int)fi.Length;

                sw.WriteLine("success," + (int)fi.Length);
                sw.Flush();

                while (left > 0)
                {
                    if (left > len)
                    {
                        int read = fs_r.Read(buffer, 0, len);
                        netStream_FileTransport.Write(buffer, 0, read);
                        left = left - len;
                    }
                    else
                    {
                        fs_r.Read(buffer, 0, left);
                        netStream_FileTransport.Write(buffer, 0, left);
                        left = left - len;
                    }

                }
                fs_r.Close();
            }
            else
            {
                sw.WriteLine("fail,-");
                sw.Flush();
            }
        }

        /// <summary>
        /// 接收文件
        /// </summary>
        /// <param name="obj">保存路径</param>
        private static void ReceiveFile(object obj)
        {
            string msg = (string)obj;
            string savePath = msg.Split(',')[0];
            if (!File.Exists(savePath))
            {
                int len = int.Parse(msg.Split(',')[1]);
                FileStream fileReceive = new FileStream(savePath, FileMode.Create, FileAccess.Write);

                int bufferSize = client_FileTransport.ReceiveBufferSize;
                byte[] buffer = new byte[bufferSize]; //定义缓冲区
                int left = len;
                //接收数据
                while (left > 0)
                {
                    if (left > buffer.Length)
                    {
                        netStream_FileTransport.Read(buffer, 0, buffer.Length);
                        fileReceive.Write(buffer, 0, buffer.Length);
                        left -= bufferSize;
                    }
                    else
                    {
                        netStream_FileTransport.Read(buffer, 0, left);
                        fileReceive.Write(buffer, 0, left);
                        left -= bufferSize;
                        sw.WriteLine("completed");
                        sw.Flush();
                    }
                }
                fileReceive.Close();
            }
            else
            {
                sw.WriteLine("fail");
                sw.Flush();
            }
        }
    }

}
