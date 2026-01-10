using BayatGames.SaveGamePro;

namespace Mimi.Prototypes.SaveLoad
{
    public class GameSaver : BaseGameSaver<SaveRoot, BaseGameContext>
    {
        public GameSaver(BaseGameContext context) : base(context)
        {
            AddSaveStrategy(new PlayerSaver());
        }

        public override int SaveFileVersion { get; }
        public override SaveGameSettings SaveGameSettings { get; }
    }
}