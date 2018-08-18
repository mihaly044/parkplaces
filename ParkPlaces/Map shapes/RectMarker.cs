using System;
using System.Drawing;
using System.Runtime.Serialization;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
// ReSharper disable MemberCanBePrivate.Global

namespace ParkPlaces.Map_shapes
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

            Size = new Size(10, 10);
            Offset = new Point(-Size.Width / 2, -Size.Height / 2);
        }

        public override void OnRender(Graphics g)
        {
            Rect = new Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
            Font = new Font("Arial", 12);
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

        #endregion ISerializable Members
    }
}