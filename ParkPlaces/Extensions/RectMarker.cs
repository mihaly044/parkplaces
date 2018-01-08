using System.Drawing;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET;
using System;
using System.Runtime.Serialization;
using System.Drawing.Drawing2D;

namespace ParkPlaces.Extensions
{
    [Serializable]
    public class RectMarker : GMapMarker, ISerializable
    {
        [NonSerialized]
        public Pen Pen;

        [NonSerialized]
        public Brush Brush;

        [NonSerialized]
        public GMarkerGoogle InnerMarker;

        [NonSerialized]
        public Font Font;

        [NonSerialized]
        public Rectangle Rect;

        public RectMarker(PointLatLng p)
           : base(p)
        {
            Pen = new Pen(Brushes.Blue, 1);
            Brush = Brushes.Red;

            Size = new System.Drawing.Size(10, 10);
            Offset = new System.Drawing.Point(-Size.Width / 2, -Size.Height / 2);
        }

        public override void OnRender(Graphics g)
        {
            Rect = new System.Drawing.Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
            Font = new System.Drawing.Font("Arial", 12);
            g.DrawRectangle(Pen, Rect);
            g.DrawString("*", Font, Brush, Rect);
        }

        public override void Dispose()
        {
            if (Pen != null)
            {
                Pen.Dispose();
                Pen = null;
            }

            if (InnerMarker != null)
            {
                InnerMarker.Dispose();
                InnerMarker = null;
            }

            if (Font != null)
            {
                Font.Dispose();
                Font = null;
            }

            if (Brush != null)
            {
                Brush.Dispose();
                Brush = null;
            }

            base.Dispose();
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected RectMarker(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
        }

        #endregion
    }
}

