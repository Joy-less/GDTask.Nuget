using System;
using System.Threading;
using System.Threading.Tasks;
using GdUnit4;

namespace GodotTask.Tests;

[TestSuite]
public class GDTaskTest_CompletionSource
{
    private const int ConcurrentCompletionIterations = 200;
    private const int ConcurrentCompletionWriters = 12;

    [TestCase, RequireGodotRuntime]
    public static void CompletionSource_Constructor()
    {
        var source = new GDTaskCompletionSource();
        Assertions.AssertThat(source).IsNotNull();
        Assertions.AssertThat(source.Task).IsNotNull();
        Assertions.AssertThat(source.Task.Status == GDTaskStatus.Pending).IsTrue();
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSource_TrySetResult()
    {
        var source = new GDTaskCompletionSource();
        source.TrySetResult();
        Assertions.AssertThat(source.Task.Status == GDTaskStatus.Succeeded).IsTrue();
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSource_TrySetException()
    {
        var source = new GDTaskCompletionSource();
        source.TrySetException(new ExpectedException());
        Assertions.AssertThat(source.Task.Status == GDTaskStatus.Faulted).IsTrue();

        try
        {
            source.GetResult(0);
        }
        catch (ExpectedException)
        {
            return;
        }

        throw new TestFailedException("ExpectedException not thrown");
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSource_TrySetCanceled()
    {
        var source = new GDTaskCompletionSource();
        source.TrySetCanceled();
        Assertions.AssertThat(source.Task.Status == GDTaskStatus.Canceled).IsTrue();

        try
        {
            source.GetResult(0);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        throw new TestFailedException("OperationCanceledException not thrown");
    }

    [TestCase, RequireGodotRuntime]
    public static async Task CompletionSource_Async()
    {
        await Constants.WaitForTaskReadyAsync();
        var source = new GDTaskCompletionSource();
        Constants.Delay().ContinueWith(() => source.TrySetResult()).Forget();

        // Await the task
        await source.Task;
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSourceT_Constructor()
    {
        var source = new GDTaskCompletionSource<int>();
        Assertions.AssertThat(source).IsNotNull();
        Assertions.AssertThat(source.Task).IsNotNull();
        Assertions.AssertThat(source.Task.Status == GDTaskStatus.Pending).IsTrue();
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSourceT_TrySetResult()
    {
        var source = new GDTaskCompletionSource<int>();
        source.TrySetResult(Constants.ReturnValue);
        Assertions.AssertThat(source.Task.Status == GDTaskStatus.Succeeded).IsTrue();
        Assertions.AssertThat(source.GetResult(0)).IsEqual(Constants.ReturnValue);
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSourceT_TrySetException()
    {
        var source = new GDTaskCompletionSource<int>();
        source.TrySetException(new ExpectedException());
        Assertions.AssertThat(source.Task.Status == GDTaskStatus.Faulted).IsTrue();

        try
        {
            source.GetResult(0);
        }
        catch (ExpectedException)
        {
            return;
        }

        throw new TestFailedException("ExpectedException not thrown");
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSourceT_TrySetCanceled()
    {
        var source = new GDTaskCompletionSource<int>();
        source.TrySetCanceled();
        Assertions.AssertThat(source.Task.Status == GDTaskStatus.Canceled).IsTrue();

        try
        {
            source.GetResult(0);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        throw new TestFailedException("OperationCanceledException not thrown");
    }

    [TestCase, RequireGodotRuntime]
    public static async Task CompletionSourceT_Async()
    {
        await Constants.WaitForTaskReadyAsync();
        var source = new GDTaskCompletionSource<int>();
        Constants.Delay().ContinueWith(() => source.TrySetResult(Constants.ReturnValue)).Forget();

        // Await the task
        var result = await source.Task;
        Assertions.AssertThat(result).IsEqual(Constants.ReturnValue);
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSource_ConcurrentTrySetResult_ObservedWinnerOnly()
    {
        for (var iteration = 0; iteration < ConcurrentCompletionIterations; iteration++)
        {
            var source = new GDTaskCompletionSource();
            var winner = RunConcurrentCompletion(index =>
                index == 0
                    ? source.TrySetResult()
                    : source.TrySetException(new ConcurrentCompletionException(index)));

            if (winner == 0)
            {
                Assertions.AssertThat(source.Task.Status == GDTaskStatus.Succeeded).IsTrue();
                source.GetResult(0);
                continue;
            }

            Assertions.AssertThat(source.Task.Status == GDTaskStatus.Faulted).IsTrue();

            try
            {
                source.GetResult(0);
            }
            catch (ConcurrentCompletionException exception)
            {
                Assertions.AssertThat(exception.Id).IsEqual(winner);
                continue;
            }

            throw new TestFailedException("Expected ConcurrentCompletionException was not thrown.");
        }
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSource_ConcurrentTrySetException_ObservedWinnerOnly()
    {
        for (var iteration = 0; iteration < ConcurrentCompletionIterations; iteration++)
        {
            var source = new GDTaskCompletionSource();
            var winner = RunConcurrentCompletion(index => source.TrySetException(new ConcurrentCompletionException(index)));

            Assertions.AssertThat(source.Task.Status == GDTaskStatus.Faulted).IsTrue();

            try
            {
                source.GetResult(0);
            }
            catch (ConcurrentCompletionException exception)
            {
                Assertions.AssertThat(exception.Id).IsEqual(winner);
                continue;
            }

            throw new TestFailedException("Expected ConcurrentCompletionException was not thrown.");
        }
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSource_ConcurrentTrySetCanceled_ObservedWinnerOnly()
    {
        for (var iteration = 0; iteration < ConcurrentCompletionIterations; iteration++)
        {
            var source = new GDTaskCompletionSource();
            var tokenSources = CreateCancellationTokenSources();

            try
            {
                var winner = RunConcurrentCompletion(index => source.TrySetCanceled(tokenSources[index].Token));

                Assertions.AssertThat(source.Task.Status == GDTaskStatus.Canceled).IsTrue();

                try
                {
                    source.GetResult(0);
                }
                catch (OperationCanceledException exception)
                {
                    Assertions.AssertThat(exception.CancellationToken.Equals(tokenSources[winner].Token)).IsTrue();
                    continue;
                }

                throw new TestFailedException("Expected OperationCanceledException was not thrown.");
            }
            finally
            {
                DisposeCancellationTokenSources(tokenSources);
            }
        }
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSourceT_ConcurrentTrySetResult_ObservedWinnerOnly()
    {
        for (var iteration = 0; iteration < ConcurrentCompletionIterations; iteration++)
        {
            var source = new GDTaskCompletionSource<int>();
            var winner = RunConcurrentCompletion(index => source.TrySetResult(index));

            Assertions.AssertThat(source.Task.Status == GDTaskStatus.Succeeded).IsTrue();
            Assertions.AssertThat(source.GetResult(0)).IsEqual(winner);
        }
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSourceT_ConcurrentTrySetException_ObservedWinnerOnly()
    {
        for (var iteration = 0; iteration < ConcurrentCompletionIterations; iteration++)
        {
            var source = new GDTaskCompletionSource<int>();
            var winner = RunConcurrentCompletion(index => source.TrySetException(new ConcurrentCompletionException(index)));

            Assertions.AssertThat(source.Task.Status == GDTaskStatus.Faulted).IsTrue();

            try
            {
                source.GetResult(0);
            }
            catch (ConcurrentCompletionException exception)
            {
                Assertions.AssertThat(exception.Id).IsEqual(winner);
                continue;
            }

            throw new TestFailedException("Expected ConcurrentCompletionException was not thrown.");
        }
    }

    [TestCase, RequireGodotRuntime]
    public static void CompletionSourceT_ConcurrentTrySetCanceled_ObservedWinnerOnly()
    {
        for (var iteration = 0; iteration < ConcurrentCompletionIterations; iteration++)
        {
            var source = new GDTaskCompletionSource<int>();
            var tokenSources = CreateCancellationTokenSources();

            try
            {
                var winner = RunConcurrentCompletion(index => source.TrySetCanceled(tokenSources[index].Token));

                Assertions.AssertThat(source.Task.Status == GDTaskStatus.Canceled).IsTrue();

                try
                {
                    source.GetResult(0);
                }
                catch (OperationCanceledException exception)
                {
                    Assertions.AssertThat(exception.CancellationToken.Equals(tokenSources[winner].Token)).IsTrue();
                    continue;
                }

                throw new TestFailedException("Expected OperationCanceledException was not thrown.");
            }
            finally
            {
                DisposeCancellationTokenSources(tokenSources);
            }
        }
    }

    private static int RunConcurrentCompletion(Func<int, bool> tryComplete)
    {
        var barrier = new Barrier(ConcurrentCompletionWriters + 1);
        var threads = new Thread[ConcurrentCompletionWriters];
        var winners = new bool[ConcurrentCompletionWriters];

        for (var index = 0; index < ConcurrentCompletionWriters; index++)
        {
            var capturedIndex = index;
            threads[index] = new Thread(() =>
            {
                barrier.SignalAndWait();
                winners[capturedIndex] = tryComplete(capturedIndex);
            });
            threads[index].Start();
        }

        barrier.SignalAndWait();

        foreach (var thread in threads)
            thread.Join();

        var winner = -1;
        for (var index = 0; index < winners.Length; index++)
        {
            if (!winners[index]) continue;

            if (winner != -1)
                throw new TestFailedException("More than one completion attempt reported success.");

            winner = index;
        }

        if (winner == -1)
            throw new TestFailedException("No completion attempt reported success.");

        return winner;
    }

    private static CancellationTokenSource[] CreateCancellationTokenSources()
    {
        var sources = new CancellationTokenSource[ConcurrentCompletionWriters];
        for (var index = 0; index < sources.Length; index++)
            sources[index] = new CancellationTokenSource();

        return sources;
    }

    private static void DisposeCancellationTokenSources(CancellationTokenSource[] sources)
    {
        foreach (var source in sources)
            source.Dispose();
    }

    private sealed class ConcurrentCompletionException(int id) : Exception($"Concurrent completion {id}")
    {
        public int Id { get; } = id;
    }
}