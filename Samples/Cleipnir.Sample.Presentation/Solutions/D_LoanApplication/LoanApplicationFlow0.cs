using Cleipnir.Flows.Sample.Presentation.Solutions.D_LoanApplication.Other;

namespace Cleipnir.Flows.Sample.Presentation.Solutions.D_LoanApplication;

[GenerateFlows]
public class LoanApplicationFlow0 : Flow<LoanApplication>
{
    public override async Task Run(LoanApplication loanApplication)
    {
        await MessageBroker.Send(new PerformCreditCheck(loanApplication.Id, loanApplication.CustomerId, loanApplication.Amount));

        var outcomes = new List<CreditCheckOutcome>();
        for (var i = 0; i < 3; i++)
            outcomes.Add(await Message<CreditCheckOutcome>());

        CommandAndEvents decision = outcomes.All(o => o.Approved)
            ? new LoanApplicationApproved(loanApplication)
            : new LoanApplicationRejected(loanApplication);

        await MessageBroker.Send(decision);
    }
}