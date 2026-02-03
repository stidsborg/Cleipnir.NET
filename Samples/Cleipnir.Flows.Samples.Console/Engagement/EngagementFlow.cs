namespace Cleipnir.Flows.Sample.ConsoleApp.Engagement;

public class EngagementFlow : Flow<string>
{
    public override async Task Run(string candidateEmail)
    {
        await Effect.Capture(
            "InitialCorrespondence",
            SendEngagementInitialCorrespondence
        );

        for (var i = 0; i < 10; i++)
        {
            var response = await Message<object>(waitFor: TimeSpan.FromHours(1));

            if (response is EngagementAccepted)
            {
                await Effect.Capture(
                    "NotifyHR",
                    () => NotifyHR(candidateEmail)
                );
                return;
            }

            if (response is EngagementRejected)
            {
                await Effect.Capture(
                    $"Reminder#{i}",
                    SendEngagementReminder
                );
                continue;
            }

            // Timeout occurred
            await Effect.Capture(
                $"Reminder#{i}",
                SendEngagementReminder
            );
        }

        throw new Exception("Max number of retries exceeded");
    }

    private static Task NotifyHR(string candidateEmail) => Task.CompletedTask;
    private static Task SendEngagementInitialCorrespondence() => Task.CompletedTask;
    private static Task SendEngagementReminder() => Task.CompletedTask;
}

public abstract record EngagementResponse(int Iteration);
public record EngagementAccepted(int Iteration) : EngagementResponse(Iteration);
public record EngagementRejected(int Iteration) : EngagementResponse(Iteration);
