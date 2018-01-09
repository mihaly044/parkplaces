using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using GMap.NET;
using GMap.NET.WindowsForms;
using ParkPlaces.IO;
using ParkPlaces.Utils;

namespace ParkPlaces.Map_shapes
{
    public class Polygon : GMapPolygon
    {
        public bool IsSelected { get; set; }
        private bool _areaNeedsUpdate;
        private double _area;
        // private Font _nameFont; //never used

        public Polygon(List<PointLatLng> points, string description) : base(points, description)
        {
            _areaNeedsUpdate = true;
        }

        public Point GetCentroid()
        {
            var count = LocalPoints.Count - 1;
            var centroidX = LocalPoints.Sum(m => m.X) / count;
            var centroidY = LocalPoints.Sum(m => m.Y) / count;

            return (new Point((int)centroidX, (int)centroidY));
        }

        public double GetArea()
        {
            if (_areaNeedsUpdate)
                _area = PolygonalUtil.ComputeArea(Points);

            _areaNeedsUpdate = false;
            return _area;
        }

        public string GetAreaAsString1(int decimalPlaces, bool inSquareMetres = true)
        {
            var sb = new StringBuilder("0.");

            for (var i = 0; i < decimalPlaces; i++)
                sb.Append("#");

            if (!inSquareMetres)
            {
                sb.Append("km2");
                return (GetArea() / 1000000).ToString(sb.ToString());
            }
            else
            {
                sb.Append("m2");
                return GetArea().ToString(sb.ToString());
            }
        }

        public void PointsHasChanged()
        {
            _areaNeedsUpdate = true;
        }

        public override void OnRender(Graphics g)
        {
            base.OnRender(g);

            if (Tag == null)
                return;

            var centroid = GetCentroid();
            centroid.X -= (((PolyZone)Tag).Zoneid.Length * 10) / 2;

            if (IsSelected)
                g.DrawString(((PolyZone)Tag).Id, new Font("Arial", (int)Overlay.Control.Zoom), Brushes.Red, centroid);
            else
                g.DrawString(((PolyZone)Tag).Zoneid, new Font("Arial", (int)Overlay.Control.Zoom), Fill, centroid);

            if (Overlay.Control.Zoom >= 15)
            {
                centroid.Y += (int)Overlay.Control.Zoom + 5;
                g.DrawString(GetAreaAsString(4), new Font("Arial", (int)(Overlay.Control.Zoom / 2)), Brushes.Black, centroid);
            }
        }
    }
}