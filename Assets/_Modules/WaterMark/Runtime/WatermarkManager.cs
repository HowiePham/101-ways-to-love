using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mimi.Watermarks
{
    internal class WatermarkManager : MonoBehaviour
    {
        private static WatermarkManager instance;

        public static WatermarkManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = new GameObject("WatermarkManager").AddComponent<WatermarkManager>();
                DontDestroyOnLoad(instance);
                return instance;
            }
        }

        private readonly Dictionary<string, List<string>> groups = new();
        private readonly Dictionary<string, WatermarkOption> options = new();

        private TextMeshProUGUI label;

        private void CreateUI()
        {
            var canvasGo = new GameObject("WatermarkCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            DontDestroyOnLoad(canvasGo);

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            var panel = new GameObject("Panel").AddComponent<RectTransform>();
            panel.SetParent(canvasGo.transform);
            panel.sizeDelta = new Vector2(700, 400);
            panel.anchorMin = new Vector2(0, 1);
            panel.anchorMax = new Vector2(0, 1);
            panel.pivot = new Vector2(0, 1);
            panel.anchoredPosition = new Vector2(20f, -10f);

            var content = new GameObject("Content").AddComponent<RectTransform>();
            content.SetParent(panel.transform);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 400f);

            this.label = content.gameObject.AddComponent<TextMeshProUGUI>();
            this.label.textWrappingMode = TextWrappingModes.Normal;
        }

        private void RefreshUI()
        {
            if (this.label == null)
            {
                CreateUI();
            }

            var sb = new StringBuilder();

            foreach (var group in this.groups)
            {
                if (!this.options.TryGetValue(group.Key, out WatermarkOption opt))
                    opt = WatermarkOption.Default;

                sb.AppendLine($"<b>[{group.Key}]</b>");
                foreach (string msg in group.Value)
                    sb.AppendLine($"• {msg}");
                sb.AppendLine();

                this.label.color = opt.Color;
                this.label.fontSize = opt.FontSize;
            }

            this.label.text = sb.ToString();
            this.label.ForceMeshUpdate();
        }

        internal void Add(string source, string message)
        {
            if (!this.groups.ContainsKey(source)) this.groups[source] = new List<string>();

            if (!this.options.ContainsKey(source)) this.options[source] = WatermarkOption.Default;

            this.groups[source].Add(message);

            RefreshUI();
        }

        internal void SetOption(string source, WatermarkOption opt)
        {
            this.options[source] = opt;
            RefreshUI();
        }

        internal void Clear(string source = null)
        {
            if (source == null)
                this.groups.Clear();
            else
                this.groups.Remove(source);

            RefreshUI();
        }
    }
}