using UnityEngine;

namespace VisualFlow
{
    public class EraserBrush : MonoBehaviour
    {
        public Transform Transform { private set; get; }

        private void Awake()
        {
            Transform = transform;
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}