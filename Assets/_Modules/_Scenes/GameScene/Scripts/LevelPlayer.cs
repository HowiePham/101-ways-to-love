using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameScenes
{
    public class LevelPlayer : MonoBehaviour
    {
        public async UniTask Play()
        {
            await UniTask.Delay(1000);
        }
    }
}