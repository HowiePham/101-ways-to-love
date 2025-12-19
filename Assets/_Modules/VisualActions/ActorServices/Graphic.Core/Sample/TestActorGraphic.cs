using Mimi.Actor.Graphic.Core;
using Mimi.VisualActions.Attribute;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestActorGraphic : MonoBehaviour
{
    [SerializeField] private BaseMonoGraphic targetGraphic;
    [SerializeField] private MonoGraphicAsset graphicSpriteAsset;
    [SerializeField] private int sortingOrder;
    [SerializeField,SortingLayerPopup] private string sortingLayerName;

    [Button]
    public void SetAssetGraphic()
    {
        targetGraphic.SetAssetGraphic(graphicSpriteAsset);
       
    }

    [Button]
    public void SetSortingOrder()
    {
        targetGraphic.SetSortingOrder(sortingOrder);
        
    }

    [Button]
    public void SetSortingLayerName()
    {
        targetGraphic.SetSortingLayerName(sortingLayerName);
    }
}
