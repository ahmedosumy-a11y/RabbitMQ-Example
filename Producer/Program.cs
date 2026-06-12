using Shared.Configration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// builder.Services.Configure<RabbitMQConfigration>(
//     builder.Configuration.GetSection("RabbitMQConfigration"));
var rabbitmqConfig = builder.Configuration
    .GetSection(nameof(RabbitMQConfigration))
    .Get<RabbitMQConfigration>();
builder.Services.AddSingleton(rabbitmqConfig);    
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();



app.Run();


