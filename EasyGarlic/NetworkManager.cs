﻿using Newtonsoft.Json.Linq;
using NLog;
using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class NetworkManager {

        public OnlineData data;
        private Linker linker;

        private static Logger logger = LogManager.GetLogger("NetworkLogger");

        public async Task Setup(Linker _linker, IProgress<ProgressReport> progress)
        {
            linker = _linker;
            logger.Info("Loading Online Data...");
            progress.Report(new ProgressReport("Loading Online Data..."));

            // Load Data from API
            try
            {
                data = OnlineData.LoadData(await FetchURLData(linker.minerManager.GetDataURL()));
            }
            catch (WebException e)
            {
                progress.Report(new ProgressReport("Could not load Online Data at " + linker.minerManager.GetDataURL(), e));
            }
        }

        public async Task<string> FetchURLData(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                return await webClient.DownloadStringTaskAsync(new Uri(url));
            }
        }

        public async Task DownloadFileAsync(string path, string url)
        {
            logger.Debug("Downloading from " + url + " to " + path);

            using (WebClient webClient = new WebClient())
            {
                string downloadToDirectory = path;
                webClient.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                await webClient.DownloadFileTaskAsync(new Uri(url), @downloadToDirectory);
            }
        }

        public async Task DownloadMultipleFilesAsync(string[] paths, string[] urls)
        {
            List<Task> downloadTasks = new List<Task>();
            for (int i = 0; i < paths.Length; i++)
            {
                downloadTasks.Add(DownloadFileAsync(paths[i], urls[i]));
            }

            await Task.WhenAll(downloadTasks);
        }

        public async Task ExtractZipFile(string file, string path)
        {
            logger.Debug("Extracting " + file + " to " + path);

            await Task.Run(() =>
            {
                var archive = ArchiveFactory.Open(file);
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        entry.WriteToDirectory(path, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                    }
                }
                // Close Archive Factory so it's not being used
                archive.Dispose();
            });
        }

        // If it's not a zip, need to cut to the new folder
        public async Task InstallInFolder(string file, string path)
        {
            string destinationPath = System.IO.Path.Combine(path, System.IO.Path.GetFileName(file));

            await Task.Run(() => { Directory.CreateDirectory(path); File.Move(file, destinationPath); });
        }

        // Get the list of pools from Watchdog
        public async Task<PoolData[]> GetPoolData(string url)
        {
            string data = await FetchURLData(url);

            JObject obj = JObject.Parse(data);

            if (obj["success"] != null && obj["success"].ToObject<bool>() == true)
            {
                PoolData[] deserialized = obj["data"].ToObject<PoolData[]>();

                return deserialized;
            }

            // Return an empty list
            return new PoolData[0];
        }
    }
}
