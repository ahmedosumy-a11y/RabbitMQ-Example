using Publisher.Services;
using Shared.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rabbitmqConfig = builder.Configuration
    .GetSection(nameof(RabbitMQConfiguration))
    .Get<RabbitMQConfiguration>()
    ?? throw new InvalidOperationException("RabbitMQConfiguration section is missing.");

builder.Services.AddSingleton(rabbitmqConfig);
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
