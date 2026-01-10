namespace Mimi.Prototypes.SaveLoad
{
    public class GameLoader : BaseGameLoader<SaveRoot, BaseGameContext>
    {
        public GameLoader(BaseGameContext context) : base(context)
        {
            AddLoadStrategy(new PlayerLoader());
        }

        public override int SaveFileVersion => 0;

        public override bool LoadFromRaw(string rawData, out ISaveRoot outSaveRoot, bool useLoadStrategy = true)
        {
            outSaveRoot = null;
            return false;
        }
    }
}