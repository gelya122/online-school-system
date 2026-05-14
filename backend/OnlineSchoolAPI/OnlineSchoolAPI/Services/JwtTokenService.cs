using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI.Services;

public sealed class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateAccessToken(User user)
    {
        var jwt = _configuration.GetSection("Jwt");
        var issuer = jwt["Issuer"] ?? "OnlineSchoolAPI";
        var audience = jwt["Audience"] ?? "OnlineSchoolClients";
        var key = jwt["Key"];
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("JWT key is not configured (Jwt:Key).");

        var minutes = 60;
        if (int.TryParse(jwt["AccessTokenMinutes"], out var parsed) && parsed > 0)
            minutes = parsed;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new("uid", user.UserId.ToString()),
            new("roleId", user.RoleId.ToString()),
            new(ClaimTypes.Role, RoleNameMapper.Canonicalize(user.Role?.RoleName))
        };

        if (user.Employee != null)
            claims.Add(new("employeeId", user.Employee.EmployeeId.ToString()));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

