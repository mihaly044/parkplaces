using System.Windows.Forms;
using ParkPlaces.IO;

namespace ParkPlaces.Forms
{
    public partial class EditZoneForm : Form
    {
        public EditZoneForm(PolyZone polygon)
        {
            InitializeComponent();
            zonePropertyGrid.SelectedObject = polygon;
        }
    }
}