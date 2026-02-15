using Cleipnir.ResilientFunctions;
using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Messaging;
using Cleipnir.ResilientFunctions.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cleipnir.Flows.Tests.Flows;

[TestClass]
public class ParamlessFlowsTests
{
    [TestMethod]
    public async Task SimpleFlowCompletesSuccessfully()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<SimpleParamlessFlow>();

        var flowStore = new InMemoryFunctionStore();
        var flowsContainer = new FlowsContainer(
            flowStore,
            serviceCollection.BuildServiceProvider(),
            Options.Default
        );

        var flows = new SimpleParamlessFlows(flowsContainer);
        await flows.Run("someInstanceId");
        
        SimpleParamlessFlow.InstanceId.ShouldBe("someInstanceId");

        var controlPanel = await flows.ControlPanel(instanceId: "someInstanceId");
        controlPanel.ShouldNotBeNull();
        controlPanel.Status.ShouldBe(Status.Succeeded);
    }

    private class SimpleParamlessFlows : Flows<SimpleParamlessFlow>
    {
        public SimpleParamlessFlows(FlowsContainer flowsContainer) 
            : base(nameof(SimpleParamlessFlow), flowsContainer, options: null) { }
    }
    
    public class SimpleParamlessFlow : Flow
    {
        public static string? InstanceId { get; set; } 

        public override async Task Run()
        {
            await Task.Delay(1);
            InstanceId = Workflow.FlowId.Instance.ToString();
        }
    }
    
    [TestMethod]
    public async Task EventDrivenFlowCompletesSuccessfully()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<EventDrivenParamlessFlow>();

        var flowStore = new InMemoryFunctionStore();
        var flowsContainer = new FlowsContainer(
            flowStore,
            serviceCollection.BuildServiceProvider(),
            new Options(watchdogCheckFrequency: TimeSpan.FromMilliseconds(100))
        );

        var flows = new EventDrivenParamlessFlows(flowsContainer); 
        await flows.Schedule("someInstanceId");

        var controlPanel = await flows.ControlPanel(instanceId: "someInstanceId");
        controlPanel.ShouldNotBeNull();
        await controlPanel.BusyWaitUntil(c => c.Status == Status.Suspended);
        
        var eventSourceWriter = flows.MessageWriter("someInstanceId");
        await eventSourceWriter.AppendMessage(new StringMessage("hello"));

        await controlPanel.WaitForCompletion(allowPostponeAndSuspended: true);

        await controlPanel.Refresh();
        controlPanel.ShouldNotBeNull();
        controlPanel.Status.ShouldBe(Status.Succeeded);
    }

    private class EventDrivenParamlessFlows : Flows<EventDrivenParamlessFlow>
    {
        public EventDrivenParamlessFlows(FlowsContainer flowsContainer) 
            : base(nameof(EventDrivenParamlessFlow), flowsContainer, options: null) { }
    }
    
    public class EventDrivenParamlessFlow : Flow
    {
        public override async Task Run()
        {
            await Message<StringMessage>();
        }
    }

    public record StringMessage(string Value);
    
    [TestMethod]
    public async Task FailingFlowCompletesWithError()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<FailingParamlessFlow>();

        var flowStore = new InMemoryFunctionStore();
        var flowsContainer = new FlowsContainer(
            flowStore,
            serviceCollection.BuildServiceProvider(),
            new Options()
        );

        var flows = new FailingParamlessFlows(flowsContainer);
        FailingParamlessFlow.ShouldThrow = true;
        
        await Should.ThrowAsync<FatalWorkflowException<TimeoutException>>(() =>
            flows.Run("someInstanceId")
        );
        
        var controlPanel = await flows.ControlPanel(instanceId: "someInstanceId");
        controlPanel.ShouldNotBeNull();
        controlPanel.Status.ShouldBe(Status.Failed);

        FailingParamlessFlow.ShouldThrow = false;
        await controlPanel.ScheduleRestart().Completion();

        await controlPanel.Refresh();
        controlPanel.Status.ShouldBe(Status.Succeeded);
    }

    private class FailingParamlessFlows : Flows<FailingParamlessFlow>
    {
        public FailingParamlessFlows(FlowsContainer flowsContainer) 
            : base(nameof(FailingParamlessFlow), flowsContainer, options: null) { }
    }
    
    public class FailingParamlessFlow : Flow
    {
        public static bool ShouldThrow = true;
        
        public override Task Run()
        {
            return ShouldThrow 
                ? Task.FromException<TimeoutException>(new TimeoutException()) 
                : Task.CompletedTask;
        }
    }
    
    [TestMethod]
    public async Task FlowCanBeCreatedWithInitialState()
    {
        var flowsContainer = FlowsContainer.Create();
        var flow = new InitialStateFlow();
        var flows = flowsContainer.RegisterAnonymousFlow(
            flowFactory: () => flow
        );

        await flows.Run(
            "SomeInstanceId",
            new InitialState(
                [new MessageAndIdempotencyKey(new StringMessage("InitialMessageValue"))],
                [new InitialEffect(0, "InitialEffectValue")]
            )
        );

        flow.InitialEffectValue.ShouldBe("InitialEffectValue");
        flow.InitialMessageValue.ShouldBe("InitialMessageValue");
    }

    private class InitialStateFlow : Flow
    {
        public string? InitialEffectValue { get; set; }
        public string? InitialMessageValue { get; set; }

        public override async Task Run()
        {
            InitialEffectValue = await Capture(() => "should not be called");
            var msg = await Message<StringMessage>();
            InitialMessageValue = msg.Value;
        }
    }
}