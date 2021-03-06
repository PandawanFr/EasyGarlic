﻿using System;
using System.IO;

namespace EasyGarlic {
    public static class Path {

        public static string GetCurrentDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\EasyGarlic\" + Config.EXTRA_PATH;
            //return Directory.GetCurrentDirectory();
        }

        public static string GetDataDirectory()
        {
            string dataPath = GetCurrentDirectory() + @"data\";
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            return dataPath;
        }

        public static string GetMinerDirectory(string id)
        {
            string dataDir = GetDataDirectory();

            return dataDir + id + @"\";
        }

        public static string GetLocalDataFile()
        {
            string dataDir = GetDataDirectory();

            return System.IO.Path.Combine(dataDir, @"data.json");
        }
    }
}
