using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Core.Configuration
{
    public class Settings : BaseSettings
    {
        public Settings()
        {
            TrySetFromConfig(() => TableStorageConnectionString = AppSettings.Get<string>(nameof(TableStorageConnectionString)));
            TrySetFromConfig(() => OrderTableName = AppSettings.Get<string>(nameof(OrderTableName)));
            TrySetFromConfig(() => TraderTableName = AppSettings.Get<string>(nameof(TraderTableName)));
        }

        // Azure settings
        public string TableStorageConnectionString { get; set; } = "";
        public string OrderTableName { get; set; } = "orders";
        public string TraderTableName { get; set; } = "traders";
    }

    public static class AppSettings
    {
        public static T Get<T>(string key)
        {
            var appSetting = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(appSetting)) throw new KeyNotFoundException($"AppSetting not found: '{key}'");

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(appSetting));
        }
    }

    public abstract class BaseSettings
    {
        protected static void TrySetFromConfig(Action action)
        {
            try
            {
                action();
            }
            catch
            {
            }
        }
    }
}
