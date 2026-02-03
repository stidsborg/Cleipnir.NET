using Cleipnir.ResilientFunctions.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;

namespace Cleipnir.Flows.Sample.ConsoleApp.Retry;

public static class Example
{
    public static async Task Do()
    {
        const string connStr = "Server=localhost;Database=retryflows;User Id=postgres;Password=Pa55word!; Include Error Detail=true;";
        await DatabaseHelper.RecreateDatabase(connStr);
        var store = new PostgreSqlFunctionStore(connStr);
        await store.Initialize();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<RetryFlow>();

        var flowsContainer = new FlowsContainer(
            store,
            serviceCollection.BuildServiceProvider(),
            new Options(unhandledExceptionHandler: Console.WriteLine)
        );

        var flows = new Flows<RetryFlow>(nameof(RetryFlow), flowsContainer);
        var flowId = "MK-54321";
        await flows.Schedule(flowId);

        Console.ReadLine();
    }
}