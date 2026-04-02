using System;
using Godot;

namespace GodotTask;

partial class TaskTrackerWindow
{
    private class CheckButtonObserver(CheckButton checkButton) : IObserver<bool>
    {
        public void OnCompleted() { }

        public void OnError(Exception error) => GD.PrintErr(error.ToString());

        public void OnNext(bool value)
        {
            if (checkButton.ButtonPressed == value) return;
            checkButton.SetPressedNoSignal(value);
        }
    }
}