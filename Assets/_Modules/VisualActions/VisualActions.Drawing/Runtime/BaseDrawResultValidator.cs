using UnityEngine;

namespace VisualActions.Drawing
{
    public abstract class BaseDrawResultValidator : MonoBehaviour
    {
        public abstract void Initialize();
        public abstract bool Validate(Vector3 pointPosition);

        public abstract void Clear();
    }
}