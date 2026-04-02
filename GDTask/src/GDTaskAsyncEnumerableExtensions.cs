using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GodotTask;

/// <summary>
/// Provides extensions methods for <see cref="IGDTaskAsyncEnumerable{T}" /> and <see cref="IAsyncEnumerable{T}" />.
/// </summary>
public static class GDTaskAsyncEnumerableExtensions
{
    /// <summary>
    /// Converts the <see cref="IAsyncEnumerable{T}" /> to an <see cref="IGDTaskAsyncEnumerable{T}" />.
    /// </summary>
    public static IGDTaskAsyncEnumerable<T> AsGDTaskAsyncEnumerable<T>(this IAsyncEnumerable<T> source) => new AsyncEnumerableToGDTaskAsyncEnumerable<T>(source);

    /// <summary>
    /// Converts the <see cref="IGDTaskAsyncEnumerable{T}" /> to an <see cref="IAsyncEnumerable{T}" />.
    /// </summary>
    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IGDTaskAsyncEnumerable<T> source) => new GDTaskAsyncEnumerableToAsyncEnumerable<T>(source);

    private sealed class AsyncEnumerableToGDTaskAsyncEnumerable<T>(IAsyncEnumerable<T> source) : IGDTaskAsyncEnumerable<T>
    {
        public IGDTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new Enumerator(source.GetAsyncEnumerator(cancellationToken));

        private sealed class Enumerator(IAsyncEnumerator<T> enumerator) : IGDTaskAsyncEnumerator<T>
        {
            public T Current => enumerator.Current;

            public async GDTask DisposeAsync() => await enumerator.DisposeAsync();

            public async GDTask<bool> MoveNextAsync() => await enumerator.MoveNextAsync();
        }
    }

    private sealed class GDTaskAsyncEnumerableToAsyncEnumerable<T>(IGDTaskAsyncEnumerable<T> source) : IAsyncEnumerable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new Enumerator(source.GetAsyncEnumerator(cancellationToken));

        private sealed class Enumerator(IGDTaskAsyncEnumerator<T> enumerator) : IAsyncEnumerator<T>
        {
            public T Current => enumerator.Current;

            public ValueTask DisposeAsync() => enumerator.DisposeAsync().AsValueTask();

            public ValueTask<bool> MoveNextAsync() => enumerator.MoveNextAsync().AsValueTask();
        }
    }
}