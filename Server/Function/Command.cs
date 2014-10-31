using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Function
{
    public static class Command
    {

        /// <summary>
        /// 运行 command 命令
        /// </summary>
        /// <param name="command">要执行的命令</param>
        /// <returns>执行命令的输出结果</returns>
        /// 
        public static string RunCommand(string command)
        {
            Process process = new Process();

            process.StartInfo.FileName = "cmd.exe";           //设置程序名
            process.StartInfo.Arguments = "/c " + command;    //设置执行参数
            process.StartInfo.UseShellExecute = false;        //关闭shell
            process.StartInfo.RedirectStandardInput = true;   //重定向标准输入
            process.StartInfo.RedirectStandardOutput = true;  //重定向标准输出
            process.StartInfo.RedirectStandardError = true;   //重定向错误输出
            process.StartInfo.CreateNoWindow = true;          //设置窗口不显示

            process.Start();//启动

            return process.StandardOutput.ReadToEnd();
        }


        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        public static void RunFile(string fileName)
        {
            Process process;
            try
            {
                process = Process.Start(fileName);
            }
            catch (Exception)
            {
                return;
            }
        }


        /// <summary>
        /// 打开指定网页
        /// </summary>
        /// <param name="url">网址</param>
        public static void RunIE(string url)
        {
            Process process = new Process();
            process.StartInfo.FileName = "iexplore.exe";
            process.StartInfo.Arguments = url;
            process.Start();
            return;
        }


        /// <summary>
        /// 打开应用程序
        /// </summary>
        /// <param name="appName">程序名</param>
        /// <param name="command">运行参数</param>
        public static void RunApplcation(string appName, string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = appName;
            process.StartInfo.Arguments = command;
            process.Start();
            return;
        }

    }
}
