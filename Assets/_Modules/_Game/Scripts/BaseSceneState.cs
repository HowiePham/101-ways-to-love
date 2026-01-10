using Mimi.Prototypes.UI;
using Mimi.StateMachine;

namespace Mimi.Prototypes
{
    public abstract class BaseSceneState : State<MachineBehaviour>
    {
        protected BaseScenePresenter Presenter { private set; get; }
        protected GameContext Context { private set; get; }

        public abstract void OnInitialized();
    }
}