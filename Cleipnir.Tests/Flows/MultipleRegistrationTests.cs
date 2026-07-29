using Cleipnir.ResilientFunctions.Storage;
using System.Threading.Tasks;
using Cleipnir.ResilientFunctions.Domain;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cleipnir.Flows.Tests.Flows;

[TestClass]
public class MultipleRegistrationTests
{
    [TestMethod]
    public async Task SameFlowTypeWithSameNameCanBeRegisteredSeveralTimes()
    {
        var serviceCollection = new ServiceCollection();
        var store = new InMemoryFunctionStore();
        var flowsContainer = await FlowsContainer.CreateAndStart(store, serviceCollection.BuildServiceProvider());
        _ = new TestFlowType1s(flowsContainer);
        _ = new TestFlowType1s(flowsContainer);
    }
    
    [TestMethod]
    public async Task DifferentFlowTypesWithSameNameCannotBeRegisteredSeveralTimes()
    {
        var serviceCollection = new ServiceCollection();
        var store = new InMemoryFunctionStore();
        var flowsContainer = await FlowsContainer.CreateAndStart(store, serviceCollection.BuildServiceProvider());
        _ = new TestFlowType1s(flowsContainer);
        Should.Throw<InvalidOperationException>(() =>
            new TestFlowType2s(flowsContainer)
        );
    }
    
    private class TestFlowType1 : Flow
    {
        public override Task Run() => Task.CompletedTask;
    }
    
    private class TestFlowType1s : Flows<TestFlowType1>
    {
        public TestFlowType1s(FlowsContainer flowsContainer, FlowOptions? options = null) 
            : base("TestFlow", flowsContainer, options) { }
    }
    
    private class TestFlowType2 : Flow
    {
        public override Task Run() => Task.CompletedTask;
    }
    
    private class TestFlowType2s : Flows<TestFlowType2>
    {
        public TestFlowType2s(FlowsContainer flowsContainer, FlowOptions? options = null) 
            : base("TestFlow", flowsContainer, options) { }
    }
}