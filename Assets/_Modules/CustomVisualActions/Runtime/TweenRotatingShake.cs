using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Games
{
    public class TweenRotatingShake : MonoBehaviour
    {
        [SerializeField] private float angle;
        [SerializeField] private float duration;
        [SerializeField] private float delay;
        [SerializeField] private Ease ease;
        [SerializeField] private int loop;
        [SerializeField] private bool autoStart;

        private void Start()
        {
            if (this.autoStart)
            {
                DoShake();
            }
        }

        private void OnDisable()
        {
            DOTween.Kill(gameObject);
        }

        public void StopShake()
        {
            DOTween.Kill(gameObject);
            transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        }

        public void DoShake()
        {
            DOTween.Kill(gameObject);
            transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -this.angle));
            TweenerCore<Quaternion, Vector3, QuaternionOptions> rotateClockwise = transform
                .DOLocalRotate(new Vector3(0f, 0f, this.angle), this.duration)
                .SetEase(this.ease);

            TweenerCore<Quaternion, Vector3, QuaternionOptions> rotateAntiClockwise = transform
                .DOLocalRotate(new Vector3(0f, 0f, -this.angle), this.duration)
                .SetEase(this.ease);

            Sequence rotate = DOTween.Sequence();
            rotate
                .Append(rotateClockwise)
                .Append(rotateAntiClockwise)
                .SetLoops(this.loop, LoopType.Yoyo);

            TweenerCore<Quaternion, Vector3, QuaternionOptions> balance = transform
                .DOLocalRotate(Vector3.zero, this.duration / 2f)
                .SetEase(this.ease);

            Sequence sequence = DOTween.Sequence();
            sequence
                .Append(rotate)
                .Append(balance)
                .AppendInterval(this.delay)
                .SetLoops(this.loop)
                .SetTarget(gameObject);
        }
    }
}