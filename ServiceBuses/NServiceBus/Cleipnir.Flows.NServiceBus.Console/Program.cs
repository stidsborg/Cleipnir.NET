﻿using Cleipnir.Flows.AspNet;
using Cleipnir.ResilientFunctions.Helpers;
using Cleipnir.ResilientFunctions.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cleipnir.Flows.NServiceBus.Console;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var host = await CreateHostBuilder([]).StartAsync();
        var bus = host.Services.GetRequiredService<IMessageSession>();
        var store = host.Services.GetRequiredService<IFunctionStore>();
        
        var simpleFlowStoredType = await store.TypeStore.InsertOrGetStoredType(nameof(SimpleFlow));
        
        var testSize = 1_000;
        for (var i = 0; i < testSize; i++)
            await bus.Publish(new MyMessage(i.ToString()));

        while (true)
        {
            var succeeded = await store.GetSucceededFunctions(
                simpleFlowStoredType,
                DateTime.UtcNow.Ticks + 1_000_000
            ).SelectAsync(f => f.Count);
            if (succeeded == testSize)
                break;

            await Task.Delay(250);
        }

        System.Console.WriteLine("All completed");
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddFlows(c => c
                        .UseInMemoryStore()
                        .RegisterFlowsAutomatically()
                );

            }).UseNServiceBus(_ =>
                {
                    var endpointConfiguration = new EndpointConfiguration("Training");
                    endpointConfiguration.UseTransport<LearningTransport>();
                    endpointConfiguration.EnableInstallers();
                    endpointConfiguration.UseSerialization(new SystemJsonSerializer());

                    return endpointConfiguration;
                }
            );

}
