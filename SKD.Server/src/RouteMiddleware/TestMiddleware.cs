using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class TestMiddleware
{
    readonly RequestDelegate _next;

    public TestMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Response.StatusCode = 200;
        context.Response.Headers.Add("content-type", "text/html");
        await context.Response.WriteAsync("<h2>Hello From Middleware</h2>");
    }
}