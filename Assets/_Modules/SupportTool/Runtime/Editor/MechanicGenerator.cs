using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Spines;
using Spine.Unity;
using UnityEngine;
using VisualActions.Areas;
using VisualActions.VisualActions.GameObjects.Runtime;

public class MechanicGenerator
{
    protected void AddAutoRenameComponent(GameObject gameObject, GameObject targetObject, string prefix, string removeString)
    {
        var boxAutoRename = gameObject.gameObject.AddComponent<AutoRenameFollow>();
        boxAutoRename.SetField("target", targetObject, AccessModifier.Private);
        boxAutoRename.SetField("prefix", prefix, AccessModifier.Private);
        boxAutoRename.SetField("removeString", removeString, AccessModifier.Private);
    }
    
    protected BoxArea CreateBoxArea()
    {
        var boxAreaObject = new GameObject();
        var boxArea2D = boxAreaObject.AddComponent<BoxArea>();
        boxArea2D.transform.localPosition = Vector3.zero;
        boxArea2D.SetField("boxCollider", boxArea2D.GetComponent<BoxCollider2D>(), AccessModifier.Private);

        return boxArea2D;
    }

    protected void HandleAnimInMechanic(GameObject blueprintObject, GameObject target, SkeletonAnimation skeletonAnimation, string suffix)
    {
        if (skeletonAnimation == null)
        {
            Debug.Log($"There is no skeleton animation in this scene!");
        }

        WaitSpineAnim[] waitSpineAnims = blueprintObject.GetComponentsInChildren<WaitSpineAnim>();
        for (var i = 0; i < waitSpineAnims.Length; i++)
        {
            WaitSpineAnim waitAnim = waitSpineAnims[i];
            if (skeletonAnimation != null)
            {
                waitAnim.SetField("skeletonAnimation", skeletonAnimation, AccessModifier.Private);
            }

            waitAnim.name = $"{waitAnim.name}_{i + 1}";
            AddAutoRenameComponent(waitAnim.gameObject, target, $"{waitAnim.name}", suffix);
        }

        PlaySpineAnim[] playSpineAnims = blueprintObject.GetComponentsInChildren<PlaySpineAnim>();
        for (var i = 0; i < playSpineAnims.Length; i++)
        {
            PlaySpineAnim playAnim = playSpineAnims[i];
            if (skeletonAnimation != null)
            {
                playAnim.SetField("skeletonAnimation", skeletonAnimation, AccessModifier.Private);
            }

            playAnim.name = $"{playAnim.name}_{i + 1}";
            AddAutoRenameComponent(playAnim.gameObject, target, $"{playAnim.name}", suffix);
        }
    }
    
    protected void HandleSetActiveCommandInMechanic(GameObject blueprintObject, GameObject[] gameObjects)
    {
        SetActiveMultipleGameObjectsAction[] setActiveCommand = blueprintObject.GetComponentsInChildren<SetActiveMultipleGameObjectsAction>();
        foreach (SetActiveMultipleGameObjectsAction setActive in setActiveCommand)
        {
            setActive.SetField("gameObjects", gameObjects, AccessModifier.Private);
        }
    }
}