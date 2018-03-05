using ParkPlaces.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParkPlaces.Forms
{
    public partial class LoginForm : Form
    {

        private Sql _sql;

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
                errorProvider.SetError(textBoxUserName, "Bad username or password");
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
