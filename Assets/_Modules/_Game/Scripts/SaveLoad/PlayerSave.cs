using System.Collections.Generic;

namespace Mimi.Prototypes.SaveLoad
{
    public class PlayerSave
    {
        public SettingModel SettingModel = new SettingModel();
        // public float Sound = 100;
        // public float Music = 100;

        public int CurrentLevel = 0;
        public int TopCompleteLevel;
        public int TopLevel = 0;

        public int CurrentLevelChallenge = 0;
        public int LastCompleteLevelChallenge = 0;
        public int TopLevelChallenge = 0;

        public List<int> lstBonusUnlock = new List<int>();
        public List<int> lstBonusComplete = new List<int>();

        public int Coin = 0;
        public bool Rated;
        public bool IsReceiveReward = false;

        public List<int> lstErase = new List<int>();
        public int currentErase;

        public Dictionary<string, bool> AngelSkins = new Dictionary<string, bool>();
        public Dictionary<string, string> EquippedAngelSkins = new Dictionary<string, string>();
    }
}