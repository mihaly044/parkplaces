using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET.MapProviders;
using ParkPlaces.IO;
using ParkPlaces.Map_shapes;
using ParkPlaces.Controls;
using ParkPlaces.Misc;

namespace ParkPlaces.Forms
{
    public partial class ParkPlacesForm : Form
    {

        /// <summary>
        /// Used to cancel CheckUserPrivileges method's runner thread that
        /// runs in intervals
        /// </summary>
        private CancellationTokenSource _userPrivilegesCtrl;

        public User LoggedInUser { get; set; }

        public ParkPlacesForm()
        {
            InitializeComponent();

            var tsRenderer = new TsRenderer();
            menuStrip.Renderer = tsRenderer;
            toolStrip.Renderer = tsRenderer;
            statusStrip.Renderer = tsRenderer;

            IoHandler.Instance.OnUpdateChangedEventHandler += (s, updateProcessChangedArgs) =>
            {
                toolStripProgressBar.Visible = true;

                if (updateProcessChangedArgs.TotalChunks == 1)
                {
                    toolStripProgressBar.Value = 0;
                    toolStripProgressBar.Minimum = 0;
                    toolStripProgressBar.Maximum = 100;
                    toolStripStatusLabel.Text = "Starting update ...";
                    return;
                }

                toolStripProgressBar.Maximum = updateProcessChangedArgs.TotalChunks;
                toolStripProgressBar.Value = updateProcessChangedArgs.CurrentChunks;

                toolStripStatusLabel.Text =
                    $"Downloaded {updateProcessChangedArgs.CurrentChunks} items of {updateProcessChangedArgs.TotalChunks}";

                if (updateProcessChangedArgs.TotalChunks != updateProcessChangedArgs.CurrentChunks) return;

                toolStripProgressBar.Visible = false;
                toolStripStatusLabel.Text = "Ready";
            };
        }

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            lblMouse.Text = Map.FromLocalToLatLng(e.X, e.Y).ToString();
        }

        private void Map_DrawPolygonEnd(Polygon polygon)
        {
            drawPolygonButton.Checked = false;
        }

        private void Map_VerticlesChanged(VerticleChangedArg verticleChangedArg)
        {
            lblShapesCount.Text =
                $"{verticleChangedArg.ShapesCount} shapes and {verticleChangedArg.VerticlesCount} verticles";
        }

        private void Map_OnMapZoomChanged()
        {
            if (lblZoom.Visible)
                lblZoom.Text = $"Zoom: {Map.Zoom}";
        }

        private void googleMapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Map.MapProvider = GMapProviders.GoogleMap;
        }

        private void openStreetMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Map.MapProvider = GMapProviders.OpenStreetMap;
        }

        private void drawPolygonButton_Click(object sender, EventArgs e)
        {
            drawPolygonButton.Checked = true;
            Map.BeginDrawPolygon();
        }

        private void RemovePolygonButton_Click(object sender, EventArgs e)
        {
            if (Map.CurrentPolygon == null && !Map.IsDrawingPolygon)
                MessageBox.Show("No polygon has been selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                Map.RemovePolygon(Map.CurrentPolygon);
        }

        private void ParkPlacesForm_Shown(object sender, EventArgs e)
        {
            lblZoom.Text = $"Zoom: {Map.Zoom}";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void closeCurrentSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Map.UnloadSession();
        }

        private void forceUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Map.LoadPolygons(true);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (
                var dlg = new OpenFileDialog()
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
                }
            )
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    Map.LoadPolygons(false, dlg.FileName);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (
                var dlg = new SaveFileDialog()
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
                }
            )
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    Map.SavePolygons(dlg.FileName);
                }
            }
        }

        private void coordinateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new GotoCoordinatesForm())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    Map.SetMapPosition(dlg.LatLng);
                    Map.SetPointerPosition(dlg.LatLng);
                }
            }
        }

        private void addressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new GotoAddressForm())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    Map.SetMapPosition(dlg.LatLng);
                    Map.SetPointerPosition(dlg.LatLng);
                }
            }
        }

        private void ParkPlacesForm_Load(object sender, EventArgs e)
        {
            OnFormLoad();
        }

        private void OnFormLoad()
        {
            Text = "ParkPlaces Editor";

            Map.UnloadSession();

            var loginForm = new LoginForm();
            if (loginForm.ShowDialog(this) != DialogResult.OK)
            {
                Environment.Exit(0);
            }

            Text += $" / Logged in as {loginForm.User.UserName} with {loginForm.User.GroupRole} access /";
            LoggedInUser = loginForm.User;

#pragma warning disable 4014
            _userPrivilegesCtrl = new CancellationTokenSource();
            CheckUserPrivileges(_userPrivilegesCtrl.Token);
#pragma warning restore 4014


            adminToolStripMenuItem.Enabled = loginForm.User.GroupRole >= GroupRole.Admin;
            Map.SetReadOnly(loginForm.User.GroupRole < GroupRole.Editor);

            var loadingForm = new LoadingForm();
            loadingForm.Show();

            loadingForm.OnReadyEventHandler += (s, dto) =>
            {
                Map.ConstructGui(dto);
                Show();
            };

            loadingForm.LoadDataAsync();
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hide();
            OnFormLoad();
        }

        /// <summary>
        /// Logout process
        /// </summary>
        private void Logout(bool forcedLogout = false)
        {

            CloseAllForms();

            if (forcedLogout)
            {
                _userPrivilegesCtrl.Cancel();
            }

            Hide();
            OnFormLoad();
        }


        // TODO: Make method async
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async void SaveButton_ClickAsync(object sender, EventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var dto = Map.GetDataObject();
            if (dto == null) return;

            var sql = new Sql();
            await sql.ExportToMySql(dto);
            MessageBox.Show("Done.");
        }

        private void manageUsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ManageUsersForm(LoggedInUser).ShowDialog(this);
        }

        private void CloseAllForms()
        {
            var fc = Application.OpenForms;
            if (fc.Count > 1)
                for (var i = fc.Count; i > 1; i--)
                {
                    var selectedForm = Application.OpenForms[i - 1];
                    selectedForm.Close();
                }
        }

        /// <summary>
        /// Check user access level in intervals and call UpdateUserAccess
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private void CheckUserPrivileges(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    UpdateUserAccess(Sql.Instance.GetUserData(LoggedInUser));

                    await Task.Delay(1000, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                        break;
                }
            });
        }

        /// <summary>
        /// Check user access level and log out if necessary
        /// </summary>
        /// <param name="user"></param>
        private void UpdateUserAccess(User user)
        {
            if (user.GroupRole != LoggedInUser.GroupRole)
            {
                Invoke(new Action(() => { Logout(true); }));
            }
        }
    }
}