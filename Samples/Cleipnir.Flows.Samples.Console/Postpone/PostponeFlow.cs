namespace Cleipnir.Flows.Sample.ConsoleApp.Postpone;

[GenerateFlows]
public class PostponeFlow : Flow<string>
{
    private readonly ExternalService _externalService = new();
    
    public override async Task Run(string orderId)
    {
        if (await _externalService.IsOverloaded())
            await Delay(TimeSpan.FromMinutes(10));

        //execute rest of the flow
    }
}