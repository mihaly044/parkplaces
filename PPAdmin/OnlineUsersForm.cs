using PPNetClient;
using PPNetLib.Prototypes;
using System;
using System.Windows.Forms;
using PPNetLib.Contracts;
using PPNetLib.Contracts.SynchroniseAcks;

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
    }
}
