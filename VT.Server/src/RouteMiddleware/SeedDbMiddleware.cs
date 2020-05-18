using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using VT.Model;
using VT.Seed;


public class SeedDbMiddleware {
    readonly RequestDelegate _next;

    public SeedDbMiddleware(RequestDelegate next) {
        _next = next;
    }

    public async Task Invoke(HttpContext context) {
    
        var ctx = context.RequestServices.GetService<AppDbContext>();
        if (ctx != null) {
            var dataSeeder = new DataSeeder();
            await dataSeeder.GenerateSeedData(ctx);
        } else {
            throw new Exception("RequestServices.GetService returned null AppDbContext ");
        }
        context.Response.StatusCode = 200;
    }
}