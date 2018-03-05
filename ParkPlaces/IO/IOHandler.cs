using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ParkPlaces.DotUtils.Net;
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace ParkPlaces.IO
{
    public class IoHandler
    {
        private NemApi _api;
        public static IoHandler Instance => _instance ?? (_instance = new IoHandler());
        public static string DataFile => _dataFile;
        public bool NeedUpdate => GetUpdateNeeded();

        private static IoHandler _instance;

        private DateTime _lastUpdate;
        private DateTime _nextUpdate;
        private int _updateInterval;
        private static string _dataFile;

        private bool GetUpdateNeeded()
        {
            return DateTime.Now >= _nextUpdate || !File.Exists("data");
        }

        private async Task<NemApi> GetApiAsync() => _api ?? await NemApi.CreateApi();

        public EventHandler<UpdateProcessChangedArgs> OnUpdateChangedEventHandler;

        private IoHandler()
        {
            _dataFile = "";
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
            if (!GetUpdateNeeded() && !forceUpdate)
            {
                return null;
            }
            var dto = new Dto2Object
            {
                Type = "ZoneCollection",
                Zones = new List<PolyZone>()
            };

            OnUpdateChangedEventHandler?.Invoke(this, new UpdateProcessChangedArgs(0));

            var api = await GetApiAsync();
            var cityTasks = api.Cities.Select(x => api.GetCityPlan<List<PolyZone>>(x)).ToList();
            var cProcess = new UpdateProcessChangedArgs(cityTasks.Count);

            while (cityTasks.Count > 0)
            {
                var res = await Task.WhenAny(cityTasks);
                cityTasks.Remove(res);

                cProcess.UpdateChunks(cProcess.TotalChunks - cityTasks.Count);

                dto.Zones.AddRange(await res);
                OnUpdateChangedEventHandler?.Invoke(this, cProcess);
            }

            _lastUpdate = DateTime.Now;

            if (saveToDisk)
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["LastUpdate"].Value = _lastUpdate.ToBinary().ToString();
                config.Save();
                ConfigurationManager.RefreshSection("appSettings");

                _dataFile = $"geo{_lastUpdate.ToBinary()}.json";
                WriteDtoToJson(_dataFile, dto);
            }

            return dto;
        }

        public static Dto2Object ReadDtoFromJson(string file)
        {
            try
            {
                return Dto2Object.FromJson(File.ReadAllText(file));
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException || e is IOException)
                {
                    throw;
                }
                return null;
            }
        }

        public static void WriteDtoToJson(string file, Dto2Object dto)
        {
            File.WriteAllText(file, dto.ToJson());
        }
    }
}