using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloStats
{
    public class TypeConfigurationManager
    {
        public int GetAppConfigInt(string key, int defaultValue)
        {
            var configString = ConfigurationManager.AppSettings[key];
            int value;
            if (int.TryParse(configString, out value))
                return value;
            else
            {
                Console.WriteLine(string.Format("Failed to parse config '{0}' value of '{1}'", key, configString));
                return defaultValue;
            }
        }

        public double GetAppConfigDouble(string key, double defaultValue)
        {
            var configString = ConfigurationManager.AppSettings[key];
            double value;
            if (double.TryParse(configString, out value))
                return value;
            else
            {
                Console.WriteLine(string.Format("Failed to parse config '{0}' value of '{1}'", key, configString));
                return defaultValue;
            }
        }

        public string[] GetAppSettingAsArray(string key)
        {
            return ConfigurationManager.AppSettings[key].Split(',').Where(v=>!string.IsNullOrWhiteSpace(v)).ToArray();
        }

        public string GetAppConfig(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
