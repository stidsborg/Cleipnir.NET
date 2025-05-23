﻿using System;
using System.Threading.Tasks;
using Cleipnir.ResilientFunctions.CoreRuntime.Invocation;
using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Messaging;
using Cleipnir.ResilientFunctions.Reactive.Extensions;
using Cleipnir.ResilientFunctions.Reactive.Utilities;

namespace Cleipnir.Flows;

public abstract class BaseFlow
{
    public Workflow Workflow { get; init; } = null!;
    public Utilities Utilities => Workflow.Utilities;
    public Messages Messages => Workflow.Messages;
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
    
    public Task<TMessage> Message<TMessage>() => Workflow.Messages.FirstOfType<TMessage>();
    public Task<Option<TMessage>> Message<TMessage>(string timeoutId, DateTime timesOutAt) => Workflow
            .Messages
            .TakeUntilTimeout(timeoutId, timesOutAt)
            .OfType<TMessage>()
            .FirstOrNone();
    public Task<Option<TMessage>> Message<TMessage>(string timeoutId, TimeSpan timesOutIn) => Workflow
            .Messages
            .TakeUntilTimeout(timeoutId, timesOutIn)
            .OfType<TMessage>()
            .FirstOrNone();
    public Task<Option<TMessage>> Message<TMessage>(DateTime timesOutAt) => Workflow
            .Messages
            .TakeUntilTimeout(timesOutAt)
            .OfType<TMessage>()
            .FirstOrNone();
    public Task<Option<TMessage>> Message<TMessage>(TimeSpan timesOutIn) => Workflow
            .Messages
            .TakeUntilTimeout(timesOutIn)
            .OfType<TMessage>()
            .FirstOrNone();
    
    public Task<Either<TMessage1, TMessage2>> Message<TMessage1, TMessage2>() 
        => Workflow.Messages.OfTypes<TMessage1, TMessage2>().First();
    public Task<EitherOrNone<TMessage1, TMessage2>> Message<TMessage1, TMessage2>(string timeoutId,
        DateTime timesOutAt) => Workflow
        .Messages
        .TakeUntilTimeout(timeoutId, timesOutAt)
        .OfTypes<TMessage1, TMessage2>()
        .FirstOrNone();
    public Task<EitherOrNone<TMessage1, TMessage2>> Message<TMessage1, TMessage2>(string timeoutId, TimeSpan timesOutIn) => Workflow
        .Messages
        .TakeUntilTimeout(timeoutId, timesOutIn)
        .OfTypes<TMessage1, TMessage2>()
        .FirstOrNone();
    public Task<EitherOrNone<TMessage1, TMessage2>> Message<TMessage1, TMessage2>(DateTime timesOutAt) => Workflow
        .Messages
        .TakeUntilTimeout(timesOutAt)
        .OfTypes<TMessage1, TMessage2>()
        .FirstOrNone();
    public Task<EitherOrNone<TMessage1, TMessage2>> Message<TMessage1, TMessage2>(TimeSpan timesOutIn) => Workflow
        .Messages
        .TakeUntilTimeout(timesOutIn)
        .OfTypes<TMessage1, TMessage2>()
        .FirstOrNone();
    public Task<Either<TMessage1, TMessage2, TMessage3>> Message<TMessage1, TMessage2, TMessage3>() 
        => Workflow.Messages.OfTypes<TMessage1, TMessage2, TMessage3>().First();
    public Task<EitherOrNone<TMessage1, TMessage2, TMessage3>> Message<TMessage1, TMessage2, TMessage3>(string timeoutId, DateTime timesOutAt) => Workflow
        .Messages
        .TakeUntilTimeout(timeoutId, timesOutAt)
        .OfTypes<TMessage1, TMessage2, TMessage3>()
        .FirstOrNone();
    public Task<EitherOrNone<TMessage1, TMessage2, TMessage3>> Message<TMessage1, TMessage2, TMessage3>(string timeoutId, TimeSpan timesOutIn) => Workflow
        .Messages
        .TakeUntilTimeout(timeoutId, timesOutIn)
        .OfTypes<TMessage1, TMessage2, TMessage3>()
        .FirstOrNone();
    public Task<EitherOrNone<TMessage1, TMessage2, TMessage3>> Message<TMessage1, TMessage2, TMessage3>(DateTime timesOutAt) => Workflow
        .Messages
        .TakeUntilTimeout(timesOutAt)
        .OfTypes<TMessage1, TMessage2, TMessage3>()
        .FirstOrNone();
    public Task<EitherOrNone<TMessage1, TMessage2, TMessage3>> Message<TMessage1, TMessage2, TMessage3>(TimeSpan timesOutIn) => Workflow
        .Messages
        .TakeUntilTimeout(timesOutIn)
        .OfTypes<TMessage1, TMessage2, TMessage3>()
        .FirstOrNone();

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