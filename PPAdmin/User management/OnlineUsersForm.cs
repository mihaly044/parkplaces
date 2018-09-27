using PPNetClient;
using PPNetLib.Prototypes;
using System;
using System.Windows.Forms;
using PPNetLib.Contracts.Monitor;

namespace PPAdmin
{
    public partial class OnlineUsersForm : Form
    {
        public OnlineUsersForm()
        {
            InitializeComponent();

            Client.Instance.OnOnlineUsersAck += OnOnlineUsersAck;
            Client.Instance.Send(new OnlineUsersReq());
        }

        ~OnlineUsersForm()
        {
            Client.Instance.OnOnlineUsersAck -= OnOnlineUsersAck;
        }

        private void OnOnlineUsersAck(OnlineUsersAck ack)
        {
            listView1.Items.Clear();
            foreach(var client in ack.OnlineUsersList)
            {
                listView1.Items.Add(new ListViewItem(new string[] {
                    client.UserName,
                    client.IpPort,
                    Enum.GetName(typeof(GroupRole), client.GroupRole),
                    client.Monitor ? "Yes" : "No"
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

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ipPort = listView1.FocusedItem.SubItems[1];
            Client.Instance.Send(new DisconnectUserReq() { IpPort = ipPort.Text });
        }

        private void banToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ipPort = listView1.FocusedItem.SubItems[1];
            Client.Instance.Send(new BanIpAddressReq() { IpAddress = ipPort.Text });
        }
    }
}
