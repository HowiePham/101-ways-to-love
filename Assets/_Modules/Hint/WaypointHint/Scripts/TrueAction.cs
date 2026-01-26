using Mimi.Prototypes.Events;
using Mimi.VisualActions;
using UnityEngine;

public class TrueAction : MonoBehaviour
{
    [SerializeField] private VisualAction actionCondition;
    [SerializeField] private MechanicType mechanicType;
    private bool isDone;

    public VisualAction ActionCondition => this.actionCondition;
    public MechanicType MechanicType => this.mechanicType;

    private void Awake()
    {
        this.isDone = false;
    }

    private void Update()
    {
        if (!this.actionCondition.Completed || this.isDone)
        {
            return;
        }

        Messenger.Broadcast(EventKey.ActionDone);
        this.isDone = true;
    }
}