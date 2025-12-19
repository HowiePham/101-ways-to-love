using Cysharp.Threading.Tasks;

namespace Mimi.Services.ScriptableObject.Core
{
    public abstract class BaseServiceSO : UnityEngine.ScriptableObject, IService
    {
        public abstract UniTask Initialize();
        public abstract void Dispose();
    }
}