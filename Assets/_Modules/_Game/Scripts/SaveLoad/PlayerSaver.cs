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
            // playerSave.LastCompleteLevel = context.GameData.LastCompletedLevelOrder;
            // playerSave.TopLevel = context.GameData.LevelTop;
            // //Games
            // playerSave.Coin = context.CurrencyRepository.GetCurrency(CurrencyType.Coin).TotalAmount;
            // playerSave.Rated = context.GameData.Rated;
            // playerSave.IsReceiveReward = context.GameData.IsReceiveReward;
            // //shop
            // playerSave.lstErase = context.GameData.gamedatalistEraseCurrent;
            // playerSave.currentErase = context.GameData.CurrentErase;
        }
    }
}