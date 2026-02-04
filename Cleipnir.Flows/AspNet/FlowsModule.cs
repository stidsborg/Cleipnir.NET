using System;
using System.Collections.Generic;
using Cleipnir.ResilientFunctions.CoreRuntime;
using Cleipnir.ResilientFunctions.CoreRuntime.Invocation;
using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Helpers;
using Cleipnir.ResilientFunctions.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Cleipnir.Flows.AspNet;

public static class FlowsModule
{
    public static IServiceCollection AddFlows(this IServiceCollection services, Func<FlowsConfigurator, FlowsConfigurator> configure)
    {
        var configurator = new FlowsConfigurator(services);
        configure(configurator);

        if (configurator.OptionsFunc is null)
            services.AddSingleton(new Options());
        else
            services.AddSingleton(configurator.OptionsFunc);
        
        services.AddSingleton<FlowsContainer>();
        services.AddTransient<Workflow>(_ => CurrentFlow.Workflow ?? throw new InvalidOperationException("Workflow is not present outside Flow"));
        services.AddTransient<Effect>(_ => CurrentFlow.Workflow?.Effect ?? throw new InvalidOperationException("Effect is not present outside Flow"));
        
        services.AddHostedService(
            s => new FlowsHostedService(s, configurator.FlowsTypes, configurator.EnableGracefulShutdown)
        );
        return services;
    }
}

public class FlowsConfigurator(IServiceCollection services)
{
    internal bool EnableGracefulShutdown = false;
    internal readonly HashSet<Type> FlowsTypes = new();

    internal Func<IServiceProvider, Options>? OptionsFunc;
    public IServiceCollection Services { get; } = services;

    public FlowsConfigurator UseInMemoryStore(InMemoryFunctionStore? store = null)
    {
        Services.AddSingleton<IFunctionStore>(store ?? new InMemoryFunctionStore());
        return this;
    }
    
    public FlowsConfigurator UseStore(IFunctionStore store)
    {
        Services.AddSingleton(store);
        return this;
    }

    public FlowsConfigurator WithOptions(Options options)
        => WithOptions(_ => options);
    
    public FlowsConfigurator WithOptions(Func<IServiceProvider, Options> optionsFunc)
    {
        OptionsFunc = optionsFunc;
        return this;
    }

    public FlowsConfigurator RegisterFlows<TFlow>() where TFlow : Flow
    {
        var added = FlowsTypes.Add(typeof(Flows<TFlow>));
        if (!added) return this;

        Services.AddScoped<TFlow>();
        Services.AddTransient(sp =>
            new Flows<TFlow>(
                flowName: typeof(TFlow).SimpleQualifiedName(),
                sp.GetRequiredService<FlowsContainer>()
            )
        );

        return this;
    }
    public FlowsConfigurator RegisterFlows<TFlow, TParam>() where TFlow : Flow<TParam> where TParam : notnull
    {
        var added = FlowsTypes.Add(typeof(Flows<TFlow, TParam>));
        if (!added) return this;

        Services.AddScoped<TFlow>();
        Services.AddTransient(sp =>
            new Flows<TFlow, TParam>(
                flowName: typeof(TFlow).SimpleQualifiedName(),
                sp.GetRequiredService<FlowsContainer>()
            )
        );

        return this;
    }
    public FlowsConfigurator RegisterFlows<TFlow, TParam, TResult>() where TFlow : Flow<TParam, TResult> where TParam : notnull
    {
        var added = FlowsTypes.Add(typeof(Flows<TFlow, TParam, TResult>));
        if (!added) return this;

        Services.AddScoped<TFlow>();
        Services.AddTransient(sp =>
            new Flows<TFlow, TParam, TResult>(
                flowName: typeof(TFlow).SimpleQualifiedName(),
                sp.GetRequiredService<FlowsContainer>()
            )
        );

        return this;
    }

    public FlowsConfigurator GracefulShutdown(bool enable)
    {
        EnableGracefulShutdown = enable;
        return this;
    }
}