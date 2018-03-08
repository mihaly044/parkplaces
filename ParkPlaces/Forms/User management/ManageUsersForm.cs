using ParkPlaces.IO;
using ParkPlaces.Misc;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ParkPlaces.Forms
{
    public partial class ManageUsersForm : Form
    {
        private List<User> _users;
        private readonly User _loggedInUser;

        public ManageUsersForm(User loggedInUser)
        {
            InitializeComponent();
            _loggedInUser = loggedInUser;
        }

        private void ManageUsersForm_Load(object sender, EventArgs e)
        {
            RefreshUsersListAsync();
        }

        private void listBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemove.Enabled = _loggedInUser.UserName != ((User)listBoxUsers.SelectedItem).UserName;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var editUserForm = new EditUserForm((User)listBoxUsers.SelectedItem, _loggedInUser);
            if(editUserForm.ShowDialog() == DialogResult.OK)
            {
                var selectedIndex = listBoxUsers.SelectedIndex; 
                Sql.Instance.UpdateUser(editUserForm.GetUser());
                RefreshUsersListAsync();
                listBoxUsers.SelectedIndex = selectedIndex;
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            Sql.Instance.RemoveUser(_users.Find(user => user.UserName == listBoxUsers.SelectedItem.ToString()));
            RefreshUsersListAsync();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshUsersListAsync();
        }

        private async void RefreshUsersListAsync()
        {
            listBoxUsers.Items.Clear();
            _users = await Sql.Instance.LoadUsers();
            foreach (var user in _users)
            {
                listBoxUsers.Items.Add(user);
            }
            listBoxUsers.SelectedIndex = 0;
        }
    }
}
