using System;
using UnityEngine;
using UnityEngine.UI;

namespace VisualFlow
{
    public class Eraser : MonoBehaviour
    {
        [SerializeField] private ClearFlagsImageEffect clearFlagsImageEffect;
        [SerializeField] private RawImage overlayImage;
        [SerializeField] private Camera drawingCamera;
        [SerializeField] private GameObject brushAreaRoot;
        [SerializeField] private MeshRenderer maskRenderer;
        [SerializeField] private Transform quadTrans;

        private Transform brushTrans;

        public Camera MaskCamera => this.drawingCamera;

        public void Init()
        {
            this.brushTrans = this.brushAreaRoot.transform;
            this.drawingCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
            this.overlayImage.texture = this.drawingCamera.targetTexture;
            this.maskRenderer.sharedMaterial.mainTexture = this.overlayImage.texture;
        }

        public void ScaleMask()
        {
            float quadHeight = this.drawingCamera.orthographicSize * 2f;
            float quadWidth = quadHeight * Screen.width / Screen.height;
            this.quadTrans.localScale = new Vector3(quadWidth, quadHeight);
        }

        public void SetBrushSize(float size)
        {
            this.brushTrans.localScale = size * Vector3.one;
        }

        public void CopyCamera(Camera camera)
        {
            this.drawingCamera.orthographic = camera.orthographic;
            this.drawingCamera.orthographicSize = camera.orthographicSize;
            this.drawingCamera.rect = camera.rect;
            this.drawingCamera.transform.position = camera.transform.position;
        }

        public void SetActiveBrush(bool active)
        {
            if (this.brushAreaRoot != null)
                this.brushAreaRoot.SetActive(active);
        }

        public void SetBrushPosition(Vector3 pos)
        {
            this.brushTrans.position = pos;
        }

        public void Clear()
        {
            this.clearFlagsImageEffect.ResetTexture();
            this.drawingCamera.enabled = false;
            this.drawingCamera.targetTexture.Release();
            this.drawingCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
            this.overlayImage.texture = this.drawingCamera.targetTexture;
            this.maskRenderer.sharedMaterial.mainTexture = this.overlayImage.texture;
            this.drawingCamera.enabled = true;
        }
    }
}