using System;
using System.Threading.Tasks;
using Cleipnir.ResilientFunctions.CoreRuntime.Invocation;
using Cleipnir.ResilientFunctions.Domain;

namespace Cleipnir.Flows;

public abstract class BaseFlow
{
    public Workflow Workflow { get; init; } = null!;
    public Utilities Utilities => Workflow.Utilities;
    public Effect Effect => Workflow.Effect;

    #region Capture explicit id with ResiliencyLevel

    public Task<T> Capture<T>(string id, Func<Task<T>> work, ResiliencyLevel resiliencyLevel = ResiliencyLevel.AtLeastOnce) 
        => Effect.Capture(id, work, resiliencyLevel);
    public Task<T> Capture<T>(string id, Func<T> work, ResiliencyLevel resiliencyLevel = ResiliencyLevel.AtLeastOnce) 
        => Effect.Capture(id, work, resiliencyLevel);
    public Task Capture(string id, Func<Task> work, ResiliencyLevel resiliencyLevel = ResiliencyLevel.AtLeastOnce) 
        => Effect.Capture(id, work, resiliencyLevel);
    public Task Capture(string id, Action work, ResiliencyLevel resiliencyLevel = ResiliencyLevel.AtLeastOnce) 
        => Effect.Capture(id, work, resiliencyLevel); 

    #endregion

    #region Capture explicit id with RetryPolicy

    public Task<T> Capture<T>(string id, Func<Task<T>> work, RetryPolicy retryPolicy, bool flush = true) 
        => Effect.Capture(id, work, retryPolicy, flush);
    public Task<T> Capture<T>(string id, Func<T> work, RetryPolicy retryPolicy, bool flush = true) 
        => Effect.Capture(id, work, retryPolicy, flush);
    public Task Capture(string id, Func<Task> work, RetryPolicy retryPolicy, bool flush = true) 
        => Effect.Capture(id, work, retryPolicy, flush);
    public Task Capture(string id, Action work, RetryPolicy retryPolicy, bool flush = true) 
        => Effect.Capture(id, work, retryPolicy, flush);

    #endregion

    #region Capture implicit id with RetryPolicy

    public Task<T> Capture<T>(Func<Task<T>> work, RetryPolicy retryPolicy, bool flush = true) 
        => Effect.Capture(work, retryPolicy, flush);
    public Task<T> Capture<T>(Func<T> work, RetryPolicy retryPolicy, bool flush = true) 
        => Effect.Capture(work, retryPolicy, flush);
    public Task Capture(Func<Task> work, RetryPolicy retryPolicy, bool flush = true) 
        => Effect.Capture(work, retryPolicy, flush);
    public Task Capture(Action work, RetryPolicy retryPolicy, bool flush = true) 
        => Effect.Capture(work, retryPolicy, flush);

    #endregion
    
    #region Capture implicit id with ResiliencyLevel

    public Task<T> Capture<T>(Func<Task<T>> work, ResiliencyLevel resiliencyLevel = ResiliencyLevel.AtLeastOnce) 
        => Effect.Capture(work, resiliencyLevel);
    public Task<T> Capture<T>(Func<T> work, ResiliencyLevel resiliencyLevel = ResiliencyLevel.AtLeastOnce) 
        => Effect.Capture(work, resiliencyLevel);
    public Task Capture(Func<Task> work, ResiliencyLevel resiliencyLevel = ResiliencyLevel.AtLeastOnce) 
        => Effect.Capture(work, resiliencyLevel);
    public Task Capture(Action work, ResiliencyLevel resiliencyLevel = ResiliencyLevel.AtLeastOnce) 
        => Effect.Capture(work, resiliencyLevel);

    #endregion
    
    public Task<TMessage> Message<TMessage>(TimeSpan? maxWait = null) where TMessage : class
        => Workflow.Message<TMessage>(maxWait);
    public Task<TMessage?> Message<TMessage>(DateTime waitUntil, TimeSpan? maxWait = null) where TMessage : class
        => Workflow.Message<TMessage>(waitUntil, maxWait);
    public Task<TMessage?> Message<TMessage>(TimeSpan waitFor, TimeSpan? maxWait = null) where TMessage : class
        => Workflow.Message<TMessage>(waitFor, maxWait);
    public Task<TMessage> Message<TMessage>(Func<TMessage, bool> filter, TimeSpan? maxWait = null) where TMessage : class
        => Workflow.Message(filter, maxWait);
    public Task<TMessage?> Message<TMessage>(Func<TMessage, bool> filter, DateTime waitUntil, TimeSpan? maxWait = null) where TMessage : class
        => Workflow.Message(filter, waitUntil, maxWait);
    public Task<TMessage?> Message<TMessage>(Func<TMessage, bool> filter, TimeSpan waitFor, TimeSpan? maxWait = null) where TMessage : class
        => Workflow.Message(filter, waitFor, maxWait);

    public Task Delay(TimeSpan @for, bool suspend = true) => Workflow.Delay(@for, suspend);
    public Task Delay(DateTime until, bool suspend = true) => Workflow.Delay(until, suspend);
}

public abstract class Flow : BaseFlow
{
    public abstract Task Run();
}

public abstract class Flow<TParam> : BaseFlow where TParam : notnull 
{
    public abstract Task Run(TParam param);
}

public abstract class Flow<TParam, TResult> : BaseFlow where TParam : notnull
{
    public abstract Task<TResult> Run(TParam param);
}