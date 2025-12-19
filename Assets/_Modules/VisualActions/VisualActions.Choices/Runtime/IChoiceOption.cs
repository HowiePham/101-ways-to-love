using System;
using Mimi.Actions;
using UnityEngine;

namespace Mimi.VisualActions.Choices
{
    public interface IChoiceOption
    {
        bool IsCorrect { get; }
        Transform Transform { get; }
        Vector3 StartingPosition { get; }
        IAsyncAction Action { get; }
        event Action<IChoiceOption> OnSelected;
    }
}