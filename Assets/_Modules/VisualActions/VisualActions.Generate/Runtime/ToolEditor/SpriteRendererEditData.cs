using System;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Sprites.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace  Mimi.VisualActions.Generate
{
    [Serializable]
    public class SpriteRendererEditData
    {
        [PreviewField]
        [OnValueChanged("OnSpriteChange")]
        public Sprite sprite;
        [OnValueChanged("OnSortingOrderChanged")]
        public int sortingOrder;
        [SortingLayerPopup]
        [OnValueChanged("OnSortingLayerNameChanged")]
        public string sortingLayerName;

        [SerializeField,HideInInspector]private SpriteRenderer spriteRenderer;
        public void InitData(SpriteRenderer spriteRenderer)
        {
            this.spriteRenderer = spriteRenderer;
            this.sprite = spriteRenderer.sprite;
            this.sortingOrder = spriteRenderer.sortingOrder;
            this.sortingLayerName = spriteRenderer.sortingLayerName;
        }

        public void OnSpriteChange(Sprite sprite)
        {
            if(this.spriteRenderer != null)
                this.spriteRenderer.sprite = sprite;
        }

        public void OnSortingOrderChanged(int newSortingOrder)
        {
            if(this.spriteRenderer != null)
                this.spriteRenderer.sortingOrder = newSortingOrder;
        }

        public void OnSortingLayerNameChanged(string newSortingLayerName)
        {
            if(this.spriteRenderer != null)
                this.spriteRenderer.sortingLayerName = newSortingLayerName;
        }
    }
}