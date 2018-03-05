using ParkPlaces.IO;
using System;
using System.Windows.Forms;

namespace ParkPlaces.Forms
{
    public partial class LoginForm : Form
    {

        private readonly Sql _sql;

        public LoginForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;

            _sql = new Sql();
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            if(!_sql.AuthenticateUser(textBoxUserName.Text, textBoxPassword.Text))
            {
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
    }
}
