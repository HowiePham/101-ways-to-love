using UnityEngine;

public class AnimationMechanicGenerator : MechanicGenerator
{
    private string animationMechanicBlueprintAddress = $"{MechanicBlueprintAddress}/AnimationMechanic/";

    public void CreateMechanic(string menuName, string objectName)
    {
        GameObject mechanicObject = CreateMechanicBlueprint(this.animationMechanicBlueprintAddress, menuName);
        mechanicObject.name = objectName;
    }
}