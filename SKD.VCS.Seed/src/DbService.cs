using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SKD.VCS.Model;
using System;

namespace SKD.VCS.Seed {
    public class DbService {
        private SkdContext ctx;
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