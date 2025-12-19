namespace Mimi.VisualActions.Data
{
    public interface Field<T>
    {
        public void SetValue(T value);
        public T GetValue();
    }
}