using Mimi.VisualActions;
using UnityEngine;

public class TrueAction : MonoBehaviour
{
    [SerializeField] private VisualAction actionCondition;
    [SerializeField] private MechanicType mechanicType;

    public VisualAction ActionCondition => this.actionCondition;

    public MechanicType MechanicType => this.mechanicType;
}