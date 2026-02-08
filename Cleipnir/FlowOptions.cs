using System;
using Cleipnir.ResilientFunctions.Domain;

namespace Cleipnir.Flows;

public class FlowOptions
{
    public static FlowOptions Default { get; } = new();

    internal TimeSpan? RetentionPeriod { get; }
    internal bool? EnableWatchdogs { get; }
    internal int? MaxParallelRetryInvocations { get; }
    internal TimeSpan? MessagesDefaultMaxWaitForCompletion { get; }

    public FlowOptions(
        TimeSpan? retentionPeriod = null,
        bool? enableWatchdogs = null,
        TimeSpan? messagesDefaultMaxWaitForCompletion = null,
        int? maxParallelRetryInvocations = null
    )
    {
        RetentionPeriod = retentionPeriod;
        EnableWatchdogs = enableWatchdogs;
        MessagesDefaultMaxWaitForCompletion = messagesDefaultMaxWaitForCompletion;
        MaxParallelRetryInvocations = maxParallelRetryInvocations;
    }

    public FlowOptions Merge(Options options)
    {
        return new FlowOptions(
            RetentionPeriod ?? options.RetentionPeriod,
            EnableWatchdogs ?? options.EnableWatchdogs,
            MessagesDefaultMaxWaitForCompletion ?? options.MessagesDefaultMaxWaitForCompletion,
            MaxParallelRetryInvocations ?? options.MaxParallelRetryInvocations
        );
    }

    internal LocalSettings MapToLocalSettings()
        => new(RetentionPeriod, EnableWatchdogs, MessagesDefaultMaxWaitForCompletion, MaxParallelRetryInvocations);
}
