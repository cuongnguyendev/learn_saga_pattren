using StateMachineWorkerService.BackgroudService;
using StateMachineWorkerService.CustomState;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString(nameof(OrderStateDbContext))));

        services.AddScoped<ShippingRequestService>(); // Keep this as Scoped

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

        services.AddHostedService<Worker>(); // Worker is registered as Hosted Service
    })
    .Build();

await host.RunAsync();
