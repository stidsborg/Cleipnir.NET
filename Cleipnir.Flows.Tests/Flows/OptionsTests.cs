using Cleipnir.Flows.AspNet;
using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Domain.Exceptions;
using Cleipnir.ResilientFunctions.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cleipnir.Flows.Tests.Flows;

[TestClass]
public class OptionsTests
{
    [TestMethod]
    public async Task SimpleFlowCompletesSuccessfully()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddFlows(c => c
            .UseInMemoryStore()
            .WithOptions(new Options(
                messagesDefaultMaxWaitForCompletion: TimeSpan.FromDays(1),
                watchdogCheckFrequency: TimeSpan.FromMilliseconds(100)
            ))
            .RegisterFlow<OptionsTestWithOverriddenOptionsFlow, OptionsTestWithOverriddenOptionsFlows>(
                flowsFactory: sp => new OptionsTestWithOverriddenOptionsFlows(
                    sp.GetRequiredService<FlowsContainer>(),
                    options: new FlowOptions(messagesDefaultMaxWaitForCompletion: TimeSpan.Zero)
                )
            )
            .RegisterFlow<OptionsTestWithDefaultProvidedOptionsFlow, OptionsTestWithDefaultProvidedOptionsFlows>()
        );

        var sp = serviceCollection.BuildServiceProvider();
        var flowsWithOverridenOptions = sp.GetRequiredService<OptionsTestWithOverriddenOptionsFlows>();

        await Should.ThrowAsync<InvocationSuspendedException>(
            () => flowsWithOverridenOptions.Run("Id")
        );

        var flowsWithDefaultProvidedOptions = sp.GetRequiredService<OptionsTestWithDefaultProvidedOptionsFlows>();
        await flowsWithDefaultProvidedOptions.Schedule("Id");

        await Task.Delay(100);

        var controlPanel = await flowsWithDefaultProvidedOptions.ControlPanel("Id");
        controlPanel.ShouldNotBeNull();
        // Flow may be Executing (waiting for message) or Suspended depending on timing
        controlPanel.Status.ShouldBeOneOf(Status.Executing, Status.Suspended);

        await controlPanel.Messages.Append(new StringWrapper("Hello"));

        await controlPanel.WaitForCompletion(allowPostponeAndSuspended: true);        
    }

   
    
    [TestMethod]
    public async Task FlowNameCanBeSpecifiedFromTheOutside()
    {
        var serviceCollection = new ServiceCollection();
        var store = new InMemoryFunctionStore();
        var storedType = await store.TypeStore.InsertOrGetStoredType("SomeOtherFlowName");
        
        serviceCollection.AddFlows(c => c
            .UseInMemoryStore(store)
            .WithOptions(new Options(
                messagesDefaultMaxWaitForCompletion: TimeSpan.FromDays(1),
                watchdogCheckFrequency: TimeSpan.FromMilliseconds(100)
            ))
            .RegisterFlow<SimpleFlow, SimpleFlows>(
                flowsFactory: sp => new SimpleFlows(
                    sp.GetRequiredService<FlowsContainer>(),
                    flowName: "SomeOtherFlowName"
                )
            )
        );

        var sp = serviceCollection.BuildServiceProvider();
        var flows = sp.GetRequiredService<SimpleFlows>();
        await flows.Run("Id");
        var sf = await store.GetFunction(StoredId.Create(storedType, "Id"));
        sf.ShouldNotBeNull();
        sf.Status.ShouldBe(Status.Succeeded);
    }
}

[GenerateFlows]
public class OptionsTestWithOverriddenOptionsFlow : Flow
{
    public override async Task Run()
    {
        await Message<StringWrapper>();
    }
}

[GenerateFlows]
public class OptionsTestWithDefaultProvidedOptionsFlow : Flow
{
    public override async Task Run()
    {
        await Message<StringWrapper>();
    }
}

public record StringWrapper(string Value);

[GenerateFlows]
public class SimpleFlow : Flow
{
    public override Task Run() => Task.CompletedTask;
}