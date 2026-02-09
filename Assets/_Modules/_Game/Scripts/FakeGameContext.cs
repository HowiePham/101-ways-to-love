using Mimi.Prototypes;
using Mimi.ServiceLocators;
using Mimi.Services.ScriptableObject.Audio;
using UnityEngine;

public class FakeGameContext : MonoBehaviour
{
    [SerializeField] private BaseAudioServiceSO audioService;
    public IAudioService AudioService { private set; get; }

    private void Awake()
    {
        CreateAudioService();
    }

    private void CreateAudioService()
    {
        this.AudioService = new AudioServiceAdapter(this.audioService);
        ServiceLocator.Global.Register(this.AudioService);
    }
}