namespace Mimi.Prototypes.LevelManagement
{
    public interface ILevelOrder
    {
        LevelInfo GetByOrder(int order);
        LevelInfo First();
        LevelInfo Last();
        bool IsFirst(string id);
        bool IsLast(string id);
        LevelInfo GetNextLevel(string id);
        LevelInfo GetPreviousLevel(string id);
        LevelInfo GetNextLevel(int currentOrder);
        LevelInfo GetPreviousLevel(int currentOrder);
    }
}