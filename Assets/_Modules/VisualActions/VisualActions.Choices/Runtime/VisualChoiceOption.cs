using System;
using Mimi.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace Mimi.VisualActions.Choices
{
    public class VisualChoiceOption : MonoBehaviour, IChoiceOption
    {
        [SerializeField] private bool isCorrect;
        [SerializeField] private VisualAction action;
        [SerializeField] private Button button;

        public bool IsCorrect => this.isCorrect;
        public Transform Transform => this.trans;
        public Vector3 StartingPosition { private set; get; }
        public IAsyncAction Action => this.action;

        public event Action<IChoiceOption> OnSelected;

        private Transform trans;

        private void Awake()
        {
            this.trans = transform;
            StartingPosition = this.trans.position;
        }

        private void OnEnable()
        {
            this.button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            this.button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            OnSelected?.Invoke(this);
        }
    }
}