using System;
using Microsoft.EntityFrameworkCore;

namespace Mynt.Data.Sqlite
{
    public class MyntDbContext : DbContext
    {
        private string _connectionString;

        public MyntDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }

        public DbSet<TradeAdapter> Orders { get; set; }
        public DbSet<TraderAdapter> Traders { get; set; }
    }
}
