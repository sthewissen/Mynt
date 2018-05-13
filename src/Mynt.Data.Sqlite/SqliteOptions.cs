using System;
using Mynt.Core.Configuration;

namespace Mynt.Data.Sqlite
{
    public class SqliteOptions
    {
        public string SqliteConnectionString { get; set; } = "Data Source=Mynt.sqlite";
    }
}
