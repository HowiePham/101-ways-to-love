using UnityEngine;

namespace Mimi.Actor.Graphic.Core
{
    public class MonoCompositeGraphic : BaseMonoGraphic
    {
        [SerializeField] private BaseMonoGraphic[] graphics;
        protected override IGraphic CreateGraphic()
        {
            return new CompositeGraphic(graphics);
        }

        public void SetGraphics(BaseMonoGraphic[] graphics)
        {
            this.graphics = graphics;
            (this.WrapperGraphic as CompositeGraphic).SetGraphics(graphics);
        }

        public BaseMonoGraphic[]  GetGraphics()
        {
            return this.graphics;
        }
    }
}