using System.Net;

namespace Cleipnir.Flows.Sample.ConsoleApp.TransientFailureHandling;

public class TransientFailureFlow : Flow<string>
{
    private static readonly HttpClient HttpClient = new();
    
    public override async Task Run(string orderId)
    {
        for (var i = 1; i < 5; i++)
        {
            var success = await Capture(async () =>
            {
                var response = await HttpClient.PostAsync(
                    "http://orderservice/api/orders/",
                    new StringContent(orderId)
                );
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    return false;
               
                //otherwise we assume it is not a transient error and thrown an exception instead if not success
                response.EnsureSuccessStatusCode();

                return true;
            });
            if (!success)
                await Delay(TimeSpan.FromSeconds(i ^ 2));
            else
                return;
        }

        throw new TimeoutException($"Unable to save order '{orderId}'");
    }
}