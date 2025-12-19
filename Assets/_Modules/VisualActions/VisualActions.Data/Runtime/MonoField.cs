using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Data
{
    public class MonoField<T> : MonoBehaviour, Field<T>
    {
        [SerializeField] protected T value;
        public void SetValue(T value)
        {
            this.value = value;
        }

        public T GetValue()
        {
            return this.value;
        }
    }
}