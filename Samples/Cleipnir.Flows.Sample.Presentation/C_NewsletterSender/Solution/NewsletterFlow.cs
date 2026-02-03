using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace Cleipnir.Flows.Sample.Presentation.C_NewsletterSender.Solution;

public class NewsletterFlow : Flow<MailAndRecipients>
{
    public override async Task Run(MailAndRecipients mailAndRecipients)
    {
        var (recipients, subject, content) = mailAndRecipients;
        using var client = new SmtpClient();
        await client.ConnectAsync("mail.smtpbucket.com", 8025);

        var atRecipient = await Effect.CreateOrGet("AtRecipient", 0);
        for (; atRecipient < mailAndRecipients.Recipients.Count; atRecipient++)
        {
            var recipient = recipients[atRecipient];
            var message = new MimeMessage();
            message.To.Add(new MailboxAddress(recipient.Name, recipient.Address));
            message.From.Add(new MailboxAddress("Cleipnir.NET", "newsletter@cleipnir.net"));

            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = content };
            await client.SendAsync(message);

            await Effect.Upsert("AtRecipient", atRecipient);
        }
    }
}

public record EmailAddress(string Name, string Address);
public record MailAndRecipients(
    List<EmailAddress> Recipients,
    string Subject,
    string Content
);