using System.Threading;
using Cysharp.Threading.Tasks;

namespace Mimi.VisualActions.Choices
{
    public class Choice : VisualAction
    {
        private IChoiceOption[] choiceOptions;
        private IChoiceOption selectedOption;
        private IChoiceAnimator choiceAnimator;
        private bool hasSelectedOption;

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            this.choiceAnimator = GetComponent<IChoiceAnimator>();
            this.choiceOptions = GetComponentsInChildren<IChoiceOption>();
            this.choiceAnimator.Reset(this.choiceOptions);
        }

        protected override async UniTask OnEnter(CancellationToken cancellationToken)
        {
            await base.OnEnter(cancellationToken);
            this.selectedOption = null;
            this.hasSelectedOption = false;
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            await this.choiceAnimator.AnimateShowing(this.choiceOptions, cancellationToken);

            foreach (IChoiceOption choiceOption in this.choiceOptions)
            {
                choiceOption.OnSelected += ChoiceSelectedHandler;
            }

            await UniTask.WaitUntil(() => this.hasSelectedOption, cancellationToken: cancellationToken);

            foreach (IChoiceOption choiceOption in this.choiceOptions)
            {
                choiceOption.OnSelected -= ChoiceSelectedHandler;
            }

            await this.choiceAnimator.AnimateSelection(this.selectedOption, this.choiceOptions, cancellationToken);

            if (!this.selectedOption.Action.IsInitialized)
            {
                await this.selectedOption.Action.Initialize();
            }

            await this.selectedOption.Action.Execute(cancellationToken);
            await this.choiceAnimator.AnimateHide(this.choiceOptions, cancellationToken);
        }

        private void ChoiceSelectedHandler(IChoiceOption choiceOption)
        {
            this.selectedOption = choiceOption;
            this.hasSelectedOption = true;
        }
    }
}