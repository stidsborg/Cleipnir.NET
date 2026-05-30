using Cleipnir.ResilientFunctions.Storage;
using Microsoft.Extensions.DependencyInjection;
using Cleipnir.ResilientFunctions.Domain;

namespace Cleipnir.Flows.Sample.ConsoleApp.AtLeastOnce;

public static class Example
{
    public static async Task Do()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<AtLeastOnceFlow>();

        var flowsContainer = new FlowsContainer(
            new InMemoryFunctionStore(),
            serviceCollection.BuildServiceProvider()
        );

        var flows = new Flows<AtLeastOnceFlow, string, string>(nameof(AtLeastOnceFlow), flowsContainer);
        var hashCode = "¤SOME_#A$H";
        var solution = await flows.Run(hashCode, hashCode);
        System.Console.WriteLine("Solution was: " + solution);
    }
}