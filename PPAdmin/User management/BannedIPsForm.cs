using PPNetClient;
using PPNetLib.Contracts.Monitor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPAdmin
{
    public partial class BannedIPsForm : Form
    {
        public BannedIPsForm()
        {
            InitializeComponent();

            Client.Instance.OnListBannedIpsAck += OnListBannedIpsAck; 
            Client.Instance.Send(new ListBannedIpsReq());
        }

        private void OnListBannedIpsAck(ListBannedIpsReq ack)
        {
            listView1.Items.Clear();
            foreach (var ip in ack.BannedIps)
            {
                var expires = DateTime.Now + (DateTime.Now - ip.Date);
                listView1.Items.Add(new ListViewItem(new string[] {
                    ip.IpPort,
                    ip.Date.ToLongDateString() + " " + ip.Date.ToLongTimeString(),
                    expires.ToLongDateString() + " " + expires.ToLongTimeString()
                }));
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listView1.FocusedItem.Bounds.Contains(e.Location))
                {
                    contextMenuStrip1.Show(listView1, e.X, e.Y);
                }
            }
        }

        private void unbanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ipAddress = listView1.FocusedItem.SubItems[1];
            Client.Instance.Send(new UnbanIPAddressReq() { IpAddress = ipAddress.Text });
        }
    }
}
