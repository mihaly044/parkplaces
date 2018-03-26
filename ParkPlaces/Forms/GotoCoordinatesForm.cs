using System.Windows.Forms;

namespace ParkPlaces.Forms
{
    public partial class GotoCoordinatesForm : Form
    {
        private GMap.NET.PointLatLng _latLng;
        public  GMap.NET.PointLatLng LatLng => _latLng;

        public GotoCoordinatesForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }


        private void Okbutton_Click(object sender, System.EventArgs e)
        {
            if(double.TryParse(latitudeTextBox.Text, out var lat) &&
               double.TryParse(longitudeTextBox.Text, out var lng))
            {
                _latLng = new GMap.NET.PointLatLng(lat, lng);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Érvénytelen formátum.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
