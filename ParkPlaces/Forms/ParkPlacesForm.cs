﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GMap.NET.MapProviders;
using ParkPlaces.IO;
using ParkPlaces.Map_shapes;
using ParkPlaces.Controls;
using ParkPlaces.Net;
using PPNetLib.Prototypes;
// ReSharper disable MethodSupportsCancellation

namespace ParkPlaces.Forms
{
    public partial class ParkPlacesForm : Form
    {
        private User _loggedInUser;

        public ParkPlacesForm()
        {
            InitializeComponent();

            // Workaround to hide the white strip
            // between the map and the menustrip
            var tsRenderer = new TsRenderer();
            menuStrip.Renderer = tsRenderer;
            toolStrip.Renderer = tsRenderer;
            statusStrip.Renderer = tsRenderer;

            // Anonymous function for IOHandler's OnUpdateChangedEventHandler
            // Specifies what happens when IOHandler invokes the aftermentioned
            // event
            IoHandler.Instance.OnUpdateChangedEventHandler += (s, updateProcessChangedArgs) =>
            {
                toolStripProgressBar.Visible = true;

                if (updateProcessChangedArgs.TotalChunks == 1)
                {
                    toolStripProgressBar.Value = 0;
                    toolStripProgressBar.Minimum = 0;
                    toolStripProgressBar.Maximum = 100;
                    toolStripStatusLabel.Text = "Frissítés indítása";
                    return;
                }

                toolStripProgressBar.Maximum = updateProcessChangedArgs.TotalChunks;
                toolStripProgressBar.Value = updateProcessChangedArgs.CurrentChunks;

                toolStripStatusLabel.Text =
                    $"Letöltve {updateProcessChangedArgs.CurrentChunks} város a(z) {updateProcessChangedArgs.TotalChunks}-ből";

                if (updateProcessChangedArgs.TotalChunks != updateProcessChangedArgs.CurrentChunks) return;

                toolStripProgressBar.Visible = false;

                toolStripProgressBar.Minimum = 0;
                toolStripProgressBar.Maximum = 100;

                toolStripStatusLabel.Text = "Kész";
            };

            Client.Instance.OnConnectionError += OnConnectionError;
        }

        private void OnConnectionError(Exception e)
        {
            Invoke(new Action(() => {
                MessageBox.Show("Hiba a szerverrel történő kommunikáció során.\nA program kilép.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(e.HResult);
            }));
        }

        /// <summary>
        /// Display the mouse cursor's position in a label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            lblMouse.Text = Map.FromLocalToLatLng(e.X, e.Y).ToString();
        }

        /// <summary>
        /// Reset the drawPolygonButton GUI control stat
        /// </summary>
        /// <param name="polygon">The polygon on the map that has finished drawing</param>
        private void Map_DrawPolygonEnd(Polygon polygon)
        {
            drawPolygonButton.Checked = false;
        }

        /// <summary>
        /// Begin drawing a polygon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawPolygonButton_Click(object sender, EventArgs e)
        {
            drawPolygonButton.Checked = true;
            Map.BeginDrawPolygon();
        }

        /// <summary>
        /// Display and update the total verticles count
        /// </summary>
        /// <param name="verticleChangedArg"></param>
        private void Map_VerticlesChanged(VerticleChangedArg verticleChangedArg)
        {
            lblShapesCount.Text =
                $"{verticleChangedArg.ShapesCount} alakzat és {verticleChangedArg.VerticlesCount} pont";
        }

        /// <summary>
        /// Display and update zoom level
        /// </summary>
        private void Map_OnMapZoomChanged()
        {
            if (lblZoom.Visible)
                lblZoom.Text = $"Zoom: {Map.Zoom}";
        }

        /// <summary>
        /// Used for changing the map provider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void googleMapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Map.MapProvider = GMapProviders.GoogleMap;
        }

        /// <summary>
        /// Used for changing the map provider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openStreetMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Map.MapProvider = GMapProviders.OpenStreetMap;
        }


        /// <summary>
        /// Removes a polygon from the map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemovePolygonButton_Click(object sender, EventArgs e)
        {
            if (Map.CurrentPolygon == null && !Map.IsDrawingPolygon)
                MessageBox.Show("Nem választott ki zónát.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                Map.RemovePolygon(Map.CurrentPolygon);
        }

        /// <summary>
        /// Specify an initial text for the label that displays
        /// the current zoom level of the map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParkPlacesForm_Shown(object sender, EventArgs e)
        {
            lblZoom.Text = $"Zoom: {Map.Zoom}";
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Unloads all shapes, markers and verticles from the map 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeCurrentSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Map.UnloadSession(true);
        }

        /// <summary>
        /// Download zone data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void forceUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Map.LoadPolygons(true);
        }

        /// <summary>
        /// Open shape data from the JSON file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Serialize shape data into a JSON file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Shows a form that allows the user to point the map
        /// to given coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Shows a form that allows the user to point the map
        /// to a given address
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Calls OnFormLoad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParkPlacesForm_Load(object sender, EventArgs e)
        {
            OnFormLoad();
        }

        /// <summary>
        /// Sets default values for each important attribute
        /// and shows the login form
        /// </summary>
        private void OnFormLoad()
        {
            Text = "ParkPlaces Editor";

            Map.UnloadSession(true);

            var loginForm = new LoginForm();
            if (loginForm.ShowDialog(this) != DialogResult.OK)
            {
                Application.Exit();
                return;
            }

            Text += $" / Belépve mint {loginForm.User.UserName}, {loginForm.User.GroupRole} jogosultsággal /";
            _loggedInUser = loginForm.User;

            var offlineMode = Client.Instance.GetOfflineMode();

            adminToolStripMenuItem.Enabled = loginForm.User.GroupRole >= GroupRole.Admin && !offlineMode;
            Map.SetReadOnly(loginForm.User.GroupRole < GroupRole.Editor);
            Map.SetPositionByKeywords("Szeged");

            if (!offlineMode)
            {
                var loadingForm = new LoadingForm();
                loadingForm.OnReadyEventHandler += (s, dto) =>
                {
                    Map.ConstructGui(dto);
                    Show();
                };

                loadingForm.LoadDataAsync();
                loadingForm.ShowDialog();
            }
            else
            {
                var dto = new Dto2Object
                {
                    Zones = new List<PolyZone>(),
                    Type = "ZoneCollection"
                };

                Map.ConstructGui(dto);
                Show();
            }
        }

        /// <summary>
        /// Logout procedure
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logout();
        }

        /// <summary>
        /// Logout process
        /// </summary>
        private void Logout()
        {
            CloseAllForms();
            Hide();
            OnFormLoad();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async void SaveButton_ClickAsync(object sender, EventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // TODO: Rewrite SaveButton_ClickAsync
            /*
            var dto = Map.GetDataObject();
            if (dto == null) return;

            saveButton.Enabled = false;

            Progress<int> progress = new Progress<int>();

            progress.ProgressChanged += (sender2, progressPercentage) =>
            {
                toolStripProgressBar.Visible = true;
                toolStripProgressBar.Value = progressPercentage;

                toolStripStatusLabel.Text =
                    $"Zónák feldolgozása ... {progressPercentage}%";

                if (progressPercentage == 100)
                {
                    toolStripProgressBar.Visible = false;
                    toolStripStatusLabel.Text = "Kész";
                    saveButton.Enabled = true;
                }
            };

            await Task.Run(() => { Sql.Instance.ExportToMySql(dto, progress); });*/
        }

        /// <summary>
        /// Shows the users management window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void manageUsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ManageUsersForm(_loggedInUser).ShowDialog(this);
        }

        /// <summary>
        /// Close  all forms that are associated 
        /// with this application
        /// </summary>
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
        /// Check user access level and log out if necessary
        /// </summary>
        /// <param name="user"></param>
        private void UpdateUserAccess(User user)
        {
            if (user.GroupRole != _loggedInUser.GroupRole)
            {
                Invoke(new Action(Logout));
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().Show();
        }

        private void ParkPlacesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Client.Instance.GetOfflineMode() && Map.IsWaiting())
            {
                var result = MessageBox.Show(
                    "Feldolgozatlan módosításai vannak. Biztosan kilép?", "Figyelmeztetés", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                e.Cancel = result == DialogResult.No; 
            }
        }
    }
}