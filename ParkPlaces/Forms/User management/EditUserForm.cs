using ParkPlaces.DotUtils.Extensions;
using ParkPlaces.IO;
using ParkPlaces.Misc;
using System;

using System.Windows.Forms;

namespace ParkPlaces.Forms.Users_management
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
        private User _loggedInUser;

        /// <summary>
        /// Used to return the edited user
        /// object to the parent form
        /// </summary>
        /// <returns></returns>
        public User GetUser() => _user;

        private static readonly byte[] defaultPasswd = new byte[]{ 0x25, 0x14, 0xe9, 0x0b,
                                                                   0xdd, 0x51, 0x3e, 0x6a,
                                                                   0xec, 0x56, 0xfd, 0xff };

        public EditUserForm(User user, User loggedInUser)
        {
            InitializeComponent();
            _user = user;
            _loggedInUser = user;
            DialogResult = DialogResult.Cancel;
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

                    if (_user.GroupRole == groupRole)
                        comboAccessLevel.SelectedItem = groupRole;
                }
            }
            else
            {
                // Since the admin area is reachable only after
                // groupRole >= Admin, it should never get there
                comboAccessLevel.Items.Add(_loggedInUser.GroupRole);
                comboAccessLevel.Enabled = false;
            }

            textBoxUserName.Text = _user.UserName;
            textBoxPassword.Text = BitConverter.ToString(defaultPasswd);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_loggedInUser == _user
                && _user.GroupRole > (GroupRole)comboAccessLevel.SelectedItem)
            {
                MessageBox.Show($"Cannot set a lower user level than {_user.GroupRole}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if(Sql.Instance.IsDuplicateUser(_user))
            {
                MessageBox.Show($"This username already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _user.UserName = textBoxUserName.Text;

            if(textBoxPassword.Text != BitConverter.ToString(defaultPasswd))
            {
                _user.Password = textBoxPassword.Text;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
