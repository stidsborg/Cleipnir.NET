using Cleipnir.ResilientFunctions.Domain;

namespace Cleipnir.Flows.Sample.ConsoleApp.Retry;

[GenerateFlows]
public class RetryFlow : Flow
{
    private readonly RetryPolicy _retryPolicy = RetryPolicy.CreateConstantDelay(interval: TimeSpan.FromSeconds(1), suspendThreshold: TimeSpan.Zero);
    
    public override async Task Run()
    {
        await Capture(async () =>
        {
            var i = await Effect.CreateOrGet("i", 0);
            while (true)
            {
                Console.WriteLine($"Retrying: {i}");
                i++;
                await Effect.Upsert("i", i);
                throw new TimeoutException();
            }
        }, _retryPolicy);
    }
}