namespace Cleipnir.Flows.Sample.ConsoleApp.Middleware;

[GenerateFlows]
public class MiddlewareFlow : Flow<string>
{
    public override Task Run(string param)
    {
        var randomValue = Random.Shared.Next(0, 3);
        if (randomValue == 1)
            throw new TimeoutException();
        if (randomValue == 2)
            return Delay(TimeSpan.Zero);
        
        return Task.CompletedTask;
    }
}