using System;
using Mynt.Core.Configuration;

namespace Mynt.Data.SQLite
{
    public class SQLiteOptions: BaseSettings
    {
        public string ConnectionString { get; set; }

        public SQLiteOptions()
        {
            TrySetFromConfig(() => ConnectionString = AppSettings.Get<string>("SQLite" + nameof(ConnectionString)));
        }
    }
}
