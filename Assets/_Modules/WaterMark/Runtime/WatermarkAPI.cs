namespace Mimi.Watermarks
{
    public static class Watermark
    {
        private const string DefaultGroup = "INFO";

        public static void SetOption(string source, WatermarkOption option)
        {
            WatermarkManager.Instance.SetOption(source, option);
        }

        public static void Add(string message, string group = DefaultGroup)
        {
            WatermarkManager.Instance.Add(group, message);
        }

        public static void Clear(string group = null)
        {
            WatermarkManager.Instance.Clear(group);
        }
    }
}