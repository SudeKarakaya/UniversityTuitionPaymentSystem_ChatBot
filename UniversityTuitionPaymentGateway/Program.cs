using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());


builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();


var rateLimits = new Dictionary<string, (DateTime date, int count)>();

app.Use(async (context, next) =>
{
    var req = context.Request;
    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var timestamp = DateTime.UtcNow;
    var fullPath = $"{req.Path}{req.QueryString}";
    var reqSize = req.ContentLength ?? 0L;
    var headersString = string.Join(" | ", req.Headers.Select(h => $"{h.Key}: {h.Value}"));

   
    if (req.Path.StartsWithSegments("/api/v1/tuition/query", StringComparison.OrdinalIgnoreCase))
    {
        var studentNo = req.Query["studentNo"].ToString();
        if (!string.IsNullOrEmpty(studentNo))
        {
            var today = DateTime.UtcNow.Date;
            if (rateLimits.TryGetValue(studentNo, out var val))
            {
                if (val.date == today && val.count >= 3)
                {
                    Log.Warning("Rate limit exceeded for student {StudentNo}", studentNo);
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Daily query limit reached (3)");
                    return;
                }
                else if (val.date == today)
                {
                    rateLimits[studentNo] = (today, val.count + 1);
                }
                else
                {
                    rateLimits[studentNo] = (today, 1);
                }
            }
            else
            {
                rateLimits[studentNo] = (today, 1);
            }
        }
    }

    var authHeader = req.Headers["Authorization"].ToString();
    var authPresent = !string.IsNullOrEmpty(authHeader);

  
    Log.Information(
        "[REQUEST] {Timestamp} {Method} {FullPath} from {IP} Size={Size} Headers={Headers} AuthHeaderPresent={AuthPresent}",
        timestamp.ToString("o"),
        req.Method,
        fullPath,
        ip,
        reqSize,
        headersString,
        authPresent
    );

   
    var originalBody = context.Response.Body;
    using var memStream = new MemoryStream();
    context.Response.Body = memStream;

    var sw = Stopwatch.StartNew();
    await next();
    sw.Stop();

    memStream.Seek(0, SeekOrigin.Begin);
    var responseBody = await new StreamReader(memStream).ReadToEndAsync();
    var respSize = memStream.Length;
    memStream.Seek(0, SeekOrigin.Begin);

    var statusCode = context.Response.StatusCode;

    string authResult = "none";
    if (authPresent)
    {
        if (statusCode == 401 || statusCode == 403) authResult = "failed";
        else if (statusCode is >= 200 and < 300) authResult = "succeeded";
        else authResult = "unknown";
    }

   
    Log.Information(
        "[RESPONSE] Status={StatusCode} Latency={Latency}ms Size={Size} AuthResult={AuthResult}",
        statusCode,
        sw.ElapsedMilliseconds,
        respSize,
        authResult
    );

    await memStream.CopyToAsync(originalBody);
    context.Response.Body = originalBody;
});

app.MapReverseProxy();

app.Run();
