using MassTransit;

namespace Cleipnir.Flows.MassTransit.Console;

[GenerateFlows]
public class SimpleFlow : Flow
{
    public override async Task Run()
    {
        var msg = await Message<MyMessage>();
        System.Console.WriteLine($"SimpleFlow({msg}) executed");
    }
}

public class SimpleFlowsHandler(SimpleFlows simpleFlows) : IConsumer<MyMessage>
{
    public Task Consume(ConsumeContext<MyMessage> context) 
        => simpleFlows.SendMessage(context.Message.Value, context.Message);
}
