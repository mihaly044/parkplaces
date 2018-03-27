using System;
using System.Windows.Forms;
using ParkPlaces.IO.Database;
using ParkPlaces.Misc;

namespace ParkPlaces.Forms
{
    public partial class EditUserForm : Form
    {
        /// <summary>
        /// The user to be edited
        /// </summary>
        private User _user;

        /// <summary>
        /// The user thats currently logged in
        /// </summary>
        private readonly User _loggedInUser;

        /// <summary>
        /// Used to return the edited user
        /// object to the parent form
        /// </summary>
        /// <returns></returns>
        public User GetUser() => _user;

        private static readonly byte[] DefaultPasswd = { 0x25, 0x14, 0xe9, 0x0b,
                                                         0xdd, 0x51, 0x3e, 0x6a,
                                                         0xec, 0x56, 0xfd, 0xff };

        public EditUserForm(User loggedInUser, User user = null)
        {
            InitializeComponent();
            _user = user;
            _loggedInUser = loggedInUser;
            DialogResult = DialogResult.Cancel;

            if (_user == null)
            {
                Text = "Új felhasználó";
            }
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void EditUserForm_Load(object sender, EventArgs e)
        {
            if (_loggedInUser.GroupRole >= GroupRole.Admin)
            {
                var accessLevels = Enum.GetValues(typeof(GroupRole));
                foreach (var accessLevel in accessLevels)
                {
                    var groupRole = (GroupRole)accessLevel;
                    comboAccessLevel.Items.Add(groupRole);

                    if (_user != null &&_user.GroupRole == groupRole)
                        comboAccessLevel.SelectedItem = groupRole;
                }
            }
            else
            {
                // Since the admin area is reachable only after
                // groupRole >= Admin, it should never get here
                comboAccessLevel.Items.Add(_loggedInUser.GroupRole);
                comboAccessLevel.Enabled = false;
            }

            if(_user != null)
            {
                textBoxUserName.Text = _user.UserName;
                textBoxPassword.Text = BitConverter.ToString(DefaultPasswd);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(_user == null)
            {
                _user = new User(0);
            }

            if (textBoxUserName.Text == string.Empty || textBoxPassword.Text == string.Empty)
            {
                MessageBox.Show("Username and password cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var accessLevel = (GroupRole) comboAccessLevel.SelectedItem;
            if (_loggedInUser.Id == _user.Id
                && _user.GroupRole > accessLevel)
            {
                MessageBox.Show($"Cannot set a lower user level than {_user.GroupRole}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _user.UserName = textBoxUserName.Text;
            _user.GroupRole = accessLevel;

            if (Sql.Instance.IsDuplicateUser(_user))
            {
                MessageBox.Show("This username already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (textBoxPassword.Text != BitConverter.ToString(DefaultPasswd))
            {
                _user.Password = textBoxPassword.Text;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
