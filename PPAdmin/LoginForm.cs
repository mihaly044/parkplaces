using PPNetClient;
using PPNetLib.Contracts;
using PPNetLib.Prototypes;
using System;
using System.Windows.Forms;

namespace PPAdmin
{
    public partial class LoginForm : Form
    {
        public User User { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
            CheckForIllegalCrossThreadCalls = false;
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            Client.Instance.SetIp(textBox3.Text);
            Client.Instance.SetPort(11000);

            Client.Instance.ResetLoginAcks();
            Client.Instance.OnLoginAck += OnLoginAck;
            Client.Instance.OnLoginDuplicateAck += OnLoginDuplicateAck;
            Client.Instance.SetOfflineMode(false);

            if (!Client.Instance.IsConnected())
                Client.Instance.Connect();

            Client.Instance.Send(new LoginReq
            {
                Username = textBox1.Text,
                Password = textBox2.Text,
                Monitor = true
            });
        }

        private void OnLoginDuplicateAck()
        {
            MessageBox.Show("Could not log in.\nThere is somebody already logged in with this account.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Client.Instance.Disconnect();
        }

        private void OnLoginAck(LoginAck ack)
        {
            User = ack.User;

            if (User == null)
            {
                MessageBox.Show("Invalid username or password.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Client.Instance.Disconnect();
            }
            else if (!User.IsAuthenticated)
            {
                MessageBox.Show("You are not authorized to log in.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Client.Instance.Disconnect();
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
