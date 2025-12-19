using System.Collections.Generic;
using UnityEngine;

namespace Mimi.Graphic.LineRenderers
{
    public interface ILineProcessor
    {
        List<Vector2> Process(List<Vector2> points);
    }
}