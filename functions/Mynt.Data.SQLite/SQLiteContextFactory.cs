using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Mynt.Data.SQLite
{
    public class SQLiteContextFactory : IDesignTimeDbContextFactory<SQLiteContext>
    {
        public SQLiteContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<SQLiteContext>();

            // TODO: Move this to a settings of some sorts...
            builder.UseSqlite("Data Source=Mynt.sqlite");

            return new SQLiteContext(builder.Options);
        }
    }
}
