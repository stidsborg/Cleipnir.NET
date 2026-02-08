using Rebus.Handlers;

namespace Cleipnir.Flows.Rebus.Console;

public class SimpleFlow : Flow
{
    public override async Task Run()
    {
        var msg = await Message<MyMessage>();
        System.Console.WriteLine($"SimpleFlow({msg}) executed");
    }
}

public class SimpleFlowsHandler(Flows<SimpleFlow> simpleFlows) : IHandleMessages<MyMessage>
{
    public Task Handle(MyMessage msg) => simpleFlows.SendMessage(msg.Value, msg);
}