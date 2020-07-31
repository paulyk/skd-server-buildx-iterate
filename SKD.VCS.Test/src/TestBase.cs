using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SKD.VCS.Model;

namespace SKD.VCS.Test {
    public class TestBase {

         public  SkdContext GetAppDbContext() {

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<SkdContext>()
                        .UseSqlite(connection)
                        .Options;

            var ctx = new SkdContext(options);

            ctx.Database.EnsureCreated();
            return ctx;
        }
    }
}