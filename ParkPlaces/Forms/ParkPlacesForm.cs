using System;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;

namespace ParkPlaces
{
    public partial class ParkPlacesForm : Form
    {
        public ParkPlacesForm()
        {
            InitializeComponent();
        }

        private void ParkPlacesForm_Load(object sender, EventArgs e)
        {
            Map.loadPolygons();
            lblZoom.Text = "Zoom: " + Map.Zoom.ToString();
        }

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            lblMouse.Text = Map.FromLocalToLatLng(e.X, e.Y).ToString();
        }

        private void Map_DrawPolygonEnd(Extensions.Polygon polygon)
        {
            drawPolygonButton.Checked = false;
        }

        private void Map_OnMapZoomChanged()
        {
            lblZoom.Text = "Zoom: " + Map.Zoom.ToString(); ;
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
            Map.RemovePolygon(Map.CurrentPolygon);
        }
    }
}
