using System;
using UnityEngine;

namespace GameScenes
{
    public class LevelAutoPlay : MonoBehaviour
    {
        private LevelPlayer levelPlayer;

        private void Start()
        {
            this.levelPlayer = FindAnyObjectByType<LevelPlayer>();
            if (this.levelPlayer == null)
            {
                return;
            }

            this.levelPlayer.Play();
        }
    }
}