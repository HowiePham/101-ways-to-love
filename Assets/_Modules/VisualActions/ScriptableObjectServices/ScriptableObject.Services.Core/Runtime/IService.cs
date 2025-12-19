using Cysharp.Threading.Tasks;

namespace Mimi.Services.ScriptableObject.Core
{
    public interface IService
    {
        public UniTask Initialize();
        public void Dispose();
    }
}