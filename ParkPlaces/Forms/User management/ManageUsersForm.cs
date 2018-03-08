using ParkPlaces.Forms.Users_management;
using ParkPlaces.IO;
using ParkPlaces.Misc;
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
    public partial class ManageUsersForm : Form
    {
        private List<User> _users;
        private User _loggedInUser;

        public ManageUsersForm(User loggedInUser)
        {
            InitializeComponent();
            _loggedInUser = loggedInUser;
        }

        private void ManageUsersForm_Load(object sender, EventArgs e)
        {
            refreshUsersListAsync();
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
                Sql.Instance.UpdateUser(editUserForm.GetUser());
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            Sql.Instance.RemoveUser(_users.Find(user => user.UserName == listBoxUsers.SelectedItem.ToString()));
            refreshUsersListAsync();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshUsersListAsync();
        }

        private async void refreshUsersListAsync()
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
