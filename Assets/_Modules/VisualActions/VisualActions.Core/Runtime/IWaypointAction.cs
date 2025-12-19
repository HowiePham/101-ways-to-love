using System.Collections.Generic;
using UnityEngine;

namespace Mimi.VisualActions
{
    public interface IWaypointAction
    {
        IEnumerable<Vector3> Waypoints { get; }
    }
}