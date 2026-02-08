using Cleipnir.Flows.Sample.Presentation.Solutions.D_LoanApplication.Other;

namespace Cleipnir.Flows.Sample.Presentation.Solutions.D_LoanApplication;

[GenerateFlows]
public class LoanApplicationFlow2 : Flow<LoanApplication>
{
    public override async Task Run(LoanApplication loanApplication)
    {
        await MessageBroker.Send(PerformCreditCheck(loanApplication));

        var outcomes = new List<CreditCheckOutcome>();
        for (var i = 0; i < 3; i++)
        {
            var outcome = await Message<CreditCheckOutcome>(TimeSpan.FromMinutes(15));
            if (outcome == null) break;
            outcomes.Add(outcome);
        }

        if (outcomes.Count < 2)
            await MessageBroker.Send(LoanApplicationRejected(loanApplication));
        else
            await MessageBroker.Send(
                outcomes.All(o => o.Approved)
                    ? LoanApplicationApproved(loanApplication)
                    : LoanApplicationRejected(loanApplication)
            );
    }

    private static PerformCreditCheck PerformCreditCheck(LoanApplication loanApplication)
        => new(loanApplication.Id, loanApplication.CustomerId, loanApplication.Amount);
    private static LoanApplicationRejected LoanApplicationRejected(LoanApplication loanApplication)
        => new(loanApplication);
    private static LoanApplicationApproved LoanApplicationApproved(LoanApplication loanApplication)
        => new(loanApplication);
}