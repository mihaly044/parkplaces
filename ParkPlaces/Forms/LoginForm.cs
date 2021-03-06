﻿using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Windows.Forms;
using PPNetClient;
using PPNetLib.Contracts;
using PPNetLib.Prototypes;

namespace ParkPlaces.Forms
{
    public partial class LoginForm : Form
    {
        /// <summary>
        /// Get user data from the login form
        /// </summary>
        public User User { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
            CheckForIllegalCrossThreadCalls = false;

            Client.Instance.Disconnect();
            lblServer.Text = Client.Instance.GetServerAddress();
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

        private void OnLoginDuplicateAck()
        {
            MessageBox.Show("Could not log in.\nThere is somebody already logged in with this account.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Client.Instance.Disconnect();
        }

        /// <summary>
        /// Occurs when the user clicks on the login button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            Client.Instance.ResetLoginAcks();
            Client.Instance.OnLoginAck += OnLoginAck;
            Client.Instance.OnLoginDuplicateAck += OnLoginDuplicateAck;
            Client.Instance.SetOfflineMode(false);

            if(!Client.Instance.IsConnected())
                Client.Instance.Connect();

            Client.Instance.Send(new LoginReq
            {
                Username = textBoxUserName.Text,
                Password = textBoxPassword.Text
            });
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// This event handler is assigned to both textboxes on the form
        /// and responsible for starting the login procedure on Enter keypress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin.PerformClick();
        }

        /// <summary>
        /// Load and fill in default user credientals from
        /// the configuration file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Fill in default credientals if given
            var defaultLogin = ConfigurationManager.GetSection("DefaultLogin") as NameValueCollection;
            if (defaultLogin == null) return;

            textBoxUserName.Text = defaultLogin["username"];
            textBoxPassword.Text = defaultLogin["password"];
        }

        private void btnOfflineMode_Click(object sender, EventArgs e)
        {
            Client.Instance.SetOfflineMode(true);
            User = new User(textBoxUserName.Text, 1) {GroupRole = GroupRole.Admin};

            DialogResult = DialogResult.OK;
            
            Client.Instance.ResetLoginAcks();

            Close();
        }
    }
}
