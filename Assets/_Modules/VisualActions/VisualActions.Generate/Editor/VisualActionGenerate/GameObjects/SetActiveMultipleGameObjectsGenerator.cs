using System.Collections.Generic;
using Mimi.Reflection.Extensions;
using UnityEngine;
using VisualActions.VisualActions.GameObjects.Runtime;

namespace Mimi.VisualActions.Generate.Editor.GameObjects
{
    public class SetActiveMultipleGameObjectsGenerator : BaseGenerateObject<SetActiveMultipleGameObjectsAction>, IVisualActionGenerator
    {
        private List<GameObject> gameObjects;
        private bool isActive;
        public override string PrefabAddress => "/Prefabs/VisualActions/GameObjects/SetActiveMultiple.prefab";

        public SetActiveMultipleGameObjectsGenerator(List<GameObject> gameObjects, bool isActive)
        {
            this.gameObjects = gameObjects;
            this.isActive = isActive;
        }
        
        public VisualAction Generate()
        {
            var instance = GeneratePrefab();
            if (gameObjects != null)
            {
                instance.SetField("gameObjects", gameObjects.ToArray(), AccessModifier.Private);
            }
            instance.SetField("status", isActive, AccessModifier.Private);
            return instance;
        }
    }
}