using System;
using System.Windows.Forms;
using GMap.NET.MapProviders;
using ParkPlaces.Map_shapes;
using ParkPlaces.IO;

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

        private void Map_OnPolygonClick(GMap.NET.WindowsForms.GMapPolygon item, MouseEventArgs e)
        {
            if (Map.CurrentPolygon != null)
                propertyGrid1.SelectedObject = Map.CurrentPolygon.Tag;
        }

        private void ParkPlacesForm_Shown(object sender, EventArgs e)
        {
            if (IoHandler.Instance.NeedUpdate)
            {
                if (MessageBox.Show("New polygon data is available. Do you wish start the update process?",
                        "Update available", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    toolStripStatusLabel.Text = "Started update...";
                    toolStripProgressBar.Visible = true;
                    IoHandler.Instance.OnUpdateChangedEventHandler += (s, updateProcessChangedArgs) =>
                    {
                        toolStripProgressBar.Maximum = updateProcessChangedArgs.TotalChunks;
                        toolStripProgressBar.Value = updateProcessChangedArgs.CurrentChunks;

                        toolStripStatusLabel.Text = $"Downloaded {updateProcessChangedArgs.CurrentChunks} items of {updateProcessChangedArgs.TotalChunks}";

                        if (updateProcessChangedArgs.TotalChunks != updateProcessChangedArgs.CurrentChunks) return;
                        toolStripProgressBar.Visible = false;
                        toolStripStatusLabel.Text = "Ready";
                    };
                }
            }

            Map.LoadPolygons();
            lblZoom.Text = $"Zoom: {Map.Zoom}";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}