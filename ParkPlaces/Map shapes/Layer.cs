using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GMap.NET.WindowsForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParkPlaces.Extensions;

namespace ParkPlaces.Map_shapes
{
    public class Layer: GMapOverlay
    {
        private PrivateObject _privateObject;

        public Layer(string id) : base(id)
        {
        }

        public override void OnRender(Graphics g)
        {
            if (Control.PolygonsEnabled)
            {
                foreach (GMapPolygon r in Polygons)
                {
                    _privateObject = new PrivateObject(r);
                    _privateObject.TryFindField("graphicsPath",
                        BindingFlags.NonPublic | BindingFlags.Instance,
                        out GraphicsPath graphicsPath);

                    if (graphicsPath != null)
                    {
                        var rect = Control.GetRectOfRoute(r);
                        if(rect.HasValue)
                            r.IsVisible = Control.ViewArea.IntersectsWith(rect.Value);
                    }
                }
            }

            base.OnRender(g);
        }
    }
}
