using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ParkPlaces.IO;
using PPNetLib.Prototypes;
using ParkPlaces.Net;
using PPNetLib.Contracts;

namespace ParkPlaces.Forms
{
    public partial class EditZoneForm : Form
    {
        private readonly PolyZone _zone;
        private List<City> _cities;

        public PolyZone GetZone() => _zone;

        public EditZoneForm(PolyZone zone)
        {
            InitializeComponent();
            _zone = zone;

            Client.Instance.OnCityListAck += OnCityListAck;
        }

        private void OnCityListAck(CityListAck packet)
        {
            comboBoxCities.Enabled = true;
            comboBoxCities.Items.Clear();
            _cities = packet.Cities;
            if (_cities != null)
            {
                foreach (var city in _cities)
                {
                    comboBoxCities.Items.Add(city);

                    if (_zone.Telepules == city.ToString())
                        comboBoxCities.SelectedItem = city;

                }
            }

            // Reset event listener
            Client.Instance.OnCityListAck -= OnCityListAck;
        }

        private void EditZoneForm_Load(object sender, System.EventArgs e)
        {
            Client.Instance.Send(new CityListReq());

            textBoxId.Text = _zone.Id;
            textBoxColor.Text = _zone.Color;
            textBoxFee.Text = _zone.Fee.ToString();
            textBoxServiceNa.Text = _zone.ServiceNa;
            textBoxTimetable.Text = _zone.Timetable;
            textBoxCommonName.Text = _zone.Zoneid;

            if (_zone.Color != string.Empty)
            {
                textBoxColor.BackColor = ColorTranslator.FromHtml(textBoxColor.Text);
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
            _zone.Timetable = textBoxTimetable.Text;

            if (_zone.Telepules == string.Empty)
            {
                MessageBox.Show("A városnév nem lehet üres", "Hiba", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

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