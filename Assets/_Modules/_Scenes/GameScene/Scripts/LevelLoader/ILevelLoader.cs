using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LeveLoaders
{
    public interface ILevelLoader
    {
        UniTask<GameObject> Load(string levelId);
        void Release(string levelId);
        void ReleaseAllExcept(string levelId);
        void ReleaseAll();
    }
}