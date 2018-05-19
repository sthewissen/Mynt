using LiteDB;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Mynt.Core.Backtester
{
    public class BacktesterDatabase
    {
        private static readonly Dictionary<string, DataStore> DatabaseInstances = new Dictionary<string, DataStore>();

        public static string GetDataDirectory(string datafolder, string exchange = null, string pair = null)
        {
            if (!Directory.Exists(datafolder))
                Directory.CreateDirectory(datafolder);

            if (exchange == null && pair == null)
            {
                return datafolder.Replace("\\","/");
            }
            return datafolder.Replace("\\", "/") + "/" + exchange + "_" + pair + ".db";
        }

        public class DataStore
        {
            private readonly LiteDatabase _liteDatabase;

            private DataStore(string databasePath)
            {
                // Workaround on OSX -> Dont support Locking/unlocking file regions 
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    _liteDatabase = new LiteDatabase("filename=" + databasePath + ";mode=Exclusive;utc=true");
                }
                else
                {
                    _liteDatabase = new LiteDatabase("filename=" + databasePath + ";mode=Exclusive;mode=Shared;utc=true");
                }
            }

            public static DataStore GetInstance(string databasePath)
            {
                if (!DatabaseInstances.ContainsKey(databasePath))
                {
                    DatabaseInstances[databasePath] = new DataStore(databasePath);
                }
                return DatabaseInstances[databasePath];
            }

            public LiteCollection<T> GetTable<T>(string collectionName = null) where T : new()
            {
                if (collectionName == null)
                {
                    return _liteDatabase.GetCollection<T>(typeof(T).Name);
                }
                return _liteDatabase.GetCollection<T>(collectionName);
            }
        }
    }
}
