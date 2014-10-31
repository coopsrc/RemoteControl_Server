using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Function
{
    public class RemoteScreen
    {

        public RemoteScreen()
        {

        }

        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        [DllImport("user32")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public void Mouse_Event(string mouseEvent, string para1, string para2)
        {
            
            if(mouseEvent=="event")
            {
                string str=para1+para2;
                switch(str)
                {
                    case "leftclick":
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        break;
                    case "rightclick":
                        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        break;
                    case "leftdoubleclick":
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        break;
                    case "rightdoubleclick":
                        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        break;
                    case "leftdown":
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        break;
                    case "rightdown":
                        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                        break;
                    case "leftup":
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        break;
                    case "rightup":
                        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        break;

                }
            }
            if(mouseEvent=="move")
            {
                //mouse_event(MOUSEEVENTF_MOVE, Convert.ToInt32(para1), Convert.ToInt32(para2), 0, 0);
                Cursor.Position = new Point(Convert.ToInt32(para1), Convert.ToInt32(para2));
            }
        }

        public void Keyboard_Event(string keyValue,string keyEvent)
        {
            byte[] keys = BitConverter.GetBytes(Convert.ToInt32(keyValue));
            if (keyEvent == "keydown")
            {
                keybd_event(keys[0], 0, 0, 0);
            }
            if (keyEvent == "keyup")
            {
                keybd_event(keys[0], 0, 2, 0);
            }
        }

        public void showBox(string[] strs)
        {
            string content = "";
            for (int i = 1; i < strs.Length; i++)
            {
                if (i == strs.Length - 1)
                    content += strs[i];
                else
                    content += strs[i] + ",";
            }
            MessageBox.Show(content);
        }

        public Bitmap CaptureScreen()
        {
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;


            Bitmap bitmap = new Bitmap(screenSize.Width, screenSize.Height);

            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CopyFromScreen(0, 0, 0, 0, screenSize);

            return bitmap;
        }
    }
}
