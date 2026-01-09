using Spine.Unity;
using UnityEngine;

public class MixingMechanicGenerator : MechanicGenerator
{
    private const string MixMechanicBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/MixMechanic/";
    
    private TapMechanicGenerator tapMechanicGenerator;
    private DragMechanicGenerator dragMechanicGenerator;
    
    public void CreateMechanic(string menuName, string objectName, SkeletonAnimation skeletonAnimation, GameObject interactableObjectParent, GameObject boxInteractionParent)
    {
        if (menuName.Contains("Tap") && menuName.Contains("Drag"))
        {
            this.tapMechanicGenerator = new TapMechanicGenerator();
            this.dragMechanicGenerator = new DragMechanicGenerator();
        }
    }

    private void CreateTapDragMechanic(string objectName)
    {
        
    }
}