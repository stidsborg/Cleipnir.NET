namespace Cleipnir.Flows.Sample.MicrosoftOpen.Flows.MessageDriven.Other;

public record EventsAndCommands;

// Response base types for Message<T>() pattern
public abstract record FundsReservationResponse(string OrderId) : EventsAndCommands;
public abstract record ProductsShippingResponse(string OrderId) : EventsAndCommands;
public abstract record FundsCaptureResponse(string OrderId) : EventsAndCommands;
public abstract record OrderConfirmationEmailResponse(string OrderId) : EventsAndCommands;

public record OrderConfirmationEmailSent(string OrderId, Guid CustomerId) : OrderConfirmationEmailResponse(OrderId);

public record ReserveFunds(string OrderId, decimal Amount, Guid TransactionId, Guid CustomerId) : EventsAndCommands;
public record FundsReserved(string OrderId) : FundsReservationResponse(OrderId);
public record ShipProducts(string OrderId, Guid CustomerId, IEnumerable<Guid> ProductIds) : EventsAndCommands;
public record ProductsShipped(string OrderId, string TrackAndTraceNumber) : ProductsShippingResponse(OrderId);

public record SendOrderConfirmationEmail(string OrderId, Guid CustomerId, string TrackAndTraceNumber) : EventsAndCommands;

public record CaptureFunds(string OrderId, Guid CustomerId, Guid TransactionId) : EventsAndCommands;
public record FundsCaptured(string OrderId) : FundsCaptureResponse(OrderId);
public record CancelFundsReservation(string OrderId, Guid TransactionId) : EventsAndCommands;
public record FundsReservationCancelled(string OrderId) : EventsAndCommands;
public record FundsReservationFailed(string OrderId) : FundsReservationResponse(OrderId);
public record FundsCaptureFailed(string OrderId) : FundsCaptureResponse(OrderId);
public record ProductsShipmentFailed(string OrderId) : ProductsShippingResponse(OrderId);
public record OrderConfirmationEmailFailed(string OrderId) : OrderConfirmationEmailResponse(OrderId);
public record CancelProductsShipment(string OrderId) : EventsAndCommands;
public record ReverseTransaction(string OrderId, Guid TransactionId) : EventsAndCommands;