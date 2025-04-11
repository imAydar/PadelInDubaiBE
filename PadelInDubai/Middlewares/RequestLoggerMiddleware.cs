
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace PadelInDubai.Middlewares
{
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

        public RequestLoggerMiddleware(RequestDelegate next)
        {
            _next = next;
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();

            var request = context.Request;
            var log = new StringBuilder();

            log.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
            log.AppendLine($"Method: {request.Method}");
            log.AppendLine($"Path: {request.Path}");
            log.AppendLine($"Query: {request.QueryString}");
            log.AppendLine("Headers:");
            foreach (var header in request.Headers)
            {
                log.AppendLine($"  {header.Key}: {header.Value}");
            }

            if (request.ContentLength > 0 && request.Body.CanSeek)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                string body = await reader.ReadToEndAsync();
                request.Body.Seek(0, SeekOrigin.Begin);
                log.AppendLine("Body:");
                log.AppendLine(body);
            }

            var fileName = $"request_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.log";
            var filePath = Path.Combine(_logDirectory, fileName);
            await File.WriteAllTextAsync(filePath, log.ToString());

            await _next(context);
        }
    }

}
