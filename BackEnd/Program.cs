using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Minio;
using TrueLine.Api.Data;
using TrueLine.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MinioOptions>>().Value;
    var endpoint = options.Endpoint
        .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
        .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
        .TrimEnd('/');

    var client = new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(options.AccessKey, options.SecretKey);

    if (options.UseSsl)
    {
        client = client.WithSSL();
    }

    return client.Build();
});
builder.Services.AddScoped<INewsImageStorage, MinioNewsImageStorage>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
