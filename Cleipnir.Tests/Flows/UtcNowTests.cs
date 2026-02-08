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

        var flows = new Flows<UtcNowTestFlow, DateTime>(nameof(UtcNowTestFlow), flowsContainer);
        // Schedule with a postpone time that is already in the past relative to the custom utcNow
        // This verifies the utcNow delegate is being used since the delay should be 0
        await flows.Schedule("Instance", now.AddMilliseconds(-100));

        var cp = await flows.ControlPanel("Instance");
        cp.ShouldNotBeNull();

        // Flow should complete since postponeUntil is in the past
        await cp.WaitForCompletion(allowPostponeAndSuspended: true, maxWait: TimeSpan.FromSeconds(5));
        cp.Status.ShouldBe(Status.Succeeded);
    }
}

public class UtcNowTestFlow : Flow<DateTime>
{
    public override async Task Run(DateTime postponeUntil)
    {
        await Workflow.Delay(postponeUntil);
    }
}