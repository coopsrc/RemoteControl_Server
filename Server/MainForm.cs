using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Server.Controller;

namespace Server
{
    public partial class MainForm : Form
    {
        private Broadcast broadcast;
        private ControlConnection controlConnection;
        public MainForm()
        {
            InitializeComponent();
            broadcast = new Broadcast();
            controlConnection = new ControlConnection();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            controlConnection.Disconnect();
            broadcast.Disconnect();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            broadcast.Start();
            controlConnection.Start();
        }
    }
}
