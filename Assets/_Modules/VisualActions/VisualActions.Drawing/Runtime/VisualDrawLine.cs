using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Mimi.Graphic.LineRenderers;
using Mimi.VisualActions;
using UnityEngine;
#if UNITY_EDITOR
using Mimi.Debugging.UnityGizmos;
#endif

namespace VisualActions.Drawing
{
    public class VisualDrawLine : VisualAction
    {
        [SerializeField] private MonoLineRenderer lineRenderer;
        [SerializeField] private BaseDrawExtension drawExtension;
        [SerializeField] private BaseDrawResultValidator resultValidator;

        private bool done;
        private Camera renderCamera;
        private bool completeAfterFingerUp;
        private bool fingerDown;
        private Transform pencilTrans;

        public Vector3[] Path { private set; get; }

        private static GameObject pencilPrefab;
        private static readonly Vector2 PenOffset = new(0f, 0.8f);

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            this.renderCamera = Camera.main;
            // Path = CreateVectorHintPath();

            if (pencilPrefab == null)
            {
                pencilPrefab = Resources.Load<GameObject>("Draw_Pencil");
            }

            this.pencilTrans = Instantiate(pencilPrefab, new Vector3(-1000f, 0f, 0f), Quaternion.identity).transform;
            this.pencilTrans.gameObject.SetActive(false);
        }

        // private Vector3[] CreateVectorHintPath()
        // {
        //     var pathList = new List<Vector3>(this.pointRoot.childCount);
        //     for (int i = 0; i < this.pointRoot.childCount; i++)
        //     {
        //         Transform pointTrans = this.pointRoot.GetChild(i);
        //         pathList.Add(pointTrans.position);
        //     }
        //
        //     return pathList.ToArray();
        // }

        private void FingerDownHandler(LeanFinger finger)
        {
            this.fingerDown = true;
        }

        private void FingerUpHandler(LeanFinger finger)
        {
            if (!this.fingerDown) return;
            this.fingerDown = false;
            this.pencilTrans.gameObject.SetActive(false);

            if (this.done)
            {
                this.completeAfterFingerUp = true;
                CleanUpLine();
            }
            else
            {
                CleanUpLine();
            }
        }

        private void FingerUpdateHandler(LeanFinger finger)
        {
            if (!this.fingerDown) return;
            Vector2 pos = this.renderCamera.ScreenToWorldPoint(finger.ScreenPosition);
            Vector2 brushLine = pos + PenOffset;
            this.pencilTrans.position = brushLine;
            this.pencilTrans.gameObject.SetActive(true);
            this.lineRenderer.AddPoint(brushLine);
            this.done = this.resultValidator.Validate(brushLine);
        }

        private void CleanUpLine()
        {
            this.lineRenderer.ClearPoints();
            this.resultValidator.Clear();
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.lineRenderer.Init();
            this.resultValidator.Initialize();

            LeanTouch.OnFingerDown += FingerDownHandler;
            LeanTouch.OnFingerUpdate += FingerUpdateHandler;
            LeanTouch.OnFingerUp += FingerUpHandler;

            try
            {
                await UniTask.WaitUntil(() => this.completeAfterFingerUp, PlayerLoopTiming.Update, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
            }
            finally
            {
                Dispose();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            this.lineRenderer.Dispose();
            LeanTouch.OnFingerDown -= FingerDownHandler;
            LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;

            if (this.pencilTrans != null)
            {
                Destroy(this.pencilTrans.gameObject);
            }
        }
    }
}