using System;
using System.Reflection;
using Lean.Common;
using Lean.Touch;
using Mimi.Reflection.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Mimi.Interactions.Dragging.LeanTouch.Editor
{
    public static class LeanTouchDragEditor
    {
        [MenuItem("GameObject/Interactions/Dragging/Lean Touch Drag Handler", false, -10000)]
        public static void CreateLeanTouchDragHandler()
        {
            NewLeanTouchDragHandler();
        }

        public static LeanTouchDragHandler NewLeanTouchDragHandler()
        {
            var dragHandlerGo = new GameObject("LeanTouchDragHandler");
            var dragHandler = dragHandlerGo.AddComponent<LeanTouchDragHandler>();
            var fingerDown = dragHandlerGo.GetComponent<LeanFingerDown>();
            var selectByFinger = dragHandlerGo.GetComponent<LeanSelectByFinger>();
            var raycastScreenQuery = new LeanScreenQuery(LeanScreenQuery.MethodType.Raycast);
            raycastScreenQuery.Search = LeanScreenQuery.SearchType.GetComponentInChildren;
            selectByFinger.ScreenQuery = raycastScreenQuery;
            selectByFinger.Limit = LeanSelect.LimitType.Unlimited;
            selectByFinger.Reselect = LeanSelect.ReselectType.Deselect;
            dragHandler.SetField("leanSelectByFinger", selectByFinger, AccessModifier.Private);
            var fixDistantScreenDepth = new LeanScreenDepth(LeanScreenDepth.ConversionType.FixedDistance);
            fixDistantScreenDepth.Distance = 30;
            fingerDown.ScreenDepth = fixDistantScreenDepth;

            // Add on finger event programmatically
            LeanFingerDown.LeanFingerEvent onFingerEvent = fingerDown.OnFinger;
            MethodInfo addPersistentListenerMethodInfo = typeof(UnityEventBase).GetMethod("AddPersistentListener",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(addPersistentListenerMethodInfo);
            addPersistentListenerMethodInfo.Invoke(onFingerEvent, null);

            MethodInfo registerPersistentListenerMethodInfo = typeof(UnityEventBase).GetMethod(
                "RegisterPersistentListener",
                BindingFlags.Instance | BindingFlags.NonPublic, null, CallingConventions.Standard,
                new[]
                {
                    typeof(int),
                    typeof(object),
                    typeof(MethodInfo)
                }, Array.Empty<ParameterModifier>());

            Assert.IsNotNull(registerPersistentListenerMethodInfo);
            MethodInfo selectScreenPositionMethodInfo = typeof(LeanSelectByFinger).GetMethod("SelectScreenPosition",
                new[]
                {
                    typeof(LeanFinger)
                });

            registerPersistentListenerMethodInfo.Invoke(onFingerEvent, new object[]
            {
                0,
                selectByFinger,
                selectScreenPositionMethodInfo
            });
            return dragHandler;
        }
    }
}