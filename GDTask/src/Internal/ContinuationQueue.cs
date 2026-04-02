using System;
using System.Threading;
using Godot;

namespace GodotTask.Internal;

sealed class ContinuationQueue
{
    private const int MaxArrayLength = 0X7FEFFFFF;
    private const int InitialSize = 16;
    private Action[] _actionList = new Action[InitialSize];

    private int _actionListCount;
    private bool _dequeuing;

    private SpinLock _gate = new(false);
    private Action[] _waitingList = new Action[InitialSize];

    private int _waitingListCount;

    public void Enqueue(Action continuation)
    {
        var lockTaken = false;

        try
        {
            _gate.Enter(ref lockTaken);

            if (_dequeuing)
            {
                // Ensure Capacity
                if (_waitingList.Length == _waitingListCount)
                {
                    var newLength = _waitingListCount * 2;
                    if ((uint)newLength > MaxArrayLength) newLength = MaxArrayLength;

                    var newArray = new Action[newLength];
                    Array.Copy(_waitingList, newArray, _waitingListCount);
                    _waitingList = newArray;
                }

                _waitingList[_waitingListCount] = continuation;
                _waitingListCount++;
            }
            else
            {
                // Ensure Capacity
                if (_actionList.Length == _actionListCount)
                {
                    var newLength = _actionListCount * 2;
                    if ((uint)newLength > MaxArrayLength) newLength = MaxArrayLength;

                    var newArray = new Action[newLength];
                    Array.Copy(_actionList, newArray, _actionListCount);
                    _actionList = newArray;
                }

                _actionList[_actionListCount] = continuation;
                _actionListCount++;
            }
        }
        finally
        {
            if (lockTaken) _gate.Exit(false);
        }
    }

    public int Clear()
    {
        var rest = _actionListCount + _waitingListCount;

        _actionListCount = 0;
        _actionList = new Action[InitialSize];

        _waitingListCount = 0;
        _waitingList = new Action[InitialSize];

        return rest;
    }

    // Delegate entrypoint.
    public void Run(double deltaTime)
    {
        {
            var lockTaken = false;

            try
            {
                _gate.Enter(ref lockTaken);
                if (_actionListCount == 0) return;
                _dequeuing = true;
            }
            finally
            {
                if (lockTaken) _gate.Exit(false);
            }
        }

        for (var i = 0; i < _actionListCount; i++)
        {

            var action = _actionList[i];
            _actionList[i] = null;

            try { action(); }
            catch (Exception ex) { GD.PrintErr(ex); }
        }

        {
            var lockTaken = false;

            try
            {
                _gate.Enter(ref lockTaken);
                _dequeuing = false;

                var swapTempActionList = _actionList;

                _actionListCount = _waitingListCount;
                _actionList = _waitingList;

                _waitingListCount = 0;
                _waitingList = swapTempActionList;
            }
            finally
            {
                if (lockTaken) _gate.Exit(false);
            }
        }
    }
}