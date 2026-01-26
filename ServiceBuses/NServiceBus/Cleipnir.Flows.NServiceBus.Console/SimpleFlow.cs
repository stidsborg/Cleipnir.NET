namespace Cleipnir.Flows.NServiceBus.Console;

[GenerateFlows]
public class SimpleFlow : Flow
{
    public override async Task Run()
    {
        var msg = await Message<MyMessage>();
        System.Console.WriteLine($"SimpleFlow({msg}) executed");
    }
}

public class SimpleFlowsHandler(SimpleFlows flows) : IHandleMessages<MyMessage>
{
    public Task Handle(MyMessage message, IMessageHandlerContext context)
        => flows.SendMessage(message.Value, message);
}