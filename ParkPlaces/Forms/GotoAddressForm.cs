﻿using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;

namespace ParkPlaces.Forms
{
    public partial class GotoAddressForm : Form
    {
        private PointLatLng _latLng;
        public PointLatLng LatLng => _latLng;

        public GotoAddressForm()
        {
            InitializeComponent();
        }

        private void Okbutton_Click(object sender, System.EventArgs e)
        {
            var pos = GMapProviders.GoogleMap.GetPoint(addressTextBox.Text, out var status);
            if (pos == null || status != GeoCoderStatusCode.G_GEO_SUCCESS) return;

            _latLng = pos.Value;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
