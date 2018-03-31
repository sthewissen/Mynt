using System;
using Microsoft.EntityFrameworkCore;
using Mynt.Core.Models;

namespace Mynt.Data.SqlServer
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
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public DbSet<TradeAdapter> Orders { get; set; }
        public DbSet<TraderAdapter> Traders { get; set; }
    }

}
