using Mimi.CommandEditor;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Generate.Editor;
using Mimi.VisualActions.Generate.Editor.TransformActions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Mimi.VisualActions.Generate
{
    public static class VisualActionsTemplateCommand
    {
        #if UNITY_EDITOR
        [MiCommand("VisualActions/Areas/BoxArea")]
        public static void BoxArea()
        {
            new BoxArea2DGenerator().Generate();
        }

        [MiCommand("VisualActions/Mechanic/BookFlip")]
        public static void BookFlipAction()
        {
            new BookFlipGenerator(null).Generate();
        }

        [MiCommand("VisualActions/Commands/MoveDraggableFromTo")]
        public static void MoveDraggableFromTo()
        {
            new MoveDraggableFromToGenerator().Generate();
        }

        [MiCommand("VisualActions/Tools/ItemDrag")]
        public static void ItemDrag()
        {
            new ItemDraggableGenerator().Generate();
        }

        [MiCommand("VisualActions/Editor/Mechanic/VisualDrag")]
        public static void VisualDrag()
        {
            var instance = new SequenceGenerator();
            instance.AddGenerator(new VisualDragGenerator(new CompositeDragExtensionGenerator(),
                new CompleteAllConditionGenerator()));
            var gameObjectInstance = instance.Generate();
            var draggable = new DraggableGenerator().Generate();
            draggable.transform.SetParent(gameObjectInstance.transform);

            var visualToolEditor = gameObjectInstance.gameObject.AddComponent<VisualToolEditor>();
            ComponentUtility.MoveComponentUp(visualToolEditor);
            visualToolEditor.SetDraggable(draggable);
            var visualDragEditor = gameObjectInstance.gameObject.AddComponent<VisualDragEditor>();
            ComponentUtility.MoveComponentUp(visualDragEditor);
            ComponentUtility.MoveComponentUp(visualDragEditor);
            visualDragEditor.Refresh();
            
            var box = new BoxArea2DGenerator().Generate();
            box.transform.SetParent(gameObjectInstance.transform);
            
            gameObjectInstance.gameObject.name = "VisualDrag";
        }

        [MiCommand("VisualActions/Editor/Mechanic/VisualSmoothDelete")]
        public static void VisualSmoothDelete()
        {
            var instance = new SequenceGenerator();
            instance.AddGenerator(new VisualSmoothDeleteGenerator(new CompositeSmoothDeleteExtensionGenerator(),
                null));
            var gameObjectInstance = instance.Generate();
            var draggable = new DraggableGenerator().Generate();
            draggable.transform.SetParent(gameObjectInstance.transform);

            var visualToolEditor = gameObjectInstance.gameObject.AddComponent<VisualToolEditor>();
            ComponentUtility.MoveComponentUp(visualToolEditor);
            visualToolEditor.SetDraggable(draggable);
            var visualSmoothDeleteEditor = gameObjectInstance.gameObject.AddComponent<VisualSmoothDeleteEditor>();
            ComponentUtility.MoveComponentUp(visualSmoothDeleteEditor);
            ComponentUtility.MoveComponentUp(visualSmoothDeleteEditor);
            visualSmoothDeleteEditor.Refresh();
            
            gameObjectInstance.gameObject.name = "VisualSmoothDelete";
        }

        [MiCommand("VisualActions/Editor/VisualTool")]
        public static void VisualToolEditor()
        {
            GeneratePrefab(ConfigGenerateSO.Instance.rootPath + "/Prefabs/Draggable/VisualTool.prefab");
        }

        #region Generate

        [MiCommand("VisualActions/Generate/VisualSmoothDelete")]
        public static void VisualSmoothDeleteGenerator()
        {
            var generateInstance = new GameObject("SmoothDeleteGenerator");
            var dragComponent = generateInstance.AddComponent<SmoothDeleteGenerator>();
        }

        [MiCommand("VisualActions/Generate/DragToolToMultiple")]
        public static void DragToolToMultipleGameObjectsGenerator()
        {
            var generateInstance = new GameObject("DragToolToMultipleGameObjectsGenerator");
            var dragComponent = generateInstance.AddComponent<DragToolToMultipleObjectsGenerator>();
        }

        [MiCommand("VisualActions/Generate/DragItemsOutside")]
        public static void DragMultipleItemsOutside()
        {
            var generateInstance = new GameObject("DragToolToMultipleItemsOutsideGenerator");
            var dragComponent = generateInstance.AddComponent<DragPickUpOtherItems>();
        }

        [MiCommand("VisualActions/Generate/DragItemsSwitch")]
        public static void DragMultipleItemsSwitch()
        {
            var generateInstance = new GameObject("DragToolToMultipleItemSwitchGenerator");
            generateInstance.AddComponent<DragItemMultipleSwitchObjectsGenerator>();
            generateInstance.AddComponent<GenerateDraggableItem>();
        }

        [MiCommand("VisualActions/Generate/DragSwitchObject")]
        public static void DragSwitchObject()
        {
            var generateInstance = new GameObject("DragSwitchObjectGenerator");
            var boxArea = new BoxArea2DGenerator().Generate();
            boxArea.transform.SetParent(generateInstance.transform);
            
            var dragComponent = generateInstance.AddComponent<DragSwitchObjectGenerator>();
            dragComponent.SetField("boxArea", boxArea, AccessModifier.Private);
        }
        #endregion
        
        #region Effect
        
        [MiCommand("VisualActions/Effect/AppearEffect (Graphic)")]
        public static void AppearEffectGraphic()
        {
            GeneratePrefab(ConfigGenerateSO.Instance.rootPath + "/Prefabs/VisualActions/Effects/AppearEffect(Graphic).prefab");
        }
        
        [MiCommand("VisualActions/Effect/AppearEffect (Sprite)")]
        public static void AppearEffectSprite()
        {
            GeneratePrefab(ConfigGenerateSO.Instance.rootPath + "/Prefabs/VisualActions/Effects/AppearEffect(Sprite).prefab");
        }

        [MiCommand("VisualActions/Effect/DisAppearEffect (Graphic)")]
        public static void DisappearEffectGraphic()
        {
            GeneratePrefab(ConfigGenerateSO.Instance.rootPath + "/Prefabs/VisualActions/Effects/DisappearEffect(Graphic).prefab");
        }
        
        [MiCommand("VisualActions/Effect/DropEffect (Graphic)")]
        public static void DropEffect()
        {
            GeneratePrefab(ConfigGenerateSO.Instance.rootPath + "/Prefabs/VisualActions/Effects/Drop.prefab");
        }
        #endregion

        #region GameFeel

        [MiCommand("VisualActions/Editor/GameFeel/Vibration")]
        public static void Vibration()
        {
            var instance = new VibrationActionGenerator().Generate();
            EditorGUIUtility.PingObject(instance);
        }

        [MiCommand("VisualActions/Editor/GameFeel/PlayAudio")]
        public static void PlayAudio()
        {
            var instance = new PlayAudioGenerator("").Generate();
            EditorGUIUtility.PingObject(instance);
        }
        #endregion
       
        public static GameObject GeneratePrefab(string prefabPath)
        {
            var prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    prefabPath);
            var instance = PrefabUtility.InstantiatePrefab(prefab);
            EditorGUIUtility.PingObject(instance);
            return instance as GameObject;
        }
        
        
        #endif
    }
}