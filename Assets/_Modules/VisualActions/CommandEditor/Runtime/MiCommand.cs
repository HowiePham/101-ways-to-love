using System;

namespace Mimi.CommandEditor
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MiCommand : Attribute
    {
        public string Path { get; }
        public string Description { get; set; }
        public string Author { get; set; } = "Unknown";

        public MiCommand(string path)
        {
            this.Path = path;
        }
    }
}