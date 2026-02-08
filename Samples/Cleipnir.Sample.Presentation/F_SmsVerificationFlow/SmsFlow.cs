namespace Cleipnir.Flows.Sample.Presentation.F_SmsVerificationFlow;

public enum MostRecentAttempt
{
    NotStarted,
    CodeExpired,
    IncorrectCode,
    Success,
    MaxAttemptsExceeded
}

public record CodeFromUser(string CustomerPhoneNumber, string Code, DateTime Timestamp);

[GenerateFlows]
public class SmsFlow : Flow<string, MostRecentAttempt>
{
    public override async Task<MostRecentAttempt> Run(string customerPhoneNumber)
    {
        for (var i = 0; i < 5; i++)
        {
            var generatedCode = await Effect.Capture(
                $"SendSms#{i}",
                async () =>
                {
                    var generatedCode = GenerateOneTimeCode();
                    await SendSms(customerPhoneNumber, generatedCode);
                    return generatedCode;
                }
            );

            var codeFromUser = await Message<CodeFromUser>();

            if (IsExpired(codeFromUser))
                continue; // CodeExpired - try again
            else if (codeFromUser.Code == generatedCode)
                return MostRecentAttempt.Success;
        }

        return MostRecentAttempt.MaxAttemptsExceeded;
    }

    private string GenerateOneTimeCode()
    {
        throw new NotImplementedException();
    }

    private Task SendSms(string customerPhoneNumber, string generatedCode)
    {
        throw new NotImplementedException();
    }

    private bool IsExpired(CodeFromUser code)
    {
        throw new NotImplementedException();
    }
}