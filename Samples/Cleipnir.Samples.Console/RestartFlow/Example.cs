using Cleipnir.ResilientFunctions;
using Cleipnir.ResilientFunctions.Storage;
using Cleipnir.ResilientFunctions.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Cleipnir.Flows.Sample.ConsoleApp.RestartFlow;

public static class Example
{
    public static async Task Do()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<RestartFailedFlow>();

        var flowsContainer = new FlowsContainer(
            new InMemoryFunctionStore(),
            serviceCollection.BuildServiceProvider()
        );

        var flows = new Flows<RestartFailedFlow, string>(nameof(RestartFailedFlow), flowsContainer);
        var flowId = "MK-54321";
        try
        {
            await flows.Run(flowId, ""); //invalid parameter    
        }
        catch (Exception)
        {
            // ignored
        }
        
        var controlPanel = await flows.ControlPanel(flowId);
        controlPanel!.Param = "valid parameter";
        await controlPanel.ScheduleRestart().Completion();
    }
}