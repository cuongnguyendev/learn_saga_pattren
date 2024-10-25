using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Consumers;
using OrderService.Infrastructure.Context;
using Shared.Infrastructure;

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
    busRegistrationConfigurator.AddConsumer<OrderRequestCompletedEventConsumer>();
    busRegistrationConfigurator.AddConsumer<OrderRequestFailedEventConsumer>();
    busRegistrationConfigurator.AddConsumer<OrderShippingRequestCompletedEventConsumer>();
    busRegistrationConfigurator.AddConsumer<OrderShippingRequestFailedEventConsumer>();
    busRegistrationConfigurator.AddConsumer<OrderCancelRequestCompletedEventConsumer>();
    busRegistrationConfigurator.AddConsumer<OrderCancelRequestFailedEventConsumer>();

    busRegistrationConfigurator.UsingRabbitMq((busRegistrationContext, rabbitMqBusFactoryConfigurator) =>
    {
        rabbitMqBusFactoryConfigurator.Host(builder.Configuration["RabbitMqSetting:HostAddress"], "/", hostConfigurator =>
        {
            hostConfigurator.Username(builder.Configuration["RabbitMqSetting:Username"]);
            hostConfigurator.Password(builder.Configuration["RabbitMqSetting:Password"]);
        });

        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(RabbitQueueName.OrderRequestCompletedEventQueueName, endpoint =>
        {
            endpoint.ConfigureConsumer<OrderRequestCompletedEventConsumer>(busRegistrationContext);
        });

        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(RabbitQueueName.OrderRequestFailedEventQueueName, endpoint =>
        {
            endpoint.ConfigureConsumer<OrderRequestFailedEventConsumer>(busRegistrationContext);
        });
        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(RabbitQueueName.ShippingCompletedEventQueueName, endpoint =>
        {
            endpoint.ConfigureConsumer<OrderShippingRequestCompletedEventConsumer>(busRegistrationContext);
        });
        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(RabbitQueueName.ShippingFailedEventQueueName, endpoint =>
        {
            endpoint.ConfigureConsumer<OrderShippingRequestFailedEventConsumer>(busRegistrationContext);
        });
        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(RabbitQueueName.OrderCancelRequestCompletedEventQueueName, endpoint =>
        {
            endpoint.ConfigureConsumer<OrderCancelRequestCompletedEventConsumer>(busRegistrationContext);
        });
        rabbitMqBusFactoryConfigurator.ReceiveEndpoint(RabbitQueueName.OrderCancelRequestFailedEventQueueName, endpoint =>
        {
            endpoint.ConfigureConsumer<OrderCancelRequestFailedEventConsumer>(busRegistrationContext);
        });
    });
});
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
