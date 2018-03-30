using System;
using Mynt.Core.Configuration;

namespace Mynt.Data.SqlServer
{
    public class SqlServerOptions: BaseSettings
    {
        public string ConnectionString { get; set; }

        public SqlServerOptions()
        {
            TrySetFromConfig(() => ConnectionString = AppSettings.Get<string>("SqlServer" + nameof(ConnectionString)));
        }
    }
}
