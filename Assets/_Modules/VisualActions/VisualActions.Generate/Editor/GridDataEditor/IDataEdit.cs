namespace Mimi.VisualActions.Generate.Editor
{
    public interface IDataEdit
    {
        bool IsToggle { get; set; }
        void TurnOnHandle();
        void TurnOffHandle();
    }
}