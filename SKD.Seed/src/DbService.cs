using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SKD.Model;
using System;

namespace SKD.Seed {
    public class DbService {
        private readonly SkdContext ctx;
        public DbService(SkdContext ctx) {
            this.ctx = ctx;
        }

        public async Task MigrateDb() {
            await ctx.Database.MigrateAsync();
        }

        public async Task DroCreateDb() {
            await ctx.Database.EnsureDeletedAsync();
            Console.WriteLine("Dropped database");
            await ctx.Database.MigrateAsync();
            Console.WriteLine("Created database");
        }
    }
}