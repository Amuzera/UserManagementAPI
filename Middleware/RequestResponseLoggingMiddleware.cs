using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UserManagement.Middleware;

public class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var traceId = context.TraceIdentifier;
        var method = context.Request.Method;
        var path = context.Request.Path + context.Request.QueryString;

        await next(context);

        sw.Stop();
        var status = context.Response.StatusCode;
        logger.LogInformation(
            "HTTP {Method} {Path} => {Status} ({Elapsed} ms) traceId={TraceId}",
            method, path, status, sw.ElapsedMilliseconds, traceId);
    }
}
