using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using ParkPlaces.DotUtils.Extensions;
using ParkPlaces.DotUtils.Net;

namespace ParkPlaces.IO
{
    public class IoHandler
    {
        private NemApi _api;
        public static IoHandler Instance => _instance ?? (_instance = new IoHandler());
        private static IoHandler _instance;

        private DateTime _lastUpdate;
        private DateTime _nextUpdate;
        private int _updateInterval;
        private bool _needUpdate => DateTime.Now >= _nextUpdate;

        private IoHandler()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["UpdateInterval"], out var updateInterval))
            {
                _updateInterval = updateInterval;
            }

            if (long.TryParse(ConfigurationManager.AppSettings["LastUpdate"], out var lastUpdate))
            {
                _lastUpdate = new DateTime(lastUpdate);
                _nextUpdate = _lastUpdate.AddDays(_updateInterval);
            }
            else
            {
                _lastUpdate = new DateTime(0);
                _nextUpdate = DateTime.Now;
            }
        }

        private async Task<NemApi> GetApiAsync()
        {
            return _api ?? await NemApi.CreateApi();
        }

        public async Task<bool> UpdateAsync(bool saveToDisk = false, bool forceUpdate = false)
        {
            if (!_instance._needUpdate && !forceUpdate)
            {
                return false;
            }

            var api = await _instance.GetApiAsync();
            var cities = await api.Cities
                .ParallelForEachTaskAsync(async x => await api.GetCityPlan<List<PolyZone>>(x));

            var dto = new Dto2Object
            {
                Type = "ZoneCollection",
                Zones = cities.SelectMany(m => m).ToList()
            };

            _instance._lastUpdate = DateTime.Now;
            ConfigurationManager.AppSettings["LastUpdate"] = _instance._lastUpdate.ToLongTimeString();

            if (saveToDisk)
            {
                File.WriteAllText("data", dto.ToJson());
            }

            return true;
        }
    }
}