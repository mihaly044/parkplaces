using System;
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

namespace ParkPlaces.Controls
{
    public partial class Map : GMapControl
    {

        #region Private fields and consts
         /// <summary>
        /// Represents the initial center point of the map
        /// This remains the same even if the map gets dragged
        /// </summary>
        private readonly PointLatLng _centerOfTheMap;

        /// <summary>
        /// Defines the stroke colour of map shapes in selected state
        /// </summary>
        private readonly Pen _selectedStrokeClr;

        /// <summary>
        /// Defines the fill colour of map shapes in selected state
        /// </summary>
        private readonly Brush _selecteFillClr;

        /// <summary>
        /// Indicates whether the left mouse button is down at a given moment
        /// </summary>
        private bool _isMouseDown;

        /// <summary>
        /// Represents a green marker on the map
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
        /// Represents a new point of a polygon that is being drawn
        /// </summary>
        private RectMarker _currentNewRectMaker;

        /// <summary>
        /// Represents a polygon that is currently in the process of drawing on the GUI
        /// </summary>
        private Polygon _currentDrawingPolygon;

        /// <summary>
        /// Represents the current data transfer object in use
        /// </summary>
        private Dto2Object _dtoObject;

        /// <summary>
        /// Used to get the previous mouse location on every OnMouseMove call
        /// </summary>
        private PointLatLng _previousMouseLocation;
        #endregion

        /// <summary>
        /// Invoked after a new polygon gets drawn on screen
        /// </summary>
        /// <param name="polygon">The polygon that the user has finished drawing</param>
        public delegate void DrawPolygonEnd(Polygon polygon);

        /// <summary>
        /// Invoked when either total shape or its verticles' count changes
        /// </summary>
        /// <param name="verticleChangedArg">Contains total verticle and shapes count</param>
        public delegate void VerticlesChanged(VerticleChangedArg verticleChangedArg);


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

            var deleteCacheDate = DateTime.Now;
            deleteCacheDate = deleteCacheDate.AddDays(-10);
            Manager.PrimaryCache.DeleteOlderThan(deleteCacheDate, null);

            _centerOfTheMap = new PointLatLng(47.49801, 19.03991);

            drawPolygonCtxMenu.Renderer = TsRenderer;
            polygonPointCtxMenu.Renderer = TsRenderer;
        }

        #endregion Constructors

        #region Events
        public event DrawPolygonEnd OnDrawPolygonEnd;
        public event VerticlesChanged OnVerticlesChanged;
        #endregion Events

        /// <summary>
        /// Indicates if the version info string should get rendered at the left top corner of the map
        /// </summary>
        [Category("Map Extension")]
        [DefaultValue(false)]
        public bool DisplayVersionInfo { get; set; }

        /// <summary>
        /// Indicates whether a polygon is currently drawing
        /// </summary>
        public bool IsDrawingPolygon { get; set; }

        public Dto2Object GetDataObject() => _dtoObject;

        /// <summary>
        /// Represents the time when rendering starts
        /// </summary>
        private DateTime _renderStart;

        /// <summary>
        /// Represents time when rendering stops
        /// </summary>
        private DateTime _renderEnd;

        /// <summary>
        /// The time it took to render a frame
        /// in milliseconds
        /// </summary>
        private int _renderDelta;

        #region Internals

        /// <summary>
        /// The very upper overlayof the map
        /// </summary>
        internal readonly Layer TopLayer = new Layer("topLayer");

        /// <summary>
        /// Map layer that contains polygons
        /// </summary>
        internal readonly Layer Polygons = new Layer("polygons");

        /// <summary>
        /// Map layer that contains rectangle markers
        /// </summary>
        internal readonly Layer PolygonRects = new Layer("polygonRects");

        /// <summary>
        /// Version info gets rendered using this font
        /// </summary>
        internal readonly Font BlueFont = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular);

        /// <summary>
        /// Workaround for a bug that causes a white line to be rendered
        /// around menus and toolstrips
        /// </summary>
        internal readonly TsRenderer TsRenderer = new TsRenderer();
        #endregion Internals

        #region GMap events

        /// <summary>
        /// Occurs when the user clicks on a polygon
        /// </summary>
        /// <param name="item">The polygon item that has been cliked on</param>
        /// <param name="e"></param>
        private void Map_OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            if (IsDrawingPolygon || item == null || _currentRectMaker != null)
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
            if (IsDrawingPolygon) return;

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
            if (IsDrawingPolygon) return;

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

        /// <summary>
        /// Occurs when Cancel gets clicked on the context menu that appears in drawing mode
        /// </summary>
        private void cancelToolStripMenuItem_Click(object sender, EventArgs e) => EndDrawPolygon(false);

        /// <summary>
        /// Called when the user clicks on "Delete point" ctx menu of a RectMarker
        /// </summary>
        private void deletePointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeletePolygonPoint(CurrentPolygon);
        }

        /// <summary>
        /// Called when the user clicks on "Add point" ctx menu of a RectMarker 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddPolygonPoint(CurrentPolygon);
        }

        #endregion Context menu events

        #region Overrides

        /// <summary>
        /// Gets called when the map gets painted
        /// </summary>
        /// <param name="e">Graphics context</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Calculate render time
            _renderStart = DateTime.Now;
            base.OnPaint(e);
            _renderEnd = DateTime.Now;
            _renderDelta = (int)(_renderEnd - _renderStart).TotalMilliseconds;


            if (DisplayVersionInfo)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

#if DEBUG
                var version =
                    $"Debug version {fvi.FileVersion}, OS: {Environment.OSVersion}, .NET v{Environment.Version}, Render: {_renderDelta} ms";
#else
                var version =
                    $"Release version {fvi.FileVersion}, OS: {Environment.OSVersion}, .NET v{Environment.Version}, Render: {_renderDelta} ms";
#endif

                e.Graphics.DrawString(version, BlueFont, Brushes.Blue, new Point(5, 5));
            }
        }

        /// <summary>
        /// Gets called when the control gets loaded
        /// Sets up an initial state
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MapProvider = GMapProviders.GoogleMap;
            Position = _centerOfTheMap;
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
            if (e.Button == MouseButtons.Left)
            {
                if (_currentRectMaker == null)
                {
                    _pointer.Position = FromLocalToLatLng(e.X, e.Y);

                    // Handles polygon dragging on the map
                    if (CurrentPolygon != null && CurrentPolygon.IsMouseOver)
                    {
                        for (int i = 0; i < CurrentPolygon.Points.Count; i++)
                        {
                            var pnew = new PointLatLng(
                                CurrentPolygon.Points[i].Lat + _pointer.Position.Lat - _previousMouseLocation.Lat,
                                CurrentPolygon.Points[i].Lng + _pointer.Position.Lng - _previousMouseLocation.Lng
                            );
                            CurrentPolygon.Points[i] = pnew;

                            ((PolyZone)CurrentPolygon.Tag).Geometry[i] = pnew.ToGeometry();
                        }

                        UpdatePolygonLocalPosition(CurrentPolygon);
                    }
                }
                // Handles dragging a point of a polygon
                else if(CurrentPolygon != null)
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
            }
            else if (IsDrawingPolygon)
            {
                // Handles dragging a point of a NEW polygon on the map
                _currentNewRectMaker.Position = FromLocalToLatLng(e.X, e.Y);

                _currentDrawingPolygon.Points[_currentDrawingPolygon.Points.Count - 1] = _currentNewRectMaker.Position;
                UpdatePolygonLocalPosition(_currentDrawingPolygon);
            }

            _previousMouseLocation = FromLocalToLatLng(e.X, e.Y);
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Gets called when any button on the mouse is released
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
            else if(e.Button == MouseButtons.Right)
            {
                // Show context menu
                if (IsDrawingPolygon)
                    drawPolygonCtxMenu.Show(this, new Point(e.X, e.Y));
                else if(_currentRectMaker != null)
                    polygonPointCtxMenu.Show(this, new Point(e.X, e.Y));
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

            // Remove "ghost" point marker that is used to show the user
            // where the next point is going to be
            _currentDrawingPolygon.Points.Remove(_currentNewRectMaker.Position);

            // Add the actual point to the current polygon
            _currentDrawingPolygon.Points.Add(_pointer.Position);
            _currentDrawingPolygon.PointsHasChanged();

            // Add the "ghost" point marker
            _currentDrawingPolygon.Points.Add(_currentNewRectMaker.Position);

            UpdatePolygonLocalPosition(_currentDrawingPolygon);
            UpdateVerticlesCount();
        }

        /// <summary>
        /// Gets called when a new polygon is about to be drawn on the GUI
        /// Aka. Draw mode start
        /// </summary>
        public void BeginDrawPolygon()
        {
            var points = new List<PointLatLng> { _pointer.Position };
            _currentDrawingPolygon = new Polygon(points, "New polygon") { IsHitTestVisible = true };
            Polygons.Polygons.Add(_currentDrawingPolygon);

            IsDrawingPolygon = true;
            _currentNewRectMaker = new RectMarker(new PointLatLng(0, 0));
            TopLayer.Markers.Add(_currentNewRectMaker);
        }

        /// <summary>
        /// Gets called when a new polygon has been drawn or discarded
        /// Aka. Draw mode end
        /// <param name="save">Indicates whether to save or discard the current drawing polygon</param>
        /// </summary>
        private void EndDrawPolygon(bool save)
        {
            IsDrawingPolygon = false;

            if (save && _currentDrawingPolygon.LocalPoints.Count - 1 > 1 && _currentDrawingPolygon.GetArea(true) > 0d)
            {
                // Remove "ghost" point marker
                _currentDrawingPolygon.Points.Remove(_currentNewRectMaker.Position);
                UpdatePolygonLocalPosition(_currentDrawingPolygon);

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

            TopLayer.Markers.Remove(_currentNewRectMaker);
            _currentNewRectMaker = null;

            UpdateVerticlesCount();
        }

        /// <summary>
        /// Add a new verticle to a given polygon
        /// </summary>
        /// <param name="p"></param>
        public void AddPolygonPoint(Polygon p)
        {
            var pIndex = (int?)_currentRectMaker?.Tag;
            if (pIndex.HasValue)
            {
                p.Points.Insert(pIndex.Value, _currentRectMaker.Position);
                ((PolyZone)p.Tag).Geometry.Insert(pIndex.Value, _currentRectMaker.Position.ToGeometry());

                UpdateVerticlesCount();
                UpdatePolygonLocalPosition(p);
                SelectPolygon(p);
            }
        }

        /// <summary>
        /// Delete the current selected point of a polygon
        /// </summary>
        /// <param name="p"></param>
        public void DeletePolygonPoint(Polygon p)
        {
            var pIndex = (int?) _currentRectMaker?.Tag;
            if(pIndex.HasValue)
            {
                if(p.Points.Count - 1 < 3)
                {
                    if(MessageBox.Show("The resulting shape won't be closed therefore all of its points will get deleted. Continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        RemovePolygon(p);
                    }
                }
                else
                {
                    // Delete from zone data
                    ((PolyZone)p.Tag).Geometry.RemoveAt(pIndex.Value);
                    // Delete from local data
                    p.Points.RemoveAt(pIndex.Value);
                    UpdatePolygonLocalPosition(p);
                }
                UpdateVerticlesCount();
                SelectPolygon(p);
            }
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
            UpdateVerticlesCount();
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


            _currentRectMaker = null;

            // Clear rect markers
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

            // Create rect markers
            for (var i = 0; i < p.Points.Count; i++)
            {
                var mBorders = new RectMarker(p.Points[i]) { Tag = i };
                PolygonRects.Markers.Add(mBorders);
            }
        }

        /// <summary>
        /// Save polygon data to a file
        /// </summary>
        /// <param name="file"></param>
        public void SavePolygons(string file)
        {
            var data = new Dto2Object()
            {
                Type = "ZoneCollection",
                Zones = new List<PolyZone>()
            };

            data.Zones.AddRange(Polygons.Polygons.Select(x => (PolyZone)x.Tag));

            IoHandler.WriteDtoToJson(file, data);
        }

        /// <summary>
        /// Loads polygon data and constructs Polygon objects that GMap.NET will use
        /// to display and draw the map control
        /// </summary>
        public async void LoadPolygons(bool forceUpdate = false, string file = "")
        {
            if (file != string.Empty)
                _dtoObject = IoHandler.ReadDtoFromJson(file);
            else if (forceUpdate)
            {
                _dtoObject = await IoHandler.Instance.UpdateAsync(true, true);
            }
            else
            {
                _dtoObject = await IoHandler.Instance.UpdateAsync(true) ??
                                Dto2Object.FromJson(File.ReadAllText("data"));
            }

            ConstructGUI();
        }

        public void ConstructGUI(Dto2Object dto = null)
        {
            if (dto != null)
                _dtoObject = dto;

            UnloadSession();
            foreach (var zone in _dtoObject.Zones)
            {
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
            UpdateVerticlesCount();
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


        /// <summary>
        /// Reset the map to its initial state with no 
        /// shapes and markers present
        /// </summary>
        public void UnloadSession()
        {
            Polygons.Polygons.Clear();
            PolygonRects.Markers.Clear();

            ResetCenter();
            UpdateVerticlesCount();
        }

        /// <summary>
        /// Resets the map to the original center point
        /// defined by _centerOfTheMap
        /// </summary>
        public void ResetCenter()
        {
            Position = _centerOfTheMap;
            _pointer.Position = _centerOfTheMap;
        }

        /// <summary>
        /// Sets the pointer to a given position
        /// </summary>
        /// <param name="pos"></param>
        public void SetPointerPosition(PointLatLng pos)
        {
            _pointer.Position = pos;
        }


        /// <summary>
        /// Drags the center of the map to a given position
        /// </summary>
        /// <param name="pos"></param>
        public void SetMapPosition(PointLatLng pos)
        {
            Position = pos;
        }

        /// <summary>
        /// Calculate total verticle count and invoke VerticlesChanged
        /// </summary>
        private void UpdateVerticlesCount()
        {
            OnVerticlesChanged?.Invoke(new VerticleChangedArg(
                Polygons.Polygons.Sum(p => p.Points.Count),
                Polygons.Polygons.Count
                ));
        }
        #endregion App logic
    }
}