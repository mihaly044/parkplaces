using PPNetClient;
using PPNetLib.Contracts.Monitor;
using PPNetLib.Prototypes;
using System;
using System.Windows.Forms;

namespace PPAdmin
{
    public partial class PpAdminForm : Form
    {
        private User _loggedInUser;

        public PpAdminForm()
        {
            InitializeComponent();
            Client.Instance.OnServerMonitorAck += OnServerMonitorAck;
            Client.Instance.OnConnectionError += OnConnectionError;
        }

        private void OnConnectionError(Exception e)
        {
            MessageBox.Show("Connection error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        private void OnServerMonitorAck(ServerMonitorAck ack)
        {
            listView1.Items.Add(new ListViewItem(new string[] { $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}", ack.Output }));
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
    }
}
