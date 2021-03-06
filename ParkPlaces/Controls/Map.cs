﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using ParkPlaces.Forms;
using ParkPlaces.Map_shapes;
using PPNetClient;
using PPNetLib.Contracts;
using Newtonsoft.Json;
using ParkPlaces.Extensions;
using PPNetLib.Contracts.SynchroniseAcks;
using PPNetLib.Prototypes;

namespace ParkPlaces.Controls
{
    // TODO: Make Map class independent of Client
    public partial class Map : GMapControl
    {
        #region Private fields and consts

        /// <summary>
        /// Indicates whether the Zones on the map should be
        /// editable or not
        /// </summary>
        private bool _readOnly;

         /// <summary>
        /// Represents the initial center point of the map
        /// This remains the same even if the map gets dragged
        /// </summary>
        private readonly GMap.NET.PointLatLng _centerOfTheMap;

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
        private RectMarker _currentRectMarker;

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
        private GMap.NET.PointLatLng _previousMouseLocation;

        /// <summary>
        /// Used for waiting for server responses
        /// </summary>
        private ManualResetEvent _manualResetEvent;

        /// <summary>
        /// Indicates whether the client is waiting for server response
        /// </summary>
        private bool _isWaitingForResponse;
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

        /// <summary>
        /// Invvoked after dragging of a polygon or pointh has finished
        /// </summary>
        public delegate void DragEnd(Polygon polygon);

        /// <summary>
        /// Invoked when the client starts waiting for a server response
        /// </summary>
        public delegate void WaitingStart();

        /// <summary>
        /// Invoked after the client has stopped waiting for a server response
        /// </summary>
        public delegate void WaitingStop();


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
            _readOnly = false;

            _pointer = new GMarkerGoogle(Position, GMarkerGoogleType.arrow) { IsHitTestVisible = false };
            _topLayer.Markers.Add(_pointer);

            var deleteCacheDate = DateTime.Now;
            deleteCacheDate = deleteCacheDate.AddDays(-10);
            Manager.PrimaryCache.DeleteOlderThan(deleteCacheDate, null);

            _centerOfTheMap = new GMap.NET.PointLatLng(47.49801, 19.03991);

            drawPolygonCtxMenu.Renderer = _tsRenderer;
            polygonPointCtxMenu.Renderer = _tsRenderer;

            _isWaitingForResponse = false;

            Client.Instance.OnPointUpdatedAck += OnPointUpdatedAck;
            Client.Instance.OnZoneInfoUpdatedAck += OnZoneInfoUpdatedAck;
        }
        #endregion Constructors

        #region Events
        public event DrawPolygonEnd OnDrawPolygonEnd;
        public event VerticlesChanged OnVerticlesChanged;
        public event DragEnd OnDragEnd;
        public event WaitingStart OnStartWaiting;
        public event WaitingStop OnStopWaiting;
        #endregion Events

        /// <summary>
        /// Indicates if the version info string should get rendered at the left top corner of the map
        /// </summary>
        [Category("Map Extension")]
        [DefaultValue(false)]
        public bool DisplayVersionInfo { get; set; }

        /// <summary>
        /// Opacity of zones
        /// </summary>
        [Category("Map Extension")]
        [DefaultValue(60)]
        public int ZoneOpacity { get; set; }

        /// <summary>
        /// Indicates whether a polygon is currently drawing
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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
        /// The very upper overlay of the map
        /// </summary>
        private readonly Layer _topLayer = new Layer("topLayer");

        /// <summary>
        /// Map layer that contains polygons
        /// </summary>
        private readonly Layer _polygons = new Layer("polygons");

        /// <summary>
        /// Map layer that contains rectangle markers
        /// </summary>
        private readonly Layer _polygonRects = new Layer("polygonRects");

        /// <summary>
        /// Version info gets rendered using this font
        /// </summary>
        private readonly Font _blueFont = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular);

        /// <summary>
        /// Workaround for a bug that causes a white line to be rendered
        /// around menus and toolstrips
        /// </summary>
        private readonly TsRenderer _tsRenderer = new TsRenderer();
        #endregion Internals

        #region GMap events

        /// <summary>
        /// Occurs when the user clicks on a polygon
        /// </summary>
        /// <param name="item">The polygon item that has been cliked on</param>
        /// <param name="e"></param>
        private void Map_OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            if (IsDrawingPolygon || item == null || _currentRectMarker != null)
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
                _currentRectMarker = marker;
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
                _currentRectMarker = null;
            }
        }

        #endregion GMap events

        #region Context menu events

        /// <summary>
        /// Occurs when Finalize gets clicked on the context menu that appears in drawing mode
        /// </summary>
        private void finalizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_readOnly) return;

            EndDrawPolygonAsync(true);
        }

        /// <summary>
        /// Occurs when Cancel gets clicked on the context menu that appears in drawing mode
        /// </summary>
        private void cancelToolStripMenuItem_Click(object sender, EventArgs e) => EndDrawPolygonAsync(false);

        /// <summary>
        /// Called when the user clicks on "Delete point" ctx menu of a RectMarker
        /// </summary>
        private void deletePointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_readOnly) return;

            DeletePolygonPoint(CurrentPolygon);
        }

        /// <summary>
        /// Called when the user clicks on "Add point" ctx menu of a RectMarker 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_readOnly) return;

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

                e.Graphics.DrawString(version, _blueFont, Brushes.Blue, new Point(5, 5));
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
            Overlays.Add(_polygons);
            Overlays.Add(_polygonRects);
            Overlays.Add(_topLayer);
        }

        /// <summary>
        /// Gets called when the cursor moves on the map
        /// </summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!_readOnly)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (Equals(_currentRectMarker, null))
                    {
                        _pointer.Position = FromLocalToLatLng(e.X, e.Y);

                        // Handles polygon dragging on the map
                        if (CurrentPolygon != null && CurrentPolygon.IsMouseOver)
                        {
                            for (var i = 0; i < CurrentPolygon.Points.Count; i++)
                            {
                                var pnew = new GMap.NET.PointLatLng(
                                    CurrentPolygon.Points[i].Lat + _pointer.Position.Lat - _previousMouseLocation.Lat,
                                    CurrentPolygon.Points[i].Lng + _pointer.Position.Lng - _previousMouseLocation.Lng
                                );
                                CurrentPolygon.Points[i] = pnew;

                                var zone = (PolyZone) CurrentPolygon.Tag;
                                var id = zone.Geometry[i].Id;

                                zone.Geometry[i] = pnew.ToGeometry(id);
                            }

                            UpdatePolygonLocalPosition(CurrentPolygon);
                        }
                    }
                    // Handles dragging a point of a polygon
                    else if (CurrentPolygon != null)
                    {

                        var pnew = FromLocalToLatLng(e.X, e.Y);

                        var pIndex = (int?) _currentRectMarker.Tag;

                        if (pIndex.HasValue && pIndex < CurrentPolygon.Points.Count)
                        {
                            CurrentPolygon.Points[pIndex.Value] = pnew;
                            var zone = (PolyZone) CurrentPolygon.Tag;

                            // TODO: Find a workaround for waiting for a server response in this
                            // non-asynch method. Currently it causes the gui to hang up 
                            if (_isWaitingForResponse)
                            {
                                while(_isWaitingForResponse)
                                    Thread.Sleep(300);
                            }

                            var id = zone.Geometry[pIndex.Value].Id;

                            zone.Geometry[pIndex.Value] = pnew.ToGeometry(id);
                            UpdatePolygonLocalPosition(CurrentPolygon);

                            _pointer.Position = pnew;
                            CurrentPolygon.PointsHasChanged();
                            _currentRectMarker.Position = pnew;

                            // TODO: Remove real-time update
                            // It is very inefficent
                            //await Task.Run(() => { Sql.Instance.UpdatePoint(zone.Geometry[pIndex.Value]); });
                        }
                    }
                }
                else if (IsDrawingPolygon)
                {
                    // Handles dragging a point of a NEW polygon on the map
                    _currentNewRectMaker.Position = FromLocalToLatLng(e.X, e.Y);

                    _currentDrawingPolygon.Points[_currentDrawingPolygon.Points.Count - 1] =
                        _currentNewRectMaker.Position;
                    UpdatePolygonLocalPosition(_currentDrawingPolygon);
                }

                _previousMouseLocation = FromLocalToLatLng(e.X, e.Y);
            } else if (e.Button == MouseButtons.Left)
            {
                _pointer.Position = FromLocalToLatLng(e.X, e.Y);
            }
            
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Gets called when any button on the mouse is released
        /// </summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = !_isMouseDown;
                if (CurrentPolygon != null)
                {
                    OnDragFinish(CurrentPolygon);
                }

            }

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Gets called when any button on the mouse gets held down
        /// </summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!_readOnly)
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        _pointer.Position = FromLocalToLatLng(e.X, e.Y);
                        _isMouseDown = true;

                        if (IsDrawingPolygon)
                            DrawNewPolygonPoint();

                        if (!IsMouseOverPolygon && !IsMouseOverMarker)
                            ClearSelection();
                        break;
                    case MouseButtons.Right:
                        // Show context menu
                        if (IsDrawingPolygon)
                            drawPolygonCtxMenu.Show(this, new Point(e.X, e.Y));
                        else if (_currentRectMarker != null)
                            polygonPointCtxMenu.Show(this, new Point(e.X, e.Y));
                        break;
                }
            } else if (e.Button == MouseButtons.Left)
                _pointer.Position = FromLocalToLatLng(e.X, e.Y);

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Gets called on double click on the map
        /// </summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnDoubleClick(EventArgs e)
        {
            if (_readOnly) return;

            base.OnDoubleClick(e);

            if (CurrentPolygon == null || !CurrentPolygon.IsMouseOver) return;

            var zone = (PolyZone) CurrentPolygon.Tag;
            var editZoneForm = new EditZoneForm(zone);
            if (editZoneForm.ShowDialog() == DialogResult.OK)
            {
                Client.Instance.Send(new UpdateZoneReq() {Zone = JsonConvert.SerializeObject(editZoneForm.GetZone())});
            }
        }

        /// <summary>
        /// Gets called when the user presses any key while the map being in focus
        /// </summary>
        /// <param name="e">K/B event arguments</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_readOnly) return;

            if (!IsDrawingPolygon) return;

            switch (e.KeyCode)
            {
                case Keys.Escape:
                    EndDrawPolygonAsync(false);
                    break;

                case Keys.Enter:
                    EndDrawPolygonAsync(true);
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
            if (_readOnly) return;

            var points = new List<GMap.NET.PointLatLng> { _pointer.Position };
            _currentDrawingPolygon = new Polygon(points, "New polygon") { IsHitTestVisible = true };
            _polygons.Polygons.Add(_currentDrawingPolygon);

            IsDrawingPolygon = true;
            _currentNewRectMaker = new RectMarker(new GMap.NET.PointLatLng(0, 0));
            _topLayer.Markers.Add(_currentNewRectMaker);
        }

        /// <summary>
        /// Gets called when a new polygon has been drawn or discarded
        /// Aka. Draw mode end
        /// <param name="save">Indicates whether to save or discard the current drawing polygon</param>
        /// </summary>
        private void EndDrawPolygonAsync(bool save)
        {
            IsDrawingPolygon = false;

            if (save && _currentDrawingPolygon.LocalPoints.Count - 1 > 1 && _currentDrawingPolygon.GetArea(true) > 0d)
            {
                // Remove "ghost" point marker
                _currentDrawingPolygon.Points.Remove(_currentNewRectMaker.Position);
                UpdatePolygonLocalPosition(_currentDrawingPolygon);

                var newZone = new PolyZone
                {
                    Geometry = new List<Geometry>(),
                    Id = "",
                    Zoneid = "",
                    ServiceNa = "Service provider",
                    Description = "New zone",
                    Timetable = "",
                    Color = ColorTranslator.ToHtml((_currentDrawingPolygon.Fill as SolidBrush)?.Color ?? Color.Black)
                };

                var editZoneForm = new EditZoneForm(newZone);
                if (editZoneForm.ShowDialog() == DialogResult.OK)
                {
                    newZone = editZoneForm.GetZone();
                    newZone.Geometry.AddRange(
                        _currentDrawingPolygon.Points.Select(x => x.ToGeometry(0))
                    );

                    var zoneSeriliazed = JsonConvert.SerializeObject(newZone, Converter.Settings);
                    Client.Instance.Send(new InsertZoneReq() { Zone = zoneSeriliazed });
                    StartWaiting();

                    Client.Instance.OnZoneInsertAck += (ack) => {
                        newZone.Id = ack.ZoneId.ToString();

                        // Set newly inserted zone's point ids
                        for (var i = 0; i < newZone.Geometry.Count; i++)
                        {
                            newZone.Geometry[i].Id = ack.PointIds[i];
                        }

                        StopWaiting();
                    };

                    _currentDrawingPolygon.Tag = newZone;
                    _dtoObject.Zones.Add(newZone);
                }
                else
                {
                    _polygons.Polygons.Remove(_currentDrawingPolygon);
                }

                Map_OnPolygonClick(_polygons.Polygons.FirstOrDefault(polygon => polygon == _currentDrawingPolygon), null);
            }
            else
            {
                _polygons.Polygons.Remove(_currentDrawingPolygon);
            }

            OnDrawPolygonEnd?.Invoke(_currentDrawingPolygon);

            _currentDrawingPolygon = null;

            _topLayer.Markers.Remove(_currentNewRectMaker);
            _currentNewRectMaker = null;

            UpdateVerticlesCount();
        }

        /// <summary>
        /// Add a new verticle to a given polygon
        /// </summary>
        /// <param name="p"></param>
        // ReSharper disable once MemberCanBePrivate.Global
        public async void AddPolygonPoint(Polygon p)
        {
            var pIndex = (int?)_currentRectMarker?.Tag;
            if (pIndex.HasValue)
            {
                p.Points.Insert(pIndex.Value, _currentRectMarker.Position);

                var geometry = _currentRectMarker.Position.ToGeometry(0);

                // Wait for ZoneId to arrivc
                await DoWaiting();

                Client.Instance.Send(new InsertPointReq()
                {
                    Lat = geometry.Lat,
                    Lng = geometry.Lng,
                    ZoneId = p.GetZoneId(),
                    Index = pIndex.Value
                });

                StartWaiting();
                Client.Instance.OnPointInsertAck += (ack) =>
                {
                    geometry.Id = ack.PointId;
                    p.GetZoneInfo().Geometry.Insert(pIndex.Value, geometry);
                    StopWaiting();
                };

                UpdateVerticlesCount();
                UpdatePolygonLocalPosition(p);
                SelectPolygon(p);
            }
        }

        /// <summary>
        /// Delete the current selected point of a polygon
        /// </summary>
        /// <param name="p"></param>
        // ReSharper disable once MemberCanBePrivate.Global
        public async void DeletePolygonPoint(Polygon p)
        {
            var pIndex = (int?) _currentRectMarker?.Tag;
            if(pIndex.HasValue)
            {
                if(p.Points.Count - 1 < 3)
                {
                    if(MessageBox.Show("The polygon you are going to create is going to be open and therefore removed. Continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        RemovePolygon(p);
                    }
                }
                else
                {
                    var zone = (PolyZone)p.Tag;
                    var point = zone.Geometry.ElementAt(pIndex.Value);
                    var zoneId = int.Parse(zone.Id);

                    // Delete from the database, wait for ZoneId to arrive
                    await DoWaiting();
                    Client.Instance.Send(new RemovePointReq()
                    {
                        PointId = point.Id,
                        ZoneId = zoneId,
                        Index = pIndex.Value
                    });

                    // Delete from zone data
                    ((PolyZone)p.Tag).Geometry.Remove(point);

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
                EndDrawPolygonAsync(false);
            }
            else if (p != null)
            {
                var iPolygon = _polygons.Polygons.IndexOf(p);

                if (iPolygon > -1)
                {
                    _polygons.Polygons.RemoveAt(iPolygon);

                    var zoneId = p.GetZoneId();
                    Client.Instance.Send(new RemoveZoneReq(){ZoneId =  zoneId });

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
            CurrentPolygon.Fill = new SolidBrush(Color.FromArgb(ZoneOpacity, polygonColor));
            CurrentPolygon.IsSelected = false;


            _currentRectMarker = null;

            // Clear rect markers
            _polygonRects.Markers.Clear();
            CurrentPolygon = null;
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

            if (_readOnly) return;

            // Create rect markers
            for (var i = 0; i < p.Points.Count; i++)
            {
                var mBorders = new RectMarker(p.Points[i]) { Tag = i };
                _polygonRects.Markers.Add(mBorders);
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

            data.Zones.AddRange(_polygons.Polygons.Select(x => (PolyZone)x.Tag));
            File.WriteAllText(file, JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Loads polygon data
        /// </summary>
        public async void LoadPolygons(bool forceUpdate = false, string file = "")
        {
            /*
            _dtoObject?.Reset();

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

            ConstructGui();*/
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructs Polygon objects that GMap.NET will use
        /// to display and draw the map control
        /// </summary>
        /// <param name="dto">Data transfer object</param>
        public void ConstructGui(Dto2Object dto = null)
        {
            if (dto != null)
                _dtoObject = dto;
            else if (_dtoObject == null) return;


            UnloadSession();
            foreach (var zone in _dtoObject.Zones)
            {
                var polygonPoints = zone.Geometry.Select(m => new GMap.NET.PointLatLng(m.Lat, m.Lng)).ToList();
                var polygonColor = ColorTranslator.FromHtml(zone.Color);

                _polygons.Polygons.Add(new Polygon(polygonPoints, zone.Description)
                {
                    Tag = zone,
                    IsHitTestVisible = true,
                    Fill = new SolidBrush(Color.FromArgb(ZoneOpacity, polygonColor)),
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
        public void UnloadSession(bool resetDto = false)
        {
            if(resetDto)
                _dtoObject?.Reset();

            _polygons.Clear();
            _polygonRects.Clear();
            //TopLayer.Clear();

            ResetCenter();
            UpdateVerticlesCount();
        }
        

        /// <summary>
        /// Resets the map to the original center point
        /// defined by _centerOfTheMap
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void ResetCenter()
        {
            Position = _centerOfTheMap;
            _pointer.Position = _centerOfTheMap;
        }

        /// <summary>
        /// Sets the pointer to a given position
        /// </summary>
        /// <param name="pos"></param>
        public void SetPointerPosition(GMap.NET.PointLatLng pos)
        {
            _pointer.Position = pos;
        }


        /// <summary>
        /// Drags the center of the map to a given position
        /// </summary>
        /// <param name="pos"></param>
        public void SetMapPosition(GMap.NET.PointLatLng pos)
        {
            Position = pos;
        }

        /// <summary>
        /// Calculate total verticle count and invoke VerticlesChanged
        /// </summary>
        private void UpdateVerticlesCount()
        {
            OnVerticlesChanged?.Invoke(new VerticleChangedArg(
                _polygons.Polygons.Sum(p => p.Points.Count),
                _polygons.Polygons.Count
                ));
        }

        /// <summary>
        /// Set whether the map should be readonly
        /// </summary>
        /// <param name="readOnly"></param>
        public void SetReadOnly(bool readOnly)
        {
            _readOnly = readOnly;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected void OnDragFinish(Polygon polygon)
        {
            OnDragEnd?.Invoke(polygon);
            UpdatePointsAsync(CurrentPolygon);
        }

        /// <summary>
        /// Send update REQs to the server
        /// </summary>
        /// <param name="p"></param>
        private async void UpdatePointsAsync(Polygon p)
        {
            var zone = p.GetZoneInfo();

            await DoWaiting();

            var i = 0;
            foreach (var geometry in zone.Geometry)
            {
                if (geometry.IsModified)
                {
                    zone.Geometry[i].IsModified = false;
                    Client.Instance.Send(new UpdatePointReq()
                    {
                        PointId = geometry.Id,
                        Lat = geometry.Lat,
                        Lng = geometry.Lng,
                        ZoneId = int.Parse(zone.Id)
                    });
                }
                i++;
            }
        }

        /// <summary>
        /// Called on POINTUPDATED_ACK, whenever a point changes
        /// in some other running client. Updates local points respectively
        /// </summary>
        /// <param name="packet"></param>
        private void OnPointUpdatedAck(PointUpdatedAck packet)
        {
            foreach (var polygon in _polygons.Polygons)
            {
                if (!(polygon.Tag is PolyZone zone)) continue;
                if (int.Parse(zone.Id) != packet.ZoneId) continue;

                if (packet.Added)
                {
                    var pointLatLng = new GMap.NET.PointLatLng(packet.Lat, packet.Lng);
                    polygon.Points.Insert(packet.Index, pointLatLng);
                    zone.Geometry.Insert(packet.Index, pointLatLng.ToGeometry(packet.PointId));
                }
                else if (packet.Removed)
                {
                    polygon.Points.RemoveAt(packet.Index);
                    zone.Geometry.RemoveAt(packet.Index);
                }
                else
                {
                    var i = zone.Geometry.FindIndex(x => x.Id == packet.PointId);
                    var pt = new GMap.NET.PointLatLng(packet.Lat, packet.Lng);
                    polygon.Points[i] = pt;
                    zone.Geometry[i] = pt.ToGeometry(packet.PointId);
                }

                UpdatePolygonLocalPosition(polygon);
                Refresh();
                break;
            }
        }

        /// <summary>
        /// Called on ZONEINFOUPDATED_ACK
        /// </summary>
        /// <param name="packet"></param>
        private void OnZoneInfoUpdatedAck(ZoneInfoUpdatedAck packet)
        {
            if (packet.Added)
            {
                var deserializedZone = JsonConvert.DeserializeObject<PolyZone>(packet.Data);

                var polygonPoints = deserializedZone.Geometry.Select(m => new GMap.NET.PointLatLng(m.Lat, m.Lng)).ToList();
                var polygonColor = ColorTranslator.FromHtml(deserializedZone.Color);

                _polygons.Polygons.Add(new Polygon(polygonPoints, deserializedZone.Description)
                {
                    Tag = deserializedZone,
                    IsHitTestVisible = true,
                    Fill = new SolidBrush(Color.FromArgb(ZoneOpacity, polygonColor)),
                    Stroke = new Pen(polygonColor) {Width = 2}
                });
            }
            else
            {
                foreach (var polygon in _polygons.Polygons)
                {
                    if (!(polygon.Tag is PolyZone zone)) continue;
                    if (int.Parse(zone.Id) != packet.ZoneId) continue;

                    if (packet.Removed)
                    {
                        _polygons.Polygons.Remove(polygon);
                        _dtoObject.Zones.Remove(zone);
                    }
                    else
                    {
                        var deserializedZone = JsonConvert.DeserializeObject<PolyZone>(packet.Data);
                        polygon.Tag = deserializedZone;

                        var polygonColor = ColorTranslator.FromHtml(deserializedZone.Color);
                        polygon.Fill = new SolidBrush(Color.FromArgb(ZoneOpacity, polygonColor));
                        polygon.Stroke = new Pen(polygonColor) {Width = 2};
                    }

                    UpdatePolygonLocalPosition(polygon);
                    break;
                }
            }

            Refresh();
            UpdateVerticlesCount();
        }

        /// <summary>
        /// Indicate that the client will be waiting for a response
        /// from the server
        /// </summary>
        private void StartWaiting()
        {
            _manualResetEvent = new ManualResetEvent(false);
            _isWaitingForResponse = true;
            OnStartWaiting?.Invoke();
        }

        /// <summary>
        /// Stop waiting for a server response
        /// </summary>
        private void StopWaiting()
        {
            _manualResetEvent.Set();
            _isWaitingForResponse = false;
            OnStopWaiting?.Invoke();
        }

        /// <summary>
        /// Checks whether the client is waiting for a server response
        /// </summary>
        /// <returns></returns>
        public bool IsWaiting()
        {
            return _isWaitingForResponse;
        }

        /// <summary>
        /// Do waiting for a server response
        /// </summary>
        /// <returns></returns>
        public async Task DoWaiting()
        {
            if (_isWaitingForResponse)
            {
                await Task.Run(() =>
                {
                    _manualResetEvent.WaitOne();
                });
            }
        }

        #endregion App logic
    }
}