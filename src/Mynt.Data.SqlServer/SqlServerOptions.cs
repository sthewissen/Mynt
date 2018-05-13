using System;
using Mynt.Core.Configuration;

namespace Mynt.Data.SqlServer
{
    public class SqlServerOptions
    {
        public string SqlServerConnectionString { get; set; } = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=Mynt;Integrated Security=SSPI";
    }
}
