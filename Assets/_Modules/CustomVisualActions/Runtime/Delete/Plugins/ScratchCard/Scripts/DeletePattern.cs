using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hint;
using Lean.Touch;
using Mimi.VisualActions;
using UnityEngine;
using VisualFlow;

namespace Erase
{
    public class DeletePattern : VisualAction, IPathHintable
    {
        [SerializeField] private Eraser eraser;
        [SerializeField] private EraserBrush eraserBrush;
        private const float brushRadius = 1f;
        [SerializeField] private Transform pointRoot;
        [SerializeField] private Transform wrongPointRoot;
        [SerializeField] private Vector3 checkBoxSize;
        [SerializeField] private Vector3 wrongBoxSize;
        [SerializeField] private Vector3 eraseAreaCenterOffset;
        [SerializeField] private Vector3 brushHeaderOffset;
        [SerializeField] private bool autoClearBrush = true;
        [SerializeField] private bool clearEraseOnFingerUp = true;

        private Camera gameCamera;
        private List<CheckBoundState> bounds;
        private List<CheckBoundState> wrongBounds;
        private bool done;
        private bool completeAfterFingerUp;
        private bool fingerDown;
        private Vector3 velocity = Vector3.zero;

        public Vector3[] Path { private set; get; }

        protected override async UniTask OnInitializing()
        {
            await base.OnInitializing();
            if (this.wrongPointRoot == null)
            {
                this.wrongPointRoot = new GameObject("WrongPoints").transform;
                this.wrongPointRoot.SetParent(transform);
            }

            this.gameCamera = Camera.main;
            this.bounds = new List<CheckBoundState>(2);
            this.wrongBounds = new List<CheckBoundState>(2);
            Path = new Vector3[this.pointRoot.childCount];

            for (int i = 0; i < this.pointRoot.childCount; i++)
            {
                var point = this.pointRoot.GetChild(i);
                Path[i] = point.position;
            }

            if (this.wrongBoxSize == Vector3.zero)
            {
                this.wrongBoxSize = this.checkBoxSize;
            }
        }

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            this.eraser.Init();
            this.eraser.SetBrushSize(brushRadius);
            this.eraser.CopyCamera(this.gameCamera);
            this.eraser.ScaleMask();
            CreateDetectBoxes(this.pointRoot, this.bounds, this.checkBoxSize);
            CreateDetectBoxes(this.wrongPointRoot, this.wrongBounds, this.wrongBoxSize);

            LeanTouch.OnFingerDown += FingerDownHandler;
            LeanTouch.OnFingerUp += FingerUpHandler;
            LeanTouch.OnFingerUpdate += FingerUpdateHandler;

            try
            {
                await UniTask.WaitUntil(() => this.completeAfterFingerUp, PlayerLoopTiming.Update, cancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                if (this.autoClearBrush)
                {
                    if (this.eraser != null)
                    {
                        this.eraser.Clear();
                    }
                }

                LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
                LeanTouch.OnFingerDown -= FingerDownHandler;
                LeanTouch.OnFingerUp -= FingerUpHandler;
            }
        }

        private void FingerDownHandler(LeanFinger finger)
        {
            if (this.completeAfterFingerUp) return;
            this.fingerDown = true;
            Vector3 pos = this.eraser.MaskCamera.ScreenToWorldPoint(finger.ScreenPosition);
            var brushUIPos = new Vector3(pos.x, pos.y, 0.0f);
            this.eraserBrush.Transform.position = brushUIPos;
            this.eraser.SetActiveBrush(true);
            this.eraserBrush.SetActive(true);
        }

        private void FingerUpHandler(LeanFinger finger)
        {
            if (this.completeAfterFingerUp) return;
            this.fingerDown = false;
            this.eraser.SetActiveBrush(false);
            this.eraserBrush.SetActive(false);
            this.overWrongPoint = false;

            if (this.done)
            {
                this.completeAfterFingerUp = true;
            }
            else
            {
                if (this.clearEraseOnFingerUp)
                {
                    this.eraser.Clear();
                }
            }
        }

        private bool overWrongPoint;
        private Vector3 brushPos;
        private Vector3 eraseCenter;

        // private void Update()
        // {
        //     if (this.eraserBrush != null)
        //     {
        //         this.eraserBrush.Transform.position =
        //             Vector3.MoveTowards(this.eraserBrush.Transform.position, this.brushPos, Time.deltaTime * 40f);
        //     }
        //
        //     if (this.eraser != null)
        //     {
        //     }
        // }

        private void FingerUpdateHandler(LeanFinger finger)
        {
            if (!this.fingerDown) return;
            Vector3 fingerWorldPos = this.eraser.MaskCamera.ScreenToWorldPoint(finger.ScreenPosition);
            this.eraseCenter = fingerWorldPos + this.eraseAreaCenterOffset;
            this.eraseCenter.z += 1f;

            this.brushPos = new Vector3(fingerWorldPos.x, fingerWorldPos.y) + this.brushHeaderOffset;
            this.eraseCenter.z = 0f;

            this.eraser.SetBrushPosition(this.eraseCenter);
            // this.eraserBrush.Transform.position =
            //     Vector3.MoveTowards(this.eraserBrush.Transform.position, this.brushPos, Time.deltaTime * 40f);

            this.eraserBrush.Transform.position =
                Vector3.SmoothDamp(this.eraserBrush.Transform.position, this.brushPos, ref this.velocity, 0.15f);

            if (this.overWrongPoint) return;
            foreach (CheckBoundState bound in this.wrongBounds)
            {
                if (bound.Bounds.Contains(this.eraseCenter))
                {
                    this.done = false;
                    this.overWrongPoint = true;
                    return;
                }
            }

            foreach (CheckBoundState bound in this.bounds)
            {
                if (bound.Done) continue;
                bound.Done = bound.Bounds.Contains(this.eraseCenter);
            }

            if (!this.done)
            {
                this.done = CheckComplete();
            }
        }

        private bool CheckComplete()
        {
            foreach (CheckBoundState bound in this.bounds)
            {
                if (!bound.Done)
                {
                    return false;
                }
            }

            return true;
        }

        private void CreateDetectBoxes(Transform root, List<CheckBoundState> boundStates, Vector3 boxSize)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform pointTrans = root.GetChild(i);
                var bound = new Bounds(pointTrans.position, boxSize);
                var boundState = new CheckBoundState(bound);
                boundStates.Add(boundState);
            }
        }


// #if UNITY_EDITOR
//         private void OnDrawGizmos()
//         {
//             if (this.pointRoot != null)
//             {
//                 for (int i = 0; i < this.pointRoot.childCount; i++)
//                 {
//                     Transform pointTrans = this.pointRoot.GetChild(i);
//                     var bound = new Bounds(pointTrans.position, this.checkBoxSize);
//                     DebugExtension.DrawBounds(bound, Color.green);
//                 }
//             }
//
//             if (this.wrongPointRoot != null)
//             {
//                 for (int i = 0; i < this.wrongPointRoot.childCount; i++)
//                 {
//                     Transform pointTrans = this.wrongPointRoot.GetChild(i);
//                     var bound = new Bounds(pointTrans.position, this.wrongBoxSize);
//                     DebugExtension.DrawBounds(bound, Color.red);
//                 }
//             }
//         }
// #endif
    }
}