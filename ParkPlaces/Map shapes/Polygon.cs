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
        private bool AreaNeedsUpdate;
        private double Area;
        private Font NameFont;

        public Polygon(List<PointLatLng> points, string description) : base(points, description)
        {
            AreaNeedsUpdate = true;
        }

        public System.Drawing.Point GetCentroid()
        {
            double centroidX = 0.0;
            double centroidY = 0.0;

            for (int i = 0; i < LocalPoints.Count - 1; i++)
            {
                centroidX += LocalPoints[i].X;
                centroidY += LocalPoints[i].Y;
            }
            centroidX /= LocalPoints.Count - 1;
            centroidY /= LocalPoints.Count - 1;

            return (new System.Drawing.Point((int)centroidX, (int)centroidY));
        }

        public double GetArea()
        {
            if (AreaNeedsUpdate)
                Area = PolygonalUtil.computeArea(Points);

            AreaNeedsUpdate = false;
            return Area;
        }

        public string GetAreaAsString(int decimalPlaces, bool inSquareMetres = true)
        {
            string format = "0.";
            for (int i = 0; i < decimalPlaces; i++)
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
            AreaNeedsUpdate = true;
        }

        public override void OnRender(Graphics g)
        {
            base.OnRender(g);

            if (Tag == null)
                return;

            System.Drawing.Point centroid = GetCentroid();
            centroid.X -= (((PolyZone)Tag).Zoneid.Length * 10) / 2;

            if (IsSelected)
                g.DrawString(((PolyZone)Tag).Id, new Font("Arial", (int)Overlay.Control.Zoom), Brushes.Red, centroid);
            else
                g.DrawString(((PolyZone)Tag).Zoneid, new Font("Arial", (int)Overlay.Control.Zoom), Fill, centroid);

            if (Overlay.Control.Zoom >= 15)
            {
                centroid.Y += (int)Overlay.Control.Zoom + 5;
                g.DrawString(GetAreaAsString(4, true), new Font("Arial", (int)Overlay.Control.Zoom / 2), Brushes.Black, centroid);
            }
        }
    }
}