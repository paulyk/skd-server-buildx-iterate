using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VT.Model;

namespace VT.Test {
    public class TestBase {

         public  AppDbContext GetAppDbContext() {

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                        .UseSqlite(connection)
                        .Options;

            var ctx = new AppDbContext(options);

            ctx.Database.EnsureCreated();
            return ctx;
        }
    }
}