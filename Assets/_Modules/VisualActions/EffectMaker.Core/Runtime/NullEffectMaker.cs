

namespace Mimi.EffectMaker.Core
{
    public class NullEffectMaker: BaseEffectMaker
    {
        public override void Initialize()
        {
            
        }

        public override void StartEffect()
        {
            base.StartEffect();
            /*int i = 0;
            for (int index = 0; index < 100; index++)
            {
                i += RandomUtils.RangeInt(1, 10);
            }
            End();*/
        }
    }
}