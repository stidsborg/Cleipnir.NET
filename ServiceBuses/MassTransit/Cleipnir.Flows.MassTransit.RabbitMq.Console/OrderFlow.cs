using Cleipnir.Flows.MassTransit.RabbitMq.Console.Other;
using Cleipnir.ResilientFunctions.Domain.Exceptions.Commands;
using MassTransit;

namespace Cleipnir.Flows.MassTransit.RabbitMq.Console;

[GenerateFlows]
public class OrderFlow(IBus bus) : Flow<Order>
{
    public override async Task Run(Order order)
    {
        var transactionId = await Capture(Guid.NewGuid);
        var fundsReserved = false;
        var productsShipped = false;
        var fundsCaptured = false;

        try
        {
            await ReserveFunds(order, transactionId);
            await Message<FundsReserved>();
            fundsReserved = true;

            await ShipProducts(order);
            var productsShippedMsg = await Message<ProductsShipped>();
            productsShipped = true;
            var trackAndTraceNumber = productsShippedMsg.TrackAndTraceNumber;

            await CaptureFunds(order, transactionId);
            await Message<FundsCaptured>();
            fundsCaptured = true;

            await SendOrderConfirmationEmail(order, trackAndTraceNumber);
            await Message<OrderConfirmationEmailSent>();
        }
        catch (Exception e) when (e is not SuspendInvocationException)
        {
            await Compensate(order, transactionId, fundsReserved, productsShipped, fundsCaptured);
        }
    }

    private async Task Compensate(Order order, Guid transactionId, bool fundsReserved, bool productsShipped, bool fundsCaptured)
    {
        if (productsShipped)
            await CancelShipment(order);

        if (fundsCaptured)
            await ReverseTransaction(order, transactionId);
        else if (fundsReserved)
            await CancelReservation(order, transactionId);
    }

    private Task ReserveFunds(Order order, Guid transactionId)
        => Capture(
            "ReserveFunds",
            () => bus.Publish(new ReserveFunds(order.OrderId, order.TotalPrice, transactionId, order.CustomerId))
        );

    private Task ShipProducts(Order order)
        => Capture(
            "ShipProducts",
            () => bus.Publish(new ShipProducts(order.OrderId, order.CustomerId, order.ProductIds))
        );

    private Task CaptureFunds(Order order, Guid transactionId)
        => Capture(
            "CaptureFunds",
            () => bus.Publish(new CaptureFunds(order.OrderId, order.CustomerId, transactionId))
        );

    private Task SendOrderConfirmationEmail(Order order, string trackAndTraceNumber)
        => Capture(
            "SendOrderConfirmationEmail",
            () => bus.Publish(new SendOrderConfirmationEmail(order.OrderId, order.CustomerId, trackAndTraceNumber))
        );

    private Task CancelShipment(Order order)
        => Capture(() => bus.Publish(new CancelShipment(order.OrderId)));
    private Task ReverseTransaction(Order order, Guid transactionId)
        => Capture(() => bus.Publish(new ReverseTransaction(order.OrderId, transactionId)));
    private Task CancelReservation(Order order, Guid transactionId)
        => Capture(() => bus.Publish(new CancelFundsReservation(order.OrderId, transactionId)));
}
