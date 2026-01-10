using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Mimi.Games.Plugins;
using Mimi.Prototypes.SceneManagement;
using Mimi.StateMachine;
using UnityEngine;

namespace Mimi.Prototypes
{
    [RequireComponent(typeof(MachineBehaviour))]
    public abstract class BaseGameScene : BaseSceneController<GameContext>
    {
        private readonly CompositePlugin localPluginContainer = new CompositePlugin();
        private MachineBehaviour machineBehaviour;

        public override void RequestAssets()
        {
        }

        protected abstract void AddLocalPlugins(CompositePlugin pluginInstaller);

        protected override async UniTask OnEnter()
        {
            await base.OnEnter();
            InstallStates();
            AddLocalPlugins(this.localPluginContainer);
            Context.InjectPluginConfigs(this.localPluginContainer.Plugins);
            await this.localPluginContainer.Install();
            await this.localPluginContainer.Begin();
        }

        private void InstallStates()
        {
            this.machineBehaviour = GetComponent<MachineBehaviour>();
            this.machineBehaviour.Initialize();

            BaseSceneState[] states = GetComponents<BaseSceneState>();
            Type sceneStateType = typeof(BaseSceneState);

            foreach (BaseSceneState state in states)
            {
                sceneStateType.GetProperty("Context",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(state, Context, null);
                sceneStateType.GetProperty("Presenter",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(state, ScenePresenter, null);
                this.machineBehaviour.AddState(state);
                state.OnInitialized();
            }
        }

        protected override async UniTask OnExit(bool isReloaded)
        {
            await base.OnExit(isReloaded);
            await this.localPluginContainer.End();
            await this.localPluginContainer.Uninstall();
        }

        protected void ChangeState<TState>() where TState : IState
        {
            this.machineBehaviour.ChangeState<TState>();
        }

        private void Update()
        {
            if (!IsInitialized) return;
            if (this.machineBehaviour != null)
            {
                this.machineBehaviour.Tick(Time.deltaTime);
            }
        }
    }
}