using System.Collections.Generic;
using System.Drawing;
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
            var centroidX = 0.0;
            var centroidY = 0.0;

            for (var i = 0; i < LocalPoints.Count - 1; i++)
            {
                centroidX += LocalPoints[i].X;
                centroidY += LocalPoints[i].Y;
            }

            centroidX /= LocalPoints.Count - 1;
            centroidY /= LocalPoints.Count - 1;

            return (new Point((int)centroidX, (int)centroidY));
        }

        public double GetArea()
        {
            if (_areaNeedsUpdate)
                _area = PolygonalUtil.ComputeArea(Points);

            _areaNeedsUpdate = false;
            return _area;
        }

        public string GetAreaAsString(int decimalPlaces, bool inSquareMetres = true)
        {
            var format = "0.";
            for (var i = 0; i < decimalPlaces; i++)
                format += "#";

            if (!inSquareMetres)
            {
                format += "km2";
                return (GetArea() / 1000000).ToString(format);
            }
            else
            {
                format += "m2";
                return GetArea().ToString(format);
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