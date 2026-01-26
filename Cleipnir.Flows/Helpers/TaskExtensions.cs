using System;
using System.Threading.Tasks;

namespace Cleipnir.Flows.Helpers;

internal static class TaskExtensions
{
    public static async Task<T> ThrowTimeoutExceptionOnNoResult<T>(this Task<T?> task) where T : class
    {
        var result = await task;
        if (result is not null)
            return result;

        throw new TimeoutException();
    }
}