using UnityEngine;

namespace VisualActions.Drawing
{
    public abstract class BaseDrawExtension : MonoBehaviour
    {
        public abstract void OnStartDrawing();
        public abstract void OnDrawing();
        public abstract void OnEndDrawing();
    }
}