using UnityEngine;

namespace VisualFlow
{
    public abstract class BaseHintGraphic : MonoBehaviour
    {
        public abstract void SetActive(bool active);
        public abstract void SetColor(Color color);
    }
}