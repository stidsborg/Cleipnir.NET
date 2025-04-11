using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cleipnir.Flows.Tests.Flows;

[TestClass]
public class UtcNowTests
{
    [TestMethod]
    public async Task ProvidedUtcNowDelegateIsUsed()
    {
        var now = DateTime.UtcNow;
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<UtcNowTestFlow>();
        var flowsContainer = new FlowsContainer(
            new InMemoryFunctionStore(),
            serviceCollection.BuildServiceProvider(),
            new Options(utcNow: () => now, watchdogCheckFrequency: TimeSpan.FromMilliseconds(100))
        );

        var flows = new UtcNowTestFlows(flowsContainer);
        await flows.Schedule("Instance", now.AddSeconds(1));

        var cp = await flows.ControlPanel("Instance");
        cp.ShouldNotBeNull();

        await cp.BusyWaitUntil(c => c.Status == Status.Postponed);
        await Task.Delay(500);

        await cp.Refresh();
        cp.Status.ShouldBe(Status.Postponed);

        now = now.AddSeconds(2);

        await cp.WaitForCompletion(allowPostponeAndSuspended: true);
    }
}

[GenerateFlows]
public class UtcNowTestFlow : Flow<DateTime>
{
    public override async Task Run(DateTime postponeUntil)
    {
        await Workflow.Delay(postponeUntil);
    }
}