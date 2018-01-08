using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
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
