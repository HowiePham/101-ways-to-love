using System;
using UnityEngine;
using UnityEngine.UI;

namespace Games
{
    public class SpriteSwapToggle : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private GameObject offIcon;
        [SerializeField] private GameObject onIcon;

        private void OnEnable()
        {
            this.toggle.onValueChanged.AddListener(ValueChangeHandler);
        }

        private void OnDisable()
        {
            this.toggle.onValueChanged.RemoveListener(ValueChangeHandler);
        }

        public void ValueChangeHandler(bool value)
        {
            this.offIcon.SetActive(!value);
            this.onIcon.SetActive(value);
        }
    }
}