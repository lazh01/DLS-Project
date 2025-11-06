using FeatureHubSDK;
using Microsoft.EntityFrameworkCore;
using SubscriberService.Data;
using EasyNetQ;
var builder = WebApplication.CreateBuilder(args);

// Database setup
/*builder.Services.AddDbContext<SubscriberDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));*/

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SubscriberDbContext>(options =>
    options.UseSqlServer(connectionString));

var fhUrl = builder.Configuration["FeatureHub:Url"]
    ?? builder.Configuration["FEATUREHUB_URL"];
var fhApiKey = builder.Configuration["FeatureHub:ApiKey"]
    ?? builder.Configuration["FEATUREHUB_APIKEY"];

if (string.IsNullOrWhiteSpace(fhUrl) || string.IsNullOrWhiteSpace(fhApiKey))
{
    throw new ApplicationException("FeatureHub URL or API key is not set.");
}

// Configure FeatureHub client
var fhConfig = new EdgeFeatureHubConfig(fhUrl, fhApiKey);
builder.Services.AddSingleton(fhConfig);

// We will build a context (for our service) and make it singleton or scoped
builder.Services.AddSingleton<FeatureHubSDK.IClientContext>(sp =>
{
    var configObj = sp.GetRequiredService<EdgeFeatureHubConfig>();
    var contextBuilder = configObj.NewContext();
    var fhContext = contextBuilder.Build().GetAwaiter().GetResult();
    return fhContext;
});


builder.Services.AddSingleton<IBus>(_ => RabbitHutch.CreateBus("host=subscriberqueue;username=guest;password=guest"));


// Custom service
builder.Services.AddScoped<SubscriberService.Services.SubscriptionService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

DbInitializer.Initialize(app.Services);

// Dev setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
