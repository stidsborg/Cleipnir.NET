using Cleipnir.Flows.Sample.Presentation.Solutions.B_OrderFlowMessaging.Other;

namespace Cleipnir.Flows.Sample.Presentation.Solutions.B_OrderFlowMessaging;

[GenerateFlows]
public class OrderFlow(Bus bus) : Flow<Order>
{
    public override async Task Run(Order order)
    {
        var transactionId = await Effect.CreateOrGet("TransactionId", Guid.NewGuid());

        await bus.Send(ReserveFundsMsg(order, transactionId));
        await Message<FundsReserved>();

        await bus.Send(ShipProductsMsg(order));
        var productsShipped = await Message<ProductsShipped>();
        var trackAndTrace = productsShipped.TrackAndTrace;

        await bus.Send(CaptureFundsMsg(order, transactionId));
        await Message<FundsCaptured>();

        await bus.Send(SendOrderConfirmationEmailMsg(order, trackAndTrace));
        await Message<OrderConfirmationEmailSent>();
    }

    #region MessageFactories

    private static ReserveFunds ReserveFundsMsg(Order order, Guid transactionId)
        => new ReserveFunds(order.OrderId, order.TotalPrice, transactionId, order.CustomerId);
    private static ShipProducts ShipProductsMsg(Order order)
        => new ShipProducts(order.OrderId, order.CustomerId, order.ProductIds);
    private static CaptureFunds CaptureFundsMsg(Order order, Guid transactionId)
        => new CaptureFunds(order.OrderId, order.CustomerId, transactionId);
    private static SendOrderConfirmationEmail SendOrderConfirmationEmailMsg(Order order, string trackAndTrace)
        => new SendOrderConfirmationEmail(order.OrderId, order.CustomerId, trackAndTrace);

    #endregion
}
