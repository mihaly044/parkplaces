using System;
using System.Collections.Generic;
using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;
using ParkPlaces.Controls;
using ParkPlaces.IO;

namespace ParkPlaces.Extensions
{
    public class Polygon : GMapPolygon
    {
        public bool IsSelected { get; set; }

        public Polygon(List<PointLatLng> points, string description) : base(points, description)
        {
        }

        public System.Drawing.Point GetCentroid()
        {
            double centroidX = 0.0;
            double centroidY = 0.0;

            for (int i = 0; i < LocalPoints.Count-1; i++)
            {
                centroidX += LocalPoints[i].X;
                centroidY += LocalPoints[i].Y;
            }
            centroidX /= LocalPoints.Count-1;
            centroidY /= LocalPoints.Count-1;

            return (new System.Drawing.Point((int)centroidX, (int)centroidY));
        }

        public double GetArea()
        {
            throw new NotImplementedException();
        }

        public override void OnRender(Graphics g)
        {
            base.OnRender(g);

            if (Tag == null)
                return;

            System.Drawing.Point centroid = GetCentroid();
            centroid.X -= (((PolyZone)Tag).Zoneid.Length * 10) / 2;

            if (IsSelected)
            {
                g.DrawString(((PolyZone)Tag).Id, new Font("Arial", (int)Overlay.Control.Zoom), Brushes.Red, centroid);
            }
            else
                g.DrawString(((PolyZone)Tag).Zoneid, new Font("Arial", (int)Overlay.Control.Zoom), Fill, centroid);
        }
    }
}
