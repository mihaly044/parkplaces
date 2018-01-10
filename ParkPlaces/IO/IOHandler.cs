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

        private async Task<NemApi> GetApiAsync() => _api ?? await NemApi.CreateApi();

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

        public async Task<bool> UpdateAsync(bool saveToDisk = false, bool forceUpdate = false)
        {
            if (!_needUpdate && !forceUpdate)
            {
                return false;
            }
            var dto = new Dto2Object
            {
                Type = "ZoneCollection",
                Zones = new List<PolyZone>()
            };

            var api = await GetApiAsync();
            var cityTasks = api.Cities.Select(x => api.GetCityPlan<List<PolyZone>>(x)).ToList();

            while (cityTasks.Count > 0)
            {
                var res = await Task.WhenAny(cityTasks);
                cityTasks.Remove(res);
                dto.Zones.AddRange(await res);
                //Put in response how many have been downloaded so far....
            }

            _lastUpdate = DateTime.Now;
            ConfigurationManager.AppSettings["LastUpdate"] = _lastUpdate.ToLongTimeString();

            if (saveToDisk)
            {
                File.WriteAllText("data", dto.ToJson());
            }

            return true;
        }
    }
}