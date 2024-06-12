using IPA.Utilities.Async;
using SiraUtil.Logging;
using SiraUtil.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Zenject;

namespace OstBackport.MapSaving
{
    internal class MapSaving : IInitializable
    {
        [Inject] private readonly IHttpService _httpService;
        [Inject] private readonly SiraLog _log;

        public bool IsReady { get; private set; } = false;

        public Action<int, int> MapSavingCallback;

        public bool GetIsReady()
        {
            return IsReady;
        }

        internal const string OST_MAPS_URL = "http://207.211.156.28:3000/OST/";

        internal List<string> OST6IDs = new List<string>()
        {
            "Cathedral",
            "TempoKatana",
            "PowerOfTheSaberBlade",
            "LiftOff",
            "HeavyWeight",
        };

        internal List<string> OST7IDs = new List<string>()
        {
            "Damage",
            "Lustre",
            "TheMaster",
            "Untamed",
            "WorldWideWeb",
        };

        internal async Task<bool> DownloadMapFromId(string id, string ostVersion)
        {
            string url = $"{OST_MAPS_URL}{id}";
            _log.Info($"Downloading map {id} from {url}");
            IHttpResponse httpResponse = await _httpService.GetAsync(url);

            if (httpResponse.Successful)
            {
                _log.Info($"Downloaded map {id} from {url}");
                byte[] bytes = await httpResponse.ReadAsByteArrayAsync();
                string storagePath = Path.Combine(Environment.CurrentDirectory, "UserData", "OstBackport", "Storage");
                string extractionPath = Path.Combine(Environment.CurrentDirectory, "UserData", "OstBackport", ostVersion);

                Directory.CreateDirectory(storagePath);
                Directory.CreateDirectory(extractionPath);

                string zipFilePath = Path.Combine(storagePath, $"{id}.zip");
                File.WriteAllBytes(zipFilePath, bytes);

                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    using (ZipArchive archive = new ZipArchive(memoryStream))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string filePath = Path.Combine(extractionPath, entry.FullName);
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                            if (!string.IsNullOrEmpty(entry.Name))
                            {
                                using (Stream entryStream = entry.Open())
                                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                                {
                                    await entryStream.CopyToAsync(fileStream);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            else
            {
                _log.Error($"Failed to download map {id} from {url}");
                return false;
            }
        }

        internal int GetTotalMapsToDownload()
        {
            int totalMaps = OST6IDs.Count + OST7IDs.Count;
            foreach (string id in OST6IDs)
            {
                string path = Path.Combine(Environment.CurrentDirectory, "UserData", "OstBackport", "OST6", id);
                if (Directory.Exists(path))
                {
                    totalMaps--;
                }
            }
            foreach (string id in OST7IDs)
            {
                string path = Path.Combine(Environment.CurrentDirectory, "UserData", "OstBackport", "OST7", id);
                if (Directory.Exists(path))
                {
                    totalMaps--;
                }
            }
            return totalMaps;
        }

        internal async Task CheckMaps()
        {
            IsReady = false;
            int totalMaps = GetTotalMapsToDownload();
            int downloadedMaps = 0;
            foreach (string id in OST6IDs)
            {
                string path = Path.Combine(Environment.CurrentDirectory, "UserData", "OstBackport", "OST6", id);
                if (!Directory.Exists(path))
                {
                    _log.Notice($"Downloading map {id} for OST6");
                    await DownloadMapFromId(id, "OST6");
                    _log.Notice($"Downloaded map {id} for OST6");
                    downloadedMaps++;
                    MapSavingCallback?.Invoke(downloadedMaps, totalMaps);
                }
            }
            foreach (string id in OST7IDs)
            {
                string path = Path.Combine(Environment.CurrentDirectory, "UserData", "OstBackport", "OST7", id);
                if (!Directory.Exists(path))
                {
                    _log.Notice($"Downloading map {id} for OST7");
                    await DownloadMapFromId(id, "OST7");
                    _log.Notice($"Downloaded map {id} for OST7");
                    downloadedMaps++;
                    MapSavingCallback?.Invoke(downloadedMaps, totalMaps);
                }
            }
            IsReady = true;
        }

        public void Initialize()
        {
            UnityMainThreadTaskScheduler.Factory.StartNew(() => Task.Run(() => CheckMaps()));
        }
    }
}
