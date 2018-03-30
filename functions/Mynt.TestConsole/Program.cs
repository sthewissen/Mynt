using System;
using Mynt.Core.Models;
using Mynt.Data.SqlServer;

namespace Mynt.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Either insert the connection string in the SQLServerOptions class or add a setting in your 
            // project called SqlServerConnectionString which will be automatically retrieved by the configuration.
            var sqlStore = new SqlServerDataStore(new SqlServerOptions
            {
                ConnectionString = "<INSERT CONNECTION STRING>"
            });

            sqlStore.SaveTraderAsync(new Trader { Identifier=Guid.NewGuid().ToString(), StakeAmount=0.01m, CurrentBalance=0.01m }).Wait();

            var traders = sqlStore.GetTradersAsync().Result;
        }
    }
}
