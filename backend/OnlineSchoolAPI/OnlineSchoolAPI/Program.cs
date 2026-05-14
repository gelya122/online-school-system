using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OnlineSchoolDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("OnlineSchoolConnection")));
builder.Services.AddScoped<IOrderReceiptEmailService, OrderReceiptEmailService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditLogWriter>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Online School API",
        Version = "v1",
        Description = "API для онлайн школы"
    });
    // Иначе при multipart + IFormFile генерация swagger.json может падать с 500.
    c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });

    // JWT Bearer в Swagger (для ручной проверки защищённых эндпоинтов).
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//��������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    // Для dev/локального запуска: чтобы проект стартовал без ручной правки appsettings.
    // В проде ключ должен быть задан через секреты/переменные окружения.
    jwtKey = "DEV_ONLY_CHANGE_ME__OnlineSchoolAPI_JWT_Key_2026";
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "OnlineSchoolAPI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "OnlineSchoolClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

var app = builder.Build();

//���������� CORS ����� MapControllers
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
// Swagger должен быть до UseHttpsRedirection
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online School API V1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Без валидного ASP.NET dev certificate (dotnet dev-certs https) Kestrel не поднимет https://localhost:*.
// В Development не форсируем HTTPS-редирект, чтобы Swagger и клиенты на http://localhost:* работали стабильно.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
