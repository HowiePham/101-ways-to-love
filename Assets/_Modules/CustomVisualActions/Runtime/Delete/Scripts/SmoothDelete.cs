using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Common;
using Lean.Touch;
using Mimi.Audio;
using Mimi.Prototypes;
using Mimi.ServiceLocators;
using Mimi.Services.ScriptableObject.Audio;
using ScratchCardAsset;
using ScratchCardAsset.Core;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.U2D.Animation;
using Mimi.VisualActions;

#if UNITY_EDITOR
#endif

namespace VisualFlow
{
    public class SmoothDelete : VisualAction, IPathHint, IBaseDelete
    {
        [SerializeField] protected Vector3 brushHeaderOffset;
        [SerializeField, Required] protected ScratchCard scratchCard;
        [SerializeField] protected EraseProgress eraseProgress;
        [SerializeField, Required] protected SpriteRenderer maskSpriteRenderer;
        [SerializeField, Range(0f, 1f)] public float targetDeletePercentage = 0.8f;
        [SerializeField, Required] protected Texture brushMaskTexture;
        [SerializeField] protected float eraseTextureScale = 1f;
        [SerializeField] protected bool scratchSurfaceSpriteHasAlpha = true;
        [SerializeField, Required] protected Shader maskShader;
        [SerializeField, Required] protected Shader brushShader;
        [SerializeField, Required] protected Shader maskProgressShader;
        [SerializeField] protected ScratchMode scratchMode;
        [SerializeField] protected bool resetIfDeleteNotComplete = true;
        [SerializeField] protected bool checkDeleteWhileDragging = false;
        [SerializeField] protected float deleteProgress;

        [Header("SFX")] [SerializeField] protected BaseAudioServiceSO audioService;
        [SerializeField, SoundKey] private string soundKey;

        protected Sprite scratchSprite;
        protected Color[] spritePixels;
        protected Material eraserMaterial;
        protected bool isFingerDowned;
        protected bool isCompleted;
        protected Vector3 startPos;

        protected bool IsDeleteComplete => this.scratchMode == ScratchMode.Erase
            ? this.deleteProgress >= this.targetDeletePercentage
            : this.deleteProgress <= this.targetDeletePercentage;

        public Vector3[] Path { protected set; get; }
        public event Action<Vector3> OnStartDelete;
        public event Action OnStopDelete;
        public event Action<Vector3> OnDeleting;

        protected float DeleteProgress => this.eraseProgress.GetProgress();

        public bool IsBeingDeleted => this.deleteProgress > 0f;

        [SerializeField] protected Camera mainCamera;

        public Camera MainCamera
        {
            get => mainCamera;
            set
            {
                mainCamera = value;
                if (scratchCard != null && scratchCard.ScratchData != null)
                {
                    scratchCard.ScratchData.Camera = mainCamera;
                }
            }
        }

        protected virtual void InitScratchCard()
        {
            Bounds bounds = this.maskSpriteRenderer.bounds;
            Path = new[] { bounds.min, bounds.max };
            mainCamera = Camera.main;
            Material scratchSurfaceMaterial = InitScratchCardSurfaceMaterial();

            ValidateSpriteIfItBelongToAtlas();
            UpdateCardSprite(this.maskSpriteRenderer.sprite);
            InitScratchCardShader();
            InitBrushMaterial();
            InitEraseProgress();
            InitScratchMode();
            SetProgressSourceTexture();

            this.scratchCard.SurfaceTransform = this.maskSpriteRenderer.transform;

            if (this.maskSpriteRenderer != null)
            {
                this.maskSpriteRenderer.material = scratchSurfaceMaterial;
            }
            else
            {
                Debug.LogError(
                    "Can't find SpriteRenderer component on " + this.maskSpriteRenderer.name + " GameObject!");
            }

            this.scratchCard.SetRenderType(ScratchCardRenderType.SpriteRenderer, this.mainCamera);
            scratchCard.BrushSize = eraseTextureScale;
            scratchCard.OnRenderTextureInitialized -= OnCardRenderTextureInitialized;
            scratchCard.OnRenderTextureInitialized += OnCardRenderTextureInitialized;
            scratchCard.Init();
            SetScratchCardFill();
        }

        protected virtual Material InitScratchCardSurfaceMaterial()
        {
            Material scratchSurfaceMaterial = null;
            // if (this.scratchCard.SurfaceMaterial == null)
            // {
            scratchSurfaceMaterial = new Material(this.maskShader)
            {
                mainTexture = this.maskSpriteRenderer.sprite.texture
            };
            this.scratchCard.SurfaceMaterial = scratchSurfaceMaterial;
            // }

            return scratchSurfaceMaterial;
        }

        protected virtual void InitEraseProgress()
        {
            if (this.eraseProgress.ProgressMaterial == null)
            {
                Shader shader = this.maskProgressShader;
                var progressMaterial = new Material(shader);
                this.eraseProgress.ProgressMaterial = progressMaterial;
                this.eraseProgress.SampleSourceTexture = this.scratchSurfaceSpriteHasAlpha;
            }

            this.eraseProgress.ProgressAccuracy = ProgressAccuracy.Default;
        }

        protected virtual void InitBrushMaterial()
        {
            if (this.scratchCard.BrushMaterial == null)
            {
                this.eraserMaterial = new Material(this.brushShader) { mainTexture = this.brushMaskTexture };
                this.scratchCard.BrushMaterial = this.eraserMaterial;
            }

            this.scratchCard.BrushSize = this.eraseTextureScale;
        }

        protected virtual void InitScratchMode()
        {
            this.scratchCard.Mode = this.scratchMode;
        }

        protected virtual void SetScratchCardFill()
        {
            if (this.scratchMode == ScratchMode.Erase)
            {
                ClearScratchCard();
            }
            else
            {
                FillScratchCard();
            }
        }

        protected virtual void InitScratchCardShader()
        {
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
        }

        protected virtual void ValidateSpriteIfItBelongToAtlas()
        {
            if (CheckSpriteBelongToAtlas(this.maskSpriteRenderer.sprite))
            {
                var croppedTexture = CreateNewTextureFromAtlas(this.maskSpriteRenderer.sprite);
                var newSpriteRenderer = CopySpriteComponent(croppedTexture, this.maskSpriteRenderer);
                ChangeOriginalSpriteAlpha();
                this.maskSpriteRenderer = newSpriteRenderer;
            }
        }

        protected virtual void ChangeOriginalSpriteAlpha()
        {
            Color color = this.maskSpriteRenderer.color;
            color.a = 0f;
            this.maskSpriteRenderer.color = color;
        }

        protected virtual SpriteRenderer CopySpriteComponent(Texture2D cloneTexture, SpriteRenderer referenceSpriteRenderer)
        {
            var originalSpriteSkin = referenceSpriteRenderer.gameObject.GetComponent<SpriteSkin>();
            var newDeleteGameObject = new GameObject(referenceSpriteRenderer.name + " (Clone)");
            CopySpriteTransform(referenceSpriteRenderer.gameObject.transform, newDeleteGameObject.transform);

            var newSpriteRenderer = newDeleteGameObject.AddComponent<SpriteRenderer>();
            CopySpriteRenderer(referenceSpriteRenderer, newSpriteRenderer);

            if (originalSpriteSkin != null)
            {
                var newSpriteSkin = newDeleteGameObject.AddComponent<SpriteSkin>();
                CopySpriteSkin(originalSpriteSkin, newSpriteSkin);
            }

            var cloneSprite = Sprite.Create(cloneTexture, new Rect(0, 0, cloneTexture.width, cloneTexture.height),
                new Vector2(0.5f, 0.5f), 100f);
            CopySpriteGeometry(referenceSpriteRenderer.sprite, cloneSprite);
            CopySpriteAnimationData(referenceSpriteRenderer.sprite, cloneSprite);
            cloneSprite.name = referenceSpriteRenderer.gameObject.name + "(Clone)";
            newSpriteRenderer.sprite = cloneSprite;

            return newSpriteRenderer;
        }

        protected virtual void CopySpriteRenderer(SpriteRenderer source, SpriteRenderer destination)
        {
            destination.sprite = source.sprite;
            destination.sortingOrder = source.sortingOrder;
            destination.maskInteraction = source.maskInteraction;
        }

        protected virtual void CopySpriteSkin(SpriteSkin source, SpriteSkin destination)
        {
            var spriteSkinType = typeof(SpriteSkin);
            spriteSkinType.GetProperty("autoRebind", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(destination, true);
            spriteSkinType.GetProperty("alwaysUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(destination, true);
            spriteSkinType.GetProperty("rootBone", BindingFlags.Instance | BindingFlags.Public)
                ?.SetValue(destination, source.rootBone);
            spriteSkinType.GetProperty("boneTransforms", BindingFlags.Instance | BindingFlags.Public)
                ?.SetValue(destination, source.boneTransforms);
        }

        protected virtual void CopySpriteTransform(Transform source, Transform destination)
        {
            destination.SetParent(source);
            destination.position = source.position;
            destination.rotation = Quaternion.identity;
            destination.localScale = new Vector3(1f, 1f, 1f);
        }

        protected virtual Texture2D CreateNewTextureFromAtlas(Sprite sprite)
        {
            var croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.ARGB32,
                false);
            var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y,
                (int)sprite.rect.width, (int)sprite.rect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            return croppedTexture;
        }

        protected virtual bool CheckSpriteBelongToAtlas(Sprite sprite) =>
            sprite.rect.width < sprite.texture.width || sprite.rect.height < sprite.texture.height;

        protected virtual void UpdateCardSprite(Sprite sprite)
        {
            //ReleaseTexture();
            var scratchSurfaceMaterial = this.scratchCard.SurfaceMaterial;
            if (Application.isPlaying)
            {
                if (scratchSurfaceMaterial != null && maskSpriteRenderer.sprite != null)
                {
                    scratchSurfaceMaterial.mainTexture = maskSpriteRenderer.sprite.texture;
                }

                UpdateProgressMaterial();
            }
            else if (this.scratchCard.SurfaceMaterial != null && maskSpriteRenderer.sprite != null)
            {
                this.scratchCard.SurfaceMaterial.mainTexture = maskSpriteRenderer.sprite.texture;
            }

            if (maskSpriteRenderer != null)
            {
                if (this.scratchCard.SurfaceMaterial != null)
                {
                    maskSpriteRenderer.sharedMaterial = this.scratchCard.SurfaceMaterial;
                }

                if (sprite != null)
                {
                    maskSpriteRenderer.sprite = sprite;
                }
            }
        }

        protected virtual void UpdateProgressMaterial()
        {
            if (eraseProgress != null)
            {
                if (eraseProgress.ProgressMaterial != null)
                {
                    SetProgressSourceTexture();
                }

                if (Application.isPlaying && spritePixels != null)
                {
                    eraseProgress.SetSpritePixels(spritePixels);
                    spritePixels = null;
                }
            }
        }

        protected virtual void SetProgressSourceTexture()
        {
            if (scratchSurfaceSpriteHasAlpha)
            {
                if (this.maskSpriteRenderer.sprite.texture != null)
                {
                    this.eraseProgress.ProgressMaterial.SetTexture(Constants.ProgressShader.SourceTexture,
                        this.maskSpriteRenderer.sprite.texture);
                }
                else if (this.maskSpriteRenderer.sprite != null)
                {
                    this.eraseProgress.ProgressMaterial.SetTexture(Constants.ProgressShader.SourceTexture,
                        this.maskSpriteRenderer.sprite.texture);
                }
            }
        }

        protected virtual void OnCardRenderTextureInitialized(RenderTexture renderTexture)
        {
            if (eraseProgress != null && eraseProgress.ProgressMaterial != null)
            {
                eraseProgress.ProgressMaterial.mainTexture = renderTexture;
            }
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            try
            {
                InitScratchCard();

                LeanTouch.OnFingerDown += FingerDownHandler;
                LeanTouch.OnFingerUpdate += FingerUpdateHandler;
                LeanTouch.OnFingerUp += FingerUpHandler;

                await UniTask.WaitUntil(() => this.isCompleted,
                    PlayerLoopTiming.Update,
                    cancellationToken);
            }
            catch (Exception e)
            {
            }
            finally
            {
                LeanTouch.OnFingerDown -= FingerDownHandler;
                LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
                LeanTouch.OnFingerUp -= FingerUpHandler;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
            LeanTouch.OnFingerDown -= FingerDownHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
        }

        [SerializeField] protected LeanSelectable selectable;

        protected Vector2 prevFingerPosition;
        protected bool isFirstDrag = true;

        public virtual void FingerUpdateHandler(LeanFinger finger)
        {
            if (this.selectable != null && !this.selectable.IsSelected) return;
            if (!this.isFingerDowned || this.IsPaused) return;
            if (finger.IsOverGui) return;

            if (isFirstDrag)
            {
                prevFingerPosition = finger.ScreenPosition;
                isFirstDrag = false;
            }

            Vector3 fingerWorldPos = mainCamera.ScreenToWorldPoint(finger.ScreenPosition);
            Vector3 eraseCenter = fingerWorldPos;
            eraseCenter.z += 1f;
            Vector3 brushPos = new Vector3(fingerWorldPos.x, fingerWorldPos.y) + this.brushHeaderOffset;
            eraseCenter.z = 0f;
            OnDeleting?.Invoke(brushPos);
            prevFingerPosition = finger.ScreenPosition;
            this.deleteProgress = this.eraseProgress.GetProgress();

            if (this.checkDeleteWhileDragging)
            {
                this.isCompleted = this.IsDeleteComplete;
            }
        }

        public virtual void FingerUpHandler(LeanFinger finger)
        {
            isFirstDrag = true;
            // ServiceLocator.GetService<IAudioService>().StopSound(this.soundFX);
            this.audioService.StopSound(this.soundKey);
            OnStopDelete?.Invoke();
            this.isFingerDowned = false;
            CheckDeleteComplete(finger);
        }

        private void PlayDeleteSound()
        {
            if (string.IsNullOrEmpty(this.soundKey))
            {
                return;
            }

            this.audioService.PlaySound(this.soundKey);
        }

        private void CheckDeleteComplete(LeanFinger finger)
        {
            this.isCompleted = this.IsDeleteComplete;
            if (!this.isCompleted && this.resetIfDeleteNotComplete)
            {
                if (this.scratchMode == ScratchMode.Restore)
                {
                    FillScratchCard();
                }
                else if (this.scratchMode == ScratchMode.Erase)
                {
                    ResetDeleteProgress();
                }

                if (finger.IsOverGui)
                {
                    return;
                }
            }
            else if (this.isCompleted && this.scratchMode == ScratchMode.Erase)
            {
                this.maskSpriteRenderer.gameObject.SetActive(false);
            }
        }

        protected virtual IEnumerator DelayFrame()
        {
            yield return new WaitForSeconds(0.1f);
            if (!this.isCompleted)
                ResetDeleteProgress();
        }

        public void ResetDeleteProgress()
        {
            this.eraseProgress.ResetProgress();
            this.scratchCard.ResetRenderTexture();
            this.maskSpriteRenderer.gameObject.SetActive(true);
        }

        public void ResetDeleteMaterial()
        {
            Debug.Log($"--- (DELETE) Mask Sprite Material: {this.maskSpriteRenderer.material}");
            // this.maskSpriteRenderer.material = this.scratchCard.SurfaceMaterial;
        }

        public virtual void FingerDownHandler(LeanFinger finger)
        {
            if (finger.IsOverGui)
            {
                this.isFingerDowned = false;
            }
            else
            {
                this.isFingerDowned = true;
                this.startPos = mainCamera.ScreenToWorldPoint(finger.ScreenPosition);
                PlayDeleteSound();
                OnStartDelete?.Invoke(this.startPos);
            }
        }

        protected virtual void CopySpriteAnimationData(Sprite source, Sprite destination)
        {
            InjectBoneWeights(source, destination);
            destination.SetBones(source.GetBones());
            destination.SetBindPoses(source.GetBindPoses());
        }

        protected virtual void CopySpriteGeometry(Sprite source, Sprite destination)
        {
            Vector2[] spriteVertices = CheckSpriteBelongToAtlas(source)
                ? CalculateVerticesOffsetFromAtlas(source)
                : source.vertices;
            destination.OverrideGeometry(spriteVertices, source.triangles);
        }

        protected virtual Vector2[] CalculateVerticesOffsetFromAtlas(Sprite referenceSprite)
        {
            Vector2[] spriteVertices = referenceSprite.vertices;
            for (int i = 0; i < spriteVertices.Length; ++i)
                spriteVertices[i] = (spriteVertices[i] * referenceSprite.pixelsPerUnit) + referenceSprite.pivot;

            return spriteVertices;
        }

        protected virtual void InjectBoneWeights(Sprite source, Sprite destination)
        {
            var vertexCount = destination.vertices.Length;
            var blendWeightArr = new NativeArray<BoneWeight>(vertexCount, Allocator.Temp);

            var blendWeightRef = source.GetVertexAttribute<BoneWeight>(VertexAttribute.BlendWeight);
            for (var i = 0; i < blendWeightArr.Length; ++i)
                blendWeightArr[i] = blendWeightRef[i % blendWeightRef.Length];

            destination.SetVertexAttribute(VertexAttribute.BlendWeight, blendWeightArr);
            blendWeightArr.Dispose();
        }

        protected virtual void FillScratchCard()
        {
            this.scratchCard.Fill(false);
            this.eraseProgress.UpdateFill();
        }

        protected virtual void ClearScratchCard()
        {
            this.scratchCard.Clear();
            this.scratchCard.ResetRenderTexture();
        }
    }
}