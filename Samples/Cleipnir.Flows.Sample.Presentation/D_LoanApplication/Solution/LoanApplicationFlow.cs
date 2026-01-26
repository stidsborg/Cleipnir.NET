namespace Cleipnir.Flows.Sample.Presentation.D_LoanApplication.Solution;

[GenerateFlows]
public class LoanApplicationFlow : Flow<LoanApplication>
{
    public override async Task Run(LoanApplication loanApplication)
    {
        await Effect.Capture(
            () => Bus.Publish(new PerformCreditCheck(loanApplication.Id, loanApplication.CustomerId, loanApplication.Amount))
        );


        var timeout = await Workflow.UtcNow() + TimeSpan.FromMinutes(15);
        var outcomes = new List<CreditCheckOutcome>();
        for (var i = 0; i < 3; i++)
        {
            var outcome = await Message<CreditCheckOutcome>(timeout);
            if (outcome == null) break;
            outcomes.Add(outcome);
        }
        
        if (outcomes.Count < 2)
            await Bus.Publish(new LoanApplicationRejected(loanApplication));
        else
            await Bus.Publish(
                outcomes.All(o => o.Approved)
                    ? new LoanApplicationApproved(loanApplication)
                    : new LoanApplicationRejected(loanApplication)
            );
    }
}