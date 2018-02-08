using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using log4net;
using Mynt.DataAccess.Interfaces;

namespace Mynt.DataAccess.FileBasedStorage
{
    public class CsvDataStorage : IDataStorage
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string folder;

        public CsvDataStorage(string folder)
        {
            this.folder = folder;
        }

        public void Save<T>(string parentKey, IEnumerable<T> items)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fileName = Path.Combine(folder, $"{parentKey}-{typeof(T).Name}");

            using (var csvStream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                using (var csvWriter = new StreamWriter(csvStream))
                {
                    var csv = new CsvWriter(csvWriter, new Configuration { Delimiter = ";" });
                    csv.WriteRecords<T>(items);
                    log.Info($"Saved the dataset '{fileName}'");
                }
            }
        }
    }
}
