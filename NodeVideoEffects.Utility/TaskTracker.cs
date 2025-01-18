namespace NodeVideoEffects.Utility;

/// <summary>
/// A utility class for tracking the number of running asynchronous tasks and raising events when the task count changes.
/// </summary>
public static class TaskTracker
{
    private static int _runningTaskCount;

    /// <summary>
    /// Event that is raised whenever the number of running tasks changes.
    /// The event argument provides the current count of running tasks.
    /// </summary>
    public static event EventHandler<int>? TaskCountChanged;

    /// <summary>
    /// Gets the current number of running tasks.
    /// </summary>
    public static int RunningTaskCount => _runningTaskCount;

    /// <summary>
    /// Executes a task while tracking it and updates the task count accordingly.
    /// This method is for tasks that do not return a value.
    /// </summary>
    /// <param name="taskFunc">The asynchronous function to be executed.</param>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    public static Task RunTrackedTask(Func<Task> taskFunc)
    {
        IncrementTaskCount();
        return Task.Run(async () =>
        {
            try
            {
                await taskFunc();
            }
            finally
            {
                DecrementTaskCount();
            }
        });
    }

    /// <summary>
    /// Executes a task while tracking it and updates the task count accordingly.
    /// This method is for tasks that return a value.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the task.</typeparam>
    /// <param name="taskFunc">The asynchronous function to be executed.</param>
    /// <returns>A Task that represents the asynchronous operation and contains the result.</returns>
    public static Task<T> RunTrackedTask<T>(Func<Task<T>> taskFunc)
    {
        IncrementTaskCount();
        return Task.Run(async () =>
        {
            try
            {
                return await taskFunc();
            }
            finally
            {
                DecrementTaskCount();
            }
        });
    }

    // Increments the running task count and triggers the TaskCountChanged event.
    private static void IncrementTaskCount()
    {
        var newCount = Interlocked.Increment(ref _runningTaskCount);
        TaskCountChanged?.Invoke(null, newCount);
    }

    // Decrements the running task count and triggers the TaskCountChanged event.
    private static void DecrementTaskCount()
    {
        var newCount = Interlocked.Decrement(ref _runningTaskCount);
        TaskCountChanged?.Invoke(null, newCount);
    }
}