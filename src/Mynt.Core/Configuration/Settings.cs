using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using Newtonsoft.Json;

namespace Mynt.Core.Configuration
{
    public static class AppSettings
    {
        public static T Get<T>(string key)
        {
            var appSetting = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(appSetting)) throw new KeyNotFoundException($"AppSetting not found: '{key}'");

            var converter = TypeDescriptor.GetConverter(typeof(T));

            return (T)converter.ConvertFromInvariantString(appSetting);
        }

        public static T Get<T>()
            where T:new()
        {
            var properties = typeof(T).GetProperties();
            var instance = new T();
            foreach (var property in properties)
            {
                var name = property.Name;
                var valueString = ConfigurationManager.AppSettings[name];
                if (property.PropertyType.GetInterface(nameof(IList)) != null)
                {
                    if (!String.IsNullOrEmpty(valueString))
                    {
                        var value = JsonConvert.DeserializeObject(valueString, property.PropertyType);
                        property.SetValue(instance, value);
                    }
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(property.PropertyType);
                    var value = converter.ConvertFromInvariantString(valueString);
                    property.SetValue(instance, value);
                }
            }

            return instance;
        }
    }
}
