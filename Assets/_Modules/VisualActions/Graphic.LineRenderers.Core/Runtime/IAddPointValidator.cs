using UnityEngine;

namespace Mimi.Interactions.Drawing
{
    public interface IAddPointValidator
    {
        bool IsValid(Vector2 point);
    }

    public class NullAddPointValidator : IAddPointValidator
    {
        public static readonly NullAddPointValidator Instance = new();

        private NullAddPointValidator()
        {
        }

        public bool IsValid(Vector2 point)
        {
            return false;
        }
    }
}