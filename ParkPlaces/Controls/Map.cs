﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using ParkPlaces.Forms;
using ParkPlaces.IO;
using ParkPlaces.Map_shapes;
using ParkPlaces.Properties;

namespace ParkPlaces.Controls
{
    // TODO: Documentate code
    public partial class Map : GMapControl
    {
        #region Delegates

        /// <summary>
        /// Delegate function for OnDrawPolygonEnd
        /// </summary>
        /// <param name="polygon">The polygon that the user has finished drawing</param>
        public delegate void DrawPolygonEnd(Polygon polygon);

        #endregion Delegates

        #region Constructors

        public Map()
        {
            InitializeComponent();
            DisableFocusOnMouseEnter = true;

            _selectedStrokeClr = new Pen(Brushes.Blue) { Width = 2 };
            _selecteFillClr = new SolidBrush(Color.FromArgb(90, Color.Blue));
            _isMouseDown = false;
            IsDrawingPolygon = false;
            ShowCenter = false;

            _pointer = new GMarkerGoogle(Position, GMarkerGoogleType.arrow) { IsHitTestVisible = false };
            TopLayer.Markers.Add(_pointer);

            deleteCacheDate = DateTime.Now;
            deleteCacheDate = deleteCacheDate.AddDays(-10);
            Manager.PrimaryCache.DeleteOlderThan(deleteCacheDate, null);
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Occurs when polygon drawing gets finished by the user
        /// </summary>
        public event DrawPolygonEnd OnDrawPolygonEnd;

        #endregion Events

        #region Fields

        /// <summary>
        /// Defines the stroke colour of map shapes in selected state
        /// </summary>
        private readonly Pen _selectedStrokeClr;

        /// <summary>
        /// Defines the fill colour of map shapes in selected state
        /// </summary>
        private readonly Brush _selecteFillClr;

        /// <summary>
        /// Downloaded and cached map tile images gets deleted on this date
        /// </summary>
        private readonly DateTime deleteCacheDate;

        /// <summary>
        /// Indicates whether the left mouse button is down at a given moment
        /// </summary>
        private bool _isMouseDown;

        /// <summary>
        /// Represents a green pointer on the map
        /// </summary>
        private readonly GMapMarker _pointer;

        /// <summary>
        /// Indicates the current point selected of currentSelPolygon
        /// </summary>
        private RectMarker _currentRectMaker;

        /// <summary>
        /// The current selected polygon. Null if nothing is selected
        /// </summary>
        public Polygon CurrentPolygon;

        /// <summary>
        /// Represents a polygon that is currently in the process of drawing on the GUI
        /// </summary>
        private Polygon _currentDrawingPolygon;

        /// <summary>
        /// Represents the current data transfer object in use
        /// </summary>
        private Dto2Object _fromJsonData;

        /// <summary>
        /// Can be used to get the previous mouse location on every OnMouseMove call
        /// </summary>
        private Point _previousMouseLocation; //Never used, just assigned

        /// <summary>
        /// Indicates whether the map has a black-transparent gradient on its left side
        /// </summary>
        [Category("Map Extension")]
        public bool HasGradientSide { get; set; }

        /// <summary>
        /// Gradient width
        /// </summary>
        [Category("Map Extension")]
        [DefaultValue(100)]
        public int GradientWidth { get; set; }

        /// <summary>
        /// Indicates if the version info string should get rendered at the left top corner of the map
        /// </summary>
        [Category("Map Extension")]
        [DefaultValue(false)]
        public bool DisplayVersionInfo { get; set; }

        /// <summary>
        /// Indicates wheether the copyright info string should get rendered at the left bottom corner of the map
        /// </summary>
        [Category("Map Extension")]
        [DefaultValue(false)]
        public bool DisplayCopyright { get; set; }

        /// <summary>
        /// Indicates whether a polygon is currently drawing
        /// </summary>
        public bool IsDrawingPolygon { get; set; }

        #endregion Fields

        #region Internals

        /// <summary>
        /// The very upper overlayof the map
        /// </summary>
        internal readonly GMapOverlay TopLayer = new GMapOverlay("topLayer");

        /// <summary>
        /// Map layer that contains polygons
        /// </summary>
        internal readonly GMapOverlay Polygons = new GMapOverlay("polygons");

        /// <summary>
        /// Map layer that contains rectangle markers
        /// </summary>
        internal readonly GMapOverlay PolygonRects = new GMapOverlay("polygonRects");

        /// <summary>
        /// Version info gets rendered using this font
        /// </summary>
        internal readonly Font BlueFont = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular);

        #endregion Internals

        #region GMap events

        /// <summary>
        /// Occurs when the user clicks on a polygon
        /// </summary>
        /// <param name="item">The polygon item that has been cliked on</param>
        /// <param name="e"></param>
        private void Map_OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            if (IsDrawingPolygon || item == null)
                return;
            if (Zoom >= 12)
                SelectPolygon((Polygon)item);
        }

        /// <summary>
        /// Occurs when the cursor moves over a Rectangle marker
        /// </summary>
        /// <param name="item"></param>
        private void Map_OnMarkerEnter(GMapMarker item)
        {
            if (item is RectMarker marker && !_isMouseDown)
            {
                marker.Pen.Color = Color.Red;
                _currentRectMaker = marker;
            }
        }

        /// <summary>
        /// Occurs when the cursor leaves a Rectangle marker
        /// </summary>
        /// <param name="item"></param>
        private void Map_OnMarkerLeave(GMapMarker item)
        {
            if (item is RectMarker marker)
            {
                marker.Pen.Color = Color.Blue;
                _currentRectMaker = null;
            }
        }

        #endregion GMap events

        #region Context menu events

        /// <summary>
        /// Occurs when Finalize gets clicked on the context menu that appears in drawing mode
        /// </summary>
        private void finalizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EndDrawPolygon(true);
        }

        // <summary>
        /// Occurs when Cancel gets clicked on the context menu that appears in drawing mode
        /// </summary>
        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EndDrawPolygon(false);
        }

        #endregion Context menu events

        #region Overrides

        /// <summary>
        /// Gets called when the map gets painted
        /// </summary>
        /// <param name="e">Graphics context</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (DisplayVersionInfo)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

#if DEBUG
                var version =
                    $"Debug version {fvi.FileVersion}, OS: {Environment.OSVersion}, .NET v{Environment.Version}";
#else
                var  version =
 $"Release version {fvi.FileVersion}, OS: {Environment.OSVersion}, .NET v{Environment.Version}";
#endif

                e.Graphics.DrawString(version, BlueFont, Brushes.Blue, new Point(5, 5));
            }

            if (HasGradientSide)
            {
                var linGrBrush = new LinearGradientBrush(
                    new Point(0, 0),
                    new Point(GradientWidth, 0),
                    Color.FromArgb(95, 0, 0, 0),
                    Color.FromArgb(0, 0, 0, 0));

                var r = new Rectangle(ClientRectangle.Location, ClientRectangle.Size) { Width = GradientWidth };

                e.Graphics.FillRectangle(linGrBrush, r);
            }

            // TODO: implement displaying copyright switch
        }

        /// <summary>
        /// Gets called when the control gets loaded
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MapProvider = GMapProviders.GoogleMap;
            Position = new PointLatLng(47.49801, 19.03991);
            Overlays.Add(Polygons);
            Overlays.Add(PolygonRects);
            Overlays.Add(TopLayer);
        }

        /// <summary>
        /// Gets called when the cursor moves on the map
        /// </summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _isMouseDown)
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
                    var pnew = FromLocalToLatLng(e.X, e.Y);

                    var pIndex = (int?)_currentRectMaker.Tag;

                    if (pIndex.HasValue && pIndex < CurrentPolygon.Points.Count)
                    {
                        CurrentPolygon.Points[pIndex.Value] = pnew;
                        ((PolyZone)CurrentPolygon.Tag).Geometry[pIndex.Value] = pnew.ToGeometry();

                        UpdatePolygonLocalPosition(CurrentPolygon);
                    }

                    CurrentPolygon.PointsHasChanged();

                    _pointer.Position = pnew;
                    _currentRectMaker.Position = pnew;
                }

            _previousMouseLocation = e.Location;
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Gets called when any button on the mouse gets released
        /// </summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _isMouseDown = !_isMouseDown;

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Gets called when any button on the mouse gets held down
        /// </summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _pointer.Position = FromLocalToLatLng(e.X, e.Y);
                _isMouseDown = true;

                if (IsDrawingPolygon)
                    DrawNewPolygonPoint();

                if (!IsMouseOverPolygon && !IsMouseOverMarker)
                    ClearSelection();
            }
            else if (IsDrawingPolygon && e.Button == MouseButtons.Right)
            {
                drawPolygonCtxMenu.Show(this, new Point(e.X, e.Y));
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Gets called on double click on the map
        /// </summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            if (CurrentPolygon != null && CurrentPolygon.IsMouseOver)
                new EditZoneForm((PolyZone)CurrentPolygon.Tag).Show();
        }

        /// <summary>
        /// Gets called when the user presses any key while the map being in focus
        /// </summary>
        /// <param name="e">K/B event arguments</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!IsDrawingPolygon) return;

            switch (e.KeyCode)
            {
                case Keys.Escape:
                    EndDrawPolygon(false);
                    break;

                case Keys.Enter:
                    EndDrawPolygon(true);
                    break;
            }
        }

        #endregion Overrides

        #region App logic

        /// <summary>
        /// Gets called when both draw mode and ismousedown conditions meet
        /// </summary>
        private void DrawNewPolygonPoint()
        {
            if (_currentDrawingPolygon == null)
            {
                var points = new List<PointLatLng> { _pointer.Position };
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

        /// <summary>
        /// Gets called when a new polygon is about to be drawn on the GUI
        /// Aka. Draw mode start
        /// </summary>
        public void BeginDrawPolygon()
        {
            IsDrawingPolygon = true;
        }

        /// <summary>
        /// Gets called when a new polygon has been drawn or discarded
        /// Aka. Draw mode end
        /// <param name="save">Indicates whether to save or discard the current drawing polygon</param>
        /// </summary>
        private void EndDrawPolygon(bool save)
        {
            IsDrawingPolygon = false;

            if (save)
            {
                _currentDrawingPolygon.Tag = new PolyZone
                {
                    Geometry = new List<Geometry>(),
                    Id = "",
                    Zoneid = "",
                    Color = ColorTranslator.ToHtml((_currentDrawingPolygon.Fill as SolidBrush)?.Color ?? Color.Black)
                };

                ((PolyZone)_currentDrawingPolygon.Tag).Geometry.AddRange(
                    _currentDrawingPolygon.Points.Select(x => x.ToGeometry())
                );

                Map_OnPolygonClick(Polygons.Polygons.FirstOrDefault(polygon => polygon == _currentDrawingPolygon), null);
            }
            else
            {
                Polygons.Polygons.Remove(_currentDrawingPolygon);
            }

            OnDrawPolygonEnd?.Invoke(_currentDrawingPolygon);

            _currentDrawingPolygon = null;
        }

        /// <summary>
        /// Removes a polygon from the map
        /// </summary>
        /// <param name="p">The polygon to be removed</param>
        public void RemovePolygon(Polygon p)
        {
            if (IsDrawingPolygon)
            {
                EndDrawPolygon(false);
            }
            else if (p != null)
            {
                var iPolygon = Polygons.Polygons.IndexOf(p);

                if (iPolygon > -1)
                {
                    Polygons.Polygons.RemoveAt(iPolygon);
                    ClearSelection();
                }
            }
        }

        /// <summary>
        /// Clears the selected polygon on the map
        /// </summary>
        private void ClearSelection()
        {
            if (CurrentPolygon == null) return;

            var polygonColor = ColorTranslator.FromHtml(((PolyZone)CurrentPolygon.Tag).Color);
            CurrentPolygon.Stroke = new Pen(polygonColor) { Width = 2 };
            CurrentPolygon.Fill = new SolidBrush(Color.FromArgb(60, polygonColor));
            CurrentPolygon.IsSelected = false;

            PolygonRects.Markers.Clear();
        }

        /// <summary>
        /// Selects a polygon on the map
        /// </summary>
        /// <param name="p"></param>
        private void SelectPolygon(Polygon p)
        {
            ClearSelection();

            p.Stroke = _selectedStrokeClr;
            p.Fill = _selecteFillClr;

            CurrentPolygon = p;
            CurrentPolygon.IsSelected = true;

            for (var i = 0; i < p.Points.Count; i++)
            {
                var mBorders = new RectMarker(p.Points[i]) { Tag = i };
                PolygonRects.Markers.Add(mBorders);
            }
        }

        /// <summary>
        /// Loads polygon data and constructs Polygon objects that GMap.NET will use
        /// to display and draw the map control
        /// </summary>
        public async void LoadPolygons()
        {

            var updateWasSuccessfull = await IoHandler.Instance.UpdateAsync(true, true);
            Debug.Write(!updateWasSuccessfull ? "Update failed\n" : "Successfull update\n");

            _fromJsonData = Dto2Object.FromJson(File.ReadAllText("data"));

            if (Polygons.Polygons.Count > 0)
                Polygons.Clear();

            foreach (var zone in _fromJsonData.Zones)
            {
                // TODO: Avoid too much data being shown on the map
                //if (!zone.Id.Contains("BUDAPEST"))
                //    continue;

                var polygonPoints = zone.Geometry.Select(m => new PointLatLng(m.Lat, m.Lng)).ToList();
                var polygonColor = ColorTranslator.FromHtml(zone.Color);

                Polygons.Polygons.Add(new Polygon(polygonPoints, zone.Description)
                {
                    Tag = zone,
                    IsHitTestVisible = true,
                    Fill = new SolidBrush(Color.FromArgb(60, polygonColor)),
                    Stroke = new Pen(polygonColor) { Width = 2 }
                });
            }
        }

        /// <summary>
        /// Converts a GPoint type object to a Point type one
        /// </summary>
        /// <param name="p">GPoint type object</param>
        /// <returns></returns>
        public Point ConvertGPointToPoint(GPoint p)
        {
            return new Point((int)p.X, (int)p.Y);
        }

        #endregion App logic
    }
}