using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Mimi.VisualActions.Choices
{
    public class CenterAlignCorrectChoiceAnimator : MonoBehaviour, IChoiceAnimator
    {
        [SerializeField] private float selectAnimateSecs;
        [SerializeField] private Ease ease;

        public async UniTask AnimateHide(IEnumerable<IChoiceOption> allChoiceOptions, CancellationToken cancellationToken)
        {
            var animatedTasks = new List<UniTask>(5);
            foreach (IChoiceOption option in allChoiceOptions)
            {
                animatedTasks.Add(option.Transform.DOScale(Vector3.zero, this.selectAnimateSecs).SetEase(this.ease)
                    .AsyncWaitForCompletion().AsUniTask());
            }

            await UniTask.WhenAll(animatedTasks).AttachExternalCancellation(cancellationToken);
        }

        public void Reset(IEnumerable<IChoiceOption> allChoiceOptions)
        {
            foreach (IChoiceOption option in allChoiceOptions)
            {
                option.Transform.localScale = Vector3.zero;
                option.Transform.position = option.StartingPosition;
            }
        }

        public async UniTask AnimateShowing(IEnumerable<IChoiceOption> allChoiceOptions,
            CancellationToken cancellationToken)
        {
            var animatedTasks = new List<UniTask>(5);
            foreach (IChoiceOption option in allChoiceOptions)
            {
                animatedTasks.Add(option.Transform.DOScale(Vector3.one, this.selectAnimateSecs).SetEase(this.ease)
                    .AsyncWaitForCompletion().AsUniTask());
            }

            await UniTask.WhenAll(animatedTasks).AttachExternalCancellation(cancellationToken);
        }

        public async UniTask AnimateSelection(IChoiceOption selectedOption, IEnumerable<IChoiceOption> allOptions,
            CancellationToken cancellationToken)
        {
            var animatedTasks = new List<UniTask>(5);
            UniTask moveSelectedOption = selectedOption.Transform.DOMoveX(0f, this.selectAnimateSecs).SetEase(this.ease)
                .AsyncWaitForCompletion().AsUniTask();
            animatedTasks.Add(moveSelectedOption);

            foreach (IChoiceOption option in allOptions)
            {
                if (option == selectedOption) continue;
                animatedTasks.Add(option.Transform.DOScale(Vector3.zero, this.selectAnimateSecs).SetEase(this.ease)
                    .AsyncWaitForCompletion().AsUniTask());
            }

            await UniTask.WhenAll(animatedTasks).AttachExternalCancellation(cancellationToken);
        }
    }
}