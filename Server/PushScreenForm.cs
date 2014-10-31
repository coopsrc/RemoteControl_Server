using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class PushScreenForm : Form
    {

        private UdpClient udpClient;
        private IPEndPoint ipEndPoint;
        private Thread thread;
        private const int portServer = 1234;
        private const int portClient = 10090;
        private IPAddress multcastAddress = IPAddress.Parse("234.5.6.7");

        public PushScreenForm()
        {
            InitializeComponent();
        }


        public delegate void SetPictureDelegate(Image image);
        private void SetPicture(Image image)
        {
            if (pictureBox.InvokeRequired)
            {
                SetPictureDelegate d = new SetPictureDelegate(SetPicture);
                this.Invoke(d, image);
            }
            else
            {
                pictureBox.Image = image;
            }
        }

        private void PushScreenForm_Load(object sender, EventArgs e)
        {

        }

        public void SetConnect()
        {
            udpClient = new UdpClient(portServer);
            udpClient.JoinMulticastGroup(multcastAddress);
            ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

            thread = new Thread(Receive);
            thread.IsBackground = true;
            thread.Start();
        }


        private void Receive()
        {
            MemoryStream ms=null;
            Bitmap image = null;
            string str = "";

            while (true)
            {
                try
                {
                    byte[] buffer =udpClient.Receive(ref ipEndPoint);
                    str = Encoding.Unicode.GetString(udpClient.Receive(ref ipEndPoint)); 
                    if(str=="Close")
                    {
                        Disconnect();
                    }
                    ms = new MemoryStream(buffer);
                    image = new Bitmap((Image)new Bitmap(ms));
                }
                catch
                {
                    this.CloseForm();
                    udpClient.Close();
                }
                try
                {
                    //放大图片
                    pictureBox.Image = GetRecoverImage
                        (image, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                }
                catch
                {
                    MessageBox.Show("image");
                }

            }
        }

        private Bitmap GetRecoverImage(Image image,int width,int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);
            graphics.DrawImage(image, new Rectangle(0, 0, width, height));
            return bitmap;
        }

        public void Disconnect()
        {
            this.CloseForm();
        }
        private delegate void CloseFormDelegate();
        private void CloseForm()
        {
            if (this.InvokeRequired)
            {
                CloseFormDelegate d = new CloseFormDelegate(CloseForm);
                this.Invoke(d);
            }
            else
            {
                this.Close();
            }
        }

        private void PushScreenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            udpClient.Close();
        }


    }
}
