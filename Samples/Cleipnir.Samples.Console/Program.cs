namespace Cleipnir.Flows.Sample.ConsoleApp;

public static class Program
{
    private static async Task Main(string[] args)
    {
        await Retry.Example.Do();
    }
}