using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using ShipService.Consumers;
using ShipService.Infrastructure.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(AppDbContext)));
});
builder.Services.AddMassTransit(busRegistrationConfigurator =>
{
    busRegistrationConfigurator.AddConsumer<PaymentCompletedRequestShippingCreatedConsumer>();
    busRegistrationConfigurator.AddConsumer<OrderCanceledRequestShippingConsumer>();


    busRegistrationConfigurator.UsingRabbitMq((busRegistrationContext, rabbitMqBusFactoryConfigurator) =>
    {
        rabbitMqBusFactoryConfigurator.Host(new Uri(builder.Configuration["RabbitMqSetting:HostAddress"]!), hostConfigurator =>
        {
            hostConfigurator.Username(builder.Configuration["RabbitMqSetting:Username"]);
            hostConfigurator.Password(builder.Configuration["RabbitMqSetting:Password"]);
        });

        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(RabbitQueueName.ShippingRequestQueueName, endpoint =>
        {
            endpoint.ConfigureConsumer<PaymentCompletedRequestShippingCreatedConsumer>(busRegistrationContext);
        });
        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(RabbitQueueName.OrderCanceledRequestShippingQueueName, endpoint =>
        {
            endpoint.ConfigureConsumer<OrderCanceledRequestShippingConsumer>(busRegistrationContext);
        });
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");
app.UseAuthorization();

app.MapControllers();
app.Run();
