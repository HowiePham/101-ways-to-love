using Mimi.EffectMaker.Core;
using UnityEngine;

namespace Mimi.EffectMaker.Vfx
{
    public class MonoParticleVfxEffectMaker : MonoEffectMaker
    {
        [SerializeField] private ParticleSystem particleSystem;
        protected override IEffectMaker CreateEffectMaker()
        {
            return new ParticleVfxEffectMaker(particleSystem);
        }
    }
}