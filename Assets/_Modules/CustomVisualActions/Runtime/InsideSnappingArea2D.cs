using Lean.Common;
using Mimi.VisualActions.Dragging;

public class InsideSnappingArea2D : InsideArea2D
{
    private BoxSnapping boxSnapping;
    private LeanSelectable leanSelectable;

    private void Start()
    {
        this.boxSnapping = this.targetArea.GetComponent<BoxSnapping>();
        this.leanSelectable = this.checkTransform.GetComponent<LeanSelectable>();
    }

    private void Update()
    {
        if (this.boxSnapping == null || this.leanSelectable == null || !this.leanSelectable.IsSelected)
        {
            return;
        }

        if (Validate())
        {
            this.boxSnapping.ShowCheckingEffect();
        }
        else
        {
            this.boxSnapping.HideCheckingEffect();
        }
    }
}