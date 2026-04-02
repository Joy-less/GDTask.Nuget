using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GdUnit4;
using Godot;
using GodotTask.Internal;

namespace GodotTask.Tests;

[TestSuite]
public class GDTaskTest_WhenAll_WhenAny_WhenEach
{
    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAll_Params()
    {
        await Constants.WaitForTaskReadyAsync();
        await GDTask.WhenAll(new[] { GDTask.CompletedTask, Constants.Delay() });
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAllT_Params()
    {
        await Constants.WaitForTaskReadyAsync();
        var results = await GDTask.WhenAll(new[] { Constants.DelayWithReturn(), GDTask.FromResult(Constants.ReturnValue) });
        Assertions.AssertThat(results.All(value => value == Constants.ReturnValue));
    }
    
    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAll_ParamsSpan()
    {
        await Constants.WaitForTaskReadyAsync();
        await GDTask.WhenAll((ReadOnlySpan<GDTask>) [GDTask.CompletedTask, Constants.Delay()]);
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAllT_ParamsSpan()
    {
        await Constants.WaitForTaskReadyAsync();
        var results = await GDTask.WhenAll((ReadOnlySpan<GDTask<int>>) [Constants.DelayWithReturn(), GDTask.FromResult(Constants.ReturnValue)]);
        Assertions.AssertThat(results.All(value => value == Constants.ReturnValue));
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAll_Enumerable()
    {
        await Constants.WaitForTaskReadyAsync();
        await GDTask.WhenAll((IEnumerable<GDTask>)new[] { GDTask.CompletedTask, Constants.Delay() });
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAllT_Enumerable()
    {
        await Constants.WaitForTaskReadyAsync();
        var results = await GDTask.WhenAll((IEnumerable<GDTask<int>>)new[] { Constants.DelayWithReturn(), GDTask.FromResult(Constants.ReturnValue) });
        Assertions.AssertThat(results.All(value => value == Constants.ReturnValue));
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAllT_Tuple()
    {
        await Constants.WaitForTaskReadyAsync();
        var (resultA, resultB) = await GDTask.WhenAll(Constants.DelayWithReturn(), GDTask.FromResult(Constants.ReturnValue));
        Assertions.AssertThat(resultA).IsEqual(Constants.ReturnValue);
        Assertions.AssertThat(resultB).IsEqual(Constants.ReturnValue);
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAll_EmptyInputs_CompleteImmediately()
    {
        await Constants.WaitForTaskReadyAsync();

        await GDTask.WhenAll(Array.Empty<GDTask>());

        var genericResults = await GDTask.WhenAll(Array.Empty<GDTask<int>>());
        Assertions.AssertThat(genericResults).IsNotNull();
        Assertions.AssertThat(genericResults.Length).IsEqual(0);
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAllT_AllSuccess_PreservesInputOrder()
    {
        await Constants.WaitForTaskReadyAsync();

        var results = await GDTask.WhenAll(new[]
        {
            GDTask.DelayFrame(3).ContinueWith(() => 3),
            GDTask.DelayFrame(1).ContinueWith(() => 1),
            GDTask.DelayFrame(2).ContinueWith(() => 2),
        });

        Assertions.AssertThat(results).ContainsExactly(new[] { 3, 1, 2 });
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAll_WaitsForRemainingTasksAfterFault()
    {
        await Constants.WaitForTaskReadyAsync();

        var faulted = new GDTaskCompletionSource();
        var delayed = new GDTaskCompletionSource();
        var combinedTask = GDTask.WhenAll(faulted.Task, delayed.Task);

        faulted.TrySetException(new ExpectedException());
        await GDTask.DelayFrame(1);

        Assertions.AssertThat(combinedTask.Status).IsEqual(GDTaskStatus.Pending);

        delayed.TrySetResult();

        try
        {
            await combinedTask;
            throw new TestFailedException("ExpectedException not thrown");
        }
        catch (ExpectedException)
        {
        }
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAllT_WaitsForRemainingTasksAfterFault()
    {
        await Constants.WaitForTaskReadyAsync();

        var faulted = new GDTaskCompletionSource<int>();
        var delayed = new GDTaskCompletionSource<int>();
        var combinedTask = GDTask.WhenAll(faulted.Task, delayed.Task);

        faulted.TrySetException(new ExpectedException());
        await GDTask.DelayFrame(1);

        Assertions.AssertThat(combinedTask.Status).IsEqual(GDTaskStatus.Pending);

        delayed.TrySetResult(42);

        try
        {
            await combinedTask;
            throw new TestFailedException("ExpectedException not thrown");
        }
        catch (ExpectedException)
        {
        }
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAll_SingleFault_PropagatesAfterAllComplete()
    {
        await Constants.WaitForTaskReadyAsync();

        var delayed = new GDTaskCompletionSource();
        var combinedTask = GDTask.WhenAll(GDTask.FromException(new ExpectedException()), delayed.Task);

        await GDTask.DelayFrame(1);
        Assertions.AssertThat(combinedTask.Status).IsEqual(GDTaskStatus.Pending);

        delayed.TrySetResult();

        try
        {
            await combinedTask;
            throw new TestFailedException("ExpectedException not thrown");
        }
        catch (ExpectedException)
        {
        }
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAll_MultipleFaults_AggregatesExceptions()
    {
        await Constants.WaitForTaskReadyAsync();

        var delayed = new GDTaskCompletionSource();
        var exceptionA = new ExpectedException();
        var exceptionB = new InvalidOperationException("boom");
        var combinedTask = GDTask.WhenAll(GDTask.FromException(exceptionA), delayed.Task, GDTask.FromException(exceptionB));

        await GDTask.DelayFrame(1);
        Assertions.AssertThat(combinedTask.Status).IsEqual(GDTaskStatus.Pending);

        delayed.TrySetResult();

        try
        {
            await combinedTask;
            throw new TestFailedException("AggregateException not thrown");
        }
        catch (AggregateException ex)
        {
            Assertions.AssertThat(ex.InnerExceptions.Count).IsEqual(2);
            Assertions.AssertThat(ex.InnerExceptions[0]).IsSame(exceptionA);
            Assertions.AssertThat(ex.InnerExceptions[1]).IsSame(exceptionB);
        }
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAll_Cancellation_WaitsForAllAndCancels()
    {
        await Constants.WaitForTaskReadyAsync();

        using var cancellationTokenSource = new CancellationTokenSource();
        var delayed = new GDTaskCompletionSource();
        var combinedTask = GDTask.WhenAll(GDTask.FromCanceled(cancellationTokenSource.Token), delayed.Task);

        await GDTask.DelayFrame(1);
        Assertions.AssertThat(combinedTask.Status).IsEqual(GDTaskStatus.Pending);

        delayed.TrySetResult();

        try
        {
            await combinedTask;
            throw new TestFailedException("OperationCanceledException not thrown");
        }
        catch (OperationCanceledException ex)
        {
            Assertions.AssertThat(ex.CancellationToken).IsEqual(cancellationTokenSource.Token);
        }
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAll_FaultTakesPrecedenceOverCancellation()
    {
        await Constants.WaitForTaskReadyAsync();

        using var cancellationTokenSource = new CancellationTokenSource();
        var delayed = new GDTaskCompletionSource();
        var combinedTask = GDTask.WhenAll(
            GDTask.FromCanceled(cancellationTokenSource.Token),
            GDTask.FromException(new ExpectedException()),
            delayed.Task);

        await GDTask.DelayFrame(1);
        Assertions.AssertThat(combinedTask.Status).IsEqual(GDTaskStatus.Pending);

        delayed.TrySetResult();

        try
        {
            await combinedTask;
            throw new TestFailedException("ExpectedException not thrown");
        }
        catch (ExpectedException)
        {
        }
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAllT_TupleFault_WaitsForAllAndPropagates()
    {
        await Constants.WaitForTaskReadyAsync();

        var faulted = new GDTaskCompletionSource<int>();
        var delayed = new GDTaskCompletionSource<int>();
        var combinedTask = GDTask.WhenAll(faulted.Task, delayed.Task);

        faulted.TrySetException(new ExpectedException());
        await GDTask.DelayFrame(1);

        Assertions.AssertThat(combinedTask.Status).IsEqual(GDTaskStatus.Pending);

        delayed.TrySetResult(7);

        try
        {
            await combinedTask;
            throw new TestFailedException("ExpectedException not thrown");
        }
        catch (ExpectedException)
        {
        }
    }
    
    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAny_Params()
    {
        await Constants.WaitForTaskReadyAsync();
        var winArgumentIndex = await GDTask.WhenAny(new[] { GDTask.Never(CancellationToken.None), Constants.Delay() });
        Assertions.AssertThat(winArgumentIndex).IsEqual(1);
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAnyT_Params()
    {
        await Constants.WaitForTaskReadyAsync();
        var (winArgumentIndex, result) = await GDTask.WhenAny(new[] { Constants.DelayWithReturn(), GDTask.Never<int>(CancellationToken.None) });
        Assertions.AssertThat(winArgumentIndex).IsEqual(0);
        Assertions.AssertThat(result).IsEqual(Constants.ReturnValue);
    }
    
    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAny_ParamsSpan()
    {
        await Constants.WaitForTaskReadyAsync();
        var winArgumentIndex = await GDTask.WhenAny((ReadOnlySpan<GDTask>) [GDTask.Never(CancellationToken.None), Constants.Delay()]);
        Assertions.AssertThat(winArgumentIndex).IsEqual(1);
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAnyT_ParamsSpan()
    {
        await Constants.WaitForTaskReadyAsync();
        var (winArgumentIndex, result) = await GDTask.WhenAny((ReadOnlySpan<GDTask<int>>) [Constants.DelayWithReturn(), GDTask.Never<int>(CancellationToken.None)]);
        Assertions.AssertThat(winArgumentIndex).IsEqual(0);
        Assertions.AssertThat(result).IsEqual(Constants.ReturnValue);
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAny_Enumerable()
    {
        await Constants.WaitForTaskReadyAsync();
        var winArgumentIndex = await GDTask.WhenAny((IEnumerable<GDTask>)new[] { GDTask.Never(CancellationToken.None), Constants.Delay() });
        Assertions.AssertThat(winArgumentIndex).IsEqual(1);
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAnyT_Enumerable()
    {
        await Constants.WaitForTaskReadyAsync();
        var (winArgumentIndex, result) = await GDTask.WhenAny((IEnumerable<GDTask<int>>)new[] { Constants.DelayWithReturn(), GDTask.Never<int>(CancellationToken.None) });
        Assertions.AssertThat(winArgumentIndex).IsEqual(0);
        Assertions.AssertThat(result).IsEqual(Constants.ReturnValue);
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenAnyT_Tuple()
    {
        await Constants.WaitForTaskReadyAsync();
        var (winArgumentIndex, result1, _) = await GDTask.WhenAny(Constants.DelayWithReturn(), GDTask.Never<int>(CancellationToken.None));
        Assertions.AssertThat(winArgumentIndex).IsEqual(0);
        Assertions.AssertThat(result1).IsEqual(Constants.ReturnValue);
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenEach_Enumerable()
    {
        await Constants.WaitForTaskReadyAsync();
        int counter = 0;
        ulong frameCount = Engine.GetProcessFrames();
        await foreach (GDTask task in GDTask.WhenEach((IEnumerable<GDTask>)new[] {
            GDTask.DelayFrame(5),
            GDTask.DelayFrame(15),
            GDTask.DelayFrame(10),
        }))
        {
            counter++;
        }
        Assertions.AssertThat(counter).IsEqual(3);
        Assertions.AssertThat(frameCount).IsEqual(Engine.GetProcessFrames() - 15);
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenEachT_Enumerable()
    {
        await Constants.WaitForTaskReadyAsync();
        int counter = 0;
        ulong frameCount = Engine.GetProcessFrames();
        List<int> results = [];
        await foreach (GDTask<int> task in GDTask.WhenEach((IEnumerable<GDTask<int>>)new[] {
            GDTask.DelayFrame(5).ContinueWith(() => 1),
            GDTask.DelayFrame(15).ContinueWith(() => 3),
            GDTask.DelayFrame(10).ContinueWith(() => 2),
        }))
        {
            counter++;
            results.Add(task.GetAwaiter().GetResult());
        }
        Assertions.AssertThat(counter).IsEqual(3);
        Assertions.AssertThat(frameCount).IsEqual(Engine.GetProcessFrames() - 15);
        Assertions.AssertThat(string.Join(", ", results)).IsEqual("1, 2, 3");
    }

    [TestCase, RequireGodotRuntime]
    public static async Task Channel_ReadAllAsync_Current_IsStableAcrossRepeatedAccess()
    {
        await Constants.WaitForTaskReadyAsync();
        var channel = Channel.CreateSingleConsumerUnbounded<int>();
        channel.Writer.TryWrite(1);
        channel.Writer.TryWrite(2);
        channel.Writer.TryComplete();

        var enumerator = channel.Reader.ReadAllAsync().GetAsyncEnumerator();

        try
        {
            Assertions.AssertThat(await enumerator.MoveNextAsync()).IsTrue();
            Assertions.AssertThat(enumerator.Current).IsEqual(1);
            Assertions.AssertThat(enumerator.Current).IsEqual(1);

            Assertions.AssertThat(await enumerator.MoveNextAsync()).IsTrue();
            Assertions.AssertThat(enumerator.Current).IsEqual(2);
            Assertions.AssertThat(enumerator.Current).IsEqual(2);

            Assertions.AssertThat(await enumerator.MoveNextAsync()).IsFalse();
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenEachT_RepeatedCurrentAccess_DoesNotLoseItems()
    {
        await Constants.WaitForTaskReadyAsync();
        var enumerator = GDTask.WhenEach((IEnumerable<GDTask<int>>)new[]
        {
            GDTask.FromResult(1),
            GDTask.FromResult(2),
            GDTask.FromResult(3),
        }).GetAsyncEnumerator();

        var results = new List<int>();

        try
        {
            while (await enumerator.MoveNextAsync())
            {
                var firstRead = enumerator.Current;
                var secondRead = enumerator.Current;

                Assertions.AssertThat(secondRead.GetAwaiter().GetResult())
                    .IsEqual(firstRead.GetAwaiter().GetResult());

                results.Add(secondRead.GetAwaiter().GetResult());
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        Assertions.AssertThat(results.Count).IsEqual(3);
        Assertions.AssertThat(results.OrderBy(value => value).ToArray())
            .ContainsExactly(new[] { 1, 2, 3 });
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenEach_EmptyEnumerable_CompletesImmediately()
    {
        await Constants.WaitForTaskReadyAsync();
        var enumerator = GDTask.WhenEach((IEnumerable<GDTask>)Array.Empty<GDTask>()).GetAsyncEnumerator();

        try
        {
            var (isTimeout, hasItem) = await enumerator.MoveNextAsync().TimeoutWithoutException(Constants.DelayTimeSpan);
            Assertions.AssertThat(isTimeout).IsFalse();
            Assertions.AssertThat(hasItem).IsFalse();
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenEachT_EmptyEnumerable_CompletesImmediately()
    {
        await Constants.WaitForTaskReadyAsync();
        var enumerator = GDTask.WhenEach((IEnumerable<GDTask<int>>)Array.Empty<GDTask<int>>()).GetAsyncEnumerator();

        try
        {
            var (isTimeout, hasItem) = await enumerator.MoveNextAsync().TimeoutWithoutException(Constants.DelayTimeSpan);
            Assertions.AssertThat(isTimeout).IsFalse();
            Assertions.AssertThat(hasItem).IsFalse();
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenEach_DisposeBeforeMoveNext_DoesNotThrow()
    {
        await Constants.WaitForTaskReadyAsync();
        var enumerator = GDTask.WhenEach((IEnumerable<GDTask>)Array.Empty<GDTask>()).GetAsyncEnumerator();
        await enumerator.DisposeAsync();
    }

    [TestCase, RequireGodotRuntime]
    public static async Task GDTask_WhenEachT_DisposeBeforeMoveNext_DoesNotThrow()
    {
        await Constants.WaitForTaskReadyAsync();
        var enumerator = GDTask.WhenEach((IEnumerable<GDTask<int>>)Array.Empty<GDTask<int>>()).GetAsyncEnumerator();
        await enumerator.DisposeAsync();
    }
}