namespace Cleipnir.Flows.Sample.Presentation.B_OrderFlow_Messaging;

[GenerateFlows]
public class OrderFlow : Flow<Order>
{
    /*
     * 1. In-memory execution
     * 2. Suspend execution while waiting for messages
     */
    
    private Bus Bus { get; }

    public OrderFlow(Bus bus) => Bus = bus;
    
    public override async Task Run(Order order)
    {
        var transactionId = await Effect.CreateOrGet("TransactionId", Guid.NewGuid());

        await Bus.Send(new ReserveFunds(order.OrderId, order.TotalPrice, transactionId, order.CustomerId));
        await Message<FundsReserved>();

        await Bus.Send(new ShipProducts(order.OrderId, order.CustomerId, order.ProductIds));
        await Message<ProductsShipped>();
        
        await Bus.Send(new CaptureFunds(order.OrderId, order.CustomerId, transactionId));
        await Message<FundsCaptured>();

        await Bus.Send(new SendOrderConfirmationEmail(order.OrderId, order.CustomerId));
        await Message<OrderConfirmationEmailSent>();
    }
}
