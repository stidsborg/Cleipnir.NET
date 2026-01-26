namespace Cleipnir.Flows.Sample.Presentation.G_SupportTicket;

[GenerateFlows]
public class SupportTicketFlow : Flow<SupportTicketRequest>
{
    public override async Task Run(SupportTicketRequest request)
    {
        var (supportTicketId, customerSupportAgents) = request;

        for (var i = 0;; i++)
        {
            var customerSupportAgent = customerSupportAgents[i % customerSupportAgents.Length]; 
            await Effect.Capture(
                $"RequestSupportForTicket{i}",
                () => RequestSupportForTicket(supportTicketId, customerSupportAgent, iteration: i)
            );

            //wait for ticket taken, ticket rejected or timeout
        }
    }

    private Task RequestSupportForTicket(Guid supportTicketId, string customerSupportAgent, int iteration)
        => MessageBroker.Send(new TakeSupportTicket(supportTicketId, customerSupportAgent, iteration));
}