using Mimi.ScratchCardAsset.Core;
using Mimi.ScratchCardAsset.Core.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mimi.ScratchCardAsset
{
    public class ScratchCard : MonoBehaviour
    {
        public enum Quality
        {
            Low = 4,
            Medium = 2,
            High = 1
        }

        public enum ScratchMode
        {
            Erase,
            Restore
        }

        public Camera MainCamera;
        public Transform Surface;
        public Quality RenderTextureQuality = Quality.High;
        public Material Eraser;
        public Material Progress;
        public Material ScratchSurface;
        public RenderTexture RenderTexture;
        public Vector2 BrushScale = Vector2.one;
        public bool InputEnabled = true;

        [SerializeField]
        private ScratchMode _mode = ScratchMode.Erase;

        public ScratchMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                if (Eraser != null)
                {
                    var blendOp = _mode == ScratchMode.Erase ? (int) BlendOp.Add : (int) BlendOp.ReverseSubtract;
                    Eraser.SetInt(BlendOpShaderParam, blendOp);
                }
            }
        }

        public bool IsScratching
        {
            get { return cardInput != null && cardInput.IsScratching; }
        }

        public bool IsScratched
        {
            get
            {
                if (cardRenderer != null)
                {
                    return cardRenderer.IsScratched;
                }
                return false;
            }
            private set { cardRenderer.IsScratched = value; }
        }

        private ScratchCardRenderer cardRenderer;
        private ScratchCardInput cardInput;
        private Triangle triangle;
        private SpriteRenderer surfaceSpriteRenderer;
        private MeshFilter surfaceMeshFilter;
        private Renderer surfaceRenderer;
        private RectTransform surfaceRectTransform;
        private Vector2 boundsSize;
        private Vector2 imageSize;
        private bool isCanvasOverlay;
        private bool isFirstFrame = true;
        private int lastFrameId;
        private bool isInitialized;
        private bool isEnableScratching;
        private Vector2 offsetBrush;

        private const string BlendOpShaderParam = "_BlendOpValue";
        private const string SourceTexProperty = "_SourceTex";

        #region MonoBehaviour Methods

        void OnDestroy()
        {
            if (RenderTexture != null && RenderTexture.IsCreated())
            {
                RenderTexture.Release();
                Destroy(RenderTexture);
            }

            cardRenderer?.Release();
        }

        void Update()
        {
            if (lastFrameId == Time.frameCount)
                return;
            if (isEnableScratching)
            {
                cardInput.Update();
            }

            if (cardInput.IsScratching)
            {
                cardInput.Scratch();
            }
            else
            {
                cardRenderer.IsScratched = false;
            }

            lastFrameId = Time.frameCount;
        }

        #endregion

        #region Initializaion

        public void Init()
        {
            if (this.isInitialized) return;
            this.isInitialized = true;
            GetScratchBounds();
            InitVariables();
            InitTriangle();
        }

        public void SetEnableScratching(bool enable)
        {
            this.isEnableScratching = enable;
        }

        public void SetOffsetBrush(Vector2 offset)
        {
            this.offsetBrush = offset;
        }
        private void GetScratchBounds()
        {
            surfaceRenderer = Surface.GetComponent<Renderer>();
            if (surfaceRenderer != null)
            {
                var sourceTexture = surfaceRenderer.sharedMaterial.mainTexture;
                imageSize = new Vector2(sourceTexture.width, sourceTexture.height);
                surfaceMeshFilter = Surface.GetComponent<MeshFilter>();
                if (surfaceMeshFilter != null)
                {
                    boundsSize = surfaceMeshFilter != null && !MainCamera.orthographic
                        ? surfaceMeshFilter.sharedMesh.bounds.size
                        : surfaceRenderer.bounds.size;
                }
                else
                {
                    surfaceSpriteRenderer = Surface.GetComponent<SpriteRenderer>();
                    boundsSize = surfaceSpriteRenderer != null && !MainCamera.orthographic
                        ? surfaceSpriteRenderer.sprite.bounds.size
                        : surfaceRenderer.bounds.size;
                }
            }
            else
            {
                surfaceRectTransform = Surface.GetComponent<RectTransform>();
                if (surfaceRectTransform != null)
                {
                    var canvas = Surface.GetComponentInParent<Canvas>();
                    if (canvas != null)
                    {
                        isCanvasOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay;
                    }
                    var rect = surfaceRectTransform.rect;
                    imageSize = new Vector2(rect.width, rect.height);
                    boundsSize = MainCamera.orthographic || isCanvasOverlay
                        ? Vector2.Scale(rect.size, surfaceRectTransform.lossyScale)
                        : imageSize;
                }
                else
                {
                    Debug.LogError("Can't find Renderer or RectTransform Component!");
                }
            }
        }

        private void InitVariables()
        {
            cardInput = new ScratchCardInput(this);
            cardInput.OnScratchStart -= OnScratchStart;
            cardInput.OnScratchStart += OnScratchStart;
            cardInput.OnScratchHole -= OnScratchHole;
            cardInput.OnScratchHole += OnScratchHole;
            cardInput.OnScratchLine -= OnScratchLine;
            cardInput.OnScratchLine += OnScratchLine;
            cardInput.OnScratch -= GetScratchPosition;
            cardInput.OnScratch += GetScratchPosition;
            if (cardRenderer != null)
            {
                cardRenderer.Release();
            }

            cardRenderer = new ScratchCardRenderer(this);
            cardRenderer.SetImageSize(imageSize);
            cardRenderer.CreateRenderTexture();
        }

        private void InitTriangle()
        {
            //bottom left
            var position0 = new Vector3(-boundsSize.x / 2f, -boundsSize.y / 2f, 0);
            var uv0 = Vector2.zero;
            //upper left
            var position1 = new Vector3(-boundsSize.x / 2f, boundsSize.y / 2f, 0);
            var uv1 = Vector2.up;
            //upper right
            var position2 = new Vector3(boundsSize.x / 2f, boundsSize.y / 2f, 0);
            var uv2 = Vector2.one;
            triangle = new Triangle(position0, position1, position2, uv0, uv1, uv2);
        }

        #endregion

        private void OnScratchStart()
        {
            cardRenderer.IsScratched = false;
        }

        private void OnScratchHole(Vector2 position)
        {
            cardRenderer.ScratchHole(position+offsetBrush);
        }

        private void OnScratchLine(Vector2 start, Vector2 end)
        {
            cardRenderer.ScratchLine(start+offsetBrush, end+offsetBrush);
        }

        private Vector2 GetScratchPosition(Vector2 position)
        {
            var scratchPosition = Vector2.zero;
            if (MainCamera.orthographic || isCanvasOverlay)
            {
                var clickPosition = isCanvasOverlay ? (Vector3)position : MainCamera.ScreenToWorldPoint(position);
                var lossyScale = Surface.lossyScale;
                var clickLocalPosition = Vector2.Scale(Surface.InverseTransformPoint(clickPosition), lossyScale) +
                                        boundsSize / 2f;
                var pixelsPerInch = new Vector2(imageSize.x / boundsSize.x / lossyScale.x,
                    imageSize.y / boundsSize.y / lossyScale.y);
                scratchPosition = Vector2.Scale(Vector2.Scale(clickLocalPosition, lossyScale), pixelsPerInch);
            }
            else
            {
                var plane = new Plane(Surface.forward, Surface.position);
                var ray = MainCamera.ScreenPointToRay(position);
                float enter;
                if (plane.Raycast(ray, out enter))
                {
                    var point = ray.GetPoint(enter);
                    var pointLocal = Surface.InverseTransformPoint(point);
                    var uv = triangle.GetUV(pointLocal);
                    scratchPosition = new Vector2(uv.x * imageSize.x, uv.y * imageSize.y);
                }
            }
            return scratchPosition;
        }

        #region Public Methods

        public void FillInstantly()
        {
            cardRenderer.FillRenderTextureWithColor(Color.white);
            IsScratched = true;
        }

        public void ClearInstantly()
        {
            cardRenderer.FillRenderTextureWithColor(Color.clear);
            IsScratched = true;
            Debug.Log($"ClearInstantly: Mode={_mode}");
        }

        public void Clear()
        {
            isFirstFrame = true;
            IsScratched = true;
            Debug.Log($"Clear: Mode={_mode}");
        }

        public void ResetRenderTexture()
        {
            cardRenderer.CreateRenderTexture();
            isFirstFrame = true;
            IsScratched = true;
        }

        public void ScratchHole(Vector2 position, float pressure = 1f)
        {
            cardRenderer.ScratchHole(position);
            IsScratched = true;
        }

        public void ScratchLine(Vector2 startPosition, Vector2 endPosition, float startPressure = 1f, float endPressure = 1f)
        {
            cardRenderer.ScratchLine(startPosition, endPosition);
            IsScratched = true;
        }

        public Texture2D GetScratchTexture()
        {
            var previousRenderTexture = RenderTexture.active;
            var texture2D = new Texture2D(RenderTexture.width, RenderTexture.height, TextureFormat.ARGB32, false);
            RenderTexture.active = RenderTexture;
            texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0, false);
            texture2D.Apply();
            RenderTexture.active = previousRenderTexture;
            return texture2D;
        }

        public void SetScratchTexture(Texture2D texture)
        {
            this.isInitialized = false;
            Init();
            ClearInstantly();
            Graphics.Blit(texture, RenderTexture);
            if (Progress.HasProperty(SourceTexProperty))
            {
                Progress.SetTexture(SourceTexProperty, texture);
            }
            ScratchSurface.mainTexture = texture;
            IsScratched = true;
            Debug.Log($"SetScratchTexture: Mode={_mode}");
        }

        #endregion
    }
}