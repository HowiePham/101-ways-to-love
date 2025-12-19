using System;
using System.Collections.Generic;
using MEC;
using Mimi.EffectMaker.Core;
using UnityEngine;

namespace Mimi.EffectMaker.Vfx
{
    public class ParticleVfxEffectMaker : BaseEffectMaker
    {
        private ParticleSystem particleSystem;
        private CoroutineHandle _coroutineHandle;

        public ParticleVfxEffectMaker(ParticleSystem particleSystem)
        {
            this.particleSystem = particleSystem;
        }
        public override void Initialize()
        {
            this.particleSystem.Stop();
        }

        public override void StartEffect()
        {
            base.StartEffect();
            particleSystem.Play();
            _coroutineHandle = Timing.RunCoroutine(WaitToEndEffectCorountine());
        }

        public override bool Pause()
        {
            if (!base.Pause()) return false;
            particleSystem.Pause();
            return true;
        }

        public override bool Continue()
        {
            if(!base.Continue()) return false;
            particleSystem.Play();
            return true;
        }

        IEnumerator<float> WaitToEndEffectCorountine()
        {
            while (particleSystem.isPlaying)
            {
                yield return Timing.WaitForOneFrame;
            }
            End();
        }

        public override void End()
        {
            base.End();
            particleSystem.Stop();
        }

        public override bool Cancel()
        {
            if (!base.Cancel()) return false;
            Timing.KillCoroutines(_coroutineHandle);
            particleSystem.Stop();
            return true;
        }
    }
}