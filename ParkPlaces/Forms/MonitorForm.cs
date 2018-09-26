using ParkPlaces.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParkPlaces.Forms
{
    public partial class MonitorForm : Form
    {
        public MonitorForm()
        {
            InitializeComponent();

            Client.Instance.OnServerMonitorAck += OnServerMonitorAck;
        }

        ~MonitorForm()
        {
            Client.Instance.OnServerMonitorAck -= OnServerMonitorAck;
        }

        private void OnServerMonitorAck(PPNetLib.Contracts.ServerMonitorAck ack)
        {
            textBox1.Text += ack.Output;
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuStrip1.Show(textBox1, e.X, e.Y);
        }
    }
}
