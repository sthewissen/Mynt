using System;
using Mynt.Core.Configuration;

namespace Mynt.Data.PostgreSQL
{
    public class PostgreSqlOptions
    {
        public string PostgreSqlConnectionString { get; set; } = "Server=127.0.0.1;Port=5432;Database=Mynt;User Id = postgres; Password=postgres;";
    }
}
