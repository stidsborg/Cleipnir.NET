using Cleipnir.Flows.Sample.MicrosoftOpen.Flows.MessageDriven.Other;

namespace Cleipnir.Flows.Sample.MicrosoftOpen.Flows.MessageDriven.Solution;

public class MessageDrivenOrderFlow(Bus bus) : Flow<Order>
{
    public override async Task Run(Order order)
    {
        var transactionId = await Capture(Guid.NewGuid);

        await ReserveFunds(order, transactionId);
        var reservationResponse = await Message<FundsReservationResponse>(TimeSpan.FromSeconds(10));
        if (reservationResponse is not FundsReserved)
            return;

        await ShipProducts(order);
        var shippingResponse = await Message<ProductsShippingResponse>(TimeSpan.FromMinutes(5));
        if (shippingResponse is not ProductsShipped productsShipped)
        {
            await CleanUp(FailedAt.ProductsShipped, order, transactionId);
            return;
        }
        var trackAndTraceNumber = productsShipped.TrackAndTraceNumber;

        await CaptureFunds(order, transactionId);
        var captureResponse = await Message<FundsCaptureResponse>(TimeSpan.FromSeconds(10));
        if (captureResponse is not FundsCaptured)
        {
            await CleanUp(FailedAt.FundsCaptured, order, transactionId);
            return;
        }

        await SendOrderConfirmationEmail(order, trackAndTraceNumber);
        await Message<OrderConfirmationEmailResponse>(TimeSpan.FromSeconds(10));
    }

    private async Task CleanUp(FailedAt failedAt, Order order, Guid transactionId)
    {
        switch (failedAt)
        {
            case FailedAt.FundsReserved:
                break;
            case FailedAt.ProductsShipped:
                await CancelFundsReservation(order, transactionId);
                break;
            case FailedAt.FundsCaptured:
                await CancelFundsReservation(order, transactionId);
                await CancelProductsShipment(order);
                break;
            case FailedAt.OrderConfirmationEmailSent:
                await CancelProductsShipment(order);
                await ReversePayment(order, transactionId);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(failedAt), failedAt, null);
        }

        throw new OrderProcessingException($"Order processing failed at: '{failedAt}'");
    }

    private enum FailedAt
    {
        FundsReserved,
        ProductsShipped,
        FundsCaptured,
        OrderConfirmationEmailSent,
    }

    private Task ReserveFunds(Order order, Guid transactionId)
        => Capture(() => bus.Send(new ReserveFunds(order.OrderId, order.TotalPrice, transactionId, order.CustomerId)));
    private Task ShipProducts(Order order)
        => Capture(() => bus.Send(new ShipProducts(order.OrderId, order.CustomerId, order.ProductIds)));
    private Task CaptureFunds(Order order, Guid transactionId)
        => Capture(() => bus.Send(new CaptureFunds(order.OrderId, order.CustomerId, transactionId)));
    private Task SendOrderConfirmationEmail(Order order, string trackAndTraceNumber)
        => Capture(() => bus.Send(new SendOrderConfirmationEmail(order.OrderId, order.CustomerId, trackAndTraceNumber)));
    private Task CancelProductsShipment(Order order)
        => Capture(() => bus.Send(new CancelProductsShipment(order.OrderId)));
    private Task CancelFundsReservation(Order order, Guid transactionId)
        => Capture(() => bus.Send(new CancelFundsReservation(order.OrderId, transactionId)));
    private Task ReversePayment(Order order, Guid transactionId)
        => Capture(() => bus.Send(new ReverseTransaction(order.OrderId, transactionId)));
}
