using Microsoft.Extensions.DependencyInjection;
using Articleservice.Services;
using ArticleService.database;
using ArticleService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.RegisterEasyNetQ(
    "host=rabbitmq;username=guest;password=guest"
    );
var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "redis:6379";
builder.Services.AddSingleton(new RedisCacheService(redisConnection));
builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<CacheUpdaterService>();
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<PublishingConsumer>();
builder.Services.AddHostedService<CacheUpdateWorker>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();