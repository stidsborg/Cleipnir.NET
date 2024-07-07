﻿using Cleipnir.Flows.Sample.MicrosoftOpen.Flows.MessageDriven.Other;
using Cleipnir.Flows.Sample.MicrosoftOpen.Flows.Rpc;
using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Reactive.Extensions;
using Route = Cleipnir.ResilientFunctions.Domain.Route;

namespace Cleipnir.Flows.Sample.MicrosoftOpen.Flows.MessageDriven;

public class MessageDrivenOrderFlow(Bus bus) : Flow<Order>,
    ISubscribeTo<FundsReserved>,
    ISubscribeTo<ProductsShipped>,
    ISubscribeTo<FundsCaptured>,
    ISubscribeTo<OrderConfirmationEmailSent>
{
    public static RoutingInfo Correlate(FundsReserved msg) => Route.To(msg.OrderId);
    public static RoutingInfo Correlate(ProductsShipped msg) => Route.To(msg.OrderId);
    public static RoutingInfo Correlate(FundsCaptured msg) => Route.To(msg.OrderId);
    public static RoutingInfo Correlate(OrderConfirmationEmailSent msg) => Route.To(msg.OrderId);
    
    private static readonly TimeSpan? MaxWait = TimeSpan.FromSeconds(30); 
    
    public override async Task Run(Order order)
    {
        Console.WriteLine("MessageDriven-OrderFlow Started");
        var transactionId = await Effect.Capture("TransactionId", Guid.NewGuid);

        await ReserveFunds(order, transactionId);
        await Messages.FirstOfType<FundsReserved>(MaxWait);

        await ShipProducts(order);
        var productsShipped = await Messages.FirstOfType<ProductsShipped>(MaxWait);
        
        await CaptureFunds(order, transactionId);
        await Messages.FirstOfType<FundsCaptured>(MaxWait);

        await SendOrderConfirmationEmail(order, productsShipped);
        await Messages.FirstOfType<OrderConfirmationEmailSent>(MaxWait);
        
        Console.WriteLine("MessageDriven-OrderFlow Completed");
    }
    
    private Task ReserveFunds(Order order, Guid transactionId) 
        => bus.Send(new ReserveFunds(order.OrderId, order.TotalPrice, transactionId, order.CustomerId));
    private Task ShipProducts(Order order)
        => bus.Send(new ShipProducts(order.OrderId, order.CustomerId, order.ProductIds));
    private Task CaptureFunds(Order order, Guid transactionId)
        => bus.Send(new CaptureFunds(order.OrderId, order.CustomerId, transactionId));
    private Task SendOrderConfirmationEmail(Order order, ProductsShipped productsShipped)
        => bus.Send(new SendOrderConfirmationEmail(order.OrderId, order.CustomerId, productsShipped.TrackAndTraceNumber));
}
