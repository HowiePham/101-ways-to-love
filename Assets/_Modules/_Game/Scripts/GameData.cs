using System.Collections.Generic;

public class GameData
{
    public SettingModel SettingModel { get; set; } = new SettingModel();
    public bool IsAdCoolDowning = false;
    public bool IsAdCoolDownCompletedAfterReward = true;

    public Dictionary<string, bool> AngelSkins = new Dictionary<string, bool>();
    public Dictionary<string, string> EquippedAngelSkins = new Dictionary<string, string>();
}