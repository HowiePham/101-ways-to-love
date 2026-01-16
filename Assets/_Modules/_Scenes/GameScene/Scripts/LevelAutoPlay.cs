using System;
using UnityEngine;

namespace GameScenes
{
    public class LevelAutoPlay : MonoBehaviour
    {
        private LevelPlayer levelPlayer;
        [SerializeField] private GameObject winCamera;

        private void Start()
        {
            this.levelPlayer = FindAnyObjectByType<LevelPlayer>();
            if (this.levelPlayer == null)
            {
                return;
            }

            this.winCamera.SetActive(false);
            this.levelPlayer.Play();
        }
    }
}