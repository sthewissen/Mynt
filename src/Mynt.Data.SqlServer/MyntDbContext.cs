using System;
using Microsoft.EntityFrameworkCore;
using Mynt.Core.Models;

namespace Mynt.Data.SqlServer
{
    public class MyntDbContext : DbContext
    {
        private string _connectionString;

        public MyntDbContext()
<<<<<<< HEAD
        {
            _connectionString = new SqlServerOptions().SqlServerConnectionString;
        }

        public MyntDbContext(string connectionString)
=======
>>>>>>> ad9a4f7bc3f735bfdb1431ca1067b3458162da67
        {
            _connectionString = new SqlServerOptions().SqlServerConnectionString;
        }

        public MyntDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(_connectionString);

        public DbSet<TradeAdapter> Orders { get; set; }
        public DbSet<TraderAdapter> Traders { get; set; }
    }

}
