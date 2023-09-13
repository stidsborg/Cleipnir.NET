﻿namespace Cleipnir.Flows.Sample.Presentation.D_SupportTicket;

public record CommandAndEvents();

public record TakeSupportTicket(Guid TicketId, string CustomerSupportAgent, int Iteration) : CommandAndEvents;
public record SupportTicketTaken(int Iteration) : CommandAndEvents;
public record SupportTicketRejected(int Iteration) : CommandAndEvents;