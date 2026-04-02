using System;

namespace GodotTask;

public static partial class TaskTracker
{
    internal record TrackingData(string FormattedType, int TrackingId, DateTime AddTime, string StackTrace, Func<GDTaskStatus> StatusProvider);

    internal class ObservableProperty(bool value) : IObservable<bool>
    {
        private IObserver<bool> _singleSubscriber;

        private bool _value = value;

        public bool Value
        {
            get => _value;
            set
            {
                _value = value;

                if (_singleSubscriber is null) return;

                _singleSubscriber.OnNext(_value);
                _singleSubscriber.OnCompleted();
            }
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            _singleSubscriber = observer;
            observer.OnNext(_value);
            return new DisposeHandle(this);
        }

        public static implicit operator bool(ObservableProperty observableProperty) => observableProperty._value;

        internal class DisposeHandle(ObservableProperty property) : IDisposable
        {
            public void Dispose() => property._singleSubscriber = null;
        }
    }
}