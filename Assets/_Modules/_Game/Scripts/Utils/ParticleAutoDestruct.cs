using Mimi.Prototypes.Pooling;
using Mimi.ServiceLocators;
using Sirenix.OdinInspector;
using UnityEngine;

public class ParticleAutoDestruct : MonoBehaviour
{
    [SerializeField] private float _duration;

    [SerializeField] private bool _unscaledTime;

    private float _counter = 0f;

    private void OnDisable()
    {
        this._counter = 0f;
    }

    private void Update()
    {
        this._counter += this._unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (this._counter >= this._duration)
        {
            this._counter = 0f;
            ServiceLocator.Global.Get<IPoolService>().Despawn(gameObject);
        }
    }

    [Button("Calculate")]
    private void CalculateDuration()
    {
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();

        this._duration = particleSystems[0].main.duration;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i].main.duration > this._duration)
            {
                this._duration = particleSystems[i].main.duration;
            }
        }
    }
}