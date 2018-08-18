using PPNetLib.Prototypes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ParkPlaces.IO.Database;
using ParkPlaces.Net;
using PPNetLib.Contracts;

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
            RefreshUsersList();
        }

        private void OnUserListAck(UserListAck packet)
        {
            listBoxUsers.Items.Clear();
            _users = packet.Users;

            listBoxUsers.Enabled = true;

            foreach (var user in _users)
            {
                listBoxUsers.Items.Add(user);

                if (user.Id == _loggedInUser.Id)
                    listBoxUsers.SelectedItem = user;
            }

            Client.Instance.OnUserListAck -= OnUserListAck;
        }

        private void listBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            var isCreatorSelected = ((User)listBoxUsers.SelectedItem).Id == _loggedInUser.CreatorId;

            btnRemove.Enabled = _loggedInUser.Id != ((User)listBoxUsers.SelectedItem).Id && !isCreatorSelected;
            btnEdit.Enabled = !isCreatorSelected;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            var newUserForm = new EditUserForm(_loggedInUser);
            if(newUserForm.ShowDialog(this) == DialogResult.OK)
            {
                var user = newUserForm.GetUser();
                Client.Instance.Send(new InsertUserReq() {User = user});
                RefreshUsersList();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var editUserForm = new EditUserForm(_loggedInUser, (User)listBoxUsers.SelectedItem);
            if(editUserForm.ShowDialog(this) == DialogResult.OK)
            {
                var selectedIndex = listBoxUsers.SelectedIndex; 
                Sql.Instance.UpdateUser(editUserForm.GetUser());
                RefreshUsersList();
                listBoxUsers.SelectedIndex = selectedIndex;
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("A fiók törlését nem lehet visszavonni. Folytatja?", "Figyelmeztetés",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Client.Instance.Send(new RemoveUserReq() {UserId = ((User) (listBoxUsers.SelectedItem)).Id});
                RefreshUsersList();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            listBoxUsers.Enabled = false;
            RefreshUsersList();
        }

        private void RefreshUsersList()
        {
            Client.Instance.Send(new UserListReq());
            Client.Instance.OnUserListAck += OnUserListAck;
        }

        private readonly SolidBrush _selectedColor = new SolidBrush(Color.FromKnownColor(KnownColor.Highlight));
        private readonly SolidBrush _unselectedColor = new SolidBrush(Color.White);
        private readonly SolidBrush _adminColor = new SolidBrush(Color.Yellow);

        private void listBoxUsers_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index >= 0 && e.Index < listBoxUsers.Items.Count)
            {
                var user = listBoxUsers.Items[e.Index] as User;
                var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

                SolidBrush backgroundColor;
                if (user?.GroupRole == GroupRole.Admin && !selected)
                {
                    backgroundColor = _adminColor;
                }
                else if (selected)
                {
                    backgroundColor = _selectedColor;
                }
                else
                {
                    backgroundColor = _unselectedColor;
                }
                
                e.Graphics.FillRectangle(backgroundColor, e.Bounds);
                e.Graphics.DrawString(user?.ToString(), e.Font, Brushes.Black, listBoxUsers.GetItemRectangle(e.Index).Location);
            }
            e.DrawFocusRectangle();
        }
    }
}
