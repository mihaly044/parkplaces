using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ParkPlaces.DotUtils.Net;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace ParkPlaces.IO
{
    public class IoHandler
    {
#pragma warning disable 649
        private NemApi _api;
#pragma warning restore 649
        public static IoHandler Instance => _instance ?? (_instance = new IoHandler());
        // ReSharper disable once MemberCanBePrivate.Global
        public static string DataFile { get; private set; }

        public bool NeedUpdate => GetUpdateNeeded();

        private static IoHandler _instance;

        private DateTime _lastUpdate;
        private DateTime _nextUpdate;
        private int _updateInterval;

        private bool GetUpdateNeeded()
        {
            return DateTime.Now >= _nextUpdate || !File.Exists("data");
        }

        private async Task<NemApi> GetApiAsync() => _api ?? await NemApi.CreateApi();

        public EventHandler<UpdateProcessChangedArgs> OnUpdateChangedEventHandler;

        private IoHandler()
        {
            DataFile = "";
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

        /// <summary>
        /// Download zone data from nemzetimobilfizetes.hu
        /// </summary>
        /// <param name="saveToDisk">Specifies if the downloaded data should be saved into a file
        /// for further usage</param>
        /// <param name="forceUpdate">Force the download even if the last downloaded data's age does not
        /// exceed <see cref="_updateInterval"/>days</param>
        /// <returns></returns>
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

                DataFile = $"geo{_lastUpdate.ToBinary()}.json";
                WriteDtoToJson(DataFile, dto);
            }

            return dto;
        }

        /// <summary>
        /// Read a Dto2Object from a JSON file
        /// </summary>
        /// <param name="file">Full location of the json file</param>
        /// <returns></returns>
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

        /// <summary>
        /// Serialize all zones into a JSON file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dto"></param>
        public static void WriteDtoToJson(string file, Dto2Object dto)
        {
            File.WriteAllText(file, dto.ToJson());
        }
    }
}