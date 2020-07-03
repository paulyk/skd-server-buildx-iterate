using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SKD.Model;
using SKD.Seed;


public class SeedDbMiddleware {
    readonly RequestDelegate _next;

    public SeedDbMiddleware(RequestDelegate next) {
        _next = next;
    }

    public async Task Invoke(HttpContext context) {
    
        var ctx = context.RequestServices.GetService<SkdContext>();
        if (ctx != null) {
            var dataSeeder = new DataSeeder();
            await dataSeeder.GenerateSeedData(ctx);
        } else {
            throw new Exception("RequestServices.GetService returned null SkdContext ");
        }
        context.Response.StatusCode = 200;
    }
}