using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Mynt.Backtester
{
    public class BacktesterDatabase
    {
        public static Dictionary<string, DataStore> instance = new Dictionary<string, DataStore>();

        public static string GetDataDirectory(string exchange = null, string pair = null)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(basePath, "data");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (exchange == null && pair == null)
            {
                return path.Replace("\\","/");
            }
            return path.Replace("\\", "/") + "/" + exchange + "_" + pair + ".db";
        }

        public class DataStore
        {
            private LiteDatabase liteDataBase;

            private DataStore(string databasePath)
            {
                //Workaround on OSX -> Dont support Locking/unlocking file regions 
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    liteDataBase = new LiteDatabase("filename=" + databasePath + ";mode=Exclusive");
                }
                else
                {
                    liteDataBase = new LiteDatabase(databasePath);
                }
            }

            public static DataStore GetInstance(string databasePath)
            {
                if (!instance.ContainsKey(databasePath))
                {
                    instance[databasePath] = new DataStore(databasePath);
                }
                return instance[databasePath];
            }

            public LiteCollection<T> GetTable<T>(string collectionName = null) where T : new()
            {
                if (collectionName == null)
                {
                    return liteDataBase.GetCollection<T>(typeof(T).Name);
                }
                return liteDataBase.GetCollection<T>(collectionName);
            }
        }
    }
}
