using UnityEngine;

public class SetActiveMechanicGenerator : MechanicGenerator
{
    private string animationMechanicBlueprintAddress = $"{MechanicBlueprintAddress}/SetActive/";

    public void CreateMechanic(string menuName, string objectName)
    {
        GameObject mechanicObject = CreateMechanicBlueprint(this.animationMechanicBlueprintAddress, menuName);
        mechanicObject.name = objectName;
    }
}