using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Process instance = RunningInstance();
            if (instance == null)
            {
            	Application.Run(new MainForm());
            } 
            else
            {
                HandleRunningInstance(instance);
            }
        }

        #region 确保程序只运行一个实例
        public static Process RunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] process = Process.GetProcessesByName(current.ProcessName);
            foreach (Process item in process)
            {
                if (item.Id != current.Id)
                {
                    if (System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("/", @"/") == current.MainModule.FileName) 
                    {
                        //返回已经存在的进程
                        return item;
                    }
                }
            }

            return null;
        }

        #endregion


        #region 如果已经运行，激活前端

        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(System.IntPtr hWnd, int cmdShow);

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(System.IntPtr hWnd);

        private static void HandleRunningInstance(Process instance)
        {
            MessageBox.Show("已经在运行！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ShowWindowAsync(instance.MainWindowHandle, 1);  //调用api函数，正常显示窗口
            SetForegroundWindow(instance.MainWindowHandle); //将窗口放置最前端
        }

        #endregion
    }
}
