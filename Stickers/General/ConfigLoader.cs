using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Stickers.General
{
    internal class ConfigLoader
    {
        readonly static string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        internal static Config GetConfig()
        {
            if (!File.Exists(_configPath))
                throw new FileNotFoundException(_configPath);

            Config config = JsonSerializer.Deserialize<Config>(File.ReadAllText(_configPath)) 
                ?? throw new NullReferenceException("Decoded config is null");

            return config;
        }
    }
}
