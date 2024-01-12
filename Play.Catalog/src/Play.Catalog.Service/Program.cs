using Play.Catalog.Service.Entities;
using Play.Common.MassTransit;
using Play.Common.MongoDB;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddMongo()
    .AddMongRepo<Item>("items")
    .AddMassTransitWithRabbitMq();

// builder.Services.AddMassTransit(x =>
// {
//     x.UsingRabbitMq((context, configurator) =>
//     {
//         var rabbitMQSettings = builder.Configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
//         configurator.Host(rabbitMQSettings.Host);
//         configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(
//             builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>().ServiceName, false));
//     });
//});

//builder.Services.AddMassTransitHostedService();  //Maybe not needed in new version of MassTransit?

builder.Services.AddControllers();


// builder.Services.AddSingleton<IMongoDatabase>(options =>
// {
//     var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
//     var client = new MongoClient(mongoSettings.ConnectionString);
//     return client.GetDatabase(builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>().ServiceName);
// });

//builder.Services.AddScoped<IRepository<Item>, MongoRepository<Item>>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //not sure if I am adding serz to the right place, but it works
    //BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
    //BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

