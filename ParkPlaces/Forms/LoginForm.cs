using ParkPlaces.IO;
using System;
using System.Windows.Forms;
using ParkPlaces.Misc;

namespace ParkPlaces.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var user = User.Login(textBoxUserName.Text, textBoxPassword.Text);

            if (user.GroupRole == GroupRole.None)
            {
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (!user.IsAuthenticated)
            {
                MessageBox.Show("You do not have the appropriate rights to use this application.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin.PerformClick();
        }
    }
}
