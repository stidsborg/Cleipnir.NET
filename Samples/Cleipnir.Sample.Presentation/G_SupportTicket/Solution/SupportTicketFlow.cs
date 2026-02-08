namespace Cleipnir.Flows.Sample.Presentation.G_SupportTicket.Solution;

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
                () => RequestSupportForTicket(supportTicketId, customerSupportAgent, iteration: i)
            );

            var response = await Message<SupportTicketResponse>(
                m => m.Iteration == i,
                TimeSpan.FromMinutes(15)
            );

            if (response is SupportTicketTaken)
                return; //ticket was taken in iteration i
        }
    }

    private Task RequestSupportForTicket(Guid supportTicketId, string customerSupportAgent, int iteration)
        => MessageBroker.Send(new TakeSupportTicket(supportTicketId, customerSupportAgent, iteration));
}