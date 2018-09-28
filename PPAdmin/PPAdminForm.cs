using PPNetClient;
using PPNetLib;
using PPNetLib.Contracts.Monitor;
using PPNetLib.Prototypes;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PPAdmin
{
    public partial class PpAdminForm : Form
    {
        private User _loggedInUser;
        private readonly Array _messageTypes;
        private const string cmdSign = "command > ";

        public PpAdminForm()
        {
            InitializeComponent();
            Client.Instance.OnServerMonitorAck += OnServerMonitorAck;
            Client.Instance.OnConnectionError += OnConnectionError;
            Client.Instance.OnCommandAck += OnCommandAck;

            _messageTypes = Enum.GetValues(typeof(ConsoleKit.MessageType));
            txtCmd.Text = cmdSign;
        }

        private void OnCommandAck(CommandAck ack)
        {
            listView1.Items.Add(new ListViewItem(new string[] { $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}", ack.Response }));
        }

        private void OnConnectionError(Exception e)
        {
            MessageBox.Show("Connection error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        private void OnServerMonitorAck(ServerMonitorAck ack)
        {
            var item = new ListViewItem(new string[] { $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}", ack.Output });

            foreach (var type in _messageTypes)
            {
                var messageType = $"[{type}]";
                if (ack.Output.IndexOf(messageType) == 0)
                {
                    switch(type)
                    {
                        case ConsoleKit.MessageType.ERROR:
                            item.BackColor = Color.Red;
                            item.ForeColor = Color.Yellow;
                        break;

                        case ConsoleKit.MessageType.WARNING:
                            item.BackColor = Color.DarkOrange;
                            item.ForeColor = Color.Red;
                            break;

                        case ConsoleKit.MessageType.DEBUG:
                            item.BackColor = Color.Cyan;
                        break;

                        case ConsoleKit.MessageType.INFO:
                            item.BackColor = Color.LightBlue;
                        break;
                    }
                }
            }

            listView1.Items.Add(item);
            listView1.Items[listView1.Items.Count - 1].EnsureVisible();
            UpdateMessagesCountLabel();
        }

        private void UpdateMessagesCountLabel()
        {
            toolStripStatusLabel1.Text = $"Message count: {listView1.Items.Count}";
        }

        private void PPAdminForm_Load(object sender, EventArgs e)
        {
            OnFormLoad();
        }

        private void OnFormLoad()
        {
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog(this) != DialogResult.OK)
            {
                Application.Exit();
                return;
            }

            Show();
            _loggedInUser = loginForm.User;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logout();
            Application.Exit();
        }

        private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            UpdateMessagesCountLabel();
        }

        private void CloseAllForms()
        {
            var fc = Application.OpenForms;
            if (fc.Count > 1)
                for (var i = fc.Count; i > 1; i--)
                {
                    var selectedForm = Application.OpenForms[i - 1];
                    selectedForm.Close();
                }
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hide();
            Logout();
            OnFormLoad();
        }

        private void Logout()
        {
            CloseAllForms();
            Client.Instance.Disconnect();
        }

        private void onlineUsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new OnlineUsersForm().ShowDialog();
        }

        private void manageUsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ManageUsersForm(_loggedInUser).ShowDialog(this);
        }

        private void bannedUsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new BannedIPsForm().ShowDialog(this);
        }

        private void txtCmd_TextChanged(object sender, EventArgs e)
        {
            ProtectCaret();
        }

        private void txtCmd_MouseClick(object sender, MouseEventArgs e)
        {
            ProtectCaret();
        }

        private void ProtectCaret()
        {
            if (txtCmd.Text.IndexOf(cmdSign) != 0)
            {
                txtCmd.Text = cmdSign;
            }

            if (txtCmd.SelectionStart <= cmdSign.Length)
                txtCmd.SelectionStart = cmdSign.Length;
        }

        private void txtCmd_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                Client.Instance.Send(new CommandReq { Command = txtCmd.Text.Substring(0, 4096).Split(' ') });
                txtCmd.Text = cmdSign;
            }
            else
            {
                ProtectCaret();
            }
        }
    }
}
