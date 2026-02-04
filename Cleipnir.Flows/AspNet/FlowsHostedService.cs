using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cleipnir.Flows.AspNet;

public class FlowsHostedService(IServiceProvider services, IEnumerable<Type> flowsTypes, bool gracefulShutdown) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var flowsType in flowsTypes)
            _ = services.GetService(flowsType); //flow is registered with the flow container when resolved

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        var flowsContainer = services.GetRequiredService<FlowsContainer>();
        
        var shutdownTask = flowsContainer.ShutdownGracefully();
        return gracefulShutdown 
            ? shutdownTask 
            : Task.CompletedTask;
    } 
}