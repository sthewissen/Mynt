using System;
using Microsoft.EntityFrameworkCore;

namespace Mynt.Data.SQLite
{
    public class SQLiteContext : DbContext
    {
        private string _connectionString;

        public DbSet<TradeAdapter> Trades { get; set; }
        public DbSet<TraderAdapter> Traders { get; set; }

        public SQLiteContext(DbContextOptions<SQLiteContext> options) : base(options) 
        { 
        }
    }
}
