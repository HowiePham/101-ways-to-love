using UnityEngine;

public class AudioMechanicGenerator : MechanicGenerator
{
    private string animationMechanicBlueprintAddress = $"{MechanicBlueprintAddress}/AudioMechanic/";

    public void CreateMechanic(string menuName, string objectName)
    {
        GameObject mechanicObject = CreateMechanicBlueprint(this.animationMechanicBlueprintAddress, menuName);
        mechanicObject.name = objectName;
    }
}