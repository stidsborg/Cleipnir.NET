using System;
using Cleipnir.ResilientFunctions.CoreRuntime;
using Cleipnir.ResilientFunctions.CoreRuntime.Serialization;
using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Domain.Exceptions;

namespace Cleipnir.Flows;

public class Options
{
    public static Options Default { get; } = new();

    internal Action<FrameworkException>? UnhandledExceptionHandler { get; }
    internal TimeSpan? RetentionPeriod { get; }
    internal TimeSpan? RetentionCleanUpFrequency { get; }
    internal TimeSpan? LeaseLength { get; }
    internal bool? EnableWatchdogs { get; }
    internal TimeSpan? WatchdogCheckFrequency { get; }
    internal TimeSpan? DelayStartup { get; }
    internal int? MaxParallelRetryInvocations { get; }
    internal TimeSpan? MessagesPullFrequency { get; }
    internal TimeSpan? MessagesDefaultMaxWaitForCompletion { get; }
    internal ISerializer? Serializer { get; }
    internal UtcNow? UtcNow { get; }

    /// <summary>
    /// Configuration options for Cleipnir
    /// </summary>
    /// <param name="unhandledExceptionHandler">Callback handler for unhandled flow exceptions</param>
    /// <param name="retentionPeriod">Period to keep completed flows before deletion. Default infinite.</param>
    /// <param name="retentionCleanUpFrequency">Retention clean-up check frequency. Default 1 hour when retention period is not infinite.</param>
    /// <param name="leaseLength">Flow lease-length. Leases are automatically renewed. Default 60 seconds.</param>
    /// <param name="enableWatchdogs">Enable background crashed, interrupted and postponed flow scheduling. Default true.</param>
    /// <param name="watchdogCheckFrequency">Check frequency for eligible crashed, interrupted and postponed flows. Default 1 second.</param>
    /// <param name="messagesPullFrequency">Pull frequency for active/max-waiting messages. Default: 250ms</param>
    /// <param name="messagesDefaultMaxWaitForCompletion">Default wait duration before suspension for messages. Defaults to none.</param>
    /// <param name="delayStartup">Delay watchdog start-up. Defaults to none.</param>
    /// <param name="maxParallelRetryInvocations">Limit the number of watchdog started invocations. Default: 100.</param>
    /// <param name="serializer">Specify custom serializer. Default built-in json-serializer.</param>
    /// <param name="utcNow">Provide custom delegate for providing current utc datetime. Default: () => DateTime.UtcNow</param>
    public Options(
        Action<FrameworkException>? unhandledExceptionHandler = null,
        TimeSpan? retentionPeriod = null,
        TimeSpan? retentionCleanUpFrequency = null,
        TimeSpan? leaseLength = null,
        bool? enableWatchdogs = null,
        TimeSpan? watchdogCheckFrequency = null,
        TimeSpan? messagesPullFrequency = null,
        TimeSpan? messagesDefaultMaxWaitForCompletion = null,
        TimeSpan? delayStartup = null,
        int? maxParallelRetryInvocations = null,
        ISerializer? serializer = null,
        UtcNow? utcNow = null
    )
    {
        UnhandledExceptionHandler = unhandledExceptionHandler;
        WatchdogCheckFrequency = watchdogCheckFrequency;
        LeaseLength = leaseLength;
        RetentionPeriod = retentionPeriod;
        RetentionCleanUpFrequency = retentionCleanUpFrequency;
        EnableWatchdogs = enableWatchdogs;
        MessagesPullFrequency = messagesPullFrequency;
        MessagesDefaultMaxWaitForCompletion = messagesDefaultMaxWaitForCompletion;
        DelayStartup = delayStartup;
        MaxParallelRetryInvocations = maxParallelRetryInvocations;
        Serializer = serializer;
        UtcNow = utcNow;
    }

    public Options Merge(Options options)
    {
        return new Options(
            UnhandledExceptionHandler ?? options.UnhandledExceptionHandler,
            RetentionPeriod ?? options.RetentionPeriod,
            RetentionCleanUpFrequency ?? options.RetentionCleanUpFrequency,
            LeaseLength ?? options.LeaseLength,
            EnableWatchdogs ?? options.EnableWatchdogs,
            WatchdogCheckFrequency ?? options.WatchdogCheckFrequency,
            MessagesPullFrequency ?? options.MessagesPullFrequency,
            MessagesDefaultMaxWaitForCompletion ?? options.MessagesDefaultMaxWaitForCompletion,
            DelayStartup ?? options.DelayStartup,
            MaxParallelRetryInvocations ?? options.MaxParallelRetryInvocations,
            Serializer ?? options.Serializer
        );
    }

    internal Settings MapToSettings()
        => new(
            UnhandledExceptionHandler,
            RetentionPeriod,
            RetentionCleanUpFrequency,
            LeaseLength,
            EnableWatchdogs,
            WatchdogCheckFrequency,
            MessagesPullFrequency,
            MessagesDefaultMaxWaitForCompletion,
            DelayStartup,
            MaxParallelRetryInvocations,
            Serializer,
            UtcNow
        );
}
