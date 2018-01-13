﻿using System;
using System.Windows.Forms;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using ParkPlaces.IO;
using ParkPlaces.Map_shapes;

namespace ParkPlaces.Forms
{
    public partial class ParkPlacesForm : Form
    {
        public ParkPlacesForm()
        {
            InitializeComponent();
        }

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            lblMouse.Text = Map.FromLocalToLatLng(e.X, e.Y).ToString();
        }

        private void Map_DrawPolygonEnd(Polygon polygon)
        {
            drawPolygonButton.Checked = false;
        }

        private void Map_OnMapZoomChanged()
        {
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
            if (IoHandler.Instance.NeedUpdate)
            {
                if (MessageBox.Show("New polygon data is available. Do you wish start the update process?",
                        "Update available", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    GetRemote();
                else
                    Map.UpdateHint = false;
            }
            lblZoom.Text = $"Zoom: {Map.Zoom}";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openRemoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetRemote();
        }

        private void GetRemote()
        {
            toolStripStatusLabel.Text = "Started update...";
            toolStripProgressBar.Visible = true;
            IoHandler.Instance.OnUpdateChangedEventHandler += (s, updateProcessChangedArgs) =>
            {
                toolStripProgressBar.Maximum = updateProcessChangedArgs.TotalChunks;
                toolStripProgressBar.Value = updateProcessChangedArgs.CurrentChunks;

                toolStripStatusLabel.Text =
                    $"Downloaded {updateProcessChangedArgs.CurrentChunks} items of {updateProcessChangedArgs.TotalChunks}";

                if (updateProcessChangedArgs.TotalChunks != updateProcessChangedArgs.CurrentChunks) return;
                toolStripProgressBar.Visible = false;
                toolStripStatusLabel.Text = "Ready";
            };
            Map.UpdateHint = true;
            Map.LoadPolygons();
        }

        private void closeCurrentSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Map.UnloadSession();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}