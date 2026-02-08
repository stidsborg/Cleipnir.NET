namespace Cleipnir.Flows.Sample.ConsoleApp.WaitForMessages;

public class WaitForMessagesFlow : Flow<string>
{
    public override async Task Run(string orderId)
    {
        await Message<FundsReserved>();
        await Message<InventoryLocked>();

        Console.WriteLine("Complete order-processing");
    }
}
