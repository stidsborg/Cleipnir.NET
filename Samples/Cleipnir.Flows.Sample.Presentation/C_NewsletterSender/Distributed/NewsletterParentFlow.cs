using Cleipnir.ResilientFunctions.Domain;

namespace Cleipnir.Flows.Sample.Presentation.C_NewsletterSender.Distributed;

[GenerateFlows]
public class NewsletterParentFlow(NewsletterChildFlows childFlows) : Flow<MailAndRecipients>
{
    public override async Task Run(MailAndRecipients param)
    {
        Console.WriteLine("Started NewsletterParentFlow");
        
        var (recipients, subject, content) = param;

        var bulkWork = recipients
            .Chunk(Math.Max(1, (recipients.Count + 2) / 3))
            .Select((emails, child) => new NewsletterChildWork(child, new MailAndRecipients(emails.ToList(), subject, content), Workflow.FlowId))
            .Select(work =>
                new BulkWork<NewsletterChildWork>(
                    Instance: $"{Workflow.FlowId.Instance}_Child{work.Child}",
                    work
                )
            );
        
        await childFlows.BulkSchedule(bulkWork);

        for (var i = 0; i < 3; i++)
            await Message<EmailsSent>(maxWait: TimeSpan.FromMinutes(30));
        
        Console.WriteLine("Finished NewsletterParentFlow");
    }
    
    public record EmailsSent(int Child);
}