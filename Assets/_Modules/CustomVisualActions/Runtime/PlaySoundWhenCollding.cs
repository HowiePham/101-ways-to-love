using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using UnityEngine;

public class PlaySoundWhenCollding : MonoBehaviour
{
    [SerializeField] private string layerName;
    [SerializeField] private BaseAudioServiceSO audioService;
    [SerializeField, SoundKey] private string soundKey;

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (collision2D.gameObject.layer == LayerMask.NameToLayer(this.layerName))
        {
            if (!this.enabled)
            {
                return;
            }

            this.audioService.PlaySound(this.soundKey);
        }
    }
}