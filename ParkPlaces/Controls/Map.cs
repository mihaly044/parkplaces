using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GMap.NET.WindowsForms;
using GMap.NET;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.MapProviders;
using ParkPlaces.IO;
using ParkPlaces.Extensions;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using ParkPlaces.Forms;
using System.Linq;

namespace ParkPlaces.Controls
{
    // TODO: Documentate code
    public partial class Map : GMapControl
    {
        #region Fields
        private Pen MouseEnterStrokeClr;
        private Brush MouseEnterFillClr;
        private bool IsMouseDown;
        private bool IsDrawingPolygon;
        private GMapMarker Pointer;
        private RectMarker CurrentRectMaker;
        public Polygon CurrentPolygon;
        private Polygon CurrentDrawingPolygon;
        private Dto2Object FromJSONData;
        private Point PreviousMouseLocation;

        [Category("Map Extension")]
        public bool HasGradientSide { get; set; }

        [Category("Map Extension")]
        [DefaultValue(false)]
        public bool DisplayVersionInfo { get; set; }

        [Category("Map Extension")]
        [DefaultValue(false)]
        public bool DisplayCopyright { get; set; }

        [Category("Map Extension")]
        [DefaultValue(100)]
        public int GradientWidth { get; set; }

        #endregion

        #region Delegates
        public delegate void DrawPolygonEnd(Polygon polygon);
        #endregion

        #region Events
        /// <summary>
        /// Occurs when polygon drawing has finished
        /// </summary>
        public event DrawPolygonEnd OnDrawPolygonEnd;
        #endregion

        #region Internals
        internal readonly GMapOverlay TopLayer = new GMapOverlay("topLayer");
        internal readonly GMapOverlay Polygons = new GMapOverlay("polygons");
        internal readonly GMapOverlay PolygonRects = new GMapOverlay("polygonRects");
        internal readonly Font BlueFont = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular);
        #endregion

        #region Constructors
        public Map()
        {
            InitializeComponent();
            DisableFocusOnMouseEnter = true;

            MouseEnterStrokeClr = new Pen(Brushes.Blue);
            MouseEnterStrokeClr.Width = 2;
            MouseEnterFillClr = new SolidBrush(Color.FromArgb(90, Color.Blue));
            IsMouseDown = false;
            IsDrawingPolygon = false;
            ShowCenter = false;

            Pointer = new GMarkerGoogle(Position, GMarkerGoogleType.arrow) { IsHitTestVisible = false };
            TopLayer.Markers.Add(Pointer);
        }
        #endregion

        #region GMap events
        private void Map_OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            if (IsDrawingPolygon)
                return;
            if(Zoom >= 12)
                SelectPolygon((Polygon)item);
        }

        private void Map_OnMarkerEnter(GMapMarker item)
        {
            if (item is RectMarker && !IsMouseDown)
            {
                RectMarker rc = item as RectMarker;
                rc.Pen.Color = Color.Red;

                CurrentRectMaker = rc;
            }
        }

        private void Map_OnMarkerLeave(GMapMarker item)
        {
            if (item is RectMarker)
            {
                CurrentRectMaker = null;
                RectMarker rc = item as RectMarker;
                rc.Pen.Color = Color.Blue;
            }
        }
        #endregion

        #region Context menu events
        private void finalizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EndDrawPolygon(true);
        }

        private void cancelEscToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EndDrawPolygon(false);
        }
        #endregion

        #region Overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if(DisplayVersionInfo)
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = "";

#if DEBUG
                version = string.Format("Debug version {0}, OS: {1}, .NET v{2}", fvi.FileVersion, Environment.OSVersion, Environment.Version);
#else
            version = string.Format("Release version {0}, OS: {1}, .NET v{2}", fvi.FileVersion, Environment.OSVersion, Environment.Version);
#endif
                e.Graphics.DrawString(version, BlueFont, Brushes.Blue, new Point(5, 5));
            }

            if (HasGradientSide)
            {
                LinearGradientBrush linGrBrush = new LinearGradientBrush(
                   new Point(0, 0),
                   new Point(GradientWidth, 0),
                   Color.FromArgb(95, 0, 0, 0),
                   Color.FromArgb(0, 0, 0, 0));

                Rectangle r = new Rectangle(ClientRectangle.Location, ClientRectangle.Size);
                r.Width = GradientWidth;

                e.Graphics.FillRectangle(linGrBrush, r);
            }

            // TODO: implement displaying copyrigt switch
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MapProvider = GMapProviders.GoogleMap;
            Position = new PointLatLng(47.49801, 19.03991);
            Overlays.Add(Polygons);
            Overlays.Add(PolygonRects);
            Overlays.Add(TopLayer);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && IsMouseDown)
            {
                if (CurrentRectMaker == null)
                {
                    Pointer.Position = FromLocalToLatLng(e.X, e.Y);

                    // TODO: Fix polygon dragging
                    /*if (CurrentPolygon != null && CurrentPolygon.IsMouseOver)
                    {
                        for (int i = 0; i < CurrentPolygon.LocalPoints.Count; i++)
                        {
                            CurrentPolygon.LocalPoints[i] = new GPoint
                               (CurrentPolygon.LocalPoints[i].X - e.X - PreviousMouseLocation.X,
                                CurrentPolygon.LocalPoints[i].Y - e.Y - PreviousMouseLocation.Y);
                        }
                        UpdatePolygonLocalPosition(CurrentPolygon);
                    }*/
                }
                else
                {
                    PointLatLng pnew = FromLocalToLatLng(e.X, e.Y);

                    int? pIndex = (int?)CurrentRectMaker.Tag;
                    if (pIndex.HasValue)
                    {
                        if (pIndex < CurrentPolygon.Points.Count)
                        {
                            CurrentPolygon.Points[pIndex.Value] = pnew;
                            ((PolyZone)CurrentPolygon.Tag).Geometry[pIndex.Value].Lat = pnew.Lat;
                            ((PolyZone)CurrentPolygon.Tag).Geometry[pIndex.Value].Lng = pnew.Lng;

                            UpdatePolygonLocalPosition(CurrentPolygon);
                        }
                    }

                    Pointer.Position = pnew;
                    CurrentRectMaker.Position = pnew;
                }
            }
            PreviousMouseLocation = e.Location;
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                IsMouseDown = !IsMouseDown;

            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Pointer.Position = FromLocalToLatLng(e.X, e.Y);
                IsMouseDown = true;

                if (IsDrawingPolygon)
                    DrawNewPolygonPoint();
            }
            else if(IsDrawingPolygon && e.Button == MouseButtons.Right)
            {
                drawPolygonCtxMenu.Show(this, new Point(e.X, e.Y));
            }
            base.OnMouseDown(e);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            if(CurrentPolygon != null && CurrentPolygon.IsMouseOver)
            {
                EditZoneForm editForm = new EditZoneForm((PolyZone)CurrentPolygon.Tag);
                editForm.Show();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if(IsDrawingPolygon)
            {
                switch(e.KeyCode)
                {
                    case Keys.Escape: EndDrawPolygon(false); break;
                    case Keys.Enter: EndDrawPolygon(true); break;
                }
            }
        }
        #endregion

        #region App logic

        private void DrawNewPolygonPoint()
        {
            if (CurrentDrawingPolygon == null)
            {
                List<PointLatLng> points = new List<PointLatLng>();
                points.Add(Pointer.Position);
                CurrentDrawingPolygon = new Polygon(points, "New polygon") { IsHitTestVisible = true };
                Polygons.Polygons.Add(CurrentDrawingPolygon);
            }
            else
            {
                CurrentDrawingPolygon.Points.Add(Pointer.Position);
                UpdatePolygonLocalPosition(CurrentDrawingPolygon);
            }
        }

        public void BeginDrawPolygon()
        {
            IsDrawingPolygon = true;
        }

        private void EndDrawPolygon(bool Save)
        {
            IsDrawingPolygon = false;

            if (Save)
            {
                CurrentDrawingPolygon.Tag = new PolyZone()
                {
                    Geometry = new List<Geometry>(),
                    Id = "",
                    Zoneid = "",
                    Color = ColorTranslator.ToHtml((CurrentDrawingPolygon.Fill as SolidBrush).Color)
                };
                ((PolyZone)CurrentDrawingPolygon.Tag).Geometry.AddRange(
                        CurrentDrawingPolygon.Points.Select(
                            polygon => new Geometry() { Lat = polygon.Lat, Lng = polygon.Lng }
                        ).ToList()
                );
                Map_OnPolygonClick(Polygons.Polygons.Where(polygon => (Polygon)polygon == CurrentDrawingPolygon).First(), null);
            }
            else
                Polygons.Polygons.Remove(CurrentDrawingPolygon);

            OnDrawPolygonEnd?.Invoke(CurrentDrawingPolygon);

            CurrentDrawingPolygon = null;
        }

        public void RemovePolygon(Polygon p)
        {
            if(p != null)
            {
                int iPolygon = Polygons.Polygons.IndexOf(p);

                if (iPolygon > -1)
                {
                    Polygons.Polygons.Remove(p);
                    ClearSelection();
                }
            }
        }

        private void ClearSelection()
        {
            if (CurrentPolygon != null)
            {
                Color polygonColor = ColorTranslator.FromHtml(((PolyZone)CurrentPolygon.Tag).Color);
                CurrentPolygon.Stroke = new Pen(polygonColor);
                CurrentPolygon.Stroke.Width = 2;
                CurrentPolygon.Fill = new SolidBrush(Color.FromArgb(60, polygonColor));
                CurrentPolygon.IsSelected = false;

                PolygonRects.Markers.Clear();
            }
        }

        private void SelectPolygon(Polygon p)
        {
            ClearSelection();

            p.Stroke = MouseEnterStrokeClr;
            p.Fill = MouseEnterFillClr;
            CurrentPolygon = p;
            CurrentPolygon.IsSelected = true;

            for (int i = 0; i < p.Points.Count; i++)
            {
                RectMarker mBorders = new RectMarker(CurrentPolygon.Points[i]);
                mBorders.Tag = i;
                PolygonRects.Markers.Add(mBorders);
            }
        }

        public void loadPolygons()
        {
            FromJSONData = Dto2Object.FromJson(Properties.Resources.data);

            foreach (PolyZone zone in FromJSONData.Zones)
            {
                // TODO: Avoid too much data being shown on the map
                if (!zone.Id.Contains("BUDAPEST"))
                    continue;

                List<PointLatLng> polygonPoints = new List<PointLatLng>();
                foreach (Geometry geometry in zone.Geometry)
                {
                    polygonPoints.Add(new PointLatLng(geometry.Lat, geometry.Lng));
                }

                Color polygonColor = ColorTranslator.FromHtml(zone.Color);
                Polygons.Polygons.Add(new Polygon(polygonPoints, zone.Description)
                {
                    Tag = zone,
                    IsHitTestVisible = true,
                    Fill = new SolidBrush(Color.FromArgb(60, polygonColor)),
                    Stroke = new Pen(polygonColor)
                    {
                        Width = 2
                    }
                });
            }
        }

        public Point ConvertGPointToPoint(GPoint p)
        {
            return new Point((int)p.X, (int)p.Y);
        }

        #endregion
    }
}