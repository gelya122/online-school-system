using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineSchoolAPI;
using OnlineSchoolAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OnlineSchoolDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("OnlineSchoolConnection")));
builder.Services.AddScoped<IOrderReceiptEmailService, OrderReceiptEmailService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Online School API",
        Version = "v1",
        Description = "API для онлайн школы"
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

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
