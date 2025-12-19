using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Mimi.VisualActions.Choices
{
    public interface IChoiceAnimator
    {
        UniTask AnimateShowing(IEnumerable<IChoiceOption> allChoiceOptions, CancellationToken cancellationToken);

        UniTask AnimateSelection(IChoiceOption choiceOption, IEnumerable<IChoiceOption> allChoiceOptions,
            CancellationToken cancellationToken);
        UniTask AnimateHide(IEnumerable<IChoiceOption> allChoiceOptions, CancellationToken cancellationToken);
        void Reset(IEnumerable<IChoiceOption> allChoiceOptions);
    }
}