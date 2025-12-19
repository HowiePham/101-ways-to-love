using Spine.Unity;
using UnityEngine;

namespace Mimi.VisualActions.Spines
{
    [RequireComponent(typeof(MonoSpineMixSkin))]
    public class SpineMixSkinIncrement : MonoBehaviour
    {
        [SerializeField] private MonoSpineMixSkin mixSkin;
        [SerializeField, SpineSkin] private string[] skins;

        private int currentIndex = -1;

        public void NextSkin()
        {
            if (this.currentIndex >= this.skins.Length)
            {
                Debug.LogError($"Out of range: There are {this.skins.Length} skins but index is {this.currentIndex}");
                return;
            }

            this.currentIndex++;
            this.mixSkin.SetSlot(this.skins[this.currentIndex]);
        }
    }
}