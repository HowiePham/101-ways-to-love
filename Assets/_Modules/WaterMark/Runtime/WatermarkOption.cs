using System;
using UnityEngine;

namespace Mimi.Watermarks
{
    [Serializable]
    public struct WatermarkOption
    {
        public Color Color;
        public int FontSize;

        public static WatermarkOption Default =>
            new WatermarkOption
            {
                Color = Color.green,
                FontSize = 42,
            };
    }
}