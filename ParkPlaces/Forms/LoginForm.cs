using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Windows.Forms;
using ParkPlaces.IO.Database;
using ParkPlaces.Misc;
using ParkPlaces.Net;
using PPNetLib.Contracts;

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
            c = new Client();
        }

        /// <summary>
        /// Occurs when the user clicks on the login button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Save selected DB configuration
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["DBConnection"].Value = comboDBConnection.SelectedIndex == 1 ? "alt" : "main";
            config.Save();
            ConfigurationManager.RefreshSection("appSettings");

            Sql.ResetInstance();

            User = Sql.Instance.AuthenticateUser(textBoxUserName.Text, textBoxPassword.Text);

            if (User == null)
            {
                MessageBox.Show("Helytelen felhasználónév vagy jelszó.", "Hiba",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (!User.IsAuthenticated)
            {
                MessageBox.Show("Nincs megfelelő jogosultsága az alkalmazás használatához.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
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

            // Select DB connection
            var dbConnection = ConfigurationManager.AppSettings["DBConnection"];
            comboDBConnection.SelectedIndex = dbConnection == "alt" ? 1 : 0;
        }

        Client c;
        private void button1_Click(object sender, EventArgs e)
        {
            
            c.Send(new LoginReq() { Username = "A", Password = "D"});
            c.Send(new LoginReq() { Username = "B", Password = "E" });
            c.Send(new LoginReq() { Username = "C", Password = "F" });

            c.OnTest += () => {
                MessageBox.Show("Success!");
            };
        }
    }
}
