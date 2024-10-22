using FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FunctionApp.Controllers;
using Microsoft.Extensions.Configuration;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var connStr = "";
        if (string.IsNullOrEmpty(connStr))
        {
            throw new InvalidOperationException("Connection string is missing");
        }
        // Register your services here
        services.AddSingleton(new TableStorageService(connStr));
        QueueService queueService = new(connStr, "logs");
        services.AddSingleton(queueService);
        services.AddSingleton<CustomerController>();
        services.AddSingleton<PurchaseController>();
        services.AddSingleton<ProductController>();
        services.AddSingleton(new AzureFileShareService(connStr, "uploads", queueService));
        services.AddSingleton(new BlobService(connStr, queueService));
        services.AddLogging();
    })
    .Build();

host.Run();
