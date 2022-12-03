using Microsoft.AspNetCore.ResponseCompression;
using ProductCatalog.Services;
using ProductCatalog.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Mime;
using System.Text.Json;
using ProductCatalog.Models.DTO;
using ProductCatalog.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Azure.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddAzureKeyVault(
            new Uri("https://sshskeyvault01.vault.azure.net/"),
            new DefaultAzureCredential());
}

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
                options.Level = System.IO.Compression.CompressionLevel.Optimal);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddControllers();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddDbContext<ProductCatalogDbContext>(opt =>
  {
      var connectionString = builder.Configuration.GetConnectionString("ProductCatalogDbPgSqlConnection");
      opt.UseNpgsql(connectionString, npgsqlOptionsAction: sqlOptions =>
      {
          sqlOptions.EnableRetryOnFailure(
              maxRetryCount: 4,
              maxRetryDelay: TimeSpan.FromSeconds(Math.Pow(2, 3)),
              errorCodesToAdd: null);
      })
      .UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);
  });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
        .AddHealthChecks()
        .AddDbContextCheck<ProductCatalogDbContext>("dbcontext", HealthStatus.Unhealthy);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler((appBuilder) =>
    {
        appBuilder.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            Exception exception = exceptionHandlerPathFeature?.Error;

            context.Response.StatusCode = exception switch
            {
                EntityNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            ApiResponse apiResponse = exception switch
            {
                EntityNotFoundException => new ApiResponse("Product not found"),
                Exception ex => new ApiResponse($"An error occurred: {ex.Message}"),
                _ => new ApiResponse("An error occurred")
            };

            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse));
        });
    });

app.Run();
