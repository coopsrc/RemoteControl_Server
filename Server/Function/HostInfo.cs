using System;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Management;
using Microsoft.Win32;
using System.Drawing;

using Server.Controller;

namespace Server.Function
{
    public static class HostInfo
    {
        public static string Address()
        {
            string address = Broadcast.LocalAddress.ToString();
            return address;
        }

        public static string Location()
        {
            string address = Broadcast.LocalAddress.ToString();
            string url = "http://www.youdao.com/smartresult-xml/search.s?type=ip&q=" + address + "";
            string location = "";

            try
            {
                using (XmlReader read = XmlReader.Create(url))  //获取youdao返回的xml格式文件内容
                {
                    while (read.Read())                          //从流中读取下一个字节
                    {
                        switch (read.NodeType)
                        {
                            case XmlNodeType.Text:               //取xml格式文件当中的文本内容
                                if (string.Format("{0}", read.Value).ToString().Trim() != address)
                                {
                                    location = string.Format("{0}", read.Value).ToString().Trim();
                                }
                                break;
                        }
                    }
                }
            }
            catch
            {
                location = "Unknown";
            }

            return location;
        }

        public static string HostName()
        {
            string hostName = Dns.GetHostName();
            return hostName;
        }

        public static string UserName()
        {
            string userName = Environment.GetEnvironmentVariable("UserName");
            return userName;
        }

        public static string OSName()
        {
            string osName = Environment.OSVersion.ToString();
            return osName;
        }

        public static string CPUType()
        {
            string cpuInfo = "";

            try
            {

                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");

                cpuInfo = rk.GetValue("ProcessorNameString").ToString();
            }
            catch
            {
                cpuInfo = "Unknow";
            }
            return cpuInfo;
        }

        public static string MemorySize()
        {
            ulong totalSize = 0;
            ulong freeSize = 0;

            ManagementClass mc = new ManagementClass("Win32_OperatingSystem");
            foreach (ManagementObject obj in mc.GetInstances())
            {
                if (obj["TotalVirtualMemorySize"] != null)
                    totalSize = (ulong)obj["TotalVirtualMemorySize"];

                if (obj["FreePhysicalMemory"] != null)
                    freeSize = (ulong)obj["FreePhysicalMemory"];
                break;
            }



            if (totalSize > 0)
            {
                return freeSize / 1024 + "M/" + totalSize / 1024 + "M";
            }
            else
            {
                return "Unknown";
            }
        }

        public static string HardDiskSize()
        {
            string hardDisk = null;
            string[] MyDrive = Environment.GetLogicalDrives();
            long s0 = 0, s1 = 0;
            foreach (string MyDriveLetter in MyDrive)
            {
                try
                {
                    DriveInfo MyDriveInfo = new DriveInfo(MyDriveLetter);
                    if (MyDriveInfo.DriveType == DriveType.CDRom || MyDriveInfo.DriveType == DriveType.Removable)
                        continue;
                    s0 += MyDriveInfo.TotalSize;
                    s1 += MyDriveInfo.TotalFreeSpace;
                }
                catch { }
            }
            hardDisk = (s1 / 1073741824).ToString() + "G/" + (s0 / 1073741824).ToString() + "G";
            return hardDisk;
        }

        public static string ScreenSize()
        {
            string screen = "";

            Rectangle rect = Screen.PrimaryScreen.Bounds;
            screen = rect.Width + "*" + rect.Height;

            return screen;
        }

        public static string VideoStatus()
        {
            string video = "可用";
            return video;
        }

        public static string ConnectStatus()
        {
            string status = "连接";
            return status;
        }
    }
}
