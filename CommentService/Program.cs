using CommentService;
using CommentService.Repositories;
using CommentService.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "commentcache:6379";
builder.Services.AddSingleton(new CommentCacheService(redisConnection));
builder.Services.AddSingleton(Database.GetInstance());
builder.Services.AddScoped<CommentDbRepository>();
builder.Services.AddScoped<ICommentRepository>(sp =>
{
    var cache = sp.GetRequiredService<CommentCacheService>();
    var dbRepo = sp.GetRequiredService<CommentDbRepository>();
    return new CommentCacheRepository(cache, dbRepo);
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
