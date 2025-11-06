using EasyNetQ;
using NewsletterService.Interfaces;
using NewsletterService.Services;
using NewsletterService.Wrappers;
using Polly;
using Polly.Extensions.Http;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<SubscriberApiClient>(client =>
{
    client.BaseAddress = new Uri("http://subscriberservice:80/");
})
.AddPolicyHandler(
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Console.WriteLine($"Retry {retryAttempt} for SubscriberService after {timespan.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
            }
        )
)
.AddPolicyHandler(
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 2,
            durationOfBreak: TimeSpan.FromSeconds(15),
            onBreak: (outcome, breakDelay) =>
            {
                Console.WriteLine($"Circuit breaker opened for {breakDelay.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit breaker reset. SubscriberService calls allowed again.");
            }
        )
);

// Subscriber queue
builder.Services.AddSingleton<SubscriberBus>(_ =>
    new SubscriberBus(RabbitHutch.CreateBus("host=subscriberqueue;username=guest;password=guest")));

// Article queue
builder.Services.AddSingleton<ArticleBus>(_ =>
{
    IBus bus = null;

    var retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryForever(
            _ => TimeSpan.FromSeconds(5),
            (ex, _) => Console.WriteLine($"Waiting for RabbitMQ: {ex.Message}")
        );

    retryPolicy.Execute(() =>
    {
        bus = RabbitHutch.CreateBus("host=rabbitmq;username=guest;password=guest");
        bus.Advanced.QueueDeclare("healthcheck-queue");
    });

    return new ArticleBus(bus);
});

// Hosted services
// Register the hosted services manually with the wrappers
builder.Services.AddSingleton<IHostedService>(sp =>
    new SubscriberEventConsumer(sp.GetRequiredService<SubscriberBus>()));

builder.Services.AddSingleton<IHostedService>(sp =>
    new ArticleCreatedConsumer(sp.GetRequiredService<ArticleBus>(), sp.GetRequiredService<SubscriberApiClient>()));

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
