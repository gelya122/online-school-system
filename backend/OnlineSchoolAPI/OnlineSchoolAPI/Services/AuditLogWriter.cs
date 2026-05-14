using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Services;

public sealed class AuditLogWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly OnlineSchoolDbContext _context;
    private readonly IHttpContextAccessor _http;

    public AuditLogWriter(OnlineSchoolDbContext context, IHttpContextAccessor http)
    {
        _context = context;
        _http = http;
    }

    public void Add(string action, string entityType, int? entityId, object? oldValues = null, object? newValues = null)
    {
        var ctx = _http.HttpContext;
        int? employeeId = null;
        int? userId = null;

        if (ctx != null)
        {
            if (int.TryParse(ctx.User.FindFirst("employeeId")?.Value, out var e) && e > 0) employeeId = e;
            if (int.TryParse(ctx.User.FindFirst("uid")?.Value, out var u) && u > 0) userId = u;
        }

        _context.AuditLogs.Add(new AuditLog
        {
            EmployeeId = employeeId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues == null ? null : JsonSerializer.Serialize(oldValues, JsonOptions),
            NewValues = newValues == null ? null : JsonSerializer.Serialize(newValues, JsonOptions),
            IpAddress = ctx?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = ctx?.Request.Headers.UserAgent.ToString(),
            CreatedAt = DateTime.UtcNow
        });
    }
}

