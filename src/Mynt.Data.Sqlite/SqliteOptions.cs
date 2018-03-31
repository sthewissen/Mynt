using System;
using Mynt.Core.Configuration;

namespace Mynt.Data.Sqlite
{
    public class SqliteOptions: BaseSettings
    {
        public string ConnectionString { get; set; }

        public SqliteOptions()
        {
            TrySetFromConfig(() => ConnectionString = AppSettings.Get<string>("Sqlite" + nameof(ConnectionString)));
        }
    }
}
