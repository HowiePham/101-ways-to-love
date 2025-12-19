using System.Reflection;
using Mimi.VisualActions.Editor;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Scanning.Editor
{
    public static class VisualScanEditor
    {
        [MenuItem("GameObject/Visual Actions/Mechanics/Scanning/Scan Spine", false, -10000)]
        private static void CreateScan(MenuCommand menuCommand)
        {
            // var scan = VisualActionBuilder.CreateActionGameObject<VisualScan>("Scan", menuCommand);
            // GameObject hiddenSpineGo = VisualActionBuilder.CreateGameObject("HiddenSpine", menuCommand);
            // hiddenSpineGo.transform.SetParent(scan.transform);
            // hiddenSpineGo.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            // var hiddenSkeleton = hiddenSpineGo.AddComponent<SkeletonAnimation>();
            // hiddenSkeleton.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            // var boxArea = BoxArea.Create(menuCommand);
            // boxArea.gameObject.name = "ScanTarget";
            // boxArea.transform.SetParent(scan.transform);
            // boxArea.transform.localPosition = Vector3.zero;
            // scan.GetType().GetField("target", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(scan, boxArea);
        }
    }
}