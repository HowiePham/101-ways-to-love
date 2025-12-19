using Mimi.EffectMaker.Core;
using Mimi.EffectMaker.Core.Decorators;
using Mimi.SpawnService;
using UnityEngine;

namespace Mimi.EffectMaker.Vfx
{
    public class MonoDespawnWhenEffectEndDecorator : MonoEffectMaker
    {
        [SerializeField] private MonoEffectMaker effectMaker;
        [SerializeField] private Component targetComponent;
        [SerializeField] private BasePoolServiceSO poolingService;
        protected override IEffectMaker CreateEffectMaker()
        {
            return new DespawnWhenEffectEndDecorator(effectMaker, poolingService, targetComponent);
        }
    }
}