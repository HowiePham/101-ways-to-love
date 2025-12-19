using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Mimi.VisualActions;
using Mimi.ScratchCardAsset;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Deleting
{
    [TypeInfoBox("Manages smooth erasing/scratching effect for SpriteRenderer using scratch card mechanics.")]
    public class VisualSmoothDelete : VisualAction
    {
        [SerializeField] private Vector3 brushHeaderOffset;
        [SerializeField] private bool isResetOnRelease = false;
        [SerializeField, Required] private ScratchCard scratchCard;
        [SerializeField] private EraseProgress eraseProgress;
        [SerializeField, Required] private SpriteRenderer maskSpriteRenderer;
        [SerializeField, Range(0f, 1f)] public float targetDeletePercentage = 0.8f;
        [SerializeField, Required] private Texture brushMaskTexture;
        [SerializeField] private Vector2 eraseTextureScale = Vector2.one;
        [SerializeField] private bool scratchSurfaceSpriteHasAlpha = true;
        [SerializeField, Required] private Shader maskShader;
        [SerializeField, Required] private Shader brushShader;
        [SerializeField, Required] private Shader maskProgressShader;
        [SerializeField, Required] private Shader maskProgressCutOffShader;
        [SerializeField] private ScratchCard.ScratchMode scratchMode = ScratchCard.ScratchMode.Erase;
        [SerializeField] private MonoSmoothDeleteExtension smoothDeleteExtension;
        private Material eraserMaterial;
        private bool isFingerDowned;
        
        public Texture BrushMaskTexture => this.brushMaskTexture;
        public Vector2 EraseSize => eraseTextureScale;
        public Vector3 BrushOffset => brushHeaderOffset;
        public SpriteRenderer MaskSpriteRenderer => maskSpriteRenderer;
        public ScratchCard ScratchCard => scratchCard;
        public bool IsDeleteComplete => scratchMode == ScratchCard.ScratchMode.Erase
            ? DeleteProgress >= this.targetDeletePercentage
            : DeleteProgress <= this.targetDeletePercentage;
        public Vector3[] Path { private set; get; }
        public event Action<Vector3> OnStartDelete;
        public event Action OnStopDelete;
        public event Action<Vector3> OnDeleting;

        public float DeleteProgress => this.eraseProgress.GetProgress();

        public float Progress
        {
            get
            {
                if (scratchMode == ScratchCard.ScratchMode.Erase)
                {
                    return DeleteProgress;
                }

                return Mathf.Abs(1 - DeleteProgress);
            }
        }
        
        private bool completable = true;

        /// <summary>
        /// Initializes scratch card with materials, shaders, and extensions.
        /// </summary>
        /// <returns>UniTask representing async completion.</returns>
        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            if (smoothDeleteExtension == null)
            {
                smoothDeleteExtension = gameObject.AddComponent<NullSmoothDeleteExtension>();
            }
            smoothDeleteExtension.OnInit(this);
            Bounds bounds = this.maskSpriteRenderer.bounds;
            Path = new[] { bounds.min, bounds.max };
            this.scratchCard.MainCamera = Camera.main;
            Material scratchSurfaceMaterial = null;
            if (this.scratchCard.ScratchSurface == null)
            {
                scratchSurfaceMaterial = new Material(this.maskShader)
                { mainTexture = this.maskSpriteRenderer.sprite.texture };
                this.scratchCard.ScratchSurface = scratchSurfaceMaterial;
            }

            if (this.maskShader == null)
            {
                this.maskShader = Shader.Find("ScratchCard/Mask");
            }

            if (this.brushShader == null)
            {
                this.brushShader = Shader.Find("ScratchCard/Brush");
            }

            if (this.maskProgressShader == null)
            {
                this.maskProgressShader = Shader.Find("ScratchCard/MaskProgress");
            }

            if (this.maskProgressCutOffShader == null)
            {
                this.maskProgressCutOffShader = Shader.Find("ScratchCard/MaskProgressCutOff");
            }

            if (this.scratchCard.Eraser == null)
            {
                this.eraserMaterial = new Material(this.brushShader) { mainTexture = this.brushMaskTexture };
                this.scratchCard.Eraser = this.eraserMaterial;
            }

            this.scratchCard.BrushScale = this.eraseTextureScale;
            this.scratchCard.Mode = this.scratchMode;
            this.ScratchCard.SetOffsetBrush(this.brushHeaderOffset);
            if (this.scratchCard.Progress == null)
            {
                Shader shader = this.scratchSurfaceSpriteHasAlpha
                    ? this.maskProgressCutOffShader
                    : this.maskProgressShader;
                var progressMaterial = new Material(shader);
                this.scratchCard.Progress = progressMaterial;
            }

            this.scratchCard.Surface = this.maskSpriteRenderer.transform;

            if (this.maskSpriteRenderer != null)
            {
                this.maskSpriteRenderer.material = scratchSurfaceMaterial;
            }
            else
            {
                Debug.LogError(
                    "Can't find SpriteRenderer component on " + this.maskSpriteRenderer.name + " GameObject!");
            }

            this.scratchCard.Init();
            this.scratchCard.enabled = false;
        }

        /// <summary>
        /// Executes erasing process, handling touch events until completion.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel async operation.</param>
        /// <returns>UniTask representing async completion.</returns>
        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            try
            {
                maskSpriteRenderer.gameObject.SetActive(true);
                scratchCard.enabled = true;
                scratchCard.SetEnableScratching(true);
                this.ResetDeleteProgress();
                LeanTouch.OnFingerDown += FingerDownHandler;
                LeanTouch.OnFingerUp += FingerUpHandler;
                LeanTouch.OnFingerUpdate += FingerUpdateHandler;
                smoothDeleteExtension.OnActionStart();
                Debug.Log($"completable : {completable} isFingerDown : {isFingerDowned} IsDeleteComplete : {IsDeleteComplete} DeleteProgress : {DeleteProgress} DeleteProgress : {DeleteProgress} targetProgress: {targetDeletePercentage}");
                await UniTask.WaitUntil(() => this.completable && !this.isFingerDowned && IsDeleteComplete,
                    PlayerLoopTiming.Update,
                    cancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during execution: {e.Message}");
            }
            finally
            {
                LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
                LeanTouch.OnFingerDown -= FingerDownHandler;
                LeanTouch.OnFingerUp -= FingerUpHandler;
                this.scratchCard.enabled = false;
                scratchCard.SetEnableScratching(false);
                if (this.maskSpriteRenderer != null)
                {
                    this.maskSpriteRenderer.enabled = false;
                }
            }
        }

        /// <summary>
        /// Disposes resources, unregisters touch handlers.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
            LeanTouch.OnFingerDown -= FingerDownHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
        }
        
        /// <summary>
        /// Handles touch update for erasing at position.
        /// </summary>
        /// <param name="finger">The LeanFinger input data.</param>
        private void FingerUpdateHandler(LeanFinger finger)
        {
            if (!this.isFingerDowned) return;
            if (finger.IsOverGui) return;

            Vector3 fingerWorldPos = this.scratchCard.MainCamera.ScreenToWorldPoint(finger.ScreenPosition);
            
            Vector3 eraseCenter = fingerWorldPos;
            eraseCenter.z += 1f;
            Vector3 brushPos = new Vector3(fingerWorldPos.x, fingerWorldPos.y) + this.brushHeaderOffset;
            eraseCenter.z = 0f;
            OnDeleting?.Invoke(brushPos);
            smoothDeleteExtension.OnDelete(brushPos);
        }

        /// <summary>
        /// Handles touch release, stops delete and resets if needed.
        /// </summary>
        /// <param name="finger">The LeanFinger input data.</param>
        private void FingerUpHandler(LeanFinger finger)
        {
            if (finger.IsOverGui) return;
            OnStopDelete?.Invoke();
            smoothDeleteExtension.OnStopDelete();
            
            this.isFingerDowned = false;
            if (!IsDeleteComplete && isResetOnRelease)
            {
                ResetDeleteProgress();
            }
        }

        /// <summary>
        /// Resets delete progress based on mode.
        /// </summary>
        public void ResetDeleteProgress()
        {
            this.eraseProgress.ResetProgress();
            if (this.scratchMode == ScratchCard.ScratchMode.Erase)
            {
                this.scratchCard.ClearInstantly();
            }
            else
            {
                this.scratchCard.FillInstantly();
            }
        }

        /// <summary>
        /// Handles touch down, starts delete at position.
        /// </summary>
        /// <param name="finger">The LeanFinger input data.</param>
        private void FingerDownHandler(LeanFinger finger)
        {
            if (finger.IsOverGui) return;
            this.isFingerDowned = true;
            Vector3 pos = this.scratchCard.MainCamera.ScreenToWorldPoint(finger.ScreenPosition);
            OnStartDelete?.Invoke(pos);
            smoothDeleteExtension.OnStartDelete(pos);
        }
    }
}