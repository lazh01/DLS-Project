using DraftService.Data;
using DraftService.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString1 = builder.Configuration.GetConnectionString("DraftDatabase");
var connectionString = "Server=draft-db;Database=Drafts;User Id=sa;Password=SuperSecret7!;Encrypt=false;";
builder.Services.AddDbContext<DraftServiceContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseInMemoryDatabase("DraftsInMemory");
    }
});

// Add services to the container.
builder.Services.AddScoped<DraftModelService>();
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DraftServiceContext>();

    if (db.Database.IsSqlServer())
    {
        db.Database.Migrate(); // applies migrations to SQL Server only
    }
}

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
