using Server.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class ChatForm : Form
    {

        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private NetworkStream ns;
        private BinaryReader br;
        private BinaryWriter bw;
        private Thread thread;
        private const int port = 10084;
        public ChatForm()
        {
            InitializeComponent();
        }

        private void ChatForm_Load(object sender, EventArgs e)
        {

        }

        public void SetConnect()
        {
            tcpListener = new TcpListener(Broadcast.LocalAddress, port);
            tcpListener.Start();
            thread = new Thread(Connect);
            thread.IsBackground = true;
            thread.Start();
        }

        private void Disconnect()
        {
            this.CloseForm();
        }

        private void Connect()
        {
            tcpClient = tcpListener.AcceptTcpClient();

            ns = tcpClient.GetStream();

            br = new BinaryReader(ns);
            bw = new BinaryWriter(ns);

            Receive();
        }

        private void Receive()
        {
            String recStr = "";
            while (true)
            {
                recStr = br.ReadString();
                if (recStr != null)
                {
                    if (recStr != "Close")
                    {
                    	AddRichTextBox( "other:" + recStr);
                    } 
                    else
                    {
                        this.Disconnect();
                    }
                }
            }
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

        private delegate void AddRichTextBoxDisplayDelegate(string str);
        private void AddRichTextBox(string str)
        {
            if (richTextBoxDisplay.InvokeRequired)
            {
                AddRichTextBoxDisplayDelegate d = new AddRichTextBoxDisplayDelegate(AddRichTextBox);
                this.Invoke(d, str);
            }
            else
            {
                richTextBoxDisplay.AppendText("\n" + str + "\n");
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            bw.Write(richTextBoxSend.Text);
            AddRichTextBox("I:" + richTextBoxSend.Text);
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            try
            {
                br.Close();
                bw.Close();

                ns.Close();

                tcpListener.Stop();

                tcpClient.Close();
                tcpClient.Client.Close();
            }
            catch
            {

                throw;
            }

            thread.Abort();
        }
    }
}
