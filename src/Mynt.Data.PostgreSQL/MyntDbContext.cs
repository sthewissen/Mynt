using System;
using Microsoft.EntityFrameworkCore;
using Mynt.Core.Models;

namespace Mynt.Data.PostgreSQL
{
    public class MyntDbContext : DbContext
    {
        private string _connectionString;

        public MyntDbContext()
        {
            _connectionString = new PostgreSqlOptions().PostgreSqlConnectionString;
        }

        public MyntDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql(_connectionString);

        public DbSet<TradeAdapter> Orders { get; set; }
        public DbSet<TraderAdapter> Traders { get; set; }
    }

}
