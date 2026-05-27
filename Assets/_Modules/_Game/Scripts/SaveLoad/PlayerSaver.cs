namespace Mimi.Prototypes.SaveLoad
{
    public class PlayerSaver : ISaveStrategy<SaveRoot, BaseGameContext>
    {
        //save PlayerSaver
        public void Save(int version, SaveRoot saveRoot, BaseGameContext context)
        {
            PlayerSave playerSave = saveRoot.PlayerSave;
            //Sound
            playerSave.SettingModel.MusicOn = context.GameData.SettingModel.MusicOn;
            playerSave.SettingModel.SoundOn = context.GameData.SettingModel.SoundOn;
            playerSave.SettingModel.VibrationOn = context.GameData.SettingModel.VibrationOn;
            // //Level
            playerSave.CurrentLevel = context.RuntimeState.CurrentLevelOrder.Value;
            playerSave.TopLevel = context.RuntimeState.TopLevelOrder.Value;
            playerSave.TopCompleteLevel = context.RuntimeState.TopCompletedLevelOrder.Value;
            playerSave.AngelSkins = context.GameData.AngelSkins;
            playerSave.EquippedAngelSkins = context.GameData.EquippedAngelSkins;
        }
    }
}