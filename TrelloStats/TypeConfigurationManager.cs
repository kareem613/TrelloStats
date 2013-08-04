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
        public int GetAppConfigInt(string p, int defaultValue)
        {
            var configString = ConfigurationManager.AppSettings[p];
            int value;
            if (int.TryParse(configString, out value))
                return value;
            else return defaultValue;
        }

        public double GetAppConfigDouble(string p, double defaultValue)
        {
            var configString = ConfigurationManager.AppSettings[p];
            double value;
            if (double.TryParse(configString, out value))
                return value;
            else return defaultValue;
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
