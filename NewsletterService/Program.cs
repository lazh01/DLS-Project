using EasyNetQ;
using Polly;
using NewsletterService.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//need to wait for rabbitmq
builder.Services.AddSingleton<IBus>(sp =>
{
    IBus bus = null;

    // Retry forever until RabbitMQ is available
    var retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryForever(
            _ => TimeSpan.FromSeconds(5),
            (ex, _) => Console.WriteLine($"Waiting for RabbitMQ: {ex.Message}")
        );

    retryPolicy.Execute(() =>
    {
        bus = RabbitHutch.CreateBus("host=rabbitmq;username=guest;password=guest");

        // Optional connectivity check
        bus.Advanced.QueueDeclare("healthcheck-queue");
    });

    return bus;
});
builder.Services.AddHostedService<ArticleCreatedConsumer>();
builder.Services.AddScoped<FetchArticlesService>();
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
