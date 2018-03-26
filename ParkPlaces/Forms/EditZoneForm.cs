﻿using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ParkPlaces.IO;
using ParkPlaces.Misc;

namespace ParkPlaces.Forms
{
    public partial class EditZoneForm : Form
    {
        private PolyZone _zone;
        private List<City> _cities;

        public PolyZone GetZone() => _zone;

        public EditZoneForm(PolyZone zone)
        {
            InitializeComponent();
            _zone = zone;
        }

        private void EditZoneForm_Load(object sender, System.EventArgs e)
        {
            LoadCitiesAsync();

            textBoxId.Text = _zone.Id;
            textBoxColor.Text = _zone.Color;
            textBoxFee.Text = _zone.Fee.ToString();
            textBoxServiceNa.Text = _zone.ServiceNa;
            textBoxTimetable.Text = _zone.Timetable;

            if (_zone.Color != string.Empty)
            {
                textBoxColor.BackColor = ColorTranslator.FromHtml(textBoxColor.Text);
            }
        }

        private async void LoadCitiesAsync()
        {
            comboBoxCities.Items.Clear();
            _cities = await Sql.Instance.LoadCities();
            foreach (var city in _cities)
            {
                comboBoxCities.Items.Add(city);

                if (_zone.Telepules == city.ToString())
                    comboBoxCities.SelectedItem = city;

            }
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonOk_Click(object sender, System.EventArgs e)
        {
            if (textBoxColor.Text == string.Empty)
            {
                MessageBox.Show("A kiválasztott szín nem lehet üres.", "Hiba", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            _zone.Color = textBoxColor.Text;

            if (!long.TryParse(textBoxFee.Text, out var fee))
            {
                MessageBox.Show("A megadott díjszabás érvénytelen.", "Hiba", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            _zone.Fee = fee;
            _zone.ServiceNa = textBoxServiceNa.Text;

            _zone.Telepules = comboBoxCities.SelectedItem?.ToString() ?? comboBoxCities.Text;

            _zone.Zoneid = textBoxCommonName.Text; 

            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonSelectColor_Click(object sender, System.EventArgs e)
        {
            if (textBoxColor.Text != string.Empty)
            {
                colorDialog.Color = ColorTranslator.FromHtml(textBoxColor.Text);
            }

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxColor.Text = ColorTranslator.ToHtml(colorDialog.Color);
                textBoxColor.BackColor = ColorTranslator.FromHtml(textBoxColor.Text);
            }
        }
    }
}