#if UNITY_EDITOR
using Mimi.CommandEditor;
using UnityEditor;
using UnityEngine;

namespace VisualActions.CommandEditor.Sample
{
    public static class VisualCommands
    {
        [MiCommand("Example/Hello World",Description = "Say Hello World",Author = "Hoang Vu")]
        public static void SayHello() => Debug.Log("Hello from Visual!");

        [MiCommand("Example/SubFolder1/Create Cube", Description = "Create simple Cube")]
        public static void SpawnCube() => GameObject.CreatePrimitive(PrimitiveType.Cube);
    }
}
#endif