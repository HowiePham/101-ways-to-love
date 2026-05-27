using System.Collections.Generic;

namespace Mimi.Prototypes.SaveLoad
{
    public class PlayerLoader : ILoadStrategy<SaveRoot, BaseGameContext>
    {
        public void Load(int version, SaveRoot saveRoot, BaseGameContext context, bool firstLoad)
        {
            PlayerSave playerSaver = saveRoot.PlayerSave;

            if (playerSaver == null)
            {
                saveRoot.PlayerSave = new PlayerSave();
            }

            GameData gameData = context.GameData;
            // gameData.SettingModel.MusicOn = playerSaver.SettingModel.MusicOn;
            // gameData.SettingModel.SoundOn = playerSaver.SettingModel.SoundOn;
            // gameData.SettingModel.VibrationOn = playerSaver.SettingModel.VibrationOn;
            gameData.SettingModel = playerSaver.SettingModel;
            context.RuntimeState.CurrentLevelOrder.Set(playerSaver.CurrentLevel);

            // Migration: existing players may have TopLevel=1 (default) while CurrentLevel is higher
            int topLevel = playerSaver.TopLevel > playerSaver.CurrentLevel
                ? playerSaver.TopLevel
                : playerSaver.CurrentLevel;
            context.RuntimeState.TopLevelOrder.Set(topLevel);
            
            int topCompleteLevel = playerSaver.TopCompleteLevel > playerSaver.TopLevel
                ? playerSaver.TopCompleteLevel
                : playerSaver.TopLevel;
            context.RuntimeState.TopCompletedLevelOrder.Set(topCompleteLevel);

            gameData.AngelSkins = playerSaver.AngelSkins ?? new Dictionary<string, bool>();
            gameData.EquippedAngelSkins = playerSaver.EquippedAngelSkins ?? new Dictionary<string, string>();
        }
    }
}