namespace Cleipnir.Flows.Sample.Presentation.G_SupportTicket.Solution;

public record CommandAndEvents();

public record TakeSupportTicket(Guid TicketId, string CustomerSupportAgent, int Iteration) : CommandAndEvents;
public abstract record SupportTicketResponse(int Iteration) : CommandAndEvents;
public record SupportTicketTaken(int Iteration) : SupportTicketResponse(Iteration);
public record SupportTicketRejected(int Iteration) : SupportTicketResponse(Iteration);