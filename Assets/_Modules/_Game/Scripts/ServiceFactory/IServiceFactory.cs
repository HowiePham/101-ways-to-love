using Cysharp.Threading.Tasks;

namespace Mimi.Prototypes
{
    public interface IServiceFactory<T>
    {
        UniTask<T> CreateService();
    }
}