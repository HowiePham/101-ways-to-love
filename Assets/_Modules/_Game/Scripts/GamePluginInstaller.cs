using Cysharp.Threading.Tasks;
using Mimi.Games.Plugins;
using Mimi.Prototypes;
using UnityEngine;

public class GamePluginInstaller : MonoPluginInstaller
{
    [SerializeField] private GameContext gameContext;
    [SerializeField] private bool persistent;

    protected override async void Awake()
    {
        base.Awake();
        if (this.persistent)
        {
            DontDestroyOnLoad(gameObject);
        }

        await UniTask.WaitUntil(() => this.gameContext.IsInitialized);
        await Install();
        await Begin();
    }

    private async void OnDestroy()
    {
        if (!this.persistent)
        {
            await End();
            await Uninstall();
        }
    }
}