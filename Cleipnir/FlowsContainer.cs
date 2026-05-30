using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cleipnir.ResilientFunctions;
using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleipnir.Flows;

public class FlowsContainer : IDisposable
{
    internal readonly IServiceProvider ServiceProvider;
    internal readonly FunctionsRegistry FunctionRegistry;
    private readonly Dictionary<string, Type> _registeredFlows = new();
    private readonly Lock _lock = new();

    public FunctionsRegistry Functions => FunctionRegistry;

    public FlowsContainer(IFunctionStore flowStore, IServiceProvider serviceProvider, Settings? settings = null)
    {
        ServiceProvider = serviceProvider;
        settings ??= new Settings();

        if (settings.UnhandledExceptionHandler == null && serviceProvider.GetService<ILogger>() != null)
        {
            var logger = serviceProvider.GetRequiredService<ILogger>();
            settings = new Settings(
                    unhandledExceptionHandler: ex => logger.LogError(ex, "Unhandled exception in Cleipnir"),
                    retentionPeriod: settings.RetentionPeriod,
                    retentionCleanUpFrequency: settings.RetentionCleanUpFrequency,
                    enableWatchdogs: settings.EnableWatchdogs,
                    watchdogCheckFrequency: settings.WatchdogCheckFrequency,
                    messagesPullFrequency: settings.MessagesPullFrequency,
                    messagesDefaultMaxWaitForCompletion: settings.MessagesDefaultMaxWaitForCompletion,
                    delayStartup: settings.DelayStartup,
                    maxParallelRetryInvocations: settings.MaxParallelRetryInvocations,
                    serializer: settings.Serializer,
                    utcNow: settings.UtcNow,
                    replicaHeartbeatFrequency: settings.ReplicaHeartbeatFrequency
                );
        }

        FunctionRegistry = new FunctionsRegistry(flowStore, settings);
    }

    internal void EnsureNoExistingRegistration(string flowName, Type flowType)
    {
        lock (_lock)
            if (_registeredFlows.TryGetValue(flowName, out var existingFlowType) && flowType != existingFlowType)
                throw new InvalidOperationException($"Flow with name '{flowName}' for type '{flowType}' has already been registered for different type: '{existingFlowType}'");
            else
                _registeredFlows[flowName] = flowType;
    }

    public void Dispose() => FunctionRegistry.Dispose();
    public Task ShutdownGracefully(TimeSpan? maxWait = null) => FunctionRegistry.ShutdownGracefully(maxWait);

    public Flows<TFlow> RegisterAnonymousFlow<TFlow>(Func<TFlow>? flowFactory = null, string? flowName = null, FlowOptions? options = null) where TFlow : Flow
    {
        flowName ??= typeof(TFlow).Name;
        return new Flows<TFlow>(flowName, flowsContainer: this, options ?? new FlowOptions(), flowFactory);
    }

    public Flows<TFlow, TParam> RegisterAnonymousFlow<TFlow, TParam>(Func<TFlow>? flowFactory = null, string? flowName = null, FlowOptions? options = null) where TFlow : Flow<TParam> where TParam : notnull
    {
        flowName ??= typeof(TFlow).Name;
        return new Flows<TFlow, TParam>(flowName, flowsContainer: this, options ?? new FlowOptions(), flowFactory);
    }

    public Flows<TFlow, TParam, TResult> RegisterAnonymousFlow<TFlow, TParam, TResult>(Func<TFlow>? flowFactory = null, string? flowName = null, FlowOptions? options = null) where TFlow : Flow<TParam, TResult> where TResult : notnull where TParam : notnull
    {
        flowName ??= typeof(TFlow).Name;
        return new Flows<TFlow, TParam, TResult>(flowName, flowsContainer: this, options ?? new FlowOptions(), flowFactory);
    }

    public static FlowsContainer Create(
        IServiceProvider? serviceProvider = null,
        IFunctionStore? functionStore = null,
        Settings? settings = null)
        => new(
            functionStore ?? new InMemoryFunctionStore(),
            serviceProvider ?? new ServiceCollection().BuildServiceProvider(),
            settings ?? new Settings()
        );
}
