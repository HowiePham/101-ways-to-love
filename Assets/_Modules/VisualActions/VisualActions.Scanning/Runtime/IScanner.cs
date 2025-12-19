using UnityEngine;

namespace Mimi.VisualActions.Scanning
{
    public interface IScanner
    {
        void SetActive(bool active);
        void SetPosition(Vector3 position);
    }
}