using ParkPlaces.IO;
using System;
using System.Windows.Forms;
using ParkPlaces.IO.Database;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using PPNetLib.Contracts;
using ParkPlaces.Net;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
// ReSharper disable RedundantCast

// ReSharper disable ArrangeRedundantParentheses

namespace ParkPlaces.Forms
{
    public partial class LoadingForm : Form
    {
        private Dto2Object _dto;

        /// <summary>
        /// Used for waiting for server acks
        /// </summary>
        private ManualResetEvent _manualResetEvent;

        public EventHandler<Dto2Object> OnReadyEventHandler;
        private int _currentProgress;
        private int _zoneCount;

        public LoadingForm()
        {
            InitializeComponent();
            _currentProgress = 0;

            _manualResetEvent = new ManualResetEvent(false);

            Client.Instance.OnZoneCountAck += OnZoneCountAck;
            Client.Instance.OnZoneListAck += OnZoneListAck;
        }

        public async void LoadDataAsync()
        {
            // Get zones count
            await Task.Run(()=> {
                Client.Instance.Send(new ZoneCountReq());
                _manualResetEvent.WaitOne();
            });
            _manualResetEvent.Reset();

            // Get zones
            _dto = new Dto2Object
            {
                Type = "ZoneCollection",
                Zones = new List<PolyZone>()
            };

            // Wait for all the data to arrive
            await Task.Run(() => {
                _manualResetEvent.WaitOne();
            });
            _manualResetEvent.Reset();

            OnReadyEventHandler?.Invoke(this, _dto);
            Close();
        }

        private void OnZoneCountAck(ZoneCountAck ack)
        {
            Invoke(new Action( () => { MessageBox.Show($"{ack.ZoneCount}"); }));
            _zoneCount = ack.ZoneCount;
            _manualResetEvent.Set();
        }

        private void OnZoneListAck(ZoneListAck ack)
        {
            _currentProgress++;
            progressBar.Value = (_currentProgress / _zoneCount)*100;

            if(_currentProgress >= _zoneCount)
            {
                // We have all the data
                _manualResetEvent.Set();
                return;
            }
            else
            {
                // De-serialize data and add to collection
                var zone = ack.Zone;
                _dto.Zones.Add(JsonConvert.DeserializeObject<PolyZone>(zone, Converter.Settings));
            }
            
        }
    }
}
