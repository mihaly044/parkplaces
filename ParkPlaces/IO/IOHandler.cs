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
        public bool NeedUpdate => _needUpdate;

        private static IoHandler _instance;

        private DateTime _lastUpdate;
        private DateTime _nextUpdate;
        private int _updateInterval;
        private bool _needUpdate => DateTime.Now >= _nextUpdate;

        private async Task<NemApi> GetApiAsync() => _api ?? await NemApi.CreateApi();

        public EventHandler<UpdateProcessChangedArgs> OnUpdateChangedEventHandler;

        private IoHandler()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["UpdateInterval"], out _updateInterval))
            {
                _updateInterval = 5;
            }

            if (long.TryParse(ConfigurationManager.AppSettings["LastUpdate"], out var lastUpdate))
            {
                _lastUpdate = DateTime.FromBinary(lastUpdate);
                _nextUpdate = _lastUpdate.AddDays(_updateInterval);
            }
            else
            {
                _lastUpdate = new DateTime(0);
                _nextUpdate = DateTime.Now;
            }
        }

        public async Task<Dto2Object> UpdateAsync(bool saveToDisk = false, bool forceUpdate = false)
        {
            if (!_needUpdate && !forceUpdate)
            {
                return null;
            }
            var dto = new Dto2Object
            {
                Type = "ZoneCollection",
                Zones = new List<PolyZone>()
            };

            var api = await GetApiAsync();
            var cityTasks = api.Cities.Select(x => api.GetCityPlan<List<PolyZone>>(x)).ToList();
            var cProcess = new UpdateProcessChangedArgs(cityTasks.Count);

            while (cityTasks.Count > 0)
            {
                var res = await Task.WhenAny(cityTasks);
                cityTasks.Remove(res);

                cProcess.UpdateChunks(cProcess.TotalChunks - cityTasks.Count);

                dto.Zones.AddRange(await res);
                //Put in response how many have been downloaded so far....
                OnUpdateChangedEventHandler?.Invoke(this, cProcess);
            }

            _lastUpdate = DateTime.Now;

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["LastUpdate"].Value = _lastUpdate.ToBinary().ToString();
            config.Save();
            ConfigurationManager.RefreshSection("appSettings");

            if (saveToDisk)
            {
                File.WriteAllText("data", dto.ToJson());
            }

            return dto;
        }
    }
}