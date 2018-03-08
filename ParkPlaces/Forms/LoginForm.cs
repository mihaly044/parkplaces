using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Windows.Forms;
using ParkPlaces.IO;
using ParkPlaces.Misc;

namespace ParkPlaces.Forms
{
    public partial class LoginForm : Form
    {
        public User User { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            User = Sql.Instance.AuthenticateUser(textBoxUserName.Text, textBoxPassword.Text);

            if (User == null)
            {
                MessageBox.Show("Invalid username or password.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (!User.IsAuthenticated)
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

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Fill in default credientals if given
            var dbSect = ConfigurationManager.GetSection("DefaultLogin") as NameValueCollection;
            if (dbSect == null) return;

            textBoxUserName.Text = dbSect["username"];
            textBoxPassword.Text = dbSect["password"];
        }
    }
}
