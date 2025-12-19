using Mimi.EffectMaker.Core;
using Mimi.EffectMaker.Core.Decorators;
using Mimi.SpawnService;
using UnityEngine;

namespace Mimi.EffectMaker.Vfx
{
    public class DespawnWhenEffectEndDecorator : BaseEffectMakerDecorator
    {
        private IPoolService poolingService;
        private Component targetComponent;
        public DespawnWhenEffectEndDecorator(IEffectMaker wrapEffectMaker, IPoolService poolingService, Component targetComponent) : base(wrapEffectMaker)
        {
            this.poolingService = poolingService;
            this.targetComponent = targetComponent;
        }

        public override void End()
        {
            base.End();
            poolingService.Despawn(targetComponent);
        }

        public override bool Cancel()
        {
            if (!base.Cancel()) return false;
            poolingService.Despawn(targetComponent);
            return true;
        }
    }
}