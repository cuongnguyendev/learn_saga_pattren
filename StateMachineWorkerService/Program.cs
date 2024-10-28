using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MassTransit;
using System.Reflection;
using StateMachineWorkerService.CustomState;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddMassTransit(busRegistrationConfigurator =>
        {
            busRegistrationConfigurator.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
                .EntityFrameworkRepository(configure =>
                {
                    configure.AddDbContext<SagaDbContext, OrderStateDbContext>((serviceProvider, dbContextOptionsBuilder) =>
                    {
                        dbContextOptionsBuilder.UseSqlServer(context.Configuration.GetConnectionString(nameof(OrderStateDbContext)), sqlServerOptions =>
                        {
                            sqlServerOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                        });
                    });
                });
            busRegistrationConfigurator.UsingRabbitMq((busRegistrationContext, rabbitMqBusFactoryConfigurator) =>
            {
                rabbitMqBusFactoryConfigurator.Host(new Uri(context.Configuration["RabbitMqSetting:HostAddress"]!), hostConfigurator =>
                {
                    hostConfigurator.Username(context.Configuration["RabbitMqSetting:Username"]);
                    hostConfigurator.Password(context.Configuration["RabbitMqSetting:Password"]);
                });

                rabbitMqBusFactoryConfigurator.ReceiveEndpoint(RabbitQueueName.OrderSaga, endpoint =>
                {
                    endpoint.ConfigureSaga<OrderStateInstance>(busRegistrationContext);
                });
            });
        });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
