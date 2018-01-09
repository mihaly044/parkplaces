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
using System.Drawing.Drawing2D;
using System.Diagnostics;
using ParkPlaces.Forms;
using System.Linq;
using ParkPlaces.Map_shapes;

namespace ParkPlaces.Controls
{
    // TODO: Documentate code
    public partial class Map : GMapControl
    {
        #region Fields

        private Pen _mouseEnterStrokeClr;
        private Brush _mouseEnterFillClr;
        private bool _isMouseDown;
        private bool _isDrawingPolygon;
        private GMapMarker _pointer;
        private RectMarker _currentRectMaker;
        public Polygon CurrentPolygon;
        private Polygon _currentDrawingPolygon;
        private Dto2Object _fromJsonData;
        private Point _previousMouseLocation;

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

        public bool GetIsDrawingPolygon { get => _isDrawingPolygon; set => _isDrawingPolygon = value; }

        #endregion Fields

        #region Delegates

        public delegate void DrawPolygonEnd(Polygon polygon);

        #endregion Delegates

        #region Events

        /// <summary>
        /// Occurs when polygon drawing has finished
        /// </summary>
        public event DrawPolygonEnd OnDrawPolygonEnd;

        #endregion Events

        #region Internals

        internal readonly GMapOverlay TopLayer = new GMapOverlay("topLayer");
        internal readonly GMapOverlay Polygons = new GMapOverlay("polygons");
        internal readonly GMapOverlay PolygonRects = new GMapOverlay("polygonRects");
        internal readonly Font BlueFont = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular);

        #endregion Internals

        #region Constructors

        public Map()
        {
            InitializeComponent();
            DisableFocusOnMouseEnter = true;

            _mouseEnterStrokeClr = new Pen(Brushes.Blue);
            _mouseEnterStrokeClr.Width = 2;
            _mouseEnterFillClr = new SolidBrush(Color.FromArgb(90, Color.Blue));
            _isMouseDown = false;
            _isDrawingPolygon = false;
            ShowCenter = false;

            _pointer = new GMarkerGoogle(Position, GMarkerGoogleType.arrow) { IsHitTestVisible = false };
            TopLayer.Markers.Add(_pointer);
        }

        #endregion Constructors

        #region GMap events

        private void Map_OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            if (_isDrawingPolygon)
                return;
            if (Zoom >= 12)
                SelectPolygon((Polygon)item);
        }

        private void Map_OnMarkerEnter(GMapMarker item)
        {
            if (item is RectMarker && !_isMouseDown)
            {
                RectMarker rc = item as RectMarker;
                rc.Pen.Color = Color.Red;

                _currentRectMaker = rc;
            }
        }

        private void Map_OnMarkerLeave(GMapMarker item)
        {
            if (item is RectMarker)
            {
                _currentRectMaker = null;
                RectMarker rc = item as RectMarker;
                rc.Pen.Color = Color.Blue;
            }
        }

        #endregion GMap events

        #region Context menu events

        private void finalizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EndDrawPolygon(true);
        }

        private void cancelEscToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EndDrawPolygon(false);
        }

        #endregion Context menu events

        #region Overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (DisplayVersionInfo)
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
            if (e.Button == MouseButtons.Left && _isMouseDown)
            {
                if (_currentRectMaker == null)
                {
                    _pointer.Position = FromLocalToLatLng(e.X, e.Y);

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

                    int? pIndex = (int?)_currentRectMaker.Tag;
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

                    CurrentPolygon.PointsHasChanged();
                    _pointer.Position = pnew;
                    _currentRectMaker.Position = pnew;
                }
            }
            _previousMouseLocation = e.Location;
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _isMouseDown = !_isMouseDown;

            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _pointer.Position = FromLocalToLatLng(e.X, e.Y);
                _isMouseDown = true;

                if (_isDrawingPolygon)
                    DrawNewPolygonPoint();

                if (!IsMouseOverPolygon && !IsMouseOverMarker)
                    ClearSelection();
            }
            else if (_isDrawingPolygon && e.Button == MouseButtons.Right)
            {
                drawPolygonCtxMenu.Show(this, new Point(e.X, e.Y));
            }
            base.OnMouseDown(e);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            if (CurrentPolygon != null && CurrentPolygon.IsMouseOver)
            {
                EditZoneForm editForm = new EditZoneForm((PolyZone)CurrentPolygon.Tag);
                editForm.Show();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (_isDrawingPolygon)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape: EndDrawPolygon(false); break;
                    case Keys.Enter: EndDrawPolygon(true); break;
                }
            }
        }

        #endregion Overrides

        #region App logic

        private void DrawNewPolygonPoint()
        {
            if (_currentDrawingPolygon == null)
            {
                List<PointLatLng> points = new List<PointLatLng>() { _pointer.Position };
                _currentDrawingPolygon = new Polygon(points, "New polygon") { IsHitTestVisible = true };
                Polygons.Polygons.Add(_currentDrawingPolygon);
            }
            else
            {
                _currentDrawingPolygon.Points.Add(_pointer.Position);
                UpdatePolygonLocalPosition(_currentDrawingPolygon);
                _currentDrawingPolygon.PointsHasChanged();
            }
        }

        public void BeginDrawPolygon()
        {
            _isDrawingPolygon = true;
        }

        private void EndDrawPolygon(bool save)
        {
            _isDrawingPolygon = false;

            if (save)
            {
                _currentDrawingPolygon.Tag = new PolyZone()
                {
                    Geometry = new List<Geometry>(),
                    Id = "",
                    Zoneid = "",
                    Color = ColorTranslator.ToHtml((_currentDrawingPolygon.Fill as SolidBrush).Color)
                };
                ((PolyZone)_currentDrawingPolygon.Tag).Geometry.AddRange(
                        _currentDrawingPolygon.Points.Select(
                            polygon => new Geometry() { Lat = polygon.Lat, Lng = polygon.Lng }
                        ).ToList()
                );
                Map_OnPolygonClick(Polygons.Polygons.Where(polygon => (Polygon)polygon == _currentDrawingPolygon).First(), null);
            }
            else
                Polygons.Polygons.Remove(_currentDrawingPolygon);

            OnDrawPolygonEnd?.Invoke(_currentDrawingPolygon);

            _currentDrawingPolygon = null;
        }

        public void RemovePolygon(Polygon p)
        {
            if (_isDrawingPolygon)
                EndDrawPolygon(false);
            else if (p != null)
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

            p.Stroke = _mouseEnterStrokeClr;
            p.Fill = _mouseEnterFillClr;
            CurrentPolygon = p;
            CurrentPolygon.IsSelected = true;

            for (int i = 0; i < p.Points.Count; i++)
            {
                RectMarker mBorders = new RectMarker(CurrentPolygon.Points[i]);
                mBorders.Tag = i;
                PolygonRects.Markers.Add(mBorders);
            }
        }

        public void LoadPolygons()
        {
            _fromJsonData = Dto2Object.FromJson(Properties.Resources.data);

            foreach (PolyZone zone in _fromJsonData.Zones)
            {
                // TODO: Avoid too much data being shown on the map
                //if (!zone.Id.Contains("BUDAPEST"))
                //    continue;

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

        #endregion App logic
    }
}