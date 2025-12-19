//The implementation is based on this article:http://rbarraza.com/html5-canvas-pageflip/
//As the rbarraza.com website is not live anymore you can get an archived version from web archive 
//or check an archived version that I uploaded on my website: https://dandarawy.com/html5-canvas-pageflip/

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Mimi.VisualActions.BookFlip
{
    public enum FlipMode
    {
        RightToLeft,
        LeftToRight
    }

    [ExecuteInEditMode]
    public class Book : MonoBehaviour
    {
        public float skipPercentage = 0f;
        public Canvas canvas;
        [SerializeField] private RectTransform BookPanel;
        public Sprite background;
        public Sprite[] bookPages;
        public bool interactable = true;

        public bool enableShadowEffect = true;

        //represent the index of the sprite shown in the right page
        public int currentPage = 0;

        public int TotalPageCount
        {
            get { return this.bookPages.Length; }
        }

        public Vector3 EndBottomLeft
        {
            get { return this.edgeBottomLeft; }
        }

        public Vector3 EndBottomRight
        {
            get { return this.edgeBottomRight; }
        }

        public float Height
        {
            get { return this.BookPanel.rect.height; }
        }

        public Image ClippingPlane;
        public Image NextPageClip;
        public Image Shadow;
        public Image ShadowLTR;
        public Image Left;
        public Image LeftNext;
        public Image Right;
        public Image RightNext;
        public UnityEvent OnFlip;
        public event Action OnFlipCompleted;
        public event Action OnFlipStarted;

        public event Action OnPageRelease;

        private float radius1, radius2;

        //Spine Bottom
        private Vector3 spineBottom;

        //Spine Top
        private Vector3 spineTop;

        //corner of the page
        private Vector3 pageCorner;

        //Edge Bottom Right
        private Vector3 edgeBottomRight;

        //Edge Bottom Left
        private Vector3 edgeBottomLeft;

        //follow point 
        private Vector3 followPoint;

        private bool pageDragging = false;

        //current flip mode
        private FlipMode mode;

        private void Start()
        {
            if (!this.canvas) this.canvas = GetComponentInParent<Canvas>();
            if (!this.canvas) Debug.LogError("Book should be a child to canvas");

            this.Left.gameObject.SetActive(false);
            this.Right.gameObject.SetActive(false);
            UpdateSprites();
            CalcCurlCriticalPoints();

            float pageWidth = this.BookPanel.rect.width / 2.0f;
            float pageHeight = this.BookPanel.rect.height;
            this.NextPageClip.rectTransform.sizeDelta = new Vector2(pageWidth, pageHeight + pageHeight * 2);


            this.ClippingPlane.rectTransform.sizeDelta =
                new Vector2(pageWidth * 2 + pageHeight, pageHeight + pageHeight * 2);

            //hypotenous (diagonal) page length
            float hyp = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight);
            float shadowPageHeight = pageWidth / 2 + hyp;

            this.Shadow.rectTransform.sizeDelta = new Vector2(pageWidth, shadowPageHeight);
            this.Shadow.rectTransform.pivot = new Vector2(1, (pageWidth / 2) / shadowPageHeight);

            this.ShadowLTR.rectTransform.sizeDelta = new Vector2(pageWidth, shadowPageHeight);
            this.ShadowLTR.rectTransform.pivot = new Vector2(0, (pageWidth / 2) / shadowPageHeight);
        }

        private void CalcCurlCriticalPoints()
        {
            this.spineBottom = new Vector3(0, -this.BookPanel.rect.height / 2);
            this.edgeBottomRight = new Vector3(this.BookPanel.rect.width / 2, -this.BookPanel.rect.height / 2);
            this.edgeBottomLeft = new Vector3(-this.BookPanel.rect.width / 2, -this.BookPanel.rect.height / 2);
            this.spineTop = new Vector3(0, this.BookPanel.rect.height / 2);
            this.radius1 = Vector2.Distance(this.spineBottom, this.edgeBottomRight);
            float pageWidth = this.BookPanel.rect.width / 2.0f;
            float pageHeight = this.BookPanel.rect.height;
            this.radius2 = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight);
        }

        public Vector3 transformPoint(Vector3 mouseScreenPos)
        {
            if (this.canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector3 mouseWorldPos =
                    this.canvas.worldCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y,
                        this.canvas.planeDistance));
                Vector2 localPos = this.BookPanel.InverseTransformPoint(mouseWorldPos);

                return localPos;
            }
            else if (this.canvas.renderMode == RenderMode.WorldSpace)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 globalEBR = transform.TransformPoint(this.edgeBottomRight);
                Vector3 globalEBL = transform.TransformPoint(this.edgeBottomLeft);
                Vector3 globalSt = transform.TransformPoint(this.spineTop);
                Plane p = new Plane(globalEBR, globalEBL, globalSt);
                float distance;
                p.Raycast(ray, out distance);
                Vector2 localPos = this.BookPanel.InverseTransformPoint(ray.GetPoint(distance));
                return localPos;
            }
            else
            {
                //Screen Space Overlay
                Vector2 localPos = this.BookPanel.InverseTransformPoint(mouseScreenPos);
                return localPos;
            }
        }

        private void Update()
        {
            if (this.pageDragging && this.interactable)
            {
                UpdateBook();
            }
        }

        public void UpdateBook()
        {
            this.followPoint = Vector3.Lerp(this.followPoint, transformPoint(Input.mousePosition), Time.deltaTime * 10);
            if (this.mode == FlipMode.RightToLeft)
                UpdateBookRTLToPoint(this.followPoint);
            else
                UpdateBookLTRToPoint(this.followPoint);
        }

        public void UpdateBookLTRToPoint(Vector3 followLocation)
        {
            this.mode = FlipMode.LeftToRight;
            this.followPoint = followLocation;
            this.ShadowLTR.transform.SetParent(this.ClippingPlane.transform, true);
            this.ShadowLTR.transform.localPosition = new Vector3(0, 0, 0);
            this.ShadowLTR.transform.localEulerAngles = new Vector3(0, 0, 0);
            this.Left.transform.SetParent(this.ClippingPlane.transform, true);

            this.Right.transform.SetParent(this.BookPanel.transform, true);
            this.Right.transform.localEulerAngles = Vector3.zero;
            this.LeftNext.transform.SetParent(this.BookPanel.transform, true);

            this.pageCorner = CalculateCornerPosition(followLocation);
            Vector3 t1;
            float clipAngle = CalculateClipAngle(this.pageCorner, this.edgeBottomLeft, out t1);
            //0 < T0_T1_Angle < 180
            clipAngle = (clipAngle + 180) % 180;

            this.ClippingPlane.transform.localEulerAngles = new Vector3(0, 0, clipAngle - 90);
            this.ClippingPlane.transform.position = this.BookPanel.TransformPoint(t1);

            //page position and angle
            this.Left.transform.position = this.BookPanel.TransformPoint(this.pageCorner);
            float C_T1_dy = t1.y - this.pageCorner.y;
            float C_T1_dx = t1.x - this.pageCorner.x;
            float C_T1_Angle = Mathf.Atan2(C_T1_dy, C_T1_dx) * Mathf.Rad2Deg;
            this.Left.transform.localEulerAngles = new Vector3(0, 0, C_T1_Angle - 90 - clipAngle);

            this.NextPageClip.transform.localEulerAngles = new Vector3(0, 0, clipAngle - 90);
            this.NextPageClip.transform.position = this.BookPanel.TransformPoint(t1);
            this.LeftNext.transform.SetParent(this.NextPageClip.transform, true);
            this.Right.transform.SetParent(this.ClippingPlane.transform, true);
            this.Right.transform.SetAsFirstSibling();

            this.ShadowLTR.rectTransform.SetParent(this.Left.rectTransform, true);
        }

        public void UpdateBookRTLToPoint(Vector3 followLocation)
        {
            this.mode = FlipMode.RightToLeft;
            this.followPoint = followLocation;
            this.Shadow.transform.SetParent(this.ClippingPlane.transform, true);
            this.Shadow.transform.localPosition = Vector3.zero;
            this.Shadow.transform.localEulerAngles = Vector3.zero;
            this.Right.transform.SetParent(this.ClippingPlane.transform, true);

            this.Left.transform.SetParent(this.BookPanel.transform, true);
            this.Left.transform.localEulerAngles = Vector3.zero;
            this.RightNext.transform.SetParent(this.BookPanel.transform, true);
            this.pageCorner = CalculateCornerPosition(followLocation);
            Vector3 t1;
            float clipAngle = CalculateClipAngle(this.pageCorner, this.edgeBottomRight, out t1);
            if (clipAngle > -90) clipAngle += 180;

            this.ClippingPlane.rectTransform.pivot = new Vector2(1, 0.35f);
            this.ClippingPlane.transform.localEulerAngles = new Vector3(0, 0, clipAngle + 90);
            this.ClippingPlane.transform.position = this.BookPanel.TransformPoint(t1);

            //page position and angle
            this.Right.transform.position = this.BookPanel.TransformPoint(this.pageCorner);
            float C_T1_dy = t1.y - this.pageCorner.y;
            float C_T1_dx = t1.x - this.pageCorner.x;
            float C_T1_Angle = Mathf.Atan2(C_T1_dy, C_T1_dx) * Mathf.Rad2Deg;
            this.Right.transform.localEulerAngles = new Vector3(0, 0, C_T1_Angle - (clipAngle + 90));

            this.NextPageClip.transform.localEulerAngles = new Vector3(0, 0, clipAngle + 90);
            this.NextPageClip.transform.position = this.BookPanel.TransformPoint(t1);
            this.RightNext.transform.SetParent(this.NextPageClip.transform, true);
            this.Left.transform.SetParent(this.ClippingPlane.transform, true);
            this.Left.transform.SetAsFirstSibling();

            this.Shadow.rectTransform.SetParent(this.Right.rectTransform, true);
        }

        private float CalculateClipAngle(Vector3 c, Vector3 bookCorner, out Vector3 t1)
        {
            Vector3 t0 = (c + bookCorner) / 2;
            float T0_CORNER_dy = bookCorner.y - t0.y;
            float T0_CORNER_dx = bookCorner.x - t0.x;
            float T0_CORNER_Angle = Mathf.Atan2(T0_CORNER_dy, T0_CORNER_dx);
            float T0_T1_Angle = 90 - T0_CORNER_Angle;

            float T1_X = t0.x - T0_CORNER_dy * Mathf.Tan(T0_CORNER_Angle);
            T1_X = normalizeT1X(T1_X, bookCorner, this.spineBottom);
            t1 = new Vector3(T1_X, this.spineBottom.y, 0);

            //clipping plane angle=T0_T1_Angle
            float T0_T1_dy = t1.y - t0.y;
            float T0_T1_dx = t1.x - t0.x;
            T0_T1_Angle = Mathf.Atan2(T0_T1_dy, T0_T1_dx) * Mathf.Rad2Deg;
            return T0_T1_Angle;
        }

        private float normalizeT1X(float t1, Vector3 corner, Vector3 sb)
        {
            if (t1 > sb.x && sb.x > corner.x)
                return sb.x;
            if (t1 < sb.x && sb.x < corner.x)
                return sb.x;
            return t1;
        }

        private Vector3 CalculateCornerPosition(Vector3 followLocation)
        {
            Vector3 c;
            this.followPoint = followLocation;
            float F_SB_dy = this.followPoint.y - this.spineBottom.y;
            float F_SB_dx = this.followPoint.x - this.spineBottom.x;
            float F_SB_Angle = Mathf.Atan2(F_SB_dy, F_SB_dx);
            Vector3 r1 = new Vector3(this.radius1 * Mathf.Cos(F_SB_Angle), this.radius1 * Mathf.Sin(F_SB_Angle), 0) +
                         this.spineBottom;

            float F_SB_distance = Vector2.Distance(this.followPoint, this.spineBottom);
            if (F_SB_distance < this.radius1)
                c = this.followPoint;
            else
                c = r1;
            float F_ST_dy = c.y - this.spineTop.y;
            float F_ST_dx = c.x - this.spineTop.x;
            float F_ST_Angle = Mathf.Atan2(F_ST_dy, F_ST_dx);
            Vector3 r2 = new Vector3(this.radius2 * Mathf.Cos(F_ST_Angle), this.radius2 * Mathf.Sin(F_ST_Angle), 0) +
                         this.spineTop;
            float C_ST_distance = Vector2.Distance(c, this.spineTop);
            if (C_ST_distance > this.radius2)
                c = r2;
            return c;
        }

        public void DragRightPageToPoint(Vector3 point)
        {
            if (this.currentPage >= this.bookPages.Length) return;
            this.pageDragging = true;
            this.mode = FlipMode.RightToLeft;
            this.followPoint = point;


            this.NextPageClip.rectTransform.pivot = new Vector2(0, 0.12f);
            this.ClippingPlane.rectTransform.pivot = new Vector2(1, 0.35f);

            this.Left.gameObject.SetActive(true);
            this.Left.rectTransform.pivot = new Vector2(0, 0);
            this.Left.transform.position = this.RightNext.transform.position;
            this.Left.transform.eulerAngles = new Vector3(0, 0, 0);
            this.Left.sprite = (this.currentPage < this.bookPages.Length)
                ? this.bookPages[this.currentPage]
                : this.background;
            this.Left.transform.SetAsFirstSibling();

            this.Right.gameObject.SetActive(true);
            this.Right.transform.position = this.RightNext.transform.position;
            this.Right.transform.eulerAngles = new Vector3(0, 0, 0);
            this.Right.sprite = (this.currentPage < this.bookPages.Length - 1)
                ? this.bookPages[this.currentPage + 1]
                : this.background;

            this.RightNext.sprite = (this.currentPage < this.bookPages.Length - 2)
                ? this.bookPages[this.currentPage + 2]
                : this.background;

            this.LeftNext.transform.SetAsFirstSibling();
            if (this.enableShadowEffect) this.Shadow.gameObject.SetActive(true);
            UpdateBookRTLToPoint(this.followPoint);
        }

        public void OnMouseDragRightPage()
        {
            if (this.interactable)
                DragRightPageToPoint(transformPoint(Input.mousePosition));
        }

        public void DragLeftPageToPoint(Vector3 point)
        {
            if (this.currentPage <= 0) return;
            this.pageDragging = true;
            OnFlipStarted?.Invoke();
            this.mode = FlipMode.LeftToRight;
            this.followPoint = point;

            this.NextPageClip.rectTransform.pivot = new Vector2(1, 0.12f);
            this.ClippingPlane.rectTransform.pivot = new Vector2(0, 0.35f);

            this.Right.gameObject.SetActive(true);
            this.Right.transform.position = this.LeftNext.transform.position;
            this.Right.sprite = this.bookPages[this.currentPage - 1];
            this.Right.transform.eulerAngles = new Vector3(0, 0, 0);
            this.Right.transform.SetAsFirstSibling();

            this.Left.gameObject.SetActive(true);
            this.Left.rectTransform.pivot = new Vector2(1, 0);
            this.Left.transform.position = this.LeftNext.transform.position;
            this.Left.transform.eulerAngles = new Vector3(0, 0, 0);
            this.Left.sprite = (this.currentPage >= 2) ? this.bookPages[this.currentPage - 2] : this.background;

            this.LeftNext.sprite = (this.currentPage >= 3) ? this.bookPages[this.currentPage - 3] : this.background;

            this.RightNext.transform.SetAsFirstSibling();
            if (this.enableShadowEffect) this.ShadowLTR.gameObject.SetActive(true);
            UpdateBookLTRToPoint(this.followPoint);
        }

        public void OnMouseDragLeftPage()
        {
            if (this.interactable)
                DragLeftPageToPoint(transformPoint(Input.mousePosition));
        }

        public void OnMouseRelease()
        {
            if (this.interactable)
                ReleasePage();
        }

        public void ReleasePage()
        {
            if (this.pageDragging)
            {
                this.pageDragging = false;
                OnPageRelease?.Invoke();
                float distanceToLeft = Vector2.Distance(this.pageCorner, this.edgeBottomLeft);
                float distanceToRight = Vector2.Distance(this.pageCorner, this.edgeBottomRight);
                if (distanceToRight < distanceToLeft && this.mode == FlipMode.RightToLeft)
                    TweenBack();
                else if (distanceToRight > distanceToLeft && this.mode == FlipMode.LeftToRight)
                    TweenBack();
                else
                    TweenForward();
            }
        }

        private Coroutine currentCoroutine;

        private void UpdateSprites()
        {
            this.LeftNext.sprite = (this.currentPage > 0 && this.currentPage <= this.bookPages.Length)
                ? this.bookPages[this.currentPage - 1]
                : this.background;
            this.RightNext.sprite = (this.currentPage >= 0 && this.currentPage < this.bookPages.Length)
                ? this.bookPages[this.currentPage]
                : this.background;
        }

        public void TweenForward()
        {
            if (this.mode == FlipMode.RightToLeft)
                this.currentCoroutine = StartCoroutine(TweenTo(this.edgeBottomLeft, 0.15f, Flip));
            else
                this.currentCoroutine = StartCoroutine(TweenTo(this.edgeBottomRight, 0.15f, Flip));
        }

        private void Flip()
        {
            if (this.mode == FlipMode.RightToLeft)
                this.currentPage += 2;
            else
                this.currentPage -= 2;
            this.LeftNext.transform.SetParent(this.BookPanel.transform, true);
            this.Left.transform.SetParent(this.BookPanel.transform, true);
            this.LeftNext.transform.SetParent(this.BookPanel.transform, true);
            this.Left.gameObject.SetActive(false);
            this.Right.gameObject.SetActive(false);
            this.Right.transform.SetParent(this.BookPanel.transform, true);
            this.RightNext.transform.SetParent(this.BookPanel.transform, true);
            UpdateSprites();
            this.Shadow.gameObject.SetActive(false);
            this.ShadowLTR.gameObject.SetActive(false);
            this.OnFlip?.Invoke();
            OnFlipCompleted?.Invoke();
        }

        public void TweenBack()
        {
            if (this.mode == FlipMode.RightToLeft)
            {
                this.currentCoroutine = StartCoroutine(TweenTo(this.edgeBottomRight, 0.15f,
                    () =>
                    {
                        UpdateSprites();
                        this.RightNext.transform.SetParent(this.BookPanel.transform);
                        this.Right.transform.SetParent(this.BookPanel.transform);

                        this.Left.gameObject.SetActive(false);
                        this.Right.gameObject.SetActive(false);
                        this.pageDragging = false;
                    }
                ));
            }
            else
            {
                this.currentCoroutine = StartCoroutine(TweenTo(this.edgeBottomLeft, 0.15f,
                    () =>
                    {
                        UpdateSprites();

                        this.LeftNext.transform.SetParent(this.BookPanel.transform);
                        this.Left.transform.SetParent(this.BookPanel.transform);

                        this.Left.gameObject.SetActive(false);
                        this.Right.gameObject.SetActive(false);
                        this.pageDragging = false;
                    }
                ));
            }
        }

        public IEnumerator TweenTo(Vector3 to, float duration, System.Action onFinish)
        {
            int steps = (int)(duration / 0.025f);
            Vector3 displacement = (to - this.followPoint) / steps;
            int startStep = Mathf.RoundToInt(steps * 1f * skipPercentage);
            for (int i = 0; i < steps - 1; i++)
            {
                if (this.mode == FlipMode.RightToLeft)
                    UpdateBookRTLToPoint(this.followPoint + displacement);
                else
                    UpdateBookLTRToPoint(this.followPoint + displacement);

                if (i >= startStep)
                {
                    yield return new WaitForSeconds(0.025f);
                }
            }

            onFinish?.Invoke();
        }
    }
}