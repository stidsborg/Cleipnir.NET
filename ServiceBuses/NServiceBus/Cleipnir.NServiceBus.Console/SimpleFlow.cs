namespace Cleipnir.Flows.NServiceBus.Console;

public class SimpleFlow : Flow
{
    public override async Task Run()
    {
        var msg = await Message<MyMessage>();
        System.Console.WriteLine($"SimpleFlow({msg}) executed");
    }
}

public class SimpleFlowsHandler(Flows<SimpleFlow> flows) : IHandleMessages<MyMessage>
{
    public Task Handle(MyMessage message, IMessageHandlerContext context)
        => flows.SendMessage(message.Value, message);
}