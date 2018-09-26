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
            listView1.Items.Add(new ListViewItem( new string[]{ $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}", ack.Output } ));
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuStrip1.Show(listView1, e.X, e.Y);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
        }
    }
}
